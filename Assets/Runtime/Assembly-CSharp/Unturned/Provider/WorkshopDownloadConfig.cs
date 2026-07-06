////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Configuration for DedicatedUGC.
	/// </summary>
	public class WorkshopDownloadConfig
	{
		public WorkshopDownloadConfig()
		{
			File_IDs = new List<ulong>();
			Ignore_Children_File_IDs = new List<ulong>();
			Query_Cache_Max_Age_Seconds = 600;
			Max_Query_Retries = 2;
			Use_Cached_Downloads = true;
			Should_Monitor_Updates = true;
			Shutdown_Update_Detected_Timer = 600;
			Shutdown_Update_Detected_Message = "Workshop file update detected, shutdown in: {0}";
			Shutdown_Kick_Message = "Shutdown for Workshop file update.";
		}

		/// <summary>
		/// Published workshop file IDs to download.
		/// </summary>
		public List<ulong> File_IDs;

		/// <summary>
		/// Published workshop file IDs whose children (dependencies) should be skipped.
		/// Useful if workshop author lists dependencies as a way of advertising.
		/// </summary>
		public List<ulong> Ignore_Children_File_IDs;

		/// <summary>
		/// Controls SetAllowCachedResponse. Disabled when set to zero.
		/// Balance between item change frequency and allowing cached results when query fails.
		/// </summary>
		public uint Query_Cache_Max_Age_Seconds;

		/// <summary>
		/// Number of total times to try re-submitting failed workshop queries before aborting.
		/// </summary>
		public uint Max_Query_Retries;

		/// <summary>
		/// Should items already installed be loaded?
		/// </summary>
		public bool Use_Cached_Downloads;

		/// <summary>
		/// Should used items be monitored for updates?
		/// </summary>
		public bool Should_Monitor_Updates;

		/// <summary>
		/// Seconds to wait before shutting down after an update is detected.
		/// </summary>
		public int Shutdown_Update_Detected_Timer;

		/// <summary>
		/// Message broadcasted when shutdown timer begins.
		/// </summary>
		public string Shutdown_Update_Detected_Message;

		/// <summary>
		/// Message sent to players when shutdown timer completes.
		/// </summary>
		public string Shutdown_Kick_Message;

		/// <summary>
		/// Get instance if loaded, but do not load.
		/// </summary>
		public static WorkshopDownloadConfig get()
		{
			return instance;
		}

		/// <summary>
		/// Get instance, or load if not yet loaded.
		/// </summary>
		public static WorkshopDownloadConfig getOrLoad()
		{
			if (instance == null)
			{
				instance = load();
			}

			return instance;
		}

		private static WorkshopDownloadConfig load()
		{
			WorkshopDownloadConfig config;

			if (ServerSavedata.fileExists("/WorkshopDownloadConfig.json"))
			{
				config = loadFromConfig();
			}
			else if (ServerSavedata.fileExists("/WorkshopDownloadIDs.json"))
			{
				config = loadFromLegacyFormat();
			}
			else
			{
				config = null;
			}

			if (config == null)
			{
				config = new WorkshopDownloadConfig();
			}

			ServerSavedata.serializeJSON("/WorkshopDownloadConfig.json", config);

			return config;
		}

		private static WorkshopDownloadConfig loadFromConfig()
		{
			WorkshopDownloadConfig config;

			try
			{
				config = ServerSavedata.deserializeJSON<WorkshopDownloadConfig>("/WorkshopDownloadConfig.json");
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Unable to parse WorkshopDownloadConfig.json! consider validating with a JSON linter");
				config = null;
			}

			return config;
		}

		private static WorkshopDownloadConfig loadFromLegacyFormat()
		{
			WorkshopDownloadConfig config;

			try
			{
				config = new WorkshopDownloadConfig();
				config.File_IDs = ServerSavedata.deserializeJSON<List<ulong>>("/WorkshopDownloadIDs.json");
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Unable to parse WorkshopDownloadIDs.json! consider validating with a JSON linter");
				config = null;
			}

			return config;
		}

		private static WorkshopDownloadConfig instance;
	}
}
