////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandSay : Command
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

			if (components.Length != 1 && components.Length != 4)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			if (components.Length == 1)
			{
				ChatManager.say(components[0], Palette.SERVER);
			}
			else if (components.Length == 4)
			{
				byte r;
				if (!byte.TryParse(components[1], out r))
				{
					CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[0]));
					return;
				}

				byte g;
				if (!byte.TryParse(components[2], out g))
				{
					CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[1]));
					return;
				}

				byte b;
				if (!byte.TryParse(components[3], out b))
				{
					CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[2]));
					return;
				}

				ChatManager.say(components[0], new Color(r / 255f, g / 255f, b / 255f));
			}
		}

		public CommandSay(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("SayCommandText");
			_info = localization.format("SayInfoText");
			_help = localization.format("SayHelpText");
		}
	}
}