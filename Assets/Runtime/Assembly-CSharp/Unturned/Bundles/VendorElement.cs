////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public abstract class VendorElement
	{
		/// <summary>
		/// Vendor asset that owns this buy/sell record.
		/// </summary>
		public VendorAsset outerAsset
		{
			get;
			protected set;
		}

		public byte index
		{
			get;
			protected set;
		}

		public System.Guid TargetAssetGuid
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public ushort id
		{
			get;
			protected set;
		}

		public uint cost
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

		public abstract string displayName
		{
			get;
		}

		/// <summary>
		/// If not null, replaces item/vehicle description.
		/// </summary>
		protected string descriptionOverride = null;

		public virtual string displayDesc => null;

		public virtual bool hasIcon => true;

		public abstract EItemRarity rarity
		{
			get;
		}

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

		public VendorElement(VendorAsset newOuterAsset, byte newIndex, System.Guid newGuid, ushort newLegacyId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride)
		{
			outerAsset = newOuterAsset;
			index = newIndex;
			TargetAssetGuid = newGuid;
#pragma warning disable
			id = newLegacyId;
#pragma warning restore
			cost = newCost;
			conditionsList = newConditionsList;
			rewardsList = newRewardsList;
			descriptionOverride = newDescriptionOverride;
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
