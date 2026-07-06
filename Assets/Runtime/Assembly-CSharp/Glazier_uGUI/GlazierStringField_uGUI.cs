////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierStringField_uGUI : GlazierElementBase_uGUI, ISleekField
	{
		public event Entered OnTextSubmitted;
		public event Typed OnTextChanged;
#pragma warning disable
		public event Escaped OnTextEscaped; // Not used by uGUI yet.
#pragma warning restore

		public bool IsPasswordField
		{
			get
			{
				ValidateNotDestroyed();
				return fieldComponent.contentType == TMP_InputField.ContentType.Password;
			}

			set
			{
				ValidateNotDestroyed();

				fieldComponent.contentType = value ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
				fieldComponent.ForceLabelUpdate();
			}
		}

		public string PlaceholderText
		{
			get
			{
				ValidateNotDestroyed();
				return placeholderComponent.text;
			}

			set
			{
				ValidateNotDestroyed();
				placeholderComponent.text = value;
			}
		}

		public bool IsMultiline
		{
			get
			{
				ValidateNotDestroyed();
				return fieldComponent.lineType != TMP_InputField.LineType.SingleLine;
			}

			set
			{
				ValidateNotDestroyed();
				fieldComponent.lineType = value ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
				fieldComponent.lineLimit = value ? 0 : 1; // Zero is unlimited.
			}
		}

		public string Text
		{
			get
			{
				ValidateNotDestroyed();
				return fieldComponent.text;
			}

			set
			{
				ValidateNotDestroyed();
				fieldComponent.SetTextWithoutNotify(value); // Change text without invoking onValueChanged.
			}
		}

		private GlazieruGUITooltip tooltipComponent;
		public string TooltipText
		{
			get
			{
				ValidateNotDestroyed();
				return tooltipComponent != null ? tooltipComponent.text : null;
			}

			set
			{
				ValidateNotDestroyed();
				if (tooltipComponent == null)
				{
					tooltipComponent = gameObject.AddComponent<GlazieruGUITooltip>();
					tooltipComponent.color = _textColor;
				}
				tooltipComponent.text = value;
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
				textComponent.fontStyle = GlazierUtils_uGUI.GetFontStyleFlags(_fontStyle);
				placeholderComponent.fontStyle = textComponent.fontStyle;
			}
		}

		private TextAnchor _fontAlignment;
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
				textComponent.alignment = GlazierUtils_uGUI.TextAnchorToTMP(_fontAlignment);
				placeholderComponent.alignment = textComponent.alignment;
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
				textComponent.fontSize = GlazierUtils_uGUI.GetFontSize(_fontSize);
				placeholderComponent.fontSize = GlazierUtils_uGUI.GetFontSize(_fontSize);
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

				ETextContrastStyle _shadowStyle = SleekShadowStyle.ContextToStyle(value);
				textComponent.fontSharedMaterial = glazier.GetFontMaterial(_shadowStyle);
				textComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(_shadowStyle);

				placeholderComponent.fontSharedMaterial = textComponent.fontSharedMaterial;
				placeholderComponent.characterSpacing = textComponent.characterSpacing;
			}
		}

		private SleekColor _textColor;
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
				placeholderComponent.color = _textColor.Get() * 0.5f;
				textComponent.color = _textColor;
				if (tooltipComponent != null)
				{
					tooltipComponent.color = _textColor;
				}
			}
		}

		public bool AllowRichText
		{
			get => false;
			set { }
		}

		private SleekColor _backgroundColor;
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
				SynchronizeBackgroundColor();
			}
		}

		public bool IsClickable
		{
			get
			{
				ValidateNotDestroyed();
				return fieldComponent.interactable;
			}

			set
			{
				ValidateNotDestroyed();
				fieldComponent.interactable = value;
				SynchronizeBackgroundColor();
			}
		}

		public int MaxLength
		{
			get
			{
				ValidateNotDestroyed();
				return fieldComponent.characterLimit;
			}

			set
			{
				ValidateNotDestroyed();
				fieldComponent.characterLimit = value;
			}
		}

		public void FocusControl()
		{
			ValidateNotDestroyed();
			fieldComponent.ActivateInputField();
		}

		public void ClearFocus()
		{
			ValidateNotDestroyed();
			fieldComponent.DeactivateInputField();
		}

		public GlazierStringField_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			// Image behind the field.
			imageComponent = gameObject.AddComponent<Image>();
			imageComponent.enabled = false;
			imageComponent.type = Image.Type.Sliced;
			imageComponent.raycastTarget = true;

			// Based on the recommended field setup.
			GameObject textViewportGameObject = new GameObject("Viewport", typeof(RectTransform));
			textViewportGameObject.transform.SetParent(transform, false);
			RectTransform textViewportTransform = textViewportGameObject.GetRectTransform();
			textViewportTransform.anchorMin = Vector2.zero;
			textViewportTransform.anchorMax = Vector2.one;
			textViewportTransform.offsetMin = new Vector2(2.0f, 2.0f);
			textViewportTransform.offsetMax = new Vector2(-2.0f, -2.0f);
			textViewportGameObject.AddComponent<RectMask2D>();

			GameObject placeholderGameObject = new GameObject("Placeholder", typeof(RectTransform));
			placeholderGameObject.transform.SetParent(textViewportTransform, false);
			RectTransform placeholderTransform = placeholderGameObject.GetRectTransform();
			placeholderTransform.reset();
			placeholderComponent = placeholderGameObject.AddComponent<TextMeshProUGUI>();
			placeholderComponent.enabled = false;
			placeholderComponent.raycastTarget = false;
			placeholderComponent.font = GlazierResources_uGUI.Font;
			placeholderComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
			placeholderComponent.extraPadding = GlazierConst_uGUI.DefaultExtraPadding;
			placeholderComponent.richText = false;

			GameObject textGameObject = new GameObject("Text", typeof(RectTransform));
			textGameObject.transform.SetParent(textViewportTransform, false);
			RectTransform textTransform = textGameObject.GetRectTransform();
			textTransform.reset();
			textComponent = textGameObject.AddComponent<TextMeshProUGUI>();
			textComponent.enabled = false;
			textComponent.raycastTarget = false;
			textComponent.font = GlazierResources_uGUI.Font;
			textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
			textComponent.extraPadding = GlazierConst_uGUI.DefaultExtraPadding;
			textComponent.richText = false;

			fieldComponent = gameObject.AddComponent<TMP_InputField>();
			fieldComponent.enabled = false;
			fieldComponent.textViewport = textViewportTransform;
			fieldComponent.textComponent = textComponent;
			fieldComponent.placeholder = placeholderComponent;
			fieldComponent.transition = Selectable.Transition.SpriteSwap;
			fieldComponent.onSubmit.AddListener(OnUnitySubmit);
			fieldComponent.onValueChanged.AddListener(OnUnityValueChanged);
			fieldComponent.caretWidth = 2;
			fieldComponent.customCaretColor = true; // caretColor field is ignored otherwise.
			fieldComponent.isRichTextEditingAllowed = false;
			fieldComponent.richText = false;
			fieldComponent.asteriskChar = '*';

			_backgroundColor = GlazierConst.DefaultFieldBackgroundColor;
			_textColor = GlazierConst.DefaultFieldForegroundColor;

			TextContrastContext = GlazierConst.DefaultFieldContrastContext;
			FontStyle = GlazierConst.DefaultFieldFontStyle;

			TextAlignment = TextAnchor.MiddleCenter;
			FontSize = ESleekFontSize.Default;
			MaxLength = GlazierConst.DefaultTextFieldMaxLength;
			IsMultiline = false;
		}

		private void SynchronizeBackgroundColor()
		{
			Color bgColor = _backgroundColor;
			if (!IsClickable)
			{
				bgColor.a *= 0.25f;
			}

			imageComponent.color = bgColor;
		}

		public override void SynchronizeColors()
		{
			SynchronizeBackgroundColor();

			placeholderComponent.color = _textColor.Get() * 0.5f;
			textComponent.color = _textColor;
			if (tooltipComponent != null)
			{
				tooltipComponent.color = _textColor;
			}

			// Experimented with inverse of text color, but found it conflicted with background color too often.
			fieldComponent.caretColor = OptionsSettings.foregroundColor;
			Color selectionColor = fieldComponent.caretColor;
			selectionColor.a = 0.5f;
			fieldComponent.selectionColor = selectionColor;
		}

		public override void SynchronizeTheme()
		{
			imageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;

			SpriteState spriteState = new SpriteState();
			spriteState.disabledSprite = imageComponent.sprite;
			spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
			spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite; // White flash from press looks bad.
			spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
			fieldComponent.spriteState = spriteState;
		}

		protected override void EnableComponents()
		{
			imageComponent.enabled = true;
			placeholderComponent.enabled = true;
			textComponent.enabled = true;
			fieldComponent.enabled = true;
		}

		protected virtual void OnUnitySubmit(string input)
		{
			OnTextSubmitted?.Invoke(this);
		}

		protected virtual void OnUnityValueChanged(string input)
		{
			OnTextChanged?.Invoke(this, input);
		}

		protected TMP_InputField fieldComponent;
		private Image imageComponent;
		private TextMeshProUGUI placeholderComponent;
		private TextMeshProUGUI textComponent;
	}
}
