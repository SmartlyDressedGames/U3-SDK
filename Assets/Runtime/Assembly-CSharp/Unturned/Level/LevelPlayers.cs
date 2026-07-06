////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelPlayers
	{
		public static readonly byte SAVEDATA_VERSION = 4;

		private static Transform _models;
		[System.Obsolete("Was the parent of all players in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Players";
#pragma warning disable
					_models.parent = Level.spawns;
#pragma warning restore
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelPlayers.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static List<PlayerSpawnpoint> _spawns;
		public static List<PlayerSpawnpoint> spawns => _spawns;

		public static void setEnabled(bool isEnabled)
		{
			for (int index = 0; index < spawns.Count; index++)
			{
				spawns[index].setEnabled(isEnabled);
			}
		}

		public static bool checkCanBuild(Vector3 point)
		{
			float sqrRadius = 256;
			if (Level.info != null && Level.info.configData != null)
			{
				float radius = Level.info.configData.Prevent_Building_Near_Spawnpoint_Radius;
				sqrRadius = radius * radius;
			}

			for (int index = 0; index < spawns.Count; index++)
			{
				PlayerSpawnpoint spawn = spawns[index];

				if ((spawn.point - point).sqrMagnitude < sqrRadius)
				{
					return false;
				}
			}

			return true;
		}

		public static void addSpawn(Vector3 point, float angle, bool isAlt)
		{
			spawns.Add(new PlayerSpawnpoint(point, angle, isAlt));
		}

		public static void removeSpawn(Vector3 point, float radius)
		{
			radius *= radius;

			List<PlayerSpawnpoint> points = new List<PlayerSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				PlayerSpawnpoint spawn = spawns[index];

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

		public static List<PlayerSpawnpoint> getRegSpawns()
		{
			List<PlayerSpawnpoint> points = new List<PlayerSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				PlayerSpawnpoint spawn = spawns[index];

				if (!spawn.isAlt)
				{
					points.Add(spawn);
				}
			}

			return points;
		}

		public static List<PlayerSpawnpoint> getAltSpawns()
		{
			List<PlayerSpawnpoint> points = new List<PlayerSpawnpoint>();
			for (int index = 0; index < spawns.Count; index++)
			{
				PlayerSpawnpoint spawn = spawns[index];

				if (spawn.isAlt)
				{
					points.Add(spawn);
				}
			}

			return points;
		}

		public static PlayerSpawnpoint getSpawn(bool isAlt)
		{
			List<PlayerSpawnpoint> points = isAlt ? getAltSpawns() : getRegSpawns();
			if (points.Count == 0)
			{
				return new PlayerSpawnpoint(new Vector3(0, 256, 0), 0, isAlt);
			}

			return points[Random.Range(0, points.Count)];
		}

		public static void load()
		{
			_spawns = new List<PlayerSpawnpoint>();

			if (ReadWrite.fileExists(Level.info.path + "/Spawns/Players.dat", false, false))
			{
				River river = new River(Level.info.path + "/Spawns/Players.dat", false);
				byte version = river.readByte();

				if (version > 1 && version < 3)
				{
					river.readSteamID();
				}

				int a = 0;
				int b = 0;

				byte count = river.readByte();
				for (int index = 0; index < count; index++)
				{
					Vector3 point = river.readSingleVector3();
					float angle = river.readByte() * 2;

					bool isAlt = false;
					if (version > 3)
					{
						isAlt = river.readBoolean();
					}

					if (isAlt)
						b++;
					else
						a++;

					addSpawn(point, angle, isAlt);
				}

				river.closeRiver();
			}
		}

		public static void save()
		{
			River river = new River(Level.info.path + "/Spawns/Players.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeByte((byte) spawns.Count);
			for (int index = 0; index < spawns.Count; index++)
			{
				PlayerSpawnpoint spawn = spawns[index];

				river.writeSingleVector3(spawn.point);
				river.writeByte(MeasurementTool.angleToByte(spawn.angle));
				river.writeBoolean(spawn.isAlt);
			}

			river.closeRiver();
		}
	}
}
