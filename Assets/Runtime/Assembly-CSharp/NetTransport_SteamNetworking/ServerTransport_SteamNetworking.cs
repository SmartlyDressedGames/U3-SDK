////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.NetTransport.SteamNetworking
{
	/// <summary>
	/// SteamNetworking is deprecated.
	/// </summary>
	public class ServerTransport_SteamNetworking : TransportBase_SteamNetworking, IServerTransport
	{
		public void Initialize(ServerTransportConnectionFailureCallback connectionClosedCallback)
		{
			p2pSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
		}

		public void TearDown()
		{
			p2pSessionRequest.Dispose(); // Stop listening for connection requests.
		}

		public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
		{
			transportConnection = default;
			size = 0;

			int channel = 0;
			uint msgSize;
			CSteamID steamIDRemote;
			if (!SteamGameServerNetworking.ReadP2PPacket(buffer, (uint) buffer.Length, out msgSize, out steamIDRemote, channel))
			{
				return false;
			}

			if (msgSize > buffer.Length)
			{
				// Truncated packets are not logged because cheaters would abuse to spam log. 
				msgSize = (uint) buffer.Length;
			}

			size = msgSize;
			transportConnection = new TransportConnection_SteamNetworking(steamIDRemote);
			Log("Server received {0} byte message from {1}", size, steamIDRemote);
			return true;
		}

#pragma warning disable
		private Callback<P2PSessionRequest_t> p2pSessionRequest;
#pragma warning restore
		private void OnP2PSessionRequest(P2PSessionRequest_t callback)
		{
			CSteamID steamIDRemote = callback.m_steamIDRemote;

			if (Unturned.Provider.shouldNetIgnoreSteamId(steamIDRemote))
			{
				// Server blocked this player.
				Log("Server ignoring session request from blocked account {0}", steamIDRemote);
				return;
			}

			if (!steamIDRemote.BIndividualAccount())
			{
				// Only trust individual accounts (server, anons, etc shouldn't be talking to us)
				Log("Server ignoring session request from non-individual account ", steamIDRemote);
				return;
			}

			SteamGameServerNetworking.AcceptP2PSessionWithUser(steamIDRemote);
			Log("Server accepted session request from {0}", steamIDRemote);
		}
	}
}
