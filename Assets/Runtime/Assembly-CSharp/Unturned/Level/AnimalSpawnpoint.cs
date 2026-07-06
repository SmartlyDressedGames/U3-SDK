////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class AnimalSpawnpoint
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

		public AnimalSpawnpoint(byte newType, Vector3 newPoint)
		{
			type = newType;
			_point = newPoint;

			if (Level.isEditor)
			{
				_node = ((GameObject) GameObject.Instantiate(Resources.Load("Edit/Animal"))).transform;
				node.name = type.ToString();
				node.position = point;
				node.GetComponent<Renderer>().material.color = LevelAnimals.tables[type].color;
			}
		}
	}
}
