////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierEmpty_UIToolkit : GlazierElementBase_UIToolkit
	{
		public GlazierEmpty_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			visualElement = new VisualElement();
			visualElement.userData = this;
			visualElement.pickingMode = PickingMode.Ignore;
			visualElement.AddToClassList("unturned-empty");
		}
	}
}
