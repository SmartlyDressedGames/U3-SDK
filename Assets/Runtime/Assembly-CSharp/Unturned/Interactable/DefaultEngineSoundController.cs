////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using UnityEngine;

namespace SDG.Unturned
{
	internal class DefaultEngineSoundController : DefaultEngineSoundControllerBase
	{
		private void Update()
		{
			VehicleAsset asset = vehicle.asset;
			float delta = Time.deltaTime;

			float prefVolume = Provider.preferenceData != null ? Provider.preferenceData.Audio.Vehicle_Engine_Volume_Multiplier : 1.0f;

			float audioSpeed;
			if (asset.engine == EEngine.CAR || asset.engine == EEngine.BOAT)
			{
				audioSpeed = Mathf.Abs(vehicle.AnimatedForwardVelocity);
			}
			else
			{
				audioSpeed = Mathf.Abs(vehicle.AnimatedVelocityInput);
			}

			if (asset.engine == EEngine.HELICOPTER)
			{
				if (engineAudioSource != null)
				{
					engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, asset.pitchIdle + (audioSpeed * asset.pitchDrive), 2 * delta);
					engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? 0.25f + (audioSpeed * 0.03f) : 0, 0.125f * delta) * prefVolume;
				}
			}
			else if (asset.engine == EEngine.BLIMP)
			{
				if (engineAudioSource != null)
				{
					engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, asset.pitchIdle + (audioSpeed * asset.pitchDrive), 2 * delta);
					engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? 0.25f + (audioSpeed * 0.1f) : 0, 0.125f * delta) * prefVolume;
				}
			}
			else
			{
				if (engineAudioSource != null)
				{
					engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, asset.pitchIdle + (audioSpeed * asset.pitchDrive), 2 * delta);
					engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? 0.75f : 0, 2 * delta) * prefVolume;
				}
				if (engineAdditiveAudioSources != null)
				{
					foreach (AudioSource audioSource in engineAdditiveAudioSources)
					{
						if (audioSource != null)
						{
							audioSource.pitch = Mathf.Lerp(audioSource.pitch, asset.pitchIdle + (audioSpeed * asset.pitchDrive), 2 * delta);
							audioSource.volume = Mathf.Lerp(audioSource.volume, Mathf.Lerp(0, 0.75f, audioSpeed / 8), 2 * delta) * prefVolume;
						}
					}
				}
			}
		}

		protected override float DefaultPitch => vehicle.asset.pitchIdle;
	}
}
#endif // !DEDICATED_SERVER
