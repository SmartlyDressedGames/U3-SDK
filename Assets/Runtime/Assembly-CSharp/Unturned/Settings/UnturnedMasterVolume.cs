////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Helper wrapping Unturned's usage of AudioListener.volume, which is the master volume level.
	/// This makes it easier to track what controls the master volume and avoid bugs.
	/// </summary>
	public static class UnturnedMasterVolume
	{
		/// <summary>
		/// Is audio muted because this is a dedicated server?
		/// 
		/// While dedicated server should not even be processing audio code,
		/// older versions of Unity in particular have issues with headless audio.
		/// </summary>
		public static bool mutedByDedicatedServer
		{
			get => internalMutedByDedicatedServer;
			set
			{
				internalMutedByDedicatedServer = value;
				synchronizeAudioListener();
			}
		}

		/// <summary>
		/// Is audio muted because loading screen is visible?
		/// </summary>
		public static bool mutedByLoadingScreen
		{
			get => internalMutedByLoadingScreen;
			set
			{
				// Only synchronize if changed, as this is called every frame.
				if (value != internalMutedByLoadingScreen)
				{
					internalMutedByLoadingScreen = value;
					synchronizeAudioListener();
				}
			}
		}

		/// <summary>
		/// Player's volume multiplier from the options menu.
		/// </summary>
		public static float preferredVolume
		{
			get => internalPreferredVolume;
			set
			{
				if (internalPreferredVolume != value)
				{
					internalPreferredVolume = value;
					synchronizeAudioListener();
				}
			}
		}

		/// <summary>
		/// Player's unfocused volume multiplier from the options menu.
		/// </summary>
		public static float UnfocusedVolume
		{
			get => internalUnfocusedVolumeMultiplier;
			set
			{
				if (internalUnfocusedVolumeMultiplier != value)
				{
					internalUnfocusedVolumeMultiplier = value;
					synchronizeAudioListener();
				}
			}
		}

		/// <summary>
		/// Mute or un-mute audio depending whether camera is valid.
		/// </summary>
		private static void handleMainCameraAvailabilityChanged()
		{
			mutedByCamera = !MainCamera.isAvailable;
			synchronizeAudioListener();
		}

		private static void OnApplicationFocusChanged(bool hasFocus)
		{
			synchronizeAudioListener();
		}

		/// <summary>
		/// Synchronize AudioListener.volume with Unturned's parameters.
		/// </summary>
		private static void synchronizeAudioListener()
		{
			float masterVolume;
			bool muted = internalMutedByDedicatedServer || internalMutedByLoadingScreen || mutedByCamera;
			if (muted)
			{
				masterVolume = 0.0f;
			}
			else
			{
				masterVolume = internalPreferredVolume;
				if (!Application.isFocused)
				{
					masterVolume *= internalUnfocusedVolumeMultiplier;
				}
			}
			AudioListener.volume = masterVolume;
		}

		private static bool internalMutedByDedicatedServer = true;
		private static bool internalMutedByLoadingScreen = true;
		private static bool mutedByCamera = true;
		private static float internalPreferredVolume = OptionsSettings.DEFAULT_MASTER_VOLUME;
		private static float internalUnfocusedVolumeMultiplier = OptionsSettings.DEFAULT_UNFOCUSED_VOLUME;

		static UnturnedMasterVolume()
		{
			// Synchronize listener volume on first use.
			synchronizeAudioListener();

			MainCamera.availabilityChanged += handleMainCameraAvailabilityChanged;
			Application.focusChanged += OnApplicationFocusChanged;
		}
	}
}
