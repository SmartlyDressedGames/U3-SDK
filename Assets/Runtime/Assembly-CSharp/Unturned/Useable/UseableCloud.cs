////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class UseableCloud : Useable
	{
		public override void equip()
		{
			player.animator.play("Equip", true);
		}

		public override void dequip()
		{
			player.movement.itemGravityMultiplier = 1;
		}

		public override void tick()
		{
			if (!player.equipment.IsEquipAnimationFinished)
			{
				// Wait until animation finishes to introduce a slight delay, preventing spam equip/dequip.
				return;
			}

			player.movement.itemGravityMultiplier = ((ItemCloudAsset) player.equipment.asset).gravity;
		}
	}
}
