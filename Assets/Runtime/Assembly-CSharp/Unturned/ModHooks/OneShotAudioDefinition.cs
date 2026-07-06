////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	[CreateAssetMenu(fileName = "OneShotAudioDef", menuName = "Unturned/One Shot Audio Def")]
	public class OneShotAudioDefinition : ScriptableObject
	{
		public List<AudioClip> clips;
		public float volumeMultiplier = 1.0f;
		public float minPitch = 0.95f;
		public float maxPitch = 1.05f;

		public AudioClip GetRandomClip()
		{
			if (clips == null)
				return null;

			switch (clips.Count)
			{
				case 0:
					return null;

				case 1:
					return clips[0];

				case 2:
					// Shuffling to prevent repeats would sound bad with only two clips.
					return clips[Random.Range(0, 2)];

				default:
					return GetRandomShuffledClip();
			}
		}

		private AudioClip GetRandomShuffledClip()
		{
			List<AudioClip> shuffledClips;
#if UNITY_EDITOR
			if (editorClips == null)
			{
				editorClips = new List<AudioClip>(clips);
			}
			shuffledClips = editorClips;
#else
			shuffledClips = clips;
#endif // UNITY_EDITOR

			if (shuffledClipIndex < 0)
			{
				// Clips have not been shuffled yet.
				ShuffleClips(shuffledClips);
				shuffledClipIndex = 0;
			}
			else if (shuffledClipIndex >= shuffledClips.Count)
			{
				ReshuffleClips(shuffledClips);
				shuffledClipIndex = 0;
			}

			return shuffledClips[shuffledClipIndex++];
		}

		private void Swap(List<AudioClip> list, int lhsIndex, int rhsIndex)
		{
			AudioClip temp = list[rhsIndex];
			list[rhsIndex] = list[lhsIndex];
			list[lhsIndex] = temp;
		}

		/// <summary>
		/// Durstenfeld version of Fisher-Yates shuffle:
		/// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_modern_algorithm
		/// </summary>
		private void ShuffleClips(List<AudioClip> list)
		{
			for (int destinationIndex = list.Count - 1; destinationIndex > 0; --destinationIndex)
			{
				int sourceIndex = Random.Range(0, destinationIndex + 1); // Inclusive
				Swap(list, sourceIndex, destinationIndex);
			}
		}

		/// <summary>
		/// Same as above, but prevent the last clip from being shuffled to the front in order to prevent repeats.
		/// </summary>
		private void ReshuffleClips(List<AudioClip> list)
		{
			Swap(list, 0, Random.Range(0, list.Count - 1)); // Exclude final clip.

			for (int destinationIndex = list.Count - 1; destinationIndex > 1; --destinationIndex)
			{
				int sourceIndex = Random.Range(1, destinationIndex + 1); // Inclusive
				Swap(list, sourceIndex, destinationIndex);
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Ensures we do not modify the asset in editor.
		/// </summary>
		[System.NonSerialized]
		private List<AudioClip> editorClips;
#endif // UNITY_EDITOR

		[System.NonSerialized]
		private int shuffledClipIndex = -1;
	}
}
