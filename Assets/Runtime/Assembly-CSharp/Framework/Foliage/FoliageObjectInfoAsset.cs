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
	public class FoliageObjectInfoAsset : FoliageInfoAsset
	{

		public AssetReference<ObjectAsset> obj;


		public float obstructionRadius;

		public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
		{
			if (!bakeSettings.bakeObjects)
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
			UnityEngine.Profiling.Profiler.BeginSample("Object Density");

			Bounds worldBounds = volume.worldBounds;
			RegionBounds regionBounds = new RegionBounds(worldBounds);

			int instanceCount = 0;
			for (byte region_x = regionBounds.min.x; region_x <= regionBounds.max.x; region_x++)
			{
				for (byte region_y = regionBounds.min.y; region_y <= regionBounds.max.y; region_y++)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Object Region");
					List<LevelObject> levelObjects = LevelObjects.objects[region_x, region_y];
					foreach (LevelObject levelObject in levelObjects)
					{
						UnityEngine.Profiling.Profiler.BeginSample("Check Reference");
						if (!obj.isReferenceTo(levelObject.asset))
						{
							UnityEngine.Profiling.Profiler.EndSample();
							continue;
						}
						UnityEngine.Profiling.Profiler.EndSample();

						if (levelObject.transform == null)
						{
							continue;
						}

						UnityEngine.Profiling.Profiler.BeginSample("Contains Point");
						if (volume.containsPoint(levelObject.transform.position))
						{
							instanceCount++;
						}
						UnityEngine.Profiling.Profiler.EndSample();
					}
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();

			return instanceCount;
		}

		protected override void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
		{
			ObjectAsset asset = Assets.find(obj);
			if (asset == null)
			{
				return;
			}

			LevelObjects.addObject(position,
			rotation,
			scale,
			0,
			asset.GUID,
			clearWhenBaked ? ELevelObjectPlacementOrigin.GENERATED : ELevelObjectPlacementOrigin.PAINTED);
		}

		protected override bool isPositionValid(Vector3 position, bool doCollisionChecks)
		{
			if (!FoliageVolumeManager.Get().IsPositionBakeable(position, false, false, true))
			{
				return false;
			}

			return true;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			obj = p.data.ParseStruct<AssetReference<ObjectAsset>>("Object");

			if (p.data.ContainsKey("Obstruction_Radius"))
			{
				obstructionRadius = p.data.ParseFloat("Obstruction_Radius");
			}
		}

		protected virtual void resetObject()
		{
			obstructionRadius = 4;
		}

		public FoliageObjectInfoAsset() : base()
		{
			resetObject();
		}
	}
}
