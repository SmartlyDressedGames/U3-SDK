////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES

using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_InvokeMethod
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			uint methodIndex;
			if (!reader.ReadBits(NetReflection.serverMethodsBitCount, out methodIndex))
			{
				Provider.refuseGarbageConnection(transportConnection, "unable to read method index");
				return;
			}

			if (methodIndex >= NetReflection.serverMethodsLength)
			{
				Provider.refuseGarbageConnection(transportConnection, "out of bounds method index");
				return;
			}

			SteamPlayer callingPlayer = Provider.findPlayer(transportConnection);
			if (callingPlayer == null)
			{
				if (NetMessages.shouldLogBadMessages)
				{
					UnturnedLog.info($"Ignoring InvokeMethod message from {transportConnection} because there is no associated player");
				}
				Provider.IncrementBadPacketsFromConnection(transportConnection);
				return;
			}

			ServerMethodInfo netMethod = NetReflection.serverMethods[(int) methodIndex];

			ServerInvocationContext context = new ServerInvocationContext(ServerInvocationContext.EOrigin.Remote, callingPlayer, reader, netMethod);

#if LOG_NETINVOCATION
			UnturnedLog.info("Receive {0}", netMethod);
#endif

			if (netMethod.rateLimitIndex >= 0)
			{
				float currentTime = Time.realtimeSinceStartup;
				float nextAllowedTime = callingPlayer.rpcAllowedTimes[netMethod.rateLimitIndex];
				if (currentTime < nextAllowedTime)
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					CommandWindow.LogWarningFormat("Hit {0} rate limit", netMethod.debugName);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					++callingPlayer.rpcHitCount[netMethod.rateLimitIndex];
					int threshold = Mathf.Max(2, Provider.configData.Server.Rate_Limit_Kick_Threshold);
					if (callingPlayer.rpcHitCount[netMethod.rateLimitIndex] >= threshold)
					{
						context.Kick($"significantly exceeded {netMethod} rate limit ({threshold} times in {netMethod.customAttribute.ratelimitSeconds} seconds)");
					}
					return;
				}
				else
				{
					callingPlayer.rpcAllowedTimes[netMethod.rateLimitIndex] = currentTime + netMethod.customAttribute.ratelimitSeconds;
					callingPlayer.rpcHitCount[netMethod.rateLimitIndex] = 0; // Reset hit count.
				}
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			netMethod.readSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			try
			{
				callingPlayer.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;

				netMethod.readMethod(context);

#if LOG_INVOKE_ERRORS
				if (reader.errors != NetPakReader.EErrorFlags.None)
				{
					UnturnedLog.error("Error {0} invoking {1} from client {2}", reader.errors, netMethod, transportConnection);
				}
				else if (!reader.ReachedEndOfSegment)
				{
					UnturnedLog.warn("Did not read to end of invocation {0} from client {1} ({2} of {3} bytes)", netMethod, transportConnection, reader.readByteIndex, reader.GetBufferSegmentLength());
				}
				reader.ResetErrors(); // Bypass default error messages.
#endif // LOG_INVOKE_ERRORS
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception invoking {0} from client {1}:", netMethod, transportConnection);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			netMethod.readSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}
	}
}
