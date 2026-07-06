////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Node
	{
		protected Vector3 _point;
		public Vector3 point => _point;

		protected ENodeType _type;
		public ENodeType type => _type;

		protected Transform _model;
		public Transform model => _model;

		public void move(Vector3 newPoint)
		{
			_point = newPoint;
			if (_model != null)
			{
				_model.position = point;
			}
		}

		public void setEnabled(bool isEnabled)
		{
			if (_model != null)
			{
				_model.gameObject.SetActive(isEnabled);
			}
		}

		public void remove()
		{
			if (_model != null)
			{
				Object.Destroy(_model.gameObject);
				_model = null;
			}
		}
	}
}
