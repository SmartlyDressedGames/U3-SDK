////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SDG.Unturned
{
	public class EconCraftOption
	{
		public string token;
		public int generate;
		public ushort scrapsNeeded;

		public EconCraftOption(string newToken, int newGenerate, ushort newScrapsNeeded)
		{
			token = newToken;
			generate = newGenerate;
			scrapsNeeded = newScrapsNeeded;
		}
	}

	public enum EEconFilterMode
	{
		SEARCH, // Default

		/// <summary>
		/// Find an item to apply stat tracker tool to.
		/// </summary>
		STAT_TRACKER,

		/// <summary>
		/// Find an item with a stat tracker to remove.
		/// </summary>
		STAT_TRACKER_REMOVAL,

		/// <summary>
		/// Find an item with a ragdoll effect to remove.
		/// </summary>
		RAGDOLL_EFFECT_REMOVAL,

		/// <summary>
		/// Find an item to apply ragdoll effect tool to.
		/// </summary>
		RAGDOLL_EFFECT,
	}

	public class MenuSurvivorsClothingUI
	{
		public static Local localization;
		public static IconsBundle icons;
		private static SleekFullscreenBox container;
		public static bool active;

		public static bool isCrafting;

		private static SleekButtonIcon backButton;
		private static SleekButtonIcon itemstoreButton;
		private static ISleekLabel itemstoreNewLabel;
		private static SleekButtonIcon craftingButton;
		private static ISleekLabel craftingNewLabel;

		private static List<SteamItemDetails_t> filteredItems;

		private static ISleekConstraintFrame inventory;
		private static ISleekConstraintFrame crafting;
		private static SleekInventory[] packageButtons;
		private static ISleekBox availableBox;
		private static ISleekScrollView craftingScrollBox;
		private static ISleekButton[] craftingButtons;

		private static ISleekBox pageBox;
		private static ISleekBox infoBox;
		private static ISleekField searchField;
		private static ISleekButton searchButton;
		private static ISleekBox filterBox;
		private static ISleekButton cancelFilterButton;
		private static SleekButtonIcon leftButton;
		private static SleekButtonIcon rightButton;
		//private static SleekButtonIcon swapButton;
		private static SleekButtonIcon refreshButton;

		/// <summary>
		/// Toggle button to open/close advanced filters panel.
		/// </summary>
		private static SleekButtonIcon optionsButton;

		/// <summary>
		/// On/off checkbox for including description text in filter.
		/// </summary>
		private static ISleekToggle searchDescriptionsToggle;

		/// <summary>
		/// Switch between sort modes.
		/// </summary>
		private static SleekButtonState sortModeButton;

		/// <summary>
		/// On/off checkbox to reverse sort results.
		/// </summary>
		private static ISleekToggle reverseSortOrderToggle;

		/// <summary>
		/// On/off checkbox to show only equipped items.
		/// </summary>
		private static ISleekToggle filterEquippedToggle;

		/// <summary>
		/// Container for advanced options.
		/// </summary>
		private static ISleekElement optionsPanel;

		private static ISleekSlider characterSlider;

#if WITH_GRANTPACKAGE_PROMO
		private static ISleekButton grantPackagePromoButton;
#endif // WITH_GRANTPACKAGE_PROMO

		private static int pageIndex;
		private static EEconFilterMode filterMode;
		private static ulong filterInstigator;

		/// <summary>
		/// Whether to include description text in filter.
		/// </summary>
		private static bool searchDescriptions = false;

		private enum ESortMode
		{
			Date,
			Rarity,
			Name,
			Type, // Disabled for now because it is not very useful.
		}

		/// <summary>
		/// How to sort filtered items.
		/// </summary>
		private static ESortMode sortMode = ESortMode.Date;

		/// <summary>
		/// Should sorted list be reversed?
		/// </summary>
		private static bool reverseSortOrder = false;

		/// <summary>
		/// Should only equipped items be shown?
		/// </summary>
		private static bool filterEquipped = false;

		private static int numberOfPages => MathfEx.GetPageCount(filteredItems.Count, 25);

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			Characters.RefreshPreviewCharacterModel();
			MenuSurvivorsUI.clothingUI.RefreshCraftingOptions();

			characterSlider.Value = Characters.characterYaw / 360.0f;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			Characters.RefreshPreviewCharacterModel();
		
			container.AnimateOutOfView(0, 1);
		}

		public static void setFilter(EEconFilterMode newFilterMode, ulong newFilterInstigator)
		{
			setCrafting(false);

			filterMode = newFilterMode;
			filterInstigator = newFilterInstigator;

			filterBox.IsVisible = filterMode != EEconFilterMode.SEARCH;
			cancelFilterButton.IsVisible = filterMode != EEconFilterMode.SEARCH;

			if (filterMode != EEconFilterMode.SEARCH)
			{
				// Reset our search if we changed from search mode,
				// for example we searched for a kill counter item and now need to search again.
				searchField.Text = string.Empty;
			}

			if (filterMode == EEconFilterMode.STAT_TRACKER || filterMode == EEconFilterMode.STAT_TRACKER_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT)
			{
				int item = Provider.provider.economyService.getInventoryItem(filterInstigator);
				string name = Provider.provider.economyService.getInventoryName(item);
				Color color = Provider.provider.economyService.getInventoryColor(item);
				string display = "<color=" + Palette.hex(color) + ">" + name + "</color>";

				filterBox.Text = localization.format("Filter_Item_Target", display);
			}

			updateFilterAndPage();
		}

		private static void updateFilterAndPage()
		{
			updateFilter();

			if (pageIndex >= numberOfPages)
			{
				pageIndex = numberOfPages - 1;
			}

			updatePage();
		}

		public static void viewPage(int newPage)
		{
			pageIndex = newPage;

			updatePage();
		}

		private static void onClickedInventory(SleekInventory button)
		{
			int offset = packageButtons.Length * pageIndex;
			int index = inventory.FindIndexOfChild(button);

			if (offset + index < filteredItems.Count)
			{
				int itemdef = button.item;
				ulong instance = button.instance;
				ushort quantity = button.quantity;

				if (filterMode == EEconFilterMode.STAT_TRACKER || filterMode == EEconFilterMode.STAT_TRACKER_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT)
				{
					bool isAdding = filterMode == EEconFilterMode.STAT_TRACKER || filterMode == EEconFilterMode.RAGDOLL_EFFECT;
					MenuSurvivorsClothingDeleteUI.viewItem(itemdef, instance, 1, isAdding ? EDeleteMode.TAG_TOOL_ADD : EDeleteMode.TAG_TOOL_REMOVE, filterInstigator);
					MenuSurvivorsClothingDeleteUI.open();

					setFilter(EEconFilterMode.SEARCH, 0);
					close();
				}
				else if (Provider.preferenceData.Allow_Ctrl_Shift_Alt_Salvage && InputEx.GetKey(KeyCode.LeftControl) && InputEx.GetKey(KeyCode.LeftShift) && InputEx.GetKey(KeyCode.LeftAlt))
				{
					// Nelson 2025-01-16: It's a little bit too easy to accidentally salvage a valuable item.
					// Restoring items with per-item tags and properties is also quite messy. Require going through
					// the deletion UI if trying to delete special items.

					Provider.provider.economyService.getInventoryStatTrackerValue(instance, out EStatTrackerType killCounter, out int kills);
					Provider.provider.economyService.getInventoryRagdollEffect(instance, out ERagdollEffect ragdollEffect);
					ushort mythicID = Provider.provider.economyService.getInventoryMythicID(itemdef);
					if (mythicID == 0)
					{
						mythicID = Provider.provider.economyService.getInventoryParticleEffect(instance);
					}

					if (killCounter == EStatTrackerType.NONE
						&& ragdollEffect == ERagdollEffect.None
						&& mythicID == 0)
					{
						MenuSurvivorsClothingDeleteUI.salvageItem(itemdef, instance);
					}
				}
				else if (InputEx.GetKey(ControlsSettings.other) && packageButtons[index].itemAsset != null)
				{
					if (button.itemAsset.type == EItemType.BOX)
					{
						MenuSurvivorsClothingItemUI.viewItem(itemdef, quantity, instance);
						MenuSurvivorsClothingBoxUI.viewItem(itemdef, quantity, instance);
						MenuSurvivorsClothingBoxUI.open();

						close();
					}
					else
					{
						Characters.ToggleEquipItemByInstanceId(instance);
					}
				}
				else
				{
					MenuSurvivorsClothingItemUI.viewItem(itemdef, quantity, instance);
					MenuSurvivorsClothingItemUI.open();

					close();
				}
			}
		}

		private static void onEnteredSearchField(ISleekField field)
		{
			updateFilterAndPage();
		}

		private static void onClickedSearchButton(ISleekElement button)
		{
			updateFilterAndPage();
		}

		private static void onClickedCancelFilterButton(ISleekElement button)
		{
			setFilter(EEconFilterMode.SEARCH, 0);
		}

		private static void onClickedLeftButton(ISleekElement button)
		{
			if (pageIndex > 0)
			{
				viewPage(pageIndex - 1);
			}
			else if (numberOfPages > 0)
			{
				// Loop around.
				viewPage(numberOfPages - 1);
			}
		}

		private static void onClickedRightButton(ISleekElement button)
		{
			if (pageIndex < numberOfPages - 1)
			{
				viewPage(pageIndex + 1);
			}
			else if (numberOfPages > 0)
			{
				// Loop around.
				viewPage(0);
			}
		}

		//private static void onClickedSwapButton(SleekButton button)
		//{
		//	Characters.clothes.isVisual = !Characters.clothes.isVisual;
		//	Characters.apply();
		//}

		private static void onClickedOptionsButton(ISleekElement button)
		{
			optionsPanel.IsVisible = !optionsPanel.IsVisible;
			optionsButton.icon = icons.load<Texture2D>(optionsPanel.IsVisible ? "Right" : "Left");
		}

		private static void onToggledSearchDescriptions(ISleekToggle toggle, bool state)
		{
			// updateFilterAndPage cannot use checkbox directly as event is triggered before state changes.
			searchDescriptions = state;
			updateFilterAndPage();
		}

		private static void onChangedSortMode(SleekButtonState button, int state)
		{
			sortMode = (ESortMode) state;
			updateFilterAndPage();
		}

		private static void onToggledReverseSortOrder(ISleekToggle toggle, bool state)
		{
			// updateFilterAndPage cannot use checkbox directly as event is triggered before state changes.
			reverseSortOrder = state;
			updateFilterAndPage();
		}

		private static void onToggledFilterEquipped(ISleekToggle toggle, bool state)
		{
			// updateFilterAndPage cannot use checkbox directly as event is triggered before state changes.
			filterEquipped = state;
			updateFilterAndPage();
		}

		private static void onClickedRefreshButton(ISleekElement button)
		{
			Provider.provider.economyService.refreshInventory();
		}

#if WITH_GRANTPACKAGE_PROMO
		private static void onClickedGrantPackagePromoButton(ISleekElement button)
		{
			button.IsVisible = false;
			GrantPackagePromo.SendRequest();
		}
#endif // WITH_GRANTPACKAGE_PROMO

		public static void prepareForCraftResult()
		{
			isCrafting = true;
			MenuUI.openAlert(localization.format("Alert_Crafting"), canBeDismissed: false);
		}

		private void onClickedCraftButton(ISleekElement button)
		{
			if (isCrafting)
			{
				return;
			}

			int index = craftingScrollBox.FindIndexOfChild(button);
			if (index == -1)
			{
				return;
			}

			EconCraftOption option = econCraftOptions[index];

			List<EconExchangePair> materials;
			if (!Provider.provider.economyService.getInventoryPackages(19000, option.scrapsNeeded, out materials))
			{
				return;
			}

			prepareForCraftResult();
			Provider.provider.economyService.exchangeInventory(option.generate, materials);
		}

		private static void onInventoryRefreshed()
		{
			infoBox.IsVisible = false;

			updateFilter();

			if (pageIndex >= numberOfPages)
			{
				pageIndex = numberOfPages - 1;
			}

			updatePage();

#if WITH_GRANTPACKAGE_PROMO
			grantPackagePromoButton.IsVisible = GrantPackagePromo.IsEligible();
#endif // WITH_GRANTPACKAGE_PROMO
		}

		// temp public for a quick test
		public static void onInventoryDropped(int item, ushort quantity, ulong instance)
		{
			MenuUI.closeAll();

			MenuUI.alert(localization.format("Origin_Drop"), instance, item, quantity);

			MenuSurvivorsClothingItemUI.viewItem(item, quantity, instance);
			MenuSurvivorsClothingItemUI.open();
		}

		private static void onCharacterUpdated(byte index, Character character)
		{
			updatePage();
		}

		private static void OnPricesReceived()
		{
			if (ItemStore.Get().HasNewListings && !ItemStoreSavedata.WasNewListingsPageSeen())
			{
				itemstoreNewLabel = Glazier.Get().CreateLabel();
				itemstoreNewLabel.SizeScale_X = 1.0f;
				itemstoreNewLabel.SizeScale_Y = 1.0f;
				itemstoreNewLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				itemstoreNewLabel.TextAlignment = TextAnchor.UpperRight;
				itemstoreNewLabel.TextColor = Color.green;
				itemstoreNewLabel.Text = Provider.localization.format("New");
				itemstoreButton.AddChild(itemstoreNewLabel);
			}
			else if (ItemStore.Get().HasDiscountedListings)
			{
				ISleekLabel saleLabel = Glazier.Get().CreateLabel();
				saleLabel.SizeScale_X = 1.0f;
				saleLabel.SizeScale_Y = 1.0f;
				saleLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				saleLabel.TextAlignment = TextAnchor.UpperRight;
				saleLabel.TextColor = Color.green;
				saleLabel.Text = localization.format("Itemstore_Sale");
				itemstoreButton.AddChild(saleLabel);
			}
		}

		/// <summary>
		/// Remove items that do not match search text.
		/// </summary>
		private static void applySearchTextFilter()
		{
			string search = searchField.Text;
			if (string.IsNullOrEmpty(search))
				return;

			TokenSearchFilter? tokenFilter = TokenSearchFilter.parse(search);
			if (tokenFilter.HasValue == false)
				return;

			for (int index = filteredItems.Count - 1; index >= 0; --index)
			{
				SteamItemDetails_t details = filteredItems[index];
				bool match = false;

				string name = Provider.provider.economyService.getInventoryName(details.m_iDefinition.m_SteamItemDef);
				if (tokenFilter.Value.matches(name))
				{
					match = true;
				}
				else
				{
					string type = Provider.provider.economyService.getInventoryType(details.m_iDefinition.m_SteamItemDef);
					if (tokenFilter.Value.matches(type))
					{
						match = true;
					}
				}

				if (match)
					continue;

				if (searchDescriptions)
				{
					string desc = Provider.provider.economyService.getInventoryDescription(details.m_iDefinition.m_SteamItemDef);
					if (tokenFilter.Value.matches(desc))
					{
						match = true;
					}
				}

				if (match)
					continue;

				filteredItems.RemoveAtFast(index);
			}
		}

		/// <summary>
		/// Removed items that are not equipped.
		/// </summary>
		private static void applyEquippedFilter()
		{
			if (filterEquipped == false)
				return; // Not enabled.

			for (int index = filteredItems.Count - 1; index >= 0; --index)
			{
				SteamItemDetails_t details = filteredItems[index];

				if (Characters.isEquipped(details.m_itemId.m_SteamItemInstanceID))
					continue;

				filteredItems.RemoveAtFast(index);
			}
		}

		private static void sortFilteredItems()
		{
			IComparer<SteamItemDetails_t> comparer;
			switch (sortMode)
			{
				default:
				case ESortMode.Date:
					comparer = null; // Items are returned by date acquired by default.
					break;

				case ESortMode.Rarity:
					comparer = new EconSortMode_Rarity();
					break;

				case ESortMode.Name:
					comparer = new EconSortMode_Name();
					break;

				case ESortMode.Type:
					comparer = new EconSortMode_Type();
					break;
			}

			if (comparer != null)
			{
				filteredItems.Sort(comparer);
			}

			if (reverseSortOrder)
			{
				// Comparer could be negated, but this is easy.
				filteredItems.Reverse();
			}
		}

		private static void updateFilter()
		{
			if (filterMode == EEconFilterMode.STAT_TRACKER)
			{
				filteredItems = new List<SteamItemDetails_t>();

				foreach (SteamItemDetails_t details in Provider.provider.economyService.inventory)
				{
					System.Guid item = Provider.provider.economyService.getInventoryItemGuid(details.m_iDefinition.m_SteamItemDef);
					int skin = Provider.provider.economyService.getInventorySkinID(details.m_iDefinition.m_SteamItemDef);
					if (item != default && skin != 0)
					{
						filteredItems.Add(details);
					}
				}
			}
			else if (filterMode == EEconFilterMode.STAT_TRACKER_REMOVAL)
			{
				filteredItems = new List<SteamItemDetails_t>();

				foreach (SteamItemDetails_t details in Provider.provider.economyService.inventory)
				{
					EStatTrackerType type;
					int kills;
					if (Provider.provider.economyService.getInventoryStatTrackerValue(details.m_itemId.m_SteamItemInstanceID, out type, out kills))
					{
						if (type != EStatTrackerType.NONE)
						{
							filteredItems.Add(details);
						}
					}
				}
			}
			else if (filterMode == EEconFilterMode.RAGDOLL_EFFECT_REMOVAL)
			{
				filteredItems = new List<SteamItemDetails_t>();

				foreach (SteamItemDetails_t details in Provider.provider.economyService.inventory)
				{
					ERagdollEffect effect;
					if (Provider.provider.economyService.getInventoryRagdollEffect(details.m_itemId.m_SteamItemInstanceID, out effect))
					{
						if (effect != ERagdollEffect.None)
						{
							filteredItems.Add(details);
						}
					}
				}
			}
			else if (filterMode == EEconFilterMode.RAGDOLL_EFFECT)
			{
				filteredItems = new List<SteamItemDetails_t>();

				foreach (SteamItemDetails_t details in Provider.provider.economyService.inventory)
				{
					System.Guid item = Provider.provider.economyService.getInventoryItemGuid(details.m_iDefinition.m_SteamItemDef);
					int skin = Provider.provider.economyService.getInventorySkinID(details.m_iDefinition.m_SteamItemDef);
					if (item != default && skin != 0)
					{
						ERagdollEffect effect;
						Provider.provider.economyService.getInventoryRagdollEffect(details.m_itemId.m_SteamItemInstanceID, out effect);
						if (effect == ERagdollEffect.None)
						{
							filteredItems.Add(details);
						}
					}
				}
			}
			else
			{
				filteredItems = new List<SteamItemDetails_t>(Provider.provider.economyService.inventory);
			}

			applySearchTextFilter();
			applyEquippedFilter();
			sortFilteredItems();
		}

		public static void updatePage()
		{
			availableBox.Text = ItemTool.filterRarityRichText(localization.format("Craft_Available", Provider.provider.economyService.countInventoryPackages(19000)));

			pageBox.Text = localization.format("Page", pageIndex + 1, numberOfPages);

			if (packageButtons == null)
			{
				return;
			}

			int offset = packageButtons.Length * pageIndex;
			for (int index = 0; index < packageButtons.Length; index++)
			{
				if (offset + index < filteredItems.Count)
				{
					packageButtons[index].updateInventory(filteredItems[offset + index].m_itemId.m_SteamItemInstanceID, filteredItems[offset + index].m_iDefinition.m_SteamItemDef, filteredItems[offset + index].m_unQuantity, true, false);
				}
				else
				{
					packageButtons[index].updateInventory(0, 0, 0, false, false);
				}
			}
		}

		private static void onDraggedCharacterSlider(ISleekSlider slider, float state)
		{
			Characters.characterYaw = state * 360;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsUI.open();
			close();
		}

		private static void onClickedItemstoreButton(ISleekElement button)
		{
			if (itemstoreNewLabel != null)
			{
				itemstoreButton.RemoveChild(itemstoreNewLabel);
				itemstoreNewLabel = null;

				ItemStoreSavedata.MarkNewListingsPageSeen();
				ItemStore.Get().ViewNewItems();
			}
			else
			{
				ItemStore.Get().ViewStore();
			}
		}

		private static void setCrafting(bool isCrafting)
		{
			inventory.IsVisible = !isCrafting;
			crafting.IsVisible = isCrafting;

			craftingButton.icon = inventory.IsVisible ? icons.load<Texture2D>("Crafting") : icons.load<Texture2D>("Backpack");
			craftingButton.text = localization.format(inventory.IsVisible ? "Crafting" : "Backpack");
			craftingButton.tooltip = localization.format(inventory.IsVisible ? "Crafting_Tooltip" : "Backpack_Tooltip");

			MenuSurvivorsUI.clothingUI.RefreshCraftingOptions();
		}

		private static void onClickedCraftingButton(ISleekElement button)
		{
			if (craftingNewLabel != null)
			{
				itemstoreButton.RemoveChild(craftingNewLabel);
				craftingNewLabel = null;

				ItemStoreSavedata.MarkNewCraftingPageSeen();
			}

			setCrafting(!crafting.IsVisible);
		}

		private static void onInventoryExchanged(List<SteamItemDetails_t> grantedItems)
		{
			if (!isCrafting)
			{
				return;
			}

			isCrafting = false;
			MenuUI.closeAlert();

			for (int index = grantedItems.Count - 1; index >= 0; --index)
			{
				if (grantedItems[index].m_iDefinition.m_SteamItemDef == 19000)
				{
					// Remove crafting materials from crafting alert.
					grantedItems.RemoveAtFast(index);
				}
			}

			MenuUI.alertNewItems(localization.format("Origin_Craft"), grantedItems);

			SteamItemDetails_t primaryItem = grantedItems[0]; // At the moment we only expect one item granted at a time...
			MenuSurvivorsClothingItemUI.viewItem(primaryItem.m_iDefinition.m_SteamItemDef, primaryItem.m_unQuantity, primaryItem.m_itemId.m_SteamItemInstanceID);
			MenuSurvivorsClothingItemUI.open();

			close();
		}

		private static void onInventoryPurchased(List<SteamItemDetails_t> grantedItems)
		{
			MenuUI.closeAlert();
			MenuUI.alertPurchasedItems(localization.format("Origin_Purchase"), grantedItems);
		}

		private static void onInventoryExchangeFailed()
		{
			if (!isCrafting)
				return;

			UnturnedLog.info("Crafting failed");

			isCrafting = false;
			MenuUI.closeAlert();
		}

		private void OnLiveConfigRefreshed()
		{
			areEconCraftOptionsDirty = true;
			RefreshCraftingOptions();
		}

		private void RefreshCraftingOptions()
		{
			if (!areEconCraftOptionsDirty || !crafting.IsVisible || !active)
			{
				return;
			}

			craftingScrollBox.RemoveAllChildren();

			econCraftOptions = new List<EconCraftOption>
			{
				new EconCraftOption("Craft_Common_Cosmetic", 10003, 2),
				new EconCraftOption("Craft_Common_Skin", 10006, 2),
				new EconCraftOption("Craft_Uncommon_Cosmetic", 10004, 13),
				new EconCraftOption("Craft_Uncommon_Skin", 10007, 13),
				new EconCraftOption("Craft_Stat_Tracker_Total_Kills", 19001, 30),
				new EconCraftOption("Craft_Stat_Tracker_Player_Kills", 19002, 30),
				new EconCraftOption("Craft_Ragdoll_Effect_Zero_Kelvin", 19003, 50),
				new EconCraftOption("Craft_Ragdoll_Effect_Jaded", 19013, 50),
				new EconCraftOption("Craft_Stat_Tracker_Removal_Tool", 19004, 15),
				new EconCraftOption("Craft_Ragdoll_Effect_Removal_Tool", 19005, 15),
			};

			if (HolidayUtil.getActiveHoliday() == ENPCHoliday.PRIDE_MONTH)
			{
				econCraftOptions.Add(new EconCraftOption("Craft_ProgressPridePin", 1333, 5));
				econCraftOptions.Add(new EconCraftOption("Craft_ProgressPrideJersey", 1334, 5));
			}

#if !DEDICATED_SERVER
			var liveConfig = LiveConfig.Get().itemCrafting;
			if (liveConfig.recipes != null)
			{
				foreach (LiveConfigItemCraftingRecipe recipe in liveConfig.recipes)
				{
					string key;
					switch (recipe.targetItemDefId)
					{
						default:
							key = $"Craft_{recipe.targetItemDefId}";
							break;

						case 19043:
							key = "Craft_Mythical_Skin";
							break;
					}
					econCraftOptions.Add(new EconCraftOption(key, recipe.targetItemDefId, (ushort) recipe.craftingMaterialsRequired));
				}
			}
#endif // !DEDICATED_SERVER

			int offset = 0;
			craftingButtons = new ISleekButton[econCraftOptions.Count];
			for (int index = 0; index < craftingButtons.Length; index++)
			{
				EconCraftOption option = econCraftOptions[index];

				ISleekButton craftButton = Glazier.Get().CreateButton();
				craftButton.PositionOffset_Y = offset;
				craftButton.SizeScale_X = 1;
				craftButton.SizeOffset_Y = 30;
				craftButton.AllowRichText = true;
				craftButton.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				craftButton.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				craftButton.Text = ItemTool.filterRarityRichText(localization.format("Craft_Entry", localization.format(option.token), option.scrapsNeeded));
				craftButton.OnClicked += onClickedCraftButton;
				craftingScrollBox.AddChild(craftButton);

				craftingButtons[index] = craftButton;
				offset += 30;
			}

#if !DEDICATED_SERVER
			if (!LiveConfig.WasPopulated)
			{
				offset += 10;
				ISleekBox waitingBox = Glazier.Get().CreateBox();
				waitingBox.PositionOffset_Y = offset;
				waitingBox.SizeScale_X = 1;
				waitingBox.SizeOffset_Y = 30;
				waitingBox.Text = localization.format("Craft_NoLiveConfig");
				craftingScrollBox.AddChild(waitingBox);
				offset += 30;
			}
#endif // !DEDICATED_SERVER

			craftingScrollBox.ScaleContentToWidth = true;
			craftingScrollBox.ContentSizeOffset = new Vector2(0.0f, offset);
		}

		public void OnDestroy()
		{
			boxUI.OnDestroy();

#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
#endif // !DEDICATED_SERVER

			Provider.provider.economyService.onInventoryExchanged -= onInventoryExchanged;
			Provider.provider.economyService.onInventoryPurchased -= onInventoryPurchased;
			Provider.provider.economyService.onInventoryExchangeFailed -= onInventoryExchangeFailed;

			Provider.provider.economyService.onInventoryRefreshed -= onInventoryRefreshed;
			Provider.provider.economyService.onInventoryDropped -= onInventoryDropped;

			Characters.onCharacterUpdated -= onCharacterUpdated;
			ItemStore.Get().OnPricesReceived -= OnPricesReceived;
		}

		public MenuSurvivorsClothingUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothing.dat");

			icons = Bundles.getIconsBundle("UI/Menu/Icons/Survivors/MenuSurvivorsClothing");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;
			pageIndex = 0;
			filterMode = EEconFilterMode.SEARCH;

			inventory = Glazier.Get().CreateConstraintFrame();
			inventory.PositionOffset_Y = 80;
			inventory.PositionScale_X = 0.5f;
			inventory.SizeScale_X = 0.5f;
			inventory.SizeScale_Y = 1;
			inventory.SizeOffset_Y = -120;
			inventory.Constraint = ESleekConstraint.FitInParent;
			container.AddChild(inventory);

			crafting = Glazier.Get().CreateConstraintFrame();
			crafting.PositionOffset_Y = 40;
			crafting.PositionScale_X = 0.5f;
			crafting.SizeScale_X = 0.5f;
			crafting.SizeScale_Y = 1;
			crafting.SizeOffset_Y = -80;
			crafting.Constraint = ESleekConstraint.FitInParent;
			container.AddChild(crafting);
			crafting.IsVisible = false;

			packageButtons = new SleekInventory[25];
			for (int index = 0; index < packageButtons.Length; index++)
			{
				SleekInventory button = new SleekInventory();
				button.PositionOffset_X = 5;
				button.PositionOffset_Y = 5;
				button.PositionScale_X = index % 5 * 0.2f;
				button.PositionScale_Y = Mathf.FloorToInt(index / 5f) * 0.2f;
				button.SizeOffset_X = -10;
				button.SizeOffset_Y = -10;
				button.SizeScale_X = 0.2f;
				button.SizeScale_Y = 0.2f;
				button.onClickedInventory = onClickedInventory;
				inventory.AddChild(button);

				packageButtons[index] = button;
			}

			searchField = Glazier.Get().CreateStringField();
			searchField.PositionOffset_X = 45;
			searchField.PositionOffset_Y = -35;
			searchField.SizeOffset_X = -160;
			searchField.SizeOffset_Y = 30;
			searchField.SizeScale_X = 1f;
			searchField.PlaceholderText = localization.format("Search_Field_Hint");
			searchField.OnTextSubmitted += onEnteredSearchField;
			inventory.AddChild(searchField);

			searchButton = Glazier.Get().CreateButton();
			searchButton.PositionOffset_X = -105;
			searchButton.PositionOffset_Y = -35;
			searchButton.PositionScale_X = 1;
			searchButton.SizeOffset_X = 100;
			searchButton.SizeOffset_Y = 30;
			searchButton.Text = localization.format("Search");
			searchButton.TooltipText = localization.format("Search_Tooltip");
			searchButton.OnClicked += onClickedSearchButton;
			inventory.AddChild(searchButton);

			filterBox = Glazier.Get().CreateBox();
			filterBox.PositionOffset_X = 5;
			filterBox.PositionOffset_Y = -75;
			filterBox.SizeOffset_X = -120;
			filterBox.SizeOffset_Y = 30;
			filterBox.SizeScale_X = 1f;
			filterBox.AllowRichText = true;
			filterBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			filterBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			inventory.AddChild(filterBox);
			filterBox.IsVisible = false;

			cancelFilterButton = Glazier.Get().CreateButton();
			cancelFilterButton.PositionOffset_X = -105;
			cancelFilterButton.PositionOffset_Y = -75;
			cancelFilterButton.PositionScale_X = 1;
			cancelFilterButton.SizeOffset_X = 100;
			cancelFilterButton.SizeOffset_Y = 30;
			cancelFilterButton.Text = localization.format("Cancel_Filter");
			cancelFilterButton.TooltipText = localization.format("Cancel_Filter_Tooltip");
			cancelFilterButton.OnClicked += onClickedCancelFilterButton;
			inventory.AddChild(cancelFilterButton);
			cancelFilterButton.IsVisible = false;

			pageBox = Glazier.Get().CreateBox();
			pageBox.PositionOffset_X = -145;
			pageBox.PositionOffset_Y = 5;
			pageBox.PositionScale_X = 1;
			pageBox.PositionScale_Y = 1;
			pageBox.SizeOffset_X = 100;
			pageBox.SizeOffset_Y = 30;
			pageBox.FontSize = ESleekFontSize.Medium;
			inventory.AddChild(pageBox);

			infoBox = Glazier.Get().CreateBox();
			infoBox.PositionOffset_X = 5;
			infoBox.PositionOffset_Y = -25;
			infoBox.PositionScale_Y = 0.5f;
			infoBox.SizeScale_X = 1;
			infoBox.SizeOffset_X = -10;
			infoBox.SizeOffset_Y = 50;
			infoBox.Text = localization.format("No_Items");
			infoBox.FontSize = ESleekFontSize.Medium;
			inventory.AddChild(infoBox);
			infoBox.IsVisible = !Provider.provider.economyService.isInventoryAvailable;

			leftButton = new SleekButtonIcon(icons.load<Texture2D>("Left"));
			leftButton.PositionOffset_X = -185;
			leftButton.PositionOffset_Y = 5;
			leftButton.PositionScale_X = 1;
			leftButton.PositionScale_Y = 1;
			leftButton.SizeOffset_X = 30;
			leftButton.SizeOffset_Y = 30;
			leftButton.tooltip = localization.format("Left_Tooltip");
			leftButton.iconColor = ESleekTint.FOREGROUND;
			leftButton.onClickedButton += onClickedLeftButton;
			inventory.AddChild(leftButton);

			rightButton = new SleekButtonIcon(icons.load<Texture2D>("Right"));
			rightButton.PositionOffset_X = -35;
			rightButton.PositionOffset_Y = 5;
			rightButton.PositionScale_X = 1;
			rightButton.PositionScale_Y = 1;
			rightButton.SizeOffset_X = 30;
			rightButton.SizeOffset_Y = 30;
			rightButton.tooltip = localization.format("Right_Tooltip");
			rightButton.iconColor = ESleekTint.FOREGROUND;
			rightButton.onClickedButton += onClickedRightButton;
			inventory.AddChild(rightButton);

			optionsButton = new SleekButtonIcon(icons.load<Texture2D>("Left"));
			optionsButton.PositionOffset_X = 5;
			optionsButton.PositionOffset_Y = -35;
			optionsButton.SizeOffset_X = 30;
			optionsButton.SizeOffset_Y = 30;
			optionsButton.tooltip = localization.format("Advanced_Options_Tooltip");
			optionsButton.iconColor = ESleekTint.FOREGROUND;
			optionsButton.onClickedButton += onClickedOptionsButton;
			inventory.AddChild(optionsButton);

			optionsPanel = Glazier.Get().CreateFrame();
			optionsPanel.PositionOffset_X = -200;
			optionsPanel.PositionOffset_Y = -35;
			optionsPanel.SizeOffset_X = 200;
			optionsPanel.SizeOffset_Y = 400;
			optionsPanel.IsVisible = false;
			inventory.AddChild(optionsPanel);

			searchDescriptionsToggle = Glazier.Get().CreateToggle();
			searchDescriptionsToggle.SizeOffset_X = 40;
			searchDescriptionsToggle.SizeOffset_Y = 40;
			searchDescriptionsToggle.AddLabel(localization.format("Search_Descriptions_Label"), ESleekSide.RIGHT);
			searchDescriptionsToggle.Value = searchDescriptions;
			searchDescriptionsToggle.OnValueChanged += onToggledSearchDescriptions;
			optionsPanel.AddChild(searchDescriptionsToggle);

			sortModeButton = new SleekButtonState(new GUIContent(localization.format("Sort_Mode_Date")),
				new GUIContent(localization.format("Sort_Mode_Rarity")),
				new GUIContent(localization.format("Sort_Mode_Name")));
			sortModeButton.PositionOffset_Y = 50;
			sortModeButton.SizeOffset_X = 100;
			sortModeButton.SizeOffset_Y = 30;
			sortModeButton.AddLabel(localization.format("Sort_Mode_Label"), ESleekSide.RIGHT);
			sortModeButton.tooltip = localization.format("Sort_Mode_Tooltip");
			sortModeButton.state = (int) sortMode;
			sortModeButton.onSwappedState = onChangedSortMode;
			optionsPanel.AddChild(sortModeButton);

			reverseSortOrderToggle = Glazier.Get().CreateToggle();
			reverseSortOrderToggle.PositionOffset_Y = 90;
			reverseSortOrderToggle.SizeOffset_X = 40;
			reverseSortOrderToggle.SizeOffset_Y = 40;
			reverseSortOrderToggle.AddLabel(localization.format("Reverse_Sort_Order_Label"), ESleekSide.RIGHT);
			reverseSortOrderToggle.Value = reverseSortOrder;
			reverseSortOrderToggle.OnValueChanged += onToggledReverseSortOrder;
			optionsPanel.AddChild(reverseSortOrderToggle);

			filterEquippedToggle = Glazier.Get().CreateToggle();
			filterEquippedToggle.PositionOffset_Y = 140;
			filterEquippedToggle.SizeOffset_X = 40;
			filterEquippedToggle.SizeOffset_Y = 40;
			filterEquippedToggle.AddLabel(localization.format("Filter_Equipped_Label"), ESleekSide.RIGHT);
			filterEquippedToggle.Value = filterEquipped;
			filterEquippedToggle.OnValueChanged += onToggledFilterEquipped;
			optionsPanel.AddChild(filterEquippedToggle);

			refreshButton = new SleekButtonIcon(icons.load<Texture2D>("Refresh"));
			refreshButton.PositionOffset_X = 5;
			refreshButton.PositionOffset_Y = 5;
			refreshButton.PositionScale_Y = 1;
			refreshButton.SizeOffset_X = 30;
			refreshButton.SizeOffset_Y = 30;
			refreshButton.tooltip = localization.format("Refresh_Tooltip");
			refreshButton.iconColor = ESleekTint.FOREGROUND;
			refreshButton.onClickedButton += onClickedRefreshButton;
			inventory.AddChild(refreshButton);

#if WITH_GRANTPACKAGE_PROMO
			grantPackagePromoButton = Glazier.Get().CreateButton();
			grantPackagePromoButton.PositionOffset_Y = -280;
			grantPackagePromoButton.PositionScale_Y = 1f;
			grantPackagePromoButton.SizeOffset_X = 200;
			grantPackagePromoButton.SizeOffset_Y = 50;
			grantPackagePromoButton.Text = "Claim Unturned II Access";
			grantPackagePromoButton.OnClicked += onClickedGrantPackagePromoButton;
			grantPackagePromoButton.IsVisible = false;
			container.AddChild(grantPackagePromoButton);
#endif // WITH_GRANTPACKAGE_PROMO

			characterSlider = Glazier.Get().CreateSlider();
			characterSlider.PositionOffset_X = 45;
			characterSlider.PositionOffset_Y = 10;
			characterSlider.PositionScale_Y = 1;
			characterSlider.SizeOffset_X = -240;
			characterSlider.SizeOffset_Y = 20;
			characterSlider.SizeScale_X = 1f;
			characterSlider.Orientation = ESleekOrientation.HORIZONTAL;
			characterSlider.OnValueChanged += onDraggedCharacterSlider;
			inventory.AddChild(characterSlider);

			availableBox = Glazier.Get().CreateBox();
			availableBox.SizeScale_X = 1;
			availableBox.SizeOffset_Y = 30;
			availableBox.AllowRichText = true;
			availableBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			availableBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			crafting.AddChild(availableBox);

			craftingScrollBox = Glazier.Get().CreateScrollView();
			craftingScrollBox.PositionOffset_Y = 40;
			craftingScrollBox.SizeScale_X = 1;
			craftingScrollBox.SizeScale_Y = 1;
			craftingScrollBox.SizeOffset_Y = -40;
			crafting.AddChild(craftingScrollBox);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);

			itemstoreButton = new SleekButtonIcon(icons.load<Texture2D>("ItemStore"), 40);
			itemstoreButton.PositionOffset_Y = -170;
			itemstoreButton.PositionScale_Y = 1f;
			itemstoreButton.SizeOffset_X = 200;
			itemstoreButton.SizeOffset_Y = 50;
			itemstoreButton.text = localization.format("Itemstore");
			itemstoreButton.tooltip = localization.format("Itemstore_Tooltip");
			itemstoreButton.onClickedButton += onClickedItemstoreButton;
			itemstoreButton.fontSize = ESleekFontSize.Medium;
			itemstoreButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(itemstoreButton);

			if (!Provider.provider.economyService.doesCountryAllowRandomItems && Provider.provider.economyService.hasCountryDetails)
			{
				ISleekLabel regionLabel = Glazier.Get().CreateLabel();
				regionLabel.PositionOffset_X = 210;
				regionLabel.PositionOffset_Y = -170;
				regionLabel.PositionScale_Y = 1f;
				regionLabel.SizeOffset_X = 200;
				regionLabel.SizeOffset_Y = 50;
				regionLabel.TextAlignment = TextAnchor.MiddleLeft;
				regionLabel.Text = localization.format("Itemstore_Region_Box_Disabled", Provider.provider.economyService.getCountryWarningId());
				container.AddChild(regionLabel);
			}

			craftingButton = new SleekButtonIcon(icons.load<Texture2D>("Crafting"));
			craftingButton.PositionOffset_Y = -110;
			craftingButton.PositionScale_Y = 1f;
			craftingButton.SizeOffset_X = 200;
			craftingButton.SizeOffset_Y = 50;
			craftingButton.text = localization.format("Crafting");
			craftingButton.tooltip = localization.format("Crafting_Tooltip");
			craftingButton.onClickedButton += onClickedCraftingButton;
			craftingButton.fontSize = ESleekFontSize.Medium;
			craftingButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(craftingButton);

			if (!ItemStoreSavedata.WasNewCraftingPageSeen())
			{
				craftingNewLabel = Glazier.Get().CreateLabel();
				craftingNewLabel.SizeScale_X = 1.0f;
				craftingNewLabel.SizeScale_Y = 1.0f;
				craftingNewLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				craftingNewLabel.TextAlignment = TextAnchor.UpperRight;
				craftingNewLabel.TextColor = Color.green;
				craftingNewLabel.Text = Provider.localization.format("New");
				craftingButton.AddChild(craftingNewLabel);
			}

			Provider.provider.economyService.onInventoryExchanged += onInventoryExchanged;
			Provider.provider.economyService.onInventoryPurchased += onInventoryPurchased;
			Provider.provider.economyService.onInventoryExchangeFailed += onInventoryExchangeFailed;

			Provider.provider.economyService.onInventoryRefreshed += onInventoryRefreshed;
			Provider.provider.economyService.onInventoryDropped += onInventoryDropped;

			Characters.onCharacterUpdated += onCharacterUpdated;
			ItemStore.Get().OnPricesReceived += OnPricesReceived;

#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed += OnLiveConfigRefreshed;
#endif // !DEDICATED_SERVER
			areEconCraftOptionsDirty = true;

			updateFilter();
			updatePage();

			itemUI = new MenuSurvivorsClothingItemUI();
			inspectUI = new MenuSurvivorsClothingInspectUI();
			deleteUI = new MenuSurvivorsClothingDeleteUI();
			boxUI = new MenuSurvivorsClothingBoxUI();

			itemStoreUI = new ItemStoreMenu();
			MenuUI.container.AddChild(itemStoreUI);
		}

		private MenuSurvivorsClothingItemUI itemUI;
		private MenuSurvivorsClothingInspectUI inspectUI;
		private MenuSurvivorsClothingDeleteUI deleteUI;
		private MenuSurvivorsClothingBoxUI boxUI;
		private ItemStoreMenu itemStoreUI;

		private bool areEconCraftOptionsDirty;
		private List<EconCraftOption> econCraftOptions;
	}
}
