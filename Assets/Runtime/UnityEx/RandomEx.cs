////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class RandomEx
	{
		public static Vector3 GetRandomForwardVectorInCone(float halfAngleRadians)
		{
			const float MAX = MathfEx.HALF_PI - 0.001f;
			halfAngleRadians = Mathf.Min(halfAngleRadians, MAX);

			// Random angle away from forward.
			// sqrt(random) is for even distribution in a circle
			float coneAngle = halfAngleRadians * Mathf.Sqrt(Random.value);

			// sin(angle) = opposite / hypotenuse
			// We know the angle, hypotenuse is 1 because it is a unit vector, and want to know the opposite (distance from forward) 
			float radius = Mathf.Sin(coneAngle);

			// Random rotation around forward.
			float rotation = MathfEx.TAU * Random.value;
			float cos = Mathf.Cos(rotation);
			float sin = Mathf.Sin(rotation);

			// Rotate distance around forward axis.
			float x = cos * radius;
			float y = sin * radius;

			// Result is a unit vector so we know that x*x + y*y + z*z = 1
			// z*z = 1 - x*x - y*y
			float z = Mathf.Sqrt(1.0f - (x * x) - (y * y));

			return new Vector3(x, y, z);
		}
	}
}
