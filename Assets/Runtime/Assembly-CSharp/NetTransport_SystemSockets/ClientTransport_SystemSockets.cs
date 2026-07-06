////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Net;
using System.Net.Sockets;
using Unturned.SystemEx;

namespace SDG.NetTransport.SystemSockets
{
	/// <summary>
	/// Implementation using .NET Berkeley sockets.
	/// </summary>
	public class ClientTransport_SystemSockets : TransportBase_SystemSockets, IClientTransport
	{
		public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
		{
			uint ip = Unturned.Provider.CurrentServerConnectParameters.address.value;
			long address = ((ip & 0xFF) << 24)
				| (((ip >> 8) & 0xFF) << 16)
				| (((ip >> 16) & 0xFF) << 8)
				| ((ip >> 24) & 0xFF);
			int port = Unturned.Provider.CurrentServerConnectParameters.connectionPort;
			remoteAddress = ip;
			remotePort = Unturned.Provider.CurrentServerConnectParameters.connectionPort;
			IPEndPoint endPoint = new IPEndPoint(address, port);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Log("Client socket connecting to {0}", endPoint);
			socket.Connect(endPoint);
			socket.Blocking = false;
			messageQueue = new SocketMessageLayer();

			SDG.Framework.Utilities.TimeUtility.updated += OnUpdate;

			callback();
		}

		public void TearDown()
		{
			SDG.Framework.Utilities.TimeUtility.updated -= OnUpdate;

			socket.Close();
			socket = null;
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
			if (socket == null)
				return; // In the process of tearing down.

			messageQueue.SendMessage(socket, buffer, (int) size);
		}

		public bool Receive(byte[] buffer, out long size)
		{
			if (socket == null) // In the process of tearing down.
			{
				size = 0;
				return false;
			}

			byte[] messageBuffer;
			if (messageQueue.DequeueMessage(out messageBuffer))
			{
				messageBuffer.CopyTo(buffer, 0);
				size = messageBuffer.Length;
				return true;
			}

			size = 0;
			return false;
		}

		public bool TryGetIPv4Address(out IPv4Address address)
		{
			address = new IPv4Address(remoteAddress);
			return true;
		}

		public bool TryGetConnectionPort(out ushort connectionPort)
		{
			connectionPort = remotePort;
			return true;
		}

		public bool TryGetQueryPort(out ushort queryPort)
		{
			queryPort = SDG.Unturned.MathfEx.ClampToUShort(remotePort - 1);
			return true;
		}

		public bool TryGetPing(out int pingMs)
		{
			pingMs = 0;
			return false;
		}

		private void OnUpdate()
		{
			messageQueue.ReceiveMessages(socket);
		}

		private Socket socket;
		private SocketMessageLayer messageQueue;
		private uint remoteAddress;
		private ushort remotePort;
	}
}
