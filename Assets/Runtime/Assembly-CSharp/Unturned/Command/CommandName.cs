////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandName : Command
	{
		private static readonly byte MIN_LENGTH = 5;
		private static readonly byte MAX_LENGTH = 50;

		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (parameter.Length < MIN_LENGTH)
			{
				CommandWindow.LogError(localization.format("MinLengthErrorText", MIN_LENGTH));
				return;
			}

			if (parameter.Length > MAX_LENGTH)
			{
				CommandWindow.LogError(localization.format("MaxLengthErrorText", MAX_LENGTH));
				return;
			}

			Provider.serverName = parameter;
			CommandWindow.Log(localization.format("NameText", parameter));
		}

		public CommandName(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("NameCommandText");
			_info = localization.format("NameInfoText");
			_help = localization.format("NameHelpText");
		}
	}
}