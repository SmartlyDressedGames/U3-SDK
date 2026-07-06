////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class VehicleTier
	{
		private List<VehicleSpawn> _table;
		public List<VehicleSpawn> table => _table;

		//private string _name;
		//public string name
		//{
		//	get { return _name; }
		//}
		public string name;

		public float chance;

		public void addVehicle(ushort id)
		{
			if (table.Count == byte.MaxValue)
			{
				return;
			}

			for (byte index = 0; index < table.Count; index++)
			{
				if (table[index].vehicle == id)
				{
					return;
				}
			}

			table.Add(new VehicleSpawn(id));
		}

		public void removeVehicle(byte index)
		{
			table.RemoveAt(index);
		}

		public VehicleTier(List<VehicleSpawn> newTable, string newName, float newChance)
		{
			_table = newTable;
			name = newName;

			chance = newChance;
		}
	}
}