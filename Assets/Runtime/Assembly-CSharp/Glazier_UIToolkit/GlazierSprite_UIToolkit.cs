////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierSprite_UIToolkit : GlazierElementBase_UIToolkit, ISleekSprite
	{
		private Sprite _sprite;
		public Sprite Sprite
		{
			get
			{
				ValidateNotDestroyed();
				return _sprite;
			}

			set
			{
				ValidateNotDestroyed();
				_sprite = value;
				SynchronizeImage();
			}
		}

		private SleekColor _color = ESleekTint.NONE;
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
                control.tintColor = _color;
				control.style.unityBackgroundImageTintColor = _color;
			}
		}

		private ESleekSpriteType _drawMethod = ESleekSpriteType.Tiled;
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
				SynchronizeImage();
			}
		}

		public bool IsRaycastTarget
		{
			get => throw new System.NotImplementedException();
			set => throw new System.NotImplementedException();
		}

		private Vector2Int _tileRepeat;
		public Vector2Int TileRepeatHintForUITK
		{
			get => _tileRepeat;
			set
			{
				if (_tileRepeat != value)
				{
					_tileRepeat = value;
					SynchronizeImage();
				}
			}
		}

		private event System.Action _onImageClicked;
		public event System.Action OnClicked
		{
			add
			{
				if (clickable == null)
				{
					CreateClickable();
				}

				_onImageClicked += value;
			}

			remove => _onImageClicked -= value;
		}

		public GlazierSprite_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			control = new Image();
			control.userData = this;
			control.AddToClassList("unturned-sprite");
			control.pickingMode = PickingMode.Ignore; // Default to not clickable unless event is bound.
			control.scaleMode = ScaleMode.StretchToFill;
			
			visualElement = control;
		}

		internal override void SynchronizeColors()
		{
			control.tintColor = _color;
			control.style.unityBackgroundImageTintColor = _color;

			if (hackTiledImages != null)
			{
				foreach (Image tile in hackTiledImages)
				{
					tile.tintColor = _color;
				}
			}
		}

		private void CreateClickable()
		{
			control.pickingMode = PickingMode.Position; // Enable clicking.
			clickable = new Clickable(OnClickedWithEventInfo);
			GlazierUtils_UIToolkit.AddClickableActivators(clickable);
			control.AddManipulator(clickable);
		}

		private void SynchronizeImage()
		{
			switch (_drawMethod)
			{
				case ESleekSpriteType.Regular:
					control.sprite = _sprite;
					control.style.backgroundImage = StyleKeyword.Null;
					DestroyTiledImages();
					break;

				case ESleekSpriteType.Tiled:
					control.sprite = null;
					control.style.backgroundImage = StyleKeyword.Null;
					UpdateTiledImages();
					break;

				case ESleekSpriteType.Sliced:
					control.sprite = null;
					control.style.backgroundImage = _sprite?.texture;
					DestroyTiledImages();
					break;
			}
		}

		private void DestroyTiledImages()
		{
			if (tiledImagesContainer != null)
			{
				tiledImagesContainer.RemoveFromHierarchy();
				tiledImagesContainer = null;
			}

			hackTiledImages = null;
		}

		private void UpdateTiledImages()
		{
			int tileCount = _tileRepeat.x * _tileRepeat.y;
			if (tileCount < 1 || _sprite == null)
			{
				if (tiledImagesContainer != null)
				{
					tiledImagesContainer.RemoveFromHierarchy();
				}
				return;
			}

			if (tiledImagesContainer == null)
			{
				tiledImagesContainer = new VisualElement();
				tiledImagesContainer.AddToClassList("unturned-empty");
				tiledImagesContainer.pickingMode = PickingMode.Ignore;
				tiledImagesContainer.style.position = Position.Absolute;
				tiledImagesContainer.style.left = 0;
				tiledImagesContainer.style.right = 0;
				tiledImagesContainer.style.top = 0;
				tiledImagesContainer.style.bottom = 0;
			}

			if (tiledImagesContainer.parent != visualElement)
			{
				visualElement.Add(tiledImagesContainer);
				tiledImagesContainer.SendToBack();
			}

			if (hackTiledImages == null)
			{
				hackTiledImages = new List<Image>(tileCount);
			}
			else
			{
				hackTiledImages.Capacity = Mathf.Max(hackTiledImages.Capacity, tileCount);
			}

			if (hackTiledImages.Count > tileCount)
			{
				// Hide extra images.
				for (int index = hackTiledImages.Count - 1; index >= tileCount; --index)
				{
					hackTiledImages[index].RemoveFromHierarchy();
				}
			}
			else if (hackTiledImages.Count < tileCount)
			{
				for (int index = hackTiledImages.Count; index < tileCount; ++index)
				{
					Image tile = new Image();
					tile.AddToClassList("unturned-sprite");
					tile.style.position = Position.Absolute;
					tile.pickingMode = PickingMode.Ignore;
					tile.scaleMode = ScaleMode.StretchToFill;
					tiledImagesContainer.Add(tile);
					hackTiledImages.Add(tile);
				}
			}

			float width = 100.0f / _tileRepeat.x;
			float height = 100.0f / _tileRepeat.y;

			for (int index = 0; index < tileCount; ++index)
			{
				Image tile = hackTiledImages[index];
				if (tile.parent == null)
				{
					tiledImagesContainer.Add(tile);
				}
				tile.sprite = _sprite;
				tile.tintColor = _color;

				int row = index / _tileRepeat.x;
				int column = index % _tileRepeat.x;
				tile.style.left = Length.Percent(column * width);
				tile.style.top = Length.Percent(row * height);
				tile.style.width = Length.Percent(width);
				tile.style.height = Length.Percent(height);
			}
		}

		private void OnClickedWithEventInfo(EventBase eventBase)
		{
			_onImageClicked?.Invoke();
		}

		private Image control;
		private Clickable clickable;

		private VisualElement tiledImagesContainer;
		private List<Image> hackTiledImages;
	}
}
