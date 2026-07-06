////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define WITH_TREE_GIZMOS

using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void DamageResourceRequestHandler(CSteamID instigatorSteamID, Transform objectTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin);

	public class ResourceManager : SteamCaller
	{
		public static readonly byte RESOURCE_REGIONS = 2;

		public static DamageResourceRequestHandler onDamageResourceRequested;

		private static ResourceManager manager;

		private static ResourceRegion[,] regions;

		private static byte respawnResources_X;
		private static byte respawnResources_Y;

		[System.Obsolete]
		public void tellClearRegionResources(CSteamID steamID, byte x, byte y)
		{
			ReceiveClearRegionResources(x, y);
		}

		private static readonly ClientStaticMethod<byte, byte> SendClearRegionResources = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionResources);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellClearRegionResources))]
		public static void ReceiveClearRegionResources(byte x, byte y)
		{
			if (!Provider.isServer)
			{
				if (!regions[x, y].isNetworked)
					return;
			}

			List<ResourceSpawnpoint> trees = LevelGround.GetTreesOrNullInRegion(x, y);
			if (trees != null)
			{
				foreach (ResourceSpawnpoint tree in trees)
				{
					tree.revive();
				}
			}
		}

		/// <summary>
		/// Revive all trees in a specific region.
		/// </summary>
		public static void askClearRegionResources(byte x, byte y)
		{
			if (!Provider.isServer)
				return;

			if (!Regions.checkSafe(x, y))
				return;

			List<ResourceSpawnpoint> trees = LevelGround.GetTreesOrNullInRegion(x, y);
			if (trees != null && trees.Count > 0)
			{
				SendClearRegionResources.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), x, y);
			}
		}

		/// <summary>
		/// Revive trees worldwide. Used between arena rounds.
		/// </summary>
		public static void askClearAllResources()
		{
			if (!Provider.isServer)
				return;

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					askClearRegionResources(x, y);
				}
			}
		}

		public static void getResourcesInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
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

				List<ResourceSpawnpoint> trees = LevelGround.GetTreesOrNullInRegion(regionCoordinate.x, regionCoordinate.y);
				if (trees != null)
				{
					foreach (ResourceSpawnpoint tree in trees)
					{
						if (tree.model == null || tree.isDead)
						{
							continue;
						}

						Vector3 offset = tree.point - center;

						if (offset.sqrMagnitude < sqrRadius)
						{
							result.Add(tree.model);
						}
					}
				}
			}
		}

		public static void damage(Transform resource, Vector3 direction, float damage, float times, float drop, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown, bool trackKill = true)
		{
			xp = 0;
			kill = EPlayerKill.NONE;

			ushort totalDamage = (ushort) (damage * times);
			bool shouldAllow = true;

			// Allow plugins to modify damage or cancel it
			onDamageResourceRequested?.Invoke(instigatorSteamID, resource, ref totalDamage, ref shouldAllow, damageOrigin);

			if (!shouldAllow || totalDamage < 1)
			{
				return;
			}

			byte x;
			byte y;

			if (Regions.tryGetCoordinate(resource.position, out x, out y))
			{
				List<ResourceSpawnpoint> region = LevelGround.GetTreesOrNullInRegion(x, y);
				if (region == null)
				{
					return;
				}

				for (ushort index = 0; index < region.Count; index++)
				{
					if (resource == region[index].model)
					{
						if (!region[index].isDead && region[index].canBeDamaged)
						{
							region[index].askDamage(totalDamage);

							if (region[index].isDead)
							{
								kill = EPlayerKill.RESOURCE;

								ResourceAsset asset = region[index].asset;
								if (region[index].asset != null)
								{
									EffectAsset explosionAsset = asset.FindExplosionEffectAsset();
									if (explosionAsset != null)
									{
										TriggerEffectParameters explosion = new TriggerEffectParameters(explosionAsset);
										explosion.position = region[index].GetEffectSpawnPosition();
										explosion.relevantDistance = EffectManager.MEDIUM;
										explosion.reliable = true;
										EffectManager.triggerEffect(explosion);
									}

									if (!asset.isForage)
									{
										float dropMultiplier = Provider.modeConfigData.Objects.Resource_Drops_Multiplier;
										dropMultiplier *= drop;

										if (asset.rewardID != 0)
										{
											Vector3 localDropDirection = resource.InverseTransformDirection(direction);
											localDropDirection.y = 0.0f;
											localDropDirection.Normalize();
											Vector3 dropDirection = resource.TransformDirection(localDropDirection);

											int rewards = Mathf.CeilToInt(Random.Range(asset.rewardMin, asset.rewardMax + 1) * dropMultiplier);
											// Prevent players from crashing themselves with huge numbers of items.
											rewards = Mathf.Clamp(rewards, 0, 100);
											for (int reward = 0; reward < rewards; reward++)
											{
												ushort id = SpawnTableTool.ResolveLegacyId(asset.rewardID, EAssetType.ITEM, asset.OnGetRewardSpawnTableErrorContext);
												if (id != 0)
												{
													Vector3 dropPosition;

													if (asset.hasDebris)
													{
														dropPosition = resource.position + (dropDirection * (2 + reward)) + resource.up * 2f;
													}
													else
													{
														dropPosition = resource.position
															+ resource.right * Random.Range(-2.0f, 2.0f)
															+ resource.up * 2.0f
															+ resource.forward * Random.Range(-2.0f, 2.0f);
													}

													ItemManager.dropItem(new Item(id, EItemOrigin.NATURE), dropPosition, false, Dedicator.IsDedicatedServer, true);
												}
											}
										}
										else
										{
											if (asset.log != 0)
											{
												Vector3 localDropDirection = resource.InverseTransformDirection(direction);
												localDropDirection.y = 0.0f;
												localDropDirection.Normalize();
												Vector3 dropDirection = resource.TransformDirection(localDropDirection);

												int drops = Mathf.CeilToInt(Random.Range(3, 7) * dropMultiplier);
												// Prevent players from crashing themselves with huge numbers of items.
												drops = Mathf.Clamp(drops, 0, 100);
												for (int step = 0; step < drops; step++)
												{
													ItemManager.dropItem(new Item(asset.log, EItemOrigin.NATURE), resource.position + (direction * (2 + (step * 2))) + resource.up, false, Dedicator.IsDedicatedServer, true);
												}
											}

											if (asset.stick != 0)
											{
												int drops = Mathf.CeilToInt(Random.Range(2, 5) * dropMultiplier);
												// Prevent players from crashing themselves with huge numbers of items.
												drops = Mathf.Clamp(drops, 0, 100);
												for (int step = 0; step < drops; step++)
												{
													float angle = Random.Range(0, Mathf.PI * 2);
													Vector3 dropPosition = resource.position
														+ resource.right * Mathf.Sin(angle) * 3
														+ resource.up
														+ resource.forward * Mathf.Cos(angle) * 3;

													ItemManager.dropItem(new Item(asset.stick, EItemOrigin.NATURE), dropPosition, false, Dedicator.IsDedicatedServer, true);
												}
											}
										}

										xp = asset.rewardXP;

										Vector3 position = region[index].point;
										System.Guid treeGuid = asset.GUID;
										for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
										{
											SteamPlayer player = Provider.clients[playerIndex];

											if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
											{
												continue;
											}

											if ((player.player.transform.position - position).sqrMagnitude < 90000) // 300 meters is the max damage distance
											{
												player.player.quests.trackTreeKill(treeGuid);
											}
										}
									}
								}

								ServerSetResourceDead(x, y, index, direction * totalDamage);
							}
						}

						break;
					}
				}
			}
		}

		public static void forage(Transform resource)
		{
			byte x;
			byte y;

			if (Regions.tryGetCoordinate(resource.position, out x, out y))
			{
				List<ResourceSpawnpoint> region = LevelGround.GetTreesOrNullInRegion(x, y);
				if (region == null)
				{
					return;
				}

				for (ushort index = 0; index < region.Count; index++)
				{
					if (resource == region[index].model)
					{
						SendForageRequest.Invoke(ENetReliability.Unreliable, x, y, index);
						break;
					}
				}
			}
		}

		[System.Obsolete]
		public void askForage(CSteamID steamID, byte x, byte y, ushort index)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveForageRequest(context, x, y, index);
		}

		private static readonly ServerStaticMethod<byte, byte, ushort> SendForageRequest = ServerStaticMethod<byte, byte, ushort>.Get(ReceiveForageRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = nameof(askForage))]
		public static void ReceiveForageRequest(in ServerInvocationContext context, byte x, byte y, ushort index)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			List<ResourceSpawnpoint> region = LevelGround.GetTreesOrNullInRegion(x, y);
			if (region == null)
			{
				return;
			}

			if (index >= region.Count)
			{
				return;
			}

			if (region[index].isDead)
			{
				return;
			}

			if ((region[index].point - player.transform.position).sqrMagnitude > 400) // 20m
			{
				return;
			}

			ResourceAsset asset = region[index].asset;
			if (asset == null || !asset.isForage)
			{
				return;
			}

			region[index].askDamage(1);

			EffectAsset explosionAsset = asset.FindExplosionEffectAsset();
			if (explosionAsset != null)
			{
				TriggerEffectParameters explosion = new TriggerEffectParameters(explosionAsset);
				explosion.position = region[index].GetEffectSpawnPosition();
				explosion.relevantDistance = EffectManager.MEDIUM;
				explosion.reliable = true;
				EffectManager.triggerEffect(explosion);
			}

			ushort id = 0;
			if (asset.rewardID != 0)
			{
				id = SpawnTableTool.ResolveLegacyId(asset.rewardID, EAssetType.ITEM, asset.OnGetRewardSpawnTableErrorContext);
			}
			else
			{
				id = asset.log;
			}

			if (id != 0)
			{
				player.inventory.forceAddItem(new Item(id, EItemOrigin.NATURE), true);
				if (Random.value < player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.AGRICULTURE))
				{
					player.inventory.forceAddItem(new Item(id, EItemOrigin.NATURE), true);
				}
			}

			player.sendStat(EPlayerStat.FOUND_PLANTS);
			player.skills.askPay(asset.forageRewardExperience);

			ServerSetResourceDead(x, y, index, Vector3.zero);
		}

		[System.Obsolete]
		public void tellResourceDead(CSteamID steamID, byte x, byte y, ushort index, Vector3 ragdoll)
		{
			ReceiveResourceDead(x, y, index, ragdoll);
		}

		private static readonly ClientStaticMethod<byte, byte, ushort, Vector3> SendResourceDead = ClientStaticMethod<byte, byte, ushort, Vector3>.Get(ReceiveResourceDead);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellResourceDead))]
		public static void ReceiveResourceDead(byte x, byte y, ushort index, Vector3 ragdoll)
		{
			List<ResourceSpawnpoint> regionTrees = LevelGround.GetTreesOrNullInRegion(x, y);
			if (regionTrees == null)
			{
				return;
			}

			if (index >= regionTrees.Count)
			{
				return;
			}

			if (!Provider.isServer)
			{
				//if(!regions[x, y].isMarked || !regions[x, y].isNetworked)
				//{
				//	return;
				//}
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			regionTrees[index].kill(ragdoll);
		}

		[System.Obsolete]
		public void tellResourceAlive(CSteamID steamID, byte x, byte y, ushort index)
		{
			ReceiveResourceAlive(x, y, index);
		}

		private static readonly ClientStaticMethod<byte, byte, ushort> SendResourceAlive = ClientStaticMethod<byte, byte, ushort>.Get(ReceiveResourceAlive);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellResourceAlive))]
		public static void ReceiveResourceAlive(byte x, byte y, ushort index)
		{
			List<ResourceSpawnpoint> regionTrees = LevelGround.GetTreesOrNullInRegion(x, y);
			if (regionTrees == null)
			{
				return;
			}

			if (index >= regionTrees.Count)
			{
				return;
			}

			if (!Provider.isServer)
			{
				//if(!regions[x, y].isMarked || !regions[x, y].isNetworked)
				//{
				//	return;
				//}
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			regionTrees[index].revive();
		}

		[System.Obsolete]
		public void tellResources(CSteamID steamID, byte x, byte y, bool[] resources)
		{ }

		private static readonly ClientStaticMethod SendResources = ClientStaticMethod.Get(ReceiveResources);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveResources(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			byte x;
			reader.ReadUInt8(out x);
			byte y;
			reader.ReadUInt8(out y);

			if (!Regions.checkSafe(x, y))
			{
				context.LogWarning($"invalid region {x} {y}");
				return;
			}

			if (regions[x, y].isNetworked)
			{
				return;
			}

			regions[x, y].isNetworked = true;

			List<ResourceSpawnpoint> regionTrees = LevelGround.GetTreesOrNullInRegion(x, y);
			if (regionTrees == null)
			{
				return;
			}

			ushort count;
			reader.ReadUInt16(out count);
			count = MathfEx.Min(count, (ushort) regionTrees.Count);

			for (ushort index = 0; index < count; ++index)
			{
				bool isDead;
				if (!reader.ReadBit(out isDead))
				{
					context.LogWarning($"index {index} / count {count}");
					break;
				}

				if (isDead)
				{
					regionTrees[index].wipe();
				}
				else
				{
					regionTrees[index].revive();
				}
			}
		}

		private static void SendResources_Write(NetPakWriter writer, byte x, byte y)
		{
			List<ResourceSpawnpoint> regionTrees = LevelGround.GetTreesOrNullInRegion(x, y);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			ushort count = (ushort) regionTrees.Count;
			writer.WriteUInt16(count);
			for (ushort index = 0; index < count; ++index)
			{
				writer.WriteBit(regionTrees[index].isDead);
			}
		}

		public static ResourceSpawnpoint getResourceSpawnpoint(byte x, byte y, ushort index)
		{
			if (!Regions.checkSafe(x, y))
			{
				return null;
			}

			List<ResourceSpawnpoint> region = LevelGround.GetTreesOrNullInRegion(x, y);
			if (region == null)
			{
				return null;
			}

			if (index >= region.Count)
			{
				return null;
			}

			return region[index];
		}

		public static Transform getResource(byte x, byte y, ushort index)
		{
			ResourceSpawnpoint resource = getResourceSpawnpoint(x, y, index);

			if (resource != null)
			{
				if (resource.model != null)
				{
					return resource.model;
				}
				else
				{
					return resource.stump;
				}
			}

			return null;
		}

		public static bool tryGetRegion(Transform resource, out byte x, out byte y, out ushort index)
		{
			x = 0;
			y = 0;
			index = 0;

			if (Regions.tryGetCoordinate(resource.position, out x, out y))
			{
				List<ResourceSpawnpoint> region = LevelGround.GetTreesOrNullInRegion(x, y);
				for (index = 0; index < region.Count; index++)
				{
					if (resource == region[index].model || resource == region[index].stump)
					{
						return true;
					}
				}
			}

			return false;
		}

		private List<Collider> treeColliders = new List<Collider>();
		private bool overlapTreeColliders(ResourceSpawnpoint tree, int layerMask)
		{
			treeColliders.Clear();
			if (tree.model == null)
				return false;

			const bool includeInactive = true;
			tree.model.GetComponentsInChildren(includeInactive, treeColliders);

			foreach (Collider treeCollider in treeColliders)
			{
				if (treeCollider is BoxCollider box)
				{
					bool hitAnything = box.OverlapBoxSingle(layerMask, QueryTriggerInteraction.Collide) != null;
					if (hitAnything)
					{
						return true;
					}
				}
				else if (treeCollider is SphereCollider sphere)
				{
					bool hitAnything = sphere.OverlapSphereSingle(layerMask, QueryTriggerInteraction.Collide) != null;
#if WITH_TREE_GIZMOS
					sphere.DrawSphereGizmo(hitAnything ? Color.red : Color.green, lifespan: 25.0f);
#endif // WITH_TREE_GIZMOS
					if (hitAnything)
					{
						return true;
					}
				}
				else if (treeCollider is CapsuleCollider capsule)
				{
					bool hitAnything = capsule.OverlapCapsuleSingle(layerMask, QueryTriggerInteraction.Collide) != null;
#if WITH_TREE_GIZMOS
					capsule.DrawCapsuleGizmo(hitAnything ? Color.red : Color.green, lifespan: 25.0f);
#endif // WITH_TREE_GIZMOS
					if (hitAnything)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static void ServerSetResourceAlive(byte x, byte y, ushort index)
		{
			SendResourceAlive.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClients(x, y), x, y, index);
		}

		public static void ServerSetResourceDead(byte x, byte y, ushort index, Vector3 baseForce)
		{
			SendResourceDead.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClients(x, y), x, y, index, baseForce);
		}

		private bool respawnResources()
		{
			List<ResourceSpawnpoint> regionTrees = LevelGround.GetTreesOrNullInRegion(respawnResources_X, respawnResources_Y);
			if (regionTrees != null && regionTrees.Count > 0)
			{
				if (regions[respawnResources_X, respawnResources_Y].respawnResourceIndex >= regionTrees.Count)
				{
					regions[respawnResources_X, respawnResources_Y].respawnResourceIndex = (ushort) (regionTrees.Count - 1);
				}

				ResourceSpawnpoint spawn = regionTrees[regions[respawnResources_X, respawnResources_Y].respawnResourceIndex];

				if (spawn.checkCanReset(Provider.modeConfigData.Objects.Resource_Reset_Multiplier))
				{
					// Prevent respawn if a player is standing on the stump, or if they have built something (e.g. bed) on the stump.
					int layerMask = RayMasks.PLAYER | RayMasks.ENEMY;
					if (Provider.modeConfigData.Objects.Items_Obstruct_Tree_Respawns)
					{
						layerMask |= RayMasks.BARRICADE;
					}

					bool hitAnything = overlapTreeColliders(spawn, layerMask);
					if (!hitAnything)
					{
						ServerSetResourceAlive(respawnResources_X, respawnResources_Y, regions[respawnResources_X, respawnResources_Y].respawnResourceIndex);
					}
				}

				return false;
			}

			return true;
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				regions = new ResourceRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						regions[x, y] = new ResourceRegion();
					}
				}

				respawnResources_X = 0;
				respawnResources_Y = 0;
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
						//if(player.channel.isOwner)
						//{
						//	if(regions[x, y].isMarked && !Regions.checkArea(x, y, new_x, new_y, RESOURCE_REGIONS))
						//	{
						//		regions[x, y].isMarked = false;
						//		regions[x, y].isNetworked = false;
						//	}
						//}

						//if(Provider.isServer)
						//{
						//	if(player.movement.loadedRegions[x, y].isResourcesLoaded && !Regions.checkArea(x, y, new_x, new_y, RESOURCE_REGIONS))
						//	{
						//		player.movement.loadedRegions[x, y].isResourcesLoaded = false;
						//	}
						//}
						if (Provider.isServer)
						{
							if (player.movement.loadedRegions[x, y].isResourcesLoaded && !Regions.checkArea(x, y, new_x, new_y, RESOURCE_REGIONS))
							{
								player.movement.loadedRegions[x, y].isResourcesLoaded = false;
							}
						}
						else if (player.channel.IsLocalPlayer)
						{
							if (regions[x, y].isNetworked && !Regions.checkArea(x, y, new_x, new_y, RESOURCE_REGIONS))
							{
								regions[x, y].isNetworked = false;
							}
						}
					}
				}
			}

			if (step == 3)
			{
				if (Dedicator.IsDedicatedServer)
				{
					if (Regions.checkSafe(new_x, new_y))
					{
						for (int x = new_x - RESOURCE_REGIONS; x <= new_x + RESOURCE_REGIONS; x++)
						{
							for (int y = new_y - RESOURCE_REGIONS; y <= new_y + RESOURCE_REGIONS; y++)
							{
								if (Regions.checkSafe((byte) x, (byte) y) && !player.movement.loadedRegions[x, y].isResourcesLoaded)
								{
									List<ResourceSpawnpoint> region = LevelGround.GetTreesOrNullInRegion(new Vector2Int(x, y));
									if (region != null)
									{
										player.movement.loadedRegions[x, y].isResourcesLoaded = true;

										SendResources.Invoke(ENetReliability.Reliable, player.channel.owner.transportConnection, SendResources_Write, (byte) x, (byte) y);
									}
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
		}

		private void Update()
		{
			if (!Provider.isServer || !Level.isLoaded)
			{
				return;
			}

			bool isValid = true;

			while (isValid)
			{
				isValid = respawnResources();

				List<ResourceSpawnpoint> regionTrees = LevelGround.GetTreesOrNullInRegion(respawnResources_X, respawnResources_Y);

				regions[respawnResources_X, respawnResources_Y].respawnResourceIndex++;
				if (regions[respawnResources_X, respawnResources_Y].respawnResourceIndex >= regionTrees?.Count)
				{
					regions[respawnResources_X, respawnResources_Y].respawnResourceIndex = 0;
				}

				respawnResources_X++;

				if (respawnResources_X >= Regions.WORLD_SIZE)
				{
					respawnResources_X = 0;
					respawnResources_Y++;

					if (respawnResources_Y >= Regions.WORLD_SIZE)
					{
						respawnResources_Y = 0;
						isValid = false;
					}
				}
			}
		}

		private void Start()
		{
			manager = this;

			Level.onLevelLoaded += onLevelLoaded;
			Player.onPlayerCreated += onPlayerCreated;
		}

		private static PooledTransportConnectionList GatherRemoteClients(byte x, byte y)
		{
			return Regions.GatherRemoteClientConnections(x, y, RESOURCE_REGIONS);
		}
	}
}
