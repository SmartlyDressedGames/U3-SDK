////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableGrower : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;

		private InteractableFarm farm;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private void grow()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemGrowerAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askGrow(CSteamID steamID)
		{
			ReceivePlayGrow();
		}

		private static readonly ClientInstanceMethod SendPlayGrow = ClientInstanceMethod.Get(typeof(UseableGrower), nameof(ReceivePlayGrow));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askGrow))]
		public void ReceivePlayGrow()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				grow();
			}
		}

		private bool fire()
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.transform == null || !info.transform.CompareTag("Barricade"))
				{
					return false;
				}

				InteractableFarm farm = info.transform.GetComponent<InteractableFarm>();

				if (farm == null)
				{
					return false;
				}

				if (!farm.canFertilize)
				{
					return false;
				}

				if (farm.IsFullyGrown)
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Grower);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Grower);

				if (info == null)
				{
					return false;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > 49)
				{
					return false;
				}

				if (info.type != ERaycastInfoType.BARRICADE)
				{
					return false;
				}

				if (info.transform == null || !info.transform.CompareTag("Barricade"))
				{
					return false;
				}

				farm = info.transform.GetComponent<InteractableFarm>();

				if (farm == null)
				{
					return false;
				}

				if (!farm.canFertilize)
				{
					return false;
				}

				if (farm.IsFullyGrown)
				{
					return false;
				}
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

					grow();

					if (Provider.isServer)
					{
						SendPlayGrow.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
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
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (Provider.isServer)
				{
					if (farm != null && farm.canFertilize && !farm.IsFullyGrown)
					{
						BarricadeManager.updateFarm(farm.transform, 1, true);

						player.equipment.use();
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
