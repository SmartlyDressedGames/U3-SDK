////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierLabel_uGUI : GlazierElementBase_uGUI, ISleekLabel
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
				textComponent.color = _textColor;
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

		private void PostConstructLabel()
		{
			TextAlignment = TextAnchor.MiddleCenter;
			FontSize = ESleekFontSize.Default;
			TextContrastContext = GlazierConst.DefaultLabelContrastContext;
			FontStyle = GlazierConst.DefaultLabelFontStyle;
			AllowRichText = false;
		}

		protected override bool ReleaseIntoPool()
		{
			if (textComponent == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.error("Text component null when releasing GlazierLabel into uGUI pool!");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return false;
			}

			textComponent.enabled = false;
			LabelPoolData poolData = new LabelPoolData();
			PopulateBasePoolData(poolData);
			poolData.textComponent = textComponent;
			textComponent = null;
			glazier.ReleaseLabelToPool(poolData);
			return true;
		}

		protected override void EnableComponents()
		{
			textComponent.enabled = true;
		}

		public GlazierLabel_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			textComponent = gameObject.AddComponent<TextMeshProUGUI>();
			textComponent.raycastTarget = false;
			textComponent.font = GlazierResources_uGUI.Font;
			textComponent.overflowMode = GlazierConst_uGUI.DefaultOverflowMode;
			textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
			textComponent.extraPadding = GlazierConst_uGUI.DefaultExtraPadding;

			PostConstructLabel();
		}

		public class LabelPoolData : PoolData
		{
			public TextMeshProUGUI textComponent;
		}

		public void ConstructFromLabelPool(LabelPoolData poolData)
		{
			ConstructFromPool(poolData);

			textComponent = poolData.textComponent;
			textComponent.text = string.Empty;

			PostConstructLabel();
		}

		public override void SynchronizeColors()
		{
			textComponent.color = _textColor;
		}

		private TextMeshProUGUI textComponent;
	}
}
