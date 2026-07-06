////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierInt32Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekInt32Field
	{
		public event TypedInt32 OnValueChanged;

		private int _state;
		public int Value
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

		public GlazierInt32Field_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{

		}

		protected override bool ParseNumericInput(string input)
		{
			bool success;
			if (string.IsNullOrEmpty(input) || string.Equals(input, "-"))
			{
				// Treat as success to prevent resetting text to "0"
				_state = 0;
				success = true;
			}
			else
			{
				success = int.TryParse(input, out _state);
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
			return Value.ToString();
		}
	}
}
