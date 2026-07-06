////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{

	public class BarricadeTool : MonoBehaviour
	{
		//private static BarricadeTool tool;

		public static Transform getBarricade(Transform parent, byte hp, Vector3 pos, Quaternion rot, ushort id, byte[] state)
		{
			ItemBarricadeAsset asset = Assets.find(EAssetType.ITEM, id) as ItemBarricadeAsset;

			return getBarricade(parent, hp, 0, 0, pos, rot, id, state, asset);
		}

		private static Transform getEmptyBarricade(ushort id)
		{
			Transform barricade = new GameObject().transform;
			barricade.name = id.ToString();
			barricade.tag = "Barricade";
			barricade.gameObject.layer = LayerMasks.BARRICADE;

			return barricade;
		}

		public static Transform getBarricade(Transform parent, byte hp, ulong owner, ulong group, Vector3 pos, Quaternion rot, ushort id, byte[] state, ItemBarricadeAsset asset)
		{
			if (asset != null)
			{
				Transform barricade;

				if (asset.barricade != null)
				{
					InstantiateParameters instantiateParameters = new InstantiateParameters()
					{
						parent = parent,
						worldSpace = false,
					};
					barricade = Instantiate(asset.barricade, pos, rot, instantiateParameters).transform;
				}
				else
				{
					barricade = null;
				}

				if (barricade == null)
				{
					// Assets log errors for missing game objects, so we return an empty one
					barricade = getEmptyBarricade(id);
					barricade.parent = parent;
					barricade.localPosition = pos;
					barricade.localRotation = rot;
				}

				if (asset.useWaterHeightTransparentSort && !Dedicator.IsDedicatedServer)
				{
					barricade.gameObject.AddComponent<WaterHeightTransparentSort>();
				}

				barricade.name = id.ToString();

				if (Provider.isServer && asset.nav != null)
				{
					Transform nav = GameObject.Instantiate(asset.nav).transform;
					nav.name = "Nav";

					if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
					{
						Transform hinge = barricade.Find("Skeleton").Find("Hinge");
						if (hinge != null)
						{
							nav.parent = hinge;
						}
						else
						{
							nav.parent = barricade;
						}
					}
					else
					{
						nav.parent = barricade;
					}

					nav.localPosition = Vector3.zero;
					nav.localRotation = Quaternion.identity;
				}

				Transform burning = barricade.FindChildRecursive("Burning");
				if (burning != null)
				{
					burning.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.BURNING;
				}

				Transform warm = barricade.FindChildRecursive("Warm");
				if (warm != null)
				{
					warm.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.WARM;
				}

				//Transform claim = barricade.FindChild("Claim");
				//if(claim != null)
				//{
				//	ClaimTrigger trigger = claim.gameObject.AddComponent<ClaimTrigger>();
				//	trigger.hasOwnership = hasOwnership;
				//	trigger.owner = owner;
				//	trigger.group = group;
				//	claim.gameObject.SetActive(true);
				//}

				if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
				{
					InteractableDoor door = barricade.gameObject.AddComponent<InteractableDoor>();
					door.updateState(asset, state);
				}
				else if (asset.build == EBuild.BED)
				{
					barricade.gameObject.AddComponent<InteractableBed>().updateState(asset, state);
				}
				else if (asset.build == EBuild.STORAGE || asset.build == EBuild.STORAGE_WALL)
				{
					barricade.gameObject.AddComponent<InteractableStorage>().updateState(asset, state);
				}
				else if (asset.build == EBuild.FARM)
				{
					barricade.gameObject.AddComponent<InteractableFarm>().updateState(asset, state);
				}
				else if (asset.build == EBuild.TORCH || asset.build == EBuild.CAMPFIRE)
				{
					barricade.gameObject.AddComponent<InteractableFire>().updateState(asset, state);
				}
				else if (asset.build == EBuild.OVEN)
				{
					barricade.gameObject.AddComponent<InteractableOven>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SPIKE || asset.build == EBuild.WIRE)
				{
					Transform trapTrigger = barricade.Find("Trap");
					if (trapTrigger != null)
					{
						InteractableTrapTrigger triggerComponent = trapTrigger.gameObject.AddComponent<InteractableTrapTrigger>();
						InteractableTrap trap = barricade.gameObject.AddComponent<InteractableTrap>();
						triggerComponent.parentTrap = trap;
						trap.updateState(asset, state);
					}
				}
				else if (asset.build == EBuild.CHARGE)
				{
					InteractableCharge charge = barricade.gameObject.AddComponent<InteractableCharge>();
					charge.updateState(asset, state);

					charge.owner = owner;
					charge.group = group;
				}
				else if (asset.build == EBuild.GENERATOR)
				{
					barricade.gameObject.AddComponent<InteractableGenerator>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SPOT || asset.build == EBuild.CAGE)
				{
					barricade.gameObject.AddComponent<InteractableSpot>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SAFEZONE)
				{
					barricade.gameObject.AddComponent<InteractableSafezone>().updateState(asset, state);
				}
				else if (asset.build == EBuild.OXYGENATOR)
				{
					barricade.gameObject.AddComponent<InteractableOxygenator>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SIGN || asset.build == EBuild.SIGN_WALL || asset.build == EBuild.NOTE)
				{
					barricade.gameObject.AddComponent<InteractableSign>().updateState(asset, state);
				}
				else if (asset.build == EBuild.CLAIM)
				{
					InteractableClaim claim = barricade.gameObject.AddComponent<InteractableClaim>();
					claim.owner = owner;
					claim.group = group;
					claim.updateState(asset);
				}
				else if (asset.build == EBuild.BEACON)
				{
					barricade.gameObject.AddComponent<InteractableBeacon>().updateState(asset);
				}
				else if (asset.build == EBuild.BARREL_RAIN)
				{
					barricade.gameObject.AddComponent<InteractableRainBarrel>().updateState(asset, state);
				}
				else if (asset.build == EBuild.OIL)
				{
					barricade.gameObject.AddComponent<InteractableOil>().updateState(asset, state);
				}
				else if (asset.build == EBuild.TANK)
				{
					barricade.gameObject.AddComponent<InteractableTank>().updateState(asset, state);
				}
				else if (asset.build == EBuild.SENTRY || asset.build == EBuild.SENTRY_FREEFORM)
				{
					InteractableSentry sentry = barricade.gameObject.AddComponent<InteractableSentry>();
					InteractablePower power = barricade.gameObject.AddComponent<InteractablePower>();

					sentry.power = power;
					sentry.updateState(asset, state);
				}
				else if (asset.build == EBuild.LIBRARY)
				{
					barricade.gameObject.AddComponent<InteractableLibrary>().updateState(asset, state);
				}
				else if (asset.build == EBuild.MANNEQUIN)
				{
					barricade.gameObject.AddComponent<InteractableMannequin>().updateState(asset, state);
				}
				else if (asset.build == EBuild.STEREO)
				{
					barricade.gameObject.AddComponent<InteractableStereo>().updateState(asset, state);
				}
				else if (asset.build == EBuild.CLOCK)
				{
					if (!Dedicator.IsDedicatedServer)
					{
						InteractableClock clock = barricade.gameObject.AddComponent<InteractableClock>();
						clock.updateState(asset, state);
					}
				}

				if (!asset.isUnpickupable)
				{
					Interactable2HP health = barricade.gameObject.AddComponent<Interactable2HP>();
					health.hp = hp;

					if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
					{
						Transform hinge = barricade.Find("Skeleton").Find("Hinge");
						if (hinge != null)
						{
							Interactable2SalvageBarricade salv = hinge.gameObject.AddComponent<Interactable2SalvageBarricade>();
							salv.root = barricade;
							salv.hp = health;
							salv.owner = owner;
							salv.group = group;
							salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
						}

						Transform hingeLeft = barricade.Find("Skeleton").Find("Left_Hinge");
						if (hingeLeft != null)
						{
							Interactable2SalvageBarricade salv = hingeLeft.gameObject.AddComponent<Interactable2SalvageBarricade>();
							salv.root = barricade;
							salv.hp = health;
							salv.owner = owner;
							salv.group = group;
							salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
						}

						Transform hingeRight = barricade.Find("Skeleton").Find("Right_Hinge");
						if (hingeRight != null)
						{
							Interactable2SalvageBarricade salv = hingeRight.gameObject.AddComponent<Interactable2SalvageBarricade>();
							salv.root = barricade;
							salv.hp = health;
							salv.owner = owner;
							salv.group = group;
							salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
						}
					}
					//else if(asset.build == EBuild.SPIKE || asset.build == EBuild.WIRE)
					//{
					//	barricade.FindChild("Trap").gameObject.AddComponent<Interactable2SalvageBarricade>().root = barricade;
					//}
					else
					{
						Interactable2SalvageBarricade salv = barricade.gameObject.AddComponent<Interactable2SalvageBarricade>();
						salv.root = barricade;
						salv.hp = health;
						salv.owner = owner;
						salv.group = group;
						salv.salvageDurationMultiplier = asset.salvageDurationMultiplier;
					}
				}

				return barricade;
			}
			else
			{
				return getEmptyBarricade(id);
			}
		}

		//		private void Start()
		//		{
		//			tool = this;
		//		}
	}
}
