////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public struct FoliageBounds
	{
		public FoliageCoord min;
		public FoliageCoord max;

		public override string ToString()
		{
			return '[' + min.ToString() + ", " + max.ToString() + ']';
		}

		public FoliageBounds(FoliageCoord newMin, FoliageCoord newMax)
		{
			min = newMin;
			max = newMax;
		}

		public FoliageBounds(Bounds worldBounds)
		{
			min = new FoliageCoord(worldBounds.min);
			max = new FoliageCoord(worldBounds.max);
		}
	}
}
