////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandBind : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (!Parser.checkIP(parameter))
			{
				CommandWindow.LogError(localization.format("InvalidIPErrorText", parameter));
				return;
			}

			if (Provider.isServer)
			{
				CommandWindow.LogError(localization.format("RunningErrorText"));
				return;
			}

			Provider.ip = Parser.getUInt32FromIP(parameter);
			Provider.bindAddress = parameter;
			CommandWindow.Log(localization.format("BindText", parameter));
		}

		public CommandBind(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("BindCommandText");
			_info = localization.format("BindInfoText");
			_help = localization.format("BindHelpText");
		}
	}
}
