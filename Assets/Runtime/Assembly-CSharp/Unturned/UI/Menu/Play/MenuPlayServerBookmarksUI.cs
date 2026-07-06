////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class MenuPlayServerBookmarksUI : SleekFullscreenBox
	{
		public bool active;

		public void open()
		{
			if (active)
			{
				return;
			}

			active = true;

#if !DEDICATED_SERVER
			SynchronizeSortedBookmarks();
#endif

			AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			AnimateOutOfView(0, 1);
		}

#if !DEDICATED_SERVER
		private void SynchronizeSortedBookmarks()
		{
			sortedBookmarks.Clear();

			foreach (ServerBookmarkDetails details in ServerBookmarksManager.GetList())
			{
				sortedBookmarks.Add(details);
			}

			sortedBookmarks.Sort(new ServerBookmarkComparer_NameAscending());
			list.NotifyDataChanged();

			tutorialBox.IsVisible = sortedBookmarks.Count < 1;
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
		}

		private void OnClickedBookmark(ServerBookmarkDetails bookmarkDetails)
		{
			string connectHost = bookmarkDetails.host;

			IPv4Address address;
			CSteamID steamIdOverride;
			ushort queryPortOverride;
			// Nelson 2025-01-20: It doesn't matter if this returns false because we can fall back to server code.
			MenuPlayConnectUI.TryParseHostString(connectHost, out address, out steamIdOverride, out queryPortOverride);

			if (!address.IsZero)
			{
				ushort queryPort = queryPortOverride > 0 ? queryPortOverride : bookmarkDetails.queryPort;
				SteamConnectionInfo info = new SteamConnectionInfo(address.value, queryPort, string.Empty);
				close();
				MenuPlayConnectUI.open();
				MenuPlayConnectUI.connect(info, false, MenuPlayServerInfoUI.EServerInfoOpenContext.BOOKMARKS);
			}
			else
			{
				CSteamID serverCode = (steamIdOverride != CSteamID.Nil && steamIdOverride.BPersistentGameServerAccount())
					? steamIdOverride : bookmarkDetails.steamId;
				ServerConnectParameters connectParameters = new ServerConnectParameters(serverCode, string.Empty);

				Provider.connect(connectParameters, null, null);
			}
		}

		private ISleekElement OnCreateBookmarkElement(ServerBookmarkDetails bookmarkDetails)
		{
			SleekServerBookmark element = new SleekServerBookmark(bookmarkDetails);
			element.OnClickedBookmark += OnClickedBookmark;
			element.SizeOffset_X = -30;
			return element;
		}
#endif // !DEDICATED_SERVER

		public MenuPlayServerBookmarksUI()
		{
			active = false;

#if !DEDICATED_SERVER
			Local localization = Localization.read("/Menu/Play/MenuPlayServerBookmarks.dat");

			sortedBookmarks = new List<ServerBookmarkDetails>();

			list = new SleekList<ServerBookmarkDetails>();
			list.SizeOffset_Y = -60;
			list.SizeScale_X = 1;
			list.SizeScale_Y = 1;
			list.itemHeight = 40;
			list.scrollView.ReduceWidthWhenScrollbarVisible = false;
			list.onCreateElement = OnCreateBookmarkElement;
			list.SetData(sortedBookmarks);
			AddChild(list);

			tutorialBox = Glazier.Get().CreateBox();
			tutorialBox.SizeOffset_Y = 60;
			tutorialBox.SizeScale_X = 1;
			tutorialBox.PositionScale_Y = 0.5f;
			tutorialBox.PositionOffset_Y = -30;
			tutorialBox.Text = localization.format("Tutorial");
			tutorialBox.FontSize = ESleekFontSize.Medium;
			tutorialBox.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(tutorialBox);
			tutorialBox.IsVisible = false;

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += OnClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			AddChild(backButton);
#endif // !DEDICATED_SERVER
		}

#if !DEDICATED_SERVER
		// Showing a copy of the list rather than the actual list allows entries to be toggled.
		private List<ServerBookmarkDetails> sortedBookmarks;
		private SleekList<ServerBookmarkDetails> list;
		private ISleekLabel tutorialBox;
		private SleekButtonIcon backButton;
#endif // !DEDICATED_SERVER
	}
}
