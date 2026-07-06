////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandVotify : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 6)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			bool voteAllowed = false;
			if (components[0].ToLower() == "y")
			{
				voteAllowed = true;
			}
			else if (components[0].ToLower() == "n")
			{
				voteAllowed = false;
			}
			else
			{
				CommandWindow.LogError(localization.format("InvalidBooleanErrorText", components[0]));
				return;
			}

			float passCooldown;
			if (!float.TryParse(components[1], out passCooldown))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[1]));
				return;
			}

			float failCooldown;
			if (!float.TryParse(components[2], out failCooldown))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[2]));
				return;
			}

			float voteDuration;
			if (!float.TryParse(components[3], out voteDuration))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[3]));
				return;
			}

			float votePercentage;
			if (!float.TryParse(components[4], out votePercentage))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[4]));
				return;
			}

			byte votePlayers;
			if (!byte.TryParse(components[5], out votePlayers))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[5]));
				return;
			}

			ChatManager.voteAllowed = voteAllowed;
			ChatManager.votePassCooldown = passCooldown;
			ChatManager.voteFailCooldown = failCooldown;
			ChatManager.voteDuration = voteDuration;
			ChatManager.votePercentage = votePercentage;
			ChatManager.votePlayers = votePlayers;

			CommandWindow.Log(localization.format("VotifyText"));
		}

		public CommandVotify(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("VotifyCommandText");
			_info = localization.format("VotifyInfoText");
			_help = localization.format("VotifyHelpText");
		}
	}
}