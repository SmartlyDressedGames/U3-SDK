////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ESensitivityScalingMode
	{
		/// <summary>
		/// Project current field of view onto screen compared to desired field of view.
		/// </summary>
		ProjectionRatio,

		/// <summary>
		/// Multiply sensitivity according to scope/optic zoom. For example an 8x zoom has 1/8th sensitivity.
		/// </summary>
		ZoomFactor,

		/// <summary>
		/// Preserve how sensitivity felt prior to 3.22.8.0 update.
		/// </summary>
		Legacy,

		/// <summary>
		/// Do not adjust sensitivity while aiming.
		/// </summary>
		None,
	}

	public class ControlsSettings
	{
		private const byte SAVEDATA_VERSION_ADDED_SENSITIVITY_SCALING_MODE = 19;
		private const byte SAVEDATA_VERSION_ADDED_SCALING_COEFFICIENT = 20;
		private const byte SAVEDATA_VERSION_ADDED_VOICE_TOGGLE = 21;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_VOICE_TOGGLE;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		public static readonly byte LEFT = 0;
		public static readonly byte RIGHT = 1;
		public static readonly byte UP = 2;
		public static readonly byte DOWN = 3;
		public static readonly byte JUMP = 4;

		public static readonly byte LEAN_LEFT = 5;
		public static readonly byte LEAN_RIGHT = 6;

		public static readonly byte PRIMARY = 7;
		public static readonly byte SECONDARY = 8;

		public static readonly byte INTERACT = 9;
		public static readonly byte CROUCH = 10;
		public static readonly byte PRONE = 11;
		public static readonly byte SPRINT = 12;

		public static readonly byte RELOAD = 13;
		public static readonly byte ATTACH = 14;
		public static readonly byte FIREMODE = 15;

		public static readonly byte DASHBOARD = 16;
		public static readonly byte INVENTORY = 17;
		public static readonly byte CRAFTING = 18;
		public static readonly byte SKILLS = 19;
		public static readonly byte MAP = 20;
		public static readonly byte QUESTS = 54;
		public static readonly byte PLAYERS = 21;
		public static readonly byte VOICE = 22;

		public static readonly byte MODIFY = 23;
		public static readonly byte SNAP = 24;
		public static readonly byte FOCUS = 25;
		public static readonly byte ASCEND = 51;
		public static readonly byte DESCEND = 52;
		public static readonly byte TOOL_0 = 26;
		public static readonly byte TOOL_1 = 27;
		public static readonly byte TOOL_2 = 28;
		public static readonly byte TOOL_3 = 50;
		public static readonly byte TERMINAL = 55;
		public static readonly byte SCREENSHOT = 56;
		public static readonly byte REFRESH_ASSETS = 57;
		public static readonly byte CLIPBOARD_DEBUG = 58;

		public static readonly byte HUD = 29;
		public static readonly byte OTHER = 30;

		public static readonly byte GLOBAL = 31;
		public static readonly byte LOCAL = 32;
		public static readonly byte GROUP = 33;

		public static readonly byte GESTURE = 34;
		public static readonly byte VISION = 35;
		public static readonly byte TACTICAL = 36;

		public static readonly byte PERSPECTIVE = 37;
		public static readonly byte DEQUIP = 38;
		public static readonly byte STANCE = 39;

		public static readonly byte ROLL_LEFT = 40;
		public static readonly byte ROLL_RIGHT = 41;
		public static readonly byte PITCH_UP = 42;
		public static readonly byte PITCH_DOWN = 43;
		public static readonly byte YAW_LEFT = 44;
		public static readonly byte YAW_RIGHT = 45;
		public static readonly byte THRUST_INCREASE = 46;
		public static readonly byte THRUST_DECREASE = 47;
		public static readonly byte LOCKER = 53;

		public static readonly byte INSPECT = 48;
		public static readonly byte ROTATE = 49;

		// Keybinds that serverside plugins can listen for.
		public static readonly byte PLUGIN_0 = 59;
		public static readonly byte PLUGIN_1 = 60;
		public static readonly byte PLUGIN_2 = 61;
		public static readonly byte PLUGIN_3 = 62;
		public static readonly byte PLUGIN_4 = 63;
		public static readonly byte NUM_PLUGIN_KEYS = 5;

		public static readonly string[] PLUGIN_KEY_TOKENS;

		// Item hotbar shown at bottom of HUD. (default numeric keys)
		public const byte ITEM_0 = 64;
		public const byte ITEM_1 = 65;
		public const byte ITEM_2 = 66;
		public const byte ITEM_3 = 67;
		public const byte ITEM_4 = 68;
		public const byte ITEM_5 = 69;
		public const byte ITEM_6 = 70;
		public const byte ITEM_7 = 71;
		public const byte ITEM_8 = 72;
		public const byte ITEM_9 = 73;
		public const byte NUM_ITEM_HOTBAR_KEYS = 10;

		/// <summary>
		/// When held the cursor is released.
		/// </summary>
		public const byte CUSTOM_MODAL = 74;

		/// <summary>
		/// If held while clicking a blueprint action in the item context menu, the crafting menu is bypassed.
		/// </summary>
		public const byte SKIP_ACTION_CRAFTING_MENU = 75;

		/// <summary>
		/// Replace instances of <plugin_num/> with their bound key text.
		/// Allows server effects to display plugin hotkeys.
		/// </summary>
		public static void formatPluginHotkeysIntoText(ref string text)
		{
			for (int pluginKeyIndex = 0; pluginKeyIndex < NUM_PLUGIN_KEYS; pluginKeyIndex++)
			{
				KeyCode boundKeyCode = getPluginKeyCode(pluginKeyIndex);
				string token = PLUGIN_KEY_TOKENS[pluginKeyIndex];
				string displayKey = MenuConfigurationControlsUI.getKeyCodeText(boundKeyCode);

				text = text.Replace(token, displayKey);
			}
		}

		/// <summary>
		/// Item 0 is "1" and item 9 is "0"
		/// </summary>
		public static string getEquipmentHotkeyText(int index)
		{
			KeyCode keyCode = getEquipmentHotbarKeyCode(index);
			return MenuConfigurationControlsUI.getKeyCodeText(keyCode);
		}

		/// <summary>
		/// Multiplier for Input.GetAxis("mouse_x") and Input.GetAxis("mouse_y")
		/// </summary>
		public static float mouseAimSensitivity;

		public static bool invert;
		public static bool invertFlight;
		public static EControlMode aiming;
		public static EControlMode crouching;
		public static EControlMode proning;
		public static EControlMode sprinting;
		public static EControlMode leaning;
		public static EControlMode voiceMode;
		public static ESensitivityScalingMode sensitivityScalingMode;
		public static float projectionRatioCoefficient;

		private static ControlBinding[] _bindings = new ControlBinding[76];
		public static ControlBinding[] bindings => _bindings;

		public static KeyCode left => bindings[LEFT].key;

		public static KeyCode up => bindings[UP].key;

		public static KeyCode right => bindings[RIGHT].key;

		public static KeyCode down => bindings[DOWN].key;

		public static KeyCode jump => bindings[JUMP].key;

		public static KeyCode leanLeft => bindings[LEAN_LEFT].key;

		public static KeyCode leanRight => bindings[LEAN_RIGHT].key;

		public static KeyCode rollLeft => bindings[ROLL_LEFT].key;

		public static KeyCode rollRight => bindings[ROLL_RIGHT].key;

		public static KeyCode pitchUp => bindings[PITCH_UP].key;

		public static KeyCode pitchDown => bindings[PITCH_DOWN].key;

		public static KeyCode primary => bindings[PRIMARY].key;

		public static KeyCode yawLeft => bindings[YAW_LEFT].key;

		public static KeyCode yawRight => bindings[YAW_RIGHT].key;

		public static KeyCode thrustIncrease => bindings[THRUST_INCREASE].key;

		public static KeyCode thrustDecrease => bindings[THRUST_DECREASE].key;

		public static KeyCode locker => bindings[LOCKER].key;

		public static KeyCode secondary => bindings[SECONDARY].key;

		// gun

		public static KeyCode reload => bindings[RELOAD].key;

		public static KeyCode attach => bindings[ATTACH].key;

		public static KeyCode firemode => bindings[FIREMODE].key;

		// ui

		public static KeyCode dashboard => bindings[DASHBOARD].key;

		public static KeyCode inventory => bindings[INVENTORY].key;

		public static KeyCode crafting => bindings[CRAFTING].key;

		public static KeyCode skills => bindings[SKILLS].key;

		public static KeyCode map => bindings[MAP].key;

		public static KeyCode quests => bindings[QUESTS].key;

		public static KeyCode players => bindings[PLAYERS].key;

		public static KeyCode voice => bindings[VOICE].key;

		// player

		public static KeyCode interact => bindings[INTERACT].key;

		public static KeyCode crouch => bindings[CROUCH].key;

		public static KeyCode prone => bindings[PRONE].key;

		public static KeyCode stance => bindings[STANCE].key;

		public static KeyCode sprint => bindings[SPRINT].key;

		// editor

		public static KeyCode modify => bindings[MODIFY].key;

		public static KeyCode snap => bindings[SNAP].key;

		public static KeyCode focus => bindings[FOCUS].key;

		public static KeyCode ascend => bindings[ASCEND].key;

		public static KeyCode descend => bindings[DESCEND].key;

		public static KeyCode tool_0 => bindings[TOOL_0].key;

		public static KeyCode tool_1 => bindings[TOOL_1].key;

		public static KeyCode tool_2 => bindings[TOOL_2].key;

		public static KeyCode tool_3 => bindings[TOOL_3].key;

		public static KeyCode terminal => bindings[TERMINAL].key;

		public static KeyCode screenshot => bindings[SCREENSHOT].key;

		public static KeyCode refreshAssets => bindings[REFRESH_ASSETS].key;

		public static KeyCode clipboardDebug => bindings[CLIPBOARD_DEBUG].key;

		// general

		public static KeyCode hud => bindings[HUD].key;

		// general

		public static KeyCode other => bindings[OTHER].key;

		public static KeyCode global => bindings[GLOBAL].key;

		public static KeyCode local => bindings[LOCAL].key;

		public static KeyCode group => bindings[GROUP].key;

		public static KeyCode gesture => bindings[GESTURE].key;

		public static KeyCode vision => bindings[VISION].key;

		public static KeyCode tactical => bindings[TACTICAL].key;

		public static KeyCode perspective => bindings[PERSPECTIVE].key;

		public static KeyCode dequip => bindings[DEQUIP].key;

		public static KeyCode inspect => bindings[INSPECT].key;

		public static KeyCode rotate => bindings[ROTATE].key;

		public static KeyCode getPluginKeyCode(int index)
		{
			return bindings[PLUGIN_0 + index].key;
		}

		public static KeyCode getEquipmentHotbarKeyCode(int index)
		{
			return bindings[ITEM_0 + index].key;
		}

		/// <summary>
		/// When held the cursor is released.
		/// </summary>
		public static KeyCode CustomModal => bindings[CUSTOM_MODAL].key;

		/// <summary>
		/// If held while clicking a blueprint action in the item context menu, the crafting menu is bypassed.
		/// </summary>
		public static KeyCode SkipActionCraftingMenu => bindings[SKIP_ACTION_CRAFTING_MENU].key;

		private static bool isTooImportantToMessUp(KeyCode key)
		{
			if (key == KeyCode.Mouse0)
			{
				return true;
			}
			else if (key == KeyCode.Mouse1)
			{
				return true;
			}

			return false;
		}

		public static void bind(byte index, KeyCode key)
		{
			if (index == HUD)
			{
				if (isTooImportantToMessUp(key))
				{
					key = KeyCode.Home;
				}
			}
			else if (index == OTHER)
			{
				if (isTooImportantToMessUp(key))
				{
					key = KeyCode.LeftControl;
				}
			}
			else if (index == TERMINAL)
			{
				if (isTooImportantToMessUp(key))
				{
					key = KeyCode.BackQuote;
				}
			}
			else if (index == REFRESH_ASSETS)
			{
				if (isTooImportantToMessUp(key))
				{
					key = KeyCode.PageUp;
				}
			}

			if (bindings[index] == null)
			{
				bindings[index] = new ControlBinding(key);
				return;
			}

			bindings[index].key = key;
		}

		public static void restoreDefaults()
		{
			bind(LEFT, KeyCode.A);
			bind(RIGHT, KeyCode.D);
			bind(UP, KeyCode.W);
			bind(DOWN, KeyCode.S);
			bind(JUMP, KeyCode.Space);

			bind(LEAN_LEFT, KeyCode.Q);
			bind(LEAN_RIGHT, KeyCode.E);

			bind(PRIMARY, KeyCode.Mouse0);
			bind(SECONDARY, KeyCode.Mouse1);

			bind(INTERACT, KeyCode.F);
			bind(CROUCH, KeyCode.X);
			bind(PRONE, KeyCode.Z);
			bind(STANCE, KeyCode.O);
			bind(SPRINT, KeyCode.LeftShift);

			bind(RELOAD, KeyCode.R);
			bind(ATTACH, KeyCode.T);
			bind(FIREMODE, KeyCode.V);

			bind(DASHBOARD, KeyCode.Tab);
			bind(INVENTORY, KeyCode.G);
			bind(CRAFTING, KeyCode.Y);
			bind(SKILLS, KeyCode.U);
			bind(MAP, KeyCode.M);
			bind(QUESTS, KeyCode.I);
			bind(PLAYERS, KeyCode.P);
			bind(VOICE, KeyCode.LeftAlt);

			bind(MODIFY, KeyCode.LeftShift);
			bind(SNAP, KeyCode.LeftControl);
			bind(FOCUS, KeyCode.F);
			bind(ASCEND, KeyCode.Q);
			bind(DESCEND, KeyCode.E);
			bind(TOOL_0, KeyCode.Q);
			bind(TOOL_1, KeyCode.W);
			bind(TOOL_2, KeyCode.E);
			bind(TOOL_3, KeyCode.R);
			bind(TERMINAL, KeyCode.BackQuote); // BackQuote = Tilde
			bind(SCREENSHOT, KeyCode.Insert);
			bind(REFRESH_ASSETS, KeyCode.PageUp);
			bind(CLIPBOARD_DEBUG, KeyCode.PageDown);

			bind(HUD, KeyCode.Home);
			bind(OTHER, KeyCode.LeftControl);

			bind(GLOBAL, KeyCode.J);
			bind(LOCAL, KeyCode.K);
			bind(GROUP, KeyCode.L);

			bind(GESTURE, KeyCode.C);
			bind(VISION, KeyCode.N);
			bind(TACTICAL, KeyCode.B);

			bind(PERSPECTIVE, KeyCode.H);
			bind(DEQUIP, KeyCode.CapsLock);

			bind(ROLL_LEFT, KeyCode.LeftArrow);
			bind(ROLL_RIGHT, KeyCode.RightArrow);
			bind(PITCH_UP, KeyCode.UpArrow);
			bind(PITCH_DOWN, KeyCode.DownArrow);
			bind(YAW_LEFT, KeyCode.A);
			bind(YAW_RIGHT, KeyCode.D);
			bind(THRUST_INCREASE, KeyCode.W);
			bind(THRUST_DECREASE, KeyCode.S);
			bind(LOCKER, KeyCode.O);

			bind(INSPECT, KeyCode.F);
			bind(ROTATE, KeyCode.R);

			bind(PLUGIN_0, KeyCode.Comma);
			bind(PLUGIN_1, KeyCode.Period);
			bind(PLUGIN_2, KeyCode.Slash);
			bind(PLUGIN_3, KeyCode.Semicolon);
			bind(PLUGIN_4, KeyCode.Quote);

			// Item hotbar shown at bottom of HUD.
			bind(ITEM_0, KeyCode.Alpha1);
			bind(ITEM_1, KeyCode.Alpha2);
			bind(ITEM_2, KeyCode.Alpha3);
			bind(ITEM_3, KeyCode.Alpha4);
			bind(ITEM_4, KeyCode.Alpha5);
			bind(ITEM_5, KeyCode.Alpha6);
			bind(ITEM_6, KeyCode.Alpha7);
			bind(ITEM_7, KeyCode.Alpha8);
			bind(ITEM_8, KeyCode.Alpha9);
			bind(ITEM_9, KeyCode.Alpha0);

			bind(CUSTOM_MODAL, KeyCode.Keypad0);
			bind(SKIP_ACTION_CRAFTING_MENU, KeyCode.LeftShift);

			aiming = EControlMode.HOLD;
			crouching = EControlMode.TOGGLE;
			proning = EControlMode.TOGGLE;
			sprinting = EControlMode.HOLD;
			leaning = EControlMode.HOLD;
			voiceMode = EControlMode.HOLD;
			sensitivityScalingMode = ESensitivityScalingMode.ProjectionRatio;
			projectionRatioCoefficient = 1.0f;

			mouseAimSensitivity = 0.2f;
			invert = false;
			invertFlight = false;
		}

		public static void load()
		{
			restoreDefaults();

			if (ReadWrite.fileExists("/Controls.dat", true))
			{
				Block block = ReadWrite.readBlock("/Controls.dat", true, 0);

				if (block != null)
				{
					byte version = block.readByte();

					if (version > 10)
					{
						mouseAimSensitivity = block.readSingle();
						if (version < 16)
						{
							mouseAimSensitivity = 0.2f;
						}
						else if (version < 18)
						{
							// This version removed the mouse_x/y/z 0.1 multiplier in the legacy Input Manager,
							// as well as the 2.0 multiplier in ControlsSettings.look.
							mouseAimSensitivity *= 0.2f;
						}

						invert = block.readBoolean();

						if (version > 13)
						{
							invertFlight = block.readBoolean();
						}
						else
						{
							invertFlight = false;
						}

						if (version > 11)
						{
							aiming = (EControlMode) block.readByte();
							crouching = (EControlMode) block.readByte();
							proning = (EControlMode) block.readByte();
						}
						else
						{
							aiming = EControlMode.HOLD;
							crouching = EControlMode.TOGGLE;
							proning = EControlMode.TOGGLE;
						}

						if (version > 12)
						{
							sprinting = (EControlMode) block.readByte();
						}
						else
						{
							sprinting = EControlMode.HOLD;
						}

						if (version > 14)
						{
							leaning = (EControlMode) block.readByte();
						}
						else
						{
							leaning = EControlMode.HOLD;
						}

						byte count = block.readByte();
						for (byte index = 0; index < count; index++)
						{
							if (index >= bindings.Length)
							{
								block.readByte();
								continue;
							}

							ushort key = block.readUInt16();
							bind(index, (KeyCode) key);
						}

						if (version < 17)
						{
							bind(DEQUIP, KeyCode.CapsLock);
						}

						if (version < SAVEDATA_VERSION_ADDED_SENSITIVITY_SCALING_MODE)
						{
							sensitivityScalingMode = ESensitivityScalingMode.ProjectionRatio;
						}
						else
						{
							sensitivityScalingMode = (ESensitivityScalingMode) block.readByte();
						}

						if (version < SAVEDATA_VERSION_ADDED_SCALING_COEFFICIENT)
						{
							projectionRatioCoefficient = 1.0f;
						}
						else
						{
							projectionRatioCoefficient = block.readSingle();
						}

						if (version >= SAVEDATA_VERSION_ADDED_VOICE_TOGGLE)
						{
							voiceMode = (EControlMode) block.readByte();
						}
						else
						{
							voiceMode = EControlMode.HOLD;
						}
					}
				}
			}
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION_NEWEST);

			block.writeSingle(mouseAimSensitivity);
			block.writeBoolean(invert);
			block.writeBoolean(invertFlight);
			block.writeByte((byte) aiming);
			block.writeByte((byte) crouching);
			block.writeByte((byte) proning);
			block.writeByte((byte) sprinting);
			block.writeByte((byte) leaning);

			block.writeByte((byte) bindings.Length);
			for (byte index = 0; index < bindings.Length; index++)
			{
				ControlBinding binding = bindings[index];

				block.writeUInt16((ushort) binding.key);
			}

			block.writeByte((byte) sensitivityScalingMode);
			block.writeSingle(projectionRatioCoefficient);
			block.writeByte((byte) voiceMode);

			ReadWrite.writeBlock("/Controls.dat", true, block);
		}

		static ControlsSettings()
		{
			for (int bindingIndex = 0; bindingIndex < bindings.Length; bindingIndex++)
			{
				bindings[bindingIndex] = new ControlBinding(KeyCode.F);
			}

			PLUGIN_KEY_TOKENS = new string[NUM_PLUGIN_KEYS];
			for (int pluginKeyIndex = 0; pluginKeyIndex < NUM_PLUGIN_KEYS; pluginKeyIndex++)
			{
				string token = string.Format("<plugin_{0}/>", pluginKeyIndex);
				PLUGIN_KEY_TOKENS[pluginKeyIndex] = token;
			}
		}
	}
}
