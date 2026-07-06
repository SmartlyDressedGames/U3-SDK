////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandFilter : Command
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

			Provider.filterName = true;
			CommandWindow.Log(localization.format("FilterText"));
		}

		public CommandFilter(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("FilterCommandText");
			_info = localization.format("FilterInfoText");
			_help = localization.format("FilterHelpText");
		}
	}
}