////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void GraphicsSettingsApplied();

	public partial class GraphicsSettings
	{
		public const float EFFECT_ULTRA = 64;
		public const float EFFECT_HIGH = 48;
		public const float EFFECT_MEDIUM = 32;
		public const float EFFECT_LOW = 16;

		public static float effect
		{
			get
			{
				if (effectQuality == EGraphicQuality.ULTRA)
				{
					return Random.Range(EFFECT_ULTRA - 16, EFFECT_ULTRA + 16);
				}
				else if (effectQuality == EGraphicQuality.HIGH)
				{
					return Random.Range(EFFECT_HIGH - 8, EFFECT_HIGH + 8);
				}
				else if (effectQuality == EGraphicQuality.MEDIUM)
				{
					return Random.Range(EFFECT_MEDIUM - 4, EFFECT_MEDIUM + 4);
				}
				else if (effectQuality == EGraphicQuality.LOW)
				{
					return Random.Range(EFFECT_LOW - 2, EFFECT_LOW + 2);
				}

				return 0f;
			}
		}

		/// <summary>
		/// Planar reflection component updates its culling distance and culling mask when this is incremented.
		/// </summary>
		public static int planarReflectionUpdateIndex;

		private static GraphicsSettingsData graphicsSettingsData = new GraphicsSettingsData();

		public static GraphicsSettingsResolution resolution
		{
			get => graphicsSettingsData.Resolution;

			set
			{
				graphicsSettingsData.Resolution = value;

				changeResolution = true;
			}
		}

		public static FullScreenMode fullscreenMode
		{
			get => graphicsSettingsData.FullscreenMode;

			set
			{
				graphicsSettingsData.FullscreenMode = value;

				changeResolution = true;
			}
		}

		public static bool buffer
		{
			get => graphicsSettingsData.IsVSyncEnabled;

			set => graphicsSettingsData.IsVSyncEnabled = value;
		}

		/// <summary>
		/// Overrides in-game UI scale setting.
		/// </summary>
		private static CommandLineFloat uiScale = new CommandLineFloat("-ui_scale");

		private static bool didCacheUIScaleOverride = false;
		private static float? cachedUIScaleOverride = null;

		public static float userInterfaceScale
		{
			get
			{
				if (didCacheUIScaleOverride == false)
				{
					didCacheUIScaleOverride = true;

					if (uiScale.hasValue && uiScale.value.IsFinite())
					{
						cachedUIScaleOverride = uiScale.value;
					}
					else if (Provider.preferenceData != null && Provider.preferenceData.Graphics != null)
					{
						float prefValue = Provider.preferenceData.Graphics.Override_UI_Scale;
						if (prefValue.IsFinite() && prefValue > 0.0f)
						{
							cachedUIScaleOverride = prefValue;
						}
					}
				}

				if (cachedUIScaleOverride.HasValue)
				{
					return cachedUIScaleOverride.Value;
				}
				else
				{
					return graphicsSettingsData.UserInterfaceScale;
				}
			}
			set => graphicsSettingsData.UserInterfaceScale = value;
		}

		private static CommandLineInt clTargetFrameRate = new CommandLineInt("-FrameRateLimit");

		public static bool UseTargetFrameRate
		{
			get => graphicsSettingsData.UseTargetFrameRate;
			set => graphicsSettingsData.UseTargetFrameRate = value;
		}

		public static int TargetFrameRate
		{
			get => graphicsSettingsData.TargetFrameRate;
			set => graphicsSettingsData.TargetFrameRate = value;
		}

		public static bool UseUnfocusedTargetFrameRate
		{
			get => graphicsSettingsData.UseUnfocusedTargetFrameRate;
			set => graphicsSettingsData.UseUnfocusedTargetFrameRate = value;
		}

		public static int UnfocusedTargetFrameRate
		{
			get => graphicsSettingsData.UnfocusedTargetFrameRate;
			set => graphicsSettingsData.UnfocusedTargetFrameRate = value;
		}

		public static EAntiAliasingType antiAliasingType
		{
			get => graphicsSettingsData.AntiAliasingType5;

			set => graphicsSettingsData.AntiAliasingType5 = value;
		}

		public static EAnisotropicFilteringMode anisotropicFilteringMode
		{
			get => graphicsSettingsData.AnisotropicFilteringMode;

			set => graphicsSettingsData.AnisotropicFilteringMode = value;
		}

		public static bool isAmbientOcclusionEnabled
		{
			get => graphicsSettingsData.IsAmbientOcclusionEnabled;

			set => graphicsSettingsData.IsAmbientOcclusionEnabled = value;
		}

		public static bool bloom
		{
			get => graphicsSettingsData.IsBloomEnabled;

			set => graphicsSettingsData.IsBloomEnabled = value;
		}

		public static bool chromaticAberration
		{
			get => graphicsSettingsData.IsChromaticAberrationEnabled;

			set => graphicsSettingsData.IsChromaticAberrationEnabled = value;
		}

		public static bool filmGrain
		{
			get => graphicsSettingsData.IsFilmGrainEnabled;

			set => graphicsSettingsData.IsFilmGrainEnabled = value;
		}

		public static bool blend
		{
			get => graphicsSettingsData.IsNiceBlendEnabled;

			set => graphicsSettingsData.IsNiceBlendEnabled = value;
		}

		/// <summary>
		/// Distance to use terrain shaders before fallback to a baked texture.
		/// </summary>
		public static float terrainBasemapDistance => WantsCinematicMode ? 8192 : (blend ? 512 : 256);

		/// <summary>
		/// Higher error reduces vertex density as distance increases.
		/// </summary>
		public static float terrainHeightmapPixelError
		{
			get
			{
				if (WantsCinematicMode)
				{
					return 1;
				}

				switch (terrainQuality)
				{
					case EGraphicQuality.LOW:
						return 64;

					case EGraphicQuality.MEDIUM:
						return 32;

					case EGraphicQuality.HIGH:
						return 16;

					case EGraphicQuality.ULTRA:
						return 8;

					default:
						UnturnedLog.warn("Unknown terrain quality {0} in terrainHeightmapPixelError", terrainQuality);
						return 1;
				}
			}
		}

		public static bool grassDisplacement
		{
			get => graphicsSettingsData.IsGrassDisplacementEnabled;

			set => graphicsSettingsData.IsGrassDisplacementEnabled = value;
		}

		public static bool foliageFocus
		{
			get => graphicsSettingsData.IsFoliageFocusEnabled;

			set => graphicsSettingsData.IsFoliageFocusEnabled = value;
		}

		public static EGraphicQuality landmarkQuality
		{
			get => graphicsSettingsData.LandmarkQuality;

			set => graphicsSettingsData.LandmarkQuality = value;
		}

		public static bool ragdolls
		{
			get => graphicsSettingsData.IsRagdollsEnabled;

			set => graphicsSettingsData.IsRagdollsEnabled = value;
		}

		public static bool debris
		{
			get => graphicsSettingsData.IsDebrisEnabled;

			set => graphicsSettingsData.IsDebrisEnabled = value;
		}

		public static bool blast
		{
			get => graphicsSettingsData.IsBlastEnabled;

			set => graphicsSettingsData.IsBlastEnabled = value;
		}

		public static bool puddle
		{
			get => graphicsSettingsData.IsPuddleEnabled;

			set => graphicsSettingsData.IsPuddleEnabled = value;
		}

		public static bool glitter
		{
			get => graphicsSettingsData.IsGlitterEnabled;

			set => graphicsSettingsData.IsGlitterEnabled = value;
		}

		public static bool triplanar
		{
			get => graphicsSettingsData.IsTriplanarMappingEnabled;

			set => graphicsSettingsData.IsTriplanarMappingEnabled = value;
		}

		public static bool skyboxReflection
		{
			get => graphicsSettingsData.IsSkyboxReflectionEnabled;

			set => graphicsSettingsData.IsSkyboxReflectionEnabled = value;
		}

		public static bool IsItemIconAntiAliasingEnabled
		{
			get => graphicsSettingsData.IsItemIconAntiAliasingEnabled;
			set => graphicsSettingsData.IsItemIconAntiAliasingEnabled = value;
		}

		public static bool IsClutterEnabled
		{
			get => graphicsSettingsData.IsClutterEnabled;
			set => graphicsSettingsData.IsClutterEnabled = value;
		}

		public static bool WantsDarkScopePeripheral
		{
			get => graphicsSettingsData.IsDarkScopePeripheralEnabled;
			set => graphicsSettingsData.IsDarkScopePeripheralEnabled = value;
		}

		public static bool IsWindEnabled
		{
			get => graphicsSettingsData.IsWindEnabled;
			set => graphicsSettingsData.IsWindEnabled = value;
		}

		/// <summary>
		/// Added for players who want to see if they can get better performance with a ridiculously low max draw distance.
		/// </summary>
		private static CommandLineFloat clFarClipDistance = new CommandLineFloat("-FarClipDistance");

		/// <summary>
		/// Multiplier for far clip plane distance.
		/// Clamped within [0, 1] range to prevent editing config files for an advantage.
		/// </summary>
		public static float NormalizedFarClipDistance
		{
			get => Mathf.Clamp01(Level.isEditor ? graphicsSettingsData.EditorFarClipDistance : graphicsSettingsData.FarClipDistance);

			set
			{
				if (Level.isEditor)
				{
					graphicsSettingsData.EditorFarClipDistance = Mathf.Clamp01(value);
				}
				else
				{
					graphicsSettingsData.FarClipDistance = Mathf.Clamp01(value);
				}
			}
		}

		/// <summary>
		/// Multiplier for draw distance.
		/// Clamped within [0, 1] range to prevent editing config files for an advantage.
		/// </summary>
		public static float normalizedDrawDistance
		{
			get => Mathf.Clamp01(graphicsSettingsData.DrawDistance);

			set => graphicsSettingsData.DrawDistance = Mathf.Clamp01(value);
		}

		/// <summary>
		/// Multiplier for draw distance of optional super-low LOD models.
		/// Clamped within [0, 1] range to prevent editing config files for an advantage.
		/// </summary>
		public static float normalizedLandmarkDrawDistance
		{
			get => Mathf.Clamp01(graphicsSettingsData.LandmarkDistance);

			set => graphicsSettingsData.LandmarkDistance = Mathf.Clamp01(value);
		}

		public static EGraphicQuality effectQuality
		{
			get => graphicsSettingsData.EffectQuality;

			set => graphicsSettingsData.EffectQuality = value;
		}

		public static EGraphicQuality foliageQuality
		{
			get => graphicsSettingsData.FoliageQuality2;
			set => graphicsSettingsData.FoliageQuality2 = value;
		}

		public static EGraphicQuality sunShaftsQuality
		{
			get => graphicsSettingsData.SunShaftsQuality;

			set => graphicsSettingsData.SunShaftsQuality = value;
		}

		public static EGraphicQuality lightingQuality
		{
			get => graphicsSettingsData.LightingQuality;
			set => graphicsSettingsData.LightingQuality = value;
		}

		public static EGraphicQuality reflectionQuality
		{
			get => graphicsSettingsData.ScreenSpaceReflectionQuality;

			set => graphicsSettingsData.ScreenSpaceReflectionQuality = value;
		}

		public static EGraphicQuality planarReflectionQuality
		{
			get => graphicsSettingsData.PlanarReflectionQuality;

			set => graphicsSettingsData.PlanarReflectionQuality = value;
		}

		public static EGraphicQuality waterQuality
		{
			get => graphicsSettingsData.WaterQuality;
			set => graphicsSettingsData.WaterQuality = value;
		}

		public static EGraphicQuality scopeQuality
		{
			get => graphicsSettingsData.ScopeQuality2;

			set => graphicsSettingsData.ScopeQuality2 = value;
		}

		public static EGraphicQuality outlineQuality
		{
			get => graphicsSettingsData.OutlineQuality;

			set => graphicsSettingsData.OutlineQuality = value;
		}

		public static EGraphicQuality terrainQuality
		{
			get => graphicsSettingsData.TerrainQuality;

			set => graphicsSettingsData.TerrainQuality = value;
		}

		public static ERenderMode renderMode
		{
			get
			{
				// Nelson 2024-03-18: Prevent players from setting invalid config, otherwise systems like terrain tile
				// don't assign a material.
				switch (graphicsSettingsData.RenderMode2)
				{
					case ERenderMode.FORWARD:
					case ERenderMode.DEFERRED:
						return graphicsSettingsData.RenderMode2;

					default:
						return ERenderMode.FORWARD;
				}
			}

			set => graphicsSettingsData.RenderMode2 = value;
		}

		public static event GraphicsSettingsApplied graphicsSettingsApplied;

		private static bool changeResolution;

		// Steam support has a command-line page suggesting -w and -h for Source Engine games,
		// and a lot of Unturned players seem to find that as I've seen it posted on the forums.
		private static CommandLineInt valveWidth = new CommandLineInt("-w");
		private static CommandLineInt valveHeight = new CommandLineInt("-h");

		// Generic width and height resolution parameters.
		private static CommandLineInt clWidth = new CommandLineInt("-width");
		private static CommandLineInt clHeight = new CommandLineInt("-height");
		private static CommandLineInt clFullscreenMode = new CommandLineInt("-fullscreenmode");
		private static CommandLineInt clRefreshRate = new CommandLineInt("-refreshrate");

		public static void applyResolution()
		{
			if (Application.isEditor)
				return;

			bool unitySpecifiedResolution = false;
			string commandLine = CommandLine.Get();
			unitySpecifiedResolution |= commandLine.IndexOf("-screen-width", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
			unitySpecifiedResolution |= commandLine.IndexOf("-screen-height", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
			unitySpecifiedResolution |= commandLine.IndexOf("-screen-fullscreen", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
			unitySpecifiedResolution |= commandLine.IndexOf("-window-mode", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
			if (unitySpecifiedResolution)
			{
				// If player specified one of Unity's built-in resolution command-line flags then we should not override it.
				UnturnedLog.info("Ignoring game resolution settings because Unity built-in command-line options were set");
				return;
			}

			/*
				This check is problematic because Unity's "supported" resolutions are not always right,
				so I've disabled it for now to maybe bring back in the future. With it enabled players
				were having issues with unusual screen resolutions, including 21:9.

			if(Provider.preferenceData.Graphics.Restrict_Resolution_To_Monitor_Supported)
			{
				Resolution[] recommendedResolutions = Screen.resolutions;
				if(recommendedResolutions.Length > 0)
				{
					if(resolution.Width < 640 || resolution.Height < 480)
					{
						// Our resolution was probably zero, so fallback to highest resolution.
						resolution = new GraphicsSettingsResolution(recommendedResolutions[recommendedResolutions.Length - 1]);
					}
					else if(resolution.Width < recommendedResolutions[0].width || resolution.Height < recommendedResolutions[0].height)
					{
						// Fallback to lowest resolution.
						resolution = new GraphicsSettingsResolution(recommendedResolutions[0]);
					}
					else if(resolution.Width > recommendedResolutions[recommendedResolutions.Length - 1].width || resolution.Height > recommendedResolutions[recommendedResolutions.Length - 1].height)
					{
						// Fallback to highest resolution.
						resolution = new GraphicsSettingsResolution(recommendedResolutions[recommendedResolutions.Length - 1]);
					}
				}
			}
			*/

			int width = resolution.Width;
			int height = resolution.Height;

			// If zero or unsupported Unity will switch to highest refresh rate supported by the monitor.
			int preferredRefreshRate = 0;

			if (clWidth.hasValue)
			{
				width = clWidth.value;
			}
			else if (valveWidth.hasValue)
			{
				width = valveWidth.value;
			}
			else if (Provider.preferenceData.Graphics.Override_Resolution_Width > 0)
			{
				width = Provider.preferenceData.Graphics.Override_Resolution_Width;
			}

			if (clHeight.hasValue)
			{
				height = clHeight.value;
			}
			else if (valveHeight.hasValue)
			{
				height = valveHeight.value;
			}
			else if (Provider.preferenceData.Graphics.Override_Resolution_Height > 0)
			{
				height = Provider.preferenceData.Graphics.Override_Resolution_Height;
			}

			if (clRefreshRate.hasValue && clRefreshRate.value > 0)
			{
				preferredRefreshRate = clRefreshRate.value;
			}
			else if (Provider.preferenceData.Graphics.Override_Refresh_Rate > 0)
			{
				preferredRefreshRate = Provider.preferenceData.Graphics.Override_Refresh_Rate;
			}

			if (clWidth.hasValue != clHeight.hasValue)
			{
				UnturnedLog.warn("Mismatch of {0} and {1}", clWidth.key, clHeight.key);
			}
			if (valveWidth.hasValue != valveHeight.hasValue)
			{
				UnturnedLog.warn("Mismatch of {0} and {1}", valveWidth.key, valveHeight.key);
			}

			FullScreenMode preferredFullscreenMode = fullscreenMode;
			if (clFullscreenMode.hasValue)
			{
				if (System.Enum.IsDefined(typeof(FullScreenMode), clFullscreenMode.value))
				{
					preferredFullscreenMode = (FullScreenMode) clFullscreenMode.value;
				}
				else
				{
					UnturnedLog.warn($"Invalid fullscreen mode on command-line: {clFullscreenMode.value}");
				}
			}
			else if (Provider.preferenceData.Graphics.Override_Fullscreen_Mode >= 0)
			{
				if (System.Enum.IsDefined(typeof(FullScreenMode), Provider.preferenceData.Graphics.Override_Fullscreen_Mode))
				{
					preferredFullscreenMode = (FullScreenMode) Provider.preferenceData.Graphics.Override_Fullscreen_Mode;
				}
				else
				{
					UnturnedLog.warn($"Invalid fullscreen mode in config: {Provider.preferenceData.Graphics.Override_Fullscreen_Mode}");
				}
			}

			// Setting preferred refresh rate is only supported in exclusive fullscreen, so for that reason and also to avoid
			// any potential per-platform (e.g. Mac) bugs with getting supported refresh rate we only call that method when necessary.
			if (fullscreenMode == FullScreenMode.ExclusiveFullScreen && preferredRefreshRate > 0)
			{
				UnturnedLog.info($"Requesting resolution change: {preferredFullscreenMode} {width} x {height} @ {preferredRefreshRate} hz");
				RefreshRate refreshRateRatio = new RefreshRate()
				{
					numerator = (uint) preferredRefreshRate,
					denominator = 1,
				};
				Screen.SetResolution(width, height, preferredFullscreenMode, refreshRateRatio);
			}
			else
			{
				UnturnedLog.info($"Requesting resolution change: {preferredFullscreenMode} {width} x {height}");
				Screen.SetResolution(width, height, preferredFullscreenMode);
			}
		}

		internal static void ApplyVSyncAndTargetFrameRate()
		{
			QualitySettings.vSyncCount = buffer ? 1 : 0;

#if !DEDICATED_SERVER
			int newTargetFrameRate;
			if (buffer)
			{
				// Nelson 2025-01-28: Target frame rate works with vsync "enabled" in the editor, but not in the
				// player, so turning off frame rate limit to make that clearer. (public issue #4817)
				newTargetFrameRate = -1;
			}
			else if (clTargetFrameRate.hasValue)
			{
				if (clTargetFrameRate.value <= 0)
				{
					newTargetFrameRate = -1;
				}
				else
				{
					newTargetFrameRate = Mathf.Max(clTargetFrameRate.value, 15);
				}
			}
			else if (UseTargetFrameRate)
			{
				int preferredTargetFrameRate;
				if (UseUnfocusedTargetFrameRate && !Application.isFocused)
				{
					preferredTargetFrameRate = Mathf.Min(TargetFrameRate, UnfocusedTargetFrameRate);
				}
				else
				{
					preferredTargetFrameRate = TargetFrameRate;
				}
				newTargetFrameRate = Mathf.Max(preferredTargetFrameRate, 15);
			}
			else
			{
				newTargetFrameRate = -1;
			}

			if (newTargetFrameRate != lastAppliedTargetFrameRate || !hasAppliedTargetFrameRate)
			{
				hasAppliedTargetFrameRate = true;
				lastAppliedTargetFrameRate = newTargetFrameRate;
				Application.targetFrameRate = newTargetFrameRate;
				UnturnedLog.info($"Set target frame rate to {newTargetFrameRate} fps");
			}

			if (!hasBoundApplicationFocusChangedEvent)
			{
				hasBoundApplicationFocusChangedEvent = true;
				Application.focusChanged += OnApplicationFocusChanged;
			}
#endif // !DEDICATED_SERVER
		}

#if !DEDICATED_SERVER
		private static void OnApplicationFocusChanged(bool hasFocus)
		{
			ApplyVSyncAndTargetFrameRate();
		}
#endif // !DEDICATED_SERVER

		private static bool hasAppliedTargetFrameRate = false;
		private static int lastAppliedTargetFrameRate = -1;
		private static bool hasBoundApplicationFocusChangedEvent;

		private static float vehicleCullDistanceWithMargin;

		/// <summary>
		/// Vehicles outside this distance are not rendered.
		/// </summary>
		internal static float sqrVehicleCullDistanceWithMargin;

		public static void apply(string reason)
		{
			UnturnedLog.info("Applying graphics settings ({0})", reason);

			if (changeResolution)
			{
				changeResolution = false;
				applyResolution();
			}

			if (LevelLighting.sun != null)
			{
				if (lightingQuality == EGraphicQuality.ULTRA || lightingQuality == EGraphicQuality.HIGH)
				{
					LevelLighting.sun.GetComponent<Light>().shadowNormalBias = 0;
				}
				else
				{
					LevelLighting.sun.GetComponent<Light>().shadowNormalBias = 0.5f;
				}
			}

			QualitySettings.SetQualityLevel(((byte) lightingQuality) + 1, true);

			ApplyVSyncAndTargetFrameRate();

			switch (anisotropicFilteringMode)
			{
				case EAnisotropicFilteringMode.DISABLED:
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
					break;
				case EAnisotropicFilteringMode.PER_TEXTURE:
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
					break;
				case EAnisotropicFilteringMode.FORCED_ON:
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
					break;
			}

			float farClipPlane = (0.3f + (NormalizedFarClipDistance * 0.7f)) * 2048.0f;
			if (clFarClipDistance.hasValue)
			{
				farClipPlane = Mathf.Clamp(clFarClipDistance.value, 16.0f, 2048.0f);
			}
			else if (WantsCinematicMode && !IsCinematicModeUnlimited)
			{
				farClipPlane = 4096;
			}
			if (WantsCinematicMode)
			{
				QualitySettings.shadowDistance = farClipPlane;
			}

			// Prevent 1024x1024 ground and water titles from popping in too late.
			// Maximum diagonal distance from tile center is 724.077344.
			float groundAndWaterBonusDistance = farClipPlane + 725.0f;

			// ~576 is the max net distance for several systems like vehicles and players.
			float defaultCullDistance = 256.0f + (normalizedDrawDistance * 256.0f);
			// Usually this will not be clamped, but if player uses command-line override we should use the smaller distance.
			defaultCullDistance = Mathf.Min(defaultCullDistance, farClipPlane);

			float[] layerCullDistances = new float[32];
			layerCullDistances[LayerMasks.DEFAULT] = 0;
			layerCullDistances[LayerMasks.TRANSPARENT_FX] = 0;
			layerCullDistances[LayerMasks.IGNORE_RAYCAST] = 0;
			layerCullDistances[3] = 0;
			layerCullDistances[LayerMasks.WATER] = groundAndWaterBonusDistance;
			layerCullDistances[LayerMasks.UI] = 0;
			layerCullDistances[6] = 0;
			layerCullDistances[7] = 0;
			layerCullDistances[LayerMasks.LOGIC] = Level.isEditor ? defaultCullDistance : 0;
			layerCullDistances[LayerMasks.PLAYER] = 0;
			layerCullDistances[LayerMasks.ENEMY] = defaultCullDistance;
			layerCullDistances[LayerMasks.VIEWMODEL] = 0;
			layerCullDistances[LayerMasks.DEBRIS] = defaultCullDistance;
			layerCullDistances[LayerMasks.ITEM] = defaultCullDistance * 0.125f;
			layerCullDistances[LayerMasks.RESOURCE] = defaultCullDistance;
			layerCullDistances[LayerMasks.LARGE] = defaultCullDistance;
			layerCullDistances[LayerMasks.MEDIUM] = defaultCullDistance * 0.5f;
			layerCullDistances[LayerMasks.SMALL] = defaultCullDistance * 0.125f;

			// 2022-04-26: foliage now uses this render layer as well so that scope focus foliage works at any range.
			layerCullDistances[LayerMasks.SKY] = farClipPlane;

			layerCullDistances[LayerMasks.ENVIRONMENT] = defaultCullDistance;
			layerCullDistances[LayerMasks.GROUND] = groundAndWaterBonusDistance;
			layerCullDistances[LayerMasks.CLIP] = 0;
			layerCullDistances[LayerMasks.NAVMESH] = Level.isEditor ? defaultCullDistance : 0.0f;
			layerCullDistances[LayerMasks.ENTITY] = defaultCullDistance;
			layerCullDistances[LayerMasks.AGENT] = 0;
			layerCullDistances[LayerMasks.LADDER] = 0;
			layerCullDistances[LayerMasks.VEHICLE] = defaultCullDistance;
			layerCullDistances[LayerMasks.BARRICADE] = defaultCullDistance;
			layerCullDistances[LayerMasks.STRUCTURE] = defaultCullDistance;
			layerCullDistances[LayerMasks.TIRE] = 0;
			layerCullDistances[LayerMasks.TRAP] = defaultCullDistance; // visible because of RPG
			layerCullDistances[LayerMasks.GROUND2] = farClipPlane;

			float landmarkExtraDistance = Mathf.Max(0.0f, farClipPlane - defaultCullDistance) * normalizedLandmarkDrawDistance;

			if (landmarkQuality >= EGraphicQuality.LOW)
			{
				layerCullDistances[LayerMasks.LARGE] += landmarkExtraDistance;
			}

			if (landmarkQuality >= EGraphicQuality.MEDIUM)
			{
				layerCullDistances[LayerMasks.RESOURCE] += landmarkExtraDistance;
			}

			if (landmarkQuality >= EGraphicQuality.ULTRA)
			{
				layerCullDistances[LayerMasks.ENVIRONMENT] += landmarkExtraDistance;
			}

			vehicleCullDistanceWithMargin = layerCullDistances[LayerMasks.VEHICLE] + 32.0f;
			sqrVehicleCullDistanceWithMargin = vehicleCullDistanceWithMargin * vehicleCullDistanceWithMargin;

			// Nelson 2025-03-27: prior to reworking this a bit, 3 regions were always enabled in each direction.
			// (7 x 7), so a 3.5*128 = 448 meter radius.
			LevelObjects.RegularObjectMaxDistance = Mathf.Min(defaultCullDistance, 447.0f);
			LevelGround.RegularTreeMaxDistance = LevelObjects.RegularObjectMaxDistance;

			LevelObjects.SkyboxObjectMaxDistance = landmarkQuality > EGraphicQuality.OFF ? LevelObjects.RegularObjectMaxDistance + landmarkExtraDistance : 0.0f;
			LevelGround.SkyboxTreeMaxDistance = landmarkQuality >= EGraphicQuality.MEDIUM ? LevelGround.RegularTreeMaxDistance + landmarkExtraDistance : 0.0f;

			LevelRoads.RoadMaxDistance = landmarkQuality >= EGraphicQuality.ULTRA ? (defaultCullDistance + landmarkExtraDistance) : defaultCullDistance;

			if (WantsCinematicMode)
			{
				layerCullDistances[LayerMasks.RESOURCE] = farClipPlane;
				layerCullDistances[LayerMasks.LARGE] = farClipPlane;
				layerCullDistances[LayerMasks.MEDIUM] = farClipPlane;
				layerCullDistances[LayerMasks.SMALL] = farClipPlane;
				layerCullDistances[LayerMasks.ENVIRONMENT] = farClipPlane;
			}
			else if (Level.isEditor)
			{
				const float editorFarClipMultiplier = 2.0f;

				farClipPlane *= editorFarClipMultiplier;
				for (int index = 0; index < 32; ++index)
				{
					layerCullDistances[index] *= editorFarClipMultiplier;
				}
			}

			if (!LevelObjects.shouldInstantlyLoad && !LevelGround.shouldInstantlyLoad
				&& LevelObjects.objects != null)
			{
				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						List<LevelObject> objects = LevelObjects.objects[x, y];
						for (int index = 0; index < objects.Count; index++)
						{
							LevelObject obj = objects[index];

							if (obj == null)
							{
								continue;
							}

							obj.UpdateSkyboxActive();
						}
					}
				}

				LevelGround.ForceUpdateSkyboxActive();
			}

			float lodBias = 2 + (normalizedDrawDistance * 3); // [2, 5]
			lodBias += Mathf.Clamp(Provider.preferenceData.Graphics.LOD_Bias, 0.0f, 5.0f); // Prevent negative or extended LOD exploits.
			if (WantsCinematicMode)
			{
				lodBias = IsCinematicModeUnlimited ? 1024.0f : 10.0f;
			}
			QualitySettings.lodBias = lodBias;

			// Notify LOD groups that lodBias may have changed.
			LODGroupManager.Get().SynchronizeLODBias();

			QualitySettings.skinWeights = SkinWeights.FourBones;

			if (MainCamera.instance != null)
			{
				MainCamera.instance.renderingPath = renderMode == ERenderMode.DEFERRED ? RenderingPath.DeferredShading : RenderingPath.Forward;
				MainCamera.instance.allowHDR = true;
				MainCamera.instance.allowMSAA = false; // We no longer use MSAA in any mode.

				ApplySunShaftsSettings();
				ApplyOutlineSettings();

				MainCamera.instance.farClipPlane = farClipPlane;
				MainCamera.instance.layerCullDistances = layerCullDistances;
				MainCamera.instance.layerCullSpherical = true;

				if (Player.LocalPlayer != null)
				{
					Player.LocalPlayer.look.scopeCamera.farClipPlane = farClipPlane;
					Player.LocalPlayer.look.scopeCamera.layerCullDistances = layerCullDistances;
					Player.LocalPlayer.look.scopeCamera.layerCullSpherical = true;
					Player.LocalPlayer.look.scopeCamera.depthTextureMode = DepthTextureMode.Depth;

					Player.LocalPlayer.look.updateScope(scopeQuality);

					Player.LocalPlayer.look.scopeCamera.renderingPath = renderMode == ERenderMode.DEFERRED ? RenderingPath.DeferredShading : RenderingPath.Forward;
					Player.LocalPlayer.look.scopeCamera.allowHDR = true;
					Player.LocalPlayer.look.scopeCamera.allowMSAA = false; // We no longer use MSAA in any mode.

					Player.LocalPlayer.animator.viewmodelCamera.renderingPath = renderMode == ERenderMode.DEFERRED ? RenderingPath.DeferredShading : RenderingPath.Forward;
					Player.LocalPlayer.animator.viewmodelCamera.allowHDR = true;
					Player.LocalPlayer.animator.viewmodelCamera.allowMSAA = false; // We no longer use MSAA in any mode.
				}
			}

			ApplyFoliageQuality();

			SDG.Framework.Foliage.FoliageSettings.focusDistance = farClipPlane;
			SDG.Framework.Foliage.FoliageSettings.drawFocus = foliageFocus;

			// Update water shader global keywords
			if (waterQuality == EGraphicQuality.LOW || waterQuality == EGraphicQuality.MEDIUM)
			{
				Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
				Shader.DisableKeyword("WATER_EDGEBLEND_ON");
			}
			else if (waterQuality == EGraphicQuality.HIGH || waterQuality == EGraphicQuality.ULTRA)
			{
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
				{
					Shader.EnableKeyword("WATER_EDGEBLEND_ON");
					Shader.DisableKeyword("WATER_EDGEBLEND_OFF");
				}
				else
				{
					Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
					Shader.DisableKeyword("WATER_EDGEBLEND_ON");
				}
			}

			LevelLighting.isSkyboxReflectionEnabled = skyboxReflection;

			if (IsWindEnabled)
			{
				Shader.EnableKeyword("NICE_FOLIAGE_ON");
				Shader.EnableKeyword("GRASS_WIND_ON");
			}
			else
			{
				Shader.DisableKeyword("NICE_FOLIAGE_ON");
				Shader.DisableKeyword("GRASS_WIND_ON");
			}

			if (grassDisplacement)
			{
				Shader.EnableKeyword("GRASS_DISPLACEMENT_ON");
			}
			else
			{
				Shader.DisableKeyword("GRASS_DISPLACEMENT_ON");
			}

			if (Level.info != null && Level.info.configData != null && Level.info.configData.Terrain_Snow_Sparkle && glitter)
			{
				Shader.EnableKeyword("IS_SNOWING");
			}
			else
			{
				Shader.DisableKeyword("IS_SNOWING");
			}

			if (triplanar)
			{
				Shader.EnableKeyword("TRIPLANAR_MAPPING_ON");
			}
			else
			{
				Shader.DisableKeyword("TRIPLANAR_MAPPING_ON");
			}

			++planarReflectionUpdateIndex;

			UnturnedPostProcess.instance.applyUserSettings();

			graphicsSettingsApplied?.Invoke();

			UnturnedLog.info("Applied graphics settings");
		}

		public static void restoreDefaults()
		{
			FullScreenMode wasFullscreenMode = FullScreenMode.Windowed;
			bool wasVSync = false;
			GraphicsSettingsResolution wasResolution = new GraphicsSettingsResolution();

			if (graphicsSettingsData != null)
			{
				wasFullscreenMode = graphicsSettingsData.FullscreenMode;
				wasVSync = graphicsSettingsData.IsVSyncEnabled;
				wasResolution = graphicsSettingsData.Resolution;
			}

			graphicsSettingsData = new GraphicsSettingsData();

			graphicsSettingsData.FullscreenMode = wasFullscreenMode;
			graphicsSettingsData.IsVSyncEnabled = wasVSync;
			graphicsSettingsData.Resolution = wasResolution;

			fixDefaultResolution();
			apply("restoring defaults");
		}

		/// <summary>
		/// Called after loading graphics settings from disk so that their values can be adjusted.
		/// </summary>
		private static void validateSettings()
		{
			if (graphicsSettingsData.UserInterfaceScale.IsFinite())
			{
				float oldUIScale = graphicsSettingsData.UserInterfaceScale;
				float newUIScale = Mathf.Clamp(oldUIScale, 0.5f, 2.0f);
				if (oldUIScale != newUIScale)
				{
					UnturnedLog.info($"Clamped UI scale from {oldUIScale} to {newUIScale}");
				}
				graphicsSettingsData.UserInterfaceScale = newUIScale;
			}
			else
			{
				UnturnedLog.info($"Reset UI scale (was {graphicsSettingsData.UserInterfaceScale})");
				graphicsSettingsData.UserInterfaceScale = 1.0f;
			}

			fixDefaultResolution();
		}

		/// <summary>
		/// If default resolution is zero, try falling back to a higher one.
		/// Used when restoring defaults and validating loaded settings.
		/// </summary>
		private static void fixDefaultResolution()
		{
			GraphicsSettingsResolution currentResolution = graphicsSettingsData.Resolution;
			if (currentResolution == null || currentResolution.Width < 1 || currentResolution.Height < 1)
			{
				// Has not been initialized yet, so we default to the highest recommended resolution.
				graphicsSettingsData.Resolution = new GraphicsSettingsResolution(ScreenEx.GetHighestRecommendedResolution());
				UnturnedLog.info($"Restored default resolution to {graphicsSettingsData.Resolution.Width}x{graphicsSettingsData.Resolution.Height}");
			}
		}

		internal static void ApplyFoliageQuality()
		{
			EGraphicQuality quality = foliageQuality;
			if (quality == EGraphicQuality.OFF && (Provider.modeConfigData?.Gameplay?.Disable_Foliage_Off ?? false))
			{
				quality = EGraphicQuality.LOW;
			}

			switch (quality)
			{
				case EGraphicQuality.OFF:
					SDG.Framework.Foliage.FoliageSettings.enabled = false;
					SDG.Framework.Foliage.FoliageSettings.drawDistance = 0;
					SDG.Framework.Foliage.FoliageSettings.instanceDensity = 0;

					SDG.Framework.Foliage.FoliageSettings.drawFocusDistance = 0;
					break;

				case EGraphicQuality.LOW:
					SDG.Framework.Foliage.FoliageSettings.enabled = true;
					SDG.Framework.Foliage.FoliageSettings.drawDistance = 2;
					SDG.Framework.Foliage.FoliageSettings.instanceDensity = 0.25f;

					SDG.Framework.Foliage.FoliageSettings.drawFocusDistance = 1;
					break;
				case EGraphicQuality.MEDIUM:
					SDG.Framework.Foliage.FoliageSettings.enabled = true;
					SDG.Framework.Foliage.FoliageSettings.drawDistance = 3;
					SDG.Framework.Foliage.FoliageSettings.instanceDensity = 0.5f;

					SDG.Framework.Foliage.FoliageSettings.drawFocusDistance = 2;
					break;
				case EGraphicQuality.HIGH:
					SDG.Framework.Foliage.FoliageSettings.enabled = true;
					SDG.Framework.Foliage.FoliageSettings.drawDistance = 4;
					SDG.Framework.Foliage.FoliageSettings.instanceDensity = 0.75f;

					SDG.Framework.Foliage.FoliageSettings.drawFocusDistance = 3;
					break;
				case EGraphicQuality.ULTRA:
					SDG.Framework.Foliage.FoliageSettings.enabled = true;
					SDG.Framework.Foliage.FoliageSettings.drawDistance = 5;
					SDG.Framework.Foliage.FoliageSettings.instanceDensity = 1;

					SDG.Framework.Foliage.FoliageSettings.drawFocusDistance = 4;
					break;
				default:
					SDG.Framework.Foliage.FoliageSettings.enabled = true;
					SDG.Framework.Foliage.FoliageSettings.drawDistance = 2;
					SDG.Framework.Foliage.FoliageSettings.instanceDensity = 0.25f;

					SDG.Framework.Foliage.FoliageSettings.drawFocusDistance = 1;

					UnturnedLog.error("Unknown foliage quality: " + quality);
					break;
			}
		}

		public static void load()
		{
			if (ReadWrite.fileExists("/Settings/Graphics.json", true))
			{
				try
				{
					graphicsSettingsData = ReadWrite.deserializeJSON<GraphicsSettingsData>("/Settings/Graphics.json", true);
				}
				catch (System.Exception e)
				{
					UnturnedLog.exception(e, "Unable to parse Graphics.json! consider validating with a JSON linter");
					graphicsSettingsData = null;
				}

				if (graphicsSettingsData == null)
				{
					restoreDefaults();
				}
				else
				{
					validateSettings();
				}
			}
			else
			{
				restoreDefaults();
			}

			if (graphicsSettingsData.EffectQuality == EGraphicQuality.OFF)
			{
				graphicsSettingsData.EffectQuality = EGraphicQuality.MEDIUM;
			}

			if (!Application.isEditor)
			{
				Resolution highestRecommendedResolution = ScreenEx.GetHighestRecommendedResolution();
				if (resolution.Width > highestRecommendedResolution.width || resolution.Height > highestRecommendedResolution.height)
				{
					resolution = new GraphicsSettingsResolution(highestRecommendedResolution);
				}
			}
		}

		public static void save()
		{
			ReadWrite.serializeJSON<GraphicsSettingsData>("/Settings/Graphics.json", true, graphicsSettingsData);
		}

#if DEDICATED_SERVER
		public const bool WantsCinematicMode = false;
#else
		/// <summary>
		/// If true, make the game look as nice as possible.
		/// Note: certain limits are imposed except in singleplayer to prevent this from being exploited.
		/// </summary>
		public static bool WantsCinematicMode => clEnableCinematicMode;
#endif // DEDICATED_SERVER

		public static bool IsCinematicModeUnlimited => Provider.isServer || Level.isEditor;
		private static CommandLineFlag clEnableCinematicMode = new CommandLineFlag(false, "-Cinematic");

		/// <summary>
		/// Moves legacy image effect dependency out of SDK release.
		/// </summary>
		static partial void ApplySunShaftsSettings();
		
		/// <summary>
		/// Moves highlighting plugin dependency out of SDK release.
		/// </summary>
		static partial void ApplyOutlineSettings();

		[System.Obsolete("Replaced by -Cinematic command-line flag")]
		public static bool uncapLandmarks
		{
			get => false;
			set { }
		}
	}
}
