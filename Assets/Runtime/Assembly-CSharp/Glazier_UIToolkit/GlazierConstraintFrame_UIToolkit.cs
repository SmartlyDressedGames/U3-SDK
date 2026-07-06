////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class ConstraintFrameParentElement : VisualElement
	{
		public VisualElement _contentContainerOverride;
		public override VisualElement contentContainer => _contentContainerOverride;
	}

	internal class ConstraintFrameChildElement : VisualElement
	{
		public ESleekConstraint constraint;
		public float aspectRatio = 1.0f;

		public void OnParentGeometryChanged(GeometryChangedEvent geometryChangedEvent)
		{
			if (constraint == ESleekConstraint.NONE)
			{
				style.left = 0;
				style.right = 0;
				style.top = 0;
				style.bottom = 0;
				return;
			}

			// Children want to use the largest area of a certain aspect
			// ratio that fits within the parent area. For example the inventory
			// on the main menu is a square (1.0 aspect ratio).
			Rect area = geometryChangedEvent.newRect;

			if (area.width < area.height * aspectRatio)
			{
				style.left = 0;
				style.right = 0;
				float normalizedHeight = (area.width / aspectRatio) / area.height;
				StyleLength spacing = Length.Percent((1.0f - normalizedHeight) * 50.0f);
				style.top = spacing;
				style.bottom = spacing;
			}
			else
			{
				float normalizedWidth = (area.height * aspectRatio) / area.width;
				StyleLength spacing = Length.Percent((1.0f - normalizedWidth) * 50.0f);
				style.left = spacing;
				style.right = spacing;
				style.top = 0;
				style.bottom = 0;
			}
		}
	}

	/// <summary>
	/// UITK implementation consists of a container element which respects the regular position and size
	/// properties, and a child content element which fits itself in the container.
	/// </summary>
	internal class GlazierConstraintFrame_UIToolkit : GlazierElementBase_UIToolkit, ISleekConstraintFrame
	{
		public ESleekConstraint Constraint
		{
			get
			{
				ValidateNotDestroyed();
				return contentElement.constraint;
			}

			set
			{
				ValidateNotDestroyed();

				if (contentElement.constraint != ESleekConstraint.NONE)
					throw new System.NotSupportedException();

				contentElement.constraint = value;
			}
		}

		public float AspectRatio
		{
			get
			{
				ValidateNotDestroyed();
				return contentElement.aspectRatio;
			}

			set
			{
				ValidateNotDestroyed();
				contentElement.aspectRatio = value;
			}
		}

		public GlazierConstraintFrame_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			containerElement = new ConstraintFrameParentElement();
			containerElement.pickingMode = PickingMode.Ignore;
			containerElement.userData = this;
			containerElement.AddToClassList("unturned-constraint-frame-container");
			containerElement._contentContainerOverride = containerElement;

			contentElement = new ConstraintFrameChildElement();
			contentElement.pickingMode = PickingMode.Ignore;
			contentElement.userData = this;
			contentElement.AddToClassList("unturned-constraint-frame-content");
			containerElement.Add(contentElement);

			containerElement.RegisterCallback<GeometryChangedEvent>(contentElement.OnParentGeometryChanged);
			containerElement._contentContainerOverride = contentElement;

			visualElement = containerElement;
		}

		private ConstraintFrameParentElement containerElement;
		private ConstraintFrameChildElement contentElement;
	}
}
