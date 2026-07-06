////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_THIRDPARTYAC
using SDG.NetPak;
using System;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_ThirdpartyAntiCheat
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			if (Provider.battlEyeClientHandle != IntPtr.Zero && Provider.battlEyeClientRunData != null && Provider.battlEyeClientRunData.pfnReceivedPacket != null)
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
							Provider.battlEyeClientRunData.pfnReceivedPacket(packetAddress, (int) length);
						}
					}
				}
			}

		}
	}
}
#endif // WITH_THIRDPARTYAC
