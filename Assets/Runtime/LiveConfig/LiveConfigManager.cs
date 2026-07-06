////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Singleton responsible for downloading live config.
	/// </summary>
	public class LiveConfigManager : MonoBehaviour
	{
		public static LiveConfigManager Get()
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("LiveConfig");
				DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				instance = gameObject.AddComponent<LiveConfigManager>();
			}

			return instance;
		}

		public void Refresh()
		{
			if (!isRefreshing)
			{
				isRefreshing = true;
				StartCoroutine(RequestConfig());
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public bool HasEditorLiveConfigFile()
		{
			return File.Exists(GetEditorLiveConfigFilePath());
		}

		/// <param name="delaySeconds">If set, artifically delay this many seconds to test menu response.</param>
		public void LoadFromFile(float delaySeconds)
		{
			if (delaySeconds > Mathf.Epsilon)
			{
				StartCoroutine(RequestConfigFromFile(delaySeconds));
			}
			else
			{
				LoadConfigFromFileInternal();
			}
		}

		private IEnumerator RequestConfigFromFile(float delaySeconds)
		{
			Debug.Log($"Delaying editor live config load by {delaySeconds} seconds...");
			yield return new WaitForSecondsRealtime(delaySeconds);
			Debug.Log($"Loading editor live config now that artifical delay ({delaySeconds} s) has passed");
			LoadConfigFromFileInternal();
		}

		private void LoadConfigFromFileInternal()
		{
			try
			{
				string filePath = GetEditorLiveConfigFilePath();
				DatParser parser = new DatParser();
				IDatDictionary data;
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (StreamReader streamReader = new StreamReader(fileStream))
				{
					data = parser.Parse(streamReader);
				}

				if (parser.HasError)
				{
					foreach (string message in parser.ErrorMessages)
					{
						Debug.LogError($"Error parsing editor live config: \"{message}\"");
					}
				}

				config = new LiveConfigData();
				config.Parse(data);
				wasPopulated = true;
				OnConfigRefreshed?.Invoke();
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Caught exception loading live config from file:");
				Debug.LogException(ex);
			}
		}

		private string GetEditorLiveConfigFilePath()
		{
			return PathEx.Join(UnityPaths.AssetsDirectory, "Editor", "LiveConfig", "LiveConfig.dat");
		}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public event System.Action OnConfigRefreshed;

		private IEnumerator RequestConfig()
		{
			string url = "https://smartlydressedgames.com/UnturnedLiveConfig.dat";
			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.timeout = 60;

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"Error getting live config: {request.error}");
				}
				else
				{
					try
					{
						DatParser parser = new DatParser();
						IDatDictionary data = parser.Parse(request.downloadHandler.data);

						if (parser.HasError)
						{
							foreach (string errorMessage in parser.ErrorMessages)
							{
								Debug.LogError($"Error parsing live config: \"{errorMessage}\"");
							}
						}

						config = new LiveConfigData();
						config.Parse(data);
						wasPopulated = true;
					}
					catch (System.Exception ex)
					{
						Debug.LogError($"Caught exception requesting live config:");
						Debug.LogException(ex);
					}
				}

				isRefreshing = false;
				OnConfigRefreshed?.Invoke();
			}
		}

		public LiveConfigData config = new LiveConfigData();
		public bool wasPopulated = false;
		private bool isRefreshing;

		private static LiveConfigManager instance;
	}
}
