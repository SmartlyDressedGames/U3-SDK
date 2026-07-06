////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Achievements;
using SDG.Provider.Services.Browser;
using SDG.Provider.Services.Cloud;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Statistics;
using SDG.Provider.Services.Store;
using SDG.Provider.Services.Translation;
using System;

namespace SDG.Provider
{
	public interface IProvider
	{
		/// <summary>
		/// Current achievements implementation.
		/// </summary>
		IAchievementsService achievementsService
		{
			get;
		}

		/// <summary>
		/// Current browser implementation.
		/// </summary>
		IBrowserService browserService
		{
			get;
		}

		/// <summary>
		/// Current cloud implementation.
		/// </summary>
		ICloudService cloudService
		{
			get;
		}

		/// <summary>
		/// Current community implementation.
		/// </summary>
		ICommunityService communityService
		{
			get;
		}

		/// <summary>
		/// Current economy implementation.
		/// </summary>
		TempSteamworksEconomy economyService
		{
			get;
		}

		/// <summary>
		/// Current matchmaking implementation.
		/// </summary>
		TempSteamworksMatchmaking matchmakingService
		{
			get;
		}

		/// <summary>
		/// Current multiplayer implementation.
		/// </summary>
		IMultiplayerService multiplayerService
		{
			get;
		}

		/// <summary>
		/// Current statistics implementation.
		/// </summary>
		IStatisticsService statisticsService
		{
			get;
		}

		/// <summary>
		/// Current store implementation.
		/// </summary>
		IStoreService storeService
		{
			get;
		}

		/// <summary>
		/// Current translation implementation.
		/// </summary>
		ITranslationService translationService
		{
			get;
		}

		/// <summary>
		/// Current workshop implementation.
		/// </summary>
		TempSteamworksWorkshop workshopService
		{
			get;
		}

		/// <summary>
		/// Initialize this provider's external API. Should be called before using provider features.
		/// </summary>
		/// <exception cref="Exception">Thrown if initializing fails.</exception>
		void intialize();

		/// <summary>
		/// Update this provider's external API. Should be called every frame if using provider features.
		/// </summary>
		void update();

		/// <summary>
		/// Shutdown this provider's external API. Should be called before closing the program if using provider features.
		/// </summary>
		void shutdown();
	}
}
