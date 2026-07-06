////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class ZombieSlot
	{
		private List<ZombieCloth> _table;
		public List<ZombieCloth> table => _table;

		public float chance;

		public void addCloth(ushort id)
		{
			if (table.Count == byte.MaxValue)
			{
				return;
			}

			for (byte index = 0; index < table.Count; index++)
			{
				if (table[index].item == id)
				{
					return;
				}
			}

			table.Add(new ZombieCloth(id));
		}

		public void removeCloth(byte index)
		{
			table.RemoveAt(index);
		}

		public ZombieSlot(float newChance, List<ZombieCloth> newTable)
		{
			_table = newTable;

			chance = newChance;
		}
	}
}