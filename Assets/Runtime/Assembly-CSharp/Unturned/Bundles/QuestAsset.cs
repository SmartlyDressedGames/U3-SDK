////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class QuestAsset : Asset
	{
		public string questName
		{
			get;
			protected set;
		}

		public string questDescription
		{
			get;
			protected set;
		}

		public INPCCondition[] conditions
		{
			get => conditionsList.conditions;
		}

		protected NPCConditionsList conditionsList;

		public INPCReward[] rewards
		{
			get => rewardsList.rewards;
		}

		protected NPCRewardsList rewardsList;

		/// <summary>
		/// Rewards to grant when quest is removed without completing.
		/// Not granted when player finishes quest.
		/// </summary>
		protected NPCRewardsList abandonmentRewardsList;

		public override EAssetType assetCategory => EAssetType.NPC;

		public bool areConditionsMet(Player player)
		{
			return conditionsList.AreConditionsMet(player);
		}

		public void ApplyConditions(Player player)
		{
			conditionsList.ApplyConditions(player);
		}

		public void GrantRewards(Player player)
		{
			rewardsList.Grant(player);
		}

		public void GrantAbandonmentRewards(Player player)
		{
			abandonmentRewardsList.Grant(player);
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 2000 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 2000");
			}

			questName = p.localization.format("Name");
			questName = ItemTool.filterRarityRichText(questName);

			string description = p.localization.format("Description");
			description = ItemTool.filterRarityRichText(description);
			RichTextUtil.replaceNewlineMarkup(ref description);
			questDescription = description;

			conditionsList.Parse(p.data, p.localization, this, "Conditions", "Condition_");

			rewardsList.Parse(p.data, p.localization, this, "Rewards", "Reward_");
			abandonmentRewardsList.Parse(p.data, p.localization, this, "AbandonmentRewards", "AbandonmentReward_");
		}

		[System.Obsolete("Removed shouldSend parameter")]
		public void applyConditions(Player player, bool shouldSend)
		{
			ApplyConditions(player);
		}

		[System.Obsolete("Removed shouldSend parameter")]
		public void grantRewards(Player player, bool shouldSend)
		{
			GrantRewards(player);
		}
	}
}
