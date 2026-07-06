////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;

namespace SDG.Unturned
{
	internal class GlazierUInt16Field_uGUI : GlazierNumericField_uGUI, ISleekUInt16Field
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

		public GlazierUInt16Field_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			fieldComponent.contentType = TMP_InputField.ContentType.IntegerNumber;
			SynchronizeText();
		}

		protected override bool ParseNumericInput(string input)
		{
			if (ushort.TryParse(input, out _state))
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
