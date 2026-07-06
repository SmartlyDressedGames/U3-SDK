////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public struct SplatmapBounds
	{
		public SplatmapCoord min;
		public SplatmapCoord max;

		public override string ToString()
		{
			return '[' + min.ToString() + ", " + max.ToString() + ']';
		}

		public SplatmapBounds(SplatmapCoord newMin, SplatmapCoord newMax)
		{
			min = newMin;
			max = newMax;
		}

		public SplatmapBounds(LandscapeCoord tileCoord, Bounds worldBounds)
		{
			int min_splatmap_x = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.z - (tileCoord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
			int max_splatmap_x = Mathf.Clamp(Mathf.FloorToInt((worldBounds.max.z - (tileCoord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
			int min_splatmap_y = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.x - (tileCoord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
			int max_splatmap_y = Mathf.Clamp(Mathf.FloorToInt((worldBounds.max.x - (tileCoord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);

			min = new SplatmapCoord(min_splatmap_x, min_splatmap_y);
			max = new SplatmapCoord(max_splatmap_x, max_splatmap_y);
		}
	}
}
