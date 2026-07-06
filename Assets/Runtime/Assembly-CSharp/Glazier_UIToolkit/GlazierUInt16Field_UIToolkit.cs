////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierUInt16Field_UIToolkit : GlazierNumericField_UIToolkit, ISleekUInt16Field
	{
		public event TypedUInt16 OnValueChanged;

		private ushort _state;
		public ushort Value
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

		public ushort MinValue
		{
			get;
			set;
		} = ushort.MinValue;

		public ushort MaxValue
		{
			get;
			set;
		} = ushort.MaxValue;

		public GlazierUInt16Field_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{

		}

		protected override bool ParseNumericInput(string input)
		{
			bool success;
			if (string.IsNullOrEmpty(input))
			{
				// Treat as success to prevent resetting text to "0"
				_state = 0;
				success = true;
			}
			else
			{
				success = ushort.TryParse(input, out _state);
			}

			if (success)
			{
				_state = MathfEx.Clamp(_state, MinValue, MaxValue);
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
