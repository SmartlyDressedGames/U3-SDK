////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandCycle : Command
	{
		//private static readonly uint MIN_NUMBER = 0;
		//private static readonly uint MAX_NUMBER = 86400;

		protected override void execute(CSteamID executorID, string parameter)
		{
			uint cycle;
			if (!uint.TryParse(parameter, out cycle))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
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

			//		 if(cycle < MIN_NUMBER)
			//{
			//	CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
			//	return;
			//}

			//if(cycle > MAX_NUMBER)
			//{
			//	CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
			//	return;
			//}

			LightingManager.cycle = cycle;

			CommandWindow.Log(localization.format("CycleText", cycle));
		}

		public CommandCycle(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("CycleCommandText");
			_info = localization.format("CycleInfoText");
			_help = localization.format("CycleHelpText");
		}
	}
}