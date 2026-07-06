////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal static class GlazierResources_UIToolkit
	{
		public static ThemeStyleSheet Theme
		{
			get
			{
				if (OptionsSettings.proUI)
				{
					return DarkTheme;
				}
				else
				{
					return LightTheme;
				}
			}
		}

		public static StaticResourceRef<ThemeStyleSheet> LightTheme
		{
			get;
			private set;
		} = new StaticResourceRef<ThemeStyleSheet>("UI/Glazier_UIToolkit/UnturnedLightTheme");

		public static StaticResourceRef<ThemeStyleSheet> DarkTheme
		{
			get;
			private set;
		} = new StaticResourceRef<ThemeStyleSheet>("UI/Glazier_UIToolkit/UnturnedDarkTheme");
	}
}
