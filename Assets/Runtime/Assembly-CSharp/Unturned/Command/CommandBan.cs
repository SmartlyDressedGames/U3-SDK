////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandBan : Command
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

			if (components.Length != 1 && components.Length != 2 && components.Length != 3)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			CSteamID steamID;
			if (!PlayerTool.tryGetSteamID(components[0], out steamID))
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
				return;
			}

			NetTransport.ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
			uint ip = 0;
			if (transportConnection != null)
			{
				transportConnection.TryGetIPv4Address(out ip);
			}

			System.Collections.Generic.IEnumerable<byte[]> hwids;
			// Can be null if ban command was used while target player is offline.
			SteamPlayer onlineClient = PlayerTool.getSteamPlayer(steamID);
			if (onlineClient != null)
			{
				hwids = onlineClient.playerID.GetHwids();
			}
			else
			{
				hwids = null;
			}

			if (components.Length == 1)
			{
				Provider.requestBanPlayer(executorID, steamID, ip, hwids, localization.format("BanTextReason"), SteamBlacklist.PERMANENT);
				CommandWindow.Log(localization.format("BanTextPermanent", steamID));
			}
			else if (components.Length == 2)
			{
				Provider.requestBanPlayer(executorID, steamID, ip, hwids, components[1], SteamBlacklist.PERMANENT);
				CommandWindow.Log(localization.format("BanTextPermanent", steamID));
			}
			else
			{
				uint duration;
				if (!uint.TryParse(components[2], out duration))
				{
					CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[2]));
					return;
				}

				Provider.requestBanPlayer(executorID, steamID, ip, hwids, components[1], duration);
				CommandWindow.Log(localization.format("BanText", steamID, duration));
			}
		}

		public CommandBan(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("BanCommandText");
			_info = localization.format("BanInfoText");
			_help = localization.format("BanHelpText");
		}
	}
}
