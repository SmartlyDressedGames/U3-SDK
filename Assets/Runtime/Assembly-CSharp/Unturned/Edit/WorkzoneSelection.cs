////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class WorkzoneSelection
	{
		private Transform _transform;
		public Transform transform => _transform;
		
		public Vector3 preTransformPosition;
		public Quaternion preTransformRotation;

		public WorkzoneSelection(Transform newTransform)
		{
			_transform = newTransform;
		}
	}
}
