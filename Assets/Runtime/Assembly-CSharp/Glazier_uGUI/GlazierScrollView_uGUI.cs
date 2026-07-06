////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class ScrollRectEx : ScrollRect
	{
		public override void OnScroll(PointerEventData data)
		{
			if (HandleScrollWheel)
			{
				base.OnScroll(data);
			}
			else if (transform.parent != null)
			{
				ScrollRect parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
				if (parentScrollRect != null)
				{
					parentScrollRect.OnScroll(data);
				}
			}
		}

		[SerializeField]
		public bool HandleScrollWheel = true;
	}

	internal class GlazierScrollView_uGUI : GlazierElementBase_uGUI, ISleekScrollView
	{
		private bool _scaleContentToWidth;
		public bool ScaleContentToWidth
		{
			get
			{
				ValidateNotDestroyed();
				return _scaleContentToWidth;
			}

			set
			{
				ValidateNotDestroyed();
				_scaleContentToWidth = value;
				contentTransform.anchorMax = new Vector2(_scaleContentToWidth ? _contentScaleFactor : 0.0f, 1.0f);
			}
		}

		private bool _scaleContentToHeight;
		public bool ScaleContentToHeight
		{
			get
			{
				ValidateNotDestroyed();
				return _scaleContentToHeight;
			}

			set
			{
				ValidateNotDestroyed();
				_scaleContentToHeight = value;
				contentTransform.anchorMin = new Vector2(0.0f, _scaleContentToHeight ? (1.0f - _contentScaleFactor) : 1.0f);
			}
		}

		private float _contentScaleFactor = 1.0f;
		public float ContentScaleFactor
		{
			get
			{
				ValidateNotDestroyed();
				return _contentScaleFactor;
			}

			set
			{
				ValidateNotDestroyed();
				_contentScaleFactor = value;
				contentTransform.anchorMin = new Vector2(0.0f, _scaleContentToHeight ? (1.0f - _contentScaleFactor) : 1.0f);
				contentTransform.anchorMax = new Vector2(_scaleContentToWidth ? ContentScaleFactor : 0.0f, 1.0f);
			}
		}

		private bool _reduceWidthWhenScrollbarVisible = true;
		public bool ReduceWidthWhenScrollbarVisible
		{
			get
			{
				ValidateNotDestroyed();
				return _reduceWidthWhenScrollbarVisible;
			}

			set
			{
				ValidateNotDestroyed();
				_reduceWidthWhenScrollbarVisible = value;
				scrollRectComponent.verticalScrollbarVisibility = value ?
					ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport :
					ScrollRect.ScrollbarVisibility.AutoHide;
			}
		}

		private ESleekScrollbarVisibility _verticalScrollbarVisibility;
		public ESleekScrollbarVisibility VerticalScrollbarVisibility
		{
			get
			{
				ValidateNotDestroyed();
				return _verticalScrollbarVisibility;
			}

			set
			{
				ValidateNotDestroyed();
				if (_verticalScrollbarVisibility != value)
				{
					_verticalScrollbarVisibility = value;
					verticalScrollbarBackgroundImage.gameObject.SetActive(_verticalScrollbarVisibility != ESleekScrollbarVisibility.Hidden);
					verticalScrollbarHandleImage.gameObject.SetActive(_verticalScrollbarVisibility != ESleekScrollbarVisibility.Hidden);
				}
			}
		}

		public Vector2 ContentSizeOffset
		{
			get
			{
				ValidateNotDestroyed();
				return contentTransform.sizeDelta;
			}

			set
			{
				ValidateNotDestroyed();
				contentTransform.sizeDelta = value;

				// Hack to ensure scrollbars are up-to-date.
				scrollRectComponent.Rebuild(CanvasUpdate.PostLayout);

				ClampScrollBars();
			}
		}

		public Vector2 NormalizedStateCenter
		{
			get
			{
				ValidateNotDestroyed();
				Rect viewportRect = scrollRectComponent.viewport.GetAbsoluteRect();
				Rect contentRect = contentTransform.GetAbsoluteRect();
				Vector2 centerOfViewport = viewportRect.center;
				return new Vector2((centerOfViewport.x - contentRect.xMin) / contentRect.width, (centerOfViewport.y - contentRect.yMin) / contentRect.height);
			}

			set
			{
				ValidateNotDestroyed();
				Rect viewportRect = scrollRectComponent.viewport.GetAbsoluteRect();
				Rect contentRect = contentTransform.GetAbsoluteRect();
				float uiScale = GraphicsSettings.userInterfaceScale;
				contentTransform.anchoredPosition = new Vector2((value.x * -contentRect.width) + (viewportRect.width * 0.5f),
					(value.y * contentRect.height) - (viewportRect.height * 0.5f)) / uiScale;
			}
		}

		public bool HandleScrollWheel
		{
			get
			{
				ValidateNotDestroyed();
				return scrollRectComponent.HandleScrollWheel;
			}

			set
			{
				ValidateNotDestroyed();
				scrollRectComponent.HandleScrollWheel = value;
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
				horizontalScrollbarBackgroundImage.color = _backgroundColor;
				verticalScrollbarBackgroundImage.color = _backgroundColor;
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
				horizontalScrollbarHandleImage.color = _foregroundColor;
				verticalScrollbarHandleImage.color = _foregroundColor;
			}
		}

		public event System.Action<Vector2> OnNormalizedValueChanged;

		public float NormalizedVerticalPosition
		{
			get
			{
				ValidateNotDestroyed();
				return 1.0f - scrollRectComponent.verticalNormalizedPosition;
			}
		}

		public float NormalizedViewportHeight
		{
			get
			{
				ValidateNotDestroyed();
				return scrollRectComponent.verticalScrollbar.size;
			}
		}

		protected bool _contentUseManualLayout = true;
		public bool ContentUseManualLayout
		{
			get
			{
				ValidateNotDestroyed();
				return _contentUseManualLayout;
			}

			set
			{
				ValidateNotDestroyed();
				if (_contentUseManualLayout != value)
				{
					_contentUseManualLayout = value;
					if (_contentUseManualLayout)
					{
						contentTransform.DestroyComponentIfExists<VerticalLayoutGroup>();
						contentTransform.DestroyComponentIfExists<ContentSizeFitter>();
					}
					else
					{
						VerticalLayoutGroup layoutGroup = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
						
						ContentSizeFitter sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
						sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
					}
				}
			}
		}

		protected bool _alignContentToBottom;
		public bool AlignContentToBottom
		{
			get
			{
				ValidateNotDestroyed();
				return _alignContentToBottom;
			}

			set
			{
				ValidateNotDestroyed();
				_alignContentToBottom = value;
				contentTransform.pivot = new Vector2(0.0f, _alignContentToBottom ? 0.0f : 1.0f);
			}
		}

		public bool IsRaycastTarget
		{
			get
			{
				ValidateNotDestroyed();
				return contentImage.raycastTarget;
			}

			set
			{
				ValidateNotDestroyed();
				contentImage.raycastTarget = value;
			}
		}

		public void ScrollToTop()
		{
			ValidateNotDestroyed();
			scrollRectComponent.verticalNormalizedPosition = 1.0f;
		}

		public void ScrollToBottom()
		{
			ValidateNotDestroyed();
			scrollRectComponent.verticalNormalizedPosition = 0.0f;
		}

		public GlazierScrollView_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			scrollRectComponent = gameObject.AddComponent<ScrollRectEx>();
			scrollRectComponent.movementType = ScrollRect.MovementType.Clamped;
			scrollRectComponent.inertia = false;
			scrollRectComponent.onValueChanged.AddListener(OnUnityValueChanged);
			scrollRectComponent.scrollSensitivity = 40.0f * GlazierBase.ScrollViewSensitivityMultiplier;

			GameObject viewportGameObject = new GameObject("Viewport", typeof(RectTransform));
			RectTransform viewportTransform = viewportGameObject.GetRectTransform();
			viewportTransform.SetParent(transform, false);
			viewportTransform.pivot = new Vector2(0.0f, 1.0f); // Affects scrollbar spacing.
			viewportTransform.anchorMin = Vector2.zero;
			viewportTransform.anchorMax = Vector2.one;
			viewportTransform.anchoredPosition = Vector2.zero;
			viewportTransform.sizeDelta = Vector2.zero;
			viewportGameObject.AddComponent<RectMask2D>();
			scrollRectComponent.viewport = viewportTransform;

			GameObject contentGameObject = new GameObject("Content", typeof(RectTransform));
			contentTransform = contentGameObject.GetRectTransform();
			contentTransform.SetParent(viewportTransform, false);
			contentTransform.pivot = new Vector2(0.0f, 1.0f);
			contentTransform.anchorMin = new Vector2(0.0f, 1.0f);
			contentTransform.anchorMax = new Vector2(0.0f, 1.0f);
			contentTransform.anchoredPosition = Vector2.zero;
			contentTransform.sizeDelta = Vector2.zero;
			scrollRectComponent.content = contentTransform;

			// Invisible image on content transform allows scroll wheel handling when hovering empty space.
			contentImage = contentGameObject.AddComponent<Image>();
			contentImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

			// Horizontal Scrollbar
			{
				GameObject horizontalScrollbarGameObject = new GameObject("Horizontal Scrollbar", typeof(RectTransform));
				RectTransform horizontalScrollbarTransform = horizontalScrollbarGameObject.GetRectTransform();
				horizontalScrollbarTransform.SetParent(transform, false);
				horizontalScrollbarTransform.pivot = new Vector2(0.0f, 0.0f);
				horizontalScrollbarTransform.anchorMin = new Vector2(0.0f, 0.0f);
				horizontalScrollbarTransform.anchorMax = new Vector2(1.0f, 0.0f);
				horizontalScrollbarTransform.anchoredPosition = Vector2.zero;
				horizontalScrollbarTransform.sizeDelta = new Vector2(0.0f, 20.0f);

				GameObject horizontalScrollbarBackgroundGameObject = new GameObject("Background", typeof(RectTransform));
				RectTransform horizontalScrollbarBackgroundTransform = horizontalScrollbarBackgroundGameObject.GetRectTransform();
				horizontalScrollbarBackgroundTransform.SetParent(horizontalScrollbarTransform, false);
				horizontalScrollbarBackgroundTransform.anchorMin = new Vector2(0.0f, 0.5f);
				horizontalScrollbarBackgroundTransform.anchorMax = new Vector2(1.0f, 0.5f);
				horizontalScrollbarBackgroundTransform.anchoredPosition = Vector2.zero;
				horizontalScrollbarBackgroundTransform.sizeDelta = new Vector2(-20.0f, 6.0f);
				horizontalScrollbarBackgroundImage = horizontalScrollbarBackgroundGameObject.AddComponent<Image>();
				horizontalScrollbarBackgroundImage.type = Image.Type.Sliced;
				horizontalScrollbarBackgroundImage.raycastTarget = true;

				GameObject horizontalScrollbarHandlePaddingGameObject = new GameObject("Handle Padding", typeof(RectTransform));
				RectTransform horizontalScrollbarHandlePaddingTransform = horizontalScrollbarHandlePaddingGameObject.GetRectTransform();
				horizontalScrollbarHandlePaddingTransform.SetParent(horizontalScrollbarTransform, false);
				horizontalScrollbarHandlePaddingTransform.anchorMin = Vector2.zero;
				horizontalScrollbarHandlePaddingTransform.anchorMax = Vector2.one;
				horizontalScrollbarHandlePaddingTransform.anchoredPosition = Vector2.zero;
				horizontalScrollbarHandlePaddingTransform.sizeDelta = new Vector2(-20.0f, 0.0f);

				GameObject horizontalScrollbarHandleGameObject = new GameObject("Handle", typeof(RectTransform));
				RectTransform horizontalScrollbarHandleTransform = horizontalScrollbarHandleGameObject.GetRectTransform();
				horizontalScrollbarHandleTransform.SetParent(horizontalScrollbarHandlePaddingTransform, false);
				horizontalScrollbarHandleTransform.anchoredPosition = Vector2.zero;
				horizontalScrollbarHandleTransform.sizeDelta = new Vector2(20.0f, 0.0f);
				horizontalScrollbarHandleImage = horizontalScrollbarHandleGameObject.AddComponent<Image>();
				horizontalScrollbarHandleImage.type = Image.Type.Sliced;
				horizontalScrollbarHandleImage.raycastTarget = true;

				horizontalScrollbarComponent = horizontalScrollbarGameObject.AddComponent<Scrollbar>();
				horizontalScrollbarComponent.SetDirection(Scrollbar.Direction.LeftToRight, false);
				horizontalScrollbarComponent.handleRect = horizontalScrollbarHandleTransform;
				horizontalScrollbarComponent.transition = Selectable.Transition.SpriteSwap;
				horizontalScrollbarComponent.targetGraphic = horizontalScrollbarHandleImage;

				scrollRectComponent.horizontalScrollbarSpacing = 10.0f;
				scrollRectComponent.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
				scrollRectComponent.horizontalScrollbar = horizontalScrollbarComponent;
			}

			// Vertical Scrollbar
			{
				GameObject verticalScrollbarGameObject = new GameObject("Vertical Scrollbar", typeof(RectTransform));
				RectTransform verticalScrollbarTransform = verticalScrollbarGameObject.GetRectTransform();
				verticalScrollbarTransform.SetParent(transform, false);
				verticalScrollbarTransform.pivot = new Vector2(1.0f, 1.0f);
				verticalScrollbarTransform.anchorMin = new Vector2(1.0f, 0.0f);
				verticalScrollbarTransform.anchorMax = new Vector2(1.0f, 1.0f);
				verticalScrollbarTransform.anchoredPosition = Vector2.zero;
				verticalScrollbarTransform.sizeDelta = new Vector2(20.0f, 0.0f);

				GameObject verticalScrollbarBackgroundGameObject = new GameObject("Background", typeof(RectTransform));
				RectTransform verticalScrollbarBackgroundTransform = verticalScrollbarBackgroundGameObject.GetRectTransform();
				verticalScrollbarBackgroundTransform.SetParent(verticalScrollbarTransform, false);
				verticalScrollbarBackgroundTransform.anchorMin = new Vector2(0.5f, 0.0f);
				verticalScrollbarBackgroundTransform.anchorMax = new Vector2(0.5f, 1.0f);
				verticalScrollbarBackgroundTransform.anchoredPosition = Vector2.zero;
				verticalScrollbarBackgroundTransform.sizeDelta = new Vector2(6.0f, -20.0f);
				verticalScrollbarBackgroundImage = verticalScrollbarBackgroundGameObject.AddComponent<Image>();
				verticalScrollbarBackgroundImage.type = Image.Type.Sliced;
				verticalScrollbarBackgroundImage.raycastTarget = true;

				GameObject verticalScrollbarHandlePaddingGameObject = new GameObject("Handle Padding", typeof(RectTransform));
				RectTransform verticalScrollbarHandlePaddingTransform = verticalScrollbarHandlePaddingGameObject.GetRectTransform();
				verticalScrollbarHandlePaddingTransform.SetParent(verticalScrollbarTransform, false);
				verticalScrollbarHandlePaddingTransform.anchorMin = Vector2.zero;
				verticalScrollbarHandlePaddingTransform.anchorMax = Vector2.one;
				verticalScrollbarHandlePaddingTransform.anchoredPosition = Vector2.zero;
				verticalScrollbarHandlePaddingTransform.sizeDelta = new Vector2(0.0f, -20.0f);

				GameObject verticalScrollbarHandleGameObject = new GameObject("Handle", typeof(RectTransform));
				RectTransform verticalScrollbarHandleTransform = verticalScrollbarHandleGameObject.GetRectTransform();
				verticalScrollbarHandleTransform.SetParent(verticalScrollbarHandlePaddingTransform, false);
				verticalScrollbarHandleTransform.anchoredPosition = Vector2.zero;
				verticalScrollbarHandleTransform.sizeDelta = new Vector2(0.0f, 20.0f);
				verticalScrollbarHandleImage = verticalScrollbarHandleGameObject.AddComponent<Image>();
				verticalScrollbarHandleImage.type = Image.Type.Sliced;
				verticalScrollbarHandleImage.raycastTarget = true;

				verticalScrollbarComponent = verticalScrollbarGameObject.AddComponent<Scrollbar>();
				verticalScrollbarComponent.SetDirection(Scrollbar.Direction.BottomToTop, false);
				verticalScrollbarComponent.handleRect = verticalScrollbarHandleTransform;
				verticalScrollbarComponent.transition = Selectable.Transition.SpriteSwap;
				verticalScrollbarComponent.targetGraphic = verticalScrollbarHandleImage;

				scrollRectComponent.verticalScrollbarSpacing = 10.0f;
				scrollRectComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
				scrollRectComponent.verticalScrollbar = verticalScrollbarComponent;
			}

			HandleScrollWheel = true;
			_backgroundColor = GlazierConst.DefaultScrollViewBackgroundColor;
			_foregroundColor = GlazierConst.DefaultScrollViewForegroundColor;
		}

		public override void SynchronizeColors()
		{
			horizontalScrollbarBackgroundImage.color = _backgroundColor;
			horizontalScrollbarHandleImage.color = _foregroundColor;

			verticalScrollbarBackgroundImage.color = _backgroundColor;
			verticalScrollbarHandleImage.color = _foregroundColor;
		}

		public override void SynchronizeTheme()
		{
			SpriteState spriteState = new SpriteState();
			spriteState.disabledSprite = GlazierResources_uGUI.Theme.BoxSprite;
			spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
			spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
			spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;

			horizontalScrollbarBackgroundImage.sprite = GlazierResources_uGUI.Theme.SliderBackgroundSprite;
			horizontalScrollbarHandleImage.sprite = GlazierResources_uGUI.Theme.BoxSprite;
			horizontalScrollbarComponent.spriteState = spriteState;

			verticalScrollbarBackgroundImage.sprite = GlazierResources_uGUI.Theme.SliderBackgroundSprite;
			verticalScrollbarHandleImage.sprite = GlazierResources_uGUI.Theme.BoxSprite;
			verticalScrollbarComponent.spriteState = spriteState;
		}

		public override RectTransform AttachmentTransform => contentTransform;

		private void ClampScrollBars()
		{
			// normalizedPosition does actually return outside the [0, 1] range, so we clamp the content into viewport.
			Vector2 normalizedPosition = scrollRectComponent.normalizedPosition;
			normalizedPosition = MathfEx.Clamp01(normalizedPosition);
			scrollRectComponent.normalizedPosition = normalizedPosition;
		}

		private void OnUnityValueChanged(Vector2 value)
		{
			value.y = 1.0f - value.y;
			OnNormalizedValueChanged?.Invoke(value);
		}

		private ScrollRectEx scrollRectComponent;
		private RectTransform contentTransform;
		private Image contentImage;

		private Image horizontalScrollbarBackgroundImage;
		private Image horizontalScrollbarHandleImage;
		private Scrollbar horizontalScrollbarComponent;

		private Image verticalScrollbarBackgroundImage;
		private Image verticalScrollbarHandleImage;
		private Scrollbar verticalScrollbarComponent;
	}
}
