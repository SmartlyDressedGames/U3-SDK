////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public delegate void DeployStructureRequestHandler(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow);
	[System.Obsolete]
	public delegate void SalvageStructureRequestHandler(CSteamID steamID, byte x, byte y, ushort index, ref bool shouldAllow);
	public delegate void DamageStructureRequestHandler(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);
	public delegate void RepairStructureRequestHandler(CSteamID instigatorSteamID, Transform structureTransform, ref float pendingTotalHealing, ref bool shouldAllow);
	public delegate void RepairedStructureHandler(CSteamID instigatorSteamID, Transform structureTransform, float totalHealing);
	public delegate void StructureSpawnedHandler(StructureRegion region, StructureDrop drop);
	public delegate void TransformStructureRequestHandler(CSteamID instigator, byte x, byte y, uint instanceID, ref Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool shouldAllow);

	public class StructureManager : SteamCaller
	{
		public const byte SAVEDATA_VERSION_INITIAL = 8;
		public const byte SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION = 9;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;
		public static readonly byte STRUCTURE_REGIONS = 2;

		public static readonly float WALL = HousingConnections.HALF_WALL_HEIGHT;
		public static readonly float PILLAR = 3.1f;
		public static readonly float HEIGHT = 2.125f;

		public static DeployStructureRequestHandler onDeployStructureRequested;

		[System.Obsolete("Please use StructureDrop.OnSalvageRequested_Global instead")]
		public static SalvageStructureRequestHandler onSalvageStructureRequested;

		public static DamageStructureRequestHandler onDamageStructureRequested;
		public static RepairStructureRequestHandler OnRepairRequested;
		public static RepairedStructureHandler OnRepaired;
		public static StructureSpawnedHandler onStructureSpawned;
		public static TransformStructureRequestHandler onTransformRequested;

		private static StructureManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static StructureManager instance => manager;

		public static StructureRegion[,] regions
		{
			get;
			private set;
		}

		internal static HousingConnections housingConnections;

		private static List<StructureRegion> regionsPendingDestroy;

		private static uint instanceCount;

		private static uint serverActiveDate;

		public static void getStructuresInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
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

				foreach (StructureDrop structure in regions[regionCoordinate.x, regionCoordinate.y].drops)
				{
					Vector3 offset = structure.model.position - center;
					if (offset.sqrMagnitude < sqrRadius)
					{
						result.Add(structure.model);
					}
				}
			}
		}

		[System.Obsolete]
		public void tellStructureOwnerAndGroup(CSteamID steamID, byte x, byte y, ushort index, ulong newOwner, ulong newGroup)
		{
			throw new System.NotSupportedException("Moved into instance method as part of structure NetId rewrite");
		}

		public static void changeOwnerAndGroup(Transform transform, ulong newOwner, ulong newGroup)
		{
			byte x;
			byte y;
			StructureRegion region;
			if (!tryGetRegion(transform, out x, out y, out region))
				return;

			StructureDrop structure = region.FindStructureByRootTransform(transform);
			if (structure == null)
				return;

			StructureDrop.SendOwnerAndGroup.InvokeAndLoopback(structure.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y), newOwner, newGroup);

			structure.serversideData.owner = newOwner;
			structure.serversideData.group = newGroup;

			sendHealthChanged(x, y, structure);
		}

		public static void transformStructure(Transform transform, Vector3 point, Quaternion rotation)
		{
			StructureDrop structure = StructureDrop.FindByRootFast(transform);
			if (structure == null)
				return;

			StructureDrop.SendTransformRequest.Invoke(structure.GetNetId(), ENetReliability.Reliable, point, rotation);
		}

		[System.Obsolete]
		public void tellTransformStructure(CSteamID steamID, byte x, byte y, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
		{
			throw new System.NotSupportedException("Moved into instance method as part of structure NetId rewrite");
		}

		public static bool ServerSetStructureTransform(Transform transform, Vector3 position, Quaternion rotation)
		{
			byte x;
			byte y;
			StructureRegion region;
			if (!tryGetRegion(transform, out x, out y, out region))
				return false;

			StructureDrop structure = region.FindStructureByRootTransform(transform);
			if (structure == null)
				return false;

			InternalSetStructureTransform(x, y, structure, position, rotation);
			return true;
		}

		internal static void InternalSetStructureTransform(byte x, byte y, StructureDrop drop, Vector3 point, Quaternion rotation)
		{
			StructureDrop.SendTransform.InvokeAndLoopback(drop.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, point, rotation);
		}

		[System.Obsolete]
		public void askTransformStructure(CSteamID steamID, byte x, byte y, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
		{
			throw new System.NotSupportedException("Moved into instance method as part of structure NetId rewrite");
		}

		[System.Obsolete]
		public void tellStructureHealth(CSteamID steamID, byte x, byte y, ushort index, byte hp)
		{
			throw new System.NotSupportedException("Moved into instance method as part of structure NetId rewrite");
		}

		public static void salvageStructure(Transform transform)
		{
			StructureDrop structure = FindStructureByRootTransform(transform);
			if (structure != null)
			{
				StructureDrop.SendSalvageRequest.Invoke(structure.GetNetId(), ENetReliability.Reliable);
			}
		}

		[System.Obsolete]
		public void askSalvageStructure(CSteamID steamID, byte x, byte y, ushort index)
		{
			throw new System.NotSupportedException("Moved into instance method as part of structure NetId rewrite");
		}

		public static void damage(Transform transform, Vector3 direction, float damage, float times, bool armor, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			byte x;
			byte y;
			StructureRegion region;
			if (!tryGetRegion(transform, out x, out y, out region))
				return;

			StructureDrop structure = region.FindStructureByRootTransform(transform);
			if (structure == null)
				return;

			if (structure.serversideData.structure.isDead)
				return;

			ItemStructureAsset asset = structure.asset;
			if (asset == null)
				return;

			if (!asset.canBeDamaged)
				return;

			if (armor)
			{
				times *= Provider.modeConfigData.Structures.getArmorMultiplier(asset.armorTier);
			}

			ushort totalDamage = (ushort) (damage * times);
			bool shouldAllow = true;

			// Allow plugins to modify damage or cancel it
			onDamageStructureRequested?.Invoke(instigatorSteamID, transform, ref totalDamage, ref shouldAllow, damageOrigin);

			if (!shouldAllow || totalDamage < 1)
			{
				return;
			}

			structure.serversideData.structure.askDamage(totalDamage);

			if (structure.serversideData.structure.isDead)
			{
				EffectAsset explosionAsset = asset.FindExplosionEffectAsset();
				if (explosionAsset != null)
				{
					TriggerEffectParameters explosion = new TriggerEffectParameters(explosionAsset);

					if (asset.ExplosionEffectFlags.HasFlag(EPlaceableExplosionEffectFlags.CopyModelPosition))
					{
						explosion.position = transform.position;
					}
					else
					{
						explosion.position = transform.position + (Vector3.down * HEIGHT);
					}

					if (asset.ExplosionEffectFlags.HasFlag(EPlaceableExplosionEffectFlags.CopyModelRotation))
					{
						explosion.SetRotation(transform.rotation);
					}

					explosion.relevantDistance = EffectManager.MEDIUM;
					explosion.reliable = true;
					EffectManager.triggerEffect(explosion);
				}

				asset.SpawnItemDropsOnDestroy(transform.position);

				destroyStructure(structure, x, y, direction * totalDamage, false);
			}
			else
			{
				sendHealthChanged(x, y, structure);
			}
		}

		[System.Obsolete("Please replace the methods which take an index")]
		public static void destroyStructure(StructureRegion region, byte x, byte y, ushort index, Vector3 ragdoll)
		{
			destroyStructure(region.drops[index], x, y, ragdoll, false);
		}

		public static void destroyStructure(StructureDrop structure, byte x, byte y, Vector3 ragdoll)
		{
			destroyStructure(structure, x, y, ragdoll, false);
		}

		/// <summary>
		/// Remove structure instance on server and client.
		/// </summary>
		public static void destroyStructure(StructureDrop structure, byte x, byte y, Vector3 ragdoll, bool wasPickedUp)
		{
			StructureRegion region;
			if (tryGetRegion(x, y, out region))
			{
#pragma warning disable
				region.structures.Remove(structure.serversideData);
#pragma warning restore
				SendDestroyStructure.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), structure.GetNetId(), ragdoll, wasPickedUp);
			}
		}

		/// <summary>
		/// Used by ownership change and damaged event to tell relevant clients the new health.
		/// </summary>
		private static void sendHealthChanged(byte x, byte y, StructureDrop structure)
		{
			StructureDrop.SendHealth.Invoke(structure.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) =>
			{
				return client.player != null &&
					OwnershipTool.checkToggle(client.playerID.steamID, structure.serversideData.owner, client.player.quests.groupID, structure.serversideData.group) &&
					Regions.checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, STRUCTURE_REGIONS);
			}), (byte) Mathf.RoundToInt(structure.serversideData.structure.health / (float) structure.asset.health * 100));
		}

		public static void repair(Transform structure, float damage, float times)
		{
			repair(structure, damage, times, instigatorSteamID: default);
		}

		public static void repair(Transform transform, float damage, float times, CSteamID instigatorSteamID = new CSteamID())
		{
			byte x;
			byte y;
			StructureRegion region;
			if (!tryGetRegion(transform, out x, out y, out region))
				return;

			StructureDrop structure = region.FindStructureByRootTransform(transform);
			if (structure == null)
				return;

			if (!structure.serversideData.structure.isDead && !structure.serversideData.structure.isRepaired)
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

				structure.serversideData.structure.askRepair(roundedTotalHealing);

				sendHealthChanged(x, y, structure);

				OnRepaired?.Invoke(instigatorSteamID, transform, roundedTotalHealing);
			}
		}

		public static StructureDrop FindStructureByRootTransform(Transform transform)
		{
			byte x;
			byte y;
			StructureRegion region;
			if (tryGetRegion(transform, out x, out y, out region))
			{
				return region.FindStructureByRootTransform(transform);
			}
			else
			{
				return null;
			}
		}

		[System.Obsolete("Please use FindStructureByRootTransform instead")]
		public static bool tryGetInfo(Transform structure, out byte x, out byte y, out ushort index, out StructureRegion region)
		{
			x = 0;
			y = 0;
			index = 0;
			region = null;

			if (tryGetRegion(structure, out x, out y, out region))
			{
				for (index = 0; index < region.drops.Count; index++)
				{
					if (structure == region.drops[index].model)
					{
						return true;
					}
				}
			}

			return false;
		}

		[System.Obsolete("Please use FindStructureByRootTransform instead")]
		public static bool tryGetInfo(Transform structure, out byte x, out byte y, out ushort index, out StructureRegion region, out StructureDrop drop)
		{
			x = 0;
			y = 0;
			index = 0;
			region = null;
			drop = null;

			if (tryGetRegion(structure, out x, out y, out region))
			{
				for (index = 0; index < region.drops.Count; index++)
				{
					if (structure == region.drops[index].model)
					{
						drop = region.drops[index];
						return true;
					}
				}
			}

			return false;
		}

		public static bool tryGetRegion(Transform structure, out byte x, out byte y, out StructureRegion region)
		{
			x = 0;
			y = 0;
			region = null;

			if (structure == null)
			{
				return false;
			}

			if (Regions.tryGetCoordinate(structure.position, out x, out y))
			{
				region = regions[x, y];

				return true;
			}

			return false;
		}

		public static bool tryGetRegion(byte x, byte y, out StructureRegion region)
		{
			region = null;

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

		/// <summary>
		/// Legacy function for UseableStructure.
		/// </summary>
		public static bool dropStructure(Structure structure, Vector3 point, float angle_x, float angle_y, float angle_z, ulong owner, ulong group)
		{
			if (structure.asset == null)
				return false;

			bool shouldAllow = true;
			onDeployStructureRequested?.Invoke(structure, structure.asset, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);

			if (!shouldAllow)
				return false;

			Quaternion rotation = Quaternion.Euler(-90, angle_y, 0);
			return dropReplicatedStructure(structure, point, rotation, owner, group);
		}

		/// <summary>
		/// Spawn a new structure and replicate it.
		/// </summary>
		public static bool dropReplicatedStructure(Structure structure, Vector3 point, Quaternion rotation, ulong owner, ulong group)
		{
			byte x;
			byte y;
			if (Regions.tryGetCoordinate(point, out x, out y) == false)
				return false;

			StructureRegion region;
			if (tryGetRegion(x, y, out region) == false)
				return false;

			StructureData serversideData = new StructureData(structure, point, rotation, owner, group, Provider.time, ++instanceCount);
			NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_STRUCTURE);
			Transform result = manager.spawnStructure(region, structure.asset.GUID, serversideData.point, serversideData.rotation, 100, serversideData.owner, serversideData.group, netId);
			if (result != null)
			{
				StructureDrop drop = region.drops.GetTail();
				drop.serversideData = serversideData;

#pragma warning disable
				region.structures.Add(serversideData);
#pragma warning restore

				SendSingleStructure.Invoke(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, structure.asset.GUID, serversideData.point, serversideData.rotation, serversideData.owner, serversideData.group, netId);

				onStructureSpawned?.Invoke(region, drop);
			}

			return true;
		}

		[System.Obsolete]
		public void tellTakeStructure(CSteamID steamID, byte x, byte y, ushort index, Vector3 ragdoll)
		{
			throw new System.NotSupportedException("Removed during structure NetId rewrite");
		}

		private static readonly ClientStaticMethod<NetId, Vector3, bool> SendDestroyStructure =
			ClientStaticMethod<NetId, Vector3, bool>.Get(ReceiveDestroyStructure);
		/// <summary>
		/// Not an instance method because structure might not exist yet, in which case we cancel instantiation.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveDestroyStructure(in ClientInvocationContext context, NetId netId, Vector3 ragdoll, bool wasPickedUp)
		{
			StructureDrop structure = NetIdRegistry.Get<StructureDrop>(netId);
			if (structure == null)
			{
				PlaceableInstantiationManager.CancelInstantiationByNetId(netId, NETIDS_PER_STRUCTURE);
				return;
			}

			byte x;
			byte y;
			StructureRegion region;
			if (!tryGetRegion(structure.model, out x, out y, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			if (Dedicator.IsDedicatedServer || !GraphicsSettings.debris || wasPickedUp)
			{
				instance.DestroyOrReleaseStructure(structure);
				structure.model.position = Vector3.zero;
			}
			else
			{
				ItemStructureAsset asset = structure.asset;

				if (asset != null && asset.construct != EConstruct.FLOOR && asset.construct != EConstruct.ROOF && asset.construct != EConstruct.FLOOR_POLY && asset.construct != EConstruct.ROOF_POLY)
				{
					Vector3 copyPosition = structure.model.position;
					Quaternion copyRotation = structure.model.rotation;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
					CheckStructureRegionCoordIsCorrect(structure, x, y, "ReceiveDestroyStructure");
#endif

					instance.DestroyOrReleaseStructure(structure);
					structure.model.position = Vector3.zero;

					GameObject debrisGameObject = Instantiate(asset.structure, copyPosition, copyRotation);
					Transform debrisTransform = debrisGameObject.transform;

					ragdoll.y += 8;
					ragdoll.x += Random.Range(-16f, 16f);
					ragdoll.z += Random.Range(-16f, 16f);
					ragdoll *= 2;

					EffectManager.RegisterDebris(debrisGameObject);

					MeshCollider mesh = debrisGameObject.GetComponent<MeshCollider>();
					if (mesh != null)
					{
						mesh.convex = true;
					}

					foreach (Transform child in debrisTransform)
					{
						if (child.CompareTag("Logic"))
						{
							Destroy(child.gameObject);
						}
					}

					debrisGameObject.tag = "Debris";
					debrisGameObject.SetLayerRecursively(LayerMasks.DEBRIS);

					Rigidbody rb = debrisGameObject.GetOrAddComponent<Rigidbody>();
					rb.useGravity = true;
					rb.isKinematic = false;
					rb.AddForce(ragdoll);
					rb.drag = 0.5f;
					rb.angularDrag = 0.1f;
					debrisTransform.localScale *= 0.75f;

					Destroy(debrisGameObject, 8f);
				}
				else
				{
					instance.DestroyOrReleaseStructure(structure);
					structure.model.position = Vector3.zero;
				}
			}

			structure.ReleaseNetId();
			region.drops.Remove(structure);
		}

		[System.Obsolete]
		public void tellClearRegionStructures(CSteamID steamID, byte x, byte y)
		{
			ReceiveClearRegionStructures(x, y);
		}

		private static readonly ClientStaticMethod<byte, byte> SendClearRegionStructures =
			ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionStructures);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellClearRegionStructures))]
		public static void ReceiveClearRegionStructures(byte x, byte y)
		{
			if (!Provider.isServer)
			{
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			StructureRegion region = regions[x, y];
			DestroyAllInRegion(region);
			PlaceableInstantiationManager.CancelInstantiationsInRegion(region, NETIDS_PER_STRUCTURE);
		}

		public static void askClearRegionStructures(byte x, byte y)
		{
			if (Provider.isServer)
			{
				if (!Regions.checkSafe(x, y))
				{
					return;
				}

				StructureRegion region = regions[x, y];

				if (region.drops.Count > 0)
				{
#pragma warning disable
					region.structures.Clear();
#pragma warning restore
					SendClearRegionStructures.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y);
				}
			}
		}

		public static void askClearAllStructures()
		{
			if (Provider.isServer)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						askClearRegionStructures(x, y);
					}
				}
			}
		}

		private Transform spawnStructure(StructureRegion region, System.Guid assetId, Vector3 point, Quaternion rotation, byte hp, ulong owner, ulong group, NetId netId)
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			ItemStructureAsset asset = Assets.find(assetId) as ItemStructureAsset;
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetId, asset, "Structure");
			}
			if (asset == null || asset.structure == null)
			{
				return null;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			asset.instantiationSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			Transform structure = null;

			try
			{
				if (asset.eligibleForPooling)
				{
					int prefabKey = asset.structure.GetInstanceID();
					Stack<GameObject> instances = pool.GetOrAddNew(prefabKey);
					while (instances.Count > 0)
					{
						GameObject pooledInstance = instances.Pop();
						if (pooledInstance != null)
						{
							Profiler.BeginSample("AcivateFromPool");
							structure = pooledInstance.transform;
							structure.SetPositionAndRotation(point, rotation);
							pooledInstance.SetActive(true);
							Profiler.EndSample();
							break;
						}
					}
				}

				if (structure == null) // Unable to find pooled instance.
				{
					GameObject structureGameObject = Instantiate(asset.structure, point, rotation);
					structure = structureGameObject.transform;

					// 2021-11-25: vanilla code no longer parses ushort from name, but plugin code may depend on this.
					structureGameObject.name = asset.id.ToString();

					if (Provider.isServer && asset.nav != null)
					{
						Transform nav = Instantiate(asset.nav).transform;
						nav.name = "Nav";
						nav.parent = structure;
						nav.localPosition = Vector3.zero;
						nav.localRotation = Quaternion.identity;
					}
				}

				if (!asset.isUnpickupable)
				{
					Profiler.BeginSample("AddOrUpdateInteractable2");
					Interactable2HP health = structure.GetOrAddComponent<Interactable2HP>();
					health.hp = hp;

					Interactable2SalvageStructure salv = structure.GetOrAddComponent<Interactable2SalvageStructure>();
					salv.hp = health;
					salv.owner = owner;
					salv.group = group;
					salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
					Profiler.EndSample();
				}

				StructureDrop drop = new StructureDrop(structure, asset);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				CheckStructureRegionCoordIsCorrect(drop, region, "spawnStructure");
#endif

				structure.GetOrAddComponent<StructureRefComponent>().tempNotSureIfStructureShouldBeAComponentYet = drop;
				drop.AssignNetId(netId);
				region.drops.Add(drop);

				if (structure != null)
				{
#if !DEDICATED_SERVER
					drop.AddFoliageCut();
#endif // !DEDICATED_SERVER

					try
					{
						housingConnections.LinkConnections(drop);
					}
					catch (System.Exception e)
					{
						// try/catch because I do not want to risk breaking structures in the big update
						UnturnedLog.exception(e, "Caught exception while linking housing connections:");
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.warn("Exception while spawning structure: {0}", asset);
				UnturnedLog.exception(exception);

				// Ensure that if something *was* spawned that bad state does not get replicated to clients.
				// Do not return to pool or clean-up in case that makes matters worse. (obviously not ideal, but the
				// underlying exception should be addressed so this should only be plugin bugs)
				if (structure != null)
				{
					Destroy(structure.gameObject);
					structure = null;
				}
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			asset.instantiationSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			return structure;
		}

		[System.Obsolete]
		public void tellStructure(CSteamID steamID, byte x, byte y, ushort id, Vector3 point, byte angle_x, byte angle_y, byte angle_z, ulong owner, ulong group, uint instanceID)
		{
			throw new System.NotSupportedException("Structures no longer function without a unique NetId");
		}

		private static readonly ClientStaticMethod<byte, byte, System.Guid, Vector3, Quaternion, ulong, ulong, NetId> SendSingleStructure =
			ClientStaticMethod<byte, byte, System.Guid, Vector3, Quaternion, ulong, ulong, NetId>.Get(ReceiveSingleStructure);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellStructure))]
		public static void ReceiveSingleStructure(byte x, byte y, System.Guid id, [NetPakVector3(fracBitCount: POSITION_FRAC_BIT_COUNT)] Vector3 point, [NetPakSpecialQuaternion(yawBitCount: YAW_BIT_COUNT)] Quaternion rotation, ulong owner, ulong group, NetId netId)
		{
			StructureRegion region;

			if (tryGetRegion(x, y, out region))
			{
				if (!Provider.isServer)
				{
					if (!region.isNetworked)
					{
						return;
					}
				}

				PlaceableInstantiationParameters instantiation = new PlaceableInstantiationParameters();
				instantiation.type = EPlaceableInstantiationType.Structure;
				instantiation.region = region;
				instantiation.assetId = id;
				instantiation.position = point;
				instantiation.rotation = rotation;
				instantiation.hp = 100;
				instantiation.owner = owner;
				instantiation.group = group;
				instantiation.netId = netId;
				instantiation.UpdateSortOrder();
				NetInvocationDeferralRegistry.MarkDeferred(instantiation.netId, NETIDS_PER_STRUCTURE);
				PlaceableInstantiationManager.AddInstantiation(ref instantiation);
			}
		}

		[System.Obsolete]
		public void tellStructures(CSteamID steamID)
		{ }

		private static ClientStaticMethod SendMultipleStructures = ClientStaticMethod.Get(ReceiveMultipleStructures);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveMultipleStructures(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			byte x;
			reader.ReadUInt8(out x);
			byte y;
			reader.ReadUInt8(out y);

			StructureRegion region;
			if (!tryGetRegion(x, y, out region))
			{
				context.LogWarning($"invalid region {x} {y}");
				return;
			}

			byte packet;
			reader.ReadUInt8(out packet);

			if (packet == 0)
			{
				if (region.isNetworked)
				{
					return;
				}

				// Immediately finish any deferred cleanup.
				DestroyAllInRegion(regions[x, y]);
			}
			else
			{
				if (!region.isNetworked)
				{
					return;
				}
			}

			region.isNetworked = true;

			ushort count;
			reader.ReadUInt16(out count);

			if (count > 0)
			{
				float sortOrder;
				reader.ReadFloat(out sortOrder);

				for (ushort index = 0; index < count; ++index)
				{
					PlaceableInstantiationParameters instantiation = new PlaceableInstantiationParameters();
					instantiation.type = EPlaceableInstantiationType.Structure;
					instantiation.region = region;
					instantiation.sortOrder = sortOrder;
					instantiation.UpdateSortOrder();

					reader.ReadGuid(out instantiation.assetId);
					reader.ReadClampedVector3(out instantiation.position, fracBitCount: POSITION_FRAC_BIT_COUNT);
					reader.ReadSpecialYawOrQuaternion(out instantiation.rotation, yawBitCount: YAW_BIT_COUNT);
					reader.ReadUInt8(out instantiation.hp);
					reader.ReadUInt64(out instantiation.owner);
					reader.ReadUInt64(out instantiation.group);
					reader.ReadNetId(out instantiation.netId);

					NetInvocationDeferralRegistry.MarkDeferred(instantiation.netId, NETIDS_PER_STRUCTURE);
					PlaceableInstantiationManager.AddInstantiation(ref instantiation);
				}
			}

			Level.isLoadingStructures = false;
		}

		[System.Obsolete]
		public void askStructures(CSteamID steamID, byte x, byte y)
		{ }

		internal void askStructures(ITransportConnection transportConnection, byte x, byte y, float sortOrder)
		{
			StructureRegion region;
			if (!tryGetRegion(x, y, out region))
				return;

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
						size += 49;

						count++;

						if (size > Block.BUFFER_SIZE / 2)
						{
							break;
						}
					}

					SendMultipleStructuresWriteParameters sendMultipleStructuresWriteParameters = new SendMultipleStructuresWriteParameters()
					{
						region = region,
						index = index,
						count = count,
						sortOrder = sortOrder,
						packet = packet,
						x = x,
						y = y,
					};
					index = count;

					SendMultipleStructures.Invoke(ENetReliability.Reliable, transportConnection,
						SendMultipleStructures_Write, sendMultipleStructuresWriteParameters);

					packet++;
				}
			}
			else
			{
				SendMultipleStructures.Invoke(ENetReliability.Reliable, transportConnection,
					SendMultipleStructures_WriteEmpty, x, y);
			}
		}

		struct SendMultipleStructuresWriteParameters
		{
			public StructureRegion region;
			public int index;
			public int count;
			public float sortOrder;
			public byte packet;
			public byte x;
			public byte y;
		}

		private static void SendMultipleStructures_Write(NetPakWriter writer, SendMultipleStructuresWriteParameters p)
		{
			writer.WriteUInt8(p.x);
			writer.WriteUInt8(p.y);
			writer.WriteUInt8(p.packet);
			writer.WriteUInt16((ushort) (p.count - p.index));
			writer.WriteFloat(p.sortOrder);

			while (p.index < p.count)
			{
				StructureData data = p.region.drops[p.index].serversideData;

				writer.WriteGuid(data.structure.asset.GUID); // 16 bytes
				writer.WriteClampedVector3(data.point, fracBitCount: POSITION_FRAC_BIT_COUNT); // 9 bytes
				writer.WriteSpecialYawOrQuaternion(data.rotation, yawBitCount: YAW_BIT_COUNT); // ~3 bytes
				writer.WriteUInt8((byte) Mathf.RoundToInt(data.structure.health / (float) data.structure.asset.health * 100));
				writer.WriteUInt64(data.owner); // 8 bytes
				writer.WriteUInt64(data.group); // 8 bytes
				writer.WriteNetId(p.region.drops[p.index].GetNetId()); // 4 bytes
				// If changing size update size counter above.

				p.index++;
			}
		}

		private static void SendMultipleStructures_WriteEmpty(NetPakWriter writer, byte x, byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt8(0);
			writer.WriteUInt16(0);
		}

		private static void updateActivity(StructureRegion region, CSteamID owner, CSteamID group)
		{
			foreach (StructureDrop structure in region.drops)
			{
				StructureData data = structure.serversideData;
				if (OwnershipTool.checkToggle(owner, data.owner, group, data.group))
				{
					//UnturnedLog.info("Marking {0} active", data.structure.asset.getTypeNameAndIdDisplayString());
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
					StructureRegion region = regions[x, y];
					updateActivity(region, owner, group);
				}
			}
		}

		/// <summary>
		/// Not ideal, but there was a problem because onLevelLoaded was not resetting these after disconnecting.
		/// </summary>
		internal static void ClearNetworkStuff()
		{
			regionsPendingDestroy = new List<StructureRegion>();
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				regions = new StructureRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						regions[x, y] = new StructureRegion();
					}
				}
				instanceCount = 0;
				pool = new Dictionary<int, Stack<GameObject>>();
				housingConnections = new HousingConnections();

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
							if (player.movement.loadedRegions[x, y].isStructuresLoaded && !Regions.checkArea(x, y, new_x, new_y, STRUCTURE_REGIONS))
							{
								player.movement.loadedRegions[x, y].isStructuresLoaded = false;
							}
						}
						else if (player.channel.IsLocalPlayer)
						{
							if (regions[x, y].isNetworked && !Regions.checkArea(x, y, new_x, new_y, STRUCTURE_REGIONS))
							{
								if (regions[x, y].drops.Count > 0)
								{
									// Defer cleanup.
									regions[x, y].isPendingDestroy = true;
									regionsPendingDestroy.Add(regions[x, y]);
								}
								PlaceableInstantiationManager.CancelInstantiationsInRegion(regions[x, y], NETIDS_PER_STRUCTURE);

								regions[x, y].isNetworked = false;
							}
						}
					}
				}
			}

			if (step == 1)
			{
				if (Dedicator.IsDedicatedServer)
				{
					if (Regions.checkSafe(new_x, new_y))
					{
						Vector3 playerPosition = player.transform.position;
						for (int x = new_x - STRUCTURE_REGIONS; x <= new_x + STRUCTURE_REGIONS; x++)
						{
							for (int y = new_y - STRUCTURE_REGIONS; y <= new_y + STRUCTURE_REGIONS; y++)
							{
								if (Regions.checkSafe((byte) x, (byte) y) && !player.movement.loadedRegions[x, y].isStructuresLoaded)
								{
									player.movement.loadedRegions[x, y].isStructuresLoaded = true;

									float sortOrder = Regions.HorizontalDistanceFromCenterSquared(x, y, playerPosition);
									askStructures(player.channel.owner.transportConnection, (byte) x, (byte) y, sortOrder);
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

			Level.onLevelLoaded += onLevelLoaded;
			Player.onPlayerCreated += onPlayerCreated;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			int regionsWithStructures = 0;
			int structuresInRegions = 0;
			foreach (StructureRegion region in regions)
			{
				if (region.drops.Count > 0)
				{
					++regionsWithStructures;
				}
				structuresInRegions += region.drops.Count;
			}

			results.Add($"Structure regions: {regionsWithStructures}");
			results.Add($"Structures placed: {structuresInRegions}");

			if (housingConnections != null)
			{
				housingConnections.OnLogMemoryUsage(results);
			}
		}

		public static void load()
		{
			bool loadDefaults = false;

			if (LevelSavedata.fileExists("/Structures.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				River river = LevelSavedata.openRiver("/Structures.dat", true);
				byte version = river.readByte();

				if (version > 3)
				{
					serverActiveDate = river.readUInt32();
				}
				else
				{
					serverActiveDate = Provider.time;
				}

				if (version < 7)
				{
					instanceCount = 0;
				}
				else
				{
					instanceCount = river.readUInt32();
				}

				if (version > 1)
				{
					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							loadRegion(version, river);
						}
					}
				}

				if (version < 6)
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

						StructureRegion region = regions[x, y];
						for (int index = 0; index < defaults.Count; index++)
						{
							LevelBuildableObject buildable = defaults[index];

							if (buildable == null)
							{
								continue;
							}

							ItemStructureAsset asset = buildable.asset as ItemStructureAsset;

							if (asset == null)
							{
								continue;
							}

							Structure structure = new Structure(asset, asset.health);
							StructureData serversideData = new StructureData(structure, buildable.point, buildable.rotation, 0, 0, uint.MaxValue, ++instanceCount);
							NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_STRUCTURE);

							Transform spawnedInstance = manager.spawnStructure(region, asset.GUID, serversideData.point, serversideData.rotation, (byte) Mathf.RoundToInt(structure.health / (float) asset.health * 100), 0, 0, netId);
							if (spawnedInstance != null)
							{
								StructureDrop drop = region.drops.GetTail();
								drop.serversideData = serversideData;
#pragma warning disable
								region.structures.Add(serversideData);
#pragma warning restore
								++spawnCount;
							}
							else
							{
								UnturnedLog.warn($"Failed to spawn default structure object {asset.name} at {buildable.point}");
							}
						}
					}
				}

				UnturnedLog.info($"Spawned {spawnCount} default structures from level");
			}

			Level.isLoadingStructures = false;
		}

		public static void save()
		{
			River river = LevelSavedata.openRiver("/Structures.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);

			river.writeUInt32(Provider.time);
			river.writeUInt32(instanceCount);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					StructureRegion region = regions[x, y];
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
					StructureRegion region = regions[x, y];
					foreach (StructureDrop structure in region.drops)
					{
						if (ids.Contains(structure.instanceID))
						{
							UnturnedLog.error("Structure instance ID {0} is not unique!", structure.instanceID);
						}
						else
						{
							ids.Add(structure.instanceID);
						}
					}
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		private static void loadRegion(byte version, River river)
		{
			ushort count = river.readUInt16();
			for (ushort index = 0; index < count; index++)
			{
				ItemStructureAsset asset;
				if (version < 8)
				{
					ushort id = river.readUInt16();
					asset = Assets.find(EAssetType.ITEM, id) as ItemStructureAsset;
				}
				else
				{
					System.Guid guid = river.readGUID();
					asset = Assets.find(guid) as ItemStructureAsset;
				}

				uint instanceID;
				if (version < 7)
				{
					instanceID = ++instanceCount;
				}
				else
				{
					instanceID = river.readUInt32();
				}

				ushort health = river.readUInt16();
				Vector3 point = river.readSingleVector3();

				Quaternion rotation;
				if (version < SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION)
				{
					byte angle_x = 0;
					if (version > 4)
					{
						angle_x = river.readByte();
					}
					byte angle_y = river.readByte();
					byte angle_z = 0;
					if (version > 4)
					{
						angle_z = river.readByte();
					}

					if (version < 5)
					{
						rotation = Quaternion.Euler(-90, angle_y * 2, 0);
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

				if (version > 2)
				{
					owner = river.readUInt64();
					group = river.readUInt64();
				}

				uint activeDate;
				if (version > 3)
				{
					activeDate = river.readUInt32();

					if (Provider.time - serverActiveDate > Provider.modeConfigData.Structures.Decay_Time / 2)
					{
						activeDate = Provider.time;
					}
				}
				else
				{
					activeDate = Provider.time;
				}

				if (asset != null)
				{
					if (!Regions.tryGetCoordinate(point, out byte x, out byte y))
					{
						UnturnedLog.warn($"Discarding loaded structure {asset.FriendlyName} because it is outside the maximum level size at {point}");
						continue;
					}

					NetId netId = NetIdRegistry.ClaimBlock(NETIDS_PER_STRUCTURE);

					StructureRegion region = regions[x, y];
					Transform spawnedInstance = manager.spawnStructure(region, asset.GUID, point, rotation, (byte) Mathf.RoundToInt(health / (float) asset.health * 100), owner, group, netId);
					if (spawnedInstance != null)
					{
						StructureDrop drop = region.drops.GetTail();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
						CheckStructureRegionCoordIsCorrect(drop, region, "loadRegion");
#endif

						StructureData serversideData = new StructureData(new Structure(asset, health), point, rotation, owner, group, activeDate, instanceID);
						drop.serversideData = serversideData;
#pragma warning disable
						region.structures.Add(serversideData);
#pragma warning restore
					}
				}
			}
		}

		private static void saveRegion(River river, StructureRegion region)
		{
			uint time = Provider.time;

			ushort count = 0;
			foreach (StructureDrop structure in region.drops)
			{
				StructureData data = structure.serversideData;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				CheckStructureRegionCoordIsCorrect(structure, region, "saveRegion");
#endif

				if (!Dedicator.IsDedicatedServer || Provider.modeConfigData.Structures.Decay_Time == 0 || time < data.objActiveDate || time - data.objActiveDate < Provider.modeConfigData.Structures.Decay_Time)
				{
					if (data.structure.asset.isSaveable)
					{
						count++;
					}
				}
			}

			river.writeUInt16(count);
			foreach (StructureDrop structure in region.drops)
			{
				StructureData data = structure.serversideData;
				if (!Dedicator.IsDedicatedServer || Provider.modeConfigData.Structures.Decay_Time == 0 || time < data.objActiveDate || time - data.objActiveDate < Provider.modeConfigData.Structures.Decay_Time)
				{
					if (data.structure.asset.isSaveable)
					{
						river.writeGUID(structure.asset.GUID);
						river.writeUInt32(data.instanceID);
						river.writeUInt16(data.structure.health);
						river.writeSingleVector3(data.point);
						river.writeSingleQuaternion(data.rotation);
						river.writeUInt64(data.owner);
						river.writeUInt64(data.group);
						river.writeUInt32(data.objActiveDate);
					}
				}
			}
		}

		public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y)
		{
			return Regions.GatherRemoteClientConnections(x, y, STRUCTURE_REGIONS);
		}

		[System.Obsolete("Replaced by GatherRemoteClientConnections")]
		public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y)
		{
			return GatherRemoteClientConnections(x, y);
		}

		private static void DestroyAllInRegion(StructureRegion region)
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

		internal void DestroyOrReleaseStructure(StructureDrop drop)
		{
			try
			{
				housingConnections.UnlinkConnections(drop);
			}
			catch (System.Exception e)
			{
				// try/catch because I do not want to risk breaking structures in the big update
				UnturnedLog.exception(e, "Caught exception while unlinking housing connections:");
			}

#if !DEDICATED_SERVER
			drop.RemoveFoliageCut();
#endif // !DEDICATED_SERVER

			EffectManager.ClearAttachments(drop.model);

			if (drop.asset.eligibleForPooling)
			{
				drop.model.gameObject.SetActive(false);
				int prefabKey = drop.asset.structure.GetInstanceID();
				Stack<GameObject> instances = pool.GetOrAddNew(prefabKey);
				instances.Push(drop.model.gameObject);
			}
			else
			{
				Destroy(drop.model.gameObject);
			}
		}

		/// <summary>
		/// Maps prefab unique id to inactive list.
		/// </summary>
		private Dictionary<int, Stack<GameObject>> pool;

#if !DEDICATED_SERVER
		internal static void HandleInstantiation(ref PlaceableInstantiationParameters instantiation)
		{
			StructureRegion region = (StructureRegion) instantiation.region;
			Transform result = instance.spawnStructure(region, instantiation.assetId, instantiation.position, instantiation.rotation, instantiation.hp, instantiation.owner, instantiation.group, instantiation.netId);
			if (result != null)
			{
				NetInvocationDeferralRegistry.Invoke(instantiation.netId, NETIDS_PER_STRUCTURE);
			}
			else
			{
				NetInvocationDeferralRegistry.Cancel(instantiation.netId, NETIDS_PER_STRUCTURE);
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal static void CheckStructureRegionCoordIsCorrect(StructureDrop structure, byte x, byte y, string context)
		{
			tryGetRegion(structure.model, out byte expected_x, out byte expected_y, out StructureRegion region);

			if (x != expected_x || y != expected_y)
			{
				UnturnedLog.error($"Structure {structure.asset?.FriendlyName} at {structure.model?.position} expected in cell {expected_x}, {expected_y} but actually in {x}, {y}, ({context})");
			}
		}

		private static void CheckStructureRegionCoordIsCorrect(StructureDrop structure, StructureRegion region, string context)
		{
			for (byte x = 0; x < Regions.WORLD_SIZE; ++x)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; ++y)
				{
					if (regions[x, y] == region)
					{
						CheckStructureRegionCoordIsCorrect(structure, x, y, context);
						return;
					}
				}
			}
		}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		private System.Diagnostics.Stopwatch destroyTimer = new System.Diagnostics.Stopwatch();
		private const int MIN_DESTROY_PER_FRAME = 10;
		private void Update()
		{
			if (MainCamera.instance != null)
			{
				housingConnections.DrawGizmos();
			}

			if (!Provider.isConnected)
				return;

			if (regionsPendingDestroy != null && regionsPendingDestroy.Count > 0)
			{
				Profiler.BeginSample("PendingDestroy");
				destroyTimer.Restart();
				int destroyCount = 0;
				do
				{
					StructureRegion region = regionsPendingDestroy.GetTail();
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
		/// +0 = StructureDrop
		/// +1 = root transform
		/// </summary>
		internal const int NETIDS_PER_STRUCTURE = 2;
	}
}
