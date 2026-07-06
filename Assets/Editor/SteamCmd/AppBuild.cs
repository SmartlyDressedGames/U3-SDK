////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace Unturned.SteamCmd
{
	public struct DepotBuildReference
	{
		public uint depotId;
		public string scriptPath;

		public DepotBuildReference(uint depotId, string scriptPath)
		{
			this.depotId = depotId;
			this.scriptPath = scriptPath;
		}
	}

	public class AppBuild
	{
		/// <summary>
		/// The AppID of your game. The uploading Steam partner account needs 'Edit App Metadata' privileges
		/// </summary>
		public uint appId;

		/// <summary>
		/// The description is only visible to you in the 'Your Builds' section of the App Admin panel.
		/// This can be changed at any time after uploading a build on the 'Your Builds' page.
		/// </summary>
		public string desc;

		/// <summary>
		/// The root folder of your game files, can be an absolute path or relative to the build script file.
		/// </summary>
		public string contentRoot;

		/// <summary>
		/// This directory will be the location for build logs, depot manifests, chunk caches, and intermediate output. For
		/// best performance, use a separate disk for your build output. This splits the disk IO workload, letting your
		/// content root disk handle the read requests and your output disk handle the write requests. 
		/// </summary>
		public string buildOutput;

		/// <summary>
		/// This type of build only outputs logs and a file manifest into the build output folder. Building preview builds
		/// is a good way to iterate on your upload scripts and make sure your file mappings, filters and properties work
		/// as intended.
		/// </summary>
		public bool preview;

		/// <summary>
		/// Set this to the htdocs path of your SteamPipe Local Content Server (LCS). LCS builds put content only on your
		/// own HTTP server and allow you to test the installation of your game using the Steam client. 
		/// </summary>
		public string local;

		/// <summary>
		/// Beta branch name to automatically set live after successful build, none if empty.
		/// Note that the 'default' branch can not be set live automatically. That must be done through the App Admin panel.
		/// </summary>
		public string setLive;

		/// <summary>
		/// This section contains all file mappings, filters and file properties for each depot or references a separate
		/// script file for each depot.
		/// </summary>
		public List<DepotBuildReference> depots = new List<DepotBuildReference>();

		public void AddDepotScript(uint depotId, string scriptPath)
		{
			depots.Add(new DepotBuildReference(depotId, scriptPath));
		}

		public void AddDepotScript(uint depotId)
		{
			string scriptPath = SteamCmdUtils.GetDepotBuildScriptPath(depotId);
			AddDepotScript(depotId, scriptPath);
		}

		public void Write(VdfWriter writer)
		{
			writer.WriteStartBlock("AppBuild");

			writer.WriteKeyValue("AppID", appId);
			writer.WriteKeyValue("Desc", desc);
			writer.WriteKeyValue("ContentRoot", contentRoot);
			writer.WriteKeyValue("BuildOutput", buildOutput);
			writer.WriteKeyValue("Preview", preview);

			if (!string.IsNullOrEmpty(local))
			{
				writer.WriteKeyValue("Local", local);
			}

			if (!string.IsNullOrEmpty(setLive))
			{
				writer.WriteKeyValue("SetLive", setLive);
			}

			writer.WriteStartBlock("Depots");
			foreach (DepotBuildReference depot in depots)
			{
				writer.WriteKeyValue(depot.depotId, depot.scriptPath);
			}
			writer.WriteEndBlock();

			writer.WriteEndBlock();
		}
	}
}
