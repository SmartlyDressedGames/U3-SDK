////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to Vehicle GameObject to receive events.
	/// </summary>
	[AddComponentMenu("Unturned/Vehicle Event Hook")]
	public class VehicleEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when any player enters the driver seat.
		/// </summary>
		public UnityEvent OnDriverAdded;

		/// <summary>
		/// Invoked when any player exits the driver seat.
		/// </summary>
		public UnityEvent OnDriverRemoved;

		/// <summary>
		/// Invoked when a locally controlled player enters the driver seat.
		/// </summary>
		public UnityEvent OnLocalDriverAdded;

		/// <summary>
		/// Invoked when a locally controlled player exits the driver seat.
		/// </summary>
		public UnityEvent OnLocalDriverRemoved;

		/// <summary>
		/// Invoked when a locally controlled player enters the vehicle.
		/// </summary>
		public UnityEvent OnLocalPassengerAdded;

		/// <summary>
		/// Invoked when a locally controlled player exits the vehicle.
		/// </summary>
		public UnityEvent OnLocalPassengerRemoved;

		/// <summary>
		/// Invoked when lock is engaged.
		/// </summary>
		public UnityEvent OnLocked;

		/// <summary>
		/// Invoked when lock is disengaged.
		/// </summary>
		public UnityEvent OnUnlocked;

		/// <summary>
		/// Invoked when horn is played.
		/// </summary>
		public UnityEvent OnHornUsed;

		/// <summary>
		/// Invoked after explosion plays.
		/// </summary>
		public UnityEvent OnExploded;

		[Tooltip("Invoked after paint color changes.")]
		public UnityEvent OnPaintColorChanged;

		[Tooltip("Invoked when sirens are turned on.")]
		public UnityEvent OnSirensActivated;

		[Tooltip("Invoked when sirens are turned off.")]
		public UnityEvent OnSirensDeactivated;

		[Tooltip("Invoked when blimp floatation mode is activated.")]
		public UnityEvent OnBlimpActivated;

		[Tooltip("Invoked when blimp floatation mode is deactivated.")]
		public UnityEvent OnBlimpDeactivated;

		[Tooltip("Invoked when headlights are turned on.")]
		public UnityEvent OnHeadlightsActivated;

		[Tooltip("Invoked when headlights are turned off.")]
		public UnityEvent OnHeadlightsDeactivated;

		[Tooltip("Invoked on Authority when vehicle enters the world. (Not when loaded or replicated.)")]
		public UnityEvent OnFirstSpawned;
	}
}
