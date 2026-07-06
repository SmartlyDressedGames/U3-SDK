////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Examine a store listing with description text.
	/// </summary>
	internal class ItemStoreDetailsMenu : SleekFullscreenBox
	{
		public static ItemStoreDetailsMenu instance;

		public bool IsOpen
		{
			get;
			private set;
		}

		public void Open(ItemStore.Listing listing)
		{
			IsOpen = true;
			AnimateIntoView();

			this.listing = listing;
			quantityInCart = ItemStore.Get().GetQuantityInCart(listing.itemdefid);

			bool isSkin = Provider.provider.economyService.getInventorySkinID(listing.itemdefid) > 0;
			bool isInspectable = isSkin;
			if (!isInspectable)
			{
				Asset gameAsset = Assets.find(Provider.provider.economyService.getInventoryItemGuid(listing.itemdefid));
				if (gameAsset != null)
				{
					isInspectable = gameAsset is VehicleAsset || gameAsset is ItemClothingAsset;
				}
			}
			inspectButton.IsVisible = isInspectable;

			List<int> containedItems = Provider.provider.economyService.GetBundleContents(listing.itemdefid);
			inspectContainedItemsButton.IsVisible = containedItems != null;

			float actionsOffset = 55;
			if (inspectButton.IsVisible)
			{
				inspectButton.PositionOffset_Y = actionsOffset;
				actionsOffset += inspectButton.SizeOffset_Y + 5;
			}
			if (inspectContainedItemsButton.IsVisible)
			{
				inspectContainedItemsButton.PositionOffset_Y = actionsOffset;
				actionsOffset += inspectContainedItemsButton.SizeOffset_Y + 5;
			}

			actionsFrame.SizeOffset_Y = actionsOffset - 5;
			lowerBox.PositionOffset_Y = actionsFrame.SizeOffset_Y - 20;
			lowerBox.SizeOffset_Y = -lowerBox.PositionOffset_Y;

			iconImage.SetItemDefId(listing.itemdefid);
			Color itemColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
			nameLabel.TextColor = itemColor;
			nameLabel.Text = Provider.provider.economyService.getInventoryName(listing.itemdefid);

			string typeText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
			string descText = Provider.provider.economyService.getInventoryDescription(listing.itemdefid);
			descriptionLabel.Text = RichTextUtil.wrapWithColor(typeText, itemColor) + "\n\n" + descText;

			RefreshQuantity();
		}

		public void OpenCurrentListing()
		{
			IsOpen = true;
			AnimateIntoView();
		}

		public void Close()
		{
			if (!IsOpen)
				return;

			IsOpen = false;
			AnimateOutOfView(0.0f, 1.0f);
		}

		public ItemStoreDetailsMenu()
		{
			Local localization = ItemStoreMenu.instance.localization;

			instance = this;

			PositionScale_Y = 1.0f;
			PositionOffset_X = 10;
			PositionOffset_Y = 10;
			SizeOffset_X = -20;
			SizeOffset_Y = -20;
			SizeScale_X = 1.0f;
			SizeScale_Y = 1.0f;

			ISleekElement outerFrame = Glazier.Get().CreateFrame();
			outerFrame.PositionScale_X = 0.6f;
			outerFrame.PositionOffset_Y = 10;
			outerFrame.SizeScale_X = 0.3f;
			outerFrame.SizeScale_Y = 1;
			outerFrame.SizeOffset_Y = -20;
			AddChild(outerFrame);

			// Upper box contains icon and name.
			ISleekBox upperBox = Glazier.Get().CreateBox();
			upperBox.SizeScale_X = 1.0f;
			upperBox.SizeScale_Y = 0.4f;
			upperBox.SizeOffset_Y = -30;
			outerFrame.AddChild(upperBox);

			ISleekConstraintFrame iconFrame = Glazier.Get().CreateConstraintFrame();
			iconFrame.PositionOffset_X = 5;
			iconFrame.PositionOffset_Y = 5;
			iconFrame.SizeScale_X = 1f;
			iconFrame.SizeScale_Y = 1f;
			iconFrame.SizeOffset_Y = -70;
			iconFrame.Constraint = ESleekConstraint.FitInParent;
			upperBox.AddChild(iconFrame);

			iconImage = new SleekEconIcon();
			iconImage.SizeScale_X = 1f;
			iconImage.SizeScale_Y = 1f;
			iconFrame.AddChild(iconImage);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionScale_Y = 1.0f;
			nameLabel.PositionOffset_Y = -70;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.SizeOffset_Y = 70;
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameLabel.FontSize = ESleekFontSize.Large;
			upperBox.AddChild(nameLabel);

			actionsFrame = Glazier.Get().CreateFrame();
			actionsFrame.PositionOffset_Y = -25;
			actionsFrame.PositionScale_Y = 0.4f;
			actionsFrame.SizeOffset_Y = 100;
			actionsFrame.SizeScale_X = 1.0f;
			outerFrame.AddChild(actionsFrame);

			priceBox = new SleekItemStorePriceBox();
			priceBox.PositionScale_X = 0.75f;
			priceBox.SizeScale_X = 0.25f;
			priceBox.SizeOffset_Y = 50.0f;
			actionsFrame.AddChild(priceBox);

			addToCartButton = Glazier.Get().CreateButton();
			addToCartButton.SizeScale_X = 0.75f;
			addToCartButton.SizeOffset_Y = 50;
			addToCartButton.FontSize = ESleekFontSize.Medium;
			addToCartButton.Text = localization.format("AddToCart_Label");
			addToCartButton.TooltipText = localization.format("AddToCart_Tooltip");
			addToCartButton.OnClicked += OnClickedAddToCart;
			actionsFrame.AddChild(addToCartButton);

			removeFromCartButton = Glazier.Get().CreateButton();
			removeFromCartButton.SizeScale_X = 0.5f;
			removeFromCartButton.SizeOffset_Y = 50;
			removeFromCartButton.FontSize = ESleekFontSize.Medium;
			removeFromCartButton.Text = localization.format("RemoveFromCart_Label");
			removeFromCartButton.TooltipText = localization.format("RemoveFromCart_Tooltip");
			removeFromCartButton.OnClicked += OnClickedRemoveFromCart;
			actionsFrame.AddChild(removeFromCartButton);

			quantityField = Glazier.Get().CreateInt32Field();
			quantityField.PositionScale_X = 0.5f;
			quantityField.SizeScale_X = 0.25f;
			quantityField.SizeOffset_X = -25;
			quantityField.SizeOffset_Y = 50;
			quantityField.OnValueChanged += OnTypedQuantity;
			actionsFrame.AddChild(quantityField);

			incrementQuantityButton = Glazier.Get().CreateButton();
			incrementQuantityButton.PositionScale_X = 0.75f;
			incrementQuantityButton.PositionOffset_X = -25;
			incrementQuantityButton.SizeOffset_X = 25;
			incrementQuantityButton.SizeOffset_Y = 25;
			incrementQuantityButton.FontSize = ESleekFontSize.Medium;
			incrementQuantityButton.Text = "+";
			incrementQuantityButton.OnClicked += OnClickedIncrementQuantity;
			actionsFrame.AddChild(incrementQuantityButton);

			decrementQuantityButton = Glazier.Get().CreateButton();
			decrementQuantityButton.PositionScale_X = 0.75f;
			decrementQuantityButton.PositionOffset_X = -25;
			decrementQuantityButton.PositionOffset_Y = 25;
			decrementQuantityButton.SizeOffset_X = 25;
			decrementQuantityButton.SizeOffset_Y = 25;
			decrementQuantityButton.FontSize = ESleekFontSize.Medium;
			decrementQuantityButton.Text = "-";
			decrementQuantityButton.OnClicked += OnClickedDecrementQuantity;
			actionsFrame.AddChild(decrementQuantityButton);

			inspectButton = Glazier.Get().CreateButton();
			inspectButton.PositionOffset_Y = 55.0f;
			inspectButton.SizeScale_X = 1.0f;
			inspectButton.SizeOffset_Y = 50.0f;
			inspectButton.Text = MenuSurvivorsClothingItemUI.localization.format("Inspect_Text");
			inspectButton.TooltipText = MenuSurvivorsClothingItemUI.localization.format("Inspect_Tooltip");
			inspectButton.FontSize = ESleekFontSize.Medium;
			inspectButton.OnClicked += OnClickedInspect;
			actionsFrame.AddChild(inspectButton);

			inspectContainedItemsButton = Glazier.Get().CreateButton();
			inspectContainedItemsButton.PositionOffset_Y = 55.0f;
			inspectContainedItemsButton.SizeScale_X = 1.0f;
			inspectContainedItemsButton.SizeOffset_Y = 50.0f;
			inspectContainedItemsButton.Text = localization.format("InspectListedItems_Text");
			inspectContainedItemsButton.TooltipText = localization.format("InspectListedItems_Tooltip");
			inspectContainedItemsButton.FontSize = ESleekFontSize.Medium;
			inspectContainedItemsButton.OnClicked += OnClickedInspectContainedItems;
			actionsFrame.AddChild(inspectContainedItemsButton);

			// Lower frame contains description.
			lowerBox = Glazier.Get().CreateBox();
			lowerBox.PositionScale_Y = 0.4f;
			lowerBox.PositionOffset_Y = 30;
			lowerBox.SizeOffset_Y = -30;
			lowerBox.SizeScale_X = 1.0f;
			lowerBox.SizeScale_Y = 0.6f;
			outerFrame.AddChild(lowerBox);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 5;
			descriptionLabel.PositionOffset_Y = 5;
			descriptionLabel.SizeScale_X = 1.0f;
			descriptionLabel.SizeScale_Y = 1.0f;
			descriptionLabel.SizeOffset_X = -10;
			descriptionLabel.SizeOffset_Y = -10;
			descriptionLabel.TextAlignment = TextAnchor.UpperLeft;
			descriptionLabel.AllowRichText = true;
			descriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			lowerBox.AddChild(descriptionLabel);

			viewCartButton = Glazier.Get().CreateButton();
			viewCartButton.PositionOffset_Y = -110;
			viewCartButton.PositionScale_Y = 1.0f;
			viewCartButton.SizeOffset_X = 200;
			viewCartButton.SizeOffset_Y = 50;
			viewCartButton.Text = ItemStoreMenu.instance.localization.format("ViewCart_Label");
			viewCartButton.TooltipText = ItemStoreMenu.instance.localization.format("ViewCart_Tooltip");
			viewCartButton.OnClicked += OnClickedViewCartButton;
			viewCartButton.FontSize = ESleekFontSize.Medium;
			AddChild(viewCartButton);

			ISleekSprite viewCartImage = Glazier.Get().CreateSprite(ItemStoreMenu.instance.icons.load<Sprite>("Cart"));
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
		}

		private void RefreshQuantity()
		{
			priceBox.SetPrice(listing.basePrice, listing.currentPrice, quantityInCart);

			bool inCart = quantityInCart > 0;
			addToCartButton.IsVisible = !inCart;
			removeFromCartButton.IsVisible = inCart;
			quantityField.Value = quantityInCart;
			quantityField.IsVisible = inCart;
			incrementQuantityButton.IsVisible = inCart;
			decrementQuantityButton.IsVisible = inCart;

			viewCartButton.IsVisible = !ItemStore.Get().IsCartEmpty;
		}

		private void SetQuantityInCart(int value)
		{
			quantityInCart = value;
			ItemStore.Get().SetQuantityInCart(listing.itemdefid, quantityInCart);
			RefreshQuantity();
		}

		private void OnClickedAddToCart(ISleekElement button)
		{
			SetQuantityInCart(1);
		}

		private void OnClickedRemoveFromCart(ISleekElement button)
		{
			SetQuantityInCart(0);
		}

		private void OnTypedQuantity(ISleekInt32Field field, int value)
		{
			SetQuantityInCart(Mathf.Max(0, value));
		}

		private void OnClickedIncrementQuantity(ISleekElement button)
		{
			SetQuantityInCart(quantityInCart + 1);
		}

		private void OnClickedDecrementQuantity(ISleekElement button)
		{
			SetQuantityInCart(quantityInCart - 1);
		}

		private void OnClickedInspect(ISleekElement button)
		{
			MenuSurvivorsClothingInspectUI.viewItem(listing.itemdefid, 0);
			MenuSurvivorsClothingInspectUI.open(EMenuSurvivorsClothingInspectUIOpenContext.ItemStoreDetailsMenu);

			Close();
		}

		private void OnClickedInspectContainedItems(ISleekElement button)
		{
			ItemStoreBundleContentsMenu.instance.Open(listing);
			Close();
		}

		private void OnClickedViewCartButton(ISleekElement button)
		{
			ItemStoreCartMenu.instance.Open();
			Close();
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			ItemStoreMenu.instance.Open();
			Close();
		}

		private ItemStore.Listing listing;
		private int quantityInCart;

		private ISleekElement actionsFrame;
		private ISleekElement lowerBox;

		private ISleekLabel nameLabel;
		private ISleekLabel descriptionLabel;
		private SleekEconIcon iconImage;
		private SleekItemStorePriceBox priceBox;
		private ISleekButton addToCartButton;
		private ISleekButton removeFromCartButton;
		private ISleekInt32Field quantityField;
		private ISleekButton incrementQuantityButton;
		private ISleekButton decrementQuantityButton;
		private ISleekButton inspectButton;
		private ISleekButton inspectContainedItemsButton;

		/// <summary>
		/// Only visible when cart is not empty.
		/// </summary>
		private ISleekButton viewCartButton;
	}
}
