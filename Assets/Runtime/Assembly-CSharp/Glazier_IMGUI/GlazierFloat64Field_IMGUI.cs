////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierFloat64Field_IMGUI : GlazierNumericField_IMGUI, ISleekFloat64Field
	{
		public event TypedDouble OnValueChanged;

		public GlazierFloat64Field_IMGUI() : base()
		{
			Value = 0.0;
		}

		private double _state;
		public double Value
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
				text = Value.ToString("F3");
			}
		}

		protected override bool ParseNumericInput(string input)
		{
			// Prevent parsing issues when player is just adding the decimal point.
			if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
			{
				input += "0";
			}

			double newValue;
			if (double.TryParse(input, out newValue))
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
