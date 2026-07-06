////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class WorkshopTool
	{
		public static bool checkMapMeta(string path, bool usePath)
		{
			return ReadWrite.fileExists(path + "/Map.meta", false, usePath);
		}

		public static bool checkMapValid(string path, bool usePath)
		{
			string[] folders = ReadWrite.getFolders(path, usePath);
			if (folders.Length != 1)
				return false;

			return ReadWrite.fileExists(folders[0] + "/Level.dat", false, usePath);
		}

		private static bool findMapNestedPath(string basePath, string searchPath, out string path)
		{
			string[] levelDirectories = ReadWrite.getFolders(basePath, false);
			foreach (string levelPath in levelDirectories)
			{
				string potentialBundlesPath = levelPath + searchPath;
				if (ReadWrite.folderExists(potentialBundlesPath, false))
				{
					path = potentialBundlesPath;
					return true;
				}
			}

			path = null;
			return false;
		}

		/// <summary>
		/// Given path to a workshop map, try to find its /Bundles folder.
		/// </summary>
		public static bool findMapBundlesPath(string path, out string bundlesPath)
		{
			return findMapNestedPath(path, "/Bundles", out bundlesPath);
		}

		/// <summary>
		/// Given path to a workshop map, try to find its /Content folder.
		/// </summary>
		public static bool findMapContentPath(string path, out string contentPath)
		{
			return findMapNestedPath(path, "/Content", out contentPath);
		}

		[System.Obsolete]
		public static void loadMapBundlesAndContent(string workshopItemPath)
		{
			loadMapBundlesAndContent(workshopItemPath, 0);
		}

		/// <summary>
		/// Maps on the workshop are a root folder named after the published file id, containing
		/// the map folder itself with the level name. In order to load the map's bundles and content
		/// properly we need to find the nested Bundles and Content folders.
		/// </summary>
		public static void loadMapBundlesAndContent(string workshopItemPath, ulong workshopFileId)
		{
			string mapBundlesPath;
			if (findMapBundlesPath(workshopItemPath, out mapBundlesPath))
			{
				Assets.RequestAddSearchLocation(mapBundlesPath, SDG.Provider.TempSteamworksWorkshop.FindOrAddOrigin(workshopFileId));
			}
		}

		public static bool checkLocalizationMeta(string path, bool usePath)
		{
			return ReadWrite.fileExists(path + "/Localization.meta", false, usePath);
		}

		public static bool checkLocalizationValid(string path, bool usePath)
		{
			return ReadWrite.getFolders(path, usePath).Length > 0;
		}

		public static bool checkObjectMeta(string path, bool usePath)
		{
			return ReadWrite.fileExists(path + "/Object.meta", false, usePath);
		}

		public static bool checkItemMeta(string path, bool usePath)
		{
			return ReadWrite.fileExists(path + "/Item.meta", false, usePath);
		}

		public static bool checkVehicleMeta(string path, bool usePath)
		{
			return ReadWrite.fileExists(path + "/Vehicle.meta", false, usePath);
		}

		public static bool checkSkinMeta(string path, bool usePath)
		{
			return ReadWrite.fileExists(path + "/Skin.meta", false, usePath);
		}

		public static bool checkBundleValid(string path, bool usePath)
		{
			return ReadWrite.getFolders(path, usePath).Length > 0;
		}

		public static bool detectUGCMetaType(string path, bool usePath, out ESteamUGCType outType)
		{
			// Creating X.meta files was from 2014-2015 code, ideally we would switch
			// to some sort of UGC descriptor file rather than all these .meta files.
			if (checkMapMeta(path, usePath))
			{
				outType = ESteamUGCType.MAP;
			}
			else if (checkLocalizationMeta(path, usePath))
			{
				outType = ESteamUGCType.LOCALIZATION;
			}
			else if (checkObjectMeta(path, usePath))
			{
				outType = ESteamUGCType.OBJECT;
			}
			else if (checkItemMeta(path, false))
			{
				outType = ESteamUGCType.ITEM;
			}
			else if (checkVehicleMeta(path, false))
			{
				outType = ESteamUGCType.VEHICLE;
			}
			else
			{
				// Unknown/Invalid
				outType = ESteamUGCType.ITEM;
				return false;
			}

			// Invalid case exits from else, so we have a valid type here.
			return true;
		}
	}
}
