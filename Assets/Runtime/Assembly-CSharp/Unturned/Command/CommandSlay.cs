////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandSlay : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 1 && components.Length != 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player))
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
				return;
			}

			uint ip = player.getIPv4AddressOrZero();
			if (components.Length == 1)
			{
				Provider.requestBanPlayer(executorID, player.playerID.steamID, ip, player.playerID.GetHwids(), localization.format("SlayTextReason"), SteamBlacklist.PERMANENT);
			}
			else if (components.Length == 2)
			{
				Provider.requestBanPlayer(executorID, player.playerID.steamID, ip, player.playerID.GetHwids(), components[1], SteamBlacklist.PERMANENT);
			}

			if (player.player != null)
			{
				EPlayerKill kill;
				player.player.life.askDamage(101, Vector3.up * 101, EDeathCause.KILL, ELimb.SKULL, executorID, out kill);
			}

			CommandWindow.Log(localization.format("SlayText", player.playerID.playerName));
		}

		public CommandSlay(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("SlayCommandText");
			_info = localization.format("SlayInfoText");
			_help = localization.format("SlayHelpText");
		}
	}
}
