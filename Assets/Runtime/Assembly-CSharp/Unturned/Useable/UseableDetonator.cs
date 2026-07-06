////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableDetonator : Useable
	{
		private float startedUse;
		private float useTime;
		private uint lastExplosion;

		private bool isUsing;
		private bool isDetonating;

		private Vector3 chargePoint;
		private List<InteractableCharge> foundInRadius;
		private List<InteractableCharge> chargesInRadius;
		private List<InteractableCharge> charges;
		private InteractableCharge target;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isDetonatable => Time.realtimeSinceStartup - startedUse > useTime * 0.33f;

		private void plunge()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemDetonatorAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askPlunge(CSteamID steamID)
		{
			ReceivePlayPlunge();
		}

		private static readonly ClientInstanceMethod SendPlayPlunge = ClientInstanceMethod.Get(typeof(UseableDetonator), nameof(ReceivePlayPlunge));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askPlunge))]
		public void ReceivePlayPlunge()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				plunge();
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
				if (channel.IsLocalPlayer)
				{
					for (int index = 0; index < charges.Count; index++)
					{
						InteractableCharge charge = charges[index];

						if (charge == null)
						{
							continue;
						}

						RaycastInfo info = new RaycastInfo(charge.transform);
						player.input.sendRaycast(info, ERaycastInfoUsage.Detonator);
					}

					charges.Clear();
				}

				if (Provider.isServer)
				{
					charges.Clear();

					if (player.input.hasInputs())
					{
						int count = player.input.getInputCount();
						for (int index = 0; index < count; index++)
						{
							InputInfo info = player.input.getInput(false, ERaycastInfoUsage.Detonator);

							if (info == null)
							{
								continue;
							}

							if (info.type != ERaycastInfoType.BARRICADE)
							{
								continue;
							}

							if (info.transform == null || !info.transform.CompareTag("Barricade"))
							{
								continue;
							}

							InteractableCharge charge = info.transform.GetComponent<InteractableCharge>();

							if (charge == null)
							{
								continue;
							}

							if (Dedicator.IsDedicatedServer ? !OwnershipTool.checkToggle(channel.owner.playerID.steamID, charge.owner, player.quests.groupID, charge.group) : !charge.hasOwnership)
							{
								continue;
							}

							charges.Add(charge);
						}
					}
				}

				player.equipment.isBusy = true;
				startedUse = Time.realtimeSinceStartup;
				isUsing = true;
				isDetonating = true;

				plunge();

				if (Provider.isServer)
				{
					player.life.markAggressive(false);

					SendPlayPlunge.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
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

			if (channel.IsLocalPlayer && !isUsing)
			{
				if (target != null)
				{
					if (target.isSelected)
					{
						target.deselect();
						charges.Remove(target);
					}
					else
					{
						target.select();
						charges.Add(target);

						if (charges.Count > PlayerInputPacket.MAX_CLIENTSIDE_INPUTS)
						{
							if (charges[0] != null)
							{
								charges[0].deselect();
							}

							charges.RemoveAt(0);
						}
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

			if (channel.IsLocalPlayer)
			{
				chargePoint = Vector3.zero;
				foundInRadius = new List<InteractableCharge>();
				chargesInRadius = new List<InteractableCharge>();
			}

			charges = new List<InteractableCharge>();
		}

		public override void dequip()
		{
			if (channel.IsLocalPlayer)
			{
				for (int index = 0; index < chargesInRadius.Count; index++)
				{
					InteractableCharge charge = chargesInRadius[index];
					charge.unhighlight();
				}
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isDetonating && isDetonatable)
			{
				if (charges.Count > 0)
				{
					if (simulation - lastExplosion > 1)
					{
						lastExplosion = simulation;

						InteractableCharge charge = charges[0];
						charges.RemoveAt(0);

						if (charge != null)
						{
							charge.Detonate(player);
						}
					}
				}
				else
				{
					isDetonating = false;
				}
			}

			if (isUsing && isUseable && charges.Count == 0)
			{
				player.equipment.isBusy = false;
				isUsing = false;
			}
		}

		public override void tick()
		{
			if (channel.IsLocalPlayer)
			{
				if ((transform.position - chargePoint).sqrMagnitude > 1.0f)
				{
					chargePoint = transform.position;

					foundInRadius.Clear();
					PowerTool.checkInteractables(chargePoint, 64.0f, foundInRadius);

					// Remove new ones
					for (int index = chargesInRadius.Count - 1; index >= 0; index--)
					{
						InteractableCharge charge = chargesInRadius[index];

						if (charge == null)
						{
							chargesInRadius.RemoveAtFast(index);
							continue;
						}

						if (!foundInRadius.Contains(charge))
						{
							charge.unhighlight();
							chargesInRadius.RemoveAtFast(index);
						}
					}

					for (int index = 0; index < foundInRadius.Count; index++)
					{
						InteractableCharge charge = foundInRadius[index];

						if (charge == null)
						{
							continue;
						}

						if (!charge.hasOwnership)
						{
							continue;
						}

						if (!chargesInRadius.Contains(charge))
						{
							charge.highlight();
							chargesInRadius.Add(charge);
						}
					}
				}

				InteractableCharge bestCharge = null;
				float bestDot = 0.98f; // > -1 lets us not have a target when not looking near one

				for (int index = 0; index < chargesInRadius.Count; index++)
				{
					InteractableCharge charge = chargesInRadius[index];

					if (charge == null)
					{
						continue;
					}

					float dot = Vector3.Dot((charge.transform.position - MainCamera.instance.transform.position).normalized, MainCamera.instance.transform.forward);

					if (dot > bestDot)
					{
						bestCharge = charge;
						bestDot = dot;
					}
				}

				if (bestCharge != target)
				{
					if (target != null)
					{
						target.untarget();
					}

					target = bestCharge;

					if (target != null)
					{
						target.target();
					}
				}
			}
		}
	}
}
