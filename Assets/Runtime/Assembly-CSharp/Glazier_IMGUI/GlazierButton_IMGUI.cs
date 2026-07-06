////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierButton_IMGUI : GlazierLabel_IMGUI, ISleekButton
	{
		public event ClickedButton OnClicked;
		public event ClickedButton OnRightClicked;

		public bool IsClickable
		{
			get;
			set;
		} = true;

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
				calculateContent();
			}
		}

		public SleekColor BackgroundColor
		{
			get;
			set;
		}

		public override void OnGUI()
		{
			bool wasGUIenabled = GUI.enabled;
			GUI.enabled = IsClickable;

			if (IsRaycastTarget)
			{
				if (GlazierUtils_IMGUI.drawButton(drawRect, BackgroundColor))
				{
					if (Event.current.button == 0)
					{
						OnClicked?.Invoke(this);
					}
					else if (Event.current.button == 1)
					{
						OnRightClicked?.Invoke(this);
					}
				}
			}
			else
			{
				GlazierUtils_IMGUI.drawBox(drawRect, BackgroundColor);
			}

			GUI.enabled = wasGUIenabled;

			GlazierUtils_IMGUI.drawLabel(drawRect, FontStyle, TextAlignment, fontSizeInt, shadowContent, TextColor, content, TextContrastContext);

			ChildrenOnGUI();
		}

		protected override void calculateContent()
		{
			base.calculateContent();

			if (!_isRaycastTarget)
			{
				content.tooltip = null;
			}
		}

		public GlazierButton_IMGUI() : base()
		{
			BackgroundColor = GlazierConst.DefaultButtonBackgroundColor;
		}
	}
}
