////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierScrollView_UIToolkit : GlazierElementBase_UIToolkit, ISleekScrollView
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
				SynchronizeContentContainerStyle();
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
				SynchronizeContentContainerStyle();
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
				SynchronizeContentContainerStyle();
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
				// todo
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
				_verticalScrollbarVisibility = value;
				control.verticalScrollerVisibility = _verticalScrollbarVisibility == ESleekScrollbarVisibility.Hidden ? ScrollerVisibility.Hidden : ScrollerVisibility.Auto;
			}
		}

		private Vector2 _contentSizeOffset;
		public Vector2 ContentSizeOffset
		{
			get
			{
				ValidateNotDestroyed();
				return _contentSizeOffset;
			}

			set
			{
				ValidateNotDestroyed();
				_contentSizeOffset = value;
				SynchronizeContentContainerStyle();
			}
		}

		public Vector2 NormalizedStateCenter
		{
			get
			{
				ValidateNotDestroyed();
				// todo, this isn't correct with the overflow problem
				return new Vector2(NormalizedHorizontalPosition + NormalizedViewportWidth * 0.5f, NormalizedVerticalPosition + NormalizedViewportHeight * 0.5f);
			}

			set
			{
				ValidateNotDestroyed();
				// todo, this isn't correct with the overflow problem
				NormalizedHorizontalPosition = value.x - NormalizedViewportWidth * 0.5f;
				NormalizedVerticalPosition = value.y - NormalizedViewportHeight * 0.5f;
			}
		}

		private const float MOUSE_WHEEL_SCROLL_SIZE = 600.0f; // Default value is wayyy too low.
		private bool _handleScrollWheel = true;
		public bool HandleScrollWheel
		{
			get
			{
				ValidateNotDestroyed();
				return false; // todo
			}

			set
			{
				ValidateNotDestroyed();
				_handleScrollWheel = value;
				control.mouseWheelScrollSize = _handleScrollWheel
					? (MOUSE_WHEEL_SCROLL_SIZE * GlazierBase.ScrollViewSensitivityMultiplier) : 0.0f;
			}
		}

		private SleekColor _backgroundColor = GlazierConst.DefaultScrollViewBackgroundColor;
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
				horizontalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
				verticalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
			}
		}

		private SleekColor _foregroundColor = GlazierConst.DefaultScrollViewForegroundColor;
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
				horizontalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
				verticalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
			}
		}

		public event System.Action<Vector2> OnNormalizedValueChanged;

		private float NormalizedHorizontalPosition
		{
			get
			{
				ValidateNotDestroyed();
				Scroller horizontalScroller = control.horizontalScroller;
				return Mathf.InverseLerp(horizontalScroller.lowValue, horizontalScroller.highValue, horizontalScroller.value);
			}

			set
			{
				ValidateNotDestroyed();
				Scroller horizontalScroller = control.horizontalScroller;
				horizontalScroller.value = Mathf.Lerp(horizontalScroller.lowValue, horizontalScroller.highValue, value);
			}
		}

		public float NormalizedVerticalPosition
		{
			get
			{
				ValidateNotDestroyed();
				Scroller verticalScroller = control.verticalScroller;
				return Mathf.InverseLerp(verticalScroller.lowValue, verticalScroller.highValue, verticalScroller.value);
			}

			private set
			{
				ValidateNotDestroyed();
				Scroller verticalScroller = control.verticalScroller;
				verticalScroller.value = Mathf.Lerp(verticalScroller.lowValue, verticalScroller.highValue, value);
			}
		}

		private float NormalizedViewportWidth
		{
			get
			{
				ValidateNotDestroyed();
				return control.contentViewport.layout.width / control.contentContainer.localBound.width;
			}
		}

		public float NormalizedViewportHeight
		{
			get
			{
				ValidateNotDestroyed();
				return control.contentViewport.layout.height / control.contentContainer.localBound.height;
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
				_contentUseManualLayout = value;
				SynchronizeContentContainerStyle();
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
				if (_alignContentToBottom != value)
				{
					_alignContentToBottom = value;
					contentViewport.style.justifyContent = _alignContentToBottom ? Justify.FlexEnd : StyleKeyword.Null;
				}
			}
		}

		private bool _isRaycastTarget = true;
		public bool IsRaycastTarget
		{
			get
			{
				ValidateNotDestroyed();
				return _isRaycastTarget;
			}

			set
			{
				ValidateNotDestroyed();
				_isRaycastTarget = value;
			}
		}

		public void ScrollToTop()
		{
			ValidateNotDestroyed();

			control.verticalScroller.value = control.verticalScroller.lowValue;
			wantsToScrollToBottom = false;
		}

		public void ScrollToBottom()
		{
			ValidateNotDestroyed();

			// Defer scrolling until Update so newest highValue is used. For example, editor spawn tables
			// call ScrollToBottom when adding a new entry, so highValue would be out of date until Update.
			wantsToScrollToBottom = true;
		}

		public GlazierScrollView_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			control = new ScrollView();
			control.userData = this;
			control.AddToClassList("unturned-scroll-view");

			control.horizontalScroller.valueChanged += OnHorizontalValueChanged;
			control.verticalScroller.valueChanged += OnVerticalValueChanged;
			control.mouseWheelScrollSize = MOUSE_WHEEL_SCROLL_SIZE * GlazierBase.ScrollViewSensitivityMultiplier;

			contentViewport = control.contentViewport;
			contentContainer = control.contentContainer;

			// Disable picking on control and viewports, otherwise viewport of SleekItems horizontal
			// scroll view blocks the background item drop handler.
			control.pickingMode = PickingMode.Ignore;
			control.Q(className: ScrollView.contentAndVerticalScrollUssClassName).pickingMode = PickingMode.Ignore;
			contentViewport.pickingMode = PickingMode.Ignore;

			VisualElement horizontalDragContainer = control.horizontalScroller.Q(className: "unity-base-slider__input").Q(className: "unity-base-slider__drag-container");
			horizontalTracker = horizontalDragContainer.Q(className: "unity-base-slider__tracker");
			horizontalDragger = horizontalDragContainer.Q(className: "unity-base-slider__dragger");

			VisualElement verticalDragContainer = control.verticalScroller.Q(className: "unity-base-slider__input").Q(className: "unity-base-slider__drag-container");
			verticalTracker = verticalDragContainer.Q(className: "unity-base-slider__tracker");
			verticalDragger = verticalDragContainer.Q(className: "unity-base-slider__dragger");

			visualElement = control;
		}

		public override void Update()
		{
			base.Update();

			if (wantsToScrollToBottom)
			{
				wantsToScrollToBottom = false;
				control.verticalScroller.value = control.verticalScroller.highValue;
			}
		}

		internal override void SynchronizeColors()
		{
			horizontalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
			horizontalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
			verticalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
			verticalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
		}

		private void OnHorizontalValueChanged(float value)
		{
			OnNormalizedValueChanged?.Invoke(new Vector2(NormalizedHorizontalPosition, NormalizedVerticalPosition));
		}

		private void OnVerticalValueChanged(float value)
		{
			OnNormalizedValueChanged?.Invoke(new Vector2(NormalizedHorizontalPosition, NormalizedVerticalPosition));
		}

		private void SynchronizeContentContainerStyle()
		{
			if (_contentUseManualLayout)
			{
				contentContainer.style.position = StyleKeyword.Null;

				float scaleFactorPercentage = (ContentScaleFactor - 1.0f) * 100.0f;
				contentContainer.style.right = Length.Percent(_scaleContentToWidth ? -scaleFactorPercentage : 100.0f);
				contentContainer.style.bottom = Length.Percent(_scaleContentToHeight ? -scaleFactorPercentage : 100.0f);

				contentContainer.style.marginRight = -_contentSizeOffset.x;
				contentContainer.style.marginBottom = -_contentSizeOffset.y;
			}
			else
			{
				contentContainer.style.position = Position.Relative;

				contentContainer.style.right = StyleKeyword.Initial;
				contentContainer.style.bottom = StyleKeyword.Initial;

				contentContainer.style.marginRight = StyleKeyword.Initial;
				contentContainer.style.marginBottom = StyleKeyword.Initial;
			}
		}

		private ScrollView control;
		private VisualElement contentViewport;
		private VisualElement contentContainer;

		private VisualElement horizontalTracker;
		private VisualElement horizontalDragger;
		private VisualElement verticalTracker;
		private VisualElement verticalDragger;

		private bool wantsToScrollToBottom;
	}
}
