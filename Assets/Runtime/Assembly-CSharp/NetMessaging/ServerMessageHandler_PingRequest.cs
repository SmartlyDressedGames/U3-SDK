////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_PingRequest
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			SteamPending pendingPlayer = Provider.findPendingPlayer(transportConnection);
			if (pendingPlayer != null)
			{
				if (pendingPlayer.averagePingRequestsReceivedPerSecond > Provider.PING_REQUEST_INTERVAL * 2)
				{
					// UnturnedLog.warn("Receiving ping request spam from " + steamID + ", so we're refusing them");
					// refuseGarbageConnection(steamID, "sv pending ping request spam");
					if (NetMessages.shouldLogBadMessages)
					{
						UnturnedLog.info($"Ignoring PingRequest message from {transportConnection} because they exceeded rate limit");
					}
					Provider.IncrementBadPacketsFromConnection(transportConnection);
				}
				else
				{
					pendingPlayer.lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
					pendingPlayer.incrementNumPingRequestsReceived();

					NetMessages.SendMessageToClient(EClientMessage.PingResponse, ENetReliability.Unreliable, transportConnection, (NetPakWriter writer) => { });
				}
				return;
			}

			SteamPlayer player = Provider.findPlayer(transportConnection);
			if (player != null)
			{
				if (player.averagePingRequestsReceivedPerSecond > Provider.PING_REQUEST_INTERVAL * 2)
				{
					// UnturnedLog.warn("Receiving ping request spam from " + steamID + ", so we're refusing them");
					// refuseGarbageConnection(steamID, "sv auth ping request spam");
					if (NetMessages.shouldLogBadMessages)
					{
						UnturnedLog.info($"Ignoring PingRequest message from {transportConnection} because they exceeded rate limit");
					}
					Provider.IncrementBadPacketsFromConnection(transportConnection);
					return; // Simply ignore request.
				}
				else
				{
					player.lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
					player.incrementNumPingRequestsReceived();

					NetMessages.SendMessageToClient(EClientMessage.PingResponse, ENetReliability.Unreliable, transportConnection, (NetPakWriter writer) => { });
					return;
				}
			}

			if (NetMessages.shouldLogBadMessages)
			{
				UnturnedLog.info($"Ignoring PingRequest message from {transportConnection} because there is no associated player");
			}
			Provider.IncrementBadPacketsFromConnection(transportConnection);
		}
	}
}
