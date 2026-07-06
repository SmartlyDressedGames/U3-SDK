////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Compares weather intensity to value.
	/// </summary>
	public class NPCWeatherBlendAlphaCondition : NPCLogicCondition
	{
		public AssetReference<WeatherAssetBase> weather
		{
			get;
			private set;
		}

		public float value
		{
			get;
			private set;
		}

		public override bool isConditionMet(Player player)
		{
			return doesLogicPass(LevelLighting.GetWeatherGlobalBlendAlpha(weather.Find()), value);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseGuid("GUID", out System.Guid _guid))
			{
				weather = new AssetReference<WeatherAssetBase>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}

			if (p.data.TryParseFloat("Value", out float _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out System.Guid _guid))
			{
				weather = new AssetReference<WeatherAssetBase>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}

			if (p.data.TryParseFloat(p.legacyPrefix + "_Value", out float _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCWeatherBlendAlphaCondition() { }

		[System.Obsolete]
		public NPCWeatherBlendAlphaCondition(AssetReference<WeatherAssetBase> newWeather, float newValue, ENPCLogicType newLogicType, string newText) : base(newLogicType, newText, false)
		{
			weather = newWeather;
			value = newValue;
		}
	}
}
