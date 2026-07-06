////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableCarjack : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool isJacking;
		private InteractableVehicle vehicle;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isJackable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

		private void pull()
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
		public void askPull(CSteamID steamID)
		{
			ReceivePlayPull();
		}

		private static readonly ClientInstanceMethod SendPlayPull = ClientInstanceMethod.Get(typeof(UseableCarjack), nameof(ReceivePlayPull));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askPull))]
		public void ReceivePlayPull()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				pull();
			}
		}

		private bool isVehicleValid(InteractableVehicle testVehicle)
		{
			if (!testVehicle.isEmpty)
			{
				// Cannot override the physics for driven vehicles, and it might be abused if passengers could be
				// launched into space along with the vehicle.
				return false;
			}

			if (channel.owner.isAdmin)
			{
				// Admins are allowed to carjack any vehicle.
				return true;
			}
			else if (player.movement.isSafe && player.movement.isSafeInfo != null && player.movement.isSafeInfo.noWeapons)
			{
				// In safezones (e.g. liberator) players are only allowed to carjack their own vehicle.
				return testVehicle.checkEnter(player);
			}

			return true;
		}

		private bool fire()
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.vehicle == null || !isVehicleValid(info.vehicle))
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Carjack);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Carjack);

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

				if (info.vehicle == null || !isVehicleValid(info.vehicle))
				{
					return false;
				}

				isJacking = true;
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

					pull();

					if (Provider.isServer)
					{
						SendPlayPull.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
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
			if (isJacking && isJackable)
			{
				isJacking = false;

				if (vehicle != null && isVehicleValid(vehicle))
				{
					Vector3 force = new Vector3(Random.Range(-32f, 32f), Random.Range(480f, 544f) * (player.skills.boost == EPlayerBoost.FLIGHT ? 4 : 1), Random.Range(-32f, 32f));
					Vector3 torque = new Vector3(Random.Range(-64f, 64f), Random.Range(-64f, 64f), Random.Range(-64f, 64f));
					VehicleManager.carjackVehicle(vehicle, player, force, torque);
					vehicle = null;
				}
			}

			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;
			}
		}
	}
}
