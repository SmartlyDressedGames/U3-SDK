////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define WITH_VEHICLE_ENTER_GIZMOS
// #define LOG_RECEIVE_VEHICLE
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void VehicleLockpickedSignature(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow);
	public delegate void DamageVehicleRequestHandler(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin);
	public delegate void RepairVehicleRequestHandler(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalHealing, ref bool shouldAllow);
	public delegate void DamageTireRequestHandler(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin);
	public delegate void VehicleCarjackedSignature(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow, ref Vector3 force, ref Vector3 torque);
	public delegate void SiphonVehicleRequestHandler(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref ushort desiredAmount);

	public class VehicleManager : SteamCaller
	{
		public const byte SAVEDATA_VERSION_ADDED_DECAY = 13;
		public const byte SAVEDATA_VERSION_REPLACED_ID_WITH_GUID = 14;
		public const byte SAVEDATA_VERSION_BATTERY_GUID = 15;
		public const byte SAVEDATA_VERSION_ADDED_PAINT_COLOR = 16;
		public const byte SAVEDATA_VERSION_ADDED_NATURAL_SPAWNED = 17;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_NATURAL_SPAWNED;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		public static VehicleLockpickedSignature onVehicleLockpicked;
		public static DamageVehicleRequestHandler onDamageVehicleRequested;
		public static RepairVehicleRequestHandler onRepairVehicleRequested;
		public static DamageTireRequestHandler onDamageTireRequested;
		public static VehicleCarjackedSignature onVehicleCarjacked;
		public static SiphonVehicleRequestHandler onSiphonVehicleRequested;

		public delegate void EnterVehicleRequestHandler(Player player, InteractableVehicle vehicle, ref bool shouldAllow);
		public static event EnterVehicleRequestHandler onEnterVehicleRequested;

		public delegate void ExitVehicleRequestHandler(Player player, InteractableVehicle vehicle, ref bool shouldAllow, ref Vector3 pendingLocation, ref float pendingYaw);
		public static event ExitVehicleRequestHandler onExitVehicleRequested;

		public delegate void SwapSeatRequestHandler(Player player, InteractableVehicle vehicle, ref bool shouldAllow, byte fromSeatIndex, ref byte toSeatIndex);
		public static event SwapSeatRequestHandler onSwapSeatRequested;

		/// <summary>
		/// Invoked immediately before Destroy vehicle.
		/// </summary>
		public static event System.Action<InteractableVehicle> OnPreDestroyVehicle;

		private static VehicleManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static VehicleManager instance => manager;

		private static List<InteractableVehicle> _vehicles;
		public static List<InteractableVehicle> vehicles => _vehicles;

		/// <summary>
		/// If true, a vehicle asset has been replaced.
		/// </summary>
		internal static bool shouldRespawnReloadedVehicles;

		private static uint highestInstanceID;

		private static uint allocateInstanceID()
		{
			return ++highestInstanceID;
		}

		private static ushort respawnVehicleIndex;
		private static float lastTick;

		public static uint maxInstances
		{
			get
			{
				switch (Level.info.size)
				{
					case ELevelSize.TINY:
						return Provider.modeConfigData.Vehicles.Max_Instances_Tiny;

					case ELevelSize.SMALL:
						return Provider.modeConfigData.Vehicles.Max_Instances_Small;

					case ELevelSize.MEDIUM:
						return Provider.modeConfigData.Vehicles.Max_Instances_Medium;

					case ELevelSize.LARGE:
						return Provider.modeConfigData.Vehicles.Max_Instances_Large;

					case ELevelSize.INSANE:
						return Provider.modeConfigData.Vehicles.Max_Instances_Insane;

					default:
						return 0;
				}
			}
		}

		public static byte getVehicleRandomTireAliveMask(VehicleAsset asset)
		{
			if (asset.canTiresBeDamaged)
			{
				int mask = 0;
				for (byte index = 0; index < 8; index++)
				{
					if (Random.value < Provider.modeConfigData.Vehicles.Has_Tire_Chance)
					{
						int flag = 1 << index;
						mask |= flag;
					}
				}

				return (byte) mask;
			}
			else
			{
				return byte.MaxValue;
			}
		}

		public static void getVehiclesInRadius(Vector3 center, float sqrRadius, List<InteractableVehicle> result)
		{
			if (vehicles == null)
			{
				return;
			}

			for (int index = 0; index < vehicles.Count; index++)
			{
				InteractableVehicle vehicle = vehicles[index];

				if (vehicle.isDead)
				{
					continue;
				}

				Vector3 offset = vehicle.transform.position - center;

				if (offset.sqrMagnitude < sqrRadius)
				{
					result.Add(vehicle);
				}
			}
		}

		/// <summary>
		/// Find vehicle with matching replicated instance ID.
		/// </summary>
		public static InteractableVehicle findVehicleByNetInstanceID(uint instanceID)
		{
			foreach (InteractableVehicle vehicle in vehicles)
			{
				if (vehicle != null && vehicle.instanceID == instanceID)
				{
					return vehicle;
				}
			}

			return null;
		}

		// Renamed to findVehicleByNetInstanceID.
		public static InteractableVehicle getVehicle(uint instanceID)
		{
			return findVehicleByNetInstanceID(instanceID);
		}

		public static void damage(InteractableVehicle vehicle, float damage, float times, bool canRepair, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			if (vehicle == null || vehicle.asset == null)
			{
				return;
			}

			if (!vehicle.isDead)
			{
				if (!vehicle.asset.isVulnerable && !vehicle.asset.isVulnerableToExplosions && !vehicle.asset.isVulnerableToEnvironment)
				{
					UnturnedLog.error("Somehow tried to damage completely invulnerable vehicle: " + vehicle + " " + damage + " " + times + " " + canRepair);
					return;
				}

				times *= Provider.modeConfigData.Vehicles.Armor_Multiplier;

				ushort totalDamage = (ushort) (damage * times);
				bool shouldAllow = true;

				// Allow plugins to modify damage or cancel it
				onDamageVehicleRequested?.Invoke(instigatorSteamID, vehicle, ref totalDamage, ref canRepair, ref shouldAllow, damageOrigin);

				if (!shouldAllow || totalDamage < 1)
				{
					return;
				}

				vehicle.askDamage(totalDamage, canRepair);
			}
		}

		public static void damageTire(InteractableVehicle vehicle, int tireIndex, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			if (tireIndex < 0)
				return;

			bool shouldAllow = true;

			// Allow plugins to modify damage or cancel it
			onDamageTireRequested?.Invoke(instigatorSteamID, vehicle, tireIndex, ref shouldAllow, damageOrigin);

			if (!shouldAllow)
				return;

			vehicle.askDamageTire(tireIndex);
		}

		public static void repair(InteractableVehicle vehicle, float damage, float times)
		{
			repair(vehicle, damage, times, CSteamID.Nil);
		}

		public static void repair(InteractableVehicle vehicle, float damage, float times, CSteamID instigatorSteamID = new CSteamID())
		{
			if (vehicle == null)
			{
				return;
			}

			if (!vehicle.isExploded && !vehicle.isRepaired)
			{
				ushort amount = (ushort) (damage * times);
				bool shouldAllow = true;

				// Allow plugins to modify or cancel it
				onRepairVehicleRequested?.Invoke(instigatorSteamID, vehicle, ref amount, ref shouldAllow);

				if (!shouldAllow || amount < 1)
					return;

				vehicle.askRepair(amount);
			}
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public static InteractableVehicle spawnVehicleV2(ushort id, Vector3 point, Quaternion angle)
		{
			Asset asset = Assets.find(EAssetType.VEHICLE, id);
			return spawnVehicleInternal(asset, point, angle, CSteamID.Nil, CSteamID.Nil, null);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If paintColor is set that takes priority, otherwise if
		/// redirector's SpawnPaintColor is set, that color is used,
		/// </summary>
		public static InteractableVehicle spawnVehicleV2(ushort id, Vector3 point, Quaternion angle, Color32? paintColor)
		{
			Asset asset = Assets.find(EAssetType.VEHICLE, id);
			return spawnVehicleInternal(asset, point, angle, CSteamID.Nil, CSteamID.Nil, paintColor);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public static InteractableVehicle spawnLockedVehicleForPlayerV2(ushort id, Vector3 point, Quaternion angle, Player player)
		{
			if (player == null)
			{
				throw new System.ArgumentNullException("player");
			}

			Asset asset = Assets.find(EAssetType.VEHICLE, id);
			return spawnVehicleInternal(asset, point, angle, player.channel.owner.playerID.steamID, player.quests.groupID, null);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If paintColor is set that takes priority, otherwise if
		/// redirector's SpawnPaintColor is set, that color is used,
		/// </summary>
		public static InteractableVehicle spawnLockedVehicleForPlayerV2(ushort id, Vector3 point, Quaternion angle, Player player, Color32? paintColor)
		{
			if (player == null)
			{
				throw new System.ArgumentNullException("player");
			}

			Asset asset = Assets.find(EAssetType.VEHICLE, id);
			return spawnVehicleInternal(asset, point, angle, player.channel.owner.playerID.steamID, player.quests.groupID, paintColor);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public static InteractableVehicle spawnVehicleV2(Asset asset, Vector3 point, Quaternion angle)
		{
			return spawnVehicleInternal(asset, point, angle, CSteamID.Nil, CSteamID.Nil, null);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If paintColor is set that takes priority, otherwise if
		/// redirector's SpawnPaintColor is set, that color is used,
		/// </summary>
		public static InteractableVehicle spawnVehicleV2(Asset asset, Vector3 point, Quaternion angle, Color32? paintColor)
		{
			return spawnVehicleInternal(asset, point, angle, CSteamID.Nil, CSteamID.Nil, paintColor);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public static InteractableVehicle spawnLockedVehicleForPlayerV2(Asset asset, Vector3 point, Quaternion angle, Player player)
		{
			if (player == null)
			{
				throw new System.ArgumentNullException("player");
			}

			return spawnVehicleInternal(asset, point, angle, player.channel.owner.playerID.steamID, player.quests.groupID, null);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If paintColor is set that takes priority, otherwise if
		/// redirector's SpawnPaintColor is set, that color is used,
		/// </summary>
		public static InteractableVehicle spawnLockedVehicleForPlayerV2(Asset asset, Vector3 point, Quaternion angle, Player player, Color32? paintColor)
		{
			if (player == null)
			{
				throw new System.ArgumentNullException("player");
			}

			return spawnVehicleInternal(asset, point, angle, player.channel.owner.playerID.steamID, player.quests.groupID, paintColor);
		}

		/// <summary>
		/// Added so that garage plugins do not need to invoke RPC manually.
		/// </summary>
		/// <param name="batteryCharge">zero spawns without a battery, ushort.MaxValue indicates the battery should be randomly spawned according to asset configuration, other values force a battery to spawn.</param>
		public static InteractableVehicle SpawnVehicleV3(VehicleAsset asset, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, byte[][] turrets, byte tireAliveMask, Color32 paintColor)
		{
			NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_VEHICLE);
			InteractableVehicle spawnedVehicle = manager.addVehicle(asset.GUID, skinID, mythicID, roadPosition, point, angle, sirens, blimp, headlights, taillights, fuel, /*isExploded*/false, health, batteryCharge, owner, group, locked, /*passengers*/null, turrets, allocateInstanceID(), tireAliveMask, netId, paintColor);
			if (spawnedVehicle == null)
				return null;

			SendSingleVehicle.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteVehicle, spawnedVehicle);

			spawnedVehicle.NotifyFirstSpawned();

			return spawnedVehicle;
		}

		/// <summary>
		/// For backwards compatibility. This older method spawns a vehicle with a random paint color. (set paintColor
		/// to zero for a random paint color)
		/// </summary>
		/// <param name="batteryCharge">zero spawns without a battery, ushort.MaxValue indicates the battery should be randomly spawned according to asset configuration, other values force a battery to spawn.</param>
		public static InteractableVehicle SpawnVehicleV3(VehicleAsset asset, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, byte[][] turrets, byte tireAliveMask)
		{
			return SpawnVehicleV3(asset, skinID, mythicID, roadPosition, point, angle, sirens, blimp, headlights, taillights, fuel, health, batteryCharge, owner, group, locked, turrets, tireAliveMask, new Color32(0, 0, 0, 0));
		}

		/// <summary>
		/// Used by external spawn vehicle methods.
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used,
		/// unless preferredColor.a is byte.MaxValue.
		/// </summary>
		/// <param name="owner">Owner to lock vehicle for by default. Used to lock vehicles to the player who purchased them.</param>
		internal static InteractableVehicle spawnVehicleInternal(Asset asset, Vector3 point, Quaternion angle, CSteamID owner, CSteamID groupId, Color32? preferredColor)
		{
			if (asset == null)
			{
				return null;
			}

			VehicleAsset vehicleAsset;
			Color32 paintColor = new Color32(0, 0, 0, 0);
			if (preferredColor.HasValue)
			{
				paintColor = preferredColor.Value;
			}
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				vehicleAsset = redirectorAsset.TargetVehicle.Find();
				// Only use redirector's color if preferredColor hasn't been set.
				if (!preferredColor.HasValue && redirectorAsset.SpawnPaintColor.HasValue)
				{
					paintColor = redirectorAsset.SpawnPaintColor.Value;
				}
			}
			else
			{
				vehicleAsset = asset as VehicleAsset;
			}

			if (vehicleAsset == null)
			{
				return null;
			}

			bool locked = owner != CSteamID.Nil; // If vehicle has a default owner, lock it for them.
			if (!locked)
			{
				groupId = CSteamID.Nil;
			}
			return SpawnVehicleV3(vehicleAsset, 0, 0, 0.0f, point, angle, false, false, false, false, vehicleAsset.fuel, vehicleAsset.health, 10000, owner, groupId, locked, null, byte.MaxValue, paintColor);
		}

		public static void enterVehicle(InteractableVehicle vehicle)
		{
			VehiclePhysicsProfileAsset physicsProfile = vehicle.asset.physicsProfileRef.Find();
			byte[] physicsProfileHash = physicsProfile != null ? physicsProfile.hash : new byte[0];
			SendEnterVehicleRequest.Invoke(ENetReliability.Unreliable, vehicle.instanceID, vehicle.asset.hash, physicsProfileHash, (byte) vehicle.asset.engine);
		}

		public static void exitVehicle()
		{
			if (Player.LocalPlayer.movement.getVehicle() != null)
			{
				SendExitVehicleRequest.Invoke(ENetReliability.Unreliable, Player.LocalPlayer.movement.getVehicle().GetComponent<Rigidbody>().velocity);
			}
		}

		public static void swapVehicle(byte toSeat)
		{
			if (Player.LocalPlayer.movement.getVehicle() != null)
			{
				SendSwapVehicleRequest.Invoke(ENetReliability.Unreliable, toSeat);
			}
		}

		public static void sendVehicleLock()
		{
			if (Player.LocalPlayer.movement.getVehicle() != null)
			{
				SendVehicleLockRequest.Invoke(ENetReliability.Unreliable);
			}
		}

		public static void sendVehicleSkin()
		{
			if (Player.LocalPlayer.movement.getVehicle() != null)
			{
				SendVehicleSkinRequest.Invoke(ENetReliability.Unreliable);
			}
		}

		/// <summary>
		/// Client-side request server to toggle headlights.
		/// </summary>
		public static void sendVehicleHeadlights()
		{
			InteractableVehicle vehicle = Player.LocalPlayer.movement.getVehicle();
			if (vehicle == null || vehicle.asset == null)
			{
				return; // Caller should have already checked this.
			}

			bool wantsHeadlightsOn = !vehicle.headlightsOn;

			if (!vehicle.asset.hasHeadlights)
			{
				// Server does not necessarily know whether the vehicle supports headlights (unfortunately),
				// so this is a hack to prevent clients from accidentally draining the battery.
				if (wantsHeadlightsOn) return;
			}

			SendToggleVehicleHeadlights.Invoke(ENetReliability.Unreliable, wantsHeadlightsOn);
		}

		/// <summary>
		/// As client request server to use bonus feature like towing hook or police sirens.
		/// </summary>
		public static void sendVehicleBonus()
		{
			InteractableVehicle vehicle = Player.LocalPlayer.movement.getVehicle();
			if (vehicle == null)
				return;

			// Client determines this mode because server does not know whether vehicle has these features.
			byte bonusType;
			if (vehicle.asset.hasSirens)
			{
				bonusType = 0;
			}
			else if (vehicle.asset.hasHook)
			{
				bonusType = 1;
			}
			else if (vehicle.asset.engine == EEngine.BLIMP)
			{
				bonusType = 2;
			}
			else
			{
				return;
			}

			SendUseVehicleBonus.Invoke(ENetReliability.Unreliable, bonusType);
		}

		public static void sendVehicleStealBattery()
		{
			if (Player.LocalPlayer.movement.getVehicle() != null)
			{
				SendStealVehicleBattery.Invoke(ENetReliability.Unreliable);
			}
		}

		public static void sendVehicleHorn()
		{
			InteractableVehicle vehicle = Player.LocalPlayer.movement.getVehicle();
			if (vehicle != null && vehicle.asset.hasHorn)
			{
				SendVehicleHornRequest.Invoke(ENetReliability.Unreliable);
			}
		}

		private static void WriteVehicle(NetPakWriter writer, InteractableVehicle vehicle)
		{
			Vector3 position;
			if (vehicle.asset.engine == EEngine.TRAIN)
			{
				position = InteractableVehicle.PackRoadPosition(vehicle.roadPosition);
			}
			else
			{
				position = vehicle.transform.position;
			}

			writer.WriteGuid(vehicle.asset.GUID);
			writer.WriteUInt16(vehicle.skinID);
			writer.WriteUInt16(vehicle.mythicID);
			writer.WriteClampedVector3(position, fracBitCount: POSITION_FRAC_BIT_COUNT);
			writer.WriteQuaternion(vehicle.transform.rotation, bitsPerComponent: ROTATION_BIT_COUNT);
			writer.WriteBit(vehicle.sirensOn);
			writer.WriteBit(vehicle.isBlimpFloating);
			writer.WriteBit(vehicle.headlightsOn);
			writer.WriteBit(vehicle.taillightsOn);
			writer.WriteUInt16(vehicle.fuel);
			writer.WriteBit(vehicle.isExploded);
			writer.WriteUInt16(vehicle.health);
			writer.WriteUInt16(vehicle.batteryCharge);
			writer.WriteSteamID(vehicle.lockedOwner);
			writer.WriteSteamID(vehicle.lockedGroup);
			writer.WriteBit(vehicle.isLocked);

			writer.WriteUInt8((byte) vehicle.passengers.Length);
			for (byte seat = 0; seat < vehicle.passengers.Length; seat++)
			{
				Passenger passenger = vehicle.passengers[seat];

				if (passenger.player != null)
				{
					writer.WriteSteamID(passenger.player.playerID.steamID);
				}
				else
				{
					writer.WriteSteamID(CSteamID.Nil);
				}
			}

			writer.WriteUInt32(vehicle.instanceID);
			writer.WriteUInt8(vehicle.tireAliveMask);
			writer.WriteNetId(vehicle.GetNetId());
			writer.WriteColor32RGBA(vehicle.PaintColor);

			if (vehicle.asset.replicatedWheelIndices != null)
			{
				// For initial send we include the number of replicated wheels, but not for subsequent state updates.
				// This way all vehicle assets will spawn correctly even if missing and we can kick the player when
				// they tell us they have a mismatched vehicle asset.
				writer.WriteUInt8((byte) vehicle.asset.replicatedWheelIndices.Length);
				foreach (int wheelIndex in vehicle.asset.replicatedWheelIndices)
				{
					Wheel wheel = vehicle.GetWheelAtIndex(wheelIndex);
					if (wheel == null)
					{
						UnturnedLog.error($"\"{vehicle.asset.FriendlyName}\" missing wheel for replicated index: {wheelIndex}");
						writer.WriteUnsignedNormalizedFloat(0.0f, 4); // Write something to avoid messing up offsets.
						continue;
					}

					writer.WriteUnsignedNormalizedFloat(wheel.replicatedSuspensionState, 4);
				}
			}
			else
			{
				writer.WriteUInt8(0);
			}
		}

		private static readonly ClientStaticMethod<uint, CSteamID, CSteamID, bool> SendVehicleLockState = ClientStaticMethod<uint, CSteamID, CSteamID, bool>.Get(ReceiveVehicleLockState);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleLock))]
		public static void ReceiveVehicleLockState(uint instanceID, CSteamID owner, CSteamID group, bool locked)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellLocked(owner, group, locked);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, ushort, ushort> SendVehicleSkin = ClientStaticMethod<uint, ushort, ushort>.Get(ReceiveVehicleSkin);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleSkin))]
		public static void ReceiveVehicleSkin(uint instanceID, ushort skinID, ushort mythicID)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellSkin(skinID, mythicID);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, bool> SendVehicleSirens = ClientStaticMethod<uint, bool>.Get(ReceiveVehicleSirens);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleSirens))]
		public static void ReceiveVehicleSirens(uint instanceID, bool on)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellSirens(on);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, bool> SendVehicleBlimp = ClientStaticMethod<uint, bool>.Get(ReceiveVehicleBlimp);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleBlimp))]
		public static void ReceiveVehicleBlimp(uint instanceID, bool on)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellBlimp(on);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, bool> SendVehicleHeadlights = ClientStaticMethod<uint, bool>.Get(ReceiveVehicleHeadlights);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleHeadlights))]
		public static void ReceiveVehicleHeadlights(uint instanceID, bool on)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellHeadlights(on);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint> SendVehicleHorn = ClientStaticMethod<uint>.Get(ReceiveVehicleHorn);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleHorn))]
		public static void ReceiveVehicleHorn(uint instanceID)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellHorn();
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, ushort> SendVehicleFuel = ClientStaticMethod<uint, ushort>.Get(ReceiveVehicleFuel);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleFuel))]
		public static void ReceiveVehicleFuel(uint instanceID, ushort newFuel)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellFuel(newFuel);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, ushort> SendVehicleBatteryCharge = ClientStaticMethod<uint, ushort>.Get(ReceiveVehicleBatteryCharge);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleBatteryCharge))]
		public static void ReceiveVehicleBatteryCharge(uint instanceID, ushort newBatteryCharge)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellBatteryCharge(newBatteryCharge);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, byte> SendVehicleTireAliveMask = ClientStaticMethod<uint, byte>.Get(ReceiveVehicleTireAliveMask);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleTireAliveMask))]
		public static void ReceiveVehicleTireAliveMask(uint instanceID, byte newTireAliveMask)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tireAliveMask = newTireAliveMask;
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint> SendVehicleExploded = ClientStaticMethod<uint>.Get(ReceiveVehicleExploded);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleExploded))]
		public static void ReceiveVehicleExploded(uint instanceID)
		{
			InteractableVehicle vehicle = findVehicleByNetInstanceID(instanceID);
			if (vehicle == null || vehicle.isExploded)
			{
				// Checks isExploded to prevent plugins from accidentally resetting the timer.
				return;
			}

			BarricadeManager.trimPlant(vehicle.transform);
			if (vehicle.trainCars != null)
			{
				for (int carIndex = 1; carIndex < vehicle.trainCars.Length; ++carIndex)
				{
					BarricadeManager.uprootPlant(vehicle.trainCars[carIndex].root);
				}
			}

			vehicle.tellExploded();
		}

		private static readonly ClientStaticMethod<uint, ushort> SendVehicleHealth = ClientStaticMethod<uint, ushort>.Get(ReceiveVehicleHealth);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleHealth))]
		public static void ReceiveVehicleHealth(uint instanceID, ushort newHealth)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellHealth(newHealth);
					return;
				}
			}
		}

		private static readonly ClientStaticMethod<uint, Vector3, int> SendVehicleRecov = ClientStaticMethod<uint, Vector3, int>.Get(ReceiveVehicleRecov);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleRecov))]
		public static void ReceiveVehicleRecov(uint instanceID, Vector3 newPosition, int newRecov)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicles[index].tellRecov(newPosition, newRecov);
					return;
				}
			}
		}

		private static uint seq;

		private static readonly ClientStaticMethod SendVehicleStates = ClientStaticMethod.Get(ReceiveVehicleStates);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveVehicleStates(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			uint newSeq;
			reader.ReadUInt32(out newSeq);
			if (newSeq <= seq)
				return;
			seq = newSeq;

			ushort count;
			reader.ReadUInt16(out count);
			if (count < 1)
				return;

			for (ushort index = 0; index < count; index++)
			{
				uint instanceID;
				reader.ReadUInt32(out instanceID);
				Vector3 position;
				reader.ReadClampedVector3(out position, fracBitCount: POSITION_FRAC_BIT_COUNT);
				Quaternion rotation;
				reader.ReadQuaternion(out rotation, bitsPerComponent: ROTATION_BIT_COUNT);
				float speed;
				reader.ReadUnsignedClampedFloat(SPEED_INT_BIT_COUNT, SPEED_FRAC_BIT_COUNT, out speed);
				float forwardVelocity;
				reader.ReadClampedFloat(FORWARD_VELOCITY_INT_BIT_COUNT, FORWARD_VELOCITY_FRAC_BIT_COUNT, out forwardVelocity);
				float steeringInput;
				reader.ReadSignedNormalizedFloat(STEERING_BIT_COUNT, out steeringInput);
				float velocityInput;
				reader.ReadClampedFloat(FORWARD_VELOCITY_INT_BIT_COUNT, FORWARD_VELOCITY_FRAC_BIT_COUNT, out velocityInput);
				bool includesHighQualityDetails;
				reader.ReadBit(out includesHighQualityDetails);

				InteractableVehicle vehicle = findVehicleByNetInstanceID(instanceID);
				if (vehicle == null)
					continue;

				vehicle.tellState(position, rotation, speed, forwardVelocity, steeringInput, velocityInput);

				if (vehicle.asset.replicatedWheelIndices != null)
				{
					foreach (int wheelIndex in vehicle.asset.replicatedWheelIndices)
					{
						Wheel wheel = vehicle.GetWheelAtIndex(wheelIndex);
						if (wheel == null)
						{
							UnturnedLog.error($"\"{vehicle.asset.FriendlyName}\" missing wheel for replicated index: {wheelIndex}");
							// Don't exit yet because we need to skip the appropriate number of bits if HQ.
						}

						if (includesHighQualityDetails)
						{
							if (wheel == null)
							{
								reader.ReadUnsignedNormalizedFloat(4, out float _);
								reader.ReadPhysicsMaterialNetId(out PhysicsMaterialNetId _);
								continue;
							}

							if (reader.ReadUnsignedNormalizedFloat(4, out float state))
							{
								wheel.replicatedSuspensionState = state;
							}

							reader.ReadPhysicsMaterialNetId(out wheel.replicatedGroundMaterial);
						}
						else
						{
							if (wheel != null)
							{
								wheel.replicatedSuspensionState = 1.0f;
								wheel.replicatedGroundMaterial = PhysicsMaterialNetId.NULL;
							}
						}
					}
				}

				if (vehicle.asset.UsesEngineRpmAndGears)
				{
					if (includesHighQualityDetails)
					{
						reader.ReadBits(GEAR_BIT_COUNT, out uint packedGear);
						int replicatedGear = ((int) packedGear) - 1;
						replicatedGear = Mathf.Clamp(replicatedGear, -1, vehicle.asset.forwardGearRatios.Length);
						vehicle.ChangeGears(replicatedGear);

						reader.ReadUnsignedNormalizedFloat(ENGINE_RPM_BIT_COUNT, out float normalizedRpm);
						vehicle.ReplicatedEngineRpm = Mathf.Lerp(vehicle.asset.EngineIdleRpm, vehicle.asset.EngineMaxRpm, normalizedRpm);
					}
					else
					{
						vehicle.ChangeGears(1);
						vehicle.ReplicatedEngineRpm = vehicle.asset.EngineIdleRpm;
					}
				}
			}
		}

		private static readonly ClientStaticMethod<uint> SendDestroySingleVehicle = ClientStaticMethod<uint>.Get(ReceiveDestroySingleVehicle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleDestroy))]
		public static void ReceiveDestroySingleVehicle(uint instanceID)
		{
			ThreadUtil.assertIsGameThread();

			InteractableVehicle vehicle = null;

			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicle = vehicles[index];
					vehicles.RemoveAt(index);
					break;
				}
			}

			if (vehicle == null)
			{
				return;
			}

			DestroyVehicleCommon(vehicle);
			respawnVehicleIndex--;
		}

		private static readonly ClientStaticMethod SendDestroyAllVehicles = ClientStaticMethod.Get(ReceiveDestroyAllVehicles);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVehicleDestroyAll))]
		public static void ReceiveDestroyAllVehicles()
		{
			ThreadUtil.assertIsGameThread();

			for (int index = vehicles.Count - 1; index >= 0; index--)
			{
				InteractableVehicle vehicle = vehicles[index];
				vehicles.RemoveAt(index);
				DestroyVehicleCommon(vehicle);
			}

			respawnVehicleIndex = 0;
			vehicles.Clear();
		}

		private static void DestroyVehicleCommon(InteractableVehicle vehicle)
		{
			// Nelson 2025-01-29: This fixes the case where player has requested exit from vehicle on the same frame
			// the server is destroying it. (public issue #4760)
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client == null)
					continue;

				Player player = client.player;
				if (player == null)
					continue;

				if (player.movement.getVehicle() == vehicle)
				{
					Debug.Assert(player.movement.hasPendingVehicleChange, "Player should already be exiting vehicle");
					Debug.Assert(player.movement.pendingVehicle == null, "Pending vehicle should be null");
					player.movement.ApplyPendingVehicleChange();
					Debug.Assert(!player.movement.hasPendingVehicleChange, "Pending change should be applied now");
					Debug.Assert(player.movement.getVehicle() == null, "Player should no longer be in vehicle");
				}

				// Note: *entering* vehicle case is already handled because player pending *enter* was in passengers list.
			}

			BarricadeManager.uprootPlant(vehicle.transform);
			if (vehicle.trainCars != null)
			{
				for (int carIndex = 1; carIndex < vehicle.trainCars.Length; carIndex++)
				{
					BarricadeManager.uprootPlant(vehicle.trainCars[carIndex].root);
				}
			}
			vehicle.IsPendingDestroy = true;
			OnPreDestroyVehicle?.TryInvoke("OnPreDestroyVehicle", vehicle);
			NetIdRegistry.ReleaseTransform(vehicle.GetNetId() + 1, vehicle.transform);
			vehicle.ReleaseNetId();
			EffectManager.ClearAttachments(vehicle.transform);
			Destroy(vehicle.gameObject);
		}

		public static void askVehicleDestroy(InteractableVehicle vehicle)
		{
			ThreadUtil.assertIsGameThread();

			if (Provider.isServer)
			{
				vehicle.forceRemoveAllPlayers();
				Debug.Assert(vehicle.isEmpty);
				// Nelson 2025-01-29: It is possible for vehicle to have Player component in children at this point
				// if a player exited on the same frame before askVehicleDestroy was called. This is resolved in
				// DestroyVehicleCommon.
				//Debug.Assert(vehicle.transform.GetComponentInChildren<Player>() == null, "Vehicle should not contain any Player components after removing passengers");
				SendDestroySingleVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID);
			}
		}

		public static void askVehicleDestroyAll()
		{
			ThreadUtil.assertIsGameThread();

			if (Provider.isServer)
			{
				for (int index = vehicles.Count - 1; index >= 0; index--)
				{
					InteractableVehicle vehicle = vehicles[index];
					vehicle.forceRemoveAllPlayers();
					Debug.Assert(vehicle.isEmpty, "Vehicle should be empty after removing passengers");
					// Nelson 2025-01-29: It is possible for vehicle to have Player component in children at this point
					// if a player exited on the same frame before askVehicleDestroy was called. This is resolved in
					// DestroyVehicleCommon.
					//Debug.Assert(vehicle.transform.GetComponentInChildren<Player>() == null, "Vehicle should not contain any Player components after removing passengers");
				}

				SendDestroyAllVehicles.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections());
			}
		}

		[System.Diagnostics.Conditional("LOG_RECEIVE_VEHICLE")]
		private static void LogReceiveVehicle(string name, object message)
		{
			UnturnedLog.info($"ReceiveVehicle {name}: {message}");
		}

		private static readonly ClientStaticMethod SendSingleVehicle = ClientStaticMethod.Get(ReceiveSingleVehicle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveSingleVehicle(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			System.Guid assetGuid;
			reader.ReadGuid(out assetGuid);

			LogReceiveVehicle(nameof(assetGuid), assetGuid);

			ushort skinID;
			reader.ReadUInt16(out skinID);

			LogReceiveVehicle(nameof(skinID), skinID);

			ushort mythicID;
			reader.ReadUInt16(out mythicID);

			LogReceiveVehicle(nameof(mythicID), mythicID);

			Vector3 position;
			reader.ReadClampedVector3(out position, fracBitCount: POSITION_FRAC_BIT_COUNT);

			LogReceiveVehicle(nameof(position), position);

			float roadPosition = InteractableVehicle.UnpackRoadPosition(position);

			LogReceiveVehicle(nameof(roadPosition), roadPosition);

			Quaternion rotation;
			reader.ReadQuaternion(out rotation, bitsPerComponent: ROTATION_BIT_COUNT);

			LogReceiveVehicle(nameof(rotation), rotation);

			bool sirens;
			reader.ReadBit(out sirens);

			LogReceiveVehicle(nameof(sirens), sirens);

			bool blimp;
			reader.ReadBit(out blimp);

			LogReceiveVehicle(nameof(blimp), blimp);

			bool headlights;
			reader.ReadBit(out headlights);

			LogReceiveVehicle(nameof(headlights), headlights);

			bool taillights;
			reader.ReadBit(out taillights);

			LogReceiveVehicle(nameof(taillights), taillights);

			ushort fuel;
			reader.ReadUInt16(out fuel);

			LogReceiveVehicle(nameof(fuel), fuel);

			bool isExploded;
			reader.ReadBit(out isExploded);

			LogReceiveVehicle(nameof(isExploded), isExploded);

			ushort health;
			reader.ReadUInt16(out health);

			LogReceiveVehicle(nameof(health), health);

			ushort batteryCharge;
			reader.ReadUInt16(out batteryCharge);

			LogReceiveVehicle(nameof(batteryCharge), batteryCharge);

			CSteamID ownerID;
			reader.ReadSteamID(out ownerID);

			LogReceiveVehicle(nameof(ownerID), ownerID);

			CSteamID groupID;
			reader.ReadSteamID(out groupID);

			LogReceiveVehicle(nameof(groupID), groupID);

			bool isLocked;
			reader.ReadBit(out isLocked);

			LogReceiveVehicle(nameof(isLocked), isLocked);

			byte passengersLength;
			reader.ReadUInt8(out passengersLength);

			LogReceiveVehicle(nameof(passengersLength), passengersLength);

			CSteamID[] passengers = new CSteamID[passengersLength];
			for (int step = 0; step < passengers.Length; step++)
			{
				reader.ReadSteamID(out passengers[step]);
				LogReceiveVehicle($"passengers[{step}]", passengers[step]);
			}

			uint instanceID;
			reader.ReadUInt32(out instanceID);

			LogReceiveVehicle(nameof(instanceID), instanceID);

			byte tireAliveMask;
			reader.ReadUInt8(out tireAliveMask);

			LogReceiveVehicle(nameof(tireAliveMask), tireAliveMask);

			NetId netId;
			reader.ReadNetId(out netId);

			LogReceiveVehicle(nameof(netId), netId);

			Color32 paintColor;
			reader.ReadColor32RGBA(out paintColor);

			LogReceiveVehicle(nameof(paintColor), paintColor);

			InteractableVehicle vehicle = manager.addVehicle(assetGuid, skinID, mythicID, roadPosition, position, rotation, sirens, blimp, headlights, taillights, fuel, isExploded, health, batteryCharge, ownerID, groupID, isLocked, passengers, null, instanceID, tireAliveMask, netId, paintColor);

			byte replicatedWheelCount;
			reader.ReadUInt8(out replicatedWheelCount);

			LogReceiveVehicle(nameof(replicatedWheelCount), replicatedWheelCount);

			for (int readWheelIndex = 0; readWheelIndex < replicatedWheelCount; ++readWheelIndex)
			{
				float replicatedWheelState;
				reader.ReadUnsignedNormalizedFloat(4, out replicatedWheelState);

				LogReceiveVehicle(nameof(replicatedWheelState), replicatedWheelState);

				if (vehicle != null && vehicle.asset != null && vehicle.asset.replicatedWheelIndices != null)
				{
					if (readWheelIndex >= vehicle.asset.replicatedWheelIndices.Length)
						continue;

					int actualWheelIndex = vehicle.asset.replicatedWheelIndices[readWheelIndex];
					Wheel wheel = vehicle.GetWheelAtIndex(actualWheelIndex);
					if (wheel == null)
						continue;

					wheel.TeleportSuspensionState(replicatedWheelState);
				}
			}

			LogReceiveVehicle(nameof(reader.RemainingSegmentLength), reader.RemainingSegmentLength);
		}
		
		private static readonly ClientStaticMethod SendMultipleVehicles = ClientStaticMethod.Get(ReceiveMultipleVehicles);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveMultipleVehicles(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			ushort count;
			reader.ReadUInt16(out count);

			LogReceiveVehicle("ReceiveMultipleVehicles.count", count);

			for (int index = 0; index < count; index++)
			{
				ReceiveSingleVehicle(context);
			}

			Level.isLoadingVehicles = false;
		}

		/// <summary>
		/// Helper for servers with huge numbers of vehicles.
		/// Called with fixed span of indexes e.g. [0, 10), then [10, 20). This function then clamps the final span to the vehicle count.
		/// </summary>
		private static void askVehiclesHelper(ITransportConnection transportConnection, int startIndex, int endIndex)
		{
			if (endIndex > vehicles.Count)
				endIndex = vehicles.Count;

			int count = endIndex - startIndex;
			if (count < 1)
				throw new System.ArgumentException("startIndex or endIndex to askVehiclesHelper invalid");

			SendMultipleVehicles.Invoke(ENetReliability.Reliable, transportConnection, SendMultipleVehicles_Write,
				startIndex, endIndex);
		}

		private static void SendMultipleVehicles_Write(NetPakWriter writer, int startIndex, int endIndex)
		{
			int count = endIndex - startIndex;
			writer.WriteUInt16((ushort) count);
			for (int index = startIndex; index < endIndex; ++index)
			{
				InteractableVehicle vehicle = vehicles[index];
				WriteVehicle(writer, vehicle);
			}
		}
		
		internal static void SendInitialGlobalState(SteamPlayer client)
		{
			const int MAX_TELLVEHICLES_PER_PACKET = 50;

			int totalVehicleCount = vehicles.Count;
			if (totalVehicleCount > 0)
			{
				int requiredPacketCount = ((totalVehicleCount - 1) / MAX_TELLVEHICLES_PER_PACKET) + 1; // Rounds up

				int startIndex = 0;
				for (int packetIndex = 0; packetIndex < requiredPacketCount; ++packetIndex)
				{
					int endIndex = startIndex + MAX_TELLVEHICLES_PER_PACKET;
					askVehiclesHelper(client.transportConnection, startIndex, endIndex);
					startIndex = endIndex;
				}
			}
			else
			{
				SendMultipleVehicles.Invoke(ENetReliability.Reliable, client.transportConnection, SendMultipleVehicles_WriteEmpty);
			}

			BarricadeManager.SendVehicleRegions(client);
		}

		private static void SendMultipleVehicles_WriteEmpty(NetPakWriter writer)
		{
			writer.WriteUInt16(0);
		}

		internal static readonly ClientStaticMethod<uint, byte, CSteamID> SendEnterVehicle = ClientStaticMethod<uint, byte, CSteamID>.Get(ReceiveEnterVehicle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellEnterVehicle))]
		public static void ReceiveEnterVehicle(uint instanceID, byte seat, CSteamID player)
		{
			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					InteractableVehicle vehicle = vehicles[index];

					vehicle.addPlayer(seat, player);
					return;
				}
			}
		}

		/// <summary>
		/// Plugin devs: if you are using reflection to call this even though it's private, please use sendExitVehicle
		/// instead which properly handles player culling.
		/// </summary>
		private static readonly ClientStaticMethod<uint, byte, Vector3, byte, bool> SendExitVehicle = ClientStaticMethod<uint, byte, Vector3, byte, bool>.Get(ReceiveExitVehicle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellExitVehicle))]
		public static void ReceiveExitVehicle(uint instanceID, byte seat, Vector3 point, byte angle, bool forceUpdate)
		{
			InteractableVehicle vehicle = findVehicleByNetInstanceID(instanceID);
			if (vehicle != null)
			{
				vehicle.removePlayer(seat, point, angle, forceUpdate);
			}
		}

		private static readonly ClientStaticMethod<uint, byte, byte> SendSwapVehicleSeats = ClientStaticMethod<uint, byte, byte>.Get(ReceiveSwapVehicleSeats);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSwapVehicle))]
		public static void ReceiveSwapVehicleSeats(uint instanceID, byte fromSeat, byte toSeat)
		{
			InteractableVehicle vehicle = findVehicleByNetInstanceID(instanceID);
			if (vehicle != null)
			{
				vehicle.swapPlayer(fromSeat, toSeat);
			}
		}

		public static void unlockVehicle(InteractableVehicle vehicle, Player instigatingPlayer)
		{
			if (vehicle == null)
			{
				return;
			}

			bool allow = true;
			onVehicleLockpicked?.Invoke(vehicle, instigatingPlayer, ref allow);

			if (!allow)
			{
				return;
			}

			ServerSetVehicleLock(vehicle, CSteamID.Nil, CSteamID.Nil, false);
			EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
		}

		public static void carjackVehicle(InteractableVehicle vehicle, Player instigatingPlayer, Vector3 force, Vector3 torque)
		{
			if (!vehicle.isEmpty)
				return;

			if (vehicle.asset != null)
			{
				VehiclePhysicsProfileAsset physicsProfile = vehicle.asset.physicsProfileRef.Find();
				if (physicsProfile != null && physicsProfile.carjackForceMultiplier.HasValue)
				{
					force *= physicsProfile.carjackForceMultiplier.Value;
				}

				force *= vehicle.asset.carjackForceMultiplier;
			}

			bool allow = true;
			onVehicleCarjacked?.Invoke(vehicle, instigatingPlayer, ref allow, ref force, ref torque);

			if (!allow)
			{
				return;
			}

			Rigidbody rb = vehicle.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.AddForce(force);
				rb.AddTorque(torque);
			}
		}

		public static ushort siphonFromVehicle(InteractableVehicle vehicle, Player instigatingPlayer, ushort desiredAmount)
		{
			bool allow = true;
			onSiphonVehicleRequested?.Invoke(vehicle, instigatingPlayer, ref allow, ref desiredAmount);

			if (!allow)
				return 0;

			if (desiredAmount > vehicle.fuel)
			{
				desiredAmount = vehicle.fuel;
			}

			if (desiredAmount < 1)
				return 0;

			vehicle.askBurnFuel(desiredAmount);
			sendVehicleFuel(vehicle, vehicle.fuel);
			return desiredAmount;
		}

		public delegate void ToggleVehicleLockRequested(InteractableVehicle vehicle, ref bool shouldAllow);
		public static event ToggleVehicleLockRequested OnToggleVehicleLockRequested;
		public static event System.Action<InteractableVehicle> OnToggledVehicleLock;

		public static void ServerSetVehicleLock(InteractableVehicle vehicle, CSteamID ownerID, CSteamID groupID, bool isLocked)
		{
			SendVehicleLockState.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, ownerID, groupID, isLocked);
		}
		
		private static readonly ServerStaticMethod SendVehicleLockRequest = ServerStaticMethod.Get(ReceiveVehicleLockRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 4, legacyName = nameof(askVehicleLock))]
		public static void ReceiveVehicleLockRequest(in ServerInvocationContext context)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null || vehicle.asset == null)
			{
				return;
			}

			if (!vehicle.checkDriver(player.channel.owner.playerID.steamID))
			{
				return;
			}

			bool oldLocked = vehicle.isLocked;
			bool newLocked;

			if (vehicle.asset.canBeLocked)
			{
				newLocked = !oldLocked;
			}
			else
			{
				newLocked = false;
			}

			if (oldLocked == newLocked)
				return;

			bool shouldAllow = true;
			OnToggleVehicleLockRequested?.Invoke(vehicle, ref shouldAllow);
			if (!shouldAllow)
				return;

			ServerSetVehicleLock(vehicle, player.channel.owner.playerID.steamID, player.quests.groupID, newLocked);
			EffectManager.TriggerFiremodeEffect(vehicle.transform.position);

			OnToggledVehicleLock.TryInvoke("OnToggledVehicleLock", vehicle);
		}
		
		private static readonly ServerStaticMethod SendVehicleSkinRequest = ServerStaticMethod.Get(ReceiveVehicleSkinRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askVehicleSkin))]
		public static void ReceiveVehicleSkinRequest(in ServerInvocationContext context)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			if (!vehicle.checkDriver(player.channel.owner.playerID.steamID))
			{
				return;
			}

			int item = 0;
			ushort skinID = 0;
			ushort mythicID = 0;
			if (player.channel.owner.skinItems != null && player.channel.owner.GetVehicleSkinItemDefId(vehicle, out item))
			{
				skinID = Provider.provider.economyService.getInventorySkinID(item);
				mythicID = Provider.provider.economyService.getInventoryMythicID(item);
			}

			if (skinID != 0)
			{
				if (skinID == vehicle.skinID && mythicID == vehicle.mythicID) // If it's not our paintjob then use ours
				{
					skinID = 0;
					mythicID = 0;
				}
			}
			else
			{
				if (!vehicle.isSkinned)
				{
					return; // Already not skinned so don't bother
				}
			}

			SendVehicleSkin.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, skinID, mythicID);
		}
		
		private static readonly ServerStaticMethod<bool> SendToggleVehicleHeadlights = ServerStaticMethod<bool>.Get(ReceiveToggleVehicleHeadlights);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = nameof(askVehicleHeadlights))]
		public static void ReceiveToggleVehicleHeadlights(in ServerInvocationContext context, bool wantsHeadlightsOn)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			if (wantsHeadlightsOn == vehicle.headlightsOn)
			{
				// Client sends want they want to happen rather than simply toggling to prevent
				// duplicate requests from accidentally reverting to the prior state.
				return;
			}

			if (!vehicle.canTurnOnLights)
			{
				return;
			}

			if (!vehicle.checkDriver(player.channel.owner.playerID.steamID))
			{
				return;
			}

			if (!vehicle.asset.hasHeadlights)
			{
				return;
			}

			SendVehicleHeadlights.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, wantsHeadlightsOn);
			EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
		}
		
		private static readonly ServerStaticMethod<byte> SendUseVehicleBonus = ServerStaticMethod<byte>.Get(ReceiveUseVehicleBonus);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 5, legacyName = nameof(askVehicleBonus))]
		public static void ReceiveUseVehicleBonus(in ServerInvocationContext context, byte bonusType)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			if (!vehicle.checkDriver(player.channel.owner.playerID.steamID))
			{
				return;
			}

			if (bonusType == 0)
			{
				if (!vehicle.canTurnOnLights)
				{
					return;
				}

				SendVehicleSirens.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, !vehicle.sirensOn);
				EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
			}
			else if (bonusType == 1)
			{
				vehicle.useHook();
			}
			else if (bonusType == 2)
			{
				SendVehicleBlimp.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, !vehicle.isBlimpFloating);
			}
		}
		
		private static readonly ServerStaticMethod SendStealVehicleBattery = ServerStaticMethod.Get(ReceiveStealVehicleBattery);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askVehicleStealBattery))]
		public static void ReceiveStealVehicleBattery(in ServerInvocationContext context)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			if (!vehicle.checkDriver(player.channel.owner.playerID.steamID))
			{
				return;
			}

			if (!vehicle.usesBattery)
			{
				return;
			}

			if (!vehicle.ContainsBatteryItem)
			{
				return;
			}

			if (!vehicle.asset.canStealBattery)
				return;

			vehicle.stealBattery(player);
		}
		
		private static readonly ServerStaticMethod SendVehicleHornRequest = ServerStaticMethod.Get(ReceiveVehicleHornRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = nameof(askVehicleHorn))]
		public static void ReceiveVehicleHornRequest(in ServerInvocationContext context)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			if (!vehicle.asset.hasHorn)
			{
				return;
			}

			if (!vehicle.canUseHorn)
			{
				return;
			}

			if (!vehicle.checkDriver(player.channel.owner.playerID.steamID))
			{
				return;
			}

			SendVehicleHorn.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID);
		}
		
		private static readonly ServerStaticMethod<uint, byte[], byte[], byte> SendEnterVehicleRequest = ServerStaticMethod<uint, byte[], byte[], byte>.Get(ReceiveEnterVehicleRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askEnterVehicle))]
		public static void ReceiveEnterVehicleRequest(in ServerInvocationContext context, uint instanceID, byte[] hash, byte[] physicsProfileHash, byte engine)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				context.LogWarning("null player");
				return;
			}

			if (player.life.isDead)
			{
				context.LogWarning("player is dead");
				return;
			}

			if (player.equipment.isBusy)
			{
				context.LogWarning("equipment is busy");
				return;
			}

			if (LevelManager.isArenaMode)
			{
				if (!LevelManager.isPlayerInArena(player))
				{
					// Apparently cheaters are using vehicle speed/fly hacks (already somewhat validated serverside) to
					// get to the arena lobby and then load their friends back up to get into the fight, effectively respawning.
					context.LogWarning("player is not in arena");
					return;
				}
			}

			if (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished)
			{
				context.LogWarning("equipment is selected but not yet equipped");
				return;
			}

			if (player.movement.getVehicle() != null)
			{
				context.LogWarning("player is seated");
				return;
			}

			InteractableVehicle vehicle = null;

			for (int index = 0; index < vehicles.Count; index++)
			{
				if (vehicles[index].instanceID == instanceID)
				{
					vehicle = vehicles[index];
					break;
				}
			}

			if (vehicle == null)
			{
				context.LogWarning("cannot find vehicle");
				return;
			}

			if (vehicle.asset.shouldVerifyHash && !Hash.verifyHash(hash, vehicle.asset.hash))
			{
				context.LogWarning("hash does not match");
				return;
			}

			if (physicsProfileHash.Length == 0)
			{
				VehiclePhysicsProfileAsset physicsProfile = vehicle.asset.physicsProfileRef.Find();
				if (physicsProfile != null)
				{
					// Client does not think the vehicle has a physics profile, but server does, so do not allow entry.
					context.LogWarning($"client null physics profile, server has physics profile ({physicsProfile.name})");
					return;
				}
			}
			else if (physicsProfileHash.Length == 20)
			{
				VehiclePhysicsProfileAsset physicsProfile = vehicle.asset.physicsProfileRef.Find();
				if (physicsProfile != null)
				{
					if (!Hash.verifyHash(physicsProfileHash, physicsProfile.hash))
					{
						// Client physics profile does not match server, so do not allow entry.
						context.LogWarning("physics profile hash does not match");
						return;
					}
				}
				else
				{
					// Client thinks the vehicle has a physics profile, but server does not, so do not allow entry.
					// Most likely the client added a physics profile to cheat boost speed.
					context.LogWarning("client has physics profile, server does not");
					return;
				}
			}
			else
			{
				context.Kick("invalid vehicle physics profile hash");
				return;
			}

			if ((EEngine) engine != vehicle.asset.engine)
			{
				context.LogWarning("engine does not match");
				return;
			}

			if (vehicle.IsPendingDestroy)
			{
				context.LogWarning("vehicle is destroyed");
				return;
			}

			if ((vehicle.transform.position - player.transform.position).sqrMagnitude > 100)
			{
				context.LogWarning("too far away");
				return;
			}

			if (!vehicle.checkEnter(player))
			{
				context.LogWarning("not allowed to enter");
				return;
			}

			byte seat;
			if (!vehicle.tryAddPlayer(out seat, player))
			{
				context.LogWarning("cannot add player");
				return;
			}

			RaycastHit entryHit;
			Transform seatTransform = vehicle.passengers[seat].seat;
			Vector3 seatBottom = seatTransform.position; // Player's capsule bottom in seat.
			Vector3 seatTop = seatTransform.position + (seatTransform.up * 2); // Player's capsule top in seat.
			Vector3 playerCenter = player.transform.position + Vector3.up;
			bool bHitSomething = Physics.Linecast(playerCenter, seatBottom, out entryHit, RayMasks.BLOCK_ENTRY, QueryTriggerInteraction.Ignore);
#if WITH_VEHICLE_ENTER_GIZMOS
			GizmosUtil.Get().Linecast(playerCenter, seatBottom, entryHit, Color.green, Color.red, 5.0f);
#endif // WITH_VEHICLE_ENTER_GIZMOS
			if (!bHitSomething)
			{
				bHitSomething = Physics.Linecast(playerCenter, seatTop, out entryHit, RayMasks.BLOCK_ENTRY, QueryTriggerInteraction.Ignore);
#if WITH_VEHICLE_ENTER_GIZMOS
				GizmosUtil.Get().Linecast(playerCenter, seatTop, entryHit, Color.green, Color.red, 5.0f);
#endif // WITH_VEHICLE_ENTER_GIZMOS
			}

			if (bHitSomething && !entryHit.transform.IsChildOf(vehicle.transform))
			{
				// Prevent entering vehicles whose center is maybe wedged into another object.
				context.LogWarning($"obstructed by {entryHit.ToDebugString()}");
				return;
			}

			if (onEnterVehicleRequested != null)
			{
				bool shouldAllow = true;
				onEnterVehicleRequested.Invoke(player, vehicle, ref shouldAllow);
				if (shouldAllow == false)
					return;
			}

			SendEnterVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), instanceID, seat, player.channel.owner.playerID.steamID);
		}

		/// <summary>
		/// Does as few tests as possible while maintaining base game expectations.
		/// </summary>
		public static bool ServerForcePassengerIntoVehicle(Player player, InteractableVehicle vehicle)
		{
			if (player == null)
				throw new System.ArgumentNullException(nameof(player));

			if (vehicle == null)
				throw new System.ArgumentNullException(nameof(vehicle));

			if (player.life.isDead)
				return false;

			if (player.equipment.isBusy)
				return false;

			if (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished)
				return false;

			if (player.movement.getVehicle() != null)
				return false;

			byte seat;
			if (!vehicle.tryAddPlayer(out seat, player))
				return false;

			SendEnterVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, seat, player.channel.owner.playerID.steamID);
			return true;
		}
		
		private static readonly ServerStaticMethod<Vector3> SendExitVehicleRequest = ServerStaticMethod<Vector3>.Get(ReceiveExitVehicleRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askExitVehicle))]
		public static void ReceiveExitVehicleRequest(in ServerInvocationContext context, Vector3 velocity)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if (player.equipment.isBusy)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			byte seat;
			Vector3 point;
			byte angle;
			if (vehicle.forceRemovePlayer(out seat, player.channel.owner.playerID.steamID, out point, out angle) == false)
				return;

			if (onExitVehicleRequested != null)
			{
				bool shouldAllow = true;

				float yaw = MeasurementTool.byteToAngle(angle);
				onExitVehicleRequested.Invoke(player, vehicle, ref shouldAllow, ref point, ref yaw);
				angle = MeasurementTool.angleToByte(yaw);

				if (shouldAllow == false)
					return;
			}

			sendExitVehicle(vehicle, seat, point, angle, false);

			if (seat == 0 && Dedicator.IsDedicatedServer)
			{
				vehicle.GetComponent<Rigidbody>().velocity = velocity;
			}
		}

		public static void forceRemovePlayer(InteractableVehicle vehicle, CSteamID player)
		{
			ThreadUtil.assertIsGameThread();

			byte seat;
			Vector3 point;
			byte angle;
			if (vehicle.forceRemovePlayer(out seat, player, out point, out angle))
			{
				sendExitVehicle(vehicle, seat, point, angle, true);
			}
		}

		/// <summary>
		/// Force remove player from vehicle they were in, if any.
		/// Called when player disconnects to tidy up and run callbacks.
		/// </summary>
		/// <returns>True if player was in a vehicle, false otherwise.</returns>
		public static bool forceRemovePlayer(CSteamID player)
		{
			ThreadUtil.assertIsGameThread();

			InteractableVehicle wasInVehicle = null;
			byte seat = 0;
			Vector3 point = Vector3.zero;
			byte angle = 0;

			foreach (InteractableVehicle vehicle in vehicles)
			{
				if (vehicle == null)
					continue;

				if (vehicle.forceRemovePlayer(out seat, player, out point, out angle))
				{
					wasInVehicle = vehicle;
					break; // Should only have been in a single vehicle.
				}
			}

			if (wasInVehicle != null)
			{
				sendExitVehicle(wasInVehicle, seat, point, angle, true);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Remove player from vehicle and teleport them to an unchecked destination.
		/// </summary>
		public static bool removePlayerTeleportUnsafe(InteractableVehicle vehicle, Player player, Vector3 position, float yaw)
		{
			byte seat;
			if (vehicle.findPlayerSeat(player, out seat))
			{
				byte netYaw = MeasurementTool.angleToByte(yaw);
				sendExitVehicle(vehicle, seat, position, netYaw, false);
				return true;
			}
			else
			{
				return false;
			}
		}
		
		private static readonly ServerStaticMethod<byte> SendSwapVehicleRequest = ServerStaticMethod<byte>.Get(ReceiveSwapVehicleRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askSwapVehicle))]
		public static void ReceiveSwapVehicleRequest(in ServerInvocationContext context, byte toSeat)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if (player.equipment.isBusy)
			{
				return;
			}

			if (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished)
			{
				return;
			}

			InteractableVehicle vehicle = player.movement.getVehicle();

			if (vehicle == null)
			{
				return;
			}

			if (Time.realtimeSinceStartup - vehicle.lastSeat < 1.0f)
			{
				return;
			}
			vehicle.lastSeat = Time.realtimeSinceStartup;

			byte fromSeat;
			if (vehicle.trySwapPlayer(player, toSeat, out fromSeat) == false)
				return;

			if (onSwapSeatRequested != null)
			{
				bool shouldAllow = true;
				onSwapSeatRequested.Invoke(player, vehicle, ref shouldAllow, fromSeat, ref toSeat);

				if (shouldAllow == false)
					return;

				// Test again in-case toSeat was modified by plugin.
				if (vehicle.trySwapPlayer(player, toSeat, out fromSeat) == false)
					return;
			}

			SendSwapVehicleSeats.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, fromSeat, toSeat);
		}

		/// <summary>
		/// Handles culling if exit position is not visible to certain clients.
		/// If adjusting how this works, PlayerLife.SendReviveTeleport may need revision.
		/// </summary>
		public static void sendExitVehicle(InteractableVehicle vehicle, byte seat, Vector3 point, byte angle, bool forceUpdate)
		{
			SteamPlayer teleportingClient = vehicle.GetClientBySeatIndex(seat);
			if (teleportingClient == null)
			{
				// Bug?
				// Adding this case as part of player culling. Previously, this would've gone through to all remote
				// clients regardless, so we'll preserve that for backwards compatibility just in case.
				SendExitVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, seat, point, angle, forceUpdate);
				return;
			}

			// Prior to player culling this used Provider.GatherRemoteClientConnections().
			// Now, we send a fake position to clients who shouldn't know the new position.
			// Please refer to Player.GatherTeleportRemoteClientConnections for more info.
			PooledTransportConnectionList visibleClients = TransportConnectionListPool.Get();
			PooledTransportConnectionList culledClients = TransportConnectionListPool.Get();

			foreach (SteamPlayer client in Provider._clients)
			{
#if !DEDICATED_SERVER
				if (client.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (client == teleportingClient)
				{
					// Always notify self of the teleport.
					visibleClients.Add(client.transportConnection);
					continue;
				}

				if (client.model == null) // error/bug?
				{
					visibleClients.Add(client.transportConnection);
					continue;
				}

				Vector3 recipientPosition = client.model.transform.position;
				bool culled = PlayerManager.IsPlayerCulledAtPosition(teleportingClient, point, client, recipientPosition);
				if (culled)
				{
					culledClients.Add(client.transportConnection);
				}
				else
				{
					visibleClients.Add(client.transportConnection);
				}
			}

			// Always invoke (even if empty) so loopback is called.
			SendExitVehicle.InvokeAndLoopback(ENetReliability.Reliable, visibleClients, vehicle.instanceID, seat, point, angle, forceUpdate);

			if (culledClients.Count > 0)
			{
				// No loopback.
				SendExitVehicle.Invoke(ENetReliability.Reliable, culledClients, vehicle.instanceID, seat, PlayerManager.CulledPosition, angle, forceUpdate);
			}
		}

		public static void sendVehicleFuel(InteractableVehicle vehicle, ushort newFuel)
		{
			SendVehicleFuel.Invoke(ENetReliability.Unreliable, Provider.GatherClientConnections(), vehicle.instanceID, newFuel);
		}

		public static void sendVehicleBatteryCharge(InteractableVehicle vehicle, ushort newBatteryCharge)
		{
			SendVehicleBatteryCharge.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, newBatteryCharge);
		}

		public static void sendVehicleTireAliveMask(InteractableVehicle vehicle, byte newTireAliveMask)
		{
			SendVehicleTireAliveMask.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, newTireAliveMask);
		}

		public static System.Action<InteractableVehicle> OnVehicleExploded;

		public static void sendVehicleExploded(InteractableVehicle vehicle)
		{
			SendVehicleExploded.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID);

			OnVehicleExploded.TryInvoke("OnVehicleExploded", vehicle);
		}

		public static void sendVehicleHealth(InteractableVehicle vehicle, ushort newHealth)
		{
			SendVehicleHealth.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, newHealth);
		}

		public static void sendVehicleRecov(InteractableVehicle vehicle, Vector3 newPosition, int newRecov)
		{
			if (vehicle.passengers[0].player != null)
			{
				SendVehicleRecov.Invoke(ENetReliability.Reliable, vehicle.passengers[0].player.transportConnection, vehicle.instanceID, newPosition, newRecov);
			}
		}

		private InteractableVehicle addVehicle(System.Guid assetGuid, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, CSteamID[] passengers, byte[][] turrets, uint instanceID, byte tireAliveMask, NetId netId, Color32 paintColor)
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			VehicleAsset asset = Assets.find(assetGuid) as VehicleAsset;
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, "Vehicle");
			}
			if (asset == null)
			{
				return null;
			}

			GameObject modelPrefab = asset.GetOrLoadModel();
			if (modelPrefab == null)
			{
				Assets.ReportError(asset, "unable to spawn any gameobject");
				return null;
			}

			// Nelson 2023-11-08: NPCs grant locked vehicles by default, but a player pointed
			// out that certain vehicles are not designed to be locked. (public issue #4198)
			if (!asset.canBeLocked)
			{
				owner = CSteamID.Nil;
				group = CSteamID.Nil;
				locked = false;
			}

			InteractableVehicle character = null;

			try
			{

				Transform vehicle = Instantiate(modelPrefab, point, angle).transform;
				vehicle.name = asset.id.ToString(); // Backwards compatibility

				Rigidbody rb = vehicle.GetOrAddComponent<Rigidbody>();
				rb.useGravity = true;
				rb.isKinematic = false;

				character = vehicle.gameObject.AddComponent<InteractableVehicle>();
				character.roadPosition = roadPosition;
				character.instanceID = instanceID;
				character.AssignNetId(netId);
				character.id = asset.id;
				character.skinID = skinID;
				character.mythicID = mythicID;
				character.fuel = fuel;
				character.isExploded = isExploded;
				character.health = health;
				character.batteryCharge = batteryCharge;
				character.PaintColor = paintColor;

				character.init(asset);
				character.gatherVehicleColliders();
				character.tellSirens(sirens);
				character.tellBlimp(blimp);
				character.tellHeadlights(headlights);
				character.tellTaillights(taillights);
				character.tellLocked(owner, group, locked);
				character.tireAliveMask = tireAliveMask;

				if (Provider.isServer)
				{
					if (turrets != null && turrets.Length == character.turrets.Length)
					{
						for (byte turret = 0; turret < character.turrets.Length; turret++)
						{
							character.turrets[turret].state = turrets[turret];
						}
					}
					else
					{
						for (byte turret = 0; turret < character.turrets.Length; turret++)
						{
							ItemAsset item = Assets.find(EAssetType.ITEM, asset.turrets[turret].itemID) as ItemAsset;

							if (item != null)
							{
								character.turrets[turret].state = item.getState();
							}
							else
							{
								character.turrets[turret].state = null;
							}
						}
					}
				}

				if (passengers != null)
				{
					for (byte seat = 0; seat < passengers.Length; seat++)
					{
						if (passengers[seat] != CSteamID.Nil)
						{
							character.addPlayer(seat, passengers[seat]);
						}
					}
				}

				if (asset.trunkStorage_Y > 0)
				{
					character.trunkItems = new Items(PlayerInventory.STORAGE);
					character.trunkItems.resize(asset.trunkStorage_X, asset.trunkStorage_Y);
				}

				vehicles.Add(character);

				NetIdRegistry.AssignTransform(++netId, character.transform);
				BarricadeManager.registerVehicleRegion(character.transform, character, 0, ++netId);
				if (character.trainCars != null)
				{
					for (int carIndex = 1; carIndex < character.trainCars.Length; carIndex++)
					{
						BarricadeManager.registerVehicleRegion(character.trainCars[carIndex].root, character, carIndex, ++netId);
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.warn("Exception while spawning vehicle: {0}", asset.name);
				UnturnedLog.exception(exception);
			}

			return character;
		}

		/// <summary>
		/// Is spawnpoint open for vehicle?
		/// </summary>
		private bool canUseSpawnpoint(VehicleSpawnpoint spawn)
		{
			const float minSpacing = 8;
			const float sqrMinSpacing = minSpacing * minSpacing;

			foreach (InteractableVehicle vehicle in vehicles)
			{
				if (vehicle == null)
					continue;

				if ((vehicle.transform.position - spawn.point).sqrMagnitude < sqrMinSpacing)
				{
					// Another vehicle is too near to spawn point.
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Try to find a random spawnpoint to spawn a vehicle while server is running.
		/// </summary>
		private VehicleSpawnpoint findRandomSpawn()
		{
			List<VehicleSpawnpoint> spawns = LevelVehicles.spawns;
			if (spawns.Count < 1)
				return null;

			int randomIndex = Random.Range(0, spawns.Count);
			VehicleSpawnpoint randomSpawn = spawns[randomIndex];
			if (randomSpawn != null && canUseSpawnpoint(randomSpawn))
			{
				return randomSpawn;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Add a new vehicle at given spawnpoint.
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		private InteractableVehicle addVehicleAtSpawn(VehicleSpawnpoint spawn)
		{
			if (spawn == null)
				return null;

			Asset asset = LevelVehicles.GetRandomAssetForSpawnpoint(spawn);
			if (asset == null)
				return null;

			VehicleAsset vehicleAsset;
			Color32 paintColor = new Color32(0, 0, 0, 0);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				vehicleAsset = redirectorAsset.TargetVehicle.Find();
				if (redirectorAsset.SpawnPaintColor.HasValue)
				{
					paintColor = redirectorAsset.SpawnPaintColor.Value;
				}
			}
			else
			{
				vehicleAsset = asset as VehicleAsset;
			}

			if (vehicleAsset == null)
			{
				return null;
			}

			Vector3 point = spawn.point;
			point.y += 0.5f;

			NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_VEHICLE);
			InteractableVehicle spawnedVehicle = addVehicle(vehicleAsset.GUID, 0, 0, 0.0f, point, Quaternion.Euler(0, spawn.angle, 0), false, false, false, false, ushort.MaxValue, false, ushort.MaxValue, ushort.MaxValue, CSteamID.Nil, CSteamID.Nil, false, null, null, allocateInstanceID(), getVehicleRandomTireAliveMask(vehicleAsset), netId, paintColor);
			if (spawnedVehicle != null)
			{
				spawnedVehicle.WasNaturallySpawned = true;
				// Doesn't call NotifyFirstSpawned because addVehicleAtSpawnAndReplicate is separate. (called there)
			}
			return spawnedVehicle;
		}

		/// <summary>
		/// Add a new vehicle at given spawnpoint and replicate to clients.
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		private void addVehicleAtSpawnAndReplicate(VehicleSpawnpoint spawn)
		{
			InteractableVehicle character = addVehicleAtSpawn(spawn);
			if (character != null)
			{
				SendSingleVehicle.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteVehicle, character);
				character.NotifyFirstSpawned();
			}
		}

		private bool respawnVehicles_Destroy()
		{
			if (respawnVehicleIndex >= vehicles.Count)
			{
				// Vehicle may have been removed between calls.
				respawnVehicleIndex = (ushort) (vehicles.Count - 1);
			}

			InteractableVehicle vehicle = vehicles[respawnVehicleIndex];

			respawnVehicleIndex++;
			if (respawnVehicleIndex >= vehicles.Count)
			{
				respawnVehicleIndex = 0;
			}

			if (vehicle == null || vehicle.asset == null)
				return false;

			if (vehicle.asset.engine == EEngine.TRAIN)
			{
				// Trains cannot be destroyed.
				return false;
			}

			if (vehicle.isEmpty == false)
			{
				// Vehicles cannot be destroyed while they have passengers.
				return false;
			}

			float delayBeforeDestroy = Provider.modeConfigData.Vehicles.Respawn_Time;

			bool shouldDestroy = false;
			shouldDestroy |= vehicle.isExploded && Time.realtimeSinceStartup - vehicle.lastExploded > delayBeforeDestroy;
			shouldDestroy |= vehicle.isDrowned && Time.realtimeSinceStartup - vehicle.lastUnderwater > delayBeforeDestroy;

			if (shouldDestroy)
			{
				askVehicleDestroy(vehicle);
				return true;
			}
			else
			{
				return false;
			}
		}

		private void despawnAndRespawnVehicles()
		{
			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return;
			}

			if (vehicles == null)
			{
				return;
			}

			if (vehicles.Count > 0)
			{
				bool destroyedAnyVehicle = respawnVehicles_Destroy();
				if (destroyedAnyVehicle)
				{
					// Exit before respawning a replacement this tick.
					return;
				}
			}

			if (LevelVehicles.spawns == null || LevelVehicles.spawns.Count == 0)
			{
				return;
			}

			if (ShouldRespawnANaturalVehicle())
			{
				VehicleSpawnpoint spawn = findRandomSpawn();
				if (spawn != null)
				{
					addVehicleAtSpawnAndReplicate(spawn);
				}
			}
		}

		private int SumNaturalVehicleCount()
		{
			int naturalVehicleCount = 0;
			foreach (InteractableVehicle vehicle in _vehicles)
			{
				naturalVehicleCount += (vehicle._wasNaturallySpawned ? 1 : 0);
			}
			return naturalVehicleCount;
		}

		/// <summary>
		/// Called when deciding whether to respawn a new vehicle, after gameplay has begun.
		/// </summary>
		private bool ShouldRespawnANaturalVehicle()
		{
			if (_vehicles == null || Level.info == null || Provider.modeConfigData == null)
			{
				return false;
			}

			if (vehicles.Count < maxInstances)
			{
				// maxInstances is more-so the "target." If we have room to add more vehicles, let's do it!
				return true;
			}

			if (SumNaturalVehicleCount() < Provider.modeConfigData.Vehicles.Min_Natural_Vehicles)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Called during level load to determine how many vehicles to create.
		/// </summary>
		private int GetNumberOfNaturalVehiclesToSpawn()
		{
			if (_vehicles == null || Level.info == null || Provider.modeConfigData == null)
			{
				return 0;
			}

			// e.g. with maxInstances of 64 and 100 vehicles this is 0, or with 50 vehicles it's 16 
			int regularSlots = Mathf.Max(0, (int) maxInstances - vehicles.Count);

			int targetNaturalVehicles = (int) Provider.modeConfigData.Vehicles.Min_Natural_Vehicles;
			int naturalSlots = Mathf.Max(0, targetNaturalVehicles - SumNaturalVehicleCount());

			return Mathf.Max(regularSlots, naturalSlots);
		}

		private void RespawnReloadedVehicles()
		{
			List<InteractableVehicle> vehiclesToDestroy = null;

			foreach (InteractableVehicle vehicle in _vehicles)
			{
				if (vehicle.asset.engine == EEngine.TRAIN)
				{
					// Trains cannot be destroyed.
					continue;
				}

				if (vehicle.asset.hasBeenReplaced)
				{
					if (vehiclesToDestroy == null)
					{
						vehiclesToDestroy = new List<InteractableVehicle>();
					}
					vehiclesToDestroy.Add(vehicle);
				}
			}

			if (vehiclesToDestroy == null)
			{
				return;
			}

			foreach (InteractableVehicle vehicle in vehiclesToDestroy)
			{
				VehicleAsset newAsset = Assets.find<VehicleAsset>(vehicle.asset.GUID);
				if (newAsset == null)
				{
					UnturnedLog.error("Missing replacement asset for reloaded vehicle");
					continue;
				}

				bool wasNaturallySpawned = vehicle.WasNaturallySpawned;

				askVehicleDestroy(vehicle);

				InteractableVehicle spawnedVehicle = SpawnVehicleV3(newAsset, vehicle.skinID, vehicle.mythicID,
					vehicle.roadPosition, vehicle.transform.position, vehicle.transform.rotation, vehicle.sirensOn,
					vehicle.isBlimpFloating, vehicle.headlightsOn, vehicle.taillightsOn, vehicle.fuel, vehicle.health,
					vehicle.batteryCharge, vehicle.lockedOwner, vehicle.lockedGroup, vehicle.isLocked, null,
					vehicle.tireAliveMask, vehicle.PaintColor);

				if (spawnedVehicle != null)
				{
					spawnedVehicle.WasNaturallySpawned = wasNaturallySpawned;
				}
				else
				{
					UnturnedLog.error($"Failed to spawn replacement for reloaded vehicle: {newAsset}");
				}
			}
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				seq = 0;

				_vehicles = new List<InteractableVehicle>();
				shouldRespawnReloadedVehicles = false;
				highestInstanceID = 0;
				respawnVehicleIndex = 0;

				BarricadeManager.clearPlants();

				if (Provider.isServer)
				{
					enableDecayUpdate = Provider.modeConfigData.Vehicles.Decay_Time > 0.0f;
					if (!enableDecayUpdate)
					{
						UnturnedLog.info($"Disabling vehicle decay because {nameof(Provider.modeConfigData.Vehicles.Decay_Time)} is negative");
					}

					if (Level.info != null && Level.info.type != ELevelType.ARENA)
					{
						load();

						if (LevelVehicles.spawns.Count > 0)
						{
							List<VehicleSpawnpoint> valid = new List<VehicleSpawnpoint>();
							for (int index = 0; index < LevelVehicles.spawns.Count; index++)
							{
								valid.Add(LevelVehicles.spawns[index]);
							}

							int remainingVehiclesToSpawn = GetNumberOfNaturalVehiclesToSpawn();
							UnturnedLog.info($"Loaded {vehicles.Count} vehicles, will spawn {remainingVehiclesToSpawn} naturally");
							while (remainingVehiclesToSpawn > 0 && valid.Count > 0)
							{
								--remainingVehiclesToSpawn;
								int index = Random.Range(0, valid.Count);

								VehicleSpawnpoint spawn = valid[index];
								valid.RemoveAt(index);

								if (canUseSpawnpoint(spawn))
								{
									InteractableVehicle vehicle = addVehicleAtSpawn(spawn);
									if (vehicle != null)
									{
										vehicle.NotifyFirstSpawned();
									}
								}
							}
						}

						foreach (LevelTrainAssociation train in Level.info.configData.Trains)
						{
							bool alreadySpawned = false;
							foreach (InteractableVehicle vehicle in vehicles)
							{
								if (vehicle.id == train.VehicleID)
								{
									alreadySpawned = true;
									break;
								}
							}

							if (alreadySpawned) // Check if this train exists in the level. If it does we don't need to spawn one
							{
								continue;
							}

							Road road = LevelRoads.getRoad(train.RoadIndex);
							if (road == null)
							{
								UnturnedLog.error("Failed to find track " + train.RoadIndex + " for train " + train.VehicleID + "!");
								continue;
							}

							// We do not clamp roadPosition here because InteractableVehicle.init will
							// clamp it taking into account the length of the train cars.
							float length = road.trackSampledLength;
							float time = Random.Range(train.Min_Spawn_Placement, train.Max_Spawn_Placement);
							float roadPosition = length * time;

							VehicleAsset asset = Assets.find(EAssetType.VEHICLE, train.VehicleID) as VehicleAsset;
							if (asset != null)
							{
								NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_VEHICLE);
								InteractableVehicle trainVehicle = addVehicle(asset.GUID, 0, 0, roadPosition, Vector3.zero, Quaternion.identity, false, false, false, false, ushort.MaxValue, false, ushort.MaxValue, ushort.MaxValue, CSteamID.Nil, CSteamID.Nil, false, null, null, allocateInstanceID(), getVehicleRandomTireAliveMask(asset), netId, new Color32(0, 0, 0, 0));
								if (trainVehicle != null)
								{
									trainVehicle.WasNaturallySpawned = true;
									trainVehicle.NotifyFirstSpawned();
								}
							}
							else if (Assets.shouldLoadAnyAssets)
							{
								UnturnedLog.error("Failed to find asset for train " + train.VehicleID + "!");
							}
						}
					}
					else
					{
						Level.isLoadingVehicles = false;
					}

					if (vehicles != null)
					{
						for (int index = 0; index < vehicles.Count; index++)
						{
							if (vehicles[index] != null)
							{
								Rigidbody rb = vehicles[index].GetComponent<Rigidbody>();
								if (rb != null)
								{
									rb.constraints = RigidbodyConstraints.FreezeAll;
								}
							}
						}
					}
				}
			}
		}

		private void onPostLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				if (Provider.isServer)
				{
					for (int index = 0; index < vehicles.Count; index++)
					{
						if (vehicles[index] != null)
						{
							Rigidbody rb = vehicles[index].GetComponent<Rigidbody>();
							if (rb != null)
							{
								rb.constraints = RigidbodyConstraints.None;
							}
						}
					}
				}
			}
		}

		private void onServerDisconnected(CSteamID player)
		{
			if (Provider.isServer)
			{
				forceRemovePlayer(player);
			}
		}

		internal HashSet<InteractableVehicle> vehiclesNeedingReplicationUpdate = new HashSet<InteractableVehicle>();

		private List<InteractableVehicle> vehiclesToSend = new List<InteractableVehicle>();
		private static float lastSendOverflowWarning;

		private void sendVehicleStates()
		{
			seq++;

			for (int clientIndex = 0; clientIndex < Provider.clients.Count; clientIndex++)
			{
				SteamPlayer client = Provider.clients[clientIndex];

				if (client == null || client.player == null)
				{
					continue;
				}

				vehiclesToSend.Clear();

				foreach (InteractableVehicle vehicle in vehiclesNeedingReplicationUpdate)
				{
					if (vehicle == null)
					{
						continue;
					}

					if (vehicle.checkDriver(client.playerID.steamID))
					{
						// Do not send redundant updates to driver.
						continue;
					}

					vehiclesToSend.Add(vehicle);
				}

				if (vehiclesToSend.IsEmpty())
				{
					continue;
				}

				Vector3 recipientPosition = client.player.transform.position;
				SendVehicleStates.Invoke(ENetReliability.Unreliable, client.transportConnection,
					SendVehicleStates_Write, recipientPosition);

#if WITH_NSB_LOGGING
				client.sentVehicleUpdate = Time.realtimeSinceStartup;
				sentAnyVehicleUpdate = Time.realtimeSinceStartup;
#endif // WITH_NSB_LOGGING
			}

			foreach (InteractableVehicle vehicle in vehiclesNeedingReplicationUpdate)
			{
				if (vehicle != null)
				{
					vehicle.needsReplicationUpdate = false;
				}
			}
			vehiclesNeedingReplicationUpdate.Clear();

#if WITH_NSB_LOGGING
			float timeSinceAnyUpdate = Time.realtimeSinceStartup - sentAnyVehicleUpdate;
			if(vehicles.Count > 1 && NsbLog.isEnabledOnServer && timeSinceAnyUpdate < 10) // > 1 because single driven vehicle may be skipped
			{
				foreach(SteamPlayer client in Provider.clients)
				{
					if(client == null)
						continue;

					float timeSinceUpdate = Time.realtimeSinceStartup - client.sentVehicleUpdate;
					if(timeSinceUpdate > 10)
					{
						client.sentVehicleUpdate = Time.realtimeSinceStartup; // Prevent warning spam.
						NsbLog.WarningFormat("{0}s since we sent tellVehicleStates to {1}", timeSinceUpdate, client.playerID);
					}
				}
			}
#endif // WITH_NSB_LOGGING
		}

		private void SendVehicleStates_Write(NetPakWriter writer, Vector3 recipientPosition)
		{
			writer.WriteUInt32(seq);
			writer.WriteUInt16((ushort) vehiclesToSend.Count);
			foreach (InteractableVehicle vehicle in vehiclesToSend)
			{
				Vector3 vehiclePosition = vehicle.transform.position;
				float sqrDistanceFromRecipient = (vehiclePosition - recipientPosition).GetHorizontalSqrMagnitude();
				const float HIGH_QUALITY_DISTANCE = 300.0f;
				const float SQR_HIGH_QUALITY_DISTANCE = HIGH_QUALITY_DISTANCE * HIGH_QUALITY_DISTANCE;
				bool isHighQuality = sqrDistanceFromRecipient < SQR_HIGH_QUALITY_DISTANCE;

				// TODO this can be significantly improved as part of removing VehicleStateUpdate.
				Vector3 sendPosition;
				if (vehicle.asset.engine == EEngine.TRAIN)
				{
					sendPosition = InteractableVehicle.PackRoadPosition(vehicle.roadPosition);
				}
				else
				{
					sendPosition = vehiclePosition;
				}

				writer.WriteUInt32(vehicle.instanceID);
				writer.WriteClampedVector3(sendPosition, fracBitCount: POSITION_FRAC_BIT_COUNT);
				writer.WriteQuaternion(vehicle.transform.rotation, bitsPerComponent: ROTATION_BIT_COUNT);
				writer.WriteUnsignedClampedFloat(vehicle.ReplicatedSpeed, SPEED_INT_BIT_COUNT, SPEED_FRAC_BIT_COUNT);
				writer.WriteClampedFloat(vehicle.ReplicatedForwardVelocity, FORWARD_VELOCITY_INT_BIT_COUNT, FORWARD_VELOCITY_FRAC_BIT_COUNT);
				writer.WriteSignedNormalizedFloat(vehicle.ReplicatedSteeringInput, STEERING_BIT_COUNT);
				writer.WriteClampedFloat(vehicle.ReplicatedVelocityInput, FORWARD_VELOCITY_INT_BIT_COUNT, FORWARD_VELOCITY_FRAC_BIT_COUNT);
				writer.WriteBit(isHighQuality);

				if (isHighQuality)
				{
					if (vehicle.asset.replicatedWheelIndices != null)
					{
						foreach (int wheelIndex in vehicle.asset.replicatedWheelIndices)
						{
							Wheel wheel = vehicle.GetWheelAtIndex(wheelIndex);
							if (wheel == null)
							{
								UnturnedLog.error($"\"{vehicle.asset.FriendlyName}\" missing wheel for replicated index: {wheelIndex}");
								writer.WriteUnsignedNormalizedFloat(0.0f, 4); // Write something to avoid messing up offsets.
								continue;
							}

							writer.WriteUnsignedNormalizedFloat(wheel.replicatedSuspensionState, 4);
							writer.WritePhysicsMaterialNetId(wheel.replicatedGroundMaterial);
						}
					}

					if (vehicle.asset.UsesEngineRpmAndGears)
					{
						uint packedGear = (uint) (vehicle.GearNumber + 1);
						writer.WriteBits(packedGear, GEAR_BIT_COUNT);

						float normalizedEngineRpm = Mathf.InverseLerp(vehicle.asset.EngineIdleRpm, vehicle.asset.EngineMaxRpm, vehicle.ReplicatedEngineRpm);
						writer.WriteUnsignedNormalizedFloat(normalizedEngineRpm, ENGINE_RPM_BIT_COUNT);
					}
				}
			}

			if (writer.errors != NetPakWriter.EErrorFlags.None && Time.realtimeSinceStartup - lastSendOverflowWarning > 1.0f)
			{
				lastSendOverflowWarning = Time.realtimeSinceStartup;
				CommandWindow.LogWarningFormat("Error {0} writing vehicle states. The vehicle count ({1}) is probably too high.", writer.errors, _vehicles.Count);
			}
		}

#if WITH_NSB_LOGGING
		private float sentAnyVehicleUpdate;
#endif // WITH_NSB_LOGGING

		private void Update()
		{
			if (vehicles == null)
			{
				return;
			}

			Profiler.BeginSample("UpdateVehicles");
			float deltaTime = Time.deltaTime;
			if (Dedicator.IsDedicatedServer)
			{
				foreach (InteractableVehicle vehicle in vehicles)
				{
					if (vehicle == null || !vehicle.hasUnityCalledStart)
						continue;

					Profiler.BeginSample("Vehicle.OnUpdate");
					try
					{
						vehicle.OnUpdate(deltaTime);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, $"Caught exception updating vehicle {vehicle.asset?.FriendlyName}:");
					}
					Profiler.EndSample();
				}
			}
			else
			{
#if !DEDICATED_SERVER
				// Note: this would need to be adjusted for listen servers.
				int frameNumber = Time.frameCount;
				int vehicleUpdateIndex = 0;
				const int nonVisibleVehicleUpdateSlices = 4;
				int thisFrameVehicleUpdateGroup = frameNumber % nonVisibleVehicleUpdateSlices;
				foreach (InteractableVehicle vehicle in vehicles)
				{
					if (vehicle == null || !vehicle.hasUnityCalledStart)
						continue;

					int vehicleUpgradeGroup = vehicleUpdateIndex % nonVisibleVehicleUpdateSlices;
					++vehicleUpdateIndex;
					float sqrLodDistance = MainCamera.SqrDistanceFromLodPosition(vehicle.transform.position);
					vehicle.isVisibleToLocalPlayer = sqrLodDistance < GraphicsSettings.sqrVehicleCullDistanceWithMargin;
					vehicle.accumulatedDeltaTime += deltaTime;
					if (!vehicle.isVisibleToLocalPlayer && thisFrameVehicleUpdateGroup != vehicleUpgradeGroup)
					{
						// Time-slice updating non-visible vehicles.
						continue;
					}

					Profiler.BeginSample("Vehicle.OnUpdate");
					float vehicleDeltaTime = vehicle.accumulatedDeltaTime;
					vehicle.accumulatedDeltaTime = 0.0f;
					try
					{
						vehicle.OnUpdate(vehicleDeltaTime);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, $"Caught exception updating vehicle {vehicle.asset?.FriendlyName}:");
					}
					Profiler.EndSample();
				}
#endif // !DEDICATED_SERVER
			}
			Profiler.EndSample();

			if (!Provider.isServer || !Level.isLoaded)
			{
				return;
			}

			if (vehicles.Count > 0 && Dedicator.IsDedicatedServer && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
			{
				lastTick += Provider.UPDATE_TIME;
				if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
				{
					lastTick = Time.realtimeSinceStartup;
				}

				sendVehicleStates();
			}

			despawnAndRespawnVehicles();

			if (enableDecayUpdate && _vehicles.Count > 0)
			{
				UpdateDecay();
			}

			if (shouldRespawnReloadedVehicles)
			{
				shouldRespawnReloadedVehicles = false;
				RespawnReloadedVehicles();
			}
		}

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onPrePreLevelLoaded += onLevelLoaded;
			Level.onPostLevelLoaded += onPostLevelLoaded;
			Provider.onServerDisconnected += onServerDisconnected;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Vehicles: {vehicles?.Count}");
		}

		public static void load()
		{
			uint highestLoadedInstanceID = 0;

			if (LevelSavedata.fileExists("/Vehicles.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				River river = LevelSavedata.openRiver("/Vehicles.dat", true);
				byte version = river.readByte();

				if (version > 2)
				{
					ushort count = river.readUInt16();

					for (ushort index = 0; index < count; index++)
					{
						Asset asset;
						if (version < SAVEDATA_VERSION_REPLACED_ID_WITH_GUID)
						{
							ushort id = river.readUInt16();
							asset = Assets.find(EAssetType.VEHICLE, id);
						}
						else
						{
							System.Guid guid = river.readGUID();
							asset = Assets.find(guid);
						}

						VehicleAsset vehicleAsset;
						Color32 paintColor = new Color32(0, 0, 0, 0);
						bool isPaintColorFromRedirector = false;
						if (asset is VehicleRedirectorAsset redirectorAsset)
						{
							vehicleAsset = redirectorAsset.TargetVehicle.Find();
							if (redirectorAsset.LoadPaintColor.HasValue)
							{
								paintColor = redirectorAsset.LoadPaintColor.Value;
								isPaintColorFromRedirector = true;
							}
						}
						else
						{
							vehicleAsset = asset as VehicleAsset;
						}

						uint instanceID;
						if (version < 12)
						{
							instanceID = allocateInstanceID();
						}
						else
						{
							instanceID = river.readUInt32();

							if (instanceID > highestLoadedInstanceID)
							{
								highestLoadedInstanceID = instanceID;
							}
						}

						ushort skinID;
						if (version < 8)
						{
							skinID = 0;
						}
						else
						{
							skinID = river.readUInt16();
						}

						ushort mythicID;
						if (version < 9)
						{
							mythicID = 0;
						}
						else
						{
							mythicID = river.readUInt16();
						}

						float roadPosition;
						if (version < 10)
						{
							roadPosition = 0;
						}
						else
						{
							roadPosition = river.readSingle();
						}

						Vector3 point = river.readSingleVector3();
						Quaternion angle = river.readSingleQuaternion();
						ushort fuel = river.readUInt16();
						ushort health = river.readUInt16();

						ushort batteryCharge = 10000;
						if (version > 5)
						{
							batteryCharge = river.readUInt16();
						}

						System.Guid batteryItemGuid;
						if (version >= SAVEDATA_VERSION_BATTERY_GUID)
						{
							batteryItemGuid = river.readGUID();
						}
						else
						{
							// Will be reset to old battery item when removed.
							batteryItemGuid = System.Guid.Empty;
						}

						byte tireAliveMask = byte.MaxValue;
						if (version > 6)
						{
							tireAliveMask = river.readByte();
						}

						CSteamID owner = CSteamID.Nil;
						CSteamID group = CSteamID.Nil;
						bool locked = false;

						if (version > 4)
						{
							owner = river.readSteamID();
							group = river.readSteamID();
							locked = river.readBoolean();
						}

						byte[][] turrets = null;
						if (version > 3)
						{
							turrets = new byte[river.readByte()][];
							for (byte turret = 0; turret < turrets.Length; turret++)
							{
								turrets[turret] = river.readBytes();
							}
						}

						point.y += 0.02f;

						bool hasTrunkStorage;
						if (version < 11)
						{
							hasTrunkStorage = false;
						}
						else
						{
							hasTrunkStorage = river.readBoolean();
						}

						ItemJar[] trunkItems = null;
						if (hasTrunkStorage)
						{
							trunkItems = new ItemJar[river.readByte()];
							for (byte trunkItemIndex = 0; trunkItemIndex < trunkItems.Length; trunkItemIndex++)
							{
								byte x = river.readByte();
								byte y = river.readByte();
								byte rot = river.readByte();
								ushort trunkItemID = river.readUInt16();
								byte amount = river.readByte();
								byte quality = river.readByte();
								byte[] state = river.readBytes();

								ItemAsset trunkItemAsset = Assets.find(EAssetType.ITEM, trunkItemID) as ItemAsset;
								if (trunkItemAsset != null)
								{
									Item trunkItem = new Item(trunkItemID, amount, quality, state);
									trunkItems[trunkItemIndex] = new ItemJar(x, y, rot, trunkItem);
								}
							}
						}

						float decayTimer;
						if (version >= SAVEDATA_VERSION_ADDED_DECAY)
						{
							decayTimer = river.readSingle();
						}
						else
						{
							decayTimer = 0.0f;
						}

						if (version >= SAVEDATA_VERSION_ADDED_PAINT_COLOR)
						{
							Color32 loadedPaintColor = new Color32(0, 0, 0, 0);
							loadedPaintColor.r = river.readByte();
							loadedPaintColor.g = river.readByte();
							loadedPaintColor.b = river.readByte();
							// Loaded alpha can still be zero if vehicle asset had no paint color before.
							// This allows modders to add paint color to existing vehicles.
							loadedPaintColor.a = river.readByte();

							if (!isPaintColorFromRedirector)
							{
								paintColor = loadedPaintColor;
							}
						}

						bool wasNaturallySpawned;
						if (version >= SAVEDATA_VERSION_ADDED_NATURAL_SPAWNED)
						{
							wasNaturallySpawned = river.readBoolean();
						}
						else
						{
							// Default to true to prevent spawning a bunch of extra vehicles.
							wasNaturallySpawned = true;
						}

						if (vehicleAsset != null)
						{
							// Nelson 2024-07-03: Auto-repair all tires in case vehicle was changed from non-damageable
							// to damageable, or more tires were added. (Otherwise old non-existant tires would have a
							// bit set to zero, as in public issue #4556.)
							if (!vehicleAsset.canTiresBeDamaged)
							{
								tireAliveMask = byte.MaxValue;
							}

							NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_VEHICLE);
							InteractableVehicle character = manager.addVehicle(vehicleAsset.GUID, skinID, mythicID, roadPosition, point, angle, false, false, false, false, fuel, false, health, batteryCharge, owner, group, locked, null, turrets, instanceID, tireAliveMask, netId, paintColor);
							if (character != null)
							{
								character.batteryItemGuid = batteryItemGuid;

								if (hasTrunkStorage && trunkItems != null && trunkItems.Length > 0 && character.trunkItems != null && character.trunkItems.height > 0)
								{
									for (byte trunkItemIndex = 0; trunkItemIndex < trunkItems.Length; trunkItemIndex++)
									{
										ItemJar trunkItem = trunkItems[trunkItemIndex];
										if (trunkItem == null)
										{
											continue;
										}

										character.trunkItems.loadItem(trunkItem.x, trunkItem.y, trunkItem.rot, trunkItem.item);
									}
								}

								character.decayTimer = decayTimer;
								character.WasNaturallySpawned = wasNaturallySpawned;
							}
						}
					}
				}
				else
				{
					ushort count = river.readUInt16();
					for (ushort index = 0; index < count; index++)
					{
						ushort id = river.readUInt16();
						river.readColor();
						Vector3 point = river.readSingleVector3();
						Quaternion angle = river.readSingleQuaternion();
						ushort fuel = river.readUInt16();
						ushort health = ushort.MaxValue;
						ushort batteryCharge = ushort.MaxValue;
						byte tireAliveMask = byte.MaxValue;

						id = (ushort) Random.Range(1, 51);

						if (version > 1)
						{
							health = river.readUInt16();
						}

						point.y += 0.02f;

						Asset asset = Assets.find(EAssetType.VEHICLE, id);

						VehicleAsset vehicleAsset;
						Color32 paintColor = new Color32(0, 0, 0, 0);
						if (asset is VehicleRedirectorAsset redirectorAsset)
						{
							vehicleAsset = redirectorAsset.TargetVehicle.Find();
							if (redirectorAsset.LoadPaintColor.HasValue)
							{
								paintColor = redirectorAsset.LoadPaintColor.Value;
							}
						}
						else
						{
							vehicleAsset = asset as VehicleAsset;
						}

						if (vehicleAsset != null)
						{
							NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_VEHICLE);
							InteractableVehicle spawnedVehicle = manager.addVehicle(vehicleAsset.GUID, 0, 0, 0, point, angle, false, false, false, false, fuel, false, health, batteryCharge, CSteamID.Nil, CSteamID.Nil, false, null, null, allocateInstanceID(), tireAliveMask, netId, paintColor);
							if (spawnedVehicle != null)
							{
								// Default to true to prevent spawning a bunch of extra vehicles.
								spawnedVehicle.WasNaturallySpawned = true;
							}
						}
					}
				}

				river.closeRiver();
			}

			// Bring instance ID allocation up-to-date with what it was when the file was saved.
			if (highestLoadedInstanceID > highestInstanceID)
			{
				highestInstanceID = highestLoadedInstanceID;
			}

			Level.isLoadingVehicles = false;
		}

		public static void save()
		{
			River river = LevelSavedata.openRiver("/Vehicles.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);

			ushort count = 0;
			for (ushort index = 0; index < vehicles.Count; index++)
			{
				InteractableVehicle vehicle = vehicles[index];
				if (vehicle == null || vehicle.transform == null)
					continue;

				if (vehicle.isAutoClearable)
					continue;

				count++;
			}

			float horizontalResetThreshold = Provider.configData.Server.Reset_Vehicles_Outside_Horizontal_Distance;
			bool useHorizontalResetThreshold = horizontalResetThreshold > 0.0f;
			float sqrHorizontalResetThreshold = horizontalResetThreshold * horizontalResetThreshold;

			river.writeUInt16(count);
			for (ushort index = 0; index < vehicles.Count; index++)
			{
				InteractableVehicle vehicle = vehicles[index];
				if (vehicle == null || vehicle.transform == null)
					continue;

				if (vehicle.isAutoClearable)
					continue;

				Vector3 savedPosition = vehicle.transform.position;
				if (!savedPosition.IsFinite())
				{
					// Just in case, we move the vehicle back into the playable space.
					savedPosition = new Vector3(0, Level.HEIGHT - 50, 0);
				}
				else if (useHorizontalResetThreshold && savedPosition.GetHorizontalSqrMagnitude() > sqrHorizontalResetThreshold)
				{
					savedPosition = new Vector3(0, Level.HEIGHT - 50, 0);
				}
				else if (savedPosition.y > Level.HEIGHT)
				{
					savedPosition.y = Level.HEIGHT - 50.0f;
				}

				river.writeGUID(vehicle.asset.GUID);
				river.writeUInt32(vehicle.instanceID);
				river.writeUInt16(vehicle.skinID);
				river.writeUInt16(vehicle.mythicID);
				river.writeSingle(vehicle.roadPosition);
				river.writeSingleVector3(savedPosition);
				river.writeSingleQuaternion(vehicle.transform.rotation);
				river.writeUInt16(vehicle.fuel);
				river.writeUInt16(vehicle.health);
				river.writeUInt16(vehicle.batteryCharge);
				river.writeGUID(vehicle.batteryItemGuid);
				river.writeByte(vehicle.tireAliveMask);

				river.writeSteamID(vehicle.lockedOwner);
				river.writeSteamID(vehicle.lockedGroup);
				river.writeBoolean(vehicle.isLocked);

				if (vehicle.turrets != null)
				{
					byte turretCount = (byte) vehicle.turrets.Length;
					river.writeByte(turretCount);
					for (byte turretIndex = 0; turretIndex < turretCount; turretIndex++)
					{
						Passenger turret = vehicle.turrets[turretIndex];
						if (turret != null && turret.state != null)
						{
							river.writeBytes(turret.state);
						}
						else
						{
							river.writeBytes(new byte[0]);
						}
					}
				}
				else
				{
					river.writeByte(0);
				}

				if (vehicle.trunkItems != null && vehicle.trunkItems.height > 0)
				{
					river.writeBoolean(true); // has trunk storage

					byte trunkItemCount = vehicle.trunkItems.getItemCount();
					river.writeByte(trunkItemCount);
					for (byte trunkItemIndex = 0; trunkItemIndex < trunkItemCount; trunkItemIndex++)
					{
						ItemJar jar = vehicle.trunkItems.getItem(trunkItemIndex);

						const byte zero = 0;
						river.writeByte(jar != null ? jar.x : zero);
						river.writeByte(jar != null ? jar.y : zero);
						river.writeByte(jar != null ? jar.rot : zero);
						river.writeUInt16(jar != null ? jar.item.id : zero);
						river.writeByte(jar != null ? jar.item.amount : zero);
						river.writeByte(jar != null ? jar.item.quality : zero);
						river.writeBytes(jar != null ? jar.item.state : new byte[0]);
					}
				}
				else
				{
					river.writeBoolean(false);
				}

				river.writeSingle(vehicle.decayTimer);
				river.writeByte(vehicle.PaintColor.r);
				river.writeByte(vehicle.PaintColor.g);
				river.writeByte(vehicle.PaintColor.b);
				river.writeByte(vehicle.PaintColor.a);
				river.writeBoolean(vehicle.WasNaturallySpawned);
			}

			river.closeRiver();
		}

		/// <summary>
		/// Called on server each frame to slowly damage abandoned vehicle.
		/// </summary>
		private void UpdateDecay()
		{
			decayUpdateIndex = (decayUpdateIndex + 1) % _vehicles.Count;
			InteractableVehicle vehicle = _vehicles[decayUpdateIndex];

			if (vehicle == null || vehicle.asset == null || !vehicle.asset.CanDecay)
			{
				// Invalid or ineligible for decay.
				return;
			}

			float deltaTime = Time.time - vehicle.decayLastUpdateTime;
			vehicle.decayLastUpdateTime = Time.time;

			if (vehicle.isDriven && (vehicle.transform.position - vehicle.decayLastUpdatePosition).sqrMagnitude > 1.0f)
			{
				vehicle.ResetDecayTimer();
				return;
			}

			vehicle.decayTimer += deltaTime;
			if (vehicle.decayTimer > Provider.modeConfigData.Vehicles.Decay_Time)
			{
				vehicle.decayPendingDamage += Provider.modeConfigData.Vehicles.Decay_Damage_Per_Second * deltaTime;
				int intDamage = Mathf.FloorToInt(vehicle.decayPendingDamage);
				if (intDamage > 0)
				{
					vehicle.decayPendingDamage -= intDamage;
					damage(vehicle, intDamage, 1.0f, true, CSteamID.Nil, EDamageOrigin.VehicleDecay);
				}
			}
		}

		private bool enableDecayUpdate;
		private int decayUpdateIndex;

		/// <summary>
		/// +0 = InteractableVehicle
		/// +1 = root transform
		/// +X = VehicleBarricadeRegion
		/// Asset does not know number of train cars, so we always reserve slack.
		/// </summary>
		internal const int NETIDS_PER_VEHICLE = 21;

		// Slightly higher precision than defaults to reduce mispredictions when player pushes against vehicle.
		internal const int POSITION_FRAC_BIT_COUNT = 8;
		internal const int ROTATION_BIT_COUNT = 11;

		/// <summary>
		/// Speed is unsigned, so 8 bits allows a range of [0, 256).
		/// </summary>
		internal const int SPEED_INT_BIT_COUNT = 8;
		internal const int SPEED_FRAC_BIT_COUNT = 2;
		/// <summary>
		/// Velocity is signed, so 9 bits allows a range of [-256, 256).
		/// </summary>
		internal const int FORWARD_VELOCITY_INT_BIT_COUNT = 9;
		internal const int FORWARD_VELOCITY_FRAC_BIT_COUNT = 2;
		internal const int STEERING_BIT_COUNT = 2;

		// Gear is unsigned, so 3 bits allows a range of [0, 8). We treat 0 as reverse.
		internal const int GEAR_BIT_COUNT = 3;

		// Engine RPM is normalized so some precision is lost.
		internal const int ENGINE_RPM_BIT_COUNT = 7;

		#region Obsolete
		[System.Obsolete]
		public void tellVehicleLock(CSteamID steamID, uint instanceID, CSteamID owner, CSteamID group, bool locked)
		{
			ReceiveVehicleLockState(instanceID, owner, group, locked);
		}

		[System.Obsolete]
		public void tellVehicleSkin(CSteamID steamID, uint instanceID, ushort skinID, ushort mythicID)
		{
			ReceiveVehicleSkin(instanceID, skinID, mythicID);
		}

		[System.Obsolete]
		public void tellVehicleSirens(CSteamID steamID, uint instanceID, bool on)
		{
			ReceiveVehicleSirens(instanceID, on);
		}

		[System.Obsolete]
		public void tellVehicleBlimp(CSteamID steamID, uint instanceID, bool on)
		{
			ReceiveVehicleBlimp(instanceID, on);
		}

		[System.Obsolete]
		public void tellVehicleHeadlights(CSteamID steamID, uint instanceID, bool on)
		{
			ReceiveVehicleHeadlights(instanceID, on);
		}

		[System.Obsolete]
		public void tellVehicleHorn(CSteamID steamID, uint instanceID)
		{
			ReceiveVehicleHorn(instanceID);
		}

		[System.Obsolete]
		public void tellVehicleFuel(CSteamID steamID, uint instanceID, ushort newFuel)
		{
			ReceiveVehicleFuel(instanceID, newFuel);
		}

		[System.Obsolete]
		public void tellVehicleBatteryCharge(CSteamID steamID, uint instanceID, ushort newBatteryCharge)
		{
			ReceiveVehicleBatteryCharge(instanceID, newBatteryCharge);
		}

		[System.Obsolete]
		public void tellVehicleTireAliveMask(CSteamID steamID, uint instanceID, byte newTireAliveMask)
		{
			ReceiveVehicleTireAliveMask(instanceID, newTireAliveMask);
		}

		[System.Obsolete]
		public void tellVehicleExploded(CSteamID steamID, uint instanceID)
		{
			ReceiveVehicleExploded(instanceID);
		}

		[System.Obsolete]
		public void tellVehicleHealth(CSteamID steamID, uint instanceID, ushort newHealth)
		{
			ReceiveVehicleHealth(instanceID, newHealth);
		}

		[System.Obsolete]
		public void tellVehicleRecov(CSteamID steamID, uint instanceID, Vector3 newPosition, int newRecov)
		{
			ReceiveVehicleRecov(instanceID, newPosition, newRecov);
		}

		[System.Obsolete]
		public void tellVehicleStates(CSteamID steamID)
		{ }

		[System.Obsolete]
		public void tellVehicleDestroy(CSteamID steamID, uint instanceID)
		{
			ThreadUtil.assertIsGameThread();
			ReceiveDestroySingleVehicle(instanceID);
		}

		[System.Obsolete]
		public void tellVehicleDestroyAll(CSteamID steamID)
		{
			ThreadUtil.assertIsGameThread();
			ReceiveDestroyAllVehicles();
		}

		[System.Obsolete]
		public void tellVehicle(CSteamID steamID)
		{ }

		[System.Obsolete]
		public void tellVehicles(CSteamID steamID)
		{ }

		[System.Obsolete]
		public void askVehicles(CSteamID steamID)
		{ }

		[System.Obsolete]
		public void tellEnterVehicle(CSteamID steamID, uint instanceID, byte seat, CSteamID player)
		{
			ReceiveEnterVehicle(instanceID, seat, player);
		}

		[System.Obsolete]
		public void tellExitVehicle(CSteamID steamID, uint instanceID, byte seat, Vector3 point, byte angle, bool forceUpdate)
		{
			ReceiveExitVehicle(instanceID, seat, point, angle, forceUpdate);
		}

		[System.Obsolete]
		public void tellSwapVehicle(CSteamID steamID, uint instanceID, byte fromSeat, byte toSeat)
		{
			ReceiveSwapVehicleSeats(instanceID, fromSeat, toSeat);
		}

		[System.Obsolete]
		public void askVehicleLock(CSteamID steamID)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveVehicleLockRequest(context);
		}

		[System.Obsolete]
		public void askVehicleSkin(CSteamID steamID)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveVehicleSkinRequest(context);
		}

		[System.Obsolete]
		public void askVehicleHeadlights(CSteamID steamID, bool wantsHeadlightsOn)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveToggleVehicleHeadlights(context, wantsHeadlightsOn);
		}

		[System.Obsolete]
		public void askVehicleBonus(CSteamID steamID, byte bonusType)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveUseVehicleBonus(context, bonusType);
		}

		[System.Obsolete]
		public void askVehicleStealBattery(CSteamID steamID)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveStealVehicleBattery(context);
		}

		[System.Obsolete]
		public void askVehicleHorn(CSteamID steamID)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveVehicleHornRequest(context);
		}

		[System.Obsolete]
		public void askEnterVehicle(CSteamID steamID, uint instanceID, byte[] hash, byte engine)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveEnterVehicleRequest(context, instanceID, hash, new byte[0], engine);
		}

		[System.Obsolete]
		public void askExitVehicle(CSteamID steamID, Vector3 velocity)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveExitVehicleRequest(context, velocity);
		}

		[System.Obsolete]
		public void askSwapVehicle(CSteamID steamID, byte toSeat)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveSwapVehicleRequest(context, toSeat);
		}

		[System.Obsolete]
		public void sendVehicle(InteractableVehicle vehicle)
		{ }

		[System.Obsolete("spawnVehicleV2 returns the spawned instance")]
		public static void spawnVehicle(ushort id, Vector3 point, Quaternion angle)
		{
			spawnVehicleV2(id, point, angle);
		}

		[System.Obsolete("spawnLockedVehicleForPlayerV2 returns the spawned instance")]
		public static void spawnLockedVehicleForPlayer(ushort id, Vector3 point, Quaternion angle, Player player)
		{
			spawnLockedVehicleForPlayerV2(id, point, angle, player);
		}
		#endregion Obsolete
	}
}
