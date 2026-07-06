////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class ItemStoreCartMenu : SleekFullscreenBox
	{
		public static ItemStoreCartMenu instance;

		public bool IsOpen
		{
			get;
			private set;
		}

		public void Open()
		{
			IsOpen = true;
			AnimateIntoView();

			RefreshCartEntries();
			ItemStore.Get().OnCartChanged += OnCartChanged;
		}

		public void Close()
		{
			if (!IsOpen)
				return;

			IsOpen = false;
			AnimateOutOfView(0.0f, 1.0f);
			ItemStore.Get().OnCartChanged -= OnCartChanged;
		}

		public override void OnDestroy()
		{
			ItemStore.Get().OnCartChanged -= OnCartChanged;
			base.OnDestroy();
		}

		public ItemStoreCartMenu()
		{
			instance = this;

			Local localization = ItemStoreMenu.instance.localization;

			PositionScale_Y = 1.0f;
			PositionOffset_X = 10;
			PositionOffset_Y = 10;
			SizeOffset_X = -20;
			SizeOffset_Y = -20;
			SizeScale_X = 1.0f;
			SizeScale_Y = 1.0f;

			ISleekElement outerFrame = Glazier.Get().CreateFrame();
			outerFrame.PositionScale_X = 0.5f;
			outerFrame.PositionOffset_Y = 10;
			outerFrame.SizeScale_X = 0.5f;
			outerFrame.SizeScale_Y = 1;
			outerFrame.SizeOffset_Y = -20;
			AddChild(outerFrame);

			scrollView = Glazier.Get().CreateScrollView();
			scrollView.SizeScale_X = 1.0f;
			scrollView.SizeScale_Y = 1.0f;
			scrollView.SizeOffset_Y = -110;
			scrollView.ScaleContentToWidth = true;
			scrollView.ReduceWidthWhenScrollbarVisible = false;
			outerFrame.AddChild(scrollView);

			ISleekElement bottomFrame = Glazier.Get().CreateFrame();
			bottomFrame.SizeScale_X = 1.0f;
			bottomFrame.SizeOffset_X = -30;
			bottomFrame.SizeOffset_Y = 105;
			bottomFrame.PositionScale_Y = 1.0f;
			bottomFrame.PositionOffset_Y = -105;
			outerFrame.AddChild(bottomFrame);

			totalPriceBox = new SleekItemStorePriceBox();
			totalPriceBox.PositionScale_X = 0.8f;
			totalPriceBox.SizeScale_X = 0.2f;
			totalPriceBox.SizeOffset_Y = 50;
			bottomFrame.AddChild(totalPriceBox);

			ISleekLabel totalPriceLabel = Glazier.Get().CreateLabel();
			totalPriceLabel.PositionOffset_X = -5;
			totalPriceLabel.SizeScale_X = 0.8f;
			totalPriceLabel.SizeOffset_X = -5;
			totalPriceLabel.SizeOffset_Y = 50;
			totalPriceLabel.FontSize = ESleekFontSize.Medium;
			totalPriceLabel.TextAlignment = TextAnchor.MiddleRight;
			totalPriceLabel.Text = localization.format("TotalPrice_Label");
			totalPriceLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			bottomFrame.AddChild(totalPriceLabel);

			startPurchaseButton = Glazier.Get().CreateButton();
			startPurchaseButton.PositionOffset_Y = 55;
			startPurchaseButton.SizeScale_X = 1.0f;
			startPurchaseButton.SizeOffset_Y = 50;
			startPurchaseButton.FontSize = ESleekFontSize.Medium;
			startPurchaseButton.Text = localization.format("StartPurchase_Label");
			startPurchaseButton.TooltipText = localization.format("StartPurchase_Tooltip");
			startPurchaseButton.OnClicked += OnClickedStartPurchase;
			bottomFrame.AddChild(startPurchaseButton);

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

		private void RefreshCartEntries()
		{
			scrollView.RemoveAllChildren();

			int offset = 0;
			entries.Clear();
			foreach (ItemStore.CartEntry cartEntry in ItemStore.Get().GetCart())
			{
				ItemStore.Listing listing;
				if (!ItemStore.Get().FindListing(cartEntry.itemdefid, out listing))
				{
					UnturnedLog.warn("Item store itemdefid {0} x{1} in cart without listing", cartEntry.itemdefid, cartEntry.quantity);
					continue;
				}

				SleekItemStoreCartEntry item = new SleekItemStoreCartEntry(cartEntry, listing);
				item.SizeOffset_X = -30;
				item.SizeScale_X = 1.0f;
				item.SizeOffset_Y = 50;
				item.PositionOffset_Y = offset;
				offset += 55;
				scrollView.AddChild(item);
				entries.Add(item);
			}
			scrollView.ContentSizeOffset = new Vector2(0, offset - 5);

			OnCartChanged();
		}

		private void OnCartChanged()
		{
			ulong totalBasePrice = 0;
			ulong totalCurrentPrice = 0;
			foreach (SleekItemStoreCartEntry item in entries)
			{
				ulong basePrice;
				ulong currentPrice;
				item.GetTotalPrice(out basePrice, out currentPrice);
				totalBasePrice += basePrice;
				totalCurrentPrice += currentPrice;
			}
			totalPriceBox.SetPrice(totalBasePrice, totalCurrentPrice, 1);

			// Cart can be empty if removed while on this page.
			startPurchaseButton.IsClickable = !ItemStore.Get().IsCartEmpty;
		}

		private void OnClickedStartPurchase(ISleekElement button)
		{
			MenuSurvivorsClothingUI.open();
			Close();
			ItemStore.Get().StartPurchase();
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			ItemStoreMenu.instance.Open();
			Close();
		}

		private ISleekScrollView scrollView;
		private List<SleekItemStoreCartEntry> entries = new List<SleekItemStoreCartEntry>();
		private SleekItemStorePriceBox totalPriceBox;
		private ISleekButton startPurchaseButton;
	}
}
