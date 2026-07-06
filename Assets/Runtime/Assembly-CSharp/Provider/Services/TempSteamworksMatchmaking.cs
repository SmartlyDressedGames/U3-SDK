////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Steamworks;
using SDG.Unturned;
using Unturned.SystemEx;
using System.Collections.Generic;
#if !DEDICATED_SERVER
using SDG.HostBans;
#endif // !DEDICATED_SERVER

namespace SDG.Provider
{
	public class TempSteamworksMatchmaking
	{
		public delegate void MasterServerAdded(int insert, SteamServerAdvertisement server);
		public delegate void MasterServerRemoved();
		public delegate void MasterServerResorted();
		public delegate void MasterServerRefreshed(EMatchMakingServerResponse response);
		public delegate void MasterServerQueryRefreshed(SteamServerAdvertisement server);
		//		public delegate void PingCompleted(SteamServerInfo server);
		public delegate void AttemptUpdated(int attempt);
		public delegate void TimedOut();

		public delegate void PlayersQueryRefreshed(string name, int score, float time);
		public delegate void RulesQueryRefreshed(Dictionary<string, string> rulesMap);

		public MasterServerAdded onMasterServerAdded;
		public MasterServerRemoved onMasterServerRemoved;
		public MasterServerResorted onMasterServerResorted;
		public MasterServerRefreshed onMasterServerRefreshed;
		public MasterServerQueryRefreshed onMasterServerQueryRefreshed;
		//		public static PingCompleted onPingCompleted;
		public event AttemptUpdated onAttemptUpdated;
		public event TimedOut onTimedOut;

		public PlayersQueryRefreshed onPlayersQueryRefreshed;
		public RulesQueryRefreshed onRulesQueryRefreshed;

		private SteamConnectionInfo connectionInfo;

		private ESteamServerList _currentList;
		public ESteamServerList currentList => _currentList;
		private string currentNameFilter;
		private System.Text.RegularExpressions.Regex currentNameRegex;
		private bool isCurrentNameFilterSet;
		private int currentMaxPingFilter;
		private EPlugins currentPluginsFilter;
		private EServerListCurationDefaultBehavior curationDefaultBehavior;
		private EServerListCurationDenyMode curationDenyMode;

		private List<SteamServerAdvertisement> _serverList = new List<SteamServerAdvertisement>();
		public List<SteamServerAdvertisement> serverList => _serverList;

		/// <summary>
		/// Used to show a warning when a lot of servers are blocked by curation list.
		/// </summary>
		public int CuratorBlockedServerCount
		{
			get;
			private set;
		}

		private List<MatchMakingKeyValuePair_t> filters;

		private ISteamMatchmakingPingResponse serverPingResponse;
		private ISteamMatchmakingServerListResponse serverListResponse;
		private ISteamMatchmakingPlayersResponse playersResponse;
		private ISteamMatchmakingRulesResponse rulesResponse;

		private HServerQuery playersQuery = HServerQuery.Invalid;
		private HServerQuery rulesQuery = HServerQuery.Invalid;
		private Dictionary<string, string> rulesMap;

		private HServerQuery serverQuery = HServerQuery.Invalid;
		private int serverQueryAttempts;
		public bool isAttemptingServerQuery
		{
			get;
			private set;
		}
		/// <summary>
		/// Reset after starting connection attempt, so set to true afterwards to auto join the server.
		/// </summary>
		public bool autoJoinServerQuery;
		public MenuPlayServerInfoUI.EServerInfoOpenContext serverQueryContext;
		private HServerListRequest serverListRequest = HServerListRequest.Invalid;
		private int serverListRefreshIndex = -1;

		private IComparer<SteamServerAdvertisement> _serverInfoComparer = new ServerListComparer_UtilityScore();
		public IComparer<SteamServerAdvertisement> serverInfoComparer => _serverInfoComparer;

		public void sortMasterServer(IComparer<SteamServerAdvertisement> newServerInfoComparer)
		{
			_serverInfoComparer = newServerInfoComparer;

			serverList.Sort(serverInfoComparer);

			onMasterServerResorted?.Invoke();
		}

		private void cleanupServerQuery()
		{
			if (serverQuery == HServerQuery.Invalid)
				return;

			SteamMatchmakingServers.CancelServerQuery(serverQuery);
			serverQuery = HServerQuery.Invalid;
		}

		private void cleanupPlayersQuery()
		{
			if (playersQuery == HServerQuery.Invalid)
				return;

			SteamMatchmakingServers.CancelServerQuery(playersQuery);
			playersQuery = HServerQuery.Invalid;
		}

		private void cleanupRulesQuery()
		{
			if (rulesQuery == HServerQuery.Invalid)
				return;

			SteamMatchmakingServers.CancelServerQuery(rulesQuery);
			rulesQuery = HServerQuery.Invalid;
		}

		private void cleanupServerListRequest()
		{
			if (serverListRequest == HServerListRequest.Invalid)
			{
				return;
			}

			SteamMatchmakingServers.ReleaseRequest(serverListRequest);
			serverListRequest = HServerListRequest.Invalid;
			serverListRefreshIndex = -1;
		}

		public void connect(SteamConnectionInfo info)
		{
			if (SDG.Unturned.Provider.isConnected)
			{
				return;
			}

			connectionInfo = info;
			serverQueryAttempts = 0;
			isAttemptingServerQuery = true;
			autoJoinServerQuery = false;
			serverQueryContext = MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT;

			attemptServerQuery();
		}

		public void cancel()
		{
			if (!isAttemptingServerQuery)
			{
				return;
			}

			serverQueryAttempts = 99;
			onPingFailedToRespond();
		}

		private void attemptServerQuery()
		{
			cleanupServerQuery();
			serverQuery = SteamMatchmakingServers.PingServer(connectionInfo.ip, connectionInfo.port, serverPingResponse);

			serverQueryAttempts++;

			onAttemptUpdated?.Invoke(serverQueryAttempts);
		}

		//public void refreshMasterServer(SteamServerInfo serverInfo)
		//{
		//	serverListRefreshIndex = serverList.IndexOf(serverInfo);
		//	if(serverListRefreshIndex < 0)
		//	{
		//		return;
		//	}

		//	SteamMatchmakingServers.RefreshServer(serverListRequest, serverListRefreshIndex);
		//}

		public void cancelRequest()
		{
			if (serverListRequest != HServerListRequest.Invalid)
			{
				SteamMatchmakingServers.CancelQuery(serverListRequest);
				cleanupServerListRequest();
			}
		}

		public void refreshMasterServer(ServerListFilters inputFilters)
		{
			_currentList = inputFilters.listSource;
			currentPluginsFilter = inputFilters.plugins;
			currentNameFilter = inputFilters.serverName;
			isCurrentNameFilterSet = !string.IsNullOrEmpty(currentNameFilter);
			ServerListCuration curation = ServerListCuration.Get();
			curationDefaultBehavior = curation.DefaultBehavior;
			curationDenyMode = curation.DenyMode;

			currentNameRegex = null;
			string regexPrefix = "regex:";
			if (isCurrentNameFilterSet && currentNameFilter.StartsWith(regexPrefix, System.StringComparison.InvariantCultureIgnoreCase))
			{
				string regexInput = currentNameFilter.Substring(regexPrefix.Length);
				try
				{
					currentNameRegex = new System.Text.RegularExpressions.Regex(regexInput);
				}
				catch (System.Exception e)
				{
					UnturnedLog.exception(e, $"Caught exception parsing server name regex \"{regexInput}\":");
					isCurrentNameFilterSet = false;
				}
			}

			currentMaxPingFilter = inputFilters.maxPing;
			_serverList.Clear();
			CuratorBlockedServerCount = 0;

			onMasterServerRemoved?.Invoke();

			cleanupServerListRequest();

			if (inputFilters.listSource == ESteamServerList.LAN)
			{
				serverListRequest = SteamMatchmakingServers.RequestLANServerList(SDG.Unturned.Provider.APP_ID, serverListResponse);

				return;
			}

			filters = new List<MatchMakingKeyValuePair_t>();

			MatchMakingKeyValuePair_t keyGame = new MatchMakingKeyValuePair_t();
			keyGame.m_szKey = "gamedir";
#if EXPERIMENTAL
			keyGame.m_szValue = "unturned experimental";
#else
			keyGame.m_szValue = "unturned";
#endif
			filters.Add(keyGame);

			if (inputFilters.mapNames != null && inputFilters.mapNames.Count > 0)
			{
				List<LevelInfo> filteredLevels = new List<LevelInfo>();
				inputFilters.GetLevels(filteredLevels);
				if (filteredLevels.Count > 0)
				{
					if (filteredLevels.Count > 1)
					{
						// Please refer to isteammatchmaking.h for explanation of how this works!
						// Polish notation: https://en.wikipedia.org/wiki/Polish_notation
						int orLength = filteredLevels.Count * 3;// and + map + version
						filters.Add(new MatchMakingKeyValuePair_t() { m_szKey = "or", m_szValue = orLength.ToString() });
					}

					foreach (LevelInfo level in filteredLevels)
					{
						filters.Add(new MatchMakingKeyValuePair_t() { m_szKey = "and", m_szValue = "2" });

						MatchMakingKeyValuePair_t keyMap = new MatchMakingKeyValuePair_t();
						keyMap.m_szKey = "map";
						keyMap.m_szValue = level.name.ToLower();
						filters.Add(keyMap);

						MatchMakingKeyValuePair_t keyMapData = new MatchMakingKeyValuePair_t();
						keyMapData.m_szKey = "gamedataand";
						keyMapData.m_szValue = "MAP_VERSION_" + VersionUtils.binaryToHexadecimal(level.configData.PackedVersion);
						filters.Add(keyMapData);
					}
				}
			}

			if (inputFilters.attendance == EAttendance.Empty)
			{
				MatchMakingKeyValuePair_t keyEmpty = new MatchMakingKeyValuePair_t();
				keyEmpty.m_szKey = "noplayers";
				keyEmpty.m_szValue = "1";
				filters.Add(keyEmpty);
			}
			else if (inputFilters.attendance == EAttendance.HasPlayers)
			{
				MatchMakingKeyValuePair_t keyPlayers = new MatchMakingKeyValuePair_t();
				keyPlayers.m_szKey = "hasplayers";
				keyPlayers.m_szValue = "1";
				filters.Add(keyPlayers);
			}

			if (inputFilters.notFull)
			{
				MatchMakingKeyValuePair_t keySpace = new MatchMakingKeyValuePair_t();
				keySpace.m_szKey = "notfull";
				keySpace.m_szValue = "1";
				filters.Add(keySpace);
			}

			MatchMakingKeyValuePair_t keyData = new MatchMakingKeyValuePair_t();
			keyData.m_szKey = "gamedataand";
			//keyData.m_szValue = filterPassword ? "PASS" : "SSAP";
			if (inputFilters.password == EPassword.YES)
			{
				keyData.m_szValue = "PASS";
			}
			else if (inputFilters.password == EPassword.NO)
			{
				keyData.m_szValue = "SSAP";
			}

			if (inputFilters.vacProtection == EVACProtectionFilter.Secure)
			{
				keyData.m_szValue += ",";
				keyData.m_szValue += "VAC_ON";

				MatchMakingKeyValuePair_t keySecure = new MatchMakingKeyValuePair_t();
				keySecure.m_szKey = "secure";
				keySecure.m_szValue = "1";
				filters.Add(keySecure);
			}
			else if (inputFilters.vacProtection == EVACProtectionFilter.Insecure)
			{
				keyData.m_szValue += ",";
				keyData.m_szValue += "VAC_OFF";
			}

			// Game Version
			keyData.m_szValue += ",GAME_VERSION_";
			keyData.m_szValue += VersionUtils.binaryToHexadecimal(Unturned.Provider.APP_VERSION_PACKED);

			if (Unturned.Provider._modInfo != null)
			{
				keyData.m_szValue += ",MOD_NAME_";
				keyData.m_szValue += Unturned.Provider._modInfo.FormatServerListName();
				keyData.m_szValue += ",MOD_VERSION_";
				keyData.m_szValue += VersionUtils.binaryToHexadecimal(Unturned.Provider._modInfo.GetPackedVersion());
			}
			else
			{
				keyData.m_szValue += ",MOD_NAME_NA,MOD_VERSION_NA";
			}

			if (!string.IsNullOrEmpty(keyData.m_szValue))
			{
				filters.Add(keyData);
			}

			MatchMakingKeyValuePair_t keyTags = new MatchMakingKeyValuePair_t();
			keyTags.m_szKey = "gametagsand";
			//keyTags.m_szValue = filterWorkshop ? "WORK" : "KROW";

			if (inputFilters.workshop == EWorkshop.YES)
			{
				keyTags.m_szValue = "WSy";
			}
			else if (inputFilters.workshop == EWorkshop.NO)
			{
				keyTags.m_szValue = "WSn";
			}

			if (inputFilters.combat == ECombat.PVP)
			{
				keyTags.m_szValue += ",PVP";
			}
			else if (inputFilters.combat == ECombat.PVE)
			{
				keyTags.m_szValue += ",PVE";
			}

			if (inputFilters.cheats == ECheats.YES)
			{
				keyTags.m_szValue += ",CHy";
			}
			else if (inputFilters.cheats == ECheats.NO)
			{
				keyTags.m_szValue += ",CHn";
			}

			if (inputFilters.camera != ECameraMode.ANY)
			{
				keyTags.m_szValue += "," + SDG.Unturned.Provider.getCameraModeTagAbbreviation(inputFilters.camera);
			}

			if (inputFilters.monetization == EServerMonetizationTag.None)
			{
				keyTags.m_szValue += "," + SDG.Unturned.Provider.GetMonetizationTagAbbreviation(inputFilters.monetization);
			}
			else
			{
				bool addNoneOrNonGameplayFilter = inputFilters.monetization == EServerMonetizationTag.NonGameplay;
#if !DEDICATED_SERVER
				if (!LiveConfig.Get().shouldServersWithoutMonetizationTagBeVisibleInInternetServerList)
				{
					// Experiment/Decision has not been rolled-back using live config.
					// We skip this extra filter if looking for a specific server by name.
					addNoneOrNonGameplayFilter |= isCurrentNameFilterSet;
				}
#endif // !DEDICATED_SERVER

				if (addNoneOrNonGameplayFilter)
				{
					// None OR NonGameplay pass
					filters.Add(new MatchMakingKeyValuePair_t() { m_szKey = "or", m_szValue = "2" });
					filters.Add(new MatchMakingKeyValuePair_t() { m_szKey = "gametagsand", m_szValue = SDG.Unturned.Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.None) });
					filters.Add(new MatchMakingKeyValuePair_t() { m_szKey = "gametagsand", m_szValue = SDG.Unturned.Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.NonGameplay) });
				}
			}

			if (inputFilters.gold == EServerListGoldFilter.RequiresGold)
			{
				keyTags.m_szValue += ",GLD";
			}
			else if (inputFilters.gold == EServerListGoldFilter.DoesNotRequireGold)
			{
				keyTags.m_szValue += ",F2P";
			}

#if WITH_THIRDPARTYAC
			if (inputFilters.thirdpartyAntiCheatProtection == EThirdpartyAntiCheatProtectionFilter.Secure)
			{
				keyTags.m_szValue += ",BEy";
			}
			else if (inputFilters.thirdpartyAntiCheatProtection == EThirdpartyAntiCheatProtectionFilter.Insecure)
			{
				keyTags.m_szValue += ",BEn";
			}
#endif

			if (!string.IsNullOrEmpty(keyTags.m_szValue))
			{
				filters.Add(keyTags);
			}

			System.Text.StringBuilder logBuilder = new System.Text.StringBuilder(128);
			logBuilder.Append("Refreshing server list with filters:");
			foreach (MatchMakingKeyValuePair_t kvp in filters)
			{
				logBuilder.Append(' ');
				logBuilder.Append(kvp.m_szKey);
				logBuilder.Append('=');
				logBuilder.Append(kvp.m_szValue);
			}
			UnturnedLog.info(logBuilder.ToString());

			if (inputFilters.listSource == ESteamServerList.HISTORY)
			{
				serverListRequest = SteamMatchmakingServers.RequestHistoryServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint) filters.Count, serverListResponse);

				return;
			}

			if (inputFilters.listSource == ESteamServerList.FAVORITES)
			{
				serverListRequest = SteamMatchmakingServers.RequestFavoritesServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint) filters.Count, serverListResponse);

				return;
			}

			if (inputFilters.listSource == ESteamServerList.INTERNET)
			{
				curation.RefreshIfDirty();
				curation.MergeRulesIfDirty();
				curation.ResetBlockedServerCounts();
				serverListRequest = SteamMatchmakingServers.RequestInternetServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint) filters.Count, serverListResponse);

				return;
			}

			if (inputFilters.listSource == ESteamServerList.FRIENDS)
			{
				serverListRequest = SteamMatchmakingServers.RequestFriendsServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint) filters.Count, serverListResponse);

				return;
			}
		}

		public void refreshPlayers(uint ip, ushort port)
		{
			cleanupPlayersQuery();
			playersQuery = SteamMatchmakingServers.PlayerDetails(ip, port, playersResponse);
		}

		public void refreshRules(uint ip, ushort port)
		{
			cleanupRulesQuery();
			rulesMap = new Dictionary<string, string>();

			rulesQuery = SteamMatchmakingServers.ServerRules(ip, port, rulesResponse);
		}

		private void onPingResponded(gameserveritem_t data)
		{
			isAttemptingServerQuery = false;
			cleanupServerQuery();

			if (data.m_nAppID == SDG.Unturned.Provider.APP_ID.m_AppId)
			{
				SteamServerAdvertisement info = new SteamServerAdvertisement(data, SteamServerAdvertisement.EInfoSource.DirectConnect);

				if (!info.isPro || SDG.Unturned.Provider.isPro)
				{
					bool shouldImmediatelyConnect = false;

					if (autoJoinServerQuery && (!info.isPassworded || !string.IsNullOrEmpty(connectionInfo.password)))
					{
						shouldImmediatelyConnect = true;
					}

					if (shouldImmediatelyConnect)
					{
						ServerConnectParameters parameters = new ServerConnectParameters(new IPv4Address(info.ip), info.queryPort, info.connectionPort, connectionInfo.password);
						SDG.Unturned.Provider.connect(parameters, info, null);
					}
					else
					{
						ServerListCurationInput curationInput = new ServerListCurationInput(info);
						ServerListCurationOutput curationOutput = default;
						ServerListCuration.Get().EvaluateMergedRules(curationInput, ref curationOutput);
						info.serverCurationLabels = curationOutput.labels;

						MenuUI.closeAll();
						MenuUI.closeAlert();
						MenuPlayServerInfoUI.open(info, connectionInfo.password, serverQueryContext);
					}
				}
				else
				{
					SDG.Unturned.Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_SERVER;
				}
			}
			else
			{
				SDG.Unturned.Provider._connectionFailureInfo = ESteamConnectionFailureInfo.TIMED_OUT;
			}

			onTimedOut?.Invoke();
		}

		private void onPingFailedToRespond()
		{
			if (serverQueryAttempts < 10)
			{
				attemptServerQuery();
			}
			else
			{
				isAttemptingServerQuery = false;
				cleanupServerQuery();

				SDG.Unturned.Provider._connectionFailureInfo = ESteamConnectionFailureInfo.TIMED_OUT;

				onTimedOut?.Invoke();
			}
		}

		private void onServerListResponded(HServerListRequest request, int index)
		{
			if (request != serverListRequest)
			{
				return;
			}

			gameserveritem_t details = SteamMatchmakingServers.GetServerDetails(request, index);

			if (_currentList == ESteamServerList.INTERNET && !details.m_steamID.BPersistentGameServerAccount())
			{
				// Nelson 2024-11-15: Turning off this logging because it's been the default for a long time now.
				// UnturnedLog.info($"Ignoring server \"{details.GetServerName()}\" because it is anonymous on the internet list");
				return;
			}

			if (details.m_nAppID != SDG.Unturned.Provider.APP_ID.m_AppId)
			{
				// Apparently LAN server list can return non-Unturned servers.
				// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/3309
				UnturnedLog.info($"Ignoring server \"{details.GetServerName()}\" because it has a different AppID ({details.m_nAppID})");
				return;
			}

			IPv4Address address = new IPv4Address(details.m_NetAdr.GetIP());

#if !DEDICATED_SERVER
			EHostBanFlags banFlags = HostBansManager.Get().MatchBasicDetails(address, details.m_NetAdr.GetQueryPort(), details.GetServerName(), details.m_steamID.m_SteamID);
			if (banFlags.HasFlag(EHostBanFlags.HiddenFromAllServerLists) ||
				banFlags.HasFlag(EHostBanFlags.Blocked) ||
				(_currentList == ESteamServerList.INTERNET && banFlags.HasFlag(EHostBanFlags.HiddenFromInternetServerList)))
			{
				return;
			}
#endif // !DEDICATED_SERVER

			SteamServerAdvertisement.EInfoSource infoSource;
			switch (_currentList)
			{
				default:
				case ESteamServerList.INTERNET:
					infoSource = SteamServerAdvertisement.EInfoSource.InternetServerList;
					break;
				case ESteamServerList.FRIENDS:
					infoSource = SteamServerAdvertisement.EInfoSource.FriendServerList;
					break;
				case ESteamServerList.FAVORITES:
					infoSource = SteamServerAdvertisement.EInfoSource.FavoriteServerList;
					break;
				case ESteamServerList.HISTORY:
					infoSource = SteamServerAdvertisement.EInfoSource.HistoryServerList;
					break;
				case ESteamServerList.LAN:
					infoSource = SteamServerAdvertisement.EInfoSource.LanServerList;
					break;
			}
			SteamServerAdvertisement serverInfo = new SteamServerAdvertisement(details, infoSource);

#if !DEDICATED_SERVER
			banFlags |= HostBansManager.Get().MatchExtendedDetails(serverInfo.descText, serverInfo.thumbnailURL);
			if (banFlags.HasFlag(EHostBanFlags.HiddenFromAllServerLists) ||
				banFlags.HasFlag(EHostBanFlags.Blocked) ||
				(_currentList == ESteamServerList.INTERNET && banFlags.HasFlag(EHostBanFlags.HiddenFromInternetServerList)))
			{
				return;
			}

			serverInfo.SetServerListHostBanFlags(banFlags);
#endif // !DEDICATED_SERVER

			string labels = null;
			if (_currentList == ESteamServerList.INTERNET)
			{
				ServerListCuration curation = ServerListCuration.Get();
				ServerListCurationInput curationInput = new ServerListCurationInput(serverInfo);
				ServerListCurationOutput curationOutput = default;
				curation.EvaluateMergedRules(curationInput, ref curationOutput);
				labels = curationOutput.labels;
				if (!curationOutput.allowed)
				{
					++CuratorBlockedServerCount;
					if (curationOutput.allowOrDenyRule != null)
					{
						++curationOutput.allowOrDenyRule.latestBlockedServerCount;
						++curationOutput.allowOrDenyRule.owner.latestBlockedServerCount;
					}

// 					if (curationOutput.allowOrDenyRule != null)
// 					{
// 						UnturnedLog.info($"Server Curation: \"{curationInput.name}\" {curationInput.address}:{curationInput.queryPort} ({curationInput.steamId}) matched Deny rule from \"{curationOutput.allowOrDenyRule.owner.Name}\": \"{curationOutput.allowOrDenyRule.description}\"");
// 					}
					if (curationDenyMode == EServerListCurationDenyMode.Hide)
					{
						return;
					}
					else
					{
						serverInfo.isDeniedByServerCurationRule = true;
						serverInfo.isDeprioritizedByServerCuration = true;
						serverInfo.deniedByRule = curationOutput.allowOrDenyRule;
					}
				}

				if (!curationOutput.matchedAnyRules)
				{
					if (curationDefaultBehavior == EServerListCurationDefaultBehavior.MoveToBottom)
					{
						serverInfo.isDeprioritizedByServerCuration = true;
					}
					else if (curationDefaultBehavior == EServerListCurationDefaultBehavior.Hide)
					{
						return;
					}
				}
			}
			serverInfo.serverCurationLabels = labels;

			if (index == serverListRefreshIndex)
			{
				onMasterServerQueryRefreshed?.Invoke(serverInfo);

				return;
			}

			if (currentPluginsFilter == EPlugins.NO)
			{
				if (serverInfo.pluginFramework != SteamServerAdvertisement.EPluginFramework.None)
				{
					return;
				}
			}
			else if (currentPluginsFilter == EPlugins.YES)
			{
				if (serverInfo.pluginFramework == SteamServerAdvertisement.EPluginFramework.None)
				{
					return;
				}
			}

			if (serverInfo.maxPlayers < CommandMaxPlayers.MIN_NUMBER)
			{
				return;
			}

			if (isCurrentNameFilterSet)
			{
				if (currentNameRegex != null)
				{
					if (!currentNameRegex.IsMatch(serverInfo.name))
					{
						return;
					}
				}
				else
				{
					if (serverInfo.name.IndexOf(currentNameFilter, System.StringComparison.OrdinalIgnoreCase) == -1)
					{
						return;
					}
				}
			}
			else
			{
				// If they aren't looking for a specific named server, then we
				// filter out servers with more than recommended max players

				// Plugins were setting advertised MaxPlayers low while allowing a higher number to bypass the
				// recommended values, so we have to consider that player count can be higher.
				int realMaxPlayers = Mathf.Max(serverInfo.players, serverInfo.maxPlayers);
				if (realMaxPlayers > CommandMaxPlayers.MAX_NUMBER)
				{
					return;
				}
			}

			if (currentMaxPingFilter > 0 && serverInfo.PingMs > currentMaxPingFilter)
			{
				return;
			}

			serverInfo.CalculateUtilityScore();

			int insert = serverList.BinarySearch(serverInfo, serverInfoComparer);

			if (insert < 0)
			{
				insert = ~insert;
			}

			serverList.Insert(insert, serverInfo);

			onMasterServerAdded?.Invoke(insert, serverInfo);
		}

		private void onServerListFailedToRespond(HServerListRequest request, int index)
		{ }

		private void onRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
		{
			if (request == serverListRequest)
			{
				onMasterServerRefreshed?.Invoke(response);

				if (response == EMatchMakingServerResponse.eNoServersListedOnMasterServer || serverList.Count == 0)
				{
					UnturnedLog.info("No servers found on the master server.");
					return;
				}

				if (response == EMatchMakingServerResponse.eServerFailedToRespond)
				{
					UnturnedLog.error("Failed to connect to the master server.");
					return;
				}

				if (response == EMatchMakingServerResponse.eServerResponded)
				{
					UnturnedLog.info("Successfully refreshed the master server.");
					return;
				}
			}
		}

		private void onAddPlayerToList(string name, int score, float time)
		{
			onPlayersQueryRefreshed?.Invoke(name, score, time);
		}

		private void onPlayersFailedToRespond()
		{
			UnturnedLog.warn("Server players query failed to respond");
		}

		private void onPlayersRefreshComplete()
		{ }

		private void onRulesResponded(string key, string value)
		{
			if (rulesMap == null)
			{
				return;
			}

			rulesMap.Add(key, value);
		}

		private void onRulesFailedToRespond()
		{
			UnturnedLog.warn("Server rules query failed to respond");
		}

		private void onRulesRefreshComplete()
		{
			onRulesQueryRefreshed?.Invoke(rulesMap);
		}

		public TempSteamworksMatchmaking()
		{
			serverPingResponse = new ISteamMatchmakingPingResponse(onPingResponded, onPingFailedToRespond);
			serverListResponse = new ISteamMatchmakingServerListResponse(onServerListResponded, onServerListFailedToRespond, onRefreshComplete);
			playersResponse = new ISteamMatchmakingPlayersResponse(onAddPlayerToList, onPlayersFailedToRespond, onPlayersRefreshComplete);
			rulesResponse = new ISteamMatchmakingRulesResponse(onRulesResponded, onRulesFailedToRespond, onRulesRefreshComplete);
		}
	}
}
