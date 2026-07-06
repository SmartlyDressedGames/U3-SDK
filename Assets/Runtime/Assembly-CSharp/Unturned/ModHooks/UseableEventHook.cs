////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to EquipablePrefab item GameObject to receive events.
	/// </summary>
	[AddComponentMenu("Unturned/Useable Event Hook")]
	public class UseableEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when item begins inspect animation.
		/// </summary>
		public UnityEvent OnInspectStarted;
	}
}
