////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_PlayerDisconnected
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			if (reader.ReadNetId(out NetId playerNetId))
			{
				SteamPlayer clientToRemove = NetIdRegistry.Get<SteamPlayer>(playerNetId);
				if (clientToRemove != null)
				{
					Provider.RemoveClient(clientToRemove);
				}
				else
				{
					UnturnedLog.info($"Received PlayerDisconnected message for unknown NetID: {playerNetId}");
				}
			}
		}
	}
}
