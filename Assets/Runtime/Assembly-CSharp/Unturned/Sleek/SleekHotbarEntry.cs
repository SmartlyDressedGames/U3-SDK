////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekHotbarEntry : SleekWrapper
	{
		public bool IsEquipped
		{
			get => _isEquipped;
			set
			{
				_isEquipped = value;
				if (!_isEquipped)
				{
					qualityLabel.IsVisible = false;
				}
				icon.color = new Color(1, 1, 1, _isEquipped ? 0.75f : 0.5f);
				hotkeyLabel.TextColor = new SleekColor(ESleekTint.FONT, _isEquipped ? 1.0f : 0.75f);
			}
		}
		private bool _isEquipped;

		public void UpdateItem(ItemJar jar)
		{
			itemJar = jar;

			ItemAsset newAsset = null;
			byte[] newState = null;
			if (jar != null && jar.item != null)
			{
				newAsset = jar.GetAsset();
				newState = jar.item.state;
			}
			displayQuality = -1;

			if (displayAsset != newAsset || displayState != newState)
			{
				displayAsset = newAsset;
				displayState = newState;

				IsVisible = displayAsset != null;
				doesItemHaveQuality = displayAsset?.showQuality ?? false;

				if (displayAsset != null)
				{
					SizeOffset_X = displayAsset.size_x * 25;
					SizeOffset_Y = displayAsset.size_y * 25;
					icon.Refresh(jar.item.id, jar.item.quality, jar.item.state, displayAsset);
				}
			}

			if (!doesItemHaveQuality)
			{
				qualityLabel.IsVisible = false;
			}

			UpdateQuality();
		}

		public void UpdateQuality()
		{
			if (!doesItemHaveQuality)
			{
				return;
			}

			qualityLabel.IsVisible = IsEquipped;

			int newQuality = -1;
			if (itemJar != null && itemJar.item != null)
			{
				newQuality = itemJar.item.quality;
			}

			if (displayQuality != newQuality)
			{
				displayQuality = newQuality;

				qualityLabel.TextColor = ItemTool.getQualityColor(displayQuality / 100.0f);
				qualityLabel.Text = $"{displayQuality}%";
			}
		}

		public SleekHotbarEntry(int hotbarIndex)
		{
			icon = new SleekItemIcon();
			icon.SizeScale_X = 1;
			icon.SizeScale_Y = 1;
			icon.color = new Color(1, 1, 1, 0.5f);
			AddChild(icon);

			hotkeyLabel = Glazier.Get().CreateLabel();
			hotkeyLabel.PositionOffset_X = -50;
			hotkeyLabel.PositionScale_X = 1f;
			hotkeyLabel.SizeOffset_X = 50;
			hotkeyLabel.SizeOffset_Y = 30;
			hotkeyLabel.Text = ControlsSettings.getEquipmentHotkeyText(hotbarIndex);
			hotkeyLabel.TextAlignment = TextAnchor.UpperRight;
			hotkeyLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.75f);
			hotkeyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(hotkeyLabel);

			qualityLabel = Glazier.Get().CreateLabel();
			qualityLabel.PositionOffset_X = -25;
			qualityLabel.PositionScale_X = 0.5f;
			qualityLabel.PositionScale_Y = 1f;
			qualityLabel.SizeOffset_X = 50;
			qualityLabel.SizeOffset_Y = 30;
			qualityLabel.TextAlignment = TextAnchor.UpperCenter;
			qualityLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(qualityLabel);
		}

		private SleekItemIcon icon;
		private ISleekLabel hotkeyLabel;
		private ISleekLabel qualityLabel;

		private ItemJar itemJar;
		private ItemAsset displayAsset;
		private byte[] displayState;
		private int displayQuality = -1;
		private bool doesItemHaveQuality;
	}
}
