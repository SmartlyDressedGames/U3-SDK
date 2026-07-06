////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandPvE : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (Provider.isServer)
			{
				CommandWindow.LogError(localization.format("RunningErrorText"));
				return;
			}

			Provider.isPvP = false;
			CommandWindow.Log(localization.format("PvEText"));
		}

		public CommandPvE(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("PvECommandText");
			_info = localization.format("PvEInfoText");
			_help = localization.format("PvEHelpText");
		}
	}
}