////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2024-12-16: This menu and ESteamUGCType are far from ideal, but I'm just trying to hack in a new tag for
	/// server browser curation assets before the update. :P
	/// </summary>
	public enum EWorkshopMenuSubmissionMode
	{
		Map,
		Localization,
		Object,
		Item,
		Vehicle,
		Skin,
		ServerCuration,
	}

	/// <summary>
	/// Nelson 2025-02-20: Hacking this in to address duplicate buttons when onPublishedAdded is called for a second
	/// page of published files. (public issue #4882)
	/// </summary>
	internal struct PublishedFileUpdateButton
	{
		public SteamPublished publishedFile;
		public ISleekButton button;
	}

	public class MenuWorkshopSubmitUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static ISleekField nameField;
		private static ISleekField descriptionField;
		private static ISleekField pathField;
		private static ISleekBox pathNotification;
		private static ISleekField previewField;
		private static ISleekBox previewNotification;
		private static ISleekField changeField;
		private static ISleekField allowedIPsField;

		private static SleekButtonState typeState;
		private static SleekButtonState mapTypeState;
		private static SleekButtonState itemTypeState;
		private static SleekButtonState vehicleTypeState;
		private static SleekButtonState skinTypeState;
		private static SleekButtonState objectTypeState;
		private static SleekButtonState visibilityState;
		private static SleekButtonState forState;

		private static SleekButtonIcon createButton;
		private static ISleekButton legalButton;

		private static ISleekScrollView publishedBox;
		private static List<PublishedFileUpdateButton> publishedButtons;

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

		private static string ExtraTag
		{
			get
			{
				EWorkshopMenuSubmissionMode submissionMode = (EWorkshopMenuSubmissionMode) typeState.state;
				switch (submissionMode)
				{
					case EWorkshopMenuSubmissionMode.Map:
						return localization.FormatEnglishOrEmpty(mapTypeKeys[mapTypeState.state]);

					case EWorkshopMenuSubmissionMode.Item:
						return localization.FormatEnglishOrEmpty(itemTypeKeys[itemTypeState.state]);

					case EWorkshopMenuSubmissionMode.Vehicle:
						return localization.FormatEnglishOrEmpty(vehicleTypeKeys[vehicleTypeState.state]);

					case EWorkshopMenuSubmissionMode.Skin:
						return localization.FormatEnglishOrEmpty(skinTypeKeys[skinTypeState.state]);

					case EWorkshopMenuSubmissionMode.Object:
						return localization.FormatEnglishOrEmpty(objectTypeKeys[objectTypeState.state]);

					default:
						return "";
				}
			}
		}

		private static void refreshPathFieldNotification()
		{
			string enteredText = pathField.Text;
			string warning = null;

			if (string.IsNullOrEmpty(enteredText))
			{
				warning = localization.format("PathFieldNotification_Empty");
			}
			else if (!ReadWrite.folderExists(enteredText, false))
			{
				warning = localization.format("PathFieldNotification_MissingFolder");
			}
			else if (!ReadWrite.hasDirectoryWritePermission(enteredText))
			{
				warning = localization.format("PathFieldNotification_NoWritePermission");
			}
			else
			{
				ESteamUGCType type = ConvertSubmissionTypeToUgcType();
				if (type == ESteamUGCType.MAP)
				{
					if (!WorkshopTool.checkMapValid(enteredText, false))
					{
						warning = localization.format("PathFieldNotification_Map");
					}
				}
				else if (type == ESteamUGCType.LOCALIZATION)
				{
					if (!WorkshopTool.checkLocalizationValid(enteredText, false))
					{
						warning = localization.format("PathFieldNotification_Localization");
					}
				}
				else
				{
					if (!WorkshopTool.checkBundleValid(enteredText, false))
					{
						warning = localization.format("PathFieldNotification_Bundle");
					}
				}
			}

			pathNotification.IsVisible = !string.IsNullOrEmpty(warning);
			pathNotification.TooltipText = warning;
		}

		private static void onPathFieldTyped(ISleekField field, string text)
		{
			refreshPathFieldNotification();
		}

		private static void refreshPreviewFieldNotification()
		{
			string enteredText = previewField.Text;
			string warning = null;

			if (string.IsNullOrEmpty(enteredText))
			{
				warning = localization.format("PreviewFieldNotification_Empty");
			}
			else if (!ReadWrite.fileExists(enteredText, false, false))
			{
				warning = localization.format("PreviewFieldNotification_MissingFile");
			}
			else
			{
				if (enteredText.EndsWith(".png", System.StringComparison.InvariantCultureIgnoreCase) || enteredText.EndsWith(".jpg", System.StringComparison.InvariantCultureIgnoreCase))
				{
					FileInfo fileInfo = new System.IO.FileInfo(enteredText);
					long fileSize = fileInfo.Length;
					const long maxFileSize = 1000000;
					if (fileSize > maxFileSize)
					{
						if (localization.has("PreviewFieldNotification_FileSize_V2"))
						{
							string fileSizeString = ByteDisplay.FileSizeToString(fileSize);
							string maxSizeString = ByteDisplay.FileSizeToString(maxFileSize);
							warning = localization.format("PreviewFieldNotification_FileSize_V2",
								fileSizeString, maxSizeString);
						}
						else
						{
							warning = localization.format("PreviewFieldNotification_FileSize");
						}
					}
				}
				else
				{
					warning = localization.format("PreviewFieldNotification_Extension");
				}
			}

			previewNotification.IsVisible = !string.IsNullOrEmpty(warning);
			previewNotification.TooltipText = warning;
		}

		private static void onPreviewFieldTyped(ISleekField field, string text)
		{
			refreshPreviewFieldNotification();
		}

		private static void onClickedCreateButton(ISleekElement button)
		{
			if (checkEntered() && checkValid())
			{
				Provider.provider.workshopService.prepareUGC(nameField.Text, descriptionField.Text, pathField.Text, previewField.Text, changeField.Text, ConvertSubmissionTypeToUgcType(), GetSubmissionTags(), allowedIPsField.Text, (ESteamUGCVisibility) visibilityState.state);
				Provider.provider.workshopService.createUGC(forState.state == 1);

				resetFields();
			}
		}

		private static void onClickedLegalButton(ISleekElement button)
		{
			if (!Provider.provider.browserService.canOpenBrowser)
			{
				MenuUI.alert(localization.format("Overlay"));

				return;
			}

			Provider.provider.browserService.open("https://steamcommunity.com/sharedfiles/workshoplegalagreement/?appid=304930");
		}

		private static void onClickedPublished(ISleekElement button)
		{
			int index = Mathf.FloorToInt(button.PositionOffset_Y / 40);
			
			if (checkValid())
			{
				Provider.provider.workshopService.prepareUGC(nameField.Text, descriptionField.Text, pathField.Text, previewField.Text, changeField.Text, ConvertSubmissionTypeToUgcType(), GetSubmissionTags(), allowedIPsField.Text, (ESteamUGCVisibility) visibilityState.state);
				Provider.provider.workshopService.prepareUGC(Provider.provider.workshopService.published[index].id);
				Provider.provider.workshopService.updateUGC();

				resetFields();
			}
		}

		private static void onPublishedAdded()
		{
			for (int index = 0; index < Provider.provider.workshopService.published.Count; index++)
			{
				SteamPublished published = Provider.provider.workshopService.published[index];

				bool alreadyHasButton = false;
				foreach (PublishedFileUpdateButton existingButton in publishedButtons)
				{
					if (existingButton.publishedFile == published)
					{
						alreadyHasButton = true;
						break;
					}
				}

				if (alreadyHasButton)
					continue;

				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_Y = index * 40;
				button.SizeOffset_Y = 30;
				button.SizeScale_X = 1;
				button.Text = published.name;
				button.OnClicked += onClickedPublished;

				publishedBox.AddChild(button);

				publishedButtons.Add(new PublishedFileUpdateButton()
				{
					publishedFile = published,
					button = button,
				});
			}

			publishedBox.ContentSizeOffset = new Vector2(0.0f, (publishedButtons.Count * 40) - 10);
		}

		private static void onPublishedRemoved()
		{
			publishedBox.RemoveAllChildren();
			publishedButtons.Clear();
		}

		private static bool checkEntered()
		{
			if (nameField.Text.Length == 0)
			{
				MenuUI.alert(localization.format("Alert_Name"));

				return false;
			}

			if (previewField.Text.Length == 0 || !ReadWrite.fileExists(previewField.Text, false, false) || new System.IO.FileInfo(previewField.Text).Length > 1000000)
			{
				MenuUI.alert(localization.format("Alert_Preview"));

				return false;
			}

			return true;
		}

		private static bool checkValid()
		{
			if (pathField.Text.Length == 0 || !ReadWrite.folderExists(pathField.Text, false))
			{
				MenuUI.alert(localization.format("Alert_Path"));

				return false;
			}

			ESteamUGCType type = ConvertSubmissionTypeToUgcType();
			bool isFor = forState.state == 1;

			if (isFor) // curated
			{
				if (type != ESteamUGCType.ITEM && type != ESteamUGCType.SKIN) // no non-items/skins on curated
				{
					MenuUI.alert(localization.format("Alert_Curated"));

					return false;
				}
			}
			else // ready to use
			{
				if (type == ESteamUGCType.SKIN) // no skins on ready-to-use
				{
					MenuUI.alert(localization.format("Alert_Curated"));

					return false;
				}
			}

			bool isValid = false;

			if (type == ESteamUGCType.MAP)
			{
				isValid = WorkshopTool.checkMapValid(pathField.Text, false);

				if (!isValid)
				{
					MenuUI.alert(localization.format("Alert_Map"));
				}
			}
			else if (type == ESteamUGCType.LOCALIZATION)
			{
				isValid = WorkshopTool.checkLocalizationValid(pathField.Text, false);

				if (!isValid)
				{
					MenuUI.alert(localization.format("Alert_Localization"));
				}
			}
			else if (type == ESteamUGCType.OBJECT || type == ESteamUGCType.ITEM || type == ESteamUGCType.VEHICLE || type == ESteamUGCType.SKIN)
			{
				isValid = WorkshopTool.checkBundleValid(pathField.Text, false);

				if (!isValid)
				{
					MenuUI.alert(localization.format("Alert_Object"));
				}
			}

			return isValid;
		}

		private static void resetFields()
		{
			nameField.Text = "";
			descriptionField.Text = "";
			pathField.Text = "";
			previewField.Text = "";
			changeField.Text = "";
			allowedIPsField.Text = "";

			refreshPathFieldNotification();
			refreshPreviewFieldNotification();
		}

		private static void onSwappedTypeState(SleekButtonState button, int state)
		{
			EWorkshopMenuSubmissionMode submissionMode = (EWorkshopMenuSubmissionMode) typeState.state;

			mapTypeState.IsVisible = submissionMode == EWorkshopMenuSubmissionMode.Map;
			itemTypeState.IsVisible = submissionMode == EWorkshopMenuSubmissionMode.Item;
			vehicleTypeState.IsVisible = submissionMode == EWorkshopMenuSubmissionMode.Vehicle;
			skinTypeState.IsVisible = submissionMode == EWorkshopMenuSubmissionMode.Skin;
			objectTypeState.IsVisible = submissionMode == EWorkshopMenuSubmissionMode.Object;

			refreshPathFieldNotification();
		}

		private static ESteamUGCType ConvertSubmissionTypeToUgcType()
		{
			EWorkshopMenuSubmissionMode mode = (EWorkshopMenuSubmissionMode) typeState.state;
			switch (mode)
			{
				case EWorkshopMenuSubmissionMode.Map:
					return ESteamUGCType.MAP;

				case EWorkshopMenuSubmissionMode.Localization:
					return ESteamUGCType.LOCALIZATION;

				default:
				case EWorkshopMenuSubmissionMode.Object:
				case EWorkshopMenuSubmissionMode.ServerCuration:
					return ESteamUGCType.OBJECT;

				case EWorkshopMenuSubmissionMode.Item:
					return ESteamUGCType.ITEM;

				case EWorkshopMenuSubmissionMode.Vehicle:
					return ESteamUGCType.VEHICLE;

				case EWorkshopMenuSubmissionMode.Skin:
					return ESteamUGCType.SKIN;
			}
		}

		private static List<string> GetSubmissionTags()
		{
			List<string> tags = new List<string>();

			EWorkshopMenuSubmissionMode submissionMode = (EWorkshopMenuSubmissionMode) typeState.state;
			switch (submissionMode)
			{
				case EWorkshopMenuSubmissionMode.Map:
					tags.Add("Map");
					break;

				case EWorkshopMenuSubmissionMode.Localization:
					tags.Add("Localization");
					break;

				case EWorkshopMenuSubmissionMode.Object:
					tags.Add("Object");
					break;

				case EWorkshopMenuSubmissionMode.Item:
					tags.Add("Item");
					break;

				case EWorkshopMenuSubmissionMode.Vehicle:
					tags.Add("Vehicle");
					break;

				case EWorkshopMenuSubmissionMode.Skin:
					tags.Add("Skin");
					break;

				case EWorkshopMenuSubmissionMode.ServerCuration:
					tags.Add("Server Curation");
					break;
			}

			string extraTag = ExtraTag;
			if (!string.IsNullOrEmpty(extraTag))
			{
				tags.Add(extraTag);
			}

			return tags;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
		}

		public void OnDestroy()
		{
			Provider.provider.workshopService.onPublishedAdded -= onPublishedAdded;
			Provider.provider.workshopService.onPublishedRemoved -= onPublishedRemoved;
		}

		public MenuWorkshopSubmitUI()
		{
			localization = Localization.read("/Menu/Workshop/MenuWorkshopSubmit.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Workshop/MenuWorkshopSubmit");

			publishedButtons = new List<PublishedFileUpdateButton>();

			Provider.provider.workshopService.onPublishedAdded += onPublishedAdded;
			Provider.provider.workshopService.onPublishedRemoved += onPublishedRemoved;

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

			nameField = Glazier.Get().CreateStringField();
			nameField.PositionOffset_X = -200;
			nameField.PositionOffset_Y = 140;
			nameField.PositionScale_X = 0.5f;
			nameField.SizeOffset_X = 200;
			nameField.SizeOffset_Y = 30;
			nameField.MaxLength = 24;
			nameField.AddLabel(localization.format("Name_Field_Label"), ESleekSide.RIGHT);
			container.AddChild(nameField);

			// Disabled because it's not very useful. Fine to keep around for the meantime
			// because we ignore the description if it's empty.
			descriptionField = Glazier.Get().CreateStringField();
			descriptionField.PositionOffset_X = -200;
			descriptionField.PositionOffset_Y = 140;
			descriptionField.PositionScale_X = 0.5f;
			descriptionField.SizeOffset_X = 400;
			descriptionField.SizeOffset_Y = 30;
			descriptionField.MaxLength = 128;
			descriptionField.Text = "";
			descriptionField.AddLabel(localization.format("Description_Field_Label"), ESleekSide.RIGHT);
			container.AddChild(descriptionField);
			descriptionField.IsVisible = false;

			pathField = Glazier.Get().CreateStringField();
			pathField.PositionOffset_X = -200;
			pathField.PositionOffset_Y = 180;
			pathField.PositionScale_X = 0.5f;
			pathField.SizeOffset_X = 400;
			pathField.SizeOffset_Y = 30;
			pathField.MaxLength = 128;
			pathField.AddLabel(localization.format("Path_Field_Label"), ESleekSide.RIGHT);
			pathField.OnTextChanged += onPathFieldTyped;
			container.AddChild(pathField);

			// Popup with information when path is invalid.
			pathNotification = Glazier.Get().CreateBox();
			pathNotification.PositionOffset_X = -240;
			pathNotification.PositionOffset_Y = 180;
			pathNotification.PositionScale_X = 0.5f;
			pathNotification.SizeOffset_X = 30;
			pathNotification.SizeOffset_Y = 30;
			pathNotification.Text = "!";
			container.AddChild(pathNotification);
			pathNotification.IsVisible = false;

			previewField = Glazier.Get().CreateStringField();
			previewField.PositionOffset_X = -200;
			previewField.PositionOffset_Y = 220;
			previewField.PositionScale_X = 0.5f;
			previewField.SizeOffset_X = 400;
			previewField.SizeOffset_Y = 30;
			previewField.MaxLength = 128;
			previewField.AddLabel(localization.format("Preview_Field_Label"), ESleekSide.RIGHT);
			previewField.OnTextChanged += onPreviewFieldTyped;
			container.AddChild(previewField);

			// Popup with information when preview image is invalid.
			previewNotification = Glazier.Get().CreateBox();
			previewNotification.PositionOffset_X = -240;
			previewNotification.PositionOffset_Y = 220;
			previewNotification.PositionScale_X = 0.5f;
			previewNotification.SizeOffset_X = 30;
			previewNotification.SizeOffset_Y = 30;
			previewNotification.Text = "!";
			container.AddChild(previewNotification);
			previewNotification.IsVisible = false;

			changeField = Glazier.Get().CreateStringField();
			changeField.PositionOffset_X = -200;
			changeField.PositionOffset_Y = 260;
			changeField.PositionScale_X = 0.5f;
			changeField.SizeOffset_X = 400;
			changeField.SizeOffset_Y = 30;
			changeField.MaxLength = 128;
			changeField.AddLabel(localization.format("Change_Field_Label"), ESleekSide.RIGHT);
			container.AddChild(changeField);

			typeState = new SleekButtonState(new GUIContent(localization.format("Map")),
				new GUIContent(localization.format("Localization")),
				new GUIContent(localization.format("Object")),
				new GUIContent(localization.format("Item")),
				new GUIContent(localization.format("Vehicle")),
				new GUIContent(localization.format("Skin")),
				new GUIContent(localization.format("ServerCuration")));
			typeState.PositionOffset_X = -200;
			typeState.PositionOffset_Y = 300;
			typeState.PositionScale_X = 0.5f;
			typeState.SizeOffset_X = 195;
			typeState.SizeOffset_Y = 30;
			typeState.onSwappedState = onSwappedTypeState;
			container.AddChild(typeState);

			GUIContent[] mapTypeContents = new GUIContent[mapTypeKeys.Length];
			for (int index = 0; index < mapTypeContents.Length; ++index)
			{
				mapTypeContents[index] = new GUIContent(localization.format(mapTypeKeys[index]));
			}

			mapTypeState = new SleekButtonState(mapTypeContents);
			mapTypeState.PositionOffset_X = 5;
			mapTypeState.PositionOffset_Y = 300;
			mapTypeState.PositionScale_X = 0.5f;
			mapTypeState.SizeOffset_X = 195;
			mapTypeState.SizeOffset_Y = 30;
			container.AddChild(mapTypeState);
			mapTypeState.IsVisible = true;

			GUIContent[] itemTypeContents = new GUIContent[itemTypeKeys.Length];
			for (int index = 0; index < itemTypeContents.Length; ++index)
			{
				itemTypeContents[index] = new GUIContent(localization.format(itemTypeKeys[index]));
			}

			itemTypeState = new SleekButtonState(itemTypeContents);
			itemTypeState.PositionOffset_X = 5;
			itemTypeState.PositionOffset_Y = 300;
			itemTypeState.PositionScale_X = 0.5f;
			itemTypeState.SizeOffset_X = 195;
			itemTypeState.SizeOffset_Y = 30;
			container.AddChild(itemTypeState);
			itemTypeState.IsVisible = false;

			GUIContent[] vehicleTypeContents = new GUIContent[vehicleTypeKeys.Length];
			for (int index = 0; index < vehicleTypeContents.Length; ++index)
			{
				vehicleTypeContents[index] = new GUIContent(localization.format(vehicleTypeKeys[index]));
			}

			vehicleTypeState = new SleekButtonState(vehicleTypeContents);
			vehicleTypeState.PositionOffset_X = 5;
			vehicleTypeState.PositionOffset_Y = 300;
			vehicleTypeState.PositionScale_X = 0.5f;
			vehicleTypeState.SizeOffset_X = 195;
			vehicleTypeState.SizeOffset_Y = 30;
			container.AddChild(vehicleTypeState);
			vehicleTypeState.IsVisible = false;

			GUIContent[] skinTypeContents = new GUIContent[skinTypeKeys.Length];
			for (int index = 0; index < skinTypeContents.Length; ++index)
			{
				skinTypeContents[index] = new GUIContent(localization.format(skinTypeKeys[index]));
			}

			skinTypeState = new SleekButtonState(skinTypeContents);
			skinTypeState.PositionOffset_X = 5;
			skinTypeState.PositionOffset_Y = 300;
			skinTypeState.PositionScale_X = 0.5f;
			skinTypeState.SizeOffset_X = 195;
			skinTypeState.SizeOffset_Y = 30;
			container.AddChild(skinTypeState);
			skinTypeState.IsVisible = false;

			GUIContent[] objectTypeContents = new GUIContent[objectTypeKeys.Length];
			for (int index = 0; index < objectTypeContents.Length; ++index)
			{
				objectTypeContents[index] = new GUIContent(localization.format(objectTypeKeys[index]));
			}

			objectTypeState = new SleekButtonState(objectTypeContents);
			objectTypeState.PositionOffset_X = 5;
			objectTypeState.PositionOffset_Y = 300;
			objectTypeState.PositionScale_X = 0.5f;
			objectTypeState.SizeOffset_X = 195;
			objectTypeState.SizeOffset_Y = 30;
			container.AddChild(objectTypeState);
			objectTypeState.IsVisible = false;

			visibilityState = new SleekButtonState(new GUIContent(localization.format("Public")), new GUIContent(localization.format("Friends_Only")), new GUIContent(localization.format("Private")), new GUIContent(localization.format("Unlisted")));
			visibilityState.PositionOffset_X = -200;
			visibilityState.PositionOffset_Y = 340;
			visibilityState.PositionScale_X = 0.5f;
			visibilityState.SizeOffset_X = 195;
			visibilityState.SizeOffset_Y = 30;
			container.AddChild(visibilityState);

			forState = new SleekButtonState(new GUIContent(localization.format("Community")), new GUIContent(localization.format("Review")));
			forState.PositionOffset_X = 5;
			forState.PositionOffset_Y = 340;
			forState.PositionScale_X = 0.5f;
			forState.SizeOffset_X = 195;
			forState.SizeOffset_Y = 30;
			container.AddChild(forState);

			allowedIPsField = Glazier.Get().CreateStringField();
			allowedIPsField.PositionOffset_X = -200;
			allowedIPsField.PositionOffset_Y = 380;
			allowedIPsField.PositionScale_X = 0.5f;
			allowedIPsField.SizeOffset_X = 400;
			allowedIPsField.SizeOffset_Y = 30;
			allowedIPsField.MaxLength = 255;
			allowedIPsField.TooltipText = localization.format("Allowed_IPs_Tooltip");
			allowedIPsField.PlaceholderText = localization.format("Allowed_IPs_Hint");
			allowedIPsField.AddLabel(localization.format("Allowed_IPs_Label"), ESleekSide.RIGHT);
			container.AddChild(allowedIPsField);

			createButton = new SleekButtonIcon(icons.load<Texture2D>("Create"));
			createButton.PositionOffset_X = -200;
			createButton.PositionOffset_Y = 420;
			createButton.PositionScale_X = 0.5f;
			createButton.SizeOffset_X = 195;
			createButton.SizeOffset_Y = 30;
			createButton.text = localization.format("Create_Button");
			createButton.tooltip = localization.format("Create_Button_Tooltip");
			createButton.onClickedButton += onClickedCreateButton;
			createButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(createButton);

			legalButton = Glazier.Get().CreateButton();
			legalButton.PositionOffset_X = 5;
			legalButton.PositionOffset_Y = 420;
			legalButton.PositionScale_X = 0.5f;
			legalButton.SizeOffset_X = 195;
			legalButton.SizeOffset_Y = 30;
			legalButton.FontSize = ESleekFontSize.Small;
			legalButton.Text = localization.format("Legal_Button");
			legalButton.TooltipText = localization.format("Legal_Button_Tooltip");
			legalButton.OnClicked += onClickedLegalButton;
			container.AddChild(legalButton);

			publishedBox = Glazier.Get().CreateScrollView();
			publishedBox.PositionOffset_X = -200;
			publishedBox.PositionOffset_Y = 460;
			publishedBox.PositionScale_X = 0.5f;
			publishedBox.SizeOffset_X = 430;
			publishedBox.SizeOffset_Y = -460;
			publishedBox.SizeScale_Y = 1;
			publishedBox.ScaleContentToWidth = true;
			container.AddChild(publishedBox);

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

			onPublishedAdded();
		}

		private static readonly string[] mapTypeKeys = new string[]
		{
			"Map_Type_Survival",
			"Map_Type_Horde",
			"Map_Type_Arena",
			"Map_Type_Custom",
		};

		private static readonly string[] itemTypeKeys = new string[]
		{
			"Item_Type_Backpack",
			"Item_Type_Barrel",
			"Item_Type_Barricade",
			"Item_Type_Fisher",
			"Item_Type_Food",
			"Item_Type_Fuel",
			"Item_Type_Glasses",
			"Item_Type_Grip",
			"Item_Type_Grower",
			"Item_Type_Gun",
			"Item_Type_Hat",
			"Item_Type_Magazine",
			"Item_Type_Mask",
			"Item_Type_Medical",
			"Item_Type_Melee",
			"Item_Type_Optic",
			"Item_Type_Shirt",
			"Item_Type_Sight",
			"Item_Type_Structure",
			"Item_Type_Supply",
			"Item_Type_Tactical",
			"Item_Type_Throwable",
			"Item_Type_Tool",
			"Item_Type_Vest",
			"Item_Type_Water",
		};

		private static readonly string[] vehicleTypeKeys = new string[]
		{
			"Vehicle_Type_Wheels_2",
			"Vehicle_Type_Wheels_4",
			"Vehicle_Type_Plane",
			"Vehicle_Type_Helicopter",
			"Vehicle_Type_Boat",
			"Vehicle_Type_Train",
		};

		private static readonly string[] skinTypeKeys = new string[]
		{
			"Skin_Type_Generic_Pattern",
			"Skin_Type_Ace",
			"Skin_Type_Augewehr",
			"Skin_Type_Avenger",
			"Skin_Type_Bluntforce",
			"Skin_Type_Bulldog",
			"Skin_Type_Butterfly_Knife",
			"Skin_Type_Calling_Card",
			"Skin_Type_Cobra",
			"Skin_Type_Colt",
			"Skin_Type_Compound_Bow",
			"Skin_Type_Crossbow",
			"Skin_Type_Desert_Falcon",
			"Skin_Type_Dragonfang",
			"Skin_Type_Eaglefire",
			"Skin_Type_Ekho",
			"Skin_Type_Fusilaut",
			"Skin_Type_Grizzly",
			"Skin_Type_Hawkhound",
			"Skin_Type_Heartbreaker",
			"Skin_Type_Hell_Fury",
			"Skin_Type_Honeybadger",
			"Skin_Type_Katana",
			"Skin_Type_Kryzkarek",
			"Skin_Type_Machete",
			"Skin_Type_Maplestrike",
			"Skin_Type_Maschinengewehr",
			"Skin_Type_Masterkey",
			"Skin_Type_Matamorez",
			"Skin_Type_Military_Knife",
			"Skin_Type_Nightraider",
			"Skin_Type_Nykorev",
			"Skin_Type_Peacemaker",
			"Skin_Type_Rocket_Launcher",
			"Skin_Type_Sabertooth",
			"Skin_Type_Scalar",
			"Skin_Type_Schofield",
			"Skin_Type_Shadowstalker",
			"Skin_Type_Snayperskya",
			"Skin_Type_Sportshot",
			"Skin_Type_Teklowvka",
			"Skin_Type_Timberwolf",
			"Skin_Type_Viper",
			"Skin_Type_Vonya",
			"Skin_Type_Yuri",
			"Skin_Type_Zubeknakov",
		};

		private static readonly string[] objectTypeKeys = new string[]
		{
			"Object_Type_Model",
			"Object_Type_Resource",
			"Object_Type_Effect",
			"Object_Type_Animal",
		};
	}
}
