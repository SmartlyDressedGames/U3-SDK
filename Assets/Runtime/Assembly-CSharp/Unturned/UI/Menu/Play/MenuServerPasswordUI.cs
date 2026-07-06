////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuServerPasswordUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool isActive;

		public static void open(SteamServerAdvertisement newServerInfo, List<PublishedFileId_t> newExpectedWorkshopItems)
		{
			if (isActive)
			{
				return;
			}

			isActive = true;

			container.AnimateIntoView();

			serverInfo = newServerInfo;
			expectedWorkshopItems = newExpectedWorkshopItems;

			connectButton.IsClickable = false;
			passwordField.Text = string.Empty;
			passwordField.IsPasswordField = true;
			showPasswordToggle.Value = false;
		}

		public static void close()
		{
			if (!isActive)
			{
				return;
			}

			isActive = false;

			container.AnimateOutOfView(0, 1);
		}

		public MenuServerPasswordUI()
		{
			localization = Localization.read("/Menu/Play/MenuServerPassword.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			isActive = false;

			explanationLabel = Glazier.Get().CreateLabel();
			explanationLabel.PositionOffset_Y = -75;
			explanationLabel.PositionScale_X = 0.25f;
			explanationLabel.PositionScale_Y = 0.5f;
			explanationLabel.SizeScale_X = 0.5f;
			explanationLabel.SizeOffset_Y = 30;
			explanationLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			explanationLabel.Text = localization.format("Explanation");
			container.AddChild(explanationLabel);

			passwordField = Glazier.Get().CreateStringField();
			passwordField.PositionOffset_X = -100;
			passwordField.PositionOffset_Y = -35;
			passwordField.PositionScale_X = 0.5f;
			passwordField.PositionScale_Y = 0.5f;
			passwordField.SizeOffset_X = 200;
			passwordField.SizeOffset_Y = 30;
			passwordField.AddLabel(localization.format("Password_Label"), ESleekSide.RIGHT);
			passwordField.IsPasswordField = true;
			passwordField.MaxLength = 0; // Disable
			passwordField.OnTextChanged += OnTypedPasswordField;
			passwordField.OnTextSubmitted += OnPasswordFieldSubmitted;
			container.AddChild(passwordField);

			showPasswordToggle = Glazier.Get().CreateToggle();
			showPasswordToggle.PositionOffset_X = -100;
			showPasswordToggle.PositionOffset_Y = 5;
			showPasswordToggle.PositionScale_X = 0.5f;
			showPasswordToggle.PositionScale_Y = 0.5f;
			showPasswordToggle.SizeOffset_X = 40;
			showPasswordToggle.SizeOffset_Y = 40;
			showPasswordToggle.OnValueChanged += OnToggledShowPassword;
			showPasswordToggle.AddLabel(localization.format("Show_Password_Label"), ESleekSide.RIGHT);
			container.AddChild(showPasswordToggle);

			connectButton = Glazier.Get().CreateButton();
			connectButton.PositionOffset_X = -100;
			connectButton.PositionOffset_Y = 55;
			connectButton.PositionScale_X = 0.5f;
			connectButton.PositionScale_Y = 0.5f;
			connectButton.SizeOffset_X = 200;
			connectButton.SizeOffset_Y = 30;
			connectButton.Text = localization.format("Connect_Button");
			connectButton.TooltipText = localization.format("Connect_Button");
			connectButton.OnClicked += OnClickedConnectButton;
			container.AddChild(connectButton);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += OnClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);
		}

		private static void OnClickedConnectButton(ISleekElement button)
		{
			if (!string.IsNullOrEmpty(passwordField.Text))
			{
				ServerConnectParameters connectParameters = new ServerConnectParameters(new global::Unturned.SystemEx.IPv4Address(serverInfo.ip), serverInfo.queryPort, serverInfo.connectionPort, passwordField.Text);
				Provider.connect(connectParameters, serverInfo, expectedWorkshopItems);
			}
		}

		private static void OnToggledShowPassword(ISleekToggle toggle, bool show)
		{
			passwordField.IsPasswordField = !show;
		}

		private static void OnTypedPasswordField(ISleekField field, string text)
		{
			connectButton.IsClickable = !string.IsNullOrEmpty(text);
		}

		private static void OnPasswordFieldSubmitted(ISleekField field)
		{
			OnClickedConnectButton(connectButton);
		}

		private static void OnClickedBackButton(ISleekElement button)
		{
			MenuPlayServerInfoUI.OpenWithoutRefresh();
			close();
		}

		private static SteamServerAdvertisement serverInfo;
		private static List<PublishedFileId_t> expectedWorkshopItems;

		private static SleekButtonIcon backButton;
		private static ISleekLabel explanationLabel;
		private static ISleekField passwordField;
		private static ISleekToggle showPasswordToggle;
		private static ISleekButton connectButton;
	}
}
