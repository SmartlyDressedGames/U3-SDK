////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemGearAsset : ItemClothingAsset
	{
		/// <summary>
		/// If set, find a child meshrenderer with this name and change its material to the character hair material.
		/// </summary>
		public string hairOverride
		{
			get;
			protected set;
		}

		/// <summary>
		/// For items using hairOverride, the hair material color will default to this for players without the
		/// Gold Upgrade. (Since the Gold Upgrade is required for full RGB control, the default hair colors may
		/// look boring for items that cover the hair but aren't hair in of themselves.) Also used as the color
		/// in the cosmetic preview.
		/// </summary>
		public Color32? hairOverrideNonGoldColor
		{
			get;
			set;
		}

		/// <summary>
		/// If set, find a child meshrenderer with this name and change its material to the character beard material.
		/// </summary>
		public string BeardOverride
		{
			get;
			set;
		}

		/// <summary>
		/// For items using BeardOverride, the beard material color will default to this for players without the
		/// Gold Upgrade. (Since the Gold Upgrade is required for full RGB control, the default beard colors may
		/// look boring for items that cover the beard but aren't beards in of themselves.)
		/// Also used as the color in the cosmetic preview.
		/// </summary>
		public Color32? beardOverrideNonGoldColor
		{
			get;
			set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			// These values were originally only in gear assets, but mesh override shirts
			// need to be able to hide the hair and beard as well.
			hairVisible = p.data.ContainsKey("Hair");
			beardVisible = p.data.ContainsKey("Beard");

			hairOverride = p.data.GetString("Hair_Override");
			if (!string.IsNullOrEmpty(hairOverride) && p.data.TryParseColor32RGB("Hair_Override_NonGoldColor", out Color32 hairColor))
			{
				hairOverrideNonGoldColor = hairColor;
			}

			BeardOverride = p.data.GetString("Beard_Override");
			if (!string.IsNullOrEmpty(BeardOverride) && p.data.TryParseColor32RGB("Beard_Override_NonGoldColor", out Color32 beardColor))
			{
				beardOverrideNonGoldColor = beardColor;
			}
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Gear
			// Game data for Gear Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Gear");
			data.Append("GUID", GUID); // Key

			data.Append("Hair", hairVisible);
			data.Append("Beard", beardVisible);
		}
	}
}
