////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class QuaternionEx
	{
		public static bool IsNormalized(this Quaternion quaternion, float threshold = 0.001f)
		{
			return (quaternion.x * quaternion.x)
				+ (quaternion.y * quaternion.y)
				+ (quaternion.z * quaternion.z)
				+ (quaternion.w * quaternion.w)
				- 1.0f
				< threshold * threshold;
		}

		public static Quaternion GetRoundedIfNearlyAxisAligned(this Quaternion quaternion, float tolerance = 0.05f)
		{
			Vector3 eulerAngles = quaternion.eulerAngles;
			Vector3 roundedEulerAngles = eulerAngles;
			const float ONE_DIVIDED_BY_90 = 1.0f / 90.0f;
			roundedEulerAngles.x = Mathf.RoundToInt(roundedEulerAngles.x * ONE_DIVIDED_BY_90) * 90;
			roundedEulerAngles.y = Mathf.RoundToInt(roundedEulerAngles.y * ONE_DIVIDED_BY_90) * 90;
			roundedEulerAngles.z = Mathf.RoundToInt(roundedEulerAngles.z * ONE_DIVIDED_BY_90) * 90;
			if (MathfEx.IsAngleDegreesNearlyEqual(eulerAngles.x, roundedEulerAngles.x, tolerance)
				&& MathfEx.IsAngleDegreesNearlyEqual(eulerAngles.y, roundedEulerAngles.y, tolerance)
				&& MathfEx.IsAngleDegreesNearlyEqual(eulerAngles.z, roundedEulerAngles.z, tolerance))
			{
				return Quaternion.Euler(roundedEulerAngles);
			}
			else
			{
				return quaternion;
			}
		}
	}
}
