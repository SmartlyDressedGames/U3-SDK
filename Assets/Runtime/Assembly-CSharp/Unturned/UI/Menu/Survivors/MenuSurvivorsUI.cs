////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon characterButton;
		private static SleekButtonIcon appearanceButton;
		private static SleekButtonIcon groupButton;
		private static SleekButtonIcon clothingButton;
		private static SleekButtonIcon backButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			//			if(MenuSurvivorsCharacterUI.active)
			//			{
			//				MenuSurvivorsCharacterUI.active = false;
			//				
			//				MenuSurvivorsCharacterUI.open();
			//			}
			//			else if(MenuSurvivorsAppearanceUI.active)
			//			{
			//				MenuSurvivorsAppearanceUI.active = false;
			//				
			//				MenuSurvivorsAppearanceUI.open();
			//			}
			//			else if(MenuSurvivorsGroupUI.active)
			//			{
			//				MenuSurvivorsGroupUI.active = false;
			//				
			//				MenuSurvivorsGroupUI.open();
			//			}
			//			else if(MenuSurvivorsClothingUI.active)
			//			{
			//				MenuSurvivorsClothingUI.active = false;
			//				
			//				MenuSurvivorsClothingUI.open();
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

			Characters.save();

			//			if(MenuSurvivorsCharacterUI.active)
			//			{
			//				MenuSurvivorsCharacterUI.close();
			//				
			//				MenuSurvivorsCharacterUI.active = true;
			//			}
			//			else if(MenuSurvivorsAppearanceUI.active)
			//			{
			//				MenuSurvivorsAppearanceUI.close();
			//				
			//				MenuSurvivorsAppearanceUI.active = true;
			//			}
			//			else if(MenuSurvivorsGroupUI.active)
			//			{
			//				MenuSurvivorsGroupUI.close();
			//				
			//				MenuSurvivorsGroupUI.active = true;
			//			}
			//			else if(MenuSurvivorsClothingUI.active)
			//			{
			//				MenuSurvivorsClothingUI.close();
			//				
			//				MenuSurvivorsClothingUI.active = true;
			//			}

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedCharacterButton(ISleekElement button)
		{
			//			MenuSurvivorsGroupUI.close();
			//			MenuSurvivorsAppearanceUI.close();
			//			MenuSurvivorsClothingUI.close();
			//
			//			if(MenuSurvivorsCharacterUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			//				MenuSurvivorsCharacterUI.open();
			//			}

			MenuSurvivorsCharacterUI.open();
			close();
		}

		private static void onClickedAppearanceButton(ISleekElement button)
		{
			//			MenuSurvivorsCharacterUI.close();
			//			MenuSurvivorsGroupUI.close();
			//			MenuSurvivorsClothingUI.close();
			//
			//			if(MenuSurvivorsAppearanceUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			//				MenuSurvivorsAppearanceUI.open();
			//			}
			MenuSurvivorsAppearanceUI.open();
			close();
		}

		private static void onClickedGroupButton(ISleekElement button)
		{
			//			MenuSurvivorsCharacterUI.close();
			//			MenuSurvivorsAppearanceUI.close();
			//			MenuSurvivorsClothingUI.close();
			//
			//			if(MenuSurvivorsGroupUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			//				MenuSurvivorsGroupUI.open();
			//			}
			MenuSurvivorsGroupUI.open();
			close();
		}

		private static void onClickedClothingButton(ISleekElement button)
		{
			//			MenuSurvivorsCharacterUI.close();
			//			MenuSurvivorsAppearanceUI.close();
			//			MenuSurvivorsGroupUI.close();
			//
			//			if(MenuSurvivorsClothingUI.active)
			//			{
			//				close();
			//				MenuTitleUI.open();
			//			}
			//			else
			//			{
			//				MenuSurvivorsClothingUI.open();
			//			}
			MenuSurvivorsClothingUI.open();
			close();
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuDashboardUI.open();
			MenuTitleUI.open();
			close();
		}

		public void OnDestroy()
		{
			characterUI.OnDestroy();
			appearanceUI.OnDestroy();
			groupUI.OnDestroy();
			clothingUI.OnDestroy();
		}

		public MenuSurvivorsUI()
		{
			Local localization = Localization.read("/Menu/Survivors/MenuSurvivors.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Survivors/MenuSurvivors");

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

			characterButton = new SleekButtonIcon(icons.load<Texture2D>("Character"));
			characterButton.PositionOffset_X = -100;
			characterButton.PositionOffset_Y = -145;
			characterButton.PositionScale_X = 0.5f;
			characterButton.PositionScale_Y = 0.5f;
			characterButton.SizeOffset_X = 200;
			characterButton.SizeOffset_Y = 50;
			characterButton.text = localization.format("CharacterButtonText");
			characterButton.tooltip = localization.format("CharacterButtonTooltip");
			characterButton.onClickedButton += onClickedCharacterButton;
			characterButton.fontSize = ESleekFontSize.Medium;
			characterButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(characterButton);

			appearanceButton = new SleekButtonIcon(icons.load<Texture2D>("Appearance"));
			appearanceButton.PositionOffset_X = -100;
			appearanceButton.PositionOffset_Y = -85;
			appearanceButton.PositionScale_X = 0.5f;
			appearanceButton.PositionScale_Y = 0.5f;
			appearanceButton.SizeOffset_X = 200;
			appearanceButton.SizeOffset_Y = 50;
			appearanceButton.text = localization.format("AppearanceButtonText");
			appearanceButton.tooltip = localization.format("AppearanceButtonTooltip");
			appearanceButton.onClickedButton += onClickedAppearanceButton;
			appearanceButton.fontSize = ESleekFontSize.Medium;
			appearanceButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(appearanceButton);

			groupButton = new SleekButtonIcon(icons.load<Texture2D>("Group"));
			groupButton.PositionOffset_X = -100;
			groupButton.PositionOffset_Y = -25;
			groupButton.PositionScale_X = 0.5f;
			groupButton.PositionScale_Y = 0.5f;
			groupButton.SizeOffset_X = 200;
			groupButton.SizeOffset_Y = 50;
			groupButton.text = localization.format("GroupButtonText");
			groupButton.tooltip = localization.format("GroupButtonTooltip");
			groupButton.onClickedButton += onClickedGroupButton;
			groupButton.iconColor = ESleekTint.FOREGROUND;
			groupButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(groupButton);

			clothingButton = new SleekButtonIcon(icons.load<Texture2D>("Clothing"));
			clothingButton.PositionOffset_X = -100;
			clothingButton.PositionOffset_Y = 35;
			clothingButton.PositionScale_X = 0.5f;
			clothingButton.PositionScale_Y = 0.5f;
			clothingButton.SizeOffset_X = 200;
			clothingButton.SizeOffset_Y = 50;
			clothingButton.text = localization.format("ClothingButtonText");
			clothingButton.tooltip = localization.format("ClothingButtonTooltip");
			clothingButton.onClickedButton += onClickedClothingButton;
			clothingButton.fontSize = ESleekFontSize.Medium;
			clothingButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(clothingButton);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_X = -100;
			backButton.PositionOffset_Y = 95;
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

			characterUI = new MenuSurvivorsCharacterUI();
			appearanceUI = new MenuSurvivorsAppearanceUI();
			groupUI = new MenuSurvivorsGroupUI();
			clothingUI = new MenuSurvivorsClothingUI();
		}

		private MenuSurvivorsCharacterUI characterUI;
		private MenuSurvivorsAppearanceUI appearanceUI;
		private MenuSurvivorsGroupUI groupUI;
		internal static MenuSurvivorsClothingUI clothingUI;
	}
}
