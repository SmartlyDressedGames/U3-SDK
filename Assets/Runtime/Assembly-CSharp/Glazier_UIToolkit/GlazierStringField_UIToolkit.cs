////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierStringField_UIToolkit : GlazierElementBase_UIToolkit, ISleekField
	{
		public event Entered OnTextSubmitted;
		public event Typed OnTextChanged;
		public event Escaped OnTextEscaped;

		public bool IsPasswordField
		{
			get
			{
				ValidateNotDestroyed();
				return control.isPasswordField;
			}

			set
			{
				ValidateNotDestroyed();
				control.isPasswordField = value;
			}
		}

		public string PlaceholderText
		{
			get
			{
				ValidateNotDestroyed();
				return placeholderLabel.text;
			}

			set
			{
				ValidateNotDestroyed();
				placeholderLabel.text = value;
			}
		}

		public bool IsMultiline
		{
			get
			{
				ValidateNotDestroyed();
				return control.multiline;
			}

			set
			{
				ValidateNotDestroyed();
				control.multiline = value;
			}
		}

		public string Text
		{
			get
			{
				ValidateNotDestroyed();
				return control.text;
			}

			set
			{
				ValidateNotDestroyed();
				control.SetValueWithoutNotify(value);
				SynchronizePlaceholderVisible();
			}
		}

		public string TooltipText
		{
			get;
			set;
		}

		private FontStyle _fontStyle = GlazierConst.DefaultFieldFontStyle;
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
				inputElement.style.unityFontStyleAndWeight = GlazierUtils_UIToolkit.GetFontStyle(_fontStyle);
				placeholderLabel.style.unityFontStyleAndWeight = GlazierUtils_UIToolkit.GetFontStyle(_fontStyle);
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
				inputElement.style.unityTextAlign = GlazierUtils_UIToolkit.GetTextAlignment(_fontAlignment);
				placeholderLabel.style.unityTextAlign = GlazierUtils_UIToolkit.GetTextAlignment(_fontAlignment);
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
				StyleLength resolvedFontSize = GlazierUtils_UIToolkit.GetFontSize(_fontSize);
				inputElement.style.fontSize = resolvedFontSize;
				placeholderLabel.style.fontSize = resolvedFontSize;
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

		private SleekColor _textColor = GlazierConst.DefaultFieldForegroundColor;
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
				Color resolvedTextColor = _textColor;
				inputElement.style.color = resolvedTextColor;
				placeholderLabel.style.color = resolvedTextColor * 0.5f;
				SynchronizeTextContrast();
			}
		}

		// Other glaziers doesn't implement field rich text.
		// This property is only here from ISleekLabel.
		public bool AllowRichText
		{
			get => false;
			set { }
		}

		private SleekColor _backgroundColor = GlazierConst.DefaultFieldBackgroundColor;
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
				inputElement.style.unityBackgroundImageTintColor = _backgroundColor;
			}
		}

		public bool IsClickable
		{
			get
			{
				ValidateNotDestroyed();
				return control.enabledSelf;
			}

			set
			{
				ValidateNotDestroyed();
				control.SetEnabled(value);
			}
		}

		public int MaxLength
		{
			get
			{
				ValidateNotDestroyed();
				return control.maxLength;
			}

			set
			{
				ValidateNotDestroyed();
				control.maxLength = value;
			}
		}

		public void FocusControl()
		{
			ValidateNotDestroyed();
			// Check whether we are already focused, otherwise it will interrupt typing.
			// (PlayerUI constantly focuses chat field while chatting.)
			if (control.focusController.focusedElement != control)
			{
				control.Focus();
			}
		}

		public void ClearFocus()
		{
			ValidateNotDestroyed();
			control.Blur();
		}

		public GlazierStringField_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			control = new TextField();
			control.userData = this;
			control.AddToClassList("unturned-field");
			control.RegisterValueChangedCallback(OnControlValueChanged);
			control.RegisterCallback<KeyUpEvent>(OnControlKeyUp);
			control.maxLength = GlazierConst.DefaultTextFieldMaxLength;

			inputElement = control.Q(className: TextField.inputUssClassName);

			placeholderLabel = new Label();
			placeholderLabel.AddToClassList("unturned-field__placeholder");
			placeholderLabel.pickingMode = PickingMode.Ignore;
			control.Add(placeholderLabel);

			visualElement = control;
		}

		protected virtual void OnControlValueChanged(ChangeEvent<string> changeEvent)
		{
			OnTextChanged?.Invoke(this, changeEvent.newValue);
			SynchronizePlaceholderVisible();
		}

		protected virtual void OnSubmitted()
		{
			OnTextSubmitted?.Invoke(this);
			SynchronizePlaceholderVisible();
		}

		internal override void SynchronizeColors()
		{
			Color resolvedTextColor = _textColor;
			inputElement.style.color = resolvedTextColor;
			inputElement.style.unityBackgroundImageTintColor = _backgroundColor;
			placeholderLabel.style.color = resolvedTextColor * 0.5f;
			SynchronizeTextContrast();
		}

		internal override bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
		{
			tooltipText = this.TooltipText;
			tooltipColor = _textColor;
			return true;
		}

		private void SynchronizePlaceholderVisible()
		{
			placeholderLabel.visible = string.IsNullOrEmpty(control.text);
		}

		private void SynchronizeTextContrast()
		{
			float alpha = _textColor.Get().a;
			GlazierUtils_UIToolkit.ApplyTextContrast(inputElement.style, _contrastContext, alpha);
			GlazierUtils_UIToolkit.ApplyTextContrast(placeholderLabel.style, _contrastContext, alpha);
		}

		private void OnControlKeyUp(KeyUpEvent keyUpEvent)
		{
			// This code was mostly copied from the IMGUI glazier.
			if (keyUpEvent.keyCode == KeyCode.Escape)
			{
				control.Blur();
				OnTextEscaped?.Invoke(this);
			}
			else if (keyUpEvent.keyCode == KeyCode.Return || keyUpEvent.keyCode == KeyCode.KeypadEnter)
			{
				// Multiline fields (namely the buildable sign editor) need Return for newlines.
				if (!IsMultiline)
				{
					OnSubmitted();
					control.Blur();
				}
			}
		}

		private TextField control;
		private Label placeholderLabel;
		private VisualElement inputElement;
	}
}
