////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierImage_uGUI : GlazierElementBase_uGUI, ISleekImage
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

		private float _angle;
		public float RotationAngle
		{
			get
			{
				ValidateNotDestroyed();
				return _angle;
			}

			set
			{
				ValidateNotDestroyed();
				_angle = value;
				pivotTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, -_angle);
			}
		}

		private bool _isAngled;
		public bool CanRotate
		{
			get
			{
				ValidateNotDestroyed();
				return _isAngled;
			}

			set
			{
				ValidateNotDestroyed();
				_isAngled = value;
				if (_isAngled)
				{
					pivotTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, -_angle);
				}
				else
				{
					pivotTransform.localRotation = Quaternion.identity;
				}
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
				SynchronizeColors();
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
				if (buttonComponent == null)
				{
					CreateButton();
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
				if (buttonComponent == null)
				{
					CreateButton();
				}

				_onImageRightClicked += value;
			}

			remove => _onImageRightClicked -= value;
		}

		protected override bool ReleaseIntoPool()
		{
			if (buttonComponent == null)
			{
				if (rawImageComponent == null)
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					UnturnedLog.error("Image component null when releasing GlazierImage into uGUI pool!");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					return false;
				}

				rawImageComponent.enabled = false;

				ImagePoolData poolData = new ImagePoolData();
				PopulateBasePoolData(poolData);
				poolData.pivotTransform = pivotTransform;
				pivotTransform = null;
				poolData.rawImageComponent = rawImageComponent;
				rawImageComponent = null;
				glazier.ReleaseImageToPool(poolData);
				return true;
			}
			else
			{
				return false;
			}
		}

		public GlazierImage_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			GameObject pivotGameObject = new GameObject("Pivot", typeof(RectTransform));
			pivotTransform = pivotGameObject.GetRectTransform();
			pivotTransform.SetParent(transform, false);
			pivotTransform.anchorMin = Vector2.zero;
			pivotTransform.anchorMax = Vector2.one;
			pivotTransform.anchoredPosition = Vector2.zero;
			pivotTransform.sizeDelta = Vector2.zero;

			rawImageComponent = pivotGameObject.AddComponent<RawImage>();
			rawImageComponent.enabled = true;
			rawImageComponent.raycastTarget = false;
			rawImageComponent.texture = GlazierResources.PixelTexture;
		}

		public class ImagePoolData : PoolData
		{
			public RectTransform pivotTransform;
			public RawImage rawImageComponent;
		}

		public void ConstructFromImagePool(ImagePoolData poolData)
		{
			ConstructFromPool(poolData);
			pivotTransform = poolData.pivotTransform;
			rawImageComponent = poolData.rawImageComponent;

			// Old transform may have been modified by layout components.
			pivotTransform.anchorMin = Vector2.zero;
			pivotTransform.anchorMax = Vector2.one;
			pivotTransform.anchoredPosition = Vector2.zero;
			pivotTransform.sizeDelta = Vector2.zero;

			pivotTransform.localRotation = Quaternion.identity;
			rawImageComponent.texture = GlazierResources.PixelTexture;
		}

		public override void SynchronizeColors()
		{
			if (desiredTexture != null)
			{
				rawImageComponent.color = _color;
				rawImageComponent.enabled = true;
			}
			else
			{
				// For invisible buttons we need image enabled.
				if (rawImageComponent.raycastTarget)
				{
					rawImageComponent.color = ColorEx.BlackZeroAlpha;
					rawImageComponent.enabled = true;
				}
				else
				{
					rawImageComponent.enabled = false;
				}
			}
		}

		protected override void EnableComponents()
		{
			// For invisible buttons we need image enabled.
			rawImageComponent.enabled = desiredTexture != null || rawImageComponent.raycastTarget;
		}

		private void CreateButton()
		{
			rawImageComponent.raycastTarget = true;

			buttonComponent = gameObject.AddComponent<ButtonEx>();
			buttonComponent.transition = Selectable.Transition.None;
			buttonComponent.onClick.AddListener(OnUnityClick);
			buttonComponent.onRightClick.AddListener(OnUnityRightClick);

			SynchronizeColors(); // Refer to impl for explanation.
		}

		private void internalSetTexture(Texture newTexture, bool newShouldDestroyTexture)
		{
			if (rawImageComponent == null)
			{
				// Download image callback may be trying to set image after we have been destroyed.
				if (newShouldDestroyTexture && newTexture != null)
				{
					Object.Destroy(newTexture);
				}
				return;
			}

			if (ShouldDestroyTexture && desiredTexture != null)
			{
				Object.Destroy(desiredTexture);
				desiredTexture = null;
			}

			desiredTexture = newTexture;
			ShouldDestroyTexture = newShouldDestroyTexture;

			rawImageComponent.texture = desiredTexture != null ? desiredTexture : GlazierResources.PixelTexture;

			SynchronizeColors(); // Refer to impl for explanation.
		}

		private void OnUnityClick()
		{
			_onImageClicked?.Invoke();
		}

		private void OnUnityRightClick()
		{
			_onImageRightClicked?.Invoke();
		}

		/// <summary>
		/// The base transform does not rotate, instead a child transform is created with the pivot in the center.
		/// </summary>
		private RectTransform pivotTransform;

		/// <summary>
		/// To work around a uGUI bug we always a sign a texture, even if desiredTexture is null.
		/// </summary>
		private Texture desiredTexture;

		private RawImage rawImageComponent;
		private ButtonEx buttonComponent;
	}
}
