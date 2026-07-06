////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Repurposed from the Modules UI because it was unused.
	/// </summary>
	public class MenuWorkshopSubscriptionsUI
	{
		// Should be cleaned up eventually.
		public static MenuWorkshopSubscriptionsUI instance
		{
			get;
			private set;
		}

		public static Local localization
		{
			get;
			private set;
		}

		private SleekFullscreenBox container;
		public static bool active;

		private SleekButtonIcon backButton;

		private ISleekBox headerBox;
		private ISleekScrollView moduleBox;
		private List<SleekManageWorkshopEntry> entryWidgets;
		private ISleekBox emptyBox;

		public void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();

			synchronizeEntries();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, 1);
		}

		private bool hasEntry(PublishedFileId_t fileId)
		{
			return entryWidgets.FindIndex(x => x.fileId == fileId) >= 0;
		}

		private void addEntry(PublishedFileId_t fileId)
		{
			SleekManageWorkshopEntry entry = new SleekManageWorkshopEntry(fileId);
			entry.PositionOffset_Y = entryWidgets.Count * 50;
			entry.SizeOffset_Y = 40;
			entry.SizeScale_X = 1;
			moduleBox.AddChild(entry);
			entryWidgets.Add(entry);
		}

		private void synchronizeEntries()
		{
			if (entryWidgets == null)
				entryWidgets = new List<SleekManageWorkshopEntry>();

			List<SteamContent> ugc = Provider.provider.workshopService.ugc;
			if (ugc != null && entryWidgets.Count != ugc.Count)
			{
				foreach (SteamContent content in ugc)
				{
					PublishedFileId_t fileId = content.publishedFileID;
					if (hasEntry(fileId))
						continue;

					addEntry(fileId);
				}
			}

			if (entryWidgets.Count > 0)
			{
				if (emptyBox != null)
				{
					container.RemoveChild(emptyBox);
					emptyBox = null;
				}
			}
			else
			{
				emptyBox = Glazier.Get().CreateBox();
				emptyBox.PositionOffset_Y = 60;
				emptyBox.SizeOffset_Y = 50;
				emptyBox.SizeScale_X = 1f;
				emptyBox.FontSize = ESleekFontSize.Medium;
				emptyBox.Text = localization.format("No_Subscriptions");
				container.AddChild(emptyBox);
			}

			moduleBox.ContentSizeOffset = new Vector2(0.0f, (entryWidgets.Count * 50) - 10);
		}

		private void onClickedManageInOverlayButton(ISleekElement button)
		{
			if (Provider.provider.browserService.canOpenBrowser)
			{
				string url = string.Format("https://steamcommunity.com/my/myworkshopfiles/?appid={0}&browsefilter=mysubscriptions", Provider.APP_ID);
				Provider.provider.browserService.open(url);
			}
			else
			{
				MenuUI.alert(MenuDashboardUI.localization.format("Overlay"));
			}
		}

		private void onClickedBackButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
		}

		public MenuWorkshopSubscriptionsUI()
		{
			instance = this;
			localization = Localization.read("/Menu/Workshop/MenuWorkshopSubscriptions.dat");

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
			container.AddChild(headerBox);

			ISleekLabel headerLabel = Glazier.Get().CreateLabel();
			headerLabel.PositionOffset_X = 10;
			headerLabel.SizeOffset_X = -210;
			headerLabel.SizeOffset_Y = 30;
			headerLabel.SizeScale_X = 1.0f;
			headerLabel.FontSize = ESleekFontSize.Medium;
			headerLabel.Text = localization.format("Header");
			headerBox.AddChild(headerLabel);

			ISleekLabel warningLabel = Glazier.Get().CreateLabel();
			warningLabel.PositionOffset_X = 10;
			warningLabel.PositionOffset_Y = -30;
			warningLabel.PositionScale_Y = 1.0f;
			warningLabel.SizeOffset_X = -210;
			warningLabel.SizeOffset_Y = 30;
			warningLabel.SizeScale_X = 1.0f;
			warningLabel.Text = localization.format("Enable_Warning");
			warningLabel.FontStyle = FontStyle.Italic;
			headerBox.AddChild(warningLabel);

			ISleekButton manageInOverlayButton = Glazier.Get().CreateButton();
			manageInOverlayButton.PositionOffset_X = -210;
			manageInOverlayButton.PositionOffset_Y = -15;
			manageInOverlayButton.PositionScale_X = 1.0f;
			manageInOverlayButton.PositionScale_Y = 0.5f;
			manageInOverlayButton.SizeOffset_X = 200;
			manageInOverlayButton.SizeOffset_Y = 30;
			manageInOverlayButton.Text = localization.format("Manage_Label");
			manageInOverlayButton.TooltipText = localization.format("Manage_Tooltip");
			manageInOverlayButton.OnClicked += onClickedManageInOverlayButton;
			headerBox.AddChild(manageInOverlayButton);

			moduleBox = Glazier.Get().CreateScrollView();
			moduleBox.PositionOffset_Y = 60;
			moduleBox.SizeOffset_Y = -120;
			moduleBox.SizeScale_X = 1;
			moduleBox.SizeScale_Y = 1;
			moduleBox.ScaleContentToWidth = true;
			container.AddChild(moduleBox);

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
