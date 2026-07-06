////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;


namespace SDG.Unturned
{
	public delegate void PrePreLevelLoaded(int level);
	public delegate void PreLevelLoaded(int level);
	public delegate void LevelLoaded(int level);
	public delegate void PostLevelLoaded(int level);
	public delegate void LevelsRefreshed();
	public delegate void LevelLoadingStepHandler();
	public delegate void LevelExited();

	public class Level : MonoBehaviour
	{
		private const float STEPS = 19f;

		/// <summary>
		/// The main/entry scene was originally called "Setup" in the same folder as other scenes
		/// but has been moved to the project root and renamed "GameStartup" to be easier to find.
		/// </summary>
		public static readonly int BUILD_INDEX_SETUP = 0;
		public static readonly int BUILD_INDEX_MENU = 1;
		public static readonly int BUILD_INDEX_GAME = 2;
		public static readonly int BUILD_INDEX_LOADING = 3;

		public static readonly float HEIGHT = 1024f;
		public static readonly float TERRAIN = 256f;
		public static readonly ushort CLIP = 8;

		public static readonly ushort TINY_BORDER = 16;
		public static readonly ushort SMALL_BORDER = 64;
		public static readonly ushort MEDIUM_BORDER = 64;
		public static readonly ushort LARGE_BORDER = 64;
		public static readonly ushort INSANE_BORDER = 128;

		public static readonly ushort TINY_SIZE = 512;
		public static readonly ushort SMALL_SIZE = 1024;
		public static readonly ushort MEDIUM_SIZE = 2048;
		public static readonly ushort LARGE_SIZE = 4096;
		public static readonly ushort INSANE_SIZE = 8192;

		public static ushort border
		{
			get
			{
				if (info == null)
				{
					return 1;
				}
				else if (info.size == ELevelSize.TINY)
				{
					return TINY_BORDER;
				}
				else if (info.size == ELevelSize.SMALL)
				{
					return SMALL_BORDER;
				}
				else if (info.size == ELevelSize.MEDIUM)
				{
					return MEDIUM_BORDER;
				}
				else if (info.size == ELevelSize.LARGE)
				{
					return LARGE_BORDER;
				}
				else if (info.size == ELevelSize.INSANE)
				{
					return INSANE_BORDER;
				}

				return 0;
			}
		}

		public static ushort size
		{
			get
			{
				if (info == null)
				{
					return 8;
				}
				else if (info.size == ELevelSize.TINY)
				{
					return TINY_SIZE;
				}
				else if (info.size == ELevelSize.SMALL)
				{
					return SMALL_SIZE;
				}
				else if (info.size == ELevelSize.MEDIUM)
				{
					return MEDIUM_SIZE;
				}
				else if (info.size == ELevelSize.LARGE)
				{
					return LARGE_SIZE;
				}
				else if (info.size == ELevelSize.INSANE)
				{
					return INSANE_SIZE;
				}

				return 0;
			}
		}

		/// <summary>
		/// Is a point safely within the level bounds?
		/// Also checks player clip volumes if legacy borders are disabled.
		/// </summary>
		public static bool checkSafeIncludingClipVolumes(Vector3 point)
		{
			if (info != null && !info.configData.Use_Legacy_Clip_Borders)
			{
				return !SDG.Framework.Devkit.PlayerClipVolumeManager.Get().IsPositionInsideAnyVolume(point);
			}

			if (!isPointWithinValidHeight(point.y))
			{
				// Outside vertical bounds.
				return false;
			}

			// Absolute value of point so that we don't need to check symmetrical - and +.
			Vector3 absPoint = new Vector3(Mathf.Abs(point.x), point.y, Mathf.Abs(point.z));

			if (absPoint.x > (size / 2) - border || absPoint.z > (size / 2) - border)
			{
				// Outside square bounds.
				return false;
			}

			return true;
		}

		/// <summary>
		/// Is given Y (vertical) coordinate within level's height range?
		/// Maps using landscapes have a larger range than older maps.
		/// </summary>
		public static bool isPointWithinValidHeight(float y)
		{
			return y >= -1024 && y <= 1024;
		}

		[System.Obsolete("Replaced by checkSafeIncludingClipVolumes or the newer isPointWithinValidHeight")]
		public static bool checkLevel(Vector3 point)
		{
			return checkSafeIncludingClipVolumes(point);
		}

		public static readonly byte SAVEDATA_VERSION = 2;

		public static PrePreLevelLoaded onPrePreLevelLoaded;
		public static PreLevelLoaded onPreLevelLoaded;
		public static LevelLoaded onLevelLoaded;
		public static PostLevelLoaded onPostLevelLoaded;
		public static LevelsRefreshed onLevelsRefreshed;
		public static event LevelLoadingStepHandler loadingSteps;
		public static LevelExited onLevelExited;

		/// <summary>
		/// Notify menus that levels list has changed.
		/// Used when creating/deleting levels, as well as following workshop changes.
		/// </summary>
		public static void broadcastLevelsRefreshed()
		{
			onLevelsRefreshed?.Invoke();
		}

		private static LevelInfo _info;
		public static LevelInfo info => _info;

		private static LevelAsset cachedLevelAsset;
		private static bool didResolveLevelAsset;

		/// <summary>
		/// Initialized along with level asset.
		/// </summary>
		private static CachingAssetRef[] cachedStaticTags;

		public static bool IsCraftingAllowedByLevel
		{
			get => _info?.configData?.Allow_Crafting ?? true;
		}

		private static void ResetCachedLevelAsset()
		{
			cachedLevelAsset = null;
			didResolveLevelAsset = false;
			cachedStaticTags = null;
		}

		public static bool IsTagEnabled(TagAsset tag)
		{
			if (cachedStaticTags == null || cachedStaticTags.Length < 1)
			{
				return false;
			}

			for (int tagIndex = 0; tagIndex < cachedStaticTags.Length; ++tagIndex)
			{
				ref CachingAssetRef tagRef = ref cachedStaticTags[tagIndex];
				if (tagRef.Get() == tag)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get level's cached asset, if any.
		/// </summary>
		public static LevelAsset getAsset()
		{
			if (!didResolveLevelAsset)
			{
				// Prevent spamming "unable to find" error if missing.
				didResolveLevelAsset = true;

				if (info != null && info.configData != null && info.configData.Asset.isValid)
				{
					cachedLevelAsset = Assets.find(info.configData.Asset);
					if (cachedLevelAsset == null)
					{
						UnturnedLog.warn("Unable to find level asset {0} for {1}", info.configData.Asset, info.name);
					}
					else
					{
						//UnturnedLog.info($"Found level asset {cachedLevelAsset.name} for {info.name}");
					}
				}

				if (cachedLevelAsset == null)
				{
					cachedLevelAsset = Assets.find(LevelAsset.defaultLevel);
					if (cachedLevelAsset == null)
					{
						UnturnedLog.error("Unable to find default level asset");
					}
				}

				List<CachingAssetRef> pendingTags = new List<CachingAssetRef>();

				if (cachedLevelAsset != null && cachedLevelAsset.Tags != null)
				{
					pendingTags.AddRange(cachedLevelAsset.Tags);
				}

				if (Provider.isServer && !Dedicator.IsDedicatedServer)
				{
					pendingTags.Add(CachingAssetRef.Parse("d7bd989414644b19b3299be0c6fab5f0")); // Singleplayer
					if (cachedLevelAsset != null && cachedLevelAsset.ShouldAllowBuildingInSafezonesInSingleplayer)
					{
						pendingTags.Add(CachingAssetRef.Parse("73eb818d1aa044c7bb4e61b8f9b37a3c")); // BuildingInSafezonesAllowed
					}
				}
				else
				{
					pendingTags.Add(CachingAssetRef.Parse("f663677b88de40ec80ff36b0c1cae544")); // NotSingleplayer
				}
				cachedStaticTags = pendingTags.ToArray();
			}

			return cachedLevelAsset;
		}

		private static void updateCachedHolidayRedirects()
		{
			shouldUseHolidayRedirects = isEditor == false
				&& info != null
				&& info.configData != null
				&& info.configData.Allow_Holiday_Redirects
				&& HolidayUtil.getActiveHoliday() != ENPCHoliday.NONE;
		}

		private static void UpdateShouldUseLevelBatching()
		{
#if !DEDICATED_SERVER
			bool canEnable = clUseLevelBatching.hasValue ? clUseLevelBatching.value : true;

			shouldUseLevelBatching = canEnable
				&& !isEditor
				&& !Dedicator.IsDedicatedServer
				&& info != null
				&& info.configData != null
				&& info.configData.Batching_Version > 1;

			ShouldSkipInstantiatingClutter = !isEditor
				&& !Dedicator.IsDedicatedServer
				&& !GraphicsSettings.IsClutterEnabled
				&& info != null
				&& info.configData != null
				&& info.configData.Enable_Clutter_Option;
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Should loading code proceed with redirects?
		/// Disabled by level and when in the editor.
		/// </summary>
		public static bool shouldUseHolidayRedirects
		{
			get;
			private set;
		}

#if DEDICATED_SERVER
		public const bool shouldUseLevelBatching = false;
		public const bool ShouldSkipInstantiatingClutter = false;
#else // !DEDICATED_SERVER
		public static bool shouldUseLevelBatching
		{
			get;
			private set;
		}

		public static bool ShouldSkipInstantiatingClutter
		{
			get;
			private set;
		}
#endif // !DEDICATED_SERVER

		private static GameObject satelliteCaptureGameObject;
		private static Transform satelliteCaptureTransform;
		private static Camera satelliteCaptureCamera;

		private static Transform _level;
		public static Transform level => _level;

		private static Transform _roots;
		public static Transform roots => _roots;

		private static Transform _clips;
		public static Transform clips => _clips;

		private static Transform _effects;
		[System.Obsolete("Was the parent of all effects in the past, but now empty for TransformHierarchy performance.")]
		public static Transform effects
		{
			get
			{
				if (_effects == null)
				{
					_effects = new GameObject().transform;
					_effects.name = "Effects";
					_effects.parent = level;
					_effects.tag = "Logic";
					_effects.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing Level.effecs which has been deprecated.");
				}

				return _effects;
			}
		}

		private static Transform _spawns;
		[System.Obsolete("Was the parent of gameplay objects in the past, but now empty for TransformHierarchy performance.")]
		public static Transform spawns
		{
			get
			{
				if (_spawns == null)
				{
					_spawns = new GameObject().transform;
					_spawns.name = "Spawns";
					_spawns.parent = level;
					_spawns.tag = "Logic";
					_spawns.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing Level.spawns which has been deprecated.");
				}

				return _spawns;
			}
		}

		private static Transform _editing;
		public static Transform editing => _editing;

		private static Level instance;

		/// <summary>
		/// Placeholder created between unloading the main menu and loading into game or editor.
		/// </summary>
		internal static AudioListener placeholderAudioListener;

#if !DEDICATED_SERVER
		/// <summary>
		/// Loading screen music.
		/// </summary>
		private static AudioSource musicAudioSource;

		/// <summary>
		/// Clip to play to fade out loop.
		/// </summary>
		private static AudioClip musicOutroClip;

		private static float musicOutroVolume;
#endif // !DEDICATED_SERVER

		private static bool _isInitialized;
		public static bool isInitialized => _isInitialized;

		private static bool _isEditor;
		public static bool isEditor => _isEditor;

		public static bool isExiting
		{
			get;
			protected set;
		}

		public static bool isLoadingContent = true;
		public static bool isLoadingLighting = true;
		public static bool isLoadingVehicles = true;
		public static bool isLoadingBarricades = true;
		public static bool isLoadingStructures = true;
		public static bool isLoadingArea = true;
		public static bool isLoading
		{
			get
			{
				//UnturnedLog.info(isLoadingContent + " " + isLoadingLighting + " " + isLoadingVehicles + " " + isLoadingBarricades + " " + isLoadingStructures + " " + isLoadingArea);
				if (Provider.isConnected)
				{
					return isLoadingContent || isLoadingLighting || isLoadingVehicles || isLoadingBarricades || isLoadingStructures || isLoadingArea;
				}
				else if (isEditor)
				{
					return isLoadingContent;
				}
				else
				{
					return false;
				}
			}
		}

		private static bool _isLoaded;
		public static bool isLoaded => _isLoaded;

		public static byte[] hash
		{
			get;
			private set;
		}

		private static List<byte[]> pendingHashes;

		/// <summary>
		/// Useful to narrow down why a player is getting kicked for modified level files when joining a server.
		/// </summary>
		private static CommandLineFlag shouldLogLevelHash = new CommandLineFlag(false, "-LogLevelHash");

		public static void includeHash(string id, byte[] pendingHash)
		{
			if (shouldLogLevelHash)
			{
				UnturnedLog.info($"[{pendingHashes.Count}] Including \"{id}\" in level hash: {Hash.toString(pendingHash)}");
			}

			if (pendingHash == null)
			{
				UnturnedLog.error($"\"{id}\" added null to level hash!");
				return;
			}

			pendingHashes.Add(pendingHash);
		}

		private static void combineHashes()
		{
			hash = Hash.combine(pendingHashes);

			if (shouldLogLevelHash)
			{
				UnturnedLog.info($"Combined level hash: {Hash.toString(hash)}");
			}
		}

		/// <summary>
		/// Display version string of the currently loaded level.
		/// </summary>
		public static string version => info != null && info.configData != null ? info.configData.Version : "0.0.0.0";

		/// <summary>
		/// Version string of the currently loaded level packed into an integer.
		/// </summary>
		public static uint packedVersion => info != null && info.configData != null ? info.configData.PackedVersion : 0;

		public static void setEnabled(bool isEnabled)
		{
			clips.gameObject.SetActive(isEnabled);
		}

		public static void add(string name, ELevelSize size, ELevelType type)
		{
			if (!ReadWrite.folderExists("/Maps/" + name))
			{
				ReadWrite.createFolder("/Maps/" + name);

				Block block = new Block();
				block.writeByte(SAVEDATA_VERSION);

				block.writeSteamID(Provider.client);
				block.writeByte((byte) size);
				block.writeByte((byte) type);

				ReadWrite.writeBlock("/Maps/" + name + "/Level.dat", false, block);

				string templateSrc = Path.Join(ReadWrite.PATH, "Extras", "LevelTemplate");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
				if (!Directory.Exists(templateSrc) && Provider.steamAppInstallDirectory != null)
				{
					templateSrc = PathEx.Join(Provider.steamAppInstallDirectory, "Extras", "LevelTemplate");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST

				string templateDst = Path.Join(ReadWrite.PATH, "Maps", name);
				File.Copy(Path.Join(templateSrc, "Charts.unity3d"), Path.Join(templateDst, "Charts.unity3d"));

				string terrainDir = Path.Join(templateDst, "Terrain");
				Directory.CreateDirectory(terrainDir);
				File.Copy(Path.Join(templateSrc, "Details.unity3d"), Path.Join(terrainDir, "Details.unity3d"));
				File.Copy(Path.Join(templateSrc, "Details.dat"), Path.Join(terrainDir, "Details.dat"));
				File.Copy(Path.Join(templateSrc, "Materials.unity3d"), Path.Join(terrainDir, "Materials.unity3d"));
				File.Copy(Path.Join(templateSrc, "Materials.dat"), Path.Join(terrainDir, "Materials.dat"));
				File.Copy(Path.Join(templateSrc, "Resources.dat"), Path.Join(terrainDir, "Resources.dat"));

				string environmentDir = Path.Join(templateDst, "Environment");
				Directory.CreateDirectory(environmentDir);
				File.Copy(Path.Join(templateSrc, "Lighting.dat"), Path.Join(environmentDir, "Lighting.dat"));
				File.Copy(Path.Join(templateSrc, "Roads.unity3d"), Path.Join(environmentDir, "Roads.unity3d"));
				File.Copy(Path.Join(templateSrc, "Roads.dat"), Path.Join(environmentDir, "Roads.dat"));
				File.Copy(Path.Join(templateSrc, "Ambience.unity3d"), Path.Join(environmentDir, "Ambience.unity3d"));

				broadcastLevelsRefreshed();
			}
		}

		[System.Obsolete]
		public static void remove(string name)
		{
			ReadWrite.deleteFolder("/Maps/" + name);

			broadcastLevelsRefreshed();
		}

		public static void Remove(LevelInfo level)
		{
			if (level.isFromWorkshop)
			{
				return;
			}

			ReadWrite.deleteFolder(level.path, false);

			broadcastLevelsRefreshed();
		}

		public static void save()
		{
			SDG.Framework.Devkit.DirtyManager.save();

			LevelObjects.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved objects.");
#endif

			LevelLighting.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved lighting.");
#endif

			LevelGround.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved ground.");
#endif

			LevelRoads.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved road.");
#endif

			LevelNavigation.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved navigation.");
#endif

			LevelNodes.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved nodes.");
#endif

			LevelItems.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved items.");
#endif

			LevelPlayers.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved players.");
#endif

			LevelZombies.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved zombies.");
#endif

			LevelVehicles.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved vehicles.");
#endif

			LevelAnimals.save();

#if EDITORDEBUG
			UnturnedLog.info("Saved animals.");
#endif

			LevelVisibility.save();

#if EDITORDEBUG
				UnturnedLog.info("Saved visibility.");
#endif

			Editor.save();
#if EDITORDEBUG
			UnturnedLog.info("Saved editor.");
#endif
		}

		public static void edit(LevelInfo newInfo)
		{
			if (newInfo == null)
			{
				return;
			}

			_isEditor = true;
			isExiting = false;

			_info = newInfo;
			ResetCachedLevelAsset();
			getAsset();
			LoadingUI.updateScene();
			UnityEngine.SceneManagement.SceneManager.LoadScene("Game");

#if !DEDICATED_SERVER
			PlayLevelLoadingScreenMusic();
#endif // !DEDICATED_SERVER

			// Not ideal, but we reset NetIds here to prevent errors when loading editor after playing.
			Provider.resetChannels();

			Provider.updateRichPresence();

			SDG.Framework.Devkit.Transactions.DevkitTransactionManager.resetTransactions();
			updateCachedHolidayRedirects();
			UpdateShouldUseLevelBatching();
		}

		public static void load(LevelInfo newInfo, bool hasAuthority)
		{
			_isEditor = false;
			isExiting = false;

			_info = newInfo;
			ResetCachedLevelAsset();
			getAsset();
			LoadingUI.updateScene();
			UnityEngine.SceneManagement.SceneManager.LoadScene("Game");

#if !DEDICATED_SERVER
			PlayLevelLoadingScreenMusic();
#endif // !DEDICATED_SERVER

			if (!Dedicator.IsDedicatedServer)
			{
				// TODO: this shouldn't be a big if/else chain.
				string achievementName = null;
				if (string.Equals(info.name, "A6 Polaris", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Frost_Visited";
				}
				else if (string.Equals(info.name, "arid", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Arid_Visited";
				}
				else if (string.Equals(info.name, "buak", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Buak_Visited";
				}
				else if (string.Equals(info.name, "california", System.StringComparison.InvariantCultureIgnoreCase) || string.Equals(info.name, "california2", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Cali2_Visited";
				}
				else if (string.Equals(info.name, "elver", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Elver_Visited";
				}
				else if (string.Equals(info.name, "limestone", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "LS_Visited";
				}
				else if (string.Equals(info.name, "germany", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Peaks";
				}
				else if (string.Equals(info.name, "hawaii", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Hawaii";
				}
				else if (string.Equals(info.name, "ireland", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Ireland_Visited";
				}
				else if (string.Equals(info.name, "kuwait", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Kuwait_Visited";
				}
				else if (string.Equals(info.name, "Escalation", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "PBS_Visited";
				}
				else if (string.Equals(info.name, "pei", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "PEI";
				}
				else if (string.Equals(info.name, "russia", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Russia";
				}
				else if (string.Equals(info.name, "rio de janeiro remastered", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "RioRemastered_Visited";
				}
				else if (string.Equals(info.name, "washington", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Washington";
				}
				else if (string.Equals(info.name, "yukon", System.StringComparison.InvariantCultureIgnoreCase))
				{
					achievementName = "Yukon";
				}

				if (!string.IsNullOrEmpty(achievementName))
				{
					Provider.provider.achievementsService.setAchievement(achievementName);
				}
			}

			if (hasAuthority)
			{
				string oldCyprusPath = LevelSavedata.transformName("Cyrpus Survival");
				string newCyprusPath = LevelSavedata.transformName("Cyprus Survival");
				//UnturnedLog.info("old cyprus " + oldCyprusPath + " " + ReadWrite.folderExists(oldCyprusPath));
				//UnturnedLog.info("new cyprus " + newCyprusPath + " " + ReadWrite.folderExists(newCyprusPath));
				if (ReadWrite.folderExists(oldCyprusPath) && !ReadWrite.folderExists(newCyprusPath))
				{
					ReadWrite.moveFolder(oldCyprusPath, newCyprusPath);
					UnturnedLog.info("Moved Cyprus save folder");
				}
			}

			Provider.updateRichPresence();

			SDG.Framework.Devkit.Transactions.DevkitTransactionManager.resetTransactions();
			updateCachedHolidayRedirects();
			UpdateShouldUseLevelBatching();
		}

		public static void loading()
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
		}

		public static void exit()
		{
			onLevelExited?.Invoke();

			_isEditor = false;
			isExiting = true;

			_info = null;
			ResetCachedLevelAsset();
			LoadingUI.updateScene();

			if (!Dedicator.IsDedicatedServer)
			{
				LoadingUI.SetLoadingText("Loading_MainMenu");

				instance.StartCoroutine(instance.ReturnToMainMenu());
			}
		}

		/// <summary>
		/// Refreshes known levels and attempts to redirect level reference if it no longer exists.
		/// </summary>
		public static void UpdateLevelReference(ref LevelInfo levelInfo)
		{
			if (levelInfo == null)
			{
				return;
			}

			// Don't need to re-scan if we already know this level is stale.
			if (!levelInfo.WasRemovedFromKnownLevels)
			{
				ScanKnownLevels();
			}

			if (levelInfo.WasRemovedFromKnownLevels)
			{
				foreach (LevelInfo knownLevel in knownLevels)
				{
					if (knownLevel.publishedFileId != levelInfo.publishedFileId)
					{
						continue;
					}

					if (string.Equals(levelInfo.name, knownLevel.name, System.StringComparison.OrdinalIgnoreCase))
					{
						levelInfo = knownLevel;
						return;
					}
				}

				levelInfo = null;
			}
		}

		public static LevelInfo getLevel(string name)
		{
			ScanKnownLevels();

			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (string.Equals(name, knownLevel.name, System.StringComparison.OrdinalIgnoreCase))
				{
					return knownLevel;
				}
			}

			return null;
		}

		/// <summary>
		/// Find level matching both name AND workshop file ID (can be zero).
		/// </summary>
		internal static LevelInfo FindLevel(SavedLevelSelection savedLevelSelection)
		{
			ScanKnownLevels();

			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (knownLevel.publishedFileId != savedLevelSelection.workshopFileId)
				{
					continue;
				}

				if (string.Equals(savedLevelSelection.name, knownLevel.name, System.StringComparison.OrdinalIgnoreCase))
				{
					return knownLevel;
				}
			}

			return null;
		}

		private static LevelInfo ReadLevelInfo(string path, bool usePath, ulong publishedFileId = 0)
		{
			if (usePath)
			{
				path = ReadWrite.PATH + path;
			}

			return ReadLevelInfo(path, publishedFileId);
		}

		/// <summary>
		/// Load level details from Level.dat in directory path.
		/// </summary>
		private static LevelInfo ReadLevelInfo(string directoryPath, ulong publishedFileId = 0)
		{
			try
			{
				string levelFilePath = Path.Combine(directoryPath, "Level.dat");

				if (!File.Exists(levelFilePath))
					return null;

				Block block = ReadWrite.readBlock(levelFilePath, false, false, 0);
				byte version = block.readByte();
				byte[] hash = block.getHash();

				bool isEditable = block.readSteamID() == Provider.client || ReadWrite.fileExists(Path.Combine(directoryPath, ".unlocker"), false, false);
				ELevelSize size = (ELevelSize) block.readByte();

				ELevelType type = ELevelType.SURVIVAL;
				if (version > 1)
				{
					type = (ELevelType) block.readByte();
				}

				string levelName = ReadWrite.folderName(directoryPath);

				LevelInfoConfigData config;
				string levelConfigFilePath = Path.Combine(directoryPath, "Config.json");
				if (File.Exists(levelConfigFilePath))
				{
					try
					{
						using (FileStream fileStream = new FileStream(levelConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
						using (SHA1Stream hashStream = new SHA1Stream(fileStream))
						using (StreamReader streamReader = new StreamReader(hashStream))
						{
							string contents = streamReader.ReadToEnd();
							config = Newtonsoft.Json.JsonConvert.DeserializeObject<LevelInfoConfigData>(contents);
							config.Hash = hashStream.Hash;
						}
					}
					catch
					{
						Assets.reportError(string.Format("Unable to parse {0}/Config.json! Consider validating with a JSON linter", levelName));
						config = null;
					}

					if (config == null)
					{
						config = new LevelInfoConfigData();
						//UnturnedLog.info("{0} using default level config as fallback", levelName);
					}
				}
				else
				{
					config = new LevelInfoConfigData();
					//UnturnedLog.info("{0} has no level config, using default", levelName);
				}

				if (!Parser.TryGetUInt32FromIP(config.Version, out config.PackedVersion))
				{
					Assets.reportError($"Unable to parse level \"{levelName}\" version \"{config.PackedVersion}\". Expected format \"#.#.#.#\". Resetting to zero.");
					config.Version = "0.0.0.0";
					config.PackedVersion = 0;
				}

				return new LevelInfo(directoryPath, levelName, size, type, isEditable, config, publishedFileId, hash);
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, $"Caught exception reading level info file ({directoryPath}):");
				return null;
			}
		}

		private static bool doesLevelPassFilter(LevelInfo levelInfo, ESingleplayerMapCategory categoryFilter)
		{
			switch (categoryFilter)
			{
				case ESingleplayerMapCategory.OFFICIAL:
					return levelInfo.configData.Category == ESingleplayerMapCategory.OFFICIAL;

				case ESingleplayerMapCategory.CURATED:
					// Hides arena maps, instead showing them in misc tab.
					return levelInfo.type != ELevelType.ARENA && levelInfo.isCurated;

				case ESingleplayerMapCategory.WORKSHOP:
					return levelInfo.isFromWorkshop && levelInfo.isCurated == false;

				case ESingleplayerMapCategory.MISC:
				{
					bool inCuratedCategory = levelInfo.type != ELevelType.ARENA && levelInfo.isCurated;
					bool inAnotherCategory = levelInfo.configData.Category == ESingleplayerMapCategory.OFFICIAL || levelInfo.isFromWorkshop || inCuratedCategory;
					bool isExplicitlyMisc = levelInfo.configData.Category == ESingleplayerMapCategory.MISC;
					return isExplicitlyMisc || (levelInfo.isEditable && inAnotherCategory == false);
				}

				case ESingleplayerMapCategory.ALL: // Used for filtering, not an actual option
					return true;

				case ESingleplayerMapCategory.EDITABLE: // Used for filtering editor list
					return levelInfo.isEditable;

				default:
					UnturnedLog.warn("Unknown map filter '{0}'", categoryFilter);
					return true;
			}
		}

		public static LevelInfo[] getLevels(ESingleplayerMapCategory categoryFilter)
		{
			ScanKnownLevels();

			List<LevelInfo> filteredLevels = new List<LevelInfo>();

			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (doesLevelPassFilter(knownLevel, categoryFilter))
				{
					filteredLevels.Add(knownLevel);
				}
			}

			return filteredLevels.ToArray();
		}

		/// <summary>
		/// Server list allows player to enter a map name when searching, so we try to find a local
		/// copy of the level for version number comparison. (Server map version might differ.)
		/// </summary>
		public static LevelInfo findLevelForServerFilter(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter) || filter.Length < 2)
				return null; // Likely accidental string.

			ScanKnownLevels();

			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (knownLevel.configData == null || knownLevel.configData.PackedVersion == 0)
					continue;

				if (knownLevel.name.StartsWith(filter, System.StringComparison.OrdinalIgnoreCase))
				{
					return knownLevel;
				}
			}

			return null;
		}

		/// <summary>
		/// New map filter uses lowercase map name and doesn't need startswith.
		/// </summary>
		public static LevelInfo FindLevelForServerFilterExact(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter) || filter.Length < 2)
				return null; // Likely accidental string.

			ScanKnownLevels();

			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (knownLevel.configData == null || knownLevel.configData.PackedVersion == 0)
					continue;

				if (knownLevel.name.Equals(filter, System.StringComparison.OrdinalIgnoreCase))
				{
					return knownLevel;
				}
			}

			return null;
		}

		private static List<LevelInfo> knownLevels = new List<LevelInfo>();

		private static LevelInfo FindKnownLevelByPath(string path)
		{
			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (string.Equals(knownLevel.path, path))
				{
					return knownLevel;
				}
			}

			return null;
		}

		private static LevelInfo FindKnownLevelByPublishedFileId(ulong fileId)
		{
			foreach (LevelInfo knownLevel in knownLevels)
			{
				if (knownLevel.publishedFileId == fileId)
				{
					return knownLevel;
				}
			}

			return null;
		}

		/// <summary>
		/// Search all map folders to add any previously unregistered maps.
		/// </summary>
		private static void ScanKnownLevels()
		{
			// First: Remove any levels found during previous calls that may have been deleted.
			// (in particular, after the menu is refreshed when the delete level button is clicked)
			try
			{
				for (int index = knownLevels.Count - 1; index >= 0; --index)
				{
					LevelInfo knownLevel = knownLevels[index];
					if (!Directory.Exists(knownLevel.path))
					{
						knownLevel.WasRemovedFromKnownLevels = true;
						knownLevels.RemoveAt(index);
						UnturnedLog.info($"Removed previously discovered level \"{knownLevel.name}\" at \"{knownLevel.path}\" (no longer exists)");
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception checking for deleted levels:");
			}

			try
			{
				string rootMapsFolder = PathEx.Join(UnturnedPaths.RootDirectory, "Maps");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
				if (!Directory.Exists(rootMapsFolder) && Provider.steamAppInstallDirectory != null)
				{
					rootMapsFolder = PathEx.Join(Provider.steamAppInstallDirectory, "Maps");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST

				foreach (string rootMapFolderPath in Directory.GetDirectories(rootMapsFolder))
				{
					LevelInfo knownLevel = FindKnownLevelByPath(rootMapFolderPath);
					if (knownLevel == null)
					{
						LevelInfo loadedLevel = ReadLevelInfo(rootMapFolderPath);
						if (loadedLevel != null)
						{
							knownLevels.Add(loadedLevel);
							UnturnedLog.info($"Discovered level \"{loadedLevel.name}\" at \"{loadedLevel.path}\"");
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception loading levels in root Maps folder:");
			}

			if (Provider.provider.workshopService.ugc != null)
			{
				try
				{
					foreach (SteamContent content in Provider.provider.workshopService.ugc)
					{
						if (content.type != ESteamUGCType.MAP)
							continue;

						if (LocalWorkshopSettings.get().getEnabled(content.publishedFileID) == false)
							continue;

						LevelInfo knownLevel = FindKnownLevelByPublishedFileId(content.publishedFileID.m_PublishedFileId);
						if (knownLevel == null)
						{
							string levelFolder = ReadWrite.folderFound(content.path, false);
							LevelInfo loadedLevel = ReadLevelInfo(levelFolder, content.publishedFileID.m_PublishedFileId);
							if (loadedLevel != null)
							{
								knownLevels.Add(loadedLevel);
								UnturnedLog.info($"Discovered level \"{loadedLevel.name}\" at \"{loadedLevel.path}\"");
							}
						}
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception loading levels from Steam Workshop:");
				}
			}
			else
			{
				string legacyServerGlobalWorkshopMapsFolder = PathEx.Join(UnturnedPaths.RootDirectory, "Bundles", "Workshop", "Maps");
				try
				{
					if (!ReadWrite.folderExists(legacyServerGlobalWorkshopMapsFolder, false))
					{
						ReadWrite.createFolder(legacyServerGlobalWorkshopMapsFolder, false);
					}

					foreach (string legacyServerGlobalWorkshopMapFolderPath in Directory.GetDirectories(legacyServerGlobalWorkshopMapsFolder))
					{
						LevelInfo knownLevel = FindKnownLevelByPath(legacyServerGlobalWorkshopMapFolderPath);
						if (knownLevel == null)
						{
							LevelInfo loadedLevel = ReadLevelInfo(legacyServerGlobalWorkshopMapFolderPath);
							if (loadedLevel != null)
							{
								knownLevels.Add(loadedLevel);
								UnturnedLog.info($"Discovered level \"{loadedLevel.name}\" at \"{loadedLevel.path}\"");
							}
						}
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception loading levels in legacy server global workshop Maps folder ({legacyServerGlobalWorkshopMapsFolder}):");
				}

				string legacyPerServerWorkshopMapsFolder = PathEx.Join(UnturnedPaths.RootDirectory, ServerSavedata.directoryName, Provider.serverID, "Workshop", "Maps");
				try
				{
					if (!ReadWrite.folderExists(legacyPerServerWorkshopMapsFolder, false))
					{
						ReadWrite.createFolder(legacyPerServerWorkshopMapsFolder, false);
					}

					foreach (string legacyPerServerWorkshopMapFolderPath in Directory.GetDirectories(legacyPerServerWorkshopMapsFolder))
					{
						LevelInfo knownLevel = FindKnownLevelByPath(legacyPerServerWorkshopMapFolderPath);
						if (knownLevel == null)
						{
							LevelInfo loadedLevel = ReadLevelInfo(legacyPerServerWorkshopMapFolderPath);
							if (loadedLevel != null)
							{
								knownLevels.Add(loadedLevel);
								UnturnedLog.info($"Discovered level \"{loadedLevel.name}\" at \"{loadedLevel.path}\"");
							}
						}
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception loading levels in legacy per-server workshop Maps folder ({legacyPerServerWorkshopMapsFolder}):");
				}

				string legacyPerServerMapsFolder = PathEx.Join(UnturnedPaths.RootDirectory, ServerSavedata.directoryName, Provider.serverID, "Maps");
				try
				{
					if (!ReadWrite.folderExists(legacyPerServerMapsFolder, false))
					{
						ReadWrite.createFolder(legacyPerServerMapsFolder, false);
					}

					foreach (string legacyPerServerWorkshopMapFolderPath in Directory.GetDirectories(legacyPerServerMapsFolder))
					{
						LevelInfo knownLevel = FindKnownLevelByPath(legacyPerServerWorkshopMapFolderPath);
						if (knownLevel == null)
						{
							LevelInfo loadedLevel = ReadLevelInfo(legacyPerServerWorkshopMapFolderPath);
							if (loadedLevel != null)
							{
								knownLevels.Add(loadedLevel);
								UnturnedLog.info($"Discovered level \"{loadedLevel.name}\" at \"{loadedLevel.path}\"");
							}
						}
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception loading levels in legacy per-server Maps folder ({legacyPerServerMapsFolder}):");
				}
			}

			if (DedicatedUGC.ugc != null)
			{
				try
				{
					foreach (SteamContent content in DedicatedUGC.ugc)
					{
						if (content.type != ESteamUGCType.MAP)
							continue;

						LevelInfo knownLevel = FindKnownLevelByPublishedFileId(content.publishedFileID.m_PublishedFileId);
						if (knownLevel == null)
						{
							string levelFolder = ReadWrite.folderFound(content.path, false);
							LevelInfo loadedLevel = ReadLevelInfo(levelFolder, content.publishedFileID.m_PublishedFileId);
							if (loadedLevel != null)
							{
								knownLevels.Add(loadedLevel);
								UnturnedLog.info($"Discovered level \"{loadedLevel.name}\" at \"{loadedLevel.path}\"");
							}
						}
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception loading levels from server Steam Workshop:");
				}
			}
		}

		public delegate void SatelliteCaptureDelegate();
		public static event SatelliteCaptureDelegate onSatellitePreCapture;
		public static event SatelliteCaptureDelegate onSatellitePostCapture;

		public static void bindSatelliteCaptureInEditor(SatelliteCaptureDelegate preCapture, SatelliteCaptureDelegate postCapture)
		{
			if (isEditor)
			{
				onSatellitePreCapture += preCapture;
				onSatellitePostCapture += postCapture;
			}
		}

		public static void unbindSatelliteCapture(SatelliteCaptureDelegate preCapture, SatelliteCaptureDelegate postCapture)
		{
			onSatellitePreCapture -= preCapture;
			onSatellitePostCapture -= postCapture;
		}

		private static void SetAllObjectsAndTreesActiveForSatelliteCapture()
		{
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<LevelObject> levelObjects = LevelObjects.objects[x, y];
					foreach (LevelObject levelObject in levelObjects)
					{
						bool shouldBeHidden = levelObject.asset?.ShouldExcludeFromSatelliteCapture ?? true;
						bool shouldBeActive = !shouldBeHidden;
						levelObject.SetIsActiveOverrideForSatelliteCapture(shouldBeActive);
					}
				}
			}

			List<ResourceSpawnpoint> allTrees = new List<ResourceSpawnpoint>();
			LevelGround.GatherAllTrees(allTrees);
			foreach (ResourceSpawnpoint tree in allTrees)
			{
				bool shouldBeActive = tree.asset?.holidayRestriction == ENPCHoliday.NONE;
				tree.SetIsActiveOverrideForSatelliteCapture(shouldBeActive);
			}
		}

		private static void RestorePreCaptureState()
		{
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					List<LevelObject> levelObjects = LevelObjects.objects[x, y];
					foreach (LevelObject levelObject in levelObjects)
					{
						levelObject.UpdateActiveAndRenderersEnabled();
					}
				}
			}

			List<ResourceSpawnpoint> allTrees = new List<ResourceSpawnpoint>();
			LevelGround.GatherAllTrees(allTrees);
			foreach (ResourceSpawnpoint tree in allTrees)
			{
				tree.UpdateActive();
			}
		}

		public static void CaptureSatelliteImage()
		{
			CartographyVolume cartographyVolume = CartographyVolumeManager.Get().GetMainVolume();

			int imageWidth;
			int imageHeight;

			if (cartographyVolume != null)
			{
				Vector3 position;
				Quaternion rotation;
				cartographyVolume.GetSatelliteCaptureTransform(out position, out rotation);
				satelliteCaptureTransform.SetPositionAndRotation(position, rotation);

				// X is left/right, Y is depth, and Z is up/down
				Bounds localBounds = cartographyVolume.CalculateLocalBounds();
				Vector3 size = localBounds.size;
				imageWidth = Mathf.CeilToInt(size.x);
				imageHeight = Mathf.CeilToInt(size.z);

				// orthographicSize is half of the vertical capture size.
				satelliteCaptureCamera.aspect = size.x / size.z;
				satelliteCaptureCamera.orthographicSize = size.z * 0.5f;
			}
			else
			{
				imageWidth = size;
				imageHeight = size;
				satelliteCaptureTransform.position = new Vector3(0, 1028, 0);
				satelliteCaptureTransform.rotation = Quaternion.Euler(90, 0, 0);
				satelliteCaptureCamera.orthographicSize = (size / 2) - border;
				satelliteCaptureCamera.aspect = 1.0f;
			}

			// 2x resolution so we can downsample to final resolution.
			RenderTexture captureRenderTexture = RenderTexture.GetTemporary(imageWidth * 2, imageHeight * 2, 32);
			captureRenderTexture.name = "Satellite";
			captureRenderTexture.filterMode = FilterMode.Bilinear; // Bilinear downsampling.
			satelliteCaptureCamera.targetTexture = captureRenderTexture;

			bool fog = RenderSettings.fog;
			UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
			Color ambientSkyColor = RenderSettings.ambientSkyColor;
			Color ambientEquatorColor = RenderSettings.ambientEquatorColor;
			Color ambientGroundColor = RenderSettings.ambientGroundColor;

			float lod = QualitySettings.lodBias;
			float spec = LevelLighting.getSeaFloat("_Shininess");
			Color col = LevelLighting.getSeaColor("_SpecularColor");

			ERenderMode mode = GraphicsSettings.renderMode;
			GraphicsSettings.renderMode = ERenderMode.FORWARD;
			GraphicsSettings.apply("capturing satellite");

			RenderSettings.fog = false;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = Palette.AMBIENT;
			RenderSettings.ambientEquatorColor = Palette.AMBIENT;
			RenderSettings.ambientGroundColor = Palette.AMBIENT;

			LevelLighting.setSeaFloat("_Shininess", 500);
			LevelLighting.setSeaColor("_SpecularColor", Color.black);
			QualitySettings.lodBias = float.MaxValue;

			SetAllObjectsAndTreesActiveForSatelliteCapture();

			onSatellitePreCapture?.Invoke();

			satelliteCaptureCamera.Render();

			onSatellitePostCapture?.Invoke();

			RestorePreCaptureState();

			GraphicsSettings.renderMode = mode;
			GraphicsSettings.apply("finished capturing satellite");

			RenderSettings.fog = fog;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.ambientSkyColor = ambientSkyColor;
			RenderSettings.ambientEquatorColor = ambientEquatorColor;
			RenderSettings.ambientGroundColor = ambientGroundColor;

			LevelLighting.setSeaFloat("_Shininess", spec);
			LevelLighting.setSeaColor("_SpecularColor", col);
			QualitySettings.lodBias = lod;

			RenderTexture downsampledRenderTexture = RenderTexture.GetTemporary(imageWidth, imageHeight);
			Graphics.Blit(captureRenderTexture, downsampledRenderTexture);
			RenderTexture.ReleaseTemporary(captureRenderTexture);

			RenderTexture.active = downsampledRenderTexture;
			Texture2D texture = new Texture2D(imageWidth, imageHeight);
			texture.name = "Satellite";
			texture.hideFlags = HideFlags.HideAndDontSave;
			texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
			RenderTexture.ReleaseTemporary(downsampledRenderTexture);

			for (int x = 0; x < texture.width; x++)
			{
				for (int y = 0; y < texture.height; y++)
				{
					Color color = texture.GetPixel(x, y);
					if (color.a < 1)
					{
						color.a = 1;

						texture.SetPixel(x, y, color);
					}
				}
			}

			texture.Apply();

			byte[] bytes = texture.EncodeToPNG();
			ReadWrite.writeBytes(Level.info.path + "/Map.png", false, false, bytes);

			DestroyImmediate(texture);
		}

		private static void FindChartHit(Vector3 pos, out EObjectChart chart, out RaycastHit hit)
		{
			Physics.Raycast(pos, Vector3.down, out hit, HEIGHT, RayMasks.CHART);

			chart = EObjectChart.NONE;

			ObjectAsset asset = LevelObjects.getAsset(hit.transform);
			if (asset != null)
			{
				chart = asset.chart;
			}
			else
			{
				ResourceSpawnpoint tree = LevelGround.FindResourceSpawnpointByTransform(hit.transform);
				ResourceAsset treeAsset = tree?.asset;
				if (treeAsset != null)
				{
					chart = treeAsset.chart;
				}
				else
				{
					if (hit.transform != null && hit.transform.gameObject.layer == LayerMasks.ENVIRONMENT)
					{
						Transform rootTransform = hit.transform.root;
						Road road = LevelRoads.FindRoadByRootTransform(rootTransform);
						if (road != null)
						{
							chart = road.GetChartMode();
						}
					}
				}
			}

			if (chart == EObjectChart.IGNORE)
			{
				FindChartHit(hit.point + new Vector3(0.0f, -0.01f, 0.0f), out chart, out hit);
			}
			else
			{
				return;
			}
		}

		public static void CaptureChartImage()
		{
			Bundle chartsBundle = Bundles.getBundle(info.path + "/Charts.unity3d", false);

			if (chartsBundle == null)
			{
				UnturnedLog.error("Unable to load chart colors");
				return;
			}

			Texture2D heightStrip = chartsBundle.load<Texture2D>("Height_Strip");
			Texture2D layerStrip = chartsBundle.load<Texture2D>("Layer_Strip");

			chartsBundle.unload();

			if (heightStrip == null || layerStrip == null)
			{
				UnturnedLog.error("Unable to find height and layer strip textures");
				return;
			}

			CartographyVolume cartographyVolume = CartographyVolumeManager.Get().GetMainVolume();

			int imageWidth;
			int imageHeight;
			float captureWidth;
			float captureHeight;

			float terrainMinHeight;
			float terrainMaxHeight;

			if (cartographyVolume != null)
			{
				Vector3 position;
				Quaternion rotation;
				cartographyVolume.GetSatelliteCaptureTransform(out position, out rotation);
				satelliteCaptureTransform.SetPositionAndRotation(position, rotation);

				Bounds worldBounds = cartographyVolume.CalculateWorldBounds();
				terrainMinHeight = worldBounds.min.y;
				terrainMaxHeight = worldBounds.max.y;

				// X is left/right, Y is depth, and Z is up/down
				Bounds localBounds = cartographyVolume.CalculateLocalBounds();
				Vector3 size = localBounds.size;
				imageWidth = Mathf.CeilToInt(size.x);
				imageHeight = Mathf.CeilToInt(size.z);
				captureWidth = size.x;
				captureHeight = size.z;
			}
			else
			{
				imageWidth = size;
				imageHeight = size;
				captureWidth = size - (border * 2f);
				captureHeight = size - (border * 2f);
				satelliteCaptureTransform.position = new Vector3(0, 1028, 0);
				satelliteCaptureTransform.rotation = Quaternion.Euler(90, 0, 0);

				terrainMinHeight = SDG.Framework.Water.WaterVolumeManager.worldSeaLevel;
				terrainMaxHeight = TERRAIN;
			}

			Texture2D texture = new Texture2D(imageWidth, imageHeight);
			texture.name = "Chart";
			texture.hideFlags = HideFlags.HideAndDontSave;

			SetAllObjectsAndTreesActiveForSatelliteCapture();

			GameObject terrainGO = new GameObject();
			terrainGO.layer = LayerMasks.GROUND;

			Color GetColor(float x, float y)
			{
				float normalizedX = x / imageWidth;
				float normalizedY = y / imageHeight;
				Vector3 localPosition = new Vector3((normalizedX - 0.5f) * captureWidth, (normalizedY - 0.5f) * captureHeight, 0);
				Vector3 worldPosition = satelliteCaptureTransform.TransformPoint(localPosition);

				EObjectChart chart;
				RaycastHit hit;

				FindChartHit(worldPosition, out chart, out hit);

				Transform transform = hit.transform;
				Vector3 point = hit.point;
				if (transform == null)
				{
					transform = terrainGO.transform;
					point = worldPosition;
					point.y = LevelGround.getHeight(worldPosition);
				}

				int layer = transform.gameObject.layer;
				if (chart == EObjectChart.GROUND)
				{
					layer = LayerMasks.GROUND;
				}
				else if (chart == EObjectChart.HIGHWAY)
				{
					layer = 0;
				}
				else if (chart == EObjectChart.ROAD)
				{
					layer = 1;
				}
				else if (chart == EObjectChart.STREET)
				{
					layer = 2;
				}
				else if (chart == EObjectChart.PATH)
				{
					layer = 3;
				}
				else if (chart == EObjectChart.LARGE)
				{
					layer = LayerMasks.LARGE;
				}
				else if (chart == EObjectChart.MEDIUM)
				{
					layer = LayerMasks.MEDIUM;
				}
				else if (chart == EObjectChart.CLIFF)
				{
					layer = 4;
				}

				Color color;

				if (chart == EObjectChart.WATER)
				{
					color = heightStrip.GetPixel(0, 0);
				}
				else if (layer == LayerMasks.GROUND)
				{
					if (SDG.Framework.Water.WaterUtility.isPointUnderwater(point))
					{
						color = heightStrip.GetPixel(0, 0);
					}
					else
					{
						float sample = Mathf.InverseLerp(terrainMinHeight, terrainMaxHeight, point.y);
						color = heightStrip.GetPixel((int) (sample * (heightStrip.width - 1)) + 1, 0);
					}
				}
				else
				{
					color = layerStrip.GetPixel(layer, 0);
				}

				return color;
			}

			for (int x = 0; x < imageWidth; ++x)
			{
				for (int y = 0; y < imageHeight; ++y)
				{
					// Essentially bilinear sampling by averaging four points inside the pixel.
					Color color = (GetColor(x + 0.25f, y + 0.25f) * 0.25f)
						+ (GetColor(x + 0.25f, y + 0.75f) * 0.25f)
						+ (GetColor(x + 0.75f, y + 0.25f) * 0.25f)
						+ (GetColor(x + 0.75f, y + 0.75f) * 0.25f);
					color.a = 1.0f;

					texture.SetPixel(x, y, color);
				}
			}

			texture.Apply();

			RestorePreCaptureState();

			byte[] bytes = texture.EncodeToPNG();
			ReadWrite.writeBytes(Level.info.path + "/Chart.png", false, false, bytes);

			DestroyImmediate(texture);
		}

		private IEnumerator ReturnToMainMenu()
		{
			// Wait a frame for loading screen to appear.
			yield return null;

			UnturnedLog.info("Returning to main menu");
			UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");

			if (placeholderAudioListener != null)
			{
				Destroy(placeholderAudioListener);
				placeholderAudioListener = null;
			}

			Provider.updateRichPresence();

#if UNITY_EDITOR
			LevelZombies.tables = null;
			ZombieClothing.build();
#endif

			LevelBatching.Get()?.Destroy();

			SDG.Framework.Devkit.Transactions.DevkitTransactionManager.resetTransactions();
			updateCachedHolidayRedirects();
			UpdateShouldUseLevelBatching();

			isExiting = false;
		}

		public IEnumerator init(int id)
		{
			LevelNavigation.load();

			if (shouldUseLevelBatching)
			{
				LevelBatching.Get()?.Reset();
			}

#if EDITORDEBUG
			UnturnedLog.info("Loaded navigation.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(1 / STEPS);
			yield return null;

			LevelObjects.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded objects.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(2 / STEPS);
			yield return null;

			LevelLighting.load(size);

#if EDITORDEBUG
			UnturnedLog.info("Loaded lighting.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(3 / STEPS);
			yield return null;

			LevelGround.load(size);

#if EDITORDEBUG
			UnturnedLog.info("Loaded ground.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(4 / STEPS);
			yield return null;

			LevelRoads.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded roads.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(5 / STEPS);
			yield return null;

			LevelNodes.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded nodes.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(6 / STEPS);
			yield return null;

			LevelItems.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded items.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(7 / STEPS);
			yield return null;

			LevelPlayers.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded players.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(8 / STEPS);
			yield return null;

			LevelZombies.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded zombies.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(9 / STEPS);
			yield return null;

			LevelVehicles.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded vehicles.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(10 / STEPS);
			yield return null;

			LevelAnimals.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded animals.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(11 / STEPS);
			yield return null;

			LevelVisibility.load();

#if EDITORDEBUG
			UnturnedLog.info("Loaded visibility.");
#endif

			LoadingUI.NotifyLevelLoadingProgress(12 / STEPS);
			yield return null;

			// Reset landscape hash before loadingSteps opens landscape tiles.
			pendingHashes = new List<byte[]>();

			loadingSteps?.Invoke();

			LoadingUI.NotifyLevelLoadingProgress(13 / STEPS);
			yield return null;

			// LevelHierarchy is initialized now, so convert legacy terrain if necessary.
			if (LevelGround.hasLegacyDataForConversion)
			{
				if (SDG.Framework.Landscapes.Landscape.instance == null)
				{
					GameObject terrainGameObject = new GameObject(); // renames itself to Landscape
					SDG.Framework.Landscapes.Landscape landscape = terrainGameObject.AddComponent<SDG.Framework.Landscapes.Landscape>();
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(landscape);
				}

				yield return SDG.Framework.Landscapes.Landscape.instance.AutoConvertLegacyTerrain();
			}

			SDG.Framework.Landscapes.LandscapeHoleVolumeManager.Get().ApplyToTerrain();

			if (LevelNodes.hasLegacyVolumesForConversion)
			{
				LevelNodes.AutoConvertLegacyVolumes();
			}

			if (LevelNodes.hasLegacyNodesForConversion)
			{
				LevelNodes.AutoConvertLegacyNodes();
			}

#if !DEDICATED_SERVER
			CullingVolumeManager.Get().RefreshOverlappingObjects();
#endif // !DEDICATED_SERVER

			LoadingUI.NotifyLevelLoadingProgress(14 / STEPS);
			yield return null;

			if (shouldUseLevelBatching)
			{
				LevelBatching.Get()?.ApplyTextureAtlas();

				LoadingUI.NotifyLevelLoadingProgress(15 / STEPS);
				yield return null;

				LevelBatching.Get()?.ApplyStaticBatching();

				LoadingUI.NotifyLevelLoadingProgress(16 / STEPS);
				yield return null;
			}

			if (!isEditor && (info?.configData?.Enable_Static_Volumes ?? false))
			{
				foreach (VolumeManagerBase volumeManager in VolumeManagerBase.allManagers)
				{
					if (!volumeManager.WantsStaticVolumes)
						continue;

					volumeManager.InitStaticVolumes();
				}
			}

			includeHash("Level.dat", info.hash);
			if (info.configData.Hash != null)
			{
				// Hash can be null if Config.json was missing or unable to parse.
				includeHash("Config.json", info.configData.Hash);
			}
			includeHash("Lighting.dat", LevelLighting.hash);
			includeHash("Nodes.dat", LevelNodes.hash);
			includeHash("Objects.dat", LevelObjects.hash);
			includeHash("Resources.dat", LevelGround.treesHash);
			combineHashes();

			Physics.gravity = new Vector3(0, info.configData.Gravity, 0);

			LoadingUI.NotifyLevelLoadingProgress(17 / STEPS);
			yield return null;

			Resources.UnloadUnusedAssets();
			System.GC.Collect();

			LoadingUI.NotifyLevelLoadingProgress(18 / STEPS);
			yield return null;

			_editing = new GameObject().transform;
			editing.name = "Editing";
			editing.parent = level;

			if (isEditor)
			{
				// Editor spawned has its own audio listener.
				Destroy(placeholderAudioListener);
				placeholderAudioListener = null;

				satelliteCaptureGameObject = Instantiate(Resources.Load<GameObject>("Edit/Mapper"));
				satelliteCaptureGameObject.name = "Mapper";
				satelliteCaptureTransform = satelliteCaptureGameObject.transform;
				satelliteCaptureTransform.parent = editing;
				satelliteCaptureCamera = satelliteCaptureGameObject.GetComponent<Camera>();

				string editorPath =
#if WITH_NOREDIST
				"Edit_NoRedist/Editor";
#else
				"Edit/Editor";
#endif
				Transform editor = Instantiate(Resources.Load<GameObject>(editorPath)).transform;
				editor.name = "Editor";
				editor.parent = editing;
				editor.tag = "Logic";
				editor.gameObject.layer = LayerMasks.LOGIC;

#if EDITORDEBUG
				UnturnedLog.info("Loaded editor.");
#endif
			}

			yield return null;

			onPrePreLevelLoaded?.Invoke(id);

			yield return null;

			onPreLevelLoaded?.Invoke(id);

			yield return null;

			onLevelLoaded?.Invoke(id);

			yield return null;

			onPostLevelLoaded?.Invoke(id);

			yield return null;

			if (!isEditor && info != null)
			{
				string hardcodedTriggersResourceName = null;
				if (string.Equals(info.name, "germany", System.StringComparison.InvariantCultureIgnoreCase))
				{
					hardcodedTriggersResourceName = "Level/Triggers_Germany";
				}
				else if (string.Equals(info.name, "pei", System.StringComparison.InvariantCultureIgnoreCase))
				{
					hardcodedTriggersResourceName = "Level/Triggers_PEI";
				}
				else if (string.Equals(info.name, "russia", System.StringComparison.InvariantCultureIgnoreCase))
				{
					hardcodedTriggersResourceName = "Level/Triggers_Russia";
				}
				else if (string.Equals(info.name, "tutorial", System.StringComparison.InvariantCultureIgnoreCase))
				{
					hardcodedTriggersResourceName = "Level/Triggers_Tutorial";
				}

				if (string.IsNullOrEmpty(hardcodedTriggersResourceName))
				{
					UnturnedLog.info($"Level \"{info.name}\" not using hardcoded special events");
				}
				else
				{
					UnturnedLog.info($"Loading hardcoded special events \"{hardcodedTriggersResourceName}\" for level \"{info.name}\"");
					GameObject triggersGameObject = Instantiate(Resources.Load<GameObject>(hardcodedTriggersResourceName));
					Transform triggersTransform = triggersGameObject.transform;
					triggersTransform.position = Vector3.zero;
					triggersTransform.rotation = Quaternion.identity;
					triggersTransform.name = "Triggers";
					triggersTransform.parent = clips;
				}
			}

			LoadingUI.NotifyLevelLoadingProgress(19 / STEPS);
			yield return null;

			if (shouldLogSpawnTablesAfterLoadingLevel)
			{
				SpawnTableTool.LogAllSpawnTables();
			}

			_isLoaded = true;
			isLoadingContent = false;

			ContinuousIntegration.reportSuccess();
		}

		private void Awake()
		{
			if (isInitialized)
			{
				Destroy(gameObject);
				return;
			}

			_isInitialized = true;
			DontDestroyOnLoad(gameObject);
			instance = this;

			foliageVolumeManager = new Framework.Foliage.FoliageVolumeManager();
			undergroundWhitelistVolumeManager = new Framework.Devkit.UndergroundWhitelistVolumeManager();
			playerClipVolumeManager = new Framework.Devkit.PlayerClipVolumeManager();
			navClipVolumeManager = new Framework.Devkit.NavClipVolumeManager();
			waterVolumeManager = new Framework.Water.WaterVolumeManager();
			landscapeHoleVolumeManager = new Framework.Landscapes.LandscapeHoleVolumeManager();
			deadzoneVolumeManager = new Framework.Devkit.DeadzoneVolumeManager();
			killVolumeManager = new Framework.Devkit.KillVolumeManager();
			effectVolumeManager = new Framework.Devkit.EffectVolumeManager();
			ambianceVolumeManager = new Framework.Devkit.AmbianceVolumeManager();
			entranceVolumeManager = new Framework.Devkit.TeleporterEntranceVolumeManager();
			exitVolumeManager = new Framework.Devkit.TeleporterExitVolumeManager();
			safezoneVolumeManager = new SafezoneVolumeManager();
			arenaVolumeManager = new ArenaCompactorVolumeManager();
			hordePurchaseVolumeManager = new HordePurchaseVolumeManager();
			cartographyVolumeManager = new CartographyVolumeManager();
			noStructuresVolumeManager = new NoStructuresVolumeManager();
			oxygenVolumeManager = new OxygenVolumeManager();
			cullingVolumeManager = new CullingVolumeManager();
			rewardVolumeManager = new NPCRewardVolumeManager();
			playerActivityVolumeManager = new NPCOverlapVolumeManager();
#if !DEDICATED_SERVER
			LevelBatching.instance = new LevelBatching();
#endif // !DEDICATED_SERVER

			airdropNodeSystem = new AirdropDevkitNodeSystem();
			locationNodeSystem = new LocationDevkitNodeSystem();
			spawnpointSystem = new Framework.Devkit.SpawnpointSystemV2();

			UnityEngine.SceneManagement.SceneManager.sceneLoaded += onSceneLoaded;
		}

		private static SDG.Framework.Foliage.FoliageVolumeManager foliageVolumeManager;
		private static SDG.Framework.Devkit.UndergroundWhitelistVolumeManager undergroundWhitelistVolumeManager;
		private static SDG.Framework.Devkit.PlayerClipVolumeManager playerClipVolumeManager;
		private static SDG.Framework.Devkit.NavClipVolumeManager navClipVolumeManager;
		private static SDG.Framework.Water.WaterVolumeManager waterVolumeManager;
		private static SDG.Framework.Landscapes.LandscapeHoleVolumeManager landscapeHoleVolumeManager;
		private static SDG.Framework.Devkit.DeadzoneVolumeManager deadzoneVolumeManager;
		private static SDG.Framework.Devkit.KillVolumeManager killVolumeManager;
		private static SDG.Framework.Devkit.EffectVolumeManager effectVolumeManager;
		private static SDG.Framework.Devkit.AmbianceVolumeManager ambianceVolumeManager;
		private static SDG.Framework.Devkit.TeleporterEntranceVolumeManager entranceVolumeManager;
		private static SDG.Framework.Devkit.TeleporterExitVolumeManager exitVolumeManager;
		private static SafezoneVolumeManager safezoneVolumeManager;
		private static ArenaCompactorVolumeManager arenaVolumeManager;
		private static HordePurchaseVolumeManager hordePurchaseVolumeManager;
		private static CartographyVolumeManager cartographyVolumeManager;
		private static NoStructuresVolumeManager noStructuresVolumeManager;
		private static OxygenVolumeManager oxygenVolumeManager;
		private static CullingVolumeManager cullingVolumeManager;
		private static NPCRewardVolumeManager rewardVolumeManager;
		private static NPCOverlapVolumeManager playerActivityVolumeManager;

		private static AirdropDevkitNodeSystem airdropNodeSystem;
		private static LocationDevkitNodeSystem locationNodeSystem;
		private static SDG.Framework.Devkit.SpawnpointSystemV2 spawnpointSystem;

		private void onSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			if (scene.buildIndex == BUILD_INDEX_GAME || scene.buildIndex == BUILD_INDEX_LOADING)
			{
				if (placeholderAudioListener == null)
				{
					// In multiplayer this already exits because the "Loading" level was loaded.
					placeholderAudioListener = instance.gameObject.AddComponent<AudioListener>();
				}
			}
			else if (scene.buildIndex == BUILD_INDEX_MENU)
			{
				// Should already have been cleaned up, but it seems like there might be an issue with changing scenes multiple times.
				if (placeholderAudioListener != null)
				{
					Destroy(placeholderAudioListener);
					placeholderAudioListener = null;
				}
			}

			if (scene.buildIndex == BUILD_INDEX_LOADING)
			{
				return;
			}

			if (scene.buildIndex > BUILD_INDEX_SETUP && info != null)
			{
				UnturnedPathfinding.Get().OnGameLevelInstantiated(); // Called here because A* object _was_ part of Game scene.

				_level = new GameObject().transform;
				level.name = info.name;
				level.tag = "Logic";
				level.gameObject.layer = LayerMasks.LOGIC;

				_roots = new GameObject().transform;
				roots.name = "Roots";
				roots.parent = level;

				_clips = new GameObject().transform;
				clips.name = "Clips";
				clips.parent = level;
				clips.tag = "Clip";
				clips.gameObject.layer = LayerMasks.CLIP;

				if (info.configData.Use_Legacy_Clip_Borders)
				{
					// caps are rounded corners + top/bottom

					Transform cap = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Cap"))).transform;
					cap.position = new Vector3(0, -4, 0);
					cap.localScale = new Vector3(size - (border * 2) + (CLIP * 2), size - (border * 2) + (CLIP * 2), 1);
					cap.rotation = Quaternion.Euler(-90, 0, 0);
					cap.name = "Cap";
					cap.parent = clips;

					cap = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Cap"))).transform;
					cap.position = new Vector3(0, HEIGHT + 4, 0);
					cap.localScale = new Vector3(size - (border * 2) + (CLIP * 2), size - (border * 2) + (CLIP * 2), 1);
					cap.rotation = Quaternion.Euler(90, 0, 0);
					cap.name = "Cap";
					cap.parent = clips;

					/* Disabled rounded corners because they were kind of pointless, and complicated the out-of-bounds check.
					cap = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Cap"))).transform;
					cap.position = new Vector3(size / 2 - border, HEIGHT / 2, size / 2 - border);
					cap.localScale = new Vector3(CLIP * 4, CLIP * 4, 64);
					cap.rotation = Quaternion.Euler(90, 0, 45);
					cap.name = "Cap";
					cap.parent = clips;

					cap = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Cap"))).transform;
					cap.position = new Vector3(-size / 2 + border, HEIGHT / 2, size / 2 - border);
					cap.localScale = new Vector3(CLIP * 4, CLIP * 4, 64);
					cap.rotation = Quaternion.Euler(90, 0, 45);
					cap.name = "Cap";
					cap.parent = clips;

					cap = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Cap"))).transform;
					cap.position = new Vector3(size / 2 - border, HEIGHT / 2, -size / 2 + border);
					cap.localScale = new Vector3(CLIP * 4, CLIP * 4, 64);
					cap.rotation = Quaternion.Euler(90, 0, 45);
					cap.name = "Cap";
					cap.parent = clips;

					cap = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Cap"))).transform;
					cap.position = new Vector3(-size / 2 + border, HEIGHT / 2, -size / 2 + border);
					cap.localScale = new Vector3(CLIP * 4, CLIP * 4, 64);
					cap.rotation = Quaternion.Euler(90, 0, 45);
					cap.name = "Cap";
					cap.parent = clips;
					*/

					// clips are the visible walls that surround the map

					Transform clip = ((GameObject) GameObject.Instantiate(Resources.Load(Level.isEditor ? "Level/Wall" : "Level/Clip"))).transform;
					clip.position = new Vector3((size / 2) - border, HEIGHT / 8, 0);
					clip.localScale = new Vector3(size - (border * 2), HEIGHT / 4, 1);
					clip.rotation = Quaternion.Euler(0, -90, 0);
					clip.name = "Clip";
					clip.parent = clips;

					if (Level.isEditor)
					{
						// modifying the material is OK here because it's cleaned up when we change maps
						clip.GetComponent<Renderer>().material.mainTextureScale = new Vector2((size - (border * 2)) / 32f, 4f);
					}

					clip = ((GameObject) GameObject.Instantiate(Resources.Load(Level.isEditor ? "Level/Wall" : "Level/Clip"))).transform;
					clip.position = new Vector3((-size / 2) + border, HEIGHT / 8, 0);
					clip.localScale = new Vector3(size - (border * 2), HEIGHT / 4, 1);
					clip.rotation = Quaternion.Euler(0, 90, 0);
					clip.name = "Clip";
					clip.parent = clips;

					if (Level.isEditor)
					{
						clip.GetComponent<Renderer>().material.mainTextureScale = new Vector2((size - (border * 2)) / 32f, 4f);
					}

					clip = ((GameObject) GameObject.Instantiate(Resources.Load(Level.isEditor ? "Level/Wall" : "Level/Clip"))).transform;
					clip.position = new Vector3(0, HEIGHT / 8, (size / 2) - border);
					clip.localScale = new Vector3(size - (border * 2), HEIGHT / 4, 1);
					clip.rotation = Quaternion.Euler(0, 180, 0);
					clip.name = "Clip";
					clip.parent = clips;

					if (Level.isEditor)
					{
						clip.GetComponent<Renderer>().material.mainTextureScale = new Vector2((size - (border * 2)) / 32f, 4f);
					}

					clip = ((GameObject) GameObject.Instantiate(Resources.Load(Level.isEditor ? "Level/Wall" : "Level/Clip"))).transform;
					clip.position = new Vector3(0, HEIGHT / 8, (-size / 2) + border);
					clip.localScale = new Vector3(size - (border * 2), HEIGHT / 4, 1);
					clip.rotation = Quaternion.identity;
					clip.name = "Clip";
					clip.parent = clips;

					if (Level.isEditor)
					{
						clip.GetComponent<Renderer>().material.mainTextureScale = new Vector2((size - (border * 2)) / 32f, 4f);
					}
				}

				StartCoroutine(instance.init(scene.buildIndex));
			}
			else
			{
				isLoadingLighting = true;
				isLoadingVehicles = true;
				isLoadingBarricades = true;
				isLoadingStructures = true;
				isLoadingContent = true;
				isLoadingArea = true;

				_isLoaded = false;

				onLevelLoaded?.Invoke(scene.buildIndex);

				LevelLighting.resetForMainMenu();
			}

			if (scene.buildIndex == BUILD_INDEX_MENU)
			{
				ContinuousIntegration.reportSuccess();
			}

			Resources.UnloadUnusedAssets();
			System.GC.Collect();
		}

#if !DEDICATED_SERVER
		private static void PlayLevelLoadingScreenMusic()
		{
			musicOutroClip = null;
			LevelAsset asset = getAsset();
			if (asset != null && asset.loadingScreenMusic != null && asset.loadingScreenMusic.Length > 0)
			{
				AudioSource audioSource = GetOrCreateMusicAudioSource();
				if (audioSource != null)
				{
					LevelAsset.LoadingScreenMusic music = asset.loadingScreenMusic.RandomOrDefault();
					AudioClip loopAudioClip = music.loopRef.loadAsset();
					if (loopAudioClip != null)
					{
						musicAudioSource.clip = loopAudioClip;
						musicAudioSource.volume *= music.loopVolume;
						musicAudioSource.loop = true;
						musicAudioSource.Play();

						// Only assign outro if we have a valid loop.
						musicOutroClip = music.outroRef.loadAsset();
						musicOutroVolume = music.outroVolume;
						if (musicOutroClip == null && music.outroRef.isValid)
						{
							UnturnedLog.warn($"Unable to find loading screen music outro \"{music.outroRef}\" for level \"{info?.getLocalizedName()}\"");
						}
					}
					else
					{
						UnturnedLog.warn($"Unable to find loading screen music loop \"{music.loopRef}\" for level \"{info?.getLocalizedName()}\"");
					}
				}
			}
		}

		private static bool _loadingScreenWantsMusic;
		internal static bool LoadingScreenWantsMusic
		{
			set
			{
				if (_loadingScreenWantsMusic != value)
				{
					_loadingScreenWantsMusic = value;
					if (!_loadingScreenWantsMusic)
					{
						PlayLoadingOutroMusic();
					}
				}
			}
		}

		private static void PlayLoadingOutroMusic()
		{
			AudioSource audioSource = GetOrCreateMusicAudioSource();
			if (audioSource != null)
			{
				if (musicOutroClip != null)
				{
					audioSource.clip = musicOutroClip;
					audioSource.volume *= musicOutroVolume;
					audioSource.loop = false;
					audioSource.Play();
					musicOutroClip = null;
				}
				else
				{
					audioSource.Stop();
				}
			}
		}

		private static AudioSource GetOrCreateMusicAudioSource()
		{
			if (Dedicator.IsDedicatedServer)
			{
				// Development build running in dedicated server mode, so do not create audio or graphics.
				return null;
			}

			if (musicAudioSource == null)
			{
				musicAudioSource = instance.gameObject.AddComponent<AudioSource>();
				musicAudioSource.playOnAwake = false;
				musicAudioSource.spatialBlend = 0.0f; // 2D
				musicAudioSource.ignoreListenerPause = true;
				musicAudioSource.ignoreListenerVolume = true; // Master volume is muted on loading screen.
				musicAudioSource.bypassEffects = true;
				musicAudioSource.bypassListenerEffects = true;
				musicAudioSource.bypassReverbZones = true;
				musicAudioSource.spatialize = false;
				musicAudioSource.dopplerLevel = 0.0f;
			}

			// Synchronize with master volume because listener volume is ignored.
			musicAudioSource.volume = OptionsSettings.volume * OptionsSettings.MusicMasterVolume * OptionsSettings.loadingScreenMusicVolume;

			if (musicAudioSource.volume > 0.0f)
			{
				return musicAudioSource;
			}
			else
			{
				// Loading screen is muted.
				return null;
			}
		}
#endif // !DEDICATED_SERVER

		private static CommandLineBool clUseLevelBatching = new CommandLineBool("-UseLevelBatching");
		private static CommandLineFlag shouldLogSpawnTablesAfterLoadingLevel = new CommandLineFlag(false, "-LogSpawnTablesAfterLoadingLevel");

		[System.Obsolete("Replaced by LevelInfo.hash")]
		public static byte[] getLevelHash(string path)
		{
			LevelInfo levelInfo = ReadLevelInfo(path);
			if (levelInfo != null)
			{
				return levelInfo.hash;
			}
			else
			{
				return new byte[20];
			}
		}

		[System.Obsolete("Was unused in vanilla, so simplified to just use the find level by name method.")]
		public static bool exists(string name)
		{
			return getLevel(name) != null;
		}
	}
}
