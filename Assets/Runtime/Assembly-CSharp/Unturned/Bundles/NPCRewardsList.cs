////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public struct NPCRewardsList
	{
		public void Grant(Player player)
		{
			if (rewards != null && rewards.Length > 0)
			{
				try
				{
					foreach (INPCReward reward in rewards)
					{
						// default is -1 so >0 is a safe comparison.
						if (reward.grantDelaySeconds > 0.0f)
						{
							player.quests.GetOrCreateDelayedQuestRewards().GrantReward(reward);
						}
						else
						{
							reward.GrantReward(player);
						}
					}
				}
				catch (System.Exception ex)
				{
					UnturnedLog.exception(ex, $"Caught exception granting NPC reward to {player?.channel?.owner?.playerID}:");
				}
			}
		}

		/// <summary>
		/// This overload supports legacy Reward_# format.
		/// </summary>
		public void Parse(IDatDictionary data, Local localization, Asset assetContext, string countKey, string prefixKey)
		{
			if (!data.TryGetNode(countKey, out IDatNode node))
				return;

			if (node is IDatValue valueNode)
			{
				int count = valueNode.ParseInt32();
				if (count > 0)
				{
					rewards = new INPCReward[count];
#pragma warning disable
					NPCTool.readRewards(data, localization, prefixKey, rewards, assetContext);
#pragma warning restore
				}
			}
			else if (node is IDatList listNode)
			{
				Parse(localization, assetContext, listNode, countKey);
			}
		}

		/// <summary>
		/// This overload doesn't support legacy Reward_# format.
		/// </summary>
		public void Parse(IDatDictionary data, Local localization, Asset assetContext, string key)
		{
			if (data.TryGetList(key, out IDatList listNode))
			{
				Parse(localization, assetContext, listNode, key);
			}
		}

		private void Parse(Local localization, Asset assetContext, IDatList listNode, string countKey)
		{
			tempRewards.Clear();

			int rewardIndex = -1;
			foreach (IDatNode rewardNode in listNode)
			{
				++rewardIndex;
				if (!(rewardNode is IDatDictionary rewardData))
					continue;

				string errorInfo = $"{countKey}[{rewardIndex}]";

				if (!rewardData.TryParseEnum("Type", out ENPCRewardType rewardType))
				{
					if (rewardData.ContainsKey("Type"))
					{
						assetContext.ReportAssetError($"{errorInfo} missing reward Type");
					}
					else
					{
						assetContext.ReportAssetError($"{errorInfo} unable to parse reward type \"{rewardData.GetString("Type")}\"");
					}
					continue;
				}

				System.Type underlyingType = NPCTool.rewardTypes[(int) rewardType];
				if (underlyingType == null)
				{
					assetContext.ReportAssetError($"{errorInfo} unable to create type {rewardType}");
					continue;
				}

				INPCReward reward;
				try
				{
					reward = System.Activator.CreateInstance(underlyingType) as INPCReward;
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception instantiating {underlyingType}:");
					assetContext.ReportAssetError($"{errorInfo} error creating type {rewardType}");
					continue;
				}

				PopulateRewardParameters p = new PopulateRewardParameters(rewardType, rewardData,
					localization, assetContext, errorInfo, null);
				try
				{
					reward.PopulateV2(p);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception populating {errorInfo} {underlyingType}:");
					continue;
				}

				tempRewards.Add(reward);
			}

			if (tempRewards.Count > 0)
			{
				rewards = tempRewards.ToArray();
			}
		}

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output)
		{
			output.AppendLine($"{rewards?.Length} reward(s)");
			if (rewards != null)
			{
				for (int index = 0; index < rewards.Length; ++index)
				{
					output.AppendLine($"[{index}]: {rewards[index]}");
				}
			}
		}

		public string DebugDumpToString()
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			DebugDumpToStringBuilder(output);
			return output.ToString();
		}

		internal INPCReward[] rewards;
		private static List<INPCReward> tempRewards = new List<INPCReward>();

		[System.Obsolete("Removed shouldSend parameter")]
		public void Grant(Player player, bool shouldSend = true)
		{
			Grant(player);
		}
	}
}
