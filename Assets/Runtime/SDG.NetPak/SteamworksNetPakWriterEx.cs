////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.NetPak
{
	public static class SteamworksNetPakWriterEx
	{
		public static bool WriteSteamID(this NetPakWriter writer, CSteamID value)
		{
			return writer.WriteUInt64(value.m_SteamID);
		}

		public static bool WriteSteamID(this NetPakWriter writer, PublishedFileId_t value)
		{
			return writer.WriteUInt64(value.m_PublishedFileId);
		}

		public static bool WriteSteamItemDefID(this NetPakWriter writer, SteamItemDef_t value)
		{
			return writer.WriteInt32(value.m_SteamItemDef);
		}

		public static bool WriteSteamItemInstanceID(this NetPakWriter writer, SteamItemInstanceID_t value)
		{
			return writer.WriteUInt64(value.m_SteamItemInstanceID);
		}
	}
}
