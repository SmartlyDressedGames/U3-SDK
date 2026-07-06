////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void TypedUInt32(ISleekUInt32Field field, uint value);

	public interface ISleekUInt32Field : ISleekElement, ISleekNumericField
	{
		event TypedUInt32 OnValueChanged;

		uint Value
		{
			get;
			set;
		}
	}
}
