////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableObjectDropper : InteractableObjectTriggerableBase
	{
		public bool isUsable => Time.realtimeSinceStartup - lastUsed > objectAsset.interactabilityDelay && (objectAsset.interactabilityPower == EObjectInteractabilityPower.NONE || isWired);

		private float lastUsed = -9999;

		private ushort[] interactabilityDrops;
		private ushort interactabilityRewardID;

		private AudioSource audioSourceComponent;

		private void initAudioSourceComponent()
		{
			audioSourceComponent = transform.GetComponent<AudioSource>();
		}

		private void updateAudioSourceComponent()
		{
			if (audioSourceComponent != null)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					audioSourceComponent.Play();
				}
			}
		}

		private Transform dropTransform;

		private void initDropTransform()
		{
			dropTransform = transform.Find("Drop");
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			interactabilityDrops = ((ObjectAsset) asset).interactabilityDrops;
			interactabilityRewardID = ((ObjectAsset) asset).interactabilityRewardID;

			initAudioSourceComponent();
			initDropTransform();
		}

		public void drop()
		{
			lastUsed = Time.realtimeSinceStartup;

			if (dropTransform == null)
			{
				return;
			}

			if (objectAsset.holidayRestriction == ENPCHoliday.NONE || Provider.modeConfigData.Objects.Allow_Holiday_Drops)
			{
				if (interactabilityRewardID != 0)
				{
					ushort id = SpawnTableTool.ResolveLegacyId(interactabilityRewardID, EAssetType.ITEM, OnGetDropSpawnTableErrorContext);

					if (id != 0)
					{
						ItemManager.dropItem(new Item(id, objectAsset.interactabilityRewardItemOrigin), dropTransform.position, false, true, false);
					}
				}
				else
				{
					ushort dropID = interactabilityDrops[Random.Range(0, interactabilityDrops.Length)];

					if (dropID != 0)
					{
						ItemManager.dropItem(new Item(dropID, objectAsset.interactabilityRewardItemOrigin), dropTransform.position, false, true, false);
					}
				}
			}
		}

		public override void use()
		{
			updateAudioSourceComponent();

			ObjectManager.useObjectDropper(transform);
		}

		public override bool checkUseable()
		{
			return (objectAsset.interactabilityPower == EObjectInteractabilityPower.NONE || isWired) && objectAsset.areInteractabilityConditionsMet(Player.LocalPlayer);
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			INPCCondition unmetCondition = objectAsset.interactabilityConditionsList.GetFirstUnmetCondition(Player.LocalPlayer);
			if (unmetCondition != null)
			{
				message = EPlayerMessage.CONDITION;
				text = unmetCondition.formatCondition(Player.LocalPlayer);
				color = Color.white;
				return true;
			}

			if (objectAsset.interactabilityPower != EObjectInteractabilityPower.NONE && !isWired)
			{
				message = EPlayerMessage.POWER;
			}
			else
			{
				switch (objectAsset.interactabilityHint)
				{
					case EObjectInteractabilityHint.DOOR:
						message = EPlayerMessage.DOOR_OPEN;
						break;
					case EObjectInteractabilityHint.SWITCH:
						message = EPlayerMessage.SPOT_ON;
						break;
					case EObjectInteractabilityHint.FIRE:
						message = EPlayerMessage.FIRE_ON;
						break;
					case EObjectInteractabilityHint.GENERATOR:
						message = EPlayerMessage.GENERATOR_ON;
						break;
					case EObjectInteractabilityHint.USE:
						message = EPlayerMessage.USE;
						break;

					case EObjectInteractabilityHint.CUSTOM:
						message = EPlayerMessage.INTERACT;
						text = objectAsset.interactabilityText;
						color = Color.white;
						return true;

					default:
						message = EPlayerMessage.NONE;
						break;
				}
			}

			text = "";
			color = Color.white;
			return true;
		}

		private string OnGetDropSpawnTableErrorContext()
		{
			return $"{objectAsset?.FriendlyName} drop";
		}
	}
}
