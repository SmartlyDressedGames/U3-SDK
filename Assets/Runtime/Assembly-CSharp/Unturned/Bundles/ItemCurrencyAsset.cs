////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Associates items of the same currency, e.g. dollars or bullets.
	/// </summary>
	public class ItemCurrencyAsset : Asset
	{
		private static ItemCurrencyComparer valueComparer = new ItemCurrencyComparer();

		public struct Entry
		{
			public AssetReference<ItemAsset> item;
			public uint value;

			/// <summary>
			/// Should this item/value be shown in the list of vendor currency items?
			/// Useful to hide modded item stacks e.g. a stack of 100x $20 bills.
			/// </summary>
			public bool isVisibleInVendorMenu;
		}

		/// <summary>
		/// String to format value {0} into.
		/// </summary>
		public string valueFormat
		{
			get;
			protected set;
		}

		/// <summary>
		/// String to format value {0} of total {1} into if not otherwise specified in NPC condition.
		/// </summary>
		public string defaultConditionFormat
		{
			get;
			protected set;
		}

		public Entry[] entries
		{
			get;
			protected set;
		}

		/// <summary>
		/// Sum up value of each currency item in player's inventory.
		/// </summary>
		public uint getInventoryValue(Player player)
		{
			uint totalInventoryValue = 0;

			foreach (Entry entry in entries)
			{
				ItemAsset itemAsset = entry.item.Find();
				if (itemAsset == null)
					continue;

				using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
				{
					player.inventory.FindItemsByAsset(scope.PooledResults, itemAsset, false, true);
					foreach (PlayerInventorySearchResultV2 result in scope.PooledResults)
					{
						totalInventoryValue += result.Jar.item.amount * entry.value;
					}
				}
			}

			return totalInventoryValue;
		}

		/// <summary>
		/// Does player have access to items covering certain value?
		/// </summary>
		public bool canAfford(Player player, uint value)
		{
			return getInventoryValue(player) >= value;
		}

		/// <summary>
		/// Add items to player's inventory to reward value.
		/// </summary>
		public void grantValue(Player player, uint requiredValue)
		{
			if (requiredValue < 1)
			{
				return;
			}

			for (int index = entries.Length - 1; index >= 0; --index)
			{
				Entry entry = entries[index];
				ItemAsset itemAsset = entry.item.Find();
				if (itemAsset == null)
					continue;

				if (requiredValue < entry.value)
				{
					// e.g. we require 13, but this item is worth 20, so we have to find an item worth less.
					continue;
				}

				// Rounded-down amount of this item that we are looking for.
				// e.g. we require 43, this item is worth 20, so ideal amount is 2.
				uint requiredAmount = requiredValue / entry.value;

				ItemTool.tryForceGiveItem(player, itemAsset.id, (byte) requiredAmount);
				requiredValue -= requiredAmount * entry.value;

				if (requiredValue == 0)
				{
					return;
				}
			}
		}

		/// <summary>
		/// Remove items from player's inventory to pay required value.
		/// </summary>
		public bool spendValue(Player player, uint requiredValue)
		{
			if (canAfford(player, requiredValue) == false)
			{
				return false;
			}

			uint spentValue = 0;

			foreach (Entry entry in entries)
			{
				ItemAsset itemAsset = entry.item.Find();
				if (itemAsset == null)
					continue;

				uint valueRemaining = requiredValue - spentValue;

				// Rounded-up amount of this item that we are looking for.
				// e.g. we require 39, this item is worth 20, so ideal amount is 2.
				uint idealAmount = ((valueRemaining - 1) / entry.value) + 1;

				using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
				{
					player.inventory.FindItemsByAsset(scope.PooledResults, itemAsset, false, true);

					foreach (PlayerInventorySearchResultV2 item in scope.PooledResults)
					{
						uint amountDeleted = item.DeleteAmount(player, idealAmount);
						idealAmount -= amountDeleted;
						spentValue += amountDeleted * entry.value;

						if (idealAmount == 0)
						{
							break;
						}
					}
				}

				if (spentValue >= requiredValue)
				{
					break;
				}
			}

			// e.g. player had 2x $20 and spent $35 we owe them $5 back
			if (spentValue > requiredValue)
			{
				uint valueDue = spentValue - requiredValue;
				//UnturnedLog.info("Spent {0}, owed {1}", spentValue, valueDue);
				grantValue(player, valueDue);
			}
			else
			{
				//UnturnedLog.info("Spent {0}", spentValue);
			}

			return true;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			valueFormat = p.data.GetString("ValueFormat");
			defaultConditionFormat = p.data.GetString("DefaultConditionFormat");
			if (string.IsNullOrEmpty(defaultConditionFormat) && !string.IsNullOrEmpty(valueFormat))
			{
				defaultConditionFormat = valueFormat + " / " + valueFormat.Replace("{0", "{1");
			}

			if (p.data.TryGetList("Entries", out IDatList entryNodes))
			{
				int numberOfItems = entryNodes.Count;
				entries = new Entry[numberOfItems];
				for (int index = 0; index < numberOfItems; ++index)
				{
					Entry entry = new Entry();

					if (entryNodes[index] is IDatDictionary entryReader)
					{
						entry.item = entryReader.ParseStruct<AssetReference<ItemAsset>>("Item");
						entry.value = entryReader.ParseUInt32("Value");

						if (entryReader.ContainsKey("Is_Visible_In_Vendor_Menu"))
						{
							entry.isVisibleInVendorMenu = entryReader.ParseBool("Is_Visible_In_Vendor_Menu");
						}
						else
						{
							entry.isVisibleInVendorMenu = true;
						}
					}

					entries[index] = entry;
				}
			}
			else
			{
				entries = new Entry[0];
			}

			System.Array.Sort(entries, valueComparer); // Sort by value.
		}
	}

	/// <summary>
	/// Sort currency entries by value.
	/// </summary>
	internal class ItemCurrencyComparer : Comparer<ItemCurrencyAsset.Entry>
	{
		public override int Compare(ItemCurrencyAsset.Entry x, ItemCurrencyAsset.Entry y)
		{
			return x.value.CompareTo(y.value);
		}
	}
}
