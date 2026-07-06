////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to any GameObject to receive weather events for a specific custom weather asset.
	/// </summary>
	[AddComponentMenu("Unturned/Custom Weather Event Hook")]
	public class CustomWeatherEventHook : MonoBehaviour
	{
		/// <summary>
		/// GUID of custom weather asset to listen for.
		/// </summary>
		public string WeatherAssetGuid;

		/// <summary>
		/// Invoked when custom weather is activated, or immediately if weather is fading in when registered.
		/// </summary>
		public UnityEvent OnWeatherBeginTransitionIn;

		/// <summary>
		/// Invoked when custom weather finishes fading in, or immediately if weather is already fully active when registered.
		/// </summary>
		public UnityEvent OnWeatherEndTransitionIn;

		/// <summary>
		/// Invoked when custom weather is deactivated and begins fading out.
		/// </summary>
		public UnityEvent OnWeatherBeginTransitionOut;

		/// <summary>
		/// Invoked when custom weather finishes fading out and is destroyed.
		/// </summary>
		public UnityEvent OnWeatherEndTransitionOut;

		protected void OnEnable()
		{
#if GAME
			if (System.Guid.TryParse(WeatherAssetGuid, out parsedGuid))
			{
				WeatherEventListenerManager.AddComponentListener(parsedGuid, this);
			}
			else
			{
				parsedGuid = System.Guid.Empty;
				UnturnedLog.warn("{0} unable to parse weather asset guid", transform.GetSceneHierarchyPath());
			}
#endif
		}

		protected void OnDisable()
		{
#if GAME
			if (!parsedGuid.Equals(System.Guid.Empty))
			{
				WeatherEventListenerManager.RemoveComponentListener(parsedGuid, this);
			}
#endif
		}

		/// <summary>
		/// GUID parsed from WeatherAssetGuid parameter.
		/// </summary>
		protected System.Guid parsedGuid;
	}
}
