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
	public class EconSortMode_Rarity : Comparer<SteamItemDetails_t>
	{
		public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
		{
			SDG.Provider.UnturnedEconInfo.ERarity rarity_x = Provider.provider.economyService.getInventoryRarity(x.m_iDefinition.m_SteamItemDef);
			SDG.Provider.UnturnedEconInfo.ERarity rarity_y = Provider.provider.economyService.getInventoryRarity(y.m_iDefinition.m_SteamItemDef);
			int rarityComparison = rarity_x.CompareTo(rarity_y);
			if (rarityComparison == 0)
			{
				string name_x = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
				string name_y = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
				return name_x.CompareTo(name_y);
			}
			else
			{
				return -rarityComparison;
			}
		}
	}

	/// <summary>
	/// Sorts name alphabetically to the front of the list.
	/// </summary>
	public class EconSortMode_Name : Comparer<SteamItemDetails_t>
	{
		public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
		{
			string name_x = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
			string name_y = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
			return name_x.CompareTo(name_y);
		}
	}

	/// <summary>
	/// Sorts type alphabetically to the front of the list.
	/// </summary>
	public class EconSortMode_Type : Comparer<SteamItemDetails_t>
	{
		public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
		{
			string type_x = Provider.provider.economyService.getInventoryType(x.m_iDefinition.m_SteamItemDef);
			string type_y = Provider.provider.economyService.getInventoryType(y.m_iDefinition.m_SteamItemDef);
			int typeComparison = type_x.CompareTo(type_y);
			if (typeComparison == 0)
			{
				string name_x = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
				string name_y = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
				return name_x.CompareTo(name_y);
			}
			else
			{
				return typeComparison;
			}
		}
	}
}
