////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierToggle_uGUI : GlazierElementBase_uGUI, ISleekToggle
	{
		public event Toggled OnValueChanged;

		public bool Value
		{
			get
			{
				ValidateNotDestroyed();
				return toggleComponent.isOn;
			}

			set
			{
				ValidateNotDestroyed();
				toggleComponent.SetIsOnWithoutNotify(value);
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
					tooltipComponent.color = new SleekColor(ESleekTint.FONT);
				}
				tooltipComponent.text = value;
			}
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
				backgroundImageComponent.color = _backgroundColor;
			}
		}

		private SleekColor _foregroundColor;
		public SleekColor ForegroundColor
		{
			get
			{
				ValidateNotDestroyed();
				return _foregroundColor;
			}

			set
			{
				ValidateNotDestroyed();
				_foregroundColor = value;
				foregroundImageComponent.color = _foregroundColor;
			}
		}

		public bool IsInteractable
		{
			get
			{
				ValidateNotDestroyed();
				return toggleComponent.interactable;
			}

			set
			{
				ValidateNotDestroyed();
				toggleComponent.interactable = value;
				SynchronizeColors();
			}
		}

		public GlazierToggle_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			SizeOffset_X = 40;
			SizeOffset_Y = 40;

			// Circle behind the checkmark.
			GameObject backgroundGameObject = new GameObject("Background", typeof(RectTransform));
			backgroundGameObject.transform.SetParent(transform, false);
			RectTransform backgroundTransform = backgroundGameObject.GetRectTransform();
			backgroundTransform.anchorMin = new Vector2(0.5f, 0.5f);
			backgroundTransform.anchorMax = new Vector2(0.5f, 0.5f);
			backgroundTransform.sizeDelta = new Vector2(20.0f, 20.0f);
			backgroundImageComponent = backgroundGameObject.AddComponent<Image>();
			backgroundImageComponent.enabled = false;
			backgroundImageComponent.raycastTarget = true;

			// Checkmark in front of circle.
			GameObject foregroundGameObject = new GameObject("Foreground", typeof(RectTransform));
			foregroundGameObject.transform.SetParent(transform, false);
			RectTransform foregroundTransform = foregroundGameObject.GetRectTransform();
			foregroundTransform.reset();
			foregroundImageComponent = foregroundGameObject.AddComponent<Image>();
			foregroundImageComponent.enabled = false;
			foregroundImageComponent.raycastTarget = false;

			toggleComponent = gameObject.AddComponent<Toggle>();
			toggleComponent.enabled = false;
			toggleComponent.transition = Selectable.Transition.SpriteSwap;
			toggleComponent.targetGraphic = backgroundImageComponent;
			toggleComponent.graphic = foregroundImageComponent;
			toggleComponent.onValueChanged.AddListener(uGUIonValueChanged);

			_backgroundColor = GlazierConst.DefaultToggleBackgroundColor;
			_foregroundColor = GlazierConst.DefaultToggleForegroundColor;
		}

		public override void SynchronizeColors()
		{
			Color bgColor = _backgroundColor;
			Color fgColor = _foregroundColor;
			if (!IsInteractable)
			{
				bgColor.a *= 0.25f;
				fgColor.a *= 0.25f;
			}
			backgroundImageComponent.color = bgColor;
			foregroundImageComponent.color = fgColor;

			if (tooltipComponent != null)
			{
				tooltipComponent.color = new SleekColor(ESleekTint.FONT);
			}
		}

		public override void SynchronizeTheme()
		{
			backgroundImageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;
			foregroundImageComponent.sprite = GlazierResources_uGUI.Theme.ToggleForegroundSprite;

			SpriteState spriteState = new SpriteState();
			spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
			spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
			spriteState.disabledSprite = backgroundImageComponent.sprite;
			spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
			toggleComponent.spriteState = spriteState;
		}

		protected override void EnableComponents()
		{
			backgroundImageComponent.enabled = true;
			foregroundImageComponent.enabled = true;
			toggleComponent.enabled = true;
		}

		private void uGUIonValueChanged(bool isOn)
		{
			OnValueChanged?.Invoke(this, isOn);
		}

		private Image backgroundImageComponent;
		private Image foregroundImageComponent;
		private Toggle toggleComponent;
	}
}
