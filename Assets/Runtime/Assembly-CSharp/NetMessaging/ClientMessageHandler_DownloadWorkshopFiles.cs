////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_DownloadWorkshopFiles
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			Provider.isWaitingForWorkshopResponse = false;

			ENPCHoliday holiday;
			reader.ReadEnum(out holiday);
			UnturnedLog.info($"Server holiday: {holiday}");

			requiredFiles.Clear();
			reader.ReadList(requiredFiles, ReadRequiredWorkshopFile, MAX_FILES);

			string serverName;
			reader.ReadString(out serverName);
			UnturnedLog.info($"Server name: \"{serverName}\"");

			string levelName;
			reader.ReadString(out levelName);
			UnturnedLog.info($"Server level name: \"{levelName}\"");

			bool isPvP;
			reader.ReadBit(out isPvP);
			UnturnedLog.info($"Server is PvP: {isPvP}");

			bool allowAdminCheatCodes;
			reader.ReadBit(out allowAdminCheatCodes);
			UnturnedLog.info($"Server allows admin cheat codes: {allowAdminCheatCodes}");

			bool isVACSecure;
			reader.ReadBit(out isVACSecure);
			UnturnedLog.info($"Server is VAC secure: {isVACSecure}");

#if WITH_THIRDPARTYAC
			bool isThirdpartyAntiCheatActive;
			reader.ReadBit(out isThirdpartyAntiCheatActive);
			UnturnedLog.info($"Server has (official) third-party anti-cheat enabled: {isThirdpartyAntiCheatActive}");
#endif // WITH_THIRDPARTYAC

			bool isGold;
			reader.ReadBit(out isGold);
			UnturnedLog.info($"Server requires gold: {isGold}");

			EGameMode gameMode;
			reader.ReadEnum(out gameMode);
			UnturnedLog.info($"Server difficulty: {gameMode}");

			ECameraMode cameraMode;
			reader.ReadEnum(out cameraMode);
			UnturnedLog.info($"Server camera mode: {cameraMode}");

			byte maxPlayers;
			reader.ReadUInt8(out maxPlayers);
			UnturnedLog.info($"Server max players: {maxPlayers}");

			string bookmarkHost;
			reader.ReadString(out bookmarkHost);
			UnturnedLog.info($"Server bookmark host: \"{bookmarkHost}\"");

			string thumbnailUrl;
			reader.ReadString(out thumbnailUrl);
			UnturnedLog.info($"Server thumbnail URL: \"{thumbnailUrl}\"");

			string description;
			reader.ReadString(out description);
			UnturnedLog.info($"Server description: \"{description}\"");

			bool hasAllowedAddress = Provider.clientTransport.TryGetIPv4Address(out IPv4Address allowedAddress);
			uint allowedIP = hasAllowedAddress ? allowedAddress.value : 0;
			if (allowedIP == 0)
			{
				UnturnedLog.warn("Unable to determine server IP for download restrictions");
			}

			Provider.CachedWorkshopResponse response = null;
			foreach (Provider.CachedWorkshopResponse cache in Provider.cachedWorkshopResponses)
			{
				if (cache.server == Provider.server)
				{
					response = cache;
					break;
				}
			}

			if (response == null) // not already cached
			{
				response = new Provider.CachedWorkshopResponse();
				response.server = Provider.server;
				Provider.cachedWorkshopResponses.Add(response);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine($"{requiredFiles.Count} workshop file(s):");
			for (int index = 0; index < requiredFiles.Count; ++index)
			{
				sb.AppendLine($"{index}: {requiredFiles[index].fileId} Timestamp: {requiredFiles[index].timestamp.ToLocalTime()}");
			}
			UnturnedLog.info(sb.ToString());
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			response.holiday = holiday;
			response.serverName = serverName;
			response.levelName = levelName;
			response.isPvP = isPvP;
			response.allowAdminCheatCodes = allowAdminCheatCodes;
			response.isVACSecure = isVACSecure;
#if WITH_THIRDPARTYAC
			response.isThirdpartyAntiCheatEnabled = isThirdpartyAntiCheatActive;
#endif
			response.isGold = isGold;
			response.gameMode = gameMode;
			response.cameraMode = cameraMode;
			response.maxPlayers = maxPlayers;
			response.bookmarkHost = bookmarkHost;
			response.thumbnailUrl = thumbnailUrl;
			response.serverDescription = description;
			response.ip = allowedIP;
			response.requiredFiles = requiredFiles;
			response.realTime = Time.realtimeSinceStartup;
			Provider.receiveWorkshopResponse(response);
		}

		private static bool ReadRequiredWorkshopFile(NetPakReader reader, out Provider.ServerRequiredWorkshopFile requiredFile)
		{
			requiredFile = default;
			return reader.ReadUInt64(out requiredFile.fileId) && reader.ReadDateTime(out requiredFile.timestamp);
		}

		private static List<Provider.ServerRequiredWorkshopFile> requiredFiles = new List<Provider.ServerRequiredWorkshopFile>();
		private static readonly NetLength MAX_FILES = new NetLength(255);
	}
}
