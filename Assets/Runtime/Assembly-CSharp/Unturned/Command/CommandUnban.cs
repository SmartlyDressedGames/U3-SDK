////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandUnban : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			CSteamID steamID;
			if (!PlayerTool.tryGetSteamID(parameter, out steamID))
			{
				CommandWindow.LogError(localization.format("InvalidSteamIDErrorText", parameter));
				return;
			}

			if (!Provider.requestUnbanPlayer(executorID, steamID))
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", steamID));
				return;
			}

			CommandWindow.Log(localization.format("UnbanText", steamID));
		}

		public CommandUnban(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("UnbanCommandText");
			_info = localization.format("UnbanInfoText");
			_help = localization.format("UnbanHelpText");
		}
	}
}
