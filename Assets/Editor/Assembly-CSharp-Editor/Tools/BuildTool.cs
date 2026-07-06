////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;
using Unturned.SteamCmd;

namespace SDG.Unturned.Tools
{
	public partial class BuildTool : EditorWindow
	{
		[MenuItem("Window/Unturned/Build Tool")]
		public static void ShowWindow()
		{
			GetWindow(typeof(BuildTool));
		}

		private void OnGUI()
		{
			titleContent = new GUIContent("Build Tool");

			GUILayout.Label("BUILD");

			if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
			{
				if (GUILayout.Button("Restore Windows 64-bit Target"))
				{
					EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
				}
			}

			if (GUILayout.Button("Build Standalone Platforms"))
			{
				BuildMethods.runBuild(standalonePlatforms: true, test: false);
			}

			if (GUILayout.Button("Code Documentation"))
			{
				BuildMethods.BuildCodeDocs();
				BuildMethods.CopyCodeDocsIntoBuiltPlayers();
			}

			if (GUILayout.Button("Hash"))
			{
				BuildMethods.hashAssemblies();
			}

			if (GUILayout.Button("Build Test"))
			{
				BuildMethods.runBuild(standalonePlatforms: false, test: true);
			}

			if (GUILayout.Button("Build Test (Scripts Only)"))
			{
				BuildMethods.runBuild(standalonePlatforms: false, test: true, scriptsOnly: true);
			}

			GUILayout.Space(20);

			GUILayout.Label("STEAM");

			if (GUILayout.Button("Run SteamCmd"))
			{
				SteamCmdUtils.RunSteamCmd();
			}

			if (GUILayout.Button("Update Steam Dedicated Server SDK"))
			{
				BuildMethods.UpdateSteamDedicatedServerRedist();
			}

			SteamOnGUI();
		}

		partial void SteamOnGUI();
	}
}
