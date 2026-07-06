////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES

using SDG.NetPak;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public delegate void NetInvokeDeferred(object voidNetObj, in ClientInvocationContext context);

	/// <summary>
	/// When a client method is called on a target object that does not exist yet this class is responsible for
	/// deferring the invocation until the instance does exist. For example until finished async loading.
	/// </summary>
	public static class NetInvocationDeferralRegistry
	{
		/// <summary>
		/// Called by generated methods when target object does not exist. If target object has been marked deferred
		/// then the method will be invoked after it exists.
		/// </summary>
		public static void Defer(NetId key, in ClientInvocationContext context, NetInvokeDeferred callback)
		{
			List<DeferredInvocation> list;
			if (deferrals.TryGetValue(key, out list))
			{
				NetPakReader reader = context.reader;
				DeferredInvocation deferredInvocation = new DeferredInvocation();
				deferredInvocation.netId = key;
				deferredInvocation.buffer = new byte[reader.RemainingSegmentLength];
				if (reader.SaveState(out deferredInvocation.scratch, out deferredInvocation.scratchBitCount, deferredInvocation.buffer))
				{
					deferredInvocation.methodInfo = context.clientMethodInfo;
					deferredInvocation.callback = callback;
					list.Add(deferredInvocation);
				}
				else
				{
					context.LogWarning("unable to save reader state for deferred invocation");
				}
			}
		}

		/// <summary>
		/// Add list of deferred invocations for key. Otherwise messages will be discarded assuming it was canceled.
		/// </summary>
		public static void MarkDeferred(NetId key, uint blockSize = 1)
		{
			List<DeferredInvocation> invocations;
			if (deferrals.TryGetValue(key, out invocations))
			{
#if LOG_INVOKE_ERRORS
				object target = NetIdRegistry.Get(key);
				UnturnedLog.warn($"Already added {(target != null ? target.ToString() : key.ToString())} x{blockSize} to net invocation deferral");
#endif // LOG_INVOKE_ERRORS
			}
			else
			{
				if (pool.Count > 0)
				{
					invocations = pool.GetAndRemoveTail();
				}
				else
				{
					invocations = new List<DeferredInvocation>();
				}
				for (uint offset = 0; offset < blockSize; ++offset)
				{
					deferrals.Add(key + offset, invocations);
				}
			}
		}

		/// <summary>
		/// Remove pending invocations.
		/// </summary>
		public static void Cancel(NetId key, uint blockSize = 1)
		{
			List<DeferredInvocation> list;
			if (deferrals.TryGetValue(key, out list))
			{
				for (uint offset = 0; offset < blockSize; ++offset)
				{
					deferrals.Remove(key + offset);
				}

				list.Clear();
				pool.Add(list);
			}
			else
			{
#if LOG_INVOKE_ERRORS
				object target = NetIdRegistry.Get(key);
				UnturnedLog.warn($"Already cancelled {(target != null ? target.ToString() : key.ToString())} x{blockSize} net invocation deferral, or was not added");
#endif // LOG_INVOKE_ERRORS
			}
		}

		private static void Invoke(object voidNetObj, DeferredInvocation invocation)
		{
			NetPakReader reader = NetMessages.GetInvokableReader();
			reader.LoadState(invocation.scratch, invocation.scratchBitCount, invocation.buffer, invocation.buffer.Length);

			ClientInvocationContext context = new ClientInvocationContext(ClientInvocationContext.EOrigin.Deferred, reader, invocation.methodInfo);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			invocation.methodInfo.deferredReadSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			try
			{
				invocation.callback(voidNetObj, context);

#if LOG_INVOKE_ERRORS
				if (reader.errors != NetPakReader.EErrorFlags.None)
				{
					UnturnedLog.error("Error {0} invoking {1} deferred", reader.errors, invocation.methodInfo);
				}
				else if (!reader.ReachedEndOfSegment)
				{
					UnturnedLog.warn("Did not read to end of invocation {0} deferred ({1} of {2} bytes)", invocation.methodInfo, reader.readByteIndex, reader.GetBufferSegmentLength());
				}
#endif // LOG_INVOKE_ERRORS
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception invoking {0} deferred:", invocation.methodInfo);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			invocation.methodInfo.deferredReadSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		/// <summary>
		/// Invoke all deferred calls.
		/// </summary>
		public static void Invoke(NetId key, uint blockSize = 1)
		{
			List<DeferredInvocation> list;
			if (deferrals.TryGetValue(key, out list))
			{
				for (uint offset = 0; offset < blockSize; ++offset)
				{
					deferrals.Remove(key + offset);
				}

				foreach (DeferredInvocation invocation in list)
				{
					object voidNetObj = NetIdRegistry.Get(invocation.netId);
					if (voidNetObj == null)
					{
						// Perhaps overkill, but this allows early exit of the deferred invocations if the instance
						// method destroys itself.
						break;
					}
					else
					{
						Invoke(voidNetObj, invocation);
					}
				}

				list.Clear();
				pool.Add(list);
			}
			else
			{
#if LOG_INVOKE_ERRORS
				object target = NetIdRegistry.Get(key);
				UnturnedLog.warn($"Already removed {(target != null ? target.ToString() : key.ToString())} x{blockSize} from net invocation deferral, or was not added");
#endif // LOG_INVOKE_ERRORS
			}
		}

		private struct DeferredInvocation
		{
			/// <summary>
			/// Invocations are grouped by net id block to ensure order is preserved between related objects. 
			/// </summary>
			public NetId netId;

			public uint scratch;
			public int scratchBitCount;
			public byte[] buffer;
			public ClientMethodInfo methodInfo;

			/// <summary>
			/// Not a member of ClientMethodInfo because it does not need to be looked up using reflection.
			/// </summary>
			public NetInvokeDeferred callback;
		}

		/// <summary>
		/// Called before loading level.
		/// </summary>
		internal static void Clear()
		{
			deferrals.Clear();
			pool.Clear();
		}

		private static Dictionary<NetId, List<DeferredInvocation>> deferrals = new Dictionary<NetId, List<DeferredInvocation>>();
		private static List<List<DeferredInvocation>> pool = new List<List<DeferredInvocation>>();
	}
}
