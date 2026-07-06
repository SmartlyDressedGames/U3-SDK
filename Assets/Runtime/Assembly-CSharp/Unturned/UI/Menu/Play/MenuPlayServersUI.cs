////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuPlayServersUI : SleekFullscreenBox
	{
		public Local localization;
		public IconsBundle icons;
		/// <summary>
		/// Contains presetsScrollView which contains customPresetsContainer and defaultPresetsContainer.
		/// </summary>
		private ISleekElement presetsContainer;
		private ISleekScrollView presetsScrollView;
		private ISleekElement customPresetsContainer;
		private ISleekElement defaultPresetsContainer;
		private ISleekElement columnTogglesContainer;
		private ISleekElement filtersEditorContainer;
		/// <summary>
		/// Contains column buttons and server list itself.
		/// </summary>
		private ISleekElement mainListContainer;
		public bool active;

		private SleekButtonIcon backButton;

		private SleekList<SteamServerAdvertisement> serverBox;
		private ISleekBox infoBox;
		private ISleekLabel noServersCuratorsHintLabel;
		private ISleekButton resetFiltersButton;

		private ISleekButton nameColumnButton;
		private ISleekButton mapColumnButton;
		private ISleekButton playersColumnButton;
		private ISleekButton maxPlayersColumnButton;
		private ISleekButton fullnessColumnButton;
		private ISleekButton pingColumnButton;
		private ISleekButton anticheatColumnButton;
		private SleekButtonIcon perspectiveColumnButton;
		private SleekButtonIcon combatColumnButton;
		private SleekButtonIcon passwordColumnButton;
		private SleekButtonIcon workshopColumnButton;
		private SleekButtonIcon goldColumnButton;
		private SleekButtonIcon cheatsColumnButton;
		private SleekButtonIcon monetizationColumnButton;
		private SleekButtonIcon pluginsColumnButton;

		private ISleekField nameField;
		private SleekButtonIcon mapButton;
		private SleekButtonState monetizationButtonState;
		private SleekButtonState passwordButtonState;
		private SleekButtonState workshopButtonState;
		private SleekButtonState pluginsButtonState;
		private SleekButtonState cheatsButtonState;
		private SleekButtonState attendanceButtonState;
		private SleekButtonState notFullButtonState;
		private SleekButtonState VACProtectionButtonState;
#if WITH_THIRDPARTYAC
		private SleekButtonState thirdpartyAntiCheatButtonState;
#endif
		private SleekButtonState combatButtonState;
		private SleekButtonState goldFilterButtonState;
		private SleekButtonState cameraButtonState;
		private SleekButtonState listSourceButtonState;
		private ISleekInt32Field maxPingField;
		private SleekButtonIcon filtersVisibilityButton;
		private ISleekButton openFiltersVisibilityButton;
		private ISleekButton closeFiltersVisibilityButton;

		private ISleekToggle listSourceToggle;
		private ISleekToggle nameToggle;
		private ISleekToggle mapToggle;
		private ISleekToggle passwordToggle;
		private ISleekToggle attendanceToggle;
		private ISleekToggle notFullToggle;
		private ISleekToggle combatToggle;
		private ISleekToggle cameraToggle;
		private ISleekToggle goldToggle;
		private ISleekToggle monetizationToggle;
		private ISleekToggle workshopToggle;
		private ISleekToggle pluginsToggle;
		private ISleekToggle cheatsToggle;
		private ISleekToggle vacToggle;
#if WITH_THIRDPARTYAC
		private ISleekToggle thirdpartyAntiCheatToggle;
#endif
		private ISleekToggle maxPingToggle;

		private ISleekButton refreshButton;
		private SleekButtonIcon presetsButton;
		private SleekButtonIcon quickFiltersButton;
		private ISleekImage refreshIcon;

		private SleekButtonIcon presetsEditorButton;

		private bool isRefreshing = false;

		public void open(bool shouldRefresh)
		{
			if (active)
			{
				return;
			}

			active = true;

			// Synchronize in case they were modified in the filters UI.
			SynchronizeFilterButtons();

			// 0 indicates the player has never used the server browser before.
			if (FilterSettings.activeFilters.presetId == 0)
			{
				FilterSettings.activeFilters.CopyFrom(FilterSettings.defaultPresetInternet);
				FilterSettings.activeFilters.presetName = localization.format("DefaultPreset_Internet_Label");
				FilterSettings.InvokeActiveFiltersReplaced();
			}
			else
			{
				if (shouldRefresh)
				{
					CancelAndRefresh();
				}
			}

			AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			AnimateOutOfView(0, 1);
		}

		private void onClickedServer(SleekServer server, SteamServerAdvertisement info)
		{
			if (info.isPro && !Provider.isPro)
			{
				return;
			}

			MenuPlayServerInfoUI.open(info, string.Empty, MenuPlayServerInfoUI.EServerInfoOpenContext.SERVERS);
			close();
		}

		private void onMasterServerAdded(int insert, SteamServerAdvertisement info)
		{
			serverBox.NotifyDataChanged();
		}

		private void onMasterServerRemoved()
		{
			infoBox.IsVisible = false;
			serverBox.NotifyDataChanged();
		}

		private void onMasterServerResorted()
		{
			infoBox.IsVisible = false;
			serverBox.NotifyDataChanged();
		}

		private void onMasterServerRefreshed(EMatchMakingServerResponse response)
		{
			SetIsRefreshing(false);

			if (Provider.provider.matchmakingService.serverList.Count == 0)
			{
				int blockedCount = Provider.provider.matchmakingService.CuratorBlockedServerCount;
				if (blockedCount > 10)
				{
					noServersCuratorsHintLabel.Text = localization.format("No_Servers_CuratorsHint", blockedCount);
					noServersCuratorsHintLabel.IsVisible = true;
					infoBox.SizeOffset_Y = 70.0f;
				}
				else
				{
					noServersCuratorsHintLabel.IsVisible = false;
					infoBox.SizeOffset_Y = 50.0f;
				}
				
				infoBox.IsVisible = true;
			}
		}

		private void CancelAndRefresh()
		{
			if (isRefreshing)
			{
				Provider.provider.matchmakingService.cancelRequest();
			}

			SetIsRefreshing(true);
			Provider.provider.matchmakingService.refreshMasterServer(FilterSettings.activeFilters);
		}

		private void OnActiveFiltersModified()
		{
			SynchronizePresetsEditorButtonLabel(); // Update " (Modified)" suffix.
		}

		private void OnActiveFiltersReplaced()
		{
			SynchronizeFilterButtons();
			SynchronizePresetsEditorButtonLabel();

			// If the server list menu is open, proceed with refreshing using new filters.
			if (active)
			{
				CancelAndRefresh();
			}
		}

		private void OnCustomPresetsListChanged()
		{
			SynchronizePresetsList();
		}

		private void OnNameColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_NameAscending))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_NameDescending());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_NameAscending());
			}
		}

		private void OnMapColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_MapAscending))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MapDescending());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MapAscending());
			}
		}

		private void OnPlayersColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PlayersDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PlayersInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PlayersDefault());
			}
		}

		private void OnMaxPlayersColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_MaxPlayersDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MaxPlayersInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MaxPlayersDefault());
			}
		}

		private void OnFullnessColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_FullnessDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_FullnessInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_FullnessDefault());
			}
		}

		private void OnPingColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PingAscending))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PingDescending());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PingAscending());
			}
		}

		private void OnAnticheatColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_AnticheatDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_AnticheatInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_AnticheatDefault());
			}
		}

		private void OnPerspectiveColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PerspectiveDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PerspectiveInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PerspectiveDefault());
			}
		}

		private void OnCombatColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_CombatDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CombatInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CombatDefault());
			}
		}

		private void OnPasswordColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PasswordDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PasswordInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PasswordDefault());
			}
		}

		private void OnWorkshopColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_WorkshopDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_WorkshopInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_WorkshopDefault());
			}
		}

		private void OnGoldColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_GoldDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_GoldInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_GoldDefault());
			}
		}

		private void OnCheatsColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_CheatsDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CheatsInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CheatsDefault());
			}
		}

		private void OnMonetizationColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_MonetizationDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MonetizationInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MonetizationDefault());
			}
		}

		private void OnPluginsColumnClicked(ISleekElement button)
		{
			if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PluginsDefault))
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PluginsInverted());
			}
			else
			{
				Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PluginsDefault());
			}
		}

		private void OnClickedColumnsButton(ISleekElement button)
		{
			FilterSettings.isColumnsEditorOpen = !FilterSettings.isColumnsEditorOpen;
			AnimateOpenSubcontainers();
		}

		private void OnClickedFiltersVisibilityButton(ISleekElement button)
		{
			FilterSettings.isQuickFiltersVisibilityEditorOpen = !FilterSettings.isQuickFiltersVisibilityEditorOpen
				&& FilterSettings.isQuickFiltersEditorOpen;
			SynchronizeVisibleFilters();
			AnimateOpenSubcontainers();
		}

		private void OnClickedOpenFiltersVisibilityButton(ISleekElement button)
		{
			FilterSettings.isQuickFiltersVisibilityEditorOpen = FilterSettings.isQuickFiltersEditorOpen;
			SynchronizeVisibleFilters();
			AnimateOpenSubcontainers();
		}

		private void OnClickedCloseFiltersVisibilityButton(ISleekElement button)
		{
			FilterSettings.isQuickFiltersVisibilityEditorOpen = false;
			SynchronizeVisibleFilters();
			AnimateOpenSubcontainers();
		}

		private void OnMapColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.map = value;
			SynchronizeVisibleColumns();
		}

		private void OnPlayersColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.players = value;
			SynchronizeVisibleColumns();
		}

		private void OnMaxPlayersColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.maxPlayers = value;
			SynchronizeVisibleColumns();
		}

		private void OnPingColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.ping = value;
			SynchronizeVisibleColumns();
		}

		private void OnAnticheatColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.anticheat = value;
			SynchronizeVisibleColumns();
		}

		private void OnPerspectiveColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.perspective = value;
			SynchronizeVisibleColumns();
		}

		private void OnCombatColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.combat = value;
			SynchronizeVisibleColumns();
		}

		private void OnPasswordColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.password = value;
			SynchronizeVisibleColumns();
		}

		private void OnWorkshopColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.workshop = value;
			SynchronizeVisibleColumns();
		}

		private void OnGoldColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.gold = value;
			SynchronizeVisibleColumns();
		}

		private void OnCheatsColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.cheats = value;
			SynchronizeVisibleColumns();
		}

		private void OnMonetizationColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.monetization = value;
			SynchronizeVisibleColumns();
		}

		private void OnPluginsColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.plugins = value;
			SynchronizeVisibleColumns();
		}

		private void OnFullnessColumnToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.columns.fullnessPercentage = value;
			SynchronizeVisibleColumns();
		}

		private void OnListSourceFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.listSource = value;
			SynchronizeVisibleFilters();
		}

		private void OnNameFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.name = value;
			SynchronizeVisibleFilters();
		}

		private void OnMapFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.map = value;
			SynchronizeVisibleFilters();
		}

		private void OnPasswordFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.password = value;
			SynchronizeVisibleFilters();
		}

		private void OnAttendanceFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.attendance = value;
			SynchronizeVisibleFilters();
		}

		private void OnSpaceFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.notFull = value;
			SynchronizeVisibleFilters();
		}

		private void OnCombatFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.combat = value;
			SynchronizeVisibleFilters();
		}

		private void OnCameraFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.camera = value;
			SynchronizeVisibleFilters();
		}

		private void OnGoldFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.gold = value;
			SynchronizeVisibleFilters();
		}

		private void OnMonetizationFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.monetization = value;
			SynchronizeVisibleFilters();
		}

		private void OnWorkshopFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.workshop = value;
			SynchronizeVisibleFilters();
		}

		private void OnPluginsFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.plugins = value;
			SynchronizeVisibleFilters();
		}

		private void OnCheatsFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.cheats = value;
			SynchronizeVisibleFilters();
		}

		private void OnVACFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.vacProtection = value;
			SynchronizeVisibleFilters();
		}

#if WITH_THIRDPARTYAC
		private void OnThirdpartyAntiCheatFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.thirdpartyAntiCheatProtection = value;
			SynchronizeVisibleFilters();
		}
#endif // WITH_THIRDPARTYAC

		private void OnMaxPingFilterToggled(ISleekToggle toggle, bool value)
		{
			FilterSettings.filterVisibility.maxPing = value;
			SynchronizeVisibleFilters();
		}

		private void onTypedNameField(ISleekField field, string text)
		{
			FilterSettings.activeFilters.serverName = text;
			FilterSettings.MarkActiveFilterModified();
		}

		private void OnMaxPingChanged(ISleekInt32Field field, int value)
		{
			FilterSettings.activeFilters.maxPing = value;
			FilterSettings.MarkActiveFilterModified();
		}

		private void OnNameSubmitted(ISleekField field)
		{
			CancelAndRefresh();
		}

		private void OnClickedMapButton(ISleekElement button)
		{
			mapFiltersUI.open(EMenuPlayMapFiltersUIOpenContext.ServerList);
			close();
		}

		private void onSwappedMonetizationState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.monetization = (EServerMonetizationTag) (index + 1);
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedPasswordState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.password = (EPassword) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedWorkshopState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.workshop = (EWorkshop) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedPluginsState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.plugins = (EPlugins) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedCheatsState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.cheats = (ECheats) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedAttendanceState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.attendance = (EAttendance) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void OnSwappedNotFullState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.notFull = index > 0;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedVACProtectionState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.vacProtection = (EVACProtectionFilter) index;
			FilterSettings.MarkActiveFilterModified();
		}

#if WITH_THIRDPARTYAC
		private void onSwappedThirdpartyAntiCheatProtectionState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.thirdpartyAntiCheatProtection = (EThirdpartyAntiCheatProtectionFilter) index;
			FilterSettings.MarkActiveFilterModified();
		}
#endif

		private void onSwappedCombatState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.combat = (ECombat) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void OnSwappedGoldFilterState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.gold = (EServerListGoldFilter) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onSwappedCameraState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.camera = (ECameraMode) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void OnSwappedListSourceState(SleekButtonState button, int index)
		{
			FilterSettings.activeFilters.listSource = (ESteamServerList) index;
			FilterSettings.MarkActiveFilterModified();
		}

		private void onClickedRefreshButton(ISleekElement button)
		{
			if (isRefreshing)
			{
				SetIsRefreshing(false);
				Provider.provider.matchmakingService.cancelRequest();
			}
			else
			{
				SetIsRefreshing(true);
				Provider.provider.matchmakingService.refreshMasterServer(FilterSettings.activeFilters);
			}
		}

		private void OnClickedCurationButton(ISleekElement button)
		{
			serverCurationUI.open();
			close();
		}

		private void OnPresetsEditorButtonClicked(ISleekElement button)
		{
			serverListFiltersUI.open();
			close();
		}

		private void SynchronizePresetsButtonLabel()
		{
			if (FilterSettings.isPresetsListOpen)
			{
				presetsButton.text = localization.format("ViewPresetsButton_Close_Label");
			}
			else
			{
				presetsButton.text = localization.format("ViewPresetsButton_Open_Label");
			}
		}

		private void SynchronizeQuickFiltersButtonLabel()
		{
			if (FilterSettings.isQuickFiltersEditorOpen)
			{
				quickFiltersButton.text = localization.format("QuickFiltersButton_Close_Label");
			}
			else
			{
				quickFiltersButton.text = localization.format("QuickFiltersButton_Open_Label");
			}
		}

		private void onClickedPresetsButton(ISleekElement button)
		{
			FilterSettings.isPresetsListOpen = !FilterSettings.isPresetsListOpen;
			SynchronizePresetsButtonLabel();
			AnimateOpenSubcontainers();
		}

		private void OnQuickFiltersButtonClicked(ISleekElement button)
		{
			FilterSettings.isQuickFiltersEditorOpen = !FilterSettings.isQuickFiltersEditorOpen;
			FilterSettings.isQuickFiltersVisibilityEditorOpen &= FilterSettings.isQuickFiltersEditorOpen;
			SynchronizeQuickFiltersButtonLabel();
			SynchronizeVisibleFilters();
			AnimateOpenSubcontainers();
		}

		private ISleekElement onCreateServerElement(SteamServerAdvertisement server)
		{
			SleekServer element = new SleekServer(Provider.provider.matchmakingService.currentList, server);
			element.onClickedServer = onClickedServer;
			element.SizeOffset_X = -30;
			return element;
		}

		/// <summary>
		/// Synchronize widgets with their values.
		/// </summary>
		private void SynchronizeFilterButtons()
		{
			nameField.Text = FilterSettings.activeFilters.serverName;

			string mapDisplayText = FilterSettings.activeFilters.GetMapDisplayText();
			if (string.IsNullOrEmpty(mapDisplayText))
			{
				mapButton.text = localization.format("MapFilter_Button_EmptyLabel");
			}
			else
			{
				mapButton.text = mapDisplayText;
			}

			passwordButtonState.state = (int) FilterSettings.activeFilters.password;
			workshopButtonState.state = (int) FilterSettings.activeFilters.workshop;
			pluginsButtonState.state = (int) FilterSettings.activeFilters.plugins;
			cheatsButtonState.state = (int) FilterSettings.activeFilters.cheats;
			attendanceButtonState.state = (int) FilterSettings.activeFilters.attendance;
			notFullButtonState.state = FilterSettings.activeFilters.notFull ? 1 : 0;
			VACProtectionButtonState.state = (int) FilterSettings.activeFilters.vacProtection;
#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatButtonState.state = (int) FilterSettings.activeFilters.thirdpartyAntiCheatProtection;
#endif
			combatButtonState.state = (int) FilterSettings.activeFilters.combat;
			goldFilterButtonState.state = (int) FilterSettings.activeFilters.gold;
			cameraButtonState.state = (int) FilterSettings.activeFilters.camera;
			monetizationButtonState.state = ((int) FilterSettings.activeFilters.monetization) - 1;
			listSourceButtonState.state = (int) FilterSettings.activeFilters.listSource;
			maxPingField.Value = FilterSettings.activeFilters.maxPing;
		}

		private void SynchronizePresetsEditorButtonLabel()
		{
			string presetName = FilterSettings.activeFilters.presetName;
			if (string.IsNullOrEmpty(presetName))
			{
				presetName = localization.format("PresetName_Empty");
			}
			presetsEditorButton.text = localization.format("PresetsEditorButton_Label", presetName);
		}

		private void SynchronizePresetsList()
		{
			customPresetsContainer.RemoveAllChildren();

			const int customPresetsBeforeLineWrap = 5;
			int customPresetIndex = 0;
			const float presetHorizontalSpacing = 0.2f;
			const float presetVerticalSpacing = 30.0f;

			foreach (ServerListFilters preset in FilterSettings.customPresets)
			{
				SleekCustomServerListPresetButton presetButton = new SleekCustomServerListPresetButton(preset);
				presetButton.PositionScale_X = (customPresetIndex % customPresetsBeforeLineWrap) * presetHorizontalSpacing;
				presetButton.PositionOffset_Y = (customPresetIndex / customPresetsBeforeLineWrap) * presetVerticalSpacing;
				presetButton.SizeScale_X = presetHorizontalSpacing;
				presetButton.SizeOffset_Y = presetVerticalSpacing;
				customPresetsContainer.AddChild(presetButton);
				++customPresetIndex;
			}

			customPresetsContainer.SizeOffset_Y = (((customPresetIndex - 1) / customPresetsBeforeLineWrap) + 1) * presetVerticalSpacing;
			if (customPresetIndex > 0)
			{
				defaultPresetsContainer.PositionOffset_Y = customPresetsContainer.SizeOffset_Y + 10;
			}
			else
			{
				defaultPresetsContainer.PositionOffset_Y = 0;
			}

			float height = defaultPresetsContainer.PositionOffset_Y + defaultPresetsContainer.SizeOffset_Y;

			presetsScrollView.ContentSizeOffset = new Vector2(0.0f, height);
			presetsScrollView.SizeOffset_Y = Mathf.Min(height, 100.0f);
			presetsContainer.SizeOffset_Y = presetsScrollView.SizeOffset_Y + 20.0f;
			presetsContainer.PositionOffset_Y = -presetsContainer.SizeOffset_Y - 70.0f;

			AnimateOpenSubcontainers();
		}

		private void CreateQuickFilterButtons()
		{
			const float filterHorizontalSpacing = 0.2f;
			const float filterVerticalSpacing = 30.0f;

			listSourceButtonState = new SleekButtonState(20, new GUIContent(localization.format("List_Internet_Label"), icons.load<Texture>("List_Internet"), localization.format("List_Internet_Tooltip")),
				new GUIContent(localization.format("List_LAN_Label"), icons.load<Texture>("List_LAN"), localization.format("List_LAN_Tooltip")),
				new GUIContent(localization.format("List_History_Label"), icons.load<Texture>("List_History"), localization.format("List_History_Tooltip")),
				new GUIContent(localization.format("List_Favorites_Label"), icons.load<Texture>("List_Favorites"), localization.format("List_Favorites_Tooltip")),
				new GUIContent(localization.format("List_Friends_Label"), icons.load<Texture2D>("List_Friends"), localization.format("List_Friends_Tooltip")));
			listSourceButtonState.SizeScale_X = filterHorizontalSpacing;
			listSourceButtonState.SizeOffset_Y = filterVerticalSpacing;
			listSourceButtonState.onSwappedState = OnSwappedListSourceState;
			listSourceButtonState.button.iconColor = ESleekTint.FOREGROUND;
			listSourceButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(listSourceButtonState);

			nameField = Glazier.Get().CreateStringField();
			nameField.SizeScale_X = filterHorizontalSpacing;
			nameField.SizeOffset_Y = filterVerticalSpacing;
			nameField.PlaceholderText = localization.format("Name_Filter_Hint");
			nameField.TooltipText = localization.format("Name_Filter_Tooltip");
			nameField.OnTextChanged += onTypedNameField;
			nameField.OnTextSubmitted += OnNameSubmitted;
			filtersEditorContainer.AddChild(nameField);

			mapButton = new SleekButtonIcon(icons.load<Texture2D>("Map"), 20);
			mapButton.SizeScale_X = filterHorizontalSpacing;
			mapButton.SizeOffset_Y = filterVerticalSpacing;
			mapButton.tooltip = localization.format("MapFilter_Button_Tooltip");
			mapButton.onClickedButton += OnClickedMapButton;
			mapButton.iconColor = ESleekTint.FOREGROUND;
			filtersEditorContainer.AddChild(mapButton);

			passwordButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Password_Button"), icons.load<Texture2D>("NotPasswordProtected"), localization.format("Password_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Password_Button"), icons.load<Texture2D>("PasswordProtected"), localization.format("Password_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Password_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Password_Filter_Any_Tooltip")));
			passwordButtonState.SizeScale_X = filterHorizontalSpacing;
			passwordButtonState.SizeOffset_Y = filterVerticalSpacing;
			passwordButtonState.onSwappedState = onSwappedPasswordState;
			passwordButtonState.button.iconColor = ESleekTint.FOREGROUND;
			passwordButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(passwordButtonState);

			attendanceButtonState = new SleekButtonState(20, new GUIContent(localization.format("Empty_Button"), icons.load<Texture>("Empty"), localization.format("Attendance_Filter_Empty_Tooltip")),
				new GUIContent(localization.format("HasPlayers_Button"), icons.load<Texture>("HasPlayers"), localization.format("Attendance_Filter_HasPlayers_Tooltip")),
				new GUIContent(localization.format("Any_Attendance_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Attendance_Filter_Any_Tooltip")));
			attendanceButtonState.SizeScale_X = filterHorizontalSpacing;
			attendanceButtonState.SizeOffset_Y = filterVerticalSpacing;
			attendanceButtonState.onSwappedState = onSwappedAttendanceState;
			attendanceButtonState.button.iconColor = ESleekTint.FOREGROUND;
			attendanceButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(attendanceButtonState);

			notFullButtonState = new SleekButtonState(20, new GUIContent(localization.format("Any_Space_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Space_Filter_Any_Tooltip")),
				new GUIContent(localization.format("Space_Button"), icons.load<Texture>("Space"), localization.format("Space_Filter_HasSpace_Tooltip")));
			notFullButtonState.SizeScale_X = filterHorizontalSpacing;
			notFullButtonState.SizeOffset_Y = filterVerticalSpacing;
			notFullButtonState.onSwappedState = OnSwappedNotFullState;
			notFullButtonState.button.iconColor = ESleekTint.FOREGROUND;
			notFullButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(notFullButtonState);

			combatButtonState = new SleekButtonState(20, new GUIContent(localization.format("PvP_Button"), icons.load<Texture>("PvP"), localization.format("Combat_Filter_PvP_Tooltip")),
				new GUIContent(localization.format("PvE_Button"), icons.load<Texture>("PvE"), localization.format("Combat_Filter_PvE_Tooltip")),
				new GUIContent(localization.format("Any_Combat_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Combat_Filter_Any_Tooltip")));
			combatButtonState.SizeScale_X = filterHorizontalSpacing;
			combatButtonState.SizeOffset_Y = filterVerticalSpacing;
			combatButtonState.onSwappedState = onSwappedCombatState;
			combatButtonState.button.iconColor = ESleekTint.FOREGROUND;
			combatButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(combatButtonState);

			cameraButtonState = new SleekButtonState(20, new GUIContent(localization.format("First_Button"), icons.load<Texture>("Perspective_FirstPerson"), localization.format("Perspective_Filter_FirstPerson_Tooltip")),
				new GUIContent(localization.format("Third_Button"), icons.load<Texture>("Perspective_ThirdPerson"), localization.format("Perspective_Filter_ThirdPerson_Tooltip")),
				new GUIContent(localization.format("Both_Button"), icons.load<Texture>("Perspective_Both"), localization.format("Perspective_Filter_Both_Tooltip")),
				new GUIContent(localization.format("Vehicle_Button"), icons.load<Texture>("Perspective_Vehicle"), localization.format("Perspective_Filter_Vehicle_Tooltip")),
				new GUIContent(localization.format("Any_Camera_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Perspective_Filter_Any_Tooltip")));
			cameraButtonState.SizeScale_X = filterHorizontalSpacing;
			cameraButtonState.SizeOffset_Y = filterVerticalSpacing;
			cameraButtonState.onSwappedState = onSwappedCameraState;
			cameraButtonState.button.iconColor = ESleekTint.FOREGROUND;
			cameraButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(cameraButtonState);

			goldFilterButtonState = new SleekButtonState(20, new GUIContent(localization.format("Gold_Filter_Any_Label"), icons.load<Texture>("AnyFilter"), localization.format("Gold_Filter_Any_Tooltip")),
				new GUIContent(localization.format("Gold_Filter_DoesNotRequireGold_Label"), icons.load<Texture>("GoldNotRequired"), localization.format("Gold_Filter_DoesNotRequireGold_Tooltip")),
				new GUIContent(localization.format("Gold_Filter_RequiresGold_Label"), icons.load<Texture>("GoldRequired"), localization.format("Gold_Filter_RequiresGold_Tooltip")));
			goldFilterButtonState.SizeScale_X = filterHorizontalSpacing;
			goldFilterButtonState.SizeOffset_Y = filterVerticalSpacing;
			goldFilterButtonState.UseContentTooltip = true;
			goldFilterButtonState.onSwappedState = OnSwappedGoldFilterState;
			goldFilterButtonState.button.textColor = Palette.PRO;
			goldFilterButtonState.button.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
			goldFilterButtonState.button.iconColor = Palette.PRO;
			filtersEditorContainer.AddChild(goldFilterButtonState);

			monetizationButtonState = new SleekButtonState(20, new GUIContent(localization.format("Monetization_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Monetization_Filter_Any_Tooltip")),
				new GUIContent(localization.format("Monetization_None_Button"), icons.load<Texture2D>("Monetization_None"), localization.format("Monetization_Filter_None_Tooltip")),
				new GUIContent(localization.format("Monetization_NonGameplay_Button"), icons.load<Texture2D>("NonGameplayMonetization"), localization.format("Monetization_Filter_NonGameplay_Tooltip")));
			monetizationButtonState.SizeScale_X = filterHorizontalSpacing;
			monetizationButtonState.SizeOffset_Y = filterVerticalSpacing;
			monetizationButtonState.onSwappedState = onSwappedMonetizationState;
			monetizationButtonState.button.iconColor = ESleekTint.FOREGROUND;
			monetizationButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(monetizationButtonState);

			workshopButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Workshop_Button"), icons.load<Texture2D>("NoMods"), localization.format("Workshop_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Workshop_Button"), icons.load<Texture2D>("HasMods"), localization.format("Workshop_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Workshop_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Workshop_Filter_Any_Tooltip")));
			workshopButtonState.SizeScale_X = filterHorizontalSpacing;
			workshopButtonState.SizeOffset_Y = filterVerticalSpacing;
			workshopButtonState.onSwappedState = onSwappedWorkshopState;
			workshopButtonState.button.iconColor = ESleekTint.FOREGROUND;
			workshopButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(workshopButtonState);

			pluginsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Plugins_Button"), icons.load<Texture2D>("Plugins_None"), localization.format("Plugins_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Plugins_Button"), icons.load<Texture2D>("Plugins"), localization.format("Plugins_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Plugins_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Plugins_Filter_Any_Tooltip")));
			pluginsButtonState.SizeScale_X = filterHorizontalSpacing;
			pluginsButtonState.SizeOffset_Y = filterVerticalSpacing;
			pluginsButtonState.onSwappedState = onSwappedPluginsState;
			pluginsButtonState.button.iconColor = ESleekTint.FOREGROUND;
			pluginsButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(pluginsButtonState);

			cheatsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Cheats_Button"), icons.load<Texture2D>("CheatCodes_None"), localization.format("Cheats_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Cheats_Button"), icons.load<Texture2D>("CheatCodes"), localization.format("Cheats_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Cheats_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Cheats_Filter_Any_Tooltip")));
			cheatsButtonState.SizeScale_X = filterHorizontalSpacing;
			cheatsButtonState.SizeOffset_Y = filterVerticalSpacing;
			cheatsButtonState.onSwappedState = onSwappedCheatsState;
			cheatsButtonState.button.iconColor = ESleekTint.FOREGROUND;
			cheatsButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(cheatsButtonState);

			VACProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format("VAC_Secure_Button"), icons.load<Texture>("VAC"), localization.format("VAC_Filter_Secure_Tooltip")),
				new GUIContent(localization.format("VAC_Insecure_Button"), icons.load<Texture2D>("VAC_Off"), localization.format("VAC_Filter_Insecure_Tooltip")),
				new GUIContent(localization.format("VAC_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("VAC_Filter_Any_Tooltip")));
			VACProtectionButtonState.SizeScale_X = filterHorizontalSpacing;
			VACProtectionButtonState.SizeOffset_Y = filterVerticalSpacing;
			VACProtectionButtonState.onSwappedState = onSwappedVACProtectionState;
			VACProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
			VACProtectionButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(VACProtectionButtonState);

#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatButtonState = new SleekButtonState(20, new GUIContent(localization.format(ThirdpartyAntiCheat.FilterSecureKey), icons.load<Texture>(ThirdpartyAntiCheat.IconName), localization.format(ThirdpartyAntiCheat.FilterSecureTooltipKey)),
				new GUIContent(localization.format(ThirdpartyAntiCheat.FilterInsecureKey), icons.load<Texture2D>(ThirdpartyAntiCheat.IconInsecureName), localization.format(ThirdpartyAntiCheat.FilterInsecureTooltipKey)),
				new GUIContent(localization.format(ThirdpartyAntiCheat.FilterAnyKey), icons.load<Texture2D>("AnyFilter"), localization.format(ThirdpartyAntiCheat.FilterAnyTooltipKey)));
			thirdpartyAntiCheatButtonState.SizeScale_X = filterHorizontalSpacing;
			thirdpartyAntiCheatButtonState.SizeOffset_Y = filterVerticalSpacing;
			thirdpartyAntiCheatButtonState.onSwappedState = onSwappedThirdpartyAntiCheatProtectionState;
			thirdpartyAntiCheatButtonState.button.iconColor = ESleekTint.FOREGROUND;
			thirdpartyAntiCheatButtonState.UseContentTooltip = true;
			filtersEditorContainer.AddChild(thirdpartyAntiCheatButtonState);
#endif

			maxPingField = Glazier.Get().CreateInt32Field();
			maxPingField.SizeScale_X = filterHorizontalSpacing;
			maxPingField.SizeOffset_Y = filterVerticalSpacing;
			maxPingField.TooltipText = localization.format("MaxPing_Filter_Tooltip");
			maxPingField.OnValueChanged += OnMaxPingChanged;
			filtersEditorContainer.AddChild(maxPingField);

// 			filtersVisibilityButton = new SleekButtonIcon(icons.load<Texture2D>("FilterVisibility"), 20);
// 			filtersVisibilityButton.PositionScale_X = 0.5f - (filterHorizontalSpacing * 0.5f);
// 			filtersVisibilityButton.SizeScale_X = filterHorizontalSpacing;
// 			filtersVisibilityButton.SizeOffset_Y = filterVerticalSpacing;
// 			filtersVisibilityButton.onClickedButton += OnClickedFiltersVisibilityButton;
// 			filtersVisibilityButton.iconColor = ESleekTint.FOREGROUND;
// 			filtersVisibilityButton.tooltip = localization.format("QuickFiltersVisibilityButton_Tooltip");
// 			filtersEditorContainer.AddChild(filtersVisibilityButton);

			openFiltersVisibilityButton = Glazier.Get().CreateButton();
			openFiltersVisibilityButton.PositionScale_X = 0.5f;
			openFiltersVisibilityButton.PositionOffset_X = -50.0f;
			openFiltersVisibilityButton.PositionOffset_Y = 2.0f;
			openFiltersVisibilityButton.SizeOffset_X = 100.0f;
			openFiltersVisibilityButton.SizeOffset_Y = 16.0f;
			openFiltersVisibilityButton.OnClicked += OnClickedOpenFiltersVisibilityButton;
			openFiltersVisibilityButton.TooltipText = localization.format("QuickFiltersVisibilityButton_Open_Label");
			filtersEditorContainer.AddChild(openFiltersVisibilityButton);

			Texture2D openIconTexture = icons.load<Texture2D>("FilterVisibility_Open");
			Texture2D closeIconTexture = icons.load<Texture2D>("FilterVisibility_Close");
			//Texture2D hintIcon = icons.load<Texture2D>("FilterVisibility_Hint");

			ISleekImage openIcon = Glazier.Get().CreateImage(openIconTexture);
			openIcon.PositionOffset_X = -8;
			openIcon.PositionScale_X = 0.5f;
			openIcon.SizeOffset_X = 16;
			openIcon.SizeOffset_Y = 16;
			openIcon.TintColor = ESleekTint.FOREGROUND;
			openFiltersVisibilityButton.AddChild(openIcon);

			// 			ISleekImage openIcon1 = Glazier.Get().CreateImage(openIconTexture);
			// 			openIcon1.PositionScale_X = 0.25f;
			// 			openIcon1.PositionOffset_X = -8;
			// 			openIcon1.SizeOffset_X = 16;
			// 			openIcon1.SizeOffset_Y = 16;
			// 			openIcon1.TintColor = ESleekTint.FOREGROUND;
			// 			openFiltersVisibilityButton.AddChild(openIcon1);
			// 
			// 			ISleekImage openIcon2 = Glazier.Get().CreateImage(openIconTexture);
			// 			openIcon2.PositionOffset_X = -8;
			// 			openIcon2.PositionScale_X = 0.75f;
			// 			openIcon2.SizeOffset_X = 16;
			// 			openIcon2.SizeOffset_Y = 16;
			// 			openIcon2.TintColor = ESleekTint.FOREGROUND;
			// 			openFiltersVisibilityButton.AddChild(openIcon2);

			// 			ISleekImage openHintIcon = Glazier.Get().CreateImage(hintIcon);
			// 			openHintIcon.PositionOffset_X = -8;
			// 			openHintIcon.PositionScale_X = 0.5f;
			// 			openHintIcon.SizeOffset_X = 16;
			// 			openHintIcon.SizeOffset_Y = 16;
			// 			openHintIcon.TintColor = ESleekTint.FOREGROUND;
			// 			openFiltersVisibilityButton.AddChild(openHintIcon);

			closeFiltersVisibilityButton = Glazier.Get().CreateButton();
			closeFiltersVisibilityButton.PositionScale_X = 0.5f;
			closeFiltersVisibilityButton.PositionOffset_X = -50.0f;
			closeFiltersVisibilityButton.PositionOffset_Y = 2.0f;
			closeFiltersVisibilityButton.SizeOffset_X = 100.0f;
			closeFiltersVisibilityButton.SizeOffset_Y = 16.0f;
			closeFiltersVisibilityButton.OnClicked += OnClickedCloseFiltersVisibilityButton;
			closeFiltersVisibilityButton.TooltipText = localization.format("QuickFiltersVisibilityButton_Close_Label");
			filtersEditorContainer.AddChild(closeFiltersVisibilityButton);

			ISleekImage closeIcon = Glazier.Get().CreateImage(closeIconTexture);
			closeIcon.PositionScale_X = 0.5f;
			closeIcon.PositionOffset_X = -8;
			closeIcon.SizeOffset_X = 16;
			closeIcon.SizeOffset_Y = 16;
			closeIcon.TintColor = ESleekTint.FOREGROUND;
			closeFiltersVisibilityButton.AddChild(closeIcon);

			// 			ISleekImage closeIcon1 = Glazier.Get().CreateImage(closeIconTexture);
			// 			closeIcon1.PositionScale_X = 0.25f;
			// 			closeIcon1.PositionOffset_X = -8;
			// 			closeIcon1.SizeOffset_X = 16;
			// 			closeIcon1.SizeOffset_Y = 16;
			// 			closeIcon1.TintColor = ESleekTint.FOREGROUND;
			// 			closeFiltersVisibilityButton.AddChild(closeIcon1);
			// 
			// 			ISleekImage closeIcon2 = Glazier.Get().CreateImage(closeIconTexture);
			// 			closeIcon2.PositionOffset_X = -8;
			// 			closeIcon2.PositionScale_X = 0.75f;
			// 			closeIcon2.SizeOffset_X = 16;
			// 			closeIcon2.SizeOffset_Y = 16;
			// 			closeIcon2.TintColor = ESleekTint.FOREGROUND;
			// 			closeFiltersVisibilityButton.AddChild(closeIcon2);

			// 			ISleekImage closeHintIcon = Glazier.Get().CreateImage(hintIcon);
			// 			closeHintIcon.PositionOffset_X = -8;
			// 			closeHintIcon.PositionScale_X = 0.5f;
			// 			closeHintIcon.SizeOffset_X = 16;
			// 			closeHintIcon.SizeOffset_Y = 16;
			// 			closeHintIcon.TintColor = ESleekTint.FOREGROUND;
			// 			closeFiltersVisibilityButton.AddChild(closeHintIcon);
		}

		private void CreateFilterVisibilityToggles()
		{
			listSourceToggle = Glazier.Get().CreateToggle();
			listSourceToggle.Value = FilterSettings.filterVisibility.listSource;
			listSourceToggle.AddLabel(localization.format("List_Label"), ESleekSide.RIGHT);
			listSourceToggle.TooltipText = localization.format("List_Toggle_Tooltip");
			listSourceToggle.OnValueChanged += OnListSourceFilterToggled;
			filtersEditorContainer.AddChild(listSourceToggle);

			nameToggle = Glazier.Get().CreateToggle();
			nameToggle.Value = FilterSettings.filterVisibility.name;
			nameToggle.AddLabel(localization.format("Name_Filter_Label"), ESleekSide.RIGHT);
			nameToggle.TooltipText = localization.format("Name_Filter_Toggle_Tooltip");
			nameToggle.OnValueChanged += OnNameFilterToggled;
			filtersEditorContainer.AddChild(nameToggle);

			mapToggle = Glazier.Get().CreateToggle();
			mapToggle.Value = FilterSettings.filterVisibility.map;
			mapToggle.AddLabel(localization.format("Map_Filter_Label"), ESleekSide.RIGHT);
			mapToggle.TooltipText = localization.format("Map_Filter_Toggle_Tooltip");
			mapToggle.OnValueChanged += OnMapFilterToggled;
			filtersEditorContainer.AddChild(mapToggle);

			passwordToggle = Glazier.Get().CreateToggle();
			passwordToggle.Value = FilterSettings.filterVisibility.password;
			passwordToggle.AddLabel(localization.format("Password_Filter_Label"), ESleekSide.RIGHT);
			passwordToggle.TooltipText = localization.format("Password_Filter_Toggle_Tooltip");
			passwordToggle.OnValueChanged += OnPasswordFilterToggled;
			filtersEditorContainer.AddChild(passwordToggle);

			attendanceToggle = Glazier.Get().CreateToggle();
			attendanceToggle.Value = FilterSettings.filterVisibility.attendance;
			attendanceToggle.AddLabel(localization.format("Attendance_Filter_Label"), ESleekSide.RIGHT);
			attendanceToggle.TooltipText = localization.format("Attendance_Filter_Toggle_Tooltip");
			attendanceToggle.OnValueChanged += OnAttendanceFilterToggled;
			filtersEditorContainer.AddChild(attendanceToggle);

			notFullToggle = Glazier.Get().CreateToggle();
			notFullToggle.Value = FilterSettings.filterVisibility.notFull;
			notFullToggle.AddLabel(localization.format("Space_Filter_Label"), ESleekSide.RIGHT);
			notFullToggle.TooltipText = localization.format("Space_Filter_Toggle_Tooltip");
			notFullToggle.OnValueChanged += OnSpaceFilterToggled;
			filtersEditorContainer.AddChild(notFullToggle);

			combatToggle = Glazier.Get().CreateToggle();
			combatToggle.Value = FilterSettings.filterVisibility.combat;
			combatToggle.AddLabel(localization.format("Combat_Filter_Label"), ESleekSide.RIGHT);
			combatToggle.TooltipText = localization.format("Combat_Filter_Toggle_Tooltip");
			combatToggle.OnValueChanged += OnCombatFilterToggled;
			filtersEditorContainer.AddChild(combatToggle);

			cameraToggle = Glazier.Get().CreateToggle();
			cameraToggle.Value = FilterSettings.filterVisibility.camera;
			cameraToggle.AddLabel(localization.format("Perspective_Filter_Label"), ESleekSide.RIGHT);
			cameraToggle.TooltipText = localization.format("Perspective_Filter_Toggle_Tooltip");
			cameraToggle.OnValueChanged += OnCameraFilterToggled;
			filtersEditorContainer.AddChild(cameraToggle);

			goldToggle = Glazier.Get().CreateToggle();
			goldToggle.Value = FilterSettings.filterVisibility.gold;
			goldToggle.AddLabel(localization.format("Gold_Filter_Label"), Palette.PRO, ESleekSide.RIGHT);
			goldToggle.TooltipText = localization.format("Gold_Filter_Toggle_Tooltip");
			goldToggle.OnValueChanged += OnGoldFilterToggled;
			filtersEditorContainer.AddChild(goldToggle);

			monetizationToggle = Glazier.Get().CreateToggle();
			monetizationToggle.Value = FilterSettings.filterVisibility.monetization;
			monetizationToggle.AddLabel(localization.format("Monetization_Filter_Label"), ESleekSide.RIGHT);
			monetizationToggle.TooltipText = localization.format("Monetization_Filter_Toggle_Tooltip");
			monetizationToggle.OnValueChanged += OnMonetizationFilterToggled;
			filtersEditorContainer.AddChild(monetizationToggle);

			workshopToggle = Glazier.Get().CreateToggle();
			workshopToggle.Value = FilterSettings.filterVisibility.workshop;
			workshopToggle.AddLabel(localization.format("Workshop_Filter_Label"), ESleekSide.RIGHT);
			workshopToggle.TooltipText = localization.format("Workshop_Filter_Toggle_Tooltip");
			workshopToggle.OnValueChanged += OnWorkshopFilterToggled;
			filtersEditorContainer.AddChild(workshopToggle);

			pluginsToggle = Glazier.Get().CreateToggle();
			pluginsToggle.Value = FilterSettings.filterVisibility.plugins;
			pluginsToggle.AddLabel(localization.format("Plugins_Filter_Label"), ESleekSide.RIGHT);
			pluginsToggle.TooltipText = localization.format("Plugins_Filter_Toggle_Tooltip");
			pluginsToggle.OnValueChanged += OnPluginsFilterToggled;
			filtersEditorContainer.AddChild(pluginsToggle);

			cheatsToggle = Glazier.Get().CreateToggle();
			cheatsToggle.Value = FilterSettings.filterVisibility.cheats;
			cheatsToggle.AddLabel(localization.format("Cheats_Filter_Label"), ESleekSide.RIGHT);
			cheatsToggle.TooltipText = localization.format("Cheats_Filter_Toggle_Tooltip");
			cheatsToggle.OnValueChanged += OnCheatsFilterToggled;
			filtersEditorContainer.AddChild(cheatsToggle);

			vacToggle = Glazier.Get().CreateToggle();
			vacToggle.Value = FilterSettings.filterVisibility.vacProtection;
			vacToggle.AddLabel(localization.format("VAC_Filter_Label"), ESleekSide.RIGHT);
			vacToggle.TooltipText = localization.format("VAC_Filter_Toggle_Tooltip");
			vacToggle.OnValueChanged += OnVACFilterToggled;
			filtersEditorContainer.AddChild(vacToggle);

#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatToggle = Glazier.Get().CreateToggle();
			thirdpartyAntiCheatToggle.Value = FilterSettings.filterVisibility.thirdpartyAntiCheatProtection;
			thirdpartyAntiCheatToggle.AddLabel(localization.format(ThirdpartyAntiCheat.FilterToggleLabelKey), ESleekSide.RIGHT);
			thirdpartyAntiCheatToggle.TooltipText = localization.format(ThirdpartyAntiCheat.FilterToggleTooltipKey);
			thirdpartyAntiCheatToggle.OnValueChanged += OnThirdpartyAntiCheatFilterToggled;
			filtersEditorContainer.AddChild(thirdpartyAntiCheatToggle);
#endif

			maxPingToggle = Glazier.Get().CreateToggle();
			maxPingToggle.Value = FilterSettings.filterVisibility.maxPing;
			maxPingToggle.AddLabel(localization.format("MaxPing_Filter_Label"), ESleekSide.RIGHT);
			maxPingToggle.TooltipText = localization.format("MaxPing_Filter_Toggle_Tooltip");
			maxPingToggle.OnValueChanged += OnMaxPingFilterToggled;
			filtersEditorContainer.AddChild(maxPingToggle);
		}

		private void AnimateOpenSubcontainers()
		{
			if (FilterSettings.isColumnsEditorOpen)
			{
				columnTogglesContainer.AnimatePositionOffset(0, columnTogglesContainer.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20.0f);
				columnTogglesContainer.AnimatePositionScale(0.0f, columnTogglesContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20.0f);
			}
			else
			{
				columnTogglesContainer.AnimatePositionOffset(20, columnTogglesContainer.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20.0f);
				columnTogglesContainer.AnimatePositionScale(1.0f, columnTogglesContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20.0f);
			}

			if (FilterSettings.isPresetsListOpen)
			{
				presetsContainer.AnimatePositionOffset(0, presetsContainer.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20.0f);
				presetsContainer.AnimatePositionScale(0.0f, presetsContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20.0f);
			}
			else
			{
				presetsContainer.AnimatePositionOffset(20, presetsContainer.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20.0f);
				presetsContainer.AnimatePositionScale(1.0f, presetsContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20.0f);
			}

			float presetsListOffset = FilterSettings.isPresetsListOpen ? presetsContainer.SizeOffset_Y : 0;

			if (FilterSettings.isQuickFiltersEditorOpen)
			{
				filtersEditorContainer.AnimatePositionOffset(0, -70 - filtersEditorContainer.SizeOffset_Y - presetsListOffset, ESleekLerp.EXPONENTIAL, 20.0f);
				filtersEditorContainer.AnimatePositionScale(0.0f, filtersEditorContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20.0f);
			}
			else
			{
				filtersEditorContainer.AnimatePositionOffset(20, -70 - filtersEditorContainer.SizeOffset_Y - presetsListOffset, ESleekLerp.EXPONENTIAL, 20.0f);
				filtersEditorContainer.AnimatePositionScale(1.0f, filtersEditorContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20.0f);
			}

			float columnsEditorOffset = FilterSettings.isColumnsEditorOpen ? columnTogglesContainer.SizeOffset_Y + 10 : 0;
			float quickFiltersOffset = FilterSettings.isQuickFiltersEditorOpen ? filtersEditorContainer.SizeOffset_Y : 0;
			mainListContainer.AnimatePositionOffset(mainListContainer.PositionOffset_X, columnsEditorOffset, ESleekLerp.EXPONENTIAL, 20.0f);
			mainListContainer.AnimateSizeOffset(mainListContainer.SizeOffset_X, -columnsEditorOffset - presetsListOffset - quickFiltersOffset, ESleekLerp.EXPONENTIAL, 20.0f);
		}

		private void SynchronizeVisibleColumns()
		{
			const float spacing = 0;
			float horizontalOffset = -30; // Accounts for width of scroll bar.

			if (FilterSettings.columns.anticheat)
			{
				horizontalOffset -= anticheatColumnButton.SizeOffset_X;
				anticheatColumnButton.PositionOffset_X = horizontalOffset;
				anticheatColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				anticheatColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.cheats)
			{
				horizontalOffset -= cheatsColumnButton.SizeOffset_X;
				cheatsColumnButton.PositionOffset_X = horizontalOffset;
				cheatsColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				cheatsColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.plugins)
			{
				horizontalOffset -= pluginsColumnButton.SizeOffset_X;
				pluginsColumnButton.PositionOffset_X = horizontalOffset;
				pluginsColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				pluginsColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.workshop)
			{
				horizontalOffset -= workshopColumnButton.SizeOffset_X;
				workshopColumnButton.PositionOffset_X = horizontalOffset;
				workshopColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				workshopColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.monetization)
			{
				horizontalOffset -= monetizationColumnButton.SizeOffset_X;
				monetizationColumnButton.PositionOffset_X = horizontalOffset;
				monetizationColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				monetizationColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.gold)
			{
				horizontalOffset -= goldColumnButton.SizeOffset_X;
				goldColumnButton.PositionOffset_X = horizontalOffset;
				goldColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				goldColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.perspective)
			{
				horizontalOffset -= perspectiveColumnButton.SizeOffset_X;
				perspectiveColumnButton.PositionOffset_X = horizontalOffset;
				perspectiveColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				perspectiveColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.combat)
			{
				horizontalOffset -= combatColumnButton.SizeOffset_X;
				combatColumnButton.PositionOffset_X = horizontalOffset;
				combatColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				combatColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.password)
			{
				horizontalOffset -= passwordColumnButton.SizeOffset_X;
				passwordColumnButton.PositionOffset_X = horizontalOffset;
				passwordColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				passwordColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.fullnessPercentage)
			{
				horizontalOffset -= fullnessColumnButton.SizeOffset_X;
				fullnessColumnButton.PositionOffset_X = horizontalOffset;
				fullnessColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				fullnessColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.maxPlayers)
			{
				horizontalOffset -= maxPlayersColumnButton.SizeOffset_X;
				maxPlayersColumnButton.PositionOffset_X = horizontalOffset;
				maxPlayersColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				maxPlayersColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.players)
			{
				if (FilterSettings.columns.maxPlayers)
				{
					playersColumnButton.SizeOffset_X = 80;
				}
				else
				{
					playersColumnButton.SizeOffset_X = 120;
				}

				horizontalOffset -= playersColumnButton.SizeOffset_X;
				playersColumnButton.PositionOffset_X = horizontalOffset;
				playersColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				playersColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.ping)
			{
				horizontalOffset -= pingColumnButton.SizeOffset_X;
				pingColumnButton.PositionOffset_X = horizontalOffset;
				pingColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				pingColumnButton.IsVisible = false;
			}

			if (FilterSettings.columns.map)
			{
				horizontalOffset -= mapColumnButton.SizeOffset_X;
				mapColumnButton.PositionOffset_X = horizontalOffset;
				mapColumnButton.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				mapColumnButton.IsVisible = false;
			}

			horizontalOffset -= nameColumnButton.PositionOffset_X;
			nameColumnButton.SizeOffset_X = horizontalOffset;

			for (int index = serverBox.ElementCount - 1; index >= 0; --index)
			{
				SleekServer serverElement = serverBox.GetElement(index) as SleekServer;
				serverElement.SynchronizeVisibleColumns();
			}
		}

		private void SynchronizeVisibleFilters()
		{
			const int filtersBeforeLineWrap = 5;
			int filterIndex = 0;
			const float filterHorizontalSpacing = 0.2f;

			bool isVisEdOpen = FilterSettings.isQuickFiltersVisibilityEditorOpen;
			float filterVerticalSpacing = isVisEdOpen ? 70.0f : 30.0f;
			float toggleHeight = isVisEdOpen ? 40.0f : 0.0f;
			float beginPadding = 20.0f;

			listSourceToggle.IsVisible = isVisEdOpen;
			listSourceButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.listSource;
			listSourceButtonState.isInteractable = FilterSettings.filterVisibility.listSource;
			if (listSourceButtonState.IsVisible)
			{
				listSourceToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				listSourceToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				listSourceButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				listSourceButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			nameToggle.IsVisible = isVisEdOpen;
			nameField.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.name;
			nameField.IsClickable = FilterSettings.filterVisibility.name;
			if (nameField.IsVisible)
			{
				nameToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				nameToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				nameField.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				nameField.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			mapToggle.IsVisible = isVisEdOpen;
			mapButton.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.map;
			mapButton.isClickable = FilterSettings.filterVisibility.map;
			if (mapButton.IsVisible)
			{
				mapToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				mapToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				mapButton.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				mapButton.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			passwordToggle.IsVisible = isVisEdOpen;
			passwordButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.password;
			passwordButtonState.isInteractable = FilterSettings.filterVisibility.password;
			if (passwordButtonState.IsVisible)
			{
				passwordToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				passwordToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				passwordButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				passwordButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			attendanceToggle.IsVisible = isVisEdOpen;
			attendanceButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.attendance;
			attendanceButtonState.isInteractable = FilterSettings.filterVisibility.attendance;
			if (attendanceButtonState.IsVisible)
			{
				attendanceToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				attendanceToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				attendanceButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				attendanceButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			notFullToggle.IsVisible = isVisEdOpen;
			notFullButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.notFull;
			notFullButtonState.isInteractable = FilterSettings.filterVisibility.notFull;
			if (notFullButtonState.IsVisible)
			{
				notFullToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				notFullToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				notFullButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				notFullButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			combatToggle.IsVisible = isVisEdOpen;
			combatButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.combat;
			combatButtonState.isInteractable = FilterSettings.filterVisibility.combat;
			if (combatButtonState.IsVisible)
			{
				combatToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				combatToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				combatButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				combatButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			cameraToggle.IsVisible = isVisEdOpen;
			cameraButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.camera;
			cameraButtonState.isInteractable = FilterSettings.filterVisibility.camera;
			if (cameraButtonState.IsVisible)
			{
				cameraToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				cameraToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				cameraButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				cameraButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			goldToggle.IsVisible = isVisEdOpen;
			goldFilterButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.gold;
			goldFilterButtonState.isInteractable = FilterSettings.filterVisibility.gold;
			if (goldFilterButtonState.IsVisible)
			{
				goldToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				goldToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				goldFilterButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				goldFilterButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			monetizationToggle.IsVisible = isVisEdOpen;
			monetizationButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.monetization;
			monetizationButtonState.isInteractable = FilterSettings.filterVisibility.monetization;
			if (monetizationButtonState.IsVisible)
			{
				monetizationToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				monetizationToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				monetizationButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				monetizationButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			workshopToggle.IsVisible = isVisEdOpen;
			workshopButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.workshop;
			workshopButtonState.isInteractable = FilterSettings.filterVisibility.workshop;
			if (workshopButtonState.IsVisible)
			{
				workshopToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				workshopToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				workshopButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				workshopButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			pluginsToggle.IsVisible = isVisEdOpen;
			pluginsButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.plugins;
			pluginsButtonState.isInteractable = FilterSettings.filterVisibility.plugins;
			if (pluginsButtonState.IsVisible)
			{
				pluginsToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				pluginsToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				pluginsButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				pluginsButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			cheatsToggle.IsVisible = isVisEdOpen;
			cheatsButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.cheats;
			cheatsButtonState.isInteractable = FilterSettings.filterVisibility.cheats;
			if (cheatsButtonState.IsVisible)
			{
				cheatsToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				cheatsToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				cheatsButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				cheatsButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			vacToggle.IsVisible = isVisEdOpen;
			VACProtectionButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.vacProtection;
			VACProtectionButtonState.isInteractable = FilterSettings.filterVisibility.vacProtection;
			if (VACProtectionButtonState.IsVisible)
			{
				vacToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				vacToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				VACProtectionButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				VACProtectionButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatToggle.IsVisible = isVisEdOpen;
			thirdpartyAntiCheatButtonState.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.thirdpartyAntiCheatProtection;
			thirdpartyAntiCheatButtonState.isInteractable = FilterSettings.filterVisibility.thirdpartyAntiCheatProtection;
			if (thirdpartyAntiCheatButtonState.IsVisible)
			{
				thirdpartyAntiCheatToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				thirdpartyAntiCheatToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				thirdpartyAntiCheatButtonState.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				thirdpartyAntiCheatButtonState.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}
#endif

			maxPingToggle.IsVisible = isVisEdOpen;
			maxPingField.IsVisible = isVisEdOpen || FilterSettings.filterVisibility.maxPing;
			maxPingField.IsClickable = FilterSettings.filterVisibility.maxPing;
			if (maxPingField.IsVisible)
			{
				maxPingToggle.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				maxPingToggle.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing;

				maxPingField.PositionScale_X = (filterIndex % filtersBeforeLineWrap) * filterHorizontalSpacing;
				maxPingField.PositionOffset_Y = beginPadding + (filterIndex / filtersBeforeLineWrap) * filterVerticalSpacing + toggleHeight;
				++filterIndex;
			}

			float listHeight = MathfEx.GetPageCount(filterIndex, filtersBeforeLineWrap) * filterVerticalSpacing;
			// 			filtersVisibilityButton.PositionOffset_Y = listHeight + 10;
			// 			if (isVisEdOpen)
			// 			{
			// 				filtersVisibilityButton.text = localization.format("QuickFiltersVisibilityButton_Close_Label");
			// 			}
			// 			else
			// 			{
			// 				filtersVisibilityButton.text = localization.format("QuickFiltersVisibilityButton_Open_Label");
			// 			}
			// 			filtersEditorContainer.SizeOffset_Y = listHeight + 60;
			filtersEditorContainer.SizeOffset_Y = listHeight + 20;
			openFiltersVisibilityButton.IsVisible = !isVisEdOpen;
			closeFiltersVisibilityButton.IsVisible = isVisEdOpen;
		}

		private void onClickedResetFilters(ISleekElement button)
		{
			FilterSettings.activeFilters.CopyFrom(FilterSettings.defaultPresetInternet);
			FilterSettings.activeFilters.presetName = localization.format("DefaultPreset_Internet_Label");
			FilterSettings.InvokeActiveFiltersReplaced();
		}

		private void onClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			if (isRefreshing)
			{
				Provider.provider.matchmakingService.cancelRequest();
			}

			Provider.provider.matchmakingService.onMasterServerAdded -= onMasterServerAdded;
			Provider.provider.matchmakingService.onMasterServerRemoved -= onMasterServerRemoved;
			Provider.provider.matchmakingService.onMasterServerResorted -= onMasterServerResorted;
			Provider.provider.matchmakingService.onMasterServerRefreshed -= onMasterServerRefreshed;
			FilterSettings.OnActiveFiltersModified -= OnActiveFiltersModified;
			FilterSettings.OnActiveFiltersReplaced -= OnActiveFiltersReplaced;
			FilterSettings.OnCustomPresetsListChanged -= OnCustomPresetsListChanged;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (isRefreshing)
			{
				float angle = refreshIcon.RotationAngle + Time.deltaTime * 90.0f;
				angle %= 360.0f;
				refreshIcon.RotationAngle = angle;
			}
		}

		private void SetIsRefreshing(bool value)
		{
			isRefreshing = value;
			if (isRefreshing)
			{
				refreshButton.Text = localization.format("Refresh_Cancel_Label");
				refreshButton.TooltipText = localization.format("Refresh_Cancel_Tooltip");
			}
			else
			{
				refreshButton.Text = localization.format("Refresh_Label");
				refreshButton.TooltipText = localization.format("Refresh_Tooltip");
				refreshIcon.RotationAngle = 0.0f;
			}
		}

		public MenuPlayServersUI()
		{
			localization = Localization.read("/Menu/Play/MenuPlayServers.dat");
			icons = Bundles.getIconsBundle("UI/Menu/Icons/Play/MenuPlayServers");

			active = false;

			columnTogglesContainer = Glazier.Get().CreateFrame();
			columnTogglesContainer.PositionOffset_X = 20;
			columnTogglesContainer.PositionScale_X = 1.0f;
			columnTogglesContainer.SizeScale_X = 1.0f;
			AddChild(columnTogglesContainer);

			const int togglesBeforeLineWrap = 5;
			int toggleIndex = 0;
			const float toggleHorizontalSpacing = 0.2f;
			const float toggleVerticalSpacing = 40.0f;

			ISleekToggle mapColumnToggle = Glazier.Get().CreateToggle();
			mapColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			mapColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			mapColumnToggle.Value = FilterSettings.columns.map;
			mapColumnToggle.AddLabel(localization.format("Map_Column_Toggle_Label"), ESleekSide.RIGHT);
			mapColumnToggle.TooltipText = localization.format("Map_Column_Toggle_Tooltip");
			mapColumnToggle.OnValueChanged += OnMapColumnToggled;
			columnTogglesContainer.AddChild(mapColumnToggle);
			++toggleIndex;

			ISleekToggle pingColumnToggle = Glazier.Get().CreateToggle();
			pingColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			pingColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			pingColumnToggle.Value = FilterSettings.columns.ping;
			pingColumnToggle.AddLabel(localization.format("Ping_Column_Toggle_Label"), ESleekSide.RIGHT);
			pingColumnToggle.TooltipText = localization.format("Ping_Column_Toggle_Tooltip");
			pingColumnToggle.OnValueChanged += OnPingColumnToggled;
			columnTogglesContainer.AddChild(pingColumnToggle);
			++toggleIndex;

			ISleekToggle playersColumnToggle = Glazier.Get().CreateToggle();
			playersColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			playersColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			playersColumnToggle.Value = FilterSettings.columns.players;
			playersColumnToggle.AddLabel(localization.format("Players_Column_Toggle_Label"), ESleekSide.RIGHT);
			playersColumnToggle.TooltipText = localization.format("Players_Column_Toggle_Tooltip");
			playersColumnToggle.OnValueChanged += OnPlayersColumnToggled;
			columnTogglesContainer.AddChild(playersColumnToggle);
			++toggleIndex;

			ISleekToggle maxPlayersColumnToggle = Glazier.Get().CreateToggle();
			maxPlayersColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			maxPlayersColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			maxPlayersColumnToggle.Value = FilterSettings.columns.maxPlayers;
			maxPlayersColumnToggle.AddLabel(localization.format("MaxPlayers_Column_Toggle_Label"), ESleekSide.RIGHT);
			maxPlayersColumnToggle.TooltipText = localization.format("MaxPlayers_Column_Toggle_Tooltip");
			maxPlayersColumnToggle.OnValueChanged += OnMaxPlayersColumnToggled;
			columnTogglesContainer.AddChild(maxPlayersColumnToggle);
			++toggleIndex;

			ISleekToggle passwordColumnToggle = Glazier.Get().CreateToggle();
			passwordColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			passwordColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			passwordColumnToggle.Value = FilterSettings.columns.password;
			passwordColumnToggle.AddLabel(localization.format("Password_Column_Toggle_Label"), ESleekSide.RIGHT);
			passwordColumnToggle.TooltipText = localization.format("Password_Column_Toggle_Tooltip");
			passwordColumnToggle.OnValueChanged += OnPasswordColumnToggled;
			columnTogglesContainer.AddChild(passwordColumnToggle);
			++toggleIndex;

			ISleekToggle combatColumnToggle = Glazier.Get().CreateToggle();
			combatColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			combatColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			combatColumnToggle.Value = FilterSettings.columns.combat;
			combatColumnToggle.AddLabel(localization.format("Combat_Column_Toggle_Label"), ESleekSide.RIGHT);
			combatColumnToggle.TooltipText = localization.format("Combat_Column_Toggle_Tooltip");
			combatColumnToggle.OnValueChanged += OnCombatColumnToggled;
			columnTogglesContainer.AddChild(combatColumnToggle);
			++toggleIndex;

			ISleekToggle perspectiveColumnToggle = Glazier.Get().CreateToggle();
			perspectiveColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			perspectiveColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			perspectiveColumnToggle.Value = FilterSettings.columns.perspective;
			perspectiveColumnToggle.AddLabel(localization.format("Perspective_Column_Toggle_Label"), ESleekSide.RIGHT);
			perspectiveColumnToggle.TooltipText = localization.format("Perspective_Column_Toggle_Tooltip");
			perspectiveColumnToggle.OnValueChanged += OnPerspectiveColumnToggled;
			columnTogglesContainer.AddChild(perspectiveColumnToggle);
			++toggleIndex;

			ISleekToggle goldColumnToggle = Glazier.Get().CreateToggle();
			goldColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			goldColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			goldColumnToggle.Value = FilterSettings.columns.maxPlayers;
			goldColumnToggle.AddLabel(localization.format("Gold_Column_Toggle_Label"), Palette.PRO, ESleekSide.RIGHT);
			goldColumnToggle.TooltipText = localization.format("Gold_Column_Toggle_Tooltip");
			goldColumnToggle.OnValueChanged += OnGoldColumnToggled;
			columnTogglesContainer.AddChild(goldColumnToggle);
			++toggleIndex;

			ISleekToggle monetizationColumnToggle = Glazier.Get().CreateToggle();
			monetizationColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			monetizationColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			monetizationColumnToggle.Value = FilterSettings.columns.cheats;
			monetizationColumnToggle.AddLabel(localization.format("Monetization_Column_Toggle_Label"), ESleekSide.RIGHT);
			monetizationColumnToggle.TooltipText = localization.format("Monetization_Column_Toggle_Tooltip");
			monetizationColumnToggle.OnValueChanged += OnMonetizationColumnToggled;
			columnTogglesContainer.AddChild(monetizationColumnToggle);
			++toggleIndex;

			ISleekToggle workshopColumnToggle = Glazier.Get().CreateToggle();
			workshopColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			workshopColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			workshopColumnToggle.Value = FilterSettings.columns.workshop;
			workshopColumnToggle.AddLabel(localization.format("Workshop_Column_Toggle_Label"), ESleekSide.RIGHT);
			workshopColumnToggle.TooltipText = localization.format("Workshop_Column_Toggle_Tooltip");
			workshopColumnToggle.OnValueChanged += OnWorkshopColumnToggled;
			columnTogglesContainer.AddChild(workshopColumnToggle);
			++toggleIndex;

			ISleekToggle pluginsColumnToggle = Glazier.Get().CreateToggle();
			pluginsColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			pluginsColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			pluginsColumnToggle.Value = FilterSettings.columns.cheats;
			pluginsColumnToggle.AddLabel(localization.format("Plugins_Column_Toggle_Label"), ESleekSide.RIGHT);
			pluginsColumnToggle.TooltipText = localization.format("Plugins_Column_Toggle_Tooltip");
			pluginsColumnToggle.OnValueChanged += OnPluginsColumnToggled;
			columnTogglesContainer.AddChild(pluginsColumnToggle);
			++toggleIndex;

			ISleekToggle cheatsColumnToggle = Glazier.Get().CreateToggle();
			cheatsColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			cheatsColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			cheatsColumnToggle.Value = FilterSettings.columns.cheats;
			cheatsColumnToggle.AddLabel(localization.format("Cheats_Column_Toggle_Label"), ESleekSide.RIGHT);
			cheatsColumnToggle.TooltipText = localization.format("Cheats_Column_Toggle_Tooltip");
			cheatsColumnToggle.OnValueChanged += OnCheatsColumnToggled;
			columnTogglesContainer.AddChild(cheatsColumnToggle);
			++toggleIndex;

			ISleekToggle anticheatColumnToggle = Glazier.Get().CreateToggle();
			anticheatColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			anticheatColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			anticheatColumnToggle.Value = FilterSettings.columns.anticheat;
			anticheatColumnToggle.AddLabel(localization.format("Anticheat_Column_Toggle_Label"), ESleekSide.RIGHT);
			anticheatColumnToggle.TooltipText = localization.format("Anticheat_Column_Toggle_Tooltip");
			anticheatColumnToggle.OnValueChanged += OnAnticheatColumnToggled;
			columnTogglesContainer.AddChild(anticheatColumnToggle);
			++toggleIndex;

			ISleekToggle fullnessColumnToggle = Glazier.Get().CreateToggle();
			fullnessColumnToggle.PositionScale_X = (toggleIndex % togglesBeforeLineWrap) * toggleHorizontalSpacing;
			fullnessColumnToggle.PositionOffset_Y = (toggleIndex / togglesBeforeLineWrap) * toggleVerticalSpacing;
			fullnessColumnToggle.Value = FilterSettings.columns.fullnessPercentage;
			fullnessColumnToggle.AddLabel(localization.format("Fullness_Column_Toggle_Label"), ESleekSide.RIGHT);
			fullnessColumnToggle.TooltipText = localization.format("Fullness_Column_Toggle_Tooltip");
			fullnessColumnToggle.OnValueChanged += OnFullnessColumnToggled;
			columnTogglesContainer.AddChild(fullnessColumnToggle);
			++toggleIndex;

			columnTogglesContainer.SizeOffset_Y = (((toggleIndex - 1) / togglesBeforeLineWrap) + 1) * toggleVerticalSpacing;

			// Create earlier here so filters open button appears on top.
			mainListContainer = Glazier.Get().CreateFrame();
			mainListContainer.SizeScale_X = 1;
			mainListContainer.SizeScale_Y = 1;
			AddChild(mainListContainer);

			filtersEditorContainer = Glazier.Get().CreateFrame();
			filtersEditorContainer.PositionOffset_X = 20;
			filtersEditorContainer.PositionOffset_Y = -190;
			filtersEditorContainer.PositionScale_X = 1.0f;
			filtersEditorContainer.PositionScale_Y = 1.0f;
			filtersEditorContainer.SizeScale_X = 1.0f;
			AddChild(filtersEditorContainer);

			ISleekImage filtersEditorHorizontalRuleLeft = Glazier.Get().CreateImage();
			filtersEditorHorizontalRuleLeft.PositionOffset_Y = 9;
			filtersEditorHorizontalRuleLeft.SizeScale_X = 0.5f;
			filtersEditorHorizontalRuleLeft.SizeOffset_X = -60.0f;
			filtersEditorHorizontalRuleLeft.SizeOffset_Y = 2;
			filtersEditorHorizontalRuleLeft.Texture = GlazierResources.PixelTexture;
			filtersEditorHorizontalRuleLeft.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
			filtersEditorContainer.AddChild(filtersEditorHorizontalRuleLeft);

			ISleekImage filtersEditorHorizontalRuleRight = Glazier.Get().CreateImage();
			filtersEditorHorizontalRuleRight.PositionScale_X = 0.5f;
			filtersEditorHorizontalRuleRight.PositionOffset_Y = 9;
			filtersEditorHorizontalRuleRight.PositionOffset_X = 60.0f;
			filtersEditorHorizontalRuleRight.SizeScale_X = 0.5f;
			filtersEditorHorizontalRuleRight.SizeOffset_X = -60.0f;
			filtersEditorHorizontalRuleRight.SizeOffset_Y = 2;
			filtersEditorHorizontalRuleRight.Texture = GlazierResources.PixelTexture;
			filtersEditorHorizontalRuleRight.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
			filtersEditorContainer.AddChild(filtersEditorHorizontalRuleRight);

			CreateQuickFilterButtons();
			CreateFilterVisibilityToggles();
			SynchronizeVisibleFilters();

			presetsContainer = Glazier.Get().CreateFrame();
			presetsContainer.PositionOffset_X = 20;
			presetsContainer.PositionScale_X = 1.0f;
			presetsContainer.PositionScale_Y = 1.0f;
			presetsContainer.SizeScale_X = 1.0f;
			AddChild(presetsContainer);

			ISleekImage presetsHorizontalRule = Glazier.Get().CreateImage();
			presetsHorizontalRule.PositionOffset_Y = 9;
			presetsHorizontalRule.SizeScale_X = 1.0f;
			presetsHorizontalRule.SizeOffset_Y = 2;
			presetsHorizontalRule.Texture = GlazierResources.PixelTexture;
			presetsHorizontalRule.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
			presetsContainer.AddChild(presetsHorizontalRule);

			presetsScrollView = Glazier.Get().CreateScrollView();
			presetsScrollView.PositionOffset_Y = 20;
			presetsScrollView.SizeScale_X = 1.0f;
			presetsScrollView.ScaleContentToWidth = true;
			presetsContainer.AddChild(presetsScrollView);

			customPresetsContainer = Glazier.Get().CreateFrame();
			customPresetsContainer.SizeScale_X = 1.0f;
			presetsScrollView.AddChild(customPresetsContainer);

			defaultPresetsContainer = Glazier.Get().CreateFrame();
			defaultPresetsContainer.SizeScale_X = 1.0f;
			defaultPresetsContainer.SizeOffset_Y = 30;
			presetsScrollView.AddChild(defaultPresetsContainer);

			SleekDefaultServerListPresetButton defaultPresetInternetButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetInternet, localization, icons);
			defaultPresetInternetButton.SizeOffset_Y = 30;
			defaultPresetInternetButton.SizeScale_X = 0.2f;
			defaultPresetsContainer.AddChild(defaultPresetInternetButton);

			SleekDefaultServerListPresetButton defaultPresetLANButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetLAN, localization, icons);
			defaultPresetLANButton.PositionScale_X = 0.2f;
			defaultPresetLANButton.SizeOffset_Y = 30;
			defaultPresetLANButton.SizeScale_X = 0.2f;
			defaultPresetsContainer.AddChild(defaultPresetLANButton);

			SleekDefaultServerListPresetButton defaultPresetHistoryButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetHistory, localization, icons);
			defaultPresetHistoryButton.PositionScale_X = 0.4f;
			defaultPresetHistoryButton.SizeOffset_Y = 30;
			defaultPresetHistoryButton.SizeScale_X = 0.2f;
			defaultPresetsContainer.AddChild(defaultPresetHistoryButton);

			SleekDefaultServerListPresetButton defaultPresetFavoritesButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFavorites, localization, icons);
			defaultPresetFavoritesButton.PositionScale_X = 0.6f;
			defaultPresetFavoritesButton.SizeOffset_Y = 30;
			defaultPresetFavoritesButton.SizeScale_X = 0.2f;
			defaultPresetsContainer.AddChild(defaultPresetFavoritesButton);

			SleekDefaultServerListPresetButton defaultPresetFriendsButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFriends, localization, icons);
			defaultPresetFriendsButton.PositionScale_X = 0.8f;
			defaultPresetFriendsButton.SizeOffset_Y = 30;
			defaultPresetFriendsButton.SizeScale_X = 0.2f;
			defaultPresetsContainer.AddChild(defaultPresetFriendsButton);

			SleekButtonIcon columnsButton = new SleekButtonIcon(icons.load<Texture2D>("Columns"));
			columnsButton.SizeOffset_X = 40;
			columnsButton.SizeOffset_Y = 40;
			columnsButton.iconPositionOffset = 10;
			columnsButton.iconColor = ESleekTint.FOREGROUND;
			columnsButton.tooltip = localization.format("Columns_Tooltip");
			columnsButton.onClickedButton += OnClickedColumnsButton;
			mainListContainer.AddChild(columnsButton);

			nameColumnButton = Glazier.Get().CreateButton();
			nameColumnButton.PositionOffset_X = 40;
			nameColumnButton.SizeOffset_X = -310;
			nameColumnButton.SizeOffset_Y = 40;
			nameColumnButton.SizeScale_X = 1;
			nameColumnButton.Text = localization.format("Sort_Name");
			nameColumnButton.TooltipText = localization.format("Sort_Name_Tooltip");
			nameColumnButton.OnClicked += OnNameColumnClicked;
			mainListContainer.AddChild(nameColumnButton);

			mapColumnButton = Glazier.Get().CreateButton();
			mapColumnButton.PositionOffset_X = -260;
			mapColumnButton.PositionScale_X = 1;
			mapColumnButton.SizeOffset_X = 153;
			mapColumnButton.SizeOffset_Y = 40;
			mapColumnButton.Text = localization.format("Sort_Map");
			mapColumnButton.TooltipText = localization.format("Sort_Map_Tooltip");
			mapColumnButton.OnClicked += OnMapColumnClicked;
			mainListContainer.AddChild(mapColumnButton);

			playersColumnButton = Glazier.Get().CreateButton();
			playersColumnButton.PositionOffset_X = -150;
			playersColumnButton.PositionScale_X = 1;
			playersColumnButton.SizeOffset_X = 80;
			playersColumnButton.SizeOffset_Y = 40;
			playersColumnButton.Text = localization.format("Sort_Players");
			playersColumnButton.TooltipText = localization.format("Sort_Players_Tooltip");
			playersColumnButton.OnClicked += OnPlayersColumnClicked;
			mainListContainer.AddChild(playersColumnButton);

			maxPlayersColumnButton = Glazier.Get().CreateButton();
			maxPlayersColumnButton.PositionOffset_X = -150;
			maxPlayersColumnButton.PositionScale_X = 1;
			maxPlayersColumnButton.SizeOffset_X = 80;
			maxPlayersColumnButton.SizeOffset_Y = 40;
			maxPlayersColumnButton.Text = localization.format("MaxPlayers_Column_Label");
			maxPlayersColumnButton.TooltipText = localization.format("MaxPlayers_Column_Tooltip");
			maxPlayersColumnButton.OnClicked += OnMaxPlayersColumnClicked;
			mainListContainer.AddChild(maxPlayersColumnButton);

			fullnessColumnButton = Glazier.Get().CreateButton();
			fullnessColumnButton.PositionOffset_X = -150;
			fullnessColumnButton.PositionScale_X = 1;
			fullnessColumnButton.SizeOffset_X = 80;
			fullnessColumnButton.SizeOffset_Y = 40;
			fullnessColumnButton.Text = localization.format("Fullness_Column_Label");
			fullnessColumnButton.TooltipText = localization.format("Fullness_Column_Tooltip");
			fullnessColumnButton.OnClicked += OnFullnessColumnClicked;
			mainListContainer.AddChild(fullnessColumnButton);

			pingColumnButton = Glazier.Get().CreateButton();
			pingColumnButton.PositionOffset_X = -80;
			pingColumnButton.PositionScale_X = 1;
			pingColumnButton.SizeOffset_X = 80;
			pingColumnButton.SizeOffset_Y = 40;
			pingColumnButton.Text = localization.format("Sort_Ping");
			pingColumnButton.TooltipText = localization.format("Sort_Ping_Tooltip");
			pingColumnButton.OnClicked += OnPingColumnClicked;
			mainListContainer.AddChild(pingColumnButton);

			anticheatColumnButton = Glazier.Get().CreateButton();
			anticheatColumnButton.PositionScale_X = 1;
			anticheatColumnButton.SizeOffset_X = 80;
			anticheatColumnButton.SizeOffset_Y = 40;
			anticheatColumnButton.Text = localization.format("Anticheat_Column_Label");
			anticheatColumnButton.TooltipText = localization.format("Anticheat_Column_Tooltip");
			anticheatColumnButton.OnClicked += OnAnticheatColumnClicked;
			mainListContainer.AddChild(anticheatColumnButton);

			perspectiveColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Perspective"), 20);
			perspectiveColumnButton.PositionScale_X = 1;
			perspectiveColumnButton.SizeOffset_X = 40;
			perspectiveColumnButton.SizeOffset_Y = 40;
			perspectiveColumnButton.tooltip = localization.format("Perspective_Column_Tooltip");
			perspectiveColumnButton.onClickedButton += OnPerspectiveColumnClicked;
			perspectiveColumnButton.iconColor = ESleekTint.FOREGROUND;
			perspectiveColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(perspectiveColumnButton);

			combatColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Combat"), 20);
			combatColumnButton.PositionScale_X = 1;
			combatColumnButton.SizeOffset_X = 40;
			combatColumnButton.SizeOffset_Y = 40;
			combatColumnButton.tooltip = localization.format("Combat_Column_Tooltip");
			combatColumnButton.onClickedButton += OnCombatColumnClicked;
			combatColumnButton.iconColor = ESleekTint.FOREGROUND;
			combatColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(combatColumnButton);

			passwordColumnButton = new SleekButtonIcon(icons.load<Texture2D>("PasswordProtected"), 20);
			passwordColumnButton.PositionScale_X = 1;
			passwordColumnButton.SizeOffset_X = 40;
			passwordColumnButton.SizeOffset_Y = 40;
			passwordColumnButton.tooltip = localization.format("Password_Column_Tooltip");
			passwordColumnButton.onClickedButton += OnPasswordColumnClicked;
			passwordColumnButton.iconColor = ESleekTint.FOREGROUND;
			passwordColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(passwordColumnButton);

			workshopColumnButton = new SleekButtonIcon(icons.load<Texture2D>("HasMods"), 20);
			workshopColumnButton.PositionScale_X = 1;
			workshopColumnButton.SizeOffset_X = 40;
			workshopColumnButton.SizeOffset_Y = 40;
			workshopColumnButton.tooltip = localization.format("Workshop_Column_Tooltip");
			workshopColumnButton.onClickedButton += OnWorkshopColumnClicked;
			workshopColumnButton.iconColor = ESleekTint.FOREGROUND;
			workshopColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(workshopColumnButton);

			goldColumnButton = new SleekButtonIcon(icons.load<Texture2D>("GoldRequired"), 20);
			goldColumnButton.PositionScale_X = 1;
			goldColumnButton.SizeOffset_X = 40;
			goldColumnButton.SizeOffset_Y = 40;
			goldColumnButton.tooltip = localization.format("Gold_Column_Tooltip");
			goldColumnButton.onClickedButton += OnGoldColumnClicked;
			goldColumnButton.textColor = Palette.PRO;
			goldColumnButton.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
			goldColumnButton.iconColor = Palette.PRO;
			goldColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(goldColumnButton);

			cheatsColumnButton = new SleekButtonIcon(icons.load<Texture2D>("CheatCodes"), 20);
			cheatsColumnButton.PositionScale_X = 1;
			cheatsColumnButton.SizeOffset_X = 40;
			cheatsColumnButton.SizeOffset_Y = 40;
			cheatsColumnButton.tooltip = localization.format("Cheats_Column_Tooltip");
			cheatsColumnButton.onClickedButton += OnCheatsColumnClicked;
			cheatsColumnButton.iconColor = ESleekTint.FOREGROUND;
			cheatsColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(cheatsColumnButton);

			monetizationColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Monetized"), 20);
			monetizationColumnButton.PositionOffset_X = -260;
			monetizationColumnButton.PositionScale_X = 1;
			monetizationColumnButton.SizeOffset_X = 40;
			monetizationColumnButton.SizeOffset_Y = 40;
			monetizationColumnButton.tooltip = localization.format("Monetization_Column_Tooltip");
			monetizationColumnButton.onClickedButton += OnMonetizationColumnClicked;
			monetizationColumnButton.iconColor = ESleekTint.FOREGROUND;
			monetizationColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(monetizationColumnButton);

			pluginsColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Plugins"), 20);
			pluginsColumnButton.PositionScale_X = 1;
			pluginsColumnButton.SizeOffset_X = 40;
			pluginsColumnButton.SizeOffset_Y = 40;
			pluginsColumnButton.tooltip = localization.format("Plugins_Column_Tooltip");
			pluginsColumnButton.onClickedButton += OnPluginsColumnClicked;
			pluginsColumnButton.iconColor = ESleekTint.FOREGROUND;
			pluginsColumnButton.iconPositionOffset = 10;
			mainListContainer.AddChild(pluginsColumnButton);

			infoBox = Glazier.Get().CreateBox();
			infoBox.PositionOffset_Y = 50;
			infoBox.SizeScale_X = 1;
			infoBox.SizeOffset_X = -30;
			infoBox.SizeOffset_Y = 50;
			mainListContainer.AddChild(infoBox);
			infoBox.IsVisible = false;

			ISleekLabel noServersLabel = Glazier.Get().CreateLabel();
			noServersLabel.SizeScale_X = 1;
			noServersLabel.SizeOffset_Y = 30;
			noServersLabel.Text = localization.format("No_Servers", Provider._modInfo != null ? Provider._modInfo.FormatModVersion() : Provider.APP_VERSION);
			noServersLabel.FontSize = ESleekFontSize.Medium;
			infoBox.AddChild(noServersLabel);

			ISleekLabel noServersHintLabel = Glazier.Get().CreateLabel();
			noServersHintLabel.PositionOffset_Y = 20;
			noServersHintLabel.SizeScale_X = 1;
			noServersHintLabel.SizeOffset_Y = 30;
			noServersHintLabel.Text = localization.format("No_Servers_Hint");
			infoBox.AddChild(noServersHintLabel);

			noServersCuratorsHintLabel = Glazier.Get().CreateLabel();
			noServersCuratorsHintLabel.PositionOffset_Y = 40;
			noServersCuratorsHintLabel.SizeScale_X = 1;
			noServersCuratorsHintLabel.SizeOffset_Y = 30;
			infoBox.AddChild(noServersCuratorsHintLabel);

			resetFiltersButton = Glazier.Get().CreateButton();
			resetFiltersButton.PositionOffset_X = -150;
			resetFiltersButton.PositionOffset_Y = 10;
			resetFiltersButton.PositionScale_X = 0.5f;
			resetFiltersButton.PositionScale_Y = 1.0f;
			resetFiltersButton.SizeOffset_X = 300;
			resetFiltersButton.SizeOffset_Y = 30;
			resetFiltersButton.Text = localization.format("Reset_Filters");
			resetFiltersButton.TooltipText = localization.format("Reset_Filters_Tooltip");
			resetFiltersButton.OnClicked += onClickedResetFilters;
			infoBox.AddChild(resetFiltersButton);

			Provider.provider.matchmakingService.onMasterServerAdded += onMasterServerAdded;
			Provider.provider.matchmakingService.onMasterServerRemoved += onMasterServerRemoved;
			Provider.provider.matchmakingService.onMasterServerResorted += onMasterServerResorted;
			Provider.provider.matchmakingService.onMasterServerRefreshed += onMasterServerRefreshed;
			FilterSettings.OnActiveFiltersModified += OnActiveFiltersModified;
			FilterSettings.OnActiveFiltersReplaced += OnActiveFiltersReplaced;
			FilterSettings.OnCustomPresetsListChanged += OnCustomPresetsListChanged;

			refreshButton = Glazier.Get().CreateButton();
			refreshButton.PositionOffset_X = -200;
			refreshButton.PositionOffset_Y = -50;
			refreshButton.PositionScale_X = 1.0f;
			refreshButton.PositionScale_Y = 1.0f;
			refreshButton.SizeOffset_X = 200;
			refreshButton.SizeOffset_Y = 50;
			refreshButton.Text = localization.format("Refresh_Label");
			refreshButton.TooltipText = localization.format("Refresh_Tooltip");
			refreshButton.OnClicked += onClickedRefreshButton;
			refreshButton.FontSize = ESleekFontSize.Medium;
			AddChild(refreshButton);

			refreshIcon = Glazier.Get().CreateImage(icons.load<Texture2D>("Refresh"));
			refreshIcon.PositionOffset_X = 5;
			refreshIcon.PositionOffset_Y = 5;
			refreshIcon.SizeOffset_X = 40;
			refreshIcon.SizeOffset_Y = 40;
			refreshIcon.CanRotate = true;
			refreshIcon.TintColor = ESleekTint.FOREGROUND;
			refreshButton.AddChild(refreshIcon);

			ISleekElement bottomButtonsFrame = Glazier.Get().CreateFrame();
			bottomButtonsFrame.PositionOffset_X = 205;
			bottomButtonsFrame.PositionOffset_Y = -50;
			bottomButtonsFrame.PositionScale_Y = 1.0f;
			bottomButtonsFrame.SizeOffset_X = -410;
			bottomButtonsFrame.SizeScale_X = 1.0f;
			bottomButtonsFrame.SizeOffset_Y = 50;
			AddChild(bottomButtonsFrame);

			ISleekImage bottomButtonsHorizontalRule = Glazier.Get().CreateImage();
			bottomButtonsHorizontalRule.PositionOffset_Y = -61;
			bottomButtonsHorizontalRule.PositionScale_Y = 1.0f;
			bottomButtonsHorizontalRule.SizeScale_X = 1.0f;
			bottomButtonsHorizontalRule.SizeOffset_Y = 2;
			bottomButtonsHorizontalRule.Texture = GlazierResources.PixelTexture;
			bottomButtonsHorizontalRule.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
			AddChild(bottomButtonsHorizontalRule);

			SleekButtonIcon curationButton = new SleekButtonIcon(icons.load<Texture2D>("ServerListCuration"), 40);
			curationButton.PositionOffset_X = 5;
			curationButton.SizeOffset_X = -10;
			curationButton.SizeOffset_Y = 50;
			curationButton.SizeScale_X = 0.25f;
			curationButton.text = localization.format("CurationButtonText");
			curationButton.tooltip = localization.format("CurationButtonTooltip");
			curationButton.onClickedButton += OnClickedCurationButton;
			curationButton.fontSize = ESleekFontSize.Medium;
			curationButton.iconColor = ESleekTint.FOREGROUND;
			bottomButtonsFrame.AddChild(curationButton);

			presetsButton = new SleekButtonIcon(icons.load<Texture2D>("Presets"), 40);
			presetsButton.PositionOffset_X = 5;
			presetsButton.PositionScale_X = 0.25f;
			presetsButton.SizeOffset_X = -10;
			presetsButton.SizeOffset_Y = 50;
			presetsButton.SizeScale_X = 0.25f;
			presetsButton.tooltip = localization.format("ViewPresetsButton_Tooltip");
			presetsButton.onClickedButton += onClickedPresetsButton;
			presetsButton.fontSize = ESleekFontSize.Medium;
			presetsButton.iconColor = ESleekTint.FOREGROUND;
			bottomButtonsFrame.AddChild(presetsButton);
			SynchronizePresetsButtonLabel();

			quickFiltersButton = new SleekButtonIcon(icons.load<Texture2D>("Filters"), 40);
			quickFiltersButton.PositionOffset_X = 5;
			quickFiltersButton.PositionScale_X = 0.5f;
			quickFiltersButton.SizeOffset_X = -10;
			quickFiltersButton.SizeOffset_Y = 50;
			quickFiltersButton.SizeScale_X = 0.25f;
			quickFiltersButton.tooltip = localization.format("QuickFiltersButton_Tooltip");
			quickFiltersButton.onClickedButton += OnQuickFiltersButtonClicked;
			quickFiltersButton.fontSize = ESleekFontSize.Medium;
			quickFiltersButton.iconColor = ESleekTint.FOREGROUND;
			bottomButtonsFrame.AddChild(quickFiltersButton);
			SynchronizeQuickFiltersButtonLabel();

			presetsEditorButton = new SleekButtonIcon(icons.load<Texture2D>("PresetsEditor"), 40);
			presetsEditorButton.PositionOffset_X = 5;
			presetsEditorButton.PositionScale_X = 0.75f;
			presetsEditorButton.SizeOffset_X = -10;
			presetsEditorButton.SizeOffset_Y = 50;
			presetsEditorButton.SizeScale_X = 0.25f;
			presetsEditorButton.tooltip = localization.format("PresetsEditorButton_Tooltip");
			presetsEditorButton.onClickedButton += OnPresetsEditorButtonClicked;
			presetsEditorButton.fontSize = ESleekFontSize.Medium;
			presetsEditorButton.iconColor = ESleekTint.FOREGROUND;
			bottomButtonsFrame.AddChild(presetsEditorButton);

			serverBox = new SleekList<SteamServerAdvertisement>();
			serverBox.PositionOffset_Y = 50;
			serverBox.SizeOffset_Y = -120;
			serverBox.SizeScale_X = 1;
			serverBox.SizeScale_Y = 1;
			serverBox.itemHeight = 40;
			serverBox.scrollView.ReduceWidthWhenScrollbarVisible = false;
			serverBox.onCreateElement = onCreateServerElement;
			serverBox.SetData(Provider.provider.matchmakingService.serverList);
			mainListContainer.AddChild(serverBox);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			AddChild(backButton);

			SynchronizeVisibleColumns();
			SynchronizePresetsList();
			SynchronizePresetsEditorButtonLabel();

			mapFiltersUI = new MenuPlayMapFiltersUI(this);
			mapFiltersUI.PositionOffset_X = 10;
			mapFiltersUI.PositionOffset_Y = 10;
			mapFiltersUI.PositionScale_Y = 1;
			mapFiltersUI.SizeOffset_X = -20;
			mapFiltersUI.SizeOffset_Y = -20;
			mapFiltersUI.SizeScale_X = 1;
			mapFiltersUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(mapFiltersUI);

			serverListFiltersUI = new MenuPlayServerListFiltersUI(this);
			serverListFiltersUI.PositionOffset_X = 10;
			serverListFiltersUI.PositionOffset_Y = 10;
			serverListFiltersUI.PositionScale_Y = 1;
			serverListFiltersUI.SizeOffset_X = -20;
			serverListFiltersUI.SizeOffset_Y = -20;
			serverListFiltersUI.SizeScale_X = 1;
			serverListFiltersUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(serverListFiltersUI);

			serverCurationUI = new MenuPlayServerCurationUI(this);
			serverCurationUI.PositionOffset_X = 10;
			serverCurationUI.PositionOffset_Y = 10;
			serverCurationUI.PositionScale_Y = 1;
			serverCurationUI.SizeOffset_X = -20;
			serverCurationUI.SizeOffset_Y = -20;
			serverCurationUI.SizeScale_X = 1;
			serverCurationUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(serverCurationUI);
		}

		public static MenuPlayMapFiltersUI mapFiltersUI;
		public static MenuPlayServerListFiltersUI serverListFiltersUI;
		public static MenuPlayServerCurationUI serverCurationUI;
	}
}
