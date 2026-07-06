////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;

namespace SDG.NetTransport.SteamNetworkingSockets
{
	/// <summary>
	/// Implementing as a struct wrapping the connection handle would remove the cost of looking up the connection,
	/// but implementing as a class makes it cheap to cache information like the remote identity.
	/// </summary>
	internal class TransportConnection_SteamNetworkingSockets : ITransportConnection
	{
		public TransportConnection_SteamNetworkingSockets(ServerTransport_SteamNetworkingSockets serverTransport, ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			this.serverTransport = serverTransport;
			steamConnectionHandle = callback.m_hConn;
			steamIdentity = callback.m_info.m_identityRemote;
		}

		public bool TryGetIPv4Address(out uint address)
		{
			SteamNetConnectionInfo_t info;
			if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out info))
			{
				address = info.m_addrRemote.GetIPv4();
				return address > 0;
			}
			else
			{
				address = 0;
				return false;
			}
		}

		public bool TryGetPort(out ushort port)
		{
			SteamNetConnectionInfo_t info;
			if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out info))
			{
				port = info.m_addrRemote.m_port;
				return port > 0;
			}
			else
			{
				port = 0;
				return false;
			}
		}

		public bool TryGetSteamId(out ulong steamId)
		{
			SteamNetConnectionInfo_t info;
			if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out info))
			{
				steamId = info.m_identityRemote.GetSteamID64();
				return steamId > 0;
			}
			else
			{
				steamId = default;
				return false;
			}
		}

		public System.Net.IPAddress GetAddress()
		{
			SteamNetConnectionInfo_t info;
			if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out info))
			{
				return new System.Net.IPAddress(info.m_addrRemote.m_ipv6);
			}
			else
			{
				return null;
			}
		}

		public string GetAddressString(bool withPort)
		{
			SteamNetConnectionInfo_t info;
			if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out info))
			{
				string buffer;
				info.m_addrRemote.ToString(out buffer, withPort);
				return buffer;
			}
			else
			{
				return null;
			}
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TransportConnection_SteamNetworkingSockets);
		}

		public bool Equals(TransportConnection_SteamNetworkingSockets other)
		{
			return other != null && steamConnectionHandle == other.steamConnectionHandle;
		}

		public bool Equals(ITransportConnection other)
		{
			return Equals(other as TransportConnection_SteamNetworkingSockets);
		}

		public override int GetHashCode()
		{
			return steamConnectionHandle.GetHashCode();
		}

		public override string ToString()
		{
			return serverTransport.IdentityToString(steamIdentity);
		}

		public void CloseConnection()
		{
			serverTransport.CloseConnection(this);
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
			int sendFlags = serverTransport.ReliabilityToSendFlags(reliability);

			EResult result;
			unsafe
			{
				fixed (byte* bufferPtr = buffer)
				{
					IntPtr bufferIntPtr = new IntPtr(bufferPtr);
					long messageNumber;
					result = SteamGameServerNetworkingSockets.SendMessageToConnection(steamConnectionHandle, bufferIntPtr, (uint) size, sendFlags, out messageNumber);
				}
			}

			if (result == EResult.k_EResultOK)
			{
				serverTransport.DebugLog("Server sent {0} byte message to client {1}", size, serverTransport.IdentityToString(steamIdentity));
			}
			else
			{
				serverTransport.DebugLog("Server error {0} sending {1} byte message to client {2}", result, size, serverTransport.IdentityToString(steamIdentity));
			}
		}

		internal bool wasClosed;
		internal HSteamNetConnection steamConnectionHandle;
		internal SteamNetworkingIdentity steamIdentity;
		private ServerTransport_SteamNetworkingSockets serverTransport;
	}
}
