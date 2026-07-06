////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableObjectNote : InteractableObjectTriggerableBase
	{
		public override void use()
		{
			PlayerBarricadeSignUI.open(objectAsset.interactabilityText);

			PlayerLifeUI.close();

			ObjectManager.useObjectQuest(transform);
		}

		public override bool checkUseable()
		{
			return !PlayerUI.window.showCursor;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			switch (objectAsset.interactabilityHint)
			{
				case EObjectInteractabilityHint.USE:
					message = EPlayerMessage.USE;
					break;
				default:
					message = EPlayerMessage.NONE;
					break;
			}

			text = "";
			color = Color.white;
			return !PlayerUI.window.showCursor;
		}
	}
}
