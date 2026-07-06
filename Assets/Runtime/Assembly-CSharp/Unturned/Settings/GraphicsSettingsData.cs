////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class GraphicsSettingsData
	{
		public GraphicsSettingsResolution Resolution { get; set; }

		public FullScreenMode FullscreenMode;
		public bool IsVSyncEnabled { get; set; }
		public float UserInterfaceScale;
		public bool UseTargetFrameRate;
		public int TargetFrameRate;
		public bool UseUnfocusedTargetFrameRate;
		public int UnfocusedTargetFrameRate;
		public bool IsAmbientOcclusionEnabled;
		public bool IsBloomEnabled { get; set; }
		public bool IsChromaticAberrationEnabled { get; set; }
		public bool IsFilmGrainEnabled { get; set; }
		public bool IsNiceBlendEnabled { get; set; }
		public bool IsGrassDisplacementEnabled;
		public bool IsFoliageFocusEnabled;
		public bool IsRagdollsEnabled;
		public bool IsDebrisEnabled;
		public bool IsBlastEnabled;
		public bool IsPuddleEnabled;
		public bool IsGlitterEnabled;
		public bool IsTriplanarMappingEnabled;
		public bool IsSkyboxReflectionEnabled;
		public bool IsItemIconAntiAliasingEnabled;
		public bool IsClutterEnabled;
		public bool IsDarkScopePeripheralEnabled;
		public bool IsWindEnabled;

		/// <summary>
		/// Far clip plane multiplier in-game.
		/// </summary>
		public float FarClipDistance;
		/// <summary>
		/// Far clip plane multiplier in level editor.
		/// </summary>
		public float EditorFarClipDistance;

		public float DrawDistance { get; set; }
		public float LandmarkDistance { get; set; }

		public EAntiAliasingType AntiAliasingType5 { get; set; }
		public EAnisotropicFilteringMode AnisotropicFilteringMode { get; set; }
		public EGraphicQuality EffectQuality { get; set; }
		public EGraphicQuality FoliageQuality2 { get; set; }
		public EGraphicQuality SunShaftsQuality { get; set; }
		public EGraphicQuality LightingQuality { get; set; }
		public EGraphicQuality ScreenSpaceReflectionQuality { get; set; }
		public EGraphicQuality PlanarReflectionQuality { get; set; }
		public EGraphicQuality WaterQuality { get; set; }
		public EGraphicQuality ScopeQuality2 { get; set; }
		public EGraphicQuality OutlineQuality { get; set; }
		public EGraphicQuality TerrainQuality { get; set; }
		public ERenderMode RenderMode2;
		public EGraphicQuality LandmarkQuality;

		public GraphicsSettingsData()
		{
			// Default resolution is configured in settings validation.
			this.Resolution = new GraphicsSettingsResolution();

			FullscreenMode = FullScreenMode.FullScreenWindow; // Borderless
			this.IsVSyncEnabled = false;
			UserInterfaceScale = 1.0f;

			UseTargetFrameRate = false;
			TargetFrameRate = 1000;
			UseUnfocusedTargetFrameRate = false;
			UnfocusedTargetFrameRate = 1000;

			IsAmbientOcclusionEnabled = false;
			this.IsBloomEnabled = false;
			this.IsChromaticAberrationEnabled = false;
			this.IsFilmGrainEnabled = false;
			this.IsNiceBlendEnabled = true;
			this.IsGrassDisplacementEnabled = false;
			this.IsWindEnabled = true;
			this.IsFoliageFocusEnabled = false;
			this.IsRagdollsEnabled = true;
			this.IsDebrisEnabled = true;
			this.IsBlastEnabled = true;
			this.IsPuddleEnabled = true;
			this.IsGlitterEnabled = true;
			this.IsTriplanarMappingEnabled = true;
			this.IsSkyboxReflectionEnabled = false;
			IsItemIconAntiAliasingEnabled = false;
			IsClutterEnabled = true;
			IsDarkScopePeripheralEnabled = false;

			// 2022-04-11: min is 50% and max is 200%, so default to "100%"
			FarClipDistance = 0.333333f;
			EditorFarClipDistance = 1.0f;
			this.DrawDistance = 1.0f;
			this.LandmarkDistance = 0.0f;

			this.AntiAliasingType5 = EAntiAliasingType.OFF;
			this.AnisotropicFilteringMode = EAnisotropicFilteringMode.FORCED_ON;
			this.EffectQuality = EGraphicQuality.MEDIUM;
			this.FoliageQuality2 = EGraphicQuality.LOW;
			this.SunShaftsQuality = EGraphicQuality.OFF;
			this.LightingQuality = EGraphicQuality.LOW;
			this.ScreenSpaceReflectionQuality = EGraphicQuality.OFF;
			this.PlanarReflectionQuality = EGraphicQuality.MEDIUM;
			this.WaterQuality = EGraphicQuality.LOW;
			this.ScopeQuality2 = EGraphicQuality.OFF;
			this.OutlineQuality = EGraphicQuality.LOW;
			this.TerrainQuality = EGraphicQuality.MEDIUM;
			this.RenderMode2 = ERenderMode.FORWARD;
			this.LandmarkQuality = EGraphicQuality.OFF;
		}
	}
}
