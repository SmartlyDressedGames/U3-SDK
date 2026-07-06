////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemTankAsset : ItemBarricadeAsset
	{
		protected ETankSource _source;
		public ETankSource source => _source;

		protected ushort _resource;
		public ushort resource => _resource;

		private byte[] resourceState;

		public override byte[] getState(EItemOrigin origin)
		{
			byte[] state = new byte[2];

			if (origin == EItemOrigin.ADMIN)
			{
				state[0] = resourceState[0];
				state[1] = resourceState[1];
			}

			return state;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			switch (source)
			{
				case ETankSource.FUEL:
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FuelCapacity", resource), DescSort_Important);
					break;
				}

				case ETankSource.WATER:
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WaterCapacity", resource), DescSort_Important);
					break;
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_source = (ETankSource) System.Enum.Parse(typeof(ETankSource), p.data.GetString("Source"), true);

			_resource = p.data.ParseUInt16("Resource");
			resourceState = System.BitConverter.GetBytes(resource);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Tank
			// Game data for Tank Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Tank");
			data.Append("GUID", GUID); // Key

			data.Append("Source", source);
			data.Append("Resource", resource);
		}
	}
}
