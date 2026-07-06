////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class PlayerSavedata
	{
		public static bool hasSync;

		public static void writeData(SteamPlayerID playerID, string path, Data data)
		{
			if (hasSync)
			{
				ReadWrite.writeData("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, false, data);
			}
			else
			{
				ServerSavedata.writeData("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, data);
			}
		}

		public static Data readData(SteamPlayerID playerID, string path)
		{
			if (hasSync)
			{
				return ReadWrite.readData("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, false);
			}
			else
			{
				return ServerSavedata.readData("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path);
			}
		}

		public static void writeBlock(SteamPlayerID playerID, string path, Block block)
		{
			if (hasSync)
			{
				ReadWrite.writeBlock("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, false, block);
			}
			else
			{
				ServerSavedata.writeBlock("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, block);
			}
		}

		public static Block readBlock(SteamPlayerID playerID, string path, byte prefix)
		{
			if (hasSync)
			{
				return ReadWrite.readBlock("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, false, prefix);
			}
			else
			{
				return ServerSavedata.readBlock("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, prefix);
			}
		}

		public static River openRiver(SteamPlayerID playerID, string path, bool isReading)
		{
			if (hasSync)
			{
				return new River("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, true, false, isReading);
			}
			else
			{
				return ServerSavedata.openRiver("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, isReading);
			}
		}

		public static void deleteFile(SteamPlayerID playerID, string path)
		{
			if (hasSync)
			{
				ReadWrite.deleteFile("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, false);
			}
			else
			{
				ServerSavedata.deleteFile("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path);
			}
		}

		public static bool fileExists(SteamPlayerID playerID, string path)
		{
			if (hasSync)
			{
				return ReadWrite.fileExists("/Sync/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path, false);
			}
			else
			{
				return ServerSavedata.fileExists("/Players/" + playerID.steamID + "_" + playerID.characterID + "/" + Level.info.name + path);
			}
		}

		/// <summary>
		/// Delete all savedata folders for player's characters.
		/// </summary>
		public static void deleteFolder(SteamPlayerID playerID)
		{
			int numCharacters = Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS;
			if (hasSync)
			{
				for (int characterIndex = 0; characterIndex < numCharacters; ++characterIndex)
				{
					string characterDir = "/Sync/" + playerID.steamID + "_" + characterIndex;
					if (ReadWrite.folderExists(characterDir, false))
					{
						ReadWrite.deleteFolder(characterDir, false);
					}
				}
			}
			else
			{
				for (int characterIndex = 0; characterIndex < numCharacters; ++characterIndex)
				{
					string characterDir = "/Players/" + playerID.steamID + "_" + characterIndex;
					if (ServerSavedata.folderExists(characterDir))
					{
						ServerSavedata.deleteFolder(characterDir);
					}
				}
			}
		}
	}
}
