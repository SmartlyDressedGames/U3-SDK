////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
// #define LOG_INVOKE_SEND

using SDG.NetPak;
using SDG.NetTransport;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public abstract class ClientMethodHandle
	{
		public override string ToString()
		{
			if (clientMethodInfo != null)
			{
				return clientMethodInfo.ToString();
			}
			else
			{
				return "invalid";
			}
		}

		protected static THandle GetInternal<THandle, TWriteDelegate>(System.Type declaringType, string methodName, System.Func<ClientMethodInfo, TWriteDelegate, THandle> makeHandle)
			where THandle : ClientMethodHandle
			where TWriteDelegate : System.Delegate
		{
			ClientMethodInfo clientMethodInfo = NetReflection.GetClientMethodInfo(declaringType, methodName);
			if (clientMethodInfo != null)
			{
				TWriteDelegate generatedWrite = NetReflection.CreateClientWriteDelegate<TWriteDelegate>(clientMethodInfo);
				if (generatedWrite != null)
				{
					return makeHandle(clientMethodInfo, generatedWrite);
				}
			}

			return null;
		}

		/// <summary>
		/// Write header common to both static and instance methods, and return writer.
		/// </summary>
		protected NetPakWriter GetWriterWithStaticHeader()
		{
			NetPakWriter writer = NetMessages.GetInvokableWriter();
			writer.Reset();
			writer.WriteEnum(EClientMessage.InvokeMethod);
			writer.WriteBits(clientMethodInfo.methodIndex, NetReflection.clientMethodsBitCount);
			return writer;
		}

		protected void SendAndLoopbackIfLocal(ENetReliability reliability, ITransportConnection transportConnection, NetPakWriter writer)
		{
			writer.Flush();

#if LOG_INVOKE_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing invocation {1} to client {2}", writer.errors, clientMethodInfo, transportConnection);
			}
#endif // LOG_INVOKE_ERRORS

#if LOG_INVOKE_SEND
			UnturnedLog.info("{0} send {1} bytes to client {2}", this, writer.writeByteIndex, transportConnection);
#endif // LOG_INVOKE_SEND

#if !DEDICATED_SERVER
			if (!Dedicator.IsDedicatedServer)
			{
				// Until listen server is supported all non-dedicated connections are local.
				InvokeLoopback(writer);
				return;
			}
#endif // !DEDICATED_SERVER

			transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
		}

		protected void SendAndLoopbackIfAnyAreLocal(ENetReliability reliability, List<ITransportConnection> transportConnections, NetPakWriter writer)
		{
			writer.Flush();

#if LOG_INVOKE_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing invocation {1} to clients", writer.errors, clientMethodInfo);
			}
#endif // LOG_INVOKE_ERRORS

#if !DEDICATED_SERVER
			// Only loopback if a connection was provided. For example in singleplayer EnumerateRemote returns empty.
			bool shouldLoopback = false;
#endif // !DEDICATED_SERVER

			foreach (ITransportConnection tc in transportConnections)
			{
#if LOG_INVOKE_SEND
				UnturnedLog.info("{0} send {1} bytes to client {2}", this, writer.writeByteIndex, tc);
#endif // LOG_INVOKE_SEND

#if !DEDICATED_SERVER
				if (!Dedicator.IsDedicatedServer)
				{
					// Until listen server is supported all non-dedicated connections are local.
					shouldLoopback = true;
					break;
				}
#endif // !DEDICATED_SERVER

				tc.Send(writer.buffer, writer.writeByteIndex, reliability);
			}

#if !DEDICATED_SERVER
			if (shouldLoopback)
			{
				InvokeLoopback(writer);
			}
#endif // !DEDICATED_SERVER
		}

		protected void SendAndLoopback(ENetReliability reliability, List<ITransportConnection> transportConnections, NetPakWriter writer)
		{
			writer.Flush();

#if LOG_INVOKE_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing invocation {1} to clients with loopback", writer.errors, clientMethodInfo);
			}
#endif // LOG_INVOKE_ERRORS

			foreach (ITransportConnection tc in transportConnections)
			{
#if LOG_INVOKE_SEND
				UnturnedLog.info("{0} send {1} bytes to client {2}", this, writer.writeByteIndex, tc);
#endif // LOG_INVOKE_SEND

#if !DEDICATED_SERVER
				if (!Dedicator.IsDedicatedServer)
				{
					// Until listen server is supported all non-dedicated connections are local.
					UnturnedLog.error("Local connection {0} passed to SendAndLoopback {1}", tc, this);
					break;
				}
#endif // !DEDICATED_SERVER

				tc.Send(writer.buffer, writer.writeByteIndex, reliability);
			}

			InvokeLoopback(writer);
		}

		protected ClientMethodHandle(ClientMethodInfo clientMethodInfo)
		{
			this.clientMethodInfo = clientMethodInfo;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			++clientMethodInfo.handleCount;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		protected ClientMethodInfo clientMethodInfo;

		private void InvokeLoopback(NetPakWriter writer)
		{
			NetPakReader reader = NetMessages.GetInvokableReader();
			reader.SetBufferSegmentCopy(writer.buffer, Provider.buffer, writer.writeByteIndex);
			reader.Reset();

			EClientMessage index;
			reader.ReadEnum(out index);
#if LOG_INVOKE_ERRORS
			if (index != EClientMessage.InvokeMethod)
			{
				UnturnedLog.error("Loopback {0} wrong message index {1}", clientMethodInfo, index);
				return;
			}
#endif // LOG_INVOKE_ERRORS

			uint methodIndex;
			reader.ReadBits(NetReflection.clientMethodsBitCount, out methodIndex);
#if LOG_INVOKE_ERRORS
			if (methodIndex != clientMethodInfo.methodIndex)
			{
				UnturnedLog.error("Loopback {0} method index {1} should be {2}", clientMethodInfo, methodIndex, clientMethodInfo.methodIndex);
				return;
			}
#endif // LOG_INVOKE_ERRORS

			ClientInvocationContext context = new ClientInvocationContext(ClientInvocationContext.EOrigin.Loopback, reader, clientMethodInfo);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			clientMethodInfo.readSampler.Begin();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			try
			{
				clientMethodInfo.readMethod(context);

#if LOG_INVOKE_ERRORS
				if (reader.errors != NetPakReader.EErrorFlags.None)
				{
					UnturnedLog.error("Error {0} invoking {1} from loopback", reader.errors, clientMethodInfo);
				}
				else if (!reader.ReachedEndOfSegment)
				{
					UnturnedLog.warn("Did not read to end of invocation {0} from loopback ({1} of {2} bytes)", clientMethodInfo, reader.readByteIndex, reader.GetBufferSegmentLength());
				}
#endif // LOG_INVOKE_ERRORS
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception invoking {0} by client loopback:", clientMethodInfo);
				UnturnedLog.error($"Additional context loopback calling stack trace:\n{System.Environment.StackTrace}");
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			clientMethodInfo.readSampler.End();
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}
	}
}
