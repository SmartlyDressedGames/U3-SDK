////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandBans : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (SteamBlacklist.list.Count == 0)
			{
				CommandWindow.LogError(localization.format("NoBansErrorText"));
				return;
			}

			CommandWindow.Log(localization.format("BansText"));
			for (int index = 0; index < SteamBlacklist.list.Count; index++)
			{
				SteamBlacklistID id = SteamBlacklist.list[index];

				CommandWindow.Log(localization.format("BanNameText", id.playerID));
				CommandWindow.Log(localization.format("BanJudgeText", id.judgeID));
				CommandWindow.Log(localization.format("BanStatusText", id.reason, id.duration, id.getTime()));
			}
		}

		public CommandBans(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("BansCommandText");
			_info = localization.format("BansInfoText");
			_help = localization.format("BansHelpText");
		}
	}
}