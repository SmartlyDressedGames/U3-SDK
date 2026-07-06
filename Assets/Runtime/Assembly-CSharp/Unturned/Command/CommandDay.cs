////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandDay : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			if (Provider.isServer && Level.info.type == ELevelType.HORDE)
			{
				CommandWindow.LogError(localization.format("HordeErrorText"));
				return;
			}

			if (Provider.isServer && Level.info.type == ELevelType.ARENA)
			{
				CommandWindow.LogError(localization.format("ArenaErrorText"));
				return;
			}

			LightingManager.time = (uint) (LightingManager.cycle * LevelLighting.transition);

			CommandWindow.Log(localization.format("DayText"));
		}

		public CommandDay(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("DayCommandText");
			_info = localization.format("DayInfoText");
			_help = localization.format("DayHelpText");
		}
	}
}