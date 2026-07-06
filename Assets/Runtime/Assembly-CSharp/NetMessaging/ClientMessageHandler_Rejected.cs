////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_Rejected
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			Provider.isWaitingForConnectResponse = false;

			ESteamRejection rejection;
			string explanation;
			reader.ReadEnum(out rejection);
			reader.ReadString(out explanation);

			Provider._connectionFailureReason = string.Empty;

			if (rejection == ESteamRejection.WHITELISTED)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.WHITELISTED;
			}
			else if (rejection == ESteamRejection.WRONG_PASSWORD)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PASSWORD;
			}
			else if (rejection == ESteamRejection.SERVER_FULL)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.FULL;
			}
			else if (rejection == ESteamRejection.WRONG_HASH_LEVEL)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_LEVEL;
			}
			else if (rejection == ESteamRejection.WRONG_HASH_ASSEMBLY)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_ASSEMBLY;
			}
			else if (rejection == ESteamRejection.WRONG_VERSION)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.VERSION;
				Provider._connectionFailureReason = explanation;
			}
			else if (rejection == ESteamRejection.PRO_SERVER)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_SERVER;
			}
			else if (rejection == ESteamRejection.PRO_CHARACTER)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_CHARACTER;
			}
			else if (rejection == ESteamRejection.PRO_DESYNC)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_DESYNC;
			}
			else if (rejection == ESteamRejection.PRO_APPEARANCE)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_APPEARANCE;
			}
			else if (rejection == ESteamRejection.AUTH_VERIFICATION)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_VERIFICATION;
			}
			else if (rejection == ESteamRejection.AUTH_NO_STEAM)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_NO_STEAM;
			}
			else if (rejection == ESteamRejection.AUTH_LICENSE_EXPIRED)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_LICENSE_EXPIRED;
			}
			else if (rejection == ESteamRejection.AUTH_VAC_BAN)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_VAC_BAN;
			}
			else if (rejection == ESteamRejection.AUTH_ELSEWHERE)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ELSEWHERE;
			}
			else if (rejection == ESteamRejection.AUTH_TIMED_OUT)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_TIMED_OUT;
			}
			else if (rejection == ESteamRejection.AUTH_USED)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_USED;
			}
			else if (rejection == ESteamRejection.AUTH_NO_USER)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_NO_USER;
			}
			else if (rejection == ESteamRejection.AUTH_PUB_BAN)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_PUB_BAN;
			}
			else if (rejection == ESteamRejection.AUTH_NETWORK_IDENTITY_FAILURE)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_NETWORK_IDENTITY_FAILURE;
			}
			else if (rejection == ESteamRejection.AUTH_ECON_DESERIALIZE)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ECON_DESERIALIZE;
			}
			else if (rejection == ESteamRejection.AUTH_ECON_VERIFY)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ECON_VERIFY;
			}
			else if (rejection == ESteamRejection.ALREADY_CONNECTED)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.ALREADY_CONNECTED;
			}
			else if (rejection == ESteamRejection.ALREADY_PENDING)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.ALREADY_PENDING;
			}
			else if (rejection == ESteamRejection.LATE_PENDING)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING;
			}
			else if (rejection == ESteamRejection.NOT_PENDING)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NOT_PENDING;
			}
			else if (rejection == ESteamRejection.NAME_PLAYER_SHORT)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_SHORT;
			}
			else if (rejection == ESteamRejection.NAME_PLAYER_LONG)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_LONG;
			}
			else if (rejection == ESteamRejection.NAME_PLAYER_INVALID)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_INVALID;
			}
			else if (rejection == ESteamRejection.NAME_PLAYER_NUMBER)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_NUMBER;
			}
			else if (rejection == ESteamRejection.NAME_CHARACTER_SHORT)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_SHORT;
			}
			else if (rejection == ESteamRejection.NAME_CHARACTER_LONG)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_LONG;
			}
			else if (rejection == ESteamRejection.NAME_CHARACTER_INVALID)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_INVALID;
			}
			else if (rejection == ESteamRejection.NAME_CHARACTER_NUMBER)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_NUMBER;
			}
			else if (rejection == ESteamRejection.PING)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PING;

				// Hack: formatted here rather than by UI because at that point the calculated ping is no longer known.
				if (MenuDashboardUI.localization.has("PingV2"))
				{
					// Explanation from the server here is the remote maximum ping.
					int calculatedPing = Provider.CurrentServerAdvertisement != null ? Provider.CurrentServerAdvertisement.PingMs : -1;
					Provider._connectionFailureReason = MenuDashboardUI.localization.format("PingV2", calculatedPing, explanation);
				}
				else
				{
					Provider._connectionFailureReason = MenuDashboardUI.localization.format("Ping");
				}
			}
			else if (rejection == ESteamRejection.PLUGIN)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PLUGIN;
				Provider._connectionFailureReason = explanation;
			}
			else if (rejection == ESteamRejection.CLIENT_MODULE_DESYNC)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CLIENT_MODULE_DESYNC;
			}
			else if (rejection == ESteamRejection.SERVER_MODULE_DESYNC)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.SERVER_MODULE_DESYNC;
			}
			else if (rejection == ESteamRejection.WRONG_LEVEL_VERSION)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LEVEL_VERSION;

				// Hack: formatted here rather than by UI because at that point the local level version is no
				// longer known. Explanation from the server here is the remote level version.
				Provider._connectionFailureReason = MenuDashboardUI.localization.format("Level_Version", explanation, Level.info.getLocalizedName(), Level.version);
			}
			else if (rejection == ESteamRejection.WRONG_HASH_ECON)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.ECON_HASH;
			}
			else if (rejection == ESteamRejection.WRONG_HASH_MASTER_BUNDLE)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_MASTER_BUNDLE;
				Provider._connectionFailureReason = explanation;
			}
			else if (rejection == ESteamRejection.LATE_PENDING_STEAM_AUTH)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING_STEAM_AUTH;
			}
			else if (rejection == ESteamRejection.LATE_PENDING_STEAM_ECON)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING_STEAM_ECON;
			}
			else if (rejection == ESteamRejection.LATE_PENDING_STEAM_GROUPS)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING_STEAM_GROUPS;
			}
			else if (rejection == ESteamRejection.NAME_PRIVATE_LONG)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PRIVATE_LONG;
			}
			else if (rejection == ESteamRejection.NAME_PRIVATE_INVALID)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PRIVATE_INVALID;
			}
			else if (rejection == ESteamRejection.NAME_PRIVATE_NUMBER)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PRIVATE_NUMBER;
			}
			else if (rejection == ESteamRejection.WRONG_HASH_RESOURCES)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_RESOURCES;
			}
			else if (rejection == ESteamRejection.SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR;
			}
			else if (rejection == ESteamRejection.STEAM_ID_MISMATCH)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.STEAM_ID_MISMATCH;
			}
			else if (rejection == ESteamRejection.CONNECT_RATE_LIMITING)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CONNECT_RATE_LIMITING;
			}
			else if (rejection == ESteamRejection.BAD_PACKET_RATE_LIMITING)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.BAD_PACKET_RATE_LIMITING;
			}
			else if (rejection == ESteamRejection.TOO_MANY_CLIENTS_WITH_SAME_IP_ADDRESS)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.TOO_MANY_CLIENTS_WITH_SAME_IP_ADDRESS;
			}
			else if (rejection == ESteamRejection.MOD_NAME_MISMATCH)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.MOD_NAME_MISMATCH;
				Provider._connectionFailureReason = explanation;
			}
			else if (rejection == ESteamRejection.MOD_VERSION_MISMATCH)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.MOD_VERSION_MISMATCH;
				Provider._connectionFailureReason = explanation;
			}
			else
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.REJECT_UNKNOWN;
				Provider._connectionFailureReason = rejection.ToString();
			}

			Provider.RequestDisconnect($"Rejected by server ({rejection}) --- Reason: \"{Provider.connectionFailureReason}\" Explanation: \"{explanation}\"");
		}
	}
}
