////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Framework.Utilities
{
	/// <summary>
	/// Pool of objects that implement the IPoolable interface.
	/// 
	/// Useful for types that do not need special construction,
	/// and want notification when claimed and released.
	/// </summary>
	public static class PoolablePool<T> where T : IPoolable
	{
		private static Pool<T> pool;

		public static void empty()
		{
			pool.empty();
		}

		public static void warmup(uint count)
		{
			pool.warmup(count, handlePoolClaim);
		}

		public static T claim()
		{
			T poolable = pool.claim(handlePoolClaim);
			poolable.poolClaim();
			return poolable;
		}

		public static void release(T poolable)
		{
			pool.release(poolable, handlePoolRelease);
		}

		private static T handlePoolClaim(Pool<T> pool)
		{
			return Activator.CreateInstance<T>();
		}

		private static void handlePoolRelease(Pool<T> pool, T poolable)
		{
			poolable.poolRelease();
		}

		static PoolablePool()
		{
			pool = new Pool<T>();
		}
	}
}
