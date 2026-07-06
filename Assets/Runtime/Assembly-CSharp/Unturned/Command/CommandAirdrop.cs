////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandAirdrop : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!LevelManager.hasAirdrop)
			{
				return;
			}

			LevelManager.airdropFrequency = 0;

			CommandWindow.Log(localization.format("AirdropText"));
		}

		public CommandAirdrop(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("AirdropCommandText");
			_info = localization.format("AirdropInfoText");
			_help = localization.format("AirdropHelpText");
		}
	}
}