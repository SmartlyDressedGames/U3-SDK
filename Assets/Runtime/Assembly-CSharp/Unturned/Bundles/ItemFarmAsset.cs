////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemFarmAsset : ItemBarricadeAsset
	{
		protected uint _growth;
		public uint growth => _growth;

		protected ushort _grow;
		public ushort grow => _grow;

		public System.Guid growSpawnTableGuid;

		public bool ignoreSoilRestrictions
		{
			get;
			protected set;
		}

		public bool canFertilize
		{
			get;
			protected set;
		}

		/// <summary>
		/// Amount of experience to reward harvesting player.
		/// </summary>
		public uint harvestRewardExperience;

		/// <summary>
		/// If true, harvesting has a chance to provide a second item.
		/// </summary>
		public bool isAffectedByAgricultureSkill
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, rain will finish growing the plant.
		/// </summary>
		public bool shouldRainAffectGrowth
		{
			get;
			protected set;
		}

		/// <summary>
		/// NPC rewards to grant upon harvesting the crop.
		/// </summary>
		internal NPCRewardsList harvestRewardsList;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (grow != 0)
			{
				ItemAsset growAsset = Assets.find(EAssetType.ITEM, grow) as ItemAsset;
				if (growAsset != null)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_GrowSpecificItem", "<color=" + Palette.hex(ItemTool.getRarityColorUI(growAsset.rarity)) + ">" + growAsset.itemName + "</color>"), DescSort_Important);
				}
			}

			// We use the string builder to group these lines as a paragraph because they are all longer sentences.
			builder.stringBuilder.Clear();

			if (!ignoreSoilRestrictions)
			{
				// In vanilla, farmable items do require soil. It's very common for mods to use farms in other ways,
				// however, so requiring soil is shown rather than the other way around.
				builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_RequiresSoil"));
			}

			if (canFertilize)
			{
				if (builder.stringBuilder.Length > 0)
				{
					builder.stringBuilder.Append(' ');
				}
				builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_CanFertilize"));
			}

			if (isAffectedByAgricultureSkill)
			{
				if (builder.stringBuilder.Length > 0)
				{
					builder.stringBuilder.Append(' ');
				}
				builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_AffectedByAgricultureSkill"));
			}

			if (shouldRainAffectGrowth)
			{
				if (builder.stringBuilder.Length > 0)
				{
					builder.stringBuilder.Append(' ');
				}
				builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_AffectedByRain"));
			}

			if (builder.stringBuilder.Length > 0)
			{
				builder.Append(builder.stringBuilder.ToString(), DescSort_FarmableText);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_growth = p.data.ParseUInt32("Growth");
			_grow = p.data.ParseUInt16("Grow");
			growSpawnTableGuid = p.data.ParseGuid("Grow_SpawnTable");
			ignoreSoilRestrictions = p.data.ContainsKey("Ignore_Soil_Restrictions");
			canFertilize = p.data.ParseBool("Allow_Fertilizer", defaultValue: true);
			harvestRewardExperience = p.data.ParseUInt32("Harvest_Reward_Experience", defaultValue: 1);
			isAffectedByAgricultureSkill = p.data.ParseBool("Affected_By_Agriculture_Skill", defaultValue: true);
			shouldRainAffectGrowth = p.data.ParseBool("Rain_Affects_Growth", defaultValue: true);
			harvestRewardsList.Parse(p.data, p.localization, this, "Harvest_Rewards", "Harvest_Reward_");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Farm
			// Game data for Farm Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Farm");
			data.Append("GUID", GUID); // Key

			data.Append("Growth", growth);
			data.Append("Grow", grow);
			data.Append("Grow_SpawnTable", growSpawnTableGuid);
			data.Append("Ignore_Soil_Restrictions", ignoreSoilRestrictions);
			data.Append("Allow_Fertilizer", canFertilize);
			data.Append("Harvest_Reward_Experience", harvestRewardExperience);
			data.Append("Affected_By_Agriculture_Skill", isAffectedByAgricultureSkill);
			data.Append("Rain_Affects_Growth", shouldRainAffectGrowth);
		}
	}
}
