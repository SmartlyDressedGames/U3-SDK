////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Steamworks;

namespace SDG.Unturned
{
	public class CommandSteamClearAchievement : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			SteamPlayer player = PlayerTool.getSteamPlayer(executorID);
			if (player == null)
				return;

			bool result = SteamUserStats.ClearAchievement(parameter);
			UnturnedLog.info($"Clear achievement \"{parameter}\" result: {result}");
		}

		public CommandSteamClearAchievement(Local newLocalization)
		{
			localization = newLocalization;
			_command = "SteamClearAchievement";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
