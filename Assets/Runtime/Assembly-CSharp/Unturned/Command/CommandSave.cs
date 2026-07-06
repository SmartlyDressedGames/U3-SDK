////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandSave : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			SaveManager.save();

			CommandWindow.Log(localization.format("SaveText"));
		}

		public CommandSave(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("SaveCommandText");
			_info = localization.format("SaveInfoText");
			_help = localization.format("SaveHelpText");
		}
	}
}