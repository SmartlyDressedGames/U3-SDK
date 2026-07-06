////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.NetPak
{
	public static class SteamworksNetPakReaderEx
	{
		public static bool ReadSteamID(this NetPakReader reader, out CSteamID value)
		{
			return reader.ReadUInt64(out value.m_SteamID);
		}

		public static bool ReadSteamID(this NetPakReader reader, out PublishedFileId_t value)
		{
			return reader.ReadUInt64(out value.m_PublishedFileId);
		}

		public static bool ReadSteamItemDefID(this NetPakReader reader, out SteamItemDef_t value)
		{
			return reader.ReadInt32(out value.m_SteamItemDef);
		}

		public static bool ReadSteamItemInstanceID(this NetPakReader reader, out SteamItemInstanceID_t value)
		{
			return reader.ReadUInt64(out value.m_SteamItemInstanceID);
		}
	}
}
