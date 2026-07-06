////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.NetTransport.SteamNetworking
{
	internal struct TransportConnection_SteamNetworking : ITransportConnection
	{
		public TransportConnection_SteamNetworking(CSteamID steamId)
		{
			this.steamId = steamId;
		}

		public CSteamID steamId;

		public bool TryGetIPv4Address(out uint address)
		{
			P2PSessionState_t state;
			if (SteamGameServerNetworking.GetP2PSessionState(steamId, out state) && state.m_bUsingRelay == 0)
			{
				address = state.m_nRemoteIP;
				return true;
			}
			else
			{
				address = 0;
				return false;
			}
		}

		public bool TryGetPort(out ushort port)
		{
			P2PSessionState_t state;
			if (SteamGameServerNetworking.GetP2PSessionState(steamId, out state) && state.m_bUsingRelay == 0)
			{
				port = state.m_nRemotePort;
				return true;
			}
			else
			{
				port = 0;
				return false;
			}
		}

		public bool TryGetSteamId(out ulong steamId)
		{
			steamId = this.steamId.m_SteamID;
			return steamId > 0;
		}

		public System.Net.IPAddress GetAddress()
		{
			P2PSessionState_t state;
			if (SteamGameServerNetworking.GetP2PSessionState(steamId, out state) && state.m_bUsingRelay == 0)
			{
				return new System.Net.IPAddress(state.m_nRemoteIP);
			}
			else
			{
				return null;
			}
		}

		public string GetAddressString(bool withPort)
		{
			P2PSessionState_t state;
			if (SteamGameServerNetworking.GetP2PSessionState(steamId, out state) && state.m_bUsingRelay == 0)
			{
				string address = Unturned.Parser.getIPFromUInt32(state.m_nRemoteIP);
				if (withPort)
				{
					address += ':';
					address += state.m_nRemotePort;
				}
				return address;
			}
			else
			{
				return null;
			}
		}

		public void CloseConnection()
		{
			SteamGameServerNetworking.CloseP2PSessionWithUser(steamId);
			TransportBase_SteamNetworking.Log("Server closed session with {0}", steamId);
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
			if (Unturned.Provider.shouldNetIgnoreSteamId(steamId))
			{
				// Do not send otherwise AcceptP2PSession is implied again.
				return;
			}

			EP2PSend steamSendType;
			switch (reliability)
			{
				case ENetReliability.Reliable:
					steamSendType = EP2PSend.k_EP2PSendReliableWithBuffering;
					break;

				default:
				case ENetReliability.Unreliable:
					steamSendType = EP2PSend.k_EP2PSendUnreliable;
					break;
			}

			const int channel = 0;
			bool success = SteamGameServerNetworking.SendP2PPacket(steamId, buffer, (uint) size, steamSendType, channel);
			if (success)
			{
				TransportBase_SteamNetworking.Log("Sent {0} byte {1} message to {2} on channel {3}", size, steamSendType, steamId, channel);
			}
			else
			{
				TransportBase_SteamNetworking.Log("Error sending {0} byte {1} message to {2} on channel {3}", size, steamSendType, steamId, channel);
			}
		}

		public override bool Equals(object obj)
		{
			return obj is TransportConnection_SteamNetworking && steamId == ((TransportConnection_SteamNetworking) obj).steamId;
		}

		public bool Equals(TransportConnection_SteamNetworking other)
		{
			return steamId == other.steamId;
		}

		public bool Equals(ITransportConnection other)
		{
			return other is TransportConnection_SteamNetworking && steamId == ((TransportConnection_SteamNetworking) other).steamId;
		}

		public override int GetHashCode()
		{
			return steamId.GetHashCode();
		}

		public override string ToString()
		{
			return steamId.ToString();
		}

		public static implicit operator CSteamID(TransportConnection_SteamNetworking clientId)
		{
			return clientId.steamId;
		}

		public static bool operator ==(TransportConnection_SteamNetworking lhs, TransportConnection_SteamNetworking rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TransportConnection_SteamNetworking lhs, TransportConnection_SteamNetworking rhs)
		{
			return !(lhs == rhs);
		}
	}
}
