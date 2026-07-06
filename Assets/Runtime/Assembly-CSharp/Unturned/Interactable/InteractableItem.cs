////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableItem : Interactable
	{
		public Item item;
		public ItemJar jar;
		public ItemAsset asset;

		private bool wasReset;

		public override void use()
		{
			ItemManager.takeItem(transform.parent, 255, 255, 0, 255);
		}

		public override bool checkHighlight(out Color color)
		{
			color = ItemTool.getRarityColorHighlight(asset.rarity);
			return true;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			message = EPlayerMessage.ITEM;
			text = asset.itemName;
			color = ItemTool.getRarityColorUI(asset.rarity);
			//if(asset.showQuality)
			//{
			//	color = ItemTool.getQualityColor(item.quality / 100.0f);
			//}
			//else
			//{
			//	color = Color.white;
			//}

			return true;
		}

		public void clampRange()
		{
			if (wasReset)
			{
				return;
			}

			if ((transform.position - transform.parent.position).sqrMagnitude > 400)
			{
				transform.position = transform.parent.position;
				wasReset = true;

				ItemManager.clampedItems.RemoveFast(this);
				Destroy(GetComponent<Rigidbody>());
			}
		}

		private void OnEnable()
		{
			ItemManager.clampedItems.Add(this);
		}

		private void OnDisable()
		{
			if (wasReset)
			{
				return;
			}

			ItemManager.clampedItems.RemoveFast(this);
		}
	}
}