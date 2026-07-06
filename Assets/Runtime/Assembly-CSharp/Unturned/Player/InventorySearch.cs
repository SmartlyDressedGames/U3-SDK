////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class InventorySearchQualityAscendingComparator : IComparer<InventorySearch>,
		IComparer<PlayerInventorySearchResultV2>
	{
		public int Compare(InventorySearch a, InventorySearch b)
		{
			return a.jar.item.quality - b.jar.item.quality;
		}

		public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
		{
			return a.Jar.item.quality - b.Jar.item.quality;
		}
	}

	public class InventorySearchQualityDescendingComparator : IComparer<InventorySearch>,
		IComparer<PlayerInventorySearchResultV2>
	{
		public int Compare(InventorySearch a, InventorySearch b)
		{
			return b.jar.item.quality - a.jar.item.quality;
		}

		public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
		{
			return b.Jar.item.quality - a.Jar.item.quality;
		}
	}

	public class InventorySearchAmountAscendingComparator : IComparer<InventorySearch>,
		IComparer<PlayerInventorySearchResultV2>
	{
		public int Compare(InventorySearch a, InventorySearch b)
		{
			return a.jar.item.amount - b.jar.item.amount;
		}

		public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
		{
			return a.Jar.item.amount - b.Jar.item.amount;
		}
	}

	public class InventorySearchAmountDescendingComparator : IComparer<InventorySearch>,
		IComparer<PlayerInventorySearchResultV2>
	{
		public int Compare(InventorySearch a, InventorySearch b)
		{
			return b.jar.item.amount - a.jar.item.amount;
		}

		public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
		{
			return b.Jar.item.amount - a.Jar.item.amount;
		}
	}

	/// <summary>
	/// Consolidates parameters for older, separate inventory search methods.
	/// 
	/// The "player" part of the name refers to the PlayerInventory-specific parameters. It can still be used to search
	/// the Items class, in which case those parameters do not apply.
	/// </summary>
	public struct PlayerInventorySearchParameters
	{
		/// <summary>
		/// List to populate with matching items.
		/// </summary>
		public List<PlayerInventorySearchResultV2> Results
		{
			get;
			set;
		}

		/// <summary>
		/// If true, search player's primary and secondary weapon slots.
		/// Only applicable when used with PlayerInventory class. (I.e., not Items class.)
		/// </summary>
		public bool IncludeEquipmentSlots
		{
			get;
			set;
		}

		/// <summary>
		/// If true, search storage container player is currently interacting with (if any).
		/// Only applicable when used with PlayerInventory class. (I.e., not Items class.)
		/// </summary>
		public bool IncludeActiveStorageContainer
		{
			get;
			set;
		}

		/// <summary>
		/// If greater than zero, search exits early once Results count meets MaxResultCount.
		/// </summary>
		public int MaxResultsCount
		{
			get;
			set;
		}

		/// <summary>
		/// If set, item must be this type to match.
		/// </summary>
		public EItemType? ItemType
		{
			get;
			set;
		}

		/// <summary>
		/// If set, AssetRef must be a reference to item's asset to match.
		/// Replaces older "id" parameter which matched if item's legacy asset ID was the same.
		/// </summary>
		public CachingBcAssetRef? AssetRef
		{
			get => _assetRef;
			set
			{
				if (value.HasValue)
				{
					// Otherwise Get() is called on a copy, in which case result would not be cached.
					CachingBcAssetRef assetRef = value.Value;
					assetRef.Get();
					_assetRef = assetRef;
				}
				else
				{
					_assetRef = value;
				}
			}
		}
		private CachingBcAssetRef? _assetRef;

		/// <summary>
		/// If true, items with amount of zero can match. Otherwise, they are ignored.
		/// Replaces older "findEmpty" parameter which matched if (findEmpty || amount > 0).
		/// </summary>
		public bool IncludeEmpty
		{
			get;
			set;
		}

		/// <summary>
		/// If true, items with an "amount" >= their MaxAmount are ignored. Otherwise, they can match (default).
		/// </summary>
		public bool ExcludeFullAmount
		{
			get;
			set;
		}

		/// <summary>
		/// If true, items with quality of 100% can match. Otherwise, they are ignored.
		/// Replaces older "findHealthy" parameter which matched if (findHealthy || quality < 100).
		/// </summary>
		public bool IncludeMaxQuality
		{
			get;
			set;
		}

		/// <summary>
		/// If set, item must be of type ItemCaliberAsset. Asset's caliber list must either:
		/// • Contain this caliber ID.
		/// • Or, if empty, IncludeUnspecifiedCaliber must be true.
		/// Otherwise, item is ignored.
		/// </summary>
		public ushort? CaliberId
		{
			get;
			set;
		}

		/// <summary>
		/// If set, item must be of type ItemCaliberAsset. Asset's caliber list must either:
		/// • Contain one of these caliber IDs.
		/// • Or, if empty, IncludeUnspecifiedCaliber must be true.
		/// Otherwise, item is ignored.
		/// </summary>
		public ushort[] AnyCaliberIds
		{
			get;
			set;
		}

		/// <summary>
		/// Only applicable if CaliberId or AnyCaliberIds is set.
		/// If true, assets with an empty calibers list can match. Otherwise, they are ignore.d
		/// </summary>
		public bool IncludeUnspecifiedCaliber
		{
			get;
			set;
		}

		/// <summary>
		/// If set, do not include this specific item instance in search results.
		/// Kind of hacked-in for ignoring "target item" as a potential input item.
		/// </summary>
		public ItemJar ItemToIgnore
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Nearly identical to InventorySearch aside from:
	/// • Struct instead of class to improve garbage collection performance in pooled lists.
	/// • More understandable name.
	/// • Provides reference to Items holding "Jar." Longer-term this should be preferred over the "Page" property.
	/// </summary>
	public struct PlayerInventorySearchResultV2
	{
		public byte Page => _jarOwner?.page ?? 0;

		private Items _jarOwner;
		public Items JarOwner => _jarOwner;

		private ItemJar _jar;
		public ItemJar Jar => _jar;

		public ItemAsset GetAsset()
		{
			return _jar != null ? _jar.GetAsset() : null;
		}

		public T GetAsset<T>() where T : ItemAsset
		{
			return _jar != null ? _jar.GetAsset<T>() : null;
		}

		public void DequipIfEquipped(Player player)
		{
			if (player.equipment.checkSelection(_jarOwner.page, _jar.x, _jar.y))
			{
				player.equipment.dequip();
			}
		}

		public override string ToString()
		{
			return $"(Page: {Page} X: {_jar?.x} Y: {_jar?.y} Item: {GetAsset()})";
		}

		public void Delete(Player player)
		{
			DequipIfEquipped(player);

			player.crafting.removeItem(_jarOwner.page, _jar);

			if (_jarOwner.page < PlayerInventory.SLOTS)
			{
				player.equipment.sendSlot(_jarOwner.page);
			}
		}

		/// <summary>
		/// Serverside delete an amount of this item.
		/// </summary>
		/// <param name="alwaysDeleteAtZeroAmount">False for crafting where original item can be kept, true when selling to vendors.</param>
		/// <returns>Total amount deleted.</returns>
		public uint DeleteAmount(Player player, uint desiredAmount, bool alwaysDeleteAtZeroAmount = true)
		{
			DequipIfEquipped(player);

			uint availableAmount = _jar.item.amount;
			if (availableAmount > desiredAmount)
			{
				player.inventory.sendUpdateAmount(_jarOwner.page, _jar.x, _jar.y, (byte) (_jar.item.amount - desiredAmount));
				return desiredAmount;
			}
			else
			{
				player.inventory.sendUpdateAmount(_jarOwner.page, _jar.x, _jar.y, 0);

				bool shouldDelete;
				if (alwaysDeleteAtZeroAmount)
				{
					shouldDelete = true;
				}
				else
				{
					ItemAsset emptiedAsset = GetAsset();
					shouldDelete = emptiedAsset?.ShouldDeleteAtZeroAmount ?? true;
				}

				if (shouldDelete)
				{
					player.crafting.removeItem(_jarOwner.page, _jar);

					if (_jarOwner.page < PlayerInventory.SLOTS)
					{
						player.equipment.sendSlot(_jarOwner.page);
					}
				}

				return availableAmount;
			}
		}

		public PlayerInventorySearchResultV2(Items newJarOwner, ItemJar newJar)
		{
			_jarOwner = newJarOwner;
			_jar = newJar;
		}
	}

	public static class PlayerInventorySearchResultV2ListEx
	{
		/// <summary>
		/// -1 if no eligible item is found.
		/// If includeMaxQuality is true an item with quality of 100 can be "lowest quality", otherwise item has to
		/// be less than 100 quality.
		/// </summary>
		public static int IndexOfItemWithLowestQuality(this List<PlayerInventorySearchResultV2> searchResults, bool includeMaxQuality = true)
		{
			byte lowestQuality = includeMaxQuality ? byte.MaxValue : (byte) 100;
			int indexOfItemWithLowestQuality = -1;

			for (int testIndex = 0; testIndex < searchResults.Count; ++testIndex)
			{
				if (searchResults[testIndex].Jar.item.quality < lowestQuality)
				{
					lowestQuality = searchResults[testIndex].Jar.item.quality;
					indexOfItemWithLowestQuality = testIndex;
				}
			}

			return indexOfItemWithLowestQuality;
		}
	}

	/// <summary>
	/// Please use PlayerInventorySearchResultV2 for better performance!
	/// </summary>
	public class InventorySearch
	{
		private byte _page;
		public byte page => _page;

		private ItemJar _jar;
		public ItemJar jar => _jar;

		public ItemAsset GetAsset()
		{
			return _jar != null ? _jar.GetAsset() : null;
		}

		public T GetAsset<T>() where T : ItemAsset
		{
			return _jar != null ? _jar.GetAsset<T>() : null;
		}

		private void dequipIfEquipped(Player player)
		{
			if (player.equipment.checkSelection(page, jar.x, jar.y))
			{
				player.equipment.dequip();
			}
		}

		/// <summary>
		/// Serverside delete an amount of this item.
		/// </summary>
		/// <returns>Total amount deleted.</returns>
		public uint deleteAmount(Player player, uint desiredAmount)
		{
			dequipIfEquipped(player);

			uint availableAmount = jar.item.amount;
			if (availableAmount > desiredAmount)
			{
				player.inventory.sendUpdateAmount(page, jar.x, jar.y, (byte) (jar.item.amount - desiredAmount));
				return desiredAmount;
			}
			else
			{
				player.inventory.sendUpdateAmount(page, jar.x, jar.y, 0);

				ItemAsset emptiedAsset = GetAsset();
				bool shouldDelete = emptiedAsset?.ShouldDeleteAtZeroAmount ?? true;
				if (shouldDelete)
				{
					player.crafting.removeItem(page, jar);

					if (page < PlayerInventory.SLOTS)
					{
						player.equipment.sendSlot(page);
					}
				}

				return availableAmount;
			}
		}

		public InventorySearch(byte newPage, ItemJar newJar)
		{
			_page = newPage;
			_jar = newJar;
		}
	}

	internal static class PlayerInventorySearchResultPool
	{
		public static List<PlayerInventorySearchResultV2> Claim()
		{
			if (pool.IsEmpty())
			{
				return new List<PlayerInventorySearchResultV2>();
			}
			else
			{
				List<PlayerInventorySearchResultV2> result = pool.GetAndRemoveTail();
				result.Clear();
				return result;
			}
		}

		public static void Release(List<PlayerInventorySearchResultV2> list)
		{
			pool.Add(list);
		}

		private static List<List<PlayerInventorySearchResultV2>> pool = new List<List<PlayerInventorySearchResultV2>>();
	}

	public struct ScopedPlayerInventorySearchResultPool : System.IDisposable
	{
		public List<PlayerInventorySearchResultV2> PooledResults
		{
			get
			{
				if (!hasClaimed)
				{
					hasClaimed = true;
					_results = PlayerInventorySearchResultPool.Claim();
				}
				return _results;
			}
		}

		public void Dispose()
		{
			if (_results != null)
			{
				PlayerInventorySearchResultPool.Release(PooledResults);
				_results = null;
			}
		}

		private bool hasClaimed;
		private List<PlayerInventorySearchResultV2> _results;
	}
}
