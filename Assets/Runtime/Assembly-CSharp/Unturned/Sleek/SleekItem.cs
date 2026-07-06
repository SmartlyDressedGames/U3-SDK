////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedItem(SleekItem item);
	public delegate void DraggedItem(SleekItem item);

	public class SleekItem : SleekWrapper
	{
		private ItemJar _jar;
		public ItemJar jar => _jar;

		private byte _hotkey = 255;
		public int hotkey => _hotkey;

		private ISleekButton button;
		private SleekItemIcon icon;
		private ISleekLabel amountLabel;
		private ISleekImage qualityImage;
		private ISleekLabel hotkeyLabel;

		//		public Texture image
		//		{
		//			get 
		//			{
		//				return icon.texture;
		//			}
		//		}

		public ClickedItem onClickedItem;
		public DraggedItem onDraggedItem;

		private bool isTemporary;

		public void enable()
		{
			ValidateNotDestroyed();
			button.IsRaycastTarget = true;

			SleekColor buttoncolor = button.BackgroundColor;
			buttoncolor.SetAlpha(1.0f);
			button.BackgroundColor = buttoncolor;

			SleekColor iconcolor = icon.color;
			iconcolor.SetAlpha(1.0f);
			icon.color = iconcolor;
		}

		public void disable()
		{
			ValidateNotDestroyed();
			button.IsRaycastTarget = false;

			SleekColor buttoncolor = button.BackgroundColor;
			buttoncolor.SetAlpha(0.5f);
			button.BackgroundColor = buttoncolor;

			SleekColor iconcolor = icon.color;
			iconcolor.SetAlpha(0.5f);
			icon.color = iconcolor;
		}

		public void setEnabled(bool enabled)
		{
			ValidateNotDestroyed();
			if (enabled)
			{
				enable();
			}
			else
			{
				disable();
			}
		}

		/// <summary>
		/// Set this item as the dragging preview.
		/// </summary>
		public void SetIsDragItem()
		{
			ValidateNotDestroyed();
			button.IsRaycastTarget = false;
		}

		public void updateHotkey(byte index)
		{
			ValidateNotDestroyed();
			_hotkey = index;

			if (hotkey == 255)
			{
				hotkeyLabel.Text = "";
				hotkeyLabel.IsVisible = false;
			}
			else
			{
				hotkeyLabel.Text = ControlsSettings.getEquipmentHotkeyText(hotkey);
				hotkeyLabel.IsVisible = true;
			}
		}

		public void updateItem(ItemJar newJar)
		{
			ValidateNotDestroyed();

			if (_jar != null && _jar.item != null && _jar.item.id != newJar.item.id)
			{
				// If item ID changed, e.g. when dragged item changes, clear the icon to avoid a confusing frame.
				icon.Clear();
			}

			_jar = newJar;
			ItemAsset asset = jar.GetAsset();

			if (asset != null)
			{
				if (!isTemporary)
				{
					button.TooltipText = asset.itemName;
				}

				if (jar.rot % 2 == 0)
				{
					SizeOffset_X = asset.size_x * 50;
					SizeOffset_Y = asset.size_y * 50;

					icon.PositionOffset_X = 0;
					icon.PositionOffset_Y = 0;
				}
				else
				{
					SizeOffset_X = asset.size_y * 50;
					SizeOffset_Y = asset.size_x * 50;

					int offset = Mathf.Abs(asset.size_y - asset.size_x);

					if (asset.size_x > asset.size_y)
					{
						icon.PositionOffset_X = -offset * 25;
						icon.PositionOffset_Y = offset * 25;
					}
					else
					{
						icon.PositionOffset_X = offset * 25;
						icon.PositionOffset_Y = -offset * 25;
					}
				}

				icon.rot = jar.rot;
				icon.SizeOffset_X = asset.size_x * 50;
				icon.SizeOffset_Y = asset.size_y * 50;
				icon.Refresh(jar.item.id, jar.item.quality, jar.item.state, asset);

				if (asset.size_x == 1 || asset.size_y == 1)
				{
					amountLabel.PositionOffset_X = 0;
					amountLabel.PositionOffset_Y = -30;
					amountLabel.SizeOffset_X = 0;

					amountLabel.FontSize = ESleekFontSize.Small;
					hotkeyLabel.FontSize = ESleekFontSize.Small;
				}
				else
				{
					amountLabel.PositionOffset_X = 5;
					amountLabel.PositionOffset_Y = -35;
					amountLabel.SizeOffset_X = -10;

					amountLabel.FontSize = ESleekFontSize.Default;
					hotkeyLabel.FontSize = ESleekFontSize.Default;
				}

				Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
				button.BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
				button.TextColor = rarityColor;

				if (asset.showQuality)
				{
					//button.backgroundColor = ItemTool.getQualityColor(jar.item.quality / 100.0f);

					if (asset.size_x == 1 || asset.size_y == 1)
					{
						qualityImage.PositionOffset_X = -15;
						qualityImage.PositionOffset_Y = -15;
						qualityImage.SizeOffset_X = 10;
						qualityImage.SizeOffset_Y = 10;
						qualityImage.Texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_1");
					}
					else
					{
						qualityImage.PositionOffset_X = -30;
						qualityImage.PositionOffset_Y = -30;
						qualityImage.SizeOffset_X = 20;
						qualityImage.SizeOffset_Y = 20;
						qualityImage.Texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_0");
					}

					//button.foregroundColor = button.backgroundColor;

					qualityImage.TintColor = ItemTool.getQualityColor(jar.item.quality / 100.0f);

					amountLabel.Text = jar.item.quality + "%";
					amountLabel.TextColor = qualityImage.TintColor;

					qualityImage.IsVisible = true;
					amountLabel.IsVisible = true;
				}
				else
				{
					qualityImage.IsVisible = false;

					if (asset.MaxAmount > 1)
					{
						amountLabel.Text = "x" + jar.item.amount;
						amountLabel.TextColor = ESleekTint.FONT;

						amountLabel.IsVisible = true;
					}
					else
					{
						amountLabel.IsVisible = false;
					}
				}
			}
		}

		private void onClickedButton(ISleekElement button)
		{
			onDraggedItem?.Invoke(this);
		}

		private void onRightClickedButton(ISleekElement button)
		{
			onClickedItem?.Invoke(this);
		}

		public SleekItem(ItemJar jar) : base()
		{
			button = Glazier.Get().CreateButton();
			button.PositionOffset_X = 1;
			button.PositionOffset_Y = 1;
			button.SizeOffset_X = -2;
			button.SizeOffset_Y = -2;
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.OnClicked += onClickedButton;
			button.OnRightClicked += onRightClickedButton;
			AddChild(button);

			icon = new SleekItemIcon();
			AddChild(icon);
			icon.isAngled = true;

			amountLabel = Glazier.Get().CreateLabel();
			amountLabel.PositionScale_Y = 1;
			amountLabel.SizeOffset_Y = 30;
			amountLabel.SizeScale_X = 1;
			amountLabel.TextAlignment = TextAnchor.LowerLeft;
			amountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(amountLabel);
			amountLabel.IsVisible = false;

			qualityImage = Glazier.Get().CreateImage();
			qualityImage.PositionScale_X = 1;
			qualityImage.PositionScale_Y = 1;
			AddChild(qualityImage);
			qualityImage.IsVisible = false;

			hotkeyLabel = Glazier.Get().CreateLabel();
			hotkeyLabel.PositionOffset_X = 5;
			hotkeyLabel.PositionOffset_Y = 5;
			hotkeyLabel.SizeOffset_X = -10;
			hotkeyLabel.SizeOffset_Y = 30;
			hotkeyLabel.SizeScale_X = 1;
			hotkeyLabel.TextAlignment = TextAnchor.UpperRight;
			hotkeyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(hotkeyLabel);
			hotkeyLabel.IsVisible = false;

			updateItem(jar);
		}

		public SleekItem() : base()
		{
			button = Glazier.Get().CreateButton();
			button.PositionOffset_X = 1;
			button.PositionOffset_Y = 1;
			button.SizeOffset_X = -2;
			button.SizeOffset_Y = -2;
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			AddChild(button);

			icon = new SleekItemIcon();
			AddChild(icon);
			icon.isAngled = true;

			amountLabel = Glazier.Get().CreateLabel();
			amountLabel.PositionScale_Y = 1;
			amountLabel.SizeOffset_Y = 30;
			amountLabel.SizeScale_X = 1;
			amountLabel.TextAlignment = TextAnchor.LowerLeft;
			amountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(amountLabel);
			amountLabel.IsVisible = false;

			qualityImage = Glazier.Get().CreateImage();
			qualityImage.PositionScale_X = 1;
			qualityImage.PositionScale_Y = 1;
			AddChild(qualityImage);
			qualityImage.IsVisible = false;

			hotkeyLabel = Glazier.Get().CreateLabel();
			hotkeyLabel.PositionOffset_X = 5;
			hotkeyLabel.PositionOffset_Y = 5;
			hotkeyLabel.SizeOffset_X = -10;
			hotkeyLabel.SizeOffset_Y = 30;
			hotkeyLabel.SizeScale_X = 1;
			hotkeyLabel.TextAlignment = TextAnchor.UpperRight;
			hotkeyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(hotkeyLabel);
			hotkeyLabel.IsVisible = false;

			isTemporary = true;
		}
	}
}
