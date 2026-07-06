////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class VendorBuyingNameAscendingComparator : IComparer<VendorBuying>
	{
		public int Compare(VendorBuying a, VendorBuying b)
		{
			string name_a = a.displayName;
			string name_b = b.displayName;

			if (name_a == null || name_b == null)
			{
				return 0;
			}

			return name_a.CompareTo(name_b);
		}
	}

	/// <summary>
	/// Represents an item the vendor is buying from players.
	/// </summary>
	public class VendorBuying : VendorElement
	{
		private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

		public ItemAsset FindItemAsset()
		{
#pragma warning disable
			return Assets.FindItemByGuidOrLegacyId<ItemAsset>(TargetAssetGuid, id);
#pragma warning restore
		}

		public override string displayName
		{
			get
			{
				ItemAsset asset = FindItemAsset();
				return asset != null ? asset.itemName : null;
			}
		}

		public override string displayDesc
		{
			get
			{
				if (descriptionOverride != null)
				{
					return descriptionOverride;
				}
				else
				{
					ItemAsset asset = FindItemAsset();
					return asset != null ? asset.itemDescription : null;
				}
			}
		}

		public override EItemRarity rarity
		{
			get
			{
				ItemAsset asset = FindItemAsset();
				return asset != null ? asset.rarity : EItemRarity.COMMON;
			}
		}

		public bool canSell(Player player)
		{
			ItemAsset asset = FindItemAsset();
			if (asset == null)
				return false;

			using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
			{
				player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);

				ushort total = 0;
				foreach (PlayerInventorySearchResultV2 searchResult in scope.PooledResults)
				{
					total += searchResult.Jar.item.amount;
				}

				return total >= asset.MaxAmount;
			}
		}

		public void sell(Player player)
		{
			ItemAsset asset = FindItemAsset();
			if (asset == null)
				return;

			using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
			{
				player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);
				scope.PooledResults.Sort(qualityAscendingComparator);

				int total = asset.MaxAmount;
				foreach (PlayerInventorySearchResultV2 check in scope.PooledResults)
				{
					uint amountDeleted = check.DeleteAmount(player, (uint) total);
					total -= (int) amountDeleted;
					if (total == 0)
					{
						break;
					}
				}

				if (outerAsset.currency.isValid)
				{
					ItemCurrencyAsset currencyAsset = outerAsset.currency.Find();
					if (currencyAsset != null)
					{
						currencyAsset.grantValue(player, cost);
					}
				}
				else
				{
					player.skills.askAward(cost);
				}
			}
		}

		public void format(Player player, out ushort total, out byte amount)
		{
			ItemAsset asset = FindItemAsset();
			if (asset == null)
			{
				total = 0;
				amount = 0;
				return;
			}

			using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
			{
				player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);

				total = 0;
				for (byte searchIndex = 0; searchIndex < scope.PooledResults.Count; searchIndex++)
				{
					total += scope.PooledResults[searchIndex].Jar.item.amount;
				}

				amount = asset.MaxAmountAsByte;
			}
		}

		public VendorBuying(VendorAsset newOuterAsset, byte newIndex, System.Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride)
			: base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditionsList, newRewardsList, newDescriptionOverride)
		{ }
	}
}
