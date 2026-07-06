////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal class JenkinsConsoleTextWindow : EditorWindow
{
	[MenuItem("Window/Jenkins/Open Job in Browser")]
	public static void OpenJobInBrowser()
	{
		JenkinsSettings settings = JenkinsSettings.GetOrCreate();
		string fileName = $"{settings.protocol}{settings.serverUrl}:{settings.serverPort}/job/{settings.jobName}";
		System.Diagnostics.Process.Start(fileName);
	}

	[MenuItem("Window/Jenkins/Console Text")]
	public static void ShowWindow()
	{
		GetWindow<JenkinsConsoleTextWindow>("Jenkins Console Text");
	}

	private void OnGUI()
	{
		JenkinsClient client = JenkinsClient.Get();

		IReadOnlyList<JenkinsBuild> builds = client.GetBuilds();
		int buildCount = builds.Count;

		if (buildCount < 1)
		{
			EditorGUILayout.HelpBox("No Builds", MessageType.Info);
			return;
		}

		int latestBuildIndex = buildCount - 1;
		int latestBuildNumber = builds[latestBuildIndex].number;

		if (cachedBuildCount != buildCount || cachedLatestBuildNumber != latestBuildNumber)
		{
			cachedBuildCount = buildCount;
			cachedLatestBuildNumber = latestBuildNumber;

			displayedOptions = new string[buildCount];
			for (int index = 0; index < buildCount; ++index)
			{
				displayedOptions[index] = $"Build: {builds[index].number} Result: {builds[index].result}";
			}

			selectedBuildIndex = latestBuildIndex;
		}

		selectedBuildIndex = EditorGUILayout.Popup(selectedBuildIndex, displayedOptions);

		if (selectedBuildIndex >= 0 && selectedBuildIndex < buildCount)
		{
			string consoleText = builds[selectedBuildIndex].consoleText;
			if (string.IsNullOrEmpty(consoleText))
			{
				EditorGUILayout.HelpBox("No Console Text", MessageType.Error);
			}
			else if (consoleText.Length > 10000) // 10 KB
			{
				if (GUILayout.Button("Open in Text Editor"))
				{
					client.OpenConsoleText(builds[selectedBuildIndex]);
				}
			}
			else
			{
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
				EditorGUILayout.TextArea(consoleText);
				EditorGUILayout.EndScrollView();
			}
		}
		else
		{
			EditorGUILayout.HelpBox("Invalid Selection", MessageType.Error);
		}
	}

	private Vector2 scrollPosition;
	private int selectedBuildIndex = -1;
	private int cachedBuildCount = -1;
	private int cachedLatestBuildNumber = -1;
	private string[] displayedOptions;
}
