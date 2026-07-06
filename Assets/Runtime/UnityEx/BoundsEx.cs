////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class BoundsEx
	{
		public static bool ContainsXZ(this Bounds bounds, Vector3 point)
		{
			Vector3 center = bounds.center;
			Vector3 extents = bounds.extents;
			return point.x >= center.x - extents.x && point.x <= center.x + extents.x && point.z >= center.z - extents.z && point.z <= center.z + extents.z;
		}

		public static float CalculateVolume(this Bounds bounds)
		{
			Vector3 size = bounds.size;
			return size.x * size.y * size.z;
		}
	}
}
