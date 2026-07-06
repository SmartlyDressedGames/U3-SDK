////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal static class GlazierResources_IMGUI
	{
		public static GUISkin ActiveSkin => OptionsSettings.proUI ? darkTheme : lightTheme;

		private static StaticResourceRef<GUISkin> lightTheme = new StaticResourceRef<GUISkin>("UI/Glazier_IMGUI/LightTheme");
		private static StaticResourceRef<GUISkin> darkTheme = new StaticResourceRef<GUISkin>("UI/Glazier_IMGUI/DarkTheme");
	}
}
