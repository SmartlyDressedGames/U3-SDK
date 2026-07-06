////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableFilter : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private void filter()
		{
			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemFilterAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askFilter(CSteamID steamID)
		{
			ReceivePlayFilter();
		}

		private static readonly ClientInstanceMethod SendPlayFilter = ClientInstanceMethod.Get(typeof(UseableFilter), nameof(ReceivePlayFilter));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askFilter))]
		public void ReceivePlayFilter()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				filter();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (player.clothing.maskAsset == null || !player.clothing.maskAsset.proofRadiation || player.clothing.maskQuality == 100)
			{
				return false;
			}

			player.equipment.isBusy = true;
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			if (Provider.isServer)
			{
				SendPlayFilter.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
			}

			filter();
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
					if (player.clothing.maskAsset != null && player.clothing.maskAsset.proofRadiation && player.clothing.maskQuality < 100)
					{
						player.equipment.use();

						player.clothing.maskQuality = 100;
						player.clothing.sendUpdateMaskQuality();
					}
					else
					{
						player.equipment.dequip();
					}
				}
			}
		}
	}
}
