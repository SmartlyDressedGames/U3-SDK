////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableVehicleBattery : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool isReplacing;
		private InteractableVehicle vehicle;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isReplaceable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

		private void replace()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemToolAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askReplace(CSteamID steamID)
		{
			ReceivePlayReplace();
		}

		private static readonly ClientInstanceMethod SendPlayReplace = ClientInstanceMethod.Get(typeof(UseableVehicleBattery), nameof(ReceivePlayReplace));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askReplace))]
		public void ReceivePlayReplace()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				replace();
			}
		}

		private bool fire()
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.vehicle == null || !info.vehicle.isBatteryReplaceable)
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Battery);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Battery);

				if (info == null)
				{
					return false;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > 49)
				{
					return false;
				}

				if (info.type != ERaycastInfoType.VEHICLE)
				{
					return false;
				}

				if (info.vehicle == null || !info.vehicle.isBatteryReplaceable)
				{
					return false;
				}

				isReplacing = true;
				vehicle = info.vehicle;
			}

			return true;
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (isUseable)
			{
				if (fire())
				{
					player.equipment.isBusy = true;
					startedUse = Time.realtimeSinceStartup;
					isUsing = true;

					replace();

					if (Provider.isServer)
					{
						SendPlayReplace.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
					}

					return true;
				}
			}

			return false;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isReplacing && isReplaceable)
			{
				isReplacing = false;

				if (vehicle != null && vehicle.isBatteryReplaceable)
				{
					vehicle.replaceBattery(player, player.equipment.quality, player.equipment.asset.GUID);
					wasSuccessfullyUsed = true;
					vehicle = null;
				}

				if (Provider.isServer)
				{
					if (wasSuccessfullyUsed)
					{
						player.equipment.useStepA();
					}
					else
					{
						player.equipment.dequip();
					}
				}
			}

			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (Provider.isServer && wasSuccessfullyUsed)
				{
					player.equipment.useStepB();
				}
			}
		}

		private bool wasSuccessfullyUsed = false;
	}
}
