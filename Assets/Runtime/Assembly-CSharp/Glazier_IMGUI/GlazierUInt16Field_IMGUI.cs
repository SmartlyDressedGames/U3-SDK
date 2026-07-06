////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierUInt16Field_IMGUI : GlazierNumericField_IMGUI, ISleekUInt16Field
	{
		public event TypedUInt16 OnValueChanged;

		public GlazierUInt16Field_IMGUI() : base()
		{
			Value = 0;
		}

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
				text = Value.ToString();
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

		protected override bool ParseNumericInput(string input)
		{
			ushort newValue;
			if (ushort.TryParse(input, out newValue))
			{
				newValue = MathfEx.Clamp(newValue, MinValue, MaxValue);
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
