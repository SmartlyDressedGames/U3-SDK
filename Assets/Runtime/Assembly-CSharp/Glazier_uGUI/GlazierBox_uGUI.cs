////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierBox_uGUI : GlazierElementBase_uGUI, ISleekBox
	{
		public string Text
		{
			get => textComponent.text;
			set => textComponent.text = value;
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
				imageComponent.color = _backgroundColor;
			}
		}

		private void PostConstructBox()
		{
			TextAlignment = TextAnchor.MiddleCenter;
			FontSize = ESleekFontSize.Default;
			TextContrastContext = GlazierConst.DefaultLabelContrastContext;
			FontStyle = GlazierConst.DefaultLabelFontStyle;
			AllowRichText = false;
		}

		protected override bool ReleaseIntoPool()
		{
			if (imageComponent == null || textComponent == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.error("Image or text component null when releasing GlazierBox into uGUI pool!");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return false;
			}

			if (tooltipComponent != null)
			{
				Object.Destroy(tooltipComponent);
				tooltipComponent = null;
			}

			imageComponent.enabled = false;
			textComponent.enabled = false;

			BoxPoolData poolData = new BoxPoolData();
			PopulateBasePoolData(poolData);
			poolData.imageComponent = imageComponent;
			imageComponent = null;
			poolData.textComponent = textComponent;
			textComponent = null;

			glazier.ReleaseBoxToPool(poolData);
			return true;
		}

		protected override void EnableComponents()
		{
			imageComponent.enabled = true;
			textComponent.enabled = true;
		}

		public GlazierBox_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			imageComponent = gameObject.AddComponent<Image>();
			imageComponent.enabled = false;
			imageComponent.type = Image.Type.Sliced;
			imageComponent.raycastTarget = true;

			GameObject labelGameObject = new GameObject("BoxText", typeof(RectTransform));
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

			PostConstructBox();
		}

		public class BoxPoolData : PoolData
		{
			public Image imageComponent;
			public TextMeshProUGUI textComponent;
		}

		public void ConstructFromBoxPool(BoxPoolData poolData)
		{
			ConstructFromPool(poolData);
			imageComponent = poolData.imageComponent;
			textComponent = poolData.textComponent;
			textComponent.rectTransform.reset(); // Old transform may have been modified by layout components.

			textComponent.text = string.Empty;

			PostConstructBox();
		}

		public override void SynchronizeColors()
		{
			imageComponent.color = _backgroundColor;
			textComponent.color = _textColor;

			if (tooltipComponent != null)
			{
				tooltipComponent.color = _textColor;
			}
		}

		public override void SynchronizeTheme()
		{
			imageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;
		}

		private Image imageComponent;
		private TextMeshProUGUI textComponent;
	}
}
