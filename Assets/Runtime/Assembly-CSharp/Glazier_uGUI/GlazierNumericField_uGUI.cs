////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal abstract class GlazierNumericField_uGUI : GlazierStringField_uGUI, ISleekNumericField
	{
		public GlazierNumericField_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		protected void SynchronizeText()
		{
			Text = NumberToString();
		}

		protected override void OnUnityValueChanged(string input)
		{
			if (!ParseNumericInput(input))
			{
				SynchronizeText();
			}
		}

		protected abstract bool ParseNumericInput(string input);
		protected abstract string NumberToString();
	}
}
