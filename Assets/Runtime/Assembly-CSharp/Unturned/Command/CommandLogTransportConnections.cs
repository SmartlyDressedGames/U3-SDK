////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned
{
	public class CommandLogTransportConnections : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (executorID != CSteamID.Nil)
				return;

			foreach (SteamPlayer client in Provider.clients)
			{
				ITransportConnection transportConnection = client.transportConnection;
				if (transportConnection == null)
				{
					CommandWindow.Log($"Client {client.playerID} has no transport connection");
					continue;
				}

				CommandWindow.Log($"{transportConnection} - {transportConnection.GetAddressString(true)}");
			}
		}

		public CommandLogTransportConnections(Local newLocalization)
		{
			localization = newLocalization;
			_command = "LogTransportConnections";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
