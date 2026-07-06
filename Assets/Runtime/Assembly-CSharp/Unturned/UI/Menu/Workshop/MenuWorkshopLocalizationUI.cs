////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuWorkshopLocalizationUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;
		private static ISleekButton refreshButton;

		private static ISleekBox headerBox;
		private static ISleekBox infoBox;
		private static ISleekScrollView messageBox;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			Localization.refresh();
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

		private static void refresh()
		{
			messageBox.RemoveAllChildren();

			for (int index = 0; index < Localization.messages.Count; index++)
			{
				ISleekBox error = Glazier.Get().CreateBox();
				error.PositionOffset_Y = index * 30;
				error.SizeOffset_Y = 30;
				error.SizeScale_X = 1;
				error.Text = Localization.messages[index];
				messageBox.AddChild(error);
			}

			messageBox.ContentSizeOffset = new Vector2(0.0f, Localization.messages.Count * 30);
			infoBox.IsVisible = Localization.messages.Count == 0;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
		}

		private static void onClickedRefreshButton(ISleekElement button)
		{
			Localization.refresh();
			refresh();
		}

		public MenuWorkshopLocalizationUI()
		{
			localization = Localization.read("/Menu/Workshop/MenuWorkshopLocalization.dat");

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

			headerBox = Glazier.Get().CreateBox();
			headerBox.SizeOffset_Y = 50;
			headerBox.SizeScale_X = 1f;
			headerBox.FontSize = ESleekFontSize.Medium;
			headerBox.Text = localization.format("Header", Provider.language, "English");
			container.AddChild(headerBox);

			infoBox = Glazier.Get().CreateBox();
			infoBox.PositionOffset_Y = 60;
			infoBox.SizeOffset_Y = 50;
			infoBox.SizeScale_X = 1f;
			infoBox.FontSize = ESleekFontSize.Medium;
			infoBox.Text = localization.format("No_Differences");
			container.AddChild(infoBox);
			infoBox.IsVisible = false;

			messageBox = Glazier.Get().CreateScrollView();
			messageBox.PositionOffset_Y = 60;
			messageBox.SizeOffset_Y = -120;
			messageBox.SizeScale_X = 1;
			messageBox.SizeScale_Y = 1;
			messageBox.ScaleContentToWidth = true;
			container.AddChild(messageBox);

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

			refreshButton = Glazier.Get().CreateButton();
			refreshButton.PositionOffset_X = -200;
			refreshButton.PositionOffset_Y = -50;
			refreshButton.PositionScale_X = 1f;
			refreshButton.PositionScale_Y = 1f;
			refreshButton.SizeOffset_X = 200;
			refreshButton.SizeOffset_Y = 50;
			refreshButton.Text = localization.format("Refresh");
			refreshButton.TooltipText = localization.format("Refresh_Tooltip");
			refreshButton.OnClicked += onClickedRefreshButton;
			refreshButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(refreshButton);
		}
	}
}
