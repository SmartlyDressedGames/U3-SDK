////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandCopyFakeIP : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (executorID != CSteamID.Nil)
				return;

			if (!Provider.configData.Server.Use_FakeIP)
			{
				CommandWindow.Log("Cannot copy Fake IP to clipboard because it's turned off in the server config.");
				return;
			}

			SteamGameServerNetworkingSockets.GetFakeIP(0, out SteamNetworkingFakeIPResult_t fakeIpInfo);
			if (fakeIpInfo.m_eResult == EResult.k_EResultBusy)
			{
				CommandWindow.Log("Cannot copy Fake IP to clipboard because it's not ready yet.");
				return;
			}
			else if (fakeIpInfo.m_eResult == EResult.k_EResultOK)
			{
				string ipString = new global::Unturned.SystemEx.IPv4Address(fakeIpInfo.m_unIP).ToString();
				string addrString = $"{ipString}:{fakeIpInfo.m_unPorts[0]}";
				GUIUtility.systemCopyBuffer = addrString;
				CommandWindow.Log($"Copied Fake IP ({addrString}) to clipboard");
			}
			else
			{
				CommandWindow.LogError($"Copy Fake IP to clipboard fatal result: {fakeIpInfo.m_eResult}");
			}
		}

		public CommandCopyFakeIP(Local newLocalization)
		{
			localization = newLocalization;
			_command = "CopyFakeIP";
			_info = "CopyFakeIP";
			_help = "Copies the Fake IP to the system clipboard. Your friends can join the server by Fake IP without port forwarding.";
		}
	}
}
