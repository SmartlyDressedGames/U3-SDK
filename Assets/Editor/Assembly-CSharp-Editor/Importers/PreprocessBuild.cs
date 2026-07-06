////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Disable "Apple Silicon" / M1 / universal support for the meantime because BattlEye does not support ARM64.
/// https://forum.unity.com/threads/cloud-build-defaulting-to-intel-64-bit-apple-silicon-dlls-failing-in-macos-build.1066970/
/// https://dev.to/clpsplug/how-to-force-unity-to-target-x64-or-arm64-for-macos-builds-2e5a
/// </summary>
public class PreprocessBuild : IPreprocessBuildWithReport
{
	public int callbackOrder => 0;

	public void OnPreprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.StandaloneOSX)
		{
			Debug.Log("Overriding Mac architecture with x86 rather than universal");
			EditorUserBuildSettings.SetPlatformSettings("Standalone", "OSXUniversal", "Architecture", "x64");
			UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.x64;
		}
	}
}
