////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandUnpermit : Command
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

			if (!SteamWhitelist.unwhitelist(steamID))
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", steamID));
				return;
			}

			CommandWindow.Log(localization.format("UnpermitText", steamID));
		}

		public CommandUnpermit(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("UnpermitCommandText");
			_info = localization.format("UnpermitInfoText");
			_help = localization.format("UnpermitHelpText");
		}
	}
}