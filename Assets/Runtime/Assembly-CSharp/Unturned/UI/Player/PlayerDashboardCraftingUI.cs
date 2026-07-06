////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// Nelson 2025-04-29: I think the available tags might be more confusing than they're worth. Adding this option to
// remove for the meantime, but can restore easily if desired.
//#define ENABLE_AVAILABLE_TAGS_DISPLAY

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class PlayerDashboardCraftingUI
	{
		public static Local localization;
		private static SleekFullscreenBox container;
		public static IconsBundle icons;
		public static bool active;

		private static ISleekBox backdropBox;
		private static ISleekField searchField;
		private static ISleekButton searchButton;
		/// <summary>
		/// List of all loaded blueprints potentially craftable by player. Updated when assets are refreshed. This
		/// allows us to skip blueprints that will never be craftable (such as level-specific blueprints).
		/// </summary>
		private static List<Blueprint> loadedBlueprints;
		private static int assetListChangeCounter;
		/// <summary>
		/// Recycled list of assets with blueprints.
		/// </summary>
		private static List<IBlueprintOwner> blueprintOwners = new List<IBlueprintOwner>();
		/// <summary>
		/// Subset of loadedBlueprints.
		/// </summary>
		private static List<Blueprint> filteredBlueprints = new List<Blueprint>();
		private static List<BlueprintStatus> visibleBlueprints;

		/// <summary>
		/// Center column.
		/// </summary>
		private static ISleekElement blueprintsContainer;

		private static SleekButtonIcon filteringDescriptionButton;
		private static SleekList<BlueprintStatus> blueprintsScrollBox;
		private static Stack<SleekBlueprint> pooledBlueprintWidgets;
		private static ISleekBox blueprintsListEmptyInfoBox;
		private static ISleekButton resetFiltersButton;
		private static ISleekToggle hideUncraftableToggle;
		private static ISleekToggle showIgnoredToggle;
		private static SleekButtonIcon favoritesButton;

		/// <summary>
		/// Used by inventory item context menu to override which blueprints are shown.
		/// </summary>
		public static Blueprint[] filteredBlueprintsOverride;
		private static HashSet<TagAsset> filterAnyOfCategories;
		private static HashSet<TagAsset> filterRequiresAnyOfTags;
		private static ICraftingTagProvider filterTagProvider;
		private static List<BlueprintStatus> updatedBlueprints;
		private static List<BlueprintStatus> blueprintStatusPool;
		private static bool hideUncraftable;
		private static bool showIgnored;
		private static bool filterFavorites;
		private static string itemTextFilter;

		/// <summary>
		/// Left-hand column.
		/// </summary>
		private static ISleekScrollView filtersScrollView;

		private static ISleekElement categoriesContainer;
		private static ISleekLabel categoriesHeader;
		private static List<SleekTagButton> categoryTagButtons;

		private static ISleekElement tagProvidersContainer;
		private static ISleekLabel tagProvidersHeader;
		private static List<SleekCraftingTagProviderButton> tagProviderButtons;
#if ENABLE_AVAILABLE_TAGS_DISPLAY
		private static ISleekElement availableTagsContainer;
		private static ISleekLabel availableTagsHeader;
		private static List<SleekTagButton> availableTagButtons;
#endif // ENABLE_AVAILABLE_TAGS_DISPLAY

		/// <summary>
		/// Right-hand column.
		/// </summary>
		private static SleekSelectedBlueprint selectedBlueprintMenu;

		private static void SetSelectedBlueprintStatus(BlueprintStatus status)
		{
			selectedBlueprintMenu.IsVisible = status != null;
			selectedBlueprintMenu.SetSelectedBlueprintStatus(status);
			blueprintsContainer.SizeOffset_X = selectedBlueprintMenu.IsVisible ? -500 : -260;
		}

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			RefreshBlueprintList();

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			filteredBlueprintsOverride = null;

			container.AnimateOutOfView(0, 1);
		}

		internal static void BuildNotCraftableTooltip(System.Text.StringBuilder craftTooltipBuilder,
			BlueprintStatus status, EBlueprintPreferences preferences)
		{
			craftTooltipBuilder.AppendLine(localization.format("NotCraftable_Header"));

			Blueprint blueprint = status.blueprint;

			if (status.isMissingTargetItem && blueprint.TargetItem != null)
			{
				ItemAsset targetItemAsset = blueprint.TargetItem.FindItemAsset();
				if (targetItemAsset != null)
				{
					craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
					craftTooltipBuilder.AppendFormat(localization.format("NotCraftable_MissingInputItem"),
						targetItemAsset.itemName);
					craftTooltipBuilder.AppendLine();
				}
			}

			if (status.totalMissingInputItemsCount > 0 && !blueprint.supplies.IsNullOrEmpty())
			{
				for (int inputItemIndex = 0; inputItemIndex < blueprint.supplies.Length; ++inputItemIndex)
				{
					BlueprintSupply inputItemConfig = blueprint.supplies[inputItemIndex];
					BlueprintInputItemStatus inputItemStatus = status.inputItems[inputItemIndex];
					if (inputItemStatus.isMissingRequiredAmount)
					{
						ItemAsset itemAsset = inputItemConfig.FindItemAsset();
						if (itemAsset != null)
						{
							craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
							craftTooltipBuilder.AppendFormat(localization.format("NotCraftable_MissingInputItem"),
								itemAsset.itemName);
							craftTooltipBuilder.AppendLine();
						}
					}
				}
			}

			if (status.isMissingRequiredSkill)
			{
				craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
				craftTooltipBuilder.AppendLine(localization.format("NotCraftable_MissingSkill"));
			}

			if (status.missingCraftingTagsCount > 0)
			{
				CachingAssetRef[] requiredTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
				if (requiredTags != null)
				{
					for (int tagIndex = 0; tagIndex < requiredTags.Length; ++tagIndex)
					{
						ref CachingAssetRef tagRef = ref requiredTags[tagIndex];
						TagAsset tag = tagRef.Get<TagAsset>();
						if (tag == null)
							continue;

						if (!Player.LocalPlayer.crafting.IsCraftingTagAvailable(tag))
						{
							craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
							craftTooltipBuilder.AppendFormat(localization.format("NotCraftable_MissingCraftingTag"),
								tag.PlainTextName);
							craftTooltipBuilder.AppendLine();
						}
					}
				}
			}

			if (status.isMissingAnyNpcConditions)
			{
				craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
				craftTooltipBuilder.AppendLine(localization.format("NotCraftable_UnmetConditions"));
			}

			if (preferences == EBlueprintPreferences.Ignored)
			{
				craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
				craftTooltipBuilder.AppendLine(localization.format("NotCraftable_Ignored"));
			}
		}

		private static bool DoesBlueprintMatchFilterText(Blueprint blueprint)
		{
			string text = itemTextFilter;

			for (byte outputIndex = 0; outputIndex < blueprint.outputs.Length; outputIndex++)
			{
				BlueprintOutput output = blueprint.outputs[outputIndex];

				ItemAsset productAsset = output.FindItemAsset();
				if (productAsset != null)
				{
					if (productAsset.itemName != null)
					{
						if (productAsset.itemName.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) != -1)
						{
							return true;
						}
					}

					if (productAsset is ItemPlaceableAsset placeableAsset)
					{
						if (placeableAsset.DoesAnyPlaceableProvidedCraftingTagNameContainText(text))
						{
							return true;
						}
					}
				}
			}

			for (byte supplyIndex = 0; supplyIndex < blueprint.supplies.Length; supplyIndex++)
			{
				BlueprintSupply supply = blueprint.supplies[supplyIndex];

				ItemAsset supplyAsset = supply.FindItemAsset();
				if (supplyAsset != null && supplyAsset.itemName != null)
				{
					if (supplyAsset.itemName.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) != -1)
					{
						return true;
					}
				}
			}

			ItemAsset targetAsset = blueprint.TargetItem?.FindItemAsset();
			if (targetAsset != null && !string.IsNullOrEmpty(targetAsset.itemName))
			{
				if (targetAsset.itemName.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) != -1)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if all filtered blueprints are craftable. (hacked-in for item action menu)
		/// </summary>
		public static bool UpdateFilteredBlueprintsAndGetAreAllCraftable()
		{
			RefreshBlueprintList();
			foreach (BlueprintStatus status in updatedBlueprints)
			{
				if (!status.IsCraftable)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// If asset mapping has changed, find all assets with blueprints and gather the ones that can ever be crafted
		/// on this level. (I.e., excluding ones that we shouldn't waste time considering.)
		/// </summary>
		private static void RefreshLoadedBlueprintsIfNecessary()
		{
			if (Assets.HasCurrentAssetMappingChanged(ref assetListChangeCounter))
			{
				loadedBlueprints.Clear();

				HashSet<TagAsset> allCategoryTags = new HashSet<TagAsset>();

				blueprintOwners.Clear();
				Assets.find(blueprintOwners);
				PlayerCrafting crafting = Player.LocalPlayer.crafting;
				foreach (IBlueprintOwner blueprintOwner in blueprintOwners)
				{
					foreach (Blueprint blueprint in blueprintOwner.GetBlueprints())
					{
						if (!crafting.IsBlueprintPermanentlyDisabled(blueprint))
						{
							loadedBlueprints.Add(blueprint);

							TagAsset tag = blueprint.GetCategoryTag();
							if (tag != null)
							{
								allCategoryTags.Add(tag);
							}
						}
					}
				}

				RefreshCategoryTagButtons(allCategoryTags);
			}
		}

		/// <summary>
		/// Accessible for UseableHousingPlanner.
		/// </summary>
		internal static List<IBlueprintOwner> GetBlueprintOwners()
		{
			RefreshLoadedBlueprintsIfNecessary();
			return blueprintOwners;
		}

		private static HashSet<ItemAsset> availableItemAssets = new HashSet<ItemAsset>();
		private static void RefreshCraftableBlueprints()
		{
			refreshCraftableBlueprintsSampler.Begin();

			availableItemAssets.Clear();
			Player.LocalPlayer.crafting.GatherUniqueInputItems(availableItemAssets);

			foreach (Blueprint blueprint in loadedBlueprints)
			{
				if (blueprint.ContainsAnyOfItems(availableItemAssets))
				{
					filteredBlueprints.Add(blueprint);
				}
			}

			// Using early-exit update mode (stop processing as soon as anything doesn't match), find all blueprints that
			// are fully craftable by the player at this moment.
			// 
			// Nelson 2025-04-18: I'm afraid this might not perform well enough to actually use. In general, for every
			// blueprint that exists, it loops through all of the players' items looking for at least the first input
			// ingredient. On my pretty good PC with a full inventory it takes ~6.5 ms.
			/*
			blueprintStatusPool.AddRange(craftableBlueprints);
			craftableBlueprints.Clear();
			BlueprintStatus pendingCraftableBlueprint = CreateBlueprintStatus();
			foreach (Blueprint blueprint in loadedBlueprints)
			{
				if (!blueprint.ContainsAnyOfItems(availableItemAssets))
				{
					continue;
				}

				pendingCraftableBlueprint.blueprint = blueprint;
				UpdateBlueprintStatusParameters updateParameters = new UpdateBlueprintStatusParameters()
				{
					status = pendingCraftableBlueprint,
					shouldExitEarly = true,
				};
				Player.player.crafting.UpdateBlueprintStaticStatus(in updateParameters);
				if (pendingCraftableBlueprint.IsCraftable)
				{
					Player.player.crafting.UpdateBlueprintDynamicStatus(in updateParameters);
					if (pendingCraftableBlueprint.IsCraftable)
					{
						craftableBlueprints.Add(pendingCraftableBlueprint);
						pendingCraftableBlueprint = CreateBlueprintStatus();
						continue;
					}
				}
				pendingCraftableBlueprint.Reset();
			}
			blueprintStatusPool.Add(pendingCraftableBlueprint);
			//UnturnedLog.info($"Craftable blueprints: {craftableBlueprints.Count}");
			*/

			refreshCraftableBlueprintsSampler.End();
		}

		private static void RefreshCategoryTagButtons(HashSet<TagAsset> allCategoryTags)
		{
			List<TagAsset> sortedCategoryTags = allCategoryTags.ToList();
			sortedCategoryTags.Sort(CompareCategoryTags);

			categoriesContainer.IsVisible = sortedCategoryTags.Count > 0;
			if (!categoriesContainer.IsVisible)
			{
				return;
			}

			int tagIndex = 0;
			while (tagIndex < sortedCategoryTags.Count)
			{
				SleekTagButton categoryButton;
				if (tagIndex < categoryTagButtons.Count)
				{
					categoryButton = categoryTagButtons[tagIndex];
					categoryButton.IsVisible = true;
				}
				else
				{
					categoryButton = new SleekTagButton();
					categoryButton.SizeOffset_X = 50;
					categoryButton.SizeOffset_Y = 50;
					categoryButton.OnClicked += OnClickedCategoryFilterButton;
					categoryButton.TooltipAppendedText = "\n\n" + localization.format("CombineFiltersTooltip",
						MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.modify));
					categoriesContainer.AddChild(categoryButton);
					categoryTagButtons.Add(categoryButton);
				}
				categoryButton.TagRef = sortedCategoryTags[tagIndex];
				categoryButton.PositionScale_X = 0.5f;
				categoryButton.PositionOffset_X = -100 + (tagIndex % 4) * 50;
				categoryButton.PositionOffset_Y = 40 + (tagIndex / 4) * 50;
				++tagIndex;
			}

			categoriesContainer.SizeOffset_Y = 40 + MathfEx.GetPageCount(sortedCategoryTags.Count, 4) * 50;

			// Hide any extra buttons.
			while (tagIndex < categoryTagButtons.Count)
			{
				SleekTagButton categoryButton = categoryTagButtons[tagIndex];
				categoryButton.IsVisible = false;
			}
		}

		private static void RefreshTagProviderButtons()
		{
#if !DEDICATED_SERVER
			tagProvidersContainer.IsVisible = PlayerCrafting.localPlayerNearbyTagProviders.Count > 0;
			if (!tagProvidersContainer.IsVisible)
			{
				return;
			}

			int buttonIndex = 0;
			foreach (NearbyCraftingTagProvider tagProvider in PlayerCrafting.localPlayerNearbyTagProviders)
			{
				SleekCraftingTagProviderButton button;
				if (buttonIndex < tagProviderButtons.Count)
				{
					button = tagProviderButtons[buttonIndex];
					button.IsVisible = true;
				}
				else
				{
					button = new SleekCraftingTagProviderButton();
					button.SizeScale_X = 1.0f;
					button.SizeOffset_Y = 50;
					button.OnClicked += OnClickedNearbyTagProviderButton;
					tagProvidersContainer.AddChild(button);
					tagProviderButtons.Add(button);
				}
				button.SetTagProvider(tagProvider);
				button.PositionOffset_Y = 40 + buttonIndex * 50;
				++buttonIndex;
			}
			tagProvidersContainer.SizeOffset_Y = 40 + buttonIndex * 50;

			while (buttonIndex < tagProviderButtons.Count)
			{
				SleekCraftingTagProviderButton button = tagProviderButtons[buttonIndex];
				button.IsVisible = false;
				++buttonIndex;
			}
#endif // !DEDICATED_SERVER
		}

#if ENABLE_AVAILABLE_TAGS_DISPLAY
		private static void RefreshAvailableTagButtons()
		{
#if !DEDICATED_SERVER
			availableTagsContainer.IsVisible = PlayerCrafting.localPlayerNearbyTags.Count > 0;
			if (!availableTagsContainer.IsVisible)
			{
				return;
			}

			int buttonIndex = 0;
			foreach (TagAsset tag in PlayerCrafting.localPlayerNearbyTags)
			{
				SleekTagButton button;
				if (buttonIndex < availableTagButtons.Count)
				{
					button = availableTagButtons[buttonIndex];
					button.IsVisible = true;
				}
				else
				{
					button = new SleekTagButton();
					button.EnableLabel = true;
					button.SizeScale_X = 1.0f;
					button.SizeOffset_Y = 50;
					button.OnClicked += OnClickedNearbyTagButton;
					availableTagsContainer.AddChild(button);
					availableTagButtons.Add(button);
				}
				button.TagRef = tag;
				button.PositionOffset_Y = 40 + buttonIndex * 50;
				++buttonIndex;
			}
			availableTagsContainer.SizeOffset_Y = 40 + buttonIndex * 50;

			while (buttonIndex < availableTagButtons.Count)
			{
				SleekTagButton button = availableTagButtons[buttonIndex];
				button.IsVisible = false;
				++buttonIndex;
			}
#endif // !DEDICATED_SERVER
		}
#endif // ENABLE_AVAILABLE_TAGS_DISPLAY

		private static void OrganizeFiltersColumn()
		{
			float offset = 100;
			if (showIgnoredToggle.IsVisible)
			{
				showIgnoredToggle.PositionOffset_Y = offset;
				offset += showIgnoredToggle.SizeOffset_Y;
			}

			if (favoritesButton.IsVisible)
			{
				favoritesButton.PositionOffset_Y = offset;
				offset += favoritesButton.SizeOffset_Y;
				offset += 10;
			}

			if (categoriesContainer.IsVisible)
			{
				categoriesContainer.PositionOffset_Y = offset;
				offset += categoriesContainer.SizeOffset_Y;
				offset += 10;
			}

			if (tagProvidersContainer.IsVisible)
			{
				tagProvidersContainer.PositionOffset_Y = offset;
				offset += tagProvidersContainer.SizeOffset_Y;
				offset += 10;
			}

#if ENABLE_AVAILABLE_TAGS_DISPLAY
			if (availableTagsContainer.IsVisible)
			{
				availableTagsContainer.PositionOffset_Y = offset;
				offset += availableTagsContainer.SizeOffset_Y;
				offset += 10;
			}
#endif // ENABLE_AVAILABLE_TAGS_DISPLAY

			filtersScrollView.ContentSizeOffset = new Vector2(0.0f, offset - 10);
		}

		private static System.Text.StringBuilder filteringDescriptionSb = new System.Text.StringBuilder();
		private static System.Text.StringBuilder filteringDescriptionCategoriesSb = new System.Text.StringBuilder();

		private static void RefreshBlueprintList()
		{
			Profiler.BeginSample("RefreshBlueprintList");

			Profiler.BeginSample("UpdateAvailableCraftingTags");
			Player.LocalPlayer.crafting.UpdateAvailableCraftingTags();
			Profiler.EndSample();

			Profiler.BeginSample("RefreshTagProviderButtons");
			RefreshTagProviderButtons();
			Profiler.EndSample();

#if ENABLE_AVAILABLE_TAGS_DISPLAY
			Profiler.BeginSample("RefreshAvailableTagButtons");
			RefreshAvailableTagButtons();
			Profiler.EndSample();
#endif

			Profiler.BeginSample("Update Filters");
			bool isUsingItemTextFilter = !string.IsNullOrEmpty(itemTextFilter);
			bool isUsingAnyFilter = false;

			filteringDescriptionSb.Clear();

			if (filterFavorites)
			{
				isUsingAnyFilter = true;

				if (filteringDescriptionSb.Length > 0)
				{
					filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
				}

				string format = localization.format("FilteringDescription_Favorites_Format");
				string filterText = $"<color={Palette.hex(OptionsSettings.fontColor)}>{localization.format("FilteringDescription_Favorites_Label")}</color>";
				filteringDescriptionSb.AppendFormat(format, filterText);
			}

			if (filterAnyOfCategories != null && filterAnyOfCategories.Count > 0)
			{
				isUsingAnyFilter = true;

				if (filteringDescriptionSb.Length > 0)
				{
					filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
				}

				if (filterAnyOfCategories.Count == 1)
				{
					TagAsset categoryTag = filterAnyOfCategories.First();
					string format = localization.format("FilteringDescription_Category");
					filteringDescriptionSb.AppendFormat(format, categoryTag?.RichTextOrPreferredFontColor);
				}
				else
				{
					filteringDescriptionCategoriesSb.Clear();

					foreach (TagAsset categoryTag in filterAnyOfCategories)
					{
						if (filteringDescriptionCategoriesSb.Length > 0)
						{
							filteringDescriptionCategoriesSb.Append(localization.format("FilteringDescription_Category_Separator"));
						}

						filteringDescriptionCategoriesSb.Append(categoryTag?.RichTextOrPreferredFontColor);
					}

					string format = localization.format("FilteringDescription_Category_Multiple");
					filteringDescriptionSb.AppendFormat(format, filteringDescriptionCategoriesSb);
				}
			}

			if (filterTagProvider != null)
			{
				isUsingAnyFilter = true;

				if (filteringDescriptionSb.Length > 0)
				{
					filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
				}

				string format = localization.format("FilteringDescription_TagProvider");
				string assetText;
				Asset filterTagProviderAsset = filterTagProvider.GetTagProviderAsset();
				if (filterTagProviderAsset != null)
				{
					if (filterTagProviderAsset is ItemAsset itemAsset)
					{
						assetText = itemAsset.RarityRichTextName;
					}
					else
					{
						assetText = filterTagProviderAsset.FriendlyName;
					}
				}
				else
				{
					assetText = filterTagProvider.ToString(); // Shouldn't happen. :S
				}
				filteringDescriptionSb.AppendFormat(format, assetText);
			}
			else if (filterRequiresAnyOfTags.Count == 1)
			{
				isUsingAnyFilter = true;

				if (filteringDescriptionSb.Length > 0)
				{
					filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
				}

				string format = localization.format("FilteringDescription_Tag");
				TagAsset tag = filterRequiresAnyOfTags.First();
				filteringDescriptionSb.AppendFormat(format, tag.RichTextOrPreferredFontColor);
			}

			if (isUsingItemTextFilter)
			{
				isUsingAnyFilter = true;

				if (filteringDescriptionSb.Length > 0)
				{
					filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
				}

				string format = localization.format("FilteringDescription_Name");
				string filterText = $"<color={Palette.hex(OptionsSettings.fontColor)}>{itemTextFilter}</color>";
				filteringDescriptionSb.AppendFormat(format, filterText);
			}

			filteringDescriptionButton.IsVisible = isUsingAnyFilter;
			if (isUsingAnyFilter)
			{
				filteringDescriptionButton.text = localization.format("FilteringDescription_Format", filteringDescriptionSb);
				blueprintsScrollBox.PositionOffset_Y = filteringDescriptionButton.SizeOffset_Y;
			}
			else
			{
				blueprintsScrollBox.PositionOffset_Y = 0;
			}
			blueprintsScrollBox.SizeOffset_Y = -blueprintsScrollBox.PositionOffset_Y;
			Profiler.EndSample(); // Update Filters

			Profiler.BeginSample("Gather FilteredBlueprints");
			filteredBlueprints.Clear();
			if (Level.IsCraftingAllowedByLevel)
			{
				// Always refresh loaded blueprints at least to populate category buttons.
				RefreshLoadedBlueprintsIfNecessary();

				if (filteredBlueprintsOverride == null)
				{
					if (isUsingAnyFilter)
					{
						foreach (Blueprint blueprint in loadedBlueprints)
						{
#if !DEDICATED_SERVER
							if (filterFavorites && PlayerCrafting.GetBlueprintPreferences(blueprint) != EBlueprintPreferences.Favorited)
							{
								continue;
							}
#endif // !DEDICATED_SERVER

							if (filterAnyOfCategories.Count > 0)
							{
								TagAsset categoryTag = blueprint.GetCategoryTag();
								if (categoryTag == null || !filterAnyOfCategories.Contains(categoryTag))
								{
									continue;
								}
							}

							if (filterRequiresAnyOfTags.Count > 0)
							{
								bool hasAnyTag = false;
								foreach (TagAsset tag in filterRequiresAnyOfTags)
								{
									if (blueprint.DoesRequireNearbyCraftingTag(tag))
									{
										hasAnyTag = true;
										break;
									}
								}

								if (!hasAnyTag)
								{
									continue;
								}
							}

							if (isUsingItemTextFilter)
							{
								if (!DoesBlueprintMatchFilterText(blueprint))
								{
									continue;
								}
							}

							filteredBlueprints.Add(blueprint);
						}
					}
					else
					{
						RefreshCraftableBlueprints();
					}
				}
				else
				{
					filteredBlueprints.AddRange(filteredBlueprintsOverride);
				}
			}
			Profiler.EndSample(); // Gather FilteredBlueprints

			// After RefreshLoadedBlueprintsIfNecessary has potentially changed categories list, too.
			OrganizeFiltersColumn();

			blueprintStatusPool.AddRange(updatedBlueprints);
			updatedBlueprints.Clear();
			visibleBlueprints.Clear();

			Blueprint currentSelectedBlueprint = selectedBlueprintMenu.SelectedBlueprint;
			BlueprintStatus newSelectedBlueprintStatus = null;

			bool isFilterVerySpecific = isUsingItemTextFilter || filterFavorites;

			foreach (Blueprint blueprint in filteredBlueprints)
			{
#if !DEDICATED_SERVER
				if (!showIgnored)
				{
					bool isIgnored = PlayerCrafting.GetBlueprintPreferences(blueprint) == EBlueprintPreferences.Ignored;
					if (isIgnored)
					{
						continue;
					}
				}
#endif // !DEDICATED_SERVER

				BlueprintStatus blueprintStatus = CreateBlueprintStatus();
				blueprintStatus.blueprint = blueprint;
				updatedBlueprints.Add(blueprintStatus);

				UpdateBlueprintStatusParameters p = new UpdateBlueprintStatusParameters()
				{
					status = blueprintStatus,
					shouldExitEarly = false,
				};
				Profiler.BeginSample("Update Blueprint Status");
				Player.LocalPlayer.crafting.UpdateBlueprintStaticStatus(in p, /*bypassWorkstationRequirements*/ false);
				Player.LocalPlayer.crafting.UpdateBlueprintDynamicStatus(in p);
				Profiler.EndSample(); // Update Blueprint Status

				if (hideUncraftable && !blueprintStatus.IsCraftable)
				{
					continue;
				}

				if (blueprintStatus.isMissingAnyCriticalInputItem || !blueprintStatus.hasAnyInputItem)
				{
					// Uh oh! Blueprint DOES have items marked "critical" and they aren't available,
					// OR player doesn't have ANY of the input items.
					if (!blueprint.canBeVisibleWhenSearchedWithoutRequiredItems || !isFilterVerySpecific)
					{
						continue;
					}
				}

				if (blueprintStatus.isMissingAnyNpcConditions && !blueprint.CanBeVisibleWithUnmetConditions)
				{
					continue;
				}

#if !DEDICATED_SERVER
				blueprintStatus.UpdateCraftabilityScore();
#endif // !DEDICATED_SERVER

				visibleBlueprints.Add(blueprintStatus);
				if (blueprint == currentSelectedBlueprint)
				{
					newSelectedBlueprintStatus = blueprintStatus;
				}
			}

#if !DEDICATED_SERVER
			Profiler.BeginSample("Sort");
			visibleBlueprints.Sort(visibleBlueprintsComparison);
			Profiler.EndSample();
#endif // !DEDICATED_SERVER

			Profiler.BeginSample("SetSelectedBlueprintStatus");
			SetSelectedBlueprintStatus(newSelectedBlueprintStatus);
			Profiler.EndSample();

			Profiler.BeginSample("ForceRebuildElements");
			blueprintsScrollBox.ForceRebuildElements();
			Profiler.EndSample();

			blueprintsListEmptyInfoBox.IsVisible = visibleBlueprints.Count == 0;
			if (blueprintsListEmptyInfoBox.IsVisible)
			{
				blueprintsListEmptyInfoBox.PositionOffset_Y = blueprintsScrollBox.PositionOffset_Y;
				resetFiltersButton.IsVisible = isUsingAnyFilter;
				if (isUsingAnyFilter)
				{
					blueprintsListEmptyInfoBox.Text = localization.format("No_Blueprints");
				}
				else
				{
					if (availableItemAssets.Count < 1)
					{
						blueprintsListEmptyInfoBox.Text = localization.format("NoBlueprints_ZeroAvailableItems");
					}
					else
					{
						blueprintsListEmptyInfoBox.Text = localization.format("NoBlueprints_HasAvailableItems");
					}
				}
			}

			Profiler.EndSample(); // RefreshBlueprintList
		}

		private static void onInventoryResized(byte page, byte newWidth, byte newHeight)
		{
			if (active)
			{
				RefreshBlueprintList();
			}
		}

		private static void onCraftingUpdated()
		{
			if (active)
			{
				RefreshBlueprintList();
			}
		}

		private static void ClearFilters()
		{
			filteredBlueprintsOverride = null;
			filterAnyOfCategories.Clear();
			filterRequiresAnyOfTags.Clear();
			filterTagProvider = null;
			searchField.Text = "";
			itemTextFilter = null;
			filterFavorites = false;
		}

		private static void OnClickedCategoryFilterButton(CachingAssetRef categoryTagRef)
		{
			filteredBlueprintsOverride = null;

			TagAsset categoryTag = categoryTagRef.Get<TagAsset>();
			if (categoryTag == null)
			{
				UnturnedLog.info("Clicked category tag is missing");
				return;
			}

			bool wasActive = filterAnyOfCategories.Remove(categoryTag);
			if (!wasActive)
			{
				if (!InputEx.GetKey(ControlsSettings.modify))
				{
					ClearFilters();
				}

				filterAnyOfCategories.Add(categoryTag);
			}
			RefreshBlueprintList();
		}

		private static void OnClickedNearbyTagProviderButton(ICraftingTagProvider tagProvider)
		{
			if (tagProvider == null)
			{
				UnturnedLog.info("Clicked nearby crafting tag provider has been destroyed");
				return;
			}

			filteredBlueprintsOverride = null;

			if (filterTagProvider == tagProvider)
			{
				filterTagProvider = null;
				filterRequiresAnyOfTags.Clear();
			}
			else
			{
				if (!InputEx.GetKey(ControlsSettings.modify))
				{
					ClearFilters();
				}

				filterTagProvider = tagProvider;
				filterRequiresAnyOfTags.Clear();
				CraftingTagProviderGetAvailableTagsParameters p = new CraftingTagProviderGetAvailableTagsParameters();
				p.ResultTags = filterRequiresAnyOfTags;
				tagProvider.GetAvailableTags(ref p);
				//UnturnedLog.info($"Filtering {tagProvider.GetTagProviderAsset()?.FriendlyName} tags: {string.Join(", ", filterRequiresAnyOfTags)}");
			}
			RefreshBlueprintList();
		}

#if ENABLE_AVAILABLE_TAGS_DISPLAY
		private static void OnClickedNearbyTagButton(CachingAssetRef tagRef)
		{
			TagAsset tag = tagRef.Get<TagAsset>();
			if (tag == null)
			{
				UnturnedLog.info("Clicked nearby crafting tag is now missing");
				return;
			}

			filteredBlueprintsOverride = null;

			if (filterTagProvider == null && filterRequiresAnyOfTags.Count == 1 && filterRequiresAnyOfTags.First() == tag)
			{
				filterRequiresAnyOfTags.Clear();
			}
			else
			{
				if (!InputEx.GetKey(ControlsSettings.modify))
				{
					ClearFilters();
				}

				filterTagProvider = null;
				filterRequiresAnyOfTags.Clear();
				filterRequiresAnyOfTags.Add(tag);
			}
			RefreshBlueprintList();
		}
#endif // ENABLE_AVAILABLE_TAGS_DISPLAY

		private static void onToggledHideUncraftableToggle(ISleekToggle toggle, bool state)
		{
			hideUncraftable = state;
			RefreshBlueprintList();
		}

		private static void OnShowIgnoredToggled(ISleekToggle toggle, bool state)
		{
			showIgnored = state;
			RefreshBlueprintList();
		}

		private static void OnClickedClearFilters(ISleekElement button)
		{
			ClearFilters();
			RefreshBlueprintList();
		}

		private static void onEnteredSearchField(ISleekField field)
		{
			filteredBlueprintsOverride = null;
			// Doesn't use InputEx because search field will block input.
			if (!Input.GetKey(ControlsSettings.modify))
			{
				filterAnyOfCategories.Clear();
				filterRequiresAnyOfTags.Clear();
				filterTagProvider = null;
				filterFavorites = false;
			}

			itemTextFilter = searchField.Text;
			RefreshBlueprintList();
		}

		private static void onClickedSearchButton(ISleekElement button)
		{
			onEnteredSearchField(searchField);
		}

		private static void OnClickedFavoritesButton(ISleekElement button)
		{
			filteredBlueprintsOverride = null;

			if (filterFavorites)
			{
				filterFavorites = false;
			}
			else
			{
				if (!InputEx.GetKey(ControlsSettings.modify))
				{
					ClearFilters();
				}

				filterFavorites = true;
			}
			RefreshBlueprintList();
		}

		private static void OnClickedBlueprint(BlueprintStatus blueprintStatus)
		{
			bool skipMenu = InputEx.GetKey(ControlsSettings.SkipActionCraftingMenu);
			bool asManyAsPossible = InputEx.GetKey(ControlsSettings.other);

			// Holding "skip menu" when click-to-craft is enabled indicates player *wants* to toggle the details panel.
			// Nelson 2025-04-25: even if Ctrl and Shift are held the input system might not report both.
			bool wantsToCraft = asManyAsPossible || (skipMenu != OptionsSettings.ShouldClickBlueprintToCraft);
			if (wantsToCraft && blueprintStatus.IsCraftable)
			{
				if (Player.LocalPlayer.equipment.isBusy)
				{
					return;
				}

				Player.LocalPlayer.crafting.SendRequestToCraft(blueprintStatus.blueprint, asManyAsPossible);
			}
			else
			{
				if (selectedBlueprintMenu.SelectedBlueprint == blueprintStatus.blueprint)
				{
					// Collapse details menu when clicking same blueprint again.
					SetSelectedBlueprintStatus(null);
				}
				else
				{
					SetSelectedBlueprintStatus(blueprintStatus);
				}
			}
		}

		private static ISleekElement onCreateBlueprint(BlueprintStatus blueprintStatus)
		{
			if (pooledBlueprintWidgets.TryPop(out SleekBlueprint blueprintButton))
			{
				blueprintButton.IsVisible = true;
			}
			else
			{
				blueprintButton = new SleekBlueprint();
				blueprintButton.OnClickedBlueprint += OnClickedBlueprint;
			}

			blueprintButton.SetBlueprintStatus(blueprintStatus);

			return blueprintButton;
		}

		private static void OnRemoveBlueprintElement(ISleekElement element)
		{
			SleekBlueprint blueprintButton = (SleekBlueprint) element;
			blueprintButton.IsVisible = false;
			pooledBlueprintWidgets.Push(blueprintButton);
		}

		/// <summary>
		/// Get a blank status from the pool or construct a new one.
		/// </summary>
		private static BlueprintStatus CreateBlueprintStatus()
		{
			BlueprintStatus blueprintStatus;
			if (blueprintStatusPool.Count > 0)
			{
				blueprintStatus = blueprintStatusPool.GetAndRemoveTail();
				blueprintStatus.Reset();
			}
			else
			{
				blueprintStatus = new BlueprintStatus();
			}
			return blueprintStatus;
		}

		private static void RefreshShowIgnoredToggleAndFavoritesButtonVisible()
		{
#if !DEDICATED_SERVER
			bool changed = false;

			bool ignoredVisible = PlayerCrafting.HasIgnoredAnyBlueprints;
			if (showIgnoredToggle.IsVisible != ignoredVisible)
			{
				changed = true;
				showIgnoredToggle.IsVisible = ignoredVisible;
			}

			bool favoritesVisible = PlayerCrafting.HasFavoritedAnyBlueprints;
			if (favoritesButton.IsVisible != favoritesVisible)
			{
				changed = true;
				favoritesButton.IsVisible = favoritesVisible;
			}

			if (changed)
			{
				OrganizeFiltersColumn();
			}
#endif // !DEDICATED_SERVER
		}

		internal void OnDestroy()
		{
#if !DEDICATED_SERVER
			PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged -= RefreshShowIgnoredToggleAndFavoritesButtonVisible;
#endif
		}

		public PlayerDashboardCraftingUI()
		{
			localization = Localization.read("/Player/PlayerDashboardCrafting.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerDashboardCrafting");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;
			filteredBlueprintsOverride = null;
			filterAnyOfCategories = new HashSet<TagAsset>();
			hideUncraftable = false;
			showIgnored = false;
			itemTextFilter = string.Empty;
			filterRequiresAnyOfTags = new HashSet<TagAsset>();
			filterTagProvider = null;
			filterFavorites = false;

			backdropBox = Glazier.Get().CreateBox();
			backdropBox.PositionOffset_Y = 60;
			backdropBox.SizeOffset_Y = -60;
			backdropBox.SizeScale_X = 1;
			backdropBox.SizeScale_Y = 1;
			backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
			container.AddChild(backdropBox);

			loadedBlueprints = new List<Blueprint>();
			assetListChangeCounter = -1;

			visibleBlueprints = new List<BlueprintStatus>();
			updatedBlueprints = new List<BlueprintStatus>();
			blueprintStatusPool = new List<BlueprintStatus>();

			pooledBlueprintWidgets = new Stack<SleekBlueprint>();

			blueprintsContainer = Glazier.Get().CreateFrame();
			blueprintsContainer.PositionOffset_X = 250;
			blueprintsContainer.PositionOffset_Y = 10;
			blueprintsContainer.SizeOffset_X = -260;
			blueprintsContainer.SizeScale_X = 1.0f;
			blueprintsContainer.SizeScale_Y = 1.0f;
			blueprintsContainer.SizeOffset_Y = -20;
			backdropBox.AddChild(blueprintsContainer);

			filteringDescriptionButton = new SleekButtonIcon(icons.load<Texture2D>("CancelFiltering"), 40);
			filteringDescriptionButton.SizeOffset_Y = 50;
			filteringDescriptionButton.SizeScale_X = 1.0f;
			filteringDescriptionButton.enableRichText = true;
			filteringDescriptionButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
			filteringDescriptionButton.fontSize = ESleekFontSize.Medium;
			filteringDescriptionButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
			filteringDescriptionButton.iconColor = ESleekTint.FOREGROUND;
			filteringDescriptionButton.onClickedButton += OnClickedClearFilters;
			filteringDescriptionButton.tooltip = $"{localization.format("ResetFilters_Label")}\n{localization.format("ResetFilters_Tooltip")}";
			blueprintsContainer.AddChild(filteringDescriptionButton);

			blueprintsScrollBox = new SleekList<BlueprintStatus>();
			blueprintsScrollBox.PositionOffset_Y = 40;
			blueprintsScrollBox.SizeScale_X = 1;
			blueprintsScrollBox.SizeScale_Y = 1;
			blueprintsScrollBox.itemHeight = 160;
			blueprintsScrollBox.onCreateElement = onCreateBlueprint;
			blueprintsScrollBox.OnRemoveElement += OnRemoveBlueprintElement;
			blueprintsScrollBox.SetData(visibleBlueprints);
			blueprintsContainer.AddChild(blueprintsScrollBox);

			filtersScrollView = Glazier.Get().CreateScrollView();
			filtersScrollView.PositionOffset_X = 10;
			filtersScrollView.PositionOffset_Y = 10;
			filtersScrollView.SizeOffset_X = 230;
			filtersScrollView.SizeOffset_Y = -20;
			filtersScrollView.SizeScale_Y = 1;
			filtersScrollView.ScaleContentToWidth = true;
			backdropBox.AddChild(filtersScrollView);

			categoriesContainer = Glazier.Get().CreateFrame();
			categoriesContainer.SizeScale_X = 1.0f;
			categoriesContainer.SizeOffset_Y = 50;
			filtersScrollView.AddChild(categoriesContainer);
			categoryTagButtons = new List<SleekTagButton>();

			categoriesHeader = Glazier.Get().CreateLabel();
			categoriesHeader.SizeScale_X = 1.0f;
			categoriesHeader.SizeOffset_Y = 40;
			categoriesHeader.Text = localization.format("Header_Categories");
			categoriesHeader.FontSize = ESleekFontSize.Medium;
			categoriesHeader.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			categoriesContainer.AddChild(categoriesHeader);

			tagProvidersContainer = Glazier.Get().CreateFrame();
			tagProvidersContainer.SizeScale_X = 1.0f;
			filtersScrollView.AddChild(tagProvidersContainer);
			tagProviderButtons = new List<SleekCraftingTagProviderButton>();

			tagProvidersHeader = Glazier.Get().CreateLabel();
			tagProvidersHeader.SizeScale_X = 1.0f;
			tagProvidersHeader.SizeOffset_Y = 40;
			tagProvidersHeader.Text = localization.format("Header_TagProviders");
			tagProvidersHeader.FontSize = ESleekFontSize.Medium;
			tagProvidersHeader.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			tagProvidersContainer.AddChild(tagProvidersHeader);

#if ENABLE_AVAILABLE_TAGS_DISPLAY
			availableTagsContainer = Glazier.Get().CreateFrame();
			availableTagsContainer.SizeScale_X = 1.0f;
			filtersScrollView.AddChild(availableTagsContainer);
			availableTagButtons = new List<SleekTagButton>();

			availableTagsHeader = Glazier.Get().CreateLabel();
			availableTagsHeader.SizeScale_X = 1.0f;
			availableTagsHeader.SizeOffset_Y = 40;
			availableTagsHeader.Text = localization.format("Header_AvailableTags");
			availableTagsHeader.FontSize = ESleekFontSize.Medium;
			availableTagsHeader.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			availableTagsContainer.AddChild(availableTagsHeader);
#endif // ENABLE_AVAILABLE_TAGS_DISPLAY

			hideUncraftableToggle = Glazier.Get().CreateToggle();
			hideUncraftableToggle.PositionOffset_Y = 60;
			hideUncraftableToggle.SizeOffset_X = 40;
			hideUncraftableToggle.SizeOffset_Y = 40;
			hideUncraftableToggle.AddLabel(localization.format("Hide_Uncraftable_Toggle_Label"), ESleekSide.RIGHT);
			hideUncraftableToggle.TooltipText = localization.format("Hide_Uncraftable_Toggle_Tooltip");
			hideUncraftableToggle.Value = hideUncraftable;
			hideUncraftableToggle.OnValueChanged += onToggledHideUncraftableToggle;
			filtersScrollView.AddChild(hideUncraftableToggle);

			showIgnoredToggle = Glazier.Get().CreateToggle();
			showIgnoredToggle.PositionOffset_Y = 100;
			showIgnoredToggle.SizeOffset_X = 40;
			showIgnoredToggle.SizeOffset_Y = 40;
			showIgnoredToggle.AddLabel(localization.format("Show_Ignored_Toggle_Label"), ESleekSide.RIGHT);
			showIgnoredToggle.TooltipText = localization.format("Show_Ignored_Toggle_Tooltip");
			showIgnoredToggle.Value = showIgnored;
			showIgnoredToggle.OnValueChanged += OnShowIgnoredToggled;
			filtersScrollView.AddChild(showIgnoredToggle);

			searchField = Glazier.Get().CreateStringField();
			searchField.SizeScale_X = 1.0f;
			searchField.SizeOffset_Y = 30;
			searchField.PlaceholderText = localization.format("Search_Field_Hint");
			searchField.OnTextSubmitted += onEnteredSearchField;
			searchField.TooltipText = localization.format("CombineFiltersTooltip",
				MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.modify));
			filtersScrollView.AddChild(searchField);

			searchButton = Glazier.Get().CreateButton();
			searchButton.PositionOffset_Y = 30;
			searchButton.SizeScale_X = 1.0f;
			searchButton.SizeOffset_Y = 30;
			searchButton.Text = localization.format("Search");
			searchButton.TooltipText = $"{localization.format("Search_Tooltip")}\n\n{searchField.TooltipText}";
			searchButton.OnClicked += onClickedSearchButton;
			filtersScrollView.AddChild(searchButton);

			favoritesButton = new SleekButtonIcon(icons.load<Texture2D>("FavoriteBlueprintIcon"), 40);
			favoritesButton.SizeScale_X = 1.0f;
			favoritesButton.SizeOffset_Y = 50;
			favoritesButton.text = localization.format("Favorites_Label");
			favoritesButton.tooltip = $"{localization.format("Favorites_Tooltip")}\n\n{searchField.TooltipText}";
			favoritesButton.onClickedButton += OnClickedFavoritesButton;
			filtersScrollView.AddChild(favoritesButton);

			RefreshShowIgnoredToggleAndFavoritesButtonVisible();

			blueprintsListEmptyInfoBox = Glazier.Get().CreateBox();
			blueprintsListEmptyInfoBox.PositionOffset_Y = 40;
			blueprintsListEmptyInfoBox.SizeOffset_Y = 50;
			blueprintsListEmptyInfoBox.SizeScale_X = 1;
			blueprintsListEmptyInfoBox.FontSize = ESleekFontSize.Medium;
			blueprintsContainer.AddChild(blueprintsListEmptyInfoBox);
			blueprintsListEmptyInfoBox.IsVisible = false;

			resetFiltersButton = Glazier.Get().CreateButton();
			resetFiltersButton.PositionOffset_X = -150;
			resetFiltersButton.PositionOffset_Y = 10;
			resetFiltersButton.PositionScale_X = 0.5f;
			resetFiltersButton.PositionScale_Y = 1.0f;
			resetFiltersButton.SizeOffset_X = 300;
			resetFiltersButton.SizeOffset_Y = 30;
			resetFiltersButton.Text = localization.format("ResetFilters_Label");
			resetFiltersButton.TooltipText = localization.format("ResetFilters_Tooltip");
			resetFiltersButton.OnClicked += OnClickedClearFilters;
			blueprintsListEmptyInfoBox.AddChild(resetFiltersButton);

			selectedBlueprintMenu = new SleekSelectedBlueprint();
			selectedBlueprintMenu.PositionOffset_X = -240;
			selectedBlueprintMenu.PositionOffset_Y = 10;
			selectedBlueprintMenu.PositionScale_X = 1.0f;
			selectedBlueprintMenu.SizeOffset_X = 230;
			selectedBlueprintMenu.SizeScale_Y = 1.0f;
			selectedBlueprintMenu.SizeOffset_Y = -20;
			selectedBlueprintMenu.IsVisible = false;
			backdropBox.AddChild(selectedBlueprintMenu);

			Player.LocalPlayer.inventory.onInventoryResized += onInventoryResized;
			Player.LocalPlayer.crafting.onCraftingUpdated += onCraftingUpdated;
#if !DEDICATED_SERVER
			PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged += RefreshShowIgnoredToggleAndFavoritesButtonVisible;
#endif
		}

		private static int CompareCategoryTags(TagAsset lhs, TagAsset rhs)
		{
			return lhs.FriendlyName.CompareTo(rhs.FriendlyName);
		}

#if !DEDICATED_SERVER
		private static string GetBlueprintStatusSortString(BlueprintStatus status)
		{
			Blueprint blueprint = status.blueprint;

			if (blueprint.TargetItem != null)
			{
				// Target item is the most important item.
				return blueprint.TargetItem.FindItemAsset()?.itemName;
			}

			if (blueprint.outputs != null && blueprint.outputs.Length == 1)
			{
				// If there's only one output item it's probably the most important item.
				// (e.g., crafting a specific item like a generator)
				return blueprint.outputs[0].FindItemAsset()?.itemName;
			}

			if (blueprint.supplies != null && blueprint.supplies.Length == 1)
			{
				// Maybe salvaging an item?
				return blueprint.supplies[0].FindItemAsset()?.itemName;
			}

			// Perhaps not ideal, but it's not clear what to sort by here.
			return blueprint.GetOwnerAsset()?.FriendlyName;
		}

		private static System.Comparison<BlueprintStatus> visibleBlueprintsComparison = CompareVisibleBlueprints;
		private static int CompareVisibleBlueprints(BlueprintStatus lhs, BlueprintStatus rhs)
		{
			if (filterRequiresAnyOfTags != null && filterRequiresAnyOfTags.Count > 1)
			{
				// When clicking a workstation that provides multiple tags we want to sort the most-matching results
				// to the front ahead of what's most-craftable.
				int lhsTagOverlaps = lhs.blueprint.CountOverlappingRequiredNearbyCraftingTags(filterRequiresAnyOfTags);
				int rhsTagOverlaps = rhs.blueprint.CountOverlappingRequiredNearbyCraftingTags(filterRequiresAnyOfTags);
				if (lhsTagOverlaps != rhsTagOverlaps)
				{
					return -lhsTagOverlaps.CompareTo(rhsTagOverlaps);
				}
			}

			int scoreComparison = -lhs.normalizedCraftability.CompareTo(rhs.normalizedCraftability);
			if (scoreComparison != 0)
			{
				return scoreComparison;
			}

			// This is to provide some stable ordering. It's OK if it's somewhat arbitrary, as the original ordering was
			// determined by asset load order. (Outside of inserting fully craftable blueprints to the front.)
			string lhsText = GetBlueprintStatusSortString(lhs);
			string rhsText = GetBlueprintStatusSortString(rhs);
			if (string.IsNullOrEmpty(lhsText) == string.IsNullOrEmpty(rhsText))
			{
				if (lhsText != null)
				{
					// Neither text is null!
					return lhsText.CompareTo(rhsText);
				}
			}
			else
			{
				if (lhsText != null)
				{
					// lhs is set, rhs isn't, so rhs might be misconfigured and go later in the list.
					return -1;
				}
				else
				{
					// rhs is set, lhs isn't, so lhs might be misconfigured and go later in the list.
					return 1;
				}
			}

			return 0;
		}
#endif // !DEDICATED_SERVER

		private static CustomSampler refreshCraftableBlueprintsSampler = CustomSampler.Create("RefreshCraftableBlueprints");
	}
}
