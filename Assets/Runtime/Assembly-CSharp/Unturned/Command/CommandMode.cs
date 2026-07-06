////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandMode : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			EGameMode gamemode;
			string mode = parameter.ToLower();

			if (mode == localization.format("ModeEasy").ToLower())
			{
				gamemode = EGameMode.EASY;
			}
			else if (mode == localization.format("ModeNormal").ToLower())
			{
				gamemode = EGameMode.NORMAL;
			}
			else if (mode == localization.format("ModeHard").ToLower())
			{
				gamemode = EGameMode.HARD;
			}
			else
			{
				CommandWindow.LogError(localization.format("NoModeErrorText", mode));
				return;
			}

			if (Provider.isServer)
			{
				CommandWindow.LogError(localization.format("RunningErrorText"));
				return;
			}

			Provider.mode = gamemode;
			CommandWindow.Log(localization.format("ModeText", mode));
		}

		public CommandMode(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ModeCommandText");
			_info = localization.format("ModeInfoText");
			_help = localization.format("ModeHelpText");
		}
	}
}