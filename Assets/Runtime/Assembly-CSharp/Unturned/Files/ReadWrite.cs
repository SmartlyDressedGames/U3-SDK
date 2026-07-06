////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public enum EReadTextureFromFileMode
	{
		UI,
	}

	public class ReadWrite
	{
		public static readonly string PATH = UnturnedPaths.RootDirectory.FullName;

		public static byte[] readData()
		{
#if UNITY_EDITOR
			FileStream stream = new FileStream(UnityPaths.ProjectDirectory.FullName + "/Builds/Windows64/Unturned_Data/Managed/Assembly-CSharp.dll", FileMode.Open, FileAccess.Read, FileShare.Read);
#else
			FileStream stream = new FileStream(UnityPaths.GameDataDirectory.FullName + "/Managed/Assembly-CSharp.dll", FileMode.Open, FileAccess.Read, FileShare.Read);
#endif
			byte[] bytes = new byte[stream.Length];
			stream.Read(bytes, 0, bytes.Length);
			stream.Close();
			stream.Dispose();

			return Hash.SHA1(bytes);
		}

		public static T deserializeJSON<T>(string path, bool useCloud)
		{
			return deserializeJSON<T>(path, useCloud, true);
		}

		public static T deserializeJSON<T>(string path, bool useCloud, bool usePath)
		{
			T instance = default;
			byte[] bytes = readBytes(path, useCloud, usePath);

			if (bytes == null)
			{
				return instance;
			}

			string data = Encoding.UTF8.GetString(bytes);

			if (data == null)
			{
				return instance;
			}

			return JsonConvert.DeserializeObject<T>(data);
		}

		/// <summary>
		/// Deserialize JSON onto an existing object instance.
		/// </summary>
		public static void populateJSON(string path, object target, bool usePath = true)
		{
			byte[] bytes = readBytes(path, false, usePath);
			if (bytes == null)
				return;

			string data = Encoding.UTF8.GetString(bytes);
			if (data == null)
				return;

			JsonConvert.PopulateObject(data, target);
		}

		public static byte[] cloudFileRead(string path)
		{
			// Respects disableSteamCloudRead command-line option.
			if (!cloudFileExists(path))
			{
				return null;
			}

			int size;
			Provider.provider.cloudService.getSize(path, out size);
			byte[] bytes = new byte[size];
			if (!Provider.provider.cloudService.read(path, bytes))
			{
				UnturnedLog.error("Failed to read the correct file size.");
				return null;
			}

			return bytes;
		}

		public static void cloudFileWrite(string path, byte[] bytes, int size)
		{
			if (!Provider.provider.cloudService.write(path, bytes, size))
			{
				UnturnedLog.error("Failed to write file.");
			}
		}

		public static void cloudFileDelete(string path)
		{
			Provider.provider.cloudService.delete(path);
		}

		/// <summary>
		/// Potentially useful for players with corrupted cloud storage.
		/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2756
		/// </summary>
		private static CommandLineFlag disableSteamCloudRead = new CommandLineFlag(false, "-DisableSteamCloudRead");

		public static bool cloudFileExists(string path)
		{
			if (disableSteamCloudRead)
			{
				return false;
			}

			bool exists;
			Provider.provider.cloudService.exists(path, out exists);

			return exists;
		}

		public static void serializeJSON<T>(string path, bool useCloud, T instance)
		{
			serializeJSON<T>(path, useCloud, true, instance);
		}

		public static void serializeJSON<T>(string path, bool useCloud, bool usePath, T instance)
		{
			string data = JsonConvert.SerializeObject(instance, Newtonsoft.Json.Formatting.Indented);
			byte[] bytes = Encoding.UTF8.GetBytes(data);

			writeBytes(path, useCloud, usePath, bytes, bytes.Length);
		}

		private static readonly XmlSerializerNamespaces XML_SERIALIZER_NAMESPACES = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
		private static readonly XmlWriterSettings XML_WRITER_SETTINGS = new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true, Encoding = new System.Text.UTF8Encoding() };

		public static T deserializeXML<T>(string path, bool useCloud)
		{
			return deserializeXML<T>(path, useCloud, true);
		}

		public static T deserializeXML<T>(string path, bool useCloud, bool usePath)
		{
			T instance = default;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			if (useCloud)
			{
				MemoryStream memoryStream = new MemoryStream(cloudFileRead(path));

				try
				{
					instance = (T) xmlSerializer.Deserialize(memoryStream);
				}
				finally
				{
					memoryStream.Close();
					memoryStream.Dispose();
				}

				return instance;
			}
			else
			{
				if (usePath)
				{
					path += PATH;
				}

				if (!Directory.Exists(Path.GetDirectoryName(path)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path));
				}

				if (!File.Exists(path))
				{
					UnturnedLog.info("Failed to find file at: " + path);
					return instance;
				}

				StreamReader streamReader = new StreamReader(path);

				try
				{
					instance = (T) xmlSerializer.Deserialize(streamReader);
				}
				finally
				{
					streamReader.Close();
					streamReader.Dispose();
				}

				return instance;
			}
		}

		public static void serializeXML<T>(string path, bool useCloud, T instance)
		{
			serializeXML<T>(path, useCloud, true, instance);
		}

		public static void serializeXML<T>(string path, bool useCloud, bool usePath, T instance)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			if (useCloud)
			{
				MemoryStream memoryStream = new MemoryStream();
				XmlWriter xmlWriter = XmlWriter.Create(memoryStream, XML_WRITER_SETTINGS);

				try
				{
					xmlSerializer.Serialize(xmlWriter, instance, XML_SERIALIZER_NAMESPACES);

					cloudFileWrite(path, memoryStream.GetBuffer(), (int) memoryStream.Length);
				}
				finally
				{
					xmlWriter.Close();

					memoryStream.Close();
					memoryStream.Dispose();
				}
			}
			else
			{
				if (usePath)
				{
					path = PATH + path;
				}

				if (!Directory.Exists(Path.GetDirectoryName(path)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path));
				}

				StreamWriter streamWriter = new StreamWriter(path);

				try
				{
					xmlSerializer.Serialize(streamWriter, instance, XML_SERIALIZER_NAMESPACES);
				}
				finally
				{
					streamWriter.Close();
					streamWriter.Dispose();
				}
			}
		}

		public static byte[] readBytes(string path, bool useCloud)
		{
			return readBytes(path, useCloud, true);
		}

		public static byte[] readBytes(string path, bool useCloud, bool usePath)
		{
			if (useCloud)
			{
				return cloudFileRead(path);
			}
			else
			{
				if (usePath)
				{
					path = PATH + path;
				}

				if (!Directory.Exists(Path.GetDirectoryName(path)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path));
				}

				if (!File.Exists(path))
				{
					UnturnedLog.info("Failed to find file at: " + path);
					return null;
				}

				FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

				byte[] bytes = new byte[stream.Length];
				int read = stream.Read(bytes, 0, bytes.Length);

				if (read != bytes.Length)
				{
					UnturnedLog.error("Failed to read the correct file size.");
					return null;
				}

				stream.Close();
				stream.Dispose();

				return bytes;
			}
		}

		/// <summary>
		/// Introduced much later (2020) than most of the other methods in this class (2014) in order to properly handle
		/// BOM/preamble of text files. Matches somewhat undesirable legacy behavior like creating directories.
		/// </summary>
		private static string readString(string filePath, bool useCloud, bool prependPath)
		{
			if (useCloud)
			{
				byte[] bytes = readBytes(filePath, useCloud, prependPath);

				if (bytes == null)
				{
					return null;
				}

				return Encoding.UTF8.GetString(bytes);
			}
			else
			{
				if (prependPath)
				{
					filePath = PATH + filePath;
				}

				string directoryPath = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				if (!File.Exists(filePath))
				{
					UnturnedLog.info("Failed to find file at: " + filePath);
					return null;
				}

				return File.ReadAllText(filePath);
			}
		}

		public static Data readData(string path, bool useCloud)
		{
			return readData(path, useCloud, true);
		}

		public static Data readData(string path, bool useCloud, bool usePath)
		{
			string content = readString(path, useCloud, usePath);

			if (content == null)
			{
				// Matches legacy behavior from before readString was added.
				return null;
			}

			if (content.Length == 0)
			{
				// Matches legacy behavior from before readString was added.
				return new Data();
			}

			return new Data(content);
		}

		private static DatParser datParser = new DatParser();

		internal static IDatDictionary ReadDataWithoutHash(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}

			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (StreamReader streamReader = new StreamReader(fileStream))
			{
				return datParser.Parse(streamReader);
			}
		}

		public static Block readBlock(string path, bool useCloud, byte prefix)
		{
			return readBlock(path, useCloud, true, prefix);
		}

		public static Block readBlock(string path, bool useCloud, bool usePath, byte prefix)
		{
			byte[] bytes = readBytes(path, useCloud, usePath);

			if (bytes == null)
			{
				return null;
			}

			return new Block(prefix, bytes);
		}

		public static void writeBytes(string path, bool useCloud, byte[] bytes)
		{
			writeBytes(path, useCloud, true, bytes, bytes.Length);
		}

		public static void writeBytes(string path, bool useCloud, byte[] bytes, int size)
		{
			writeBytes(path, useCloud, true, bytes, size);
		}

		public static void writeBytes(string path, bool useCloud, bool usePath, byte[] bytes)
		{
			writeBytes(path, useCloud, usePath, bytes, bytes.Length);
		}

		public static void writeBytes(string path, bool useCloud, bool usePath, byte[] bytes, int size)
		{
			if (useCloud)
			{
				cloudFileWrite(path, bytes, size);
			}
			else
			{
				if (usePath)
				{
					path = PATH + path;
				}

				if (!Directory.Exists(Path.GetDirectoryName(path)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path));
				}

				FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
				stream.Write(bytes, 0, size);
				stream.SetLength(size);
				stream.Flush();
				stream.Close();
				stream.Dispose();
			}
		}

		public static void writeData(string path, bool useCloud, Data data)
		{
			writeData(path, useCloud, true, data);
		}

		public static void writeData(string path, bool useCloud, bool usePath, Data data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data.getFile());

			writeBytes(path, useCloud, usePath, bytes);
		}

		public static void writeBlock(string path, bool useCloud, Block block)
		{
			writeBlock(path, useCloud, true, block);
		}

		public static void writeBlock(string path, bool useCloud, bool usePath, Block block)
		{
			int size;
			byte[] bytes = block.getBytes(out size);
			writeBytes(path, useCloud, usePath, bytes, size);
		}

		public static void deleteFile(string path, bool useCloud)
		{
			deleteFile(path, useCloud, true);
		}

		public static void deleteFile(string path, bool useCloud, bool usePath)
		{
			if (useCloud)
			{
				cloudFileDelete(path);
			}
			else
			{
				if (usePath)
				{
					path = PATH + path;
				}

				File.Delete(path);
			}
		}

		public static void deleteFolder(string path)
		{
			deleteFolder(path, true);
		}

		public static void deleteFolder(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			Directory.Delete(path, true);
		}

		public static void moveFolder(string origin, string target)
		{
			moveFolder(origin, target, true);
		}

		public static void moveFolder(string origin, string target, bool usePath)
		{
			if (usePath)
			{
				origin = PATH + origin;
				target = PATH + target;
			}

			Directory.Move(origin, target);
		}

		public static void createFolder(string path)
		{
			createFolder(path, true);
		}

		public static void createFolder(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static void createHidden(string path)
		{
			createHidden(path, true);
		}

		public static void createHidden(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			if (!Directory.Exists(path))
			{
				DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
				directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			}
		}

		public static string folderName(string path)
		{
			return new DirectoryInfo(path).Name;
		}

		public static string folderPath(string path)
		{
			return Path.GetDirectoryName(path);
		}

		public static void renameFile(string path_0, string path_1)
		{
			path_0 = PATH + path_0;
			path_1 = PATH + path_1;

			File.Move(path_0, path_1);
		}

		/// <summary>
		/// NOTE: From and to are both relative to PATH.
		/// </summary>
		public static void MoveIfExists(string sourceFileName, string destFileName)
		{
			sourceFileName = PATH + sourceFileName;
			destFileName = PATH + destFileName;
			MoveIfExistsAbsolute(sourceFileName, destFileName);
		}

		public static void MoveIfExistsAbsolute(string sourceFileName, string destFileName)
		{
			try
			{
				// Nelson 2024-09-20: At the time of writing, File.Move with `overwrite` parameter is unavailable.
				if (File.Exists(destFileName))
				{
					File.Delete(destFileName);
				}
				if (File.Exists(sourceFileName))
				{
					File.Move(sourceFileName, destFileName);
				}
			}
			catch (System.Exception exception)
			{
				// Nelson 2024-09-20: Don't want to break savedata if moving the backup fails, and in testing I ran into
				// an exception because the barricade file wasn't closed during load. :x
				UnturnedLog.exception(exception, $"Caught exception moving \"{sourceFileName}\" to \"{destFileName}\":");
			}
		}

		/// <summary>
		/// NOTE: From and to are both relative to PATH.
		/// </summary>
		public static void DeleteIfExists(string fileName)
		{
			DeleteIfExistsAbsolute(PATH + fileName);
		}

		public static void DeleteIfExistsAbsolute(string fileName)
		{
			try
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, $"Caught exception deleting \"{fileName}\":");
			}
		}

		public static string fileName(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		public static bool fileExists(string path, bool useCloud)
		{
			return fileExists(path, useCloud, true);
		}

		public static bool fileExists(string path, bool useCloud, bool usePath)
		{
			if (useCloud)
			{
				return cloudFileExists(path);
			}
			else
			{
				if (usePath)
				{
					path = PATH + path;
				}

				return File.Exists(path);
			}
		}

		public static string folderFound(string path)
		{
			return folderFound(path, true);
		}

		public static string folderFound(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			string[] folders = Directory.GetDirectories(path);
			if (folders.Length > 0)
			{
				return folders[0];
			}
			else
			{
				return null;
			}
		}

		public static bool folderExists(string path)
		{
			return folderExists(path, true);
		}

		public static bool folderExists(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			return Directory.Exists(path);
		}

		public static bool hasDirectoryWritePermission(string path)
		{
#if UNITY_STANDALONE_WIN
			try
			{
				DirectorySecurity accessControl = Directory.GetAccessControl(path);
				AuthorizationRuleCollection accessRules = accessControl.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

				bool allowed = false;
				foreach (FileSystemAccessRule rule in accessRules)
				{
					if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
					{
						switch (rule.AccessControlType)
						{
							case AccessControlType.Allow:
								allowed = true;
								break;

							case AccessControlType.Deny:
								return false;
						}
					}
				}

				return allowed;
			}
			catch
			{
				return false;
			}
#else // UNITY_STANDALONE_WIN
			// 2023-05-31: GetAccessControl is not currently implemented on other platforms. (public issue #3901)
			return true;
#endif // UNITY_STANDALONE_WIN
		}

		public static string[] getFolders(string path)
		{
			return getFolders(path, true);
		}

		public static string[] getFolders(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			return Directory.GetDirectories(path);
		}

		public static string[] getFiles(string path)
		{
			return getFiles(path, true);
		}

		public static string[] getFiles(string path, bool usePath)
		{
			if (usePath)
			{
				path = PATH + path;
			}

			return Directory.GetFiles(path);
		}

		public static void copyFile(string source, string destination)
		{
			source = PATH + source;
			destination = PATH + destination;

			if (!Directory.Exists(Path.GetDirectoryName(destination)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
			}

			File.Copy(source, destination);
		}

		/// <summary>
		/// Read GUI texture from a .jpg or .png file.
		/// </summary>
		public static Texture2D readTextureFromFile(string path, bool useBasePath, EReadTextureFromFileMode mode = EReadTextureFromFileMode.UI)
		{
			if (useBasePath)
			{
				path = PATH + path;
			}

			return readTextureFromFile(path);
		}

		/// <summary>
		/// Read GUI texture from a .jpg or .png file.
		/// </summary>
		public static Texture2D readTextureFromFile(string absolutePath, EReadTextureFromFileMode mode = EReadTextureFromFileMode.UI)
		{
			byte[] bytes = File.ReadAllBytes(absolutePath);

			bool mipChain = false;
			bool linear = false;

			// Dimensions do not matter, as LoadImage will use dimensions from file.
			Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain, linear);
			texture.hideFlags = HideFlags.HideAndDontSave;
			bool nonReadableOnCPU = true;
			texture.LoadImage(bytes, nonReadableOnCPU);

			return texture;
		}

		public static bool SupportsOpeningFileBrowser =>
#if UNITY_STANDALONE_WIN
				true;
#else // !UNITY_STANDALONE_WIN
				false;
#endif // !UNITY_STANDALONE_WIN

		public static void OpenFileBrowser(string folderPath)
		{
#if UNITY_STANDALONE_WIN
			try
			{
				folderPath = Path.GetFullPath(folderPath); // Cleans up mixed '/' and '\' otherwise it will open My Documents.
				System.Diagnostics.Process.Start("explorer.exe", $"\"{folderPath}\"");
				UnturnedLog.info($"Opened Windows Explorer at path: \"{folderPath}\"");
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, $"Exception opening Windows Explorer at path: \"{folderPath}\"");
			}
#else // !UNITY_STANDALONE_WIN
			UnturnedLog.info($"Cannot open file browser to path (not supported): \"{folderPath}\"");
#endif // !UNITY_STANDALONE_WIN
		}
	}
}
