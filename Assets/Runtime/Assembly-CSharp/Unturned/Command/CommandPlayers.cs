////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandPlayers : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (Provider.clients.Count == 0)
			{
				CommandWindow.LogError(localization.format("NoPlayersErrorText"));
				return;
			}

			CommandWindow.Log(localization.format("PlayersText"));
			for (int index = 0; index < Provider.clients.Count; index++)
			{
				SteamPlayer player = Provider.clients[index];

				CommandWindow.Log(localization.format("PlayerIDText", player.playerID.steamID, player.playerID.playerName, player.playerID.characterName, (int) (player.ping * 1000)));
			}
		}

		public CommandPlayers(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("PlayersCommandText");
			_info = localization.format("PlayersInfoText");
			_help = localization.format("PlayersHelpText");
		}
	}
}