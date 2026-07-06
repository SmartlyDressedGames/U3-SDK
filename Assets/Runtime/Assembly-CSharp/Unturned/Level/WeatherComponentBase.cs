////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Unturned
{
	public class WeatherComponentBase : MonoBehaviour
	{
		public WeatherAssetBase asset;

		/// <summary>
		/// [0, 1] blends towards one while active regardless of local volume.
		/// </summary>
		public float globalBlendAlpha;

		/// <summary>
		/// [0, 1] blends towards one if current volume bitwise AND with asset is non-zero.
		/// </summary>
		public float localVolumeBlendAlpha;

		/// <summary>
		/// Lesser of global or volume blend alphas. 
		/// </summary>
		public float EffectBlendAlpha => Mathf.Min(globalBlendAlpha, localVolumeBlendAlpha);

		public bool isWeatherActive;

		/// <summary>
		/// If blending was not ticket yet then local blend can use global value, e.g. loading into rain storm.
		/// </summary>
		public bool hasTickedBlending;

		/// <summary>
		/// Is blendAlpha at 100%?
		/// </summary>
		public bool isFullyTransitionedIn;

		public Color fogColor;
		public float fogDensity;

		public bool overrideFog;
		public bool overrideAtmosphericFog;
		public bool overrideCloudColors;

		public Color cloudColor;
		public Color cloudRimColor;

		/// <summary>
		/// [0, 1] Rain puddle alpha cutoff.
		/// </summary>
		public float puddleWaterLevel;
		/// <summary>
		/// [0, 1] Rain puddle ripples alpha.
		/// </summary>
		public float puddleIntensity;

		public float brightnessMultiplier = 1.0f;
		public float shadowStrengthMultiplier = 1.0f;
		public float fogBlendExponent = 1.0f;
		public float cloudBlendExponent = 1.0f;
		public float windMain;

		public NetId GetNetId()
		{
			return netId;
		}

#if !DEDICATED_SERVER
		public AudioSource ambientAudioSource;
#endif // !DEDICATED_SERVER

		public virtual void InitializeWeather()
		{
#if !DEDICATED_SERVER
			if (Dedicator.IsDedicatedServer)
				return;

			if (asset.ambientAudio.isValid)
			{
				StartCoroutine(AsyncLoadAmbientAudio());
			}
#endif // !DEDICATED_SERVER
		}

		public virtual void UpdateWeather() { }

		public virtual void UpdateLightingTime(int blendKey, int currentKey, float timeAlpha) { }

		public virtual void PreDestroyWeather() { }

		public virtual void OnBeginTransitionIn() { }
		public virtual void OnEndTransitionIn() { }
		public virtual void OnBeginTransitionOut() { }
		public virtual void OnEndTransitionOut() { }

		public System.Collections.Generic.IEnumerable<Player> EnumerateMaskedPlayers()
		{
			foreach (Player player in PlayerTool.EnumeratePlayers())
			{
				if ((player.movement.WeatherMask & asset.volumeMask) != 0)
					yield return player;
			}
		}

#if !DEDICATED_SERVER
		private IEnumerator AsyncLoadAmbientAudio()
		{
			AssetBundleRequest request = asset.ambientAudio.LoadAssetAsync();
			if (request == null)
				yield break;

			yield return request;

			AudioClip ambientAudioClip = request.asset as AudioClip;
			if (ambientAudioClip != null)
			{
				ambientAudioSource = gameObject.AddComponent<AudioSource>();
				ambientAudioSource.loop = true;
				ambientAudioSource.playOnAwake = false;
				ambientAudioSource.volume = 0.0f;
				ambientAudioSource.spatialBlend = 0.0f; // 2D
				ambientAudioSource.clip = ambientAudioClip;
				ambientAudioSource.Play();
				ambientAudioSource.dopplerLevel = 0.0f;
				ambientAudioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetDefaultGroup();
			}
		}
#endif // !DEDICATED_SERVER

		internal NetId netId;
	}
}
