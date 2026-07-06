////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using UnityEngine;

namespace SDG.Unturned
{
	internal class RpmEngineSoundController : DefaultEngineSoundControllerBase
	{
		protected override void Start()
		{
			// Needs to be set before base.Start() because fake OnPassengersUpdated needs our DefaultPitch override.
			soundConfiguration = vehicle.asset.engineSoundConfiguration;
			base.Start();
		}

		private void Update()
		{
			VehicleAsset asset = vehicle.asset;
			float prefVolume = Provider.preferenceData != null ? Provider.preferenceData.Audio.Vehicle_Engine_Volume_Multiplier : 1.0f;

			float normalizedRpm = Mathf.InverseLerp(asset.EngineIdleRpm, asset.EngineMaxRpm, vehicle.AnimatedEngineRpm);
			float targetPitch = Mathf.Lerp(soundConfiguration.idlePitch, soundConfiguration.maxPitch, normalizedRpm);
			float targetVolume = Mathf.Lerp(soundConfiguration.idleVolume, soundConfiguration.maxVolume, normalizedRpm);

			if (engineAudioSource != null)
			{
				engineAudioSource.pitch = targetPitch;
				// Keep old interpolation here to fade in from "ignition" audio clip.
				engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? targetVolume : 0, 2 * Time.deltaTime) * prefVolume;
			}
		}

		protected override float DefaultPitch => soundConfiguration.idlePitch;

		private RpmEngineSoundConfiguration soundConfiguration;
	}
}
#endif // !DEDICATED_SERVER
