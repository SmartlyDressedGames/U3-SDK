////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandFlag : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			if (!Provider.hasCheats)
			{
				CommandWindow.LogError(localization.format("CheatsErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length < 2 || components.Length > 3)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			bool isMe = false;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player))
			{
				player = PlayerTool.getSteamPlayer(executorID);

				if (player == null)
				{
					CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
					return;
				}
				else
				{
					isMe = true;
				}
			}

			ushort flag;
			if (!ushort.TryParse(components[isMe ? 0 : 1], out flag))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[isMe ? 0 : 1]));
				return;
			}

			short value;
			if (!short.TryParse(components[isMe ? 1 : 2], out value))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[isMe ? 1 : 2]));
				return;
			}

			player.player.quests.sendSetFlag(flag, value);

			CommandWindow.Log(localization.format("FlagText", player.playerID.playerName, flag, value));
		}

		public CommandFlag(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("FlagCommandText");
			_info = localization.format("FlagInfoText");
			_help = localization.format("FlagHelpText");
		}
	}
}