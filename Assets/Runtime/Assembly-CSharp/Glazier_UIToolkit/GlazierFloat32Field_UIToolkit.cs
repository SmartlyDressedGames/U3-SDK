////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierFloat32Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekFloat32Field
	{
		public event TypedSingle OnValueSubmitted;
		public event TypedSingle OnValueChanged;

		private float _state;
		public float Value
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

		public GlazierFloat32Field_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{

		}

		protected override void OnSubmitted()
		{
			OnValueSubmitted?.Invoke(this, Value);
		}

		protected override bool ParseNumericInput(string input)
		{
			bool success;
			if (string.IsNullOrEmpty(input) || string.Equals(input, "-"))
			{
				// Treat as success to prevent resetting text to "0"
				_state = 0.0f;
				success = true;
			}
			else
			{
				// Prevent parsing issues when player is just adding the decimal point.
				if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
				{
					input += "0";
				}

				success = float.TryParse(input, out _state);
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
