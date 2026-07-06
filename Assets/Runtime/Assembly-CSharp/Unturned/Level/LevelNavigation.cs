////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void EditorBoundUpdated(byte oldBound, byte newBound);
	public delegate void PlayerBoundUpdated(Player player, byte oldBound, byte newBound);
	public delegate void PlayerNavChanged(PlayerMovement sender, byte oldNav, byte newNav);

	public class LevelNavigation
	{
		public static readonly Vector3 BOUNDS_SIZE = new Vector3(64, 64, 64);

		public static readonly byte SAVEDATA_BOUNDS_VERSION = 1;
		public static readonly byte SAVEDATA_FLAGS_VERSION = 4;
		internal const byte SAVEDATA_VERSION_FLAG_DATA_ADDED_HYPER_AGRO = 4;
		internal const byte SAVEDATA_VERSION_FLAG_DATA_ADDED_MAX_BOSS_COUNT = 5;
		internal const byte SAVEDATA_VERSION_FLAG_DATA_NEWEST = SAVEDATA_VERSION_FLAG_DATA_ADDED_MAX_BOSS_COUNT;
		public static readonly byte SAVEDATA_FLAG_DATA_VERSION = SAVEDATA_VERSION_FLAG_DATA_NEWEST;
		public static readonly byte SAVEDATA_NAVIGATION_VERSION = 1;

		private static Transform _models;
		[System.Obsolete("Was the parent of misc objects in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Navigation";
					_models.parent = Level.level;
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelNavigation.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static List<Flag> flags;

		private static List<Bounds> _bounds;
		public static List<Bounds> bounds => _bounds;
		/// <summary>
		/// Nelson 2026-03-04: the "bounds" list contains navmesh baking bounds + "BOUNDS_SIZE" expansion.
		/// Several parts of the code were looping through ASPFP active graphs and checking whether a
		/// point is within those bounds, so we'll now duplicate some bounds data for that purpose.
		/// </summary>
		private static List<Bounds> nonExpandedNavmeshBounds;

		private static RegionList<int> regionalBounds;
		private static RegionList<int> regionalNonExpandedNavmeshBounds;

		private static bool TryGetRegionalBoundsIndex(RegionList<int> regionalInput, List<Bounds> actualBounds, Vector3 position, out int result)
		{
			List<int> indices = regionalInput.GetList(position);
			if (indices != null)
			{
				foreach (int index in indices)
				{
					if (actualBounds[index].ContainsXZ(position))
					{
						result = index;
						return true;
					}
				}
			}

			result = -1;
			return false;
		}

		public static List<FlagData> flagData
		{
			get;
			private set;
		}

		public static bool tryGetBounds(Vector3 point, out byte bound)
		{
			bound = 255;

			if (regionalBounds != null)
			{
				if (TryGetRegionalBoundsIndex(regionalBounds, bounds, point, out int result))
				{
					bound = (byte) result;
					return true;
				}
			}
			else if (bounds != null)
			{
				for (byte index = 0; index < bounds.Count; index++)
				{
					if (bounds[index].ContainsXZ(point))
					{
						bound = index;

						return true;
					}
				}
			}

			return false;
		}

		public static bool tryGetNavigation(Vector3 point, out byte nav)
		{
			nav = 255;
			UnityEngine.Profiling.Profiler.BeginSample("LevelNavigation.tryGetNavigation");
			bool result = false;
			if (regionalNonExpandedNavmeshBounds != null)
			{
				if (TryGetRegionalBoundsIndex(regionalNonExpandedNavmeshBounds, nonExpandedNavmeshBounds, point, out int temp))
				{
					nav = (byte) temp;
					result = true;
				}
			}
			else if (nonExpandedNavmeshBounds != null)
			{
				for (int index = 0; index < nonExpandedNavmeshBounds.Count; index++)
				{
					if (nonExpandedNavmeshBounds[index].ContainsXZ(point))
					{
						nav = (byte) index;
						result = true;
						break;
					}
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();

			return result;
		}

		public static bool checkSafe(byte bound)
		{
			if (bounds == null)
			{
				return false;
			}

			return bound < bounds.Count;
		}

		public static bool checkSafe(Vector3 point)
		{
			if (bounds == null)
			{
				return false;
			}

			if (regionalBounds != null)
			{
				return TryGetRegionalBoundsIndex(regionalBounds, bounds, point, out int unused);
			}

			for (byte index = 0; index < bounds.Count; index++)
			{
				if (bounds[index].ContainsXZ(point))
				{
					return true;
				}
			}

			return false;
		}

		public static bool checkSafeFakeNav(Vector3 point)
		{
			if (regionalNonExpandedNavmeshBounds != null)
			{
				return TryGetRegionalBoundsIndex(regionalNonExpandedNavmeshBounds, nonExpandedNavmeshBounds, point, out int unused);
			}
			else if (nonExpandedNavmeshBounds != null)
			{
				for (int index = 0; index < nonExpandedNavmeshBounds.Count; index++)
				{
					if (nonExpandedNavmeshBounds[index].ContainsXZ(point))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool checkNavigation(Vector3 point)
		{
			if (regionalNonExpandedNavmeshBounds != null)
			{
				return TryGetRegionalBoundsIndex(regionalNonExpandedNavmeshBounds, nonExpandedNavmeshBounds, point, out int unused);
			}
			else if (nonExpandedNavmeshBounds != null)
			{
				for (int index = 0; index < nonExpandedNavmeshBounds.Count; index++)
				{
					if (nonExpandedNavmeshBounds[index].ContainsXZ(point))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static void setEnabled(bool isEnabled)
		{
			if (flags == null)
			{
				return;
			}

			for (int index = 0; index < flags.Count; index++)
			{
				flags[index].setEnabled(isEnabled);
			}
		}

		public static void updateBounds()
		{
			// Adding this check rather than exception because plugins may have been calling this in unexpected ways.
			if (!Level.isEditor)
			{
				UnturnedLog.error("LevelNavigation.updateBounds should not be called from outside the level editor");
				return;
			}

			_bounds = new List<Bounds>(flags.Count);
			nonExpandedNavmeshBounds = new List<Bounds>(flags.Count);

			foreach (Flag flag in flags)
			{
				Bounds bakingBounds = flag.CalculateBakingBounds();
				nonExpandedNavmeshBounds.Add(bakingBounds);

				Bounds expandedBounds = new Bounds(bakingBounds.center, bakingBounds.size + BOUNDS_SIZE);
				_bounds.Add(expandedBounds);
			}
		}

		public static Transform addFlag(Vector3 point)
		{
			IUnturnedNavmeshInterface navmeshInterface = UnturnedPathfinding.Get().CreateNavmesh();

			FlagData data = new FlagData();
			flags.Add(new Flag(point, navmeshInterface, data));
			flagData.Add(data);
			return flags[flags.Count - 1].model;
		}

		public static void removeFlag(Transform select)
		{
			for (int index = 0; index < flags.Count; index++)
			{
				if (flags[index].model == select)
				{
					for (int step = index + 1; step < flags.Count; step++)
					{
						flags[step].needsNavigationSave = true;
					}

					try
					{
						flags[index].remove();
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "Caught exception removing navmesh:");
					}

					flags.RemoveAt(index);
					flagData.RemoveAt(index);

					break;
				}
			}

			updateBounds();
		}

		public static Flag getFlag(Transform select)
		{
			for (int index = 0; index < flags.Count; index++)
			{
				if (flags[index].model == select)
				{
					return flags[index];
				}
			}

			return null;
		}

		public static void load()
		{
			_bounds = new List<Bounds>();
			nonExpandedNavmeshBounds = new List<Bounds>();
			regionalBounds = null;
			regionalNonExpandedNavmeshBounds = null;
			flagData = new List<FlagData>();

			if (ReadWrite.fileExists(Level.info.path + "/Environment/Bounds.dat", false, false))
			{
				River river = new River(Level.info.path + "/Environment/Bounds.dat", false);
				byte version = river.readByte();

				if (version > 0)
				{
					byte boundsCount = river.readByte();
					for (byte boundIndex = 0; boundIndex < boundsCount; boundIndex++)
					{
						Vector3 center = river.readSingleVector3();
						Vector3 size = river.readSingleVector3();

						bounds.Add(new Bounds(center, size));
						nonExpandedNavmeshBounds.Add(new Bounds(center, size - BOUNDS_SIZE));
					}
				}

				river.closeRiver();
			}

			if (!Level.isEditor)
			{
				regionalBounds = new RegionList<int>(8);
				for (int index = 0; index < _bounds.Count; ++index)
				{
					Bounds bounds = _bounds[index];
					regionalBounds.Add(bounds, index);
				}

				regionalNonExpandedNavmeshBounds = new RegionList<int>(8);
				for (int index = 0; index < nonExpandedNavmeshBounds.Count; ++index)
				{
					Bounds bounds = nonExpandedNavmeshBounds[index];
					regionalNonExpandedNavmeshBounds.Add(bounds, index);
				}
			}

			if (ReadWrite.fileExists(Level.info.path + "/Environment/Flags_Data.dat", false, false))
			{
				River river = new River(Level.info.path + "/Environment/Flags_Data.dat", false);
				byte version = river.readByte();

				if (version > 0)
				{
					byte flagCount = river.readByte();
					for (byte flagIndex = 0; flagIndex < flagCount; flagIndex++)
					{
						string difficultyGUID = river.readString();

						byte maxZombies = 64;
						if (version > 1)
						{
							maxZombies = river.readByte();
						}

						bool spawnZombies = true;
						if (version > 2)
						{
							spawnZombies = river.readBoolean();
						}

						bool hyperAgro = false;
						if (version >= SAVEDATA_VERSION_FLAG_DATA_ADDED_HYPER_AGRO)
						{
							hyperAgro = river.readBoolean();
						}

						int maxBossZombies = -1;
						if (version >= SAVEDATA_VERSION_FLAG_DATA_ADDED_MAX_BOSS_COUNT)
						{
							maxBossZombies = river.readInt32();
						}

						flagData.Add(new FlagData(difficultyGUID, maxZombies, spawnZombies, hyperAgro, maxBossZombies));
					}
				}

				river.closeRiver();
			}

			if (flagData.Count < bounds.Count)
			{
				for (int flagIndex = flagData.Count; flagIndex < bounds.Count; flagIndex++)
				{
					flagData.Add(new FlagData());
				}
			}

			if (Level.isEditor)
			{
				flags = new List<Flag>();

				if (ReadWrite.fileExists(Level.info.path + "/Environment/Flags.dat", false, false))
				{
					River flagRiver = new River(Level.info.path + "/Environment/Flags.dat", false);
					byte flagVersion = flagRiver.readByte();

					if (flagVersion > 2)
					{
						byte flagCount = flagRiver.readByte();

						if (flagData.Count < flagCount)
						{
							UnturnedLog.error($"Navigation flag data count ({flagData.Count}) does not match flags count ({flagCount}) during editor load, fixing");
							for (int flagIndex = flagData.Count; flagIndex < flagCount; ++flagIndex)
							{
								flagData.Add(new FlagData());
							}
						}

						for (byte flagIndex = 0; flagIndex < flagCount; flagIndex++)
						{
							Vector3 point = flagRiver.readSingleVector3();
							float width = flagRiver.readSingle();
							float height = flagRiver.readSingle();

							if (flagVersion < 4)
							{
								width *= 0.5f;
								height *= 0.5f;
							}

							IUnturnedNavmeshInterface navmeshInterface = UnturnedPathfinding.Get().CreateNavmesh();

							if (ReadWrite.fileExists(Level.info.path + "/Environment/Navigation_" + flagIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat", false, false))
							{
								River navigationRiver = new River(Level.info.path + "/Environment/Navigation_" + flagIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat", false);
								byte navigationVersion = navigationRiver.readByte();

								if (navigationVersion > 0)
								{
									navmeshInterface.Deserialize(navigationRiver);
								}

								navigationRiver.closeRiver();
							}

							flags.Add(new Flag(point, width, height, navmeshInterface, flagData[flagIndex]));
						}
					}

					flagRiver.closeRiver();
				}

				if (bounds.Count != flags.Count)
				{
					UnturnedLog.error("Navigation bounds count ({0}) does not match flags count ({1}) during editor load, fixing", bounds.Count, flags.Count);
					updateBounds();
				}
			}
			else if (Provider.isServer)
			{
				int consecutiveNotFoundCount = 0;
				int flagIndex = 0;
				while (consecutiveNotFoundCount < 5)
				{
					string path = Level.info.path + "/Environment/Navigation_" + flagIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat";
					if (ReadWrite.fileExists(path, false, false))
					{
						River river = new River(path, false);
						byte version = river.readByte();

						if (version > 0)
						{
							// Interface isn't held onto because the graph is owned by ASPFP and we don't
							// need to re-serialize in this mode.
							IUnturnedNavmeshInterface navmeshInterface = UnturnedPathfinding.Get().CreateNavmesh();
							navmeshInterface.Deserialize(river);
						}

						river.closeRiver();

						consecutiveNotFoundCount = 0;
					}
					else
					{
						++consecutiveNotFoundCount;
					}
					++flagIndex;
				}
			}
		}

		public static void save()
		{
			if (bounds.Count != flags.Count)
			{
				UnturnedLog.error("Navigation bounds count ({0}) does not match flags count ({1}) during save", bounds.Count, flags.Count);
				updateBounds();
			}

			River flagBounds = new River(Level.info.path + "/Environment/Bounds.dat", false);
			flagBounds.writeByte(SAVEDATA_BOUNDS_VERSION);

			flagBounds.writeByte((byte) bounds.Count);
			for (byte boundIndex = 0; boundIndex < bounds.Count; boundIndex++)
			{
				flagBounds.writeSingleVector3(bounds[boundIndex].center);
				flagBounds.writeSingleVector3(bounds[boundIndex].size);
			}

			flagBounds.closeRiver();

			River flagDatas = new River(Level.info.path + "/Environment/Flags_Data.dat", false);
			flagDatas.writeByte(SAVEDATA_VERSION_FLAG_DATA_NEWEST);

			flagDatas.writeByte((byte) flagData.Count);
			for (byte flagIndex = 0; flagIndex < flagData.Count; flagIndex++)
			{
				flagDatas.writeString(flagData[flagIndex].difficultyGUID);
				flagDatas.writeByte(flagData[flagIndex].maxZombies);
				flagDatas.writeBoolean(flagData[flagIndex].spawnZombies);
				flagDatas.writeBoolean(flagData[flagIndex].hyperAgro);
				flagDatas.writeInt32(flagData[flagIndex].maxBossZombies);
			}

			flagDatas.closeRiver();

			River flagRiver = new River(Level.info.path + "/Environment/Flags.dat", false);
			flagRiver.writeByte(SAVEDATA_FLAGS_VERSION);

			int deleteIndex = flags.Count;
			while (ReadWrite.fileExists(Level.info.path + "/Environment/Navigation_" + deleteIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat", false, false))
			{
				ReadWrite.deleteFile(Level.info.path + "/Environment/Navigation_" + deleteIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat", false, false);

				deleteIndex++;
			}

			flagRiver.writeByte((byte) flags.Count);
			for (byte flagIndex = 0; flagIndex < flags.Count; flagIndex++)
			{
				Flag flag = flags[flagIndex];

				flagRiver.writeSingleVector3(flag.point);
				flagRiver.writeSingle(flag.width);
				flagRiver.writeSingle(flag.height);

				if (!flag.navmeshInterface.ContainsAnyBakedData)
				{
					UnturnedLog.warn($"Navmesh at {flag.point} has not been baked yet");
				}

				if (flag.needsNavigationSave)
				{
					River navigationRiver = new River(Level.info.path + "/Environment/Navigation_" + flagIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".dat", false);
					navigationRiver.writeByte(SAVEDATA_NAVIGATION_VERSION);

					flag.navmeshInterface.Serialize(navigationRiver);

					navigationRiver.closeRiver();
					flag.needsNavigationSave = false;
				}
			}

			flagRiver.closeRiver();
		}
	}
}
