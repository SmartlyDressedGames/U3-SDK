////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableTire : Useable
	{
		public delegate void ModifyTireRequestHandler(UseableTire useable, InteractableVehicle vehicle, int wheelIndex, ref bool shouldAllow);
		public static event ModifyTireRequestHandler onModifyTireRequested;

		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool isAttaching;
		private InteractableVehicle vehicle;
		private int vehicleWheelIndex = -1;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isAttachable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

		private void attach()
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
		public void askAttach(CSteamID steamID)
		{
			ReceivePlayAttach();
		}

		private static readonly ClientInstanceMethod SendPlayAttach = ClientInstanceMethod.Get(typeof(UseableTire), nameof(ReceivePlayAttach));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askAttach))]
		public void ReceivePlayAttach()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				attach();
			}
		}

		private bool fire()
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.vehicle == null || !info.vehicle.isTireReplaceable)
				{
					return false;
				}

				if (((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.ADD)
				{
					if (!info.vehicle.isTireCompatible(player.equipment.itemID))
					{
						return false;
					}
				}

				int wheelIndex = info.vehicle.getClosestAliveTireIndex(info.point, ((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.REMOVE);
				if (wheelIndex == -1)
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Tire);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Tire);

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

				if (info.vehicle == null || !info.vehicle.isTireReplaceable)
				{
					return false;
				}

				if (((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.ADD)
				{
					if (!info.vehicle.isTireCompatible(player.equipment.itemID))
					{
						return false;
					}
				}

				int wheelIndex = info.vehicle.getClosestAliveTireIndex(info.point, ((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.REMOVE);
				if (wheelIndex == -1)
				{
					return false;
				}

				if (onModifyTireRequested != null)
				{
					bool shouldAllow = true;
					onModifyTireRequested(this, info.vehicle, wheelIndex, ref shouldAllow);
					if (shouldAllow == false)
					{
						return false;
					}
				}

				isAttaching = true;
				vehicle = info.vehicle;
				vehicleWheelIndex = wheelIndex;
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

					attach();

					if (Provider.isServer)
					{
						SendPlayAttach.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
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
			if (isAttaching && isAttachable)
			{
				isAttaching = false;

				if (vehicle != null && vehicle.isTireReplaceable && vehicleWheelIndex != -1)
				{
					if (((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.ADD)
					{
						if (!vehicle.tires[vehicleWheelIndex].isAlive)
						{
							vehicle.askRepairTire(vehicleWheelIndex);
							wasSuccessfullyUsed = vehicle.tires[vehicleWheelIndex].isAlive;
						}
					}
					else
					{
						if (vehicle.tires[vehicleWheelIndex].isAlive)
						{
							vehicle.askDamageTire(vehicleWheelIndex);

							// Only spawn the tire item if it was successfully removed.
							// Safe zone or plugin may have prevented it.
							if (!vehicle.tires[vehicleWheelIndex].isAlive)
							{
								player.inventory.forceAddItem(new Item(vehicle.asset.tireID, true), false);
							}
						}
					}

					vehicle = null;
				}

				if (((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.ADD)
				{
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
			}

			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (((ItemTireAsset) player.equipment.asset).mode == EUseableTireMode.ADD)
				{
					if (Provider.isServer && wasSuccessfullyUsed)
					{
						player.equipment.useStepB();
					}
				}
			}
		}

		private bool wasSuccessfullyUsed = false;
	}
}
