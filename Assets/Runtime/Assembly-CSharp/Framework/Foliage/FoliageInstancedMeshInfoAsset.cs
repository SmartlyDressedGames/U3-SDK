////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageInstancedMeshInfoAsset : FoliageInfoAsset
	{

		public ContentReference<Mesh> mesh;


		public ContentReference<Material> material;


		public bool castShadows;


		public bool tileDither;

		/// <summary>
		/// If true, mesh is not loaded when clutter is turned off in graphics menu.
		/// Defaults to false.
		/// </summary>
		public bool IsClutter
		{
			get;
			set;
		}

		public int drawDistance;

		/// <summary>
		/// Foliage to use during the Christmas event instead.
		/// </summary>

		public AssetReference<FoliageInstancedMeshInfoAsset>? christmasRedirect;

		/// <summary>
		/// Foliage to use during the Halloween event instead.
		/// </summary>

		public AssetReference<FoliageInstancedMeshInfoAsset>? halloweenRedirect;

		/// <summary>
		/// Get asset ref to replace this one for holiday, invalid to disable, or null if it should not be redirected.
		/// </summary>
		public AssetReference<FoliageInstancedMeshInfoAsset>? getHolidayRedirect()
		{
			switch (HolidayUtil.getActiveHoliday())
			{
				case ENPCHoliday.CHRISTMAS:
					return christmasRedirect;

				case ENPCHoliday.HALLOWEEN:
					return halloweenRedirect;

				default:
					// Null rather than invalid, otherwise foliage gets disabled.
					return null;
			}
		}

		public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
		{
			if (!bakeSettings.bakeInstancesMeshes)
			{
				return;
			}

			if (bakeSettings.bakeClear)
			{
				return;
			}

			base.bakeFoliage(bakeSettings, surface, bounds, surfaceWeight, collectionWeight);
		}

		public override int getInstanceCountInVolume<T>(T volume)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Instance Density");

			Bounds worldBounds = volume.worldBounds;
			FoliageBounds foliageBounds = new FoliageBounds(worldBounds);

			int instanceCount = 0;
			for (int tile_x = foliageBounds.min.x; tile_x <= foliageBounds.max.x; tile_x++)
			{
				for (int tile_y = foliageBounds.min.y; tile_y <= foliageBounds.max.y; tile_y++)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Get Foliage Tile");
					FoliageCoord foliageCoord = new FoliageCoord(tile_x, tile_y);
					FoliageTile foliageTile = FoliageSystem.getTile(foliageCoord);
					UnityEngine.Profiling.Profiler.EndSample();

					if (foliageTile != null)
					{
						UnityEngine.Profiling.Profiler.BeginSample("Get List");
						FoliageInstanceList list;
						if (foliageTile.instances != null && foliageTile.instances.TryGetValue(getReferenceTo<FoliageInstancedMeshInfoAsset>(), out list))
						{
							UnityEngine.Profiling.Profiler.BeginSample("Search List");
							foreach (List<Matrix4x4> matrices in list.matrices)
							{
								foreach (Matrix4x4 matrix in matrices)
								{
									UnityEngine.Profiling.Profiler.BeginSample("Contains Point");
									if (volume.containsPoint(matrix.GetPosition()))
									{
										instanceCount++;
									}
									UnityEngine.Profiling.Profiler.EndSample();
								}
							}
							UnityEngine.Profiling.Profiler.EndSample();
						}
						UnityEngine.Profiling.Profiler.EndSample();
					}
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();

			return instanceCount;
		}

		protected override void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
		{
			FoliageSystem.addInstance(getReferenceTo<FoliageInstancedMeshInfoAsset>(),
									  position,
									  rotation,
									  scale,
									  clearWhenBaked);
		}

		protected override bool isPositionValid(Vector3 position, bool doCollisionChecks)
		{
			if (!FoliageVolumeManager.Get().IsPositionBakeable(position, true, false, false))
			{
				return false;
			}

			return true;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			mesh = p.data.ParseStruct<ContentReference<Mesh>>("Mesh");
			material = p.data.ParseStruct<ContentReference<Material>>("Material");

			if (p.data.ContainsKey("Cast_Shadows"))
			{
				castShadows = p.data.ParseBool("Cast_Shadows");
			}
			else
			{
				castShadows = false;
			}

			if (p.data.ContainsKey("Tile_Dither"))
			{
				tileDither = p.data.ParseBool("Tile_Dither");
			}
			else
			{
				tileDither = true;
			}

			IsClutter = p.data.ParseBool("Is_Clutter");

			if (p.data.ContainsKey("Draw_Distance"))
			{
				drawDistance = p.data.ParseInt32("Draw_Distance");
			}
			else
			{
				drawDistance = -1;
			}

			if (p.data.ContainsKey("Christmas_Redirect"))
			{
				christmasRedirect = p.data.ParseStruct<AssetReference<FoliageInstancedMeshInfoAsset>>("Christmas_Redirect");
			}
			if (p.data.ContainsKey("Halloween_Redirect"))
			{
				halloweenRedirect = p.data.ParseStruct<AssetReference<FoliageInstancedMeshInfoAsset>>("Halloween_Redirect");
			}
		}

		protected virtual void resetInstancedMeshInfo()
		{
			tileDither = true;
			drawDistance = -1;
		}

		public FoliageInstancedMeshInfoAsset() : base()
		{
			resetInstancedMeshInfo();
		}
	}
}
