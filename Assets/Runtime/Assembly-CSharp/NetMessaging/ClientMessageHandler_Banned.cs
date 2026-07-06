////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_Banned
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			string reason;
			reader.ReadString(out reason);
			uint duration;
			reader.ReadUInt32(out duration);

			Provider._connectionFailureInfo = ESteamConnectionFailureInfo.BANNED;
			Provider._connectionFailureReason = reason;
			Provider._connectionFailureDuration = duration;

			Provider.RequestDisconnect($"Banned from server. Reason: \"{reason}\" Duration: {System.TimeSpan.FromSeconds(duration)}");
		}
	}
}
