////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandDialogue : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			SteamPlayer player = PlayerTool.getSteamPlayer(executorID);
			if (player == null || player.player == null)
				return;

			CachingAssetRef assetRef;
			if (!CachingAssetRef.TryParse(parameter, out assetRef))
			{
				return;
			}

			DialogueAsset dialogueAsset = assetRef.Get<DialogueAsset>();
			if (dialogueAsset == null)
			{
				return;
			}

			player.player.quests.ApproveTalkWithNpcRequest(player.player, dialogueAsset);
		}

		public CommandDialogue(Local newLocalization)
		{
			localization = newLocalization;
			_command = "Dialogue";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
