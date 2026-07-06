////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define DRAW_ITEM_DROP_SPHERECAST
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void ServerSpawningItemDropHandler(Item item, ref Vector3 location, ref bool shouldAllow);
	public delegate void TakeItemRequestHandler(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow);

	public delegate void ItemDropAdded(Transform model, InteractableItem interactableItem);
	public delegate void ItemDropRemoved(Transform model, InteractableItem interactableItem);

	internal struct ItemInstantiationParameters : System.IComparable<ItemInstantiationParameters>
	{
		public byte region_x;
		public byte region_y;
		public ushort assetId;
		public byte amount;
		public byte quality;
		public byte[] state;
		public Vector3 point;
		public uint instanceID;
		public float sortOrder;
		public bool shouldPlayEffect;

		public int CompareTo(ItemInstantiationParameters other)
		{
			return sortOrder.CompareTo(other.sortOrder);
		}
	}

	public class ItemManager : SteamCaller
	{
		public static readonly byte ITEM_REGIONS = 1;

		public static ServerSpawningItemDropHandler onServerSpawningItemDrop;
		public static TakeItemRequestHandler onTakeItemRequested;

		public static ItemDropAdded onItemDropAdded;
		public static ItemDropRemoved onItemDropRemoved;

		private static ItemManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static ItemManager instance => manager;

		public static ItemRegion[,] regions
		{
			get;
			private set;
		}

		/// <summary>
		/// List of all interactable items. Originally only used to clamp their distance from the drop point to ensure
		/// clients can always pick them up, but now used to find items within a radius for nearby menu as well.
		/// </summary>
		public static List<InteractableItem> clampedItems;
		private static List<ItemInstantiationParameters> pendingInstantiations = new List<ItemInstantiationParameters>();
		private static List<ItemInstantiationParameters> instantiationsToInsert = new List<ItemInstantiationParameters>();
		private static List<ItemRegion> regionsPendingDestroy = new List<ItemRegion>();

		private static uint instanceCount;

		private static int clampItemIndex;
		private static byte despawnItems_X;
		private static byte despawnItems_Y;
		private static byte respawnItems_X;
		private static byte respawnItems_Y;

		/// <summary>
		/// Kept for plugin backwards compatibility.
		/// This one is problematic because on the client physics can move items between regions.
		/// </summary>
		public static void getItemsInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<InteractableItem> result)
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

				for (int dropIndex = 0; dropIndex < regions[regionCoordinate.x, regionCoordinate.y].drops.Count; dropIndex++)
				{
					ItemDrop drop = regions[regionCoordinate.x, regionCoordinate.y].drops[dropIndex];
					Vector3 offset = drop.model.position - center;

					if (offset.sqrMagnitude < sqrRadius)
					{
						result.Add(drop.interactableItem);
					}
				}
			}
		}

		/// <summary>
		/// Find physically simulated items within radius.
		/// </summary>
		public static void findSimulatedItemsInRadius(Vector3 center, float sqrRadius, List<InteractableItem> result)
		{
			if (clampedItems == null)
				return;

			foreach (InteractableItem item in clampedItems)
			{
				if (item == null)
					continue;

				Vector3 position = item.transform.position;
				Vector3 delta = position - center;
				if (delta.sqrMagnitude <= sqrRadius)
				{
					result.Add(item);
				}
			}
		}

		public static void takeItem(Transform item, byte to_x, byte to_y, byte to_rot, byte to_page)
		{
			byte x;
			byte y;

			if (Regions.tryGetCoordinate(item.position, out x, out y))
			{
				ItemRegion region = regions[x, y];

				for (int index = 0; index < region.drops.Count; index++)
				{
					if (region.drops[index].model == item)
					{
						SendTakeItemRequest.Invoke(ENetReliability.Unreliable, x, y, region.drops[index].instanceID, to_x, to_y, to_rot, to_page);
						return;
					}
				}
			}
		}

		public static void dropItem(Item item, Vector3 point, bool playEffect, bool isDropped, bool wideSpread)
		{
			if (regions == null || manager == null)
			{
				return;
			}

			if (wideSpread)
			{
				point.x += Random.Range(-0.75f, 0.75f);
				point.z += Random.Range(-0.75f, 0.75f);
			}
			else
			{
				point.x += Random.Range(-0.125f, 0.125f);
				point.z += Random.Range(-0.125f, 0.125f);
			}

			byte x;
			byte y;

			if (Regions.tryGetCoordinate(point, out x, out y))
			{
				ItemAsset asset = item.GetAsset();

				if (asset != null && !asset.isPro)
				{
					if (point.y > 0.0f)
					{
						Ray ray = new Ray(point + Vector3.up, Vector3.down);
						const float radius = 0.1f;
						const float maxDistance = 2048.0f;
						RaycastHit hit;
						Physics.SphereCast(ray, radius, out hit, maxDistance, RayMasks.BLOCK_ITEM);
#if DRAW_ITEM_DROP_SPHERECAST
						RuntimeGizmos.Get().Spherecast(ray, radius, maxDistance, hit, Color.red, Color.green, lifespan: 0.5f);
#endif // DRAW_ITEM_DROP_SPHERECAST

						if (hit.collider != null)
						{
							point.y = hit.point.y;
						}
					}

					bool shouldAllow = true;
					onServerSpawningItemDrop?.Invoke(item, ref point, ref shouldAllow);

					if (!shouldAllow)
					{
						return;
					}

					ItemData itemData = new ItemData(item, ++instanceCount, point, isDropped);
					regions[x, y].items.Add(itemData);

					SendItem.Invoke(ENetReliability.Reliable, Regions.GatherClientConnections(x, y, ITEM_REGIONS), x, y, item.id, item.amount, item.quality, item.state, point, itemData.instanceID, playEffect);
				}
			}
		}

		[System.Obsolete]
		public void tellTakeItem(CSteamID steamID, byte x, byte y, uint instanceID)
		{

		}

		private static void PlayInventoryAudio(ItemAsset item, Vector3 position)
		{
#if !DEDICATED_SERVER // OneShotAudio is excluded from dedicated server.
			if (item == null || item.inventoryAudio.IsNullOrEmpty)
			{
				return;
			}

			float volumeMultiplier;
			float pitchMultiplier;
			AudioClip clip = item.inventoryAudio.LoadAudioClip(out volumeMultiplier, out pitchMultiplier);
			if (clip == null)
			{
				return;
			}

			volumeMultiplier *= 0.25f;

			OneShotAudioParameters parameters = new OneShotAudioParameters(position, clip);
			parameters.volume = volumeMultiplier;
			parameters.pitch = pitchMultiplier;
			parameters.SetLinearRolloff(0.5f, 8.0f); // Matches legacy audio effect.
			parameters.Play();
#endif // !DEDICATED_SERVER
		}

		private static readonly ClientStaticMethod<byte, byte, uint, bool> SendDestroyItem = ClientStaticMethod<byte, byte, uint, bool>.Get(ReceiveDestroyItem);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveDestroyItem(byte x, byte y, uint instanceID, bool shouldPlayEffect)
		{
			if (!Provider.isServer)
			{
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			ItemRegion region = regions[x, y];

			for (ushort index = 0; index < region.drops.Count; index++)
			{
				if (region.drops[index].instanceID == instanceID)
				{
					onItemDropRemoved?.Invoke(region.drops[index].model, region.drops[index].interactableItem);

					if (shouldPlayEffect)
					{
						PlayInventoryAudio(region.drops[index].interactableItem.asset, region.drops[index].model.position);
					}

					Destroy(region.drops[index].model.gameObject);
					region.drops.RemoveAt(index);

					return;
				}
			}

			// Item was not found, so instantation was pending.
			CancelInstantiationByInstanceId(instanceID);
		}

		[System.Obsolete]
		public void askTakeItem(CSteamID steamID, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveTakeItemRequest(context, x, y, instanceID, to_x, to_y, to_rot, to_page);
		}

		private static readonly ServerStaticMethod<byte, byte, uint, byte, byte, byte, byte> SendTakeItemRequest = ServerStaticMethod<byte, byte, uint, byte, byte, byte, byte>.Get(ReceiveTakeItemRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = nameof(askTakeItem))]
		public static void ReceiveTakeItemRequest(in ServerInvocationContext context, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page)
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

			if (player.animator.gesture == EPlayerGesture.ARREST_START)
			{
				return;
			}

			ItemRegion region = regions[x, y];

			for (ushort index = 0; index < region.items.Count; index++)
			{
				ItemData itemData = region.items[index];
				if (itemData.instanceID == instanceID)
				{
					if (Dedicator.IsDedicatedServer)
					{
						if ((itemData.point - player.transform.position).sqrMagnitude > 400)
						{
							return;
						}
					}

					/* Server cannot do a LoS test because client may have simulated the item falling.
						
					Vector3 viewPosition = player.look.getEyesPosition();
					bool bHitSomething = Physics.Linecast(itemData.point, viewPosition, RayMasks.BLOCK_BARRICADE_INTERACT_LOS, QueryTriggerInteraction.Ignore);
					if(bHitSomething)
					{
						// Prevent grabbing items through walls.
						return;
					}
					*/

					bool shouldAllow = true;

					if (onTakeItemRequested != null)
					{
						try
						{
							onTakeItemRequested(player, x, y, instanceID, to_x, to_y, to_rot, to_page, itemData, ref shouldAllow);
						}
						catch (System.Exception e)
						{
							UnturnedLog.exception(e, "Caught exception invoking onTakeItemRequested:");
						}
					}

					if (!shouldAllow)
						return;

					bool succesfullyTook = false;
					if (to_page == 255)
					{
						succesfullyTook = player.inventory.tryAddItem(regions[x, y].items[index].item, true);
					}
					else
					{
						succesfullyTook = player.inventory.tryAddItem(regions[x, y].items[index].item, to_x, to_y, to_page, to_rot);
					}

					if (succesfullyTook)
					{
						if (!player.equipment.wasTryingToSelect && !player.equipment.HasValidUseable)
						{
							player.animator.sendGesture(EPlayerGesture.PICKUP, true);
						}

						regions[x, y].items.RemoveAt(index);

						player.sendStat(EPlayerStat.FOUND_ITEMS);
						SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.GatherClientConnections(x, y, ITEM_REGIONS), x, y, instanceID, true);
					}
					else
					{
						player.sendMessage(EPlayerMessage.SPACE);
					}

					return;
				}
			}
		}

		[System.Obsolete]
		public void tellClearRegionItems(CSteamID steamID, byte x, byte y)
		{
			ReceiveClearRegionItems(x, y);
		}

		private static readonly ClientStaticMethod<byte, byte> SendClearRegionItems = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionItems);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellClearRegionItems))]
		public static void ReceiveClearRegionItems(byte x, byte y)
		{
			if (!Provider.isServer)
			{
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			ItemRegion region = regions[x, y];
			DestroyAllInRegion(region);
			CancelInstantiationsInRegion(x, y);
		}

		public static void askClearRegionItems(byte x, byte y)
		{
			if (Provider.isServer)
			{
				if (!Regions.checkSafe(x, y))
				{
					return;
				}

				ItemRegion region = regions[x, y];

				if (region.items.Count > 0)
				{
					region.items.Clear();
					SendClearRegionItems.Invoke(ENetReliability.Reliable, Regions.GatherClientConnections(x, y, ITEM_REGIONS), x, y);
				}
			}
		}

		public static void askClearAllItems()
		{
			if (Provider.isServer)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						askClearRegionItems(x, y);
					}
				}
			}
		}

		private static List<RegionCoordinate> clearItemRegions = new List<RegionCoordinate>(4);
		public static void ServerClearItemsInSphere(Vector3 center, float radius)
		{
			clearItemRegions.Clear();
			Regions.getRegionsInRadius(center, radius, clearItemRegions);
			float sqrRadius = MathfEx.Square(radius);
			foreach (RegionCoordinate coord in clearItemRegions)
			{
				ItemRegion region = regions[coord.x, coord.y];
				for (int itemIndex = region.items.Count - 1; itemIndex >= 0; --itemIndex)
				{
					ItemData itemData = region.items[itemIndex];

					if ((itemData.point - center).sqrMagnitude > sqrRadius)
					{
						// Outside sphere.
						continue;
					}

					uint instanceID = itemData.instanceID;
					region.items.RemoveAt(itemIndex);
					SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.GatherClientConnections(coord.x, coord.y, ITEM_REGIONS), coord.x, coord.y, instanceID, false);
				}
			}
		}

		private void spawnItem(byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID, bool shouldPlayEffect)
		{
			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

			if (asset != null)
			{
				Transform origin = new GameObject().transform;
				origin.name = id.ToString();
				origin.transform.position = point;

				Transform item = ItemTool.getItem(id, 0, quality, state, false, asset, null);
				item.parent = origin;

				InteractableItem interactableItem = item.gameObject.AddComponent<InteractableItem>();
				interactableItem.item = new Item(id, amount, quality, state);
				interactableItem.asset = asset;

				item.position = point + (Vector3.up * 0.75f);
				item.rotation = Quaternion.Euler(-90 + Random.Range(-15, 15), Random.Range(0, 360), Random.Range(-15, 15));

				item.gameObject.AddComponent<Rigidbody>();
				item.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
				item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
				item.GetComponent<Rigidbody>().drag = 0.5f;
				item.GetComponent<Rigidbody>().angularDrag = 0.1f;

				if (LevelObjects.IsRegionUpdating(new Vector2Int(x, y))) // disable physics if still loading objects
				{
					item.GetComponent<Rigidbody>().useGravity = false;
					item.GetComponent<Rigidbody>().isKinematic = true;
				}

				ItemDrop drop = new ItemDrop(origin, interactableItem, instanceID);
				regions[x, y].drops.Add(drop);

				onItemDropAdded?.Invoke(item, interactableItem);

				if (shouldPlayEffect)
				{
					PlayInventoryAudio(asset, point);
				}
			}
		}

		[System.Obsolete]
		public void tellItem(CSteamID steamID, byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID)
		{
			ReceiveItem(x, y, id, amount, quality, state, point, instanceID, false);
		}

		private static readonly ClientStaticMethod<byte, byte, ushort, byte, byte, byte[], Vector3, uint, bool> SendItem =
			ClientStaticMethod<byte, byte, ushort, byte, byte, byte[], Vector3, uint, bool>.Get(ReceiveItem);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveItem(byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID, bool shouldPlayEffect)
		{
			if (!Regions.checkSafe(x, y))
			{
				return;
			}

			if (!regions[x, y].isNetworked)
			{
				return;
			}

			float sortOrder = 0.0f;
			if (MainCamera.instance != null)
			{
				sortOrder = (MainCamera.instance.transform.position - point).sqrMagnitude;
			}

			ItemInstantiationParameters instantiation = new ItemInstantiationParameters();
			instantiation.region_x = x;
			instantiation.region_y = y;
			instantiation.assetId = id;
			instantiation.amount = amount;
			instantiation.quality = quality;
			instantiation.state = state;
			instantiation.point = point;
			instantiation.instanceID = instanceID;
			instantiation.sortOrder = sortOrder;
			instantiation.shouldPlayEffect = shouldPlayEffect;
			pendingInstantiations.Insert(pendingInstantiations.FindInsertionIndex(instantiation), instantiation);
		}

		[System.Obsolete]
		public void tellItems(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendItems = ClientStaticMethod.Get(ReceiveItems);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveItems(in ClientInvocationContext context)
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

			byte packet;
			reader.ReadUInt8(out packet);

			if (packet == 0)
			{
				if (regions[x, y].isNetworked)
				{
					return;
				}

				// Immediately finish any deferred cleanup.
				DestroyAllInRegion(regions[x, y]);
			}
			else
			{
				if (!regions[x, y].isNetworked)
				{
					return;
				}
			}

			regions[x, y].isNetworked = true;

			ushort count;
			reader.ReadUInt16(out count);
			if (count > 0)
			{
				float sortOrder;
				reader.ReadFloat(out sortOrder);

				instantiationsToInsert.Clear();
				for (ushort index = 0; index < count; ++index)
				{
					ItemInstantiationParameters instantiation = new ItemInstantiationParameters();
					instantiation.region_x = x;
					instantiation.region_y = y;
					instantiation.sortOrder = sortOrder;

					reader.ReadUInt16(out instantiation.assetId);
					reader.ReadUInt8(out instantiation.amount);
					reader.ReadUInt8(out instantiation.quality);

					byte stateLength;
					reader.ReadUInt8(out stateLength);
					byte[] state = new byte[stateLength];
					reader.ReadBytes(state);
					instantiation.state = state;

					reader.ReadClampedVector3(out instantiation.point);
					reader.ReadUInt32(out instantiation.instanceID);

					instantiationsToInsert.Add(instantiation);
				}
				pendingInstantiations.InsertRange(pendingInstantiations.FindInsertionIndex(instantiationsToInsert[0]), instantiationsToInsert);
			}
		}

		[System.Obsolete]
		public void askItems(CSteamID steamID, byte x, byte y)
		{ }

		internal void askItems(ITransportConnection transportConnection, byte x, byte y, float sortOrder)
		{
			if (regions[x, y].items.Count > 0)
			{
				byte packet = 0;
				int index = 0;
				int count = 0;
				int size = 0;
				while (index < regions[x, y].items.Count)
				{
					size = 0;
					while (count < regions[x, y].items.Count)
					{
						size += 2 + 1 + 1 + regions[x, y].items[count].item.state.Length + 12 + 4;// id, amount, quality, state, point, instance

						count++;

						if (size > Block.BUFFER_SIZE / 2)
						{
							break;
						}
					}

					SendItemsWriteParameters parameters = new SendItemsWriteParameters()
					{
						x = x,
						y = y,
						packet = packet,
						index = index,
						count = count,
						sortOrder = sortOrder,
					};
					index = count;

					SendItems.Invoke(ENetReliability.Reliable, transportConnection, SendItems_Write, parameters);

					packet++;
				}
			}
			else
			{
				SendItems.Invoke(ENetReliability.Reliable, transportConnection, SendItems_WriteEmpty, x, y);
			}
		}

		struct SendItemsWriteParameters
		{
			public int index;
			public int count;
			public float sortOrder;
			public byte x;
			public byte y;
			public byte packet;
		}

		private static void SendItems_Write(NetPakWriter writer, SendItemsWriteParameters p)
		{
			writer.WriteUInt8(p.x);
			writer.WriteUInt8(p.y);
			writer.WriteUInt8(p.packet);
			writer.WriteUInt16((ushort) (p.count - p.index));
			writer.WriteFloat(p.sortOrder);

			List<ItemData> items = regions[p.x, p.y].items;
			while (p.index < p.count)
			{
				ItemData data = items[p.index];

				writer.WriteUInt16(data.item.id);
				writer.WriteUInt8(data.item.amount);
				writer.WriteUInt8(data.item.quality);
				writer.WriteUInt8((byte) data.item.state.Length);
				writer.WriteBytes(data.item.state);
				writer.WriteClampedVector3(data.point);
				writer.WriteUInt32(data.instanceID);

				p.index++;
			}
		}

		private static void SendItems_WriteEmpty(NetPakWriter writer, byte x, byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt8(0);
			writer.WriteUInt16(0);
		}

		/// <summary>
		/// Despawn any old items in the current despawn region.
		/// </summary>
		/// <returns>True if the region had items to search through.</returns>
		private bool despawnItems()
		{
			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return false;
			}

			if (regions[despawnItems_X, despawnItems_Y].items.Count > 0) // this is still here from when despawns tried until failure
			{
				for (int index = 0; index < regions[despawnItems_X, despawnItems_Y].items.Count; index++)
				{
					if (Time.realtimeSinceStartup - regions[despawnItems_X, despawnItems_Y].items[index].lastDropped > (regions[despawnItems_X, despawnItems_Y].items[index].isDropped ? Provider.modeConfigData.Items.Despawn_Dropped_Time : Provider.modeConfigData.Items.Despawn_Natural_Time))
					{
						uint instanceID = regions[despawnItems_X, despawnItems_Y].items[index].instanceID;
						regions[despawnItems_X, despawnItems_Y].items.RemoveAt(index);

						SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.GatherClientConnections(despawnItems_X, despawnItems_Y, ITEM_REGIONS), despawnItems_X, despawnItems_Y, instanceID, false);

						// Only despawn one item per call to avoid getting rid of every initially spawned item at once.
						break;
					}
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempt to respawn an item in the current respawn region.
		/// </summary>
		/// <returns>True if an item was succesfully respawned.</returns>
		private bool respawnItems()
		{
			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return false;
			}

			if (LevelItems.spawns[respawnItems_X, respawnItems_Y].Count > 0)
			{
				if (Time.realtimeSinceStartup - regions[respawnItems_X, respawnItems_Y].lastRespawn > Provider.modeConfigData.Items.Respawn_Time)
				{
					int currentNumItems = regions[respawnItems_X, respawnItems_Y].items.Count;
					int desiredNumItems = (int) (LevelItems.spawns[respawnItems_X, respawnItems_Y].Count * Provider.modeConfigData.Items.Spawn_Chance);
					bool spawnedAnyItems = false;

					for (int attempt = currentNumItems; attempt < desiredNumItems; attempt++)
					{
						ItemSpawnpoint spawn = LevelItems.spawns[respawnItems_X, respawnItems_Y][Random.Range(0, LevelItems.spawns[respawnItems_X, respawnItems_Y].Count)];
						bool isValid = true;

						if (!SafezoneManager.checkPointValid(spawn.point))
						{
							isValid = false;
						}

						for (ushort index = 0; index < regions[respawnItems_X, respawnItems_Y].items.Count; index++)
						{
							if ((regions[respawnItems_X, respawnItems_Y].items[index].point - spawn.point).sqrMagnitude < 4)
							{
								isValid = false;
								break;
							}
						}

						if (!isValid)
						{
							// Tested point wasn't available
							continue;
						}

						ushort id = LevelItems.getItem(spawn);

						ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
						if (asset != null)
						{
							Item item = new Item(id, EItemOrigin.WORLD);
							Vector3 point = spawn.point;

							bool shouldAllow = true;
							onServerSpawningItemDrop?.Invoke(item, ref point, ref shouldAllow);

							if (!shouldAllow)
							{
								continue;
							}

							ItemData itemData = new ItemData(item, ++instanceCount, spawn.point, false);
							regions[respawnItems_X, respawnItems_Y].items.Add(itemData);

							SendItem.Invoke(ENetReliability.Reliable, Regions.GatherClientConnections(respawnItems_X, respawnItems_Y, ITEM_REGIONS), respawnItems_X, respawnItems_Y, item.id, item.amount, item.quality, item.state, point, itemData.instanceID, false);
						}
						else if (Assets.shouldLoadAnyAssets)
						{
							UnturnedLog.error("Failed to respawn an item with ID " + id + " from type " + LevelItems.tables[spawn.type].name + " [" + spawn.type + "]");
						}

						spawnedAnyItems = true;
					}

					if (spawnedAnyItems)
					{
						regions[respawnItems_X, respawnItems_Y].lastRespawn = Time.realtimeSinceStartup;
						return true;
					}
				}
			}

			return false;
		}

		private void generateItems(byte x, byte y)
		{
			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return;
			}

			List<ItemData> items = new List<ItemData>();

			if (LevelItems.spawns[x, y].Count > 0)
			{
				List<ItemSpawnpoint> valid = new List<ItemSpawnpoint>();
				for (int index = 0; index < LevelItems.spawns[x, y].Count; index++)
				{
					ItemSpawnpoint isp = LevelItems.spawns[x, y][index];

					if (SafezoneManager.checkPointValid(isp.point))
					{
						valid.Add(isp);
					}
				}

				while (items.Count < LevelItems.spawns[x, y].Count * Provider.modeConfigData.Items.Spawn_Chance && valid.Count > 0)
				{
					int index = Random.Range(0, valid.Count);

					ItemSpawnpoint spawn = valid[index];
					valid.RemoveAt(index);

					ushort id = LevelItems.getItem(spawn);

					ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
					if (asset != null)
					{
						Item item = new Item(id, EItemOrigin.WORLD);
						Vector3 point = spawn.point;

						bool shouldAllow = true;
						onServerSpawningItemDrop?.Invoke(item, ref point, ref shouldAllow);

						if (shouldAllow)
						{
							items.Add(new ItemData(item, ++instanceCount, point, false));
						}
					}
					else if (Assets.shouldLoadAnyAssets)
					{
						UnturnedLog.error("Failed to generate an item with ID " + id + " from type " + LevelItems.tables[spawn.type].name + " [" + spawn.type + "]");
					}
				}
			}

			for (int index = 0; index < regions[x, y].items.Count; index++)
			{
				if (regions[x, y].items[index].isDropped)
				{
					items.Add(regions[x, y].items[index]);
				}
			}

			regions[x, y].items = items;
		}

		/// <summary>
		/// Not ideal, but there was a problem because onLevelLoaded was not resetting these after disconnecting.
		/// </summary>
		internal static void ClearNetworkStuff()
		{
			pendingInstantiations = new List<ItemInstantiationParameters>();
			instantiationsToInsert = new List<ItemInstantiationParameters>();
			regionsPendingDestroy = new List<ItemRegion>();
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				regions = new ItemRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						regions[x, y] = new ItemRegion();
					}
				}
				clampedItems = new List<InteractableItem>();
				instanceCount = 0;

				clampItemIndex = 0;
				despawnItems_X = 0;
				despawnItems_Y = 0;
				respawnItems_X = 0;
				respawnItems_Y = 0;

				if (Dedicator.IsDedicatedServer)
				{
					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							generateItems(x, y);
						}
					}
				}
			}
		}

		private void onRegionActivated(byte x, byte y)
		{
			if (regions != null && regions[x, y] != null)
			{
				for (int index = 0; index < regions[x, y].drops.Count; index++)
				{
					ItemDrop drop = regions[x, y].drops[index];

					if (drop == null || drop.interactableItem == null)
					{
						continue;
					}

					Rigidbody rb = drop.interactableItem.GetComponent<Rigidbody>();

					if (rb == null)
					{
						continue;
					}

					rb.useGravity = true;
					rb.isKinematic = false;
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
						if (player.channel.IsLocalPlayer)
						{
							if (regions[x, y].isNetworked && !Regions.checkArea(x, y, new_x, new_y, ITEM_REGIONS))
							{
								if (regions[x, y].drops.Count > 0)
								{
									// Defer cleanup.
									regions[x, y].isPendingDestroy = true;
									regionsPendingDestroy.Add(regions[x, y]);
								}
								CancelInstantiationsInRegion(x, y);

								regions[x, y].isNetworked = false;
							}
						}

						if (Provider.isServer)
						{
							if (player.movement.loadedRegions[x, y].isItemsLoaded && !Regions.checkArea(x, y, new_x, new_y, ITEM_REGIONS))
							{
								player.movement.loadedRegions[x, y].isItemsLoaded = false;
							}
						}
					}
				}
			}

			if (step == 5)
			{
				if (Provider.isServer)
				{
					if (Regions.checkSafe(new_x, new_y))
					{
						Vector3 playerPosition = player.transform.position;
						for (int x = new_x - ITEM_REGIONS; x <= new_x + ITEM_REGIONS; x++)
						{
							for (int y = new_y - ITEM_REGIONS; y <= new_y + ITEM_REGIONS; y++)
							{
								if (Regions.checkSafe((byte) x, (byte) y) && !player.movement.loadedRegions[x, y].isItemsLoaded)
								{
									if (player.channel.IsLocalPlayer)
									{
										generateItems((byte) x, (byte) y);
									}

									player.movement.loadedRegions[x, y].isItemsLoaded = true;

									float sortOrder = Regions.HorizontalDistanceFromCenterSquared(x, y, playerPosition);
									if (Dedicator.IsDedicatedServer)
									{
										askItems(player.channel.owner.transportConnection, (byte) x, (byte) y, sortOrder);
									}
									else
									{
										// Immediately finish any deferred cleanup.
										DestroyAllInRegion(regions[x, y]);

										regions[x, y].isNetworked = true;

										if (regions[x, y].items.Count > 0)
										{
											instantiationsToInsert.Clear();
											foreach (ItemData data in regions[x, y].items)
											{
												ItemInstantiationParameters instantiation = new ItemInstantiationParameters();
												instantiation.region_x = (byte) x;
												instantiation.region_y = (byte) y;
												instantiation.sortOrder = sortOrder;
												instantiation.assetId = data.item.id;
												instantiation.amount = data.item.amount;
												instantiation.quality = data.item.quality;
												instantiation.state = data.item.state;
												instantiation.point = data.point;
												instantiation.instanceID = data.instanceID;
												instantiation.sortOrder = sortOrder;
												instantiationsToInsert.Add(instantiation);
											}
											pendingInstantiations.InsertRange(pendingInstantiations.FindInsertionIndex(instantiationsToInsert[0]), instantiationsToInsert);
										}
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

		private static void DestroyAllInRegion(ItemRegion region)
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

		private static void CancelInstantiationsInRegion(byte x, byte y)
		{
			for (int index = pendingInstantiations.Count - 1; index >= 0; --index)
			{
				if (pendingInstantiations[index].region_x == x && pendingInstantiations[index].region_y == y)
				{
					pendingInstantiations.RemoveAt(index);
				}
			}
		}

		private static void CancelInstantiationByInstanceId(uint instanceId)
		{
			for (int index = pendingInstantiations.Count - 1; index >= 0; --index)
			{
				if (pendingInstantiations[index].instanceID == instanceId)
				{
					pendingInstantiations.RemoveAt(index);
					return;
				}
			}
		}

#if !DEDICATED_SERVER
		private System.Diagnostics.Stopwatch instantiationTimer = new System.Diagnostics.Stopwatch();
		/// <summary>
		/// Instantiate at least this many items per frame even if we exceed our time budget.
		/// </summary>
		private const int MIN_INSTANTIATIONS_PER_FRAME = 5;
		private const int MIN_DESTROY_PER_FRAME = 10;
#endif // !DEDICATED_SERVER

		private void Update()
		{
			if (!Provider.isServer)
			{
				if (clampedItems != null && clampedItems.Count > 0)
				{
					if (clampItemIndex >= clampedItems.Count)
					{
						clampItemIndex = 0;
					}

					InteractableItem interactable = clampedItems[clampItemIndex];
					if (interactable != null)
					{
						interactable.clampRange();
						++clampItemIndex;
					}
					else
					{
						clampedItems.RemoveAtFast(clampItemIndex);
					}
				}
			}

#if !DEDICATED_SERVER
			if (Provider.isConnected)
			{
				if (pendingInstantiations != null && pendingInstantiations.Count > 0)
				{
					Profiler.BeginSample("PendingInstantiations");
					instantiationTimer.Restart();
					int instantiationIndex = 0;
					do
					{
						ItemInstantiationParameters instantiation = pendingInstantiations[instantiationIndex];
						spawnItem(instantiation.region_x, instantiation.region_y, instantiation.assetId, instantiation.amount, instantiation.quality, instantiation.state, instantiation.point, instantiation.instanceID, instantiation.shouldPlayEffect);
						++instantiationIndex;
					}
					while (instantiationIndex < pendingInstantiations.Count && (instantiationTimer.ElapsedMilliseconds < 1 || instantiationIndex < MIN_INSTANTIATIONS_PER_FRAME));
					pendingInstantiations.RemoveRange(0, instantiationIndex);
					instantiationTimer.Stop();
					Profiler.EndSample();
				}

				if (regionsPendingDestroy != null && regionsPendingDestroy.Count > 0)
				{
					Profiler.BeginSample("PendingDestroy");
					instantiationTimer.Restart();
					int destroyCount = 0;
					do
					{
						ItemRegion region = regionsPendingDestroy.GetTail();
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
					while (regionsPendingDestroy.Count > 0 && (instantiationTimer.ElapsedMilliseconds < 1 || destroyCount < MIN_DESTROY_PER_FRAME));
					instantiationTimer.Stop();
					Profiler.EndSample();
				}
			}
#endif // !DEDICATED_SERVER

			// Only despawn/respawn on servers,
			// as singleplayer generates batches of items when you enter the area
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (!Level.isLoaded)
			{
				return;
			}

			while (true)
			{
				bool regionHadItems = despawnItems();

				despawnItems_X++;

				if (despawnItems_X >= Regions.WORLD_SIZE)
				{
					despawnItems_X = 0;
					despawnItems_Y++;

					if (despawnItems_Y >= Regions.WORLD_SIZE)
					{
						despawnItems_Y = 0;

						// Spanned the entire world grid,
						// so exit out of the loop to prevent stalls on maps with 0 items.
						break;
					}
				}

				if (regionHadItems)
				{
					// Didn't necessarily despawn any items,
					// but searched through a potentially long list of dropped items so that's enough work for this frame.
					//UnturnedLog.info("Despawned item(s) @ " + despawnItems_X + ", " + despawnItems_Y);
					break;
				}
			}

			while (true)
			{
				bool succesfullyRespawnedAnItem = respawnItems();

				respawnItems_X++;

				if (respawnItems_X >= Regions.WORLD_SIZE)
				{
					respawnItems_X = 0;
					respawnItems_Y++;

					if (respawnItems_Y >= Regions.WORLD_SIZE)
					{
						respawnItems_Y = 0;

						// Spanned the entire world grid,
						// so exit out of the loop to prevent stalls on maps with 0 valid respawns.
						break;
					}
				}

				if (succesfullyRespawnedAnItem)
				{
					// Our work here is done! For this frame anyway...
					//UnturnedLog.info("Respawned item(s) @ " + respawnItems_X + ", " + respawnItems_Y);
					break;
				}
			}
		}

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onLevelLoaded += onLevelLoaded;
			LevelObjects.onRegionActivated += onRegionActivated;
			Player.onPlayerCreated += onPlayerCreated;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			int regionsWithItems = 0;
			int items = 0;
			int regionsWithPhysicalItems = 0;
			int physicalItems = 0;

			foreach (ItemRegion region in regions)
			{
				if (region.items.Count > 0)
				{
					++regionsWithItems;
				}
				items += region.items.Count;

				if (region.drops.Count > 0)
				{
					++regionsWithPhysicalItems;
				}
				physicalItems += region.drops.Count;
			}

			results.Add($"Item regions: {regionsWithItems}");
			results.Add($"Dropped items: {items}");
			results.Add($"Item regions with physical items: {regionsWithPhysicalItems}");
			results.Add($"Dropped items with physics: {physicalItems}");
		}
	}
}
