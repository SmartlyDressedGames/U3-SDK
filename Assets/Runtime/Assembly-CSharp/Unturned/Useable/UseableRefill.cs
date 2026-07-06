////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableRefill : Useable
	{
		private float startedUse;
		private float useTime;
		private float refillTime;

		private bool isUsing;
		private ERefillMode refillMode;
		private ERefillWaterType refillWaterType;

		private bool isUseable
		{
			get
			{
				if (refillMode == ERefillMode.USE)
				{
					return Time.realtimeSinceStartup - startedUse > useTime;
				}
				else if (refillMode == ERefillMode.REFILL)
				{
					return Time.realtimeSinceStartup - startedUse > refillTime;
				}

				return false;
			}
		}

		private ERefillWaterType waterType
		{
			get
			{
				if (player.equipment.state != null && player.equipment.state.Length > 0)
				{
					return (ERefillWaterType) player.equipment.state[0];
				}
				else
				{
					return ERefillWaterType.EMPTY;
				}
			}
		}

		private void use()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemRefillAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askUse(CSteamID steamID)
		{
			ReceivePlayUse();
		}

		private static readonly ClientInstanceMethod SendPlayUse = ClientInstanceMethod.Get(typeof(UseableRefill), nameof(ReceivePlayUse));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askUse))]
		public void ReceivePlayUse()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				use();
			}
		}

		private void refill()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Refill", false);
		}

		[System.Obsolete]
		public void askRefill(CSteamID steamID)
		{
			ReceivePlayRefill();
		}

		private static readonly ClientInstanceMethod SendPlayRefill = ClientInstanceMethod.Get(typeof(UseableRefill), nameof(ReceivePlayRefill));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askRefill))]
		public void ReceivePlayRefill()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				refill();
			}
		}

		// mode is true if depositing, false is withdrawing
		private bool fire(bool mode, out ERefillWaterType newWaterType)
		{
			newWaterType = ERefillWaterType.EMPTY;

			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				if (info.transform != null)
				{
					InteractableRainBarrel barrel = info.transform.GetComponent<InteractableRainBarrel>();
					InteractableTank tank = info.transform.GetComponent<InteractableTank>();
					InteractableObjectResource resource = info.transform.GetComponentInParent<InteractableObjectResource>();

					SDG.Framework.Water.WaterVolume volume;
					if (SDG.Framework.Water.WaterUtility.isPointUnderwater(info.point, out volume))
					{
						if (mode) // trying to pour into ocean
						{
							return false;
						}

						if (waterType != ERefillWaterType.EMPTY)
						{
							return false;
						}

						if (volume == null)
						{
							newWaterType = ERefillWaterType.SALTY;
						}
						else
						{
							newWaterType = volume.waterType;
						}
					}
					else if (barrel != null)
					{
						if (mode) // pour
						{
							if (waterType != ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (barrel.isFull)
							{
								return false;
							}

							newWaterType = ERefillWaterType.EMPTY;
						}
						else // take
						{
							if (waterType == ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (!barrel.isFull)
							{
								return false;
							}

							newWaterType = ERefillWaterType.CLEAN;
						}
					}
					else if (tank != null)
					{
						if (tank.source != ETankSource.WATER)
						{
							return false;
						}

						if (mode) // pour
						{
							if (waterType != ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (tank.amount == tank.capacity)
							{
								return false;
							}

							newWaterType = ERefillWaterType.EMPTY;
						}
						else // take
						{
							if (waterType == ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (tank.amount == 0)
							{
								return false;
							}

							newWaterType = ERefillWaterType.CLEAN;
						}
					}
					else if (resource != null)
					{
						if (resource.objectAsset.interactability != EObjectInteractability.WATER || !resource.IsRubbleNullOrAllAlive)
						{
							return false;
						}

						if (mode) // pour
						{
							if (waterType == ERefillWaterType.EMPTY)
							{
								return false;
							}

							if (resource.amount == resource.capacity)
							{
								return false;
							}

							newWaterType = ERefillWaterType.EMPTY;
						}
						else // take
						{
							if (waterType == ERefillWaterType.CLEAN || waterType == ERefillWaterType.DIRTY)
							{
								return false;
							}

							if (resource.amount == 0)
							{
								return false;
							}

							newWaterType = ERefillWaterType.DIRTY;
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

				player.input.sendRaycast(info, ERaycastInfoUsage.Refill);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Refill);

				if (info == null)
				{
					return false;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > 49)
				{
					return false;
				}

				SDG.Framework.Water.WaterVolume volume;
				if (SDG.Framework.Water.WaterUtility.isPointUnderwater(info.point, out volume))
				{
					if (mode)
					{
						return false;
					}

					if (waterType != ERefillWaterType.EMPTY)
					{
						return false;
					}

					if (volume == null)
					{
						newWaterType = ERefillWaterType.SALTY;
					}
					else
					{
						newWaterType = volume.waterType;
					}
				}
				else if (info.type == ERaycastInfoType.BARRICADE)
				{
					if (info.transform == null || !info.transform.CompareTag("Barricade"))
					{
						return false;
					}

					InteractableRainBarrel barrel = info.transform.GetComponent<InteractableRainBarrel>();
					InteractableTank tank = info.transform.GetComponent<InteractableTank>();

					if (barrel != null)
					{
						if (mode) // pour
						{
							if (waterType != ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (barrel.isFull)
							{
								return false;
							}

							BarricadeManager.updateRainBarrel(barrel.transform, true, true);
							newWaterType = ERefillWaterType.EMPTY;
						}
						else // take
						{
							if (waterType == ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (!barrel.isFull)
							{
								return false;
							}

							BarricadeManager.updateRainBarrel(barrel.transform, false, true);
							newWaterType = ERefillWaterType.CLEAN;
						}
					}
					else if (tank != null)
					{
						if (tank.source != ETankSource.WATER)
						{
							return false;
						}

						if (mode) // pour
						{
							if (waterType != ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (tank.amount == tank.capacity)
							{
								return false;
							}

							tank.ServerSetAmount((ushort) (tank.amount + 1));
							newWaterType = ERefillWaterType.EMPTY;
						}
						else // take
						{
							if (waterType == ERefillWaterType.CLEAN)
							{
								return false;
							}

							if (tank.amount == 0)
							{
								return false;
							}

							tank.ServerSetAmount((ushort) (tank.amount - 1));
							newWaterType = ERefillWaterType.CLEAN;
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

					if (resource == null || resource.objectAsset.interactability != EObjectInteractability.WATER || !resource.IsRubbleNullOrAllAlive)
					{
						return false;
					}

					if (mode) // pour
					{
						if (waterType == ERefillWaterType.EMPTY)
						{
							return false;
						}

						if (resource.amount == resource.capacity)
						{
							return false;
						}

						ObjectManager.updateObjectResource(resource.transform, (byte) (resource.amount + 1), true);
						newWaterType = ERefillWaterType.EMPTY;
					}
					else // take
					{
						if (waterType == ERefillWaterType.CLEAN || waterType == ERefillWaterType.DIRTY)
						{
							return false;
						}

						if (resource.amount == 0)
						{
							return false;
						}

						ObjectManager.updateObjectResource(resource.transform, (byte) (resource.amount - 1), true);
						newWaterType = ERefillWaterType.DIRTY;
					}
				}
			}

			return true;
		}

		private void msg()
		{
			EPlayerMessage message;

			switch (waterType)
			{
				case ERefillWaterType.EMPTY:
					message = EPlayerMessage.EMPTY;
					break;
				case ERefillWaterType.CLEAN:
					message = EPlayerMessage.CLEAN;
					break;
				case ERefillWaterType.SALTY:
					message = EPlayerMessage.SALTY;
					break;
				case ERefillWaterType.DIRTY:
					message = EPlayerMessage.DIRTY;
					break;
				default:
					message = EPlayerMessage.FULL;
					break;
			}

			PlayerUI.message(message, "");
		}

		private void start(ERefillWaterType newWaterType)
		{
			player.equipment.isBusy = true;
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			refillMode = ERefillMode.REFILL;
			refill();

			player.equipment.quality = (byte) (newWaterType == ERefillWaterType.DIRTY ? 0 : 100);
			player.equipment.updateQuality();

			player.equipment.state[0] = (byte) newWaterType;
			player.equipment.updateState();

			if (Provider.isServer)
			{
				SendPlayRefill.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
			}

			if (channel.IsLocalPlayer)
			{
				msg();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (isUseable)
			{
				ERefillWaterType newWaterType;
				if (fire(true, out newWaterType))
				{
					start(newWaterType);
				}
				else if (waterType != ERefillWaterType.EMPTY)
				{
					player.equipment.isBusy = true;
					startedUse = Time.realtimeSinceStartup;
					isUsing = true;

					refillMode = ERefillMode.USE;
					refillWaterType = waterType;
					use();

					player.equipment.quality = (byte) (newWaterType == ERefillWaterType.DIRTY ? 0 : 100);
					player.equipment.updateQuality();

					player.equipment.state[0] = (byte) ERefillWaterType.EMPTY;
					player.equipment.updateState();

					if (Provider.isServer)
					{
						SendPlayUse.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
					}

					if (channel.IsLocalPlayer)
					{
						msg();
					}
				}

				return true;
			}

			return false;
		}

		public override bool startSecondary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (isUseable)
			{
				ERefillWaterType newWaterType;
				if (fire(false, out newWaterType))
				{
					start(newWaterType);
					return true;
				}
			}

			return false;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");
			refillTime = player.animator.GetAnimationLength("Refill");

			if (channel.IsLocalPlayer)
			{
				msg();
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (refillMode == ERefillMode.USE)
				{
					ItemRefillAsset refillAsset = player.equipment.asset as ItemRefillAsset;

					// Owner and Server
					{
						float stamina;
						float oxygen;

						switch (refillWaterType)
						{
							case ERefillWaterType.CLEAN:
								stamina = refillAsset.cleanStamina;
								oxygen = refillAsset.cleanOxygen;
								break;
							case ERefillWaterType.SALTY:
								stamina = refillAsset.saltyStamina;
								oxygen = refillAsset.saltyOxygen;
								break;
							case ERefillWaterType.DIRTY:
								stamina = refillAsset.dirtyStamina;
								oxygen = refillAsset.dirtyOxygen;
								break;
							default:
								stamina = 0.0f;
								oxygen = 0.0f;
								break;
						}

						player.life.simulatedModifyStamina(stamina);
						player.life.simulatedModifyOxygen(oxygen);
					}

					if (Provider.isServer)
					{
						float health;
						float food;
						float water;
						float virus;

						switch (refillWaterType)
						{
							case ERefillWaterType.CLEAN:
								health = refillAsset.cleanHealth;
								food = refillAsset.cleanFood;
								water = refillAsset.cleanWater;
								virus = refillAsset.cleanVirus;
								break;
							case ERefillWaterType.SALTY:
								health = refillAsset.saltyHealth;
								food = refillAsset.saltyFood;
								water = refillAsset.saltyWater;
								virus = refillAsset.saltyVirus;
								break;
							case ERefillWaterType.DIRTY:
								health = refillAsset.dirtyHealth;
								food = refillAsset.dirtyFood;
								water = refillAsset.dirtyWater;
								virus = refillAsset.dirtyVirus;
								break;
							default:
								health = 0.0f;
								food = 0.0f;
								water = 0.0f;
								virus = 0.0f;
								break;
						}

						player.life.serverModifyHealth(health);
						player.life.serverModifyFood(food);
						player.life.serverModifyWater(water);
						player.life.serverModifyVirus(virus);
					}
				}
			}
		}
	}
}
