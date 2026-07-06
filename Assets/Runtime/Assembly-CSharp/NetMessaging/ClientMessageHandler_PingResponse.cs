////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_PingResponse
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			if (Provider.timeLastPingRequestWasSentToServer > 0)
			{
				float processingDelay = Time.deltaTime;

				Provider.lag(Time.realtimeSinceStartup - Provider.timeLastPingRequestWasSentToServer - processingDelay);
				Provider.timeLastPingRequestWasSentToServer = -1;
			}
		}
	}
}
