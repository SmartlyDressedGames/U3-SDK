////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public struct HeightmapBounds
	{
		public HeightmapCoord min;
		public HeightmapCoord max;

		public override string ToString()
		{
			return '[' + min.ToString() + ", " + max.ToString() + ']';
		}

		public HeightmapBounds(HeightmapCoord newMin, HeightmapCoord newMax)
		{
			min = newMin;
			max = newMax;
		}

		public HeightmapBounds(LandscapeCoord tileCoord, Bounds worldBounds)
		{
			int min_heightmap_x = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.z - (tileCoord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
			int max_heightmap_x = Mathf.Clamp(Mathf.CeilToInt((worldBounds.max.z - (tileCoord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
			int min_heightmap_y = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.x - (tileCoord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
			int max_heightmap_y = Mathf.Clamp(Mathf.CeilToInt((worldBounds.max.x - (tileCoord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);

			min = new HeightmapCoord(min_heightmap_x, min_heightmap_y);
			max = new HeightmapCoord(max_heightmap_x, max_heightmap_y);
		}
	}
}
