////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemLibraryAsset : ItemBarricadeAsset
	{
		protected uint _capacity;
		public uint capacity => _capacity;

		protected byte _tax;
		public byte tax => _tax;

		public override byte[] getState(EItemOrigin origin)
		{
			return new byte[20];
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_capacity = p.data.ParseUInt32("Capacity");
			_tax = p.data.ParseUInt8("Tax");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Library
			// Game data for Library Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Library");
			data.Append("GUID", GUID); // Key

			data.Append("Capacity", capacity);
			data.Append("Tax", tax);
		}
	}
}
