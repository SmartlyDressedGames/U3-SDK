////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageVolumeUtility
	{
		[System.Obsolete]
		public static bool isTileBakeable(FoliageTile tile)
		{
			return FoliageVolumeManager.Get().IsTileBakeable(tile);
		}

		[System.Obsolete]
		public static bool isPointValid(Vector3 point, bool instancedMeshes, bool resources, bool objects)
		{
			return FoliageVolumeManager.Get().IsPositionBakeable(point, instancedMeshes, resources, objects);
		}

		[System.Obsolete]
		public static bool isPointInsideVolume(FoliageVolume volume, Vector3 point)
		{
			return volume.IsPositionInsideVolume(point);
		}
	}
}
