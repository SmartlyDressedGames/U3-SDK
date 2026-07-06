////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to Vehicle Turret_# GameObject to receive events.
	/// </summary>
	[AddComponentMenu("Unturned/Vehicle Turret Event Hook")]
	public class VehicleTurretEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when turret gun is fired.
		/// </summary>
		public UnityEvent OnShotFired;

		/// <summary>
		/// Invoked when turret gun begins reload sequence.
		/// </summary>
		public UnityEvent OnReloadingStarted;

		/// <summary>
		/// Invoked when turret gun begins hammer sequence.
		/// </summary>
		public UnityEvent OnChamberingStarted;

		/// <summary>
		/// Invoked when turret gun begins aiming.
		/// </summary>
		public UnityEvent OnAimingStarted;

		/// <summary>
		/// Invoked when turret gun ends aiming.
		/// </summary>
		public UnityEvent OnAimingStopped;

		/// <summary>
		/// Invoked when turret gun controlled by a local player begins aiming.
		/// </summary>
		public UnityEvent OnAimingStarted_Local;

		/// <summary>
		/// Invoked when turret gun controlled by a local player ends aiming.
		/// </summary>
		public UnityEvent OnAimingStopped_Local;

		/// <summary>
		/// Invoked when turret gun controlled by a local player begins inspecting attachments.
		/// </summary>
		public UnityEvent OnInspectingAttachmentsStarted_Local;

		/// <summary>
		/// Invoked when turret gun controlled by a local player ends inspecting attachments.
		/// </summary>
		public UnityEvent OnInspectingAttachmentsStopped_Local;

		/// <summary>
		/// Invoked when any player enters the seat.
		/// </summary>
		public UnityEvent OnPassengerAdded;

		/// <summary>
		/// Invoked when any player exits the seat.
		/// </summary>
		public UnityEvent OnPassengerRemoved;

		/// <summary>
		/// Invoked when a locally controlled player enters the seat.
		/// </summary>
		public UnityEvent OnLocalPassengerAdded;

		/// <summary>
		/// Invoked when a locally controlled player exits the seat.
		/// </summary>
		public UnityEvent OnLocalPassengerRemoved;
	}
}
