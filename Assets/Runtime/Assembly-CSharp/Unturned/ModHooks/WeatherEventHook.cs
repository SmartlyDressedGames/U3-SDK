////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;
#if GAME
using Unturned.UnityEx;
#endif // GAME

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to any GameObject to receive weather events:
	///	- Day/Night
	///	- Full Moon
	///	- Rain
	///	- Snow
	/// </summary>
	[AddComponentMenu("Unturned/Weather Event Hook")]
	public class WeatherEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when night changes to day.
		/// </summary>
		public UnityEvent OnDay;

		/// <summary>
		/// Invoked when day changes to night.
		/// </summary>
		public UnityEvent OnNight;

#if GAME
		protected void onDayNightUpdated(bool isDaytime)
		{
			if (isDaytime)
			{
				OnDay.TryInvoke(this);
			}
			else
			{
				OnNight.TryInvoke(this);
			}
		}
#endif // GAME

		/// <summary>
		/// Invoked when a zombie full-moon event starts.
		/// </summary>
		public UnityEvent OnFullMoonBegin;

		/// <summary>
		/// Invoked when a zombie full-moon event finishes.
		/// </summary>
		public UnityEvent OnFullMoonEnd;

#if GAME
		protected void onMoonUpdated(bool isFullMoon)
		{
			if (isFullMoon)
			{
				OnFullMoonBegin.TryInvoke(this);
			}
			else
			{
				OnFullMoonEnd.TryInvoke(this);
			}
		}
#endif // GAME

		/// <summary>
		/// Invoked when rain starts to fall.
		/// </summary>
		public UnityEvent OnRainBegin;

		/// <summary>
		/// Invoked when rain finishes falling.
		/// </summary>
		public UnityEvent OnRainEnd;

#if GAME
		protected void onRainUpdated(ELightingRain rain)
		{
			if (rain == ELightingRain.DRIZZLE)
			{
				OnRainBegin.TryInvoke(this);
			}
			else
			{
				OnRainEnd.TryInvoke(this);
			}
		}
#endif // GAME

		/// <summary>
		/// Invoked when snow starts to fall.
		/// </summary>
		public UnityEvent OnSnowBegin;

		/// <summary>
		/// Invoked when snow finishes falling.
		/// </summary>
		public UnityEvent OnSnowEnd;

#if GAME
		protected void onSnowUpdated(ELightingSnow snow)
		{
			if (snow == ELightingSnow.BLIZZARD)
			{
				OnSnowBegin.TryInvoke(this);
			}
			else
			{
				OnSnowEnd.TryInvoke(this);
			}
		}

		protected void OnEnable()
		{
			LightingManager.onDayNightUpdated_ModHook += onDayNightUpdated;
			LightingManager.onMoonUpdated_ModHook += onMoonUpdated;
			LightingManager.onRainUpdated_ModHook += onRainUpdated;
			LightingManager.onSnowUpdated_ModHook += onSnowUpdated;

			onDayNightUpdated(LightingManager.isDaytime);
			onMoonUpdated(LightingManager.isFullMoon);
			onRainUpdated(LevelLighting.rainyness);
			onSnowUpdated(LevelLighting.snowyness);
		}

		protected void OnDisable()
		{
			LightingManager.onDayNightUpdated_ModHook -= onDayNightUpdated;
			LightingManager.onMoonUpdated_ModHook -= onMoonUpdated;
			LightingManager.onRainUpdated_ModHook -= onRainUpdated;
			LightingManager.onSnowUpdated_ModHook -= onSnowUpdated;
		}
#endif // GAME
	}
}
