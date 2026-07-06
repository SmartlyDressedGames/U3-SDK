////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorDrag
	{
		private Transform _transform;
		public Transform transform => _transform;

		private Vector3 _screen;
		public Vector3 screen => _screen;

		public EditorDrag(Transform newTransform, Vector3 newScreen)
		{
			_transform = newTransform;
			_screen = newScreen;
		}
	}
}