////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class GlazierUtils_uGUI
	{
		public static TextAlignmentOptions TextAnchorToTMP(TextAnchor textAnchor)
		{
			switch (textAnchor)
			{
				case TextAnchor.LowerCenter:
					return TextAlignmentOptions.Bottom;

				case TextAnchor.LowerLeft:
					return TextAlignmentOptions.BottomLeft;

				case TextAnchor.LowerRight:
					return TextAlignmentOptions.BottomRight;

				default:
				case TextAnchor.MiddleCenter:
					return TextAlignmentOptions.Center;

				case TextAnchor.MiddleLeft:
					return TextAlignmentOptions.Left;

				case TextAnchor.MiddleRight:
					return TextAlignmentOptions.Right;

				case TextAnchor.UpperCenter:
					return TextAlignmentOptions.Top;

				case TextAnchor.UpperLeft:
					return TextAlignmentOptions.TopLeft;

				case TextAnchor.UpperRight:
					return TextAlignmentOptions.TopRight;
			}
		}

		public static float GetFontSize(ESleekFontSize fontSize)
		{
			switch (fontSize)
			{
				case ESleekFontSize.Tiny:
					return 8.0f;

				case ESleekFontSize.Small:
					return 10.0f;

				default:
				case ESleekFontSize.Default:
					return 12.0f;

				case ESleekFontSize.Medium:
					return 14.0f;

				case ESleekFontSize.Large:
					return 20.0f;

				case ESleekFontSize.Title:
					return 50.0f;
			}
		}

		public static FontStyles GetFontStyleFlags(FontStyle fontStyle)
		{
			switch (fontStyle)
			{
				case FontStyle.Bold:
					return FontStyles.Bold;

				case FontStyle.BoldAndItalic:
					return FontStyles.Bold | FontStyles.Italic;

				case FontStyle.Italic:
					return FontStyles.Italic;

				default:
				case FontStyle.Normal:
					return FontStyles.Normal;
			}
		}

		public static float GetCharacterSpacing(ETextContrastStyle shadowStyle)
		{
			switch (shadowStyle)
			{
				default:
				case ETextContrastStyle.None:
				case ETextContrastStyle.Shadow:
					return 0.0f;

				case ETextContrastStyle.Outline:
					return 10.0f;

				case ETextContrastStyle.Tooltip:
					return 15.0f;
			}
		}
	}
}
