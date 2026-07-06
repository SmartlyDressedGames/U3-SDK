////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Net.Sockets;

namespace SDG.NetTransport.SystemSockets
{
	internal class TransportConnection_SystemSocket : ITransportConnection
	{
		public TransportConnection_SystemSocket(ServerTransport_SystemSockets serverTransport, Socket clientSocket)
		{
			this.serverTransport = serverTransport;
			this.clientSocket = clientSocket;
		}

		public bool TryGetIPv4Address(out uint address)
		{
			if (!wasClosed)
			{
				IPEndPoint endPoint = clientSocket.RemoteEndPoint as IPEndPoint;
				if (endPoint != null)
				{
					byte[] addressBytes = endPoint.Address.GetAddressBytes();
					if (addressBytes.Length == 4)
					{
						address = (uint) ((addressBytes[0] << 24) & 0xFF)
							| (uint) ((addressBytes[0] << 16) & 0xFF)
							| (uint) ((addressBytes[0] << 8) & 0xFF)
							| (uint) (addressBytes[0] & 0xFF);
						return true;
					}
				}
			}

			address = 0;
			return false;
		}

		public bool TryGetPort(out ushort port)
		{
			if (!wasClosed)
			{
				IPEndPoint endPoint = clientSocket.RemoteEndPoint as IPEndPoint;
				if (endPoint != null)
				{
					port = (ushort) endPoint.Port;
					return true;
				}
			}

			port = 0;
			return false;
		}

		public bool TryGetSteamId(out ulong steamId)
		{
			steamId = default;
			return false;
		}

		public System.Net.IPAddress GetAddress()
		{
			if (!wasClosed)
			{
				IPEndPoint endPoint = clientSocket.RemoteEndPoint as IPEndPoint;
				if (endPoint != null)
				{
					return endPoint.Address;
				}
			}

			return null;
		}

		public string GetAddressString(bool withPort)
		{
			if (!wasClosed)
			{
				IPEndPoint endPoint = clientSocket.RemoteEndPoint as IPEndPoint;
				if (endPoint != null)
				{
					string address = endPoint.Address.ToString();
					if (withPort)
					{
						address += ':';
						address += endPoint.Port;
					}
					return address;
				}
			}

			return null;
		}

		public void CloseConnection()
		{
			serverTransport.CloseConnection(this);
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
			if (wasClosed)
			{
				return;
			}

			messageQueue.SendMessage(clientSocket, buffer, (int) size);
		}

		bool IEquatable<ITransportConnection>.Equals(ITransportConnection other)
		{
			return ReferenceEquals(this, other);
		}

		public override int GetHashCode()
		{
			return clientSocket.GetHashCode();
		}

		public override string ToString()
		{
			if (wasClosed)
			{
				return "Closed Socket";
			}

			return clientSocket.RemoteEndPoint != null ? clientSocket.RemoteEndPoint.ToString() : "Invalid Socket";
		}

		public ServerTransport_SystemSockets serverTransport;
		public Socket clientSocket;
		public SocketMessageLayer messageQueue = new SocketMessageLayer();
		internal bool wasClosed;
	}
}
