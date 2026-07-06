////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuOverridableObjects : MonoBehaviour
	{
		[Tooltip("Point of view when menu first loads. Blends into Title Camera.")]
		public Camera initialCamera;
		[Tooltip("Point of view with the game name and news feed.")]
		public Camera titleCamera;
		public Camera playCamera;
		public Camera survivorsCamera;
		public Camera optionsCamera;
		public Camera workshopCamera;
		public Transform playerCharacterTransform;

#if GAME
		internal static event System.Action<MenuOverridableObjects> OnMenuOverridesApplied;

		private void Awake()
		{
			bool isDestination = GetComponent<MenuStartup>();
			if (isDestination)
			{
				destinationInstance = this;
			}
			else
			{
				if (destinationInstance != null)
				{
					ApplyMenuOverrides(this, destinationInstance);
					OnMenuOverridesApplied?.Invoke(this);
				}
				else
				{
					UnturnedLog.warn("MenuOverridableObjects without destination");
				}
			}
		}

		private void OnDestroy()
		{
			if (destinationInstance == this)
			{
				destinationInstance = null;
			}
		}

		private void ApplyMenuOverrides(MenuOverridableObjects source, MenuOverridableObjects destination)
		{
			ApplyOverride(source.initialCamera, destination.initialCamera);
			ApplyOverride(source.titleCamera, destination.titleCamera);
			ApplyOverride(source.playCamera, destination.playCamera);
			ApplyOverride(source.survivorsCamera, destination.survivorsCamera);
			ApplyOverride(source.optionsCamera, destination.optionsCamera);
			ApplyOverride(source.workshopCamera, destination.workshopCamera);

			ApplyOverride(source.playerCharacterTransform, destination.playerCharacterTransform);
		}

		private void ApplyOverride(Camera sourceCamera, Camera destinationCamera)
		{
			sourceCamera.enabled = false;
			ApplyOverride(sourceCamera.transform, destinationCamera.transform);
		}

		private void ApplyOverride(Transform sourceTransform, Transform destinationTransform)
		{
			sourceTransform.gameObject.SetActive(false);
			sourceTransform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
			destinationTransform.SetPositionAndRotation(position, rotation);
		}

		private static MenuOverridableObjects destinationInstance;
#endif // GAME
	}
}
