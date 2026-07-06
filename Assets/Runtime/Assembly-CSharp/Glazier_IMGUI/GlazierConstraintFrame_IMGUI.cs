////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierConstraintFrame_IMGUI : GlazierElementBase_IMGUI, ISleekConstraintFrame
	{
		private ESleekConstraint _constraint;
		public ESleekConstraint Constraint
		{
			get
			{
				ValidateNotDestroyed();
				return _constraint;
			}

			set
			{
				ValidateNotDestroyed();
				_constraint = value;
				isTransformDirty = true;
			}
		}

		private float _aspectRatio = 1.0f;
		public float AspectRatio
		{
			get
			{
				ValidateNotDestroyed();
				return _aspectRatio;
			}

			set
			{
				ValidateNotDestroyed();
				_aspectRatio = value;
				isTransformDirty = true;
			}
		}

		protected override Rect CalculateDrawRect()
		{
			Rect area = base.CalculateDrawRect();

			if (Constraint == ESleekConstraint.FitInParent)
			{
				// Children want to use the largest area of a certain aspect
				// ratio that fits within the parent area. For example the inventory
				// on the main menu is a square (1.0 aspect ratio).

				if (area.width < area.height * _aspectRatio)
				{
					float newHeight = area.width / _aspectRatio;
					area.y += (area.height - newHeight) * 0.5f;
					area.height = newHeight;
				}
				else
				{
					float newWidth = area.height * _aspectRatio;
					area.x += (area.width - newWidth) * 0.5f;
					area.width = newWidth;
				}
			}

			return area;
		}
	}
}
