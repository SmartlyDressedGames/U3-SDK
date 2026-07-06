////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableFuel : Useable
	{
		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool shouldDeleteAfterUse;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private ushort fuel;

		private void glug()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemFuelAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askGlug(CSteamID steamID)
		{
			ReceivePlayGlug();
		}

		private static readonly ClientInstanceMethod SendPlayGlug = ClientInstanceMethod.Get(typeof(UseableFuel), nameof(ReceivePlayGlug));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askGlug))]
		public void ReceivePlayGlug()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				glug();
			}
		}

		private bool fire(EUseMode mode)
		{
			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.vehicle != null)
				{
					if (!info.vehicle.checkEnter(player))
						return false;

					if (mode == EUseMode.Deposit)
					{
						if (fuel == 0)
						{
							return false;
						}

						if (!info.vehicle.isRefillable)
						{
							return false;
						}
					}
					else // take
					{
						if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
						{
							return false;
						}

						if (!info.vehicle.isSiphonable)
						{
							return false;
						}
					}
				}
				else if (info.transform != null)
				{
					InteractableGenerator generator = info.transform.GetComponent<InteractableGenerator>();
					InteractableOil oil = info.transform.GetComponent<InteractableOil>();
					InteractableTank tank = info.transform.GetComponent<InteractableTank>();
					InteractableObjectResource resource = info.transform.GetComponentInParent<InteractableObjectResource>();

					if (generator != null)
					{
						if (mode == EUseMode.Deposit)
						{
							if (fuel == 0)
							{
								return false;
							}

							if (!generator.isRefillable)
							{
								return false;
							}
						}
						else
						{
							if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
							{
								return false;
							}

							if (!generator.isSiphonable)
							{
								return false;
							}
						}
					}
					else if (oil != null)
					{
						//if(isFull)
						//{
						//	if(oil.isFull)
						//	{
						//		return false;
						//	}
						//}
						//else
						//{
						//	if(!oil.isFull)
						//	{
						//		return false;
						//	}
						//}
					}
					else if (tank != null)
					{
						if (tank.source != ETankSource.FUEL)
						{
							return false;
						}

						if (mode == EUseMode.Deposit)
						{
							if (fuel == 0)
							{
								return false;
							}

							if (!tank.isRefillable)
							{
								return false;
							}
						}
						else
						{
							if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
							{
								return false;
							}

							if (!tank.isSiphonable)
							{
								return false;
							}
						}
					}
					else if (resource != null)
					{
						if (resource.objectAsset.interactability != EObjectInteractability.FUEL || !resource.IsRubbleNullOrAllAlive)
						{
							return false;
						}

						if (mode == EUseMode.Deposit)
						{
							if (fuel == 0)
							{
								return false;
							}

							if (resource.amount == resource.capacity)
							{
								return false;
							}
						}
						else
						{
							if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
							{
								return false;
							}

							if (resource.amount == 0)
							{
								return false;
							}
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Fuel);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Fuel);

				if (info == null)
				{
					return false;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > 49)
				{
					return false;
				}

				if (info.type == ERaycastInfoType.VEHICLE)
				{
					if (info.vehicle == null)
					{
						return false;
					}

					if (!info.vehicle.checkEnter(player))
						return false;

					if (mode == EUseMode.Deposit) // pour
					{
						if (fuel == 0)
						{
							return false;
						}

						if (!info.vehicle.isRefillable)
						{
							return false;
						}

						ushort amount = (ushort) Mathf.Min(fuel, info.vehicle.asset.fuel - info.vehicle.fuel);
						info.vehicle.askFillFuel(amount);
						fuel -= amount;
					}
					else // take
					{
						if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
						{
							return false;
						}

						if (!info.vehicle.isSiphonable)
						{
							return false;
						}

						ushort capacity = ((ItemFuelAsset) player.equipment.asset).fuel;
						ushort desiredAmount = (ushort) (capacity - fuel);
						fuel += VehicleManager.siphonFromVehicle(info.vehicle, player, desiredAmount);
					}
				}
				else if (info.type == ERaycastInfoType.BARRICADE)
				{
					if (info.transform == null || !info.transform.CompareTag("Barricade"))
					{
						return false;
					}

					InteractableGenerator generator = info.transform.GetComponent<InteractableGenerator>();
					InteractableOil oil = info.transform.GetComponent<InteractableOil>();
					InteractableTank tank = info.transform.GetComponent<InteractableTank>();

					if (generator != null)
					{
						if (mode == EUseMode.Deposit)
						{
							if (fuel == 0)
							{
								return false;
							}

							if (!generator.isRefillable)
							{
								return false;
							}

							ushort amount = (ushort) Mathf.Min(fuel, generator.capacity - generator.fuel);
							generator.askFill(amount);
							BarricadeManager.sendFuel(info.transform, generator.fuel);
							fuel -= amount;
						}
						else
						{
							if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
							{
								return false;
							}

							if (!generator.isSiphonable)
							{
								return false;
							}

							ushort amount = (ushort) Mathf.Min(generator.fuel, ((ItemFuelAsset) player.equipment.asset).fuel - fuel);
							generator.askBurn(amount);
							BarricadeManager.sendFuel(info.transform, generator.fuel);
							fuel += amount;
						}
					}
					else if (oil != null)
					{
						if (mode == EUseMode.Deposit)
						{
							if (fuel == 0)
							{
								return false;
							}

							if (!oil.isRefillable)
							{
								return false;
							}

							ushort amount = (ushort) Mathf.Min(fuel, oil.capacity - oil.fuel);
							oil.askFill(amount);
							BarricadeManager.sendOil(info.transform, oil.fuel);
							fuel -= amount;
						}
						else
						{
							if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
							{
								return false;
							}

							if (!oil.isSiphonable)
							{
								return false;
							}

							ushort amount = (ushort) Mathf.Min(oil.fuel, ((ItemFuelAsset) player.equipment.asset).fuel - fuel);
							oil.askBurn(amount);
							BarricadeManager.sendOil(info.transform, oil.fuel);
							fuel += amount;
						}
					}
					else if (tank != null)
					{
						if (tank.source != ETankSource.FUEL)
						{
							return false;
						}

						if (mode == EUseMode.Deposit)
						{
							if (fuel == 0)
							{
								return false;
							}

							if (!tank.isRefillable)
							{
								return false;
							}

							ushort amount = (ushort) Mathf.Min(fuel, tank.capacity - tank.amount);
							tank.ServerSetAmount((ushort) (tank.amount + amount));
							fuel -= amount;
						}
						else
						{
							if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
							{
								return false;
							}

							if (!tank.isSiphonable)
							{
								return false;
							}

							ushort amount = (ushort) Mathf.Min(tank.amount, ((ItemFuelAsset) player.equipment.asset).fuel - fuel);
							tank.ServerSetAmount((ushort) (tank.amount - amount));
							fuel += amount;
						}
					}
					else
					{
						return false;
					}
				}
				else if (info.type == ERaycastInfoType.OBJECT)
				{
					if (info.transform == null)
					{
						return false;
					}

					InteractableObjectResource resource = info.transform.GetComponentInParent<InteractableObjectResource>();

					if (resource == null || resource.objectAsset.interactability != EObjectInteractability.FUEL || !resource.IsRubbleNullOrAllAlive)
					{
						return false;
					}

					if (mode == EUseMode.Deposit)
					{
						if (fuel == 0)
						{
							return false;
						}

						if (!resource.isRefillable)
						{
							return false;
						}

						ushort amount = (ushort) Mathf.Min(fuel, resource.capacity - resource.amount);
						ObjectManager.updateObjectResource(resource.transform, (ushort) (resource.amount + amount), true);
						fuel -= amount;
					}
					else
					{
						if (fuel == ((ItemFuelAsset) player.equipment.asset).fuel)
						{
							return false;
						}

						if (!resource.isSiphonable)
						{
							return false;
						}

						ushort amount = (ushort) Mathf.Min(resource.amount, ((ItemFuelAsset) player.equipment.asset).fuel - fuel);
						ObjectManager.updateObjectResource(resource.transform, (ushort) (resource.amount - amount), true);
						fuel += amount;
					}
				}
			}

			return true;
		}

		private bool start(EUseMode mode)
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (isUseable)
			{
				if (fire(mode))
				{
					if (Provider.isServer)
					{
						byte[] state = System.BitConverter.GetBytes(fuel);
						player.equipment.state[0] = state[0];
						player.equipment.state[1] = state[1];
						player.equipment.sendUpdateState();
					}

					player.equipment.isBusy = true;
					startedUse = Time.realtimeSinceStartup;
					isUsing = true;

					glug();

					if (Provider.isServer)
					{
						SendPlayGlug.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());

						ItemFuelAsset asset = player.equipment.asset as ItemFuelAsset;
						if (mode == EUseMode.Deposit && asset != null && asset.shouldDeleteAfterFillingTarget)
						{
							shouldDeleteAfterUse = true;
						}
					}

					return true;
				}
			}

			return false;
		}

		public override bool startPrimary()
		{
			return start(EUseMode.Deposit);
		}

		public override bool startSecondary()
		{
			return start(EUseMode.Withdraw);
		}

		public override void updateState(byte[] newState)
		{
			if (channel.IsLocalPlayer)
			{
				fuel = System.BitConverter.ToUInt16(newState, 0);

				PlayerUI.message(EPlayerMessage.FUEL, ((int) (fuel / (float) ((ItemFuelAsset) player.equipment.asset).fuel * 100)).ToString());
			}
		}

		public override void equip()
		{
			if (channel.IsLocalPlayer || Provider.isServer)
			{
				if (player.equipment.state.Length < 2)
				{
					player.equipment.state = ((ItemFuelAsset) player.equipment.asset).getState(EItemOrigin.ADMIN);
					player.equipment.updateState();
				}

				fuel = System.BitConverter.ToUInt16(player.equipment.state, 0);
			}

			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");

			if (channel.IsLocalPlayer)
			{
				PlayerUI.message(EPlayerMessage.FUEL, ((int) (fuel / (float) ((ItemFuelAsset) player.equipment.asset).fuel * 100)).ToString());
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (Provider.isServer && shouldDeleteAfterUse)
				{
					player.equipment.use();
				}
			}
		}

		private enum EUseMode
		{
			/// <summary>
			/// Add fuel to target.
			/// </summary>
			Deposit,

			/// <summary>
			/// Remove fuel from target.
			/// </summary>
			Withdraw,
		}
	}
}
