////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class InteractableObjectDialogue : InteractableObject, IDialogueTarget
	{
		public Vector3 GetDialogueTargetWorldPosition()
		{
			return transform.position;
		}

		public NetId GetDialogueTargetNetId()
		{
			return NetIdRegistry.GetTransformNetId(transform);
		}

		public bool ShouldServerApproveDialogueRequest(Player withPlayer)
		{
			return objectAsset.areConditionsMet(withPlayer);
		}

		public DialogueAsset FindStartingDialogueAsset()
		{
			return objectAsset.FindInteractabilityDialogueAsset();
		}

		public string GetDialogueTargetDebugName()
		{
			string displayName = objectAsset.InteractabilityDialogueDisplayName;
			if (string.IsNullOrEmpty(displayName))
			{
				return $"object {objectAsset.objectName}";
			}
			else
			{
				return $"object {displayName} / {objectAsset.objectName}";
			}
		}

		public string GetDialogueTargetNameShownToPlayer(Player player)
		{
			string displayName = objectAsset.InteractabilityDialogueDisplayName;
			if (string.IsNullOrEmpty(displayName))
			{
				displayName = objectAsset.objectName;
			}
			return displayName;
		}

		public void SetFaceOverride(byte? faceOverride)
		{
			// N/A
		}

		public void SetIsTalkingWithLocalPlayer(bool isTalkingWithLocalPlayer)
		{
			// N/A
		}

		public override void use()
		{
			DialogueAsset dialogueAsset = FindStartingDialogueAsset();
			if (dialogueAsset == null)
			{
				UnturnedLog.warn("Failed to find interactable object dialogue: " + GetDialogueTargetDebugName());
				return;
			}

			ObjectManager.SendTalkWithNpcRequest.Invoke(NetTransport.ENetReliability.Reliable, GetDialogueTargetNetId());
		}

		public override bool checkUseable()
		{
			return objectAsset.areInteractabilityConditionsMet(Player.LocalPlayer);
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			INPCCondition unmetCondition = objectAsset.interactabilityConditionsList.GetFirstUnmetCondition(Player.LocalPlayer);
			if (unmetCondition != null)
			{
				text = unmetCondition.formatCondition(Player.LocalPlayer);
				color = Color.white;

				if (string.IsNullOrEmpty(text))
				{
					message = EPlayerMessage.NONE;
					return false;
				}
				else
				{
					message = EPlayerMessage.CONDITION;
					return true;
				}
			}

			message = EPlayerMessage.INTERACT;
			text = objectAsset.interactabilityText;
			color = Color.white;
			return true;
		}
	}
}
