////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Parent for widgets composed of primitive widgets.
	/// </summary>
	public class SleekWrapper : ISleekElement
	{
		public bool IsVisible
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.IsVisible;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.IsVisible = value;
			}
		}

		public ISleekElement Parent
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.Parent;
			}
		}

		public ISleekLabel SideLabel
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.SideLabel;
			}
		}

		public float PositionOffset_X
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.PositionOffset_X;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.PositionOffset_X = value;
			}
		}

		public float PositionOffset_Y
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.PositionOffset_Y;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.PositionOffset_Y = value;
			}
		}

		public float PositionScale_X
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.PositionScale_X;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.PositionScale_X = value;
			}
		}

		public float PositionScale_Y
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.PositionScale_Y;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.PositionScale_Y = value;
			}
		}

		public float SizeOffset_X
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.SizeOffset_X;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.SizeOffset_X = value;
			}
		}

		public float SizeOffset_Y
		{
			get => implementation.SizeOffset_Y;
			set => implementation.SizeOffset_Y = value;
		}

		public float SizeScale_X
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.SizeScale_X;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.SizeScale_X = value;
			}
		}

		public float SizeScale_Y
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.SizeScale_Y;
			}
			set
			{
				ValidateNotDestroyed();
				implementation.SizeScale_Y = value;
			}
		}

		public ISleekElement AttachmentRoot
		{
			get
			{
				ValidateNotDestroyed();
				return implementation;
			}
		}

		public bool IsAnimatingTransform
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.IsAnimatingTransform;
			}
		}

		public virtual bool UseManualLayout
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.UseManualLayout;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.UseManualLayout = value;
			}
		}

		public bool UseWidthLayoutOverride
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.UseWidthLayoutOverride;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.UseWidthLayoutOverride = value;
			}
		}

		public bool UseHeightLayoutOverride
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.UseHeightLayoutOverride;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.UseHeightLayoutOverride = value;
			}
		}

		public ESleekChildLayout UseChildAutoLayout
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.UseChildAutoLayout;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.UseChildAutoLayout = value;
			}
		}

		public ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.ChildPerpendicularAlignment;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.ChildPerpendicularAlignment = value;
			}
		}

		public bool ExpandChildren
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.ExpandChildren;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.ExpandChildren = value;
			}
		}

		public bool IgnoreLayout
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.IgnoreLayout;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.IgnoreLayout = value;
			}
		}

		public float ChildAutoLayoutPadding
		{
			get
			{
				ValidateNotDestroyed();
				return implementation.ChildAutoLayoutPadding;
			}

			set
			{
				ValidateNotDestroyed();
				implementation.ChildAutoLayoutPadding = value;
			}
		}

		public void InternalDestroy()
		{
			implementation.InternalDestroy();
		}

		public void AnimatePositionOffset(float newPositionOffset_X, float newPositionOffset_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			implementation.AnimatePositionOffset(newPositionOffset_X, newPositionOffset_Y, lerp, time);
		}

		public void AnimatePositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			implementation.AnimatePositionScale(newPositionScale_X, newPositionScale_Y, lerp, time);
		}

		public void AnimateSizeOffset(float newSizeOffset_X, float newSizeOffset_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			implementation.AnimateSizeOffset(newSizeOffset_X, newSizeOffset_Y, lerp, time);
		}
		public void AnimateSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time)
		{
			ValidateNotDestroyed();
			implementation.AnimateSizeScale(newSizeScale_X, newSizeScale_Y, lerp, time);
		}

		public void AddChild(ISleekElement sleek)
		{
			ValidateNotDestroyed();
			implementation.AddChild(sleek);
		}

		public void AddLabel(string text, ESleekSide side)
		{
			ValidateNotDestroyed();
			implementation.AddLabel(text, side);
		}
		public void AddLabel(string text, Color color, ESleekSide side)
		{
			ValidateNotDestroyed();
			implementation.AddLabel(text, color, side);
		}

		public void UpdateLabel(string text)
		{
			ValidateNotDestroyed();
			implementation.UpdateLabel(text);
		}

		public int FindIndexOfChild(ISleekElement sleek)
		{
			ValidateNotDestroyed();
			return implementation.FindIndexOfChild(sleek);
		}

		public ISleekElement GetChildAtIndex(int index)
		{
			ValidateNotDestroyed();
			return implementation.GetChildAtIndex(index);
		}

		public int GetChildCount()
		{
			ValidateNotDestroyed();
			return implementation.GetChildCount();
		}

		public void Update()
		{
			implementation.Update();
		}

		public void RemoveChild(ISleekElement sleek)
		{
			ValidateNotDestroyed();
			implementation.RemoveChild(sleek);
		}

		public void RemoveAllChildren()
		{
			ValidateNotDestroyed();
			implementation.RemoveAllChildren();
		}

		public Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
		{
			ValidateNotDestroyed();
			return implementation.ViewportToNormalizedPosition(viewportPosition);
		}

		public Vector2 GetNormalizedCursorPosition()
		{
			ValidateNotDestroyed();
			return implementation.GetNormalizedCursorPosition();
		}

		public Vector2 GetAbsoluteSize()
		{
			ValidateNotDestroyed();
			return implementation.GetAbsoluteSize();
		}

		public void SetAsFirstSibling()
		{
			ValidateNotDestroyed();
			implementation.SetAsFirstSibling();
		}

		public void ForceLayoutUpdate()
		{
			ValidateNotDestroyed();
			implementation.ForceLayoutUpdate();
		}

		/// <summary>
		/// Potentially useful, for example to find index of this element.
		/// </summary>
		public ISleekProxyImplementation GetProxyImplementation() => implementation;

		/// <summary>
		/// Called by proxy implementation before Update.
		/// </summary>
		public virtual void OnUpdate() { }

		/// <summary>
		/// Called by proxy implementation before Destroy.
		/// </summary>
		public virtual void OnDestroy() { }

#if VALIDATE_SLEEK_PROXY_USE_AFTER_DESTROY
		public bool wasDestroyed = false;
#endif
		/// <summary>
		/// Unlike glazier implementation, this class is not pooled and should not be used after destroy.
		/// </summary>
		[System.Diagnostics.Conditional("VALIDATE_SLEEK_PROXY_USE_AFTER_DESTROY")]
		public void ValidateNotDestroyed()
		{
#if VALIDATE_SLEEK_PROXY_USE_AFTER_DESTROY
			if (wasDestroyed)
			{
				throw new System.Exception("SleekProxy was used after being destroyed");
			}
#endif
		}

		public SleekWrapper()
		{
			implementation = Glazier.Get().CreateProxyImplementation(this);
		}

		private ISleekProxyImplementation implementation;
	}
}
