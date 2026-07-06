////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableClothing : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private void wear()
		{
			player.animator.play("Use", false);

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askWear(CSteamID steamID)
		{
			ReceivePlayWear();
		}

		private static readonly ClientInstanceMethod SendPlayWear = ClientInstanceMethod.Get(typeof(UseableClothing), nameof(ReceivePlayWear));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askWear))]
		public void ReceivePlayWear()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				wear();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			player.equipment.isBusy = true;
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			if (Provider.isServer)
			{
				SendPlayWear.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
			}

			wear();
			return true;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (Provider.isServer)
				{
					ItemAsset asset = player.equipment.asset;
					EItemType type = asset.type;
					byte quality = player.equipment.quality;
					byte[] state = player.equipment.state;

					player.equipment.use();

					if (type == EItemType.HAT)
					{
						player.clothing.askWearHat(asset as ItemHatAsset, quality, state, true);
					}
					else if (type == EItemType.SHIRT)
					{
						player.clothing.askWearShirt(asset as ItemShirtAsset, quality, state, true);
					}
					else if (type == EItemType.PANTS)
					{
						player.clothing.askWearPants(asset as ItemPantsAsset, quality, state, true);
					}
					else if (type == EItemType.BACKPACK)
					{
						player.clothing.askWearBackpack(asset as ItemBackpackAsset, quality, state, true);
					}
					else if (type == EItemType.VEST)
					{
						player.clothing.askWearVest(asset as ItemVestAsset, quality, state, true);
					}
					else if (type == EItemType.MASK)
					{
						player.clothing.askWearMask(asset as ItemMaskAsset, quality, state, true);
					}
					else if (type == EItemType.GLASSES)
					{
						player.clothing.askWearGlasses(asset as ItemGlassesAsset, quality, state, true);
					}
				}
			}
		}
	}
}
