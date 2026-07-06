////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class DialogueElement
	{
		public byte index
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public INPCCondition[] conditions
		{
			get => conditionsList.conditions;
		}

		protected NPCConditionsList conditionsList;

		[System.Obsolete]
		public INPCReward[] rewards
		{
			get => rewardsList.rewards;
		}

		protected NPCRewardsList rewardsList;

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

		public DialogueElement(byte newIndex, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList)
		{
			index = newIndex;
			conditionsList = newConditionsList;
			rewardsList = newRewardsList;
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
