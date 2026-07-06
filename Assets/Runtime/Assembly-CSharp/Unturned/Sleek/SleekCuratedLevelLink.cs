////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class SleekCuratedLevelLink : SleekWrapper
	{
		private CuratedMapLink curatedMap;

		private ISleekBox backdrop;
		private ISleekButton viewOnWorkshopButton;
		private ISleekButton manageButton;
		private ISleekImage icon;

		private ISleekLabel nameLabel;

		private void onClickedViewButton(ISleekElement button)
		{
			if (curatedMap == null)
				return;

			if (Provider.provider.browserService.canOpenBrowser)
			{
				string url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + curatedMap.Workshop_File_Id;
				Provider.provider.browserService.open(url);
			}
			else
			{
				MenuUI.alert(MenuDashboardUI.localization.format("Overlay"));
			}
		}

		private bool getSubscribed()
		{
			return Provider.provider.workshopService.getSubscribed(curatedMap.Workshop_File_Id);
		}

		private void setSubscribed(bool subscribe)
		{
			Provider.provider.workshopService.setSubscribed(curatedMap.Workshop_File_Id, subscribe);

			// We auto-subscribe to required items, but we don't auto-unsubscribe because
			// there may be other installed mods that require the same files.
			if (subscribe)
			{
				foreach (ulong requiredFileId in curatedMap.Required_Workshop_File_Ids)
				{
					Provider.provider.workshopService.setSubscribed(requiredFileId, subscribe);
				}
			}
		}

		private void onClickedManageButton(ISleekElement button)
		{
			if (curatedMap == null)
				return;

			bool newSubscribed = !getSubscribed();
			updateManageLabel(newSubscribed);

			// Do not modify UI after setSubscribed because it may rebuild the level list.
			setSubscribed(newSubscribed);
		}

		private void updateManageLabel()
		{
			if (curatedMap == null)
				return;

			bool subscribed = getSubscribed();
			updateManageLabel(subscribed);
		}

		private void updateManageLabel(bool subscribed)
		{
			manageButton.Text = MenuPlaySingleplayerUI.localization.format(subscribed ? "Retired_Manage_Unsub" : "Retired_Manage_Sub");
		}

#if !DEDICATED_SERVER
		private void OnLiveConfigRefreshed()
		{
			if (hasCreatedStatusLabel)
			{
				return;
			}

			MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;
			if (liveConfig.status != EMapStatus.None && liveConfig.IsNowFeaturedTimeOrBypassed()
				&& liveConfig.IsFeatured(curatedMap.Workshop_File_Id))
			{
				SleekNew statusLabel = new SleekNew(liveConfig.status == EMapStatus.Updated);
				if (icon != null)
				{
					icon.AddChild(statusLabel);
				}
				else
				{
					AddChild(statusLabel);
				}
				hasCreatedStatusLabel = true;
			}
		}

		private bool hasCreatedStatusLabel;
#endif // !DEDICATED_SERVER

		public override void OnDestroy()
		{
			base.OnDestroy();

#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
#endif // !DEDICATED_SERVER
		}

		public SleekCuratedLevelLink(CuratedMapLink curatedMap) : base()
		{
			this.curatedMap = curatedMap;

			SizeOffset_X = 400;
			SizeOffset_Y = 100;

			backdrop = Glazier.Get().CreateBox();
			backdrop.SizeOffset_X = 0;
			backdrop.SizeOffset_Y = 0;
			backdrop.SizeScale_X = 1;
			backdrop.SizeScale_Y = 1;
			AddChild(backdrop);

			string iconsDir = Path.Join(ReadWrite.PATH, "CuratedMapIcons");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
			if (!Directory.Exists(iconsDir) && Provider.steamAppInstallDirectory != null)
			{
				iconsDir = PathEx.Join(Provider.steamAppInstallDirectory, "CuratedMapIcons");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST

			string iconPath = Path.Join(iconsDir, curatedMap.Workshop_File_Id + ".png");
			if (ReadWrite.fileExists(iconPath, false, false))
			{
				icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 10;
				icon.PositionOffset_Y = 10;
				icon.SizeOffset_X = -20;
				icon.SizeOffset_Y = -20;
				icon.SizeScale_X = 1;
				icon.SizeScale_Y = 1;
				icon.Texture = ReadWrite.readTextureFromFile(iconPath);
				icon.ShouldDestroyTexture = true;
				backdrop.AddChild(icon);
			}

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_Y = 10;
			nameLabel.SizeScale_X = 1f;
			nameLabel.SizeOffset_Y = 50;
			nameLabel.TextAlignment = TextAnchor.MiddleCenter;
			nameLabel.FontSize = ESleekFontSize.Medium;
			nameLabel.Text = curatedMap.Name;
			nameLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			backdrop.AddChild(nameLabel);

			viewOnWorkshopButton = Glazier.Get().CreateButton();
			viewOnWorkshopButton.PositionOffset_X = 15;
			viewOnWorkshopButton.PositionOffset_Y = -45;
			viewOnWorkshopButton.PositionScale_Y = 1;
			viewOnWorkshopButton.SizeOffset_X = 150;
			viewOnWorkshopButton.SizeOffset_Y = 30;
			viewOnWorkshopButton.FontSize = ESleekFontSize.Small;
			viewOnWorkshopButton.Text = MenuPlaySingleplayerUI.localization.format("Retired_View_Label");
			viewOnWorkshopButton.TooltipText = MenuPlaySingleplayerUI.localization.format("Retired_View_Tooltip");
			viewOnWorkshopButton.OnClicked += onClickedViewButton;
			backdrop.AddChild(viewOnWorkshopButton);

			manageButton = Glazier.Get().CreateButton();
			manageButton.PositionOffset_X = -165;
			manageButton.PositionOffset_Y = -45;
			manageButton.PositionScale_X = 1;
			manageButton.PositionScale_Y = 1;
			manageButton.SizeOffset_X = 150;
			manageButton.SizeOffset_Y = 30;
			manageButton.FontSize = ESleekFontSize.Small;
			manageButton.TooltipText = MenuPlaySingleplayerUI.localization.format("Retired_Manage_Tooltip");
			updateManageLabel();
			manageButton.OnClicked += onClickedManageButton;
			backdrop.AddChild(manageButton);

#if !DEDICATED_SERVER
			hasCreatedStatusLabel = false;
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
			OnLiveConfigRefreshed();
#endif // !DEDICATED_SERVER
		}
	}
}
