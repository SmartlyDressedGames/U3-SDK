////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierLabel_IMGUI : GlazierElementBase_IMGUI, ISleekLabel
	{
		private string _text = "";
		public string Text
		{
			get
			{
				ValidateNotDestroyed();
				return _text;
			}

			set
			{
				ValidateNotDestroyed();
				_text = value;
				calculateContent();
			}
		}

		private string _tooltip = "";
		public string TooltipText
		{
			get
			{
				ValidateNotDestroyed();
				return _tooltip;
			}

			set
			{
				ValidateNotDestroyed();
				_tooltip = value;
				calculateContent();
			}
		}

		public FontStyle FontStyle
		{
			get;
			set;
		} = GlazierConst.DefaultLabelFontStyle;

		public TextAnchor TextAlignment
		{
			get;
			set;
		} = TextAnchor.MiddleCenter;

		protected int fontSizeInt;
		private ESleekFontSize fontSizeEnum;
		public ESleekFontSize FontSize
		{
			get
			{
				ValidateNotDestroyed();
				return fontSizeEnum;
			}

			set
			{
				ValidateNotDestroyed();
				fontSizeEnum = value;
				fontSizeInt = GlazierUtils_IMGUI.GetFontSize(fontSizeEnum);
			}
		}

		public ETextContrastContext TextContrastContext
		{
			get;
			set;
		} = GlazierConst.DefaultLabelContrastContext;

		public SleekColor TextColor
		{
			get;
			set;
		} = GlazierConst.DefaultLabelForegroundColor;

		public bool AllowRichText
		{
			get;
			set;
		}

		public GUIContent content;
		protected GUIContent shadowContent;

		protected virtual void calculateContent()
		{
			ValidateNotDestroyed();
			content = new GUIContent(Text, TooltipText);

			if (AllowRichText)
			{
				shadowContent = RichTextUtil.makeShadowContent(content);
			}
			else
			{
				shadowContent = null;
			}
		}

		public GlazierLabel_IMGUI() : base()
		{
			calculateContent();
			FontSize = ESleekFontSize.Default;
		}

		public override void OnGUI()
		{
			GlazierUtils_IMGUI.drawLabel(drawRect, FontStyle, TextAlignment, fontSizeInt, shadowContent, TextColor, content, shadowStyle: TextContrastContext);
			ChildrenOnGUI();
		}
	}
}
