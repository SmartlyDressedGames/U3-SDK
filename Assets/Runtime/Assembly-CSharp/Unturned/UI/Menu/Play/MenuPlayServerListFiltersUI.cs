////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuPlayServerListFiltersUI : SleekFullscreenBox
	{
		public Local localization;
		public IconsBundle icons;
		public bool active;

		private SleekButtonIcon backButton;

		private ISleekBox presetsTitleBox;
		private ISleekScrollView presetsScrollView;
		private ISleekElement customPresetsContainer;
		private ISleekElement defaultPresetsContainer;

		private ISleekBox filtersTitleBox;
		private ISleekScrollView filtersScrollView;
		private SleekButtonIconConfirm deletePresetButton;
		private ISleekField presetNameField;

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
		private SleekButtonState thirdpartyAntiCheatProtectionButtonState;
#endif
		private SleekButtonState combatButtonState;
		private SleekButtonState goldFilterButtonState;
		private SleekButtonState cameraButtonState;
		private SleekButtonState listSourceButtonState;
		private ISleekInt32Field maxPingField;

		public void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			// Synchronize in case they were modified in the servers UI.
			SynchronizeFilterButtons();

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

		public void OpenForMap(string map)
		{
			LevelInfo levelInfo = Level.getLevel(map);
			FilterSettings.activeFilters.ClearMaps();
			FilterSettings.activeFilters.ToggleMap(levelInfo);
			FilterSettings.MarkActiveFilterModified();
			open();
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
			thirdpartyAntiCheatProtectionButtonState.state = (int) FilterSettings.activeFilters.thirdpartyAntiCheatProtection;
#endif
			combatButtonState.state = (int) FilterSettings.activeFilters.combat;
			goldFilterButtonState.state = (int) FilterSettings.activeFilters.gold;
			cameraButtonState.state = (int) FilterSettings.activeFilters.camera;
			monetizationButtonState.state = ((int) FilterSettings.activeFilters.monetization) - 1;
			listSourceButtonState.state = (int) FilterSettings.activeFilters.listSource;
			maxPingField.Value = FilterSettings.activeFilters.maxPing;
		}

		private void SynchronizeDeletePresetButtonVisible()
		{
			deletePresetButton.IsVisible = FilterSettings.activeFilters.presetId > 0;

			if (deletePresetButton.IsVisible)
			{
				filtersScrollView.ContentSizeOffset = new Vector2(0.0f, deletePresetButton.PositionOffset_Y + deletePresetButton.SizeOffset_Y);
			}
			else
			{
				filtersScrollView.ContentSizeOffset = new Vector2(0.0f, presetNameField.PositionOffset_Y + presetNameField.SizeOffset_Y);
			}
		}

		private void onTypedNameField(ISleekField field, string text)
		{
			FilterSettings.activeFilters.serverName = text;
			FilterSettings.MarkActiveFilterModified();
		}

		private void OnClickedMapButton(ISleekElement button)
		{
			MenuPlayServersUI.mapFiltersUI.open(EMenuPlayMapFiltersUIOpenContext.Filters);
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

		private void OnMaxPingChanged(ISleekInt32Field field, int value)
		{
			FilterSettings.activeFilters.maxPing = value;
			FilterSettings.MarkActiveFilterModified();
		}

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

		private void OnClickedCreatePreset(ISleekElement button)
		{
			if (string.IsNullOrWhiteSpace(presetNameField.Text))
			{
				return;
			}

			FilterSettings.activeFilters.presetName = presetNameField.Text.Trim();
			FilterSettings.activeFilters.presetId = FilterSettings.CreatePresetId();
			presetNameField.Text = string.Empty;

			ServerListFilters newPreset = new ServerListFilters();
			newPreset.CopyFrom(FilterSettings.activeFilters);
			FilterSettings.customPresets.Add(newPreset);

			FilterSettings.customPresets.Sort((ServerListFilters lhs, ServerListFilters rhs) =>
			{
				if (!string.IsNullOrEmpty(lhs.presetName) && !string.IsNullOrEmpty(rhs.presetName))
				{
					return lhs.presetName.CompareTo(rhs.presetName);
				}
				else
				{
					return 0;
				}
			});

			FilterSettings.InvokeActiveFiltersReplaced();
			FilterSettings.InvokeCustomFiltersListChanged();
		}

		private void OnClickedDeletePreset(ISleekElement button)
		{
			if (FilterSettings.activeFilters.presetId <= 0)
			{
				return;
			}

			FilterSettings.RemovePreset(FilterSettings.activeFilters.presetId);

			FilterSettings.activeFilters.presetName = string.Empty;
			FilterSettings.activeFilters.presetId = -1;

			FilterSettings.InvokeActiveFiltersReplaced();
			FilterSettings.InvokeCustomFiltersListChanged();
		}

		private void onClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.serverListUI.open(true);
			close();
		}

		private void SynchronizePresetTitle()
		{
			if (string.IsNullOrEmpty(FilterSettings.activeFilters.presetName))
			{
				filtersTitleBox.Text = localization.format("PresetName_Empty");
			}
			else
			{
				filtersTitleBox.Text = FilterSettings.activeFilters.presetName;
			}
		}

		private void SynchronizePresetsList()
		{
			customPresetsContainer.RemoveAllChildren();

			float presetButtonVerticalOffset = 0;

			foreach (ServerListFilters preset in FilterSettings.customPresets)
			{
				SleekCustomServerListPresetButton presetButton = new SleekCustomServerListPresetButton(preset);
				presetButton.PositionOffset_Y = presetButtonVerticalOffset;
				presetButton.SizeOffset_X = 200;
				presetButton.SizeOffset_Y = 30;
				customPresetsContainer.AddChild(presetButton);
				presetButtonVerticalOffset += presetButton.SizeOffset_Y;
			}

			customPresetsContainer.SizeOffset_Y = presetButtonVerticalOffset;
			if (presetButtonVerticalOffset > 0)
			{
				defaultPresetsContainer.PositionOffset_Y = customPresetsContainer.SizeOffset_Y + 10;
			}
			else
			{
				defaultPresetsContainer.PositionOffset_Y = 0;
			}

			presetsScrollView.ContentSizeOffset = new Vector2(0.0f, defaultPresetsContainer.PositionOffset_Y + defaultPresetsContainer.SizeOffset_Y);
		}

		private void OnActiveFiltersModified()
		{
			SynchronizePresetTitle(); // Update " (Modified)" suffix.
			SynchronizeDeletePresetButtonVisible(); // Hide delete button.
		}

		private void OnActiveFiltersReplaced()
		{
			SynchronizePresetTitle();
			SynchronizeFilterButtons();
			SynchronizeDeletePresetButtonVisible();
		}

		private void OnCustomPresetsListChanged()
		{
			SynchronizePresetsList();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			FilterSettings.OnActiveFiltersModified -= OnActiveFiltersModified;
			FilterSettings.OnActiveFiltersReplaced -= OnActiveFiltersReplaced;
			FilterSettings.OnCustomPresetsListChanged -= OnCustomPresetsListChanged;
		}

		public MenuPlayServerListFiltersUI(MenuPlayServersUI serverListUI)
		{
			localization = serverListUI.localization;
			icons = serverListUI.icons;

			active = false;

			ISleekElement presetsContainer = Glazier.Get().CreateFrame();
			presetsContainer.PositionOffset_X = -335;
			presetsContainer.PositionOffset_Y = 100;
			presetsContainer.PositionScale_X = 0.5f;
			presetsContainer.SizeOffset_X = 230;
			presetsContainer.SizeOffset_Y = -200;
			presetsContainer.SizeScale_Y = 1.0f;
			AddChild(presetsContainer);

			ISleekElement filtersContainer = Glazier.Get().CreateFrame();
			filtersContainer.PositionOffset_X = -95;
			filtersContainer.PositionOffset_Y = 100;
			filtersContainer.PositionScale_X = 0.5f;
			filtersContainer.SizeOffset_X = 430;
			filtersContainer.SizeOffset_Y = -200;
			filtersContainer.SizeScale_Y = 1.0f;
			AddChild(filtersContainer);

			presetsTitleBox = Glazier.Get().CreateBox();
			presetsTitleBox.SizeOffset_X = 200.0f;
			presetsTitleBox.SizeOffset_Y = 50.0f;
			presetsTitleBox.FontSize = ESleekFontSize.Medium;
			presetsTitleBox.Text = localization.format("Presets_Label");
			presetsTitleBox.TooltipText = localization.format("Presets_Tooltip");
			presetsContainer.AddChild(presetsTitleBox);

			presetsScrollView = Glazier.Get().CreateScrollView();
			presetsScrollView.PositionOffset_Y = 60.0f;
			presetsScrollView.SizeOffset_X = 230;
			presetsScrollView.SizeOffset_Y = -60;
			presetsScrollView.SizeScale_Y = 1.0f;
			presetsScrollView.ScaleContentToWidth = true;
			presetsContainer.AddChild(presetsScrollView);

			customPresetsContainer = Glazier.Get().CreateFrame();
			customPresetsContainer.SizeScale_X = 1.0f;
			presetsScrollView.AddChild(customPresetsContainer);

			defaultPresetsContainer = Glazier.Get().CreateFrame();
			defaultPresetsContainer.SizeScale_X = 1.0f;
			presetsScrollView.AddChild(defaultPresetsContainer);

			const float presetButtonWidth = 200;
			const float presetButtonHeight = 30;
			const float presetButtonVerticalSpacing = 0;
			float presetButtonVerticalOffset = 0;

			SleekDefaultServerListPresetButton defaultPresetInternetButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetInternet, localization, icons);
			defaultPresetInternetButton.PositionOffset_Y = presetButtonVerticalOffset;
			defaultPresetInternetButton.SizeOffset_X = presetButtonWidth;
			defaultPresetInternetButton.SizeOffset_Y = presetButtonHeight;
			defaultPresetsContainer.AddChild(defaultPresetInternetButton);
			presetButtonVerticalOffset += defaultPresetInternetButton.SizeOffset_Y + presetButtonVerticalSpacing;

			SleekDefaultServerListPresetButton defaultPresetLANButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetLAN, localization, icons);
			defaultPresetLANButton.PositionOffset_Y = presetButtonVerticalOffset;
			defaultPresetLANButton.SizeOffset_X = presetButtonWidth;
			defaultPresetLANButton.SizeOffset_Y = presetButtonHeight;
			defaultPresetsContainer.AddChild(defaultPresetLANButton);
			presetButtonVerticalOffset += defaultPresetLANButton.SizeOffset_Y + presetButtonVerticalSpacing;

			SleekDefaultServerListPresetButton defaultPresetHistoryButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetHistory, localization, icons);
			defaultPresetHistoryButton.PositionOffset_Y = presetButtonVerticalOffset;
			defaultPresetHistoryButton.SizeOffset_X = presetButtonWidth;
			defaultPresetHistoryButton.SizeOffset_Y = presetButtonHeight;
			defaultPresetsContainer.AddChild(defaultPresetHistoryButton);
			presetButtonVerticalOffset += defaultPresetHistoryButton.SizeOffset_Y + presetButtonVerticalSpacing;

			SleekDefaultServerListPresetButton defaultPresetFavoritesButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFavorites, localization, icons);
			defaultPresetFavoritesButton.PositionOffset_Y = presetButtonVerticalOffset;
			defaultPresetFavoritesButton.SizeOffset_X = presetButtonWidth;
			defaultPresetFavoritesButton.SizeOffset_Y = presetButtonHeight;
			defaultPresetsContainer.AddChild(defaultPresetFavoritesButton);
			presetButtonVerticalOffset += defaultPresetFavoritesButton.SizeOffset_Y + presetButtonVerticalSpacing;

			SleekDefaultServerListPresetButton defaultPresetFriendsButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFriends, localization, icons);
			defaultPresetFriendsButton.PositionOffset_Y = presetButtonVerticalOffset;
			defaultPresetFriendsButton.SizeOffset_X = presetButtonWidth;
			defaultPresetFriendsButton.SizeOffset_Y = presetButtonHeight;
			defaultPresetsContainer.AddChild(defaultPresetFriendsButton);
			presetButtonVerticalOffset += defaultPresetFriendsButton.SizeOffset_Y + presetButtonVerticalSpacing;

			defaultPresetsContainer.SizeOffset_Y = presetButtonVerticalOffset;

			SynchronizePresetsList();

			filtersTitleBox = Glazier.Get().CreateBox();
			filtersTitleBox.SizeOffset_X = 400.0f;
			filtersTitleBox.SizeOffset_Y = 50.0f;
			filtersTitleBox.FontSize = ESleekFontSize.Medium;
			filtersContainer.AddChild(filtersTitleBox);

			filtersScrollView = Glazier.Get().CreateScrollView();
			filtersScrollView.PositionOffset_Y = 60.0f;
			filtersScrollView.SizeOffset_X = 430;
			filtersScrollView.SizeOffset_Y = -60;
			filtersScrollView.SizeScale_Y = 1.0f;
			filtersScrollView.ScaleContentToWidth = true;
			filtersContainer.AddChild(filtersScrollView);

			const float filterButtonWidth = 200;
			const float filterButtonHeight = 30;
			const float filterButtonVerticalSpacing = 10;
			float filterButtonVerticalOffset = 0;

			listSourceButtonState = new SleekButtonState(20, new GUIContent(localization.format("List_Internet_Label"), icons.load<Texture>("List_Internet"), localization.format("List_Internet_Tooltip")),
				new GUIContent(localization.format("List_LAN_Label"), icons.load<Texture>("List_LAN"), localization.format("List_LAN_Tooltip")),
				new GUIContent(localization.format("List_History_Label"), icons.load<Texture>("List_History"), localization.format("List_History_Tooltip")),
				new GUIContent(localization.format("List_Favorites_Label"), icons.load<Texture>("List_Favorites"), localization.format("List_Favorites_Tooltip")),
				new GUIContent(localization.format("List_Friends_Label"), icons.load<Texture2D>("List_Friends"), localization.format("List_Friends_Tooltip")));
			listSourceButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			listSourceButtonState.SizeOffset_X = filterButtonWidth;
			listSourceButtonState.SizeOffset_Y = filterButtonHeight;
			listSourceButtonState.onSwappedState = OnSwappedListSourceState;
			listSourceButtonState.button.iconColor = ESleekTint.FOREGROUND;
			listSourceButtonState.UseContentTooltip = true;
			listSourceButtonState.AddLabel(localization.format("List_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(listSourceButtonState);
			filterButtonVerticalOffset += listSourceButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			nameField = Glazier.Get().CreateStringField();
			nameField.PositionOffset_Y = filterButtonVerticalOffset;
			nameField.SizeOffset_X = filterButtonWidth;
			nameField.SizeOffset_Y = filterButtonHeight;
			nameField.PlaceholderText = localization.format("Name_Filter_Hint");
			nameField.OnTextChanged += onTypedNameField;
			nameField.AddLabel(localization.format("Name_Filter_Label"), ESleekSide.RIGHT);
			nameField.TooltipText = localization.format("Name_Filter_Tooltip");
			filtersScrollView.AddChild(nameField);
			filterButtonVerticalOffset += nameField.SizeOffset_Y + filterButtonVerticalSpacing;

			mapButton = new SleekButtonIcon(icons.load<Texture2D>("Map"), 20);
			mapButton.PositionOffset_Y = filterButtonVerticalOffset;
			mapButton.SizeOffset_X = filterButtonWidth;
			mapButton.SizeOffset_Y = filterButtonHeight;
			mapButton.AddLabel(localization.format("Map_Filter_Label"), ESleekSide.RIGHT);
			mapButton.onClickedButton += OnClickedMapButton;
			mapButton.iconColor = ESleekTint.FOREGROUND;
			filtersScrollView.AddChild(mapButton);
			filterButtonVerticalOffset += mapButton.SizeOffset_Y + filterButtonVerticalSpacing;

			passwordButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Password_Button"), icons.load<Texture2D>("NotPasswordProtected"), localization.format("Password_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Password_Button"), icons.load<Texture2D>("PasswordProtected"), localization.format("Password_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Password_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Password_Filter_Any_Tooltip")));
			passwordButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			passwordButtonState.SizeOffset_X = filterButtonWidth;
			passwordButtonState.SizeOffset_Y = filterButtonHeight;
			passwordButtonState.onSwappedState = onSwappedPasswordState;
			passwordButtonState.button.iconColor = ESleekTint.FOREGROUND;
			passwordButtonState.UseContentTooltip = true;
			passwordButtonState.AddLabel(localization.format("Password_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(passwordButtonState);
			filterButtonVerticalOffset += passwordButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			attendanceButtonState = new SleekButtonState(20, new GUIContent(localization.format("Empty_Button"), icons.load<Texture>("Empty"), localization.format("Attendance_Filter_Empty_Tooltip")),
				new GUIContent(localization.format("HasPlayers_Button"), icons.load<Texture>("HasPlayers"), localization.format("Attendance_Filter_HasPlayers_Tooltip")),
				new GUIContent(localization.format("Any_Attendance_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Attendance_Filter_Any_Tooltip")));
			attendanceButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			attendanceButtonState.SizeOffset_X = filterButtonWidth;
			attendanceButtonState.SizeOffset_Y = filterButtonHeight;
			attendanceButtonState.onSwappedState = onSwappedAttendanceState;
			attendanceButtonState.button.iconColor = ESleekTint.FOREGROUND;
			attendanceButtonState.UseContentTooltip = true;
			attendanceButtonState.AddLabel(localization.format("Attendance_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(attendanceButtonState);
			filterButtonVerticalOffset += attendanceButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			notFullButtonState = new SleekButtonState(20, new GUIContent(localization.format("Any_Space_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Space_Filter_Any_Tooltip")),
				new GUIContent(localization.format("Space_Button"), icons.load<Texture>("Space"), localization.format("Space_Filter_HasSpace_Tooltip")));
			notFullButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			notFullButtonState.SizeOffset_X = filterButtonWidth;
			notFullButtonState.SizeOffset_Y = filterButtonHeight;
			notFullButtonState.onSwappedState = OnSwappedNotFullState;
			notFullButtonState.button.iconColor = ESleekTint.FOREGROUND;
			notFullButtonState.UseContentTooltip = true;
			notFullButtonState.AddLabel(localization.format("Space_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(notFullButtonState);
			filterButtonVerticalOffset += notFullButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			combatButtonState = new SleekButtonState(20, new GUIContent(localization.format("PvP_Button"), icons.load<Texture>("PvP"), localization.format("Combat_Filter_PvP_Tooltip")),
				new GUIContent(localization.format("PvE_Button"), icons.load<Texture>("PvE"), localization.format("Combat_Filter_PvE_Tooltip")),
				new GUIContent(localization.format("Any_Combat_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Combat_Filter_Any_Tooltip")));
			combatButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			combatButtonState.SizeOffset_X = filterButtonWidth;
			combatButtonState.SizeOffset_Y = filterButtonHeight;
			combatButtonState.onSwappedState = onSwappedCombatState;
			combatButtonState.button.iconColor = ESleekTint.FOREGROUND;
			combatButtonState.UseContentTooltip = true;
			combatButtonState.AddLabel(localization.format("Combat_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(combatButtonState);
			filterButtonVerticalOffset += combatButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			cameraButtonState = new SleekButtonState(20, new GUIContent(localization.format("First_Button"), icons.load<Texture>("Perspective_FirstPerson"), localization.format("Perspective_Filter_FirstPerson_Tooltip")),
				new GUIContent(localization.format("Third_Button"), icons.load<Texture>("Perspective_ThirdPerson"), localization.format("Perspective_Filter_ThirdPerson_Tooltip")),
				new GUIContent(localization.format("Both_Button"), icons.load<Texture>("Perspective_Both"), localization.format("Perspective_Filter_Both_Tooltip")),
				new GUIContent(localization.format("Vehicle_Button"), icons.load<Texture>("Perspective_Vehicle"), localization.format("Perspective_Filter_Vehicle_Tooltip")),
				new GUIContent(localization.format("Any_Camera_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Perspective_Filter_Any_Tooltip")));
			cameraButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			cameraButtonState.SizeOffset_X = filterButtonWidth;
			cameraButtonState.SizeOffset_Y = filterButtonHeight;
			cameraButtonState.onSwappedState = onSwappedCameraState;
			cameraButtonState.button.iconColor = ESleekTint.FOREGROUND;
			cameraButtonState.UseContentTooltip = true;
			cameraButtonState.AddLabel(localization.format("Perspective_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(cameraButtonState);
			filterButtonVerticalOffset += cameraButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			goldFilterButtonState = new SleekButtonState(20, new GUIContent(localization.format("Gold_Filter_Any_Label"), icons.load<Texture>("AnyFilter"), localization.format("Gold_Filter_Any_Tooltip")),
				new GUIContent(localization.format("Gold_Filter_DoesNotRequireGold_Label"), icons.load<Texture>("GoldNotRequired"), localization.format("Gold_Filter_DoesNotRequireGold_Tooltip")),
				new GUIContent(localization.format("Gold_Filter_RequiresGold_Label"), icons.load<Texture>("GoldRequired"), localization.format("Gold_Filter_RequiresGold_Tooltip")));
			goldFilterButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			goldFilterButtonState.SizeOffset_X = filterButtonWidth;
			goldFilterButtonState.SizeOffset_Y = filterButtonHeight;
			goldFilterButtonState.UseContentTooltip = true;
			goldFilterButtonState.onSwappedState = OnSwappedGoldFilterState;
			goldFilterButtonState.button.textColor = Palette.PRO;
			goldFilterButtonState.button.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
			goldFilterButtonState.button.iconColor = Palette.PRO;
			goldFilterButtonState.AddLabel(localization.format("Gold_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(goldFilterButtonState);
			filterButtonVerticalOffset += goldFilterButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			monetizationButtonState = new SleekButtonState(20, new GUIContent(localization.format("Monetization_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Monetization_Filter_Any_Tooltip")),
				new GUIContent(localization.format("Monetization_None_Button"), icons.load<Texture2D>("Monetization_None"), localization.format("Monetization_Filter_None_Tooltip")),
				new GUIContent(localization.format("Monetization_NonGameplay_Button"), icons.load<Texture2D>("NonGameplayMonetization"), localization.format("Monetization_Filter_NonGameplay_Tooltip")));
			monetizationButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			monetizationButtonState.SizeOffset_X = filterButtonWidth;
			monetizationButtonState.SizeOffset_Y = filterButtonHeight;
			monetizationButtonState.onSwappedState = onSwappedMonetizationState;
			monetizationButtonState.button.iconColor = ESleekTint.FOREGROUND;
			monetizationButtonState.UseContentTooltip = true;
			monetizationButtonState.AddLabel(localization.format("Monetization_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(monetizationButtonState);
			filterButtonVerticalOffset += monetizationButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			workshopButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Workshop_Button"), icons.load<Texture2D>("NoMods"), localization.format("Workshop_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Workshop_Button"), icons.load<Texture2D>("HasMods"), localization.format("Workshop_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Workshop_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Workshop_Filter_Any_Tooltip")));
			workshopButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			workshopButtonState.SizeOffset_X = filterButtonWidth;
			workshopButtonState.SizeOffset_Y = filterButtonHeight;
			workshopButtonState.onSwappedState = onSwappedWorkshopState;
			workshopButtonState.button.iconColor = ESleekTint.FOREGROUND;
			workshopButtonState.UseContentTooltip = true;
			workshopButtonState.AddLabel(localization.format("Workshop_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(workshopButtonState);
			filterButtonVerticalOffset += workshopButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			pluginsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Plugins_Button"), icons.load<Texture2D>("Plugins_None"), localization.format("Plugins_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Plugins_Button"), icons.load<Texture2D>("Plugins"), localization.format("Plugins_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Plugins_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Plugins_Filter_Any_Tooltip")));
			pluginsButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			pluginsButtonState.SizeOffset_X = filterButtonWidth;
			pluginsButtonState.SizeOffset_Y = filterButtonHeight;
			pluginsButtonState.onSwappedState = onSwappedPluginsState;
			pluginsButtonState.button.iconColor = ESleekTint.FOREGROUND;
			pluginsButtonState.UseContentTooltip = true;
			pluginsButtonState.AddLabel(localization.format("Plugins_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(pluginsButtonState);
			filterButtonVerticalOffset += pluginsButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			cheatsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Cheats_Button"), icons.load<Texture2D>("CheatCodes_None"), localization.format("Cheats_Filter_No_Tooltip")),
				new GUIContent(localization.format("Yes_Cheats_Button"), icons.load<Texture2D>("CheatCodes"), localization.format("Cheats_Filter_Yes_Tooltip")),
				new GUIContent(localization.format("Any_Cheats_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Cheats_Filter_Any_Tooltip")));
			cheatsButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			cheatsButtonState.SizeOffset_X = filterButtonWidth;
			cheatsButtonState.SizeOffset_Y = filterButtonHeight;
			cheatsButtonState.onSwappedState = onSwappedCheatsState;
			cheatsButtonState.button.iconColor = ESleekTint.FOREGROUND;
			cheatsButtonState.UseContentTooltip = true;
			cheatsButtonState.AddLabel(localization.format("Cheats_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(cheatsButtonState);
			filterButtonVerticalOffset += cheatsButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

			VACProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format("VAC_Secure_Button"), icons.load<Texture>("VAC"), localization.format("VAC_Filter_Secure_Tooltip")),
				new GUIContent(localization.format("VAC_Insecure_Button"), icons.load<Texture2D>("VAC_Off"), localization.format("VAC_Filter_Insecure_Tooltip")),
				new GUIContent(localization.format("VAC_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("VAC_Filter_Any_Tooltip")));
			VACProtectionButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			VACProtectionButtonState.SizeOffset_X = filterButtonWidth;
			VACProtectionButtonState.SizeOffset_Y = filterButtonHeight;
			VACProtectionButtonState.onSwappedState = onSwappedVACProtectionState;
			VACProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
			VACProtectionButtonState.UseContentTooltip = true;
			VACProtectionButtonState.AddLabel(localization.format("VAC_Filter_Label"), ESleekSide.RIGHT);
			filtersScrollView.AddChild(VACProtectionButtonState);
			filterButtonVerticalOffset += VACProtectionButtonState.SizeOffset_Y + filterButtonVerticalSpacing;

#if WITH_THIRDPARTYAC
			thirdpartyAntiCheatProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format(ThirdpartyAntiCheat.FilterSecureKey), icons.load<Texture>(ThirdpartyAntiCheat.IconName), localization.format(ThirdpartyAntiCheat.FilterSecureTooltipKey)),
				new GUIContent(localization.format(ThirdpartyAntiCheat.FilterInsecureKey), icons.load<Texture2D>(ThirdpartyAntiCheat.IconInsecureName), localization.format(ThirdpartyAntiCheat.FilterInsecureTooltipKey)),
				new GUIContent(localization.format(ThirdpartyAntiCheat.FilterAnyKey), icons.load<Texture2D>("AnyFilter"), localization.format(ThirdpartyAntiCheat.FilterAnyTooltipKey)));
			thirdpartyAntiCheatProtectionButtonState.PositionOffset_Y = filterButtonVerticalOffset;
			thirdpartyAntiCheatProtectionButtonState.SizeOffset_X = filterButtonWidth;
			thirdpartyAntiCheatProtectionButtonState.SizeOffset_Y = filterButtonHeight;
			thirdpartyAntiCheatProtectionButtonState.onSwappedState = onSwappedThirdpartyAntiCheatProtectionState;
			thirdpartyAntiCheatProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
			thirdpartyAntiCheatProtectionButtonState.UseContentTooltip = true;
			thirdpartyAntiCheatProtectionButtonState.AddLabel(localization.format(ThirdpartyAntiCheat.FilterToggleLabelKey), ESleekSide.RIGHT);
			filtersScrollView.AddChild(thirdpartyAntiCheatProtectionButtonState);
			filterButtonVerticalOffset += thirdpartyAntiCheatProtectionButtonState.SizeOffset_Y + filterButtonVerticalSpacing;
#endif

			maxPingField = Glazier.Get().CreateInt32Field();
			maxPingField.PositionOffset_Y = filterButtonVerticalOffset;
			maxPingField.SizeOffset_X = filterButtonWidth;
			maxPingField.SizeOffset_Y = filterButtonHeight;
			maxPingField.OnValueChanged += OnMaxPingChanged;
			maxPingField.AddLabel(localization.format("MaxPing_Filter_Label"), ESleekSide.RIGHT);
			maxPingField.TooltipText = localization.format("MaxPing_Filter_Tooltip");
			filtersScrollView.AddChild(maxPingField);
			filterButtonVerticalOffset += maxPingField.SizeOffset_Y + filterButtonVerticalSpacing;

			filterButtonVerticalOffset += filterButtonVerticalSpacing;
			filterButtonVerticalOffset += filterButtonVerticalSpacing;

			presetNameField = Glazier.Get().CreateStringField();
			presetNameField.PositionOffset_Y = filterButtonVerticalOffset;
			presetNameField.SizeOffset_X = filterButtonWidth;
			presetNameField.SizeOffset_Y = filterButtonHeight;
			presetNameField.PlaceholderText = localization.format("PresetNameField_Hint");
			presetNameField.TooltipText = localization.format("PresetNameField_Tooltip");
			filtersScrollView.AddChild(presetNameField);

			SleekButtonIcon createPresetButton = new SleekButtonIcon(icons.load<Texture2D>("NewPreset"), 20);
			createPresetButton.PositionOffset_X = filterButtonWidth;
			createPresetButton.PositionOffset_Y = filterButtonVerticalOffset;
			createPresetButton.SizeOffset_X = filterButtonWidth;
			createPresetButton.SizeOffset_Y = filterButtonHeight;
			createPresetButton.text = localization.format("NewPreset_Label");
			createPresetButton.tooltip = localization.format("NewPreset_Tooltip");
			createPresetButton.onClickedButton += OnClickedCreatePreset;
			createPresetButton.iconColor = ESleekTint.FOREGROUND;
			filtersScrollView.AddChild(createPresetButton);
			filterButtonVerticalOffset += createPresetButton.SizeOffset_Y + filterButtonVerticalSpacing;

			deletePresetButton = new SleekButtonIconConfirm(icons.load<Texture2D>("DeletePreset"), localization.format("DeletePreset_Confirm_Label"), localization.format("DeletePreset_Confirm_Tooltip"), localization.format("DeletePreset_Deny_Label"), localization.format("DeletePreset_Deny_Tooltip"), 20);
			deletePresetButton.PositionOffset_X = filterButtonWidth * 0.5f;
			deletePresetButton.PositionOffset_Y = filterButtonVerticalOffset;
			deletePresetButton.SizeOffset_X = filterButtonWidth;
			deletePresetButton.SizeOffset_Y = filterButtonHeight;
			deletePresetButton.text = localization.format("DeletePreset_Label");
			deletePresetButton.tooltip = localization.format("DeletePreset_Tooltip");
			deletePresetButton.onConfirmed = OnClickedDeletePreset;
			deletePresetButton.iconColor = ESleekTint.FOREGROUND;
			filtersScrollView.AddChild(deletePresetButton);
			filterButtonVerticalOffset += deletePresetButton.SizeOffset_Y + filterButtonVerticalSpacing;

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

			FilterSettings.OnActiveFiltersModified += OnActiveFiltersModified;
			FilterSettings.OnActiveFiltersReplaced += OnActiveFiltersReplaced;
			FilterSettings.OnCustomPresetsListChanged += OnCustomPresetsListChanged;

			SynchronizePresetTitle();
			SynchronizeFilterButtons();
			SynchronizeDeletePresetButtonVisible();
		}
	}
}
