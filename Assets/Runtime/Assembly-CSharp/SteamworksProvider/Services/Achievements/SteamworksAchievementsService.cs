////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Achievements;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Achievements
{
	public class SteamworksAchievementsService : Service, IAchievementsService
	{
		public bool getAchievement(string name, out bool has)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			bool success = SteamUserStats.GetAchievement(name, out has);
			if (!success)
			{
				SDG.Unturned.UnturnedLog.error($"Failed to get Steam achievement \"{name}\" status");
			}
			return success;
		}

		public bool setAchievement(string name)
		{
			if (name == null)
			{
				throw new System.ArgumentNullException("name");
			}

			bool success = SteamUserStats.SetAchievement(name);
			if (success)
			{
				SDG.Unturned.UnturnedLog.info($"Unlocked Steam achievement \"{name}\"");
			}
			else
			{
				// Nelson 2025-03-19: the Steamworks SDK was changed to return false when the achievement is already
				// unlocked, so we no longer log an here.
			}

			SteamUserStats.StoreStats();
			return success;
		}
	}
}
