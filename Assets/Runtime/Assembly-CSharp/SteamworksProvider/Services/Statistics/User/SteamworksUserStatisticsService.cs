////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Statistics.User;
using SDG.SteamworksProvider.Services.Community;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Statistics.User
{
	public class SteamworksUserStatisticsService : Service, IUserStatisticsService
	{
		public event UserStatisticsRequestReady onUserStatisticsRequestReady;

#pragma warning disable
		private Callback<UserStatsReceived_t> userStatsReceivedCallback;
#pragma warning restore

		private void triggerUserStatisticsRequestReady(ICommunityEntity entityID)
		{
			onUserStatisticsRequestReady?.Invoke(entityID);
		}

		public bool getStatistic(string name, out int data)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			return SteamUserStats.GetStat(name, out data);
		}

		public bool setStatistic(string name, int data)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			bool success = SteamUserStats.SetStat(name, data);
			SteamUserStats.StoreStats();
			return success;
		}

		public bool getStatistic(string name, out float data)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			return SteamUserStats.GetStat(name, out data);
		}

		public bool setStatistic(string name, float data)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			bool success = SteamUserStats.SetStat(name, data);
			SteamUserStats.StoreStats();
			return success;
		}

		public bool requestStatistics()
		{
			SteamUserStats.RequestCurrentStats();
			return true;
		}

		public SteamworksUserStatisticsService() : base()
		{
			userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(onUserStatsReceived);
		}

		private void onUserStatsReceived(UserStatsReceived_t callback)
		{
			if (callback.m_nGameID != SteamUtils.GetAppID().m_AppId)
			{
				return;
			}

			UnityEngine.Profiling.Profiler.BeginSample("onUserStatsReceived");
			SteamworksCommunityEntity steamworksEntityID = new SteamworksCommunityEntity(callback.m_steamIDUser);
			triggerUserStatisticsRequestReady(steamworksEntityID);
			UnityEngine.Profiling.Profiler.EndSample();
		}
	}
}