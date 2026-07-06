////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemArrestEndAsset : ItemAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		protected ushort _recover;
		public ushort recover => _recover;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (_recover != 0)
			{
				ItemArrestStartAsset arrestStartAsset = Assets.find(EAssetType.ITEM, _recover) as ItemArrestStartAsset;
				if (arrestStartAsset != null)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ArrestEnd_UnlocksItem", "<color=" + Palette.hex(ItemTool.getRarityColorUI(arrestStartAsset.rarity)) + ">" + arrestStartAsset.itemName + "</color>"), DescSort_Important);
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = p.bundle.load<AudioClip>("Use");

			_recover = p.data.ParseUInt16("Recover");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/ArrestEnd
			// Game data for ArrestEnd Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("ArrestEnd");
			data.Append("GUID", GUID); // Key

			data.Append("Recover", recover);
		}
	}
}
