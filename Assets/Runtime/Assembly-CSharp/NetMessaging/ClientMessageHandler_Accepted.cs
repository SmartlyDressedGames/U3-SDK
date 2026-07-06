////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using SDG.NetPak;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal static partial class ClientMessageHandler_Accepted
	{
		internal static void ReadMessage(NetPakReader reader)
		{
#if !DEDICATED_SERVER
			Provider.isWaitingForAuthenticationResponse = false;
#endif // !DEDICATED_SERVER

			uint connectIP;
			ushort connectPort;
			reader.ReadUInt32(out connectIP);
			reader.ReadUInt16(out connectPort);
			bool isConnectIpFake = SteamNetworkingUtils.IsFakeIPv4(connectIP);

			UnturnedLog.info("Accepted by server");

#if WITH_THIRDPARTYAC
			if (Provider.IsThirdpartyAntiCheatActiveOnCurrentServer)
			{
				if (!InitThirdpartyAntiCheat(connectIP, connectPort, isConnectIpFake))
				{
					return;
				}
			}
#endif

			RichPresenceConnectionTarget = Provider.server.ToString();
			// Nelson 2025-06-19: Ideally, we would use WAN address if available. But, although server may have provided
			// its public IP address, it doesn't know whether incoming traffic will pass (e.g., whether ports are open
			// to unsolicited traffic). We now only use Fake IP or server codes so that invites to private servers
			// should usually work automatically.
			if (isConnectIpFake)
			{
				IPv4Address address = new IPv4Address(connectIP);
				RichPresenceConnectionTarget = $"{address}:{connectPort}";
				UnturnedLog.info($"Rich presence advertisement using Fake IP address ({RichPresenceConnectionTarget})");
			}
			else
			{
				UnturnedLog.info($"Rich presence advertisement using server code ({RichPresenceConnectionTarget})");
			}

			if (OptionsSettings.ShouldHideRichPresence)
			{
				SteamFriends.SetRichPresence("connect", "");
			}
			else
			{
				SteamUser.AdvertiseGame(Provider.server, 0, 0); // IP and port are not required because Steam can find the server details using unique ID.

				// For CommandLine.TryGetSteamConnect
				SteamFriends.SetRichPresence("connect", $"+connect {RichPresenceConnectionTarget}");
			}

			Lobbies.leaveLobby();

			Steamworks.SteamMatchmaking.AddFavoriteGame(Provider.APP_ID, connectIP, (ushort) (connectPort + 1), connectPort, Provider.STEAM_FAVORITE_FLAG_HISTORY, SteamUtils.GetServerRealTime()); // Add server to history

			Provider.updateRichPresence();

			Provider.onClientConnected?.Invoke();

			// Separate from onClientConnected to make usage clearer.
			OnGameplayConfigReceived?.Invoke();
		}

		/// <summary>
		/// Nelson 2025-06-19: using server-provided connection details is useful because
		/// it can find its public IP (e.g., joining by LAN and sharing WAN IP), and/or
		/// its fake IP (again when joining by LAN).
		/// </summary>
		internal static string RichPresenceConnectionTarget
		{
			get;
			private set;
		}

		internal static event System.Action OnGameplayConfigReceived;

#if WITH_THIRDPARTYAC
		private static partial bool InitThirdpartyAntiCheat(uint ip, ushort port, bool isIpFake);
#endif
	}
}
