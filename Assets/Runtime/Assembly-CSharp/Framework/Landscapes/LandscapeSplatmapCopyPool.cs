////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;

namespace SDG.Framework.Landscapes
{
	public static class LandscapeSplatmapCopyPool
	{
		private static Pool<float[,,]> pool;

		public static void empty()
		{
			pool.empty();
		}

		public static void warmup(uint count)
		{
			pool.warmup(count, handlePoolClaim);
		}

		public static float[,,] claim()
		{
			return pool.claim(handlePoolClaim);
		}

		public static void release(float[,,] copy)
		{
			pool.release(copy, handlePoolRelease);
		}

		private static float[,,] handlePoolClaim(Pool<float[,,]> pool)
		{
			return new float[Landscape.SPLATMAP_RESOLUTION, Landscape.SPLATMAP_RESOLUTION, Landscape.SPLATMAP_LAYERS];
		}

		private static void handlePoolRelease(Pool<float[,,]> pool, float[,,] copy)
		{ }

		static LandscapeSplatmapCopyPool()
		{
			pool = new Pool<float[,,]>();
		}
	}
}
