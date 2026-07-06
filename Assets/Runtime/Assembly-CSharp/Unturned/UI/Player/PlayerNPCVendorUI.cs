////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerNPCVendorUI
	{
		private static SleekFullscreenBox container;
		public static Local localization;
		public static bool active;

		private static VendorAsset vendor;
		private static DialogueAsset dialogue;
		private static DialogueMessage nextMessage;
		private static bool hasNextDialogue;
		private static List<VendorBuying> buying;
		private static List<VendorSellingBase> selling;
		private static List<SleekVendor> buyingButtons;
		private static List<SleekVendor> sellingButtons;
		private static VendorBuyingNameAscendingComparator buyingComparator = new VendorBuyingNameAscendingComparator();
		private static VendorSellingNameAscendingComparator sellingComparator = new VendorSellingNameAscendingComparator();

		private static ISleekBox vendorBox;
		private static ISleekLabel nameLabel;
		private static ISleekLabel descriptionLabel;

		private static ISleekLabel sellingLabel;
		private static ISleekScrollView sellingBox;
		private static ISleekLabel buyingLabel;
		private static ISleekScrollView buyingBox;

		private static ISleekBox experienceBox;
		private static ISleekBox currencyBox;
		private static ISleekElement currencyPanel;
		private static ISleekLabel currencyLabel;
		private static ISleekButton returnButton;

		public static void open(VendorAsset newVendor, DialogueAsset newDialogue, DialogueMessage newNextMessage, bool newHasNextDialogue)
		{
			if (active)
			{
				return;
			}

			active = true;
			updateVendor(newVendor, newDialogue, newNextMessage, newHasNextDialogue);

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, 1);
		}

		public static void closeNicely()
		{
			close();

			PlayerNPCDialogueUI.open(dialogue, nextMessage, hasNextDialogue);
		}

		/// <summary>
		/// Update currency and owned items if inventory has changed and menu is open.
		/// </summary>
		public static void MaybeRefresh()
		{
			if (!active || !needsRefresh || vendor == null)
				return;

			Player player = Player.LocalPlayer;
			if (player == null || player.inventory == null)
				return;

			needsRefresh = false;

			RefreshExperienceOrCurrencyBoxAmount();
			RefreshButtonVisibility();

			foreach (SleekVendor button in buyingButtons)
			{
				button.updateAmount();
			}

			foreach (SleekVendor button in sellingButtons)
			{
				button.updateAmount();
			}
		}

		private static void RefreshButtonVisibility()
		{
			Player player = Player.LocalPlayer;

			float buyingOffset = 0;
			for (int index = 0; index < buying.Count; ++index)
			{
				bool isVisible = buying[index].areConditionsMet(player);
				buyingButtons[index].IsVisible = isVisible;
				if (isVisible)
				{
					buyingButtons[index].PositionOffset_Y = buyingOffset;
					buyingOffset += buyingButtons[index].SizeOffset_Y;
				}
			}
			buyingBox.IsVisible = buyingOffset > 0;
			buyingBox.ContentSizeOffset = new Vector2(0.0f, buyingOffset);

			float sellingOffset = 0;
			for (int index = 0; index < selling.Count; ++index)
			{
				bool isVisible = selling[index].areConditionsMet(player);
				sellingButtons[index].IsVisible = isVisible;
				if (isVisible)
				{
					sellingButtons[index].PositionOffset_Y = sellingOffset;
					sellingOffset += sellingButtons[index].SizeOffset_Y;
				}
			}
			sellingBox.IsVisible = sellingOffset > 0;
			sellingBox.ContentSizeOffset = new Vector2(0.0f, sellingOffset);
		}

		private static void RefreshExperienceOrCurrencyBoxAmount()
		{
			if (experienceBox.IsVisible)
			{
				experienceBox.Text = localization.format("Experience", Player.LocalPlayer.skills.experience.ToString());
			}
			else if (currencyBox.IsVisible)
			{
				ItemCurrencyAsset currencyAsset = vendor.currency.Find();
				if (currencyAsset != null)
				{
					uint totalValue = currencyAsset.getInventoryValue(Player.LocalPlayer);
					if (string.IsNullOrEmpty(currencyAsset.valueFormat))
					{
						currencyLabel.Text = totalValue.ToString("N");
					}
					else
					{
						currencyLabel.Text = string.Format(currencyAsset.valueFormat, totalValue);
					}
				}
			}
		}

		/// <summary>
		/// Update currency or experience depending what the vendor accepts.
		/// </summary>
		private static void updateCurrencyOrExperienceBox()
		{
			currencyBox.IsVisible = vendor.currency.isValid;
			experienceBox.IsVisible = !currencyBox.IsVisible;

			if (currencyBox.IsVisible == false)
				return;

			currencyPanel.RemoveAllChildren(); // Always remove currency icons in case currency asset is invalid.

			ItemCurrencyAsset currencyAsset = vendor.currency.Find();
			if (currencyAsset == null)
			{
				Assets.ReportError(vendor, "unable to find currency");
				currencyLabel.Text = "Invalid";
				return;
			}

			float offset_x = 5;
			foreach (ItemCurrencyAsset.Entry entry in currencyAsset.entries)
			{
				ItemAsset itemAsset = entry.item.Find();
				if (itemAsset == null)
				{
					Assets.ReportError(vendor, "unable to find entry item {0}", entry.item);
					continue;
				}

				if (!entry.isVisibleInVendorMenu)
				{
					// Explicitly excluded.
					continue;
				}

				SleekItemIcon itemImage = new SleekItemIcon();
				itemImage.PositionOffset_X = offset_x;
				itemImage.PositionOffset_Y = 5;

				// All icons are 40 pixels tall, so we adjust width to preserve aspect ratio.
				float iconAspectRatio = itemAsset.size_x / (float) itemAsset.size_y;
				itemImage.SizeOffset_X = Mathf.RoundToInt(iconAspectRatio * 40);
				itemImage.SizeOffset_Y = 40;

				currencyPanel.AddChild(itemImage);

				itemImage.Refresh(itemAsset.id, 100, itemAsset.getState(false), itemAsset, Mathf.RoundToInt(itemImage.SizeOffset_X), Mathf.RoundToInt(itemImage.SizeOffset_Y));

				ISleekLabel valueLabel = Glazier.Get().CreateLabel();
				valueLabel.PositionOffset_X = itemImage.PositionOffset_X;
				valueLabel.PositionOffset_Y = 0;
				valueLabel.SizeOffset_X = itemImage.SizeOffset_X;
				valueLabel.SizeScale_Y = 1.0f;
				valueLabel.Text = entry.value.ToString();
				valueLabel.TextAlignment = TextAnchor.LowerCenter;
				valueLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				currencyPanel.AddChild(valueLabel);

				offset_x += itemImage.SizeOffset_X + 2;
			}
		}

		private static void updateVendor(VendorAsset newVendor, DialogueAsset newDialogue, DialogueMessage newNextMessage, bool newHasNextDialogue)
		{
			vendor = newVendor;
			dialogue = newDialogue;
			nextMessage = newNextMessage;
			hasNextDialogue = newHasNextDialogue;

			if (vendor == null)
			{
				return;
			}

			if (PlayerLifeUI.npc != null)
			{
				PlayerLifeUI.npc.SetFaceOverride(vendor.faceOverride);
			}

			nameLabel.Text = vendor.vendorName;
			descriptionLabel.Text = vendor.vendorDescription;

			buyingButtons.Clear();
			sellingButtons.Clear();

			buying.Clear();
			buying.AddRange(vendor.buying);
			if (vendor.enableSorting)
			{
				buying.Sort(buyingComparator);
			}

			buyingBox.RemoveAllChildren();
			foreach (VendorBuying element in buying)
			{
				SleekVendor buyingButton = new SleekVendor(element);
				buyingButton.SizeScale_X = 1;
				buyingButton.onClickedButton += onClickedBuyingButton;
				buyingBox.AddChild(buyingButton);
				buyingButtons.Add(buyingButton);
			}

			selling.Clear();
			selling.AddRange(vendor.selling);
			if (vendor.enableSorting)
			{
				selling.Sort(sellingComparator);
			}

			sellingBox.RemoveAllChildren();
			foreach (VendorSellingBase element in selling)
			{
				SleekVendor sellingButton = new SleekVendor(element);
				sellingButton.SizeScale_X = 1;
				sellingButton.onClickedButton += onClickedSellingButton;
				sellingBox.AddChild(sellingButton);
				sellingButtons.Add(sellingButton);
			}

			needsRefresh = false;
			updateCurrencyOrExperienceBox();
			RefreshExperienceOrCurrencyBoxAmount();
			RefreshButtonVisibility();
			// Do not call button.updateAmount here because constructing them already updates.
		}

		private static void onInventoryStateUpdated()
		{
			needsRefresh = true;
		}

		private static void onExperienceUpdated(uint newExperience)
		{
			needsRefresh = true;
		}

		private static void onReputationUpdated(int newReputation)
		{
			needsRefresh = true;
		}

		private static void onFlagsUpdated()
		{
			needsRefresh = true;
		}

		private static void onFlagUpdated(ushort id)
		{
			needsRefresh = true;
		}

		private static void onClickedBuyingButton(ISleekElement button)
		{
			byte index = (byte) buyingBox.FindIndexOfChild(button);
			VendorBuying element = buying[index];

			if (!element.canSell(Player.LocalPlayer))
			{
				return;
			}

			Player.LocalPlayer.quests.sendSellToVendor(vendor.GUID, element.index, InputEx.GetKey(ControlsSettings.other));
		}

		private static void onClickedSellingButton(ISleekElement button)
		{
			byte index = (byte) sellingBox.FindIndexOfChild(button);
			VendorSellingBase element = selling[index];

			if (!element.canBuy(Player.LocalPlayer))
			{
				return;
			}

			Player.LocalPlayer.quests.sendBuyFromVendor(vendor.GUID, element.index, InputEx.GetKey(ControlsSettings.other));
		}

		private static void onClickedReturnButton(ISleekElement button)
		{
			closeNicely();
		}

		public PlayerNPCVendorUI()
		{
			localization = Localization.read("/Player/PlayerNPCVendor.dat");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;

			buying = new List<VendorBuying>();
			selling = new List<VendorSellingBase>();
			buyingButtons = new List<SleekVendor>();
			sellingButtons = new List<SleekVendor>();

			vendorBox = Glazier.Get().CreateBox();
			//vendorBox.positionOffset_X = 100;
			//vendorBox.positionOffset_Y = 100;
			//vendorBox.sizeOffset_X = -200;
			vendorBox.SizeOffset_Y = -60;
			vendorBox.SizeScale_X = 1;
			vendorBox.SizeScale_Y = 1;
			container.AddChild(vendorBox);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 5;
			nameLabel.PositionOffset_Y = 5;
			nameLabel.SizeOffset_X = -10;
			nameLabel.SizeOffset_Y = 40;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameLabel.AllowRichText = true;
			nameLabel.FontSize = ESleekFontSize.Large;
			vendorBox.AddChild(nameLabel);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 5;
			descriptionLabel.PositionOffset_Y = 40;
			descriptionLabel.SizeOffset_X = -10;
			descriptionLabel.SizeOffset_Y = 40;
			descriptionLabel.SizeScale_X = 1.0f;
			descriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			descriptionLabel.AllowRichText = true;
			vendorBox.AddChild(descriptionLabel);

			buyingLabel = Glazier.Get().CreateLabel();
			buyingLabel.PositionOffset_X = 5;
			buyingLabel.PositionOffset_Y = 80;
			buyingLabel.SizeOffset_X = -40;
			buyingLabel.SizeOffset_Y = 30;
			buyingLabel.SizeScale_X = 0.5f;
			buyingLabel.FontSize = ESleekFontSize.Medium;
			buyingLabel.Text = localization.format("Buying");
			vendorBox.AddChild(buyingLabel);

			buyingBox = Glazier.Get().CreateScrollView();
			buyingBox.PositionOffset_X = 5;
			buyingBox.PositionOffset_Y = 115;
			buyingBox.SizeOffset_X = -10;
			buyingBox.SizeOffset_Y = -120;
			buyingBox.SizeScale_X = 0.5f;
			buyingBox.SizeScale_Y = 1.0f;
			buyingBox.ScaleContentToWidth = true;
			buyingBox.ContentSizeOffset = new Vector2(0.0f, 1024);
			vendorBox.AddChild(buyingBox);

			sellingLabel = Glazier.Get().CreateLabel();
			sellingLabel.PositionOffset_X = 5;
			sellingLabel.PositionOffset_Y = 80;
			sellingLabel.PositionScale_X = 0.5f;
			sellingLabel.SizeOffset_X = -40;
			sellingLabel.SizeOffset_Y = 30;
			sellingLabel.SizeScale_X = 0.5f;
			sellingLabel.FontSize = ESleekFontSize.Medium;
			sellingLabel.Text = localization.format("Selling");
			vendorBox.AddChild(sellingLabel);

			sellingBox = Glazier.Get().CreateScrollView();
			sellingBox.PositionOffset_X = 5;
			sellingBox.PositionOffset_Y = 115;
			sellingBox.PositionScale_X = 0.5f;
			sellingBox.SizeOffset_X = -10;
			sellingBox.SizeOffset_Y = -120;
			sellingBox.SizeScale_X = 0.5f;
			sellingBox.SizeScale_Y = 1.0f;
			sellingBox.ScaleContentToWidth = true;
			sellingBox.ContentSizeOffset = new Vector2(0.0f, 1024);
			vendorBox.AddChild(sellingBox);

			experienceBox = Glazier.Get().CreateBox();
			experienceBox.PositionOffset_Y = 10;
			experienceBox.PositionScale_Y = 1;
			experienceBox.SizeOffset_X = -5;
			experienceBox.SizeOffset_Y = 50;
			experienceBox.SizeScale_X = 0.5f;
			experienceBox.FontSize = ESleekFontSize.Medium;
			experienceBox.IsVisible = false;
			vendorBox.AddChild(experienceBox);

			currencyBox = Glazier.Get().CreateBox();
			currencyBox.PositionOffset_Y = 10;
			currencyBox.PositionScale_Y = 1;
			currencyBox.SizeOffset_X = -5;
			currencyBox.SizeOffset_Y = 50;
			currencyBox.SizeScale_X = 0.5f;
			currencyBox.IsVisible = false;
			vendorBox.AddChild(currencyBox);

			currencyPanel = Glazier.Get().CreateFrame();
			currencyPanel.SizeScale_X = 1;
			currencyPanel.SizeScale_Y = 1;
			currencyBox.AddChild(currencyPanel);

			currencyLabel = Glazier.Get().CreateLabel();
			currencyLabel.PositionOffset_X = -160;
			currencyLabel.PositionScale_X = 1.0f;
			currencyLabel.SizeOffset_X = 150;
			currencyLabel.SizeScale_Y = 1.0f;
			currencyLabel.TextAlignment = TextAnchor.MiddleRight;
			currencyLabel.FontSize = ESleekFontSize.Medium;
			currencyBox.AddChild(currencyLabel);

			returnButton = Glazier.Get().CreateButton();
			returnButton.PositionOffset_X = 5;
			returnButton.PositionOffset_Y = 10;
			returnButton.PositionScale_X = 0.5f;
			returnButton.PositionScale_Y = 1;
			returnButton.SizeOffset_X = -5;
			returnButton.SizeOffset_Y = 50;
			returnButton.SizeScale_X = 0.5f;
			returnButton.FontSize = ESleekFontSize.Medium;
			returnButton.Text = localization.format("Return");
			returnButton.TooltipText = localization.format("Return_Tooltip");
			returnButton.OnClicked += onClickedReturnButton;
			vendorBox.AddChild(returnButton);

			Player.LocalPlayer.inventory.onInventoryStateUpdated += onInventoryStateUpdated;
			Player.LocalPlayer.skills.onExperienceUpdated += onExperienceUpdated;
			Player.LocalPlayer.skills.onReputationUpdated += onReputationUpdated;
			Player.LocalPlayer.quests.onFlagsUpdated += onFlagsUpdated;
			Player.LocalPlayer.quests.onFlagUpdated += onFlagUpdated;

			needsRefresh = true;
		}

		private static bool needsRefresh;
	}
}
