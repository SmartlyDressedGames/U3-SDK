////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public struct HeightmapCoord : IEquatable<HeightmapCoord>
	{
		public int x;
		public int y;

		public static bool operator ==(HeightmapCoord a, HeightmapCoord b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(HeightmapCoord a, HeightmapCoord b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			HeightmapCoord coord = (HeightmapCoord) obj;
			return x == coord.x && y == coord.y;
		}

		public override int GetHashCode()
		{
			return x ^ y;
		}

		public override string ToString()
		{
			return '(' + x.ToString() + ", " + y.ToString() + ')';
		}

		public bool Equals(HeightmapCoord other)
		{
			return x == other.x && y == other.y;
		}

		public HeightmapCoord(int new_x, int new_y)
		{
			x = new_x;
			y = new_y;
		}

		public HeightmapCoord(LandscapeCoord tileCoord, Vector3 worldPosition)
		{
			x = Mathf.Clamp(Mathf.RoundToInt((worldPosition.z - (tileCoord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
			y = Mathf.Clamp(Mathf.RoundToInt((worldPosition.x - (tileCoord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
		}
	}
}
