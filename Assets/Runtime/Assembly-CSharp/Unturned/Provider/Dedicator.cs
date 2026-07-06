////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;

namespace SDG.Unturned
{
	public class Dedicator : MonoBehaviour
	{
		public static CommandWindow commandWindow
		{
			get;
			protected set;
		}

		//private static ESteamSecurity _security;
		public static ESteamServerVisibility serverVisibility;
		//{
		//	get { return _security; }
		//}

		public static string serverID;

		/// <summary>
		/// Is the application running as a headless server?
		/// Replacement for isDedicated property. The property could not be changed to const in dedicated-server-only
		/// builds without potentially breaking plugins. Only development builds can be run as both client or server.
		/// </summary>
#if DEDICATED_SERVER
		public const bool IsDedicatedServer = true;
#else
		public static bool IsDedicatedServer => _isDedicated;
#endif // DEDICATED_SERVER

#if !DEDICATED_SERVER
		private static bool _isDedicated;
#endif
		[System.Obsolete("Server plugins do not need to check this because they run on the dedicated-server-only builds.")]
		public static bool isDedicated =>
#if DEDICATED_SERVER
				true;
#else
				_isDedicated;
#endif

		/// <summary>
		/// Are we currently running the standalone dedicated server app?
		/// </summary>
		public static bool isStandaloneDedicatedServer =>
#if DEDICATED_SERVER
				true;
#else
				false;
#endif

		/// <summary>
		/// Should dedicated server disable requests to internet?
		/// While in LAN mode skips the Steam backend connection and workshop item queries.
		/// Needs a non-Steam networking implementation before it will be truly offline only.
		/// </summary>
		public static CommandLineFlag offlineOnly = new CommandLineFlag(false, "-OfflineOnly");

#if WITH_THIRDPARTYAC
		private static bool _hasThirdpartyAntiCheat;
		public static bool hasThirdpartyAntiCheat => _hasThirdpartyAntiCheat;
#endif // WITH_THIRDPARTYAC

		private void Update()
		{
			if (IsDedicatedServer)
			{
				if (commandWindow != null)
				{
					commandWindow.update();
				}
			}
		}

		public void awake()
		{
			bool launchedWithServerArgs = CommandLine.tryGetServer(out serverVisibility, out serverID);
#if !DEDICATED_SERVER
			_isDedicated = launchedWithServerArgs;
#endif

#if UNITY_EDITOR
			if (runServerInEditor)
			{
				serverVisibility = ESteamServerVisibility.LAN;
				serverID = "Singleplayer";
#if !DEDICATED_SERVER
				_isDedicated = true;
#endif // !DEDICATED_SERVER
			}
#endif // UNITY_EDITOR

#if WITH_THIRDPARTYAC
			_hasThirdpartyAntiCheat = CommandLine.Get().IndexOf(ThirdpartyAntiCheat.CommandLineFlag, StringComparison.OrdinalIgnoreCase) != -1;
#endif

#if DEDICATED_SERVER
			bool usingDefaultServer = !launchedWithServerArgs; // Used to log once command window is created.
			if (usingDefaultServer)
			{
				// Server type was not specified on command-line, but we are running the standalone dedicated server.
				// Default to a LAN server type.
				serverVisibility = ESteamServerVisibility.LAN;
				serverID = "Default";
			}
#endif // DEDICATED_SERVER

			UnturnedMasterVolume.mutedByDedicatedServer = IsDedicatedServer; // Notify in either case because it defaults to muted.

			if (IsDedicatedServer)
			{
				commandWindow = new CommandWindow();

				int targetFrameRate = 50;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				string targetFrameRateString;
				if (CommandLine.TryParseValue("-ApplicationTargetFrameRate", out targetFrameRateString))
				{
					// CommandLineValue<int> cannot be used at this point during startup.
					int.TryParse(targetFrameRateString, out targetFrameRate);
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

				Application.targetFrameRate = targetFrameRate;
				UnturnedLog.info($"Dedicated server set target update rate to {targetFrameRate}");

#if DEDICATED_SERVER
				if (usingDefaultServer)
				{
					// We wait to log until here so that commandWindow is valid.
					CommandWindow.Log("Running standalone dedicated server, but launch arguments were not specified on the command-line.");
					CommandWindow.LogFormat("Defaulting to {0} {1}. Valid command-line dedicated server launch arguments are:", serverID, serverVisibility);
					CommandWindow.Log("+InternetServer/{ID}");
					CommandWindow.Log("+LANServer/{ID}");
				}
#endif // DEDICATED_SERVER
			}
		}

		private void OnApplicationQuit()
		{
			if (IsDedicatedServer)
			{
				if (commandWindow != null)
				{
					commandWindow.shutdown();
				}
			}
		}

#if UNITY_EDITOR
		private static CommandLineFlag runServerInEditor = new CommandLineFlag(false, "-DedicatedServerInEditor");
#endif // UNITY_EDITOR
	}
}
