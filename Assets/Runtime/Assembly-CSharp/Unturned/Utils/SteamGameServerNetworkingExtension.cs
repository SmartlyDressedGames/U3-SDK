////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	[System.Obsolete("Should not be specific to SteamGameServerNetworking after NetTransport rewrite")]
	public static class SteamGameServerNetworkingUtils
	{
		/// <summary>
		/// Get real IPv4 address of remote player NOT the relay server.
		/// </summary>
		/// <returns>True if address was available, and not flagged as a relay server.</returns>
		[System.Obsolete("Should not be specific to SteamGameServerNetworking")]
		public static bool getIPv4Address(CSteamID steamIDRemote, out uint address)
		{
			NetTransport.ITransportConnection transportConnection = Provider.findTransportConnection(steamIDRemote);
			if (transportConnection != null)
			{
				return transportConnection.TryGetIPv4Address(out address);
			}
			else
			{
				address = 0;
				return false;
			}
		}

		/// <summary>
		/// See above, returns zero if failed.
		/// </summary>
		[System.Obsolete("Should not be specific to SteamGameServerNetworking")]
		public static uint getIPv4AddressOrZero(CSteamID steamIDRemote)
		{
			uint address;
			getIPv4Address(steamIDRemote, out address);
			return address;
		}
	}
}
