////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Utilities
{
	public static class ListPool<T>
	{
		private static Pool<List<T>> pool;

		public static void empty()
		{
			pool.empty();
		}

		public static void warmup(uint count)
		{
			pool.warmup(count, handlePoolClaim);
		}

		public static List<T> claim()
		{
			return pool.claim(handlePoolClaim);
		}

		public static void release(List<T> list)
		{
			pool.release(list, handlePoolRelease);
		}

		private static List<T> handlePoolClaim(Pool<List<T>> pool)
		{
			return new List<T>();
		}

		private static void handlePoolRelease(Pool<List<T>> pool, List<T> list)
		{
			list.Clear();
		}

		static ListPool()
		{
			pool = new Pool<List<T>>();
		}
	}
}
