////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierScrollView_IMGUI : GlazierElementBase_IMGUI, ISleekScrollView
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
				isTransformDirty = true;
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
				isTransformDirty = true;
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
				isTransformDirty = true;
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
				isTransformDirty = true;
			}
		}

		public ESleekScrollbarVisibility VerticalScrollbarVisibility
		{
			get;
			set;
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
				isTransformDirty = true;
			}
		}

		private Vector2 state;

		public Vector2 NormalizedStateCenter
		{
			get
			{
				ValidateNotDestroyed();
				if (isTransformDirty)
				{
					UpdateDirtyTransform();
				}
				return new Vector2((state.x + (drawRect.width * 0.5f)) / contentRect.width,
					(state.y + (drawRect.height * 0.5f)) / contentRect.height);
			}
			set
			{
				ValidateNotDestroyed();
				if (isTransformDirty)
				{
					UpdateDirtyTransform();
				}
				state = new Vector2((value.x * contentRect.width) - (drawRect.width * 0.5f),
					(value.y * contentRect.height) - (drawRect.height * 0.5f));
			}
		}

		public bool HandleScrollWheel
		{
			get;
			set;
		} = true;

		public SleekColor BackgroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultScrollViewBackgroundColor;

		public SleekColor ForegroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultScrollViewForegroundColor;

		public event System.Action<Vector2> OnNormalizedValueChanged;

		public float NormalizedVerticalPosition
		{
			get
			{
				ValidateNotDestroyed();
				if (isTransformDirty)
				{
					UpdateDirtyTransform();
				}
				return state.y / (contentRect.height - drawRect.height);
			}
		}

		public float NormalizedViewportHeight
		{
			get
			{
				ValidateNotDestroyed();
				if (isTransformDirty)
				{
					UpdateDirtyTransform();
				}
				return drawRect.height / contentRect.height;
			}
		}

		public bool ContentUseManualLayout
		{
			get;
			set;
		} = true;

		public bool AlignContentToBottom
		{
			get;
			set;
		} = false;

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
			state = new Vector2(state.x, state.y);
		}

		public void ScrollToBottom()
		{
			ValidateNotDestroyed();
			state = new Vector2(state.x, contentRect.height);
		}

		public override void OnGUI()
		{
			GUI.backgroundColor = BackgroundColor;

			// scrollPosition is measured in pixels from the upper-left.
			Vector2 newScrollPosition = GUI.BeginScrollView(drawRect, state, viewRect);
			if (state != newScrollPosition)
			{
				state = newScrollPosition;

				if (OnNormalizedValueChanged != null)
				{
					Vector2 normalizedValue = new Vector2(state.x / (contentRect.width - drawRect.width),
						state.y / (contentRect.height - drawRect.height));

					OnNormalizedValueChanged.Invoke(normalizedValue);
				}
			}

			ChildrenOnGUI();

			GUI.EndScrollView(HandleScrollWheel);
		}

		protected override void TransformChildDrawPositionIntoParentSpace(ref Vector2 position)
		{
			position.x += drawRect.x;
			position.x -= state.x;
			position.y += drawRect.y;
			position.y -= state.y;
		}

		protected override Rect GetLayoutRect()
		{
			return contentRect;
		}

		protected override void UpdateDirtyTransform()
		{
			base.UpdateDirtyTransform();

			float layoutScale = GraphicsSettings.userInterfaceScale;
			contentRect.width = ContentSizeOffset.x * layoutScale;
			contentRect.height = ContentSizeOffset.y * layoutScale;

			if (ScaleContentToWidth)
			{
				contentRect.width += drawRect.width * ContentScaleFactor;
			}

			if (ScaleContentToHeight)
			{
				contentRect.height += drawRect.height * ContentScaleFactor;
			}

			bool verticalScrollbarVisible = contentRect.height >= drawRect.height;
			if (verticalScrollbarVisible && ReduceWidthWhenScrollbarVisible && ScaleContentToWidth)
			{
				contentRect.width -= 30.0f;
			}

			bool horizontalScrollbarVisible = contentRect.width >= drawRect.width;
			if (horizontalScrollbarVisible && ScaleContentToHeight)
			{
				contentRect.height -= 30.0f;
			}

			viewRect = contentRect;

			if (verticalScrollbarVisible && !ReduceWidthWhenScrollbarVisible && ScaleContentToWidth)
			{
				// Special case to keep server list headers aligned.
				viewRect.width -= 30.0f;
			}
		}

		private Rect contentRect;
		private Rect viewRect;
	}
}
