////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class SleekItemStoreListing : SleekWrapper
	{
		public bool canShowAsInCart = true;

		public SleekItemStoreListing()
		{
			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.OnClicked += OnClickedButton;
			AddChild(button);

			ISleekConstraintFrame iconFrame = Glazier.Get().CreateConstraintFrame();
			iconFrame.PositionOffset_X = 5;
			iconFrame.PositionOffset_Y = 5;
			iconFrame.SizeScale_X = 1f;
			iconFrame.SizeScale_Y = 1f;
			iconFrame.SizeOffset_X = -10;
			iconFrame.SizeOffset_Y = -50;
			iconFrame.Constraint = ESleekConstraint.FitInParent;
			AddChild(iconFrame);

			iconImage = new SleekEconIcon();
			iconImage.SizeScale_X = 1.0f;
			iconImage.SizeScale_Y = 1.0f;
			iconFrame.AddChild(iconImage);

			nameAndPriceLabel = Glazier.Get().CreateLabel();
			nameAndPriceLabel.PositionScale_Y = 1.0f;
			nameAndPriceLabel.PositionOffset_Y = -50;
			nameAndPriceLabel.SizeScale_X = 1.0f;
			nameAndPriceLabel.SizeOffset_Y = 50;
			nameAndPriceLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameAndPriceLabel.TextAlignment = TextAnchor.LowerLeft;
			nameAndPriceLabel.AllowRichText = true;
			AddChild(nameAndPriceLabel);

			cartImage = Glazier.Get().CreateSprite(ItemStoreMenu.instance.icons.load<Sprite>("Cart"));
			cartImage.PositionOffset_X = 5;
			cartImage.PositionOffset_Y = 5;
			cartImage.SizeOffset_X = 20;
			cartImage.SizeOffset_Y = 20;
			cartImage.DrawMethod = ESleekSpriteType.Regular;
			cartImage.TintColor = ESleekTint.FOREGROUND;
			AddChild(cartImage);

			stampLabel = Glazier.Get().CreateLabel();
			stampLabel.SizeScale_X = 1.0f;
			stampLabel.SizeOffset_Y = 50;
			stampLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			stampLabel.TextAlignment = TextAnchor.UpperRight;
			stampLabel.TextColor = Color.green;
			AddChild(stampLabel);
		}

		public void RefreshInCart()
		{
			cartImage.IsVisible = canShowAsInCart && button.IsClickable && (ItemStore.Get().GetQuantityInCart(listing.itemdefid) > 0);
		}

		public void SetListing(ItemStore.Listing listing)
		{
			button.IsClickable = true;
			iconImage.IsVisible = true;
			nameAndPriceLabel.IsVisible = true;

			this.listing = listing;
			Color itemColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
			string itemName = Provider.provider.economyService.getInventoryName(listing.itemdefid);
			string priceText = ItemStore.Get().FormatPrice(listing.currentPrice);

			nameAndPriceLabel.Text = RichTextUtil.wrapWithColor(itemName, itemColor) + '\n' + RichTextUtil.wrapWithColor(priceText, ItemStore.PremiumColor);
			nameAndPriceLabel.TextColor = itemColor;
			button.BackgroundColor = SleekColor.BackgroundIfLight(itemColor);
			button.TextColor = itemColor;
			button.TooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);

			iconImage.SetItemDefId(listing.itemdefid);

			if (listing.isNew && !ItemStoreSavedata.WasNewListingSeen(listing.itemdefid))
			{
				hasNewLabel = true;
				stampLabel.Text = Provider.localization.format("New");
				stampLabel.IsVisible = true;
			}
			else if (listing.currentPrice < listing.basePrice)
			{
				hasNewLabel = false;
				stampLabel.Text = MenuSurvivorsClothingUI.localization.format("Itemstore_Sale")
					+ '\n' + ItemStore.Get().FormatDiscount(listing.currentPrice, listing.basePrice);
				stampLabel.IsVisible = true;
			}
			else
			{
				hasNewLabel = false;
				stampLabel.IsVisible = false;
			}

			RefreshInCart();
		}

		public void ClearListing()
		{
			button.IsClickable = false;
			iconImage.IsVisible = false;
			nameAndPriceLabel.IsVisible = false;
			cartImage.IsVisible = false;
			button.TooltipText = null;
			stampLabel.IsVisible = false;
		}

		private void OnClickedButton(ISleekElement button)
		{
			if (hasNewLabel)
			{
				ItemStoreSavedata.MarkNewListingSeen(listing.itemdefid);
				stampLabel.IsVisible = false;
			}
			ItemStore.Get().ViewItem(listing.itemdefid);
		}

		private ItemStore.Listing listing;
		private bool hasNewLabel;

		private ISleekButton button;
		private SleekEconIcon iconImage;
		private ISleekLabel nameAndPriceLabel;
		/// <summary>
		/// Icon visible when this listing is in the cart.
		/// </summary>
		private ISleekSprite cartImage;
		/// <summary>
		/// "SALE" or "NEW" text visible when applicable.
		/// </summary>
		private ISleekLabel stampLabel;
	}
}
