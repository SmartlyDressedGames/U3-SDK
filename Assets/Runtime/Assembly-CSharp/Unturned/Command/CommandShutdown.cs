////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandShutdown : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length > 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			if (components.Length == 0)
			{
				Provider.shutdown();
			}
			else
			{
				int timer;
				if (!int.TryParse(components[0], out timer))
				{
					CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
					return;
				}

				string explanation = "";
				if (components.Length > 1)
				{
					explanation = components[1];
				}

				Provider.shutdown(timer, explanation);
				CommandWindow.LogError(localization.format("ShutdownText", parameter));
			}
		}

		public CommandShutdown(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ShutdownCommandText");
			_info = localization.format("ShutdownInfoText");
			_help = localization.format("ShutdownHelpText");
		}
	}
}
