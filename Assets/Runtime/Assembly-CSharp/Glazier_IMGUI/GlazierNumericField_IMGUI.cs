////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal abstract class GlazierNumericField_IMGUI : GlazierElementBase_IMGUI, ISleekNumericField
	{
		public GlazierNumericField_IMGUI() : base()
		{
			controlName = GlazierUtils_IMGUI.CreateUniqueControlName();
		}

		public string TooltipText
		{
			get;
			set;
		} = string.Empty;

		public SleekColor TextColor
		{
			get;
			set;
		} = GlazierConst.DefaultFieldForegroundColor;

		public SleekColor BackgroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultFieldBackgroundColor;

		public bool IsClickable
		{
			get;
			set;
		} = true;

		public override void OnGUI()
		{
			bool wasGUIenabled = GUI.enabled;
			GUI.enabled = IsClickable;

			GUI.SetNextControlName(controlName);
			string value = GlazierUtils_IMGUI.drawField(drawRect, fontStyle, fontAlignment, fontSizeInt, BackgroundColor, TextColor, text, 64, false, ETextContrastContext.Default);

			GUI.enabled = wasGUIenabled;

			if (GUI.changed && ParseNumericInput(value))
			{
				text = value;
			}

			if (GUI.GetNameOfFocusedControl() == controlName)
			{
				if (Event.current.isKey && Event.current.type == EventType.KeyUp)
				{
					if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == ControlsSettings.dashboard)
					{
						GUI.FocusControl(string.Empty);
					}
					else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
					{
						OnReturnPressed();
						GUI.FocusControl(string.Empty);
					}
				}
			}

			ChildrenOnGUI();
		}

		protected abstract bool ParseNumericInput(string input);
		protected virtual void OnReturnPressed() { }

		protected string text; // No default because subclass sets text.

		public FontStyle fontStyle = GlazierConst.DefaultFieldFontStyle;
		public TextAnchor fontAlignment = TextAnchor.MiddleCenter;
		public int fontSizeInt = GlazierUtils_IMGUI.GetFontSize(ESleekFontSize.Default);

		private string controlName;
	}
}
