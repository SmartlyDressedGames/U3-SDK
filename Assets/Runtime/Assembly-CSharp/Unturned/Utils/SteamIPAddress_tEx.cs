////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public static class SteamIPAddress_tEx
	{
		/// <summary>
		/// Steam APIs returned uint32 IPv4 addresses in the past, so Unturned code depends on them in some places.
		/// Ideally these uses should be updated for IPv6 support going forward.
		/// For the meantime this method converts from the new format to the old format for backwards compatibility.
		/// </summary>
		public static bool TryGetIPv4Address(this SteamIPAddress_t steamIPAddress, out uint address)
		{
			if (steamIPAddress.GetIPType() == ESteamIPType.k_ESteamIPTypeIPv4)
			{
				byte[] bytes = steamIPAddress.ToIPAddress().GetAddressBytes();
				address = (uint) ((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
				return true;
			}
			else
			{
				address = 0;
				return false;
			}
		}
	}
}
