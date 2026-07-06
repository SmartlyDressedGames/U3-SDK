////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandPort : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			ushort port;
			if (!ushort.TryParse(parameter, out port))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			if (Provider.isServer)
			{
				CommandWindow.LogError(localization.format("RunningErrorText"));
				return;
			}

			Provider.port = port;
			CommandWindow.Log(localization.format("PortText", port));
		}

		public CommandPort(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("PortCommandText");
			_info = localization.format("PortInfoText");
			_help = localization.format("PortHelpText");
		}
	}
}