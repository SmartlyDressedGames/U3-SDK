////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using UnityEngine;
using UnityEngine.Audio;

namespace SDG.Unturned
{
	internal static class UnturnedAudioMixer
	{
		public static AudioMixerGroup GetDefaultGroup()
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			return defaultGroup;
		}

		public static AudioMixerGroup GetMusicGroup()
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			return musicGroup;
		}

		public static AudioMixerGroup GetAtmosphereGroup()
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			return atmosphereGroup;
		}

		public static AudioMixerGroup GetZombieFootstepsGroup()
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			return zombieFootstepsGroup;
		}

		public static void SetDefaultVolume(float linearVolume)
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			bool found = mainMix.SetFloat("DefaultVolume", LinearToDb(linearVolume));
			Debug.Assert(found);
		}

		public static void SetVoiceVolume(float linearVolume)
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			bool found = mainMix.SetFloat("VoiceVolume", LinearToDb(linearVolume));
			Debug.Assert(found);
		}

		public static void SetMusicMasterVolume(float linearVolume)
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			bool found = mainMix.SetFloat("MusicVolume", LinearToDb(linearVolume));
			Debug.Assert(found);
		}

		public static void SetMainMenuMusicVolume(float linearVolume)
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			bool found = mainMix.SetFloat("MainMenuMusicVolume", LinearToDb(linearVolume));
			Debug.Assert(found);
		}

		public static void SetAtmosphereVolume(float linearVolume)
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			bool found = mainMix.SetFloat("AtmosphereVolume", LinearToDb(linearVolume));
			Debug.Assert(found);
		}

		public static void SetZombieFootstepsVolume(float linearVolume)
		{
			if (!hasBeenInitialized)
			{
				Initialize();
			}

			bool found = mainMix.SetFloat("ZombieFootstepsVolume", LinearToDb(linearVolume));
			Debug.Assert(found);
		}

		private static float LinearToDb(float linearVolume)
		{
			if (linearVolume < 0.0001f)
			{
				return -80.0f;
			}
			else
			{
				return Mathf.Log10(linearVolume) * 20.0f;
			}
		}

		private static void Initialize()
		{
			hasBeenInitialized = true;

			mainMix = Resources.Load<AudioMixer>("Sounds/MainMix");
			Debug.Assert(mainMix != null);
			defaultGroup = mainMix.FindMatchingGroups("Default")[0];
			Debug.Assert(defaultGroup != null);
			musicGroup = mainMix.FindMatchingGroups("Music")[0];
			Debug.Assert(musicGroup != null);
			atmosphereGroup = mainMix.FindMatchingGroups("Atmosphere")[0];
			Debug.Assert(atmosphereGroup != null);
			zombieFootstepsGroup = mainMix.FindMatchingGroups("ZombieFootsteps")[0];
			Debug.Assert(zombieFootstepsGroup != null);
		}

		private static bool hasBeenInitialized;
		private static AudioMixer mainMix;
		private static AudioMixerGroup defaultGroup;
		private static AudioMixerGroup musicGroup;
		private static AudioMixerGroup atmosphereGroup;
		private static AudioMixerGroup zombieFootstepsGroup;
	}
}
#endif // !DEDICATED_SERVER
