////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class LodGroupExtension
	{
		/// <summary>
		/// Lod group will be culled when screen size is smaller than this value.
		/// </summary>
		public static float GetCullingScreenSize(this LODGroup lodGroup)
		{
			LOD[] levelsOfDetail = lodGroup.GetLODs();
			return levelsOfDetail[levelsOfDetail.Length - 1].screenRelativeTransitionHeight;
		}

		/// <summary>
		/// Clamp the culling screen percentage to be less than or equal to a maximum value.
		/// </summary>
		public static void ClampCulling(this LODGroup lodGroup, float max)
		{
			LOD[] levelsOfDetail = lodGroup.GetLODs();
			int lowestIndex = levelsOfDetail.Length - 1;
			if (lowestIndex <= levelsOfDetail.Length && levelsOfDetail[lowestIndex].screenRelativeTransitionHeight > max)
			{
				levelsOfDetail[lowestIndex].screenRelativeTransitionHeight = max;
				lodGroup.SetLODs(levelsOfDetail);
			}
		}

		/// <summary>
		/// Prevent the lowest LOD from being culled.
		/// </summary>
		public static void DisableCulling(this LODGroup lodGroup)
		{
			ClampCulling(lodGroup, 0.0f);
		}
	}
}
