////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	/// <summary>
	/// Base class for uGUI implementations of primitive building block widgets.
	/// </summary>
	internal abstract class GlazierElementBase_uGUI : GlazierElementBase
	{
		public Glazier_uGUI glazier
		{
			get;
			private set;
		}

		public override bool IsVisible
		{
			get => base.IsVisible;

			set
			{
				ValidateNotDestroyed();
				if (_isVisible != value)
				{
					_isVisible = value;
					gameObject.SetActive(value);
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
				bool changed = _useWidthLayoutOverride != value;
				isTransformDirty |= changed;
				_useWidthLayoutOverride = value;
				if (changed)
				{
					if (ShouldHaveLayoutElementComponent)
					{
						transform.GetOrAddComponent<LayoutElement>();
					}
					else
					{
						transform.DestroyComponentIfExists<LayoutElement>();
					}
				}
			}
		}

		public override bool UseHeightLayoutOverride
		{
			set
			{
				ValidateNotDestroyed();
				bool changed = _useHeightLayoutOverride != value;
				isTransformDirty |= changed;
				_useHeightLayoutOverride = value;
				if (changed)
				{
					if (ShouldHaveLayoutElementComponent)
					{
						transform.GetOrAddComponent<LayoutElement>();
					}
					else
					{
						transform.DestroyComponentIfExists<LayoutElement>();
					}
				}
			}
		}

		public override ESleekChildLayout UseChildAutoLayout
		{
			set
			{
				ValidateNotDestroyed();
				bool changed = _useChildAutoLayout != value;
				_useChildAutoLayout = value;
				if (changed)
				{
					if (_useChildAutoLayout == ESleekChildLayout.Horizontal)
					{
						HorizontalLayoutGroup layoutGroup = transform.GetOrAddComponent<HorizontalLayoutGroup>();
						layoutGroup.childForceExpandWidth = _expandChildren;
						layoutGroup.childForceExpandHeight = false;
					}
					else
					{
						transform.DestroyComponentIfExists<HorizontalLayoutGroup>();
					}

					if (_useChildAutoLayout == ESleekChildLayout.Vertical)
					{
						VerticalLayoutGroup layoutGroup = transform.GetOrAddComponent<VerticalLayoutGroup>();
						layoutGroup.childForceExpandWidth = false;
						layoutGroup.childForceExpandHeight = _expandChildren;
					}
					else
					{
						transform.DestroyComponentIfExists<VerticalLayoutGroup>();
					}

					ApplyChildPerpendicularAlignment();
					ApplyChildLayoutPadding();
				}
			}
		}

		public override ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
		{
			set
			{
				ValidateNotDestroyed();
				_childPerpendicularAlignment = value;
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
					switch (_useChildAutoLayout)
					{
						case ESleekChildLayout.Horizontal:
							transform.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = _expandChildren;
							break;

						case ESleekChildLayout.Vertical:
							transform.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = _expandChildren;
							break;
					}
				}
			}
		}

		public override bool IgnoreLayout
		{
			set
			{
				ValidateNotDestroyed();
				bool changed = _ignoreLayout != value;
				_ignoreLayout = value;
				if (changed)
				{
					if (ShouldHaveLayoutElementComponent)
					{
						LayoutElement layout = transform.GetOrAddComponent<LayoutElement>();
						layout.ignoreLayout = true;
					}
					else
					{
						transform.DestroyComponentIfExists<LayoutElement>();
					}
				}
			}
		}

		public override float ChildAutoLayoutPadding
		{
			set
			{
				ValidateNotDestroyed();
				_childAutoLayoutPadding = value;
				ApplyChildLayoutPadding();
			}
		}

		public GlazierElementBase_uGUI _parent;
		public override ISleekElement Parent
		{
			get
			{
				ValidateNotDestroyed();
				return _parent;
			}
		}

		public GlazierElementBase_uGUI(Glazier_uGUI glazier)
		{
			this.glazier = glazier;
		}

		/// <summary>
		/// Called after constructor when not populating from component pool.
		/// </summary>
		public virtual void ConstructNew()
		{
			// Note: DontDestroyOnLoad is applied when returning to pool.
			gameObject = new GameObject(GetType().Name, typeof(RectTransform));
			transform = gameObject.GetRectTransform();
			transform.pivot = new Vector2(0.0f, 1.0f);
		}

		public class PoolData
		{
			public GameObject gameObject;
		}

		/// <summary>
		/// Called after constructor when re-using components from pool.
		/// </summary>
		public void ConstructFromPool(PoolData poolData)
		{
			gameObject = poolData.gameObject;
			transform = gameObject.GetRectTransform();

			gameObject.SetActive(true);
		}

		public override int FindIndexOfChild(ISleekElement child)
		{
			ValidateNotDestroyed();
			return _children.IndexOf((GlazierElementBase_uGUI) child.AttachmentRoot);
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
				throw new System.ArgumentNullException("child");

			ValidateNotDestroyed();
			if (child.AttachmentRoot is GlazierElementBase_uGUI typedChild)
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
				UnturnedLog.warn("{0} cannot remove non-IMGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
			}
		}

		public override void RemoveAllChildren()
		{
			ValidateNotDestroyed();
			foreach (GlazierElementBase_uGUI child in _children)
			{
				child.ValidateNotDestroyed();
				child._parent = null;
				child.InternalDestroy();
			}

			_children.Clear();
		}

		protected override void UpdateChildren()
		{
			foreach (GlazierElementBase_uGUI child in _children)
			{
				if (child.IsVisible)
				{
					child.Update();
				}
			}
		}

		/// <summary>
		/// Synchronize uGUI component colors with background/text/image etc. colors.
		/// Called when custom UI colors are changed, and after constructor.
		/// </summary>
		public virtual void SynchronizeColors() { }

		/// <summary>
		/// Synchronize uGUI component sprites with theme sprites.
		/// Called when custom UI theme is changed, and after constructor.
		/// </summary>
		public virtual void SynchronizeTheme() { }

		public override void AddChild(ISleekElement child)
		{
			ValidateNotDestroyed();
			if (child.AttachmentRoot is GlazierElementBase_uGUI typedChild)
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
				typedChild.transform.SetParent(AttachmentTransform, false);
				typedChild.UpdateDirtyTransform();
				typedChild.EnableComponents();
			}
			else
			{
				UnturnedLog.warn("{0} cannot add non-uGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
			}
		}

		public override Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
		{
			ValidateNotDestroyed();
			Rect absoluteRect = transform.GetAbsoluteRect();
			return new Vector2(((viewportPosition.x * Screen.width) - absoluteRect.xMin) / absoluteRect.width, (((1.0f - viewportPosition.y) * Screen.height) - absoluteRect.yMin) / absoluteRect.height);
		}

		public override Vector2 GetNormalizedCursorPosition()
		{
			ValidateNotDestroyed();
			Vector2 mousePosition = Input.mousePosition;
			Rect absoluteRect = transform.GetAbsoluteRect();
			return new Vector2((mousePosition.x - absoluteRect.xMin) / absoluteRect.width, (Screen.height - mousePosition.y - absoluteRect.yMin) / absoluteRect.height);
		}

		public override Vector2 GetAbsoluteSize()
		{
			ValidateNotDestroyed();
			Rect absoluteRect = transform.GetAbsoluteRect();
			return absoluteRect.size;
		}

		public override void SetAsFirstSibling()
		{
			ValidateNotDestroyed();
			if (_parent != null)
			{
				transform.SetAsFirstSibling();

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
			LayoutRebuilder.MarkLayoutForRebuild(transform);
		}

		protected override void UpdateDirtyTransform()
		{
			isTransformDirty = false;
			if (_useManualLayout)
			{
				transform.anchorMin = new Vector2(PositionScale_X, 1.0f - PositionScale_Y - SizeScale_Y);
				transform.anchorMax = new Vector2(PositionScale_X + SizeScale_X, 1.0f - PositionScale_Y);
				transform.anchoredPosition = new Vector2(PositionOffset_X, -PositionOffset_Y);
				transform.sizeDelta = new Vector2(SizeOffset_X, SizeOffset_Y);
			}
			else
			{
				transform.anchorMin = new Vector2(0.0f, 1.0f);
				transform.anchorMax = new Vector2(0.0f, 1.0f);
				transform.anchoredPosition = Vector2.zero;
				transform.sizeDelta = Vector2.zero;
			}

			if (_useWidthLayoutOverride || _useHeightLayoutOverride)
			{
				LayoutElement layoutElement = transform.GetComponent<LayoutElement>();
				if (layoutElement != null)
				{
					layoutElement.preferredWidth = _useWidthLayoutOverride ? SizeOffset_X : 0.0f;
					layoutElement.preferredHeight = _useHeightLayoutOverride ? SizeOffset_Y : 0.0f;

					// Setting minimums as well prevents larger elements from squishing us below preferred size.
					layoutElement.minWidth = layoutElement.preferredWidth;
					layoutElement.minHeight = layoutElement.preferredHeight;
				}
			}
		}

		/// <returns>False if element couldn't be released into pool and should be destroyed.</returns>
		protected virtual bool ReleaseIntoPool()
		{
			return false;
		}

		/// <summary>
		/// Unity recommends enabling components after parenting into the destination hierarchy.
		/// </summary>
		protected virtual void EnableComponents()
		{

		}

		protected void PopulateBasePoolData(PoolData poolData)
		{
			transform.SetParent(null, false);

			// Pooled UI is recycled between scenes. We flag DontDestroyOnLoad here because it is reset when reparented. 
			Object.DontDestroyOnLoad(gameObject);
			poolData.gameObject = gameObject;

			gameObject = null;
			transform = null;
		}

		public override void InternalDestroy()
		{
			RemoveAllChildren();

			// Nelson 2024-04-01: Got a log where an exception was being thrown in GlazierBox_uGUI.ReleaseIntoPool
			// because the components were null. My guess is that somehow it got destroyed twice, so along with checking
			// they exist before returning to pool I'm also adding this gameObject check here just in case.
			if (gameObject != null)
			{
				// Nelson 2023-10-14: Ideally, we would remove the LayoutElement or LayoutGroup and return to pool.
				// Unfortunately, there seems to be a case where the component is destroyed but because it's not
				// actually removed by Unity until the end of frame it can be recycled and throw an exception when
				// another element tries to add a VerticalLayout alongside a HorizontalLayout (or vice versa).
				// e.g. public issue #4139
				// As a hacky workaround to make sure everything's working properly I'm just going to destroy them for now.
				bool mustDestroy = ShouldHaveLayoutElementComponent || _useChildAutoLayout != ESleekChildLayout.None;
				if (mustDestroy || !ReleaseIntoPool())
				{
					if (gameObject != null)
					{
						Object.Destroy(gameObject);
						gameObject = null;
					}
				}
			}
			else
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.error("InternalDestroy called when gameObject is already null! Was it destroyed twice?");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}

#if VALIDATE_GLAZIER_USE_AFTER_DESTROY
			wasDestroyed = true;
#endif
		}

		/// <summary>
		/// RectTransform children should be attached to. Overridden by ScrollView content panel.
		/// </summary>
		public virtual RectTransform AttachmentTransform
		{
			get
			{
				ValidateNotDestroyed();
				return transform;
			}
		}

		public GameObject gameObject
		{
			get;
			private set;
		}

		public RectTransform transform
		{
			get;
			private set;
		}

		/// <summary>
		/// This helper property's purpose is to:
		/// - Ensure other properties don't accidentally remove LayoutElement if others need it.
		/// - Ensure LayoutElement is destroyed before returning to pool.
		/// </summary>
		private bool ShouldHaveLayoutElementComponent => _useWidthLayoutOverride || _useHeightLayoutOverride || _ignoreLayout;

		private void ApplyChildPerpendicularAlignment()
		{
			if (_useChildAutoLayout == ESleekChildLayout.Horizontal)
			{
				HorizontalLayoutGroup layoutGroup = transform.GetComponent<HorizontalLayoutGroup>();
				switch (_childPerpendicularAlignment)
				{
					default:
					case ESleekChildPerpendicularAlignment.Center:
						layoutGroup.childAlignment = TextAnchor.MiddleLeft;
						break;

					case ESleekChildPerpendicularAlignment.Top:
						layoutGroup.childAlignment = TextAnchor.UpperLeft;
						break;

					case ESleekChildPerpendicularAlignment.Bottom:
						layoutGroup.childAlignment = TextAnchor.LowerLeft;
						break;
				}
			}
		}

		private void ApplyChildLayoutPadding()
		{
			int rectOffsetPixels = Mathf.RoundToInt(_childAutoLayoutPadding);
			RectOffset rectOffset = new RectOffset(rectOffsetPixels, rectOffsetPixels, rectOffsetPixels, rectOffsetPixels);

			switch (_useChildAutoLayout)
			{
				case ESleekChildLayout.Horizontal:
					transform.GetComponent<HorizontalLayoutGroup>().padding = rectOffset;
					break;

				case ESleekChildLayout.Vertical:
					transform.GetComponent<VerticalLayoutGroup>().padding = rectOffset;
					break;
			}
		}

		internal List<GlazierElementBase_uGUI> _children = new List<GlazierElementBase_uGUI>();
	}
}
