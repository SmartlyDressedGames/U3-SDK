////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemOpticAsset : ItemAsset
	{
		/// <summary>
		/// Factor e.g. 2 is a 2x multiplier.
		/// Prior to 2022-04-11 this was the target field of view. (90/fov)
		/// </summary>
		public float zoom
		{
			get;
			private set;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (zoom != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ZoomFactor", zoom), DescSort_ItemStat);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			zoom = Mathf.Max(1.0f, p.data.ParseFloat("Zoom"));
		}
	}
}
