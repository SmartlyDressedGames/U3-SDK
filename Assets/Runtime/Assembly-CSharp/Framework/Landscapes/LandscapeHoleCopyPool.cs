////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;

namespace SDG.Framework.Landscapes
{
	public static class LandscapeHoleCopyPool
	{
		private static Pool<bool[,]> pool;

		public static void empty()
		{
			pool.empty();
		}

		public static void warmup(uint count)
		{
			pool.warmup(count, handlePoolClaim);
		}

		public static bool[,] claim()
		{
			return pool.claim(handlePoolClaim);
		}

		public static void release(bool[,] copy)
		{
			pool.release(copy, handlePoolRelease);
		}

		private static bool[,] handlePoolClaim(Pool<bool[,]> pool)
		{
			return new bool[Landscape.HOLES_RESOLUTION, Landscape.HOLES_RESOLUTION];
		}

		private static void handlePoolRelease(Pool<bool[,]> pool, bool[,] copy)
		{ }

		static LandscapeHoleCopyPool()
		{
			pool = new Pool<bool[,]>();
		}
	}
}
