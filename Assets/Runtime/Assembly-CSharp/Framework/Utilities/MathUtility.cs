////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public class MathUtility
	{
		// Supposedly it's faster to cache these than referring to the Quaternion/Matrix4x4 version
		public static readonly Quaternion IDENTITY_QUATERNION = Quaternion.identity;
		public static readonly Matrix4x4 IDENTITY_MATRIX = Matrix4x4.identity;

		public static void getPitchYawFromDirection(Vector3 direction, out float pitch, out float yaw)
		{
			pitch = Mathf.Rad2Deg * -Mathf.Sin(direction.y / direction.magnitude);
			yaw = (Mathf.Rad2Deg * -Mathf.Atan2(direction.z, direction.x)) + 90;
		}
	}
}
