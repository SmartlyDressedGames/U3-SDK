////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierSlider_uGUI : GlazierElementBase_uGUI, ISleekSlider
	{
		public event Dragged OnValueChanged;

		private ESleekOrientation _orientation = ESleekOrientation.VERTICAL;
		public ESleekOrientation Orientation
		{
			get
			{
				ValidateNotDestroyed();
				return _orientation;
			}

			set
			{
				ValidateNotDestroyed();
				if (_orientation != value)
				{
					_orientation = value;
					UpdateOrientation();
				}
			}
		}

		public float Value
		{
			get
			{
				ValidateNotDestroyed();
				return scrollbarComponent.value;
			}

			set
			{
				ValidateNotDestroyed();
				scrollbarComponent.SetValueWithoutNotify(value);
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
				backgroundImage.color = _backgroundColor;
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
				handleImage.color = _foregroundColor;
			}
		}

		public bool IsInteractable
		{
			get
			{
				ValidateNotDestroyed();
				return scrollbarComponent.interactable;
			}

			set
			{
				ValidateNotDestroyed();
				scrollbarComponent.interactable = value;
				SynchronizeColors();
			}
		}

		public GlazierSlider_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			scrollbarComponent = gameObject.AddComponent<Scrollbar>();
			scrollbarComponent.onValueChanged.AddListener(OnSliderComponentValueChanged);

			GameObject backgroundGameObject = new GameObject("Background", typeof(RectTransform));
			backgroundTransform = backgroundGameObject.GetRectTransform();
			backgroundTransform.SetParent(transform, false);
			backgroundTransform.anchoredPosition = Vector2.zero;
			backgroundImage = backgroundGameObject.AddComponent<Image>();
			backgroundImage.type = Image.Type.Sliced;
			backgroundImage.raycastTarget = true;

			GameObject handleGameObject = new GameObject("Handle", typeof(RectTransform));
			RectTransform handleTransform = handleGameObject.GetRectTransform();
			handleTransform.SetParent(transform, false);
			// Handle anchors are managed by scrollbar component.
			handleTransform.anchoredPosition = Vector2.zero;
			handleTransform.sizeDelta = Vector2.zero;
			handleImage = handleGameObject.AddComponent<Image>();
			handleImage.type = Image.Type.Sliced;
			handleImage.raycastTarget = true;

			scrollbarComponent.handleRect = handleTransform;
			scrollbarComponent.size = 0.25f; // Matches IMGUI default.

			scrollbarComponent.transition = Selectable.Transition.SpriteSwap;
			scrollbarComponent.targetGraphic = handleImage;

			_orientation = ESleekOrientation.VERTICAL; // User code assumes vertical default.
			UpdateOrientation();

			_backgroundColor = GlazierConst.DefaultSliderBackgroundColor;
			_foregroundColor = GlazierConst.DefaultSliderForegroundColor;
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
			backgroundImage.color = bgColor;
			handleImage.color = fgColor;
		}

		public override void SynchronizeTheme()
		{
			backgroundImage.sprite = GlazierResources_uGUI.Theme.SliderBackgroundSprite;
			handleImage.sprite = GlazierResources_uGUI.Theme.BoxSprite;

			SpriteState spriteState = new SpriteState();
			spriteState.disabledSprite = handleImage.sprite;
			spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
			spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
			spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
			scrollbarComponent.spriteState = spriteState;
		}

		private void UpdateOrientation()
		{
			switch (Orientation)
			{
				case ESleekOrientation.HORIZONTAL:
					backgroundTransform.anchorMin = new Vector2(0.0f, 0.5f);
					backgroundTransform.anchorMax = new Vector2(1.0f, 0.5f);
					backgroundTransform.sizeDelta = new Vector2(-20.0f, 6.0f);
					scrollbarComponent.SetDirection(Scrollbar.Direction.LeftToRight, false);
					return;

				case ESleekOrientation.VERTICAL:
					backgroundTransform.anchorMin = new Vector2(0.5f, 0.0f);
					backgroundTransform.anchorMax = new Vector2(0.5f, 1.0f);
					backgroundTransform.sizeDelta = new Vector2(6.0f, -20.0f);
					scrollbarComponent.SetDirection(Scrollbar.Direction.TopToBottom, false);
					return;
			}
		}

		private void OnSliderComponentValueChanged(float value)
		{
			OnValueChanged?.Invoke(this, value);
		}

		private Scrollbar scrollbarComponent;
		private Image backgroundImage;
		private Image handleImage;
		private RectTransform backgroundTransform;
	}
}
