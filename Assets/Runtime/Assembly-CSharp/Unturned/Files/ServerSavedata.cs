////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ServerSavedata
	{
		public static string directoryName
		{
			get
			{
				if (Dedicator.IsDedicatedServer)
				{
					return "Servers";
				}
				else
				{
					return "Worlds";
				}
			}
		}

		public static string directory
		{
			get
			{
				if (Dedicator.IsDedicatedServer)
				{
					return "/Servers";
				}
				else
				{
					return "/Worlds";
				}
			}
		}

		public static string transformPath(string path)
		{
			return directory + "/" + Provider.serverID + path;
		}

		/// <summary>
		/// If the file already exists when writing we will move it to this path. (public issue #4636)
		/// Appends '~' so that terminal autocomplete finds the original file first.
		/// Use GetBackupFilePathV1 for the earlier "example-backup.dat" style.
		/// </summary>
		public static string GetBackupFilePath(string filePath)
		{
			return filePath + '~';
		}

		/// <summary>
		/// To assist with getting "example-backup.dat" path from prior to "example.dat~" change.
		/// </summary>
		public static string GetBackupFilePathV1(string filePath)
		{
			int extIndex = filePath.LastIndexOf('.');
			if (extIndex < 0)
			{
				return filePath + "-backup";
			}
			else
			{
				return filePath.Insert(extIndex, "-backup");
			}
		}

		public static void serializeJSON<T>(string path, T instance)
		{
			string filePath = directory + "/" + Provider.serverID + path;
			ReadWrite.DeleteIfExists(GetBackupFilePathV1(filePath)); // Cleanup
			ReadWrite.MoveIfExists(filePath, GetBackupFilePath(filePath));
			ReadWrite.serializeJSON<T>(filePath, false, instance);
		}

		public static T deserializeJSON<T>(string path)
		{
			return ReadWrite.deserializeJSON<T>(directory + "/" + Provider.serverID + path, false);
		}

		public static void populateJSON(string path, object target)
		{
			ReadWrite.populateJSON(directory + "/" + Provider.serverID + path, target, usePath: true);
		}

		public static void writeData(string path, Data data)
		{
			string filePath = directory + "/" + Provider.serverID + path;
			ReadWrite.DeleteIfExists(GetBackupFilePathV1(filePath)); // Cleanup
			ReadWrite.MoveIfExists(filePath, GetBackupFilePath(filePath));
			ReadWrite.writeData(filePath, false, data);
		}

		public static Data readData(string path)
		{
			return ReadWrite.readData(directory + "/" + Provider.serverID + path, false);
		}

		public static void writeBlock(string path, Block block)
		{
			string filePath = directory + "/" + Provider.serverID + path;
			ReadWrite.DeleteIfExists(GetBackupFilePathV1(filePath)); // Cleanup
			ReadWrite.MoveIfExists(filePath, GetBackupFilePath(filePath));
			ReadWrite.writeBlock(filePath, false, block);
		}

		public static Block readBlock(string path, byte prefix)
		{
			return ReadWrite.readBlock(directory + "/" + Provider.serverID + path, false, prefix);
		}

		public static River openRiver(string path, bool isReading)
		{
			string filePath = directory + "/" + Provider.serverID + path;
			if (!isReading)
			{
				ReadWrite.DeleteIfExists(GetBackupFilePathV1(filePath)); // Cleanup
				ReadWrite.MoveIfExists(filePath, GetBackupFilePath(filePath));
			}
			return new River(filePath, true, false, isReading);
		}

		public static void deleteFile(string path)
		{
			ReadWrite.deleteFile(directory + "/" + Provider.serverID + path, false);
		}

		public static bool fileExists(string path)
		{
			return ReadWrite.fileExists(directory + "/" + Provider.serverID + path, false);
		}

		public static void createFolder(string path)
		{
			ReadWrite.createFolder(directory + "/" + Provider.serverID + path);
		}

		public static void deleteFolder(string path)
		{
			ReadWrite.deleteFolder(directory + "/" + Provider.serverID + path);
		}

		public static bool folderExists(string path)
		{
			return ReadWrite.folderExists(directory + "/" + Provider.serverID + path);
		}
	}
}
