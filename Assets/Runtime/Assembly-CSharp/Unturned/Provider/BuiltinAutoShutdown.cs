////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned
{
	public static class GameUpdateMonitor
	{
		public delegate void GameUpdateDetectedHandler(string newVersion, ref bool shouldShutdown);

		/// <summary>
		/// Event for plugins to be notified when a server update is detected.
		/// 
		/// Some hosts requested this because they run the game as a Windows service and need to shutdown
		/// through their central management system rather than per-process.
		/// </summary>
		public static event GameUpdateDetectedHandler OnGameUpdateDetected;

		internal static void NotifyGameUpdateDetected(string newVersion, ref bool shouldShutdown)
		{
			try
			{
				OnGameUpdateDetected?.Invoke(newVersion, ref shouldShutdown);
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught plugin exception during OnGameUpdateDetected:");
			}
		}
	}

	/// <summary>
	/// Manages scheduled restart for dedicated server.
	/// </summary>
	internal class BuiltinAutoShutdown : MonoBehaviour
	{
		public bool isScheduledShutdownEnabled;
		public System.DateTime scheduledShutdownTime;

		private void InitScheduledShutdown()
		{
			if (!Provider.configData.Server.Enable_Scheduled_Shutdown)
			{
				return;
			}

			System.DateTime parsedDateTime;
			if (!System.DateTime.TryParse(Provider.configData.Server.Scheduled_Shutdown_Time, out parsedDateTime))
			{
				CommandWindow.LogWarning($"Unable to parse scheduled shutdown time \"{Provider.configData.Server.Scheduled_Shutdown_Time}\"");
				return;
			}

			// For example a time like "1:30 am" was entered so assume daily restarts.
			isScheduledShutdownEnabled = true;
			System.DateTime now = System.DateTime.UtcNow;
			scheduledShutdownTime = now.Date + parsedDateTime.ToUniversalTime().TimeOfDay;
			if (scheduledShutdownTime < now)
			{
				scheduledShutdownTime = scheduledShutdownTime.AddDays(1);
			}

			System.TimeSpan timeUntilShutdown = scheduledShutdownTime - System.DateTime.UtcNow;
			CommandWindow.LogFormat($"Shutdown is scheduled for {scheduledShutdownTime.ToLocalTime()} ({timeUntilShutdown:g} from now)");

			scheduledShutdownRealtime = Time.realtimeSinceStartupAsDouble + timeUntilShutdown.TotalSeconds;

			scheduledShutdownWarnings = new List<double>(Provider.configData.Server.Scheduled_Shutdown_Warnings.Length);
			foreach (string warningInput in Provider.configData.Server.Scheduled_Shutdown_Warnings)
			{
				System.TimeSpan parsedTimeSpan;
				if (System.TimeSpan.TryParse(warningInput, out parsedTimeSpan))
				{
					scheduledShutdownWarnings.Add(parsedTimeSpan.TotalSeconds);
				}
				else
				{
					CommandWindow.LogWarning($"Unable to parse scheduled shutdown warning time \"{warningInput}\"");
				}
			}
			scheduledShutdownWarnings.Sort();

			if (scheduledShutdownWarnings.Count > 0)
			{
				double realtimeUntilShutdown = scheduledShutdownRealtime - Time.realtimeSinceStartupAsDouble;
				for (scheduledShutdownWarningIndex = scheduledShutdownWarnings.Count - 1; scheduledShutdownWarningIndex >= 0; --scheduledShutdownWarningIndex)
				{
					if (realtimeUntilShutdown > scheduledShutdownWarnings[scheduledShutdownWarningIndex])
					{
						break;
					}
				}
			}
			else
			{
				scheduledShutdownWarningIndex = -1;
			}
		}

		private void InitUpdateShutdown()
		{
			if (!Provider.configData.Server.Enable_Update_Shutdown || Provider.GetModInfo() != null)
			{
				return;
			}

			if (Dedicator.offlineOnly)
			{
				CommandWindow.LogWarning("Disabling check for game updates because Offline-Only mode is enabled");
				return;
			}

			string betaName = Provider.configData.Server.Update_Steam_Beta_Name;
			if (string.IsNullOrEmpty(betaName))
			{
				CommandWindow.LogWarning("Unable to check for game updates with empty Steam beta name (default is \"public\")");
				return;
			}

			updateShutdownWarnings = new List<double>(Provider.configData.Server.Update_Shutdown_Warnings.Length);
			foreach (string warningInput in Provider.configData.Server.Update_Shutdown_Warnings)
			{
				System.TimeSpan parsedTimeSpan;
				if (System.TimeSpan.TryParse(warningInput, out parsedTimeSpan))
				{
					updateShutdownWarnings.Add(parsedTimeSpan.TotalSeconds);
				}
				else
				{
					CommandWindow.LogWarning($"Unable to parse update shutdown warning time \"{warningInput}\"");
				}
			}
			updateShutdownWarnings.Sort();

			CommandWindow.LogFormat($"Monitoring for game updates on Steam beta branch \"{betaName}\"");
			string url = $"https://smartlydressedgames.com/unturned-steam-versions/{betaName}.txt";
			StartCoroutine(CheckVersion(url));
		}

		private void OnEnable()
		{
			InitScheduledShutdown();
			InitUpdateShutdown();
		}

		private void Update()
		{
			if (isShuttingDownForUpdate)
			{
				double realtimeUntilShutdown = updateShutdownRealtime - Time.realtimeSinceStartupAsDouble;

				if (realtimeUntilShutdown < 0.0)
				{
					isShuttingDownForUpdate = false;
					string key = isUpdateRollback ? "RollbackShutdown_KickExplanation" : "UpdateShutdown_KickExplanation";
					Provider.shutdown(0, Provider.localization.format(key, updateVersionString));
				}
				else if (updateShutdownWarnings.Count > 0 && updateShutdownWarningIndex >= 0)
				{
					if (realtimeUntilShutdown < updateShutdownWarnings[updateShutdownWarningIndex])
					{
						System.TimeSpan timeSpan = new System.TimeSpan(0, 0, (int) updateShutdownWarnings[updateShutdownWarningIndex]);
						string key = isUpdateRollback ? "RollbackShutdown_Timer" : "UpdateShutdown_Timer";
						string message = Provider.localization.format(key, updateVersionString, timeSpan.ToString("g"));
						CommandWindow.Log(message);
						ChatManager.say(message, ChatManager.welcomeColor);
						--updateShutdownWarningIndex;
					}
				}
			}
			else if (isScheduledShutdownEnabled)
			{
				double realtimeUntilShutdown = scheduledShutdownRealtime - Time.realtimeSinceStartupAsDouble;

				if (realtimeUntilShutdown < 0.0)
				{
					isScheduledShutdownEnabled = false;
					Provider.shutdown(0, Provider.localization.format("ScheduledMaintenance_KickExplanation"));
				}
				else if (scheduledShutdownWarnings.Count > 0 && scheduledShutdownWarningIndex >= 0)
				{
					if (realtimeUntilShutdown < scheduledShutdownWarnings[scheduledShutdownWarningIndex])
					{
						System.TimeSpan timeSpan = new System.TimeSpan(0, 0, (int) scheduledShutdownWarnings[scheduledShutdownWarningIndex]);
						string message = Provider.localization.format("ScheduledMaintenance_Timer", timeSpan.ToString("g"));
						CommandWindow.Log(message);
						ChatManager.say(message, ChatManager.welcomeColor);
						--scheduledShutdownWarningIndex;
					}
				}
			}
		}

		private IEnumerator CheckVersion(string url)
		{
			yield return new WaitForSecondsRealtime(60.0f * 5.0f); // Wait 5 minutes before first check, then longer between subsequent checks.

			while (true)
			{
				UnturnedLog.info($"Checking for game updates...");

				using (UnityWebRequest request = UnityWebRequest.Get(url))
				{
					request.timeout = 30;
					yield return request.SendWebRequest();

					if (request.result == UnityWebRequest.Result.Success)
					{
						string versionString = request.downloadHandler.text;
						uint packedVersion;
						if (Parser.TryGetUInt32FromIP(versionString, out packedVersion))
						{
							if (packedVersion != Provider.APP_VERSION_PACKED)
							{
								if (packedVersion > Provider.APP_VERSION_PACKED)
								{
									CommandWindow.Log($"Detected newer game version: {versionString}");
								}
								else
								{
									CommandWindow.Log($"Detected rollback to older game version: {versionString}");
								}

								bool shouldShutdown = true;
								GameUpdateMonitor.NotifyGameUpdateDetected(versionString, ref shouldShutdown);

								// Plugins may want to use our update check without our default shutdown behavior.
								if (shouldShutdown)
								{
									isShuttingDownForUpdate = true;
									isUpdateRollback = packedVersion < Provider.APP_VERSION_PACKED;
									updateVersionString = versionString;
									updateShutdownWarningIndex = updateShutdownWarnings.Count - 1;
									updateShutdownRealtime = Time.realtimeSinceStartupAsDouble + (updateShutdownWarningIndex >= 0 ? updateShutdownWarnings[updateShutdownWarningIndex] : 0.0);
								}

								yield break;
							}
							else
							{
								UnturnedLog.info("Game version is up-to-date");
							}
						}
						else
						{
							UnturnedLog.info($"Unable to parse newest game version \"{versionString}\"");
						}
					}
					else
					{
						UnturnedLog.info($"Network error checking for game updates: \"{request.error}\"");
					}

					yield return new WaitForSecondsRealtime(60.0f * 10.0f); // Wait 10 minutes between subsequent checks.
				}
			}
		}

		private double scheduledShutdownRealtime;

		/// <summary>
		/// Sorted from low to high.
		/// </summary>
		private List<double> scheduledShutdownWarnings;

		private int scheduledShutdownWarningIndex = -1;

		private bool isShuttingDownForUpdate;
		private bool isUpdateRollback;
		private double updateShutdownRealtime;
		private string updateVersionString;

		/// <summary>
		/// Sorted from low to high.
		/// </summary>
		private List<double> updateShutdownWarnings;

		private int updateShutdownWarningIndex = -1;
	}
}
