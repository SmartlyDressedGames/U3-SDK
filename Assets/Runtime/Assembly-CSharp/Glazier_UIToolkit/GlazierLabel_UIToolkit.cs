////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierLabel_UIToolkit : GlazierElementBase_UIToolkit, ISleekLabel
	{
		private string _text = string.Empty;
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
				labelElement.text = _text;
			}
		}

		private string _tooltip = string.Empty;
		public string tooltipText
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
				labelElement.tooltip = _tooltip;
			}
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

		private SleekColor _textColor = GlazierConst.DefaultLabelForegroundColor;
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

		public override bool UseManualLayout
		{
			set
			{
				base.UseManualLayout = value;
				labelElement.style.position = _useManualLayout ? StyleKeyword.Null : Position.Relative;
			}
		}

		public GlazierLabel_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			containerElement = new VisualElement();
			containerElement.userData = this;
			containerElement.AddToClassList("unturned-label");
			containerElement.pickingMode = PickingMode.Ignore;

			labelElement = new Label();
			labelElement.AddToClassList("unturned-box-label");
			labelElement.pickingMode = PickingMode.Ignore; // Labels can never be clicked.
			labelElement.enableRichText = false;
			containerElement.Add(labelElement);

			visualElement = containerElement;
		}

		internal override void SynchronizeColors()
		{
			labelElement.style.color = _textColor;
			SynchronizeTextContrast();
		}

		private void SynchronizeTextContrast()
		{
			GlazierUtils_UIToolkit.ApplyTextContrast(labelElement.style, _contrastContext, _textColor.Get().a);
		}

		private VisualElement containerElement;
		private Label labelElement;
	}
}
