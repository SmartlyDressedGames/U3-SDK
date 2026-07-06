////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2025-04-09: this acted as both category AND behaviour modifier, so I'm separating it into a custom tag
	/// for categorization and a property for overriding how the blueprint processes input items.
	///
	/// Nelson 2025-04-10: repair and ammo "types" had a variety of quirks I wanted to sort out:
	/// • Moving amount between items required ammo type blueprint, but some modders expressed interest in non-ammo use.
	///   (I.e., ideally better supporting amount on non-ammo items going forward.)
	/// • Both types ignored output items. Output was used to represent the target item. Similarly, the UI added a fake
	///   extra input item representing target item.
	/// • PlayerCrafting and PlayerDashboardCraftingUI re-implemented some crafting item searching logic for finding
	///   the item to refill or repair that can be converted into input item parameters.
	/// The plan at the moment is to make the last input item the "target" item for operations. Legacy ammo/repair
	/// blueprints will then default to no output item and add an extra input item. (And add a variety of parameters
	/// needed to replicate the specialized item search behaviour.)
	/// </summary>
	[System.Obsolete("Separated into Category Tags and EBlueprintOperation.")]
	public enum EBlueprintType
	{
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		TOOL,
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		APPAREL,
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		SUPPLY,
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		GEAR,

		[System.Obsolete("Separated into Category Tags and EBlueprintOperation.FillTargetItem.")]
		AMMO,

		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		BARRICADE,
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		STRUCTURE,
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		UTILITIES,
		[System.Obsolete("Only used for categorization. Please use Category Tags instead.")]
		FURNITURE,

		[System.Obsolete("Separated into Category Tags and EBlueprintOperation.RepairTargetItem.")]
		REPAIR,
	}

	/// <summary>
	/// Controls what blueprint does with input items.
	/// Separated from EBlueprintType which acted as both category AND operation.
	/// </summary>
	public enum EBlueprintOperation
	{
		/// <summary>
		/// No special modification to input items.
		/// </summary>
		None,

		/// <summary>
		/// Restore target input item to full quality.
		/// </summary>
		RepairTargetItem,

		/// <summary>
		/// Transfer amount from input items to target item.
		/// </summary>
		FillTargetItem,
	}

	public static class EBlueprintTypeEx
	{
		public static readonly CachingAssetRef[] legacyBlueprintTypeCategoryTagRefs = new CachingAssetRef[]
		{
			CachingAssetRef.Parse("ad1804b6945145f3b308738b0b8ea447"), // Tool
			CachingAssetRef.Parse("ebe755533bdd42d1871c3ac66b89530f"), // Apparel
			CachingAssetRef.Parse("d089feb7e43f40c5a7dfcefc36998cfb"), // Supply
			CachingAssetRef.Parse("cdb2df24b76d4c6e9d8411c940d8337f"), // Gear
			CachingAssetRef.Parse("d739926736374e5ba34b4ac6ffbb5c8f"), // Ammo
			CachingAssetRef.Parse("31a59b5fec3f4ec5b2887b1ce4acb029"), // Barricade
			CachingAssetRef.Parse("71d9e182c18b4aad8e87778e4f621995"), // Structure
			CachingAssetRef.Parse("bfac6026305f4737a95fd275ebff65a6"), // Utilities
			CachingAssetRef.Parse("b0c6cc0a8b4346be89aef697ecdb8e46"), // Furniture
			CachingAssetRef.Parse("732ee6ffeb18418985cf4f9fde33dd11"), // Repair
		};

		public static readonly CachingAssetRef salvageCategoryTagRef = CachingAssetRef.Parse("7ed29f9101ae4523a3b2e389414b7bd9");

#pragma warning disable
		public static CachingAssetRef GetCategoryTagRef(this EBlueprintType type)
		{
			return legacyBlueprintTypeCategoryTagRefs[(int) type];
		}

		public static TagAsset GetCategoryTag(this EBlueprintType type)
		{
			ref CachingAssetRef tagRef = ref legacyBlueprintTypeCategoryTagRefs[(int) type];
			return tagRef.Get<TagAsset>();
		}
#pragma warning restore
	}
}
