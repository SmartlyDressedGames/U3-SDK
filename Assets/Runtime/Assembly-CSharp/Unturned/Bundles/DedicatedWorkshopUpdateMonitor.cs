////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_WORKSHOP_UPDATE_MONITOR

using Steamworks;
using System;

namespace SDG.Unturned
{
	public interface IDedicatedWorkshopUpdateMonitor
	{
		void tick(float deltaTime);
	}

	/// <summary>
	/// If using a map or mods from the workshop, this class monitors them for changes so the server can be restarted.
	/// </summary>
	public class DedicatedWorkshopUpdateMonitor : IDedicatedWorkshopUpdateMonitor
	{
		public struct MonitoredItem
		{
			public PublishedFileId_t fileId;
			public uint initialTimestamp;
		}

		public DedicatedWorkshopUpdateMonitor(MonitoredItem[] monitoredItems, float queryInterval = 60 * 15)
		{
			this.monitoredItems = monitoredItems;
			fileIdsForQuery = new PublishedFileId_t[monitoredItems.Length];
			for (int index = 0; index < monitoredItems.Length; ++index)
			{
				fileIdsForQuery[index] = monitoredItems[index].fileId;
			}

			this.queryInterval = queryInterval;

			// Intentionally starts at zero to wait before querying right after startup.
			queryTimer = 0.0f;

			queryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onQueryCompleted);
			queryHandle = UGCQueryHandle_t.Invalid;
		}

		public void tick(float deltaTime)
		{
			if (isFinishedTicking)
				return;

			if (shouldDoFinalTick)
			{
				shouldDoFinalTick = false;
				isFinishedTicking = true;
				handleFinalTick();
				return;
			}

			if (queryHandle != UGCQueryHandle_t.Invalid)
			{
				// Waiting for a query response, so pause timer.
				return;
			}

			queryTimer += deltaTime;
			if (queryTimer > queryInterval)
			{
				queryTimer = 0.0f;
				handleTimerTriggered();
			}
		}

		/// <summary>
		/// Request status of workshop items.
		/// </summary>
		protected void submitQueryRequest(PublishedFileId_t[] fileIds)
		{
			if (fileIds == null || fileIds.Length < 1)
				return;

			if (queryHandle != UGCQueryHandle_t.Invalid)
			{
				throw new Exception("Already waiting on a pending query");
			}

			debugMessage("Query request {0} item(s)", fileIds.Length);

			queryHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(fileIds, (uint) fileIds.Length);
			SteamAPICall_t callHandle = SteamGameServerUGC.SendQueryUGCRequest(queryHandle);
			queryCompleted.Set(callHandle);
		}

		/// <summary>
		/// Called the next tick after update(s) detected.
		/// </summary>
		protected virtual void handleFinalTick()
		{
			WorkshopDownloadConfig config = WorkshopDownloadConfig.get();

			// Uses rich text so hosts can configure fancy shutdown messages.
			string message = string.Format(config.Shutdown_Update_Detected_Message, config.Shutdown_Update_Detected_Timer);
			ChatManager.say(message, ChatManager.welcomeColor, isRich: true);

			Provider.shutdown(config.Shutdown_Update_Detected_Timer, config.Shutdown_Kick_Message);
		}

		/// <summary>
		/// Called when a queried item's update timestamp is newer than our initially loaded version.
		/// </summary>
		protected virtual void handleUpdateDetected(SteamUGCDetails_t fileDetails)
		{
			CommandWindow.LogFormat("Detected an update to '{0}' ({1})", fileDetails.m_rgchTitle, fileDetails.m_nPublishedFileId);
			shouldDoFinalTick = true;
		}

		/// <summary>
		/// Called when results from a call to submitQueryRequest are available.
		/// </summary>
		protected virtual void handleQueryResponse(SteamUGCQueryCompleted_t callback)
		{
			debugMessage("Query response {0} item(s)", callback.m_unNumResultsReturned);

			for (uint resultIndex = 0; resultIndex < callback.m_unNumResultsReturned; ++resultIndex)
			{
				SteamUGCDetails_t details;
				if (SteamGameServerUGC.GetQueryUGCResult(callback.m_handle, resultIndex, out details))
				{
					uint initialTimestamp;
					if (getInitialTimestamp(details.m_nPublishedFileId, out initialTimestamp))
					{
						debugMessage("Comparing '{0}' - Initial: {1} Latest: {2}", details.m_rgchTitle, initialTimestamp, details.m_rtimeUpdated);

						if (details.m_rtimeUpdated > initialTimestamp)
						{
							handleUpdateDetected(details);
						}
					}
					else
					{
						UnturnedLog.warn("Unable to find local timestamp for monitored workshop item '{0}' ({1})", details.m_rgchTitle, details.m_nPublishedFileId);
					}
				}
			}
		}

		/// <summary>
		/// Called once timer reaches interval.
		/// </summary>
		protected virtual void handleTimerTriggered()
		{
			submitQueryRequest(fileIdsForQuery);
		}

		protected bool getInitialTimestamp(PublishedFileId_t fileId, out uint timestamp)
		{
			foreach (MonitoredItem item in monitoredItems)
			{
				if (item.fileId == fileId)
				{
					timestamp = item.initialTimestamp;
					return true;
				}
			}

			timestamp = 0;
			return false;
		}

		/// <summary>
		/// Were update(s) detected that should be handled on next tick?
		/// </summary>
		protected bool shouldDoFinalTick;

		/// <summary>
		/// Are we done monitoring?
		/// Default finished once an update is detected.
		/// </summary>
		protected bool isFinishedTicking;

		protected MonitoredItem[] monitoredItems;
		protected PublishedFileId_t[] fileIdsForQuery;

		/// <summary>
		/// Interval between query submissions.
		/// </summary>
		protected float queryInterval;

		/// <summary>
		/// Accumulated time before submitting query after passing interval.
		/// </summary>
		protected float queryTimer;

		/// <summary>
		/// Callback from Steam when results from a call to submitQueryRequest are available.
		/// </summary>
		private void onQueryCompleted(SteamUGCQueryCompleted_t callback, bool ioFailure)
		{
			if (callback.m_handle != queryHandle)
			{
				// Not for us!
				return;
			}

			if (ioFailure)
			{
				UnturnedLog.warn("Encountered IO error when monitoring workshop changes");
			}
			else if (callback.m_eResult == EResult.k_EResultOK)
			{
				handleQueryResponse(callback);
			}
			else
			{
				UnturnedLog.warn("Encountered error '{0}' when monitoring workshop changes", callback.m_eResult);
			}

			SteamGameServerUGC.ReleaseQueryUGCRequest(queryHandle);
			queryHandle = UGCQueryHandle_t.Invalid;
		}

#pragma warning disable
		private CallResult<SteamUGCQueryCompleted_t> queryCompleted;
#pragma warning restore
		private UGCQueryHandle_t queryHandle;

		[System.Diagnostics.Conditional("LOG_WORKSHOP_UPDATE_MONITOR")]
		private void debugMessage(string message)
		{
			CommandWindow.Log(message);
		}

		[System.Diagnostics.Conditional("LOG_WORKSHOP_UPDATE_MONITOR")]
		private void debugMessage(string format, params object[] args)
		{
			CommandWindow.LogFormat(format, args);
		}
	}
}
