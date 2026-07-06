////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Statistics;
using SDG.Provider.Services.Statistics.Global;
using SDG.Provider.Services.Statistics.User;
using SDG.SteamworksProvider.Services.Statistics.Global;
using SDG.SteamworksProvider.Services.Statistics.User;

namespace SDG.SteamworksProvider.Services.Statistics
{
	public class SteamworksStatisticsService : IStatisticsService
	{
		public IUserStatisticsService userStatisticsService
		{
			get;
			protected set;
		}

		public IGlobalStatisticsService globalStatisticsService
		{
			get;
			protected set;
		}

		public void initialize()
		{
			userStatisticsService.initialize();
			globalStatisticsService.initialize();
		}

		public void update()
		{
			userStatisticsService.update();
			globalStatisticsService.update();
		}

		public void shutdown()
		{
			userStatisticsService.shutdown();
			globalStatisticsService.shutdown();
		}

		public SteamworksStatisticsService() : base()
		{
			userStatisticsService = new SteamworksUserStatisticsService();
			globalStatisticsService = new SteamworksGlobalStatisticsService();
		}
	}
}