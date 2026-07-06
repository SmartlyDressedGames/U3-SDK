////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandAdmin : Command
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
				CommandWindow.LogError(localization.format("NoPlayerErrorText", parameter));
				return;
			}

			SteamAdminlist.admin(steamID, executorID);
			CommandWindow.Log(localization.format("AdminText", steamID));
		}

		public CommandAdmin(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("AdminCommandText");
			_info = localization.format("AdminInfoText");
			_help = localization.format("AdminHelpText");
		}
	}
}