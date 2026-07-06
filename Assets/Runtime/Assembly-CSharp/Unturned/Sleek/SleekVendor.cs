////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekVendor : SleekWrapper
	{
		public event ClickedButton onClickedButton;

		protected string formatCost(uint value)
		{
			if (element.outerAsset.currency.isValid)
			{
				ItemCurrencyAsset asset = element.outerAsset.currency.Find();
				if (asset != null && string.IsNullOrEmpty(asset.valueFormat) == false)
				{
					return string.Format(asset.valueFormat, value);
				}
			}

			return value.ToString();
		}

		public void updateAmount()
		{
			if (element == null || amountLabel == null)
			{
				return;
			}

			if (element is VendorBuying buying)
			{
				ushort total;
				byte amount;
				buying.format(Player.LocalPlayer, out total, out amount);

				button.IsClickable = total >= amount;
				amountLabel.Text = PlayerNPCVendorUI.localization.format("Amount_Buy", total, amount);
			}
			else if (element is VendorSellingBase selling)
			{
				ushort total;
				selling.format(Player.LocalPlayer, out total);

				button.IsClickable = selling.canBuy(Player.LocalPlayer);
				amountLabel.Text = PlayerNPCVendorUI.localization.format("Amount_Sell", total);
			}
			amountLabel.TextColor = button.IsClickable ? ESleekTint.FONT : ESleekTint.BAD;
		}

		public SleekVendor(VendorElement newElement) : base()
		{
			element = newElement;

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.OnClicked += onClickedInternalButton;
			AddChild(button);

			float imagePadding = 0;
			SizeOffset_Y = 60;
			if (element.hasIcon)
			{
#pragma warning disable
				ItemAsset itemAsset = Assets.FindItemByGuidOrLegacyId<ItemAsset>(element.TargetAssetGuid, element.id);
#pragma warning restore

				if (itemAsset != null)
				{
					SleekItemIcon itemImage = new SleekItemIcon();
					itemImage.PositionOffset_X = 5;
					itemImage.PositionOffset_Y = 5;

					if (itemAsset.size_y == 1)
					{
						itemImage.SizeOffset_X = itemAsset.size_x * 100;
						itemImage.SizeOffset_Y = itemAsset.size_y * 100;
					}
					else
					{
						itemImage.SizeOffset_X = itemAsset.size_x * 50;
						itemImage.SizeOffset_Y = itemAsset.size_y * 50;
					}
					imagePadding = itemImage.PositionOffset_X + itemImage.SizeOffset_X;

					AddChild(itemImage);

					byte[] state = null;
					if (itemAsset is ItemGunAsset gunAsset && element is VendorSellingItem sellingItem)
					{
						state = sellingItem.GetGunStateOverride(gunAsset);
					}
					if (state == null)
					{
						state = itemAsset.getState(false);
					}

					itemImage.Refresh(itemAsset.id, 100, state, itemAsset, Mathf.RoundToInt(itemImage.SizeOffset_X), Mathf.RoundToInt(itemImage.SizeOffset_Y));

					SizeOffset_Y = itemImage.SizeOffset_Y + 10;
				}
			}
			else
			{
				if (element is VendorSellingVehicle sellingVehicle)
				{
					Color32? color = sellingVehicle.paintColor;
					if (!color.HasValue)
					{
						Asset asset = sellingVehicle.FindAsset();
						if (asset is VehicleRedirectorAsset redirectorAsset)
						{
							color = redirectorAsset.SpawnPaintColor;
						}
					}

					if (color.HasValue)
					{
						ISleekImage colorSwatch = Glazier.Get().CreateImage();
						colorSwatch.PositionOffset_X = 10;
						colorSwatch.PositionOffset_Y = 10;
						colorSwatch.SizeOffset_X = 20;
						colorSwatch.SizeOffset_Y = 40;
						colorSwatch.Texture = GlazierResources.PixelTexture;
						colorSwatch.TintColor = color.Value;
						AddChild(colorSwatch);
						imagePadding = colorSwatch.PositionOffset_X + colorSwatch.SizeOffset_X;
					}
				}
			}

			string name = element.displayName;
			if (!string.IsNullOrEmpty(name))
			{
				ISleekLabel nameLabel = Glazier.Get().CreateLabel();
				nameLabel.PositionOffset_X = imagePadding + 5;
				nameLabel.PositionOffset_Y = 5;
				nameLabel.SizeOffset_X = -imagePadding - 10;
				nameLabel.SizeOffset_Y = 30;
				nameLabel.SizeScale_X = 1;
				nameLabel.Text = name;
				nameLabel.FontSize = ESleekFontSize.Medium;
				nameLabel.TextAlignment = TextAnchor.UpperLeft;
				nameLabel.TextColor = ItemTool.getRarityColorUI(element.rarity);
				nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				AddChild(nameLabel);
			}

			string desc = element.displayDesc;
			if (!string.IsNullOrEmpty(desc))
			{
				ISleekLabel descriptionLabel = Glazier.Get().CreateLabel();
				descriptionLabel.PositionOffset_X = imagePadding + 5;
				descriptionLabel.PositionOffset_Y = 25;
				descriptionLabel.SizeOffset_X = -imagePadding - 10;
				descriptionLabel.SizeOffset_Y = -30;
				descriptionLabel.SizeScale_X = 1;
				descriptionLabel.SizeScale_Y = 1;
				descriptionLabel.TextAlignment = TextAnchor.UpperLeft;
				descriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				descriptionLabel.AllowRichText = true;
				descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				descriptionLabel.Text = desc;
				AddChild(descriptionLabel);
			}

			ISleekLabel costLabel = Glazier.Get().CreateLabel();
			costLabel.PositionOffset_X = imagePadding + 5;
			costLabel.PositionOffset_Y = -35;
			costLabel.PositionScale_Y = 1;
			costLabel.SizeOffset_X = -imagePadding - 10;
			costLabel.SizeOffset_Y = 30;
			costLabel.SizeScale_X = 1;
			costLabel.TextAlignment = TextAnchor.LowerRight;
			AddChild(costLabel);

			if (element is VendorBuying)
			{
				costLabel.Text = PlayerNPCVendorUI.localization.format("Price", formatCost(element.cost));
			}
			else
			{
				costLabel.Text = PlayerNPCVendorUI.localization.format("Cost", formatCost(element.cost));
			}

			amountLabel = Glazier.Get().CreateLabel();
			amountLabel.PositionOffset_X = imagePadding + 5;
			amountLabel.PositionOffset_Y = -35;
			amountLabel.PositionScale_Y = 1;
			amountLabel.SizeOffset_X = -imagePadding - 10;
			amountLabel.SizeOffset_Y = 30;
			amountLabel.SizeScale_X = 1;
			amountLabel.TextAlignment = TextAnchor.LowerLeft;
			AddChild(amountLabel);

			updateAmount();
		}

		private void onClickedInternalButton(ISleekElement internalButton)
		{
			onClickedButton?.Invoke(this);
		}

		private VendorElement element;
		private ISleekButton button;
		private ISleekLabel amountLabel;
	}
}
