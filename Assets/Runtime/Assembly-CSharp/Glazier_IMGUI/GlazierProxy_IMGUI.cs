////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierProxy_IMGUI : GlazierElementBase_IMGUI, ISleekProxyImplementation
	{
		public SleekWrapper GetWrapper() => owner;

		public GlazierProxy_IMGUI(SleekWrapper owner) : base()
		{
			this.owner = owner;
		}

		public override void Update()
		{
			owner.OnUpdate();
			base.Update();
		}

		public override void InternalDestroy()
		{
			owner.OnDestroy();
#if VALIDATE_SLEEK_PROXY_USE_AFTER_DESTROY
			owner.wasDestroyed = true;
#endif
			base.InternalDestroy();
		}

		private SleekWrapper owner;
	}
}
