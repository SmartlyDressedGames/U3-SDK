////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Framework.Utilities
{
	public class Pool<T>
	{
		public delegate T PoolClaimHandler(Pool<T> pool);
		public delegate void PoolReleaseHandler(Pool<T> pool, T item);

		public delegate void PoolClaimedHandler(Pool<T> pool, T item);
		public delegate void PoolReleasedHandler(Pool<T> pool, T item);

		public event PoolClaimedHandler claimed;
		public event PoolReleasedHandler released;

		/// <summary>
		/// Number of items in underlying queue.
		/// </summary>
		public int count => pool.Count;

		protected Queue<T> pool;

		public void empty()
		{
			pool.Clear();
		}

		public void warmup(uint count)
		{
			warmup(count, null);
		}

		public void warmup(uint count, PoolClaimHandler callback)
		{
			if (callback == null)
			{
				callback = handleClaim;
			}

			for (uint index = 0; index < count; index++)
			{
				T item = callback(this);
				release(item);
			}
		}

		public T claim()
		{
			return claim(null);
		}

		public T claim(PoolClaimHandler callback)
		{
			T item;

			if (pool.Count > 0)
			{
				item = pool.Dequeue();
			}
			else
			{
				if (callback != null)
				{
					item = callback(this);
				}
				else
				{
					item = handleClaim(this);
				}
			}

			triggerClaimed(item);
			return item;
		}

		public void release(T item)
		{
			release(item, null);
		}

		public void release(T item, PoolReleasedHandler callback)
		{
			if (item == null)
			{
				return;
			}

			callback?.Invoke(this, item);

			triggerReleased(item);
			pool.Enqueue(item);
		}

		protected T handleClaim(Pool<T> pool)
		{
			return Activator.CreateInstance<T>();
		}

		protected void triggerClaimed(T item)
		{
			claimed?.Invoke(this, item);
		}

		protected void triggerReleased(T item)
		{
			released?.Invoke(this, item);
		}

		public Pool()
		{
			pool = new Queue<T>();
		}
	}
}
