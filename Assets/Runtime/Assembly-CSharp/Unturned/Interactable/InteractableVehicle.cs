////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define DRAW_EXIT_GIZMOS
// #define LOG_GEAR_SHIFT
// #define DRAW_BICYCLE_GIZMOS
// #define ENABLE_VEHICLE_PROFILING
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using Steamworks;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_VEHICLE_PROFILING
using UnityEngine.Profiling;
#endif
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public delegate void VehiclePassengersUpdated();
	public delegate void VehicleLockUpdated();
	public delegate void VehicleHeadlightsUpdated();
	public delegate void VehicleTaillightsUpdated();
	public delegate void VehicleSirensUpdated();
	public delegate void VehicleBlimpUpdated();
	public delegate void VehicleBatteryChangedHandler();
	public delegate void VehicleSkinChangedHandler();
	public delegate void HookVehicleRequestHandler(InteractableVehicle instigatingVehicle, InteractableVehicle targetVehicle, ref bool shouldAllow);

	public class PropellerModel
	{
		public Transform transform;
		/// <summary>
		/// Material on Model_0, the low-speed actual blade.
		/// </summary>
		public Material bladeMaterial;
		/// <summary>
		/// Renderer on Model_1.
		/// </summary>
		public Renderer motionBlurRenderer;
		/// <summary>
		/// Material on Model_1, the high-speed blurred outline.
		/// </summary>
		public Material motionBlurMaterial;
		/// <summary>
		/// transform's localRotation when the vehicle was instantiated.
		/// </summary>
		public Quaternion baseLocationRotation;
	}

	public class HookInfo
	{
		[System.Obsolete("This is vehicle's root transform. Will be removed in a future release.")]
		public Transform target;
		public InteractableVehicle vehicle;
		public Vector3 deltaPosition;
		public Quaternion deltaRotation;
	}

	[System.Obsolete("Replaced by MarkForReplicationUpdate. Will be removed in a future release.")]
	public struct VehicleStateUpdate
	{
		public Vector3 pos;
		public Quaternion rot;

		public VehicleStateUpdate(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}
	}

	public class TrainCar
	{
		public float trackPositionOffset;

		public Vector3 currentFrontPosition;
		public Vector3 currentFrontNormal;
		public Vector3 currentFrontDirection;

		public Vector3 currentBackPosition;
		public Vector3 currentBackNormal;
		public Vector3 currentBackDirection;

		public Transform root;
		public Transform trackFront;
		public Transform trackBack;

		/// <summary>
		/// Objects transform relative to Root.
		/// Identity in vanilla, but may be otherwise in mods.
		/// Necessary for fix interpolation issue with front/back transform.
		/// </summary>
		public Matrix4x4 objectsToRoot;

		/// <summary>
		/// Rigidbody component on the root game object.
		/// </summary>
		public Rigidbody rootRigidbody;
	}

	internal struct VehicleSkinMaterialChange
	{
		public Renderer renderer;
		public Material originalMaterial;
		/// <summary>
		/// If true, set sharedMaterial. If false, set material.
		/// </summary>
		public bool shared;
	}

	internal class CrawlerTrackTilingMaterialInstance
	{
		public Material material;
		public Wheel[] wheels;
		public Vector2 initialUvPosition;
		public float uvOffset;
		public float repeatDistance;
		public Vector2 uvDirection;
		/// <summary>
		/// Calculated speed of this track. Used by some wheels.
		/// </summary>
		public float speed;
	}

	public class InteractableVehicle : Interactable, IExplosionDamageable, ICraftingTagProvider, IOwnershipInfo
	{
		/// <summary>
		/// Temporary array for use with physics queries.
		/// </summary>
		private static Collider[] tempCollidersArray = new Collider[4];
		/// <summary>
		/// Temporary list for gathering materials.
		/// </summary>
		private static List<Material> tempMaterialsList = new List<Material>();
		private static List<Wheel> tempWheels = new List<Wheel>();
		private const float EXPLODE = 4;

		private const ushort SMOKE_0_HEALTH_THRESHOLD = 100;
		private const ushort SMOKE_1_HEALTH_THRESHOLD = 200;

		public event VehiclePassengersUpdated onPassengersUpdated;
		public event VehicleLockUpdated onLockUpdated;
		public event VehicleHeadlightsUpdated onHeadlightsUpdated;
		public event VehicleTaillightsUpdated onTaillightsUpdated;
		public event VehicleSirensUpdated onSirensUpdated;
		public event VehicleBlimpUpdated onBlimpUpdated;
		public event VehicleBatteryChangedHandler batteryChanged;
		public event VehicleSkinChangedHandler skinChanged;
		public event System.Action<InteractableVehicle> OnGearChanged;
		public event System.Action<InteractableVehicle> OnHealthChanged;

		public static event System.Action<InteractableVehicle> OnHealthChanged_Global;
		public static event System.Action<InteractableVehicle> OnLockChanged_Global;
		public static event System.Action<InteractableVehicle> OnFuelChanged_Global;
		public static event System.Action<InteractableVehicle> OnBatteryLevelChanged_Global;
		public static event System.Action<InteractableVehicle, int> OnPassengerAdded_Global;
		public static event System.Action<InteractableVehicle, int, int> OnPassengerChangedSeats_Global;
		public static event System.Action<InteractableVehicle, int, Player> OnPassengerRemoved_Global;
		public static event HookVehicleRequestHandler OnHookVehicleRequested_Global;

		/// <summary>
		/// Precursor to Net ID. Should eventually become obsolete.
		/// </summary>
		public uint instanceID;

		/// <summary>
		/// Asset ID. Essentially obsolete at this point.
		/// </summary>
		public ushort id;

		public Items trunkItems;

		public ushort skinID;
		public ushort mythicID;
		protected SkinAsset skinAsset;
		private List<Mesh> tempMesh;
		/// <summary>
		/// Used to restore vehicle materials when changing skin.
		/// </summary>
		private List<VehicleSkinMaterialChange> skinOriginalMaterials;
		protected Transform effectSlotsRoot;
		protected Transform[] effectSlots;
		protected MythicalEffectController[] effectSystems;

		/// <summary>
		/// Only used by trains. Constrains the train to this path.
		/// </summary>
		public ushort roadIndex;
		public float roadPosition;

		/// <summary>
		/// Unfortunately old netcode sends train position as a Vector3 using the X channel, but new code only supports
		/// [-4096, 4096) so we pack the train position into all three channels. Eventually this should be cleaned up.
		/// </summary>
		internal static Vector3 PackRoadPosition(float roadPosition)
		{
			if (roadPosition >= 16384.0f)
			{
				return new Vector3(4096.0f, 4096.0f, roadPosition - 20480.0f);
			}
			else if (roadPosition >= 8192.0f)
			{
				return new Vector3(4096.0f, roadPosition - 12288.0f, -4096.0f);
			}
			else
			{
				return new Vector3(roadPosition - 4096.0f, -4096.0f, -4096.0f);
			}
		}

		internal static float UnpackRoadPosition(Vector3 roadPosition)
		{
			return roadPosition.x + roadPosition.y + roadPosition.z + 12288.0f;
		}

		public Road road
		{
			get;
			protected set;
		}

		public ushort fuel;
		public ushort health;
		/// <summary>
		/// Nelson 2024-06-24: When first implementing batteries there was only the vanilla battery item, and it was
		/// fine to delete it when the charge reached zero. This may not be desirable, however, so zero now represents
		/// no battery item is present, and one represents the battery is completely drained but still there.
		/// </summary>
		public ushort batteryCharge;
		internal System.Guid batteryItemGuid;
		public Color32 PaintColor
		{
			get;
			internal set;
		}

		/// <summary>
		/// Is this vehicle inside a safezone?
		/// </summary>
		public bool isInsideSafezone
		{
			get;
			protected set;
		}

		public SafezoneNode insideSafezoneNode
		{
			get;
			protected set;
		}

		/// <summary>
		/// Duration in seconds since this vehicle entered a safezone,
		/// or -1 if it's not in a safezone.
		/// </summary>
		public float timeInsideSafezone
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should askDamage requests currently be ignored because we are inside a safezone?
		/// </summary>
		public bool isInsideNoDamageZone => insideSafezoneNode != null && insideSafezoneNode.noIncomingDamage;

		public bool usesFuel => !(asset.isStaminaPowered || asset.isBatteryPowered);

		public bool usesBattery => !asset.isStaminaPowered || asset.isBatteryPowered;

		public bool usesHealth => asset.engine != EEngine.TRAIN;

		public bool isBoosting
		{
			get;
			protected set;
		}

		private float horned;

		/// <summary>
		/// Nelson 2024-06-24: This property is confusing, especially with isEnginePowered, but essentially represents
		/// starting the engine ignition when a player enters the driver's seat. If true, there's a driver, there was
		/// sufficient battery to start (or battery not required), and the engine wasn't underwater.
		/// </summary>
		public bool isEngineOn
		{
			get;
			protected set;
		}

		public bool isEnginePowered
		{
			get
			{
				if (asset.isStaminaPowered)
					return true;

				if (asset.isBatteryPowered)
					return HasBatteryWithCharge;

				return fuel > 0 && isEngineOn;
			}
		}

		/// <summary>
		/// Doesn't imply the vehicle *uses* batteries, only that it contains a battery item with some charge left.
		/// </summary>
		public bool HasBatteryWithCharge
		{
			get => batteryCharge > 1;
		}

		/// <summary>
		/// Doesn't imply the vehicle *uses* batteries, only that it contains a (potentially uncharged) battery item.
		/// </summary>
		public bool ContainsBatteryItem
		{
			get => batteryCharge > 0;
		}

		public bool isBatteryFull
		{
			get
			{
				if (usesBattery)
				{
					return batteryCharge >= 10000;
				}
				else
				{
					return true;
				}
			}
		}

		internal bool _wasNaturallySpawned;
		/// <summary>
		/// Nelson 2024-11-13: Adding this primarily to indicate whether a vehicle was spawned by the level versus
		/// placed by a player or bought from a vendor. This way if the number of "naturally"-spawned vehicles is below
		/// a certain threshold the level can spawn more. (e.g., a server where players have hoarded a bunch of
		/// vendor-purchased vehicles and no default vehicles are left for new players.)
		///
		/// Only available on the server.
		/// Defaults to true for old saves to prevent suddenly spawning a bunch more vehicles.
		/// </summary>
		public bool WasNaturallySpawned
		{
			get => _wasNaturallySpawned;
			set => _wasNaturallySpawned = value;
		}

		protected VehicleEventHook eventHook;
		private CraftingTagProviderComponent craftingTagProviderModHook;

		public bool canUseHorn => Time.realtimeSinceStartup - horned > 0.5f && (!usesBattery || HasBatteryWithCharge);

		/// <summary>
		/// Whether the player can shoot their equipped turret.
		/// </summary>
		public bool canUseTurret => !isDead;

		public bool canTurnOnLights => (!usesBattery || HasBatteryWithCharge) && !isUnderwater;

		public bool isRefillable => usesFuel && fuel < asset.fuel && !isDriven && !isExploded;

		public bool isSiphonable => usesFuel && fuel > 0 && !isDriven && !isExploded;

		public bool isRepaired => health == asset.health;

		public bool isDriven => passengers != null && passengers[0].player != null;

		/// <summary>
		/// Do any of the passenger seats have a player?
		/// </summary>
		public bool anySeatsOccupied
		{
			get
			{
				if (passengers != null)
				{
					foreach (Passenger seat in passengers)
					{
						if (seat.player != null)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		public bool isDriver => !Dedicator.IsDedicatedServer && checkDriver(Provider.client);

		public bool isEmpty
		{
			get
			{
				for (byte index = 0; index < passengers.Length; index++)
				{
					if (passengers[index].player != null)
					{
						return false;
					}
				}

				return true;
			}
		}

		private bool _isDrowned;
		public bool isDrowned => _isDrowned;

		public event System.Action OnIsDrownedChanged;

		public bool isUnderwater
		{
			get
			{
				BeginSample("IsUnderwater");
				bool result;
				if (waterCenterTransform != null)
				{
					result = SDG.Framework.Water.WaterUtility.isPointUnderwater(waterCenterTransform.position);
				}
				else
				{
					result = SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position + new Vector3(0, 1, 0));
				}
				EndSample();
				return result;
			}
		}

		public bool isBatteryReplaceable => usesBattery && !isBatteryFull && !isDriven && !isExploded;

		public bool isTireReplaceable => !isDriven && !isExploded && asset.canTiresBeDamaged;

		public bool canBeDamaged => asset.engine != EEngine.TRAIN;

		public bool isGoingToRespawn => isExploded || isDrowned;

		/// <summary>
		/// When the server saves it doesn't include any cleared vehicles.
		/// </summary>
		public bool isAutoClearable
		{
			get
			{
				if (isExploded)
				{
					return true;
				}

				if (isUnderwater && buoyancy == null)
				{
					return true;
				}

				if (asset != null)
				{
					// Nelson 2024-07-22: Battery-powered boats were getting deleted! (public issue #4597)
					if ((asset.engine == EEngine.BOAT && fuel == 0) && !asset.isBatteryPowered)
					{
						return true;
					}

					if (asset.engine == EEngine.TRAIN)
					{
						return false;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// If true, the vehicle will be destroyed at the end of the frame. Set before OnPreDestroyVehicle.
		/// Used to reject requests to enter the vehicle on the same frame it's being destroyed.
		/// </summary>
		public bool IsPendingDestroy
		{
			get;
			internal set;
		}

		private float _lastDead;
		public float lastDead => _lastDead;

		private float _lastUnderwater;
		public float lastUnderwater => _lastUnderwater;

		private float _lastExploded;
		public float lastExploded => _lastExploded;

		private float _slip;
		public float slip => _slip;

		public bool isExploded;

		public bool isDead => health == 0;

		/// <summary>
		/// Magnitude of rigidbody velocity, replicated by current simulation owner.
		/// </summary>
		public float ReplicatedSpeed
		{
			get;
			private set;
		}

		/// <summary>
		/// Rigidbody velocity along forward axis, replicated by current simulation owner.
		/// </summary>
		public float ReplicatedForwardVelocity
		{
			get;
			private set;
		}

		/// <summary>
		/// Replicated by current simulation owner. Target velocity used, e.g., for helicopter engine speed.
		/// </summary>
		public float ReplicatedVelocityInput
		{
			get;
			private set;
		}

		/// <summary>
		/// [0, 1] If forward velocity is greater than zero, get normalized by target forward speed. If less than zero,
		/// get normalized by target reverse speed. Result is always positive.
		/// </summary>
		public float GetReplicatedForwardSpeedPercentageOfTargetSpeed()
		{
			if (ReplicatedForwardVelocity > 0.0f)
			{
				return Mathf.Clamp01(ReplicatedForwardVelocity / asset.TargetForwardVelocity);
			}
			else
			{
				return Mathf.Clamp01(ReplicatedForwardVelocity / asset.TargetReverseVelocity);
			}
		}

		public float GetAnimatedForwardSpeedPercentageOfTargetSpeed()
		{
			if (AnimatedForwardVelocity > 0.0f)
			{
				return Mathf.Clamp01(AnimatedForwardVelocity / asset.TargetForwardVelocity);
			}
			else
			{
				return Mathf.Clamp01(AnimatedForwardVelocity / asset.TargetReverseVelocity);
			}
		}

		/// <summary>
		/// Animated toward ReplicatedForwardVelocity.
		/// </summary>
		public float AnimatedForwardVelocity
		{
			get;
			private set;
		}

		/// <summary>
		/// Animated toward ReplicatedVelocityInput.
		/// </summary>
		public float AnimatedVelocityInput
		{
			get;
			private set;
		}

		/// <summary>
		/// [-1.0, 1.0] Available on both client and server.
		/// </summary>
		public float ReplicatedSteeringInput
		{
			get;
			private set;
		}

		/// <summary>
		/// Animated towards replicated steering angle. Used for steering wheel and front steering column.
		/// Non-simulating wheels (e.g., car driven by remote client) use this as steering angle multiplied by their
		/// per-wheel <see cref="VehicleWheelConfiguration.steeringAngleMultiplier"/>.
		/// </summary>
		public float AnimatedSteeringAngle
		{
			get;
			private set;
		}

		private float propellerRotationDegrees;

		private PropellerModel[] propellerModels;
		private bool isPropellerMotionBlurEnabled;
		private GameObject exhaustGameObject;
		private bool isExhaustGameObjectActive;
		private bool isExhaustRateOverTimeZero;
		private ParticleSystem[] exhaustParticleSystems;
		private Transform steeringWheelModelTransform;
		public TrainCar[] trainCars
		{
			get;
			protected set;
		}
		private Transform overlapFront;
		private Transform overlapBack;
		private Transform pedalLeft;
		private Transform pedalRight;
		/// <summary>
		/// Front steering column of bicycles and motorcycles.
		/// </summary>
		private Transform frontModelTransform;
		private Quaternion steeringWheelRestLocalRotation;
		private Quaternion frontModelRestLocalRotation;
		private Transform waterCenterTransform;

		private Transform fire;
		private Transform smoke_0;
		private Transform smoke_1;

		[System.Obsolete("Replaced by MarkForReplicationUpdate. Will be removed in a future release.")]
		public List<VehicleStateUpdate> updates;

		/// <summary>
		/// If true, server should replicate latest state to clients.
		/// </summary>
		internal bool needsReplicationUpdate;

		private Material[] sirenMaterials;

		private bool sirenState;
		private List<GameObject> sirenGameObjects = new List<GameObject>();
		private List<GameObject> sirenGameObjects_0 = new List<GameObject>();
		private List<GameObject> sirenGameObjects_1 = new List<GameObject>();

		private bool _sirensOn;
		public bool sirensOn => _sirensOn;

		private Transform _headlights;
		public Transform headlights => _headlights;

		private Material headlightsMaterial;

		private bool _headlightsOn;
		public bool headlightsOn => _headlightsOn;

		private Transform _taillights;
		public Transform taillights => _taillights;

		private Material taillightsMaterial;
		private Material[] taillightMaterials;

		private bool _taillightsOn;
		public bool taillightsOn => _taillightsOn;

		private CSteamID _lockedOwner;
		public CSteamID lockedOwner => _lockedOwner;

		private CSteamID _lockedGroup;
		public CSteamID lockedGroup => _lockedGroup;

		private bool _isLocked;
		public bool isLocked => _isLocked;

		public bool isSkinned => skinID != 0;

		private VehicleAsset _asset;
		public VehicleAsset asset => _asset;

		public float lastSeat;
		private Passenger[] _passengers;
		public Passenger[] passengers => _passengers;
		private Passenger[] _turrets;
		public Passenger[] turrets => _turrets;

		internal Wheel[] _wheels;
		public Wheel[] tires
		{
			get => _wheels;
		}

		internal Wheel GetWheelAtIndex(int index)
		{
			if (_wheels != null && index >= 0 && index < _wheels.Length)
			{
				return _wheels[index];
			}
			else
			{
				return null;
			}
		}

		public bool isHooked;

		private Transform buoyancy;
		private Transform hook;
		private List<HookInfo> hooked;

		private Vector3 lastUpdatedPos;
		private Vector3 interpTargetPosition;
		private Quaternion interpTargetRotation;

		private Vector3 real;
		private float lastTick;

		private float lastWeeoo;

		private AudioSource clipAudioSource;
		private WindZone windZone;

		private bool isRecovering;
		private float lastRecover;

		private bool usesGravity => asset.engine != EEngine.TRAIN;

		private bool isKinematic => !usesGravity;

		private bool isPhysical;
		private bool isFrozen;

		public bool isBlimpFloating;
		/// <summary>
		/// Used by several engine modes to represent an interpolated velocity target according to input.
		/// </summary>
		private float inputTargetVelocity;
		/// <summary>
		/// Set from inputTargetVelocity then multiplied by any factors which shouldn't affect the player's "target"
		/// speed ike boatTraction.
		/// </summary>
		private float inputEngineVelocity;
		/// <summary>
		/// Vehicles with buoyancy interpolate this value according to whether it's in the water, and multiply
		/// boat-related forces by it.
		/// </summary>
		private float boatTraction;
		private float batteryBuffer;
		private float fuelBurnBuffer;

		/// <summary>
		/// Rigidbody on the Vehicle prefab.
		/// (not called "rigidbody" because as of 2024-02-28 the deprecated "rigidbody" property still exists)
		/// </summary>
		private Rigidbody rootRigidbody;

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get
			{
				if (isDead)
				{
					return false;
				}

				if (asset == null || !asset.isVulnerableToExplosions)
				{
					return false;
				}

				return true;
			}
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return getClosestPointOnHull(explosionCenter);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (!damageParameters.shouldAffectVehicles)
			{
				return;
			}

			Vector3 offset = damageParameters.closestPoint - explosionParameters.point;
			float range = offset.magnitude;
			if (range > explosionParameters.damageRadius)
			{
				return;
			}

			float damageMultiplier = 1.0f - (range / explosionParameters.damageRadius);

			Vector3 normal = offset / range;
			if (damageParameters.LineOfSightTest(explosionParameters.point, normal, range, out RaycastHit block))
			{
				if (block.transform != null)
				{
					// obstructionMask ignores vehicles, so we don't have to worry about hitting vehicle colliders.
					bool isChild = block.transform.IsChildOf(transform);
					if (!isChild)
						return;

					damageMultiplier *= asset.childExplosionArmorMultiplier;
					damageMultiplier *= Provider.modeConfigData.Vehicles.Child_Explosion_Armor_Multiplier;
				}
			}

			VehicleManager.damage(this, explosionParameters.vehicleDamage, damageMultiplier, false,
				instigatorSteamID: explosionParameters.killer, damageOrigin: explosionParameters.damageOrigin);
		}
		#endregion IExplosionDamageable

		#region ICraftingTagProvider
		public Asset GetTagProviderAsset()
		{
			return asset;
		}

		public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
		{
			if (craftingTagProviderModHook != null)
			{
				p.ApplyModHooks(craftingTagProviderModHook);
			}
		}

		public bool HasAnyCraftingTagsConfigured()
		{
			return craftingTagProviderModHook != null;
		}

		public bool Equals(ICraftingTagProvider obj)
		{
			return ReferenceEquals(this, obj);
		}
		#endregion ICraftingTagProvider

		#region IOwnershipInfo
		public bool TryGetOwnership(out ulong ownerUser, out ulong ownerGroup)
		{
			ownerUser = isLocked ? lockedOwner.m_SteamID : 0;
			ownerGroup = isLocked ? lockedGroup.m_SteamID : 0;
			return true;
		}
		#endregion IOwnershipInfo

		/// <summary>
		/// Primarily for backwards compatibility with plugins. Previously, multiple "updates" could be queued per
		/// vehicle and sent to clients. This list was public, unfortunately, so plugins may rely on submitting vehicle
		/// updates. After making it obsolete each vehicle can only be flagged as needing a replication update, and
		/// this is reset after each server replication update.
		/// </summary>
		public void MarkForReplicationUpdate()
		{
			if (!needsReplicationUpdate)
			{
				needsReplicationUpdate = true;
				VehicleManager.instance.vehiclesNeedingReplicationUpdate.Add(this);
			}
		}

		public void ResetDecayTimer()
		{
			decayTimer = 0.0f;
			decayPendingDamage = 0.0f;
			decayLastUpdatePosition = transform.position;
		}

		/// <summary>
		/// Is player currently allowed to repair this vehicle?
		/// </summary>
		public bool canPlayerRepair(Player player)
		{
			return asset.canRepairWhileSeated || player.movement.getVehicle() != this;
		}

		public void replaceBattery(Player player, byte quality, System.Guid newBatteryItemGuid)
		{
			if (ContainsBatteryItem)
			{
				GiveBatteryItem(player);
			}
			batteryItemGuid = newBatteryItemGuid;
			// Clamp to at least 1 because 0 represents no battery item.
			int newCharge = Mathf.Clamp(Mathf.RoundToInt(quality * 100), 1, 10000);
			VehicleManager.sendVehicleBatteryCharge(this, (ushort) newCharge);
			ResetDecayTimer();
		}

		/// <summary>
		/// Give battery item to player and set battery charge to zero.
		/// </summary>
		public void stealBattery(Player player)
		{
			if (ContainsBatteryItem)
			{
				GiveBatteryItem(player);
				VehicleManager.sendVehicleBatteryCharge(this, 0);
			}
		}

		/// <summary>
		/// Nelson 2024-06-24: Previously, this wouldn't give an item to the player if the quality was zero. Now it
		/// trusts the caller to validate we have a battery item to give, and respects <see cref="ItemAsset.shouldDeleteAtZeroQuality"/>.
		/// </summary>
		private void GiveBatteryItem(Player player)
		{
			Debug.Assert(ContainsBatteryItem);
			byte quality = (byte) Mathf.FloorToInt(batteryCharge / 100f);

			if (batteryItemGuid == System.Guid.Empty)
			{
				batteryItemGuid = asset.defaultBatteryGuid;
			}

			ItemAsset itemAsset = Assets.find(batteryItemGuid) as ItemAsset;
			if (itemAsset != null)
			{
				if (itemAsset.shouldDeleteAtZeroQuality && quality < 1)
				{
					return;
				}

				Item item = new Item(itemAsset.id, 1, quality);
				player.inventory.forceAddItem(item, false);
			}
		}

		public byte tireAliveMask
		{
			get
			{
				int mask = 0;
				for (byte index = 0; index < Mathf.Min(8, _wheels.Length); index++)
				{
					if (_wheels[index].isAlive)
					{
						int flag = 1 << index;
						mask |= flag;
					}
				}
				return (byte) mask;
			}

			set
			{
				int mask = value;
				for (byte index = 0; index < Mathf.Min(8, _wheels.Length); index++)
				{
					if (_wheels[index].wheel == null) // client adds tires with no collider that should always be alive
					{
						continue;
					}

					int flag = 1 << index;
					_wheels[index].isAlive = (mask & flag) == flag;
				}
			}
		}

		public void sendTireAliveMaskUpdate()
		{
			VehicleManager.sendVehicleTireAliveMask(this, tireAliveMask);
		}

		/// <summary>
		/// Can a tire item be used with this vehicle?
		/// </summary>
		public bool isTireCompatible(ushort itemID)
		{
			return asset != null && asset.tireID == itemID;
		}

		public void askRepairTire(int index)
		{
			if (index < 0 || index >= _wheels.Length)
			{
				return;
			}

			_wheels[index].askRepair();
		}

		public void askDamageTire(int index)
		{
			if (isInsideNoDamageZone)
				return;

			if (index < 0 || index >= _wheels.Length)
			{
				return;
			}

			if (asset != null && !asset.canTiresBeDamaged)
			{
				return;
			}

			_wheels[index].askDamage();
		}

		/// <summary>
		/// Find the index of the wheel collider that contains this position.
		/// </summary>
		public int getHitTireIndex(Vector3 position)
		{
			for (int index = 0; index < _wheels.Length; index++)
			{
				WheelCollider wheelCollider = _wheels[index].wheel;
				if (wheelCollider == null)
				{
					continue;
				}

				if ((wheelCollider.transform.position - position).sqrMagnitude < wheelCollider.radius * wheelCollider.radius)
				{
					return index;
				}
			}

			return -1;
		}

		/// <summary>
		/// Find the index of the wheel collider closest to this position, or -1 if not near any.
		/// </summary>
		public int getClosestAliveTireIndex(Vector3 position, bool isAlive)
		{
			int wheelIndex = -1;
			float sqrDistanceThreshold = 16;
			for (int index = 0; index < _wheels.Length; index++)
			{
				if (_wheels[index].isAlive != isAlive)
				{
					continue;
				}

				if (_wheels[index].wheel == null)
				{
					continue;
				}

				float sqrDistance = (_wheels[index].wheel.transform.position - position).sqrMagnitude;
				if (sqrDistance < sqrDistanceThreshold)
				{
					wheelIndex = index;
					sqrDistanceThreshold = sqrDistance;
				}
			}

			return wheelIndex;
		}

		public void getDisplayFuel(out ushort currentFuel, out ushort maxFuel)
		{
			if (usesFuel)
			{
				currentFuel = fuel;
				maxFuel = asset.fuel;
			}
			else
			{
				if (passengers[0].player != null && passengers[0].player.player != null)
				{
					currentFuel = passengers[0].player.player.life.stamina;
				}
				else if (Player.LocalPlayer != null)
				{
					currentFuel = Player.LocalPlayer.life.stamina;
				}
				else
				{
					currentFuel = 0;
				}

				maxFuel = 100;
			}
		}

		public void askBurnFuel(ushort amount)
		{
			if (amount == 0 || isExploded)
			{
				return;
			}

			if (amount >= fuel)
			{
				fuel = 0;
			}
			else
			{
				fuel -= amount;
			}
		}

		public void askFillFuel(ushort amount)
		{
			if (amount == 0 || isExploded)
			{
				return;
			}

			if (amount >= asset.fuel - fuel)
			{
				fuel = asset.fuel;
			}
			else
			{
				fuel += amount;
			}

			VehicleManager.sendVehicleFuel(this, fuel);
			ResetDecayTimer();
		}

		/// <summary>
		/// Called during simulate at fixed rate.
		/// </summary>
		protected void simulateBurnFuel()
		{
			if (!usesFuel || !isEngineOn)
				return;

			float deltaTime = PlayerInput.RATE;
			fuelBurnBuffer += deltaTime * asset.fuelBurnRate;
			ushort fuelBurnDelta = (ushort) Mathf.FloorToInt(fuelBurnBuffer);
			if (fuelBurnDelta > 0)
			{
				fuelBurnBuffer -= fuelBurnDelta;
				askBurnFuel(fuelBurnDelta);
			}
		}

		public void askBurnBattery(ushort amount)
		{
			// Nelson 2024-06-24: One now indicates a battery item is present but depleted, and zero is no battery item.
			if (amount == 0 || isExploded || batteryCharge < 1)
			{
				return;
			}

			// -1 ushort is OK because we checked it's >0 above.
			if (amount >= (batteryCharge - 1))
			{
				batteryCharge = 1;
			}
			else
			{
				batteryCharge -= amount;
			}
		}

		public void askChargeBattery(ushort amount)
		{
			if (amount == 0 || isExploded)
			{
				return;
			}

			if (amount >= 10000 - batteryCharge)
			{
				batteryCharge = 10000;
			}
			else
			{
				batteryCharge += amount;
			}
		}

		public void sendBatteryChargeUpdate()
		{
			VehicleManager.sendVehicleBatteryCharge(this, batteryCharge);
		}

		public void askDamage(ushort amount, bool canRepair)
		{
			if (isInsideNoDamageZone)
				return;

			if (amount == 0)
			{
				return;
			}

			if (isDead)
			{
				if (!canRepair)
				{
					explode();
				}

				return;
			}

#if EXPLOSIONDEBUG
				health = 0;
#else
			if (amount >= health)
			{
				health = 0;
			}
			else
			{
				health -= amount;
			}
#endif

			VehicleManager.sendVehicleHealth(this, health);

			if (isDead && !canRepair)
			{
				explode();
			}
		}

		public void askRepair(ushort amount)
		{
			if (amount == 0 || isExploded)
			{
				return;
			}

			if (amount >= asset.health - health)
			{
				health = asset.health;
			}
			else
			{
				health += amount;
			}

			VehicleManager.sendVehicleHealth(this, health);
		}

		private void explode()
		{
			Vector3 explosionForce = new Vector3(Random.Range(asset.minExplosionForce.x, asset.maxExplosionForce.x),
													Random.Range(asset.minExplosionForce.y, asset.maxExplosionForce.y),
													Random.Range(asset.minExplosionForce.z, asset.maxExplosionForce.z));

			rootRigidbody.AddForce(explosionForce);
			rootRigidbody.AddTorque(16, 0, 0);

			dropTrunkItems();

			if (asset.ShouldExplosionCauseDamage)
			{
				List<EPlayerKill> kills;
				DamageTool.explode(transform.position, 8, EDeathCause.VEHICLE, Steamworks.CSteamID.Nil, 200, 200, 200, 0, 0, 500, 2000, 500, out kills, damageOrigin: EDamageOrigin.Vehicle_Explosion);
			}

			for (int index = 0; index < passengers.Length; index++)
			{
				Passenger passenger = passengers[index];

				if (passenger == null)
				{
					continue;
				}

				SteamPlayer client = passenger.player;

				if (client == null)
				{
					continue;
				}

				Player player = client.player;

				if (player == null)
				{
					continue;
				}

				if (player.life.isDead)
				{
					continue;
				}

				if (asset.ShouldExplosionCauseDamage)
				{
					EPlayerKill kill;
					player.life.askDamage(101, Vector3.up * 101, EDeathCause.VEHICLE, ELimb.SPINE, CSteamID.Nil, out kill);
				}
				else
				{
					VehicleManager.forceRemovePlayer(this, client.playerID.steamID);
				}
			}

			DropScrapItems();

			VehicleManager.sendVehicleExploded(this);

			EffectAsset explosionEffect = asset.FindExplosionEffectAsset();
			if (explosionEffect != null)
			{
				TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(explosionEffect);
				triggerEffectParameters.position = transform.position;
				triggerEffectParameters.relevantDistance = EffectManager.LARGE;
				triggerEffectParameters.reliable = true;
				EffectManager.triggerEffect(triggerEffectParameters);
			}
		}

		public bool checkEnter(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (isHooked)
			{
				return false;
			}

			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return !isLocked || enemyPlayer == lockedOwner || (lockedGroup != CSteamID.Nil && enemyGroup == lockedGroup);
		}

		/// <summary>
		/// Is a given player allowed access to this vehicle?
		/// </summary>
		public bool checkEnter(Player player)
		{
			if (player == null)
				return false;

			CSteamID playerID = player.channel.owner.playerID.steamID;
			CSteamID groupID = player.quests.groupID;
			return checkEnter(playerID, groupID);
		}

		/// <summary>
		/// If true, sentry ignores this vehicle early in target scanning.
		/// Friendly if locked by owner/group of sentry, or if a passenger is owner/group of sentry.
		/// </summary>
		public bool IsFriendlyToSentry(InteractableSentry sentry)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // Singleplayer
			{
				return true;
			}

			if (isLocked && (lockedOwner == sentry.owner || (lockedGroup != CSteamID.Nil && lockedGroup == sentry.group)))
			{
				return true;
			}

			// Originally, only considered friendly if DRIVER was owner/group of sentry, not ANY passenger.
			// IIRC the intention was to fire at vehicles if the owner was taken hostage.
			// We changed this because it may have been unintuitive. (E.g., public issue #5429)
			// On the other hand, it might be MORE interesting to use the hostage as a way to break into a base.
			if (_passengers != null)
			{
				foreach (Passenger slot in _passengers)
				{
					if (slot.player == null)
					{
						continue;
					}

					if (slot.player.playerID.steamID == sentry.owner)
					{
						return true;
					}

					if (slot.player.player?.quests?.isMemberOfGroup(sentry.group) ?? false)
					{
						return true;
					}
				}
			}

			return false;
		}

		public Vector3 GetSentryTargetingPoint()
		{
			Debug.Assert(center != null);
			return center != null ? center.position : transform.position;
		}

		public override bool checkUseable()
		{
			if (Player.LocalPlayer == null || (transform.position - Player.LocalPlayer.transform.position).sqrMagnitude > 100)
			{
				return false;
			}

			return !isExploded && checkEnter(Provider.client, Player.LocalPlayer.quests.groupID);
		}

		public override void use()
		{
			VehicleManager.enterVehicle(this);
		}

		public override bool checkHighlight(out Color color)
		{
			color = ItemTool.getRarityColorHighlight(asset.rarity);
			return true;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (checkUseable())
			{
				message = EPlayerMessage.VEHICLE_ENTER;
				text = asset.vehicleName;
				color = ItemTool.getRarityColorUI(asset.rarity);
			}
			else
			{
				if (Player.LocalPlayer == null || (transform.position - Player.LocalPlayer.transform.position).sqrMagnitude > 100)
				{
					message = EPlayerMessage.BLOCKED;
				}
				else
				{
					message = EPlayerMessage.LOCKED;
				}

				text = "";
				color = Color.white;
			}

			return !isExploded;
		}

		public void updateVehicle()
		{
			lastUpdatedPos = transform.position;

			interpTargetPosition = transform.position;
			interpTargetRotation = transform.rotation;

			real = transform.position;

			isRecovering = false;
			lastRecover = Time.realtimeSinceStartup;
			isFrozen = false;
		}

		/// <summary>
		/// Average vehicle-space position of wheel bases.
		/// </summary>
		private Vector3? calculateAverageLocalTireContactPosition()
		{
			if (_wheels == null)
				return null;

			Vector3 averagePosition = Vector3.zero;
			int numberOfWheelColliders = 0;

			foreach (Wheel wheel in _wheels)
			{
				WheelCollider wheelCollider = wheel.wheel;
				if (wheelCollider == null)
					continue;

				Transform wheelTransform = wheelCollider.transform;
				Vector3 contactWorldPosition = wheelTransform.TransformPoint(wheelCollider.center - new Vector3(0, wheelCollider.radius, 0));
				Vector3 contactLocalPosition = transform.InverseTransformPoint(contactWorldPosition);

				averagePosition += contactLocalPosition;
				++numberOfWheelColliders;
			}

			if (numberOfWheelColliders > 0)
			{
				return averagePosition / numberOfWheelColliders;
			}
			else
			{
				return null;
			}
		}

		public void updatePhysics()
		{
			if (checkDriver(Provider.client) || (Provider.isServer && !isDriven))
			{
				rootRigidbody.useGravity = usesGravity;
				rootRigidbody.isKinematic = isKinematic;

				isPhysical = true;

				if (!isExploded)
				{
					if (_wheels != null)
					{
						foreach (Wheel wheel in _wheels)
						{
							wheel.isPhysical = true;
						}
					}

					if (buoyancy != null)
					{
						buoyancy.gameObject.SetActive(true);
					}
				}
			}
			else
			{
				rootRigidbody.useGravity = false;
				rootRigidbody.isKinematic = true;
				isPhysical = false;

				if (_wheels != null)
				{
					foreach (Wheel wheel in _wheels)
					{
						wheel.isPhysical = false;
					}
				}

				if (buoyancy != null)
				{
					buoyancy.gameObject.SetActive(false);
				}
			}

			if (hasDefaultCenterOfMass == false)
			{
				hasDefaultCenterOfMass = true;
				defaultCenterOfMass = rootRigidbody.centerOfMass;
			}

			Vector3 localCenterOfMass;
			if (asset.hasCenterOfMassOverride)
			{
				localCenterOfMass = asset.centerOfMass;
			}
			else
			{
				Transform cog = transform.Find("Cog");
				if (cog)
				{
					localCenterOfMass = cog.localPosition;
				}
				else
				{
					localCenterOfMass = new Vector3(0, -0.25f, 0);

					if (asset.engine == EEngine.CAR)
					{
						Vector3? averageTirePosition = calculateAverageLocalTireContactPosition();
						if (averageTirePosition.HasValue)
						{
							localCenterOfMass = averageTirePosition.Value;
						}
					}
				}
			}
#if UNITY_EDITOR
			centerOfMassOverride = localCenterOfMass;
#endif // UNITY_EDITOR
			rootRigidbody.centerOfMass = localCenterOfMass;
		}

		public void updateEngine()
		{
			synchronizeTaillights();

			if (!Dedicator.IsDedicatedServer)
			{
				foreach (GameObject siren in sirenGameObjects)
				{
					AudioSource audio = siren.GetComponent<AudioSource>();
					if (audio != null)
					{
						audio.enabled = isDriven;
					}
				}
			}
		}

		internal static readonly ClientInstanceMethod<Color32> SendPaintColor = ClientInstanceMethod<Color32>.Get(typeof(InteractableVehicle), nameof(ReceivePaintColor));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceivePaintColor([NetPakColor32(withAlpha: true)] Color32 newPaintColor)
		{
			PaintColor = newPaintColor;
			ApplyPaintColor();

			if (eventHook != null)
			{
				eventHook.OnPaintColorChanged.TryInvoke(this);
			}
		}

		public void ServerSetPaintColor(Color32 newPaintColor)
		{
			if (!PaintColor.Equals(newPaintColor))
			{
				SendPaintColor.InvokeAndLoopback(GetNetId(), NetTransport.ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newPaintColor);
			}
		}

		public void tellLocked(CSteamID owner, CSteamID group, bool locked)
		{
			_lockedOwner = owner;
			_lockedGroup = group;
			_isLocked = locked;

			onLockUpdated?.Invoke();

			if (eventHook != null)
			{
				if (locked)
				{
					eventHook.OnLocked.TryInvoke(this);
				}
				else
				{
					eventHook.OnUnlocked.TryInvoke(this);
				}
			}

			OnLockChanged_Global.TryInvoke("OnLockChanged_Global", this);
		}

		public void tellSkin(ushort newSkinID, ushort newMythicID)
		{
			skinID = newSkinID;
			mythicID = newMythicID;
			updateSkin();

			skinChanged?.Invoke();
		}

		public void updateSkin()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			skinAsset = Assets.find(EAssetType.SKIN, skinID) as SkinAsset;

			if (tempMesh != null)
			{
				HighlighterTool.remesh(transform, tempMesh, null);
			}

			if (skinOriginalMaterials != null && skinOriginalMaterials.Count > 0)
			{
				foreach (VehicleSkinMaterialChange materialChange in skinOriginalMaterials)
				{
					if (materialChange.shared)
					{
						materialChange.renderer.sharedMaterial = materialChange.originalMaterial;
					}
					else
					{
						materialChange.renderer.material = materialChange.originalMaterial;
					}
				}

				skinOriginalMaterials.Clear();
			}

			if (skinMaterialToDestroy != null)
			{
				Destroy(skinMaterialToDestroy);
				skinMaterialToDestroy = null;
			}

			if (effectSystems != null)
			{
				for (int systemIndex = 0; systemIndex < effectSystems.Length; systemIndex++)
				{
					MythicalEffectController system = effectSystems[systemIndex];
					if (system != null)
					{
						Destroy(system);
					}
				}
			}

			if (skinAsset != null)
			{
				VehicleAsset sharedVehicleAsset = asset.FindSharedSkinVehicleAsset();

				if (mythicID != 0)
				{
					if (effectSlotsRoot == null)
					{
						effectSlotsRoot = transform.Find("Effect_Slots");
						if (effectSlotsRoot == null)
						{
							effectSlotsRoot = Instantiate(sharedVehicleAsset.GetOrLoadModel().transform.Find("Effect_Slots").gameObject).transform;
							effectSlotsRoot.parent = transform;
							effectSlotsRoot.name = "Effect_Slots";
							effectSlotsRoot.localPosition = Vector3.zero;
							effectSlotsRoot.localRotation = Quaternion.identity;
							effectSlotsRoot.localScale = Vector3.one;
						}

						effectSlots = new Transform[effectSlotsRoot.childCount];
						for (int childIndex = 0; childIndex < effectSlots.Length; childIndex++)
						{
							effectSlots[childIndex] = effectSlotsRoot.GetChild(childIndex);
						}

						effectSystems = new MythicalEffectController[effectSlots.Length];
					}

					ItemTool.ApplyMythicalEffectToMultipleTransforms(effectSlots, effectSystems, mythicID, EEffectType.AREA);
				}

				if (skinAsset.overrideMeshes != null && skinAsset.overrideMeshes.Count > 0)
				{
					if (tempMesh == null)
					{
						tempMesh = new List<Mesh>();
						HighlighterTool.remesh(transform, skinAsset.overrideMeshes, tempMesh);
					}
					else
					{
						HighlighterTool.remesh(transform, skinAsset.overrideMeshes, null);
					}
				}

				if (skinAsset.primarySkin != null)
				{
					Material materialToApply;

					if (skinAsset.isPattern)
					{
						Material material = Instantiate(skinAsset.primarySkin);
						material.SetTexture("_AlbedoBase", sharedVehicleAsset.albedoBase);
						material.SetTexture("_MetallicBase", sharedVehicleAsset.metallicBase);
						material.SetTexture("_EmissionBase", sharedVehicleAsset.emissionBase);

						materialToApply = material;

						// Material will need to be destroyed because it is instantiated, not shared.
						skinMaterialToDestroy = material;

					}
					else
					{
						materialToApply = skinAsset.primarySkin;

						// Material will not need to be destroyed because it is shared, not instantiated.
						skinMaterialToDestroy = null;
					}

					if (skinOriginalMaterials == null)
					{
						skinOriginalMaterials = new List<VehicleSkinMaterialChange>();
					}
					else
					{
						skinOriginalMaterials.Clear();
					}

					// This is not great and mostly copied from HighlighterTool.rematerialize, but necessary to properly
					// restore paintable sections when clearing skin.
					bool shared = paintableMaterials == null || paintableMaterials.Count < 1;
					Renderer renderer = GetComponent<Renderer>();
					if (renderer != null)
					{
						ApplySkinToRenderer(renderer, materialToApply, shared);
					}
					else
					{
						for (int lod = 0; lod < 4; lod++)
						{
							Transform model = transform.Find("Model_" + lod);

							if (model == null)
							{
								continue;
							}

							renderer = model.GetComponent<Renderer>();
							if (renderer != null)
							{
								ApplySkinToRenderer(renderer, materialToApply, shared);
							}
						}
					}
				}
			}
		}

		public void tellSirens(bool on)
		{
			_sirensOn = on;

			if (!Dedicator.IsDedicatedServer)
			{
				foreach (GameObject siren in sirenGameObjects)
				{
					siren.SetActive(sirensOn);
				}

				if (sirenMaterials != null)
				{
					for (int index = 0; index < sirenMaterials.Length; index++)
					{
						if (sirenMaterials[index] != null)
						{
							sirenMaterials[index].SetColor("_EmissionColor", Color.black);
						}
					}
				}
			}

			onSirensUpdated?.Invoke();

			if (eventHook != null)
			{
				if (_sirensOn)
				{
					eventHook.OnSirensActivated.TryInvoke(this);
				}
				else
				{
					eventHook.OnSirensDeactivated.TryInvoke(this);
				}
			}
		}

		public void tellBlimp(bool on)
		{
			isBlimpFloating = on;

			if (asset.engine != EEngine.BLIMP)
			{
				return;
			}

			int buoyancyCount = buoyancy.childCount;
			for (int buoyancyIndex = 0; buoyancyIndex < buoyancyCount; buoyancyIndex++)
			{
				buoyancy.GetChild(buoyancyIndex).GetComponent<Buoyancy>().enabled = isBlimpFloating;
			}

			onBlimpUpdated?.Invoke();

			if (eventHook != null)
			{
				if (isBlimpFloating)
				{
					eventHook.OnBlimpActivated.TryInvoke(this);
				}
				else
				{
					eventHook.OnBlimpDeactivated.TryInvoke(this);
				}
			}
		}
		public void tellHeadlights(bool on)
		{
			_headlightsOn = on;

			if (!Dedicator.IsDedicatedServer)
			{
				if (headlights != null)
				{
					headlights.gameObject.SetActive(headlightsOn);
				}

				if (headlightsMaterial != null)
				{
					headlightsMaterial.SetColor("_EmissionColor", headlightsOn ? headlightsMaterial.color * 2f : Color.black);
				}
			}

			onHeadlightsUpdated?.Invoke();

			if (eventHook != null)
			{
				if (headlightsOn)
				{
					eventHook.OnHeadlightsActivated.TryInvoke(this);
				}
				else
				{
					eventHook.OnHeadlightsDeactivated.TryInvoke(this);
				}
			}
		}

		public void tellTaillights(bool on)
		{
			// Does not test whether value actually changed due to backwards compatibility - caller does that test.
			_taillightsOn = on;

			if (!Dedicator.IsDedicatedServer)
			{
				if (taillights != null)
				{
					taillights.gameObject.SetActive(taillightsOn);
				}

				if (taillightsMaterial != null)
				{
					taillightsMaterial.SetColor("_EmissionColor", taillightsOn ? taillightsMaterial.color * 2f : Color.black);
				}
				else if (taillightMaterials != null)
				{
					for (int index = 0; index < taillightMaterials.Length; index++)
					{
						if (taillightMaterials[index] != null)
						{
							taillightMaterials[index].SetColor("_EmissionColor", taillightsOn ? taillightMaterials[index].color * 2f : Color.black);
						}
					}
				}
			}

			onTaillightsUpdated?.Invoke();
		}

		/// <summary>
		/// Turn taillights on/off depending on state.
		/// </summary>
		private void synchronizeTaillights()
		{
			bool desiredState = isDriven && canTurnOnLights;
			if (taillightsOn != desiredState)
			{
				tellTaillights(desiredState);
			}
		}

		public void tellHorn()
		{
			horned = Time.realtimeSinceStartup;

			if (!Dedicator.IsDedicatedServer && clipAudioSource != null && asset.horn != null)
			{
				clipAudioSource.pitch = 1f;
				clipAudioSource.PlayOneShot(asset.horn);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 32);
			}

			eventHook?.OnHornUsed.TryInvoke(this);
		}

		public void tellFuel(ushort newFuel)
		{
			fuel = newFuel;

			OnFuelChanged_Global.TryInvoke("OnFuelChanged_Global", this);
		}

		public void tellBatteryCharge(ushort newBatteryCharge)
		{
			batteryCharge = newBatteryCharge;
			if (!HasBatteryWithCharge)
			{
				isEngineOn = false;
			}

			batteryChanged?.Invoke();

			OnBatteryLevelChanged_Global.TryInvoke("OnBatteryLevelChanged_Global", this);
		}

		public void tellExploded()
		{
			clearHooked();

			isExploded = true;
			_lastExploded = Time.realtimeSinceStartup;

			if (sirensOn)
			{
				tellSirens(false);
			}

			if (isBlimpFloating)
			{
				tellBlimp(false);
			}

			if (headlightsOn)
			{
				tellHeadlights(false);
			}

			if (_wheels != null)
			{
				foreach (Wheel wheel in _wheels)
				{
					wheel.isPhysical = false;
				}
			}

			if (buoyancy != null)
			{
				buoyancy.gameObject.SetActive(false);
			}

			if (!Dedicator.IsDedicatedServer)
			{
				if (asset.ShouldExplosionBurnMaterials)
				{
					ApplyExplosionBurnMaterials();
				}

				updateFires();

				if (_wheels != null)
				{
					foreach (Wheel wheel in _wheels)
					{
						wheel.Explode();
					}
				}

				if (propellerModels != null)
				{
					foreach (PropellerModel model in propellerModels)
					{
						if (model.transform != null)
						{
							Destroy(model.transform.gameObject);
							model.transform = null;
						}
					}
				}

				if (exhaustParticleSystems != null)
				{
					if (!isExhaustRateOverTimeZero)
					{
						SetExhaustParticleSystemsRateOverTimeToZero();
					}
				}
			}

			if (eventHook != null)
			{
				eventHook.OnExploded.TryInvoke(this);
			}
		}

		public void updateFires()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (fire != null)
				{
					fire.gameObject.SetActive((isExploded || isDead) && !isUnderwater);
				}

				if (smoke_0 != null)
				{
					smoke_0.gameObject.SetActive((isExploded || health < SMOKE_0_HEALTH_THRESHOLD) && !isUnderwater);
				}

				if (smoke_1 != null)
				{
					smoke_1.gameObject.SetActive((isExploded || health < SMOKE_1_HEALTH_THRESHOLD) && !isUnderwater);
				}
			}
		}

		public void tellHealth(ushort newHealth)
		{
			health = newHealth;

			if (isDead)
			{
				_lastDead = Time.realtimeSinceStartup;
			}

			updateFires();

			OnHealthChanged_Global.TryInvoke("OnHealthChanged_Global", this);
			OnHealthChanged?.TryInvoke("OnHealthChanged", this);
		}

		public void tellRecov(Vector3 newPosition, int newRecov)
		{
			lastTick = Time.realtimeSinceStartup;

			rootRigidbody.MovePosition(newPosition);

			isFrozen = true;
			rootRigidbody.useGravity = false;
			rootRigidbody.isKinematic = true;

			if (passengers[0] != null && passengers[0].player != null && passengers[0].player.player != null && passengers[0].player.player.input != null)
			{
				passengers[0].player.player.input.recov = newRecov;
			}
		}

		public void tellState(Vector3 newPosition, Quaternion newRotation, float newSpeed, float newForwardVelocity, float newReplicatedSteeringInput, float newReplicatedVelocityInput)
		{
			if (isDriver)
			{
				return;
			}

			lastTick = Time.realtimeSinceStartup;
			lastUpdatedPos = newPosition;

			interpTargetPosition = newPosition;
			interpTargetRotation = newRotation;

			if (asset.engine == EEngine.TRAIN)
			{
				roadPosition = UnpackRoadPosition(newPosition);
			}

			ReplicatedSpeed = newSpeed;
			ReplicatedForwardVelocity = newForwardVelocity;
			ReplicatedSteeringInput = newReplicatedSteeringInput;
			ReplicatedVelocityInput = newReplicatedVelocityInput;
		}

		public bool checkDriver(CSteamID steamID)
		{
			return isDriven && passengers[0].player.playerID.steamID == steamID;
		}

		public SteamPlayer GetDriverClient()
		{
			if (passengers != null && passengers.Length > 0)
			{
				return passengers[0].player;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Returns null if index is out of bounds or initialization has failed.
		/// </summary>
		public Passenger GetSeatByIndex(int seatIndex)
		{
			if (seatIndex >= 0 && passengers != null && seatIndex < passengers.Length)
			{
				return passengers[seatIndex];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Returns null if index is out of bounds, initialization failed, or seat is empty.
		/// </summary>
		public SteamPlayer GetClientBySeatIndex(int seatIndex)
		{
			return GetSeatByIndex(seatIndex)?.player;
		}

		public Player GetDriverPlayer()
		{
			SteamPlayer driverClient = GetDriverClient();
			return driverClient?.player;
		}

		public void grantTrunkAccess(Player player)
		{
			if (Provider.isServer && trunkItems != null && trunkItems.height > 0)
			{
				player.inventory.openTrunk(trunkItems);
			}
		}

		public void revokeTrunkAccess(Player player)
		{
			if (Provider.isServer)
			{
				player.inventory.closeTrunk();
			}
		}

		public void dropTrunkItems()
		{
			if (Provider.isServer && trunkItems != null)
			{
				for (byte index = 0; index < trunkItems.getItemCount(); index++)
				{
					ItemJar jar = trunkItems.getItem(index);

					ItemManager.dropItem(jar.item, transform.position, false, true, true);
				}

				trunkItems.clear();
				trunkItems = null;

				if (passengers[0].player != null && passengers[0].player.player != null)
				{
					revokeTrunkAccess(passengers[0].player.player);
				}
			}
		}

		/// <summary>
		/// This check should really not be necessary, but somehow it is a recurring issue that servers get slowed down
		/// by something going wrong and the vehicle exploding a billion times leaving items everywhere.
		/// </summary>
		private bool hasDroppedScrapItemsAlready;
		private void DropScrapItems()
		{
			if (!hasDroppedScrapItemsAlready && asset.dropsTableId > 0)
			{
				hasDroppedScrapItemsAlready = true;

				int drops = Random.Range(asset.dropsMin, asset.dropsMax);
				// Prevent players from crashing themselves with huge numbers of items.
				drops = Mathf.Clamp(drops, 0, 100);
				for (int step = 0; step < drops; step++)
				{
					float angle = Random.Range(0, Mathf.PI * 2);
					ushort id = SpawnTableTool.ResolveLegacyId(asset.dropsTableId, EAssetType.ITEM, OnGetDropsSpawnTableErrorContext);
					if (id == 0)
						continue;

					ItemManager.dropItem(new Item(id, EItemOrigin.NATURE), transform.position + new Vector3(Mathf.Sin(angle) * 3, 1, Mathf.Cos(angle) * 3), false, Dedicator.IsDedicatedServer, true);
				}
			}
		}

		private string OnGetDropsSpawnTableErrorContext()
		{
			return $"{asset?.FriendlyName} explosion drops";
		}

		public void addPlayer(byte seatIndex, CSteamID steamID)
		{
			SteamPlayer player = PlayerTool.getSteamPlayer(steamID);

			if (player != null)
			{
				passengers[seatIndex].player = player;

				if (player.player != null)
				{
					player.player.movement.setVehicle(this, seatIndex, passengers[seatIndex].seat, Vector3.zero, 0, false);

					if (passengers[seatIndex].turret != null)
					{
						player.player.equipment.turretEquipClient();

						if (Provider.isServer)
						{
							player.player.equipment.turretEquipServer(passengers[seatIndex].turret.itemID, passengers[seatIndex].state);
						}
					}
				}

				if (passengers[seatIndex].collider != null)
				{
					passengers[seatIndex].collider.enabled = true;
				}

				updatePhysics();

				if (seatIndex == 0)
				{
					grantTrunkAccess(player.player);
				}
			}

			if (seatIndex == 0)
			{
				isEngineOn = (!usesBattery || HasBatteryWithCharge) && !isUnderwater;
			}

			updateEngine();

			if (seatIndex == 0 && isEnginePowered && isEngineOn && !Dedicator.IsDedicatedServer && !isUnderwater)
			{
				PlayIgnitionSound();
			}

			onPassengersUpdated?.Invoke();

			bool isLocallyControlled = !Dedicator.IsDedicatedServer && player != null && Player.LocalPlayer != null && Player.LocalPlayer == player.player; // // 7 "player" in a row... sigh.

			if (eventHook != null)
			{
				if (seatIndex == 0)
				{
					eventHook.OnDriverAdded.TryInvoke(this);
					if (isLocallyControlled)
					{
						eventHook.OnLocalDriverAdded.TryInvoke(this);
					}
				}

				if (isLocallyControlled)
				{
					eventHook.OnLocalPassengerAdded.TryInvoke(this);
				}
			}

			if (passengers[seatIndex].turretEventHook != null)
			{
				passengers[seatIndex].turretEventHook.OnPassengerAdded.TryInvoke(this);
				if (isLocallyControlled)
				{
					passengers[seatIndex].turretEventHook.OnLocalPassengerAdded.TryInvoke(this);
				}
			}

			OnPassengerAdded_Global.TryInvoke("OnPassengerAdded_Global", this, seatIndex);
		}

		public void removePlayer(byte seatIndex, Vector3 point, byte angle, bool forceUpdate)
		{
			SteamPlayer removedClient = null;
			if (passengers != null && seatIndex < passengers.Length)
			{
				Passenger seat = passengers[seatIndex];
				removedClient = seat.player;

				if (removedClient != null && removedClient.player != null)
				{
					if (seat.turret != null)
					{
						removedClient.player.equipment.turretDequipClient();

						if (Provider.isServer)
						{
							removedClient.player.equipment.turretDequipServer();
						}
					}

					removedClient.player.movement.setVehicle(null, 0, null, point, angle, forceUpdate);
				}

				if (passengers[seatIndex].collider != null)
				{
					passengers[seatIndex].collider.enabled = false;
				}

				seat.player = null;

				updatePhysics();

				if (Provider.isServer)
				{
					VehicleManager.sendVehicleFuel(this, fuel);
					VehicleManager.sendVehicleBatteryCharge(this, batteryCharge);
				}

				if (seatIndex == 0 && removedClient != null && removedClient.player != null)
				{
					revokeTrunkAccess(removedClient.player);
				}
			}

			if (seatIndex == 0)
			{
				isEngineOn = false;
			}

			updateEngine();

			if (seatIndex == 0)
			{
				inputTargetVelocity = 0;
				inputEngineVelocity = 0;

				// Reset steering angle when driver exits the vehicle.
				ReplicatedSteeringInput = 0.0f;

				if (!Dedicator.IsDedicatedServer)
				{
					if (windZone != null)
					{
						windZone.windMain = 0;
					}
				}

				if (_wheels != null)
				{
					foreach (Wheel wheel in _wheels)
					{
						wheel.Reset();
					}
				}
			}

			onPassengersUpdated?.Invoke();

			bool isLocallyControlled = !Dedicator.IsDedicatedServer && removedClient != null && Player.LocalPlayer != null && Player.LocalPlayer == removedClient.player;

			if (passengers[seatIndex].turretEventHook != null)
			{
				if (isLocallyControlled)
				{
					passengers[seatIndex].turretEventHook.OnLocalPassengerRemoved.TryInvoke(this);
				}
				passengers[seatIndex].turretEventHook.OnPassengerRemoved.TryInvoke(this);
			}

			if (eventHook != null)
			{
				if (isLocallyControlled)
				{
					eventHook.OnLocalPassengerRemoved.TryInvoke(this);
				}

				if (seatIndex == 0)
				{
					if (isLocallyControlled)
					{
						eventHook.OnLocalDriverRemoved.TryInvoke(this);
					}
					eventHook.OnDriverRemoved.TryInvoke(this);
				}
			}

			OnPassengerRemoved_Global.TryInvoke("OnPassengerRemoved_Global", this, seatIndex, removedClient?.player);
		}

		public void swapPlayer(byte fromSeatIndex, byte toSeatIndex)
		{
			Passenger fromSeat = GetSeatByIndex(fromSeatIndex);
			Passenger toSeat = GetSeatByIndex(toSeatIndex);

			SteamPlayer swappedClient = null;
			if (fromSeat != null && toSeat != null)
			{
				swappedClient = fromSeat.player;

				if (swappedClient != null && swappedClient.player != null)
				{
					if (fromSeat.turret != null)
					{
						swappedClient.player.equipment.turretDequipClient();

						if (Provider.isServer)
						{
							swappedClient.player.equipment.turretDequipServer();
						}
					}

					swappedClient.player.movement.setVehicle(this, toSeatIndex, toSeat.seat, Vector3.zero, 0, false);

					if (toSeat.turret != null)
					{
						swappedClient.player.equipment.turretEquipClient();

						if (Provider.isServer)
						{
							swappedClient.player.equipment.turretEquipServer(toSeat.turret.itemID, toSeat.state);
						}
					}
				}

				if (fromSeat.collider != null)
				{
					fromSeat.collider.enabled = false;
				}

				if (toSeat.collider != null)
				{
					toSeat.collider.enabled = true;
				}

				fromSeat.player = null;
				toSeat.player = swappedClient;

				updatePhysics();

				if (Provider.isServer)
				{
					VehicleManager.sendVehicleFuel(this, fuel);
					VehicleManager.sendVehicleBatteryCharge(this, batteryCharge);
				}

				if (fromSeatIndex == 0 && swappedClient != null && swappedClient.player != null)
				{
					revokeTrunkAccess(swappedClient.player);
				}

				if (toSeatIndex == 0 && swappedClient != null && swappedClient.player != null)
				{
					grantTrunkAccess(swappedClient.player);
				}
			}

			if (toSeatIndex == 0)
			{
				isEngineOn = (!usesBattery || HasBatteryWithCharge) && !isUnderwater;
			}

			if (fromSeatIndex == 0)
			{
				isEngineOn = false;
			}

			updateEngine();

			if (toSeatIndex == 0 && isEnginePowered && isEngineOn && !Dedicator.IsDedicatedServer && !isUnderwater)
			{
				PlayIgnitionSound();
			}

			if (fromSeatIndex == 0)
			{
				inputTargetVelocity = 0;
				inputEngineVelocity = 0;

				// Reset steering angle when driver exits the vehicle.
				ReplicatedSteeringInput = 0.0f;

				if (!Dedicator.IsDedicatedServer)
				{
					if (windZone != null)
					{
						windZone.windMain = 0;
					}
				}

				if (_wheels != null)
				{
					foreach (Wheel wheel in _wheels)
					{
						wheel.Reset();
					}
				}
			}

			onPassengersUpdated?.Invoke();

			bool isLocallyControlled = !Dedicator.IsDedicatedServer && swappedClient != null && Player.LocalPlayer != null && Player.LocalPlayer == swappedClient.player;

			if (fromSeat?.turretEventHook != null)
			{
				if (isLocallyControlled)
				{
					fromSeat.turretEventHook.OnLocalPassengerRemoved.TryInvoke(this);
				}
				fromSeat.turretEventHook.OnPassengerRemoved.TryInvoke(this);
			}

			if (toSeat?.turretEventHook != null)
			{
				toSeat.turretEventHook.OnPassengerAdded.TryInvoke(this);
				if (isLocallyControlled)
				{
					toSeat.turretEventHook.OnLocalPassengerAdded.TryInvoke(this);
				}
			}

			if (eventHook != null)
			{
				if (fromSeatIndex == 0)
				{
					if (isLocallyControlled)
					{
						eventHook.OnLocalDriverRemoved.TryInvoke(this);
					}
					eventHook.OnDriverRemoved.TryInvoke(this);
				}

				if (toSeatIndex == 0)
				{
					eventHook.OnDriverAdded.TryInvoke(this);
					if (isLocallyControlled)
					{
						eventHook.OnLocalDriverAdded.TryInvoke(this);
					}
				}
			}

			OnPassengerChangedSeats_Global.TryInvoke("OnPassengerChangedSeats_Global", this, fromSeatIndex, toSeatIndex);
		}

		/// <summary>
		/// VehicleManager expects this to only find the seat, not add the player,
		/// because it does a LoS check.
		/// </summary>
		public bool tryAddPlayer(out byte seat, Player player)
		{
			seat = 255;

			if (player == null)
			{
				return false;
			}

			if (isExploded)
			{
				return false;
			}

			if (!isExitable)
			{
				return false;
			}

			// Sanity check that player isn't already in this vehicle.
			for (byte index = 0; index < passengers.Length; ++index)
			{
				if (passengers[index] == null)
					continue;

				if (passengers[index].player == player.channel.owner)
				{
					// Already seated!
					return false;
				}
			}

			bool isHandcuffed = player.animator.gesture == EPlayerGesture.ARREST_START;
			for (byte index = (byte) (isHandcuffed ? 1 : 0); index < passengers.Length; index++)
			{
				if (passengers[index] == null)
					continue;

				if (passengers[index].player != null)
					continue; // Already occupied.

				if (isHandcuffed)
				{
					if (passengers[index].turret != null)
						continue; // No using a mounted machine gun while handcuffed!
				}

				seat = index;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Call on the server to empty the vehicle of passengers.
		/// </summary>
		public void forceRemoveAllPlayers()
		{
			ThreadUtil.assertIsGameThread();

			for (int index = 0; index < passengers.Length; index++)
			{
				Passenger passenger = passengers[index];

				if (passenger == null)
				{
					// Nelson 2024-11-12: Probably shouldn't happen? But harmless here?
					continue;
				}

				SteamPlayer client = passenger.player;

				if (client == null)
				{
					// Nobody in this seat.
					continue;
				}

				Player player = client.player;

				if (player == null)
				{
					UnturnedLog.error($"Encountered client ({client}) without player game object when force-removing passengers from vehicle");
					continue;
				}

				// Nelson 2024-11-12: Previously, this checked whether the player was dead. The player *should* have
				// already been removed if they are dead, but I think it's worth removing this check just in case to
				// assist with public issue #4760.

				VehicleManager.forceRemovePlayer(this, client.playerID.steamID);
			}
		}

		/// <summary>
		/// Kicks them out even if there isn't a good spot. Used when killing the occupant.
		/// </summary>
		/// <returns>True if player is seated, false otherwise.</returns>
		public bool forceRemovePlayer(out byte seat, CSteamID player, out Vector3 point, out byte angle)
		{
			seat = 255;

			point = Vector3.zero;
			angle = 0;

			if (findPlayerSeat(player, out seat))
			{
				forceGetExit(passengers[seat]?.player?.player, seat, out point, out angle);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool findPlayerSeat(CSteamID player, out byte seat)
		{
			seat = byte.MaxValue;

			for (byte index = 0; index < passengers.Length; index++)
			{
				if (passengers[index] != null && passengers[index].player != null && passengers[index].player.playerID.steamID == player)
				{
					seat = index;
					return true;
				}
			}

			return false;
		}

		public bool findPlayerSeat(Player player, out byte seat)
		{
			return findPlayerSeat(player.channel.owner.playerID.steamID, out seat);
		}

		public bool trySwapPlayer(Player player, byte toSeat, out byte fromSeat)
		{
			fromSeat = 255;

			if (toSeat >= passengers.Length)
			{
				return false;
			}

			if (player.animator.gesture == EPlayerGesture.ARREST_START)
			{
				if (toSeat < 1)
				{
					return false; // No driving while handcuffed.
				}

				if (passengers[toSeat].turret != null)
				{
					return false; // No using a mounted machine gun while handcuffed!
				}
			}

			for (byte index = 0; index < passengers.Length; index++)
			{
				if (passengers[index] != null && passengers[index].player != null && passengers[index].player.player == player)
				{
					if (toSeat != index)
					{
						fromSeat = index;

						if (passengers[toSeat].player == null)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Can a safe exit point currently be found?
		///
		/// Called when considering to add a new passenger to prevent players from entering
		/// a vehicle that they wouldn't be able to exit properly.
		/// </summary>
		public bool isExitable
		{
			get
			{
				// Note that this calls tryGetExit which only returns true when a safe exit is available.
				Vector3 point;
				byte angle;
				return tryGetExit(0, out point, out angle);
			}
		}

		/// <summary>
		/// Could a player capsule fit in a given exit position?
		/// </summary>
		protected bool isExitPositionEmpty(Vector3 position)
		{
			bool isEmpty = PlayerStance.hasTeleportClearanceAtPosition(position);
#if DRAW_EXIT_GIZMOS
			PlayerStance.drawStandingCapsule(position, isEmpty ? Color.green : Color.red, 4.0f);
#endif // DRAW_EXIT_GIZMOS
			return isEmpty;
		}

		/// <returns>True if anything was hit.</returns>
		protected bool raycastIgnoringVehicleAndChildren(Vector3 origin, Vector3 direction, float maxDistance, out float hitDistance)
		{
			hitDistance = maxDistance;
			bool hitAnything = false;

			RaycastHit[] hits = Physics.RaycastAll(new Ray(origin, direction), maxDistance, RayMasks.BLOCK_EXIT);
			if (hits != null && hits.Length > 0)
			{
				foreach (RaycastHit hit in hits)
				{
					if (hit.transform != null && !hit.transform.IsChildOf(transform))
					{
						hitDistance = Mathf.Min(hitDistance, hit.distance);
						hitAnything = true;
					}
				}
			}

			return hitAnything;
		}

#if WITH_EXIT_OVERLAP
		/// <returns>True if anything was hit.</returns>
		protected bool overlapCapsuleIgnoringVehicleAndChildren(Vector3 point0, Vector3 point1, float radius, int layerMask)
		{
			layerMask = layerMask & (~LayerMasks.GROUND);
			Collider[] hits = Physics.OverlapCapsule(point0, point1, radius, layerMask, QueryTriggerInteraction.Ignore);
			if(hits != null && hits.Length > 0)
			{
				foreach(Collider hit in hits)
				{
					if(hit != null && !hit.transform.IsChildOf(transform))
					{
						return true;
					}
				}
			}

			return false;
		}
#endif // WITH_EXIT_OVERLAP

		/// <summary>
		/// Raycast along a given direction, penetrating through barricades attached to THIS vehicle.
		/// Returns point at the end of the ray if unblocked, or a safe (radius) distance away from hit.
		/// </summary>
		protected Vector3 getExitDistanceInDirection(Vector3 origin, Vector3 direction, float maxDistance, float extraPadding = 0.1f)
		{
			float distance;
			raycastIgnoringVehicleAndChildren(origin, direction, maxDistance, out distance);

			// Padding away from the end point because we do a capsule overlap.
			float padding = PlayerStance.RADIUS + extraPadding;

			// Note that we do not clamp max - safe because it could be a small
			// max distance after penetrating barricades, in which case we want
			// to move backward toward origin.
			return origin + (direction * (distance - padding));
		}

		protected void findGroundForExitPosition(ref Vector3 exitPosition)
		{
			RaycastHit groundHit;
			Physics.Raycast(new Ray(exitPosition, Vector3.down), out groundHit, 3.0f, RayMasks.BLOCK_EXIT_FIND_GROUND);
			if (groundHit.transform != null)
			{
				exitPosition = groundHit.point + new Vector3(0, 0.25f, 0);
			}
		}

		protected bool getSafeExitInDirection(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 exitPosition)
		{
			exitPosition = getExitDistanceInDirection(origin, direction, maxDistance);
			findGroundForExitPosition(ref exitPosition);
			return isExitPositionEmpty(exitPosition);
		}

		protected bool getExitSidePoint(Vector3 direction, out Vector3 exitPosition)
		{
			float radius = PlayerStance.RADIUS + 0.1f;
			float distance = asset.exit + (Mathf.Abs(ReplicatedSpeed) * 0.1f) + radius;
			Vector3 origin = center.position;
			return getSafeExitInDirection(origin, direction, distance, out exitPosition);
		}

		protected bool getExitUpwardPoint(out Vector3 exitPosition)
		{
			Vector3 origin = center.position;
			Vector3 direction = center.up;

			// We need standing padding from ceiling, so distance is higher.
			exitPosition = getExitDistanceInDirection(origin, direction, 6.0f, extraPadding: PlayerMovement.HEIGHT_STAND);
			findGroundForExitPosition(ref exitPosition);
			if (isExitPositionEmpty(exitPosition))
			{
				return true;
			}

			// Try for helicopter at sky limit or upside-down vehicle.
			exitPosition = getExitDistanceInDirection(origin, Vector3.up, 6.0f, extraPadding: PlayerMovement.HEIGHT_STAND);
			findGroundForExitPosition(ref exitPosition);
			return isExitPositionEmpty(exitPosition);
		}

		protected bool getExitDownwardPoint(out Vector3 exitPosition)
		{
			Vector3 origin = center.position;
			Vector3 direction = -center.up;
			if (getSafeExitInDirection(origin, direction, 6.0f, out exitPosition))
			{
				return true;
			}

			// Try for helicopter at sky limit or upside-down vehicle.
			return getSafeExitInDirection(origin, Vector3.down, 6.0f, out exitPosition);
		}

		protected bool getExitForwardPoint(Vector3 direction, out Vector3 exitPosition)
		{
			float distance = 3.0f + (asset.exit * 2.0f);
			Vector3 origin = center.position;
			return getSafeExitInDirection(origin, direction, distance, out exitPosition);
		}

		/// <summary>
		/// Fallback if there are absolutely no good exit points.
		/// Sets point and angle with a normal player spawnpoint.
		///
		/// Once vehicle is completely surrounded there is no nice way to pick an exit point. Finding
		/// a point upwards is abused to teleport upward into bases, finding an empty capsule nearby is
		/// abused to teleport through walls, so if we're sure there isn't a nice exit point we can
		/// fallback to teleporting them to a safe spawnpoint.
		/// </summary>
		protected void getExitSpawnPoint(Player player, ref Vector3 point, ref byte angle)
		{
			// In arena mode there was an exploit to teleport back to the lobby.
			bool arenaSpawn = Level.info != null &&
				Level.info.type == ELevelType.ARENA &&
				LevelManager.isPlayerInArena(player);

			PlayerSpawnpoint spawnpoint = LevelPlayers.getSpawn(arenaSpawn);
			if (spawnpoint != null)
			{
				point = spawnpoint.point;
				angle = MeasurementTool.angleToByte(angle);
			}
			else
			{
				// Would rarely happen, only if nobody put spawns in the map.
				point = new Vector3(0, 256, 0);
				angle = 0;
			}
		}

#if WITH_EXIT_OVERLAP
		/// <returns>True if anything was hit.</returns>
		protected bool overlapSeatCapsule(byte seatIndex)
		{
			float playerRadius = PlayerStance.RADIUS;
			Transform seatTransform = passengers[seatIndex].seat;
			Vector3 point0 = seatTransform.TransformPoint(new Vector3(0f, playerRadius, 0f));
			Vector3 point1 = seatTransform.TransformPoint(new Vector3(0f, PlayerMovement.HEIGHT_STAND - playerRadius, 0f));
			bool hitAnything = overlapCapsuleIgnoringVehicleAndChildren(point0, point1, playerRadius, RayMasks.BLOCK_EXIT);
#if DRAW_EXIT_GIZMOS
			GizmosUtil.Get().Capsule(point0, point1, playerRadius, hitAnything ? Color.red : Color.green, lifespan: 5f);
#endif // DRAW_EXIT_GIZMOS
			return hitAnything;
		}
#endif // WITH_EXIT_OVERLAP

		/// <returns>True if we can safely exit.</returns>
		internal bool tryGetExit(byte seat, out Vector3 point, out byte angle)
		{
			point = center.position;// + new Vector3(0, 2, 0);
			angle = MeasurementTool.angleToByte(center.rotation.eulerAngles.y);

			// Replaced exit overlap with better center calculation based on volume, but it might prove useful in the future.
#if WITH_EXIT_OVERLAP
			if(overlapSeatCapsule(seat))
			{
				// Center of vehicle may be overlapping an object. Exiting might put player inside the object.
				return false;
			}
#endif // WITH_EXIT_OVERLAP

			Vector3 sideDir = (seat % 2 == 0) ? -center.right : center.right;
			if (getExitSidePoint(sideDir, out point))
			{
				return true;
			}
			sideDir = -sideDir;
			if (getExitSidePoint(sideDir, out point))
			{
				return true;
			}

			if (getExitUpwardPoint(out point))
			{
				return true;
			}

			if (getExitDownwardPoint(out point))
			{
				return true;
			}

			// Forward and backward are lower priority because player might get run over:
			if (getExitForwardPoint(-center.forward, out point))
			{
				return true;
			}

			if (getExitForwardPoint(center.forward, out point))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Initially use tryGetExit to find a safe exit, but if one isn't available then fallback to getExitSpawnPoint.
		/// </summary>
		protected void forceGetExit(Player player, byte seat, out Vector3 point, out byte angle)
		{
			if (!tryGetExit(seat, out point, out angle))
			{
				getExitSpawnPoint(player, ref point, ref angle);
			}
		}

		/// <summary>
		/// Dedicated server simulate driving input.
		/// </summary>
		public void simulate(uint simulation, int recov, bool inputStamina, Vector3 point, Quaternion angle, float newSpeed, float newForwardVelocity, float newSteeringInput, float newVelocityInput, float delta)
		{
			if (asset.useStaminaBoost)
			{
				bool driverHasStamina = passengers[0].player != null && passengers[0].player.player != null && passengers[0].player.player.life.stamina > 0;
				if (inputStamina && driverHasStamina)
				{
					isBoosting = true;
				}
				else
				{
					isBoosting = false;
				}
			}
			else
			{
				isBoosting = false;
			}

			if (isRecovering)
			{
				if (recov < passengers[0].player.player.input.recov)
				{
					if (Time.realtimeSinceStartup - lastRecover > 5.0f)
					{
						lastRecover = Time.realtimeSinceStartup;

						VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);
					}

					return;
				}

				isRecovering = false;
				isFrozen = false;
			}

			bool canTrustClient = Dedicator.serverVisibility == ESteamServerVisibility.LAN || PlayerMovement.forceTrustClient;
			if (canTrustClient) // Cheating on LAN is very obvious, so allow it if they want to
			{

			}
			else
			{
				if (asset.engine == EEngine.CAR)
				{
					if (MathfEx.HorizontalDistanceSquared(point, real) > ((usesFuel && fuel == 0) ? 0.5f : asset.sqrDelta))
					{
						isRecovering = true;
						lastRecover = Time.realtimeSinceStartup;
						passengers[0].player.player.input.recov++;

						VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);

						return;
					}
				}
				else if (asset.engine == EEngine.BOAT)
				{
					if (MathfEx.HorizontalDistanceSquared(point, real) > (SDG.Framework.Water.WaterUtility.isPointUnderwater(point + new Vector3(0, -4, 0)) ? asset.sqrDelta : 0.5f))
					{
						isRecovering = true;
						lastRecover = Time.realtimeSinceStartup;
						passengers[0].player.player.input.recov++;

						VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);

						return;
					}
				}
				else if (asset.engine == EEngine.TRAIN)
				{
					// They can do basically whatever they want right now
				}
				else
				{
					if (MathfEx.HorizontalDistanceSquared(point, real) > asset.sqrDelta)
					{
						isRecovering = true;
						lastRecover = Time.realtimeSinceStartup;
						passengers[0].player.player.input.recov++;

						VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);

						return;
					}
				}

				if (asset.engine != EEngine.TRAIN)
				{
					float validSpeed = (point.y > real.y) ? asset.validSpeedUp : asset.validSpeedDown;
					float approxSpeed = Mathf.Abs(point.y - real.y) / delta;
					if (approxSpeed > validSpeed)
					{
						isRecovering = true;
						lastRecover = Time.realtimeSinceStartup;
						passengers[0].player.player.input.recov++;

						VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);

						return;
					}
				}
			}

			// Perform the adjustment after movement validation otherwise it gets flagged as a quick upward teleport.
			if (asset.engine != EEngine.TRAIN)
			{
				UndergroundAllowlist.AdjustPosition(ref point, 10.0f, threshold: 2.0f);
			}

			simulateBurnFuel();

			// Nelson 2024-07-10: Reported as a bug that vehicles didn't replicate the change when steering input
			// changed. It's a bit overkill but we now replicate the entire state when steering changes.
			bool steeringInputChanged = !MathfEx.IsNearlyEqual(ReplicatedSteeringInput, newSteeringInput, tolerance: 0.5f);

			ReplicatedSpeed = newSpeed;
			ReplicatedForwardVelocity = newForwardVelocity;
			ReplicatedSteeringInput = newSteeringInput;
			ReplicatedVelocityInput = newVelocityInput;
			real = point;

			if (asset.engine == EEngine.TRAIN)
			{
				roadPosition = ClampEngineRoadPosition(UnpackRoadPosition(point));
				TeleportTrainCars(false);
			}
			else
			{
				rootRigidbody.MovePosition(point);
				rootRigidbody.MoveRotation(angle);
			}

			if (steeringInputChanged || Mathf.Abs(lastUpdatedPos.x - real.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.y - real.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.z - real.z) > Provider.UPDATE_DISTANCE)
			{
				lastUpdatedPos = real;
				MarkForReplicationUpdate();
			}
		}

		public void clearHooked()
		{
			foreach (HookInfo info in hooked)
			{
				if (info.vehicle == null)
				{
					continue;
				}

				info.vehicle.isHooked = false;

				// Restore collision between this vehicle and the attached vehicle.
				ignoreCollisionWithVehicle(info.vehicle, false);
			}

			hooked.Clear();
		}

		public void useHook()
		{
			if (hooked.Count > 0)
			{
				clearHooked();
			}
			else
			{
				int grabbed = Physics.OverlapSphereNonAlloc(hook.position, 3.0f, tempCollidersArray, RayMasks.VEHICLE);

				for (int index = 0; index < grabbed; index++)
				{
					InteractableVehicle vehicle = DamageTool.getVehicle(tempCollidersArray[index].transform);

					if (vehicle == null || vehicle == this || !vehicle.isEmpty || vehicle.isHooked || vehicle.isExploded || vehicle.asset.engine == EEngine.TRAIN)
					{
						continue;
					}

					bool shouldAllow = true;

					VehicleBarricadeRegion region = BarricadeManager.findRegionFromVehicle(vehicle);
					if (region != null)
					{
						bool doBarricadesAllowPickup = true;
						foreach (BarricadeDrop drop in region.drops)
						{
							if (drop.asset != null && !drop.asset.CanParentVehicleBePickedUp)
							{
								doBarricadesAllowPickup = false;
								break;
							}
						}

						if (!doBarricadesAllowPickup)
						{
							shouldAllow = false;
						}
					}

					if (OnHookVehicleRequested_Global != null)
					{
						try
						{
							OnHookVehicleRequested_Global.Invoke(this, vehicle, ref shouldAllow);
						}
						catch (System.Exception exception)
						{
							UnturnedLog.exception(exception, "Caught exception in OnHookVehicleRequested_Global:");
						}
					}

					if (!shouldAllow)
					{
						continue;
					}

					vehicle.transform.GetPositionAndRotation(out Vector3 globalPosition, out Quaternion globalRotation);

					HookInfo info = new HookInfo();
#pragma warning disable
					info.target = vehicle.transform;
#pragma warning restore
					info.vehicle = vehicle;
					info.deltaPosition = hook.InverseTransformPoint(globalPosition);
					info.deltaRotation = hook.InverseTransformRotation(globalRotation);
					hooked.Add(info);

					vehicle.isHooked = true;

					// Disable collision between this vehicle and the attached vehicle. This prevent slight overlaps
					// from sending the vehicles flying into space.
					ignoreCollisionWithVehicle(vehicle, true);
				}
			}
		}

		/// <summary>
		/// -1 is reverse.
		/// 0 is neutral.
		/// +1 is index 0 in gear ratios list.
		/// </summary>
		public int GearNumber
		{
			get;
			internal set;
		}
		/// <summary>
		/// Engine RPM replicated by current simulation owner.
		/// </summary>
		public float ReplicatedEngineRpm
		{
			get;
			internal set;
		}
		/// <summary>
		/// Animated toward ReplicatedEngineRpm.
		/// </summary>
		public float AnimatedEngineRpm
		{
			get;
			private set;
		}
		private EngineCurvesComponent engineCurvesComponent;
		private float timeSinceLastGearChange;

		internal float latestGasInput;

		/// <summary>
		/// Called when engine RPM exceeds threshold and there are more gears available.
		/// Purpose is to skip gear numbers that don't bring engine RPM within threshold (if possible).
		/// </summary>
		private int GetShiftUpGearNumber(float averagePoweredWheelRpm)
		{
			Debug.Assert(GearNumber >= 0);
			int testGearIndex = GearNumber; // number-1 is index, and we want to start testing with the *next* gear.
			while (testGearIndex < asset.forwardGearRatios.Length)
			{
				float testRpm = averagePoweredWheelRpm * asset.forwardGearRatios[testGearIndex];
				if (testRpm > asset.GearShiftDownThresholdRpm && testRpm < asset.GearShiftUpThresholdRpm)
				{
					// This gear would put us into the middle RPM range, so it works.
#if LOG_GEAR_SHIFT
					UnturnedLog.info($"Shifting up chose gear {testGearIndex + 1} with estimated RPM of {testRpm}");
#endif
					return testGearIndex + 1;
				}
				else
				{
#if LOG_GEAR_SHIFT
					UnturnedLog.info($"Shifting up skipped gear {testGearIndex + 1} with estimated RPM of {testRpm}");
#endif
				}
				++testGearIndex;
			}

#if LOG_GEAR_SHIFT
			UnturnedLog.info($"Shifting up but no ideal gear to use, defaulting to {gearNumber + 1}");
#endif
			// Default to next gear.
			return GearNumber + 1;
		}

		/// <summary>
		/// Called when engine RPM is below threshold and there are more lower gears available.
		/// Purpose is to skip gear numbers that don't bring engine RPM within threshold (if possible).
		/// </summary>
		private int GetShiftDownGearNumber(float averagePoweredWheelRpm)
		{
			int testGearIndex = GearNumber - 2; // number-1 is index, and we want to start testing with the *next* gear.
			while (testGearIndex >= 0)
			{
				float testRpm = averagePoweredWheelRpm * asset.forwardGearRatios[testGearIndex];
				if (testRpm > asset.GearShiftDownThresholdRpm && testRpm < asset.GearShiftUpThresholdRpm)
				{
					// This gear would put us into the middle RPM range, so it works.
#if LOG_GEAR_SHIFT
					UnturnedLog.info($"Shifting down chose gear {testGearIndex + 1} with estimated RPM of {testRpm}");
#endif
					return testGearIndex + 1;
				}
				else
				{
#if LOG_GEAR_SHIFT
					UnturnedLog.info($"Shifting down skipped gear {testGearIndex + 1} with estimated RPM of {testRpm}");
#endif
				}
				--testGearIndex;
			}

#if LOG_GEAR_SHIFT
			UnturnedLog.info($"Shifting down but no ideal gear to use, defaulting to {gearNumber - 1}");
#endif
			// Default to next gear.
			return GearNumber - 1;
		}

		internal void ChangeGears(int newGearNumber)
		{
			if (GearNumber == newGearNumber)
				return;

#if LOG_GEAR_SHIFT
			UnturnedLog.info($"Shifting gear to {newGearNumber}");
#endif
			timeSinceLastGearChange = 0.0f;
			GearNumber = newGearNumber;
			OnGearChanged?.TryInvoke("OnGearChanged", this);
		}

		/// <summary>
		/// Client simulate driving input.
		/// </summary>
		public void simulate(uint simulation, int recov, int input_x, int input_y, float look_x, float look_y, bool inputBrake, bool inputStamina, float delta)
		{
			// In multiplayer only the dedicated server performs the upward adjustment.
			// Otherwise it gets flagged as a quick upward movement and reverted.
			if (Provider.isServer && asset.engine != EEngine.TRAIN)
			{
				Vector3 adjustedPosition = transform.position;
				if (UndergroundAllowlist.AdjustPosition(ref adjustedPosition, 10.0f, threshold: 2.0f))
				{
					rootRigidbody.MovePosition(adjustedPosition);
				}
			}

			latestGasInput = input_y;

			float inputMod_Y = input_y;
			float boostMultiplier = 1.0f;
			if (asset.useStaminaBoost)
			{
				bool driverHasStamina = passengers[0].player != null && passengers[0].player.player != null && passengers[0].player.player.life.stamina > 0;
				if (inputStamina && driverHasStamina)
				{
					isBoosting = true;
				}
				else
				{
					isBoosting = false;
					inputMod_Y *= asset.staminaBoost;
					boostMultiplier *= asset.staminaBoost;
				}
			}
			else
			{
				isBoosting = false;
			}

			if (isFrozen)
			{
				isFrozen = false;
				rootRigidbody.useGravity = usesGravity;
				rootRigidbody.isKinematic = isKinematic;
				return;
			}

			bool isTorqueBlocked = false;
			if ((usesFuel && fuel == 0) || isUnderwater || isDead || !isEnginePowered)
			{
				inputMod_Y = 0;
				boostMultiplier = 1.0f;
				isTorqueBlocked = true;
			}

			bool anyOnGround = false;

			float steeringLeaningForceMultiplier = asset.steeringLeaningForceMultiplier;
			if (steeringLeaningForceMultiplier > 0.0f)
			{
				if (asset.steeringLeaningForceShouldScaleWithSpeed)
				{
					float normalizedSpeed = GetReplicatedForwardSpeedPercentageOfTargetSpeed();
					steeringLeaningForceMultiplier *= Mathf.Pow(normalizedSpeed, asset.steeringLeaningForceSpeedExponent);
				}

				rootRigidbody.AddRelativeTorque(0.0f, 0.0f, input_x * -steeringLeaningForceMultiplier * PlayerInput.SAMPLES);
			}

			if (_wheels != null)
			{
				foreach (Wheel wheel in _wheels)
				{
					wheel.ClientSimulate(input_x, inputMod_Y, inputBrake, delta, isTorqueBlocked);
					anyOnGround |= wheel.isGrounded;
				}

				if (anyOnGround && asset.wheelBalancingForceMultiplier > 0.0f)
				{
					// Multiply by samples for now because when we increase the tick rate we will need to reduce this force.
					ApplyWheelBalancingForce(Time.fixedDeltaTime * PlayerInput.SAMPLES);
				}
			}

			if (asset.rollAngularVelocityDamping > 0.0f)
			{
				// Multiply by samples for now because when we increase the tick rate we will need to reduce this force.
				ApplyAngularVelocityDamping(Time.fixedDeltaTime * PlayerInput.SAMPLES);
			}

			switch (asset.engine)
			{
				case EEngine.CAR:
				{
					float normalizedSpeed = GetReplicatedForwardSpeedPercentageOfTargetSpeed();

					if (anyOnGround)
					{
						rootRigidbody.AddForce(-transform.up * normalizedSpeed * 40f);
					}

					if (buoyancy != null)
					{
						float carboatSteer = Mathf.Lerp(asset.MaxSteeringAngle, asset.MaxSteeringAngleAtFullSpeed, normalizedSpeed);
						bool isCarboatUnderwater = SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position + new Vector3(0, -1, 0));
						boatTraction = Mathf.Lerp(boatTraction, isCarboatUnderwater ? 1 : 0, 4 * delta);

						if (MathfEx.IsNearlyZero(boatTraction) == false)
						{
							if (inputMod_Y > 0)
							{
								inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetForwardVelocity, delta / 4.0f);
							}
							else if (inputMod_Y < 0)
							{
								inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetReverseVelocity, delta / 4.0f);
							}
							else
							{
								inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 8.0f);
							}
							inputEngineVelocity = inputTargetVelocity * boatTraction;

							Vector3 carboatForward = transform.forward;
							carboatForward.y = 0;

							rootRigidbody.AddForce(carboatForward.normalized * inputEngineVelocity * 2.0f * boatTraction * asset.engineForceMultiplier);
							rootRigidbody.AddRelativeTorque(input_y * -2.5f * boatTraction, input_x * carboatSteer / 8.0f * boatTraction, input_x * -2.5f * boatTraction);
						}
					}

					break;
				}

				case EEngine.PLANE:
				{
					float normalizedSpeed = GetReplicatedForwardSpeedPercentageOfTargetSpeed();
					float planeSteer = Mathf.Lerp(asset.airSteerMax, asset.airSteerMin, normalizedSpeed);

					if (inputMod_Y > 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetForwardVelocity * boostMultiplier, delta);
					}
					else if (inputMod_Y < 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 8.0f);
					}
					else
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 16.0f);
					}
					inputEngineVelocity = inputTargetVelocity;

					rootRigidbody.AddForce(transform.forward * inputEngineVelocity * 2.0f * asset.engineForceMultiplier);
					rootRigidbody.AddForce(Mathf.Lerp(0.0f, 1.0f, transform.InverseTransformDirection(rootRigidbody.velocity).z / asset.TargetForwardVelocity) * asset.lift * -Physics.gravity);

					if (_wheels == null || _wheels.Length == 0 || (!_wheels[0].isGrounded && !_wheels[1].isGrounded))
					{
						rootRigidbody.AddRelativeTorque(Mathf.Clamp(look_y, -asset.airTurnResponsiveness, asset.airTurnResponsiveness) * planeSteer, input_x * asset.airTurnResponsiveness * planeSteer / 4.0f, Mathf.Clamp(look_x, -asset.airTurnResponsiveness, asset.airTurnResponsiveness) * -planeSteer / 2.0f);
					}

					if ((_wheels == null || _wheels.Length == 0) && inputMod_Y < 0)
					{
						rootRigidbody.AddForce(transform.forward * asset.TargetReverseVelocity * 4.0f * asset.engineForceMultiplier);
					}

					break;
				}

				case EEngine.HELICOPTER:
				{
					float normalizedSpeed = GetReplicatedForwardSpeedPercentageOfTargetSpeed();
					float heliSteer = Mathf.Lerp(asset.MaxSteeringAngle, asset.MaxSteeringAngleAtFullSpeed, normalizedSpeed);

					if (inputMod_Y > 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetForwardVelocity * boostMultiplier, delta / 4.0f);
					}
					else if (inputMod_Y < 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 8.0f);
					}
					else
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 16.0f);
					}
					inputEngineVelocity = inputTargetVelocity;

					rootRigidbody.AddForce(transform.up * inputEngineVelocity * 3.0f);
					rootRigidbody.AddRelativeTorque(Mathf.Clamp(look_y, -2.0f, 2.0f) * heliSteer, input_x * heliSteer / 2.0f, Mathf.Clamp(look_x, -2.0f, 2.0f) * -heliSteer / 4.0f);

					break;
				}

				case EEngine.BLIMP:
				{
					float normalizedSpeed = GetReplicatedForwardSpeedPercentageOfTargetSpeed();
					float blimpSteer = Mathf.Lerp(asset.MaxSteeringAngle, asset.MaxSteeringAngleAtFullSpeed, normalizedSpeed);

					if (inputMod_Y > 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetForwardVelocity * boostMultiplier, delta / 4.0f);
					}
					else if (inputMod_Y < 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetReverseVelocity * boostMultiplier, delta / 4.0f);
					}
					else
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 8.0f);
					}
					inputEngineVelocity = inputTargetVelocity;

					rootRigidbody.AddForce(transform.forward * inputEngineVelocity * 2.0f);

					if (!isBlimpFloating)
					{
						rootRigidbody.AddForce(-Physics.gravity * 0.5f);
					}

					rootRigidbody.AddRelativeTorque(Mathf.Clamp(look_y, -asset.airTurnResponsiveness, asset.airTurnResponsiveness) * blimpSteer / 4.0f, input_x * asset.airTurnResponsiveness * blimpSteer * 2.0f, Mathf.Clamp(look_x, -asset.airTurnResponsiveness, asset.airTurnResponsiveness) * -blimpSteer / 4.0f);

					break;
				}

				case EEngine.BOAT:
				{
					float normalizedSpeed = GetReplicatedForwardSpeedPercentageOfTargetSpeed();
					float boatSteer = Mathf.Lerp(asset.MaxSteeringAngle, asset.MaxSteeringAngleAtFullSpeed, normalizedSpeed);
					boatTraction = Mathf.Lerp(boatTraction, SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position + new Vector3(0, -1, 0)) ? 1 : 0, 4 * delta);

					if (inputMod_Y > 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetForwardVelocity * boostMultiplier, delta / 4.0f);
					}
					else if (inputMod_Y < 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetReverseVelocity * boostMultiplier, delta / 4.0f);
					}
					else
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 8.0f);
					}
					inputEngineVelocity = inputTargetVelocity * boatTraction;

					Vector3 boatForward = transform.forward;
					boatForward.y = 0;

					rootRigidbody.AddForce(boatForward.normalized * inputEngineVelocity * 4.0f * boatTraction * asset.engineForceMultiplier);

					if (_wheels == null || _wheels.Length == 0 || (!_wheels[0].isGrounded && !_wheels[1].isGrounded))
					{
						rootRigidbody.AddRelativeTorque(inputMod_Y * -10.0f * boatTraction, input_x * boatSteer / 2.0f * boatTraction, input_x * -5.0f * boatTraction);
					}

					break;
				}

				case EEngine.TRAIN:
				{
					if (inputMod_Y > 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetForwardVelocity * boostMultiplier, delta / 8.0f);
					}
					else if (inputMod_Y < 0)
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, asset.TargetReverseVelocity * boostMultiplier, delta / 8.0f);
					}
					else
					{
						inputTargetVelocity = Mathf.Lerp(inputTargetVelocity, 0, delta / 8.0f);
					}
					inputEngineVelocity = inputTargetVelocity;

					break;
				}
			}

			if (asset.engine == EEngine.TRAIN)
			{
				ReplicatedSpeed = Mathf.Abs(inputEngineVelocity);
				ReplicatedForwardVelocity = inputEngineVelocity;
				ReplicatedVelocityInput = inputEngineVelocity;
			}
			else
			{
				Vector3 worldVelocity = rootRigidbody.velocity;
				ReplicatedSpeed = worldVelocity.magnitude;
				Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);
				if (asset.engine == EEngine.HELICOPTER)
				{
					ReplicatedForwardVelocity = localVelocity.y;
				}
				else
				{
					ReplicatedForwardVelocity = localVelocity.z;
				}
				ReplicatedVelocityInput = inputEngineVelocity;
			}

			ReplicatedSteeringInput = input_x;

			simulateBurnFuel();

			lastUpdatedPos = transform.position;

			interpTargetPosition = transform.position;
			interpTargetRotation = transform.rotation;
		}

		private void MoveTrainCar(Vector3 frontPosition, Vector3 frontNormal, Vector3 frontDirection, Vector3 backPosition, Vector3 backNormal, Vector3 backDirection, TrainCar car, bool forceSetTransform)
		{
			Vector3 vehiclePosition = (frontPosition + backPosition) * 0.5f;
			Vector3 vehicleNormal = Vector3.Lerp(backNormal, frontNormal, 0.5f).normalized;
			Vector3 vehicleDirection = (frontPosition - backPosition).normalized;
			Quaternion vehicleRotation = Quaternion.LookRotation(vehicleDirection, vehicleNormal);

			Vector3 rootPosition = vehiclePosition + (vehicleNormal * asset.trainTrackOffset);

			// Refer to UpdateNonTrainInterpolatedTransform comment
			if (car.rootRigidbody != null)
			{
				// We assert rootRigidbody exists, but keep this for backwards compatibility with old train mods.
				car.rootRigidbody.MovePosition(rootPosition);
				car.rootRigidbody.MoveRotation(vehicleRotation);
			}
			if (car.root != null && (forceSetTransform || car.rootRigidbody == null))
			{
				car.root.SetPositionAndRotation(rootPosition, vehicleRotation);
			}

			Matrix4x4 objectsToWorld = Matrix4x4.TRS(rootPosition, vehicleRotation, transform.localScale) * car.objectsToRoot;
			Matrix4x4 worldToObjects = objectsToWorld.inverse;

			if (car.trackFront != null)
			{
				Vector3 localDir = worldToObjects.MultiplyVector(frontDirection);
				Vector3 localUp = worldToObjects.MultiplyVector(frontNormal);
				Quaternion frontRotation = Quaternion.LookRotation(localDir, localUp);

				Vector3 localPos = vehiclePosition + (vehicleDirection * asset.trainWheelOffset);
				localPos = worldToObjects.MultiplyPoint(localPos);

				car.trackFront.SetLocalPositionAndRotation(localPos, frontRotation);
			}

			if (car.trackBack != null)
			{
				Vector3 localDir = worldToObjects.MultiplyVector(backDirection);
				Vector3 localUp = worldToObjects.MultiplyVector(backNormal);
				Quaternion backRotation = Quaternion.LookRotation(localDir, localUp);

				Vector3 localPos = vehiclePosition - (vehicleDirection * asset.trainWheelOffset);
				localPos = worldToObjects.MultiplyPoint(localPos);

				car.trackBack.SetLocalPositionAndRotation(localPos, backRotation);
			}
		}

		/// <summary>
		/// 2026-01-30: adding forceSetTransform to work around an issue with newly-spawned trains:
		/// It seems after Unity 2022 LTS or 3.26.1.0 the position passed to Instantiate takes
		/// priority over the call to rigidbody SetPosition, and new trains default to zero. This
		/// prevents players from entering the train.
		/// </summary>
		private void TeleportTrainCars(bool forceSetTransform)
		{
			foreach (TrainCar car in trainCars)
			{
				Vector3 targetFrontPosition;
				Vector3 targetFrontNormal;
				Vector3 targetFrontDirection;
				road.getTrackData(ClampCarRoadPosition(roadPosition + car.trackPositionOffset + asset.trainWheelOffset), out targetFrontPosition, out targetFrontNormal, out targetFrontDirection);

				car.currentFrontPosition = targetFrontPosition;
				car.currentFrontNormal = targetFrontNormal;
				car.currentFrontDirection = targetFrontDirection;

				Vector3 targetBackPosition;
				Vector3 targetBackNormal;
				Vector3 targetBackDirection;
				road.getTrackData(ClampCarRoadPosition(roadPosition + car.trackPositionOffset - asset.trainWheelOffset), out targetBackPosition, out targetBackNormal, out targetBackDirection);

				car.currentBackPosition = targetBackPosition;
				car.currentBackNormal = targetBackNormal;
				car.currentBackDirection = targetBackDirection;

				MoveTrainCar(targetFrontPosition, targetFrontNormal, targetFrontDirection, targetBackPosition, targetBackNormal, targetBackDirection, car, forceSetTransform);
			}
		}

		private TrainCar getTrainCar(Transform root)
		{
			TrainCar car = new TrainCar();
			car.root = root;

			Transform carObjects = root.Find("Objects");
			if (carObjects != null)
			{
				car.objectsToRoot = root.worldToLocalMatrix * carObjects.localToWorldMatrix;

				car.trackFront = carObjects.Find("Track_Front");
				car.trackBack = carObjects.Find("Track_Back");
			}

			car.rootRigidbody = root.GetComponent<Rigidbody>();

			if (car.rootRigidbody != null)
			{
				// Train positions are updated every Update, unfortunately.
				car.rootRigidbody.interpolation = RigidbodyInterpolation.None;
			}

			Debug.Assert(car.rootRigidbody != null);
			return car;
		}

		private float ClampCarRoadPosition(float newRoadPosition)
		{
			if (road.isLoop)
			{
				if (newRoadPosition < 0.0f)
				{
					return road.trackSampledLength + newRoadPosition;
				}
				else if (newRoadPosition > road.trackSampledLength)
				{
					return newRoadPosition - road.trackSampledLength;
				}
				else
				{
					return newRoadPosition;
				}
			}
			else
			{
				return Mathf.Clamp(newRoadPosition, 0.01f, road.trackSampledLength - 0.01f);
			}
		}

		private float ClampEngineRoadPosition(float newRoadPosition)
		{
			if (road.isLoop)
			{
				if (newRoadPosition < 0.0f)
				{
					return road.trackSampledLength + newRoadPosition;
				}
				else if (newRoadPosition > road.trackSampledLength)
				{
					return newRoadPosition - road.trackSampledLength;
				}
				else
				{
					return newRoadPosition;
				}
			}
			else
			{
				return Mathf.Clamp(newRoadPosition, 0.5f + asset.trainWheelOffset + ((trainCars.Length - 1) * asset.trainCarLength), road.trackSampledLength - asset.trainWheelOffset - 0.5f);
			}
		}

		public bool hasDefaultCenterOfMass;
		public Vector3 defaultCenterOfMass;

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.TransformPoint(defaultCenterOfMass), 0.2f);

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(rootRigidbody.worldCenterOfMass, 0.2f);

			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(center.position, 0.2f);
		}

		// Disable warning that these properties are unset.
#pragma warning disable 0649
		public bool overrideCenterOfMass;
		public Vector3 centerOfMassOverride;
#pragma warning restore 0649

#endif // UNITY_EDITOR

#if DEDICATED_SERVER
		internal const bool isVisibleToLocalPlayer = false;
#else
		internal bool isVisibleToLocalPlayer;
		internal float accumulatedDeltaTime;
#endif
		/// <summary>
		/// Nelson 2025-05-05: ran into a bug where our manual OnUpdate is called before Unity calls Start!
		/// </summary>
		internal bool hasUnityCalledStart;

		/// <summary>
		/// Nelson 2025-05-02: keeping the previous comment from 2020-11-26 here. At first I wondered if 24 vehicles
		/// wasn't enough to properly test, but even with a higher vehicle count it can seemingly be *slower* to
		/// call Update manually. That said, calling Update manually does give us the option to time-slice vehicle
		/// updates. On the client and singleplayer we now update vehicles outside render distance at a lower
		/// frequency which saves ~0.1 ms per frame on my PC.
		/// 
		/// 2020-11-26 experimented with dispatching all vehicle updates from C# in VehicleManager because they make up
		/// a significant portion of the MonoBehaviour Update, but the savings on my PC with 24 vehicles on PEI was
		/// minor. Not worth the potential troubles.
		/// </summary>
		internal void OnUpdate(float deltaTime)
		{
			if (asset == null)
			{
				return;
			}

#if UNITY_EDITOR
			if (overrideCenterOfMass)
			{
				rootRigidbody.centerOfMass = centerOfMassOverride;
			}
#endif // UNITY_EDITOR

			if (Provider.isServer && hooked != null)
			{
				BeginSample("UpdateHookedVehicleTransforms");
				UpdateHookedVehicleTransforms();
				EndSample();
			}

#pragma warning disable
			if (Provider.isServer && !needsReplicationUpdate && updates != null && updates.Count > 0)
			{
				updates.Clear();
#pragma warning restore
				MarkForReplicationUpdate();
			}

			if (Dedicator.IsDedicatedServer)
			{
				if (isPhysical)
				{
					if (!needsReplicationUpdate)
					{
						if (Mathf.Abs(lastUpdatedPos.x - transform.position.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.y - transform.position.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.z - transform.position.z) > Provider.UPDATE_DISTANCE)
						{
							lastUpdatedPos = transform.position;
							MarkForReplicationUpdate();
						}
					}
				}
			}
			else
			{
				float maxSteeringAngleMagnitude = Mathf.Lerp(asset.MaxSteeringAngle, asset.MaxSteeringAngleAtFullSpeed, GetReplicatedForwardSpeedPercentageOfTargetSpeed());
				float targetSteeringAngle = ReplicatedSteeringInput * maxSteeringAngleMagnitude;
				float steeringAngleMaxDelta = asset.SteeringAngleTurnSpeed * deltaTime;
				AnimatedSteeringAngle = Mathf.MoveTowards(AnimatedSteeringAngle, targetSteeringAngle, steeringAngleMaxDelta);

				// 1 is 50% per second, 2 is 75%/s, 3 is 87.5%/s, etc.
				const float BLEND_SPEED = 13.0f;
				float lerpWeight = 1.0f - Mathf.Pow(2.0f, -BLEND_SPEED * deltaTime);
				AnimatedForwardVelocity = Mathf.Lerp(AnimatedForwardVelocity, ReplicatedForwardVelocity, lerpWeight);
				AnimatedVelocityInput = Mathf.Lerp(AnimatedVelocityInput, ReplicatedVelocityInput, lerpWeight);
				AnimatedEngineRpm = Mathf.Lerp(AnimatedEngineRpm, ReplicatedEngineRpm, lerpWeight);

				if (!isExploded && isVisibleToLocalPlayer)
				{
					if (isDriven)
					{
						// Propellers spin slightly at zero speed unless engine is off.
						propellerRotationDegrees += (AnimatedVelocityInput + (isEnginePowered ? 8.0f : 0.0f)) * 89.0f * deltaTime;
						propellerRotationDegrees %= 360.0f;
					}

					if (_wheels != null)
					{
						// Update crawler track *before* wheels because some wheels use the crawler track speed.
						if (crawlerTrackMaterials != null && crawlerTrackMaterials.Count > 0)
						{
							BeginSample("UpdateCrawlerTrackTilingMaterials");
							UpdateCrawlerTrackTilingMaterials(deltaTime);
							EndSample();
						}

						BeginSample("WheelUpdateModel");
						foreach (Wheel wheel in _wheels)
						{
							if (wheel.model == null)
							{
								continue;
							}

							wheel.UpdateModel(deltaTime);
						}
						EndSample();
					}

					if (frontModelTransform != null)
					{
						Vector3 frontModelTurnAxis = frontModelRestLocalRotation * new Vector3(0.0f, 0.0f, 1.0f);
						frontModelTransform.localRotation = Quaternion.AngleAxis(AnimatedSteeringAngle, frontModelTurnAxis) * frontModelRestLocalRotation;
					}

					if (propellerModels != null && propellerModels.Length > 0)
					{
						BeginSample("UpdatePropellerVisuals");
						UpdatePropellerVisuals();
						EndSample();
					}

					if (exhaustParticleSystems != null)
					{
						BeginSample("UpdateExhaustParticles");
						UpdateExhaustParticles();
						EndSample();
					}

					if (steeringWheelModelTransform != null)
					{
						Vector3 steeringWheelTurnAxis = steeringWheelRestLocalRotation * new Vector3(0.0f, -1.0f, 0.0f);
						steeringWheelModelTransform.localRotation = Quaternion.AngleAxis(AnimatedSteeringAngle, steeringWheelTurnAxis) * steeringWheelRestLocalRotation;
					}

					if (pedalLeft != null && pedalRight != null)
					{
						BeginSample("UpdateBicyclePedals");
						UpdateBicyclePedals();
						EndSample();
					}
				}

				if (windZone != null && (isDriven && !isUnderwater))
				{
					float windZoneSpeed;
					if (asset.engine == EEngine.CAR || asset.engine == EEngine.BOAT)
					{
						windZoneSpeed = Mathf.Abs(AnimatedForwardVelocity);
					}
					else
					{
						windZoneSpeed = Mathf.Abs(AnimatedVelocityInput);
					}

					if (asset.engine == EEngine.HELICOPTER)
					{
						windZone.windMain = Mathf.Lerp(windZone.windMain, isEnginePowered ? windZoneSpeed * 0.1f : 0, 0.125f * deltaTime);
					}
					else if (asset.engine == EEngine.BLIMP)
					{
						windZone.windMain = Mathf.Lerp(windZone.windMain, isEnginePowered ? windZoneSpeed * 0.5f : 0, 0.125f * deltaTime);
					}
				}
			}

			if (!Provider.isServer && !isPhysical)
			{
				if (asset.engine != EEngine.TRAIN)
				{
					BeginSample("UpdateNonTrainInterpolatedTransform");
					UpdateNonTrainInterpolatedTransform(deltaTime);
					EndSample();
				}
			}

			// If server-side physics simulation causes vehicle to fall through the ground then pull it back up.
			if (Provider.isServer && isPhysical && asset.engine != EEngine.TRAIN && !isDriven)
			{
				Vector3 adjustedPosition = transform.position;
				if (UndergroundAllowlist.AdjustPosition(ref adjustedPosition, 10.0f, threshold: 2.0f))
				{
					rootRigidbody.MovePosition(adjustedPosition);
				}
			}

			if (headlightsOn)
			{
				if (!canTurnOnLights)
				{
					tellHeadlights(false);
				}
			}

			if (sirensOn)
			{
				if (!canTurnOnLights)
				{
					tellSirens(false);
				}
			}

			if (isUnderwater)
			{
				if (!isDrowned)
				{
					_lastUnderwater = Time.realtimeSinceStartup;
					_isDrowned = true;
					OnIsDrownedChanged?.Invoke();

					tellSirens(false);
					tellBlimp(false);
					tellHeadlights(false);
					updateFires();

					if (!Dedicator.IsDedicatedServer)
					{
						if (windZone != null)
						{
							windZone.windMain = 0;
						}
					}
				}
			}
			else
			{
				if (_isDrowned)
				{
					_isDrowned = false;
					OnIsDrownedChanged?.Invoke();

					updateFires();
				}
			}

			// Handles turning off taillights if battery changed or entered/exited water.
			synchronizeTaillights();

			if (isDriver)
			{
				// Nelson 2025-04-22: previously, "slip" was updated here as well. However, this value is only used by
				// the wheels, so I've moved it into UpdateLocallyDrivenWheelPhysicsAndGears.

				if (_wheels != null)
				{
					BeginSample("UpdateLocallyDrivenWheelPhysicsAndGears");
					UpdateLocallyDrivenWheelPhysicsAndGears(deltaTime);
					EndSample();
				}

				if (asset.engine == EEngine.TRAIN && road != null)
				{
					BeginSample("UpdateLocallyDrivenTrainPhysics");
					UpdateLocallyDrivenTrainPhysics(deltaTime);
					EndSample();
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer && road != null)
				{
					// Nelson 2025-04-22: I had a moment of panic like "what! train isn't moved on the server?" but in
					// that case it "teleports" when player input is received.
					BeginSample("UpdateTrainCarTransforms");
					UpdateTrainCarTransforms(deltaTime);
					EndSample();
				}
			}

			if (Provider.isServer)
			{
				if (isDriven)
				{
					if (asset != null && asset.canTiresBeDamaged)
					{
						if (_wheels != null)
						{
							BeginSample("CheckForTraps");
							foreach (Wheel wheel in _wheels)
							{
								if (wheel.wheel == null || wheel.IsDead)
								{
									continue;
								}

								wheel.CheckForTraps();
							}
							EndSample();
						}
					}
				}
				else
				{
					ReplicatedSpeed = rootRigidbody.velocity.magnitude;
					ReplicatedForwardVelocity = transform.InverseTransformDirection(rootRigidbody.velocity).z;
					ReplicatedSteeringInput = 0.0f;
					ReplicatedVelocityInput = 0.0f;

					real = transform.position;
				}

				if (isDead && !isExploded && !isUnderwater && Time.realtimeSinceStartup - lastDead > EXPLODE)
				{
					explode();
				}
			}

			if (!Provider.isServer && !isPhysical)
			{
				if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME * 2)
				{
					lastTick = Time.realtimeSinceStartup;

					// Nelson 2024-07-22: Previously, this would also reset ReplicatedSteeringInput, but this was
					// changed to preserve steering angle while car is idle. (public issue #4574) I believe it should
					// still be safe to assume the car is at zero speed when it hasn't moved recently.
					ReplicatedSpeed = 0.0f;
					ReplicatedForwardVelocity = 0.0f;
					ReplicatedVelocityInput = 0.0f;
				}
			}

			if (sirensOn && !Dedicator.IsDedicatedServer)
			{
				UpdateSirenVisuals();
			}

			if (usesBattery)
			{
				UpdatePredictedBatteryCharge(deltaTime);
			}

			// Safezone timer
			if (Provider.isServer)
			{
				BeginSample("UpdateSafezoneStatus");
				UpdateSafezoneStatus(deltaTime);
				EndSample();
			}
		}

		private void FixedUpdate()
		{
			if (!isPhysical || isDriven || !Provider.isServer)
			{
				return;
			}

			bool anyWheelOnGround = false;

			if (Dedicator.IsDedicatedServer)
			{
				// Dedicated server only needs to update suspension state if enabled for the vehicle.
				if (asset.replicatedWheelIndices != null)
				{
					foreach (int wheelIndex in asset.replicatedWheelIndices)
					{
						Wheel wheel = GetWheelAtIndex(wheelIndex);
						if (wheel == null)
						{
							UnturnedLog.error($"\"{asset.FriendlyName}\" missing wheel for replicated index: {wheelIndex}");
							continue;
						}

						wheel.UpdateServerSuspensionAndPhysicsMaterial();
						anyWheelOnGround |= wheel.isGrounded;
					}
				}
			}
			else
			{
				// Listen server
				if (_wheels != null)
				{
					foreach (Wheel wheel in _wheels)
					{
						wheel.UpdateGrounded();
						anyWheelOnGround |= wheel.isGrounded;
					}
				}
			}

			if (_wheels != null)
			{
				if (anyWheelOnGround && asset.wheelBalancingForceMultiplier > 0.0f)
				{
					ApplyWheelBalancingForce(Time.fixedDeltaTime);
				}
			}

			if (asset.rollAngularVelocityDamping > 0.0f)
			{
				ApplyAngularVelocityDamping(Time.fixedDeltaTime);
			}
		}

		private void UpdateHookedVehicleTransforms()
		{
			foreach(HookInfo info in hooked)
			{
				if (info == null || info.vehicle == null)
				{
					continue;
				}

				Vector3 newGlobalPosition = hook.TransformPoint(info.deltaPosition);
				Quaternion newGlobalRotation = hook.rotation * info.deltaRotation;

				if (info.vehicle.rootRigidbody != null)
				{
					info.vehicle.rootRigidbody.MovePosition(newGlobalPosition);
					info.vehicle.rootRigidbody.MoveRotation(newGlobalRotation);
				}
				else
				{
					info.vehicle.transform.SetPositionAndRotation(newGlobalPosition, newGlobalRotation);
				}
			}
		}

		private void UpdatePropellerVisuals()
		{
			Quaternion propellerRotationOffset = Quaternion.AngleAxis(propellerRotationDegrees, Vector3.up);
			float bladeAlpha;
			if (isDriven)
			{
				if (asset.engine == EEngine.PLANE)
				{
					bladeAlpha = Mathf.Lerp(1.0f, 0.0f, (AnimatedVelocityInput - 16) / 8.0f);
				}
				else
				{
					bladeAlpha = Mathf.Lerp(1.0f, 0.0f, (AnimatedVelocityInput - 8) / 8.0f);
				}
			}
			else
			{
				bladeAlpha = 1.0f;
			}

			bool shouldPropellerMotionBlurBeEnabled = bladeAlpha < 0.99999f;
			if (isPropellerMotionBlurEnabled != shouldPropellerMotionBlurBeEnabled)
			{
				isPropellerMotionBlurEnabled = shouldPropellerMotionBlurBeEnabled;
				foreach (PropellerModel model in propellerModels)
				{
					if (model.motionBlurRenderer != null)
					{
						model.motionBlurRenderer.enabled = isPropellerMotionBlurEnabled;
					}
				}
			}

			foreach (PropellerModel model in propellerModels)
			{
				if (model == null || model.transform == null || model.bladeMaterial == null || model.motionBlurMaterial == null)
				{
					break;
				}

				model.transform.localRotation = model.baseLocationRotation * propellerRotationOffset;

				Color color = model.bladeMaterial.color;
				color.a = bladeAlpha;
				model.bladeMaterial.color = color;
				color.a = (1.0f - color.a) * 0.25f;
				model.motionBlurMaterial.color = color;
			}
		}

		private void UpdateExhaustParticles()
		{
			float exhaust = MathfEx.IsNearlyZero(AnimatedForwardVelocity, 0.04f) ? 0.0f : Mathf.Max(0.0f, Mathf.InverseLerp(0, asset.TargetForwardVelocity, AnimatedForwardVelocity));
			if (exhaust > 0.0f)
			{
				if (!isExhaustGameObjectActive)
				{
					exhaustGameObject.SetActive(true);
					isExhaustGameObjectActive = true;
				}

				foreach (ParticleSystem ps in exhaustParticleSystems)
				{
					ParticleSystem.EmissionModule emission = ps.emission;
					emission.rateOverTime = ps.main.maxParticles * exhaust;
				}
				isExhaustRateOverTimeZero = false;
			}
			else if (isExhaustGameObjectActive)
			{
				if (!isExhaustRateOverTimeZero)
				{
					SetExhaustParticleSystemsRateOverTimeToZero();
				}

				// Wait until particle systems finish before deactivating.
				bool isAnyParticleSystemActive = false;
				foreach (ParticleSystem ps in exhaustParticleSystems)
				{
					if (ps.particleCount > 0)
					{
						isAnyParticleSystemActive = true;
						break;
					}
				}

				if (!isAnyParticleSystemActive)
				{
					exhaustGameObject.SetActive(false);
					isExhaustGameObjectActive = false;
				}
			}
		}

		/// <summary>
		/// Nelson 2025-04-22: it hopefully goes without saying the bicycle pedals are janky as heck, I'm just separating
		/// out the Update method to make profiling it easier.
		/// </summary>
		private void UpdateBicyclePedals()
		{
			if (passengers[0].player != null && passengers[0].player.player != null)
			{
				Transform skeleton = passengers[0].player.player.animator.thirdSkeleton;
				Transform footLeft = skeleton.Find("Left_Hip").Find("Left_Leg").Find("Left_Foot");
				Transform footRight = skeleton.Find("Right_Hip").Find("Right_Leg").Find("Right_Foot");

				if (passengers[0].player.IsLeftHanded)
				{
					pedalLeft.position = footRight.position + (footRight.right * 0.325f);
					pedalRight.position = footLeft.position + (footLeft.right * 0.325f);
				}
				else
				{
					pedalLeft.position = footLeft.position + (footLeft.right * -0.325f);
					pedalRight.position = footRight.position + (footRight.right * -0.325f);
				}
			}
		}

		private void UpdateNonTrainInterpolatedTransform(float deltaTime)
		{
			Vector3 currentPosition;
			Quaternion currentRotation;
			transform.GetPositionAndRotation(out currentPosition, out currentRotation);

			// 1 is 50% per second, 2 is 75%/s, 3 is 87.5%/s, etc.
			const float BLEND_SPEED = 13.0f;
			float lerpWeight = 1.0f - Mathf.Pow(2.0f, -BLEND_SPEED * deltaTime);

			Vector3 newPosition = Vector3.Lerp(currentPosition, interpTargetPosition, lerpWeight);
			Quaternion newRotation = Quaternion.Slerp(currentRotation, interpTargetRotation, lerpWeight);

			// Nelson 2024-07-10: In the vehicle update I changed this to SetPositionAndRotation which broke
			// some physics effects modders were relying on client-side. (public issue #4573)
			// Come to think of it, this is also probably benefical to prevent players from driving vehicles
			// through vehicles driven by other players. Maybe I was wrong about public issue #4565?
			rootRigidbody.MovePosition(newPosition);
			rootRigidbody.MoveRotation(newRotation);
		}

		/// <summary>
		/// Nelson 2025-04-22: this should ideally be moved into FixedUpdate, incorrect to run in Update.
		/// </summary>
		private void UpdateLocallyDrivenWheelPhysicsAndGears(float deltaTime)
		{
			if (!asset.hasTraction)
			{
				bool inSnow = LevelLighting.isPositionSnowy(transform.position);
				if (!inSnow && Level.info != null && Level.info.configData.Use_Snow_Volumes)
				{
					SDG.Framework.Devkit.AmbianceVolume ambianceVolume = SDG.Framework.Devkit.AmbianceVolumeManager.Get().GetFirstOverlappingVolume(transform.position);
					if (ambianceVolume != null)
					{
						inSnow = (ambianceVolume.weatherMask & (1U << 1)) != 0;
					}
				}
				inSnow &= LevelLighting.snowyness == ELightingSnow.BLIZZARD;

				_slip = Mathf.Lerp(_slip, inSnow ? 1 : 0, deltaTime * 0.05f);
			}
			else
			{
				_slip = 0f;
			}

			float averagePoweredWheelRpm = 0.0f;
			int numPoweredWheels = 0;
			if (asset.poweredWheelIndices != null)
			{
				float totalPoweredWheelRpm = 0.0f;
				bool includeUngroundedWheels = asset.ShouldIncludeAirbornWheelsInAverageRpm;
				foreach (int index in asset.poweredWheelIndices)
				{
					Wheel wheel = GetWheelAtIndex(index);
					if (wheel == null || wheel.wheel == null)
					{
						continue;
					}

					if (wheel.isGrounded || includeUngroundedWheels)
					{
						totalPoweredWheelRpm += Mathf.Abs(wheel.wheel.rpm);
						++numPoweredWheels;
					}
				}

				if (numPoweredWheels > 0)
				{
					averagePoweredWheelRpm = totalPoweredWheelRpm / numPoweredWheels;
				}
			}

			float expectedWheelRpm = ReplicatedEngineRpm;
			if (GearNumber == -1)
			{
				expectedWheelRpm = averagePoweredWheelRpm * asset.reverseGearRatio;
			}
			else if (asset.UsesEngineRpmAndGears && GearNumber >= 1 && GearNumber <= asset.forwardGearRatios.Length)
			{
				expectedWheelRpm = averagePoweredWheelRpm * asset.forwardGearRatios[GearNumber - 1];
			}
			float rpmMismatch = expectedWheelRpm - ReplicatedEngineRpm;

			if (Wheel.clEnableWheeledVehicleGizmos)
			{
				string debugText = $"Avg pwd wheel RPM: {averagePoweredWheelRpm:N1}";
				debugText += $"\nExpected wheel RPM: {expectedWheelRpm:N1}";
				RuntimeGizmos.Get().Label(transform.position, debugText);
			}
			
			float newEngineRpm = averagePoweredWheelRpm;
			if (asset.UsesEngineRpmAndGears)
			{
				timeSinceLastGearChange += deltaTime;
				if (timeSinceLastGearChange > asset.GearShiftInterval)
				{
					if (latestGasInput < -0.01f && ReplicatedForwardVelocity < 0.05f)
					{
						ChangeGears(-1);
					}
					else
					{
						if (GearNumber < 1 && ReplicatedForwardVelocity > -0.05f)
						{
							ChangeGears(1);
						}
						else if (ReplicatedEngineRpm > asset.GearShiftUpThresholdRpm && GearNumber > 0 && GearNumber < asset.forwardGearRatios.Length)
						{
							bool allow = true;
							if (asset.EngineRpmMismatchGearShiftPreventShifting)
							{
								allow = rpmMismatch >= asset.EngineRpmMismatchGearShiftUpMinThreshold
									&& rpmMismatch <= asset.EngineRpmMismatchGearShiftUpMaxThreshold;
							}

							if (allow)
							{
								int nextGear = asset.GearShiftAllowSkippingGears
									? GetShiftUpGearNumber(averagePoweredWheelRpm)
									: GearNumber + 1;

								ChangeGears(nextGear);
							}
						}
						else if (ReplicatedEngineRpm < asset.GearShiftDownThresholdRpm && GearNumber > 1)
						{
							bool allow = true;
							if (asset.EngineRpmMismatchGearShiftPreventShifting)
							{
								allow = rpmMismatch >= asset.EngineRpmMismatchGearShiftDownMinThreshold
									&& rpmMismatch <= asset.EngineRpmMismatchGearShiftDownMaxThreshold;
							}

							if (allow)
							{
								int nextGear = asset.GearShiftAllowSkippingGears
									? GetShiftDownGearNumber(averagePoweredWheelRpm)
									: GearNumber - 1;

								ChangeGears(nextGear);
							}
						}
					}
				}

				if (GearNumber == -1)
				{
					newEngineRpm *= asset.reverseGearRatio;
				}
				else if (GearNumber >= 1 && GearNumber <= asset.forwardGearRatios.Length)
				{
					newEngineRpm *= asset.forwardGearRatios[GearNumber - 1];
				}
				newEngineRpm = Mathf.Max(newEngineRpm, asset.EngineIdleRpm);
			}

			if (newEngineRpm > ReplicatedEngineRpm)
			{
				if (asset.EngineRpmIncreaseRate > 0.001f)
				{
					ReplicatedEngineRpm = Mathf.MoveTowards(ReplicatedEngineRpm, newEngineRpm, asset.EngineRpmIncreaseRate * deltaTime);
				}
				else
				{
					ReplicatedEngineRpm = newEngineRpm;
				}
			}
			else if (newEngineRpm < ReplicatedEngineRpm)
			{
				if (asset.EngineRpmDecreaseRate > 0.001f)
				{
					ReplicatedEngineRpm = Mathf.MoveTowards(ReplicatedEngineRpm, newEngineRpm, asset.EngineRpmDecreaseRate * deltaTime);
				}
				else
				{
					ReplicatedEngineRpm = newEngineRpm;
				}
			}
			ReplicatedEngineRpm = Mathf.Clamp(ReplicatedEngineRpm, asset.EngineIdleRpm, asset.EngineMaxRpm);

			float normalizedEngineRpm = Mathf.InverseLerp(asset.EngineIdleRpm, asset.EngineMaxRpm, ReplicatedEngineRpm);
			float torqueCurveValue = engineCurvesComponent != null ?
				engineCurvesComponent.engineRpmToTorqueCurve.Evaluate(normalizedEngineRpm)
				: Mathf.Lerp(0.5f, 1.0f, normalizedEngineRpm);
			float engineTorque = torqueCurveValue * asset.EngineMaxTorque;

			if (asset.EngineRpmMismatchTorqueReductionEnabled)
			{
				float normalizedDifference = Mathf.Clamp(rpmMismatch / asset.EngineRpmMismatchTorqueReductionThreshold, -1.0f, 1.0f);
				float torqueReductionCurveValue;
				if (engineCurvesComponent != null && engineCurvesComponent.useEngineRpmMismatchTorqueReductionCurve)
				{
					torqueReductionCurveValue = engineCurvesComponent.engineRpmMismatchTorqueReductionCurve.Evaluate(normalizedDifference);
				}
				else
				{
					torqueReductionCurveValue = Mathf.Lerp(1.0f, 0.0f, Mathf.Abs(normalizedDifference));
				}
			}

			float wheelAvailableTorque = engineTorque * Mathf.Abs(latestGasInput);
			if (timeSinceLastGearChange < asset.GearShiftDuration)
			{
				wheelAvailableTorque = 0.0f;
			}

			if (GearNumber == -1)
			{
				wheelAvailableTorque *= asset.reverseGearRatio;
			}
			else if (asset.UsesEngineRpmAndGears && GearNumber >= 1 && GearNumber <= asset.forwardGearRatios.Length)
			{
				wheelAvailableTorque *= asset.forwardGearRatios[GearNumber - 1];
			}
			if (asset.poweredWheelIndices != null && asset.poweredWheelIndices.Length > 0)
			{
				wheelAvailableTorque /= asset.poweredWheelIndices.Length;
			}

			foreach (Wheel wheel in _wheels)
			{
				if (wheel == null)
				{
					break;
				}

				wheel.UpdateLocallyDriven(deltaTime, wheelAvailableTorque);
			}
		}

		/// <summary>
		/// Nelson 2025-04-22: this should ideally be moved into FixedUpdate, incorrect to run in Update.
		/// </summary>
		private void UpdateLocallyDrivenTrainPhysics(float deltaTime)
		{
			TeleportTrainCars(false);

			float step = inputEngineVelocity * deltaTime;

			Transform overlap;
			if (inputEngineVelocity > 0)
			{
				overlap = overlapFront;
			}
			else
			{
				overlap = overlapBack;
			}

			BoxCollider overlapBox = overlap?.GetComponent<BoxCollider>();

			bool isObstructed;
			if (overlapBox != null)
			{
				isObstructed = false;
				Vector3 overlapPosition = overlap.position + (overlap.forward * step / 2);
				Vector3 overlapSize = overlapBox.size;
				overlapSize.z = step;

				int hitCount = Physics.OverlapBoxNonAlloc(overlapPosition, overlapSize / 2, tempCollidersArray, overlap.rotation, RayMasks.BLOCK_TRAIN, QueryTriggerInteraction.Ignore);
				for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
				{
					bool isChild = false;
					for (int carIndex = 0; carIndex < trainCars.Length; carIndex++)
					{
						if (tempCollidersArray[hitIndex].transform.IsChildOf(trainCars[carIndex].root) || tempCollidersArray[hitIndex].transform == trainCars[carIndex].root)
						{
							isChild = true;
							break;
						}
					}

					if (!isChild)
					{
						if (tempCollidersArray[hitIndex].CompareTag("Vehicle"))
						{
							Rigidbody rb = tempCollidersArray[hitIndex].GetComponent<Rigidbody>();
							if (!rb.isKinematic)
							{
								rb.AddForce(transform.forward * inputEngineVelocity, ForceMode.VelocityChange);
							}
						}

						isObstructed = true;
						break;
					}
				}
			}
			else
			{
				isObstructed = true;
			}

			if (isObstructed)
			{
				if (inputEngineVelocity > 0)
				{
					if (inputTargetVelocity > 0)
					{
						inputTargetVelocity = 0;
					}
				}
				else
				{
					if (inputTargetVelocity < 0)
					{
						inputTargetVelocity = 0;
					}
				}
			}
			else
			{
				roadPosition += step;
				roadPosition = ClampEngineRoadPosition(roadPosition);
			}
		}

		private void UpdateTrainCarTransforms(float deltaTime)
		{
			foreach (TrainCar car in trainCars)
			{
				Vector3 targetFrontPosition;
				Vector3 targetFrontNormal;
				Vector3 targetFrontDirection;
				road.getTrackData(ClampCarRoadPosition(roadPosition + car.trackPositionOffset + asset.trainWheelOffset), out targetFrontPosition, out targetFrontNormal, out targetFrontDirection);

				car.currentFrontPosition = Vector3.Lerp(car.currentFrontPosition, targetFrontPosition, 8 * deltaTime);
				car.currentFrontNormal = Vector3.Lerp(car.currentFrontNormal, targetFrontNormal, 8 * deltaTime);
				car.currentFrontDirection = Vector3.Lerp(car.currentFrontDirection, targetFrontDirection, 8 * deltaTime);

				Vector3 targetBackPosition;
				Vector3 targetBackNormal;
				Vector3 targetBackDirection;
				road.getTrackData(ClampCarRoadPosition(roadPosition + car.trackPositionOffset - asset.trainWheelOffset), out targetBackPosition, out targetBackNormal, out targetBackDirection);

				car.currentBackPosition = Vector3.Lerp(car.currentBackPosition, targetBackPosition, 8 * deltaTime);
				car.currentBackNormal = Vector3.Lerp(car.currentBackNormal, targetBackNormal, 8 * deltaTime);
				car.currentBackDirection = Vector3.Lerp(car.currentBackDirection, targetBackDirection, 8 * deltaTime);

				MoveTrainCar(car.currentFrontPosition, car.currentFrontNormal, car.currentFrontDirection, car.currentBackPosition, car.currentBackNormal, car.currentBackDirection, car, false);
			}
		}

		private void UpdateSirenVisuals()
		{
			if (Time.realtimeSinceStartup - lastWeeoo <= 0.33f)
			{
				return;
			}
			lastWeeoo = Time.realtimeSinceStartup;

			sirenState = !sirenState;

			foreach (GameObject siren in sirenGameObjects_0)
			{
				siren.SetActive(!sirenState);
			}

			foreach (GameObject siren in sirenGameObjects_1)
			{
				siren.SetActive(sirenState);
			}

			if (sirenMaterials != null)
			{
				if (sirenMaterials[0] != null)
				{
					sirenMaterials[0].SetColor("_EmissionColor", !sirenState ? sirenMaterials[0].color * 2f : Color.black);
				}

				if (sirenMaterials[1] != null)
				{
					sirenMaterials[1].SetColor("_EmissionColor", sirenState ? sirenMaterials[1].color * 2f : Color.black);
				}
			}
		}

		private void UpdatePredictedBatteryCharge(float deltaTime)
		{
			bool isCharging = false;
			bool isBurning = false;

			if (isDriven && isEnginePowered)
			{
				switch (asset.batteryDriving)
				{
					case EBatteryMode.Burn:
						isBurning = true;
						break;
					case EBatteryMode.Charge:
						isCharging = true;
						break;
				}
			}
			else
			{
				switch (asset.batteryEmpty)
				{
					case EBatteryMode.Burn:
						isBurning = true;
						break;
					case EBatteryMode.Charge:
						isCharging = true;
						break;
				}
			}

			if (headlightsOn)
			{
				switch (asset.batteryHeadlights)
				{
					case EBatteryMode.Burn:
						isBurning = true;
						break;
					case EBatteryMode.Charge:
						isCharging = true;
						break;
				}
			}

			if (sirensOn)
			{
				switch (asset.batterySirens)
				{
					case EBatteryMode.Burn:
						isBurning = true;
						break;
					case EBatteryMode.Charge:
						isCharging = true;
						break;
				}
			}

			// Only charge when not zero, otherwise a "new" battery will be created and scrapable.
			isCharging &= ContainsBatteryItem;

			float batteryPerSecond = 0;
			if (isCharging)
			{
				batteryPerSecond = asset.batteryChargeRate;
			}
			else if (isBurning)
			{
				batteryPerSecond = asset.batteryBurnRate;
			}

			batteryBuffer += deltaTime * batteryPerSecond;

			ushort batteryDelta = (ushort) Mathf.FloorToInt(batteryBuffer);
			if (batteryDelta > 0)
			{
				batteryBuffer -= batteryDelta;

				if (isCharging)
				{
					askChargeBattery(batteryDelta);
				}
				else if (isBurning)
				{
					askBurnBattery(batteryDelta);
				}
			}
		}

		/// <summary>
		/// Update whether this vehicle is inside a safezone.
		/// If a certain option is enabled, unlock after time threshold is passed.
		/// </summary>
		private void UpdateSafezoneStatus(float deltaSeconds)
		{
			SafezoneNode newNode;
			isInsideSafezone = LevelNodes.isPointInsideSafezone(transform.position, out newNode);
			insideSafezoneNode = newNode;

			if (isInsideSafezone)
			{
				timeInsideSafezone += deltaSeconds;

				// set to -1 to disable
				// prevents parking in safezone safely forever
				if (Provider.modeConfigData != null && Provider.modeConfigData.Vehicles.Unlocked_After_Seconds_In_Safezone > 0.0f)
				{
					if (timeInsideSafezone > Provider.modeConfigData.Vehicles.Unlocked_After_Seconds_In_Safezone)
					{
						if (isEmpty && isLocked)
						{
							VehicleManager.unlockVehicle(this, null);
						}
					}
				}
			}
			else
			{
				timeInsideSafezone = -1.0f;
			}
		}

		protected virtual void handleTireAliveChanged(Wheel wheel)
		{
			if (isPhysical)
			{
				rootRigidbody.WakeUp();
			}
		}

		/// <summary>
		/// Can be called without calling init.
		/// </summary>
		internal void safeInit(VehicleAsset asset)
		{
			_asset = asset;

			if (!Dedicator.IsDedicatedServer)
			{
				fire = transform.Find("Fire");
				LightLODTool.applyLightLOD(fire);

				smoke_0 = transform.Find("Smoke_0");
				smoke_1 = transform.Find("Smoke_1");
			}

			ApplyDepthMaskMaterial();
		}

		internal void init(VehicleAsset asset)
		{
			safeInit(asset);

			eventHook = gameObject.GetComponent<VehicleEventHook>();
			craftingTagProviderModHook = gameObject.GetComponent<CraftingTagProviderComponent>();
			engineCurvesComponent = gameObject.GetComponentInChildren<EngineCurvesComponent>(true);

			if (Provider.isServer)
			{
				if (fuel == ushort.MaxValue)
				{
					if (Provider.mode == EGameMode.TUTORIAL)
					{
						fuel = 0;
					}
					else
					{
						fuel = (ushort) Random.Range(asset.fuelMin, asset.fuelMax);
					}
				}

				if (health == ushort.MaxValue)
				{
					health = (ushort) Random.Range(asset.healthMin, asset.healthMax);
				}

				if (batteryCharge == ushort.MaxValue)
				{
					if (usesBattery)
					{
						if (asset.canSpawnWithBattery && Random.value < Provider.modeConfigData.Vehicles.Has_Battery_Chance)
						{
							float multiplier = Random.Range(Provider.modeConfigData.Vehicles.Min_Battery_Charge, Provider.modeConfigData.Vehicles.Max_Battery_Charge);
							multiplier *= asset.batterySpawnChargeMultiplier;

							batteryCharge = (ushort) Mathf.Max(1, 10000 * multiplier);
						}
						else
						{
							batteryCharge = 0;
						}
					}
					else
					{
						batteryCharge = 10000;
					}
				}

				if (PaintColor.a != byte.MaxValue)
				{
					Color32? defaultPaintColor = asset.GetRandomDefaultPaintColor();
					if (defaultPaintColor.HasValue)
					{
						Color32 newPaintColor = defaultPaintColor.Value;
						newPaintColor.a = byte.MaxValue;
						PaintColor = newPaintColor;
					}
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				transform.FindAllChildrenWithName("Sirens", sirenGameObjects);
				transform.FindAllChildrenWithName("Siren_0", sirenGameObjects_0);
				transform.FindAllChildrenWithName("Siren_1", sirenGameObjects_1);

				foreach (GameObject siren in sirenGameObjects)
				{
					LightLODTool.applyLightLOD(siren.transform);
				}

				sirenMaterials = new Material[2];

				List<GameObject> sirenModelGameObjects = new List<GameObject>();
				transform.FindAllChildrenWithName("Siren_0_Model", sirenModelGameObjects);
				foreach (GameObject model in sirenModelGameObjects)
				{
					if (sirenMaterials[0] == null)
					{
						Renderer sirenRenderer = model.GetComponent<Renderer>();
						if (sirenRenderer != null)
						{
							sirenMaterials[0] = sirenRenderer.material;
						}
					}
					else
					{
						model.GetComponent<Renderer>().sharedMaterial = sirenMaterials[0];
					}
				}
				sirenModelGameObjects.Clear();
				transform.FindAllChildrenWithName("Siren_1_Model", sirenModelGameObjects);
				foreach (GameObject model in sirenModelGameObjects)
				{
					if (sirenMaterials[1] == null)
					{
						Renderer sirenRenderer = model.GetComponent<Renderer>();
						if (sirenRenderer != null)
						{
							sirenMaterials[1] = sirenRenderer.material;
						}
					}
					else
					{
						model.GetComponent<Renderer>().sharedMaterial = sirenMaterials[1];
					}
				}

				_headlights = transform.Find("Headlights");
				LightLODTool.applyLightLOD(headlights);

				Transform headlightsModel = transform.FindChildRecursive("Headlights_Model");
				if (headlightsModel != null)
				{
					Renderer headlightsRenderer = headlightsModel.GetComponent<Renderer>();
					if (headlightsRenderer)
					{
						headlightsMaterial = headlightsRenderer.material;
					}
				}

				_taillights = transform.Find("Taillights");
				LightLODTool.applyLightLOD(taillights);

				Transform taillightsModel = transform.FindChildRecursive("Taillights_Model");
				if (taillightsModel != null)
				{
					Renderer taillightsRenderer = taillightsModel.GetComponent<Renderer>();
					if (taillightsRenderer)
					{
						taillightsMaterial = taillightsRenderer.material;
					}
				}
				else
				{
					tempMaterialsList.Clear();
					for (int index = 0; index < 4; index++)
					{
						Transform model = transform.Find("Taillight_" + index + "_Model");
						if (model == null)
						{
							break;
						}

						Renderer taillightRenderer = model.GetComponent<Renderer>();
						if (taillightRenderer != null)
						{
							tempMaterialsList.Add(taillightRenderer.material);
						}
					}

					if (tempMaterialsList.Count > 0)
					{
						taillightMaterials = tempMaterialsList.ToArray();
					}
				}

				if ((asset.engine == EEngine.HELICOPTER || asset.engine == EEngine.BLIMP) && clipAudioSource != null)
				{
					windZone = clipAudioSource.gameObject.AddComponent<WindZone>();
					windZone.mode = WindZoneMode.Spherical;
					windZone.radius = 64;
					windZone.windMain = 0;
					windZone.windTurbulence = 0;
					windZone.windPulseFrequency = 0;
					windZone.windPulseMagnitude = 0;
				}
			}

			_sirensOn = false;
			_headlightsOn = false;
			_taillightsOn = false;

			waterCenterTransform = transform.Find("Water_Center");

			Transform seatsTransform = transform.Find("Seats");
			if (seatsTransform == null)
			{
				Assets.ReportError(asset, "missing 'Seats' Transform");
				seatsTransform = new GameObject("Seats").transform;
				seatsTransform.parent = transform;
			}

			Transform objectsTransform = transform.Find("Objects");
			Transform turretsTransform = transform.Find("Turrets");
			Transform trainCarsTransform = transform.Find("Train_Cars");
			_passengers = new Passenger[seatsTransform.childCount];

			for (int index = 0; index < passengers.Length; index++)
			{
				string seatTransformName = "Seat_" + index;
				Transform seat = seatsTransform.Find(seatTransformName);
				if (seat == null)
				{
					Assets.ReportError(asset, "missing '{0}' Transform", seatTransformName);
					seat = new GameObject(seatTransformName).transform;
					seat.parent = seatsTransform;
				}

				Transform obj = null;

				if (objectsTransform != null)
				{
					obj = objectsTransform.Find("Seat_" + index);
				}

				Transform turret = null;
				Transform turretYaw = null;
				Transform turretPitch = null;
				Transform turretAim = null;

				if (turretsTransform != null)
				{
					turret = turretsTransform.Find("Turret_" + index);

					if (turret != null)
					{
						turretYaw = turret.Find("Yaw");

						if (turretYaw != null)
						{
							Transform turretSeatsTransform = turretYaw.Find("Seats");
							Transform turretObjectsTransform = turretYaw.Find("Objects");

							turretPitch = turretYaw.Find("Pitch");

							if (turretPitch != null)
							{
								if (turretSeatsTransform == null)
								{
									turretSeatsTransform = turretPitch.Find("Seats");
								}
								if (turretObjectsTransform == null)
								{
									obj = turretPitch.Find("Objects");
								}
							}

							if (turretSeatsTransform != null)
							{
								seat = turretSeatsTransform.Find("Seat_" + index);
							}

							if (turretObjectsTransform != null)
							{
								obj = turretObjectsTransform.Find("Seat_" + index);
							}
						}

						turretAim = turret.FindChildRecursive("Aim");
					}
				}

				if (trainCarsTransform != null)
				{
					Transform trainSeatTransform = trainCarsTransform.FindChildRecursive(seatTransformName);
					if (trainSeatTransform != null)
					{
						seat = trainSeatTransform;
					}
				}

				passengers[index] = new Passenger(seat, obj, turretYaw, turretPitch, turretAim);

				if (turret != null)
				{
					passengers[index].turretEventHook = turret.GetComponent<VehicleTurretEventHook>();
				}

				if (asset.shouldSpawnSeatCapsules)
				{
					GameObject seatClip = new GameObject("Clip");
					seatClip.layer = LayerMasks.CLIP;
					Transform seatClipTransform = seatClip.transform;
					seatClipTransform.parent = seat;
					seatClipTransform.localPosition = Vector3.zero;
					seatClipTransform.localRotation = Quaternion.identity;
					seatClipTransform.localScale = Vector3.one;
					seatClipTransform.parent = transform;

					CapsuleCollider collider = seatClipTransform.GetOrAddComponent<CapsuleCollider>();
					collider.center = new Vector3(0.0f, PlayerMovement.HEIGHT_STAND * 0.5f, 0.0f);
					collider.height = PlayerMovement.HEIGHT_STAND;
					collider.radius = PlayerStance.RADIUS;
					collider.enabled = false;
					passengers[index].collider = collider;
				}
			}

			_turrets = new Passenger[asset.turrets.Length];
			for (int index = 0; index < turrets.Length; index++)
			{
				TurretInfo info = asset.turrets[index];

				if (info.seatIndex >= passengers.Length)
				{
					continue;
				}

				passengers[info.seatIndex].turret = info;
				_turrets[index] = passengers[info.seatIndex];
			}

			InitializeWheels();

			buoyancy = transform.Find("Buoyancy");
			if (buoyancy != null)
			{
				for (int index = 0; index < buoyancy.childCount; index++)
				{
					Transform pontoon = buoyancy.GetChild(index);
					pontoon.gameObject.AddComponent<Buoyancy>().density = buoyancy.childCount * 500;

					if (asset.engine == EEngine.BLIMP)
					{
						pontoon.GetComponent<Buoyancy>().overrideSurfaceElevation = Level.info.configData.Blimp_Altitude;
					}
				}
			}

			hook = transform.Find("Hook");
			hooked = new List<HookInfo>();

			if (!Dedicator.IsDedicatedServer)
			{
				steeringWheelModelTransform = transform.Find("Objects/Steer");
				if (steeringWheelModelTransform != null)
				{
					steeringWheelRestLocalRotation = steeringWheelModelTransform.localRotation;
				}

				pedalLeft = transform.Find("Objects").Find("Pedal_Left");
				pedalRight = transform.Find("Objects").Find("Pedal_Right");

				Transform rotorsTransform = transform.Find("Rotors");
				if (rotorsTransform != null)
				{
					propellerModels = new PropellerModel[rotorsTransform.childCount];
					isPropellerMotionBlurEnabled = false;
					int index = 0;
					foreach (Transform propellerTransform in rotorsTransform)
					{
						PropellerModel model = new PropellerModel();
						model.transform = propellerTransform;
						model.bladeMaterial = propellerTransform.Find("Model_0").GetComponent<Renderer>()?.material;
						model.motionBlurRenderer = propellerTransform.Find("Model_1")?.GetComponent<Renderer>();
						model.motionBlurMaterial = model.motionBlurRenderer?.material;
						model.baseLocationRotation = propellerTransform.localRotation;

						if (model.motionBlurRenderer != null)
						{
							model.motionBlurRenderer.enabled = false; // Initial sync to isPropellerMotionBlurEnabled.
						}

						if (asset.requiredShaderUpgrade)
						{
							// Unfortunately the cloned material does not carry over the fix,
							// but we cannot test if it WAS fade, so we always fixup old standard.

							if (StandardShaderUtils.isMaterialUsingStandardShader(model.bladeMaterial))
							{
								StandardShaderUtils.setModeToTransparent(model.bladeMaterial);
							}

							if (StandardShaderUtils.isMaterialUsingStandardShader(model.motionBlurMaterial))
							{
								StandardShaderUtils.setModeToTransparent(model.motionBlurMaterial);
							}
						}

						materialsToDestroy.Add(model.bladeMaterial);
						materialsToDestroy.Add(model.motionBlurMaterial);

						waterSortHandles.Add(DynamicWaterTransparentSort.Get().Register(propellerTransform, model.bladeMaterial));
						waterSortHandles.Add(DynamicWaterTransparentSort.Get().Register(propellerTransform, model.motionBlurMaterial));

						propellerModels[index] = model;
						++index;
					}
				}

				Transform exhaustTransform = transform.Find("Exhaust");
				if (exhaustTransform != null)
				{
					exhaustGameObject = exhaustTransform.gameObject;
					isExhaustGameObjectActive = exhaustGameObject.activeSelf;
					exhaustParticleSystems = new ParticleSystem[exhaustTransform.childCount];
					for (int index = 0; index < exhaustTransform.childCount; index++)
					{
						Transform emitterTransform = exhaustTransform.GetChild(index);
						exhaustParticleSystems[index] = emitterTransform.GetComponent<ParticleSystem>();
						ParticleSystem.EmissionModule emission = exhaustParticleSystems[index].emission;
						emission.rateOverTime = 0;
					}
					isExhaustRateOverTimeZero = true;
				}

				frontModelTransform = transform.Find("Objects/Front");
				if (frontModelTransform != null)
				{
					frontModelRestLocalRotation = frontModelTransform.localRotation;
				}

				tellFuel(fuel);
				tellHealth(health);
				tellBatteryCharge(batteryCharge);

				InitializeAdditionalTransparentSections();
				InitializePaintableSections();
				InitializeCrawlerTrackTilingMaterials();
				updateSkin();
			}

			if (isExploded)
			{
				tellExploded();
			}

			if (asset.engine == EEngine.TRAIN)
			{
				int trainCarsCount = trainCarsTransform.childCount;
				trainCars = new TrainCar[1 + trainCarsCount];
				trainCars[0] = getTrainCar(transform);

				for (int trainCarIndex = 1; trainCarIndex <= trainCarsCount; trainCarIndex++)
				{
					Transform trainCarTransform = trainCarsTransform.Find("Train_Car_" + trainCarIndex);
					trainCarTransform.parent = null;
					trainCarTransform.GetOrAddComponent<VehicleRef>().vehicle = this;

					TrainCar car = getTrainCar(trainCarTransform);
					car.trackPositionOffset = trainCarIndex * -asset.trainCarLength;
					trainCars[trainCarIndex] = car;
				}

				foreach (TrainCar car in trainCars)
				{
					if (overlapFront == null)
					{
						overlapFront = car.root.Find("Overlap_Front");
					}

					if (overlapBack == null)
					{
						overlapBack = car.root.Find("Overlap_Back");
					}

					if (overlapFront != null && overlapBack != null)
					{
						break;
					}
				}

				foreach (LevelTrainAssociation train in Level.info.configData.Trains)
				{
					if (train.VehicleID == id)
					{
						roadIndex = train.RoadIndex;
						break;
					}
				}

				road = LevelRoads.getRoad(roadIndex);
				roadPosition = ClampEngineRoadPosition(roadPosition);
				TeleportTrainCars(true);
			}

			if (asset.physicsProfileRef.isValid)
			{
				VehiclePhysicsProfileAsset physicsProfile = asset.physicsProfileRef.Find();
				if (physicsProfile != null)
				{
					physicsProfile.applyTo(this);
				}
			}

			decayLastUpdateTime = Time.time;
			decayLastUpdatePosition = transform.position;
		}

		private void Awake()
		{
			rootRigidbody = GetComponent<Rigidbody>();

			if (!Dedicator.IsDedicatedServer)
			{
				Transform sound = transform.Find("Sound");
				if (sound != null)
				{
					clipAudioSource = sound.GetComponent<AudioSource>();
				}
			}
		}

		private void initBumper(Transform bumper, bool reverse, bool instakill)
		{
			if (bumper == null)
			{
				return;
			}

			if (Provider.isServer)
			{
				Bumper bumperComponent = bumper.gameObject.AddComponent<Bumper>();
				bumperComponent.reverse = reverse;
				bumperComponent.instakill = instakill;
				bumperComponent.init(this);
			}
			else
			{
				Destroy(bumper.gameObject);
			}
		}

		private void initBumpers(Transform root)
		{
			Transform nav = root.FindChildRecursive("Nav");
			if (nav != null)
			{
				if (Provider.isServer)
				{
					nav.DestroyRigidbody();
				}
				else
				{
					Destroy(nav.gameObject);
				}
			}

			Transform bumper = root.FindChildRecursive("Bumper");
			initBumper(bumper, false, asset.engine == EEngine.TRAIN);

			Transform bumperFront = root.FindChildRecursive("Bumper_Front");
			initBumper(bumperFront, false, asset.engine == EEngine.TRAIN);

			Transform bumperBack = root.FindChildRecursive("Bumper_Back");
			initBumper(bumperBack, true, asset.engine == EEngine.TRAIN);
		}

		private void Start()
		{
			hasUnityCalledStart = true;

			if (trainCars != null && trainCars.Length > 0)
			{
				foreach (TrainCar car in trainCars)
				{
					initBumpers(car.root);
				}
			}
			else
			{
				initBumpers(transform);
			}

			updateVehicle();
			updatePhysics();
			updateEngine();

#pragma warning disable
			updates = new List<VehicleStateUpdate>();
#pragma warning restore

#if !DEDICATED_SERVER
			switch (asset.engineSoundType)
			{
				case EVehicleEngineSoundType.Legacy:
				{
					DefaultEngineSoundController esc = gameObject.AddComponent<DefaultEngineSoundController>();
					esc.vehicle = this;
					break;
				}

				case EVehicleEngineSoundType.EngineRPMSimple:
				{
					RpmEngineSoundController esc = gameObject.AddComponent<RpmEngineSoundController>();
					esc.vehicle = this;
					break;
				}
			}
#endif // !DEDICATED_SERVER
		}

		private void OnDestroy()
		{
			dropTrunkItems();

			if (_wheels != null)
			{
				foreach (Wheel wheel in _wheels)
				{
					wheel.OnVehicleDestroyed();
				}
			}

			if (skinMaterialToDestroy != null)
			{
				Destroy(skinMaterialToDestroy);
				skinMaterialToDestroy = null;
			}

			if (materialsToDestroy != null)
			{
				foreach (Material material in materialsToDestroy)
				{
					if (material != null)
					{
						Destroy(material);
					}
				}
				materialsToDestroy.Clear();
			}

			if (waterSortHandles != null)
			{
				DynamicWaterTransparentSort singleton = DynamicWaterTransparentSort.Get();
				foreach (object handle in waterSortHandles)
				{
					singleton.Unregister(handle);
				}
				waterSortHandles.Clear();
			}

			if (isExploded)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (asset.ShouldExplosionBurnMaterials && asset.explosionBurnMaterialSections == null)
					{
						HighlighterTool.destroyMaterials(transform);

						if (turrets != null)
						{
							for (int index = 0; index < turrets.Length; index++)
							{
								HighlighterTool.destroyMaterials(turrets[index].turretYaw);
								HighlighterTool.destroyMaterials(turrets[index].turretPitch);
							}
						}
					}
				}
			}

			if (headlightsMaterial != null)
			{
				DestroyImmediate(headlightsMaterial);
			}

			if (taillightsMaterial != null)
			{
				DestroyImmediate(taillightsMaterial);
			}
			else if (taillightMaterials != null)
			{
				for (int index = 0; index < taillightMaterials.Length; index++)
				{
					if (taillightMaterials[index] != null)
					{
						DestroyImmediate(taillightMaterials[index]);
					}
				}
			}

			if (sirenMaterials != null)
			{
				for (int index = 0; index < sirenMaterials.Length; index++)
				{
					if (sirenMaterials[index] != null)
					{
						DestroyImmediate(sirenMaterials[index]);
					}
				}
			}
		}

		internal List<Collider> _vehicleColliders;
#if UNITY_EDITOR
		[System.Obsolete("Built-in features should use _vehicleColliders instead to avoid garbage when iterating.")]
#endif // UNITY_EDITOR
		public IEnumerable<Collider> vehicleColliders => _vehicleColliders;

		private static List<Collider> _trainCarColliders = new List<Collider>(16);

		/// <summary>
		/// Called after initializing vehicle.
		/// </summary>
		public void gatherVehicleColliders()
		{
			_vehicleColliders = new List<Collider>(); // In-case a plugin calls this method multiple times.
			gameObject.GetComponentsInChildren(true, _vehicleColliders);
			// Init auto center from base vehicle BEFORE adding train cars.
			initCenterCollider();

			if (trainCars != null)
			{
				foreach (TrainCar car in trainCars)
				{
					_trainCarColliders.Clear();
					car.root.GetComponentsInChildren(true, _trainCarColliders);
					_vehicleColliders.AddRange(_trainCarColliders);
				}
			}
		}

		/// <summary>
		/// Makes the collision detection system ignore all collisions between this vehicle and the given colliders.
		/// Used to prevent vehicle from colliding with attached items.
		/// </summary>
		public void ignoreCollisionWith(IEnumerable<Collider> otherColliders, bool shouldIgnore)
		{
			if (_vehicleColliders == null)
			{
				throw new System.Exception("gatherVehicleColliders was not called yet");
			}

			for (int index = _vehicleColliders.Count - 1; index >= 0; --index)
			{
				Collider vehicleCollider = _vehicleColliders[index];
				if (vehicleCollider == null)
				{
					// We have to check for null because collider might get destroyed, so may as well tidy up.
					_vehicleColliders.RemoveAtFast(index);
					continue;
				}

				foreach (Collider otherCollider in otherColliders)
				{
					if (otherCollider == null)
					{
						// Not ideal to have null colliders in this list, but we check just-in-case because Unity
						// throws an exception if either collider is null.
						continue;
					}

					Physics.IgnoreCollision(vehicleCollider, otherCollider, shouldIgnore);
				}
			}
		}

		/// <summary>
		/// Used to disable collision between skycrane and held vehicle.
		/// </summary>
		private void ignoreCollisionWithVehicle(InteractableVehicle otherVehicle, bool shouldIgnore)
		{
			ignoreCollisionWith(otherVehicle._vehicleColliders, shouldIgnore);
		}

		public Vector3 getClosestPointOnHull(Vector3 position)
		{
			if (_vehicleColliders == null)
			{
				throw new System.Exception("gatherVehicleColliders was not called yet");
			}

			return CollisionUtil.ClosestPoint(_vehicleColliders, position);
		}

		public float getSqrDistanceFromHull(Vector3 position)
		{
			Vector3 closestPoint = getClosestPointOnHull(position);
			return (closestPoint - position).sqrMagnitude;
		}

		/// <summary>
		/// Transform used for exit physics queries.
		/// </summary>
		private Transform center;

		/// <summary>
		/// Find collider with the largest volume to use for exit physics queries.
		/// </summary>
		private void initCenterCollider()
		{
			center = transform.Find("Center");
			if (center != null)
				return;

			center = new GameObject("Center").transform;
			center.parent = transform;
			center.localPosition = Vector3.zero;
			center.localRotation = Quaternion.identity;
			center.localScale = Vector3.one;

			float largestVolume = 0.001f;
			foreach (Collider testCollider in _vehicleColliders)
			{
				if (testCollider.isTrigger)
					continue;

				if (testCollider is BoxCollider box)
				{
					float testVolume = box.GetBoxVolume();
					if (testVolume > largestVolume)
					{
						largestVolume = testVolume;
						center.position = box.TransformBoxCenter();
					}
				}
				else if (testCollider is SphereCollider sphere)
				{
					float testVolume = sphere.GetSphereVolume();
					if (testVolume > largestVolume)
					{
						largestVolume = testVolume;
						center.position = sphere.TransformSphereCenter();
					}
				}
				else if (testCollider is CapsuleCollider capsule)
				{
					float testVolume = capsule.GetCapsuleVolume();
					if (testVolume > largestVolume)
					{
						largestVolume = testVolume;
						center.position = capsule.TransformCapsuleCenter();
					}
				}
			}
		}

		private void InitializeWheels()
		{
			if (asset.wheelConfiguration != null && asset.wheelConfiguration.Length > 0)
			{
				List<Wheel> pendingWheels = new List<Wheel>(asset.wheelConfiguration.Length);

				foreach (VehicleWheelConfiguration configuration in asset.wheelConfiguration)
				{
					WheelCollider wheelCollider = null;
					if (!string.IsNullOrEmpty(configuration.wheelColliderPath))
					{
						Transform wheelColliderTransform = transform.Find(configuration.wheelColliderPath);
						if (wheelColliderTransform == null)
						{
							Assets.ReportError(asset, $"missing wheel collider transform at path \"{configuration.wheelColliderPath}\"");
						}
						else
						{
							wheelCollider = wheelColliderTransform.GetComponent<WheelCollider>();
							if (wheelCollider == null)
							{
								Assets.ReportError(asset, $"missing WheelCollider component at path \"{configuration.wheelColliderPath}\"");
							}
							else
							{
								if (asset.wheelColliderMassOverride.HasValue)
								{
									wheelCollider.mass = asset.wheelColliderMassOverride.Value;
								}
							}
						}
					}

					Transform modelTransform = null;
					if (!string.IsNullOrEmpty(configuration.modelPath))
					{
						modelTransform = transform.Find(configuration.modelPath);
						if (modelTransform == null)
						{
							Assets.ReportError(asset, $"missing wheel model transform at path \"{configuration.modelPath}\"");
						}
					}

					if (wheelCollider == null && modelTransform == null)
					{
						// Something is very wrong!
						continue;
					}

					Wheel wheel = new Wheel(this, pendingWheels.Count, wheelCollider, modelTransform, configuration);
					wheel.Reset();
					wheel.aliveChanged += handleTireAliveChanged;
					pendingWheels.Add(wheel);
				}

				_wheels = pendingWheels.ToArray();
			}
			else
			{
				_wheels = new Wheel[0];
			}
		}

		/// <summary>
		/// Set material on DepthMask child renderer responsible for hiding water when interior of vehicle is submerged.
		/// </summary>
		private void ApplyDepthMaskMaterial()
		{
			Transform depthMask = transform.Find("DepthMask");
			if (depthMask != null)
			{
				Renderer depthMaskRenderer = depthMask.GetComponent<Renderer>();
				if (depthMaskRenderer != null)
				{
					depthMaskRenderer.sharedMaterial = Resources.Load<Material>("Materials/DepthMask");
				}
			}
		}

		private void InitializeAdditionalTransparentSections()
		{
			if (asset.extraTransparentSections == null || asset.extraTransparentSections.Length < 1)
			{
				return;
			}

			DynamicWaterTransparentSort singleton = DynamicWaterTransparentSort.Get();
			foreach (PaintableVehicleSection section in asset.extraTransparentSections)
			{
				Transform child = transform.Find(section.path);
				if (child == null)
				{
					Assets.ReportError(asset, $"missing additional transparent section transform \"{section.path}\"");
					continue;
				}

				Renderer transparentRenderer = child.GetComponent<Renderer>();
				if (transparentRenderer == null)
				{
					Assets.ReportError(asset, $"additional transparent section \"{section.path}\" missing Renderer component");
					continue;
				}

				tempMaterialsList.Clear();
				transparentRenderer.GetMaterials(tempMaterialsList);

				if (section.allMaterials)
				{
					foreach (Material material in tempMaterialsList)
					{
						// Now that we instantiated them we need to destroy them.
						materialsToDestroy.Add(material);

						if (material.renderQueue != 3000)
						{
							Assets.ReportError(asset, $"additional transparent section \"{section.path}\" material render queue {material.renderQueue} is not transparent");
							continue;
						}

						object handle = singleton.Register(child, material);
						waterSortHandles.Add(handle);
					}
				}
				else
				{
					// Now that we instantiated them we need to destroy them.
					foreach (Material material in tempMaterialsList)
					{
						materialsToDestroy.Add(material);
					}

					if (section.materialIndex < 0 || section.materialIndex >= tempMaterialsList.Count)
					{
						Assets.ReportError(asset, $"additional transparent section \"{section.path}\" material index out of range (index: {section.materialIndex} length: {tempMaterialsList.Count})");
						continue;
					}

					Material transparentMaterial = tempMaterialsList[section.materialIndex];
					if (transparentMaterial.renderQueue != 3000)
					{
						Assets.ReportError(asset, $"additional transparent section \"{section.path}\" material {section.materialIndex} render queue {transparentMaterial.renderQueue} is not transparent");
						continue;
					}

					object handle = singleton.Register(child, transparentMaterial);
					waterSortHandles.Add(handle);
				}
			}
		}

		private void ApplyExplosionBurnMaterials()
		{
			if (asset.explosionBurnMaterialSections == null)
			{
				HighlighterTool.color(transform, new Color(0.25f, 0.25f, 0.25f));

				if (frontModelTransform != null)
				{
					HighlighterTool.color(frontModelTransform, new Color(0.25f, 0.25f, 0.25f));
				}

				if (turrets != null)
				{
					for (int index = 0; index < turrets.Length; index++)
					{
						HighlighterTool.color(turrets[index].turretYaw, new Color(0.25f, 0.25f, 0.25f));
						HighlighterTool.color(turrets[index].turretPitch, new Color(0.25f, 0.25f, 0.25f));
					}
				}
			}
			else
			{
				foreach (PaintableVehicleSection section in asset.explosionBurnMaterialSections)
				{
					Transform child = transform.Find(section.path);
					if (child == null)
					{
						Assets.ReportError(asset, $"explosion burn section missing transform \"{section.path}\"");
						continue;
					}

					Renderer burnableRenderer = child.GetComponent<Renderer>();
					if (burnableRenderer == null)
					{
						Assets.ReportError(asset, $"explosion burn section missing renderer \"{section.path}\"");
						continue;
					}

					tempMaterialsList.Clear();
					burnableRenderer.GetMaterials(tempMaterialsList);

					if (section.allMaterials)
					{
						foreach (Material material in tempMaterialsList)
						{
							// Now that we instantiated them we need to destroy them.
							materialsToDestroy.Add(material);

							material.color = new Color(0.25f, 0.25f, 0.25f);
						}
					}
					else
					{
						// Now that we instantiated them we need to destroy them.
						foreach (Material material in tempMaterialsList)
						{
							materialsToDestroy.Add(material);
						}

						if (section.materialIndex < 0 || section.materialIndex >= tempMaterialsList.Count)
						{
							Assets.ReportError(asset, $"explosion burn section \"{section.path}\" material index out of range (index: {section.materialIndex} length: {tempMaterialsList.Count})");
							continue;
						}

						tempMaterialsList[section.materialIndex].color = new Color(0.25f, 0.25f, 0.25f);
					}
				}
			}
		}

		private void InitializePaintableSections()
		{
			if (!asset.SupportsPaintColor)
			{
				return;
			}

			paintableMaterials = new List<Material>();
			foreach (PaintableVehicleSection section in asset.PaintableVehicleSections)
			{
				Transform child = transform.Find(section.path);
				if (child == null)
				{
					Assets.ReportError(asset, $"paintable section missing transform \"{section.path}\"");
					continue;
				}

				Renderer paintableRenderer = child.GetComponent<Renderer>();
				if (paintableRenderer == null)
				{
					Assets.ReportError(asset, $"paintable section missing renderer \"{section.path}\"");
					continue;
				}

				tempMaterialsList.Clear();
				paintableRenderer.GetMaterials(tempMaterialsList);

				// Now that we instantiated them we need to destroy them.
				foreach (Material material in tempMaterialsList)
				{
					materialsToDestroy.Add(material);
				}

				if (section.allMaterials)
				{
					paintableMaterials.AddRange(tempMaterialsList);
				}
				else
				{
					if (section.materialIndex < 0 || section.materialIndex >= tempMaterialsList.Count)
					{
						Assets.ReportError(asset, $"paintable section \"{section.path}\" material index out of range (index: {section.materialIndex} length: {tempMaterialsList.Count})");
						continue;
					}

					paintableMaterials.Add(tempMaterialsList[section.materialIndex]);
				}
			}

			ApplyPaintColor();
		}

		private void ApplyPaintColor()
		{
			if (paintableMaterials == null || Dedicator.IsDedicatedServer)
				return;

			foreach (Material material in paintableMaterials)
			{
				material.SetColor(PAINT_COLOR_ID, PaintColor);
			}
		}

		private void ApplySkinToRenderer(Renderer renderer, Material skinMaterial, bool shared)
		{
			skinOriginalMaterials.Add(new VehicleSkinMaterialChange()
			{
				renderer = renderer,
				originalMaterial = shared ? renderer.sharedMaterial : renderer.material,
				shared = shared,
			});

			if (shared)
			{
				renderer.material = skinMaterial;
			}
			else
			{
				renderer.sharedMaterial = skinMaterial;
			}
		}

		private void InitializeCrawlerTrackTilingMaterials()
		{
			if (asset.crawlerTrackTilingMaterials == null)
			{
				return;
			}

			crawlerTrackMaterials = new List<CrawlerTrackTilingMaterialInstance>(asset.crawlerTrackTilingMaterials.Length);
			foreach (CrawlerTrackTilingMaterial section in asset.crawlerTrackTilingMaterials)
			{
				Transform child = transform.Find(section.path);
				if (child == null)
				{
					Assets.ReportError(asset, $"crawler track tiling material missing transform \"{section.path}\"");
					continue;
				}

				Renderer trackRenderer = child.GetComponent<Renderer>();
				if (trackRenderer == null)
				{
					Assets.ReportError(asset, $"crawler track tiling material missing renderer \"{section.path}\"");
					continue;
				}

				tempMaterialsList.Clear();
				trackRenderer.GetMaterials(tempMaterialsList);

				// Now that we instantiated them we need to destroy them.
				foreach (Material material in tempMaterialsList)
				{
					materialsToDestroy.Add(material);
				}

				if (section.materialIndex < 0 || section.materialIndex >= tempMaterialsList.Count)
				{
					Assets.ReportError(asset, $"crawler track tiling material \"{section.path}\" material index out of range (index: {section.materialIndex} length: {tempMaterialsList.Count})");
					continue;
				}

				tempWheels.Clear();
				foreach (int wheelIndex in section.wheelIndices)
				{
					Wheel wheel = GetWheelAtIndex(wheelIndex);
					if (wheel == null)
					{
						Assets.ReportError(asset, $"crawler track tiling material \"{section.path}\" invalid wheel index: {wheelIndex}");
						continue;
					}

					if (wheel.wheel == null)
					{
						Assets.ReportError(asset, $"crawler track tiling material \"{section.path}\" wheel index {wheelIndex} should have a collider (this wheel is visual-only)");
						continue;
					}

					tempWheels.Add(wheel);
				}

				if (tempWheels.Count < 1)
				{
					Assets.ReportError(asset, $"crawler track tiling material \"{section.path}\" has no wheels");
					continue;
				}

				crawlerTrackMaterials.Add(new CrawlerTrackTilingMaterialInstance()
				{
					material = tempMaterialsList[section.materialIndex],
					wheels = tempWheels.ToArray(),
					initialUvPosition = tempMaterialsList[section.materialIndex].mainTextureOffset,
					repeatDistance = section.repeatDistance,
					uvDirection = section.uvDirection,
				});
			}

			if (_wheels != null)
			{
				foreach (Wheel wheel in _wheels)
				{
					if (wheel.config == null)
					{
						continue;
					}

					int crawlerTrackIndex = wheel.config.copyCrawlerTrackSpeedIndex;
					if (crawlerTrackIndex < 0)
					{
						continue;
					}

					if (crawlerTrackIndex >= crawlerTrackMaterials.Count)
					{
						Assets.ReportError(asset, $"wheel CopyCrawlerTrackSpeedIndex out of bounds (index: {crawlerTrackIndex} length: {crawlerTrackMaterials.Count})");
						continue;
					}

					wheel.copyCrawlerTrack = crawlerTrackMaterials[crawlerTrackIndex];
				}
			}
		}

		private void UpdateCrawlerTrackTilingMaterials(float deltaTime)
		{
			// CalculateWheelSpeed
			foreach (CrawlerTrackTilingMaterialInstance instance in crawlerTrackMaterials)
			{
				float speed = 0.0f;
				if (isPhysical)
				{
					int wheelCount = 0;
					foreach (Wheel wheel in instance.wheels)
					{
						speed += wheel.CalculateWheelSpeed();
						++wheelCount;
					}
					if (wheelCount > 0)
					{
						speed /= wheelCount;
					}
				}
				else
				{
					speed = ReplicatedForwardVelocity;
				}
				instance.speed = speed;
				float distanceTraveled = speed * deltaTime;

				instance.uvOffset = (instance.uvOffset + distanceTraveled * instance.repeatDistance) % 1.0f;
				instance.material.mainTextureOffset = instance.initialUvPosition + instance.uvDirection * instance.uvOffset;
			}
		}

		private void SetExhaustParticleSystemsRateOverTimeToZero()
		{
			foreach (ParticleSystem ps in exhaustParticleSystems)
			{
				ParticleSystem.EmissionModule emission = ps.emission;
				emission.rateOverTime = 0;
			}
			isExhaustRateOverTimeZero = true;
		}

		private void ApplyAngularVelocityDamping(float deltaTime)
		{
			Debug.Assert(asset.rollAngularVelocityDamping > 0.0f);

			Vector3 localAngularVelocity = transform.InverseTransformDirection(rootRigidbody.angularVelocity);
			float accel = localAngularVelocity.z * -asset.rollAngularVelocityDamping * deltaTime * 50.0f;
			// Avoid waking up rigidbody if near zero.
			if (!MathfEx.IsNearlyZero(accel, 0.001f))
			{
				rootRigidbody.AddRelativeTorque(0.0f, 0.0f, accel, ForceMode.Acceleration);
			}
		}

		private void ApplyWheelBalancingForce(float deltaTime)
		{
			Debug.Assert(asset.wheelBalancingForceMultiplier > 0.0f);

			Vector3 totalGroundNormal = Vector3.zero;
			int numGroundedWheels = 0;

			foreach (Wheel wheel in _wheels)
			{
				if (wheel.isGrounded)
				{
					totalGroundNormal += wheel.mostRecentGroundHit.normal;
					++numGroundedWheels;
				}
			}

			Vector3 averageGroundNormal = totalGroundNormal / numGroundedWheels;
			Vector3 localGroundNormal = transform.InverseTransformDirection(averageGroundNormal);
			localGroundNormal.z = 0.0f;
			localGroundNormal = localGroundNormal.normalized;

			// Factor is near zero when vehicle up is aligned with ground and approaches one when perpendicular.
			// Raised to an exponent to reduce the torque when almost upright.
			float uprightFactor = Mathf.Clamp01(1.0f - Vector3.Dot(Vector3.up, localGroundNormal));
			uprightFactor = Mathf.Pow(uprightFactor, asset.wheelBalancingUprightExponent);
			float direction = localGroundNormal.x > 0.0f ? -1.0f : +1.0f;
			uprightFactor *= direction;
			float torque = uprightFactor * asset.wheelBalancingForceMultiplier * deltaTime * 50.0f;
			// Avoid waking up rigidbody if near zero.
			if (!MathfEx.IsNearlyZero(torque, 0.001f))
			{
				rootRigidbody.AddRelativeTorque(0.0f, 0.0f, uprightFactor * asset.wheelBalancingForceMultiplier * deltaTime * 50.0f);
			}

#if DRAW_BICYCLE_GIZMOS
			float gizmoLifespan = delta;
			RuntimeGizmos.Get().Arrow(transform.position, averageGroundNormal, 5.0f, Color.yellow, delta);
			RuntimeGizmos.Get().Arrow(transform.position, transform.TransformDirection(localGroundNormal), 5.0f, Color.green, delta);
			RuntimeGizmos.Get().Label(transform.position, uprightFactor.ToString("N2"), delta);
#endif
		}

		private void PlayIgnitionSound()
		{
			if (!Dedicator.IsDedicatedServer && clipAudioSource != null && asset.ignition != null)
			{
				clipAudioSource.pitch = Random.Range(0.9f, 1.1f);
				clipAudioSource.PlayOneShot(asset.ignition);
			}
		}

		/// <summary>
		/// Skin material does not always need to be destroyed, so this is only valid if it should be destroyed.
		/// </summary>
		private Material skinMaterialToDestroy;

		/// <summary>
		/// Materials that should be destroyed when this vehicle is destroyed.
		/// </summary>
		private HashSet<Material> materialsToDestroy = new HashSet<Material>();

		/// <summary>
		/// Handles to unregister from DynamicWaterTransparentSort.
		/// </summary>
		private List<object> waterSortHandles = new List<object>();

		private static int PAINT_COLOR_ID = Shader.PropertyToID("_PaintColor");

		/// <summary>
		/// Materials to set _PaintColor on.
		/// </summary>
		private List<Material> paintableMaterials;

		/// <summary>
		/// Materials to move UVs in sync with wheels.
		/// </summary>
		private List<CrawlerTrackTilingMaterialInstance> crawlerTrackMaterials;

		/// <summary>
		/// Time.time decayTimer was last updated.
		/// </summary>
		internal float decayLastUpdateTime;

		/// <summary>
		/// Seconds since vehicle was interacted with.
		/// </summary>
		internal float decayTimer;

		/// <summary>
		/// Fractional damage counter.
		/// </summary>
		internal float decayPendingDamage;

		/// <summary>
		/// transform.position used to test whether vehicle is moving.
		/// </summary>
		internal Vector3 decayLastUpdatePosition;

		/// <summary>
		/// Should be called AFTER replication (if applicable) so that events can call methods relying on replication.
		/// </summary>
		internal void NotifyFirstSpawned()
		{
			if (eventHook != null)
			{
				eventHook.OnFirstSpawned.TryInvoke(this);
			}
		}

		#region Obsolete
		[System.Obsolete]
		public bool isUpdated;

		[System.Obsolete]
		public void tellState(Vector3 newPosition, byte newAngle_X, byte newAngle_Y, byte newAngle_Z, byte newSpeed, byte newPhysicsSpeed, byte newTurn)
		{
#pragma warning disable
			Quaternion newRotation = Quaternion.Euler(MeasurementTool.byteToAngle2(newAngle_X), MeasurementTool.byteToAngle2(newAngle_Y), MeasurementTool.byteToAngle2(newAngle_Z));
#pragma warning restore
			tellState(newPosition, newRotation, newSpeed, newPhysicsSpeed, newTurn, 0.0f);
		}

		[System.Obsolete("This override uses the vanilla battery item rather than the equipped battery item.")]
		public void replaceBattery(Player player, byte quality)
		{
			replaceBattery(player, quality, new System.Guid("098b13be34a7411db7736b7f866ada69"));
		}

		[System.Obsolete]
		public void safeInit()
		{
			safeInit(Assets.find(EAssetType.VEHICLE, id) as VehicleAsset);
		}

		[System.Obsolete]
		public void init()
		{
			init(Assets.find(EAssetType.VEHICLE, id) as VehicleAsset);
		}

		[System.Obsolete("Replaced by ReplicatedSteeringInput")]
		public int turn => Mathf.RoundToInt(ReplicatedSteeringInput);

		[System.Obsolete("Replaced by AnimatedSteeringAngle")]
		public float steer => AnimatedSteeringAngle;

		[System.Obsolete("Replaced by ReplicatedSpeed")]
		public float speed => ReplicatedSpeed * Mathf.Sign(ReplicatedForwardVelocity);
		[System.Obsolete("Replaced by ReplicatedForwardVelocity")]
		public float physicsSpeed => ReplicatedForwardVelocity;

		[System.Obsolete("Replaced by GetReplicatedForwardSpeedPercentageOfTargetSpeed")]
		public float factor => GetReplicatedForwardSpeedPercentageOfTargetSpeed();

		[System.Obsolete("Clarified with HasBatteryWithCharge and ContainsBatteryItem properties.")]
		public bool hasBattery
		{
			get
			{
				if (usesBattery)
				{
					// Nelson 2024-06-24: Changed depleted battery value from zero to one.
					return batteryCharge > 1;
				}
				else
				{
					return true;
				}
			}
		}
		#endregion Obsolete

		[System.Diagnostics.Conditional("ENABLE_VEHICLE_PROFILING")]
		private void BeginSample(string name)
		{
#if ENABLE_VEHICLE_PROFILING
			Profiler.BeginSample(name);
#endif
		}
		[System.Diagnostics.Conditional("ENABLE_VEHICLE_PROFILING")]
		private void EndSample()
		{
#if ENABLE_VEHICLE_PROFILING
			Profiler.EndSample();
#endif
		}
	}
}
