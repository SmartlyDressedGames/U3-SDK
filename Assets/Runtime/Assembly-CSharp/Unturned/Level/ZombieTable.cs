////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ZombieTable
	{
		private ZombieSlot[] _slots;
		public ZombieSlot[] slots => _slots;

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
						for (ushort index = 0; index < LevelZombies.spawns[x, y].Count; index++)
						{
							ZombieSpawnpoint spawn = LevelZombies.spawns[x, y][index];

							if (spawn.type == EditorSpawns.selectedZombie)
							{
								spawn.node.GetComponent<Renderer>().material.color = color;
							}
						}

						EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = color;
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

		/// <summary>
		/// ID unique to this zombie table in the level. If this table is deleted the ID will not be recycled. Used to
		/// refer to zombie table from external files, e.g., NPC zombie kills condition.
		/// </summary>
		public int tableUniqueId
		{
			get;
			private set;
		}

		public bool isMega;
		public ushort health;
		public byte damage;
		public byte lootIndex;
		public ushort lootID;
		public uint xp;
		public float regen;

		private string _difficultyGUID;
		public string difficultyGUID
		{
			get => _difficultyGUID;
			set
			{
				_difficultyGUID = value;

				try
				{
					difficulty = new AssetReference<ZombieDifficultyAsset>(new System.Guid(difficultyGUID));
				}
				catch
				{
					difficulty = AssetReference<ZombieDifficultyAsset>.invalid;
				}
			}
		}

		public AssetReference<ZombieDifficultyAsset> difficulty
		{
			get;
			private set;
		}

		private ZombieDifficultyAsset cachedDifficulty = null;
		public ZombieDifficultyAsset resolveDifficulty()
		{
			if (cachedDifficulty == null && difficulty.isValid)
			{
				cachedDifficulty = Assets.find(difficulty);
			}

			return cachedDifficulty;
		}

		public void addCloth(byte slotIndex, ushort id)
		{
			slots[slotIndex].addCloth(id);
		}

		public void removeCloth(byte slotIndex, byte clothIndex)
		{
			slots[slotIndex].removeCloth(clothIndex);
		}

		internal void GetSpawnClothingParameters(out byte shirt, out byte pants, out byte hat, out byte gear)
		{
			shirt = 255;
			if (slots[0].table.Count > 0 && Random.value < slots[0].chance)
			{
				shirt = (byte) Random.Range(0, slots[0].table.Count);
			}

			pants = 255;
			if (slots[1].table.Count > 0 && Random.value < slots[1].chance)
			{
				pants = (byte) Random.Range(0, slots[1].table.Count);
			}

			hat = 255;
			if (slots[2].table.Count > 0 && Random.value < slots[2].chance)
			{
				hat = (byte) Random.Range(0, slots[2].table.Count);
			}

			gear = 255;
			if (slots[3].table.Count > 0 && Random.value < slots[3].chance)
			{
				gear = (byte) Random.Range(0, slots[3].table.Count);
			}
		}

		public ZombieTable(string newName)
		{
			// 0 Shirt
			// 1 Pants
			// 2 Hat
			// 3 Gear

			_slots = new ZombieSlot[4];
			for (int index = 0; index < slots.Length; index++)
			{
				slots[index] = new ZombieSlot(1f, new List<ZombieCloth>());
			}

			_color = Color.white;
			name = newName;

			isMega = false;
			health = 100;
			damage = 15;
			lootIndex = 0;
			lootID = 0;
			xp = 3;
			regen = 10.0f;
			difficultyGUID = string.Empty;
			tableUniqueId = LevelZombies.GenerateTableUniqueId();
		}

		public ZombieTable(ZombieSlot[] newSlots, Color newColor, string newName, bool newMega, ushort newHealth, byte newDamage, byte newLootIndex, ushort newLootID, uint newXP, float newRegen, string newDifficultyGUID, int newTableUniqueId)
		{
			_slots = newSlots;
			_color = newColor;
			name = newName;

			isMega = newMega;
			health = newHealth;
			damage = newDamage;
			lootIndex = newLootIndex;
			lootID = newLootID;
			xp = newXP;
			regen = newRegen;
			difficultyGUID = newDifficultyGUID;
			tableUniqueId = newTableUniqueId;
		}
	}
}
