////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_Shutdown
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			string reason;
			reader.ReadString(out reason);

			Provider._connectionFailureInfo = ESteamConnectionFailureInfo.SHUTDOWN;
			Provider._connectionFailureReason = reason;

			Provider.RequestDisconnect($"Server was shutdown --- Reason: \"{reason}\"");
		}
	}
}
