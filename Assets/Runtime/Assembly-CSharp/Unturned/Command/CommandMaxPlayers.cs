////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandMaxPlayers : Command
	{
		public static readonly byte MIN_NUMBER = 1;
		[System.Obsolete]
		public static readonly byte RECOMMENDED_NUMBER = 24;
		public static readonly byte MAX_NUMBER = 200;

		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			byte maxPlayers;
			if (!byte.TryParse(parameter, out maxPlayers))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			if (maxPlayers < MIN_NUMBER)
			{
				CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
				return;
			}

			if (maxPlayers > MAX_NUMBER)
			{
				CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
				return;
			}

			Provider.maxPlayers = maxPlayers;
			CommandWindow.Log(localization.format("MaxPlayersText", maxPlayers));
		}

		public CommandMaxPlayers(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("MaxPlayersCommandText");
			_info = localization.format("MaxPlayersInfoText");
			_help = localization.format("MaxPlayersHelpText");
		}
	}
}
