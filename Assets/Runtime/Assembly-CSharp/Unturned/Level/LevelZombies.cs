////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelZombies
	{
		public const byte SAVEDATA_TABLE_VERSION_OLDER = 9;
		public const byte SAVEDATA_TABLE_VERSION_ADDED_UNIQUE_ID = 10;
		private const byte SAVEDATA_TABLE_VERSION_NEWEST = SAVEDATA_TABLE_VERSION_ADDED_UNIQUE_ID;
		public static readonly byte SAVEDATA_TABLE_VERSION = SAVEDATA_TABLE_VERSION_NEWEST;
		public static readonly byte SAVEDATA_SPAWN_VERSION = 1;

		private static Transform _models;
		[System.Obsolete("Was the parent of all zombies in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Zombies";
#pragma warning disable
					_models.parent = Level.spawns;
#pragma warning restore
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelZombies.models which has been deprecated.");
				}

				return _models;
			}
		}

		//private static List<ZombieTable> _tables;
		public static List<ZombieTable> tables;
		//{
		//	get { return _tables; }
		//}

		private static List<ZombieSpawnpoint>[] _zombies;
		public static List<ZombieSpawnpoint>[] zombies => _zombies;

		private static List<ZombieSpawnpoint>[,] _spawns;
		public static List<ZombieSpawnpoint>[,] spawns => _spawns;

		private static int nextTableUniqueId;

		internal static int GenerateTableUniqueId()
		{
			int id = nextTableUniqueId;
			nextTableUniqueId += 1;
			return id;
		}

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

			tables.Add(new ZombieTable(name));
		}

		public static void removeTable()
		{
			tables.RemoveAt(EditorSpawns.selectedZombie);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<ZombieSpawnpoint> points = new List<ZombieSpawnpoint>();
					for (int index = 0; index < spawns[x, y].Count; index++)
					{
						ZombieSpawnpoint spawn = spawns[x, y][index];

						if (spawn.type == EditorSpawns.selectedZombie)
						{
							GameObject.Destroy(spawn.node.gameObject);
						}
						else
						{
							if (spawn.type > EditorSpawns.selectedZombie)
							{
								spawn.type--;
							}

							points.Add(spawn);
						}
					}

					_spawns[x, y] = points;
				}
			}

			EditorSpawns.selectedZombie = 0;

			if (EditorSpawns.selectedZombie < tables.Count)
			{
				EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedZombie].color;
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

			if (EditorSpawns.selectedZombie >= tables.Count)
			{
				return;
			}

			spawns[x, y].Add(new ZombieSpawnpoint(EditorSpawns.selectedZombie, point));
		}

		public static void removeSpawn(Vector3 point, float radius)
		{
			radius *= radius;

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<ZombieSpawnpoint> points = new List<ZombieSpawnpoint>();
					for (int index = 0; index < spawns[x, y].Count; index++)
					{
						ZombieSpawnpoint spawn = spawns[x, y][index];

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

		/// <returns>-1 if table was not found.</returns>
		public static int FindTableIndexByUniqueId(int uniqueId)
		{
			if (tables != null && uniqueId > 0)
			{
				for (int index = 0; index < tables.Count; ++index)
				{
					ZombieTable table = tables[index];
					if (table != null && table.tableUniqueId == uniqueId)
					{
						return index;
					}
				}
			}

			return -1;
		}

		public static void load()
		{
			tables = new List<ZombieTable>();
			nextTableUniqueId = 1;

			if (ReadWrite.fileExists(Level.info.path + "/Spawns/Zombies.dat", false, false))
			{
				Block block = ReadWrite.readBlock(Level.info.path + "/Spawns/Zombies.dat", false, false, 0);
				byte version = block.readByte();

				if (version > 3 && version < 5)
				{
					block.readSteamID();
				}

				if (version >= SAVEDATA_TABLE_VERSION_ADDED_UNIQUE_ID)
				{
					nextTableUniqueId = block.readInt32();
				}

				if (version > 2)
				{
					byte tableCount = block.readByte();
					for (byte tableIndex = 0; tableIndex < tableCount; tableIndex++)
					{
						int tableUniqueId;
						if (version >= SAVEDATA_TABLE_VERSION_ADDED_UNIQUE_ID)
						{
							tableUniqueId = block.readInt32();
						}
						else
						{
							tableUniqueId = GenerateTableUniqueId();
						}

						Color color = block.readColor();
						string tableName = block.readString();
						bool isMega = block.readBoolean();
						ushort health = block.readUInt16();
						byte damage = block.readByte();
						byte loot = block.readByte();

						ushort tableID;
						if (version > 6)
						{
							tableID = block.readUInt16();
						}
						else
						{
							tableID = 0;
						}

						uint xp;
						if (version > 7)
						{
							xp = block.readUInt32();
						}
						else
						{
							if (isMega)
							{
								xp = 40;
							}
							else
							{
								xp = 3;
							}
						}

						float regen = 10.0f;
						if (version > 5)
						{
							regen = block.readSingle();
						}

						string difficultyGUID = string.Empty;
						if (version > 8)
						{
							difficultyGUID = block.readString();
						}

						ZombieSlot[] slots = new ZombieSlot[4];

						byte slotCount = block.readByte();
						for (byte slotIndex = 0; slotIndex < slotCount; slotIndex++)
						{
							List<ZombieCloth> cloths = new List<ZombieCloth>();

							float chance = block.readSingle();
							byte clothCount = block.readByte();
							for (byte clothIndex = 0; clothIndex < clothCount; clothIndex++)
							{
								ushort item = block.readUInt16();

								ItemAsset asset = Assets.find(EAssetType.ITEM, item) as ItemAsset;

								if (asset == null)
								{
									continue;
								}

								cloths.Add(new ZombieCloth(item));
							}

							slots[slotIndex] = new ZombieSlot(chance, cloths);
						}

						tables.Add(new ZombieTable(slots, color, tableName, isMega, health, damage, loot, tableID, xp, regen, difficultyGUID, tableUniqueId));
					}
				}
				else
				{
					byte tableCount = block.readByte();
					for (byte tableIndex = 0; tableIndex < tableCount; tableIndex++)
					{
						int tableUniqueId = GenerateTableUniqueId();

						Color color = block.readColor();
						string tableName = block.readString();
						byte loot = block.readByte();

						ZombieSlot[] slots = new ZombieSlot[4];

						byte slotCount = block.readByte();
						for (byte slotIndex = 0; slotIndex < slotCount; slotIndex++)
						{
							List<ZombieCloth> cloths = new List<ZombieCloth>();

							float chance = block.readSingle();
							byte clothCount = block.readByte();
							for (byte clothIndex = 0; clothIndex < clothCount; clothIndex++)
							{
								ushort item = block.readUInt16();

								ItemAsset asset = Assets.find(EAssetType.ITEM, item) as ItemAsset;

								if (asset == null)
								{
									continue;
								}

								cloths.Add(new ZombieCloth(item));
							}

							slots[slotIndex] = new ZombieSlot(chance, cloths);
						}

						tables.Add(new ZombieTable(slots, color, tableName, false, 100, 15, loot, 0, 5, 10.0f, string.Empty, tableUniqueId));
					}
				}
			}

			_spawns = new List<ZombieSpawnpoint>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					spawns[x, y] = new List<ZombieSpawnpoint>();
				}
			}

			if (Level.isEditor)
			{
				if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals.dat", false, false))
				{
					River river = new River(Level.info.path + "/Spawns/Animals.dat", false);
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

									spawns[x, y].Add(new ZombieSpawnpoint(type, point));
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
							spawns[x, y] = new List<ZombieSpawnpoint>();

							if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals_" + x + "_" + y + ".dat", false, false))
							{
								River river = new River(Level.info.path + "/Spawns/Animals_" + x + "_" + y + ".dat", false);
								byte version = river.readByte();

								if (version > 0)
								{
									ushort count = river.readUInt16();
									for (ushort index = 0; index < count; index++)
									{
										byte type = river.readByte();
										Vector3 point = river.readSingleVector3();

										spawns[x, y].Add(new ZombieSpawnpoint(type, point));
									}

									river.closeRiver();
								}

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
			else if (Provider.isServer)
			{
				_zombies = new List<ZombieSpawnpoint>[LevelNavigation.bounds.Count];
				for (int index = 0; index < zombies.Length; index++)
				{
					zombies[index] = new List<ZombieSpawnpoint>();
				}

				if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals.dat", false, false))
				{
					River river = new River(Level.info.path + "/Spawns/Animals.dat", false);
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

									byte bound;
									if (LevelNavigation.tryGetBounds(point, out bound))
									{
										if (LevelNavigation.checkNavigation(point))
										{
											zombies[bound].Add(new ZombieSpawnpoint(type, point));
										}
									}
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
							if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals_" + x + "_" + y + ".dat", false, false))
							{
								River river = new River(Level.info.path + "/Spawns/Animals_" + x + "_" + y + ".dat", false);
								byte version = river.readByte();

								if (version > 0)
								{
									ushort count = river.readUInt16();
									for (ushort index = 0; index < count; index++)
									{
										byte type = river.readByte();
										Vector3 point = river.readSingleVector3();

										byte bound;
										if (LevelNavigation.tryGetBounds(point, out bound))
										{
											if (LevelNavigation.checkNavigation(point))
											{
												zombies[bound].Add(new ZombieSpawnpoint(type, point));
											}
										}
									}

									river.closeRiver();
								}
							}
						}
					}
				}
			}
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_TABLE_VERSION);
			block.writeInt32(nextTableUniqueId);

			block.writeByte((byte) tables.Count);
			for (byte tableIndex = 0; tableIndex < tables.Count; tableIndex++)
			{
				ZombieTable table = tables[tableIndex];

				block.writeInt32(table.tableUniqueId);
				block.writeColor(table.color);
				block.writeString(table.name);
				block.writeBoolean(table.isMega);
				block.writeUInt16(table.health);
				block.writeByte(table.damage);
				block.writeByte(table.lootIndex);
				block.writeUInt16(table.lootID);
				block.writeUInt32(table.xp);
				block.writeSingle(table.regen);
				block.writeString(table.difficultyGUID);

				block.write((byte) table.slots.Length);
				for (byte slotIndex = 0; slotIndex < table.slots.Length; slotIndex++)
				{
					ZombieSlot slot = table.slots[slotIndex];

					block.writeSingle(slot.chance);
					block.writeByte((byte) slot.table.Count);
					for (byte clothIndex = 0; clothIndex < slot.table.Count; clothIndex++)
					{
						ZombieCloth cloth = slot.table[clothIndex];

						block.writeUInt16(cloth.item);
					}
				}
			}

			ReadWrite.writeBlock(Level.info.path + "/Spawns/Zombies.dat", false, false, block);

			River river = new River(Level.info.path + "/Spawns/Animals.dat", false);
			river.writeByte(SAVEDATA_SPAWN_VERSION);

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<ZombieSpawnpoint> zombies = spawns[x, y];

					river.writeUInt16((ushort) zombies.Count);
					for (ushort index = 0; index < zombies.Count; index++)
					{
						ZombieSpawnpoint spawn = zombies[index];

						river.writeByte(spawn.type);
						river.writeSingleVector3(spawn.point);
					}
				}
			}

			river.closeRiver();
		}
	}
}
