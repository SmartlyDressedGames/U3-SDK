////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class DialogueResponse : DialogueElement
	{
		public byte[] messages
		{
			get;
			protected set;
		}

		public System.Guid dialogueGuid;

		public ushort dialogue
		{
			[System.Obsolete]
			get;
			protected set;
		}

		public bool IsDialogueRefNull()
		{
#pragma warning disable
			return dialogue == 0 && dialogueGuid.IsEmpty();
#pragma warning restore
		}

		public DialogueAsset FindDialogueAsset()
		{
#pragma warning disable
			return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(dialogueGuid, dialogue);
#pragma warning restore
		}

		public System.Guid questGuid;

		public ushort quest
		{
			[System.Obsolete]
			get;
			protected set;
		}

		public bool IsQuestRefNull()
		{
#pragma warning disable
			return quest == 0 && questGuid.IsEmpty();
#pragma warning restore
		}

		public QuestAsset FindQuestAsset()
		{
#pragma warning disable
			return Assets.FindNpcAssetByGuidOrLegacyId<QuestAsset>(questGuid, quest);
#pragma warning restore
		}

		public System.Guid vendorGuid;

		public ushort vendor
		{
			[System.Obsolete]
			get;
			protected set;
		}

		public bool IsVendorRefNull()
		{
#pragma warning disable
			return vendor == 0 && vendorGuid.IsEmpty();
#pragma warning restore
		}

		public VendorAsset FindVendorAsset()
		{
#pragma warning disable
			return Assets.FindNpcAssetByGuidOrLegacyId<VendorAsset>(vendorGuid, vendor);
#pragma warning restore
		}

		public string text
		{
			get;
			protected set;
		}

		public DialogueResponse(byte newID, byte[] newMessages, ushort newDialogue, System.Guid newDialogueGuid, ushort newQuest, System.Guid newQuestGuid, ushort newVendor, System.Guid newVendorGuid, string newText, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList) : base(newID, newConditionsList, newRewardsList)
		{
			messages = newMessages;
			dialogue = newDialogue;
			dialogueGuid = newDialogueGuid;
			quest = newQuest;
			questGuid = newQuestGuid;
			vendor = newVendor;
			vendorGuid = newVendorGuid;
			text = newText;
		}
	}
}
