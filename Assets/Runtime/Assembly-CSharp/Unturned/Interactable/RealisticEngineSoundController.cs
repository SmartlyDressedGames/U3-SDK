////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// Nelson 2024-06-17: Deciding against using this asset for the meantime. Prioritizing getting vehicles working with
// a forward gear ratio and reverse gear ratio first.
#if !DEDICATED_SERVER && WITH_RES2
using SkrilStudio;
using UnityEngine;

namespace SDG.Unturned
{
	internal class RealisticEngineSoundController : MonoBehaviour
	{
		private void Awake()
		{
			AudioSource defaultEngineAudioSource = GetComponent<AudioSource>();
			if (defaultEngineAudioSource != null)
			{
				Destroy(defaultEngineAudioSource);
			}

			Transform additive = transform.Find("Engine_Additive");
			if (additive != null)
			{
				Destroy(additive.gameObject);
			}
		}

		private System.Collections.IEnumerator Start()
		{
			string path = "Diesel_2.5_German_HQ";
			ResourceRequest rr = Resources.LoadAsync<GameObject>(path);
			yield return rr;
			GameObject instance = Instantiate(rr.asset as GameObject, transform, false);
			instance.transform.parent = transform;
			instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			res = instance.GetComponent<RealisticEngineSound>();
			res.windNoiseEnabled = DynamicSoundController.WindNoiseEnum.Off;
			res.dynamicAudioMixer = RealisticEngineSound.DynamicAudioMixer.Off;
			res.maxRPMLimit = vehicle.asset.engineMaxRpm;

			if (MainCamera.instance != null)
			{
				res.audioListener = MainCamera.instance.GetComponent<AudioListener>();
			}
			else
			{
				res.audioListener = Level.placeholderAudioListener;

				isWaitingForAudioListener = true;
				MainCamera.instanceChanged += OnMainCameraInstanceChanged;
			}
		}

		private void OnDestroy()
		{
			if (isWaitingForAudioListener)
			{
				isWaitingForAudioListener = false;
				MainCamera.instanceChanged -= OnMainCameraInstanceChanged;
			}
		}

		private void OnMainCameraInstanceChanged()
		{
			isWaitingForAudioListener = false;
			MainCamera.instanceChanged -= OnMainCameraInstanceChanged;
			if (res != null && MainCamera.instance != null)
			{
				res.audioListener = MainCamera.instance.GetComponent<AudioListener>();
			}
		}

		private void Update()
		{
			if (res == null)
				return;

			res.engineCurrentRPM = vehicle.engineRpm;
			res.gasPedalPressing = !MathfEx.IsNearlyZero(vehicle.latestGasInput);
		}

		internal InteractableVehicle vehicle;

		private RealisticEngineSound res;
		private bool isWaitingForAudioListener;
	}
}
#endif // !DEDICATED_SERVER && WITH_RES2
