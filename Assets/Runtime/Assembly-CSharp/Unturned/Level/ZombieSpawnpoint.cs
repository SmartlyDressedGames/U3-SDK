////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ZombieSpawnpoint
	{
		public byte type;

		private Vector3 _point;
		public Vector3 point => _point;

		private Transform _node;
		public Transform node => _node;

		public void setEnabled(bool isEnabled)
		{
			node.transform.gameObject.SetActive(isEnabled);
		}

		public ZombieSpawnpoint(byte newType, Vector3 newPoint)
		{
			type = newType;
			_point = newPoint;

			if (Level.isEditor)
			{
				_node = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Zombie"))).transform;
				node.name = type.ToString();
				node.position = point + Vector3.up;
				node.GetComponent<Renderer>().material.color = LevelZombies.tables[type].color;
			}
		}
	}
}
