////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public abstract class GlazierElementBase : ISleekElement
	{
		protected bool _isVisible = true;
		public virtual bool IsVisible
		{
			get
			{
				ValidateNotDestroyed();
				return _isVisible;
			}

			set
			{
				ValidateNotDestroyed();
				_isVisible = value;
			}
		}

		public abstract ISleekElement Parent { get; }

		private float fromPositionOffset_X;
		private float fromPositionOffset_Y;
		private float toPositionOffset_X;
		private float toPositionOffset_Y;

		private float fromPositionScale_X;
		private float fromPositionScale_Y;
		private float toPositionScale_X;
		private float toPositionScale_Y;

		private float fromSizeOffset_X;
		private float fromSizeOffset_Y;
		private float toSizeOffset_X;
		private float toSizeOffset_Y;

		private float fromSizeScale_X;
		private float fromSizeScale_Y;
		private float toSizeScale_X;
		private float toSizeScale_Y;

		private ESleekLerp positionOffsetLerpMethod;
		private float positionOffsetLerpTime;
		private float positionOffsetLerpValue;
		private bool isAnimatingPositionOffset;

		private ESleekLerp positionScaleLerpMethod;
		private float positionScaleLerpTime;
		private float positionScaleLerpValue;
		private bool isAnimatingPositionScale;

		private ESleekLerp sizeOffsetLerpMethod;
		private float sizeOffsetLerpTime;
		private float sizeOffsetLerpValue;
		private bool isAnimatingSizeOffset;

		private ESleekLerp sizeScaleLerpMethod;
		private float sizeScaleLerpTime;
		private float sizeScaleLerpValue;
		private bool isAnimatingSizeScale;

		public bool isTransformDirty;

		public ISleekLabel SideLabel
		{
			get;
			private set;
		}

		private float _positionOffset_X;
		public float PositionOffset_X
		{
			get
			{
				ValidateNotDestroyed();
				return _positionOffset_X;
			}

			set
			{
				ValidateNotDestroyed();
				_positionOffset_X = value;
				isTransformDirty = true;
			}
		}

		private float _positionOffset_Y;
		public float PositionOffset_Y
		{
			get
			{
				ValidateNotDestroyed();
				return _positionOffset_Y;
			}

			set
			{
				ValidateNotDestroyed();
				_positionOffset_Y = value;
				isTransformDirty = true;
			}
		}

		private float _positionScale_X;
		public float PositionScale_X
		{
			get
			{
				ValidateNotDestroyed();
				return _positionScale_X;
			}

			set
			{
				ValidateNotDestroyed();
				_positionScale_X = value;
				isTransformDirty = true;
			}
		}

		private float _positionScale_Y;
		public float PositionScale_Y
		{
			get
			{
				ValidateNotDestroyed();
				return _positionScale_Y;
			}

			set
			{
				ValidateNotDestroyed();
				_positionScale_Y = value;
				isTransformDirty = true;
			}
		}

		private float _sizeOffset_X;
		public float SizeOffset_X
		{
			get
			{
				ValidateNotDestroyed();
				return _sizeOffset_X;
			}

			set
			{
				ValidateNotDestroyed();
				_sizeOffset_X = value;
				isTransformDirty = true;
			}
		}

		private float _sizeOffset_Y;
		public float SizeOffset_Y
		{
			get
			{
				ValidateNotDestroyed();
				return _sizeOffset_Y;
			}

			set
			{
				ValidateNotDestroyed();
				_sizeOffset_Y = value;
				isTransformDirty = true;
			}
		}

		private float _sizeScale_X;
		public float SizeScale_X
		{
			get
			{
				ValidateNotDestroyed();
				return _sizeScale_X;
			}

			set
			{
				ValidateNotDestroyed();
				_sizeScale_X = value;
				isTransformDirty = true;
			}
		}

		private float _sizeScale_Y;
		public float SizeScale_Y
		{
			get
			{
				ValidateNotDestroyed();
				return _sizeScale_Y;
			}

			set
			{
				ValidateNotDestroyed();
				_sizeScale_Y = value;
				isTransformDirty = true;
			}
		}

		public ISleekElement AttachmentRoot => this;

		public bool IsAnimatingTransform =>
				isAnimatingPositionOffset | isAnimatingPositionScale | isAnimatingSizeOffset | isAnimatingSizeScale;

		protected bool _useManualLayout = true;
		public virtual bool UseManualLayout
		{
			get
			{
				ValidateNotDestroyed();
				return _useManualLayout;
			}

			set
			{
				ValidateNotDestroyed();
				_useManualLayout = value;
			}
		}

		protected bool _useWidthLayoutOverride = false;
		public virtual bool UseWidthLayoutOverride
		{
			get
			{
				ValidateNotDestroyed();
				return _useWidthLayoutOverride;
			}

			set
			{
				ValidateNotDestroyed();
				_useWidthLayoutOverride = value;
			}
		}

		protected bool _useHeightLayoutOverride = false;
		public virtual bool UseHeightLayoutOverride
		{
			get
			{
				ValidateNotDestroyed();
				return _useHeightLayoutOverride;
			}

			set
			{
				ValidateNotDestroyed();
				_useHeightLayoutOverride = value;
			}
		}

		protected ESleekChildLayout _useChildAutoLayout = ESleekChildLayout.None;
		public virtual ESleekChildLayout UseChildAutoLayout
		{
			get
			{
				ValidateNotDestroyed();
				return _useChildAutoLayout;
			}

			set
			{
				ValidateNotDestroyed();
				_useChildAutoLayout = value;
			}
		}

		protected ESleekChildPerpendicularAlignment _childPerpendicularAlignment;
		public virtual ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
		{
			get
			{
				ValidateNotDestroyed();
				return _childPerpendicularAlignment;
			}

			set
			{
				ValidateNotDestroyed();
				_childPerpendicularAlignment = value;
			}
		}

		protected bool _expandChildren = false;
		public virtual bool ExpandChildren
		{
			get
			{
				ValidateNotDestroyed();
				return _expandChildren;
			}

			set
			{
				ValidateNotDestroyed();
				_expandChildren = value;
			}
		}

		protected bool _ignoreLayout = false;
		public virtual bool IgnoreLayout
		{
			get
			{
				ValidateNotDestroyed();
				return _ignoreLayout;
			}

			set
			{
				ValidateNotDestroyed();
				_ignoreLayout = value;
			}
		}

		protected float _childAutoLayoutPadding = 0.0f;
		public virtual float ChildAutoLayoutPadding
		{
			get
			{
				ValidateNotDestroyed();
				return _childAutoLayoutPadding;
			}

			set
			{
				ValidateNotDestroyed();
				_childAutoLayoutPadding = value;
			}
		}

		public abstract void InternalDestroy();

		public void AnimatePositionOffset(float newPositionOffset_X, float newPositionOffset_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			isAnimatingPositionOffset = true;
			positionOffsetLerpMethod = lerp;
			positionOffsetLerpTime = time;
			positionOffsetLerpValue = 0.0f;

			fromPositionOffset_X = PositionOffset_X;
			fromPositionOffset_Y = PositionOffset_Y;

			toPositionOffset_X = newPositionOffset_X;
			toPositionOffset_Y = newPositionOffset_Y;
		}

		public void AnimatePositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			isAnimatingPositionScale = true;
			positionScaleLerpMethod = lerp;
			positionScaleLerpTime = time;
			positionScaleLerpValue = 0.0f;

			fromPositionScale_X = PositionScale_X;
			fromPositionScale_Y = PositionScale_Y;

			toPositionScale_X = newPositionScale_X;
			toPositionScale_Y = newPositionScale_Y;
		}

		public void AnimateSizeOffset(float newSizeOffset_X, float newSizeOffset_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			isAnimatingSizeOffset = true;
			sizeOffsetLerpMethod = lerp;
			sizeOffsetLerpTime = time;
			sizeOffsetLerpValue = 0.0f;

			fromSizeOffset_X = SizeOffset_X;
			fromSizeOffset_Y = SizeOffset_Y;

			toSizeOffset_X = newSizeOffset_X;
			toSizeOffset_Y = newSizeOffset_Y;
		}

		public void AnimateSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			isAnimatingSizeScale = true;
			sizeScaleLerpMethod = lerp;
			sizeScaleLerpTime = time;
			sizeScaleLerpValue = 0.0f;

			fromSizeScale_X = SizeScale_X;
			fromSizeScale_Y = SizeScale_Y;

			toSizeScale_X = newSizeScale_X;
			toSizeScale_Y = newSizeScale_Y;
		}

		public abstract void AddChild(ISleekElement child);

		public void AddLabel(string text, ESleekSide side)
		{
			AddLabel(text, Color.white, side);
		}

		public void AddLabel(string text, Color color, ESleekSide side)
		{
			ValidateNotDestroyed();
			SideLabel = Glazier.Get().CreateLabel();

			if (side == ESleekSide.LEFT)
			{
				SideLabel.PositionOffset_X = -205;
				SideLabel.TextAlignment = TextAnchor.MiddleRight;
			}
			else if (side == ESleekSide.RIGHT)
			{
				SideLabel.PositionOffset_X = 5;
				SideLabel.PositionScale_X = 1;
				SideLabel.TextAlignment = TextAnchor.MiddleLeft;
			}

			SideLabel.PositionOffset_Y = -30;
			SideLabel.PositionScale_Y = 0.5f;
			SideLabel.SizeOffset_X = 200;
			SideLabel.SizeOffset_Y = 60;
			if (color != Color.white)
			{
				SideLabel.TextColor = color;
			}
			SideLabel.Text = text;
			SideLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

			AddChild(SideLabel);
		}

		public void UpdateLabel(string text)
		{
			ValidateNotDestroyed();
			SideLabel.Text = text;
		}

		public abstract int FindIndexOfChild(ISleekElement sleek);
		public abstract ISleekElement GetChildAtIndex(int index);
		public abstract int GetChildCount();
		public abstract void RemoveChild(ISleekElement child);
		public abstract void RemoveAllChildren();

		public virtual void Update()
		{
			UnityEngine.Profiling.Profiler.BeginSample("Sleek Update");

			if (IsAnimatingTransform)
			{
				UpdateAnimation();
			}

			if (isTransformDirty)
			{
				UpdateDirtyTransform();
			}

			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("UpdateChildren()");
			UpdateChildren();
			UnityEngine.Profiling.Profiler.EndSample();
		}
		protected abstract void UpdateChildren();

		private float InterpValue(float value, ESleekLerp method, float time, float deltaTime)
		{
			switch (method)
			{
				case ESleekLerp.LINEAR:
					// Time is the duration of the transition.
					return value + (deltaTime / time);

				case ESleekLerp.EXPONENTIAL:
					// Time is the speed of the transition.
					return value + ((1.0f - value) * time * deltaTime);

				default:
					return value;
			}
		}

		private void UpdateAnimation()
		{
			float deltaTime = Time.unscaledDeltaTime; // Game can be paused in singleplayer.
			const float completionThreshold = 0.999f;

			if (isAnimatingPositionOffset)
			{
				if (positionOffsetLerpValue >= completionThreshold)
				{
					isAnimatingPositionOffset = false;
					PositionOffset_X = toPositionOffset_X;
					PositionOffset_Y = toPositionOffset_Y;
				}
				else
				{
					PositionOffset_X = Mathf.Lerp(fromPositionOffset_X, toPositionOffset_X, positionOffsetLerpValue);
					PositionOffset_Y = Mathf.Lerp(fromPositionOffset_Y, toPositionOffset_Y, positionOffsetLerpValue);
				}

				// Intentionally AFTER the first frame of the animation. This hides uGUI flicking bugs by placing the
				// first frame of SleekFullscreenBox animations off-screen when they become visible.
				positionOffsetLerpValue = InterpValue(positionOffsetLerpValue, positionOffsetLerpMethod, positionOffsetLerpTime, deltaTime);
			}

			if (isAnimatingPositionScale)
			{
				if (positionScaleLerpValue >= completionThreshold)
				{
					isAnimatingPositionScale = false;
					PositionScale_X = toPositionScale_X;
					PositionScale_Y = toPositionScale_Y;
				}
				else
				{
					PositionScale_X = Mathf.Lerp(fromPositionScale_X, toPositionScale_X, positionScaleLerpValue);
					PositionScale_Y = Mathf.Lerp(fromPositionScale_Y, toPositionScale_Y, positionScaleLerpValue);
				}

				// Intentionally AFTER the first frame of the animation. This hides uGUI flicking bugs by placing the
				// first frame of SleekFullscreenBox animations off-screen when they become visible.
				positionScaleLerpValue = InterpValue(positionScaleLerpValue, positionScaleLerpMethod, positionScaleLerpTime, deltaTime);
			}

			if (isAnimatingSizeOffset)
			{
				if (sizeOffsetLerpValue >= completionThreshold)
				{
					isAnimatingSizeOffset = false;
					SizeOffset_X = toSizeOffset_X;
					SizeOffset_Y = toSizeOffset_Y;
				}
				else
				{
					SizeOffset_X = Mathf.Lerp(fromSizeOffset_X, toSizeOffset_X, sizeOffsetLerpValue);
					SizeOffset_Y = Mathf.Lerp(fromSizeOffset_Y, toSizeOffset_Y, sizeOffsetLerpValue);
				}

				// Intentionally AFTER the first frame of the animation. This hides uGUI flicking bugs by placing the
				// first frame of SleekFullscreenBox animations off-screen when they become visible.
				sizeOffsetLerpValue = InterpValue(sizeOffsetLerpValue, sizeOffsetLerpMethod, sizeOffsetLerpTime, deltaTime);
			}

			if (isAnimatingSizeScale)
			{
				if (sizeScaleLerpValue >= completionThreshold)
				{
					isAnimatingSizeScale = false;
					SizeScale_X = toSizeScale_X;
					SizeScale_Y = toSizeScale_Y;
				}
				else
				{
					SizeScale_X = Mathf.Lerp(fromSizeScale_X, toSizeScale_X, sizeScaleLerpValue);
					SizeScale_Y = Mathf.Lerp(fromSizeScale_Y, toSizeScale_Y, sizeScaleLerpValue);
				}

				// Intentionally AFTER the first frame of the animation. This hides uGUI flicking bugs by placing the
				// first frame of SleekFullscreenBox animations off-screen when they become visible.
				sizeScaleLerpValue = InterpValue(sizeScaleLerpValue, sizeScaleLerpMethod, sizeScaleLerpTime, deltaTime);
			}
		}

		public abstract Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition);
		public abstract Vector2 GetNormalizedCursorPosition();
		public abstract Vector2 GetAbsoluteSize();
		public abstract void SetAsFirstSibling();
		public abstract void ForceLayoutUpdate();
		protected abstract void UpdateDirtyTransform();

		[System.Diagnostics.Conditional("VALIDATE_GLAZIER_USE_AFTER_DESTROY")]
		public void ValidateNotDestroyed()
		{
#if VALIDATE_GLAZIER_USE_AFTER_DESTROY
			if (wasDestroyed)
			{
				throw new System.Exception("Glazier element was used after being destroyed");
			}
#endif
		}

		public GlazierElementBase()
		{
			isAnimatingPositionOffset = false;
			isAnimatingPositionScale = false;
			isAnimatingSizeOffset = false;
			isAnimatingSizeScale = false;

			isTransformDirty = true;
			SideLabel = null;
			_positionOffset_X = 0;
			_positionOffset_Y = 0;
			_positionScale_X = 0.0f;
			_positionScale_Y = 0.0f;
			_sizeOffset_X = 0;
			_sizeOffset_Y = 0;
			_sizeScale_X = 0.0f;
			_sizeScale_Y = 0.0f;
		}

#if VALIDATE_GLAZIER_USE_AFTER_DESTROY
		public bool wasDestroyed = false;
#endif
	}
}
