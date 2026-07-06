////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Implemented by glazier primitives that support tooltip text when hovered by the pointer.
	/// </summary>
	public interface ISleekWithTooltip
	{
		string TooltipText
		{
			get;
			set;
		}
	}
}
