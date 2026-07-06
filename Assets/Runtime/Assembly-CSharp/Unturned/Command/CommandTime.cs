////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandTime : Command
	{
		//private static readonly uint MIN_NUMBER = 0;
		//private static readonly uint MAX_NUMBER = 86400;

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

			uint time;
			if (!uint.TryParse(parameter, out time))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			//if(time < MIN_NUMBER)
			//{
			//	CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
			//	return;
			//}

			//if(time > MAX_NUMBER)
			//{
			//	CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
			//	return;
			//}

			LightingManager.time = time;

			CommandWindow.Log(localization.format("TimeText", time));
		}

		public CommandTime(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("TimeCommandText");
			_info = localization.format("TimeInfoText");
			_help = localization.format("TimeHelpText");
		}
	}
}