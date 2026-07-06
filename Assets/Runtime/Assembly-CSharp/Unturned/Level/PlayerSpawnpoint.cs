////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerSpawnpoint
	{
		private Vector3 _point;
		public Vector3 point => _point;

		private float _angle;
		public float angle => _angle;

		private bool _isAlt;
		public bool isAlt => _isAlt;

		private Transform _node;
		public Transform node => _node;

		public void setEnabled(bool isEnabled)
		{
			node.transform.gameObject.SetActive(isEnabled);
		}

		public PlayerSpawnpoint(Vector3 newPoint, float newAngle, bool newIsAlt)
		{
			_point = newPoint;
			_angle = newAngle;
			_isAlt = newIsAlt;

			if (Level.isEditor)
			{
				_node = ((GameObject) GameObject.Instantiate(Resources.Load(isAlt ? "Edit/Player_Alt" : "Edit/Player"))).transform;
				node.name = "Player";
				node.position = point;
				node.rotation = Quaternion.Euler(0, angle, 0);
			}
		}
	}
}
