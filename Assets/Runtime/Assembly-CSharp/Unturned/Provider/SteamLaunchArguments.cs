////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using Steamworks;

namespace SDG.Unturned
{
	public class SteamLaunchArguments
	{
		public static string Get() => commandLine;

		internal static void Init()
		{
			// Note: see also Provider.OnNewUrlLaunchParametersPosted.
			const int bufferSize = 2048;
			int resultLength = SteamApps.GetLaunchCommandLine(out string result, bufferSize);
			if (resultLength > 0 && !string.IsNullOrEmpty(result))
			{
				commandLine = result;
				UnturnedLog.info($"Steam launch command-line: \"{commandLine}\"");

				// Playing it safe here because Steam's documentation doesn't specify whether enabling this option:
				// > Enable using ISteamApps::GetLaunchCommandLine().
				// > This will disable the warning popup in Steam when your app is launched with a command line.
				// also stops appending Steam launch options to environment command-line. Don't want to risk enabling
				// before the update is released in case it breaks rich presence joins in the current stable version
				// (by removing from environment command-line).
				if (string.IsNullOrEmpty(CommandLine.commandLineOverride))
				{
					UnturnedLog.info("Overriding environment command-line with Steam's because environment's is empty");
					CommandLine.commandLineOverride = result;
				}
				else
				{
					if (CommandLine.commandLineOverride.Contains(commandLine))
					{
						UnturnedLog.info("Skipping Steam's command-line because environment's already contains it");
					}
					else
					{
						UnturnedLog.info("Appending Steam's command-line to environment's because environment's is not empty");
						CommandLine.commandLineOverride = $"{CommandLine.commandLineOverride} {result}";
					}
				}
			}
			else
			{
				UnturnedLog.info("Steam launch command-line is empty");
			}
		}

		private static string commandLine = string.Empty;
	}
}
#endif // !DEDICATED_SERVER
