////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class DamageInfo
	{
		public Transform transform;
		public Collider collider;
		public float distance;
		public Vector3 point;
		public Vector3 normal;
		public Player player;
		public Zombie zombie;
		public InteractableVehicle vehicle;

		public void update(RaycastHit hit)
		{
			transform = hit.transform;
			collider = hit.collider;
			distance = hit.distance;
			point = hit.point;
			normal = hit.normal;
		}
	}
}