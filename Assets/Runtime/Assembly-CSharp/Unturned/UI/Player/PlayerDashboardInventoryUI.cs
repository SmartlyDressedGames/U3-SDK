////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define WITH_INVENTORY_CLICK_GIZMOS
#define WITH_NEARBY_ITEM_LOS_DEBUG
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public class PlayerDashboardInventoryUI
	{
		private static List<InteractableItem> pendingItemsInRadius;

		private static SleekFullscreenBox container;
		public static Local localization;
		public static IconsBundle icons;
		public static bool active;

		private static ISleekBox backdropBox;

		/// <summary>
		/// Added during the UI refactor to catch unhandled mouse clicks during drag.
		/// </summary>
		private static ISleekImage dragOutsideHandler;
		private static ISleekImage dragOutsideClothingHandler;
		private static ISleekImage dragOutsideAreaHandler;
		private static bool hasDragOutsideHandlers;

		public static bool isDragging
		{
			get;
			private set;
		}
		private static ItemJar dragJar;
		private static SleekItem dragSource;
		private static SleekItem dragItem;
		private static Vector2 dragOffset;
		private static Vector2 dragPivot;

		private static byte dragFromPage;
		private static byte dragFrom_x;
		private static byte dragFrom_y;
		private static byte dragFromRot;

		private static SleekCameraImage characterImage;
		private static ISleekSlider characterSlider;
		private static SleekButtonIcon swapCosmeticsButton;
		private static SleekButtonIcon swapSkinsButton;
		private static SleekButtonIcon swapMythicsButton;
		private static SleekPlayer characterPlayer;

		private static SleekSlot[] slots;
		private static ISleekElement box;
		private static ISleekScrollView clothingBox;
		private static ISleekScrollView areaBox;
		private static ISleekButton[] headers;
		private static SleekItemIcon[] headerItemIcons;
		private static SleekItems[] items;

		/// <summary>
		/// Contains inspect item box and invisible button.
		/// </summary>
		private static ISleekElement selectionFrame;

		/// <summary>
		/// Added during the UI refactor to catch mouse clicks outside the selection box.
		/// </summary>
		private static ISleekImage outsideSelectionInvisibleButton;

		private static ISleekBox selectionBackdropBox;
		//		private static ISleekImage iconImage;
		//		private static ISleekLabel nameLabel;
		private static ISleekBox selectionIconBox;
		private static SleekItemIcon selectionIconImage;
		private static ISleekScrollView selectionDescriptionScrollView;
		private static ISleekBox selectionDescriptionBox;
		private static ISleekLabel selectionDescriptionLabel;
		private static ISleekLabel selectionNameLabel;
		private static ISleekLabel selectionHotkeyLabel;
		//private static ISleekBox selectionQualityBox;

		private static ISleekBox vehicleBox;
		private static ISleekLabel vehicleNameLabel;
		private static ISleekElement vehicleActionsBox;
		private static ISleekElement vehiclePassengersBox;
		//private static SleekScrollBox vehiclePassengersScrollBox;
		private static ISleekButton vehicleLockButton;
		private static ISleekButton vehicleHornButton;
		private static ISleekButton vehicleHeadlightsButton;
		private static ISleekButton vehicleSirensButton;
		private static ISleekButton vehicleBlimpButton;
		private static ISleekButton vehicleHookButton;
		private static ISleekButton vehicleStealBatteryButton;
		private static ISleekButton vehicleSkinButton;

		private static ISleekScrollView selectionActionsBox;
		private static ISleekButton selectionEquipButton;
		private static ISleekButton selectionContextButton;
		private static ISleekButton selectionDropButton;
		private static ISleekButton selectionStorageButton;
		private static ISleekElement selectionExtraActionsBox;

		private static ISleekButton rot_xButton;
		private static ISleekButton rot_yButton;
		private static ISleekButton rot_zButton;

		private static byte _selectedPage;
		public static byte selectedPage => _selectedPage;

		private static byte _selected_x;
		public static byte selected_x => _selected_x;

		private static byte _selected_y;
		public static byte selected_y => _selected_y;

		private static ItemJar _selectedJar;
		public static ItemJar selectedJar => _selectedJar;

		private static ItemAsset _selectedAsset;
		public static ItemAsset selectedAsset => _selectedAsset;

		private static Items areaItems;

		private static bool isSplitClothingArea => Screen.width >= 1350;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			Player.LocalPlayer.animator.sendGesture(EPlayerGesture.INVENTORY_START, false);
			Player.LocalPlayer.character.Find("Camera").gameObject.SetActive(true);

			if (isSplitClothingArea)
			{
				clothingBox.SizeOffset_X = -5;
				clothingBox.SizeScale_X = 0.5f;

				areaBox.IsVisible = true;
			}
			else
			{
				clothingBox.SizeOffset_X = 0;
				clothingBox.SizeScale_X = 1;

				areaBox.IsVisible = false;
			}

			updateVehicle();
			resetNearbyDrops();
			updateHotkeys();

			if (characterPlayer != null)
			{
				backdropBox.RemoveChild(characterPlayer);
				characterPlayer = null;
			}

			if (Player.LocalPlayer != null)
			{
				characterPlayer = new SleekPlayer(Player.LocalPlayer.channel.owner, true, SleekPlayer.ESleekPlayerDisplayContext.NONE);
				characterPlayer.PositionOffset_X = 10;
				characterPlayer.PositionOffset_Y = 10;
				characterPlayer.SizeOffset_X = 410;
				characterPlayer.SizeOffset_Y = 50;
				backdropBox.AddChild(characterPlayer);
			}

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			Player.LocalPlayer.animator.sendGesture(EPlayerGesture.INVENTORY_STOP, false);
			Player.LocalPlayer.character.Find("Camera").gameObject.SetActive(false);

			stopDrag();

			closeSelection();

			container.AnimateOutOfView(0, 1);
		}

		private static void startDrag()
		{
			if (isDragging)
			{
				return;
			}

			isDragging = true;

			setItemsEnabled(false);

			dragItem.IsVisible = true;

			if (hasDragOutsideHandlers)
			{
				dragOutsideHandler.IsVisible = true;
				dragOutsideClothingHandler.IsVisible = true;
				dragOutsideAreaHandler.IsVisible = true;
			}

			PlayInventoryAudio(dragJar.GetAsset());
		}

		public static void stopDrag()
		{
			if (!isDragging)
			{
				return;
			}

			isDragging = false;

			PlayInventoryAudio(dragJar.GetAsset());

			dragJar.rot = dragFromRot;
			setItemsEnabled(true);
			dragItem.IsVisible = false;

			if (hasDragOutsideHandlers)
			{
				dragOutsideHandler.IsVisible = false;
				dragOutsideClothingHandler.IsVisible = false;
				dragOutsideAreaHandler.IsVisible = false;
			}
		}

		private static void setItemsEnabled(bool enabled)
		{
			foreach (SleekSlot slot in slots)
			{
				slot.isItemEnabled = enabled;
			}

			foreach (SleekItems container in items)
			{
				container.areItemsEnabled = enabled;
			}
		}

		/// <summary>
		/// Workaround for IMGUI. Disable inventory headers, grids and slots while selection is open
		/// to prevent them from interfering with selection menu.
		/// </summary>
		private static void setMiscButtonsEnabled(bool enabled)
		{
			foreach (ISleekButton header in headers)
			{
				header.IsRaycastTarget = enabled;
			}

			foreach (SleekSlot slot in slots)
			{
				slot.isImageRaycastTarget = enabled;
			}

			foreach (SleekItems container in items)
			{
				container.isGridRaycastTarget = enabled;
			}
		}

		private static void onDraggedCharacterSlider(ISleekSlider slider, float state)
		{
			PlayerLook.characterYaw = state * 360;
		}

		private static void onClickedSwapCosmeticsButton(ISleekElement button)
		{
			Player.LocalPlayer.clothing.sendVisualToggle(EVisualToggleType.COSMETIC);
		}

		private static void onClickedSwapSkinsButton(ISleekElement button)
		{
			Player.LocalPlayer.clothing.sendVisualToggle(EVisualToggleType.SKIN);
		}

		private static void onClickedSwapMythicsButton(ISleekElement button)
		{
			Player.LocalPlayer.clothing.sendVisualToggle(EVisualToggleType.MYTHIC);
		}

		private static void onClickedVehicleLockButton(ISleekElement button)
		{
			VehicleManager.sendVehicleLock();
		}

		private static void onClickedVehicleHornButton(ISleekElement button)
		{
			VehicleManager.sendVehicleHorn();
		}

		private static void onClickedVehicleHeadlightsButton(ISleekElement button)
		{
			VehicleManager.sendVehicleHeadlights();
		}

		private static void onClickedVehicleSirensButton(ISleekElement button)
		{
			VehicleManager.sendVehicleBonus();
		}

		private static void onClickedVehicleBlimpButton(ISleekElement button)
		{
			VehicleManager.sendVehicleBonus();
		}

		private static void onClickedVehicleHookButton(ISleekElement button)
		{
			VehicleManager.sendVehicleBonus();
		}

		private static void onClickedVehicleStealBatteryButton(ISleekElement button)
		{
			VehicleManager.sendVehicleStealBattery();
		}

		private static void onClickedVehicleSkinButton(ISleekElement button)
		{
			VehicleManager.sendVehicleSkin();
		}

		private static void onClickedVehiclePassengerButton(ISleekElement button)
		{
			int index = vehiclePassengersBox.FindIndexOfChild(button);

			if (index < 0)
			{
				return;
			}

			VehicleManager.swapVehicle((byte) index);
		}

		private static OncePerFrameGuard eventGuard;

		/// <summary>
		/// Was ConsumeEvent called during this frame?
		/// This is a hack to prevent firing when clicking in the UI on the same frame it closes.
		/// Moved from SleekWindow and Event.current.Use() during UI refactor.
		/// </summary>
		public static bool WasEventConsumed => eventGuard.HasBeenConsumed;

		private static void ConsumeEvent()
		{
			eventGuard.Consume();
		}

		private static void onClickedEquip(ISleekElement button)
		{
			if (selectedPage != 255)
			{
				checkEquip(selectedPage, selected_x, selected_y, Player.LocalPlayer.inventory.getItem(selectedPage, Player.LocalPlayer.inventory.getIndex(selectedPage, selected_x, selected_y)), 255);

				ConsumeEvent();
			}
		}

		private static void onClickedContext(ISleekElement button)
		{
			if (selectedPage != 255)
			{
				if (selectedAsset.type == EItemType.GUN)
				{
					Player.LocalPlayer.crafting.sendStripAttachments(selectedPage, selected_x, selected_y);
				}
				//else if(selectedAsset.type == EItemType.SHIRT || selectedAsset.type == EItemType.PANTS || selectedAsset.type == EItemType.HAT || selectedAsset.type == EItemType.BACKPACK || selectedAsset.type == EItemType.VEST || selectedAsset.type == EItemType.GLASSES || selectedAsset.type == EItemType.MASK)
				//{
				//	checkAction(selectedPage, selected_x, selected_y, Player.player.inventory.getItem(selectedPage, Player.player.inventory.getIndex(selectedPage, selected_x, selected_y)));
				//}

				ConsumeEvent();
				closeSelection();
			}
		}

		private static void onClickedDrop(ISleekElement button)
		{
			if (selectedPage != 255)
			{
				if (selectedPage == PlayerInventory.AREA)
				{
					if (selectedJar.interactableItem != null)
					{
						ItemManager.takeItem(selectedJar.interactableItem.transform.parent, 255, 255, 0, 255);
					}
					closeSelection();
				}
				else
				{
					Player.LocalPlayer.inventory.sendDropItem(selectedPage, selected_x, selected_y);
				}

				ConsumeEvent();
			}
		}

		private static void onClickedStore(ISleekElement button)
		{
			if (selectedPage != 255)
			{
				if (selectedPage == PlayerInventory.AREA)
				{
					if (selectedJar.interactableItem != null)
					{
						ItemManager.takeItem(selectedJar.interactableItem.transform.parent, 255, 255, 0, PlayerInventory.STORAGE);
					}
					closeSelection();
				}
				else if (selectedPage == PlayerInventory.STORAGE)
				{
					byte newPage;
					byte new_x;
					byte new_y;
					byte newRot;
					if (Player.LocalPlayer.inventory.tryFindSpace(selectedJar.size_x, selectedJar.size_y, out newPage, out new_x, out new_y, out newRot))
					{
						PlayInventoryAudio(selectedJar.GetAsset());

						Player.LocalPlayer.inventory.sendDragItem(selectedPage, selected_x, selected_y, newPage, new_x, new_y, newRot);
					}
				}
				else
				{
					byte new_x;
					byte new_y;
					byte newRot;
					if (Player.LocalPlayer.inventory.tryFindSpace(PlayerInventory.STORAGE, selectedJar.size_x, selectedJar.size_y, out new_x, out new_y, out newRot))
					{
						PlayInventoryAudio(selectedJar.GetAsset());

						Player.LocalPlayer.inventory.sendDragItem(selectedPage, selected_x, selected_y, PlayerInventory.STORAGE, new_x, new_y, newRot);
					}
				}

				ConsumeEvent();
			}
		}

		private static void onClickedRot_XButton(ISleekElement button)
		{
			InteractableStorage storage = PlayerInteract.interactable as InteractableStorage;

			if (storage == null || !storage.isDisplay)
			{
				return;
			}

			byte rot = storage.rot_x;
			rot++;
			if (rot > 3)
			{
				rot = 0;
			}

			byte rotComp = storage.getRotation(rot, storage.rot_y, storage.rot_z);
			storage.ClientSetDisplayRotation(rotComp);
		}

		private static void onClickedRot_YButton(ISleekElement button)
		{
			InteractableStorage storage = PlayerInteract.interactable as InteractableStorage;

			if (storage == null || !storage.isDisplay)
			{
				return;
			}

			byte rot = storage.rot_y;
			rot++;
			if (rot > 3)
			{
				rot = 0;
			}

			byte rotComp = storage.getRotation(storage.rot_x, rot, storage.rot_z);
			storage.ClientSetDisplayRotation(rotComp);
		}

		private static void onClickedRot_ZButton(ISleekElement button)
		{
			InteractableStorage storage = PlayerInteract.interactable as InteractableStorage;

			if (storage == null || !storage.isDisplay)
			{
				return;
			}

			byte rot = storage.rot_z++;
			rot++;
			if (rot > 3)
			{
				rot = 0;
			}

			byte rotComp = storage.getRotation(storage.rot_x, storage.rot_y, rot);
			storage.ClientSetDisplayRotation(rotComp);
		}

		private static void openSelection(byte page, byte x, byte y)
		{
			_selectedPage = page;
			_selected_x = x;
			_selected_y = y;

			if (!Glazier.Get().SupportsDepth)
			{
				setItemsEnabled(false);
				setMiscButtonsEnabled(false);
			}

			selectionFrame.IsVisible = true;

			//selectionBox.lerpPositionOffset((int) PlayerUI.window.mouse_x, (int) PlayerUI.window.mouse_y, ESleekLerp.EXPONENTIAL, 20);
			//selectionBox.lerpPositionScale(0, 0, ESleekLerp.EXPONENTIAL, 20);

			_selectedJar = Player.LocalPlayer.inventory.getItem(page, Player.LocalPlayer.inventory.getIndex(page, x, y));

			if (selectedJar == null)
			{
				return;
			}

			_selectedAsset = selectedJar.GetAsset();

			selectionIconImage.Clear();

			if (selectedAsset != null)
			{
				int iconWidth;
				int iconHeight;

				if (selectedAsset.size_x <= selectedAsset.size_y) // vertical orientation, icon on the left and info on the right
				{
					selectionBackdropBox.SizeOffset_X = 490;
					selectionBackdropBox.SizeOffset_Y = 330; // 300 for item, 20 for buffer and 10 for middle

					selectionIconBox.SizeOffset_X = 210;
					selectionIconBox.SizeOffset_Y = 310;

					if (selectionDescriptionScrollView != null)
					{
						selectionDescriptionScrollView.PositionOffset_X = 230;
						selectionDescriptionScrollView.PositionOffset_Y = 10;
						selectionDescriptionScrollView.SizeOffset_X = 250;
						selectionDescriptionScrollView.SizeOffset_Y = 150;
					}
					else
					{
						selectionDescriptionBox.PositionOffset_X = 230;
						selectionDescriptionBox.PositionOffset_Y = 10;
						selectionDescriptionBox.SizeOffset_X = 250;
						selectionDescriptionBox.SizeOffset_Y = 150;
					}

					selectionActionsBox.PositionOffset_X = 230;
					selectionActionsBox.PositionOffset_Y = 170;
					selectionActionsBox.SizeOffset_Y = 150;

					if (selectedAsset.size_x == selectedAsset.size_y)
					{
						iconWidth = 200;
						iconHeight = 200;
					}
					else
					{
						iconWidth = 200;
						iconHeight = 300;
					}
				}
				else // horizontal orientation, icon on the top and info on the bottom
				{
					selectionBackdropBox.SizeOffset_X = 530; // 500 for item, 20 for buffer and 10 for middle
					selectionBackdropBox.SizeOffset_Y = 390;

					selectionIconBox.SizeOffset_X = 510;
					selectionIconBox.SizeOffset_Y = 210;

					if (selectionDescriptionScrollView != null)
					{
						selectionDescriptionScrollView.PositionOffset_X = 10;
						selectionDescriptionScrollView.PositionOffset_Y = 230;
						selectionDescriptionScrollView.SizeOffset_X = 250;
						selectionDescriptionScrollView.SizeOffset_Y = 150;
					}
					else
					{
						selectionDescriptionBox.PositionOffset_X = 10;
						selectionDescriptionBox.PositionOffset_Y = 230;
						selectionDescriptionBox.SizeOffset_X = 250;
						selectionDescriptionBox.SizeOffset_Y = 150;
					}

					selectionActionsBox.PositionOffset_X = 270;
					selectionActionsBox.PositionOffset_Y = 230;
					selectionActionsBox.SizeOffset_Y = 150;

					iconWidth = 500;
					iconHeight = 200;
				}

				selectionIconImage.PositionOffset_X = -iconWidth / 2;
				selectionIconImage.PositionOffset_Y = -iconHeight / 2;
				selectionIconImage.SizeOffset_X = iconWidth;
				selectionIconImage.SizeOffset_Y = iconHeight;
				selectionIconImage.Refresh(selectedJar.item.id, selectedJar.item.quality, selectedJar.item.state, selectedAsset, iconWidth, iconHeight);

				Vector2 mousePosition = Input.mousePosition;
				mousePosition.y = Screen.height - mousePosition.y;
				mousePosition /= GraphicsSettings.userInterfaceScale;
				selectionBackdropBox.PositionOffset_X = (int) Mathf.Clamp(mousePosition.x - (selectionBackdropBox.SizeOffset_X / 2), 0, (Screen.width / GraphicsSettings.userInterfaceScale) - selectionBackdropBox.SizeOffset_X);
				selectionBackdropBox.PositionOffset_Y = (int) Mathf.Clamp(mousePosition.y - (selectionBackdropBox.SizeOffset_Y / 2), 0, (Screen.height / GraphicsSettings.userInterfaceScale) - selectionBackdropBox.SizeOffset_Y);

				ItemDescriptionBuilder descriptionBuilder = ItemDescriptionBuilderUtils.CreateForUI(selectedAsset);
				selectedAsset.BuildDescription(descriptionBuilder, selectedJar.item);
				selectionDescriptionLabel.Text = ItemDescriptionBuilderUtils.FormatLines();

				if (selectionDescriptionScrollView != null)
				{
					selectionDescriptionScrollView.ScrollToTop();
				}

				selectionNameLabel.Text = selectedAsset.itemName;

				if (selectedPage < PlayerInventory.SLOTS)
				{
					selectionHotkeyLabel.Text = localization.format("Hotkey_Set", ControlsSettings.getEquipmentHotkeyText(selectedPage));
					selectionHotkeyLabel.IsVisible = true;
				}
				else if (selectedPage < PlayerInventory.STORAGE && ItemTool.checkUseable(selectedPage, selectedJar.item.id))
				{
					selectionHotkeyLabel.Text = localization.format("Hotkey_Unset");
					selectionHotkeyLabel.IsVisible = true;

					for (byte hotkeyIndex = 0; hotkeyIndex < Player.LocalPlayer.equipment.hotkeys.Length; hotkeyIndex++)
					{
						HotkeyInfo hotkeyInfo = Player.LocalPlayer.equipment.hotkeys[hotkeyIndex];

						if (hotkeyInfo.page == selectedPage && hotkeyInfo.x == selected_x && hotkeyInfo.y == selected_y)
						{
							selectionHotkeyLabel.Text = localization.format("Hotkey_Set", ControlsSettings.getEquipmentHotkeyText(hotkeyIndex + 2));
							break;
						}
					}
				}
				else
				{
					selectionHotkeyLabel.IsVisible = false;
				}

				//if(asset.showQuality)
				//{
				//	selectionQualityBox.text = jar.item.quality + "%";
				//	selectionQualityBox.textColor = ItemTool.getQualityColor(jar.item.quality / 100.0f);

				//	selectionQualityBox.isVisible = true;
				//}
				//else
				//{
				//	selectionQualityBox.isVisible = false;
				//}

				if (Player.LocalPlayer.equipment.checkSelection(page, x, y))
				{
					selectionEquipButton.Text = localization.format("Dequip_Button");
					selectionEquipButton.TooltipText = localization.format("Dequip_Button_Tooltip");
				}
				else
				{
					selectionEquipButton.Text = localization.format("Equip_Button");
					selectionEquipButton.TooltipText = localization.format("Equip_Button_Tooltip");
				}

				if (selectedAsset.type == EItemType.GUN)
				{
					selectionContextButton.Text = localization.format("Attachments_Button");
					selectionContextButton.TooltipText = localization.format("Attachments_Button_Tooltip");
					selectionContextButton.IsVisible = selectedPage >= PlayerInventory.SLOTS && selectedPage < PlayerInventory.AREA;
				}
				//else if(selectedAsset.type == EItemType.SHIRT || selectedAsset.type == EItemType.PANTS || selectedAsset.type == EItemType.HAT || selectedAsset.type == EItemType.BACKPACK || selectedAsset.type == EItemType.VEST || selectedAsset.type == EItemType.GLASSES || selectedAsset.type == EItemType.MASK)
				//{
				//	selectionContextButton.text = localization.format("Wear_Button");
				//	selectionContextButton.tooltip = localization.format("Wear_Button_Tooltip");
				//	selectionContextButton.isVisible = selectedPage < PlayerInventory.AREA;
				//}
				else
				{
					selectionContextButton.IsVisible = false;
				}

				bool isPickingUp = page == PlayerInventory.AREA;
				if (isPickingUp)
				{
					selectionDropButton.Text = localization.format("Pickup_Button");
					selectionDropButton.TooltipText = localization.format("Pickup_Button_Tooltip");
				}
				else
				{
					selectionDropButton.Text = localization.format("Drop_Button");
					selectionDropButton.TooltipText = localization.format("Drop_Button_Tooltip");
				}

				if (page == PlayerInventory.STORAGE)
				{
					selectionStorageButton.Text = localization.format("Take_Button");
					selectionStorageButton.TooltipText = localization.format("Take_Button_Tooltip");
				}
				else
				{
					selectionStorageButton.Text = localization.format("Store_Button");
					selectionStorageButton.TooltipText = localization.format("Store_Button_Tooltip");
				}

				selectionEquipButton.IsVisible = selectedAsset.canPlayerEquip && page < PlayerInventory.PAGES - 2;
				selectionDropButton.IsVisible = isPickingUp || selectedAsset.allowManualDrop;
				selectionStorageButton.IsVisible = Player.LocalPlayer.inventory.isStoring;

				int offset = 0;

				if (selectionEquipButton.IsVisible)
				{
					selectionEquipButton.PositionOffset_Y = offset;
					offset += 40;
				}

				if (selectionContextButton.IsVisible)
				{
					selectionContextButton.PositionOffset_Y = offset;
					offset += 40;
				}

				if (selectionDropButton.IsVisible)
				{
					selectionDropButton.PositionOffset_Y = offset;
					offset += 40;
				}

				if (selectionStorageButton.IsVisible)
				{
					selectionStorageButton.PositionOffset_Y = offset;
					offset += 40;
				}

				selectionExtraActionsBox.RemoveAllChildren();
				selectionExtraActionsBox.PositionOffset_Y = offset;

				int extras = 0;

				if (page != PlayerInventory.AREA)
				{
					foreach (Action action in selectedAsset.actions)
					{
						if (action.type == EActionType.BLUEPRINT)
						{
							if (page < PlayerInventory.SLOTS)
							{
								continue;
							}

							if (page >= PlayerInventory.STORAGE)
							{
								continue;
							}

							IBlueprintOwner blueprintOwner = action.FindBlueprintOwnerAsset() as IBlueprintOwner;
							if (blueprintOwner == null)
							{
								continue;
							}

							Blueprint blueprint = action.blueprints[0].FindBlueprint(blueprintOwner);

							if (Player.LocalPlayer.crafting.IsBlueprintPermanentlyDisabled(blueprint))
							{
								continue;
							}

							if (blueprint.Operation == EBlueprintOperation.RepairTargetItem && selectedJar.item.quality >= 100)
							{
								continue; // hide repair option on fully repaired items
							}

							if (!blueprint.areConditionsMet(Player.LocalPlayer))
							{
								// Some maps have similar actions on some of items where only one should be visible at a time.
								continue;
							}
						}

						SleekItemActionButton selectionActionButton = new SleekItemActionButton(action);
						selectionActionButton.PositionOffset_Y = extras;
						selectionActionButton.SizeScale_X = 1.0f;
						selectionActionButton.SizeOffset_Y = 30;
						selectionExtraActionsBox.AddChild(selectionActionButton);

						extras += 40;
						offset += 40;
					}
				}

				selectionExtraActionsBox.SizeOffset_Y = extras - 10;
				selectionActionsBox.ContentSizeOffset = new Vector2(0.0f, offset - 10);
				selectionNameLabel.TextColor = ItemTool.getRarityColorUI(selectedAsset.rarity);
			}
		}

		public static void closeSelection()
		{
			if (selectedPage == 255)
			{
				return;
			}

			_selectedPage = 255;
			_selected_x = 255;
			_selected_y = 255;

			if (!Glazier.Get().SupportsDepth)
			{
				setItemsEnabled(true);
				setMiscButtonsEnabled(true);
			}

			selectionFrame.IsVisible = false;
			//
			//			selectionBox.lerpPositionOffset(20, 0, ESleekLerp.EXPONENTIAL, 20);
			//			selectionBox.lerpPositionScale(1, 0, ESleekLerp.EXPONENTIAL, 20);
		}

		// Called when right clicking on item
		private static void onSelectedItem(byte page, byte x, byte y)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (isDragging)
			{
				UnturnedLog.warn("onSelectedItem should not happen during drag after glazier refactor!");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (page == 255 || (page == selectedPage && x == selected_x && y == selected_y))
			{
				closeSelection();
			}
			else
			{
				if (InputEx.GetKey(ControlsSettings.other))
				{
					ItemJar jar = Player.LocalPlayer.inventory.getItem(page, Player.LocalPlayer.inventory.getIndex(page, x, y));
					if (jar == null)
						return;

					if (Player.LocalPlayer.inventory.isStoring)
					{
						if (page == PlayerInventory.AREA)
						{
							if (jar.interactableItem != null)
							{
								ItemManager.takeItem(jar.interactableItem.transform.parent, 255, 255, 0, PlayerInventory.STORAGE);
							}
						}
						else if (page == PlayerInventory.STORAGE)
						{
							byte newPage;
							byte new_x;
							byte new_y;
							byte newRot;
							if (Player.LocalPlayer.inventory.tryFindSpace(jar.size_x, jar.size_y, out newPage, out new_x, out new_y, out newRot))
							{
								PlayInventoryAudio(jar.GetAsset());

								Player.LocalPlayer.inventory.sendDragItem(page, x, y, newPage, new_x, new_y, newRot);
							}
						}
						else
						{
							byte new_x;
							byte new_y;
							byte newRot;
							if (Player.LocalPlayer.inventory.tryFindSpace(PlayerInventory.STORAGE, jar.size_x, jar.size_y, out new_x, out new_y, out newRot))
							{
								PlayInventoryAudio(jar.GetAsset());

								Player.LocalPlayer.inventory.sendDragItem(page, x, y, PlayerInventory.STORAGE, new_x, new_y, newRot);
							}
						}
					}
					else
					{
						checkAction(page, x, y, jar);
					}
				}
				else
				{
					openSelection(page, x, y);
				}
			}
		}

		// checks whether the slot has room for the item from page, x, y
		private static bool checkSlot(byte page, byte x, byte y, ItemJar jar, byte slot)
		{
			// check whether we can drag the item into the slot
			if (Player.LocalPlayer.inventory.checkSpaceEmpty(slot, 255, 255, 0, 0, 0))
			{
				// Doesn't PlayInventoryAudio because there may be Equip audio.

				Player.LocalPlayer.inventory.sendDragItem(page, x, y, slot, 0, 0, 0);
				Player.LocalPlayer.equipment.ClientEquipAfterItemDrag(slot, 0, 0);

				PlayerDashboardUI.close();
				PlayerLifeUI.open();

				return true;
			}
			else
			{
				// move other selected item into bag
				ItemJar existingJar = Player.LocalPlayer.inventory.getItem(slot, 0);

				byte rot_1 = existingJar.rot;
				if (!Player.LocalPlayer.inventory.checkSpaceSwap(page, x, y, jar.size_x, jar.size_y, jar.rot, existingJar.size_x, existingJar.size_y, rot_1))
				{
					// Try rotating existing item.
					rot_1 = (byte) ((rot_1 + 1) % 4);
					if (!Player.LocalPlayer.inventory.checkSpaceSwap(page, x, y, jar.size_x, jar.size_y, jar.rot, existingJar.size_x, existingJar.size_y, rot_1))
					{
						// No space.
						return false;
					}
				}

				// Doesn't PlayInventoryAudio because there may be Equip audio.

				Player.LocalPlayer.inventory.sendSwapItem(page, x, y, rot_1, slot, 0, 0, jar.rot);
				Player.LocalPlayer.equipment.ClientEquipAfterItemDrag(slot, 0, 0);

				PlayerDashboardUI.close();
				PlayerLifeUI.open();

				return true;
			}
		}

		private static void checkEquip(byte page, byte x, byte y, ItemJar jar, byte slot)
		{
			if (page == PlayerInventory.AREA)
			{
				if (page == selectedPage && x == selected_x && y == selected_y)
				{
					closeSelection();
				}

				if (jar.interactableItem != null)
				{
					ItemManager.takeItem(jar.interactableItem.transform.parent, 255, 255, 0, 255);
				}
				return;
			}

			if (!Player.LocalPlayer.equipment.checkSelection(page, x, y))
			{
				ItemAsset asset = jar.GetAsset();

				if (asset != null)
				{
					if (asset.canPlayerEquip && asset.slot.canEquipInPage(page))
					{
						Player.LocalPlayer.equipment.equip(page, x, y);

						PlayerDashboardUI.close();
						PlayerLifeUI.open();
					}
					else
					{
						if (asset.slot == ESlotType.PRIMARY)
						{
							if (!checkSlot(page, x, y, jar, 0))
							{
								return;
							}
						}
						else if (asset.slot == ESlotType.SECONDARY)
						{
							if (slot == 255)
							{
								if (!checkSlot(page, x, y, jar, 1))
								{
									if (!checkSlot(page, x, y, jar, 0))
									{
										return;
									}
								}
							}
							else
							{
								if (!checkSlot(page, x, y, jar, slot))
								{
									return;
								}
							}
						}
					}
				}
			}
			else if (Player.LocalPlayer.equipment.HasValidUseable && !Player.LocalPlayer.equipment.isBusy && Player.LocalPlayer.equipment.IsEquipAnimationFinished)
			{
				Player.LocalPlayer.equipment.dequip();

				if (page == selectedPage && x == selected_x && y == selected_y)
				{
					closeSelection();
				}
			}
		}

		private static void checkAction(byte page, byte x, byte y, ItemJar jar)
		{
			if (page == PlayerInventory.AREA)
			{
				if (jar.interactableItem != null)
				{
					ItemManager.takeItem(jar.interactableItem.transform.parent, 255, 255, 0, 255);
				}
				return;
			}

			ItemAsset asset = jar.GetAsset();
			if (asset == null)
				return;

			if (asset.type == EItemType.HAT)
			{
				Player.LocalPlayer.clothing.sendSwapHat(page, x, y);
			}
			else if (asset.type == EItemType.SHIRT)
			{
				Player.LocalPlayer.clothing.sendSwapShirt(page, x, y);
			}
			else if (asset.type == EItemType.PANTS)
			{
				Player.LocalPlayer.clothing.sendSwapPants(page, x, y);
			}
			else if (asset.type == EItemType.BACKPACK)
			{
				Player.LocalPlayer.clothing.sendSwapBackpack(page, x, y);
			}
			else if (asset.type == EItemType.VEST)
			{
				Player.LocalPlayer.clothing.sendSwapVest(page, x, y);
			}
			else if (asset.type == EItemType.MASK)
			{
				Player.LocalPlayer.clothing.sendSwapMask(page, x, y);
			}
			else if (asset.type == EItemType.GLASSES)
			{
				Player.LocalPlayer.clothing.sendSwapGlasses(page, x, y);
			}
			else if (asset.canPlayerEquip)
			{
				checkEquip(page, x, y, jar, 255);
			}
		}

		// Called when left clicking on an item
		private static void onGrabbedItem(byte page, byte x, byte y, SleekItem item)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (isDragging)
			{
				UnturnedLog.warn("onGrabbedItem should not happen during drag after glazier refactor!");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (InputEx.GetKey(ControlsSettings.other))
			{
				if (page == PlayerInventory.AREA)
				{
					if (item.jar.interactableItem == null)
					{
						UnturnedLog.warn("onGrabbedItem nearby without interactable");
						return;
					}

					ItemManager.takeItem(item.jar.interactableItem.transform.parent, 255, 255, 0, 255);
				}
				else
				{
					Player.LocalPlayer.inventory.sendDropItem(page, x, y);
				}
				return;
			}

			dragJar = Player.LocalPlayer.inventory.getItem(page, Player.LocalPlayer.inventory.getIndex(page, x, y));
			if (dragJar == null)
				return;

			dragSource = item;

			dragFromPage = page;
			dragFrom_x = x;
			dragFrom_y = y;
			dragFromRot = dragJar.rot;

			dragOffset = -item.GetNormalizedCursorPosition();
			dragOffset.x *= item.SizeOffset_X;
			dragOffset.y *= item.SizeOffset_Y;

			if (dragJar.rot == 1)
			{
				float temp = dragOffset.x;
				dragOffset.x = dragOffset.y;
				dragOffset.y = -((dragJar.size_y * 50) + temp);
			}
			else if (dragJar.rot == 2)
			{
				dragOffset.x = -((dragJar.size_x * 50) + dragOffset.x);
				dragOffset.y = -((dragJar.size_y * 50) + dragOffset.y);
			}
			else if (dragJar.rot == 3)
			{
				float temp = dragOffset.x;
				dragOffset.x = -((dragJar.size_x * 50) + dragOffset.y);
				dragOffset.y = temp;
			}

			updatePivot();

			dragItem.updateItem(dragJar);
			refreshDraggedVisualPosition();

			startDrag();
		}

		// Called then clicking on the grid
		private static void onPlacedItem(byte page, byte x, byte y)
		{
			ConsumeEvent();

			if (dragSource != null && isDragging)
			{
				if (page >= PlayerInventory.SLOTS)
				{
					int offset_x = x + (int) (dragPivot.x / 50f);
					int offset_y = y + (int) (dragPivot.y / 50f);

					if (offset_x < 0)
					{
						offset_x = 0;
					}

					if (offset_y < 0)
					{
						offset_y = 0;
					}

					byte size_x = dragJar.size_x;
					byte size_y = dragJar.size_y;
					if (dragJar.rot % 2 == 1)
					{
						size_x = dragJar.size_y;
						size_y = dragJar.size_x;
					}

					if (offset_x >= Player.LocalPlayer.inventory.getWidth(page) - size_x)
					{
						offset_x = (byte) (Player.LocalPlayer.inventory.getWidth(page) - size_x);
					}

					if (offset_y >= Player.LocalPlayer.inventory.getHeight(page) - size_y)
					{
						offset_y = (byte) (Player.LocalPlayer.inventory.getHeight(page) - size_y);
					}

					x = (byte) offset_x;
					y = (byte) offset_y;
				}

				ItemAsset asset = dragJar.GetAsset();
				if (asset == null)
				{
					return;
				}

				if (page < PlayerInventory.SLOTS && asset.slot.canEquipInPage(page) == false)
				{
					return;
				}

				if (dragFromPage == page && dragFrom_x == x && dragFrom_y == y && dragFromRot == dragJar.rot)
				{
					stopDrag();

					return;
				}

				if (page == PlayerInventory.AREA)
				{
					stopDrag();

					if (page != dragFromPage)
					{
						Player.LocalPlayer.inventory.sendDropItem(dragFromPage, dragFrom_x, dragFrom_y);
					}

					return;
				}

				if (dragFromPage == PlayerInventory.AREA)
				{
					byte rot = dragJar.rot;

					stopDrag();

					if (page != dragFromPage)
					{
						if (Player.LocalPlayer.inventory.checkSpaceEmpty(page, x, y, dragJar.size_x, dragJar.size_y, rot))
						{
							// May have been destroyed during drag.
							if (dragItem.jar != null && dragItem.jar.interactableItem != null)
							{
								ItemManager.takeItem(dragItem.jar.interactableItem.transform.parent, x, y, rot, page);
							}
						}
					}

					return;
				}

				if (Player.LocalPlayer.inventory.checkSpaceDrag(page, dragFrom_x, dragFrom_y, dragFromRot, x, y, dragJar.rot, dragJar.size_x, dragJar.size_y, page == dragFromPage))
				{
					byte rot = dragJar.rot;

					stopDrag();

					Player.LocalPlayer.inventory.sendDragItem(dragFromPage, dragFrom_x, dragFrom_y, page, x, y, rot);

					if (page < PlayerInventory.SLOTS)
					{
						Player.LocalPlayer.equipment.equip(page, 0, 0);

						PlayerDashboardUI.close();
						PlayerLifeUI.open();
					}
				}
				else
				{
					if (page < PlayerInventory.SLOTS)
					{
						stopDrag();

						checkEquip(dragFromPage, dragFrom_x, dragFrom_y, dragJar, page);
					}
					else
					{
						byte find_x;
						byte find_y;
						byte index = Player.LocalPlayer.inventory.findIndex(page, x, y, out find_x, out find_y);

						if (index == 255)
						{
							return;
						}

						if (dragFromPage == page && dragFrom_x == find_x && dragFrom_y == find_y)
						{
							stopDrag();

							return;
						}

						ItemJar other = Player.LocalPlayer.inventory.getItem(page, index);

						if (Player.LocalPlayer.inventory.checkSpaceSwap(page, find_x, find_y, other.size_x, other.size_y, other.rot, dragJar.size_x, dragJar.size_y, dragJar.rot))
						{
							byte rot_1 = other.rot;
							if (!Player.LocalPlayer.inventory.checkSpaceSwap(dragFromPage, dragFrom_x, dragFrom_y, dragJar.size_x, dragJar.size_y, dragFromRot, other.size_x, other.size_y, rot_1))
							{
								// Try rotating other item.
								rot_1 = (byte) ((rot_1 + 1) % 4);
								if (!Player.LocalPlayer.inventory.checkSpaceSwap(dragFromPage, dragFrom_x, dragFrom_y, dragJar.size_x, dragJar.size_y, dragFromRot, other.size_x, other.size_y, rot_1))
								{
									// No space.
									return;
								}
							}

							ItemAsset allowed = other.GetAsset();

							if (allowed != null && (dragFromPage >= PlayerInventory.SLOTS || allowed.slot.canEquipInPage(dragFromPage)))
							{
								byte rot_0 = dragJar.rot; // Save before stopDrag.

								stopDrag();

								Player.LocalPlayer.inventory.sendSwapItem(page, find_x, find_y, rot_0, dragFromPage, dragFrom_x, dragFrom_y, rot_1);

								if (dragFromPage < PlayerInventory.SLOTS)
								{
									checkEquip(dragFromPage, dragFrom_x, dragFrom_y, dragJar, page);
								}
							}
						}
					}
				}
			}
		}

		private static void onClickedCharacter()
		{
			if (dragSource != null && isDragging)
			{
				byte page = dragFromPage;
				byte x = dragFrom_x;
				byte y = dragFrom_y;
				ItemJar jar = dragJar;

				stopDrag();

				checkAction(page, x, y, jar);
			}
			else
			{
				Vector2 normalizedMousePosition = characterImage.GetNormalizedCursorPosition();
				Vector3 viewportPosition = new Vector3(normalizedMousePosition.x, 1.0f - normalizedMousePosition.y, 0.0f);
				Ray ray = Player.LocalPlayer.look.characterCamera.ViewportPointToRay(viewportPosition);

				RaycastHit hit;
				Physics.Raycast(ray, out hit, 8f, RayMasks.CLOTHING_INTERACT);

#if WITH_INVENTORY_CLICK_GIZMOS
				RuntimeGizmos.Get().Raycast(ray, 8.0f, hit, Color.green, Color.red, 10.0f);
#endif // WITH_INVENTORY_CLICK_GIZMOS

				if (hit.collider != null)
				{
					Transform colliderTransform = hit.collider.transform;
					if (colliderTransform.CompareTag("Player"))
					{
						ELimb limb = DamageTool.getLimb(colliderTransform);

						if (limb == ELimb.LEFT_FOOT || limb == ELimb.LEFT_LEG || limb == ELimb.RIGHT_FOOT || limb == ELimb.RIGHT_LEG)
						{
							Player.LocalPlayer.clothing.sendSwapPants(255, 255, 255);
						}
						else if (limb == ELimb.LEFT_HAND || limb == ELimb.LEFT_ARM || limb == ELimb.RIGHT_HAND || limb == ELimb.RIGHT_ARM || limb == ELimb.SPINE)
						{
							Player.LocalPlayer.clothing.sendSwapShirt(255, 255, 255);
						}
					}
					else if (colliderTransform.CompareTag("Enemy"))
					{
						if (colliderTransform.name == "Hat")
						{
							Player.LocalPlayer.clothing.sendSwapHat(255, 255, 255);
						}
						else if (colliderTransform.name == "Glasses")
						{
							Player.LocalPlayer.clothing.sendSwapGlasses(255, 255, 255);
						}
						else if (colliderTransform.name == "Mask")
						{
							Player.LocalPlayer.clothing.sendSwapMask(255, 255, 255);
						}
						else if (colliderTransform.name == "Vest")
						{
							Player.LocalPlayer.clothing.sendSwapVest(255, 255, 255);
						}
						else if (colliderTransform.name == "Backpack")
						{
							Player.LocalPlayer.clothing.sendSwapBackpack(255, 255, 255);
						}
					}
					else if (colliderTransform.CompareTag("Item"))
					{
						Player.LocalPlayer.equipment.dequip();
					}
				}
			}

			ConsumeEvent();
		}

		private static void onClickedOutsideSelection()
		{
			closeSelection();
		}

		private static void onClickedDuringDrag()
		{
			if (dragSource != null && isDragging)
			{
				byte page = dragFromPage;
				byte x = dragFrom_x;
				byte y = dragFrom_y;

				stopDrag();

				if (page != PlayerInventory.AREA)
				{
					Player.LocalPlayer.inventory.sendDropItem(page, x, y);
				}

				ConsumeEvent();
			}
		}

		private static void onRightClickedDuringDrag()
		{
			if (dragSource != null && isDragging)
			{
				stopDrag();
			}
		}

		private static void onItemDropAdded(Transform model, InteractableItem interactableItem)
		{
			if (!active || !PlayerDashboardUI.active)
			{
				return;
			}

			if (Player.LocalPlayer == null)
			{
				return;
			}

			if (areaItems.getItemCount() >= 200)
			{
				return;
			}

			Vector3 eyesPosition = Player.LocalPlayer.look.GetEyesPositionWithoutLeaning();
			if ((model.position - eyesPosition).sqrMagnitude > 16)
			{
				return;
			}

			pendingItemsInRadius.Add(interactableItem);
		}

		private static void onItemDropRemoved(Transform model, InteractableItem interactableItem)
		{
			if (!active || !PlayerDashboardUI.active)
			{
				return;
			}

			ItemJar jar = interactableItem.jar;

			if (jar == null)
			{
				return;
			}

			int index = areaItems.FindIndexOfJar(jar);
			if (index < 0)
			{
				pendingItemsInRadius.RemoveFast(interactableItem);
				return;
			}

			areaItems.removeItem((byte) index);
			items[PlayerInventory.AREA - PlayerInventory.SLOTS].removeItem(jar);
		}

		private static void onSeated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle)
		{
			if (oldVehicle != null)
			{
				oldVehicle.onPassengersUpdated -= updateVehicle;
				oldVehicle.onLockUpdated -= onVehicleLockUpdated;
				oldVehicle.onHeadlightsUpdated -= updateVehicle;
				oldVehicle.onSirensUpdated -= updateVehicle;
				oldVehicle.onBlimpUpdated -= updateVehicle;
				oldVehicle.batteryChanged -= updateVehicle;
				oldVehicle.skinChanged -= updateVehicle;
			}

			if (newVehicle != null)
			{
				newVehicle.onPassengersUpdated += updateVehicle;
				newVehicle.onLockUpdated += onVehicleLockUpdated;
				newVehicle.onHeadlightsUpdated += updateVehicle;
				newVehicle.onSirensUpdated += updateVehicle;
				newVehicle.onBlimpUpdated += updateVehicle;
				newVehicle.batteryChanged += updateVehicle;
				newVehicle.skinChanged += updateVehicle;
			}

			updateVehicle();
		}

		private static void onVehicleLockUpdated()
		{
			updateVehicle();

			InteractableVehicle vehicle = Player.LocalPlayer.movement.getVehicle();
			if (vehicle == null)
				return;

			PlayerUI.message(vehicle.isLocked ? EPlayerMessage.VEHICLE_LOCKED : EPlayerMessage.VEHICLE_UNLOCKED, string.Empty);
		}

		private static void updateVehicle()
		{
			if (!active)
			{
				return;
			}

			InteractableVehicle vehicle = Player.LocalPlayer.movement.getVehicle();

			if (vehicle != null && vehicle.asset != null)
			{
				VehicleAsset asset = vehicle.asset;

				vehicleNameLabel.Text = asset.vehicleName;
				vehicleNameLabel.TextColor = ItemTool.getRarityColorUI(asset.rarity);

				int offsetActions = 0;
				int offsetPassengers = 0;

				if (asset.canBeLocked)
				{
					vehicleLockButton.Text = localization.format(vehicle.isLocked ? "Vehicle_Lock_Off" : "Vehicle_Lock_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
					vehicleLockButton.TooltipText = localization.format("Vehicle_Lock_Tooltip");

					vehicleLockButton.IsVisible = true;
					vehicleLockButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleLockButton.IsVisible = false;
				}

				if (asset.hasHorn)
				{
					vehicleHornButton.Text = localization.format("Vehicle_Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
					vehicleHornButton.TooltipText = localization.format("Vehicle_Horn_Tooltip");

					vehicleHornButton.IsVisible = true;
					vehicleHornButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleHornButton.IsVisible = false;
				}

				if (asset.hasHeadlights)
				{
					vehicleHeadlightsButton.Text = localization.format(vehicle.headlightsOn ? "Vehicle_Headlights_Off" : "Vehicle_Headlights_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary));
					vehicleHeadlightsButton.TooltipText = localization.format("Vehicle_Headlights_Tooltip");

					vehicleHeadlightsButton.IsVisible = true;
					vehicleHeadlightsButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleHeadlightsButton.IsVisible = false;
				}

				if (asset.hasSirens)
				{
					vehicleSirensButton.Text = localization.format(vehicle.sirensOn ? "Vehicle_Sirens_Off" : "Vehicle_Sirens_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
					vehicleSirensButton.TooltipText = localization.format("Vehicle_Sirens_Tooltip");

					vehicleSirensButton.IsVisible = true;
					vehicleSirensButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleSirensButton.IsVisible = false;
				}

				if (asset.engine == EEngine.BLIMP)
				{
					vehicleBlimpButton.Text = localization.format(vehicle.isBlimpFloating ? "Vehicle_Blimp_Off" : "Vehicle_Blimp_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
					vehicleBlimpButton.TooltipText = localization.format("Vehicle_Blimp_Tooltip");

					vehicleBlimpButton.IsVisible = true;
					vehicleBlimpButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleBlimpButton.IsVisible = false;
				}

				if (asset.hasHook)
				{
					vehicleHookButton.Text = localization.format("Vehicle_Hook", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
					vehicleHookButton.TooltipText = localization.format("Vehicle_Hook_Tooltip");

					vehicleHookButton.IsVisible = true;
					vehicleHookButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleHookButton.IsVisible = false;
				}

				if (vehicle.usesBattery && vehicle.ContainsBatteryItem && vehicle.asset.canStealBattery)
				{
					vehicleStealBatteryButton.Text = localization.format("Vehicle_Steal_Battery");
					vehicleStealBatteryButton.TooltipText = localization.format("Vehicle_Steal_Battery_Tooltip");

					vehicleStealBatteryButton.IsVisible = true;
					vehicleStealBatteryButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleStealBatteryButton.IsVisible = false;
				}

				bool isSkinned;
				bool showSkinButton;

				int item = 0;
				ushort skinID = 0;
				ushort mythicID = 0;
				if (Player.LocalPlayer.channel.owner.skinItems != null && Player.LocalPlayer.channel.owner.GetVehicleSkinItemDefId(vehicle, out item))
				{
					skinID = Provider.provider.economyService.getInventorySkinID(item);
					mythicID = Provider.provider.economyService.getInventoryMythicID(item);
				}

				if (skinID != 0)
				{
					if (skinID == vehicle.skinID && mythicID == vehicle.mythicID) // Our paintjob is already applied so remove it
					{
						isSkinned = true;
						showSkinButton = true;
					}
					else // Our paintjob isn't applied so we're going to put it on
					{
						isSkinned = false;
						showSkinButton = true;
					}
				}
				else
				{
					if (vehicle.isSkinned) // We don't have a skin but we can remove the existing one
					{
						isSkinned = true;
						showSkinButton = true;
					}
					else // Already not skinned so there's nothing to do
					{
						isSkinned = false;
						showSkinButton = false;
					}
				}

				if (showSkinButton)
				{
					vehicleSkinButton.Text = localization.format(isSkinned ? "Vehicle_Skin_Off" : "Vehicle_Skin_On");
					vehicleSkinButton.TooltipText = localization.format("Vehicle_Skin_Tooltip");

					vehicleSkinButton.IsVisible = true;
					vehicleSkinButton.PositionOffset_Y = offsetActions;
					offsetActions += 40;
				}
				else
				{
					vehicleSkinButton.IsVisible = false;
				}

				if (Player.LocalPlayer.stance.stance == EPlayerStance.DRIVING)
				{
					vehiclePassengersBox.PositionOffset_X = 270;
					vehiclePassengersBox.SizeOffset_X = -280;

					vehicleActionsBox.IsVisible = true;
				}
				else
				{
					vehiclePassengersBox.PositionOffset_X = 10;
					vehiclePassengersBox.SizeOffset_X = -20;

					vehicleActionsBox.IsVisible = false;
				}

				vehiclePassengersBox.RemoveAllChildren();

				for (int index = 0; index < vehicle.passengers.Length; index++)
				{
					Passenger passenger = vehicle.passengers[index];

					ISleekButton passengerButton = Glazier.Get().CreateButton();
					passengerButton.PositionOffset_Y = offsetPassengers;
					passengerButton.SizeOffset_Y = 30;
					passengerButton.SizeScale_X = 1.0f;
					passengerButton.OnClicked += onClickedVehiclePassengerButton;
					vehiclePassengersBox.AddChild(passengerButton);

					if (passenger.player != null)
					{
						string name = passenger.player.GetLocalDisplayName();
						if (index < 12)
						{
							passengerButton.Text = localization.format("Vehicle_Seat_Slot", name, "F" + (index + 1));
						}
						else
						{
							passengerButton.Text = name;
						}
					}
					else
					{
						if (index < 12)
						{
							passengerButton.Text = localization.format("Vehicle_Seat_Slot", localization.format("Vehicle_Seat_Empty"), "F" + (index + 1));
						}
						else
						{
							passengerButton.Text = localization.format("Vehicle_Seat_Empty");
						}
					}

					offsetPassengers += 40;
				}

				vehicleActionsBox.SizeOffset_Y = offsetActions - 10;
				vehiclePassengersBox.SizeOffset_Y = offsetPassengers - 10;
				//vehiclePassengersScrollBox.area = new Vector2(5, offsetPassengers - 10);

				vehicleBox.IsVisible = true;

				int offset = Mathf.Max(offsetActions, offsetPassengers);
				vehicleBox.SizeOffset_Y = 60 + offset;

				headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].TextColor = vehicleNameLabel.TextColor;
				headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].Text = localization.format("Storage_Trunk", vehicleNameLabel.Text);
			}
			else
			{
				vehicleBox.IsVisible = false;

				headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].TextColor = ESleekTint.FONT;
				headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].Text = localization.format("Storage");
			}

			updateBoxAreas();
		}

		private static void resetNearbyDrops()
		{
			Vector3 eyesPosition = Player.LocalPlayer.look.GetEyesPositionWithoutLeaning();
			pendingItemsInRadius.Clear();
			ItemManager.findSimulatedItemsInRadius(eyesPosition, 16.0f, pendingItemsInRadius);

			areaItems.clear();
			areaItems.resize(8, 3);
			Player.LocalPlayer.inventory.replaceItems(PlayerInventory.AREA, areaItems);

			SleekItems itemsUI = items[PlayerInventory.AREA - PlayerInventory.SLOTS];
			itemsUI.clear();
			itemsUI.resize(areaItems.width, areaItems.height);

			updateBoxAreas();
		}

		private static void onInventoryResized(byte page, byte newWidth, byte newHeight)
		{
			if (page < PlayerInventory.SLOTS)
			{
				return;
			}

			page -= PlayerInventory.SLOTS;

			items[page].resize(newWidth, newHeight);

			if (page > 0)
			{
				headers[page].IsVisible = newHeight > 0;
			}

			items[page].IsVisible = newHeight > 0;

			if (page == PlayerInventory.STORAGE - PlayerInventory.SLOTS && newHeight == 0)
			{
				items[page].clear();
			}

			updateBoxAreas();
		}

		private static void updateBoxAreas()
		{
			Profiler.BeginSample("UpdateBoxAreas");

			float offsetClothing_y = 0;
			float offsetArea_y = 0;

			bool cachedIsSplitClothingArea = isSplitClothingArea;

			if (vehicleBox.IsVisible)
			{
				if (cachedIsSplitClothingArea)
				{
					if (vehicleBox.Parent != areaBox)
					{
						areaBox.AddChild(vehicleBox);
					}

					vehicleBox.PositionOffset_Y = offsetArea_y;
					offsetArea_y += vehicleBox.SizeOffset_Y + 10;
				}
				else
				{
					if (vehicleBox.Parent != clothingBox)
					{
						clothingBox.AddChild(vehicleBox);
					}

					vehicleBox.PositionOffset_Y = offsetClothing_y;
					offsetClothing_y += vehicleBox.SizeOffset_Y + 10;
				}
			}

			for (byte index = 0; index < items.Length; index++)
			{
				if (headers[index].IsVisible)
				{
					if (cachedIsSplitClothingArea && (index == PlayerInventory.STORAGE - PlayerInventory.SLOTS || index == PlayerInventory.AREA - PlayerInventory.SLOTS))
					{
						if (headers[index].Parent != areaBox)
						{
							areaBox.AddChild(headers[index]);
						}

						if (items[index].Parent != areaBox)
						{
							areaBox.AddChild(items[index]);
						}

						headers[index].PositionOffset_Y = offsetArea_y;
						items[index].PositionOffset_Y = offsetArea_y + 70;

						offsetArea_y += items[index].SizeOffset_Y + 80;
					}
					else
					{
						if (headers[index].Parent != clothingBox)
						{
							clothingBox.AddChild(headers[index]);
						}

						if (items[index].Parent != clothingBox)
						{
							clothingBox.AddChild(items[index]);
						}

						headers[index].PositionOffset_Y = offsetClothing_y;
						items[index].PositionOffset_Y = offsetClothing_y + 70;

						offsetClothing_y += items[index].SizeOffset_Y + 80;
					}
				}
			}

			headers[7].IsVisible = Player.LocalPlayer.clothing.hatAsset != null;
			if (headers[7].IsVisible)
			{
				headers[7].PositionOffset_Y = offsetClothing_y;
				offsetClothing_y += 70;
			}

			headers[8].IsVisible = Player.LocalPlayer.clothing.maskAsset != null;
			if (headers[8].IsVisible)
			{
				headers[8].PositionOffset_Y = offsetClothing_y;
				offsetClothing_y += 70;
			}

			headers[9].IsVisible = Player.LocalPlayer.clothing.glassesAsset != null;
			if (headers[9].IsVisible)
			{
				headers[9].PositionOffset_Y = offsetClothing_y;
				offsetClothing_y += 70;
			}

			clothingBox.ContentSizeOffset = new Vector2(0.0f, offsetClothing_y - 10);
			areaBox.ContentSizeOffset = new Vector2(0.0f, offsetArea_y - 10);

			InteractableStorage storage = PlayerInteract.interactable as InteractableStorage;

			if (storage != null && storage.isDisplay)
			{
				headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].SizeOffset_X = -180;

				rot_xButton.IsVisible = true;
				rot_yButton.IsVisible = true;
				rot_zButton.IsVisible = true;
			}
			else
			{
				headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].SizeOffset_X = 0;

				rot_xButton.IsVisible = false;
				rot_yButton.IsVisible = false;
				rot_zButton.IsVisible = false;
			}

			Profiler.EndSample();
		}

		// New hotkey system
		private static void updateHotkeys()
		{
			for (byte page = 0; page < PlayerInventory.STORAGE - PlayerInventory.SLOTS; page++)
			{
				SleekItems pageItems = items[page];
				pageItems.resetHotkeyDisplay();
			}

			for (byte hotkeyIndex = 0; hotkeyIndex < Player.LocalPlayer.equipment.hotkeys.Length; hotkeyIndex++)
			{
				HotkeyInfo hotkeyInfo = Player.LocalPlayer.equipment.hotkeys[hotkeyIndex];
				byte button = (byte) (hotkeyIndex + 2);
				byte page = (byte) (hotkeyInfo.page - 2);

				if (hotkeyInfo.id == 0)
				{
					continue;
				}

				byte index = Player.LocalPlayer.inventory.getIndex(hotkeyInfo.page, hotkeyInfo.x, hotkeyInfo.y);
				ItemJar jar = Player.LocalPlayer.inventory.getItem(hotkeyInfo.page, index);

				if (jar == null || jar.item.id != hotkeyInfo.id)
				{
					hotkeyInfo.id = 0;
					hotkeyInfo.page = 255;
					hotkeyInfo.x = 255;
					hotkeyInfo.y = 255;
					continue;
				}

				items[page].updateHotkey(jar, button);
			}
		}

		private static void onHotkeysUpdated()
		{
			updateHotkeys();
		}

		// Old hotkey system
		//public static void hotkey(byte button)
		//{
		//	if(selectedPage == PlayerInventory.SLOTS)
		//	{
		//		for(int index = 0; index < items[0].items.Count; index ++)
		//		{
		//			SleekItem item = items[0].items[index];

		//			if(item.jar.x == selected_x && item.jar.y == selected_y)
		//			{
		//				if(ItemTool.checkUseable(PlayerInventory.SLOTS, item.jar.item.id))
		//				{
		//					for(int step = 0; step < items[0].items.Count; step ++)
		//					{
		//						SleekItem other = items[0].items[step];

		//						if(other.hotkey == button)
		//						{
		//							other.updateHotkey(255);
		//						}
		//					}

		//					item.updateHotkey(button);

		//					closeSelection();
		//					return;
		//				}
		//			}
		//		}
		//	}
		//	else
		//	{
		//		for(int index = 0; index < items[0].items.Count; index ++)
		//		{
		//			SleekItem item = items[0].items[index];

		//			if(item.hotkey == button)
		//			{
		//				item.updateHotkey(255);
		//			}
		//		}
		//	}
		//}

		//public static ItemJar key(byte button)
		//{
		//	for(int index = 0; index < items[0].items.Count; index ++)
		//	{
		//		SleekItem item = items[0].items[index];

		//		if(item.hotkey == button)
		//		{
		//			return item.jar;
		//		}
		//	}

		//	return null;
		//}

		// Initial release hotkey system?
		//		private static void updateHotkeys()
		//		{
		//			byte hotkey = 2;
		//			for(byte index = 0; index < items[0].items.Count; index ++)
		//			{
		//				SleekItem item = items[0].items[index];
		//
		//				if(ItemTool.checkUseable(PlayerInventory.SLOTS, item.jar.item.id))
		//				{
		//					items[0].items[index].updateHotkey(hotkey);
		//				
		//					hotkey ++;
		//
		//					if(hotkey >= 9)
		//					{
		//						return;
		//					}
		//				}
		//			}
		//		}

		private static void onInventoryUpdated(byte page, byte index, ItemJar jar)
		{
			if (page < PlayerInventory.SLOTS)
			{
				slots[page].updateItem(jar);
			}
			else
			{
				page -= PlayerInventory.SLOTS;
				items[page].updateItem(jar);
			}
		}

		private static void onInventoryAdded(byte page, byte index, ItemJar jar)
		{
			if (page < PlayerInventory.SLOTS)
			{
				slots[page].applyItem(jar);
			}
			else
			{
				page -= PlayerInventory.SLOTS;
				items[page].addItem(jar);
			}
		}

		private static void onInventoryRemoved(byte page, byte index, ItemJar jar)
		{
			if (page == selectedPage && jar.x == selected_x && jar.y == selected_y)
			{
				closeSelection();
			}

			if (page < PlayerInventory.SLOTS)
			{
				slots[page].applyItem(null);
			}
			else
			{
				page -= PlayerInventory.SLOTS;
				items[page].removeItem(jar);
			}
		}

		private static void onInventoryStored()
		{
			if (Player.LocalPlayer.inventory.shouldStorageOpenDashboard)
			{
				PlayerLifeUI.close();
				PlayerPauseUI.close();

				if (PlayerDashboardUI.active)
				{
					PlayerDashboardCraftingUI.close();
					PlayerDashboardSkillsUI.close();
					PlayerDashboardInformationUI.close();

					PlayerDashboardInventoryUI.open();
				}
				else
				{
					PlayerDashboardInventoryUI.active = true;
					PlayerDashboardCraftingUI.active = false;
					PlayerDashboardSkillsUI.active = false;
					PlayerDashboardInformationUI.active = false;

					PlayerDashboardUI.open();
				}
			}

			if (!isSplitClothingArea)
			{
				clothingBox.ScrollToBottom();
			}
		}

		private static void UpdateHeaderIcon(int index, ItemAsset asset, byte quality, byte[] state)
		{
			float size_x;
			float size_y;
			if (asset.size_y > 2)
			{
				size_y = 50.0f;
				size_x = size_y * ((float) asset.size_x / (float) asset.size_y);
			}
			else
			{
				size_x = asset.size_x * 25.0f;
				size_y = asset.size_y * 25.0f;
			}

			headerItemIcons[index].SizeOffset_X = size_x;
			headerItemIcons[index].SizeOffset_Y = size_y;
			headerItemIcons[index].PositionOffset_Y = -size_y / 2;

			headerItemIcons[index].Refresh(asset.id, quality, state, asset, Mathf.RoundToInt(size_x), Mathf.RoundToInt(size_y));
		}

		private static void onShirtUpdated(ushort newShirtObsolete, byte newShirtQuality, byte[] newShirtState)
		{
			ItemAsset asset = Player.LocalPlayer.clothing.shirtAsset;

			if (asset != null)
			{
				headers[3].Text = asset.itemName;

				UpdateHeaderIcon(3, asset, newShirtQuality, newShirtState);

				((ISleekLabel) headers[3].GetChildAtIndex(2)).Text = newShirtQuality + "%";

				Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
				headers[3].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
				headers[3].TextColor = rarityColor;

				Color qualityColor = ItemTool.getQualityColor(newShirtQuality / 100.0f);
				((ISleekImage) headers[3].GetChildAtIndex(1)).TintColor = qualityColor;
				((ISleekLabel) headers[3].GetChildAtIndex(2)).TextColor = qualityColor;
			}
		}

		private static void onPantsUpdated(ushort newPantsObsolete, byte newPantsQuality, byte[] newPantsState)
		{
			if (headers != null)
			{
				ItemAsset asset = Player.LocalPlayer.clothing.pantsAsset;

				if (asset != null)
				{
					headers[4].Text = asset.itemName;

					UpdateHeaderIcon(4, asset, newPantsQuality, newPantsState);

					((ISleekLabel) headers[4].GetChildAtIndex(2)).Text = newPantsQuality + "%";

					Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
					headers[4].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					headers[4].TextColor = rarityColor;

					Color qualityColor = ItemTool.getQualityColor(newPantsQuality / 100.0f);
					((ISleekImage) headers[4].GetChildAtIndex(1)).TintColor = qualityColor;
					((ISleekLabel) headers[4].GetChildAtIndex(2)).TextColor = qualityColor;
				}
			}
		}

		private static void onHatUpdated(ushort newHatObsolete, byte newHatQuality, byte[] newHatState)
		{
			if (headers != null)
			{
				ItemAsset asset = Player.LocalPlayer.clothing.hatAsset;

				if (asset != null)
				{
					headers[7].Text = asset.itemName;

					UpdateHeaderIcon(7, asset, newHatQuality, newHatState);

					((ISleekLabel) headers[7].GetChildAtIndex(2)).Text = newHatQuality + "%";

					Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
					headers[7].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					headers[7].TextColor = rarityColor;

					Color qualityColor = ItemTool.getQualityColor(newHatQuality / 100.0f);
					((ISleekImage) headers[7].GetChildAtIndex(1)).TintColor = qualityColor;
					((ISleekLabel) headers[7].GetChildAtIndex(2)).TextColor = qualityColor;
				}

				headers[7].IsVisible = asset != null;
				updateBoxAreas();
			}
		}

		private static void onBackpackUpdated(ushort newBackpackObsolete, byte newBackpackQuality, byte[] newBackpackState)
		{
			ItemAsset asset = Player.LocalPlayer.clothing.backpackAsset;

			if (asset != null)
			{
				headers[1].Text = asset.itemName;

				UpdateHeaderIcon(1, asset, newBackpackQuality, newBackpackState);

				((ISleekLabel) headers[1].GetChildAtIndex(2)).Text = newBackpackQuality + "%";

				Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
				headers[1].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
				headers[1].TextColor = rarityColor;

				Color qualityColor = ItemTool.getQualityColor(newBackpackQuality / 100.0f);
				((ISleekImage) headers[1].GetChildAtIndex(1)).TintColor = qualityColor;
				((ISleekLabel) headers[1].GetChildAtIndex(2)).TextColor = qualityColor;
			}
		}

		private static void onVestUpdated(ushort newVestObsolete, byte newVestQuality, byte[] newVestState)
		{
			ItemAsset asset = Player.LocalPlayer.clothing.vestAsset;

			if (asset != null)
			{
				headers[2].Text = asset.itemName;

				UpdateHeaderIcon(2, asset, newVestQuality, newVestState);

				((ISleekLabel) headers[2].GetChildAtIndex(2)).Text = newVestQuality + "%";

				Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
				headers[2].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
				headers[2].TextColor = rarityColor;

				Color qualityColor = ItemTool.getQualityColor(newVestQuality / 100.0f);
				((ISleekImage) headers[2].GetChildAtIndex(1)).TintColor = qualityColor;
				((ISleekLabel) headers[2].GetChildAtIndex(2)).TextColor = qualityColor;
			}
		}

		private static void onMaskUpdated(ushort newMaskObsolete, byte newMaskQuality, byte[] newMaskState)
		{
			if (headers != null)
			{
				ItemAsset asset = Player.LocalPlayer.clothing.maskAsset;

				if (asset != null)
				{
					headers[8].Text = asset.itemName;

					UpdateHeaderIcon(8, asset, newMaskQuality, newMaskState);

					((ISleekLabel) headers[8].GetChildAtIndex(2)).Text = newMaskQuality + "%";

					Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
					headers[8].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					headers[8].TextColor = rarityColor;

					Color qualityColor = ItemTool.getQualityColor(newMaskQuality / 100.0f);
					((ISleekImage) headers[8].GetChildAtIndex(1)).TintColor = qualityColor;
					((ISleekLabel) headers[8].GetChildAtIndex(2)).TextColor = qualityColor;
				}

				headers[8].IsVisible = asset != null;
				updateBoxAreas();
			}
		}

		private static void onGlassesUpdated(ushort newGlassesObsolete, byte newGlassesQuality, byte[] newGlassesState)
		{
			if (headers != null)
			{
				ItemAsset asset = Player.LocalPlayer.clothing.glassesAsset;

				if (asset != null)
				{
					headers[9].Text = asset.itemName;

					UpdateHeaderIcon(9, asset, newGlassesQuality, newGlassesState);

					((ISleekLabel) headers[9].GetChildAtIndex(2)).Text = newGlassesQuality + "%";

					Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
					headers[9].BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					headers[9].TextColor = rarityColor;

					Color qualityColor = ItemTool.getQualityColor(newGlassesQuality / 100.0f);
					((ISleekImage) headers[9].GetChildAtIndex(1)).TintColor = qualityColor;
					((ISleekLabel) headers[9].GetChildAtIndex(2)).TextColor = qualityColor;
				}

				headers[9].IsVisible = asset != null;
				updateBoxAreas();
			}
		}

		private static void onClickedHeader(ISleekElement button)
		{
			int index;
			for (index = 0; index < headers.Length; index++)
			{
				if (headers[index] == button)
				{
					break;
				}
			}

			switch (index)
			{
				case 0: // hands
					if (Player.LocalPlayer.equipment.HasValidUseable && !Player.LocalPlayer.equipment.isBusy && Player.LocalPlayer.equipment.IsEquipAnimationFinished)
					{
						Player.LocalPlayer.equipment.dequip();
					}
					return;
				case 1: // backpack
					Player.LocalPlayer.clothing.sendSwapBackpack(255, 255, 255);
					return;
				case 2: // vest
					Player.LocalPlayer.clothing.sendSwapVest(255, 255, 255);
					return;
				case 3: // shirt
					Player.LocalPlayer.clothing.sendSwapShirt(255, 255, 255);
					return;
				case 4: // pants
					Player.LocalPlayer.clothing.sendSwapPants(255, 255, 255);
					return;
				case 5: // storage
					PlayerDashboardUI.close();
					PlayerLifeUI.open();
					return;
				case 6: // area
					PlayerDashboardUI.close();
					PlayerLifeUI.open();
					return;
				case 7: // hat
					Player.LocalPlayer.clothing.sendSwapHat(255, 255, 255);
					return;
				case 8: // mask
					Player.LocalPlayer.clothing.sendSwapMask(255, 255, 255);
					return;
				case 9: // glasses
					Player.LocalPlayer.clothing.sendSwapGlasses(255, 255, 255);
					return;
			}
		}

		private static void updatePivot()
		{
			if (dragJar.rot == 0)
			{
				dragPivot.x = dragOffset.x;
				dragPivot.y = dragOffset.y;
			}
			else if (dragJar.rot == 1)
			{
				dragPivot.x = -((dragJar.size_y * 50) + dragOffset.y);
				dragPivot.y = dragOffset.x;
			}
			else if (dragJar.rot == 2)
			{
				dragPivot.x = -((dragJar.size_x * 50) + dragOffset.x);
				dragPivot.y = -((dragJar.size_y * 50) + dragOffset.y);
			}
			else if (dragJar.rot == 3)
			{
				dragPivot.x = dragOffset.y;
				dragPivot.y = -((dragJar.size_x * 50) + dragOffset.x);
			}
		}

		/// <summary>
		/// Move item drag visual to the cursor's position.
		/// </summary>
		private static void refreshDraggedVisualPosition()
		{
			dragItem.PositionOffset_X = (int) dragPivot.x;
			dragItem.PositionOffset_Y = (int) dragPivot.y;

			Vector2 position = PlayerUI.container.ViewportToNormalizedPosition(InputEx.NormalizedMousePosition);
			dragItem.PositionScale_X = position.x;
			dragItem.PositionScale_Y = position.y;
		}

		public static void updateDraggedItem()
		{
			if (!active || !PlayerDashboardUI.active || dragFromPage == 255 || dragJar == null || !isDragging)
			{
				return;
			}

			if (InputEx.GetKeyDown(ControlsSettings.rotate))
			{
				dragJar.rot++;
				dragJar.rot %= 4;

				updatePivot();

				dragItem.updateItem(dragJar);

				PlayInventoryAudio(dragJar.GetAsset());
			}

			refreshDraggedVisualPosition();
		}

		private static void createElementForNearbyDrop(InteractableItem interactableItem)
		{
			while (true)
			{
				if (areaItems.tryAddItem(interactableItem.item))
				{
					break;
				}
				else
				{
					if (areaItems.height < 200)
					{
						areaItems.resize(areaItems.width, (byte) (areaItems.height + 1));
					}
					else
					{
						return;
					}
				}
			}

			ItemJar jar = areaItems.getItem((byte) (areaItems.getItemCount() - 1));
			jar.interactableItem = interactableItem;
			interactableItem.jar = jar;

			byte space = (byte) (areaItems.height - (jar.y + (jar.rot % 2 == 0 ? jar.size_y : jar.size_x)));
			if (space < 3)
			{
				if (areaItems.height + space <= 200)
				{
					areaItems.resize(areaItems.width, (byte) (areaItems.height + (3 - space)));
				}
			}

			SleekItems itemsUI = items[PlayerInventory.AREA - PlayerInventory.SLOTS];
			itemsUI.resize(areaItems.width, areaItems.height);
			itemsUI.addItem(jar);
		}

		public static void updateNearbyDrops()
		{
			if (!active || pendingItemsInRadius.Count < 1)
				return;

			int oldHeight = areaItems.height;

			Vector3 eyesPosition = Player.LocalPlayer.look.GetEyesPositionWithoutLeaning();

			const int elementsPerUpdate = 20;
			int endIndex = Mathf.Max(0, pendingItemsInRadius.Count - elementsPerUpdate);
			for (int index = pendingItemsInRadius.Count - 1; index >= endIndex; --index)
			{
				InteractableItem interactableItem = pendingItemsInRadius[index];
				pendingItemsInRadius.RemoveAt(index);

				if (interactableItem == null)
				{
					// Destroyed?
					continue;
				}

				Item item = interactableItem.item;
				if (item == null)
				{
					continue;
				}

				Transform model = interactableItem.transform;
				Renderer renderer = model.GetComponentInChildren<Renderer>();

				if (renderer == null)
				{
					continue;
				}

				Vector3 endPoint = renderer.bounds.center;
				bool hitSomething = Physics.Linecast(eyesPosition, endPoint, out RaycastHit losHit, RayMasks.BLOCK_PICKUP, QueryTriggerInteraction.Ignore);
				if (hitSomething)
				{
#if WITH_NEARBY_ITEM_LOS_DEBUG
					RuntimeGizmos gizmos = RuntimeGizmos.Get();
					gizmos.Linecast(eyesPosition, endPoint, losHit, Color.green, Color.red, 5.0f);
					UnturnedLog.info($"Line of sight to {renderer.GetSceneHierarchyPath()} at {endPoint} is blocked");
#endif // WITH_NEARBY_ITEM_LOS_DEBUG
					// View is obstructed.
					continue;
				}

				createElementForNearbyDrop(interactableItem);
			}

			if (areaItems.height > oldHeight)
			{
				updateBoxAreas();
			}
		}

		private static void PlayInventoryAudio(ItemAsset item)
		{
#if !DEDICATED_SERVER // OneShotAudio is excluded from dedicated server.
			item?.PlayInventoryAudio2D();
#endif // !DEDICATED_SERVER
		}

		public PlayerDashboardInventoryUI()
		{
			pendingItemsInRadius = new List<InteractableItem>();

			localization = Localization.read("/Player/PlayerDashboardInventory.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerDashboardInventory");

			_selectedPage = 255;
			_selected_x = 255;
			_selected_y = 255;
			isDragging = false;

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = true;

			backdropBox = Glazier.Get().CreateBox();
			backdropBox.PositionOffset_Y = 60;
			backdropBox.SizeOffset_Y = -60;
			backdropBox.SizeScale_X = 1;
			backdropBox.SizeScale_Y = 1;
			backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
			container.AddChild(backdropBox);

			characterPlayer = null;

			hasDragOutsideHandlers = Glazier.Get().SupportsDepth;

			if (hasDragOutsideHandlers)
			{
				dragOutsideHandler = Glazier.Get().CreateImage();
				dragOutsideHandler.SizeScale_X = 1.0f;
				dragOutsideHandler.SizeScale_Y = 1.0f;
				dragOutsideHandler.OnClicked += onClickedDuringDrag;
				dragOutsideHandler.OnRightClicked += onRightClickedDuringDrag;
				dragOutsideHandler.IsVisible = false;
				backdropBox.AddChild(dragOutsideHandler);
			}
			else
			{
				dragOutsideHandler = null;
			}

			ISleekBox characterBox = Glazier.Get().CreateBox();
			characterBox.PositionOffset_X = 10;
			characterBox.PositionOffset_Y = 70;
			characterBox.SizeOffset_X = 410;
			characterBox.SizeOffset_Y = -280;
			characterBox.SizeScale_Y = 1;
			backdropBox.AddChild(characterBox);

			ISleekConstraintFrame characterConstraintFrame = Glazier.Get().CreateConstraintFrame();
			characterConstraintFrame.SizeScale_Y = 1.0f;
			characterConstraintFrame.Constraint = ESleekConstraint.FitInParent;
			if (Glazier.Get().SupportsDepth)
			{
				// Was 20% horizontal padding because character RT is usually empty there, but general consensus
				// after the uGUI update was that the image was too small. ¯\_(ツ)_/¯
				characterConstraintFrame.PositionScale_X = -0.5f;
				characterConstraintFrame.SizeScale_X = 2.0f;
			}
			else
			{
				// IMGUI cannot overlap other UI elements.
				characterConstraintFrame.PositionScale_X = 0.0f;
				characterConstraintFrame.SizeScale_X = 1.0f;
			}
			characterBox.AddChild(characterConstraintFrame);

			characterImage = new SleekCameraImage();
			characterImage.SizeScale_X = 1;
			characterImage.SizeScale_Y = 1;
			characterImage.internalImage.OnClicked += onClickedCharacter;
			characterImage.SetCamera(Player.LocalPlayer.look.characterCamera);
			characterConstraintFrame.AddChild(characterImage);

			slots = new SleekSlot[PlayerInventory.SLOTS];
			for (byte index = 0; index < slots.Length; index++)
			{
				slots[index] = new SleekSlot(index);
				slots[index].onSelectedItem = onSelectedItem;
				slots[index].onGrabbedItem = onGrabbedItem;
				slots[index].onPlacedItem = onPlacedItem;
				backdropBox.AddChild(slots[index]);
			}

			slots[0].PositionOffset_X = 10;
			slots[0].PositionOffset_Y = -160;
			slots[0].PositionScale_Y = 1;

			slots[1].PositionOffset_X = 270;
			slots[1].PositionOffset_Y = -160;
			slots[1].PositionScale_Y = 1;
			slots[1].SizeOffset_X = 150;

			characterSlider = Glazier.Get().CreateSlider();
			characterSlider.SizeOffset_Y = 20;
			characterSlider.SizeScale_X = 1;
			characterSlider.SizeOffset_X = -120;
			characterSlider.PositionOffset_X = 120;
			characterSlider.PositionOffset_Y = 15;
			characterSlider.PositionScale_Y = 1;
			characterSlider.Orientation = ESleekOrientation.HORIZONTAL;
			characterSlider.OnValueChanged += onDraggedCharacterSlider;
			characterBox.AddChild(characterSlider);

			swapCosmeticsButton = new SleekButtonIcon(icons.load<Texture2D>("Swap_Cosmetics"));
			swapCosmeticsButton.PositionOffset_Y = 10;
			swapCosmeticsButton.PositionScale_Y = 1;
			swapCosmeticsButton.SizeOffset_X = 30;
			swapCosmeticsButton.SizeOffset_Y = 30;
			swapCosmeticsButton.tooltip = localization.format("Swap_Cosmetics_Tooltip");
			swapCosmeticsButton.iconColor = ESleekTint.FOREGROUND;
			swapCosmeticsButton.onClickedButton += onClickedSwapCosmeticsButton;
			characterBox.AddChild(swapCosmeticsButton);

			swapSkinsButton = new SleekButtonIcon(icons.load<Texture2D>("Swap_Skins"));
			swapSkinsButton.PositionOffset_X = 40;
			swapSkinsButton.PositionOffset_Y = 10;
			swapSkinsButton.PositionScale_Y = 1;
			swapSkinsButton.SizeOffset_X = 30;
			swapSkinsButton.SizeOffset_Y = 30;
			swapSkinsButton.tooltip = localization.format("Swap_Skins_Tooltip");
			swapSkinsButton.iconColor = ESleekTint.FOREGROUND;
			swapSkinsButton.onClickedButton += onClickedSwapSkinsButton;
			characterBox.AddChild(swapSkinsButton);

			swapMythicsButton = new SleekButtonIcon(icons.load<Texture2D>("Swap_Mythics"));
			swapMythicsButton.PositionOffset_X = 80;
			swapMythicsButton.PositionOffset_Y = 10;
			swapMythicsButton.PositionScale_Y = 1;
			swapMythicsButton.SizeOffset_X = 30;
			swapMythicsButton.SizeOffset_Y = 30;
			swapMythicsButton.tooltip = localization.format("Swap_Mythics_Tooltip");
			swapMythicsButton.iconColor = ESleekTint.FOREGROUND;
			swapMythicsButton.onClickedButton += onClickedSwapMythicsButton;
			characterBox.AddChild(swapMythicsButton);

			box = Glazier.Get().CreateFrame();
			box.PositionOffset_X = 430;
			box.PositionOffset_Y = 10;
			box.SizeOffset_X = -440;
			box.SizeOffset_Y = -20;
			box.SizeScale_X = 1;
			box.SizeScale_Y = 1;
			backdropBox.AddChild(box);

			clothingBox = Glazier.Get().CreateScrollView();
			clothingBox.SizeScale_X = 1;
			clothingBox.SizeScale_Y = 1;
			clothingBox.ContentSizeOffset = new Vector2(0.0f, 1000);
			clothingBox.ScaleContentToWidth = true;
			box.AddChild(clothingBox);

			areaBox = Glazier.Get().CreateScrollView();
			areaBox.PositionOffset_X = 5;
			areaBox.PositionScale_X = 0.5f;
			areaBox.SizeOffset_X = -5;
			areaBox.SizeScale_X = 0.5f;
			areaBox.SizeScale_Y = 1;
			areaBox.ContentSizeOffset = new Vector2(0.0f, 1000);
			areaBox.ScaleContentToWidth = true;
			box.AddChild(areaBox);

			if (hasDragOutsideHandlers)
			{
				dragOutsideClothingHandler = Glazier.Get().CreateImage();
				dragOutsideClothingHandler.SizeScale_X = 1.0f;
				dragOutsideClothingHandler.SizeScale_Y = 1.0f;
				dragOutsideClothingHandler.OnClicked += onClickedDuringDrag;
				dragOutsideClothingHandler.OnRightClicked += onRightClickedDuringDrag;
				dragOutsideClothingHandler.IsVisible = false;
				clothingBox.AddChild(dragOutsideClothingHandler);

				dragOutsideAreaHandler = Glazier.Get().CreateImage();
				dragOutsideAreaHandler.SizeScale_X = 1.0f;
				dragOutsideAreaHandler.SizeScale_Y = 1.0f;
				dragOutsideAreaHandler.OnClicked += onClickedDuringDrag;
				dragOutsideAreaHandler.OnRightClicked += onRightClickedDuringDrag;
				dragOutsideAreaHandler.IsVisible = false;
				areaBox.AddChild(dragOutsideAreaHandler);
			}
			else
			{
				dragOutsideClothingHandler = null;
				dragOutsideAreaHandler = null;
			}

			headers = new ISleekButton[PlayerInventory.PAGES - PlayerInventory.SLOTS + 3];
			for (byte index = 0; index < headers.Length; index++)
			{
				headers[index] = Glazier.Get().CreateButton();
				headers[index].SizeOffset_Y = 60;
				headers[index].SizeScale_X = 1;
				headers[index].FontSize = ESleekFontSize.Medium;
				headers[index].OnClicked += onClickedHeader;
				headers[index].TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				clothingBox.AddChild(headers[index]);
				headers[index].IsVisible = false;
			}

			headers[0].IsVisible = true;
			headers[PlayerInventory.AREA - PlayerInventory.SLOTS].IsVisible = true;

			headerItemIcons = new SleekItemIcon[headers.Length];

			for (byte index = 1; index < headers.Length; index++)
			{
				if (index == PlayerInventory.STORAGE - PlayerInventory.SLOTS || index == PlayerInventory.AREA - PlayerInventory.SLOTS)
				{
					continue; // no header img for storage
				}

				SleekItemIcon icon = new SleekItemIcon();
				icon.PositionOffset_X = 5;
				icon.PositionScale_Y = 0.5f;
				headerItemIcons[index] = icon;
				headers[index].AddChild(icon);

				ISleekImage qualityImage = Glazier.Get().CreateImage();
				qualityImage.PositionOffset_X = -25;
				qualityImage.PositionOffset_Y = -25;
				qualityImage.PositionScale_X = 1;
				qualityImage.PositionScale_Y = 1;
				qualityImage.SizeOffset_X = 20;
				qualityImage.SizeOffset_Y = 20;
				qualityImage.Texture = icons.load<Texture2D>("Quality_0");
				headers[index].AddChild(qualityImage);

				ISleekLabel qualityLabel = Glazier.Get().CreateLabel();
				qualityLabel.PositionOffset_X = -105;
				qualityLabel.PositionOffset_Y = 5;
				qualityLabel.PositionScale_X = 1;
				qualityLabel.SizeOffset_X = 100;
				qualityLabel.SizeOffset_Y = -10;
				qualityLabel.SizeScale_Y = 1.0f;
				qualityLabel.TextAlignment = TextAnchor.UpperRight;
				qualityLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				headers[index].AddChild(qualityLabel);
			}

			headers[0].Text = localization.format("Hands");
			headers[PlayerInventory.AREA - PlayerInventory.SLOTS].Text = localization.format("Area");

			onShirtUpdated(0, Player.LocalPlayer.clothing.shirtQuality, Player.LocalPlayer.clothing.shirtState);
			onPantsUpdated(0, Player.LocalPlayer.clothing.pantsQuality, Player.LocalPlayer.clothing.pantsState);
			onBackpackUpdated(0, Player.LocalPlayer.clothing.backpackQuality, Player.LocalPlayer.clothing.backpackState);
			onVestUpdated(0, Player.LocalPlayer.clothing.vestQuality, Player.LocalPlayer.clothing.vestState);

			items = new SleekItems[PlayerInventory.PAGES - PlayerInventory.SLOTS];
			for (byte index = 0; index < items.Length; index++)
			{
				items[index] = new SleekItems((byte) (PlayerInventory.SLOTS + index));
				items[index].onSelectedItem = onSelectedItem;
				items[index].onGrabbedItem = onGrabbedItem;
				items[index].onPlacedItem = onPlacedItem;
				clothingBox.AddChild(items[index]);
			}

			areaItems = new Items(PlayerInventory.AREA);

			//			items[PlayerInventory.SLOTS] = new SleekItems(0);
			//			items[PlayerInventory.SLOTS].positionOffset_Y = 450;
			//			items[0].sizeOffset_X = 0;
			//			items[0].onSelectedItem = onSelectedItem;
			//			items[0].onGrabbedItem = onGrabbedItem;
			//			items[0].onPlacedItem = onPlacedItem;
			//			container.add(items[0]);

			selectionFrame = Glazier.Get().CreateFrame();
			selectionFrame.SizeScale_X = 1.0f;
			selectionFrame.SizeScale_Y = 1.0f;
			selectionFrame.IsVisible = false;
			PlayerUI.container.AddChild(selectionFrame);

			if (hasDragOutsideHandlers)
			{
				// Nelson 2023-10-06: this *was* the parent of selectionBackdropBox, but this
				// allowed clicks to bubble up and close the selection. Problematic with the addition
				// of the description scroll bar.
				outsideSelectionInvisibleButton = Glazier.Get().CreateImage();
				outsideSelectionInvisibleButton.SizeScale_X = 1.0f;
				outsideSelectionInvisibleButton.SizeScale_Y = 1.0f;
				outsideSelectionInvisibleButton.OnClicked += onClickedOutsideSelection;
				outsideSelectionInvisibleButton.OnRightClicked += onClickedOutsideSelection;
				outsideSelectionInvisibleButton.Texture = GlazierResources.PixelTexture;
				outsideSelectionInvisibleButton.TintColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
				selectionFrame.AddChild(outsideSelectionInvisibleButton);
			}
			else
			{
				outsideSelectionInvisibleButton = null;
			}

			selectionBackdropBox = Glazier.Get().CreateBox();
			selectionBackdropBox.SizeOffset_X = 530;
			selectionBackdropBox.SizeOffset_Y = 440;
			selectionFrame.AddChild(selectionBackdropBox);

			//			nameLabel = Glazier.Get().CreateLabel();
			//			nameLabel.sizeOffset_Y = 30;
			//			nameLabel.sizeScale_X = 1;
			//			nameLabel.fontAlignment = TextAnchor.MiddleLeft;
			//			selectionBox.add(nameLabel);

			selectionIconBox = Glazier.Get().CreateBox();
			selectionIconBox.PositionOffset_X = 10;
			selectionIconBox.PositionOffset_Y = 10;
			selectionIconBox.SizeOffset_X = 510;
			selectionIconBox.SizeOffset_Y = 310;
			selectionBackdropBox.AddChild(selectionIconBox);

			selectionIconImage = new SleekItemIcon();
			selectionIconImage.PositionScale_X = 0.5f;
			selectionIconImage.PositionScale_Y = 0.5f;
			selectionIconBox.AddChild(selectionIconImage);

			if (Glazier.Get().SupportsAutomaticLayout)
			{
				selectionDescriptionScrollView = Glazier.Get().CreateScrollView();
				selectionDescriptionScrollView.PositionOffset_X = 10;
				selectionDescriptionScrollView.PositionOffset_Y = 330;
				selectionDescriptionScrollView.SizeOffset_X = 250;
				selectionDescriptionScrollView.SizeOffset_Y = 100;
				selectionDescriptionScrollView.ScaleContentToWidth = true;
				selectionDescriptionScrollView.ContentUseManualLayout = false;
				selectionBackdropBox.AddChild(selectionDescriptionScrollView);

				selectionDescriptionLabel = Glazier.Get().CreateLabel();
				selectionDescriptionLabel.UseManualLayout = false;
				selectionDescriptionLabel.AllowRichText = true;
				selectionDescriptionLabel.TextAlignment = TextAnchor.UpperLeft;
				selectionDescriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				selectionDescriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				selectionDescriptionScrollView.AddChild(selectionDescriptionLabel);
			}
			else
			{
				selectionDescriptionBox = Glazier.Get().CreateBox();
				selectionDescriptionBox.PositionOffset_X = 10;
				selectionDescriptionBox.PositionOffset_Y = 330;
				selectionDescriptionBox.SizeOffset_X = 250;
				selectionDescriptionBox.SizeOffset_Y = 100;
				selectionBackdropBox.AddChild(selectionDescriptionBox);

				selectionDescriptionLabel = Glazier.Get().CreateLabel();
				selectionDescriptionLabel.AllowRichText = true;
				selectionDescriptionLabel.PositionOffset_X = 5;
				selectionDescriptionLabel.PositionOffset_Y = 5;
				selectionDescriptionLabel.SizeOffset_X = -10;
				selectionDescriptionLabel.SizeOffset_Y = -10;
				selectionDescriptionLabel.SizeScale_X = 1.0f;
				selectionDescriptionLabel.SizeScale_Y = 1.0f;
				selectionDescriptionLabel.TextAlignment = TextAnchor.UpperLeft;
				selectionDescriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				selectionDescriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				selectionDescriptionBox.AddChild(selectionDescriptionLabel);
			}

			selectionNameLabel = Glazier.Get().CreateLabel();
			selectionNameLabel.PositionOffset_Y = -70;
			selectionNameLabel.PositionScale_Y = 1.0f;
			selectionNameLabel.SizeOffset_Y = 70;
			selectionNameLabel.SizeScale_X = 1.0f;
			selectionNameLabel.FontSize = ESleekFontSize.Large;
			selectionNameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			selectionIconBox.AddChild(selectionNameLabel);

			selectionHotkeyLabel = Glazier.Get().CreateLabel();
			selectionHotkeyLabel.PositionOffset_X = 5;
			selectionHotkeyLabel.PositionOffset_Y = 5;
			selectionHotkeyLabel.SizeOffset_X = -10;
			selectionHotkeyLabel.SizeOffset_Y = 30;
			selectionHotkeyLabel.SizeScale_X = 1;
			selectionHotkeyLabel.TextAlignment = TextAnchor.UpperRight;
			selectionIconBox.AddChild(selectionHotkeyLabel);

			selectionActionsBox = Glazier.Get().CreateScrollView();
			selectionActionsBox.PositionOffset_X = 270;
			selectionActionsBox.PositionOffset_Y = 330;
			selectionActionsBox.SizeOffset_X = -280;
			selectionActionsBox.SizeOffset_Y = 100;
			selectionActionsBox.SizeScale_X = 1.0f;
			selectionActionsBox.ScaleContentToWidth = true;
			selectionBackdropBox.AddChild(selectionActionsBox);

			//selectionQualityBox = Glazier.Get().CreateBox();
			//selectionQualityBox.positionOffset_Y = -30;
			//selectionQualityBox.positionScale_Y = 1;
			//selectionQualityBox.sizeOffset_Y = 30;
			//selectionQualityBox.sizeScale_X = 1;
			//selectionQualityBox.isVisible = false;
			//selectionQualityBox.foregroundTint = ESleekTint.NONE;
			//selectionBox.add(selectionQualityBox);


			//			iconImage = Glazier.Get().CreateImage();
			//			iconImage.positionScale_Y = 0.5f;
			//			iconImage.positionOffset_X = 10;
			//			selectionBox.add(iconImage);

			selectionEquipButton = Glazier.Get().CreateButton();// Icon(icons.load<Texture2D>("Equip"));
																//			equipButton.positionOffset_X = 10;
																//equipButton.positionScale_X = 1;
																//			equipButton.sizeOffset_X = -20;
			selectionEquipButton.SizeScale_X = 1.0f;
			selectionEquipButton.SizeOffset_Y = 30;
			//equipButton.sizeScale_X = 0.5f;
			//equipButton.iconImage.backgroundColor = ESleekTint.FOREGROUND;
			selectionEquipButton.OnClicked += onClickedEquip;
			selectionActionsBox.AddChild(selectionEquipButton);


			selectionContextButton = Glazier.Get().CreateButton();
			selectionContextButton.SizeScale_X = 1.0f;
			selectionContextButton.SizeOffset_Y = 30;
			selectionContextButton.OnClicked += onClickedContext;
			selectionActionsBox.AddChild(selectionContextButton);



			selectionDropButton = Glazier.Get().CreateButton();
			selectionDropButton.SizeScale_X = 1.0f;
			selectionDropButton.SizeOffset_Y = 30;
			selectionDropButton.OnClicked += onClickedDrop;
			selectionActionsBox.AddChild(selectionDropButton);

			selectionStorageButton = Glazier.Get().CreateButton();
			selectionStorageButton.SizeScale_X = 1.0f;
			selectionStorageButton.SizeOffset_Y = 30;
			selectionStorageButton.OnClicked += onClickedStore;
			selectionActionsBox.AddChild(selectionStorageButton);

			selectionExtraActionsBox = Glazier.Get().CreateFrame();
			selectionExtraActionsBox.SizeScale_X = 1.0f;
			selectionActionsBox.AddChild(selectionExtraActionsBox);

			vehicleBox = Glazier.Get().CreateBox();
			vehicleBox.SizeScale_X = 1;
			clothingBox.AddChild(vehicleBox);

			vehicleNameLabel = Glazier.Get().CreateLabel();
			vehicleNameLabel.SizeOffset_Y = 60;
			vehicleNameLabel.SizeScale_X = 1.0f;
			vehicleNameLabel.FontSize = ESleekFontSize.Medium;
			vehicleNameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			vehicleBox.AddChild(vehicleNameLabel);

			vehicleActionsBox = Glazier.Get().CreateFrame();
			vehicleActionsBox.PositionOffset_X = 10;
			vehicleActionsBox.PositionOffset_Y = 60;
			vehicleActionsBox.SizeOffset_X = 250;
			vehicleBox.AddChild(vehicleActionsBox);

			vehicleLockButton = Glazier.Get().CreateButton();
			vehicleLockButton.SizeOffset_Y = 30;
			vehicleLockButton.SizeScale_X = 1.0f;
			vehicleLockButton.OnClicked += onClickedVehicleLockButton;
			vehicleActionsBox.AddChild(vehicleLockButton);
			vehicleLockButton.IsVisible = false;

			vehicleHornButton = Glazier.Get().CreateButton();
			vehicleHornButton.SizeOffset_Y = 30;
			vehicleHornButton.SizeScale_X = 1.0f;
			vehicleHornButton.OnClicked += onClickedVehicleHornButton;
			vehicleActionsBox.AddChild(vehicleHornButton);
			vehicleHornButton.IsVisible = false;

			vehicleHeadlightsButton = Glazier.Get().CreateButton();
			vehicleHeadlightsButton.SizeOffset_Y = 30;
			vehicleHeadlightsButton.SizeScale_X = 1.0f;
			vehicleHeadlightsButton.OnClicked += onClickedVehicleHeadlightsButton;
			vehicleActionsBox.AddChild(vehicleHeadlightsButton);
			vehicleHeadlightsButton.IsVisible = false;

			vehicleSirensButton = Glazier.Get().CreateButton();
			vehicleSirensButton.SizeOffset_Y = 30;
			vehicleSirensButton.SizeScale_X = 1.0f;
			vehicleSirensButton.OnClicked += onClickedVehicleSirensButton;
			vehicleActionsBox.AddChild(vehicleSirensButton);
			vehicleSirensButton.IsVisible = false;

			vehicleBlimpButton = Glazier.Get().CreateButton();
			vehicleBlimpButton.SizeOffset_Y = 30;
			vehicleBlimpButton.SizeScale_X = 1.0f;
			vehicleBlimpButton.OnClicked += onClickedVehicleBlimpButton;
			vehicleActionsBox.AddChild(vehicleBlimpButton);
			vehicleBlimpButton.IsVisible = false;

			vehicleHookButton = Glazier.Get().CreateButton();
			vehicleHookButton.SizeOffset_Y = 30;
			vehicleHookButton.SizeScale_X = 1.0f;
			vehicleHookButton.OnClicked += onClickedVehicleHookButton;
			vehicleActionsBox.AddChild(vehicleHookButton);
			vehicleHookButton.IsVisible = false;

			vehicleStealBatteryButton = Glazier.Get().CreateButton();
			vehicleStealBatteryButton.SizeOffset_Y = 30;
			vehicleStealBatteryButton.SizeScale_X = 1.0f;
			vehicleStealBatteryButton.OnClicked += onClickedVehicleStealBatteryButton;
			vehicleActionsBox.AddChild(vehicleStealBatteryButton);
			vehicleStealBatteryButton.IsVisible = false;

			vehicleSkinButton = Glazier.Get().CreateButton();
			vehicleSkinButton.SizeOffset_Y = 30;
			vehicleSkinButton.SizeScale_X = 1.0f;
			vehicleSkinButton.OnClicked += onClickedVehicleSkinButton;
			vehicleActionsBox.AddChild(vehicleSkinButton);
			vehicleSkinButton.IsVisible = false;

			vehiclePassengersBox = Glazier.Get().CreateFrame();
			vehiclePassengersBox.PositionOffset_Y = 60;
			vehiclePassengersBox.SizeScale_X = 1;
			vehicleBox.AddChild(vehiclePassengersBox);
			//vehiclePassengersScrollBox = new SleekScrollBox();
			//vehiclePassengersScrollBox.positionOffset_Y = 60;
			//vehiclePassengersScrollBox.sizeScale_X = 1;
			//vehicleBox.add(vehiclePassengersScrollBox);

			rot_xButton = Glazier.Get().CreateButton();
			rot_xButton.PositionScale_X = 1;
			rot_xButton.SizeOffset_X = 60;
			rot_xButton.SizeOffset_Y = 60;
			rot_xButton.OnClicked += onClickedRot_XButton;
			rot_xButton.Text = localization.format("Rot_X");
			headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].AddChild(rot_xButton);
			rot_xButton.IsVisible = false;

			rot_yButton = Glazier.Get().CreateButton();
			rot_yButton.PositionScale_X = 1;
			rot_yButton.PositionOffset_X = 60;
			rot_yButton.SizeOffset_X = 60;
			rot_yButton.SizeOffset_Y = 60;
			rot_yButton.OnClicked += onClickedRot_YButton;
			rot_yButton.Text = localization.format("Rot_Y");
			headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].AddChild(rot_yButton);
			rot_yButton.IsVisible = false;

			rot_zButton = Glazier.Get().CreateButton();
			rot_zButton.PositionScale_X = 1;
			rot_zButton.PositionOffset_X = 120;
			rot_zButton.SizeOffset_X = 60;
			rot_zButton.SizeOffset_Y = 60;
			rot_zButton.OnClicked += onClickedRot_ZButton;
			rot_zButton.Text = localization.format("Rot_Z");
			headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].AddChild(rot_zButton);
			rot_zButton.IsVisible = false;

			dragItem = new SleekItem();
			PlayerUI.container.AddChild(dragItem);
			dragItem.IsVisible = false;
			dragItem.SetIsDragItem();
			dragOffset = Vector2.zero;
			dragPivot = Vector2.zero;

			dragFromPage = 255;
			dragFrom_x = 255;
			dragFrom_y = 255;
			dragFromRot = 0;

			Player.LocalPlayer.inventory.onInventoryResized += onInventoryResized;
			Player.LocalPlayer.inventory.onInventoryUpdated += onInventoryUpdated;
			Player.LocalPlayer.inventory.onInventoryAdded += onInventoryAdded;
			Player.LocalPlayer.inventory.onInventoryRemoved += onInventoryRemoved;
			Player.LocalPlayer.inventory.onInventoryStored += onInventoryStored;
			Player.LocalPlayer.equipment.onHotkeysUpdated += onHotkeysUpdated;

			ItemManager.onItemDropAdded = onItemDropAdded;
			ItemManager.onItemDropRemoved = onItemDropRemoved;

			Player.LocalPlayer.movement.onSeated += onSeated;

			Player.LocalPlayer.clothing.onShirtUpdated += onShirtUpdated;
			Player.LocalPlayer.clothing.onPantsUpdated += onPantsUpdated;
			Player.LocalPlayer.clothing.onHatUpdated += onHatUpdated;
			Player.LocalPlayer.clothing.onBackpackUpdated += onBackpackUpdated;
			Player.LocalPlayer.clothing.onVestUpdated += onVestUpdated;
			Player.LocalPlayer.clothing.onMaskUpdated += onMaskUpdated;
			Player.LocalPlayer.clothing.onGlassesUpdated += onGlassesUpdated;
		}

		internal static string FormatStatColor(string text, bool isBeneficial)
		{
			Color32 color = isBeneficial ? OptionsSettings.fontColor : OptionsSettings.badColor;
			return $"<color={Palette.hex(color)}>{text}</color>";
		}

		internal static string FormatStatModifier(float modifier, bool higherIsPositive, bool higherIsBeneficial)
		{
			char sign = higherIsPositive ? (modifier > 1.0f ? '+' : '-') : (modifier > 1.0f ? '-' : '+');
			bool isBeneficial = higherIsBeneficial ? (modifier > 1.0f) : (modifier < 1.0f);
			float percentageDelta = modifier > 1.0f ? (modifier - 1.0f) : (1.0f - modifier);
			return FormatStatColor($"{sign}{percentageDelta:P}", isBeneficial);
		}
	}
}
