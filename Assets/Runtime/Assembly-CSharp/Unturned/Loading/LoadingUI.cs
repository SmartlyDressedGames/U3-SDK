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
	public enum ELoadingTip
	{
		NONE,      // 0
		HOTKEY,  // 1
		EQUIP,    // 2
		DROP,      // 3
		SIRENS,  // 4
		TRANSFORM,  // 5
		QUALITY,    // 6
		UMBRELLA,   // 7
		HEAL,      // 8
		ROTATE,  // 9
		BASE,      // 10
		DEQUIP,  // 11
		NIGHTVISION,// 12
		TRANSFER,   // 13
		SURFACE,    // 14
		ARREST,  // 15
		SAFEZONE,   // 16
		CLAIM,    // 17
		GROUP,    // 18
		MAP,        // 19
		BEACON,  // 20
		HORN,      // 21
		LIGHTS,  // 22
		SNAP,      // 23
		UPGRADE,    // 24
		GRAB,      // 25
		SKYCRANE,   // 26
		SEAT,      // 27
		RARITY,  // 28
		ORIENTATION,// 29
		RED,        // 30
		STEADY,    // 31
		STREAMER,    // 32
		SKIP_ACTION_CRAFTING_MENU, // 33
		WORKSTATION, // 34
		WORKSTATION_HEAT, // 35
		WORKSTATION_FOOD, // 36
		WORKSTATION_MEDICINE, // 37
		WORKSTATION_DYE, // 38
		WORKSTATION_CLOTHES, // 39
		WORKSTATION_POTTERY, // 40
		MINING_CLAY, // 41
		FISHING, // 42

		/// <summary>
		/// Marker for counting number of tips.
		/// </summary>
		MAX,
	}

	public class LoadingUI : MonoBehaviour
	{
		private static bool _isInitialized;
		public static bool isInitialized => _isInitialized;

		public static GameObject loader
		{
			get;
			private set;
		}

		/// <summary>
		/// Camera used while transitioning between scenes to prevent the "no cameras rendering" warning.
		/// </summary>
		public Camera placeholderCamera;

		public static SleekWindow window;
		private static Local localization;

		private static ISleekImage backgroundImage;
		private static ISleekLabel tipLabel;

		private static ISleekBox loadingBarBox;

		private static SleekLoadingScreenProgressBar loadingProgressBar;

		// Asset bundle bar is visible while async loading a "masterbundle" file.
		private static SleekLoadingScreenProgressBar assetBundleProgressBar;

		// Download bar is visible while downloading a workshop file.
		private static SleekLoadingScreenProgressBar downloadProgressBar;

		// Search bar is visible while finding .asset/.dat files.
		private static SleekLoadingScreenProgressBar searchProgressBar;

		// Reading bar is visible while loading in the found .asset/.dat files.
		private static SleekLoadingScreenProgressBar readProgressBar;

		private static ISleekButton cancelButton;
		private static ISleekLabel creditsLabel;

		/// <summary>
		/// Shown when game connection ping is significantly higher than server browser ping. At the time of writing
		/// (2025-01-17) this is likely because the server is using an "anycast proxy" in front of Steam A2S cache.
		/// </summary>
		private static ISleekBox pingWarning;

		/// <summary>
		/// Set to Time.frameCount + 1 while loading.
		/// In the past used realtime, but that was unreliable if an individual frame took too long.
		/// </summary>
		private static int lastLoading;

		private static ELoadingTip tip = ELoadingTip.NONE;

		public static bool isBlocked => Time.frameCount <= lastLoading;

		public static void SetLoadingText(string key)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (loadingProgressBar == null)
				{
					return;
				}

				loadingProgressBar.DescriptionText = localization.format(key);
				loadingProgressBar.ProgressPercentage = 1.0f;
			}
			else
			{
				CommandWindow.Log(localization.format(key));
			}
		}

		public static void NotifyLevelLoadingProgress(float progress)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (loadingProgressBar == null)
				{
					return;
				}

				loadingProgressBar.ProgressPercentage = progress;

				UpdateBackgroundAnim(progress);
			}
			else
			{
				CommandWindow.Log(localization.format("Level_Load", (int) (progress * 100)));
			}
		}

		private static bool wasLoadingAssetBundles;
		private static int previousAssetBundlesLoaded;
		private static int previousAssetBundlesFound;

		private static void UpdateAssetBundleProgress(AssetLoadingStats loadingStats)
		{
			// We show for one frame after finishing loading so that "100%" is shown.
			bool shouldShowAssetBundleInfo = loadingStats.isLoadingAssetBundles || wasLoadingAssetBundles;
			if (loadingStats.isLoadingAssetBundles != wasLoadingAssetBundles)
			{
				if (!wasLoadingAssetBundles)
				{
					previousAssetBundlesLoaded = -1;
					previousAssetBundlesFound = -1;
				}
				wasLoadingAssetBundles = loadingStats.isLoadingAssetBundles;
			}

			if (shouldShowAssetBundleInfo)
			{
				int newAssetBundlesLoaded = loadingStats.AssetBundlesLoaded;
				int newAssetBundlesFound = loadingStats.AssetBundlesFound;

				if (newAssetBundlesLoaded != previousAssetBundlesLoaded || newAssetBundlesFound != previousAssetBundlesFound)
				{
					previousAssetBundlesLoaded = newAssetBundlesLoaded;
					previousAssetBundlesFound = newAssetBundlesFound;

					string assetBundleMessage = localization.format("Loading_Asset_Bundles",
						Assets.loadingStats.AssetBundlesLoaded,
						Assets.loadingStats.AssetBundlesFound);

					if (Dedicator.IsDedicatedServer)
					{
						CommandWindow.Log(assetBundleMessage);
					}
					else
					{
						assetBundleProgressBar.DescriptionText = assetBundleMessage;
					}
				}

				if (!Dedicator.IsDedicatedServer)
				{
					if (!assetBundleProgressBar.IsVisible)
					{
						assetBundleProgressBar.IsVisible = true;
						UpdateLoadingBarPositions();
					}

					assetBundleProgressBar.ProgressPercentage = loadingStats.EstimateAssetBundleProgressPercentage();
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (assetBundleProgressBar.IsVisible)
					{
						assetBundleProgressBar.IsVisible = false;
						UpdateLoadingBarPositions();
					}
				}
			}
		}

		private static bool wasSearching;
		private static int previousFilesFound;

		private static void UpdateSearchProgress(AssetLoadingStats loadingStats)
		{
			// We show for one frame after finishing loading so that "100%" is shown.
			bool isSearching = loadingStats.SearchLocationsFinishedSearching < loadingStats.RegisteredSearchLocations;
			bool shouldShowSearchInfo = isSearching || wasSearching;
			if (isSearching != wasSearching)
			{
				if (!wasSearching)
				{
					previousFilesFound = -1;
				}
				wasSearching = isSearching;
			}

			if (shouldShowSearchInfo)
			{
				int newFilesFound = loadingStats.FilesFound;

				if (newFilesFound != previousFilesFound)
				{
					previousFilesFound = newFilesFound;

					string searchMessage = localization.format("Loading_Search",
						Assets.loadingStats.SearchLocationsFinishedSearching,
						Assets.loadingStats.RegisteredSearchLocations,
						Assets.loadingStats.FilesFound);

					if (Dedicator.IsDedicatedServer)
					{
						CommandWindow.Log(searchMessage);
					}
					else
					{
						searchProgressBar.DescriptionText = searchMessage;
					}
				}

				if (!Dedicator.IsDedicatedServer)
				{
					if (!searchProgressBar.IsVisible)
					{
						searchProgressBar.IsVisible = true;
						UpdateLoadingBarPositions();
					}

					searchProgressBar.ProgressPercentage = loadingStats.EstimateSearchProgressPercentage();
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (searchProgressBar.IsVisible)
					{
						searchProgressBar.IsVisible = false;
						UpdateLoadingBarPositions();
					}
				}
			}
		}

		private static bool wasReading;
		private static int previousReadFilesRead;
		private static int previousReadFilesFound;

		private static void UpdateReadProgress(AssetLoadingStats loadingStats)
		{
			// We show for one frame after finishing loading so that "100%" is shown.
			bool isReading = loadingStats.FilesRead < loadingStats.FilesFound;
			bool shouldShowReadingInfo = isReading || wasReading;
			if (isReading != wasReading)
			{
				if (!wasReading)
				{
					previousReadFilesRead = -1;
					previousReadFilesFound = -1;
				}
				wasReading = isReading;
			}

			if (shouldShowReadingInfo)
			{
				int newFilesRead = loadingStats.FilesRead;
				int newFilesFound = loadingStats.FilesFound;

				if (newFilesRead != previousReadFilesRead || newFilesFound != previousReadFilesFound)
				{
					previousReadFilesRead = newFilesRead;
					previousReadFilesFound = newFilesFound;

					string readMessage = localization.format("Loading_Read",
						newFilesRead,
						newFilesFound);

					if (Dedicator.IsDedicatedServer)
					{
						CommandWindow.Log(readMessage);
					}
					else
					{
						readProgressBar.DescriptionText = readMessage;
					}
				}

				if (!Dedicator.IsDedicatedServer)
				{
					if (!readProgressBar.IsVisible)
					{
						readProgressBar.IsVisible = true;
						UpdateLoadingBarPositions();
					}

					readProgressBar.ProgressPercentage = loadingStats.EstimateReadProgressPercentage();
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (readProgressBar.IsVisible)
					{
						readProgressBar.IsVisible = false;
						UpdateLoadingBarPositions();
					}
				}
			}
		}

		private static int previousAssetLoadingFilesLoaded = -1;
		private static int previousAssetLoadingFilesFound = -1;

		private static void UpdateAssetLoadingProgress(AssetLoadingStats loadingStats)
		{
			int newFilesLoaded = loadingStats.FilesLoaded;
			int newFilesFound = loadingStats.FilesFound;
			if (newFilesLoaded != previousAssetLoadingFilesLoaded || newFilesFound != previousAssetLoadingFilesFound)
			{
				previousAssetLoadingFilesLoaded = newFilesLoaded;
				previousAssetLoadingFilesFound = newFilesFound;

				string message = localization.format("Loading_Asset_Definitions",
					newFilesLoaded,
					newFilesFound);

				if (!Dedicator.IsDedicatedServer)
				{
					if (loadingProgressBar == null)
					{
						return;
					}

					loadingProgressBar.DescriptionText = message;

					loadingProgressBar.ProgressPercentage = loadingStats.EstimateFileProgressPercentage();
				}
				else
				{
					CommandWindow.Log(message);
				}
			}
		}

		private static void HideAllLoadingBars()
		{
			if (assetBundleProgressBar != null)
			{
				assetBundleProgressBar.IsVisible = false;
				searchProgressBar.IsVisible = false;
				readProgressBar.IsVisible = false;
				downloadProgressBar.IsVisible = false;
			}
		}

		internal static void NotifyAssetDefinitionLoadingProgress()
		{
			AssetLoadingStats loadingStats = Assets.loadingStats;
			UpdateAssetBundleProgress(loadingStats);
			UpdateSearchProgress(loadingStats);
			UpdateReadProgress(loadingStats);
			UpdateAssetLoadingProgress(loadingStats);
		}

		public static void SetIsDownloading(bool isDownloading)
		{
			if (downloadProgressBar != null)
			{
				downloadProgressBar.IsVisible = isDownloading;
				UpdateLoadingBarPositions();
			}
		}

		public static void SetDownloadFileName(string name)
		{
			if (downloadProgressBar != null)
			{
				downloadProgressBar.DescriptionText = localization.format("Download_Progress", name);
			}
		}

		public static void NotifyDownloadProgress(float progress)
		{
			if (downloadProgressBar != null)
			{
				downloadProgressBar.ProgressPercentage = progress;
			}
		}

		private static bool loadBackgroundImage(string path)
		{
			if (backgroundImage.Texture != null && backgroundImage.ShouldDestroyTexture)
			{
				Object.Destroy(backgroundImage.Texture);
				backgroundImage.Texture = null;
			}

			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			if (!File.Exists(path))
			{
				return false;
			}

			backgroundImage.Texture = ReadWrite.readTextureFromFile(path);
			backgroundImage.ShouldDestroyTexture = true;

			return true;
		}

		internal static string GetRandomImagePathInDirectory(string path, bool onlyWithoutHud)
		{
			try
			{
				List<string> imagePaths = new List<string>();

				DirectoryInfo dirInfo = new DirectoryInfo(path);
				foreach (FileInfo fileInfo in dirInfo.EnumerateFiles())
				{
					if (fileInfo.Length > 10000000)
					{
						// Exclude files bigger than 10MB. Our example loading screens are average 5MB.
						// This is important for screenshots captured with high resolution multiplier which can get
						// into the GB size causing the game to freeze during load.
						continue;
					}

					if (onlyWithoutHud && !fileInfo.Name.Contains("NoUI"))
					{
						// Only show nice cinematic screenshots on the startup loading screen.
						// Received emails from players concerned screenshots with HUD were a bug.
						continue;
					}

					string ext = fileInfo.Extension;
					if (string.Equals(ext, ".png", System.StringComparison.InvariantCultureIgnoreCase)
						|| string.Equals(ext, ".jpg", System.StringComparison.InvariantCultureIgnoreCase))
					{
						imagePaths.Add(fileInfo.FullName);
					}
				}

				return imagePaths.RandomOrDefault();
			}
			// Catch because I am not sure if the Info classes have different permission requirements.
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception loading background image:");
			}

			return null;
		}

		private static bool pickBackgroundImage(string path, bool onlyWithoutHud)
		{
			if (!Directory.Exists(path))
			{
				loadBackgroundImage(null);
				return false;
			}

			string imagePath = GetRandomImagePathInDirectory(path, onlyWithoutHud);
			if (!string.IsNullOrEmpty(imagePath))
			{
				loadBackgroundImage(imagePath);
				return true;
			}
			else
			{
				loadBackgroundImage(null);
				return false;
			}
		}

		/// <summary>
		/// Select a loading image while on the startup screen or a level without any images.
		/// </summary>
		private static void PickNonLevelBackgroundImage()
		{
			// Note: prior to 2022-08-10 the vanilla loading screens were kept in the Screenshots directory.

			if (OptionsSettings.enableScreenshotsOnLoadingScreen)
			{
				string playerScreenshotsPath = PathEx.Join(UnturnedPaths.RootDirectory, "Screenshots");
				if (pickBackgroundImage(playerScreenshotsPath, true))
				{
					return;
				}
			}

			string vanillaImagesPath = PathEx.Join(UnturnedPaths.RootDirectory, "LoadingScreens");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
			if (!Directory.Exists(vanillaImagesPath) && Provider.steamAppInstallDirectory != null)
			{
				vanillaImagesPath = PathEx.Join(Provider.steamAppInstallDirectory, "LoadingScreens");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
			pickBackgroundImage(vanillaImagesPath, false);
		}

		public static void updateScene()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (backgroundImage == null)
				{
					return;
				}

				if (loadingProgressBar == null)
				{
					return;
				}

				HideAllLoadingBars();
				UpdateLoadingBarPositions();

				NotifyLevelLoadingProgress(0);

				Local localization2 = Localization.read("/Menu/MenuTips.dat");

				int currentTipIndex = (int) tip;
				int newTipIndex;
				int maxTipIndexPlusOne = (int) ELoadingTip.MAX;
				do
				{
					// Random.Range max is exclusive
					newTipIndex = Random.Range(1, maxTipIndexPlusOne);
				}
				while (newTipIndex == currentTipIndex);
				tip = (ELoadingTip) newTipIndex;

				string tipText;
				if (OptionsSettings.ShouldAnonymizeMultiplayerDetails && Provider.streamerNames != null
					&& Provider.streamerNames.Count > 0 && Provider.streamerNames[0] == "Alpha" && Random.value < 0.5f)
				{
					tipText = localization2.format("Streamer");
				}
				else
				{
					switch (tip)
					{
						case ELoadingTip.HOTKEY:
							tipText = localization2.format("Hotkey");
							break;
						case ELoadingTip.EQUIP:
							tipText = localization2.format("Equip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.DROP:
							tipText = localization2.format("Drop", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.SIRENS:
							tipText = localization2.format("Sirens", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.TRANSFORM:
							tipText = localization2.format("Transform");
							break;
						case ELoadingTip.QUALITY:
							tipText = localization2.format("Quality");
							break;
						case ELoadingTip.UMBRELLA:
							tipText = localization2.format("Umbrella");
							break;
						case ELoadingTip.HEAL:
							tipText = localization2.format("Heal");
							break;
						case ELoadingTip.ROTATE:
							tipText = localization2.format("Rotate");
							break;
						case ELoadingTip.BASE:
							tipText = localization2.format("Base");
							break;
						case ELoadingTip.DEQUIP:
							tipText = localization2.format("Dequip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.dequip));
							break;
						case ELoadingTip.NIGHTVISION:
							tipText = localization2.format("Nightvision", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.vision));
							break;
						case ELoadingTip.TRANSFER:
							tipText = localization2.format("Transfer", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.SURFACE:
							tipText = localization2.format("Surface", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
							break;
						case ELoadingTip.ARREST:
							tipText = localization2.format("Arrest", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.leanLeft), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.leanRight));
							break;
						case ELoadingTip.SAFEZONE:
							tipText = localization2.format("Safezone");
							break;
						case ELoadingTip.CLAIM:
							tipText = localization2.format("Claim");
							break;
						case ELoadingTip.GROUP:
							tipText = localization2.format("Group");
							break;
						case ELoadingTip.MAP:
							tipText = localization2.format("Map", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.map));
							break;
						case ELoadingTip.BEACON:
							tipText = localization2.format("Beacon");
							break;
						case ELoadingTip.HORN:
							tipText = localization2.format("Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
							break;
						case ELoadingTip.LIGHTS:
							tipText = localization2.format("Lights", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary));
							break;
						case ELoadingTip.SNAP:
							tipText = localization2.format("Snap", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.snap));
							break;
						case ELoadingTip.UPGRADE:
							tipText = localization2.format("Upgrade", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.GRAB:
							tipText = localization2.format("Grab", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.SKYCRANE:
							tipText = localization2.format("Skycrane", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
							break;
						case ELoadingTip.SEAT:
							tipText = localization2.format("Seat");
							break;
						case ELoadingTip.RARITY:
							tipText = localization2.format("Rarity");
							break;
						case ELoadingTip.ORIENTATION:
							tipText = localization2.format("Orientation", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.rotate));
							break;
						case ELoadingTip.RED:
							tipText = localization2.format("Red");
							break;
						case ELoadingTip.STEADY:
							tipText = localization2.format("Steady", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.sprint));
							break;
						case ELoadingTip.STREAMER:
							tipText = localization2.format("Streamer");
							break;
						case ELoadingTip.SKIP_ACTION_CRAFTING_MENU:
							tipText = localization2.format("SkipActionCraftingMenu", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.SkipActionCraftingMenu));
							break;
						case ELoadingTip.WORKSTATION:
							tipText = localization2.format("Workstation");
							break;
						case ELoadingTip.WORKSTATION_HEAT:
							tipText = localization2.format("WorkstationHeat");
							break;
						case ELoadingTip.WORKSTATION_FOOD:
							tipText = localization2.format("WorkstationFood");
							break;
						case ELoadingTip.WORKSTATION_MEDICINE:
							tipText = localization2.format("WorkstationMedicine");
							break;
						case ELoadingTip.WORKSTATION_DYE:
							tipText = localization2.format("WorkstationDye");
							break;
						case ELoadingTip.WORKSTATION_CLOTHES:
							tipText = localization2.format("WorkstationClothes");
							break;
						case ELoadingTip.WORKSTATION_POTTERY:
							tipText = localization2.format("WorkstationPottery");
							break;
						case ELoadingTip.MINING_CLAY:
							tipText = localization2.format("MiningClay");
							break;
						case ELoadingTip.FISHING:
							tipText = localization2.format("Fishing");
							break;
						default:
							tipText = "#" + tip.ToString();
							break;
					}
				}

				if (Level.info != null)
				{
					// Defaults to false because some servers have rules and info on the loading screen.
					bool shouldAnimate;
					if (pickBackgroundImage(Level.info.path + "/Screenshots", false))
					{
						shouldAnimate = Level.getAsset()?.shouldAnimateBackgroundImage ?? false;
					}
					else
					{
						if (loadBackgroundImage(Level.info.path + "/Level.png"))
						{
							shouldAnimate = Level.getAsset()?.shouldAnimateBackgroundImage ?? false;
						}
						else
						{
							shouldAnimate = true; // Default backgrounds can be animated.
							PickNonLevelBackgroundImage();
						}
					}

					// 2022-05-17 disabling because it sadly does not look good enough yet without more async loading.
					shouldAnimate = false;

					if (shouldAnimate)
					{
						EnableBackgroundAnim();
					}
					else
					{
						DisableBackgroundAnim();
					}

					string levelDisplayName = Level.info.getLocalizedName();
					Local localization3 = Level.info.getLocalization();

					// Per-level custom tips.
					if (Level.info.configData.Tips > 0 && localization3 != null)
					{
						int tipIndex = Random.Range(0, Level.info.configData.Tips);
						string tipKey = "Tip_" + tipIndex;
						tipText = localization3.format(tipKey);
					}

					if (Provider.isConnected)
					{
						string security;
						if (Provider.isServer)
						{
							security = localization.format("Offline");
						}
						else
						{
							if (Provider.IsVacActiveOnCurrentServer)
							{
								security = localization.format("VAC_Secure");
							}
							else
							{
								security = localization.format("VAC_Insecure");
							}

#if WITH_THIRDPARTYAC
							if (Provider.IsThirdpartyAntiCheatActiveOnCurrentServer)
							{
								security += " + " + localization.format(ThirdpartyAntiCheat.SecureLocalizationKey);
							}
							else
							{
								security += " + " + localization.format(ThirdpartyAntiCheat.InsecureLocalizationKey);
							}
#endif
						}

						loadingProgressBar.DescriptionText = localization.format("Loading_Level_Play", levelDisplayName, Level.version, OptionsSettings.ShouldAnonymizeMultiplayerDetails ? localization.format("Streamer") : Provider.serverName, security);
					}
					else
					{
						loadingProgressBar.DescriptionText = localization.format("Loading_Level_Edit", levelDisplayName);
					}

					if (Level.info.configData.Creators.Length > 0
						|| Level.info.configData.Collaborators.Length > 0
						|| Level.info.configData.Thanks.Length > 0
						|| Level.info.configData.CustomCredits.Count > 0)
					{
						System.Text.StringBuilder credits = new System.Text.StringBuilder();

						if (Level.info.configData.Creators.Length > 0)
						{
							credits.Append("<color=#f0f0f0>");
							credits.Append(localization.format("Creators"));
							credits.AppendLine("</color>");
							credits.AppendLine();

							for (int index = 0; index < Level.info.configData.Creators.Length; index++)
							{
								credits.AppendLine(Level.info.configData.Creators[index]);
							}
						}

						if (Level.info.configData.Collaborators.Length > 0)
						{
							if (credits.Length > 0)
							{
								credits.AppendLine();
							}

							credits.Append("<color=#f0f0f0>");
							credits.Append(localization.format("Collaborators"));
							credits.AppendLine("</color>");
							credits.AppendLine();

							for (int index = 0; index < Level.info.configData.Collaborators.Length; index++)
							{
								credits.AppendLine(Level.info.configData.Collaborators[index]);
							}
						}

						if (Level.info.configData.Thanks.Length > 0)
						{
							if (credits.Length > 0)
							{
								credits.AppendLine();
							}

							credits.Append("<color=#f0f0f0>");
							credits.Append(localization.format("Thanks"));
							credits.AppendLine("</color>");
							credits.AppendLine();

							for (int index = 0; index < Level.info.configData.Thanks.Length; index++)
							{
								credits.AppendLine(Level.info.configData.Thanks[index]);
							}
						}

						if (Level.info.configData.CustomCredits.Count > 0 && localization3 != null)
						{
							foreach (KeyValuePair<string, string[]> kvp in Level.info.configData.CustomCredits)
							{
								if (credits.Length > 0)
								{
									credits.AppendLine();
								}

								credits.Append("<color=#f0f0f0>");
								credits.Append(localization3.format(kvp.Key));
								credits.AppendLine("</color>");
								credits.AppendLine();
								foreach (string name in kvp.Value)
								{
									credits.AppendLine(name);
								}
							}
						}

						creditsLabel.Text = credits.ToString();
						creditsLabel.IsVisible = true;
					}
					else
					{
						creditsLabel.IsVisible = false;
					}
				}
				else
				{
					// Startup loading
					PickNonLevelBackgroundImage();

					// 2022-05-17 disabling because it sadly does not look good enough yet without more async loading.
					DisableBackgroundAnim();

					loadingProgressBar.DescriptionText = localization.format("Loading");

					creditsLabel.IsVisible = false;
				}

				// tipText may have been overridden by level.
				RichTextUtil.replaceNewlineMarkup(ref tipText);
				tipText = ItemTool.filterRarityRichText(tipText);
				tipLabel.Text = localization2.format("Tip", tipText);

				loadingBarBox.SizeOffset_X = -20;
				cancelButton.IsVisible = false;
			}
		}

		private static void onQueuePositionUpdated()
		{
			loadingProgressBar.DescriptionText = localization.format("Queue_Position", Provider.queuePosition + 1);

			loadingBarBox.SizeOffset_X = -130;
			cancelButton.IsVisible = true;
		}

		private static void onClickedCancelButton(ISleekElement button)
		{
			Provider.RequestDisconnect("clicked queue cancel button");
		}

		private void Update()
		{
			if (!Dedicator.IsDedicatedServer && (Assets.isLoading || Provider.isLoading || Level.isLoading || Player.isLoading || Level.isExiting))
			{
				lastLoading = Time.frameCount + 1;
			}

			bool blockedThisFrame = isBlocked;

			// Block while screen is being shown. There was a bug where loading UI was still visible,
			// but audio began playing without a proper camera.
			UnturnedMasterVolume.mutedByLoadingScreen = blockedThisFrame;

			// Empty camera renders while nothing else is.
			placeholderCamera.enabled = blockedThisFrame;

#if !DEDICATED_SERVER
			Level.LoadingScreenWantsMusic = blockedThisFrame;
#endif // !DEDICATED_SERVER

			if (blockedThisFrame)
			{
				Glazier.Get().Root = window;

#if !DEDICATED_SERVER
				if (Provider.isClient && Provider.CurrentServerAdvertisement != null)
				{
					int currentPingMs = Provider.ClientPingMs;
					int advertisedPingMs = Provider.CurrentServerAdvertisement.PingMs;
					if (currentPingMs > advertisedPingMs + LiveConfig.Get().pingMismatchWarningThresholdMs)
					{
						pingWarning.Text = localization.format("PingMismatchWarning", currentPingMs, advertisedPingMs);
						pingWarning.IsVisible = true;
					}
					else
					{
						pingWarning.IsVisible = false;
					}
				}
				else
				{
					pingWarning.IsVisible = false;
				}
#endif // !DEDICATED_SERVER
			}
			else if (PlayerUI.instance != null)
			{
				PlayerUI.instance.Player_OnGUI();
			}
			else if (MenuUI.instance != null)
			{
				MenuUI.instance.Menu_OnGUI();
			}
			else if (EditorUI.instance != null)
			{
				EditorUI.instance.Editor_OnGUI();
			}
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
		}

		private void Start()
		{
			localization = Localization.read("/Menu/MenuLoading.dat");
			loader = gameObject;

			if (Dedicator.IsDedicatedServer)
			{
				Destroy(gameObject);
			}
			else
			{
				if (placeholderCamera == null)
				{
					UnturnedLog.warn("LoadingUI.placeholderCamera is null");
				}
				else
				{
					placeholderCamera.enabled = true;
				}

				window = new SleekWindow();
				window.showTooltips = false;
				window.hackSortOrder = true;
				Glazier.Get().Root = window;

				backgroundImage = Glazier.Get().CreateImage();
				backgroundImage.SizeScale_X = 1;
				backgroundImage.SizeScale_Y = 1;
				window.AddChild(backgroundImage);

				tipLabel = Glazier.Get().CreateLabel();
				tipLabel.PositionOffset_X = 10;
				tipLabel.PositionScale_Y = 1;
				tipLabel.SizeOffset_X = -20;
				tipLabel.SizeOffset_Y = 100;
				tipLabel.SizeScale_X = 1;
				tipLabel.FontSize = ESleekFontSize.Medium;
				tipLabel.AllowRichText = true;
				tipLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				tipLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				tipLabel.TextAlignment = TextAnchor.LowerCenter;
				window.AddChild(tipLabel);

				loadingBarBox = Glazier.Get().CreateBox();
				loadingBarBox.PositionOffset_X = 10;
				loadingBarBox.PositionScale_Y = 1.0f;
				loadingBarBox.SizeOffset_X = -20;
				loadingBarBox.SizeScale_X = 1.0f;
				window.AddChild(loadingBarBox);

				loadingProgressBar = new SleekLoadingScreenProgressBar();
				loadingProgressBar.PositionOffset_X = 10;
				loadingProgressBar.SizeOffset_X = -20;
				loadingProgressBar.SizeOffset_Y = 20;
				loadingProgressBar.SizeScale_X = 1;
				loadingBarBox.AddChild(loadingProgressBar);

				downloadProgressBar = new SleekLoadingScreenProgressBar();
				downloadProgressBar.PositionOffset_X = 10;
				downloadProgressBar.SizeOffset_X = -20;
				downloadProgressBar.SizeOffset_Y = 20;
				downloadProgressBar.SizeScale_X = 1;
				downloadProgressBar.IsVisible = false;
				loadingBarBox.AddChild(downloadProgressBar);

				assetBundleProgressBar = new SleekLoadingScreenProgressBar();
				assetBundleProgressBar.PositionOffset_X = 10;
				assetBundleProgressBar.SizeOffset_X = -20;
				assetBundleProgressBar.SizeOffset_Y = 20;
				assetBundleProgressBar.SizeScale_X = 1;
				assetBundleProgressBar.IsVisible = false;
				loadingBarBox.AddChild(assetBundleProgressBar);

				searchProgressBar = new SleekLoadingScreenProgressBar();
				searchProgressBar.PositionOffset_X = 10;
				searchProgressBar.SizeOffset_X = -20;
				searchProgressBar.SizeOffset_Y = 20;
				searchProgressBar.SizeScale_X = 1;
				searchProgressBar.IsVisible = false;
				loadingBarBox.AddChild(searchProgressBar);

				readProgressBar = new SleekLoadingScreenProgressBar();
				readProgressBar.PositionOffset_X = 10;
				readProgressBar.SizeOffset_X = -20;
				readProgressBar.SizeOffset_Y = 20;
				readProgressBar.SizeScale_X = 1;
				readProgressBar.IsVisible = false;
				loadingBarBox.AddChild(readProgressBar);

				creditsLabel = Glazier.Get().CreateLabel();
				creditsLabel.PositionOffset_X = -250;
				creditsLabel.PositionOffset_Y = -500;
				creditsLabel.PositionScale_X = 0.75f;
				creditsLabel.PositionScale_Y = 0.5f;
				creditsLabel.SizeOffset_X = 500;
				creditsLabel.SizeOffset_Y = 1000;
				creditsLabel.IsVisible = false;
				creditsLabel.AllowRichText = true;
				creditsLabel.TextAlignment = TextAnchor.MiddleCenter;
				creditsLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				creditsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				window.AddChild(creditsLabel);

				pingWarning = Glazier.Get().CreateBox();
				pingWarning.PositionOffset_X = 50.0f;
				pingWarning.PositionOffset_Y = 50.0f;
				pingWarning.SizeOffset_X = -100.0f;
				pingWarning.SizeScale_X = 1.0f;
				pingWarning.SizeOffset_Y = 50.0f;
				pingWarning.IsVisible = false;
				pingWarning.FontSize = ESleekFontSize.Medium;
				pingWarning.Text = localization.format("PingMismatchWarning");
				pingWarning.TextAlignment = TextAnchor.MiddleCenter;
				pingWarning.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				window.AddChild(pingWarning);

				cancelButton = Glazier.Get().CreateButton();
				cancelButton.PositionOffset_X = -110;
				cancelButton.PositionOffset_Y = -50;
				cancelButton.PositionScale_X = 1;
				cancelButton.PositionScale_Y = 1;
				cancelButton.SizeOffset_X = 100;
				cancelButton.SizeOffset_Y = 40;
				cancelButton.FontSize = ESleekFontSize.Medium;
				cancelButton.Text = localization.format("Queue_Cancel");
				cancelButton.TooltipText = localization.format("Queue_Cancel_Tooltip");
				cancelButton.OnClicked += onClickedCancelButton;
				cancelButton.IsVisible = false;
				window.AddChild(cancelButton);

				tip = ELoadingTip.NONE;
				Provider.onQueuePositionUpdated += onQueuePositionUpdated;

				UpdateLoadingBarPositions();
			}
		}

		private void OnDestroy()
		{
			// In the past we destroyed window here, but since LoadingUI is only destroyed during shutdown it is a waste of time.
		}

		private static float animMaxProgress;
		private static float animStart_X;
		private static float animStart_Y;
		private static float animEnd_X;
		private static float animEnd_Y;

		private static void UpdateLoadingBarPositions()
		{
			if (Dedicator.IsDedicatedServer)
				return;

			const float padding = 10;
			float offset_y = padding;

			if (downloadProgressBar.IsVisible)
			{
				downloadProgressBar.PositionOffset_Y = offset_y;
				offset_y += downloadProgressBar.SizeOffset_Y;
				offset_y += padding;
			}

			if (assetBundleProgressBar.IsVisible)
			{
				assetBundleProgressBar.PositionOffset_Y = offset_y;
				offset_y += assetBundleProgressBar.SizeOffset_Y;
				offset_y += padding;
			}

			if (searchProgressBar.IsVisible)
			{
				searchProgressBar.PositionOffset_Y = offset_y;
				offset_y += searchProgressBar.SizeOffset_Y;
				offset_y += padding;
			}

			if (readProgressBar.IsVisible)
			{
				readProgressBar.PositionOffset_Y = offset_y;
				offset_y += readProgressBar.SizeOffset_Y;
				offset_y += padding;
			}

			loadingProgressBar.PositionOffset_Y = offset_y;
			offset_y += loadingProgressBar.SizeOffset_Y;
			offset_y += padding;

			loadingBarBox.SizeOffset_Y = offset_y;
			loadingBarBox.PositionOffset_Y = -offset_y - 10;

			tipLabel.PositionOffset_Y = Mathf.Min(-210, loadingBarBox.PositionOffset_Y - padding - tipLabel.SizeOffset_Y);
		}

		private static void UpdateBackgroundAnim(float progress)
		{
			// Currently server loading goes back and forth a bit
			progress = Mathf.Max(animMaxProgress, progress);
			animMaxProgress = progress;

			backgroundImage.PositionScale_X = Mathf.Lerp(animStart_X, animEnd_X, progress);
			backgroundImage.PositionScale_Y = Mathf.Lerp(animStart_Y, animEnd_Y, progress);
		}

		private static void DisableBackgroundAnim()
		{
			animMaxProgress = 0.0f;
			backgroundImage.PositionScale_X = 0.0f;
			backgroundImage.PositionScale_Y = 0.0f;
			backgroundImage.SizeScale_X = 1.0f;
			backgroundImage.SizeScale_Y = 1.0f;
		}

		private static void EnableBackgroundAnim()
		{
			animMaxProgress = 0.0f;

			const float BACKGROUND_PADDING = 0.01f;

			backgroundImage.SizeScale_X = 1.0f + BACKGROUND_PADDING;
			backgroundImage.SizeScale_Y = 1.0f + BACKGROUND_PADDING;

			if (Random.value < 0.5f)
			{
				// Left to right
				animStart_X = -BACKGROUND_PADDING;
				animEnd_X = 0.0f;
			}
			else
			{
				// Right to left
				animStart_X = 0.0f;
				animEnd_X = -BACKGROUND_PADDING;
			}

			// We do not travel the entire padding distance vertically.
			float verticalDistance = Random.Range(0.0f, BACKGROUND_PADDING);
			float verticalOffset = Random.Range(0.0f, BACKGROUND_PADDING - verticalDistance);
			if (Random.value < 0.5f)
			{
				// Top to bottom
				animStart_Y = -verticalOffset - verticalDistance;
				animEnd_Y = -verticalOffset;
			}
			else
			{
				// Bottom to top
				animStart_Y = -verticalOffset;
				animEnd_Y = -verticalOffset - verticalDistance;
			}

			backgroundImage.PositionScale_X = animStart_X;
			backgroundImage.PositionScale_Y = animStart_Y;
		}
	}
}
