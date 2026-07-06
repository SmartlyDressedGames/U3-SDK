////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_PingResponse
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			SteamPlayer player = Provider.findPlayer(transportConnection);
			if (player != null)
			{
				if (player.timeLastPingRequestWasSentToClient > 0)
				{
					float processingDelay = Time.deltaTime;
					player.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
					player.lag(Time.realtimeSinceStartup - player.timeLastPingRequestWasSentToClient - processingDelay);
					player.timeLastPingRequestWasSentToClient = -1;
				}

				return;
			}

			// May be from a player that just disconnected, so don't refuse them.
			if (NetMessages.shouldLogBadMessages)
			{
				UnturnedLog.info($"Ignoring PingResponse message from {transportConnection} because there is no associated player");
			}
			Provider.IncrementBadPacketsFromConnection(transportConnection);
		}
	}
}
