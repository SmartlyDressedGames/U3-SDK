////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemBagAsset : ItemClothingAsset
	{
		private byte _width;
		public byte width => _width;

		private byte _height;
		public byte height => _height;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (width > 0 && height > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_StorageDimensions", width, height), DescSort_Important);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (!isPro)
			{
				_width = p.data.ParseUInt8("Width");
				_height = p.data.ParseUInt8("Height");
			}
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Bag
			// Game data for Bag Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Bag");
			data.Append("GUID", GUID); // Key

			data.Append("Width", width);
			data.Append("Height", height);
		}
	}
}
