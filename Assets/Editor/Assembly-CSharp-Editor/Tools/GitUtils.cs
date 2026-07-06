////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Diagnostics;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	internal static class GitUtils
	{
		public static bool TryGetCommitMessageBody(out string message)
		{
			// Refer to https://git-scm.com/docs/git-log for explanation of git log.
			// --max-count=1 limits log to the most recent commit, rather than all history.
			// --format=%b outputs only the message body.
			string commandLine = "log --max-count=1 --format=%b";

			Process gitProcess = new Process();
			gitProcess.StartInfo.FileName = "git.exe";
			gitProcess.StartInfo.Arguments = commandLine;
			gitProcess.StartInfo.UseShellExecute = false; // Must be false to redirect IO
			gitProcess.StartInfo.CreateNoWindow = true; // Otherwise CMD window opens.
			gitProcess.StartInfo.WorkingDirectory = UnityPaths.ProjectDirectory.FullName;

			string output = string.Empty;

			gitProcess.StartInfo.RedirectStandardOutput = true;
			gitProcess.OutputDataReceived += (object sender, DataReceivedEventArgs eventArgs) =>
			{
				output += eventArgs.Data;
			};

			gitProcess.Start();
			gitProcess.BeginOutputReadLine();
			bool exited = gitProcess.WaitForExit(1000);
			message = output;
			return exited && (gitProcess.ExitCode == 0);
		}
	}
}
