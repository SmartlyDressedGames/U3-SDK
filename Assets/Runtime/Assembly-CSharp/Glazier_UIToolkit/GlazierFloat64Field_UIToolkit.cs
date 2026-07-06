////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierFloat64Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekFloat64Field
	{
		public event TypedDouble OnValueChanged;

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
				SynchronizeText();
			}
		}

		public GlazierFloat64Field_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{

		}

		protected override bool ParseNumericInput(string input)
		{
			bool success;
			if (string.IsNullOrEmpty(input) || string.Equals(input, "-"))
			{
				// Treat as success to prevent resetting text to "0"
				_state = 0.0;
				success = true;
			}
			else
			{
				// Prevent parsing issues when player is just adding the decimal point.
				if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
				{
					input += "0";
				}

				success = double.TryParse(input, out _state);
			}

			if (success)
			{
				OnValueChanged?.Invoke(this, _state);
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override string NumberToString()
		{
			return Value.ToString("F3");
		}
	}
}
