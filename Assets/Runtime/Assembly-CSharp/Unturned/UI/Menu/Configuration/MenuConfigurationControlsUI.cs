////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuConfigurationControlsUI
	{
		private static byte[][] layouts = {
			new byte[] {ControlsSettings.UP, ControlsSettings.DOWN, ControlsSettings.LEFT, ControlsSettings.RIGHT, ControlsSettings.JUMP, ControlsSettings.SPRINT},
			new byte[] {ControlsSettings.CROUCH, ControlsSettings.PRONE, ControlsSettings.STANCE, ControlsSettings.LEAN_LEFT, ControlsSettings.LEAN_RIGHT, ControlsSettings.PERSPECTIVE, ControlsSettings.GESTURE},
			new byte[] {ControlsSettings.INTERACT, ControlsSettings.PRIMARY, ControlsSettings.SECONDARY},
			new byte[] {ControlsSettings.RELOAD, ControlsSettings.ATTACH, ControlsSettings.FIREMODE, ControlsSettings.TACTICAL, ControlsSettings.VISION, ControlsSettings.INSPECT, ControlsSettings.ROTATE, ControlsSettings.DEQUIP, ControlsSettings.SKIP_ACTION_CRAFTING_MENU},
			new byte[] {ControlsSettings.VOICE, ControlsSettings.GLOBAL, ControlsSettings.LOCAL, ControlsSettings.GROUP},
			new byte[] {ControlsSettings.HUD, ControlsSettings.OTHER, ControlsSettings.DASHBOARD, ControlsSettings.INVENTORY, ControlsSettings.CRAFTING, ControlsSettings.SKILLS, ControlsSettings.MAP, ControlsSettings.QUESTS, ControlsSettings.PLAYERS},
			new byte[] {ControlsSettings.LOCKER, ControlsSettings.ROLL_LEFT, ControlsSettings.ROLL_RIGHT, ControlsSettings.PITCH_UP, ControlsSettings.PITCH_DOWN, ControlsSettings.YAW_LEFT, ControlsSettings.YAW_RIGHT, ControlsSettings.THRUST_INCREASE, ControlsSettings.THRUST_DECREASE},
			new byte[] {ControlsSettings.MODIFY, ControlsSettings.SNAP, ControlsSettings.FOCUS, ControlsSettings.ASCEND, ControlsSettings.DESCEND, ControlsSettings.TOOL_0, ControlsSettings.TOOL_1, ControlsSettings.TOOL_2, ControlsSettings.TOOL_3, ControlsSettings.TERMINAL, ControlsSettings.SCREENSHOT, ControlsSettings.REFRESH_ASSETS, ControlsSettings.CLIPBOARD_DEBUG},
			new byte[] { ControlsSettings.PLUGIN_0, ControlsSettings.PLUGIN_1, ControlsSettings.PLUGIN_2, ControlsSettings.PLUGIN_3, ControlsSettings.PLUGIN_4, ControlsSettings.CUSTOM_MODAL },
			new byte[] { ControlsSettings.ITEM_0, ControlsSettings.ITEM_1, ControlsSettings.ITEM_2, ControlsSettings.ITEM_3, ControlsSettings.ITEM_4, ControlsSettings.ITEM_5, ControlsSettings.ITEM_6, ControlsSettings.ITEM_7, ControlsSettings.ITEM_8, ControlsSettings.ITEM_9 },
		};

		private static Local localization;
		private static Local localizationKeyCodes;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;
		private static ISleekButton defaultButton;

		private static ISleekFloat32Field sensitivityField;
		private static SleekButtonState sensitivityScalingModeButton;
		private static ISleekFloat32Field projectionRatioCoefficientField;
		private static ISleekToggle invertToggle;
		private static ISleekToggle invertFlightToggle;

		private static ISleekScrollView controlsBox;
		private static ISleekButton[] buttons;

		private static SleekButtonState aimingButton;
		private static SleekButtonState crouchingButton;
		private static SleekButtonState proningButton;
		private static SleekButtonState sprintingButton;
		private static SleekButtonState leaningButton;
		private static SleekButtonState voiceModeButton;

		[System.Obsolete]
		public static byte binding = byte.MaxValue;
		private static bool wasBindingThisFrame;
		private static int bindingFrameNumber;

		private static byte ActiveKeyBindingIndex
		{
#pragma warning disable
			get => binding;
#pragma warning restore

			set
			{
#pragma warning disable
				binding = value;
#pragma warning restore
				wasBindingThisFrame = true;
				bindingFrameNumber = Time.frameCount;
			}
		}

		public static bool IsRebindingKey => ActiveKeyBindingIndex != byte.MaxValue;

		public static bool ShouldGameIgnoreInput
		{
			get
			{
				return IsRebindingKey || wasBindingThisFrame;
			}
		}

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
			ActiveKeyBindingIndex = byte.MaxValue;

			container.AnimateOutOfView(0, 1);
		}

		public static void cancel()
		{
			ActiveKeyBindingIndex = byte.MaxValue;
		}

		public static void bind(KeyCode key)
		{
			ControlsSettings.bind(ActiveKeyBindingIndex, key);
			updateButton(ActiveKeyBindingIndex);

			cancel();
		}

		public static string getKeyCodeText(KeyCode key)
		{
			if (localizationKeyCodes == null)
			{
				localizationKeyCodes = Localization.read("/Shared/KeyCodes.dat");
			}

			string text = key.ToString();
			if (localizationKeyCodes.has(text))
			{
				text = localizationKeyCodes.format(text);
			}

			return text;
		}

		public static void updateButton(byte index)
		{
			KeyCode key = ControlsSettings.bindings[index].key;
			string text = getKeyCodeText(key);

			buttons[index].Text = localization.format("Key_" + index + "_Button", text);
		}

		private static void onTypedSensitivityField(ISleekFloat32Field field, float state)
		{
			ControlsSettings.mouseAimSensitivity = state;
		}

		private static void onTypedProjectionRatioCoefficientField(ISleekFloat32Field field, float state)
		{
			ControlsSettings.projectionRatioCoefficient = state;
		}

		private static void onToggledInvertToggle(ISleekToggle toggle, bool state)
		{
			ControlsSettings.invert = state;
		}

		private static void onToggledInvertFlightToggle(ISleekToggle toggle, bool state)
		{
			ControlsSettings.invertFlight = state;
		}

		private static void onSwappedAimingState(SleekButtonState button, int index)
		{
			ControlsSettings.aiming = (EControlMode) index;
		}

		private static void onSwappedCrouchingState(SleekButtonState button, int index)
		{
			ControlsSettings.crouching = (EControlMode) index;
		}

		private static void onSwappedProningState(SleekButtonState button, int index)
		{
			ControlsSettings.proning = (EControlMode) index;
		}

		private static void onSwappedSprintingState(SleekButtonState button, int index)
		{
			ControlsSettings.sprinting = (EControlMode) index;
		}

		private static void onSwappedLeaningState(SleekButtonState button, int index)
		{
			ControlsSettings.leaning = (EControlMode) index;
		}

		private static void OnSwappedVoiceMode(SleekButtonState button, int index)
		{
			ControlsSettings.voiceMode = (EControlMode) index;
		}

		private static void OnSwappedSensitivityScalingMode(SleekButtonState button, int index)
		{
			ControlsSettings.sensitivityScalingMode = (ESensitivityScalingMode) index;
		}

		private static void onClickedKeyButton(ISleekElement button)
		{
			byte newBindingIndex;
			for (newBindingIndex = 0; newBindingIndex < buttons.Length; ++newBindingIndex)
			{
				if (buttons[newBindingIndex] == button)
				{
					break;
				}
			}

			ActiveKeyBindingIndex = newBindingIndex;
			(button as ISleekButton).Text = localization.format("Key_" + ActiveKeyBindingIndex + "_Button", "?");
		}

		public static void bindOnGUI()
		{
			if (IsRebindingKey)
			{
				if (Event.current.type == EventType.KeyDown)
				{
					if (Event.current.keyCode == KeyCode.Backspace)
					{
						updateButton(ActiveKeyBindingIndex);
						cancel();
					}
					else if (Event.current.keyCode != KeyCode.Escape)
					{
						bind(Event.current.keyCode);
					}
				}
				else if (Event.current.type == EventType.MouseDown)
				{
					if (Event.current.button == 0)
					{
						bind(KeyCode.Mouse0);
					}
					else if (Event.current.button == 1)
					{
						bind(KeyCode.Mouse1);
					}
					else if (Event.current.button == 2)
					{
						bind(KeyCode.Mouse2);
					}
					else if (Event.current.button == 3)
					{
						bind(KeyCode.Mouse3);
					}
					else if (Event.current.button == 4)
					{
						bind(KeyCode.Mouse4);
					}
					else if (Event.current.button == 5)
					{
						bind(KeyCode.Mouse5);
					}
					else if (Event.current.button == 6)
					{
						bind(KeyCode.Mouse6);
					}
				}
				else if (Event.current.shift)
				{
					bind(KeyCode.LeftShift);
				}
			}
			else
			{
				if (wasBindingThisFrame && Time.frameCount > bindingFrameNumber)
				{
					wasBindingThisFrame = false;
				}
			}
		}

		// extra mouse buttons are only available in Update
		public static void bindUpdate()
		{
			// Nelson 2025-03-26: this broke after InputEx started ignoring KeyDown during key rebinding. (Oops!)
			// That's why we use Input.GetKeyDown here directly unlike the rest of the game.
			if (IsRebindingKey && Glazier.Get().ShouldGameProcessKeyDown)
			{
				if (Input.GetKeyDown(KeyCode.Mouse3))
				{
					bind(KeyCode.Mouse3);
				}
				else if (Input.GetKeyDown(KeyCode.Mouse4))
				{
					bind(KeyCode.Mouse4);
				}
				else if (Input.GetKeyDown(KeyCode.Mouse5))
				{
					bind(KeyCode.Mouse5);
				}
				else if (Input.GetKeyDown(KeyCode.Mouse6))
				{
					bind(KeyCode.Mouse6);
				}
			}
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
			ControlsSettings.restoreDefaults();

			updateAll();
		}

		private static void updateAll()
		{
			for (byte layoutIndex = 0; layoutIndex < layouts.Length; layoutIndex++)
			{
				for (byte bindIndex = 0; bindIndex < layouts[layoutIndex].Length; bindIndex++)
				{
					updateButton(layouts[layoutIndex][bindIndex]);
				}
			}

			leaningButton.state = (int) ControlsSettings.leaning;
			sprintingButton.state = (int) ControlsSettings.sprinting;
			proningButton.state = (int) ControlsSettings.proning;
			crouchingButton.state = (int) ControlsSettings.crouching;
			aimingButton.state = (int) ControlsSettings.aiming;
			sensitivityField.Value = ControlsSettings.mouseAimSensitivity;
			projectionRatioCoefficientField.Value = ControlsSettings.projectionRatioCoefficient;
			voiceModeButton.state = (int) ControlsSettings.voiceMode;
			invertToggle.Value = ControlsSettings.invert;
			invertFlightToggle.Value = ControlsSettings.invert;
			sensitivityScalingModeButton.state = (int) ControlsSettings.sensitivityScalingMode;
		}

		public MenuConfigurationControlsUI()
		{
			localization = Localization.read("/Menu/Configuration/MenuConfigurationControls.dat");

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
			ActiveKeyBindingIndex = byte.MaxValue;

			controlsBox = Glazier.Get().CreateScrollView();
			controlsBox.PositionOffset_X = -200;
			controlsBox.PositionOffset_Y = 100;
			controlsBox.PositionScale_X = 0.5f;
			controlsBox.SizeOffset_X = 430;
			controlsBox.SizeOffset_Y = -200;
			controlsBox.SizeScale_Y = 1;
			controlsBox.ScaleContentToWidth = true;
			container.AddChild(controlsBox);
			int verticalOffset = 0;

			invertToggle = Glazier.Get().CreateToggle();
			invertToggle.PositionOffset_Y = verticalOffset;
			invertToggle.SizeOffset_X = 40;
			invertToggle.SizeOffset_Y = 40;
			invertToggle.AddLabel(localization.format("Invert_Toggle_Label"), ESleekSide.RIGHT);
			invertToggle.OnValueChanged += onToggledInvertToggle;
			controlsBox.AddChild(invertToggle);
			verticalOffset += 50;

			invertFlightToggle = Glazier.Get().CreateToggle();
			invertFlightToggle.PositionOffset_Y = verticalOffset;
			invertFlightToggle.SizeOffset_X = 40;
			invertFlightToggle.SizeOffset_Y = 40;
			invertFlightToggle.AddLabel(localization.format("Invert_Flight_Toggle_Label"), ESleekSide.RIGHT);
			invertFlightToggle.OnValueChanged += onToggledInvertFlightToggle;
			controlsBox.AddChild(invertFlightToggle);
			verticalOffset += 50;

			sensitivityField = Glazier.Get().CreateFloat32Field();
			sensitivityField.PositionOffset_Y = verticalOffset;
			sensitivityField.SizeOffset_X = 200;
			sensitivityField.SizeOffset_Y = 30;
			sensitivityField.AddLabel(localization.format("Sensitivity_Field_Label"), ESleekSide.RIGHT);
			sensitivityField.OnValueChanged += onTypedSensitivityField;
			controlsBox.AddChild(sensitivityField);
			verticalOffset += 40;

			sensitivityScalingModeButton = new SleekButtonState(new GUIContent(localization.format("SensitivityScalingMode_ProjectionRatio"), localization.format("SensitivityScalingMode_ProjectionRatio_Tooltip")),
				new GUIContent(localization.format("SensitivityScalingMode_ZoomFactor"), localization.format("SensitivityScalingMode_ZoomFactor_Tooltip")),
				new GUIContent(localization.format("SensitivityScalingMode_Legacy"), localization.format("SensitivityScalingMode_Legacy_Tooltip")),
				new GUIContent(localization.format("SensitivityScalingMode_None"), localization.format("SensitivityScalingMode_None_Tooltip")));
			sensitivityScalingModeButton.PositionOffset_Y = verticalOffset;
			sensitivityScalingModeButton.SizeOffset_X = 200;
			sensitivityScalingModeButton.SizeOffset_Y = 30;
			sensitivityScalingModeButton.AddLabel(localization.format("SensitivityScalingMode_Label"), ESleekSide.RIGHT);
			sensitivityScalingModeButton.onSwappedState = OnSwappedSensitivityScalingMode;
			sensitivityScalingModeButton.UseContentTooltip = true;
			controlsBox.AddChild(sensitivityScalingModeButton);
			verticalOffset += 40;

			projectionRatioCoefficientField = Glazier.Get().CreateFloat32Field();
			projectionRatioCoefficientField.PositionOffset_Y = verticalOffset;
			projectionRatioCoefficientField.SizeOffset_X = 200;
			projectionRatioCoefficientField.SizeOffset_Y = 30;
			projectionRatioCoefficientField.TooltipText = localization.format("ProjectionRatioCoefficient_Tooltip");
			projectionRatioCoefficientField.AddLabel(localization.format("ProjectionRatioCoefficient_Label"), ESleekSide.RIGHT);
			projectionRatioCoefficientField.OnValueChanged += onTypedProjectionRatioCoefficientField;
			controlsBox.AddChild(projectionRatioCoefficientField);
			verticalOffset += 40;

			aimingButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
			aimingButton.PositionOffset_Y = verticalOffset;
			aimingButton.SizeOffset_X = 200;
			aimingButton.SizeOffset_Y = 30;
			aimingButton.AddLabel(localization.format("Aiming_Label"), ESleekSide.RIGHT);
			aimingButton.onSwappedState = onSwappedAimingState;
			controlsBox.AddChild(aimingButton);
			verticalOffset += 40;

			crouchingButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
			crouchingButton.PositionOffset_Y = verticalOffset;
			crouchingButton.SizeOffset_X = 200;
			crouchingButton.SizeOffset_Y = 30;
			crouchingButton.AddLabel(localization.format("Crouching_Label"), ESleekSide.RIGHT);
			crouchingButton.onSwappedState = onSwappedCrouchingState;
			controlsBox.AddChild(crouchingButton);
			verticalOffset += 40;

			proningButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
			proningButton.PositionOffset_Y = verticalOffset;
			proningButton.SizeOffset_X = 200;
			proningButton.SizeOffset_Y = 30;
			proningButton.AddLabel(localization.format("Proning_Label"), ESleekSide.RIGHT);
			proningButton.onSwappedState = onSwappedProningState;
			controlsBox.AddChild(proningButton);
			verticalOffset += 40;

			sprintingButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
			sprintingButton.PositionOffset_Y = verticalOffset;
			sprintingButton.SizeOffset_X = 200;
			sprintingButton.SizeOffset_Y = 30;
			sprintingButton.AddLabel(localization.format("Sprinting_Label"), ESleekSide.RIGHT);
			sprintingButton.onSwappedState = onSwappedSprintingState;
			controlsBox.AddChild(sprintingButton);
			verticalOffset += 40;

			leaningButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
			leaningButton.PositionOffset_Y = verticalOffset;
			leaningButton.SizeOffset_X = 200;
			leaningButton.SizeOffset_Y = 30;
			leaningButton.AddLabel(localization.format("Leaning_Label"), ESleekSide.RIGHT);
			leaningButton.onSwappedState = onSwappedLeaningState;
			controlsBox.AddChild(leaningButton);
			verticalOffset += 40;

			voiceModeButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
			voiceModeButton.PositionOffset_Y = verticalOffset;
			voiceModeButton.SizeOffset_X = 200;
			voiceModeButton.SizeOffset_Y = 30;
			voiceModeButton.AddLabel(localization.format("Voice_Mode_Label"), ESleekSide.RIGHT);
			voiceModeButton.onSwappedState = OnSwappedVoiceMode;
			controlsBox.AddChild(voiceModeButton);
			verticalOffset += 40;

			buttons = new ISleekButton[ControlsSettings.bindings.Length];

			for (byte layoutIndex = 0; layoutIndex < layouts.Length; layoutIndex++)
			{
				ISleekBox box = Glazier.Get().CreateBox();
				box.PositionOffset_Y = verticalOffset;
				box.SizeOffset_Y = 30;
				box.SizeScale_X = 1;
				box.Text = localization.format("Layout_" + layoutIndex);
				controlsBox.AddChild(box);
				verticalOffset += 40;

				for (byte bindIndex = 0; bindIndex < layouts[layoutIndex].Length; bindIndex++)
				{
					ISleekButton button = Glazier.Get().CreateButton();
					button.PositionOffset_Y = 40 + (bindIndex * 30);
					button.SizeOffset_Y = 30;
					button.SizeScale_X = 1;
					button.OnClicked += onClickedKeyButton;
					box.AddChild(button);
					verticalOffset += 30;

					buttons[layouts[layoutIndex][bindIndex]] = button;
				}

				verticalOffset += 10;
			}

			controlsBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10);

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

			updateAll();
		}
	}
}
