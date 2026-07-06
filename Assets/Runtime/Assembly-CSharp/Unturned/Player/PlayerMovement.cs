////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define WITH_MOVEMENT_GIZMOS
// #define LOG_FOOTSTEP_AUDIO
// #define WITH_FLOOR_SNAPPING_GIZMOS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.Framework.Devkit;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void Landed(float velocity);
	public delegate void Seated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle);
	public delegate void VehicleUpdated(bool isDriveable, ushort newFuel, ushort maxFuel, float newSpeed, float minSpeed, float maxSpeed, ushort newHealth, ushort maxHealth, ushort newBatteryCharge);
	public delegate void SafetyUpdated(bool isSafe);
	public delegate void RadiationUpdated(bool isRadiated);
	public delegate void PurchaseUpdated(HordePurchaseVolume newNode);

	public struct PlayerStateUpdate
	{
		public Vector3 pos;
		public byte angle;
		public byte rot;

		public PlayerStateUpdate(Vector3 pos, byte angle, byte rot)
		{
			this.pos = pos;
			this.angle = angle;
			this.rot = rot;
		}
	}

	public enum EPlayerHeight
	{
		STAND,
		CROUCH,
		PRONE
	}

	public class PlayerMovement : PlayerCaller
	{
		public static readonly float HEIGHT_STAND = 2;
		public static readonly float HEIGHT_CROUCH = 1.2f;
		public static readonly float HEIGHT_PRONE = 0.8f;

		/// <summary>
		/// Nelson 2024-10-18: Moved to a constant because clients need this value for footsteps and they don't have the
		/// character controller component.
		/// </summary>
		internal const float SKIN_WIDTH = 0.1f;

		public static bool forceTrustClient
		{
			get => GameplayConfigData._forceTrustClient;
			set
			{
				GameplayConfigData._forceTrustClient.value = value;

				UnturnedLog.info("Set ForceTrustClient to: " + forceTrustClient);
			}
		}

		public Landed onLanded;
		public Seated onSeated;
		public VehicleUpdated onVehicleUpdated;
		public SafetyUpdated onSafetyUpdated;
		public RadiationUpdated onRadiationUpdated;
		public PurchaseUpdated onPurchaseUpdated;

		public PlayerRegionUpdated onRegionUpdated;
		public PlayerBoundUpdated onBoundUpdated;

		public event PlayerNavChanged PlayerNavChanged;
		private void TriggerPlayerNavChanged(byte oldNav, byte newNav)
		{
			if (PlayerNavChanged == null)
			{
				return;
			}

			PlayerNavChanged(this, oldNav, newNav);
		}

		private static readonly float SPEED_CLIMB = 4.5f;
		private static readonly float SPEED_SWIM = 3f;
		private static readonly float SPEED_SPRINT = 7f;
		private static readonly float SPEED_STAND = 4.5f;
		private static readonly float SPEED_CROUCH = 2.5f;
		private static readonly float SPEED_PRONE = 1.5f;

		/// <summary>
		/// Jump speed = sqrt(2 * jump height * gravity)
		/// Jump height = (jump speed ^ 2) / (2 * gravity)
		/// With 7 speed and 9.81 * 3 gravity = apex height of 1.66496772
		/// </summary>
		private static readonly float JUMP = 7.0f;
		private static readonly float SWIM = 3;

		/// <summary>
		/// Note: Only UpdateCharacterControllerEnabled should modify whether controller is enabled.
		/// (turning off and back on is fine though)
		/// </summary>
		public CharacterController controller
		{
			get;
			protected set;
		}

		[System.Obsolete("Was current value of interpolated aiming speed multiplier.")]
		public float _multiplier;
		[System.Obsolete("Was target value of interpolated aiming speed multiplier.")]
		public float multiplier;
		public float itemGravityMultiplier;
		public float pluginGravityMultiplier;
		public float pluginSpeedMultiplier;
		public float pluginJumpMultiplier = 1.0f;

		public float totalGravityMultiplier => itemGravityMultiplier * pluginGravityMultiplier;

		public float totalSpeedMultiplier => pluginSpeedMultiplier * player.clothing.movementSpeedMultiplier
					* (player.equipment.asset?.equipableMovementSpeedMultiplier ?? 1.0f)
					* (player.equipment.useable?.movementSpeedMultiplier ?? 1.0f);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public bool enableFly;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		private float lastFootstep;

		[System.Obsolete]
		public SDG.Framework.Landscapes.LandscapeHoleVolume landscapeHoleVolume => null;

		internal bool CanEnterTeleporter => player.life.IsAlive && getVehicle() == null;

		private void DoTeleport(Transform inputTransform, Transform outputTransform)
		{
			Vector3 relativePosition = inputTransform.InverseTransformPoint(transform.position);
			Quaternion relativeRotation = inputTransform.InverseTransformRotation(transform.rotation);
			transform.position = outputTransform.TransformPoint(relativePosition);
			Quaternion outputRotation = outputTransform.TransformRotation(relativeRotation);
			float yaw = outputRotation.eulerAngles.y;
			player.look.TeleportYaw(yaw);

			// Adjusting last update pos is particularly important for first-person view interpolation.
			Vector3 relativeLastUpdatePos = inputTransform.InverseTransformPoint(lastUpdatePos);
			lastUpdatePos = outputTransform.TransformPoint(relativeLastUpdatePos);

			player.PostTeleport();
		}

		internal void EnterCollisionTeleporter(CollisionTeleporter teleporter)
		{
			Transform inputTransform = teleporter.transform;
			Transform outputTransform = teleporter.DestinationTransform;
			DoTeleport(inputTransform, outputTransform);
		}

		internal void EnterTeleporterVolume(TeleporterEntranceVolume entrance, TeleporterExitVolume exit)
		{
			Transform inputTransform = entrance.transform;
			Transform outputTransform = exit.transform;
			DoTeleport(inputTransform, outputTransform);
		}

		internal void UpdateCharacterControllerEnabled()
		{
			if (controller != null)
			{
				controller.enabled = vehicle == null && player.life.IsAlive;
			}
		}

		private bool _isGrounded;
		public bool isGrounded => _isGrounded;

		private bool _isSafe;
		public bool isSafe
		{
			get => _isSafe;

			set => _isSafe = value;
		}

		public SafezoneNode isSafeInfo;

		private bool _isRadiated;
		public bool isRadiated
		{
			get => _isRadiated;

			set => _isRadiated = value;
		}

		/// <summary>
		/// Valid while isRadiated.
		/// </summary>
		public IDeadzoneNode ActiveDeadzone { get; private set; }

		private HordePurchaseVolume _purchaseNode;
		public HordePurchaseVolume purchaseNode
		{
			get => _purchaseNode;

			set => _purchaseNode = value;
		}

		public IAmbianceNode effectNode
		{
			get;
			private set;
		}

		[System.Obsolete]
		public bool inRain;

		public bool inSnow;

		/// <summary>
		/// Set according to volume or level global asset fallback.
		/// </summary>
		public uint WeatherMask
		{
			get;
			protected set;
		}

		private string materialName;

		// Plays swimming audio if true. Placeholder-ish if we want to support custom liquids in the future.
		private bool materialIsWater;

		public RaycastHit ground;

		internal EPlayerHeight height = EPlayerHeight.STAND;
		public void setSize(EPlayerHeight newHeight)
		{
			if (newHeight == height)
			{
				return;
			}
			height = newHeight;

			applySize();
		}

		private void applySize()
		{
			float size;
			switch (height)
			{
				case EPlayerHeight.STAND:
					size = HEIGHT_STAND;
					break;
				case EPlayerHeight.CROUCH:
					size = HEIGHT_CROUCH;
					break;
				case EPlayerHeight.PRONE:
					size = HEIGHT_PRONE;
					break;
				default:
					size = 2.0f;
					break;
			}

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				if (controller != null)
				{
					// Nelson 2024-09-23: If prone on a surface slightly above a death barrier (e.g., on the staircase
					// of the air traffic control tower) and standing up the capsule could hit the trigger and kill the
					// player. (private issue #2551) At first I wondered if we could work around this by changing the
					// order of setting height/center, but sadly that doesn't seem to be the case. Fortunately,
					// disabling the collider and re-enabling after changing the size seems to work.
					bool wasControllerEnabled = controller.enabled;
					controller.enabled = false;
					controller.height = size;
					controller.center = new Vector3(0, size * 0.5f, 0);
					controller.enabled = wasControllerEnabled;
				}
			}
		}

		private bool _isMoving;
		public bool isMoving => _isMoving;

		public float speed
		{
			get
			{
				if (player.stance.stance == EPlayerStance.SWIM)
				{
					return SPEED_SWIM * (1f + (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.DIVING) * 0.25f)) * totalSpeedMultiplier;
				}
				else
				{
					float exercise = 1f + (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.EXERCISE) * 0.25f);

					if (player.stance.stance == EPlayerStance.CLIMB)
					{
						return SPEED_CLIMB * exercise * totalSpeedMultiplier;
					}
					else if (player.stance.stance == EPlayerStance.SPRINT)
					{
						return SPEED_SPRINT * exercise * totalSpeedMultiplier;
					}
					else if (player.stance.stance == EPlayerStance.STAND)
					{
						return SPEED_STAND * exercise * totalSpeedMultiplier;
					}
					else if (player.stance.stance == EPlayerStance.CROUCH)
					{
						return SPEED_CROUCH * exercise * totalSpeedMultiplier;
					}
					else if (player.stance.stance == EPlayerStance.PRONE)
					{
						return SPEED_PRONE * exercise * totalSpeedMultiplier;
					}
				}

				return 0;
			}
		}

		private Vector3 _move;
		public Vector3 move => _move;

		private byte _region_x;
		public byte region_x => _region_x;

		private byte _region_y;
		public byte region_y => _region_y;

		private byte _bound;
		public byte bound => _bound;

		private byte _nav;
		public byte nav => _nav;

		private byte updateRegionOld_X;
		private byte updateRegionOld_Y;
		private byte updateRegionNew_X;
		private byte updateRegionNew_Y;
		private byte updateRegionIndex;

		private LoadedRegion[,] _loadedRegions;
		public LoadedRegion[,] loadedRegions => _loadedRegions;

		private LoadedBound[] _loadedBounds;
		public LoadedBound[] loadedBounds => _loadedBounds;

		internal Vector3 velocity;
		public float fall => velocity.y;

		public Vector3 pendingLaunchVelocity;

		[System.Obsolete]
		public Vector3 real => transform.position;

		private Vector3 lastUpdatePos;
		public PitchYawSnapshotInfo snapshot;
		private NetworkSnapshotBuffer<PitchYawSnapshotInfo> nsb;

		private byte _horizontal = 1;
		public byte horizontal => _horizontal;

		private byte _vertical = 1;
		public byte vertical => _vertical;

		private int warp_x;
		private int warp_y;
		internal int input_x;
		internal int input_y;

		private bool _jump;
		public bool jump => _jump;

		/// <summary>
		/// Was set to true during teleport, and restored to false during the next movement tick.
		/// 
		/// Server pauses movement when this is set until next client update that matches,
		/// in order to prevent rubberbanding following a teleport.
		/// </summary>
		[System.Obsolete]
		public bool isAllowed;

		[System.Obsolete]
		public bool isUpdated;

		public List<PlayerStateUpdate> updates;
		public bool canAddSimulationResultsToUpdates;
		/// <summary>
		/// Used instead of actual position to avoid revealing admins in "vanish" mode.
		/// </summary>
		internal PlayerStateUpdate mostRecentlyAddedUpdate;
		internal bool hasMostRecentlyAddedUpdate;

		/// <summary>
		/// Flag for plugins to allow maintenance access underneath the map.
		/// </summary>
		public bool bypassUndergroundWhitelist = false;

		internal bool hasPendingVehicleChange;
		internal InteractableVehicle pendingVehicle;
		private byte pendingSeatIndex;
		private Transform pendingSeatTransform;
		private Vector3 pendingSeatPosition;
		private byte pendingSeatAngle;

		private Vector3 lastStatPos;
		private float lastStatTime;

		private InteractableVehicle vehicle;
		private byte seat;

		public InteractableVehicle getVehicle()
		{
			return vehicle;
		}

		/// <summary>
		/// Get seat (if any), otherwise null.
		/// </summary>
		public Passenger getVehicleSeat()
		{
			return vehicle != null && vehicle.passengers != null && seat < vehicle.passengers.Length ? vehicle.passengers[seat] : null;
		}

		public byte getSeat()
		{
			return seat;
		}

		/// <summary>
		/// Get ragdoll effect to use when running enemies over with the current vehicle.
		/// </summary>
		public ERagdollEffect GetVehicleRagdollEffect()
		{
			// Player can temporarily disable ragdoll effects along with their cosmetic/skin effects for stealth.
			if (vehicle != null && player.clothing.isMythic)
			{
				if (channel.owner.GetVehicleSkinItemDefId(vehicle, out int itemdefid))
				{
					if (channel.owner.TryGetRagdollEffectForItemDef(itemdefid, out ERagdollEffect effect))
					{
						return effect;
					}

					SkinAsset skinAsset = Assets.find(EAssetType.SKIN, Provider.provider.economyService.getInventorySkinID(itemdefid)) as SkinAsset;
					if (skinAsset != null)
					{
						return skinAsset.ragdollEffect;
					}
				}
			}

			return ERagdollEffect.None;
		}

		internal void ApplyPendingVehicleChange()
		{
			hasPendingVehicleChange = false;

			InteractableVehicle oldVehicle = vehicle;
			vehicle = pendingVehicle;
			seat = pendingSeatIndex;

			bool isDriver = vehicle != null && seat == 0;

			if (vehicle == null) // remove
			{
				player.transform.parent = pendingSeatTransform;
				player.ReceiveTeleport(pendingSeatPosition, pendingSeatAngle);
			}

			if (channel.IsLocalPlayer)
			{
				if (isDriver && Level.info != null && Level.info.name.ToLower() != "tutorial")
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Wheel", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Wheel");
					}
				}

				if (vehicle != null)
				{
					PlayerUI.disableDot();

					if (player.equipment.useable is UseableGun gun)
					{
						gun.UpdateCrosshairEnabled();
					}
				}
				else
				{
					if (player.equipment.useable is UseableGun gun)
					{
						gun.UpdateCrosshairEnabled();
					}
					else
					{
						PlayerUI.enableDot();
					}
				}
			}

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				UpdateCharacterControllerEnabled();

				if (vehicle != null)
				{
					if (isDriver)
					{
						player.stance.checkStance(EPlayerStance.DRIVING);
					}
					else
					{
						player.stance.checkStance(EPlayerStance.SITTING);
					}
				}
				else
				{
					player.stance.checkStance(EPlayerStance.STAND);
				}
			}

			if (channel.IsLocalPlayer)
			{
				onSeated?.Invoke(isDriver, vehicle != null, oldVehicle != null, oldVehicle, vehicle);

				if (isDriver)
				{
					if (onVehicleUpdated != null)
					{
						ushort displayCurrentFuel;
						ushort displayMaxFuel;
						vehicle.getDisplayFuel(out displayCurrentFuel, out displayMaxFuel);

						onVehicleUpdated(!vehicle.isUnderwater && !vehicle.isDead, displayCurrentFuel, displayMaxFuel, vehicle.AnimatedForwardVelocity, vehicle.asset.TargetReverseVelocity, vehicle.asset.TargetForwardVelocity, vehicle.health, vehicle.asset.health, vehicle.batteryCharge);
					}
				}

				if (vehicle != null)
				{
					if (isDriver)
					{
						if (oldVehicle == null)
						{
							PlayerUI.message(EPlayerMessage.VEHICLE_EXIT, "");
						}
						else
						{
							PlayerUI.message(EPlayerMessage.VEHICLE_SWAP, "");
						}
					}
					else
					{
						PlayerUI.message(EPlayerMessage.VEHICLE_SWAP, "");
					}
				}
			}

			if (vehicle != null)
			{
				player.transform.parent = pendingSeatTransform;
				player.transform.localPosition = pendingSeatPosition;
				player.transform.localRotation = Quaternion.identity;

				player.look.updateLook();
			}
		}

		public void setVehicle(InteractableVehicle newVehicle, byte newSeat, Transform newSeatingTransform, Vector3 newSeatingPosition, byte newSeatingAngle, bool forceUpdate)
		{
			hasPendingVehicleChange = true;
			pendingVehicle = newVehicle;
			pendingSeatIndex = newSeat;
			pendingSeatTransform = newSeatingTransform;
			pendingSeatPosition = newSeatingPosition;
			pendingSeatAngle = newSeatingAngle;

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				if (player.life.IsAlive && !forceUpdate)
				{
					return;
				}
			}

			ApplyPendingVehicleChange();
		}

		[System.Obsolete]
		public void tellPluginGravityMultiplier(CSteamID steamID, float newPluginGravityMultiplier)
		{
			ReceivePluginGravityMultiplier(newPluginGravityMultiplier);
		}

		private static readonly ClientInstanceMethod<float> SendPluginGravityMultiplier = ClientInstanceMethod<float>.Get(typeof(PlayerMovement), nameof(ReceivePluginGravityMultiplier));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellPluginGravityMultiplier))]
		public void ReceivePluginGravityMultiplier(float newPluginGravityMultiplier)
		{
			pluginGravityMultiplier = newPluginGravityMultiplier;
		}

		public void sendPluginGravityMultiplier(float newPluginGravityMultiplier)
		{
			pluginGravityMultiplier = newPluginGravityMultiplier;

			if (!channel.IsLocalPlayer)
			{
				SendPluginGravityMultiplier.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), newPluginGravityMultiplier);
			}
		}

		[System.Obsolete]
		public void tellPluginJumpMultiplier(CSteamID steamID, float newPluginJumpMultiplier)
		{
			ReceivePluginJumpMultiplier(newPluginJumpMultiplier);
		}

		private static readonly ClientInstanceMethod<float> SendPluginJumpMultiplier = ClientInstanceMethod<float>.Get(typeof(PlayerMovement), nameof(ReceivePluginJumpMultiplier));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellPluginJumpMultiplier))]
		public void ReceivePluginJumpMultiplier(float newPluginJumpMultiplier)
		{
			pluginJumpMultiplier = newPluginJumpMultiplier;
		}

		public void sendPluginJumpMultiplier(float newPluginJumpMultiplier)
		{
			pluginJumpMultiplier = newPluginJumpMultiplier;

			if (!channel.IsLocalPlayer)
			{
				SendPluginJumpMultiplier.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), newPluginJumpMultiplier);
			}
		}

		[System.Obsolete]
		public void tellPluginSpeedMultiplier(CSteamID steamID, float newPluginSpeedMultiplier)
		{
			ReceivePluginSpeedMultiplier(newPluginSpeedMultiplier);
		}

		private static readonly ClientInstanceMethod<float> SendPluginSpeedMultiplier = ClientInstanceMethod<float>.Get(typeof(PlayerMovement), nameof(ReceivePluginSpeedMultiplier));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellPluginSpeedMultiplier))]
		public void ReceivePluginSpeedMultiplier(float newPluginSpeedMultiplier)
		{
			pluginSpeedMultiplier = newPluginSpeedMultiplier;
		}

		public void sendPluginSpeedMultiplier(float newPluginSpeedMultiplier)
		{
			pluginSpeedMultiplier = newPluginSpeedMultiplier;

			if (!channel.IsLocalPlayer)
			{
				SendPluginSpeedMultiplier.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), newPluginSpeedMultiplier);
			}
		}

		public void tellState(Vector3 newPosition, byte newPitch, byte newYaw)
		{
			if (channel.IsLocalPlayer)
			{
				return;
			}

			checkGround(newPosition);

			// We reset interpolation if it's a big skip (e.g., a culling change)
			const float LARGE_DISTANCE = 16f;
			const float SQR_LARGE_DISTANCE = LARGE_DISTANCE * LARGE_DISTANCE;
			bool isLargeDelta = (newPosition - lastUpdatePos).sqrMagnitude > SQR_LARGE_DISTANCE;

			lastUpdatePos = newPosition;
			PitchYawSnapshotInfo newSnapshot = new PitchYawSnapshotInfo(newPosition, newPitch, newYaw * 2.0f);

			if (nsb != null)
			{
				if (isLargeDelta)
				{
					nsb.updateLastSnapshot(newSnapshot);
				}
				else
				{
					nsb.addNewSnapshot(new PitchYawSnapshotInfo(newPosition, newPitch, newYaw * 2.0f));
				}
			}
#if WITH_NSB_LOGGING
			else if(!Provider.isServer)
			{
				NsbLog.WarningFormat("player tellState null buffer ({0})", channel.owner.playerID);
			}
#endif // WITH_NSB_LOGGING
		}

		public void updateMovement()
		{
			lastUpdatePos = transform.localPosition;
			if (nsb != null)
			{
				nsb.updateLastSnapshot(new PitchYawSnapshotInfo(lastUpdatePos, player.look.pitch, player.look.yaw));
			}
#if WITH_NSB_LOGGING
			else if(!Provider.isServer)
			{
				NsbLog.WarningFormat("player updateMovement null buffer ({0})", channel.owner.playerID);
			}
#endif // WITH_NSB_LOGGING

			pendingLaunchVelocity = Vector3.zero;
			velocity = Vector3.zero; // Reset velocity on death.
			mostRecentControllerColliderHit = null;
		}

		private void checkGround(Vector3 position)
		{
			materialName = null;
			materialIsWater = false;

			int mask = RayMasks.BLOCK_COLLISION;
			float CAST_RADIUS = PlayerStance.RADIUS - 0.001f;
			const float CHECK_LENGTH = SKIN_WIDTH + 0.025f;
			Ray ray = new Ray(position + new Vector3(0.0f, CAST_RADIUS, 0.0f), Vector3.down);
			Physics.SphereCast(ray, CAST_RADIUS, out ground, CHECK_LENGTH, mask, QueryTriggerInteraction.Ignore);
			_isGrounded = ground.transform != null;

			if ((channel.IsLocalPlayer || Provider.isServer) && controller.enabled && controller.isGrounded)
			{
				_isGrounded = true;
			}

			if (player.stance.stance == EPlayerStance.CLIMB || player.stance.stance == EPlayerStance.SWIM || player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
			{
				_isGrounded = true;
			}

			if (player.stance.stance == EPlayerStance.CLIMB)
			{
				// 2021-11-16: why does climbing use tile? Sigh.
				materialName = "Tile";
			}
			else if (player.stance.stance == EPlayerStance.SWIM || SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position))
			{
				materialName = "Water";
				materialIsWater = true;
			}
			else if (ground.transform != null)
			{
				if (ground.transform.CompareTag("Ground"))
				{
					materialName = PhysicsTool.GetTerrainMaterialName(transform.position);
				}
				else
				{
					materialName = ground.collider?.sharedMaterial?.name;
				}
			}

#if WITH_MOVEMENT_GIZMOS
			Color debugColor = isGrounded ? Color.green : Color.red;
			RuntimeGizmos.Get().Spherecast(ray, CAST_RADIUS, 0.125f, debugColor, lifespan: PlayerInput.RATE);
			RuntimeGizmos.Get().Arrow(ground.point, ground.normal, 0.5f, debugColor, lifespan: PlayerInput.RATE);
#endif // WITH_MOVEMENT_GIZMOS
		}

#if !DEDICATED_SERVER
		private bool PlayLandAudioClip()
		{
			if (player.stance.stance == EPlayerStance.PRONE || string.IsNullOrEmpty(materialName))
				return false;

			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "BipedLand");
			if (audioDef == null)
				return false;

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return false;

			float volume = 1.0f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SNEAKYBEAKY) * 0.75f);

			if (player.stance.stance == EPlayerStance.CROUCH)
			{
				volume *= 0.5f;
			}

			volume *= 0.15f;

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, audioClip);
			parameters.volume = volume * audioDef.volumeMultiplier;
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
			parameters.SetLinearRolloff(1.0f, 24.0f);
			parameters.Play();

#if LOG_FOOTSTEP_AUDIO
			UnturnedLog.info($"Play material \"{materialName}\" BipedLand audio");
#endif

			lastFootstep = Time.time;

			return true;
		}

		private void PlayFootstepAudioClip()
		{
			string key = player.stance.stance == EPlayerStance.SPRINT ? "FootstepRun" : "FootstepWalk";
			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, key);
			if (audioDef == null)
				return;

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return;

			float volume = 1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SNEAKYBEAKY) * 0.75f);

			if (player.stance.stance == EPlayerStance.CROUCH)
			{
				volume *= 0.5f;
			}

			volume *= 0.125f;

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, audioClip);
			parameters.volume = volume * audioDef.volumeMultiplier;
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
			parameters.SetLinearRolloff(1.0f, 32.0f);
			parameters.Play();

#if LOG_FOOTSTEP_AUDIO
			UnturnedLog.info($"Play material \"{materialName}\" {key} audio");
#endif
		}

		private static MasterBundleReference<OneShotAudioDefinition> lightWadingAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Effects/Physics/Swim/LightWading/Swim_LightWading.asset");
		private static MasterBundleReference<OneShotAudioDefinition> mediumWadingAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Effects/Physics/Swim/MediumWading/Swim_MediumWading.asset");
		private static MasterBundleReference<OneShotAudioDefinition> heavyWadingAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Effects/Physics/Swim/HeavyWading/Swim_HeavyWading.asset");

		internal void PlaySwimAudioClip()
		{
			OneShotAudioDefinition audioDef;
			if (player.stance.stance == EPlayerStance.SWIM)
			{
				if (player.stance.areEyesUnderwater)
				{
					// This clip sounds the most like underwater swimming to me.
					audioDef = mediumWadingAudioRef.loadAsset();
					if (audioDef == null)
					{
						UnturnedLog.warn("Missing built-in medium wading audio");
					}

#if LOG_FOOTSTEP_AUDIO
					UnturnedLog.info($"Play swimming medium wading audio (position {transform.position} eyes underwater)");
#endif
				}
				else
				{
					// Treading water at the surface.
					audioDef = heavyWadingAudioRef.loadAsset();
					if (audioDef == null)
					{
						UnturnedLog.warn("Missing built-in heavy wading audio");
					}

#if LOG_FOOTSTEP_AUDIO
					UnturnedLog.info($"Play swimming heavy wading audio (position {transform.position} eyes aren't underwater)");
#endif
				}
			}
			else
			{
				// Swim stance threshold is 1.25m, so use a midpoint for light wading.
				if (SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position + new Vector3(0.0f, 0.5f, 0.0f)))
				{
					audioDef = lightWadingAudioRef.loadAsset();
					if (audioDef == null)
					{
						UnturnedLog.warn("Missing built-in light wading audio");
					}

#if LOG_FOOTSTEP_AUDIO
					UnturnedLog.info($"Play near-swimming light wading audio (position {transform.position} body underwater)");
#endif
				}
				else
				{
					// Just splashing.
					string key = player.stance.stance == EPlayerStance.SPRINT ? "FootstepRun" : "FootstepWalk";
					audioDef = PhysicMaterialCustomData.GetAudioDef("Water", key);

#if LOG_FOOTSTEP_AUDIO
					UnturnedLog.info($"Play material Water {key} audio");
#endif
				}
			}

			if (audioDef == null)
				return;

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return;

			float volume = 0.15f;
			if (player.stance.stance == EPlayerStance.CROUCH)
			{
				volume *= 0.5f;
			}

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, audioClip);
			parameters.volume = volume * audioDef.volumeMultiplier;
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
			parameters.SetLinearRolloff(1.0f, 32.0f);
			parameters.Play();
		}
#endif // !DEDICATED_SERVER

		private void onVisionUpdated(bool isViewing)
		{
			if (isViewing)
			{
				warp_x = Random.value < 0.25 ? -1 : 1;
				warp_y = Random.value < 0.25 ? -1 : 1;
			}
			else
			{
				warp_x = 1;
				warp_y = 1;
			}
		}

		/// <summary>
		/// Serverside force player to exit vehicle regardless of safe exit points.
		/// </summary>
		/// <returns>True if player was seated in vehicle.</returns>
		public bool forceRemoveFromVehicle()
		{
			if (vehicle != null && channel != null && channel.owner != null)
			{
				byte seat;
				Vector3 point;
				byte angle;
				if (vehicle.forceRemovePlayer(out seat, channel.owner.playerID.steamID, out point, out angle))
				{
					VehicleManager.sendExitVehicle(vehicle, seat, point, angle, true);
					return true;
				}
			}

			// If prior to death player was entering vehicle, then cancel and notify clients.
			if (hasPendingVehicleChange && pendingVehicle != null)
			{
				byte angle = MeasurementTool.angleToByte(transform.rotation.eulerAngles.y);
				VehicleManager.sendExitVehicle(pendingVehicle, pendingSeatIndex, transform.position, angle, true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnityEngine.Assertions.Assert.IsNull(pendingVehicle);
#endif
				return true;
			}

			return false;
		}

		/// <summary>
		/// Dedicated server simulate while input queue is empty.
		/// </summary>
		public void simulate()
		{
			updateRegionAndBound();

			if (channel.IsLocalPlayer)
			{
				lastUpdatePos = transform.position;
			}

			if (hasPendingVehicleChange)
			{
				ApplyPendingVehicleChange();
				return;
			}
		}

		/// <summary>
		/// Dedicated server simulate driving input.
		/// </summary>
		public void simulate(uint simulation, int recov, bool inputBrake, bool inputStamina, Vector3 point, Quaternion rotation, float newSpeed, float newForwardVelocity, float newSteeringInput, float newVelocityInput, float delta)
		{
			updateRegionAndBound();

			if (channel.IsLocalPlayer)
			{
				lastUpdatePos = transform.position;
			}

			velocity = Vector3.zero;
			pendingLaunchVelocity = Vector3.zero;
			mostRecentControllerColliderHit = null;

			if (hasPendingVehicleChange)
			{
				ApplyPendingVehicleChange();
				return;
			}

			if (player.stance.stance == EPlayerStance.DRIVING)
			{
				ServerUpdateTurretAim();

				if (vehicle != null)
				{
					vehicle.simulate(simulation, recov, inputStamina, point, rotation, newSpeed, newForwardVelocity, newSteeringInput, newVelocityInput, delta);
				}
			}
		}

		private void ServerUpdateTurretAim()
		{
			Passenger seatObject = getVehicleSeat();
			if (seatObject?.turret != null)
			{
				if (Mathf.Abs(player.look.lastAngle - player.look.angle) > 1 || Mathf.Abs(player.look.lastRot - player.look.rot) > 1)
				{
					player.look.lastAngle = player.look.angle;
					player.look.lastRot = player.look.rot;

					if (canAddSimulationResultsToUpdates)
					{
						mostRecentlyAddedUpdate = new PlayerStateUpdate(transform.position, player.look.angle, player.look.rot);
						hasMostRecentlyAddedUpdate = true;
						updates.Add(mostRecentlyAddedUpdate);
					}
				}
			}
		}

		/// <summary>
		/// Client and dedicated server simulate walking input.
		/// </summary>
		public void simulate(uint simulation, int recov, int input_x, int input_y, float look_x, float look_y, bool inputJump, bool inputSprint, float deltaTime)
		{
			updateRegionAndBound();

			if (channel.IsLocalPlayer)
			{
				lastUpdatePos = transform.position;
			}

			if (hasPendingVehicleChange)
			{
				ApplyPendingVehicleChange();
				// this does not return, as pausing movement will be handled by the waiting-for-teleport section.
			}

			_move.x = input_x;
			_move.z = input_y;

			if (player.stance.stance == EPlayerStance.SITTING)
			{
				_isMoving = false;
				checkGround(transform.position);
				mostRecentControllerColliderHit = null;

				velocity = Vector3.zero;
				pendingLaunchVelocity = Vector3.zero;

				ServerUpdateTurretAim();
				return;
			}
			else if (player.stance.stance == EPlayerStance.DRIVING)
			{
				_isMoving = false;
				checkGround(transform.position);
				mostRecentControllerColliderHit = null;

				velocity = Vector3.zero;
				pendingLaunchVelocity = Vector3.zero;

				if (channel.IsLocalPlayer)
				{
					vehicle.simulate(simulation, recov, input_x, input_y, look_x, look_y, inputJump, inputSprint, deltaTime);

					if (onVehicleUpdated != null)
					{
						ushort displayCurrentFuel;
						ushort displayMaxFuel;
						vehicle.getDisplayFuel(out displayCurrentFuel, out displayMaxFuel);

						onVehicleUpdated(!vehicle.isUnderwater && !vehicle.isDead, displayCurrentFuel, displayMaxFuel, vehicle.ReplicatedForwardVelocity, vehicle.asset.TargetReverseVelocity, vehicle.asset.TargetForwardVelocity, vehicle.health, vehicle.asset.health, vehicle.batteryCharge);
					}
				}

				return;
			}
			else if (player.stance.stance == EPlayerStance.CLIMB)
			{
				_isMoving = Mathf.Abs(move.x) > 0.1 || Mathf.Abs(move.z) > 0.1;
				checkGround(transform.position);

				// Nelson 2025-09-01: previously, this always set pendingLaunchVelocity to zero.
				// However, when air acceleration multiplier is low, the player isn't necessarily
				// able to build up enough speed to move off the ladder and onto a surface in front.
				// E.g., public issue #5106, and on maps which override the default air acceleration.
				// *Usually* if the player is holding forward input they will immediately start
				// walking forward after exiting the ladder, so if the player is holding forward
				// while climbing I think it's reasonable to queue up a small forward boost.
				if (move.z > 0.1f)
				{
					pendingLaunchVelocity = transform.rotation * new Vector3(0.0f, 0.0f, 0.5f);
				}
				else
				{
					pendingLaunchVelocity = Vector3.zero;
				}

				velocity = new Vector3(0.0f, _move.z * speed * 0.5f, 0.0f);
				mostRecentControllerColliderHit = null;
				if (controller.enabled)
				{
					controller.CheckedMove(velocity * deltaTime);
				}
			}
			else if (player.stance.stance == EPlayerStance.SWIM)
			{
				_isMoving = Mathf.Abs(move.x) > 0.1 || Mathf.Abs(move.z) > 0.1;
				checkGround(transform.position);

				pendingLaunchVelocity = Vector3.zero;

				if (player.stance.isSubmerged || (player.look.pitch > 110 && move.z > 0.1))
				{
					velocity = player.look.aim.rotation * move.normalized * speed * 1.5f;
					if (inputJump)
					{
						velocity.y = SWIM * pluginJumpMultiplier;
					}

					mostRecentControllerColliderHit = null;
					if (controller.enabled)
					{
						controller.CheckedMove(velocity * deltaTime);
					}
				}
				else
				{
					// Swimming along surface

					bool isUnderwater;
					float surfaceElevation;
					WaterUtility.getUnderwaterInfo(transform.position, out isUnderwater, out surfaceElevation);

					velocity = transform.rotation * move.normalized * speed * 1.5f;
					velocity.y = (surfaceElevation - 1.275f - transform.position.y) / 8f;

					mostRecentControllerColliderHit = null;
					if (controller.enabled)
					{
						controller.CheckedMove(velocity * deltaTime);
					}
				}
			}
			else
			{
				_isMoving = Mathf.Abs(move.x) > 0.1 || Mathf.Abs(move.z) > 0.1;
				bool wasGrounded = isGrounded;
				checkGround(transform.position);

				bool updateVelocityAfterMove = false;
				bool sliding = false;
				Vector3 slidingGroundNormal = Vector3.up;

				if (isGrounded && ground.normal.y > 0)
				{
					float angle = Vector3.Angle(Vector3.up, ground.normal);

					float maxWalkableSlope = 59;
					if (Level.info != null && Level.info.configData != null && Level.info.configData.Max_Walkable_Slope > -0.5f)
					{
						maxWalkableSlope = Level.info.configData.Max_Walkable_Slope;
					}

					sliding = angle > maxWalkableSlope;
					slidingGroundNormal = ground.normal;
				}

				if (!sliding)
				{
					if (mostRecentControllerColliderHit != null
						&& mostRecentControllerColliderHit.collider != null // public issue #3726
						&& mostRecentControllerColliderHit.gameObject != null
						&& mostRecentControllerColliderHit.normal.y > 0.0f
						&& mostRecentControllerColliderHit.gameObject.CompareTag("Agent"))
					{
						// Prevent standing on zombie head.
						sliding = true;
						slidingGroundNormal = mostRecentControllerColliderHit.normal;
					}
				}

				if (sliding)
				{
					Vector3 perp = Vector3.Cross(Vector3.up, slidingGroundNormal).normalized;
					Vector3 slide = Vector3.Cross(perp, slidingGroundNormal).normalized;
					velocity += slide * 16.0f * deltaTime;

					// Prevent building up huge velocity by sliding against a wall.
					updateVelocityAfterMove = true;
				}
				else
				{
					Vector3 desiredWalkVelocity = transform.rotation * move.normalized * speed;

					if (isGrounded)
					{
						PhysicsMaterialCharacterFrictionProperties frictionProperties = PhysicMaterialCustomData.GetCharacterFrictionProperties(materialName);
						if (frictionProperties.mode == EPhysicsMaterialCharacterFrictionMode.ImmediatelyResponsive)
						{
							// Rather than adding gravity while grounded to smoothly walk down slopes, we adjust the
							// downward velocity to align with the floor plane. We do not allow an upward velocity here
							// because it would bounce us over the top of the ramp while walking up a slope.
							Vector3 desiredWalkRightDirection = Vector3.Cross(Vector3.up, desiredWalkVelocity).normalized;
							Vector3 desiredWalkForwardDirection = Vector3.Cross(desiredWalkRightDirection, ground.normal).normalized;
							desiredWalkVelocity = desiredWalkForwardDirection * speed;
							desiredWalkVelocity.y = Mathf.Min(desiredWalkVelocity.y, 0.0f);

							// Immediately responsive
							velocity = desiredWalkVelocity;
						}
						else
						{
							Vector3 currentVelocityAlongFloor = Vector3.ProjectOnPlane(velocity, ground.normal);
							float currentSpeedAlongFloor = currentVelocityAlongFloor.magnitude;

							Vector3 desiredWalkRightDirection = Vector3.Cross(Vector3.up, desiredWalkVelocity).normalized;
							Vector3 desiredWalkDirectionAlongFloor = Vector3.Cross(desiredWalkRightDirection, ground.normal).normalized;
							Vector3 desiredWalkVelocityAlongFloor = desiredWalkDirectionAlongFloor * speed;
							// note we do not clamp Y component here so that we can slide off jumps
							desiredWalkVelocityAlongFloor *= frictionProperties.maxSpeedMultiplier;
							float desiredSpeed = desiredWalkVelocityAlongFloor.magnitude;

							float maxSpeed;
							if (currentSpeedAlongFloor > desiredSpeed)
							{
								// Base deceleration is 2.0 m/s²
								float deceleration = -2.0f * frictionProperties.decelerationMultiplier;
								maxSpeed = Mathf.Max(desiredSpeed, currentSpeedAlongFloor + (deceleration * deltaTime));
							}
							else
							{
								maxSpeed = desiredSpeed;
							}

							// Questionable units-wise, but pretend base acceleration is proportional to desired speed.
							// For example if walk speed is 4.5 m/s then acceleration is 4.5 m/s².
							Vector3 acceleration = desiredWalkVelocityAlongFloor * frictionProperties.accelerationMultiplier;

							Vector3 newVelocity = currentVelocityAlongFloor + (acceleration * deltaTime);
							velocity = newVelocity.ClampMagnitude(maxSpeed);

							// Prevent sticking to walls at high velocity.
							updateVelocityAfterMove = true;
						}
					}
					else
					{
						velocity.y += Physics.gravity.y * (fall <= 0 ? totalGravityMultiplier : 1f) * deltaTime * 3;

						// Clamp free-fall / terminal velocity
						float minVerticalVelocity = totalGravityMultiplier < 0.99f ? Physics.gravity.y * 2.0f * totalGravityMultiplier : -100.0f;
						velocity.y = Mathf.Max(minVerticalVelocity, velocity.y);

						// Midair, maybe with already high rocket-jump speed
						float desiredWalkSpeed = desiredWalkVelocity.GetHorizontalMagnitude();
						Vector3 currentHorizontalVelocity = velocity.GetHorizontal();
						float currentHorizontalSpeed = velocity.GetHorizontalMagnitude();
						float maxHorizontalSpeed;
						if (currentHorizontalSpeed > desiredWalkSpeed)
						{
							float deceleration = 2.0f * Provider.modeConfigData.Gameplay.AirStrafing_Deceleration_Multiplier;
							maxHorizontalSpeed = Mathf.Max(desiredWalkSpeed, currentHorizontalSpeed - (deceleration * deltaTime));
						}
						else
						{
							maxHorizontalSpeed = desiredWalkSpeed;
						}
						Vector3 accel = desiredWalkVelocity * (8.0f * Provider.modeConfigData.Gameplay.AirStrafing_Acceleration_Multiplier);
						Vector3 newHorizontalVelocity = currentHorizontalVelocity + (accel * deltaTime);
						newHorizontalVelocity = newHorizontalVelocity.ClampHorizontalMagnitude(maxHorizontalSpeed);
						velocity.x = newHorizontalVelocity.x;
						velocity.z = newHorizontalVelocity.z;

						// Prevent sticking to walls at high velocity.
						updateVelocityAfterMove = true;
					}
				}

				if (inputJump)
				{
					if (isGrounded && !player.life.isBroken && player.life.stamina >= 10 * (1f - (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.PARKOUR) * 0.5f)) && (player.stance.stance == EPlayerStance.STAND || player.stance.stance == EPlayerStance.SPRINT) && !MathfEx.IsNearlyZero(pluginJumpMultiplier, 0.001f))
					{
						velocity.y = JUMP * (1f + (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.PARKOUR) * 0.25f)) * pluginJumpMultiplier;
						player.life.askTire((byte) (10 * (1f - (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.PARKOUR) * 0.5f))));
					}
				}

				velocity += pendingLaunchVelocity;
				pendingLaunchVelocity = Vector3.zero;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (enableFly && MainCamera.instance != null)
				{
					velocity = MainCamera.instance.transform.rotation * move * speed;
					if (player.stance.stance == EPlayerStance.SPRINT)
					{
						velocity *= 2.0f;
					}
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

				if (channel.IsLocalPlayer && LoadingUI.isBlocked)
				{
					// Client does not move or fall while loading into the level. Potentially important in
					// singleplayer while waiting for the surrounding geometry to enable its collision.
					velocity = Vector3.zero;
				}
				else
				{
					Vector3 oldPosition = transform.position;
					mostRecentControllerColliderHit = null;
					if (controller.enabled)
					{
						controller.CheckedMove(velocity * deltaTime);
					}

					// Moved from the earlier ground test because we want onLanded to happen before updateVelocityAfterMove 
					if (!wasGrounded)
					{
						checkGround(transform.position);

						if (isGrounded)
						{
							// Before velocity was added this passed the vertical distance from the last grounded height.
							onLanded?.Invoke(velocity.y);

#if !DEDICATED_SERVER
							if (!player.input.isResimulating && Mathf.Abs(velocity.y) > 1.0f)
							{
								PlayLandAudioClip();
							}
#endif // !DEDICATED_SERVER
						}
					}
					else
					{
						// Player *was* grounded, so let's see if we should slightly snap the player to the ground.
						// Don't snap if velocity was upward to prevent sticking to ground when e.g. jumping.
						if (velocity.y < 0.01f)
						{
							int mask = RayMasks.BLOCK_COLLISION;
							float CAST_RADIUS = PlayerStance.RADIUS - 0.001f;
							float snapLength = controller.stepOffset + SKIN_WIDTH;
							Ray snapRay = new Ray(transform.position + new Vector3(0.0f, CAST_RADIUS, 0.0f), Vector3.down);
							bool snapRayHit = Physics.SphereCast(snapRay, CAST_RADIUS, out RaycastHit snapHit, snapLength, mask, QueryTriggerInteraction.Ignore);
#if WITH_FLOOR_SNAPPING_GIZMOS
							RuntimeGizmos.Get().Spherecast(snapRay, CAST_RADIUS, snapLength, snapHit, Color.green, Color.red, 0.1f);
#endif
							if (snapRayHit)
							{
								float snapDelta = (snapHit.distance - SKIN_WIDTH);
								if (snapDelta > Mathf.Epsilon)
								{
									Vector3 snapFloorNormal = snapHit.normal;
									float maxWalkableSlope = 59;
									if (Level.info != null && Level.info.configData != null && Level.info.configData.Max_Walkable_Slope > -0.5f)
									{
										maxWalkableSlope = Level.info.configData.Max_Walkable_Slope;
									}

									// Nelson 2024-08-19: Snapping to *any* surface was quickly exploited to climb trees!
									bool canSnap = Vector3.Angle(Vector3.up, snapFloorNormal) < maxWalkableSlope;
#if WITH_FLOOR_SNAPPING_GIZMOS
									RuntimeGizmos.Get().Arrow(snapHit.point, snapHit.normal, 0.4f, canSnap ? Color.green : Color.red, 0.1f, EGizmoLayer.Foreground);
#endif
									if (canSnap)
									{
										transform.position += new Vector3(0.0f, -snapDelta, 0.0f);
									}
								}
							}
						}
					}

					if (updateVelocityAfterMove)
					{
						// We do not always want to do this, for example when walking up a slope this would launch
						// us over the top. Useful while midair to prevent player from sticking to walls at high velocity.
						velocity = (transform.position - oldPosition) / deltaTime;
					}
				}
			}

			if (Level.info != null && Level.info.configData.Use_Legacy_Clip_Borders)
			{
				Vector3 clampedPosition = transform.position;

				// Clamp players into horizontal bounds, but only within a certain range because
				// technically the Russia lab is outside the map border.
				float borderXZMin = (Level.size / 2.0f) - Level.border;
				float borderXZMax = borderXZMin + 8.0f;

				bool bPushedIntoBounds = false;

				if (clampedPosition.x > -borderXZMax && clampedPosition.x < -borderXZMin)
				{
					clampedPosition.x = -borderXZMin + 1.0f;
					bPushedIntoBounds = true;
				}
				else if (clampedPosition.x < borderXZMax && clampedPosition.x > borderXZMin)
				{
					clampedPosition.x = borderXZMin - 1.0f;
					bPushedIntoBounds = true;
				}

				if (clampedPosition.z > -borderXZMax && clampedPosition.z < -borderXZMin)
				{
					clampedPosition.z = -borderXZMin + 1.0f;
					bPushedIntoBounds = true;
				}
				else if (clampedPosition.z < borderXZMax && clampedPosition.z > borderXZMin)
				{
					clampedPosition.z = borderXZMin - 1.0f;
					bPushedIntoBounds = true;
				}

				if (bPushedIntoBounds)
				{
					// Lift up to help prevent falling into ground.
					clampedPosition.y += 8.0f;
				}

				// Caps are 8m tall and offset +/-4m from the level bounds.
				clampedPosition.y = Mathf.Clamp(clampedPosition.y, 0, Level.HEIGHT);

				transform.position = clampedPosition;
			}

			// Only server can perform ground adjustment because bypassUndergroundWhitelist flag is not replicated.
			if (Provider.isServer)
			{
				// Player is admin in singleplayer, so we only allow maintenance access on dedicated servers.
				bool bypassWhitelist = bypassUndergroundWhitelist || (Dedicator.IsDedicatedServer && channel.owner.isAdmin);
				if (!bypassWhitelist)
				{
					Vector3 adjustedPosition = transform.position;
					if (UndergroundAllowlist.AdjustPosition(ref adjustedPosition, 0.5f))
					{
						transform.position = adjustedPosition;
					}
				}
			}

			// After level clamping and ground adjustment because those affect server result.
			if (!channel.IsLocalPlayer && Provider.isServer)
			{
				if (updates != null)
				{
					Vector3 currentPos = transform.position;
					if (Mathf.Abs(player.look.lastAngle - player.look.angle) > 1 || Mathf.Abs(player.look.lastRot - player.look.rot) > 1 || Mathf.Abs(lastUpdatePos.x - currentPos.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatePos.y - currentPos.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatePos.z - currentPos.z) > Provider.UPDATE_DISTANCE)
					{
						player.look.lastAngle = player.look.angle;
						player.look.lastRot = player.look.rot;
						lastUpdatePos = currentPos;

						if (canAddSimulationResultsToUpdates)
						{
							mostRecentlyAddedUpdate = new PlayerStateUpdate(currentPos, player.look.angle, player.look.rot);
							hasMostRecentlyAddedUpdate = true;
							updates.Add(mostRecentlyAddedUpdate);
						}
					}
				}
			}
		}

		private void Update()
		{
			if (nsb != null)
			{
				snapshot = nsb.getCurrentSnapshot();
			}
#if WITH_NSB_LOGGING
			else if(!Provider.isServer)
			{
				NsbLog.WarningFormat("player Update null buffer ({0})", channel.owner.playerID);
			}
#endif // WITH_NSB_LOGGING

			UnityEngine.Profiling.Profiler.BeginSample("Input");

			if (channel.IsLocalPlayer)
			{
				if (!PlayerUI.window.showCursor && !LoadingUI.isBlocked)
				{
					_jump = InputEx.GetKey(ControlsSettings.jump);

					if (getVehicle() != null)
					{
						if (InputEx.GetKeyDown(ControlsSettings.locker))
						{
							VehicleManager.sendVehicleLock();
						}

						if (InputEx.GetKeyDown(ControlsSettings.primary))
						{
							VehicleManager.sendVehicleHorn();
						}

						if (InputEx.GetKeyDown(ControlsSettings.secondary))
						{
							VehicleManager.sendVehicleHeadlights();
						}

						if (InputEx.GetKeyDown(ControlsSettings.other))
						{
							VehicleManager.sendVehicleBonus();
						}
					}

					if (getVehicle() != null && getVehicle().asset != null && (getVehicle().asset.engine == EEngine.PLANE || getVehicle().asset.engine == EEngine.HELICOPTER || getVehicle().asset.engine == EEngine.BLIMP))
					{
						if (InputEx.GetKey(ControlsSettings.yawLeft))
						{
							input_x = -1;
						}
						else if (InputEx.GetKey(ControlsSettings.yawRight))
						{
							input_x = 1;
						}
						else
						{
							input_x = 0;
						}

						if (InputEx.GetKey(ControlsSettings.thrustIncrease))
						{
							input_y = 1;
						}
						else if (InputEx.GetKey(ControlsSettings.thrustDecrease))
						{
							input_y = -1;
						}
						else
						{
							input_y = 0;
						}
					}
					else
					{
						if (InputEx.GetKey(ControlsSettings.left))
						{
							input_x = -1;
						}
						else if (InputEx.GetKey(ControlsSettings.right))
						{
							input_x = 1;
						}
						else
						{
							input_x = 0;
						}

						if (InputEx.GetKey(ControlsSettings.up))
						{
							input_y = 1;
						}
						else if (InputEx.GetKey(ControlsSettings.down))
						{
							input_y = -1;
						}
						else
						{
							input_y = 0;
						}
					}
				}
				else
				{
					_jump = false;

					input_x = 0;
					input_y = 0;
				}

				input_x *= warp_x;
				input_y *= warp_y;

				if (player.look.IsControllingFreecam)
				{
					_jump = false;
					_horizontal = 1;
					_vertical = 1;
				}
				else
				{
					_horizontal = (byte) (input_x + 1);
					_vertical = (byte) (input_y + 1);
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Footsteps");

#if !DEDICATED_SERVER
			if (!Dedicator.IsDedicatedServer)
			{
				if (Time.time - lastFootstep > 2.1f / speed)
				{
					lastFootstep = Time.time;

					bool playedLand = false;

					if (!channel.IsLocalPlayer)
					{
						bool wasGrounded = isGrounded;
						checkGround(transform.position);
						if (!wasGrounded && isGrounded)
						{
							playedLand = PlayLandAudioClip();
						}
					}

					if (isGrounded && !playedLand)
					{
						if (isMoving && player.stance.stance != EPlayerStance.PRONE)
						{
							if (materialIsWater || player.stance.stance == EPlayerStance.SWIM)
							{
								PlaySwimAudioClip();
							}
							else if (!string.IsNullOrEmpty(materialName))
							{
								PlayFootstepAudioClip();
							}
						}
					}
				}
			}
#endif // !DEDICATED_SERVER

			UnityEngine.Profiling.Profiler.EndSample();

			if (channel.IsLocalPlayer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Orbit");

				if (player.look.IsControllingFreecam)
				{
					if (!player.workzone.isBuilding || InputEx.GetKey(ControlsSettings.secondary))
					{
						bool acceptInput = !PlayerUI.window.showCursor;
						float scrollWheelInput = acceptInput ? Input.GetAxis("mouse_z") : 0.0f;

						if (InputEx.GetKey(ControlsSettings.other))
						{
							if (player.look.freecamVerticalFieldOfView > 0.0f)
							{
								player.look.freecamVerticalFieldOfView = Mathf.Clamp(player.look.freecamVerticalFieldOfView + scrollWheelInput * 5.0f, 1.0f, 179.0f);
							}
						}
						else
						{
							player.look.orbitSpeed = Mathf.Clamp(player.look.orbitSpeed + (scrollWheelInput * 0.2f * player.look.orbitSpeed), 0.5f, 2048);
						}

						Vector3 orbitHorizontalMotion = MainCamera.instance.transform.right * input_x * Time.deltaTime * player.look.orbitSpeed;
						if (player.look.isFocusing)
						{
							// Nelson 2024-06-11: Fixes drifting closer or further away while spinning around the focal
							// point. (public issue #4487)
							Vector3 focalPoint = player.first.position + Vector3.up;
							Vector3 oldCameraPosition = player.look.lockPosition + player.look.orbitPosition;
							Vector3 oldFocusVector = focalPoint - oldCameraPosition;
							float oldHorizontalMagnitude = oldFocusVector.GetHorizontalMagnitude();

							Vector3 newOrbitPosition = player.look.orbitPosition + orbitHorizontalMotion;
							Vector3 newCameraPosition = player.look.lockPosition + newOrbitPosition;
							Vector3 newFocusVector = focalPoint - newCameraPosition;
							float newHorizontalMagnitude = newFocusVector.GetHorizontalMagnitude();
							if (newHorizontalMagnitude < 0.001f)
							{
								// Prevent division by zero;
								newHorizontalMagnitude = 1.0f;
							}

							float scaleFactor = oldHorizontalMagnitude / newHorizontalMagnitude;
							newOrbitPosition.x *= scaleFactor;
							newOrbitPosition.z *= scaleFactor;
							player.look.orbitPosition = newOrbitPosition;

							Debug.Assert(MathfEx.IsNearlyEqual(oldHorizontalMagnitude, (focalPoint - (player.look.lockPosition + player.look.orbitPosition)).GetHorizontalMagnitude()));
						}
						else
						{
							player.look.orbitPosition += orbitHorizontalMotion;
						}
						player.look.orbitPosition += MainCamera.instance.transform.forward * input_y * Time.deltaTime * player.look.orbitSpeed;

						float height = 0.0f;
						if (acceptInput)
						{
							if (InputEx.GetKey(ControlsSettings.ascend))
							{
								height = 1.0f;
							}
							else if (InputEx.GetKey(ControlsSettings.descend))
							{
								height = -1.0f;
							}
						}

						player.look.orbitPosition += Vector3.up * height * Time.deltaTime * player.look.orbitSpeed;
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Lerp");

				if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
				{
					player.first.localPosition = Vector3.zero;
					player.third.localPosition = Vector3.zero;
				}
				else
				{
					player.first.position = Vector3.Lerp(lastUpdatePos, transform.position, (Time.realtimeSinceStartup - player.input.tick) / PlayerInput.RATE);
					// Nelson 2024-08-19: Previously, this pushed the first-person position down by 10 cm while prone.
					// I think we can remove that offset now that the character is snapped to the ground.
					player.third.position = player.first.position;
				}

				player.look.aim.parent.transform.position = player.first.position;//Vector3.Lerp(position, transform.position, (Time.realtimeSinceStartup-player.input.tick)/PlayerInput.RATE);//player.animator.hook.transform.position;

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Stats");

				if (vehicle != null)
				{
					if ((transform.position - lastStatPos).sqrMagnitude > 1024)
					{
						lastStatPos = transform.position;
					}
					else if (Time.realtimeSinceStartup - lastStatTime > 1)
					{
						lastStatTime = Time.realtimeSinceStartup;

						if ((transform.position - lastStatPos).sqrMagnitude > 0.1f)
						{
							int data;
							if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Travel_Vehicle", out data))
							{
								Provider.provider.statisticsService.userStatisticsService.setStatistic("Travel_Vehicle", data + (int) (transform.position - lastStatPos).magnitude);
							}

							lastStatPos = transform.position;
						}
					}
				}
				else
				{
					if ((transform.position - lastStatPos).sqrMagnitude > 256)
					{
						lastStatPos = transform.position;
					}
					else if (Time.realtimeSinceStartup - lastStatTime > 1)
					{
						lastStatTime = Time.realtimeSinceStartup;

						if ((transform.position - lastStatPos).sqrMagnitude > 0.1f)
						{
							int data;
							if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Travel_Foot", out data))
							{
								Provider.provider.statisticsService.userStatisticsService.setStatistic("Travel_Foot", data + (int) (transform.position - lastStatPos).magnitude);
							}

							lastStatPos = transform.position;
						}
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();
			}
			else if (!Provider.isServer)
			{
				if (player.stance.stance == EPlayerStance.SITTING || player.stance.stance == EPlayerStance.DRIVING)
				{
					_isMoving = false;

					transform.localPosition = Vector3.zero;
				}
				else
				{
					if (Mathf.Abs(lastUpdatePos.x - transform.position.x) > 0.01f || Mathf.Abs(lastUpdatePos.y - transform.position.y) > 0.01f || Mathf.Abs(lastUpdatePos.z - transform.position.z) > 0.01f)
					{
						_isMoving = true;
					}
					else
					{
						_isMoving = false;
					}

					transform.localPosition = snapshot.pos;
				}
			}

			if (!channel.IsLocalPlayer)
			{
				if (player.third != null)
				{
					if (player.stance.stance == EPlayerStance.PRONE)
					{
						player.third.localPosition = new Vector3(0.0f, -0.1f, 0.0f);
					}
					else
					{
						player.third.localPosition = Vector3.zero;
					}
				}
			}
		}

		private void updateRegionAndBound()
		{
			byte new_x;
			byte new_y;

			if (Regions.tryGetCoordinate(transform.position, out new_x, out new_y))
			{
				if (new_x != region_x || new_y != region_y)
				{
					byte old_x = region_x;
					byte old_y = region_y;

					_region_x = new_x;
					_region_y = new_y;

					updateRegionOld_X = old_x;
					updateRegionOld_Y = old_y;
					updateRegionNew_X = new_x;
					updateRegionNew_Y = new_y;
					updateRegionIndex = 0;
				}
			}

			if (updateRegionIndex < 6)
			{
				bool canIncrementIndex = true;

				onRegionUpdated?.Invoke(player, updateRegionOld_X, updateRegionOld_Y, updateRegionNew_X, updateRegionNew_Y, updateRegionIndex, ref canIncrementIndex);

				if (canIncrementIndex)
				{
					updateRegionIndex++;
				}
			}

			byte newBound;
			LevelNavigation.tryGetBounds(transform.position, out newBound);

			if (newBound != bound)
			{
				byte oldBound = bound;

				_bound = newBound;

				onBoundUpdated?.Invoke(player, oldBound, newBound);
			}

			if (Provider.isServer)
			{
				byte newNav;
				LevelNavigation.tryGetNavigation(transform.position, out newNav);

				if (newNav != nav)
				{
					byte oldNav = nav;
					_nav = newNav;

					TriggerPlayerNavChanged(oldNav, newNav);
				}
			}

			bool newSafe = false;
			newSafe = LevelNodes.isPointInsideSafezone(transform.position, out isSafeInfo);

			bool newRadiated = false;
			IDeadzoneNode newDeadzoneInfo = null;

			HordePurchaseVolume newPurchaseNode = HordePurchaseVolumeManager.Get().GetFirstOverlappingVolume(transform.position);
			effectNode = null;
			inSnow = LevelLighting.isPositionSnowy(transform.position);

			SDG.Framework.Devkit.AmbianceVolume ambianceVolume = SDG.Framework.Devkit.AmbianceVolumeManager.Get().GetFirstOverlappingVolume(transform.position);
			if (ambianceVolume != null)
			{
				effectNode = ambianceVolume;

				if (!inSnow && Level.info != null && Level.info.configData.Use_Snow_Volumes)
				{
					inSnow = (ambianceVolume.weatherMask & (1U << 1)) != 0;
				}

				WeatherMask = ambianceVolume.weatherMask;
			}
			else
			{
				LevelAsset levelAsset = Level.getAsset();
				WeatherMask = levelAsset != null ? levelAsset.globalWeatherMask : uint.MaxValue;
			}

			inSnow &= LevelLighting.snowyness == ELightingSnow.BLIZZARD;

			SDG.Framework.Devkit.DeadzoneVolume deadzoneVolume = DeadzoneVolumeManager.Get().GetMostDangerousOverlappingVolume(transform.position);
			if (deadzoneVolume != null)
			{
				newRadiated = true;
				newDeadzoneInfo = deadzoneVolume;
			}

			if (newSafe != isSafe)
			{
				_isSafe = newSafe;

				onSafetyUpdated?.Invoke(isSafe);
			}

			ActiveDeadzone = newDeadzoneInfo;
			if (newRadiated != isRadiated)
			{
				_isRadiated = newRadiated;

				onRadiationUpdated?.Invoke(isRadiated);
			}

			if (newPurchaseNode != purchaseNode)
			{
				_purchaseNode = newPurchaseNode;

				onPurchaseUpdated?.Invoke(purchaseNode);
			}

			player.inventory.closeDistantStorage();
		}

		internal void InitializePlayer()
		{
			itemGravityMultiplier = 1;
			pluginGravityMultiplier = 1;
			pluginSpeedMultiplier = 1;

			_region_x = 255;
			_region_y = 255;
			_bound = 255;
			_nav = 255;

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				_loadedRegions = new LoadedRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						loadedRegions[x, y] = new LoadedRegion();
					}
				}

				_loadedBounds = new LoadedBound[LevelNavigation.bounds.Count];
				for (byte index = 0; index < LevelNavigation.bounds.Count; index++)
				{
					loadedBounds[index] = new LoadedBound();
				}
			}

			warp_x = 1;
			warp_y = 1;

			if (Provider.isServer || channel.IsLocalPlayer) // if(channel.isOwner)//
			{
				controller = GetComponent<CharacterController>();

				// Overlap recovery tries to force character's capsule out of
				// rigidbodies that it overlaps which might be fine in singleplayer,
				// but is inevitable in multiplayer. Often players abused overlaps
				// to teleport through walls and the like, so it's important to disable.
				//
				// Sadly there are issues when walking toward slopes that bend inward,
				// so CheckedMove performs a capsule sweep from start to end.
				// To test if it works in the future walk toward the inside of a barracks building in PEI base.
				//
				// 2021-09-16: tried enabling again because CheckedMove can return false positives, for example when
				// substep moves around a corner. Unfortunately vehicles falling on players heads or driven into a
				// player near a wall were abused in multiplayer to teleport through walls. In the future we might be
				// able to work around this by using fully authoritative vehicle physics.
				//
				// 2026-05-04: there is speculation that overlap and sweep tests necessary when overlap recovery is
				// *disabled* were very expensive on maps with lots of colliders, so we added an option to enable it.
				controller.enableOverlapRecovery = CharacterControllerExtension.EnableOverlapRecovery;
			}

			if (Provider.isServer)
			{
				//capsule = gameObject.AddComponent<CapsuleCollider>();
				//capsule.isTrigger = true;
				//capsule.center = new Vector3(0, 1, 0);
				//capsule.radius = controller.radius;
				//capsule.height = controller.height;
				//capsule.enabled = false; //!channel.isOwner;

				player.life.onVisionUpdated += onVisionUpdated;
			}
			else
			{
				nsb = new NetworkSnapshotBuffer<PitchYawSnapshotInfo>(Provider.UPDATE_TIME, Provider.UPDATE_DELAY);
			}

			applySize();

			if (Dedicator.IsDedicatedServer)
			{
				gameObject.AddComponent<Rigidbody>();
				GetComponent<Rigidbody>().useGravity = false;
				GetComponent<Rigidbody>().isKinematic = true;
			}

			updateMovement();

			updates = new List<PlayerStateUpdate>();
			canAddSimulationResultsToUpdates = true;

			lastFootstep = Time.time;
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			mostRecentControllerColliderHit = hit;
		}

		private void OnDrawGizmos()
		{
			if (nsb == null)
			{
				return;
			}

			for (int index = 0; index < nsb.snapshots.Length; index++)
			{
				if (nsb.snapshots[index].timestamp <= 0.01f)
				{
					return;
				}

				PitchYawSnapshotInfo info = nsb.snapshots[index].info;
				Gizmos.DrawLine(info.pos, info.pos + (Vector3.up * 2));
			}
		}

		private void OnDestroy()
		{
			updates = null;
		}

		/// <summary>
		/// In the future this can probably replace checkGround for locally simulated character?
		/// (Unturned only started using OnControllerColliderHit on 2023-01-31)
		///
		/// 2023-02-28: be careful with .gameObject property because it returns .collider.gameObject
		/// which can cause a null reference exception. (public issue #3726)
		/// </summary>
		private ControllerColliderHit mostRecentControllerColliderHit;
	}
}
