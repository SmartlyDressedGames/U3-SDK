////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandOwner : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (Provider.isServer)
			{
				CommandWindow.LogError(localization.format("RunningErrorText"));
				return;
			}

			CSteamID steamID;
			if (!PlayerTool.tryGetSteamID(parameter, out steamID))
			{
				CommandWindow.LogError(localization.format("InvalidSteamIDErrorText", parameter));
				return;
			}

			SteamAdminlist.ownerID = steamID;
			CommandWindow.Log(localization.format("OwnerText", steamID));
		}

		public CommandOwner(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("OwnerCommandText");
			_info = localization.format("OwnerInfoText");
			_help = localization.format("OwnerHelpText");
		}
	}
}