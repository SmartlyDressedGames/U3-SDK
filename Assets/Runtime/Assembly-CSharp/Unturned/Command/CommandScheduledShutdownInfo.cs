////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandScheduledShutdownInfo : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (executorID != CSteamID.Nil)
				return;

			if (Provider.autoShutdownManager.isScheduledShutdownEnabled)
			{
				CommandWindow.Log($"Shutdown is scheduled for {Provider.autoShutdownManager.scheduledShutdownTime.ToLocalTime()} ({Provider.autoShutdownManager.scheduledShutdownTime - System.DateTime.UtcNow:g} from now)");
			}
			else
			{
				CommandWindow.Log("Scheduled shutdown is disabled");
			}
		}

		public CommandScheduledShutdownInfo(Local newLocalization)
		{
			localization = newLocalization;
			_command = "ScheduledShutdownInfo";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
