////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandUnadmin : Command
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

			SteamAdminlist.unadmin(steamID);
			CommandWindow.Log(localization.format("UnadminText", steamID));
		}

		public CommandUnadmin(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("UnadminCommandText");
			_info = localization.format("UnadminInfoText");
			_help = localization.format("UnadminHelpText");
		}
	}
}