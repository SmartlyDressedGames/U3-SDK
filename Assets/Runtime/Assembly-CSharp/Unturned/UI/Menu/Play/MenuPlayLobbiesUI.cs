////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuPlayLobbiesUI
	{
		public class SleekLobbyPlayerButton : SleekWrapper
		{
			private CSteamID steamID;

			private ISleekButton button;
			private ISleekImage avatarImage;
			private ISleekLabel nameLabel;

			private void onClickedPlayerButton(ISleekElement button)
			{
				Provider.provider.browserService.open("https://steamcommunity.com/profiles/" + steamID);
			}

			public SleekLobbyPlayerButton(CSteamID newSteamID) : base()
			{
				steamID = newSteamID;

				button = Glazier.Get().CreateButton();
				button.SizeScale_X = 1;
				button.SizeScale_Y = 1;
				button.OnClicked += onClickedPlayerButton;
				AddChild(button);

				avatarImage = Glazier.Get().CreateImage();
				avatarImage.PositionOffset_X = 9;
				avatarImage.PositionOffset_Y = 9;
				avatarImage.SizeOffset_X = 32;
				avatarImage.SizeOffset_Y = 32;
				avatarImage.Texture = Provider.provider.communityService.getIcon(steamID);
				avatarImage.ShouldDestroyTexture = true;
				button.AddChild(avatarImage);

				nameLabel = Glazier.Get().CreateLabel();
				nameLabel.PositionOffset_X = 40;
				nameLabel.SizeOffset_X = -40;
				nameLabel.SizeScale_X = 1;
				nameLabel.SizeScale_Y = 1;
				nameLabel.Text = SteamFriends.GetFriendPersonaName(steamID);
				nameLabel.FontSize = ESleekFontSize.Medium;
				button.AddChild(nameLabel);
			}
		}

		public static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekLabel membersLabel;
		private static ISleekScrollView membersBox;
		private static SleekButtonIcon inviteButton;
		private static ISleekLabel waitingLabel;

		private static SleekButtonIcon backButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			if (Lobbies.inLobby)
			{
				setWaitingForLobby(false);
				refresh();
			}
			else
			{
				setWaitingForLobby(true);
				Lobbies.createLobby();
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

			container.AnimateOutOfView(0, 1);
		}

		private static void refresh()
		{
			membersBox.RemoveAllChildren();

			int memberCount = Lobbies.getLobbyMemberCount();
			for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
			{
				CSteamID memberID = Lobbies.getLobbyMemberByIndex(memberIndex);

				SleekLobbyPlayerButton memberButton = new SleekLobbyPlayerButton(memberID);
				memberButton.PositionOffset_Y = memberIndex * 50;
				memberButton.SizeOffset_Y = 50;
				memberButton.SizeScale_X = 1;
				membersBox.AddChild(memberButton);
			}

			membersBox.ContentSizeOffset = new Vector2(0.0f, memberCount * 50);
		}

		private static void handleLobbiesRefreshed()
		{
			if (!active)
			{
				return;
			}

			refresh();
		}

		private static void handleLobbiesEntered()
		{
			if (active)
			{
				setWaitingForLobby(false);
				return;
			}

			MenuUI.closeAll();
			open();
		}

		private static void onClickedInviteButton(ISleekElement button)
		{
			if (!Lobbies.canOpenInvitations)
			{
				MenuUI.alert(localization.format("Overlay"));
				return;
			}

			Lobbies.openInvitations();
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
		}

		private static void setWaitingForLobby(bool waiting)
		{
			inviteButton.isClickable = !waiting;
			waitingLabel.IsVisible = waiting;
		}

		public void OnDestroy()
		{
			Lobbies.lobbiesRefreshed -= handleLobbiesRefreshed;
			Lobbies.lobbiesEntered -= handleLobbiesEntered;
		}

		public MenuPlayLobbiesUI()
		{
			localization = Localization.read("/Menu/Play/MenuPlayLobbies.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Play/MenuPlayLobbies");

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

			membersLabel = Glazier.Get().CreateLabel();
			membersLabel.PositionOffset_X = -200;
			membersLabel.PositionOffset_Y = 100;
			membersLabel.PositionScale_X = 0.5f;
			membersLabel.SizeOffset_X = 400;
			membersLabel.SizeOffset_Y = 50;
			membersLabel.Text = localization.format("Members");
			membersLabel.FontSize = ESleekFontSize.Medium;
			container.AddChild(membersLabel);

			membersBox = Glazier.Get().CreateScrollView();
			membersBox.PositionOffset_X = -200;
			membersBox.PositionOffset_Y = 150;
			membersBox.PositionScale_X = 0.5f;
			membersBox.SizeOffset_X = 430;
			membersBox.SizeOffset_Y = -300;
			membersBox.SizeScale_Y = 1;
			membersBox.ScaleContentToWidth = true;
			container.AddChild(membersBox);

			inviteButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Invite"), 40);
			inviteButton.PositionOffset_X = -200;
			inviteButton.PositionOffset_Y = -150;
			inviteButton.PositionScale_X = 0.5f;
			inviteButton.PositionScale_Y = 1;
			inviteButton.SizeOffset_X = 400;
			inviteButton.SizeOffset_Y = 50;
			inviteButton.text = localization.format("Invite_Button");
			inviteButton.tooltip = localization.format("Invite_Button_Tooltip");
			inviteButton.onClickedButton += onClickedInviteButton;
			inviteButton.fontSize = ESleekFontSize.Medium;
			inviteButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(inviteButton);

			waitingLabel = Glazier.Get().CreateLabel();
			waitingLabel.PositionOffset_X = -200;
			waitingLabel.PositionOffset_Y = -200;
			waitingLabel.PositionScale_X = 0.5f;
			waitingLabel.PositionScale_Y = 1;
			waitingLabel.SizeOffset_X = 400;
			waitingLabel.SizeOffset_Y = 50;
			waitingLabel.Text = localization.format("Waiting");
			waitingLabel.IsVisible = false;
			container.AddChild(waitingLabel);

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

			Lobbies.lobbiesRefreshed += handleLobbiesRefreshed;
			Lobbies.lobbiesEntered += handleLobbiesEntered;
		}
	}
}
