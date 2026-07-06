////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Static functions for creating monitor instance on server.
	/// </summary>
	public static class DedicatedWorkshopUpdateMonitorFactory
	{
		public delegate IDedicatedWorkshopUpdateMonitor CreateHandler(LevelInfo level);
		public static event CreateHandler onCreateForLevel;

		/// <summary>
		/// Entry point called by dedicated server after loading level.
		/// </summary>
		public static IDedicatedWorkshopUpdateMonitor createForLevel(LevelInfo level)
		{
			if (WorkshopDownloadConfig.get().Should_Monitor_Updates == false)
				return null;

			if (onCreateForLevel == null)
			{
				return createDefaultForLevel(level);
			}
			else
			{
				return onCreateForLevel(level);
			}
		}

		/// <summary>
		/// Create vanilla update monitor that watches for changes to workshop level file and any other mods.
		/// </summary>
		public static IDedicatedWorkshopUpdateMonitor createDefaultForLevel(LevelInfo level)
		{
			List<DedicatedWorkshopUpdateMonitor.MonitoredItem> monitoredItems = new List<DedicatedWorkshopUpdateMonitor.MonitoredItem>();

			if (createMonitoredItemForLevel(level, out DedicatedWorkshopUpdateMonitor.MonitoredItem levelMonitoredItem))
			{
				CommandWindow.LogFormat("Monitoring workshop map \"{0}\" ({1}) for changes", level.name, level.publishedFileId);
				monitoredItems.Add(levelMonitoredItem);
			}
			else if (level.isFromWorkshop)
			{
				UnturnedLog.info($"Unable to monitor workshop map \"{level.name}\" ({level.publishedFileId}) for changes");
			}

			foreach (ulong fileId in Provider.getServerWorkshopFileIDs())
			{
				if (fileId == level.publishedFileId)
				{
					// Don't add the map a second time.
					continue;
				}

				if (createMonitoredItem(new PublishedFileId_t(fileId), out DedicatedWorkshopUpdateMonitor.MonitoredItem monitoredItem))
				{
					CommandWindow.LogFormat("Monitoring workshop file {0} for changes", fileId);
					monitoredItems.Add(monitoredItem);
				}
				else
				{
					UnturnedLog.info($"Unable to monitor workshop file {fileId} for changes");
				}
			}

			if (monitoredItems.Count < 1)
			{
				UnturnedLog.info("No workshop items to monitor for updates");
				return null;
			}

			return new DedicatedWorkshopUpdateMonitor(monitoredItems.ToArray());
		}

		/// <summary>
		/// Helper to get updated timestamp from workshop items loaded by DedicatedUGC.
		/// </summary>
		public static bool getCachedInitialTimestamp(PublishedFileId_t fileId, out uint timestamp)
		{
			CachedUGCDetails cachedDetails;
			if (TempSteamworksWorkshop.getCachedDetails(fileId, out cachedDetails))
			{
				timestamp = cachedDetails.updateTimestamp;
				return true;
			}

			timestamp = 0;
			return false;
		}

		/// <summary>
		/// Helper to create monitored item for use with default DedicatedWorkshopUpdateMonitor implementation.
		/// </summary>
		public static bool createMonitoredItem(PublishedFileId_t fileId, out DedicatedWorkshopUpdateMonitor.MonitoredItem monitoredItem)
		{
			uint initialTimestamp;
			if (getCachedInitialTimestamp(fileId, out initialTimestamp))
			{
				monitoredItem = new DedicatedWorkshopUpdateMonitor.MonitoredItem();
				monitoredItem.fileId = fileId;
				monitoredItem.initialTimestamp = initialTimestamp;
				return true;
			}
			else
			{
				monitoredItem = new DedicatedWorkshopUpdateMonitor.MonitoredItem();
				return false;
			}
		}

		/// <summary>
		/// For use with default DedicatedWorkshopUpdateMonitor implementation.
		/// </summary>
		public static bool createMonitoredItemForLevel(LevelInfo level, out DedicatedWorkshopUpdateMonitor.MonitoredItem monitoredItem)
		{
			if (level != null && level.isFromWorkshop)
			{
				PublishedFileId_t levelFileId = new PublishedFileId_t(level.publishedFileId);
				if (createMonitoredItem(levelFileId, out monitoredItem))
				{
					return true;
				}
				else
				{
					CommandWindow.LogWarningFormat("Unable to monitor level '{0}' ({1}) for changes because no details were cached", level.name, levelFileId);
				}
			}

			monitoredItem = new DedicatedWorkshopUpdateMonitor.MonitoredItem();
			return false;
		}
	}
}
