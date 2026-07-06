////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal abstract class GlazierNumericField_UIToolkit : GlazierStringField_UIToolkit, ISleekNumericField
	{
		public GlazierNumericField_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{ }

		protected void SynchronizeText()
		{
			Text = NumberToString();
		}

		protected override void OnControlValueChanged(ChangeEvent<string> changeEvent)
		{
			if (!ParseNumericInput(changeEvent.newValue))
			{
				SynchronizeText();
			}
		}

		protected abstract bool ParseNumericInput(string input);
		protected abstract string NumberToString();
	}
}
