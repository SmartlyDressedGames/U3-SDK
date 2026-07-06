////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class SpawnTableTool
	{
		/// <summary>
		/// Returning an Asset rather than the older IDs allows GUIDs to be used.
		/// legacyTargetAssetType is required for compatibility with spawn tables using legacy 16-bit IDs. If set to
		/// None and the spawn asset uses legacy IDs a warning is logged explaining GUIDs are necessary.
		/// </summary>
		/// <returns></returns>
		public static Asset Resolve(SpawnAsset spawnAsset, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
		{
			if (spawnAsset == null)
			{
				if (Assets.shouldLoadAnyAssets)
				{
					UnturnedLog.error($"{errorContextCallback?.Invoke() ?? "Unknown"} attempted to resolve null spawn table");
				}
				return null;
			}

			int depth = 0;
			while (depth < 32)
			{
				SpawnTable randomEntry = spawnAsset.PickRandomEntry(errorContextCallback);
				if (randomEntry == null)
				{
					UnturnedLog.warn($"Spawn table \"{spawnAsset.name}\" from {spawnAsset.GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} returned null entry");
					return null;
				}

				if (randomEntry.legacySpawnId != 0)
				{
					SpawnAsset nextSpawnAsset = Assets.find(EAssetType.SPAWN, randomEntry.legacySpawnId) as SpawnAsset;
					if (nextSpawnAsset == null)
					{
						// We don't check shouldLoadAnyAssets here because we wouldn't have reached this point otherwise.
						UnturnedLog.warn($"Spawn table \"{spawnAsset.name}\" from {spawnAsset.GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} unable to find table matching legacy spawn ID {randomEntry.legacySpawnId}");
						return null;
					}
					spawnAsset = nextSpawnAsset;
				}
				else if (randomEntry.legacyAssetId != 0)
				{
					if (legacyTargetAssetType != EAssetType.NONE)
					{
						Asset resultAsset = Assets.find(legacyTargetAssetType, randomEntry.legacyAssetId);
						if (resultAsset == null)
						{
							// We don't check shouldLoadAnyAssets here because we wouldn't have reached this point otherwise.
							UnturnedLog.warn($"Spawn table \"{spawnAsset.name}\" from {spawnAsset.GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} unable to find asset matching legacy ID {randomEntry.legacyAssetId} in legacy category {legacyTargetAssetType}");
							return null;
						}
						return resultAsset;
					}
					else
					{
						UnturnedLog.warn($"Spawn table \"{spawnAsset.name}\" from {spawnAsset.GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} unable to use legacy ID {randomEntry.legacyAssetId} because context does not support legacy IDs");
						return null;
					}
				}
				else if (!randomEntry.targetGuid.IsEmpty())
				{
					Asset targetAsset = Assets.find(randomEntry.targetGuid);
					if (targetAsset == null)
					{
						// We don't check shouldLoadAnyAssets here because we wouldn't have reached this point otherwise.
						UnturnedLog.warn($"Spawn table \"{spawnAsset.name}\" from {spawnAsset.GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} unable to find asset matching GUID {randomEntry.targetGuid}");
						return null;
					}

					if (targetAsset is SpawnAsset nextSpawnAsset)
					{
						spawnAsset = nextSpawnAsset;
					}
					else
					{
						return targetAsset;
					}
				}
				else
				{
					// This may be intentional as a lot of curated maps have some empty spawn tables.
					return null;
				}

				++depth;
			}

			UnturnedLog.warn($"Spawn table \"{spawnAsset.name}\" from {spawnAsset.GetOriginName()} resolved by {errorContextCallback?.Invoke() ?? "Unknown"} may have encountered a recursive loop and has given up");
			return null;
		}

		public static TAsset Resolve<TAsset>(SpawnAsset spawnAsset, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
			where TAsset : Asset
		{
			return Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback) as TAsset;
		}

		/// <summary>
		/// Doesn't support spawn assets with legacy 16-bit IDs.
		/// </summary>
		public static Asset Resolve(SpawnAsset spawnAsset, System.Func<string> errorContextCallback)
		{
			return Resolve(spawnAsset, EAssetType.NONE, errorContextCallback);
		}

		public static Asset Resolve(System.Guid spawnAssetGuid, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
		{
			if (spawnAssetGuid.IsEmpty())
			{
				// Don't log an error for this because it was likely intentionally empty.
				return null;
			}

			SpawnAsset spawnAsset = Assets.find(spawnAssetGuid) as SpawnAsset;
			if (spawnAsset == null)
			{
				if (Assets.shouldLoadAnyAssets)
				{
					UnturnedLog.error($"Unable to find spawn table with guid {spawnAssetGuid} resolved by {errorContextCallback?.Invoke() ?? "Unknown"}");
				}
				return null;
			}

			return Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback);
		}

		public static TAsset Resolve<TAsset>(System.Guid spawnAssetGuid, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
			where TAsset : Asset
		{
			return Resolve(spawnAssetGuid, legacyTargetAssetType, errorContextCallback) as TAsset;
		}

		public static Asset Resolve(ushort spawnAssetLegacyId, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
		{
			if (spawnAssetLegacyId == 0)
			{
				// Don't log an error for this because it was likely intentionally empty.
				return null;
			}

			SpawnAsset spawnAsset = Assets.find(EAssetType.SPAWN, spawnAssetLegacyId) as SpawnAsset;
			if (spawnAsset == null)
			{
				if (Assets.shouldLoadAnyAssets)
				{
					UnturnedLog.error($"Unable to find spawn table with legacy ID {spawnAssetLegacyId} resolved by {errorContextCallback?.Invoke() ?? "Unknown"}");
				}
				return null;
			}

			return Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback);
		}

		public static TAsset Resolve<TAsset>(ushort spawnAssetLegacyId, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
			where TAsset : Asset
		{
			return Resolve(spawnAssetLegacyId, legacyTargetAssetType, errorContextCallback) as TAsset;
		}

		/// <summary>
		/// For backwards compatibility with features that still need a legacy ID rather than asset.
		/// </summary>
		public static ushort ResolveLegacyId(SpawnAsset spawnAsset, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
		{
			Asset resultAsset = Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback);
			return resultAsset?.id ?? 0;
		}

		/// <summary>
		/// For backwards compatibility with features that still need a legacy ID rather than asset.
		/// </summary>
		public static ushort ResolveLegacyId(System.Guid spawnAssetGuid, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
		{
			Asset resultAsset = Resolve(spawnAssetGuid, legacyTargetAssetType, errorContextCallback);
			return resultAsset?.id ?? 0;
		}

		/// <summary>
		/// For backwards compatibility with features that still need a legacy ID rather than asset.
		/// </summary>
		public static ushort ResolveLegacyId(ushort spawnAssetLegacyId, EAssetType legacyTargetAssetType, System.Func<string> errorContextCallback)
		{
			Asset resultAsset = Resolve(spawnAssetLegacyId, legacyTargetAssetType, errorContextCallback);
			return resultAsset?.id ?? 0;
		}

		[System.Obsolete("Please update to the newer Resolve methods with legacyTargetAssetType parameter which support GUIDs")]
		public static ushort resolve(ushort id)
		{
			SpawnAsset spawnAsset = Assets.find(EAssetType.SPAWN, id) as SpawnAsset;

			if (spawnAsset == null)
			{
				if (Assets.shouldLoadAnyAssets)
				{
					UnturnedLog.error("Unable to find spawn table for resolve with id " + id);
				}

				return 0;
			}

			bool isSpawn;
			spawnAsset.resolve(out id, out isSpawn);

			if (isSpawn) // Is the ID it picked another spawn table?
			{
				id = resolve(id);
			}

			return id;
		}

		private static bool isVariantItemTier(ItemTier tier)
		{
			if (tier.table.Count < 6)
			{
				return false;
			}

			ItemAsset asset = Assets.find(EAssetType.ITEM, tier.table[0].item) as ItemAsset;

			if (asset == null)
			{
				return false;
			}

			int space = asset.itemName.IndexOf(" ");
			if (space <= 0)
			{
				return false;
			}

			string name = asset.itemName.Substring(space + 1);

			if (name.Length <= 1)
			{
				UnturnedLog.error(asset.itemName + " name has a trailing space!");
				return false;
			}

			for (int index = 1; index < tier.table.Count; index++)
			{
				ItemAsset other = Assets.find(EAssetType.ITEM, tier.table[index].item) as ItemAsset;

				if (!other.itemName.Contains(name))
				{
					return false;
				}
			}

			tier.name = name;
			return true;
		}

		private static bool isVariantVehicleTier(VehicleTier tier)
		{
			if (tier.table.Count < 6)
			{
				return false;
			}

			VehicleAsset asset = Assets.find(EAssetType.VEHICLE, tier.table[0].vehicle) as VehicleAsset;

			if (asset == null)
			{
				return false;
			}

			int space = asset.vehicleName.IndexOf(" ");
			if (space <= 0)
			{
				return false;
			}

			string name = asset.vehicleName.Substring(space + 1);

			if (name.Length <= 1)
			{
				UnturnedLog.error(asset.vehicleName + " name has a trailing space!");
				return false;
			}

			for (int index = 1; index < tier.table.Count; index++)
			{
				VehicleAsset other = Assets.find(EAssetType.VEHICLE, tier.table[index].vehicle) as VehicleAsset;

				if (!other.vehicleName.Contains(name))
				{
					return false;
				}
			}

			tier.name = name;
			return true;
		}

		private static void exportItems(string path, Data spawnsData, ref ushort id, bool isLegacy)
		{
			for (int tableIndex = 0; tableIndex < LevelItems.tables.Count; tableIndex++)
			{
				ItemTable table = LevelItems.tables[tableIndex];

				if (table.tableID != 0)
				{
					continue;
				}

				table.tableID = id;
				spawnsData.writeString(id.ToString(), Level.info.name + "_" + table.name);
				Data proxyData = new Data();
				proxyData.writeString("Type", "Spawn");
				proxyData.writeUInt16("ID", id++);

				if (ReadWrite.fileExists("/Bundles/Spawns/Items/" + table.name + "/" + table.name + ".dat", false, true))
				{
					Data tableData = ReadWrite.readData("/Bundles/Spawns/Items/" + table.name + "/" + table.name + ".dat", false, true);

					proxyData.writeInt32("Tables", 1);
					proxyData.writeUInt16("Table_0_Spawn_ID", tableData.readUInt16("ID"));
					proxyData.writeInt32("Table_0_Weight", 100);
				}
				else
				{
					proxyData.writeInt32("Tables", 1);
					proxyData.writeUInt16("Table_0_Spawn_ID", id);
					proxyData.writeInt32("Table_0_Weight", 100);

					spawnsData.writeString(id.ToString(), table.name);
					Data tableData = new Data();
					tableData.writeString("Type", "Spawn");
					tableData.writeUInt16("ID", id++);

					if (isLegacy)
					{
						if (table.tiers.Count > 1)
						{
							float lowest = float.MaxValue;

							for (int tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
							{
								ItemTier tier = table.tiers[tierIndex];

								if (tier.chance < lowest)
								{
									lowest = tier.chance;
								}
							}

							int weight = Mathf.CeilToInt(10.0f / lowest);

							tableData.writeInt32("Tables", table.tiers.Count);
							for (int tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
							{
								ItemTier tier = table.tiers[tierIndex];
								bool isVariantTier = isVariantItemTier(tier);

								if (isVariantTier && ReadWrite.fileExists("/Bundles/Spawns/Items/" + tier.name + "/" + tier.name + ".dat", false, true))
								{
									Data tierData = ReadWrite.readData("/Bundles/Spawns/Items/" + tier.name + "/" + tier.name + ".dat", false, true);

									tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", tierData.readUInt16("ID"));
									tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));
								}
								else if (isVariantTier && ReadWrite.fileExists(path + "/Items/" + tier.name + "/" + tier.name + ".dat", false, false))
								{
									Data tierData = ReadWrite.readData(path + "/Items/" + tier.name + "/" + tier.name + ".dat", false, false);

									tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", tierData.readUInt16("ID"));
									tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));
								}
								else
								{
									tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", id);
									tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));

									if (isVariantTier)
									{
										spawnsData.writeString(id.ToString(), tier.name);
									}
									else
									{
										spawnsData.writeString(id.ToString(), table.name + "_" + tier.name);
									}

									Data tierData = new Data();
									tierData.writeString("Type", "Spawn");
									tierData.writeUInt16("ID", id++);

									tierData.writeInt32("Tables", tier.table.Count);
									for (int spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
									{
										ItemSpawn spawn = tier.table[spawnIndex];

										tierData.writeUInt16("Table_" + spawnIndex + "_Asset_ID", spawn.item);
										tierData.writeInt32("Table_" + spawnIndex + "_Weight", 10);
									}

									if (isVariantTier)
									{
										ReadWrite.writeData(path + "/Items/" + tier.name + "/" + tier.name + ".dat", false, false, tierData);
									}
									else
									{
										ReadWrite.writeData(path + "/Items/" + table.name + "_" + tier.name + "/" + table.name + "_" + tier.name + ".dat", false, false, tierData);
									}
								}
							}
						}
						else
						{
							ItemTier tier = table.tiers[0];

							tableData.writeInt32("Tables", tier.table.Count);
							for (int spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
							{
								ItemSpawn spawn = tier.table[spawnIndex];

								tableData.writeUInt16("Table_" + spawnIndex + "_Asset_ID", spawn.item);
								tableData.writeInt32("Table_" + spawnIndex + "_Weight", 10);
							}
						}
					}

					ReadWrite.writeData(path + "/Items/" + table.name + "/" + table.name + ".dat", false, false, tableData);
				}

				ReadWrite.writeData(path + "/Items/" + Level.info.name + "_" + table.name + "/" + Level.info.name + "_" + table.name + ".dat", false, false, proxyData);
			}
		}

		private static void exportVehicles(string path, Data spawnsData, ref ushort id, bool isLegacy)
		{
			for (int tableIndex = 0; tableIndex < LevelVehicles.tables.Count; tableIndex++)
			{
				VehicleTable table = LevelVehicles.tables[tableIndex];

				if (table.tableID != 0)
				{
					continue;
				}

				table.tableID = id;
				spawnsData.writeString(id.ToString(), Level.info.name + "_" + table.name);
				Data proxyData = new Data();
				proxyData.writeString("Type", "Spawn");
				proxyData.writeUInt16("ID", id++);

				if (ReadWrite.fileExists("/Bundles/Spawns/Vehicles/" + table.name + "/" + table.name + ".dat", false, true))
				{
					Data tableData = ReadWrite.readData("/Bundles/Spawns/Vehicles/" + table.name + "/" + table.name + ".dat", false, true);

					proxyData.writeInt32("Tables", 1);
					proxyData.writeUInt16("Table_0_Spawn_ID", tableData.readUInt16("ID"));
					proxyData.writeInt32("Table_0_Weight", 100);
				}
				else
				{
					proxyData.writeInt32("Tables", 1);
					proxyData.writeUInt16("Table_0_Spawn_ID", id);
					proxyData.writeInt32("Table_0_Weight", 100);

					spawnsData.writeString(id.ToString(), table.name);
					Data tableData = new Data();
					tableData.writeString("Type", "Spawn");
					tableData.writeUInt16("ID", id++);

					if (isLegacy)
					{
						if (table.tiers.Count > 1)
						{
							float lowest = float.MaxValue;

							for (int tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
							{
								VehicleTier tier = table.tiers[tierIndex];

								if (tier.chance < lowest)
								{
									lowest = tier.chance;
								}
							}

							int weight = Mathf.CeilToInt(10.0f / lowest);

							tableData.writeInt32("Tables", table.tiers.Count);
							for (int tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
							{
								VehicleTier tier = table.tiers[tierIndex];
								bool isVariantTier = isVariantVehicleTier(tier);

								if (isVariantTier && ReadWrite.fileExists("/Bundles/Spawns/Vehicles/" + tier.name + "/" + tier.name + ".dat", false, true))
								{
									Data tierData = ReadWrite.readData("/Bundles/Spawns/Vehicles/" + tier.name + "/" + tier.name + ".dat", false, true);

									tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", tierData.readUInt16("ID"));
									tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));
								}
								else if (isVariantTier && ReadWrite.fileExists(path + "/Vehicles/" + tier.name + "/" + tier.name + ".dat", false, false))
								{
									Data tierData = ReadWrite.readData(path + "/Vehicles/" + tier.name + "/" + tier.name + ".dat", false, false);

									tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", tierData.readUInt16("ID"));
									tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));
								}
								else
								{
									tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", id);
									tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));

									if (isVariantTier)
									{
										spawnsData.writeString(id.ToString(), tier.name);
									}
									else
									{
										spawnsData.writeString(id.ToString(), table.name + "_" + tier.name);
									}

									Data tierData = new Data();
									tierData.writeString("Type", "Spawn");
									tierData.writeUInt16("ID", id++);

									tierData.writeInt32("Tables", tier.table.Count);
									for (int spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
									{
										VehicleSpawn spawn = tier.table[spawnIndex];

										tierData.writeUInt16("Table_" + spawnIndex + "_Asset_ID", spawn.vehicle);
										tierData.writeInt32("Table_" + spawnIndex + "_Weight", 10);
									}

									if (isVariantTier)
									{
										ReadWrite.writeData(path + "/Vehicles/" + tier.name + "/" + tier.name + ".dat", false, false, tierData);
									}
									else
									{
										ReadWrite.writeData(path + "/Vehicles/" + table.name + "_" + tier.name + "/" + table.name + "_" + tier.name + ".dat", false, false, tierData);
									}
								}
							}
						}
						else
						{
							VehicleTier tier = table.tiers[0];

							tableData.writeInt32("Tables", tier.table.Count);
							for (int spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
							{
								VehicleSpawn spawn = tier.table[spawnIndex];

								tableData.writeUInt16("Table_" + spawnIndex + "_Asset_ID", spawn.vehicle);
								tableData.writeInt32("Table_" + spawnIndex + "_Weight", 10);
							}
						}
					}

					ReadWrite.writeData(path + "/Vehicles/" + table.name + "/" + table.name + ".dat", false, false, tableData);
				}

				ReadWrite.writeData(path + "/Vehicles/" + Level.info.name + "_" + table.name + "/" + Level.info.name + "_" + table.name + ".dat", false, false, proxyData);
			}
		}

		private static void exportZombies(string path, Data spawnsData, ref ushort id, bool isLegacy)
		{
			for (int tableIndex = 0; tableIndex < LevelZombies.tables.Count; tableIndex++)
			{
				ZombieTable table = LevelZombies.tables[tableIndex];

				if (table.lootID == 0 && table.lootIndex < LevelItems.tables.Count)
				{
					table.lootID = LevelItems.tables[table.lootIndex].tableID;
				}
			}
		}

		private static void exportAnimals(string path, Data spawnsData, ref ushort id, bool isLegacy)
		{
			for (int tableIndex = 0; tableIndex < LevelAnimals.tables.Count; tableIndex++)
			{
				AnimalTable table = LevelAnimals.tables[tableIndex];

				if (table.tableID != 0)
				{
					continue;
				}

				table.tableID = id;
				spawnsData.writeString(id.ToString(), Level.info.name + "_" + table.name);
				Data proxyData = new Data();
				proxyData.writeString("Type", "Spawn");
				proxyData.writeUInt16("ID", id++);

				if (ReadWrite.fileExists("/Bundles/Spawns/Animals/" + table.name + "/" + table.name + ".dat", false, true))
				{
					Data tableData = ReadWrite.readData("/Bundles/Spawns/Animals/" + table.name + "/" + table.name + ".dat", false, true);

					proxyData.writeInt32("Tables", 1);
					proxyData.writeUInt16("Table_0_Spawn_ID", tableData.readUInt16("ID"));
					proxyData.writeInt32("Table_0_Weight", 100);
				}
				else
				{
					proxyData.writeInt32("Tables", 1);
					proxyData.writeUInt16("Table_0_Spawn_ID", id);
					proxyData.writeInt32("Table_0_Weight", 100);

					spawnsData.writeString(id.ToString(), table.name);
					Data tableData = new Data();
					tableData.writeString("Type", "Spawn");
					tableData.writeUInt16("ID", id++);

					if (isLegacy)
					{
						if (table.tiers.Count > 1)
						{
							float lowest = float.MaxValue;

							for (int tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
							{
								AnimalTier tier = table.tiers[tierIndex];

								if (tier.chance < lowest)
								{
									lowest = tier.chance;
								}
							}

							int weight = Mathf.CeilToInt(10.0f / lowest);

							tableData.writeInt32("Tables", table.tiers.Count);
							for (int tierIndex = 0; tierIndex < table.tiers.Count; tierIndex++)
							{
								AnimalTier tier = table.tiers[tierIndex];

								tableData.writeUInt16("Table_" + tierIndex + "_Spawn_ID", id);
								tableData.writeInt32("Table_" + tierIndex + "_Weight", (int) (tier.chance * weight));

								spawnsData.writeString(id.ToString(), table.name + "_" + tier.name);
								Data tierData = new Data();
								tierData.writeString("Type", "Spawn");
								tierData.writeUInt16("ID", id++);

								tierData.writeInt32("Tables", tier.table.Count);
								for (int spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
								{
									AnimalSpawn spawn = tier.table[spawnIndex];

									tierData.writeUInt16("Table_" + spawnIndex + "_Asset_ID", spawn.animal);
									tierData.writeInt32("Table_" + spawnIndex + "_Weight", 10);
								}

								ReadWrite.writeData(path + "/Animals/" + table.name + "_" + tier.name + "/" + table.name + "_" + tier.name + ".dat", false, false, tierData);
							}
						}
						else
						{
							AnimalTier tier = table.tiers[0];

							tableData.writeInt32("Tables", tier.table.Count);
							for (int spawnIndex = 0; spawnIndex < tier.table.Count; spawnIndex++)
							{
								AnimalSpawn spawn = tier.table[spawnIndex];

								tableData.writeUInt16("Table_" + spawnIndex + "_Asset_ID", spawn.animal);
								tableData.writeInt32("Table_" + spawnIndex + "_Weight", 10);
							}
						}
					}

					ReadWrite.writeData(path + "/Animals/" + table.name + "/" + table.name + ".dat", false, false, tableData);
				}

				ReadWrite.writeData(path + "/Animals/" + Level.info.name + "_" + table.name + "/" + Level.info.name + "_" + table.name + ".dat", false, false, proxyData);
			}
		}

		public static void export(ushort id, bool isLegacy)
		{
			string path = Level.info.path;

			if (isLegacy)
			{
				path += "/Exported_Legacy_Spawn_Tables";
			}
			else
			{
				path += "/Exported_Proxy_Spawn_Tables";
			}

			if (ReadWrite.folderExists(path, false))
			{
				ReadWrite.deleteFolder(path, false);
			}

			Data spawnsData = new Data();
			spawnsData.writeString("ID", "Spawn");

			exportItems(path, spawnsData, ref id, isLegacy);
			exportVehicles(path, spawnsData, ref id, isLegacy);
			exportZombies(path, spawnsData, ref id, isLegacy);
			exportAnimals(path, spawnsData, ref id, isLegacy);

			spawnsData.isCSV = true;
			ReadWrite.writeData(path + "/IDs.csv", false, false, spawnsData);
		}

		public static void LogAllSpawnTables()
		{
			List<SpawnAsset> spawnAssets = new List<SpawnAsset>(1000);
			Assets.find(spawnAssets);
			UnturnedLog.info($"Dumping {spawnAssets.Count} spawn tables:");

			for (int spawnIndex = 0; spawnIndex < spawnAssets.Count; ++spawnIndex)
			{
				SpawnAsset asset = spawnAssets[spawnIndex];

				if (asset == null)
				{
					UnturnedLog.error("null entry in spawnAssets list???");
					continue;
				}

				if (asset.tables == null || asset.tables.Count < 1)
				{
					UnturnedLog.info($"[{spawnIndex + 1} of {spawnAssets.Count}] {asset.name} is empty");
					continue;
				}

				UnturnedLog.info($"[{spawnIndex + 1} of {spawnAssets.Count}] {asset.name} has {asset.tables.Count} children:");
				for (int childIndex = 0; childIndex < asset.tables.Count; ++childIndex)
				{
					SpawnTable child = asset.tables[childIndex];

					string targetName;
					if (child.legacySpawnId != 0)
					{
						SpawnAsset targetSpawnAsset = Assets.find(EAssetType.SPAWN, child.legacySpawnId) as SpawnAsset;
						string targetSpawnName = targetSpawnAsset?.name ?? $"Unknown ID {child.legacySpawnId}";
						targetName = $"{targetSpawnName} (Spawn)";
					}
					else if (child.legacyAssetId != 0)
					{
						ItemAsset targetItemAsset = Assets.find(EAssetType.ITEM, child.legacyAssetId) as ItemAsset;
						VehicleAsset targetVehicleAsset = VehicleTool.FindVehicleByLegacyIdAndHandleRedirects(child.legacyAssetId);
						AnimalAsset targetAnimalAsset = Assets.find(EAssetType.ANIMAL, child.legacyAssetId) as AnimalAsset;
						string targetItemName = targetItemAsset?.FriendlyName ?? $"Unknown ID {child.legacyAssetId}";
						string targetVehicleName = targetVehicleAsset?.FriendlyName ?? $"Unknown ID {child.legacyAssetId}";
						string targetAnimalName = targetAnimalAsset?.FriendlyName ?? $"Unknown ID {child.legacyAssetId}";
						targetName = $"{targetItemName} (Item) or {targetVehicleName} (Vehicle) or {targetAnimalName} (Animal) depending on context";
					}
					else if (!child.targetGuid.IsEmpty())
					{
						Asset targetAsset = Assets.find(child.targetGuid);
						if (targetAsset != null)
						{
							targetName = $"{targetAsset.FriendlyName} ({targetAsset.GetTypeFriendlyName()})";
						}
						else
						{
							targetName = $"Unknown GUID {child.targetGuid}";
						}
					}
					else
					{
						targetName = "Empty";
					}

					float chance = child.normalizedWeight;
					if (childIndex > 0)
					{
						chance -= asset.tables[childIndex - 1].normalizedWeight;
					}

					UnturnedLog.info($"[{spawnIndex + 1} of {spawnAssets.Count}][{childIndex + 1} of {asset.tables.Count}] {chance:P} {targetName}");
				}
			}
		}
	}
}
