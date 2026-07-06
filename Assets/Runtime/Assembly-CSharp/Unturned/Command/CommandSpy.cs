////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandSpy : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 1)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player) || player.player == null)
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
				return;
			}

			player.player.sendScreenshot(executorID);
			CommandWindow.Log(localization.format("SpyText", player.playerID.playerName));
		}

		public CommandSpy(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("SpyCommandText");
			_info = localization.format("SpyInfoText");
			_help = localization.format("SpyHelpText");
		}
	}
}