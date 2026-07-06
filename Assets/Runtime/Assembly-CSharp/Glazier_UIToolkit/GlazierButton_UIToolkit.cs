////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierButton_UIToolkit : GlazierElementBase_UIToolkit, ISleekButton
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

		public event ClickedButton OnClicked;
		public event ClickedButton OnRightClicked;

		public bool IsClickable
		{
			get
			{
				ValidateNotDestroyed();
				return buttonElement.enabledSelf;
			}

			set
			{
				ValidateNotDestroyed();
				buttonElement.SetEnabled(value);
			}
		}

		public bool IsRaycastTarget
		{
			get
			{
				ValidateNotDestroyed();
				return buttonElement.pickingMode == PickingMode.Position;
			}

			set
			{
				ValidateNotDestroyed();
				buttonElement.pickingMode = value ? PickingMode.Position : PickingMode.Ignore;
			}
		}

		private SleekColor _textColor = GlazierConst.DefaultButtonForegroundColor;
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

		private SleekColor _backgroundColor = GlazierConst.DefaultButtonBackgroundColor;
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
				buttonElement.style.unityBackgroundImageTintColor = _backgroundColor;
			}
		}

		public GlazierButton_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			buttonElement = new VisualElement();
			buttonElement.userData = this;
			buttonElement.AddToClassList("unturned-button");
			clickable = new Clickable(OnClickedWithEventInfo);
			GlazierUtils_UIToolkit.AddClickableActivators(clickable);
			buttonElement.AddManipulator(clickable);

			labelElement = new Label();
			labelElement.pickingMode = PickingMode.Ignore;
			labelElement.AddToClassList("unturned-box-label");
			labelElement.enableRichText = false;
			buttonElement.Add(labelElement);

			visualElement = buttonElement;
		}

		internal override void SynchronizeColors()
		{
			labelElement.style.color = _textColor;
			buttonElement.style.unityBackgroundImageTintColor = _backgroundColor;
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

		private void OnClickedWithEventInfo(EventBase eventBase)
		{
			if (eventBase is IMouseEvent mouseEvent)
			{
				switch (mouseEvent.button)
				{
					case 0:
						OnClicked?.Invoke(this);
						break;

					case 1:
						OnRightClicked?.Invoke(this);
						break;
				}
			}
		}

		private VisualElement buttonElement;
		private Clickable clickable;
		private Label labelElement;
	}
}
