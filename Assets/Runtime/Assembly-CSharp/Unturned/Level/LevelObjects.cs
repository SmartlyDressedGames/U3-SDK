////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void RegionActivated(byte x, byte y);

	public class LevelObjects : MonoBehaviour
	{
		/// <summary>
		/// Should objects that failed to load due to missing assets be saved?
		/// If true, a placeholder transform is created and used to save.
		/// If false, objects without assets are zeroed during save. (old default)  
		/// </summary>
		public static CommandLineFlag preserveMissingAssets = new CommandLineFlag(true, "-NoPreserveMissingObjects");

		//private static List<GameObject> batchable = new List<GameObject>();
		//private static List<GameObject> batchableSkybox = new List<GameObject>();

		private const byte SAVEDATA_VERSION_BEFORE_NAMED_VERSIONS = 10;
		private const byte SAVEDATA_VERSION_ADDED_MATERIAL_OVERRIDES = 11;
		private const byte SAVEDATA_VERSION_ADDED_PER_OBJECT_CULLING_OVERRIDES = 12;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_PER_OBJECT_CULLING_OVERRIDES;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

#if BEAUTIFUL
		public static readonly byte OBJECT_REGIONS = 16;
#else
		public static readonly byte OBJECT_REGIONS = 3;
#endif

		private static uint availableInstanceID;

		private static uint generateUniqueInstanceID()
		{
			uint instanceID = availableInstanceID;
			availableInstanceID = instanceID + 1;
			return instanceID;
		}

		private static IReun[] reun;
		public static int step;
		private static int frame;

		private static Transform _models;
		[System.Obsolete("Was the parent of all objects in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Objects";
					_models.parent = Level.level;
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelObjects.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static List<LevelObject>[,] _objects;
		public static List<LevelObject>[,] objects => _objects;

		private static List<LevelBuildableObject>[,] _buildables;
		public static List<LevelBuildableObject>[,] buildables => _buildables;

		internal static Dictionary<uint, LevelObject> instanceIdToObject;

		private static int _total;
		public static int total => _total;

		private static bool[,] _regions;
		public static bool[,] regions => _regions;
		public static float RegularObjectMaxDistance
		{
			get;
			set;
		}
		public static float SkyboxObjectMaxDistance
		{
			get;
			set;
		}
		private static RegionIncrementalVisibilityTracker regionTracker;
		private static RegionIncrementalVisibilityTracker skyboxRegionTracker;
		private static Dictionary<Vector2Int, RegionVisibilityData> regionTrackerData = new Dictionary<Vector2Int, RegionVisibilityData>();

		public static bool IsRegionUpdating(Vector2Int coord)
		{
			return regionTracker.IsRegionUpdating(coord);
		}

		private static bool isHierarchyReady;

		/// <summary>
		/// Hash of Objects.dat
		/// </summary>
		public static byte[] hash
		{
			get;
			private set;
		}

		public static RegionActivated onRegionActivated;

		public static bool shouldInstantlyLoad
		{
			get;
			internal set;
		}

		public static void undo()
		{
			while (frame <= reun.Length - 1)
			{
				if (reun[frame] != null)
				{
					reun[frame].undo();
				}

				if (frame < reun.Length - 1 && reun[frame + 1] != null)
				{
					frame++;

					if (reun[frame].step != step)
					{
						step--;
						break;
					}
				}
				else
				{
					break;
				}
			}
		}

		public static void redo()
		{
			while (frame >= 0)
			{
				if (reun[frame] != null)
				{
					reun[frame].redo();
				}

				if (frame > 0 && reun[frame - 1] != null)
				{
					frame--;

					if (reun[frame].step != step)
					{
						step++;
						break;
					}
				}
				else
				{
					break;
				}
			}
		}

		public static Transform register(IReun newReun)
		{
			if (frame > 0)
			{
				reun = new IReun[reun.Length];
				frame = 0;
			}

			for (int index = reun.Length - 1; index > 0; index--)
			{
				reun[index] = reun[index - 1];
			}

			reun[0] = newReun;
			return reun[0].redo();
		}

		public static void transformObject(Transform select, Vector3 toPosition, Quaternion toRotation, Vector3 toScale, Vector3 fromPosition, Quaternion fromRotation, Vector3 fromScale)
		{
			byte start_x;
			byte start_y;
			if (Regions.tryGetCoordinate(fromPosition, out start_x, out start_y))
			{
				byte end_x;
				byte end_y;
				if (Regions.tryGetCoordinate(toPosition, out end_x, out end_y))
				{
					LevelObject levelObject = null;
					int levelObjectIndex = -1;

					for (int index = 0; index < objects[start_x, start_y].Count; index++)
					{
						if (objects[start_x, start_y][index].transform == select)
						{
							levelObject = objects[start_x, start_y][index];
							levelObjectIndex = index;

							break;
						}
					}

					if (levelObject != null)
					{
						if (start_x != end_x || start_y != end_y)
						{
							objects[start_x, start_y].RemoveAt(levelObjectIndex);
							objects[end_x, end_y].Add(levelObject);
						}

						if (levelObject.transform != null)
						{
							levelObject.transform.position = toPosition;
							levelObject.transform.rotation = toRotation;
							levelObject.transform.localScale = toScale;
						}

#if !DEDICATED_SERVER
						if (levelObject.ownedCullingVolume != null)
						{
							levelObject.ownedCullingVolume.OnLevelObjectMoved();
						}
#endif // !DEDICATED_SERVER

						if (levelObject.skybox != null)
						{
							levelObject.skybox.position = toPosition;
							levelObject.skybox.rotation = toRotation;
							levelObject.skybox.localScale = toScale;
						}
					}
					else
					{
						LevelBuildableObject buildableObject = null;
						int levelBuildableIndex = -1;

						for (int index = 0; index < buildables[start_x, start_y].Count; index++)
						{
							if (buildables[start_x, start_y][index].transform == select)
							{
								buildableObject = buildables[start_x, start_y][index];
								levelBuildableIndex = index;

								break;
							}
						}

						if (buildableObject != null)
						{
							if (start_x != end_x || start_y != end_y)
							{
								buildables[start_x, start_y].RemoveAt(levelBuildableIndex);
								buildables[end_x, end_y].Add(buildableObject);
							}

							if (buildableObject.transform != null)
							{
								buildableObject.transform.position = toPosition;
								buildableObject.transform.rotation = toRotation;
							}
						}
						else
						{
							select.position = fromPosition;
							select.rotation = fromRotation;
							select.localScale = fromScale;
						}
					}
				}
				else
				{
					select.position = fromPosition;
					select.rotation = fromRotation;
					select.localScale = fromScale;
				}
			}
			else
			{
				select.position = fromPosition;
				select.rotation = fromRotation;
				select.localScale = fromScale;
			}
		}

		public static void registerTransformObject(Transform select, Vector3 toPosition, Quaternion toRotation, Vector3 toScale, Vector3 fromPosition, Quaternion fromRotation, Vector3 fromScale)
		{
			register(new ReunObjectTransform(step, select, fromPosition, fromRotation, fromScale, toPosition, toRotation, toScale));
		}

		[System.Obsolete]
		public static SDG.Framework.Devkit.DevkitHierarchyWorldObject addDevkitObject(System.Guid GUID, Vector3 position, Quaternion rotation, Vector3 scale, ELevelObjectPlacementOrigin placementOrigin)
		{
			addObject(position, rotation, scale, 0, GUID, placementOrigin);
			return null;
		}

		[System.Obsolete]
		public static void registerDevkitObject(LevelObject levelObject, out byte x, out byte y)
		{
			if (Regions.tryGetCoordinate(levelObject.transform.position, out x, out y))
			{
				objects[x, y].Add(levelObject);

				if (regions[x, y])
				{
					levelObject.SetIsActiveInRegion(true);
				}
				else
				{
					levelObject.SetIsActiveInRegion(false);
				}
			}
			else
			{
				levelObject.SetIsActiveInRegion(true);
			}
		}

		[System.Obsolete]
		public static void moveDevkitObject(LevelObject levelObject, byte old_x, byte old_y, byte new_x, byte new_y)
		{
			// Old coordinate can be invalid if object was moved from outside the regular level bounds.
			if (Regions.checkSafe(old_x, old_y))
			{
				objects[old_x, old_y].Remove(levelObject);
			}
			objects[new_x, new_y].Add(levelObject);
		}

		[System.Obsolete]
		public static void unregisterDevkitObject(LevelObject levelObject, byte x, byte y)
		{
			// Coordinate can be invalid if object was placed outside the regular level bounds.
			if (Regions.checkSafe(x, y))
			{
				objects[x, y].Remove(levelObject);
			}
		}

		[System.Obsolete]
		public static Transform addObject(Vector3 position, Quaternion rotation, Vector3 scale, ushort id, string name, System.Guid GUID, ELevelObjectPlacementOrigin placementOrigin)
		{
			return addObject(position, rotation, scale, id, GUID, placementOrigin);
		}

		internal static Transform addObject(Vector3 position, Quaternion rotation, Vector3 scale, ushort id, System.Guid GUID, ELevelObjectPlacementOrigin placementOrigin)
		{
			byte x;
			byte y;
			if (Regions.tryGetCoordinate(position, out x, out y))
			{
				LevelObject levelObject = new LevelObject(position, rotation, scale, id, GUID, placementOrigin, generateUniqueInstanceID(), AssetReference<MaterialPaletteAsset>.invalid, -1, NetId.INVALID, true);
#if !FORCE_LOW_LOD
				levelObject.SetIsActiveInRegion(true);
#endif

				objects[x, y].Add(levelObject);
				_total++;

				return levelObject.transform;
			}

			return null;
		}

		public static Transform addBuildable(Vector3 position, Quaternion rotation, ushort id)
		{
			byte x;
			byte y;
			if (Regions.tryGetCoordinate(position, out x, out y))
			{
				LevelBuildableObject buildableObject = new LevelBuildableObject(position, rotation, id);
				buildableObject.enable();

				buildables[x, y].Add(buildableObject);
				_total++;

				return buildableObject.transform;
			}

			return null;
		}

		public static Transform registerAddObject(Vector3 position, Quaternion rotation, Vector3 scale, ObjectAsset objectAsset, ItemAsset itemAsset)
		{
			return register(new ReunObjectAdd(step, objectAsset, itemAsset, position, rotation, scale));
		}

		public static void removeObject(Transform select)
		{
			if (select == null)
			{
				return;
			}

			byte x;
			byte y;
			if (Regions.tryGetCoordinate(select.position, out x, out y))
			{
				for (int index = 0; index < objects[x, y].Count; index++)
				{
					if (objects[x, y][index].transform == select)
					{
						objects[x, y][index].destroy();
						objects[x, y].RemoveAt(index);
						_total--;

						break;
					}
				}
			}
		}

		public static void removeBuildable(Transform select)
		{
			if (select == null)
			{
				return;
			}

			byte x;
			byte y;
			if (Regions.tryGetCoordinate(select.position, out x, out y))
			{
				for (int index = 0; index < buildables[x, y].Count; index++)
				{
					if (buildables[x, y][index].transform == select)
					{
						buildables[x, y][index].destroy();
						buildables[x, y].RemoveAt(index);
						_total--;

						break;
					}
				}
			}
		}

		public static void registerRemoveObject(Transform select)
		{
			if (select == null)
			{
				return;
			}

			byte x;
			byte y;
			if (Regions.tryGetCoordinate(select.position, out x, out y))
			{
				if (select.CompareTag("Barricade") || select.CompareTag("Structure"))
				{
					for (int index = 0; index < buildables[x, y].Count; index++)
					{
						if (buildables[x, y][index].transform == select)
						{
							register(new ReunObjectRemove(step, select, null, buildables[x, y][index].asset, select.position, select.rotation, select.localScale));
							break;
						}
					}
				}
				else
				{
					for (int index = 0; index < objects[x, y].Count; index++)
					{
						if (objects[x, y][index].transform == select)
						{
							register(new ReunObjectRemove(step, select, objects[x, y][index].asset, null, select.position, select.rotation, select.localScale));
							break;
						}
					}
				}
			}
		}

		public static ObjectAsset getAsset(Transform select)
		{
			if (select != null)
			{
				select = select.root;
			}

			if (select != null)
			{
				byte x;
				byte y;
				if (Regions.tryGetCoordinate(select.position, out x, out y))
				{
					for (int index = 0; index < objects[x, y].Count; index++)
					{
						if (objects[x, y][index].transform == select)
						{
							return objects[x, y][index].asset;
						}
					}
				}
			}

			return null;
		}

		public static void getAssetEditor(Transform select, out ObjectAsset objectAsset, out ItemAsset itemAsset)
		{
			objectAsset = null;
			itemAsset = null;

			if (select == null)
			{
				return;
			}

			byte x;
			byte y;
			if (Regions.tryGetCoordinate(select.position, out x, out y))
			{
				if (select.CompareTag("Barricade") || select.CompareTag("Structure"))
				{
					for (int index = 0; index < buildables[x, y].Count; index++)
					{
						if (buildables[x, y][index].transform == select)
						{
							itemAsset = buildables[x, y][index].asset;
							return;
						}
					}
				}
				else
				{
					for (int index = 0; index < objects[x, y].Count; index++)
					{
						if (objects[x, y][index].transform == select)
						{
							objectAsset = objects[x, y][index].asset;
							return;
						}
					}
				}
			}
		}

		internal static LevelObject FindLevelObject(GameObject rootGameObject)
		{
			if (rootGameObject == null)
				return null;

			Transform rootTransform = rootGameObject.transform;
			byte x;
			byte y;
			if (Regions.tryGetCoordinate(rootTransform.position, out x, out y))
			{
				for (int index = 0; index < objects[x, y].Count; index++)
				{
					if (objects[x, y][index].transform == rootTransform)
					{
						return objects[x, y][index];
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Note: refers to per-LevelObject unique ID, not Unity object instance ID.
		/// </summary>
		public static LevelObject FindLevelObjectByInstanceId(uint instanceId)
		{
			if (instanceIdToObject == null)
			{
				// Called too early?
				return null;
			}

			instanceIdToObject.TryGetValue(instanceId, out LevelObject value);
			return value;
		}

		public static void load()
		{
			_objects = new List<LevelObject>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
			_buildables = new List<LevelBuildableObject>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
			instanceIdToObject = new Dictionary<uint, LevelObject>();
			_total = 0;
			_regions = new bool[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
			shouldInstantlyLoad = true;
			isHierarchyReady = false;
			regionTracker = new RegionIncrementalVisibilityTracker();
			skyboxRegionTracker = new RegionIncrementalVisibilityTracker();

			//LevelLighting.reflectionProbes = new List<ReflectionProbe>();

			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					objects[x, y] = new List<LevelObject>();
					buildables[x, y] = new List<LevelBuildableObject>();
				}
			}

			// Zero hash because in most of these coming branches it's not assigned.
			hash = new byte[20];

			if (ReadWrite.fileExists(Level.info.path + "/Level/Objects.dat", false, false))
			{
				River river = new River(Level.info.path + "/Level/Objects.dat", false);
				byte version = river.readByte();

				LegacyObjectRedirectorMap redirectorMap = null;
				if (Level.shouldUseHolidayRedirects)
				{
					redirectorMap = new LegacyObjectRedirectorMap();
				}
				bool useEditorAssetRedirector = Level.isEditor && EditorAssetRedirector.HasRedirects;

				LevelBatching levelBatching = Level.shouldUseLevelBatching ? LevelBatching.Get() : null;

				if (version > 0)
				{
					if (version > 1 && version < 3)
					{
						river.readSteamID();
					}

					if (version > 8)
					{
						availableInstanceID = river.readUInt32();
					}
					else
					{
						availableInstanceID = 1;
					}

					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							ushort count = river.readUInt16();
							for (ushort index = 0; index < count; index++)
							{
								Vector3 point = river.readSingleVector3();
								Quaternion rotation = river.readSingleQuaternion().GetRoundedIfNearlyAxisAligned();
								Vector3 scale;
								if (version > 3)
								{
									scale = river.readSingleVector3().GetRoundedIfNearlyEqualToOne();
								}
								else
								{
									scale = Vector3.one;
								}

								ushort id = river.readUInt16();
								if (version > 5 && version < 10)
								{
									// name
									river.readString();
								}

								System.Guid GUID = System.Guid.Empty;
								if (version > 7)
								{
									GUID = river.readGUID();
								}

								ELevelObjectPlacementOrigin placementOrigin = ELevelObjectPlacementOrigin.MANUAL;
								if (version > 6)
								{
									placementOrigin = (ELevelObjectPlacementOrigin) river.readByte();
								}

								uint instanceID;
								if (version > 8)
								{
									instanceID = river.readUInt32();
								}
								else
								{
									instanceID = generateUniqueInstanceID();
								}

								if (redirectorMap != null)
								{
									ObjectAsset redirect = redirectorMap.redirect(GUID);
									if (redirect == null)
									{
										id = 0;
										GUID = System.Guid.Empty;
									}
									else
									{
										id = redirect.id;
										GUID = redirect.GUID;
									}
								}
								else if (useEditorAssetRedirector)
								{
									ObjectAsset redirect = EditorAssetRedirector.Redirect<ObjectAsset>(GUID);
									if (redirect != null)
									{
										id = redirect.id;
										GUID = redirect.GUID;
									}
								}

								AssetReference<MaterialPaletteAsset> materialPaletteOverride;
								int materialIndexOverride;
								if (version >= SAVEDATA_VERSION_ADDED_MATERIAL_OVERRIDES)
								{
									System.Guid materialPaletteGuid = river.readGUID();
									if (useEditorAssetRedirector)
									{
										MaterialPaletteAsset redirect = EditorAssetRedirector.Redirect<MaterialPaletteAsset>(materialPaletteGuid);
										if (redirect != null)
										{
											materialPaletteGuid = redirect.GUID;
										}
									}
									materialPaletteOverride = new AssetReference<MaterialPaletteAsset>(materialPaletteGuid);
									materialIndexOverride = river.readInt32();
								}
								else
								{
									materialPaletteOverride = default;
									materialIndexOverride = -1;
								}

								bool isOwnedCullingVolumeAllowed;
								if (version >= SAVEDATA_VERSION_ADDED_PER_OBJECT_CULLING_OVERRIDES)
								{
									isOwnedCullingVolumeAllowed = river.readBoolean();
								}
								else
								{
									isOwnedCullingVolumeAllowed = true;
								}

								if (GUID != System.Guid.Empty || id != 0)
								{
									NetId netId = LevelNetIdRegistry.GetRegularObjectNetId(x, y, index);
									LevelObject levelObject = new LevelObject(point, rotation, scale, id, GUID, placementOrigin, instanceID, materialPaletteOverride, materialIndexOverride, netId, isOwnedCullingVolumeAllowed);

									if (levelObject.asset == null && Assets.shouldLoadAnyAssets)
									{
										// https://support.smartlydressedgames.com/hc/en-us/articles/12294374485780
										UnturnedLog.error("Object with no asset in region {0}, {1}: {2} {3}", x, y, id, GUID);
									}

									byte regionAddX = x;
									byte regionAddY = y;
									if (Level.isEditor)
									{
										byte h;
										byte v;

										if (Regions.tryGetCoordinate(point, out h, out v))
										{
											if (h != x || v != y)
											{
												UnturnedLog.error(id + " should be in " + h + ", " + v + " but was in " + x + ", " + y + "!");
												regionAddX = h;
												regionAddY = v;
											}
										}
										else
										{
											UnturnedLog.warn("Object '{0}' ({1}) is outside the map bounds. Position: {2}", levelObject.asset?.name, id, point);
										}
									}

									objects[regionAddX, regionAddY].Add(levelObject);

									levelBatching?.AddLevelObject(levelObject);

									_total++;
								}
							}
						}
					}
				}

				hash = river.getHash();

				river.closeRiver();
			}
			else
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						if (ReadWrite.fileExists(Level.info.path + "/Objects/Objects_" + x + "_" + y + ".dat", false, false))
						{
							River river = new River(Level.info.path + "/Objects/Objects_" + x + "_" + y + ".dat", false);
							byte version = river.readByte();

							if (version > 0)
							{
								ushort count = river.readUInt16();
								for (ushort index = 0; index < count; index++)
								{
									Vector3 point = river.readSingleVector3();
									Quaternion rotation = river.readSingleQuaternion();
									ushort id = river.readUInt16();
									System.Guid GUID = System.Guid.Empty;
									ELevelObjectPlacementOrigin placementOrigin = ELevelObjectPlacementOrigin.MANUAL;

									if (id != 0)
									{
										addObject(point, rotation, Vector3.one, id, GUID, placementOrigin);
									}
								}
							}

							river.closeRiver();
						}
					}
				}
			}

			if (Provider.isServer || Level.isEditor)
			{
				if (ReadWrite.fileExists(Level.info.path + "/Level/Buildables.dat", false, false))
				{
					River river = new River(Level.info.path + "/Level/Buildables.dat", false);
					river.readByte(); // version

					for (byte x = 0; x < Regions.WORLD_SIZE; x++)
					{
						for (byte y = 0; y < Regions.WORLD_SIZE; y++)
						{
							ushort count = river.readUInt16();
							for (ushort index = 0; index < count; index++)
							{
								Vector3 point = river.readSingleVector3();
								Quaternion rotation = river.readSingleQuaternion();
								ushort id = river.readUInt16();

								if (id != 0)
								{
									LevelBuildableObject levelBuildableObject = new LevelBuildableObject(point, rotation, id);

									if (levelBuildableObject.asset == null)
									{
										UnturnedLog.warn($"Missing asset for default buildable object ID {id} in region ({x}, {y})");
									}
									else if (!(levelBuildableObject.asset is ItemBarricadeAsset || levelBuildableObject.asset is ItemStructureAsset))
									{
										UnturnedLog.warn($"Default buildable object ID {id} in region ({x}, {y}) loaded as {levelBuildableObject.asset.name} (this is probably an ID conflict)");
									}

									if (Level.isEditor)
									{
										byte h;
										byte v;

										if (Regions.tryGetCoordinate(point, out h, out v))
										{
											if (h != x || v != y)
											{
												UnturnedLog.error(id + " should be in " + h + ", " + v + " but was in " + x + ", " + y + "!");
												x = h;
												y = v;
											}
										}
										else
										{
											UnturnedLog.warn("Buildable {0} is outside the map bounds. Position: {1}", id, point);
										}
									}

									buildables[x, y].Add(levelBuildableObject);

									_total++;
								}
							}
						}
					}

					river.closeRiver();
				}
			}

			if (Level.isEditor)
			{
				reun = new IReun[256];
				step = 0;
				frame = 0;
			}
		}

		public static void save()
		{
			{
				River river = new River(Level.info.path + "/Level/Objects.dat", false);
				river.writeByte(SAVEDATA_VERSION_NEWEST);

				river.writeUInt32(availableInstanceID);

				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						List<LevelObject> spawns = objects[x, y];

						river.writeUInt16((ushort) spawns.Count);
						for (ushort index = 0; index < spawns.Count; index++)
						{
							LevelObject spawn = spawns[index];

							Transform saveTransform = spawn.transform;
							if (saveTransform == null)
							{
								// Asset failed to load, so fallback to the loaded transform.
								saveTransform = spawn.placeholderTransform;
							}

							if (spawn != null && saveTransform != null && (spawn.GUID != System.Guid.Empty || spawn.id != 0))
							{
								Vector3 savePosition = saveTransform.position;
								if (Regions.clampPositionIntoBounds(ref savePosition))
								{
									UnturnedLog.warn("Object '{0}' ({1}) was clamped into map bounds. Position: {2}", spawn.asset?.name, spawn.id, savePosition);
								}

								river.writeSingleVector3(savePosition);
								river.writeSingleQuaternion(saveTransform.rotation);
								river.writeSingleVector3(saveTransform.localScale);
								river.writeUInt16(spawn.id);
								river.writeGUID(spawn.GUID);
								river.writeByte((byte) spawn.placementOrigin);
								river.writeUInt32(spawn.instanceID);
								river.writeGUID(spawn.customMaterialOverride.GUID);
								river.writeInt32(spawn.materialIndexOverride);
								river.writeBoolean(spawn.isOwnedCullingVolumeAllowed);
							}
							else
							{
								river.writeSingleVector3(Vector3.zero);
								river.writeSingleQuaternion(Quaternion.identity);
								river.writeSingleVector3(Vector3.one);
								river.writeUInt16(0);
								river.writeGUID(System.Guid.Empty);
								river.writeByte(0);
								river.writeUInt32(0);
								river.writeGUID(System.Guid.Empty); // material palette
								river.writeInt32(-1); // material index
								river.writeBoolean(true);

								UnturnedLog.error("Found invalid object at " + x + ", " + y + " with model: " + spawn.transform + " and ID: " + spawn.id);
							}
						}
					}
				}

				river.closeRiver();
			}

			{
				River river = new River(Level.info.path + "/Level/Buildables.dat", false);
				river.writeByte(SAVEDATA_VERSION);

				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						List<LevelBuildableObject> spawns = buildables[x, y];

						river.writeUInt16((ushort) spawns.Count);
						for (ushort index = 0; index < spawns.Count; index++)
						{
							LevelBuildableObject spawn = spawns[index];

							if (spawn != null && spawn.transform != null && spawn.id != 0)
							{
								river.writeSingleVector3(spawn.transform.position);
								river.writeSingleQuaternion(spawn.transform.rotation);
								river.writeUInt16(spawn.id);
							}
							else
							{
								river.writeSingleVector3(Vector3.zero);
								river.writeSingleQuaternion(Quaternion.identity);
								river.writeUInt16(0);

								UnturnedLog.error("Found invalid object at " + x + ", " + y + " with model: " + spawn.transform + " and ID: " + spawn.id);
							}
						}
					}
				}

				river.closeRiver();
			}
		}

		private static void onRegionUpdated(byte old_x, byte old_y, byte new_x, byte new_y)
		{
			bool canIncrementIndex = true;
			onRegionUpdated(null, old_x, old_y, new_x, new_y, 0, ref canIncrementIndex);
		}

		private static void onPlayerTeleported(Player player, Vector3 position)
		{
			shouldInstantlyLoad = true;
			LevelRoads.shouldInstantlyLoad = true;
		}

		private static void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
		{
			if (step != 0)
			{
				return;
			}

#if !FORCE_LOW_LOD
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					if (regions[x, y] && !Regions.checkArea(x, y, new_x, new_y, OBJECT_REGIONS))
					{
						regions[x, y] = false;

						if (Level.isEditor)
						{
							List<LevelBuildableObject> oldBuildables = buildables[x, y];
							for (int index = 0; index < oldBuildables.Count; index++)
							{
								oldBuildables[index].disable();
							}
						}
					}
				}
			}

			if (Regions.checkSafe(new_x, new_y))
			{
				for (int x = new_x - OBJECT_REGIONS; x <= new_x + OBJECT_REGIONS; x++)
				{
					for (int y = new_y - OBJECT_REGIONS; y <= new_y + OBJECT_REGIONS; y++)
					{
						if (Regions.checkSafe((byte) x, (byte) y) && !regions[x, y])
						{
							regions[x, y] = true;

							if (Level.isEditor)
							{
								List<LevelBuildableObject> newBuildables = buildables[x, y];
								for (int index = 0; index < newBuildables.Count; index++)
								{
									newBuildables[index].enable();
								}
							}
						}
					}
				}
			}
#endif

			// Singleplayer has just finished loading objects, so re-evaluate whether position is obstructed.
			if (Level.isLoadingArea && Player.LocalPlayer != null && Provider.isServer)
			{
				Player.LocalPlayer.adjustStanceOrTeleportIfStuck();
			}

			Level.isLoadingArea = false;
		}

		private static void onPlayerCreated(Player player)
		{
			if (player.channel.IsLocalPlayer)
			{
				Player.LocalPlayer.onPlayerTeleported += onPlayerTeleported;
				Player.LocalPlayer.movement.onRegionUpdated += onRegionUpdated;
			}
		}

		private static void handleEditorAreaRegistered(EditorArea area)
		{
			area.onRegionUpdated += onRegionUpdated;
		}

		private static void handleLevelHierarchyReady()
		{
			isHierarchyReady = true;
		}

		/// <summary>
		/// Called by navmesh baking to complete pending object changes that may affect which nav objects are enabled.
		/// </summary>
		internal static void ImmediatelySyncRegionalVisibility()
		{
			regionTrackerData.Clear();
			regionTracker.MaxDistance = RegularObjectMaxDistance;
			regionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				if (!Regions.checkSafe(coord.x, coord.y))
					continue;

				RegionVisibilityData visData = coordDataPair.Value;

				List<LevelObject> regionObjects = objects[coord.x, coord.y];
				foreach (LevelObject obj in regionObjects)
				{
					obj.SetIsActiveInRegion(visData.isInsideMask);
				}

				if (visData.isInsideMask)
				{
					onRegionActivated?.Invoke((byte) coord.x, (byte) coord.y);
				}
			}

			regionTracker.FlushProgress();

			regionTrackerData.Clear();
			skyboxRegionTracker.MaxDistance = SkyboxObjectMaxDistance;
			skyboxRegionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				if (!Regions.checkSafe(coord.x, coord.y))
					continue;

				RegionVisibilityData visData = coordDataPair.Value;

				List<LevelObject> regionObjects = objects[coord.x, coord.y];
				foreach (LevelObject obj in regionObjects)
				{
					obj.SetIsSkyboxActiveInRegion(visData.isInsideMask);
				}
			}

			skyboxRegionTracker.FlushProgress();
		}

		/// <summary>
		/// Stagger regional visibility across multiple frames.
		/// </summary>
		private void tickRegionalVisibility()
		{
			Vector3 cameraPosition = MainCamera.RenderingPosition;
			Vector2Int newCameraCoord = Regions.GetCoordinateVector2Int(cameraPosition);

			shouldInstantlyLoad |= ((newCameraCoord - regionTracker.CameraCoord).sqrMagnitude >= 4);
			regionTracker.CameraCoord = newCameraCoord;
			skyboxRegionTracker.CameraCoord = newCameraCoord;

			if (shouldInstantlyLoad)
			{
				shouldInstantlyLoad = false;
				ImmediatelySyncRegionalVisibility();
				return;
			}

			regionTrackerData.Clear();
			regionTracker.MaxDistance = RegularObjectMaxDistance;
			regionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				if (!Regions.checkSafe(coord.x, coord.y))
				{
					regionTracker.NotifyRegionFinishedUpdating(coord);
					continue;
				}

				RegionVisibilityData visData = coordDataPair.Value;

				List<LevelObject> regionObjects = objects[coord.x, coord.y];
				if (visData.progressIndex < regionObjects.Count)
				{
					regionObjects[visData.progressIndex].SetIsActiveInRegion(visData.isInsideMask);
				}
				else
				{
					regionTracker.NotifyRegionFinishedUpdating(coord);
					onRegionActivated?.Invoke((byte) coord.x, (byte) coord.y);
				}
			}

			regionTrackerData.Clear();
			skyboxRegionTracker.MaxDistance = SkyboxObjectMaxDistance;
			skyboxRegionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				if (!Regions.checkSafe(coord.x, coord.y))
				{
					skyboxRegionTracker.NotifyRegionFinishedUpdating(coord);
					continue;
				}

				RegionVisibilityData visData = coordDataPair.Value;

				List<LevelObject> regionObjects = objects[coord.x, coord.y];
				if (visData.progressIndex < regionObjects.Count)
				{
					regionObjects[visData.progressIndex].SetIsSkyboxActiveInRegion(visData.isInsideMask);
				}
				else
				{
					skyboxRegionTracker.NotifyRegionFinishedUpdating(coord);
				}
			}
		}

		private void Update()
		{
			if (Level.isLoaded == false || Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (regions == null || objects == null)
			{
				// Objects not yet initialized.
				return;
			}

			if (isHierarchyReady == false)
			{
				// Waiting for devkit hierarchy.
				return;
			}

#if !FORCE_LOW_LOD
			if (MainCamera.instance == null)
				return;

			tickRegionalVisibility();
			LevelRoads.UpdateRegionalVisibility();
#endif
		}

		public void Start()
		{
			Player.onPlayerCreated += onPlayerCreated;
			EditorArea.registered += handleEditorAreaRegistered;
			SDG.Framework.Devkit.LevelHierarchy.ready += handleLevelHierarchyReady;
		}
	}

	/// <summary>
	/// Caches uint16 ID to ID redirects.
	/// </summary>
	internal class LegacyObjectRedirectorMap
	{
		public LegacyObjectRedirectorMap()
		{
			redirectedIds = new Dictionary<System.Guid, ObjectAsset>();
		}

		public ObjectAsset redirect(System.Guid originalGUID)
		{
			ObjectAsset redirectedAsset = null;
			if (redirectedIds.TryGetValue(originalGUID, out redirectedAsset) == false)
			{
				ObjectAsset originalAsset = Assets.find(originalGUID) as ObjectAsset;

				if (!Dedicator.IsDedicatedServer)
				{
					ClientAssetIntegrity.QueueRequest(originalGUID, originalAsset, "Object Holiday Redirect (Original)");
				}

				if (originalAsset != null)
				{
					AssetReference<ObjectAsset> redirectedRef = originalAsset.getHolidayRedirect();
					if (redirectedRef.isValid)
					{
						redirectedAsset = redirectedRef.Find();

						if (!Dedicator.IsDedicatedServer)
						{
							ClientAssetIntegrity.QueueRequest(redirectedRef.GUID, redirectedAsset, "Object Holiday Redirect");
						}

						if (redirectedAsset == null)
						{
							if (Assets.shouldLoadAnyAssets)
							{
								UnturnedLog.error("Missing holiday redirect for object {0}", originalAsset);
							}

							// If object is missing on the server then do not kick clients for missing it as well.
							ClientAssetIntegrity.ServerAddKnownMissingAsset(redirectedRef.GUID, "Object Holiday Redirect");
						}
					}
					else
					{
						// Does not have a redirect for this event, so use the original asset.
						redirectedAsset = originalAsset;
					}
				}
				else
				{
					// If object is missing on the server then do not kick clients for missing it as well.
					ClientAssetIntegrity.ServerAddKnownMissingAsset(originalGUID, "Object Holiday Redirect (Original)");
				}

				redirectedIds.Add(originalGUID, redirectedAsset);
			}

			return redirectedAsset;
		}

		private Dictionary<System.Guid, ObjectAsset> redirectedIds;
	}
}
