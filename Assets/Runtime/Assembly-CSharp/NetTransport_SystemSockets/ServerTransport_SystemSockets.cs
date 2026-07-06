////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SDG.NetTransport.SystemSockets
{
	/// <summary>
	/// Implementation using .NET Berkeley sockets.
	/// </summary>
	public class ServerTransport_SystemSockets : TransportBase_SystemSockets, IServerTransport
	{
		internal struct PendingMessage
		{
			public TransportConnection_SystemSocket transportConnection;
			public byte[] buffer;
		}

		public void Initialize(ServerTransportConnectionFailureCallback connectionClosedCallback)
		{
			int port = Unturned.Provider.GetServerConnectionPort();

			IPAddress address;
			if (!IPAddress.TryParse(Unturned.Provider.bindAddress, out address))
			{
				Log("Unable to parse \"{0}\" as listen bind address", Unturned.Provider.bindAddress);
				address = IPAddress.Any;
			}

			IPEndPoint localEndPoint = new IPEndPoint(address, port);
			listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenSocket.Blocking = false;
			listenSocket.Bind(localEndPoint);

			Log("Server listen socket bound to {0}", localEndPoint);

			const int backlog = 10; // Maximum length of the pending connections queue.
			listenSocket.Listen(backlog);

			SDG.Framework.Utilities.TimeUtility.updated += OnUpdate;
		}

		public void TearDown()
		{
			SDG.Framework.Utilities.TimeUtility.updated -= OnUpdate;

			listenSocket.Close();
			listenSocket = null;

			foreach (TransportConnection_SystemSocket connection in connections)
			{
				connection.clientSocket.Close();
			}
			connections.Clear();
		}

		public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
		{
			if (listenSocket == null)
			{
				size = 0;
				transportConnection = default;
				return false; // In the process of tearing down.
			}

			if (messages.Count > 0)
			{
				PendingMessage message = messages.Dequeue();
				message.buffer.CopyTo(buffer, 0);
				size = message.buffer.Length;
				transportConnection = message.transportConnection;
				return true;
			}

			transportConnection = default;
			size = 0;
			return false;
		}

		internal void CloseConnection(TransportConnection_SystemSocket connection)
		{
			if (connection.wasClosed)
			{
				return;
			}

			connection.wasClosed = true;
			connection.clientSocket.Close();
			connections.RemoveFast(connection);
		}

		private void OnUpdate()
		{
			foreach (TransportConnection_SystemSocket socketConnection in connections)
			{
				socketConnection.messageQueue.ReceiveMessages(socketConnection.clientSocket);

				byte[] buffer;
				while (socketConnection.messageQueue.DequeueMessage(out buffer))
				{
					PendingMessage message = new PendingMessage();
					message.transportConnection = socketConnection;
					message.buffer = buffer;
					messages.Enqueue(message);
				}
			}

			if (Unturned.Provider.hasRoomForNewConnection)
			{
				try
				{
					Socket clientSocket = listenSocket.Accept();
					clientSocket.Blocking = false;
					TransportConnection_SystemSocket newConnection = new TransportConnection_SystemSocket(this, clientSocket);
					connections.Add(newConnection);
					Log("Server socket accepted connection from {0}", clientSocket.RemoteEndPoint);
				}
				catch
				{
					// Non-blocking socket throws an exception if there are no pending connections.
				}
			}
		}

		private Socket listenSocket;
		private List<TransportConnection_SystemSocket> connections = new List<TransportConnection_SystemSocket>();
		private Queue<PendingMessage> messages = new Queue<PendingMessage>();
	}
}
