////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandPermit : Command
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

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			CSteamID steamID;
			if (!PlayerTool.tryGetSteamID(components[0], out steamID))
			{
				CommandWindow.LogError(localization.format("InvalidSteamIDErrorText", components[0]));
				return;
			}

			SteamWhitelist.whitelist(steamID, components[1], executorID);
			CommandWindow.Log(localization.format("PermitText", steamID, components[1]));
		}

		public CommandPermit(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("PermitCommandText");
			_info = localization.format("PermitInfoText");
			_help = localization.format("PermitHelpText");
		}
	}
}