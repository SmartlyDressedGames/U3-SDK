////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unturned.SystemEx;
using Unturned.UnityEx;

internal class JenkinsBuild
{
	public enum EResult
	{
		Success,
		Failure,
		Aborted,
	}

	public int number;
	public EResult result;
	public string consoleText;
}

[InitializeOnLoad]
internal class JenkinsClient
{
	public static JenkinsClient Get()
	{
		return instance;
	}

	public IReadOnlyList<JenkinsBuild> GetBuilds() { return builds; }

	public void ExecuteBuild(Dictionary<string, string> buildArgs)
	{
		if (string.IsNullOrEmpty(settings.username) || string.IsNullOrEmpty(settings.apiToken))
		{
			Debug.LogError("Cannot build without username and API token");
			return;
		}

		EditorCoroutineUtility.StartCoroutine(RequestExecuteBuild(buildArgs), this);
	}

	public void ExecuteScript(string scriptText)
	{
		if (string.IsNullOrEmpty(settings.username) || string.IsNullOrEmpty(settings.apiToken))
		{
			Debug.LogError("Cannot execute script without username and API token");
			return;
		}

		EditorCoroutineUtility.StartCoroutine(RequestExecuteScript(scriptText), this);
	}

	public void ExecuteCommand(string command)
	{
		// '/C' runs the args as a command
		string scriptText = $"println new ProcessBuilder('cmd.exe','/C','{command}').redirectErrorStream(true).start().text";
		ExecuteScript(scriptText);
	}

	public void OpenConsoleText(JenkinsBuild build)
	{
		string tempPath = PathEx.Join(UnityPaths.TempDirectory, $"JenkinsConsoleText_{build.number}.txt");
		Debug.Log(tempPath);
		File.WriteAllText(tempPath, build.consoleText);
		System.Diagnostics.Process.Start(tempPath);
	}

	private JenkinsClient()
	{
		if (Application.isBatchMode)
		{
			// Disable on build machine.
			return;
		}

		settings = JenkinsSettings.GetOrCreate();
		EditorCoroutineUtility.StartCoroutine(Update(), this);
	}

	private IEnumerator RequestExecuteBuild(Dictionary<string, string> buildArgs)
	{
		string url = $"{settings.protocol}{settings.username}:{settings.apiToken}@{settings.serverUrl}:{settings.serverPort}/job/{settings.jobName}/buildWithParameters";

		string message = $"Requesting build with {buildArgs.Count} parameters:";
		foreach (KeyValuePair<string, string> pair in buildArgs)
		{
			message += $"\n\"{pair.Key}\" = \"{pair.Value}\"";
		}
		Debug.Log(message);

		using (UnityWebRequest request = UnityWebRequest.Post(url, buildArgs))
		{
			request.timeout = settings.timeoutSeconds;
			request.certificateHandler = new JenkinsCertificateHandler(settings.expectedCertThumbprint);
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogErrorFormat("Error requesting build with parameters: {0}", request.error);
			}
			else
			{
				Debug.Log("Jenkins request build response: " + request.downloadHandler.text);
			}
		}
	}

	private IEnumerator RequestExecuteScript(string scriptText)
	{
		string url = $"{settings.protocol}{settings.username}:{settings.apiToken}@{settings.serverUrl}:{settings.serverPort}/scriptText";

		Dictionary<string, string> formFields = new Dictionary<string, string>
		{
			{ "script", scriptText }
		};

		using (UnityWebRequest request = UnityWebRequest.Post(url, formFields))
		{
			request.timeout = settings.timeoutSeconds;
			request.certificateHandler = new JenkinsCertificateHandler(settings.expectedCertThumbprint);
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogErrorFormat("Error executing script: {0}", request.error);
			}
			else
			{
				Debug.Log(request.downloadHandler.text);
			}
		}
	}

	private IEnumerator RequestLatestBuildNumber()
	{
		string url = $"{settings.protocol}{settings.username}:{settings.apiToken}@{settings.serverUrl}:{settings.serverPort}/job/{settings.jobName}/api/json?pretty=true&tree=lastCompletedBuild[number]";
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			request.timeout = settings.timeoutSeconds;
			request.certificateHandler = new JenkinsCertificateHandler(settings.expectedCertThumbprint);
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogErrorFormat("Error requesting latest Jenkins build number: {0}", request.error);
			}
			else
			{
				try
				{
					JObject responseObject = JObject.Parse(request.downloadHandler.text);
					int latestBuildNumber = (int) responseObject["lastCompletedBuild"]["number"];
					if (latestBuildNumber > knownBuildNumber)
					{
						knownBuildNumber = latestBuildNumber;

						pendingBuild = new JenkinsBuild();
						pendingBuild.number = latestBuildNumber;

						isBuildResultDirty = true;
						shouldRequestConsoleText = true;
					}
				}
				catch (System.Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	private IEnumerator RequestBuildResult()
	{
		string url = $"{settings.protocol}{settings.username}:{settings.apiToken}@{settings.serverUrl}:{settings.serverPort}/job/{settings.jobName}/{knownBuildNumber}/api/json?pretty=true&tree=result";
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			request.timeout = settings.timeoutSeconds;
			request.certificateHandler = new JenkinsCertificateHandler(settings.expectedCertThumbprint);
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogErrorFormat("Error requesting latest Jenkins build result: {0}", request.error);
			}
			else
			{
				try
				{
					JObject responseObject = JObject.Parse(request.downloadHandler.text);
					string latestResult = (string) responseObject["result"];

					if (latestResult == "SUCCESS")
					{
						Debug.Log($"Jenkins build {knownBuildNumber} success");
						pendingBuild.result = JenkinsBuild.EResult.Success;
					}
					else if (latestResult == "FAILURE")
					{
						Debug.LogError($"Jenkins build {knownBuildNumber} failure");
						pendingBuild.result = JenkinsBuild.EResult.Failure;
					}
					else if (latestResult == "ABORTED")
					{
						Debug.Log($"Jenkins build {knownBuildNumber} aborted");
						pendingBuild.result = JenkinsBuild.EResult.Aborted;
					}
					else
					{
						Debug.LogWarning($"Jenkins build {knownBuildNumber} unknown result \"{latestResult}\"");
						pendingBuild.result = JenkinsBuild.EResult.Failure;
					}
				}
				catch (System.Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	private void ParseConsoleText(string consoleText)
	{
		int searchIndex = 0;
		while (true)
		{
			int compilerOutputStartIndex = consoleText.IndexOf("-----CompilerOutput", searchIndex);
			if (compilerOutputStartIndex < 0)
			{
				break;
			}

			int compilerOutputEndIndex = consoleText.IndexOf("-----EndCompilerOutput", compilerOutputStartIndex);
			if (compilerOutputEndIndex < 0)
			{
				break;
			}

			int errorIndex = compilerOutputStartIndex;
			while (true)
			{
				errorIndex = consoleText.IndexOf("error", errorIndex + 1);
				if (errorIndex < 0 || errorIndex > compilerOutputEndIndex)
				{
					break;
				}

				int startOfLineIndex = consoleText.LastIndexOf('\n', errorIndex);
				int endOfLineIndex = consoleText.IndexOf('\n', errorIndex);
				string errorInfo = consoleText.Substring(startOfLineIndex + 1, endOfLineIndex - startOfLineIndex - 1);
				Debug.LogError("Jenkins: " + errorInfo);
			}

			searchIndex = compilerOutputEndIndex;
		}
	}

	private IEnumerator RequestConsoleText()
	{
		string url = $"{settings.protocol}{settings.username}:{settings.apiToken}@{settings.serverUrl}:{settings.serverPort}/job/{settings.jobName}/{knownBuildNumber}/consoleText";
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			request.timeout = settings.timeoutSeconds;
			request.certificateHandler = new JenkinsCertificateHandler(settings.expectedCertThumbprint);
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogErrorFormat("Error requesting latest Jenkins console text: {0}", request.error);
			}
			else
			{
				pendingBuild.consoleText = request.downloadHandler.text;

				if (!string.IsNullOrEmpty(pendingBuild.consoleText))
				{
					try
					{
						ParseConsoleText(pendingBuild.consoleText);
					}
					catch (System.Exception exception)
					{
						Debug.LogException(exception);
					}
				}

				builds.Add(pendingBuild);
				pendingBuild = null;
			}
		}
	}

	private IEnumerator Update()
	{
		yield return new EditorWaitForSeconds(1.0f);

		while (true)
		{
			if (!string.IsNullOrEmpty(settings.serverUrl) && !string.IsNullOrEmpty(settings.jobName))
			{
				yield return RequestLatestBuildNumber();

				if (isBuildResultDirty)
				{
					isBuildResultDirty = false;
					yield return RequestBuildResult();
				}

				if (shouldRequestConsoleText)
				{
					shouldRequestConsoleText = false;
					yield return RequestConsoleText();
				}
			}
			else
			{
				if (!hasWarnedJenkinsIsNotConfigured)
				{
					hasWarnedJenkinsIsNotConfigured = true;
#if WITH_NOREDIST // Reminder is unhelpful for modders
					Debug.Log("Jenkins connection has not been setup");
#endif
				}
			}

			yield return new EditorWaitForSeconds(Mathf.Max(1.0f, settings.pollingInterval));
		}
	}

	private class JenkinsCertificateHandler : UnityEngine.Networking.CertificateHandler
	{
		protected override bool ValidateCertificate(byte[] certificateData)
		{
			// Nelson 2024-11-18: Unity 2021.3.29f1 is crashing here. Will retry in a future version. Call stack:
			// 0x00007FFF389344F5(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\metadata\metadata.c:6164] do_mono_metadata_type_equal
			// 0x00007FFF3895AD6C(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\metadata\mono - hash.c:122] mono_g_hash_table_find_slot
			// 0x00007FFF389D0DE4(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\metadata\reflection.c:491] mono_type_get_object_checked
			// 0x00007FFF3896CAEE(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\metadata\object.c:2280] mono_class_create_runtime_vtable
			// 0x00007FFF38A27249(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\mini\mini.c:4206] mono_jit_compile_method_inner
			// 0x00007FFF38A2DA7C(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\mini\mini - runtime.c:2700] mono_jit_compile_method_with_opt
			// 0x00007FFF38A2F4C8(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\mini\mini - runtime.c:3309] mono_jit_runtime_invoke
			// 0x00007FFF3896E794(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\metadata\object.c:3066] do_runtime_invoke
			// 0x00007FFF3896E92C(mono - 2.0 - bdwgc)[C:\build\output\Unity - Technologies\mono\mono\metadata\object.c:3113] mono_runtime_invoke
			// 0x00007FF72403D174(Unity) scripting_method_invoke
			// 0x00007FF72401CAB4(Unity) ScriptingInvocation::Invoke
			// 0x00007FF7240176DA(Unity) ScriptingInvocation::Invoke<bool>
			// 0x00007FF7245B7BD7(Unity) CertificateHandlerScript::ValidateCertificate
			// 0x00007FF7245B49EB(Unity) `CurlInstallValidateCertificateCallback'::`2'::EstablishProtectedContext::ValidateCertificate
			return true;
// 			var x509Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificateData);
// 			string actualThumbprint = x509Certificate.Thumbprint;
// 			//Debug.Log($"Actual thumbprint: {actualThumbprint} Expected thumbprint: {expectedCertThumbprint}");
// 			return string.Equals(actualThumbprint, expectedCertThumbprint);
		}

		public JenkinsCertificateHandler(string expectedCertThumbprint)
		{
			expectedCertThumbprint = expectedCertThumbprint.Trim().ToUpper();
			expectedCertThumbprint = expectedCertThumbprint.Replace(":", "");
			this.expectedCertThumbprint = expectedCertThumbprint;
		}

		private string expectedCertThumbprint;
	}

	private JenkinsSettings settings;
	private bool isBuildResultDirty;
	private bool shouldRequestConsoleText;
	private bool hasWarnedJenkinsIsNotConfigured;
	private int knownBuildNumber = -1;
	private JenkinsBuild pendingBuild;
	private List<JenkinsBuild> builds = new List<JenkinsBuild>();

	private static JenkinsClient instance = new JenkinsClient();
}
