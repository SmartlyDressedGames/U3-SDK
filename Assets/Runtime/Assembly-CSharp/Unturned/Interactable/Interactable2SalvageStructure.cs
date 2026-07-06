////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class Interactable2SalvageStructure : Interactable2
	{
		public Interactable2HP hp;

		public override bool checkHint(out EPlayerMessage message, out float data)
		{
			message = EPlayerMessage.SALVAGE;

			if (hp != null)
			{
				data = hp.hp / 100.0f;
			}
			else
			{
				data = 0.0f;
			}

			if (!hasOwnership)
			{
				return false;
			}

			return true;
		}

		public override void use()
		{
			StructureManager.salvageStructure(transform);
		}
	}
}