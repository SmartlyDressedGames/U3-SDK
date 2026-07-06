////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum ECraftingInputPrioritization
	{
		/// <summary>
		/// Sort items with lowest "amount" to front of list.
		/// </summary>
		LowestAmount,

		/// <summary>
		/// Sort items with highest "amount" to front of list.
		/// </summary>
		HighestAmount,

		/// <summary>
		/// Sort items with lowest quality% to front of list.
		/// </summary>
		LowestQuality,

		/// <summary>
		/// Sort items with highest quality% to front of list.
		/// </summary>
		HighestQuality,
	}

	public enum ECraftingInputCountingMethod
	{
		/// <summary>
		/// Sum up number of items found, ignoring amount.
		/// Default except as described in TotalAmount comment.
		/// </summary>
		TotalItems,

		/// <summary>
		/// Sum up "amount" of each item. Optionally counting zero as one (ShouldCountEmptyAsOne).
		/// Default for legacy "ammo type" blueprints and FillTargetItem operation.
		/// </summary>
		TotalAmount,
	}

	public class BlueprintSupply : System.IEquatable<BlueprintSupply>
	{
		/// <summary>
		/// Note: if calling ItemRef.Get() please use FindItemAsset instead to avoid redundant asset lookups.
		/// </summary>
		public CachingBcAssetRef ItemRef
		{
			get => _itemRef;
			internal set => _itemRef = value;
		}
		private CachingBcAssetRef _itemRef;

		internal bool _isCritical;
		public bool isCritical => _isCritical;

		/// <summary>
		/// If true, items with an "amount" of zero are included in eligible supplies as amount 1.
		/// In practice (as of 2025-03-03), items with zero amount are empty containers such as magazines.
		/// </summary>
		public bool ShouldCountEmptyAsOne
		{
			get;
			private set;
		}

		/// <summary>
		/// Determines how totalAmount of each input is calculated.
		/// </summary>
		public ECraftingInputCountingMethod CountingMethod
		{
			get;
			internal set;
		}

		/// <summary>
		/// If true, items with an "amount" of zero are included in eligible supplies.
		/// Otherwise, they are ignored (default).
		/// </summary>
		public bool ShouldIncludeEmptyAmount
		{
			get;
			internal set;
		}

		/// <summary>
		/// If true, items with an "amount" >= their MaxAmount are ignored. Otherwise, they are eligible (default).
		/// </summary>
		public bool ShouldExcludeFullAmount
		{
			get;
			internal set;
		}

		/// <summary>
		/// If true, items with quality of 100% are eligible (default). Otherwise, they are ignored.
		/// </summary>
		public bool ShouldIncludeMaxQuality
		{
			get;
			internal set;
		}

		/// <summary>
		/// Controls which items are used first. For example, whether to use the lowest quality items first.
		/// </summary>
		public ECraftingInputPrioritization Prioritization
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, delete input item. Defaults to true.
		/// Replaces the "tool" blueprint option.
		/// </summary>
		public bool ShouldConsume
		{
			get;
			internal set;
		}

		public int amount;

		public ItemAsset FindItemAsset()
		{
			return _itemRef.Get<ItemAsset>();
		}

		/// <summary>
		/// Does this blueprint input require the specified item?
		/// </summary>
		public bool IsItem(ItemAsset asset)
		{
			if (asset == null)
				return false;

			return _itemRef.IsReferenceTo(asset);
		}

		public bool Equals(BlueprintSupply other)
		{
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			if (ItemRef != other.ItemRef)
			{
				return false;
			}

			if (isCritical != other.isCritical)
			{
				return false;
			}

			if (ShouldCountEmptyAsOne != other.ShouldCountEmptyAsOne)
			{
				return false;
			}

			if (CountingMethod != other.CountingMethod)
			{
				return false;
			}

			if (ShouldIncludeEmptyAmount != other.ShouldIncludeEmptyAmount)
			{
				return false;
			}

			if (ShouldExcludeFullAmount != other.ShouldExcludeFullAmount)
			{
				return false;
			}

			if (ShouldIncludeMaxQuality != other.ShouldIncludeMaxQuality)
			{
				return false;
			}

			if (Prioritization != other.Prioritization)
			{
				return false;
			}

			if (ShouldConsume != other.ShouldConsume)
			{
				return false;
			}

			return true;
		}

		public override string ToString()
		{
			return $@"(Item: {_itemRef} Amount: {amount} Critical: {isCritical} CountEmptyAsOne: {ShouldCountEmptyAsOne}
 CountingMethod: {CountingMethod} IncludeEmpty: {ShouldIncludeEmptyAmount} ExcludeFull: {ShouldExcludeFullAmount}
 IncludeMaxQuality: {ShouldIncludeMaxQuality} Prioritization: {Prioritization} Consume: {ShouldConsume})";
		}

		public BlueprintSupply(ushort newID, bool newCritical, int newAmount, bool newTreatEmptyAsOne, ECraftingInputPrioritization newPrioritization)
		{
			_isCritical = newCritical;
			ShouldCountEmptyAsOne = newTreatEmptyAsOne;
			ShouldIncludeEmptyAmount = newTreatEmptyAsOne;
			Prioritization = newPrioritization;
			ShouldConsume = true;
			ShouldIncludeMaxQuality = true;

			amount = newAmount;
		}

		[System.Obsolete("Please use FindItemAsset instead for GUID support")]
		public ushort id => _itemRef.LegacyId;
	}
}
