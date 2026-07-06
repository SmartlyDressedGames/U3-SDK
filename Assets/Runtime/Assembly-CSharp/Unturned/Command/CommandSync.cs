////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandSync : Command
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

			PlayerSavedata.hasSync = true;
			CommandWindow.Log(localization.format("SyncText"));
		}

		public CommandSync(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("SyncCommandText");
			_info = localization.format("SyncInfoText");
			_help = localization.format("SyncHelpText");
		}
	}
}