////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuConfigurationUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon optionsButton;
		private static SleekButtonIcon displayButton;
		private static SleekButtonIcon graphicsButton;
		private static SleekButtonIcon controlsButton;
		private static SleekButtonIcon audioButton;
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

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedOptionsButton(ISleekElement button)
		{
			MenuConfigurationOptionsUI.open();
			close();
		}

		private static void onClickedDisplayButton(ISleekElement button)
		{
			MenuConfigurationDisplayUI.open();
			close();
		}

		private static void onClickedGraphicsButton(ISleekElement button)
		{
			MenuConfigurationGraphicsUI.open();
			close();
		}

		private static void onClickedControlsButton(ISleekElement button)
		{
			MenuConfigurationControlsUI.open();
			close();
		}

		private static void onClickedAudioButton(ISleekElement button)
		{
			audioMenu.open();
			close();
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuDashboardUI.open();
			MenuTitleUI.open();
			close();
		}

		public MenuConfigurationUI()
		{
			Local localization = Localization.read("/Menu/Configuration/MenuConfiguration.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Configuration/MenuConfiguration");

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

			int verticalOffset = -185;

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
			optionsButton.fontSize = ESleekFontSize.Medium;
			optionsButton.iconColor = ESleekTint.FOREGROUND;
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
			displayButton.onClickedButton += onClickedDisplayButton;
			displayButton.fontSize = ESleekFontSize.Medium;
			displayButton.iconColor = ESleekTint.FOREGROUND;
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
			graphicsButton.onClickedButton += onClickedGraphicsButton;
			graphicsButton.fontSize = ESleekFontSize.Medium;
			graphicsButton.iconColor = ESleekTint.FOREGROUND;
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
			controlsButton.onClickedButton += onClickedControlsButton;
			controlsButton.fontSize = ESleekFontSize.Medium;
			controlsButton.iconColor = ESleekTint.FOREGROUND;
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
			audioButton.onClickedButton += onClickedAudioButton;
			audioButton.fontSize = ESleekFontSize.Medium;
			audioButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(audioButton);
			verticalOffset += 60;

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_X = -100;
			backButton.PositionOffset_Y = verticalOffset;
			backButton.PositionScale_X = 0.5f;
			backButton.PositionScale_Y = 0.5f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);
			verticalOffset += 60;

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
			MenuUI.container.AddChild(audioMenu);
		}

		internal static MenuConfigurationAudioUI audioMenu;
	}
}
