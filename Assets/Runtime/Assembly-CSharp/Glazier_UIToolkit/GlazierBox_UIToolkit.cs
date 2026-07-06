////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierBox_UIToolkit : GlazierElementBase_UIToolkit, ISleekBox
	{
		public override bool UseManualLayout
		{
			set
			{
				base.UseManualLayout = value;
				labelElement.style.position = _useManualLayout ? StyleKeyword.Null : Position.Relative;
			}
		}

		public string Text
		{
			get
			{
				ValidateNotDestroyed();
				return labelElement.text;
			}

			set
			{
				ValidateNotDestroyed();
				labelElement.text = value;
				bool labelVisible = !string.IsNullOrEmpty(value);
				labelElement.visible = labelVisible;
				labelElement.style.visibility = labelVisible ? Visibility.Visible : Visibility.Hidden;
				labelElement.style.display = labelVisible ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		public string TooltipText
		{
			get;
			set;
		}

		private FontStyle _fontStyle;
		public FontStyle FontStyle
		{
			get
			{
				ValidateNotDestroyed();
				return _fontStyle;
			}

			set
			{
				ValidateNotDestroyed();
				_fontStyle = value;
				labelElement.style.unityFontStyleAndWeight = GlazierUtils_UIToolkit.GetFontStyle(_fontStyle);
			}
		}

		private TextAnchor _fontAlignment = TextAnchor.MiddleCenter;
		public TextAnchor TextAlignment
		{
			get
			{
				ValidateNotDestroyed();
				return _fontAlignment;
			}

			set
			{
				ValidateNotDestroyed();
				_fontAlignment = value;
				labelElement.style.unityTextAlign = GlazierUtils_UIToolkit.GetTextAlignment(_fontAlignment);
			}
		}

		private ESleekFontSize _fontSize;
		public ESleekFontSize FontSize
		{
			get
			{
				ValidateNotDestroyed();
				return _fontSize;
			}

			set
			{
				ValidateNotDestroyed();
				_fontSize = value;
				labelElement.style.fontSize = GlazierUtils_UIToolkit.GetFontSize(_fontSize);
			}
		}


		private ETextContrastContext _contrastContext;
		public ETextContrastContext TextContrastContext
		{
			get
			{
				ValidateNotDestroyed();
				return _contrastContext;
			}

			set
			{
				ValidateNotDestroyed();
				_contrastContext = value;
				SynchronizeTextContrast();
			}
		}

		private SleekColor _textColor = GlazierConst.DefaultBoxForegroundColor;
		public SleekColor TextColor
		{
			get
			{
				ValidateNotDestroyed();
				return _textColor;
			}

			set
			{
				ValidateNotDestroyed();
				_textColor = value;
				labelElement.style.color = _textColor;
				SynchronizeTextContrast();
			}
		}

		public bool AllowRichText
		{
			get
			{
				ValidateNotDestroyed();
				return labelElement.enableRichText;
			}

			set
			{
				ValidateNotDestroyed();
				labelElement.enableRichText = value;
			}
		}

		private SleekColor _backgroundColor = GlazierConst.DefaultBoxBackgroundColor;
		public SleekColor BackgroundColor
		{
			get
			{
				ValidateNotDestroyed();
				return _backgroundColor;
			}

			set
			{
				ValidateNotDestroyed();
				_backgroundColor = value;
				boxElement.style.unityBackgroundImageTintColor = _backgroundColor;
			}
		}

		public GlazierBox_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			boxElement = new VisualElement();
			boxElement.AddToClassList("unturned-box");
			boxElement.userData = this;

			labelElement = new Label();
			labelElement.pickingMode = PickingMode.Ignore;
			labelElement.AddToClassList("unturned-box-label");
			labelElement.enableRichText = false;
			boxElement.Add(labelElement);

			Text = string.Empty;

			visualElement = boxElement;
		}

		internal override void SynchronizeColors()
		{
			labelElement.style.color = _textColor;
			boxElement.style.unityBackgroundImageTintColor = _backgroundColor;
			SynchronizeTextContrast();
		}

		internal override bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
		{
			tooltipText = this.TooltipText;
			tooltipColor = _textColor;
			return true;
		}

		private void SynchronizeTextContrast()
		{
			GlazierUtils_UIToolkit.ApplyTextContrast(labelElement.style, _contrastContext, _textColor.Get().a);
		}

		private VisualElement boxElement;
		private Label labelElement;
	}
}
