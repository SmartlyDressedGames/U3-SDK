////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class ItemTier
	{
		private List<ItemSpawn> _table;
		public List<ItemSpawn> table => _table;

		//private string _name;
		//public string name
		//{
		//	get { return _name; }
		//}
		public string name;

		public float chance;

		public void addItem(ushort id)
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

			table.Add(new ItemSpawn(id));
		}

		public void removeItem(byte index)
		{
			table.RemoveAt(index);
		}

		public ItemTier(List<ItemSpawn> newTable, string newName, float newChance)
		{
			_table = newTable;
			name = newName;

			chance = newChance;
		}
	}
}