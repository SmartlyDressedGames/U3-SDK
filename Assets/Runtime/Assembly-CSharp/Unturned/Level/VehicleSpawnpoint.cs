////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class VehicleSpawnpoint
	{
		public byte type;

		private Vector3 _point;
		public Vector3 point => _point;

		private float _angle;
		public float angle => _angle;

		private Transform _node;
		public Transform node => _node;

		public void setEnabled(bool isEnabled)
		{
			node.transform.gameObject.SetActive(isEnabled);
		}

		public VehicleSpawnpoint(byte newType, Vector3 newPoint, float newAngle)
		{
			type = newType;
			_point = newPoint;
			_angle = newAngle;

			if (Level.isEditor)
			{
				_node = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Vehicle"))).transform;
				node.name = type.ToString();
				node.position = point;
				node.rotation = Quaternion.Euler(0, angle, 0);
				node.GetComponent<Renderer>().material.color = LevelVehicles.tables[type].color;
				node.Find("Arrow").GetComponent<Renderer>().material.color = LevelVehicles.tables[type].color;
			}
		}
	}
}
