////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Tracks whether we should show the "NEW" label on listings and item store button.
	/// </summary>
	public static class ItemStoreSavedata
	{
		public static bool WasNewCraftingPageSeen()
		{
#if !DEDICATED_SERVER
			LiveConfigData liveConfig = LiveConfig.Get();
			if (liveConfig.craftingPromotionId <= 0)
			{
				// No current promotion.
				return true;
			}

			long seenId;
			if (ConvenientSavedata.get().read("CraftingSeenPromotionId", out seenId) && seenId >= liveConfig.craftingPromotionId)
			{
				// Alert has been dismissed by the player.
				return true;
			}
			else
			{
				return false;
			}
#else // !DEDICATED_SERVER
			return true;
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Track that player has seen the new crafting blueprints.
		/// </summary>
		public static void MarkNewCraftingPageSeen()
		{
#if !DEDICATED_SERVER
			LiveConfigData liveConfig = LiveConfig.Get();
			ConvenientSavedata.get().write("CraftingSeenPromotionId", liveConfig.craftingPromotionId);
#endif // !DEDICATED_SERVER
		}

		public static bool WasNewListingsPageSeen()
		{
#if !DEDICATED_SERVER
			long seenId;
			ItemStoreLiveConfig liveConfig = LiveConfig.Get().itemStore;
			if (ConvenientSavedata.get().read("ItemStoreSeenPromotionId", out seenId) && seenId >= liveConfig.promotionId)
			{
				// Alert has been dismissed by the player.
				return true;
			}
			else
			{
				return false;
			}
#else // !DEDICATED_SERVER
			return true;
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Track that player has seen the page with all new listings.
		/// </summary>
		public static void MarkNewListingsPageSeen()
		{
#if !DEDICATED_SERVER
			ItemStoreLiveConfig liveConfig = LiveConfig.Get().itemStore;
			ConvenientSavedata.get().write("ItemStoreSeenPromotionId", liveConfig.promotionId);
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Has player seen the given listing?
		/// </summary>
		public static bool WasNewListingSeen(int itemdefid)
		{
			string flag = FormatNewListingSeenFlag(itemdefid);
			return ConvenientSavedata.get().hasFlag(flag);
		}

		/// <summary>
		/// Track that the player has seen the given listing.
		/// </summary>
		public static void MarkNewListingSeen(int itemdefid)
		{
			string flag = FormatNewListingSeenFlag(itemdefid);
			ConvenientSavedata.get().setFlag(flag);
		}

		private static string FormatNewListingSeenFlag(int itemdefid)
		{
			return "New_Listing_Seen_" + itemdefid;
		}
	}
}
