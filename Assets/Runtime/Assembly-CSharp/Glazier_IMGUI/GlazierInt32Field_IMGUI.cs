////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierInt32Field_IMGUI : GlazierNumericField_IMGUI, ISleekInt32Field
	{
		public event TypedInt32 OnValueChanged;

		public GlazierInt32Field_IMGUI() : base()
		{
			Value = 0;
		}

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
				text = Value.ToString();
			}
		}

		protected override bool ParseNumericInput(string input)
		{
			int newValue;
			if (int.TryParse(input, out newValue))
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
