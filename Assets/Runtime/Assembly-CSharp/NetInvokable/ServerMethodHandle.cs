////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES

using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned
{
	public abstract class ServerMethodHandle
	{
		public override string ToString()
		{
			if (serverMethodInfo != null)
			{
				return serverMethodInfo.ToString();
			}
			else
			{
				return "invalid";
			}
		}

		protected static THandle GetInternal<THandle, TWriteDelegate>(System.Type declaringType, string methodName, System.Func<ServerMethodInfo, TWriteDelegate, THandle> makeHandle)
			where THandle : ServerMethodHandle
			where TWriteDelegate : System.Delegate
		{
			ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(declaringType, methodName);
			if (serverMethodInfo != null)
			{
				TWriteDelegate generatedWrite = NetReflection.CreateServerWriteDelegate<TWriteDelegate>(serverMethodInfo);
				if (generatedWrite != null)
				{
					return makeHandle(serverMethodInfo, generatedWrite);
				}
			}

			return null;
		}

		protected NetPakWriter GetWriterWithStaticHeader()
		{
			NetPakWriter writer = NetMessages.GetInvokableWriter();
			writer.Reset();
			writer.WriteEnum(EServerMessage.InvokeMethod);
			writer.WriteBits(serverMethodInfo.methodIndex, NetReflection.serverMethodsBitCount);
			return writer;
		}

		protected void SendAndLoopbackIfLocal(ENetReliability reliability, NetPakWriter writer)
		{
			writer.Flush();

#if LOG_INVOKE_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing invocation {1} to server", writer.errors, serverMethodInfo);
			}
#endif // LOG_INVOKE_ERRORS

#if !DEDICATED_SERVER
			if (!Provider.isConnected)
			{
				UnturnedLog.warn($"Ignoring request to invoke {this} on server because we are not connected");
				return;
			}

			if (!Provider.isServer)
			{
				Provider.clientTransport.Send(writer.buffer, writer.writeByteIndex, reliability);
				return;
			}
#endif // !DEDICATED_SERVER

			InvokeLoopback(writer);
		}

		protected ServerMethodHandle(ServerMethodInfo serverMethodInfo)
		{
			this.serverMethodInfo = serverMethodInfo;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			++serverMethodInfo.handleCount;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		protected ServerMethodInfo serverMethodInfo;

		private void InvokeLoopback(NetPakWriter writer)
		{
			NetPakReader reader = NetMessages.GetInvokableReader();
			reader.SetBufferSegmentCopy(writer.buffer, Provider.buffer, writer.writeByteIndex);
			reader.Reset();

			EServerMessage index;
			reader.ReadEnum(out index);
#if LOG_INVOKE_ERRORS
			if (index != EServerMessage.InvokeMethod)
			{
				UnturnedLog.error("Loopback {0} wrong message index {1}", serverMethodInfo, index);
				return;
			}
#endif // LOG_INVOKE_ERRORS

			uint methodIndex;
			reader.ReadBits(NetReflection.serverMethodsBitCount, out methodIndex);
#if LOG_INVOKE_ERRORS
			if (methodIndex != serverMethodInfo.methodIndex)
			{
				UnturnedLog.error("Loopback {0} method index {1} should be {2}", serverMethodInfo, methodIndex, serverMethodInfo.methodIndex);
				return;
			}
#endif // LOG_INVOKE_ERRORS

			SteamPlayer callingPlayer = null;
#if !DEDICATED_SERVER
			callingPlayer = Provider.clients[0];
#endif // !DEDICATED_SERVER

			ServerInvocationContext context = new ServerInvocationContext(ServerInvocationContext.EOrigin.Loopback, callingPlayer, reader, serverMethodInfo);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			serverMethodInfo.readSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			try
			{
				serverMethodInfo.readMethod(context);

#if LOG_INVOKE_ERRORS
				if (reader.errors != NetPakReader.EErrorFlags.None)
				{
					UnturnedLog.error("Error {0} invoking {1} from loopback", reader.errors, serverMethodInfo);
				}
				else if (!reader.ReachedEndOfSegment)
				{
					UnturnedLog.warn("Did not read to end of invocation {0} from loopback ({1} of {2} bytes)", serverMethodInfo, reader.readByteIndex, reader.GetBufferSegmentLength());
				}
#endif // LOG_INVOKE_ERRORS
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception invoking {0} by server loopback:", serverMethodInfo);
				UnturnedLog.error($"Additional context loopback calling stack trace:\n{System.Environment.StackTrace}");
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			serverMethodInfo.readSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}
	}
}
