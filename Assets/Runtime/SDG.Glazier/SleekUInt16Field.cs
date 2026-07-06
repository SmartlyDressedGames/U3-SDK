////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void TypedUInt16(ISleekUInt16Field field, ushort value);

	public interface ISleekUInt16Field : ISleekElement, ISleekNumericField
	{
		event TypedUInt16 OnValueChanged;

		ushort Value
		{
			get;
			set;
		}

		ushort MinValue
		{
			get;
			set;
		}

		ushort MaxValue
		{
			get;
			set;
		}
	}
}
