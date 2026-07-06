////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandTimeout : Command
	{
		private static readonly ushort MIN_NUMBER = 50;
		private static readonly ushort MAX_NUMBER = 10000;

		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			ushort timeout;
			if (!ushort.TryParse(parameter, out timeout))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			if (timeout < MIN_NUMBER)
			{
				CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
				return;
			}

			if (timeout > MAX_NUMBER)
			{
				CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
				return;
			}

			if (Provider.configData != null)
			{
				Provider.configData.Server.Max_Ping_Milliseconds = timeout;
			}

			CommandWindow.Log(localization.format("TimeoutText", timeout));
		}

		public CommandTimeout(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("TimeoutCommandText");
			_info = localization.format("TimeoutInfoText");
			_help = localization.format("TimeoutHelpText");
		}
	}
}