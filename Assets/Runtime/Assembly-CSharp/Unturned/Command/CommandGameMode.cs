////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	/// <summary>
	/// Essentially deprecated for now.
	/// </summary>
	public class CommandGameMode : Command
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

			// Provider.selectedGameModeName = parameter;
			CommandWindow.Log(localization.format("GameModeText", parameter));
		}

		public CommandGameMode(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("GameModeCommandText");
			_info = localization.format("GameModeInfoText");
			_help = localization.format("GameModeHelpText");
		}
	}
}
