////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal enum EDelayedQuestRewardsInterruption
	{
		NotInterrupted,
		Death,
		Disconnection,
		Shutdown,
	}

	public class PlayerDelayedQuestRewardsComponent : MonoBehaviour
	{
		internal void GrantReward(INPCReward reward)
		{
			StartCoroutine(GrantRewardCoroutine(reward));
		}

		internal void Interrupt(EDelayedQuestRewardsInterruption interruption)
		{
			StopAllCoroutines();
			foreach (INPCReward reward in rewardsToApplyWhenInterrupted)
			{
				GrantRewardSafe(reward, interruption);
			}
			rewardsToApplyWhenInterrupted.Clear();
		}

		private IEnumerator GrantRewardCoroutine(INPCReward reward)
		{
			bool shouldApplyWhenInterrupted = reward.grantDelayApplyWhenInterrupted;
			if (shouldApplyWhenInterrupted)
			{
				rewardsToApplyWhenInterrupted.Add(reward);
			}
			yield return new WaitForSeconds(reward.grantDelaySeconds);
			GrantRewardSafe(reward, EDelayedQuestRewardsInterruption.NotInterrupted);
			if (shouldApplyWhenInterrupted)
			{
				// List<T>.Remove removes only the first occurrence, so instances added later will remain.
				rewardsToApplyWhenInterrupted.Remove(reward);
			}
		}

		private void GrantRewardSafe(INPCReward reward, EDelayedQuestRewardsInterruption interruption)
		{
			try
			{
				reward.GrantReward(player);
			}
			catch (System.Exception ex)
			{
				UnturnedLog.exception(ex, $"Caught exception granting delayed NPC reward to {player?.channel?.owner?.playerID} ({interruption}):");
			}
		}

		public Player player;
		private List<INPCReward> rewardsToApplyWhenInterrupted = new List<INPCReward>();
	}
}
