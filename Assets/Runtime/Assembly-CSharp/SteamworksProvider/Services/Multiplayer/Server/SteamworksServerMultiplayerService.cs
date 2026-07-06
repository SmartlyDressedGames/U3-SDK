////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Multiplayer.Server;
using SDG.SteamworksProvider.Services.Community;
using SDG.Unturned;
using Steamworks;
using System;
using System.IO;

namespace SDG.SteamworksProvider.Services.Multiplayer.Server
{
	public class SteamworksServerMultiplayerService : Service, IServerMultiplayerService
	{
		public IServerInfo serverInfo
		{
			get;
			protected set;
		}

		public bool isHosting
		{
			get;
			protected set;
		}

		public MemoryStream stream
		{
			get;
			protected set;
		}

		public BinaryReader reader
		{
			get;
			protected set;
		}

		public BinaryWriter writer
		{
			get;
			protected set;
		}

		public event ServerMultiplayerServiceReadyHandler ready;

		private SteamworksAppInfo appInfo;

		public void open(uint ip, ushort port, ESecurityMode security)
		{
			if (isHosting)
			{
				return;
			}

			EServerMode serverMode = EServerMode.eServerModeInvalid;

			switch (security)
			{
				case ESecurityMode.LAN:
					serverMode = EServerMode.eServerModeNoAuthentication;
					break;
				case ESecurityMode.SECURE:
					serverMode = EServerMode.eServerModeAuthenticationAndSecure;
					break;
				case ESecurityMode.INSECURE:
					serverMode = EServerMode.eServerModeAuthentication;
					break;
			}

			// Prior to 2021-10-15 queryPort was port plus one. Now this is reversed, the client treats port plus one
			// as the connection port at the transport layer. Despite this we pass queryPort as the gamePort because
			// the legacy Steam server browser join provides the connection port, not the query port - maybe because
			// Valve games used Game Socket Share mode before Steam Networking Sockets released?
			ushort gamePort = port;
			ushort queryPort = port;
			if (!GameServer.Init(ip, gamePort, queryPort, serverMode, "1.0.0.0"))
			{
				throw new Exception("GameServer API initialization failed!");
			}

			SteamGameServer.SetDedicatedServer(appInfo.isDedicated);
			// SteamGameServer.SetGameDescription(appInfo.name); // In 2014 seemed to be needed to determine game ID. Now fine to use for server details?
			SteamGameServer.SetProduct(appInfo.name);
			SteamGameServer.SetModDir(appInfo.name);

			if (SDG.Unturned.Provider.configData.Server.Use_FakeIP)
			{
				bool success = SteamGameServerNetworkingSockets.BeginAsyncRequestFakeIP(1);
				if (success)
				{
					CommandWindow.Log("Requesting \"FakeIP\" from Steam");
				}
				else
				{
					CommandWindow.LogError("Fatal: BeginAsyncRequestFakeIP returned false");
					throw new System.NotSupportedException("BeginAsyncRequestFakeIP returned false");
				}
			}

			if (clShouldLogin)
			{
				string loginToken = Unturned.CommandGSLT.loginToken?.Trim();
				if (string.IsNullOrEmpty(loginToken))
				{
					loginToken = SDG.Unturned.Provider.configData.Browser.Login_Token?.Trim();
				}

				if (string.IsNullOrEmpty(loginToken))
				{
					Unturned.UnturnedLog.info("Not using login token");
					if (security != ESecurityMode.LAN)
					{
						SDG.Unturned.Level.onPostLevelLoaded += OnPostLevelLoaded;
					}
					SteamGameServer.LogOnAnonymous();
				}
				else
				{
					if (loginToken.Length == 32)
					{
						Unturned.UnturnedLog.info("Using login token");
					}
					else
					{
						// Valve may change the GSLT size so this is just a warning.
						Unturned.UnturnedLog.warn("Using login token, but it does not seem to be correctly formatted");
					}
					SteamGameServer.LogOn(loginToken);
				}
			}
			else
			{
				SDG.Unturned.UnturnedLog.info("Skipping Steam game server login");
			}

			if (clShouldEnableAdvertisement)
			{
				SteamGameServer.SetAdvertiseServerActive(true);
			}
			else
			{
				SDG.Unturned.UnturnedLog.info("Not enabling Steam game server advertisement");
			}

			isHosting = true;
		}

		public void close()
		{
			if (!isHosting)
			{
				return;
			}

			SteamGameServer.SetAdvertiseServerActive(false);

			SteamGameServer.LogOff();
			GameServer.Shutdown();

			isHosting = false;
		}

		public bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel)
		{
			entity = SteamworksCommunityEntity.INVALID;
			length = 0;
			return false;
		}

		public void write(ICommunityEntity entity, byte[] data, ulong length)
		{ }

		public void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel)
		{ }

		public SteamworksServerMultiplayerService(SteamworksAppInfo newAppInfo)
		{
			appInfo = newAppInfo;
			steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(onSteamServerConnectFailure);
			steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(onSteamServersConnected);
			steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(onSteamServersDisconnected);
		}

#pragma warning disable
		private static Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
#pragma warning restore
		private void onSteamServerConnectFailure(SteamServerConnectFailure_t callback)
		{
			if (SDG.Unturned.Dedicator.offlineOnly)
			{
				// Steam we are intentionally not connected!
				return;
			}

			if (callback.m_bStillRetrying)
			{
				Unturned.CommandWindow.LogFormat("Failed to connect to Steam servers because {0}, still retrying", callback.m_eResult);
			}
			else
			{
				Unturned.CommandWindow.LogFormat("Failed to connect to Steam servers because {0}, no longer retrying", callback.m_eResult);
			}

			if (callback.m_eResult == EResult.k_EResultInvalidParam || callback.m_eResult == EResult.k_EResultAccountNotFound)
			{
				Unturned.CommandWindow.LogWarning($"{callback.m_eResult} probably means Game Server Login Token (GSLT) is invalid");
			}
		}

#pragma warning disable
		private static Callback<SteamServersConnected_t> steamServersConnected;
#pragma warning restore
		private void onSteamServersConnected(SteamServersConnected_t callback)
		{
			ready?.Invoke();
		}

#pragma warning disable
		private static Callback<SteamServersDisconnected_t> steamServersDisconnected;
#pragma warning restore
		private void onSteamServersDisconnected(SteamServersDisconnected_t callback)
		{
			if (SDG.Unturned.Dedicator.offlineOnly)
			{
				// Steam we are intentionally not connected!
				return;
			}

			Unturned.CommandWindow.LogFormat("Lost connection to Steam servers because {0}", callback.m_eResult);
		}

		private void OnPostLevelLoaded(int id)
		{
			SDG.Unturned.CommandWindow.LogWarning("Steam Game Server Login Token (GSLT) not set");
			SDG.Unturned.CommandWindow.LogWarning("Without a login token the server:");
			SDG.Unturned.CommandWindow.LogWarning("- Is not visible in Internet server list");
			SDG.Unturned.CommandWindow.LogWarning("- Cannot be joined over the Internet");
			SDG.Unturned.CommandWindow.LogWarning("See this link for guide and more information:");
			SDG.Unturned.CommandWindow.LogWarning("https://docs.smartlydressedgames.com/en/stable/servers/game-server-login-tokens.html");
		}

		private static SDG.Unturned.CommandLineFlag clShouldLogin = new SDG.Unturned.CommandLineFlag(true, "-NoSteamGameServerLogin");
		private static SDG.Unturned.CommandLineFlag clShouldEnableAdvertisement = new SDG.Unturned.CommandLineFlag(true, "-NoSteamGameServerAdvertisement");
	}
}
