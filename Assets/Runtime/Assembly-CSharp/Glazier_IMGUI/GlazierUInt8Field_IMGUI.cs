////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierUInt8Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt8Field
	{
		public event TypedByte OnValueChanged;

		public GlazierUInt8Field_IMGUI() : base()
		{
			Value = 0;
		}

		private byte _state;
		public byte Value
		{
			get
			{
				ValidateNotDestroyed();
				return _state;
			}

			set
			{
				ValidateNotDestroyed();
				_state = value;
				text = Value.ToString();
			}
		}

		protected override bool ParseNumericInput(string input)
		{
			byte newValue;
			if (byte.TryParse(input, out newValue))
			{
				if (_state != newValue)
				{
					_state = newValue;
					OnValueChanged?.Invoke(this, _state);
				}
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
