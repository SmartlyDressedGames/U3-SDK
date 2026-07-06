////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierProxy_UIToolkit : GlazierElementBase_UIToolkit, ISleekProxyImplementation
	{
		public SleekWrapper GetWrapper() => owner;

		public GlazierProxy_UIToolkit(Glazier_UIToolkit glazier, SleekWrapper owner) : base(glazier)
		{
			this.owner = owner;
			visualElement = new VisualElement();
			visualElement.userData = this;
			visualElement.AddToClassList("unturned-empty");
			visualElement.pickingMode = PickingMode.Ignore; // Proxy is not clickable in of itself, rather its contents are.
			visualElement.name = owner.GetType().Name;
		}

		public override void Update()
		{
			owner.OnUpdate();
			base.Update();
		}

		public override void InternalDestroy()
		{
			owner.OnDestroy();
			base.InternalDestroy();
		}

		private SleekWrapper owner;
	}
}
