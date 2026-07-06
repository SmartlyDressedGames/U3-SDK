////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_CONNECT_ARGS
#define LOG_CONNECT_MOD_INFO
#define LOG_SKIN_TERRAIN_COLOR_COMPARISON
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_ReadyToConnect
	{
		[System.Diagnostics.Conditional("LOG_CONNECT_ARGS")]
		private static void LogRead(string key, object value)
		{
			UnturnedLog.info("{0} = {1}", key, value);
		}

		[System.Diagnostics.Conditional("LOG_CONNECT_MOD_INFO")]
		private static void LogModInfo(object message)
		{
			UnturnedLog.info(message);
		}

		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			// 2022-02-23: prior to net transport rewrite these existingPending/existingPlayer tests also served to
			// prevent the same Steam account from joining on multiple connections, but now we check that later too.
			SteamPending existingPending = Provider.findPendingPlayer(transportConnection);
			if (existingPending != null)
			{
				Provider.reject(transportConnection, ESteamRejection.ALREADY_PENDING);
				return;
			}

			SteamPlayer existingPlayer = Provider.findPlayer(transportConnection);
			if (existingPlayer != null)
			{
				Provider.reject(transportConnection, ESteamRejection.ALREADY_CONNECTED);
				return;
			}

			byte characterID;
			reader.ReadUInt8(out characterID);
			LogRead("characterID", characterID);

			string playerName;
			reader.ReadString(out playerName);
			LogRead("playerName", playerName);
			playerName = playerName.Trim();

			string characterName;
			reader.ReadString(out characterName);
			LogRead("characterName", characterName);
			characterName = characterName.Trim();

			byte[] hashPassword = new byte[20];
			reader.ReadBytes(hashPassword, 20);

			byte[] hashLevel = new byte[20];
			reader.ReadBytes(hashLevel, 20);

			byte[] app = new byte[20];
			reader.ReadBytes(app, 20);

			byte[] resourceHash = new byte[20];
			reader.ReadBytes(resourceHash, 20);

			EClientPlatform clientPlatform;
			reader.ReadEnum(out clientPlatform);
			LogRead("clientPlatform", clientPlatform);

			uint packedVersion;
			reader.ReadUInt32(out packedVersion);
			LogRead("packedVersion", packedVersion);

			string modName;
			reader.ReadString(out modName, 8);
			LogRead("modName", modName);

			uint packedModVersion;
			reader.ReadUInt32(out packedModVersion);
			LogRead("packedModVersion", packedModVersion);

			if (Provider._modInfo != null)
			{
				if (!string.Equals(modName, Provider._modInfo.Name, System.StringComparison.Ordinal))
				{
					LogModInfo($"Server using \"{Provider._modInfo.Name}\" mod, client attempted connecting with \"{modName}\" mod");
					Provider.reject(transportConnection, ESteamRejection.MOD_NAME_MISMATCH, Provider._modInfo.Name);
					return;
				}

				if (Provider._modInfo.GetPackedVersion() != packedModVersion)
				{
					LogModInfo($"Server using \"{Provider._modInfo.Name}\" mod v{Provider._modInfo.GetPackedVersion()}, client attempted connecting with v{packedModVersion}");
					Provider.reject(transportConnection, ESteamRejection.MOD_VERSION_MISMATCH, Provider._modInfo.FormatModVersion());
					return;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(modName))
				{
					LogModInfo($"Server is not modded, client attempted connecting with \"{modName}\" mod");
					Provider.reject(transportConnection, ESteamRejection.MOD_NAME_MISMATCH, string.Empty);
					return;
				}
			}

			bool pro;
			reader.ReadBit(out pro);
			LogRead("pro", pro);

			ushort reportedPing;
			reader.ReadUInt16(out reportedPing);
			LogRead("reportedPing", reportedPing);

			string nickname;
			reader.ReadString(out nickname);
			LogRead("nickname", nickname);
			nickname = nickname.Trim();

			CSteamID groupID;
			reader.ReadSteamID(out groupID);
			LogRead("groupID", groupID);

			byte face;
			reader.ReadUInt8(out face);
			LogRead("face", face);

			byte hair;
			reader.ReadUInt8(out hair);
			LogRead("hair", hair);

			byte beard;
			reader.ReadUInt8(out beard);
			LogRead("beard", beard);

			Color32 skinColor;
			reader.ReadColor32RGB(out skinColor);
			LogRead("skinColor", skinColor);

			Color32 hairColor;
			reader.ReadColor32RGB(out hairColor);
			LogRead("hairColor", hairColor);

			Color32 markerColor;
			reader.ReadColor32RGB(out markerColor);
			LogRead("markerColor", markerColor);

			Color beardColor;
			reader.ReadColor32RGB(out beardColor);
			LogRead("beardColor", beardColor);

			bool leftHanded;
			reader.ReadBit(out leftHanded);
			LogRead("leftHanded", leftHanded);

			ulong packageShirt;
			reader.ReadUInt64(out packageShirt);
			LogRead("packageShirt", packageShirt);

			ulong packagePants;
			reader.ReadUInt64(out packagePants);
			LogRead("packagePants", packagePants);

			ulong packageHat;
			reader.ReadUInt64(out packageHat);
			LogRead("packageHat", packageHat);

			ulong packageBackpack;
			reader.ReadUInt64(out packageBackpack);
			LogRead("packageBackpack", packageBackpack);

			ulong packageVest;
			reader.ReadUInt64(out packageVest);
			LogRead("packageVest", packageVest);

			ulong packageMask;
			reader.ReadUInt64(out packageMask);
			LogRead("packageMask", packageMask);

			ulong packageGlasses;
			reader.ReadUInt64(out packageGlasses);
			LogRead("packageGlasses", packageGlasses);

			pendingPackageSkins.Clear();
			reader.ReadList(pendingPackageSkins, reader.ReadUInt64, Provider.MAX_SKINS_LENGTH);

			EPlayerSkillset playerSkillset;
			reader.ReadEnum(out playerSkillset);
			LogRead("playerSkillset", playerSkillset);

			string modPacket;
			reader.ReadString(out modPacket);
			LogRead("modPacket", modPacket);

			string language;
			reader.ReadString(out language);
			LogRead("language", language);

			CSteamID lobbyID;
			reader.ReadSteamID(out lobbyID);
			LogRead("lobbyID", lobbyID);

			uint clientLevelVersion;
			reader.ReadUInt32(out clientLevelVersion);
			LogRead("clientLevelVersion", clientLevelVersion);

			byte hwidCount;
			reader.ReadUInt8(out hwidCount);
			if (hwidCount > LocalHwid.MAX_HWIDS)
			{
				// We do not have a specific rejection for this, but it should never occur legitimately anyway. 
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
				return;
			}

			byte[][] hwids = new byte[hwidCount][];
			for (byte hwidIndex = 0; hwidIndex < hwidCount; ++hwidIndex)
			{
				hwids[hwidIndex] = new byte[20];
				reader.ReadBytes(hwids[hwidIndex]);
			}

			byte[] econHash = new byte[20];
			reader.ReadBytes(econHash, 20);

			CSteamID steamID;
			reader.ReadSteamID(out steamID);
			LogRead("steamID", steamID);

			if (transportConnection.TryGetSteamId(out ulong transportSteamId))
			{
				if (steamID.m_SteamID != transportSteamId)
				{
					Provider.reject(transportConnection, ESteamRejection.STEAM_ID_MISMATCH);
					return;
				}
			}

			if (joinRateLimiter.IsBlockedBySteamIdRateLimiting(steamID))
			{
				Provider.reject(transportConnection, ESteamRejection.CONNECT_RATE_LIMITING);
				return;
			}

			// These two tests are a preliminary check preventing the same Steam account from joining on multiple
			// connections simultaneously.
			existingPending = Provider.findPendingPlayerBySteamId(steamID);
			if (existingPending != null)
			{
				Provider.reject(transportConnection, ESteamRejection.ALREADY_PENDING);
				return;
			}

			existingPlayer = PlayerTool.getSteamPlayer(steamID);
			if (existingPlayer != null)
			{
				Provider.reject(transportConnection, ESteamRejection.ALREADY_CONNECTED);
				return;
			}

			if (Provider.modeConfigData.Players.Allow_Per_Character_Saves == false)
			{
				// Only affects using save slot zero.
				characterID = 0;
			}

			SteamPlayerID playerID = new SteamPlayerID(steamID, characterID, playerName, characterName, nickname, groupID, hwids);

			if (!Provider.canClientVersionJoinServer(packedVersion))
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_VERSION, Provider.APP_VERSION);
				return;
			}

			if (clientLevelVersion != Level.packedVersion)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_LEVEL_VERSION, Level.version);
				return;
			}

			if (string.IsNullOrWhiteSpace(playerID.playerName) || NameTool.containsRichText(playerID.playerName) || playerID.playerName.ContainsNewLine())
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
				return;
			}

			if (string.IsNullOrWhiteSpace(playerID.characterName) || NameTool.containsRichText(playerID.characterName) || playerID.characterName.ContainsNewLine())
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
				return;
			}

			if (string.IsNullOrWhiteSpace(playerID.nickName) || NameTool.containsRichText(playerID.nickName) || playerID.nickName.ContainsNewLine())
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_INVALID);
				return;
			}

			if (playerID.playerName.Length < 2)
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_SHORT);
				return;
			}

			if (playerID.characterName.Length < 2)
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_SHORT);
				return;
			}

			if (playerID.playerName.Length > 32)
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_LONG);
				return;
			}

			if (playerID.characterName.Length > 32)
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_LONG);
				return;
			}

			if (playerID.nickName.Length > 32)
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_LONG);
				return;
			}

			long integerPlayerName;
			double floatPlayerName;
			if (long.TryParse(playerID.playerName, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out integerPlayerName) || double.TryParse(playerID.playerName, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out floatPlayerName))
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_NUMBER);
				return;
			}

			long integerCharacterName;
			double floatCharacterName;
			if (long.TryParse(playerID.characterName, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out integerCharacterName) || double.TryParse(playerID.characterName, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out floatCharacterName))
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_NUMBER);
				return;
			}

			long integerPrivateName;
			double floatPrivateName;
			if (long.TryParse(playerID.nickName, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out integerPrivateName) || double.TryParse(playerID.nickName, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out floatPrivateName))
			{
				Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_NUMBER);
				return;
			}

			if (Provider.filterName)
			{
				if (!NameTool.isValid(playerID.playerName))
				{
					Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
					return;
				}

				if (!NameTool.isValid(playerID.characterName))
				{
					Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
					return;
				}

				if (!NameTool.isValid(playerID.nickName))
				{
					Provider.reject(transportConnection, ESteamRejection.NAME_PRIVATE_INVALID);
					return;
				}
			}

			bool isAllowedToBeNamedNelson = (playerID.steamID.m_SteamID == 76561198036822957) // Personal Steam account
				|| (playerID.steamID.m_SteamID == 76561198267201306); // SDGNelson Steam Account
			if (!isAllowedToBeNamedNelson)
			{
				if (IsNameBlockedByNelsonFilter(playerName))
				{
					Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
					return;
				}
				if (IsNameBlockedByNelsonFilter(characterName))
				{
					Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
					return;
				}
			}

			uint remoteIP;
			bool gotRemoteAddr = transportConnection.TryGetIPv4Address(out remoteIP);
			bool isBanned;
			string banReason;
			uint banRemainingDuration;
			Provider.checkBanStatus(playerID, remoteIP, out isBanned, out banReason, out banRemainingDuration);
			if (isBanned)
			{
				Provider.notifyBannedInternal(transportConnection, banReason, banRemainingDuration);
				return;
			}

			if (gotRemoteAddr && !Provider.configData.Server.Use_FakeIP)
			{
				if (joinRateLimiter.IsBlockedByAddressRateLimiting(remoteIP))
				{
					Provider.reject(transportConnection, ESteamRejection.CONNECT_RATE_LIMITING);
					return;
				}
			}

			bool hasPermit = SteamWhitelist.checkWhitelisted(steamID);
			if (Provider.isWhitelisted && !hasPermit)
			{
				Provider.reject(transportConnection, ESteamRejection.WHITELISTED);
				return;
			}

			if (Provider.clients.Count + 1 > Provider.maxPlayers && Provider.pending.Count + 1 > Provider.queueSize)
			{
				Provider.reject(transportConnection, ESteamRejection.SERVER_FULL);
				return;
			}

			if (hashPassword.Length != 20)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_PASSWORD);
				return;
			}

			if (hashLevel.Length != 20)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_LEVEL);
				return;
			}

			if (app.Length != 20)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
				return;
			}

			if (resourceHash.Length != 20)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_RESOURCES);
				return;
			}

			if (Provider.configData.Server.Validate_EconInfo_Hash && !Hash.verifyHash(econHash, SDG.Provider.TempSteamworksEconomy.econInfoHash) && !playerID.BypassIntegrityChecks)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ECON);
				return;
			}

			SDG.Framework.Modules.ModuleDependency[] mods;
			if (string.IsNullOrEmpty(modPacket))
			{
				mods = new Framework.Modules.ModuleDependency[0];
			}
			else
			{
				string[] modInfo = modPacket.Split(';');
				mods = new Framework.Modules.ModuleDependency[modInfo.Length];
				for (int i = 0; i < mods.Length; i++)
				{
					string[] modDetails = modInfo[i].Split(',');

					if (modDetails.Length != 2)
					{
						continue;
					}

					mods[i] = new Framework.Modules.ModuleDependency();
					mods[i].Name = modDetails[0];
					uint.TryParse(modDetails[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mods[i].Version_Internal);
				}
			}

			List<SDG.Framework.Modules.Module> critMods = Provider.critMods;
			Provider.critMods.Clear();
			SDG.Framework.Modules.ModuleHook.getRequiredModules(critMods);

			bool serverHasClientMods = true;
			for (int i = 0; i < mods.Length; i++)
			{
				bool found = false;
				if (mods[i] != null)
				{
					for (int k = 0; k < critMods.Count; k++)
					{
						if (critMods[k] == null || critMods[k].config == null)
						{
							continue;
						}

						if (critMods[k].config.Name == mods[i].Name && critMods[k].config.Version_Internal >= mods[i].Version_Internal)
						{
							found = true;
							break;
						}
					}
				}

				if (!found)
				{
					serverHasClientMods = false;
					break;
				}
			}

			if (!serverHasClientMods)
			{
				Provider.reject(transportConnection, ESteamRejection.CLIENT_MODULE_DESYNC);
				return;
			}

			bool clientHasServerMods = true;
			for (int i = 0; i < critMods.Count; i++)
			{
				bool found = false;
				if (critMods[i] != null && critMods[i].config != null)
				{
					for (int k = 0; k < mods.Length; k++)
					{
						if (mods[k] == null)
						{
							continue;
						}

						if (mods[k].Name == critMods[i].config.Name && mods[k].Version_Internal >= critMods[i].config.Version_Internal)
						{
							found = true;
							break;
						}
					}
				}

				if (!found)
				{
					clientHasServerMods = false;
					break;
				}
			}

			if (!clientHasServerMods)
			{
				Provider.reject(transportConnection, ESteamRejection.SERVER_MODULE_DESYNC);
				return;
			}

			if (!string.IsNullOrEmpty(Provider.serverPassword) && !Hash.verifyHash(hashPassword, Provider._serverPasswordHash))
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_PASSWORD);
				return;
			}

			if (!Hash.verifyHash(hashLevel, Level.hash) && !playerID.BypassIntegrityChecks)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_LEVEL);
				return;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEDICATED_SERVER
			if (!PlayerHashValidation.IsAssemblyHashValid(app, clientPlatform) && !playerID.BypassIntegrityChecks)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
				return;
			}

			if (!PlayerHashValidation.IsResourcesHashValid(resourceHash, clientPlatform) && !playerID.BypassIntegrityChecks)
			{
				Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_RESOURCES);
				return;
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEDICATED_SERVER

			if (reportedPing > Provider.configData.Server.Max_Ping_Milliseconds)
			{
				Provider.reject(transportConnection, ESteamRejection.PING, Provider.configData.Server.Max_Ping_Milliseconds.ToString());
				return;
			}

			if (Provider.modeConfigData.Players.Enable_Terrain_Color_Kick)
			{
				if (IsSkinColorWithinThresholdOfTerrainColor(skinColor))
				{
					Provider.reject(transportConnection, ESteamRejection.SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR);
					return;
				}
			}

			// Nelson 2025-05-13: check IP address rule relatively late in this process in case there's much overhead
			// to requesting all connected clients' IP addresses from server transport.
			if (Provider.IsBlockedByMaxClientsWithSameIpAddressRule(transportConnection, includeQueuedPlayers: true))
			{
				Provider.reject(transportConnection, ESteamRejection.TOO_MANY_CLIENTS_WITH_SAME_IP_ADDRESS);
				return;
			}

			byte queuePosition;
			bool shouldVerify;

			SteamPending newPendingPlayer = new SteamPending(transportConnection, playerID, pro, face, hair, beard, skinColor, hairColor, markerColor, beardColor, leftHanded, packageShirt, packagePants, packageHat, packageBackpack, packageVest, packageMask, packageGlasses, pendingPackageSkins.ToArray(), playerSkillset, language, lobbyID, clientPlatform);

			// If server is not whitelist-only then whitelisted players are treated as VIPs.
			bool prioritizeInQueue = !Provider.isWhitelisted & hasPermit;

			if (prioritizeInQueue)
			{
				if (Provider.pending.Count == 0)
				{
					Provider.pending.Add(newPendingPlayer);
					queuePosition = 0;
					shouldVerify = true;
				}
				else
				{
					// Player at front of queue is already in the process of being verified.
					Provider.pending.Insert(1, newPendingPlayer);
					queuePosition = 1;
					shouldVerify = false;
				}
			}
			else
			{
				queuePosition = MathfEx.ClampToByte(Provider.pending.Count);
				Provider.pending.Add(newPendingPlayer);

				// If there are other players in the queue then they are already being verified.
				shouldVerify = queuePosition == 0;
			}
			Provider._transportConnectionToPendingPlayerMap.Add(transportConnection, newPendingPlayer);

			UnturnedLog.info($"Added {playerID} to queue position {queuePosition} (shouldVerify: {shouldVerify})");

			newPendingPlayer.lastNotifiedQueuePosition = queuePosition;

			// Notification is unnecessary when immediately verified, but this feels safer.
			NetMessages.SendMessageToClient(EClientMessage.QueuePositionChanged, ENetReliability.Reliable, transportConnection, (NetPakWriter writer) =>
			{
				writer.WriteUInt8(queuePosition);
			});

			if (shouldVerify)
			{
				Provider.verifyNextPlayerInQueue();
			}
		}

		private static List<ulong> pendingPackageSkins = new List<ulong>();

		/// <summary>
		/// Kick players maybe trying to impersonate devs. (Reports of sad/bad experiences with pretenders, unfortunately.)
		/// 2023-09-19: relaxed this a bit by trimming names and using Equals/Starts/Ends rather than Contains
		/// because there was a player with Nelson in their username.
		/// </summary>
		private static bool IsNameBlockedByNelsonFilter(string name)
		{
			if (name.Equals("Nelson", System.StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			if (name.StartsWith("SDG", System.StringComparison.InvariantCultureIgnoreCase) && name.EndsWith("Nelson", System.StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private static bool IsSkinColorWithinThresholdOfTerrainColor(Color32 skinColor)
		{
			LevelAsset asset = Level.getAsset();
			if (asset == null || asset.terrainColorRules == null || asset.terrainColorRules.Count < 1)
			{
#if LOG_SKIN_TERRAIN_COLOR_COMPARISON
				UnturnedLog.info("Skipping skin color terrain color comparison because there is no asset or no rules");
#endif
				return false;
			}

			Color.RGBToHSV(skinColor, out float hue, out float saturation, out float value);

#if LOG_SKIN_TERRAIN_COLOR_COMPARISON
			UnturnedLog.info($"Skin color for terrain color comparison: {skinColor} Hue: {hue} Saturation: {saturation} Value: {value}");
#endif

			foreach (LevelAsset.TerrainColorRule rule in asset.terrainColorRules)
			{
				if (rule == null)
				{
#if LOG_SKIN_TERRAIN_COLOR_COMPARISON
					UnturnedLog.warn("Level asset terrain color rules contains null item");
#endif
					continue;
				}

				LevelAsset.TerrainColorRule.EComparisonResult comparisonResult = rule.CompareColors(hue, saturation, value);

#if LOG_SKIN_TERRAIN_COLOR_COMPARISON
				if (comparisonResult == LevelAsset.TerrainColorRule.EComparisonResult.OutsideHueThreshold)
				{
					UnturnedLog.info($"Skin color {skinColor} is outside hue threshold ({rule.hueThreshold}) for terrain color Hue: {rule.ruleHue} Saturation: {rule.ruleSaturation} Value: {rule.ruleValue}");
				}
				else if (comparisonResult == LevelAsset.TerrainColorRule.EComparisonResult.OutsideSaturationThreshold)
				{
					UnturnedLog.info($"Skin color {skinColor} is outside saturation threshold ({rule.saturationThreshold}) for terrain color Hue: {rule.ruleHue} Saturation: {rule.ruleSaturation} Value: {rule.ruleValue}");
				}
				else if (comparisonResult == LevelAsset.TerrainColorRule.EComparisonResult.OutsideValueThreshold)
				{
					UnturnedLog.info($"Skin color {skinColor} is outside value threshold ({rule.valueThreshold}) for terrain color Hue: {rule.ruleHue} Saturation: {rule.ruleSaturation} Value: {rule.ruleValue}");
				}
#endif // LOG_SKIN_TERRAIN_COLOR_COMPARISON

				if (comparisonResult == LevelAsset.TerrainColorRule.EComparisonResult.TooSimilar)
				{
					return true;
				}
			}

			return false;
		}

		internal static TransportConnectionRateLimiter joinRateLimiter = new TransportConnectionRateLimiter();
	}
}
