////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define WITH_BYPASS
#endif

using Steamworks;

namespace SDG.Unturned
{
	public enum EWorkshopDownloadRestrictionResult
	{
		/// <summary>
		/// Workshop item does not have any IP restrictions in place.
		/// </summary>
		NoRestrictions,

		/// <summary>
		/// Workshop item has an IP whitelist, and server IP is not on it.
		/// </summary>
		NotWhitelisted,

		/// <summary>
		/// Workshop item has an IP blacklist, and server IP is on it.
		/// </summary>
		Blacklisted,

		/// <summary>
		/// Workshop item does have IP restrictions, and server IP is allowed.
		/// </summary>
		Allowed,

		/// <summary>
		/// Workshop item has been banned by an admin.
		/// </summary>
		Banned,

		/// <summary>
		/// Workshop item is hidden from everyone.
		/// </summary>
		PrivateVisibility,
	}

	/// <summary>
	/// Utilities for testing whether a particular server is allowed to download a workshop item.
	/// Available from client and server side so that clients can help enforce restrictions.
	/// </summary>
	public class WorkshopDownloadRestrictions
	{
		/// <summary>
		/// Workshop item key-value tag storing IP whitelist and blacklist.
		/// </summary>
		public static readonly string IP_RESTRICTIONS_KVTAG = "allowed_ips";

		/// <summary>
		/// Get ip restrictions value if set, otherwise null.
		/// Can be called from client or server.
		/// </summary>
		public static string getAllowedIpsTagValue(UGCQueryHandle_t queryHandle, uint resultIndex)
		{
			string value;
			WorkshopUtils.findQueryUGCKeyValue(queryHandle, resultIndex, IP_RESTRICTIONS_KVTAG, out value);
			return value;
		}

		public static EWorkshopDownloadRestrictionResult getRestrictionResult(UGCQueryHandle_t queryHandle, uint resultIndex, uint ip)
		{
#if WITH_BYPASS
			if (shouldBypass)
			{
				return EWorkshopDownloadRestrictionResult.NoRestrictions;
			}
#endif // WITH_BYPASS

			SteamUGCDetails_t details;
			if (WorkshopUtils.getQueryUGCResult(queryHandle, resultIndex, out details))
			{
				if (details.m_bBanned)
				{
					return EWorkshopDownloadRestrictionResult.Banned;
				}

				if (details.m_eVisibility == ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate ||
					details.m_eResult == EResult.k_EResultAccessDenied) // DMCA takedown items can be unbanned and public.
				{
					return EWorkshopDownloadRestrictionResult.PrivateVisibility;
				}
			}

			string filter = getAllowedIpsTagValue(queryHandle, resultIndex);
			if (string.IsNullOrEmpty(filter))
			{
				return EWorkshopDownloadRestrictionResult.NoRestrictions;
			}
			else
			{
				return getRestrictionResult(filter, ip);
			}
		}

		/// <summary>
		/// Test whether IP is whitelisted or blacklisted in filter.
		/// </summary>
		public static EWorkshopDownloadRestrictionResult getRestrictionResult(string filter, uint ip)
		{
#if WITH_BYPASS
			if (shouldBypass)
			{
				return EWorkshopDownloadRestrictionResult.NoRestrictions;
			}
#endif // WITH_BYPASS

			uint[] whitelist;
			uint[] blacklist;
			parseAllowedIPs(filter, out whitelist, out blacklist);

			if (whitelist != null && whitelist.Length > 0)
			{
				bool onWhitelist = isAddressInList(ip, whitelist);
				if (onWhitelist)
				{
					return EWorkshopDownloadRestrictionResult.Allowed;
				}
				else
				{
					return EWorkshopDownloadRestrictionResult.NotWhitelisted;
				}
			}

			if (blacklist != null && blacklist.Length > 0)
			{
				bool onBlacklist = isAddressInList(ip, blacklist);
				if (onBlacklist)
				{
					return EWorkshopDownloadRestrictionResult.Blacklisted;
				}
				else
				{
					return EWorkshopDownloadRestrictionResult.Allowed;
				}
			}

			return EWorkshopDownloadRestrictionResult.NoRestrictions;
		}

		private static readonly char[] IP_SEPARATOR = { ',' };

		/// <summary>
		/// Split x,y-z format into whitelist [x, y] and blacklist [z].
		/// </summary>
		public static void splitAllowedIPs(string allowedIPs, out string[] whitelistIps, out string[] blacklistIps)
		{
			whitelistIps = null;
			blacklistIps = null;

			int whitelistBlacklistSplit = allowedIPs.IndexOf('-');
			if (whitelistBlacklistSplit >= 0 && whitelistBlacklistSplit < allowedIPs.Length - 1)
			{
				string whitelistStr = allowedIPs.Substring(0, whitelistBlacklistSplit);
				string blacklistStr = allowedIPs.Substring(whitelistBlacklistSplit + 1);

				whitelistIps = whitelistStr.Split(IP_SEPARATOR, System.StringSplitOptions.RemoveEmptyEntries);
				blacklistIps = blacklistStr.Split(IP_SEPARATOR, System.StringSplitOptions.RemoveEmptyEntries);
			}
			else
			{
				whitelistIps = allowedIPs.Split(',');
			}
		}

		/// <summary>
		/// Split whitelist-blacklist format and parse string IPs into integer IPs.
		/// </summary>
		public static void parseAllowedIPs(string allowedIPs, out uint[] whitelist, out uint[] blacklist)
		{
			string[] whitelistStrings;
			string[] blacklistStrings;
			splitAllowedIPs(allowedIPs, out whitelistStrings, out blacklistStrings);

			if (whitelistStrings == null || whitelistStrings.Length < 1)
			{
				whitelist = null;
			}
			else
			{
				parseStringIps(whitelistStrings, out whitelist);
			}

			if (blacklistStrings == null || blacklistStrings.Length < 1)
			{
				blacklist = null;
			}
			else
			{
				parseStringIps(blacklistStrings, out blacklist);
			}
		}

		/// <summary>
		/// Parse CIDR string IPs into integer IPs.
		/// </summary>
		public static void parseStringIps(string[] strings, out uint[] integers)
		{
			int length = strings.Length;
			integers = new uint[length];
			for (int index = 0; index < length; ++index)
			{
				integers[index] = Parser.getUInt32FromIP(strings[index]);
			}
		}

		public static bool isAddressInList(uint ip, uint[] list)
		{
			foreach (uint listedIP in list)
			{
				if (ip == listedIP)
				{
					return true;
				}
			}

			return false;
		}

#if WITH_BYPASS
		private static CommandLineFlag shouldBypass = new CommandLineFlag(false, "-BypassWorkshopDownloadRestrictions");
#endif // WITH_BYPASS
	}
}
