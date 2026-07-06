////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class Localization
	{
		private static List<string> _messages;
		public static List<string> messages => _messages;

		private static List<string> keys = new List<string>();

		[System.Obsolete]
		public static Local tryRead(string path)
		{
			return tryRead(path, true);
		}

		/// <summary>
		/// Load {Language}.dat and/or English.dat from folder path.
		/// </summary>
		public static Local tryRead(string path, bool usePath)
		{
			if (usePath)
			{
				path = ReadWrite.PATH + path;
			}

			string languageFilePath = Path.Combine(path, Provider.language + ".dat");
			string englishFilePath = Path.Combine(path, "English.dat");

			if (ReadWrite.fileExists(languageFilePath, false, false))
			{
				IDatDictionary data = ReadWrite.ReadDataWithoutHash(languageFilePath);
				IDatDictionary fallbackData = Provider.languageIsEnglish ? null : ReadWrite.ReadDataWithoutHash(englishFilePath);
				return new Local(data, fallbackData);
			}
			else if (ReadWrite.fileExists(englishFilePath, false, false))
			{
				IDatDictionary data = ReadWrite.ReadDataWithoutHash(englishFilePath);
				return new Local(data);
			}
			else
			{
				return new Local();
			}
		}

		public static Local read(string path)
		{
			string languageFilePath = Provider.localizationRoot + path;
			string englishFilePath = englishLocalizationRoot + path;
			if (ReadWrite.fileExists(languageFilePath, false, false))
			{
				IDatDictionary data = ReadWrite.ReadDataWithoutHash(languageFilePath);
				IDatDictionary fallbackData = Provider.languageIsEnglish ? null : ReadWrite.ReadDataWithoutHash(englishFilePath);
				return new Local(data, fallbackData);
			}
			else if (ReadWrite.fileExists(englishFilePath, false, false))
			{
				IDatDictionary data = ReadWrite.ReadDataWithoutHash(englishFilePath);
				return new Local(data);
			}
			else
			{
				return new Local();
			}
		}

		private static void scanFile(string path)
		{
			IDatDictionary fromData = ReadWrite.ReadDataWithoutHash(Path.Join(englishLocalizationRoot, path));
			IDatDictionary toData = ReadWrite.ReadDataWithoutHash(Provider.localizationRoot + path);

			List<KeyValuePair<string, string>> fromContents = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, IDatNode> pair in fromData)
			{
				if (pair.Value is IDatValue value)
				{
					fromContents.Add(new KeyValuePair<string, string>(pair.Key, value.Value));
				}
			}

			List<KeyValuePair<string, string>> toContents = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, IDatNode> pair in toData)
			{
				if (pair.Value is IDatValue value)
				{
					toContents.Add(new KeyValuePair<string, string>(pair.Key, value.Value));
				}
			}

			keys.Clear();
			for (int fromIndex = 0; fromIndex < fromContents.Count; fromIndex++)
			{
				string from = fromContents[fromIndex].Key;

				bool hasKey = false;
				for (int toIndex = 0; toIndex < toContents.Count; toIndex++)
				{
					string to = toContents[toIndex].Key;

					if (from == to)
					{
						hasKey = true;
						break;
					}
				}

				if (!hasKey)
				{
					keys.Add(from);
				}
			}

			if (keys.Count > 0)
			{
				messages.Add(path + " has " + keys.Count + " new keys:");
				for (int index = 0; index < keys.Count; index++)
				{
					messages.Add("[" + index + "]: " + keys[index]);
				}
			}
		}

		private static void scanFolder(string path)
		{
			string[] fromFiles = ReadWrite.getFiles(Path.Join(englishLocalizationRoot, path), false);
			string[] toFiles = ReadWrite.getFiles(Provider.localizationRoot + path, false);

			for (int fromIndex = 0; fromIndex < fromFiles.Length; fromIndex++)
			{
				string from = Path.GetFileName(fromFiles[fromIndex]);

				bool hasFile = false;
				for (int toIndex = 0; toIndex < toFiles.Length; toIndex++)
				{
					string to = Path.GetFileName(toFiles[toIndex]);

					if (from == to)
					{
						hasFile = true;
						break;
					}
				}

				if (hasFile)
				{
					scanFile(path + "/" + from);
				}
				else
				{
					messages.Add("New file \"" + from + "\" in " + path);
				}
			}

			string[] fromFolders = ReadWrite.getFolders(Path.Join(englishLocalizationRoot, path), false);
			string[] toFolders = ReadWrite.getFolders(Provider.localizationRoot + path, false);

			for (int fromIndex = 0; fromIndex < fromFolders.Length; fromIndex++)
			{
				string from = Path.GetFileName(fromFolders[fromIndex]);

				bool hasFolder = false;
				for (int toIndex = 0; toIndex < toFolders.Length; toIndex++)
				{
					string to = Path.GetFileName(toFolders[toIndex]);

					if (from == to)
					{
						hasFolder = true;
						break;
					}
				}

				if (hasFolder)
				{
					scanFolder(path + "/" + from);
				}
				else
				{
					messages.Add("New folder \"" + from + "\" in " + path);
				}
			}
		}

		public static void refresh()
		{
			if (messages == null)
			{
				_messages = new List<string>();
			}
			else
			{
				messages.Clear();
			}

			// Nelson 2025-09-12: localizationRoot can be invalid while falling back to English
			// so that mods with translations in the player's language are loaded. (3.25.8.0)
			if (!ReadWrite.folderExists(Provider.localizationRoot, false))
			{
				return;
			}

			scanFolder("/Player");
			scanFolder("/Menu");
			scanFolder("/Server");
			scanFolder("/Editor");
		}

		static Localization()
		{
			englishLocalizationRoot = Path.Combine(ReadWrite.PATH, "Localization", "English");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
			if (!Directory.Exists(englishLocalizationRoot) && Provider.steamAppInstallDirectory != null)
			{
				englishLocalizationRoot = PathEx.Join(Provider.steamAppInstallDirectory, "Localization", "English");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
		}

		private static string englishLocalizationRoot;
	}
}
