////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Passenger
	{
		public SteamPlayer player;
		public TurretInfo turret;

		private Transform _seat;
		public Transform seat => _seat;

		private Transform _obj;
		public Transform obj => _obj;

		/// <summary>
		/// Optional component on Turret_# GameObject for modding UnityEvents.
		/// </summary>
		public VehicleTurretEventHook turretEventHook;

		public Quaternion rotationYaw
		{
			get;
			private set;
		}

		private Transform _turretYaw;
		public Transform turretYaw => _turretYaw;

		public Quaternion rotationPitch
		{
			get;
			private set;
		}

		private Transform _turretPitch;
		public Transform turretPitch => _turretPitch;

		private Transform _turretAim;
		public Transform turretAim => _turretAim;

		public byte[] state;

		/// <summary>
		/// Optional collider matching the player capsule to prevent short vehicles (e.g. bikes) from clipping into walls.
		/// </summary>
		internal CapsuleCollider collider;

		public Passenger(Transform newSeat, Transform newObj, Transform newTurretYaw, Transform newTurretPitch, Transform newTurretAim)
		{
			_seat = newSeat;
			_obj = newObj;
			_turretYaw = newTurretYaw;
			_turretPitch = newTurretPitch;
			_turretAim = newTurretAim;

			if (turretYaw != null)
			{
				rotationYaw = turretYaw.localRotation;
			}

			if (turretPitch != null)
			{
				rotationPitch = turretPitch.localRotation;
			}
		}
	}
}
