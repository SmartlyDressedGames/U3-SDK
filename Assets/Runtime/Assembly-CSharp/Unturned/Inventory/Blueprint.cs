////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class Blueprint
	{
		/// <summary>
		/// Optional case-sensitive identifier in list of blueprints.
		/// Added as an alternative to referencing blueprints by index.
		/// Defaults to null.
		/// </summary>
		public string Name
		{
			get;
			internal set;
		}

		public IBlueprintOwner Owner
		{
			get;
			internal set;
		}

		public Asset GetOwnerAsset()
		{
			return Owner.GetBlueprintOwnerAsset();
		}

		private byte index;
		/// <summary>
		/// Index into Owner's blueprints list.
		/// </summary>
		public byte Index => index;

		internal EBlueprintOperation _operation;
		/// <summary>
		/// Operation replaces the special behavior for EBlueprintType.Ammo and EBlueprintType.Repair.
		/// </summary>
		public EBlueprintOperation Operation => _operation;

		/// <summary>
		/// Note: if resolving ref please use GetCategoryTag instead for caching.
		/// </summary>
		public CachingAssetRef CategoryTagRef => _categoryTagRef;
		internal CachingAssetRef _categoryTagRef;

		/// <summary>
		/// Category tag replaces the blueprint "Type" which acted as both category AND behaviour modifier.
		/// </summary>
		public TagAsset GetCategoryTag()
		{
			return _categoryTagRef.Get<TagAsset>();
		}

		private BlueprintSupply[] _supplies;
		public BlueprintSupply[] supplies => _supplies;

		/// <summary>
		/// Only applicable for operations with a target item.
		/// 
		/// Nelson 2025-04-11: initially, this was implemented as the last item in supplies list. However, there are a
		/// lot of checks for special handling of target item, so I think it makes sense to separate.
		/// </summary>
		public BlueprintSupply TargetItem
		{
			get;
			set;
		}

		private BlueprintOutput[] _outputs;
		public BlueprintOutput[] outputs => _outputs;

		/// <summary>
		/// If not null, these tags must be provided by nearby objects to craft this blueprint.
		/// Note: this is the list as-configured. It has not been filtered according to gameplay config.
		/// </summary>
		public CachingAssetRef[] RequiresNearbyCraftingTags
		{
			get;
			internal set;
		}

		/// <summary>
		/// Not shown in the UI. These tags are checked once per-level-startup.
		/// For example, used to check for "singleplayer" tag or a map-specific tag.
		/// </summary>
		public CachingAssetRef[] RequiresStaticTags
		{
			get;
			set;
		}

		private bool hasCheckedForVanillaHeatSourceTag;
		private bool requiresVanillaHeatSourceTag;
		private static CachingAssetRef[] onlyVanillaHeatSourceTag = new CachingAssetRef[] { PowerTool.VanillaCraftingHeatTag };

		public CachingAssetRef[] GetApplicableRequiredNearbyCraftingTags()
		{
			if (RequiresNearbyCraftingTags == null || RequiresNearbyCraftingTags.Length < 1)
				return null;

			if (Provider.modeConfigData?.Gameplay?.Enable_Workstation_Requirements ?? true)
			{
				return RequiresNearbyCraftingTags;
			}

			if (!hasCheckedForVanillaHeatSourceTag)
			{
				hasCheckedForVanillaHeatSourceTag = true;

				foreach (CachingAssetRef tagRef in RequiresNearbyCraftingTags)
				{
					if (tagRef == PowerTool.VanillaCraftingHeatTag)
					{
						requiresVanillaHeatSourceTag = true;
						break;
					}
				}
			}

			return requiresVanillaHeatSourceTag ? onlyVanillaHeatSourceTag : null;
		}

		internal CachingBcAssetRef effectAssetRef;
		public System.Guid BuildEffectGuid => effectAssetRef.Guid;

		public EffectAsset FindBuildEffectAsset()
		{
			return effectAssetRef.Get<EffectAsset>();
		}

		private byte _level;
		public byte level => _level;

		public int SkillSpecialityIndex
		{
			get;
			internal set;
		} = -1;
		public int SkillIndex
		{
			get;
			internal set;
		} = -1;

		public string DebugGetSkillName()
		{
			if (RequiresSkill)
			{
				EPlayerSpeciality speciality = (EPlayerSpeciality) SkillSpecialityIndex;
				switch (speciality)
				{
					case EPlayerSpeciality.OFFENSE:
					{
						return ((EPlayerOffense) SkillIndex).ToString();
					}

					case EPlayerSpeciality.DEFENSE:
					{
						return ((EPlayerDefense) SkillIndex).ToString();
					}

					case EPlayerSpeciality.SUPPORT:
					{
						return ((EPlayerSupport) SkillIndex).ToString();
					}
				}
			}

			return null;
		}

		private bool _transferState;
		public bool transferState => _transferState;

		/// <summary>
		/// If true, and transferState is enabled, delete attached items.
		/// </summary>
		public bool withoutAttachments;

		public string map
		{
			get;
			private set;
		}

		/// <summary>
		/// Must match conditions to craft.
		/// </summary>
		public INPCCondition[] questConditions
		{
			get => questConditionsList.conditions;
		}

		protected NPCConditionsList questConditionsList;

		/// <summary>
		/// Extra rewards given after crafting. Not displayed.
		/// </summary>
		public INPCReward[] questRewards
		{
			get => questRewardsList.rewards;
		}

		protected NPCRewardsList questRewardsList;

		/// <summary>
		/// Useful for hiding developer/debug internal blueprints that should not be visible when players search by name.
		/// </summary>
		public bool canBeVisibleWhenSearchedWithoutRequiredItems = true;

		/// <summary>
		/// Defaults to false. If true, blueprint can become visible in the crafting list even when NPC conditions
		/// are not met. This should typically only be enabled if all conditions are configured to be visible in the
		/// details panel. Otherwise, the default "conditions unmet" label isn't very informative for players.
		/// </summary>
		public bool CanBeVisibleWithUnmetConditions
		{
			get;
			set;
		}

		public bool areConditionsMet(Player player)
		{
			return questConditionsList.AreConditionsMet(player);
		}

		public void ApplyConditions(Player player)
		{
			questConditionsList.ApplyConditions(player);
		}

		public void GrantRewards(Player player)
		{
			questRewardsList.Grant(player);
		}

		public bool DoesRequireNearbyCraftingTag(TagAsset tag)
		{
			if (tag == null || RequiresNearbyCraftingTags == null)
			{
				return false;
			}

			for (int index = 0; index < RequiresNearbyCraftingTags.Length; ++index)
			{
				ref CachingAssetRef tagRef = ref RequiresNearbyCraftingTags[index];
				if (tagRef.IsReferenceTo(tag))
				{
					return true;
				}
			}

			return false;
		}

		public int CountOverlappingRequiredNearbyCraftingTags(HashSet<TagAsset> tags)
		{
			if (tags == null || tags.Count < 1 || RequiresNearbyCraftingTags.IsNullOrEmpty())
			{
				return 0;
			}

			int sum = 0;
			for (int index = 0; index < RequiresNearbyCraftingTags.Length; ++index)
			{
				ref CachingAssetRef tagRef = ref RequiresNearbyCraftingTags[index];
				TagAsset tag = tagRef.Get<TagAsset>();
				if (tag != null && tags.Contains(tag))
				{
					++sum;
				}
			}
			return sum;
		}

		public bool ContainsAnyOfItems(HashSet<ItemAsset> availableItems)
		{
			if (supplies == null)
				return false;

			foreach (BlueprintSupply inputItemConfig in supplies)
			{
				ItemAsset inputItem = inputItemConfig.FindItemAsset();
				if (inputItem != null && availableItems.Contains(inputItem))
				{
					return true;
				}
			}

			ItemAsset targetItemAsset = TargetItem?.FindItemAsset();
			if (targetItemAsset != null && availableItems.Contains(targetItemAsset))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Search output items (excluding target item) for specific item.
		/// </summary>
		public bool DoesOutputCreateItem(ItemAsset itemAsset)
		{
			if (itemAsset == null || _outputs == null || _outputs.Length < 1)
			{
				return false;
			}

			foreach (BlueprintOutput output in _outputs)
			{
				if (output.FindItemAsset() == itemAsset)
				{
					return true;
				}
			}

			return false;
		}

		internal bool IsOutputFreeformBuildable
		{
			get
			{
				if (_outputs == null || _outputs.Length < 1)
					return false;

				foreach (BlueprintOutput output in _outputs)
				{
					ItemBarricadeAsset asset = output.FindItemAsset() as ItemBarricadeAsset;
					if (asset != null && asset.build == EBuild.FREEFORM)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool RequiresSkill => SkillSpecialityIndex >= 0 && SkillIndex >= 0 && level > 0;

		public EBlueprintSkill GetLegacyBlueprintSkill()
		{
			if (SkillSpecialityIndex == (int) EPlayerSpeciality.SUPPORT)
			{
				if (SkillIndex == (int) EPlayerSupport.CRAFTING)
				{
					return EBlueprintSkill.CRAFT;
				}
				else if (SkillIndex == (int) EPlayerSupport.COOKING)
				{
					return EBlueprintSkill.COOK;
				}
				else if (SkillIndex == (int) EPlayerSupport.ENGINEER)
				{
					return EBlueprintSkill.REPAIR;
				}
			}

			return EBlueprintSkill.NONE;
		}

		public int GetPlayerSkillLevel(Player player)
		{
			return player.skills.skills[SkillSpecialityIndex][SkillIndex].level;
		}

		[System.Obsolete]
		public Blueprint(ItemAsset newSourceItem, byte newID, EBlueprintType newType, BlueprintSupply[] newSupplies,
			BlueprintOutput[] newOutputs, ushort newTool, bool newToolCritical, ushort newBuild, byte newLevel,
			EBlueprintSkill newSkill, bool newTransferState, string newMap, NPCConditionsList newQuestConditionsList,
			NPCRewardsList newQuestRewardsList)
			: this(newID, newSupplies, newOutputs,
				  newLevel, newSkill, newTransferState, false, newMap, newQuestConditionsList, newQuestRewardsList)
		{ }

		public Blueprint(byte newIndex, BlueprintSupply[] newSupplies,
			BlueprintOutput[] newOutputs, byte newLevel, EBlueprintSkill newSkill, bool newTransferState,
			bool newWithoutAttachments, string newMap, NPCConditionsList newQuestConditionsList,
			NPCRewardsList newQuestRewardsList)
		{
			index = newIndex;
			_supplies = newSupplies;
			_outputs = newOutputs;
			_level = newLevel;
			_transferState = newTransferState;
			withoutAttachments = newWithoutAttachments;
			map = newMap;

			questConditionsList = newQuestConditionsList;
			questRewardsList = newQuestRewardsList;
		}

		public override string ToString()
		{
			string text = string.Empty;
			text += GetCategoryTag()?.FriendlyName;
			text += ": ";

			for (int supplyIndex = 0; supplyIndex < supplies.Length; supplyIndex++)
			{
				if (supplyIndex > 0)
				{
					text += " + ";
				}

				text += supplies[supplyIndex].FindItemAsset()?.FriendlyName ?? "null";
				text += " x";
				text += supplies[supplyIndex].amount;
			}

			if (TargetItem != null)
			{
				text += " -> ";
				text += TargetItem.FindItemAsset()?.FriendlyName ?? "null";
				text += " x";
				text += TargetItem.amount;
			}

			if (outputs != null && outputs.Length > 0)
			{
				text += " = ";

				for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
				{
					if (outputIndex > 0)
					{
						text += " + ";
					}

					text += outputs[outputIndex].FindItemAsset()?.FriendlyName ?? "null";
					text += " x";
					text += outputs[outputIndex].amount;
				}
			}

			return text;
		}

		[System.Obsolete("Changed to OwnerAsset because blueprints can be contained in CraftingAsset now")]
		public ItemAsset sourceItem
		{
			get => GetOwnerAsset() as ItemAsset;
		}

		[System.Obsolete("Replaced by input item ShouldConsume false")]
		public ushort tool
		{
			get
			{
				if (_supplies != null && _supplies.Length > 0)
				{
					BlueprintSupply potentialToolInput = _supplies[_supplies.Length - 1];
					if (!potentialToolInput.ShouldConsume)
					{
						return potentialToolInput.id;
					}
				}

				return 0;
			}
		}

		[System.Obsolete("Replaced by input item ShouldConsume false")]
		public bool toolCritical
		{
			get
			{
				if (_supplies != null && _supplies.Length > 0)
				{
					BlueprintSupply potentialToolInput = _supplies[_supplies.Length - 1];
					if (!potentialToolInput.ShouldConsume)
					{
						return potentialToolInput.ShouldConsume;
					}
				}

				return false;
			}
		}

		[System.Obsolete("Renamed to Index to distinguish from named blueprint")]
		public byte id => index;

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

		[System.Obsolete]
		public ushort build => effectAssetRef.LegacyId;

		[System.Obsolete("Please use CategoryTags and Operation properties instead.")]
		public EBlueprintType type
		{
			get
			{
				// Most code referencing type is using it for AMMO/REPAIR special cases.
				switch (_operation)
				{
					case EBlueprintOperation.FillTargetItem:
						return EBlueprintType.AMMO;

					case EBlueprintOperation.RepairTargetItem:
						return EBlueprintType.REPAIR;
				}

				// At least when this update first comes out, all CategoryTags will be one of the old defaults and
				// should line up with the old value.
				for (int index = 0; index < EBlueprintTypeEx.legacyBlueprintTypeCategoryTagRefs.Length; ++index)
				{
					if (_categoryTagRef == EBlueprintTypeEx.legacyBlueprintTypeCategoryTagRefs[index])
					{
						return (EBlueprintType) index;
					}
				}

				return default;
			}
		}

		[System.Obsolete("Replaced in favor of supporting all skills, ideally more customizable in future.")]
		public EBlueprintSkill skill => GetLegacyBlueprintSkill();
	}
}
