////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_UNETLLAPI
#pragma warning disable CS0618

using SDG.Unturned;
using UnityEngine.Networking;

namespace SDG.NetTransport.UNetLLAPI
{
	/// <summary>
	/// UNet is deprecated. This implementation exists to ensure the abstraction is solid.
	/// </summary>
	public class ClientTransport_UNetLLAPI : IClientTransport
	{
		public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
		{
			NetworkTransport.Init();

			ConnectionConfig defaultConfig = new ConnectionConfig();
			HostTopology topology = new HostTopology(defaultConfig, 1);
			serverHostId = NetworkTransport.AddHost(topology);

			string address = Parser.getIPFromUInt32(Unturned.Provider.currentServerInfo.ip);
			int port = Unturned.Provider.currentServerInfo.port;
			byte errorCode;
			serverConnectionId = NetworkTransport.Connect(serverHostId, address, port, 0, out errorCode);
			NetworkError error = (NetworkError) errorCode;
			if(error != NetworkError.Ok)
			{
				UnturnedLog.error("Client LLAPI connect error: {0}", error);
			}
		}

		public void TearDown()
		{
			NetworkTransport.Shutdown();
		}
		
		public void Send(byte[] buffer, long size, ESendType sendType)
		{
			byte error;
			NetworkTransport.Send(serverHostId, 0, 0, buffer, (int) size, out error);
		}

		public bool Receive(byte[] buffer, out long size)
		{
			int hostId = 0;

			int connectionId; // The connection ID that received the event.
			int channelId; // The channel ID associated with the event.
			int receivedSize; // The actual receive size of the data.
			byte error; // 	Error (can be cast to NetworkError for more information).
			NetworkEventType eventType = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, buffer, buffer.Length, out receivedSize, out error);

			switch(eventType)
			{
				// The event queue has nothing to report.
				case NetworkEventType.Nothing:
					break;

				// You have received a connect event.
				// This can be either a successful connect request, or a connection response.
				case NetworkEventType.ConnectEvent:
					// if(myConnectionId == connectionId)
						//my connect request was approved
					// else
						//somebody else sent a connect request to me
					break;

				case NetworkEventType.DataEvent:
					//  You have received a data event. You receive a data event when there is some data ready to be
					// recieved. If the recBuffer is big enough to contain data, data is copied into the buffer. If
					// not, the event contains a MessageToLong network error. If this happens, you need to reallocate
					// the buffer to a larger size and call the DataEvent function again.
					break;

				case NetworkEventType.DisconnectEvent:
					// Your established connection has disconnected, or your connect request has failed.
					// Check the error code to find out why this has happened.
					// if(myid == connectionId)
						//cannot connect for some reason, see error
					// else
						//one of the established connections has disconnected
					break;

				case NetworkEventType.BroadcastEvent:
					// Indicates that you have received a broadcast event, and you can now call GetBroadcastConnectionInfo
					// and GetBroadcastConnectionMessage to retrieve more information.
					break;
			}

			size = 0;
			return false;
		}

		private int serverHostId;
		private int serverConnectionId;
	}
}

#pragma warning restore CS0618
#endif // WITH_UNETLLAPI
