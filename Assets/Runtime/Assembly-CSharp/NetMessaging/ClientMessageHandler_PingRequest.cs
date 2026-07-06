////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_PingRequest
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			NetMessages.SendMessageToServer(EServerMessage.PingResponse, NetTransport.ENetReliability.Unreliable, (NetPakWriter writer) => { });
		}
	}
}
