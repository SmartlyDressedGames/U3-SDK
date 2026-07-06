////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Shows inspect buttons for each item mentioned in purchasable box or bundle's description text.
	/// </summary>
	internal class ItemStoreBundleContentsMenu : SleekFullscreenBox
	{
		public static ItemStoreBundleContentsMenu instance;

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
			containedItems = Provider.provider.economyService.GetBundleContents(listing.itemdefid);

			Color headerColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
			string headerName = Provider.provider.economyService.getInventoryName(listing.itemdefid);
			headerIconImage.SetItemDefId(listing.itemdefid);
			bool isBundle = Provider.provider.economyService.IsItemBundle(listing.itemdefid);
			headerName = $"<color={Palette.hex(headerColor)}>{headerName}</color>";
			string headerKey = isBundle ? "ListedItemsHeader_Bundle" : "ListedItemsHeader_Box";
			headerLabel.Text = ItemStoreMenu.instance.localization.format(headerKey, headerName, containedItems.Count);

			RefreshEntries();
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

		public ItemStoreBundleContentsMenu()
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

			ISleekBox headerBox = Glazier.Get().CreateBox();
			headerBox.SizeScale_X = 1.0f;
			headerBox.SizeOffset_X = -30;
			headerBox.SizeOffset_Y = 50;
			outerFrame.AddChild(headerBox);

			headerIconImage = new SleekEconIcon();
			headerIconImage.PositionOffset_X = 5;
			headerIconImage.PositionOffset_Y = 5;
			headerIconImage.SizeOffset_X = 40;
			headerIconImage.SizeOffset_Y = 40;
			headerBox.AddChild(headerIconImage);

			headerLabel = Glazier.Get().CreateLabel();
			headerLabel.PositionOffset_X = 50;
			headerLabel.SizeScale_X = 1.0f;
			headerLabel.SizeScale_Y = 1.0f;
			headerLabel.SizeOffset_X = -50;
			headerLabel.TextAlignment = TextAnchor.MiddleLeft;
			headerLabel.FontSize = ESleekFontSize.Medium;
			headerLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			headerLabel.AllowRichText = true;
			headerLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			headerBox.AddChild(headerLabel);

			scrollView = Glazier.Get().CreateScrollView();
			scrollView.PositionOffset_Y = 55;
			scrollView.SizeScale_X = 1.0f;
			scrollView.SizeScale_Y = 1.0f;
			scrollView.SizeOffset_Y = -55;
			scrollView.ScaleContentToWidth = true;
			scrollView.ReduceWidthWhenScrollbarVisible = false;
			outerFrame.AddChild(scrollView);

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

		private void RefreshEntries()
		{
			scrollView.RemoveAllChildren();

			int offset = 0;
			foreach (int itemdefid in containedItems)
			{
				SleekItemStoreBundleContentEntry item = new SleekItemStoreBundleContentEntry(itemdefid);
				item.SizeOffset_X = -30;
				item.SizeScale_X = 1.0f;
				item.SizeOffset_Y = 50;
				item.PositionOffset_Y = offset;
				offset += 55;
				scrollView.AddChild(item);
			}
			scrollView.ContentSizeOffset = new Vector2(0, offset - 5);
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			ItemStoreDetailsMenu.instance.OpenCurrentListing();
			Close();
		}

		private ItemStore.Listing listing;
		private List<int> containedItems;

		private ISleekBox headerBox;
		private SleekEconIcon headerIconImage;
		private ISleekLabel headerLabel;

		private ISleekScrollView scrollView;
	}
}
