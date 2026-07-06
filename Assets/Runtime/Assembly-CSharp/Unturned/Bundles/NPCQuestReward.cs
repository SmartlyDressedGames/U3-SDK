////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCQuestReward : INPCReward
	{
		public CachingBcAssetRef QuestAssetRef
		{
			get => _questAssetRef;
		}
		private CachingBcAssetRef _questAssetRef;

		public QuestAsset GetQuestAsset()
		{
			return _questAssetRef.Get<QuestAsset>();
		}

		public override void GrantReward(Player player)
		{
			QuestAsset asset = GetQuestAsset();
			if (asset == null)
				return;

			player.quests.ServerAddQuest(asset);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (!p.data.TryParseBcAssetRef("ID", EAssetType.NPC, out _questAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.NPC, out _questAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}
		}

		public NPCQuestReward() { }

		[System.Obsolete]
		public NPCQuestReward(System.Guid newQuestGuid, ushort newID, string newText) : base(newText)
		{
			_questAssetRef = new CachingBcAssetRef(newQuestGuid, EAssetType.NPC, newID);
		}

		[System.Obsolete]
		public System.Guid questGuid => QuestAssetRef.Guid;

		[System.Obsolete]
		public ushort id => QuestAssetRef.LegacyId;
	}
}
