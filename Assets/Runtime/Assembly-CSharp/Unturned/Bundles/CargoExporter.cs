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
	internal class CargoDeclaration
	{
		public void Append(string key, bool value)
		{
			string valueString = value ? "yes" : "no";
			lines.Add($"| {key} = {valueString}");
		}

		public void Append(string key, string value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, System.Guid guid)
		{
			lines.Add($"| {key} = {guid:N}");
		}

		public void Append(string key, byte value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, ushort value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, uint value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, ulong value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, sbyte value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, short value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, int value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, long value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, float value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, double value)
		{
			lines.Add($"| {key} = {value}");
		}

		public void Append(string key, Color32 value)
		{
			lines.Add($"| {key} = {Palette.hex(value)}");
		}

		public void Append(string key, object value)
		{
			lines.Add($"| {key} = {value}");
		}

		internal List<string> lines = new List<string>();
	}

	internal class CargoBuilder
	{
		/// <summary>
		/// Finds an existing "{{Cargo/name" (if any), otherwise adds a new one.
		/// </summary>
		public CargoDeclaration GetOrAddDeclaration(string name)
		{
			List<CargoDeclaration> list = declarations.GetOrAddNew(name);
			if (list.IsEmpty())
			{
				CargoDeclaration declaration = new CargoDeclaration();
				list.Add(declaration);
			}
			return list[0];
		}

		/// <summary>
		/// Adds a new "{{Cargo/name" even if one already exists.
		/// </summary>
		public CargoDeclaration AddDeclaration(string name)
		{
			List<CargoDeclaration> list = declarations.GetOrAddNew(name);
			CargoDeclaration declaration = new CargoDeclaration();
			list.Add(declaration);
			return declaration;
		}

		public void Clear()
		{
			declarations.Clear();
		}

		internal Dictionary<string, List<CargoDeclaration>> declarations = new Dictionary<string, List<CargoDeclaration>>();
	}

	/// <summary>
	/// Helper for wiki writers to dump game data into a useful format.
	/// </summary>
	public static class CargoExporter
	{
		public static void Export()
		{
			string basePath = Path.Join(ReadWrite.PATH, "Extras", "WikiCargoData");
			ReadWrite.createFolder(basePath, false);

			CargoBuilder builder = new CargoBuilder();

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

				foreach (Asset asset in origin.assets)
				{
					builder.Clear();
					asset.BuildCargoData(builder);

					if (builder.declarations.Count < 1)
					{
						continue;
					}

					string typeDirName = PathEx.ReplaceInvalidFileNameChars(asset.GetTypeFriendlyName(), '_');
					string exportDir = Path.Combine(originPath, typeDirName);
					Directory.CreateDirectory(exportDir);

					string fileName = PathEx.ReplaceInvalidFileNameChars($"{asset.name} ({asset.FriendlyName})", '_');
					string filePath = Path.Join(exportDir, fileName + ".txt");

					using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
					using (StreamWriter sw = new StreamWriter(fs))
					{
						foreach (KeyValuePair<string, List<CargoDeclaration>> nameListPair in builder.declarations)
						{
							foreach (CargoDeclaration declaration in nameListPair.Value)
							{
								sw.Write("{{Cargo/");
								sw.WriteLine(nameListPair.Key);

								foreach (string line in declaration.lines)
								{
									sw.WriteLine(line);
								}

								sw.WriteLine("}}");
								sw.WriteLine();
							}
						}
					}
				}
			}
		}
	}
}
