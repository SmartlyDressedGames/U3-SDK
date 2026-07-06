////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierSprite_uGUI : GlazierElementBase_uGUI, ISleekSprite
	{
		public Sprite Sprite
		{
			get
			{
				ValidateNotDestroyed();
				return imageComponent.sprite;
			}

			set
			{
				ValidateNotDestroyed();
				imageComponent.sprite = value;
			}
		}

		private SleekColor _color;
		public SleekColor TintColor
		{
			get
			{
				ValidateNotDestroyed();
				return _color;
			}

			set
			{
				ValidateNotDestroyed();
				_color = value;
				SynchronizeColors();
			}
		}

		private ESleekSpriteType _drawMethod;
		public ESleekSpriteType DrawMethod
		{
			get
			{
				ValidateNotDestroyed();
				return _drawMethod;
			}

			set
			{
				ValidateNotDestroyed();
				_drawMethod = value;

				switch (value)
				{
					case ESleekSpriteType.Tiled:
						imageComponent.type = Image.Type.Tiled;
						break;

					case ESleekSpriteType.Sliced:
						imageComponent.type = Image.Type.Sliced;
						break;

					default:
					case ESleekSpriteType.Regular:
						imageComponent.type = Image.Type.Simple;
						break;
				}
			}
		}

		public bool IsRaycastTarget
		{
			get => throw new System.NotImplementedException();
			set => throw new System.NotImplementedException();
		}

		public Vector2Int TileRepeatHintForUITK
		{
			get;
			set;
		}

		private event System.Action _onImageClicked;
		public event System.Action OnClicked
		{
			add
			{
				if (buttonComponent == null)
				{
					CreateButton();
				}

				_onImageClicked += value;
			}

			remove => _onImageClicked -= value;
		}

		public GlazierSprite_uGUI(Glazier_uGUI glazier, Sprite sprite) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			imageComponent = gameObject.AddComponent<Image>();
			imageComponent.enabled = false;
			imageComponent.raycastTarget = false;
			imageComponent.sprite = Sprite;
			_color = ESleekTint.NONE;
			DrawMethod = ESleekSpriteType.Tiled;
		}

		public override void SynchronizeColors()
		{
			if (Sprite != null)
			{
				imageComponent.color = _color;
				imageComponent.enabled = true;
			}
			else
			{
				// Image shows a white square if texture is null, but for invisible buttons we need image enabled.
				if (imageComponent.raycastTarget)
				{
					imageComponent.color = ColorEx.BlackZeroAlpha;
					imageComponent.enabled = true;
				}
				else
				{
					imageComponent.enabled = false;
				}
			}
		}

		protected override void EnableComponents()
		{
			// Image shows a white square if texture is null, but for invisible buttons we need image enabled.
			imageComponent.enabled = Sprite != null || imageComponent.raycastTarget;
		}

		private void CreateButton()
		{
			imageComponent.raycastTarget = true;

			buttonComponent = gameObject.AddComponent<ButtonEx>();
			buttonComponent.transition = Selectable.Transition.None;
			buttonComponent.onClick.AddListener(OnUnityClick);

			SynchronizeColors(); // Refer to impl for explanation.
		}

		private void OnUnityClick()
		{
			_onImageClicked?.Invoke();
		}

		private Image imageComponent;
		private ButtonEx buttonComponent;
	}
}
