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
	[AddComponentMenu("Unturned/Useable Gun Event Hook")]
	public class UseableGunEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when gun is fired.
		/// </summary>
		public UnityEvent OnShotFired;

		/// <summary>
		/// Invoked when gun begins reload sequence.
		/// </summary>
		public UnityEvent OnReloadingStarted;

		/// <summary>
		/// Invoked when gun begins hammer sequence.
		/// </summary>
		public UnityEvent OnChamberingStarted;

		/// <summary>
		/// Invoked when gun begins aiming.
		/// </summary>
		public UnityEvent OnAimingStarted;

		/// <summary>
		/// Invoked when gun ends aiming.
		/// </summary>
		public UnityEvent OnAimingStopped;

		/// <summary>
		/// Invoked when Magazine game object is activated.
		/// </summary>
		public UnityEvent OnMagazineVisible;

		/// <summary>
		/// Invoked when Magazine game object is activated.
		/// </summary>
		public UnityEvent OnMagazineHidden;
	}
}
