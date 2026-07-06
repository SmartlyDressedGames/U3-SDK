////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class GlazierConst
	{
		public static readonly SleekColor DefaultBoxBackgroundColor = ESleekTint.BACKGROUND;
		public static readonly SleekColor DefaultButtonBackgroundColor = ESleekTint.BACKGROUND;
		public static readonly SleekColor DefaultFieldBackgroundColor = ESleekTint.BACKGROUND;

		public static readonly SleekColor DefaultSliderBackgroundColor = ESleekTint.BACKGROUND;
		public static readonly SleekColor DefaultSliderForegroundColor = ESleekTint.FOREGROUND;

		public static readonly SleekColor DefaultScrollViewBackgroundColor = ESleekTint.BACKGROUND;
		public static readonly SleekColor DefaultScrollViewForegroundColor = ESleekTint.FOREGROUND;

		public static readonly SleekColor DefaultToggleBackgroundColor = ESleekTint.BACKGROUND;
		public static readonly SleekColor DefaultToggleForegroundColor = ESleekTint.FOREGROUND;

		public static readonly SleekColor DefaultBoxForegroundColor = ESleekTint.FONT;
		public static readonly SleekColor DefaultButtonForegroundColor = ESleekTint.FONT;
		public static readonly SleekColor DefaultLabelForegroundColor = ESleekTint.FONT;
		public static readonly SleekColor DefaultFieldForegroundColor = ESleekTint.FONT;

		public const ETextContrastContext DefaultFieldContrastContext = ETextContrastContext.Default;
		public const ETextContrastContext DefaultLabelContrastContext = ETextContrastContext.Default;

		// If changing the default font style, please also update the USS font style!
		// The default is applied by the stylesheet rather than inline style.
		public const FontStyle DefaultLabelFontStyle = FontStyle.Normal;
		public const FontStyle DefaultFieldFontStyle = FontStyle.Normal;

		public const int DefaultTextFieldMaxLength = 100;
	}
}
