////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void TypedByte(ISleekUInt8Field field, byte value);

	public interface ISleekUInt8Field : ISleekElement, ISleekNumericField
	{
		event TypedByte OnValueChanged;

		byte Value
		{
			get;
			set;
		}
	}
}
