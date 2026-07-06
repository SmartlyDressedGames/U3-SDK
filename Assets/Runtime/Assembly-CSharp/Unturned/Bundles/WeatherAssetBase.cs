////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class WeatherAssetBase : Asset
	{
		public static readonly AssetReference<WeatherAssetBase> DEFAULT_SNOW = new AssetReference<WeatherAssetBase>("903577da2ecd4f5784b2f7aed8c300c1");
		public static readonly AssetReference<WeatherAssetBase> DEFAULT_RAIN = new AssetReference<WeatherAssetBase>("d73923f4416c43dfa5bc8b6234cf0257");

		/// <summary>
		/// Seconds between weather event starting and reaching full intensity.
		/// </summary>
		public float fadeInDuration
		{
			get;
			protected set;
		}

		/// <summary>
		/// Seconds between weather event ending and reaching zero intensity.
		/// </summary>
		public float fadeOutDuration
		{
			get;
			protected set;
		}

		/// <summary>
		/// Sound clip to play. Volume matches the intensity.
		/// </summary>
		public MasterBundleReference<AudioClip> ambientAudio
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, this weather's ambientAudio is higher priority than volumes.
		/// </summary>
		public bool IsAudioHigherPriorityThanAmbianceVolumes
		{
			get;
			set;
		}

		/// <summary>
		/// Component to spawn for additional weather logic.
		/// </summary>
		public System.Type componentType
		{
			get;
			protected set;
		}

		/// <summary>
		/// If per-volume mask AND is non zero the weather will blend in.
		/// </summary>
		public uint volumeMask
		{
			get;
			protected set;
		}

		public bool hasLightning
		{
			get;
			protected set;
		}

		public float minLightningInterval;
		public float maxLightningInterval;
		public float lightningTargetRadius;

		/// <summary>
		/// Multiplier for interval before a fish takes the bait.
		/// Defaults to 1.
		/// </summary>
		public float FishBiteIntervalMultiplier
		{
			get;
			set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			fadeInDuration = p.data.ParseFloat("Fade_In_Duration");
			fadeOutDuration = p.data.ParseFloat("Fade_Out_Duration");

			ambientAudio = p.data.ParseStruct<MasterBundleReference<AudioClip>>("Ambient_Audio_Clip");
			IsAudioHigherPriorityThanAmbianceVolumes = p.data.ParseBool("Ambient_Audio_Takes_Priority_Over_Ambiance_Volumes");

			componentType = p.data.ParseType("Component_Type");
			if (componentType == null)
			{
				componentType = typeof(WeatherComponentBase);
			}

			if (p.data.ContainsKey("Volume_Mask"))
			{
				volumeMask = p.data.ParseUInt32("Volume_Mask");
			}
			else
			{
				volumeMask = uint.MaxValue;
			}

			hasLightning = p.data.ParseBool("Has_Lightning");
			if (hasLightning)
			{
				minLightningInterval = Mathf.Max(5.0f, p.data.ParseFloat("Min_Lightning_Interval"));
				maxLightningInterval = Mathf.Max(5.0f, p.data.ParseFloat("Max_Lightning_Interval"));
				if (p.data.ContainsKey("Lightning_Target_Radius"))
					lightningTargetRadius = Mathf.Max(0.0f, p.data.ParseFloat("Lightning_Target_Radius"));
				else
					lightningTargetRadius = 500.0f;
			}

			FishBiteIntervalMultiplier = p.data.ParseFloat("Fish_Bite_Interval_Multiplier", 1f);
		}
	}
}
