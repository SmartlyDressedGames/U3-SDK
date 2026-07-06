////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Statistics.Global;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Statistics.Global
{
	public class SteamworksGlobalStatisticsService : Service, IGlobalStatisticsService
	{
		public event GlobalStatisticsRequestReady onGlobalStatisticsRequestReady;

		private void triggerGlobalStatisticsRequestReady()
		{
			onGlobalStatisticsRequestReady?.Invoke();
		}

		public bool getStatistic(string name, out long data)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			return SteamUserStats.GetGlobalStat(name, out data);
		}

		public bool getStatistic(string name, out double data)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			return SteamUserStats.GetGlobalStat(name, out data);
		}

		public bool requestStatistics()
		{
			SteamUserStats.RequestGlobalStats(0);
			return true;
		}

		public SteamworksGlobalStatisticsService()
		{
			globalStatsReceived = Callback<GlobalStatsReceived_t>.Create(onGlobalStatsReceived);
		}

#pragma warning disable
		private Callback<GlobalStatsReceived_t> globalStatsReceived;
#pragma warning restore

		private void onGlobalStatsReceived(GlobalStatsReceived_t callback)
		{
			if (callback.m_nGameID != SteamUtils.GetAppID().m_AppId)
			{
				return;
			}

			UnityEngine.Profiling.Profiler.BeginSample("onGlobalStatsReceived");
			triggerGlobalStatisticsRequestReady();
			UnityEngine.Profiling.Profiler.EndSample();
		}
	}
}