////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace Unturned.UnityEditorEx
{
	public static class ShellUtils
	{
		/// <summary>
		/// Run cmd.exe with arguments. Redirect standard output/error to Unity's logging.
		/// </summary>
		public static int RunCmdExe(string arguments)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/C " + arguments; // '/C' runs the args as a command
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false; // Must be false to redirect IO.
			process.StartInfo.RedirectStandardOutput = true;
			process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((object sender, System.Diagnostics.DataReceivedEventArgs eventArgs) =>
			{
				if (!string.IsNullOrWhiteSpace(eventArgs.Data))
				{
					Debug.Log(eventArgs.Data.Trim());
				}
			});
			process.StartInfo.RedirectStandardError = true;
			process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((object sender, System.Diagnostics.DataReceivedEventArgs eventArgs) =>
			{
				if (!string.IsNullOrWhiteSpace(eventArgs.Data))
				{
					Debug.LogError(eventArgs.Data.Trim());
				}
			});
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();
			return process.ExitCode;
		}
	}
}
