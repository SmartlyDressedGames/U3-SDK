////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandKill : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			SteamPlayer player;
			if (!PlayerTool.tryGetSteamPlayer(parameter, out player))
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", parameter));
				return;
			}

			if (player.player != null)
			{
				EPlayerKill kill;
				player.player.life.askDamage(101, Vector3.up * 101, EDeathCause.KILL, ELimb.SKULL, executorID, out kill);
			}

			CommandWindow.Log(localization.format("KillText", player.playerID.playerName));
		}

		public CommandKill(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("KillCommandText");
			_info = localization.format("KillInfoText");
			_help = localization.format("KillHelpText");
		}
	}
}