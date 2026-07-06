////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class PooledTransportConnectionList : List<ITransportConnection>
	{
		internal PooledTransportConnectionList(int capacity) : base(capacity)
		{ }
	}

	/// <summary>
	/// Pool lists to avoid loopback re-using an existing list.
	/// Callers do not need to manually return lists because they are reset before each frame.
	/// </summary>
	internal static class TransportConnectionListPool
	{
		public static PooledTransportConnectionList Get()
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			PooledTransportConnectionList list;
			if (available.Count > 0)
			{
				list = available.GetAndRemoveTail();
				if (list.Count > 0)
				{
					list.Clear();

					// Only warn once per frame to reduce spam.
					int frameNumber = UnityEngine.Time.frameCount;
					if (frameNumber != lastWarningFrameNumber)
					{
						lastWarningFrameNumber = frameNumber;
						UnturnedLog.warn("PooledConnectionList was used after end of frame! Plugins should not hold onto these lists.");
					}
				}
			}
			else
			{
				list = new PooledTransportConnectionList(Provider.maxPlayers);
			}
			claimed.Add(list);
			return list;
		}

		public static void ReleaseAll()
		{
			foreach (PooledTransportConnectionList list in claimed)
			{
				list.Clear();
				available.Add(list);
			}
			claimed.Clear();
		}

		static TransportConnectionListPool()
		{
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		private static void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Transport connection list pool size: {available.Count}");
			results.Add($"Transport connection list active count: {claimed.Count}");
		}

		private static List<PooledTransportConnectionList> available = new List<PooledTransportConnectionList>();
		private static List<PooledTransportConnectionList> claimed = new List<PooledTransportConnectionList>();
		private static int lastWarningFrameNumber = -1;
	}
}
