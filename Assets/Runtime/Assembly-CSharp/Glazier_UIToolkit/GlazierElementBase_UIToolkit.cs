////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	/// <summary>
	/// Base class for UIToolkit implementations of primitive building block widgets.
	/// </summary>
	internal abstract class GlazierElementBase_UIToolkit : GlazierElementBase
	{
		public Glazier_UIToolkit glazier
		{
			get;
			private set;
		}

		public override bool IsVisible
		{
			set
			{
				ValidateNotDestroyed();
				if (_isVisible != value)
				{
					_isVisible = value;
					// 2023-09-27: I played with removing from hierarchy, but that messes with draw order.
					visualElement.visible = _isVisible;
					visualElement.style.visibility = _isVisible ? Visibility.Visible : Visibility.Hidden;
					visualElement.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
				}
			}
		}

		public override bool UseManualLayout
		{
			set
			{
				ValidateNotDestroyed();
				isTransformDirty |= (_useManualLayout != value);
				_useManualLayout = value;
			}
		}

		public override bool UseWidthLayoutOverride
		{
			set
			{
				ValidateNotDestroyed();
				isTransformDirty |= (_useWidthLayoutOverride != value);
				_useWidthLayoutOverride = value;
			}
		}

		public override bool UseHeightLayoutOverride
		{
			set
			{
				ValidateNotDestroyed();
				isTransformDirty |= (_useHeightLayoutOverride != value);
				_useHeightLayoutOverride = value;
			}
		}

		public override ESleekChildLayout UseChildAutoLayout
		{
			set
			{
				base.UseChildAutoLayout = value;
				// UITK always has layout enabled with a default of Column (vertical).
				visualElement.style.flexDirection = value == ESleekChildLayout.Horizontal ? FlexDirection.Row : StyleKeyword.Null;
				ApplyChildPerpendicularAlignment();
			}
		}

		public override ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
		{
			set
			{
				base.ChildPerpendicularAlignment = value;
				ApplyChildPerpendicularAlignment();
			}
		}

		public override bool ExpandChildren
		{
			set
			{
				ValidateNotDestroyed();
				bool changed = _expandChildren != value;
				_expandChildren = value;
				if (changed)
				{
					StyleFloat flexGrow = _expandChildren ? 1.0f : StyleKeyword.Null;
					foreach (GlazierElementBase_UIToolkit child in _children)
					{
						child.visualElement.style.flexGrow = flexGrow;
					}
				}
			}
		}

		public override bool IgnoreLayout
		{
			set
			{
				ValidateNotDestroyed();
				isTransformDirty |= (_ignoreLayout != value);
				_ignoreLayout = value;
			}
		}

		public GlazierElementBase_UIToolkit _parent;
		public override ISleekElement Parent
		{
			get { return _parent; }
		}

		/// <summary>
		/// Set by child.
		/// </summary>
		public VisualElement visualElement;

		public GlazierElementBase_UIToolkit(Glazier_UIToolkit glazier)
		{
			this.glazier = glazier;
		}

		public override int FindIndexOfChild(ISleekElement child)
		{
			ValidateNotDestroyed();
			return _children.IndexOf((GlazierElementBase_UIToolkit) child.AttachmentRoot);
		}

		public override ISleekElement GetChildAtIndex(int index)
		{
			ValidateNotDestroyed();
			return _children[index];
		}

		public override int GetChildCount()
		{
			ValidateNotDestroyed();
			return _children.Count;
		}

		public override void RemoveChild(ISleekElement child)
		{
			if (child == null)
				throw new System.ArgumentNullException(nameof(child));

			ValidateNotDestroyed();
			if (child.AttachmentRoot is GlazierElementBase_UIToolkit typedChild)
			{
				typedChild.ValidateNotDestroyed();
				typedChild._parent = null;
				typedChild.InternalDestroy();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				bool wasRemoved =
#endif

				// Order of children is important for depth and UIs which rely on index.
				_children.Remove(typedChild);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (!wasRemoved)
				{
					UnturnedLog.warn("Child was not in children list");
				}
#endif
			}
			else
			{
				UnturnedLog.warn("{0} cannot remove non-UIToolkit element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
			}
		}

		public override void RemoveAllChildren()
		{
			ValidateNotDestroyed();
			foreach (GlazierElementBase_UIToolkit child in _children)
			{
				child.ValidateNotDestroyed();
				child._parent = null;
				child.InternalDestroy();
			}

			_children.Clear();
		}

		protected override void UpdateChildren()
		{
			foreach (GlazierElementBase_UIToolkit child in _children)
			{
				if (child.IsVisible)
				{
					child.Update();
				}
			}
		}

		public override void AddChild(ISleekElement child)
		{
			ValidateNotDestroyed();
			if (child.AttachmentRoot is GlazierElementBase_UIToolkit typedChild)
			{
				typedChild.ValidateNotDestroyed();

				if (typedChild._parent == this)
					return;

				if (typedChild._parent != null)
				{
					// Order of children is important for depth and UIs which rely on index.
					typedChild._parent._children.Remove(typedChild);
				}

				_children.Add(typedChild);
				typedChild._parent = this;
				typedChild.visualElement.style.flexGrow = _expandChildren ? 1.0f : StyleKeyword.Null;
				visualElement.Add(typedChild.visualElement);
				typedChild.UpdateDirtyTransform();
			}
			else
			{
				UnturnedLog.warn("{0} cannot add non-UIToolkit element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
			}
		}

		public override Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
		{
			ValidateNotDestroyed();

			if (visualElement.panel == null)
				return Vector2.zero;

			Rect elementRect = visualElement.worldBound;
			if (Mathf.Approximately(elementRect.width, 0.0f) || Mathf.Approximately(elementRect.height, 0.0f))
				return Vector2.zero;

			Rect rootRect = visualElement.panel.visualTree.worldBound;
			return new Vector2(((viewportPosition.x * rootRect.width) - elementRect.xMin) / elementRect.width, (((1.0f - viewportPosition.y) * rootRect.height) - elementRect.yMin) / elementRect.height);
		}

		public override Vector2 GetNormalizedCursorPosition()
		{
			ValidateNotDestroyed();

			if (visualElement.panel == null)
				return Vector2.zero;

			Rect rootRect = visualElement.panel.visualTree.worldBound;
			if (Mathf.Approximately(rootRect.width, 0.0f) || Mathf.Approximately(rootRect.height, 0.0f))
				return Vector2.zero;

			Vector2 viewportPosition = InputEx.NormalizedMousePosition;
			Rect elementRect = visualElement.worldBound;
			return new Vector2((viewportPosition.x - (elementRect.xMin / rootRect.width)) / (elementRect.width / rootRect.width),
				(1.0f - viewportPosition.y - (elementRect.yMin / rootRect.height)) / (elementRect.height / rootRect.height));
		}

		public override Vector2 GetAbsoluteSize()
		{
			ValidateNotDestroyed();

			if (visualElement.panel == null)
				return Vector2.zero;
			
			Rect rootRect = visualElement.panel.visualTree.worldBound;
			if (Mathf.Approximately(rootRect.width, 0.0f) || Mathf.Approximately(rootRect.height, 0.0f))
				return Vector2.zero;

			Rect elementRect = visualElement.worldBound;
			return new Vector2(elementRect.width / rootRect.width * Screen.width, elementRect.height / rootRect.height * Screen.height);
		}

		public override void SetAsFirstSibling()
		{
			ValidateNotDestroyed();
			if (_parent != null)
			{
				visualElement.SendToBack();

				bool wasRemoved = _parent._children.Remove(this);
				if (wasRemoved)
				{
					_parent._children.Insert(0, this);
				}
			}
		}

		public override void ForceLayoutUpdate()
		{
			ValidateNotDestroyed();
			isTransformDirty = true;
		}

		public override void InternalDestroy()
		{
			RemoveAllChildren();

			visualElement.RemoveFromHierarchy();

			glazier.RemoveDestroyedElement(this);

#if VALIDATE_GLAZIER_USE_AFTER_DESTROY
			wasDestroyed = true;
#endif
		}

		/// <summary>
		/// Synchronize control colors with background/text/image etc. colors.
		/// Called when custom UI colors are changed, and after constructor.
		/// </summary>
		internal virtual void SynchronizeColors()
		{

		}

		internal virtual bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
		{
			tooltipText = null;
			tooltipColor = default;
			return false;
		}
		
		protected override void UpdateDirtyTransform()
		{
			isTransformDirty = false;

			visualElement.style.position = _useManualLayout || _ignoreLayout ? Position.Absolute : Position.Relative;

			if (_useManualLayout)
			{
				// Percentages aren't normalized 0-1, rather 100 is 100%.
				// These offsets are inwards from the bounds. For example, a positive right value
				// pushes left, and a negative right value expands right.
				visualElement.style.left = Length.Percent(PositionScale_X * 100.0f);
				visualElement.style.top = Length.Percent(PositionScale_Y * 100.0f);
				visualElement.style.right = Length.Percent((1.0f - SizeScale_X - PositionScale_X) * 100.0f);
				visualElement.style.bottom = Length.Percent((1.0f - SizeScale_Y - PositionScale_Y) * 100.0f);

				visualElement.style.marginLeft = PositionOffset_X;
				visualElement.style.marginTop = PositionOffset_Y;
				visualElement.style.marginRight = -PositionOffset_X - SizeOffset_X;
				visualElement.style.marginBottom = -PositionOffset_Y - SizeOffset_Y;

				visualElement.style.width = StyleKeyword.Null;
				visualElement.style.height = StyleKeyword.Null;
			}
			else
			{
				visualElement.style.left = StyleKeyword.Null;
				visualElement.style.right = StyleKeyword.Null;
				visualElement.style.top = StyleKeyword.Null;
				visualElement.style.bottom = StyleKeyword.Null;

				visualElement.style.marginLeft = StyleKeyword.Null;
				visualElement.style.marginTop = StyleKeyword.Null;
				visualElement.style.marginRight = StyleKeyword.Null;
				visualElement.style.marginBottom = StyleKeyword.Null;

				visualElement.style.width = _useWidthLayoutOverride ? SizeOffset_X : StyleKeyword.Null;
				visualElement.style.height = _useHeightLayoutOverride ? SizeOffset_Y : StyleKeyword.Null;
			}
		}

		private void ApplyChildPerpendicularAlignment()
		{
			if (_useChildAutoLayout == ESleekChildLayout.Horizontal)
			{
				switch (_childPerpendicularAlignment)
				{
					default:
					case ESleekChildPerpendicularAlignment.Center:
						visualElement.style.alignItems = StyleKeyword.Null;
						break;

					case ESleekChildPerpendicularAlignment.Top:
						visualElement.style.alignItems = Align.FlexStart;
						break;

					case ESleekChildPerpendicularAlignment.Bottom:
						visualElement.style.alignItems = Align.FlexEnd;
						break;
				}
			}
		}

		private List<GlazierElementBase_UIToolkit> _children = new List<GlazierElementBase_UIToolkit>();
	}
}
