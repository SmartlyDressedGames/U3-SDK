////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;


namespace SDG.Unturned
{
	public class MenuPauseUI
	{
		class CustomMenuLinkButton : SleekButtonIcon
		{
			public string url;

			public void OnClickedLink(ISleekElement element)
			{
				Provider.provider.browserService.open(url);
			}

			public CustomMenuLinkButton(Texture2D icon, int newSize) : base(icon, newSize)
			{
				onClickedButton += OnClickedLink;
			}
		}

		public static Local localization;
		public static IconsBundle icons;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon returnButton;
		private static SleekButtonIconConfirm quitButton;
		private static SleekButtonIcon creditsButton;

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

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedReturnButton(ISleekElement button)
		{
			close();
			MenuDashboardUI.open();
			MenuTitleUI.open();
		}

		private static void onClickedQuitButton(SleekButtonIconConfirm button)
		{
			Provider.QuitGame("clicked quit in main menu");
		}

		private static void onClickedCreditsButton(ISleekElement button)
		{
			close();
			MenuCreditsUI.open();
		}

		public MenuPauseUI()
		{
			localization = Localization.read("/Menu/MenuPause.dat");

			icons = Bundles.getIconsBundle("UI/Menu/Icons/MenuPause");

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

			ISleekElement buttonsContainer = Glazier.Get().CreateFrame();
			buttonsContainer.PositionScale_X = 0.5f;
			buttonsContainer.PositionScale_Y = 0.5f;

			int offset = 0;

			quitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Quit"), localization.format("Exit_Button"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button"), string.Empty);
			quitButton.PositionOffset_X = -100;
			quitButton.PositionOffset_Y = offset;
			quitButton.PositionScale_X = 0.5f;
			quitButton.SizeOffset_X = 200;
			quitButton.SizeOffset_Y = 50;
			quitButton.text = localization.format("Exit_Button");
			quitButton.tooltip = localization.format("Exit_Button_Tooltip");
			quitButton.onConfirmed += onClickedQuitButton;
			quitButton.fontSize = ESleekFontSize.Medium;
			quitButton.iconColor = ESleekTint.FOREGROUND;
			buttonsContainer.AddChild(quitButton);
			offset += 60;

			returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
			returnButton.PositionOffset_X = -100;
			returnButton.PositionOffset_Y = offset;
			returnButton.PositionScale_X = 0.5f;
			returnButton.SizeOffset_X = 200;
			returnButton.SizeOffset_Y = 50;
			returnButton.text = localization.format("Return_Button");
			returnButton.tooltip = localization.format("Return_Button_Tooltip");
			returnButton.onClickedButton += onClickedReturnButton;
			returnButton.fontSize = ESleekFontSize.Medium;
			returnButton.iconColor = ESleekTint.FOREGROUND;
			buttonsContainer.AddChild(returnButton);
			offset += 60;

			foreach (CustomMenuLink customMenuLink in Provider.statusData.Menu.Custom_Menu_Links)
			{
				CustomMenuLinkButton supportButton = new CustomMenuLinkButton(icons.load<Texture2D>(customMenuLink.Icon), 40);
				supportButton.PositionOffset_X = -100;
				supportButton.PositionOffset_Y = offset;
				supportButton.PositionScale_X = 0.5f;
				supportButton.SizeOffset_X = 200;
				supportButton.SizeOffset_Y = 50;
				supportButton.text = localization.format(customMenuLink.Label_Key);
				supportButton.tooltip = localization.format(customMenuLink.Tooltip_Key);
				supportButton.url = customMenuLink.Web_Link;
				supportButton.fontSize = ESleekFontSize.Medium;
				supportButton.iconColor = ESleekTint.FOREGROUND;
				buttonsContainer.AddChild(supportButton);
				offset += 60;
			}

			creditsButton = new SleekButtonIcon(icons.load<Texture2D>("Credits"));
			creditsButton.PositionOffset_X = -100;
			creditsButton.PositionOffset_Y = offset;
			creditsButton.PositionScale_X = 0.5f;
			creditsButton.SizeOffset_X = 200;
			creditsButton.SizeOffset_Y = 50;
			creditsButton.text = localization.format("Credits_Button");
			creditsButton.tooltip = localization.format("Credits_Button_Tooltip");
			creditsButton.onClickedButton += onClickedCreditsButton;
			creditsButton.fontSize = ESleekFontSize.Medium;
			creditsButton.iconColor = ESleekTint.FOREGROUND;
			buttonsContainer.AddChild(creditsButton);
			offset += 60;

			buttonsContainer.SizeOffset_Y = offset - 10;
			buttonsContainer.PositionOffset_Y = buttonsContainer.SizeOffset_Y * -0.5f;
			container.AddChild(buttonsContainer);
		}
	}
}
