////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void TypedSingle(ISleekFloat32Field field, float value);

	public interface ISleekFloat32Field : ISleekElement, ISleekNumericField
	{
		/// <summary>
		/// Invoked after return key is pressed while typing.
		/// </summary>
		event TypedSingle OnValueSubmitted;
		event TypedSingle OnValueChanged;

		float Value
		{
			get;
			set;
		}
	}
}
