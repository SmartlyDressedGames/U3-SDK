////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Code common to <see cref="DefaultEngineSoundController"/> and <see cref="RpmEngineSoundController"/>.
	/// </summary>
	internal abstract class DefaultEngineSoundControllerBase : MonoBehaviour
	{
		private void Awake()
		{
			engineAudioSource = GetComponent<AudioSource>();
			if (engineAudioSource != null)
			{
				engineAudioSource.maxDistance *= 2;
			}

			Transform additive = transform.Find("Engine_Additive");
			if (additive != null)
			{
				AudioSource engineAdditiveAudioSource = additive.GetComponent<AudioSource>();
				if (engineAdditiveAudioSource != null)
				{
					engineAdditiveAudioSource.maxDistance *= 2;

					engineAdditiveAudioSources = new List<AudioSource>(1)
					{
						engineAdditiveAudioSource,
					};
				}
			}
		}

		protected virtual void Start()
		{
			vehicle.onPassengersUpdated += OnPassengersUpdated;
			vehicle.OnIsDrownedChanged += OnIsDrownedChanged;
			wasDriven = vehicle.isDriven;

			if (vehicle.trainCars != null)
			{
				for (int index = 1; index < vehicle.trainCars.Length; ++index)
				{
					TrainCar trainCar = vehicle.trainCars[index];
					Transform additive = trainCar.root.Find("Engine_Additive");
					if (additive != null)
					{
						AudioSource engineAdditiveAudioSource = additive.GetComponent<AudioSource>();
						if (engineAdditiveAudioSource != null)
						{
							engineAdditiveAudioSources.Add(engineAdditiveAudioSource);
						}
					}
				}
			}

			ResetPitchToDefault();
			SynchronizeEnabled();
		}

		private void OnDestroy()
		{
			vehicle.onPassengersUpdated -= OnPassengersUpdated;
			vehicle.OnIsDrownedChanged -= OnIsDrownedChanged;
		}

		protected abstract float DefaultPitch
		{ get; }

		private void OnPassengersUpdated()
		{
			bool isDriven = vehicle.isDriven;
			if (wasDriven != isDriven)
			{
				wasDriven = isDriven;
				if (isDriven)
				{
					ResetPitchToDefault();
				}

				SynchronizeEnabled();
			}
		}

		private void OnIsDrownedChanged()
		{
			SynchronizeEnabled();
		}

		private void ResetPitchToDefault()
		{
			if (engineAudioSource != null)
			{
				engineAudioSource.pitch = DefaultPitch;
			}

			if (engineAdditiveAudioSources != null)
			{
				foreach (AudioSource audioSource in engineAdditiveAudioSources)
				{
					if (audioSource != null)
					{
						audioSource.pitch = DefaultPitch;
					}
				}
			}
		}

		private void SynchronizeEnabled()
		{
			if (vehicle.isDriven && !vehicle.isDrowned)
			{
				if (engineAudioSource != null)
				{
					engineAudioSource.enabled = true;
				}

				if (engineAdditiveAudioSources != null)
				{
					foreach (AudioSource audioSource in engineAdditiveAudioSources)
					{
						if (audioSource != null)
						{
							audioSource.enabled = true;
						}
					}
				}

				enabled = true;
			}
			else
			{
				if (engineAudioSource != null)
				{
					engineAudioSource.volume = 0;
					engineAudioSource.enabled = false;
				}

				if (engineAdditiveAudioSources != null)
				{
					foreach (AudioSource audioSource in engineAdditiveAudioSources)
					{
						if (audioSource != null)
						{
							audioSource.volume = 0;
							audioSource.enabled = false;
						}
					}
				}

				enabled = false;
			}
		}

		internal InteractableVehicle vehicle;
		protected bool wasDriven;

		protected AudioSource engineAudioSource;
		protected List<AudioSource> engineAdditiveAudioSources;
	}
}
#endif // !DEDICATED_SERVER
