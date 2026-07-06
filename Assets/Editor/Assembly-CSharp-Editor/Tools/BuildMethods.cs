////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Unturned.Jenkins;
using Unturned.SteamCmd;
using Unturned.SystemEx;
using Unturned.UnityEx;

public enum EUnturnedBuildTarget
{
	Windows32,
	Windows64,
	Windows64DedicatedServer,
	Mac64,
	Linux64,
	Linux64DedicatedServer,
	Test,
}

/// <summary>
/// Methods to run from the Unity commandline with build scripts.
/// <example>-executeMethod BuildMethods.XYZ</example>
/// </summary>
public partial class BuildMethods
{
	/// <returns>True if [name] is present on the command line. </returns>
	protected static bool hasBuildCommandLineSwitch(string name)
	{
		int keyIndex = Environment.CommandLine.IndexOf(name);
		if (keyIndex == -1)
			return false;

		int startIndex = keyIndex + name.Length;
		int endIndex = Environment.CommandLine.IndexOf(' ', startIndex);

		if (endIndex == -1) // Name was the last argument
		{
			return true;
		}
		else
		{
			// If name is -steamUploadBetaBranch we don't want to match -steamUpload
			return endIndex == startIndex;
		}
	}

	/// <summary>
	/// Parses -key value from the command line.
	/// </summary>
	/// <returns>Value component of -key [value].</returns>
	protected static string getBuildCommandLineVar(string key)
	{
		int keyIndex = Environment.CommandLine.IndexOf(key);
		if (keyIndex == -1)
			return null;

		int startIndex = keyIndex + key.Length + 1;
		int endIndex = Environment.CommandLine.IndexOf(' ', startIndex);

		if (endIndex == -1) // Value continues to end of command line
		{
			return Environment.CommandLine.Substring(startIndex);
		}
		else
		{
			int length = endIndex - startIndex;
			return Environment.CommandLine.Substring(startIndex, length);
		}
	}

	public static void setDefineEnabled(string define, bool enabled)
	{
		string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
		if (defines.Contains(define))
		{
			// define is currently enabled
			if (enabled)
				return; // already enabled, no change needed

			defines = defines.Replace(';' + define, "");
			defines = defines.Replace(define + ';', "");
		}
		else
		{
			// define is currently disabled
			if (!enabled)
				return; // already disabled, no change needed

			if (defines.Length > 0)
				defines += ';';
			defines += define;
		}
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
	}

	/// <returns>True if successfully built.</returns>
	protected static bool buildPlayer(string locationPathName, BuildTargetGroup group, BuildTarget target, EUnturnedBuildTarget unturnedTarget, BuildOptions options)
	{
		List<string> extraScriptingDefines = new List<string>();

		// Should always be enabled.
		extraScriptingDefines.Add("GAME");

		bool isDedicatedServerTarget = unturnedTarget == EUnturnedBuildTarget.Linux64DedicatedServer || unturnedTarget == EUnturnedBuildTarget.Windows64DedicatedServer;
		if (isDedicatedServerTarget)
		{
			extraScriptingDefines.Add("DEDICATED_SERVER");
			extraScriptingDefines.Add("WITH_GAME_THREAD_ASSERTIONS");
		}

		if (unturnedTarget == EUnturnedBuildTarget.Windows32 || unturnedTarget == EUnturnedBuildTarget.Windows64 || unturnedTarget == EUnturnedBuildTarget.Test)
		{
			// 2022 version of beta is Windows-only, so we exclude it from Mac/Linux to avoid disappointment.
			extraScriptingDefines.Add("WITH_GRANTPACKAGE_PROMO");
		}

		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

		List<string> scenePaths = new List<string>(scenes.Length);
		foreach (EditorBuildSettingsScene scene in scenes)
		{
			if (scene.enabled)
			{
				scenePaths.Add(scene.path);
			}
		}

		int subtarget = isDedicatedServerTarget ? (int) StandaloneBuildSubtarget.Server : (int) StandaloneBuildSubtarget.Player;

		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.extraScriptingDefines = extraScriptingDefines.ToArray();
		buildPlayerOptions.scenes = scenePaths.ToArray();
		buildPlayerOptions.locationPathName = locationPathName;
		buildPlayerOptions.targetGroup = group;
		buildPlayerOptions.target = target;
		buildPlayerOptions.subtarget = subtarget;
		buildPlayerOptions.options = options;

		EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");

		// Nelson 2023-10-31: one would think that audio should be disabled by default in headless builds,
		// but one server host sent in a crash dump with a call stack in audio initialization, so to be
		// safe we explicitly check the "Disable Audio" setting for server builds.
		UnityEngine.Object audioManager = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/AudioManager.asset");
		SerializedObject serializedAudioManager = new SerializedObject(audioManager);
		SerializedProperty disableAudioProperty = serializedAudioManager.FindProperty("m_DisableAudio");
		disableAudioProperty.boolValue = isDedicatedServerTarget;
		serializedAudioManager.ApplyModifiedPropertiesWithoutUndo();

		UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
		if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
		{
			return true;
		}
		else
		{
			string message = string.Format("Build {0} result: {1} Warnings: {2} Errors: {3}", target, report.summary.result, report.summary.totalWarnings, report.summary.totalErrors);
			CLCIReport.logError(message);
			return false;
		}
	}

	protected static BuildOptions getBuildOptions()
	{
		BuildOptions options = BuildOptions.None;
		if (scriptsOnly)
		{
			options |= BuildOptions.BuildScriptsOnly;
		}
		return options;
	}

	protected static bool buildWindows32Player()
	{
		return buildPlayer("Builds/Windows32/Unturned.exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, EUnturnedBuildTarget.Windows32, getBuildOptions());
	}

	protected static bool buildWindows64Player()
	{
		return buildPlayer("Builds/Windows64/Unturned.exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, EUnturnedBuildTarget.Windows64, getBuildOptions());
	}

	protected static bool buildWindows64DedicatedServer()
	{
		// No "_Headless.exe" suffix because it replaces the non-headless release.
		return buildPlayer("Builds/Windows64_Headless/Unturned.exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, EUnturnedBuildTarget.Windows64DedicatedServer, getBuildOptions());
	}

	protected static bool buildMac64Player()
	{
		return buildPlayer("Builds/OSX64/Unturned.app", BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, EUnturnedBuildTarget.Mac64, getBuildOptions());
	}

	protected static bool buildLinux64Player()
	{
		return buildPlayer("Builds/Linux64/Unturned.x86_64", BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, EUnturnedBuildTarget.Linux64, getBuildOptions());
	}

	protected static bool buildLinux64DedicatedServer()
	{
		// "_Headless" suffix because it was kept in the same directory the Linux64 release
		// for several years, so hosts have scripts referring to it as such.
		return buildPlayer("Builds/Linux64_Headless/Unturned_Headless.x86_64", BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, EUnturnedBuildTarget.Linux64DedicatedServer, getBuildOptions());
	}

	private static void ReplaceTestDir(string srcPath, string destPath, string suffix)
	{
		srcPath += suffix;
		destPath += suffix;

		FileUtil.DeleteFileOrDirectory(destPath);
		FileUtil.CopyFileOrDirectory(srcPath, destPath);
	}

	private static void ReplaceTestFile(string srcPath, string destPath, string fileName)
	{
		string srcFilePath = Path.Join(srcPath, fileName);
		string destFilePath = Path.Join(destPath, fileName);
		File.Copy(srcFilePath, destFilePath, true);
	}

	protected static bool buildTestPlayer()
	{
		bool success = buildPlayer("Builds/Test/Unturned.exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, EUnturnedBuildTarget.Test, getBuildOptions() | BuildOptions.AllowDebugging | BuildOptions.Development);
		if (!success)
			return false;

		string projectPath = UnityPaths.ProjectDirectory.FullName;
		string testPath = UnityPaths.ProjectDirectory.FullName + "/Builds/Test";
		string win64Path = UnityPaths.ProjectDirectory.FullName + "/Builds/Windows64";

#if WITH_THIRDPARTYAC
		ReplaceTestDir(win64Path, testPath, "/" + ThirdpartyAntiCheat.ExtraFilesDirName);
#endif // WITH_THIRDPARTYAC

		string steamRedistPath = Path.Combine(SteamCmdUtils.GetSteamAppsPath(), "content", "app_1007", "depot_1004");
		if (!Directory.Exists(steamRedistPath))
		{
			if (!runBuildStep("Updating Steam dedicated server SDK (was missing)", UpdateSteamDedicatedServerRedist))
				return false;
		}

		ReplaceTestFile(steamRedistPath, testPath, "steamclient64.dll");
		ReplaceTestFile(steamRedistPath, testPath, "tier0_s64.dll");
		ReplaceTestFile(steamRedistPath, testPath, "vstdlib_s64.dll");

		return true;
	}

	private static void assertEnvVarSet(string var)
	{
		if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(var)))
			throw new System.Exception(string.Format("Environment variable '{0}' is unset!", var));
	}

	/// <summary>
	/// Process created by <see cref="runProcessFromUnity(string, string, bool)"/>
	/// </summary>
	private static System.Diagnostics.Process process;

	private static bool wasProcessForceKilled;

	/// <summary>s
	/// Kill external process and suppress associated exceptions.
	/// </summary>
	private static void forceKillProcess()
	{
		process.Close();
		wasProcessForceKilled = true;
	}

	/// <summary>
	/// Run a .bat, block until exit and redirect script output to Unity logs.
	/// </summary>
	/// <returns>True if script ran successfully.</returns>
	public static bool runProcessFromUnity(string processPath, string args = "", bool captureOutput = true)
	{
		bool success;

		try
		{
			wasProcessForceKilled = false;

			process = new System.Diagnostics.Process();
			process.EnableRaisingEvents = false;
			process.StartInfo.FileName = processPath;
			process.StartInfo.Arguments = args;
			process.StartInfo.UseShellExecute = false; // Must be false to redirect IO
			process.StartInfo.CreateNoWindow = true;

			if (captureOutput)
			{
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardError = true;
				process.OutputDataReceived += onBatchScriptOutputDataReceived;
				process.ErrorDataReceived += onBatchScriptErrorDataReceived;
			}

			process.Start();

			if (captureOutput)
			{
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}

			process.WaitForExit();

			if (process.ExitCode == 0)
			{
				CLCIReport.log("Process successful: " + processPath + " " + args);
				success = true;
			}
			else
			{
				CLCIReport.logError("Process failed (exit code " + process.ExitCode + "): " + processPath + " " + args);
				success = false;
			}
		}
		catch (Exception exception)
		{
			CLCIReport.logError("Failed to run process: " + processPath);

			if (wasProcessForceKilled)
			{
				CLCIReport.logError("Process was force-killed");
			}
			else
			{
				Debug.LogException(exception);
			}

			success = false;
		}

		process = null;

		return success;
	}

	protected static string cleanBatchScriptEventArgs(System.Diagnostics.DataReceivedEventArgs eventArgs)
	{
		string text = eventArgs.Data;
		text = text.Replace("\n", string.Empty);
		text = text.Replace("\r", string.Empty); // \r\n
		text = text.Replace("\t", string.Empty);
		return text;
	}

	protected static void onBatchScriptOutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs eventArgs)
	{
		string text = cleanBatchScriptEventArgs(eventArgs);
		if (string.IsNullOrEmpty(text))
			return;

		CLCIReport.log(text);
	}

	protected static void onBatchScriptErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs eventArgs)
	{
		string text = cleanBatchScriptEventArgs(eventArgs);
		if (!string.IsNullOrEmpty(text))
		{
			CLCIReport.logWarning(text);
		}

		// Some process will spam error data without closing, so we force quit it.
		forceKillProcess();
	}

	protected static bool includeEmptyHash(List<byte> hashBuilder)
	{
		byte[] emptyHash = new byte[21];
		emptyHash[0] = 20;
		hashBuilder.AddRange(emptyHash);
		return true;
	}

	/// <summary>
	/// Generate a 20 byte SHA1 hash of the .dll, and append to hash list
	/// </summary>
	/// <returns>True if hashing was successful.</returns>
	protected static bool includeAssemblyHash(List<byte> hashBuilder, string pathToAssembly)
	{
		if (!File.Exists(pathToAssembly))
		{
			includeEmptyHash(hashBuilder);
			CLCIReport.logError("Assembly missing for hashing: " + pathToAssembly);
			return false;
		}

		FileStream stream = new FileStream(pathToAssembly, FileMode.Open, FileAccess.Read, FileShare.Read);
		byte[] contents = new byte[stream.Length];
		stream.Read(contents, 0, contents.Length);
		stream.Close();
		stream.Dispose();

		SHA1CryptoServiceProvider cryptoService = new SHA1CryptoServiceProvider();
		byte[] assemblyHash = cryptoService.ComputeHash(contents);

		hashBuilder.Add(20); // Length of hash
		hashBuilder.AddRange(assemblyHash);
		return true;
	}

	protected static bool IncludeResourcesHash(List<byte> hashBuilder, string dataPath)
	{
		if (!Directory.Exists(dataPath))
		{
			CLCIReport.logError("Resources folder missing for hashing: " + dataPath);
			return false;
		}

		List<string> filePaths = ResourceHash.GatherFilePaths(dataPath);
		if (filePaths.Count < 10)
		{
			CLCIReport.logError("Resources folder has unusually low number of files: " + dataPath);
			return false;
		}

		List<byte[]> fileHashes = new List<byte[]>();
		foreach (string path in filePaths)
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			using (SHA1Stream hashStream = new SHA1Stream(fileStream))
			using (MemoryStream memoryStream = new MemoryStream())
			{
				hashStream.CopyTo(memoryStream);
				byte[] fileHash = hashStream.Hash;
				fileHashes.Add(fileHash);
			}
		}

		byte[] combinedHash = Hash.combine(fileHashes);

		hashBuilder.Add(20);
		hashBuilder.AddRange(combinedHash);
		return true;
	}

	/// <summary>
	/// While still used this was more useful prior to implementing BattlEye.
	/// Server requires that clients send a SHA1 hash of their assembly prior to connecting,
	/// so this method generates a file with all of the client hashes for the server to check.
	/// </summary>
	/// <returns>True if hashing was successful.</returns>
	public static bool hashAssemblies()
	{
		bool success = true;
		string buildsPath = PathEx.Join(UnityPaths.ProjectDirectory, "Builds");

		List<byte> hashBuilder = new List<byte>();

		success &= includeAssemblyHash(hashBuilder, buildsPath + "/Windows64/Unturned_Data/Managed/Assembly-CSharp.dll");
		success &= includeAssemblyHash(hashBuilder, buildsPath + "/OSX64/Unturned.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll");
		success &= includeAssemblyHash(hashBuilder, buildsPath + "/Linux64/Unturned_Data/Managed/Assembly-CSharp.dll");

		success &= IncludeResourcesHash(hashBuilder, Path.Combine(buildsPath, "Windows64", "Unturned_Data"));
		success &= IncludeResourcesHash(hashBuilder, Path.Combine(buildsPath, "OSX64", "Unturned.app", "Contents", "Resources", "Data"));
		success &= IncludeResourcesHash(hashBuilder, Path.Combine(buildsPath, "Linux64", "Unturned_Data"));

		byte[] hashOutput = hashBuilder.ToArray();
		File.WriteAllBytes(buildsPath + "/Shared/Extras/Sources/Animation/appout.log", hashOutput); // Misleading path to keep cheat devs off the scent for a little while at least.

		if (hashOutput.Length == 126)
		{
			return success;
		}
		else
		{
			CLCIReport.logError("Assembly hash length should be 126 but was " + hashOutput.Length + "!");
			return false;
		}
	}

	/// <summary>
	/// Version of hashAssemblies that exits with error code 0 on success,
	/// or error code 1 on failure.
	/// </summary>
	public static void hashAssembliesFromCommandLine()
	{
		bool success = hashAssemblies();
		EditorApplication.Exit(success ? 0 : 1);
	}

	/// <summary>
	/// Get absolute path to build scripts folder.
	/// e.g. C:/Unturned/Build_Scripts
	/// </summary>
	/// <returns></returns>
	protected static string getProjectBuildScriptsPath()
	{
		return PathEx.Join(UnityPaths.ProjectDirectory, "Build_Scripts");
	}

	protected static string getSharedReleasePath()
	{
		return PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "Shared_Release");
	}

	protected static string getSharedExperimentalPath()
	{
		return PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "Shared_Experimental");
	}

	protected static string getSharedSpecialPath()
	{
#if EXPERIMENTAL
		return getSharedExperimentalPath();
#else
		return getSharedReleasePath();
#endif
	}

	protected static string GetBuildsPath()
	{
		return PathEx.Join(UnityPaths.ProjectDirectory, "Builds");
	}

	protected static void GenerateSteamDepotScript(uint depotID, string contentRoot, string installScript, List<string> fileExclusions = null)
	{
		DepotBuild build = new DepotBuild();
		build.depotId = depotID;
		build.contentRoot = contentRoot;
		build.AddFileMapping(localPath: "*", recursive: true);

		if (!string.IsNullOrEmpty(installScript))
		{
			build.AddInstallScript(installScript);
		}

		build.AddFileExclusion("*.pdb");
		build.AddFileExclusion("*.db");
		build.AddFileExclusion("*.debug");
		build.AddFileExclusion("*BurstDebugInformation*"); // e.g. "Unturned_BurstDebugInformation_DoNotShip"

		if (fileExclusions != null)
		{
			foreach (string fileExclusion in fileExclusions)
			{
				build.AddFileExclusion(fileExclusion);
			}
		}

		SteamCmdUtils.WriteDepotBuildScript(build);
	}

	/// <param name="setlive">Beta branch name to set live on, or empty for none. Default doesn't support setlive.</param>
	protected static void GenerateSteamAppScript(uint appID, string setlive, List<uint> depotIDs)
	{
		AppBuild build = new AppBuild();
		build.appId = appID;

		build.desc = "Version " + SDG.Unturned.EditorUtils.GetStatusDataVersion();

		if (JenkinsEnvironment.BuildNumber > -1)
		{
			build.desc += " Build " + JenkinsEnvironment.BuildNumber;
		}

		if (!string.IsNullOrEmpty(JenkinsEnvironment.BranchName))
		{
			build.desc += $" Git Branch \"{JenkinsEnvironment.BranchName}\"";
		}

		build.buildOutput = PathEx.Join(UnityPaths.TempDirectory, "SteamCmdBuildLogs");
		build.setLive = setlive;
		build.contentRoot = GetBuildsPath();

		string depotLines = string.Empty;
		foreach (uint depotID in depotIDs)
		{
			build.AddDepotScript(depotID);
		}

		SteamCmdUtils.WriteAppBuildScript(build);
	}

	protected static string getSteamUploadScriptPath(uint appID)
	{
		return getProjectBuildScriptsPath() + "/Steam_Upload_" + appID + ".bat";
	}

	protected static void generateSteamUploadScript(uint appID)
	{
		string appBuildPath = SteamCmdUtils.GetAppBuildScriptPath(appID);

		// Theoretically the password does not need to be entered after Steam Guard is setup, but it can be hit-or-miss unfortunately.
		string steamcmdLoginEnvVar = "U3_STEAMCMD_LOGIN"; // Username for upload account
		string steamcmdPasswordEnvVar = "U3_STEAMCMD_PASSWORD"; // Password for upload account
		assertEnvVarSet(steamcmdLoginEnvVar);
		assertEnvVarSet(steamcmdPasswordEnvVar);

		string scriptContents = "CALL \"" + SteamCmdUtils.GetSteamCmdPath() + "\" +login \"%" + steamcmdLoginEnvVar + "%\" \"%" + steamcmdPasswordEnvVar + "%\" +run_app_build \"" + appBuildPath + "\" +quit";
		File.WriteAllText(getSteamUploadScriptPath(appID), scriptContents);
	}

	private static bool ValidateDirectoryDoesNotExist(string path)
	{
		if (Directory.Exists(path))
		{
			CLCIReport.logWarning(path + " should not exist");
			return false;
		}
		else
		{
			return true;
		}
	}

	private static bool ValidatePriorToSteamUpload()
	{
		string buildsPath = PathEx.Join(UnityPaths.ProjectDirectory, "Builds");

		bool success = true;
		success &= ValidateDirectoryDoesNotExist(Path.Combine(buildsPath, "Linux64", "Logs"));
		success &= ValidateDirectoryDoesNotExist(Path.Combine(buildsPath, "Linux64_Headless", "Logs"));
		success &= ValidateDirectoryDoesNotExist(Path.Combine(buildsPath, "OSX64", "Logs"));
		success &= ValidateDirectoryDoesNotExist(Path.Combine(buildsPath, "Windows32", "Logs"));
		success &= ValidateDirectoryDoesNotExist(Path.Combine(buildsPath, "Windows64", "Logs"));
		success &= ValidateDirectoryDoesNotExist(Path.Combine(buildsPath, "Windows64_Headless", "Logs"));

		return true;
	}

	public static bool runSteamUploadScriptFromUnity()
	{
		bool success = ValidatePriorToSteamUpload();
		if (!success)
			return false;

		success = runProcessFromUnity(getSteamUploadScriptPath(304930));
		if (!success)
			return false;

		success = runProcessFromUnity(getSteamUploadScriptPath(1110390));
		return success;
	}

	/// <summary>
	/// Nelson 2024-06-12: Test builds in subdirectories of the project folder need a copy of the Steam redist files
	/// that are otherwise automatically included when downloading the dedicated server from steamcmd. We can manually
	/// copy files over, but from time to time the old versions will become incompatible, e.g., causing GameServer_Init
	/// to fail. To prevent this we automatically pull the redist files using steamcmd and copy them into test builds.
	/// </summary>
	public static bool UpdateSteamDedicatedServerRedist()
	{
		if (!InstallSteamCmdIfMissing())
		{
			CLCIReport.logError("Cannot update Steam dedicated server SDK redist because Steam CMD install failed");
			return false;
		}

		string steamCmdPath = SteamCmdUtils.GetSteamCmdPath();
		// App ID 1007 is "Steamworks SDK Redist" and Depot ID 1004 is Windows files.
		string arguments = "+login anonymous +download_depot 1007 1004 +quit";
		bool success = runProcessFromUnity(steamCmdPath, arguments);
		if (!success)
		{
			// Nelson 2024-07-15: steamcmd returns exit code 1 more than we might otherwise expect. Experimenting with
			// not returning false in case maybe it indicates it's hitting a rate limit. If the files don't exist at all
			// we still fail the build later on.
			CLCIReport.log("steamcmd reported failure updating Steam dedicated server SDK redist, but maybe it's already up to date");
		}

		string expectedPath = Path.Combine(SteamCmdUtils.GetSteamAppsPath(), "content", "app_1007", "depot_1004");
		if (!Directory.Exists(expectedPath))
		{
			CLCIReport.log("Steam dedicated server SDK redist files missing");
			return false;
		}

		return true;
	}

	private static void GenerateSteamWindowsDepotScript(uint depotID, string contentRoot, string installScript, List<string> fileExclusions = null)
	{
		List<string> defaultFileExclusions = new List<string>
		{
			"WinPixEventRuntime.dll",
		};

		if (fileExclusions != null)
		{
			defaultFileExclusions.AddRange(fileExclusions);
		}

		GenerateSteamDepotScript(depotID, contentRoot, installScript, defaultFileExclusions);
	}

	private static void GenerateSharedDepotScript(uint sharedDepotID, List<string> fileExclusions = null)
	{
		List<string> defaultFileExclusions = new List<string>
		{
			@"Cloud\*",
			@"Logs\*",
			@"Worlds\*",
			@"Servers\*",
			@"Preferences.json",
			@"Execute.config",
			@"Spy.jpg", // Default client->server->client screenshot location.
			@"Screenshot.png", // Default 4K screenshot location.

			// In the past we included pre-rendered item icons, but it was thousands of PNGs accounting for hundreds
			// of MB, so now the game still supports rendering them but will not include them in Steam or Git.
			@"Extras\Econ\*",
			@"Extras\Icons\*",
		};

		if (fileExclusions != null)
		{
			defaultFileExclusions.AddRange(fileExclusions);
		}

		GenerateSteamDepotScript(sharedDepotID, Path.Combine(GetBuildsPath(), "Shared"), string.Empty, defaultFileExclusions);
	}

	private static void generateGameSteamScript(string betaBranch, bool uploadSharedOnly)
	{
		bool experimental = false;
		uint appID;

		uint windows32DepotID;
		uint windows64DepotID;
		uint mac64DepotID;
		uint linux64DepotID;

		uint sharedDepotID;
		uint sharedSpecialDepotID;
		string sharedSpecialName;

		if (experimental)
		{
			appID = 407660;

			windows32DepotID = 407661;
			windows64DepotID = 407665;
			mac64DepotID = 407662;
			linux64DepotID = 407664;

			sharedDepotID = 407663;
			sharedSpecialDepotID = 407668;
			sharedSpecialName = "Shared_Experimental";
		}
		else
		{
			appID = 304930;

			windows32DepotID = 304931;
			windows64DepotID = 304935;
			mac64DepotID = 304932;
			linux64DepotID = 304934;

			sharedDepotID = 304933;
			sharedSpecialDepotID = 304938;
			sharedSpecialName = "Shared_Release";
		}

		GenerateSteamWindowsDepotScript(windows32DepotID, Path.Combine(GetBuildsPath(), "Windows32"), "install.vdf");
		GenerateSteamWindowsDepotScript(windows64DepotID, Path.Combine(GetBuildsPath(), "Windows64"), "install.vdf");
		GenerateSteamDepotScript(mac64DepotID, Path.Combine(GetBuildsPath(), "OSX64"), "install.vdf");
		GenerateSteamDepotScript(linux64DepotID, Path.Combine(GetBuildsPath(), "Linux64"), "install.vdf");

		GenerateSharedDepotScript(sharedDepotID);
		GenerateSteamDepotScript(sharedSpecialDepotID, Path.Combine(GetBuildsPath(), sharedSpecialName), string.Empty);

		List<uint> depotToUploadIDs = new List<uint>();

		if (!uploadSharedOnly)
		{
			depotToUploadIDs.AddRange(new List<uint>
			{
				windows32DepotID,
				windows64DepotID,
				mac64DepotID,
				linux64DepotID,
			});
		}

		depotToUploadIDs.AddRange(new List<uint>
		{
			sharedDepotID,
			sharedSpecialDepotID,
		});

		GenerateSteamAppScript(appID, betaBranch, depotToUploadIDs);
		generateSteamUploadScript(appID);
	}

	private static void generateServerSteamScript(string betaBranch, bool uploadSharedOnly)
	{
		uint appID = 1110390;
		uint windowsDepotID = 1110391;
		uint linuxDepotID = 1110392;
		uint sharedDepotID = 1110393;
		uint sharedSpecialDepotID = 1110394;
		string sharedSpecialName = "Shared_Release";

		GenerateSteamWindowsDepotScript(windowsDepotID, Path.Combine(GetBuildsPath(), "Windows64_Headless"), "install.vdf");
		GenerateSteamDepotScript(linuxDepotID, Path.Combine(GetBuildsPath(), "Linux64_Headless"), "install.vdf");

		GenerateSharedDepotScript(sharedDepotID, new List<string>
		{
			// Foliage.blob is included on server because it is included in hash.
			@"Maps\*\Foliage", // This older, naive format has been tidied up now.
			@"Maps\*\Screenshots\*",
			@"RetiredMapIcons\*",
			@"Screenshots\*",

			// We need Extras\Rocket.Unturned
			// Econ and Icons folders are already ignored by default.
			@"Extras\Bundles\*",
			@"Extras\Maps\*",
			@"Extras\Wallpaper.png",

			// We need Sources\Animation\* because it has a hidden hash file we use to test client.
			@"Extras\Sources\UnturnedExampleMod\*",
			@"Extras\Sources\ExampleAssets.unitypackage",
			@"Extras\Sources\Project.unitypackage",
			@"Extras\Sources\README.txt",
		});
		GenerateSteamDepotScript(sharedSpecialDepotID, Path.Combine(GetBuildsPath(), sharedSpecialName), string.Empty);

		List<uint> depotToUploadIDs = new List<uint>();

		if (!uploadSharedOnly)
		{
			depotToUploadIDs.AddRange(new List<uint>
			{
				windowsDepotID,
				linuxDepotID,
			});
		}

		depotToUploadIDs.AddRange(new List<uint>
		{
			sharedDepotID,
			sharedSpecialDepotID,
		});

		GenerateSteamAppScript(appID, betaBranch, depotToUploadIDs);
		generateSteamUploadScript(appID);
	}

	/// <summary>
	/// Generate all app and depot build .vdfs, as well as a .bat to run SteamCMD.
	/// </summary>
	/// <param name="steamBranch">Specify Steam beta branch to set live on.</param>
	/// <param name="sharedOnly">Only upload the Shared depots. This saves time when only shared files have changed.</param>
	/// <returns>True if scripts were generated successfully.</returns>
	public static bool generateSteamScripts(string betaBranch, bool uploadSharedOnly)
	{
		generateGameSteamScript(betaBranch, uploadSharedOnly);
		generateServerSteamScript(betaBranch, uploadSharedOnly);
		return true;
	}

	public static bool generateSteamScriptsFromCommandLine()
	{
		string steamBranch = getBuildCommandLineVar("-steamBetaBranch");
		if (steamBranch == null)
			steamBranch = string.Empty;

		bool uploadSharedOnly = hasBuildCommandLineSwitch("-steamUploadSharedOnly");

		return generateSteamScripts(steamBranch, uploadSharedOnly);
	}

	protected delegate bool BuildStepSignature();
	protected static bool runBuildStep(string name, BuildStepSignature step)
	{
		CLCIReport.log(name + "...");
		bool success = step();

		if (success)
		{
			CLCIReport.log(name + " successful!");
		}
		else
		{
			CLCIReport.logError(name + " failed!");
		}

		return success;
	}

	private static bool CheckCoreAssetBundleSize(string path)
	{
		try
		{
			FileInfo exportInfo = new FileInfo(path);
			if (exportInfo.Length < 100_000_000) // 100 MB
			{
				CLCIReport.logError($"{path} is unusually small ({ByteDisplay.FileSizeToString(exportInfo.Length)})! Last time this happened it meant some shaders were missing.");
				return false;
			}
		}
		catch (System.Exception e)
		{
			CLCIReport.logError($"Caught exception checking {path} size!");
			Debug.LogException(e);
			return false;
		}

		return true;
	}

	public static bool runBuild(bool standalonePlatforms, bool test, bool scriptsOnly = false, bool assetBundles = false, bool codeDoc = false, bool updateSteamRedistFiles = false)
	{
		BuildMethods.scriptsOnly = scriptsOnly;

		string assetBundlesOutputDir = PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "CoreAssetBundle");
		Directory.CreateDirectory(assetBundlesOutputDir); // Only if it does not already exist.

		string assetBundleName = "core.masterbundle";
		string[] assetPaths = null;
		if (assetBundles)
		{
			assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
		}
		// Saves some perf by disabling these unused loading options.
		// If changing remember to update the master bundle tool.
		BuildAssetBundleOptions assetBundleOptions = BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

		if (codeDoc)
		{
			if (!runBuildStep("Building code documentation", BuildCodeDocs))
				return false;
		}

		if (standalonePlatforms)
		{
			// Nelson 2025-11-12: order of dedicated server vs. player matters! Now in 2022 LTS there's a distinction
			// between player and server asset bundles, and it seems exporting the player asset bundle while the
			// editor is targeting the headless server results in stripping shaders from the asset bundle.
			if (!runBuildStep("Building Linux64 server", buildLinux64DedicatedServer))
				return false;

			if (!runBuildStep("Building Linux64 player", buildLinux64Player))
				return false;

			// Export Linux asset bundle while target platform is Linux to avoid unnecessary switching overhead.
			if (assetBundles)
			{
				AssetBundleBuild[] linuxBuilds = new AssetBundleBuild[1];
				linuxBuilds[0].assetBundleName = SDG.Unturned.MasterBundleHelper.getLinuxAssetBundleName(assetBundleName);
				linuxBuilds[0].assetNames = assetPaths;
				BuildPipeline.BuildAssetBundles(assetBundlesOutputDir, linuxBuilds, assetBundleOptions, BuildTarget.StandaloneLinux64);

				if (!CheckCoreAssetBundleSize(Path.Combine(assetBundlesOutputDir, linuxBuilds[0].assetBundleName)))
				{
					return false;
				}
			}

			if (!runBuildStep("Building Mac64 player", buildMac64Player))
				return false;

			// Export Mac asset bundle while target platform is Mac to avoid unnecessary switching overhead.
			if (assetBundles)
			{
				AssetBundleBuild[] macBuilds = new AssetBundleBuild[1];
				macBuilds[0].assetBundleName = SDG.Unturned.MasterBundleHelper.getMacAssetBundleName(assetBundleName);
				macBuilds[0].assetNames = assetPaths;
				BuildPipeline.BuildAssetBundles(assetBundlesOutputDir, macBuilds, assetBundleOptions, BuildTarget.StandaloneOSX);

				if (!CheckCoreAssetBundleSize(Path.Combine(assetBundlesOutputDir, macBuilds[0].assetBundleName)))
				{
					return false;
				}
			}

			if (!runBuildStep("Building Windows32 player", buildWindows32Player))
				return false;

			if (!runBuildStep("Building Windows64 server", buildWindows64DedicatedServer))
				return false;

			// Build Win64 last so that editor build target is maintained.
			// Previously was Linux64 so it always had to be manually reverted.
			if (!runBuildStep("Building Windows64 player", buildWindows64Player))
				return false;

			// Export Windows asset bundle while target platform is Windows to avoid unnecessary switching overhead.
			if (assetBundles)
			{
				AssetBundleBuild[] windowsBuilds = new AssetBundleBuild[1];
				windowsBuilds[0].assetBundleName = assetBundleName;
				windowsBuilds[0].assetNames = assetPaths;
				BuildPipeline.BuildAssetBundles(assetBundlesOutputDir, windowsBuilds, assetBundleOptions, BuildTarget.StandaloneWindows64);

				if (!CheckCoreAssetBundleSize(Path.Combine(assetBundlesOutputDir, windowsBuilds[0].assetBundleName)))
				{
					return false;
				}
			}
		}

		if (test)
		{
			if (updateSteamRedistFiles)
			{
				if (!runBuildStep("Updating Steam dedicated server SDK", UpdateSteamDedicatedServerRedist))
					return false;
			}

			if (!runBuildStep("Building Test player", buildTestPlayer))
				return false;
		}

#if REPLACE_MONO
		replaceMono();
#endif

		if (assetBundles)
		{
			SDG.Unturned.Tools.EditorAssetBundleHelper.CleanupAfterBuildingAssetBundle(assetBundlesOutputDir);
			SDG.Unturned.Tools.EditorAssetBundleHelper.HashAssetBundle(assetBundlesOutputDir + '/' + assetBundleName);
			SDG.Unturned.Tools.EditorAssetBundleHelper.PostBuildCoreMasterBundle(assetBundlesOutputDir);
		}

		if (codeDoc)
		{
			if (!runBuildStep("Copying code documentation to builds", CopyCodeDocsIntoBuiltPlayers))
				return false;
		}

		return true;
	}

	private static string[] CODE_DOCS_ASSEMBLY_NAMES = new string[]
	{
		"Assembly-CSharp",
		"SDG.NetPak.Runtime",
	};

	/// <summary>
	/// Generate xml docs for plugin devs in temp folder.
	/// Built before Unity players to ensure nothing unexpected ends up in those folders.
	/// </summary>
	internal static bool BuildCodeDocs()
	{
		foreach (string assemblyName in CODE_DOCS_ASSEMBLY_NAMES)
		{
			string csprojPath = PathEx.Join(UnityPaths.ProjectDirectory, assemblyName + ".csproj");
			if (!File.Exists(csprojPath))
			{
				CLCIReport.logError($"Code docs project file \"{csprojPath}\" missing");
				return false;
			}

			// Nelson 2024-06-12: Try multiple times because rarely the Jenkins build (randomly?) fails due to one of
			// these files being busy, and then the next build works fine without any changes.
			int sleepTimeout = 1000;
			const int MAX_ATTEMPTS = 5;
			for (int attemptNumber = 1; attemptNumber <= MAX_ATTEMPTS; ++attemptNumber)
			{
				string outputPath = PathEx.Join(UnityPaths.TempDirectory, assemblyName + ".xml");
				string args = $"build \"{csprojPath}\" -p:DocumentationFile=\"{outputPath}\" /clp:ErrorsOnly";
				bool exporedSuccessfully = runProcessFromUnity("dotnet", args, captureOutput: false);
				if (!exporedSuccessfully)
				{
					if (attemptNumber < MAX_ATTEMPTS)
					{
						Debug.Log($"Problem exporting docs (attempt #{attemptNumber}), will try again after {sleepTimeout} ms");
						System.Threading.Thread.Sleep(sleepTimeout);
						sleepTimeout *= 2;
					}
					else
					{
						Debug.Log($"Problem exporting docs (attempt #{attemptNumber}), canceling build");
						return false;
					}
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Copy xml files from <see cref="BuildCodeDocs"/> into Unity players.
	/// </summary>
	internal static bool CopyCodeDocsIntoBuiltPlayers()
	{
		string[] destinationFolders = new string[]
		{
			Path.Combine(GetBuildsPath(), "Windows64", "Unturned_Data", "Managed"),
			Path.Combine(GetBuildsPath(), "Windows64_Headless", "Unturned_Data", "Managed"),
			Path.Combine(GetBuildsPath(), "Linux64", "Unturned_Data", "Managed"),
			Path.Combine(GetBuildsPath(), "Linux64_Headless", "Unturned_Headless_Data", "Managed"),
		};

		foreach (string destinationFolder in destinationFolders)
		{
			if (!Directory.Exists(destinationFolder))
			{
				CLCIReport.logError($"Code docs destination folder \"{destinationFolder}\" missing");
				return false;
			}
		}

		foreach (string assemblyName in CODE_DOCS_ASSEMBLY_NAMES)
		{
			string sourceFile = PathEx.Join(UnityPaths.TempDirectory, assemblyName + ".xml");

			if (!File.Exists(sourceFile))
			{
				CLCIReport.logError($"Code docs source file \"{sourceFile}\" missing");
				return false;
			}

			foreach (string destinationFolder in destinationFolders)
			{
				string destinationXmlFile = Path.Combine(destinationFolder, assemblyName + ".xml");
				string destinationDllFile = Path.Combine(destinationFolder, assemblyName + ".dll");
				if (!File.Exists(destinationDllFile))
				{
					CLCIReport.logError($"Code docs expected dll \"{destinationDllFile}\" missing for \"{destinationXmlFile}\"");
					return false;
				}

				File.Copy(sourceFile, destinationXmlFile, true);
			}
		}

		return true;
	}

	/// <summary>
	/// Run all necessary build actions after a commit.
	/// </summary>
	public static void runBuildFromCommandLine()
	{
		if (!runBuild(/*standalonePlatforms*/ true, /*test*/ true, assetBundles: true, codeDoc: true, updateSteamRedistFiles: true))
		{
			EditorApplication.Exit(1);
			return;
		}

		EditorApplication.Exit(0);
	}

	public static void NetGenFromCommandLine()
	{
		SDG.Unturned.NetGenUtils.Delete();
		SDG.Unturned.NetGenUtils.Generate();
		EditorApplication.Exit(0);
	}

	static partial void UpdateBetaVersionFile(string betaName);

	/// <summary>
	/// Temporary until we get the chance to consolidate the build machine stuff into a main entry point.
	/// </summary>
	public static void MaybeUploadToSteam()
	{
		// First this might be a parameterized build.
		string betaBranch = Environment.GetEnvironmentVariable("STEAM_DEPLOY");
		if (!string.IsNullOrEmpty(betaBranch))
		{
			betaBranch = betaBranch.Trim();
		}
		if (string.IsNullOrEmpty(betaBranch))
		{
			// Secondly upload may be requested in commit message body.
			string commitMessageBody;
			if (SDG.Unturned.GitUtils.TryGetCommitMessageBody(out commitMessageBody))
			{
				string prefix = "STEAM_DEPLOY=";
				int index = commitMessageBody.IndexOf(prefix, StringComparison.InvariantCultureIgnoreCase);
				if (index >= 0)
				{
					betaBranch = commitMessageBody.Substring(index + prefix.Length).Trim();
					Debug.Log($"Upload to Steam beta branch \"{betaBranch}\" requested in commit message body");
				}
			}
		}
		else
		{
			UnturnedLog.info($"Upload to Steam beta branch \"{betaBranch}\" requested by environment");
		}

		if (!string.IsNullOrEmpty(betaBranch))
		{
			if (!InstallSteamCmdIfMissing())
			{
				EditorApplication.Exit(1);
				return;
			}

			generateSteamScripts(betaBranch, false);

			if (!runBuildStep("Uploading to Steam", runSteamUploadScriptFromUnity))
			{
				EditorApplication.Exit(1);
				return;
			}

			UpdateBetaVersionFile(betaBranch);
		}

		EditorApplication.Exit(0);
	}

	private static bool InstallSteamCmdIfMissing()
	{
		ESteamCmdInstallResult result = SteamCmdUtils.InstallIfMissing();
		switch (result)
		{
			case ESteamCmdInstallResult.AlreadyInstalled:
				return true;

			case ESteamCmdInstallResult.Success:
				CLCIReport.log("Installed Steam CMD!");
				return true;

			case ESteamCmdInstallResult.Failed:
				CLCIReport.logError("Failed to install Steam CMD!");
				return false;
		}

		CLCIReport.logError("Unknown Steam CMD install result: " + result);
		return false;
	}

	private static bool scriptsOnly = false;
}
