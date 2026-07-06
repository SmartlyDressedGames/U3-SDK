////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekManageWorkshopEntry : SleekWrapper
	{
		public SleekManageWorkshopEntry(PublishedFileId_t fileId) : base()
		{
			this.fileId = fileId;

			CachedUGCDetails details;
			bool hasCachedDetails = TempSteamworksWorkshop.getCachedDetails(fileId, out details);
			string name = hasCachedDetails ? details.GetTitle() : fileId.ToString();

			ISleekBox background = Glazier.Get().CreateBox();
			background.SizeScale_X = 1;
			background.SizeScale_Y = 1;
			AddChild(background);

			ISleekToggle toggle = Glazier.Get().CreateToggle();
			toggle.PositionOffset_Y = -20;
			toggle.PositionScale_Y = 0.5f;
			toggle.SizeOffset_X = 40;
			toggle.SizeOffset_Y = 40;
			toggle.OnValueChanged += onToggledEnabled;
			toggle.Value = getEnabled();
			AddChild(toggle);

			ISleekLabel nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 40;
			nameLabel.PositionOffset_Y = -15;
			nameLabel.PositionScale_Y = 0.5f;
			nameLabel.SizeOffset_X = -40;
			nameLabel.SizeOffset_Y = 30;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.FontSize = ESleekFontSize.Medium;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.Text = name;
			nameLabel.TextColor = details.isBannedOrPrivate ? ESleekTint.BAD : ESleekTint.FONT;
			AddChild(nameLabel);

			float horizontalOffset = -5;
			SleekWorkshopSubscriptionButton manageSubscription = new SleekWorkshopSubscriptionButton();
			manageSubscription.PositionOffset_Y = -15;
			manageSubscription.PositionScale_X = 1.0f;
			manageSubscription.PositionScale_Y = 0.5f;
			manageSubscription.SizeOffset_X = 100;
			manageSubscription.SizeOffset_Y = 30;
			horizontalOffset -= manageSubscription.SizeOffset_X;
			manageSubscription.PositionOffset_X = horizontalOffset;
			manageSubscription.subscribeText = MenuWorkshopSubscriptionsUI.localization.format("Subscribe_Label");
			manageSubscription.unsubscribeText = MenuWorkshopSubscriptionsUI.localization.format("Unsubscribe_Label");
			manageSubscription.subscribeTooltip = MenuWorkshopSubscriptionsUI.localization.format("Subscribe_Tooltip", name);
			manageSubscription.unsubscribeTooltip = MenuWorkshopSubscriptionsUI.localization.format("Unsubscribe_Tooltip", name);
			manageSubscription.fileId = fileId;
			manageSubscription.synchronizeText();
			AddChild(manageSubscription);
			horizontalOffset -= 5;

			ISleekButton viewButton = Glazier.Get().CreateButton();
			viewButton.PositionOffset_Y = -15;
			viewButton.PositionScale_X = 1.0f;
			viewButton.PositionScale_Y = 0.5f;
			viewButton.SizeOffset_X = 100;
			viewButton.SizeOffset_Y = 30;
			horizontalOffset -= viewButton.SizeOffset_X;
			viewButton.PositionOffset_X = horizontalOffset;
			viewButton.Text = MenuWorkshopSubscriptionsUI.localization.format("View_Label");
			viewButton.TooltipText = MenuWorkshopSubscriptionsUI.localization.format("View_Tooltip", name);
			viewButton.TextAlignment = TextAnchor.MiddleCenter;
			viewButton.OnClicked += onClickedViewButton;
			AddChild(viewButton);
			horizontalOffset -= 5;

			if (ReadWrite.SupportsOpeningFileBrowser)
			{
				ulong sizeOnDisk;
				uint localTimestamp;
				if (SteamUGC.GetItemInstallInfo(fileId, out sizeOnDisk, out installPath, 1024, out localTimestamp))
				{
					ISleekButton browseFilesButton = Glazier.Get().CreateButton();
					browseFilesButton.PositionOffset_Y = -15;
					browseFilesButton.PositionScale_X = 1.0f;
					browseFilesButton.PositionScale_Y = 0.5f;
					browseFilesButton.SizeOffset_X = 100;
					browseFilesButton.SizeOffset_Y = 30;
					horizontalOffset -= browseFilesButton.SizeOffset_X;
					browseFilesButton.PositionOffset_X = horizontalOffset;
					browseFilesButton.Text = MenuWorkshopSubscriptionsUI.localization.format("BrowseFiles_Label");
					browseFilesButton.TooltipText = MenuWorkshopSubscriptionsUI.localization.format("BrowseFiles_Tooltip", name);
					browseFilesButton.OnClicked += OnClickedBrowseFilesButton;
					AddChild(browseFilesButton);
					horizontalOffset -= 5;
				}
				else
				{
					ISleekLabel notInstalledLabel = Glazier.Get().CreateLabel();
					notInstalledLabel.PositionOffset_Y = -15;
					notInstalledLabel.PositionScale_X = 1.0f;
					notInstalledLabel.PositionScale_Y = 0.5f;
					notInstalledLabel.SizeOffset_X = 100;
					notInstalledLabel.SizeOffset_Y = 30;
					horizontalOffset -= notInstalledLabel.SizeOffset_X;
					notInstalledLabel.PositionOffset_X = horizontalOffset;
					notInstalledLabel.Text = MenuWorkshopSubscriptionsUI.localization.format("NotInstalledLabel");
					notInstalledLabel.TextColor = ESleekTint.BAD;
					AddChild(notInstalledLabel);
					horizontalOffset -= 5;
				}

				ISleekLabel localTimestampLabel = Glazier.Get().CreateLabel();
				localTimestampLabel.PositionScale_X = 1.0f;
				localTimestampLabel.SizeOffset_X = 150;
				localTimestampLabel.SizeScale_Y = 1.0f;
				horizontalOffset -= localTimestampLabel.SizeOffset_X;
				localTimestampLabel.PositionOffset_X = horizontalOffset;
				localTimestampLabel.Text = MenuWorkshopSubscriptionsUI.localization.format("LocalTimestampLabel") + '\n' + DateTimeEx.FromUtcUnixTimeSeconds(localTimestamp).ToLocalTime();
				localTimestampLabel.FontSize = ESleekFontSize.Small;
				AddChild(localTimestampLabel);
				horizontalOffset -= 5;
			}

			if (hasCachedDetails)
			{
				ISleekLabel remoteTimestampLabel = Glazier.Get().CreateLabel();
				remoteTimestampLabel.PositionScale_X = 1.0f;
				remoteTimestampLabel.SizeOffset_X = 150;
				remoteTimestampLabel.SizeScale_Y = 1.0f;
				horizontalOffset -= remoteTimestampLabel.SizeOffset_X;
				remoteTimestampLabel.PositionOffset_X = horizontalOffset;
				remoteTimestampLabel.Text = MenuWorkshopSubscriptionsUI.localization.format("RemoteTimestampLabel") + '\n' + DateTimeEx.FromUtcUnixTimeSeconds(details.updateTimestamp).ToLocalTime();
				remoteTimestampLabel.FontSize = ESleekFontSize.Small;
				AddChild(remoteTimestampLabel);
				horizontalOffset -= 5;
			}

			EItemState stateFlags = (EItemState) SteamUGC.GetItemState(fileId);
			if ((stateFlags & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate)
			{
				ISleekLabel needsUpdateLabel = Glazier.Get().CreateLabel();
				needsUpdateLabel.PositionOffset_Y = -15;
				needsUpdateLabel.PositionScale_X = 1.0f;
				needsUpdateLabel.PositionScale_Y = 0.5f;
				needsUpdateLabel.SizeOffset_X = 100;
				needsUpdateLabel.SizeOffset_Y = 30;
				horizontalOffset -= needsUpdateLabel.SizeOffset_X;
				needsUpdateLabel.PositionOffset_X = horizontalOffset;
				needsUpdateLabel.Text = MenuWorkshopSubscriptionsUI.localization.format("ItemState_NeedsUpdate");
				needsUpdateLabel.TextColor = ESleekTint.BAD;
				AddChild(needsUpdateLabel);
				horizontalOffset -= 5;
			}
		}

		public PublishedFileId_t fileId;

		protected bool getEnabled()
		{
			return LocalWorkshopSettings.get().getEnabled(fileId);
		}

		protected void setEnabled(bool newEnabled)
		{
			LocalWorkshopSettings.get().setEnabled(fileId, newEnabled);
		}

		protected void onToggledEnabled(ISleekToggle toggle, bool state)
		{
			setEnabled(state);
		}

		protected void OnClickedBrowseFilesButton(ISleekElement button)
		{
			ReadWrite.OpenFileBrowser(installPath);
		}

		protected void onClickedViewButton(ISleekElement viewButton)
		{
			if (Provider.provider.browserService.canOpenBrowser)
			{
				string url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + fileId;
				Provider.provider.browserService.open(url);
			}
			else
			{
				MenuUI.alert(MenuDashboardUI.localization.format("Overlay"));
			}
		}

		private string installPath;
	}
}
