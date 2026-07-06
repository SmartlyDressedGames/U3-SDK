////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorSpawnsZombiesUI
	{
		private static SleekFullscreenBox container;
		private static Local localization;
		public static bool active;

		private static ISleekScrollView tableScrollBox;
		private static ISleekScrollView spawnsScrollBox;

		private static ISleekButton[] tableButtons;
		private static ISleekButton[] slotButtons;
		private static ISleekButton[] clothButtons;

		private static SleekColorPicker tableColorPicker;
		private static ISleekToggle megaToggle;
		private static ISleekUInt16Field healthField;
		private static ISleekUInt8Field damageField;
		private static ISleekUInt8Field lootIndexField;
		private static ISleekUInt16Field lootIDField;
		private static ISleekUInt32Field xpField;
		private static ISleekFloat32Field regenField;
		private static ISleekField difficultyGUIDField;
		private static ISleekUInt16Field itemIDField;
		private static SleekButtonIcon addItemButton;
		private static SleekButtonIcon removeItemButton;

		private static ISleekBox selectedBox;
		private static ISleekField tableNameField;
		private static SleekButtonIcon addTableButton;
		private static SleekButtonIcon removeTableButton;

		private static ISleekSlider radiusSlider;
		private static SleekButtonIcon addButton;
		private static SleekButtonIcon removeButton;

		private static byte selectedSlot;
		private static byte selectItem;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			EditorSpawns.isSpawning = true;
			EditorSpawns.spawnMode = ESpawnMode.ADD_ZOMBIE;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			EditorSpawns.isSpawning = false;

			container.AnimateOutOfView(1, 0);
		}

		public static void updateTables()
		{
			if (tableButtons != null)
			{
				for (int index = 0; index < tableButtons.Length; index++)
				{
					tableScrollBox.RemoveChild(tableButtons[index]);
				}
			}

			tableButtons = new ISleekButton[LevelZombies.tables.Count];

			tableScrollBox.ContentSizeOffset = new Vector2(0.0f, (tableButtons.Length * 40) - 10);
			for (int index = 0; index < tableButtons.Length; index++)
			{
				ISleekButton tableButton = Glazier.Get().CreateButton();
				tableButton.PositionOffset_X = 240;
				tableButton.PositionOffset_Y = index * 40;
				tableButton.SizeOffset_X = 200;
				tableButton.SizeOffset_Y = 30;
				tableButton.Text = $"{index} {LevelZombies.tables[index].name} ({LevelZombies.tables[index].tableUniqueId})";
				tableButton.OnClicked += onClickedTableButton;
				tableScrollBox.AddChild(tableButton);

				tableButtons[index] = tableButton;
			}
		}

		public static void updateSelection()
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				ZombieTable table = LevelZombies.tables[EditorSpawns.selectedZombie];

				selectedBox.Text = table.name;
				tableNameField.Text = table.name;
				tableColorPicker.state = table.color;
				megaToggle.Value = table.isMega;
				healthField.Value = table.health;
				damageField.Value = table.damage;
				lootIndexField.Value = table.lootIndex;
				lootIDField.Value = table.lootID;
				xpField.Value = table.xp;
				regenField.Value = table.regen;
				difficultyGUIDField.Text = table.difficultyGUID;

				if (slotButtons != null)
				{
					for (int index = 0; index < slotButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(slotButtons[index]);
					}
				}

				slotButtons = new ISleekButton[table.slots.Length];

				for (int index = 0; index < slotButtons.Length; index++)
				{
					ZombieSlot slot = table.slots[index];

					ISleekButton slotButton = Glazier.Get().CreateButton();
					slotButton.PositionOffset_X = 240;
					slotButton.PositionOffset_Y = 460 + (index * 70);
					slotButton.SizeOffset_X = 200;
					slotButton.SizeOffset_Y = 30;
					slotButton.Text = localization.format("Slot_" + index);
					slotButton.OnClicked += onClickedSlotButton;
					spawnsScrollBox.AddChild(slotButton);

					ISleekSlider chanceSlider = Glazier.Get().CreateSlider();
					chanceSlider.PositionOffset_Y = 40;
					chanceSlider.SizeOffset_X = 200;
					chanceSlider.SizeOffset_Y = 20;
					chanceSlider.Orientation = ESleekOrientation.HORIZONTAL;
					chanceSlider.Value = slot.chance;
					chanceSlider.AddLabel(Mathf.RoundToInt(slot.chance * 100) + "%", ESleekSide.LEFT);
					chanceSlider.OnValueChanged += onDraggedChanceSlider;
					slotButton.AddChild(chanceSlider);

					slotButtons[index] = slotButton;
				}

				if (clothButtons != null)
				{
					for (int index = 0; index < clothButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(clothButtons[index]);
					}
				}

				if (selectedSlot < table.slots.Length)
				{
					clothButtons = new ISleekButton[table.slots[selectedSlot].table.Count];

					for (int index = 0; index < clothButtons.Length; index++)
					{
						ISleekButton itemButton = Glazier.Get().CreateButton();
						itemButton.PositionOffset_X = 240;
						itemButton.PositionOffset_Y = 460 + (slotButtons.Length * 70) + (index * 40);
						itemButton.SizeOffset_X = 200;
						itemButton.SizeOffset_Y = 30;

						ItemAsset asset = Assets.find(EAssetType.ITEM, table.slots[selectedSlot].table[index].item) as ItemAsset;

						string name = "?";
						if (asset != null)
						{
							if (string.IsNullOrEmpty(asset.itemName))
							{
								name = asset.name;
							}
							else
							{
								name = asset.itemName;
							}
						}

						itemButton.Text = table.slots[selectedSlot].table[index].item.ToString() + " " + name;

						itemButton.OnClicked += onClickItemButton;
						spawnsScrollBox.AddChild(itemButton);

						clothButtons[index] = itemButton;
					}
				}
				else
				{
					clothButtons = new ISleekButton[0];
				}

				itemIDField.PositionOffset_Y = 460 + (slotButtons.Length * 70) + (clothButtons.Length * 40);
				addItemButton.PositionOffset_Y = 460 + (slotButtons.Length * 70) + (clothButtons.Length * 40) + 40;
				removeItemButton.PositionOffset_Y = 460 + (slotButtons.Length * 70) + (clothButtons.Length * 40) + 40;

				spawnsScrollBox.ContentSizeOffset = new Vector2(0.0f, 460 + (slotButtons.Length * 70) + (clothButtons.Length * 40) + 70);
			}
			else
			{
				selectedBox.Text = "";
				tableNameField.Text = "";
				tableColorPicker.state = Color.white;
				megaToggle.Value = false;
				healthField.Value = 0;
				damageField.Value = 0;
				lootIndexField.Value = 0;
				lootIDField.Value = 0;
				xpField.Value = 0;
				regenField.Value = 0.0f;
				difficultyGUIDField.Text = string.Empty;

				if (slotButtons != null)
				{
					for (int index = 0; index < slotButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(slotButtons[index]);
					}
				}

				slotButtons = null;

				if (clothButtons != null)
				{
					for (int index = 0; index < clothButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(clothButtons[index]);
					}
				}

				clothButtons = null;

				itemIDField.PositionOffset_Y = 460;
				addItemButton.PositionOffset_Y = 500;
				removeItemButton.PositionOffset_Y = 500;

				spawnsScrollBox.ContentSizeOffset = new Vector2(0.0f, 530);
			}
		}

		private static void onClickedTableButton(ISleekElement button)
		{
			if (EditorSpawns.selectedZombie != (byte) (button.PositionOffset_Y / 40))
			{
				EditorSpawns.selectedZombie = (byte) (button.PositionOffset_Y / 40);
				EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = LevelZombies.tables[EditorSpawns.selectedZombie].color;
			}
			else
			{
				EditorSpawns.selectedZombie = 255;
				EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = Color.white;
			}

			updateSelection();
		}

		private static void onZombieColorPicked(SleekColorPicker picker, Color color)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].color = color;
			}
		}

		private static void onToggledMegaToggle(ISleekToggle toggle, bool state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].isMega = state;
			}
		}

		private static void onHealthFieldTyped(ISleekUInt16Field field, ushort state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].health = state;
			}
		}

		private static void onDamageFieldTyped(ISleekUInt8Field field, byte state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].damage = state;
			}
		}

		private static void onLootIndexFieldTyped(ISleekUInt8Field field, byte state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count && state < LevelItems.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].lootIndex = state;
			}
		}

		private static void onLootIDFieldTyped(ISleekUInt16Field field, ushort state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].lootID = state;
			}
		}

		private static void onXPFieldTyped(ISleekUInt32Field field, uint state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].xp = state;
			}
		}

		private static void onRegenFieldTyped(ISleekFloat32Field field, float state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].regen = state;
			}
		}

		private static void onDifficultyGUIDFieldTyped(ISleekField field, string state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.tables[EditorSpawns.selectedZombie].difficultyGUID = state;
			}
		}

		private static void onClickedSlotButton(ISleekElement button)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				selectedSlot = (byte) ((button.PositionOffset_Y - 460) / 70);
				updateSelection();
			}
		}

		private static void onClickItemButton(ISleekElement button)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				selectItem = (byte) ((button.PositionOffset_Y - 460 - (slotButtons.Length * 70)) / 40);
				updateSelection();
			}
		}

		private static void onDraggedChanceSlider(ISleekSlider slider, float state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				int slotIndex = Mathf.FloorToInt((slider.Parent.PositionOffset_Y - 460) / 70);
				LevelZombies.tables[EditorSpawns.selectedZombie].slots[slotIndex].chance = state;

				ISleekSlider chance = (ISleekSlider) slotButtons[slotIndex].GetChildAtIndex(0);
				chance.UpdateLabel(Mathf.RoundToInt(state * 100) + "%");
			}
		}

		private static void onTypedNameField(ISleekField field, string state)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				selectedBox.Text = state;
				LevelZombies.tables[EditorSpawns.selectedZombie].name = state;

				tableButtons[EditorSpawns.selectedZombie].Text = EditorSpawns.selectedZombie + " " + state + $" ({LevelZombies.tables[EditorSpawns.selectedZombie].tableUniqueId})";
			}
		}

		private static void onClickedAddTableButton(ISleekElement button)
		{
			if (tableNameField.Text != "")
			{
				LevelZombies.addTable(tableNameField.Text);

				tableNameField.Text = "";
				updateTables();

				tableScrollBox.ScrollToBottom();
			}
		}

		private static void onClickedRemoveTableButton(ISleekElement button)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				LevelZombies.removeTable();

				updateTables();
				updateSelection();

				tableScrollBox.ScrollToBottom();
			}
		}

		private static void onClickedAddItemButton(ISleekElement button)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				ItemAsset asset = Assets.find(EAssetType.ITEM, itemIDField.Value) as ItemAsset;

				if (asset != null)
				{
					if (selectedSlot == 0 && asset.type != EItemType.SHIRT)
					{
						return;
					}

					if (selectedSlot == 1 && asset.type != EItemType.PANTS)
					{
						return;
					}

					if ((selectedSlot == 2 || selectedSlot == 3) && asset.type != EItemType.HAT && asset.type != EItemType.BACKPACK && asset.type != EItemType.VEST && asset.type != EItemType.MASK && asset.type != EItemType.GLASSES)
					{
						return;
					}

					LevelZombies.tables[EditorSpawns.selectedZombie].addCloth(selectedSlot, itemIDField.Value);
					updateSelection();

					spawnsScrollBox.ScrollToBottom();
				}

				itemIDField.Value = 0;
			}
		}

		private static void onClickedRemoveItemButton(ISleekElement button)
		{
			if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
			{
				if (selectItem < LevelZombies.tables[EditorSpawns.selectedZombie].slots[selectedSlot].table.Count)
				{
					LevelZombies.tables[EditorSpawns.selectedZombie].removeCloth(selectedSlot, selectItem);
					updateSelection();

					spawnsScrollBox.ScrollToBottom();
				}
			}
		}

		private static void onDraggedRadiusSlider(ISleekSlider slider, float state)
		{
			EditorSpawns.radius = (byte) (EditorSpawns.MIN_REMOVE_SIZE + (state * EditorSpawns.MAX_REMOVE_SIZE));
		}

		private static void onClickedAddButton(ISleekElement button)
		{
			EditorSpawns.spawnMode = ESpawnMode.ADD_ZOMBIE;
		}

		private static void onClickedRemoveButton(ISleekElement button)
		{
			EditorSpawns.spawnMode = ESpawnMode.REMOVE_ZOMBIE;
		}

		public EditorSpawnsZombiesUI()
		{
			localization = Localization.read("/Editor/EditorSpawnsZombies.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorSpawnsZombies");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_X = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);
			active = false;

			tableScrollBox = Glazier.Get().CreateScrollView();
			tableScrollBox.PositionOffset_X = -470;
			tableScrollBox.PositionOffset_Y = 120;
			tableScrollBox.PositionScale_X = 1;
			tableScrollBox.SizeOffset_X = 470;
			tableScrollBox.SizeOffset_Y = 200;
			container.AddChild(tableScrollBox);

			tableNameField = Glazier.Get().CreateStringField();
			tableNameField.PositionOffset_X = -230;
			tableNameField.PositionOffset_Y = 330;
			tableNameField.PositionScale_X = 1;
			tableNameField.SizeOffset_X = 230;
			tableNameField.SizeOffset_Y = 30;
			tableNameField.MaxLength = 64;
			tableNameField.AddLabel(localization.format("TableNameFieldLabelText"), ESleekSide.LEFT);
			tableNameField.OnTextChanged += onTypedNameField;
			container.AddChild(tableNameField);

			addTableButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
			addTableButton.PositionOffset_X = -230;
			addTableButton.PositionOffset_Y = 370;
			addTableButton.PositionScale_X = 1;
			addTableButton.SizeOffset_X = 110;
			addTableButton.SizeOffset_Y = 30;
			addTableButton.text = localization.format("AddTableButtonText");
			addTableButton.tooltip = localization.format("AddTableButtonTooltip");
			addTableButton.onClickedButton += onClickedAddTableButton;
			container.AddChild(addTableButton);

			removeTableButton = new SleekButtonIcon(icons.load<Texture2D>("Remove"));
			removeTableButton.PositionOffset_X = -110;
			removeTableButton.PositionOffset_Y = 370;
			removeTableButton.PositionScale_X = 1;
			removeTableButton.SizeOffset_X = 110;
			removeTableButton.SizeOffset_Y = 30;
			removeTableButton.text = localization.format("RemoveTableButtonText");
			removeTableButton.tooltip = localization.format("RemoveTableButtonTooltip");
			removeTableButton.onClickedButton += onClickedRemoveTableButton;
			container.AddChild(removeTableButton);

			tableButtons = null;
			updateTables();

			spawnsScrollBox = Glazier.Get().CreateScrollView();
			spawnsScrollBox.PositionOffset_X = -470;
			spawnsScrollBox.PositionOffset_Y = 410;
			spawnsScrollBox.PositionScale_X = 1;
			spawnsScrollBox.SizeOffset_X = 470;
			spawnsScrollBox.SizeOffset_Y = -410;
			spawnsScrollBox.SizeScale_Y = 1;
			spawnsScrollBox.ScaleContentToWidth = true;
			spawnsScrollBox.ContentSizeOffset = new Vector2(0.0f, 1000);
			container.AddChild(spawnsScrollBox);

			tableColorPicker = new SleekColorPicker();
			tableColorPicker.PositionOffset_X = 200;
			tableColorPicker.onColorPicked = onZombieColorPicked;
			spawnsScrollBox.AddChild(tableColorPicker);

			megaToggle = Glazier.Get().CreateToggle();
			megaToggle.PositionOffset_X = 240;
			megaToggle.PositionOffset_Y = 130;
			megaToggle.SizeOffset_X = 40;
			megaToggle.SizeOffset_Y = 40;
			megaToggle.OnValueChanged += onToggledMegaToggle;
			megaToggle.AddLabel(localization.format("MegaToggleLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(megaToggle);

			healthField = Glazier.Get().CreateUInt16Field();
			healthField.PositionOffset_X = 240;
			healthField.PositionOffset_Y = 180;
			healthField.SizeOffset_X = 200;
			healthField.SizeOffset_Y = 30;
			healthField.OnValueChanged += onHealthFieldTyped;
			healthField.AddLabel(localization.format("HealthFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(healthField);

			damageField = Glazier.Get().CreateUInt8Field();
			damageField.PositionOffset_X = 240;
			damageField.PositionOffset_Y = 220;
			damageField.SizeOffset_X = 200;
			damageField.SizeOffset_Y = 30;
			damageField.OnValueChanged += onDamageFieldTyped;
			damageField.AddLabel(localization.format("DamageFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(damageField);

			lootIndexField = Glazier.Get().CreateUInt8Field();
			lootIndexField.PositionOffset_X = 240;
			lootIndexField.PositionOffset_Y = 260;
			lootIndexField.SizeOffset_X = 200;
			lootIndexField.SizeOffset_Y = 30;
			lootIndexField.OnValueChanged += onLootIndexFieldTyped;
			lootIndexField.AddLabel(localization.format("LootIndexFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(lootIndexField);

			lootIDField = Glazier.Get().CreateUInt16Field();
			lootIDField.PositionOffset_X = 240;
			lootIDField.PositionOffset_Y = 300;
			lootIDField.SizeOffset_X = 200;
			lootIDField.SizeOffset_Y = 30;
			lootIDField.OnValueChanged += onLootIDFieldTyped;
			lootIDField.AddLabel(localization.format("LootIDFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(lootIDField);

			xpField = Glazier.Get().CreateUInt32Field();
			xpField.PositionOffset_X = 240;
			xpField.PositionOffset_Y = 340;
			xpField.SizeOffset_X = 200;
			xpField.SizeOffset_Y = 30;
			xpField.OnValueChanged += onXPFieldTyped;
			xpField.AddLabel(localization.format("XPFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(xpField);

			regenField = Glazier.Get().CreateFloat32Field();
			regenField.PositionOffset_X = 240;
			regenField.PositionOffset_Y = 380;
			regenField.SizeOffset_X = 200;
			regenField.SizeOffset_Y = 30;
			regenField.OnValueChanged += onRegenFieldTyped;
			regenField.AddLabel(localization.format("RegenFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(regenField);

			difficultyGUIDField = Glazier.Get().CreateStringField();
			difficultyGUIDField.PositionOffset_X = 240;
			difficultyGUIDField.PositionOffset_Y = 420;
			difficultyGUIDField.SizeOffset_X = 200;
			difficultyGUIDField.SizeOffset_Y = 30;
			difficultyGUIDField.MaxLength = 32;
			difficultyGUIDField.OnTextChanged += onDifficultyGUIDFieldTyped;
			difficultyGUIDField.AddLabel(localization.format("DifficultyGUIDFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(difficultyGUIDField);

			itemIDField = Glazier.Get().CreateUInt16Field();
			itemIDField.PositionOffset_X = 240;
			itemIDField.SizeOffset_X = 200;
			itemIDField.SizeOffset_Y = 30;
			itemIDField.AddLabel(localization.format("ItemIDFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(itemIDField);

			addItemButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
			addItemButton.PositionOffset_X = 240;
			addItemButton.SizeOffset_X = 95;
			addItemButton.SizeOffset_Y = 30;
			addItemButton.text = localization.format("AddItemButtonText");
			addItemButton.tooltip = localization.format("AddItemButtonTooltip");
			addItemButton.onClickedButton += onClickedAddItemButton;
			spawnsScrollBox.AddChild(addItemButton);

			removeItemButton = new SleekButtonIcon(icons.load<Texture2D>("Remove"));
			removeItemButton.PositionOffset_X = 345;
			removeItemButton.SizeOffset_X = 95;
			removeItemButton.SizeOffset_Y = 30;
			removeItemButton.text = localization.format("RemoveItemButtonText");
			removeItemButton.tooltip = localization.format("RemoveItemButtonTooltip");
			removeItemButton.onClickedButton += onClickedRemoveItemButton;
			spawnsScrollBox.AddChild(removeItemButton);

			selectedBox = Glazier.Get().CreateBox();
			selectedBox.PositionOffset_X = -230;
			selectedBox.PositionOffset_Y = 80;
			selectedBox.PositionScale_X = 1;
			selectedBox.SizeOffset_X = 230;
			selectedBox.SizeOffset_Y = 30;
			selectedBox.AddLabel(localization.format("SelectionBoxLabelText"), ESleekSide.LEFT);
			container.AddChild(selectedBox);

			slotButtons = null;
			clothButtons = null;
			updateSelection();

			radiusSlider = Glazier.Get().CreateSlider();
			radiusSlider.PositionOffset_Y = -100;
			radiusSlider.PositionScale_Y = 1;
			radiusSlider.SizeOffset_X = 200;
			radiusSlider.SizeOffset_Y = 20;
			radiusSlider.Value = (EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float) EditorSpawns.MAX_REMOVE_SIZE;
			radiusSlider.Orientation = ESleekOrientation.HORIZONTAL;
			radiusSlider.AddLabel(localization.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
			radiusSlider.OnValueChanged += onDraggedRadiusSlider;
			container.AddChild(radiusSlider);

			addButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
			addButton.PositionOffset_Y = -70;
			addButton.PositionScale_Y = 1;
			addButton.SizeOffset_X = 200;
			addButton.SizeOffset_Y = 30;
			addButton.text = localization.format("AddButtonText", ControlsSettings.tool_0);
			addButton.tooltip = localization.format("AddButtonTooltip");
			addButton.onClickedButton += onClickedAddButton;
			container.AddChild(addButton);

			removeButton = new SleekButtonIcon(icons.load<Texture2D>("Remove"));
			removeButton.PositionOffset_Y = -30;
			removeButton.PositionScale_Y = 1;
			removeButton.SizeOffset_X = 200;
			removeButton.SizeOffset_Y = 30;
			removeButton.text = localization.format("RemoveButtonText", ControlsSettings.tool_1);
			removeButton.tooltip = localization.format("RemoveButtonTooltip");
			removeButton.onClickedButton += onClickedRemoveButton;
			container.AddChild(removeButton);
		}
	}
}
