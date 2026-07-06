////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class GlazierEmpty_uGUI : GlazierElementBase_uGUI
	{
		public GlazierEmpty_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		protected override bool ReleaseIntoPool()
		{
			if (transform == null || gameObject == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.error("Transform or gameObject null when releasing GlazierEmpty into uGUI pool!");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return false;
			}

			PoolData poolData = new PoolData();
			PopulateBasePoolData(poolData);
			glazier.ReleaseEmptyToPool(poolData);
			return true;
		}
	}
}
