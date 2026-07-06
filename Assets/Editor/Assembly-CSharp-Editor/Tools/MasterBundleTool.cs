////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using UnityEditor;
using UnityEngine;
#if GAME
using Unturned.UnityEx;
using Unturned.SystemEx;
#endif

namespace SDG.Unturned.Tools
{
	public class MasterBundleTool : EditorWindow
	{
		[MenuItem("Window/Unturned/Master Bundle Tool")]
		public static void ShowWindow()
		{
			GetWindow(typeof(MasterBundleTool));
		}

		protected virtual bool IsMasterBundle(string AssetBundleName)
		{
			return EditorPrefs.GetBool(AssetBundleName + "_IsMasterBundle", AssetBundleName.EndsWith("masterbundle", System.StringComparison.Ordinal));
		}

		protected virtual void SetMasterBundle(string AssetBundleName, bool IsMasterBundle)
		{
			EditorPrefs.SetBool(AssetBundleName + "_IsMasterBundle", IsMasterBundle);
		}

		protected virtual string GetMasterBundleExportPath(string AssetBundleName)
		{
			return EditorPrefs.GetString(AssetBundleName + "_ExportPath");
		}

		protected bool FoldoutState_AllAssetBundles;
		protected Vector2 AssetScrollPosition;
		protected virtual void OnGUI_AllAssetBundles()
		{
			string[] AssetBundles = AssetDatabase.GetAllAssetBundleNames();
			foreach (string AssetBundleName in AssetBundles)
			{
				bool CurrentlyMasterBundle = IsMasterBundle(AssetBundleName);
				bool CheckState = GUILayout.Toggle(CurrentlyMasterBundle, AssetBundleName);
				if (CheckState != CurrentlyMasterBundle)
				{
					SetMasterBundle(AssetBundleName, CheckState);
				}
			}
		}

		protected virtual void BuildMasterBundle(string AssetBundleName, bool Multiplatform)
		{
			string OutputPath = GetMasterBundleExportPath(AssetBundleName);
			if (string.IsNullOrEmpty(OutputPath))
			{
				Debug.LogWarning("Output path unset for: " + AssetBundleName);
				return;
			}

			EditorAssetBundleHelper.Build(AssetBundleName, OutputPath, Multiplatform);
		}

		protected bool FoldoutState_MasterBundles;
		protected Vector2 MasterScrollPosition;
		protected bool ToggleState_Multiplatform;
		protected virtual void OnGUI_MasterBundles()
		{
			ToggleState_Multiplatform = GUILayout.Toggle(ToggleState_Multiplatform, new GUIContent("Multi-platform", "Build for mac and linux as well?"));

			string[] AssetBundles = AssetDatabase.GetAllAssetBundleNames();
			foreach (string AssetBundleName in AssetBundles)
			{
				if (!IsMasterBundle(AssetBundleName))
					continue;

#if GAME
				bool isCore = string.Equals(AssetBundleName, "core.masterbundle", System.StringComparison.Ordinal);
#endif

				GUILayout.BeginHorizontal();

				GUILayout.Label(AssetBundleName);

				string CurrentPath = GetMasterBundleExportPath(AssetBundleName);
				bool HasPath = !string.IsNullOrEmpty(CurrentPath);

				if (GUILayout.Button(new GUIContent("...", CurrentPath)))
				{
					string NewPath = EditorUtility.OpenFolderPanel("Master Bundle", CurrentPath, "");
					EditorPrefs.SetString(AssetBundleName + "_ExportPath", NewPath);
				}

#if GAME
				if (isCore)
				{
					if (GUILayout.Button("Default Path"))
					{
						string defaultPath = PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "CoreAssetBundle");
						Directory.CreateDirectory(defaultPath); // Only if it does not already exist.
						EditorPrefs.SetString(AssetBundleName + "_ExportPath", defaultPath);
					}
				}
#endif // GAME

				bool WasEnabled = GUI.enabled;
				GUI.enabled = HasPath;
				if (GUILayout.Button("Export"))
				{
					BuildMasterBundle(AssetBundleName, ToggleState_Multiplatform);
				}
				GUI.enabled = WasEnabled;

				GUILayout.EndHorizontal();

				if (HasPath)
				{
					if (!MasterBundleHelper.containsMasterBundle(CurrentPath))
					{
#if GAME
						if (isCore)
						{
							continue;
						}
#endif // GAME

						EditorGUILayout.HelpBox("Path does not contain MasterBundle.dat!", MessageType.Warning);
					}
				}
			}
		}

		protected virtual void OnGUI()
		{
			FoldoutState_AllAssetBundles = EditorGUILayout.Foldout(FoldoutState_AllAssetBundles, "Asset Bundles");
			if (FoldoutState_AllAssetBundles)
			{
				AssetScrollPosition = GUILayout.BeginScrollView(AssetScrollPosition);
				OnGUI_AllAssetBundles();
				GUILayout.EndScrollView();
			}

			FoldoutState_MasterBundles = EditorGUILayout.Foldout(FoldoutState_MasterBundles, "Master Bundles");
			if (FoldoutState_MasterBundles)
			{
				MasterScrollPosition = GUILayout.BeginScrollView(MasterScrollPosition);
				OnGUI_MasterBundles();
				GUILayout.EndScrollView();
			}
		}

		protected virtual void OnEnable()
		{
			titleContent = new GUIContent("Master Bundle Tool");
		}
	}
}
