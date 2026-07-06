////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using UnityEditor;
using UnityEngine;
using Unturned.SystemEx;
using Unturned.UnityEx;

/// <summary>
/// Auto-executes on editor startup to import latest version of TMPro essential resources
/// if not already present.
/// </summary>
[InitializeOnLoad]
public static class TMProSetup
{
	static TMProSetup()
	{
		string expectedPath = PathEx.Join(UnityPaths.AssetsDirectory, "TextMesh Pro");
		if (Directory.Exists(expectedPath))
		{
			// Already imported.
			return;

		}

		// Interestingly, it sounds like Unity patches implementation of Path.GetFullPath
		// explicitly to support package lookup:
		// https://discussions.unity.com/t/how-unity-packages-redirect-full-path/950312
		string importPath = Path.GetFullPath(Path.Join("Packages", "com.unity.textmeshpro"));
		importPath = Path.Join(importPath, "Package Resources", "TMP Essential Resources.unitypackage");
		if (!File.Exists(importPath))
		{
			Debug.LogError($"Expected to find TextMesh Pro essential resources package at: {importPath}");
			return;
		}

		AssetDatabase.ImportPackage(importPath, /*interactive*/ false);
		Debug.Log("Imported TextMesh Pro essential resources!");
	}
}
