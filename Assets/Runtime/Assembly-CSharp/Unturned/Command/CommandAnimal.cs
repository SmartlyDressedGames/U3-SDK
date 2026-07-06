////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandAnimal : Command
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

			if (components.Length < 1 || components.Length > 3)
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

			ushort animalID;
			if (!ushort.TryParse(components[isMe ? 0 : 1], out animalID))
			{
				CommandWindow.LogError(localization.format("InvalidAnimalIDErrorText", components[isMe ? 0 : 1]));
				return;
			}

			if (!AnimalManager.giveAnimal(player.player, animalID))
			{
				CommandWindow.LogError(localization.format("NoAnimalIDErrorText", animalID));
				return;
			}

			CommandWindow.Log(localization.format("AnimalText", player.playerID.playerName, animalID));
		}

		public CommandAnimal(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("AnimalCommandText");
			_info = localization.format("AnimalInfoText");
			_help = localization.format("AnimalHelpText");
		}
	}
}
