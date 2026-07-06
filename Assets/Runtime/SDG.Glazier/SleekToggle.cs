////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void Toggled(ISleekToggle toggle, bool state);

	public interface ISleekToggle : ISleekElement, ISleekWithTooltip
	{
		event Toggled OnValueChanged;

		bool Value
		{
			get;
			set;
		}

		SleekColor BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Checkmark color. Not supported by IMGUI.
		/// </summary>
		SleekColor ForegroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// If false the button is disabled.
		/// </summary>
		bool IsInteractable
		{
			get;
			set;
		}
	}
}
