////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned
{
	public delegate void DedicatedUGCInstalledHandler();

	public static class DedicatedUGC
	{
		public static List<SteamContent> ugc
		{
			get;
			private set;
		}

		/// <summary>
		/// Broadcasts once all workshop assets are finished installing.
		/// </summary>
		public static event DedicatedUGCInstalledHandler installed;

		public static void registerItemInstallation(ulong id)
		{
			enqueueItemToQuery(new PublishedFileId_t(id));
		}

		/// <summary>
		/// Called once the server is done registering items it wants to install.
		/// </summary>
		/// <param name="onlyFromCache">True when running in offline-only mode.</param>
		public static void beginInstallingItems(bool onlyFromCache)
		{
			CommandWindow.Log(itemsToQuery.Count + " workshop item(s) requested");
			if (itemsToQuery.Count == 0)
			{
				// Done!
				OnFinishedDownloadingItems();
			}
			else
			{
				Assets.loadingStats.Reset();
				if (onlyFromCache)
				{
					installItemsToQueryFromCache();
				}
				else
				{
					submitQuery();
				}
			}
		}

		/// <summary>
		/// Request for details about the pending items.
		/// </summary>
		private static UGCQueryHandle_t queryHandle;

		/// <summary>
		/// File IDs of all the items we have enqueued for query.
		/// </summary>
		private static HashSet<ulong> itemsQueried;

		/// <summary>
		/// Built from user-specified workshop item IDs, and then expanded as the query results
		/// arrive with details about any dependent or child items.
		/// </summary>
		private static Queue<PublishedFileId_t> itemsToQuery;

		/// <summary>
		/// File IDs requested by the latest query submitted.
		/// </summary>
		private static PublishedFileId_t[] itemsPendingQuery;

		/// <summary>
		/// Number of times we've tried re-submitted failed queries.
		/// </summary>
		private static uint queryRetryCount = 0;

		private static uint maxQueryRetries => WorkshopDownloadConfig.get().Max_Query_Retries;

		/// <summary>
		/// Built as the valid list of items arrive.
		/// </summary>
		private static Queue<PublishedFileId_t> itemsToDownload;

		/// <summary>
		/// ID of the latest item we requested for download so that we can test if the callback is for us.
		/// </summary>
		private static PublishedFileId_t currentDownload;

		/// <summary>
		/// Enqueue an item if we have not queried it yet. This guards against querying an item
		/// that is in two separate collections leading to duplicates.
		/// </summary>
		private static bool enqueueItemToQuery(PublishedFileId_t item)
		{
			if (itemsQueried.Contains(item.m_PublishedFileId))
				return false;

			itemsToQuery.Enqueue(item);
			itemsQueried.Add(item.m_PublishedFileId);
			return true;
		}

		private static void enqueueItemToDownload(PublishedFileId_t item)
		{
			// This should not happen because we guard against querying the same item twice e.g.
			// two separate collections reference the same child item, but just in case...
			if (itemsToDownload.Contains(item))
			{
				UnturnedLog.warn("Tried to enqueue {0} for download more than once", item);
				return;
			}

			itemsToDownload.Enqueue(item);
		}

		/// <returns>True if item was installed from cache.</returns>
		private static bool installFromCache(PublishedFileId_t fileId)
		{
			ulong size;
			string path;
			uint localTimestamp;
			if (SteamGameServerUGC.GetItemInstallInfo(fileId, out size, out path, 1024, out localTimestamp) && ReadWrite.folderExists(path, false))
			{
				// Is installed on-disk, so maybe we can skip downloading.

				EItemState stateFlags = (EItemState) SteamGameServerUGC.GetItemState(fileId);
				bool needsUpdate = (stateFlags & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate;
				if (needsUpdate)
				{
					CommandWindow.LogFormat("Workshop item {0} found in cache, but was flagged as needing update", fileId);
				}
				else
				{
					// Use queried details to manually determine if local copy is out-of-date.
					SDG.Provider.CachedUGCDetails cachedDetails;
					bool hasCachedDetails = SDG.Provider.TempSteamworksWorkshop.getCachedDetails(fileId, out cachedDetails);

					// Will not have queried details if running offline-only.
					if (hasCachedDetails && cachedDetails.updateTimestamp > localTimestamp)
					{
						CommandWindow.LogFormat("Workshop item {0} found in cache, but remote ({1}) is newer than local ({2})",
							fileId,
							DateTimeEx.FromUtcUnixTimeSeconds(cachedDetails.updateTimestamp).ToLocalTime(),
							DateTimeEx.FromUtcUnixTimeSeconds(localTimestamp).ToLocalTime());
					}
					else
					{
						// Supposedly the cached item does not need an update, so install now.
						CommandWindow.Log("Workshop item found in cache: " + fileId);
						installDownloadedItem(fileId, path);
						return true;
					}
				}
			}

			return false;
		}

		private static void installNextItem()
		{
			if (itemsToDownload.Count == 0)
			{
				OnFinishedDownloadingItems();
			}
			else
			{
				PublishedFileId_t nextItem = itemsToDownload.Dequeue();

				bool installedFromCache = false;
				if (WorkshopDownloadConfig.get().Use_Cached_Downloads)
				{
					installedFromCache = installFromCache(nextItem);
				}

				if (installedFromCache)
				{
					installNextItem();
				}
				else
				{
					currentDownload = nextItem;
					CommandWindow.Log("Downloading workshop item: " + currentDownload);
					bool downloadStarted = SteamGameServerUGC.DownloadItem(currentDownload, true);
					if (downloadStarted == false)
					{
						CommandWindow.Log("Unable to download item!");
						installNextItem();
					}
				}
			}
		}

		/// <summary>
		/// Used in offline-only mode.
		/// </summary>
		private static void installItemsToQueryFromCache()
		{
			CommandWindow.Log("Only installing cached workshop files (no query / download)");

			while (itemsToQuery.Count > 0)
			{
				PublishedFileId_t nextItem = itemsToQuery.Dequeue();
				bool found = installFromCache(nextItem);
				if (!found)
				{
					CommandWindow.LogFormat("Unable to find workshop item in cache: {0}", nextItem);
				}
			}

			OnFinishedDownloadingItems();
		}

		/// <summary>
		/// Prepare a query that will request metadata for all the workshop items we want to install.
		/// This allows us to check if the items are allowed to be auto-downloaded to this server, and to
		/// detect any child or dependent items.
		///
		/// Waits for onQueryCompleted.
		/// </summary>
		private static void submitQuery()
		{
			CommandWindow.Log("Submitting workshop query for " + itemsToQuery.Count + " item(s)...");
			itemsPendingQuery = itemsToQuery.ToArray();
			itemsToQuery.Clear();
			submitQueryHelper(itemsPendingQuery);
		}

		/// <summary>
		/// Re-submit previous query after a query failure.
		/// </summary>
		private static void resubmitQuery()
		{
			++queryRetryCount;
			CommandWindow.LogFormat("Re-submitting ({0} of {1}) workshop query for {2} item(s)...", queryRetryCount, maxQueryRetries, itemsPendingQuery.Length);
			submitQueryHelper(itemsPendingQuery);
		}

		private static void submitQueryHelper(PublishedFileId_t[] fileIDs)
		{
			queryHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(fileIDs, (uint) fileIDs.Length);
			SteamGameServerUGC.SetReturnKeyValueTags(queryHandle, true);
			SteamGameServerUGC.SetReturnChildren(queryHandle, true);

			uint maxAgeSeconds = WorkshopDownloadConfig.get().Query_Cache_Max_Age_Seconds;
			if (maxAgeSeconds > 0)
			{
				SteamGameServerUGC.SetAllowCachedResponse(queryHandle, maxAgeSeconds);
			}

			SteamAPICall_t callHandle = SteamGameServerUGC.SendQueryUGCRequest(queryHandle);
			queryCompleted.Set(callHandle);
		}

		private static bool testDownloadRestrictions(UGCQueryHandle_t queryHandle, uint resultIndex, uint ip, string itemDisplayText)
		{
			EWorkshopDownloadRestrictionResult restriction = WorkshopDownloadRestrictions.getRestrictionResult(queryHandle, resultIndex, ip);
			switch (restriction)
			{
				case EWorkshopDownloadRestrictionResult.NoRestrictions:
					return true;

				case EWorkshopDownloadRestrictionResult.NotWhitelisted:
				{
					CommandWindow.LogWarning("Not authorized in the IP whitelist for " + itemDisplayText);
					return false;
				}

				case EWorkshopDownloadRestrictionResult.Blacklisted:
				{
					CommandWindow.LogWarning("Blocked in IP blacklist from downloading " + itemDisplayText);
					return false;
				}

				case EWorkshopDownloadRestrictionResult.Allowed:
				{
					CommandWindow.Log("Authorized to download " + itemDisplayText);
					return true;
				}

				case EWorkshopDownloadRestrictionResult.Banned:
				{
					CommandWindow.LogWarning("Workshop file is banned " + itemDisplayText);
					return false;
				}

				case EWorkshopDownloadRestrictionResult.PrivateVisibility:
				{
					CommandWindow.LogWarning("Workshop file is private " + itemDisplayText);
					return false;
				}

				default:
				{
					CommandWindow.LogWarningFormat("Unknown restriction result '{0}' for '{1}'", restriction, itemDisplayText);
					return false;
				}
			}
		}

		private static void OnNextFrameResubmitQuery()
		{
			SDG.Framework.Utilities.TimeUtility.updated -= OnNextFrameResubmitQuery;
			resubmitQuery();
		}

		private static void OnNextFrameSubmitQuery()
		{
			SDG.Framework.Utilities.TimeUtility.updated -= OnNextFrameSubmitQuery;
			submitQuery();
		}

#pragma warning disable
		private static CallResult<SteamUGCQueryCompleted_t> queryCompleted;
#pragma warning restore
		private static void onQueryCompleted(SteamUGCQueryCompleted_t callback, bool ioFailure)
		{
			if (callback.m_handle != queryHandle)
			{
				// Not for us!
				return;
			}

			// Do we need to resubmit this query?
			// False if successful, true if failed.
			bool needsResubmit;

			if (!ioFailure)
			{
				if (callback.m_eResult == EResult.k_EResultOK)
				{
					needsResubmit = false;

					CommandWindow.Log("Workshop query yielded " + callback.m_unNumResultsReturned + " result(s)");

					// Needs to be updated for IPv6. Originally Steam only supported IPv4, but now supports IPv6.
					SteamIPAddress_t steamAddress = SteamGameServer.GetPublicIP();
					uint allowedPublicIP;
					steamAddress.TryGetIPv4Address(out allowedPublicIP);
					string displayAllowedPublicIP = Parser.getIPFromUInt32(allowedPublicIP);
					if (Logs.ShouldRedactLogs)
					{
						displayAllowedPublicIP = Logs.RedactionReplacement;
					}
					CommandWindow.Log("This server's allowed IP for Workshop downloads: " + displayAllowedPublicIP);

					for (uint resultIndex = 0; resultIndex < callback.m_unNumResultsReturned; resultIndex++)
					{
						SteamUGCDetails_t details;
						bool hasDetails = SteamGameServerUGC.GetQueryUGCResult(queryHandle, resultIndex, out details);
						if (!hasDetails)
						{
							CommandWindow.LogWarning($"Workshop query unable to get details for result index {resultIndex}");
							continue;
						}

						string itemDisplayText = details.m_nPublishedFileId + " '" + details.m_rgchTitle + "'";

						// Check result before restrictions because some results are treated as restrictions.
						if (details.m_eResult != EResult.k_EResultOK)
						{
							CommandWindow.LogWarning($"Error {details.m_eResult} querying workshop file {itemDisplayText}");
							continue;
						}

						bool isAllowedToDownload = testDownloadRestrictions(queryHandle, resultIndex, allowedPublicIP, itemDisplayText);
						if (!isAllowedToDownload)
							continue;

						SDG.Provider.CachedUGCDetails cachedDetails;
						SDG.Provider.TempSteamworksWorkshop.cacheDetails(queryHandle, resultIndex, out cachedDetails);

						if (details.m_eFileType != EWorkshopFileType.k_EWorkshopFileTypeCollection)
						{
							CommandWindow.Log(itemDisplayText + " queued for download");
							enqueueItemToDownload(details.m_nPublishedFileId);
						}

						// Children are dependencies or items in a collection.
						uint numChildren = details.m_unNumChildren;
						if (numChildren > 0)
						{
							if (WorkshopDownloadConfig.get().Ignore_Children_File_IDs.Contains(details.m_nPublishedFileId.m_PublishedFileId))
							{
								CommandWindow.LogFormat("Ignoring {0} children of {1}", numChildren, itemDisplayText);
							}
							else
							{
								CommandWindow.Log(itemDisplayText + " has " + numChildren + " children");
								PublishedFileId_t[] childIds = new PublishedFileId_t[numChildren];
								if (SteamGameServerUGC.GetQueryUGCChildren(queryHandle, resultIndex, childIds, numChildren))
								{
									foreach (PublishedFileId_t child in childIds)
									{
										bool enqueued = enqueueItemToQuery(child);
										CommandWindow.LogFormat(enqueued ? "\t{0}" : "\t{0} (already queued)", child);
									}
								}
							}
						}
					}
				}
				else
				{
					needsResubmit = true;
					CommandWindow.LogError("Encountered an error when querying workshop: " + callback.m_eResult);
				}
			}
			else
			{
				needsResubmit = true;
				CommandWindow.LogError("Encountered an IO error when querying workshop!");
			}

			SteamGameServerUGC.ReleaseQueryUGCRequest(queryHandle);
			queryHandle = UGCQueryHandle_t.Invalid;

			if (needsResubmit)
			{
				if (queryRetryCount < maxQueryRetries)
				{
					SDG.Framework.Utilities.TimeUtility.updated += OnNextFrameResubmitQuery;
				}
				else
				{
					CommandWindow.LogWarning("Reached maximum workshop query retry count!");
					Provider.QuitGame("reached maximum workshop query retry count");
				}
			}
			else if (itemsToQuery.Count > 0)
			{
				// Found child items to query.
				SDG.Framework.Utilities.TimeUtility.updated += OnNextFrameSubmitQuery;
			}
			else
			{
				CommandWindow.Log(itemsToDownload.Count + " workshop item(s) to download...");
				installNextItem();
			}
		}

		private static void installDownloadedItem(PublishedFileId_t fileId, string path)
		{
			ESteamUGCType type;
			if (WorkshopTool.detectUGCMetaType(path, false, out type))
			{
				CommandWindow.LogFormat("Installing workshop item: {0}", fileId);

				string warningMessage;
				if (!SDG.Provider.TempSteamworksWorkshop.isCompatible(fileId, type, path, out warningMessage))
				{
					CommandWindow.LogWarning(warningMessage);
				}

				string ignoreExplanation;
				if (SDG.Provider.TempSteamworksWorkshop.shouldIgnoreFile(fileId, out ignoreExplanation))
				{
					CommandWindow.LogFormat("Ignoring downloaded workshop item {0} because '{1}'");
					// Even if we ignore, we still register as using the item for clients to download.
				}
				else
				{
					ugc.Add(new SteamContent(fileId, path, type));

					switch (type)
					{
						case ESteamUGCType.MAP:
							WorkshopTool.loadMapBundlesAndContent(path, fileId.m_PublishedFileId);
							break;

						case ESteamUGCType.LOCALIZATION:
							break;

						default:
							Assets.RequestAddSearchLocation(path, SDG.Provider.TempSteamworksWorkshop.FindOrAddOrigin(fileId.m_PublishedFileId));
							break;
					}

					CommandWindow.LogFormat("Installed workshop item: {0}", fileId);
				}

				uint timestamp = 0;
				if (SDG.Provider.TempSteamworksWorkshop.getCachedDetails(fileId, out SDG.Provider.CachedUGCDetails cachedDetails))
				{
					timestamp = cachedDetails.updateTimestamp;
				}

				// Register as a requirement for joining the server only once we have loaded it.
				Provider.registerServerUsingWorkshopFileId(fileId.m_PublishedFileId, timestamp);
			}
			else
			{
				CommandWindow.LogWarning("Unable to determine UGC type for downloaded item: " + fileId);
			}
		}

#pragma warning disable
		private static Callback<DownloadItemResult_t> itemDownloaded;
#pragma warning restore
		private static void onItemDownloaded(DownloadItemResult_t callback)
		{
			if (callback.m_nPublishedFileId != currentDownload)
			{
				// Not for us.
				return;
			}

			if (callback.m_eResult == EResult.k_EResultOK)
			{
				CommandWindow.Log("Successfully downloaded workshop item: " + callback.m_nPublishedFileId.m_PublishedFileId);

				ulong size;
				string path;
				uint timestamp;

				if (SteamGameServerUGC.GetItemInstallInfo(callback.m_nPublishedFileId, out size, out path, 1024, out timestamp))
				{
					if (ReadWrite.folderExists(path, false))
					{
						installDownloadedItem(callback.m_nPublishedFileId, path);
					}
					else
					{
						CommandWindow.LogWarningFormat("Finished downloading workshop item {0}, but unable to find the files on disk ({1})", callback.m_nPublishedFileId, path);
					}
				}
				else
				{
					CommandWindow.LogWarningFormat("Finished downloading workshop item {0}, but unable to get install info", callback.m_nPublishedFileId);
				}
			}
			else
			{
				CommandWindow.LogWarningFormat("Download workshop item {0} failed, result: {1}", callback.m_nPublishedFileId, callback.m_eResult);
			}

			installNextItem();
		}

		public static void initialize()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				throw new System.NotSupportedException("DedicatedUGC should only be used on dedicated server!");
			}

			ugc = new List<SteamContent>();
			itemsQueried = new HashSet<ulong>();
			itemsToQuery = new Queue<PublishedFileId_t>();
			itemsToDownload = new Queue<PublishedFileId_t>();

			queryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onQueryCompleted);
			itemDownloaded = Callback<DownloadItemResult_t>.CreateGameServer(onItemDownloaded);

			string workshopInstallFolder = ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Workshop/Steam";
			if (!Directory.Exists(workshopInstallFolder))
			{
				Directory.CreateDirectory(workshopInstallFolder);
			}

			CommandWindow.Log("Workshop install folder: " + workshopInstallFolder);
			SteamGameServerUGC.BInitWorkshopForGameServer((DepotId_t) Provider.APP_ID.m_AppId, workshopInstallFolder);
		}

		private static bool linkedSpawns = false;
		private static bool initializedValidation = false;

		private static void OnFinishedDownloadingItems()
		{
			if (Assets.ShouldWaitForNewAssetsToFinishLoading)
			{
				UnturnedLog.info("Server UGC waiting for assets to finish loading...");
				Assets.OnNewAssetsFinishedLoading += OnNewAssetsFinishedLoading;
			}
			else
			{
				OnNewAssetsFinishedLoading();
			}
		}

		private static void OnNewAssetsFinishedLoading()
		{
			Assets.OnNewAssetsFinishedLoading -= OnNewAssetsFinishedLoading;

			if (!linkedSpawns)
			{
				linkedSpawns = true;
				Assets.linkSpawns(); // Server is done loading every asset it will need
			}

			if (!initializedValidation)
			{
				initializedValidation = true;
				Assets.initializeMasterBundleValidation();
			}

			installed?.Invoke();
		}
	}
}
