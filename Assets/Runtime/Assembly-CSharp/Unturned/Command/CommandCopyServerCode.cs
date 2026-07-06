////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandCopyServerCode : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (executorID != CSteamID.Nil)
				return;

			string serverCode = SteamGameServer.GetSteamID().ToString();
			GUIUtility.systemCopyBuffer = serverCode;
			CommandWindow.Log($"Copied server code ({serverCode}) to clipboard");
		}

		public CommandCopyServerCode(Local newLocalization)
		{
			localization = newLocalization;
			_command = "CopyServerCode";
			_info = "CopyServerCode";
			_help = "Copies the Server Code to the system clipboard. Your friends can join the server by Server Code without port forwarding.";
		}
	}
}
