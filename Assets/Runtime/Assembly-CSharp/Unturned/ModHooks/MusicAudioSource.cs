////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	[AddComponentMenu("Unturned/Music Audio Source")]
	[RequireComponent(typeof(AudioSource))]
	[Tooltip("Reassigns AudioSource's outputAudioMixerGroup to the vanilla Music group")]
	public class MusicAudioSource : MonoBehaviour
	{
		private void Awake()
		{
#if GAME && !DEDICATED_SERVER
			AudioSource audioSource = GetComponent<AudioSource>();
			if (audioSource != null)
			{
				audioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetMusicGroup();
			}
#endif // GAME && !DEDICATED_SERVER
		}
	}
}
