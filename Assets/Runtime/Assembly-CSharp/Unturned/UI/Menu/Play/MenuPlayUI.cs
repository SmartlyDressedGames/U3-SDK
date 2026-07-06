////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuPlayUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon connectButton;
		private static SleekButtonIcon serversButton;
		private static SleekButtonIcon serverBookmarksButton;
		private static SleekButtonIcon singleplayerButton;
		private static SleekButtonIcon lobbiesButton;
		private static SleekButtonIconConfirm tutorialButton;
		private static SleekButtonIcon backButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			tutorialButton.reset();

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedConnectButton(ISleekElement button)
		{
			onlineSafetyUI.OpenIfNecessary(EOnlineSafetyDestination.Connect);
			close();
		}

		private static void onClickedServersButton(ISleekElement button)
		{
			onlineSafetyUI.OpenIfNecessary(EOnlineSafetyDestination.ServerList);
			close();
		}

		private static void OnClickedServerBookmarksButton(ISleekElement button)
		{
			onlineSafetyUI.OpenIfNecessary(EOnlineSafetyDestination.Bookmarks);
			close();
		}

		private static void onClickedSingleplayerButton(ISleekElement button)
		{
			MenuPlaySingleplayerUI.open();
			close();
		}

		private static void onClickedLobbiesButton(ISleekElement button)
		{
			onlineSafetyUI.OpenIfNecessary(EOnlineSafetyDestination.Lobby);
			close();
		}

		private static void onClickedTutorialButton(ISleekElement button)
		{
			if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Level/Tutorial"))
			{
				ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Level/Tutorial");
			}

			if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user + "_" + Characters.selected + "/Tutorial"))
			{
				ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user + "_" + Characters.selected + "/Tutorial");
			}

			Provider.map = "Tutorial";
			Provider.singleplayer(EGameMode.TUTORIAL, false);
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuDashboardUI.open();
			MenuTitleUI.open();
			close();
		}

		public void OnDestroy()
		{
			connectUI.OnDestroy();
			serverInfoUI.OnDestroy();
			singleplayerUI.OnDestroy();
			lobbiesUI.OnDestroy();
		}

		public MenuPlayUI()
		{
			Local localization = Localization.read("/Menu/Play/MenuPlay.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Play/MenuPlay");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = -1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			float verticalOffset = 0;
			const float spacing = 10;

			ISleekElement centerFrame = Glazier.Get().CreateFrame();
			centerFrame.PositionOffset_X = -100;
			centerFrame.PositionScale_X = 0.5f;
			centerFrame.PositionScale_Y = 0.5f;
			centerFrame.SizeOffset_X = 200;
			container.AddChild(centerFrame);

			tutorialButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Tutorial"),
					localization.format("Tutorial_Confirm_Label"),
					localization.format("Tutorial_Confirm_Tooltip"),
					localization.format("Tutorial_Deny_Label"),
					localization.format("Tutorial_Deny_Tooltip"),
					40);
			tutorialButton.PositionOffset_Y = verticalOffset;
			tutorialButton.SizeOffset_X = 200;
			tutorialButton.SizeOffset_Y = 50;
			tutorialButton.text = localization.format("TutorialButtonText");
			tutorialButton.tooltip = localization.format("TutorialButtonTooltip");
			tutorialButton.onConfirmed += onClickedTutorialButton;
			tutorialButton.fontSize = ESleekFontSize.Medium;
			tutorialButton.iconColor = ESleekTint.FOREGROUND;
			centerFrame.AddChild(tutorialButton);
			verticalOffset += tutorialButton.SizeOffset_Y;
			verticalOffset += spacing;

			singleplayerButton = new SleekButtonIcon(icons.load<Texture2D>("Singleplayer"));
			singleplayerButton.PositionOffset_Y = verticalOffset;
			singleplayerButton.SizeOffset_X = 200;
			singleplayerButton.SizeOffset_Y = 50;
			singleplayerButton.text = localization.format("SingleplayerButtonText");
			singleplayerButton.tooltip = localization.format("SingleplayerButtonTooltip");
			singleplayerButton.onClickedButton += onClickedSingleplayerButton;
			singleplayerButton.iconColor = ESleekTint.FOREGROUND;
			singleplayerButton.fontSize = ESleekFontSize.Medium;
			centerFrame.AddChild(singleplayerButton);
			verticalOffset += singleplayerButton.SizeOffset_Y;
			verticalOffset += spacing;

			serversButton = new SleekButtonIcon(icons.load<Texture2D>("Servers"));
			serversButton.PositionOffset_Y = verticalOffset;
			serversButton.SizeOffset_X = 200;
			serversButton.SizeOffset_Y = 50;
			serversButton.text = localization.format("ServersButtonText");
			serversButton.tooltip = localization.format("ServersButtonTooltip");
			serversButton.iconColor = ESleekTint.FOREGROUND;
			serversButton.onClickedButton += onClickedServersButton;
			serversButton.fontSize = ESleekFontSize.Medium;
			centerFrame.AddChild(serversButton);
			verticalOffset += serversButton.SizeOffset_Y;
			verticalOffset += spacing;

			connectButton = new SleekButtonIcon(icons.load<Texture2D>("Connect"));
			connectButton.PositionOffset_Y = verticalOffset;
			connectButton.SizeOffset_X = 200;
			connectButton.SizeOffset_Y = 50;
			connectButton.text = localization.format("ConnectButtonText");
			connectButton.tooltip = localization.format("ConnectButtonTooltip");
			connectButton.iconColor = ESleekTint.FOREGROUND;
			connectButton.onClickedButton += onClickedConnectButton;
			connectButton.fontSize = ESleekFontSize.Medium;
			centerFrame.AddChild(connectButton);
			verticalOffset += connectButton.SizeOffset_Y;
			verticalOffset += spacing;

			serverBookmarksButton = new SleekButtonIcon(icons.load<Texture2D>("Bookmarks"), 40);
			serverBookmarksButton.PositionOffset_Y = verticalOffset;
			serverBookmarksButton.SizeOffset_X = 200;
			serverBookmarksButton.SizeOffset_Y = 50;
			serverBookmarksButton.text = localization.format("ServerBookmarksButtonText");
			serverBookmarksButton.tooltip = localization.format("ServerBookmarksButtonTooltip");
			serverBookmarksButton.iconColor = ESleekTint.FOREGROUND;
			serverBookmarksButton.onClickedButton += OnClickedServerBookmarksButton;
			serverBookmarksButton.fontSize = ESleekFontSize.Medium;
			centerFrame.AddChild(serverBookmarksButton);
			verticalOffset += serverBookmarksButton.SizeOffset_Y;
			verticalOffset += spacing;

			lobbiesButton = new SleekButtonIcon(icons.load<Texture2D>("Lobbies"));
			lobbiesButton.PositionOffset_Y = verticalOffset;
			lobbiesButton.SizeOffset_X = 200;
			lobbiesButton.SizeOffset_Y = 50;
			lobbiesButton.text = localization.format("LobbiesButtonText");
			lobbiesButton.tooltip = localization.format("LobbiesButtonTooltip");
			lobbiesButton.onClickedButton += onClickedLobbiesButton;
			lobbiesButton.iconColor = ESleekTint.FOREGROUND;
			lobbiesButton.fontSize = ESleekFontSize.Medium;
			centerFrame.AddChild(lobbiesButton);
			verticalOffset += lobbiesButton.SizeOffset_Y;
			verticalOffset += spacing;

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = verticalOffset;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			centerFrame.AddChild(backButton);
			verticalOffset += backButton.SizeOffset_Y;

			centerFrame.SizeOffset_Y = verticalOffset;
			centerFrame.PositionOffset_Y = -(verticalOffset / 2);

#if !UNITY_64
			lobbiesButton.IsVisible = false;
			serversButton.IsVisible = false;
			connectButton.IsVisible = false;
			serverBookmarksButton.IsVisible = false;
#endif // !UNITY_64

			connectUI = new MenuPlayConnectUI();

			serverListUI = new MenuPlayServersUI();
			serverListUI.PositionOffset_X = 10;
			serverListUI.PositionOffset_Y = 10;
			serverListUI.PositionScale_Y = 1;
			serverListUI.SizeOffset_X = -20;
			serverListUI.SizeOffset_Y = -20;
			serverListUI.SizeScale_X = 1;
			serverListUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(serverListUI);

			serverBookmarksUI = new MenuPlayServerBookmarksUI();
			serverBookmarksUI.PositionOffset_X = 10;
			serverBookmarksUI.PositionOffset_Y = 10;
			serverBookmarksUI.PositionScale_Y = 1;
			serverBookmarksUI.SizeOffset_X = -20;
			serverBookmarksUI.SizeOffset_Y = -20;
			serverBookmarksUI.SizeScale_X = 1;
			serverBookmarksUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(serverBookmarksUI);

			onlineSafetyUI = new MenuPlayOnlineSafetyUI();
			onlineSafetyUI.PositionOffset_X = 10;
			onlineSafetyUI.PositionOffset_Y = 10;
			onlineSafetyUI.PositionScale_Y = 1;
			onlineSafetyUI.SizeOffset_X = -20;
			onlineSafetyUI.SizeOffset_Y = -20;
			onlineSafetyUI.SizeScale_X = 1;
			onlineSafetyUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(onlineSafetyUI);

			serverInfoUI = new MenuPlayServerInfoUI();
			singleplayerUI = new MenuPlaySingleplayerUI();
			lobbiesUI = new MenuPlayLobbiesUI();
		}

		private MenuPlayConnectUI connectUI;
		public static MenuPlayServersUI serverListUI;
		public static MenuPlayServerBookmarksUI serverBookmarksUI;
		public static MenuPlayOnlineSafetyUI onlineSafetyUI;
		private MenuPlayServerInfoUI serverInfoUI;
		private MenuPlaySingleplayerUI singleplayerUI;
		private MenuPlayLobbiesUI lobbiesUI;
	}
}
