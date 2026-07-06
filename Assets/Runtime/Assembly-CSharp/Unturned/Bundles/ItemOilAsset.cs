////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemOilPumpAsset : ItemBarricadeAsset
	{
		public ushort fuelCapacity
		{
			get;
			protected set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			fuelCapacity = p.data.ParseUInt16("Fuel_Capacity");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/OilPump
			// Game data for OilPump Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("OilPump");
			data.Append("GUID", GUID); // Key

			data.Append("Fuel_Capacity", fuelCapacity);
		}
	}
}
