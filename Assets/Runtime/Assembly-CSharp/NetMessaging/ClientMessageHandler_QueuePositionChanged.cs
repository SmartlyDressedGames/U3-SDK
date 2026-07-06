////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_QueuePositionChanged
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			if (Provider.isWaitingForConnectResponse)
			{
				Provider.isWaitingForConnectResponse = false;
				UnturnedLog.info("Connection pending verification");
			}

			byte oldQueuePosition = Provider.queuePosition;
			byte newQueuePosition;
			reader.ReadUInt8(out newQueuePosition);
			Provider._queuePosition = newQueuePosition;

			if (oldQueuePosition != newQueuePosition)
			{
				UnturnedLog.info("Queue position: {0}", Provider.queuePosition);
			}

			Provider.onQueuePositionUpdated?.Invoke();
		}
	}
}
