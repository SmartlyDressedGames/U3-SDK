////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableArrestEnd : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;

		private Player enemy;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private void arrest()
		{
			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemArrestEndAsset) player.equipment.asset).use);
			}
		}

		[System.Obsolete]
		public void askArrest(CSteamID steamID)
		{
			ReceivePlayArrest();
		}

		private static readonly ClientInstanceMethod SendPlayArrest = ClientInstanceMethod.Get(typeof(UseableArrestEnd), nameof(ReceivePlayArrest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askArrest))]
		public void ReceivePlayArrest()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				arrest();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.player != null && info.player.animator.gesture == EPlayerGesture.ARREST_START)
				{
					player.input.sendRaycast(info, ERaycastInfoUsage.ArrestEnd);

					if (!Provider.isServer)
					{
						player.equipment.isBusy = true;
						startedUse = Time.realtimeSinceStartup;
						isUsing = true;

						arrest();
					}
				}
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.ArrestEnd);

				if (info == null)
				{
					return false;
				}

				if (info.type == ERaycastInfoType.PLAYER)
				{
					if (info.player != null)
					{
						enemy = info.player;

						player.equipment.isBusy = true;
						startedUse = Time.realtimeSinceStartup;
						isUsing = true;

						arrest();

						SendPlayArrest.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
					}
				}
			}

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
					if (enemy != null && enemy.animator.gesture == EPlayerGesture.ARREST_START && enemy.animator.captorID == channel.owner.playerID.steamID && enemy.animator.captorItem == ((ItemArrestEndAsset) player.equipment.asset).recover)
					{
						enemy.animator.captorID = CSteamID.Nil;
						enemy.animator.captorStrength = 0;
						enemy.animator.sendGesture(EPlayerGesture.ARREST_STOP, true);
						player.inventory.forceAddItem(new Item(((ItemArrestEndAsset) player.equipment.asset).recover, EItemOrigin.NATURE), false);
					}

					player.equipment.dequip();
				}
			}
		}
	}
}
