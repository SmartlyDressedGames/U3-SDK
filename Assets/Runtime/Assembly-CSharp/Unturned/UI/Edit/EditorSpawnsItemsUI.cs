////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorSpawnsItemsUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekScrollView tableScrollBox;
		private static ISleekScrollView spawnsScrollBox;

		private static ISleekButton[] tableButtons;
		private static ISleekButton[] tierButtons;
		private static ISleekButton[] itemButtons;

		private static SleekColorPicker tableColorPicker;
		private static ISleekUInt16Field tableIDField;
		private static ISleekField tierNameField;
		private static SleekButtonIcon addTierButton;
		private static SleekButtonIcon removeTierButton;
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

		private static byte selectedTier;
		private static byte selectItem;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			EditorSpawns.isSpawning = true;
			EditorSpawns.spawnMode = ESpawnMode.ADD_ITEM;

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

			tableButtons = new ISleekButton[LevelItems.tables.Count];

			tableScrollBox.ContentSizeOffset = new Vector2(0.0f, (tableButtons.Length * 40) - 10);
			for (int index = 0; index < tableButtons.Length; index++)
			{
				ISleekButton tableButton = Glazier.Get().CreateButton();
				tableButton.PositionOffset_X = 240;
				tableButton.PositionOffset_Y = index * 40;
				tableButton.SizeOffset_X = 200;
				tableButton.SizeOffset_Y = 30;
				tableButton.Text = index + " " + LevelItems.tables[index].name;
				tableButton.OnClicked += onClickedTableButton;
				tableScrollBox.AddChild(tableButton);

				tableButtons[index] = tableButton;
			}
		}

		public static void updateSelection()
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				ItemTable table = LevelItems.tables[EditorSpawns.selectedItem];

				selectedBox.Text = table.name;
				tableNameField.Text = table.name;
				tableIDField.Value = table.tableID;
				tableColorPicker.state = table.color;

				if (tierButtons != null)
				{
					for (int index = 0; index < tierButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(tierButtons[index]);
					}
				}

				tierButtons = new ISleekButton[table.tiers.Count];

				for (int index = 0; index < tierButtons.Length; index++)
				{
					ItemTier tier = table.tiers[index];

					ISleekButton tierButton = Glazier.Get().CreateButton();
					tierButton.PositionOffset_X = 240;
					tierButton.PositionOffset_Y = 170 + (index * 70);
					tierButton.SizeOffset_X = 200;
					tierButton.SizeOffset_Y = 30;
					tierButton.Text = tier.name;
					tierButton.OnClicked += onClickedTierButton;
					spawnsScrollBox.AddChild(tierButton);

					ISleekSlider chanceSlider = Glazier.Get().CreateSlider();
					chanceSlider.PositionOffset_Y = 40;
					chanceSlider.SizeOffset_X = 200;
					chanceSlider.SizeOffset_Y = 20;
					chanceSlider.Orientation = ESleekOrientation.HORIZONTAL;
					chanceSlider.Value = tier.chance;
					chanceSlider.AddLabel(Mathf.RoundToInt(tier.chance * 100) + "%", ESleekSide.LEFT);
					chanceSlider.OnValueChanged += onDraggedChanceSlider;
					tierButton.AddChild(chanceSlider);

					tierButtons[index] = tierButton;
				}

				tierNameField.PositionOffset_Y = 170 + (tierButtons.Length * 70);
				addTierButton.PositionOffset_Y = 170 + (tierButtons.Length * 70) + 40;
				removeTierButton.PositionOffset_Y = 170 + (tierButtons.Length * 70) + 40;

				if (itemButtons != null)
				{
					for (int index = 0; index < itemButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(itemButtons[index]);
					}
				}

				if (selectedTier < table.tiers.Count)
				{
					tierNameField.Text = table.tiers[selectedTier].name;

					itemButtons = new ISleekButton[table.tiers[selectedTier].table.Count];

					for (int index = 0; index < itemButtons.Length; index++)
					{
						ISleekButton itemButton = Glazier.Get().CreateButton();
						itemButton.PositionOffset_X = 240;
						itemButton.PositionOffset_Y = 170 + (tierButtons.Length * 70) + 80 + (index * 40);
						itemButton.SizeOffset_X = 200;
						itemButton.SizeOffset_Y = 30;

						ItemAsset asset = Assets.find(EAssetType.ITEM, table.tiers[selectedTier].table[index].item) as ItemAsset;

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

						itemButton.Text = table.tiers[selectedTier].table[index].item.ToString() + " " + name;

						itemButton.OnClicked += onClickItemButton;
						spawnsScrollBox.AddChild(itemButton);

						itemButtons[index] = itemButton;
					}
				}
				else
				{
					tierNameField.Text = "";

					itemButtons = new ISleekButton[0];
				}

				itemIDField.PositionOffset_Y = 170 + (tierButtons.Length * 70) + 80 + (itemButtons.Length * 40);
				addItemButton.PositionOffset_Y = 170 + (tierButtons.Length * 70) + 80 + (itemButtons.Length * 40) + 40;
				removeItemButton.PositionOffset_Y = 170 + (tierButtons.Length * 70) + 80 + (itemButtons.Length * 40) + 40;

				spawnsScrollBox.ContentSizeOffset = new Vector2(0.0f, 170 + (tierButtons.Length * 70) + 80 + (itemButtons.Length * 40) + 70);
			}
			else
			{
				selectedBox.Text = "";
				tableNameField.Text = "";
				tableIDField.Value = 0;
				tableColorPicker.state = Color.white;

				if (tierButtons != null)
				{
					for (int index = 0; index < tierButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(tierButtons[index]);
					}
				}

				tierButtons = null;

				tierNameField.Text = "";
				tierNameField.PositionOffset_Y = 170;
				addTierButton.PositionOffset_Y = 210;
				removeTierButton.PositionOffset_Y = 210;

				if (itemButtons != null)
				{
					for (int index = 0; index < itemButtons.Length; index++)
					{
						spawnsScrollBox.RemoveChild(itemButtons[index]);
					}
				}

				itemButtons = null;

				itemIDField.PositionOffset_Y = 250;
				addItemButton.PositionOffset_Y = 290;
				removeItemButton.PositionOffset_Y = 290;

				spawnsScrollBox.ContentSizeOffset = new Vector2(0.0f, 320);
			}
		}

		private static void onClickedTableButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem != (byte) (button.PositionOffset_Y / 40))
			{
				EditorSpawns.selectedItem = (byte) (button.PositionOffset_Y / 40);
				EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = LevelItems.tables[EditorSpawns.selectedItem].color;
			}
			else
			{
				EditorSpawns.selectedItem = 255;
				EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = Color.white;
			}

			updateSelection();
		}

		private static void onItemColorPicked(SleekColorPicker picker, Color color)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				LevelItems.tables[EditorSpawns.selectedItem].color = color;
			}
		}

		private static void onTableIDFieldTyped(ISleekUInt16Field field, ushort state)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				LevelItems.tables[EditorSpawns.selectedItem].tableID = state;
			}
		}

		private static void onClickedTierButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				if (selectedTier != (byte) ((button.PositionOffset_Y - 170) / 70))
				{
					selectedTier = (byte) ((button.PositionOffset_Y - 170) / 70);
				}
				else
				{
					selectedTier = 255;
				}

				updateSelection();
			}
		}

		private static void onClickItemButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				selectItem = (byte) ((button.PositionOffset_Y - 170 - (tierButtons.Length * 70) - 80) / 40);
				updateSelection();
			}
		}

		private static void onDraggedChanceSlider(ISleekSlider slider, float state)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				int tierIndex = Mathf.FloorToInt((slider.Parent.PositionOffset_Y - 170) / 70);
				LevelItems.tables[EditorSpawns.selectedItem].updateChance(tierIndex, state);

				for (int index = 0; index < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count; index++)
				{
					ItemTier tier = LevelItems.tables[EditorSpawns.selectedItem].tiers[index];
					ISleekSlider chance = (ISleekSlider) tierButtons[index].GetChildAtIndex(0);

					if (index != tierIndex)
					{
						chance.Value = tier.chance;
					}

					chance.UpdateLabel(Mathf.RoundToInt(tier.chance * 100) + "%");
				}
			}
		}

		private static void onTypedTableNameField(ISleekField field, string state)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				selectedBox.Text = state;
				LevelItems.tables[EditorSpawns.selectedItem].name = state;

				tableButtons[EditorSpawns.selectedItem].Text = EditorSpawns.selectedItem + " " + state;
			}
		}

		private static void onClickedAddTableButton(ISleekElement button)
		{
			if (tableNameField.Text != "")
			{
				LevelItems.addTable(tableNameField.Text);

				tableNameField.Text = "";
				updateTables();

				tableScrollBox.ScrollToBottom();
			}
		}

		private static void onClickedRemoveTableButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				LevelItems.removeTable();

				updateTables();
				updateSelection();

				tableScrollBox.ScrollToBottom();
			}
		}

		private static void onTypedTierNameField(ISleekField field, string state)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				if (selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
				{
					LevelItems.tables[EditorSpawns.selectedItem].tiers[selectedTier].name = state;

					tierButtons[selectedTier].Text = state;
				}
			}
		}

		private static void onClickedAddTierButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				if (tierNameField.Text != "")
				{
					LevelItems.tables[EditorSpawns.selectedItem].addTier(tierNameField.Text);

					tierNameField.Text = "";
					updateSelection();
				}
			}
		}

		private static void onClickedRemoveTierButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count)
			{
				if (selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
				{
					LevelItems.tables[EditorSpawns.selectedItem].removeTier(selectedTier);
					updateSelection();
				}
			}
		}

		private static void onClickedAddItemButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count && selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
			{
				ItemAsset asset = Assets.find(EAssetType.ITEM, itemIDField.Value) as ItemAsset;

				if (asset != null && !asset.isPro)
				{
					LevelItems.tables[EditorSpawns.selectedItem].addItem(selectedTier, itemIDField.Value);
					updateSelection();

					spawnsScrollBox.ScrollToBottom();
				}

				itemIDField.Value = 0;
			}
		}

		private static void onClickedRemoveItemButton(ISleekElement button)
		{
			if (EditorSpawns.selectedItem < LevelItems.tables.Count && selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
			{
				if (selectItem < LevelItems.tables[EditorSpawns.selectedItem].tiers[selectedTier].table.Count)
				{
					LevelItems.tables[EditorSpawns.selectedItem].removeItem(selectedTier, selectItem);
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
			EditorSpawns.spawnMode = ESpawnMode.ADD_ITEM;
		}

		private static void onClickedRemoveButton(ISleekElement button)
		{
			EditorSpawns.spawnMode = ESpawnMode.REMOVE_ITEM;
		}

		public EditorSpawnsItemsUI()
		{
			Local localization = Localization.read("/Editor/EditorSpawnsItems.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorSpawnsItems");

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
			tableScrollBox.ScaleContentToWidth = true;
			container.AddChild(tableScrollBox);

			tableNameField = Glazier.Get().CreateStringField();
			tableNameField.PositionOffset_X = -230;
			tableNameField.PositionOffset_Y = 330;
			tableNameField.PositionScale_X = 1;
			tableNameField.SizeOffset_X = 230;
			tableNameField.SizeOffset_Y = 30;
			tableNameField.MaxLength = 64;
			tableNameField.AddLabel(localization.format("TableNameFieldLabelText"), ESleekSide.LEFT);
			tableNameField.OnTextChanged += onTypedTableNameField;
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
			tableColorPicker.onColorPicked = onItemColorPicked;
			spawnsScrollBox.AddChild(tableColorPicker);

			tableIDField = Glazier.Get().CreateUInt16Field();
			tableIDField.PositionOffset_X = 240;
			tableIDField.PositionOffset_Y = 130;
			tableIDField.SizeOffset_X = 200;
			tableIDField.SizeOffset_Y = 30;
			tableIDField.OnValueChanged += onTableIDFieldTyped;
			tableIDField.AddLabel(localization.format("TableIDFieldLabelText"), ESleekSide.LEFT);
			spawnsScrollBox.AddChild(tableIDField);

			tierNameField = Glazier.Get().CreateStringField();
			tierNameField.PositionOffset_X = 240;
			tierNameField.SizeOffset_X = 200;
			tierNameField.SizeOffset_Y = 30;
			tierNameField.MaxLength = 64;
			tierNameField.AddLabel(localization.format("TierNameFieldLabelText"), ESleekSide.LEFT);
			tierNameField.OnTextChanged += onTypedTierNameField;
			spawnsScrollBox.AddChild(tierNameField);

			addTierButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
			addTierButton.PositionOffset_X = 240;
			addTierButton.SizeOffset_X = 95;
			addTierButton.SizeOffset_Y = 30;
			addTierButton.text = localization.format("AddTierButtonText");
			addTierButton.tooltip = localization.format("AddTierButtonTooltip");
			addTierButton.onClickedButton += onClickedAddTierButton;
			spawnsScrollBox.AddChild(addTierButton);

			removeTierButton = new SleekButtonIcon(icons.load<Texture2D>("Remove"));
			removeTierButton.PositionOffset_X = 345;
			removeTierButton.SizeOffset_X = 95;
			removeTierButton.SizeOffset_Y = 30;
			removeTierButton.text = localization.format("RemoveTierButtonText");
			removeTierButton.tooltip = localization.format("RemoveTierButtonTooltip");
			removeTierButton.onClickedButton += onClickedRemoveTierButton;
			spawnsScrollBox.AddChild(removeTierButton);

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

			tierButtons = null;
			itemButtons = null;
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
