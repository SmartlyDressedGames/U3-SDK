////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class UseableWalkieTalkie : Useable
	{
		public override void equip()
		{
			player.animator.play("Equip", true);

			player.voice.hasUseableWalkieTalkie = true;
		}

		public override void dequip()
		{
			player.voice.hasUseableWalkieTalkie = false;
		}
	}
}
