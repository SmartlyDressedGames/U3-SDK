////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.HostBans
{
	[System.Flags]
	public enum EHostBanFlags : uint
	{
		None = 0,

		/// <summary>
		/// Hide from all in-game server lists.
		/// </summary>
		HiddenFromAllServerLists = 1 << 0,

		/// <summary>
		/// Show warning that server is breaking monetization rules.
		/// </summary>
		MonetizationWarning = 1 << 1,

		/// <summary>
		/// Client cannot join. Also hide from all in-game server lists.
		/// </summary>
		Blocked = 1 << 2,

		/// <summary>
		/// Show warning that server was using banned workshop files, typically from DMCA takedown.
		/// </summary>
		WorkshopWarning = 1 << 3,

		/// <summary>
		/// Show message apologizing for mistakenly flagging the server.
		/// </summary>
		Apology = 1 << 4,

		/// <summary>
		/// Hide from the in-game internet server list, but not LAN/history/friends/favorites lists.
		/// </summary>
		HiddenFromInternetServerList = 1 << 5,

		/// <summary>
		/// Host is using a proxy to handle Steam queries (details, rules, players) but there
		/// is a big discrepancy between the proxy ping and in-game ping.
		/// </summary>
		QueryPingWarning = 1 << 6,

		/// <summary>
		/// Show warning that server has None or Non-Gameplay monetization tag when it is selling those benefits.
		/// </summary>
		IncorrectMonetizationTagWarning = 1 << 7,

		/// <summary>
		/// Show message that this ban applies to the hosting provider, not necessarily an individual server.
		/// 
		/// Added because there was a server that got banned 6+ times on the same IP address and kept
		/// changing ports, so we banned the address until the hosting provider stops letting them
		/// evade the ban.
		/// </summary>
		HostingProvider = 1 << 8,

		/// <summary>
		/// The server console outputs a warning if any of these flags are set.
		/// </summary>
		RecommendHostCheckWarningsList = MonetizationWarning | WorkshopWarning | IncorrectMonetizationTagWarning,
	}
}
