////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;

using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Framework.Foliage
{
	public delegate void FoliageSystemPreBakeHandler();
	public delegate void FoliageSystemPreBakeTileHandler(FoliageBakeSettings bakeSettings, FoliageTile foliageTile);
	public delegate void FoliageSystemPostBakeTileHandler(FoliageBakeSettings bakeSettings, FoliageTile foliageTile);
	public delegate void FoliageSystemGlobalBakeHandler();
	public delegate void FoliageSystemLocalBakeHandler(Vector3 localPosition);
	public delegate void FoliageSystemPostBakeHandler();

	internal struct FoliageInstancingBatchConfig : System.IEquatable<FoliageInstancingBatchConfig>
	{
		public Mesh mesh;
		public Material material;
		public bool castShadows;
		public int hashCode;

		public override int GetHashCode()
		{
			return hashCode;
		}

		public bool Equals(FoliageInstancingBatchConfig other)
		{
			return mesh == other.mesh && material == other.material && castShadows == other.castShadows;
		}

		public override string ToString()
		{
			return $"(Mesh: {mesh} Material: {material} Shadows: {castShadows})";
		}

		public FoliageInstancingBatchConfig(Mesh mesh, Material material, bool castShadows)
		{
			this.mesh = mesh;
			this.material = material;
			this.castShadows = castShadows;
			hashCode = System.HashCode.Combine(mesh, material, castShadows);
		}
	}

	internal struct FoliageInstancingBatchData
	{
		public Matrix4x4[] list;
		public int count;
	}

	public class FoliageSystem : DevkitHierarchyItemBase
	{
		public static float TILE_SIZE = 32;
		public static int TILE_SIZE_INT = 32;
		public static int SPLATMAP_RESOLUTION_PER_TILE = 8;

		public static FoliageSystem instance
		{
			get;
			private set;
		}

		public static List<IFoliageSurface> surfaces
		{
			get;
			private set;
		}

		public static event FoliageSystemPreBakeHandler preBake;
		public static event FoliageSystemPreBakeTileHandler preBakeTile;
		public static event FoliageSystemPostBakeTileHandler postBakeTile;
		public static event FoliageSystemGlobalBakeHandler globalBake;
		public static event FoliageSystemLocalBakeHandler localBake;
		public static event FoliageSystemPostBakeHandler postBake;

		public static int bakeQueueProgress => bakeQueueTotal - bakeQueue.Count;

		public static int bakeQueueTotal
		{
			get;
			private set;
		}

		/// <summary>
		/// Settings configured when starting the bake.
		/// </summary>
		public static FoliageBakeSettings bakeSettings
		{
			get;
			private set;
		}

		protected static Dictionary<FoliageCoord, FoliageTile> previousFrameRelevantTiles = new Dictionary<FoliageCoord, FoliageTile>();
		protected static Dictionary<FoliageCoord, FoliageTile> relevantTiles = new Dictionary<FoliageCoord, FoliageTile>();
		protected static Dictionary<FoliageCoord, FoliageTile> tiles = new Dictionary<FoliageCoord, FoliageTile>();

		/// <summary>
		/// Implementation of tile data storage.
		/// </summary>
		protected static IFoliageStorage storage = null;

		protected static Queue<KeyValuePair<FoliageTile, List<IFoliageSurface>>> bakeQueue = new Queue<KeyValuePair<FoliageTile, List<IFoliageSurface>>>();
		protected static FoliageSystemPostBakeHandler bakeEnd;
		protected static Vector3 bakeLocalPosition;

		private static Plane[] mainCameraFrustumPlanes = new Plane[6];
		private static Plane[] focusCameraFrustumPlanes = new Plane[6];

		public static Vector3 focusPosition;
		public static bool isFocused;
		public static Camera focusCamera;

		// These flags are used to improve terrain editing visibility.
		public bool hiddenByHeightEditor;
		public bool hiddenByMaterialEditor;

		/// <summary>
		/// Nelson 2025-04-22: instanced foliage rendering is a decent chunk of CPU time. In retrospect this seems like
		/// an obvious optimization: Graphics.DrawMeshInstanced accepts up to 1023* instances per call. Each tile
		/// groups instances in lists of up to 1023*, but often isn't that high. Now, we collect instances until we
		/// hit the 1023* limit. This is particularly useful for sparse variants like colored flowers.
		/// With a consistent camera transform ("/copycameratransform") on an upcoming map remaster I went from between
		/// 0.72-0.8 ms on my PC to 0.55-0.6 ms!
		/// *Nelson 2025-07-14: refer to NON_UNIFORM_SCALE_INSTANCES_PER_BATCH.
		/// </summary>
		private static Dictionary<FoliageInstancingBatchConfig, FoliageInstancingBatchData> batches = new Dictionary<FoliageInstancingBatchConfig, FoliageInstancingBatchData>();
		private static Stack<Matrix4x4[]> activeMatrixLists = new Stack<Matrix4x4[]>();
		private static Stack<Matrix4x4[]> matrixListPool = new Stack<Matrix4x4[]>();

		public static void CreateInLevelIfMissing()
		{
			if (instance == null)
			{
				UnturnedLog.info("Adding default foliage system to level");
				GameObject foliageGameObject = new GameObject(); // renames itself to Foliage System
				FoliageSystem foliageSystem = foliageGameObject.AddComponent<FoliageSystem>();
				LevelHierarchy.AssignInstanceIdAndMarkDirty(foliageSystem);

				// Only create additive volume if foliage system was also missing.
				// Otherwise we might be creating a volume that the editor keeps trying to remove.
				if (FoliageVolumeManager.Get().additiveVolumes.Count < 1)
				{
					UnturnedLog.info("Adding default additive foliage volume to level");
					GameObject volumeGameObject = new GameObject();
					volumeGameObject.transform.position = Vector3.zero;
					volumeGameObject.transform.rotation = Quaternion.identity;
					volumeGameObject.transform.localScale = new Vector3(Level.size, Landscapes.Landscape.TILE_HEIGHT, Level.size);
					FoliageVolume additiveVolume = volumeGameObject.AddComponent<FoliageVolume>();
					LevelHierarchy.AssignInstanceIdAndMarkDirty(additiveVolume);
					additiveVolume.mode = Foliage.FoliageVolume.EFoliageVolumeMode.ADDITIVE;
				}
			}
		}

		public static void addSurface(IFoliageSurface surface)
		{
			surfaces.Add(surface);
		}

		public static void removeSurface(IFoliageSurface surface)
		{
			surfaces.Remove(surface);
		}

		[System.Obsolete]
		public static void addCut(IShapeVolume cut)
		{

		}

		internal static void AddCut(FoliageCut cut)
		{
			for (int tile_x = cut.foliageBounds.min.x; tile_x <= cut.foliageBounds.max.x; tile_x++)
			{
				for (int tile_y = cut.foliageBounds.min.y; tile_y <= cut.foliageBounds.max.y; tile_y++)
				{
					FoliageCoord tileCoord = new FoliageCoord(tile_x, tile_y);
					FoliageTile tile = getOrAddTile(tileCoord);

					tile.AddCut(cut);
				}
			}
		}

		internal static void RemoveCut(FoliageCut cut)
		{
			for (int tile_x = cut.foliageBounds.min.x; tile_x <= cut.foliageBounds.max.x; tile_x++)
			{
				for (int tile_y = cut.foliageBounds.min.y; tile_y <= cut.foliageBounds.max.y; tile_y++)
				{
					FoliageCoord tileCoord = new FoliageCoord(tile_x, tile_y);
					FoliageTile tile = getOrAddTile(tileCoord);

					tile.RemoveCut(cut);
				}
			}
		}

		private static Dictionary<FoliageTile, List<IFoliageSurface>> getTileSurfacePairs()
		{
			Dictionary<FoliageTile, List<IFoliageSurface>> tileSurfacesPairs = new Dictionary<FoliageTile, List<IFoliageSurface>>();

			foreach (KeyValuePair<FoliageCoord, FoliageTile> pair in tiles)
			{
				FoliageTile tile = pair.Value;

				if (FoliageVolumeManager.Get().IsTileBakeable(tile))
				{
					tileSurfacesPairs.Add(tile, new List<IFoliageSurface>());
				}
			}

			foreach (IFoliageSurface surface in surfaces)
			{
				if (!surface.IsValidFoliageSurface)
					continue;

				FoliageBounds foliageBounds = surface.getFoliageSurfaceBounds();

				for (int tile_x = foliageBounds.min.x; tile_x <= foliageBounds.max.x; tile_x++)
				{
					for (int tile_y = foliageBounds.min.y; tile_y <= foliageBounds.max.y; tile_y++)
					{
						FoliageCoord tileCoord = new FoliageCoord(tile_x, tile_y);
						FoliageTile tile = getOrAddTile(tileCoord);

						if (FoliageVolumeManager.Get().IsTileBakeable(tile))
						{
							List<IFoliageSurface> list;
							if (!tileSurfacesPairs.TryGetValue(tile, out list))
							{
								list = new List<IFoliageSurface>();
								tileSurfacesPairs.Add(tile, list);
							}

							list.Add(surface);
						}
					}
				}
			}

			return tileSurfacesPairs;
		}

		private static void bakePre()
		{
			preBake?.Invoke();

			bakeQueue.Clear();
		}

		public static void bakeGlobal(FoliageBakeSettings bakeSettings)
		{
			CreateInLevelIfMissing();
			FoliageSystem.bakeSettings = bakeSettings;

			bakePre();
			bakeGlobalBegin();
		}

		private static void bakeGlobalBegin()
		{
			Dictionary<FoliageTile, List<IFoliageSurface>> tileSurfacesPairs = getTileSurfacePairs();
			foreach (KeyValuePair<FoliageTile, List<IFoliageSurface>> tileSurfacePair in tileSurfacesPairs)
			{
				bakeQueue.Enqueue(tileSurfacePair);
			}
			bakeQueueTotal = bakeQueue.Count;

			bakeEnd = bakeGlobalEnd;
			bakeEnd();
		}

		private static void bakeGlobalEnd()
		{
			globalBake?.Invoke();

			bakePost();
		}

		public static void bakeLocal(FoliageBakeSettings bakeSettings)
		{
			CreateInLevelIfMissing();
			FoliageSystem.bakeSettings = bakeSettings;

			bakePre();
			bakeLocalBegin();
		}

		private static void bakeLocalBegin()
		{
			bakeLocalPosition = MainCamera.instance.transform.position;

			int tileBakeDistance = 6;
			int sqrTileBakeDistance = tileBakeDistance * tileBakeDistance;

			FoliageCoord cameraCoord = new FoliageCoord(bakeLocalPosition);
			Dictionary<FoliageTile, List<IFoliageSurface>> tileSurfacesPairs = getTileSurfacePairs();

			for (int tile_x = -tileBakeDistance; tile_x <= tileBakeDistance; tile_x++)
			{
				for (int tile_y = -tileBakeDistance; tile_y <= tileBakeDistance; tile_y++)
				{
					int sqrDistance = (tile_x * tile_x) + (tile_y * tile_y);
					if (sqrDistance > sqrTileBakeDistance)
					{
						continue;
					}

					FoliageCoord tileCoord = new FoliageCoord(cameraCoord.x + tile_x, cameraCoord.y + tile_y);
					FoliageTile tile = getTile(tileCoord);

					if (tile == null)
					{
						continue;
					}

					List<IFoliageSurface> list;
					if (tileSurfacesPairs.TryGetValue(tile, out list))
					{
						KeyValuePair<FoliageTile, List<IFoliageSurface>> tileSurfacePair = new KeyValuePair<FoliageTile, List<IFoliageSurface>>(tile, list);
						bakeQueue.Enqueue(tileSurfacePair);
					}
				}
			}
			bakeQueueTotal = bakeQueue.Count;

			bakeEnd = bakeLocalEnd;
			bakeEnd();
		}

		private static void bakeLocalEnd()
		{
			localBake?.Invoke(bakeLocalPosition);

			bakePost();
		}

		public static void bakeCancel()
		{
			if (bakeQueue.Count == 0)
			{
				return;
			}

			bakeQueue.Clear();
			bakeEnd();
		}

		private static void bakePreTile(FoliageBakeSettings bakeSettings, FoliageTile foliageTile)
		{
			if (!bakeSettings.bakeInstancesMeshes)
			{
				return;
			}

			if (bakeSettings.bakeApplyScale)
			{
				foliageTile.applyScale();
			}
			else
			{
				foliageTile.clearGeneratedInstances();
			}
		}

		private static void bake(FoliageTile tile, List<IFoliageSurface> list)
		{
			bakePreTile(bakeSettings, tile);

			preBakeTile?.Invoke(bakeSettings, tile);

			if (!bakeSettings.bakeApplyScale)
			{
				foreach (IFoliageSurface surface in list)
				{
					if (surface.IsValidFoliageSurface)
					{
						surface.bakeFoliageSurface(bakeSettings, tile);
					}
				}
			}

			postBakeTile?.Invoke(bakeSettings, tile);
		}

		private static void bakePost()
		{
			if (LevelHierarchy.instance != null)
			{
				LevelHierarchy.instance.isDirty = true;
			}

			postBake?.Invoke();
		}

		public static void addInstance(AssetReference<FoliageInstancedMeshInfoAsset> assetReference, Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
		{
			FoliageTile tile = getOrAddTile(position);

			Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
			tile.addInstance(new FoliageInstanceGroup(assetReference, matrix, clearWhenBaked));
		}

		protected static void shutdownStorage()
		{
			if (storage != null)
			{
				storage.Shutdown();
				storage = null;
			}
		}

		protected static void clearAndReleaseTiles()
		{
			foreach (KeyValuePair<FoliageCoord, FoliageTile> pair in tiles)
			{
				pair.Value.clearAndReleaseInstances();
			}
			tiles.Clear();
		}

#if !DEDICATED_SERVER
		/// <summary>
		/// 2022-04-26: drawTiles previously looped over a square [-N, +N] from the upper-left to the bottom-right,
		/// and each tile checked radial distance. We can improve over this by pre-computing the radial offsets and
		/// starting from the center to improve responsiveness. N is [1, 5]
		/// </summary>
		private static readonly FoliageCoord[][] DRAW_OFFSETS = new FoliageCoord[6][]
		{
			// 0 draw distance
			new FoliageCoord[0],
			
			// 1 draw distance
			// X X X
			// X X X
			// X X X
			new FoliageCoord[9]
			{
				new FoliageCoord(0, 0),
				new FoliageCoord(1, 0),
				new FoliageCoord(1, 1),
				new FoliageCoord(0, 1),
				new FoliageCoord(-1, 1),
				new FoliageCoord(-1, 0),
				new FoliageCoord(-1, -1),
				new FoliageCoord(0, -1),
				new FoliageCoord(1, -1),
			},

			// 2 draw distance
			//   X X X
			// X X X X X
			// X X X X X
			// X X X X X
			//   X X X
			new FoliageCoord[21]
			{
				// 3x3 square
				new FoliageCoord(0, 0),
				new FoliageCoord(1, 0),
				new FoliageCoord(1, 1),
				new FoliageCoord(0, 1),
				new FoliageCoord(-1, 1),
				new FoliageCoord(-1, 0),
				new FoliageCoord(-1, -1),
				new FoliageCoord(0, -1),
				new FoliageCoord(1, -1),

				// 5x5 edge
				new FoliageCoord(2, -1),
				new FoliageCoord(2, 0),
				new FoliageCoord(2, 1),
				new FoliageCoord(1, 2),
				new FoliageCoord(0, 2),
				new FoliageCoord(-1, 2),
				new FoliageCoord(-2, 1),
				new FoliageCoord(-2, 0),
				new FoliageCoord(-2, -1),
				new FoliageCoord(-1, -2),
				new FoliageCoord(0, -2),
				new FoliageCoord(1, -2),
			},

			// 3 draw distance
			//   X X X X X
			// X X X X X X X
			// X X X X X X X
			// X X X X X X X
			// X X X X X X X
			// X X X X X X X
			//   X X X X X
			new FoliageCoord[45]
			{
				// 3x3 square
				new FoliageCoord(0, 0),
				new FoliageCoord(1, 0),
				new FoliageCoord(1, 1),
				new FoliageCoord(0, 1),
				new FoliageCoord(-1, 1),
				new FoliageCoord(-1, 0),
				new FoliageCoord(-1, -1),
				new FoliageCoord(0, -1),
				new FoliageCoord(1, -1),

				// 5x5 square
				new FoliageCoord(2, -1),
				new FoliageCoord(2, 0),
				new FoliageCoord(2, 1),
				new FoliageCoord(2, 2),
				new FoliageCoord(1, 2),
				new FoliageCoord(0, 2),
				new FoliageCoord(-1, 2),
				new FoliageCoord(-2, 2),
				new FoliageCoord(-2, 1),
				new FoliageCoord(-2, 0),
				new FoliageCoord(-2, -1),
				new FoliageCoord(-2, -2),
				new FoliageCoord(-1, -2),
				new FoliageCoord(0, -2),
				new FoliageCoord(1, -2),
				new FoliageCoord(2, -2),

				// 7x7 edge
				new FoliageCoord(3, -2),
				new FoliageCoord(3, -1),
				new FoliageCoord(3, 0),
				new FoliageCoord(3, 1),
				new FoliageCoord(3, 2),
				new FoliageCoord(2, 3),
				new FoliageCoord(1, 3),
				new FoliageCoord(0, 3),
				new FoliageCoord(-1, 3),
				new FoliageCoord(-2, 3),
				new FoliageCoord(-3, 2),
				new FoliageCoord(-3, 1),
				new FoliageCoord(-3, 0),
				new FoliageCoord(-3, -1),
				new FoliageCoord(-3, -2),
				new FoliageCoord(-2, -3),
				new FoliageCoord(-1, -3),
				new FoliageCoord(0, -3),
				new FoliageCoord(1, -3),
				new FoliageCoord(2, -3),
			},

			// 4 draw distance
			//     X X X X X
			//   X X X X X X X
			// X X X X X X X X X
			// X X X X X X X X X
			// X X X X X X X X X
			// X X X X X X X X X
			// X X X X X X X X X
			//   X X X X X X X
			//     X X X X X
			new FoliageCoord[69]
			{
				// 3x3 square
				new FoliageCoord(0, 0),
				new FoliageCoord(1, 0),
				new FoliageCoord(1, 1),
				new FoliageCoord(0, 1),
				new FoliageCoord(-1, 1),
				new FoliageCoord(-1, 0),
				new FoliageCoord(-1, -1),
				new FoliageCoord(0, -1),
				new FoliageCoord(1, -1),

				// 5x5 square
				new FoliageCoord(2, -1),
				new FoliageCoord(2, 0),
				new FoliageCoord(2, 1),
				new FoliageCoord(2, 2),
				new FoliageCoord(1, 2),
				new FoliageCoord(0, 2),
				new FoliageCoord(-1, 2),
				new FoliageCoord(-2, 2),
				new FoliageCoord(-2, 1),
				new FoliageCoord(-2, 0),
				new FoliageCoord(-2, -1),
				new FoliageCoord(-2, -2),
				new FoliageCoord(-1, -2),
				new FoliageCoord(0, -2),
				new FoliageCoord(1, -2),
				new FoliageCoord(2, -2),

				// 7x7 square
				new FoliageCoord(3, -2),
				new FoliageCoord(3, -1),
				new FoliageCoord(3, 0),
				new FoliageCoord(3, 1),
				new FoliageCoord(3, 2),
				new FoliageCoord(3, 3),
				new FoliageCoord(2, 3),
				new FoliageCoord(1, 3),
				new FoliageCoord(0, 3),
				new FoliageCoord(-1, 3),
				new FoliageCoord(-2, 3),
				new FoliageCoord(-3, 3),
				new FoliageCoord(-3, 2),
				new FoliageCoord(-3, 1),
				new FoliageCoord(-3, 0),
				new FoliageCoord(-3, -1),
				new FoliageCoord(-3, -2),
				new FoliageCoord(-3, -3),
				new FoliageCoord(-2, -3),
				new FoliageCoord(-1, -3),
				new FoliageCoord(0, -3),
				new FoliageCoord(1, -3),
				new FoliageCoord(2, -3),
				new FoliageCoord(3, -3),

				// 9x9 edge
				new FoliageCoord(4, -2),
				new FoliageCoord(4, -1),
				new FoliageCoord(4, 0),
				new FoliageCoord(4, 1),
				new FoliageCoord(4, 2),
				new FoliageCoord(2, 4),
				new FoliageCoord(1, 4),
				new FoliageCoord(0, 4),
				new FoliageCoord(-1, 4),
				new FoliageCoord(-2, 4),
				new FoliageCoord(-4, 2),
				new FoliageCoord(-4, 1),
				new FoliageCoord(-4, 0),
				new FoliageCoord(-4, -1),
				new FoliageCoord(-4, -2),
				new FoliageCoord(-2, -4),
				new FoliageCoord(-1, -4),
				new FoliageCoord(0, -4),
				new FoliageCoord(1, -4),
				new FoliageCoord(2, -4),
			},
			
			// 5 draw distance
			//         X X X
			//     X X X X X X X
			//   X X X X X X X X X
			//   X X X X X X X X X
			// X X X X X X X X X X X
			// X X X X X X X X X X X
			// X X X X X X X X X X X
			//   X X X X X X X X X
			//   X X X X X X X X X
			//     X X X X X X X
			//         X X X
			new FoliageCoord[89]
			{
				// 3x3 square
				new FoliageCoord(0, 0),
				new FoliageCoord(1, 0),
				new FoliageCoord(1, 1),
				new FoliageCoord(0, 1),
				new FoliageCoord(-1, 1),
				new FoliageCoord(-1, 0),
				new FoliageCoord(-1, -1),
				new FoliageCoord(0, -1),
				new FoliageCoord(1, -1),

				// 5x5 square
				new FoliageCoord(2, -1),
				new FoliageCoord(2, 0),
				new FoliageCoord(2, 1),
				new FoliageCoord(2, 2),
				new FoliageCoord(1, 2),
				new FoliageCoord(0, 2),
				new FoliageCoord(-1, 2),
				new FoliageCoord(-2, 2),
				new FoliageCoord(-2, 1),
				new FoliageCoord(-2, 0),
				new FoliageCoord(-2, -1),
				new FoliageCoord(-2, -2),
				new FoliageCoord(-1, -2),
				new FoliageCoord(0, -2),
				new FoliageCoord(1, -2),
				new FoliageCoord(2, -2),

				// 7x7 square
				new FoliageCoord(3, -2),
				new FoliageCoord(3, -1),
				new FoliageCoord(3, 0),
				new FoliageCoord(3, 1),
				new FoliageCoord(3, 2),
				new FoliageCoord(3, 3),
				new FoliageCoord(2, 3),
				new FoliageCoord(1, 3),
				new FoliageCoord(0, 3),
				new FoliageCoord(-1, 3),
				new FoliageCoord(-2, 3),
				new FoliageCoord(-3, 3),
				new FoliageCoord(-3, 2),
				new FoliageCoord(-3, 1),
				new FoliageCoord(-3, 0),
				new FoliageCoord(-3, -1),
				new FoliageCoord(-3, -2),
				new FoliageCoord(-3, -3),
				new FoliageCoord(-2, -3),
				new FoliageCoord(-1, -3),
				new FoliageCoord(0, -3),
				new FoliageCoord(1, -3),
				new FoliageCoord(2, -3),
				new FoliageCoord(3, -3),

				// 9x9 square
				new FoliageCoord(4, -3),
				new FoliageCoord(4, -2),
				new FoliageCoord(4, -1),
				new FoliageCoord(4, 0),
				new FoliageCoord(4, 1),
				new FoliageCoord(4, 2),
				new FoliageCoord(4, 3),
				new FoliageCoord(3, 4),
				new FoliageCoord(2, 4),
				new FoliageCoord(1, 4),
				new FoliageCoord(0, 4),
				new FoliageCoord(-1, 4),
				new FoliageCoord(-2, 4),
				new FoliageCoord(-3, 4),
				new FoliageCoord(-4, 3),
				new FoliageCoord(-4, 2),
				new FoliageCoord(-4, 1),
				new FoliageCoord(-4, 0),
				new FoliageCoord(-4, -1),
				new FoliageCoord(-4, -2),
				new FoliageCoord(-4, -3),
				new FoliageCoord(-3, -4),
				new FoliageCoord(-2, -4),
				new FoliageCoord(-1, -4),
				new FoliageCoord(0, -4),
				new FoliageCoord(1, -4),
				new FoliageCoord(2, -4),
				new FoliageCoord(3, -4),

				// 11x11 edge
				new FoliageCoord(5, -1),
				new FoliageCoord(5, 0),
				new FoliageCoord(5, 1),
				new FoliageCoord(1, 5),
				new FoliageCoord(0, 5),
				new FoliageCoord(-1, 5),
				new FoliageCoord(5, 1),
				new FoliageCoord(5, 0),
				new FoliageCoord(5, -1),
				new FoliageCoord(-1, -5),
				new FoliageCoord(0, -5),
				new FoliageCoord(1, -5),
			},
		};

		private static void drawTiles(Vector3 position, int drawDistance, Camera camera, Plane[] frustumPlanes)
		{
			batches.Clear();

			// 2026-02-09: initially, this simply swapped active and pool to avoid extra work pushing
			// all instances from active onto pool. But, when rendering both main camera and scope
			// camera, it's possible to end up in a feedback loop where one list infinitely grows.
			// As a simple fix we only swap if it actually refills the pool.
			if (activeMatrixLists.Count > matrixListPool.Count)
			{
				Stack<Matrix4x4[]> temp = activeMatrixLists;
				activeMatrixLists = matrixListPool;
				matrixListPool = temp;
			}
			
			FoliageCoord cameraCoord = new FoliageCoord(position);

			UnityEngine.Profiling.Profiler.BeginSample("Draw Tiles");
			// Find nearby tiles within view distance, draw them and add them to the active set
			foreach (FoliageCoord offset in DRAW_OFFSETS[drawDistance])
			{
				FoliageCoord tileCoord = new FoliageCoord(cameraCoord.x + offset.x, cameraCoord.y + offset.y);

				if (relevantTiles.ContainsKey(tileCoord))
				{
					continue;
				}

				FoliageTile tile = getTile(tileCoord);

				if (tile == null)
				{
					continue;
				}

				// Nelson 2025-06-27: moving relevantTiles here from drawTile because zooming in with
				// dual-render scope OFF was marking nearby tiles irrelevant. (failing frustum test)
				// Checking relevant tiles contains key should still work fine because with scope camera drawing
				// second any tiles visible to the main camera are visible to the scope camera.
				relevantTiles.Add(tileCoord, tile);
				if (!tile.isRelevantToViewer)
				{
					tile.isRelevantToViewer = true;
					storage?.TileBecameRelevantToViewer(tile);
				}

				UnityEngine.Profiling.Profiler.BeginSample("Frustum Culling");
				bool withinFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, tile.worldBounds);
				UnityEngine.Profiling.Profiler.EndSample();
				if (!withinFrustum)
				{
					continue;
				}

				UnityEngine.Profiling.Profiler.BeginSample("Draw Tile");
				int sqrDistance = (offset.x * offset.x) + (offset.y * offset.y);
				drawTile(tile, sqrDistance, camera);
				UnityEngine.Profiling.Profiler.EndSample();
			}

			// Finish drawing any remaining instances.
			UnityEngine.Profiling.Profiler.BeginSample("Draw Leftovers");
			foreach (KeyValuePair<FoliageInstancingBatchConfig, FoliageInstancingBatchData> kvp in batches)
			{
				FoliageInstancingBatchData data = kvp.Value;
				if (data.count < 1)
				{
					continue;
				}

				FoliageInstancingBatchConfig config = kvp.Key;
				DrawInstances(in config, data.list, data.count, camera);
			}
			UnityEngine.Profiling.Profiler.EndSample(); // Draw Leftovers
			UnityEngine.Profiling.Profiler.EndSample();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void drawTile(FoliageTile tile, int sqrDistance, Camera camera)
		{
			// Nelson 2025-05-02: iterating Values is ~0.01 ms faster than KeyValuePair. :P
			foreach (FoliageInstanceList list in tile.instances.Values)
			{
				int listBatchCount = list.matrices.Count;
				if (listBatchCount < 1 || !list.isLoadedAndRenderable)
				{
					continue;
				}

				if (list.sqrDrawDistance != -1 && sqrDistance > list.sqrDrawDistance)
				{
					continue;
				}

				// If there is more than one list then every list except the last should have 1023 instances.
				// Probably not very common? I suspect this is from when foliage was first added.
				if (listBatchCount > 1)
				{
					Profiler.BeginSample("Draw Full Batches");
					for (int fullBatchIndex = 0; fullBatchIndex < listBatchCount - 1; ++fullBatchIndex)
					{
						List<Matrix4x4> matrices = list.matrices[fullBatchIndex];
						int matrixCount = Mathf.RoundToInt(matrices.Count * FoliageSettings._instanceDensity);
						DrawInstances(in list.batchConfig, matrices.GetInternalArray(), matrixCount, camera);
					}
					Profiler.EndSample(); // Draw Full Batches
				}

				// Final batch of this list
				{
					List<Matrix4x4> matrices = list.matrices[listBatchCount - 1];
					int matrixCount = Mathf.RoundToInt(matrices.Count * FoliageSettings._instanceDensity);
					if (matrixCount < 1)
						continue;

					Profiler.BeginSample("Setup Batch Config");
					if (!batches.TryGetValue(list.batchConfig, out FoliageInstancingBatchData batchData))
					{
						if (!matrixListPool.TryPop(out Matrix4x4[] buffer))
						{
							buffer = new Matrix4x4[MAX_MATRICES_PER_BATCH];
						}
						activeMatrixLists.Push(buffer);

						batchData = new FoliageInstancingBatchData()
						{
							list = buffer,
							count = 0,
						};
					}
					Profiler.EndSample(); // Setup Batch Config

					Profiler.BeginSample("Handle Batch Config");
					int batchCapacity = list.maxMatricesPerBatch - batchData.count;
					if (matrixCount < batchCapacity)
					{
						FastMatrixCopy(matrices, 0, batchData.list, batchData.count, matrixCount);
						batchData.count += matrixCount;
					}
					else
					{
						// Fill remaining space
						FastMatrixCopy(matrices, 0, batchData.list, batchData.count, batchCapacity);
						DrawInstances(in list.batchConfig, batchData.list, list.maxMatricesPerBatch, camera);

						// Reset batch data with remaining instances from this list
						batchData.count = matrixCount - batchCapacity;
						if (batchData.count > 0)
						{
							FastMatrixCopy(matrices, batchCapacity, batchData.list, 0, batchData.count);
						}
					}
					batches[list.batchConfig] = batchData;
					Profiler.EndSample(); // Handle Batch Config
				}
			}
		}

		/// <summary>
		/// Nelson 2025-05-01: slightly faster than List<Matrix4x4>.CopyTo.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void FastMatrixCopy(List<Matrix4x4> source, int sourceIndex, Matrix4x4[] destination, int destinationIndex, int count)
		{
			UnityEngine.Profiling.Profiler.BeginSample("FastMatrixCopy");
			Matrix4x4[] sourceArray = source.GetInternalArray();

			unsafe
			{
				fixed (Matrix4x4* sourceArrayPtr = sourceArray)
				fixed (Matrix4x4* destinationArrayPtr = destination)
				{
					Matrix4x4* sourcePtr = sourceArrayPtr + sourceIndex;
					Matrix4x4* destinationPtr = destinationArrayPtr + destinationIndex;
					long destinationSizeInBytes = (MAX_MATRICES_PER_BATCH - destinationIndex) * sizeof(Matrix4x4);
					long sourceBytesToCopy = count * sizeof(Matrix4x4);
					System.Buffer.MemoryCopy(sourcePtr, destinationPtr, destinationSizeInBytes, sourceBytesToCopy);
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}
#endif // !DEDICATED_SERVER

		internal const int MAX_MATRICES_PER_BATCH = 1023;
		/// <summary>
		/// Nelson 2025-07-14: although Graphics.DrawMeshInstanced accepts 1023 instances, it will split them into max
		/// batches of [1, 511, 511] if shader does not have: #pragma instancing_options assumeuniformscaling
		/// So, we might as well do our own splitting of batches to avoid batches of 1.
		/// </summary>
		internal const int NON_UNIFORM_SCALE_INSTANCES_PER_BATCH = 511;

		/// <param name="matrixCount">Must be within [0, MAX_MATRICES_PER_BATCH] range.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DrawInstances(in FoliageInstancingBatchConfig config, Matrix4x4[] matrices, int matrixCount, Camera camera)
		{
			// Nelson 2025-05-02: branching here is ~0.2 ms faster than calling DrawInstanced vs DrawNonInstanced
			if (shouldDrawWithoutInstancing)
			{
				UnityEngine.Profiling.Profiler.BeginSample("DrawMesh");
				for (int matrixIndex = 0; matrixIndex < matrixCount; matrixIndex++)
				{
					Graphics.DrawMesh(config.mesh, matrices[matrixIndex], config.material, foliageRenderLayer, camera, 0, null, config.castShadows, true);
				}
				UnityEngine.Profiling.Profiler.EndSample();
			}
			else
			{
				UnityEngine.Rendering.ShadowCastingMode shadowCastingMode = config.castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
				UnityEngine.Profiling.Profiler.BeginSample("DrawMeshInstanced");
				Graphics.DrawMeshInstanced(config.mesh, 0, config.material, matrices, matrixCount, null, shadowCastingMode, true, foliageRenderLayer, camera);
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		public static FoliageTile getOrAddTile(Vector3 worldPosition)
		{
			FoliageCoord tileCoord = new FoliageCoord(worldPosition);
			return getOrAddTile(tileCoord);
		}

		public static FoliageTile getTile(Vector3 worldPosition)
		{
			FoliageCoord tileCoord = new FoliageCoord(worldPosition);
			return getTile(tileCoord);
		}

		public static FoliageTile getOrAddTile(FoliageCoord tileCoord)
		{
			FoliageTile tile;
			if (!tiles.TryGetValue(tileCoord, out tile))
			{
				tile = new FoliageTile(tileCoord);
				tiles.Add(tileCoord, tile);
			}
			return tile;
		}

		public static FoliageTile getTile(FoliageCoord tileCoord)
		{
			FoliageTile tile;
			tiles.TryGetValue(tileCoord, out tile);
			return tile;
		}

		/// <summary>
		/// Version number associated with this particular system instance.
		/// </summary>
		protected uint version;

		public override void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();

			if (reader.containsKey("Version"))
			{
				version = reader.readValue<uint>("Version");
			}
			else
			{
				version = 1;
			}

			int tileCount = reader.readArrayLength("Tiles");
			if (instance != this)
			{
				UnturnedLog.warn("Level contains multiple FoliageSystems. Ignoring {0} tile(s) with instance ID: {1}", tileCount, instanceID);
				return;
			}

			if (version == 2)
			{
				storage = new FoliageStorageV2();
			}
			else
			{
				storage = new FoliageStorageV1();
			}
			storage.Initialize();

			// Dedicated server loads the foliage v2 header to calculate hash and then discards.
			if (Dedicator.IsDedicatedServer)
			{
				shutdownStorage();
				return;
			}

			for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
			{
				reader.readArrayIndex(tileIndex);

				FoliageTile tile = new FoliageTile(FoliageCoord.ZERO);
				tile.read(reader);
				tiles.Add(tile.coord, tile);
			}

			if (Level.isEditor)
			{
				storage.EditorLoadAllTiles(tiles.Values);

				if (version < 2)
				{
					// Mark dirty to easily resave.
					LevelHierarchy.instance.isDirty = true;
				}
			}
		}

		public override void write(IFormattedFileWriter writer)
		{
			// Storage may be null if this is a newly placed instance in a new level, in which case we default to V2.
			if (storage == null || version < 2)
			{
				FoliageStorageV2 v2 = new FoliageStorageV2();
				v2.EditorSaveAllTiles(tiles.Values);

				version = 2;
			}
			else
			{
				storage.EditorSaveAllTiles(tiles.Values);
			}

			writer.beginObject();
			writer.writeValue("Version", version);
			writer.beginArray("Tiles");

			foreach (KeyValuePair<FoliageCoord, FoliageTile> pair in tiles)
			{
				FoliageTile tile = pair.Value;
				writer.writeValue(tile);
			}

			writer.endArray();
			writer.endObject();
		}

		/// <summary>
		/// Automatically placing foliage onto tiles in editor.
		/// </summary>
		protected void tickBakeQueue()
		{
			UnityEngine.Profiling.Profiler.BeginSample("Bake");
			KeyValuePair<FoliageTile, List<IFoliageSurface>> tileSurfacePair = bakeQueue.Dequeue();
			bake(tileSurfacePair.Key, tileSurfacePair.Value);

			if (bakeQueue.Count == 0)
			{
				bakeEnd();
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private static bool shouldDrawWithoutInstancing;

#if !DEDICATED_SERVER
		protected void Update()
		{
			if (MainCamera.instance == null || this != instance)
			{
				// Checks (this != instance) to avoid rendering twice if a second foliage system is accidentally placed.
				return;
			}

			relevantTiles.Clear();
			if (FoliageSettings.enabled && !(hiddenByHeightEditor | hiddenByMaterialEditor))
			{
				shouldDrawWithoutInstancing = FoliageSettings.forceInstancingOff || !SystemInfo.supportsInstancing;

				GeometryUtility.CalculateFrustumPlanes(MainCamera.instance, mainCameraFrustumPlanes);
				drawTiles(MainCamera.RenderingPosition, FoliageSettings.drawDistance, null, mainCameraFrustumPlanes);

				if (FoliageSettings.drawFocus && isFocused && focusCamera != null)
				{
					Plane[] frustumPlanesTemp;
					if (MainCamera.instance == focusCamera)
					{
						frustumPlanesTemp = mainCameraFrustumPlanes;
					}
					else
					{
						GeometryUtility.CalculateFrustumPlanes(focusCamera, focusCameraFrustumPlanes);
						frustumPlanesTemp = focusCameraFrustumPlanes;
					}

					drawTiles(focusPosition, FoliageSettings.drawFocusDistance, focusCamera, frustumPlanesTemp);
				}
			}

			UnityEngine.Profiling.Profiler.BeginSample("Clear PrevTiles");
			// Go through previously active tiles, and if they're no longer active remove all instance data
			foreach (KeyValuePair<FoliageCoord, FoliageTile> prevTile in previousFrameRelevantTiles)
			{
				if (relevantTiles.ContainsKey(prevTile.Key) || prevTile.Value == null)
				{
					continue;
				}

				if (prevTile.Value.isRelevantToViewer)
				{
					prevTile.Value.isRelevantToViewer = false;
					storage?.TileNoLongerRelevantToViewer(prevTile.Value);
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();

			Dictionary<FoliageCoord, FoliageTile> temp = previousFrameRelevantTiles;
			previousFrameRelevantTiles = relevantTiles;
			relevantTiles = temp;

			if (Level.isEditor && bakeQueue.Count > 0)
			{
				tickBakeQueue();
			}

			if (storage != null)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Update Storage");
				storage.Update();
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}
#endif // !DEDICATED_SERVER

		protected void OnEnable()
		{
			LevelHierarchy.addItem(this);
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		protected void OnDisable()
		{
			CommandLogMemoryUsage.OnExecuted -= OnLogMemoryUsage;
			LevelHierarchy.removeItem(this);
		}

		protected void Awake()
		{
			name = "Foliage_System";
			gameObject.layer = LayerMasks.GROUND;

			if (instance == null)
			{
				instance = this;

				previousFrameRelevantTiles.Clear();
				relevantTiles.Clear();
				bakeQueue.Clear();

				batches.Clear();
				activeMatrixLists.Clear();
				matrixListPool.Clear();

				shutdownStorage();
				clearAndReleaseTiles();
			}
		}

		protected void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;

				previousFrameRelevantTiles.Clear();
				relevantTiles.Clear();
				bakeQueue.Clear();

				batches.Clear();
				activeMatrixLists.Clear();
				matrixListPool.Clear();

				shutdownStorage();
				clearAndReleaseTiles();
			}
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			if (instance != this)
			{
				return;
			}

			results.Add($"Foliage active instance lists: {activeMatrixLists.Count} Matrix list pool: {matrixListPool.Count}");
		}

		static FoliageSystem()
		{
			surfaces = new List<IFoliageSurface>();
		}

		/// <summary>
		/// 2022-04-26: this used to be environment layer, but "scope focus foliage" can draw outside that render distance
		/// so we now use the sky layer which is visible up to the far clip plane.
		/// </summary>
		private const int foliageRenderLayer = LayerMasks.SKY;
	}
}
