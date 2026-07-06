////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekSelectedBlueprintItem : SleekWrapper
	{
		internal BlueprintStatus blueprintStatus;

		internal void SetInputItem(BlueprintSupply config, BlueprintInputItemStatus status, int index)
		{
			ItemAsset itemAsset = config.FindItemAsset();

			PlayerInventorySearchResultV2? itemResult = null;
			if (status.searchResults.Count > 0)
			{
				itemResult = status.searchResults[0];
			}
			byte quality;
			byte[] state;
			Item item = itemResult?.Jar?.item ?? null;
			if (item != null)
			{
				quality = item.quality;
				state = item.state;
			}
			else
			{
				quality = 100;
				state = itemAsset.getState(false);
			}
			SetItemAsset(itemAsset, quality, state);

			descriptionLabel.IsVisible = true;
			descriptionLabel.TextColor = ESleekTint.FONT;
			if (blueprintStatus.blueprint.Operation == EBlueprintOperation.FillTargetItem && index == 0)
			{
				descriptionLabel.AllowRichText = false;
				descriptionLabel.Text = $"x{status.totalAmount}";
			}
			else
			{
				Local localization = PlayerDashboardCraftingUI.localization;
				if (status.isMissingRequiredAmount)
				{
					int missingAmount = config.amount - status.totalAmount;
					string missingText = localization.format("MissingAmount", missingAmount);
					missingText = RichTextUtil.wrapWithColor(missingText, OptionsSettings.badColor);
					descriptionLabel.AllowRichText = true;
					descriptionLabel.Text = localization.format("BlueprintAmountLabel_Missing",
						status.totalAmount, config.amount, missingText);
				}
				else
				{
					descriptionLabel.AllowRichText = false;
					descriptionLabel.Text = localization.format("BlueprintAmountLabel",
						status.totalAmount, config.amount);
				}
			}

			nameLabel.SizeOffset_Y = descriptionLabel.IsVisible ? 30 : 50;
		}

		internal void SetOutputItem(BlueprintStatus blueprintStatus, BlueprintOutput output, int outputIndex)
		{
			ItemAsset itemAsset = output.FindItemAsset();

			byte quality;
			byte[] state;
			if (blueprintStatus.blueprint.transferState)
			{
				blueprintStatus.GetPreviewOutputTransferState(itemAsset, out quality, out state);
			}
			else
			{
				quality = 100;
				state = itemAsset.getState();
			}
			SetItemAsset(itemAsset, quality, state);

			if (output.amount > 1 || quality != 100)
			{
				string text = string.Empty;
				if (output.amount > 1)
				{
					text = $"x{output.amount}";
				}
				if (quality != 100)
				{
					if (text.Length > 0)
					{
						text += " ";
					}
					Color qualityColor = ItemTool.getQualityColor(quality / 100.0f);
					text += $"<color={Palette.hex(qualityColor)}>{quality}%</color>";
				}

				descriptionLabel.IsVisible = true;
				descriptionLabel.AllowRichText = true;
				descriptionLabel.Text = text;
				descriptionLabel.TextColor = ESleekTint.FONT;
			}
			else
			{
				descriptionLabel.IsVisible = false;
			}

			nameLabel.SizeScale_Y = descriptionLabel.IsVisible ? 0.5f : 1.0f;
		}

		private void SetItemAsset(ItemAsset itemAsset, byte quality, byte[] state)
		{
			if (currentAsset != itemAsset)
			{
				currentAsset = itemAsset;
				itemImage.Clear();
			}

			itemImage.Refresh(itemAsset.id, quality, state, itemAsset, Mathf.RoundToInt(itemImage.SizeOffset_X), Mathf.RoundToInt(itemImage.SizeOffset_Y));

			Color rarityColor = ItemTool.getRarityColorUI(itemAsset.rarity);
			nameLabel.TextColor = rarityColor;
			nameLabel.Text = itemAsset.itemName;

			string rarityDesc = PlayerDashboardInventoryUI.localization.format("Rarity_" + (int) itemAsset.rarity);
			string typeDesc = PlayerDashboardInventoryUI.localization.format("Type_" + (int) itemAsset.type);
			string rarityTypeText = PlayerDashboardInventoryUI.localization.format("Rarity_Type_Label", rarityDesc, typeDesc);
			backgroundBox.TooltipText = $"<color={Palette.hex(rarityColor)}>{rarityTypeText}</color>\n{itemAsset.itemDescription}";
		}

		public SleekSelectedBlueprintItem()
		{
			// Nelson 2025-04-28: initially this resized according to item size, but now we're trying a constant square
			// icon size to help the menu flow nicer.
			SizeOffset_Y = 50;

			backgroundBox = Glazier.Get().CreateBox();
			backgroundBox.SizeScale_X = 1f;
			backgroundBox.SizeScale_Y = 1f;
			backgroundBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			backgroundBox.AllowRichText = true;
			AddChild(backgroundBox);

			itemImage = new SleekItemIcon();
			itemImage.PositionOffset_X = 5;
			itemImage.PositionOffset_Y = 5;
			itemImage.SizeOffset_X = 40;
			itemImage.SizeOffset_Y = 40;
			AddChild(itemImage);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 50;
			nameLabel.SizeScale_X = 1f;
			nameLabel.SizeOffset_X = -50;
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			AddChild(nameLabel);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 50;
			descriptionLabel.PositionOffset_Y = 20;
			descriptionLabel.SizeScale_X = 1f;
			descriptionLabel.SizeOffset_X = -50;
			descriptionLabel.SizeOffset_Y = 30;
			descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			descriptionLabel.TextAlignment = TextAnchor.MiddleLeft;
			AddChild(descriptionLabel);
		}

		private ItemAsset currentAsset;
		private ISleekBox backgroundBox;
		private SleekItemIcon itemImage;
		private ISleekLabel nameLabel;
		private ISleekLabel descriptionLabel;
	}
}
