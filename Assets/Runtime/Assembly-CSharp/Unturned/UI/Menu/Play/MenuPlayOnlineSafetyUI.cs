////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum EOnlineSafetyDestination
	{
		Connect,
		ServerList,
		Bookmarks,
		Lobby,
	}

	public class MenuPlayOnlineSafetyUI : SleekFullscreenBox
	{
		public Local localization;
		public Bundle icons;
		public bool active;

		private EOnlineSafetyDestination destination;

		private ISleekToggle profanityFilterToggle;
		private ISleekLabel profanityFilter_Header;

		private ISleekToggle inboundVoiceChatToggle;
		private ISleekLabel inboundVoiceChat_Header;

		private ISleekToggle outboundVoiceChatToggle;
		private ISleekLabel outboundVoiceChat_Header;
		private ISleekLabel outboundVoiceChat_Description;

		private ISleekToggle streamerModeToggle;
		private ISleekLabel streamerMode_Header;

		private ISleekToggle dontShowAgainToggle;

		public void OpenIfNecessary(EOnlineSafetyDestination destination)
		{
			if (OptionsSettings.ShouldShowOnlineSafetyMenu)
			{
				open(destination);
			}
			else
			{
				this.destination = destination;
				ProceedToDestination();
			}
		}

		public void open(EOnlineSafetyDestination destination)
		{
			if (active)
			{
				return;
			}

			active = true;
			this.destination = destination;

			SynchronizeValues();

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

		private void ProceedToDestination()
		{
			switch (destination)
			{
				case EOnlineSafetyDestination.Connect:
					MenuPlayConnectUI.open();
					break;

				case EOnlineSafetyDestination.ServerList:
					MenuPlayUI.serverListUI.open(true);
					break;

				case EOnlineSafetyDestination.Bookmarks:
					MenuPlayUI.serverBookmarksUI.open();
					break;

				case EOnlineSafetyDestination.Lobby:
					MenuPlayLobbiesUI.open();
					break;
			}
		}

		private void OnBackClicked(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
		}

		private void OnContinueClicked(ISleekElement button)
		{
			OptionsSettings.onlineSafetyMenuProceedCount += 1;
			OptionsSettings.didProceedThroughOnlineSafetyMenuThisSession = true;
			ProceedToDestination();
			close();
		}

		private void OnProfanityFilterToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.filter = state;
			SynchronizeValues();
		}

		private void OnInboundVoiceChatToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.chatVoiceIn = state;
			OptionsSettings.EnableOutboundVoiceChat &= state; // Turn off outbound if inbound is turned off.
			SynchronizeValues();
		}

		private void OnOutboundVoiceChatToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.EnableOutboundVoiceChat = state;
			SynchronizeValues();
		}

		private void OnStreamerModeToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.ShouldAnonymizeMultiplayerDetails = state;
			OptionsSettings.ShouldHideRichPresence = state;
			SynchronizeValues();
		}

		private void OnDontShowAgainToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.wantsToHideOnlineSafetyMenu = state;
		}

		private void SynchronizeValues()
		{
			profanityFilterToggle.Value = OptionsSettings.filter;
			profanityFilter_Header.Text = localization.format("ProfanityFilter_Header",
				localization.format(OptionsSettings.filter ? "Feature_On" : "Feature_Off"));

			inboundVoiceChatToggle.Value = OptionsSettings.chatVoiceIn;
			inboundVoiceChat_Header.Text = localization.format("InboundVoiceChat_Header",
				localization.format(OptionsSettings.chatVoiceIn ? "Feature_On" : "Feature_Off"));

			outboundVoiceChatToggle.Value = OptionsSettings.EnableOutboundVoiceChat;
			outboundVoiceChat_Header.Text = localization.format("OutboundVoiceChat_Header",
				localization.format(OptionsSettings.EnableOutboundVoiceChat ? "Feature_On" : "Feature_Off"));
			outboundVoiceChat_Description.Text = localization.format("OutboundVoiceChat_Description", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.voice));
			outboundVoiceChatToggle.IsInteractable = OptionsSettings.chatVoiceIn;
			outboundVoiceChat_Header.TextColor = new SleekColor(ESleekTint.FONT, OptionsSettings.chatVoiceIn ? 1.0f : 0.5f);
			outboundVoiceChat_Description.TextColor = new SleekColor(ESleekTint.RICH_TEXT_DEFAULT, OptionsSettings.chatVoiceIn ? 1.0f : 0.5f);

			bool streamerMode = OptionsSettings.ShouldAnonymizeMultiplayerDetails && OptionsSettings.ShouldHideRichPresence;
			streamerModeToggle.Value = streamerMode;
			streamerMode_Header.Text = localization.format("StreamerMode_Header",
				localization.format(streamerMode ? "Feature_On" : "Feature_Off"));

			dontShowAgainToggle.Value = OptionsSettings.wantsToHideOnlineSafetyMenu;
		}

		public MenuPlayOnlineSafetyUI()
		{
			active = false;

			localization = Localization.read("/Menu/Play/MenuPlayOnlineSafety.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Play/MenuPlayOnlineSafety");

			ISleekBox backgroundBox = Glazier.Get().CreateBox();
			backgroundBox.SizeScale_X = 1.0f;
			backgroundBox.SizeScale_Y = 1.0f;
			backgroundBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
			AddChild(backgroundBox);

			ISleekScrollView scrollView = Glazier.Get().CreateScrollView();
			scrollView.PositionOffset_X = -380;
			scrollView.PositionScale_X = 0.5f;
			scrollView.PositionScale_Y = 0.1f;
			scrollView.SizeOffset_X = 790;
			scrollView.SizeScale_Y = 0.8f;
			scrollView.ScaleContentToWidth = true;
			AddChild(scrollView);

			float verticalOffset = 0;

			ISleekImage alertImage = Glazier.Get().CreateImage(icons.load<Texture2D>("OnlineSafetyAlert"));
			alertImage.PositionScale_X = 0.5f;
			alertImage.PositionOffset_X = -64;
			alertImage.PositionOffset_Y = verticalOffset;
			alertImage.SizeOffset_X = 128;
			alertImage.SizeOffset_Y = 128;
			alertImage.TintColor = ESleekTint.FOREGROUND;
			scrollView.AddChild(alertImage);
			verticalOffset += 128;

			ISleekLabel headerLabel = Glazier.Get().CreateLabel();
			headerLabel.PositionOffset_Y = verticalOffset;
			headerLabel.SizeScale_X = 1.0f;
			headerLabel.SizeOffset_Y = 50;
			headerLabel.Text = localization.format("Header");
			headerLabel.FontSize = ESleekFontSize.Large;
			headerLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(headerLabel);
			verticalOffset += 40;

			ISleekLabel warningLabel = Glazier.Get().CreateLabel();
			warningLabel.PositionOffset_Y = verticalOffset;
			warningLabel.SizeScale_X = 1.0f;
			warningLabel.SizeOffset_Y = 70;
			warningLabel.Text = localization.format("Warning");
			warningLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(warningLabel);
			verticalOffset += warningLabel.SizeOffset_Y + 10;

			profanityFilterToggle = Glazier.Get().CreateToggle();
			profanityFilterToggle.PositionOffset_X = -240;
			profanityFilterToggle.PositionOffset_Y = verticalOffset;
			profanityFilterToggle.PositionScale_X = 0.5f;
			profanityFilterToggle.SizeOffset_X = 40;
			profanityFilterToggle.SizeOffset_Y = 40;
			profanityFilterToggle.OnValueChanged += OnProfanityFilterToggled;
			scrollView.AddChild(profanityFilterToggle);

			profanityFilter_Header = Glazier.Get().CreateLabel();
			profanityFilter_Header.PositionOffset_X = -190;
			profanityFilter_Header.PositionOffset_Y = verticalOffset - 10;
			profanityFilter_Header.PositionScale_X = 0.5f;
			profanityFilter_Header.SizeOffset_X = 400;
			profanityFilter_Header.SizeOffset_Y = 30;
			profanityFilter_Header.TextAlignment = TextAnchor.LowerLeft;
			profanityFilter_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(profanityFilter_Header);

			ISleekLabel profanityFilter_Description = Glazier.Get().CreateLabel();
			profanityFilter_Description.PositionOffset_X = -190;
			profanityFilter_Description.PositionOffset_Y = verticalOffset + 20;
			profanityFilter_Description.PositionScale_X = 0.5f;
			profanityFilter_Description.SizeOffset_X = 400;
			profanityFilter_Description.SizeOffset_Y = 50;
			profanityFilter_Description.TextAlignment = TextAnchor.UpperLeft;
			profanityFilter_Description.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			profanityFilter_Description.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			profanityFilter_Description.Text = localization.format("ProfanityFilter_Description");
			scrollView.AddChild(profanityFilter_Description);

			verticalOffset += 60;

			inboundVoiceChatToggle = Glazier.Get().CreateToggle();
			inboundVoiceChatToggle.PositionOffset_X = -240;
			inboundVoiceChatToggle.PositionOffset_Y = verticalOffset;
			inboundVoiceChatToggle.PositionScale_X = 0.5f;
			inboundVoiceChatToggle.SizeOffset_X = 40;
			inboundVoiceChatToggle.SizeOffset_Y = 40;
			inboundVoiceChatToggle.OnValueChanged += OnInboundVoiceChatToggled;
			scrollView.AddChild(inboundVoiceChatToggle);

			inboundVoiceChat_Header = Glazier.Get().CreateLabel();
			inboundVoiceChat_Header.PositionOffset_X = -190;
			inboundVoiceChat_Header.PositionOffset_Y = verticalOffset - 10;
			inboundVoiceChat_Header.PositionScale_X = 0.5f;
			inboundVoiceChat_Header.SizeOffset_X = 400;
			inboundVoiceChat_Header.SizeOffset_Y = 30;
			inboundVoiceChat_Header.TextAlignment = TextAnchor.LowerLeft;
			inboundVoiceChat_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(inboundVoiceChat_Header);

			ISleekLabel inboundVoiceChat_Description = Glazier.Get().CreateLabel();
			inboundVoiceChat_Description.PositionOffset_X = -190;
			inboundVoiceChat_Description.PositionOffset_Y = verticalOffset + 20;
			inboundVoiceChat_Description.PositionScale_X = 0.5f;
			inboundVoiceChat_Description.SizeOffset_X = 400;
			inboundVoiceChat_Description.SizeOffset_Y = 50;
			inboundVoiceChat_Description.TextAlignment = TextAnchor.UpperLeft;
			inboundVoiceChat_Description.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			inboundVoiceChat_Description.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			inboundVoiceChat_Description.Text = localization.format("InboundVoiceChat_Description");
			scrollView.AddChild(inboundVoiceChat_Description);

			verticalOffset += 60;

			outboundVoiceChatToggle = Glazier.Get().CreateToggle();
			outboundVoiceChatToggle.PositionOffset_X = -240;
			outboundVoiceChatToggle.PositionOffset_Y = verticalOffset;
			outboundVoiceChatToggle.PositionScale_X = 0.5f;
			outboundVoiceChatToggle.SizeOffset_X = 40;
			outboundVoiceChatToggle.SizeOffset_Y = 40;
			outboundVoiceChatToggle.OnValueChanged += OnOutboundVoiceChatToggled;
			scrollView.AddChild(outboundVoiceChatToggle);

			outboundVoiceChat_Header = Glazier.Get().CreateLabel();
			outboundVoiceChat_Header.PositionOffset_X = -190;
			outboundVoiceChat_Header.PositionOffset_Y = verticalOffset - 10;
			outboundVoiceChat_Header.PositionScale_X = 0.5f;
			outboundVoiceChat_Header.SizeOffset_X = 400;
			outboundVoiceChat_Header.SizeOffset_Y = 30;
			outboundVoiceChat_Header.TextAlignment = TextAnchor.LowerLeft;
			outboundVoiceChat_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(outboundVoiceChat_Header);

			outboundVoiceChat_Description = Glazier.Get().CreateLabel();
			outboundVoiceChat_Description.PositionOffset_X = -190;
			outboundVoiceChat_Description.PositionOffset_Y = verticalOffset + 20;
			outboundVoiceChat_Description.PositionScale_X = 0.5f;
			outboundVoiceChat_Description.SizeOffset_X = 400;
			outboundVoiceChat_Description.SizeOffset_Y = 50;
			outboundVoiceChat_Description.TextAlignment = TextAnchor.UpperLeft;
			outboundVoiceChat_Description.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			outboundVoiceChat_Description.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(outboundVoiceChat_Description);

			verticalOffset += 60;

			streamerModeToggle = Glazier.Get().CreateToggle();
			streamerModeToggle.PositionOffset_X = -240;
			streamerModeToggle.PositionOffset_Y = verticalOffset;
			streamerModeToggle.PositionScale_X = 0.5f;
			streamerModeToggle.SizeOffset_X = 40;
			streamerModeToggle.SizeOffset_Y = 40;
			streamerModeToggle.OnValueChanged += OnStreamerModeToggled;
			scrollView.AddChild(streamerModeToggle);

			streamerMode_Header = Glazier.Get().CreateLabel();
			streamerMode_Header.PositionOffset_X = -190;
			streamerMode_Header.PositionOffset_Y = verticalOffset - 10;
			streamerMode_Header.PositionScale_X = 0.5f;
			streamerMode_Header.SizeOffset_X = 400;
			streamerMode_Header.SizeOffset_Y = 30;
			streamerMode_Header.TextAlignment = TextAnchor.LowerLeft;
			streamerMode_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			scrollView.AddChild(streamerMode_Header);

			ISleekLabel streamerMode_Description = Glazier.Get().CreateLabel();
			streamerMode_Description.PositionOffset_X = -190;
			streamerMode_Description.PositionOffset_Y = verticalOffset + 20;
			streamerMode_Description.PositionScale_X = 0.5f;
			streamerMode_Description.SizeOffset_X = 400;
			streamerMode_Description.SizeOffset_Y = 50;
			streamerMode_Description.TextAlignment = TextAnchor.UpperLeft;
			streamerMode_Description.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			streamerMode_Description.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			streamerMode_Description.Text = localization.format("StreamerMode_Description");
			scrollView.AddChild(streamerMode_Description);

			verticalOffset += 60;

			ISleekLabel optionsNoteLabel = Glazier.Get().CreateLabel();
			optionsNoteLabel.PositionOffset_Y = verticalOffset;
			optionsNoteLabel.SizeScale_X = 1.0f;
			optionsNoteLabel.SizeOffset_Y = 30;
			optionsNoteLabel.Text = localization.format("OptionsNote");
			optionsNoteLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			optionsNoteLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			scrollView.AddChild(optionsNoteLabel);
			verticalOffset += optionsNoteLabel.SizeOffset_Y + 10;

			SleekButtonIcon backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_X = -205;
			backButton.PositionOffset_Y = verticalOffset;
			backButton.PositionScale_X = 0.5f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += OnBackClicked;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			scrollView.AddChild(backButton);

			SleekButtonIcon continueButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Play"));
			continueButton.PositionOffset_X = 5;
			continueButton.PositionOffset_Y = verticalOffset;
			continueButton.PositionScale_X = 0.5f;
			continueButton.SizeOffset_X = 200;
			continueButton.SizeOffset_Y = 50;
			continueButton.text = localization.format("ContinueButton_Label");
			continueButton.tooltip = localization.format("ContinueButton_Tooltip");
			continueButton.onClickedButton += OnContinueClicked;
			continueButton.fontSize = ESleekFontSize.Medium;
			continueButton.iconColor = ESleekTint.FOREGROUND;
			scrollView.AddChild(continueButton);

			verticalOffset += 60;

			dontShowAgainToggle = Glazier.Get().CreateToggle();
			dontShowAgainToggle.PositionOffset_X = 5;
			dontShowAgainToggle.PositionOffset_Y = verticalOffset;
			dontShowAgainToggle.PositionScale_X = 0.5f;
			dontShowAgainToggle.SizeOffset_X = 40;
			dontShowAgainToggle.SizeOffset_Y = 40;
			dontShowAgainToggle.AddLabel(localization.format("DontShowAgain_Label"), ESleekSide.RIGHT);
			dontShowAgainToggle.TooltipText = localization.format("DontShowAgain_Tooltip");
			dontShowAgainToggle.OnValueChanged += OnDontShowAgainToggled;
			scrollView.AddChild(dontShowAgainToggle);
			verticalOffset += 50;

			scrollView.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10.0f);
		}
	}
}
