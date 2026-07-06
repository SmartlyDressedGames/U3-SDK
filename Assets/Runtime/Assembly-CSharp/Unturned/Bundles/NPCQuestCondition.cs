////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class NPCQuestCondition : NPCLogicCondition
	{
		public CachingBcAssetRef QuestAssetRef
		{
			get => _questAssetRef;
		}
		private CachingBcAssetRef _questAssetRef;

		public ENPCQuestStatus status
		{
			get;
			protected set;
		}

		public bool ignoreNPC
		{
			get;
			protected set;
		}

		public QuestAsset GetQuestAsset()
		{
			return _questAssetRef.Get<QuestAsset>();
		}

		public override bool isConditionMet(Player player)
		{
			QuestAsset asset = GetQuestAsset();
			return doesLogicPass(player.quests.GetQuestStatus(asset), status);
		}

		public override void ApplyCondition(Player player)
		{
			if (shouldReset == false)
			{
				return;
			}

			QuestAsset asset = GetQuestAsset();
			if (asset == null)
				return;

			switch (status)
			{
				case ENPCQuestStatus.ACTIVE: // abandon
					player.quests.ServerRemoveQuest(asset);
					return;

				case ENPCQuestStatus.READY: // turn in
					player.quests.CompleteQuest(asset, ignoreNPC);
					return;

				case ENPCQuestStatus.COMPLETED: // make available again
					player.quests.sendRemoveFlag(asset.id);
					return;
			}
		}

		public override bool isAssociatedWithFlag(ushort flagID)
		{
			// Quest ID is also used as a flag ID to mark when the quest is completed.
#pragma warning disable
			return flagID == id;
#pragma warning restore
		}

		internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
		{
			// Quest ID is also used as a flag ID to mark when the quest is completed.
			// Ideally in the future this can be updated to not need to find the quest asset.
#pragma warning disable
			if (id > 0)
			{
				associatedFlags.Add(id);
			}
#pragma warning restore
			else
			{
				QuestAsset asset = GetQuestAsset();
				if (asset != null)
				{
					associatedFlags.Add(asset.id);
				}
			}
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (!p.data.TryParseBcAssetRef("ID", EAssetType.NPC, out _questAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseEnum("Status", out ENPCQuestStatus _status))
			{
				status = _status;
				if (_status == ENPCQuestStatus.NONE && shouldReset)
				{
					p.ReportError("Quest condition has Reset enabled with Status None (probably accidental)");
				}
			}
			else
			{
				p.ReportRequiredOptionInvalid("Status");
			}

			ignoreNPC = p.data.ParseBool("Ignore_NPC");
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.NPC, out _questAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseEnum(p.legacyPrefix + "_Status", out ENPCQuestStatus _status))
			{
				status = _status;
				if (_status == ENPCQuestStatus.NONE && shouldReset)
				{
					p.ReportError("Quest condition has Reset enabled with Status None (probably accidental)");
				}
			}
			else
			{
				p.ReportRequiredOptionInvalid("Status");
			}

			ignoreNPC = p.data.ContainsKey(p.legacyPrefix + "_Ignore_NPC");
		}

		public NPCQuestCondition() { }

		[System.Obsolete]
		public NPCQuestCondition(System.Guid newQuestGuid, ushort newID, ENPCQuestStatus newStatus, bool newIgnoreNPC, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			_questAssetRef = new CachingBcAssetRef(newQuestGuid, EAssetType.NPC, newID);
			status = newStatus;
			ignoreNPC = newIgnoreNPC;
		}

		[System.Obsolete]
		public System.Guid questGuid => QuestAssetRef.Guid;

		[System.Obsolete]
		public ushort id => QuestAssetRef.LegacyId;
	}
}
