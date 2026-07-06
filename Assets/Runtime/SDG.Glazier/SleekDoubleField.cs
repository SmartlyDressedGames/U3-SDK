////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void TypedDouble(ISleekFloat64Field field, double value);

	public interface ISleekFloat64Field : ISleekElement, ISleekNumericField
	{
		event TypedDouble OnValueChanged;

		double Value
		{
			get;
			set;
		}
	}
}
