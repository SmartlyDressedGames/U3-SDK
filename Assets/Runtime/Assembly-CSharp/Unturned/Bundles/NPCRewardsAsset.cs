////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCRewardsAsset : Asset
	{
		[System.Obsolete]
		public INPCCondition[] conditions
		{
			get => conditionsList.conditions;
		}

		internal NPCConditionsList conditionsList;
		private NPCRewardsList rewardsList;

		public bool AreConditionsMet(Player player)
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

		public override string GetTypeFriendlyName()
		{
			return "NPC Rewards List";
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			conditionsList.Parse(p.data, p.localization, this, "Conditions", "Condition_");
			rewardsList.Parse(p.data, p.localization, this, "Rewards", "Reward_");
		}
	}
}
