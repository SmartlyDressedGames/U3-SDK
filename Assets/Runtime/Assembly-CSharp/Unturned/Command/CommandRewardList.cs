////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned
{
	public class CommandRewardList : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			if (executorID == CSteamID.Nil)
			{
				// Executed from the server console.
				if (Provider.clients.Count > 0)
				{
					executorID = Provider.clients[0].playerID.steamID;
				}
			}

			Player player = PlayerTool.getPlayer(executorID);
			if (player == null)
				return;

			CachingAssetRef assetRef;
			if (!CachingAssetRef.TryParse(parameter, out assetRef))
			{
				CommandWindow.LogWarning($"Unable to parse \"{parameter}\" as asset");
				return;
			}

			NPCRewardsAsset asset = assetRef.Get<NPCRewardsAsset>();
			if (asset == null)
			{
				CommandWindow.LogWarning($"No reward list for \"{assetRef}\"");
				return;
			}

			if (asset.AreConditionsMet(player))
			{
				CommandWindow.Log($"Running \"{asset.FriendlyName}\"");
				asset.ApplyConditions(player);
				asset.GrantRewards(player);
			}
			else
			{
				CommandWindow.Log($"Cannot run \"{asset.FriendlyName}\" because conditions are unmet:");
				CommandWindow.Log(asset.conditionsList.DebugDumpToString(player));
			}
		}

		public CommandRewardList(Local newLocalization)
		{
			localization = newLocalization;
			_command = "RunRewardList";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
