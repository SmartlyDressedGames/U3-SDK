////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Represents whether a player can craft a provided blueprint. If yes, which items to use, if no, why not.
	/// Previously, some of this data was (confusingly) stored in the blueprint definition.
	/// For performance, caller should re-use a list of BlueprintStatus and *not* discard unused results.
	/// </summary>
	internal class BlueprintStatus
	{
		public Blueprint blueprint;

		public List<BlueprintInputItemStatus> inputItems = new List<BlueprintInputItemStatus>();
		public BlueprintInputItemStatus targetStatus;
		public bool isMissingRequiredSkill;
		/// <summary>
		/// Total number of missing required nearby crafting tags.
		/// </summary>
		public int missingCraftingTagsCount;
		public bool isMissingAnyNpcConditions;
		public bool isMissingTargetItem;
		/// <summary>
		/// Total required input item count minus available input item count.
		/// </summary>
		public int totalMissingInputItemsCount;
		public bool isMissingAnyCriticalInputItem;
		public bool hasAnyInputItem;

		public bool IsCraftable
		{
			get
			{
				return !(isMissingRequiredSkill || missingCraftingTagsCount > 0 || isMissingAnyNpcConditions
					|| isMissingTargetItem || totalMissingInputItemsCount > 0 || isMissingAnyCriticalInputItem);
			}
		}

		/// <summary>
		/// Currently only used by housing planner.
		/// Doesn't work with NPC conditions / rewards.
		/// </summary>
		public int EstimateMaxCraftingRepeatCount()
		{
			if (!IsCraftable)
			{
				return 0;
			}

			int minTimesCrafted = -1;

			// We can assume at least ONE is craftable because IsCraftable is true,
			// so we skip and ShouldConsume(false) inputs.
			for (int index = 0; index < blueprint.supplies.Length; ++index)
			{
				BlueprintSupply inputItemConfig = blueprint.supplies[index];
				BlueprintInputItemStatus inputItemStatus = inputItems[index];

				if (inputItemConfig.ShouldConsume)
				{
					int amount = inputItemStatus.totalAmount / inputItemConfig.amount;
					if (minTimesCrafted == -1)
					{
						minTimesCrafted = amount;
					}
					else
					{
						minTimesCrafted = Mathf.Min(minTimesCrafted, amount);
					}
				}
			}

			return Mathf.Max(0, minTimesCrafted);
		}

		/// <summary>
		/// Currently only used by housing planner.
		/// Doesn't work with NPC conditions / rewards.
		/// </summary>
		public int EstimateOutputMaxAmount(int outputIndex)
		{
			if (outputIndex < 0 || blueprint.outputs == null || outputIndex >= blueprint.outputs.Length)
			{
				return 0;
			}

			return blueprint.outputs[outputIndex].amount * EstimateMaxCraftingRepeatCount();
		}

#if !DEDICATED_SERVER
		/// <summary>
		/// Used to sort blueprints from "most craftable" (1) to "least craftable" (0).
		/// </summary>
		public float normalizedCraftability;

		internal void UpdateCraftabilityScore()
		{
			// Nelson 2025-04-29: initially, this added points for every missing requirement. But, that meant a blueprint
			// with a mix of available/unavailable requirements could sort *after* a blueprint without any available.
			// So, now using a "utility score" approach.

			normalizedCraftability = 1.0f;
			if (!blueprint.supplies.IsNullOrEmpty())
			{
				for (int index = 0; index < blueprint.supplies.Length; ++index)
				{
					BlueprintSupply inputItemConfig = blueprint.supplies[index];
					BlueprintInputItemStatus inputItemStatus = inputItems[index];
					if (inputItemStatus.isMissingRequiredAmount)
					{
						float hasPercentage = Mathf.Clamp01(inputItemStatus.totalAmount / (float) inputItemConfig.amount);
						normalizedCraftability *= Mathf.Lerp(0.1f, 1.0f, hasPercentage);
					}
				}
			}
			if (blueprint.TargetItem != null && targetStatus.isMissingRequiredAmount)
			{
				float hasPercentage = Mathf.Clamp01(targetStatus.totalAmount / (float) blueprint.TargetItem.amount);
				normalizedCraftability *= Mathf.Lerp(0.1f, 1.0f, hasPercentage);
			}

			if (isMissingRequiredSkill)
			{
				normalizedCraftability *= 0.1f;
			}
			if (missingCraftingTagsCount > 0)
			{
				CachingAssetRef[] requiredTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
				float missingTagsPerc = missingCraftingTagsCount / (float) requiredTags.Length;
				normalizedCraftability *= Mathf.Lerp(1.0f, 0.1f, missingTagsPerc);
			}
			if (isMissingAnyNpcConditions)
			{
				normalizedCraftability *= 0.1f;
			}
		}
#endif // !DEDICATED_SERVER

		public void Reset()
		{
			isMissingRequiredSkill = false;
			missingCraftingTagsCount = 0;
			ResetDynamicStatus();
		}

		/// <summary>
		/// Reset values set by PlayerCrafting.UpdateBlueprintDynamicStatus.
		/// </summary>
		public void ResetDynamicStatus()
		{
			isMissingAnyNpcConditions = false;
			isMissingTargetItem = false;
			totalMissingInputItemsCount = 0;
			isMissingAnyCriticalInputItem = false;
			hasAnyInputItem = false;
			inputItemsPool.AddRange(inputItems);
			inputItems.Clear();
			if (targetStatus != null)
			{
				inputItemsPool.Add(targetStatus);
				targetStatus = null;
			}
		}

		public BlueprintInputItemStatus AddInputItem()
		{
			BlueprintInputItemStatus result = CreateInputItemStatus();
			inputItems.Add(result);
			return result;
		}

		public BlueprintInputItemStatus AddTargetItem()
		{
			targetStatus = CreateInputItemStatus();
			return targetStatus;
		}

		internal void GetPreviewOutputTransferState(ItemAsset outputItemAsset, out byte quality, out byte[] state)
		{
			if (blueprint.withoutAttachments)
			{
				// This is kind of silly. It matches PlayerCrafting behavior of zeroing the state array.
				quality = 100;
				state = new byte[18];
			}
			else
			{
				Item transferFromItem = null;
				if (inputItems.Count > 0 && inputItems[0].searchResults.Count > 0)
				{
					transferFromItem = inputItems[0].FirstItemOrNull;
				}
				if (transferFromItem != null)
				{
					quality = transferFromItem.quality;
					state = new byte[transferFromItem.state.Length];
					transferFromItem.state.CopyTo(state, 0);
				}
				else
				{
					quality = 100;
					ItemAsset transferFromAsset = null;
					if (!blueprint.supplies.IsNullOrEmpty())
					{
						transferFromAsset = blueprint.supplies[0].FindItemAsset();
					}
					if (transferFromAsset != null)
					{
						state = transferFromAsset.getState();
					}
					else
					{
						state = outputItemAsset.getState();
					}
				}
			}
		}

		private BlueprintInputItemStatus CreateInputItemStatus()
		{
			BlueprintInputItemStatus result;
			if (inputItemsPool.Count > 0)
			{
				result = inputItemsPool.GetAndRemoveTail();
				result.searchResults.Clear();
			}
			else
			{
				result = new BlueprintInputItemStatus()
				{
					searchResults = new List<PlayerInventorySearchResultV2>(),
				};
			}
			result.totalAmount = 0;
			result.requiredAmountOverride = 0;
			result.isMissingRequiredAmount = false;
			return result;
		}

		private static List<BlueprintInputItemStatus> inputItemsPool = new List<BlueprintInputItemStatus>();
	}

	internal class BlueprintInputItemStatus
	{
		public List<PlayerInventorySearchResultV2> searchResults;
		public int totalAmount;
		/// <summary>
		/// If not zero, use this amount instead of <see cref="BlueprintSupply.amount"/>.
		/// Used by <see cref="EBlueprintOperation.FillTargetItem"/> as amount of ammo needed.
		/// </summary>
		public int requiredAmountOverride;
		public bool isMissingRequiredAmount;

		public PlayerInventorySearchResultV2? FirstResultOrNull => searchResults.Count > 0 ? searchResults[0] : null;
		public Item FirstItemOrNull => FirstResultOrNull?.Jar?.item;
	}

	internal struct UpdateBlueprintStatusParameters
	{
		public BlueprintStatus status;

		/// <summary>
		/// If true, cancel updating status as soon as anything goes wrong.
		/// False for client UI where all info about blueprint is needed for display.
		/// True on server where extra processing is a waste.
		/// </summary>
		public bool shouldExitEarly;

		/// <summary>
		/// If set, log errors here.
		/// </summary>
		public System.Action<string> logCallback;
	}
}
