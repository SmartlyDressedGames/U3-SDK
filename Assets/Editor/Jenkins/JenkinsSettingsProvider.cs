////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;

internal static class JenkinsSettingsProvider
{
	[SettingsProvider]
	public static SettingsProvider CreateJenkinsSettingsProvider()
	{
		SettingsProvider provider = new SettingsProvider("Preferences/Jenkins", SettingsScope.User);
		provider.label = "Jenkins";
		provider.guiHandler = OnGUI;
		return provider;
	}

	private static void OnGUI(string searchContext)
	{
		SerializedObject settings = JenkinsSettings.GetSerializedSettings();
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.protocol)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.serverUrl)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.serverPort)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.jobName)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.expectedCertThumbprint)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.username)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.apiToken)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.pollingInterval)));
		EditorGUILayout.PropertyField(settings.FindProperty(nameof(JenkinsSettings.timeoutSeconds)));
		settings.ApplyModifiedProperties();
	}
}
