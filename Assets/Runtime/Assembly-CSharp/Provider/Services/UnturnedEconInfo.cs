////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider
{
	public class UnturnedEconInfo
	{
		public string name;
		public string display_type;
		public string description;
		public string name_color;
		public int itemdefid;
		public bool marketable;
		public int scraps;
		public System.Guid target_game_asset_guid;
		public int item_skin;
		public int item_effect;
		public EQuality quality;

		/// <summary>
		/// EItemType
		/// </summary>
		public int econ_type;

		/// <summary>
		/// Nelson 2024-12-06: This was added 2023-06-19, so unfortunately it will be inaccurate for older items.
		/// </summary>
		public System.DateTime creationTimeUtc;

		public bool isEligibleForPromotion = true;

		public enum EQuality
		{
			None = 0,
			Common = 1,
			Uncommon = 2,
			Gold = 3,
			Rare = 4,
			Epic = 5,
			Legendary = 6,
			Mythical = 7,
			Premium = 8,
			Achievement = 9,
		}

		/// <summary>
		/// This enum exists for sorting items based on rarity, and is derived from quality.
		/// Quality order cannot be changed due to loading from older files, but this one is ordered
		/// from lowest rarity to highest rarity and should match entries in quality.
		/// </summary>
		public enum ERarity
		{
			Common,
			Uncommon,
			Achievement,
			Unknown,
			Gold,
			Premium,
			Rare,
			Epic,
			Legendary,
			Mythical,
		}

		public UnturnedEconInfo()
		{
			name = "";
			display_type = "";
			description = "";
			name_color = "";
			itemdefid = 0;
			scraps = 0;
			target_game_asset_guid = System.Guid.Empty;
			item_skin = 0;
			item_effect = 0;
			quality = EQuality.None;
			econ_type = -1;
		}
	}
}
