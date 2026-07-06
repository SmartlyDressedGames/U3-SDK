////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandExperience : Command
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

			uint experience;
			if (!uint.TryParse(components[isMe ? 0 : 1], out experience))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[isMe ? 0 : 1]));
				return;
			}

			player.player.skills.askAward(experience);

			CommandWindow.Log(localization.format("ExperienceText", player.playerID.playerName, experience));
		}

		public CommandExperience(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ExperienceCommandText");
			_info = localization.format("ExperienceInfoText");
			_help = localization.format("ExperienceHelpText");
		}
	}
}