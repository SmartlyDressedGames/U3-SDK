////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace Unturned.SteamCmd
{
	public struct FileMapping
	{
		/// <summary>
		/// The LocalPath parameter is a relative path to the content root folder and may contain wildcards like '?' or '*'.
		/// It will also apply to matching files in subfolders if Recursive is enabled.
		/// </summary>
		public string localPath;

		/// <summary>
		/// The DepotPath parameter specifies where the selected files should appear in the depot.
		/// (use just '.' for no special mapping)
		/// </summary>
		public string depotPath;

		public bool recursive;

		public FileMapping(string localPath = "*", string depotPath = ".", bool recursive = false)
		{
			this.localPath = localPath;
			this.depotPath = depotPath;
			this.recursive = recursive;
		}
	}

	public class DepotBuild
	{
		/// <summary>
		/// The DepotID for this section.
		/// </summary>
		public uint depotId;

		/// <summary>
		/// Lets you optionally override the ContentRoot folder from the app build script on a per depot basis
		/// </summary>
		public string contentRoot;

		/// <summary>
		/// This maps a single file or a set of files from the local content root into your depot.
		/// There can be multiple file mappings that add files to the depot.
		/// </summary>
		public List<FileMapping> fileMappings = new List<FileMapping>();

		/// <summary>
		/// Will excluded mapped files again and can also contain wildcards like '?' or '*'
		/// </summary>
		public List<string> fileExclusions = new List<string>();

		/// <summary>
		/// Will mark a file as install scripts and will sign the file during the build process.
		/// The Steam client knows to run them for any application which mounts this depot.
		/// </summary>
		public List<string> installScripts = new List<string>();

		public void AddFileMapping(string localPath = "*", string depotPath = ".", bool recursive = false)
		{
			fileMappings.Add(new FileMapping(localPath, depotPath, recursive));
		}

		public void AddFileExclusion(string path)
		{
			fileExclusions.Add(path);
		}

		public void AddInstallScript(string path)
		{
			installScripts.Add(path);
		}

		public void Write(VdfWriter writer)
		{
			writer.WriteStartBlock("DepotBuild");
			writer.WriteKeyValue("DepotID", depotId);

			if (!string.IsNullOrEmpty(contentRoot))
			{
				writer.WriteKeyValue("ContentRoot", contentRoot);
			}

			foreach (FileMapping mapping in fileMappings)
			{
				writer.WriteStartBlock("FileMapping");
				writer.WriteKeyValue("LocalPath", mapping.localPath);
				writer.WriteKeyValue("DepotPath", mapping.depotPath);
				writer.WriteKeyValue("Recursive", mapping.recursive);
				writer.WriteEndBlock();
			}

			foreach (string path in fileExclusions)
			{
				writer.WriteKeyValue("FileExclusion", path);
			}

			foreach (string path in installScripts)
			{
				writer.WriteKeyValue("InstallScript", path);
			}

			writer.WriteEndBlock();
		}
	}
}
