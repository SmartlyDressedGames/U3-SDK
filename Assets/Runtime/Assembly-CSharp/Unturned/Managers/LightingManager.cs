////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void DayNightUpdated(bool isDaytime);
	public delegate void TimeOfDayChanged();
	public delegate void MoonUpdated(bool isFullMoon);
	public delegate void RainUpdated(ELightingRain rain);
	public delegate void SnowUpdated(ELightingSnow snow);

	public class LightingManager : SteamCaller
	{
		/// <summary>
		/// Version before named version constants were introduced. (2023-11-07)
		/// </summary>
		public const byte SAVEDATA_VERSION_INITIAL = 6;
		public const byte SAVEDATA_VERSION_ADDED_DATE_COUNTER = 7;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_DATE_COUNTER;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		public static DayNightUpdated onDayNightUpdated;

		/// <summary>
		/// Delegate not reset when level reset.
		/// </summary>
		public static DayNightUpdated onDayNightUpdated_ModHook;

		private static void broadcastDayNightUpdated(bool isDaytime)
		{
			onDayNightUpdated?.Invoke(isDaytime);

			onDayNightUpdated_ModHook?.Invoke(isDaytime);
		}

		public static TimeOfDayChanged onTimeOfDayChanged;

		public static MoonUpdated onMoonUpdated;

		/// <summary>
		/// Delegate not reset when level reset.
		/// </summary>
		public static MoonUpdated onMoonUpdated_ModHook;

		private static void broadcastMoonUpdated(bool isFullMoon)
		{
			onMoonUpdated?.Invoke(isFullMoon);

			onMoonUpdated_ModHook?.Invoke(isFullMoon);
		}

		public static RainUpdated onRainUpdated;

		/// <summary>
		/// Delegate not reset when level reset.
		/// </summary>
		public static RainUpdated onRainUpdated_ModHook;

		internal static void broadcastRainUpdated(ELightingRain rain)
		{
			onRainUpdated?.Invoke(rain);

			onRainUpdated_ModHook?.Invoke(rain);
		}

		public static SnowUpdated onSnowUpdated;

		/// <summary>
		/// Delegate not reset when level reset.
		/// </summary>
		public static SnowUpdated onSnowUpdated_ModHook;

		internal static void broadcastSnowUpdated(ELightingSnow snow)
		{
			onSnowUpdated?.Invoke(snow);

			onSnowUpdated_ModHook?.Invoke(snow);
		}

		private static LightingManager manager;

		public static float day // percentage of progress through the day
=> time / (float) cycle;

		private static uint _cycle; // seconds per cycle
		public static uint cycle
		{
			get => _cycle;

			set
			{
				_offset = Provider.time - (uint) (day * value);
				_cycle = value > 0 ? value : 3600; // Prevent division by zero

				if (Provider.isServer)
				{
					LevelLighting.MarkParticleCloudsNeedRestart();
					manager.updateLighting();
					SendLightingCycle.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), cycle);
				}
			}
		}

		private static uint _time; // current progress through the day
		public static uint time
		{
			get => _time;

			set
			{
				if (cycle > 0) // Prevent division by zero.
				{
					value %= cycle;
				}

				_offset = Provider.time - value;
				_time = value;

				LevelLighting.MarkParticleCloudsNeedRestart();
				manager.updateLighting();
				SendLightingOffset.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), offset);
			}
		}

		/// <summary>
		/// Number of in-game days this world has run.
		/// Incremented each time night ends.
		/// Saved between sessions.
		/// </summary>
		public static long DateCounter
		{
			get => dateCounter;
		}
		private static long dateCounter;

		[System.Obsolete("Replaced by LevelLighting.GetActiveWeatherAsset")]
		public static WeatherAssetBase getCustomWeather()
		{
			return LevelLighting.GetActiveWeatherAsset();
		}

		private static void SetAndReplicateActiveWeatherAsset(WeatherAssetBase asset, float blendAlpha)
		{
			NetId netId = asset != null ? NetIdRegistry.ClaimBlock(2) : default;
			LevelLighting.SetActiveWeatherAsset(asset, blendAlpha, netId);
			System.Guid assetGuid = asset != null ? asset.GUID : System.Guid.Empty;
			SendLightingActiveWeather.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), assetGuid, blendAlpha, netId);
		}

		private static uint _offset; // how much to offset the steam time by
		public static uint offset => _offset;

		[System.Obsolete]
		public static uint rainFrequency;
		[System.Obsolete]
		public static uint rainDuration;
		[System.Obsolete]
		public static bool hasRain => false;

		[System.Obsolete]
		public static uint snowFrequency;
		[System.Obsolete]
		public static uint snowDuration;
		[System.Obsolete]
		public static bool hasSnow => false;

		private enum EScheduledWeatherStage
		{
			/// <summary>
			/// Weather has not been decided yet. Level might not have any enabled.
			/// </summary>
			None,

			/// <summary>
			/// Weather has been forecast. Timer counts down until activation.
			/// </summary>
			Forecast,

			/// <summary>
			/// Weather is now active. Timer counts down until deactivation.
			/// </summary>
			Active,

			/// <summary>
			/// Weather is active. Will not deactivate naturally.
			/// Prevents loaded perpetual weather from deactivating.
			/// </summary>
			PerpetuallyActive,
		}

		/// <summary>
		/// Determines which weather can naturally be scheduled in this level.
		/// Includes default rain and snow for older levels.
		/// </summary>
		private static LevelAsset.SchedulableWeather[] schedulableWeathers;
		private static EScheduledWeatherStage scheduledWeatherStage;
		/// <summary>
		/// Seconds until weather activates.
		/// </summary>
		private static float scheduledWeatherForecastTimer;
		/// <summary>
		/// Seconds until weather deactivates.
		/// </summary>
		private static float scheduledWeatherActiveTimer;
		/// <summary>
		/// Forecast or active weather.
		/// </summary>
		private static AssetReference<WeatherAssetBase> scheduledWeatherRef;
		private static bool shouldTickScheduledWeather;
		private static float loadedWeatherBlendAlpha;

		public static bool IsWeatherActive(WeatherAssetBase weatherAsset)
		{
			if (weatherAsset == null)
				throw new System.ArgumentNullException("weatherAsset");

			return LevelLighting.IsWeatherActive(weatherAsset);
		}

		private static void SetPerpetualWeather(WeatherAssetBase asset, float blendAlpha)
		{
			if (asset == null)
				throw new System.ArgumentNullException("asset");

			scheduledWeatherStage = EScheduledWeatherStage.PerpetuallyActive;
			scheduledWeatherForecastTimer = 0.0f;
			scheduledWeatherActiveTimer = 0.0f;
			scheduledWeatherRef = asset.getReferenceTo<WeatherAssetBase>();
			SetAndReplicateActiveWeatherAsset(asset, blendAlpha);
			shouldTickScheduledWeather = false;
		}

		public static void SetScheduledWeather(WeatherAssetBase weatherAsset, float forecastDuration, float activeDuration)
		{
			if (weatherAsset == null)
				throw new System.ArgumentNullException("weatherAsset");

			scheduledWeatherStage = EScheduledWeatherStage.Forecast;
			scheduledWeatherForecastTimer = forecastDuration;
			scheduledWeatherActiveTimer = activeDuration;
			scheduledWeatherRef = weatherAsset.getReferenceTo<WeatherAssetBase>();
			shouldTickScheduledWeather = true;
		}

		/// <summary>
		/// Set weather active and disable scheduling.
		/// </summary>
		public static void ActivatePerpetualWeather(WeatherAssetBase asset)
		{
			SetPerpetualWeather(asset, 0.0f);
		}

		/// <returns>True if given weather has config.</returns>
		public static bool ForecastWeatherImmediately(WeatherAssetBase weatherAsset)
		{
			if (schedulableWeathers != null)
			{
				foreach (LevelAsset.SchedulableWeather schedulableWeather in schedulableWeathers)
				{
					if (schedulableWeather.assetRef.isReferenceTo(weatherAsset))
					{
						float activeTimer = Random.Range(schedulableWeather.minDuration, schedulableWeather.maxDuration) *
							Provider.modeConfigData.Events.Weather_Duration_Multiplier *
							cycle;
						SetScheduledWeather(weatherAsset, 0.0f, activeTimer);
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Cancel scheduled weather and re-evaluate on next update.
		/// </summary>
		public static void ResetScheduledWeather()
		{
			if (LevelLighting.GetActiveWeatherAsset() != null)
			{
				SetAndReplicateActiveWeatherAsset(null, 0.0f);
			}

			// None weather will change to Forecast on the next update.
			scheduledWeatherStage = EScheduledWeatherStage.None;
		}

		/// <summary>
		/// Cancel active weather and prevent next weather from being scheduled.
		/// </summary>
		public static void DisableWeather()
		{
			if (LevelLighting.GetActiveWeatherAsset() != null)
			{
				SetAndReplicateActiveWeatherAsset(null, 0.0f);
			}

			scheduledWeatherStage = EScheduledWeatherStage.None;
			shouldTickScheduledWeather = false;
		}

		/// <summary>
		/// Get weather override for the currently loaded level.
		/// Warning: this is kept for backwards compatibility, whereas newer maps will set LevelAsset.perpetualWeather.
		/// </summary>
		public static ELevelWeatherOverride levelWeatherOverride
		{
			get
			{
				if (Level.info != null && Level.info.configData != null)
				{
					return Level.info.configData.Weather_Override;
				}
				else
				{
					return ELevelWeatherOverride.NONE;
				}
			}
		}

		private static bool isCycled;

		private static bool _isFullMoon;
		public static bool isFullMoon
		{
			set
			{
				if (value != isFullMoon)
				{
					_isFullMoon = value;
					broadcastMoonUpdated(isFullMoon);
				}
			}

			get => _isFullMoon;
		}

		private static float lastWind;
		private static float windDelay;

		public static bool isDaytime => day < LevelLighting.bias;

		public static bool isNighttime => !isDaytime;

		[System.Obsolete]
		public void tellLighting(CSteamID steamID, uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, byte rain, byte snow, System.Guid activeWeatherGuid)
		{
			tellLighting(steamID, serverTime, newCycle, newOffset, moon, wind, activeWeatherGuid, 0.0f);
		}

		[System.Obsolete]
		public void tellLighting(CSteamID steamID, uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, System.Guid activeWeatherGuid, float activeWeatherBlendAlpha)
		{
			ReceiveInitialLightingState(serverTime, newCycle, newOffset, moon, wind, activeWeatherGuid, activeWeatherBlendAlpha, new NetId(), 0);
		}

		private static readonly ClientStaticMethod<uint, uint, uint, byte, byte, System.Guid, float, NetId, int> SendInitialLightingState = ClientStaticMethod<uint, uint, uint, byte, byte, System.Guid, float, NetId, int>.Get(ReceiveInitialLightingState);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLighting))]
		public static void ReceiveInitialLightingState(uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, System.Guid activeWeatherGuid, float activeWeatherBlendAlpha, NetId activeWeatherNetId, int newDateCounter)
		{
			Provider.time = serverTime; // steam utils time returns a bit differently for some reason

			_cycle = newCycle;
			_offset = newOffset;
			dateCounter = newDateCounter;
			UnturnedLog.info($"Received initial date counter: {dateCounter}");

			AssetReference<WeatherAssetBase> activeWeatherAssetRef = new AssetReference<WeatherAssetBase>(activeWeatherGuid);
			WeatherAssetBase activeWeatherAsset = activeWeatherAssetRef.Find();
			LevelLighting.SetActiveWeatherAsset(activeWeatherAsset, activeWeatherBlendAlpha, activeWeatherNetId);
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(activeWeatherGuid, activeWeatherAsset, nameof(ReceiveInitialLightingState));
			}

			LevelLighting.MarkParticleCloudsNeedRestart();
			manager.updateLighting();

			LevelLighting.moon = moon;
			isCycled = day > LevelLighting.bias;
			isFullMoon = isCycled && LevelLighting.moon == 2;

			broadcastDayNightUpdated(isDaytime);

			onTimeOfDayChanged?.Invoke();

			LevelLighting.wind = wind * 2f;

			Level.isLoadingLighting = false;
		}

		[System.Obsolete]
		public void askLighting(CSteamID steamID)
		{ }

		internal static void SendInitialGlobalState(SteamPlayer client)
		{
			if (Level.info.type != ELevelType.SURVIVAL)
			{
				// This check is really unfortunate, but prior to the RPC rewrite the client would only ask the server for
				// lighting state in survival levels.
				return;
			}

			WeatherAssetBase activeWeatherAsset;
			float activeWeatherBlendAlpha;
			NetId activeWeatherNetId;
			LevelLighting.GetActiveWeatherNetState(out activeWeatherAsset, out activeWeatherBlendAlpha, out activeWeatherNetId);
			System.Guid activeWeatherGuid = activeWeatherAsset != null ? activeWeatherAsset.GUID : System.Guid.Empty;
			SendInitialLightingState.Invoke(ENetReliability.Reliable,
				client.transportConnection,
				Provider.time,
				cycle,
				offset,
				LevelLighting.moon,
				MeasurementTool.angleToByte(LevelLighting.wind),
				activeWeatherGuid,
				activeWeatherBlendAlpha,
				activeWeatherNetId,
				(int) dateCounter);
		}

		[System.Obsolete]
		public void tellLightingCycle(CSteamID steamID, uint newScale)
		{
			ReceiveLightingCycle(newScale);
		}

		private static readonly ClientStaticMethod<uint> SendLightingCycle = ClientStaticMethod<uint>.Get(ReceiveLightingCycle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLightingCycle))]
		public static void ReceiveLightingCycle(uint newScale)
		{
			_offset = Provider.time - (uint) (day * newScale);
			_cycle = newScale;

			LevelLighting.MarkParticleCloudsNeedRestart();
			manager.updateLighting();
		}

		[System.Obsolete]
		public void tellLightingOffset(CSteamID steamID, uint newOffset)
		{
			ReceiveLightingOffset(newOffset);
		}

		private static readonly ClientStaticMethod<uint> SendLightingOffset = ClientStaticMethod<uint>.Get(ReceiveLightingOffset);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLightingOffset))]
		public static void ReceiveLightingOffset(uint newOffset)
		{
			_offset = newOffset;

			LevelLighting.MarkParticleCloudsNeedRestart();
			manager.updateLighting();
		}

		[System.Obsolete]
		public void tellLightingWind(CSteamID steamID, byte newWind)
		{
			ReceiveLightingWind(newWind);
		}

		private static readonly ClientStaticMethod<byte> SendLightingWind = ClientStaticMethod<byte>.Get(ReceiveLightingWind);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLightingWind))]
		public static void ReceiveLightingWind(byte newWind)
		{
			LevelLighting.wind = MeasurementTool.byteToAngle(newWind);
		}

		private static readonly ClientStaticMethod<long> SendDateCounter = ClientStaticMethod<long>.Get(ReceiveDateCounter);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveDateCounter(long newValue)
		{
			dateCounter = newValue;
			UnturnedLog.info($"Received date counter update: {dateCounter}");

			// Invoke this event because it's the one objects monitor for date conditions.
			onTimeOfDayChanged?.Invoke();
		}

		[System.Obsolete]
		public void tellLightingRain(CSteamID steamID, byte newRain)
		{ }

		[System.Obsolete]
		public void tellLightingSnow(CSteamID steamID, byte newSnow)
		{ }

		[System.Obsolete]
		public void tellLightingActiveWeather(CSteamID steamID, System.Guid assetGuid, float blendAlpha)
		{
			ReceiveLightingActiveWeather(assetGuid, blendAlpha, new NetId());
		}

		private static readonly ClientStaticMethod<System.Guid, float, NetId> SendLightingActiveWeather = ClientStaticMethod<System.Guid, float, NetId>.Get(ReceiveLightingActiveWeather);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLightingActiveWeather))]
		public static void ReceiveLightingActiveWeather(System.Guid assetGuid, float blendAlpha, NetId netId)
		{
			AssetReference<WeatherAssetBase> assetRef = new AssetReference<WeatherAssetBase>(assetGuid);
			WeatherAssetBase asset = assetRef.Find();
			LevelLighting.SetActiveWeatherAsset(asset, blendAlpha, netId);
			if (!Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(assetGuid, asset, nameof(ReceiveLightingActiveWeather));
			}
		}

		private void updateLighting()
		{
			_time = (Provider.time - offset) % cycle;

			if (Provider.isServer)
			{
				if (Time.time - lastWind > windDelay)
				{
					windDelay = Random.Range(45, 75);
					lastWind = Time.time;

					LevelLighting.wind = Random.Range(0, 360);
					SendLightingWind.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), MeasurementTool.angleToByte(LevelLighting.wind));
				}
			}

			if (day > LevelLighting.bias)
			{
				if (!isCycled)
				{
					isCycled = true;

					if (LevelLighting.moon < LevelLighting.MOON_CYCLES - 1)
					{
						LevelLighting.moon++;

						isFullMoon = LevelLighting.moon == 2;
					}
					else
					{
						LevelLighting.moon = 0;

						isFullMoon = false;
					}

					broadcastDayNightUpdated(false);
				}
			}
			else
			{
				if (isCycled)
				{
					isCycled = false;

					isFullMoon = false;

					if (Provider.isServer)
					{
						// It's morning! We lived to simulate another in-game day.
						++dateCounter;
						UnturnedLog.info($"Incremented date counter: {dateCounter}");
						SendDateCounter.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), dateCounter);
					}

					broadcastDayNightUpdated(true);
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				LevelLighting.time = day;
			}

			onTimeOfDayChanged?.Invoke();
		}

		private void TickScheduledWeather()
		{
			if (scheduledWeatherStage == EScheduledWeatherStage.None)
			{
				if (schedulableWeathers != null && schedulableWeathers.Length > 0)
				{
					int index = Random.Range(0, schedulableWeathers.Length);
					LevelAsset.SchedulableWeather nextWeather = schedulableWeathers[index];
					WeatherAssetBase nextWeatherAsset = nextWeather.assetRef.Find();
					if (nextWeatherAsset != null)
					{
						float forecastTimer = Random.Range(nextWeather.minFrequency, nextWeather.maxFrequency) *
							Provider.modeConfigData.Events.Weather_Frequency_Multiplier *
							cycle;
						float activeTimer = Random.Range(nextWeather.minDuration, nextWeather.maxDuration) *
							Provider.modeConfigData.Events.Weather_Duration_Multiplier *
							cycle;
						SetScheduledWeather(nextWeatherAsset, forecastTimer, activeTimer);
						UnturnedLog.info("Weather {0} forecast in {1} seconds", nextWeatherAsset.name, scheduledWeatherForecastTimer);
					}
					else
					{
						UnturnedLog.warn("Missing level weather asset {0}", nextWeather.assetRef);
						shouldTickScheduledWeather = false; // Disable scheduled weather to prevent log spam.
					}
				}
				else
				{
					// We can end up in this case if plugin tried to schedule level weather.
					UnturnedLog.warn("Disabling scheduled weather because none are available");
					shouldTickScheduledWeather = false; // Disable scheduled weather to prevent log spam.
				}
			}
			else if (scheduledWeatherStage == EScheduledWeatherStage.Forecast)
			{
				scheduledWeatherForecastTimer -= Time.deltaTime;
				if (scheduledWeatherForecastTimer <= 0.0f)
				{
					WeatherAssetBase nextWeatherAsset = scheduledWeatherRef.Find();
					if (nextWeatherAsset != null)
					{
						scheduledWeatherStage = EScheduledWeatherStage.Active;
						SetAndReplicateActiveWeatherAsset(scheduledWeatherRef.Find(), 0.0f);
						UnturnedLog.info("Weather {0} starting for {1} seconds", nextWeatherAsset.name, scheduledWeatherActiveTimer);
					}
					else
					{
						// Assets may have been modified since this weather was forecast.
						scheduledWeatherStage = EScheduledWeatherStage.None;
						UnturnedLog.warn("Missing forecast weather asset {0}", scheduledWeatherRef);
					}
				}
			}
			else if (scheduledWeatherStage == EScheduledWeatherStage.Active)
			{
				scheduledWeatherActiveTimer -= Time.deltaTime;
				if (scheduledWeatherActiveTimer <= 0.0f)
				{
					WeatherAssetBase prevWeatherAsset = scheduledWeatherRef.Find();
					if (prevWeatherAsset != null)
					{
						scheduledWeatherStage = EScheduledWeatherStage.None;
						SetAndReplicateActiveWeatherAsset(null, 0.0f);
						UnturnedLog.info("Weather {0} ending", prevWeatherAsset.name);
					}
					else
					{
						scheduledWeatherStage = EScheduledWeatherStage.None;
						UnturnedLog.warn("Missing active weather asset {0}", scheduledWeatherRef);
					}
				}
			}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			else if (scheduledWeatherStage == EScheduledWeatherStage.PerpetuallyActive)
			{
				shouldTickScheduledWeather = false;
				UnturnedLog.error("TickScheduledWeather was called while PerpetuallyActive");
			}
#endif
		}

		/// <summary>
		/// Assign schedulableWeathers array according to level asset or legacy lighting settings.
		/// </summary>
		private void InitSchedulableWeathers()
		{
			if (Provider.modeConfigData.Events.Weather_Duration_Multiplier < 0.001f)
			{
				UnturnedLog.info("Disabling scheduled weather because duration multiplier is zero");
				schedulableWeathers = null;
				return;
			}

			LevelAsset levelAsset = Level.getAsset();
			if (levelAsset != null && levelAsset.schedulableWeathers != null)
			{
				schedulableWeathers = levelAsset.schedulableWeathers;
				return;
			}

			if (!LevelLighting.canRain && !LevelLighting.canSnow)
			{
				// Neither legacy lighting weather is enabled, so disable schedulable weathers.
				schedulableWeathers = null;
				return;
			}

			List<LevelAsset.SchedulableWeather> pendingSchedulableWeathers = new List<LevelAsset.SchedulableWeather>(2);

			if (LevelLighting.canRain)
			{
				float minDuration = Provider.modeConfigData.Events.Rain_Duration_Min * LevelLighting.rainDur;
				float maxDuration = Provider.modeConfigData.Events.Rain_Duration_Max * LevelLighting.rainDur;
				if (Mathf.Max(minDuration, maxDuration) > 0.001f)
				{
					LevelAsset.SchedulableWeather rain = new LevelAsset.SchedulableWeather();
					rain.assetRef = WeatherAssetBase.DEFAULT_RAIN;
					rain.minFrequency = Mathf.Max(0.0f, Provider.modeConfigData.Events.Rain_Frequency_Min * LevelLighting.rainFreq);
					rain.maxFrequency = Mathf.Max(0.0f, Provider.modeConfigData.Events.Rain_Frequency_Max * LevelLighting.rainFreq);
					rain.minDuration = Mathf.Max(0.0f, minDuration);
					rain.maxDuration = Mathf.Max(0.0f, maxDuration);
					pendingSchedulableWeathers.Add(rain);
				}
				else
				{
					UnturnedLog.info("Disabling legacy rain because max duration is zero");
				}
			}

			if (LevelLighting.canSnow)
			{
				float minDuration = Provider.modeConfigData.Events.Snow_Duration_Min * LevelLighting.snowDur;
				float maxDuration = Provider.modeConfigData.Events.Snow_Duration_Max * LevelLighting.snowDur;
				if (Mathf.Max(minDuration, maxDuration) > 0.001f)
				{
					LevelAsset.SchedulableWeather snow = new LevelAsset.SchedulableWeather();
					snow.assetRef = WeatherAssetBase.DEFAULT_SNOW;
					snow.minFrequency = Mathf.Max(0.0f, Provider.modeConfigData.Events.Snow_Frequency_Min * LevelLighting.snowFreq);
					snow.maxFrequency = Mathf.Max(0.0f, Provider.modeConfigData.Events.Snow_Frequency_Max * LevelLighting.snowFreq);
					snow.minDuration = Mathf.Max(0.0f, minDuration);
					snow.maxDuration = Mathf.Max(0.0f, maxDuration);
					pendingSchedulableWeathers.Add(snow);
				}
				else
				{
					UnturnedLog.info("Disabling legacy snow because max duration is zero");
				}
			}

			schedulableWeathers = pendingSchedulableWeathers.ToArray();
		}

		/// <returns>True if perpetual weather was enabled, false otherwise.</returns>
		private bool InitPerpetualWeather()
		{
			AssetReference<WeatherAssetBase> perpetualWeatherRef;

			LevelAsset levelAsset = Level.getAsset();
			if (levelAsset != null && levelAsset.perpetualWeatherRef.isValid)
			{
				perpetualWeatherRef = levelAsset.perpetualWeatherRef;
			}
			else
			{
				switch (levelWeatherOverride)
				{
					case ELevelWeatherOverride.RAIN:
						perpetualWeatherRef = WeatherAssetBase.DEFAULT_RAIN;
						break;

					case ELevelWeatherOverride.SNOW:
						perpetualWeatherRef = WeatherAssetBase.DEFAULT_SNOW;
						break;

					default:
						// Neither level asset nor legacy config has a perpetual override.
						return false;
				}
			}

			WeatherAssetBase perpetualWeatherAsset = perpetualWeatherRef.Find();
			if (perpetualWeatherAsset != null)
			{
				UnturnedLog.info("Level perpetual weather override {0}", perpetualWeatherAsset.name);
				SetPerpetualWeather(perpetualWeatherAsset, 1.0f);
				return true;
			}
			else
			{
				UnturnedLog.warn("Missing level perpetual weather asset {0}", perpetualWeatherRef);
				return false;
			}
		}

		private void InitLoadedWeather()
		{
			if (scheduledWeatherStage == EScheduledWeatherStage.Forecast)
			{
				WeatherAssetBase nextWeatherAsset = scheduledWeatherRef.Find();
				if (nextWeatherAsset != null)
				{
					UnturnedLog.info("Loaded weather {0} forecast in {1} seconds", nextWeatherAsset.name, scheduledWeatherForecastTimer);
				}
				else
				{
					// Do not waste time waiting until timer finishes, reset immediately.
					scheduledWeatherStage = EScheduledWeatherStage.None;
					UnturnedLog.warn("Missing loaded forecast weather asset {0}", scheduledWeatherRef);
				}
			}
			else if (scheduledWeatherStage == EScheduledWeatherStage.Active)
			{
				WeatherAssetBase nextWeatherAsset = scheduledWeatherRef.Find();
				if (nextWeatherAsset != null)
				{
					SetAndReplicateActiveWeatherAsset(scheduledWeatherRef.Find(), loadedWeatherBlendAlpha);
					UnturnedLog.info("Loaded weather {0} with global alpha {1} ending in {2} seconds",
						nextWeatherAsset.name,
						loadedWeatherBlendAlpha,
						scheduledWeatherActiveTimer);
				}
				else
				{
					// Do not waste time waiting until timer finishes, reset immediately.
					scheduledWeatherStage = EScheduledWeatherStage.None;
					UnturnedLog.warn("Missing loaded active weather asset {0}", scheduledWeatherRef);
				}
			}
			else if (scheduledWeatherStage == EScheduledWeatherStage.PerpetuallyActive)
			{
				WeatherAssetBase nextWeatherAsset = scheduledWeatherRef.Find();
				if (nextWeatherAsset != null)
				{
					SetAndReplicateActiveWeatherAsset(scheduledWeatherRef.Find(), loadedWeatherBlendAlpha);
					UnturnedLog.info("Loaded perpetual weather {0} with global alpha {1}",
						nextWeatherAsset.name,
						loadedWeatherBlendAlpha);
					shouldTickScheduledWeather = false;
				}
				else
				{
					scheduledWeatherStage = EScheduledWeatherStage.None;
					UnturnedLog.warn("Missing loaded perpetual weather asset {0}", scheduledWeatherRef);
				}
			}
		}

		private void onPrePreLevelLoaded(int level)
		{
			onDayNightUpdated = null;
			onTimeOfDayChanged = null;
			onMoonUpdated = null;
			onRainUpdated = null;
			onSnowUpdated = null;
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				scheduledWeatherStage = EScheduledWeatherStage.None;
				scheduledWeatherForecastTimer = -1.0f;
				scheduledWeatherActiveTimer = -1.0f;
				scheduledWeatherRef = AssetReference<WeatherAssetBase>.invalid;
				InitSchedulableWeathers();
				// Will set shouldTick to false if perpetual weather gets enabled.
				shouldTickScheduledWeather = schedulableWeathers != null && schedulableWeathers.Length > 0;

				LevelLighting.rainyness = ELightingRain.NONE;
				LevelLighting.snowyness = ELightingSnow.NONE;

				if (Level.info != null && Level.info.type != ELevelType.SURVIVAL)
				{
					_cycle = 3600;
					_offset = 0;
					dateCounter = 0;

					if (Level.info.type == ELevelType.HORDE)
					{
						_time = (uint) ((LevelLighting.bias + ((1f - LevelLighting.bias) / 2f)) * cycle);
						_isFullMoon = true;
					}
					else if (Level.info.type == ELevelType.ARENA)
					{
						_time = (uint) (LevelLighting.transition * cycle);
						_isFullMoon = false;
					}

					windDelay = Random.Range(45, 75);
					LevelLighting.wind = Random.Range(0, 360);
					InitPerpetualWeather();

					Level.isLoadingLighting = false;

					if (!Dedicator.IsDedicatedServer)
					{
						LevelLighting.MarkParticleCloudsNeedRestart();
						LevelLighting.time = day;
						LevelLighting.moon = 2;
					}

					return;
				}

				_cycle = 3600;
				_time = 0;
				_offset = 0;
				dateCounter = 0;
				_isFullMoon = false;
				isCycled = false;

				broadcastDayNightUpdated(true);

				onTimeOfDayChanged?.Invoke();

				windDelay = Random.Range(45, 75);
				LevelLighting.wind = Random.Range(0, 360);

				if (Provider.isServer)
				{
					load();
					if (!InitPerpetualWeather())
					{
						// Level perpetual weather takes priority because it may have changed between updates.
						InitLoadedWeather();
					}

					LevelLighting.MarkParticleCloudsNeedRestart();
					updateLighting();

					Level.isLoadingLighting = false;
				}
			}
		}

		private void Update()
		{
			if (!Level.isLoaded || Level.info == null)
			{
				return;
			}

			if (Level.isEditor)
			{
				UnityEngine.Profiling.Profiler.BeginSample("LevelLighting.updateLighting()");
				LevelLighting.updateLighting();
				UnityEngine.Profiling.Profiler.EndSample();
			}
			else
			{
				if (Level.info.type == ELevelType.SURVIVAL)
				{
					UnityEngine.Profiling.Profiler.BeginSample("LightingManager.updateLighting()");
					updateLighting();
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}

			if (Dedicator.IsDedicatedServer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("LightingManager.tickCustomWeatherBlending()");
				LevelLighting.tickCustomWeatherBlending(uint.MaxValue);
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (Provider.isServer && shouldTickScheduledWeather)
			{
				UnityEngine.Profiling.Profiler.BeginSample("LightingManager.TickScheduledWeather()");
				TickScheduledWeather();
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		private void Start()
		{
			manager = this;

			Level.onPrePreLevelLoaded += onPrePreLevelLoaded;
			Level.onLevelLoaded += onLevelLoaded;
		}

		public static void load()
		{
			bool useDefaults = true;

			if (LevelSavedata.fileExists("/Lighting.dat"))
			{
				River river = LevelSavedata.openRiver("/Lighting.dat", true);
				byte version = river.readByte();

				if (version > 0)
				{
					_cycle = river.readUInt32();
					if (_cycle < 1)
						_cycle = 3600; // Prevent division by zero

					_time = river.readUInt32();

					if (version > 1 && version < 5)
					{
						river.readUInt32(); // rainFrequency
						river.readUInt32(); // rainDuration
						river.readBoolean(); // _hasRain
						river.readByte(); // ELightingRain
					}

					if (version > 2 && version < 5)
					{
						river.readUInt32(); // snowFrequency
						river.readUInt32(); // snowDuration
						river.readBoolean(); // _hasSnow
						river.readByte(); // ELightingSnow
					}

					if (version > 3)
					{
						scheduledWeatherStage = (EScheduledWeatherStage) river.readByte();
						scheduledWeatherForecastTimer = river.readSingle();
						scheduledWeatherActiveTimer = river.readSingle();
						scheduledWeatherRef = new AssetReference<WeatherAssetBase>(river.readGUID());
						if (version > 5)
						{
							loadedWeatherBlendAlpha = river.readSingle();
						}
						else
						{
							loadedWeatherBlendAlpha = 0.0f;
						}
					}

					_offset = Provider.time - time;

					if (version >= SAVEDATA_VERSION_ADDED_DATE_COUNTER)
					{
						dateCounter = river.readInt64();
						if (dateCounter < 0)
						{
							dateCounter = 0;
						}
						UnturnedLog.info($"Loaded date counter: {dateCounter}");
					}
					else
					{
						dateCounter = 0;
					}

					useDefaults = false;
				}

				river.closeRiver();
			}

			if (useDefaults)
			{
				_time = (uint) (cycle * LevelLighting.transition);
				_offset = Provider.time - time;
				dateCounter = 0;
			}
		}

		public static void save()
		{
			River river = LevelSavedata.openRiver("/Lighting.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);
			river.writeUInt32(cycle);
			river.writeUInt32(time);
			river.writeByte((byte) scheduledWeatherStage);
			river.writeSingle(scheduledWeatherForecastTimer);
			river.writeSingle(scheduledWeatherActiveTimer);
			river.writeGUID(scheduledWeatherRef.GUID);
			river.writeSingle(LevelLighting.GetActiveWeatherGlobalBlendAlpha());
			river.writeInt64(dateCounter);
			river.closeRiver();
		}
	}
}
