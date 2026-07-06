////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ImpactGrenade : TriggerGrenadeBase
	{
		public IExplodableThrowable explodable;

		protected override void GrenadeTriggered()
		{
			base.GrenadeTriggered();

			if (explodable == null)
			{
				UnturnedLog.warn("Missing explodable", this);
				return;
			}

			explodable.Explode();
		}
	}
}
