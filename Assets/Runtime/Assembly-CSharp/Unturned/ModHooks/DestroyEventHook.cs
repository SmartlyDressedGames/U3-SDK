////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	[AddComponentMenu("Unturned/Destroy Event Hook")]
	public class DestroyEventHook : MonoBehaviour
	{
		/// <summary>
		/// If true the event will only be invoked in offline mode and on the server.
		/// </summary>
		public bool AuthorityOnly;

		public UnityEvent OnDestroyed;

		private void OnDestroy()
		{
#if GAME
			if (AuthorityOnly && !Provider.isServer)
				return;
#endif // GAME

			OnDestroyed.Invoke();
		}
	}
}
