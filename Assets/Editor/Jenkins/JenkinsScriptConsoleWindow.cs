////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unturned.SystemEx;

internal class JenkinsScriptConsoleWindow : EditorWindow
{
	[MenuItem("Window/Jenkins/Script Console")]
	public static void ShowWindow()
	{
		GetWindow<JenkinsScriptConsoleWindow>("Jenkins Script Console");
	}

	private void OnGUI()
	{
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		scriptText = EditorGUILayout.TextArea(scriptText);
		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Execute Script"))
		{
			JenkinsClient client = JenkinsClient.Get();
			client.ExecuteScript(scriptText);
		}

		if (GUILayout.Button("Execute Command"))
		{
			JenkinsClient client = JenkinsClient.Get();
			client.ExecuteCommand(scriptText);
		}

		if (GUILayout.Button("Execute Build"))
		{
			JenkinsClient client = JenkinsClient.Get();

			Dictionary<string, string> buildArgs = new Dictionary<string, string>();
			foreach (string line in scriptText.SplitLines())
			{
				string[] pair = line.Split('=');
				if (pair.Length == 1)
				{
					buildArgs.Add(line, string.Empty);
				}
				else if (pair.Length == 2)
				{
					buildArgs.Add(pair[0], pair[1]);
				}
			}

			client.ExecuteBuild(buildArgs);
		}
	}

	private string scriptText;
	private Vector2 scrollPosition;
}
