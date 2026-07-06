////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class ButtonEx : Button
	{
		public ButtonClickedEvent onRightClick = new ButtonClickedEvent();

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				if (!IsActive() || !IsInteractable())
					return;

				onRightClick.Invoke();
			}
			else
			{
				base.OnPointerClick(eventData);
			}
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			// Hack to enable effects for right-clicks.
			// Events are re-used between multiple callbacks, so we need to restore the button afterwards.
			PointerEventData.InputButton originalButton = eventData.button;
			eventData.button = PointerEventData.InputButton.Left;
			base.OnPointerDown(eventData);
			eventData.button = originalButton;
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			// Hack to enable effects for right-clicks.
			// Events are re-used between multiple callbacks, so we need to restore the button afterwards.
			PointerEventData.InputButton originalButton = eventData.button;
			eventData.button = PointerEventData.InputButton.Left;
			base.OnPointerUp(eventData);
			eventData.button = originalButton;
		}
	}

	internal class GlazierButton_uGUI : GlazierElementBase_uGUI, ISleekButton
	{
		public string Text
		{
			get
			{
				ValidateNotDestroyed();
				return textComponent.text;
			}

			set
			{
				ValidateNotDestroyed();
				textComponent.text = value;
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
			}
		}

		public event ClickedButton OnClicked;
		public event ClickedButton OnRightClicked;

		public bool IsClickable
		{
			get
			{
				ValidateNotDestroyed();
				return buttonComponent.interactable;
			}

			set
			{
				ValidateNotDestroyed();
				buttonComponent.interactable = value;
				SynchronizeColors();
			}
		}

		public bool IsRaycastTarget
		{
			get
			{
				ValidateNotDestroyed();
				return imageComponent.raycastTarget;
			}

			set
			{
				ValidateNotDestroyed();
				imageComponent.raycastTarget = value;
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
				textComponent.color = _textColor;
				if (tooltipComponent != null)
				{
					tooltipComponent.color = _textColor;
				}
			}
		}

		public bool AllowRichText
		{
			get
			{
				ValidateNotDestroyed();
				return textComponent.richText;
			}

			set
			{
				ValidateNotDestroyed();
				textComponent.richText = value;
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
				imageComponent.color = _backgroundColor;
			}
		}

		private void PostConstructButton()
		{
			TextAlignment = TextAnchor.MiddleCenter;
			FontSize = ESleekFontSize.Default;
			TextContrastContext = GlazierConst.DefaultLabelContrastContext;
			FontStyle = GlazierConst.DefaultLabelFontStyle;
			AllowRichText = false;
		}

		protected override bool ReleaseIntoPool()
		{
			if (imageComponent == null || buttonComponent == null || textComponent == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.error("Image, button, or text component null when releasing GlazierButton into uGUI pool!");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return false;
			}

			if (tooltipComponent != null)
			{
				Object.Destroy(tooltipComponent);
				tooltipComponent = null;
			}

			imageComponent.enabled = false;
			buttonComponent.enabled = false;
			textComponent.enabled = false;

			ButtonPoolData poolData = new ButtonPoolData();
			PopulateBasePoolData(poolData);
			poolData.imageComponent = imageComponent;
			imageComponent = null;
			poolData.buttonComponent = buttonComponent;
			buttonComponent = null;
			poolData.textComponent = textComponent;
			textComponent = null;

			glazier.ReleaseButtonToPool(poolData);
			return true;
		}

		protected override void EnableComponents()
		{
			imageComponent.enabled = true;
			buttonComponent.enabled = true;
			textComponent.enabled = true;
		}

		public GlazierButton_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			imageComponent = gameObject.AddComponent<Image>();
			imageComponent.enabled = false;
			imageComponent.type = Image.Type.Sliced;

			buttonComponent = gameObject.AddComponent<ButtonEx>();
			buttonComponent.enabled = false;
			buttonComponent.transition = Selectable.Transition.SpriteSwap;
			buttonComponent.onClick.AddListener(OnUnityButtonClicked);
			buttonComponent.onRightClick.AddListener(OnUnityButtonRightClicked);

			GameObject labelGameObject = new GameObject("ButtonText", typeof(RectTransform));
			labelGameObject.transform.SetParent(transform, false);
			RectTransform labelTransform = labelGameObject.GetRectTransform();
			labelTransform.reset();
			textComponent = labelGameObject.AddComponent<TextMeshProUGUI>();
			textComponent.enabled = false;
			textComponent.raycastTarget = false;
			textComponent.font = GlazierResources_uGUI.Font;
			textComponent.overflowMode = GlazierConst_uGUI.DefaultOverflowMode;
			textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
			textComponent.extraPadding = GlazierConst_uGUI.DefaultExtraPadding;

			PostConstructButton();
		}

		public class ButtonPoolData : PoolData
		{
			public Image imageComponent;
			public ButtonEx buttonComponent;
			public TextMeshProUGUI textComponent;
		}

		public void ConstructFromButtonPool(ButtonPoolData poolData)
		{
			ConstructFromPool(poolData);

			imageComponent = poolData.imageComponent;
			buttonComponent = poolData.buttonComponent;
			textComponent = poolData.textComponent;

			imageComponent.raycastTarget = true;
			textComponent.text = string.Empty;
			textComponent.rectTransform.reset(); // Old transform may have been modified by layout components.
			buttonComponent.interactable = true;

			buttonComponent.onClick.RemoveAllListeners();
			buttonComponent.onRightClick.RemoveAllListeners();
			buttonComponent.onClick.AddListener(OnUnityButtonClicked);
			buttonComponent.onRightClick.AddListener(OnUnityButtonRightClicked);

			PostConstructButton();
		}

		public override void SynchronizeColors()
		{
			Color bgColor = _backgroundColor;
			if (!IsClickable)
			{
				bgColor.a *= 0.25f;
			}

			imageComponent.color = bgColor;
			textComponent.color = TextColor;

			if (tooltipComponent != null)
			{
				tooltipComponent.color = _textColor;
			}
		}

		public override void SynchronizeTheme()
		{
			imageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;

			SpriteState spriteState = new SpriteState();
			spriteState.disabledSprite = imageComponent.sprite;
			spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
			spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
			spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
			buttonComponent.spriteState = spriteState;
		}

		private void OnUnityButtonClicked()
		{
			OnClicked?.Invoke(this);
		}

		private void OnUnityButtonRightClicked()
		{
			OnRightClicked?.Invoke(this);
		}

		private Image imageComponent;
		private ButtonEx buttonComponent;
		private TextMeshProUGUI textComponent;
	}
}
