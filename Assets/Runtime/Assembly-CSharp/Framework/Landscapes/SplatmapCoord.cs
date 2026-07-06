////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public struct SplatmapCoord : IEquatable<SplatmapCoord>
	{
		public int x;
		public int y;

		public static bool operator ==(SplatmapCoord a, SplatmapCoord b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(SplatmapCoord a, SplatmapCoord b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			SplatmapCoord coord = (SplatmapCoord) obj;
			return x.Equals(coord.x) && y.Equals(coord.y);
		}

		public override int GetHashCode()
		{
			return x ^ y;
		}

		public override string ToString()
		{
			return '(' + x.ToString() + ", " + y.ToString() + ')';
		}

		public bool Equals(SplatmapCoord other)
		{
			return x == other.x && y == other.y;
		}

		public SplatmapCoord(int new_x, int new_y)
		{
			x = new_x;
			y = new_y;
		}

		public SplatmapCoord(LandscapeCoord tileCoord, Vector3 worldPosition)
		{
			x = Mathf.Clamp(Mathf.FloorToInt((worldPosition.z - (tileCoord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
			y = Mathf.Clamp(Mathf.FloorToInt((worldPosition.x - (tileCoord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
		}
	}
}
