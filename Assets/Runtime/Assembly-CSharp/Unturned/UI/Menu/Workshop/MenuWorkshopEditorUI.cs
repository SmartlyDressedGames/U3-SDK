////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuWorkshopEditorUI
	{
		public static IconsBundle icons;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static LevelInfo[] levels;

		private static ISleekBox previewBox;
		private static ISleekImage previewImage;

		private static ISleekScrollView levelScrollBox;
		private static SleekLevel[] levelButtons;

		private static ISleekField mapNameField;
		private static SleekButtonState mapSizeState;
		private static SleekButtonState mapTypeState;
		private static SleekButtonIcon addButton;
		private static SleekButtonIconConfirm removeButton;
		private static SleekButtonIcon editButton;
		private static ISleekBox selectedBox;
		private static ISleekBox descriptionBox;

		private static LevelInfo selectedLevel;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			removeButton.reset();

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

		private static void SyncSelectedLevelDetails()
		{
			if (previewImage.Texture != null && previewImage.ShouldDestroyTexture)
			{
				Object.Destroy(previewImage.Texture);
				previewImage.Texture = null;
			}

			if (selectedLevel == null)
			{
				descriptionBox.Text = string.Empty;
				selectedBox.Text = string.Empty;
				return;
			}

			Local localization = selectedLevel.getLocalization();
			if (localization != null)
			{
				string desc = localization.format("Description");
				desc = ItemTool.filterRarityRichText(desc);
				RichTextUtil.replaceNewlineMarkup(ref desc);

				descriptionBox.Text = desc;
			}

			if (localization != null && localization.has("Name"))
			{
				selectedBox.Text = localization.format("Name");
			}
			else
			{
				selectedBox.Text = selectedLevel.name;
			}

			string previewPath = selectedLevel.GetPreviewImageFilePath();
			if (!string.IsNullOrEmpty(previewPath))
			{
				previewImage.Texture = ReadWrite.readTextureFromFile(previewPath);
			}
		}

		private static void onClickedLevel(SleekLevel level, byte index)
		{
			SetAndSaveLevelSelection(level.level);
			SyncSelectedLevelDetails();
		}

		private static void onClickedAddButton(ISleekElement button)
		{
			if (mapNameField.Text != "")
			{
				Level.add(mapNameField.Text, (ELevelSize) (mapSizeState.state + 1), mapTypeState.state == 0 ? ELevelType.SURVIVAL : ELevelType.ARENA);

				mapNameField.Text = "";
			}
		}

		private static void onClickedRemoveButton(SleekButtonIconConfirm button)
		{
			if (selectedLevel != null && selectedLevel.isEditable)
			{
				// This invokes onLevelsRefreshed which updates UI.
				Level.Remove(selectedLevel);
			}
		}

		private static void onClickedEditButton(ISleekElement button)
		{
			Level.UpdateLevelReference(ref selectedLevel);
			if (selectedLevel != null && selectedLevel.isEditable && !selectedLevel.IsMissingAnyDependencies())
			{
				Level.edit(selectedLevel);
			}
		}

		protected void OnClickedBrowseFilesButton(ISleekElement button)
		{
			if (selectedLevel != null && selectedLevel.isEditable)
			{
				ReadWrite.OpenFileBrowser(selectedLevel.path);
			}
		}

		private static void onLevelsRefreshed()
		{
			if (levelScrollBox == null)
				return;

			levelScrollBox.RemoveAllChildren();

			levels = Level.getLevels(ESingleplayerMapCategory.EDITABLE);

			levelButtons = new SleekLevel[levels.Length];
			for (int index = 0; index < levels.Length; index++)
			{
				if (levels[index] != null)
				{
					SleekLevel level = new SleekEditorLevel(levels[index]);
					level.PositionOffset_Y = index * 110;
					level.onClickedLevel = onClickedLevel;
					levelScrollBox.AddChild(level);

					levelButtons[index] = level;
				}
			}

			selectedLevel = Level.FindLevel(PlaySettings.editorLevelSelection);
			if (selectedLevel == null && levels.Length > 0)
			{
				SetAndSaveLevelSelection(levels[0]);
			}

			SyncSelectedLevelDetails();

			levelScrollBox.ContentSizeOffset = new Vector2(0.0f, (levels.Length * 110) - 10);
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
		}

		private static void SetAndSaveLevelSelection(LevelInfo newLevel)
		{
			selectedLevel = newLevel;
			if (newLevel != null)
			{
				PlaySettings.editorLevelSelection = new SavedLevelSelection(newLevel);
			}
			else
			{
				PlaySettings.editorLevelSelection.Clear();
			}
		}

		public void OnDestroy()
		{
			Level.onLevelsRefreshed -= onLevelsRefreshed;
		}

		public MenuWorkshopEditorUI()
		{
			selectedLevel = null;

			Local localization = Localization.read("/Menu/Workshop/MenuWorkshopEditor.dat");
			icons = Bundles.getIconsBundle("UI/Menu/Icons/Workshop/MenuWorkshopEditor");

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

			previewBox = Glazier.Get().CreateBox();
			previewBox.PositionOffset_X = -305;
			previewBox.PositionOffset_Y = 80;
			previewBox.PositionScale_X = 0.5f;
			previewBox.SizeOffset_X = 340;
			previewBox.SizeOffset_Y = 200;
			container.AddChild(previewBox);

			// Preview.png is 320x180
			previewImage = Glazier.Get().CreateImage();
			previewImage.PositionOffset_X = 10;
			previewImage.PositionOffset_Y = 10;
			previewImage.SizeOffset_X = -20;
			previewImage.SizeOffset_Y = -20;
			previewImage.SizeScale_X = 1;
			previewImage.SizeScale_Y = 1;
			previewImage.ShouldDestroyTexture = true;
			previewBox.AddChild(previewImage);

			levelScrollBox = Glazier.Get().CreateScrollView();
			levelScrollBox.PositionOffset_X = -95;
			levelScrollBox.PositionOffset_Y = 290;
			levelScrollBox.PositionScale_X = 0.5f;
			levelScrollBox.SizeOffset_X = 430;
			levelScrollBox.SizeOffset_Y = -390;
			levelScrollBox.SizeScale_Y = 1;
			levelScrollBox.ScaleContentToWidth = true;
			container.AddChild(levelScrollBox);

			selectedBox = Glazier.Get().CreateBox();
			selectedBox.PositionOffset_X = 45;
			selectedBox.PositionOffset_Y = 80;
			selectedBox.PositionScale_X = 0.5f;
			selectedBox.SizeOffset_X = 260;
			selectedBox.SizeOffset_Y = 30;
			container.AddChild(selectedBox);

			descriptionBox = Glazier.Get().CreateBox();
			descriptionBox.PositionOffset_X = 45;
			descriptionBox.PositionOffset_Y = 120;
			descriptionBox.PositionScale_X = 0.5f;
			descriptionBox.SizeOffset_X = 260;
			descriptionBox.SizeOffset_Y = 160;
			descriptionBox.TextAlignment = TextAnchor.UpperCenter;
			descriptionBox.AllowRichText = true;
			descriptionBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			container.AddChild(descriptionBox);

			mapNameField = Glazier.Get().CreateStringField();
			mapNameField.PositionOffset_X = -305;
			mapNameField.PositionOffset_Y = 370;
			mapNameField.PositionScale_X = 0.5f;
			mapNameField.SizeOffset_X = 200;
			mapNameField.SizeOffset_Y = 30;
			mapNameField.MaxLength = 24;
			mapNameField.AddLabel(localization.format("Name_Field_Label"), ESleekSide.LEFT);
			container.AddChild(mapNameField);

			mapSizeState = new SleekButtonState(new GUIContent(MenuPlaySingleplayerUI.localization.format("Small")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Medium")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Large")));//, new GUIContent(MenuPlaySingleplayerUI.localization.format("Insane")));
			mapSizeState.PositionOffset_X = -305;
			mapSizeState.PositionOffset_Y = 410;
			mapSizeState.PositionScale_X = 0.5f;
			mapSizeState.SizeOffset_X = 200;
			mapSizeState.SizeOffset_Y = 30;
			container.AddChild(mapSizeState);

			mapTypeState = new SleekButtonState(new GUIContent(MenuPlaySingleplayerUI.localization.format("Survival")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Arena")));//, new GUIContent(MenuPlaySingleplayerUI.localization.format("Insane")));
			mapTypeState.PositionOffset_X = -305;
			mapTypeState.PositionOffset_Y = 450;
			mapTypeState.PositionScale_X = 0.5f;
			mapTypeState.SizeOffset_X = 200;
			mapTypeState.SizeOffset_Y = 30;
			container.AddChild(mapTypeState);

			addButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
			addButton.PositionOffset_X = -305;
			addButton.PositionOffset_Y = 490;
			addButton.PositionScale_X = 0.5f;
			addButton.SizeOffset_X = 200;
			addButton.SizeOffset_Y = 30;
			addButton.text = localization.format("Add_Button");
			addButton.tooltip = localization.format("Add_Button_Tooltip");
			addButton.onClickedButton += onClickedAddButton;
			container.AddChild(addButton);

			removeButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Remove"), localization.format("Remove_Button_Confirm"), localization.format("Remove_Button_Confirm_Tooltip"), localization.format("Remove_Button_Deny"), localization.format("Remove_Button_Deny_Tooltip"));
			removeButton.PositionOffset_X = -305;
			removeButton.PositionOffset_Y = 530;
			removeButton.PositionScale_X = 0.5f;
			removeButton.SizeOffset_X = 200;
			removeButton.SizeOffset_Y = 30;
			removeButton.text = localization.format("Remove_Button");
			removeButton.tooltip = localization.format("Remove_Button_Tooltip");
			removeButton.onConfirmed = onClickedRemoveButton;
			container.AddChild(removeButton);

			if (ReadWrite.SupportsOpeningFileBrowser)
			{
				ISleekButton browseFilesButton = Glazier.Get().CreateButton();
				browseFilesButton.PositionOffset_X = -305;
				browseFilesButton.PositionOffset_Y = 330;
				browseFilesButton.PositionScale_X = 0.5f;
				browseFilesButton.SizeOffset_X = 200;
				browseFilesButton.SizeOffset_Y = 30;
				browseFilesButton.Text = localization.format("BrowseFiles_Label");
				browseFilesButton.OnClicked += OnClickedBrowseFilesButton;
				container.AddChild(browseFilesButton);
			}

			editButton = new SleekButtonIcon(icons.load<Texture2D>("Edit"));
			editButton.PositionOffset_X = -305;
			editButton.PositionOffset_Y = 290;
			editButton.PositionScale_X = 0.5f;
			editButton.SizeOffset_X = 200;
			editButton.SizeOffset_Y = 30;
			editButton.text = localization.format("Edit_Button");
			editButton.tooltip = localization.format("Edit_Button_Tooltip");
			editButton.iconColor = ESleekTint.FOREGROUND;
			editButton.onClickedButton += onClickedEditButton;
			container.AddChild(editButton);

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

			onLevelsRefreshed();
			Level.onLevelsRefreshed += onLevelsRefreshed;
		}
	}
}
