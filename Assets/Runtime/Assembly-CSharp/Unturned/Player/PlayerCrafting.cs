////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void CraftingUpdated();
	public delegate void PlayerCraftingRequestHandler(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow);
	public delegate void PlayerCraftingRequestHandlerV2(PlayerCrafting crafting, ref Blueprint blueprint, ref bool shouldAllow);

	/// <summary>
	/// This prevents identical tag provider setups from listing in the UI.
	/// For example, two workbenches providing the same tags shouldn't show two UI listings.
	/// </summary>
	internal struct NearbyCraftingTagProvider : System.IEquatable<NearbyCraftingTagProvider>
	{
		public ICraftingTagProvider component;
		public Asset asset;
		public HashSet<TagAsset> tags;

		public override string ToString()
		{
			return $"(Component: {component} Asset: {asset} Tags: {string.Join(", ", tags)}";
		}

		public bool Equals(NearbyCraftingTagProvider other)
		{
			// None of these should be null because they were validated immediately before this is called by HashSet.
			// Excluding component because that will always be different.
			return asset.Equals(other.asset) && tags.SetEquals(other.tags);
		}
	}

	public enum EBlueprintPreferences
	{
		None,

		/// <summary>
		/// Player does not want to see this blueprint.
		/// </summary>
		Ignored,

		/// <summary>
		/// Player wants to save this blueprint in a special category.
		/// </summary>
		Favorited,
	}

	internal struct BlueprintPreferencesPair
	{
		public byte index;
		public EBlueprintPreferences preferences;
	}

	public class PlayerCrafting : PlayerCaller
	{
		private const byte SAVEDATA_VERSION_BLUEPRINT_IGNORE_BY_GUID = 2;
		private const byte SAVEDATA_VERSION_ADDED_BLUEPRINT_PREFERENCES = 3;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_BLUEPRINT_PREFERENCES;
		
		private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();
		private static InventorySearchQualityDescendingComparator qualityDescendingComparator = new InventorySearchQualityDescendingComparator();
		private static InventorySearchAmountAscendingComparator amountAscendingComparator = new InventorySearchAmountAscendingComparator();
		private static InventorySearchAmountDescendingComparator amountDescendingComparator = new InventorySearchAmountDescendingComparator();
		private static System.Comparison<PlayerInventorySearchResultV2> qualityAscendingComparison = qualityAscendingComparator.Compare;
		private static System.Comparison<PlayerInventorySearchResultV2> qualityDescendingComparison = qualityDescendingComparator.Compare;
		private static System.Comparison<PlayerInventorySearchResultV2> amountAscendingComparison = amountAscendingComparator.Compare;
		private static System.Comparison<PlayerInventorySearchResultV2> amountDescendingComparison = amountDescendingComparator.Compare;

		[System.Obsolete("Use the static onCraftBlueprintRequested for ease-of-use instead.")]
		public PlayerCraftingRequestHandler onCraftingRequested;

		[System.Obsolete("Please use V2 which takes a reference to the underlying blueprint")]
		public static PlayerCraftingRequestHandler onCraftBlueprintRequested;

		public static PlayerCraftingRequestHandlerV2 OnCraftBlueprintRequestedV2;

		public CraftingUpdated onCraftingUpdated;

		/// <summary>
		/// Find nearby crafting tag providers and query their tags.
		/// </summary>
		public void UpdateAvailableCraftingTags()
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			Vector3 position = transform.position + Vector3.up;
			float radius = 8.0f;
			nearbyCraftingTags.Clear();

			if (!channel.IsLocalPlayer)
			{
				CraftingTagPhysicsUtil.QueryAvailableTags(position, radius, nearbyCraftingTags);
				return;
			}

#if !DEDICATED_SERVER
			Profiler.BeginSample("Reset localPlayerNearbyTagProviders");
			foreach (NearbyCraftingTagProvider nearbyCraftingTagProvider in localPlayerNearbyTagProviders)
			{
				tagPool.Push(nearbyCraftingTagProvider.tags);
			}
			localPlayerNearbyTagProviders.Clear();
			Profiler.EndSample(); // Reset localPlayerNearbyTagProviders

			Profiler.BeginSample("QueryTagProviders");
			tempTagProviders.Clear();
			CraftingTagPhysicsUtil.QueryTagProviders(position, radius, tempTagProviders);
			Profiler.EndSample();

			CraftingTagProviderGetAvailableTagsParameters getAvailableTagsParameters = new CraftingTagProviderGetAvailableTagsParameters();
			foreach (ICraftingTagProvider craftingTagProvider in tempTagProviders)
			{
				Asset tagProviderAsset = craftingTagProvider.GetTagProviderAsset();
				if (tagProviderAsset == null)
				{
					if (craftingTagProvider is Component component)
					{
						UnturnedLog.warn($"Crafting tag provider without asset: {component.GetSceneHierarchyPath()}");
					}
					else
					{
						UnturnedLog.warn($"Crafting tag provider without asset: {craftingTagProvider}");
					}
					continue;
				}

				Profiler.BeginSample("GetAvailableTags");
				tempTags.Clear();
				getAvailableTagsParameters.ResultTags = tempTags;
				craftingTagProvider.GetAvailableTags(ref getAvailableTagsParameters);
				Profiler.EndSample(); // GetAvailableTags

				// This ensures inactive tag providers aren't included in UI. :P
				if (tempTags.Count > 0)
				{
					foreach (TagAsset tag in tempTags)
					{
						nearbyCraftingTags.Add(tag);
					}

					NearbyCraftingTagProvider testTagProvider = new NearbyCraftingTagProvider()
					{
						component = craftingTagProvider,
						asset = tagProviderAsset,
						tags = tempTags,
					};

					if (!localPlayerNearbyTagProviders.Contains(testTagProvider))
					{
						localPlayerNearbyTagProviders.Add(testTagProvider);
						if (!tagPool.TryPop(out tempTags))
						{
							tempTags = new HashSet<TagAsset>();
						}
					}
				}
			}

			Profiler.BeginSample("Sort");
			localPlayerNearbyTagProviders.Sort(localPlayerNearbyTagProvidersComparison);
			Profiler.EndSample();

// 			localPlayerNearbyTags.Clear();
// 			localPlayerNearbyTags.AddRange(nearbyCraftingTags);
// 			localPlayerNearbyTags.Sort((TagAsset lhs, TagAsset rhs) =>
// 			{
// 				return lhs.PlainTextName.CompareTo(rhs.PlainTextName);
// 			});
#endif // !DEDICATED_SERVER
		}

		private static System.Comparison<NearbyCraftingTagProvider> localPlayerNearbyTagProvidersComparison = CompareLocalPlayerNearbyTagProviders;
		private static int CompareLocalPlayerNearbyTagProviders(NearbyCraftingTagProvider lhs, NearbyCraftingTagProvider rhs)
		{
			return lhs.asset.FriendlyName.CompareTo(rhs.asset.FriendlyName);
		}

		/// <summary>
		/// Tests whether nearby tags include specified tag.
		/// Doesn't update nearby tags, so call UpdateAvailableCraftingTags if out-of-date.
		/// </summary>
		public bool IsCraftingTagAvailable(TagAsset tag)
		{
			if (tag == null)
				return false;

			return nearbyCraftingTags.Contains(tag);
		}

		public bool isBlueprintBlacklisted(Blueprint blueprint)
		{
			LevelAsset levelAsset = Level.getAsset();
			return levelAsset != null && levelAsset.isBlueprintBlacklisted(blueprint);
		}

		private bool stripAttachments(byte page, ItemJar jar)
		{
			ItemAsset asset = jar.GetAsset();
			if (asset != null)
			{
				if (asset.type == EItemType.GUN && jar.item.state != null && jar.item.state.Length == 18)
				{
					if (((ItemGunAsset) asset).hasSight)
					{
						ushort sightID = System.BitConverter.ToUInt16(jar.item.state, 0);

						if (sightID != 0 && sightID != ((ItemGunAsset) asset).sightID)
						{
							player.inventory.forceAddItem(new Item(sightID, false, jar.item.state[13]), true);

							jar.item.state[0] = 0;
							jar.item.state[1] = 0;
							jar.item.state[13] = 0;
						}
					}

					if (((ItemGunAsset) asset).hasTactical)
					{
						ushort tacticalID = System.BitConverter.ToUInt16(jar.item.state, 2);

						if (tacticalID != 0)
						{
							player.inventory.forceAddItem(new Item(tacticalID, false, jar.item.state[14]), true);

							jar.item.state[2] = 0;
							jar.item.state[3] = 0;
							jar.item.state[14] = 0;
						}
					}

					if (((ItemGunAsset) asset).hasGrip)
					{
						ushort gripID = System.BitConverter.ToUInt16(jar.item.state, 4);

						if (gripID != 0)
						{
							player.inventory.forceAddItem(new Item(gripID, false, jar.item.state[15]), true);

							jar.item.state[4] = 0;
							jar.item.state[5] = 0;
							jar.item.state[15] = 0;
						}
					}

					if (((ItemGunAsset) asset).hasBarrel)
					{
						ushort barrelID = System.BitConverter.ToUInt16(jar.item.state, 6);

						if (barrelID != 0)
						{
							player.inventory.forceAddItem(new Item(barrelID, false, jar.item.state[16]), true);

							jar.item.state[6] = 0;
							jar.item.state[7] = 0;
							jar.item.state[16] = 0;
						}
					}

					if (((ItemGunAsset) asset).allowMagazineChange)
					{
						ushort magazineID = System.BitConverter.ToUInt16(jar.item.state, 8);

						if (magazineID != 0 && jar.item.state[10] > 0)
						{
							player.inventory.forceAddItem(new Item(magazineID, jar.item.state[10], jar.item.state[17]), true);

							jar.item.state[8] = 0;
							jar.item.state[9] = 0;
							jar.item.state[10] = 0;
							jar.item.state[17] = 0;
						}
					}

					return true;
				}
			}

			return false;
		}

		public void removeItem(byte page, ItemJar jar)
		{
			player.inventory.removeItem(page, player.inventory.getIndex(page, jar.x, jar.y));

			stripAttachments(page, jar);
		}

		[System.Obsolete]
		public void askStripAttachments(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveStripAttachments(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendStripAttachments = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerCrafting), nameof(ReceiveStripAttachments));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askStripAttachments))]
		public void ReceiveStripAttachments(byte page, byte x, byte y)
		{
			if (page < PlayerInventory.SLOTS || page >= PlayerInventory.PAGES - 1)
			{
				return;
			}

			if (player.equipment.checkSelection(page, x, y))
			{
				if (player.equipment.isBusy)
				{
					return;
				}

				player.equipment.dequip();
			}

			byte index = player.inventory.getIndex(page, x, y);

			if (index == 255)
			{
				return;
			}

			ItemJar jar = player.inventory.getItem(page, index);

			if (jar == null)
			{
				return;
			}

			if (!stripAttachments(page, jar))
			{
				return;
			}

			player.inventory.sendUpdateInvState(page, x, y, jar.item.state);
		}

		public void sendStripAttachments(byte page, byte x, byte y)
		{
			SendStripAttachments.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellCraft(CSteamID steamID)
		{
			ReceiveRefreshCrafting();
		}

		private static readonly ClientInstanceMethod SendRefreshCrafting = ClientInstanceMethod.Get(typeof(PlayerCrafting), nameof(ReceiveRefreshCrafting));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellCraft))]
		public void ReceiveRefreshCrafting()
		{
			onCraftingUpdated?.Invoke();
		}

		/// <summary>
		/// Requested for plugin use.
		/// Notifies owner they should refresh the crafting menu.
		/// </summary>
		public void ServerRefreshOwnerCrafting()
		{
			SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection());
		}

		internal bool IsBlueprintPermanentlyDisabled(Blueprint blueprint)
		{
			if (isBlueprintBlacklisted(blueprint))
			{
				return true;
			}

			if (blueprint.GetLegacyBlueprintSkill() == EBlueprintSkill.REPAIR && blueprint.level > Provider.modeConfigData.Gameplay.Repair_Level_Max)
			{
				return true;
			}

			if (!string.IsNullOrEmpty(blueprint.map) && !blueprint.map.Equals(Level.info.name, System.StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			if (!Provider.modeConfigData.Gameplay.Allow_Freeform_Buildables && !Provider.modeConfigData.Gameplay.Allow_Freeform_Buildables_On_Vehicles && blueprint.IsOutputFreeformBuildable)
			{
				return true;
			}

			if (blueprint.Operation != EBlueprintOperation.None && (blueprint.TargetItem == null || blueprint.TargetItem.FindItemAsset() == null))
			{
				// Mis-configured?
				return true;
			}

			if (blueprint.RequiresStaticTags != null)
			{
				CachingAssetRef[] requiredTags = blueprint.RequiresStaticTags;
				if (requiredTags != null)
				{
					for (int tagIndex = 0; tagIndex < requiredTags.Length; ++tagIndex)
					{
						ref CachingAssetRef tagRef = ref requiredTags[tagIndex];
						TagAsset tag = tagRef.Get<TagAsset>();
						if (tag == null)
						{
							return true;
						}

						if (!Level.IsTagEnabled(tag))
						{
							UnturnedLog.info($"Cannot craft blueprint {blueprint} because tag {tag} is unavailable");
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Update anything that will not change as blueprint is invoked repeatedly on server.
		/// </summary>
		internal void UpdateBlueprintStaticStatus(in UpdateBlueprintStatusParameters p, bool bypassWorkstationRequirements)
		{
			Blueprint blueprint = p.status.blueprint;

			if (blueprint.RequiresSkill)
			{
				int playerLevel = blueprint.GetPlayerSkillLevel(player);
				if (playerLevel < blueprint.level)
				{
					p.status.isMissingRequiredSkill = true;
					p.logCallback?.Invoke($"skill {blueprint.DebugGetSkillName()} level {playerLevel}) is less than required {blueprint.level}");
					if (p.shouldExitEarly)
					{
						return;
					}
				}
			}

			if (!bypassWorkstationRequirements)
			{
				CachingAssetRef[] requiredTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
				if (requiredTags != null)
				{
					// Caller should UpdateAvailableCraftingTags first! This method may be called in a loop. 
					for (int tagIndex = 0; tagIndex < requiredTags.Length; ++tagIndex)
					{
						ref CachingAssetRef tagRef = ref requiredTags[tagIndex];
						TagAsset tag = tagRef.Get<TagAsset>();
						if (tag == null)
							continue;

						if (!IsCraftingTagAvailable(tag))
						{
							p.status.missingCraftingTagsCount += 1;
							p.logCallback?.Invoke($"requires nearby crafting tag \"{tag.PlainTextName}\"");
							if (p.shouldExitEarly)
							{
								return;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Update anything that can change as blueprint is invoked repeatedly on server.
		/// </summary>
		internal void UpdateBlueprintDynamicStatus(in UpdateBlueprintStatusParameters p)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Debug.Assert(p.status.inputItems.IsEmpty(), "Forgot to call ResetDynamicStatus?");
#endif

			updateBlueprintDynamicStatusSampler.Begin();
			
			Blueprint blueprint = p.status.blueprint;

			if (!blueprint.areConditionsMet(player))
			{
				p.status.isMissingAnyNpcConditions = true;
				p.logCallback?.Invoke("NPC conditions not met");
				if (p.shouldExitEarly)
				{
					updateBlueprintDynamicStatusSampler.End();
					return;
				}
			}

			PlayerInventorySearchResultV2? targetItemRef = null;
			if (blueprint.TargetItem != null)
			{
				BlueprintInputItemStatus targetStatus = p.status.AddTargetItem();
				Profiler.BeginSample("UpdateBlueprintInputItemStatus");
				UpdateBlueprintInputItemStatus(in p, blueprint.TargetItem, targetStatus, null);
				Profiler.EndSample();
				if (targetStatus.isMissingRequiredAmount)
				{
					p.status.isMissingTargetItem = true;
					p.logCallback?.Invoke("missing target item");
					if (p.shouldExitEarly)
					{
						updateBlueprintDynamicStatusSampler.End();
						return;
					}
				}

				targetItemRef = targetStatus.FirstResultOrNull;
			}

			int inputsRequirementsCount = blueprint.supplies?.Length ?? 0;
			// Ignoring target item prevents it from being counted twice (e.g., refilling from same item type)
			ItemJar ignoreTargetItem = targetItemRef?.Jar;
			for (int inputRequirementIndex = 0; inputRequirementIndex < inputsRequirementsCount; ++inputRequirementIndex)
			{
				BlueprintSupply inputItemConfig = blueprint.supplies[inputRequirementIndex];
				BlueprintInputItemStatus inputStatus = p.status.AddInputItem();
				Profiler.BeginSample("UpdateBlueprintInputItemStatus");
				bool error = UpdateBlueprintInputItemStatus(in p, inputItemConfig, inputStatus, ignoreTargetItem);
				Profiler.EndSample();
				if (error && p.shouldExitEarly)
				{
					updateBlueprintDynamicStatusSampler.End();
					return;
				}
			}

			updateBlueprintDynamicStatusSampler.End();
		}

		/// <summary>
		/// Returns true if should exit early.
		/// If updating behavior here please remember to update <see cref="GatherUniqueInputItems"/>.
		/// </summary>
		private bool UpdateBlueprintInputItemStatus(in UpdateBlueprintStatusParameters p,
			BlueprintSupply inputItemConfig, BlueprintInputItemStatus inputStatus, ItemJar ignoreTargetItem)
		{
			Blueprint blueprint = p.status.blueprint;
			ItemAsset inputItemAsset = inputItemConfig.FindItemAsset();
			if (inputItemAsset == null)
			{
				p.status.totalMissingInputItemsCount += inputItemConfig.amount;
				p.status.isMissingAnyCriticalInputItem |= inputItemConfig.isCritical;
				inputStatus.isMissingRequiredAmount = true;
				p.logCallback?.Invoke($"no asset for input item {inputItemConfig.ItemRef}");
				return true;
			}

			Profiler.BeginSample("UpdateBlueprintInputItemStatus.Search");
			PlayerInventorySearchParameters searchParameters = new PlayerInventorySearchParameters()
			{
				Results = inputStatus.searchResults,

				// Nelson 2025-04-02: If consuming items, we exclude player's primary and secondary weapons
				// to avoid getting them in unexpected trouble in the middle of combat. As for storage
				// container, I'm preserving the old behavior (in part because I haven't double-checked
				// whether consuming items from storage would work as-expected here yet).
				IncludeEquipmentSlots = !inputItemConfig.ShouldConsume,
				IncludeActiveStorageContainer = !inputItemConfig.ShouldConsume,

				AssetRef = inputItemConfig.ItemRef,
				IncludeEmpty = inputItemConfig.ShouldIncludeEmptyAmount,
				ExcludeFullAmount = inputItemConfig.ShouldExcludeFullAmount,
				IncludeMaxQuality = inputItemConfig.ShouldIncludeMaxQuality,
				ItemToIgnore = ignoreTargetItem,
			};
			player.inventory.SearchContents(in searchParameters);
			Profiler.EndSample(); // UpdateBlueprintInputItemStatus.Search

			if (inputStatus.searchResults.Count < 1)
			{
				p.status.totalMissingInputItemsCount += inputItemConfig.amount;
				p.status.isMissingAnyCriticalInputItem |= inputItemConfig.isCritical;
				inputStatus.isMissingRequiredAmount = true;
				p.logCallback?.Invoke($"no results for supply item {inputItemAsset}");
				return true;
			}

			p.status.hasAnyInputItem = true;

			Profiler.BeginSample("UpdateBlueprintInputItemStatus.Count");
			switch (inputItemConfig.CountingMethod)
			{
				case ECraftingInputCountingMethod.TotalItems:
					inputStatus.totalAmount = inputStatus.searchResults.Count;
					break;

				case ECraftingInputCountingMethod.TotalAmount:
				{
					foreach (PlayerInventorySearchResultV2 searchResult in inputStatus.searchResults)
					{
						inputStatus.totalAmount += inputItemConfig.ShouldCountEmptyAsOne ? Mathf.Max(1, searchResult.Jar.item.amount) : searchResult.Jar.item.amount;
					}
					break;
				}

				default:
					UnturnedLog.warn($"unhandled crafting input counting method ({inputItemConfig.CountingMethod})");
					Profiler.EndSample(); // UpdateBlueprintInputItemStatus.Count
					return true;
			}
			Profiler.EndSample(); // UpdateBlueprintInputItemStatus.Count

			// Nelson 2025-03-25: !ShouldConsume check is here for backwards compatibility with ammo blueprints
			// requiring tool items.
			if (inputStatus.totalAmount < inputItemConfig.amount)
			{
				p.status.totalMissingInputItemsCount += (inputItemConfig.amount - inputStatus.totalAmount);
				p.status.isMissingAnyCriticalInputItem |= inputItemConfig.isCritical;
				inputStatus.isMissingRequiredAmount = true;
				p.logCallback?.Invoke($"input item ({inputItemAsset}) x{inputStatus.totalAmount} less than required {inputItemConfig.amount}");
				if (p.shouldExitEarly)
				{
					return true;
				}
			}

			if (!inputItemConfig.ShouldConsume)
			{
				// Nelson 2025-04-11: primarily for display purposes. If we have 10 blowtorches and the recipe only
				// needs 1 as a tool we may as well show 1 of 1 instead of 10 of 1.
				inputStatus.totalAmount = Mathf.Min(inputStatus.totalAmount, inputItemConfig.amount);
			}

			Profiler.BeginSample("UpdateBlueprintInputItemStatus.Sort");
			switch (inputItemConfig.Prioritization)
			{
				case ECraftingInputPrioritization.LowestAmount:
					inputStatus.searchResults.Sort(amountAscendingComparison);
					break;

				case ECraftingInputPrioritization.HighestAmount:
					inputStatus.searchResults.Sort(amountDescendingComparison);
					break;

				case ECraftingInputPrioritization.LowestQuality:
					inputStatus.searchResults.Sort(qualityAscendingComparison);
					break;

				case ECraftingInputPrioritization.HighestQuality:
					inputStatus.searchResults.Sort(qualityDescendingComparison);
					break;

				default:
					UnturnedLog.warn($"unhandled crafting input prioritization ({inputItemConfig.Prioritization})");
					Profiler.EndSample(); // UpdateBlueprintInputItemStatus.Sort
					return true;
			}
			Profiler.EndSample(); // UpdateBlueprintInputItemStatus.Sort

			return false;
		}

		/// <summary>
		/// Find all item assets available to the player for crafting.
		/// Used to more quickly identify blueprints that might be craftable, rather than testing all blueprints.
		/// If updating behavior here please remember to update <see cref="UpdateBlueprintInputItemStatus"/>.
		/// </summary>
		internal void GatherUniqueInputItems(HashSet<ItemAsset> results)
		{
			for (int pageIndex = 0; pageIndex <= PlayerInventory.STORAGE; ++pageIndex)
			{
				Items items = player.inventory.items[pageIndex];
				if (items != null)
				{
					items.GatherUniqueItems(results);
				}
			}
		}

		private static readonly ServerInstanceMethod<System.Guid, byte, bool> SendCraft = ServerInstanceMethod<System.Guid, byte, bool>.Get(typeof(PlayerCrafting), nameof(ReceiveCraft));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
		public void ReceiveCraft(in ServerInvocationContext context, System.Guid assetGuid, byte index, bool asManyAsPossible)
		{
			Asset asset = Assets.find(assetGuid);
			if (asset == null)
			{
				context.LogWarning("null asset");
				return;
			}

			ushort preEventAssetId = asset.id;
			ushort postEventAssetId = preEventAssetId;

			bool shouldAllow = true;
#pragma warning disable 0618
			if (onCraftBlueprintRequested != null)
			{
				onCraftBlueprintRequested(this, ref postEventAssetId, ref index, ref shouldAllow);
			}
			else
			{
				onCraftingRequested?.Invoke(this, ref postEventAssetId, ref index, ref shouldAllow);
			}
#pragma warning restore 0618

			if (!shouldAllow)
			{
				context.LogWarning("Cancelled by plugin");
				return;
			}

			if (postEventAssetId != preEventAssetId)
			{
				asset = Assets.find(EAssetType.ITEM, postEventAssetId);
				if (asset == null)
				{
					context.LogWarning($"Unable to use plugin replacement asset ID {postEventAssetId}");
					return;
				}
			}

			IBlueprintOwner blueprintOwner = asset as IBlueprintOwner;
			if (blueprintOwner == null)
			{
				context.LogWarning($"Requested asset {asset} is not a blueprint owner");
				return;
			}

			Blueprint blueprint = blueprintOwner.GetBlueprintByIndex(index);
			if (blueprint == null)
			{
				context.LogWarning($"Index ({index}) does not correspond to a blueprint");
				return;
			}

			HandleCraftRequestInternal(in context, blueprint, asManyAsPossible, /*playEffect*/ true, /*bypassWorkstationRequirements*/ false);
		}

		/// <summary>
		/// Allows housing planner to craft without playing effect, without also allowing
		/// cheaters to craft without playing effect. (if it were an RPC param)
		/// </summary>
		internal bool HandleCraftRequestInternal(in ServerInvocationContext context, Blueprint blueprint, bool asManyAsPossible, bool playEffect, bool bypassWorkstationRequirements)
		{
			if (!Level.IsCraftingAllowedByLevel)
			{
				context.LogWarning("Crafting disabled by level config");
				return false;
			}

			if (player.equipment.isBusy)
			{
				context.LogWarning("Equipment busy");
				return false;
			}

			bool shouldAllow = true;
			if (OnCraftBlueprintRequestedV2 != null)
			{
				try
				{
					OnCraftBlueprintRequestedV2.Invoke(this, ref blueprint, ref shouldAllow);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught plugin exception during OnCraftBlueprintRequestedV2 for {blueprint}:");
				}
			}

			if (!shouldAllow || blueprint == null)
			{
				context.LogWarning("Cancelled by plugin (V2)");
				return false;
			}

			if (IsBlueprintPermanentlyDisabled(blueprint))
			{
				context.LogWarning("Blueprint is permanently disabled");
				return false;
			}

			if (!bypassWorkstationRequirements && blueprint.GetApplicableRequiredNearbyCraftingTags() != null)
			{
				UpdateAvailableCraftingTags();
			}

			activeBlueprintStatus.Reset();
			activeBlueprintStatus.blueprint = blueprint;
			UpdateBlueprintStatusParameters updateStatusParameters = new UpdateBlueprintStatusParameters();
			updateStatusParameters.status = activeBlueprintStatus;
			updateStatusParameters.shouldExitEarly = true;
			//p.logCallback = context.LogWarning;

			UpdateBlueprintStaticStatus(in updateStatusParameters, bypassWorkstationRequirements);
			if (!activeBlueprintStatus.IsCraftable)
			{
				return false;
			}

			bool wasBlueprintExecuted = false;

			// At one point this was a while(true) loop, but some mods have crafting recipes that can repeat infinitely
			// crashing the server in an infinite loop.
			for (int loopCount = 0; loopCount < 64; ++loopCount)
			{
				activeBlueprintStatus.ResetDynamicStatus();
				UpdateBlueprintDynamicStatus(in updateStatusParameters);
				if (!activeBlueprintStatus.IsCraftable)
				{
					break;
				}

				PlayerInventorySearchResultV2? targetItemRef = null;
				if (blueprint.Operation != EBlueprintOperation.None)
				{
					if (blueprint.TargetItem == null)
					{
						context.LogWarning("operation requires target item (bug?)");
						break;
					}

					BlueprintInputItemStatus targetStatus = activeBlueprintStatus.targetStatus;
					if (targetStatus.searchResults.Count < 1)
					{
						context.LogWarning("operation missing target item (bug?)");
						break;
					}

					targetItemRef = targetStatus.searchResults[0];
				}

				if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
				{
					PlayerInventorySearchResultV2 itemToRefill = targetItemRef.Value;
					ItemAsset assetToRefill = itemToRefill.GetAsset();
					int refillAmount = assetToRefill.MaxAmount - itemToRefill.Jar.item.amount;
					if (activeBlueprintStatus.inputItems.Count > 0)
					{
						// Hacky? Currently assuming all amount comes from first input.
						BlueprintInputItemStatus inputStatus = activeBlueprintStatus.inputItems[0];
						refillAmount = Mathf.Min(refillAmount, inputStatus.totalAmount);
						inputStatus.requiredAmountOverride = refillAmount;
						player.inventory.sendUpdateAmount(itemToRefill.Page, itemToRefill.Jar.x, itemToRefill.Jar.y,
							(byte) (itemToRefill.Jar.item.amount + refillAmount));
					}
				}

				for (int inputItemIndex = 0; inputItemIndex < blueprint.supplies.Length; ++inputItemIndex)
				{
					BlueprintSupply inputItemConfig = blueprint.supplies[inputItemIndex];
					if (!inputItemConfig.ShouldConsume)
						continue;

					BlueprintInputItemStatus inputItemStatus = activeBlueprintStatus.inputItems[inputItemIndex];
					List<PlayerInventorySearchResultV2> inputItems = inputItemStatus.searchResults;

					int remainingAmountNeeded = inputItemStatus.requiredAmountOverride > 0
						? inputItemStatus.requiredAmountOverride : inputItemConfig.amount;

					switch (inputItemConfig.CountingMethod)
					{
						case ECraftingInputCountingMethod.TotalItems:
						{
							foreach (PlayerInventorySearchResultV2 inputItem in inputItems)
							{
								inputItem.Delete(player);
								remainingAmountNeeded -= 1;
								if (remainingAmountNeeded == 0)
								{
									break;
								}
							}
							break;
						}

						case ECraftingInputCountingMethod.TotalAmount:
						{
							foreach (PlayerInventorySearchResultV2 inputItem in inputItems)
							{
								if (inputItem.Jar.item.amount == 0 && inputItemConfig.ShouldCountEmptyAsOne)
								{
									inputItem.Delete(player);
									remainingAmountNeeded -= 1;
								}
								else
								{
									uint amountDeleted = inputItem.DeleteAmount(player, (uint) remainingAmountNeeded, false);
									remainingAmountNeeded -= (int) amountDeleted;
								}
								if (remainingAmountNeeded == 0)
								{
									break;
								}
							}
							break;
						}

						default:
							context.LogWarning($"unhandled counting method during blueprint execution! ({inputItemConfig.CountingMethod})");
							break;
					}

					if (remainingAmountNeeded > 0)
					{
						context.LogWarning($"Crafting bug! Asset: {blueprint.GetOwnerAsset()} Input item: {inputItemConfig} Remaining amount: {remainingAmountNeeded}");
					}
				}

				if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
				{
					PlayerInventorySearchResultV2 targetItem = targetItemRef.Value;
					player.inventory.sendUpdateQuality(targetItem.Page, targetItem.Jar.x, targetItem.Jar.y, 100);

					ItemAsset repairedAsset = targetItem.GetAsset();
					if (repairedAsset != null && repairedAsset.type == EItemType.REFILL
						&& targetItem.Jar.item.state.Length == 1) // remarkably hacked together :D
					{
						if ((ERefillWaterType) targetItem.Jar.item.state[0] == ERefillWaterType.DIRTY)
						{
							targetItem.Jar.item.state[0] = (byte) ERefillWaterType.CLEAN;
							player.inventory.sendUpdateInvState(targetItem.Page, targetItem.Jar.x, targetItem.Jar.y, targetItem.Jar.item.state);
						}
					}
				}

				foreach (BlueprintOutput output in blueprint.outputs)
				{
					ItemAsset outputAsset = output.FindItemAsset();
					if (outputAsset == null)
						continue;

					for (int grantedItemIndex = 0; grantedItemIndex < output.amount; grantedItemIndex++)
					{
						if (blueprint.transferState)
						{
							PlayerInventorySearchResultV2 transferFrom = updateStatusParameters.status.inputItems[0].searchResults[0];
							ItemAsset inputAsset = transferFrom.GetAsset();
							Item item = new Item(outputAsset.id, transferFrom.Jar.item.amount, transferFrom.Jar.item.quality, transferFrom.Jar.item.state);

							if (inputAsset != null && inputAsset.type == EItemType.GUN && outputAsset != null && outputAsset.type == EItemType.GUN && item.state.Length >= 12)
							{
								if (blueprint.withoutAttachments)
								{
									for (int zeroindex = 0; zeroindex < item.state.Length; ++zeroindex)
									{
										item.state[zeroindex] = 0;
									}
								}

								ItemGunAsset outputGunAsset = outputAsset as ItemGunAsset;
								if (outputGunAsset != null)
								{
									item.state[11] = (byte) outputGunAsset.firemode;
								}
							}

							player.inventory.forceAddItem(item, true);
						}
						else
						{
							player.inventory.forceAddItem(new Item(outputAsset.id, output.origin), true);
						}
					}
				}

				blueprint.ApplyConditions(player);
				blueprint.GrantRewards(player);

				wasBlueprintExecuted = true;

				if (!asManyAsPossible || blueprint.Operation != EBlueprintOperation.None)
				{
					break;
				}
			}

			if (wasBlueprintExecuted)
			{
				SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection());

				player.sendStat(EPlayerStat.FOUND_CRAFTS);

				if (playEffect)
				{
					EffectAsset buildEffect = blueprint.FindBuildEffectAsset();
					if (buildEffect != null)
					{
						TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(buildEffect);
						triggerEffectParameters.position = transform.position;
						triggerEffectParameters.relevantDistance = EffectManager.SMALL;
						EffectManager.triggerEffect(triggerEffectParameters);

						if (Provider.isServer)
						{
							AlertTool.alert(transform.position, 8);
						}
					}
				}
			}

			return wasBlueprintExecuted;
		}

		[System.Obsolete("Please use SendRequestToCraft which takes a blueprint parameter")]
		public void sendCraft(ushort id, byte index, bool force)
		{
			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
			if (asset != null)
			{
				Blueprint blueprint = asset.GetBlueprintByIndex(index);
				if (blueprint != null)
				{
					SendRequestToCraft(blueprint, force);
				}
			}
		}

		public void SendRequestToCraft(Blueprint blueprint, bool asManyAsPossible)
		{
			Asset ownerAsset = blueprint.GetOwnerAsset();
			if (ownerAsset == null)
			{
				UnturnedLog.warn($"Unable to craft blueprint without owner asset {blueprint}");
				return;
			}

			SendCraft.Invoke(GetNetId(), ENetReliability.Unreliable, ownerAsset.GUID, blueprint.Index, asManyAsPossible);
		}

#if !DEDICATED_SERVER
		internal static System.Action OnLocalPlayerBlueprintPreferencesChanged;
		private static Dictionary<System.Guid, List<BlueprintPreferencesPair>> localPlayerBlueprintPreferences = new Dictionary<System.Guid, List<BlueprintPreferencesPair>>();
		private static int ignoredBlueprintsCount;
		private static int favoritedBlueprintsCount;
		private static bool isLoadingBlueprintPreferences;

		public static bool HasIgnoredAnyBlueprints => ignoredBlueprintsCount > 0;
		public static bool HasFavoritedAnyBlueprints => favoritedBlueprintsCount > 0;

		/// <summary>
		/// Get local player's per-blueprint preferences.
		/// </summary>
		public static EBlueprintPreferences GetBlueprintPreferences(Blueprint blueprint)
		{
			if (blueprint == null)
				return EBlueprintPreferences.None;

			Asset asset = blueprint.GetOwnerAsset();
			if (asset == null)
				return EBlueprintPreferences.None;

			if (localPlayerBlueprintPreferences.TryGetValue(asset.GUID, out List<BlueprintPreferencesPair> perAssetBlueprintPreferences))
			{
				foreach (BlueprintPreferencesPair pair in perAssetBlueprintPreferences)
				{
					if (pair.index == blueprint.Index)
					{
						return pair.preferences;
					}
				}
			}

			return EBlueprintPreferences.None;
		}

		/// <summary>
		/// Set local player's per-blueprint preferences.
		/// This is helpful both to prevent accidentally crafting certain blueprints (like blindfolds) when click to
		/// craft is enabled, and to save frequently used blueprints.
		/// </summary>
		public static void SetBlueprintPreferences(Blueprint blueprint, EBlueprintPreferences preferences)
		{
			if (blueprint == null)
				return;

			Asset ownerAsset = blueprint.GetOwnerAsset();
			if (ownerAsset == null)
			{
				return;
			}

			bool changed;
			if (localPlayerBlueprintPreferences.TryGetValue(ownerAsset.GUID, out List<BlueprintPreferencesPair> perAssetBlueprintPreferences))
			{
				byte blueprintIndex = blueprint.Index;

				int existingIndex = -1;
				for (int searchIndex = 0; searchIndex < perAssetBlueprintPreferences.Count; ++searchIndex)
				{
					if (perAssetBlueprintPreferences[searchIndex].index == blueprintIndex)
					{
						existingIndex = searchIndex;
						break;
					}
				}

				if (existingIndex >= 0)
				{
					// Player already has preferences for this blueprint.

					EBlueprintPreferences oldPreferences = perAssetBlueprintPreferences[existingIndex].preferences;
					changed = (preferences != oldPreferences);
					if (changed)
					{
						switch (oldPreferences)
						{
							case EBlueprintPreferences.Ignored:
								--ignoredBlueprintsCount;
								break;

							case EBlueprintPreferences.Favorited:
								--favoritedBlueprintsCount;
								break;
						}

						if (preferences != EBlueprintPreferences.None)
						{
							perAssetBlueprintPreferences[existingIndex] = new BlueprintPreferencesPair()
							{
								index = blueprint.Index,
								preferences = preferences,
							};
						}
						else
						{
							perAssetBlueprintPreferences.RemoveAt(existingIndex);
						}
					}
				}
				else
				{
					// Player has preferences for this asset, but not this blueprint.

					changed = (preferences != EBlueprintPreferences.None);
					if (changed)
					{
						perAssetBlueprintPreferences.Add(new BlueprintPreferencesPair()
						{
							index = blueprintIndex,
							preferences = preferences,
						});
					}
				}
			}
			else
			{
				// No preferences for this asset yet.
				changed = (preferences != EBlueprintPreferences.None);
				if (changed)
				{
					perAssetBlueprintPreferences = new List<BlueprintPreferencesPair>()
					{
						new BlueprintPreferencesPair()
						{
							index = blueprint.Index,
							preferences = preferences,
						},
					};
					localPlayerBlueprintPreferences.Add(ownerAsset.GUID, perAssetBlueprintPreferences);
				}
			}

			if (changed)
			{
				switch (preferences)
				{
					case EBlueprintPreferences.Ignored:
						++ignoredBlueprintsCount;
						break;

					case EBlueprintPreferences.Favorited:
						++favoritedBlueprintsCount;
						break;
				}
			}

			if (!isLoadingBlueprintPreferences && changed)
			{
				OnLocalPlayerBlueprintPreferencesChanged?.Invoke();
			}
		}
#endif // !DEDICATED_SERVER

		internal void InitializePlayer()
		{
			if (channel.IsLocalPlayer)
			{
#if !DEDICATED_SERVER
				LoadBlueprintPreferences();
#endif
			}
		}

		private void OnDestroy()
		{
			if (channel.IsLocalPlayer)
			{
#if !DEDICATED_SERVER
				SaveBlueprintPreferences();
#endif
			}
		}

#if !DEDICATED_SERVER
		private void LoadBlueprintPreferences()
		{
			isLoadingBlueprintPreferences = true;
			localPlayerBlueprintPreferences.Clear();
			ignoredBlueprintsCount = 0;
			favoritedBlueprintsCount = 0;

			try
			{
				if (ReadWrite.fileExists("/Cloud/Ignored_Blueprints.dat", false))
				{
					Block block = ReadWrite.readBlock("/Cloud/Ignored_Blueprints.dat", false, 0);
					byte version = block.readByte(); // version #

					int numIgnoredBlueprints = block.readInt32();

					// Somehow a player's file got corrupted with 789,184,661 entries!
					numIgnoredBlueprints = Mathf.Min(numIgnoredBlueprints, 10000);

					if (version >= SAVEDATA_VERSION_BLUEPRINT_IGNORE_BY_GUID)
					{
						for (int index = 0; index < numIgnoredBlueprints; index++)
						{
							System.Guid assetGuid = block.readGUID();

							// Use default asset mapping so that preferences are not lost when switching between servers
							// with different mods enabled. Though this does mean some preferences are lost when mods
							// are uninstalled or only installed for a particular server. :(
							Asset asset = Assets.Find_UseDefaultAssetMapping(assetGuid);

							IBlueprintOwner blueprintOwner = asset as IBlueprintOwner;
							// Can't early exit if blueprintOwner or asset is null because we need to read indices.
							int perAssetBlueprintCount = block.readInt32();
							for (int perAssetBlueprintIndex = 0; perAssetBlueprintIndex < perAssetBlueprintCount; ++perAssetBlueprintIndex)
							{
								byte blueprintIndex = block.readByte();
								EBlueprintPreferences preferences = version >= SAVEDATA_VERSION_ADDED_BLUEPRINT_PREFERENCES
									? (EBlueprintPreferences) block.readByte()
									: EBlueprintPreferences.Ignored;
								if (blueprintOwner != null)
								{
									Blueprint blueprint = blueprintOwner.GetBlueprintByIndex(blueprintIndex);
									if (blueprint != null)
									{
										SetBlueprintPreferences(blueprint, preferences);
									}
								}
							}
						}
					}
					else
					{
						for (int index = 0; index < numIgnoredBlueprints; index++)
						{
							ushort itemId = block.readUInt16();
							byte blueprintIndex = block.readByte();

							if (itemId == 0)
								continue; // Bad data?

							ItemAsset asset = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
							if (asset != null)
							{
								Blueprint blueprint = asset.GetBlueprintByIndex(blueprintIndex);
								if (blueprint != null)
								{
									SetBlueprintPreferences(blueprint, EBlueprintPreferences.Ignored);
								}
							}
						}
					}

					OnLocalPlayerBlueprintPreferencesChanged?.Invoke();
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception loading ignored blueprints:");
			}
			isLoadingBlueprintPreferences = false;
		}

		private void SaveBlueprintPreferences()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION_NEWEST);

			block.writeInt32(localPlayerBlueprintPreferences.Count);
			foreach (KeyValuePair<System.Guid, List<BlueprintPreferencesPair>> kvp in localPlayerBlueprintPreferences)
			{
				block.writeGUID(kvp.Key);
				block.writeInt32(kvp.Value.Count);
				foreach (BlueprintPreferencesPair preferencesPair in kvp.Value)
				{
					block.writeByte(preferencesPair.index);
					block.writeByte((byte) preferencesPair.preferences);
				}
			}

			ReadWrite.writeBlock("/Cloud/Ignored_Blueprints.dat", false, block);
		}
#endif // !DEDICATED_SERVER

		/// <summary>
		/// Why isn't tags list public visibility? Because if adding features to (for example) consume a resource when
		/// crafting tag provider is used that will require an API change.
		/// </summary>
		private HashSet<TagAsset> nearbyCraftingTags = new HashSet<TagAsset>();
#if !DEDICATED_SERVER
		internal static List<NearbyCraftingTagProvider> localPlayerNearbyTagProviders = new List<NearbyCraftingTagProvider>();
		//internal static List<TagAsset> localPlayerNearbyTags = new List<TagAsset>();
		private static HashSet<ICraftingTagProvider> tempTagProviders = new HashSet<ICraftingTagProvider>();
		private static HashSet<TagAsset> tempTags = new HashSet<TagAsset>();
		private static Stack<HashSet<TagAsset>> tagPool = new Stack<HashSet<TagAsset>>();
#endif // !DEDICATED_SERVER

		#region Obsolete
		[System.Obsolete]
		public void askCraft(CSteamID steamID, ushort id, byte index, bool force)
		{

		}

		[System.Obsolete("Should not have been called externally to begin with")]
		public void ReceiveCraft(in ServerInvocationContext context, ushort id, byte index, bool force)
		{
			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
			if (asset == null)
			{
				context.LogWarning("null asset");
				return;
			}

			ReceiveCraft(in context, asset.GUID, index, force);
		}

		[System.Obsolete("Removed from dedicated server builds and made static")]
		public bool IsIgnoringAnyBlueprints
		{
			get
			{
#if !DEDICATED_SERVER
				return localPlayerBlueprintPreferences.Count > 0;
#else
				return false;
#endif
			}
		}

		[System.Obsolete("Removed from dedicated server builds and made static")]
		public bool getIgnoringBlueprint(Blueprint blueprint)
		{
#if !DEDICATED_SERVER
			return GetBlueprintPreferences(blueprint) == EBlueprintPreferences.Ignored;
#else
			return false;
#endif
		}

		[System.Obsolete("Removed from dedicated server builds and made static")]
		public void setIgnoringBlueprint(Blueprint blueprint, bool isIgnoring)
		{
#if !DEDICATED_SERVER
			SetBlueprintPreferences(blueprint, isIgnoring ? EBlueprintPreferences.Ignored : EBlueprintPreferences.None);
#endif
		}

#endregion Obsolete

		private static BlueprintStatus activeBlueprintStatus = new BlueprintStatus();

		private static CustomSampler updateBlueprintDynamicStatusSampler = CustomSampler.Create("UpdateBlueprintDynamicStatus");
	}
}
