////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandKick : Command
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

			if (components.Length != 1 && components.Length != 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player))
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
				return;
			}

			if (components.Length == 1)
			{
				Provider.kick(player.playerID.steamID, localization.format("KickTextReason"));
			}
			else if (components.Length == 2)
			{
				Provider.kick(player.playerID.steamID, components[1]);
			}

			CommandWindow.Log(localization.format("KickText", player.playerID.playerName));
		}

		public CommandKick(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("KickCommandText");
			_info = localization.format("KickInfoText");
			_help = localization.format("KickHelpText");
		}
	}
}