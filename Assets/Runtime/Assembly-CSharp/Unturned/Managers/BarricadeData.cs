////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class BarricadeData
	{
		private Barricade _barricade;
		public Barricade barricade => _barricade;

		public Vector3 point;

		/// <summary>
		/// Note: If barricade is attached to a vehicle this is the local rotation.
		/// </summary>
		public Quaternion rotation;

		[System.Obsolete("Replaced by rotation quaternion, but you should probably not be accessing either of these directly.")]
		public byte angle_x;
		[System.Obsolete("Replaced by rotation quaternion, but you should probably not be accessing either of these directly.")]
		public byte angle_y;
		[System.Obsolete("Replaced by rotation quaternion, but you should probably not be accessing either of these directly.")]
		public byte angle_z;

		public ulong owner;
		public ulong group;

		public uint objActiveDate;
		public uint instanceID
		{
			get;
			private set;
		}

		public BarricadeData(Barricade newBarricade, Vector3 newPoint, Quaternion newRotation, ulong newOwner, ulong newGroup, uint newObjActiveDate, uint newInstanceID)
		{
			_barricade = newBarricade;
			point = newPoint;
			rotation = newRotation;
			owner = newOwner;
			group = newGroup;

			objActiveDate = newObjActiveDate;
			instanceID = newInstanceID;
		}

		[System.Obsolete]
		public BarricadeData(Barricade newBarricade, Vector3 newPoint, byte newAngle_X, byte newAngle_Y, byte newAngle_Z, ulong newOwner, ulong newGroup, uint newObjActiveDate, uint newInstanceID)
		{
			_barricade = newBarricade;
			point = newPoint;
			rotation = Quaternion.Euler(newAngle_X * 2.0f, newAngle_Y * 2.0f, newAngle_Z * 2.0f);
			angle_x = newAngle_X;
			angle_y = newAngle_Y;
			angle_z = newAngle_Z;

			owner = newOwner;
			group = newGroup;

			objActiveDate = newObjActiveDate;
			instanceID = newInstanceID;
		}
	}
}
