////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandLog : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 4)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			bool chat = true;
			if (components[0].ToLower() == "y")
			{
				chat = true;
			}
			else if (components[0].ToLower() == "n")
			{
				chat = false;
			}
			else
			{
				CommandWindow.LogError(localization.format("InvalidBooleanErrorText", components[0]));
				return;
			}

			bool join = true;
			if (components[1].ToLower() == "y")
			{
				join = true;
			}
			else if (components[1].ToLower() == "n")
			{
				join = false;
			}
			else
			{
				CommandWindow.LogError(localization.format("InvalidBooleanErrorText", components[1]));
				return;
			}

			bool death = true;
			if (components[2].ToLower() == "y")
			{
				death = true;
			}
			else if (components[2].ToLower() == "n")
			{
				death = false;
			}
			else
			{
				CommandWindow.LogError(localization.format("InvalidBooleanErrorText", components[2]));
				return;
			}

			bool anticheat = true;
			if (components[3].ToLower() == "y")
			{
				anticheat = true;
			}
			else if (components[3].ToLower() == "n")
			{
				anticheat = false;
			}
			else
			{
				CommandWindow.LogError(localization.format("InvalidBooleanErrorText", components[3]));
				return;
			}

			CommandWindow.shouldLogChat = chat;
			CommandWindow.shouldLogJoinLeave = join;
			CommandWindow.shouldLogDeaths = death;
			CommandWindow.shouldLogAnticheat = anticheat;
			CommandWindow.Log(localization.format("LogText"));
		}

		public CommandLog(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("LogCommandText");
			_info = localization.format("LogInfoText");
			_help = localization.format("LogHelpText");
		}
	}
}