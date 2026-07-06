////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;

namespace SDG.Unturned.Tools
{
	public class EditorSettingsTool : EditorWindow
	{
		[MenuItem("Window/Unturned/Editor Settings")]
		public static void ShowWindow()
		{
			GetWindow(typeof(EditorSettingsTool));
		}

		private void prefCheckbox(string name)
		{
			bool savedValue = EditorPrefs.GetInt(name, 0) == 1;
			bool newValue = EditorGUILayout.Toggle(PrettyCommandLineFlagName(name), savedValue);
			if (newValue != savedValue)
			{
				EditorPrefs.SetInt(name, newValue ? 1 : 0);
			}
		}

		private void OnEnable()
		{
			titleContent = new GUIContent("Editor Settings");
		}

		private void OnGUI()
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			float oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 300.0f;

			// Assets
			{
				foldoutAssets = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAssets, "Assets");
				if (foldoutAssets)
				{
					prefCheckbox("-SkipAssets");
					prefCheckbox("-ValidateAssets");
					prefCheckbox("-ParseAssetMetadata");
					prefCheckbox("-ResaveAssets");
					prefCheckbox("-ExportAssetsReport");
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}

			// Level Batching
			{
				foldoutLevelBatching = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutLevelBatching, "Level Batching");
				if (foldoutLevelBatching)
				{
					prefCheckbox("-PreviewLevelBatchingTextureAtlas");
					prefCheckbox("-PreviewLevelBatchingMeshExclusions");
					prefCheckbox("-PreviewLevelBatchingUniqueMaterials");
					prefCheckbox("-LogLevelBatchingTextureAtlasExclusions");
					prefCheckbox("-ValidateLevelBatchingUVs");
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}

			// Live Config
			{
				foldoutLiveConfig = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutLiveConfig, "Live Config");
				if (foldoutLiveConfig)
				{
					prefCheckbox("-EditorLiveConfig");
					prefCheckbox("-DelayEditorLiveConfig");
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}

			// Playing in Unity
			{
				foldoutPlayingInUnity = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutPlayingInUnity, "Playing in Unity");
				if (foldoutPlayingInUnity)
				{
					string savedLevel = EditorPrefs.GetString("AutoLoadLevel");
					string newLevel = EditorGUILayout.TextField("Auto Load Level", savedLevel);
					if (newLevel != savedLevel)
					{
						EditorPrefs.SetString("AutoLoadLevel", newLevel);
					}

					EAutoLoadMode savedMode = (EAutoLoadMode) EditorPrefs.GetInt("AutoLoadMode");
					EAutoLoadMode newMode = (EAutoLoadMode) EditorGUILayout.EnumPopup("Auto Load Mode", savedMode);
					if (newMode != savedMode)
					{
						EditorPrefs.SetInt("AutoLoadMode", (int) newMode);
					}

					string languageOverride = EditorPrefs.GetString("LanguageOverride");
					string newLanguage = EditorGUILayout.TextField("Language Override", languageOverride);
					if (newLanguage != languageOverride)
					{
						EditorPrefs.SetString("LanguageOverride", newLanguage);
					}

					prefCheckbox("-DedicatedServerInEditor");
					prefCheckbox("-LoadCoreAssetBundleFromSteamInstall");
					prefCheckbox("-NoGoldUpgrade");
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}

			// Misc
			// Options which are sometimes useful, but not most of the time.
			{
				foldoutMisc = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMisc, "Misc");
				if (foldoutMisc)
				{
					prefCheckbox("-BypassWorkshopDownloadRestrictions");
					prefCheckbox("-IgnoreServerWorkshopFiles");
					prefCheckbox("-Cinematic");
					prefCheckbox("-LogWorkshopAssets");
					prefCheckbox("-SaveFoliageUsingV2");
					prefCheckbox("-EditorUseWin64Hash");
					prefCheckbox("-NoAsyncMB");
					prefCheckbox("-NoWebRequests");
					prefCheckbox("-NoAdditiveMenu");
					prefCheckbox("-LogLevelHash");
					prefCheckbox("-FallbackGizmos");
					prefCheckbox("-NoPreserveMissingObjects");
					prefCheckbox("-DisableCullingVolumes");

					EGlazier savedGlazier = (EGlazier) EditorPrefs.GetInt("Glazier");
					EGlazier newGlazier = (EGlazier) EditorGUILayout.EnumPopup("Glazier", savedGlazier);
					if (newGlazier != savedGlazier)
					{
						EditorPrefs.SetInt("Glazier", (int) newGlazier);
					}
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}

			EditorGUIUtility.labelWidth = oldLabelWidth;

			EditorGUILayout.EndScrollView();
		}

		private string PrettyCommandLineFlagName(string name)
		{
			sb.Clear();

			bool wasPreviousCharLower = false;
			foreach (char c in name)
			{
				if (c == '-')
				{
					continue;
				}

				if (char.IsUpper(c))
				{
					if (wasPreviousCharLower)
					{
						sb.Append(' ');
					}
					wasPreviousCharLower = false;
				}
				else
				{
					wasPreviousCharLower = true;
				}

				sb.Append(c);
			}

			return sb.ToString();
		}

		private System.Text.StringBuilder sb = new System.Text.StringBuilder();
		private Vector2 scrollPosition = Vector2.zero;
		private bool foldoutAssets;
		private bool foldoutLevelBatching;
		private bool foldoutLiveConfig;
		private bool foldoutPlayingInUnity = true;
		private bool foldoutMisc;

		private enum EGlazier
		{
			Default,
			IMGUI,
			uGUI,
			UIToolkit,
		}

		private enum EAutoLoadMode
		{
			Singleplayer,
			Editor,
		}
	}
}
