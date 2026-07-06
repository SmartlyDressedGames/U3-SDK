////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.IO;

namespace SDG.Provider
{
	/// <summary>
	/// Details of a workshop item that the game may want to refer to later.
	/// Cached during client startup after getting installed items, and while
	/// downloading UGC for the dedicated server.
	/// </summary>
	public struct CachedUGCDetails
	{
		public PublishedFileId_t fileId;
		public string name;
		public byte compatibilityVersion;

		/// <summary>
		/// Banned workshop files are shown in red.
		/// </summary>
		public bool isBannedOrPrivate;

		/// <summary>
		/// Used on dedicated server to test whether map has been updated, and whether local copy of file is out-of-date.
		/// </summary>
		public uint updateTimestamp;

		/// <summary>
		/// Some workshop copyright infringers use an empty title, in which case we show the file ID as title text.
		/// </summary>
		public string GetTitle()
		{
			return string.IsNullOrEmpty(name) ? fileId.ToString() : name;
		}
	}

	public class TempSteamworksWorkshop
	{
		/// <summary>
		/// Used when transitioning Unity versions breaks asset bundles. Replaced by AssetBundleVersion const values.
		/// </summary>
		[System.Obsolete]
		public static readonly byte COMPATIBILITY_VERSION = 3;

		/// <summary>
		/// Workshop item key-value tag storing the version number.
		/// </summary>
		public static readonly string COMPATIBILITY_VERSION_KVTAG = "compatibility_version";

		public bool canOpenWorkshop => true;// SteamUtils.IsOverlayEnabled();

		public void open(Steamworks.PublishedFileId_t id)
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/filedetails/?id=" + id.m_PublishedFileId);
		}

		private SDG.SteamworksProvider.SteamworksAppInfo appInfo;

		public delegate void PublishedAdded();
		public delegate void PublishedRemoved();

		public PublishedAdded onPublishedAdded;
		public PublishedRemoved onPublishedRemoved;

		private PublishedFileId_t publishedFileID;
		private UGCQueryHandle_t ugcRequest;
		private uint ugcRequestPage;
		private bool shouldRequestAnotherPage;
		private string ugcName;
		private string ugcDescription;
		private string ugcPath;
		private string ugcPreview;
		private string ugcChange;
		private ESteamUGCType ugcType;
		private List<string> ugcTags;
		private string ugcAllowedIPs;
		private ESteamUGCVisibility ugcVisibility;
		private bool ugcVerified;

		public int totalNumberOfFilesToDownload;
		private float progressPerFileDownloaded;
		public List<PublishedFileId_t> downloaded;
		public List<PublishedFileId_t> installing;

		private List<SteamContent> _ugc;
		public List<SteamContent> ugc => _ugc;

		private List<SteamPublished> _published;
		public List<SteamPublished> published => _published;

		/// <summary>
		/// Maps published file id to name, version, etc.
		/// </summary>
		private static Dictionary<ulong, CachedUGCDetails> cachedUGCDetails = new Dictionary<ulong, CachedUGCDetails>();

		/// <summary>
		/// Get compatibility version from workshop query, or zero if unset.
		/// </summary>
		public static byte getCompatibilityVersion(UGCQueryHandle_t queryHandle, uint index)
		{
			uint numKeyValueTags = Dedicator.IsDedicatedServer ? SteamGameServerUGC.GetQueryUGCNumKeyValueTags(queryHandle, index) : SteamUGC.GetQueryUGCNumKeyValueTags(queryHandle, index);
			for (uint tagIndex = 0; tagIndex < numKeyValueTags; tagIndex++)
			{
				string key;
				string value;
				if (Dedicator.IsDedicatedServer ? SteamGameServerUGC.GetQueryUGCKeyValueTag(queryHandle, index, tagIndex, out key, 255, out value, 255) : SteamUGC.GetQueryUGCKeyValueTag(queryHandle, index, tagIndex, out key, 255, out value, 255))
				{
					if (key.Equals(COMPATIBILITY_VERSION_KVTAG, System.StringComparison.InvariantCultureIgnoreCase))
					{
						byte parsedVersion;
						if (byte.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedVersion))
						{
							return parsedVersion;
						}
						else
						{
							UnturnedLog.warn("Unable to parse workshop item compatibility version from '{0}'", value);
							return 0;
						}
					}
				}
			}

			return 0;
		}

		private static void DumpDetails(in SteamUGCDetails_t details)
		{
			UnturnedLog.info("{0} \"{1}\"", details.m_nPublishedFileId, details.m_rgchTitle);
			UnturnedLog.info("\tBanned: {0}", details.m_bBanned);
			UnturnedLog.info("\tResult: {0}", details.m_eResult);
			UnturnedLog.info("\tVisibility: {0}", details.m_eVisibility);
		}

		/// <summary>
		/// Save the details from a workshop query for lookup later.
		/// Allows game to inspect the installed files before deciding if they are
		/// compatible, since maps and localization are not affected by unity upgrades.
		/// Previously the compatibility test occurred before downloading the content.
		/// </summary>
		public static bool cacheDetails(UGCQueryHandle_t queryHandle, uint index, out CachedUGCDetails cachedDetails)
		{
			cachedDetails = new CachedUGCDetails();

			SteamUGCDetails_t details;
			bool hasDetails = Dedicator.IsDedicatedServer ? SteamGameServerUGC.GetQueryUGCResult(queryHandle, index, out details) : SteamUGC.GetQueryUGCResult(queryHandle, index, out details);
			if (hasDetails)
			{
				//DumpDetails(details);

				PublishedFileId_t fileId = details.m_nPublishedFileId;
				byte version = getCompatibilityVersion(queryHandle, index);

				cachedDetails.fileId = fileId;
				cachedDetails.name = details.m_rgchTitle;
				cachedDetails.compatibilityVersion = version;
				cachedDetails.isBannedOrPrivate = details.m_bBanned ||
					details.m_eVisibility == ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate ||
					details.m_eResult == EResult.k_EResultAccessDenied; // DMCA takedown items can be unbanned and public.
				cachedDetails.updateTimestamp = MathfEx.Max(details.m_rtimeCreated, details.m_rtimeUpdated);

				cachedUGCDetails[fileId.m_PublishedFileId] = cachedDetails;

				// Updated name if possible.
				if (!string.IsNullOrEmpty(cachedDetails.name))
				{
					AssetOrigin origin = Assets.FindWorkshopFileOrigin(fileId.m_PublishedFileId);
					if (origin != null)
					{
						origin.name = $"Workshop File \"{cachedDetails.name}\" ({cachedDetails.fileId})";
					}
				}
			}
			else
			{
				UnturnedLog.warn("Unable to get query UGC result for caching");
			}

			return hasDetails;
		}

		/// <summary>
		/// Get cached workshop item details.
		/// </summary>
		public static bool getCachedDetails(PublishedFileId_t fileId, out CachedUGCDetails cachedDetails)
		{
			return cachedUGCDetails.TryGetValue(fileId.m_PublishedFileId, out cachedDetails);
		}

		public static AssetOrigin FindOrAddOrigin(ulong fileId)
		{
			AssetOrigin origin = Assets.FindOrAddWorkshopFileOrigin(fileId, true);

			// Updated name if possible.
			CachedUGCDetails details;
			if (cachedUGCDetails.TryGetValue(fileId, out details) && !string.IsNullOrEmpty(details.name))
			{
				origin.name = $"Workshop File \"{details.name}\" ({details.fileId})";
			}

			return origin;
		}

		public static bool isCompatible(PublishedFileId_t fileId, ESteamUGCType type, string dir, out string explanation)
		{
			CachedUGCDetails cachedDetails;
			if (!getCachedDetails(fileId, out cachedDetails))
			{
				// Query failed so default to compatible.
				explanation = null;
				return true;
			}

			bool assetBundleCompatMatters;
			switch (type)
			{
				case ESteamUGCType.MAP:
					// Map only relies on asset bundles if it has custom bundles.
					assetBundleCompatMatters = Directory.Exists(dir + "/Bundles");
					break;

				case ESteamUGCType.LOCALIZATION:
					// Localization does not use asset bundles.
					assetBundleCompatMatters = false;
					break;

				default:
					// Items, vehicles, objects, etc which rely on asset bundles.
					assetBundleCompatMatters = true;
					break;
			}

			if (assetBundleCompatMatters)
			{
				if (cachedDetails.compatibilityVersion < AssetBundleVersion.UNITY_2017_LTS)
				{
					explanation = string.Format("Workshop version of \"{0}\" has not yet been updated from Unity 5.5 and cannot be loaded.", cachedDetails.GetTitle());
					return false;
				}
				else if (cachedDetails.compatibilityVersion > AssetBundleVersion.NEWEST)
				{
					explanation = string.Format("Workshop version of \"{0}\" has been updated to an unknown future version of Unity and cannot be loaded.", cachedDetails.GetTitle());
					return false;
				}
			}

			explanation = null;
			return true;
		}

		private static readonly PublishedFileId_t FRANCE = new PublishedFileId_t(1975500516);
		private static readonly PublishedFileId_t CALIFORNIA = new PublishedFileId_t(1905768396);

		/// <summary>
		/// Should caller skip loading a given workshop file?
		/// 
		/// Used to skip workshop version of map if the map is locally installed,
		/// e.g. Canyon Arena moved to workshop and auto-subscribed.
		/// </summary>
		public static bool shouldIgnoreFile(PublishedFileId_t fileId, out string explanation)
		{
			if (fileId == FRANCE)
			{
				// Post-patch it turned out some players had unlocked or edited the map preventing it from being deleted,
				// so we check for the config file which is unlikely to have been edited.
				string mapRelativePath = "/Maps/France/Config.json";
				if (ReadWrite.fileExists(mapRelativePath, useCloud: false, usePath: true))
				{
					explanation = "non-Workshop version of France is still installed";
					return true;
				}
			}
			else if (fileId == CALIFORNIA && !Dedicator.IsDedicatedServer)
			{
				// Loading subscriptions happens earlier in startup than auto-subscribe/unsubscribe. Unfortunately,
				// Cali 2 has some conflicts with Cali 1, so we auto-unsubscribe it later and in the meantime we
				// prevent loading it (once only).
				if (!ConvenientSavedata.get().hasFlag("Skipped_Cali1"))
				{
					ConvenientSavedata.get().setFlag("Skipped_Cali1");
					explanation = "skipped loading California 1 in case California 2 is about to install";
					return true;
				}
			}

			explanation = null;
			return false;
		}

#pragma warning disable
		private CallResult<CreateItemResult_t> createItemResult;
#pragma warning restore
		private void onCreateItemResult(CreateItemResult_t callback, bool io)
		{
			if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				MenuUI.alert(MenuDashboardUI.localization.format("UGC_NeedsToAcceptWorkshopLegalAgreement"));
				return;
			}

			if (callback.m_eResult != EResult.k_EResultOK)
			{
				MenuUI.alert(MenuDashboardUI.localization.format("UGC_UnknownResult", callback.m_eResult));
				return;
			}

			if (io)
			{
				MenuUI.alert(MenuDashboardUI.localization.format("UGC_IOError"));
				return;
			}

			publishedFileID = callback.m_nPublishedFileId;

			updateUGC();
		}

#pragma warning disable
		private CallResult<SubmitItemUpdateResult_t> submitItemUpdateResult;
#pragma warning restore
		private void onSubmitItemUpdateResult(SubmitItemUpdateResult_t callback, bool io)
		{
			if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				MenuUI.alert(MenuDashboardUI.localization.format("UGC_NeedsToAcceptWorkshopLegalAgreement"));
				return;
			}

			if (callback.m_eResult != EResult.k_EResultOK)
			{
				MenuUI.alert(MenuDashboardUI.localization.format("UGC_UnknownResult", callback.m_eResult));
				return;
			}

			if (io)
			{
				MenuUI.alert(MenuDashboardUI.localization.format("UGC_IOError"));
				return;
			}

			MenuUI.alert(MenuDashboardUI.localization.format("UGC_Success"));

			SDG.Unturned.Provider.provider.workshopService.open(publishedFileID);
			refreshPublished();
		}

#pragma warning disable
		private CallResult<SteamUGCQueryCompleted_t> queryCompleted;
#pragma warning restore
		private void onQueryCompleted(SteamUGCQueryCompleted_t callback, bool io)
		{
			if (callback.m_eResult != EResult.k_EResultOK || io)
			{
				return;
			}

			if (callback.m_unNumResultsReturned < 1)
			{
				return;
			}

			for (uint index = 0; index < callback.m_unNumResultsReturned; index++)
			{
				SteamUGCDetails_t details;
				SteamUGC.GetQueryUGCResult(ugcRequest, index, out details);

				SteamPublished content = new SteamPublished(details.m_rgchTitle, details.m_nPublishedFileId);

				published.Add(content);
			}

			onPublishedAdded?.Invoke();

			cleanupUGCRequest();

			shouldRequestAnotherPage = true;
		}

		public void update()
		{
			if (shouldRequestAnotherPage)
			{
				shouldRequestAnotherPage = false;

				ugcRequestPage++;
				ugcRequest = SteamUGC.CreateQueryUserUGCRequest(SDG.Unturned.Provider.client.GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, SteamUtils.GetAppID(), SteamUtils.GetAppID(), ugcRequestPage);
				SteamAPICall_t handle = SteamUGC.SendQueryUGCRequest(ugcRequest);
				queryCompleted.Set(handle);
			}

			if (currentlyDownloadingFileId != PublishedFileId_t.Invalid)
			{
				// Note: this method returns false shortly after calling DownloadItem.
				ulong bytesDownloaded;
				ulong bytesTotal;
				if (SteamUGC.GetItemDownloadInfo(currentlyDownloadingFileId, out bytesDownloaded, out bytesTotal) && bytesTotal > 0)
				{
					// UnturnedLog.info($"GetItemDownloadInfo: {bytesDownloaded} / {bytesTotal} bytes");
					currentlyDownloadingFileEstimatedProgress = (float) (bytesDownloaded / (double) bytesTotal);
					if (bytesDownloaded >= bytesTotal)
					{
						currentlyDownloadingFileId = PublishedFileId_t.Invalid;
					}
					UpdateEstimatedDownloadProgress();
				}
			}
		}

		private float currentlyDownloadingFileEstimatedProgress;
		private int previousEstimatedDownloadProgress;
		private PublishedFileId_t currentlyDownloadingFileId;

		private void UpdateEstimatedDownloadProgress()
		{
			float progress = progressPerFileDownloaded * (totalNumberOfFilesToDownload - installing.Count) + progressPerFileDownloaded * currentlyDownloadingFileEstimatedProgress;
			int roundedProgress = UnityEngine.Mathf.RoundToInt(progress * 100.0f);
			if (previousEstimatedDownloadProgress != roundedProgress)
			{
				previousEstimatedDownloadProgress = roundedProgress;
				LoadingUI.NotifyDownloadProgress(progress);
			}
		}

		private void OnFinishedDownloadingItems()
		{
			if (Assets.ShouldWaitForNewAssetsToFinishLoading)
			{
				UnturnedLog.info("Client UGC waiting for assets to finish loading...");
				Assets.OnNewAssetsFinishedLoading += OnNewAssetsFinishedLoading;
			}
			else
			{
				OnNewAssetsFinishedLoading();
			}
		}

		private void OnNewAssetsFinishedLoading()
		{
			Assets.OnNewAssetsFinishedLoading -= OnNewAssetsFinishedLoading;
			SDG.Unturned.Provider.launch();
		}

		public void downloadNextItem()
		{
			if (installing.Count == 0)
			{
				LoadingUI.SetIsDownloading(false);
				OnFinishedDownloadingItems();
			}
			else
			{
				PublishedFileId_t id = installing[0];

				string name;
				CachedUGCDetails details;
				if (getCachedDetails(id, out details))
				{
					name = details.GetTitle();
				}
				else
				{
					name = "Unknown ID " + id;
				}

				LoadingUI.SetDownloadFileName(name);

				currentlyDownloadingFileId = id;
				currentlyDownloadingFileEstimatedProgress = 0.0f;
				SteamUGC.DownloadItem(id, true);
			}
		}

		/// <summary>
		/// Helper for downloadServerItems.
		/// Called for each workshop item we want to download for the server.
		/// </summary>
		private void enqueueServerItemDownloadOrInstallFromCache(PublishedFileId_t fileId)
		{
			bool alreadyRegistered = isInstalledItemAlreadyRegistered(fileId);

			string ignoreExplanation;
			if (shouldIgnoreFile(fileId, out ignoreExplanation))
			{
				UnturnedLog.info("Ignoring server download {0} because '{1}'", fileId, ignoreExplanation);
				return;
			}

			ulong size;
			string path;
			uint localTimestamp;

			if (SteamUGC.GetItemInstallInfo(fileId, out size, out path, 1024, out localTimestamp) && ReadWrite.folderExists(path, false))
			{
				// Is installed on-disk, so maybe we can skip downloading.

				EItemState stateFlags = (EItemState) SteamUGC.GetItemState(fileId);
				bool needsUpdate = (stateFlags & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate;
				if (needsUpdate)
				{
					if (alreadyRegistered)
					{
						UnturnedLog.info($"Server workshop file {fileId} is already loaded, but was flagged as needing update");
					}
					else
					{
						UnturnedLog.info("Server workshop item {0} found in cache, but was flagged as needing update", fileId);

						// Enqueue for download.
						installing.Add(fileId);
					}
				}
				else
				{
					// Use queried details to manually determine if local copy is out-of-date.
					CachedUGCDetails cachedDetails;
					bool hasCachedDetails = getCachedDetails(fileId, out cachedDetails);

					if (hasCachedDetails && cachedDetails.updateTimestamp > localTimestamp)
					{
						if (alreadyRegistered)
						{
							UnturnedLog.info("Server workshop file {0} is already loaded, but remote ({1}) is newer than local ({2})",
								fileId,
								DateTimeEx.FromUtcUnixTimeSeconds(cachedDetails.updateTimestamp).ToLocalTime(),
								DateTimeEx.FromUtcUnixTimeSeconds(localTimestamp).ToLocalTime());
						}
						else
						{
							UnturnedLog.info("Server workshop item {0} found in cache, but remote ({1}) is newer than local ({2})",
								fileId,
								DateTimeEx.FromUtcUnixTimeSeconds(cachedDetails.updateTimestamp).ToLocalTime(),
								DateTimeEx.FromUtcUnixTimeSeconds(localTimestamp).ToLocalTime());

							// Enqueue for download.
							installing.Add(fileId);
						}
					}
					else
					{
						if (!alreadyRegistered)
						{
							// Supposedly the cached item does not need an update, so install now.
							UnturnedLog.info("Installing cached server workshop item: " + fileId);
							installItemDownloadedFromServer(fileId, path);
						}
					}
				}
			}
			else
			{
				// Enqueue for download.
				installing.Add(fileId);
			}
		}

		/// <summary>
		/// Called once we know which items the server is allowed to use (queryServerItems),
		/// or the query has failed in which case we proceed with all items it told us.
		/// </summary>
		private void downloadServerItems(List<PublishedFileId_t> itemIDs)
		{
			installing = new List<PublishedFileId_t>();

			Assets.loadingStats.Reset();

			foreach (PublishedFileId_t file in itemIDs)
			{
				enqueueServerItemDownloadOrInstallFromCache(file);
			}

			if (installing.Count < 1)
			{
				UnturnedLog.info("Server has {0} valid workshop item(s), but we already have them downloaded", itemIDs.Count);
				OnFinishedDownloadingItems();
			}
			else
			{
				UnturnedLog.info("Server has {0} valid workshop item(s), of which {1} need to be downloaded", itemIDs.Count, installing.Count);
				totalNumberOfFilesToDownload = installing.Count;
				progressPerFileDownloaded = 1.0f / totalNumberOfFilesToDownload;
				previousEstimatedDownloadProgress = 0;
				LoadingUI.SetIsDownloading(true);
				LoadingUI.NotifyDownloadProgress(0.0f);
				downloadNextItem();
			}
		}

		/// <summary>
		/// Is currently connected server allowed to auto-download the workshop item?
		/// Requested by mod authors so that they can whitelist/blacklist access.
		/// </summary>
		private bool testDownloadRestrictions(UGCQueryHandle_t queryHandle, uint resultIndex, uint ip, string itemDisplayText)
		{
			if (ip == 0)
				return true;

			EWorkshopDownloadRestrictionResult restriction = WorkshopDownloadRestrictions.getRestrictionResult(queryHandle, resultIndex, ip);
			switch (restriction)
			{
				case EWorkshopDownloadRestrictionResult.NoRestrictions:
					return true;

				case EWorkshopDownloadRestrictionResult.NotWhitelisted:
				{
					UnturnedLog.warn("Server is not authorized in the IP whitelist for " + itemDisplayText);
					return false;
				}

				case EWorkshopDownloadRestrictionResult.Blacklisted:
				{
					UnturnedLog.warn("Server is blocked in IP blacklist from downloading " + itemDisplayText);
					return false;
				}

				case EWorkshopDownloadRestrictionResult.Allowed:
				{
					UnturnedLog.info("Server is authorized to download " + itemDisplayText);
					return true;
				}

				case EWorkshopDownloadRestrictionResult.Banned:
				{
					UnturnedLog.warn("Workshop file is banned " + itemDisplayText);
					return false;
				}

				case EWorkshopDownloadRestrictionResult.PrivateVisibility:
				{
					UnturnedLog.warn("Workshop file is private " + itemDisplayText);
					return false;
				}

				default:
				{
					UnturnedLog.warn("Unknown restriction result '{0}' for '{1}'", restriction, itemDisplayText);
					return true; // Server defaults to false, but client defaults to true considering server sent it to us.
				}
			}
		}

		/// <summary>
		/// Successfully queried details of the items current server is using.
		/// Ensure server has permission to use these items, then proceed with downloading.
		/// Also caches item titles for use on the loading screen.
		/// </summary>
		private void handleServerItemsQuerySuccess(SteamUGCQueryCompleted_t callback)
		{
			string displayAllowedPublicIP = Parser.getIPFromUInt32(serverDownloadIP);
			UnturnedLog.info("Server's allowed IP for Workshop downloads: " + displayAllowedPublicIP);

			// Reserve capacity for the items returned.
			serverPendingIDs = new List<PublishedFileId_t>((int) callback.m_unNumResultsReturned);

			for (uint resultIndex = 0; resultIndex < callback.m_unNumResultsReturned; resultIndex++)
			{
				CachedUGCDetails cachedDetails;
				cacheDetails(callback.m_handle, resultIndex, out cachedDetails);

				bool canDownload = testDownloadRestrictions(callback.m_handle, resultIndex, serverDownloadIP, cachedDetails.GetTitle());
				if (canDownload)
				{
					serverPendingIDs.Add(cachedDetails.fileId);
				}
				else
				{
					++serverInvalidItemsCount;
				}
			}

			downloadServerItems(serverPendingIDs);
		}

		/// <summary>
		/// IO or bad result occurred when querying items the current server is using.
		/// We do not know the file details, but we proceed with downloading them all.
		/// </summary>
		private void handleServerItemsQueryFailed()
		{
			downloadServerItems(serverPendingIDs);
		}

		private UGCQueryHandle_t serverItemsQueryHandle;
#pragma warning disable
		private CallResult<SteamUGCQueryCompleted_t> serverItemsQueryCompleted;
#pragma warning restore
		private void onServerItemsQueryCompleted(SteamUGCQueryCompleted_t callback, bool ioFailure)
		{
			if (callback.m_handle != serverItemsQueryHandle)
			{
				// Not for us!
				return;
			}

			if (ioFailure)
			{
				UnturnedLog.error("IO error querying workshop for server items!");
				handleServerItemsQueryFailed();
			}
			else
			{
				if (callback.m_eResult == EResult.k_EResultOK)
				{
					handleServerItemsQuerySuccess(callback);
				}
				else
				{
					UnturnedLog.error("Error querying workshop for server items: " + callback.m_eResult);
					handleServerItemsQueryFailed();
				}
			}

			SteamUGC.ReleaseQueryUGCRequest(serverItemsQueryHandle);
			serverItemsQueryHandle = UGCQueryHandle_t.Invalid;
		}

		/// <summary>
		/// File IDs the client knows the server is using. Fallback in-case the query fails.
		/// </summary>
		internal List<PublishedFileId_t> serverPendingIDs;

		/// <summary>
		/// IP of the currently connected server, or zero if unable to retrieve from network system.
		/// Used for testing download restrictions.
		/// </summary>
		protected uint serverDownloadIP;

		/// <summary>
		/// Number of items currently connected server was not authorized to download.
		/// </summary>
		public int serverInvalidItemsCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Called prior to downloading, and after a connection failure.
		/// </summary>
		public void resetServerInvalidItems()
		{
			serverPendingIDs = null;
			serverInvalidItemsCount = 0;
		}

		/// <summary>
		/// Client now knows the published file IDs the server is using, but
		/// queries the workshop for additional information before installing.
		/// </summary>
		public void queryServerWorkshopItems(List<PublishedFileId_t> fileIDs, uint serverIP)
		{
			serverPendingIDs = fileIDs;
			serverDownloadIP = serverIP;

			serverItemsQueryHandle = SteamUGC.CreateQueryUGCDetailsRequest(fileIDs.ToArray(), (uint) fileIDs.Count);
			SteamUGC.SetReturnKeyValueTags(serverItemsQueryHandle, true);
			SteamUGC.SetAllowCachedResponse(serverItemsQueryHandle, 60);
			SteamAPICall_t callHandle = SteamUGC.SendQueryUGCRequest(serverItemsQueryHandle);
			serverItemsQueryCompleted.Set(callHandle);
		}

		private void installItemDownloadedFromServer(PublishedFileId_t fileId, string path)
		{
			ESteamUGCType type;
			if (WorkshopTool.detectUGCMetaType(path, false, out type))
			{
				ugc.Add(new SteamContent(fileId, path, type));

				LoadFileIfAssetStartupAlreadyRan(fileId, path, type);
			}
			else
			{
				UnturnedLog.warn("Unable to determine UGC type for downloaded item: " + fileId);
			}
		}

#pragma warning disable
		private Callback<DownloadItemResult_t> itemDownloaded;
#pragma warning restore
		private void onItemDownloaded(DownloadItemResult_t callback)
		{
			if (installing == null || installing.Count == 0)
			{
				return;
			}

			if (callback.m_unAppID.m_AppId != appInfo.id)
			{
				// Not for this game!
				return;
			}

			UnturnedLog.info("Workshop item downloaded: " + callback.m_nPublishedFileId);

			if (callback.m_nPublishedFileId == currentlyDownloadingFileId)
			{
				currentlyDownloadingFileId = PublishedFileId_t.Invalid;
				currentlyDownloadingFileEstimatedProgress = 0.0f;
			}

			UnityEngine.Profiling.Profiler.BeginSample("onItemDownloaded");
			installing.Remove(callback.m_nPublishedFileId);

			UpdateEstimatedDownloadProgress();

			if (callback.m_eResult == EResult.k_EResultOK)
			{
				if (isInstalledItemAlreadyRegistered(callback.m_nPublishedFileId))
				{
					UnturnedLog.warn("Already registered newly downloaded workshop item '{0}', so ignoring this callback", callback.m_nPublishedFileId);
				}
				else
				{
					string ignoreExplanation;
					if (shouldIgnoreFile(callback.m_nPublishedFileId, out ignoreExplanation))
					{
						UnturnedLog.info("Ignoring newly downloaded workshop item {0} because '{1}'", callback.m_nPublishedFileId, ignoreExplanation);
					}
					else
					{
						ulong size;
						string path;
						uint timestamp;

						if (SteamUGC.GetItemInstallInfo(callback.m_nPublishedFileId, out size, out path, 1024, out timestamp))
						{
							if (ReadWrite.folderExists(path, false))
							{
								installItemDownloadedFromServer(callback.m_nPublishedFileId, path);
							}
							else
							{
								UnturnedLog.warn("Finished downloading workshop item {0}, but unable to find the files on disk ({1})", callback.m_nPublishedFileId, path);
							}
						}
						else
						{
							UnturnedLog.warn("Finished downloading workshop item {0}, but unable get install info", callback.m_nPublishedFileId);
						}
					}
				}
			}
			else
			{
				UnturnedLog.warn("Download workshop item {0} failed, result: {1}", callback.m_nPublishedFileId, callback.m_eResult);
			}

			downloadNextItem();
			UnityEngine.Profiling.Profiler.EndSample();
		}

		/// <summary>
		/// Callback when player subscribes to an item and it finishes downloading.
		/// Different than the game-managed DownloadItem calls.
		/// </summary>
#pragma warning disable
		private Callback<ItemInstalled_t> itemInstalled;
#pragma warning restore
		private void onItemInstalled(ItemInstalled_t callback)
		{
			if (callback.m_unAppID.m_AppId != appInfo.id)
			{
				// Not for this game!
				return;
			}

			UnturnedLog.info("Workshop item installed: " + callback.m_nPublishedFileId);

			if (isInstalledItemAlreadyRegistered(callback.m_nPublishedFileId))
			{
				UnturnedLog.warn("Already registered newly installed workshop item '{0}', so ignoring this callback", callback.m_nPublishedFileId);
				return;
			}

			string ignoreExplanation;
			if (shouldIgnoreFile(callback.m_nPublishedFileId, out ignoreExplanation))
			{
				UnturnedLog.info("Ignoring newly installed workshop item because '{0}'", ignoreExplanation);
				return;
			}

			string path;
			if (getInstalledItemPath(callback.m_nPublishedFileId, out path) == false)
			{
				UnturnedLog.warn("Unable to determine newly installed workshop item '{0}' file path", callback.m_nPublishedFileId);
				return;
			}

			ESteamUGCType type;
			if (WorkshopTool.detectUGCMetaType(path, false, out type) == false)
			{
				UnturnedLog.warn("Unable to determine newly installed workshop item '{0}' type", callback.m_nPublishedFileId);
				return;
			}

			ugc.Add(new SteamContent(callback.m_nPublishedFileId, path, type));

			LoadFileIfAssetStartupAlreadyRan(callback.m_nPublishedFileId, path, type);
		}

		private void LoadFileIfAssetStartupAlreadyRan(PublishedFileId_t fileId, string path, ESteamUGCType type)
		{
			// Should assets be loaded now?
			// We defer loading if the initial loading step has not run yet.
			bool shouldLoadNow = type == ESteamUGCType.MAP ? Assets.hasLoadedMaps : Assets.hasLoadedUgc;
			if (shouldLoadNow == false)
			{
				UnturnedLog.info($"Workshop file {fileId} not requesting load because asset refresh is in progress");
				return;
			}

			switch (type)
			{
				case ESteamUGCType.MAP:
					WorkshopTool.loadMapBundlesAndContent(path, fileId.m_PublishedFileId);

					// Refreshes the levels list in-case players
					// were on the main menu browsing the workshop externally or from a featured article.
					Level.broadcastLevelsRefreshed();
					break;

				case ESteamUGCType.LOCALIZATION:
					break;

				default:
					Assets.RequestAddSearchLocation(path, FindOrAddOrigin(fileId.m_PublishedFileId));
					break;
			}
		}

		private void cleanupUGCRequest()
		{
			if (ugcRequest == UGCQueryHandle_t.Invalid)
			{
				return;
			}

			SteamUGC.ReleaseQueryUGCRequest(ugcRequest);
			ugcRequest = UGCQueryHandle_t.Invalid;
		}

		public void prepareUGC(string name, string description, string path, string preview, string change, ESteamUGCType type, List<string> tags, string allowedIPs, ESteamUGCVisibility visibility)
		{
			bool verified = File.Exists(path + "/Skin.kvt");
			prepareUGC(name, description, path, preview, change, type, tags, allowedIPs, visibility, verified);
		}

		public void prepareUGC(string name, string description, string path, string preview, string change, ESteamUGCType type, List<string> tags, string allowedIPs, ESteamUGCVisibility visibility, bool verified)
		{
			ugcName = name;
			ugcDescription = description;
			ugcPath = path;
			ugcPreview = preview;
			ugcChange = change;
			ugcType = type;
			ugcTags = tags;
			ugcAllowedIPs = allowedIPs;
			ugcVisibility = visibility;
			ugcVerified = verified;
		}

		public void prepareUGC(PublishedFileId_t id)
		{
			publishedFileID = id;
		}

		public void createUGC(bool ugcFor)
		{
			SteamAPICall_t handle = SteamUGC.CreateItem(SteamUtils.GetAppID(), ugcFor ? EWorkshopFileType.k_EWorkshopFileTypeMicrotransaction : EWorkshopFileType.k_EWorkshopFileTypeCommunity);
			createItemResult.Set(handle);
		}

		public void updateUGC()
		{
			UGCUpdateHandle_t update = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), publishedFileID);

			if (ugcType == ESteamUGCType.MAP)
			{
				ReadWrite.writeBytes(ugcPath + "/Map.meta", false, false, new byte[1]);
			}
			else if (ugcType == ESteamUGCType.LOCALIZATION)
			{
				ReadWrite.writeBytes(ugcPath + "/Localization.meta", false, false, new byte[1]);
			}
			else if (ugcType == ESteamUGCType.OBJECT)
			{
				ReadWrite.writeBytes(ugcPath + "/Object.meta", false, false, new byte[1]);
			}
			else if (ugcType == ESteamUGCType.ITEM)
			{
				ReadWrite.writeBytes(ugcPath + "/Item.meta", false, false, new byte[1]);
			}
			else if (ugcType == ESteamUGCType.VEHICLE)
			{
				ReadWrite.writeBytes(ugcPath + "/Vehicle.meta", false, false, new byte[1]);
			}
			else if (ugcType == ESteamUGCType.SKIN)
			{
				ReadWrite.writeBytes(ugcPath + "/Skin.meta", false, false, new byte[1]);
			}

			SteamUGC.SetItemContent(update, ugcPath);

			if (ugcDescription.Length > 0)
			{
				SteamUGC.SetItemDescription(update, ugcDescription);
			}

			if (ugcPreview.Length > 0)
			{
				SteamUGC.SetItemPreview(update, ugcPreview);
			}

			List<string> tags = new List<string>();

			if (ugcTags != null)
			{
				foreach (string tag in ugcTags)
				{
					if (!string.IsNullOrWhiteSpace(tag))
					{
						tags.Add(tag);
					}
				}
			}

			if (ugcVerified)
			{
				tags.Add("Verified");
			}

			SteamUGC.SetItemTags(update, tags.ToArray());

			if (ugcName.Length > 0)
			{
				SteamUGC.SetItemTitle(update, ugcName);
			}

			SteamUGC.RemoveItemKeyValueTags(update, WorkshopDownloadRestrictions.IP_RESTRICTIONS_KVTAG);
			if (!string.IsNullOrEmpty(ugcAllowedIPs))
			{
				SteamUGC.AddItemKeyValueTag(update, WorkshopDownloadRestrictions.IP_RESTRICTIONS_KVTAG, ugcAllowedIPs);
			}

			SteamUGC.RemoveItemKeyValueTags(update, COMPATIBILITY_VERSION_KVTAG);
			SteamUGC.AddItemKeyValueTag(update, COMPATIBILITY_VERSION_KVTAG, AssetBundleVersion.NEWEST.ToString());

			if (ugcVisibility == ESteamUGCVisibility.PUBLIC)
			{
				SteamUGC.SetItemVisibility(update, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
			}
			else if (ugcVisibility == ESteamUGCVisibility.FRIENDS_ONLY)
			{
				SteamUGC.SetItemVisibility(update, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly);
			}
			else if (ugcVisibility == ESteamUGCVisibility.PRIVATE)
			{
				SteamUGC.SetItemVisibility(update, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
			}
			else if (ugcVisibility == ESteamUGCVisibility.UNLISTED)
			{
				SteamUGC.SetItemVisibility(update, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted);
			}

			SteamAPICall_t submit = SteamUGC.SubmitItemUpdate(update, ugcChange);
			submitItemUpdateResult.Set(submit);
		}

		private bool isInstalledItemAlreadyRegistered(PublishedFileId_t fileId)
		{
			foreach (SteamContent item in ugc)
			{
				if (item.publishedFileID == fileId)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get path to an already-installed workshop item.
		/// </summary>
		/// <returns>True if the path was found.</returns>
		private bool getInstalledItemPath(PublishedFileId_t fileId, out string path)
		{
			// Note that this check seems important for GetItemInstallInfo to work properly while in offline mode.
			EItemState stateFlags = (EItemState) SteamUGC.GetItemState(fileId);
			if ((stateFlags & EItemState.k_EItemStateInstalled) != EItemState.k_EItemStateInstalled)
			{
				UnturnedLog.warn("Installed item {0} state flags missing k_EItemStateInstalled: {1}", fileId, stateFlags);
			}

			ulong size;
			uint timestamp;
			return SteamUGC.GetItemInstallInfo(fileId, out size, out path, 1024, out timestamp) && ReadWrite.folderExists(path, false);
		}

		/// <summary>
		/// Used during startup to register subscribed workshop items.
		/// Given a workshop item file id, if its files exist on disk then register it.
		/// </summary>
		private void registerInstalledItem(PublishedFileId_t fileId)
		{
			if (isInstalledItemAlreadyRegistered(fileId))
			{
				// May have been registered as part of localization init.
				return;
			}

			string ignoreExplanation;
			if (shouldIgnoreFile(fileId, out ignoreExplanation))
			{
				UnturnedLog.info("Ignoring subscribed item {0} because '{1}'", fileId, ignoreExplanation);
				return;
			}

			string path;
			if (!getInstalledItemPath(fileId, out path))
			{
				UnturnedLog.warn("Unable to register installed item during startup: {0}\nPath:{1}", fileId, path);
				return;
			}

			ESteamUGCType type;
			if (!WorkshopTool.detectUGCMetaType(path, false, out type))
			{
				UnturnedLog.warn("Unable to determine UGC type for installed item: " + fileId);
				return;
			}

			string warningMessage;
			if (!isCompatible(fileId, type, path, out warningMessage))
			{
				Assets.reportError(warningMessage);
				return;
			}

			ugc.Add(new SteamContent(fileId, path, type));

			if (!LocalWorkshopSettings.get().getEnabled(fileId))
				return;

			LoadFileIfAssetStartupAlreadyRan(fileId, path, type);
		}

		/// <summary>
		/// Workshop file ids we were locally subscribed to during startup.
		/// These items are queried for compatibility before registering.
		/// </summary>
		private PublishedFileId_t[] locallySubscribedFileIds = null;

		/// <summary>
		/// Called when subscribed items callback was successful to register all compatible files.
		/// </summary>
		private void handleSubscribedItemsCallbackSuccess(SteamUGCQueryCompleted_t callback)
		{
			UnturnedLog.info($"Received details for {callback.m_unNumResultsReturned} subscribed workshop file(s)");
			for (uint resultIndex = 0; resultIndex < callback.m_unNumResultsReturned; resultIndex++)
			{
				CachedUGCDetails cachedDetails;
				if (cacheDetails(callback.m_handle, resultIndex, out cachedDetails))
				{
					UnturnedLog.info($"Subscribed workshop file {resultIndex + 1} of {callback.m_unNumResultsReturned}: \"{cachedDetails.name}\" ({cachedDetails.fileId})");
					registerInstalledItem(cachedDetails.fileId);
				}
			}
		}

		/// <summary>
		/// Called when subscribed items callback did not execute as expected,
		/// maybe because steam's servers are offline. In this case we can't check
		/// compatibility so we register all the locally subscribed items as compatible.
		/// </summary>
		private void handleSubscribedItemsCallbackFailed()
		{
			UnturnedLog.info("Registering {0} locally subscribed item(s)", locallySubscribedFileIds.Length);
			foreach (PublishedFileId_t fileId in locallySubscribedFileIds)
			{
				registerInstalledItem(fileId);
			}
		}

		/// <summary>
		/// Register any localization-type workshop content before waiting for the steam callbacks.
		/// Important so that localizations are available for loading screens and whatnot during startup.
		/// Any items we register now will be skipped later.
		/// </summary>
		private void registerLocalizations()
		{
			foreach (PublishedFileId_t localItem in locallySubscribedFileIds)
			{
				string path;
				if (!getInstalledItemPath(localItem, out path))
					continue;

				ESteamUGCType type;
				if (!WorkshopTool.detectUGCMetaType(path, false, out type))
					continue;

				if (type == ESteamUGCType.LOCALIZATION)
				{
					registerInstalledItem(localItem);
				}
			}
		}

		private UGCQueryHandle_t subscribedQueryHandle;
#pragma warning disable
		private CallResult<SteamUGCQueryCompleted_t> subscribedQueryCompleted;
#pragma warning restore
		private void onSubscribedQueryCompleted(SteamUGCQueryCompleted_t callback, bool ioFailure)
		{
			if (callback.m_handle != subscribedQueryHandle)
			{
				// Not for us!
				return;
			}

			if (!ioFailure)
			{
				if (callback.m_eResult == EResult.k_EResultOK)
				{
					handleSubscribedItemsCallbackSuccess(callback);
				}
				else
				{
					UnturnedLog.error("Encountered an error when querying workshop for subscribed items: " + callback.m_eResult);
					handleSubscribedItemsCallbackFailed();
				}
			}
			else
			{
				UnturnedLog.error("Encountered an IO error when querying workshop for subscribed items!");
				handleSubscribedItemsCallbackFailed();
			}

			SteamUGC.ReleaseQueryUGCRequest(subscribedQueryHandle);
			subscribedQueryHandle = UGCQueryHandle_t.Invalid;
		}

		/// <summary>
		/// If specified, player's workshop file subscriptions are not registered at startup.
		/// </summary>
		private static CommandLineFlag shouldIgnoreSubscribedItems = new CommandLineFlag(false, "-NoWorkshopSubscriptions");

		public void refreshUGC()
		{
			_ugc = new List<SteamContent>();
			uint content = SteamUGC.GetNumSubscribedItems();
			if (content < 1)
			{
				UnturnedLog.info("Found zero workshop file subscriptions");
				return;
			}

			if (shouldIgnoreSubscribedItems)
			{
				UnturnedLog.info("Ignoring all workshop file subscriptions");
				return;
			}

			locallySubscribedFileIds = new PublishedFileId_t[content];
			SteamUGC.GetSubscribedItems(locallySubscribedFileIds, content);

			UnturnedLog.info($"Subscribed workshop file ID(s): {string.Join(", ", locallySubscribedFileIds)}");

			registerLocalizations();

			UnturnedLog.info("Querying details for subscribed workshop files...");
			subscribedQueryHandle = SteamUGC.CreateQueryUGCDetailsRequest(locallySubscribedFileIds, content);
			SteamUGC.SetReturnKeyValueTags(subscribedQueryHandle, true);
			SteamAPICall_t callHandle = SteamUGC.SendQueryUGCRequest(subscribedQueryHandle);
			subscribedQueryCompleted.Set(callHandle);
		}

		public void refreshPublished()
		{
			onPublishedRemoved?.Invoke();

			cleanupUGCRequest();

			_published = new List<SteamPublished>();

			ugcRequestPage = 1;
			shouldRequestAnotherPage = false;
			ugcRequest = SteamUGC.CreateQueryUserUGCRequest(SDG.Unturned.Provider.client.GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, SteamUtils.GetAppID(), SteamUtils.GetAppID(), ugcRequestPage);
			SteamAPICall_t handle = SteamUGC.SendQueryUGCRequest(ugcRequest);
			queryCompleted.Set(handle);
		}

		/// <summary>
		/// Map of subscriptions added/removed by the player through the in-game client API, as opposed to the web browser.
		/// </summary>
		private Dictionary<PublishedFileId_t, bool> ingameSubscriptions = new Dictionary<PublishedFileId_t, bool>();

		public bool getSubscribed(ulong fileId)
		{
			if (ugc == null)
				return false;

			PublishedFileId_t steamFileId = new PublishedFileId_t(fileId);

			bool subscribed;
			if (ingameSubscriptions.TryGetValue(steamFileId, out subscribed))
			{
				return subscribed;
			}

			foreach (SteamContent foundItem in ugc)
			{
				if (foundItem.publishedFileID == steamFileId)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Called by us when we subscribe to an item from in-game.
		/// If item already exists on-disk steam doesn't always call onItemInstalled, so we do our own check and potentially load.
		/// </summary>
		private void gameSubscribed(PublishedFileId_t fileId)
		{
			if (isInstalledItemAlreadyRegistered(fileId))
				return; // Already loaded.

			EItemState stateFlags = (EItemState) SteamUGC.GetItemState(fileId);

			if ((stateFlags & EItemState.k_EItemStateInstalled) != EItemState.k_EItemStateInstalled)
				return; // Item is not installed, so we should get the normal installed callback.

			if ((stateFlags & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading)
				return; // Downloading an update, so presumably Steam will give us the callback.

			if ((stateFlags & EItemState.k_EItemStateDownloadPending) == EItemState.k_EItemStateDownloading)
				return; // DownloadItem was called, so we will wait for the download item callback.

			UnturnedLog.info("Triggering a fake onItemInstalled callback for {0} because game subscribed to a pre-installed item", fileId);

			ItemInstalled_t callback;
			callback.m_unAppID.m_AppId = appInfo.id;
			callback.m_nPublishedFileId = fileId;
			onItemInstalled(callback);
		}

		public void setSubscribed(ulong fileId, bool subscribe)
		{
			PublishedFileId_t steamFileId = new PublishedFileId_t(fileId);

			if (subscribe)
			{
				SteamUGC.SubscribeItem(steamFileId);
				UnturnedLog.info("Game subscribed to " + fileId);
				gameSubscribed(steamFileId);
			}
			else
			{
				SteamUGC.UnsubscribeItem(steamFileId);
				UnturnedLog.info("Game un-subscribed from " + fileId);
			}

			ingameSubscriptions[steamFileId] = subscribe;
		}

		public TempSteamworksWorkshop(SDG.SteamworksProvider.SteamworksAppInfo newAppInfo)
		{
			appInfo = newAppInfo;

			downloaded = new List<PublishedFileId_t>();

			if (!appInfo.isDedicated)
			{
				createItemResult = CallResult<CreateItemResult_t>.Create(onCreateItemResult);
				submitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(onSubmitItemUpdateResult);
				queryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onQueryCompleted);
				itemDownloaded = Callback<DownloadItemResult_t>.Create(onItemDownloaded);
				itemInstalled = Callback<ItemInstalled_t>.Create(onItemInstalled);
				subscribedQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onSubscribedQueryCompleted);
				serverItemsQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onServerItemsQueryCompleted);
			}
		}
	}
}
