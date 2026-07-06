////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class MenuWorkshopSpawnsUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static ISleekScrollView spawnsBox;
		private static SleekButtonState typeButton;
		private static ISleekField viewIDField;
		private static ISleekButton viewButton;
		private static ISleekButton rawButton;
		private static ISleekButton newButton;
		private static ISleekButton writeButton;
		private static ISleekBox rootsBox;
		private static ISleekBox tablesBox;
		private static ISleekField rawField;
		private static ISleekField addRootIDField;
		private static SleekButtonIcon addRootSpawnButton;
		private static ISleekField addTableIDField;
		private static SleekButtonIcon addTableAssetButton;
		private static SleekButtonIcon addTableSpawnButton;
		private static ISleekButton applyWeightsButton;

		private static SpawnAsset asset;
		private static EAssetType type;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			refresh();

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

		private static SpawnAsset FindCurrentAsset()
		{
			if (ushort.TryParse(viewIDField.Text, out ushort legacyId))
			{
				return Assets.find(EAssetType.SPAWN, legacyId) as SpawnAsset;
			}
			else if (System.Guid.TryParse(viewIDField.Text, out System.Guid guid))
			{
				return Assets.find(guid) as SpawnAsset;
			}
			else
			{
				return null;
			}
		}

		private static void refresh()
		{
			rawField.IsVisible = false;
			rootsBox.IsVisible = true;
			tablesBox.IsVisible = true;

			rootsBox.RemoveAllChildren();
			tablesBox.RemoveAllChildren();

			asset = FindCurrentAsset();

			switch (typeButton.state)
			{
				case 0:
					type = EAssetType.ITEM;
					break;
				case 1:
					type = EAssetType.VEHICLE;
					break;
				case 2:
					type = EAssetType.ANIMAL;
					break;
				default:
					type = EAssetType.NONE;
					return;
			}

			int offset = 120;

			rootsBox.PositionOffset_Y = offset;
			offset += 40;

			if (asset != null)
			{
				rootsBox.Text = localization.format("Roots_Box", asset.name);

				for (int index = 0; index < asset.roots.Count; index++)
				{
					SpawnTable table = asset.roots[index];
					SpawnAsset targetAsset;
					if (table.legacySpawnId != 0)
					{
						targetAsset = Assets.find(EAssetType.SPAWN, table.legacySpawnId) as SpawnAsset;
					}
					else if (!table.targetGuid.IsEmpty())
					{
						targetAsset = Assets.find(table.targetGuid) as SpawnAsset;
					}
					else
					{
						continue;
					}

					ISleekButton rootButton = Glazier.Get().CreateButton();
					rootButton.PositionOffset_Y = 40 + (index * 40);
					rootButton.SizeOffset_X = -260;
					rootButton.SizeScale_X = 1.0f;
					rootButton.SizeOffset_Y = 30;
					rootButton.OnClicked += onClickedRootButton;
					rootsBox.AddChild(rootButton);
					offset += 40;

					if (targetAsset != null)
					{
						rootButton.Text = targetAsset.name;

						if (table.legacySpawnId != 0)
						{
							rootButton.TooltipText = $"{table.legacySpawnId} - {targetAsset.GetOriginName()}";
						}
						else
						{
							rootButton.TooltipText = $"{table.targetGuid:N} - {targetAsset.GetOriginName()}";
						}
					}
					else if (table.legacySpawnId != 0)
					{
						rootButton.Text = $"{table.legacySpawnId} ?";
					}
					else
					{
						rootButton.Text = $"{table.targetGuid:N} ?";
					}

					ISleekInt32Field weightField = Glazier.Get().CreateInt32Field();
					weightField.PositionOffset_X = 10;
					weightField.PositionScale_X = 1.0f;
					weightField.SizeOffset_X = 55;
					weightField.SizeOffset_Y = 30;
					weightField.Value = table.weight;
					weightField.TooltipText = localization.format("Weight_Tooltip");
					weightField.OnValueChanged += onTypedRootWeightField;
					rootButton.AddChild(weightField);

					ISleekBox chanceBox = Glazier.Get().CreateBox();
					chanceBox.PositionOffset_X = 65;
					chanceBox.PositionScale_X = 1.0f;
					chanceBox.SizeOffset_X = 65;
					chanceBox.SizeOffset_Y = 30;
					chanceBox.Text = table.normalizedWeight.ToString("P2");
					chanceBox.TooltipText = localization.format("Chance_Tooltip");
					rootButton.AddChild(chanceBox);

					SleekButtonIcon removeRootButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
					removeRootButton.PositionOffset_X = 140;
					removeRootButton.PositionScale_X = 1.0f;
					removeRootButton.SizeOffset_X = 120;
					removeRootButton.SizeOffset_Y = 30;
					removeRootButton.text = localization.format("Remove_Root_Button");
					removeRootButton.tooltip = localization.format("Remove_Root_Button_Tooltip");
					removeRootButton.onClickedButton += onClickedRemoveRootButton;
					rootButton.AddChild(removeRootButton);
				}

				addRootIDField.PositionOffset_Y = offset;
				addRootSpawnButton.PositionOffset_Y = offset;
				offset += 40;

				addRootIDField.IsVisible = true;
				addRootSpawnButton.IsVisible = true;
			}
			else
			{
				rootsBox.Text = localization.format("Roots_Box", viewIDField.Text + " ?");

				addRootIDField.IsVisible = false;
				addRootSpawnButton.IsVisible = false;
			}

			offset += 40;

			tablesBox.PositionOffset_Y = offset;
			offset += 40;

			if (asset != null)
			{
				tablesBox.Text = localization.format("Tables_Box", asset.name);

				for (int index = 0; index < asset.tables.Count; index++)
				{
					SpawnTable table = asset.tables[index];
					ISleekElement row = null;

					Asset targetAsset;
					SpawnAsset targetSpawnAsset;
					bool isTargetSpawn;
					if (table.legacySpawnId != 0)
					{
						targetAsset = null;
						targetSpawnAsset = Assets.find(EAssetType.SPAWN, table.legacySpawnId) as SpawnAsset;
						isTargetSpawn = true;
					}
					else if (table.legacyAssetId != 0)
					{
						targetAsset = Assets.find(type, table.legacyAssetId);
						targetSpawnAsset = null;
						isTargetSpawn = false;
					}
					else
					{
						targetAsset = Assets.find(table.targetGuid);
						targetSpawnAsset = targetAsset as SpawnAsset;
						isTargetSpawn = targetSpawnAsset != null;
					}

					if (isTargetSpawn)
					{
						ISleekButton spawnButton = Glazier.Get().CreateButton();
						spawnButton.PositionOffset_Y = 40 + (index * 40);
						spawnButton.SizeOffset_X = -260;
						spawnButton.SizeScale_X = 1.0f;
						spawnButton.SizeOffset_Y = 30;
						spawnButton.OnClicked += onClickedTableButton;
						tablesBox.AddChild(spawnButton);
						row = spawnButton;
						offset += 40;

						if (targetSpawnAsset != null)
						{
							spawnButton.Text = targetSpawnAsset.name;

							if (table.legacySpawnId != 0)
							{
								spawnButton.TooltipText = $"{table.legacySpawnId} - {targetSpawnAsset.GetOriginName()}";
							}
							else
							{
								spawnButton.TooltipText = $"{table.targetGuid:N} - {targetSpawnAsset.GetOriginName()}";
							}
						}
						else if (table.legacySpawnId != 0)
						{
							spawnButton.Text = $"{table.legacySpawnId} ?";
						}
						else
						{
							spawnButton.Text = $"{table.targetGuid:N} ?";
						}
					}
					else
					{
						ISleekBox assetBox = Glazier.Get().CreateBox();
						assetBox.PositionOffset_Y = 40 + (index * 40);
						assetBox.SizeOffset_X = -260;
						assetBox.SizeScale_X = 1.0f;
						assetBox.SizeOffset_Y = 30;
						assetBox.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
						tablesBox.AddChild(assetBox);
						row = assetBox;
						offset += 40;

						if (targetAsset != null)
						{
							assetBox.Text = targetAsset.FriendlyName;

							if (targetAsset is ItemAsset itemAsset)
							{
								assetBox.TextColor = ItemTool.getRarityColorUI(itemAsset.rarity);
							}
							else if (targetAsset is VehicleAsset vehicleAsset)
							{
								assetBox.TextColor = ItemTool.getRarityColorUI(vehicleAsset.rarity);
							}

							if (table.legacyAssetId != 0)
							{
								assetBox.TooltipText = $"{table.legacyAssetId} - {targetAsset.GetOriginName()}";
							}
							else
							{
								assetBox.TooltipText = $"{table.targetGuid:N} - {targetAsset.GetOriginName()}";
							}
						}
						else if (table.legacyAssetId != 0)
						{
							assetBox.Text = $"{table.legacyAssetId} ?";
						}
						else
						{
							assetBox.Text = $"{table.targetGuid:N} ?";
						}
					}

					if (row != null)
					{
						ISleekInt32Field weightField = Glazier.Get().CreateInt32Field();
						weightField.PositionOffset_X = 10;
						weightField.PositionScale_X = 1.0f;
						weightField.SizeOffset_X = 55;
						weightField.SizeOffset_Y = 30;
						weightField.Value = table.weight;
						weightField.TooltipText = localization.format("Weight_Tooltip");
						weightField.OnValueChanged += onTypedTableWeightField;
						row.AddChild(weightField);

						float chance = table.normalizedWeight;
						if (index > 0)
						{
							chance -= asset.tables[index - 1].normalizedWeight;
						}

						ISleekBox chanceBox = Glazier.Get().CreateBox();
						chanceBox.PositionOffset_X = 65;
						chanceBox.PositionScale_X = 1.0f;
						chanceBox.SizeOffset_X = 65;
						chanceBox.SizeOffset_Y = 30;
						chanceBox.Text = chance.ToString("P2");
						chanceBox.TooltipText = localization.format("Chance_Tooltip");
						row.AddChild(chanceBox);

						SleekButtonIcon removeTableButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
						removeTableButton.PositionOffset_X = 140;
						removeTableButton.PositionScale_X = 1.0f;
						removeTableButton.SizeOffset_X = 120;
						removeTableButton.SizeOffset_Y = 30;
						removeTableButton.text = localization.format("Remove_Table_Button");
						removeTableButton.tooltip = localization.format("Remove_Table_Button_Tooltip");
						removeTableButton.onClickedButton += onClickedRemoveTableButton;
						row.AddChild(removeTableButton);
					}
				}

				addTableIDField.PositionOffset_Y = offset;
				addTableAssetButton.PositionOffset_Y = offset;
				addTableSpawnButton.PositionOffset_Y = offset;
				offset += 40;

				addTableIDField.IsVisible = true;
				addTableAssetButton.IsVisible = true;
				addTableSpawnButton.IsVisible = true;
			}
			else
			{
				tablesBox.Text = localization.format("Tables_Box", viewIDField.Text + " ?");

				addTableIDField.IsVisible = false;
				addTableAssetButton.IsVisible = false;
				addTableSpawnButton.IsVisible = false;
			}

			if (asset != null)
			{
				applyWeightsButton.PositionOffset_Y = offset;
				offset += 40;

				applyWeightsButton.IsVisible = true;
			}
			else
			{
				applyWeightsButton.IsVisible = false;
			}

			spawnsBox.ContentSizeOffset = new Vector2(0.0f, offset - 10);
		}

		private static string getRaw(SpawnAsset asset)
		{
			string result = null;

			using (System.IO.StringWriter stringWriter = new System.IO.StringWriter())
			using (DatWriter datWriter = new DatWriter(stringWriter))
			{
				datWriter.WriteKeyValue("GUID", asset.GUID);
				datWriter.WriteKeyValue("Type", "Spawn");
				if (asset.id != 0)
				{
					datWriter.WriteKeyValue("ID", asset.id);
				}

				bool hasAnyValidParents = false;
				if (asset.roots != null)
				{
					foreach (SpawnTable tableEntry in asset.roots)
					{
						if (tableEntry.isLink && (tableEntry.weight > 0 || tableEntry.isOverride))
						{
							hasAnyValidParents = true;
							break;
						}
					}
				}

				if (hasAnyValidParents)
				{
					datWriter.WriteEmptyLine();
					datWriter.WriteListStart("Roots");

					foreach (SpawnTable tableEntry in asset.roots)
					{
						if (tableEntry.isLink && (tableEntry.weight > 0 || tableEntry.isOverride))
						{
							datWriter.WriteDictionaryStart();
							tableEntry.Write(datWriter, type);
							datWriter.WriteDictionaryEnd();
						}
					}

					datWriter.WriteListEnd();
				}

				bool hasAnyValidChildren = false;
				if (asset.tables != null)
				{
					foreach (SpawnTable tableEntry in asset.tables)
					{
						if (!tableEntry.isLink && tableEntry.weight > 0)
						{
							hasAnyValidChildren = true;
							break;
						}
					}
				}

				if (hasAnyValidChildren)
				{
					datWriter.WriteEmptyLine();
					datWriter.WriteListStart("Tables");

					foreach (SpawnTable tableEntry in asset.tables)
					{
						if (!tableEntry.isLink && tableEntry.weight > 0)
						{
							datWriter.WriteDictionaryStart();
							tableEntry.Write(datWriter, type);
							datWriter.WriteDictionaryEnd();
						}
					}

					datWriter.WriteListEnd();
				}

				result = stringWriter.ToString();
			}

			return result;
		}

		private static void raw()
		{
			rawField.IsVisible = true;
			rootsBox.IsVisible = false;
			tablesBox.IsVisible = false;
			addRootIDField.IsVisible = false;
			addRootSpawnButton.IsVisible = false;
			addTableIDField.IsVisible = false;
			addTableAssetButton.IsVisible = false;
			addTableSpawnButton.IsVisible = false;
			applyWeightsButton.IsVisible = false;

			asset = FindCurrentAsset();
			string export;
			if (asset != null)
			{
				export = getRaw(asset);
			}
			else
			{
				export = "?";
			}

			rawField.Text = export;
			GUIUtility.systemCopyBuffer = export;

			spawnsBox.ContentSizeOffset = new Vector2(0.0f, 1080);
		}

		private static void write()
		{
			asset = FindCurrentAsset();
			if (asset == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(asset.absoluteOriginFilePath) || !System.IO.File.Exists(asset.absoluteOriginFilePath))
			{
				return;
			}

			string export = getRaw(asset);
			System.IO.File.WriteAllText(asset.absoluteOriginFilePath, export);
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
		}

		private static void onClickedViewButton(ISleekElement button)
		{
			refresh();
		}

		private static void onClickedRawButton(ISleekElement button)
		{
			raw();
		}

		private static void onClickedNewButton(ISleekElement button)
		{
			ushort legacyId;
			ushort.TryParse(viewIDField.Text, out legacyId);
			SpawnAsset created = Assets.CreateAtRuntime<SpawnAsset>(legacyId);
			if (created != null)
			{
				viewIDField.Text = created.GUID.ToString("N");
				refresh();
			}
		}

		private static void onClickedWriteButton(ISleekElement button)
		{
			write();
		}

		private static void onClickedRootButton(ISleekElement button)
		{
			int index = rootsBox.FindIndexOfChild(button);
			SpawnTable tableEntry = asset.roots[index];
			if (tableEntry.legacySpawnId != 0)
			{
				viewIDField.Text = tableEntry.legacySpawnId.ToString();
			}
			else
			{
				viewIDField.Text = tableEntry.targetGuid.ToString("N");
			}

			refresh();
		}

		private static void onClickedTableButton(ISleekElement button)
		{
			int index = tablesBox.FindIndexOfChild(button);
			SpawnTable tableEntry = asset.tables[index];
			if (tableEntry.legacySpawnId != 0)
			{
				viewIDField.Text = tableEntry.legacySpawnId.ToString();
			}
			else
			{
				viewIDField.Text = tableEntry.targetGuid.ToString("N");
			}

			refresh();
		}

		private static void onTypedRootWeightField(ISleekInt32Field field, int state)
		{
			int index = rootsBox.FindIndexOfChild(field.Parent);

			asset.roots[index].weight = state;
		}

		private static void onClickedAddRootSpawnButton(ISleekElement button)
		{
			SpawnAsset foundAsset;
			if (ushort.TryParse(addRootIDField.Text, out ushort legacyId))
			{
				foundAsset = Assets.find(EAssetType.SPAWN, legacyId) as SpawnAsset;
			}
			else if (System.Guid.TryParse(addRootIDField.Text, out System.Guid guid))
			{
				foundAsset = Assets.find<SpawnAsset>(guid);
			}
			else
			{
				foundAsset = null;
			}

			if (foundAsset == null)
			{
				UnturnedLog.info($"Spawns editor unable to find parent spawn asset matching \"{addRootIDField.Text}\"");
				return;
			}

			foreach (SpawnTable tableEntry in asset.roots)
			{
				if ((tableEntry.legacySpawnId != 0 && tableEntry.legacySpawnId == foundAsset.id) || tableEntry.targetGuid == foundAsset.GUID)
				{
					UnturnedLog.info($"Spawns editor current asset {asset.FriendlyName} already contains parent {foundAsset.FriendlyName}");
					return;
				}
			}

			SpawnTable root = new SpawnTable();
			root.targetGuid = foundAsset.GUID;
			root.isLink = true;
			asset.roots.Add(root);

			SpawnTable table = new SpawnTable();
			table.targetGuid = asset.GUID;
			table.isLink = true;
			foundAsset.tables.Add(table);
			foundAsset.markTablesDirty();

			addRootIDField.Text = string.Empty;
			refresh();
		}

		private static void onClickedRemoveRootButton(ISleekElement button)
		{
			int index = rootsBox.FindIndexOfChild(button.Parent);
			asset.EditorRemoveParentAtIndex(index);
			refresh();
		}

		private static void onTypedTableWeightField(ISleekInt32Field field, int state)
		{
			int index = tablesBox.FindIndexOfChild(field.Parent);
			asset.setTableWeightAtIndex(index, state);
		}

		private static void onClickedAddTableAssetButton(ISleekElement button)
		{
			Asset foundAsset;
			if (ushort.TryParse(addTableIDField.Text, out ushort legacyId))
			{
				foundAsset = Assets.find(type, legacyId);
			}
			else if (System.Guid.TryParse(addTableIDField.Text, out System.Guid guid))
			{
				foundAsset = Assets.find(guid);
			}
			else
			{
				foundAsset = null;
			}

			if (foundAsset == null)
			{
				UnturnedLog.info($"Spawns editor unable to find child asset matching \"{addTableIDField.Text}\"");
				return;
			}

			foreach (SpawnTable tableEntry in asset.tables)
			{
				if ((tableEntry.legacyAssetId != 0 && tableEntry.legacyAssetId == foundAsset.id) || tableEntry.targetGuid == foundAsset.GUID)
				{
					UnturnedLog.info($"Spawns editor current asset {asset.FriendlyName} already contains child asset {foundAsset.FriendlyName}");
					return;
				}
			}

			asset.EditorAddChild(foundAsset);

			addTableIDField.Text = string.Empty;
			refresh();
		}

		private static void onClickedAddTableSpawnButton(ISleekElement button)
		{
			SpawnAsset foundAsset;
			if (ushort.TryParse(addTableIDField.Text, out ushort legacyId))
			{
				foundAsset = Assets.find(EAssetType.SPAWN, legacyId) as SpawnAsset;
			}
			else if (System.Guid.TryParse(addTableIDField.Text, out System.Guid guid))
			{
				foundAsset = Assets.find(guid) as SpawnAsset;
			}
			else
			{
				foundAsset = null;
			}

			if (foundAsset == null)
			{
				UnturnedLog.info($"Spawns editor unable to find child spawn matching \"{addTableIDField.Text}\"");
				return;
			}

			foreach (SpawnTable tableEntry in asset.tables)
			{
				if ((tableEntry.legacySpawnId != 0 && tableEntry.legacySpawnId == foundAsset.id) || tableEntry.targetGuid == foundAsset.GUID)
				{
					UnturnedLog.info($"Spawns editor current asset {asset.FriendlyName} already contains child spawn {foundAsset.FriendlyName}");
					return;
				}
			}

			asset.EditorAddChild(foundAsset);

			addTableIDField.Text = string.Empty;
			refresh();
		}

		private static void onClickedRemoveTableButton(ISleekElement button)
		{
			int index = tablesBox.FindIndexOfChild(button.Parent);
			asset.EditorRemoveChildAtIndex(index);
			refresh();
		}

		private static void onClickedApplyWeightsButton(ISleekElement button)
		{
			asset.sortAndNormalizeWeights();
			refresh();
		}

		public MenuWorkshopSpawnsUI()
		{
			localization = Localization.read("/Menu/Workshop/MenuWorkshopSpawns.dat");

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

			spawnsBox = Glazier.Get().CreateScrollView();
			spawnsBox.PositionOffset_X = -315;
			spawnsBox.PositionOffset_Y = 100;
			spawnsBox.PositionScale_X = 0.5f;
			spawnsBox.SizeOffset_X = 630;
			spawnsBox.SizeOffset_Y = -200;
			spawnsBox.SizeScale_Y = 1;
			spawnsBox.ScaleContentToWidth = true;
			container.AddChild(spawnsBox);

			typeButton = new SleekButtonState(new GUIContent(localization.format("Type_Item")), new GUIContent(localization.format("Type_Vehicle")), new GUIContent(localization.format("Type_Animal")));
			typeButton.SizeOffset_X = 600;
			typeButton.SizeOffset_Y = 30;
			typeButton.tooltip = localization.format("Type_Tooltip");
			spawnsBox.AddChild(typeButton);

			viewIDField = Glazier.Get().CreateStringField();
			viewIDField.PositionOffset_Y = 40;
			viewIDField.SizeOffset_X = 160;
			viewIDField.SizeOffset_Y = 30;
			viewIDField.PlaceholderText = localization.format("ID_Field_Hint");
			spawnsBox.AddChild(viewIDField);

			viewButton = Glazier.Get().CreateButton();
			viewButton.PositionOffset_X = 170;
			viewButton.PositionOffset_Y = 40;
			viewButton.SizeOffset_X = 100;
			viewButton.SizeOffset_Y = 30;
			viewButton.Text = localization.format("View_Button");
			viewButton.TooltipText = localization.format("View_Button_Tooltip");
			viewButton.OnClicked += onClickedViewButton;
			spawnsBox.AddChild(viewButton);

			rawButton = Glazier.Get().CreateButton();
			rawButton.PositionOffset_X = 280;
			rawButton.PositionOffset_Y = 40;
			rawButton.SizeOffset_X = 100;
			rawButton.SizeOffset_Y = 30;
			rawButton.Text = localization.format("Raw_Button");
			rawButton.TooltipText = localization.format("Raw_Button_Tooltip");
			rawButton.OnClicked += onClickedRawButton;
			spawnsBox.AddChild(rawButton);

			newButton = Glazier.Get().CreateButton();
			newButton.PositionOffset_X = 390;
			newButton.PositionOffset_Y = 40;
			newButton.SizeOffset_X = 100;
			newButton.SizeOffset_Y = 30;
			newButton.Text = localization.format("New_Button");
			newButton.TooltipText = localization.format("New_Button_Tooltip");
			newButton.OnClicked += onClickedNewButton;
			spawnsBox.AddChild(newButton);

			writeButton = Glazier.Get().CreateButton();
			writeButton.PositionOffset_X = 500;
			writeButton.PositionOffset_Y = 40;
			writeButton.SizeOffset_X = 100;
			writeButton.SizeOffset_Y = 30;
			writeButton.Text = localization.format("Write_Button");
			writeButton.TooltipText = localization.format("Write_Button_Tooltip");
			writeButton.OnClicked += onClickedWriteButton;
			spawnsBox.AddChild(writeButton);

			addRootIDField = Glazier.Get().CreateStringField();
			addRootIDField.SizeOffset_X = 470;
			addRootIDField.SizeOffset_Y = 30;
			addRootIDField.PlaceholderText = localization.format("ID_Field_Hint");
			spawnsBox.AddChild(addRootIDField);

			addRootSpawnButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
			addRootSpawnButton.PositionOffset_X = 480;
			addRootSpawnButton.SizeOffset_X = 120;
			addRootSpawnButton.SizeOffset_Y = 30;
			addRootSpawnButton.text = localization.format("Add_Root_Spawn_Button");
			addRootSpawnButton.tooltip = localization.format("Add_Root_Spawn_Button_Tooltip");
			addRootSpawnButton.onClickedButton += onClickedAddRootSpawnButton;
			spawnsBox.AddChild(addRootSpawnButton);

			addTableIDField = Glazier.Get().CreateStringField();
			addTableIDField.SizeOffset_X = 340;
			addTableIDField.SizeOffset_Y = 30;
			addTableIDField.PlaceholderText = localization.format("ID_Field_Hint");
			spawnsBox.AddChild(addTableIDField);

			addTableAssetButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
			addTableAssetButton.PositionOffset_X = 350;
			addTableAssetButton.SizeOffset_X = 120;
			addTableAssetButton.SizeOffset_Y = 30;
			addTableAssetButton.text = localization.format("Add_Table_Asset_Button");
			addTableAssetButton.tooltip = localization.format("Add_Table_Asset_Button_Tooltip");
			addTableAssetButton.onClickedButton += onClickedAddTableAssetButton;
			spawnsBox.AddChild(addTableAssetButton);

			addTableSpawnButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
			addTableSpawnButton.PositionOffset_X = 480;
			addTableSpawnButton.SizeOffset_X = 120;
			addTableSpawnButton.SizeOffset_Y = 30;
			addTableSpawnButton.text = localization.format("Add_Table_Spawn_Button");
			addTableSpawnButton.tooltip = localization.format("Add_Table_Spawn_Button_Tooltip");
			addTableSpawnButton.onClickedButton += onClickedAddTableSpawnButton;
			spawnsBox.AddChild(addTableSpawnButton);

			applyWeightsButton = Glazier.Get().CreateButton();
			applyWeightsButton.SizeOffset_X = 600;
			applyWeightsButton.SizeOffset_Y = 30;
			applyWeightsButton.Text = localization.format("Apply_Weights_Button");
			applyWeightsButton.TooltipText = localization.format("Apply_Weights_Button_Tooltip");
			applyWeightsButton.OnClicked += onClickedApplyWeightsButton;
			spawnsBox.AddChild(applyWeightsButton);

			rootsBox = Glazier.Get().CreateBox();
			rootsBox.PositionOffset_Y = 40;
			rootsBox.SizeOffset_X = 600;
			rootsBox.SizeOffset_Y = 30;
			spawnsBox.AddChild(rootsBox);

			tablesBox = Glazier.Get().CreateBox();
			tablesBox.PositionOffset_Y = 80;
			tablesBox.SizeOffset_X = 600;
			tablesBox.SizeOffset_Y = 30;
			spawnsBox.AddChild(tablesBox);

			rawField = Glazier.Get().CreateStringField();
			rawField.PositionOffset_Y = 80;
			rawField.SizeOffset_X = 600;
			rawField.SizeOffset_Y = 1000;
			rawField.IsMultiline = true;
			rawField.MaxLength = 4096;
			rawField.TextAlignment = TextAnchor.UpperLeft;
			spawnsBox.AddChild(rawField);

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
