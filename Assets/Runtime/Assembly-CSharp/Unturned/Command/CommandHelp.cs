////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandHelp : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (string.IsNullOrEmpty(parameter))
			{
				if (!Dedicator.IsDedicatedServer)
				{
					return;
				}

				CommandWindow.Log(localization.format("HelpText"));

				string commands = "";
				for (int index = 0; index < Commander.commands.Count; index++)
				{
					if (string.IsNullOrEmpty(Commander.commands[index].info))
						continue;

					commands += Commander.commands[index].info;
					if (index < Commander.commands.Count - 1)
					{
						commands += "\n";
					}
				}

				CommandWindow.Log(commands);
			}
			else
			{
				for (int index = 0; index < Commander.commands.Count; index++)
				{
					if (parameter.ToLower() == Commander.commands[index].command.ToLower())
					{
						if (executorID == CSteamID.Nil)
						{
							CommandWindow.Log(Commander.commands[index].info);
							CommandWindow.Log(Commander.commands[index].help);
						}
						else
						{
							ChatManager.say(executorID, Commander.commands[index].info, Palette.SERVER, EChatMode.SAY);
							ChatManager.say(executorID, Commander.commands[index].help, Palette.SERVER, EChatMode.SAY);
						}

						return;
					}
				}

				if (executorID == CSteamID.Nil)
				{
					CommandWindow.Log(localization.format("NoCommandErrorText", parameter));
				}
				else
				{
					ChatManager.say(executorID, localization.format("NoCommandErrorText", parameter), Palette.SERVER, EChatMode.SAY);
				}
			}
		}

		public CommandHelp(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("HelpCommandText");
			_info = localization.format("HelpInfoText");
			_help = localization.format("HelpHelpText");
		}
	}
}
