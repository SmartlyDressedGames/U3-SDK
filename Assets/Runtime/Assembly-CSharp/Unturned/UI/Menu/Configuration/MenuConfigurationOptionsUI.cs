////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuConfigurationOptionsUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;
		private static ISleekButton defaultButton;

		private static ISleekScrollView optionsBox;

		private static ISleekSlider fovSlider;
		private static ISleekLabel fovLabel;
		private static ISleekToggle debugToggle;
		private static ISleekToggle timerToggle;
		private static ISleekToggle goreToggle;
		private static ISleekToggle filterToggle;
		private static ISleekToggle chatTextToggle;
		private static ISleekToggle chatVoiceInToggle;
		private static ISleekToggle chatVoiceOutToggle;
		private static ISleekToggle chatVoiceAlwaysRecordingToggle;
		private static ISleekToggle showOutboundVoiceChatOffHintToggle;
		private static ISleekToggle clickBlueprintToCraftToggle;
		private static ISleekToggle hintsToggle;
		private static ISleekToggle anonymizeMultiplerDetailsToggle;
		private static ISleekToggle hideRichPresenceToggle;
		private static ISleekToggle featuredWorkshopToggle;
		private static ISleekToggle showHotbarToggle;
		private static ISleekToggle pauseWhenUnfocusedToggle;
		private static ISleekToggle nametagFadeOutToggle;
		private static ISleekInt32Field screenshotSizeMultiplierField;
		private static ISleekToggle screenshotSupersamplingToggle;
		private static ISleekToggle screenshotsWhileLoadingToggle;
		private static ISleekToggle staticCrosshairToggle;
		private static ISleekSlider staticCrosshairSizeSlider;
		private static SleekButtonState crosshairShapeButton;
		private static SleekButtonState metricButton;
		private static SleekButtonState talkButton;
		private static SleekButtonState uiButton;
		private static SleekButtonState hitmarkerButton;
		private static SleekButtonState hitmarkerStyleButton;
		private static SleekButtonState vehicleThirdPersonCameraModeButton;
		private static SleekButtonState aircraftThirdPersonCameraModeButton;
		private static ISleekSlider flashbangBrightnessSlider;
		private static ISleekSlider cameraShakeIntensitySlider;
		private static SleekButtonState damageFlinchModeButton;
		private static ISleekSlider damageFlinchIntensitySlider;
		private static ISleekSlider sprintFovBoostIntensitySlider;
		private static ISleekSlider viewmodelBobScaleSlider;
		private static ISleekBox crosshairBox;
		private static SleekColorPicker crosshairColorPicker;
		private static ISleekBox hitmarkerBox;
		private static SleekColorPicker hitmarkerColorPicker;
		private static ISleekBox criticalHitmarkerBox;
		private static SleekColorPicker criticalHitmarkerColorPicker;
		private static ISleekBox cursorBox;
		private static SleekColorPicker cursorColorPicker;
		private static ISleekBox backgroundBox;
		private static SleekColorPicker backgroundColorPicker;
		private static ISleekBox foregroundBox;
		private static SleekColorPicker foregroundColorPicker;
		private static ISleekBox fontBox;
		private static SleekColorPicker fontColorPicker;
		private static ISleekBox shadowBox;
		private static SleekColorPicker shadowColorPicker;
		private static ISleekBox badColorBox;
		private static SleekColorPicker badColorPicker;
		private static ISleekToggle dontShowOnlineSafetyMenuAgainToggle;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			// Nelson 2024-08-13: Synchronizing here in case they've been changed by MenuPlayOnlineSafetyUI.
			updateAll();

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

		private static string FormatFieldOfViewTooltip()
		{
			float verticalFieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
			if (localization.has("FOV_Slider_LabelV2_Value"))
			{
				float horizontalFieldOfView = Camera.VerticalToHorizontalFieldOfView(verticalFieldOfView, ScreenEx.GetCurrentAspectRatio());
				return localization.format("FOV_Slider_LabelV2_Name") + '\n' +
					localization.format("FOV_Slider_LabelV2_Value", Mathf.RoundToInt(horizontalFieldOfView), Mathf.RoundToInt(verticalFieldOfView));
			}
			else
			{
				return localization.format("FOV_Slider_Label", Mathf.RoundToInt(verticalFieldOfView));
			}
		}

		private static string FormatFlashbangBrightnessLabel()
		{
			return localization.format("FlashbangBrightness_Label", OptionsSettings.flashbangBrightness.ToString("P0"));
		}

		private static string FormatCameraShakeIntensityLabel()
		{
			return localization.format("CameraShakeIntensity_Label", OptionsSettings.cameraShakeIntensity.ToString("P0"));
		}

		private static string FormatDamageFlinchIntensityLabel()
		{
			return localization.format("DamageFlinchIntensity_Label", OptionsSettings.damageFlinchIntensity.ToString("P0"));
		}

		private static string FormatSprintFovBoostIntensityLabel()
		{
			return localization.format("SprintFovBoostIntensity_Label", OptionsSettings.sprintFovBoostIntensity.ToString("P0"));
		}

		private static string FormatViewmodelBobScaleLabel()
		{
			return localization.format("ViewmodelBobScale_Label", OptionsSettings.viewmodelBobScale.ToString("P0"));
		}

		private static void onDraggedFOVSlider(ISleekSlider slider, float state)
		{
			OptionsSettings.fov = state;
			OptionsSettings.apply();

			fovLabel.Text = FormatFieldOfViewTooltip();
		}

		private static void onToggledDebugToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.debug = state;
		}

		private static void onToggledTimerToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.timer = state;
			OptionsSettings.apply();
		}

		//private static void onToggledPhysicsToggle(ISleekToggle toggle, bool state)
		//{
		//	OptionsSettings.physics = state;
		//}

		private static void onToggledGoreToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.EnableGore = state;
		}

		private static void onToggledFilterToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.filter = state;
		}

		private static void onToggledChatTextToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.chatText = state;
		}

		private static void onToggledChatVoiceInToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.chatVoiceIn = state;
			chatVoiceOutToggle.IsInteractable = state;
			chatVoiceAlwaysRecordingToggle.IsInteractable = OptionsSettings.chatVoiceIn && OptionsSettings.EnableOutboundVoiceChat;
		}

		private static void onToggledChatVoiceOutToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.EnableOutboundVoiceChat = state;
			chatVoiceAlwaysRecordingToggle.IsInteractable = OptionsSettings.chatVoiceIn && state;
		}

		private static void onToggledChatVoiceAlwaysRecordingToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.VoiceAlwaysRecording = state;
		}

		private static void OnToggledShowOutboundVoiceChatOffHint(ISleekToggle toggle, bool state)
		{
			OptionsSettings.ShowOutboundVoiceChatOffHint = state;
		}

		private static void OnClickBlueprintToCraftToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.ShouldClickBlueprintToCraft = state;
		}

		private static void onToggledHintsToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.hints = state;
		}

		private static void OnAnonymizeMultiplayerDetailsToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.ShouldAnonymizeMultiplayerDetails = state;
		}

		private static void OnHideRichPresenceToggled(ISleekToggle toggle, bool state)
		{
			OptionsSettings.ShouldHideRichPresence = state;
		}

		private static void onToggledFeaturedWorkshopToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.featuredWorkshop = state;
		}

		private static void onToggledShowHotbarToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.showHotbar = state;
		}

		private static void onToggledPauseWhenUnfocusedToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.pauseWhenUnfocused = state;
		}

		private static void onToggledNametagFadeOutToggle(ISleekToggle toggle, bool state)
		{
			OptionsSettings.shouldNametagFadeOut = state;
		}

		private static void OnScreenshotSizeMultiplierChanged(ISleekInt32Field field, int value)
		{
			OptionsSettings.screenshotSizeMultiplier = value;
		}

		private static void OnScreenshotSupersamplingChanged(ISleekToggle toggle, bool state)
		{
			OptionsSettings.enableScreenshotSupersampling = state;
		}

		private static void OnScreenshotsWhileLoadingChanged(ISleekToggle toggle, bool state)
		{
			OptionsSettings.enableScreenshotsOnLoadingScreen = state;
		}

		private static void OnUseStaticCrosshairChanged(ISleekToggle toggle, bool state)
		{
			OptionsSettings.useStaticCrosshair = state;
		}

		private static void OnStaticCrosshairSizeChanged(ISleekSlider slider, float state)
		{
			OptionsSettings.staticCrosshairSize = state;
		}

		private static void OnCrosshairShapeChanged(SleekButtonState button, int index)
		{
			OptionsSettings.crosshairShape = (ECrosshairShape) index;

			if (PlayerLifeUI.crosshair != null)
			{
				PlayerLifeUI.crosshair.SynchronizeImages();
			}
		}

		private static void onSwappedMetricState(SleekButtonState button, int index)
		{
			OptionsSettings.metric = index == 1;
		}

		private static void onSwappedTalkState(SleekButtonState button, int index)
		{
			OptionsSettings.talk = index == 1;
		}

		private static void onSwappedUIState(SleekButtonState button, int index)
		{
			OptionsSettings.proUI = index == 1;
		}

		private static void onSwappedHitmarkerState(SleekButtonState button, int index)
		{
			OptionsSettings.ShouldHitmarkersFollowWorldPosition = index == 1;
		}

		private static void onSwappedHitmarkerStyleState(SleekButtonState button, int index)
		{
			OptionsSettings.hitmarkerStyle = (EHitmarkerStyle) index;
		}

		private static void onSwappedVehicleThirdPersonCameraModeState(SleekButtonState button, int index)
		{
			OptionsSettings.vehicleThirdPersonCameraMode = (EVehicleThirdPersonCameraMode) index;
		}

		private static void onSwappedAircraftThirdPersonCameraModeState(SleekButtonState button, int index)
		{
			OptionsSettings.vehicleAircraftThirdPersonCameraMode = (EVehicleThirdPersonCameraMode) index;
		}

		private static void OnFlashbangBrightnessChanged(ISleekSlider slider, float state)
		{
			OptionsSettings.flashbangBrightness = state;
			flashbangBrightnessSlider.UpdateLabel(FormatFlashbangBrightnessLabel());
		}

		private static void OnCameraShakeIntensityChanged(ISleekSlider slider, float state)
		{
			OptionsSettings.cameraShakeIntensity = state;
			cameraShakeIntensitySlider.UpdateLabel(FormatCameraShakeIntensityLabel());
		}

		private static void onSwappedDamageFlinchModeState(SleekButtonState button, int index)
		{
			OptionsSettings.damageFlinchMode = (EDamageFlinchMode) index;
		}

		private static void OnDamageFlinchIntensityChanged(ISleekSlider slider, float state)
		{
			OptionsSettings.damageFlinchIntensity = state;
			damageFlinchIntensitySlider.UpdateLabel(FormatDamageFlinchIntensityLabel());
		}

		private static void OnSprintFovBoostIntensityChanged(ISleekSlider slider, float state)
		{
			OptionsSettings.sprintFovBoostIntensity = state;
			sprintFovBoostIntensitySlider.UpdateLabel(FormatSprintFovBoostIntensityLabel());
		}

		private static void OnViewmodelBobScaleChanged(ISleekSlider slider, float state)
		{
			OptionsSettings.viewmodelBobScale = state;
			viewmodelBobScaleSlider.UpdateLabel(FormatViewmodelBobScaleLabel());
		}

		private static void onCrosshairColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.crosshairColor = color;

			if (PlayerLifeUI.crosshair != null)
			{
				PlayerLifeUI.crosshair.SynchronizeCustomColors();
			}
		}

		private static void onHitmarkerColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.hitmarkerColor = color;
		}

		private static void onCriticalHitmarkerColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.criticalHitmarkerColor = color;
		}

		private static void onCursorColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.cursorColor = color;
		}

		private static void onBackgroundColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.backgroundColor = color;
		}

		private static void onForegroundColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.foregroundColor = color;
		}

		private static void onFontColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.fontColor = color;
		}

		private static void onShadowColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.shadowColor = color;
		}

		private static void onBadColorPicked(SleekColorPicker picker, Color color)
		{
			OptionsSettings.badColor = color;
		}

		private static void OnDontShowOnlineSafetyMenuAgainToggled(ISleekToggle toggle, bool value)
		{
			OptionsSettings.wantsToHideOnlineSafetyMenu = value;
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

		private static void onClickedDefaultButton(ISleekElement button)
		{
			OptionsSettings.restoreDefaults();

			updateAll();
		}

		private static void updateAll()
		{
			fovSlider.Value = OptionsSettings.fov;
			fovLabel.Text = FormatFieldOfViewTooltip();
			debugToggle.Value = OptionsSettings.debug;
			timerToggle.Value = OptionsSettings.timer;
			goreToggle.Value = OptionsSettings.EnableGore;
			filterToggle.Value = OptionsSettings.filter;
			chatTextToggle.Value = OptionsSettings.chatText;
			chatVoiceInToggle.Value = OptionsSettings.chatVoiceIn;
			chatVoiceOutToggle.Value = OptionsSettings.EnableOutboundVoiceChat;
			chatVoiceOutToggle.IsInteractable = OptionsSettings.chatVoiceIn;
			chatVoiceAlwaysRecordingToggle.Value = OptionsSettings.VoiceAlwaysRecording;
			chatVoiceAlwaysRecordingToggle.IsInteractable = OptionsSettings.chatVoiceIn && OptionsSettings.EnableOutboundVoiceChat;
			showOutboundVoiceChatOffHintToggle.Value = OptionsSettings.ShowOutboundVoiceChatOffHint;
			clickBlueprintToCraftToggle.Value = OptionsSettings.ShouldClickBlueprintToCraft;
			hintsToggle.Value = OptionsSettings.hints;
			anonymizeMultiplerDetailsToggle.Value = OptionsSettings.ShouldAnonymizeMultiplayerDetails;
			hideRichPresenceToggle.Value = OptionsSettings.ShouldHideRichPresence;
			featuredWorkshopToggle.Value = OptionsSettings.featuredWorkshop;
			showHotbarToggle.Value = OptionsSettings.showHotbar;
			pauseWhenUnfocusedToggle.Value = OptionsSettings.pauseWhenUnfocused;
			nametagFadeOutToggle.Value = OptionsSettings.shouldNametagFadeOut;
			screenshotSizeMultiplierField.Value = OptionsSettings.screenshotSizeMultiplier;
			screenshotSupersamplingToggle.Value = OptionsSettings.enableScreenshotSupersampling;
			screenshotsWhileLoadingToggle.Value = OptionsSettings.enableScreenshotsOnLoadingScreen;
			staticCrosshairToggle.Value = OptionsSettings.useStaticCrosshair;
			staticCrosshairSizeSlider.Value = OptionsSettings.staticCrosshairSize;
			crosshairShapeButton.state = (int) OptionsSettings.crosshairShape;
			metricButton.state = OptionsSettings.metric ? 1 : 0;
			talkButton.state = OptionsSettings.talk ? 1 : 0;
			uiButton.state = OptionsSettings.proUI ? 1 : 0;
			hitmarkerButton.state = OptionsSettings.ShouldHitmarkersFollowWorldPosition ? 1 : 0;
			hitmarkerStyleButton.state = (int) OptionsSettings.hitmarkerStyle;
			vehicleThirdPersonCameraModeButton.state = (int) OptionsSettings.vehicleThirdPersonCameraMode;
			aircraftThirdPersonCameraModeButton.state = (int) OptionsSettings.vehicleAircraftThirdPersonCameraMode;
			flashbangBrightnessSlider.Value = OptionsSettings.flashbangBrightness;
			flashbangBrightnessSlider.UpdateLabel(FormatFlashbangBrightnessLabel());
			cameraShakeIntensitySlider.Value = OptionsSettings.cameraShakeIntensity;
			cameraShakeIntensitySlider.UpdateLabel(FormatCameraShakeIntensityLabel());
			damageFlinchModeButton.state = (int) OptionsSettings.damageFlinchMode;
			damageFlinchIntensitySlider.Value = OptionsSettings.damageFlinchIntensity;
			damageFlinchIntensitySlider.UpdateLabel(FormatDamageFlinchIntensityLabel());
			sprintFovBoostIntensitySlider.Value = OptionsSettings.sprintFovBoostIntensity;
			sprintFovBoostIntensitySlider.UpdateLabel(FormatSprintFovBoostIntensityLabel());
			viewmodelBobScaleSlider.Value = OptionsSettings.viewmodelBobScale;
			viewmodelBobScaleSlider.UpdateLabel(FormatViewmodelBobScaleLabel());

			crosshairColorPicker.state = OptionsSettings.crosshairColor;
			hitmarkerColorPicker.state = OptionsSettings.hitmarkerColor;
			criticalHitmarkerColorPicker.state = OptionsSettings.criticalHitmarkerColor;
			cursorColorPicker.state = OptionsSettings.cursorColor;
			backgroundColorPicker.state = OptionsSettings.backgroundColor;
			foregroundColorPicker.state = OptionsSettings.foregroundColor;
			fontColorPicker.state = OptionsSettings.fontColor;
			shadowColorPicker.state = OptionsSettings.shadowColor;
			badColorPicker.state = OptionsSettings.badColor;

			dontShowOnlineSafetyMenuAgainToggle.Value = OptionsSettings.wantsToHideOnlineSafetyMenu;
		}

		public MenuConfigurationOptionsUI()
		{
			localization = Localization.read("/Menu/Configuration/MenuConfigurationOptions.dat");

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

			optionsBox = Glazier.Get().CreateScrollView();
			optionsBox.PositionOffset_X = -250;
			optionsBox.PositionOffset_Y = 100;
			optionsBox.PositionScale_X = 0.5f;
			optionsBox.SizeOffset_X = 530;
			optionsBox.SizeOffset_Y = -200;
			optionsBox.SizeScale_Y = 1;
			optionsBox.ScaleContentToWidth = true;
			container.AddChild(optionsBox);

			float verticalOffset = 0;

			debugToggle = Glazier.Get().CreateToggle();
			debugToggle.PositionOffset_Y = verticalOffset;
			debugToggle.SizeOffset_X = 40;
			debugToggle.SizeOffset_Y = 40;
			debugToggle.AddLabel(localization.format("Debug_Toggle_Label"), ESleekSide.RIGHT);
			debugToggle.OnValueChanged += onToggledDebugToggle;
			optionsBox.AddChild(debugToggle);
			verticalOffset += 50;

			timerToggle = Glazier.Get().CreateToggle();
			timerToggle.PositionOffset_Y = verticalOffset;
			timerToggle.SizeOffset_X = 40;
			timerToggle.SizeOffset_Y = 40;
			timerToggle.AddLabel(localization.format("Timer_Toggle_Label"), ESleekSide.RIGHT);
			timerToggle.OnValueChanged += onToggledTimerToggle;
			optionsBox.AddChild(timerToggle);
			verticalOffset += 50;

			goreToggle = Glazier.Get().CreateToggle();
			goreToggle.PositionOffset_Y = verticalOffset;
			goreToggle.SizeOffset_X = 40;
			goreToggle.SizeOffset_Y = 40;
			goreToggle.AddLabel(localization.format("Gore_Toggle_Label"), ESleekSide.RIGHT);
			goreToggle.OnValueChanged += onToggledGoreToggle;
			optionsBox.AddChild(goreToggle);
			verticalOffset += 50;

			filterToggle = Glazier.Get().CreateToggle();
			filterToggle.PositionOffset_Y = verticalOffset;
			filterToggle.SizeOffset_X = 40;
			filterToggle.SizeOffset_Y = 40;
			filterToggle.AddLabel(localization.format("Filter_Toggle_Label"), ESleekSide.RIGHT);
			filterToggle.TooltipText = localization.format("Filter_Toggle_Tooltip");
			filterToggle.OnValueChanged += onToggledFilterToggle;
			optionsBox.AddChild(filterToggle);
			verticalOffset += 50;

			chatTextToggle = Glazier.Get().CreateToggle();
			chatTextToggle.PositionOffset_Y = verticalOffset;
			chatTextToggle.SizeOffset_X = 40;
			chatTextToggle.SizeOffset_Y = 40;
			chatTextToggle.AddLabel(localization.format("Chat_Text_Toggle_Label"), ESleekSide.RIGHT);
			chatTextToggle.OnValueChanged += onToggledChatTextToggle;
			optionsBox.AddChild(chatTextToggle);
			verticalOffset += 50;

			chatVoiceInToggle = Glazier.Get().CreateToggle();
			chatVoiceInToggle.PositionOffset_Y = verticalOffset;
			chatVoiceInToggle.SizeOffset_X = 40;
			chatVoiceInToggle.SizeOffset_Y = 40;
			chatVoiceInToggle.AddLabel(localization.format("Chat_Voice_In_Toggle_Label"), ESleekSide.RIGHT);
			chatVoiceInToggle.OnValueChanged += onToggledChatVoiceInToggle;
			optionsBox.AddChild(chatVoiceInToggle);
			verticalOffset += 50;

			chatVoiceOutToggle = Glazier.Get().CreateToggle();
			chatVoiceOutToggle.PositionOffset_Y = verticalOffset;
			chatVoiceOutToggle.SizeOffset_X = 40;
			chatVoiceOutToggle.SizeOffset_Y = 40;
			chatVoiceOutToggle.AddLabel(localization.format("Chat_Voice_Out_Toggle_Label"), ESleekSide.RIGHT);
			chatVoiceOutToggle.OnValueChanged += onToggledChatVoiceOutToggle;
			optionsBox.AddChild(chatVoiceOutToggle);
			verticalOffset += 50;

			chatVoiceAlwaysRecordingToggle = Glazier.Get().CreateToggle();
			chatVoiceAlwaysRecordingToggle.PositionOffset_Y = verticalOffset;
			chatVoiceAlwaysRecordingToggle.SizeOffset_X = 40;
			chatVoiceAlwaysRecordingToggle.SizeOffset_Y = 40;
			chatVoiceAlwaysRecordingToggle.AddLabel(localization.format("VoiceAlwaysRecording_Label"), ESleekSide.RIGHT);
			chatVoiceAlwaysRecordingToggle.TooltipText = localization.format("VoiceAlwaysRecording_Tooltip");
			chatVoiceAlwaysRecordingToggle.OnValueChanged += onToggledChatVoiceAlwaysRecordingToggle;
			optionsBox.AddChild(chatVoiceAlwaysRecordingToggle);
			verticalOffset += 50;

			showOutboundVoiceChatOffHintToggle = Glazier.Get().CreateToggle();
			showOutboundVoiceChatOffHintToggle.PositionOffset_Y = verticalOffset;
			showOutboundVoiceChatOffHintToggle.SizeOffset_X = 40;
			showOutboundVoiceChatOffHintToggle.SizeOffset_Y = 40;
			showOutboundVoiceChatOffHintToggle.AddLabel(localization.format("ShowOutboundVoiceChatOffHint_Label"), ESleekSide.RIGHT);
			showOutboundVoiceChatOffHintToggle.TooltipText = localization.format("ShowOutboundVoiceChatOffHint_Tooltip");
			showOutboundVoiceChatOffHintToggle.OnValueChanged += OnToggledShowOutboundVoiceChatOffHint;
			optionsBox.AddChild(showOutboundVoiceChatOffHintToggle);
			verticalOffset += 50;

			hintsToggle = Glazier.Get().CreateToggle();
			hintsToggle.PositionOffset_Y = verticalOffset;
			hintsToggle.SizeOffset_X = 40;
			hintsToggle.SizeOffset_Y = 40;
			hintsToggle.AddLabel(localization.format("Hints_Toggle_Label"), ESleekSide.RIGHT);
			hintsToggle.OnValueChanged += onToggledHintsToggle;
			optionsBox.AddChild(hintsToggle);
			verticalOffset += 50;

			clickBlueprintToCraftToggle = Glazier.Get().CreateToggle();
			clickBlueprintToCraftToggle.PositionOffset_Y = verticalOffset;
			clickBlueprintToCraftToggle.SizeOffset_X = 40;
			clickBlueprintToCraftToggle.SizeOffset_Y = 40;
			clickBlueprintToCraftToggle.AddLabel(localization.format("ClickBlueprintToCraft_Label"), ESleekSide.RIGHT);
			clickBlueprintToCraftToggle.TooltipText = localization.format("ClickBlueprintToCraft_Tooltip");
			clickBlueprintToCraftToggle.OnValueChanged += OnClickBlueprintToCraftToggled;
			optionsBox.AddChild(clickBlueprintToCraftToggle);
			verticalOffset += 50;

			anonymizeMultiplerDetailsToggle = Glazier.Get().CreateToggle();
			anonymizeMultiplerDetailsToggle.PositionOffset_Y = verticalOffset;
			anonymizeMultiplerDetailsToggle.SizeOffset_X = 40;
			anonymizeMultiplerDetailsToggle.SizeOffset_Y = 40;
			anonymizeMultiplerDetailsToggle.AddLabel(localization.format("AnonymizeMultiplayerDetails_Label"), ESleekSide.RIGHT);
			anonymizeMultiplerDetailsToggle.TooltipText = localization.format("AnonymizeMultiplayerDetails_Tooltip");
			anonymizeMultiplerDetailsToggle.OnValueChanged += OnAnonymizeMultiplayerDetailsToggled;
			optionsBox.AddChild(anonymizeMultiplerDetailsToggle);
			verticalOffset += 50;

			hideRichPresenceToggle = Glazier.Get().CreateToggle();
			hideRichPresenceToggle.PositionOffset_Y = verticalOffset;
			hideRichPresenceToggle.SizeOffset_X = 40;
			hideRichPresenceToggle.SizeOffset_Y = 40;
			hideRichPresenceToggle.AddLabel(localization.format("HideRichPresence_Label"), ESleekSide.RIGHT);
			hideRichPresenceToggle.TooltipText = localization.format("HideRichPresence_Tooltip");
			hideRichPresenceToggle.OnValueChanged += OnHideRichPresenceToggled;
			optionsBox.AddChild(hideRichPresenceToggle);
			verticalOffset += 50;

			featuredWorkshopToggle = Glazier.Get().CreateToggle();
			featuredWorkshopToggle.PositionOffset_Y = verticalOffset;
			featuredWorkshopToggle.SizeOffset_X = 40;
			featuredWorkshopToggle.SizeOffset_Y = 40;
			featuredWorkshopToggle.AddLabel(localization.format("Featured_Workshop_Toggle_Label"), ESleekSide.RIGHT);
			featuredWorkshopToggle.OnValueChanged += onToggledFeaturedWorkshopToggle;
			optionsBox.AddChild(featuredWorkshopToggle);
			verticalOffset += 50;

			showHotbarToggle = Glazier.Get().CreateToggle();
			showHotbarToggle.PositionOffset_Y = verticalOffset;
			showHotbarToggle.SizeOffset_X = 40;
			showHotbarToggle.SizeOffset_Y = 40;
			showHotbarToggle.AddLabel(localization.format("Show_Hotbar_Toggle_Label"), ESleekSide.RIGHT);
			showHotbarToggle.OnValueChanged += onToggledShowHotbarToggle;
			optionsBox.AddChild(showHotbarToggle);
			verticalOffset += 50;

			pauseWhenUnfocusedToggle = Glazier.Get().CreateToggle();
			pauseWhenUnfocusedToggle.PositionOffset_Y = verticalOffset;
			pauseWhenUnfocusedToggle.SizeOffset_X = 40;
			pauseWhenUnfocusedToggle.SizeOffset_Y = 40;
			pauseWhenUnfocusedToggle.AddLabel(localization.format("Pause_When_Unfocused_Label"), ESleekSide.RIGHT);
			pauseWhenUnfocusedToggle.OnValueChanged += onToggledPauseWhenUnfocusedToggle;
			optionsBox.AddChild(pauseWhenUnfocusedToggle);
			verticalOffset += 50;

			nametagFadeOutToggle = Glazier.Get().CreateToggle();
			nametagFadeOutToggle.PositionOffset_Y = verticalOffset;
			nametagFadeOutToggle.SizeOffset_X = 40;
			nametagFadeOutToggle.SizeOffset_Y = 40;
			nametagFadeOutToggle.AddLabel(localization.format("Nametag_Fade_Out_Label"), ESleekSide.RIGHT);
			nametagFadeOutToggle.TooltipText = localization.format("Nametag_Fade_Out_Tooltip");
			nametagFadeOutToggle.OnValueChanged += onToggledNametagFadeOutToggle;
			optionsBox.AddChild(nametagFadeOutToggle);
			verticalOffset += 50;

			fovSlider = Glazier.Get().CreateSlider();
			fovSlider.PositionOffset_Y = verticalOffset;
			fovSlider.SizeOffset_X = 200;
			fovSlider.SizeOffset_Y = 20;
			fovSlider.Orientation = ESleekOrientation.HORIZONTAL;
			fovSlider.OnValueChanged += onDraggedFOVSlider;
			optionsBox.AddChild(fovSlider);
			fovLabel = Glazier.Get().CreateLabel();
			fovLabel.PositionOffset_X = 5;
			fovLabel.PositionOffset_Y = -30;
			fovLabel.PositionScale_X = 1.0f;
			fovLabel.PositionScale_Y = 0.5f;
			fovLabel.SizeOffset_X = 300;
			fovLabel.SizeOffset_Y = 60;
			fovLabel.TextAlignment = TextAnchor.MiddleLeft;
			fovLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			fovLabel.Text = FormatFieldOfViewTooltip();
			fovSlider.AddChild(fovLabel);
			verticalOffset += 30;

			screenshotSizeMultiplierField = Glazier.Get().CreateInt32Field();
			screenshotSizeMultiplierField.PositionOffset_Y = verticalOffset;
			screenshotSizeMultiplierField.SizeOffset_X = 200;
			screenshotSizeMultiplierField.SizeOffset_Y = 30;
			screenshotSizeMultiplierField.AddLabel(localization.format("ScreenshotSizeMultiplier_Label"), ESleekSide.RIGHT);
			screenshotSizeMultiplierField.TooltipText = localization.format("ScreenshotSizeMultiplier_Tooltip");
			screenshotSizeMultiplierField.OnValueChanged += OnScreenshotSizeMultiplierChanged;
			optionsBox.AddChild(screenshotSizeMultiplierField);
			verticalOffset += 40;

			screenshotSupersamplingToggle = Glazier.Get().CreateToggle();
			screenshotSupersamplingToggle.PositionOffset_Y = verticalOffset;
			screenshotSupersamplingToggle.SizeOffset_X = 40;
			screenshotSupersamplingToggle.SizeOffset_Y = 40;
			screenshotSupersamplingToggle.AddLabel(localization.format("ScreenshotSupersampling_Label"), ESleekSide.RIGHT);
			screenshotSupersamplingToggle.TooltipText = localization.format("ScreenshotSupersampling_Tooltip");
			screenshotSupersamplingToggle.OnValueChanged += OnScreenshotSupersamplingChanged;
			optionsBox.AddChild(screenshotSupersamplingToggle);
			verticalOffset += 50;

			screenshotsWhileLoadingToggle = Glazier.Get().CreateToggle();
			screenshotsWhileLoadingToggle.PositionOffset_Y = verticalOffset;
			screenshotsWhileLoadingToggle.SizeOffset_X = 40;
			screenshotsWhileLoadingToggle.SizeOffset_Y = 40;
			screenshotsWhileLoadingToggle.AddLabel(localization.format("ScreenshotsWhileLoading_Label"), ESleekSide.RIGHT);
			screenshotsWhileLoadingToggle.TooltipText = localization.format("ScreenshotsWhileLoading_Tooltip");
			screenshotsWhileLoadingToggle.OnValueChanged += OnScreenshotsWhileLoadingChanged;
			optionsBox.AddChild(screenshotsWhileLoadingToggle);
			verticalOffset += 50;

			staticCrosshairToggle = Glazier.Get().CreateToggle();
			staticCrosshairToggle.PositionOffset_Y = verticalOffset;
			staticCrosshairToggle.SizeOffset_X = 40;
			staticCrosshairToggle.SizeOffset_Y = 40;
			staticCrosshairToggle.AddLabel(localization.format("UseStaticCrosshair_Label"), ESleekSide.RIGHT);
			staticCrosshairToggle.TooltipText = localization.format("UseStaticCrosshair_Tooltip");
			staticCrosshairToggle.OnValueChanged += OnUseStaticCrosshairChanged;
			optionsBox.AddChild(staticCrosshairToggle);
			verticalOffset += 50;

			staticCrosshairSizeSlider = Glazier.Get().CreateSlider();
			staticCrosshairSizeSlider.PositionOffset_Y = verticalOffset;
			staticCrosshairSizeSlider.SizeOffset_X = 200;
			staticCrosshairSizeSlider.SizeOffset_Y = 20;
			staticCrosshairSizeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			staticCrosshairSizeSlider.AddLabel(localization.format("StaticCrosshairSize_Label"), ESleekSide.RIGHT);
			staticCrosshairSizeSlider.OnValueChanged += OnStaticCrosshairSizeChanged;
			optionsBox.AddChild(staticCrosshairSizeSlider);
			verticalOffset += 30;

			crosshairShapeButton = new SleekButtonState(new GUIContent(localization.format("CrosshairShape_Line")), new GUIContent(localization.format("CrosshairShape_Classic")));
			crosshairShapeButton.PositionOffset_Y = verticalOffset;
			crosshairShapeButton.SizeOffset_X = 200;
			crosshairShapeButton.SizeOffset_Y = 30;
			crosshairShapeButton.state = (int) OptionsSettings.crosshairShape;
			crosshairShapeButton.AddLabel(localization.format("CrosshairShape_Label"), ESleekSide.RIGHT);
			crosshairShapeButton.onSwappedState = OnCrosshairShapeChanged;
			optionsBox.AddChild(crosshairShapeButton);
			verticalOffset += 40;

			talkButton = new SleekButtonState(new GUIContent(localization.format("Talk_Off")), new GUIContent(localization.format("Talk_On")));
			talkButton.PositionOffset_Y = verticalOffset;
			talkButton.SizeOffset_X = 200;
			talkButton.SizeOffset_Y = 30;
			talkButton.state = OptionsSettings.talk ? 1 : 0;
			talkButton.tooltip = localization.format("Talk_Tooltip");
			talkButton.AddLabel(localization.format("Talk_Label"), ESleekSide.RIGHT);
			talkButton.onSwappedState = onSwappedTalkState;
			optionsBox.AddChild(talkButton);
			verticalOffset += 40;

			metricButton = new SleekButtonState(new GUIContent(localization.format("Metric_Off")), new GUIContent(localization.format("Metric_On")));
			metricButton.PositionOffset_Y = verticalOffset;
			metricButton.SizeOffset_X = 200;
			metricButton.SizeOffset_Y = 30;
			metricButton.state = OptionsSettings.metric ? 1 : 0;
			metricButton.tooltip = localization.format("Metric_Tooltip");
			metricButton.AddLabel(localization.format("Metric_Label"), ESleekSide.RIGHT);
			metricButton.onSwappedState = onSwappedMetricState;
			optionsBox.AddChild(metricButton);
			verticalOffset += 40;

			uiButton = new SleekButtonState(new GUIContent(localization.format("UI_Free")), new GUIContent(localization.format("UI_Pro")));
			uiButton.PositionOffset_Y = verticalOffset;
			uiButton.SizeOffset_X = 200;
			uiButton.SizeOffset_Y = 30;
			uiButton.tooltip = localization.format("UI_Tooltip");
			uiButton.AddLabel(localization.format("UI_Label"), ESleekSide.RIGHT);
			uiButton.onSwappedState = onSwappedUIState;
			optionsBox.AddChild(uiButton);
			verticalOffset += 40;

			hitmarkerButton = new SleekButtonState(new GUIContent(localization.format("Hitmarker_Static")), new GUIContent(localization.format("Hitmarker_Dynamic")));
			hitmarkerButton.PositionOffset_Y = verticalOffset;
			hitmarkerButton.SizeOffset_X = 200;
			hitmarkerButton.SizeOffset_Y = 30;
			hitmarkerButton.tooltip = localization.format("Hitmarker_Tooltip");
			hitmarkerButton.AddLabel(localization.format("Hitmarker_Label"), ESleekSide.RIGHT);
			hitmarkerButton.onSwappedState = onSwappedHitmarkerState;
			optionsBox.AddChild(hitmarkerButton);
			verticalOffset += 40;

			hitmarkerStyleButton = new SleekButtonState(new GUIContent(localization.format("HitmarkerStyle_Animated")), new GUIContent(localization.format("HitmarkerStyle_Classic")));
			hitmarkerStyleButton.PositionOffset_Y = verticalOffset;
			hitmarkerStyleButton.SizeOffset_X = 200;
			hitmarkerStyleButton.SizeOffset_Y = 30;
			hitmarkerStyleButton.AddLabel(localization.format("HitmarkerStyle_Label"), ESleekSide.RIGHT);
			hitmarkerStyleButton.onSwappedState = onSwappedHitmarkerStyleState;
			optionsBox.AddChild(hitmarkerStyleButton);
			verticalOffset += 40;

			vehicleThirdPersonCameraModeButton = new SleekButtonState(new GUIContent(localization.format("VehicleThirdPersonCameraMode_RotationDetached")), new GUIContent(localization.format("VehicleThirdPersonCameraMode_RotationAttached")));
			vehicleThirdPersonCameraModeButton.PositionOffset_Y = verticalOffset;
			vehicleThirdPersonCameraModeButton.SizeOffset_X = 200;
			vehicleThirdPersonCameraModeButton.SizeOffset_Y = 30;
			vehicleThirdPersonCameraModeButton.AddLabel(localization.format("VehicleThirdPersonCameraMode_Label"), ESleekSide.RIGHT);
			vehicleThirdPersonCameraModeButton.onSwappedState = onSwappedVehicleThirdPersonCameraModeState;
			optionsBox.AddChild(vehicleThirdPersonCameraModeButton);
			verticalOffset += 40;

			aircraftThirdPersonCameraModeButton = new SleekButtonState(new GUIContent(localization.format("VehicleThirdPersonCameraMode_RotationDetached")), new GUIContent(localization.format("VehicleThirdPersonCameraMode_RotationAttached")));
			aircraftThirdPersonCameraModeButton.PositionOffset_Y = verticalOffset;
			aircraftThirdPersonCameraModeButton.SizeOffset_X = 200;
			aircraftThirdPersonCameraModeButton.SizeOffset_Y = 30;
			aircraftThirdPersonCameraModeButton.AddLabel(localization.format("AircraftThirdPersonCameraMode_Label"), ESleekSide.RIGHT);
			aircraftThirdPersonCameraModeButton.onSwappedState = onSwappedAircraftThirdPersonCameraModeState;
			optionsBox.AddChild(aircraftThirdPersonCameraModeButton);
			verticalOffset += 40;
			
			flashbangBrightnessSlider = Glazier.Get().CreateSlider();
			flashbangBrightnessSlider.PositionOffset_Y = verticalOffset;
			flashbangBrightnessSlider.SizeOffset_X = 200;
			flashbangBrightnessSlider.SizeOffset_Y = 20;
			flashbangBrightnessSlider.Orientation = ESleekOrientation.HORIZONTAL;
			flashbangBrightnessSlider.AddLabel(FormatFlashbangBrightnessLabel(), ESleekSide.RIGHT);
			flashbangBrightnessSlider.OnValueChanged += OnFlashbangBrightnessChanged;
			optionsBox.AddChild(flashbangBrightnessSlider);
			verticalOffset += 30;

			cameraShakeIntensitySlider = Glazier.Get().CreateSlider();
			cameraShakeIntensitySlider.PositionOffset_Y = verticalOffset;
			cameraShakeIntensitySlider.SizeOffset_X = 200;
			cameraShakeIntensitySlider.SizeOffset_Y = 20;
			cameraShakeIntensitySlider.Orientation = ESleekOrientation.HORIZONTAL;
			cameraShakeIntensitySlider.AddLabel(FormatCameraShakeIntensityLabel(), ESleekSide.RIGHT);
			cameraShakeIntensitySlider.OnValueChanged += OnCameraShakeIntensityChanged;
			optionsBox.AddChild(cameraShakeIntensitySlider);
			verticalOffset += 30;

			sprintFovBoostIntensitySlider = Glazier.Get().CreateSlider();
			sprintFovBoostIntensitySlider.PositionOffset_Y = verticalOffset;
			sprintFovBoostIntensitySlider.SizeOffset_X = 200;
			sprintFovBoostIntensitySlider.SizeOffset_Y = 20;
			sprintFovBoostIntensitySlider.Orientation = ESleekOrientation.HORIZONTAL;
			sprintFovBoostIntensitySlider.AddLabel(FormatSprintFovBoostIntensityLabel(), ESleekSide.RIGHT);
			sprintFovBoostIntensitySlider.OnValueChanged += OnSprintFovBoostIntensityChanged;
			optionsBox.AddChild(sprintFovBoostIntensitySlider);
			verticalOffset += 30;

			viewmodelBobScaleSlider = Glazier.Get().CreateSlider();
			viewmodelBobScaleSlider.PositionOffset_Y = verticalOffset;
			viewmodelBobScaleSlider.SizeOffset_X = 200;
			viewmodelBobScaleSlider.SizeOffset_Y = 20;
			viewmodelBobScaleSlider.Orientation = ESleekOrientation.HORIZONTAL;
			viewmodelBobScaleSlider.AddLabel(FormatViewmodelBobScaleLabel(), ESleekSide.RIGHT);
			viewmodelBobScaleSlider.OnValueChanged += OnViewmodelBobScaleChanged;
			optionsBox.AddChild(viewmodelBobScaleSlider);
			verticalOffset += 30;

			damageFlinchModeButton = new SleekButtonState(new GUIContent(localization.format("DamageFlinchMode_RollOnly")), new GUIContent(localization.format("DamageFlinchMode_Directional")));
			damageFlinchModeButton.PositionOffset_Y = verticalOffset;
			damageFlinchModeButton.SizeOffset_X = 200;
			damageFlinchModeButton.SizeOffset_Y = 30;
			damageFlinchModeButton.AddLabel(localization.format("DamageFlinchMode_Label"), ESleekSide.RIGHT);
			damageFlinchModeButton.onSwappedState = onSwappedDamageFlinchModeState;
			optionsBox.AddChild(damageFlinchModeButton);
			verticalOffset += 40;

			damageFlinchIntensitySlider = Glazier.Get().CreateSlider();
			damageFlinchIntensitySlider.PositionOffset_Y = verticalOffset;
			damageFlinchIntensitySlider.SizeOffset_X = 200;
			damageFlinchIntensitySlider.SizeOffset_Y = 20;
			damageFlinchIntensitySlider.Orientation = ESleekOrientation.HORIZONTAL;
			damageFlinchIntensitySlider.AddLabel(FormatDamageFlinchIntensityLabel(), ESleekSide.RIGHT);
			damageFlinchIntensitySlider.OnValueChanged += OnDamageFlinchIntensityChanged;
			optionsBox.AddChild(damageFlinchIntensitySlider);
			verticalOffset += 30;

			crosshairBox = Glazier.Get().CreateBox();
			crosshairBox.PositionOffset_Y = verticalOffset;
			crosshairBox.SizeOffset_X = 240;
			crosshairBox.SizeOffset_Y = 30;
			crosshairBox.Text = localization.format("Crosshair_Box");
			optionsBox.AddChild(crosshairBox);
			verticalOffset += 40;

			crosshairColorPicker = new SleekColorPicker();
			crosshairColorPicker.PositionOffset_Y = verticalOffset;
			crosshairColorPicker.onColorPicked = onCrosshairColorPicked;
			crosshairColorPicker.SetAllowAlpha(true);
			optionsBox.AddChild(crosshairColorPicker);
			verticalOffset += 160;

			hitmarkerBox = Glazier.Get().CreateBox();
			hitmarkerBox.PositionOffset_Y = verticalOffset;
			hitmarkerBox.SizeOffset_X = 240;
			hitmarkerBox.SizeOffset_Y = 30;
			hitmarkerBox.Text = localization.format("Hitmarker_Box");
			optionsBox.AddChild(hitmarkerBox);
			verticalOffset += 40;

			hitmarkerColorPicker = new SleekColorPicker();
			hitmarkerColorPicker.PositionOffset_Y = verticalOffset;
			hitmarkerColorPicker.onColorPicked = onHitmarkerColorPicked;
			hitmarkerColorPicker.SetAllowAlpha(true);
			optionsBox.AddChild(hitmarkerColorPicker);
			verticalOffset += 160;

			criticalHitmarkerBox = Glazier.Get().CreateBox();
			criticalHitmarkerBox.PositionOffset_Y = verticalOffset;
			criticalHitmarkerBox.SizeOffset_X = 240;
			criticalHitmarkerBox.SizeOffset_Y = 30;
			criticalHitmarkerBox.Text = localization.format("Critical_Hitmarker_Box");
			optionsBox.AddChild(criticalHitmarkerBox);
			verticalOffset += 40;

			criticalHitmarkerColorPicker = new SleekColorPicker();
			criticalHitmarkerColorPicker.PositionOffset_Y = verticalOffset;
			criticalHitmarkerColorPicker.onColorPicked = onCriticalHitmarkerColorPicked;
			criticalHitmarkerColorPicker.SetAllowAlpha(true);
			optionsBox.AddChild(criticalHitmarkerColorPicker);
			verticalOffset += 160;

			cursorBox = Glazier.Get().CreateBox();
			cursorBox.PositionOffset_Y = verticalOffset;
			cursorBox.SizeOffset_X = 240;
			cursorBox.SizeOffset_Y = 30;
			cursorBox.Text = localization.format("Cursor_Box");
			optionsBox.AddChild(cursorBox);
			verticalOffset += 40;

			cursorColorPicker = new SleekColorPicker();
			cursorColorPicker.PositionOffset_Y = verticalOffset;
			cursorColorPicker.onColorPicked = onCursorColorPicked;
			optionsBox.AddChild(cursorColorPicker);
			verticalOffset += 130;

			backgroundBox = Glazier.Get().CreateBox();
			backgroundBox.PositionOffset_Y = verticalOffset;
			backgroundBox.SizeOffset_X = 240;
			backgroundBox.SizeOffset_Y = 30;
			backgroundBox.Text = localization.format("Background_Box");
			optionsBox.AddChild(backgroundBox);
			verticalOffset += 40;

			backgroundColorPicker = new SleekColorPicker();
			backgroundColorPicker.PositionOffset_Y = verticalOffset;

			if (Provider.isPro)
			{
				backgroundColorPicker.onColorPicked = onBackgroundColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				backgroundColorPicker.AddChild(pro);
			}

			optionsBox.AddChild(backgroundColorPicker);
			verticalOffset += 130;

			foregroundBox = Glazier.Get().CreateBox();
			foregroundBox.PositionOffset_Y = verticalOffset;
			foregroundBox.SizeOffset_X = 240;
			foregroundBox.SizeOffset_Y = 30;
			foregroundBox.Text = localization.format("Foreground_Box");
			optionsBox.AddChild(foregroundBox);
			verticalOffset += 40;

			foregroundColorPicker = new SleekColorPicker();
			foregroundColorPicker.PositionOffset_Y = verticalOffset;

			if (Provider.isPro)
			{
				foregroundColorPicker.onColorPicked = onForegroundColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				foregroundColorPicker.AddChild(pro);
			}

			optionsBox.AddChild(foregroundColorPicker);
			verticalOffset += 130;

			fontBox = Glazier.Get().CreateBox();
			fontBox.PositionOffset_Y = verticalOffset;
			fontBox.SizeOffset_X = 240;
			fontBox.SizeOffset_Y = 30;
			fontBox.Text = localization.format("Font_Box");
			optionsBox.AddChild(fontBox);
			verticalOffset += 40;

			fontColorPicker = new SleekColorPicker();
			fontColorPicker.PositionOffset_Y = verticalOffset;

			if (Provider.isPro)
			{
				fontColorPicker.onColorPicked = onFontColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				fontColorPicker.AddChild(pro);
			}

			optionsBox.AddChild(fontColorPicker);
			verticalOffset += 130;

			shadowBox = Glazier.Get().CreateBox();
			shadowBox.PositionOffset_Y = verticalOffset;
			shadowBox.SizeOffset_X = 240;
			shadowBox.SizeOffset_Y = 30;
			shadowBox.Text = localization.format("Shadow_Box");
			optionsBox.AddChild(shadowBox);
			verticalOffset += shadowBox.SizeOffset_Y + 10;

			shadowColorPicker = new SleekColorPicker();
			shadowColorPicker.PositionOffset_Y = verticalOffset;

			if (Provider.isPro)
			{
				shadowColorPicker.onColorPicked = onShadowColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				shadowColorPicker.AddChild(pro);
			}

			optionsBox.AddChild(shadowColorPicker);
			verticalOffset += shadowColorPicker.SizeOffset_Y + 10;

			badColorBox = Glazier.Get().CreateBox();
			badColorBox.PositionOffset_Y = verticalOffset;
			badColorBox.SizeOffset_X = 240;
			badColorBox.SizeOffset_Y = 30;
			badColorBox.Text = localization.format("Bad_Color_Box");
			optionsBox.AddChild(badColorBox);
			verticalOffset += badColorBox.SizeOffset_Y + 10;

			badColorPicker = new SleekColorPicker();
			badColorPicker.PositionOffset_Y = verticalOffset;

			if (Provider.isPro)
			{
				badColorPicker.onColorPicked = onBadColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				badColorPicker.AddChild(pro);
			}

			optionsBox.AddChild(badColorPicker);
			verticalOffset += badColorPicker.SizeOffset_Y;

			dontShowOnlineSafetyMenuAgainToggle = Glazier.Get().CreateToggle();
			dontShowOnlineSafetyMenuAgainToggle.PositionOffset_Y = verticalOffset;
			dontShowOnlineSafetyMenuAgainToggle.SizeOffset_X = 40;
			dontShowOnlineSafetyMenuAgainToggle.SizeOffset_Y = 40;
			dontShowOnlineSafetyMenuAgainToggle.AddLabel(localization.format("DontShowOnlineSafetyMenuAgain_Label"), ESleekSide.RIGHT);
			dontShowOnlineSafetyMenuAgainToggle.TooltipText = localization.format("DontShowOnlineSafetyMenuAgain_Tooltip");
			dontShowOnlineSafetyMenuAgainToggle.OnValueChanged += OnDontShowOnlineSafetyMenuAgainToggled;
			optionsBox.AddChild(dontShowOnlineSafetyMenuAgainToggle);
			verticalOffset += 50;

			optionsBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10.0f);

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

			defaultButton = Glazier.Get().CreateButton();
			defaultButton.PositionOffset_X = -200;
			defaultButton.PositionOffset_Y = -50;
			defaultButton.PositionScale_X = 1f;
			defaultButton.PositionScale_Y = 1f;
			defaultButton.SizeOffset_X = 200;
			defaultButton.SizeOffset_Y = 50;
			defaultButton.Text = MenuPlayConfigUI.localization.format("Default");
			defaultButton.TooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
			defaultButton.OnClicked += onClickedDefaultButton;
			defaultButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(defaultButton);
		}
	}
}
