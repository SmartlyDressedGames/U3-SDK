////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Base class for IMGUI implementations of primitive building block widgets.
	/// </summary>
	internal class GlazierElementBase_IMGUI : GlazierElementBase
	{
		public virtual void OnGUI()
		{
			ChildrenOnGUI();
		}

		public override int FindIndexOfChild(ISleekElement child)
		{
			ValidateNotDestroyed();
			return _children.IndexOf((GlazierElementBase_IMGUI) child.AttachmentRoot);
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
			ValidateNotDestroyed();
			if (child.AttachmentRoot is GlazierElementBase_IMGUI typedChild)
			{
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
			foreach (GlazierElementBase_IMGUI child in _children)
			{
				child._parent = null;
				child.InternalDestroy();
			}

			_children.Clear();
		}

		protected override void UpdateChildren()
		{
			foreach (GlazierElementBase_IMGUI child in _children)
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
			if (child.AttachmentRoot is GlazierElementBase_IMGUI typedChild)
			{
				if (typedChild._parent == this)
					return;

				if (typedChild._parent != null)
				{
					typedChild._parent._children.Remove(typedChild);
				}

				_children.Add(typedChild);
				typedChild._parent = this;
				typedChild.UpdateDirtyTransform();
			}
			else
			{
				UnturnedLog.warn("{0} cannot add non-IMGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
			}
		}

		public override void InternalDestroy()
		{
			RemoveAllChildren();

#if VALIDATE_GLAZIER_USE_AFTER_DESTROY
			wasDestroyed = true;
#endif
		}

		public override Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
		{
			ValidateNotDestroyed();
			Vector2 normalizedPosition;

			Rect screenRect = GetDrawRectInScreenSpace();

			if (screenRect.width > 0)
			{
				normalizedPosition.x = ((viewportPosition.x * Screen.width) - screenRect.xMin) / screenRect.width;
			}
			else
			{
				normalizedPosition.x = 0.5f;
			}

			if (screenRect.height > 0)
			{
				normalizedPosition.y = (((1.0f - viewportPosition.y) * Screen.height) - screenRect.yMin) / screenRect.height;
			}
			else
			{
				normalizedPosition.y = 0.5f;
			}

			return normalizedPosition;
		}

		public override Vector2 GetNormalizedCursorPosition()
		{
			ValidateNotDestroyed();
			Vector2 normalizedPosition;
			Vector2 mousePosition = Input.mousePosition;
			Rect screenRect = GetDrawRectInScreenSpace();

			if (screenRect.width > 0)
			{
				normalizedPosition.x = (mousePosition.x - screenRect.xMin) / screenRect.width;
			}
			else
			{
				normalizedPosition.x = 0.5f;
			}

			if (screenRect.height > 0)
			{
				normalizedPosition.y = (Screen.height - mousePosition.y - screenRect.yMin) / screenRect.height;
			}
			else
			{
				normalizedPosition.y = 0.5f;
			}

			return normalizedPosition;
		}

		public override Vector2 GetAbsoluteSize()
		{
			ValidateNotDestroyed();
			Rect screenRect = GetDrawRectInScreenSpace();
			return screenRect.size;
		}

		public override void SetAsFirstSibling()
		{
			ValidateNotDestroyed();
			if (_parent != null)
			{
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

		public GlazierElementBase_IMGUI _parent;
		public override ISleekElement Parent
		{
			get
			{
				ValidateNotDestroyed();
				return _parent;
			}
		}

		/// <summary>
		/// Position passed into the GUI draw methods.
		/// </summary>
		public Rect drawRect;

		protected virtual void TransformChildDrawPositionIntoParentSpace(ref Vector2 position)
		{ }

		protected Rect GetDrawRectInScreenSpace()
		{
			Rect rect = drawRect;
			Vector2 position = rect.position;

			GlazierElementBase_IMGUI element = _parent;
			while (element != null)
			{
				element.TransformChildDrawPositionIntoParentSpace(ref position);
				element = element._parent;
			}

			rect.position = position;
			return rect;
		}

		protected virtual Rect GetLayoutRect()
		{
			return drawRect;
		}

		protected virtual Rect CalculateDrawRect()
		{
			if (_parent == null)
			{
#if WINDEBUG
				return new Rect(Screen.width * 0.25f, 0, Screen.width * 0.5f, Screen.height);
#else
				if (Screen.width == 5760 && Screen.height == 1080) // triple monitor hack
				{
					return new Rect(1920, 0, 1920, 1080);
				}
				else
				{
					return new Rect(PositionOffset_X, PositionOffset_Y, Screen.width, Screen.height);
				}
#endif
			}

			float layoutScale = GraphicsSettings.userInterfaceScale;
			Rect area = _parent.GetLayoutRect();

			area.x += (PositionOffset_X * layoutScale) + (area.width * PositionScale_X);
			area.y += (PositionOffset_Y * layoutScale) + (area.height * PositionScale_Y);
			area.width = (SizeOffset_X * layoutScale) + (area.width * SizeScale_X);
			area.height = (SizeOffset_Y * layoutScale) + (area.height * SizeScale_Y);

			return area;
		}

		protected override void UpdateDirtyTransform()
		{
			isTransformDirty = false;
			drawRect = CalculateDrawRect();

			foreach (GlazierElementBase_IMGUI child in _children)
			{
				child.isTransformDirty = true;
			}
		}

		protected void ChildrenOnGUI()
		{
			// Tried GUI.depth here, but that only works from separate MonoBehaviours.
			// Cannot use foreach because child OnGUI might invoke an event (e.g. OnClick) that modifies children array.
			for (int childIndex = 0; childIndex < _children.Count; ++childIndex)
			{
				GlazierElementBase_IMGUI child = _children[childIndex];
				if (child.IsVisible)
				{
					child.OnGUI();
				}
			}
		}

		private List<GlazierElementBase_IMGUI> _children = new List<GlazierElementBase_IMGUI>();
	}
}
