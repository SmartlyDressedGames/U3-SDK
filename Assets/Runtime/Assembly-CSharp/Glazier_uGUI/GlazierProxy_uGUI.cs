////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define GLAZIER_PROXY_UGUI_PROFILING
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

namespace SDG.Unturned
{
	internal class GlazierProxy_uGUI : GlazierElementBase_uGUI, ISleekProxyImplementation
	{
		public SleekWrapper GetWrapper() => owner;

		public GlazierProxy_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public void InitOwner(SleekWrapper owner)
		{
			this.owner = owner;
			gameObject.name = owner.GetType().Name;
#if GLAZIER_PROXY_UGUI_PROFILING
			updateSampler = UnityEngine.Profiling.CustomSampler.Create(owner.GetType().Name + ".OnUpdate()");
#endif // GLAZIER_PROXY_UGUI_PROFILING
		}

		public override void Update()
		{
#if GLAZIER_PROXY_UGUI_PROFILING
			updateSampler.Begin();
#endif // GLAZIER_PROXY_UGUI_PROFILING
			owner.OnUpdate();
#if GLAZIER_PROXY_UGUI_PROFILING
			updateSampler.End();
#endif // GLAZIER_PROXY_UGUI_PROFILING
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

		protected override bool ReleaseIntoPool()
		{
			if (transform == null || gameObject == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.error("Transform or gameObject null when releasing GlazierProxy into uGUI pool!");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return false;
			}

			PoolData poolData = new PoolData();
			PopulateBasePoolData(poolData);
			glazier.ReleaseEmptyToPool(poolData);
			return true;
		}

		private SleekWrapper owner;
#if GLAZIER_PROXY_UGUI_PROFILING
		private UnityEngine.Profiling.CustomSampler updateSampler;
#endif // GLAZIER_PROXY_UGUI_PROFILING
	}
}
