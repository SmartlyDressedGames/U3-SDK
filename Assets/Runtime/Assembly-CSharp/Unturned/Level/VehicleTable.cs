////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class VehicleTable
	{
		private List<VehicleTier> _tiers;
		public List<VehicleTier> tiers => _tiers;

		private Color _color;
		public Color color
		{
			get => _color;

			set
			{
				_color = value;

				for (ushort vehicleIndex = 0; vehicleIndex < LevelVehicles.spawns.Count; vehicleIndex++)
				{
					VehicleSpawnpoint spawn = LevelVehicles.spawns[vehicleIndex];

					if (spawn.type == EditorSpawns.selectedVehicle)
					{
						spawn.node.GetComponent<Renderer>().material.color = color;
						spawn.node.Find("Arrow").GetComponent<Renderer>().material.color = color;
					}
				}

				EditorSpawns.vehicleSpawn.GetComponent<Renderer>().material.color = color;
				EditorSpawns.vehicleSpawn.Find("Arrow").GetComponent<Renderer>().material.color = color;
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
				tiers.Add(new VehicleTier(new List<VehicleSpawn>(), name, 1));
			}
			else
			{
				tiers.Add(new VehicleTier(new List<VehicleSpawn>(), name, 0));
			}
		}

		public void removeTier(int tierIndex)
		{
			updateChance(tierIndex, 0);

			tiers.RemoveAt(tierIndex);
		}

		public void addVehicle(byte tierIndex, ushort id)
		{
			tiers[tierIndex].addVehicle(id);
		}

		public void removeVehicle(byte tierIndex, byte vehicleIndex)
		{
			tiers[tierIndex].removeVehicle(vehicleIndex);
		}

		/// <summary>
		/// Resolve spawn table asset if set, otherwise find asset by legacy in-editor ID configuration.
		/// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
		/// vehicle spawner requires to properly set paint color.
		/// </summary>
		public Asset GetRandomAsset()
		{
			if (tableID != 0)
			{
				// Note: we don't use FindVehicleByLegacyIdAndHandleRedirects here because the actual vehicle spawning
				// needs to use the redirector (if any) to set paint color.
				return SpawnTableTool.Resolve(tableID, EAssetType.VEHICLE, OnGetSpawnTableErrorContext);
			}
			else
			{
				return Assets.find(EAssetType.VEHICLE, GetRandomLegacyVehicleId());
			}
		}

		public void buildTable()
		{
			List<VehicleTier> sorted = new List<VehicleTier>();

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

		public VehicleTable(string newName)
		{
			_tiers = new List<VehicleTier>();
			_color = Color.white;
			name = newName;
			tableID = 0;
		}

		public VehicleTable(List<VehicleTier> newTiers, Color newColor, string newName, ushort newTableID)
		{
			_tiers = newTiers;
			_color = newColor;
			name = newName;
			tableID = newTableID;
		}

		/// <summary>
		/// Used when spawn table asset is not assigned. Pick a random legacy ID using in-editor list of spawns.
		/// </summary>
		private ushort GetRandomLegacyVehicleId()
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
					VehicleTier tier = tiers[index];

					if (tier.table.Count > 0)
					{
						return tier.table[Random.Range(0, tier.table.Count)].vehicle;
					}
					else
					{
						return 0;
					}
				}
			}

			VehicleTier backupTier = tiers[Random.Range(0, tiers.Count)];

			if (backupTier.table.Count > 0)
			{
				return backupTier.table[Random.Range(0, backupTier.table.Count)].vehicle;
			}
			else
			{
				return 0;
			}
		}

		private string OnGetSpawnTableErrorContext()
		{
			return $"\"{Level.info.name}\" vehicle table \"{name}\"";
		}

		internal string OnGetSpawnTableValidationErrorContext()
		{
			return $"\"{Level.info.name}\" vehicle table \"{name}\" validation";
		}

		[System.Obsolete("GetRandomAsset should be used instead because it properly supports guids in spawn assets.")]
		public ushort getVehicle()
		{
			if (tableID != 0)
			{
				// Note: we don't use FindVehicleByLegacyIdAndHandleRedirects here because the actual vehicle spawning
				// needs to use the redirector (if any) to set paint color.
				return SpawnTableTool.ResolveLegacyId(tableID, EAssetType.VEHICLE, OnGetSpawnTableErrorContext);
			}
			else
			{
				return GetRandomLegacyVehicleId();
			}
		}
	}
}
