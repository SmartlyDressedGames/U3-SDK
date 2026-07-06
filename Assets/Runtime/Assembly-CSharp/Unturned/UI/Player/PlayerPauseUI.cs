////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class PlayerPauseUI
	{
		private static SleekFullscreenBox container;
		public static Local localization;
		private static IconsBundle icons;
		public static bool active;

		private static SleekButtonIcon returnButton;
		private static SleekButtonIcon inviteFriendsButton;
		private static SleekButtonIcon optionsButton;
		private static SleekButtonIcon displayButton;
		private static SleekButtonIcon graphicsButton;
		private static SleekButtonIcon controlsButton;
		private static SleekButtonIcon audioButton;
		public static SleekButtonIconConfirm exitButton;
		public static SleekButtonIconConfirm quitButton;
		private static SleekButtonIconConfirm suicideButton;
		private static ISleekLabel suicideDisabledLabel;

		private static ISleekBox spyBox;
		private static ISleekImage spyImage;
		private static ISleekButton spyRefreshButton;
		private static ISleekButton spySlayButton;

		private static ISleekBox serverBox;
		private static SleekButtonIcon favoriteButton;
		private static SleekButtonIcon bookmarkButton;
		private static SleekButtonIcon copyServerCodeButton;
		private static ISleekButton quicksaveButton;

		private static CSteamID spySteamID;

		public static float lastLeave;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			lastLeave = Time.realtimeSinceStartup;

			if (Level.info != null)
			{
				string name = Level.info.getLocalizedName();

				string security;
				if (Provider.isServer)
				{
					security = localization.format("Offline");
				}
				else
				{
					if (Provider.IsVacActiveOnCurrentServer)
					{
						security = localization.format("VAC_Secure");
					}
					else
					{
						security = localization.format("VAC_Insecure");
					}

#if WITH_THIRDPARTYAC
					if (Provider.IsThirdpartyAntiCheatActiveOnCurrentServer)
					{
						security += " + " + localization.format(ThirdpartyAntiCheat.SecureLocalizationKey);
					}
					else
					{
						security += " + " + localization.format(ThirdpartyAntiCheat.InsecureLocalizationKey);
					}
#endif
				}

				serverBox.Text = localization.format("Server_WithVersion", name, Level.version, OptionsSettings.ShouldAnonymizeMultiplayerDetails ? localization.format("Streamer") : Provider.serverName, security);
			}

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			exitButton.reset();
			quitButton.reset();
			suicideButton.reset();

			container.AnimateOutOfView(0, 1);
		}

		public static void closeAndGotoAppropriateHUD()
		{
			close();

			if (Player.LocalPlayer.life.isDead)
			{
				PlayerDeathUI.open(false);
			}
			else
			{
				PlayerLifeUI.open();
			}
		}

		private static void onClickedReturnButton(ISleekElement button)
		{
			closeAndGotoAppropriateHUD();
		}

		private static void OnInviteFriendsClicked(ISleekElement button)
		{
			string connectInfo = $"+connect {ClientMessageHandler_Accepted.RichPresenceConnectionTarget}";
			if (!string.IsNullOrEmpty(Provider.CurrentServerConnectParameters.password))
			{
				connectInfo += $" +password \"{Provider.CurrentServerConnectParameters.password}\"";
			}
			UnturnedLog.info($"Sending rich presence invite: {connectInfo}");

			// Recipient gets GameRichPresenceJoinRequested_t handled by Provider.onGameRichPresenceJoinRequested.
			SteamFriends.ActivateGameOverlayInviteDialogConnectString(connectInfo);
		}

		private static void onClickedOptionsButton(ISleekElement button)
		{
			close();

			MenuConfigurationOptionsUI.open();
		}

		private static void onClickedDisplayButton(ISleekElement button)
		{
			close();

			MenuConfigurationDisplayUI.open();
		}

		private static void onClickedGraphicsButton(ISleekElement button)
		{
			close();

			MenuConfigurationGraphicsUI.open();
		}

		private static void onClickedControlsButton(ISleekElement button)
		{
			close();

			MenuConfigurationControlsUI.open();
		}

		private static void onClickedAudioButton(ISleekElement button)
		{
			close();

			audioMenu.open();
		}

		private static void onClickedSpyRefreshButton(ISleekElement button)
		{
			ChatManager.sendChat(EChatMode.GLOBAL, "/spy " + spySteamID);
		}

		private static void onClickedSpySlayButton(ISleekElement button)
		{
			ChatManager.sendChat(EChatMode.GLOBAL, "/slay " + spySteamID + "/Screenshot Evidence");
		}

		/// <summary>
		/// Exit button only needs to wait for timer in certain conditions.
		/// </summary>
		public static bool shouldExitButtonRespectTimer
		{
			get
			{
				if (Provider.isServer)
					return false; // Singleplayer

				if (!Provider.isPvP)
					return false; // Combat logging is not an issue in PvE mode.

				if (Provider.clients.Count < 2)
					return false; // We are the only player on the server, so nobody is affected by us leaving.

				if (Player.LocalPlayer == null)
					return false; // Should not even happen...

				if (Player.LocalPlayer.life.isDead)
					return false; // Already died so allow immediate disconnect.

				if (Player.LocalPlayer.movement.isSafe && Player.LocalPlayer.movement.isSafeInfo.noIncomingDamage)
					return false; // Inside safezone, so we are not in combat.

				return true; // Yes, respect the timer.
			}
		}

		private static void onClickedExitButton(SleekButtonIconConfirm button)
		{
			if (shouldExitButtonRespectTimer && Time.realtimeSinceStartup - lastLeave < Provider.modeConfigData.Gameplay.Timer_Exit)
			{
				return;
			}

			Provider.RequestDisconnect("clicked exit button from in-game pause menu");
		}

		private static void onClickedQuitButton(SleekButtonIconConfirm button)
		{
			if (shouldExitButtonRespectTimer && Time.realtimeSinceStartup - lastLeave < Provider.modeConfigData.Gameplay.Timer_Exit)
			{
				return;
			}

			Provider.QuitGame("clicked quit from in-game pause menu");
		}

		private static void onClickedSuicideButton(SleekButtonIconConfirm button)
		{
			if ((Level.info != null && Level.info.type == ELevelType.SURVIVAL) || !(Player.LocalPlayer.movement.isSafe && Player.LocalPlayer.movement.isSafeInfo.noIncomingDamage))
			{
				if (Provider.modeConfigData.Gameplay.Can_Suicide)
				{
					closeAndGotoAppropriateHUD();

					Player.LocalPlayer.life.sendSuicide();
				}
			}
		}

		private static void onClickedFavoriteButton(ISleekElement button)
		{
			Provider.toggleCurrentServerFavorited();

			updateFavorite();
		}

		private static void OnClickedBookmarkButton(ISleekElement button)
		{
			Provider.ToggleCurrentServerBookmarked();

			UpdateBookmarkButton();
		}

		private static void OnCopyServerCodeClicked(ISleekElement button)
		{
			GUIUtility.systemCopyBuffer = Provider.server.ToString();
		}

		private static void onClickedQuicksaveButton(ISleekElement button)
		{
			SaveManager.save();
		}

		private static void updateFavorite()
		{
			if (Provider.isCurrentServerFavorited)
			{
				favoriteButton.text = localization.format("Favorite_Off_Button_Text");
				favoriteButton.icon = icons.load<Texture2D>("Favorite_Off");
			}
			else
			{
				favoriteButton.text = localization.format("Favorite_On_Button_Text");
				favoriteButton.icon = icons.load<Texture2D>("Favorite_On");
			}
		}

		private static void UpdateBookmarkButton()
		{
			if (Provider.IsCurrentServerBookmarked)
			{
				bookmarkButton.text = MenuPlayServerInfoUI.localization.format("Bookmark_Off_Button");
				bookmarkButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Bookmark_Remove");
			}
			else
			{
				bookmarkButton.text = MenuPlayServerInfoUI.localization.format("Bookmark_On_Button");
				bookmarkButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Bookmark_Add");
			}
		}

		private static void onSpyReady(CSteamID steamID, byte[] data)
		{
			spySteamID = steamID;

			Texture2D spyTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
			spyTexture.name = "Spy";
			spyTexture.filterMode = FilterMode.Trilinear;
			spyTexture.hideFlags = HideFlags.HideAndDontSave;
			const bool nonReadableOnCPU = true;
			spyTexture.LoadImage(data, nonReadableOnCPU);

			spyImage.Texture = spyTexture;

			returnButton.PositionOffset_X = -435;

			if (inviteFriendsButton != null)
			{
				inviteFriendsButton.PositionOffset_X = -435;
			}
			
			optionsButton.PositionOffset_X = -435;
			displayButton.PositionOffset_X = -435;
			graphicsButton.PositionOffset_X = -435;
			controlsButton.PositionOffset_X = -435;
			audioButton.PositionOffset_X = -435;
			exitButton.PositionOffset_X = -435;
			quitButton.PositionOffset_X = -435;
			suicideButton.PositionOffset_X = -435;
			spyBox.PositionOffset_X = -225;
			spyBox.IsVisible = true;
		}

		internal void OnDestroy()
		{
			ClientMessageHandler_Accepted.OnGameplayConfigReceived -= OnGameplayConfigReceived;
		}

		private void OnGameplayConfigReceived()
		{
			SyncSuicideButtonAvailable();
		}

		private void SyncSuicideButtonAvailable()
		{
			bool canSuicide = Provider.modeConfigData.Gameplay.Can_Suicide;
			suicideButton.isClickable = canSuicide;
			suicideDisabledLabel.IsVisible = !canSuicide;
		}

		public PlayerPauseUI()
		{
			inviteFriendsButton = null;

			localization = Localization.read("/Player/PlayerPause.dat");

			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerPause");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;

			int verticalOffset = -290;

			returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
			returnButton.PositionOffset_X = -100;
			returnButton.PositionOffset_Y = verticalOffset;
			returnButton.PositionScale_X = 0.5f;
			returnButton.PositionScale_Y = 0.5f;
			returnButton.SizeOffset_X = 200;
			returnButton.SizeOffset_Y = 50;
			returnButton.text = localization.format("Return_Button_Text");
			returnButton.tooltip = localization.format("Return_Button_Tooltip");
			returnButton.iconColor = ESleekTint.FOREGROUND;
			returnButton.onClickedButton += onClickedReturnButton;
			returnButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(returnButton);
			verticalOffset += 60;

			if (!Provider.isServer && SteamUtils.IsOverlayEnabled())
			{
				inviteFriendsButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Invite"), 40);
				inviteFriendsButton.PositionOffset_X = -100;
				inviteFriendsButton.PositionOffset_Y = verticalOffset;
				inviteFriendsButton.PositionScale_X = 0.5f;
				inviteFriendsButton.PositionScale_Y = 0.5f;
				inviteFriendsButton.SizeOffset_X = 200;
				inviteFriendsButton.SizeOffset_Y = 50;
				inviteFriendsButton.text = localization.format("InviteFriends_Label");
				inviteFriendsButton.tooltip = localization.format("InviteFriends_Tooltip");
				inviteFriendsButton.onClickedButton += OnInviteFriendsClicked;
				inviteFriendsButton.iconColor = ESleekTint.FOREGROUND;
				inviteFriendsButton.fontSize = ESleekFontSize.Medium;
				container.AddChild(inviteFriendsButton);
				verticalOffset += 60;
			}

			optionsButton = new SleekButtonIcon(icons.load<Texture2D>("Options"));
			optionsButton.PositionOffset_X = -100;
			optionsButton.PositionOffset_Y = verticalOffset;
			optionsButton.PositionScale_X = 0.5f;
			optionsButton.PositionScale_Y = 0.5f;
			optionsButton.SizeOffset_X = 200;
			optionsButton.SizeOffset_Y = 50;
			optionsButton.text = localization.format("Options_Button_Text");
			optionsButton.tooltip = localization.format("Options_Button_Tooltip");
			optionsButton.onClickedButton += onClickedOptionsButton;
			optionsButton.iconColor = ESleekTint.FOREGROUND;
			optionsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(optionsButton);
			verticalOffset += 60;

			displayButton = new SleekButtonIcon(icons.load<Texture2D>("Display"));
			displayButton.PositionOffset_X = -100;
			displayButton.PositionOffset_Y = verticalOffset;
			displayButton.PositionScale_X = 0.5f;
			displayButton.PositionScale_Y = 0.5f;
			displayButton.SizeOffset_X = 200;
			displayButton.SizeOffset_Y = 50;
			displayButton.text = localization.format("Display_Button_Text");
			displayButton.tooltip = localization.format("Display_Button_Tooltip");
			displayButton.iconColor = ESleekTint.FOREGROUND;
			displayButton.onClickedButton += onClickedDisplayButton;
			displayButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(displayButton);
			verticalOffset += 60;

			graphicsButton = new SleekButtonIcon(icons.load<Texture2D>("Graphics"));
			graphicsButton.PositionOffset_X = -100;
			graphicsButton.PositionOffset_Y = verticalOffset;
			graphicsButton.PositionScale_X = 0.5f;
			graphicsButton.PositionScale_Y = 0.5f;
			graphicsButton.SizeOffset_X = 200;
			graphicsButton.SizeOffset_Y = 50;
			graphicsButton.text = localization.format("Graphics_Button_Text");
			graphicsButton.tooltip = localization.format("Graphics_Button_Tooltip");
			graphicsButton.iconColor = ESleekTint.FOREGROUND;
			graphicsButton.onClickedButton += onClickedGraphicsButton;
			graphicsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(graphicsButton);
			verticalOffset += 60;

			controlsButton = new SleekButtonIcon(icons.load<Texture2D>("Controls"));
			controlsButton.PositionOffset_X = -100;
			controlsButton.PositionOffset_Y = verticalOffset;
			controlsButton.PositionScale_X = 0.5f;
			controlsButton.PositionScale_Y = 0.5f;
			controlsButton.SizeOffset_X = 200;
			controlsButton.SizeOffset_Y = 50;
			controlsButton.text = localization.format("Controls_Button_Text");
			controlsButton.tooltip = localization.format("Controls_Button_Tooltip");
			controlsButton.iconColor = ESleekTint.FOREGROUND;
			controlsButton.onClickedButton += onClickedControlsButton;
			controlsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(controlsButton);
			verticalOffset += 60;

			audioButton = new SleekButtonIcon(icons.load<Texture2D>("Audio"));
			audioButton.PositionOffset_X = -100;
			audioButton.PositionOffset_Y = verticalOffset;
			audioButton.PositionScale_X = 0.5f;
			audioButton.PositionScale_Y = 0.5f;
			audioButton.SizeOffset_X = 200;
			audioButton.SizeOffset_Y = 50;
			audioButton.text = localization.format("Audio_Button_Text");
			audioButton.tooltip = localization.format("Audio_Button_Tooltip");
			audioButton.iconColor = ESleekTint.FOREGROUND;
			audioButton.onClickedButton += onClickedAudioButton;
			audioButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(audioButton);
			verticalOffset += 60;

			suicideButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Suicide"), localization.format("Suicide_Button_Confirm"), localization.format("Suicide_Button_Confirm_Tooltip"), localization.format("Suicide_Button_Deny"), localization.format("Suicide_Button_Deny_Tooltip"));
			suicideButton.PositionOffset_X = -100;
			suicideButton.PositionOffset_Y = verticalOffset;
			suicideButton.PositionScale_X = 0.5f;
			suicideButton.PositionScale_Y = 0.5f;
			suicideButton.SizeOffset_X = 200;
			suicideButton.SizeOffset_Y = 50;
			suicideButton.text = localization.format("Suicide_Button_Text");
			suicideButton.tooltip = localization.format("Suicide_Button_Tooltip");
			suicideButton.iconColor = ESleekTint.FOREGROUND;
			suicideButton.onConfirmed = onClickedSuicideButton;
			suicideButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(suicideButton);
			suicideDisabledLabel = Glazier.Get().CreateLabel();
			suicideDisabledLabel.PositionOffset_X = -100;
			suicideDisabledLabel.PositionOffset_Y = verticalOffset;
			suicideDisabledLabel.PositionScale_X = 0.5f;
			suicideDisabledLabel.PositionScale_Y = 0.5f;
			suicideDisabledLabel.SizeOffset_X = 200;
			suicideDisabledLabel.SizeOffset_Y = 50;
			suicideDisabledLabel.Text = localization.format("Suicide_Disabled");
			suicideDisabledLabel.TextColor = ESleekTint.BAD;
			suicideDisabledLabel.FontSize = ESleekFontSize.Large;
			suicideDisabledLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			suicideDisabledLabel.IsVisible = false;
			container.AddChild(suicideDisabledLabel);
			verticalOffset += 60;

			exitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Exit"), localization.format("Exit_Button_Text"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button_Text"), string.Empty);
			exitButton.PositionOffset_X = -100;
			exitButton.PositionOffset_Y = verticalOffset;
			exitButton.PositionScale_X = 0.5f;
			exitButton.PositionScale_Y = 0.5f;
			exitButton.SizeOffset_X = 200;
			exitButton.SizeOffset_Y = 50;
			exitButton.text = localization.format("Exit_Button_Text");
			exitButton.tooltip = localization.format("Exit_Button_Tooltip");
			exitButton.iconColor = ESleekTint.FOREGROUND;
			exitButton.onConfirmed += onClickedExitButton;
			exitButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(exitButton);
			verticalOffset += 60;

			quitButton = new SleekButtonIconConfirm(MenuPauseUI.icons.load<Texture2D>("Quit"), localization.format("Quit_Button"), localization.format("Quit_Button_Tooltip"), localization.format("Return_Button_Text"), string.Empty);
			quitButton.PositionOffset_X = -100;
			quitButton.PositionOffset_Y = verticalOffset;
			quitButton.PositionScale_X = 0.5f;
			quitButton.PositionScale_Y = 0.5f;
			quitButton.SizeOffset_X = 200;
			quitButton.SizeOffset_Y = 50;
			quitButton.text = localization.format("Quit_Button");
			quitButton.tooltip = localization.format("Quit_Button_Tooltip");
			quitButton.iconColor = ESleekTint.FOREGROUND;
			quitButton.onConfirmed += onClickedQuitButton;
			quitButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(quitButton);
			verticalOffset += 60;

			spyBox = Glazier.Get().CreateBox();
			spyBox.PositionOffset_Y = -310;
			spyBox.PositionScale_X = 0.5f;
			spyBox.PositionScale_Y = 0.5f;
			spyBox.SizeOffset_X = 660;
			spyBox.SizeOffset_Y = 500;
			container.AddChild(spyBox);
			spyBox.IsVisible = false;

			spyImage = Glazier.Get().CreateImage();
			spyImage.PositionOffset_X = 10;
			spyImage.PositionOffset_Y = 10;
			spyImage.SizeOffset_X = 640;
			spyImage.SizeOffset_Y = 480;
			spyBox.AddChild(spyImage);

			spyRefreshButton = Glazier.Get().CreateButton();
			spyRefreshButton.PositionOffset_X = -205;
			spyRefreshButton.PositionOffset_Y = 10;
			spyRefreshButton.PositionScale_X = 0.5f;
			spyRefreshButton.PositionScale_Y = 1;
			spyRefreshButton.SizeOffset_X = 200;
			spyRefreshButton.SizeOffset_Y = 50;
			spyRefreshButton.Text = localization.format("Spy_Refresh_Button_Text");
			spyRefreshButton.TooltipText = localization.format("Spy_Refresh_Button_Tooltip");
			spyRefreshButton.OnClicked += onClickedSpyRefreshButton;
			spyRefreshButton.FontSize = ESleekFontSize.Medium;
			spyBox.AddChild(spyRefreshButton);

			spySlayButton = Glazier.Get().CreateButton();
			spySlayButton.PositionOffset_X = 5;
			spySlayButton.PositionOffset_Y = 10;
			spySlayButton.PositionScale_X = 0.5f;
			spySlayButton.PositionScale_Y = 1;
			spySlayButton.SizeOffset_X = 200;
			spySlayButton.SizeOffset_Y = 50;
			spySlayButton.Text = localization.format("Spy_Slay_Button_Text");
			spySlayButton.TooltipText = localization.format("Spy_Slay_Button_Tooltip");
			spySlayButton.OnClicked += onClickedSpySlayButton;
			spySlayButton.FontSize = ESleekFontSize.Medium;
			spyBox.AddChild(spySlayButton);

			serverBox = Glazier.Get().CreateBox();
			serverBox.PositionOffset_Y = -50;
			serverBox.PositionScale_Y = 1f;
			serverBox.SizeOffset_X = -5;
			serverBox.SizeOffset_Y = 50;
			serverBox.SizeScale_X = 0.75f;
			serverBox.FontSize = ESleekFontSize.Medium;
			container.AddChild(serverBox);

			if (Provider.isServer)
			{
				quicksaveButton = Glazier.Get().CreateButton();
				quicksaveButton.PositionScale_X = 0.75f;
				quicksaveButton.PositionOffset_Y = -50;
				quicksaveButton.PositionOffset_X = 5;
				quicksaveButton.PositionScale_Y = 1f;
				quicksaveButton.SizeOffset_X = -5;
				quicksaveButton.SizeOffset_Y = 50;
				quicksaveButton.SizeScale_X = 0.25f;
				quicksaveButton.Text = localization.format("Quicksave_Button");
				quicksaveButton.TooltipText = localization.format("Quicksave_Button_Tooltip");
				quicksaveButton.FontSize = ESleekFontSize.Medium;
				quicksaveButton.OnClicked += onClickedQuicksaveButton;
				container.AddChild(quicksaveButton);

				favoriteButton = null;
				bookmarkButton = null;
				copyServerCodeButton = null;
			}
			else
			{
				quicksaveButton = null;
				favoriteButton = null;
				bookmarkButton = null;
				copyServerCodeButton = null;

				int visibleButtonCount = 1;
				bool canFavorite = Provider.CanFavoriteCurrentServer;
				visibleButtonCount += canFavorite ? 1 : 0;
				bool canBookmark = Provider.CanBookmarkCurrentServer;
				visibleButtonCount += canBookmark ? 1 : 0;

				float buttonsTotalWidth = visibleButtonCount > 1 ? 0.5f : 0.25f;
				float buttonWidth = buttonsTotalWidth / visibleButtonCount;

				serverBox.SizeScale_X = 1.0f - buttonsTotalWidth;

				float buttonPosition = serverBox.SizeScale_X;

				if (canFavorite)
				{
					favoriteButton = new SleekButtonIcon(null);
					favoriteButton.PositionOffset_X = 5;
					favoriteButton.PositionOffset_Y = -50;
					favoriteButton.PositionScale_X = buttonPosition;
					favoriteButton.PositionScale_Y = 1f;
					favoriteButton.SizeOffset_X = -10;
					favoriteButton.SizeOffset_Y = 50;
					favoriteButton.SizeScale_X = buttonWidth;
					favoriteButton.tooltip = localization.format("Favorite_Button_Tooltip");
					favoriteButton.fontSize = ESleekFontSize.Medium;
					favoriteButton.onClickedButton += onClickedFavoriteButton;
					container.AddChild(favoriteButton);
					buttonPosition += buttonWidth;
				}

				if (canBookmark)
				{
					bookmarkButton = new SleekButtonIcon(null, 40);
					bookmarkButton.PositionOffset_X = 5;
					bookmarkButton.PositionOffset_Y = -50;
					bookmarkButton.PositionScale_X = buttonPosition;
					bookmarkButton.PositionScale_Y = 1f;
					bookmarkButton.SizeOffset_X = -10;
					bookmarkButton.SizeOffset_Y = 50;
					bookmarkButton.SizeScale_X = buttonWidth;
					bookmarkButton.tooltip = MenuPlayServerInfoUI.localization.format("Bookmark_Button_Tooltip");
					bookmarkButton.fontSize = ESleekFontSize.Medium;
					bookmarkButton.onClickedButton += OnClickedBookmarkButton;
					container.AddChild(bookmarkButton);
					buttonPosition += buttonWidth;
				}

				copyServerCodeButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Clipboard"), 40);
				copyServerCodeButton.PositionOffset_X = 5;
				copyServerCodeButton.PositionOffset_Y = -50;
				copyServerCodeButton.PositionScale_X = buttonPosition;
				copyServerCodeButton.PositionScale_Y = 1f;
				copyServerCodeButton.SizeOffset_X = -5;
				copyServerCodeButton.SizeOffset_Y = 50;
				copyServerCodeButton.SizeScale_X = buttonWidth;
				copyServerCodeButton.text = MenuPlayServerInfoUI.localization.format("CopyServerCode_Label");
				copyServerCodeButton.tooltip = MenuPlayServerInfoUI.localization.format("CopyServerCode_Tooltip");
				copyServerCodeButton.onClickedButton += OnCopyServerCodeClicked;
				copyServerCodeButton.fontSize = ESleekFontSize.Medium;
				container.AddChild(copyServerCodeButton);
				buttonPosition += buttonWidth;
			}

			new MenuConfigurationOptionsUI();
			new MenuConfigurationDisplayUI();
			new MenuConfigurationGraphicsUI();
			new MenuConfigurationControlsUI();

			audioMenu = new MenuConfigurationAudioUI();
			audioMenu.PositionOffset_X = 10;
			audioMenu.PositionOffset_Y = 10;
			audioMenu.PositionScale_Y = 1;
			audioMenu.SizeOffset_X = -20;
			audioMenu.SizeOffset_Y = -20;
			audioMenu.SizeScale_X = 1;
			audioMenu.SizeScale_Y = 1;
			PlayerUI.container.AddChild(audioMenu);

			if (favoriteButton != null)
			{
				updateFavorite();
			}

			if (bookmarkButton != null)
			{
				UpdateBookmarkButton();
			}

			Player.onSpyReady = onSpyReady;

			ClientMessageHandler_Accepted.OnGameplayConfigReceived += OnGameplayConfigReceived;
			SyncSuicideButtonAvailable();
		}

		internal static MenuConfigurationAudioUI audioMenu;
	}
}
