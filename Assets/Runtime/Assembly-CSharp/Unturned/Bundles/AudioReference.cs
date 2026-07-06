////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public struct AudioReference
	{
		public AudioReference(string assetBundleName, string path)
		{
			this.assetBundleName = assetBundleName;
			this.path = path;
		}

		public bool IsNullOrEmpty => string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(path);

		public AudioClip LoadAudioClip(out float volumeMultiplier, out float pitchMultiplier)
		{
			Profiler.BeginSample("AudioReference.LoadAudioClip");
			AudioClip audioClip;
			volumeMultiplier = 1.0f;
			pitchMultiplier = 1.0f;

			if (IsNullOrEmpty)
			{
				audioClip = null;
			}
			else
			{
				MasterBundleConfig config = Assets.findMasterBundleByName(assetBundleName);
				if (config == null || config.assetBundle == null)
				{
					UnturnedLog.warn("Unable to find master bundle '{0}' when loading audio reference '{1}'", assetBundleName, path);
					audioClip = null;
				}
				else
				{
					string formattedPath = config.FormatAssetPathAndCache(path);
					if (formattedPath.EndsWith(".asset", System.StringComparison.Ordinal))
					{
						OneShotAudioDefinition audioDef = config.assetBundle.LoadAsset<OneShotAudioDefinition>(formattedPath);
						if (audioDef == null)
						{
							UnturnedLog.warn("Failed to load audio def '{0}' from master bundle '{1}'", formattedPath, assetBundleName);
							audioClip = null;
						}
						else
						{
							volumeMultiplier = audioDef.volumeMultiplier;
							pitchMultiplier = Random.Range(audioDef.minPitch, audioDef.maxPitch);
							audioClip = audioDef.GetRandomClip();
						}
					}
					else
					{
						audioClip = config.assetBundle.LoadAsset<AudioClip>(formattedPath);
						if (audioClip == null)
						{
							UnturnedLog.warn("Failed to load audio clip '{0}' from master bundle '{1}'", formattedPath, assetBundleName);
						}
					}
				}
			}
			Profiler.EndSample();
			return audioClip;
		}

		public AudioClip LoadAudioClip()
		{
			return LoadAudioClip(out float _unusedVolume, out float _unusedPitch);
		}

		private string assetBundleName;
		private string path;
	}
}
