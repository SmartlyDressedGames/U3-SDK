////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class SleekItemStoreBundleContentEntry : SleekWrapper
	{
		public SleekItemStoreBundleContentEntry(int itemdefid)
		{
			Local localization = ItemStoreMenu.instance.localization;

			this.itemdefid = itemdefid;
			Color itemColor = Provider.provider.economyService.getInventoryColor(itemdefid);

			itemButton = Glazier.Get().CreateButton();
			itemButton.SizeScale_X = 1.0f;
			itemButton.SizeScale_Y = 1.0f;
			itemButton.OnClicked += OnClickedItemButton;
			itemButton.TooltipText = Provider.provider.economyService.getInventoryType(itemdefid);
			itemButton.TextColor = itemColor; // Tooltip color.
			AddChild(itemButton);

			iconImage = new SleekEconIcon();
			iconImage.PositionOffset_X = 5;
			iconImage.PositionOffset_Y = 5;
			iconImage.SizeOffset_X = 40;
			iconImage.SizeOffset_Y = 40;
			iconImage.SetItemDefId(itemdefid);
			itemButton.AddChild(iconImage);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 50;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.SizeScale_Y = 1.0f;
			nameLabel.SizeOffset_X = -50;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.FontSize = ESleekFontSize.Medium;
			nameLabel.Text = Provider.provider.economyService.getInventoryName(itemdefid);
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameLabel.TextColor = itemColor;
			itemButton.AddChild(nameLabel);
		}

		private void OnClickedItemButton(ISleekElement button)
		{
			MenuSurvivorsClothingInspectUI.viewItem(itemdefid, 0);
			MenuSurvivorsClothingInspectUI.open(EMenuSurvivorsClothingInspectUIOpenContext.ItemStoreBundleContents);

			ItemStoreBundleContentsMenu.instance.Close();
		}

		private int itemdefid;

		private ISleekButton itemButton;
		private SleekEconIcon iconImage;
		private ISleekLabel nameLabel;
	}
}
