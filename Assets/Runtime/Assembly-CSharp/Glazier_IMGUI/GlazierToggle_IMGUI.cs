////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierToggle_IMGUI : GlazierElementBase_IMGUI, ISleekToggle
	{
		public event Toggled OnValueChanged;

		public bool Value
		{
			get;
			set;
		}

		private string _tooltip;

		/// <summary>
		/// Tooltip text.
		/// </summary>
		public string TooltipText
		{
			get
			{
				ValidateNotDestroyed();
				return _tooltip;
			}

			set
			{
				ValidateNotDestroyed();
				_tooltip = value;
				content = new GUIContent(string.Empty, _tooltip);
			}
		}

		public SleekColor BackgroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultToggleBackgroundColor;

		public SleekColor ForegroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultToggleForegroundColor;

		public bool IsInteractable
		{
			get;
			set;
		} = true;

		/// <summary>
		/// Holds tooltip text
		/// </summary>
		protected GUIContent content = new GUIContent();

		public override void OnGUI()
		{
			bool wasGUIenabled = GUI.enabled;
			GUI.enabled = IsInteractable;
			bool value = GlazierUtils_IMGUI.drawToggle(drawRect, BackgroundColor, Value, content);
			GUI.enabled = wasGUIenabled;

			if (value != Value)
			{
				Value = value;
				OnValueChanged?.Invoke(this, value);
			}

			ChildrenOnGUI();
		}

		public GlazierToggle_IMGUI() : base()
		{
			SizeOffset_X = 40;
			SizeOffset_Y = 40;
		}
	}
}
