////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;

namespace SDG.Unturned
{
	internal class GlazierInt32Field_uGUI : GlazierNumericField_uGUI, ISleekInt32Field
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

		public GlazierInt32Field_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			fieldComponent.contentType = TMP_InputField.ContentType.IntegerNumber;
			SynchronizeText();
		}

		protected override bool ParseNumericInput(string input)
		{
			if (int.TryParse(input, out _state))
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
