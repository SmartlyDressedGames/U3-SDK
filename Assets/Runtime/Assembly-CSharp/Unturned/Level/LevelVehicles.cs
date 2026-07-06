////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelVehicles
	{
		public static readonly byte SAVEDATA_VERSION = 4;

		private static Transform _models;
		[System.Obsolete("Was the parent of all vehicles in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Vehicles";
#pragma warning disable
					_models.parent = Level.spawns;
#pragma warning restore
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelVehicles.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static List<VehicleTable> _tables;
		public static List<VehicleTable> tables => _tables;

		private static List<VehicleSpawnpoint> _spawns;
		public static List<VehicleSpawnpoint> spawns => _spawns;

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

			tables.Add(new VehicleTable(name));
		}

		public static void removeTable()
		{
			tables.RemoveAt(EditorSpawns.selectedVehicle);

			List<VehicleSpawnpoint> points = new List<VehicleSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				VehicleSpawnpoint spawn = spawns[index];

				if (spawn.type == EditorSpawns.selectedVehicle)
				{
					GameObject.Destroy(spawn.node.gameObject);
				}
				else
				{
					if (spawn.type > EditorSpawns.selectedVehicle)
					{
						spawn.type--;
					}

					points.Add(spawn);
				}
			}

			_spawns = points;

			EditorSpawns.selectedVehicle = 0;

			if (EditorSpawns.selectedVehicle < tables.Count)
			{
				EditorSpawns.vehicleSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedVehicle].color;
			}
		}

		public static void addSpawn(Vector3 point, float angle)
		{
			if (EditorSpawns.selectedVehicle >= tables.Count)
			{
				return;
			}

			spawns.Add(new VehicleSpawnpoint(EditorSpawns.selectedVehicle, point, angle));
		}

		public static void removeSpawn(Vector3 point, float radius)
		{
			radius *= radius;

			List<VehicleSpawnpoint> points = new List<VehicleSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				VehicleSpawnpoint spawn = spawns[index];

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

		/// <summary>
		/// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
		/// vehicle spawner requires to properly set paint color.
		/// </summary>
		public static Asset GetRandomAssetForSpawnpoint(VehicleSpawnpoint spawnpoint)
		{
			return tables[spawnpoint.type].GetRandomAsset();
		}

		public static void load()
		{
			if (Level.isEditor || Provider.isServer)
			{
				_tables = new List<VehicleTable>();
				_spawns = new List<VehicleSpawnpoint>();

				if (ReadWrite.fileExists(Level.info.path + "/Spawns/Vehicles.dat", false, false))
				{
					River river = new River(Level.info.path + "/Spawns/Vehicles.dat", false);
					byte version = river.readByte();

					if (version > 1 && version < 3)
					{
						river.readSteamID();
					}

					byte tableCount = river.readByte();
					for (byte tableIndex = 0; tableIndex < tableCount; tableIndex++)
					{
						Color color = river.readColor();
						string tableName = river.readString();

						ushort tableID;
						if (version > 3)
						{
							tableID = river.readUInt16();
						}
						else
						{
							tableID = 0;
						}

						List<VehicleTier> tiers = new List<VehicleTier>();

						byte tierCount = river.readByte();
						for (byte tierIndex = 0; tierIndex < tierCount; tierIndex++)
						{
							string tierName = river.readString();
							float chance = river.readSingle();

							List<VehicleSpawn> vehicles = new List<VehicleSpawn>();

							byte spawnCount = river.readByte();
							for (byte spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
							{
								ushort vehicle = river.readUInt16();

								vehicles.Add(new VehicleSpawn(vehicle));
							}

							tiers.Add(new VehicleTier(vehicles, tierName, chance));
						}

						VehicleTable newTable = new VehicleTable(tiers, color, tableName, tableID);
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
							Asset test = SpawnTableTool.Resolve(tableID, EAssetType.VEHICLE, newTable.OnGetSpawnTableValidationErrorContext);
							if (test == null && Assets.shouldLoadAnyAssets)
							{
								Assets.reportError(Level.info.name + " vehicle table \"" + tableName + "\" references invalid spawn table " + tableID + "!");
							}
						}
					}

					ushort pointCount = river.readUInt16();
					for (int index = 0; index < pointCount; index++)
					{
						byte type = river.readByte();
						Vector3 point = river.readSingleVector3();
						float angle = river.readByte() * 2;

						spawns.Add(new VehicleSpawnpoint(type, point, angle));
					}

					river.closeRiver();
				}
			}
		}

		public static void save()
		{
			River river = new River(Level.info.path + "/Spawns/Vehicles.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeByte((byte) tables.Count);
			for (byte tableIndex = 0; tableIndex < tables.Count; tableIndex++)
			{
				VehicleTable table = tables[tableIndex];

				river.writeColor(table.color);
				river.writeString(table.name);
				river.writeUInt16(table.tableID);

				river.writeByte((byte) table.tiers.Count);
				for (byte tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
				{
					VehicleTier tier = table.tiers[tierIndex];

					river.writeString(tier.name);
					river.writeSingle(tier.chance);

					river.writeByte((byte) tier.table.Count);
					for (byte spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
					{
						VehicleSpawn vehicle = tier.table[spawnIndex];

						river.writeUInt16(vehicle.vehicle);
					}
				}
			}

			river.writeUInt16((ushort) spawns.Count);
			for (int index = 0; index < spawns.Count; index++)
			{
				VehicleSpawnpoint spawn = spawns[index];

				river.writeByte(spawn.type);
				river.writeSingleVector3(spawn.point);
				river.writeByte(MeasurementTool.angleToByte(spawn.angle));
			}

			river.closeRiver();
		}

		#region Obsolete
		[System.Obsolete("GetRandomAssetForSpawnpoint should be used instead because it properly supports guids in spawn assets.")]
		public static ushort getVehicle(VehicleSpawnpoint spawn)
		{
			return getVehicle(spawn.type);
		}

		[System.Obsolete("GetRandomAssetForSpawnpoint should be used instead because it properly supports guids in spawn assets.")]
		public static ushort getVehicle(byte type)
		{
			return tables[type].getVehicle();
		}
		#endregion Obsolete
	}
}
