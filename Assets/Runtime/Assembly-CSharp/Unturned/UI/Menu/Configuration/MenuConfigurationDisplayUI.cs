////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuConfigurationDisplayUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static ISleekScrollView resolutionsBox;
		private static ISleekButton[] buttons;

		private static SleekButtonState fullscreenMode;
		private static ISleekToggle bufferToggle;
		private static ISleekFloat32Field userInterfaceScaleField;
		private static ISleekToggle targetFrameRateToggle;
		private static ISleekUInt32Field targetFrameRateField;
		private static ISleekToggle unfocusedTargetFrameRateToggle;
		private static ISleekUInt32Field unfocusedTargetFrameRateField;

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

			container.AnimateOutOfView(0, 1);
		}

		private static void onClickedResolutionButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt((button.PositionOffset_Y - 300) / 40);

			Resolution resolution = ScreenEx.GetRecommendedResolutions()[index];
			GraphicsSettings.resolution = new GraphicsSettingsResolution(resolution);
			GraphicsSettings.apply($"changed resolution to {resolution.width} x {resolution.height} [{resolution.refreshRateRatio.value} Hz]");
		}

		private static void onSwappedFullscreenState(SleekButtonState button, int index)
		{
			UnityEngine.FullScreenMode unityFullscreenMode;
			switch (index)
			{
				case 0:
					unityFullscreenMode = FullScreenMode.ExclusiveFullScreen;
					break;

				default:
				case 1:
					unityFullscreenMode = FullScreenMode.FullScreenWindow;
					break;

				case 2:
					unityFullscreenMode = FullScreenMode.Windowed;
					break;
			}

			GraphicsSettings.fullscreenMode = unityFullscreenMode;
			GraphicsSettings.apply("changed fullscreen mode");
		}

		private static void onToggledBufferToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.buffer = state;
			GraphicsSettings.apply("changed vsync");
			SynchronizeTargetFrameRateVisibility();
		}

		private static void onTypedUserInterfaceScale(ISleekFloat32Field field, float state)
		{
			GraphicsSettings.userInterfaceScale = Mathf.Clamp(state, 0.5f, 2.0f);
			GraphicsSettings.apply("changed UI scale");
		}

		private static void OnToggledTargetFrameRate(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.UseTargetFrameRate = state;
			GraphicsSettings.apply("changed use target frame rate");
			SynchronizeTargetFrameRateVisibility();
		}

		private static void OnTypedTargetFrameRate(ISleekUInt32Field field, uint state)
		{
			GraphicsSettings.TargetFrameRate = (int) state;
			GraphicsSettings.apply("changed target frame rate");
		}

		private static void OnToggledUnfocusedTargetFrameRate(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.UseUnfocusedTargetFrameRate = state;
			GraphicsSettings.apply("changed use unfocused target frame rate");
			SynchronizeTargetFrameRateVisibility();
		}

		private static void OnTypedUnfocusedTargetFrameRate(ISleekUInt32Field field, uint state)
		{
			GraphicsSettings.UnfocusedTargetFrameRate = (int) state;
			GraphicsSettings.apply("changed unfocused target frame rate");
		}

		private static void SynchronizeTargetFrameRateVisibility()
		{
			targetFrameRateToggle.IsVisible = !GraphicsSettings.buffer;
			targetFrameRateField.IsVisible = GraphicsSettings.UseTargetFrameRate && targetFrameRateToggle.IsVisible;
			unfocusedTargetFrameRateToggle.IsVisible = targetFrameRateField.IsVisible;
			unfocusedTargetFrameRateField.IsVisible = GraphicsSettings.UseUnfocusedTargetFrameRate && unfocusedTargetFrameRateToggle.IsVisible;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			if (Player.LocalPlayer != null)
			{
				PlayerPauseUI.open();
			}
			else if (Level.isEditor)
			{
				EditorPauseUI.open();
			}
			else
			{
				MenuConfigurationUI.open();
			}

			close();
		}

		public MenuConfigurationDisplayUI()
		{
			localization = Localization.read("/Menu/Configuration/MenuConfigurationDisplay.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;

			if (Provider.isConnected)
			{
				PlayerUI.container.AddChild(container);
			}
			else if (Level.isEditor)
			{
				// Yep this is messy and ideally needs to be cleaned up.
				EditorUI.window.AddChild(container);
			}
			else
			{
				MenuUI.container.AddChild(container);
			}

			active = false;

			Resolution[] resolutions = ScreenEx.GetRecommendedResolutions();

			resolutionsBox = Glazier.Get().CreateScrollView();
			//			resolutionsBox.positionOffset_X = -200;
			//			resolutionsBox.positionOffset_Y = 220;
			//			resolutionsBox.positionScale_X = 0.5f;
			//			resolutionsBox.sizeOffset_X = 430;
			//			resolutionsBox.sizeOffset_Y = -220;
			//			resolutionsBox.sizeScale_Y = 1;
			resolutionsBox.PositionOffset_X = -200;
			resolutionsBox.PositionOffset_Y = 100;
			resolutionsBox.PositionScale_X = 0.5f;
			resolutionsBox.SizeOffset_X = 430;
			resolutionsBox.SizeOffset_Y = -200;
			resolutionsBox.SizeScale_Y = 1;
			resolutionsBox.ScaleContentToWidth = true;
			resolutionsBox.ContentSizeOffset = new Vector2(0.0f, 300 + (resolutions.Length * 40) - 10);
			container.AddChild(resolutionsBox);

			buttons = new ISleekButton[resolutions.Length];
			for (byte index = 0; index < buttons.Length; index++)
			{
				Resolution resolution = resolutions[index];

				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_Y = 300 + (index * 40);
				button.SizeOffset_Y = 30;
				button.SizeScale_X = 1;
				button.OnClicked += onClickedResolutionButton;
				// Nelson 2025-11-14: refreshRateRatio may have a lot of decimal places, so only display 2. (public issue #5295)
				button.Text = resolution.width + " x " + resolution.height + " [" + resolution.refreshRateRatio.value.ToString("N2") + "Hz]";
				resolutionsBox.AddChild(button);

				buttons[index] = button;
			}

			fullscreenMode = new SleekButtonState(new GUIContent(localization.format("Fullscreen_Mode_Exclusive")), new GUIContent(localization.format("Fullscreen_Mode_Borderless")), new GUIContent(localization.format("Fullscreen_Mode_Windowed")));
			fullscreenMode.SizeOffset_X = 200;
			fullscreenMode.SizeOffset_Y = 30;
			fullscreenMode.AddLabel(localization.format("Fullscreen_Mode_Label"), ESleekSide.RIGHT);
			fullscreenMode.tooltip = localization.format("Fullscreen_Mode_Tooltip");
			switch (GraphicsSettings.fullscreenMode)
			{
				case FullScreenMode.ExclusiveFullScreen:
					fullscreenMode.state = 0;
					break;

				default:
				case FullScreenMode.FullScreenWindow:
					fullscreenMode.state = 1;
					break;

				case FullScreenMode.Windowed:
					fullscreenMode.state = 2;
					break;
			}
			fullscreenMode.onSwappedState = onSwappedFullscreenState;
			resolutionsBox.AddChild(fullscreenMode);

			bufferToggle = Glazier.Get().CreateToggle();
			//			bufferToggle.positionOffset_X = -200;
			//			bufferToggle.positionOffset_Y = 170;
			//			bufferToggle.positionScale_X = 0.5f;
			bufferToggle.PositionOffset_Y = 50;
			bufferToggle.SizeOffset_X = 40;
			bufferToggle.SizeOffset_Y = 40;
			bufferToggle.AddLabel(localization.format("Buffer_Toggle_Label"), ESleekSide.RIGHT);
			bufferToggle.Value = GraphicsSettings.buffer;
			bufferToggle.OnValueChanged += onToggledBufferToggle;
			resolutionsBox.AddChild(bufferToggle);

			userInterfaceScaleField = Glazier.Get().CreateFloat32Field();
			userInterfaceScaleField.PositionOffset_Y = 100;
			userInterfaceScaleField.SizeOffset_X = 200;
			userInterfaceScaleField.SizeOffset_Y = 30;
			userInterfaceScaleField.AddLabel(localization.format("User_Interface_Scale_Field_Label"), ESleekSide.RIGHT);
			userInterfaceScaleField.Value = GraphicsSettings.userInterfaceScale;
			userInterfaceScaleField.OnValueSubmitted += onTypedUserInterfaceScale;
			resolutionsBox.AddChild(userInterfaceScaleField);

			targetFrameRateToggle = Glazier.Get().CreateToggle();
			targetFrameRateToggle.PositionOffset_Y = 140;
			targetFrameRateToggle.SizeOffset_X = 40;
			targetFrameRateToggle.SizeOffset_Y = 40;
			targetFrameRateToggle.AddLabel(localization.format("UseTargetFrameRate_Toggle_Label"), ESleekSide.RIGHT);
			targetFrameRateToggle.Value = GraphicsSettings.UseTargetFrameRate;
			targetFrameRateToggle.OnValueChanged += OnToggledTargetFrameRate;
			resolutionsBox.AddChild(targetFrameRateToggle);

			targetFrameRateField = Glazier.Get().CreateUInt32Field();
			targetFrameRateField.PositionOffset_Y = 180;
			targetFrameRateField.SizeOffset_X = 200;
			targetFrameRateField.SizeOffset_Y = 30;
			targetFrameRateField.AddLabel(localization.format("TargetFrameRate_Field_Label"), ESleekSide.RIGHT);
			targetFrameRateField.Value = (uint) GraphicsSettings.TargetFrameRate;
			targetFrameRateField.OnValueChanged += OnTypedTargetFrameRate;
			resolutionsBox.AddChild(targetFrameRateField);

			unfocusedTargetFrameRateToggle = Glazier.Get().CreateToggle();
			unfocusedTargetFrameRateToggle.PositionOffset_Y = 220;
			unfocusedTargetFrameRateToggle.SizeOffset_X = 40;
			unfocusedTargetFrameRateToggle.SizeOffset_Y = 40;
			unfocusedTargetFrameRateToggle.AddLabel(localization.format("UseUnfocusedTargetFrameRate_Toggle_Label"), ESleekSide.RIGHT);
			unfocusedTargetFrameRateToggle.Value = GraphicsSettings.UseUnfocusedTargetFrameRate;
			unfocusedTargetFrameRateToggle.OnValueChanged += OnToggledUnfocusedTargetFrameRate;
			resolutionsBox.AddChild(unfocusedTargetFrameRateToggle);

			unfocusedTargetFrameRateField = Glazier.Get().CreateUInt32Field();
			unfocusedTargetFrameRateField.PositionOffset_Y = 260;
			unfocusedTargetFrameRateField.SizeOffset_X = 200;
			unfocusedTargetFrameRateField.SizeOffset_Y = 30;
			unfocusedTargetFrameRateField.AddLabel(localization.format("UnfocusedTargetFrameRate_Field_Label"), ESleekSide.RIGHT);
			unfocusedTargetFrameRateField.Value = (uint) GraphicsSettings.UnfocusedTargetFrameRate;
			unfocusedTargetFrameRateField.OnValueChanged += OnTypedUnfocusedTargetFrameRate;
			resolutionsBox.AddChild(unfocusedTargetFrameRateField);

			SynchronizeTargetFrameRateVisibility();

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
		}
	}
}
