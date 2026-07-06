////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandQuest : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			if (!Provider.hasCheats)
			{
				CommandWindow.LogError(localization.format("CheatsErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length < 1 || components.Length > 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			bool isMe = false;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player))
			{
				player = PlayerTool.getSteamPlayer(executorID);

				if (player == null)
				{
					CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
					return;
				}
				else
				{
					isMe = true;
				}
			}

			QuestAsset questAsset = null;
			string idString = components[isMe ? 0 : 1];

			System.Guid parsedGuid;
			if (System.Guid.TryParse(idString, out parsedGuid))
			{
				questAsset = Assets.find<QuestAsset>(parsedGuid);
			}
			else
			{
				ushort legacyId;
				if (!ushort.TryParse(idString, out legacyId))
				{
					CommandWindow.LogError(localization.format("InvalidNumberErrorText", idString));
					return;
				}

				questAsset = Assets.find(EAssetType.NPC, legacyId) as QuestAsset;
			}

			if (questAsset != null)
			{
				player.player.quests.ServerAddQuest(questAsset);
			}

			CommandWindow.Log(localization.format("QuestText", player.playerID.playerName, questAsset?.FriendlyName ?? idString));
		}

		public CommandQuest(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("QuestCommandText");
			_info = localization.format("QuestInfoText");
			_help = localization.format("QuestHelpText");
		}
	}
}
