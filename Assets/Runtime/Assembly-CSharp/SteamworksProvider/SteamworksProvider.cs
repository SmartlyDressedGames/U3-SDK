////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using SDG.Provider.Services.Achievements;
using SDG.Provider.Services.Browser;
using SDG.Provider.Services.Cloud;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Statistics;
using SDG.Provider.Services.Store;
using SDG.Provider.Services.Translation;
using SDG.SteamworksProvider.Services.Achievements;
using SDG.SteamworksProvider.Services.Browser;
using SDG.SteamworksProvider.Services.Cloud;
using SDG.SteamworksProvider.Services.Community;
using SDG.SteamworksProvider.Services.Multiplayer;
using SDG.SteamworksProvider.Services.Statistics;
using SDG.SteamworksProvider.Services.Store;
using SDG.SteamworksProvider.Services.Translation;
using Steamworks;
using System;

namespace SDG.SteamworksProvider
{
	public class SteamworksProvider : IProvider
	{
		public IAchievementsService achievementsService
		{
			get;
			protected set;
		}

		public IBrowserService browserService
		{
			get;
			protected set;
		}

		public ICloudService cloudService
		{
			get;
			protected set;
		}

		public ICommunityService communityService
		{
			get;
			protected set;
		}

		public TempSteamworksEconomy economyService
		{
			get;
			protected set;
		}

		public TempSteamworksMatchmaking matchmakingService
		{
			get;
			protected set;
		}

		public IMultiplayerService multiplayerService
		{
			get;
			protected set;
		}

		public IStatisticsService statisticsService
		{
			get;
			protected set;
		}

		public IStoreService storeService
		{
			get;
			protected set;
		}

		public ITranslationService translationService
		{
			get;
			protected set;
		}

		public TempSteamworksWorkshop workshopService
		{
			get;
			protected set;
		}

		private SteamworksAppInfo appInfo;

		public void intialize()
		{
			if (!appInfo.isDedicated)
			{
				if (SteamAPI.RestartAppIfNecessary((AppId_t) appInfo.id))
				{
					throw new Exception("Restarting app from Steam.");
				}

				if (!SteamAPI.Init())
				{
					throw new Exception("Steam API initialization failed.");
				}

#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
				string appInstallDir;
				uint appInstallDirLength = SteamApps.GetAppInstallDir((AppId_t) appInfo.id, out appInstallDir, /*bufferSize*/ 1024);
				if (appInstallDirLength > 0)
				{
					SDG.Unturned.Provider.steamAppInstallDirectory = new System.IO.DirectoryInfo(appInstallDir);
					if (!SDG.Unturned.Provider.steamAppInstallDirectory.Exists)
					{
						SDG.Unturned.UnturnedLog.warn($"Steam app install directory \"{appInstallDir}\" does not exist");
						SDG.Unturned.Provider.steamAppInstallDirectory = null;
					}
				}
				else
				{
					SDG.Unturned.UnturnedLog.info("Unable to get Steam app install directory");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
			}

			initializeServices();
		}

		public void update()
		{
			if (multiplayerService.serverMultiplayerService.isHosting)
			{
				UnityEngine.Profiling.Profiler.BeginSample("GS Callbacks");
				GameServer.RunCallbacks();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (!appInfo.isDedicated)
			{
				UnityEngine.Profiling.Profiler.BeginSample("CL Callbacks");
				SteamAPI.RunCallbacks();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			UnityEngine.Profiling.Profiler.BeginSample("Services");
			updateServices();
			UnityEngine.Profiling.Profiler.EndSample();
		}

		public void shutdown()
		{
			if (!appInfo.isDedicated)
			{
				SteamAPI.Shutdown();
			}

			shutdownServices();
		}

		private void constructServices()
		{
			achievementsService = new SteamworksAchievementsService();
			economyService = new TempSteamworksEconomy(appInfo); // new SteamworksEconomyService();
			multiplayerService = new SteamworksMultiplayerService(appInfo);
			statisticsService = new SteamworksStatisticsService();
			workshopService = new TempSteamworksWorkshop(appInfo);// new SteamworksWorkshopService();

			if (!appInfo.isDedicated)
			{
				browserService = new SteamworksBrowserService();
				cloudService = new SteamworksCloudService();
				communityService = new SteamworksCommunityService();
				matchmakingService = new TempSteamworksMatchmaking();// new SteamworksMatchmakingService();
				storeService = new SteamworksStoreService(appInfo);
				translationService = new SteamworksTranslationService();
			}
		}

		private void initializeServices()
		{
			if (achievementsService != null)
			{
				achievementsService.initialize();
			}

			if (economyService != null)
			{
				economyService.initialize();
			}

			if (multiplayerService != null)
			{
				multiplayerService.initialize();
			}

			if (statisticsService != null)
			{
				statisticsService.initialize();
			}

			//workshopService.initialize();

			if (!appInfo.isDedicated)
			{
				if (browserService != null)
				{
					browserService.initialize();
				}

				if (cloudService != null)
				{
					cloudService.initialize();
				}

				if (communityService != null)
				{
					communityService.initialize();
				}

				if (economyService != null)
				{
					economyService.initializeClient();
				}

				//matchmakingService.initialize();

				if (storeService != null)
				{
					storeService.initialize();
				}

				if (translationService != null)
				{
					translationService.initialize();
				}
			}
		}

		private void updateServices()
		{
			if (achievementsService != null)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Achievements");
				achievementsService.update();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			//economyService.update();

			if (multiplayerService != null)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Multiplayer");
				multiplayerService.update();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (statisticsService != null)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Statistics");
				statisticsService.update();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (workshopService != null)
			{
				workshopService.update();
			}

			if (!appInfo.isDedicated)
			{
				if (browserService != null)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Browser");
					browserService.update();
					UnityEngine.Profiling.Profiler.EndSample();
				}

				if (cloudService != null)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Cloud");
					cloudService.update();
					UnityEngine.Profiling.Profiler.EndSample();
				}

				if (communityService != null)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Community");
					communityService.update();
					UnityEngine.Profiling.Profiler.EndSample();
				}

				//matchmakingService.update();

				if (storeService != null)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Store");
					storeService.update();
					UnityEngine.Profiling.Profiler.EndSample();
				}

				if (translationService != null)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Translation");
					translationService.update();
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}
		}

		private void shutdownServices()
		{
			if (achievementsService != null)
			{
				achievementsService.shutdown();
			}

			//economyService.shutdown();

			if (multiplayerService != null)
			{
				multiplayerService.shutdown();
			}

			if (statisticsService != null)
			{
				statisticsService.shutdown();
			}

			//workshopService.shutdown();

			if (!appInfo.isDedicated)
			{
				if (browserService != null)
				{
					browserService.shutdown();
				}

				if (cloudService != null)
				{
					cloudService.shutdown();
				}

				if (communityService != null)
				{
					communityService.shutdown();
				}

				//matchmakingService.shutdown();

				if (storeService != null)
				{
					storeService.shutdown();
				}

				if (translationService != null)
				{
					translationService.shutdown();
				}
			}
		}

		public SteamworksProvider(SteamworksAppInfo newAppInfo)
		{
			appInfo = newAppInfo;

			constructServices();
		}
	}
}
