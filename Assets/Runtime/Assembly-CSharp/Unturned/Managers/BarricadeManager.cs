////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_BARRICADE_LOADING
#endif
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public delegate void DeployBarricadeRequestHandler(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow);

	[System.Obsolete]
	public delegate void SalvageBarricadeRequestHandler(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow);

	public delegate void DamageBarricadeRequestHandler(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);
	public delegate void RepairBarricadeRequestHandler(CSteamID instigatorSteamID, Transform barricadeTransform, ref float pendingTotalHealing, ref bool shouldAllow);
	public delegate void RepairedBarricadeHandler(CSteamID instigatorSteamID, Transform barricadeTransform, float totalHealing);
	public delegate void BarricadeSpawnedHandler(BarricadeRegion region, BarricadeDrop drop);
	public delegate void ModifySignRequestHandler(CSteamID instigator, InteractableSign sign, ref string text, ref bool shouldAllow);
	public delegate void OpenStorageRequestHandler(CSteamID instigator, InteractableStorage storage, ref bool shouldAllow);
	public delegate void TransformBarricadeRequestHandler(CSteamID instigator, byte x, byte y, ushort plant, uint instanceID, ref Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool shouldAllow);

	public interface IBarricadePlacedHandler
	{
		/// <summary>
		/// Called on barricade's Interactable after being placed into the world. (Not after loading or replication.)
		/// </summary>
		public void OnBarricadePlaced(BarricadeRegion region, BarricadeDrop barricade)
		{

		}
	}

	public class BarricadeManager : SteamCaller
	{
		private static Collider[] checkColliders = new Collider[2];

		/// <summary>
		/// Barricade asset's EBuild included in saves to fix state length problems. (public issue #3725)
		/// </summary>
		public const byte SAVEDATA_VERSION_INCLUDE_BUILD_ENUM = 18;
		public const byte SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION = 19;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;
		public static readonly byte BARRICADE_REGIONS = 2;

		public static DeployBarricadeRequestHandler onDeployBarricadeRequested;

		[System.Obsolete("Please use BarricadeDrop.OnSalvageRequested_Global instead")]
		public static SalvageBarricadeRequestHandler onSalvageBarricadeRequested;

		public static DamageBarricadeRequestHandler onDamageBarricadeRequested;
		public static event RepairBarricadeRequestHandler OnRepairRequested;
		public static event RepairedBarricadeHandler OnRepaired;

		[System.Obsolete("Please use InteractableFarm.OnHarvestRequested_Global instead")]
		public static SalvageBarricadeRequestHandler onHarvestPlantRequested;

		public static BarricadeSpawnedHandler onBarricadeSpawned;
		public static ModifySignRequestHandler onModifySignRequested;
		public static OpenStorageRequestHandler onOpenStorageRequested;
		public static TransformBarricadeRequestHandler onTransformRequested;

		private static BarricadeManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static BarricadeManager instance => manager;

		public static byte version = SAVEDATA_VERSION;

		public static BarricadeRegion[,] regions
		{
			get;
			private set;
		}

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static BarricadeRegion[,] BarricadeRegions
		{
			get => regions;
			set => regions = value;
		}

		/// <summary>
		/// Writable list of vehicle regions. Public add/remove methods should not be necessary.
		/// </summary>
		private static List<VehicleBarricadeRegion> internalVehicleRegions;

		public static IReadOnlyList<VehicleBarricadeRegion> vehicleRegions
		{
			get;
			private set;
		}

		private static List<BarricadeRegion> backwardsCompatVehicleRegions;
		[System.Obsolete("Please update to vehicleRegions instead (this property copies the list)")]
		public static List<BarricadeRegion> plants
		{
			get
			{
				// backwardsCompatVehicleRegions is reset to null every time vehicleRegions is modified.
				if (backwardsCompatVehicleRegions == null)
				{
					backwardsCompatVehicleRegions = new List<BarricadeRegion>(vehicleRegions);
				}
				return backwardsCompatVehicleRegions;
			}
		}

		private static List<BarricadeRegion> regionsPendingDestroy;
		private static List<Collider> barricadeColliders;

		private static uint instanceCount;
		private static uint serverActiveDate;

		public static void getBarricadesInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
		{
			if (regions == null)
			{
				return;
			}

			for (int regionIndex = 0; regionIndex < search.Count; regionIndex++)
			{
				RegionCoordinate regionCoordinate = search[regionIndex];

				if (regions[regionCoordinate.x, regionCoordinate.y] == null)
				{
					continue;
				}

				foreach (BarricadeDrop barricade in regions[regionCoordinate.x, regionCoordinate.y].drops)
				{
					Transform barricadeTransform = barricade.model;
					if (barricadeTransform == null)
						continue; // Not good and should not happen, but do not throw exceptions here.

					if ((barricadeTransform.position - center).sqrMagnitude < sqrRadius)
					{
						result.Add(barricadeTransform);
					}
				}
			}
		}

		public static void getBarricadesInRadius(Vector3 center, float sqrRadius, ushort plant, List<Transform> result)
		{
			if (vehicleRegions == null)
			{
				return;
			}

			if (plant >= vehicleRegions.Count)
			{
				return;
			}

			VehicleBarricadeRegion region = vehicleRegions[plant];
			foreach (BarricadeDrop barricade in region.drops)
			{
				Transform barricadeTransform = barricade.model;
				if (barricadeTransform == null)
					continue; // Not good and should not happen, but do not throw exceptions here.

				if ((barricadeTransform.position - center).sqrMagnitude < sqrRadius)
				{
					result.Add(barricadeTransform);
				}
			}
		}

		public static void getBarricadesInRadius(Vector3 center, float sqrRadius, List<Transform> result)
		{
			if (vehicleRegions == null)
			{
				return;
			}

			foreach (VehicleBarricadeRegion region in vehicleRegions)
			{
				if (region == null)
					continue; // Not good and should not happen, but do not throw exceptions here.

				if (region.drops.Count < 1)
					continue; // Skip to save the distance check.

				Transform parent = region.parent;
				if (parent == null)
					continue; // Not good and should not happen, but do not throw exceptions here.

				// Hack for performance because the vanilla usage does not pass radius beyond max power range.
				if ((parent.position - center).sqrMagnitude < 65536.0f) // 256m
				{
					foreach (BarricadeDrop barricade in region.drops)
					{
						Transform barricadeTransform = barricade.model;
						if (barricadeTransform == null)
							continue; // Not good and should not happen, but do not throw exceptions here.

						if ((barricadeTransform.position - center).sqrMagnitude < sqrRadius)
						{
							result.Add(barricadeTransform);
						}
					}
				}
			}
		}

		[System.Obsolete]
		public void tellBarricadeOwnerAndGroup(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ulong newOwner, ulong newGroup)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void changeOwnerAndGroup(Transform transform, ulong newOwner, ulong newGroup)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(transform, out x, out y, out plant, out region))
				return;

			BarricadeDrop barricade = region.FindBarricadeByRootTransform(transform);
			if (barricade == null)
				return;

			BarricadeDrop.SendOwnerAndGroup.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), newOwner, newGroup);

			barricade.serversideData.owner = newOwner;
			barricade.serversideData.group = newGroup;

			sendHealthChanged(x, y, plant, barricade);
		}

		public static void transformBarricade(Transform transform, Vector3 point, Quaternion rotation)
		{
			BarricadeDrop barricade = BarricadeDrop.FindByRootFast(transform);
			if (barricade == null)
				return;

			BarricadeDrop.SendTransformRequest.Invoke(barricade.GetNetId(), ENetReliability.Reliable, point, rotation);
		}

		[System.Obsolete]
		public void tellTransformBarricade(CSteamID steamID, byte x, byte y, ushort plant, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static bool ServerSetBarricadeTransform(Transform transform, Vector3 position, Quaternion rotation)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(transform, out x, out y, out plant, out region))
				return false;

			BarricadeDrop barricade = region.FindBarricadeByRootTransform(transform);
			if (barricade == null)
				return false;

			InternalSetBarricadeTransform(x, y, plant, barricade, position, rotation);
			return true;
		}

		internal static void InternalSetBarricadeTransform(byte x, byte y, ushort plant, BarricadeDrop barricade, Vector3 point, Quaternion rotation)
		{
			BarricadeDrop.SendTransform.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), x, y, plant, point, rotation);
		}

		[System.Obsolete]
		public void askTransformBarricade(CSteamID steamID, byte x, byte y, ushort plant, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void poseMannequin(Transform transform, byte poseComp)
		{
			InteractableMannequin mannequin = transform.GetComponent<InteractableMannequin>();
			if (mannequin != null)
			{
				mannequin.ClientSetPose(poseComp);
			}
		}

		[System.Obsolete]
		public void tellPoseMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte poseComp)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static bool ServerSetMannequinPose(InteractableMannequin mannequin, byte poseComp)
		{
			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;

			if (!tryGetRegion(mannequin.transform, out x, out y, out plant, out region))
				return false;

			InternalSetMannequinPose(mannequin, x, y, plant, poseComp);
			return true;
		}

		internal static void InternalSetMannequinPose(InteractableMannequin mannequin, byte x, byte y, ushort plant, byte poseComp)
		{
			InteractableMannequin.SendPose.InvokeAndLoopback(mannequin.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), poseComp);
			mannequin.rebuildState();
		}

		[System.Obsolete]
		public void askPoseMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte poseComp)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void tellUpdateMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte[] state)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void updateMannequin(Transform transform, EMannequinUpdateMode updateMode)
		{
			InteractableMannequin mannequin = transform.GetComponent<InteractableMannequin>();
			if (mannequin != null)
			{
				mannequin.ClientRequestUpdate(updateMode);
			}
		}

		[System.Obsolete]
		public void askUpdateMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte mode)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void rotDisplay(Transform transform, byte rotComp)
		{
			InteractableStorage storage = transform.GetComponent<InteractableStorage>();
			if (storage != null)
			{
				storage.ClientSetDisplayRotation(rotComp);
			}
		}

		[System.Obsolete]
		public void tellRotDisplay(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte rotComp)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askRotDisplay(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte rotComp)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void tellBarricadeHealth(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte hp)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void salvageBarricade(Transform transform)
		{
			BarricadeDrop barricade = FindBarricadeByRootTransform(transform);
			if (barricade != null)
			{
				BarricadeDrop.SendSalvageRequest.Invoke(barricade.GetNetId(), ENetReliability.Reliable);
			}
		}

		[System.Obsolete]
		public void askSalvageBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void tellTank(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort amount)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete("Moved into InteractableTank.ServerSetAmount")]
		public static void updateTank(Transform transform, ushort amount)
		{
			InteractableTank tank = transform.GetComponent<InteractableTank>();
			if (tank != null)
			{
				tank.ServerSetAmount(amount);
			}
		}

		[System.Obsolete]
		public static void updateSign(Transform transform, string newText)
		{
			InteractableSign sign = transform.GetComponent<InteractableSign>();
			if (sign != null)
			{
				sign.ClientSetText(newText);
			}
		}

		[System.Obsolete]
		public void tellUpdateSign(CSteamID steamID, byte x, byte y, ushort plant, ushort index, string newText)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static bool ServerSetSignText(InteractableSign sign, string newText)
		{
			if (sign == null)
				throw new System.ArgumentNullException(nameof(sign));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(sign.transform, out x, out y, out plant, out region))
				return false;

			string trimmedText = sign.trimText(newText);
			if (!sign.isTextValid(trimmedText))
				return false;

			ServerSetSignTextInternal(sign, region, x, y, plant, trimmedText);
			return true;
		}

		internal static void ServerSetSignTextInternal(InteractableSign sign, BarricadeRegion region, byte x, byte y, ushort plant, string trimmedText)
		{
			// Nelson 2024-04-30: Looks like UTF8.GetBytes throws an exception if input string is null, so I'm going
			// through and ensuring we never pass null to it.
			if (trimmedText == null)
			{
				trimmedText = string.Empty;
			}

			InteractableSign.SendChangeText.InvokeAndLoopback(sign.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), trimmedText);

			BarricadeDrop barricade = region.FindBarricadeByRootFast(sign.transform);
			byte[] oldState = barricade.serversideData.barricade.state;
			byte[] textState = System.Text.Encoding.UTF8.GetBytes(trimmedText);
			byte[] newState = new byte[16 + 1 + textState.Length];

			System.Buffer.BlockCopy(oldState, 0, newState, 0, 16);
			newState[16] = (byte) textState.Length;
			if (textState.Length > 0)
			{
				System.Buffer.BlockCopy(textState, 0, newState, 17, textState.Length);
			}

			barricade.serversideData.barricade.state = newState;
		}

		[System.Obsolete]
		public void askUpdateSign(CSteamID steamID, byte x, byte y, ushort plant, ushort index, string newText)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void updateStereoTrack(Transform transform, System.Guid newTrack)
		{
			InteractableStereo stereo = transform.GetComponent<InteractableStereo>();
			if (stereo != null)
			{
				stereo.ClientSetTrack(newTrack);
			}
		}

		public static bool ServerSetStereoTrack(InteractableStereo stereo, System.Guid track)
		{
			if (stereo == null)
				throw new System.ArgumentNullException(nameof(stereo));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(stereo.transform, out x, out y, out plant, out region))
				return false;

			ServerSetStereoTrackInternal(stereo, x, y, plant, region, track);
			return true;
		}

		internal static void ServerSetStereoTrackInternal(InteractableStereo stereo, byte x, byte y, ushort plant, BarricadeRegion region, System.Guid track)
		{
			InteractableStereo.SendTrack.InvokeAndLoopback(stereo.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), track);

			BarricadeDrop barricade = region.FindBarricadeByRootFast(stereo.transform);
			byte[] state = barricade.serversideData.barricade.state;
			System.GuidBuffer buffer = new System.GuidBuffer(track);
			buffer.Write(state, 0);
		}

		[System.Obsolete]
		public void tellUpdateStereoTrack(CSteamID steamID, byte x, byte y, ushort plant, ushort index, System.Guid newTrack)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askUpdateStereoTrack(CSteamID steamID, byte x, byte y, ushort plant, ushort index, System.Guid newTrack)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void updateStereoVolume(Transform transform, byte newVolume)
		{
			InteractableStereo stereo = transform.GetComponent<InteractableStereo>();
			if (stereo != null)
			{
				stereo.ClientSetVolume(newVolume);
			}
		}

		[System.Obsolete]
		public void tellUpdateStereoVolume(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte newVolume)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askUpdateStereoVolume(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte newVolume)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void transferLibrary(Transform transform, byte transaction, uint delta)
		{
			InteractableLibrary library = transform.GetComponent<InteractableLibrary>();
			if (library != null)
			{
				library.ClientTransfer(transaction, delta);
			}
		}

		[System.Obsolete]
		public void tellTransferLibrary(CSteamID steamID, byte x, byte y, ushort plant, ushort index, uint newAmount)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askTransferLibrary(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte transaction, uint delta)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleSafezone(Transform transform)
		{
			InteractableSafezone safezone = transform.GetComponent<InteractableSafezone>();
			if (safezone != null)
			{
				safezone.ClientToggle();
			}
		}

		public static bool ServerSetSafezonePowered(InteractableSafezone safezone, bool isPowered)
		{
			if (safezone == null)
				throw new System.ArgumentNullException(nameof(safezone));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(safezone.transform, out x, out y, out plant, out region))
				return false;

			ServerSetSafezonePoweredInternal(safezone, x, y, plant, region, isPowered);
			return true;
		}

		internal static void ServerSetSafezonePoweredInternal(InteractableSafezone safezone, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
		{
			InteractableSafezone.SendPowered.InvokeAndLoopback(safezone.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
			BarricadeDrop barricade = region.FindBarricadeByRootFast(safezone.transform);
			barricade.serversideData.barricade.state[0] = (byte) (safezone.isPowered ? 1 : 0);
		}

		[System.Obsolete]
		public void tellToggleSafezone(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askToggleSafezone(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleOxygenator(Transform transform)
		{
			InteractableOxygenator oxygenator = transform.GetComponent<InteractableOxygenator>();
			if (oxygenator != null)
			{
				oxygenator.ClientToggle();
			}
		}

		public static bool ServerSetOxygenatorPowered(InteractableOxygenator oxygenator, bool isPowered)
		{
			if (oxygenator == null)
				throw new System.ArgumentNullException(nameof(oxygenator));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(oxygenator.transform, out x, out y, out plant, out region))
				return false;

			ServerSetOxygenatorPoweredInternal(oxygenator, x, y, plant, region, isPowered);
			return true;
		}

		internal static void ServerSetOxygenatorPoweredInternal(InteractableOxygenator oxygenator, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(oxygenator.transform);
			InteractableOxygenator.SendPowered.InvokeAndLoopback(oxygenator.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
			barricade.serversideData.barricade.state[0] = (byte) (oxygenator.isPowered ? 1 : 0);
		}

		[System.Obsolete]
		public void tellToggleOxygenator(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askToggleOxygenator(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleSpot(Transform transform)
		{
			InteractableSpot spot = transform.GetComponent<InteractableSpot>();
			if (spot != null)
			{
				spot.ClientToggle();
			}
		}

		public static bool ServerSetSpotPowered(InteractableSpot spot, bool isPowered)
		{
			if (spot == null)
				throw new System.ArgumentNullException(nameof(spot));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(spot.transform, out x, out y, out plant, out region))
				return false;

			ServerSetSpotPoweredInternal(spot, x, y, plant, region, isPowered);
			return true;
		}

		internal static void ServerSetSpotPoweredInternal(InteractableSpot spot, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(spot.transform);
			InteractableSpot.SendPowered.InvokeAndLoopback(spot.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
			barricade.serversideData.barricade.state[0] = (byte) (spot.isPowered ? 1 : 0);
		}

		[System.Obsolete]
		public void tellToggleSpot(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askToggleSpot(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void sendFuel(Transform transform, ushort fuel)
		{
			InteractableGenerator generator = transform.GetComponent<InteractableGenerator>();
			if (generator != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					InteractableGenerator.SendFuel.InvokeAndLoopback(generator.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), fuel);
				}
			}
		}

		[System.Obsolete]
		public void tellFuel(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort fuel)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleGenerator(Transform transform)
		{
			InteractableGenerator generator = transform.GetComponent<InteractableGenerator>();
			if (generator != null)
			{
				generator.ClientToggle();
			}
		}

		public static bool ServerSetGeneratorPowered(InteractableGenerator generator, bool isPowered)
		{
			if (generator == null)
				throw new System.ArgumentNullException(nameof(generator));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(generator.transform, out x, out y, out plant, out region))
				return false;

			ServerSetGeneratorPoweredInternal(generator, x, y, plant, region, isPowered);
			return true;
		}

		internal static void ServerSetGeneratorPoweredInternal(InteractableGenerator generator, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(generator.transform);
			InteractableGenerator.SendPowered.InvokeAndLoopback(generator.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
			barricade.serversideData.barricade.state[0] = (byte) (generator.isPowered ? 1 : 0);
		}

		[System.Obsolete]
		public void tellToggleGenerator(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askToggleGenerator(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleFire(Transform transform)
		{
			InteractableFire fire = transform.GetComponent<InteractableFire>();
			if (fire != null)
			{
				fire.ClientToggle();
			}
		}

		public static bool ServerSetFireLit(InteractableFire fire, bool isLit)
		{
			if (fire == null)
				throw new System.ArgumentNullException(nameof(fire));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(fire.transform, out x, out y, out plant, out region))
				return false;

			ServerSetFireLitInternal(fire, x, y, plant, region, isLit);
			return true;
		}

		internal static void ServerSetFireLitInternal(InteractableFire fire, byte x, byte y, ushort plant, BarricadeRegion region, bool isLit)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(fire.transform);
			InteractableFire.SendLit.InvokeAndLoopback(fire.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isLit);
			barricade.serversideData.barricade.state[0] = (byte) (fire.isLit ? 1 : 0);
		}

		[System.Obsolete]
		public void tellToggleFire(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isLit)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askToggleFire(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleOven(Transform transform)
		{
			InteractableOven oven = transform.GetComponent<InteractableOven>();
			if (oven != null)
			{
				oven.ClientToggle();
			}
		}

		public static bool ServerSetOvenLit(InteractableOven oven, bool isLit)
		{
			if (oven == null)
				throw new System.ArgumentNullException(nameof(oven));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(oven.transform, out x, out y, out plant, out region))
				return false;

			ServerSetOvenLitInternal(oven, x, y, plant, region, isLit);
			return true;
		}

		internal static void ServerSetOvenLitInternal(InteractableOven oven, byte x, byte y, ushort plant, BarricadeRegion region, bool isLit)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(oven.transform);
			InteractableOven.SendLit.InvokeAndLoopback(oven.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isLit);
			barricade.serversideData.barricade.state[0] = (byte) (oven.isLit ? 1 : 0);
		}

		[System.Obsolete]
		public void tellToggleOven(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isLit)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askToggleOven(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void farm(Transform transform)
		{
			InteractableFarm harvestable = transform.GetComponent<InteractableFarm>();
			if (harvestable != null)
			{
				harvestable.ClientHarvest();
			}
		}

		[System.Obsolete]
		public void tellFarm(CSteamID steamID, byte x, byte y, ushort plant, ushort index, uint planted)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askFarm(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void updateFarm(Transform transform, uint planted, bool shouldSend)
		{
			InteractableFarm harvestable = transform.GetComponent<InteractableFarm>();
			if (harvestable != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;

				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					if (shouldSend)
					{
						InteractableFarm.SendPlanted.InvokeAndLoopback(harvestable.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), planted);
					}

					BarricadeDrop barricade = region.FindBarricadeByRootFast(transform);
					System.BitConverter.GetBytes(planted).CopyTo(barricade.serversideData.barricade.state, 0);
				}
			}
		}

		[System.Obsolete]
		public void tellOil(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort fuel)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void sendOil(Transform transform, ushort fuel)
		{
			InteractableOil oil = transform.GetComponent<InteractableOil>();
			if (oil != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					InteractableOil.SendFuel.InvokeAndLoopback(oil.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), fuel);
				}
			}
		}

		[System.Obsolete]
		public void tellRainBarrel(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isFull)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void updateRainBarrel(Transform transform, bool isFull, bool shouldSend)
		{
			InteractableRainBarrel rainBarrel = transform.GetComponent<InteractableRainBarrel>();
			if (rainBarrel != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					if (shouldSend)
					{
						InteractableRainBarrel.SendFull.InvokeAndLoopback(rainBarrel.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isFull);
					}

					BarricadeDrop barricade = region.FindBarricadeByRootFast(transform);
					barricade.serversideData.barricade.state[0] = (byte) (isFull ? 1 : 0);
				}
			}
		}

		public static void sendStorageDisplay(Transform transform, Item item, ushort skin, ushort mythic, string tags, string dynamicProps)
		{
			InteractableStorage storage = transform.GetComponent<InteractableStorage>();
			if (storage != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					ushort id;
					byte quality;
					byte[] state;
					if (item != null)
					{
						id = item.id;
						quality = item.quality;
						state = item.state;
					}
					else
					{
						id = 0;
						quality = 0;
						state = new byte[0];
					}

					InteractableStorage.SendDisplay.Invoke(storage.GetNetId(), ENetReliability.Reliable, GatherClientConnections(x, y, plant), id, quality, state, skin, mythic, tags, dynamicProps);
				}
			}
		}

		[System.Obsolete]
		public void tellStorageDisplay(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void storeStorage(Transform transform, bool quickGrab)
		{
			InteractableStorage storage = transform.GetComponent<InteractableStorage>();
			if (storage != null)
			{
				storage.ClientInteract(quickGrab);
			}
		}

		[System.Obsolete]
		public void askStoreStorage(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool quickGrab)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public static void toggleDoor(Transform transform)
		{
			InteractableDoor door = transform.GetComponent<InteractableDoor>();
			if (door != null)
			{
				door.ClientToggle();
			}
		}

		[System.Obsolete]
		public void tellToggleDoor(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isOpen)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static bool ServerSetDoorOpen(InteractableDoor door, bool isOpen)
		{
			if (door == null)
				throw new System.ArgumentNullException(nameof(door));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(door.transform, out x, out y, out plant, out region))
				return false;

			ServerSetDoorOpenInternal(door, x, y, plant, region, isOpen);
			return true;
		}

		internal static void ServerSetDoorOpenInternal(InteractableDoor door, byte x, byte y, ushort plant, BarricadeRegion region, bool isOpen)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(door.transform);
			InteractableDoor.SendOpen.InvokeAndLoopback(door.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isOpen);
			barricade.serversideData.barricade.state[16] = (byte) (isOpen ? 1 : 0);
		}

		[System.Obsolete]
		public void askToggleDoor(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		private static bool tryGetBedInRegion(BarricadeRegion region, CSteamID owner, ref Vector3 point, ref byte angle)
		{
			foreach (BarricadeDrop barricade in region.drops)
			{
				BarricadeData data = barricade.serversideData;

				if (data.barricade.state.Length > 0)
				{
					InteractableBed bed = barricade.interactable as InteractableBed;

					if (bed != null && bed.owner == owner && (Provider.modeConfigData.Gameplay.Bypass_No_Building_Zones || Level.checkSafeIncludingClipVolumes(bed.transform.position)))
					{
						point = bed.transform.position;

						float yaw = HousingConnections.GetModelYaw(bed.transform);
						angle = MeasurementTool.angleToByte(yaw + 90.0f);

						int count = Physics.OverlapCapsuleNonAlloc(point + new Vector3(0.0f, PlayerStance.RADIUS, 0.0f), point + new Vector3(0.0f, 2.5f - PlayerStance.RADIUS, 0.0f), PlayerStance.RADIUS, checkColliders, RayMasks.BLOCK_STANCE, QueryTriggerInteraction.Ignore);
						for (int i = 0; i < count; i++)
						{
							if (checkColliders[i].gameObject != bed.gameObject)
							{
								return false;
							}
						}

						return true;
					}
				}
			}

			return false;
		}

		public static bool tryGetBed(CSteamID owner, out Vector3 point, out byte angle)
		{
			point = Vector3.zero;
			angle = 0;

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					BarricadeRegion region = regions[x, y];
					if (tryGetBedInRegion(region, owner, ref point, ref angle))
					{
						return true;
					}
				}
			}

			for (ushort plant = 0; plant < vehicleRegions.Count; plant++)
			{
				BarricadeRegion region = vehicleRegions[plant];
				if (tryGetBedInRegion(region, owner, ref point, ref angle))
				{
					return true;
				}
			}

			return false;
		}

		private static bool UnclaimBedsInRegion(CSteamID owner, BarricadeRegion region, byte x, byte y, ushort plant)
		{
			for (ushort index = 0; index < region.drops.Count; index++)
			{
				BarricadeDrop barricade = region.drops[index];
				if (barricade.serversideData.barricade.state.Length > 0)
				{
					InteractableBed bed = barricade.interactable as InteractableBed;

					if (bed != null && bed.owner == owner)
					{
						InteractableBed.SendClaim.InvokeAndLoopback(bed.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), CSteamID.Nil);
						System.BitConverter.GetBytes(bed.owner.m_SteamID).CopyTo(barricade.serversideData.barricade.state, 0);

						return true;
					}
				}
			}

			return false;
		}

		public static void unclaimBeds(CSteamID owner)
		{
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					BarricadeRegion region = regions[x, y];
					if (UnclaimBedsInRegion(owner, region, x, y, ushort.MaxValue))
						return;
				}
			}

			for (ushort plant = 0; plant < vehicleRegions.Count; plant++)
			{
				BarricadeRegion region = vehicleRegions[plant];
				if (UnclaimBedsInRegion(owner, region, byte.MaxValue, byte.MaxValue, plant))
					return;
			}
		}

		[System.Obsolete]
		public static void claimBed(Transform transform)
		{
			InteractableBed bed = transform.GetComponent<InteractableBed>();
			if (bed != null)
			{
				bed.ClientClaim();
			}
		}

		[System.Obsolete]
		public void tellClaimBed(CSteamID steamID, byte x, byte y, ushort plant, ushort index, CSteamID owner)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		[System.Obsolete]
		public void askClaimBed(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static bool ServerUnclaimBed(InteractableBed bed)
		{
			if (bed == null)
				throw new System.ArgumentNullException(nameof(bed));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(bed.transform, out x, out y, out plant, out region))
				return false;

			ServerSetBedOwnerInternal(bed, x, y, plant, region, CSteamID.Nil);
			return true;
		}

		public static bool ServerClaimBedForPlayer(InteractableBed bed, Player player)
		{
			if (bed == null)
				throw new System.ArgumentNullException(nameof(bed));

			if (player == null)
				throw new System.ArgumentNullException(nameof(player));

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(bed.transform, out x, out y, out plant, out region))
				return false;

			unclaimBeds(player.channel.owner.playerID.steamID);
			ServerSetBedOwnerInternal(bed, x, y, plant, region, player.channel.owner.playerID.steamID);
			return true;
		}

		internal static void ServerSetBedOwnerInternal(InteractableBed bed, byte x, byte y, ushort plant, BarricadeRegion region, CSteamID steamID)
		{
			BarricadeDrop barricade = region.FindBarricadeByRootFast(bed.transform);
			InteractableBed.SendClaim.InvokeAndLoopback(bed.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), steamID);
			System.BitConverter.GetBytes(bed.owner.m_SteamID).CopyTo(barricade.serversideData.barricade.state, 0);
		}

		[System.Obsolete]
		public void tellShootSentry(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void sendShootSentry(Transform transform)
		{
			InteractableSentry sentry = transform.GetComponent<InteractableSentry>();
			if (sentry != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					InteractableSentry.SendShoot.InvokeAndLoopback(sentry.GetNetId(), ENetReliability.Unreliable, GatherRemoteClientConnections(x, y, plant));
				}
			}
		}

		[System.Obsolete]
		public void tellAlertSentry(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte yaw, byte pitch)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		public static void sendAlertSentry(Transform transform, float yaw, float pitch)
		{
			InteractableSentry sentry = transform.GetComponent<InteractableSentry>();
			if (sentry != null)
			{
				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (tryGetRegion(transform, out x, out y, out plant, out region))
				{
					InteractableSentry.SendAlert.InvokeAndLoopback(sentry.GetNetId(), ENetReliability.Unreliable, GatherRemoteClientConnections(x, y, plant), MeasurementTool.angleToByte(yaw), MeasurementTool.angleToByte(pitch));
				}
			}
		}

		public static void damage(Transform transform, float damage, float times, bool armor, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(transform, out x, out y, out plant, out region))
				return;

			BarricadeDrop barricade = region.FindBarricadeByRootTransform(transform);
			if (barricade == null)
				return;

			if (!barricade.serversideData.barricade.isDead)
			{
				ItemBarricadeAsset asset = barricade.asset;
				if (asset == null)
					return;

				if (!asset.canBeDamaged)
					return;

				if (armor)
				{
					times *= Provider.modeConfigData.Barricades.getArmorMultiplier(asset.armorTier);
				}

				ushort totalDamage = (ushort) (damage * times);
				bool shouldAllow = true;

				// Allow plugins to modify damage or cancel it
				onDamageBarricadeRequested?.Invoke(instigatorSteamID, transform, ref totalDamage, ref shouldAllow, damageOrigin);

				if (!shouldAllow || totalDamage < 1)
				{
					return;
				}

				barricade.serversideData.barricade.askDamage(totalDamage);

				if (barricade.serversideData.barricade.isDead)
				{
					PlayBarricadeExplosionEffect(barricade);

					asset.SpawnItemDropsOnDestroy(transform.position);

					destroyBarricade(barricade, x, y, plant);
				}
				else
				{
					sendHealthChanged(x, y, plant, barricade);
				}
			}
		}

		private static void PlayBarricadeExplosionEffect(BarricadeDrop barricade)
		{
			EffectAsset barricadeExplosionAsset = barricade?.asset?.FindExplosionEffectAsset();
			if (barricadeExplosionAsset != null)
			{
				if (barricade?.model == null)
					return;

				TriggerEffectParameters barricadeExplosion = new TriggerEffectParameters(barricadeExplosionAsset);

				if (barricade.asset.ExplosionEffectFlags.HasFlag(EPlaceableExplosionEffectFlags.CopyModelPosition))
				{
					barricadeExplosion.position = barricade.model.position;
				}
				else
				{
					barricadeExplosion.position = barricade.model.position + (Vector3.down * barricade.asset.offset);
				}

				if (barricade.asset.ExplosionEffectFlags.HasFlag(EPlaceableExplosionEffectFlags.CopyModelRotation))
				{
					barricadeExplosion.SetRotation(barricade.model.rotation);
				}

				barricadeExplosion.relevantDistance = EffectManager.MEDIUM;
				barricadeExplosion.reliable = true;
				EffectManager.triggerEffect(barricadeExplosion);
			}
		}

		[System.Obsolete("Please replace the methods which take an index")]
		public static void destroyBarricade(BarricadeRegion region, byte x, byte y, ushort plant, ushort index)
		{
			destroyBarricade(region.drops[index], x, y, plant);
		}

		/// <summary>
		/// Remove barricade instance on server and client.
		/// </summary>
		public static void destroyBarricade(BarricadeDrop barricade, byte x, byte y, ushort plant)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			BarricadeRegion region;
			if (tryGetRegion(x, y, plant, out region))
			{
#pragma warning disable
				region.barricades.Remove(barricade.serversideData);
#pragma warning restore

				SendDestroyBarricade.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), barricade.GetNetId());
			}
		}

		/// <summary>
		/// Used by ownership change and damaged event to tell relevant clients the new health.
		/// </summary>
		private static void sendHealthChanged(byte x, byte y, ushort plant, BarricadeDrop barricade)
		{
			if (plant == ushort.MaxValue)
			{
				BarricadeDrop.SendHealth.Invoke(barricade.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) =>
				{
					return client.player != null &&
						Regions.checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, BARRICADE_REGIONS) &&
						OwnershipTool.checkToggle(client.playerID.steamID, barricade.serversideData.owner, client.player.quests.groupID, barricade.serversideData.group);
				}), (byte) (barricade.serversideData.barricade.health / (float) barricade.serversideData.barricade.asset.health * 100));
			}
			else
			{
				BarricadeDrop.SendHealth.Invoke(barricade.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) =>
				{
					return OwnershipTool.checkToggle(client.playerID.steamID, barricade.serversideData.owner, client.player.quests.groupID, barricade.serversideData.group);
				}), (byte) (barricade.serversideData.barricade.health / (float) barricade.serversideData.barricade.asset.health * 100));
			}
		}

		public static void repair(Transform transform, float damage, float times)
		{
			repair(transform, damage, times, instigatorSteamID: default);
		}

		public static void repair(Transform transform, float damage, float times, CSteamID instigatorSteamID = new CSteamID())
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(transform, out x, out y, out plant, out region))
				return;

			BarricadeDrop barricade = region.FindBarricadeByRootTransform(transform);
			if (barricade == null)
				return;

			if (!barricade.serversideData.barricade.isDead && !barricade.serversideData.barricade.isRepaired)
			{
				float pendingTotalHealing = damage * times;
				bool shouldAllow = true;

				// Allow plugins to modify healing or cancel it
				OnRepairRequested?.Invoke(instigatorSteamID, transform, ref pendingTotalHealing, ref shouldAllow);

				ushort roundedTotalHealing = MathfEx.RoundAndClampToUShort(pendingTotalHealing);
				if (!shouldAllow || roundedTotalHealing < 1)
				{
					return;
				}

				barricade.serversideData.barricade.askRepair(roundedTotalHealing);

				sendHealthChanged(x, y, plant, barricade);

				OnRepaired?.Invoke(instigatorSteamID, transform, roundedTotalHealing);
			}
		}

		/// <summary>
		/// Legacy function for UseableBarricade.
		/// </summary>
		public static Transform dropBarricade(Barricade barricade, Transform hit, Vector3 point, float angle_x, float angle_y, float angle_z, ulong owner, ulong group)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			if (barricade.asset == null)
				return null;

			bool shouldAllow = true;
			onDeployBarricadeRequested?.Invoke(barricade, barricade.asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);

			if (!shouldAllow)
				return null;

			Quaternion rotation = getRotation(barricade.asset, angle_x, angle_y, angle_z);

			if (hit != null && hit.transform.CompareTag("Vehicle"))
			{
				return dropPlantedBarricade(hit, barricade, point, rotation, owner, group);
			}
			else
			{
				return dropNonPlantedBarricade(barricade, point, rotation, owner, group);
			}
		}

		/// <summary>
		/// Common code between dropping barricade onto vehicle or into world.
		/// </summary>
		private static Transform dropBarricadeIntoRegionInternal(BarricadeRegion region, Barricade barricade, Vector3 point, Quaternion rotation, ulong owner, ulong group)
		{
			uint instanceID = ++instanceCount;
			BarricadeData data = new BarricadeData(barricade, point, rotation, owner, group, Provider.time, instanceID);
			NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_BARRICADE);

			Transform result = manager.spawnBarricade(region, barricade.asset.GUID, barricade.state, data.point, rotation, 100, data.owner, data.group, netId);
			if (result != null)
			{
				BarricadeDrop drop = region.drops.GetTail();
				drop.serversideData = data;
#pragma warning disable
				region.barricades.Add(data);
#pragma warning restore
			}
			return result;
		}

		/// <summary>
		/// Spawn a new barricade attached to a vehicle and replicate it.
		/// </summary>
		public static Transform dropPlantedBarricade(Transform parent, Barricade barricade, Vector3 point, Quaternion rotation, ulong owner, ulong group)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			VehicleBarricadeRegion region = FindVehicleRegionByTransform(parent);
			if (region == null)
				return null;

			Transform result = dropBarricadeIntoRegionInternal(region, barricade, point, rotation, owner, group);
			if (result != null)
			{
				BarricadeDrop drop = region.drops.GetTail();
				BarricadeData data = drop.serversideData;

				SendSingleBarricade.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), region._netId, barricade.asset.GUID, barricade.state, data.point, data.rotation, data.owner, data.group, drop.GetNetId());

				onBarricadeSpawned?.Invoke(region, drop);

				if (drop.interactable is IBarricadePlacedHandler spawnHandler)
				{
					spawnHandler.OnBarricadePlaced(region, drop);
				}
			}

			return result;
		}

		/// <summary>
		/// Spawn a new barricade and replicate it.
		/// </summary>
		public static Transform dropNonPlantedBarricade(Barricade barricade, Vector3 point, Quaternion rotation, ulong owner, ulong group)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			byte x;
			byte y;
			if (Regions.tryGetCoordinate(point, out x, out y) == false)
				return null;

			BarricadeRegion region;
			if (tryGetRegion(x, y, ushort.MaxValue, out region) == false)
				return null;

			Transform result = dropBarricadeIntoRegionInternal(region, barricade, point, rotation, owner, group);
			if (result != null)
			{
				BarricadeDrop drop = region.drops.GetTail();
				BarricadeData data = drop.serversideData;

				SendSingleBarricade.Invoke(ENetReliability.Reliable, Regions.GatherRemoteClientConnections(x, y, BARRICADE_REGIONS), NetId.INVALID, barricade.asset.GUID, barricade.state, data.point, data.rotation, data.owner, data.group, drop.GetNetId());

				onBarricadeSpawned?.Invoke(region, drop);

				if (drop.interactable is IBarricadePlacedHandler spawnHandler)
				{
					spawnHandler.OnBarricadePlaced(region, drop);
				}
			}

			return result;
		}

		[System.Obsolete]
		public void tellTakeBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
		{
			throw new System.NotSupportedException("Removed during barricade NetId rewrite");
		}

		private static readonly ClientStaticMethod<NetId> SendDestroyBarricade =
			ClientStaticMethod<NetId>.Get(ReceiveDestroyBarricade);
		/// <summary>
		/// Not an instance method because structure might not exist yet, in which case we cancel instantiation.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveDestroyBarricade(in ClientInvocationContext context, NetId netId)
		{
			BarricadeDrop barricade = NetIdRegistry.Get<BarricadeDrop>(netId);
			if (barricade == null)
			{
				PlaceableInstantiationManager.CancelInstantiationByNetId(netId, NETIDS_PER_BARRICADE);
				return;
			}

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!tryGetRegion(barricade.model, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			barricade.CustomDestroy();
			region.drops.Remove(barricade);
		}

		[System.Obsolete]
		public void tellClearRegionBarricades(CSteamID steamID, byte x, byte y)
		{
			ReceiveClearRegionBarricades(x, y);
		}

		private static readonly ClientStaticMethod<byte, byte> SendClearRegionBarricades =
			ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionBarricades);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellClearRegionBarricades))]
		public static void ReceiveClearRegionBarricades(byte x, byte y)
		{
			if (!Provider.isServer)
			{
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			BarricadeRegion region = regions[x, y];
			DestroyAllInRegion(region);
			PlaceableInstantiationManager.CancelInstantiationsInRegion(regions[x, y], NETIDS_PER_BARRICADE);
		}

		public static void askClearRegionBarricades(byte x, byte y)
		{
			if (Provider.isServer)
			{
				if (!Regions.checkSafe(x, y))
				{
					return;
				}

				BarricadeRegion region = regions[x, y];

				if (region.drops.Count > 0)
				{
#pragma warning disable
					region.barricades.Clear();
#pragma warning restore
					SendClearRegionBarricades.InvokeAndLoopback(ENetReliability.Reliable, Regions.GatherRemoteClientConnections(x, y, BARRICADE_REGIONS), x, y);
				}
			}
		}

		public static void askClearAllBarricades()
		{
			if (Provider.isServer)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						askClearRegionBarricades(x, y);
					}
				}
			}
		}

		private static List<Transform> tempTransforms = new List<Transform>();
		/// <summary>
		/// Destroy barricades whose pivots are within sphere.
		/// </summary>
		public static void DestroyBarricadesInSphere(Vector3 center, float radius, bool playEffect, bool spawnItems)
		{
			tempTransforms.Clear();
			PowerTool.GetBarricadeTransformsInSphere(center, radius, tempTransforms);

			foreach (Transform transform in tempTransforms)
			{
				BarricadeDrop barricade = BarricadeDrop.FindByRootFast(transform);
				if (barricade == null)
					continue;

				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (!tryGetRegion(transform, out x, out y, out plant, out region))
				{
					continue;
				}

				if (playEffect)
				{
					PlayBarricadeExplosionEffect(barricade);
				}

				if (spawnItems && barricade.asset != null)
				{
					barricade.asset.SpawnItemDropsOnDestroy(transform.position);
				}

				destroyBarricade(barricade, x, y, plant);
			}
		}

		public static Quaternion getRotation(ItemBarricadeAsset asset, float angle_x, float angle_y, float angle_z)
		{
			Quaternion rotation = Quaternion.Euler(0, angle_y, 0);
			rotation *= Quaternion.Euler(((asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH) ? 0 : -90) + angle_x, 0, 0);
			rotation *= Quaternion.Euler(0, angle_z, 0);

			return rotation;
		}

		private Transform spawnBarricade(BarricadeRegion region, System.Guid assetGuid, byte[] state, Vector3 point, Quaternion rotation, byte hp, ulong owner, ulong group, NetId netId)
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			ItemBarricadeAsset asset = Assets.find(assetGuid) as ItemBarricadeAsset;
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, "Barricade");
			}
			if (asset == null || asset.barricade == null)
			{
				return null;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			asset.instantiationSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			Transform barricade = null;

			try
			{
				if (asset.eligibleForPooling)
				{
					int prefabKey = asset.barricade.GetInstanceID();
					Stack<GameObject> instances = pool.GetOrAddNew(prefabKey);
					while (instances.Count > 0)
					{
						GameObject pooledInstance = instances.Pop();
						if (pooledInstance != null)
						{
							Profiler.BeginSample("AcivateFromPool");
							barricade = pooledInstance.transform;
							barricade.parent = region.parent;
							barricade.localPosition = point;
							barricade.localRotation = rotation;
							barricade.localScale = Vector3.one;
							pooledInstance.SetActive(true);
							Profiler.EndSample();
							break;
						}
					}
				}

				if (barricade == null) // Unable to find pooled instance.
				{
					GameObject barricadeGameObject;
					if (region.parent == null)
					{
						barricadeGameObject = Instantiate(asset.barricade, point, rotation);
						barricade = barricadeGameObject.transform;
					}
					else
					{
						InstantiateParameters instantiateParameters = new InstantiateParameters()
						{
							parent = region.parent,
							worldSpace = false,
						};
						barricadeGameObject = Instantiate(asset.barricade, point, rotation, instantiateParameters);
						barricade = barricadeGameObject.transform;
					}
					barricade.localScale = Vector3.one;

					// 2021-11-25: vanilla code no longer parses ushort from name, but plugin code may depend on this.
					barricade.name = asset.id.ToString();

					if (asset.useWaterHeightTransparentSort && !Dedicator.IsDedicatedServer)
					{
						barricadeGameObject.AddComponent<WaterHeightTransparentSort>();
					}

					if (Provider.isServer && asset.nav != null)
					{
						Transform nav = Instantiate(asset.nav).transform;
						nav.name = "Nav";

						if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
						{
							Transform hinge = barricade.Find("Skeleton").Find("Hinge");
							if (hinge != null)
							{
								nav.parent = hinge;
							}
							else
							{
								nav.parent = barricade;
							}
						}
						else
						{
							nav.parent = barricade;
						}

						nav.localPosition = Vector3.zero;
						nav.localRotation = Quaternion.identity;
					}

					Transform burning = barricade.FindChildRecursive("Burning");
					if (burning != null)
					{
						burning.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.BURNING;
					}

					Transform warm = barricade.FindChildRecursive("Warm");
					if (warm != null)
					{
						warm.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.WARM;
					}
				}
				else
				{
					if (asset.useWaterHeightTransparentSort && !Dedicator.IsDedicatedServer)
					{
						// Updated pooled instance at new position.
						barricade.GetOrAddComponent<WaterHeightTransparentSort>().updateRenderQueue();
					}
				}

				Profiler.BeginSample("AddOrUpdateInteractable");
				if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
				{
					InteractableDoor door = barricade.GetOrAddComponent<InteractableDoor>();
					door.updateState(asset, state);
				}
				else if (asset.build == EBuild.BED)
				{
					barricade.GetOrAddComponent<InteractableBed>().updateState(asset, state);
				}
				else if (asset.build == EBuild.STORAGE || asset.build == EBuild.STORAGE_WALL)
				{
					barricade.GetOrAddComponent<InteractableStorage>().updateState(asset, state);
				}
				else if (asset.build == EBuild.FARM)
				{
					barricade.GetOrAddComponent<InteractableFarm>().updateState(asset, state);
				}
				else if (asset.build == EBuild.TORCH || asset.build == EBuild.CAMPFIRE)
				{
					barricade.GetOrAddComponent<InteractableFire>().updateState(asset, state);
				}
				else if (asset.build == EBuild.OVEN)
				{
					barricade.GetOrAddComponent<InteractableOven>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SPIKE || asset.build == EBuild.WIRE)
				{
					Transform trapTrigger = barricade.Find("Trap");
					if (trapTrigger != null)
					{
						InteractableTrapTrigger triggerComponent = trapTrigger.gameObject.GetOrAddComponent<InteractableTrapTrigger>();
						InteractableTrap trap = barricade.gameObject.GetOrAddComponent<InteractableTrap>();
						triggerComponent.parentTrap = trap;
						trap.updateState(asset, state);
					}
				}
				else if (asset.build == EBuild.CHARGE)
				{
					InteractableCharge charge = barricade.GetOrAddComponent<InteractableCharge>();
					charge.updateState(asset, state);

					charge.owner = owner;
					charge.group = group;
				}
				else if (asset.build == EBuild.GENERATOR)
				{
					barricade.GetOrAddComponent<InteractableGenerator>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SPOT || asset.build == EBuild.CAGE)
				{
					barricade.GetOrAddComponent<InteractableSpot>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SAFEZONE)
				{
					barricade.GetOrAddComponent<InteractableSafezone>().updateState(asset, state);
				}
				else if (asset.build == EBuild.OXYGENATOR)
				{
					barricade.GetOrAddComponent<InteractableOxygenator>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SIGN || asset.build == EBuild.SIGN_WALL || asset.build == EBuild.NOTE)
				{
					barricade.GetOrAddComponent<InteractableSign>().updateState(asset, state);
				}
				else if (asset.build == EBuild.CLAIM)
				{
					InteractableClaim claim = barricade.GetOrAddComponent<InteractableClaim>();
					claim.owner = owner;
					claim.group = group;
					claim.updateState(asset);
				}
				else if (asset.build == EBuild.BEACON)
				{
					barricade.GetOrAddComponent<InteractableBeacon>().updateState(asset);
				}
				else if (asset.build == EBuild.BARREL_RAIN)
				{
					barricade.GetOrAddComponent<InteractableRainBarrel>().updateState(asset, state);
				}
				else if (asset.build == EBuild.OIL)
				{
					barricade.GetOrAddComponent<InteractableOil>().updateState(asset, state);
				}
				else if (asset.build == EBuild.TANK)
				{
					barricade.GetOrAddComponent<InteractableTank>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SENTRY || asset.build == EBuild.SENTRY_FREEFORM)
				{
					InteractableSentry sentry = barricade.GetOrAddComponent<InteractableSentry>();
					InteractablePower power = barricade.GetOrAddComponent<InteractablePower>();

					sentry.power = power;
					sentry.updateState(asset, state);
					power.RefreshIsConnectedToPower();
				}
				else if (asset.build == EBuild.LIBRARY)
				{
					barricade.GetOrAddComponent<InteractableLibrary>().updateState(asset, state);
				}
				else if (asset.build == EBuild.MANNEQUIN)
				{
					barricade.GetOrAddComponent<InteractableMannequin>().updateState(asset, state);
				}
				else if (asset.build == EBuild.STEREO)
				{
					barricade.GetOrAddComponent<InteractableStereo>().updateState(asset, state);
				}
				else if (asset.build == EBuild.CLOCK)
				{
					if (!Dedicator.IsDedicatedServer)
					{
						InteractableClock clock = barricade.GetOrAddComponent<InteractableClock>();
						clock.updateState(asset, state);
					}
				}
				Profiler.EndSample();

				if (!asset.isUnpickupable)
				{
					Profiler.BeginSample("AddOrUpdateInteractable2");
					Interactable2HP health = barricade.GetOrAddComponent<Interactable2HP>();
					health.hp = hp;

					if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
					{
						Transform hinge = barricade.Find("Skeleton").Find("Hinge");
						if (hinge != null)
						{
							Interactable2SalvageBarricade salv = hinge.GetOrAddComponent<Interactable2SalvageBarricade>();
							salv.root = barricade;
							salv.hp = health;
							salv.owner = owner;
							salv.group = group;
							salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
							salv.shouldBypassPickupOwnership = asset.shouldBypassPickupOwnership;
						}

						Transform hingeLeft = barricade.Find("Skeleton").Find("Left_Hinge");
						if (hingeLeft != null)
						{
							Interactable2SalvageBarricade salv = hingeLeft.GetOrAddComponent<Interactable2SalvageBarricade>();
							salv.root = barricade;
							salv.hp = health;
							salv.owner = owner;
							salv.group = group;
							salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
							salv.shouldBypassPickupOwnership = asset.shouldBypassPickupOwnership;
						}

						Transform hingeRight = barricade.Find("Skeleton").Find("Right_Hinge");
						if (hingeRight != null)
						{
							Interactable2SalvageBarricade salv = hingeRight.GetOrAddComponent<Interactable2SalvageBarricade>();
							salv.root = barricade;
							salv.hp = health;
							salv.owner = owner;
							salv.group = group;
							salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
							salv.shouldBypassPickupOwnership = asset.shouldBypassPickupOwnership;
						}
					}
					else
					{
						Interactable2SalvageBarricade salv = barricade.GetOrAddComponent<Interactable2SalvageBarricade>();
						salv.root = barricade;
						salv.hp = health;
						salv.owner = owner;
						salv.group = group;
						salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
						salv.shouldBypassPickupOwnership = asset.shouldBypassPickupOwnership;
					}
					Profiler.EndSample();
				}

				// Non-null parent means it is attached to a vehicle.
				if (region.parent != null)
				{
					barricadeColliders.Clear();
					barricade.GetComponentsInChildren(barricadeColliders);

					foreach (Collider barricadeCollider in barricadeColliders)
					{
						bool isMeshCollider = barricadeCollider is MeshCollider;
						if (isMeshCollider)
						{
							barricadeCollider.enabled = false; // Disable meshcollider to avoid warning about non-kinematic rigidbody (will become kinematic).
						}

						// Need rigidbody for collision to work while on vehicle
						Rigidbody rb = barricadeCollider.GetComponent<Rigidbody>();
						if (rb == null)
						{
							rb = barricadeCollider.gameObject.AddComponent<Rigidbody>();
							rb.useGravity = false;
							rb.isKinematic = true;
						}

						if (isMeshCollider)
						{
							barricadeCollider.enabled = true; // Renable meshcollider (see above).
						}

						if (barricadeCollider.gameObject.layer == LayerMasks.BARRICADE)
						{
							// So the tires do not collide with it, but players still can
							// 2016-03-15: thanks past self for putting that comment! Was worried that you hadn't explained why it was on resource layer
							// Edit 2019-02-24: Vehicle entry obstruction check now depends on this being Resource as well.
							// Nelson 2024-07-16: If adjusting this please remember to update DestroyOrReleaseBarricade.
							barricadeCollider.gameObject.layer = LayerMasks.RESOURCE;
						}
					}

					// Nelson 2024-09-30:
					// Perhaps foolish considering the comment below, but it seems like we might be able to remove this
					// deactivate-reactivate now. I tested with the off roader in singleplayer and multiplayer after
					// attaching regular barricades. The reason to change is the deactivation calls OnDisable on
					// interactable components that implemented OnDisable with only pooling in mind, so no OnEnable.
					// This is a lazy fix but also might save some perf. :P
					// Older note:
					// I forget exactly why we turn it on and off here, but it's something to do with how Unity handles adding new rigidbodies to the car rigidbody.
					// Do not remove it or cars start getting stuck moving straight.
					// barricade.gameObject.SetActive(false);
					// barricade.gameObject.SetActive(true);

					// Train sub-vehicle parent will not return a vehicle, but that should not be a problem for now.
					InteractableVehicle vehicle = region.parent.GetComponent<InteractableVehicle>();
					if (vehicle != null)
					{
						// Nelson 2024-07-16: If adjusting this please remember to update DestroyOrReleaseBarricade.
						vehicle.ignoreCollisionWith(barricadeColliders, /*shouldIgnore*/ true);
					}
				}

				BarricadeDrop drop = new BarricadeDrop(barricade, barricade.GetComponent<Interactable>(), asset);
				drop.AssignNetId(netId);
				barricade.GetOrAddComponent<BarricadeRefComponent>().tempNotSureIfBarricadeShouldBeAComponentYet = drop;
				region.drops.Add(drop);

			}
			catch (System.Exception exception)
			{
				UnturnedLog.warn("Exception while spawning barricade: {0}", asset);
				UnturnedLog.exception(exception);

				// Ensure that if something *was* spawned that bad state does not get replicated to clients.
				// Do not return to pool or clean-up in case that makes matters worse. (obviously not ideal, but the
				// underlying exception should be addressed so this should only be plugin bugs)
				if (barricade != null)
				{
					Destroy(barricade.gameObject);
					barricade = null;
				}
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			asset.instantiationSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			return barricade;
		}

		[System.Obsolete]
		public void tellBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort id, byte[] state, Vector3 point, byte angle_x, byte angle_y, byte angle_z, ulong owner, ulong group, uint instanceID)
		{
			throw new System.NotSupportedException("Barricades no longer function without a unique NetId");
		}

		private static readonly ClientStaticMethod<NetId, System.Guid, byte[], Vector3, Quaternion, ulong, ulong, NetId> SendSingleBarricade =
			ClientStaticMethod<NetId, System.Guid, byte[], Vector3, Quaternion, ulong, ulong, NetId>.Get(ReceiveSingleBarricade);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveSingleBarricade(in ClientInvocationContext context, NetId parentNetId, System.Guid assetId, byte[] state, [NetPakVector3(fracBitCount: POSITION_FRAC_BIT_COUNT)] Vector3 point, [NetPakSpecialQuaternion(yawBitCount: YAW_BIT_COUNT)] Quaternion rotation, ulong owner, ulong group, NetId netId)
		{
			BarricadeRegion region;
			if (parentNetId.IsNull())
			{
				byte x;
				byte y;
				if (!Regions.tryGetCoordinate(point, out x, out y))
				{
					context.LogWarning("invalid coord");
					return;
				}

				if (!tryGetRegion(x, y, ushort.MaxValue, out region))
				{
					context.LogWarning("invalid region");
					return;
				}
			}
			else
			{
				region = NetIdRegistry.Get<BarricadeRegion>(parentNetId);
				if (region == null)
				{
					context.LogWarning("invalid vehicle region");
					return;
				}
			}

			if (!Provider.isServer)
			{
				if (!region.isNetworked)
				{
					return;
				}
			}

			PlaceableInstantiationParameters instantiation = new PlaceableInstantiationParameters();
			instantiation.type = EPlaceableInstantiationType.Barricade;
			instantiation.region = region;
			instantiation.assetId = assetId;
			instantiation.state = state;
			instantiation.position = point;
			instantiation.rotation = rotation;
			instantiation.hp = 100;
			instantiation.owner = owner;
			instantiation.group = group;
			instantiation.netId = netId;
			instantiation.UpdateSortOrder();
			NetInvocationDeferralRegistry.MarkDeferred(netId, NETIDS_PER_BARRICADE);
			PlaceableInstantiationManager.AddInstantiation(ref instantiation);
		}

		[System.Obsolete]
		public void tellBarricades(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendMultipleBarricades = ClientStaticMethod.Get(ReceiveMultipleBarricades);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveMultipleBarricades(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			byte x;
			reader.ReadUInt8(out x);
			byte y;
			reader.ReadUInt8(out y);
			NetId parentNetId;
			reader.ReadNetId(out parentNetId);

			BarricadeRegion region;
			if (parentNetId == NetId.INVALID)
			{
				if (!tryGetRegion(x, y, ushort.MaxValue, out region))
				{
					context.LogWarning($"invalid region {x} {y}");
					return;
				}
			}
			else
			{
				region = NetIdRegistry.Get<BarricadeRegion>(parentNetId);
				if (region == null)
				{
					context.LogWarning("invalid vehicle region");
					return;
				}
			}

			byte packet;
			reader.ReadUInt8(out packet);

			if (parentNetId == NetId.INVALID) // Vehicle regions are marked "isNetworked" by default.
			{
				if (packet == 0)
				{
					if (region.isNetworked)
					{
						return;
					}

					// Immediately finish any deferred cleanup.
					DestroyAllInRegion(region);
				}
				else
				{
					if (!region.isNetworked)
					{
						return;
					}
				}
			}

			region.isNetworked = true;

			ushort count;
			reader.ReadUInt16(out count);

			if (count > 0)
			{
				float sortOrder;
				reader.ReadFloat(out sortOrder);

				for (ushort index = 0; index < count; index++)
				{
					PlaceableInstantiationParameters instantiation = new PlaceableInstantiationParameters();
					instantiation.type = EPlaceableInstantiationType.Barricade;
					instantiation.region = region;
					instantiation.sortOrder = sortOrder;
					instantiation.UpdateSortOrder();

					reader.ReadGuid(out instantiation.assetId);

					byte stateLength;
					reader.ReadUInt8(out stateLength);
					byte[] state = new byte[stateLength];
					reader.ReadBytes(state);
					instantiation.state = state;

					reader.ReadClampedVector3(out instantiation.position, fracBitCount: POSITION_FRAC_BIT_COUNT);
					reader.ReadSpecialYawOrQuaternion(out instantiation.rotation, yawBitCount: YAW_BIT_COUNT);
					reader.ReadUInt8(out instantiation.hp);
					reader.ReadUInt64(out instantiation.owner);
					reader.ReadUInt64(out instantiation.group);
					reader.ReadNetId(out instantiation.netId);

					NetInvocationDeferralRegistry.MarkDeferred(instantiation.netId, NETIDS_PER_BARRICADE);
					PlaceableInstantiationManager.AddInstantiation(ref instantiation);
				}
			}

			Level.isLoadingBarricades = false;
		}

		[System.Obsolete]
		public void askBarricades(CSteamID steamID, byte x, byte y, ushort plant)
		{ }

		internal void SendRegion(SteamPlayer client, BarricadeRegion region, byte x, byte y, NetId parentNetId, float sortOrder)
		{
			if (region.drops.Count > 0)
			{
				byte packet = 0;
				int index = 0;
				int count = 0;
				int size = 0;
				while (index < region.drops.Count)
				{
					size = 0;
					while (count < region.drops.Count)
					{
						// id = 8
						// point = 12
						// angles = 3
						// owner = 8
						// group = 8
						// instanceID = 4
						// health = 1
						// TOTAL = 38 + unknown state length
						size += 44 + region.drops[count].serversideData.barricade.state.Length;

						count++;

						if (size > Block.BUFFER_SIZE / 2)
						{
							break;
						}
					}

					SendMultipleBarricadesWriteParameters sendMultipleBarricadesWriteParameters = new SendMultipleBarricadesWriteParameters()
					{
						region = region,
						x = x,
						y = y,
						parentNetId = parentNetId,
						sortOrder = sortOrder,
						index = index,
						count = count,
						packet = packet,
					};
					index = count;

					SendMultipleBarricades.Invoke(ENetReliability.Reliable, client.transportConnection,
						SendMultipleBarricades_Write, sendMultipleBarricadesWriteParameters);

					packet++;
				}
			}
			else
			{
				SendMultipleBarricades.Invoke(ENetReliability.Reliable, client.transportConnection, SendMultipleBarricades_WriteEmpty, x, y);
			}
		}

		struct SendMultipleBarricadesWriteParameters
		{
			public BarricadeRegion region;
			public int index;
			public int count;
			public float sortOrder;
			public NetId parentNetId;
			public byte packet;
			public byte x;
			public byte y;
		}

		private static void SendMultipleBarricades_Write(NetPakWriter writer, SendMultipleBarricadesWriteParameters p)
		{
			writer.WriteUInt8(p.x);
			writer.WriteUInt8(p.y);
			writer.WriteNetId(p.parentNetId);
			writer.WriteUInt8(p.packet);
			writer.WriteUInt16((ushort) (p.count - p.index));
			writer.WriteFloat(p.sortOrder);

			while (p.index < p.count)
			{
				BarricadeDrop barricade = p.region.drops[p.index];
				BarricadeData data = barricade.serversideData;
				InteractableStorage storage = barricade.interactable as InteractableStorage;

				writer.WriteGuid(barricade.asset.GUID);

				if (storage != null) // this is far from ideal
				{
					byte[] state;

					if (storage.isDisplay)
					{
						// Nelson 2024-04-30: Looks like UTF8.GetBytes throws an exception if input string
						// is null, so I'm going through and ensuring we never pass null to it. I wonder if
						// this is also the potential cause of some barricade networking issues.
						string displayTags = storage.displayTags != null ? storage.displayTags : string.Empty;
						byte[] tagsBytes = System.Text.Encoding.UTF8.GetBytes(displayTags);
						string displayDynamicProps = storage.displayDynamicProps != null ? storage.displayDynamicProps : string.Empty;
						byte[] dynamicPropsBytes = System.Text.Encoding.UTF8.GetBytes(displayDynamicProps);

						state = new byte[16 + 2 + 1 + 1 + (storage.displayItem != null ? storage.displayItem.state.Length : 0) + 4 + 1 + tagsBytes.Length + 1 + dynamicPropsBytes.Length + 1];

						if (storage.displayItem != null)
						{
							System.Array.Copy(System.BitConverter.GetBytes(storage.displayItem.id), 0, state, 16, 2);
							state[18] = storage.displayItem.quality;
							state[19] = (byte) storage.displayItem.state.Length;
							System.Array.Copy(storage.displayItem.state, 0, state, 20, storage.displayItem.state.Length);
							System.Array.Copy(System.BitConverter.GetBytes(storage.displaySkin), 0, state, 20 + storage.displayItem.state.Length, 2);
							System.Array.Copy(System.BitConverter.GetBytes(storage.displayMythic), 0, state, 20 + storage.displayItem.state.Length + 2, 2);

							state[20 + storage.displayItem.state.Length + 4] = (byte) tagsBytes.Length;
							System.Array.Copy(tagsBytes, 0, state, 20 + storage.displayItem.state.Length + 5, tagsBytes.Length);

							state[20 + storage.displayItem.state.Length + 5 + tagsBytes.Length] = (byte) dynamicPropsBytes.Length;
							System.Array.Copy(dynamicPropsBytes, 0, state, 20 + storage.displayItem.state.Length + 5 + tagsBytes.Length + 1, dynamicPropsBytes.Length);

							state[20 + storage.displayItem.state.Length + 5 + tagsBytes.Length + 1 + dynamicPropsBytes.Length] = storage.rot_comp;
						}
					}
					else
					{
						state = new byte[16];
					}
					System.Array.Copy(data.barricade.state, 0, state, 0, 16);

					writer.WriteUInt8((byte) state.Length);
					writer.WriteBytes(state);
				}
				else
				{
					writer.WriteUInt8((byte) data.barricade.state.Length);
					writer.WriteBytes(data.barricade.state);
				}

				writer.WriteClampedVector3(data.point, fracBitCount: POSITION_FRAC_BIT_COUNT);
				writer.WriteSpecialYawOrQuaternion(data.rotation, yawBitCount: YAW_BIT_COUNT);
				writer.WriteUInt8((byte) Mathf.RoundToInt(data.barricade.health / (float) data.barricade.asset.health * 100));
				writer.WriteUInt64(data.owner);
				writer.WriteUInt64(data.group);
				writer.WriteNetId(barricade.GetNetId());

				p.index++;
			}
		}

		private static void SendMultipleBarricades_WriteEmpty(NetPakWriter writer, byte x, byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteNetId(NetId.INVALID);
			writer.WriteUInt8(0);
			writer.WriteUInt16(0);
		}

		/// <summary>
		/// Clean up before loading vehicles.
		/// </summary>
		public static void clearPlants()
		{
			internalVehicleRegions = new List<VehicleBarricadeRegion>();
			vehicleRegions = internalVehicleRegions.AsReadOnly();
			backwardsCompatVehicleRegions = null;
		}

		/// <summary>
		/// Register a new vehicle as a valid parent for barricades.
		/// Each train car is registered after the root of the train.
		/// Note: why they are called "plants", refer to "only god and i" meme. 
		/// </summary>
		[System.Obsolete("Plugins should not be calling this")]
		public static void waterPlant(Transform parent)
		{
			InteractableVehicle vehicle = DamageTool.getVehicle(parent);
			registerVehicleRegion(parent, vehicle, 0, NetId.INVALID);
		}

		internal static void registerVehicleRegion(Transform parent, InteractableVehicle vehicle, int subvehicleIndex, NetId netId)
		{
			VehicleBarricadeRegion region = new VehicleBarricadeRegion(parent, vehicle, subvehicleIndex);
			region.isNetworked = true;
			region._netId = netId;
			NetIdRegistry.Assign(netId, region);

			internalVehicleRegions.Add(region);
			backwardsCompatVehicleRegions = null;
		}

		/// <summary>
		/// Called before destroying a vehicle GameObject because storage needed to be ManualDestroyed.
		/// </summary>
		public static void uprootPlant(Transform parent)
		{
			for (ushort plant = 0; plant < vehicleRegions.Count; plant++)
			{
				VehicleBarricadeRegion region = vehicleRegions[plant];

				if (region.parent == parent)
				{
#pragma warning disable
					region.barricades.Clear();
#pragma warning restore
					DestroyAllInRegion(region);
					PlaceableInstantiationManager.CancelInstantiationsInRegion(region, NETIDS_PER_BARRICADE);

					NetIdRegistry.Release(region._netId);
					internalVehicleRegions.RemoveAt(plant);
					backwardsCompatVehicleRegions = null;
					return;
				}
			}
		}

		public static void trimPlant(Transform parent)
		{
			for (ushort plant = 0; plant < vehicleRegions.Count; plant++)
			{
				BarricadeRegion region = vehicleRegions[plant];

				if (region.parent == parent)
				{
#pragma warning disable
					region.barricades.Clear();
#pragma warning restore
					DestroyAllInRegion(region);
					PlaceableInstantiationManager.CancelInstantiationsInRegion(region, NETIDS_PER_BARRICADE);

					return;
				}
			}
		}

		[System.Obsolete]
		public static void askPlants(CSteamID steamID)
		{ }

		/// <summary>
		/// Send all vehicle-mounted barricades to client.
		/// Called after sending vehicles so all plant indexes will be valid.
		/// </summary>
		internal static void SendVehicleRegions(SteamPlayer client)
		{
			foreach (VehicleBarricadeRegion region in vehicleRegions)
			{
				if (region.drops.Count > 0)
				{
					float sortOrder = (client.player.transform.position - region.parent.position).sqrMagnitude;
					manager.SendRegion(client, region, byte.MaxValue, byte.MaxValue, region._netId, sortOrder);
				}
			}
		}

		public static BarricadeDrop FindBarricadeByRootTransform(Transform transform)
		{
			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (tryGetRegion(transform, out x, out y, out plant, out region))
			{
				return region.FindBarricadeByRootTransform(transform);
			}
			else
			{
				return null;
			}
		}

		[System.Obsolete("Please use FindBarricadeByRootTransform instead")]
		public static bool tryGetInfo(Transform barricade, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region)
		{
			x = 0;
			y = 0;
			plant = 0;
			index = 0;
			region = null;

			if (tryGetRegion(barricade, out x, out y, out plant, out region))
			{
				for (index = 0; index < region.drops.Count; index++)
				{
					if (barricade == region.drops[index].model)
					{
						return true;
					}
				}
			}

			return false;
		}

		[System.Obsolete("Please use FindBarricadeByRootTransform instead")]
		public static bool tryGetInfo(Transform barricade, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop)
		{
			x = 0;
			y = 0;
			plant = 0;
			index = 0;
			region = null;
			drop = null;

			if (tryGetRegion(barricade, out x, out y, out plant, out region))
			{
				for (index = 0; index < region.drops.Count; index++)
				{
					if (barricade == region.drops[index].model)
					{
						drop = region.drops[index];
						return true;
					}
				}
			}

			return false;
		}

		public static bool tryGetPlant(Transform parent, out byte x, out byte y, out ushort plant, out BarricadeRegion region)
		{
			x = 255;
			y = 255;
			plant = ushort.MaxValue;
			region = null;

			if (parent == null)
			{
				return false;
			}

			for (plant = 0; plant < vehicleRegions.Count; plant++)
			{
				region = vehicleRegions[plant];

				if (region.parent == parent)
				{
					return true;
				}
			}

			return false;
		}

		public static bool tryGetRegion(Transform barricade, out byte x, out byte y, out ushort plant, out BarricadeRegion region)
		{
			x = 0;
			y = 0;
			plant = 0;
			region = null;

			if (barricade == null)
			{
				return false;
			}

			if (barricade.parent != null && barricade.parent.CompareTag("Vehicle"))
			{
				for (plant = 0; plant < vehicleRegions.Count; plant++)
				{
					region = vehicleRegions[plant];

					if (region.parent == barricade.parent)
					{
						return true;
					}
				}
			}
			else
			{
				plant = ushort.MaxValue;

				if (Regions.tryGetCoordinate(barricade.position, out x, out y))
				{
					region = regions[x, y];

					return true;
				}
			}

			return false;
		}

		public static InteractableVehicle getVehicleFromPlant(ushort plant)
		{
			if (plant < vehicleRegions.Count)
			{
				return vehicleRegions[plant].vehicle;
			}
			else
			{
				return null;
			}
		}

		public static BarricadeRegion getRegionFromVehicle(InteractableVehicle vehicle)
		{
			return findRegionFromVehicle(vehicle, subvehicleIndex: 0);
		}

		public static VehicleBarricadeRegion findRegionFromVehicle(InteractableVehicle vehicle, int subvehicleIndex = 0)
		{
			if (vehicle == null)
				return null;

			foreach (VehicleBarricadeRegion region in vehicleRegions)
			{
				if (region.vehicle == vehicle && region.subvehicleIndex == subvehicleIndex)
					return region;
			}

			return null;
		}

		public static VehicleBarricadeRegion findVehicleRegionByNetInstanceID(uint instanceID, int subvehicleIndex = 0)
		{
			foreach (VehicleBarricadeRegion region in vehicleRegions)
			{
				if (region.vehicle.instanceID == instanceID && region.subvehicleIndex == subvehicleIndex)
					return region;
			}

			return null;
		}

		public static VehicleBarricadeRegion FindVehicleRegionByTransform(Transform parent)
		{
			foreach (VehicleBarricadeRegion region in internalVehicleRegions)
			{
				if (region.parent == parent)
				{
					return region;
				}
			}

			return null;
		}

		public static bool tryGetRegion(byte x, byte y, ushort plant, out BarricadeRegion region)
		{
			region = null;

			if (plant < ushort.MaxValue)
			{
				if (plant < vehicleRegions.Count)
				{
					region = vehicleRegions[plant];

					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (Regions.checkSafe(x, y))
				{
					region = regions[x, y];

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[System.Obsolete]
		public void tellBarricadeUpdateState(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte[] newState)
		{
			throw new System.NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
		}

		/// <summary>
		/// Original server-only version that does not replicate changes to clients.
		/// </summary>
		public static void updateState(Transform transform, byte[] state, int size)
		{
			updateStateInternal(transform, state, size, shouldReplicate: false);
		}

		/// <summary>
		/// Only used by plugins. Replicates state change to clients.
		/// </summary>
		public static void updateReplicatedState(Transform transform, byte[] state, int size)
		{
			updateStateInternal(transform, state, size, shouldReplicate: true);
		}

		private static void updateStateInternal(Transform transform, byte[] state, int size, bool shouldReplicate = false)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (tryGetRegion(transform, out x, out y, out plant, out region))
			{
				BarricadeDrop barricade = region.FindBarricadeByRootTransform(transform);

				if (barricade.serversideData.barricade.state.Length != size) // don't make a new array if same size, makes performance a bit better for things like the sentry gun
				{
					barricade.serversideData.barricade.state = new byte[size];
				}

				System.Array.Copy(state, barricade.serversideData.barricade.state, size); // we copy it over because storage uses the steampacker to build its state

				if (shouldReplicate)
				{
					BarricadeDrop.SendUpdateState.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), state);
				}
			}
		}

		private static void updateActivity(BarricadeRegion region, CSteamID owner, CSteamID group)
		{
			foreach (BarricadeDrop barricade in region.drops)
			{
				BarricadeData data = barricade.serversideData;
				if (OwnershipTool.checkToggle(owner, data.owner, group, data.group))
				{
					//UnturnedLog.info("Marking {0} active", data.barricade.asset.getTypeNameAndIdDisplayString());
					data.objActiveDate = Provider.time;
				}
			}
		}

		private static void updateActivity(CSteamID owner, CSteamID group)
		{
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					BarricadeRegion region = regions[x, y];
					updateActivity(region, owner, group);
				}
			}

			for (ushort plant = 0; plant < vehicleRegions.Count; plant++)
			{
				BarricadeRegion region = vehicleRegions[plant];
				updateActivity(region, owner, group);
			}
		}

		/// <summary>
		/// Not ideal, but there was a problem because onLevelLoaded was not resetting these after disconnecting.
		/// </summary>
		internal static void ClearNetworkStuff()
		{
			regionsPendingDestroy = new List<BarricadeRegion>();
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				regions = new BarricadeRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						regions[x, y] = new BarricadeRegion(null);
					}
				}
				barricadeColliders = new List<Collider>();
				version = SAVEDATA_VERSION;
				instanceCount = 0;
				pool = new Dictionary<int, Stack<GameObject>>();

				if (Provider.isServer)
				{
					load();
				}
			}
		}

		private void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
		{
			if (step == 0)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						if (Provider.isServer)
						{
							if (player.movement.loadedRegions[x, y].isBarricadesLoaded && !Regions.checkArea(x, y, new_x, new_y, BARRICADE_REGIONS))
							{
								player.movement.loadedRegions[x, y].isBarricadesLoaded = false;
							}
						}
						else if (player.channel.IsLocalPlayer)
						{
							if (regions[x, y].isNetworked && !Regions.checkArea(x, y, new_x, new_y, BARRICADE_REGIONS))
							{
								if (regions[x, y].drops.Count > 0)
								{
									// Defer cleanup.
									regions[x, y].isPendingDestroy = true;
									regionsPendingDestroy.Add(regions[x, y]);
								}
								PlaceableInstantiationManager.CancelInstantiationsInRegion(regions[x, y], NETIDS_PER_BARRICADE);

								regions[x, y].isNetworked = false;
							}
						}
					}
				}
			}

			if (step == 2)
			{
				if (Dedicator.IsDedicatedServer)
				{
					if (Regions.checkSafe(new_x, new_y))
					{
						Vector3 playerPosition = player.transform.position;
						for (int x = new_x - BARRICADE_REGIONS; x <= new_x + BARRICADE_REGIONS; x++)
						{
							for (int y = new_y - BARRICADE_REGIONS; y <= new_y + BARRICADE_REGIONS; y++)
							{
								if (Regions.checkSafe((byte) x, (byte) y) && !player.movement.loadedRegions[x, y].isBarricadesLoaded)
								{
									player.movement.loadedRegions[x, y].isBarricadesLoaded = true;

									float sortOrder = Regions.HorizontalDistanceFromCenterSquared(x, y, playerPosition);
									SendRegion(player.channel.owner, regions[x, y], (byte) x, (byte) y, NetId.INVALID, sortOrder);
								}
							}
						}
					}
				}
			}
		}

		private void onPlayerCreated(Player player)
		{
			player.movement.onRegionUpdated += onRegionUpdated;

			if (Provider.isServer)
			{
				// onPlayerCreated is called by PlayerQuests after loading groupID,
				// so this does work properly with dynamic (in-game) groups.
				SteamPlayerID id = player.channel.owner.playerID;
				updateActivity(id.steamID, player.quests.groupID);
			}
		}

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onPreLevelLoaded += onLevelLoaded;
			Player.onPlayerCreated += onPlayerCreated;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			int regionsWithBarricades = 0;
			int barricadesInRegions = 0;
			foreach (BarricadeRegion region in regions)
			{
				if (region.drops.Count > 0)
				{
					++regionsWithBarricades;
				}
				barricadesInRegions += region.drops.Count;
			}

			results.Add($"Barricade regions: {regionsWithBarricades}");
			results.Add($"Barricades placed on ground: {barricadesInRegions}");

			int vehicleRegionsWithBarricades = 0;
			int barricadesInVehicleRegions = 0;
			foreach (VehicleBarricadeRegion region in internalVehicleRegions)
			{
				if (region.drops.Count > 0)
				{
					++vehicleRegionsWithBarricades;
				}
				barricadesInVehicleRegions += region.drops.Count;
			}

			results.Add($"Barricade vehicle regions: {internalVehicleRegions.Count}");
			results.Add($"Vehicles with barricades: {vehicleRegionsWithBarricades}");
			results.Add($"Barricades placed on vehicles: {barricadesInVehicleRegions}");
		}

		public static void load()
		{
			bool loadDefaults = false;

			if (LevelSavedata.fileExists("/Barricades.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				River river = LevelSavedata.openRiver("/Barricades.dat", true);
				version = river.readByte();

				if (version > 6)
				{
					serverActiveDate = river.readUInt32();
				}
				else
				{
					serverActiveDate = Provider.time;
				}

				if (version < 15)
				{
					instanceCount = 0;
				}
				else
				{
					instanceCount = river.readUInt32();
				}

				if (version > 0)
				{
					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							loadRegion(version, river, null);
						}
					}

					if (version > 1)
					{
						if (version > 13)
						{
							// In versions 13+ we began saving vehicle instance IDs and which instance ID the
							// barricades were attached to so that saves aren't mixed up if vehicle assets are deleted.
							ushort count = river.readUInt16();
							for (ushort index = 0; index < count; ++index)
							{
								uint vehicleInstanceID = river.readUInt32();
								int subvehicleIndex;
								if (version < 16)
								{
									subvehicleIndex = 0;
								}
								else
								{
									subvehicleIndex = river.readByte();
								}

								BarricadeRegion region = findVehicleRegionByNetInstanceID(vehicleInstanceID, subvehicleIndex);
								if (region == null)
								{
									CommandWindow.LogWarning(string.Format("Barricades associated with missing vehicle instance ID '{0}' subindex {1} were lost", vehicleInstanceID, subvehicleIndex));

									// Fallback to region 0 because we still need to continue reading the binary file with the correct size.
									region = regions[0, 0];
								}

								loadRegion(version, river, region);
							}
						}
						else
						{
							ushort count = river.readUInt16();
							count = (ushort) Mathf.Min(count, vehicleRegions.Count);
							for (int index = 0; index < count; index++)
							{
								BarricadeRegion region = vehicleRegions[index];

								loadRegion(version, river, region);
							}
						}
					}
				}

				if (version < 11)
				{
					loadDefaults = true;
				}

				river.closeRiver();
			}
			else
			{
				loadDefaults = true;
			}

			if (loadDefaults && LevelObjects.buildables != null)
			{
				int spawnCount = 0;

				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						List<LevelBuildableObject> defaults = LevelObjects.buildables[x, y];

						if (defaults == null || defaults.Count == 0)
						{
							continue;
						}

						BarricadeRegion region = regions[x, y];
						for (int index = 0; index < defaults.Count; index++)
						{
							LevelBuildableObject buildable = defaults[index];

							if (buildable == null)
							{
								continue;
							}

							ItemBarricadeAsset asset = buildable.asset as ItemBarricadeAsset;

							if (asset == null)
							{
								continue;
							}

							Barricade barricade = new Barricade(asset);
							BarricadeData data = new BarricadeData(barricade, buildable.point, buildable.rotation, 0, 0, uint.MaxValue, ++instanceCount);
							NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_BARRICADE);

							Transform spawnedInstance = manager.spawnBarricade(region, barricade.asset.GUID, barricade.state, data.point, data.rotation, (byte) Mathf.RoundToInt(barricade.health / (float) asset.health * 100), 0, 0, netId);
							if (spawnedInstance != null)
							{
								BarricadeDrop drop = region.drops.GetTail();
								drop.serversideData = data;
#pragma warning disable
								region.barricades.Add(data);
#pragma warning restore
								++spawnCount;
							}
							else
							{
								UnturnedLog.warn($"Failed to spawn default barricade object {asset.name} at {buildable.point}");
							}
						}
					}
				}

				UnturnedLog.info($"Spawned {spawnCount} default barricades from level");
			}

			Level.isLoadingBarricades = false;
		}

		public static void save()
		{
			River river = LevelSavedata.openRiver("/Barricades.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);

			river.writeUInt32(Provider.time);
			river.writeUInt32(instanceCount);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					BarricadeRegion region = regions[x, y];

					saveRegion(river, region);
				}
			}

			ushort count = 0;
			foreach (VehicleBarricadeRegion region in vehicleRegions)
			{
				InteractableVehicle vehicle = region.vehicle;
				if (vehicle != null && !vehicle.isAutoClearable)
				{
					count++;
				}
			}

			river.writeUInt16(count);
			foreach (VehicleBarricadeRegion region in vehicleRegions)
			{
				InteractableVehicle vehicle = region.vehicle;
				if (vehicle != null && !vehicle.isAutoClearable)
				{
					// We save out the instanceID so that we can perform plant index fixup prior to loadRegion.
					river.writeUInt32(vehicle.instanceID);
					river.writeByte((byte) region.subvehicleIndex);

					saveRegion(river, region);
				}
			}

			river.closeRiver();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			HashSet<uint> ids = new HashSet<uint>();
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					BarricadeRegion region = regions[x, y];
					foreach (BarricadeDrop barricade in region.drops)
					{
						if (ids.Contains(barricade.instanceID))
						{
							UnturnedLog.error("Barricade instance ID {0} is not unique!", barricade.instanceID);
						}
						else
						{
							ids.Add(barricade.instanceID);
						}
					}
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		[System.Diagnostics.Conditional("LOG_BARRICADE_LOADING")]
		private static void LogLoading(string message)
		{
			UnturnedLog.info(message);
		}

		private static void loadRegion(byte version, River river, BarricadeRegion regionOverride)
		{
			ushort count = river.readUInt16();

#if LOG_BARRICADE_LOADING
			if (count > 0)
			{
				LogLoading($"Loading {count} barricades in region");
			}
#endif // LOG_BARRICADE_LOADING

			for (ushort index = 0; index < count; index++)
			{
				ItemBarricadeAsset asset;
				if (version < 17)
				{
					ushort id = river.readUInt16();
					asset = Assets.find(EAssetType.ITEM, id) as ItemBarricadeAsset;
					LogLoading($"[{index + 1} of {count}] Asset: {asset?.FriendlyName} ({id})");
				}
				else
				{
					System.Guid guid = river.readGUID();
					asset = Assets.find(guid) as ItemBarricadeAsset;
					LogLoading($"[{index + 1} of {count}] Asset: {asset?.FriendlyName} ({guid:N})");
				}

				uint instanceID;
				if (version < 15)
				{
					instanceID = ++instanceCount;
				}
				else
				{
					instanceID = river.readUInt32();
				}
				LogLoading($"[{index + 1} of {count}] Instance ID: {instanceID}");

				ushort health = river.readUInt16();
				LogLoading($"[{index + 1} of {count}] Health: {health}");

				byte[] state = river.readBytes();
				LogLoading($"[{index + 1} of {count}] State: {state.Length}");

				Vector3 point = river.readSingleVector3();
				LogLoading($"[{index + 1} of {count}] Position: {point}");

				Quaternion rotation;
				if (version < SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION)
				{
					byte angle_x = 0;
					if (version > 2)
					{
						angle_x = river.readByte();
					}
					byte angle_y = river.readByte();
					byte angle_z = 0;
					if (version > 3)
					{
						angle_z = river.readByte();
					}

					LogLoading($"[{index + 1} of {count}] Rotation: ({angle_x}, {angle_y}, {angle_z})");

					if (version < 10 && asset != null)
					{
						rotation = getRotation(asset, angle_x * 2, angle_y * 2, angle_z * 2);
					}
					else
					{
						rotation = Quaternion.Euler(angle_x * 2.0f, angle_y * 2.0f, angle_z * 2.0f);
					}
				}
				else
				{
					rotation = river.readSingleQuaternion();
				}

				ulong owner = 0;
				ulong group = 0;
				if (version > 4)
				{
					owner = river.readUInt64();
					group = river.readUInt64();
				}
				LogLoading($"[{index + 1} of {count}] Owner: {owner}");
				LogLoading($"[{index + 1} of {count}] Group: {group}");

				uint activeDate;
				if (version > 5)
				{
					activeDate = river.readUInt32();

					if (Provider.time - serverActiveDate > Provider.modeConfigData.Barricades.Decay_Time / 2)
					{
						activeDate = Provider.time;
					}
				}
				else
				{
					activeDate = Provider.time;
				}
				LogLoading($"[{index + 1} of {count}] Active time: {activeDate}");

				byte buildEnumByte;
				if (version >= SAVEDATA_VERSION_INCLUDE_BUILD_ENUM)
				{
					buildEnumByte = river.readByte();
				}
				else
				{
					buildEnumByte = byte.MaxValue;
				}
				LogLoading($"[{index + 1} of {count}] Build type: {buildEnumByte}");

				if (asset != null)
				{
					if (version >= SAVEDATA_VERSION_INCLUDE_BUILD_ENUM)
					{
						if (buildEnumByte != (byte) asset.build)
						{
							UnturnedLog.info($"Discarding barricade \"{asset.FriendlyName}\" because asset Build property changed which might cause bigger problems (public issue #3725)");
							continue;
						}
					}

					if (asset.type == EItemType.TANK && state.Length < 2)
					{
						state = asset.getState(EItemOrigin.ADMIN);
					}

					if (asset.build == EBuild.OIL && state.Length < 2)
					{
						state = asset.getState(EItemOrigin.ADMIN);
					}

					BarricadeRegion region = regionOverride;
					if (region == null)
					{
						if (!Regions.tryGetCoordinate(point, out byte x, out byte y))
						{
							UnturnedLog.warn($"Discarding loaded barricade {asset.FriendlyName} because it is outside the maximum level size at {point}");
							continue;
						}

						region = regions[x, y];
					}

					NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_BARRICADE);
					Transform spawnedInstance = manager.spawnBarricade(region, asset.GUID, state, point, rotation, (byte) Mathf.RoundToInt(health / (float) asset.health * 100), owner, group, netId);
					if (spawnedInstance != null)
					{
						BarricadeDrop drop = region.drops.GetTail();
						BarricadeData serversideData = new BarricadeData(new Barricade(asset, health, state), point, rotation, owner, group, activeDate, instanceID);
						drop.serversideData = serversideData;
#pragma warning disable
						region.barricades.Add(serversideData);
#pragma warning restore
					}
				}
			}
		}

		private static void saveRegion(River river, BarricadeRegion region)
		{
			uint time = Provider.time;

			ushort count = 0;
			foreach (BarricadeDrop barricade in region.drops)
			{
				BarricadeData data = barricade.serversideData;
				if (!Dedicator.IsDedicatedServer || Provider.modeConfigData.Barricades.Decay_Time == 0 || time < data.objActiveDate || time - data.objActiveDate < Provider.modeConfigData.Barricades.Decay_Time)
				{
					if (data.barricade.asset.isSaveable)
					{
						count++;
					}
				}
			}

			river.writeUInt16(count);
			foreach (BarricadeDrop barricade in region.drops)
			{
				BarricadeData data = barricade.serversideData;
				if (!Dedicator.IsDedicatedServer || Provider.modeConfigData.Barricades.Decay_Time == 0 || time < data.objActiveDate || time - data.objActiveDate < Provider.modeConfigData.Barricades.Decay_Time)
				{
					if (data.barricade.asset.isSaveable)
					{
						river.writeGUID(barricade.asset.GUID);
						river.writeUInt32(data.instanceID);
						river.writeUInt16(data.barricade.health);
						river.writeBytes(data.barricade.state);
						river.writeSingleVector3(data.point);
						river.writeSingleQuaternion(data.rotation);
						river.writeUInt64(data.owner);
						river.writeUInt64(data.group);
						river.writeUInt32(data.objActiveDate);
						river.writeByte((byte) barricade.asset.build);
					}
				}
			}
		}

		public static PooledTransportConnectionList GatherClientConnections(byte x, byte y, ushort plant)
		{
			if (plant == ushort.MaxValue)
			{
				return Regions.GatherClientConnections(x, y, BARRICADE_REGIONS);
			}
			else
			{
				return Provider.GatherClientConnections();
			}
		}

		[System.Obsolete("Replaced by GatherClients")]
		public static IEnumerable<ITransportConnection> EnumerateClients(byte x, byte y, ushort plant)
		{
			return GatherClientConnections(x, y, plant);
		}

		public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y, ushort plant)
		{
			if (plant == ushort.MaxValue)
			{
				return Regions.GatherRemoteClientConnections(x, y, BARRICADE_REGIONS);
			}
			else
			{
				return Provider.GatherRemoteClientConnections();
			}
		}

		[System.Obsolete("Replaced by GatherRemoteClients")]
		public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y, ushort plant)
		{
			return GatherRemoteClientConnections(x, y, plant);
		}

		private static void DestroyAllInRegion(BarricadeRegion region)
		{
			if (region.drops.Count > 0)
			{
				region.DestroyAll();
			}
			if (region.isPendingDestroy)
			{
				region.isPendingDestroy = false;
				regionsPendingDestroy.RemoveFast(region);
			}
		}

		internal void DestroyOrReleaseBarricade(ItemBarricadeAsset asset, GameObject instance)
		{
			Transform instanceTransform = instance.transform;
			EffectManager.ClearAttachments(instanceTransform);

			if (asset.eligibleForPooling)
			{
				if (instanceTransform.parent != null)
				{
					barricadeColliders.Clear();
					instance.GetComponentsInChildren(barricadeColliders);

					// Reverse layer change from instantiating barricade.
					foreach (Collider barricadeCollider in barricadeColliders)
					{
						if (barricadeCollider.gameObject.layer == LayerMasks.RESOURCE)
						{
							barricadeCollider.gameObject.layer = LayerMasks.BARRICADE;
						}
					}

					// Un-do ignoring collision with parent vehicle.
					InteractableVehicle vehicle = instanceTransform.parent.GetComponent<InteractableVehicle>();
					if (vehicle != null)
					{
						// Nelson 2024-07-16: If adjusting this please remember to update DestroyOrReleaseBarricade.
						vehicle.ignoreCollisionWith(barricadeColliders, /*shouldIgnore*/ false);
					}
				}

				instance.SetActive(false);
				instanceTransform.parent = null;
				int prefabKey = asset.barricade.GetInstanceID();
				Stack<GameObject> instances = pool.GetOrAddNew(prefabKey);
				instances.Push(instance);
			}
			else
			{
				Destroy(instance);
			}
		}

		/// <summary>
		/// Maps prefab unique id to inactive list.
		/// </summary>
		private Dictionary<int, Stack<GameObject>> pool;

#if !DEDICATED_SERVER
		internal static void HandleInstantiation(ref PlaceableInstantiationParameters instantiation)
		{
			BarricadeRegion region = (BarricadeRegion) instantiation.region;
			Transform result = instance.spawnBarricade(region, instantiation.assetId, instantiation.state, instantiation.position, instantiation.rotation, instantiation.hp, instantiation.owner, instantiation.group, instantiation.netId);
			if (result != null)
			{
				NetInvocationDeferralRegistry.Invoke(instantiation.netId, NETIDS_PER_BARRICADE);
			}
			else
			{
				NetInvocationDeferralRegistry.Cancel(instantiation.netId, NETIDS_PER_BARRICADE);
			}
		}

		private System.Diagnostics.Stopwatch destroyTimer = new System.Diagnostics.Stopwatch();
		private const int MIN_DESTROY_PER_FRAME = 10;

		private void Update()
		{
			if (!Provider.isConnected)
				return;

			PlaceableInstantiationManager.ProcessPendingInstantiations();

			if (regionsPendingDestroy != null && regionsPendingDestroy.Count > 0)
			{
				Profiler.BeginSample("PendingDestroy");
				destroyTimer.Restart();
				int destroyCount = 0;
				do
				{
					BarricadeRegion region = regionsPendingDestroy.GetTail();
					if (region.drops.Count > 0)
					{
						region.DestroyTail();
						++destroyCount;
						if (region.drops.Count < 1)
						{
							region.isPendingDestroy = false;
							regionsPendingDestroy.RemoveTail();
						}
					}
					else
					{
						region.isPendingDestroy = false;
						regionsPendingDestroy.RemoveTail();
					}
				}
				while (regionsPendingDestroy.Count > 0 && (destroyTimer.ElapsedMilliseconds < 1 || destroyCount < MIN_DESTROY_PER_FRAME));
				destroyTimer.Stop();
				Profiler.EndSample();
			}
		}
#endif // !DEDICATED_SERVER

		internal const int POSITION_FRAC_BIT_COUNT = 11;
		/// <summary>
		/// Sending yaw only costs 1 bit (flag) plus yaw bits, so compared to the old 24-bit rotation we may as well
		/// make it high-precision. Quaternion mode uses 1+27 bits!
		/// </summary>
		internal const int YAW_BIT_COUNT = 23;

		/// <summary>
		/// +0 = BarricadeDrop
		/// +1 = root transform
		/// +2 = Interactable (if exists)
		/// </summary>
		internal const int NETIDS_PER_BARRICADE = 3;
	}
}
