////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class ItemStoreMenu : SleekFullscreenBox
	{
		public static ItemStoreMenu instance;

		public Local localization
		{
			get;
			private set;
		}

		public IconsBundle icons
		{
			get;
			private set;
		}

		public bool IsOpen
		{
			get;
			private set;
		}

		public void Open()
		{
			IsOpen = true;
			AnimateIntoView();

			// Items in cart cannot be modified from this menu, so setting visibility when opened is a safe spot.
			viewCartButton.IsVisible = !ItemStore.Get().IsCartEmpty;

			if (areListingsDirty)
			{
				areListingsDirty = false;
				FilterListings();
			}
			else
			{
				RefreshListingsInCart();
			}
		}

		public void OpenNewItems()
		{
			searchField.Text = string.Empty;
			categoryFilter = ECategoryFilter.New;
			areListingsDirty = true;
			Open();
		}

		public void Close()
		{
			if (!IsOpen)
				return;

			IsOpen = false;
			AnimateOutOfView(0.0f, 1.0f);
		}

		public ItemStoreMenu()
		{
			localization = Localization.read("/Menu/Survivors/ItemStoreMenu.dat");
			icons = Bundles.getIconsBundle("UI/Menu/Icons/Survivors/ItemStore");

			instance = this;

			PositionScale_Y = 1.0f;
			PositionOffset_X = 10;
			PositionOffset_Y = 10;
			SizeOffset_X = -20;
			SizeOffset_Y = -20;
			SizeScale_X = 1.0f;
			SizeScale_Y = 1.0f;

			ISleekConstraintFrame grid = Glazier.Get().CreateConstraintFrame();
			grid.PositionOffset_Y = 70;
			grid.PositionScale_X = 0.5f;
			grid.SizeScale_X = 0.5f;
			grid.SizeScale_Y = 1;
			grid.SizeOffset_Y = -105;
			grid.Constraint = ESleekConstraint.FitInParent;
			AddChild(grid);

			listingButtons = new SleekItemStoreListing[25];
			for (int listingIndex = 0; listingIndex < 25; ++listingIndex)
			{
				SleekItemStoreListing button = new SleekItemStoreListing();
				button.PositionOffset_X = 5;
				button.PositionOffset_Y = 5;
				button.PositionScale_X = listingIndex % 5 * 0.2f;
				button.PositionScale_Y = Mathf.FloorToInt(listingIndex / 5f) * 0.2f;
				button.SizeOffset_X = -10;
				button.SizeOffset_Y = -10;
				button.SizeScale_X = 0.2f;
				button.SizeScale_Y = 0.2f;
				grid.AddChild(button);
				listingButtons[listingIndex] = button;
			}

			categoryButtonsFrame = Glazier.Get().CreateFrame();
			categoryButtonsFrame.PositionOffset_Y = -70;
			categoryButtonsFrame.SizeScale_X = 1.0f;
			categoryButtonsFrame.SizeOffset_Y = 30;
			grid.AddChild(categoryButtonsFrame);

			searchField = Glazier.Get().CreateStringField();
			searchField.PositionOffset_X = 40;
			searchField.PositionOffset_Y = -35;
			searchField.SizeOffset_X = -150;
			searchField.SizeOffset_Y = 30;
			searchField.SizeScale_X = 1f;
			searchField.PlaceholderText = MenuSurvivorsClothingUI.localization.format("Search_Field_Hint");
			searchField.OnTextSubmitted += OnEnteredSearchField;
			grid.AddChild(searchField);

			ISleekButton searchButton = Glazier.Get().CreateButton();
			searchButton.PositionOffset_X = -100;
			searchButton.PositionOffset_Y = -35;
			searchButton.PositionScale_X = 1.0f;
			searchButton.SizeOffset_X = 100;
			searchButton.SizeOffset_Y = 30;
			searchButton.Text = MenuSurvivorsClothingUI.localization.format("Search");
			searchButton.TooltipText = MenuSurvivorsClothingUI.localization.format("Search_Tooltip");
			searchButton.OnClicked += OnClickedSearchButton;
			grid.AddChild(searchButton);

			optionsButton = new SleekButtonIcon(MenuSurvivorsClothingUI.icons.load<Texture2D>("Left"));
			optionsButton.PositionOffset_Y = -35;
			optionsButton.SizeOffset_X = 30;
			optionsButton.SizeOffset_Y = 30;
			optionsButton.tooltip = MenuSurvivorsClothingUI.localization.format("Advanced_Options_Tooltip");
			optionsButton.iconColor = ESleekTint.FOREGROUND;
			optionsButton.onClickedButton += OnClickedOptionsButton;
			grid.AddChild(optionsButton);

			optionsPanel = Glazier.Get().CreateFrame();
			optionsPanel.PositionOffset_X = -205;
			optionsPanel.PositionOffset_Y = -35;
			optionsPanel.SizeOffset_X = 200;
			optionsPanel.SizeOffset_Y = 400;
			optionsPanel.IsVisible = false;
			grid.AddChild(optionsPanel);

			showOwnedToggle = Glazier.Get().CreateToggle();
			showOwnedToggle.SizeOffset_X = 40;
			showOwnedToggle.SizeOffset_Y = 40;
			showOwnedToggle.AddLabel(localization.format("FilterShowOwned_Label"), ESleekSide.RIGHT);
			showOwnedToggle.OnValueChanged += OnShowOwnedToggled;
			optionsPanel.AddChild(showOwnedToggle);

			pageBox = Glazier.Get().CreateBox();
			pageBox.PositionOffset_X = -50;
			pageBox.PositionOffset_Y = 5;
			pageBox.PositionScale_X = 0.5f;
			pageBox.PositionScale_Y = 1.0f;
			pageBox.SizeOffset_X = 100;
			pageBox.SizeOffset_Y = 30;
			pageBox.FontSize = ESleekFontSize.Medium;
			grid.AddChild(pageBox);

			SleekButtonIcon leftButton = new SleekButtonIcon(MenuSurvivorsClothingUI.icons.load<Texture2D>("Left"));
			leftButton.PositionOffset_X = -85;
			leftButton.PositionOffset_Y = 5;
			leftButton.PositionScale_X = 0.5f;
			leftButton.PositionScale_Y = 1.0f;
			leftButton.SizeOffset_X = 30;
			leftButton.SizeOffset_Y = 30;
			leftButton.tooltip = MenuSurvivorsClothingUI.localization.format("Left_Tooltip");
			leftButton.iconColor = ESleekTint.FOREGROUND;
			leftButton.onClickedButton += OnClickedLeftPageButton;
			grid.AddChild(leftButton);

			SleekButtonIcon rightButton = new SleekButtonIcon(MenuSurvivorsClothingUI.icons.load<Texture2D>("Right"));
			rightButton.PositionOffset_X = 55;
			rightButton.PositionOffset_Y = 5;
			rightButton.PositionScale_X = 0.5f;
			rightButton.PositionScale_Y = 1.0f;
			rightButton.SizeOffset_X = 30;
			rightButton.SizeOffset_Y = 30;
			rightButton.tooltip = MenuSurvivorsClothingUI.localization.format("Right_Tooltip");
			rightButton.iconColor = ESleekTint.FOREGROUND;
			rightButton.onClickedButton += OnClickedRightPageButton;
			grid.AddChild(rightButton);

			viewCartButton = Glazier.Get().CreateButton();
			viewCartButton.PositionOffset_Y = -110;
			viewCartButton.PositionScale_Y = 1.0f;
			viewCartButton.SizeOffset_X = 200;
			viewCartButton.SizeOffset_Y = 50;
			viewCartButton.Text = localization.format("ViewCart_Label");
			viewCartButton.TooltipText = localization.format("ViewCart_Tooltip");
			viewCartButton.OnClicked += OnClickedViewCartButton;
			viewCartButton.FontSize = ESleekFontSize.Medium;
			AddChild(viewCartButton);

			ISleekSprite viewCartImage = Glazier.Get().CreateSprite(icons.load<Sprite>("Cart"));
			viewCartImage.PositionOffset_X = 5;
			viewCartImage.PositionOffset_Y = 5;
			viewCartImage.SizeOffset_X = 40;
			viewCartImage.SizeOffset_Y = 40;
			viewCartImage.TintColor = ESleekTint.FOREGROUND;
			viewCartImage.DrawMethod = ESleekSpriteType.Regular;
			viewCartButton.AddChild(viewCartImage);

			SleekButtonIcon backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += OnClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			AddChild(backButton);

			cartMenu = new ItemStoreCartMenu();
			MenuUI.container.AddChild(cartMenu);

			detailsMenu = new ItemStoreDetailsMenu();
			MenuUI.container.AddChild(detailsMenu);

			bundleContentsMenu = new ItemStoreBundleContentsMenu();
			MenuUI.container.AddChild(bundleContentsMenu);

			ItemStore.Get().OnPricesReceived += OnPricesReceived;
			ItemStore.Get().OnPurchaseResult += OnPurchaseResult;
			ItemStore.Get().RequestPrices();
		}

		public override void OnDestroy()
		{
			ItemStore.Get().OnPricesReceived -= OnPricesReceived;
			ItemStore.Get().OnPurchaseResult -= OnPurchaseResult;
			base.OnDestroy();
		}

		private void OnPricesReceived()
		{
			filteredListings = new List<ItemStore.Listing>(ItemStore.Get().GetListings().Length);
			CreateFilterCategoryButtons();
			areListingsDirty = true;
		}

		private void OnPurchaseResult(ItemStore.EPurchaseResult result)
		{
			switch (result)
			{
				case ItemStore.EPurchaseResult.UnableToInitialize:
					MenuUI.alert(localization.format("PurchaseResult_UnableToInitialize"));
					break;

				case ItemStore.EPurchaseResult.Denied:
					MenuUI.alert(localization.format("PurchaseResult_Denied"));
					break;
			}
		}

		private void BuildListingsFromIndices(int[] listingIndices)
		{
			ItemStore.Listing[] listings = ItemStore.Get().GetListings();
			foreach (int index in listingIndices)
			{
				filteredListings.Add(listings[index]);
			}
		}

		private void BuildCategoryListings()
		{
			if (categoryFilter == ECategoryFilter.Specials)
			{
				BuildListingsFromIndices(ItemStore.Get().GetDiscountedListingIndices());
			}
			else if (categoryFilter == ECategoryFilter.New)
			{
				BuildListingsFromIndices(ItemStore.Get().GetNewListingIndices());
			}
			else if (categoryFilter == ECategoryFilter.Featured)
			{
				BuildListingsFromIndices(ItemStore.Get().GetFeaturedListingIndices());
			}
			else
			{
				ItemStore.Listing[] listings = ItemStore.Get().GetListings();
				filteredListings.AddRange(listings);

				if (categoryFilter == ECategoryFilter.Bundles)
				{
					for (int index = filteredListings.Count - 1; index >= 0; --index)
					{
						if (!Provider.provider.economyService.IsItemBundle(filteredListings[index].itemdefid))
						{
							filteredListings.RemoveAtFast(index);
						}
					}
				}
			}
		}

		/// <summary>
		/// Remove items that do not match search text.
		/// </summary>
		private void ApplySearchTextFilter()
		{
			string search = searchField.Text;
			if (string.IsNullOrEmpty(search))
				return;

			TokenSearchFilter? tokenFilter = TokenSearchFilter.parse(search);
			if (!tokenFilter.HasValue)
				return;

			for (int index = filteredListings.Count - 1; index >= 0; --index)
			{
				int itemdefid = filteredListings[index].itemdefid;

				string name = Provider.provider.economyService.getInventoryName(itemdefid);
				if (tokenFilter.Value.matches(name))
				{
					continue;
				}

				string type = Provider.provider.economyService.getInventoryType(itemdefid);
				if (tokenFilter.Value.matches(type))
				{
					continue;
				}

				filteredListings.RemoveAtFast(index);
			}
		}

		private void ApplyOwnedFilter()
		{
			if (!string.IsNullOrEmpty(searchField.Text))
			{
				// If searching for a specific item we include owned items.
				return;
			}

			if (showOwnedToggle.Value)
			{
				// Player specifically wants to include owned items.
				return;
			}

			HashSet<int> ownedItemDefIds = Provider.provider.economyService.GatherOwnedItemDefIds();

			for (int index = filteredListings.Count - 1; index >= 0; --index)
			{
				int itemdefid = filteredListings[index].itemdefid;

				if (Provider.provider.economyService.IsItemBundle(itemdefid))
				{
					List<int> containedItemDefIds = Provider.provider.economyService.GetBundleContents(itemdefid);
					if (containedItemDefIds != null && containedItemDefIds.Count > 0)
					{
						bool ownsAnyContainedItem = false;
						foreach (int containedItemDefId in containedItemDefIds)
						{
							if (ownedItemDefIds.Contains(containedItemDefId))
							{
								ownsAnyContainedItem = true;
								break;
							}
						}

						if (ownsAnyContainedItem)
						{
							filteredListings.RemoveAtFast(index);
							continue;
						}
					}
				}

				// Box, key, cosmetic, skin, etc.
				if (ownedItemDefIds.Contains(itemdefid))
				{
					filteredListings.RemoveAtFast(index);
				}
			}
		}

		private void SortListings()
		{
			filteredListings.Sort((ItemStore.Listing lhs, ItemStore.Listing rhs) =>
			{
				// Sort newer items to the front. Fallback to names.
				// (This was added 2023-06-19, so unfortunately it will be inaccurate for older items.)
				System.DateTime lhsTime = Provider.provider.economyService.GetCreationTime(lhs.itemdefid);
				System.DateTime rhsTime = Provider.provider.economyService.GetCreationTime(rhs.itemdefid);
				int comparison = -lhsTime.CompareTo(rhsTime);
				if (comparison == 0)
				{
					string lhsName = Provider.provider.economyService.getInventoryName(lhs.itemdefid);
					string rhsName = Provider.provider.economyService.getInventoryName(rhs.itemdefid);
					comparison = lhsName.CompareTo(rhsName);
				}
				return comparison;
			});
		}

		private void FilterListings()
		{
			filteredListings.Clear();
			BuildCategoryListings();
			ApplySearchTextFilter();
			ApplyOwnedFilter();
			SortListings();

			pageCount = MathfEx.GetPageCount(filteredListings.Count, listingButtons.Length);
			if (pageIndex >= pageCount)
			{
				pageIndex = pageCount - 1;
			}

			RefreshPage();
		}

		private void RefreshListingsInCart()
		{
			foreach (SleekItemStoreListing button in listingButtons)
			{
				button.RefreshInCart();
			}
		}

		/// <summary>
		/// Note SetListing also calls RefreshInCart.
		/// </summary>
		private void RefreshPage()
		{
			pageBox.Text = MenuSurvivorsClothingUI.localization.format("Page", pageIndex + 1, pageCount);

			int listingIndexOffset = pageIndex * listingButtons.Length;

			// Enable buttons which have a valid listing index.
			int visibleButtonCount = Mathf.Min(filteredListings.Count - listingIndexOffset, listingButtons.Length);
			for (int buttonIndex = 0; buttonIndex < visibleButtonCount; ++buttonIndex)
			{
				int listingIndex = listingIndexOffset + buttonIndex;
				listingButtons[buttonIndex].SetListing(filteredListings[listingIndex]);
			}
			// Disable buttons without a valid listing index.
			for (int buttonIndex = visibleButtonCount; buttonIndex < listingButtons.Length; ++buttonIndex)
			{
				listingButtons[buttonIndex].ClearListing();
			}
		}

		/// <summary>
		/// Cannot be created until store data is available.
		/// </summary>
		private void CreateFilterCategoryButtons()
		{
			ItemStore itemStore = ItemStore.Get();

			bool withSpecialsButton = itemStore.HasDiscountedListings;
			bool withNewButton = itemStore.HasNewListings;
			bool withFeaturedButton = itemStore.HasFeaturedListings;
			int categoryButtonCount = 2 +
				(withNewButton ? 1 : 0) +
				(withSpecialsButton ? 1 : 0) +
				(withFeaturedButton ? 1 : 0);
			float categoryButtonPosition = 0.0f;
			float categoryButtonScale = 1.0f / categoryButtonCount;

			if (withNewButton)
			{
				ISleekButton filterNewButton = Glazier.Get().CreateButton();
				filterNewButton.PositionScale_X = categoryButtonPosition;
				filterNewButton.SizeScale_X = categoryButtonScale;
				filterNewButton.SizeScale_Y = 1.0f;
				filterNewButton.Text = localization.format("FilterNewButton_Label") + " x" + itemStore.GetNewListingIndices().Length;
				filterNewButton.TooltipText = localization.format("FilterNewButton_Tooltip");
				filterNewButton.OnClicked += OnClickedFilterNew;
				categoryButtonsFrame.AddChild(filterNewButton);
				categoryButtonPosition += categoryButtonScale;
			}

			if (withFeaturedButton)
			{
				ISleekButton filterFeaturedButton = Glazier.Get().CreateButton();
				filterFeaturedButton.PositionScale_X = categoryButtonPosition;
				filterFeaturedButton.SizeScale_X = categoryButtonScale;
				filterFeaturedButton.SizeScale_Y = 1.0f;
				filterFeaturedButton.Text = localization.format("FilterFeaturedButton_Label") + " x" + itemStore.GetFeaturedListingIndices().Length;
				filterFeaturedButton.TooltipText = localization.format("FilterFeaturedButton_Label");
				filterFeaturedButton.OnClicked += OnClickedFilterFeatured;
				categoryButtonsFrame.AddChild(filterFeaturedButton);
				categoryButtonPosition += categoryButtonScale;
			}

			ISleekButton filterAllButton = Glazier.Get().CreateButton();
			filterAllButton.PositionScale_X = categoryButtonPosition;
			filterAllButton.SizeScale_X = categoryButtonScale;
			filterAllButton.SizeScale_Y = 1.0f;
			filterAllButton.Text = localization.format("FilterAllButton_Label");
			filterAllButton.TooltipText = localization.format("FilterAllButton_Tooltip");
			filterAllButton.OnClicked += OnClickedFilterAll;
			categoryButtonsFrame.AddChild(filterAllButton);
			categoryButtonPosition += categoryButtonScale;

			ISleekButton filterBundlesButton = Glazier.Get().CreateButton();
			filterBundlesButton.PositionScale_X = categoryButtonPosition;
			filterBundlesButton.SizeScale_X = categoryButtonScale;
			filterBundlesButton.SizeScale_Y = 1.0f;
			filterBundlesButton.Text = localization.format("FilterBundlesButton_Label");
			filterBundlesButton.TooltipText = localization.format("FilterBundlesButton_Tooltip");
			filterBundlesButton.OnClicked += OnClickedFilterBundles;
			categoryButtonsFrame.AddChild(filterBundlesButton);
			categoryButtonPosition += categoryButtonScale;

			if (withSpecialsButton)
			{
				ISleekButton filterSpecialsButton = Glazier.Get().CreateButton();
				filterSpecialsButton.PositionScale_X = categoryButtonPosition;
				filterSpecialsButton.SizeScale_X = categoryButtonScale;
				filterSpecialsButton.SizeScale_Y = 1.0f;
				filterSpecialsButton.Text = localization.format("FilterSpecialsButton_Label") + " x" + itemStore.GetDiscountedListingIndices().Length;
				filterSpecialsButton.TooltipText = localization.format("FilterSpecialsButton_Tooltip");
				filterSpecialsButton.OnClicked += OnClickedFilterSpecials;
				categoryButtonsFrame.AddChild(filterSpecialsButton);
				categoryButtonPosition += categoryButtonScale;
			}

			// Default to most interesting category.
			if (withSpecialsButton)
			{
				categoryFilter = ECategoryFilter.Specials;
			}
			else if (withNewButton)
			{
				categoryFilter = ECategoryFilter.New;
			}
			else if (withFeaturedButton)
			{
				categoryFilter = ECategoryFilter.Featured;
			}
		}

		private void OnClickedLeftPageButton(ISleekElement button)
		{
			if (pageIndex > 0)
			{
				--pageIndex;
			}
			else
			{
				pageIndex = pageCount - 1;
			}
			RefreshPage();
		}

		private void OnClickedRightPageButton(ISleekElement button)
		{
			if (pageIndex < pageCount - 1)
			{
				++pageIndex;
			}
			else
			{
				pageIndex = 0;
			}
			RefreshPage();
		}

		private void OnClickedFilterAll(ISleekElement button)
		{
			categoryFilter = ECategoryFilter.None;
			FilterListings();
		}

		private void OnClickedFilterBundles(ISleekElement button)
		{
			categoryFilter = ECategoryFilter.Bundles;
			FilterListings();
		}

		private void OnClickedFilterSpecials(ISleekElement button)
		{
			categoryFilter = ECategoryFilter.Specials;
			FilterListings();
		}

		private void OnClickedFilterNew(ISleekElement button)
		{
			categoryFilter = ECategoryFilter.New;
			FilterListings();
		}

		private void OnClickedFilterFeatured(ISleekElement button)
		{
			categoryFilter = ECategoryFilter.Featured;
			FilterListings();
		}

		private void OnEnteredSearchField(ISleekField field)
		{
			FilterListings();
		}

		private void OnClickedSearchButton(ISleekElement button)
		{
			FilterListings();
		}

		private void OnClickedOptionsButton(ISleekElement button)
		{
			optionsPanel.IsVisible = !optionsPanel.IsVisible;
			optionsButton.icon = MenuSurvivorsClothingUI.icons.load<Texture2D>(optionsPanel.IsVisible ? "Right" : "Left");
		}

		private void OnShowOwnedToggled(ISleekToggle toggle, bool state)
		{
			FilterListings();
		}

		private void OnClickedViewCartButton(ISleekElement button)
		{
			cartMenu.Open();
			Close();
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsClothingUI.open();
			Close();
		}

		private ItemStoreCartMenu cartMenu;
		private ItemStoreDetailsMenu detailsMenu;
		private ItemStoreBundleContentsMenu bundleContentsMenu;
		private SleekItemStoreListing[] listingButtons;

		private ISleekElement categoryButtonsFrame;
		private ISleekField searchField;

		/// <summary>
		/// Toggle button to open/close advanced filters panel.
		/// </summary>
		private static SleekButtonIcon optionsButton;

		/// <summary>
		/// On/off checkbox for including already-owned items in filter.
		/// </summary>
		private static ISleekToggle showOwnedToggle;

		/// <summary>
		/// Container for advanced options.
		/// </summary>
		private static ISleekElement optionsPanel;

		/// <summary>
		/// Displays the current page number.
		/// </summary>
		private ISleekBox pageBox;

		/// <summary>
		/// Only visible when cart is not empty.
		/// </summary>
		private ISleekButton viewCartButton;

		private List<ItemStore.Listing> filteredListings;

		/// <summary>
		/// [0, pageCount)
		/// </summary>
		private int pageIndex;
		private int pageCount;

		/// <summary>
		/// If true, listings should be re-filtered when opening the menu.
		/// </summary>
		private bool areListingsDirty;

		private enum ECategoryFilter
		{
			None,

			/// <summary>
			/// Collections of multiple items. 
			/// </summary>
			Bundles,

			/// <summary>
			/// Discounted items.
			/// </summary>
			Specials,

			/// <summary>
			/// Items marked as new in the Status.json file.
			/// </summary>
			New,

			/// <summary>
			/// Items marked as featured in the Status.json file.
			/// </summary>
			Featured,
		}

		private ECategoryFilter categoryFilter;
	}
}
