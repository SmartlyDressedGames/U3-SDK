////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void Dragged(ISleekSlider slider, float state);

	public interface ISleekSlider : ISleekElement
	{
		event Dragged OnValueChanged;

		ESleekOrientation Orientation
		{
			get;
			set;
		}

		float Value
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
		/// Handle color. Not supported by IMGUI.
		/// </summary>
		SleekColor ForegroundColor
		{
			get;
			set;
		}

		bool IsInteractable
		{
			get;
			set;
		}
	}
}
