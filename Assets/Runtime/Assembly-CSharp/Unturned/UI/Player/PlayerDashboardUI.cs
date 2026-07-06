////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerDashboardUI
	{
		public static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon inventoryButton;
		private static SleekButtonIcon craftingButton;
		private static SleekButtonIcon skillsButton;
		private static SleekButtonIcon informationButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			if (PlayerDashboardInventoryUI.active)
			{
				PlayerDashboardInventoryUI.active = false;

				PlayerDashboardInventoryUI.open();
			}
			else if (PlayerDashboardCraftingUI.active)
			{
				PlayerDashboardCraftingUI.active = false;

				PlayerDashboardCraftingUI.open();
			}
			else if (PlayerDashboardSkillsUI.active)
			{
				PlayerDashboardSkillsUI.active = false;

				PlayerDashboardSkillsUI.open();
			}
			else if (PlayerDashboardInformationUI.active)
			{
				PlayerDashboardInformationUI.active = false;

				PlayerDashboardInformationUI.open();
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

			if (PlayerDashboardInventoryUI.active)
			{
				PlayerDashboardInventoryUI.close();

				PlayerDashboardInventoryUI.active = true;
			}
			else if (PlayerDashboardCraftingUI.active)
			{
				PlayerDashboardCraftingUI.close();

				PlayerDashboardCraftingUI.active = true;
			}
			else if (PlayerDashboardSkillsUI.active)
			{
				PlayerDashboardSkillsUI.close();

				PlayerDashboardSkillsUI.active = true;
			}
			else if (PlayerDashboardInformationUI.active)
			{
				PlayerDashboardInformationUI.close();

				PlayerDashboardInformationUI.active = true;
			}

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedInventoryButton(ISleekElement button)
		{
			PlayerDashboardCraftingUI.close();
			PlayerDashboardSkillsUI.close();
			PlayerDashboardInformationUI.close();

			if (PlayerDashboardInventoryUI.active)
			{
				close();
				PlayerLifeUI.open();
			}
			else
			{
				PlayerDashboardInventoryUI.open();
			}
		}

		private static void onClickedCraftingButton(ISleekElement button)
		{
			PlayerDashboardInventoryUI.close();
			PlayerDashboardSkillsUI.close();
			PlayerDashboardInformationUI.close();

			if (PlayerDashboardCraftingUI.active)
			{
				close();
				PlayerLifeUI.open();
			}
			else
			{
				PlayerDashboardCraftingUI.open();
			}
		}

		private static void onClickedSkillsButton(ISleekElement button)
		{
			PlayerDashboardInventoryUI.close();
			PlayerDashboardCraftingUI.close();
			PlayerDashboardInformationUI.close();

			if (PlayerDashboardSkillsUI.active)
			{
				close();
				PlayerLifeUI.open();
			}
			else
			{
				PlayerDashboardSkillsUI.open();
			}
		}

		private static void onClickedInformationButton(ISleekElement button)
		{
			PlayerDashboardInventoryUI.close();
			PlayerDashboardCraftingUI.close();
			PlayerDashboardSkillsUI.close();

			if (PlayerDashboardInformationUI.active)
			{
				close();
				PlayerLifeUI.open();
			}
			else
			{
				PlayerDashboardInformationUI.open();
			}
		}

		private void createDisabledLabel(SleekButtonIcon parentButton, Local localization)
		{
			parentButton.isClickable = false;

			ISleekLabel disabledLabel = Glazier.Get().CreateLabel();
			disabledLabel.PositionOffset_X = parentButton.PositionOffset_X;
			disabledLabel.PositionScale_X = parentButton.PositionScale_X; ;
			disabledLabel.SizeOffset_X = -parentButton.SizeOffset_X;
			disabledLabel.SizeOffset_Y = parentButton.SizeOffset_Y;
			disabledLabel.SizeScale_X = parentButton.SizeScale_X;
			disabledLabel.Text = localization.format("Crafting_Disabled");
			disabledLabel.TextColor = ESleekTint.BAD;
			disabledLabel.FontSize = ESleekFontSize.Large;
			disabledLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			container.AddChild(disabledLabel);
		}

		/// <summary>
		/// Temporary to unbind events because this class is static for now. (sigh)
		/// </summary>
		public void OnDestroy()
		{
			craftingUI.OnDestroy();
			infoUI.OnDestroy();
		}

		public PlayerDashboardUI()
		{
			Local localization = Localization.read("/Player/PlayerDashboard.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerDashboard");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = -1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;

			inventoryButton = new SleekButtonIcon(icons.load<Texture2D>("Inventory"));
			inventoryButton.SizeOffset_X = -5;
			inventoryButton.SizeOffset_Y = 50;
			inventoryButton.SizeScale_X = 0.25f;
			inventoryButton.text = localization.format("Inventory", ControlsSettings.inventory);
			inventoryButton.tooltip = localization.format("Inventory_Tooltip");
			inventoryButton.onClickedButton += onClickedInventoryButton;
			inventoryButton.fontSize = ESleekFontSize.Medium;
			inventoryButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(inventoryButton);

			craftingButton = new SleekButtonIcon(icons.load<Texture2D>("Crafting"));
			craftingButton.PositionOffset_X = 5;
			craftingButton.PositionScale_X = 0.25f;
			craftingButton.SizeOffset_X = -10;
			craftingButton.SizeOffset_Y = 50;
			craftingButton.SizeScale_X = 0.25f;
			craftingButton.text = localization.format("Crafting", ControlsSettings.crafting);
			craftingButton.tooltip = localization.format("Crafting_Tooltip");
			craftingButton.iconColor = ESleekTint.FOREGROUND;
			craftingButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(craftingButton);

			if (!Level.IsCraftingAllowedByLevel)
			{
				createDisabledLabel(craftingButton, localization);
			}
			else
			{
				craftingButton.onClickedButton += onClickedCraftingButton;
			}

			skillsButton = new SleekButtonIcon(icons.load<Texture2D>("Skills"));
			skillsButton.PositionOffset_X = 5;
			skillsButton.PositionScale_X = 0.5f;
			skillsButton.SizeOffset_X = -10;
			skillsButton.SizeOffset_Y = 50;
			skillsButton.SizeScale_X = 0.25f;
			skillsButton.text = localization.format("Skills", ControlsSettings.skills);
			skillsButton.tooltip = localization.format("Skills_Tooltip");
			skillsButton.iconColor = ESleekTint.FOREGROUND;
			skillsButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(skillsButton);

			if (Level.info != null && Level.info.configData.Allow_Skills == false)
			{
				createDisabledLabel(skillsButton, localization);
			}
			else
			{
				skillsButton.onClickedButton += onClickedSkillsButton;
			}

			informationButton = new SleekButtonIcon(icons.load<Texture2D>("Information"));
			informationButton.PositionOffset_X = 5;
			informationButton.PositionScale_X = 0.75f;
			informationButton.SizeOffset_X = -5;
			informationButton.SizeOffset_Y = 50;
			informationButton.SizeScale_X = 0.25f;
			informationButton.text = localization.format("Information", ControlsSettings.map);
			informationButton.tooltip = localization.format("Information_Tooltip");
			informationButton.iconColor = ESleekTint.FOREGROUND;
			informationButton.fontSize = ESleekFontSize.Medium;
			container.AddChild(informationButton);

			if (Level.info != null && Level.info.configData.Allow_Information == false)
			{
				createDisabledLabel(informationButton, localization);
			}
			else
			{
				informationButton.onClickedButton += onClickedInformationButton;
			}

			if (Level.info != null && Level.info.type == ELevelType.HORDE)
			{
				inventoryButton.SizeScale_X = 0.5f;

				craftingButton.IsVisible = false;
				skillsButton.IsVisible = false;

				informationButton.PositionScale_X = 0.5f;
				informationButton.SizeScale_X = 0.5f;
			}

			new PlayerDashboardInventoryUI();
			craftingUI = new PlayerDashboardCraftingUI();
			new PlayerDashboardSkillsUI();
			infoUI = new PlayerDashboardInformationUI();
		}

		private PlayerDashboardCraftingUI craftingUI;
		private PlayerDashboardInformationUI infoUI;
	}
}
