////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void ClickedButton(ISleekElement button);

	public interface ISleekButton : ISleekElement, ISleekLabel, ISleekWithTooltip
	{
		event ClickedButton OnClicked;

		/// <summary>
		/// Not ideal to have a separate event, but legacy code assumed it could use Event.current, and the only need
		/// for right clicks was the item context menu.
		/// </summary>
		event ClickedButton OnRightClicked;

		SleekColor BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// When false the button is disabled and greyed out.
		/// </summary>
		bool IsClickable
		{
			get;
			set;
		}

		/// <summary>
		/// When false the mouse can click buttons underneath this button.
		/// </summary>
		bool IsRaycastTarget
		{
			get;
			set;
		}
	}
}
