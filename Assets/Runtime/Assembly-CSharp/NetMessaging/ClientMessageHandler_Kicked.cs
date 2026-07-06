////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_Kicked
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			string message;
			reader.ReadString(out message);

			Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
			Provider._connectionFailureReason = message;

			Provider.RequestDisconnect($"Kicked from server. Reason: \"{message}\"");
		}
	}
}
