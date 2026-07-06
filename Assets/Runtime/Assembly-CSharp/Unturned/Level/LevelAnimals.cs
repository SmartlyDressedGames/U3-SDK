////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelAnimals
	{
		public static readonly byte SAVEDATA_VERSION = 3;

		private static Transform _models;
		[System.Obsolete("Was the parent of all animals in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Animals";
#pragma warning disable
					_models.parent = Level.spawns;
#pragma warning restore
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelAnimals.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static List<AnimalTable> _tables;
		public static List<AnimalTable> tables => _tables;

		private static List<AnimalSpawnpoint> _spawns;
		public static List<AnimalSpawnpoint> spawns => _spawns;

		public static void setEnabled(bool isEnabled)
		{
			if (spawns == null)
			{
				return;
			}

			for (int index = 0; index < spawns.Count; index++)
			{
				spawns[index].setEnabled(isEnabled);
			}
		}

		public static void addTable(string name)
		{
			if (tables.Count == byte.MaxValue)
			{
				return;
			}

			tables.Add(new AnimalTable(name));
		}

		public static void removeTable()
		{
			tables.RemoveAt(EditorSpawns.selectedAnimal);

			List<AnimalSpawnpoint> points = new List<AnimalSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				AnimalSpawnpoint spawn = spawns[index];

				if (spawn.type == EditorSpawns.selectedAnimal)
				{
					GameObject.Destroy(spawn.node.gameObject);
				}
				else
				{
					if (spawn.type > EditorSpawns.selectedAnimal)
					{
						spawn.type--;
					}

					points.Add(spawn);
				}
			}

			_spawns = points;

			EditorSpawns.selectedAnimal = 0;

			if (EditorSpawns.selectedAnimal < tables.Count)
			{
				EditorSpawns.animalSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedAnimal].color;
			}
		}

		public static void addSpawn(Vector3 point)
		{
			if (EditorSpawns.selectedAnimal >= tables.Count)
			{
				return;
			}

			spawns.Add(new AnimalSpawnpoint(EditorSpawns.selectedAnimal, point));
		}

		public static void removeSpawn(Vector3 point, float radius)
		{
			radius *= radius;
			List<AnimalSpawnpoint> points = new List<AnimalSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				AnimalSpawnpoint spawn = spawns[index];

				if ((spawn.point - point).sqrMagnitude < radius)
				{
					GameObject.Destroy(spawn.node.gameObject);
				}
				else
				{
					points.Add(spawn);
				}
			}

			_spawns = points;
		}

		public static ushort getAnimal(AnimalSpawnpoint spawn)
		{
			return getAnimal(spawn.type);
		}

		public static ushort getAnimal(byte type)
		{
			return tables[type].getAnimal();
		}

		public static void load()
		{
			if (Level.isEditor || Provider.isServer)
			{
				_tables = new List<AnimalTable>();
				_spawns = new List<AnimalSpawnpoint>();

				if (ReadWrite.fileExists(Level.info.path + "/Spawns/Fauna.dat", false, false))
				{
					River river = new River(Level.info.path + "/Spawns/Fauna.dat", false);
					byte version = river.readByte();

					byte tableCount = river.readByte();
					for (byte tableIndex = 0; tableIndex < tableCount; tableIndex++)
					{
						Color color = river.readColor();
						string tableName = river.readString();

						ushort tableID;
						if (version > 2)
						{
							tableID = river.readUInt16();
						}
						else
						{
							tableID = 0;
						}

						List<AnimalTier> tiers = new List<AnimalTier>();

						byte tierCount = river.readByte();
						for (byte tierIndex = 0; tierIndex < tierCount; tierIndex++)
						{
							string tierName = river.readString();
							float chance = river.readSingle();

							List<AnimalSpawn> animals = new List<AnimalSpawn>();

							byte spawnCount = river.readByte();
							for (byte spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
							{
								ushort animal = river.readUInt16();

								animals.Add(new AnimalSpawn(animal));
							}

							tiers.Add(new AnimalTier(animals, tierName, chance));
						}

						AnimalTable newTable = new AnimalTable(tiers, color, tableName, tableID);
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
							Asset test = SpawnTableTool.Resolve(tableID, EAssetType.ANIMAL, newTable.OnGetSpawnTableValidationErrorContext);
							if (test == null && Assets.shouldLoadAnyAssets)
							{
								Assets.reportError(Level.info.name + " animal table \"" + tableName + "\" references invalid spawn table " + tableID + "!");
							}
						}
					}

					ushort pointCount = river.readUInt16();
					for (int index = 0; index < pointCount; index++)
					{
						byte type = river.readByte();
						Vector3 point = river.readSingleVector3();

						spawns.Add(new AnimalSpawnpoint(type, point));
					}

					river.closeRiver();
				}
			}
		}

		public static void save()
		{
			River river = new River(Level.info.path + "/Spawns/Fauna.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeByte((byte) tables.Count);
			for (byte tableIndex = 0; tableIndex < tables.Count; tableIndex++)
			{
				AnimalTable table = tables[tableIndex];

				river.writeColor(table.color);
				river.writeString(table.name);
				river.writeUInt16(table.tableID);

				river.writeByte((byte) table.tiers.Count);
				for (byte tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
				{
					AnimalTier tier = table.tiers[tierIndex];

					river.writeString(tier.name);
					river.writeSingle(tier.chance);

					river.writeByte((byte) tier.table.Count);
					for (byte spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
					{
						AnimalSpawn animal = tier.table[spawnIndex];

						river.writeUInt16(animal.animal);
					}
				}
			}

			river.writeUInt16((ushort) spawns.Count);
			for (int index = 0; index < spawns.Count; index++)
			{
				AnimalSpawnpoint spawn = spawns[index];

				river.writeByte(spawn.type);
				river.writeSingleVector3(spawn.point);
			}

			river.closeRiver();
		}
	}
}
