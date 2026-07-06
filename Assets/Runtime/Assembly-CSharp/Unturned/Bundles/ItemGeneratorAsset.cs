////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemGeneratorAsset : ItemBarricadeAsset
	{
		protected ushort _capacity;
		public ushort capacity => _capacity;

		protected float _wirerange;
		public float wirerange => _wirerange;

		protected float _burn;
		/// <summary>
		/// Seconds to wait between burning one unit of fuel.
		/// </summary>
		public float burn => _burn;

		public override byte[] getState(EItemOrigin origin)
		{
			return new byte[3];
			// powered
			// 1-2 fuel
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FuelCapacity", capacity), DescSort_Important);

			if (burn > 0.0f)
			{
				const float SECONDS_PER_HOUR = 3600;
				float fuelPerHour = SECONDS_PER_HOUR / burn;
				int roundedFuelPerHour = Mathf.RoundToInt(fuelPerHour);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FuelBurnRate", roundedFuelPerHour), DescSort_Important);

				float maxRuntimeSeconds = burn * capacity;
				float maxRuntimeHours = maxRuntimeSeconds / SECONDS_PER_HOUR;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FuelMaxRuntime", maxRuntimeHours.ToString("0.00")), DescSort_Important);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_capacity = p.data.ParseUInt16("Capacity");

			_wirerange = p.data.ParseFloat("Wirerange");
			if (wirerange > PowerTool.MAX_POWER_RANGE + 0.1f)
			{
				Assets.ReportError(this, "Wirerange is further than the max supported power range of " + PowerTool.MAX_POWER_RANGE);
			}

			_burn = p.data.ParseFloat("Burn");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Generator
			// Game data for Generator Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Generator");
			data.Append("GUID", GUID); // Key

			data.Append("Capacity", capacity);
			data.Append("Wirerange", wirerange);
			data.Append("Burn", burn);
		}
	}
}
