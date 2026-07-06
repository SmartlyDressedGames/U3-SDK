////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemFuelAsset : ItemAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		protected ushort _fuel;
		public ushort fuel => _fuel;

		public bool shouldDeleteAfterFillingTarget
		{
			get;
			protected set;
		}

		private bool shouldAlwaysSpawnFull;

		private byte[] fuelState;

		public override byte[] getState(EItemOrigin origin)
		{
			byte[] state = new byte[2];

			if (origin == EItemOrigin.ADMIN || shouldAlwaysSpawnFull)
			{
				state[0] = fuelState[0];
				state[1] = fuelState[1];
			};

			return state;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (itemInstance != null)
			{
				ushort stateFuel = System.BitConverter.ToUInt16(itemInstance.state, 0);
				float percentage = (float) stateFuel / (float) fuel;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FuelAmountWithCapacity", stateFuel, fuel, percentage.ToString("P")), DescSort_Important);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = p.bundle.load<AudioClip>("Use");

			_fuel = p.data.ParseUInt16("Fuel");
			fuelState = System.BitConverter.GetBytes(fuel);
			shouldDeleteAfterFillingTarget = p.data.ParseBool("Delete_After_Filling_Target");
			shouldAlwaysSpawnFull = p.data.ParseBool("Always_Spawn_Full");
		}
	}
}
