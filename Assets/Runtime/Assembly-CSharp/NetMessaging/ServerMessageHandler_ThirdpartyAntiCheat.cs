////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_THIRDPARTYAC
using SDG.NetPak;
using SDG.NetTransport;
using System;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_ThirdpartyAntiCheat
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			if (Provider.battlEyeServerHandle != IntPtr.Zero && Provider.battlEyeServerRunData != null && Provider.battlEyeServerRunData.pfnReceivedPacket != null)
			{
				SteamPlayer player = Provider.findPlayer(transportConnection);
				if (player != null)
				{
					uint length;
					reader.ReadBits(Provider.battlEyeBufferSize.bitCount, out length);
					byte[] source;
					int bufferOffset;
					if (length > 0 && reader.ReadBytesPtr((int) length, out source, out bufferOffset))
					{
						unsafe
						{
							fixed (byte* bufferPtr = source)
							{
								IntPtr packetAddress = new IntPtr(bufferPtr + bufferOffset);
								Provider.battlEyeServerRunData.pfnReceivedPacket(player.thirdpartyAntiCheatId, packetAddress, (int) length);
							}
						}
					}
					else
					{
						UnturnedLog.warn("Received empty BattlEye payload from {0}, so we're refusing them", transportConnection);
						Provider.refuseGarbageConnection(transportConnection, "sv empty BE payload");
					}
				}
				else
				{
					if (NetMessages.shouldLogBadMessages)
					{
						UnturnedLog.info($"Ignoring BattlEye message from {transportConnection} because there is no associated player");
					}
					Provider.IncrementBadPacketsFromConnection(transportConnection);
				}
			}
			else
			{
				if (NetMessages.shouldLogBadMessages)
				{
					UnturnedLog.info($"Ignoring BattlEye message from {transportConnection} because BattlEye is not running");
				}
				Provider.IncrementBadPacketsFromConnection(transportConnection);
			}
		}
	}
}
#endif // WITH_THIRDPARTYAC
