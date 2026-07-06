////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Globalization;

namespace SDG.Unturned
{
	/// <summary>
	/// All main menu MTX shop code should be routed through here so that it could theoretically be ported to other
	/// platforms or stores. Obviously this is all very Steam specific at the moment, but at least the UI does not
	/// depend on Steam API here as much as older parts of the game.
	/// </summary>
	internal abstract class ItemStore
	{
		public static readonly UnityEngine.Color32 PremiumColor = new UnityEngine.Color32(0x64, 0xC8, 0x19, 255);

		public struct Listing
		{
			public int itemdefid;

			/// <summary>
			/// Was this item marked as new in the config?
			/// If new, and not marked as seen, then a "NEW" label is shown on the listing.
			/// </summary>
			public bool isNew; // Here in the struct for 4, 1, 8, 8 packing.

			public ulong currentPrice;
			public ulong basePrice;
		}

		public struct CartEntry
		{
			public int itemdefid;
			public int quantity;
		}

		public static ItemStore Get()
		{
			return instance;
		}

		public IEnumerable<CartEntry> GetCart()
		{
			return itemsInCart;
		}

		public bool IsCartEmpty => itemsInCart.IsEmpty();

		public abstract void ViewItem(int itemdefid);
		public abstract void ViewNewItems();
		public abstract void ViewStore();

		/// <summary>
		/// Do we have pricing details for a given item?
		/// Price results may not have been returned yet, or item might not be public.
		/// </summary>
		public bool FindListing(int itemdefid, out Listing listing)
		{
			foreach (Listing testListing in listings)
			{
				if (testListing.itemdefid == itemdefid)
				{
					listing = testListing;
					return true;
				}
			}

			listing = new Listing();
			return false;
		}

		public int GetQuantityInCart(int itemdefid)
		{
			foreach (CartEntry entry in itemsInCart)
			{
				if (entry.itemdefid == itemdefid)
					return entry.quantity;
			}
			return 0;
		}

		public void SetQuantityInCart(int itemdefid, int quantity)
		{
			for (int index = 0; index < itemsInCart.Count; ++index)
			{
				CartEntry entry = itemsInCart[index];
				if (entry.itemdefid == itemdefid)
				{
					if (quantity > 0)
					{
						entry.quantity = quantity;
						itemsInCart[index] = entry;
					}
					else
					{
						// Preserve order for cart menu.
						itemsInCart.RemoveAt(index);
					}

					OnCartChanged?.Invoke();
					return;
				}
			}

			if (quantity > 0)
			{
				CartEntry newEntry = new CartEntry();
				newEntry.itemdefid = itemdefid;
				newEntry.quantity = quantity;
				itemsInCart.Add(newEntry);
				OnCartChanged?.Invoke();
			}
		}

		public abstract event System.Action OnPricesReceived;

		/// <summary>
		/// Messy, but we only show a menu alert if there was a problem.
		/// </summary>
		public enum EPurchaseResult
		{
			UnableToInitialize,
			Denied,
		}

		public abstract event System.Action<EPurchaseResult> OnPurchaseResult;

		public event System.Action OnCartChanged;

		public abstract void RequestPrices();
		public abstract void StartPurchase();

		/// <summary>
		/// Already filtered to only return locally known items which pass country restrictions.
		/// </summary>
		public Listing[] GetListings() { return listings; }

		/// <summary>
		/// Empty if outside new time window.
		/// </summary>
		public int[] GetNewListingIndices() { return newListingIndices; }

		public bool HasNewListings => newListingIndices != null && newListingIndices.Length > 0;

		public int[] GetFeaturedListingIndices() { return featuredListingIndices; }

		public bool HasFeaturedListings => featuredListingIndices != null && featuredListingIndices.Length > 0;

		public int[] GetDiscountedListingIndices() { return discountedListingIndices; }

		public bool HasDiscountedListings => discountedListingIndices != null && discountedListingIndices.Length > 0;

		public int[] GetUnownedDiscountedBundleListingIndices() { return unownedDiscountedBundleListingIndices; }

		public int[] GetExcludedListingIndices() { return exludedListingIndices; }

		public string FormatPrice(ulong price)
		{
			return (price / 100.0).ToString("C", numberFormatInfo);
		}

		public string FormatDiscount(ulong currentPrice, ulong basePrice)
		{
			double ratio = currentPrice / (double) basePrice;
			return (ratio - 1.0).ToString("P0", numberFormatInfo);
		}

		private static ItemStore instance = new SteamItemStore();

		protected int FindListingIndex(int itemdefid)
		{
			for (int index = 0; index < listings.Length; ++index)
			{
				if (listings[index].itemdefid == itemdefid)
				{
					return index;
				}
			}

			return -1;
		}

		protected void RefreshNewItems()
		{
#if !DEDICATED_SERVER
			int[] newItemdefids = LiveConfig.Get().itemStore.newItems;
			if (newItemdefids != null && newItemdefids.Length > 0)
			{
				List<int> pendingNewListings = new List<int>(newItemdefids.Length);
				foreach (int itemdefid in newItemdefids)
				{
					int listingIndex = FindListingIndex(itemdefid);
					if (listingIndex >= 0)
					{
						listings[listingIndex].isNew = true;
						pendingNewListings.Add(listingIndex);
					}
				}

				// Can be empty if new items did not pass country restrictions, or are not purchasable yet.
				newListingIndices = pendingNewListings.ToArray();
			}
			else
			{
				newListingIndices = null;
			}
#endif // !DEDICATED_SERVER
		}

		protected void RefreshFeaturedItems()
		{
#if !DEDICATED_SERVER
			int[] featuredItemdefids = LiveConfig.Get().itemStore.featuredItems;
			if (featuredItemdefids != null && featuredItemdefids.Length > 0)
			{
				List<int> pendingFeaturedListings = new List<int>(featuredItemdefids.Length);
				foreach (int itemdefid in featuredItemdefids)
				{
					int listingIndex = FindListingIndex(itemdefid);
					if (listingIndex >= 0)
					{
						pendingFeaturedListings.Add(listingIndex);
					}
				}

				// Can be empty if items did not pass country restrictions, or are not purchasable yet.
				featuredListingIndices = pendingFeaturedListings.ToArray();
			}
			else
			{
				featuredListingIndices = null;
			}
#endif // !DEDICATED_SERVER
		}

		protected void RefreshExcludedItems()
		{
#if !DEDICATED_SERVER
			int[] excludedItemdefids = LiveConfig.Get().itemStore.excludeItemsFromHighlight;
			if (excludedItemdefids != null && excludedItemdefids.Length > 0)
			{
				List<int> pendingExcludedListings = new List<int>();
				foreach (int itemdefid in excludedItemdefids)
				{
					int listingIndex = FindListingIndex(itemdefid);
					if (listingIndex >= 0)
					{
						pendingExcludedListings.Add(listingIndex);
					}
				}

				// Can be empty if items did not pass country restrictions, or are not purchasable yet.
				exludedListingIndices = pendingExcludedListings.ToArray();
			}
			else
			{
				exludedListingIndices = null;
			}
#endif // !DEDICATED_SERVER
		}

		protected NumberFormatInfo numberFormatInfo;
		protected Listing[] listings;
		/// <summary>
		/// Subset of listings.
		/// </summary>
		protected int[] newListingIndices;
		/// <summary>
		/// Subset of listings.
		/// </summary>
		protected int[] featuredListingIndices;
		/// <summary>
		/// Subset of listings.
		/// </summary>
		protected int[] discountedListingIndices;
		/// <summary>
		/// Subset of listings.
		/// </summary>
		protected int[] exludedListingIndices;
		/// <summary>
		/// Subset of listings.
		/// </summary>
		protected int[] unownedDiscountedBundleListingIndices;
		protected List<CartEntry> itemsInCart = new List<CartEntry>();
	}
}
