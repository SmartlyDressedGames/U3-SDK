////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_Unadmined
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			byte channel;
			reader.ReadUInt8(out channel);

			SteamPlayer client = PlayerTool.findSteamPlayerByChannel(channel);
			if (client != null)
			{
				client.isAdmin = false;
			}
			else
			{
				UnturnedLog.error("Unadmined unable to find channel {0}", channel);
			}
		}
	}
}
