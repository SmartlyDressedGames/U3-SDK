////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define LOG_RECEIVE_FROM_CLIENT_ERRORS
#define LOG_SEND_TO_CLIENT_ERRORS
#define LOG_SEND_TO_SERVER_ERRORS
#define LOG_RECEIVE_FROM_SERVER_ERRORS
#define PROFILE_NET_MESSAGE_READ_HANDLERS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.NetPak;
using SDG.NetTransport;
using System.Collections.Generic;

namespace SDG.Unturned
{
	internal static class NetMessages
	{
		public delegate void ClientWriteHandler(NetPakWriter writer);
		public delegate void ClientReadHandler(NetPakReader reader);
		public delegate void ServerReadHandler(ITransportConnection transportConnection, NetPakReader reader);

		public static void SendMessageToClient(EClientMessage index, ENetReliability reliability, ITransportConnection transportConnection, ClientWriteHandler callback)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!Provider.isServer)
			{
				// Loopback is handled by net invokables. Other messages do not use loopback.
				throw new System.Exception($"Only server can send message {index}");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			writer.Reset();
			writer.WriteEnum(index);
			callback(writer);
			writer.Flush();

#if LOG_SEND_TO_CLIENT_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing message {1} to client {2}", writer.errors, index, transportConnection);
			}
#endif // LOG_SEND_TO_CLIENT_ERRORS

			transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
		}

		public static void SendMessageToClients(EClientMessage index, ENetReliability reliability, List<ITransportConnection> transportConnections, ClientWriteHandler callback)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!Provider.isServer)
			{
				// Loopback is handled by net invokables. Other messages do not use loopback.
				throw new System.Exception($"Only server can send message {index}");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			writer.Reset();
			writer.WriteEnum(index);
			callback(writer);
			writer.Flush();

#if LOG_SEND_TO_CLIENT_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing conditional message {1}", writer.errors, index);
			}
#endif // LOG_SEND_TO_CLIENT_ERRORS

			foreach (ITransportConnection transportConnection in transportConnections)
			{
				transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
			}
		}

		[System.Obsolete]
		public static void SendMessageToClients(EClientMessage index, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, ClientWriteHandler callback)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				SendMessageToClients(index, reliability, list, callback);
			}
			else
			{
				throw new System.ArgumentException("should be a list", nameof(transportConnections));
			}
		}

		public static void SendMessageToServer(EServerMessage index, ENetReliability reliability, ClientWriteHandler callback)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (Provider.isServer)
			{
				// Loopback is handled by net invokables. Other messages do not use loopback.
				throw new System.Exception($"Only client can send message {index}");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (!Provider.isConnected)
			{
				UnturnedLog.warn($"Ignoring request to send message {index} to server because we are not connected");
				return;
			}

			writer.Reset();
			writer.WriteEnum(index);
			callback(writer);
			writer.Flush();

#if LOG_SEND_TO_SERVER_ERRORS
			if (writer.errors != NetPakWriter.EErrorFlags.None)
			{
				UnturnedLog.error("Error {0} writing message {1} to server", writer.errors, index);
			}
#endif // LOG_SEND_TO_SERVER_ERRORS

			Provider.clientTransport.Send(writer.buffer, writer.writeByteIndex, reliability);
		}

		public static void ReceiveMessageFromClient(ITransportConnection transportConnection, byte[] packet, int offset, int size)
		{
			reader.SetBufferSegment(packet, size);
			reader.Reset();

			EServerMessage index;
			if (!reader.ReadEnum(out index))
			{
				UnturnedLog.warn("Received invalid packet index from {0}, so we're refusing them", transportConnection);
				Provider.refuseGarbageConnection(transportConnection, "sv invalid packet index");
				return;
			}

			// Catching here prevents message loop from falling behind, and provides helpful context (message index),
			// but handlers should be as bulletproof as possible.
			try
			{
#if PROFILE_NET_MESSAGE_READ_HANDLERS
				serverSamplers[(int) index].Begin();
#endif // PROFILE_NET_MESSAGE_READ_HANDLERS
				serverReadCallbacks[(int) index]?.Invoke(transportConnection, reader);
#if PROFILE_NET_MESSAGE_READ_HANDLERS
				serverSamplers[(int) index].End();
#endif // PROFILE_NET_MESSAGE_READ_HANDLERS

#if LOG_RECEIVE_FROM_CLIENT_ERRORS
				if (reader.errors != NetPakReader.EErrorFlags.None)
				{
					UnturnedLog.error("Error {0} reading message {1} from client {2}", reader.errors, index, transportConnection);
				}
				else if (!reader.ReachedEndOfSegment)
				{
					// Note: imprecise because byte length is rounded up from bit length, but should help find
					// particularly egregious reading errors.
					UnturnedLog.warn("Did not read to end of message {0} from client {1}", index, transportConnection);
				}
#endif // LOG_RECEIVE_FROM_CLIENT_ERRORS
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception reading message {0} from client {1}:", index, transportConnection);
			}
		}

		public static void ReceiveMessageFromServer(byte[] packet, int offset, int size)
		{
			reader.SetBufferSegment(packet, size);
			reader.Reset();

			EClientMessage index;
			if (!reader.ReadEnum(out index))
			{
				UnturnedLog.error("Client received invalid message index from server");
				return;
			}

			// Catching here prevents message loop from falling behind, and provides helpful context (message index),
			// but handlers should be as bulletproof as possible.
			try
			{
				switch (index)
				{
					case EClientMessage.UPDATE_RELIABLE_BUFFER:
					case EClientMessage.UPDATE_UNRELIABLE_BUFFER:
						reader.AlignToByte();
						Provider.legacyReceiveClient(packet, offset, size);
						break;

					default:
						Provider.timeLastPacketWasReceivedFromServer = UnityEngine.Time.realtimeSinceStartup;

#if PROFILE_NET_MESSAGE_READ_HANDLERS
						clientSamplers[(int) index].Begin();
#endif // PROFILE_NET_MESSAGE_READ_HANDLERS
						clientReadCallbacks[(int) index]?.Invoke(reader);
#if PROFILE_NET_MESSAGE_READ_HANDLERS
						clientSamplers[(int) index].End();
#endif // PROFILE_NET_MESSAGE_READ_HANDLERS

						// We cannot do these for "legacy" messages yet because they use SteamPacker reader.
#if LOG_RECEIVE_FROM_SERVER_ERRORS
						if (reader.errors != NetPakReader.EErrorFlags.None)
						{
							UnturnedLog.error("Error {0} reading message {1} from server", reader.errors, index);
						}
						else if (!reader.ReachedEndOfSegment)
						{
							UnturnedLog.warn("Did not read to end of message {0} from server", index);
						}
#endif // LOG_RECEIVE_FROM_SERVER_ERRORS
						break;
				}
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Exception reading message {0} from server:", index);
			}
		}

		static NetMessages()
		{
			reader = new NetPakReader();
			writer = new NetPakWriter();
			writer.buffer = Block.buffer;

			clientReadCallbacks = new ClientReadHandler[System.Enum.GetNames(typeof(EClientMessage)).Length];
			clientReadCallbacks[(int) EClientMessage.PingRequest] = ClientMessageHandler_PingRequest.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.PingResponse] = ClientMessageHandler_PingResponse.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Shutdown] = ClientMessageHandler_Shutdown.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.PlayerConnected] = ClientMessageHandler_PlayerConnected.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.PlayerDisconnected] = ClientMessageHandler_PlayerDisconnected.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.DownloadWorkshopFiles] = ClientMessageHandler_DownloadWorkshopFiles.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Verify] = ClientMessageHandler_Verify.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Accepted] = ClientMessageHandler_Accepted.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Rejected] = ClientMessageHandler_Rejected.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Banned] = ClientMessageHandler_Banned.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Kicked] = ClientMessageHandler_Kicked.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Admined] = ClientMessageHandler_Admined.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.Unadmined] = ClientMessageHandler_Unadmined.ReadMessage;
#if WITH_THIRDPARTYAC
			clientReadCallbacks[(int) EClientMessage.ThirdpartyAntiCheat] = ClientMessageHandler_ThirdpartyAntiCheat.ReadMessage;
#endif // WITH_THIRDPARTYAC
			clientReadCallbacks[(int) EClientMessage.QueuePositionChanged] = ClientMessageHandler_QueuePositionChanged.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.InvokeMethod] = ClientMessageHandler_InvokeMethod.ReadMessage;
			clientReadCallbacks[(int) EClientMessage.ReplicateConfig] = ClientMessageHandler_ReplicateConfig.ReadMessage;

			serverReadCallbacks = new ServerReadHandler[System.Enum.GetNames(typeof(EServerMessage)).Length];
			serverReadCallbacks[(int) EServerMessage.GetWorkshopFiles] = ServerMessageHandler_GetWorkshopFiles.ReadMessage;
			serverReadCallbacks[(int) EServerMessage.ReadyToConnect] = ServerMessageHandler_ReadyToConnect.ReadMessage;
			serverReadCallbacks[(int) EServerMessage.Authenticate] = ServerMessageHandler_Authenticate.ReadMessage;
#if WITH_THIRDPARTYAC
			serverReadCallbacks[(int) EServerMessage.ThirdPartyAntiCheat] = ServerMessageHandler_ThirdpartyAntiCheat.ReadMessage;
#endif // WITH_THIRDPARTYAC
			serverReadCallbacks[(int) EServerMessage.PingRequest] = ServerMessageHandler_PingRequest.ReadMessage;
			serverReadCallbacks[(int) EServerMessage.PingResponse] = ServerMessageHandler_PingResponse.ReadMessage;
			serverReadCallbacks[(int) EServerMessage.InvokeMethod] = ServerMessageHandler_InvokeMethod.ReadMessage;
			serverReadCallbacks[(int) EServerMessage.ValidateAssets] = ServerMessageHandler_ValidateAssets.ReadMessage;
			serverReadCallbacks[(int) EServerMessage.GracefullyDisconnect] = ServerMessageHandler_GracefullyDisconnect.ReadMessage;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			// Ensure there are no null entries.
			// The reader arrays _should_ be sized accordingly to the number of compiled handlers.
			for (int index = 2; index < clientReadCallbacks.Length; ++index)
			{
				if (clientReadCallbacks[index] == null)
					UnturnedLog.info("Missing client message handler {0}", index);
			}
			for (int index = 0; index < serverReadCallbacks.Length; ++index)
			{
				if (serverReadCallbacks[index] == null)
					UnturnedLog.info("Missing server message handler {0}", index);
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

#if PROFILE_NET_MESSAGE_READ_HANDLERS
			clientSamplers = new UnityEngine.Profiling.CustomSampler[clientReadCallbacks.Length];
			for (int index = 2; index < clientSamplers.Length; ++index)
			{
				System.Reflection.MethodInfo method = clientReadCallbacks[index].Method;
				clientSamplers[index] = UnityEngine.Profiling.CustomSampler.Create($"{method.DeclaringType}.{method.Name}");
			}
			serverSamplers = new UnityEngine.Profiling.CustomSampler[serverReadCallbacks.Length];
			for (int index = 0; index < serverSamplers.Length; ++index)
			{
				System.Reflection.MethodInfo method = serverReadCallbacks[index].Method;
				serverSamplers[index] = UnityEngine.Profiling.CustomSampler.Create($"{method.DeclaringType}.{method.Name}");
			}
#endif // PROFILE_NET_MESSAGE_READ_HANDLERS
		}

		internal static NetPakReader GetInvokableReader()
		{
			return reader;
		}

		internal static NetPakWriter GetInvokableWriter()
		{
			return writer;
		}

		internal static CommandLineFlag shouldLogBadMessages = new CommandLineFlag(false, "-LogBadMessages");

		private static NetPakReader reader;
		private static NetPakWriter writer;
		private static ClientReadHandler[] clientReadCallbacks;
		private static ServerReadHandler[] serverReadCallbacks;
#if PROFILE_NET_MESSAGE_READ_HANDLERS
		private static UnityEngine.Profiling.CustomSampler[] clientSamplers;
		private static UnityEngine.Profiling.CustomSampler[] serverSamplers;
#endif // PROFILE_NET_MESSAGE_READ_HANDLERS
	}
}
