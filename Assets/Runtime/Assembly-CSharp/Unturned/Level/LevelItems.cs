////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelItems
	{
		public static readonly byte SAVEDATA_VERSION = 4;

		private static Transform _models;
		[System.Obsolete("Was the parent of all items in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Items";
#pragma warning disable
					_models.parent = Level.spawns;
#pragma warning restore
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;
				}

				return _models;
			}
		}

		private static List<ItemTable> _tables;
		public static List<ItemTable> tables => _tables;

		private static List<ItemSpawnpoint>[,] _spawns;
		public static List<ItemSpawnpoint>[,] spawns => _spawns;

		public static void setEnabled(bool isEnabled)
		{
			if (spawns == null)
			{
				return;
			}

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					for (int index = 0; index < spawns[x, y].Count; index++)
					{
						spawns[x, y][index].setEnabled(isEnabled);
					}
				}
			}
		}

		public static void addTable(string name)
		{
			if (tables.Count == byte.MaxValue)
			{
				return;
			}

			tables.Add(new ItemTable(name));
		}

		public static void removeTable()
		{
			tables.RemoveAt(EditorSpawns.selectedItem);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<ItemSpawnpoint> points = new List<ItemSpawnpoint>();
					for (int index = 0; index < spawns[x, y].Count; index++)
					{
						ItemSpawnpoint spawn = spawns[x, y][index];

						if (spawn.type == EditorSpawns.selectedItem)
						{
							GameObject.Destroy(spawn.node.gameObject);
						}
						else
						{
							if (spawn.type > EditorSpawns.selectedItem)
							{
								spawn.type--;
							}

							points.Add(spawn);
						}
					}

					_spawns[x, y] = points;
				}
			}

			for (int index = 0; index < LevelZombies.tables.Count; index++)
			{
				ZombieTable table = LevelZombies.tables[index];

				if (table.lootIndex > EditorSpawns.selectedItem)
				{
					table.lootIndex--;
				}
			}

			EditorSpawns.selectedItem = 0;

			if (EditorSpawns.selectedItem < tables.Count)
			{
				EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedItem].color;
			}
		}

		public static void addSpawn(Vector3 point)
		{
			byte x;
			byte y;

			if (!Regions.tryGetCoordinate(point, out x, out y))
			{
				return;
			}

			if (EditorSpawns.selectedItem >= tables.Count)
			{
				return;
			}

			spawns[x, y].Add(new ItemSpawnpoint(EditorSpawns.selectedItem, point));
		}

		public static void removeSpawn(Vector3 point, float radius)
		{
			radius *= radius;

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<ItemSpawnpoint> points = new List<ItemSpawnpoint>();
					for (int index = 0; index < spawns[x, y].Count; index++)
					{
						ItemSpawnpoint spawn = spawns[x, y][index];

						if ((spawn.point - point).sqrMagnitude < radius)
						{
							GameObject.Destroy(spawn.node.gameObject);
						}
						else
						{
							points.Add(spawn);
						}
					}

					_spawns[x, y] = points;
				}
			}
		}

		public static ushort getItem(ItemSpawnpoint spawn)
		{
			return getItem(spawn.type);
		}

		public static ushort getItem(byte type)
		{
			return tables[type].getItem();
		}

		public static void load()
		{
			if (Level.isEditor || Provider.isServer)
			{
				_tables = new List<ItemTable>();
				_spawns = new List<ItemSpawnpoint>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];

				if (ReadWrite.fileExists(Level.info.path + "/Spawns/Items.dat", false, false))
				{
					Block block = ReadWrite.readBlock(Level.info.path + "/Spawns/Items.dat", false, false, 0);
					byte version = block.readByte();

					if (version > 1 && version < 3)
					{
						block.readSteamID();
					}

					byte tableCount = block.readByte();
					for (byte tableIndex = 0; tableIndex < tableCount; tableIndex++)
					{
						Color color = block.readColor();
						string tableName = block.readString();

						ushort tableID;
						if (version > 3)
						{
							tableID = block.readUInt16();
						}
						else
						{
							tableID = 0;
						}

						List<ItemTier> tiers = new List<ItemTier>();

						byte tierCount = block.readByte();
						for (byte tierIndex = 0; tierIndex < tierCount; tierIndex++)
						{
							string tierName = block.readString();
							float chance = block.readSingle();

							List<ItemSpawn> items = new List<ItemSpawn>();

							byte spawnCount = block.readByte();
							for (byte spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
							{
								ushort item = block.readUInt16();

								ItemAsset asset = Assets.find(EAssetType.ITEM, item) as ItemAsset;

								if (asset == null || asset.isPro)
								{
									continue;
								}

								items.Add(new ItemSpawn(item));
							}

							if (items.Count > 0)
							{
								tiers.Add(new ItemTier(items, tierName, chance));
							}
						}

						ItemTable newTable = new ItemTable(tiers, color, tableName, tableID);
						tables.Add(newTable);

						if (!Level.isEditor)
						{
							newTable.buildTable();
						}

						if (newTable.tableID != 0)
						{
							// Nelson 2025-03-10: Previously, this logged a warning if the returned ID was zero. This is
							// problematic now when (for example) referencing redirector asset that points to an asset
							// without a legacy ID.
							Asset test = SpawnTableTool.Resolve(tableID, EAssetType.ITEM, newTable.OnGetSpawnTableValidationErrorContext);
							if (test == null && Assets.shouldLoadAnyAssets)
							{
								Assets.reportError(Level.info.name + " item table \"" + tableName + "\" references invalid spawn table " + tableID + "!");
							}
						}
					}
				}

				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						spawns[x, y] = new List<ItemSpawnpoint>();
					}
				}

				if (ReadWrite.fileExists(Level.info.path + "/Spawns/Jars.dat", false, false))
				{
					River river = new River(Level.info.path + "/Spawns/Jars.dat", false);
					byte version = river.readByte();

					if (version > 0)
					{
						for (byte x = 0; x < Regions.WORLD_SIZE; x++)
						{
							for (byte y = 0; y < Regions.WORLD_SIZE; y++)
							{
								ushort count = river.readUInt16();
								for (ushort index = 0; index < count; index++)
								{
									byte type = river.readByte();
									Vector3 point = river.readSingleVector3();

									spawns[x, y].Add(new ItemSpawnpoint(type, point));
								}
							}
						}
					}

					river.closeRiver();
				}
				else
				{
					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							spawns[x, y] = new List<ItemSpawnpoint>();

							if (ReadWrite.fileExists(Level.info.path + "/Spawns/Items_" + x + "_" + y + ".dat", false, false))
							{
								River river = new River(Level.info.path + "/Spawns/Items_" + x + "_" + y + ".dat", false);
								byte version = river.readByte();

								if (version > 0)
								{
									ushort count = river.readUInt16();
									for (ushort index = 0; index < count; index++)
									{
										byte type = river.readByte();
										Vector3 point = river.readSingleVector3();

										spawns[x, y].Add(new ItemSpawnpoint(type, point));
									}
								}

								river.closeRiver();

								//							Block block = ReadWrite.readBlock("/Maps/"+Level.map+"/Spawns/Items_" + x + "_" + y + ".dat", false, 1);
								//
								//							ushort itemCount = block.readUInt16();
								//							for(ushort itemIndex = 0; itemIndex < itemCount; itemIndex ++)
								//							{
								//								byte type = block.readByte();
								//								Vector3 point = block.readSingleVector3();
								//
								//								spawns[x, y].Add(new ItemSpawnpoint(type, point));
								//							}
							}
						}
					}
				}
			}

#if LOOTDEBUG
			Asset[] itemAssets = Assets.find(EAssetType.Item);

			for(int assetIndex = 0; assetIndex < itemAssets.Length; assetIndex ++)
			{
				ItemAsset itemAsset = (ItemAsset) itemAssets[assetIndex];
				bool hasSpawn = false;

				for(int tableIndex = 0; tableIndex < tables.Count; tableIndex ++)
				{
					ItemTable itemTable = tables[tableIndex];

					for(int tierIndex = 0; tierIndex < itemTable.tiers.Count; tierIndex ++)
					{
						ItemTier itemTier = itemTable.tiers[tierIndex];

						for(int itemIndex = 0; itemIndex < itemTier.table.Count; itemIndex ++)
						{
							ItemSpawn itemSpawn = itemTier.table[itemIndex];

							if(itemAsset.id == itemSpawn.item)
							{
								hasSpawn = true;
								goto search;
							}
						}
					}
				}

				search:
				{
					if(!hasSpawn)
					{
						UnturnedLog.error("Failed to find a table for: " + itemAsset.itemName);
					}
				}
			}
#endif
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION);

			block.writeByte((byte) tables.Count);
			for (byte tableIndex = 0; tableIndex < tables.Count; tableIndex++)
			{
				ItemTable table = tables[tableIndex];

				block.writeColor(table.color);
				block.writeString(table.name);
				block.writeUInt16(table.tableID);

				block.write((byte) table.tiers.Count);
				for (byte tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
				{
					ItemTier tier = table.tiers[tierIndex];

					block.writeString(tier.name);
					block.writeSingle(tier.chance);

					block.writeByte((byte) tier.table.Count);
					for (byte spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
					{
						ItemSpawn item = tier.table[spawnIndex];

						block.writeUInt16(item.item);
					}
				}
			}

			ReadWrite.writeBlock(Level.info.path + "/Spawns/Items.dat", false, false, block);

			River river = new River(Level.info.path + "/Spawns/Jars.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<ItemSpawnpoint> items = spawns[x, y];

					river.writeUInt16((ushort) items.Count);
					for (ushort index = 0; index < items.Count; index++)
					{
						ItemSpawnpoint spawn = items[index];

						river.writeByte(spawn.type);
						river.writeSingleVector3(spawn.point);
					}
				}
			}

			river.closeRiver();
		}
	}
}
