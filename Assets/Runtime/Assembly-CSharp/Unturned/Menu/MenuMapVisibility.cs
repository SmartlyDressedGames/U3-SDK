////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SDG.Unturned
{
	/// <summary>
	/// Component in the root Menu scene.
	/// Additively loads decoration levels without modifying main scene.
	/// </summary>
	public class MenuMapVisibility : MonoBehaviour
	{
		public void Awake()
		{
			if (HelperClass.clNoAdditiveMenu)
			{
				UnturnedLog.info("Skipping loading of additive menu scenes");
				return;
			}

#if WITH_NOREDIST
			string promoLevel = null;
			if (HelperClass.clAdditiveMenuOverride.hasValue)
			{
				promoLevel = HelperClass.clAdditiveMenuOverride.value;
			}
			else if (Provider.statusData != null && Provider.statusData.Menu != null && string.IsNullOrEmpty(Provider.statusData.Menu.PromoLevel) == false)
			{
				DateTime start = Provider.statusData.Menu.PromoStart;
				DateTime end = Provider.statusData.Menu.PromoEnd;

				DateTimeRange range = new DateTimeRange(start, end);
				if (range.isNowWithinRange())
				{
					promoLevel = Provider.statusData.Menu.PromoLevel;
				}
			}

			if (!string.IsNullOrEmpty(promoLevel))
			{
				UnturnedLog.info("Loading promo menu scene {0}", promoLevel);
				SceneManager.LoadSceneAsync(promoLevel, LoadSceneMode.Additive);
				return; // We do not consider loading base or holiday levels if promo level is active.
			}

			SceneManager.LoadSceneAsync("Menu_Base", LoadSceneMode.Additive);

			ENPCHoliday holiday = HolidayUtil.getActiveHoliday();
			if (holiday == ENPCHoliday.CHRISTMAS)
			{
				UnturnedLog.info("Loading additive Christmas scene");
				SceneManager.LoadSceneAsync("Menu_Christmas", LoadSceneMode.Additive);
			}
			else if (holiday == ENPCHoliday.HALLOWEEN)
			{
				UnturnedLog.info("Loading additive Halloween scene");
				SceneManager.LoadSceneAsync("Menu_Halloween", LoadSceneMode.Additive);
			}
			else if (holiday == ENPCHoliday.PRIDE_MONTH)
			{
				UnturnedLog.info("Loading additive Pride Month scene");
				SceneManager.LoadSceneAsync("Menu_PrideMonth", LoadSceneMode.Additive);
			}
			else if (holiday == ENPCHoliday.UNTURNED_ANNIVERSARY)
			{
				UnturnedLog.info("Loading additive Unturned Anniversary scene");
				SceneManager.LoadSceneAsync("Menu_UnturnedAnniversary", LoadSceneMode.Additive);
			}
			else
			{
				UnturnedLog.info("Loading additive default menu");
				SceneManager.LoadSceneAsync("Menu_NoHoliday", LoadSceneMode.Additive);
			}
#else // WITH_NOREDIST
			SceneManager.LoadSceneAsync("Menu_Fallback", LoadSceneMode.Additive);
#endif // WITH_NOREDIST
		}

		/// <summary>
		/// Prevents static member from being initialized during MonoBehaviour construction. (Unity warning)
		/// </summary>
		private static class HelperClass
		{
			public static CommandLineString clAdditiveMenuOverride = new CommandLineString("-AdditiveMenuOverride");
			public static CommandLineFlag clNoAdditiveMenu = new CommandLineFlag(false, "-NoAdditiveMenu");
		}
	}
}
