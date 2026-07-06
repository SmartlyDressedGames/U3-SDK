////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Foliage;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelGround : MonoBehaviour
	{
		private static int _Triplanar_Primary_Size = -1;
		private static float _triplanarPrimarySize = 16;
		public static float triplanarPrimarySize
		{
			get => _triplanarPrimarySize;
			set
			{
				_triplanarPrimarySize = value;
				Shader.SetGlobalFloat(_Triplanar_Primary_Size, triplanarPrimarySize);

				UnturnedLog.info("Set triplanar_primary_size to: " + triplanarPrimarySize);
			}
		}

		private static int _Triplanar_Primary_Weight = -1;
		private static float _triplanarPrimaryWeight = 0.4f;
		public static float triplanarPrimaryWeight
		{
			get => _triplanarPrimaryWeight;
			set
			{
				_triplanarPrimaryWeight = value;
				Shader.SetGlobalFloat(_Triplanar_Primary_Weight, triplanarPrimaryWeight);

				UnturnedLog.info("Set triplanar_primary_weight to: " + triplanarPrimaryWeight);
			}
		}

		private static int _Triplanar_Secondary_Size = -1;
		private static float _triplanarSecondarySize = 64;
		public static float triplanarSecondarySize
		{
			get => _triplanarSecondarySize;
			set
			{
				_triplanarSecondarySize = value;
				Shader.SetGlobalFloat(_Triplanar_Secondary_Size, triplanarSecondarySize);

				UnturnedLog.info("Set triplanar_secondary_size to: " + triplanarSecondarySize);
			}
		}

		private static int _Triplanar_Secondary_Weight = -1;
		private static float _triplanarSecondaryWeight = 0.4f;
		public static float triplanarSecondaryWeight
		{
			get => _triplanarSecondaryWeight;
			set
			{
				_triplanarSecondaryWeight = value;
				Shader.SetGlobalFloat(_Triplanar_Secondary_Weight, triplanarSecondaryWeight);

				UnturnedLog.info("Set triplanar_secondary_weight to: " + triplanarSecondaryWeight);
			}
		}

		private static int _Triplanar_Tertiary_Size = -1;
		private static float _triplanarTertiarySize = 4;
		public static float triplanarTertiarySize
		{
			get => _triplanarTertiarySize;
			set
			{
				_triplanarTertiarySize = value;
				Shader.SetGlobalFloat(_Triplanar_Tertiary_Size, triplanarTertiarySize);

				UnturnedLog.info("Set triplanar_tertiary_size to: " + triplanarTertiarySize);
			}
		}

		private static int _Triplanar_Tertiary_Weight = -1;
		private static float _triplanarTertiaryWeight = 0.2f;
		public static float triplanarTertiaryWeight
		{
			get => _triplanarTertiaryWeight;
			set
			{
				_triplanarTertiaryWeight = value;
				Shader.SetGlobalFloat(_Triplanar_Tertiary_Weight, triplanarTertiaryWeight);

				UnturnedLog.info("Set triplanar_tertiary_weight to: " + triplanarTertiaryWeight);
			}
		}

		private static Collider[] obstructionColliders = new Collider[16];

		private const byte SAVEDATA_TREES_VERSION_ADDED_GUID = 6;
		private const byte SAVEDATA_TREES_VERSION_INT_REGION_COORDS = 7;
		private const byte SAVEDATA_TREES_VERSION_ROTATION_AND_SCALE = 8;
		private const byte SAVEDATA_TREES_VERSION_NEWEST = SAVEDATA_TREES_VERSION_ROTATION_AND_SCALE;
		public static readonly byte SAVEDATA_TREES_VERSION = SAVEDATA_TREES_VERSION_NEWEST;

#if BEAUTIFUL
		public static readonly byte RESOURCE_REGIONS = 16;
#else
		public static readonly byte RESOURCE_REGIONS = 3;
#endif

		public static readonly byte ALPHAMAPS = 2;

		private static float[,,] alphamapHQ;
		private static float[,,] alphamap2HQ;

		/// <summary>
		/// If true then level should convert old terrain.
		/// </summary>
		public static bool hasLegacyDataForConversion;

		/// <summary>
		/// If true, splatmap conversion should use weights as-is.
		/// </summary>
		public static bool doesLegacyDataIncludeSplatmapWeights;

		/// <summary>
		/// Material guids converted by legacy asset bundle hash or texture names.
		/// </summary>
		public static AssetReference<LandscapeMaterialAsset>[] legacyMaterialGuids;

		/// <summary>
		/// Hash of Trees.dat, or zeroed if any assets were missing locally.
		/// Should only be used if level is configured to, as many mod maps are typically missing assets.
		/// </summary>
		public static byte[] treesHash
		{
			get;
			private set;
		}

		private static Transform _models;
		[System.Obsolete("Legacy terrain game object only exists for auto-conversion")]
		public static Transform models => _models;

		private static Transform _models2;
		[System.Obsolete("Legacy terrain game object only exists for auto-conversion")]
		public static Transform models2 => _models2;

		private static List<ResourceSpawnpoint>[,] _trees; // for lack of a better name :/
		[System.Obsolete("Replaced by GetTree* helper methods and GatherAllTrees")]
		public static List<ResourceSpawnpoint>[,] trees => _trees;

		/// <summary>
		/// Nelson 2025-06-10: replacement for _trees. Enables trees outside the "insane" level bounds.
		/// </summary>
		private static RegionDictionary<ResourceSpawnpoint> _regionTrees;

		public static int GetTreeCountInRegion(Vector2Int coord)
		{
			return _regionTrees?.GetListOrNull(coord)?.Count ?? 0;
		}

		public static int GetTreeCountInRegion(byte x, byte y)
		{
			return GetTreeCountInRegion(new Vector2Int(x, y));
		}

		public static List<ResourceSpawnpoint> GetTreesOrNullInRegion(Vector2Int coord)
		{
			return _regionTrees?.GetListOrNull(coord);
		}

		public static List<ResourceSpawnpoint> GetTreesOrNullInRegion(byte x, byte y)
		{
			return GetTreesOrNullInRegion(new Vector2Int(x, y));
		}

		/// <summary>
		/// Append all trees in the level to results list.
		/// </summary>
		public static void GatherAllTrees(List<ResourceSpawnpoint> results)
		{
			_regionTrees?.GatherAllItems(results);
		}

		public static void ForceUpdateSkyboxActive()
		{
			if (_regionTrees == null)
				return;

			foreach (List<ResourceSpawnpoint> trees in _regionTrees.data.Values)
			{
				foreach (ResourceSpawnpoint tree in trees)
				{
					if (tree != null)
					{
						tree.UpdateSkyboxActive();
					}
				}
			}
		}

		private static int _total;
		public static int total => _total;

		public static float RegularTreeMaxDistance
		{
			get;
			set;
		}
		public static float SkyboxTreeMaxDistance
		{
			get;
			set;
		}
		private static RegionIncrementalVisibilityTracker regionTracker;
		private static RegionIncrementalVisibilityTracker skyboxRegionTracker;
		private static Dictionary<Vector2Int, RegionVisibilityData> regionTrackerData = new Dictionary<Vector2Int, RegionVisibilityData>();

		public static bool shouldInstantlyLoad
		{
			get;
			private set;
		}

		private static Terrain _terrain;
		[System.Obsolete("Legacy terrain only exists for auto-conversion")]
		public static Terrain terrain => _terrain;

		private static Terrain _terrain2;
		[System.Obsolete("Legacy terrain only exists for auto-conversion")]
		public static Terrain terrain2 => _terrain2;

		private static TerrainData _data;
		public static TerrainData data => _data;

		private static TerrainData _data2;
		public static TerrainData data2 => _data2;

		internal static ResourceSpawnpoint FindResourceSpawnpointByTransform(Transform transform)
		{
			if (transform != null)
			{
				transform = transform.root;
			}

			TreeRefComponent treeRefComponent = transform?.GetComponent<TreeRefComponent>();
			return treeRefComponent?.owner;
		}

		[System.Obsolete]

		public static Vector3 checkSafe(Vector3 point)
		{
			UndergroundAllowlist.AdjustPosition(ref point, 0.5f, threshold: 1.0f);
			return point;
		}

		[System.Obsolete]
		public static int getMaterialIndex(Vector3 point)
		{
			return 0;
		}

		public static float getHeight(Vector3 point)
		{
			float height;
			SDG.Framework.Landscapes.Landscape.getWorldHeight(point, out height);
			return height;
		}

		public static float getConversionHeight(Vector3 point)
		{
			if (point.x < -Level.size / 2 || point.z < -Level.size / 2 || point.x > Level.size / 2 || point.z > Level.size / 2)
			{
				return _terrain2.SampleHeight(point);
			}
			else
			{
				return Mathf.Max(_terrain.SampleHeight(point), _terrain2.SampleHeight(point));
			}
		}

		public static float getConversionWeight(Vector3 point, int layer)
		{
			if (point.x < -Level.size / 2 || point.z < -Level.size / 2 || point.x > Level.size / 2 || point.z > Level.size / 2 || _terrain2.SampleHeight(point) > _terrain.SampleHeight(point))
			{
				int x = getAlphamap2_X(point);

				if (x < 0 || x >= data2.alphamapWidth)
				{
					return 0;
				}

				int y = getAlphamap2_Y(point);

				if (y < 0 || y >= data2.alphamapWidth)
				{
					return 0;
				}

				return alphamap2HQ[y, x, layer];
			}
			else
			{
				int x = getAlphamap_X(point);

				if (x < 0 || x >= data.alphamapWidth)
				{
					return 0;
				}

				int y = getAlphamap_Y(point);

				if (y < 0 || y >= data.alphamapWidth)
				{
					return 0;
				}

				return alphamapHQ[y, x, layer];
			}
		}

		public static Vector3 getNormal(Vector3 point)
		{
			Vector3 normal;
			SDG.Framework.Landscapes.Landscape.getNormal(point, out normal);
			return normal;
		}

		public static int getAlphamap_X(Vector3 point)
		{
			return (int) ((point.x - _terrain.transform.position.x) / data.size.x * data.alphamapWidth);
		}

		public static int getAlphamap_Y(Vector3 point)
		{
			return (int) ((point.z - _terrain.transform.position.z) / data.size.z * data.alphamapHeight);
		}

		public static int getAlphamap2_X(Vector3 point)
		{
			return (int) ((point.x - _terrain2.transform.position.x) / data2.size.x * data2.alphamapWidth);
		}

		public static int getAlphamap2_Y(Vector3 point)
		{
			return (int) ((point.z - _terrain2.transform.position.z) / data2.size.z * data2.alphamapHeight);
		}

		public static int getHeightmap_X(Vector3 point)
		{
			return (int) ((point.x - _terrain.transform.position.x) / data.size.x * data.heightmapResolution);
		}

		public static int getHeightmap_Y(Vector3 point)
		{
			return (int) ((point.z - _terrain.transform.position.z) / data.size.z * data.heightmapResolution);
		}

		public static int getHeightmap2_X(Vector3 point)
		{
			return (int) ((point.x - _terrain2.transform.position.x) / data2.size.x * data2.heightmapResolution);
		}

		public static int getHeightmap2_Y(Vector3 point)
		{
			return (int) ((point.z - _terrain2.transform.position.z) / data2.size.z * data2.heightmapResolution);
		}

		[System.Obsolete]
		public static void cutFoliage(Vector3 point, float radius = 6)
		{

		}

		protected static void handlePreBakeTile(FoliageBakeSettings bakeSettings, FoliageTile foliageTile)
		{
			if (!bakeSettings.bakeResources)
			{
				return;
			}

			Bounds worldBounds = foliageTile.worldBounds;
			RegionBoundsInt bounds = Regions.GetCoordinateBoundsInt(worldBounds);
			foreach (Vector2Int coord in bounds)
			{
				List<ResourceSpawnpoint> trees = GetTreesOrNullInRegion(coord);
				if (trees == null)
					continue;

				for (int index = trees.Count - 1; index >= 0; index--)
				{
					ResourceSpawnpoint spawn = trees[index];

					if (spawn.isGenerated)
					{
						if (!worldBounds.ContainsXZ(spawn.point))
						{
							continue;
						}

						spawn.destroy();
						trees.RemoveAt(index);
					}
				}

				if (trees.Count < 1)
				{
					_regionTrees.ReleaseListIfEmpty(coord);
				}
			}
		}

		public static void addSpawn(Vector3 point, Quaternion rotation, Vector3 scale, System.Guid guid, bool isGenerated = false)
		{
			ResourceSpawnpoint spawn = new ResourceSpawnpoint(0, 0, guid, point, rotation, scale, isGenerated, NetId.INVALID);
#if !FORCE_LOW_LOD
			spawn.SetIsActiveInRegion(true);
#endif

			Vector2Int coord = Regions.GetCoordinateVector2Int(point);
			List<ResourceSpawnpoint> spawns = _regionTrees.GetOrAddList(coord);
			spawns.Add(spawn);

			if (Regions.IsVector2IntWithinLegacyRange(coord))
			{
				// Backwards compatibility.
				_trees[coord.x, coord.y].Add(spawn);
			}

			_total++;
		}

		[System.Obsolete("Replaced by overload which takes a rotation and scale")]
		public static void addSpawn(Vector3 point, System.Guid guid, bool isGenerated = false)
		{
			addSpawn(point, Quaternion.identity, Vector3.one, guid, isGenerated);
		}

		[System.Obsolete("Replaced by overload which takes GUID rather than legacy ID")]
		public static void addSpawn(Vector3 point, ushort id, bool isGenerated = false)
		{
			ResourceAsset asset = Assets.find(EAssetType.RESOURCE, id) as ResourceAsset;
			if (asset != null)
			{
				addSpawn(point, asset.GUID, isGenerated);
			}
		}

		[System.Obsolete("Replaced by overload which takes ID rather than index.")]
		public static void addSpawn(Vector3 point, byte index, bool isGenerated = false)
		{

		}

		public static void removeSpawn(Vector3 point, float radius)
		{
			float sqrRadius = radius * radius;

			Regions.GetCoordinateBoundsVector2Int(point, radius, out Vector2Int min, out Vector2Int max);
			for (int x = min.x; x <= max.x; ++x)
			{
				for (int y = min.y; y <= max.y; ++y)
				{
					Vector2Int coord = new Vector2Int(x, y);
					List<ResourceSpawnpoint> trees = _regionTrees.GetListOrNull(coord);
					if (trees == null)
						continue;

					for (int index = trees.Count - 1; index >= 0; --index)
					{
						ResourceSpawnpoint tree = trees[index];

						if ((tree.point - point).sqrMagnitude < sqrRadius)
						{
							tree.destroy();
							trees.RemoveAt(index);
						}
					}

					if (trees.IsEmpty())
					{
						_regionTrees.ReleaseListIfEmpty(coord);
					}
				}
			}
		}

		protected static void loadSplatPrototypes()
		{
			string materialsAssetBundlePath = Level.info.path + "/Terrain/Materials.unity3d";
			if (ReadWrite.fileExists(materialsAssetBundlePath, false, false))
			{
				byte[] bytes = ReadWrite.readBytes(materialsAssetBundlePath, false, false);
				byte[] hash = Hash.SHA1(bytes);
				UnturnedLog.info($"Legacy terrain material hash: {Hash.ToCodeString(hash)}");

				byte[] pei = new byte[20] { 0x4F, 0x94, 0x81, 0x0E, 0xAA, 0x4F, 0x2F, 0x17, 0x3C, 0xF1, 0x67, 0x43, 0xEA, 0xB0, 0x84, 0x8E, 0x63, 0x29, 0x58, 0xCF };
				if (Hash.verifyHash(hash, pei))
				{
					UnturnedLog.info("Matched PEI legacy terrain materials hash");
					legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("92cb5a3afd534054a64eb320b50c48de"); // PEI Dirt 1
					legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("22b77c4c51514b0fbb66765eedf1a7f4"); // PEI Farm Wheat 0
					legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("3d7717c2bc074401853b2fdacd9db1ba"); // PEI Grass 0
					legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("9a2de27c10aa41438154105292b2fd4a"); // PEI Gravel 0
					legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b"); // Russia Road 0
					legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("a9f5c606fe0d433ab167fbe8e3273055"); // PEI Sand 1
					legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("e25f0351181f4ad1a9c0dc31d2fedade"); // Yukon Snow
					legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("2e329671e8c9432eae580f7807acc021"); // PEI Stone 1
					return;
				}

				byte[] russia = new byte[20] { 0xB0, 0x7C, 0xE5, 0x26, 0x3D, 0xB5, 0xEA, 0xDE, 0xF8, 0x4F, 0x2B, 0x14, 0xD8, 0x09, 0xDF, 0xFC, 0x66, 0x80, 0xD0, 0x03 };
				if (Hash.verifyHash(hash, russia))
				{
					UnturnedLog.info("Matched Russia legacy terrain materials hash");
					legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("17b88227113041869ba4661b227a0590"); // Russia Grass Dead
					legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("8a2b6f2215d6460f8b6fece2ccd9c208"); // Russia Stone
					legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("79787e2ca948457a9a322179cf580386"); // Russia Farm Wheat
					legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("db482f0f23d1414096114aee61195058"); // Russia Grass
					legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("ceb122707edc4f349be0a97d8f05fd09"); // Russia Gravel
					legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("b4ffe0d7b8ed4ff2b4c302c489108b02"); // Russia Leaves
					legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b"); // Russia Road
					legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619"); // Russia Gravel Shore
					return;
				}

				byte[] washington = new byte[20] { 0x42, 0xA1, 0x7C, 0xF8, 0x80, 0x6E, 0x89, 0xCC, 0xC0, 0x80, 0x26, 0x51, 0xF6, 0x9E, 0x18, 0x43, 0x4C, 0xF6, 0xC6, 0x4C };
				if (Hash.verifyHash(hash, washington))
				{
					UnturnedLog.info("Matched Washington legacy terrain materials hash");
					legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("e52b20e26b7c47c89aa5a350938f8f42"); // Yukon Dirt
					legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("e981f9fae3fa43d68a9a0bfa6472a69f"); // Farm Corn
					legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("5020515a0b9a4b1eb610c006d81f806c"); // Washington Grass 0
					legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("a14df8dd9bb44f1d967a53f43bde54e6"); // Yukon Gravel
					legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b"); // Russia Road
					legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619"); // Russia Gravel Shore
					legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("d691f78202c84951a3a697f310abd115"); // Washington Grass 1
					legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("50acf0bddd844f93addd0097f7d95d95"); // PEI Stone 0
					return;
				}

				byte[] yukon = new byte[20] { 0xFB, 0xBA, 0x1D, 0x90, 0xF0, 0xAB, 0x4A, 0x56, 0xC8, 0x1E, 0xF1, 0xF0, 0x04, 0xBF, 0x4D, 0x50, 0x4D, 0xC5, 0xB4, 0xCE };
				if (Hash.verifyHash(hash, yukon))
				{
					UnturnedLog.info("Matched Yukon legacy terrain materials hash");
					legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("e52b20e26b7c47c89aa5a350938f8f42"); // Yukon Dirt
					legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("e981f9fae3fa43d68a9a0bfa6472a69f"); // Farm Corn
					legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("3d7717c2bc074401853b2fdacd9db1ba"); // PEI Grass 0
					legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("a14df8dd9bb44f1d967a53f43bde54e6"); // Yukon Gravel
					legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b"); // Russia Road
					legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619"); // Russia Gravel Shore
					legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("e25f0351181f4ad1a9c0dc31d2fedade"); // Yukon Snow
					legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("50acf0bddd844f93addd0097f7d95d95"); // PEI Stone 0
					return;
				}

				byte[] greece = new byte[20] { 0x60, 0xA2, 0xF0, 0x6A, 0xC7, 0xE3, 0x19, 0x4C, 0xD3, 0x01, 0x12, 0x68, 0x40, 0x22, 0x7F, 0xBC, 0x80, 0x86, 0x01, 0x0B };
				if (Hash.verifyHash(hash, greece))
				{
					UnturnedLog.info("Matched Greece legacy terrain materials hash");
					legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("cf33a7a8fd52461bb523d84234b3a232"); // Greece Dead
					legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("1a917f0fbc0f48d2a4dfda5e15a623df"); // Greece Dirt
					legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("76c32cee254f4aeda910d3d8d9788a46"); // Greece Farm
					legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("98c2ae7c2aad48148c9daeb6fab4aa2a"); // Greece Grass
					legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("c743d33c42f54753a529886997626040"); // Greece Road
					legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("e476cd429bdb41fcafa1df84663e0a47"); // Greece Stone
					legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("30fe7a6c14ee4064865054b82cd71d13"); // Greece Dead 2
					legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("1b30a651c2ff4c90b8c62d6d9212c146"); // Greece Sand
					return;
				}

				UnturnedLog.info("Unable to match legacy terrain materials hash, using names instead");

				try
				{
					Bundle materialBundle = Bundles.getBundle(materialsAssetBundlePath, false);
					Texture2D[] materialTextures = materialBundle.loadAll<Texture2D>();
					int layerIndex = 0;
					foreach (Texture2D materialTexture in materialTextures)
					{
						string materialName = materialTexture.name;
						if (materialName.IndexOf("_Mask") != -1)
						{
							continue;
						}

						if (materialName.IndexOf("Farm", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("22b77c4c51514b0fbb66765eedf1a7f4"); // PEI Farm Wheat
						}
						else if (materialName.IndexOf("Road", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b"); // Russia Road
						}
						else if (materialName.IndexOf("Grass", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("3d7717c2bc074401853b2fdacd9db1ba"); // PEI Grass
						}
						else if (materialName.IndexOf("Gravel", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("a14df8dd9bb44f1d967a53f43bde54e6"); // Yukon Gravel
						}
						else if (materialName.IndexOf("Sand", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619"); // Russia Gravel Shore
						}
						else if (materialName.IndexOf("Snow", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("e25f0351181f4ad1a9c0dc31d2fedade"); // Yukon Snow
						}
						else if (materialName.IndexOf("Stone", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("8a2b6f2215d6460f8b6fece2ccd9c208"); // Russia Stone
						}
						else if (materialName.IndexOf("Dirt", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("e52b20e26b7c47c89aa5a350938f8f42"); // Yukon Dirt
						}
						else if (materialName.IndexOf("Leaves", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("b4ffe0d7b8ed4ff2b4c302c489108b02"); // Russia Leaves
						}
						else if (materialName.IndexOf("Dead", System.StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							legacyMaterialGuids[layerIndex] = new AssetReference<LandscapeMaterialAsset>("17b88227113041869ba4661b227a0590"); // Russia Grass Dead
						}

						if (legacyMaterialGuids[layerIndex].isNull)
						{
							UnturnedLog.warn($"Unable to match layer {layerIndex} name \"{materialName}\" with any known materials");
						}
						else
						{
							UnturnedLog.info($"Matched layer {layerIndex} name \"{materialName}\" with \"{legacyMaterialGuids[layerIndex].Find()?.name}\"");
						}

						++layerIndex;
						if (layerIndex >= 8)
						{
							break;
						}
					}
					materialBundle.unload();
				}
				catch (System.Exception exception)
				{
					// Issue #3348 loading Materials.unity3d can throw an exception if already loaded.
					UnturnedLog.exception(exception, "Caught exception loading legacy terrain materials:");
				}

				// Worst case scenario we fill in any empty slots with colors because we do not want to scare any map
				// creators into thinking their painting data has been lost. Colors are sorted from least to most
				// confusing e.g. white is confusing because it looks like snow.
				AssetReference<LandscapeMaterialAsset>[] fallbacks = new AssetReference<LandscapeMaterialAsset>[8]
				{
					new AssetReference<LandscapeMaterialAsset>("64357418ae184a959186d1f592a93761"), // Red
					new AssetReference<LandscapeMaterialAsset>("8ea9b170d93e4f9a8a0e0c61cd4bee6a"), // Blue
					new AssetReference<LandscapeMaterialAsset>("e54a3da2c46e4927848fed4cdead560a"), // Yellow
					new AssetReference<LandscapeMaterialAsset>("713fe6ff00e647408047d5dd39d815c0"), // Purple
					new AssetReference<LandscapeMaterialAsset>("00ddd72266914141b39e33227942a7df"), // Orange
					new AssetReference<LandscapeMaterialAsset>("498ca625072d443a876b2a4f11896018"), // Green
					new AssetReference<LandscapeMaterialAsset>("9889a0b5aad04ddd8c4c463f3e1b79f6"), // Black
					new AssetReference<LandscapeMaterialAsset>("5fd97d1f946c45a79e3d47b49d0348d8"), // White
				};

				int fallbackIndex = 0;
				for (int index = 0; index < 8; ++index)
				{
					if (legacyMaterialGuids[index].isNull)
					{
						UnturnedLog.warn($"Defaulting empty layer {index} to \"{fallbacks[fallbackIndex].Find()?.name}\"");
						legacyMaterialGuids[index] = fallbacks[fallbackIndex];
						++fallbackIndex;
					}
				}
			}
		}

		protected static void loadTrees()
		{
			_trees = new List<ResourceSpawnpoint>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					_trees[x, y] = new List<ResourceSpawnpoint>();
				}
			}

			_total = 0;
			shouldInstantlyLoad = true;
			regionTracker = new RegionIncrementalVisibilityTracker();
			skyboxRegionTracker = new RegionIncrementalVisibilityTracker();

			_regionTrees = new RegionDictionary<ResourceSpawnpoint>();

			treesHash = new byte[20];

			if (ReadWrite.fileExists(Level.info.path + "/Terrain/Trees.dat", false, false))
			{
				River river = new River(Level.info.path + "/Terrain/Trees.dat", false);
				byte version = river.readByte();

				if (version > 3)
				{
#if BATCH
					List<GameObject>[] combine = new List<GameObject>[resources.Length];
					
					for(int index = 0; index < combine.Length; index ++)
					{
						combine[index] = new List<GameObject>();
					}
#endif

					TreeRedirectorMap treeRedirectorMap = null;
					if (Level.shouldUseHolidayRedirects)
					{
						treeRedirectorMap = new TreeRedirectorMap();
					}
					bool useEditorAssetRedirector = Level.isEditor && EditorAssetRedirector.HasRedirects;

					LevelBatching levelBatching = Level.shouldUseLevelBatching ? LevelBatching.Get() : null;

					if (version >= SAVEDATA_TREES_VERSION_INT_REGION_COORDS)
					{
						int allTreesCount = river.readInt32();
						for (int treeIndex = 0; treeIndex < allTreesCount; ++treeIndex)
						{
							System.Guid guid = river.readGUID();
							Vector3 point = river.readSingleVector3();

							Quaternion rotation;
							Vector3 scale;
							if (version >= SAVEDATA_TREES_VERSION_ROTATION_AND_SCALE)
							{
								rotation = river.readSingleQuaternion();
								scale = river.readSingleVector3();
							}
							else
							{
								// Fixed-up later.
								rotation = Quaternion.identity;
								scale = Vector3.one;
							}

							bool isGenerated = river.readBoolean();

							if (guid == System.Guid.Empty)
							{
								continue;
							}

							if (treeRedirectorMap != null)
							{
								ResourceAsset redirect = treeRedirectorMap.redirect(guid);
								if (redirect == null)
								{
									guid = System.Guid.Empty;
								}
								else
								{
									guid = redirect.GUID;
								}
							}
							else if (useEditorAssetRedirector)
							{
								ResourceAsset redirect = EditorAssetRedirector.Redirect<ResourceAsset>(guid);
								if (redirect != null)
								{
									guid = redirect.GUID;
								}
							}

							if (guid == System.Guid.Empty)
							{
								continue;
							}

							if (version < SAVEDATA_TREES_VERSION_ROTATION_AND_SCALE)
							{
								ResourceAsset asset = Assets.find(guid) as ResourceAsset;
								if (asset != null)
								{
									asset.GetLegacyRotationAndScale(point, out rotation, out scale);
								}
							}

							NetId netId = LevelNetIdRegistry.GetTreeNetIdV2(treeIndex);
							ResourceSpawnpoint resource = new ResourceSpawnpoint(0, 0, guid, point, rotation, scale, isGenerated, netId);

							if (resource.asset == null && Assets.shouldLoadAnyAssets)
							{
								UnturnedLog.error("Tree with no asset at {0} (ID: {1})", point, guid);
							}

							Vector2Int regionCoord = Regions.GetCoordinateVector2Int(point);
							List<ResourceSpawnpoint> spawns = _regionTrees.GetOrAddList(regionCoord);
							spawns.Add(resource);

							if (Regions.IsVector2IntWithinLegacyRange(regionCoord))
							{
								// Backwards compatibility.
								_trees[regionCoord.x, regionCoord.y].Add(resource);
							}

							levelBatching?.AddResourceSpawnpoint(resource);

							_total++;
						}
					}
					else
					{
						for (byte x = 0; x < Regions.WORLD_SIZE; x++)
						{
							for (byte y = 0; y < Regions.WORLD_SIZE; y++)
							{
								ushort count = river.readUInt16();
								for (ushort index = 0; index < count; index++)
								{
									if (version > 4)
									{
										ushort id = river.readUInt16();
										System.Guid guid;

										if (version < SAVEDATA_TREES_VERSION_ADDED_GUID)
										{
											guid = System.Guid.Empty;
										}
										else
										{
											guid = river.readGUID();
										}

										Vector3 point = river.readSingleVector3();
										bool isGenerated = river.readBoolean();

										if (id != 0 || guid != System.Guid.Empty)
										{
											if (treeRedirectorMap != null)
											{
												ResourceAsset redirect = treeRedirectorMap.redirect(guid);
												if (redirect == null)
												{
													id = 0;
													guid = System.Guid.Empty;
												}
												else
												{
													id = redirect.id;
													guid = redirect.GUID;
												}
											}
											else if (useEditorAssetRedirector)
											{
												ResourceAsset redirect = EditorAssetRedirector.Redirect<ResourceAsset>(guid);
												if (redirect != null)
												{
													id = redirect.id;
													guid = redirect.GUID;
												}
											}

											if (id != 0 || guid != System.Guid.Empty)
											{
												Quaternion rotation = Quaternion.identity;
												Vector3 scale = Vector3.one;
												ResourceAsset asset = Assets.find(guid) as ResourceAsset;
												if (asset != null)
												{
													asset.GetLegacyRotationAndScale(point, out rotation, out scale);
												}

												NetId netId = LevelNetIdRegistry.GetTreeNetId(x, y, index);
												ResourceSpawnpoint resource = new ResourceSpawnpoint(0, id, guid, point, rotation, scale, isGenerated, netId);

												if (resource.asset == null && Assets.shouldLoadAnyAssets)
												{
													UnturnedLog.error("Tree with no asset in region {0}, {1}: {2} {3}", x, y, id, guid);
												}

												List<ResourceSpawnpoint> spawns = _regionTrees.GetOrAddList(x, y);
												spawns.Add(resource);

												// Backwards compatibility.
												_trees[x, y].Add(resource);

												levelBatching?.AddResourceSpawnpoint(resource);

												_total++;
											}
										}
									}
									else
									{
										byte type = river.readByte();
										ushort id = 3; // Birch tree
										Vector3 point = river.readSingleVector3();
										bool isGenerated = river.readBoolean();

										Quaternion rotation = Quaternion.identity;
										Vector3 scale = Vector3.one;
										ResourceAsset asset = Assets.find(EAssetType.RESOURCE, id) as ResourceAsset;
										if (asset != null)
										{
											asset.GetLegacyRotationAndScale(point, out rotation, out scale);
										}

										NetId netId = LevelNetIdRegistry.GetTreeNetId(x, y, index);
										ResourceSpawnpoint resource = new ResourceSpawnpoint(type, id, System.Guid.Empty, point, rotation, scale, isGenerated, netId);

										if (resource.asset == null && Assets.shouldLoadAnyAssets)
										{
											UnturnedLog.error("Tree with no asset in region {0}, {1}: {2} {3}", x, y, id, type);
										}

#if BATCH
								combine[type].Add(resource.model.gameObject);
#endif

										List<ResourceSpawnpoint> spawns = _regionTrees.GetOrAddList(x, y);
										spawns.Add(resource);

										// Backwards compatibility.
										_trees[x, y].Add(resource);

										_total++;
									}
								}
							}
						}
					}
				}

				treesHash = river.getHash();

				river.closeRiver();
			}
		}

		public static void load(ushort size)
		{
			hasLegacyDataForConversion = false;
			doesLegacyDataIncludeSplatmapWeights = false;

			if (!Level.info.configData.Use_Legacy_Ground)
			{
				loadTrees();
				return;
			}

			if (System.IO.File.Exists(GetConversionMarkerFilePath()))
			{
				UnturnedLog.info("Skipping legacy terrain loading because it has already been converted");
				loadTrees();
				return;
			}

			hasLegacyDataForConversion = true;
			legacyMaterialGuids = new AssetReference<LandscapeMaterialAsset>[8];

			_models = new GameObject().transform;
			_models.name = "Ground";
			_models.parent = Level.level;
			_models.tag = "Ground";
			_models.gameObject.layer = LayerMasks.GROUND;

			_terrain = _models.gameObject.AddComponent<Terrain>();
			_terrain.drawInstanced = SystemInfo.supportsInstancing;
			_terrain.name = "Ground";
			_terrain.heightmapPixelError = 200;
			_terrain.transform.position = new Vector3(-size / 2, 0, -size / 2);
			_terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
			_terrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_terrain.drawHeightmap = false;
			_terrain.drawTreesAndFoliage = false;

			_terrain.treeDistance = 0;
			_terrain.treeBillboardDistance = 0;
			_terrain.treeCrossFadeLength = 0;
			_terrain.treeMaximumFullLODCount = 0;

			_data = new TerrainData();
			data.name = "Ground";
			data.heightmapResolution = size / 8;
			data.alphamapResolution = size / 4;
			data.size = new Vector3(size, Level.TERRAIN, size);

			data.wavingGrassTint = Color.white;

			byte heightmapVersion = 0;
			byte heightmap2Version = 0;

			if (ReadWrite.fileExists(Level.info.path + "/Terrain/Heights.dat", false, false))
			{
				Block heightsBlock = ReadWrite.readBlock(Level.info.path + "/Terrain/Heights.dat", false, false, 0);
				heightmapVersion = heightsBlock.readByte();
				heightmap2Version = heightsBlock.readByte();
			}

			if (ReadWrite.fileExists(Level.info.path + "/Terrain/Heightmap.png", false, false))
			{
				byte[] heightBytes = ReadWrite.readBytes(Level.info.path + "/Terrain/Heightmap.png", false, false);
				Texture2D heightTexture = new Texture2D(data.heightmapResolution, data.heightmapResolution, TextureFormat.ARGB32, false);
				heightTexture.name = "Heightmap_Load";
				heightTexture.hideFlags = HideFlags.HideAndDontSave;
				heightTexture.LoadImage(heightBytes);

				float[,] heightmap = new float[heightTexture.width, heightTexture.height];

				for (int x = 0; x < heightTexture.width; x++)
				{
					for (int y = 0; y < heightTexture.height; y++)
					{
						if (heightmapVersion > 0)
						{
							byte[] pack = { (byte) (heightTexture.GetPixel(x, y).r * 255), (byte) (heightTexture.GetPixel(x, y).g * 255), (byte) (heightTexture.GetPixel(x, y).b * 255), (byte) (heightTexture.GetPixel(x, y).a * 255) };

							heightmap[x, y] = System.BitConverter.ToSingle(pack, 0);
						}
						else
						{
							heightmap[x, y] = heightTexture.GetPixel(x, y).r;
						}
					}
				}

				data.SetHeights(0, 0, heightmap);

				heightBytes = null;

				DestroyImmediate(heightTexture);
			}
			else
			{
				float[,] heightmap = new float[data.heightmapResolution, data.heightmapResolution];

				for (int x = 0; x < data.heightmapResolution; x++)
				{
					for (int y = 0; y < data.heightmapResolution; y++)
					{
						heightmap[x, y] = 0.03f;
					}
				}

				data.SetHeights(0, 0, heightmap);
			}

			loadSplatPrototypes();

			alphamapHQ = new float[data.alphamapWidth, data.alphamapHeight, ALPHAMAPS * 4];

			for (int index = 0; index < ALPHAMAPS; index++)
			{
				bool hasHQ = false;
				if (ReadWrite.fileExists(Level.info.path + "/Terrain/AlphamapHQ_" + index + ".png", false, false))
				{
					byte[] alphaBytesHQ = ReadWrite.readBytes(Level.info.path + "/Terrain/AlphamapHQ_" + index + ".png", false, false);
					Texture2D alphaTextureHQ = new Texture2D(data.heightmapResolution, data.heightmapResolution, TextureFormat.ARGB32, false);
					alphaTextureHQ.name = "AlphamapHQ_Load";
					alphaTextureHQ.hideFlags = HideFlags.HideAndDontSave;
					alphaTextureHQ.LoadImage(alphaBytesHQ);
					alphaBytesHQ = null;

					for (int x = 0; x < alphaTextureHQ.width; x++)
					{
						for (int y = 0; y < alphaTextureHQ.height; y++)
						{
							Color color = alphaTextureHQ.GetPixel(x, y);

							alphamapHQ[x, y, (index * 4) + 0] = color.r;
							alphamapHQ[x, y, (index * 4) + 1] = color.g;
							alphamapHQ[x, y, (index * 4) + 2] = color.b;
							alphamapHQ[x, y, (index * 4) + 3] = color.a;
						}
					}

					DestroyImmediate(alphaTextureHQ);
					hasHQ = true;
					doesLegacyDataIncludeSplatmapWeights = true;
				}

				if (!hasHQ)
				{
					if (ReadWrite.fileExists(Level.info.path + "/Terrain/Alphamap_" + index + ".png", false, false))
					{
						byte[] alphaBytes = ReadWrite.readBytes(Level.info.path + "/Terrain/Alphamap_" + index + ".png", false, false);
						Texture2D alphaTexture = new Texture2D(data.heightmapResolution, data.heightmapResolution, TextureFormat.ARGB32, false);
						alphaTexture.name = "Alphamap_Load";
						alphaTexture.hideFlags = HideFlags.HideAndDontSave;
						alphaTexture.LoadImage(alphaBytes);
						alphaBytes = null;

						for (int x = 0; x < alphaTexture.width; x++)
						{
							for (int y = 0; y < alphaTexture.height; y++)
							{
								Color color = alphaTexture.GetPixel(x, y);

								alphamapHQ[x, y, (index * 4) + 0] = color.r;
								alphamapHQ[x, y, (index * 4) + 1] = color.g;
								alphamapHQ[x, y, (index * 4) + 2] = color.b;
								alphamapHQ[x, y, (index * 4) + 3] = color.a;
							}
						}

						doesLegacyDataIncludeSplatmapWeights = true;
						DestroyImmediate(alphaTexture);
					}
				}
			}

			data.baseMapResolution = size / 8;
			data.baseMapResolution = size / 4;

			_terrain.terrainData = data;

			_terrain.terrainData.wavingGrassAmount = 0;
			_terrain.terrainData.wavingGrassSpeed = 1;
			_terrain.terrainData.wavingGrassStrength = 1;

			loadTrees();

			_models2 = new GameObject().transform;
			_models2.name = "Ground2";
			_models2.parent = Level.level;
			_models2.tag = "Ground2";
			_models2.gameObject.layer = LayerMasks.GROUND2;

			_terrain2 = _models2.gameObject.AddComponent<Terrain>();
			_terrain2.drawInstanced = _terrain.drawInstanced;
			_terrain2.name = "Ground2";
			_terrain2.heightmapPixelError = 200;
			_terrain2.transform.position = new Vector3(-size, 0, -size);
			_terrain2.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
			_terrain2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_terrain2.drawHeightmap = _terrain.drawHeightmap;
			_terrain2.drawTreesAndFoliage = false;

			_terrain2.treeDistance = 0;
			_terrain2.treeBillboardDistance = 0;
			_terrain2.treeCrossFadeLength = 0;
			_terrain2.treeMaximumFullLODCount = 0;

			_data2 = new TerrainData();
			data2.name = "Ground2";
			data2.heightmapResolution = size / 16;
			data2.alphamapResolution = size / 8;
			data2.size = new Vector3(size * 2, Level.TERRAIN, size * 2);

			if (ReadWrite.fileExists(Level.info.path + "/Terrain/Heightmap2.png", false, false))
			{
				byte[] heightBytes = ReadWrite.readBytes(Level.info.path + "/Terrain/Heightmap2.png", false, false);
				Texture2D heightTexture = new Texture2D(data2.heightmapResolution, data2.heightmapResolution, TextureFormat.ARGB32, false);
				heightTexture.name = "Heightmap2_Load";
				heightTexture.hideFlags = HideFlags.HideAndDontSave;
				heightTexture.LoadImage(heightBytes);
				heightBytes = null;

				float[,] heightmap = new float[heightTexture.width, heightTexture.height];

				for (int x = 0; x < heightTexture.width; x++)
				{
					for (int y = 0; y < heightTexture.height; y++)
					{
						if (heightmap2Version > 0)
						{
							byte[] pack = { (byte) (heightTexture.GetPixel(x, y).r * 255), (byte) (heightTexture.GetPixel(x, y).g * 255), (byte) (heightTexture.GetPixel(x, y).b * 255), (byte) (heightTexture.GetPixel(x, y).a * 255) };

							heightmap[x, y] = System.BitConverter.ToSingle(pack, 0);
						}
						else
						{
							heightmap[x, y] = heightTexture.GetPixel(x, y).r;
						}
					}
				}

				data2.SetHeights(0, 0, heightmap);

				DestroyImmediate(heightTexture);
			}
			else
			{
				float[,] heightmap = new float[data2.heightmapResolution, data2.heightmapResolution];

				for (int x = 0; x < data2.heightmapResolution; x++)
				{
					for (int y = 0; y < data2.heightmapResolution; y++)
					{
						heightmap[x, y] = 0;
					}
				}

				data2.SetHeights(0, 0, heightmap);
			}

			alphamap2HQ = new float[data2.alphamapWidth, data2.alphamapHeight, ALPHAMAPS * 4];

			for (int index = 0; index < ALPHAMAPS; index++)
			{
				bool hasHQ = false;
				if (ReadWrite.fileExists(Level.info.path + "/Terrain/Alphamap2HQ_" + index + ".png", false, false))
				{
					byte[] alphaBytesHQ = ReadWrite.readBytes(Level.info.path + "/Terrain/Alphamap2HQ_" + index + ".png", false, false);
					Texture2D alphaTextureHQ = new Texture2D(data2.heightmapResolution, data2.heightmapResolution, TextureFormat.ARGB32, false);
					alphaTextureHQ.name = "Alphamap2HQ_Load";
					alphaTextureHQ.hideFlags = HideFlags.HideAndDontSave;
					alphaTextureHQ.LoadImage(alphaBytesHQ);
					alphaBytesHQ = null;

					for (int x = 0; x < alphaTextureHQ.width; x++)
					{
						for (int y = 0; y < alphaTextureHQ.height; y++)
						{
							Color color = alphaTextureHQ.GetPixel(x, y);

							alphamap2HQ[x, y, (index * 4) + 0] = color.r;
							alphamap2HQ[x, y, (index * 4) + 1] = color.g;
							alphamap2HQ[x, y, (index * 4) + 2] = color.b;
							alphamap2HQ[x, y, (index * 4) + 3] = color.a;
						}
					}

					DestroyImmediate(alphaTextureHQ);
					hasHQ = true;
				}

				if (!hasHQ)
				{
					if (ReadWrite.fileExists(Level.info.path + "/Terrain/Alphamap2_" + index + ".png", false, false))
					{
						byte[] alphaBytes = ReadWrite.readBytes(Level.info.path + "/Terrain/Alphamap2_" + index + ".png", false, false);
						Texture2D alphaTexture = new Texture2D(data2.heightmapResolution, data2.heightmapResolution, TextureFormat.ARGB32, false);
						alphaTexture.name = "Alphamap2_Load";
						alphaTexture.hideFlags = HideFlags.HideAndDontSave;
						alphaTexture.LoadImage(alphaBytes);
						alphaBytes = null;

						for (int x = 0; x < alphaTexture.width; x++)
						{
							for (int y = 0; y < alphaTexture.height; y++)
							{
								Color color = alphaTexture.GetPixel(x, y);

								alphamap2HQ[x, y, (index * 4) + 0] = color.r;
								alphamap2HQ[x, y, (index * 4) + 1] = color.g;
								alphamap2HQ[x, y, (index * 4) + 2] = color.b;
								alphamap2HQ[x, y, (index * 4) + 3] = color.a;
							}
						}

						DestroyImmediate(alphaTexture);
					}
				}
			}

			data2.baseMapResolution = size / 8;
			data2.baseMapResolution = size / 4;

			_terrain2.terrainData = data2;

			data2.wavingGrassTint = Color.white;
		}

		private static List<ResourceSpawnpoint> saveTreesAllTreesList;
		protected static void saveTrees()
		{
			River river = new River(Level.info.path + "/Terrain/Trees.dat", false);
			river.writeByte(SAVEDATA_TREES_VERSION_NEWEST);

			if (saveTreesAllTreesList == null)
			{
				saveTreesAllTreesList = new List<ResourceSpawnpoint>();
			}
			else
			{
				saveTreesAllTreesList.Clear();
			}
			GatherAllTrees(saveTreesAllTreesList);

			river.writeInt32(saveTreesAllTreesList.Count);
			foreach (ResourceSpawnpoint tree in saveTreesAllTreesList)
			{
				if (tree != null && tree.model != null && tree.guid != System.Guid.Empty)
				{
					river.writeGUID(tree.guid);
					river.writeSingleVector3(tree.point);
					river.writeSingleQuaternion(tree.angle);
					river.writeSingleVector3(tree.scale);
					river.writeBoolean(tree.isGenerated);
				}
				else
				{
					river.writeGUID(System.Guid.Empty);
					river.writeSingleVector3(Vector3.zero);
					river.writeSingleQuaternion(Quaternion.identity);
					river.writeSingleVector3(Vector3.one);
					river.writeBoolean(true);
				}
			}

			river.closeRiver();
		}

		public static void save()
		{
			if (!Level.info.configData.Use_Legacy_Ground)
			{
				saveTrees();

				return;
			}

			if (hasLegacyDataForConversion)
			{
				// This was the first time saving after conversion so write the file.
				System.IO.File.WriteAllText(GetConversionMarkerFilePath(), "1");
			}

			saveTrees();
		}

		/// <summary>
		/// Game does not currently have a way to resave level's Config.json file, so instead we save a text file
		/// indicating that the terrain auto conversion was performed. If there was a bug with auto conversion then
		/// all of the old files are still present and can be re-converted.
		/// </summary>
		private static string GetConversionMarkerFilePath()
		{
			return System.IO.Path.Combine(Level.info.path, "Terrain", "TerrainWasAutoConverted.txt");
		}

		private static void onPlayerTeleported(Player player, Vector3 position)
		{
			shouldInstantlyLoad = true;
		}

		private static void onPlayerCreated(Player player)
		{
			if (player.channel.IsLocalPlayer)
			{
				Player.LocalPlayer.onPlayerTeleported += onPlayerTeleported;
			}
		}

		private static void ImmediatelySyncRegionalVisibility()
		{
			regionTrackerData.Clear();
			regionTracker.MaxDistance = RegularTreeMaxDistance;
			regionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				List<ResourceSpawnpoint> regionTrees = GetTreesOrNullInRegion(coord);
				if (regionTrees == null)
					continue;

				RegionVisibilityData visData = coordDataPair.Value;
				foreach (ResourceSpawnpoint tree in regionTrees)
				{
					tree.SetIsActiveInRegion(visData.isInsideMask);
				}
			}

			regionTracker.FlushProgress();

			regionTrackerData.Clear();
			skyboxRegionTracker.MaxDistance = SkyboxTreeMaxDistance;
			skyboxRegionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				List<ResourceSpawnpoint> regionTrees = GetTreesOrNullInRegion(coord);
				if (regionTrees == null)
					continue;

				RegionVisibilityData visData = coordDataPair.Value;
				foreach (ResourceSpawnpoint tree in regionTrees)
				{
					tree.SetIsSkyboxActiveInRegion(visData.isInsideMask);
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
			regionTracker.MaxDistance = RegularTreeMaxDistance;
			regionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				List<ResourceSpawnpoint> regionTrees = GetTreesOrNullInRegion(coord);
				if (regionTrees == null)
				{
					regionTracker.NotifyRegionFinishedUpdating(coord);
					continue;
				}

				RegionVisibilityData visData = coordDataPair.Value;
				if (visData.progressIndex < regionTrees.Count)
				{
					regionTrees[visData.progressIndex].SetIsActiveInRegion(visData.isInsideMask);
				}
				else
				{
					regionTracker.NotifyRegionFinishedUpdating(coord);
				}
			}

			regionTrackerData.Clear();
			skyboxRegionTracker.MaxDistance = SkyboxTreeMaxDistance;
			skyboxRegionTracker.UpdateRegions(regionTrackerData);

			foreach (KeyValuePair<Vector2Int, RegionVisibilityData> coordDataPair in regionTrackerData)
			{
				Vector2Int coord = coordDataPair.Key;
				List<ResourceSpawnpoint> regionTrees = GetTreesOrNullInRegion(coord);
				if (regionTrees == null)
				{
					skyboxRegionTracker.NotifyRegionFinishedUpdating(coord);
					continue;
				}

				RegionVisibilityData visData = coordDataPair.Value;
				if (visData.progressIndex < regionTrees.Count)
				{
					regionTrees[visData.progressIndex].SetIsSkyboxActiveInRegion(visData.isInsideMask);
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

			if (_regionTrees == null)
			{
				// Ground has not been initialized yet.
				return;
			}

#if !FORCE_LOW_LOD
			if (MainCamera.instance == null)
				return;

			tickRegionalVisibility();
#endif
		}

		private void Start()
		{
			Player.onPlayerCreated += onPlayerCreated;

			if (_Triplanar_Primary_Size == -1)
			{
				_Triplanar_Primary_Size = Shader.PropertyToID("_Triplanar_Primary_Size");
			}

			Shader.SetGlobalFloat(_Triplanar_Primary_Size, triplanarPrimarySize);

			if (_Triplanar_Primary_Weight == -1)
			{
				_Triplanar_Primary_Weight = Shader.PropertyToID("_Triplanar_Primary_Weight");
			}

			Shader.SetGlobalFloat(_Triplanar_Primary_Weight, triplanarPrimaryWeight);

			if (_Triplanar_Secondary_Size == -1)
			{
				_Triplanar_Secondary_Size = Shader.PropertyToID("_Triplanar_Secondary_Size");
			}

			Shader.SetGlobalFloat(_Triplanar_Secondary_Size, triplanarSecondarySize);

			if (_Triplanar_Secondary_Weight == -1)
			{
				_Triplanar_Secondary_Weight = Shader.PropertyToID("_Triplanar_Secondary_Weight");
			}

			Shader.SetGlobalFloat(_Triplanar_Secondary_Weight, triplanarSecondaryWeight);

			if (_Triplanar_Tertiary_Size == -1)
			{
				_Triplanar_Tertiary_Size = Shader.PropertyToID("_Triplanar_Tertiary_Size");
			}

			Shader.SetGlobalFloat(_Triplanar_Tertiary_Size, triplanarTertiarySize);

			if (_Triplanar_Tertiary_Weight == -1)
			{
				_Triplanar_Tertiary_Weight = Shader.PropertyToID("_Triplanar_Tertiary_Weight");
			}

			Shader.SetGlobalFloat(_Triplanar_Tertiary_Weight, triplanarTertiaryWeight);
		}

		static LevelGround()
		{
			SDG.Framework.Foliage.FoliageSystem.preBakeTile += handlePreBakeTile;
		}
	}

	/// <summary>
	/// Caches uint16 ID to ID redirects.
	/// </summary>
	internal class TreeRedirectorMap
	{
		public TreeRedirectorMap()
		{
			redirectedIds = new Dictionary<System.Guid, ResourceAsset>();
		}

		public ResourceAsset redirect(System.Guid originalId)
		{
			ResourceAsset redirectedAsset;
			if (redirectedIds.TryGetValue(originalId, out redirectedAsset) == false)
			{
				ResourceAsset originalAsset = Assets.find(originalId) as ResourceAsset;

				if (!Dedicator.IsDedicatedServer)
				{
					ClientAssetIntegrity.QueueRequest(originalId, originalAsset, "Tree Holiday Redirect (Original)");
				}

				if (originalAsset != null)
				{
					AssetReference<ResourceAsset> redirectedRef = originalAsset.getHolidayRedirect();
					if (redirectedRef.isValid)
					{
						redirectedAsset = redirectedRef.Find();

						if (!Dedicator.IsDedicatedServer)
						{
							ClientAssetIntegrity.QueueRequest(redirectedRef.GUID, redirectedAsset, "Tree Holiday Redirect");
						}

						if (redirectedAsset == null)
						{
							if (Assets.shouldLoadAnyAssets)
							{
								UnturnedLog.error("Missing holiday redirect for tree {0}", originalAsset);
							}

							// If object is missing on the server then do not kick clients for missing it as well.
							ClientAssetIntegrity.ServerAddKnownMissingAsset(redirectedRef.GUID, "Tree Holiday Redirect");
						}
					}
					else
					{
						// Does not have a redirect for this event, so use the original tree.
						redirectedAsset = originalAsset;
					}
				}
				else
				{
					// If object is missing on the server then do not kick clients for missing it as well.
					ClientAssetIntegrity.ServerAddKnownMissingAsset(originalId, "Tree Holiday Redirect (Original)");
				}

				redirectedIds.Add(originalId, redirectedAsset);
			}

			return redirectedAsset;
		}

		private Dictionary<System.Guid, ResourceAsset> redirectedIds;
	}
}
