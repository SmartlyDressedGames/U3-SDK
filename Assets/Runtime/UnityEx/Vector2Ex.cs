////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace UnityEngine
{
	public static class Vector2Ex
	{
		/// <summary>
		/// Helper to get Vector2 perpendicular to this one.
		/// </summary>
		public static Vector2 Cross(this Vector2 vector)
		{
			return new Vector2(vector.y, -vector.x);
		}
	}
}
