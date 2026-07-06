////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;

namespace SDG.Unturned
{
	public static class TextMeshProUtils
	{
		public const string DefaultFontName = "LiberationSans SDF";

		public static StaticResourceRef<TMP_FontAsset> DefaultFont
		{
			get;
			private set;
		} = new StaticResourceRef<TMP_FontAsset>("UI/LiberationSans SDF with CJK");

		public static void FixupFont(TextMeshPro component)
		{
			if (component.font == null || component.font.name.Equals(DefaultFontName))
			{
				component.font = DefaultFont;
			}
		}

		public static void FixupFont(TextMeshProUGUI component)
		{
			if (component.font == null || component.font.name.Equals(DefaultFontName))
			{
				component.font = DefaultFont;
			}
		}

		public static void FixupFont(TMP_InputField component)
		{
			if (component.fontAsset == null || component.fontAsset.name.Equals(DefaultFontName))
			{
				component.fontAsset = DefaultFont;
			}
		}
	}
}
