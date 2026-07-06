////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuWorkshopErrorUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;
		private static ISleekButton refreshButton;

		private static ISleekBox headerBox;
		private static ISleekBox infoBox;
		private static SleekList<string> errorBox;

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

		private static void refresh()
		{
			errorBox.NotifyDataChanged();

			List<string> reportedErrors = Assets.getReportedErrorsList();
			infoBox.IsVisible = reportedErrors.Count == 0;
		}

		private static void OnClickedBrowseLogs(ISleekElement button)
		{
			ReadWrite.OpenFileBrowser(ReadWrite.folderPath(Logs.getLogFilePath()));
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
		}

		private static void onClickedRefreshButton(ISleekElement button)
		{
			refresh();
		}

		private static ISleekElement onCreateErrorMessage(string message)
		{
			ISleekBox error = Glazier.Get().CreateBox();
			error.Text = message;
			return error;
		}

		public MenuWorkshopErrorUI()
		{
			localization = Localization.read("/Menu/Workshop/MenuWorkshopError.dat");

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
			headerBox.Text = localization.format("Header");
			container.AddChild(headerBox);

			if (ReadWrite.SupportsOpeningFileBrowser)
			{
				ISleekButton browseLogsButton = Glazier.Get().CreateButton();
				browseLogsButton.PositionOffset_X = -210;
				browseLogsButton.PositionOffset_Y = -15;
				browseLogsButton.PositionScale_X = 1.0f;
				browseLogsButton.PositionScale_Y = 0.5f;
				browseLogsButton.SizeOffset_X = 200;
				browseLogsButton.SizeOffset_Y = 30;
				browseLogsButton.Text = localization.format("BrowseLogs_Label");
				browseLogsButton.TooltipText = localization.format("BrowseLogs_Tooltip");
				browseLogsButton.OnClicked += OnClickedBrowseLogs;
				headerBox.AddChild(browseLogsButton);
			}

			infoBox = Glazier.Get().CreateBox();
			infoBox.PositionOffset_Y = 60;
			infoBox.SizeOffset_Y = 50;
			infoBox.SizeScale_X = 1f;
			infoBox.FontSize = ESleekFontSize.Medium;
			infoBox.Text = localization.format("No_Errors");
			container.AddChild(infoBox);
			infoBox.IsVisible = false;

			errorBox = new SleekList<string>();
			errorBox.PositionOffset_Y = 60;
			errorBox.SizeOffset_Y = -120;
			errorBox.SizeScale_X = 1;
			errorBox.SizeScale_Y = 1;
			errorBox.itemHeight = 50;
			errorBox.onCreateElement = onCreateErrorMessage;
			errorBox.SetData(Assets.getReportedErrorsList());
			container.AddChild(errorBox);

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
