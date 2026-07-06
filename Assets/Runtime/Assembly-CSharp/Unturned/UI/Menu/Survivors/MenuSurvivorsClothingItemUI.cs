////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsClothingItemUI
	{
		internal static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static int item;
		private static ushort quantity;
		private static ulong instance;

		private static ISleekConstraintFrame inventory;

		private static SleekInventory packageBox;
		private static ISleekBox descriptionBox;
		private static ISleekLabel infoLabel;
		private static ISleekButton useButton;
		private static ISleekButton inspectButton;
		private static ISleekButton marketButton;
		private static ISleekButton deleteButton;
		private static ISleekButton scrapButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

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

		public static void viewItem()
		{
			viewItem(item, quantity, instance);
		}

		public static void viewItem(int newItem, ushort newQuantity, ulong newInstance)
		{
			UnturnedLog.info("View: " + newItem + " x" + newQuantity + " [" + newInstance + "]");

			//SteamInventoryUpdateHandle_t handle = SteamInventory.StartUpdateProperties();
			//UnturnedLog.info("SetProperty: " + SteamInventory.SetProperty(handle, new SteamItemInstanceID_t(newInstance), "total_kills", (long) 55));
			//SteamInventoryResult_t result;
			//UnturnedLog.info("SubmitUpdateProperties: " + SteamInventory.SubmitUpdateProperties(handle, out result));

			item = newItem;
			quantity = newQuantity;
			instance = newInstance;
			packageBox.updateInventory(instance, item, newQuantity, false, true);

			if (packageBox.itemAsset == null && packageBox.vehicleAsset == null)
			{
				useButton.IsVisible = false;
				inspectButton.IsVisible = false;
				marketButton.IsVisible = false;
				scrapButton.IsVisible = false;

				deleteButton.IsVisible = true;
				descriptionBox.SizeOffset_Y = -60;
				deleteButton.PositionOffset_Y = -descriptionBox.SizeOffset_Y - 50;
				deleteButton.SizeScale_X = 0.5f;

				infoLabel.Text = localization.format("Unknown");
			}
			else
			{
				if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.KEY)
				{
					if ((packageBox.itemAsset as ItemKeyAsset).exchangeWithTargetItem)
					{
						useButton.IsVisible = true;

						useButton.Text = localization.format("Target_Item_Text");
						useButton.TooltipText = localization.format("Target_Item_Tooltip");
					}
					else
					{
						useButton.IsVisible = false;
					}

					inspectButton.IsVisible = false;
				}
				else if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.BOX)
				{
					useButton.IsVisible = true;
					inspectButton.IsVisible = false;

					useButton.Text = localization.format("Contents_Text");
					useButton.TooltipText = localization.format("Contents_Tooltip");
				}
				else
				{
					useButton.IsVisible = true;
					inspectButton.IsVisible = true;

					bool isUsing = false;
					if (packageBox.itemAsset == null || packageBox.itemAsset.proPath == null || packageBox.itemAsset.proPath.Length == 0)
					{
						isUsing = Characters.isSkinEquipped(instance);
					}
					else
					{
						isUsing = Characters.isCosmeticEquipped(instance);
					}

					useButton.Text = localization.format(isUsing ? "Dequip_Text" : "Equip_Text");
					useButton.TooltipText = localization.format(isUsing ? "Dequip_Tooltip" : "Equip_Tooltip");
				}

				marketButton.IsVisible = Provider.provider.economyService.getInventoryMarketable(item);

				int scraps = Provider.provider.economyService.getInventoryScraps(item);
				scrapButton.Text = localization.format("Scrap_Text", scraps);
				scrapButton.TooltipText = localization.format("Scrap_Tooltip", scraps);
				scrapButton.IsVisible = scraps > 0;

				descriptionBox.SizeOffset_Y = 0;

				if (useButton.IsVisible || inspectButton.IsVisible)
				{
					descriptionBox.SizeOffset_Y -= 60;

					useButton.PositionOffset_Y = -descriptionBox.SizeOffset_Y - 50;
					inspectButton.PositionOffset_Y = -descriptionBox.SizeOffset_Y - 50;
				}

				if (scrapButton.IsVisible)
				{
					deleteButton.SizeScale_X = 0.25f;
				}
				else
				{
					deleteButton.SizeScale_X = 0.5f;
				}

				if (marketButton.IsVisible || deleteButton.IsVisible || scrapButton.IsVisible)
				{
					descriptionBox.SizeOffset_Y -= 60;

					marketButton.PositionOffset_Y = -descriptionBox.SizeOffset_Y - 50;
					deleteButton.PositionOffset_Y = -descriptionBox.SizeOffset_Y - 50;
					scrapButton.PositionOffset_Y = -descriptionBox.SizeOffset_Y - 50;
				}

				infoLabel.Text = "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryType(item) + "</color>\n\n" + Provider.provider.economyService.getInventoryDescription(item);
			}
		}

		private static void onClickedUseButton(ISleekElement button)
		{
			if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.KEY)
			{
				EEconFilterMode filterMode;
				switch (packageBox.itemAsset.id) // Messy, but not a big deal because these do not need to be configurable.
				{
					case 845: // Total Kills
					case 846: // Player Kills
						filterMode = EEconFilterMode.STAT_TRACKER;
						break;
					case 992:
						filterMode = EEconFilterMode.STAT_TRACKER_REMOVAL;
						break;
					case 993:
						filterMode = EEconFilterMode.RAGDOLL_EFFECT_REMOVAL;
						break;
					case 1524: // Zero Kelvin
					case 1868: // Jaded
					case 1869: // Green Soul Crystal
					case 1870: // Magenta Soul Crystal
					case 1871: // Red Soul Crystal
					case 1872: // Yellow Soul Crystal
						filterMode = EEconFilterMode.RAGDOLL_EFFECT;
						break;
					default:
						UnturnedLog.warn($"Unknown tool {packageBox.itemAsset.name}");
						filterMode = EEconFilterMode.STAT_TRACKER;
						break;
				}

				MenuSurvivorsClothingUI.setFilter(filterMode, instance);
				MenuSurvivorsClothingUI.open();

				close();
			}
			else if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.BOX)
			{
				MenuSurvivorsClothingBoxUI.viewItem(item, quantity, instance);
				MenuSurvivorsClothingBoxUI.open();

				close();
			}
			else
			{
				Characters.ToggleEquipItemByInstanceId(instance);

				viewItem();
			}
		}

		private static void onClickedInspectButton(ISleekElement button)
		{
			MenuSurvivorsClothingInspectUI.viewItem(item, instance);
			MenuSurvivorsClothingInspectUI.open(EMenuSurvivorsClothingInspectUIOpenContext.OwnedItem);

			close();
		}

		private static void onClickedMarketButton(ISleekElement button)
		{
			if (!Provider.provider.economyService.canOpenInventory)
			{
				MenuUI.alert(localization.format("Overlay"));

				return;
			}

			Provider.provider.economyService.open(instance);
		}

		private static void onClickedDeleteButton(ISleekElement button)
		{
			MenuSurvivorsClothingDeleteUI.viewItem(item, instance, quantity, EDeleteMode.DELETE, 0);
			MenuSurvivorsClothingDeleteUI.open();

			close();
		}

		private static void onClickedScrapButton(ISleekElement button)
		{
			if (Provider.provider.economyService.getInventoryMythicID(item) != 0 || !InputEx.GetKey(ControlsSettings.other))
			{
				MenuSurvivorsClothingDeleteUI.viewItem(item, instance, quantity, EDeleteMode.SALVAGE, 0);
				MenuSurvivorsClothingDeleteUI.open();

				close();
			}
			else
			{
				MenuSurvivorsClothingDeleteUI.salvageItem(item, instance);
				onClickedBackButton(null);
			}
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsClothingUI.open();
			close();
		}

		public MenuSurvivorsClothingItemUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothingItem.dat");
			//Bundle icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Survivors/MenuSurvivorsClothingBox/MenuSurvivorsClothingItem.unity3d");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			inventory = Glazier.Get().CreateConstraintFrame();
			inventory.PositionScale_X = 0.5f;
			inventory.PositionOffset_Y = 10;
			inventory.SizeScale_X = 0.5f;
			inventory.SizeScale_Y = 1;
			inventory.SizeOffset_Y = -20;
			inventory.Constraint = ESleekConstraint.FitInParent;
			container.AddChild(inventory);

			ISleekConstraintFrame packageFrame = Glazier.Get().CreateConstraintFrame();
			packageFrame.SizeScale_X = 1;
			packageFrame.SizeScale_Y = 0.5f;
			packageFrame.SizeOffset_Y = -5;
			packageFrame.Constraint = ESleekConstraint.FitInParent;
			inventory.AddChild(packageFrame);

			packageBox = new SleekInventory();
			packageBox.SizeScale_X = 1;
			packageBox.SizeScale_Y = 1;
			packageFrame.AddChild(packageBox);

			descriptionBox = Glazier.Get().CreateBox();
			descriptionBox.PositionOffset_Y = 10;
			descriptionBox.PositionScale_Y = 1;
			descriptionBox.SizeScale_X = 1;
			descriptionBox.SizeScale_Y = 1;
			packageBox.AddChild(descriptionBox);

			infoLabel = Glazier.Get().CreateLabel();
			infoLabel.AllowRichText = true;
			infoLabel.PositionOffset_X = 5;
			infoLabel.PositionOffset_Y = 5;
			infoLabel.SizeScale_X = 1;
			infoLabel.SizeScale_Y = 1;
			infoLabel.SizeOffset_X = -10;
			infoLabel.SizeOffset_Y = -10;
			infoLabel.TextAlignment = TextAnchor.UpperLeft;
			infoLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			infoLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			descriptionBox.AddChild(infoLabel);

			useButton = Glazier.Get().CreateButton();
			useButton.PositionScale_Y = 1f;
			useButton.SizeOffset_X = -5;
			useButton.SizeOffset_Y = 50;
			useButton.SizeScale_X = 0.5f;
			useButton.OnClicked += onClickedUseButton;
			descriptionBox.AddChild(useButton);
			useButton.FontSize = ESleekFontSize.Medium;
			useButton.IsVisible = false;

			inspectButton = Glazier.Get().CreateButton();
			inspectButton.PositionOffset_X = 5;
			inspectButton.PositionScale_X = 0.5f;
			inspectButton.PositionScale_Y = 1f;
			inspectButton.SizeOffset_X = -5;
			inspectButton.SizeOffset_Y = 50;
			inspectButton.SizeScale_X = 0.5f;
			inspectButton.Text = localization.format("Inspect_Text");
			inspectButton.TooltipText = localization.format("Inspect_Tooltip");
			inspectButton.OnClicked += onClickedInspectButton;
			descriptionBox.AddChild(inspectButton);
			inspectButton.FontSize = ESleekFontSize.Medium;
			inspectButton.IsVisible = false;

			marketButton = Glazier.Get().CreateButton();
			marketButton.PositionScale_Y = 1f;
			marketButton.SizeOffset_X = -5;
			marketButton.SizeOffset_Y = 50;
			marketButton.SizeScale_X = 0.5f;
			marketButton.Text = localization.format("Market_Text");
			marketButton.TooltipText = localization.format("Market_Tooltip");
			marketButton.OnClicked += onClickedMarketButton;
			descriptionBox.AddChild(marketButton);
			marketButton.FontSize = ESleekFontSize.Medium;
			marketButton.IsVisible = false;

			deleteButton = Glazier.Get().CreateButton();
			deleteButton.PositionOffset_X = 5;
			deleteButton.PositionScale_X = 0.5f;
			deleteButton.PositionScale_Y = 1f;
			deleteButton.SizeOffset_X = -5;
			deleteButton.SizeOffset_Y = 50;
			deleteButton.SizeScale_X = 0.5f;
			deleteButton.Text = localization.format("Delete_Text");
			deleteButton.TooltipText = localization.format("Delete_Tooltip");
			deleteButton.OnClicked += onClickedDeleteButton;
			descriptionBox.AddChild(deleteButton);
			deleteButton.FontSize = ESleekFontSize.Medium;

			scrapButton = Glazier.Get().CreateButton();
			scrapButton.PositionOffset_X = 5;
			scrapButton.PositionScale_X = 0.75f;
			scrapButton.PositionScale_Y = 1f;
			scrapButton.SizeOffset_X = -5;
			scrapButton.SizeOffset_Y = 50;
			scrapButton.SizeScale_X = 0.25f;
			scrapButton.OnClicked += onClickedScrapButton;
			descriptionBox.AddChild(scrapButton);
			scrapButton.FontSize = ESleekFontSize.Medium;
			scrapButton.IsVisible = false;

			//icons.unload();

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);
		}
	}
}
