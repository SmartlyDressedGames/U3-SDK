////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierUInt32Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt32Field
	{
		public event TypedUInt32 OnValueChanged;

		public GlazierUInt32Field_IMGUI() : base()
		{
			Value = 0;
		}

		private uint _state;
		public uint Value
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
			uint newValue;
			if (uint.TryParse(input, out newValue))
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
