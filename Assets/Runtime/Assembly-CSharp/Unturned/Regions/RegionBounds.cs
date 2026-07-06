////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public struct RegionBounds
	{
		public RegionCoord min;
		public RegionCoord max;

		public override string ToString()
		{
			return '[' + min.ToString() + ", " + max.ToString() + ']';
		}

		public RegionBounds(RegionCoord newMin, RegionCoord newMax)
		{
			min = newMin;
			max = newMax;
		}

		public RegionBounds(Bounds worldBounds)
		{
			min = new RegionCoord(worldBounds.min);
			min.ClampIntoBounds();
			max = new RegionCoord(worldBounds.max);
			max.ClampIntoBounds();
		}
	}
}
