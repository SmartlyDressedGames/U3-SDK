////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandNpcEvent : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			SteamPlayer player = PlayerTool.getSteamPlayer(executorID);
			if (player == null || player.player == null)
				return;

			NPCEventManager.BroadcastEvent(player.player, parameter, ENPCEventReplicationMode.AuthorityAndClients);
		}

		public CommandNpcEvent(Local newLocalization)
		{
			localization = newLocalization;
			_command = "NpcEvent";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
