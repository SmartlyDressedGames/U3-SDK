////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public interface ITransformedHandler
	{
		public void OnTransformed(Vector3 oldPosition, Quaternion oldRotation, Vector3 oldLocalScale, Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale, bool modifyRotation, bool modifyScale);
	}
}
