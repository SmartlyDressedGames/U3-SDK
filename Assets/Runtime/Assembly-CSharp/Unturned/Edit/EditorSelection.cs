////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorSelection
	{
		private Transform _transform;
		public Transform transform => _transform;

		public Vector3 fromPosition;
		public Quaternion fromRotation;
		public Vector3 fromScale;
		public Matrix4x4 relativeToPivot;

		public EditorSelection(Transform newTransform, Vector3 newFromPosition, Quaternion newFromRotation, Vector3 newFromScale)
		{
			_transform = newTransform;

			fromPosition = newFromPosition;
			fromRotation = newFromRotation;
			fromScale = newFromScale;
		}
	}
}
