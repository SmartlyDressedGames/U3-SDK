////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;

namespace SDG.Unturned
{
	public class ServerListFilters
	{
		public string presetName = string.Empty;
		/// <summary>
		/// Assigned when a named preset is created.
		/// 0 is the default and should be replaced by a preset when loaded.
		/// -1 indicates the preset was modified.
		/// -2 and below are the default presets.
		/// </summary>
		public int presetId;

		public string serverName = string.Empty;
		public HashSet<string> mapNames = new HashSet<string>();
		public EPassword password = EPassword.NO;
		public EWorkshop workshop = EWorkshop.ANY;
		public EPlugins plugins = EPlugins.ANY;

		/// <summary>
		/// Nelson 2024-09-20: Changing default to any (from has players) because the default server list sort will now
		/// push empty servers to the bottom.
		/// </summary>
		public EAttendance attendance = EAttendance.Any;

		/// <summary>
		/// If true, only servers with available player slots are shown.
		/// Nelson 2024-09-20: Changing default to false because the default server list sort will now push them down
		/// from the top a little bit.
		/// </summary>
		public bool notFull = false;

		public EVACProtectionFilter vacProtection = EVACProtectionFilter.Secure;
#if WITH_THIRDPARTYAC
		public EThirdpartyAntiCheatProtectionFilter thirdpartyAntiCheatProtection = EThirdpartyAntiCheatProtectionFilter.Secure;
#endif
		public ECombat combat = ECombat.ANY;
		public ECheats cheats = ECheats.ANY;
		public ECameraMode camera = ECameraMode.ANY;
		public EServerMonetizationTag monetization = EServerMonetizationTag.Any;
		public EServerListGoldFilter gold = EServerListGoldFilter.Any;
		public ESteamServerList listSource;

		/// <summary>
		/// If >0, servers with ping higher than this will not be shown.
		/// </summary>
		public int maxPing = FilterSettings.DEFAULT_MAX_PING;

		public void GetLevels(List<LevelInfo> levels)
		{
			foreach (string mapName in mapNames)
			{
				LevelInfo levelInfo = Level.FindLevelForServerFilterExact(mapName);
				if (levelInfo != null)
				{
					levels.Add(levelInfo);
				}
			}
		}

		public string GetMapDisplayText()
		{
			string result = string.Empty;

			foreach (string mapName in mapNames)
			{
				if (result.Length > 0)
				{
					result += ", ";
				}

				LevelInfo levelInfo = Level.FindLevelForServerFilterExact(mapName);
				if (levelInfo != null)
				{
					result += levelInfo.getLocalizedName();
				}
				else
				{
					result += mapName;
				}
			}

			return result;
		}

		/// <returns>True if level was added to the list of maps.</returns>
		public bool ToggleMap(LevelInfo levelInfo)
		{
			if (levelInfo == null)
			{
				return false;
			}

			string mapName = levelInfo.name.ToLower();
			bool removed = mapNames.Remove(mapName);
			if (!removed)
			{
				mapNames.Add(mapName);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void ClearMaps()
		{
			mapNames.Clear();
		}

		public void CopyFrom(ServerListFilters source)
		{
			presetName = source.presetName;
			presetId = source.presetId;
			serverName = source.serverName;
			mapNames = new HashSet<string>(source.mapNames);
			password = source.password;
			workshop = source.workshop;
			plugins = source.plugins;
			attendance = source.attendance;
			notFull = source.notFull;
			vacProtection = source.vacProtection;
#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatProtection = source.thirdpartyAntiCheatProtection;
#endif
			combat = source.combat;
			cheats = source.cheats;
			camera = source.camera;
			monetization = source.monetization;
			gold = source.gold;
			listSource = source.listSource;
			maxPing = source.maxPing;
		}

		public void Read(byte version, Block block)
		{
			if (version >= FilterSettings.SAVEDATA_VERSION_MULTIPLE_MAPS)
			{
				int mapCount = block.readInt32();
				for (int index = 0; index < mapCount; ++index)
				{
					string mapName = block.readString();
					if (!string.IsNullOrEmpty(mapName))
					{
						mapNames.Add(mapName);
					}
				}
			}
			else
			{
				string mapName = block.readString();
				if (!string.IsNullOrEmpty(mapName))
				{
					mapNames.Add(mapName);
				}
			}

			password = (EPassword) block.readByte();
			workshop = (EWorkshop) block.readByte();
			plugins = (EPlugins) block.readByte();
			attendance = (EAttendance) block.readByte();
			notFull = block.readBoolean();
			vacProtection = (EVACProtectionFilter) block.readByte();
#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatProtection = (EThirdpartyAntiCheatProtectionFilter) block.readByte();
#else
			block.readByte();
#endif
			combat = (ECombat) block.readByte();
			cheats = (ECheats) block.readByte();
			camera = (ECameraMode) block.readByte();
			monetization = (EServerMonetizationTag) block.readByte();
			gold = (EServerListGoldFilter) block.readByte();
			serverName = block.readString();
			listSource = (ESteamServerList) block.readByte();
			presetName = block.readString();
			presetId = block.readInt32();

			if (version >= FilterSettings.SAVEDATA_VERSION_MAX_PING)
			{
				maxPing = block.readInt32();
				if (version < FilterSettings.SAVEDATA_VERSION_INCREASED_DEFAULT_MAX_PING && maxPing == 200)
				{
					maxPing = FilterSettings.DEFAULT_MAX_PING;
				}
			}
			else
			{
				maxPing = FilterSettings.DEFAULT_MAX_PING;
			}
		}

		public void Write(Block block)
		{
			block.writeInt32(mapNames.Count);
			foreach (string mapName in mapNames)
			{
				block.writeString(mapName);
			}
			block.writeByte((byte) password);
			block.writeByte((byte) workshop);
			block.writeByte((byte) plugins);
			block.writeByte((byte) attendance);
			block.writeBoolean(notFull);
			block.writeByte((byte) vacProtection);
#if WITH_THIRDPARTYAC
			block.writeByte((byte) thirdpartyAntiCheatProtection);
#else
			block.writeByte(0);
#endif
			block.writeByte((byte) combat);
			block.writeByte((byte) cheats);
			block.writeByte((byte) camera);
			block.writeByte((byte) monetization);
			block.writeByte((byte) gold);
			block.writeString(serverName);
			block.writeByte((byte) listSource);
			block.writeString(presetName);
			block.writeInt32(presetId);
			block.writeInt32(maxPing);
		}
	}

	public static class FilterSettings
	{
		/// <summary>
		/// Version before named version constants were introduced. (2023-11-13)
		/// </summary>
		public const byte SAVEDATA_VERSION_INITIAL = 14;
		public const byte SAVEDATA_VERSION_ADDED_GOLD_FILTER = 15;
		public const byte SAVEDATA_VERSION_MOVED_SERVER_NAME_FILTER = 16;
		public const byte SAVEDATA_VERSION_ADDED_PRESETS = 17;
		public const byte SAVEDATA_VERSION_SAVE_COLUMNS = 18;
		public const byte SAVEDATA_VERSION_SAVE_SUBMENUS_OPEN = 19;
		public const byte SAVEDATA_VERSION_MULTIPLE_MAPS = 20;
		public const byte SAVEDATA_VERSION_FILTER_VISIBILITY = 21;
		public const byte SAVEDATA_VERSION_MAX_PING = 22;
		public const byte SAVEDATA_VERSION_ADDED_FULLNESS_COLUMN = 23;
		public const byte SAVEDATA_VERSION_INCREASED_DEFAULT_MAX_PING = 24;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_INCREASED_DEFAULT_MAX_PING;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		public const int DEFAULT_MAX_PING = 300;

		public static ServerListFilters activeFilters = new ServerListFilters();
		public static bool isColumnsEditorOpen;
		public static bool isPresetsListOpen;
		public static bool isQuickFiltersEditorOpen;
		public static bool isQuickFiltersVisibilityEditorOpen;

		public static event System.Action OnActiveFiltersModified;

		public static event System.Action OnActiveFiltersReplaced;
		public static void InvokeActiveFiltersReplaced()
		{
			OnActiveFiltersReplaced?.Invoke();
		}

		public static event System.Action OnCustomPresetsListChanged;
		public static void InvokeCustomFiltersListChanged()
		{
			OnCustomPresetsListChanged?.Invoke();
		}

		public static List<ServerListFilters> customPresets = new List<ServerListFilters>();
		private static int nextCustomPresetId = 1;

		public static ServerListFilters defaultPresetInternet = new ServerListFilters();
		public static ServerListFilters defaultPresetLAN = new ServerListFilters();
		public static ServerListFilters defaultPresetHistory = new ServerListFilters();
		public static ServerListFilters defaultPresetFavorites = new ServerListFilters();
		public static ServerListFilters defaultPresetFriends = new ServerListFilters();

		public class ServerBrowserColumns
		{
			public bool map = true;
			public bool players = true;
			public bool maxPlayers = false;
			public bool ping = true;
			public bool anticheat = false;
			public bool perspective = false;
			public bool combat = false;
			public bool password = false;
			public bool workshop = false;
			public bool gold = false;
			public bool cheats = false;
			public bool monetization = false;
			public bool plugins = false;

			/// <summary>
			/// % Full
			/// </summary>
			public bool fullnessPercentage = false;

			public void Read(byte version, Block block)
			{
				map = block.readBoolean();
				players = block.readBoolean();
				maxPlayers = block.readBoolean();
				ping = block.readBoolean();
				anticheat = block.readBoolean();
				perspective = block.readBoolean();
				combat = block.readBoolean();
				password = block.readBoolean();
				workshop = block.readBoolean();
				gold = block.readBoolean();
				cheats = block.readBoolean();
				monetization = block.readBoolean();
				plugins = block.readBoolean();

				if (version >= SAVEDATA_VERSION_ADDED_FULLNESS_COLUMN)
				{
					fullnessPercentage = block.readBoolean();
				}
				else
				{
					fullnessPercentage = false;
				}
			}

			public void Write(Block block)
			{
				block.writeBoolean(map);
				block.writeBoolean(players);
				block.writeBoolean(maxPlayers);
				block.writeBoolean(ping);
				block.writeBoolean(anticheat);
				block.writeBoolean(perspective);
				block.writeBoolean(combat);
				block.writeBoolean(password);
				block.writeBoolean(workshop);
				block.writeBoolean(gold);
				block.writeBoolean(cheats);
				block.writeBoolean(monetization);
				block.writeBoolean(plugins);
				block.writeBoolean(fullnessPercentage);
			}
		}

		public static ServerBrowserColumns columns = new ServerBrowserColumns();

		public class ServerBrowserFilterVisibility
		{
			public bool name = true;
			public bool map = true;
			public bool password = false;
			public bool workshop = false;
			public bool plugins = false;
			public bool attendance = false;
			public bool notFull = false;
			public bool vacProtection = false;
#if WITH_THIRDPARTYAC
			public bool thirdpartyAntiCheatProtection = false;
#endif
			public bool combat = true;
			public bool cheats = false;
			public bool camera = true;
			public bool monetization = false;
			public bool gold = false;
			public bool listSource = true;
			public bool maxPing = false;

			public void Read(byte version, Block block)
			{
				name = block.readBoolean();
				map = block.readBoolean();
				password = block.readBoolean();
				workshop = block.readBoolean();
				plugins = block.readBoolean();
				attendance = block.readBoolean();
				notFull = block.readBoolean();
				vacProtection = block.readBoolean();
#if WITH_THIRDPARTYAC
				thirdpartyAntiCheatProtection = block.readBoolean();
#else
				block.readBoolean();
#endif
				combat = block.readBoolean();
				cheats = block.readBoolean();
				camera = block.readBoolean();
				monetization = block.readBoolean();
				gold = block.readBoolean();
				listSource = block.readBoolean();

				if (version >= SAVEDATA_VERSION_MAX_PING)
				{
					maxPing = block.readBoolean();
				}
				else
				{
					maxPing = false;
				}
			}

			public void Write(Block block)
			{
				block.writeBoolean(name);
				block.writeBoolean(map);
				block.writeBoolean(password);
				block.writeBoolean(workshop);
				block.writeBoolean(plugins);
				block.writeBoolean(attendance);
				block.writeBoolean(notFull);
				block.writeBoolean(vacProtection);
#if WITH_THIRDPARTYAC
				block.writeBoolean(thirdpartyAntiCheatProtection);
#else
				block.writeBoolean(false);
#endif
				block.writeBoolean(combat);
				block.writeBoolean(cheats);
				block.writeBoolean(camera);
				block.writeBoolean(monetization);
				block.writeBoolean(gold);
				block.writeBoolean(listSource);
				block.writeBoolean(maxPing);
			}
		}

		public static ServerBrowserFilterVisibility filterVisibility = new ServerBrowserFilterVisibility();

		public static int CreatePresetId()
		{
			int id = nextCustomPresetId;
			++nextCustomPresetId;
			return id;
		}

		public static void RemovePreset(int presetId)
		{
			for (int index = customPresets.Count - 1; index >= 0; --index)
			{
				ServerListFilters preset = customPresets[index];
				if (preset.presetId == presetId)
				{
					customPresets.RemoveAt(index);
				}
			}
		}

		public static void MarkActiveFilterModified()
		{
			if (activeFilters.presetId != -1)
			{
				if (activeFilters.presetId < -1)
				{
					// If modifying default filter, reset name to list name before adding (Modified)
					// suffix. This allows the default filters to be named like "Internet (Default)"
					// without becoming "Internet (Default) (Modified)".
					if (activeFilters.presetId == defaultPresetInternet.presetId)
					{
						activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("List_Internet_Label");
					}
					else if (activeFilters.presetId == defaultPresetLAN.presetId)
					{
						activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("List_LAN_Label");
					}
					else if (activeFilters.presetId == defaultPresetHistory.presetId)
					{
						activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("List_History_Label");
					}
					else if (activeFilters.presetId == defaultPresetFavorites.presetId)
					{
						activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("List_Favorites_Label");
					}
					else if (activeFilters.presetId == defaultPresetFriends.presetId)
					{
						activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("List_Friends_Label");
					}
					else
					{
						UnturnedLog.warn($"Marking active filter modified unknown default preset ID ({activeFilters.presetId})");
					}
				}

				if (!string.IsNullOrEmpty(activeFilters.presetName))
				{
					activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("PresetName_Modified", activeFilters.presetName);
				}
				activeFilters.presetId = -1;

				OnActiveFiltersModified?.Invoke();
			}
		}

		public static void load()
		{
			if (ReadWrite.fileExists("/Filters.dat", true))
			{
				Block block = ReadWrite.readBlock("/Filters.dat", true, 0);

				if (block != null)
				{
					byte version = block.readByte();

					if (version > 2)
					{
						if (version >= SAVEDATA_VERSION_MULTIPLE_MAPS)
						{
							int mapCount = block.readInt32();
							for (int index = 0; index < mapCount; ++index)
							{
								string mapName = block.readString();
								if (!string.IsNullOrEmpty(mapName))
								{
									activeFilters.mapNames.Add(mapName);
								}
							}
						}
						else
						{
							string mapName = block.readString();
							if (!string.IsNullOrEmpty(mapName))
							{
								activeFilters.mapNames.Add(mapName);
							}
						}

						if (version > 5)
						{
							activeFilters.password = (EPassword) block.readByte();

							activeFilters.workshop = (EWorkshop) block.readByte();
							if (version < 12)
							{
								// Changed default to include workshop since many servers are now using workshop content.
								activeFilters.workshop = EWorkshop.ANY;
							}
						}
						else
						{
							block.readBoolean();
							block.readBoolean();

							activeFilters.password = EPassword.NO;
							activeFilters.workshop = EWorkshop.ANY;
						}

						if (version < 7)
						{
							activeFilters.plugins = EPlugins.ANY;
						}
						else
						{
							activeFilters.plugins = (EPlugins) block.readByte();
						}

						activeFilters.attendance = (EAttendance) block.readByte();

						if (version >= 14)
						{
							activeFilters.notFull = block.readBoolean();
						}
						else
						{
							// Added in version 14. Default to only showing servers with available player slots.
							activeFilters.notFull = true;
						}

						activeFilters.vacProtection = (EVACProtectionFilter) block.readByte();

#if WITH_THIRDPARTYAC
						if (version > 10)
						{
							activeFilters.thirdpartyAntiCheatProtection = (EThirdpartyAntiCheatProtectionFilter) block.readByte();
						}
						else
						{
							activeFilters.thirdpartyAntiCheatProtection = EThirdpartyAntiCheatProtectionFilter.Secure;
						}
#else
						if (version > 10)
						{
							block.readByte();
						}
#endif // WITH_THIRDPARTYAC

						activeFilters.combat = (ECombat) block.readByte();

						if (version < 8)
						{
							activeFilters.cheats = ECheats.ANY;
						}
						else
						{
							activeFilters.cheats = (ECheats) block.readByte();
						}

						if (version < SAVEDATA_VERSION_ADDED_GOLD_FILTER)
						{
							// easy/normal/hard filter
							block.readByte();
						}

						if (version > 3)
						{
							activeFilters.camera = (ECameraMode) block.readByte();
						}
						else
						{
							activeFilters.camera = ECameraMode.ANY;
						}

						if (version >= 13)
						{
							activeFilters.monetization = (EServerMonetizationTag) block.readByte();
						}
						else
						{
							activeFilters.monetization = EServerMonetizationTag.Any;
						}

						if (version >= SAVEDATA_VERSION_ADDED_GOLD_FILTER)
						{
							activeFilters.gold = (EServerListGoldFilter) block.readByte();
						}
						else
						{
							activeFilters.gold = EServerListGoldFilter.Any;
						}

						if (version >= SAVEDATA_VERSION_MOVED_SERVER_NAME_FILTER)
						{
							activeFilters.serverName = block.readString();
						}
						else
						{
							activeFilters.serverName = string.Empty;
						}

						if (version >= SAVEDATA_VERSION_ADDED_PRESETS)
						{
							activeFilters.listSource = (ESteamServerList) block.readByte();
							activeFilters.presetName = block.readString();
							activeFilters.presetId = block.readInt32();
						}
						else
						{
							activeFilters.listSource = ESteamServerList.INTERNET;
							activeFilters.presetName = string.Empty;
							activeFilters.presetId = -1;
						}

						if (version >= SAVEDATA_VERSION_MAX_PING)
						{
							activeFilters.maxPing = block.readInt32();
							if (version < SAVEDATA_VERSION_INCREASED_DEFAULT_MAX_PING && activeFilters.maxPing == 200)
							{
								activeFilters.maxPing = DEFAULT_MAX_PING;
							}
						}
						else
						{
							activeFilters.maxPing = DEFAULT_MAX_PING;
						}

						if (version >= SAVEDATA_VERSION_ADDED_PRESETS)
						{
							nextCustomPresetId = block.readInt32();

							int presetCount = block.readInt32();
							for (int presetIndex = 0; presetIndex < presetCount; ++presetIndex)
							{
								ServerListFilters preset = new ServerListFilters();
								preset.Read(version, block);
								customPresets.Add(preset);
							}
						}
						else
						{
							nextCustomPresetId = 1;
						}

						if (version >= SAVEDATA_VERSION_SAVE_COLUMNS)
						{
							columns.Read(version, block);
						}

						if (version >= SAVEDATA_VERSION_SAVE_SUBMENUS_OPEN)
						{
							isColumnsEditorOpen = block.readBoolean();
							isPresetsListOpen = block.readBoolean();
							isQuickFiltersEditorOpen = block.readBoolean();
						}
						else
						{
							isColumnsEditorOpen = false;
							isPresetsListOpen = true;
							isQuickFiltersEditorOpen = false;
						}

						if (version >= SAVEDATA_VERSION_FILTER_VISIBILITY)
						{
							isQuickFiltersVisibilityEditorOpen = block.readBoolean();
							filterVisibility.Read(version, block);
						}
						else
						{
							isQuickFiltersVisibilityEditorOpen = false;
						}

#if CLOUDDEBUG
						UnturnedLog.info("Filters: " + filterMap + " " + filterDedicated + " " + filterPassword + " " + filterAttendance + " " + filterSecurity + " " + filterMode);
#endif

						return;
					}
				}
			}

			isPresetsListOpen = true;
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION_NEWEST);

			activeFilters.Write(block);
			block.writeInt32(nextCustomPresetId);

			block.writeInt32(customPresets.Count);
			foreach (ServerListFilters preset in customPresets)
			{
				preset.Write(block);
			}

			columns.Write(block);

			block.writeBoolean(isColumnsEditorOpen);
			block.writeBoolean(isPresetsListOpen);
			block.writeBoolean(isQuickFiltersEditorOpen);

			block.writeBoolean(isQuickFiltersVisibilityEditorOpen);
			filterVisibility.Write(block);

			ReadWrite.writeBlock("/Filters.dat", true, block);
		}

		static FilterSettings()
		{
			defaultPresetInternet.presetId = -2;

			defaultPresetLAN.presetId = -3;
			defaultPresetLAN.listSource = ESteamServerList.LAN;
			defaultPresetLAN.password = EPassword.ANY;
			defaultPresetLAN.vacProtection = EVACProtectionFilter.Any;
#if WITH_THIRDPARTYAC
			defaultPresetLAN.thirdpartyAntiCheatProtection = EThirdpartyAntiCheatProtectionFilter.Any;
#endif
			defaultPresetLAN.maxPing = 0;

			defaultPresetHistory.presetId = -4;
			defaultPresetHistory.listSource = ESteamServerList.HISTORY;
			defaultPresetHistory.password = EPassword.ANY;
			defaultPresetHistory.vacProtection = EVACProtectionFilter.Any;
#if WITH_THIRDPARTYAC
			defaultPresetHistory.thirdpartyAntiCheatProtection = EThirdpartyAntiCheatProtectionFilter.Any;
#endif
			defaultPresetHistory.maxPing = 0;

			defaultPresetFavorites.presetId = -5;
			defaultPresetFavorites.listSource = ESteamServerList.FAVORITES;
			defaultPresetFavorites.password = EPassword.ANY;
			defaultPresetFavorites.vacProtection = EVACProtectionFilter.Any;
#if WITH_THIRDPARTYAC
			defaultPresetFavorites.thirdpartyAntiCheatProtection = EThirdpartyAntiCheatProtectionFilter.Any;
#endif
			defaultPresetFavorites.maxPing = 0;

			defaultPresetFriends.presetId = -6;
			defaultPresetFriends.listSource = ESteamServerList.FRIENDS;
			defaultPresetFriends.password = EPassword.ANY;
			defaultPresetFriends.vacProtection = EVACProtectionFilter.Any;
#if WITH_THIRDPARTYAC
			defaultPresetFriends.thirdpartyAntiCheatProtection = EThirdpartyAntiCheatProtectionFilter.Any;
#endif
			defaultPresetFriends.maxPing = 0;
		}
	}
}
