////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void TypedInt32(ISleekInt32Field field, int value);

	public interface ISleekNumericField : ISleekWithTooltip
	{
		SleekColor BackgroundColor
		{
			get;
			set;
		}

		SleekColor TextColor
		{
			get;
			set;
		}

		/// <summary>
		/// When false the field is disabled and greyed out.
		/// </summary>
		bool IsClickable
		{
			get;
			set;
		}
	}

	public interface ISleekInt32Field : ISleekElement, ISleekNumericField
	{
		event TypedInt32 OnValueChanged;

		int Value
		{
			get;
			set;
		}
	}
}
