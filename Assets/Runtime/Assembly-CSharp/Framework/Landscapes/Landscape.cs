////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Debug;
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public delegate void LandscapeLoadedHandler();

	public partial class Landscape : DevkitHierarchyItemBase
	{
		public static readonly float TILE_SIZE = 1024;
		public static readonly int TILE_SIZE_INT = 1024;
		public static readonly float TILE_HEIGHT = 2048;
		public static readonly int TILE_HEIGHT_INT = 2048;
		public static readonly int HEIGHTMAP_RESOLUTION = 257;
		public static readonly int HEIGHTMAP_RESOLUTION_MINUS_ONE = 256;
		public static readonly float HEIGHTMAP_WORLD_UNIT = 4;
		public static readonly float HALF_HEIGHTMAP_WORLD_UNIT = 2;
		public static readonly int SPLATMAP_RESOLUTION = 256;
		public static readonly int SPLATMAP_RESOLUTION_MINUS_ONE = 255;
		public static readonly float SPLATMAP_WORLD_UNIT = 4;
		public static readonly float HALF_SPLATMAP_WORLD_UNIT = 2;
		public static readonly int BASEMAP_RESOLUTION = 256;
		public static readonly int SPLATMAP_COUNT = 2;
		public static readonly int SPLATMAP_CHANNELS = 4;
		public static readonly int SPLATMAP_LAYERS = SPLATMAP_COUNT * SPLATMAP_CHANNELS;
		public const int HOLES_RESOLUTION = 256; // Heightmap resolution minus one.
		public const float HALF_DIAGONAL_SPLATMAP_WORLD_UNIT = 2.82842712f;

		protected static readonly float[] SPLATMAP_LAYER_BUFFER = new float[SPLATMAP_LAYERS];

		public static Landscape instance
		{
			get;
			protected set;
		}

		public static event LandscapeLoadedHandler loaded;

		protected static Dictionary<LandscapeCoord, LandscapeTile> tiles = new Dictionary<LandscapeCoord, LandscapeTile>();
		protected static Dictionary<LandscapeCoord, LandscapeHeightmapTransaction> heightmapTransactions = new Dictionary<LandscapeCoord, LandscapeHeightmapTransaction>();
		protected static Dictionary<LandscapeCoord, LandscapeSplatmapTransaction> splatmapTransactions = new Dictionary<LandscapeCoord, LandscapeSplatmapTransaction>();
		protected static Dictionary<LandscapeCoord, LandscapeHoleTransaction> holeTransactions = new Dictionary<LandscapeCoord, LandscapeHoleTransaction>();

		private static bool _disableHoleColliders;
		/// <summary>
		/// Hacky workaround for height and material brushes in editor. As far as I can tell in Unity 2019 LTS there is no method to ignore
		/// holes when raycasting against terrain (e.g. when painting holes), so we use a duplicate TerrainData without holes in the editor.
		/// </summary>
		public static bool DisableHoleColliders
		{
			get => _disableHoleColliders;
			set
			{
				if (_disableHoleColliders == value)
					return;

				_disableHoleColliders = value;

				foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
				{
					LandscapeTile tile = pair.Value;
					if (tile.collider != null)
					{
						tile.collider.terrainData = _disableHoleColliders ? tile.dataWithoutHoles : tile.data;
					}
				}
			}
		}

		private static bool _highlightHoles;
		public static bool HighlightHoles
		{
			get => _highlightHoles;
			set
			{
				if (_highlightHoles == value)
					return;

				_highlightHoles = value;
				Shader.SetGlobalFloat("_TerrainHighlightHoles", _highlightHoles ? 1.0f : 0.0f);
			}
		}

		public static void GetUniqueMaterials(List<LandscapeMaterialAsset> materials)
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				foreach (AssetReference<LandscapeMaterialAsset> assetRef in tile.materials)
				{
					LandscapeMaterialAsset asset = assetRef.Find();
					if (asset != null && !materials.Contains(asset))
					{
						materials.Add(asset);
					}
				}
			}
		}

		/// <summary>
		/// Is point (on XZ plane) inside a masked-out pixel?
		/// </summary>
		public static bool IsPointInsideHole(Vector3 worldPosition)
		{
			LandscapeCoord tileCoord = new LandscapeCoord(worldPosition);
			LandscapeTile tile = getTile(tileCoord);
			if (tile != null)
			{
				SplatmapCoord splatmapCoord = new SplatmapCoord(tileCoord, worldPosition);
				return !tile.holes[splatmapCoord.x, splatmapCoord.y];
			}
			else
			{
				return false;
			}
		}

		public static bool getWorldHeight(Vector3 position, out float height)
		{
			LandscapeCoord coord = new LandscapeCoord(position);
			LandscapeTile tile = getTile(coord);
			if (tile != null)
			{
				height = tile.terrain.SampleHeight(position) - (TILE_HEIGHT / 2);
				return true;
			}
			else
			{
				height = 0;
				return false;
			}
		}

		public static bool getWorldHeight(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, out float height)
		{
			LandscapeTile tile = getTile(tileCoord);
			if (tile != null)
			{
				height = (tile.heightmap[heightmapCoord.x, heightmapCoord.y] * TILE_HEIGHT) - (TILE_HEIGHT / 2);
				return true;
			}
			else
			{
				height = 0;
				return false;
			}
		}

		public static bool getHeight01(Vector3 position, out float height)
		{
			LandscapeCoord coord = new LandscapeCoord(position);
			LandscapeTile tile = getTile(coord);
			if (tile != null)
			{
				height = tile.terrain.SampleHeight(position) / TILE_HEIGHT;
				return true;
			}
			else
			{
				height = 0;
				return false;
			}
		}

		public static bool getHeight01(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, out float height)
		{
			LandscapeTile tile = getTile(tileCoord);
			if (tile != null)
			{
				height = tile.heightmap[heightmapCoord.x, heightmapCoord.y];
				return true;
			}
			else
			{
				height = 0;
				return false;
			}
		}

		public static bool getNormal(Vector3 position, out Vector3 normal)
		{
			LandscapeCoord coord = new LandscapeCoord(position);
			LandscapeTile tile = getTile(coord);
			if (tile != null)
			{
				normal = tile.data.GetInterpolatedNormal((position.x - (coord.x * TILE_SIZE)) / TILE_SIZE, (position.z - (coord.y * TILE_SIZE)) / TILE_SIZE);
				return true;
			}
			else
			{
				normal = Vector3.up;
				return false;
			}
		}

		public static bool getSplatmapMaterial(Vector3 position, out AssetReference<LandscapeMaterialAsset> materialAsset)
		{
			LandscapeCoord tileCoord = new LandscapeCoord(position);
			SplatmapCoord splatmapCoord = new SplatmapCoord(tileCoord, position);

			return getSplatmapMaterial(tileCoord, splatmapCoord, out materialAsset);
		}

		public static bool getSplatmapMaterial(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, out AssetReference<LandscapeMaterialAsset> materialAsset)
		{
			int layer;
			if (getSplatmapLayer(tileCoord, splatmapCoord, out layer))
			{
				materialAsset = getTile(tileCoord).materials[layer];
				return true;
			}
			else
			{
				materialAsset = AssetReference<LandscapeMaterialAsset>.invalid;
				return false;
			}
		}

		public static bool getSplatmapLayer(Vector3 position, out int layer)
		{
			LandscapeCoord tileCoord = new LandscapeCoord(position);
			SplatmapCoord splatmapCoord = new SplatmapCoord(tileCoord, position);

			return getSplatmapLayer(tileCoord, splatmapCoord, out layer);
		}

		public static bool getSplatmapLayer(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, out int layer)
		{
			LandscapeTile tile = getTile(tileCoord);
			if (tile != null)
			{
				layer = getSplatmapHighestWeightLayerIndex(splatmapCoord, tile.splatmap);
				return true;
			}
			else
			{
				layer = -1;
				return false;
			}
		}

		/// <param name="ignoreLayer">If the highest weight layer is ignoreLayer then the next highest will be returned.</param>
		public static int getSplatmapHighestWeightLayerIndex(SplatmapCoord splatmapCoord, float[,,] currentWeights, int ignoreLayer = -1)
		{
			float highestWeight = -1;
			int highestWeightLayerIndex = -1;

			for (int layer = 0; layer < SPLATMAP_LAYERS; layer++)
			{
				if (layer == ignoreLayer)
				{
					continue;
				}

				if (currentWeights[splatmapCoord.x, splatmapCoord.y, layer] > highestWeight)
				{
					highestWeight = currentWeights[splatmapCoord.x, splatmapCoord.y, layer];
					highestWeightLayerIndex = layer;
				}
			}

			return highestWeightLayerIndex;
		}

		/// <param name="ignoreLayer">If the highest weight layer is ignoreLayer then the next highest will be returned.</param>
		public static int getSplatmapHighestWeightLayerIndex(float[] currentWeights, int ignoreLayer = -1)
		{
			float highestWeight = -1;
			int highestWeightLayerIndex = -1;

			for (int layer = 0; layer < SPLATMAP_LAYERS; layer++)
			{
				if (layer == ignoreLayer)
				{
					continue;
				}

				if (currentWeights[layer] > highestWeight)
				{
					highestWeight = currentWeights[layer];
					highestWeightLayerIndex = layer;
				}
			}

			return highestWeightLayerIndex;
		}

		public static void clearHeightmapTransactions()
		{
			heightmapTransactions.Clear();
		}

		public static void clearSplatmapTransactions()
		{
			splatmapTransactions.Clear();
		}

		public static void clearHoleTransactions()
		{
			holeTransactions.Clear();
		}

		public static bool isPointerInTile(Vector3 worldPosition)
		{
			return getTile(worldPosition) != null;
		}

		public static Vector3 getWorldPosition(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, float height)
		{
			float world_x = (tileCoord.x * TILE_SIZE) + (heightmapCoord.y / (float) HEIGHTMAP_RESOLUTION_MINUS_ONE * TILE_SIZE);
			world_x = Mathf.RoundToInt(world_x);
			float world_y = (-TILE_HEIGHT / 2) + (height * TILE_HEIGHT);
			float world_z = (tileCoord.y * TILE_SIZE) + (heightmapCoord.x / (float) HEIGHTMAP_RESOLUTION_MINUS_ONE * TILE_SIZE);
			world_z = Mathf.RoundToInt(world_z);

			return new Vector3(world_x, world_y, world_z);
		}

		public static Vector3 getWorldPosition(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord)
		{
			float world_x = (tileCoord.x * TILE_SIZE) + (splatmapCoord.y / (float) SPLATMAP_RESOLUTION * TILE_SIZE);
			world_x = Mathf.RoundToInt(world_x) + HALF_SPLATMAP_WORLD_UNIT;
			float world_y;
			float world_z = (tileCoord.y * TILE_SIZE) + (splatmapCoord.x / (float) SPLATMAP_RESOLUTION * TILE_SIZE);
			world_z = Mathf.RoundToInt(world_z) + HALF_SPLATMAP_WORLD_UNIT;

			Vector3 worldPosition = new Vector3(world_x, 0, world_z);
			getWorldHeight(worldPosition, out world_y);
			worldPosition.y = world_y;

			return worldPosition;
		}

		public delegate void LandscapeReadHeightmapHandler(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight);

		public static void readHeightmap(Bounds worldBounds, LandscapeReadHeightmapHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					HeightmapBounds heightmapBounds = new HeightmapBounds(tileCoord, worldBounds);

					for (int heightmap_x = heightmapBounds.min.x; heightmap_x < heightmapBounds.max.x; heightmap_x++)
					{
						for (int heightmap_y = heightmapBounds.min.y; heightmap_y < heightmapBounds.max.y; heightmap_y++)
						{
							HeightmapCoord heightmapCoord = new HeightmapCoord(heightmap_x, heightmap_y);

							float currentHeight = tile.heightmap[heightmap_x, heightmap_y];
							Vector3 worldPosition = getWorldPosition(tileCoord, heightmapCoord, currentHeight);
							callback(tileCoord, heightmapCoord, worldPosition, currentHeight);
						}
					}
				}
			}
		}

		public delegate void LandscapeReadSplatmapHandler(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights);

		public static void readSplatmap(Bounds worldBounds, LandscapeReadSplatmapHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					SplatmapBounds splatmapBounds = new SplatmapBounds(tileCoord, worldBounds);

					for (int splatmap_x = splatmapBounds.min.x; splatmap_x < splatmapBounds.max.x; splatmap_x++)
					{
						for (int splatmap_y = splatmapBounds.min.y; splatmap_y < splatmapBounds.max.y; splatmap_y++)
						{
							SplatmapCoord splatmapCoord = new SplatmapCoord(splatmap_x, splatmap_y);

							for (int layer = 0; layer < SPLATMAP_LAYERS; layer++)
							{
								SPLATMAP_LAYER_BUFFER[layer] = tile.splatmap[splatmap_x, splatmap_y, layer];
							}

							Vector3 worldPosition = getWorldPosition(tileCoord, splatmapCoord);
							callback(tileCoord, splatmapCoord, worldPosition, SPLATMAP_LAYER_BUFFER);
						}
					}
				}
			}
		}

		public delegate float LandscapeWriteHeightmapHandler(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight);

		public static void writeHeightmap(Bounds worldBounds, LandscapeWriteHeightmapHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);

			// Execute callback for all affected heightmap coords:
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
						continue;

					if (!heightmapTransactions.ContainsKey(tileCoord))
					{
						LandscapeHeightmapTransaction heightmapTransaction = new LandscapeHeightmapTransaction(tile);
						Devkit.Transactions.DevkitTransactionManager.recordTransaction(heightmapTransaction);
						heightmapTransactions.Add(tileCoord, heightmapTransaction);
					}

					HeightmapBounds heightmapBounds = new HeightmapBounds(tileCoord, worldBounds);

					for (int heightmap_x = heightmapBounds.min.x; heightmap_x <= heightmapBounds.max.x; heightmap_x++)
					{
						for (int heightmap_y = heightmapBounds.min.y; heightmap_y <= heightmapBounds.max.y; heightmap_y++)
						{
							HeightmapCoord heightmapCoord = new HeightmapCoord(heightmap_x, heightmap_y);

							float currentHeight = tile.heightmap[heightmap_x, heightmap_y];
							Vector3 worldPosition = getWorldPosition(tileCoord, heightmapCoord, currentHeight);
							tile.heightmap[heightmap_x, heightmap_y] = Mathf.Clamp01(callback(tileCoord, heightmapCoord, worldPosition, currentHeight));
						}
					}
				}
			}

			// Stitch seams to prevent holes between tiles:
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
						continue;

					// Note it may seem obvious to convert the for loop to < rather than <=,
					// but these < are for the cases like x == max.x while y < max.y
					if (tile_x < tileBounds.max.x)
					{
						LandscapeCoord nextTileCoordX = new LandscapeCoord(tile_x + 1, tile_y);
						LandscapeTile nextTileX = getTile(nextTileCoordX);

						if (nextTileX != null)
						{
							// Heightmap coords are switched from landscape coords, so the tile X border is along heightmap X.
							for (int heightmapCoord = 0; heightmapCoord <= HEIGHTMAP_RESOLUTION_MINUS_ONE; ++heightmapCoord)
							{
								tile.heightmap[heightmapCoord, HEIGHTMAP_RESOLUTION_MINUS_ONE] = nextTileX.heightmap[heightmapCoord, 0];
							}
						}
					}

					if (tile_y < tileBounds.max.y)
					{
						LandscapeCoord nextTileCoordY = new LandscapeCoord(tile_x, tile_y + 1);
						LandscapeTile nextTileY = getTile(nextTileCoordY);

						if (nextTileY != null)
						{
							// Heightmap coords are switched from landscape coords, so the tile Y border is along heightmap Y.
							for (int heightmapCoord = 0; heightmapCoord <= HEIGHTMAP_RESOLUTION_MINUS_ONE; ++heightmapCoord)
							{
								tile.heightmap[HEIGHTMAP_RESOLUTION_MINUS_ONE, heightmapCoord] = nextTileY.heightmap[0, heightmapCoord];
							}
						}
					}

					// Literal corner case! Stitch up the single vertex in the corner.
					if (tile_x < tileBounds.max.x && tile_y < tileBounds.max.y)
					{
						LandscapeCoord nextTileCoordXY = new LandscapeCoord(tile_x + 1, tile_y + 1);
						LandscapeTile nextTileXY = getTile(nextTileCoordXY);

						if (nextTileXY != null)
						{
							tile.heightmap[HEIGHTMAP_RESOLUTION_MINUS_ONE, HEIGHTMAP_RESOLUTION_MINUS_ONE] = nextTileXY.heightmap[0, 0];
						}
					}
				}
			}

			// Apply heights to the underlying Unity terrains:
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
						continue;

					tile.SetHeightsDelayLOD();
				}
			}

			LevelHierarchy.MarkDirty();
		}

		public delegate void LandscapeWriteSplatmapHandler(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights);

		public static void writeSplatmap(Bounds worldBounds, LandscapeWriteSplatmapHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					if (!splatmapTransactions.ContainsKey(tileCoord))
					{
						LandscapeSplatmapTransaction splatmapTransaction = new LandscapeSplatmapTransaction(tile);
						Devkit.Transactions.DevkitTransactionManager.recordTransaction(splatmapTransaction);
						splatmapTransactions.Add(tileCoord, splatmapTransaction);
					}

					SplatmapBounds splatmapBounds = new SplatmapBounds(tileCoord, worldBounds);

					for (int splatmap_x = splatmapBounds.min.x; splatmap_x <= splatmapBounds.max.x; splatmap_x++)
					{
						for (int splatmap_y = splatmapBounds.min.y; splatmap_y <= splatmapBounds.max.y; splatmap_y++)
						{
							SplatmapCoord splatmapCoord = new SplatmapCoord(splatmap_x, splatmap_y);

							for (int layer = 0; layer < SPLATMAP_LAYERS; layer++)
							{
								SPLATMAP_LAYER_BUFFER[layer] = tile.splatmap[splatmap_x, splatmap_y, layer];
							}

							Vector3 worldPosition = getWorldPosition(tileCoord, splatmapCoord);
							callback(tileCoord, splatmapCoord, worldPosition, SPLATMAP_LAYER_BUFFER);

							for (int layer = 0; layer < SPLATMAP_LAYERS; layer++)
							{
								tile.splatmap[splatmap_x, splatmap_y, layer] = Mathf.Clamp01(SPLATMAP_LAYER_BUFFER[layer]);
							}
						}
					}

					tile.data.SetAlphamaps(0, 0, tile.splatmap);
				}
			}

			LevelHierarchy.MarkDirty();
		}

		public delegate bool LandscapeWriteHolesHandler(Vector3 worldPosition, bool currentlyVisible);

		public static void writeHoles(Bounds worldBounds, LandscapeWriteHolesHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					if (!holeTransactions.ContainsKey(tileCoord))
					{
						LandscapeHoleTransaction holeTransaction = new LandscapeHoleTransaction(tile);
						Devkit.Transactions.DevkitTransactionManager.recordTransaction(holeTransaction);
						holeTransactions.Add(tileCoord, holeTransaction);
					}

					SplatmapBounds splatmapBounds = new SplatmapBounds(tileCoord, worldBounds);

					for (int splatmap_x = splatmapBounds.min.x; splatmap_x <= splatmapBounds.max.x; splatmap_x++)
					{
						for (int splatmap_y = splatmapBounds.min.y; splatmap_y <= splatmapBounds.max.y; splatmap_y++)
						{
							SplatmapCoord splatmapCoord = new SplatmapCoord(splatmap_x, splatmap_y);

							Vector3 worldPosition = getWorldPosition(tileCoord, splatmapCoord);
							bool wasVisible = tile.holes[splatmap_x, splatmap_y];
							bool nowVisible = callback(worldPosition, wasVisible);
							tile.holes[splatmap_x, splatmap_y] = nowVisible;
							tile.hasAnyHolesData |= nowVisible != wasVisible;
						}
					}

					tile.SetHoles();
				}
			}

			LevelHierarchy.MarkDirty();
		}

		public delegate void LandscapeGetHeightmapVerticesHandler(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition);

		/// <summary>
		/// Appends heightmap vertices to points list.
		/// </summary>
		public static void getHeightmapVertices(Bounds worldBounds, LandscapeGetHeightmapVerticesHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					HeightmapBounds heightmapBounds = new HeightmapBounds(tileCoord, worldBounds);

					for (int heightmap_x = heightmapBounds.min.x; heightmap_x <= heightmapBounds.max.x; heightmap_x++)
					{
						for (int heightmap_y = heightmapBounds.min.y; heightmap_y <= heightmapBounds.max.y; heightmap_y++)
						{
							HeightmapCoord heightmapCoord = new HeightmapCoord(heightmap_x, heightmap_y);

							float currentHeight = tile.heightmap[heightmap_x, heightmap_y];
							Vector3 worldPosition = getWorldPosition(tileCoord, heightmapCoord, currentHeight);
							callback(tileCoord, heightmapCoord, worldPosition);
						}
					}
				}
			}
		}

		public delegate void LandscapeGetSplatmapVerticesHandler(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition);

		/// <summary>
		/// Appends heightmap vertices to points list.
		/// </summary>
		public static void getSplatmapVertices(Bounds worldBounds, LandscapeGetSplatmapVerticesHandler callback)
		{
			if (callback == null)
			{
				return;
			}

			LandscapeBounds tileBounds = new LandscapeBounds(worldBounds);
			for (int tile_x = tileBounds.min.x; tile_x <= tileBounds.max.x; tile_x++)
			{
				for (int tile_y = tileBounds.min.y; tile_y <= tileBounds.max.y; tile_y++)
				{
					LandscapeCoord tileCoord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					SplatmapBounds splatmapBounds = new SplatmapBounds(tileCoord, worldBounds);

					for (int splatmap_x = splatmapBounds.min.x; splatmap_x <= splatmapBounds.max.x; splatmap_x++)
					{
						for (int splatmap_y = splatmapBounds.min.y; splatmap_y <= splatmapBounds.max.y; splatmap_y++)
						{
							SplatmapCoord splatmapCoord = new SplatmapCoord(splatmap_x, splatmap_y);

							Vector3 worldPosition = getWorldPosition(tileCoord, splatmapCoord);
							callback(tileCoord, splatmapCoord, worldPosition);
						}
					}
				}
			}
		}

		public static void applyLOD()
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.SyncDelayedLOD();
			}
		}

		/// <summary>
		/// Call this after you're done adding new tiles.
		/// </summary>
		public static void linkNeighbors()
		{
			if (Dedicator.IsDedicatedServer)
			{
				// Nelson 2023-08-07: dedicated server is failing here after updating to Unity 2021 LTS
				// because it internally tries to do some Graphics.Blit operations.
				return;
			}

			// 2019-10-16: For some reason instancing gets reset unless we re-enable:
			bool drawInstanced = SystemInfo.supportsInstancing;
			foreach (LandscapeTile tile in tiles.Values)
			{
				tile.terrain.drawInstanced = drawInstanced;
			}

			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;

				LandscapeTile leftTile = getTile(new LandscapeCoord(tile.coord.x - 1, tile.coord.y));
				LandscapeTile topTile = getTile(new LandscapeCoord(tile.coord.x, tile.coord.y + 1));
				LandscapeTile rightTile = getTile(new LandscapeCoord(tile.coord.x + 1, tile.coord.y));
				LandscapeTile bottomTile = getTile(new LandscapeCoord(tile.coord.x, tile.coord.y - 1));

				Terrain leftTerrain = leftTile == null ? null : leftTile.terrain;
				Terrain topTerrain = topTile == null ? null : topTile.terrain;
				Terrain rightTerrain = rightTile == null ? null : rightTile.terrain;
				Terrain bottomTerrain = bottomTile == null ? null : bottomTile.terrain;

				/*
				UnturnedLog.info("{0} - Left: {1} Top: {2} Right: {3} Bottom: {4}"
					, tile.coord
					, (leftTile == null ? "None" : leftTile.coord.ToString())
					, (topTile == null ? "None" : topTile.coord.ToString())
					, (rightTile == null ? "None" : rightTile.coord.ToString())
					, (bottomTile == null ? "None" : bottomTile.coord.ToString()));
				*/

				tile.terrain.SetNeighbors(leftTerrain, topTerrain, rightTerrain, bottomTerrain);
			}

			foreach (LandscapeTile tile in tiles.Values)
			{
				tile.terrain.Flush();
			}
		}

		/// <summary>
		/// Call this to sync a new tile up with nearby tiles.
		/// </summary>
		public static void reconcileNeighbors(LandscapeTile tile)
		{
			LandscapeTile leftTile = getTile(new LandscapeCoord(tile.coord.x - 1, tile.coord.y));
			if (leftTile != null)
			{
				for (int index = 0; index < HEIGHTMAP_RESOLUTION; index++)
				{
					tile.heightmap[index, 0] = leftTile.heightmap[index, HEIGHTMAP_RESOLUTION - 1];
				}
			}

			LandscapeTile topTile = getTile(new LandscapeCoord(tile.coord.x, tile.coord.y - 1));
			if (topTile != null)
			{
				for (int index = 0; index < HEIGHTMAP_RESOLUTION; index++)
				{
					tile.heightmap[0, index] = topTile.heightmap[HEIGHTMAP_RESOLUTION - 1, index];
				}
			}

			LandscapeTile rightTile = getTile(new LandscapeCoord(tile.coord.x + 1, tile.coord.y));
			if (rightTile != null)
			{
				for (int index = 0; index < HEIGHTMAP_RESOLUTION; index++)
				{
					tile.heightmap[index, HEIGHTMAP_RESOLUTION - 1] = rightTile.heightmap[index, 0];
				}
			}

			LandscapeTile bottomTile = getTile(new LandscapeCoord(tile.coord.x, tile.coord.y + 1));
			if (bottomTile != null)
			{
				for (int index = 0; index < HEIGHTMAP_RESOLUTION; index++)
				{
					tile.heightmap[HEIGHTMAP_RESOLUTION - 1, index] = bottomTile.heightmap[0, index];
				}
			}

			tile.SetHeightsDelayLOD();
		}

		public static LandscapeTile addTile(LandscapeCoord coord)
		{
			if (instance == null)
			{
				UnturnedLog.info("Adding default landscape to level");
				GameObject terrainGameObject = new GameObject(); // renames itself to Landscape
				Landscape landscape = terrainGameObject.AddComponent<Landscape>();
				LevelHierarchy.AssignInstanceIdAndMarkDirty(landscape);
			}

			if (tiles.ContainsKey(coord))
			{
				return null;
			}

			LandscapeTile tile = new LandscapeTile(coord);
			tile.enable();
			tile.applyGraphicsSettings();
			tiles.Add(coord, tile);
			return tile;
		}

		protected static void clearTiles()
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.disable();
			}
			tiles.Clear();
		}

		public static void CopyLayersToAllTiles(LandscapeTile source)
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile destination = pair.Value;
				if (destination != source)
				{
					for (int layerIndex = 0; layerIndex < SPLATMAP_LAYERS; ++layerIndex)
					{
						destination.materials[layerIndex] = source.materials[layerIndex];
					}
					destination.updatePrototypes();
				}
			}
		}

		public static LandscapeTile getOrAddTile(Vector3 worldPosition)
		{
			LandscapeCoord tileCoord = new LandscapeCoord(worldPosition);
			return getOrAddTile(tileCoord);
		}

		public static LandscapeTile getTile(Vector3 worldPosition)
		{
			LandscapeCoord tileCoord = new LandscapeCoord(worldPosition);
			return getTile(tileCoord);
		}

		public static LandscapeTile getOrAddTile(LandscapeCoord coord)
		{
			LandscapeTile tile;
			if (!tiles.TryGetValue(coord, out tile))
			{
				tile = addTile(coord);
			}
			return tile;
		}

		public static LandscapeTile getTile(LandscapeCoord coord)
		{
			LandscapeTile tile;
			tiles.TryGetValue(coord, out tile);
			return tile;
		}

		public static bool removeTile(LandscapeCoord coord)
		{
			LandscapeTile tile;
			if (!tiles.TryGetValue(coord, out tile))
			{
				return false;
			}

			tile.disable();
			Destroy(tile.gameObject);
			tiles.Remove(coord);
			return true;
		}

		public override void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();

			int tileCount = reader.readArrayLength("Tiles");
			if (instance != this)
			{
				UnturnedLog.warn("Level contains multiple Landscapes. Ignoring {0} tile(s) with instance ID: {1}", tileCount, instanceID);
				return;
			}

			UnturnedLog.info("Loading {0} landscape tiles", tileCount);

			for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
			{
				reader.readArrayIndex(tileIndex);

				LandscapeTile tile = new LandscapeTile(LandscapeCoord.ZERO);
				tile.enable();
				tile.applyGraphicsSettings();
				tile.read(reader);

				if (tiles.ContainsKey(tile.coord))
				{
					UnturnedLog.error("Duplicate landscape coord read: " + tile.coord);
				}
				else
				{
					tiles.Add(tile.coord, tile);
				}
			}

			linkNeighbors();
			applyLOD();
		}

		public override void write(IFormattedFileWriter writer)
		{
			writer.beginObject();
			writer.beginArray("Tiles");

			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				writer.writeValue(tile);
			}

			writer.endArray();
			writer.endObject();
		}

		protected void triggerLandscapeLoaded()
		{
			loaded?.Invoke();
		}

		protected void handleGraphicsSettingsApplied()
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.applyGraphicsSettings();
			}
		}

		partial void SubscribePlanarReflectionEvents();
		partial void UnsubscribePlanarReflectionEvents();

		protected void handlePlanarReflectionPreRender()
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.terrain.basemapDistance = 0;
			}
		}

		protected void handlePlanarReflectionPostRender()
		{
			float basemapDistance = GraphicsSettings.terrainBasemapDistance;
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.terrain.basemapDistance = basemapDistance;
			}
		}

		/// <summary>
		/// Capturing ortho view of map, so we raise the terrain to max quality.
		/// </summary>
		protected void onSatellitePreCapture()
		{
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.terrain.basemapDistance = 8192;
				tile.terrain.heightmapPixelError = 1;
			}
		}

		/// <summary>
		/// Finished capturing ortho view of map, so we restore the terrain to preferred quality.
		/// </summary>
		protected void onSatellitePostCapture()
		{
			float basemapDistance = GraphicsSettings.terrainBasemapDistance;
			float heightmapPixelError = GraphicsSettings.terrainHeightmapPixelError;
			foreach (KeyValuePair<LandscapeCoord, LandscapeTile> pair in tiles)
			{
				LandscapeTile tile = pair.Value;
				tile.terrain.basemapDistance = basemapDistance;
				tile.terrain.heightmapPixelError = heightmapPixelError;
			}
		}

		protected void OnEnable()
		{
			LevelHierarchy.addItem(this);
		}

		protected void OnDisable()
		{
			LevelHierarchy.removeItem(this);
		}

		protected void Awake()
		{
			name = "Landscape";
			gameObject.layer = LayerMasks.GROUND;

			if (instance == null)
			{
				instance = this;
				clearTiles();
				_disableHoleColliders = false;
				HighlightHoles = false;

				if (Level.isEditor)
				{
					LandscapeHeightmapCopyPool.warmup(Devkit.Transactions.DevkitTransactionManager.historyLength);
					LandscapeSplatmapCopyPool.warmup(Devkit.Transactions.DevkitTransactionManager.historyLength);
					LandscapeHoleCopyPool.warmup(Devkit.Transactions.DevkitTransactionManager.historyLength);
				}

				GraphicsSettings.graphicsSettingsApplied += handleGraphicsSettingsApplied;
				SubscribePlanarReflectionEvents();
				Level.bindSatelliteCaptureInEditor(onSatellitePreCapture, onSatellitePostCapture);
			}
		}

		private bool shouldTriggerLandscapeLoaded = true;

		protected void Start()
		{
			if (instance == this && shouldTriggerLandscapeLoaded)
			{
				triggerLandscapeLoaded();
			}
		}

		protected void OnDestroy()
		{
			if (instance == this)
			{
				GraphicsSettings.graphicsSettingsApplied -= handleGraphicsSettingsApplied;
				UnsubscribePlanarReflectionEvents();
				Level.unbindSatelliteCapture(onSatellitePreCapture, onSatellitePostCapture);

				instance = null;
				clearTiles();
				_disableHoleColliders = false;
				HighlightHoles = false;

				LandscapeHeightmapCopyPool.empty();
				LandscapeSplatmapCopyPool.empty();
				LandscapeHoleCopyPool.empty();
			}
		}

		internal IEnumerator AutoConvertLegacyTerrain()
		{
			// Wait until conversion finishes to notify other systems.
			shouldTriggerLandscapeLoaded = false;

			int size = Level.size;

			// e.g. on Medium map this is 2, Large is 4
			int tilesWithinBounds = size / TILE_SIZE_INT;

			// e.g. on Large map this is only 1 extra tile outside bounds on each side
			int halfTiles = (tilesWithinBounds / 2) + 1;

			for (int tile_x = -halfTiles; tile_x < halfTiles; tile_x++)
			{
				for (int tile_y = -halfTiles; tile_y < halfTiles; tile_y++)
				{
					LandscapeCoord coord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getOrAddTile(coord);

					UnturnedLog.info("Auto convert heightmap {0}", coord);
					tile.convertLegacyHeightmap();
					yield return null;

					if (LevelGround.doesLegacyDataIncludeSplatmapWeights)
					{
						UnturnedLog.info("Auto convert splatmap {0}", coord);
						tile.convertLegacySplatmap();
						yield return null;

						for (int layerIndex = 0; layerIndex < SPLATMAP_LAYERS; layerIndex++)
						{
							tile.materials[layerIndex] = LevelGround.legacyMaterialGuids[layerIndex];
						}
					}

					tile.updatePrototypes();
					yield return null;
				}
			}

			Foliage.FoliageSystem.CreateInLevelIfMissing();

			triggerLandscapeLoaded();
		}

		private IEnumerator convertLegacyTerrainImpl(InspectableList<AssetReference<LandscapeMaterialAsset>> materials)
		{
			yield return null;

			int size = Level.size;
			int tiles = size / TILE_SIZE_INT;

			for (int tile_x = -tiles; tile_x < tiles; tile_x++)
			{
				for (int tile_y = -tiles; tile_y < tiles; tile_y++)
				{
					LandscapeCoord coord = new LandscapeCoord(tile_x, tile_y);
					LandscapeTile tile = getOrAddTile(coord);

					UnturnedLog.info("Convert heightmap {0}", coord);
					tile.convertLegacyHeightmap();
					yield return null;

					UnturnedLog.info("Convert splatmap {0}", coord);
					tile.convertLegacySplatmap();
					yield return null;

					for (int layer = 0; layer < SPLATMAP_LAYERS; layer++)
					{
						tile.materials[layer] = materials[layer];
					}

					UnturnedLog.info("Convert prototypes {0}", coord);
					tile.updatePrototypes();
					yield return null;
				}
			}

			GameObject gameObject = new GameObject();
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject.transform.localScale = new Vector3(size, Landscape.TILE_HEIGHT, size);
			Foliage.FoliageVolume additiveVolume = gameObject.AddComponent<Foliage.FoliageVolume>();
			additiveVolume.mode = Foliage.FoliageVolume.EFoliageVolumeMode.ADDITIVE;
		}

		private bool hasConverted = false;

		public void convertLegacyTerrain(InspectableList<AssetReference<LandscapeMaterialAsset>> materials)
		{
			if (hasConverted) return;
			hasConverted = true;
			StartCoroutine(convertLegacyTerrainImpl(materials));
		}

		/// <summary>
		/// Nelson 2025-03-10: I want to experiment whether this fixes a strange terrain hole painting bug (public issue
		/// #4851) without potentially introducing crashes for other players. (Per an earlier, undated comment we'd
		/// run into a SetHolesDelayLOD-related crash in 2019 LTS.)
		/// </summary>
		internal static bool ShouldUseSetHolesDelayLOD
		{
			get => !clNoSetHolesDelayLod;
		}

		internal static CommandLineFlag clNoSetHolesDelayLod = new CommandLineFlag(false, "-NoSetHolesDelayLOD");
	}
}
