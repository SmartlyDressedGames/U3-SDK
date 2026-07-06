////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned
{
	/// <summary>
	/// Manages global post-process volumes.
	/// </summary>
	public class UnturnedPostProcess : MonoBehaviour
	{
		public const int BASE_LAYER = LayerMasks.LOGIC;
		public const int VIEWMODEL_LAYER = LayerMasks.VIEWMODEL;
		public const int SCOPE_LAYER = LayerMasks.GROUND2;

		private bool _disableAntiAliasingForScreenshot;
		public bool DisableAntiAliasingForScreenshot
		{
			get => _disableAntiAliasingForScreenshot;
			set
			{
				if (_disableAntiAliasingForScreenshot != value)
				{
					_disableAntiAliasingForScreenshot = value;
					if (basePostProcessLayer != null)
					{
						applyAntiAliasing(basePostProcessLayer);
					}
					if (scopePostProcessLayer != null)
					{
						applyAntiAliasing(scopePostProcessLayer);
					}
				}
			}
		}

		public static UnturnedPostProcess instance
		{
			get;
			private set;
		}

		public void setBaseCamera(Camera baseCamera)
		{
			basePostProcessLayer = baseCamera.GetComponent<PostProcessLayer>();

			// Deferred fog excludes skybox because Unturned treats "atmosphere" fog separately.
			basePostProcessLayer.fog.enabled = true;
			basePostProcessLayer.fog.excludeSkybox = true;
		}

		public void setOverlayCamera(Camera overlayCamera)
		{
			viewmodelPostProcessLayer = overlayCamera.GetComponent<PostProcessLayer>();
			viewmodelPostProcessLayer.fog.enabled = false;
			viewmodelPostProcessLayer.fog.excludeSkybox = true;
		}

		public void setScopeCamera(Camera scopeCamera)
		{
			scopePostProcessLayer = scopeCamera.GetComponent<PostProcessLayer>();
			scopePostProcessLayer.fog.enabled = true; // Refer to setBaseCamera.
			scopePostProcessLayer.fog.excludeSkybox = true;
		}

		public bool IsSingleRenderScopeActive()
		{
			return baseProfile.singleRenderScope.active;
		}

		public void SetSingleRenderScopeIsActive(bool isActive)
		{
			baseProfile.singleRenderScope.active = isActive;
		}

		public void SetSingleRenderScopeZoomFactor(float zoomFactor, float alpha)
		{
			if (zoomFactor > 1.0001f)
			{
				float blur = ((zoomFactor - 1.0f) * 0.5f);
				baseProfile.singleRenderScope.standardDeviation.Override(Mathf.Min(blur, 8f));
			}
			else
			{
				baseProfile.singleRenderScope.standardDeviation.Override(-1.0f);
			}
			baseProfile.singleRenderScope.scopeAlpha.Override(alpha);
		}

		public void SetSingleRenderScopeTarget(RenderTexture target)
		{
			baseProfile.singleRenderScope.renderTarget.Override(target);
		}

		public void setIsHallucinating(bool isHallucinating)
		{
			baseProfile.colorGrading.active = isHallucinating;
			baseProfile.colorGrading.hueShift.Override(Random.Range(-180.0f, 180.0f));
			viewmodelProfile.colorGrading.active = isHallucinating;
			viewmodelProfile.colorGrading.hueShift.Override(Random.Range(-180.0f, 180.0f));
			scopeProfile.colorGrading.active = isHallucinating;
			scopeProfile.colorGrading.hueShift.Override(Random.Range(-180.0f, 180.0f));

			baseProfile.vignette.active = isHallucinating;
		}

		private void tickHallucinationColorGrading(PostProcessProfileWrapper profile, float deltaTime)
		{
			float cgSpeed = 2.5f; // How much to increase hue shift per second. Hue shift ranges from -180 to 180
			float hueShift = profile.colorGrading.hueShift.value;
			hueShift += deltaTime * cgSpeed;
			if (hueShift > 180.0f)
			{
				hueShift -= 360.0f;
			}
			profile.colorGrading.hueShift.Override(hueShift);
		}

		public void tickIsHallucinating(float deltaTime, float hallucinationTimer)
		{
			tickHallucinationColorGrading(baseProfile, deltaTime);
			tickHallucinationColorGrading(viewmodelProfile, deltaTime);
			tickHallucinationColorGrading(scopeProfile, deltaTime);

			float vignetteMaxIntensity = 0.333f;
			float vignettePeriod = 4.0f;
			baseProfile.vignette.intensity.Override(Mathf.Abs(Mathf.Sin(hallucinationTimer / vignettePeriod)) * vignetteMaxIntensity);
		}

		public void SetIsMainBlurEnabled(bool enabled)
		{
			baseProfile.dof.active = enabled;
		}

		/// <summary>
		/// Callback when in-game graphic settings change.
		/// </summary>
		public void applyUserSettings()
		{
			if (basePostProcessLayer != null)
			{
				applyAntiAliasing(basePostProcessLayer);
			}
			if (scopePostProcessLayer != null)
			{
				applyAntiAliasing(scopePostProcessLayer);
			}

			syncAmbientOcclusion();
			syncBloom();
			syncChromaticAberration();
			syncFilmGrain();
			syncScreenSpaceReflections();
		}

		/// <summary>
		/// Callback when player changes perspective.
		/// </summary>
		public void notifyPerspectiveChanged()
		{
			syncBloom();
			syncChromaticAberration();
			syncFilmGrain();
		}

		private void syncAmbientOcclusion()
		{
			baseProfile.ambientOcclusion.active = GraphicsSettings.isAmbientOcclusionEnabled;
			viewmodelProfile.ambientOcclusion.active = GraphicsSettings.isAmbientOcclusionEnabled;
			scopeProfile.ambientOcclusion.active = GraphicsSettings.isAmbientOcclusionEnabled;
		}

		private void syncBloom()
		{
			// Bloom effects apply to all pixels even with forward rendering, so only active one at a time.
			if (hasActiveOverlay)
			{
				baseProfile.bloom.active = false;
				viewmodelProfile.bloom.active = GraphicsSettings.bloom;
			}
			else
			{
				baseProfile.bloom.active = GraphicsSettings.bloom;
				viewmodelProfile.bloom.active = false;
			}
			scopeProfile.bloom.active = false;
		}

		private void syncChromaticAberration()
		{
			if (hasActiveOverlay)
			{
				baseProfile.chromaticAberration.active = false;
				viewmodelProfile.chromaticAberration.active = GraphicsSettings.chromaticAberration;
			}
			else
			{
				baseProfile.chromaticAberration.active = GraphicsSettings.chromaticAberration;
				viewmodelProfile.chromaticAberration.active = false;
			}
			scopeProfile.chromaticAberration.active = false;
		}

		private void syncFilmGrain()
		{
			if (hasActiveOverlay)
			{
				baseProfile.filmGrain.active = false;
				viewmodelProfile.filmGrain.active = GraphicsSettings.filmGrain;
			}
			else
			{
				baseProfile.filmGrain.active = GraphicsSettings.filmGrain;
				viewmodelProfile.filmGrain.active = false;
			}
			scopeProfile.filmGrain.active = false;
		}

		private void syncScreenSpaceReflections()
		{
			bool active = GraphicsSettings.reflectionQuality != EGraphicQuality.OFF && GraphicsSettings.renderMode == ERenderMode.DEFERRED;
			baseProfile.screenSpaceReflections.active = active;
			scopeProfile.screenSpaceReflections.active = false;

			if (!active)
				return;

			ScreenSpaceReflectionPreset preset;
			switch (GraphicsSettings.reflectionQuality)
			{
				default:
					preset = ScreenSpaceReflectionPreset.Low;
					break;

				case EGraphicQuality.LOW:
					preset = ScreenSpaceReflectionPreset.Low;
					break;

				case EGraphicQuality.MEDIUM:
					preset = ScreenSpaceReflectionPreset.Medium;
					break;

				case EGraphicQuality.HIGH:
					preset = ScreenSpaceReflectionPreset.High;
					break;

				case EGraphicQuality.ULTRA:
					preset = ScreenSpaceReflectionPreset.Ultra;
					break;
			}

			baseProfile.screenSpaceReflections.preset.Override(preset);
		}

		private void applyAntiAliasing(PostProcessLayer layer)
		{
			if (_disableAntiAliasingForScreenshot)
			{
				layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
				return;
			}

			switch (GraphicsSettings.antiAliasingType)
			{
				default:
				case EAntiAliasingType.OFF:
					layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
					break;

				case EAntiAliasingType.FXAA:
					layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
					break;

				case EAntiAliasingType.TAA:
					layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
					break;

				case EAntiAliasingType.SMAA:
					layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
					break;
			}
		}

		private PostProcessProfileWrapper createGlobalProfile(string name, int physicsLayer, EPostProcessLayer layer)
		{
			GameObject volumeGameObject = new GameObject(name);
			volumeGameObject.transform.parent = transform;
			volumeGameObject.layer = physicsLayer;

			PostProcessVolume volume = volumeGameObject.AddComponent<PostProcessVolume>();
			volume.isGlobal = true;
			volume.priority = 1.0f;

			return new PostProcessProfileWrapper(volume.profile, layer); // Instantiates an empty profile.
		}

		public void initialize()
		{
			if (Dedicator.IsDedicatedServer)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(this);

			baseProfile = createGlobalProfile("Base", BASE_LAYER, EPostProcessLayer.Base);
			viewmodelProfile = createGlobalProfile("Viewmodel", VIEWMODEL_LAYER, EPostProcessLayer.Viewmodel);
			scopeProfile = createGlobalProfile("Scope", SCOPE_LAYER, EPostProcessLayer.Scope);

			// Base AO is weak due to artifacts, but strong AO on the gun looks nice.
			viewmodelProfile.ambientOcclusion.intensity.Override(1.0f);

			if (Provider.preferenceData.Graphics.Use_Lens_Dirt)
			{
				baseProfile.bloom.dirtTexture.Override(dirtTexture);
				baseProfile.bloom.dirtIntensity.Override(1.0f);
				viewmodelProfile.bloom.dirtTexture.Override(dirtTexture);
				viewmodelProfile.bloom.dirtIntensity.Override(1.0f);
			}

			baseProfile.chromaticAberration.intensity.Override(Provider.preferenceData.Graphics.Chromatic_Aberration_Intensity);
			viewmodelProfile.chromaticAberration.intensity.Override(Provider.preferenceData.Graphics.Chromatic_Aberration_Intensity);
			scopeProfile.chromaticAberration.intensity.Override(Provider.preferenceData.Graphics.Chromatic_Aberration_Intensity);
		}

		public Texture dirtTexture;

		private PostProcessProfileWrapper baseProfile;
		private PostProcessProfileWrapper viewmodelProfile;
		private PostProcessProfileWrapper scopeProfile;

		private PostProcessLayer basePostProcessLayer;
		private PostProcessLayer viewmodelPostProcessLayer;
		private PostProcessLayer scopePostProcessLayer;

		private bool hasActiveOverlay => viewmodelPostProcessLayer != null && viewmodelPostProcessLayer.gameObject.activeInHierarchy;

		private enum EPostProcessLayer
		{
			Base,
			Viewmodel,
			Scope,
		}

		private class PostProcessProfileWrapper
		{
			public PostProcessProfile profile;
			public AmbientOcclusion ambientOcclusion;
			public Bloom bloom;
			public ChromaticAberration chromaticAberration;
			public ColorGrading colorGrading;
			public Grain filmGrain;
			public ScreenSpaceReflections screenSpaceReflections;
			public Vignette vignette;
			public DepthOfField dof;
			public SrScope singleRenderScope;

			public PostProcessProfileWrapper(PostProcessProfile profile, EPostProcessLayer layer)
			{
				this.profile = profile;

				ambientOcclusion = profile.AddSettings<AmbientOcclusion>();
				ambientOcclusion.active = false;
				ambientOcclusion.intensity.Override(0.25f);

				bloom = profile.AddSettings<Bloom>();
				bloom.active = false;
				bloom.intensity.Override(1f);
				bloom.softKnee.Override(0f);

				colorGrading = profile.AddSettings<ColorGrading>();
				colorGrading.active = false;

				chromaticAberration = profile.AddSettings<ChromaticAberration>();
				chromaticAberration.active = false;

				filmGrain = profile.AddSettings<Grain>();
				filmGrain.active = false;
				filmGrain.intensity.Override(0.25f);

				screenSpaceReflections = profile.AddSettings<ScreenSpaceReflections>();
				screenSpaceReflections.active = false;

				vignette = profile.AddSettings<Vignette>();
				vignette.active = false;
				vignette.rounded.Override(true);

				if (layer == EPostProcessLayer.Base)
				{
					// We currently use depth of field to as a background blur for the in-game dashboard. ;)
					dof = profile.AddSettings<DepthOfField>();
					dof.active = false;
					dof.focusDistance.Override(1.0f);
				}

				if (layer != EPostProcessLayer.Viewmodel)
				{
					profile.AddSettings<SkyFog>();
				}

				if (layer == EPostProcessLayer.Base)
				{
					singleRenderScope = profile.AddSettings<SrScope>();
					singleRenderScope.active = false;
				}
			}
		}
	}
}
