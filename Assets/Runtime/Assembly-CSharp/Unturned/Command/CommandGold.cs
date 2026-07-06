////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandGold : Command
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

			Provider.isGold = true;
			CommandWindow.Log(localization.format("GoldText"));
		}

		public CommandGold(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("GoldCommandText");
			_info = localization.format("GoldInfoText");
			_help = localization.format("GoldHelpText");
		}
	}
}