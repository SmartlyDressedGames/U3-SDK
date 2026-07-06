////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using Unturned.SystemEx;

namespace SDG.NetTransport.SteamNetworking
{
	/// <summary>
	/// SteamNetworking is deprecated.
	/// </summary>
	public class ClientTransport_SteamNetworking : TransportBase_SteamNetworking, IClientTransport
	{
		public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
		{
			p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);

			callback();
		}

		public void TearDown()
		{
			p2pSessionRequest.Dispose(); // Stop listening for connection requests.

			Steamworks.SteamNetworking.CloseP2PSessionWithUser(serverId);
			Log("Client closing session with {0}", serverId);
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
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
			bool success = Steamworks.SteamNetworking.SendP2PPacket(serverId, buffer, (uint) size, steamSendType, channel);
			if (success)
			{
				Log("Sent {0} byte {1} message to {2} on channel {3}", size, steamSendType, serverId, channel);
			}
			else
			{
				Log("Error sending {0} byte {1} message to {2} on channel {3}", size, steamSendType, serverId, channel);
			}
		}

		public bool Receive(byte[] buffer, out long size)
		{
			size = 0;

			int channel = 0;
			uint msgSize;
			CSteamID steamIDRemote;
			if (!Steamworks.SteamNetworking.ReadP2PPacket(buffer, (uint) buffer.Length, out msgSize, out steamIDRemote, channel))
			{
				return false;
			}

			if (steamIDRemote != serverId)
			{
				Log("Client received unsolicited packet from {0}", steamIDRemote);
				return false;
			}

			size = msgSize;
			Log("Client received {0} byte message from {1}", size, steamIDRemote);
			return true;
		}

		public bool TryGetIPv4Address(out IPv4Address address)
		{
			P2PSessionState_t state;
			if (Steamworks.SteamNetworking.GetP2PSessionState(serverId, out state) && state.m_bUsingRelay == 0)
			{
				address = new IPv4Address(state.m_nRemoteIP);
				return address.value > 0;
			}
			else
			{
				address = IPv4Address.Zero;
				return false;
			}
		}

		public bool TryGetConnectionPort(out ushort connectionPort)
		{
			P2PSessionState_t state;
			if (Steamworks.SteamNetworking.GetP2PSessionState(serverId, out state) && state.m_bUsingRelay == 0)
			{
				connectionPort = state.m_nRemotePort;
				return connectionPort > 0;
			}
			else
			{
				connectionPort = 0;
				return false;
			}
		}

		public bool TryGetQueryPort(out ushort queryPort)
		{
			P2PSessionState_t state;
			if (Steamworks.SteamNetworking.GetP2PSessionState(serverId, out state) && state.m_bUsingRelay == 0)
			{
				queryPort = SDG.Unturned.MathfEx.ClampToUShort(state.m_nRemotePort - 1);
				return queryPort > 0;
			}
			else
			{
				queryPort = 0;
				return false;
			}
		}

		public bool TryGetPing(out int pingMs)
		{
			pingMs = 0;
			return false;
		}

#pragma warning disable
		private static Callback<P2PSessionRequest_t> p2pSessionRequest;
#pragma warning restore
		private void OnP2PSessionRequest(P2PSessionRequest_t callback)
		{
			if (callback.m_steamIDRemote == serverId)
			{
				Steamworks.SteamNetworking.AcceptP2PSessionWithUser(serverId);
				Log("Client accepted session request from {0}", serverId);
			}
			else
			{
				Log("Client ignoring unsolicited session request from {0}", callback.m_steamIDRemote);
			}
		}

		private CSteamID serverId => Unturned.Provider.server;
	}
}
