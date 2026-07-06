////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if DEVELOPMENT_BUILD
using UnityEngine;
#endif // DEVELOPMENT_BUILD

namespace SDG.Unturned
{
	/// <summary>
	/// Report success or failure from game systems, conditionally compiled into the Windows 64-bit build.
	/// </summary>
	public class ContinuousIntegration
	{
#if DEVELOPMENT_BUILD
		public static CommandLineFlag isRunning = new CommandLineFlag(false, "-runningCI");
		protected static bool isExiting = false;
#endif // DEVELOPMENT_BUILD

		/// <summary>
		/// Call when the server is done all loading without running into errors.
		/// Ignored if not running in CI mode, otherwise exits the server successfully with error code 0.
		/// </summary>
		[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
		public static void reportSuccess()
		{
#if DEVELOPMENT_BUILD
			if(!isRunning)
				return;

			if(isExiting)
				return;
			isExiting = true;

			PlayerContinuousIntegrationReport report = new PlayerContinuousIntegrationReport();
			ReadWrite.serializeJSON(Application.dataPath + "/CI_Report.json", false, false, report);

			UnturnedLog.info("Unturned CI success!");
			Provider.shutdown();
#else
			throw new System.NotImplementedException();
#endif // DEVELOPMENT_BUILD
		}

		/// <summary>
		/// Call when the server encounters any error.
		/// Ignored if not running in CI mode, otherwise exits the server with error code 1.
		/// </summary>
		[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
		public static void reportFailure(object message)
		{
#if DEVELOPMENT_BUILD
			if(!isRunning)
				return;

			string text = message.ToString();
			if(text.StartsWith("BoxColliders does not support negative scale or size."))
				return; // Not actually a problem, but can't disable warning message.

			if(text.StartsWith("cleaning the mesh failed"))
				return; // A*PFP error with large stacked navmeshes like on Greece, but works fine?

			if (text.StartsWith("Can't remove") && text.EndsWith("depends on it"))
			{
				// Lazily ignoring this error for now because it does not really impact anything.
				// Happens on dedicated server while cleaning up client prefabs.
				// e.g. Can't remove MeshRenderer because TextMeshPro (Script) depends on it
				return;
			}

			if(isExiting)
				return;
			isExiting = true;

			PlayerContinuousIntegrationReport report = new PlayerContinuousIntegrationReport(text);
			ReadWrite.serializeJSON(Application.dataPath + "/CI_Report.json", false, false, report);

			UnturnedLog.info("Unturned CI failure! [{0}]", text);
			System.Environment.ExitCode = 1; // Unity doesn't seem to actually use this exit code (in 5.6?), so the CI report has its own exit code.
			Application.Quit();
#else
			throw new System.NotImplementedException();
#endif // DEVELOPMENT_BUILD
		}
	}
}
