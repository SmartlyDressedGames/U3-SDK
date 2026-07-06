////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public static class AssetIdListExporter
	{
		public static void Export()
		{
			List<Asset> allAssets = new List<Asset>();
			Assets.FindAssetsByType_UseDefaultAssetMapping(allAssets);

			string basePath = Path.Join(ReadWrite.PATH, "Extras", "AssetIDs");
			ReadWrite.createFolder(basePath, false);

			string allAssetsPath = Path.Join(basePath, "All Assets");
			ReadWrite.createFolder(allAssetsPath, false);

			ExportAssetsToCsv(allAssets, Path.Join(allAssetsPath, "All Assets.csv"));
			ExportAssetsToCsvGroupedByLegacyCategory(allAssetsPath, allAssets);
			ExportAssetsToCsvGroupedByType(allAssetsPath, allAssets);

			foreach (AssetOrigin origin in Assets.assetOrigins)
			{
				if (origin.assets.IsEmpty())
				{
					continue;
				}

				string originFolderName = PathEx.ReplaceInvalidFileNameChars(origin.name, '_');

				if (string.IsNullOrEmpty(originFolderName))
				{
					UnturnedLog.error($"Unable to export origin {origin.name} Asset IDs because file name would be empty");
					continue;
				}

				string originPath = Path.Join(basePath, originFolderName);
				ReadWrite.createFolder(originPath, false);

				string originAssetsPath = Path.Join(originPath, originFolderName + ".csv");

				ExportAssetsToCsv(origin.assets, originAssetsPath);
				ExportAssetsToCsvGroupedByLegacyCategory(originPath, origin.assets);
				ExportAssetsToCsvGroupedByType(originPath, origin.assets);
			}
		}

		private static void ExportAssetsToCsvGroupedByLegacyCategory(string basePath, List<Asset> assets)
		{
			basePath = Path.Join(basePath, "Grouped by Legacy Category");
			ReadWrite.createFolder(basePath, false);

			Dictionary<EAssetType, List<Asset>> groups = GroupAssetsByLegacyCategory(assets);
			foreach (KeyValuePair<EAssetType, List<Asset>> kvp in groups)
			{
				string legacyTypeName = kvp.Key.ToString();
				string csvPath = Path.Combine(basePath, legacyTypeName + ".csv");
				ExportAssetsToCsv(kvp.Value, csvPath);

				string availableLegacyIdsPath = Path.Combine(basePath, legacyTypeName + " Legacy ID Availability.csv");
				ExportLegacyIdAvailabilityToCsv(kvp.Key, kvp.Value, availableLegacyIdsPath);
			}
		}

		private static void ExportAssetsToCsvGroupedByType(string basePath, List<Asset> assets)
		{
			basePath = Path.Join(basePath, "Grouped by Type");
			ReadWrite.createFolder(basePath, false);

			Dictionary<System.Type, List<Asset>> groups = GroupAssetsByType(assets);
			foreach (KeyValuePair<System.Type, List<Asset>> kvp in groups)
			{
				string typeName = kvp.Value[0].GetTypeFriendlyName();
				string csvPath = Path.Combine(basePath, typeName + ".csv");
				ExportAssetsToCsv(kvp.Value, csvPath);
			}
		}

		private static Dictionary<EAssetType, List<Asset>> GroupAssetsByLegacyCategory(List<Asset> assets)
		{
			Dictionary<EAssetType, List<Asset>> result = new Dictionary<EAssetType, List<Asset>>();
			foreach (Asset asset in assets)
			{
				EAssetType legacyType = asset.assetCategory;
				if (!result.TryGetValue(legacyType, out List<Asset> resultList))
				{
					resultList = new List<Asset>();
					result[legacyType] = resultList;
				}
				resultList.Add(asset);
			}
			return result;
		}

		private static Dictionary<System.Type, List<Asset>> GroupAssetsByType(List<Asset> assets)
		{
			Dictionary<System.Type, List<Asset>> result = new Dictionary<System.Type, List<Asset>>();
			foreach (Asset asset in assets)
			{
				System.Type actualType = asset.GetType();
				if (!result.TryGetValue(actualType, out List<Asset> resultList))
				{
					resultList = new List<Asset>();
					result[actualType] = resultList;
				}
				resultList.Add(asset);
			}
			return result;
		}

		private static void ExportAssetsToCsv(IEnumerable<Asset> assets, string csvPath)
		{
			using (FileStream fs = new FileStream(csvPath, FileMode.Create, FileAccess.Write))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				sw.WriteLine("Name,GUID,Type,Origin,Legacy ID,Legacy Category");

				foreach (Asset asset in assets)
				{
					WriteEscapedString(sw, asset.FriendlyName);
					sw.Write(',');
					sw.Write(asset.GUID.ToString("N"));
					sw.Write(',');
					sw.Write(asset.GetTypeFriendlyName());
					sw.Write(',');
					WriteEscapedString(sw, asset.GetOriginName());
					sw.Write(',');
					sw.Write(asset.id);
					sw.Write(',');
					sw.WriteLine(asset.assetCategory.ToString());
				}
			}
		}

		private static void ExportLegacyIdAvailabilityToCsv(EAssetType legacyType, IEnumerable<Asset> assets, string csvPath)
		{
			Dictionary<int, List<Asset>> legacyIdTable = new Dictionary<int, List<Asset>>();
			foreach (Asset asset in assets)
			{
				if (legacyIdTable.TryGetValue(asset.id, out List<Asset> assetsWithId))
				{
					assetsWithId.Add(asset);
				}
				else
				{
					assetsWithId = new List<Asset>() { asset };
					legacyIdTable.Add(asset.id, assetsWithId);
				}
			}

			using (FileStream fs = new FileStream(csvPath, FileMode.Create, FileAccess.Write))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				sw.WriteLine("Legacy ID,Used By,Reserved for Vanilla");

				int vanillaLimit = 0;
				switch (legacyType)
				{
					case EAssetType.ITEM:
						vanillaLimit = 2000;
						break;
					case EAssetType.EFFECT:
						vanillaLimit = 200;
						break;
					case EAssetType.RESOURCE:
						vanillaLimit = 50;
						break;
					case EAssetType.ANIMAL:
						vanillaLimit = 50;
						break;
					case EAssetType.MYTHIC:
						vanillaLimit = 500;
						break;
					case EAssetType.SKIN:
						vanillaLimit = 2000;
						break;
					case EAssetType.NPC:
						vanillaLimit = 2000;
						break;
				}

				for (int legacyId = 1; legacyId <= ushort.MaxValue; ++legacyId)
				{
					sw.Write(legacyId);
					sw.Write(',');

					if (legacyIdTable.TryGetValue(legacyId, out List<Asset> assetsWithId))
					{
						string names = assetsWithId[0].FriendlyName;
						for (int assetIndex = 1; assetIndex < assetsWithId.Count; ++assetIndex)
						{
							names += ", ";
							names += assetsWithId[assetIndex].FriendlyName;
						}
						WriteEscapedString(sw, names);
					}
					else
					{
						sw.Write("---");
					}
					sw.Write(',');

					if (legacyId < vanillaLimit)
					{
						if (legacyType == EAssetType.ITEM && (legacyId == 786 || (legacyId >= 1800 && legacyId <= 1806)))
						{
							sw.Write("Hawaii Overlap");
						}
						else
						{
							sw.Write("Reserved");
						}
					}
					sw.WriteLine();
				}
			}
		}

		private static void WriteEscapedString(StreamWriter sw, string value)
		{
			sw.Write('"');
			foreach (char c in value)
			{
				if (c == '"')
				{
					sw.Write('"');
				}
				sw.Write(c);
			}
			sw.Write('"');
		}
	}
}
