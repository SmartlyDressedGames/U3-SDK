////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandChatrate : Command
	{
		private static readonly float MIN_NUMBER = 0;
		private static readonly float MAX_NUMBER = 60;

		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			float chatrate;
			if (!float.TryParse(parameter, out chatrate))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			if (chatrate < MIN_NUMBER)
			{
				CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
				return;
			}

			if (chatrate > MAX_NUMBER)
			{
				CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
				return;
			}

			ChatManager.chatrate = chatrate;

			CommandWindow.Log(localization.format("ChatrateText", chatrate));
		}

		public CommandChatrate(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ChatrateCommandText");
			_info = localization.format("ChatrateInfoText");
			_help = localization.format("ChatrateHelpText");
		}
	}
}