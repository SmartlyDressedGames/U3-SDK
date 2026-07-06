////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class Matrix4x4Ex
	{
		/// <summary>
		/// Matrices in Unity are column major; i.e. the position of a transformation matrix is in the last column.
		/// </summary>
		public static Vector3 GetPosition(this Matrix4x4 matrix)
		{
			Vector3 position;
			position.x = matrix.m03;
			position.y = matrix.m13;
			position.z = matrix.m23;
			return position;
		}

		/// <summary>
		/// 2021-06-16 it looks like Matrix4x4 has a `rotation` property now, but I am hesitant to modify this
		/// method without testing.
		/// </summary>
		public static Quaternion GetRotation(this Matrix4x4 matrix)
		{
			Vector3 forward;
			forward.x = matrix.m02;
			forward.y = matrix.m12;
			forward.z = matrix.m22;

			Vector3 upwards;
			upwards.x = matrix.m01;
			upwards.y = matrix.m11;
			upwards.z = matrix.m21;

			return Quaternion.LookRotation(forward, upwards);
		}
	}
}
