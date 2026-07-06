////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows map makers to create custom weather events.
	/// </summary>
	public class /*Custom*/WeatherAsset : WeatherAssetBase
	{
		/// <summary>
		/// Does this weather affect fog color and density?
		/// </summary>
		public bool overrideFog
		{
			get;
			protected set;
		}

		/// <summary>
		/// Does this weather affect sky fog color?
		/// </summary>
		public bool overrideAtmosphericFog
		{
			get;
			protected set;
		}

		/// <summary>
		/// Does this weather affect cloud colors?
		/// </summary>
		public bool overrideCloudColors
		{
			get;
			protected set;
		}

		/// <summary>
		/// Directional light shadow strength multiplier.
		/// </summary>
		public float shadowStrengthMultiplier;

		/// <summary>
		/// Exponent applied to effect blend alpha.
		/// </summary>
		public float fogBlendExponent;

		/// <summary>
		/// Exponent applied to effect blend alpha.
		/// </summary>
		public float cloudBlendExponent;

		/// <summary>
		/// SpeedTree wind strength for blizzard. Should be removed?
		/// </summary>
		public float windMain;

		public struct WeatherColor
		{
			public Color customColor;

			/// <summary>
			/// If specified level editor color can be used rather than a per-asset color.
			/// </summary>
			public ELightingColor levelEnum;

			public WeatherColor(IDatDictionary data)
			{
				if (data == null)
				{
					customColor = Color.black;
					levelEnum = ELightingColor.CUSTOM_OVERRIDE;
					return;
				}

				byte r = data.ContainsKey("R") ? data.ParseUInt8("R") : byte.MaxValue;
				byte g = data.ContainsKey("G") ? data.ParseUInt8("G") : byte.MaxValue;
				byte b = data.ContainsKey("B") ? data.ParseUInt8("B") : byte.MaxValue;
				customColor = new Color32(r, g, b, byte.MaxValue);

				if (data.ContainsKey("Level_Enum"))
				{
					levelEnum = data.ParseEnum<ELightingColor>("Level_Enum");
				}
				else
				{
					levelEnum = ELightingColor.CUSTOM_OVERRIDE;
				}
			}

			public Color Evaluate(LightingInfo levelValues)
			{
				switch (levelEnum)
				{
					default:
						return levelValues.colors[(int) levelEnum] * customColor;

					case ELightingColor.CUSTOM_OVERRIDE:
						return customColor;
				}
			}
		}

		public class TimeValues
		{
			public WeatherColor fogColor;
			public float fogDensity;
			public WeatherColor cloudColor;
			public WeatherColor cloudRimColor;
			public float brightnessMultiplier;

			public TimeValues(IDatDictionary data)
			{
				if (data == null)
				{
					brightnessMultiplier = 1.0f;
					return;
				}

				fogColor = new WeatherColor(data.GetDictionary("Fog_Color"));
				fogDensity = data.ParseFloat("Fog_Density");
				cloudColor = new WeatherColor(data.GetDictionary("Cloud_Color"));
				cloudRimColor = new WeatherColor(data.GetDictionary("Cloud_Rim_Color"));
				if (data.ContainsKey("Brightness_Multiplier"))
				{
					brightnessMultiplier = data.ParseFloat("Brightness_Multiplier");
				}
				else
				{
					brightnessMultiplier = 1.0f;
				}
			}
		}

		public struct Effect : IDatParseable
		{
			public MasterBundleReference<GameObject> prefab;
			public float emissionExponent;
			public float pitch;
			public bool translateWithView;
			public bool rotateYawWithWind;

			public bool TryParse(IDatNode node)
			{
				if (!(node is IDatDictionary dictionary))
					return false;

				prefab = dictionary.ParseStruct<MasterBundleReference<GameObject>>("Prefab");
				emissionExponent = dictionary.ParseFloat("Emission_Exponent");
				pitch = dictionary.ParseFloat("Pitch");
				translateWithView = dictionary.ParseBool("Translate_With_View");
				rotateYawWithWind = dictionary.ParseBool("Rotate_Yaw_With_Wind");
				return true;
			}
		}

		public float staminaPerSecond;
		public float healthPerSecond;
		public float foodPerSecond;
		public float waterPerSecond;
		public float virusPerSecond;

		public Effect[] effects;

		protected TimeValues[] timeValues;

		public void getTimeValues(int blendKey, int currentKey, out TimeValues blendFrom, out TimeValues blendTo)
		{
			blendTo = timeValues[currentKey];
			blendFrom = blendKey == -1 ? blendTo : timeValues[blendKey];
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (componentType == typeof(WeatherComponentBase))
			{
				// Asset did not specify a custom type.
				componentType = typeof(CustomWeatherComponent);
			}

			overrideFog = p.data.ParseBool("Override_Fog");
			overrideAtmosphericFog = p.data.ParseBool("Override_Atmospheric_Fog");
			overrideCloudColors = p.data.ParseBool("Override_Cloud_Colors");

			if (p.data.ContainsKey("Shadow_Strength_Multiplier"))
			{
				shadowStrengthMultiplier = p.data.ParseFloat("Shadow_Strength_Multiplier");
			}
			else
			{
				shadowStrengthMultiplier = 1.0f;
			}

			if (p.data.ContainsKey("Fog_Blend_Exponent"))
			{
				fogBlendExponent = p.data.ParseFloat("Fog_Blend_Exponent");
			}
			else
			{
				fogBlendExponent = 1.0f;
			}

			if (p.data.ContainsKey("Cloud_Blend_Exponent"))
			{
				cloudBlendExponent = p.data.ParseFloat("Cloud_Blend_Exponent");
			}
			else
			{
				cloudBlendExponent = 1.0f;
			}

			windMain = p.data.ParseFloat("Wind_Main");

			staminaPerSecond = p.data.ParseFloat("Stamina_Per_Second");
			healthPerSecond = p.data.ParseFloat("Health_Per_Second");
			foodPerSecond = p.data.ParseFloat("Food_Per_Second");
			waterPerSecond = p.data.ParseFloat("Water_Per_Second");
			virusPerSecond = p.data.ParseFloat("Virus_Per_Second");

			timeValues = new TimeValues[4];
			timeValues[(int) ELightingTime.DAWN] = new TimeValues(p.data.GetDictionary("Dawn"));
			timeValues[(int) ELightingTime.MIDDAY] = new TimeValues(p.data.GetDictionary("Midday"));
			timeValues[(int) ELightingTime.DUSK] = new TimeValues(p.data.GetDictionary("Dusk"));
			timeValues[(int) ELightingTime.MIDNIGHT] = new TimeValues(p.data.GetDictionary("Midnight"));

			if (p.data.TryGetList("Effects", out IDatList effectsList))
			{
				effects = new Effect[effectsList.Count];
				for (int index = 0; index < effectsList.Count; ++index)
				{
					effects[index] = effectsList[index].ParseStruct<Effect>();
				}
			}
		}
	}
}
