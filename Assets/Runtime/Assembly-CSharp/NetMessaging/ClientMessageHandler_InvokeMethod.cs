////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES

using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_InvokeMethod
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			uint methodIndex;
			if (!reader.ReadBits(NetReflection.clientMethodsBitCount, out methodIndex))
			{
				UnturnedLog.warn("unable to read method index");
				return;
			}

			if (methodIndex >= NetReflection.clientMethodsLength)
			{
				UnturnedLog.warn("out of bounds method index ({0}/{1})", methodIndex, NetReflection.clientMethodsLength);
				return;
			}

			ClientMethodInfo netMethod = NetReflection.clientMethods[(int) methodIndex];

#if LOG_NETINVOCATION
			UnturnedLog.info("Receive {0}", netMethod);
#endif

			ClientInvocationContext context = new ClientInvocationContext(ClientInvocationContext.EOrigin.Remote, reader, netMethod);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			netMethod.readSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			try
			{
				netMethod.readMethod(context);

#if LOG_INVOKE_ERRORS
				if (reader.errors != NetPakReader.EErrorFlags.None)
				{
					UnturnedLog.error("Error {0} invoking {1} from server", reader.errors, netMethod);
				}
				else if (!reader.ReachedEndOfSegment)
				{
					UnturnedLog.warn("Did not read to end of invocation {0} from server ({1} of {2} bytes)", netMethod, reader.readByteIndex, reader.GetBufferSegmentLength());
				}
				reader.ResetErrors(); // Bypass default error messages.
#endif // LOG_INVOKE_ERRORS
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception invoking {0} from server:", netMethod);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			netMethod.readSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}
	}
}
