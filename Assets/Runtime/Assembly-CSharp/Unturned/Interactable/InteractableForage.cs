////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableForage : Interactable
	{
		public override void use()
		{
			ResourceManager.forage(transform.parent);
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (asset != null && !string.IsNullOrEmpty(asset.interactabilityText))
			{
				message = EPlayerMessage.INTERACT;
				text = asset.interactabilityText;
			}
			else
			{
				message = EPlayerMessage.FORAGE;
				text = string.Empty;
			}

			color = Color.white;
			return true;
		}

		internal ResourceAsset asset;
	}
}
