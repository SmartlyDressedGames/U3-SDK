////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class AnimalTier
	{
		private List<AnimalSpawn> _table;
		public List<AnimalSpawn> table => _table;

		//private string _name;
		//public string name
		//{
		//	get { return _name; }
		//}
		public string name;

		public float chance;

		public void addAnimal(ushort id)
		{
			if (table.Count == byte.MaxValue)
			{
				return;
			}

			for (byte index = 0; index < table.Count; index++)
			{
				if (table[index].animal == id)
				{
					return;
				}
			}

			table.Add(new AnimalSpawn(id));
		}

		public void removeAnimal(byte index)
		{
			table.RemoveAt(index);
		}

		public AnimalTier(List<AnimalSpawn> newTable, string newName, float newChance)
		{
			_table = newTable;
			name = newName;

			chance = newChance;
		}
	}
}