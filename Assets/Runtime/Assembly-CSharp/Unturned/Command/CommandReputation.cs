////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandReputation : Command
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

			if (components.Length < 1 || components.Length > 2)
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

			int reputation;
			if (!int.TryParse(components[isMe ? 0 : 1], out reputation))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[isMe ? 0 : 1]));
				return;
			}

			player.player.skills.askRep(reputation);

			string text = reputation.ToString();
			if (reputation > 0)
			{
				text = '+' + text;
			}

			CommandWindow.Log(localization.format("ReputationText", player.playerID.playerName, text));
		}

		public CommandReputation(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ReputationCommandText");
			_info = localization.format("ReputationInfoText");
			_help = localization.format("ReputationHelpText");
		}
	}
}