////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;

[System.Serializable]
internal class JenkinsSettings : ScriptableObject
{
	/// <summary>
	/// http:// or https://
	/// </summary>
	public string protocol = "https://";
	public string serverUrl;
	public int serverPort = 8443;
	public string jobName;

	/// <summary>
	/// HTTPS public key must match.
	/// </summary>
	public string expectedCertThumbprint = string.Empty;

	/// <summary>
	/// User for requests which require authentication.
	/// </summary>
	public string username;

	/// <summary>
	/// API token associated with username.
	/// </summary>
	public string apiToken;

	/// <summary>
	/// Seconds between automatic refreshes.
	/// </summary>
	public float pollingInterval = 20.0f;

	/// <summary>
	/// Seconds before request automatically fails.
	/// </summary>
	public int timeoutSeconds = 30;

	/// <summary>
	/// In 2019 LTS the UserSettings folder is not an option, but we should move there in the future.
	/// </summary>
	public const string assetPath = "Assets/Editor/Jenkins.asset";

	public static JenkinsSettings GetOrCreate()
	{
		JenkinsSettings settings = AssetDatabase.LoadAssetAtPath<JenkinsSettings>(assetPath);
		if (settings == null)
		{
			settings = CreateInstance<JenkinsSettings>();
			AssetDatabase.CreateAsset(settings, assetPath);
			AssetDatabase.SaveAssets();
		}
		return settings;
	}

	public static SerializedObject GetSerializedSettings()
	{
		return new SerializedObject(GetOrCreate());
	}
}
