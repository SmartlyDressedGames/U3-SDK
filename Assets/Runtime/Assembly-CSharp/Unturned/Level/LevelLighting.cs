////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Water;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum ELightingRain
	{
		/// <summary>
		/// Corresponds to not active and not blending with new weather system. 
		/// </summary>
		NONE,
		/// <summary>
		/// Corresponds to transitioning in with new weather system. 
		/// </summary>
		PRE_DRIZZLE,
		/// <summary>
		/// Corresponds to active with new weather system. 
		/// </summary>
		DRIZZLE,
		/// <summary>
		/// Corresponds to transitioning out with new weather system. 
		/// </summary>
		POST_DRIZZLE
	}

	public enum ELightingSnow
	{
		/// <summary>
		/// Corresponds to not active and not blending with new weather system. 
		/// </summary>
		NONE,
		/// <summary>
		/// Corresponds to transitioning in with new weather system. 
		/// </summary>
		PRE_BLIZZARD,
		/// <summary>
		/// Corresponds to active with new weather system. 
		/// </summary>
		BLIZZARD,
		/// <summary>
		/// Corresponds to transitioning out with new weather system. 
		/// </summary>
		POST_BLIZZARD
	}

	public partial class LevelLighting
	{
		public static bool enableUnderwaterEffects => !Level.isEditor || _editorWantsUnderwaterEffects;

		private static bool _editorWantsUnderwaterEffects = true;
		public static bool EditorWantsUnderwaterEffects
		{
			get => _editorWantsUnderwaterEffects;
			set
			{
				_editorWantsUnderwaterEffects = value;
				ConvenientSavedata.get().write("EditorWantsUnderwaterEffects", value);
			}
		}

		private static bool _editorWantsWaterSurface = true;
		public static bool EditorWantsWaterSurface
		{
			get => _editorWantsWaterSurface;
			set
			{
				if (_editorWantsWaterSurface != value)
				{
					_editorWantsWaterSurface = value;
					ConvenientSavedata.get().write("EditorWantsWaterSurfaceVisible", value);

					if (Level.isEditor)
					{
						WaterVolumeManager.Get().ForceUpdateEditorVisibility();
					}
				}
			}
		}

		private static bool _editorWantsNoLightingPreview = false;
		public static bool EditorWantsNoLightingPreview
		{
			get => _editorWantsNoLightingPreview;
			set
			{
				_editorWantsNoLightingPreview = value;
				ConvenientSavedata.get().write("EditorWantsNoLightingPreview", value);
			}
		}

		public static readonly byte SAVEDATA_VERSION = 12;

		public static readonly byte MOON_CYCLES = 5;
		[System.Obsolete("Never used?")]
		public static readonly float CLOUDS = 2;
		public static readonly float AUDIO_MIN = 0.075f;
		public static readonly float AUDIO_MAX = 0.15f;

		private static readonly Color FOAM_DAWN = new Color(0.125f, 0, 0, 0);
		private static readonly Color FOAM_MIDDAY = new Color(0.25f, 0, 0, 0);
		private static readonly Color FOAM_DUSK = new Color(0.05f, 0, 0, 0);
		private static readonly Color FOAM_MIDNIGHT = new Color(0.01f, 0, 0, 0);

		private static readonly float SPECULAR_DAWN = 5f;
		private static readonly float SPECULAR_MIDDAY = 50f;
		private static readonly float SPECULAR_DUSK = 5f;
		private static readonly float SPECULAR_MIDNIGHT = 50f;

		private static readonly float PITCH_DARK_WATER_BLEND = 0.9f;

		private static readonly float REFLECTION_DAWN = 0.75f;
		private static readonly float REFLECTION_MIDDAY = 0.75f;
		private static readonly float REFLECTION_DUSK = 0.5f;
		private static readonly float REFLECTION_MIDNIGHT = 0.5f;

		internal static readonly Color NIGHTVISION_MILITARY = new Color32(20, 120, 80, 0);
		internal static readonly Color NIGHTVISION_CIVILIAN = new Color(0.4f, 0.4f, 0.4f, 0);

		private static float _azimuth;
		public static float azimuth
		{
			get => _azimuth;

			set
			{
				_azimuth = value;

				updateLighting();
			}
		}

		private static float _transition;
		public static float transition => _transition;

		private static float _bias;
		public static float bias
		{
			get => _bias;

			set
			{
				_bias = value;

				if (bias < 1 - bias) // morning shorter than evening
				{
					_transition = bias / 2 * fade;
				}
				else
				{
					_transition = (1 - bias) / 2 * fade;
				}

				updateLighting();
			}
		}

		private static float _fade;
		public static float fade
		{
			get => _fade;

			set
			{
				_fade = value;

				if (bias < 1 - bias) // morning shorter than evening
				{
					_transition = bias / 2 * fade;
				}
				else
				{
					_transition = (1 - bias) / 2 * fade;
				}

				updateLighting();
			}
		}

		private static float _time;
		public static float time
		{
			get => _time;

			set
			{
				UnityEngine.Profiling.Profiler.BeginSample("LevelLighting.time.set");
				// Essentially which angle is shorter around the 0-1 circle: direct or looping around.
				float delta = Mathf.Min(Mathf.Abs(value - _time), value + 1.0f - _time);

				// Update reflections if this was an admin command day-to-night change.
				// 'OR' because vision may have triggered a change even if delta does not.
				skyboxNeedsReflectionUpdate = skyboxNeedsReflectionUpdate || (delta > 0.05f);

				// We update time regardless of delta because in play mode there are other factors like ambiance volume.
				_time = value;
				updateLighting();
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		private static float _wind;
		public static float wind
		{
			set => _wind = value;
			get => _wind;
		}

		/// <summary>
		/// Kept for backwards compatibility with mod hooks, plugins, and events.
		/// </summary>
		public static ELightingRain rainyness;
		/// <summary>
		/// Kept for backwards compatibility with mod hooks, plugins, and events.
		/// </summary>
		public static ELightingSnow snowyness;

		private class CustomWeatherInstance
		{
			public WeatherAssetBase asset;
			public NetId netId;

			/// <summary>
			/// [0, 1] used to avoid invoking BlendAlphaChanged every frame.
			/// Compared against globalBlendAlpha not taking into account local volume.
			/// </summary>
			public float eventBlendAlpha;

			public GameObject gameObject;
			public WeatherComponentBase component;

			public void initialize()
			{
				gameObject = new GameObject(asset.name);
				gameObject.transform.parent = lighting;
				gameObject.transform.localPosition = Vector3.zero;
				component = gameObject.AddComponent(asset.componentType) as WeatherComponentBase;
				component.asset = asset;
				component.netId = netId;

				if (!netId.IsNull())
				{
					NetIdRegistry.Assign(netId, component);
				}

				component.InitializeWeather();

				if (asset.hasLightning && !netId.IsNull())
				{
					LightningWeatherComponent lightning = gameObject.AddComponent<LightningWeatherComponent>();
					lightning.weatherComponent = component;
					lightning.netId = netId + 1U;
					NetIdRegistry.Assign(lightning.netId, lightning);
				}
			}

			public void teardown()
			{
				if (component != null)
				{
					component.PreDestroyWeather();
					component = null;
				}

				if (gameObject != null)
				{
					Object.Destroy(gameObject);
					gameObject = null;
				}

				if (!netId.IsNull())
				{
					NetIdRegistry.Release(netId);
					netId.Clear();
				}
			}
		}

		private static List<CustomWeatherInstance> customWeatherInstances = new List<CustomWeatherInstance>();
		private static CustomWeatherInstance activeCustomWeather = null;

		public static float GetFishingBiteIntervalMultiplier(uint weatherMask)
		{
			float result = 1.0f;

			foreach (CustomWeatherInstance instance in customWeatherInstances)
			{
				if ((instance.asset.volumeMask & weatherMask) != 0)
				{
					result *= Mathf.Lerp(1.0f, instance.asset.FishBiteIntervalMultiplier, instance.component.globalBlendAlpha);
				}
			}

			return result;
		}

		private static CustomWeatherInstance FindWeatherInstanceByAsset(WeatherAssetBase asset)
		{
			foreach (CustomWeatherInstance instance in customWeatherInstances)
			{
				if (instance.asset == asset)
					return instance;
			}
			return null;
		}

		[System.Obsolete("Renamed to GetActiveWeatherAsset")]
		public static WeatherAssetBase getCustomWeather()
		{
			return GetActiveWeatherAsset();
		}

		public static WeatherAssetBase GetActiveWeatherAsset()
		{
			return activeCustomWeather != null ? activeCustomWeather.asset : null;
		}

		public static float GetActiveWeatherGlobalBlendAlpha()
		{
			return activeCustomWeather != null ? activeCustomWeather.component.globalBlendAlpha : 0.0f;
		}

		public static bool GetActiveWeatherNetState(out WeatherAssetBase asset, out float blendAlpha, out NetId netId)
		{
			if (activeCustomWeather != null)
			{
				asset = activeCustomWeather.asset;
				blendAlpha = activeCustomWeather.component.globalBlendAlpha;
				netId = activeCustomWeather.component.GetNetId();
				return true;
			}
			else
			{
				asset = null;
				blendAlpha = 0.0f;
				netId = default;
				return false;
			}
		}

		public static bool IsWeatherActive(WeatherAssetBase asset)
		{
			return activeCustomWeather != null && activeCustomWeather.asset == asset;
		}

		public static bool IsWeatherTransitioningIn(WeatherAssetBase asset)
		{
			// Only the active weather is ever transitioning in.
			return activeCustomWeather != null && !activeCustomWeather.component.isFullyTransitionedIn && activeCustomWeather.asset == asset;
		}

		public static bool IsWeatherFullyTransitionedIn(WeatherAssetBase asset)
		{
			// Only the active weather can ever be fully transitioned in.
			return activeCustomWeather != null && activeCustomWeather.component.isFullyTransitionedIn && activeCustomWeather.asset == asset;
		}

		public static bool IsWeatherTransitioningOut(WeatherAssetBase asset)
		{
			CustomWeatherInstance instance = FindWeatherInstanceByAsset(asset);
			return instance != null && !instance.component.isWeatherActive;
		}

		public static bool IsWeatherFullyTransitionedOut(WeatherAssetBase asset)
		{
			return FindWeatherInstanceByAsset(asset) == null;
		}

		public static bool IsWeatherTransitioning(WeatherAssetBase asset)
		{
			CustomWeatherInstance instance = FindWeatherInstanceByAsset(asset);
			return instance != null && !instance.component.isFullyTransitionedIn;
		}

		public static float GetWeatherGlobalBlendAlpha(WeatherAssetBase asset)
		{
			CustomWeatherInstance instance = FindWeatherInstanceByAsset(asset);
			return instance != null ? instance.component.globalBlendAlpha : 0.0f;
		}

		internal static bool GetWeatherStateForListeners(System.Guid assetGuid, out bool isActive, out bool isFullyTransitionedIn)
		{
			foreach (CustomWeatherInstance instance in customWeatherInstances)
			{
				if (instance.asset.GUID == assetGuid)
				{
					isActive = instance.component.isWeatherActive;
					isFullyTransitionedIn = instance.component.isFullyTransitionedIn;
					return true;
				}
			}

			isActive = false;
			isFullyTransitionedIn = false;
			return false;
		}

		[System.Obsolete]
		public static void setCustomWeather(WeatherAssetBase asset)
		{ }

		internal static void SetActiveWeatherAsset(WeatherAssetBase asset, float blendAlpha, NetId netId)
		{
			if (activeCustomWeather != null)
			{
				if (activeCustomWeather.asset == asset)
					return; // Custom weather has not changed.

				activeCustomWeather.component.OnBeginTransitionOut();
				WeatherEventListenerManager.InvokeBeginTransitionOut(activeCustomWeather.asset.GUID);
				WeatherEventListenerManager.InvokeStatusChange(activeCustomWeather.asset, EWeatherStatusChange.BeginTransitionOut);
				activeCustomWeather.component.isWeatherActive = false;
				activeCustomWeather = null;
			}

			if (asset == null)
				return; // Custom weather is no longer active.

			foreach (CustomWeatherInstance instance in customWeatherInstances)
			{
				if (instance.asset.GUID == asset.GUID)
				{
					activeCustomWeather = instance;
					break;
				}
			}

			if (activeCustomWeather == null)
			{
				activeCustomWeather = new CustomWeatherInstance();
				activeCustomWeather.asset = asset;
				activeCustomWeather.netId = netId;
				activeCustomWeather.initialize();
				customWeatherInstances.Add(activeCustomWeather);
			}

			activeCustomWeather.component.isWeatherActive = true;
			WeatherEventListenerManager.InvokeBeginTransitionIn(activeCustomWeather.asset.GUID);
			WeatherEventListenerManager.InvokeStatusChange(asset, EWeatherStatusChange.BeginTransitionIn);
			activeCustomWeather.component.globalBlendAlpha = blendAlpha;
			activeCustomWeather.component.OnBeginTransitionIn();
		}

		[System.Obsolete]
		public static float christmasyness
		{
			get;
			private set;
		}

		[System.Obsolete]
		public static float blizzardyness
		{
			get;
			private set;
		}

		[System.Obsolete]
		public static float mistyness
		{
			get;
			private set;
		}

		[System.Obsolete]
		public static float drizzlyness
		{
			get;
			private set;
		}

		/// <summary>
		/// Hash of lighting config.
		/// Prevents using the level editor to make night time look like day.
		/// </summary>
		public static byte[] hash
		{
			get;
			private set;
		}

		private static LightingInfo[] _times;
		public static LightingInfo[] times => _times;

		private static float _seaLevel;
		public static float seaLevel
		{
			get => _seaLevel;

			set
			{
				_seaLevel = value;

				UpdateBubblesActive();
				UpdateLegacyWaterTransform();
			}
		}

		private static float _snowLevel;
		public static float snowLevel
		{
			get => _snowLevel;

			set => _snowLevel = value;
		}

		public static float rainFreq;
		public static float rainDur;
		public static float snowFreq;
		public static float snowDur;

		public static bool canRain;
		public static bool canSnow;

		private static ELightingVision _vision;
		public static ELightingVision vision
		{
			get => _vision;
			set
			{
				if (value != _vision)
				{
					_vision = value;
					skyboxNeedsReflectionUpdate = true; // Force sky cubemap update.
				}
			}
		}

		public static Color nightvisionColor;
		public static float nightvisionFogIntensity;

		public delegate void IsSeaChangedHandler(bool isSea);
		public static event IsSeaChangedHandler isSeaChanged;

		protected static bool _isSea;
		public static bool isSea
		{
			get => _isSea;
			protected set
			{
				if (isSea == value)
				{
					return;
				}
				_isSea = value;

				isSeaChanged?.Invoke(isSea);

				skyboxNeedsReflectionUpdate = true; // Force sky cubemap update.
			}
		}

		private static Material skybox;
		private static Transform lighting;
		private static Rain puddles;

		private static bool cloudOverrideParticlesNeedRestart;
		private static CloudParticleSystemInstance[] cloudOverrideParticles;
		struct CloudParticleSystemInstance
		{
			public LevelAsset.CloudOverrideParticleSystemsPath config;
			public ParticleSystem particleSystem;
			public float defaultEmissionRateMin;
			public float defaultEmissionRateMax;
			public Material material;
			public CloudParticleSystemMaterialColor[] materialColorProperties;
		}

		struct CloudParticleSystemMaterialColor
		{
			public int propertyId;
			public float defaultColorAlpha;
		}

		private static float auroraBorealisCurrentIntensity;
		private static float auroraBorealisTargetIntensity;

		public static Color skyboxSky { get; private set; }
		public static Color skyboxEquator { get; private set; }
		private static Color skyboxGround;
		private static Color cloudColor;
		private static Color cloudRimColor;
		private static bool skyboxNeedsReflectionUpdate;
		private static float lastSkyboxReflectionUpdate;
		private static Color particleLightingColor;

		private static Color raysColor;
		private static float raysIntensity;

		/// <summary>
		/// Level designed target fog color.
		/// </summary>
		private static Color levelFogColor;

		/// <summary>
		/// Level designed target fog intensity.
		/// </summary>
		private static float levelFogIntensity;

		/// <summary>
		/// Level designed target atmospheric fog intensity.
		/// </summary>
		private static float levelAtmosphericFog;

		public static Transform sun;
		private static Light sunLight;
		private static Transform sunFlare;

		private static GameObject ambianceAudioGameObject;
		private static List<AmbianceAudioInstance> activeAmbianceAudioInstances;
		private static Stack<AmbianceAudioInstance> ambianceAudioPool;

		private static AudioSource _dayAudio;
		public static AudioSource dayAudio => _dayAudio;

		private static AudioSource _nightAudio;
		public static AudioSource nightAudio => _nightAudio;

		private static AudioSource _waterAudio;
		public static AudioSource waterAudio => _waterAudio;

		private static AudioSource _windAudio;
		public static AudioSource windAudio => _windAudio;

		private static AudioSource _belowAudio;
		public static AudioSource belowAudio => _belowAudio;

		private static float currentAudioVolume;
		private static float targetAudioVolume;
		private static float nextAudioVolumeChangeTime;
		private static float dayVolume;
		private static float nightVolume;

		private static Camera reflectionCamera;
		private static RenderTexture reflectionMap;
		private static RenderTexture reflectionMapVision;
		private static int reflectionIndex;
		private static int reflectionIndexVision;
		private static bool isReflectionBuilding;
		private static bool isReflectionBuildingVision;

		private static bool _isSkyboxReflectionEnabled;
		public static bool isSkyboxReflectionEnabled
		{
			get => _isSkyboxReflectionEnabled;

			set
			{
				_isSkyboxReflectionEnabled = value;

				updateSkyboxReflections();
			}
		}

		private static Transform _bubbles;
		public static Transform bubbles => _bubbles;

		private static WindZone _windZone;
		public static WindZone windZone => _windZone;

		private static WaterVolume legacyWater;
		private static Transform legacyWaterTransform;

		// Moon transforms represent same "light directions" as old materials.
		private static Transform[] moons;
		private static byte _moon;
		public static byte moon
		{
			get => _moon;
			set => _moon = value;
		}

		public static void setEnabled(bool isEnabled)
		{
			if (sun != null)
			{
				sunLight.enabled = isEnabled;
			}
		}

		public static bool isPositionSnowy(Vector3 position)
		{
			if (Level.info != null && Level.info.configData.Use_Legacy_Snow_Height)
			{
				return snowLevel > 0.01f && position.y > snowLevel * Level.TERRAIN;
			}

			return false;
		}

		[System.Obsolete("Replaced by WaterUtility")]
		public static bool isPositionUnderwater(Vector3 position)
		{
			if (Level.info != null && Level.info.configData.Use_Legacy_Water)
			{
				return seaLevel < 0.99f && position.y < seaLevel * Level.TERRAIN;
			}

			return false;
		}

		/// <summary>
		/// If global ocean plane is enabled then return the worldspace height,
		/// otherwise return the optional default value. Default for volume based
		/// water is -1024, but atmosphere measure uses a default of zero.
		/// </summary>
		public static float getWaterSurfaceElevation(float defaultValue = -1024)
		{
			// seaLevel at 1.0f is a hack to disable water on older maps.
			if (Level.info != null && Level.info.configData.Use_Legacy_Water && seaLevel < 0.99f)
			{
				return seaLevel * Level.TERRAIN;
			}
			else
			{
				return defaultValue;
			}
		}

		public static void setSeaVector(string name, Vector4 vector)
		{
			UnityEngine.Profiling.Profiler.BeginSample("LevelLighting.setSeaVector()");
			foreach (SDG.Framework.Water.WaterVolume volume in SDG.Framework.Water.WaterVolumeManager.Get().InternalGetAllVolumes())
			{
				if (volume.sharedMaterial == null)
				{
					continue;
				}

				volume.sharedMaterial.SetVector(name, vector);
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static Color getSeaColor(string name)
		{
			WaterVolume waterVolume = GetFirstOrDefaultWaterVolume();
			return waterVolume?.sharedMaterial?.GetColor(name) ?? Vector4.zero;
		}

		public static void setSeaColor(string name, Color color)
		{
			foreach (SDG.Framework.Water.WaterVolume volume in SDG.Framework.Water.WaterVolumeManager.Get().InternalGetAllVolumes())
			{
				if (volume.sharedMaterial == null)
				{
					continue;
				}

				volume.sharedMaterial.SetColor(name, color);
			}
		}

		public static float getSeaFloat(string name)
		{
			WaterVolume waterVolume = GetFirstOrDefaultWaterVolume();
			return waterVolume?.sharedMaterial?.GetFloat(name) ?? 0.0f;
		}

		public static void setSeaFloat(string name, float value)
		{
			foreach (SDG.Framework.Water.WaterVolume volume in SDG.Framework.Water.WaterVolumeManager.Get().InternalGetAllVolumes())
			{
				if (volume.sharedMaterial == null)
				{
					continue;
				}

				volume.sharedMaterial.SetFloat(name, value);
			}
		}

		private static WaterVolume GetFirstOrDefaultWaterVolume()
		{
			IReadOnlyList<WaterVolume> waterVolumes = SDG.Framework.Water.WaterVolumeManager.Get().InternalGetAllVolumes();
			return waterVolumes.Count > 0 ? waterVolumes[0] : null;
		}

		private static void GetLightingIndices(out int blendLightingIndex, out int currentLightingIndex, out float blendAlpha)
		{
			if (time < bias) // before or after dusk
			{
				if (time < transition) // dawn - midday
				{
					blendLightingIndex = (int) ELightingTime.DAWN;
					currentLightingIndex = (int) ELightingTime.MIDDAY;
					blendAlpha = time / transition;
				}
				else if (time < bias - transition) // midday
				{
					blendLightingIndex = -1;
					currentLightingIndex = (int) ELightingTime.MIDDAY;
					blendAlpha = 0;
				}
				else // midday - dusk
				{
					blendLightingIndex = (int) ELightingTime.MIDDAY;
					currentLightingIndex = (int) ELightingTime.DUSK;
					blendAlpha = (time - bias + transition) / transition;
				}
			}
			else
			{
				if (time < bias + transition) // dusk - midnight
				{
					blendLightingIndex = (int) ELightingTime.DUSK;
					currentLightingIndex = (int) ELightingTime.MIDNIGHT;
					blendAlpha = (time - bias) / transition;
				}
				else if (time < 1 - transition) // midnight
				{
					blendLightingIndex = -1;
					currentLightingIndex = (int) ELightingTime.MIDNIGHT;
					blendAlpha = 0;
				}
				else // midnight to dawn
				{
					blendLightingIndex = (int) ELightingTime.MIDNIGHT;
					currentLightingIndex = (int) ELightingTime.DAWN;
					blendAlpha = (time - 1 + transition) / transition;
				}
			}
		}

		public static void updateLighting()
		{
			UnityEngine.Profiling.Profiler.BeginSample("LevelLighting.updateLighting()");
			if (sun == null)
			{
				UnityEngine.Profiling.Profiler.EndSample();
				return;
			}

			// Indices into times array.
			int blendLightingIndex;
			int currentLightingIndex;

			float starsCutoff;
			float blend = 0f;

			setSeaVector("_WorldLightDir", sun.forward);
			if (time < bias) // before or after dusk
			{
				sun.rotation = Quaternion.Euler(time / bias * 180, azimuth, 0);

				if (time < transition) // dawn - midday
				{
					dayVolume = Mathf.Lerp(0.5f, 1, time / transition);
					nightVolume = Mathf.Lerp(0.5f, 0, time / transition);

					blendLightingIndex = (int) ELightingTime.DAWN;
					currentLightingIndex = (int) ELightingTime.MIDDAY;

					blend = time / transition;

					setSeaColor("_Foam", Color.Lerp(FOAM_DAWN, FOAM_MIDDAY, time / transition));
					setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_DAWN, SPECULAR_MIDDAY, time / transition));

					RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_DAWN, REFLECTION_MIDDAY, time / transition);
				}
				else if (time < bias - transition) // midday
				{
					dayVolume = 1;
					nightVolume = 0;

					blendLightingIndex = -1;
					currentLightingIndex = (int) ELightingTime.MIDDAY;

					setSeaColor("_Foam", FOAM_MIDDAY);
					setSeaFloat("_Shininess", SPECULAR_MIDDAY);

					RenderSettings.reflectionIntensity = REFLECTION_MIDDAY;
				}
				else // midday - dusk
				{
					dayVolume = Mathf.Lerp(1, 0.5f, (time - bias + transition) / transition);
					nightVolume = Mathf.Lerp(0, 0.5f, (time - bias + transition) / transition);

					blendLightingIndex = (int) ELightingTime.MIDDAY;
					currentLightingIndex = (int) ELightingTime.DUSK;

					blend = (time - bias + transition) / transition;

					setSeaColor("_Foam", Color.Lerp(FOAM_MIDDAY, FOAM_DUSK, (time - bias + transition) / transition));
					setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_MIDDAY, SPECULAR_DUSK, (time - bias + transition) / transition));

					RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_MIDDAY, REFLECTION_DUSK, (time - bias + transition) / transition);
				}

				starsCutoff = 1.0f;
				auroraBorealisTargetIntensity = 0;
			}
			else
			{
				sun.rotation = Quaternion.Euler(180 + ((time - bias) / (1 - bias) * 180), azimuth, 0);

				if (time < bias + transition) // dusk - midnight
				{
					dayVolume = Mathf.Lerp(0.5f, 0, (time - bias) / transition);
					nightVolume = Mathf.Lerp(0.5f, 1, (time - bias) / transition);

					blendLightingIndex = (int) ELightingTime.DUSK;
					currentLightingIndex = (int) ELightingTime.MIDNIGHT;

					blend = (time - bias) / transition;

					setSeaColor("_Foam", Color.Lerp(FOAM_DUSK, FOAM_MIDNIGHT, (time - bias) / transition));
					setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_DUSK, SPECULAR_MIDNIGHT, (time - bias) / transition));

					RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_DUSK, REFLECTION_MIDNIGHT, (time - bias) / transition);

					starsCutoff = Mathf.Lerp(1.0f, 0.05f, blend);
					auroraBorealisTargetIntensity = 0;
				}
				else if (time < 1 - transition) // midnight
				{
					dayVolume = 0;
					nightVolume = 1;

					blendLightingIndex = -1;
					currentLightingIndex = (int) ELightingTime.MIDNIGHT;

					setSeaColor("_Foam", FOAM_MIDNIGHT);
					setSeaFloat("_Shininess", SPECULAR_MIDNIGHT);

					RenderSettings.reflectionIntensity = REFLECTION_MIDNIGHT;

					starsCutoff = 0.05f;
					auroraBorealisTargetIntensity = 1;
				}
				else // midnight to dawn
				{
					dayVolume = Mathf.Lerp(0, 0.5f, (time - 1 + transition) / transition);
					nightVolume = Mathf.Lerp(1, 0.5f, (time - 1 + transition) / transition);

					blendLightingIndex = (int) ELightingTime.MIDNIGHT;
					currentLightingIndex = (int) ELightingTime.DAWN;

					blend = (time - 1 + transition) / transition;

					setSeaColor("_Foam", Color.Lerp(FOAM_MIDNIGHT, FOAM_DAWN, (time - 1 + transition) / transition));
					setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_MIDNIGHT, SPECULAR_DAWN, (time - 1 + transition) / transition));

					RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_MIDNIGHT, REFLECTION_DAWN, (time - 1 + transition) / transition);

					starsCutoff = Mathf.Lerp(0.05f, 1.0f, blend);
					auroraBorealisTargetIntensity = 0;
				}
			}

			LightingInfo blendLighting = blendLightingIndex < 0 ? null : times[blendLightingIndex];
			LightingInfo currentLighting = times[currentLightingIndex];

			float shadowStrength;
			float cloudIntensity;

			if (blendLighting == null)
			{
				sunLight.color = currentLighting.colors[(int) ELightingColor.SUN];
				sunLight.intensity = currentLighting.singles[(int) ELightingSingle.INTENSITY];
				shadowStrength = currentLighting.singles[(int) ELightingSingle.SHADOWS];

				setSeaColor("_BaseColor", currentLighting.colors[(int) ELightingColor.SEA]);
				setSeaColor("_ReflectionColor", currentLighting.colors[(int) ELightingColor.SEA]);

				RenderSettings.ambientSkyColor = currentLighting.colors[(int) ELightingColor.AMBIENT_SKY];
				RenderSettings.ambientEquatorColor = currentLighting.colors[(int) ELightingColor.AMBIENT_EQUATOR];
				RenderSettings.ambientGroundColor = currentLighting.colors[(int) ELightingColor.AMBIENT_GROUND];

				skyboxSky = currentLighting.colors[(int) ELightingColor.SKY_SKY];
				skyboxEquator = currentLighting.colors[(int) ELightingColor.SKY_EQUATOR];
				skyboxGround = currentLighting.colors[(int) ELightingColor.SKY_GROUND];
				cloudRimColor = currentLighting.colors[(int) ELightingColor.CLOUDS];
				particleLightingColor = currentLighting.colors[(int) ELightingColor.PARTICLE_LIGHTING];

				raysColor = currentLighting.colors[(int) ELightingColor.RAYS];
				raysIntensity = currentLighting.singles[(int) ELightingSingle.RAYS] * 4.0f;

				levelFogColor = currentLighting.colors[(int) ELightingColor.FOG];
				levelFogIntensity = currentLighting.singles[(int) ELightingSingle.FOG];

				cloudIntensity = currentLighting.singles[(int) ELightingSingle.CLOUDS];
			}
			else
			{
				sunLight.color = Color.Lerp(blendLighting.colors[(int) ELightingColor.SUN], currentLighting.colors[(int) ELightingColor.SUN], blend);
				sunLight.intensity = Mathf.Lerp(blendLighting.singles[(int) ELightingSingle.INTENSITY], currentLighting.singles[(int) ELightingSingle.INTENSITY], blend);
				shadowStrength = Mathf.Lerp(blendLighting.singles[(int) ELightingSingle.SHADOWS], currentLighting.singles[(int) ELightingSingle.SHADOWS], blend);

				setSeaColor("_BaseColor", Color.Lerp(blendLighting.colors[(int) ELightingColor.SEA], currentLighting.colors[(int) ELightingColor.SEA], blend));
				setSeaColor("_ReflectionColor", Color.Lerp(blendLighting.colors[(int) ELightingColor.SEA], currentLighting.colors[(int) ELightingColor.SEA], blend));

				RenderSettings.ambientSkyColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.AMBIENT_SKY], currentLighting.colors[(int) ELightingColor.AMBIENT_SKY], blend);
				RenderSettings.ambientEquatorColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.AMBIENT_EQUATOR], currentLighting.colors[(int) ELightingColor.AMBIENT_EQUATOR], blend);
				RenderSettings.ambientGroundColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.AMBIENT_GROUND], currentLighting.colors[(int) ELightingColor.AMBIENT_GROUND], blend);

				skyboxSky = Color.Lerp(blendLighting.colors[(int) ELightingColor.SKY_SKY], currentLighting.colors[(int) ELightingColor.SKY_SKY], blend);
				skyboxEquator = Color.Lerp(blendLighting.colors[(int) ELightingColor.SKY_EQUATOR], currentLighting.colors[(int) ELightingColor.SKY_EQUATOR], blend);
				skyboxGround = Color.Lerp(blendLighting.colors[(int) ELightingColor.SKY_GROUND], currentLighting.colors[(int) ELightingColor.SKY_GROUND], blend);
				cloudRimColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.CLOUDS], currentLighting.colors[(int) ELightingColor.CLOUDS], blend);
				particleLightingColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.PARTICLE_LIGHTING], currentLighting.colors[(int) ELightingColor.PARTICLE_LIGHTING], blend);

				raysColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.RAYS], currentLighting.colors[(int) ELightingColor.RAYS], blend);
				raysIntensity = Mathf.Lerp(blendLighting.singles[(int) ELightingSingle.RAYS], currentLighting.singles[(int) ELightingSingle.RAYS], blend) * 4.0f;

				levelFogColor = Color.Lerp(blendLighting.colors[(int) ELightingColor.FOG], currentLighting.colors[(int) ELightingColor.FOG], blend);
				levelFogIntensity = Mathf.Lerp(blendLighting.singles[(int) ELightingSingle.FOG], currentLighting.singles[(int) ELightingSingle.FOG], blend);

				cloudIntensity = Mathf.Lerp(blendLighting.singles[(int) ELightingSingle.CLOUDS], currentLighting.singles[(int) ELightingSingle.CLOUDS], blend);
			}

			cloudColor = cloudRimColor;

			levelAtmosphericFog = 0.0f;
			float shadowStrengthMultiplier = 1.0f;
			float brightnessMultiplier = 1.0f;
			foreach (CustomWeatherInstance instance in customWeatherInstances)
			{
				instance.component.UpdateLightingTime(blendLightingIndex, currentLightingIndex, blend);

				if (instance.component.overrideFog)
				{
					float fogLerpAlpha = Mathf.Pow(instance.component.EffectBlendAlpha, instance.component.fogBlendExponent);

					levelFogColor = Color.Lerp(levelFogColor, instance.component.fogColor, fogLerpAlpha);
					levelFogIntensity = Mathf.Lerp(levelFogIntensity, instance.component.fogDensity, fogLerpAlpha);

					if (instance.component.overrideAtmosphericFog)
					{
						// In the future weather should probably override sky color instead. Sky fog less than 1 looks bad.
						levelAtmosphericFog = Mathf.Lerp(levelAtmosphericFog, 1.0f, fogLerpAlpha);
					}
				}

				if (instance.component.overrideCloudColors)
				{
					float cloudLerpAlpha = Mathf.Pow(instance.component.EffectBlendAlpha, instance.component.cloudBlendExponent);
					cloudColor = Color.Lerp(cloudColor, instance.component.cloudColor, cloudLerpAlpha);
					cloudRimColor = Color.Lerp(cloudRimColor, instance.component.cloudRimColor, cloudLerpAlpha);
				}

				shadowStrengthMultiplier = Mathf.Lerp(shadowStrengthMultiplier, instance.component.shadowStrengthMultiplier, instance.component.EffectBlendAlpha);
				brightnessMultiplier = Mathf.Lerp(brightnessMultiplier, instance.component.brightnessMultiplier, instance.component.EffectBlendAlpha);
			}

			if (localBlendingFog)
			{
				levelFogColor = Color.Lerp(levelFogColor, localFogColor, localFogBlend);
				levelFogIntensity = Mathf.Lerp(levelFogIntensity, localFogIntensity, localFogBlend);
				levelAtmosphericFog = Mathf.Lerp(levelAtmosphericFog, localAtmosphericFog, localFogBlend);
			}

			sunLight.shadowStrength = shadowStrength * shadowStrengthMultiplier;

			if (brightnessMultiplier != 1.0f)
			{
				// Not ideal... :S

				setSeaColor("_Foam", getSeaColor("_Foam") * brightnessMultiplier);
				setSeaFloat("_Shininess", getSeaFloat("_Shininess") * brightnessMultiplier);

				setSeaColor("_BaseColor", getSeaColor("_BaseColor") * brightnessMultiplier);
				setSeaColor("_ReflectionColor", getSeaColor("_ReflectionColor") * brightnessMultiplier);

				sunLight.intensity *= brightnessMultiplier;

				RenderSettings.ambientSkyColor *= brightnessMultiplier;
				RenderSettings.ambientEquatorColor *= brightnessMultiplier;
				RenderSettings.ambientGroundColor *= brightnessMultiplier;

				skyboxSky *= brightnessMultiplier;
				skyboxEquator *= brightnessMultiplier;
				skyboxGround *= brightnessMultiplier;
				particleLightingColor *= brightnessMultiplier;
			}

			if (localBlendingLight)
			{
				setSeaColor("_Foam", Color.Lerp(getSeaColor("_Foam"), Color.black, localLightingBlend * PITCH_DARK_WATER_BLEND));
				setSeaFloat("_Shininess", Mathf.Lerp(getSeaFloat("_Shininess"), 0.0f, localLightingBlend * PITCH_DARK_WATER_BLEND));

				setSeaColor("_BaseColor", Color.Lerp(getSeaColor("_BaseColor"), Color.black, localLightingBlend * PITCH_DARK_WATER_BLEND));
				setSeaColor("_ReflectionColor", Color.Lerp(getSeaColor("_ReflectionColor"), Color.black, localLightingBlend * PITCH_DARK_WATER_BLEND));

				sunLight.color = Color.Lerp(sunLight.color, Color.black, localLightingBlend);
				sunLight.intensity = Mathf.Lerp(sunLight.intensity, 0.0f, localLightingBlend);
				sunLight.shadowStrength = Mathf.Lerp(sunLight.shadowStrength, 0.0f, localLightingBlend);

				RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend);
				RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, Color.black, localLightingBlend);
				RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, Color.black, localLightingBlend);
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

				skyboxSky = Color.Lerp(skyboxSky, Color.black, localLightingBlend);
				skyboxEquator = Color.Lerp(skyboxEquator, Color.black, localLightingBlend);
				skyboxGround = Color.Lerp(skyboxGround, Color.black, localLightingBlend);
				cloudRimColor = Color.Lerp(cloudRimColor, Color.black, localLightingBlend);
				particleLightingColor = Color.Lerp(particleLightingColor, Color.black, localLightingBlend);
			}

			setSeaColor("_SpecularColor", sunLight.color);

			if (vision == ELightingVision.MILITARY || vision == ELightingVision.CIVILIAN)
			{
				setSeaColor("_BaseColor", nightvisionColor);
				setSeaColor("_ReflectionColor", nightvisionColor);

				RenderSettings.ambientSkyColor = nightvisionColor;
				RenderSettings.ambientEquatorColor = nightvisionColor;
				RenderSettings.ambientGroundColor = nightvisionColor;
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

				skyboxSky = nightvisionColor;
				skyboxEquator = nightvisionColor;
				skyboxGround = nightvisionColor;
				cloudRimColor = nightvisionColor;

				levelFogColor = nightvisionColor;
				levelFogIntensity = Mathf.Max(levelFogIntensity, nightvisionFogIntensity);

				if (localBlendingLight)
				{
					RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend / 2);
					RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend / 2);
					RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend / 2);

					skyboxSky = Color.Lerp(skyboxSky, Color.black, localLightingBlend / 2);
					skyboxEquator = Color.Lerp(skyboxEquator, Color.black, localLightingBlend / 2);
					skyboxGround = Color.Lerp(skyboxGround, Color.black, localLightingBlend / 2);
					cloudRimColor = Color.Lerp(cloudRimColor, Color.black, localLightingBlend / 2);
				}
			}

			UpdateSunShafts(sunFlare, raysColor);

			UnityEngine.Profiling.Profiler.BeginSample("UpdateSkyboxMaterial");
			// Refer to DecalRenderer for explanation of this hack. (item icon renderer ambient color)
			skybox.SetVector("_SkyHackAmbientEquator", RenderSettings.ambientEquatorColor.linear);
			skybox.SetVector("_SkyHackAmbientGround", RenderSettings.ambientGroundColor.linear);

			// This is a pretty good place to update because transform and color are updated above.
			skybox.SetVector("_SunDirection", sun.forward);
			skybox.SetColor("_SunColor", Color.Lerp(sunLight.color, Color.white, 0.5f));
			skybox.SetFloat("_StarsCutoff", starsCutoff);
			skybox.SetVector("_MoonDirection", -sun.forward);
			skybox.SetVector("_MoonLightDirection", moons[_moon].forward);
			skybox.SetColor("_CloudColor", cloudColor);
			skybox.SetColor("_CloudRimColor", cloudRimColor);
			skybox.SetFloat("_CloudIntensity", cloudIntensity);
			UnityEngine.Profiling.Profiler.EndSample(); // UpdateSkyboxMaterial

			if (cloudOverrideParticles != null)
			{
				foreach (CloudParticleSystemInstance ps in cloudOverrideParticles)
				{
					if (ps.particleSystem != null)
					{
						float effectiveScale = cloudIntensity * ps.config.RateOverTimeScale;

						// Nelson 2025-09-01: realized the value we need to set depends on the minmax curve mode,
						// e.g., rateOverTimeMultiplier seems to set constantMax rather than an overall multiplier.
						ParticleSystem.EmissionModule emission = ps.particleSystem.emission;
						switch (emission.rateOverTime.mode)
						{
							case ParticleSystemCurveMode.Constant:
							case ParticleSystemCurveMode.Curve:
							case ParticleSystemCurveMode.TwoCurves:
							{
								emission.rateOverTimeMultiplier = effectiveScale;
								break;
							}

							case ParticleSystemCurveMode.TwoConstants:
							{
								ParticleSystem.MinMaxCurve curve = emission.rateOverTime;
								curve.constantMin = ps.defaultEmissionRateMin * effectiveScale;
								curve.constantMax = ps.defaultEmissionRateMax * effectiveScale;
								emission.rateOverTime = curve;
								break;
							}
						}
					}

					if (ps.material != null)
					{
						foreach (CloudParticleSystemMaterialColor color in ps.materialColorProperties)
						{
							Color adjustedColor = cloudColor.WithAlpha(color.defaultColorAlpha);
							ps.material.SetColor(color.propertyId, adjustedColor);
						}
					}
				}

				if (cloudOverrideParticlesNeedRestart)
				{
					cloudOverrideParticlesNeedRestart = false;
					foreach (CloudParticleSystemInstance ps in cloudOverrideParticles)
					{
						if (ps.particleSystem != null)
						{
							ps.particleSystem.Simulate(ps.config.WarmupTime, /*withChildren*/ true, /*restart*/ true);
						}
					}
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		/// <summary>
		/// Nelson 2025-09-01: hacking this in to reset cloud particle systems when changing time
		/// in the level editor. Otherwise, it's hard to tell how the intensity affects them.
		/// </summary>
		public static void MarkParticleCloudsNeedRestart()
		{
			cloudOverrideParticlesNeedRestart = true;
		}

		private static void updateHolidayWeatherRestrictions()
		{
			// Holiday redirects are only enabled in play mode (we do not want to modify canX flags in editor).
			// For PEI right now we want to disable rain when snow redirects are enabled,
			// but this might be expanded in a future update.
			if (Level.shouldUseHolidayRedirects)
			{
				canRain = false;
				canSnow = false;
			}
		}

		public static void load(ushort size)
		{
			vision = ELightingVision.NONE;
			isSea = false;
			activeAmbianceVolumes.Clear();
			localBlendingLight = false;
			localLightingBlend = 1.0f;
			localLightingBlendTimeAlpha = 0.0f;
			localLightingBlendFadeOutDuration = null;
			localBlendingFog = false;
			localBlendingFogTimeAlpha = 0.0f;
			localBlendingFogFadeOutDuration = null;
			localFogBlend = 0.0f;

			auroraBorealisCurrentIntensity = 0;
			auroraBorealisTargetIntensity = 0;

			currentAudioVolume = 0;
			targetAudioVolume = 0;
			nextAudioVolumeChangeTime = -1;

			customWeatherInstances.Clear();
			activeCustomWeather = null;

			legacyWater = null;
			legacyWaterTransform = null;

			if (cloudOverrideParticles != null)
			{
				foreach (CloudParticleSystemInstance ps in cloudOverrideParticles)
				{
					if (ps.material != null)
					{
						Object.Destroy(ps.material);
					}
				}
				cloudOverrideParticles = null;
			}
			cloudOverrideParticlesNeedRestart = true;

			if (!Dedicator.IsDedicatedServer)
			{
				skybox = (Material) Material.Instantiate(Resources.Load("Level/Skybox"));
				RenderSettings.skybox = skybox;
				if (Level.info.configData.Is_Aurora_Borealis_Visible)
				{
					skybox.EnableKeyword("WITH_AURORA_BOREALIS");
				}

				LevelAsset levelAsset = Level.getAsset();
				if (!levelAsset?.hasClouds ?? false)
				{
					skybox.DisableKeyword("WITH_CLOUDS");
				}
				if (!Level.info.configData.Has_Atmosphere)
				{
					skybox.DisableKeyword("WITH_STARS");
				}

				lighting = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Lighting"))).transform;
				lighting.name = "Lighting";
				lighting.position = Vector3.zero;
				lighting.rotation = Quaternion.identity;
				lighting.parent = Level.level;

				if (levelAsset != null && levelAsset.CloudOverridePrefab.isValid)
				{
					GameObject cloudParticlePrefab = levelAsset.CloudOverridePrefab.loadAsset();
					if (cloudParticlePrefab != null)
					{
						GameObject cloudParticleInstance = Object.Instantiate(cloudParticlePrefab, Vector3.zero, Quaternion.identity, lighting);

						List<CloudParticleSystemInstance> temp = new List<CloudParticleSystemInstance>(levelAsset.CloudOverrideParticleSystemPaths.Length);
						foreach (LevelAsset.CloudOverrideParticleSystemsPath config in levelAsset.CloudOverrideParticleSystemPaths)
						{
							Transform child = cloudParticleInstance.transform.Find(config.ComponentPath);
							if (child == null)
							{
								levelAsset.ReportAssetError($"cloud override missing child at \"{config.ComponentPath}\"");
								continue;
							}

							ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
							if (particleSystem == null)
							{
								levelAsset.ReportAssetError($"cloud override missing ParticleSystem on \"{config.ComponentPath}\"");
								continue;
							}

							Material material = particleSystem.GetComponent<ParticleSystemRenderer>().material;

							CloudParticleSystemMaterialColor[] colors = new CloudParticleSystemMaterialColor[config.MaterialColorPropertyNames.Length];
							for (int propertyIndex = 0; propertyIndex < colors.Length; ++propertyIndex)
							{
								ref CloudParticleSystemMaterialColor color = ref colors[propertyIndex];
								string propertyName = config.MaterialColorPropertyNames[propertyIndex];
								color.propertyId = Shader.PropertyToID(propertyName);
								color.defaultColorAlpha = material.GetColor(color.propertyId).a;
							}

							ParticleSystem.MinMaxCurve emissionRateOverTime = particleSystem.emission.rateOverTime;

							CloudParticleSystemInstance instance = new CloudParticleSystemInstance()
							{
								config = config,
								particleSystem = particleSystem,
								defaultEmissionRateMin = emissionRateOverTime.constantMin,
								defaultEmissionRateMax = emissionRateOverTime.constantMax,
								material = material,
								materialColorProperties = colors,
							};
							temp.Add(instance);
						}
						if (temp.Count > 0)
						{
							cloudOverrideParticles = temp.ToArray();
						}
						else
						{
							levelAsset.ReportAssetError("cloud override particle system list is effectively empty");
						}
					}
				}

				sun = lighting.Find("Sun");
				sunLight = sun.GetComponent<Light>();
				if (GraphicsSettings.WantsCinematicMode)
				{
					sunLight.shadowCustomResolution = SystemInfo.maxTextureSize;
				}
				sunFlare = sun.Find("Flare_Sun");

				_bubbles = lighting.Find("Bubbles");
				UpdateBubblesActive();

				_windZone = lighting.Find("WindZone").GetComponent<WindZone>();

				reflectionCamera = lighting.Find("Reflection").GetComponent<Camera>();

				if (reflectionMap == null)
				{
					reflectionMap = new RenderTexture(32, 32, 0);
					reflectionMap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
				}

				if (reflectionMapVision == null)
				{
					reflectionMapVision = new RenderTexture(32, 32, 0);
					reflectionMapVision.dimension = UnityEngine.Rendering.TextureDimension.Cube;
				}

				RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;

				reflectionIndex = 0;
				reflectionIndexVision = 0;
				isReflectionBuilding = false;
				isReflectionBuildingVision = false;

				puddles = lighting.GetComponent<Rain>();

				moons = new Transform[MOON_CYCLES];
				for (int index = 0; index < moons.Length; index++)
				{
					moons[index] = sun.Find("MoonLightDirection_" + index);
				}

				ambianceAudioGameObject = lighting.Find("Effect").gameObject;
				activeAmbianceAudioInstances = new List<AmbianceAudioInstance>();
				ambianceAudioPool = new Stack<AmbianceAudioInstance>();

				_dayAudio = lighting.Find("Day").GetComponent<AudioSource>();
				_nightAudio = lighting.Find("Night").GetComponent<AudioSource>();
				_waterAudio = lighting.Find("Water").GetComponent<AudioSource>();
				_windAudio = lighting.Find("Wind").GetComponent<AudioSource>();
				_belowAudio = lighting.Find("Below").GetComponent<AudioSource>();

				if (ReadWrite.fileExists(Level.info.path + "/Environment/Ambience.unity3d", false, false))
				{
					try
					{
						Bundle audio = Bundles.getBundle(Level.info.path + "/Environment/Ambience.unity3d", false);

						dayAudio.clip = audio.load<AudioClip>("Day");
						nightAudio.clip = audio.load<AudioClip>("Night");
						waterAudio.clip = audio.load<AudioClip>("Water");
						windAudio.clip = audio.load<AudioClip>("Wind");
						belowAudio.clip = audio.load<AudioClip>("Below");

						audio.unload();
					}
					catch (System.Exception e)
					{
						UnturnedLog.exception(e, "Exception loading ambient audio:");
					}
				}
			}

			// Important to load this to hash the lighting config.
			if (ReadWrite.fileExists(Level.info.path + "/Environment/Lighting.dat", false, false))
			{
				Block block = ReadWrite.readBlock(Level.info.path + "/Environment/Lighting.dat", false, false, 0);
				byte version = block.readByte();

				_azimuth = block.readSingle();
				_bias = block.readSingle();
				_fade = block.readSingle();
				_time = block.readSingle();
				moon = block.readByte();

				if (version >= 5)
				{
					_seaLevel = block.readSingle();
					_snowLevel = block.readSingle();

					if (version > 6)
					{
						canRain = block.readBoolean();
					}
					else
					{
						canRain = false;
					}

					if (version > 10)
					{
						canSnow = block.readBoolean();
					}
					else
					{
						canSnow = false;
					}

					if (version < 8)
					{
						rainFreq = 1.0f;
						rainDur = 1.0f;
					}
					else
					{
						rainFreq = block.readSingle();
						rainDur = block.readSingle();
					}

					if (version < 11)
					{
						snowFreq = 1.0f;
						snowDur = 1.0f;
					}
					else
					{
						snowFreq = block.readSingle();
						snowDur = block.readSingle();
					}

					_times = new LightingInfo[4];
					for (int index = 0; index < times.Length; index++)
					{
						// If changing the length of these arrays remember to update older version upgrades.
						Color[] colors = new Color[12];
						float[] singles = new float[5];

						if (version > 9)
						{
							for (int color = 0; color < colors.Length; color++)
							{
								colors[color] = block.readColor();
							}

							for (int single = 0; single < singles.Length; single++)
							{
								singles[single] = block.readSingle();
							}
						}
						else if (version > 8)
						{
							for (int color = 0; color < colors.Length - 1; color++)
							{
								colors[color] = block.readColor();
							}
							colors[(int) ELightingColor.PARTICLE_LIGHTING] = colors[(int) ELightingColor.SKY_SKY];

							for (int single = 0; single < singles.Length; single++)
							{
								singles[single] = block.readSingle();
							}
						}
						else
						{
							if (version >= 6)
							{
								for (int color = 0; color < colors.Length - 2; color++)
								{
									colors[color] = block.readColor();
								}
							}
							else
							{
								for (int color = 0; color < colors.Length - 3; color++)
								{
									colors[color] = block.readColor();
								}

								colors[9] = colors[(int) ELightingColor.FOG];
							}

							colors[(int) ELightingColor.RAYS] = colors[(int) ELightingColor.SUN];
							colors[(int) ELightingColor.PARTICLE_LIGHTING] = colors[(int) ELightingColor.SKY_SKY];

							for (int single = 0; single < singles.Length - 1; single++)
							{
								singles[single] = block.readSingle();
							}

							singles[(int) ELightingSingle.RAYS] = 0.25f;
						}

						if (version < 12)
						{
							// Switched from height fog to distance fog, so we reduce the intensity on all maps.
							singles[(int) ELightingSingle.FOG] = Mathf.Min(singles[(int) ELightingSingle.FOG], 0.33f);
						}

						LightingInfo lightingInfo = new LightingInfo(colors, singles);
						times[index] = lightingInfo;
					}
				}
				else
				{
					_times = new LightingInfo[4];
					for (int index = 0; index < times.Length; index++)
					{
						Color[] colors = new Color[12];
						float[] singles = new float[5];

						LightingInfo lightingInfo = new LightingInfo(colors, singles);
						times[index] = lightingInfo;
					}

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SKY_SKY] = block.readColor();
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SKY_SKY] = block.readColor();
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SKY_SKY] = block.readColor();
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SKY_SKY] = block.readColor();

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SKY_EQUATOR] = times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SKY_SKY];
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SKY_EQUATOR] = times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SKY_SKY];
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SKY_EQUATOR] = times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SKY_SKY];
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SKY_EQUATOR] = times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SKY_SKY];

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SKY_GROUND] = times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SKY_SKY];
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SKY_GROUND] = times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SKY_SKY];
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SKY_GROUND] = times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SKY_SKY];
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SKY_GROUND] = times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SKY_SKY];

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.AMBIENT_SKY] = block.readColor();
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.AMBIENT_SKY] = block.readColor();
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.AMBIENT_SKY] = block.readColor();
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.AMBIENT_SKY] = block.readColor();

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.AMBIENT_EQUATOR] = times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.AMBIENT_SKY];
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.AMBIENT_EQUATOR] = times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.AMBIENT_SKY];
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.AMBIENT_EQUATOR] = times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.AMBIENT_SKY];
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.AMBIENT_EQUATOR] = times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.AMBIENT_SKY];

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.AMBIENT_GROUND] = times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.AMBIENT_SKY];
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.AMBIENT_GROUND] = times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.AMBIENT_SKY];
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.AMBIENT_GROUND] = times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.AMBIENT_SKY];
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.AMBIENT_GROUND] = times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.AMBIENT_SKY];

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.FOG] = block.readColor();
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.FOG] = block.readColor();
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.FOG] = block.readColor();
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.FOG] = block.readColor();

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SUN] = block.readColor();
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SUN] = block.readColor();
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SUN] = block.readColor();
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SUN] = block.readColor();

					times[(int) ELightingTime.DAWN].singles[(int) ELightingSingle.INTENSITY] = block.readSingle();
					times[(int) ELightingTime.MIDDAY].singles[(int) ELightingSingle.INTENSITY] = block.readSingle();
					times[(int) ELightingTime.DUSK].singles[(int) ELightingSingle.INTENSITY] = block.readSingle();
					times[(int) ELightingTime.MIDNIGHT].singles[(int) ELightingSingle.INTENSITY] = block.readSingle();

					times[(int) ELightingTime.DAWN].singles[(int) ELightingSingle.FOG] = block.readSingle();
					times[(int) ELightingTime.MIDDAY].singles[(int) ELightingSingle.FOG] = block.readSingle();
					times[(int) ELightingTime.DUSK].singles[(int) ELightingSingle.FOG] = block.readSingle();
					times[(int) ELightingTime.MIDNIGHT].singles[(int) ELightingSingle.FOG] = block.readSingle();

					times[(int) ELightingTime.DAWN].singles[(int) ELightingSingle.CLOUDS] = block.readSingle();
					times[(int) ELightingTime.MIDDAY].singles[(int) ELightingSingle.CLOUDS] = block.readSingle();
					times[(int) ELightingTime.DUSK].singles[(int) ELightingSingle.CLOUDS] = block.readSingle();
					times[(int) ELightingTime.MIDNIGHT].singles[(int) ELightingSingle.CLOUDS] = block.readSingle();

					times[(int) ELightingTime.DAWN].singles[(int) ELightingSingle.SHADOWS] = block.readSingle();
					times[(int) ELightingTime.MIDDAY].singles[(int) ELightingSingle.SHADOWS] = block.readSingle();
					times[(int) ELightingTime.DUSK].singles[(int) ELightingSingle.SHADOWS] = block.readSingle();
					times[(int) ELightingTime.MIDNIGHT].singles[(int) ELightingSingle.SHADOWS] = block.readSingle();

					if (version > 2)
					{
						_seaLevel = block.readSingle();
					}
					else
					{
						_seaLevel = block.readSingle() / 2f;
					}

					if (version > 1)
					{
						_snowLevel = block.readSingle();
					}
					else
					{
						_snowLevel = 0f;
					}

					canRain = false;
					canSnow = false;

					times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SEA] = block.readColor();
					times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SEA] = block.readColor();
					times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SEA] = block.readColor();
					times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SEA] = block.readColor();
				}

				hash = block.getHash();
			}
			else
			{
				_azimuth = 0.2f;
				_bias = 0.5f;
				_fade = 1;
				_time = bias / 2f;
				moon = 0;

				_seaLevel = 1f;
				_snowLevel = 0f;

				canRain = true;
				canSnow = false;

				rainFreq = 1.0f;
				rainDur = 1.0f;
				snowFreq = 1.0f;
				snowDur = 1.0f;

				_times = new LightingInfo[4];
				for (int index = 0; index < times.Length; index++)
				{
					Color[] colors = new Color[12];
					float[] singles = new float[5];

					LightingInfo lightingInfo = new LightingInfo(colors, singles);
					times[index] = lightingInfo;
				}

				hash = new byte[20];
			}

			if (bias < 1 - bias) // morning shorter than evening
			{
				_transition = bias / 2 * fade;
			}
			else
			{
				_transition = (1 - bias) / 2 * fade;
			}

			times[(int) ELightingTime.DAWN].colors[(int) ELightingColor.SEA].a = 0.25f;
			times[(int) ELightingTime.MIDDAY].colors[(int) ELightingColor.SEA].a = 0.5f;
			times[(int) ELightingTime.DUSK].colors[(int) ELightingColor.SEA].a = 0.75f;
			times[(int) ELightingTime.MIDNIGHT].colors[(int) ELightingColor.SEA].a = 0.9f;

			if (Level.info.configData.Use_Legacy_Water)
			{
				GameObject legacyWaterGameObject = new GameObject(); // WaterVolume renames itself to Water_Volume
				legacyWaterTransform = legacyWaterGameObject.transform;
				legacyWaterTransform.parent = Level.level;
				legacyWater = legacyWaterGameObject.AddComponent<WaterVolume>();
				legacyWater.isManagedByLighting = true;
				legacyWater.isSeaLevel = true;
				legacyWater.isSurfaceVisible = true;
				legacyWater.isReflectionVisible = true;
				UpdateLegacyWaterTransform();
			}

			init = false;

			// Call after loading canRain and canSnow.
			updateHolidayWeatherRestrictions();
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION);

			block.writeSingle(azimuth);
			block.writeSingle(bias);
			block.writeSingle(fade);
			block.writeSingle(time);
			block.writeByte(moon);

			block.writeSingle(seaLevel);
			block.writeSingle(snowLevel);
			block.writeBoolean(canRain);
			block.writeBoolean(canSnow);

			block.writeSingle(rainFreq);
			block.writeSingle(rainDur);
			block.writeSingle(snowFreq);
			block.writeSingle(snowDur);

			for (int index = 0; index < times.Length; index++)
			{
				LightingInfo lightingInfo = times[index];

				for (int color = 0; color < lightingInfo.colors.Length; color++)
				{
					block.writeColor(lightingInfo.colors[color]);
				}

				for (int single = 0; single < lightingInfo.singles.Length; single++)
				{
					block.writeSingle(lightingInfo.singles[single]);
				}
			}

			ReadWrite.writeBlock(Level.info.path + "/Environment/Lighting.dat", false, false, block);
		}

		private static bool init;

		private static void UpdateLegacyWaterTransform()
		{
			if (legacyWater == null)
			{
				// Depends whether Level.info.configData.Use_Legacy_Water is true
				return;
			}

			if (seaLevel < 0.99f)
			{
				// Volume starts from -1,024m to match legacy infinite water behaviour.
				float waterSize = Level.size * 2.0f;
				float waterDepth = seaLevel * Level.TERRAIN;
				legacyWaterTransform.position = new Vector3(0.0f, -512.0f + (waterDepth * 0.5f), 0.0f);
				legacyWaterTransform.localScale = new Vector3(waterSize, 1024.0f + waterDepth, waterSize);
				legacyWater.gameObject.SetActive(true);
			}
			else
			{
				legacyWater.gameObject.SetActive(false);
			}
		}

		private static void UpdateBubblesActive()
		{
			bool shouldActivateBubbles;
			if (Level.info.configData.Use_Legacy_Water)
			{
				shouldActivateBubbles = seaLevel < 0.99f;
			}
			else
			{
				shouldActivateBubbles = true;
			}

			if (!Level.info.configData.Use_Vanilla_Bubbles)
			{
				shouldActivateBubbles = false;
			}

			bubbles.gameObject.SetActive(shouldActivateBubbles);
			if (shouldActivateBubbles)
			{
				bubbles.GetComponent<ParticleSystem>().Play();
			}
		}

		private static Vector3 localPoint;
		private static float localWindOverride;
		private static List<VolumeAlphaPair<AmbianceVolume>> activeAmbianceVolumes = new List<VolumeAlphaPair<AmbianceVolume>>();
		private static bool localBlendingLight;
		private static float localLightingBlend;
		private static float localLightingBlendTimeAlpha;
		private static float? localLightingBlendFadeOutDuration;
		private static bool localBlendingFog;
		private static float localBlendingFogTimeAlpha;
		private static float? localBlendingFogFadeOutDuration;
		private static float localFogBlend;
		private static Color localFogColor;
		private static float localFogIntensity;
		private static float localAtmosphericFog;

		private static int tickedWeatherBlendingFrame;
		/// <summary>
		/// Ticked on dedicated server as well as client so that server can listen for weather events.
		/// </summary>
		/// <param name="localVolumeMask">On dedicated server this is always 0xFFFFFFFF.</param>
		public static void tickCustomWeatherBlending(uint localVolumeMask)
		{
			int frame = Time.frameCount;
			if (frame == tickedWeatherBlendingFrame)
			{
				return;
			}
			tickedWeatherBlendingFrame = frame;
			float deltaTime = Time.deltaTime;

			const float invokeBlendAlphaChangedThreshold = 0.01f; // 100 rather than every frame.

			for (int index = customWeatherInstances.Count - 1; index >= 0; --index)
			{
				CustomWeatherInstance instance = customWeatherInstances[index];
				bool matchesMask = (instance.asset.volumeMask & localVolumeMask) != 0;

				if (!instance.component.hasTickedBlending)
				{
					instance.component.hasTickedBlending = true;
					// Initialize with global alpha so loaded weather instantly applies.
					instance.component.localVolumeBlendAlpha = matchesMask ? instance.component.globalBlendAlpha : 0.0f;
				}

				if (matchesMask && instance.component.isWeatherActive)
				{
					instance.component.localVolumeBlendAlpha = Mathf.Min(1.0f,
						instance.component.localVolumeBlendAlpha + (deltaTime / Mathf.Max(0.1f, instance.asset.fadeInDuration)));
				}
				else
				{
					instance.component.localVolumeBlendAlpha = Mathf.Max(0.0f,
						instance.component.localVolumeBlendAlpha - (deltaTime / Mathf.Max(0.1f, instance.asset.fadeOutDuration)));
				}

				if (instance.component.isWeatherActive)
				{
					instance.component.globalBlendAlpha += deltaTime / Mathf.Max(0.1f, instance.asset.fadeInDuration);
					if (instance.component.globalBlendAlpha >= 1.0f)
					{
						instance.component.globalBlendAlpha = 1.0f;
						if (!instance.component.isFullyTransitionedIn)
						{
							instance.component.isFullyTransitionedIn = true;
							WeatherEventListenerManager.InvokeEndTransitionIn(instance.asset.GUID);
							WeatherEventListenerManager.InvokeStatusChange(instance.component.asset, EWeatherStatusChange.EndTransitionIn);
							instance.component.OnEndTransitionIn();

							instance.eventBlendAlpha = 1.0f;
							WeatherEventListenerManager.InvokeBlendAlphaChanged(instance.component.asset, 1.0f);
						}
					}
					else
					{
						if (instance.component.globalBlendAlpha - instance.eventBlendAlpha >= invokeBlendAlphaChangedThreshold)
						{
							instance.eventBlendAlpha = instance.component.globalBlendAlpha;
							WeatherEventListenerManager.InvokeBlendAlphaChanged(instance.asset, instance.component.globalBlendAlpha);
						}
					}
				}
				else
				{
					instance.component.isFullyTransitionedIn = false;
					instance.component.globalBlendAlpha -= deltaTime / Mathf.Max(0.1f, instance.asset.fadeOutDuration);
					if (instance.component.globalBlendAlpha <= 0.0f)
					{
						instance.component.globalBlendAlpha = 0.0f;
						WeatherEventListenerManager.InvokeEndTransitionOut(instance.component.asset.GUID);
						WeatherEventListenerManager.InvokeStatusChange(instance.component.asset, EWeatherStatusChange.EndTransitionOut);
						instance.component.OnEndTransitionOut();

						instance.eventBlendAlpha = 0.0f;
						WeatherEventListenerManager.InvokeBlendAlphaChanged(instance.component.asset, 0.0f);

						instance.teardown();
						customWeatherInstances.RemoveAtFast(index);
					}
					else
					{
						if (instance.eventBlendAlpha - instance.component.globalBlendAlpha >= invokeBlendAlphaChangedThreshold)
						{
							instance.eventBlendAlpha = instance.component.globalBlendAlpha;
							WeatherEventListenerManager.InvokeBlendAlphaChanged(instance.asset, instance.component.globalBlendAlpha);
						}
					}
				}
			}
		}

		public static void ForceRefreshForLatestViewer()
		{
			UpdateForViewer(localPoint, localWindOverride, Time.deltaTime);
		}

		public static void UpdateForViewer(Vector3 point, float windOverride, float deltaTime)
		{
			localPoint = point;
			localWindOverride = windOverride;

			AmbianceVolumeManager.Get().GetOverlappingVolumesWithAlpha(point, activeAmbianceVolumes);

			if (activeAmbianceVolumes.Count > 1)
			{
				// Sort *highest* priority ambiance volumes to front of list.
				activeAmbianceVolumes.Sort(ambianceVolumeComparison);
			}

			UpdateAmbianceAudio(deltaTime, out float maxAmbianceAudioVolume);

			if (!Level.isEditor || _editorWantsNoLightingPreview)
			{
				bool isAnyNonDistanceVolumeOverlapping = false;

				float? minFadeInDuration = null;
				// We do not reset minFadeOutDuration here because we want it to retain value when leaving volume.

				float maxDistanceAlpha = 0.0f;
				foreach (VolumeAlphaPair<AmbianceVolume> active in activeAmbianceVolumes)
				{
					if (!active.volume.noLighting)
					{
						continue;
					}

					if (active.volume.enableFalloff)
					{
						maxDistanceAlpha = Mathf.Max(maxDistanceAlpha, active.alpha);
					}
					else
					{
						isAnyNonDistanceVolumeOverlapping = true;

						if (minFadeInDuration.HasValue)
						{
							minFadeInDuration = Mathf.Max(0.0001f, Mathf.Min(minFadeInDuration.Value,
								active.volume.lightingFadeInDuration));
						}
						else
						{
							minFadeInDuration = Mathf.Max(0.0001f, active.volume.lightingFadeInDuration);
						}

						if (localLightingBlendFadeOutDuration.HasValue)
						{
							localLightingBlendFadeOutDuration = Mathf.Max(0.0001f, Mathf.Min(localLightingBlendFadeOutDuration.Value,
								active.volume.lightingFadeOutDuration));
						}
						else
						{
							localLightingBlendFadeOutDuration = Mathf.Max(0.0001f, active.volume.lightingFadeOutDuration);
						}
					}
				}

				float duration = isAnyNonDistanceVolumeOverlapping
					? (minFadeInDuration ?? 4.0f)
					: (localLightingBlendFadeOutDuration ?? 4.0f);
				float maxDelta = deltaTime / duration;
				float targetTimeAlpha = isAnyNonDistanceVolumeOverlapping ? 1.0f : 0.0f;
				localLightingBlendTimeAlpha = Mathf.MoveTowards(localLightingBlendTimeAlpha, targetTimeAlpha, maxDelta);
				localLightingBlend = Mathf.Max(localLightingBlendTimeAlpha, maxDistanceAlpha);
				localBlendingLight = localLightingBlend > 0.001f;
				if (!localBlendingLight)
				{
					localLightingBlendFadeOutDuration = null;
				}
			}
			else
			{
				localBlendingLight = false;
			}

			UpdateFogBlend(deltaTime);

			// todo: support falloff
			uint volumeBlendMask;
			AmbianceVolume weatherVolume = null;
			if (activeAmbianceVolumes.Count > 0)
			{
				// Ambiance volume with highest priority is at front of list.
				weatherVolume = activeAmbianceVolumes[0].volume;
			}
			if (weatherVolume != null)
			{
				volumeBlendMask = weatherVolume.weatherMask;
			}
			else
			{
				LevelAsset levelAsset = Level.getAsset();
				volumeBlendMask = levelAsset != null ? levelAsset.globalWeatherMask : uint.MaxValue;
			}

			if (Level.info != null && Level.info.configData != null)
			{
				if (!Level.info.configData.Use_Rain_Volumes)
				{
					// Default to raining everywhere.
					volumeBlendMask |= 1U << 0;
				}

				if (!Level.info.configData.Use_Snow_Volumes)
				{
					// Default to snowing everywhere.
					volumeBlendMask |= 1U << 1;
				}

				if (Level.info.configData.Use_Legacy_Snow_Height)
				{
					// Override toggling snow according to old rules.
					if (isPositionSnowy(point))
					{
						volumeBlendMask |= 1U << 1;
					}
					else
					{
						volumeBlendMask &= ~(1U << 1);
					}
				}
			}

			tickCustomWeatherBlending(volumeBlendMask);

			UnityEngine.Profiling.Profiler.BeginSample("Init");

			if (!init)
			{
				init = true;

				resetCachedValues();
				updateLighting();

				bubbles.GetComponent<ParticleSystem>().Play();

				if (dayAudio.clip != null)
				{
					dayAudio.Play();
				}
				if (nightAudio.clip != null)
				{
					nightAudio.Play();
				}
				if (waterAudio.clip != null)
				{
					waterAudio.Play();
				}
				if (windAudio.clip != null)
				{
					windAudio.Play();
				}
				if (belowAudio.clip != null)
				{
					belowAudio.Play();
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Reposition");

			lighting.position = point;

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Colors");

			setSkyColor(skyboxSky);
			setEquatorColor(skyboxEquator);
			setGroundColor(skyboxGround);

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Water");

			UnityEngine.Profiling.Profiler.BeginSample("getWaterSurfaceElevation");
			float surfaceLevel = SDG.Framework.Water.WaterUtility.getWaterSurfaceElevation(point);
			UnityEngine.Profiling.Profiler.EndSample();
			if (!enableUnderwaterEffects)
			{
				surfaceLevel = -1024;
			}

			if (enableUnderwaterEffects && SDG.Framework.Water.WaterUtility.isPointUnderwater(point))
			{
				waterAudio.volume = 0;
				belowAudio.volume = 1;

				isSea = true;
			}
			else
			{
				bool doesAnyAmbianceVolumeSetNoWater = false;
				foreach (VolumeAlphaPair<AmbianceVolume> active in activeAmbianceVolumes)
				{
					doesAnyAmbianceVolumeSetNoWater |= active.volume.noWater;
				}

				if (point.y < surfaceLevel + 8f && !doesAnyAmbianceVolumeSetNoWater)
				{
					waterAudio.volume = Mathf.Lerp(0, 0.25f, 1f - ((point.y - surfaceLevel) / 8f));
					belowAudio.volume = 0;
				}
				else
				{
					waterAudio.volume = 0;
					belowAudio.volume = 0;
				}

				isSea = false;
			}

			UnityEngine.Profiling.Profiler.EndSample();

			if (isSea)
			{
				RenderSettings.fogColor = getSeaColor("_BaseColor");
				RenderSettings.fogDensity = Level.getAsset()?.UnderwaterFogDensity ?? LevelAsset.DEFAULT_UNDERWATER_FOG_DENSITY;
				setAtmosphericFog(1.0f);
			}
			else
			{
				RenderSettings.fogColor = levelFogColor;
				RenderSettings.fogDensity = Mathf.Pow(levelFogIntensity, 3) * 0.025f;
				setAtmosphericFog(levelAtmosphericFog);
			}

			auroraBorealisCurrentIntensity = Mathf.Clamp01(Mathf.Lerp(auroraBorealisCurrentIntensity, auroraBorealisTargetIntensity, 0.1f * deltaTime));
			skybox.SetFloat("_AuroraBorealisIntensity", auroraBorealisCurrentIntensity);

			setAlphaParticleLightingColor(particleLightingColor);

			// Unfortunate to override the sky color so many times, but while underwater we change the sky color
			// to match the water color.
			if (isSea)
			{
				Color _waterColor = RenderSettings.fogColor;
				setSkyColor(_waterColor);
				setEquatorColor(_waterColor);
				setGroundColor(_waterColor);
			}

			if (puddles != null)
			{
				float maxWaterLevel = 0.0f;
				float maxIntensity = 0.0f;

				foreach (CustomWeatherInstance instance in customWeatherInstances)
				{
					maxWaterLevel = Mathf.Max(maxWaterLevel, instance.component.puddleWaterLevel * instance.component.EffectBlendAlpha);
					maxIntensity = Mathf.Max(maxIntensity, instance.component.puddleIntensity * instance.component.EffectBlendAlpha);
				}

				if (maxWaterLevel > puddles.Water_Level)
				{
					puddles.Water_Level = Mathf.Lerp(puddles.Water_Level, maxWaterLevel, 0.2f * deltaTime);
				}
				else
				{
					puddles.Water_Level = Mathf.Lerp(puddles.Water_Level, maxWaterLevel, 0.025f * deltaTime);
				}
				puddles.Intensity = maxIntensity;
			}

			UnityEngine.Profiling.Profiler.BeginSample("Audio");

			if (Time.time > nextAudioVolumeChangeTime)
			{
				nextAudioVolumeChangeTime = Time.time + Random.Range(15, 60);
				targetAudioVolume = Random.Range(AUDIO_MIN, AUDIO_MAX);
			}
			currentAudioVolume = Mathf.Lerp(currentAudioVolume, targetAudioVolume, 0.1f * deltaTime);

			float targetCustomWeatherVolume = 1.0f - maxAmbianceAudioVolume;
			bool isAnyCustomWeatherHigherPriorityThanVolumes = false;
			float maxCustomWeatherVolume = 0.0f;
			float maxWindMain = 0.15f;
			foreach (CustomWeatherInstance instance in customWeatherInstances)
			{
#if !DEDICATED_SERVER
				if (instance.component.ambientAudioSource != null)
				{
					float targetVolume = instance.component.EffectBlendAlpha;
					if (instance.asset.IsAudioHigherPriorityThanAmbianceVolumes)
					{
						isAnyCustomWeatherHigherPriorityThanVolumes = true;
					}
					else
					{
						targetVolume *= targetCustomWeatherVolume;
					}
					instance.component.ambientAudioSource.volume = Mathf.Lerp(instance.component.ambientAudioSource.volume, targetVolume, 0.5f * deltaTime);
					maxCustomWeatherVolume = Mathf.Max(maxCustomWeatherVolume, instance.component.ambientAudioSource.volume);
				}

				float targetWindMain = instance.component.windMain * instance.component.EffectBlendAlpha;
				maxWindMain = Mathf.Max(maxWindMain, targetWindMain);
#endif // !DEDICATED_SERVER

				instance.component.UpdateWeather();
			}
			// Fade out rain/day/night audio while custom weather is audible.
			float customWeatherVolumeMultiplier = 1.0f - maxCustomWeatherVolume;
#if !DEDICATED_SERVER
			if (isAnyCustomWeatherHigherPriorityThanVolumes)
			{
				foreach (AmbianceAudioInstance instance in activeAmbianceAudioInstances)
				{
					instance.audioSource.volume *= customWeatherVolumeMultiplier;
				}
			}
#endif // !DEDICATED_SERVER

			windAudio.volume = windOverride;
			dayAudio.volume = Mathf.Lerp(dayAudio.volume, dayVolume * currentAudioVolume * (1 - (waterAudio.volume * 4)) * (1 - belowAudio.volume) * (1 - windAudio.volume) * (1 - maxAmbianceAudioVolume) * customWeatherVolumeMultiplier, 0.5f * Time.deltaTime);
			nightAudio.volume = Mathf.Lerp(nightAudio.volume, nightVolume * currentAudioVolume * (1 - (waterAudio.volume * 4)) * (1 - belowAudio.volume) * (1 - windAudio.volume) * (1 - maxAmbianceAudioVolume) * customWeatherVolumeMultiplier, 0.5f * Time.deltaTime);

			windZone.transform.rotation = Quaternion.Slerp(windZone.transform.rotation, Quaternion.Euler(0, wind, 0), 0.5f * deltaTime);
			windZone.windMain = Mathf.Lerp(windZone.windMain, maxWindMain, 0.5f * deltaTime);

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Bubbles");

			point.y = Mathf.Min(point.y - 16, surfaceLevel - 32);
			bubbles.position = point;

			UnityEngine.Profiling.Profiler.EndSample();

			if (skyboxNeedsColorUpdate)
			{
				updateSkyboxColors();
			}

			// AFTER maybe updating sky colors.
			if (skyboxNeedsReflectionUpdate)
			{
				lastSkyboxReflectionUpdate = Time.time;
				skyboxNeedsReflectionUpdate = false;

				isReflectionBuilding = true;
				isReflectionBuildingVision = true;
			}
			else if (Time.time - lastSkyboxReflectionUpdate > 3)
			{
				// Flag as dirty for next frame.
				skyboxNeedsReflectionUpdate = true;
			}

			// Tick reflection capture after maybe flagging as dirty.
			updateSkyboxReflections();
		}

		private static void renderSkyboxReflection(RenderTexture target, ref int index, ref bool isBuilding)
		{
			if (!isBuilding)
			{
				return;
			}

			if (target == null || reflectionCamera == null)
			{
				return;
			}

			// Documentation notes the texture can be destroyed by certain events like entering screensaver mode.
			if (!target.IsCreated())
			{
				target.Create();
			}

			int mask = 1 << index;

			index++;
			if (index > 5)
			{
				index = 0;
				isBuilding = false;
			}

			reflectionCamera.RenderToCubemap(target, mask);
		}

		public static void updateSkyboxReflections()
		{
			UnityEngine.Profiling.Profiler.BeginSample("LevelLighting.updateSkyboxReflections");
			if (isSkyboxReflectionEnabled)
			{
				if (vision == ELightingVision.NONE)
				{
					renderSkyboxReflection(reflectionMap, ref reflectionIndex, ref isReflectionBuilding);

					RenderSettings.customReflectionTexture = reflectionMap;
				}
				else
				{
					renderSkyboxReflection(reflectionMapVision, ref reflectionIndexVision, ref isReflectionBuildingVision);

					RenderSettings.customReflectionTexture = reflectionMapVision;
				}
			}
			else
			{
				RenderSettings.customReflectionTexture = null;
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private static float cachedAtmosphericFog;
		private static void setAtmosphericFog(float newFog)
		{
			if (MathfEx.IsNearlyEqual(cachedAtmosphericFog, newFog, tolerance: 0.001f) == false)
			{
				cachedAtmosphericFog = newFog;
				Shader.SetGlobalFloat("_AtmosphericFog", newFog);
			}

			UpdateSunShaftsIntensity(1f - newFog);
		}

		private static Color cachedAlphaParticleLightingColor;
		private static void setAlphaParticleLightingColor(Color newColor)
		{
			if (MathfEx.IsNearlyEqual(newColor, cachedAlphaParticleLightingColor) == false)
			{
				cachedAlphaParticleLightingColor = newColor;
				Shader.SetGlobalColor("_AlphaParticleLightingColor", newColor);
			}
		}

		internal static Color cachedSkyColor;
		private static void setSkyColor(Color skyColor)
		{
			if (MathfEx.IsNearlyEqual(skyColor, cachedSkyColor) == false)
			{
				cachedSkyColor = skyColor;
				skyboxNeedsColorUpdate = true;
			}
		}

		internal static Color cachedEquatorColor;
		private static void setEquatorColor(Color equatorColor)
		{
			if (MathfEx.IsNearlyEqual(equatorColor, cachedEquatorColor) == false)
			{
				cachedEquatorColor = equatorColor;
				skyboxNeedsColorUpdate = true;
			}
		}

		internal static Color cachedGroundColor;
		private static void setGroundColor(Color groundColor)
		{
			if (MathfEx.IsNearlyEqual(groundColor, cachedGroundColor) == false)
			{
				cachedGroundColor = groundColor;
				skyboxNeedsColorUpdate = true;
			}
		}

		private static bool skyboxNeedsColorUpdate = false;
		private static void updateSkyboxColors()
		{
			skyboxNeedsColorUpdate = false;

			skybox.SetColor("_SkyColor", cachedSkyColor);
			skybox.SetColor("_EquatorColor", cachedEquatorColor);
			skybox.SetColor("_GroundColor", cachedGroundColor);

			setSeaColor("_SkyColor", cachedSkyColor);
			setSeaColor("_EquatorColor", cachedEquatorColor);
			setSeaColor("_GroundColor", cachedGroundColor);
		}

		private static void resetCachedValues()
		{
			cachedAtmosphericFog = -1.0f;
			cachedAlphaParticleLightingColor = new Color(-1.0f, -1.0f, -1.0f);
			cachedSkyColor = new Color(-1.0f, -1.0f, -1.0f);
			cachedEquatorColor = new Color(-1.0f, -1.0f, -1.0f);
			cachedGroundColor = new Color(-1.0f, -1.0f, -1.0f);
		}

		/// <summary>
		/// Reset any global shader properties that may affect the main menu.
		/// </summary>
		public static void resetForMainMenu()
		{
			setAtmosphericFog(0f); // Otherwise menu skybox retains the in-game atmospheric fog value.
			setAlphaParticleLightingColor(Color.white); // Freezing mythical uses lighting color.
		}

		static LevelLighting()
		{
			bool newEdWaterFx;
			if (ConvenientSavedata.get().read("EditorWantsUnderwaterEffects", out newEdWaterFx))
			{
				_editorWantsUnderwaterEffects = newEdWaterFx;
			}

			bool newEdWaterSurface;
			if (ConvenientSavedata.get().read("EditorWantsWaterSurfaceVisible", out newEdWaterSurface))
			{
				_editorWantsWaterSurface = newEdWaterSurface;
			}

			bool newEdNoLighting;
			if (ConvenientSavedata.get().read("EditorWantsNoLightingPreview", out newEdNoLighting))
			{
				_editorWantsNoLightingPreview = newEdNoLighting;
			}
		}

		class AmbianceAudioInstance
		{
			/// <summary>
			/// Source effect to group multiple volumes.
			/// </summary>
			public EffectAsset effect;

			/// <summary>
			/// Audio source added to AmbianceAudioGameObject.
			/// </summary>
			public AudioSource audioSource;

			/// <summary>
			/// Reset to false before updating volumes.
			/// </summary>
			public bool isAnyVolumeOverlapping;

			/// <summary>
			/// Reset to false before updating volumes.
			/// </summary>
			public bool isAnyNonDistanceVolumeOverlapping;

			/// <summary>
			/// Reset to zero before updating volumes. If any volume uses distance fadeout, this is the maximum alpha.
			/// </summary>
			public float maxDistanceAlpha;

			/// <summary>
			/// If any volume doesn't use distance fadeout, this is the alpha based on time spent inside..
			/// </summary>
			public float timeAlpha;

			/// <summary>
			/// Highest priority of overlapping volumes.
			/// </summary>
			public int maxPriority;

			/// <summary>
			/// If any volume doesn't use distance fadeout, this is the minimum of their audio fade-in time.
			/// </summary>
			public float? minFadeInDuration;

			/// <summary>
			/// If any volume doesn't use distance fadeout, this is the minimum of their audio fade-out time.
			/// Only reset when created so that value is available after leaving all volumes.
			/// </summary>
			public float? minFadeOutDuration;
		}

		private static AmbianceAudioInstance FindAudioInstanceForEffect(EffectAsset asset)
		{
			foreach (AmbianceAudioInstance instance in activeAmbianceAudioInstances)
			{
				if (instance.effect == asset)
				{
					return instance;
				}
			}

			return null;
		}

		private static AmbianceAudioInstance CreateAudioInstance(EffectAsset asset)
		{
			if (!ambianceAudioPool.TryPop(out AmbianceAudioInstance instance))
			{
				instance = new AmbianceAudioInstance();
				instance.audioSource = ambianceAudioGameObject.AddComponent<AudioSource>();
				instance.audioSource.playOnAwake = false;
				instance.audioSource.loop = true;
				instance.audioSource.spatialBlend = 0.0f; // 2D
				instance.audioSource.dopplerLevel = 0.0f;
			}

			instance.effect = asset;
			instance.audioSource.volume = 0.0f;
			instance.isAnyVolumeOverlapping = false;
			instance.isAnyNonDistanceVolumeOverlapping = false;
			instance.maxDistanceAlpha = 0.0f;
			instance.timeAlpha = 0.0f;
			instance.maxPriority = 0;
			instance.minFadeInDuration = null;
			instance.minFadeOutDuration = null;

#if !DEDICATED_SERVER
			if (asset.isMusic)
			{
				instance.audioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetMusicGroup();
			}
			else
			{
				instance.audioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetAtmosphereGroup();
			}
#endif // !DEDICATED_SERVER

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			instance.audioSource.name = $"AmbientAudioInstance {asset.name}";
#endif

			activeAmbianceAudioInstances.Add(instance);
			return instance;
		}

		private static void RemoveAudioInstance(int index)
		{
			AmbianceAudioInstance instance = activeAmbianceAudioInstances[index];
			// Not RemoveAtFast because list is sorted.
			activeAmbianceAudioInstances.RemoveAt(index);

			instance.effect = null;
			instance.audioSource.Stop();
			ambianceAudioPool.Push(instance);
		}

		private static void UpdateFogBlend(float deltaTime)
		{
			if (localFogBlend < 0.0001f)
			{
				localFogColor = levelFogColor;
				localFogIntensity = levelFogIntensity;
				localAtmosphericFog = levelAtmosphericFog;
				localBlendingFogFadeOutDuration = null;
			}

			int lightingFromIndex;
			int lightingToIndex;
			float lightingBlendAlpha;
			GetLightingIndices(out lightingFromIndex, out lightingToIndex, out lightingBlendAlpha);

			bool isAnyNonDistanceVolumeOverlapping = false;

			float? minFadeInDuration = null;
			// We do not reset minFadeOutDuration here because we want it to retain value when leaving volume.

			float maxDistanceAlpha = 0.0f;
			// Highest priority volumes are at front of list, so accumulate fog from lowest to highest priority.
			for (int index = activeAmbianceVolumes.Count - 1; index >= 0; --index)
			{
				VolumeAlphaPair<AmbianceVolume> active = activeAmbianceVolumes[index];
				if (active.volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.None)
				{
					continue;
				}

				Color volumeFogColor;
				float volumeFogIntensity;
				active.volume.GetFogSettings(lightingFromIndex, lightingToIndex, lightingBlendAlpha, out volumeFogColor, out volumeFogIntensity);

				float volumeAtmosphericFog = active.volume.overrideAtmosphericFog ? volumeFogIntensity : 0.0f;
				if (active.volume.enableFalloff)
				{
					maxDistanceAlpha = Mathf.Max(maxDistanceAlpha, active.alpha);

					localFogColor = Color.Lerp(localFogColor, volumeFogColor, active.alpha);
					localFogIntensity = Mathf.Lerp(localFogIntensity, volumeFogIntensity, active.alpha);
					localAtmosphericFog = Mathf.Lerp(localAtmosphericFog, volumeAtmosphericFog, active.alpha);
				}
				else
				{
					isAnyNonDistanceVolumeOverlapping = true;
					localFogColor = volumeFogColor;
					localFogIntensity = volumeFogIntensity;
					localAtmosphericFog = volumeAtmosphericFog;

					if (minFadeInDuration.HasValue)
					{
						minFadeInDuration = Mathf.Max(0.0001f, Mathf.Min(minFadeInDuration.Value,
							active.volume.fogFadeInDuration));
					}
					else
					{
						minFadeInDuration = Mathf.Max(0.0001f, active.volume.fogFadeInDuration);
					}

					if (localBlendingFogFadeOutDuration.HasValue)
					{
						localBlendingFogFadeOutDuration = Mathf.Max(0.0001f, Mathf.Min(localBlendingFogFadeOutDuration.Value,
							active.volume.fogFadeOutDuration));
					}
					else
					{
						localBlendingFogFadeOutDuration = Mathf.Max(0.0001f, active.volume.fogFadeOutDuration);
					}
				}
			}

			float duration = isAnyNonDistanceVolumeOverlapping
				? (minFadeInDuration ?? 20.0f)
				: (localBlendingFogFadeOutDuration ?? 8.0f);
			float maxDelta = deltaTime / duration;
			float targetTimeAlpha = isAnyNonDistanceVolumeOverlapping ? 1.0f : 0.0f;
			localBlendingFogTimeAlpha = Mathf.MoveTowards(localBlendingFogTimeAlpha, targetTimeAlpha, maxDelta);

			localFogBlend = Mathf.Max(maxDistanceAlpha, localBlendingFogTimeAlpha);
			localBlendingFog = localFogBlend > 0.001f;
		}

		private static System.Comparison<VolumeAlphaPair<AmbianceVolume>> ambianceVolumeComparison = CompareAmbianceVolumes;

		private static int CompareAmbianceVolumes(VolumeAlphaPair<AmbianceVolume> lhs, VolumeAlphaPair<AmbianceVolume> rhs)
		{
			return -lhs.volume.priority.CompareTo(rhs.volume.priority);
		}

		private static System.Comparison<AmbianceAudioInstance> ambianceAudioComparison = CompareAmbianceAudioInstances;

		private static int CompareAmbianceAudioInstances(AmbianceAudioInstance lhs, AmbianceAudioInstance rhs)
		{
			return lhs.maxPriority.CompareTo(rhs.maxPriority);
		}

		private static void UpdateAmbianceAudio(float deltaTime, out float maxAmbianceAudioVolume)
		{
			maxAmbianceAudioVolume = 0.0f;
			if (activeAmbianceAudioInstances == null)
			{
				return;
			}

			foreach (AmbianceAudioInstance instance in activeAmbianceAudioInstances)
			{
				instance.isAnyVolumeOverlapping = false;
				instance.isAnyNonDistanceVolumeOverlapping = false;
				instance.maxDistanceAlpha = 0.0f;
				instance.minFadeInDuration = null;
				// We do not reset maxPriority here because we want it to retain priority when leaving volume.
				// ↑ same for minFadeOutTime.
			}

			foreach (VolumeAlphaPair<AmbianceVolume> active in activeAmbianceVolumes)
			{
				EffectAsset effect = active.volume.GetEffectAsset();
				AudioSource effectAudioSource = effect?.effect?.GetComponent<AudioSource>();
				if (effectAudioSource == null || effectAudioSource.clip == null)
				{
					continue;
				}

				if (effect.isMusic && OptionsSettings.ambientMusicVolume <= 0.001f)
				{
					continue;
				}

				AmbianceAudioInstance instance = FindAudioInstanceForEffect(effect);
				if (instance == null)
				{
					instance = CreateAudioInstance(effect);
					instance.audioSource.clip = effectAudioSource.clip;
					instance.audioSource.Play();
				}

				instance.isAnyVolumeOverlapping = true;
				if (active.volume.enableFalloff)
				{
					instance.maxDistanceAlpha = Mathf.Max(instance.maxDistanceAlpha, active.alpha);
				}
				else
				{
					instance.isAnyNonDistanceVolumeOverlapping = true;

					if (instance.minFadeInDuration.HasValue)
					{
						instance.minFadeInDuration = Mathf.Max(0.0001f, Mathf.Min(instance.minFadeInDuration.Value,
							active.volume.audioFadeInDuration));
					}
					else
					{
						instance.minFadeInDuration = Mathf.Max(0.0001f, active.volume.audioFadeInDuration);
					}

					if (instance.minFadeOutDuration.HasValue)
					{
						instance.minFadeOutDuration = Mathf.Max(0.0001f, Mathf.Min(instance.minFadeOutDuration.Value,
							active.volume.audioFadeOutDuration));
					}
					else
					{
						instance.minFadeOutDuration = Mathf.Max(0.0001f, active.volume.audioFadeOutDuration);
					}
				}
				instance.maxPriority = Mathf.Max(instance.maxPriority, active.volume.priority);
			}

			if (activeAmbianceAudioInstances.Count < 1)
			{
				return;
			}
			else if (activeAmbianceAudioInstances.Count > 1)
			{
				// Sort highest priority to back of list because we iterate in reverse order to remove items.
				activeAmbianceAudioInstances.Sort(CompareAmbianceAudioInstances);
			}

			// Nelson 2025-05-29: Volume Priority: we iterate from highest priority (back of list) to lowest (front of
			// list). Each time priority decreases we update the amount to decrease volume according to the highest
			// pre-priority volume we've seen so far. For example:
			// 1: Priority 3, Volume: 0.7 → no change, highestVolume: 0.7
			// 2: Priority 3, Volume: 0.9 → highestVolume: 0.9
			// 3: Priority 2, Volume: 1.0 → decreaseVolume: highestVolume (0.9), highestVolume: 1.0, new volume: 0.1
			// 4: Priority 2, Volume: 0.5 → new volume: 0.0
			// 5: Priority 1, Volume: 1.0 → decreaseVolume: highestVolume (1.0), new volume: 0.0 
			int previousTierHighestPriority = activeAmbianceAudioInstances[activeAmbianceAudioInstances.Count - 1].maxPriority;
			float highestVolume = 0.0f;
			float decreaseVolume = 0.0f;

			for (int index = activeAmbianceAudioInstances.Count - 1; index >= 0; --index)
			{
				AmbianceAudioInstance instance = activeAmbianceAudioInstances[index];
				
				float alpha;
				if (instance.isAnyVolumeOverlapping)
				{
					float maxDelta = deltaTime / (instance.minFadeInDuration ?? 2.0f);
					float targetTimeAlpha = instance.isAnyNonDistanceVolumeOverlapping ? 1.0f : 0.0f;
					instance.timeAlpha = Mathf.MoveTowards(instance.timeAlpha, targetTimeAlpha, maxDelta);
					alpha = Mathf.Max(instance.timeAlpha, instance.maxDistanceAlpha);
				}
				else
				{
					float maxDelta = deltaTime / (instance.minFadeOutDuration ?? 2.0f);
					instance.timeAlpha = Mathf.MoveTowards(instance.timeAlpha, 0.0f, maxDelta);
					alpha = instance.timeAlpha;
				}

				if (alpha <= 0.001f)
				{
					RemoveAudioInstance(index);
					continue;
				}

				float targetVolume = instance.effect.isMusic ? OptionsSettings.ambientMusicVolume : 1.0f;
				float volume = targetVolume * alpha;
				if (instance.maxPriority < previousTierHighestPriority)
				{
					previousTierHighestPriority = instance.maxPriority;
					decreaseVolume = highestVolume;
				}
				highestVolume = Mathf.Max(highestVolume, volume);
				volume = Mathf.Max(0.0f, volume - decreaseVolume);
				//UnturnedLog.info($"[{index}] {instance.audioSource.clip.name} Priority: {instance.maxPriority} Alpha: {alpha} Decrease: {decreaseVolume} Volume: {volume}");

				maxAmbianceAudioVolume = Mathf.Max(volume, maxAmbianceAudioVolume);
				instance.audioSource.volume = volume;
			}
		}

		/// <summary>
		/// Moves legacy image effect dependency out of SDK release.
		/// </summary>
		static partial void UpdateSunShafts(Transform sunTransform, Color sunColor);

		/// <summary>
		/// Moves legacy image effect dependency out of SDK release.
		/// </summary>
		static partial void UpdateSunShaftsIntensity(float intensity);

		#region Obsolete
		[System.Obsolete("Renamed to UpdateForViewer and added deltaTime parameter")]
		public static void updateLocal(Vector3 point, float windOverride, IAmbianceNode effectNode)
		{
			UpdateForViewer(localPoint, localWindOverride, Time.deltaTime);
		}

		[System.Obsolete("Renamed to ForceRefreshForLatestViewer")]
		public static void updateLocal()
		{
			ForceRefreshForLatestViewer();
		}
		#endregion Obsolete
	}
}
