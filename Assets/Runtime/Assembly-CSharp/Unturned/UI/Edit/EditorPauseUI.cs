////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorPauseUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon saveButton;
		private static SleekButtonIcon mapButton;
		private static SleekButtonIconConfirm exitButton;
		private static SleekButtonIconConfirm quitButton;
		private static ISleekUInt16Field legacyIDField;
		private static ISleekButton legacyButton;
		private static ISleekUInt16Field proxyIDField;
		private static ISleekButton proxyButton;
		private static SleekButtonIcon chartButton;
		private static SleekButtonIcon optionsButton;
		private static SleekButtonIcon displayButton;
		private static SleekButtonIcon graphicsButton;
		private static SleekButtonIcon controlsButton;
		private static SleekButtonIcon audioButton;

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

			exitButton.reset();
			quitButton.reset();

			container.AnimateOutOfView(1, 0);
		}

		private static void onClickedSaveButton(ISleekElement button)
		{
			Level.save();
		}

		private static void onClickedMapButton(ISleekElement button)
		{
			Level.CaptureSatelliteImage();
		}

		private static void onClickedChartButton(ISleekElement button)
		{
			Level.CaptureChartImage();
		}

		private static void onClickedLegacyButton(ISleekElement button)
		{
			ushort id = legacyIDField.Value;
			if (id == 0)
			{
				return;
			}

			SpawnTableTool.export(id, true);
		}

		private static void onClickedProxyButton(ISleekElement button)
		{
			ushort id = proxyIDField.Value;
			if (id == 0)
			{
				return;
			}

			SpawnTableTool.export(id, false);
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

		private static void onClickedExitButton(SleekButtonIconConfirm button)
		{
			Level.exit();
		}

		private static void onClickedQuitButton(SleekButtonIconConfirm button)
		{
			Provider.QuitGame("clicked quit in level editor");
		}

		public EditorPauseUI()
		{
			Local localization = Localization.read("/Editor/EditorPause.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorPause");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_X = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);
			active = false;

			saveButton = new SleekButtonIcon(icons.load<Texture2D>("Save"));
			saveButton.PositionOffset_X = -100;
			saveButton.PositionOffset_Y = -115;
			saveButton.PositionScale_X = 0.5f;
			saveButton.PositionScale_Y = 0.5f;
			saveButton.SizeOffset_X = 200;
			saveButton.SizeOffset_Y = 30;
			saveButton.text = localization.format("Save_Button");
			saveButton.tooltip = localization.format("Save_Button_Tooltip");
			saveButton.onClickedButton += onClickedSaveButton;
			container.AddChild(saveButton);

			mapButton = new SleekButtonIcon(icons.load<Texture2D>("Map"));
			mapButton.PositionOffset_X = -100;
			mapButton.PositionOffset_Y = -75;
			mapButton.PositionScale_X = 0.5f;
			mapButton.PositionScale_Y = 0.5f;
			mapButton.SizeOffset_X = 200;
			mapButton.SizeOffset_Y = 30;
			mapButton.text = localization.format("Map_Button");
			mapButton.tooltip = localization.format("Map_Button_Tooltip");
			mapButton.onClickedButton += onClickedMapButton;
			container.AddChild(mapButton);

			chartButton = new SleekButtonIcon(icons.load<Texture2D>("Chart"));
			chartButton.PositionOffset_X = -100;
			chartButton.PositionOffset_Y = -35;
			chartButton.PositionScale_X = 0.5f;
			chartButton.PositionScale_Y = 0.5f;
			chartButton.SizeOffset_X = 200;
			chartButton.SizeOffset_Y = 30;
			chartButton.text = localization.format("Chart_Button");
			chartButton.tooltip = localization.format("Chart_Button_Tooltip");
			chartButton.onClickedButton += onClickedChartButton;
			container.AddChild(chartButton);

			legacyIDField = Glazier.Get().CreateUInt16Field();
			legacyIDField.PositionOffset_X = -100;
			legacyIDField.PositionOffset_Y = 5;
			legacyIDField.PositionScale_X = 0.5f;
			legacyIDField.PositionScale_Y = 0.5f;
			legacyIDField.SizeOffset_X = 50;
			legacyIDField.SizeOffset_Y = 30;
			container.AddChild(legacyIDField);

			legacyButton = Glazier.Get().CreateButton();
			legacyButton.PositionOffset_X = -40;
			legacyButton.PositionOffset_Y = 5;
			legacyButton.PositionScale_X = 0.5f;
			legacyButton.PositionScale_Y = 0.5f;
			legacyButton.SizeOffset_X = 140;
			legacyButton.SizeOffset_Y = 30;
			legacyButton.Text = localization.format("Legacy_Spawns");
			legacyButton.TooltipText = localization.format("Legacy_Spawns_Tooltip");
			legacyButton.OnClicked += onClickedLegacyButton;
			container.AddChild(legacyButton);

			proxyIDField = Glazier.Get().CreateUInt16Field();
			proxyIDField.PositionOffset_X = -100;
			proxyIDField.PositionOffset_Y = 45;
			proxyIDField.PositionScale_X = 0.5f;
			proxyIDField.PositionScale_Y = 0.5f;
			proxyIDField.SizeOffset_X = 50;
			proxyIDField.SizeOffset_Y = 30;
			container.AddChild(proxyIDField);

			proxyButton = Glazier.Get().CreateButton();
			proxyButton.PositionOffset_X = -40;
			proxyButton.PositionOffset_Y = 45;
			proxyButton.PositionScale_X = 0.5f;
			proxyButton.PositionScale_Y = 0.5f;
			proxyButton.SizeOffset_X = 140;
			proxyButton.SizeOffset_Y = 30;
			proxyButton.Text = localization.format("Proxy_Spawns");
			proxyButton.TooltipText = localization.format("Proxy_Spawns_Tooltip");
			proxyButton.OnClicked += onClickedProxyButton;
			container.AddChild(proxyButton);

			Local playerPauseLocalization = Localization.read("/Player/PlayerPause.dat");
			IconsBundle playerPauseIcons = Bundles.getIconsBundle("UI/Player/Icons/PlayerPause");

			optionsButton = new SleekButtonIcon(playerPauseIcons.load<Texture2D>("Options"));
			optionsButton.PositionOffset_X = 110;
			optionsButton.PositionOffset_Y = -115;
			optionsButton.PositionScale_X = 0.5f;
			optionsButton.PositionScale_Y = 0.5f;
			optionsButton.SizeOffset_X = 200;
			optionsButton.SizeOffset_Y = 50;
			optionsButton.text = playerPauseLocalization.format("Options_Button_Text");
			optionsButton.tooltip = playerPauseLocalization.format("Options_Button_Tooltip");
			optionsButton.onClickedButton += onClickedOptionsButton;
			optionsButton.iconColor = ESleekTint.FOREGROUND;
			optionsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(optionsButton);

			displayButton = new SleekButtonIcon(playerPauseIcons.load<Texture2D>("Display"));
			displayButton.PositionOffset_X = 110;
			displayButton.PositionOffset_Y = -55;
			displayButton.PositionScale_X = 0.5f;
			displayButton.PositionScale_Y = 0.5f;
			displayButton.SizeOffset_X = 200;
			displayButton.SizeOffset_Y = 50;
			displayButton.text = playerPauseLocalization.format("Display_Button_Text");
			displayButton.tooltip = playerPauseLocalization.format("Display_Button_Tooltip");
			displayButton.iconColor = ESleekTint.FOREGROUND;
			displayButton.onClickedButton += onClickedDisplayButton;
			displayButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(displayButton);

			graphicsButton = new SleekButtonIcon(playerPauseIcons.load<Texture2D>("Graphics"));
			graphicsButton.PositionOffset_X = 110;
			graphicsButton.PositionOffset_Y = 5;
			graphicsButton.PositionScale_X = 0.5f;
			graphicsButton.PositionScale_Y = 0.5f;
			graphicsButton.SizeOffset_X = 200;
			graphicsButton.SizeOffset_Y = 50;
			graphicsButton.text = playerPauseLocalization.format("Graphics_Button_Text");
			graphicsButton.tooltip = playerPauseLocalization.format("Graphics_Button_Tooltip");
			graphicsButton.iconColor = ESleekTint.FOREGROUND;
			graphicsButton.onClickedButton += onClickedGraphicsButton;
			graphicsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(graphicsButton);

			controlsButton = new SleekButtonIcon(playerPauseIcons.load<Texture2D>("Controls"));
			controlsButton.PositionOffset_X = 110;
			controlsButton.PositionOffset_Y = 65;
			controlsButton.PositionScale_X = 0.5f;
			controlsButton.PositionScale_Y = 0.5f;
			controlsButton.SizeOffset_X = 200;
			controlsButton.SizeOffset_Y = 50;
			controlsButton.text = playerPauseLocalization.format("Controls_Button_Text");
			controlsButton.tooltip = playerPauseLocalization.format("Controls_Button_Tooltip");
			controlsButton.iconColor = ESleekTint.FOREGROUND;
			controlsButton.onClickedButton += onClickedControlsButton;
			controlsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(controlsButton);

			audioButton = new SleekButtonIcon(playerPauseIcons.load<Texture2D>("Audio"));
			audioButton.PositionOffset_X = 110;
			audioButton.PositionOffset_Y = 125;
			audioButton.PositionScale_X = 0.5f;
			audioButton.PositionScale_Y = 0.5f;
			audioButton.SizeOffset_X = 200;
			audioButton.SizeOffset_Y = 50;
			audioButton.text = playerPauseLocalization.format("Audio_Button_Text");
			audioButton.tooltip = playerPauseLocalization.format("Audio_Button_Tooltip");
			audioButton.iconColor = ESleekTint.FOREGROUND;
			audioButton.onClickedButton += onClickedAudioButton;
			audioButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(audioButton);

			exitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Exit"), localization.format("Exit_Button"), localization.format("Exit_Button_Tooltip"), "Cancel", string.Empty);
			exitButton.PositionOffset_X = -100;
			exitButton.PositionOffset_Y = 85;
			exitButton.PositionScale_X = 0.5f;
			exitButton.PositionScale_Y = 0.5f;
			exitButton.SizeOffset_X = 200;
			exitButton.SizeOffset_Y = 30;
			exitButton.text = localization.format("Exit_Button");
			exitButton.tooltip = localization.format("Exit_Button_Tooltip");
			exitButton.onConfirmed += onClickedExitButton;
			container.AddChild(exitButton);

			quitButton = new SleekButtonIconConfirm(MenuPauseUI.icons.load<Texture2D>("Quit"), MenuPauseUI.localization.format("Exit_Button"), MenuPauseUI.localization.format("Exit_Button_Tooltip"), "Cancel", string.Empty);
			quitButton.PositionOffset_X = -100;
			quitButton.PositionOffset_Y = 125;
			quitButton.PositionScale_X = 0.5f;
			quitButton.PositionScale_Y = 0.5f;
			quitButton.SizeOffset_X = 200;
			quitButton.SizeOffset_Y = 50;
			quitButton.text = MenuPauseUI.localization.format("Exit_Button");
			quitButton.tooltip = MenuPauseUI.localization.format("Exit_Button_Tooltip");
			quitButton.onConfirmed += onClickedQuitButton;
			quitButton.fontSize = ESleekFontSize.Medium;
			quitButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(quitButton);

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
			EditorUI.window.AddChild(audioMenu);
		}

		internal static MenuConfigurationAudioUI audioMenu;
	}
}
