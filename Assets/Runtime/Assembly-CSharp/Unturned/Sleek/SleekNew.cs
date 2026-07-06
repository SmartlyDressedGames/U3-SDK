////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekNew : SleekWrapper
	{
		public SleekNew(bool isUpdate = false) : base()
		{
			PositionOffset_X = -105;
			PositionScale_X = 1.0f;
			SizeOffset_X = 100;
			SizeOffset_Y = 30;

			label = Glazier.Get().CreateLabel();
			label.SizeScale_X = 1.0f;
			label.SizeScale_Y = 1.0f;
			label.TextAlignment = TextAnchor.MiddleRight;
			label.Text = Provider.localization.format(isUpdate ? "Updated" : "New");
			label.TextColor = Color.green;
			label.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(label);
		}

		internal ISleekLabel label;
	}
}
