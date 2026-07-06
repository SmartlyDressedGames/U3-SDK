////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierTheme_uGUI
	{
		public GlazierTheme_uGUI(string prefix)
		{
			BoxSprite = new StaticResourceRef<Sprite>(prefix + "/Box");
			BoxHighlightedSprite = new StaticResourceRef<Sprite>(prefix + "/Box_Highlighted");
			BoxPressedSprite = new StaticResourceRef<Sprite>(prefix + "/Box_Pressed");
			SliderBackgroundSprite = new StaticResourceRef<Sprite>(prefix + "/Slider_Background");
			ToggleForegroundSprite = new StaticResourceRef<Sprite>(prefix + "/Toggle_Foreground");
		}

		public StaticResourceRef<Sprite> BoxSprite
		{
			get;
			private set;
		}

		public StaticResourceRef<Sprite> BoxHighlightedSprite
		{
			get;
			private set;
		}

		public Sprite BoxSelectedSprite => BoxHighlightedSprite;

		public StaticResourceRef<Sprite> BoxPressedSprite
		{
			get;
			private set;
		}

		public StaticResourceRef<Sprite> SliderBackgroundSprite
		{
			get;
			private set;
		}

		public StaticResourceRef<Sprite> ToggleForegroundSprite
		{
			get;
			private set;
		}
	}

	internal static class GlazierResources_uGUI
	{
		private static GlazierTheme_uGUI lightTheme = new GlazierTheme_uGUI("UI/Glazier_uGUI/LightTheme");
		private static GlazierTheme_uGUI darkTheme = new GlazierTheme_uGUI("UI/Glazier_uGUI/DarkTheme");

		public static GlazierTheme_uGUI Theme
		{
			get
			{
				if (OptionsSettings.proUI)
				{
					return darkTheme;
				}
				else
				{
					return lightTheme;
				}
			}
		}

		public static StaticResourceRef<Sprite> TooltipShadowSprite
		{
			get;
			private set;
		} = new StaticResourceRef<Sprite>("UI/Glazier_uGUI/TooltipShadow");

		public static StaticResourceRef<TMP_FontAsset> Font
		{
			get;
			private set;
		} = new StaticResourceRef<TMP_FontAsset>("UI/Glazier_uGUI/LiberationSans");

		public static StaticResourceRef<Material> FontMaterial_Default = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Default");
		public static StaticResourceRef<Material> FontMaterial_Outline = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Outline");
		public static StaticResourceRef<Material> FontMaterial_Shadow = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Shadow");
		public static StaticResourceRef<Material> FontMaterial_Tooltip = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Tooltip");
	}
}
