////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandPermits : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (SteamWhitelist.list.Count == 0)
			{
				CommandWindow.LogError(localization.format("NoPermitsErrorText"));
				return;
			}

			CommandWindow.Log(localization.format("PermitsText"));
			for (int index = 0; index < SteamWhitelist.list.Count; index++)
			{
				SteamWhitelistID id = SteamWhitelist.list[index];

				CommandWindow.Log(localization.format("PermitNameText", id.steamID, id.tag));
				CommandWindow.Log(localization.format("PermitJudgeText", id.judgeID));
			}
		}

		public CommandPermits(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("PermitsCommandText");
			_info = localization.format("PermitsInfoText");
			_help = localization.format("PermitsHelpText");
		}
	}
}