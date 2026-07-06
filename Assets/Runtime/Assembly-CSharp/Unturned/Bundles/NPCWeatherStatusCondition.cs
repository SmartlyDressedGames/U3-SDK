////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum ENPCWeatherStatus
	{
		/// <summary>
		/// True while fading in or fully transitioned in. 
		/// </summary>
		Active,

		/// <summary>
		/// True while fading in, but not at full intensity.
		/// </summary>
		Transitioning_In,

		/// <summary>
		/// True while finished fading in.
		/// </summary>
		Fully_Transitioned_In,

		/// <summary>
		/// True while fading out, but not at zero intensity.
		/// </summary>
		Transitioning_Out,

		/// <summary>
		/// True while finished fading out.
		/// </summary>
		Fully_Transitioned_Out,

		/// <summary>
		/// True while fading in or out.
		/// </summary>
		Transitioning,
	}

	public class NPCWeatherStatusCondition : NPCLogicCondition
	{
		public AssetReference<WeatherAssetBase> weather
		{
			get;
			private set;
		}

		public ENPCWeatherStatus value
		{
			get;
			private set;
		}

		public override bool isConditionMet(Player player)
		{
			switch (value)
			{
				case ENPCWeatherStatus.Active:
					return doesLogicPass(LevelLighting.IsWeatherActive(weather.Find()), true);

				case ENPCWeatherStatus.Transitioning_In:
					return doesLogicPass(LevelLighting.IsWeatherTransitioningIn(weather.Find()), true);

				case ENPCWeatherStatus.Fully_Transitioned_In:
					return doesLogicPass(LevelLighting.IsWeatherFullyTransitionedIn(weather.Find()), true);

				case ENPCWeatherStatus.Transitioning_Out:
					return doesLogicPass(LevelLighting.IsWeatherTransitioningOut(weather.Find()), true);

				case ENPCWeatherStatus.Fully_Transitioned_Out:
					return doesLogicPass(LevelLighting.IsWeatherFullyTransitionedOut(weather.Find()), true);

				case ENPCWeatherStatus.Transitioning:
					return doesLogicPass(LevelLighting.IsWeatherTransitioning(weather.Find()), true);
			}

			return false;
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

			if (p.data.TryParseEnum("Value", out ENPCWeatherStatus _value))
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

			if (p.data.TryParseEnum(p.legacyPrefix + "_Value", out ENPCWeatherStatus _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCWeatherStatusCondition() { }

		[System.Obsolete]
		public NPCWeatherStatusCondition(AssetReference<WeatherAssetBase> newWeather, ENPCWeatherStatus newValue, ENPCLogicType newLogicType, string newText) : base(newLogicType, newText, false)
		{
			weather = newWeather;
			value = newValue;
		}
	}
}
