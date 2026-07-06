////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Displays a single random item. Placed under the other main menu buttons.
	/// </summary>
	internal class SleekItemStoreMainMenuButton : SleekWrapper
	{
		public enum ELabelType
		{
			None,
			New,
			Sale,
		}

		public SleekItemStoreMainMenuButton(ItemStore.Listing listing, ELabelType labelType)
		{
			this.listing = listing;

			Color itemColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);

			ISleekButton itemButton = Glazier.Get().CreateButton();
			itemButton.SizeScale_X = 1.0f;
			itemButton.SizeScale_Y = 1.0f;
			itemButton.OnClicked += OnClickedItemButton;
			itemButton.TextColor = itemColor; // Tooltip color.
			itemButton.TooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
			itemButton.BackgroundColor = SleekColor.BackgroundIfLight(itemColor);
			AddChild(itemButton);

			SleekEconIcon iconImage = new SleekEconIcon();
			iconImage.PositionOffset_X = 5;
			iconImage.PositionOffset_Y = 5;
			iconImage.SizeOffset_X = 40;
			iconImage.SizeOffset_Y = 40;
			iconImage.SetItemDefId(listing.itemdefid);
			itemButton.AddChild(iconImage);

			ISleekLabel nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 50;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.SizeScale_Y = 1.0f;
			nameLabel.SizeOffset_X = -50;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.FontSize = ESleekFontSize.Medium;
			nameLabel.Text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
			nameLabel.TextColor = itemColor;
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			itemButton.AddChild(nameLabel);

			ISleekLabel priceLabel = Glazier.Get().CreateLabel();
			priceLabel.SizeScale_X = 1.0f;
			priceLabel.SizeScale_Y = 1.0f;
			priceLabel.TextAlignment = TextAnchor.LowerRight;
			priceLabel.TextColor = ItemStore.PremiumColor;
			priceLabel.Text = ItemStore.Get().FormatPrice(listing.currentPrice);
			priceLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			itemButton.AddChild(priceLabel);

			if (labelType != ELabelType.None)
			{
				ISleekLabel extraLabel = Glazier.Get().CreateLabel();
				extraLabel.SizeScale_X = 1.0f;
				extraLabel.SizeScale_Y = 1.0f;
				extraLabel.TextAlignment = TextAnchor.UpperRight;
				extraLabel.TextColor = Color.green;
				extraLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				itemButton.AddChild(extraLabel);

				switch (labelType)
				{
					case ELabelType.New:
						hasNewLabel = true;
						extraLabel.Text = Provider.localization.format("New");
						break;

					case ELabelType.Sale:
						extraLabel.Text = MenuSurvivorsClothingUI.localization.format("Itemstore_Sale")
							+ '\n' + ItemStore.Get().FormatDiscount(listing.currentPrice, listing.basePrice);
						break;
				}
			}
		}

		private void OnClickedItemButton(ISleekElement button)
		{
			if (hasNewLabel)
			{
				ItemStoreSavedata.MarkNewListingSeen(listing.itemdefid);
			}
			ItemStore.Get().ViewItem(listing.itemdefid);
			IsVisible = false; // Dismiss to avoid annoying player.
		}

		private ItemStore.Listing listing;
		private bool hasNewLabel;
	}
}
