////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class ColorEx
	{
		public static readonly Color BlackZeroAlpha = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		public static readonly Color WhiteZeroAlpha = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		public static bool IsNearlyBlack(this Color color, float tolerance = 0.001f)
		{
			return (Mathf.Abs(color.r) < tolerance) & (Mathf.Abs(color.g) < tolerance) & (Mathf.Abs(color.b) < tolerance);
		}

		public static bool IsNearlyWhite(this Color color, float tolerance = 0.001f)
		{
			return (Mathf.Abs(color.r - 1.0f) < tolerance) & (Mathf.Abs(color.g - 1.0f) < tolerance) & (Mathf.Abs(color.b - 1.0f) < tolerance);
		}

		public static Color WithAlpha(this Color color, float alphaOverride)
		{
			return new Color(color.r, color.g, color.b, alphaOverride);
		}
	}
}
