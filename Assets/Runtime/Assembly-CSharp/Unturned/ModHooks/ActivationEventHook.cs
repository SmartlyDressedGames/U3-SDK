////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	[AddComponentMenu("Unturned/Activation Event Hook")]
	public class ActivationEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when component is enabled and when the game object is activated.
		/// </summary>
		public UnityEvent OnEnabled;

		/// <summary>
		/// Invoked when component is disabled and when the game object is deactivated.
		/// Note that if the component or game object spawn deactivated this will not be immediately invoked.
		/// </summary>
		public UnityEvent OnDisabled;

		private void OnEnable()
		{
			OnEnabled.Invoke();
		}

		private void OnDisable()
		{
			OnDisabled.Invoke();
		}
	}
}
