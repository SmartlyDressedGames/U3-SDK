////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;

namespace SDG.Unturned
{
	internal class GlazierFloat32Field_uGUI : GlazierNumericField_uGUI, ISleekFloat32Field
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

		public GlazierFloat32Field_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			fieldComponent.contentType = TMP_InputField.ContentType.DecimalNumber;
			SynchronizeText();
		}

		protected override void OnUnitySubmit(string input)
		{
			OnValueSubmitted?.Invoke(this, _state);
		}

		protected override bool ParseNumericInput(string input)
		{
			// Prevent parsing issues when player is just adding the decimal point.
			if (input.Length > 0 && !char.IsDigit(input, input.Length - 1))
			{
				input += "0";
			}

			if (float.TryParse(input, out _state))
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
