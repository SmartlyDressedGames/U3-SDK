////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableCarlockpick : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool isUnlocking;
		/// <summary>
		/// If true, unlocking has failed.
		/// </summary>
		private bool isUnlockingFailure;
		private InteractableVehicle vehicle;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isUnlockable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

		private void jimmy()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			if (isUnlockingFailure && player.animator.checkExists("Use_Failure"))
			{
				player.animator.play("Use_Failure", false);
			}
			else
			{
				player.animator.play("Use", false);
			}

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
		public void askJimmy(CSteamID steamID)
		{
			ReceivePlayJimmy(false);
		}

		private static readonly ClientInstanceMethod<bool> SendPlayJimmy = ClientInstanceMethod<bool>.Get(typeof(UseableCarlockpick), nameof(ReceivePlayJimmy));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askJimmy))]
		public void ReceivePlayJimmy(bool isFailure)
		{
			isUnlockingFailure = isFailure;
			if (player.equipment.IsEquipAnimationFinished)
			{
				jimmy();
			}
		}

		private bool fire()
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.vehicle == null || !info.vehicle.isEmpty || !info.vehicle.isLocked)
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Carlockpick);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Carlockpick);

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

				if (info.vehicle == null || !info.vehicle.isEmpty || !info.vehicle.isLocked)
				{
					return false;
				}

				isUnlocking = true;
				if (player.equipment.asset is ItemVehicleLockpickToolAsset asset && asset.CanFail)
				{
					isUnlockingFailure = Random.value <= asset.FailureProbability;
				}
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

					jimmy();

					if (Provider.isServer)
					{
						player.life.markAggressive(true);

						SendPlayJimmy.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(), isUnlockingFailure);
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
			if (isUnlocking && isUnlockable)
			{
				isUnlocking = false;

				if (vehicle != null && vehicle.isEmpty && vehicle.isLocked)
				{
					if (isUnlockingFailure)
					{
						if (player.equipment.asset is ItemVehicleLockpickToolAsset asset)
						{
							EffectAsset effect = asset.FindFailureEffect();
							if (effect != null)
							{
								TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(effect);
								triggerEffectParameters.position = transform.position;
								triggerEffectParameters.reliable = true;
								EffectManager.triggerEffect(triggerEffectParameters);
							}
						}
					}
					else
					{
						VehicleManager.unlockVehicle(vehicle, player);
					}
					shouldConsumeItem = isUnlockingFailure || !vehicle.isLocked;
					vehicle = null;
				}

				if (Provider.isServer)
				{
					if (shouldConsumeItem)
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

				if (Provider.isServer && shouldConsumeItem)
				{
					player.equipment.useStepB();
				}
			}
		}

		private bool shouldConsumeItem = false;
	}
}
