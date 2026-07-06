////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandAdmins : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (SteamAdminlist.list.Count == 0)
			{
				CommandWindow.LogError(localization.format("NoAdminsErrorText"));
				return;
			}

			CommandWindow.Log(localization.format("AdminsText"));
			for (int index = 0; index < SteamAdminlist.list.Count; index++)
			{
				SteamAdminID adminID = SteamAdminlist.list[index];

				CommandWindow.Log(localization.format("AdminNameText", adminID.playerID));
				CommandWindow.Log(localization.format("AdminJudgeText", adminID.judgeID));
			}
		}

		public CommandAdmins(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("AdminsCommandText");
			_info = localization.format("AdminsInfoText");
			_help = localization.format("AdminsHelpText");
		}
	}
}