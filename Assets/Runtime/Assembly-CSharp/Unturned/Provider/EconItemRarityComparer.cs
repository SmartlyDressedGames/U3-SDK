////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Sorts higher rarity items into the front of the list.
	/// </summary>
	public class EconItemRarityComparer : Comparer<SteamItemDetails_t>
	{
		public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
		{
			EItemRarity rarity_x = Provider.provider.economyService.getGameRarity(x.m_iDefinition.m_SteamItemDef);
			EItemRarity rarity_y = Provider.provider.economyService.getGameRarity(y.m_iDefinition.m_SteamItemDef);
			int rarityComparison = rarity_x.CompareTo(rarity_y);
			if (rarityComparison == 0)
			{
				string name_x = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
				string name_y = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
				return -name_x.CompareTo(name_y); // Negative to match website view.
			}
			else
			{
				return -rarityComparison; // Negative to match website view.
			}
		}
	}
}
