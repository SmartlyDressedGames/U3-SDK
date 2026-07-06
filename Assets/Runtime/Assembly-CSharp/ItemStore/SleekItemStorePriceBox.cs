////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class SleekItemStorePriceBox : SleekWrapper
	{
		public void SetPrice(ulong basePrice, ulong currentPrice, int quantity)
		{
			uint quantityForPrice = (uint) Mathf.Max(quantity, 1);
			if (currentPrice == basePrice)
			{
				basePriceLabel.IsVisible = false;
				discountStrikethrough.IsVisible = false;
				percentageLabel.IsVisible = false;

				currentPriceLabel.PositionScale_X = 0.0f;
				currentPriceLabel.PositionScale_Y = 0.0f;
				currentPriceLabel.SizeScale_X = 1.0f;
				currentPriceLabel.SizeScale_Y = 1.0f;

				currentPriceLabel.Text = ItemStore.Get().FormatPrice(currentPrice * quantityForPrice);

				// Tooltip in case text is truncated on small screens.
				if (quantity > 1)
				{
					backdropBox.TooltipText = string.Format("{0} x {1} = {2}",
						ItemStore.Get().FormatPrice(currentPrice),
						quantity,
						currentPriceLabel.Text);
				}
				else
				{
					backdropBox.TooltipText = currentPriceLabel.Text;
				}
			}
			else
			{
				basePriceLabel.IsVisible = true;
				discountStrikethrough.IsVisible = true;
				percentageLabel.IsVisible = true;

				currentPriceLabel.PositionScale_X = 0.5f;
				currentPriceLabel.PositionScale_Y = 0.5f;
				currentPriceLabel.SizeScale_X = 0.5f;
				currentPriceLabel.SizeScale_Y = 0.5f;

				ulong totalBasePrice = basePrice * quantityForPrice;
				ulong totalCurrentPrice = currentPrice * quantityForPrice;
				basePriceLabel.Text = ItemStore.Get().FormatPrice(totalBasePrice);
				currentPriceLabel.Text = ItemStore.Get().FormatPrice(totalCurrentPrice);
				percentageLabel.Text = ItemStore.Get().FormatDiscount(totalCurrentPrice, totalBasePrice);

				// Tooltip in case text is truncated on small screens.
				if (quantity > 1)
				{
					string baseTooltipText = string.Format("{0} x {1} = {2}",
						ItemStore.Get().FormatPrice(basePrice),
						quantity,
						basePriceLabel.Text);
					string currentTooltipText = string.Format("{0} x {1} = {2}",
						ItemStore.Get().FormatPrice(currentPrice),
						quantity,
						currentPriceLabel.Text);
					backdropBox.TooltipText = RichTextUtil.wrapWithColor(baseTooltipText, Color.gray)
						+ '\n' + RichTextUtil.wrapWithColor(percentageLabel.Text, Color.green) + '\n'
						+ RichTextUtil.wrapWithColor(currentTooltipText, ItemStore.PremiumColor);
				}
				else
				{
					backdropBox.TooltipText = RichTextUtil.wrapWithColor(basePriceLabel.Text, Color.gray)
						+ '\n' + RichTextUtil.wrapWithColor(percentageLabel.Text, Color.green) + '\n'
						+ RichTextUtil.wrapWithColor(currentPriceLabel.Text, ItemStore.PremiumColor);
				}
			}
		}

		public SleekItemStorePriceBox()
		{
			backdropBox = Glazier.Get().CreateBox();
			backdropBox.SizeScale_X = 1.0f;
			backdropBox.SizeScale_Y = 1.0f;
			backdropBox.TextColor = ItemStore.PremiumColor; // Tooltip color.
			AddChild(backdropBox);

			basePriceLabel = Glazier.Get().CreateLabel();
			basePriceLabel.PositionScale_X = 0.5f;
			basePriceLabel.SizeScale_X = 0.5f;
			basePriceLabel.SizeScale_Y = 0.5f;
			basePriceLabel.FontSize = ESleekFontSize.Medium;
			basePriceLabel.TextColor = Color.gray;
			AddChild(basePriceLabel);

			discountStrikethrough = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			discountStrikethrough.PositionScale_X = 0.5f;
			discountStrikethrough.PositionScale_Y = 0.25f;
			discountStrikethrough.PositionOffset_Y = -1;
			discountStrikethrough.SizeOffset_Y = 1;
			discountStrikethrough.SizeScale_X = 0.5f;
			discountStrikethrough.CanRotate = true;
			discountStrikethrough.RotationAngle = -15.0f;
			discountStrikethrough.TintColor = Palette.COLOR_R;
			AddChild(discountStrikethrough);

			currentPriceLabel = Glazier.Get().CreateLabel();
			currentPriceLabel.SizeScale_X = 1.0f;
			currentPriceLabel.FontSize = ESleekFontSize.Medium;
			currentPriceLabel.TextColor = ItemStore.PremiumColor;
			AddChild(currentPriceLabel);

			percentageLabel = Glazier.Get().CreateLabel();
			percentageLabel.SizeScale_X = 0.5f;
			percentageLabel.SizeScale_Y = 1.0f;
			percentageLabel.FontSize = ESleekFontSize.Medium;
			percentageLabel.TextColor = Color.green;
			AddChild(percentageLabel);
		}

		private ISleekBox backdropBox;
		private ISleekLabel basePriceLabel;
		private ISleekLabel currentPriceLabel;
		private ISleekImage discountStrikethrough;
		private ISleekLabel percentageLabel;
	}
}
