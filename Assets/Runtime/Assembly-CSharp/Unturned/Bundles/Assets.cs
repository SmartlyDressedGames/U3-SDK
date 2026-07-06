////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define WITH_ASSETS_PROFILING
#define LOG_ASSET_REDIRECTORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.Framework.Devkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using Unturned.SystemEx;
using Unturned.UnityEx;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SDG.Unturned
{
	public delegate void AssetsRefreshed();

	/// <summary>
	/// Used to aid backwards compatibility as much as possible when transitioning Unity versions breaks asset bundles.
	/// </summary>
	public static class AssetBundleVersion
	{
		/// <summary>
		/// Unity 5.5 and earlier per-asset .unity3d file.
		/// </summary>
		public const int UNITY_5 = 1;

		/// <summary>
		/// When "master bundles" were first introduced in order to convert older Unity 5.5 asset bundles in bulk.
		/// </summary>
		public const int UNITY_2017_LTS = 2;

		/// <summary>
		/// Unity 2018 needed a new version number in order to convert materials from 2017 LTS asset bundles. 2019 did not need a
		/// new version number, but in retrospect it seems unfortunate that we cannot distinguish them, so 2020 does have its own.
		/// </summary>
		public const int UNITY_2018_AND_2019_LTS = 3;

		public const int UNITY_2020_LTS = 4;
		public const int UNITY_2021_LTS = 5;

		/// <summary>
		/// 2022 LTS+
		/// </summary>
		public const int NEWEST = 6;
	}

	internal class AssetLoadingStats
	{
		public int RegisteredSearchLocations => totalRegisteredSearchLocations - baselineRegisteredSearchLocations;
		public int SearchLocationsFinishedSearching => totalSearchLocationsFinishedSearching - baselineSearchLocationsFinishedSearching;
		public int AssetBundlesFound => totalMasterBundlesFound - baselineMasterBundlesFound;
		public int AssetBundlesLoaded => totalMasterBundlesLoaded - baselineMasterBundlesLoaded;
		public int FilesFound => totalFilesFound - baselineFilesFound;
		public int FilesRead => totalFilesRead - baselineFilesRead;
		public int FilesLoaded => totalFilesLoaded - baselineFilesLoaded;

		public bool isLoadingAssetBundles;
		public int totalRegisteredSearchLocations;
		public int totalSearchLocationsFinishedSearching;
		public int totalMasterBundlesFound;
		public int totalMasterBundlesLoaded;
		public int totalFilesFound;
		public int totalFilesRead;
		public int totalFilesLoaded;

		public float EstimateAssetBundleProgressPercentage()
		{
			float progressPerLocation = RegisteredSearchLocations > 1 ? (1.0f / RegisteredSearchLocations) : 1.0f;
			float maxProgress = SearchLocationsFinishedSearching > 0 ? SearchLocationsFinishedSearching * progressPerLocation : progressPerLocation;
			float assetBundleProgress = AssetBundlesFound > 1 ? (AssetBundlesLoaded / (float) AssetBundlesFound) : 0.0f;
			return Mathf.Clamp01(assetBundleProgress * maxProgress);
		}

		public float EstimateSearchProgressPercentage()
		{
			return RegisteredSearchLocations > 0 ? (SearchLocationsFinishedSearching / (float) RegisteredSearchLocations) : 0.0f;
		}

		public float EstimateReadProgressPercentage()
		{
			float progressPerLocation = RegisteredSearchLocations > 1 ? (1.0f / RegisteredSearchLocations) : 1.0f;
			float maxProgress = SearchLocationsFinishedSearching > 0 ? SearchLocationsFinishedSearching * progressPerLocation : progressPerLocation;
			float fileProgress = FilesFound > 1 ? (FilesRead / (float) FilesFound) : 0.0f;
			return Mathf.Clamp01(fileProgress * maxProgress);
		}

		public float EstimateFileProgressPercentage()
		{
			float progressPerLocation = RegisteredSearchLocations > 1 ? (1.0f / RegisteredSearchLocations) : 1.0f;
			float maxProgress = SearchLocationsFinishedSearching > 0 ? SearchLocationsFinishedSearching * progressPerLocation : progressPerLocation;
			float fileProgress = FilesFound > 1 ? (FilesLoaded / (float) FilesFound) : 0.0f;
			return Mathf.Clamp01(fileProgress * maxProgress);
		}

		public void Reset()
		{
			baselineRegisteredSearchLocations = totalRegisteredSearchLocations;
			baselineSearchLocationsFinishedSearching = totalSearchLocationsFinishedSearching;
			baselineMasterBundlesFound = totalMasterBundlesFound;
			baselineMasterBundlesLoaded = totalMasterBundlesLoaded;
			baselineFilesFound = totalFilesFound;
			baselineFilesRead = totalFilesRead;
			baselineFilesLoaded = totalFilesLoaded;
		}

		private int baselineRegisteredSearchLocations;
		private int baselineSearchLocationsFinishedSearching;
		private int baselineMasterBundlesFound;
		private int baselineMasterBundlesLoaded;
		private int baselineFilesFound;
		private int baselineFilesRead;
		private int baselineFilesLoaded;
	}

	public class Assets : MonoBehaviour
	{
		private static TypeRegistryDictionary _assetTypes = new TypeRegistryDictionary(typeof(Asset));
		public static TypeRegistryDictionary assetTypes => _assetTypes;

		private static TypeRegistryDictionary _useableTypes = new TypeRegistryDictionary(typeof(Useable));
		public static TypeRegistryDictionary useableTypes => _useableTypes;

		private static Assets instance;

		/// <summary>
		/// The first time asset loading finishes it will load the main menu.
		/// </summary>
		private static bool hasFinishedInitialStartupLoading;

		/// <summary>
		/// If true, either loading during initial startup or full refresh.
		/// </summary>
		private static bool isLoadingAllAssets;

		/// <summary>
		/// If true, currently searching locations added after initial startup loading.
		/// </summary>
		private static bool isLoadingFromUpdate;

		/// <summary>
		/// Has initial client UGC loading step run yet?
		/// Used to defer asset loading for workshop installs that occured during startup.
		/// </summary>
		public static bool hasLoadedUgc
		{
			get;
			protected set;
		}

		/// <summary>
		/// Has initial map loading step run yet?
		/// Used to defer map loading for workshop installs that occured during startup.
		/// </summary>
		public static bool hasLoadedMaps
		{
			get;
			protected set;
		}

		public static bool isLoading => (isLoadingAllAssets || isLoadingFromUpdate);

		internal static bool ShouldWaitForNewAssetsToFinishLoading => isLoading || instance.worker.IsWorking;

		public static AssetsRefreshed onAssetsRefreshed;
		internal static System.Action OnNewAssetsFinishedLoading;

		internal class AssetMapping
		{
			/// <summary>
			/// Calling this "legacy" is a bit of a stretch because even most of the vanilla assets are
			/// built around the 16-bit IDs. Ideally no new code should be relying on 16-bit IDs however.
			/// </summary>
			public Dictionary<EAssetType, Dictionary<ushort, Asset>> legacyAssetsTable;
			public Dictionary<Guid, Asset> assetDictionary;
			public List<Asset> assetList;

			/// <summary>
			/// Incremented when assets are added or removed.
			/// Used by boombox UI to only refresh songs list if assets have changed.
			/// </summary>
			public int modificationCounter;

			public AssetMapping()
			{
				legacyAssetsTable = new Dictionary<EAssetType, Dictionary<ushort, Asset>>();
				legacyAssetsTable.Add(EAssetType.ITEM, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.EFFECT, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.OBJECT, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.RESOURCE, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.VEHICLE, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.ANIMAL, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.MYTHIC, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.SKIN, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.SPAWN, new Dictionary<ushort, Asset>());
				legacyAssetsTable.Add(EAssetType.NPC, new Dictionary<ushort, Asset>());

				assetDictionary = new Dictionary<Guid, Asset>();
				assetList = new List<Asset>();
				modificationCounter = 0;
			}
		}

		internal static AssetMapping defaultAssetMapping;

		/// <summary>
		/// In singleplayer and the level editor this is the same as defaultAssetMapping,
		/// but when playing on a server a subset of assets based on the server's workshop files is used.
		/// </summary>
		private static AssetMapping currentAssetMapping;

		/// <summary>
		/// Should folders be scanned for and load .dat and asset bundle files?
		/// Plugin developers find it useful to quickly launch the server.
		/// </summary>
		public static CommandLineFlag shouldLoadAnyAssets = new CommandLineFlag(true, "-SkipAssets");

		/// <summary>
		/// Do we want to enable shouldDeferLoadingAssets?
		/// </summary>
		public static CommandLineFlag wantsDeferLoadingAssets = new CommandLineFlag(true, "-NoDeferAssets");

		/// <summary>
		/// Should extra validation be performed on assets as they load?
		/// Useful for developing, but it does slow down loading.
		/// </summary>
		public static CommandLineFlag shouldValidateAssets = new CommandLineFlag(false, "-ValidateAssets");

		/// <summary>
		/// Should asset file metadata such as line numbers and comments be parsed?
		/// Useful for development (e.g., error messages), but may slow down loading and increases RAM usage.
		/// </summary>
		public static CommandLineFlag shouldParseMetadata = new CommandLineFlag(false, "-ParseAssetMetadata");

		/// <summary>
		/// Should asset files be re-saved after all loading is finished?
		/// Requires asset metadata. Useful for automatically upgrading .dat/.asset files.
		/// </summary>
		public static CommandLineFlag shouldResaveAssets = new CommandLineFlag(false, "-ResaveAssets");

		/// <summary>
		/// Should some specific asset types which opt-in be allowed to defer loading from asset bundles until used?
		/// Disabled by asset validation because all assets need to be loaded.
		/// </summary>
		public static bool shouldDeferLoadingAssets => wantsDeferLoadingAssets && shouldValidateAssets == false;

		/// <summary>
		/// Should workshop asset names and IDs be logged while loading?
		/// Useful when debugging unknown workshop content.
		/// </summary>
		public static CommandLineFlag shouldLogWorkshopAssets = new CommandLineFlag(false, "-LogWorkshopAssets");

#if UNITY_EDITOR
		/// <summary>
		/// Should a JSON report of all the game's assets be exported?
		/// </summary>
		public static CommandLineFlag exportAssetsReport = new CommandLineFlag(false, "-ExportAssetsReport");
#endif // UNITY_EDITOR

		/// <summary>
		/// Should GC and clear unused assets be called after every loading frame?
		/// Potentially useful for players running out of RAM, refer to:
		/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/1352#issuecomment-751138105
		/// </summary>
		private static CommandLineFlag shouldCollectGarbageAggressively = new CommandLineFlag(false, "-AggressiveGC");

		/// <summary>
		/// Should modded spawn tables being inserted into parents be logged?
		/// Useful for debugging workshop spawn table problems.
		/// </summary>
		private static CommandLineFlag shouldLogSpawnInsertions = new CommandLineFlag(false, "-LogSpawnInsertions");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal static CommandLineFlag shouldLoadCoreAssetBundleFromSteamInstall = new CommandLineFlag(false, "-LoadCoreAssetBundleFromSteamInstall");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		/// <summary>
		/// Loaded master bundles.
		/// </summary>
		private static List<MasterBundleConfig> allMasterBundles;

		/// <summary>
		/// Loading master bundles.
		/// </summary>
		private static List<MasterBundleConfig> pendingMasterBundles;

		private static Queue<AssetsWorker.AssetDefinition> pendingAssetsToLoad;

		/// <summary>
		/// Master bundle from root /Bundles directory containing vanilla assets.
		/// </summary>
		internal static MasterBundleConfig coreMasterBundle
		{
			get;
			private set;
		}

		/// <summary>
		/// While an asset is being loaded, this is the master bundle for that asset.
		/// Used by master bundle pointer as a default.
		/// </summary>
		public static MasterBundleConfig currentMasterBundle
		{
			get;
			private set;
		}

		/// <summary>
		/// While an asset is being loaded, this is the asset.
		/// Used by some error logging.
		/// Note: not ideal because any global state like this prevents parallelization.
		/// </summary>
		internal static Asset currentAsset;

		internal static List<AssetOrigin> assetOrigins;
		internal static AssetOrigin coreOrigin;
		internal static AssetOrigin reloadOrigin;
		private static AssetOrigin legacyServerSharedOrigin;
		private static AssetOrigin legacyPerServerOrigin;

#if WITH_ASSET_CONSOLIDATION
		/// <summary>
		/// Absolute file path of the .dat file being loaded.
		/// </summary>
		public static string currentFilePath;
#endif // WITH_ASSET_CONSOLIDATION

		private static List<string> errors;

		private static string getExceptionMessage(Exception e)
		{
			if (e != null)
			{
				if (e.InnerException != null)
				{
					return e.InnerException.Message;
				}
				else
				{
					return e.Message;
				}
			}
			else
			{
				return "Exception = Null";
			}
		}

		public static void reportError(string error)
		{
			errors.Add(error);
			UnturnedLog.warn(error);
		}

		public static void ReportError(IAssetErrorContext context, string error)
		{
			if (context is Asset asset)
			{
				asset.HasErrors = true;
			}
			reportError($"{context.AssetErrorPrefix}: {error}");
		}

		public static void ReportError(IAssetErrorContext context, string format, params object[] args)
		{
			string error = string.Format(format, args);
			ReportError(context, error);
		}

		public static void ReportError(IAssetErrorContext context, string format, object arg0)
		{
			string error = string.Format(format, arg0);
			ReportError(context, error);
		}

		public static void ReportError(IAssetErrorContext context, string format, object arg0, object arg1)
		{
			string error = string.Format(format, arg0, arg1);
			ReportError(context, error);
		}

		public static void ReportError(IAssetErrorContext context, string format, object arg0, object arg1, object arg2)
		{
			string error = string.Format(format, arg0, arg1, arg2);
			ReportError(context, error);
		}

		public static List<string> getReportedErrorsList()
		{
			return errors;
		}

		internal static AssetOrigin FindWorkshopFileOrigin(ulong workshopFileId)
		{
			foreach (AssetOrigin origin in assetOrigins)
			{
				if (origin.workshopFileId == workshopFileId)
				{
					return origin;
				}
			}

			return null;
		}

		private static AssetOrigin FindLevelOrigin(LevelInfo level)
		{
			if (level.publishedFileId != 0)
			{
				return FindWorkshopFileOrigin(level.publishedFileId);
			}

			string expectedOriginName = $"Map \"{level.name}\"";

			// Directly in the Maps folder.
			foreach (AssetOrigin origin in assetOrigins)
			{
				if (string.Equals(origin.name, expectedOriginName))
				{
					return origin;
				}
			}

			return null;
		}

		internal static AssetOrigin FindOrAddWorkshopFileOrigin(ulong workshopFileId, bool shouldOverrideIds)
		{
			AssetOrigin existingOrigin = FindWorkshopFileOrigin(workshopFileId);
			if (existingOrigin != null)
			{
				return existingOrigin;
			}

			AssetOrigin newOrigin = new AssetOrigin();
			newOrigin.name = $"Workshop File ({workshopFileId})";
			newOrigin.workshopFileId = workshopFileId;
			newOrigin.shouldAssetsOverrideExistingIds = shouldOverrideIds;
			assetOrigins.Add(newOrigin);
			return newOrigin;
		}

		internal static AssetOrigin FindOrAddLevelOrigin(LevelInfo level)
		{
			if (level.publishedFileId != 0)
			{
				return FindOrAddWorkshopFileOrigin(level.publishedFileId, false);
			}

			string expectedOriginName = $"Map \"{level.name}\"";

			// Directly in the Maps folder.
			foreach (AssetOrigin origin in assetOrigins)
			{
				if (string.Equals(origin.name, expectedOriginName))
				{
					return origin;
				}
			}

			AssetOrigin newOrigin = new AssetOrigin();
			newOrigin.name = expectedOriginName;
			newOrigin.canResave = true; // Allow re-saving assets in the Maps folder.
			assetOrigins.Add(newOrigin);
			return newOrigin;
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		public static Asset find(EAssetType type, ushort id)
		{
			if (type == EAssetType.NONE || id == 0)
			{
				return null;
			}

			Asset resultAsset;
			currentAssetMapping.legacyAssetsTable[type].TryGetValue(id, out resultAsset);

			int redirectCount = 0;
			do
			{
				if (resultAsset is RedirectorAsset redirectorAsset)
				{
					currentAssetMapping.assetDictionary.TryGetValue(redirectorAsset.TargetGuid, out resultAsset);
#if LOG_ASSET_REDIRECTORS
					UnturnedLog.info($"Find asset by type {type} and legacy ID {id} hit redirector \"{redirectorAsset.FriendlyName}\", target: \"{resultAsset?.FriendlyName ?? "null"}\" ({redirectorAsset.TargetGuid:N})");
#endif
					++redirectCount;
					if (redirectCount > 32)
					{
						resultAsset = null;
						UnturnedLog.warn($"Infinite asset director loop encountered when resolving Type: {type} Legacy ID: {id}");
						break;
					}
				}
				else
				{
					break;
				}
			}
			while (true);

			return resultAsset;
		}

		/// <summary>
		/// Find an asset by GUID reference.
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		/// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
		public static T find<T>(AssetReference<T> reference) where T : Asset
		{
			if (!reference.isValid)
			{
				return null;
			}

			Asset result = find(reference.GUID);
			return result as T;
		}

		/// <summary>
		/// Find an asset by GUID reference.
		/// This method supports <see cref="RedirectorAsset"/>.
		/// Maybe considered a hack? Ignores the current per-server asset mapping.
		/// </summary>
		/// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
		public static T Find_UseDefaultAssetMapping<T>(AssetReference<T> reference) where T : Asset
		{
			return Find_UseDefaultAssetMapping(reference.GUID) as T;
		}

		/// <summary>
		/// Find an asset by GUID reference.
		/// This method supports <see cref="RedirectorAsset"/>.
		/// Maybe considered a hack? Ignores the current per-server asset mapping.
		/// </summary>
		/// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
		public static Asset Find_UseDefaultAssetMapping(System.Guid assetGuid)
		{
			Asset resultAsset;
			defaultAssetMapping.assetDictionary.TryGetValue(assetGuid, out resultAsset);

			int redirectCount = 0;
			do
			{
				if (resultAsset is RedirectorAsset redirectorAsset)
				{
					currentAssetMapping.assetDictionary.TryGetValue(redirectorAsset.TargetGuid, out resultAsset);
#if LOG_ASSET_REDIRECTORS
					UnturnedLog.info($"Find asset {assetGuid:N} hit redirector \"{redirectorAsset.FriendlyName}\", target: \"{resultAsset?.FriendlyName ?? "null"}\" ({redirectorAsset.TargetGuid:N})");
#endif
					++redirectCount;
					if (redirectCount > 32)
					{
						resultAsset = null;
						UnturnedLog.warn($"Infinite asset director loop encountered when resolving: {assetGuid:N}");
						break;
					}
				}
				else
				{
					break;
				}
			}
			while (true);

			return resultAsset;
		}

		/// <summary>
		/// Load content from an assetbundle.
		/// </summary>
		public static T load<T>(ContentReference<T> reference) where T : UnityEngine.Object
		{
			if (!reference.isValid)
			{
				return null;
			}

			// Migrating towards removing .content files.
			MasterBundleConfig config = findMasterBundleByName(reference.name);
			if (config != null && config.assetBundle != null)
			{
				string formattedPath = config.FormatAssetPathAndCache(reference.path);
				T asset = config.assetBundle.LoadAsset<T>(formattedPath);
				if (asset == null)
				{
					UnturnedLog.warn("Failed to load content reference '{0}' from master bundle '{1}' as {2}", formattedPath, reference.name, typeof(T).Name);
				}
				return asset;
			}

			return null;
		}

		/// <summary>
		/// Find an asset by GUID reference.
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		/// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
		public static Asset find(Guid GUID)
		{
			Asset resultAsset;
			currentAssetMapping.assetDictionary.TryGetValue(GUID, out resultAsset);

			int redirectCount = 0;
			do
			{
				if (resultAsset is RedirectorAsset redirectorAsset)
				{
					currentAssetMapping.assetDictionary.TryGetValue(redirectorAsset.TargetGuid, out resultAsset);
#if LOG_ASSET_REDIRECTORS
					UnturnedLog.info($"Find asset {GUID} hit redirector \"{redirectorAsset.FriendlyName}\", target: \"{resultAsset?.FriendlyName ?? "null"}\" ({redirectorAsset.TargetGuid:N})");
#endif
					++redirectCount;
					if (redirectCount > 32)
					{
						resultAsset = null;
						UnturnedLog.warn($"Infinite asset director loop encountered when resolving: {GUID}");
						break;
					}
				}
				else
				{
					break;
				}
			}
			while (true);

			return resultAsset;
		}

		/// <summary>
		/// Find an asset by GUID reference.
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		/// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
		public static T find<T>(Guid guid) where T : Asset
		{
			return find(guid) as T;
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		public static EffectAsset FindEffectAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
		{
			if (guid.IsEmpty())
			{
				return find(EAssetType.EFFECT, legacyId) as EffectAsset;
			}
			else
			{
				return find<EffectAsset>(guid);
			}
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		public static T FindNpcAssetByGuidOrLegacyId<T>(Guid guid, ushort legacyId) where T : Asset
		{
			if (guid.IsEmpty())
			{
				return find(EAssetType.NPC, legacyId) as T;
			}
			else
			{
				return find<T>(guid);
			}
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// Note: this method doesn't handle redirects by VehicleRedirectorAsset.
		/// </summary>
		public static VehicleAsset FindVehicleAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
		{
			if (guid.IsEmpty())
			{
				return find(EAssetType.VEHICLE, legacyId) as VehicleAsset;
			}
			else
			{
				return find<VehicleAsset>(guid);
			}
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// Note: this method doesn't handle redirects by VehicleRedirectorAsset.
		/// </summary>
		public static Asset FindBaseVehicleAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
		{
			if (guid.IsEmpty())
			{
				return find(EAssetType.VEHICLE, legacyId);
			}
			else
			{
				return find(guid);
			}
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		public static SpawnAsset FindSpawnAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
		{
			if (guid.IsEmpty())
			{
				return find(EAssetType.SPAWN, legacyId) as SpawnAsset;
			}
			else
			{
				return find<SpawnAsset>(guid);
			}
		}

		/// <summary>
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		internal static T FindItemByGuidOrLegacyId<T>(Guid guid, ushort legacyId) where T : ItemAsset
		{
			if (guid.IsEmpty())
			{
				return find(EAssetType.ITEM, legacyId) as T;
			}
			else
			{
				return find<T>(guid);
			}
		}

		/// <summary>
		/// Useful if GUID can reference a different asset type than legacy ID. For example, gun magazine GUID can
		/// reference a SpawnAsset while its legacy ID cannot.
		/// This method supports <see cref="RedirectorAsset"/>.
		/// </summary>
		internal static Asset FindByGuidOrLegacyId(Guid guid, EAssetType legacyAssetType, ushort legacyId)
		{
			if (guid.IsEmpty())
			{
				return find(legacyAssetType, legacyId);
			}
			else
			{
				return find(guid);
			}
		}

		/// <summary>
		/// Append assets that extend from result type.
		/// </summary>
		public static void find<T>(List<T> results) where T : class
		{
			FindAssetsInListByType(currentAssetMapping.assetList, results);
		}

		internal static bool HasCurrentAssetMappingChanged(ref int counter)
		{
			bool changed = currentAssetMapping.modificationCounter != counter;
			counter = currentAssetMapping.modificationCounter;
			return changed;
		}

		internal static bool HasDefaultAssetMappingChanged(ref int counter)
		{
			bool changed = defaultAssetMapping.modificationCounter != counter;
			counter = defaultAssetMapping.modificationCounter;
			return changed;
		}

		/// <summary>
		/// Maybe considered a hack? Ignores the current per-server asset mapping.
		/// Append assets that extend from result type.
		/// </summary>
		internal static void FindAssetsByType_UseDefaultAssetMapping<T>(List<T> results) where T : class
		{
			FindAssetsInListByType(defaultAssetMapping.assetList, results);
		}

		private static void FindAssetsInListByType<T>(List<Asset> assetList, List<T> results) where T : class
		{
			foreach (Asset asset in assetList)
			{
				T result = asset as T;
				if (result != null)
				{
					results.Add(result);
				}
			}
		}

		public static Asset findByAbsolutePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;

			path = Path.GetFullPath(path);
			foreach (Asset testAsset in currentAssetMapping.assetList)
			{
				if (path.Equals(testAsset.absoluteOriginFilePath))
				{
					return testAsset;
				}
			}

			return null;
		}

		internal static Asset CreateAtRuntime(Type type, ushort legacyId)
		{
			try
			{
				Asset createdInstance = Activator.CreateInstance(type) as Asset;

				if (createdInstance != null)
				{
					createdInstance.GUID = Guid.NewGuid();
					createdInstance.id = legacyId;

					AddToMapping(createdInstance, false, defaultAssetMapping);

					if (createdInstance is IDirtyable)
					{
						(createdInstance as IDirtyable).isDirty = true;
					}

					createdInstance.OnCreatedAtRuntime();

					return createdInstance;
				}
			}
			catch (Exception exception)
			{
				UnturnedLog.exception(exception);
			}

			return null;
		}

		internal static T CreateAtRuntime<T>(ushort legacyId) where T : Asset
		{
			return CreateAtRuntime(typeof(T), legacyId) as T;
		}

		internal static void AddToMapping(Asset asset, bool overrideExistingID, AssetMapping assetMapping)
		{
			if (asset == null)
			{
				return;
			}

			EAssetType type = asset.assetCategory;

			if (type == EAssetType.SPAWN)
			{
				hasUnlinkedSpawns = true;
			}

			bool replacedAnything = false;

			if (type == EAssetType.OBJECT)
			{
				if (overrideExistingID)
				{
					Asset existingAsset;
					if (assetMapping.legacyAssetsTable[type].TryGetValue(asset.id, out existingAsset))
					{
						assetMapping.legacyAssetsTable[type].Remove(asset.id);
						existingAsset.hasBeenReplaced = true;
						replacedAnything = true;
					}

					assetMapping.legacyAssetsTable[type].Add(asset.id, asset);
				}
				else
				{
					if (!assetMapping.legacyAssetsTable[type].ContainsKey(asset.id))
					{
						assetMapping.legacyAssetsTable[type].Add(asset.id, asset);
					}
				}
			}
			else if (type != EAssetType.NONE)
			{
				if (asset.id != 0)
				{
					if (overrideExistingID)
					{
						Asset existingAsset;
						if (assetMapping.legacyAssetsTable[type].TryGetValue(asset.id, out existingAsset))
						{
							assetMapping.legacyAssetsTable[type].Remove(asset.id);
							existingAsset.hasBeenReplaced = true;
							replacedAnything = true;
						}
					}
					else
					{
						if (assetMapping.legacyAssetsTable[type].ContainsKey(asset.id))
						{
							Asset other;
							assetMapping.legacyAssetsTable[type].TryGetValue(asset.id, out other);

							ReportError(asset, $"legacy ID {asset.id} already taken by {other.FriendlyNameWithFriendlyType} in {other.GetOriginName()}!");
							return;
						}
					}

					assetMapping.legacyAssetsTable[type].Add(asset.id, asset);
				}
				else
				{
					bool needsLegacyId;
					switch (type)
					{
						case EAssetType.ITEM:
							needsLegacyId = !(asset is ItemAsset itemAsset && itemAsset.isPro);
							break;

						case EAssetType.EFFECT:
						case EAssetType.VEHICLE:
						case EAssetType.SPAWN:
						case EAssetType.NPC:
							needsLegacyId = false;
							break;

						default:
							needsLegacyId = true;
							break;
					}

					if (needsLegacyId)
					{
						ReportError(asset, "needs a non-zero ID");
					}
				}
			}

			if (asset.GUID != Guid.Empty)
			{
				if (overrideExistingID)
				{
					Asset existingAsset;
					if (assetMapping.assetDictionary.TryGetValue(asset.GUID, out existingAsset))
					{
						// If we found an existing asset then remove it from dictionary and list
						assetMapping.assetDictionary.Remove(existingAsset.GUID);
						assetMapping.assetList.Remove(existingAsset);
						existingAsset.hasBeenReplaced = true;
						replacedAnything = true;
					}
				}
				else
				{
					if (assetMapping.assetDictionary.ContainsKey(asset.GUID))
					{
						Asset other;
						assetMapping.assetDictionary.TryGetValue(asset.GUID, out other);

						ReportError(asset, $"GUID already taken by {other.FriendlyNameWithFriendlyType} in {other.GetOriginName()}!");
						return;
					}
				}

				assetMapping.assetDictionary.Add(asset.GUID, asset);
				assetMapping.assetList.Add(asset);
			}

			++assetMapping.modificationCounter;

			// This check is pretty hacky. Vehicles are the first place we are seeing about supporting asset reloading though.
			if (replacedAnything
				&& type == EAssetType.VEHICLE
				&& Level.isLoaded
				&& Provider.isServer
				&& VehicleManager.vehicles != null
				&& VehicleManager.vehicles.Count > 0)
			{
				VehicleManager.shouldRespawnReloadedVehicles = true;
			}

			if (asset.origin != null && asset.origin.workshopFileId != 0 && shouldLogWorkshopAssets)
			{
				UnturnedLog.info(asset.getTypeNameAndIdDisplayString());
			}
		}

		private static void AddAssetsFromOriginToCurrentMapping(AssetOrigin origin)
		{
			UnturnedLog.info($"Adding {origin.assets.Count} asset(s) from origin {origin.name} to server mapping");
			foreach (Asset asset in origin.assets)
			{
				AddToMapping(asset, true, currentAssetMapping);
			}
		}

		/// <summary>
		/// While playing on server the client will use the same dictionary/list of assets the server uses in order
		/// to reduce issues with ID conflicts.
		///
		/// 2023-05-27: server now ALSO uses the same logic to ensure IDs are applied in consistent order regardless
		/// of multi-threaded loading order.
		/// </summary>
		internal static void ApplyServerAssetMapping(LevelInfo pendingLevel, List<Steamworks.PublishedFileId_t> serverWorkshopFileIds)
		{
			currentAssetMapping = new AssetMapping();

			// List rather than directly adding immediately so that dedicated server can insert origins at front.
			List<AssetOrigin> originsToAdd = new List<AssetOrigin>();

			originsToAdd.Add(coreOrigin);

			AssetOrigin levelAssetOrigin = null;
			if (pendingLevel != null)
			{
				// Special handling for levels in the Maps folder.
				levelAssetOrigin = FindLevelOrigin(pendingLevel);
				if (levelAssetOrigin != null)
				{
					originsToAdd.Add(levelAssetOrigin);
				}
			}

			if (serverWorkshopFileIds != null)
			{
				foreach (Steamworks.PublishedFileId_t fileId in serverWorkshopFileIds)
				{
					AssetOrigin origin = FindWorkshopFileOrigin(fileId.m_PublishedFileId);
					if (origin != null)
					{
						if (origin == levelAssetOrigin)
						{
							// Skip because it was already added.
							continue;
						}

						originsToAdd.Add(origin);
					}
					else
					{
						// This can happen if server manually installed workshop content so the file ID is unknown during loading.
						UnturnedLog.info($"Unable to find assets for server mapping (file ID {fileId})");
					}
				}
			}

			if (Dedicator.IsDedicatedServer)
			{
				// Client will not have these same assets, so load them first to reduce chances of breaking things.
				foreach (AssetOrigin origin in assetOrigins)
				{
					if (origin == reloadOrigin || origin.assets.Count < 1)
						continue;

					if (!originsToAdd.Contains(origin))
					{
						UnturnedLog.info($"Inserting asset origin {origin.name} before other assets to reduce chances of ID conflicts because otherwise it was not included");
						originsToAdd.Insert(0, origin);
					}
				}
			}

			foreach (AssetOrigin origin in originsToAdd)
			{
				AddAssetsFromOriginToCurrentMapping(origin);
			}
		}

		internal static void ClearServerAssetMapping()
		{
			currentAssetMapping = defaultAssetMapping;
		}

		public static void RequestReloadAllAssets()
		{
			if (hasFinishedInitialStartupLoading && !isLoading)
			{
				instance.StartCoroutine(instance.LoadAllAssets());
			}
		}

		/// <summary>
		/// Search all loaded master bundles for one in path's hierarchy.
		/// </summary>
		public static MasterBundleConfig findMasterBundleByPath(string path)
		{
			int longestLength = 0;
			MasterBundleConfig bestMatch = null;

			foreach (MasterBundleConfig masterBundle in allMasterBundles)
			{
				if (masterBundle.directoryPath.Length < longestLength)
				{
					// Quick way to skip bundles higher in the file hierarchy.
					// For example bundle A in "Folder" has a shorter length than bundle B in
					// "Folder/SubFolder" and is therefore higher in the file hierarchy.
					continue;
				}

				if (!path.StartsWith(masterBundle.directoryPath))
				{
					// Asset file path doesn't start with the same path, so asset
					// can't be in a subdirectory. For example if path is "Items/Guns" and
					// bundle path is "Items/Hats" we know path can't be a subdirectory.
					continue;
				}

				// masterBundle.directoryPath *doesn't* end in '/' or '\', so we need
				// to check whether next char in file path is a separator otherwise it
				// could be a different folder with same prefix. (public issue #3885)
				if (path.Length > masterBundle.directoryPath.Length)
				{
					char nextChar = path[masterBundle.directoryPath.Length];
					if (nextChar != '/' && nextChar != '\\')
					{
						// For example Items_Suffix/Guns is not a child of Items/Guns.
						continue;
					}
				}

				longestLength = masterBundle.directoryPath.Length;
				bestMatch = masterBundle;
			}

			return bestMatch;
		}

		public static MasterBundleConfig findMasterBundleInListByName(List<MasterBundleConfig> list, string name, bool matchExtension = true)
		{
			foreach (MasterBundleConfig potentialMasterBundle in list)
			{
				string matchName = matchExtension ? potentialMasterBundle.assetBundleName : potentialMasterBundle.assetBundleNameWithoutExtension;
				if (matchName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return potentialMasterBundle;
				}
			}

			return null;
		}

		/// <summary>
		/// Find loaded master bundle by name.
		/// </summary>
		public static MasterBundleConfig findMasterBundleByName(string name, bool matchExtension = true)
		{
			return findMasterBundleInListByName(allMasterBundles, name, matchExtension);
		}

		/// <summary>
		/// Unload all asset bundles from memory, and empty known list.
		/// Called when reloading assets.
		/// </summary>
		private static void UnloadAllMasterBundles()
		{
			foreach (MasterBundleConfig config in allMasterBundles)
			{
				config.unload();
			}
			allMasterBundles.Clear();
		}

		/// <summary>
		/// Catches exceptions thrown by LoadFile to avoid breaking loading.
		/// </summary>
		private static void TryLoadFile(AssetsWorker.AssetDefinition file)
		{
			try
			{
#if WITH_ASSETS_PROFILING
				loadFileSampler.Begin();
#endif // WITH_ASSETS_PROFILING

				++loadingStats.totalFilesLoaded;
				LoadFile(file);

#if WITH_ASSETS_PROFILING
				loadFileSampler.End();
#endif // WITH_ASSETS_PROFILING
			}
			catch (Exception e)
			{
				UnturnedLog.error("Exception loading file {0}:", file.path);
				UnturnedLog.exception(e);
			}
		}

		private static void LoadFile(AssetsWorker.AssetDefinition file)
		{
			string assetPath = file.path;
			IDatDictionary rootData = file.assetData;
			byte[] dataHash = file.hash;

			if (assetPath.Length > 260)
			{
				// Nelson 2024-11-11: Haven't run into this MAX_PATH limit directly, but might be helpful for some
				// modders and server hosts. (public issue #4777)
				reportError($"Asset path exceeds 260 characters and might not load properly on Windows: \"{assetPath}\"");
			}

			if (file.assetErrors != null)
			{
				foreach (string errorMessage in file.assetErrors)
				{
					reportError($"Error parsing \"{assetPath}\": \"{errorMessage}\"");
				}
				// Parsing does not stop when an error is encountered. Unfortunately lots of third-party assets
				// have typos which technically work correctly if ignored, and the old parser didn't log them.
			}

			string assetDirectory = Path.GetDirectoryName(assetPath);
			string assetInternalName = assetPath.EndsWith("Asset.dat", StringComparison.OrdinalIgnoreCase) ? Path.GetFileName(assetDirectory) : Path.GetFileNameWithoutExtension(assetPath);

			System.Guid assetGuid = default;
			System.Type assetType = null;
			IDatDictionary metadata;
			if (rootData.TryGetDictionary("Metadata", out metadata))
			{
				if (!metadata.TryParseGuid("GUID", out assetGuid))
				{
					reportError($"Unable to parse Metadata.GUID in \"{assetPath}\"");
					return;
				}

				assetType = metadata.ParseType("Type");
				if (assetType == null)
				{
					reportError($"Unable to parse Metadata.Type in \"{assetPath}\"");
					return;
				}
			}
			else
			{
				// If the GUID is missing we can assign one which is very convenient, but if the GUID
				// is mis-formatted it will use the one lower in the file causing it to be repeatedly
				// re-assigned. (public issue #4078)
				if (!rootData.ContainsKey("GUID"))
				{
					assetGuid = Guid.NewGuid();

					try
					{
						string text = File.ReadAllText(assetPath);
						// Nelson 2025-04-14: changed this from '\n' to Environment.NewLine to fix inadvertently
						// mixing \r\n and \n in asset files.
						text = "GUID " + assetGuid.ToString("N") + Environment.NewLine + text;
						File.WriteAllText(assetPath, text);
						UnturnedLog.info($"Assigned GUID {assetGuid:N} to asset \"{assetPath}\"");
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, $"Caught IO exception adding GUID to \"{assetPath}\":");
					}
				}
				else if (!rootData.TryParseGuid("GUID", out assetGuid))
				{
					reportError($"Unable to parse GUID in \"{assetPath}\"");
					return;
				}
			}

			if (assetGuid.IsEmpty())
			{
				reportError($"Cannot use empty GUID in \"{assetPath}\"");
				return;
			}

			IDatDictionary data = rootData;
			if (rootData.TryGetDictionary("Asset", out IDatDictionary assetData))
			{
				data = assetData;
			}

			if (assetType == null)
			{
				string legacyType = data.GetString("Type");
				if (string.IsNullOrEmpty(legacyType))
				{
					reportError($"Missing asset Type in \"{assetPath}\"");
					return;
				}

				assetType = assetTypes.getType(legacyType);
				if (assetType == null)
				{
					// "v2" assets all have an actual type name in the Metadata dictionary, but "v1" assets
					// use an enum-to-type mapping. Parsing Type here is only to avoid unexpected behaviour
					// if a modder tries to use an actual type name in a v1-style asset.
					assetType = data.ParseType("Type");
					if (assetType == null)
					{
						reportError($"Unhandled asset type \"{legacyType}\" in \"{assetPath}\"");
						return;
					}
				}
			}

			if (!typeof(Asset).IsAssignableFrom(assetType))
			{
				reportError($"Type \"{assetType}\" is not a valid asset type in \"{assetPath}\"");
				return;
			}

			MasterBundleConfig masterBundle = findMasterBundleByPath(assetPath);
			string masterBundleOverrideName = data.GetString("Master_Bundle_Override", defaultValue: null);
			if (masterBundleOverrideName != null)
			{
				masterBundle = findMasterBundleByName(masterBundleOverrideName);
				if (masterBundle == null)
				{
					UnturnedLog.warn("Unable to find master bundle override '{0}' for '{1}'", masterBundleOverrideName, assetPath);
				}
			}
			else if (data.ContainsKey("Exclude_From_Master_Bundle"))
			{
				masterBundle = null;
			}

			if (masterBundle != null && masterBundle.assetBundle == null)
			{
				UnturnedLog.warn("Skipping master bundle '{0}' for '{1}' because asset bundle is null", masterBundle.assetBundleName, assetPath);
				masterBundle = null;
			}

			currentMasterBundle = masterBundle;

			// Default -1 so that we know if it was set from master bundle.
			int masterBundleVersion = -1;

			Bundle bundle;
			if (masterBundle != null)
			{
				string bundleRelativePath;
				if (!data.TryGetString("Bundle_Override_Path", out bundleRelativePath))
				{
					// Did not override, so figure out the filesystem relative path.

					// Nelson 2025-02-26: First case here is the old default (off). When off:
					// Guns/Eaglefire/Eaglefire.asset → Guns/Eaglefire/Item.prefab
					// If enabled, multiple *.asset are mapped to subdirectories:
					// Guns/Eaglefire.asset → Guns/Eaglefire/Item.prefab
					// Guns/Maplestrike.asset → Guns/Maplestrike/Item.prefab
					if (!data.ParseBool("Bundle_Path_Include_Filename"))
					{
						bundleRelativePath = assetDirectory.Substring(masterBundle.directoryPath.Length);
					}
					else
					{
						bundleRelativePath = Path.ChangeExtension(assetPath, null).Substring(masterBundle.directoryPath.Length);
					}
					bundleRelativePath = bundleRelativePath.Replace('\\', '/');
				}
				bundle = new MasterBundle(masterBundle, bundleRelativePath, assetInternalName);

				// Nelson 2023-12-18: a player reported that they swapped the contents of a scope asset
				// file with the contents of an ironsights asset file so that they could use the scope
				// by default on guns. This worked because the .dat and masterbundle hash matched the server,
				// but the path was different. Now including the relative path in the hash to prevent it.
				// Nelson 2023-12-23: oof, used ToLower rather than ToLowerInvariant which caused
				// a different result for players with different culture info. (public issue #4279)
				string hashablePath = bundleRelativePath.ToLowerInvariant() + '/' + assetInternalName.ToLowerInvariant();
				dataHash = Hash.combine(dataHash, Hash.SHA1(hashablePath));

				// If we're using the newer master bundles feature we don't need shader conversion.
				// Otherwise actually all of the vanilla content gets converted!
				masterBundleVersion = masterBundle.version;
			}
			else if (data.ContainsKey("Bundle_Override_Path"))
			{
				string bundleOverridePath = data.GetString("Bundle_Override_Path");
				string fileName;
				int lastSlash = bundleOverridePath.LastIndexOf('/');
				if (lastSlash == -1) // Probably will never happen?
				{
					fileName = bundleOverridePath;
				}
				else
				{
					fileName = bundleOverridePath.Substring(lastSlash + 1);
				}
				bundleOverridePath += "/" + fileName + ".unity3d";

				bundle = new Bundle(bundleOverridePath, false, assetInternalName);
			}
			else
			{
				bundle = new Bundle(assetDirectory + "/" + assetInternalName + ".unity3d", false);
			}

			int individualVersion = data.ParseInt32("Asset_Bundle_Version", defaultValue: AssetBundleVersion.UNITY_5);
			if (individualVersion < AssetBundleVersion.UNITY_5)
			{
				reportError(assetInternalName + " Lowest individual asset bundle version is 1 (default), associated with 5.5.");
				individualVersion = AssetBundleVersion.UNITY_5;
			}
			else if (individualVersion > AssetBundleVersion.NEWEST)
			{
				reportError(assetInternalName + " Highest individual asset bundle version is 6, associated with 2022 LTS.");
				individualVersion = AssetBundleVersion.NEWEST;
			}

			// We use higher of the two because some modders increased the asset bundle version on a per-file
			// basis rather than for the master bundle as a whole, but were confused why it did not apply.
			int effectiveAssetBundleVersion = Mathf.Max(masterBundleVersion, individualVersion);

			bundle.convertShadersToStandard = effectiveAssetBundleVersion < AssetBundleVersion.UNITY_2017_LTS;
			bundle.consolidateShaders = effectiveAssetBundleVersion < AssetBundleVersion.UNITY_2018_AND_2019_LTS
				|| (data.ContainsKey("Enable_Shader_Consolidation") && !data.ContainsKey("Disable_Shader_Consolidation"));

			Local localization = new Local(file.translationData, file.fallbackTranslationData);

			ushort legacyId = data.ParseUInt16("ID");

			Asset asset;
			try
			{
#if WITH_ASSET_CONSOLIDATION
				currentFilePath = assetPath;
#endif

#if WITH_ASSETS_PROFILING
				createInstance.Begin();
#endif // WITH_ASSETS_PROFILING
				asset = System.Activator.CreateInstance(assetType) as Asset;
#if WITH_ASSETS_PROFILING
				createInstance.End();
#endif // WITH_ASSETS_PROFILING
			}
			catch (System.Exception exception)
			{
				reportError($"Caught exception while constructing {assetType} in \"{assetPath}\": {getExceptionMessage(exception)}");
				UnturnedLog.exception(exception);

				bundle.unload();
				currentMasterBundle = null;
				currentAsset = null;
				return;
			}

			if (asset == null)
			{
				reportError($"Failed to construct {assetType} in \"{assetPath}\"");
				bundle.unload();
				currentMasterBundle = null;
				currentAsset = null;
				return;
			}

			currentAsset = asset;

			try
			{
				asset.id = legacyId;
				asset.GUID = assetGuid;
				asset.hash = dataHash;
				asset.requiredShaderUpgrade = bundle.convertShadersToStandard || bundle.consolidateShaders;
				asset.HasErrors = file.assetErrors != null && file.assetErrors.Count > 0;

				asset.absoluteOriginFilePath = assetPath;
				asset.origin = file.origin;

				bool canPerformDataConversions = shouldResaveAssets && (asset.origin?.canResave ?? false) && shouldParseMetadata;
				if (canPerformDataConversions)
				{
					asset.OriginParsedData = rootData;
				}

				if (data.ParseBool("Keep_Localization_Loaded"))
				{
					asset.Localization = localization;
				}

#if WITH_ASSETS_PROFILING
				populateAsset.Begin();
				try
				{
#endif // WITH_ASSETS_PROFILING
					PopulateAssetParameters populateAssetParameters = new PopulateAssetParameters()
					{
						bundle = bundle,
						data = data,
						localization = localization,
						CanPerformDataConversions = canPerformDataConversions,
					};
					asset.PopulateAsset(populateAssetParameters);
#if WITH_ASSETS_PROFILING
				}
				finally
				{
					populateAsset.End();
				}
#endif // WITH_ASSETS_PROFILING

				asset.origin.assets.Add(asset);
				AddToMapping(asset, file.origin.shouldAssetsOverrideExistingIds, defaultAssetMapping);

				bundle.unload();
			}
			catch (System.Exception exception)
			{
				reportError($"Caught exception while populating \"{assetPath}\": {getExceptionMessage(exception)}");
				UnturnedLog.exception(exception);

				bundle.unload();
			}

			currentMasterBundle = null;
			currentAsset = null;
		}

		/// <summary>
		/// Called when a new workshop item is installed either on client or server.
		/// </summary>
		public static void RequestAddSearchLocation(string absoluteDirectoryPath, AssetOrigin origin)
		{
			instance.AddSearchLocation(absoluteDirectoryPath, origin);
		}

		/// <summary>
		/// Reload assets in given folder.
		/// </summary>
		public static void reload(string absolutePath)
		{
			if (hasFinishedInitialStartupLoading && !isLoading)
			{
				loadingStats.Reset();
				RequestAddSearchLocation(absolutePath, reloadOrigin);
			}
		}

		public static void ReloadAsset(Asset asset)
		{
			string directory = Path.GetDirectoryName(asset.absoluteOriginFilePath);
			reload(directory);
		}

		/// <summary>
		/// Do we have any new spawn assets that have not been linked yet?
		/// Used to skip linking spawns if not required when downloading assets.
		/// </summary>
		private static bool hasUnlinkedSpawns;

		public static void linkSpawnsIfDirty()
		{
			if (hasUnlinkedSpawns)
			{
				UnturnedLog.info("Linking spawns because changes were detected");
				linkSpawns();
			}
			else
			{
				UnturnedLog.info("Skipping link spawns because no changes were detected");
			}
		}

		/// <summary>
		/// Can now be safely called multiple times on client in order to handle spawns for downloaded maps.
		/// Spawn tables have "roots" which allow mods to insert custom spawns into the vanilla spawn tables.
		/// This method is used after workshop assets are loaded on client, or after the dedicated server is done downloading workshop content.
		/// </summary>
		public static void linkSpawns()
		{
			if (hasUnlinkedSpawns)
				hasUnlinkedSpawns = false;
			else
				return;

			List<SpawnAsset> spawnAssetList = new List<SpawnAsset>();
			FindAssetsByType_UseDefaultAssetMapping(spawnAssetList);

			int numChildrenInserted = 0;
			int numTablesNormalized = 0;
			int numParentsInserted = 0;

			// Insert roots as children of other spawn tables and then empty roots
			foreach (SpawnAsset spawnAsset in spawnAssetList)
			{
				if (spawnAsset.insertRoots.Count < 1)
					continue; // Already inserted roots, or none to insert.

				foreach (SpawnTable spawnTableEntry in spawnAsset.insertRoots)
				{
					SpawnAsset desiredParentAsset;
					if (spawnTableEntry.legacySpawnId != 0)
					{
						desiredParentAsset = find(EAssetType.SPAWN, spawnTableEntry.legacySpawnId) as SpawnAsset;
						if (desiredParentAsset == null)
						{
							ReportError(spawnAsset, "unable to find root {0} during link", spawnTableEntry.legacySpawnId);
							continue;
						}
					}
					else if (!spawnTableEntry.targetGuid.IsEmpty())
					{
						Asset targetAsset = find(spawnTableEntry.targetGuid);
						if (targetAsset == null)
						{
							ReportError(spawnAsset, "unable to find root {0} during link", spawnTableEntry.targetGuid);
							continue;
						}

						desiredParentAsset = targetAsset as SpawnAsset;
						if (desiredParentAsset == null)
						{
							ReportError(spawnAsset, $"root {spawnTableEntry.targetGuid} found as {targetAsset.GetTypeFriendlyName()} {targetAsset.FriendlyName} (not a spawn table)");
							continue;
						}
					}
					else
					{
						// Mis-configured and should already have been logged.
						continue;
					}

					spawnTableEntry.legacySpawnId = 0;
					spawnTableEntry.targetGuid = spawnAsset.GUID;
					spawnTableEntry.isLink = true;
					desiredParentAsset.tables.Add(spawnTableEntry);

					if (spawnTableEntry.isOverride)
					{
						desiredParentAsset.markOverridden();
					}

					desiredParentAsset.markTablesDirty();

					++numChildrenInserted;

					if (shouldLogSpawnInsertions)
					{
						if (spawnTableEntry.isOverride)
						{
							UnturnedLog.info("Spawn {0} overriding {1}", spawnAsset.name, desiredParentAsset.name);
						}
						else
						{
							UnturnedLog.info("Spawn {0} inserted into {1}", spawnAsset.name, desiredParentAsset.name);
						}
					}
				}

				spawnAsset.insertRoots.Clear();
			}

			// Sort and total up the weights of every table
			foreach (SpawnAsset spawnAsset in spawnAssetList)
			{
				if (spawnAsset.areTablesDirty)
				{
					spawnAsset.sortAndNormalizeWeights();

					++numTablesNormalized;
				}
			}

			// Insert tables as roots of child spawn tables
			foreach (SpawnAsset spawnAsset in spawnAssetList)
			{
				foreach (SpawnTable spawnTableEntry in spawnAsset.tables)
				{
					if (spawnTableEntry.hasNotifiedChild)
					{
						continue; // Already added to child roots.
					}
					else
					{
						spawnTableEntry.hasNotifiedChild = true;
					}

					SpawnAsset childAsset;
					if (spawnTableEntry.legacySpawnId != 0)
					{
						childAsset = find(EAssetType.SPAWN, spawnTableEntry.legacySpawnId) as SpawnAsset;
						if (childAsset == null)
						{
							ReportError(spawnAsset, "unable to find child table {0} during link", spawnTableEntry.legacySpawnId);
							continue;
						}
					}
					else if (!spawnTableEntry.targetGuid.IsEmpty())
					{
						Asset targetAsset = find(spawnTableEntry.targetGuid);
						if (targetAsset == null)
						{
							ReportError(spawnAsset, "unable to find child {0} during link", spawnTableEntry.targetGuid);
							continue;
						}

						childAsset = targetAsset as SpawnAsset;
						if (childAsset == null)
						{
							// Don't warn for this because targetGuid can be a spawn asset or a regular asset.
							continue;
						}
					}
					else
					{
						continue;
					}

					SpawnTable root = new SpawnTable();
					root.targetGuid = spawnAsset.GUID;
					root.weight = spawnTableEntry.weight;
					root.normalizedWeight = spawnTableEntry.normalizedWeight;
					root.isLink = spawnTableEntry.isLink;
					root.isOverride = spawnTableEntry.isOverride;

					childAsset.roots.Add(root);

					++numParentsInserted;
				}
			}

			UnturnedLog.info("Link spawns: {0} children, {1} sorted/normalized and {2} parents", numChildrenInserted, numTablesNormalized, numParentsInserted);
		}

		public static void initializeMasterBundleValidation()
		{
			MasterBundleValidation.initialize(allMasterBundles);
		}

		/// <summary>
		/// Look through all item blueprints and log errors if there are duplicates.
		/// </summary>
		private void CheckForBlueprintErrors()
		{
			Func<Blueprint, Blueprint, bool> AreBlueprintsIdentical = (Blueprint myBlueprint, Blueprint yourBlueprint) =>
			{
				if (myBlueprint.Operation != yourBlueprint.Operation)
				{
					return false;
				}

				if (myBlueprint.SkillSpecialityIndex != yourBlueprint.SkillSpecialityIndex)
				{
					return false;
				}

				if (myBlueprint.SkillIndex != yourBlueprint.SkillIndex)
				{
					return false;
				}

				if (myBlueprint.CategoryTagRef != yourBlueprint.CategoryTagRef)
				{
					return false;
				}

				if (myBlueprint.outputs.Length != yourBlueprint.outputs.Length)
				{
					return false;
				}

				if (myBlueprint.supplies.Length != yourBlueprint.supplies.Length)
				{
					return false;
				}

				if ((myBlueprint.questConditions != null) != (yourBlueprint.questConditions != null))
				{
					return false;
				}

				if (myBlueprint.questConditions != null && myBlueprint.questConditions.Length != yourBlueprint.questConditions.Length)
				{
					return false;
				}

				if ((myBlueprint.questRewards != null) != (yourBlueprint.questRewards != null))
				{
					return false;
				}

				if (myBlueprint.questRewards != null && myBlueprint.questRewards.Length != yourBlueprint.questRewards.Length)
				{
					return false;
				}

				if ((myBlueprint.RequiresNearbyCraftingTags != null) != (yourBlueprint.RequiresNearbyCraftingTags != null))
				{
					return false;
				}

				if (myBlueprint.RequiresNearbyCraftingTags != null && myBlueprint.RequiresNearbyCraftingTags.Length != yourBlueprint.RequiresNearbyCraftingTags.Length)
				{
					return false;
				}

				if ((myBlueprint.TargetItem != null) != (yourBlueprint.TargetItem != null))
				{
					return false;
				}

				if (myBlueprint.TargetItem != null && !myBlueprint.TargetItem.Equals(yourBlueprint.TargetItem))
				{
					return false;
				}

				for (byte outputIndex = 0; outputIndex < myBlueprint.outputs.Length; outputIndex++)
				{
					if (myBlueprint.outputs[outputIndex].ItemRef != yourBlueprint.outputs[outputIndex].ItemRef)
					{
						return false;
					}
				}

				for (byte supplyIndex = 0; supplyIndex < myBlueprint.supplies.Length; supplyIndex++)
				{
					if (!myBlueprint.supplies[supplyIndex].Equals(yourBlueprint.supplies[supplyIndex]))
					{
						return false;
					}
				}

				if (myBlueprint.questConditions != null)
				{
					for (int conditionIndex = 0; conditionIndex < myBlueprint.questConditions.Length; ++conditionIndex)
					{
						if (!myBlueprint.questConditions[conditionIndex].Equals(yourBlueprint.questConditions[conditionIndex]))
						{
							return false;
						}
					}
				}

				if (myBlueprint.questRewards != null)
				{
					for (int rewardIndex = 0; rewardIndex < myBlueprint.questRewards.Length; ++rewardIndex)
					{
						if (!myBlueprint.questRewards[rewardIndex].Equals(yourBlueprint.questRewards[rewardIndex]))
						{
							return false;
						}
					}
				}

				if (myBlueprint.RequiresNearbyCraftingTags != null)
				{
					for (int tagIndex = 0; tagIndex < myBlueprint.RequiresNearbyCraftingTags.Length; ++tagIndex)
					{
						if (!myBlueprint.RequiresNearbyCraftingTags[tagIndex].Equals(yourBlueprint.RequiresNearbyCraftingTags[tagIndex]))
						{
							return false;
						}
					}
				}

				return true;
			};

			List<ItemAsset> itemAssets = new List<ItemAsset>();
			find(itemAssets);
			if (itemAssets.Count > 0)
			{
				for (int myItemAssetIndex = 0; myItemAssetIndex < itemAssets.Count; myItemAssetIndex++)
				{
					ItemAsset myItemAsset = itemAssets[myItemAssetIndex];

					for (byte myBlueprintIndex = 0; myBlueprintIndex < myItemAsset.blueprints.Count; myBlueprintIndex++)
					{
						Blueprint myBlueprint = myItemAsset.blueprints[myBlueprintIndex];

						for (byte yourBlueprintIndex = 0; yourBlueprintIndex < myItemAsset.blueprints.Count; yourBlueprintIndex++)
						{
							if (yourBlueprintIndex == myBlueprintIndex)
							{
								continue;
							}

							Blueprint yourBlueprint = myItemAsset.blueprints[yourBlueprintIndex];

							if (AreBlueprintsIdentical(myBlueprint, yourBlueprint))
							{
								ReportError(myItemAsset, $"blueprint [{myBlueprintIndex}] is identical to blueprint [{yourBlueprintIndex}]");
							}
						}

						if (myBlueprint.supplies != null && myBlueprint.supplies.Length > 1)
						{
							for (int supplyIndexA = 0; supplyIndexA < myBlueprint.supplies.Length - 1; ++supplyIndexA)
							{
								for (int supplyIndexB = supplyIndexA + 1; supplyIndexB < myBlueprint.supplies.Length; ++supplyIndexB)
								{
									BlueprintSupply supplyA = myBlueprint.supplies[supplyIndexA];
									BlueprintSupply supplyB = myBlueprint.supplies[supplyIndexB];
									if (supplyA.Equals(supplyB))
									{
										ReportError(myItemAsset, $"blueprint [{myBlueprintIndex}] input items [{supplyIndexA}] and [{supplyIndexB}] are identical");
									}
								}
							}
						}
					}

					for (int yourItemAssetIndex = 0; yourItemAssetIndex < itemAssets.Count; yourItemAssetIndex++)
					{
						if (yourItemAssetIndex == myItemAssetIndex)
						{
							continue;
						}

						ItemAsset yourItemAsset = itemAssets[yourItemAssetIndex];

						for (byte myBlueprintIndex = 0; myBlueprintIndex < myItemAsset.blueprints.Count; myBlueprintIndex++)
						{
							Blueprint myBlueprint = myItemAsset.blueprints[myBlueprintIndex];

							for (byte yourBlueprintIndex = 0; yourBlueprintIndex < yourItemAsset.blueprints.Count; yourBlueprintIndex++)
							{
								Blueprint yourBlueprint = yourItemAsset.blueprints[yourBlueprintIndex];

								if (AreBlueprintsIdentical(myBlueprint, yourBlueprint))
								{
									ReportError(myItemAsset, $"blueprint [{myBlueprintIndex}] is identical to {yourItemAsset.itemName} blueprint [{yourBlueprintIndex}]");
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Look through all dialogue and check that their referenced
		/// dialogueID or vendorID is an actual loaded asset.
		/// </summary>
		private void CheckForNpcErrors()
		{
			List<DialogueAsset> npcAssets = new List<DialogueAsset>();
			find(npcAssets);
			foreach (DialogueAsset dialogueAsset in npcAssets)
			{
				int responseCount = dialogueAsset.responses.Length;
				for (int responseIndex = 0; responseIndex < responseCount; responseIndex++)
				{
					DialogueResponse response = dialogueAsset.responses[responseIndex];

					if (!response.IsDialogueRefNull())
					{
						DialogueAsset otherDialogue = response.FindDialogueAsset();
						if (otherDialogue == null)
						{
							ReportError(dialogueAsset, "unable to find dialogue asset for response " + responseIndex);
						}
					}

					if (!response.IsVendorRefNull())
					{
						VendorAsset vendor = response.FindVendorAsset();
						if (vendor == null)
						{
							ReportError(dialogueAsset, "unable to find vendor asset for response " + responseIndex);
						}
					}
				}
			}
		}

		/// <summary>
		/// Manually run asset unload and garbage collection in the hope
		/// that it will minimize RAM allocated during loading.
		/// </summary>
		private void CleanupMemory()
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		/// <summary>
		/// Helper for Assets.init.
		/// Load all non-map assets from:
		///		/Bundles/Workshop/Content
		///		/Servers/ServerID/Workshop/Content
		///		/Servers/ServerID/Bundles
		/// </summary>
		private void AddDedicatedServerUgcSearchLocations()
		{
			string legacyServerSharedPath = Path.Combine(ReadWrite.PATH, "Bundles", "Workshop", "Content");
			if (ReadWrite.folderExists(legacyServerSharedPath, false))
			{
				AddSearchLocation(legacyServerSharedPath, legacyServerSharedOrigin);
			}

			string legacyPerServerPath = Path.Combine(ReadWrite.PATH, ServerSavedata.directoryName, Provider.serverID, "Workshop", "Content");
			if (ReadWrite.folderExists(legacyPerServerPath, false))
			{
				AddSearchLocation(legacyPerServerPath, legacyPerServerOrigin);
			}

			string legacyPerServerBundlesPath = Path.Combine(ReadWrite.PATH, ServerSavedata.directoryName, Provider.serverID, "Bundles");
			if (ReadWrite.folderExists(legacyPerServerBundlesPath, false))
			{
				AddSearchLocation(legacyPerServerBundlesPath, legacyPerServerOrigin);
			}
		}

		/// <summary>
		/// Helper for Assets.init.
		/// Load all non-map assets from subscribed UGC.
		/// </summary>
		private void AddClientUgcSearchLocations()
		{
			if (Provider.provider.workshopService.ugc != null)
			{
				SteamContent[] ugc = Provider.provider.workshopService.ugc.ToArray();

				// We have our copy of UGC, so new items added won't be loaded.
				hasLoadedUgc = true;

				foreach (SteamContent content in ugc)
				{
					if (LocalWorkshopSettings.get().getEnabled(content.publishedFileID) == false)
						continue;

					if (content.type == ESteamUGCType.OBJECT || content.type == ESteamUGCType.ITEM || content.type == ESteamUGCType.VEHICLE)
					{
						AssetOrigin origin = FindOrAddWorkshopFileOrigin(content.publishedFileID.m_PublishedFileId, false);
						AddSearchLocation(content.path, origin);
					}
				}
			}
		}

		/// <summary>
		/// Helper for modders creating workshop content.
		/// Loads folders in the "Sandbox" directory the same way workshop files are loaded.
		/// </summary>
		private void AddSandboxSearchLocations()
		{
			string sandboxPath = Path.Combine(ReadWrite.PATH, "Sandbox");
			if (Directory.Exists(sandboxPath))
			{
				string[] sandboxFolders = ReadWrite.getFolders(sandboxPath, false);
				foreach (string sandboxFolder in sandboxFolders)
				{
					// Nelson 2024-11-11: Yes, we want GetFileName here, not GetDirectoryName. The latter returns the
					// directory path whereas GetFileName returns just the name of the directory.
					string folderName = Path.GetFileName(sandboxFolder);
					UnturnedLog.info("Sandbox: {0}", folderName);

					AssetOrigin origin = new AssetOrigin();
					origin.name = $"Sandbox Folder \"{folderName}\"";
					origin.shouldAssetsOverrideExistingIds = true;
					origin.canResave = true;
					assetOrigins.Add(origin);

					AddSearchLocation(sandboxFolder, origin);
				}
			}
			else
			{
				Directory.CreateDirectory(sandboxPath);
			}
		}

		/// <summary>
		/// Helper for Assets.init.
		/// Load all assets in each map's Bundles folder, and content in map's Content folder.
		/// </summary>
		private void AddMapSearchLocations()
		{
			LevelInfo[] levels = Level.getLevels(ESingleplayerMapCategory.ALL);

			// We have our copy of maps, so new maps added won't be loaded.
			hasLoadedMaps = true;

			for (int index = 0; index < levels.Length; index++)
			{
				LevelInfo level = levels[index];

				if (level == null)
				{
					continue;
				}

				string mapBundlesPath = Path.Combine(level.path, "Bundles");
				if (ReadWrite.folderExists(mapBundlesPath, false))
				{
					AssetOrigin origin = FindOrAddLevelOrigin(level);
					AddSearchLocation(mapBundlesPath, origin);
				}
			}
		}

		private void AddSearchLocation(string path, AssetOrigin origin)
		{
			path = Path.GetFullPath(path);
			UnturnedLog.info($"{origin.name} added asset search location \"{path}\"");
			worker.RequestSearch(path, origin);
		}

		private MasterBundleConfig FindAndRemoveLoadedPendingMasterBundle()
		{
			for (int index = pendingMasterBundles.Count - 1; index >= 0; --index)
			{
				MasterBundleConfig config = pendingMasterBundles[index];
				if (config.assetBundleCreateRequest.isDone)
				{
					pendingMasterBundles.RemoveAtFast(index);
					return config;
				}
			}

			return null;
		}

		private IEnumerator LoadAssetsFromWorkerThread()
		{
			double previousFrameTime = Time.realtimeSinceStartupAsDouble;

			int gcFrameCount = 0;

			while (worker.IsWorking || pendingMasterBundles.Count > 0 || pendingAssetsToLoad.Count > 0)
			{
				AssetsWorker.ResultItem nextItem;
				if (worker.TryDequeueResult(out nextItem))
				{
					switch (nextItem.ResultType)
					{
						case AssetsWorker.EResultType.MasterBundle:
						{
							AssetsWorker.MasterBundle mb = (AssetsWorker.MasterBundle) nextItem;
							MasterBundleConfig config = mb.config;
							pendingMasterBundles.Add(config);
							config.StartLoad(mb.assetBundleData, mb.hash);
							loadingStats.isLoadingAssetBundles = true;
							break;
						}

						case AssetsWorker.EResultType.Asset:
						{
							AssetsWorker.AssetDefinition asset = (AssetsWorker.AssetDefinition) nextItem;
							pendingAssetsToLoad.Enqueue(asset);
							break;
						}

						case AssetsWorker.EResultType.Exception:
						{
							AssetsWorker.ExceptionDetails exceptionDetails = (AssetsWorker.ExceptionDetails) nextItem;
							UnturnedLog.exception(exceptionDetails.exception, exceptionDetails.message);
							break;
						}
					}
				}

				if (pendingMasterBundles.Count > 0)
				{
					MasterBundleConfig config = FindAndRemoveLoadedPendingMasterBundle();
					if (config != null)
					{
						config.FinishLoad();
						++loadingStats.totalMasterBundlesLoaded;

						if (config.assetBundle != null)
						{
							// Check name matches (oops!) because technically modders can put additional asset bundles in the Bundles folder. (public issue #5453)
							if (config.origin == coreOrigin && string.Equals(config.assetBundleName, "core.masterbundle", StringComparison.InvariantCulture))
							{
								coreMasterBundle = config;
							}
							allMasterBundles.Add(config);
						}
						else
						{
							// It might have failed because another asset bundle with the same files
							// was already loaded. If that's the case we create a "proxy" config.
							MasterBundleConfig existingConfig = findMasterBundleByName(config.assetBundleName);
							if (existingConfig != null)
							{
								config.CopyAssetBundleFromDuplicateConfig(existingConfig);
								if (config.assetBundle != null)
								{
									UnturnedLog.info($"Using \"{existingConfig.assetBundleName}\" in \"{existingConfig.directoryPath}\" as fallback asset bundle for \"{config.directoryPath}\"");
									allMasterBundles.Add(config);
								}
								else
								{
									UnturnedLog.info($"Unable to use \"{existingConfig.assetBundleName}\" in \"{existingConfig.directoryPath}\" as fallback asset bundle for \"{config.directoryPath}\"");
								}
							}
							else
							{
								UnturnedLog.info($"Unable to find a fallback asset bundle for \"{config.assetBundleName}\"");
							}
						}
					}

					if (pendingMasterBundles.Count < 1)
					{
						loadingStats.isLoadingAssetBundles = false;
					}
				}
				else
				{
					if (coreMasterBundle == null)
					{
						// Do nothing until core master bundle is loaded.
						// Kind of a hack, but lots of stuff depends on it.
					}
					else
					{
						AssetsWorker.AssetDefinition asset;
						if (pendingAssetsToLoad.TryDequeue(out asset))
						{
							//UnturnedLog.info("Main thread received asset: " + asset.path);
							TryLoadFile(asset);
						}
					}
				}

				// Previously the game loaded a constant (25) number of assets per frame, but this had two bad cases:
				// - If those assets loaded quickly an unnecessary amount of time would be wasted on highish FPS loading screen.
				// - If those assets loaded slowly the game was very unresponsive and Windows might show the unresponsive program popup.
				// Instead we now target 20 frames per second.
				double timeSinceLastFrame = Time.realtimeSinceStartupAsDouble - previousFrameTime;
				if (timeSinceLastFrame > 0.05)
				{
					SyncAssetDefinitionLoadingProgress();
					++gcFrameCount;
					if (gcFrameCount % 25 == 0 && shouldCollectGarbageAggressively)
					{
						CleanupMemory();
					}
					yield return null;
					previousFrameTime = Time.realtimeSinceStartupAsDouble;
				}
			}
		}

		internal static void SyncAssetDefinitionLoadingProgress()
		{
			loadingStats.totalRegisteredSearchLocations = instance.worker.totalSearchLocationRequests;
			loadingStats.totalSearchLocationsFinishedSearching = instance.worker.totalSearchLocationsFinishedSearching;
			loadingStats.totalMasterBundlesFound = instance.worker.totalMasterBundlesFound;
			loadingStats.totalFilesFound = instance.worker.totalAssetDefinitionsFound;
			loadingStats.totalFilesRead = instance.worker.totalAssetDefinitionsRead;
			LoadingUI.NotifyAssetDefinitionLoadingProgress();
		}

		private IEnumerator LoadAllAssets()
		{
			isLoadingAllAssets = true;
			double startTime = Time.realtimeSinceStartupAsDouble;

			if (errors == null)
			{
				errors = new List<string>();
			}
			else
			{
				errors.Clear();
			}

			defaultAssetMapping = new AssetMapping();
			currentAssetMapping = defaultAssetMapping;

#if WITH_ASSET_CONSOLIDATION
			AssetConsolidation.ReadDuplicateReport();
#endif // WITH_ASSET_CONSOLIDATION

			coreMasterBundle = null;
			if (allMasterBundles == null)
			{
				allMasterBundles = new List<MasterBundleConfig>();
				pendingMasterBundles = new List<MasterBundleConfig>();
				pendingAssetsToLoad = new Queue<AssetsWorker.AssetDefinition>();
			}
			else
			{
				UnloadAllMasterBundles();
				pendingAssetsToLoad.Clear();
			}

			assetOrigins = new List<AssetOrigin>();

			loadingStats.Reset();

			coreOrigin = new AssetOrigin();
			coreOrigin.name = "Vanilla Built-in Assets";
			coreOrigin.canResave = Application.isEditor; // Only allow re-saving vanilla in Unity. (reduce chance of asset conflicts for modders)
			assetOrigins.Add(coreOrigin);

			reloadOrigin = new AssetOrigin();
			reloadOrigin.name = "Reloaded Assets (Debug)";
			reloadOrigin.shouldAssetsOverrideExistingIds = true;
			assetOrigins.Add(reloadOrigin);

			legacyServerSharedOrigin = new AssetOrigin();
			legacyServerSharedOrigin.name = "Server Common (Legacy)";
			assetOrigins.Add(legacyServerSharedOrigin);

			legacyPerServerOrigin = new AssetOrigin();
			legacyPerServerOrigin.name = "Per-Server (Legacy)";
			assetOrigins.Add(legacyPerServerOrigin);

			yield return null;

#if !DEDICATED_SERVER
			ResourceHash.Initialize();
#endif // !DEDICATED_SERVER

			if (shouldLoadAnyAssets)
			{
				string coreBundlesPath = Path.Combine(ReadWrite.PATH, "Bundles");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
				if (!Directory.Exists(coreBundlesPath))
				{
					if (Provider.steamAppInstallDirectory != null)
					{
						coreBundlesPath = PathEx.Join(Provider.steamAppInstallDirectory, "Bundles");
					}
					else
					{
						// Missing Steam install error will have been shown.
						yield break;
					}
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
				AddSearchLocation(coreBundlesPath, coreOrigin);

				if (Dedicator.IsDedicatedServer)
				{
					AddDedicatedServerUgcSearchLocations();
				}
				else
				{
					AddClientUgcSearchLocations();
				}

				AddSandboxSearchLocations();
				AddMapSearchLocations();

				yield return null;

#if WITH_NOREDIST // Auto-sub slows down U3-SDK initial startup, skip.
				if (Dedicator.IsDedicatedServer == false)
				{
					// We trigger here because any earlier and fake installed callback may try loading before we are initialized.
					Provider.initAutoSubscribeMaps();
				}
#endif // WITH_NOREDIST

				yield return LoadAssetsFromWorkerThread();
			}

			LoadingUI.SetLoadingText("Loading_Blueprints");
			yield return null;

			if (shouldValidateAssets)
			{
				CheckForBlueprintErrors();
			}

			LoadingUI.SetLoadingText("Loading_Spawns");
			yield return null;

			if (!Dedicator.IsDedicatedServer)
			{
				linkSpawns();
			}

			if (shouldValidateAssets)
			{
				CheckForNpcErrors();
			}

			CleanupMemory();

			if (shouldResaveAssets && shouldParseMetadata)
			{
				ResaveAssets();
			}

			LoadingUI.SetLoadingText("Loading_Misc");
			yield return null;

			onAssetsRefreshed?.Invoke();

			yield return null;

			UnturnedLog.info($"Loading all assets took {Time.realtimeSinceStartupAsDouble - startTime}s");
			isLoadingAllAssets = false;
		}

		private IEnumerator StartupAssetLoading()
		{
			yield return LoadAllAssets();
			Debug.Assert(!hasFinishedInitialStartupLoading);
			hasFinishedInitialStartupLoading = true;

			if (shouldLoadAnyAssets && coreMasterBundle == null)
			{
				if (!Provider.WasQuitGameCalled) // Don't show a second dialog.
				{
					Provider.QuitGame("Missing core asset bundle. By default this is loaded from the Steam install.");
#if UNITY_EDITOR
					EditorUtility.DisplayDialog("Missing Core Assets", "If overriding Bundles please also include core asset bundle!", "OK");
					EditorApplication.ExitPlaymode();
#endif // UNITY_EDITOR
				}
				yield break;
			}

#if UNITY_EDITOR
			if (exportAssetsReport && !Dedicator.IsDedicatedServer)
			{
				EconAssetsReport.buildReport();
			}
#endif // UNITY_EDITOR

#if WITH_ASSET_CONSOLIDATION
			AssetConsolidation.WriteAssetFilePaths();
#endif // WITH_ASSET_CONSOLIDATION

			if (Dedicator.IsDedicatedServer)
			{
				Provider.host();
			}
			else
			{
				LoadingUI.SetLoadingText("Loading_MainMenu");
				yield return null;

				UnturnedLog.info("Launching main menu");
				UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
			}
		}

		private void ResaveAssets()
		{
			UnturnedLog.info("Re-saving assets!");
			DatWriter datWriter = new DatWriter();
			MetadataPreservingDatWriter metadataPreservingDatWriter = new MetadataPreservingDatWriter();

			foreach (Asset asset in defaultAssetMapping.assetList)
			{
				if (asset.OriginParsedData == null)
				{
					continue;
				}

				if (asset.HasErrors)
				{
					UnturnedLog.info($"Skipping re-saving asset {asset} because it loaded with errors and may lose data");
					continue;
				}

				try
				{
					asset.PreResaveAsset(asset.OriginParsedData);
					
					const bool append = false;
					using (StreamWriter fileStream = new StreamWriter(asset.absoluteOriginFilePath, append, System.Text.Encoding.UTF8))
					{
						datWriter.SetOutput(fileStream);
						metadataPreservingDatWriter.WriteRootDictionary(asset.OriginParsedData, datWriter);
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception re-saving asset {asset}");
				}
			}
		}

		private IEnumerator LoadNewAssetsFromUpdate()
		{
			isLoadingFromUpdate = true;
			double startTime = Time.realtimeSinceStartupAsDouble;

			yield return LoadAssetsFromWorkerThread();

			linkSpawnsIfDirty();

			CleanupMemory();

			UnturnedLog.info($"Loading new assets took {Time.realtimeSinceStartupAsDouble - startTime}s");
			isLoadingFromUpdate = false;

			OnNewAssetsFinishedLoading?.Invoke();
		}

		/// <summary>
		/// Not the tidiest place for this, but it allows startup to pause and show error message.
		/// Occasionally there have been reports of the steamclient redist files being out of date on the dedicated
		/// server causing problems. For example: https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2866#issuecomment-965945985
		/// </summary>
		private bool TestDedicatedServerSteamRedist()
		{
			string filePath;
#if UNITY_EDITOR
			// see: SteamCmdUtils.GetSteamAppsPath
			filePath = PathEx.Join(UnityPaths.LibraryDirectory, "SteamCmd/steamapps/content/app_1007/depot_1004/steamclient64.dll");
#elif UNITY_STANDALONE_LINUX
			filePath = PathEx.Join(UnityPaths.GameDirectory, "linux64", "steamclient.so");
#else // !UNITY_STANDALONE_LINUX
			filePath = PathEx.Join(UnityPaths.GameDirectory, "steamclient64.dll");
#endif // !UNITY_STANDALONE_LINUX

			if (!File.Exists(filePath))
			{
				CommandWindow.LogError($"Missing steamclient redist file at: {filePath}");
#if UNITY_EDITOR
				UnturnedLog.error("HOW TO UPDATE DEDICATED SERVER REDIST FILE IN UNITY:");
				UnturnedLog.error("Window > Unturned > Build Tool > Update Steam Dedicated Server SDK");
				UnityEditor.EditorApplication.ExitPlaymode();
#endif // UNITY_EDITOR
				return false;
			}

			try
			{
				FileInfo fileInfo = new FileInfo(filePath);

				// Windows: https://steamdb.info/depot/1004/history/
				// Linux: https://steamdb.info/depot/1006/history/
				DateTime expectedLastWriteTime = new DateTime(2021, 09, 14, /*hour*/ 21, /*minute*/ 30, /*second*/ 00, DateTimeKind.Utc);
				if (fileInfo.LastWriteTimeUtc >= expectedLastWriteTime)
				{
					return true;
				}
				else
				{
					CommandWindow.LogError($"Out-of-date steamclient redist file (expected: {expectedLastWriteTime} actual: {fileInfo.LastWriteTimeUtc})");
					return false;
				}
			}
			catch (System.Exception ex)
			{
				UnturnedLog.exception(ex, "Unable to get steamclient redist file info");
				return false;
			}
		}

		private void Start()
		{
			if (Dedicator.IsDedicatedServer)
			{
				Framework.Modules.Module rocketModule = Framework.Modules.ModuleHook.getModuleByName("Rocket.Unturned");
				if (rocketModule != null) // Module will be null if Rocket.Unturned is not installed.
				{
					string maintainedVersionString = "4.9.3.1";
					uint maintainedVersionInt = Parser.getUInt32FromIP(maintainedVersionString);
					if (rocketModule.config.Version_Internal < maintainedVersionInt)
					{
						CommandWindow.LogError("Upgrading to the officially maintained version of Rocket, or a custom fork of it, is required.");
						CommandWindow.LogErrorFormat("Installed version: {0} Maintained version: 4.9.3.3+", rocketModule.config.Version);
						CommandWindow.Log(string.Empty);
						CommandWindow.Log("--- Overview ---");
						CommandWindow.Log(string.Empty);
						CommandWindow.Log("SDG maintains a fork of Rocket called the Legally Distinct Missile (or LDM) after the resignation of its original community team. Using this fork is important because it preserves compatibility, and has fixes for important legacy Rocket issues like multithreading exceptions and teleportation exploits.");
						CommandWindow.Log(string.Empty);
						CommandWindow.Log("--- Installation ---");
						CommandWindow.Log(string.Empty);
						CommandWindow.Log("The dedicated server includes the latest version, so an external download is not necessary:");
						CommandWindow.Log("1. Copy the Rocket.Unturned module from the game's Extras directory.");
						CommandWindow.Log("2. Paste it into the game's Modules directory.");
						CommandWindow.Log(string.Empty);
						CommandWindow.Log("--- Resources ---");
						CommandWindow.Log(string.Empty);
						CommandWindow.Log("https://github.com/SmartlyDressedGames/Legally-Distinct-Missile");
						CommandWindow.Log("https://www.reddit.com/r/UnturnedLDM/");
						CommandWindow.Log("https://steamcommunity.com/app/304930/discussions/17/");

						return; // Abort startup.
					}
				}

				// Test builds can still host dedicated server for convenience.
#if !DEDICATED_SERVER && !UNITY_EDITOR && !DEVELOPMENT_BUILD
				CommandWindow.LogError("Hosting dedicated servers using client files has been deprecated since June 2019.");
				CommandWindow.Log("Please use the standalone dedicated server app ID 1110390 available through SteamCMD instead.");
				CommandWindow.Log("For more information and an installation guide read more at:");
				CommandWindow.Log("https://docs.smartlydressedgames.com/en/stable/servers/server-hosting.html");
				return; // Abort startup.
#endif // !DEDICATED_SERVER && !UNITY_EDITOR && !DEVELOPMENT_BUILD

				// AFTER the client-as-server error, whoops the initial Steam redist error update was confusing.
				if (!TestDedicatedServerSteamRedist())
				{
					return; // Abort startup.
				}
			}

			worker = new AssetsWorker();
			worker.Initialize();

			StartCoroutine(StartupAssetLoading());
		}

		private void Awake()
		{
			instance = this;
		}

		private void Update()
		{
			worker.Update();

			if (!isLoading && worker.IsWorking)
			{
				StartCoroutine(LoadNewAssetsFromUpdate());
			}
		}

		private void OnDestroy()
		{
			worker.Shutdown();
		}

#if UNITY_EDITOR
		private void OnApplicationQuit()
		{
			if (errors == null)
			{
				return;
			}

			UnityEditor.EditorPrefs.SetInt("Asset_Errors", errors.Count);
		}
#endif

		internal static readonly ClientStaticMethod<System.Guid> SendKickForInvalidGuid = ClientStaticMethod<System.Guid>.Get(ReceiveKickForInvalidGuid);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveKickForInvalidGuid(System.Guid guid)
		{
			Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CUSTOM;

			Asset asset = find(guid);
			if (asset != null)
			{
				string message = $"Server missing asset: \"{asset.FriendlyName}\" File: \"{asset.name}\" Id: {guid:N}";
				message += $"\nFile path: \"{asset.absoluteOriginFilePath}\"";
				message += $"\nClient asset is from {asset.GetOriginName()}.";
				Provider._connectionFailureReason = message;
			}
			else
			{
				string message = "Client and server are both missing unknown asset! ID: " + guid.ToString("N");
				message += "\nThis probably means either an invalid ID was sent by the server,";
				message += "\nthe ID got corrupted for example by plugins modifying network traffic,";
				message += "\nor a required level asset like materials/foliage/trees/objects is missing.";
				Provider._connectionFailureReason = message;
			}

			Provider.RequestDisconnect($"Kicked for sending invalid asset guid: {guid:N}");
		}

		internal static readonly ClientStaticMethod<System.Guid, string, string, byte[], string, string> SendKickForHashMismatch = ClientStaticMethod<System.Guid, string, string, byte[], string, string>.Get(ReceiveKickForHashMismatch);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveKickForHashMismatch(System.Guid guid, string serverName, string serverFriendlyName, byte[] serverHash, string serverAssetBundleNameWithoutExtension, string serverAssetOrigin)
		{
			bool shouldVerifyGameFiles;
			
			Asset asset = find(guid);
			if (asset != null)
			{
				string clientAssetOrigin = asset.origin?.name;
				if (string.IsNullOrEmpty(clientAssetOrigin))
				{
					clientAssetOrigin = "Unknown";
				}

				string message;

				if (string.Equals(asset.name, serverName) && string.Equals(asset.FriendlyName, serverFriendlyName))
				{
					if (!string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle != null && !string.Equals(asset.originMasterBundle.assetBundleNameWithoutExtension, serverAssetBundleNameWithoutExtension))
					{
						// Client and server both have .masterbundle but name does not match.
						message = $"Client and server loaded \"{serverFriendlyName}\" from different asset bundles! (File: \"{asset.name}\" ID: {guid:N})";
						message += $"\nClient asset bundle is \"{asset.originMasterBundle.assetBundleNameWithoutExtension}\", whereas server asset bundle is \"{serverAssetBundleNameWithoutExtension}\".";
						shouldVerifyGameFiles = true;
					}
					else if (!string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle == null)
					{
						// Server asset has .masterbundle but client has .unity3d.
						message = $"Client loaded \"{serverFriendlyName}\" from legacy asset bundle but server did not! (File: \"{asset.name}\" ID: {guid:N})";
						message += $"\nServer asset bundle name: \"{serverAssetBundleNameWithoutExtension}\".";
						shouldVerifyGameFiles = true;
					}
					else if (string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle != null)
					{
						// Server asset has .unity3d but client has .masterbundle.
						message = $"Server loaded \"{serverFriendlyName}\" from legacy asset bundle but client did not! (File: \"{asset.name}\" ID: {guid:N})";
						message += $"\nClient asset bundle name: \"{asset.originMasterBundle.assetBundleNameWithoutExtension}\"";
						shouldVerifyGameFiles = true;
					}
					else if (Hash.verifyHash(asset.hash, serverHash))
					{
						// Client hash is equal to server hash, what?! There is a good reason for this so bear with me:
						// asset.hash is only combined with assetbundle hash if client detects the multiplatform "*.hash" file used by the
						// server, so if the hash used by the server is equal to asset.hash (not the same as hash sent by client) that means
						// the "*.hash" file was not used on the server, maybe because it was out of date.
						message = $"Server asset bundle hash out of date for \"{serverFriendlyName}\"! (File: \"{asset.name}\" ID: {guid:N})";
						message += $"\nThis probably means the mod creator should re-export the \"{serverAssetBundleNameWithoutExtension}\" asset bundle.";
						shouldVerifyGameFiles = false;
					}
					else
					{
						// Exactly same name, probably same asset.
						// https://support.smartlydressedgames.com/hc/en-us/articles/20501325473812
						message = $"Client and server disagree on asset \"{asset.FriendlyName}\" configuration. (File: \"{asset.name}\" ID: {guid:N})";
						message += $"\nUsually this means the files are different versions in which case updating the client and server might fix it.";
						message += $"\nAlternatively the file may have been corrupted, locally modified, or modified on the server.";
						message += $"\nClient hash is {Hash.toString(asset.hash)}, whereas server hash is {Hash.toString(serverHash)}.";
						shouldVerifyGameFiles = true;
					}
				}
				else
				{
					// Different names, so maybe different asset with same id?
					message = $"Client and server have different assets with the same ID! ({guid:N})";
					message += "\nThis probably means an existing file was copied, but the mod creator can fix it by changing the ID.";

					if (string.Equals(asset.FriendlyName, serverFriendlyName))
					{
						message += $"\nDisplay name \"{serverFriendlyName}\" matches between client and server.";
					}
					else
					{
						message += $"\nClient display name is \"{asset.FriendlyName}\", whereas server display name is \"{serverFriendlyName}\".";
					}

					if (string.Equals(asset.name, serverName))
					{
						message += $"\nFile name \"{asset.name}\" matches between client and server.";
					}
					else
					{
						message += $"\nClient file name is \"{asset.name}\", whereas server file name is \"{serverName}\".";
					}

					shouldVerifyGameFiles = true;
				}

				if (string.Equals(clientAssetOrigin, serverAssetOrigin))
				{
					message += $"\nClient and server agree this asset is from {clientAssetOrigin}.";
				}
				else
				{
					message += $"\nClient asset is from {clientAssetOrigin}, whereas server asset is from {serverAssetOrigin}.";
				}

				Provider._connectionFailureReason = message;
			}
			else
			{
				Provider._connectionFailureReason = $"Unknown asset hash mismatch? (should never happen) Name: \"{serverFriendlyName}\" File: \"{serverName}\" Id: {guid:N}";
				shouldVerifyGameFiles = true;
			}

			Provider._connectionFailureInfo = shouldVerifyGameFiles ?
				ESteamConnectionFailureInfo.CUSTOM_SHOULD_VERIFY_GAME_FILES : ESteamConnectionFailureInfo.CUSTOM;

			Provider.RequestDisconnect($"Kicked for asset hash mismatch guid: {guid:N} serverName: \"{serverName}\" serverFriendlyName: \"{serverFriendlyName}\" serverHash: {Hash.toString(serverHash)} serverAssetBundleName: \"{serverAssetBundleNameWithoutExtension}\" serverAssetOrigin: \"{serverAssetOrigin}\"");
		}

		internal static AssetLoadingStats loadingStats = new AssetLoadingStats();

		private AssetsWorker worker;

#if WITH_ASSETS_PROFILING
		private static CustomSampler loadFileSampler = CustomSampler.Create("LoadFile");
		private static CustomSampler createInstance = CustomSampler.Create("Activator.CreateInstance");
		private static CustomSampler populateAsset = CustomSampler.Create("PopulateAsset");
#endif // WITH_ASSETS_PROFILING

		#region DEPRECATED
		[System.Obsolete("Renamed to RequestAddSearchLocation")]
		public static void load(string absoluteDirectoryPath, AssetOrigin origin, bool overrideExistingIDs)
		{
			RequestAddSearchLocation(absoluteDirectoryPath, origin);
		}

		[System.Obsolete("Renamed to RequestReloadAllAssets")]
		public static void refresh()
		{
			RequestReloadAllAssets();
		}

		[System.Obsolete]
		public static void rename(Asset asset, string newName)
		{

		}

		[System.Obsolete]
		public static AssetOrigin ConvertLegacyOrigin(EAssetOrigin legacyOrigin)
		{
			if (legacyOrigin == EAssetOrigin.OFFICIAL)
			{
				if (legacyOfficialOrigin == null)
				{
					legacyOfficialOrigin = new AssetOrigin();
					legacyOfficialOrigin.name = "Official (Legacy)";
					assetOrigins.Add(legacyOfficialOrigin);
				}

				return legacyOfficialOrigin;
			}
			else if (legacyOrigin == EAssetOrigin.MISC)
			{
				if (legacyMiscOrigin == null)
				{
					legacyMiscOrigin = new AssetOrigin();
					legacyMiscOrigin.name = "Misc (Legacy)";
					assetOrigins.Add(legacyMiscOrigin);
				}

				return legacyMiscOrigin;
			}
			else
			{
				if (legacyWorkshopOrigin == null)
				{
					legacyWorkshopOrigin = new AssetOrigin();
					legacyWorkshopOrigin.name = "Workshop File (Legacy)";
					assetOrigins.Add(legacyWorkshopOrigin);
				}

				return legacyWorkshopOrigin;
			}
		}

		// These will be initialized by ConvertLegacyOrigin if they are actually needed.
		internal static AssetOrigin legacyOfficialOrigin;
		internal static AssetOrigin legacyMiscOrigin;
		internal static AssetOrigin legacyWorkshopOrigin;

		[System.Obsolete]
		public static Asset find(EAssetType type, string name)
		{
			return null;
		}

		[System.Obsolete]
		public static void add(Asset asset, bool overrideExistingID)
		{
			AddToMapping(asset, overrideExistingID, defaultAssetMapping);
		}

		[System.Obsolete]
		public static void load(string path, bool usePath, bool loadFromResources, bool canUse, EAssetOrigin origin, bool overrideExistingIDs)
		{
			load(path, usePath, loadFromResources, canUse, origin, overrideExistingIDs, 0);
		}

		[System.Obsolete("Remove unused loadFromResources which was used by vanilla assets before masterbundles, and canUse which was for timed curated maps.")]
		public static void load(string path, bool usePath, bool loadFromResources, bool canUse, EAssetOrigin origin, bool overrideExistingIDs, ulong workshopFileId)
		{
			load(path, usePath, origin, overrideExistingIDs, workshopFileId);
		}

		[System.Obsolete("Replaced origin enum with class")]
		public static void load(string path, bool usePath, EAssetOrigin legacyOrigin, bool overrideExistingIDs, ulong workshopFileId)
		{
			if (usePath)
			{
				path = ReadWrite.PATH + path;
			}

			AssetOrigin origin = ConvertLegacyOrigin(legacyOrigin);
			load(path, origin, overrideExistingIDs);
		}

		[System.Obsolete("Please use the method which takes a List instead.")]
		public static Asset[] find(EAssetType type)
		{
			if (type == EAssetType.NONE)
			{
				return null;
			}

			if (type == EAssetType.OBJECT)
			{
				throw new System.NotSupportedException();
			}
			else
			{
				Asset[] found = new Asset[currentAssetMapping.legacyAssetsTable[type].Values.Count];

				int index = 0;
				foreach (KeyValuePair<ushort, Asset> asset in currentAssetMapping.legacyAssetsTable[type])
				{
					found[index] = asset.Value;
					index++;
				}

				return found;
			}
		}

		[System.Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
		public static void reportError(Asset offendingAsset, string error)
		{
			reportError(offendingAsset, error);
		}

		[System.Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
		public static void reportError(Asset offendingAsset, string format, params object[] args)
		{
			string error = string.Format(format, args);
			reportError(offendingAsset, error);
		}

		[System.Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
		public static void reportError(Asset offendingAsset, string format, object arg0)
		{
			string error = string.Format(format, arg0);
			reportError(offendingAsset, error);
		}

		[System.Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
		public static void reportError(Asset offendingAsset, string format, object arg0, object arg1)
		{
			string error = string.Format(format, arg0, arg1);
			reportError(offendingAsset, error);
		}

		[System.Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
		public static void reportError(Asset offendingAsset, string format, object arg0, object arg1, object arg2)
		{
			string error = string.Format(format, arg0, arg1, arg2);
			reportError(offendingAsset, error);
		}
		#endregion // DEPRECATED
	}
}
