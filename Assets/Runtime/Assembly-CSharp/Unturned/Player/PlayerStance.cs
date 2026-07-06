////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define DRAW_LADDER_GIZMOS
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void StanceUpdated();

	public class PlayerStance : PlayerCaller
	{
		public static readonly float COOLDOWN = 0.5f;

		public static readonly float RADIUS = 0.4f;

		public static readonly float DETECT_MOVE = 1.1f;

		public static readonly float DETECT_FORWARD = 48;
		public static readonly float DETECT_BACKWARD = 24;
		public static readonly float DETECT_SPRINT = 20;
		public static readonly float DETECT_STAND = 12;
		public static readonly float DETECT_CROUCH = 6;
		public static readonly float DETECT_PRONE = 3;

		public StanceUpdated onStanceUpdated;

		private EPlayerStance _stance;
		public EPlayerStance stance
		{
			get => _stance;

			set =>
				// Plugins may have been directly setting stance on the server causing desync,
				// so the original functionality was moved into internalSetStance.
				checkStance(value, true);
		}

		/// <summary>
		/// Invoked after any player's stance changes (not including loading).
		/// </summary>
		public static event System.Action<PlayerStance> OnStanceChanged_Global;

		/// <summary>
		/// Stance to fit available space when loading in.
		/// </summary>
		public EPlayerStance initialStance = EPlayerStance.STAND;

		/// <returns>Distance zombies can detect this player within.</returns>
		public float GetStealthDetectionRadius()
		{
			if (player.movement.nav != 255 && ZombieManager.regions[player.movement.nav].isHyper)
			{
				return 24.0f;
			}
			else if (stance == EPlayerStance.DRIVING)
			{
				if (player.movement.getVehicle().sirensOn)
				{
					return DETECT_FORWARD;
				}
				else
				{
					return DETECT_FORWARD * player.movement.getVehicle().GetReplicatedForwardSpeedPercentageOfTargetSpeed();
				}
			}
			else if (stance == EPlayerStance.SITTING)
			{
				return 0;
			}
			else if (stance == EPlayerStance.SPRINT)
			{
				return DETECT_SPRINT * (player.movement.isMoving ? DETECT_MOVE : 1);
			}
			else if (stance == EPlayerStance.STAND || stance == EPlayerStance.SWIM)
			{
				float volume = 1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SNEAKYBEAKY) * 0.5f);

				return DETECT_STAND * (player.movement.isMoving ? DETECT_MOVE : 1) * volume;
			}
			else
			{
				float volume = 1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SNEAKYBEAKY) * 0.75f);

				if (stance == EPlayerStance.CROUCH || stance == EPlayerStance.CLIMB)
				{
					return DETECT_CROUCH * (player.movement.isMoving ? DETECT_MOVE : 1) * volume;
				}
				else if (stance == EPlayerStance.PRONE)
				{
					return DETECT_PRONE * (player.movement.isMoving ? DETECT_MOVE : 1) * volume;
				}
			}

			return 0;
		}

		[System.Obsolete("Renamed to GetStealthDetectionRadius.")]
		public float radius
		{
			get => GetStealthDetectionRadius();
		}

		private float lastStance;
#if !DEDICATED_SERVER
		private float lastSubmergeSound;
#endif // !DEDICATED_SERVER
		private float lastDetect;

		private float lastHold;
		private bool isHolding;

		private bool _localWantsToCrouch;
		public bool crouch => _localWantsToCrouch;

		private bool _localWantsToProne;
		public bool prone => _localWantsToProne;

		private bool _localWantsToSprint;
		public bool sprint => _localWantsToSprint;

		internal bool localWantsToSteadyAim;

		private bool _isSubmerged;
		/// <summary>
		/// Older, cached version of areEyesUnderwater.
		/// </summary>
		public bool isSubmerged => _isSubmerged;

		private bool _inShallows;

		internal bool canCurrentStanceTransitionToClimbing => stance == EPlayerStance.STAND || stance == EPlayerStance.SPRINT || stance == EPlayerStance.SWIM;

		/// <summary>
		/// Return false if there are any external restrictions (e.g. reloading, handcuffed) preventing climbing.
		/// </summary>
		internal bool isAllowedToStartClimbing => !player.equipment.isBusy && player.animator.gesture != EPlayerGesture.ARREST_START;

		/// <summary>
		/// Test whether bottom of controller is currently inside a water volume.
		/// </summary>
		public bool areFeetUnderwater => SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position);

		/// <summary>
		/// Test whether viewpoint is currently inside a water volume.
		/// </summary>
		public bool areEyesUnderwater => SDG.Framework.Water.WaterUtility.isPointUnderwater(player.look.aim.position);

		/// <summary>
		/// Test whether body is currently inside a water volume.
		/// Enters the swimming stance while true.
		/// </summary>
		public bool isBodyUnderwater => SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position + new Vector3(0, 1.25f, 0));

		private RaycastHit ladder;

		/// <summary>
		/// Draw debug capsule matching the player size.
		/// </summary>
		public static void drawCapsule(Vector3 position, float height, Color color, float lifespan = 0.0f)
		{
			Vector3 p0 = position + new Vector3(0.0f, RADIUS, 0.0f);
			Vector3 p1 = position + new Vector3(0.0f, height - RADIUS, 0.0f);
			RuntimeGizmos.Get().Capsule(p0, p1, RADIUS, color, lifespan);
		}

		/// <summary>
		/// Draw standing-height debug capsule matching the player size.
		/// </summary>
		public static void drawStandingCapsule(Vector3 position, Color color, float lifespan = 0.0f)
		{
			Vector3 p0 = position + new Vector3(0.0f, RADIUS, 0.0f);
			Vector3 p1 = position + new Vector3(0.0f, PlayerMovement.HEIGHT_STAND - RADIUS, 0.0f);
			RuntimeGizmos.Get().Capsule(p0, p1, RADIUS, color, lifespan);
		}

		/// <summary>
		/// Is there enough height for our capsule at a position?
		/// </summary>
		public static bool hasHeightClearanceAtPosition(Vector3 position, float height)
		{
			Vector3 p0 = position + new Vector3(0.0f, RADIUS + 0.01f, 0.0f); // +0.01 height to help avoid detecting ground
			Vector3 p1 = position + new Vector3(0.0f, height - RADIUS, 0.0f); // must use full height to prevent standing up into object

			// 2022-06-23: excluding ground from capsule and moving into linecast because for some mysterious reason
			// the capsule tests are not finding terrain in multiplayer whereas line tests are. (issue #3273)
			if (Physics.CheckCapsule(p0, p1, RADIUS, RayMasks.BLOCK_STANCE & (~LayerMasks.GROUND), QueryTriggerInteraction.Ignore))
				return false;

			if (Physics.Linecast(position + new Vector3(0.0f, height, 0.0f), position + new Vector3(0.0f, 0.01f, 0.0f), RayMasks.GROUND))
				return false;

			return true;
		}

		/// <summary>
		/// Could a standing player capsule fit at the given position?
		/// </summary>
		public static bool hasStandingHeightClearanceAtPosition(Vector3 position)
		{
			return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_STAND);
		}

		/// <summary>
		/// Could a crouching player capsule fit at the given position?
		/// </summary>
		public static bool hasCrouchingHeightClearanceAtPosition(Vector3 position)
		{
			return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_CROUCH);
		}

		/// <summary>
		/// Could a prone player capsule fit at the given position?
		/// </summary>
		public static bool hasProneHeightClearanceAtPosition(Vector3 position)
		{
			return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_PRONE);
		}

		/// <summary>
		/// Could a standing player capsule teleport to the given position?
		/// </summary>
		public static bool hasTeleportClearanceAtPosition(Vector3 position)
		{
			const float teleportPadding = 0.5f; // Keep in sync with Player.askTeleport
			return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_STAND + teleportPadding);
		}

		/// <summary>
		/// Is there any compatible stance that can fit at position?
		/// </summary>
		public static bool getStanceForPosition(Vector3 position, ref EPlayerStance stance)
		{
			bool hasStandRoom = hasStandingHeightClearanceAtPosition(position);
			if (hasStandRoom)
			{
				stance = EPlayerStance.STAND;
				return true;
			}

			bool hasCrouchRoom = hasCrouchingHeightClearanceAtPosition(position);
			if (hasCrouchRoom)
			{
				stance = EPlayerStance.CROUCH;
				return true;
			}

			bool hasProneRoom = hasProneHeightClearanceAtPosition(position);
			if (hasProneRoom)
			{
				stance = EPlayerStance.PRONE;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Using our capsule's current height would there be enough space at a given position?
		/// </summary>
		public bool wouldHaveHeightClearanceAtPosition(Vector3 position, float padding = 0.0f)
		{
			CharacterController cc = player.movement.controller;
			float height = cc != null ? cc.height : PlayerMovement.HEIGHT_STAND;
			return hasHeightClearanceAtPosition(position, height + padding);
		}

		/// <summary>
		/// Does capsule have appropriate clearance for a pending height change?
		/// </summary>
		public bool hasHeightClearance(float height)
		{
			return hasHeightClearanceAtPosition(transform.position, height);
		}

		private EPlayerHeight getHeightForStance(EPlayerStance testStance)
		{
			switch (testStance)
			{
				case EPlayerStance.PRONE:
					return EPlayerHeight.PRONE;

				case EPlayerStance.CROUCH:
					return EPlayerHeight.CROUCH;

				default:
					return EPlayerHeight.STAND;
			}
		}

		internal void internalSetStance(EPlayerStance newStance)
		{
			if (_stance != newStance)
			{
				_stance = newStance;

				EPlayerHeight newHeight = getHeightForStance(newStance);
				player.movement.setSize(newHeight);

				onStanceUpdated?.Invoke();
			}
		}

		/// <summary>
		/// Replicate stance to clients.
		/// </summary>
		private void replicateStance(bool notifyOwner)
		{
			if (notifyOwner)
			{
				SendStance.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), stance);
			}
			else
			{
				SendStance.Invoke(GetNetId(), ENetReliability.Reliable, channel.GatherRemoteClientConnectionsExcludingOwner(), stance);
			}
		}

		public void checkStance(EPlayerStance newStance)
		{
			checkStance(newStance, false);
		}

		public void checkStance(EPlayerStance newStance, bool all)
		{
			if (player.movement.getVehicle() != null && newStance != EPlayerStance.DRIVING && newStance != EPlayerStance.SITTING)
			{
				return;
			}

			if (newStance == stance)
			{
				return;
			}

			if (newStance == EPlayerStance.PRONE || newStance == EPlayerStance.CROUCH)
			{
				if (_inShallows)
				{
					return;
				}
			}

			// Cooldown to prevent rapidly going prone/crouch. Be careful touching this - I accidentally got in trouble.
			if ((stance == EPlayerStance.CROUCH && newStance == EPlayerStance.STAND) ||
				(stance == EPlayerStance.PRONE && (newStance == EPlayerStance.CROUCH || newStance == EPlayerStance.STAND)))
			{
				if (Time.realtimeSinceStartup - lastStance > COOLDOWN)
				{
					lastStance = Time.realtimeSinceStartup;
				}
				else
				{
					return;
				}
			}

			// When entering vehicle we skip height query because player might be prone underneath something,
			// and when exiting the currentHeight and pendingHeight will both be standing.
			if (newStance != EPlayerStance.DRIVING && newStance != EPlayerStance.SITTING)
			{
				EPlayerHeight currentHeight = player.movement.height;
				EPlayerHeight pendingHeight = getHeightForStance(newStance);
				if (pendingHeight != currentHeight)
				{
					if (pendingHeight == EPlayerHeight.STAND)
					{
						// pendingHeight != currentHeight, so must be crouched or prone standing upward into something.
						if (!hasHeightClearance(PlayerMovement.HEIGHT_STAND))
						{
							return;
						}
					}
					else if (pendingHeight == EPlayerHeight.CROUCH && currentHeight == EPlayerHeight.PRONE)
					{
						if (!hasHeightClearance(PlayerMovement.HEIGHT_CROUCH))
						{
							return;
						}
					}
				}
			}

			if (Provider.isServer)
			{
				if (player.animator.gesture == EPlayerGesture.INVENTORY_START)
				{
					if (newStance != EPlayerStance.STAND && newStance != EPlayerStance.SPRINT && newStance != EPlayerStance.CROUCH)
					{
						player.animator.sendGesture(EPlayerGesture.INVENTORY_STOP, false);
					}
				}
				else if (player.animator.gesture == EPlayerGesture.SURRENDER_START)
				{
					player.animator.sendGesture(EPlayerGesture.SURRENDER_STOP, true);
				}
				else if (player.animator.gesture == EPlayerGesture.REST_START)
				{
					player.animator.sendGesture(EPlayerGesture.REST_STOP, true);
				}
				else if (player.animator.gesture == EPlayerGesture.T_POSE_START)
				{
					player.animator.sendGesture(EPlayerGesture.T_POSE_STOP, true);
				}
			}

			internalSetStance(newStance);

			if (Provider.isServer)
			{
				replicateStance(/*notifyOwner*/ all);
			}
		}

		public bool adjustStanceOrTeleportIfStuck()
		{
			EPlayerStance allowedStance = stance;
			if (getStanceForPosition(transform.position, ref allowedStance))
			{
				internalSetStance(allowedStance);
				replicateStance(/*notifyOwner*/ Dedicator.IsDedicatedServer);
				return true;
			}
			else
			{
				return player.teleportToRandomSpawnPoint();
			}
		}

		/// <summary>
		/// Regular interact ray still hits the ladder, but we only allow climbing within a smaller range to make its
		/// teleport less powerful.
		/// </summary>
		internal const float LADDER_INTERACT_RANGE = 4.0f;

		/// <summary>
		/// Ladder forward ray is 0.75m, so we move slightly less than that away from the ladder.
		/// </summary>
		internal const float LADDER_INTERACT_TELEPORT_OFFSET = 0.65f;

		internal static readonly ServerInstanceMethod<Vector3> SendClimbRequest = ServerInstanceMethod<Vector3>.Get(typeof(PlayerStance), nameof(ReceiveClimbRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2)]
		public void ReceiveClimbRequest(in ServerInvocationContext context, [NetPakNormal] Vector3 direction)
		{
			if (!canCurrentStanceTransitionToClimbing)
			{
				context.LogWarning($"current stance ({stance}) cannot transition to climbing");
				return;
			}

			if (!isAllowedToStartClimbing)
			{
				context.LogWarning("not allowed to start climbing");
				return;
			}

			// Sphere cast prevents climbing through a tiny gap.
			RaycastHit sphereHit;
			if (!Physics.SphereCast(new Ray(player.look.aim.position, direction), RADIUS, out sphereHit, LADDER_INTERACT_RANGE, RayMasks.LADDER_INTERACT) || sphereHit.collider == null)
			{
				context.LogWarning("sphere cast did not hit anything");
				return;
			}

			if (!sphereHit.collider.CompareTag("Ladder"))
			{
				context.LogWarning($"sphere cast did not hit ladder ({sphereHit.ToDebugString()})");
				return;
			}

			// We also do a raycast because sphere cast returns an adjusted normal.
			RaycastHit rayHit;
			if (!Physics.Raycast(new Ray(player.look.aim.position, direction), out rayHit, LADDER_INTERACT_RANGE, RayMasks.LADDER_INTERACT) || rayHit.collider == null)
			{
				context.LogWarning("ray did not hit anything");
				return;
			}

			if (!rayHit.collider.CompareTag("Ladder"))
			{
				context.LogWarning($"ray did not hit ladder ({rayHit.ToDebugString()})");
				return;
			}

			float forwardAlignment = Vector3.Dot(rayHit.normal, rayHit.collider.transform.up);
			if (Mathf.Abs(forwardAlignment) <= 0.9f)
			{
				// Ladder movement code does this check as well to see if we hit the front/back of the ladder.
				context.LogWarning("ray did not hit front/back of ladder");
				return;
			}

			// Prevent climbing angled ladders. Only "mostly" up/down ladders.
			float worldUpAlignment = Vector3.Dot(Vector3.up, rayHit.collider.transform.up);
			if (Mathf.Abs(worldUpAlignment) > 0.1f)
			{
				context.LogWarning("ladder is too sloped");
				return;
			}

			// Teleport adds vertical offset, but ladder checks cast from 0.5m above feet, and we also check downward a bit extra.
			Vector3 climbPoint = new Vector3(rayHit.collider.transform.position.x, rayHit.point.y - Player.TELEPORT_VERTICAL_OFFSET - 0.5f - 0.1f, rayHit.collider.transform.position.z) + (rayHit.normal * LADDER_INTERACT_TELEPORT_OFFSET);
			float testHeight = PlayerMovement.HEIGHT_STAND + 0.1f + Player.TELEPORT_VERTICAL_OFFSET;

			// Test first hit has line-of-sight to center of climbing capsule, otherwise
			// player may have angled ladder to place capsule on the other side of a thin wall.
			RaycastHit losHit;
			Vector3 losCenter = climbPoint + new Vector3(0.0f, testHeight * 0.5f, 0.0f);
			if (Physics.Linecast(rayHit.point, losCenter, out losHit, RayMasks.BLOCK_STANCE, QueryTriggerInteraction.Collide))
			{
				context.LogWarning($"line-of-sight ray hit {losHit.ToDebugString()}");
				return;
			}

			if (!hasHeightClearanceAtPosition(climbPoint, testHeight))
			{
				context.LogWarning("insufficient clearance");
				return;
			}

			float climbYaw = rayHit.collider.transform.rotation.eulerAngles.y;
			if (forwardAlignment < 0.0f)
			{
				climbYaw += 180.0f;
			}

			if (!player.teleportToLocation(climbPoint, climbYaw))
			{
				context.LogWarning("teleport failed");
			}
		}

		[System.Obsolete]
		public void tellStance(CSteamID steamID, byte newStance)
		{
			ReceiveStance((EPlayerStance) newStance);
		}

		private static readonly ClientInstanceMethod<EPlayerStance> SendStance = ClientInstanceMethod<EPlayerStance>.Get(typeof(PlayerStance), nameof(ReceiveStance));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellStance))]
		public void ReceiveStance(EPlayerStance newStance)
		{
			internalSetStance(newStance);

			// For players with toggle-mode stance changes this prevents an immediate switch.
			_localWantsToCrouch = newStance == EPlayerStance.CROUCH;
			_localWantsToProne = newStance == EPlayerStance.PRONE;

			OnStanceChanged_Global?.Invoke(this);
		}

		[System.Obsolete]
		public void askStance(CSteamID steamID)
		{ }

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			SendStance.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, stance);
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			SendStance.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, stance);
		}

		public void simulate(uint simulation, bool inputCrouch, bool inputProne, bool inputSprint)
		{
			_isSubmerged = areEyesUnderwater;
			_inShallows = areFeetUnderwater;

			// If revising ladder climbing rules please adjust InteractableLadder accordingly!
			if (stance == EPlayerStance.CLIMB || (canCurrentStanceTransitionToClimbing && isAllowedToStartClimbing))
			{
				Ray ray = new Ray(transform.position + (Vector3.up * 0.5f), transform.forward);
				Physics.Raycast(ray, out ladder, 0.75f, RayMasks.LADDER_INTERACT);
#if DRAW_LADDER_GIZMOS
				RuntimeGizmos.Get().Raycast(ray, 0.75f, ladder, Color.green, Color.red, lifespan: PlayerInput.RATE);
#endif // DRAW_LADDER_GIZMOS
				if (ladder.collider != null && ladder.collider.transform.CompareTag("Ladder") && Mathf.Abs(Vector3.Dot(ladder.normal, ladder.collider.transform.up)) > 0.9f)
				{
					if (stance != EPlayerStance.CLIMB)
					{
						Vector3 climbPoint = new Vector3(ladder.collider.transform.position.x, ladder.point.y - 0.5f, ladder.collider.transform.position.z) + (ladder.normal * 0.5f);

						Vector3 capsulePoint0 = transform.position + new Vector3(0.0f, PlayerStance.RADIUS, 0.0f);
						Vector3 capsulePoint1 = transform.position + new Vector3(0.0f, PlayerMovement.HEIGHT_STAND - PlayerStance.RADIUS, 0.0f);
						Vector3 capsuleCastOffset = (climbPoint - transform.position);
						Vector3 capsuleCastDirection = capsuleCastOffset.normalized;
						float capsuleCastDistance = capsuleCastOffset.magnitude;
						if (!Physics.CapsuleCast(capsulePoint0, capsulePoint1, RADIUS, capsuleCastDirection, capsuleCastDistance, RayMasks.BLOCK_LADDER, QueryTriggerInteraction.Ignore))
						{
							// CapsuleCast will not detect colliders we were already overlapping, so we also check
							// that there are no colliders at the destination. Otherwise there were some exploits
							// where players would exit a vehicle through a small gap into a barricade mounted to a ladder.
							Vector3 destCapsulePoint0 = climbPoint + new Vector3(0, RADIUS, 0);
							Vector3 destCapsulePoint1 = climbPoint + new Vector3(0, PlayerMovement.HEIGHT_STAND - RADIUS, 0);
							if (!Physics.CheckCapsule(destCapsulePoint0, destCapsulePoint1, RADIUS, RayMasks.BLOCK_LADDER, QueryTriggerInteraction.Ignore))
							{
								transform.position = climbPoint;

								checkStance(EPlayerStance.CLIMB);
							}
#if DRAW_LADDER_GIZMOS
							else
							{
								RuntimeGizmos.Get().Capsule(destCapsulePoint0, destCapsulePoint1, RADIUS, Color.red, lifespan: PlayerInput.RATE);
							}
#endif // DRAW_LADDER_GIZMOS
						}
#if DRAW_LADDER_GIZMOS
						else
						{
							RuntimeGizmos.Get().Capsule(capsulePoint0 + capsuleCastOffset, capsulePoint1 + capsuleCastOffset, RADIUS, Color.yellow, lifespan: PlayerInput.RATE);
						}
#endif // DRAW_LADDER_GIZMOS
					}

					if (stance == EPlayerStance.CLIMB)
					{
						return;
					}
				}
				else
				{
					if (stance == EPlayerStance.CLIMB)
					{
						checkStance(EPlayerStance.STAND);
					}
				}
			}

			if (isBodyUnderwater)
			{
				if (stance != EPlayerStance.SWIM)
				{
					checkStance(EPlayerStance.SWIM);

#if !DEDICATED_SERVER
					// Only play sound if stance actually changed to swim. (e.g. cannot change stances in car)
					if (stance == EPlayerStance.SWIM && !player.input.isResimulating)
					{
						// Hack to prevent silent swimming. Players could angle themselves to rapidly switch
						// between swimming and walking stances entering and exiting the water, so now we
						// play a sound when they change states.
						if (!Dedicator.IsDedicatedServer && Time.time - lastSubmergeSound > 0.1f)
						{
							lastSubmergeSound = Time.time;
							player.movement.PlaySwimAudioClip();
						}
					}
#endif // !DEDICATED_SERVER
				}

				return;
			}
			else if (_inShallows)
			{
				if (stance != EPlayerStance.STAND && stance != EPlayerStance.SPRINT)
				{
					checkStance(EPlayerStance.STAND);
				}
			}
			else
			{
				if (stance == EPlayerStance.SWIM)
				{
					checkStance(EPlayerStance.STAND);
				}
			}

			if (stance != EPlayerStance.CLIMB && stance != EPlayerStance.SITTING && stance != EPlayerStance.DRIVING)
			{
				// Players with crouch input set to hold mode (rather than toggle) should stay crouched while in rest gesture. 
				if (inputCrouch || (player.animator.gesture == EPlayerGesture.REST_START && !inputProne))
				{
					if (stance != EPlayerStance.CROUCH)
					{
						checkStance(EPlayerStance.CROUCH);
					}
				}
				else if (inputProne)
				{
					if (stance != EPlayerStance.PRONE)
					{
						checkStance(EPlayerStance.PRONE);
					}
				}
				// Moved into this branch so that crouch->prone or prone->crouch do not "pass through" standing.
				else if (stance == EPlayerStance.CROUCH || stance == EPlayerStance.PRONE)
				{
					checkStance(EPlayerStance.STAND);
				}

				bool doesEquipmentAllowSprinting = true;
				if (player.equipment.useable is UseableGun gun && player.equipment.asset is ItemGunAsset gunAsset)
				{
					doesEquipmentAllowSprinting = gunAsset.canAimDuringSprint || !gun.isAiming;
				}
				if (inputSprint && !player.life.isBroken && player.life.stamina > 0 && doesEquipmentAllowSprinting && player.movement.isMoving)
				{
					if (stance == EPlayerStance.STAND)
					{
						checkStance(EPlayerStance.SPRINT);
					}
				}
				else if (stance == EPlayerStance.SPRINT)
				{
					checkStance(EPlayerStance.STAND);
				}
			}
		}

		private void onLifeUpdated(bool isDead)
		{
			if (!isDead)
			{
				checkStance(EPlayerStance.STAND);
			}
		}

		private void Update()
		{
			if (channel.IsLocalPlayer && !PlayerUI.window.showCursor)
			{
				if (!player.look.IsControllingFreecam)
				{
					if (InputEx.GetKey(ControlsSettings.stance))
					{
						if (isHolding)
						{
							if (Time.realtimeSinceStartup - lastHold > 0.33f)
							{
								_localWantsToCrouch = false;
								_localWantsToProne = true;
							}
						}
						else
						{
							isHolding = true;
							lastHold = Time.realtimeSinceStartup;
						}
					}
					else
					{
						if (isHolding)
						{
							if (Time.realtimeSinceStartup - lastHold < 0.33f)
							{
								if (crouch)
								{
									_localWantsToCrouch = false;
									_localWantsToProne = false;
								}
								else
								{
									_localWantsToCrouch = true;
									_localWantsToProne = false;
								}
							}

							isHolding = false;
						}
					}

					if (player.animator.gesture == EPlayerGesture.REST_START
						|| player.animator.gesture == EPlayerGesture.T_POSE_START)
					{
						bool exitGesture = false;

						if (InputEx.GetKeyDown(ControlsSettings.crouch))
						{
							exitGesture = true;

							_localWantsToCrouch = true;
							_localWantsToProne = false;
						}
						else if (InputEx.GetKeyDown(ControlsSettings.prone))
						{
							exitGesture = true;

							_localWantsToCrouch = false;
							_localWantsToProne = true;
						}

						if (exitGesture)
						{
							if (player.animator.gesture == EPlayerGesture.REST_START)
							{
								player.animator.sendGesture(EPlayerGesture.REST_STOP, true);
							}
							else
							{
								player.animator.sendGesture(EPlayerGesture.T_POSE_STOP, true);
							}
						}
					}
					else
					{
						if (ControlsSettings.crouching == EControlMode.TOGGLE)
						{
							if (InputEx.GetKeyDown(ControlsSettings.crouch))
							{
								_localWantsToCrouch = !crouch;
								if (_localWantsToCrouch)
								{
									_localWantsToProne = false;
								}
							}
						}
						else
						{
							_localWantsToCrouch = InputEx.GetKey(ControlsSettings.crouch);
							if (_localWantsToCrouch)
							{
								_localWantsToProne = false;
							}
						}

						if (ControlsSettings.proning == EControlMode.TOGGLE)
						{
							if (InputEx.GetKeyDown(ControlsSettings.prone))
							{
								_localWantsToProne = !prone;
								if (_localWantsToProne)
								{
									_localWantsToCrouch = false;
								}
							}
						}
						else
						{
							_localWantsToProne = InputEx.GetKey(ControlsSettings.prone);
							if (_localWantsToProne)
							{
								_localWantsToCrouch = false;
							}
						}
					}

					if (ControlsSettings.sprinting == EControlMode.TOGGLE)
					{
						if (InputEx.GetKeyDown(ControlsSettings.sprint))
						{
							_localWantsToSprint = !sprint;
						}
					}
					else
					{
						_localWantsToSprint = InputEx.GetKey(ControlsSettings.sprint);
					}

					localWantsToSteadyAim = InputEx.GetKey(ControlsSettings.sprint);
				}

				if ((stance == EPlayerStance.PRONE || stance == EPlayerStance.CROUCH) && InputEx.GetKey(ControlsSettings.jump))
				{
					_localWantsToCrouch = false;
					_localWantsToProne = false;
				}

				if (_inShallows || stance == EPlayerStance.SWIM || stance == EPlayerStance.CLIMB || stance == EPlayerStance.SITTING || stance == EPlayerStance.DRIVING)
				{
					_localWantsToCrouch = false;
					_localWantsToProne = false;
				}

				if (ControlsSettings.sprinting == EControlMode.TOGGLE && player.movement.input_x == 0 && player.movement.input_y == 0)
				{
					_localWantsToSprint = false;
				}

				if (PlayerUI.window.showCursor)
				{
					_localWantsToSprint = false;
					localWantsToSteadyAim = false;
				}
			}

			if (Provider.isServer && Time.realtimeSinceStartup - lastDetect > 0.1)
			{
				lastDetect = Time.realtimeSinceStartup;

				if (player.life.IsAlive)
				{
					AlertTool.alert(player, transform.position, GetStealthDetectionRadius(), stance != EPlayerStance.SPRINT && stance != EPlayerStance.DRIVING, player.look.aim.forward, player.isSpotOn);
				}
			}
		}

		internal void InitializePlayer()
		{
			_stance = EPlayerStance.STAND;

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				lastStance = 0.0f;
#if !DEDICATED_SERVER
				lastSubmergeSound = 0;
#endif // !DEDICATED_SERVER

				player.life.onLifeUpdated += onLifeUpdated;
			}

			if (Provider.isServer)
			{
				internalSetStance(initialStance);
			}
		}
	}
}
