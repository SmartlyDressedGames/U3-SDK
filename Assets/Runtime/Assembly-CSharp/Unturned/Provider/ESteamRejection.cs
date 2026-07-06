////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	[NetEnum]
	public enum ESteamRejection
	{
		SERVER_FULL,
		WRONG_HASH_LEVEL,
		WRONG_HASH_ASSEMBLY,
		WRONG_VERSION,
		WRONG_PASSWORD,
		NAME_PLAYER_SHORT,
		NAME_PLAYER_LONG,

		/// <summary>
		/// If modifying usage please update support article:
		/// https://support.smartlydressedgames.com/hc/en-us/articles/13452208765716
		/// </summary>
		NAME_PLAYER_INVALID,

		NAME_PLAYER_NUMBER,
		NAME_CHARACTER_SHORT,
		NAME_CHARACTER_LONG,

		/// <summary>
		/// If modifying usage please update support article:
		/// https://support.smartlydressedgames.com/hc/en-us/articles/13452208765716
		/// </summary>
		NAME_CHARACTER_INVALID,

		NAME_CHARACTER_NUMBER,
		PRO_SERVER,
		PRO_CHARACTER,
		PRO_DESYNC,
		PRO_APPEARANCE,
		ALREADY_PENDING,
		ALREADY_CONNECTED,
		NOT_PENDING,
		LATE_PENDING,
		WHITELISTED,
		AUTH_VERIFICATION,
		AUTH_NO_STEAM,
		AUTH_LICENSE_EXPIRED,
		AUTH_VAC_BAN,
		AUTH_ELSEWHERE,
		AUTH_TIMED_OUT,
		AUTH_USED,
		AUTH_NO_USER,
		AUTH_PUB_BAN,
		AUTH_ECON_DESERIALIZE,
		AUTH_ECON_VERIFY,
		PING,
		PLUGIN,
		/// <summary>
		/// Client has a critical module the server doesn't.
		/// </summary>
		CLIENT_MODULE_DESYNC,
		/// <summary>
		/// Server has a critical module the client doesn't.
		/// </summary>
		SERVER_MODULE_DESYNC,
		/// <summary>
		/// Level config's version number does not match.
		/// </summary>
		WRONG_LEVEL_VERSION,
		/// <summary>
		/// EconInfo.json hash does not match.
		/// </summary>
		WRONG_HASH_ECON,
		/// <summary>
		/// Master bundle hashes do not match.
		/// </summary>
		WRONG_HASH_MASTER_BUNDLE,
		/// <summary>
		/// Server has not received an auth session response from Steam yet.
		/// </summary>
		LATE_PENDING_STEAM_AUTH,
		/// <summary>
		/// Server has not received an economy response from Steam yet.
		/// </summary>
		LATE_PENDING_STEAM_ECON,
		/// <summary>
		/// Server has not received a groups response from Steam yet.
		/// </summary>
		LATE_PENDING_STEAM_GROUPS,
		/// <summary>
		/// Player nickname exceeds limit.
		/// </summary>
		NAME_PRIVATE_LONG,

		/// <summary>
		/// Player nickname contains invalid characters.
		/// 
		/// If modifying usage please update support article:
		/// https://support.smartlydressedgames.com/hc/en-us/articles/13452208765716
		/// </summary>
		NAME_PRIVATE_INVALID,

		/// <summary>
		/// Player nickname should not be a number.
		/// </summary>
		NAME_PRIVATE_NUMBER,

		/// <summary>
		/// Player resources folders don't match.
		/// </summary>
		WRONG_HASH_RESOURCES,

		/// <summary>
		/// The network identity in the ticket does not match the server authenticating the ticket.
		/// This can happen if server's Steam ID has changed from what the client thinks it is.
		/// For example, joining a stale entry in the server list. (public issue #4101)
		/// </summary>
		AUTH_NETWORK_IDENTITY_FAILURE,

		/// <summary>
		/// Player's skin color is too similar to one of <see cref="LevelAsset.terrainColorRules"/>.
		/// </summary>
		SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR,

		/// <summary>
		/// Steam ID reported by net transport doesn't match client's reported Steam ID.
		/// This was exploited to fill the server queue with fake players.
		/// </summary>
		STEAM_ID_MISMATCH,

		/// <summary>
		/// Received too many connection requests from player in a short window.
		/// </summary>
		CONNECT_RATE_LIMITING,

		/// <summary>
		/// Received too many invalid messages from connection in a short window.
		/// </summary>
		BAD_PACKET_RATE_LIMITING,

		/// <summary>
		/// Server limits how many clients are joining from the same IP address.
		/// (public issue #5001)
		/// </summary>
		TOO_MANY_CLIENTS_WITH_SAME_IP_ADDRESS,

		/// <summary>
		/// Provider.GetModInfo().Name did not match between client and server.
		/// </summary>
		MOD_NAME_MISMATCH,

		/// <summary>
		/// Provider.GetModInfo().Version did not match between client and server.
		/// </summary>
		MOD_VERSION_MISMATCH,
	}
}
