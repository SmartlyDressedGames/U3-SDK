////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierStringField_IMGUI : GlazierElementBase_IMGUI, ISleekField
	{
		public event Entered OnTextSubmitted;
		public event Typed OnTextChanged;
		public event Escaped OnTextEscaped;

		public bool IsPasswordField
		{
			get;
			set;
		} = false;

		public string PlaceholderText
		{
			get;
			set;
		} = string.Empty;

		public bool IsMultiline
		{
			get;
			set;
		} = false;

		public string Text
		{
			get;
			set;
		} = string.Empty;

		public string TooltipText
		{
			get;
			set;
		} = string.Empty;

		public FontStyle FontStyle
		{
			get;
			set;
		} = GlazierConst.DefaultFieldFontStyle;

		public TextAnchor TextAlignment
		{
			get;
			set;
		} = TextAnchor.MiddleCenter;

		private int fontSizeInt;
		private ESleekFontSize fontSizeEnum;
		public ESleekFontSize FontSize
		{
			get
			{
				ValidateNotDestroyed();
				return fontSizeEnum;
			}

			set
			{
				ValidateNotDestroyed();
				fontSizeEnum = value;
				fontSizeInt = GlazierUtils_IMGUI.GetFontSize(fontSizeEnum);
			}
		}

		public ETextContrastContext TextContrastContext
		{
			get;
			set;
		} = GlazierConst.DefaultFieldContrastContext;

		public SleekColor TextColor
		{
			get;
			set;
		} = GlazierConst.DefaultFieldForegroundColor;

		public bool AllowRichText
		{
			get => false;
			set { }
		}

		public SleekColor BackgroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultFieldBackgroundColor;

		public int MaxLength
		{
			get;
			set;
		} = GlazierConst.DefaultTextFieldMaxLength;

		public bool IsClickable
		{
			get;
			set;
		} = true;

		public void FocusControl()
		{
			GUI.FocusControl(controlName);
		}

		public void ClearFocus()
		{
			if (GUI.GetNameOfFocusedControl() == controlName)
			{
				GUI.FocusControl(string.Empty);
			}
		}

		public override void OnGUI()
		{
			bool wasGUIenabled = GUI.enabled;
			GUI.enabled = IsClickable;

			GUI.SetNextControlName(controlName);

			if (IsPasswordField)
			{
				Text = GlazierUtils_IMGUI.DrawPasswordField(drawRect, FontStyle, TextAlignment, fontSizeInt, BackgroundColor, TextColor, Text, MaxLength, PlaceholderText, '*', TextContrastContext);
			}
			else
			{
				Text = GlazierUtils_IMGUI.DrawTextInputField(drawRect, FontStyle, TextAlignment, fontSizeInt, BackgroundColor, TextColor, Text, MaxLength, PlaceholderText, IsMultiline, TextContrastContext);
			}

			GUI.enabled = wasGUIenabled;

			if (GUI.changed)
			{
				OnTextChanged?.Invoke(this, Text);
			}

			if (GUI.GetNameOfFocusedControl() == controlName)
			{
				if (Event.current.isKey && Event.current.type == EventType.KeyUp)
				{
					if (Event.current.keyCode == KeyCode.Escape)
					{
						GUI.FocusControl(string.Empty);
						OnTextEscaped?.Invoke(this);
					}
					else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
					{
						// Multiline fields (namely the buildable sign editor) need Return for newlines. Accidentally
						// broke this when restoring IMGUI support after the uGUI update.
						if (!IsMultiline)
						{
							OnTextSubmitted?.Invoke(this);
							GUI.FocusControl(string.Empty);
						}
					}
				}
			}

			ChildrenOnGUI();
		}

		public GlazierStringField_IMGUI() : base()
		{
			BackgroundColor = GlazierConst.DefaultFieldBackgroundColor;
			controlName = GlazierUtils_IMGUI.CreateUniqueControlName();
			FontSize = ESleekFontSize.Default;
		}

		private string controlName;
	}
}
