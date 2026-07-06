////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuWorkshopUI
	{
		private static SleekFullscreenBox container;
		private static Local localization;
		public static bool active;

		private static SleekButtonIcon browseButton;
		private static SleekButtonIcon submitButton;
		private static SleekButtonIcon editorButton;
		private static SleekButtonIcon errorButton;
		private static SleekButtonIcon localizationButton;
		private static SleekButtonIcon spawnsButton;
		private static SleekButtonIcon subscriptionsButton;
		private static SleekButtonIcon docsButton;
		private static SleekButtonIcon backButton;
#if UNITY_EDITOR
		private static ISleekButton tutorialButton;
#endif

		private static ISleekElement iconToolsContainer;
		private static ISleekUInt16Field itemIDField;
		private static ISleekUInt16Field vehicleIDField;
		private static ISleekUInt16Field skinIDField;
		private static ISleekField guidField;
		private static ISleekButton captureItemIconButton;
		private static ISleekButton captureAllItemIconsButton;
		private static ISleekButton captureAllSkinIconsButton;
		private static ISleekButton captureItemDefIconButton;
		private static ISleekButton captureOutfitPreviewButton;
		private static ISleekButton captureCosmeticPreviewsButton;
		private static ISleekButton captureAllOutfitPreviewsButton;
#if UNITY_EDITOR
		private static ISleekButton logAllTextButton;
#endif // UNITY_EDITOR

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			//			if(MenuWorkshopSubmitUI.active)
			//			{
			//				MenuWorkshopSubmitUI.active = false;
			//				
			//				MenuWorkshopSubmitUI.open();
			//			}
			//			else if(MenuWorkshopEditorUI.active)
			//			{
			//				MenuWorkshopEditorUI.active = false;
			//				
			//				MenuWorkshopEditorUI.open();
			//			}

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			//			if(MenuWorkshopSubmitUI.active)
			//			{
			//				MenuWorkshopSubmitUI.close();
			//				
			//				MenuWorkshopSubmitUI.active = true;
			//			}
			//			else if(MenuWorkshopEditorUI.active)
			//			{
			//				MenuWorkshopEditorUI.close();
			//				
			//				MenuWorkshopEditorUI.active = true;
			//			}

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedBrowseButton(ISleekElement button)
		{
			if (!Provider.provider.browserService.canOpenBrowser)
			{
				MenuUI.alert(localization.format("Overlay"));

				return;
			}

			Provider.provider.browserService.open("https://steamcommunity.com/app/304930/workshop/");
		}

		private static void onClickedSubmitButton(ISleekElement button)
		{
			//			MenuWorkshopEditorUI.close();

			//			if(MenuWorkshopSubmitUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			MenuWorkshopSubmitUI.open();
			close();
			//			}
		}

		private static void onClickedEditorButton(ISleekElement button)
		{
			//			MenuWorkshopSubmitUI.close();
			//
			//			if(MenuWorkshopEditorUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			MenuWorkshopEditorUI.open();
			close();
			//			}
		}

		private static void onClickedErrorButton(ISleekElement button)
		{
			//			MenuWorkshopSubmitUI.close();
			//
			//			if(MenuWorkshopEditorUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			MenuWorkshopErrorUI.open();
			close();
			//			}
		}

		private static void onClickedLocalizationButton(ISleekElement button)
		{
			//			MenuWorkshopSubmitUI.close();
			//
			//			if(MenuWorkshopEditorUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			MenuWorkshopLocalizationUI.open();
			close();
			//			}
		}

		private static void onClickedSpawnsButton(ISleekElement button)
		{
			MenuWorkshopSpawnsUI.open();
			close();
		}

		private static void onClickedSubscriptionsButton(ISleekElement button)
		{
			MenuWorkshopSubscriptionsUI.instance.open();
			close();
		}

		private static void onClickedDocsButton(ISleekElement button)
		{
			Provider.provider.browserService.open("https://docs.smartlydressedgames.com");
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuDashboardUI.open();
			MenuTitleUI.open();
			close();
		}

#if UNITY_EDITOR
		private static void onClickedTutorialButton(ISleekElement button)
		{
			Level.edit(Level.getLevel("Tutorial"));
		}
#endif

		private static void onClickedCaptureItemIconButton(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();
			ItemAsset asset = Assets.find(EAssetType.ITEM, itemIDField.Value) as ItemAsset;
			IconUtils.captureItemIcon(asset);
		}

		private static void onClickedCaptureAllItemIconsButton(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();
			IconUtils.captureAllItemIcons();
		}

		private static void onClickedCaptureAllSkinIconsButton(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();
			IconUtils.CaptureAllSkinIcons();
		}

		private static void onClickedCaptureItemDefIconButton(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();

			if (System.Guid.TryParse(guidField.Text, out System.Guid assetGuid))
			{
				Asset asset = Assets.find(assetGuid);
				ItemAsset itemAsset = asset as ItemAsset;
				VehicleAsset vehicleAsset = asset as VehicleAsset;
				if (itemAsset != null || vehicleAsset != null)
				{
					IconUtils.getItemDefIcon(itemAsset, vehicleAsset, skinIDField.Value);
					return;
				}
			}

			IconUtils.getItemDefIcon(itemIDField.Value, vehicleIDField.Value, skinIDField.Value);
		}

		private static void OnCaptureOutfitPreviewClicked(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();
			IconUtils.CaptureOutfitPreview(new System.Guid(guidField.Text));
		}

		private static void OnCaptureCosmeticPreviewsClicked(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();
			IconUtils.CaptureCosmeticPreviews();
		}

		private static void OnCaptureAllOutfitPreviewsClicked(ISleekElement button)
		{
			IconUtils.CreateExtrasDirectory();
			IconUtils.CaptureAllOutfitPreviews();
		}

		private static void OnExportAssetIdListClicked(ISleekElement button)
		{
			AssetIdListExporter.Export();
		}

		private static void OnExportCargoClicked(ISleekElement button)
		{
			CargoExporter.Export();
		}

#if UNITY_EDITOR
		private static void OnExportGunStatsClicked(ISleekElement button)
		{
			GunStatsExporter.Export();
		}
#endif // UNITY_EDITOR

#if UNITY_EDITOR
		private static void OnLogAllTextClicked(ISleekElement button)
		{
			TextDebug.LogAllText();
		}
#endif // UNITY_EDITOR

		public static void toggleIconTools()
		{
			iconToolsContainer.IsVisible = !iconToolsContainer.IsVisible;
		}

		public void OnDestroy()
		{
			editorUI.OnDestroy();
			submitUI.OnDestroy();
		}

		public MenuWorkshopUI()
		{
			localization = Localization.read("/Menu/Workshop/MenuWorkshop.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Workshop/MenuWorkshop");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = -1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			// Browse	Manage
			// Submit	Editor
			// Errors	Docs
			// Spawns	Loc

			browseButton = new SleekButtonIcon(icons.load<Texture2D>("Browse"));
			browseButton.PositionOffset_X = -205;
			browseButton.PositionOffset_Y = -115;
			browseButton.PositionScale_X = 0.5f;
			browseButton.PositionScale_Y = 0.5f;
			browseButton.SizeOffset_X = 200;
			browseButton.SizeOffset_Y = 50;
			browseButton.text = localization.format("BrowseButtonText");
			browseButton.tooltip = localization.format("BrowseButtonTooltip");
			browseButton.onClickedButton += onClickedBrowseButton;
			browseButton.fontSize = ESleekFontSize.Medium;
			browseButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(browseButton);

			submitButton = new SleekButtonIcon(icons.load<Texture2D>("Submit"));
			submitButton.PositionOffset_X = -205;
			submitButton.PositionOffset_Y = -55;
			submitButton.PositionScale_X = 0.5f;
			submitButton.PositionScale_Y = 0.5f;
			submitButton.SizeOffset_X = 200;
			submitButton.SizeOffset_Y = 50;
			submitButton.text = localization.format("SubmitButtonText");
			submitButton.tooltip = localization.format("SubmitButtonTooltip");
			submitButton.onClickedButton += onClickedSubmitButton;
			submitButton.fontSize = ESleekFontSize.Medium;
			submitButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(submitButton);

			editorButton = new SleekButtonIcon(icons.load<Texture2D>("Editor"));
			editorButton.PositionOffset_X = 5;
			editorButton.PositionOffset_Y = -55;
			editorButton.PositionScale_X = 0.5f;
			editorButton.PositionScale_Y = 0.5f;
			editorButton.SizeOffset_X = 200;
			editorButton.SizeOffset_Y = 50;
			editorButton.text = localization.format("EditorButtonText");
			editorButton.tooltip = localization.format("EditorButtonTooltip");
			editorButton.onClickedButton += onClickedEditorButton;
			editorButton.fontSize = ESleekFontSize.Medium;
			editorButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(editorButton);

			errorButton = new SleekButtonIcon(icons.load<Texture2D>("Error"));
			errorButton.PositionOffset_X = -205;
			errorButton.PositionOffset_Y = 5;
			errorButton.PositionScale_X = 0.5f;
			errorButton.PositionScale_Y = 0.5f;
			errorButton.SizeOffset_X = 200;
			errorButton.SizeOffset_Y = 50;
			errorButton.text = localization.format("ErrorButtonText");
			errorButton.tooltip = localization.format("ErrorButtonTooltip");
			errorButton.onClickedButton += onClickedErrorButton;
			errorButton.fontSize = ESleekFontSize.Medium;
			errorButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(errorButton);

			localizationButton = new SleekButtonIcon(icons.load<Texture2D>("Localization"));
			localizationButton.PositionOffset_X = 5;
			localizationButton.PositionOffset_Y = 65;
			localizationButton.PositionScale_X = 0.5f;
			localizationButton.PositionScale_Y = 0.5f;
			localizationButton.SizeOffset_X = 200;
			localizationButton.SizeOffset_Y = 50;
			localizationButton.text = localization.format("LocalizationButtonText");
			localizationButton.tooltip = localization.format("LocalizationButtonTooltip");
			localizationButton.onClickedButton += onClickedLocalizationButton;
			localizationButton.fontSize = ESleekFontSize.Medium;
			localizationButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(localizationButton);

			spawnsButton = new SleekButtonIcon(icons.load<Texture2D>("Spawns"));
			spawnsButton.PositionOffset_X = -205;
			spawnsButton.PositionOffset_Y = 65;
			spawnsButton.PositionScale_X = 0.5f;
			spawnsButton.PositionScale_Y = 0.5f;
			spawnsButton.SizeOffset_X = 200;
			spawnsButton.SizeOffset_Y = 50;
			spawnsButton.text = localization.format("SpawnsButtonText");
			spawnsButton.tooltip = localization.format("SpawnsButtonTooltip");
			spawnsButton.onClickedButton += onClickedSpawnsButton;
			spawnsButton.fontSize = ESleekFontSize.Medium;
			spawnsButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(spawnsButton);

			subscriptionsButton = new SleekButtonIcon(icons.load<Texture2D>("Subscriptions"));
			subscriptionsButton.PositionOffset_X = 5;
			subscriptionsButton.PositionOffset_Y = -115;
			subscriptionsButton.PositionScale_X = 0.5f;
			subscriptionsButton.PositionScale_Y = 0.5f;
			subscriptionsButton.SizeOffset_X = 200;
			subscriptionsButton.SizeOffset_Y = 50;
			subscriptionsButton.text = localization.format("SubscriptionsButtonText");
			subscriptionsButton.tooltip = localization.format("SubscriptionsButtonTooltip");
			subscriptionsButton.onClickedButton += onClickedSubscriptionsButton;
			subscriptionsButton.fontSize = ESleekFontSize.Medium;
			subscriptionsButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(subscriptionsButton);

			docsButton = new SleekButtonIcon(icons.load<Texture2D>("Docs"));
			docsButton.PositionOffset_X = 5;
			docsButton.PositionOffset_Y = 5;
			docsButton.PositionScale_X = 0.5f;
			docsButton.PositionScale_Y = 0.5f;
			docsButton.SizeOffset_X = 200;
			docsButton.SizeOffset_Y = 50;
			docsButton.text = localization.format("DocsButtonText");
			docsButton.tooltip = localization.format("DocsButtonTooltip");
			docsButton.onClickedButton += onClickedDocsButton;
			docsButton.fontSize = ESleekFontSize.Medium;
			docsButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(docsButton);

			//SleekNew newLabel = new SleekNew();
			//spawnsButton.add(newLabel);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_X = -100;
			backButton.PositionOffset_Y = 125;
			backButton.PositionScale_X = 0.5f;
			backButton.PositionScale_Y = 0.5f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);

#if UNITY_EDITOR
			tutorialButton = Glazier.Get().CreateButton();
			tutorialButton.PositionOffset_X = -100;
			tutorialButton.PositionOffset_Y = 245;
			tutorialButton.PositionScale_X = 0.5f;
			tutorialButton.PositionScale_Y = 0.5f;
			tutorialButton.SizeOffset_X = 200;
			tutorialButton.SizeOffset_Y = 50;
			tutorialButton.Text = "EDIT TUTORIAL";
			tutorialButton.TooltipText = "EDIT TUTORIAL";
			tutorialButton.OnClicked += onClickedTutorialButton;
			tutorialButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(tutorialButton);
#endif

			iconToolsContainer = Glazier.Get().CreateFrame();
			iconToolsContainer.PositionOffset_X = 40;
			iconToolsContainer.PositionOffset_Y = 40;
			iconToolsContainer.SizeOffset_X = -80;
			iconToolsContainer.SizeOffset_Y = -80;
			iconToolsContainer.SizeScale_X = 1;
			iconToolsContainer.SizeScale_Y = 1;
			container.AddChild(iconToolsContainer);
			iconToolsContainer.IsVisible = false;

			int debugVerticalOffset = 0;

			itemIDField = Glazier.Get().CreateUInt16Field();
			itemIDField.PositionOffset_Y = debugVerticalOffset;
			itemIDField.SizeOffset_X = 150;
			itemIDField.SizeOffset_Y = 25;
			itemIDField.AddLabel("Item ID", ESleekSide.RIGHT);
			iconToolsContainer.AddChild(itemIDField);
			debugVerticalOffset += 25;

			vehicleIDField = Glazier.Get().CreateUInt16Field();
			vehicleIDField.PositionOffset_Y = debugVerticalOffset;
			vehicleIDField.SizeOffset_X = 150;
			vehicleIDField.SizeOffset_Y = 25;
			vehicleIDField.AddLabel("Vehicle ID", ESleekSide.RIGHT);
			iconToolsContainer.AddChild(vehicleIDField);
			debugVerticalOffset += 25;

			skinIDField = Glazier.Get().CreateUInt16Field();
			skinIDField.PositionOffset_Y = debugVerticalOffset;
			skinIDField.SizeOffset_X = 150;
			skinIDField.SizeOffset_Y = 25;
			skinIDField.AddLabel("Skin ID", ESleekSide.RIGHT);
			iconToolsContainer.AddChild(skinIDField);
			debugVerticalOffset += 25;

			captureItemIconButton = Glazier.Get().CreateButton();
			captureItemIconButton.PositionOffset_Y = debugVerticalOffset;
			captureItemIconButton.SizeOffset_X = 150;
			captureItemIconButton.SizeOffset_Y = 25;
			captureItemIconButton.Text = "Item Icon";
			captureItemIconButton.OnClicked += onClickedCaptureItemIconButton;
			iconToolsContainer.AddChild(captureItemIconButton);
			debugVerticalOffset += 25;

			captureAllItemIconsButton = Glazier.Get().CreateButton();
			captureAllItemIconsButton.PositionOffset_Y = debugVerticalOffset;
			captureAllItemIconsButton.SizeOffset_X = 150;
			captureAllItemIconsButton.SizeOffset_Y = 25;
			captureAllItemIconsButton.Text = "All Item Icons";
			captureAllItemIconsButton.OnClicked += onClickedCaptureAllItemIconsButton;
			iconToolsContainer.AddChild(captureAllItemIconsButton);
			debugVerticalOffset += 25;

			captureAllSkinIconsButton = Glazier.Get().CreateButton();
			captureAllSkinIconsButton.PositionOffset_Y = debugVerticalOffset;
			captureAllSkinIconsButton.SizeOffset_X = 150;
			captureAllSkinIconsButton.SizeOffset_Y = 25;
			captureAllSkinIconsButton.Text = "All Skin Icons";
			captureAllSkinIconsButton.OnClicked += onClickedCaptureAllSkinIconsButton;
			iconToolsContainer.AddChild(captureAllSkinIconsButton);
			debugVerticalOffset += 25;

			captureItemDefIconButton = Glazier.Get().CreateButton();
			captureItemDefIconButton.PositionOffset_Y = debugVerticalOffset;
			captureItemDefIconButton.SizeOffset_X = 150;
			captureItemDefIconButton.SizeOffset_Y = 25;
			captureItemDefIconButton.Text = "Econ Icon";
			captureItemDefIconButton.OnClicked += onClickedCaptureItemDefIconButton;
			iconToolsContainer.AddChild(captureItemDefIconButton);
			debugVerticalOffset += 25;

			guidField = Glazier.Get().CreateStringField();
			guidField.PositionOffset_Y = debugVerticalOffset;
			guidField.SizeOffset_X = 150;
			guidField.SizeOffset_Y = 25;
			guidField.AddLabel("GUID", ESleekSide.RIGHT);
			iconToolsContainer.AddChild(guidField);
			debugVerticalOffset += 25;

			captureOutfitPreviewButton = Glazier.Get().CreateButton();
			captureOutfitPreviewButton.PositionOffset_Y = debugVerticalOffset;
			captureOutfitPreviewButton.SizeOffset_X = 150;
			captureOutfitPreviewButton.SizeOffset_Y = 25;
			captureOutfitPreviewButton.Text = "Outfit Preview";
			captureOutfitPreviewButton.OnClicked += OnCaptureOutfitPreviewClicked;
			iconToolsContainer.AddChild(captureOutfitPreviewButton);
			debugVerticalOffset += 25;

			captureCosmeticPreviewsButton = Glazier.Get().CreateButton();
			captureCosmeticPreviewsButton.PositionOffset_Y = debugVerticalOffset;
			captureCosmeticPreviewsButton.SizeOffset_X = 150;
			captureCosmeticPreviewsButton.SizeOffset_Y = 25;
			captureCosmeticPreviewsButton.Text = "All Cosmetic Previews";
			captureCosmeticPreviewsButton.OnClicked += OnCaptureCosmeticPreviewsClicked;
			iconToolsContainer.AddChild(captureCosmeticPreviewsButton);
			debugVerticalOffset += 25;

			captureAllOutfitPreviewsButton = Glazier.Get().CreateButton();
			captureAllOutfitPreviewsButton.PositionOffset_Y = debugVerticalOffset;
			captureAllOutfitPreviewsButton.SizeOffset_X = 150;
			captureAllOutfitPreviewsButton.SizeOffset_Y = 25;
			captureAllOutfitPreviewsButton.Text = "All Outfit Previews";
			captureAllOutfitPreviewsButton.OnClicked += OnCaptureAllOutfitPreviewsClicked;
			iconToolsContainer.AddChild(captureAllOutfitPreviewsButton);
			debugVerticalOffset += 25;

#if UNITY_EDITOR
			logAllTextButton = Glazier.Get().CreateButton();
			logAllTextButton.PositionOffset_Y = debugVerticalOffset;
			logAllTextButton.SizeOffset_X = 150;
			logAllTextButton.SizeOffset_Y = 25;
			logAllTextButton.Text = "Log Text";
			logAllTextButton.OnClicked += OnLogAllTextClicked;
			iconToolsContainer.AddChild(logAllTextButton);
			debugVerticalOffset += 25;
#endif // UNITY_EDITOR

			ISleekButton exportAssetIdListButton = Glazier.Get().CreateButton();
			exportAssetIdListButton.PositionOffset_Y = debugVerticalOffset;
			exportAssetIdListButton.SizeOffset_X = 150;
			exportAssetIdListButton.SizeOffset_Y = 25;
			exportAssetIdListButton.Text = "Export Asset IDs";
			exportAssetIdListButton.OnClicked += OnExportAssetIdListClicked;
			iconToolsContainer.AddChild(exportAssetIdListButton);
			debugVerticalOffset += 25;

			ISleekButton exportCargoButton = Glazier.Get().CreateButton();
			exportCargoButton.PositionOffset_Y = debugVerticalOffset;
			exportCargoButton.SizeOffset_X = 150;
			exportCargoButton.SizeOffset_Y = 25;
			exportCargoButton.Text = "Export Wiki Cargo Data";
			exportCargoButton.OnClicked += OnExportCargoClicked;
			iconToolsContainer.AddChild(exportCargoButton);
			debugVerticalOffset += 25;

#if UNITY_EDITOR
			ISleekButton exportGunStatsButton = Glazier.Get().CreateButton();
			exportGunStatsButton.PositionOffset_Y = debugVerticalOffset;
			exportGunStatsButton.SizeOffset_X = 150;
			exportGunStatsButton.SizeOffset_Y = 25;
			exportGunStatsButton.Text = "Export Gun Stats";
			exportGunStatsButton.OnClicked += OnExportGunStatsClicked;
			iconToolsContainer.AddChild(exportGunStatsButton);
			debugVerticalOffset += 25;
#endif // UNITY_EDITOR

			submitUI = new MenuWorkshopSubmitUI();
			editorUI = new MenuWorkshopEditorUI();
			errorUI = new MenuWorkshopErrorUI();
			localizationUI = new MenuWorkshopLocalizationUI();
			spawnsUI = new MenuWorkshopSpawnsUI();
			subscriptionsUI = new MenuWorkshopSubscriptionsUI();
		}

		private MenuWorkshopSubmitUI submitUI;
		private MenuWorkshopEditorUI editorUI;
		private MenuWorkshopErrorUI errorUI;
		private MenuWorkshopLocalizationUI localizationUI;
		private MenuWorkshopSpawnsUI spawnsUI;
		private MenuWorkshopSubscriptionsUI subscriptionsUI;
	}
}
