////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.IO.Compression;
using UnityEngine.Networking;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace Unturned.SteamCmd
{
	public enum ESteamCmdInstallResult
	{
		Success,
		AlreadyInstalled,
		Failed,
	}

	public static class SteamCmdUtils
	{
		public static void WriteAppBuildScript(AppBuild build, string path)
		{
			// Create default directory in case it does not exist yet.
			Directory.CreateDirectory(PathEx.Join(UnityPaths.TempDirectory, "SteamCmdBuildScripts"));

			using (StreamWriter streamWriter = new StreamWriter(path))
			{
				VdfWriter vdfWriter = new VdfWriter(streamWriter);
				build.Write(vdfWriter);
			}
		}

		public static void WriteAppBuildScript(AppBuild build)
		{
			string path = GetAppBuildScriptPath(build.appId);
			WriteAppBuildScript(build, path);
		}

		public static void WriteDepotBuildScript(DepotBuild build, string path)
		{
			// Create default directory in case it does not exist yet.
			Directory.CreateDirectory(PathEx.Join(UnityPaths.TempDirectory, "SteamCmdBuildScripts"));

			using (StreamWriter streamWriter = new StreamWriter(path))
			{
				VdfWriter vdfWriter = new VdfWriter(streamWriter);
				build.Write(vdfWriter);
			}
		}

		public static void WriteDepotBuildScript(DepotBuild build)
		{
			string path = GetDepotBuildScriptPath(build.depotId);
			WriteDepotBuildScript(build, path);
		}

		/// <summary>
		/// Default app build script name recommended by Valve.
		/// </summary>
		public static string GetAppBuildScriptName(uint appId)
		{
			return $"app_build_{appId}.vdf";
		}

		public static string GetAppBuildScriptPath(uint appId)
		{
			return PathEx.Join(UnityPaths.TempDirectory, "SteamCmdBuildScripts", GetAppBuildScriptName(appId));
		}

		/// <summary>
		/// Default depot build script name recommended by Valve.
		/// </summary>
		public static string GetDepotScriptName(uint depotId)
		{
			return $"depot_build_{depotId}.vdf";
		}

		public static string GetDepotBuildScriptPath(uint appId)
		{
			return PathEx.Join(UnityPaths.TempDirectory, "SteamCmdBuildScripts", GetDepotScriptName(appId));
		}

		public static ESteamCmdInstallResult InstallIfMissing()
		{
			string exePath = GetSteamCmdPath();
			if (File.Exists(exePath))
			{
				return ESteamCmdInstallResult.AlreadyInstalled;
			}
			else
			{
				try
				{
					DownloadSteamCmd();
				}
				catch (System.Exception exception)
				{
					UnityEngine.Debug.LogError("Caught exception installing Steam CMD:");
					UnityEngine.Debug.LogException(exception);
				}

				if (File.Exists(exePath))
				{
					return ESteamCmdInstallResult.Success;
				}
				else
				{
					return ESteamCmdInstallResult.Failed;
				}
			}
		}

		public static void RunSteamCmd()
		{
			string exePath = GetSteamCmdPath();

			if (!File.Exists(exePath))
			{
				DownloadSteamCmd();
			}

			string login = System.Environment.GetEnvironmentVariable("U3_STEAMCMD_LOGIN");
			string password = System.Environment.GetEnvironmentVariable("U3_STEAMCMD_PASSWORD");

			string arguments;
			if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
			{
				arguments = string.Empty;
			}
			else
			{
				arguments = $"+login {login} {password}";
			}

			System.Diagnostics.Process.Start(exePath, arguments);
		}

		private static void DownloadSteamCmd()
		{
			string url = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
			using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
			{
				webRequest.SendWebRequest();

				while (!webRequest.isDone)
					continue;

				string zipFilePath = PathEx.Join(UnityPaths.TempDirectory, "SteamCmd.zip");
				string extractDirPath = PathEx.Join(UnityPaths.LibraryDirectory, "SteamCmd");
				File.WriteAllBytes(zipFilePath, webRequest.downloadHandler.data);
				ZipFile.ExtractToDirectory(zipFilePath, extractDirPath);
			}
		}

		public static string GetSteamCmdPath()
		{
			return Path.Combine(UnityPaths.LibraryDirectory.FullName, "SteamCmd", "steamcmd.exe");
		}

		/// <returns>Absolute path to steamapps folder in the SteamCmd folder.</returns>
		public static string GetSteamAppsPath()
		{
			return Path.Combine(UnityPaths.LibraryDirectory.FullName, "SteamCmd", "steamapps");
		}
	}
}
