////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ECrosshairShape
	{
		Line,
		Classic,
	}

	public enum EHitmarkerStyle
	{
		Animated,
		Classic,
	}

	public enum EVehicleThirdPersonCameraMode
	{
		/// <summary>
		/// Camera does not rotate when the car rotates.
		/// </summary>
		RotationDetached,

		/// <summary>
		/// Camera rotates when the car rotates.
		/// </summary>
		RotationAttached,
	}

	public enum EDamageFlinchMode
	{
		/// <summary>
		/// If hit from the left view rolls right, if hit from the right view rolls left. This may reduce motion
		/// sickness for some players.
		/// </summary>
		RollOnly,

		/// <summary>
		/// Rotate on all axes according to damage direction. This may induce motion sickness.
		/// </summary>
		Directional,
	}

	public class OptionsSettings
	{
		private const byte SAVEDATA_VERSION_ADDED_LOADING_SCREEN_MUSIC = 37;
		private const byte SAVEDATA_VERSION_ADDED_SCREENSHOT_SIZE = 38;
		private const byte SAVEDATA_VERSION_ADDED_SCREENSHOT_SUPERSAMPLING = 39;
		private const byte SAVEDATA_VERSION_ADDED_LOADING_SCREEN_SCREENSHOTS = 40;
		private const byte SAVEDATA_VERSION_ADDED_STATIC_CROSSHAIR = 41;
		private const byte SAVEDATA_VERSION_ADDED_CROSSHAIR_SHAPE = 42;
		private const byte SAVEDATA_VERSION_ADDED_CROSSHAIR_AND_HITMARKER_ALPHA = 43;
		private const byte SAVEDATA_VERSION_ADDED_HITMARKER_STYLE = 44;
		/// <summary>
		/// Unfortunately the version which added hitmarker style saved but didn't actually load (sigh).
		/// </summary>
		private const byte SAVEDATA_VERSION_ADDED_HITMARKER_STYLE_FIX = 45;
		private const byte SAVEDATA_VERSION_REMOVED_MATCHMAKING = 46;
		private const byte SAVEDATA_VERSION_ADDED_VOICE_ALWAYS_RECORDING = 47;
		/// <summary>
		/// Nelson 2023-12-28: this option was causing players to crash in the 3.23.14.0 update. Hopefully
		/// it's resolved for the patch, but to be safe it will default to false.
		/// </summary>
		private const byte SAVEDATA_VERSION_RESET_VOICE_ALWAYS_RECORDING = 48;
		private const byte SAVEDATA_VERSION_ADDED_NAMETAG_FADEOUT_OPT = 49;
		private const byte SAVEDATA_VERSION_ADDED_GAME_VOLUME = 50;
		private const byte SAVEDATA_VERSION_ADDED_UNFOCUSED_VOLUME = 51;
		private const byte SAVEDATA_VERSION_ADDED_VEHICLE_THIRD_PERSON_CAMERA_MODE = 52;
		private const byte SAVEDATA_VERSION_REPLACTED_MUSIC_TOGGLE_WITH_VOLUMES = 53;
		private const byte SAVEDATA_VERSION_ADDED_MUSIC_MASTER_VOLUME = 54;
		private const byte SAVEDATA_VERSION_ADDED_ATMOSPHERE_VOLUME = 55;
		private const byte SAVEDATA_VERSION_SEPARATED_AIRCRAFT_THIRD_PERSON_CAMERA_MODE = 56;
		private const byte SAVEDATA_VERSION_ADDED_ONLINE_SAFETY_MENU = 57;
		private const byte SAVEDATA_VERSION_ADDED_FLASHBANG_BRIGHTNESS = 58;
		private const byte SAVEDATA_VERSION_ADDED_CAMERA_SHAKE_INTENSITY = 59;
		private const byte SAVEDATA_VERSION_ADDED_DAMAGE_FLINCH_OPTIONS = 60;
		private const byte SAVEDATA_VERSION_ADDED_SHOW_OUTBOUND_VOICE_CHAT_OFF_HINT = 61;
		private const byte SAVEDATA_VERSION_ADDED_SPRINT_FOV_OPTION = 62;
		private const byte SAVEDATA_VERSION_SEPARATED_STREAMER_MODE = 63;
		private const byte SAVEDATA_VERSION_ADDED_CLICK_BLUEPRINT_TO_CRAFT = 64;
		private const byte SAVEDATA_VERSION_ADDED_VIEWMODEL_BOB_SCALE = 65;
		private const byte SAVEDATA_VERSION_ADDED_ZOMBIE_FOOTSTEPS_VOLUME = 66;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_ZOMBIE_FOOTSTEPS_VOLUME;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		public static readonly byte MIN_FOV = 60;
		public static readonly byte MAX_FOV = 40;

		private static float _fov;
		public static float fov
		{
			get => _fov;

			set
			{
				_fov = value;
				CacheVerticalFOV();
			}
		}

		private static float _cachedVerticalFOV;
		public static float DesiredVerticalFieldOfView => _cachedVerticalFOV;

		/// <summary>
		/// Prior to 3.22.8.0 all scopes/optics had a base fov of 90 degrees.
		/// </summary>
		public static float GetZoomBaseFieldOfView()
		{
			return ControlsSettings.sensitivityScalingMode == ESensitivityScalingMode.Legacy ? 90.0f : DesiredVerticalFieldOfView;
		}

		private static void CacheVerticalFOV()
		{
			if (Provider.preferenceData != null &&
				Provider.preferenceData.Graphics != null &&
				Provider.preferenceData.Graphics.Override_Vertical_Field_Of_View > 0.5f)
			{
				// Clamp, otherwise players will use extreme values to push near clip plane through walls.
				_cachedVerticalFOV = Mathf.Clamp(Provider.preferenceData.Graphics.Override_Vertical_Field_Of_View, 1.0f, 100.0f);
			}
			else
			{
				_cachedVerticalFOV = MIN_FOV + (MAX_FOV * _fov);
			}
		}

		public const float DEFAULT_MASTER_VOLUME = 1.0f;
		public static float volume;

		public const float DEFAULT_UNFOCUSED_VOLUME = 0.5f;
		public static float UnfocusedVolume
		{
			get => UnturnedMasterVolume.UnfocusedVolume;
			set => UnturnedMasterVolume.UnfocusedVolume = value;
		}

		public const float DEFAULT_MUSIC_MASTER_VOLUME = 0.7f;
		private static float _musicMasterVolume;
		public static float MusicMasterVolume
		{
			get => _musicMasterVolume;
			set
			{
				_musicMasterVolume = value;
#if !DEDICATED_SERVER
				UnturnedAudioMixer.SetMusicMasterVolume(value);
#endif
			}
		}

		private const float DEFAULT_GAME_VOLUME = 0.7f;
		private static float _gameVolume;
		public static float gameVolume
		{
			get => _gameVolume;
			set
			{
				_gameVolume = value;
#if !DEDICATED_SERVER
				UnturnedAudioMixer.SetDefaultVolume(value);
#endif
			}
		}

		private const float DEFAULT_VOICE_VOLUME = 0.7f;
		private static float _voiceVolume;
		public static float voiceVolume
		{
			get => _voiceVolume;
			set
			{
				_voiceVolume = value;
#if !DEDICATED_SERVER
				UnturnedAudioMixer.SetVoiceVolume(value);
#endif
			}
		}

		private const float DEFAULT_LOADING_SCREEN_MUSIC_VOLUME = 0.5f;
		public static float loadingScreenMusicVolume;
		public const float DEFAULT_DEATH_MUSIC_VOLUME = 0.7f;
		public static float deathMusicVolume;

		public const float DEFAULT_MAIN_MENU_MUSIC_VOLUME = 0.7f;
		private static float _mainMenuMusicVolume;
		public static float MainMenuMusicVolume
		{
			get => _mainMenuMusicVolume;
			set
			{
				_mainMenuMusicVolume = value;
#if !DEDICATED_SERVER
				UnturnedAudioMixer.SetMainMenuMusicVolume(value);
#endif
			}
		}

		public const float DEFAULT_ATMOSPHERE_VOLUME = 0.7f;
		private static float _atmosphereVolume;
		public static float AtmosphereVolume
		{
			get => _atmosphereVolume;
			set
			{
				_atmosphereVolume = value;
#if !DEDICATED_SERVER
				UnturnedAudioMixer.SetAtmosphereVolume(value);
#endif
			}
		}

		public const float DEFAULT_ZOMBIE_FOOTSTEPS_VOLUME = 0.7f;
		internal static float _zombieFootstepsVolume;
		public static float ZombieFootstepsVolume
		{
			get => _zombieFootstepsVolume;
			set
			{
				_zombieFootstepsVolume = value;
#if !DEDICATED_SERVER
				UnturnedAudioMixer.SetZombieFootstepsVolume(value);
#endif
			}
		}

		public const float DEFAULT_AMBIENT_MUSIC_VOLUME = 0.7f;
		public static float ambientMusicVolume;
		public static bool debug;
		public static bool splashscreen;
		public static bool timer;
		//public static bool physics;
		public static bool gore;
		public static bool EnableGore
		{
			get => gore;
			set
			{
				if (gore != value)
				{
					gore = value;
					OnEnableGoreChanged?.Invoke();
				}
			}
		}
		public static event System.Action OnEnableGoreChanged;
		public static bool filter;
		public static bool chatText;
		public static bool chatVoiceIn;
		public static bool chatVoiceOut;
		public static bool EnableOutboundVoiceChat
		{
			get => chatVoiceOut;
			set
			{
				if (chatVoiceOut != value)
				{
					chatVoiceOut = value;
					OnEnableOutboundVoiceChatChanged?.Invoke();
				}
			}
		}
		public static event System.Action OnEnableOutboundVoiceChatChanged;

		private static bool _metric;
		public static bool metric
		{
			get => _metric;
			set
			{
				if (_metric != value)
				{
					_metric = value;
					OnUnitSystemChanged?.Invoke();
				}
			}
		}
		public static event System.Action OnUnitSystemChanged;

		private static bool _showOutboundVoiceChatOffHint;
		public static bool ShowOutboundVoiceChatOffHint
		{
			get => _showOutboundVoiceChatOffHint;
			set
			{
				if (_showOutboundVoiceChatOffHint != value)
				{
					_showOutboundVoiceChatOffHint = value;
					OnShowOutboundVoiceChatOffHintChanged?.Invoke();
				}
			}
		}
		public static event System.Action OnShowOutboundVoiceChatOffHintChanged;

		public static bool ShouldClickBlueprintToCraft
		{
			get;
			set;
		}

		public static bool talk;
		public static bool hints;
		public static bool ambience;

		public static bool proUI
		{
			get => SleekCustomization.darkTheme;
			set
			{
				SleekCustomization.darkTheme = value;
				OnThemeChanged?.Invoke();
			}
		}

		public static bool ShouldHitmarkersFollowWorldPosition
		{
#pragma warning disable
			get { return hitmarker; }
			set { hitmarker = value; }
#pragma warning restore
		}

		private static bool _voiceAlwaysRecording;
		/// <summary>
		/// If false, call Start and Stop recording before and after push-to-talk key is pressed. This was the
		/// original default behavior, but causes a hitch for some players. As a workaround we can always keep
		/// the microphone rolling and only send data when the push-to-talk key is held. (public issue #4248)
		/// </summary>
		public static bool VoiceAlwaysRecording
		{
			get => _voiceAlwaysRecording;
			set
			{
				if (_voiceAlwaysRecording != value)
				{
					_voiceAlwaysRecording = value;
					OnVoiceAlwaysRecordingChanged?.Invoke();
				}
			}
		}
		public static event System.Action OnVoiceAlwaysRecordingChanged;

		/// <summary>
		/// If true, group member name labels fade out when near the center of the screen.
		/// Defaults to true.
		/// </summary>
		public static bool shouldNametagFadeOut;

		[System.Obsolete("Renamed to ShouldHitmarkersFollowWorldPosition")]
		public static bool hitmarker;

		[System.Obsolete("Separated into ShouldAnonymizeMultiplayerDetails and ShouldHideRichPresence")]
		public static bool streamer;

		/// <summary>
		/// If true, hide identifiable details of other multiplayer clients like avatars, player names, number of
		/// players online, server name, etc. Live streamers may find this useful to help prevent stream sniping.
		///
		/// Separated from the older "streamer mode" option.
		/// </summary>
		public static bool ShouldAnonymizeMultiplayerDetails
		{
#pragma warning disable
			get => streamer;
			set => streamer = value;
#pragma warning restore
		}

		private static bool _hideRichPresence;
		/// <summary>
		/// If true, don't share details like "editing map X" or "join" with Steam. Useful for anyone who might be
		/// targeted / followed into servers, or who has a project to keep secret.
		///
		/// Separated from the older "streamer mode" option.
		/// </summary>
		public static bool ShouldHideRichPresence
		{
			get => _hideRichPresence;
			set
			{
				if (_hideRichPresence != value)
				{
					_hideRichPresence = value;
					Provider.updateRichPresence();
				}
			}
		}

		public static bool featuredWorkshop;
		public static bool showHotbar;
		public static bool pauseWhenUnfocused;
		public static int screenshotSizeMultiplier;
		public static bool enableScreenshotSupersampling;
		public static bool enableScreenshotsOnLoadingScreen;
		public static bool useStaticCrosshair;
		public static float staticCrosshairSize;
		public static ECrosshairShape crosshairShape;

		/// <summary>
		/// Controls whether hitmarkers are animated outward (newer) or just a static image ("classic"). 
		/// </summary>
		public static EHitmarkerStyle hitmarkerStyle;

		/// <summary>
		/// Determines how camera follows vehicle in third-person view.
		/// </summary>
		public static EVehicleThirdPersonCameraMode vehicleThirdPersonCameraMode;

		/// <summary>
		/// Determines how camera follows aircraft vehicle in third-person view.
		/// </summary>
		public static EVehicleThirdPersonCameraMode vehicleAircraftThirdPersonCameraMode;

		/// <summary>
		/// [0, 1] Blend factor between black and flashbang's desired color.
		/// </summary>
		public static float flashbangBrightness;

		/// <summary>
		/// [0, 1] Multiplier for shake from <see cref="EffectAsset.cameraShakeMagnitudeDegrees"/>.
		/// </summary>
		public static float cameraShakeIntensity;

		/// <summary>
		/// Controls whether camera is constrained to roll-only or all axes.
		/// </summary>
		public static EDamageFlinchMode damageFlinchMode;

		/// <summary>
		/// Multiplier for flinch away from damage source in <see cref="PlayerLook.FlinchFromDamage(byte, Vector3)"/>.
		/// </summary>
		public static float damageFlinchIntensity;

		/// <summary>
		/// [0, 1] Intensity of FOV boost while sprinting.
		/// </summary>
		public static float sprintFovBoostIntensity;

		/// <summary>
		/// [0, 1] Intensity of first-person motion caused by walking.
		/// </summary>
		public static float viewmodelBobScale;

		public static Color crosshairColor;
		public static Color hitmarkerColor;
		public static Color criticalHitmarkerColor;
		public static Color cursorColor
		{
			get => SleekCustomization.cursorColor;
			set => SleekCustomization.cursorColor = value;
		}

		public static Color backgroundColor
		{
			get => SleekCustomization.backgroundColor;
			set
			{
				SleekCustomization.backgroundColor = value;
				OnCustomColorsChanged?.Invoke();
			}
		}

		public static Color foregroundColor
		{
			get => SleekCustomization.foregroundColor;
			set
			{
				SleekCustomization.foregroundColor = value;
				OnCustomColorsChanged?.Invoke();
			}
		}

		public static Color fontColor
		{
			get => SleekCustomization.fontColor;
			set
			{
				SleekCustomization.fontColor = value;
				OnCustomColorsChanged?.Invoke();
			}
		}
		
		public static Color shadowColor
		{
			get => SleekCustomization.shadowColor;
			set
			{
				SleekCustomization.shadowColor = value;
				OnCustomColorsChanged?.Invoke();
			}
		}

		public static Color badColor
		{
			get => SleekCustomization.badColor;
			set
			{
				SleekCustomization.badColor = value;
				OnCustomColorsChanged?.Invoke();
			}
		}

		/// <summary>
		/// Invoked when custom UI colors are set.
		/// </summary>
		public static event System.Action OnCustomColorsChanged;

		/// <summary>
		/// Invoked when dark/light theme is set.
		/// </summary>
		public static event System.Action OnThemeChanged;

		/// <summary>
		/// Number of times the player has clicked "Proceed" in the online safety menu.
		/// </summary>
		public static int onlineSafetyMenuProceedCount;
		/// <summary>
		/// If true, "don't show again" is checked in the online safety menu.
		/// </summary>
		public static bool wantsToHideOnlineSafetyMenu;

		/// <summary>
		/// Prevents menu from being shown twice without a restart.
		/// </summary>
		internal static bool didProceedThroughOnlineSafetyMenuThisSession;

		internal static bool ShouldShowOnlineSafetyMenu
		{
			get => (!wantsToHideOnlineSafetyMenu || onlineSafetyMenuProceedCount < 1) && !didProceedThroughOnlineSafetyMenuThisSession;
		}

		public static void apply()
		{
			if (!Level.isLoaded)
			{
				if (MainCamera.instance != null)
				{
					MainCamera.instance.fieldOfView = DesiredVerticalFieldOfView;
				}
			}

			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex <= Level.BUILD_INDEX_MENU)
			{
				MenuConfigurationOptions.apply();
			}

			UnturnedMasterVolume.preferredVolume = volume;
		}

		public static void RestoreAudioDefaults()
		{
			volume = DEFAULT_MASTER_VOLUME;
			UnfocusedVolume = DEFAULT_UNFOCUSED_VOLUME;
			gameVolume = DEFAULT_GAME_VOLUME;
			MusicMasterVolume = DEFAULT_MUSIC_MASTER_VOLUME;
			loadingScreenMusicVolume = DEFAULT_LOADING_SCREEN_MUSIC_VOLUME;
			deathMusicVolume = DEFAULT_DEATH_MUSIC_VOLUME;
			MainMenuMusicVolume = DEFAULT_MAIN_MENU_MUSIC_VOLUME;
			ambientMusicVolume = DEFAULT_AMBIENT_MUSIC_VOLUME;
			voiceVolume = DEFAULT_VOICE_VOLUME;
			AtmosphereVolume = DEFAULT_ATMOSPHERE_VOLUME;
			ZombieFootstepsVolume = DEFAULT_ZOMBIE_FOOTSTEPS_VOLUME;
		}

		public static void restoreDefaults()
		{
			splashscreen = true;
			timer = false;
			//physics = true;

			fov = 0.75f;
			debug = false;

			EnableGore = true;
			filter = true;

			chatText = true;
			chatVoiceIn = false;
			EnableOutboundVoiceChat = false;
			ShowOutboundVoiceChatOffHint = true;
			ShouldClickBlueprintToCraft = false;

			metric = true;
			talk = false;
			hints = true;
			proUI = true;
			ShouldHitmarkersFollowWorldPosition = false;
			ShouldAnonymizeMultiplayerDetails = false;
			ShouldHideRichPresence = false;
			featuredWorkshop = true;
			showHotbar = true;
			pauseWhenUnfocused = true;

			screenshotSizeMultiplier = 1;
			enableScreenshotSupersampling = true;
			enableScreenshotsOnLoadingScreen = true;

			useStaticCrosshair = false;
			staticCrosshairSize = 0.1f;
			crosshairShape = ECrosshairShape.Line;
			hitmarkerStyle = EHitmarkerStyle.Animated;
			vehicleThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationDetached;
			vehicleAircraftThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationAttached;
			flashbangBrightness = 1.0f;
			cameraShakeIntensity = 1.0f;

			damageFlinchMode = EDamageFlinchMode.RollOnly;
			damageFlinchIntensity = 1.0f;
			sprintFovBoostIntensity = 1.0f;
			viewmodelBobScale = 1.0f;

			crosshairColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			hitmarkerColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			criticalHitmarkerColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
			cursorColor = Color.white;
			backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			foregroundColor = new Color(0.9f, 0.9f, 0.9f);
			fontColor = new Color(0.9f, 0.9f, 0.9f);
			shadowColor = Color.black;
			badColor = Palette.COLOR_R;

			_voiceAlwaysRecording = false;
			shouldNametagFadeOut = true;

			wantsToHideOnlineSafetyMenu = false;
		}

		public static void load()
		{
			restoreDefaults();
			RestoreAudioDefaults();

			if (ReadWrite.fileExists("/Options.dat", true))
			{
				Block block = ReadWrite.readBlock("/Options.dat", true, 0);

				if (block != null)
				{
					byte version = block.readByte();

					if (version > 2)
					{
						// Nelson 2024-07-16: Keeping this here for now so that players switching between preview and
						// main branch don't lose their settings.
						bool oldMusicToggle = block.readBoolean();

						if (version < 31)
						{
							splashscreen = true;
						}
						else
						{
							splashscreen = block.readBoolean();
						}

						if (version < 20)
						{
							timer = false;
						}
						else
						{
							timer = block.readBoolean();
						}

						//physics = block.readBoolean();
						if (version < 10)
						{
							block.readBoolean();
						}

						if (version > 7)
						{
							fov = block.readSingle();
						}
						else
						{
							fov = block.readSingle() * 0.5f;
						}

						if (version < 24)
						{
							fov *= 1.5f;
							fov = Mathf.Clamp01(fov);
						}

						if (version > 4)
						{
							volume = block.readSingle();
						}
						else
						{
							volume = DEFAULT_MASTER_VOLUME;
						}

						if (version > 22)
						{
							voiceVolume = block.readSingle();
							if (version < 36)
							{
								// Version 36 added voice normalization. Older versions had [0, 4] gain so clamp.
								voiceVolume = Mathf.Min(voiceVolume, 1.0f);
							}
						}
						else
						{
							voiceVolume = DEFAULT_VOICE_VOLUME;
						}

						if (version >= SAVEDATA_VERSION_ADDED_LOADING_SCREEN_MUSIC)
						{
							loadingScreenMusicVolume = block.readSingle();
						}
						else
						{
							loadingScreenMusicVolume = DEFAULT_LOADING_SCREEN_MUSIC_VOLUME;
						}

						debug = block.readBoolean();

						EnableGore = block.readBoolean();
						filter = block.readBoolean();
						if (version < SAVEDATA_VERSION_ADDED_ONLINE_SAFETY_MENU)
						{
							// Reset to false, opt-in from multiplayer menu.
							filter = true;
						}

						chatText = block.readBoolean();

						if (version > 8)
						{
							chatVoiceIn = block.readBoolean();
						}
						else
						{
							chatVoiceIn = false;
						}
						if (version < SAVEDATA_VERSION_ADDED_ONLINE_SAFETY_MENU)
						{
							// Reset to false, opt-in from multiplayer menu.
							chatVoiceIn = false;
						}

						EnableOutboundVoiceChat = block.readBoolean();
						if (version < SAVEDATA_VERSION_ADDED_ONLINE_SAFETY_MENU)
						{
							// Reset to false, opt-in from multiplayer menu.
							EnableOutboundVoiceChat = false;
						}

						metric = block.readBoolean();

						if (version > 24)
						{
							talk = block.readBoolean();
						}
						else
						{
							talk = false;
						}

						if (version > 3)
						{
							hints = block.readBoolean();
						}
						else
						{
							hints = true;
						}

						// Nelson 2024-07-16: Keeping this here for now so that players switching between preview and
						// main branch don't lose their settings.
						bool oldAmbienceToggle;
						if (version > 13)
						{
							oldAmbienceToggle = block.readBoolean();
						}
						else
						{
							oldAmbienceToggle = true;
						}

						if (version > 12)
						{
							proUI = block.readBoolean();
						}
						else
						{
							proUI = true;
						}

						if (version > 20)
						{
							ShouldHitmarkersFollowWorldPosition = block.readBoolean();
						}
						else
						{
							ShouldHitmarkersFollowWorldPosition = false;
						}

						if (version >= SAVEDATA_VERSION_SEPARATED_STREAMER_MODE)
						{
							ShouldAnonymizeMultiplayerDetails = block.readBoolean();
						}
						else if (version > 21)
						{
							bool oldStreamerMode = block.readBoolean();
							ShouldAnonymizeMultiplayerDetails = oldStreamerMode;
							ShouldHideRichPresence = oldStreamerMode;
						}
						else
						{
							ShouldAnonymizeMultiplayerDetails = false;
							ShouldHideRichPresence = false;
						}

						if (version > 25)
						{
							featuredWorkshop = block.readBoolean();
						}
						else
						{
							featuredWorkshop = true;
						}

						if (version > 28 && version < SAVEDATA_VERSION_REMOVED_MATCHMAKING)
						{
							// matchmakingShowAllMaps
							block.readBoolean();
						}

						if (version > 29)
						{
							showHotbar = block.readBoolean();
						}
						else
						{
							showHotbar = true;
						}

						if (version > 32)
						{
							pauseWhenUnfocused = block.readBoolean();
						}
						else
						{
							pauseWhenUnfocused = true;
						}

						if (version > 27 && version < SAVEDATA_VERSION_REMOVED_MATCHMAKING)
						{
							// minMatchmakingPlayers
							block.readInt32();
						}

						if (version > 26 && version < SAVEDATA_VERSION_REMOVED_MATCHMAKING)
						{
							// maxMatchmakingPing
							block.readInt32();
						}

						if (version > 6)
						{
							crosshairColor = block.readColor();
							hitmarkerColor = block.readColor();
							criticalHitmarkerColor = block.readColor();
							cursorColor = block.readColor();
						}
						else
						{
							crosshairColor = Color.white;
							hitmarkerColor = Color.white;
							criticalHitmarkerColor = Color.red;
							cursorColor = Color.white;
						}

						if (version > 18)
						{
							backgroundColor = block.readColor();
							foregroundColor = block.readColor();
							fontColor = block.readColor();
						}
						else
						{
							backgroundColor = new Color(0.9f, 0.9f, 0.9f);
							foregroundColor = new Color(0.9f, 0.9f, 0.9f);
							fontColor = new Color(0.9f, 0.9f, 0.9f);
						}

						if (version > 33)
						{
							shadowColor = block.readColor();
						}
						else
						{
							shadowColor = Color.black;
						}

						if (version > 34)
						{
							badColor = block.readColor();
						}
						else
						{
							badColor = Palette.COLOR_R;
						}

						if (version < SAVEDATA_VERSION_ADDED_SCREENSHOT_SIZE)
						{
							screenshotSizeMultiplier = 1;
						}
						else
						{
							screenshotSizeMultiplier = block.readInt32();
						}

						if (version < SAVEDATA_VERSION_ADDED_SCREENSHOT_SUPERSAMPLING)
						{
							enableScreenshotSupersampling = true;
						}
						else
						{
							enableScreenshotSupersampling = block.readBoolean();
						}

						if (version < SAVEDATA_VERSION_ADDED_LOADING_SCREEN_SCREENSHOTS)
						{
							enableScreenshotsOnLoadingScreen = true;
						}
						else
						{
							enableScreenshotsOnLoadingScreen = block.readBoolean();
						}

						if (version < SAVEDATA_VERSION_ADDED_STATIC_CROSSHAIR)
						{
							useStaticCrosshair = false;
							staticCrosshairSize = 0.25f;
						}
						else
						{
							useStaticCrosshair = block.readBoolean();
							staticCrosshairSize = block.readSingle();
						}

						if (version < SAVEDATA_VERSION_ADDED_CROSSHAIR_SHAPE)
						{
							crosshairShape = ECrosshairShape.Line;
						}
						else
						{
							crosshairShape = (ECrosshairShape) block.readByte();
						}

						if (version < SAVEDATA_VERSION_ADDED_CROSSHAIR_AND_HITMARKER_ALPHA)
						{
							crosshairColor.a = 0.5f;
							hitmarkerColor.a = 0.5f;
							criticalHitmarkerColor.a = 0.5f;
						}
						else
						{
							crosshairColor.a = block.readByte() / 255.0f;
							hitmarkerColor.a = block.readByte() / 255.0f;
							criticalHitmarkerColor.a = block.readByte() / 255.0f;
						}

						if (version < SAVEDATA_VERSION_ADDED_HITMARKER_STYLE_FIX)
						{
							hitmarkerStyle = EHitmarkerStyle.Animated;
						}
						else
						{
							hitmarkerStyle = (EHitmarkerStyle) block.readByte();
						}

						if (version >= SAVEDATA_VERSION_ADDED_VOICE_ALWAYS_RECORDING)
						{
							_voiceAlwaysRecording = block.readBoolean();
							if (version < SAVEDATA_VERSION_RESET_VOICE_ALWAYS_RECORDING)
							{
								_voiceAlwaysRecording = false;
							}
						}
						else
						{
							_voiceAlwaysRecording = false;
						}

						if (version >= SAVEDATA_VERSION_ADDED_NAMETAG_FADEOUT_OPT)
						{
							shouldNametagFadeOut = block.readBoolean();
						}
						else
						{
							shouldNametagFadeOut = true;
						}

						if (version >= SAVEDATA_VERSION_ADDED_GAME_VOLUME)
						{
							gameVolume = block.readSingle();
						}
						else
						{
							gameVolume = DEFAULT_GAME_VOLUME;
						}

						if (version >= SAVEDATA_VERSION_ADDED_UNFOCUSED_VOLUME)
						{
							UnfocusedVolume = block.readSingle();
						}
						else
						{
							UnfocusedVolume = DEFAULT_UNFOCUSED_VOLUME;
						}

						if (version >= SAVEDATA_VERSION_ADDED_VEHICLE_THIRD_PERSON_CAMERA_MODE)
						{
							vehicleThirdPersonCameraMode = (EVehicleThirdPersonCameraMode) block.readByte();
						}
						else
						{
							vehicleThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationDetached;
						}

						if (version >= SAVEDATA_VERSION_REPLACTED_MUSIC_TOGGLE_WITH_VOLUMES)
						{
							deathMusicVolume = block.readSingle();
							MainMenuMusicVolume = block.readSingle();
							ambientMusicVolume = block.readSingle();
						}
						else
						{
							if (oldMusicToggle)
							{
								deathMusicVolume = DEFAULT_DEATH_MUSIC_VOLUME;
								MainMenuMusicVolume = DEFAULT_MAIN_MENU_MUSIC_VOLUME;
								ambientMusicVolume = DEFAULT_AMBIENT_MUSIC_VOLUME;
							}
							else
							{
								deathMusicVolume = 0.0f;
								MainMenuMusicVolume = 0.0f;
								ambientMusicVolume = 0.0f;
							}
						}

						if (version >= SAVEDATA_VERSION_ADDED_MUSIC_MASTER_VOLUME)
						{
							MusicMasterVolume = block.readSingle();
						}
						else
						{
							MusicMasterVolume = DEFAULT_MUSIC_MASTER_VOLUME;
						}

						if (version >= SAVEDATA_VERSION_ADDED_ATMOSPHERE_VOLUME)
						{
							AtmosphereVolume = block.readSingle();
						}
						else
						{
							AtmosphereVolume = oldAmbienceToggle ? DEFAULT_ATMOSPHERE_VOLUME : 0.0f;
						}

						if (version >= SAVEDATA_VERSION_SEPARATED_AIRCRAFT_THIRD_PERSON_CAMERA_MODE)
						{
							vehicleAircraftThirdPersonCameraMode = (EVehicleThirdPersonCameraMode) block.readByte();
						}
						else
						{
							vehicleAircraftThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationAttached;
						}

						if (version >= SAVEDATA_VERSION_ADDED_ONLINE_SAFETY_MENU)
						{
							onlineSafetyMenuProceedCount = block.readInt32();
							wantsToHideOnlineSafetyMenu = block.readBoolean();
						}

						if (version >= SAVEDATA_VERSION_ADDED_FLASHBANG_BRIGHTNESS)
						{
							flashbangBrightness = block.readSingle();
						}
						else
						{
							flashbangBrightness = 1.0f;
						}

						if (version >= SAVEDATA_VERSION_ADDED_CAMERA_SHAKE_INTENSITY)
						{
							cameraShakeIntensity = block.readSingle();
						}
						else
						{
							cameraShakeIntensity = 1.0f;
						}

						if (version >= SAVEDATA_VERSION_ADDED_DAMAGE_FLINCH_OPTIONS)
						{
							damageFlinchMode = (EDamageFlinchMode) block.readByte();
							damageFlinchIntensity = block.readSingle();
						}
						else
						{
							damageFlinchMode = EDamageFlinchMode.RollOnly;
							damageFlinchIntensity = 1.0f;
						}

						if (version >= SAVEDATA_VERSION_ADDED_SHOW_OUTBOUND_VOICE_CHAT_OFF_HINT)
						{
							ShowOutboundVoiceChatOffHint = block.readBoolean();
						}
						else
						{
							ShowOutboundVoiceChatOffHint = true;
						}

						if (version >= SAVEDATA_VERSION_ADDED_SPRINT_FOV_OPTION)
						{
							sprintFovBoostIntensity = block.readSingle();
						}
						else
						{
							sprintFovBoostIntensity = 1.0f;
						}

						if (version >= SAVEDATA_VERSION_SEPARATED_STREAMER_MODE)
						{
							ShouldHideRichPresence = block.readBoolean();
						}

						if (version >= SAVEDATA_VERSION_ADDED_CLICK_BLUEPRINT_TO_CRAFT)
						{
							ShouldClickBlueprintToCraft = block.readBoolean();
						}
						else
						{
							ShouldClickBlueprintToCraft = false;
						}

						if (version >= SAVEDATA_VERSION_ADDED_VIEWMODEL_BOB_SCALE)
						{
							viewmodelBobScale = block.readSingle();
						}
						else
						{
							viewmodelBobScale = 1.0f;
						}

						if (version >= SAVEDATA_VERSION_ADDED_ZOMBIE_FOOTSTEPS_VOLUME)
						{
							ZombieFootstepsVolume = block.readSingle();
						}
						else
						{
							ZombieFootstepsVolume = DEFAULT_ZOMBIE_FOOTSTEPS_VOLUME;
						}

						// We can safely check Provider.isPro here, see Setup.cs.
						if (!Provider.isPro)
						{
							// In-case their settings were corrupted and they don't have Gold, or maybe they
							// refunded their Gold membership we restore UI colors since they can't adjusted them.
							backgroundColor = new Color(0.9f, 0.9f, 0.9f);
							foregroundColor = new Color(0.9f, 0.9f, 0.9f);
							fontColor = new Color(0.9f, 0.9f, 0.9f);
							shadowColor = Color.black;
							badColor = Palette.COLOR_R;
						}

#if CLOUDDEBUG
						UnturnedLog.info("Options: " + fps + " " + music + " " + physics + " " + fov);
#endif

						return;
					}
				}
			}
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION_NEWEST);

			block.writeBoolean(false); // Old music toggle.
			block.writeBoolean(splashscreen);
			block.writeBoolean(timer);
			//block.writeBoolean(physics);

			block.writeSingle(fov);
			block.writeSingle(volume);
			block.writeSingle(voiceVolume);
			block.writeSingle(loadingScreenMusicVolume);
			block.writeBoolean(debug);

			block.writeBoolean(EnableGore);
			block.writeBoolean(filter);

			block.writeBoolean(chatText);
			block.writeBoolean(chatVoiceIn);
			block.writeBoolean(EnableOutboundVoiceChat);

			block.writeBoolean(metric);
			block.writeBoolean(talk);
			block.writeBoolean(hints);
			block.writeBoolean(false); // Old ambience toggle.
			block.writeBoolean(proUI);
			block.writeBoolean(ShouldHitmarkersFollowWorldPosition);
			block.writeBoolean(ShouldAnonymizeMultiplayerDetails);
			block.writeBoolean(featuredWorkshop);
			block.writeBoolean(showHotbar);
			block.writeBoolean(pauseWhenUnfocused);

			block.writeColor(crosshairColor);
			block.writeColor(hitmarkerColor);
			block.writeColor(criticalHitmarkerColor);
			block.writeColor(cursorColor);
			block.writeColor(backgroundColor);
			block.writeColor(foregroundColor);
			block.writeColor(fontColor);
			block.writeColor(shadowColor);
			block.writeColor(badColor);
			block.writeInt32(screenshotSizeMultiplier);
			block.writeBoolean(enableScreenshotSupersampling);
			block.writeBoolean(enableScreenshotsOnLoadingScreen);
			block.writeBoolean(useStaticCrosshair);
			block.writeSingle(staticCrosshairSize);
			block.writeByte((byte) crosshairShape);

			block.writeByte(MathfEx.RoundAndClampToByte(crosshairColor.a * byte.MaxValue));
			block.writeByte(MathfEx.RoundAndClampToByte(hitmarkerColor.a * byte.MaxValue));
			block.writeByte(MathfEx.RoundAndClampToByte(criticalHitmarkerColor.a * byte.MaxValue));

			block.writeByte((byte) hitmarkerStyle);
			block.writeBoolean(_voiceAlwaysRecording);
			block.writeBoolean(shouldNametagFadeOut);
			block.writeSingle(gameVolume);
			block.writeSingle(UnfocusedVolume);
			block.writeByte((byte) vehicleThirdPersonCameraMode);
			block.writeSingle(deathMusicVolume);
			block.writeSingle(MainMenuMusicVolume);
			block.writeSingle(ambientMusicVolume);
			block.writeSingle(MusicMasterVolume);
			block.writeSingle(AtmosphereVolume);
			block.writeByte((byte) vehicleAircraftThirdPersonCameraMode);

			block.writeInt32(onlineSafetyMenuProceedCount);
			block.writeBoolean(wantsToHideOnlineSafetyMenu);
			block.writeSingle(flashbangBrightness);
			block.writeSingle(cameraShakeIntensity);

			block.writeByte((byte) damageFlinchMode);
			block.writeSingle(damageFlinchIntensity);
			block.writeBoolean(ShowOutboundVoiceChatOffHint);
			block.writeSingle(sprintFovBoostIntensity);
			block.writeBoolean(ShouldHideRichPresence);
			block.writeBoolean(ShouldClickBlueprintToCraft);
			block.writeSingle(viewmodelBobScale);
			block.writeSingle(ZombieFootstepsVolume);

			ReadWrite.writeBlock("/Options.dat", true, block);
		}
	}
}
