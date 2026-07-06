////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemTable
	{
		private List<ItemTier> _tiers;
		public List<ItemTier> tiers => _tiers;

		private Color _color;
		public Color color
		{
			get => _color;

			set
			{
				_color = value;

				for (byte x = 0; x < Regions.WORLD_SIZE; x++)
				{
					for (byte y = 0; y < Regions.WORLD_SIZE; y++)
					{
						for (ushort itemIndex = 0; itemIndex < LevelItems.spawns[x, y].Count; itemIndex++)
						{
							ItemSpawnpoint spawn = LevelItems.spawns[x, y][itemIndex];

							if (spawn.type == EditorSpawns.selectedItem)
							{
								spawn.node.GetComponent<Renderer>().material.color = color;
							}
						}

						EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = color;
					}
				}
			}
		}

		//private string _name;
		//public string name
		//{
		//	get { return _name; }
		//}
		public string name;

		public ushort tableID;

		public void addTier(string name)
		{
			if (tiers.Count == byte.MaxValue)
			{
				return;
			}

			for (int index = 0; index < tiers.Count; index++)
			{
				if (tiers[index].name == name)
				{
					return;
				}
			}

			if (tiers.Count == 0)
			{
				tiers.Add(new ItemTier(new List<ItemSpawn>(), name, 1));
			}
			else
			{
				tiers.Add(new ItemTier(new List<ItemSpawn>(), name, 0));
			}
		}

		public void removeTier(int tierIndex)
		{
			updateChance(tierIndex, 0);

			tiers.RemoveAt(tierIndex);
		}

		public void addItem(byte tierIndex, ushort id)
		{
			tiers[tierIndex].addItem(id);
		}

		public void removeItem(byte tierIndex, byte itemIndex)
		{
			tiers[tierIndex].removeItem(itemIndex);
		}

		public ushort getItem()
		{
			if (tableID != 0)
			{
				return SpawnTableTool.ResolveLegacyId(tableID, EAssetType.ITEM, OnGetSpawnTableErrorContext);
			}
			else
			{
				float random = Random.value;

				if (tiers.Count == 0)
				{
					return 0;
				}

				for (int index = 0; index < tiers.Count; index++)
				{
					if (random < tiers[index].chance)
					{
						ItemTier tier = tiers[index];

						if (tier.table.Count > 0)
						{
							return tier.table[Random.Range(0, tier.table.Count)].item;
						}
						else
						{
							return 0;
						}
					}
				}

				ItemTier backupTier = tiers[Random.Range(0, tiers.Count)];

				if (backupTier.table.Count > 0)
				{
					return backupTier.table[Random.Range(0, backupTier.table.Count)].item;
				}
				else
				{
					return 0;
				}
			}
		}

		public void buildTable()
		{
			List<ItemTier> sorted = new List<ItemTier>();

			for (int tierIndex = 0; tierIndex < tiers.Count; tierIndex++)
			{
				if (sorted.Count == 0)
				{
					sorted.Add(tiers[tierIndex]);
					continue;
				}

				bool sort = false;
				for (int sortedIndex = 0; sortedIndex < sorted.Count; sortedIndex++)
				{
					if (tiers[tierIndex].chance < sorted[sortedIndex].chance)
					{
						sort = true;
						sorted.Insert(sortedIndex, tiers[tierIndex]);
						break;
					}
				}

				if (!sort)
				{
					sorted.Add(tiers[tierIndex]);
				}
			}

			float total = 0;
			for (int index = 0; index < sorted.Count; index++)
			{
				total += sorted[index].chance;
				sorted[index].chance = total;
			}

			_tiers = sorted;
		}

		public void updateChance(int tierIndex, float chance)
		{
			float change = chance - tiers[tierIndex].chance;
			tiers[tierIndex].chance = chance;

			if (tiers.Count < 2)
			{
				// Don't normalize tiers if this is the only entry. (public issue #4720)
				return;
			}

			float remaining = Mathf.Abs(change);
			while (remaining > 0.001f)
			{
				int others = 0;

				for (int index = 0; index < tiers.Count; index++)
				{
					if (((change < 0 && tiers[index].chance < 1) || (change > 0 && tiers[index].chance > 0)) && index != tierIndex)
					{
						others++;
					}
				}

				if (others == 0)
				{
					break;
				}

				float split = remaining / others;

				for (int index = 0; index < tiers.Count; index++)
				{
					if (((change < 0 && tiers[index].chance < 1) || (change > 0 && tiers[index].chance > 0)) && index != tierIndex)
					{
						if (change > 0)
						{
							if (tiers[index].chance >= split)
							{
								remaining -= split;
								tiers[index].chance -= split;
							}
							else
							{
								remaining -= tiers[index].chance;
								tiers[index].chance = 0;
							}
						}
						else
						{
							if (tiers[index].chance <= 1 - split)
							{
								remaining -= split;
								tiers[index].chance += split;
							}
							else
							{
								remaining -= 1 - tiers[index].chance;
								tiers[index].chance = 1;
							}
						}
					}
				}
			}

			float total = 0;
			for (int index = 0; index < tiers.Count; index++)
			{
				total += tiers[index].chance;
			}

			for (int index = 0; index < tiers.Count; index++)
			{
				tiers[index].chance /= total;
			}
		}

		public ItemTable(string newName)
		{
			_tiers = new List<ItemTier>();
			_color = Color.white;
			name = newName;
			tableID = 0;
		}

		public ItemTable(List<ItemTier> newTiers, Color newColor, string newName, ushort newTableID)
		{
			_tiers = newTiers;
			_color = newColor;
			name = newName;
			tableID = newTableID;
		}

		private string OnGetSpawnTableErrorContext()
		{
			return $"\"{Level.info.name}\" item table \"{name}\"";
		}

		internal string OnGetSpawnTableValidationErrorContext()
		{
			return $"\"{Level.info.name} item table \"{name}\" validation";
		}
	}
}
