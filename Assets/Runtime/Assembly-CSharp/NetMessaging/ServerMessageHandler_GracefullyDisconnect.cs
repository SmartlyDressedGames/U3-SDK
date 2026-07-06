////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_GracefullyDisconnect
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			SteamPlayer player = Provider.findPlayer(transportConnection);
			if (player == null)
			{
				if (NetMessages.shouldLogBadMessages)
				{
					UnturnedLog.info($"Ignoring GracefullyDisconnect message from {transportConnection} because there is no associated player");
				}
				Provider.IncrementBadPacketsFromConnection(transportConnection);
				return;
			}

			UnturnedLog.info($"Removing player {transportConnection} after graceful disconnect message");
			Provider.dismiss(player.playerID.steamID);
		}
	}
}
