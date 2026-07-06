////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandWeather : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (string.Equals(parameter, "0")) // Cancel custom weather.
			{
				LightingManager.ResetScheduledWeather();
				CommandWindow.Log(localization.format("WeatherText", "null"));
				return;
			}

			AssetReference<WeatherAssetBase> assetRef;
			if (AssetReference<WeatherAssetBase>.TryParse(parameter, out assetRef))
			{
				WeatherAssetBase asset = assetRef.Find();
				if (asset != null)
				{
					// Try to forecast according to level rules, but if unset treat it as perpetual.
					if (!LightingManager.ForecastWeatherImmediately(asset))
					{
						LightingManager.ActivatePerpetualWeather(asset);
					}
					CommandWindow.Log(localization.format("WeatherText", asset.name));
					return;
				}
			}

			string mode = parameter.ToLower();
			if (mode == localization.format("WeatherNone").ToLower())
			{
				LightingManager.ResetScheduledWeather();
			}
			else if (mode == localization.format("WeatherDisable").ToLower())
			{
				LightingManager.DisableWeather();
			}
			else if (mode == localization.format("WeatherStorm").ToLower())
			{
				WeatherAssetBase defaultRain = WeatherAssetBase.DEFAULT_RAIN.Find();
				if (defaultRain != null)
				{
					if (LightingManager.IsWeatherActive(defaultRain))
					{
						LightingManager.ResetScheduledWeather();
					}
					else
					{
						LightingManager.ForecastWeatherImmediately(defaultRain);
					}
				}
			}
			else if (mode == localization.format("WeatherBlizzard").ToLower())
			{
				WeatherAssetBase defaultSnow = WeatherAssetBase.DEFAULT_SNOW.Find();
				if (defaultSnow != null)
				{
					if (LightingManager.IsWeatherActive(defaultSnow))
					{
						LightingManager.ResetScheduledWeather();
					}
					else
					{
						LightingManager.ForecastWeatherImmediately(defaultSnow);
					}
				}
			}
			else
			{
				CommandWindow.LogError(localization.format("NoWeatherErrorText", mode));
				return;
			}

			CommandWindow.Log(localization.format("WeatherText", mode));
		}

		public CommandWeather(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("WeatherCommandText");
			_info = localization.format("WeatherInfoText");
			_help = localization.format("WeatherHelpText");
		}
	}
}
