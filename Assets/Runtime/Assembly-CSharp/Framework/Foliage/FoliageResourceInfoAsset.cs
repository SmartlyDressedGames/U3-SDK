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
	public class FoliageResourceInfoAsset : FoliageInfoAsset
	{
		private static readonly Collider[] OBSTRUCTION_COLLIDERS = new Collider[16];


		public AssetReference<ResourceAsset> resource;


		public float obstructionRadius;

		/// <summary>
		/// If true, ResourceAsset's legacy random rotation and scale properties are used.
		/// (As opposed to FoliageInfoAsset properties.)
		/// Defaults to true for backwards compatibility.
		/// </summary>
		public bool UsesLegacyRotationAndScale
		{
			get;
			set;
		}
		
		public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
		{
			if (!bakeSettings.bakeResources)
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
			UnityEngine.Profiling.Profiler.BeginSample("Resource Density");

			int instanceCount = 0;

			Bounds worldBounds = volume.worldBounds;
			RegionBoundsInt bounds = Regions.GetCoordinateBoundsInt(worldBounds);
			foreach (Vector2Int coord in bounds)
			{
				List<ResourceSpawnpoint> trees = LevelGround.GetTreesOrNullInRegion(coord);
				if (trees == null)
					continue;

				foreach (ResourceSpawnpoint tree in trees)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Check Reference");
					if (!resource.isReferenceTo(tree.asset))
					{
						UnityEngine.Profiling.Profiler.EndSample(); // Check Reference
						continue;
					}
					UnityEngine.Profiling.Profiler.EndSample(); // Check Reference

					UnityEngine.Profiling.Profiler.BeginSample("Contains Point");
					if (volume.containsPoint(tree.point))
					{
						instanceCount++;
					}
					UnityEngine.Profiling.Profiler.EndSample(); // Contains Point
				}
			}

			UnityEngine.Profiling.Profiler.EndSample(); // Resource Density

			return instanceCount;
		}

		protected override void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
		{
			ResourceAsset asset = Assets.find(resource);
			if (asset == null)
			{
				return;
			}

			if (UsesLegacyRotationAndScale)
			{
				asset.GetLegacyRotationAndScale(position, out rotation, out scale);
			}

			LevelGround.addSpawn(position, rotation, scale, asset.GUID, clearWhenBaked);
		}

		protected override bool isPositionValid(Vector3 position, bool doCollisionChecks)
		{
			if (!FoliageVolumeManager.Get().IsPositionBakeable(position, false, true, false))
			{
				return false;
			}

			if (doCollisionChecks)
			{
				int obstructionCount = Physics.OverlapSphereNonAlloc(position, obstructionRadius, OBSTRUCTION_COLLIDERS, RayMasks.BLOCK_RESOURCE);
				for (int obstructionIndex = 0; obstructionIndex < obstructionCount; obstructionIndex++)
				{
					ObjectAsset objAsset = LevelObjects.getAsset(OBSTRUCTION_COLLIDERS[obstructionIndex].transform);
					if (objAsset != null && !objAsset.isSnowshoe)
					{
						return false;
					}
				}
			}

			return true;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			resource = p.data.ParseStruct<AssetReference<ResourceAsset>>("Resource");

			if (p.data.ContainsKey("Obstruction_Radius"))
			{
				obstructionRadius = p.data.ParseFloat("Obstruction_Radius");
			}

			UsesLegacyRotationAndScale = p.data.ParseBool("Legacy_Rotation_and_Scale", defaultValue: true);
		}

		protected virtual void resetResource()
		{
			obstructionRadius = 4;
		}

		public FoliageResourceInfoAsset() : base()
		{
			resetResource();
		}
	}
}
