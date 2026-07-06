////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierImage_UIToolkit : GlazierElementBase_UIToolkit, ISleekImage
	{
		public Texture Texture
		{
			get
			{
				ValidateNotDestroyed();
				return desiredTexture;
			}

			set
			{
				ValidateNotDestroyed();
				if (desiredTexture != value)
				{
					internalSetTexture(value, ShouldDestroyTexture);
				}
			}
		}

		private float _rotationAngle;
		public float RotationAngle
		{
			get
			{
				ValidateNotDestroyed();
				return _rotationAngle;
			}

			set
			{
				ValidateNotDestroyed();
				_rotationAngle = value;
				SynchronizeRotation();
			}
		}

		private bool _canRotate;
		public bool CanRotate
		{
			get
			{
				ValidateNotDestroyed();
				return _canRotate;
			}

			set
			{
				ValidateNotDestroyed();
				_canRotate = value;
				SynchronizeRotation();
			}
		}

		public bool ShouldDestroyTexture
		{
			get;
			set;
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
				imageElement.tintColor = _color;
			}
		}

		public void UpdateTexture(Texture2D newTexture)
		{
			ValidateNotDestroyed();
			if (desiredTexture != newTexture)
			{
				internalSetTexture(newTexture, ShouldDestroyTexture);
			}
		}

		public void SetTextureAndShouldDestroy(Texture2D newTexture, bool newShouldDestroyTexture)
		{
			ValidateNotDestroyed();
			if (desiredTexture != newTexture || ShouldDestroyTexture != newShouldDestroyTexture)
			{
				internalSetTexture(newTexture, newShouldDestroyTexture);
			}
		}

		public override void InternalDestroy()
		{
			if (ShouldDestroyTexture && desiredTexture != null)
			{
				Object.Destroy(desiredTexture);
				desiredTexture = null;
			}

			base.InternalDestroy();
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

		private event System.Action _onImageRightClicked;
		public event System.Action OnRightClicked
		{
			add
			{
				if (clickable == null)
				{
					CreateClickable();
				}

				_onImageRightClicked += value;
			}

			remove => _onImageRightClicked -= value;
		}

		public GlazierImage_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			containerElement = new VisualElement();
			containerElement.userData = this;
			containerElement.AddToClassList("unturned-empty");
			containerElement.pickingMode = PickingMode.Ignore; // Default to not clickable unless event is bound.

			imageElement = new Image();
			imageElement.AddToClassList("unturned-image");
			imageElement.scaleMode = ScaleMode.StretchToFill;
			imageElement.pickingMode = PickingMode.Ignore;
			containerElement.Add(imageElement);

			visualElement = containerElement;
		}

		internal override void SynchronizeColors()
		{
			imageElement.tintColor = _color;
		}

		private void internalSetTexture(Texture newTexture, bool newShouldDestroyTexture)
		{
			if (ShouldDestroyTexture && desiredTexture != null)
			{
				Object.Destroy(desiredTexture);
				desiredTexture = null;
			}

			desiredTexture = newTexture;
			ShouldDestroyTexture = newShouldDestroyTexture;

			imageElement.image = desiredTexture;
		}

		private void CreateClickable()
		{
			containerElement.pickingMode = PickingMode.Position; // Enable clicking.
			clickable = new Clickable(OnClickedWithEventInfo);
			GlazierUtils_UIToolkit.AddClickableActivators(clickable);
			containerElement.AddManipulator(clickable);
		}

		private void OnClickedWithEventInfo(EventBase eventBase)
		{
			if (eventBase is IMouseEvent mouseEvent)
			{
				switch (mouseEvent.button)
				{
					case 0:
						_onImageClicked?.Invoke();
						break;

					case 1:
						_onImageRightClicked?.Invoke();
						break;
				}
			}
		}

		private void SynchronizeRotation()
		{
			imageElement.transform.rotation = _canRotate ? Quaternion.AngleAxis(_rotationAngle, Vector3.forward) : Quaternion.identity;
		}

		private VisualElement containerElement;
		private Image imageElement;
		private Clickable clickable;
		private Texture desiredTexture;
	}
}
