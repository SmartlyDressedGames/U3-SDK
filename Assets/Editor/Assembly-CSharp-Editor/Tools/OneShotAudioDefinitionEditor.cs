////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OneShotAudioDefinition))]
public class OneShotAudioDefinitionEditor : UnityEditor.Editor
{
	[MenuItem("Assets/Create/Unturned/One Shot Audio Def from Selection")]
	public static void CreateFromSelection()
	{
		AudioClip[] selection = Selection.GetFiltered<AudioClip>(SelectionMode.Assets);

		string path = "Assets";
		foreach (AudioClip clip in selection)
		{
			path = AssetDatabase.GetAssetPath(clip);
			path = System.IO.Path.GetDirectoryName(path);
			break;
		}

		OneShotAudioDefinition audioDef = ScriptableObject.CreateInstance<OneShotAudioDefinition>();
		audioDef.clips = new List<AudioClip>(selection);

		AssetDatabase.CreateAsset(audioDef, path + "/OneShotAudioDef.asset");
		AssetDatabase.SaveAssets();

		Selection.objects = new Object[] { audioDef };
	}

	[MenuItem("Assets/Create/Unturned/One Shot Audio Def from Selection", validate = true)]
	public static bool CreateFromSelection_Validate()
	{
		AudioClip[] clips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets);
		return clips.Length > 0;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		OneShotAudioDefinition targetAudioDef = target as OneShotAudioDefinition;
		if (targetAudioDef == null)
			return;

		GUILayout.Space(20);

		GUI.enabled = targetAudioDef.clips != null && targetAudioDef.clips.Count > 0;
		if (GUILayout.Button("Play Random"))
		{
			previewAudioSource.pitch = Random.Range(targetAudioDef.minPitch, targetAudioDef.maxPitch);
			previewAudioSource.volume = targetAudioDef.volumeMultiplier;
			previewAudioSource.clip = targetAudioDef.GetRandomClip();
			previewAudioSource.Play();
		}

		GUILayout.Space(20);

		GUI.enabled = previewAudioSource.isPlaying;
		GUILayout.Label("Playing: " + (previewAudioSource.clip != null ? previewAudioSource.clip.name : "null"));
		if (GUILayout.Button("Stop"))
		{
			previewAudioSource.Stop();
		}
	}

	private void OnEnable()
	{
		GameObject previewGameObject = new GameObject("OneShotAudioDefinitionPreview");
		previewGameObject.hideFlags = HideFlags.HideAndDontSave;

		previewAudioSource = previewGameObject.AddComponent<AudioSource>();
		previewAudioSource.playOnAwake = false;
		previewAudioSource.spatialBlend = 0.0f; // 2D
	}

	private void OnDestroy()
	{
		if (previewAudioSource != null)
		{
			DestroyImmediate(previewAudioSource.gameObject);
			previewAudioSource = null;
		}
	}

	private AudioSource previewAudioSource;
}
