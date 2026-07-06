////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class AnimalTable
	{
		private List<AnimalTier> _tiers;
		public List<AnimalTier> tiers => _tiers;

		private Color _color;
		public Color color
		{
			get => _color;

			set
			{
				_color = value;

				for (ushort animalIndex = 0; animalIndex < LevelAnimals.spawns.Count; animalIndex++)
				{
					AnimalSpawnpoint spawn = LevelAnimals.spawns[animalIndex];

					if (spawn.type == EditorSpawns.selectedAnimal)
					{
						spawn.node.GetComponent<Renderer>().material.color = color;
					}
				}

				EditorSpawns.animalSpawn.GetComponent<Renderer>().material.color = color;
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
				tiers.Add(new AnimalTier(new List<AnimalSpawn>(), name, 1));
			}
			else
			{
				tiers.Add(new AnimalTier(new List<AnimalSpawn>(), name, 0));
			}
		}

		public void removeTier(int tierIndex)
		{
			updateChance(tierIndex, 0);

			tiers.RemoveAt(tierIndex);
		}

		public void addAnimal(byte tierIndex, ushort id)
		{
			tiers[tierIndex].addAnimal(id);
		}

		public void removeAnimal(byte tierIndex, byte animalIndex)
		{
			tiers[tierIndex].removeAnimal(animalIndex);
		}

		public ushort getAnimal()
		{
			if (tableID != 0)
			{
				return SpawnTableTool.ResolveLegacyId(tableID, EAssetType.ANIMAL, OnGetSpawnTableErrorContext);
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
						AnimalTier tier = tiers[index];

						if (tier.table.Count > 0)
						{
							return tier.table[Random.Range(0, tier.table.Count)].animal;
						}
						else
						{
							return 0;
						}
					}
				}

				AnimalTier backupTier = tiers[Random.Range(0, tiers.Count)];

				if (backupTier.table.Count > 0)
				{
					return backupTier.table[Random.Range(0, backupTier.table.Count)].animal;
				}
				else
				{
					return 0;
				}
			}
		}

		public void buildTable()
		{
			List<AnimalTier> sorted = new List<AnimalTier>();

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

		public AnimalTable(string newName)
		{
			_tiers = new List<AnimalTier>();
			_color = Color.white;
			name = newName;
			tableID = 0;
		}

		public AnimalTable(List<AnimalTier> newTiers, Color newColor, string newName, ushort newTableID)
		{
			_tiers = newTiers;
			_color = newColor;
			name = newName;
			tableID = newTableID;
		}

		private string OnGetSpawnTableErrorContext()
		{
			return $"\"{Level.info.name}\" animal table \"{name}\"";
		}

		internal string OnGetSpawnTableValidationErrorContext()
		{
			return $"\"{Level.info.name}\" animal table \"{name}\" validation";
		}
	}
}
