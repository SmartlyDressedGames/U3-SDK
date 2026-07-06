////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Landscapes;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelRoads
	{
		public static readonly byte SAVEDATA_ROADS_VERSION = 2;

		public const byte SAVEDATA_PATHS_VERSION_INITIAL = 5;
		public const byte SAVEDATA_PATHS_VERSION_ADDED_ROAD_ASSET = 6;
		private const byte SAVEDATA_PATHS_VERSION_NEWEST = SAVEDATA_PATHS_VERSION_ADDED_ROAD_ASSET;
		public static readonly byte SAVEDATA_PATHS_VERSION = SAVEDATA_PATHS_VERSION_NEWEST;

		private static Transform _models;
		[System.Obsolete("Was the parent of all roads in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Roads";
					_models.parent = Level.level;
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelRoads.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static RoadMaterial[] _materials;
		public static RoadMaterial[] materials => _materials;

		private static List<Road> roads;
		/// <summary>
		/// Maps region coord to a list of sub-road renderers in that region.
		/// Unlike older "region" features, coord can be outside of the old bounds.
		/// Not used in the editor or the dedicated server.
		/// </summary>
		private static Dictionary<Vector2Int, List<MeshRenderer>> regionSegmentRenderers;
		private static bool isListeningForLandscape;

		/// <summary>
		/// Max draw distance outside editor.
		/// </summary>
		public static float RoadMaxDistance
		{
			get;
			set;
		}
		private static RegionIncrementalVisibilityTracker regionTracker;
		private static Dictionary<Vector2Int, RegionVisibilityData> regionTrackerData = new Dictionary<Vector2Int, RegionVisibilityData>();
		internal static bool shouldInstantlyLoad = false;

		public static void GatherUniqueAssets(List<RoadAsset> assets)
		{
			foreach (Road road in roads)
			{
				RoadAsset asset = road.GetRoadAsset();
				if (asset != null && !assets.Contains(asset))
				{
					assets.Add(asset);
				}
			}
		}

		public static void setEnabled(bool isEnabled)
		{
			for (int index = 0; index < roads.Count; index++)
			{
				roads[index].setEnabled(isEnabled);
			}
		}

		public static Transform addRoad(Vector3 point)
		{
			roads.Add(new Road(EditorRoads.selected, EditorRoads.selectedAssetRef, 0));
			return roads[roads.Count - 1].addVertex(0, point);
		}

		[System.Obsolete]
		public static void removeRoad(Transform select)
		{
			for (int index = 0; index < roads.Count; index++)
			{
				for (int step = 0; step < roads[index].paths.Count; step++)
				{
					if (roads[index].paths[step].vertex == select)
					{
						roads[index].remove();
						roads.RemoveAt(index);

						return;
					}
				}
			}
		}

		public static void removeRoad(Road road)
		{
			for (int index = 0; index < roads.Count; index++)
			{
				if (roads[index] == road)
				{
					roads[index].remove();
					roads.RemoveAt(index);

					return;
				}
			}
		}

		public static RoadMaterial getRoadMaterial(Transform road)
		{
			if (road == null || road.parent == null)
			{
				return null;
			}

			for (int index = 0; index < roads.Count; index++)
			{
				if (roads[index].road == road || roads[index].road == road.parent)
				{
					return materials[roads[index].material];
				}
			}

			return null;
		}

		public static RoadMaterial GetLegacyRoadConfig(int materialIndex)
		{
			return _materials != null && materialIndex >= 0 && materialIndex < _materials.Length ? _materials[materialIndex] : null;
		}

		public static Road FindRoadByRootTransform(Transform transform)
		{
			if (transform == null)
			{
				return null;
			}

			foreach (Road road in roads)
			{
				if (road.road == transform)
				{
					return road;
				}
			}

			return null;
		}

		public static Road getRoad(int index)
		{
			if (index < 0 || index >= roads.Count)
			{
				return null;
			}

			return roads[index];
		}

		public static int getRoadIndex(Road road)
		{
			for (int index = 0; index < roads.Count; index++)
			{
				if (roads[index] == road)
				{
					return index;
				}
			}

			return -1;
		}

		public static Road getRoad(Transform target, out int vertexIndex, out int tangentIndex)
		{
			vertexIndex = -1;
			tangentIndex = -1;

			for (int index = 0; index < roads.Count; index++)
			{
				Road road = roads[index];

				for (int step = 0; step < road.paths.Count; step++)
				{
					RoadPath path = road.paths[step];

					if (path.vertex == target)
					{
						vertexIndex = step;
						return road;
					}
					else if (path.tangents[0] == target)
					{
						vertexIndex = step;
						tangentIndex = 0;
						return road;
					}
					else if (path.tangents[1] == target)
					{
						vertexIndex = step;
						tangentIndex = 1;
						return road;
					}
				}
			}

			return null;
		}

		public static void bakeRoads()
		{
			for (int index = 0; index < roads.Count; index++)
			{
				roads[index].updatePoints();
			}

			buildMeshes();
		}

		public static void load()
		{
			if (ReadWrite.fileExists(Level.info.path + "/Environment/Roads.unity3d", false, false))
			{
				try
				{
					Bundle materialBundle = Bundles.getBundle(Level.info.path + "/Environment/Roads.unity3d", false);
					Texture2D[] materialTextures = materialBundle.loadAll<Texture2D>();
					materialBundle.unload();

					_materials = new RoadMaterial[materialTextures.Length];
					for (int index = 0; index < materials.Length; index++)
					{
						materials[index] = new RoadMaterial(materialTextures[index]);
					}
				}
				catch (System.Exception e)
				{
					UnturnedLog.error("Failed to load level Roads bundle! Most likely needs to be re-built from Unity.");
					UnturnedLog.exception(e);
					_materials = new RoadMaterial[0];
				}
			}
			else
			{
				_materials = new RoadMaterial[0];
			}

			roads = new List<Road>();
			regionSegmentRenderers = new Dictionary<Vector2Int, List<MeshRenderer>>();
			regionTracker = new RegionIncrementalVisibilityTracker();
			shouldInstantlyLoad = true;

			if (ReadWrite.fileExists(Level.info.path + "/Environment/Roads.dat", false, false))
			{
				River river = new River(Level.info.path + "/Environment/Roads.dat", false);
				byte version = river.readByte();

				if (version > 0)
				{
					byte count = river.readByte();
					for (byte index = 0; index < count; index++)
					{
						if (index >= materials.Length)
						{
							break;
						}

						materials[index].width = river.readSingle();
						materials[index].height = river.readSingle();
						materials[index].depth = river.readSingle();

						if (version > 1)
						{
							materials[index].offset = river.readSingle();
						}

						materials[index].isConcrete = river.readBoolean();
					}
				}

				river.closeRiver();
			}

			if (ReadWrite.fileExists(Level.info.path + "/Environment/Paths.dat", false, false))
			{
				River river = new River(Level.info.path + "/Environment/Paths.dat", false);
				byte version = river.readByte();

				if (version > 1)
				{
					ushort count = river.readUInt16();
					for (ushort index = 0; index < count; index++)
					{
						ushort length = river.readUInt16();
						byte material = river.readByte();

						bool isLoop;
						if (version > 2)
						{
							isLoop = river.readBoolean();
						}
						else
						{
							isLoop = false;
						}

						CachingAssetRef roadAssetRef;
						if (version >= SAVEDATA_PATHS_VERSION_ADDED_ROAD_ASSET)
						{
							roadAssetRef = river.readGUID();
						}
						else
						{
							roadAssetRef = CachingAssetRef.Empty;
						}

						List<RoadJoint> joints = new List<RoadJoint>();

						for (ushort step = 0; step < length; step++)
						{
							Vector3 point = river.readSingleVector3();

							Vector3[] tangents = new Vector3[2];
							if (version > 2)
							{
								tangents[0] = river.readSingleVector3();
								tangents[1] = river.readSingleVector3();
							}

							ERoadMode mode;
							if (version > 2)
							{
								mode = (ERoadMode) river.readByte();
							}
							else
							{
								mode = ERoadMode.FREE;
							}

							float offset;
							if (version > 4)
							{
								offset = river.readSingle();
							}
							else
							{
								offset = 0.0f;
							}

							bool ignoreTerrain;
							if (version > 3)
							{
								ignoreTerrain = river.readBoolean();
							}
							else
							{
								ignoreTerrain = false;
							}

							RoadJoint joint = new RoadJoint(point, tangents, mode, offset, ignoreTerrain);
							joints.Add(joint);
						}

						if (version < 3)
						{
							for (ushort step = 0; step < length; step++)
							{
								RoadJoint joint = joints[step];

								if (step == 0)
								{
									joint.setTangent(0, (joint.vertex - joints[step + 1].vertex).normalized * 2.5f);
									joint.setTangent(1, (joints[step + 1].vertex - joint.vertex).normalized * 2.5f);
								}
								else if (step == length - 1)
								{
									joint.setTangent(0, (joints[step - 1].vertex - joint.vertex).normalized * 2.5f);
									joint.setTangent(1, (joint.vertex - joints[step - 1].vertex).normalized * 2.5f);
								}
								else
								{
									joint.setTangent(0, (joints[step - 1].vertex - joint.vertex).normalized * 2.5f);
									joint.setTangent(1, (joints[step + 1].vertex - joint.vertex).normalized * 2.5f);
								}
							}
						}

						roads.Add(new Road(material, roadAssetRef, index, isLoop, joints));
					}
				}
				else if (version > 0)
				{
					byte roadCount = river.readByte();
					for (byte index = 0; index < roadCount; index++)
					{
						byte pointCount = river.readByte();
						byte material = river.readByte();
						List<RoadJoint> joints = new List<RoadJoint>();

						for (byte pointIndex = 0; pointIndex < pointCount; pointIndex++)
						{
							Vector3 point = river.readSingleVector3();
							Vector3[] tangents = new Vector3[2];
							ERoadMode mode = ERoadMode.FREE;

							RoadJoint joint = new RoadJoint(point, tangents, mode, 0.0f, false);
							joints.Add(joint);
						}

						for (byte step = 0; step < pointCount; step++)
						{
							RoadJoint joint = joints[step];

							if (step == 0)
							{
								joint.setTangent(0, (joint.vertex - joints[step + 1].vertex).normalized * 2.5f);
								joint.setTangent(1, (joints[step + 1].vertex - joint.vertex).normalized * 2.5f);
							}
							else if (step == pointCount - 1)
							{
								joint.setTangent(0, (joints[step - 1].vertex - joint.vertex).normalized * 2.5f);
								joint.setTangent(1, (joint.vertex - joints[step - 1].vertex).normalized * 2.5f);
							}
							else
							{
								joint.setTangent(0, (joints[step - 1].vertex - joint.vertex).normalized * 2.5f);
								joint.setTangent(1, (joints[step + 1].vertex - joint.vertex).normalized * 2.5f);
							}
						}

						roads.Add(new Road(material, index, false, joints));
					}
				}

				river.closeRiver();
			}

			if (!isListeningForLandscape)
			{
				isListeningForLandscape = true;
				SDG.Framework.Landscapes.Landscape.loaded += handleLandscapeLoaded;
			}
		}

		public static void save()
		{
			River river = new River(Level.info.path + "/Environment/Roads.dat", false);
			river.writeByte(SAVEDATA_ROADS_VERSION);

			river.writeByte((byte) materials.Length);
			for (byte index = 0; index < materials.Length; index++)
			{
				river.writeSingle(materials[index].width);
				river.writeSingle(materials[index].height);
				river.writeSingle(materials[index].depth);
				river.writeSingle(materials[index].offset);

				river.writeBoolean(materials[index].isConcrete);
			}

			river.closeRiver();

			river = new River(Level.info.path + "/Environment/Paths.dat", false);
			river.writeByte(SAVEDATA_PATHS_VERSION_NEWEST);

			ushort count = 0;
			for (ushort index = 0; index < roads.Count; index++)
			{
				if (roads[index].joints.Count > 1)
				{
					count++;
				}
			}

			river.writeUInt16(count);
			foreach (Road road in roads)
			{
				List<RoadJoint> joints = road.joints;

				if (joints.Count > 1)
				{
					river.writeUInt16((ushort) joints.Count);
					river.writeByte(road.material);
					river.writeBoolean(road.isLoop);
					river.writeGUID(road.RoadAssetRef.Guid);

					for (ushort step = 0; step < joints.Count; step++)
					{
						RoadJoint joint = joints[step];

						river.writeSingleVector3(joint.vertex);
						river.writeSingleVector3(joint.getTangent(0));
						river.writeSingleVector3(joint.getTangent(1));
						river.writeByte((byte) joint.mode);
						river.writeSingle(joint.offset);
						river.writeBoolean(joint.ignoreTerrain);
					}
				}
			}

			river.closeRiver();
		}

		private static void buildMeshes()
		{
			LevelBatching levelBatching = Level.shouldUseLevelBatching && shouldIncludeRoadsInLevelBatching ? LevelBatching.Get() : null;
			foreach (Road road in roads)
			{
				road.buildMesh();
				levelBatching?.AddRoad(road);
			}

			if (!Dedicator.IsDedicatedServer && !Level.isEditor && shouldDeactivateDistantRoads && !GraphicsSettings.WantsCinematicMode)
			{
				PopulateRegionSegmentRenderers();
			}
		}

		private static void PopulateRegionSegmentRenderers()
		{
			int totalRoadSegments = 0;
			regionSegmentRenderers.Clear();
			foreach (Road road in roads)
			{
				foreach (MeshRenderer renderer in road.segmentRenderers)
				{
					renderer.forceRenderingOff = true;
					Vector2Int coord = Regions.GetCoordinateVector2Int(renderer.bounds.center);
					if (!regionSegmentRenderers.TryGetValue(coord, out List<MeshRenderer> regionRenderers))
					{
						regionRenderers = new List<MeshRenderer>();
						regionSegmentRenderers[coord] = regionRenderers;
					}
					regionRenderers.Add(renderer);
				}
				totalRoadSegments += road.segmentRenderers.Count;
			}
			UnturnedLog.info($"Level contains {totalRoadSegments} road segment(s) divided into {regionSegmentRenderers.Count} region(s)");
		}

		private static void handleLandscapeLoaded()
		{
			if (Level.isEditor)
			{
				bakeRoads();
			}
			else
			{
				buildMeshes();
			}
		}

		/// <summary>
		/// Called by navmesh baking to complete pending object changes that may affect which nav objects are enabled.
		/// </summary>
		internal static void ImmediatelySyncRegionalVisibility()
		{
			if (regionTracker == null || regionSegmentRenderers == null || regionSegmentRenderers.Count < 1)
				return;

			regionTrackerData.Clear();
			regionTracker.MaxDistance = RoadMaxDistance;
			regionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				if (!regionSegmentRenderers.TryGetValue(coord, out List<MeshRenderer> segmentRenderers))
				{
					continue;
				}

				RegionVisibilityData visData = coordDataPair.Value;
				foreach (MeshRenderer segmentRenderer in segmentRenderers)
				{
					segmentRenderer.forceRenderingOff = !visData.isInsideMask;
				}
			}

			regionTracker.FlushProgress();
		}

		internal static void UpdateRegionalVisibility()
		{
			if (regionTracker == null || regionSegmentRenderers == null || regionSegmentRenderers.Count < 1)
				return;

			Vector3 cameraPosition = MainCamera.RenderingPosition;
			Vector2Int newCameraCoord = Regions.GetCoordinateVector2Int(cameraPosition);

			shouldInstantlyLoad |= ((newCameraCoord - regionTracker.CameraCoord).sqrMagnitude >= 4);
			regionTracker.CameraCoord = newCameraCoord;

			if (shouldInstantlyLoad)
			{
				shouldInstantlyLoad = false;
				ImmediatelySyncRegionalVisibility();
				return;
			}

			regionTrackerData.Clear();
			regionTracker.MaxDistance = RoadMaxDistance;
			regionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				if (!regionSegmentRenderers.TryGetValue(coord, out List<MeshRenderer> segmentRenderers))
				{
					regionTracker.NotifyRegionFinishedUpdating(coord);
					continue;
				}

				RegionVisibilityData visData = coordDataPair.Value;

				if (visData.progressIndex < segmentRenderers.Count)
				{
					segmentRenderers[visData.progressIndex].forceRenderingOff = !visData.isInsideMask;
				}
				else
				{
					regionTracker.NotifyRegionFinishedUpdating(coord);
				}
			}
		}

		private static CommandLineFlag shouldIncludeRoadsInLevelBatching = new CommandLineFlag(true, "-ExcludeRoadsFromBatching");
		private static CommandLineFlag shouldDeactivateDistantRoads = new CommandLineFlag(true, "-NoManualRoadCulling");
	}
}
