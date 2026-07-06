////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class GlazierConst_uGUI
	{
		public const TextOverflowModes DefaultOverflowMode = TextOverflowModes.Truncate; // Masking requires a masking rect.
		public static readonly Vector4 DefaultTextMargin = new Vector4(5.0f, 3.0f, 5.0f, 3.0f);
		public const bool DefaultExtraPadding = true; // Otherwise non-shadowed text gets cut off.
	}
}
