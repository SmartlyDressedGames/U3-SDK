////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuPlaySingleplayerUI
	{
		public static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static LevelInfo[] levels;

		private static ISleekBox previewBox;
		private static ISleekImage previewImage;

		private static ISleekScrollView levelScrollBox;
		private static SleekLevel[] levelButtons;

		private static SleekButtonIcon playButton;
		private static SleekButtonState modeButtonState;
		private static ISleekButton configButton;
		private static SleekButtonIconConfirm resetButton;
		private static ISleekButton browseServersButton;
		private static ISleekBox selectedBox;
		private static ISleekBox descriptionBox;
		private static ISleekToggle cheatsToggle;
		private static ISleekBox creditsBox;
		private static ISleekButton itemButton;
		private static ISleekButton feedbackButton;
		private static ISleekButton newsButton;

		private static ISleekButton officalMapsButton;
		private static ISleekButton curatedMapsButton;
		private static ISleekButton workshopMapsButton;
		private static ISleekButton miscMapsButton;

		private static SleekNew curatedStatusLabel;

		/// <summary>
		/// Stockpile item definition id with rev-share for the level creators.
		/// Randomly selected from associated items list.
		/// </summary>
		private static int featuredItemDefId;

		private static LevelInfo selectedLevel;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			// Only show browse servers button if multiplayer warning has already been shown.
#if UNITY_64
			browseServersButton.IsVisible = !OptionsSettings.ShouldShowOnlineSafetyMenu;
#endif

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, 1);
		}

		private static void SyncSelectedLevelDetails()
		{
			if (previewImage.Texture != null && previewImage.ShouldDestroyTexture)
			{
				Object.Destroy(previewImage.Texture);
				previewImage.Texture = null;
			}

			if (selectedLevel == null)
			{
				descriptionBox.Text = string.Empty;
				selectedBox.Text = string.Empty;
				creditsBox.IsVisible = false;
				itemButton.IsVisible = false;
				feedbackButton.IsVisible = false;
				newsButton.IsVisible = false;
				return;
			}

			Local localization2 = selectedLevel.getLocalization();
			if (localization2 != null)
			{
				string desc = localization2.format("Description");
				desc = ItemTool.filterRarityRichText(desc);
				RichTextUtil.replaceNewlineMarkup(ref desc);

				descriptionBox.Text = desc;
			}

			if (localization2 != null && localization2.has("Name"))
			{
				selectedBox.Text = localization2.format("Name");
			}
			else
			{
				selectedBox.Text = selectedLevel.name;
			}

			string previewPath = selectedLevel.GetPreviewImageFilePath();
			if (!string.IsNullOrEmpty(previewPath))
			{
				previewImage.Texture = ReadWrite.readTextureFromFile(previewPath);
			}

			float offset = creditsBox.PositionOffset_Y;

			if (selectedLevel.configData.Creators.Length > 0
				|| selectedLevel.configData.Collaborators.Length > 0
				|| selectedLevel.configData.Thanks.Length > 0
				|| selectedLevel.configData.CustomCredits.Count > 0)
			{
				int size = 0;

				string credits = string.Empty;
				if (selectedLevel.configData.Creators.Length > 0)
				{
					credits += localization.format("Creators");
					size += 20;

					for (int index = 0; index < selectedLevel.configData.Creators.Length; index++)
					{
						credits += "\n" + selectedLevel.configData.Creators[index];
						size += 20;
					}
				}

				if (selectedLevel.configData.Collaborators.Length > 0)
				{
					if (credits.Length > 0)
					{
						credits += "\n\n";
						size += 30;
					}

					credits += localization.format("Collaborators");
					size += 20;

					for (int index = 0; index < selectedLevel.configData.Collaborators.Length; index++)
					{
						credits += "\n" + selectedLevel.configData.Collaborators[index];
						size += 20;
					}
				}

				if (selectedLevel.configData.Thanks.Length > 0)
				{
					if (credits.Length > 0)
					{
						credits += "\n\n";
						size += 30;
					}

					credits += localization.format("Thanks");
					size += 20;

					for (int index = 0; index < selectedLevel.configData.Thanks.Length; index++)
					{
						credits += "\n" + selectedLevel.configData.Thanks[index];
						size += 20;
					}
				}

				if (selectedLevel.configData.CustomCredits.Count > 0 && localization2 != null)
				{
					foreach (KeyValuePair<string, string[]> kvp in selectedLevel.configData.CustomCredits)
					{
						if (credits.Length > 0)
						{
							credits += "\n\n";
							size += 30;
						}

						credits += localization2.format(kvp.Key);
						size += 20;
						foreach (string name in kvp.Value)
						{
							credits += $"\n{name}";
							size += 20;
						}
					}
				}

				size = Mathf.Max(size, 40);
				creditsBox.SizeOffset_Y = size;

				creditsBox.Text = credits;
				creditsBox.IsVisible = true;

				offset += size + 10;
			}
			else
			{
				creditsBox.IsVisible = false;
			}

			List<int> eligibleItems = new List<int>(4);
			if (selectedLevel.configData.Item > 0 && !Provider.provider.economyService.isItemHiddenByCountryRestrictions(selectedLevel.configData.Item))
			{
				eligibleItems.Add(selectedLevel.configData.Item);
			}
			if (selectedLevel.configData.Associated_Stockpile_Items.Length > 0)
			{
				foreach (int item in selectedLevel.configData.Associated_Stockpile_Items)
				{
					if (item > 0 && !Provider.provider.economyService.isItemHiddenByCountryRestrictions(item))
					{
						eligibleItems.Add(item);
					}
				}
			}
			featuredItemDefId = eligibleItems.RandomOrDefault();

			if (featuredItemDefId > 0)
			{
				itemButton.PositionOffset_Y = offset;

				itemButton.Text = localization.format("Credits_Text", selectedBox.Text, "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(featuredItemDefId)) + ">" + Provider.provider.economyService.getInventoryName(featuredItemDefId) + "</color>");
				itemButton.TooltipText = localization.format("Credits_Tooltip");

				itemButton.IsVisible = true;

				offset += itemButton.SizeOffset_Y + 10;
			}
			else
			{
				itemButton.IsVisible = false;
			}

			string feedbackUrl = selectedLevel.feedbackUrl;
			if (!string.IsNullOrEmpty(feedbackUrl) && !WebUtils.CanParseThirdPartyUrl(feedbackUrl))
			{
				UnturnedLog.warn("Ignoring potentially unsafe level feedback url {0}", feedbackUrl);
				feedbackUrl = null;
			}

			if (string.IsNullOrEmpty(feedbackUrl))
			{
				feedbackButton.IsVisible = false;
			}
			else
			{
				feedbackButton.PositionOffset_Y = offset;
				feedbackButton.IsVisible = true;
				offset += feedbackButton.SizeOffset_Y + 10;
			}

#if !DEDICATED_SERVER
			MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;

			bool isFeatured = liveConfig.IsNowFeaturedTimeOrBypassed() && liveConfig.IsFeatured(selectedLevel.publishedFileId);
			if (isFeatured && !string.IsNullOrEmpty(liveConfig.linkURL))
			{
				newsButton.Text = liveConfig.linkText;
				newsButton.PositionOffset_Y = offset;
				newsButton.IsVisible = true;
				offset += newsButton.SizeOffset_Y + 10;
			}
			else
			{
				newsButton.IsVisible = false;
			}
#endif // !DEDICATED_SERVER
		}

		private static void onClickedLevel(SleekLevel level, byte index)
		{
			SetAndSaveLevelSelection(level.level);
			SyncSelectedLevelDetails();
		}

		private static void onClickedPlayButton(ISleekElement button)
		{
			Level.UpdateLevelReference(ref selectedLevel);
			if (selectedLevel == null || selectedLevel.IsMissingAnyDependencies())
				return;

			Provider.map = selectedLevel.name;
			Provider.singleplayer(PlaySettings.singleplayerMode, PlaySettings.singleplayerCheats);
		}

		private static void onClickedOfficialMapsButton(ISleekElement button)
		{
			PlaySettings.singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
			refreshLevels();
		}

		private static void onClickedCuratedMapsButton(ISleekElement button)
		{
			// Remove new/updated label if present.
			if (curatedStatusLabel != null)
			{
				curatedMapsButton.RemoveChild(curatedStatusLabel);
				curatedStatusLabel = null;

#if !DEDICATED_SERVER
				MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;
				ConvenientSavedata.get().write("SingleplayerCuratedSeenId", liveConfig.id);
#endif // !DEDICATED_SERVER
			}

			PlaySettings.singleplayerCategory = ESingleplayerMapCategory.CURATED;
			refreshLevels();
		}

		private static void onClickedWorkshopMapsButton(ISleekElement button)
		{
			PlaySettings.singleplayerCategory = ESingleplayerMapCategory.WORKSHOP;
			refreshLevels();
		}

		private static void onClickedMiscMapsButton(ISleekElement button)
		{
			PlaySettings.singleplayerCategory = ESingleplayerMapCategory.MISC;
			refreshLevels();
		}

		private static void onClickedManageSubscriptionsButton(ISleekElement button)
		{
			MenuUI.closeAll();
			MenuWorkshopSubscriptionsUI.instance.open();
		}

		private static void onSwappedModeState(SleekButtonState button, int index)
		{
			PlaySettings.singleplayerMode = (EGameMode) index;
		}

		private static void onClickedConfigButton(ISleekElement button)
		{
			if (selectedLevel == null)
				return;

			MenuPlayConfigUI.open();
			close();
		}

		private static void onClickedBrowseServersButton(ISleekElement button)
		{
			if (selectedLevel == null)
				return;

			MenuPlayServersUI.serverListFiltersUI.OpenForMap(selectedLevel.name);
			close();
		}

		private static void onClickedResetButton(SleekButtonIconConfirm button)
		{
			if (selectedLevel == null)
				return;

			if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Level/" + selectedLevel.name))
			{
				ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Level/" + selectedLevel.name);
			}

			if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user + "_" + Characters.selected + "/" + selectedLevel.name))
			{
				ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user + "_" + Characters.selected + "/" + selectedLevel.name);
			}
		}

		private static void onToggledCheatsToggle(ISleekToggle toggle, bool state)
		{
			PlaySettings.singleplayerCheats = state;
		}

		private static void refreshLevels()
		{
			if (levelScrollBox == null)
				return;

			levelScrollBox.RemoveAllChildren();

			levels = Level.getLevels(PlaySettings.singleplayerCategory);

			int verticalOffset = 0;

			levelButtons = new SleekLevel[levels.Length];
			for (int index = 0; index < levels.Length; index++)
			{
				if (levels[index] != null)
				{
					SleekLevel level = new SleekLevel(levels[index]);
					level.PositionOffset_Y = verticalOffset;
					level.onClickedLevel = onClickedLevel;
					levelScrollBox.AddChild(level);
					verticalOffset += 110;

					levelButtons[index] = level;
				}
			}

			selectedLevel = Level.FindLevel(PlaySettings.singleplayerLevelSelection);
			if (selectedLevel == null && levels.Length > 0)
			{
				SetAndSaveLevelSelection(levels[0]);
			}

			SyncSelectedLevelDetails();

			if (PlaySettings.singleplayerCategory == ESingleplayerMapCategory.CURATED)
			{
				// List retired curated maps as well.
				foreach (CuratedMapLink retiredMap in Provider.statusData.Maps.Curated_Map_Links)
				{
					bool subscribed = Provider.provider.workshopService.getSubscribed(retiredMap.Workshop_File_Id);
					if (subscribed)
					{
						// Player has this map installed, so they will see the ACTUAL level not this retired link.
						continue;
					}

					if (!retiredMap.Visible_In_Singleplayer_Recommendations_List)
						continue;

					SleekCuratedLevelLink level = new SleekCuratedLevelLink(retiredMap);
					level.PositionOffset_Y = verticalOffset;
					levelScrollBox.AddChild(level);

					verticalOffset += 110;
				}
			}

			if (PlaySettings.singleplayerCategory == ESingleplayerMapCategory.WORKSHOP)
			{
				ISleekButton manageButton = Glazier.Get().CreateButton();
				manageButton.PositionOffset_Y = verticalOffset;
				manageButton.SizeOffset_X = 400;
				manageButton.SizeOffset_Y = 30;
				manageButton.Text = localization.format("Manage_Workshop_Label");
				manageButton.TooltipText = localization.format("Manage_Workshop_Tooltip");
				manageButton.OnClicked += onClickedManageSubscriptionsButton;
				levelScrollBox.AddChild(manageButton);
				verticalOffset += 40;
			}

			levelScrollBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10);
		}

		private static void onLevelsRefreshed()
		{
			refreshLevels();
		}

		private static void onClickedItemButton(ISleekElement button)
		{
			if (featuredItemDefId <= 0)
			{
				return;
			}

			ItemStore.Get().ViewItem(featuredItemDefId);
		}

		private static void onClickedFeedbackButton(ISleekElement button)
		{
			if (selectedLevel == null)
				return;

			string parsedFeedbackUrl;
			if (WebUtils.ParseThirdPartyUrl(selectedLevel.feedbackUrl, out parsedFeedbackUrl))
			{
				Provider.provider.browserService.open(parsedFeedbackUrl);
			}
			else
			{
				UnturnedLog.warn("Ignoring potentially unsafe level feedback url {0}", selectedLevel.feedbackUrl);
			}
		}

		private static void onClickedNewsButton(ISleekElement button)
		{
#if !DEDICATED_SERVER
			MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;
			Provider.provider.browserService.open(liveConfig.linkURL);
#endif // !DEDICATED_SERVER
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
		}

		private static void SetAndSaveLevelSelection(LevelInfo newLevel)
		{
			selectedLevel = newLevel;
			if (newLevel != null)
			{
				PlaySettings.singleplayerLevelSelection = new SavedLevelSelection(newLevel);
			}
			else
			{
				PlaySettings.singleplayerLevelSelection.Clear();
			}
		}

#if !DEDICATED_SERVER
		private void OnLiveConfigRefreshed()
		{
			if (hasCreatedFeaturedMapLabel)
			{
				return;
			}

			MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;
			if (liveConfig.status != EMapStatus.None
				&& liveConfig.type == EFeaturedWorkshopType.Curated)
			{
				long seenId;
				if (ConvenientSavedata.get().read("SingleplayerCuratedSeenId", out seenId) && seenId >= liveConfig.id)
				{
					// Alert has been dismissed by the player.
					return;
				}

				curatedStatusLabel = new SleekNew(liveConfig.status == EMapStatus.Updated);
				curatedMapsButton.AddChild(curatedStatusLabel);
				hasCreatedFeaturedMapLabel = true;
			}
		}

		private bool hasCreatedFeaturedMapLabel;
#endif // !DEDICATED_SERVER

		public void OnDestroy()
		{
			Level.onLevelsRefreshed -= onLevelsRefreshed;

#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
#endif // !DEDICATED_SERVER
		}

		public MenuPlaySingleplayerUI()
		{
			localization = Localization.read("/Menu/Play/MenuPlaySingleplayer.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Play/MenuPlaySingleplayer");

			selectedLevel = null;

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			previewBox = Glazier.Get().CreateBox();
			previewBox.PositionOffset_X = -305;
			previewBox.PositionOffset_Y = 80;
			previewBox.PositionScale_X = 0.5f;
			previewBox.SizeOffset_X = 340;
			previewBox.SizeOffset_Y = 200;
			container.AddChild(previewBox);

			// Preview.png is 320x180
			previewImage = Glazier.Get().CreateImage();
			previewImage.PositionOffset_X = 10;
			previewImage.PositionOffset_Y = 10;
			previewImage.SizeOffset_X = -20;
			previewImage.SizeOffset_Y = -20;
			previewImage.SizeScale_X = 1;
			previewImage.SizeScale_Y = 1;
			previewImage.ShouldDestroyTexture = true;
			previewBox.AddChild(previewImage);

			levelScrollBox = Glazier.Get().CreateScrollView();
			levelScrollBox.PositionOffset_X = -95;
			levelScrollBox.PositionOffset_Y = 340;
			levelScrollBox.PositionScale_X = 0.5f;
			levelScrollBox.SizeOffset_X = 430;
			levelScrollBox.SizeOffset_Y = -440;
			levelScrollBox.SizeScale_Y = 1;
			levelScrollBox.ScaleContentToWidth = true;
			container.AddChild(levelScrollBox);

			officalMapsButton = Glazier.Get().CreateButton();
			officalMapsButton.PositionOffset_X = -95;
			officalMapsButton.PositionOffset_Y = 290;
			officalMapsButton.PositionScale_X = 0.5f;
			officalMapsButton.SizeOffset_X = 100;
			officalMapsButton.SizeOffset_Y = 50;
			officalMapsButton.Text = localization.format("Maps_Official");
			officalMapsButton.TooltipText = localization.format("Maps_Official_Tooltip");
			officalMapsButton.OnClicked += onClickedOfficialMapsButton;
			officalMapsButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(officalMapsButton);

			curatedMapsButton = Glazier.Get().CreateButton();
			curatedMapsButton.PositionOffset_X = 5;
			curatedMapsButton.PositionOffset_Y = 290;
			curatedMapsButton.PositionScale_X = 0.5f;
			curatedMapsButton.SizeOffset_X = 100;
			curatedMapsButton.SizeOffset_Y = 50;
			curatedMapsButton.Text = localization.format("Maps_Curated");
			curatedMapsButton.TooltipText = localization.format("Maps_Curated_Tooltip");
			curatedMapsButton.OnClicked += onClickedCuratedMapsButton;
			curatedMapsButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(curatedMapsButton);

#if !DEDICATED_SERVER
			hasCreatedFeaturedMapLabel = false;
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
			OnLiveConfigRefreshed();
#endif // !DEDICATED_SERVER

			workshopMapsButton = Glazier.Get().CreateButton();
			workshopMapsButton.PositionOffset_X = 105;
			workshopMapsButton.PositionOffset_Y = 290;
			workshopMapsButton.PositionScale_X = 0.5f;
			workshopMapsButton.SizeOffset_X = 100;
			workshopMapsButton.SizeOffset_Y = 50;
			workshopMapsButton.Text = localization.format("Maps_Workshop");
			workshopMapsButton.TooltipText = localization.format("Maps_Workshop_Tooltip");
			workshopMapsButton.OnClicked += onClickedWorkshopMapsButton;
			workshopMapsButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(workshopMapsButton);

			miscMapsButton = Glazier.Get().CreateButton();
			miscMapsButton.PositionOffset_X = 205;
			miscMapsButton.PositionOffset_Y = 290;
			miscMapsButton.PositionScale_X = 0.5f;
			miscMapsButton.SizeOffset_X = 100;
			miscMapsButton.SizeOffset_Y = 50;
			miscMapsButton.Text = localization.format("Maps_Misc");
			miscMapsButton.TooltipText = localization.format("Maps_Misc_Tooltip");
			miscMapsButton.OnClicked += onClickedMiscMapsButton;
			miscMapsButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(miscMapsButton);

			selectedBox = Glazier.Get().CreateBox();
			selectedBox.PositionOffset_X = 45;
			selectedBox.PositionOffset_Y = 80;
			selectedBox.PositionScale_X = 0.5f;
			selectedBox.SizeOffset_X = 260;
			selectedBox.SizeOffset_Y = 30;
			container.AddChild(selectedBox);

			descriptionBox = Glazier.Get().CreateBox();
			descriptionBox.PositionOffset_X = 45;
			descriptionBox.PositionOffset_Y = 120;
			descriptionBox.PositionScale_X = 0.5f;
			descriptionBox.SizeOffset_X = 260;
			descriptionBox.SizeOffset_Y = 160;
			descriptionBox.TextAlignment = TextAnchor.UpperCenter;
			descriptionBox.AllowRichText = true;
			descriptionBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			container.AddChild(descriptionBox);

			creditsBox = Glazier.Get().CreateBox();
			creditsBox.PositionOffset_X = 345;
			creditsBox.PositionOffset_Y = 100;
			creditsBox.PositionScale_X = 0.5f;
			creditsBox.SizeOffset_X = 250;
			container.AddChild(creditsBox);
			creditsBox.IsVisible = false;

			itemButton = Glazier.Get().CreateButton();
			itemButton.AllowRichText = true;
			itemButton.PositionOffset_X = 345;
			itemButton.PositionOffset_Y = 100;
			itemButton.PositionScale_X = 0.5f;
			itemButton.SizeOffset_X = 250;
			itemButton.SizeOffset_Y = 100;
			itemButton.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			itemButton.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			itemButton.OnClicked += onClickedItemButton;
			container.AddChild(itemButton);
			itemButton.IsVisible = false;

			feedbackButton = Glazier.Get().CreateButton();
			feedbackButton.PositionOffset_X = 345;
			feedbackButton.PositionOffset_Y = 100;
			feedbackButton.PositionScale_X = 0.5f;
			feedbackButton.SizeOffset_X = 250;
			feedbackButton.SizeOffset_Y = 30;
			feedbackButton.Text = localization.format("Feedback_Button");
			feedbackButton.TooltipText = localization.format("Feedback_Button_Tooltip");
			feedbackButton.OnClicked += onClickedFeedbackButton;
			container.AddChild(feedbackButton);
			feedbackButton.IsVisible = false;

			newsButton = Glazier.Get().CreateButton();
			newsButton.PositionOffset_X = 345;
			newsButton.PositionOffset_Y = 100;
			newsButton.PositionScale_X = 0.5f;
			newsButton.SizeOffset_X = 250;
			newsButton.SizeOffset_Y = 30;
			newsButton.OnClicked += onClickedNewsButton;
			container.AddChild(newsButton);
			newsButton.IsVisible = false;

			playButton = new SleekButtonIcon(icons.load<Texture2D>("Play"));
			playButton.PositionOffset_X = -305;
			playButton.PositionOffset_Y = 290;
			playButton.PositionScale_X = 0.5f;
			playButton.SizeOffset_X = 200;
			playButton.SizeOffset_Y = 30;
			playButton.text = localization.format("Play_Button");
			playButton.tooltip = localization.format("Play_Button_Tooltip");
			playButton.iconColor = ESleekTint.FOREGROUND;
			playButton.onClickedButton += onClickedPlayButton;
			container.AddChild(playButton);

			browseServersButton = Glazier.Get().CreateButton();
			browseServersButton.PositionOffset_X = -305;
			browseServersButton.PositionOffset_Y = 420;
			browseServersButton.PositionScale_X = 0.5f;
			browseServersButton.SizeOffset_X = 200;
			browseServersButton.SizeOffset_Y = 30;
			browseServersButton.Text = localization.format("Browse_Servers_Label");
			browseServersButton.TooltipText = localization.format("Browse_Servers_Tooltip");
			browseServersButton.OnClicked += onClickedBrowseServersButton;
			container.AddChild(browseServersButton);

#if !UNITY_64
			browseServersButton.IsVisible = false;
#endif // !UNITY_64

			modeButtonState = new SleekButtonState(new GUIContent(localization.format("Easy_Button"), icons.load<Texture>("Easy")), new GUIContent(localization.format("Normal_Button"), icons.load<Texture>("Normal")), new GUIContent(localization.format("Hard_Button"), icons.load<Texture>("Hard")));
			modeButtonState.PositionOffset_X = -305;
			modeButtonState.PositionOffset_Y = 330;
			modeButtonState.PositionScale_X = 0.5f;
			modeButtonState.SizeOffset_X = 105;
			modeButtonState.SizeOffset_Y = 30;
			modeButtonState.state = (int) PlaySettings.singleplayerMode;
			modeButtonState.onSwappedState = onSwappedModeState;
			container.AddChild(modeButtonState);

			configButton = Glazier.Get().CreateButton();
			configButton.PositionOffset_X = -195;
			configButton.PositionOffset_Y = 330;
			configButton.PositionScale_X = 0.5f;
			configButton.SizeOffset_X = 85;
			configButton.SizeOffset_Y = 30;
			configButton.Text = localization.format("Config_Button");
			configButton.TooltipText = localization.format("Config_Button_Tooltip");
			configButton.OnClicked += onClickedConfigButton;
			container.AddChild(configButton);

			cheatsToggle = Glazier.Get().CreateToggle();
			cheatsToggle.PositionOffset_X = -305;
			cheatsToggle.PositionOffset_Y = 370;
			cheatsToggle.PositionScale_X = 0.5f;
			cheatsToggle.SizeOffset_X = 40;
			cheatsToggle.SizeOffset_Y = 40;
			cheatsToggle.AddLabel(localization.format("Cheats_Label"), ESleekSide.RIGHT);
			cheatsToggle.Value = PlaySettings.singleplayerCheats;
			cheatsToggle.OnValueChanged += onToggledCheatsToggle;
			container.AddChild(cheatsToggle);

			resetButton = new SleekButtonIconConfirm(null, localization.format("Reset_Button_Confirm"), localization.format("Reset_Button_Confirm_Tooltip"), localization.format("Reset_Button_Deny"), localization.format("Reset_Button_Deny_Tooltip"));
			resetButton.PositionOffset_X = -305;
			resetButton.PositionOffset_Y = 480;
			resetButton.PositionScale_X = 0.5f;
			resetButton.SizeOffset_X = 200;
			resetButton.SizeOffset_Y = 30;
			resetButton.text = localization.format("Reset_Button");
			resetButton.tooltip = localization.format("Reset_Button_Tooltip");
			resetButton.onConfirmed = onClickedResetButton;
			container.AddChild(resetButton);

			refreshLevels();
			Level.onLevelsRefreshed += onLevelsRefreshed;

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
			container.AddChild(backButton);

			new MenuPlayConfigUI();
		}
	}
}
