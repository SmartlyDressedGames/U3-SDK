////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class SubcontentInfo
	{
		public string content;
		public string url;
		public bool isImage;
		public bool isLink;
	}

	public class MenuDashboardUI
	{
		public static Local localization;
		public static IconsBundle icons;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon playButton;
		private static SleekButtonIcon survivorsButton;
		private static SleekButtonIcon configurationButton;
		private static SleekButtonIcon workshopButton;
		private static SleekButtonIcon exitButton;

		private static ISleekScrollView mainScrollView;

		private static ISleekButton proButton;
		private static ISleekLabel proLabel;
		private static ISleekLabel featureLabel;

		private static ISleekButton alertBox;
		private static ISleekImage alertImage;
		private static SleekWebImage alertWebImage;
		private static ISleekLabel alertHeaderLabel;
		private static ISleekLabel alertLinkLabel;
		private static ISleekLabel dismissAlertLabel;
		private static ISleekLabel alertBodyLabel;

		private static float mainHeaderOffset;
#if WITH_NOREDIST // Don't advertise items in U3-SDK.
		private static bool hasCreatedItemStoreButton;
#endif

		private static NewsResponse newsResponse;

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

		private static void onClickedPlayButton(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
			MenuTitleUI.close();
		}

		private static void onClickedSurvivorsButton(ISleekElement button)
		{
			MenuSurvivorsUI.open();
			close();
			MenuTitleUI.close();
		}

		private static void onClickedConfigurationButton(ISleekElement button)
		{
			MenuConfigurationUI.open();
			close();
			MenuTitleUI.close();
		}

		private static void onClickedWorkshopButton(ISleekElement button)
		{
			MenuWorkshopUI.open();
			close();
			MenuTitleUI.close();
		}

		private static void onClickedExitButton(ISleekElement button)
		{
			MenuPauseUI.open();
			close();
			MenuTitleUI.close();
		}

		private static void onClickedProButton(ISleekElement button)
		{
			Provider.provider.storeService.open(new SteamworksProvider.Services.Store.SteamworksStorePackageID(Provider.PRO_ID.m_AppId));
		}

		private static void onClickedAlertButton(ISleekElement button)
		{
#if !DEDICATED_SERVER
			LiveConfigData liveConfig = LiveConfig.Get();

			ConvenientSavedata.get().write("MainMenuAlertSeenId", liveConfig.mainMenuAlert.id);

			if (alertBox != null)
			{
				alertBox.IsVisible = false;
			}

			if (!string.IsNullOrEmpty(liveConfig.mainMenuAlert.link))
			{
				if (!Provider.provider.browserService.canOpenBrowser)
				{
					MenuUI.alert(MenuSurvivorsCharacterUI.localization.format("Overlay"));

					return;
				}

				Provider.provider.browserService.open(liveConfig.mainMenuAlert.link);
			}
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Has a new announcement been posted by the developer?
		/// If so, it is given priority over the featured workshop item.
		/// </summary>
		private static ISleekElement newAnnouncement;
		private static ISleekBox workshopBox;
		private static ISleekElement itemStoreSaleNews;
		private static UGCQueryHandle_t popularWorkshopHandle = UGCQueryHandle_t.Invalid; // Handle to query current popular stuff
		private static UGCQueryHandle_t featuredWorkshopHandle = UGCQueryHandle_t.Invalid; // Retrieve data on the most popular item

		private static void InsertSteamBbCode(ISleekElement parent, string contents, bool useLinkFiltering, bool inferLineBreaks)
		{
			if (string.IsNullOrEmpty(contents))
			{
				return;
			}

			BbCodeTokenizer tokenizer = new BbCodeTokenizer();
			tokenizer.ParseLineBreaks = !inferLineBreaks;
			List<BbCodeToken> tokens = tokenizer.Tokenize(contents);
			if (tokenizer.HasError)
			{
				UnturnedLog.warn($"Error tokenizing Steam BBcode: \"{tokenizer.ErrorMessage}\" Input: \"{contents}\"");

				ISleekLabel contentLabel = Glazier.Get().CreateLabel();
				contentLabel.Text = contents;
				contentLabel.UseManualLayout = false;
				contentLabel.TextAlignment = TextAnchor.UpperLeft;
				parent.AddChild(contentLabel);

				return;
			}

#if UNITY_EDITOR
// 			foreach (BbCodeToken token in tokens)
// 			{
// 				UnturnedLog.info(token);
// 			}
#endif // UNITY_EDITOR

			BbCodeWidgetConverter widgetConverter = new BbCodeWidgetConverter();
			widgetConverter.InferLineBreaks = inferLineBreaks;
			List<BbCodeWidget> widgets = widgetConverter.Convert(tokens);
			if (widgetConverter.HasError)
			{
				UnturnedLog.warn($"Error converting Steam BBcode to widgets: \"{widgetConverter.ErrorMessage}\" Input: \"{contents}\"");

				ISleekLabel contentLabel = Glazier.Get().CreateLabel();
				contentLabel.Text = contents;
				contentLabel.UseManualLayout = false;
				contentLabel.TextAlignment = TextAnchor.UpperLeft;
				parent.AddChild(contentLabel);

				return;
			}

			foreach (BbCodeWidget widget in	widgets)
			{
				switch (widget.widgetType)
				{
					case EBbCodeWidgetType.RichTextLabel:
					{
						ISleekLabel contentLabel = Glazier.Get().CreateLabel();
						contentLabel.Text = widget.widgetData;
						contentLabel.UseManualLayout = false;
						contentLabel.AllowRichText = true;
						contentLabel.TextAlignment = TextAnchor.UpperLeft;
						parent.AddChild(contentLabel);
						break;
					}

					case EBbCodeWidgetType.Image:
					{
						SleekWebImage webImage = new SleekWebImage();
						webImage.UseManualLayout = false;
						webImage.UseWidthLayoutOverride = true;
						webImage.UseHeightLayoutOverride = true;
						webImage.useImageDimensions = true;

						// Steam stopped formatting this in the API response, so we hack-in the expected value.
						string url = widget.widgetData.Replace("{STEAM_CLAN_IMAGE}", STEAM_CLAN_IMAGE);
						webImage.Refresh(url, shouldCache: false);

						parent.AddChild(webImage);
						break;
					}

					case EBbCodeWidgetType.YouTubeButton:
					{
						SleekYouTubeVideoButton youtubeButton = new SleekYouTubeVideoButton(icons);
						youtubeButton.UseManualLayout = false;
						youtubeButton.UseWidthLayoutOverride = true;
						youtubeButton.UseHeightLayoutOverride = true;
						youtubeButton.Refresh(widget.widgetData);
						parent.AddChild(youtubeButton);
						break;
					}

					case EBbCodeWidgetType.LinkButton:
					{
						string url;
						string displayText;
						int delimiterIndex = widget.widgetData.IndexOf(',');
						if (delimiterIndex < 0)
						{
							url = widget.widgetData;
							displayText = url;
						}
						else
						{
							url = widget.widgetData.Substring(0, delimiterIndex);
							displayText = widget.widgetData.Substring(delimiterIndex + 1);
						}

						if (!useLinkFiltering || WebUtils.CanParseThirdPartyUrl(url))
						{
							SleekWebLinkButton linkButton = new SleekWebLinkButton();
							linkButton.Text = displayText;
							linkButton.Url = url;
							linkButton.UseManualLayout = false;
							linkButton.UseChildAutoLayout = ESleekChildLayout.Vertical;
							linkButton.UseHeightLayoutOverride = true;
							linkButton.ExpandChildren = true;
							linkButton.SizeOffset_Y = 30;
							linkButton.useLinkFiltering = useLinkFiltering;
							parent.AddChild(linkButton);
						}
						else
						{
							UnturnedLog.warn("Ignoring potentially unsafe link in BBcode: {0}", url);
						}
						break;
					}
				}
			}
		}

		private static void ReviseNewsOrder()
		{
			// If there's a new unseen announcement then item store sale should go between news and workshop.
			bool itemStoreBeforeWorkshop = newAnnouncement != null;

			if (itemStoreSaleNews != null && !itemStoreBeforeWorkshop)
			{
				itemStoreSaleNews.SetAsFirstSibling();
			}

			if (workshopBox != null)
			{
				workshopBox.SetAsFirstSibling();
			}

			if (itemStoreSaleNews != null && itemStoreBeforeWorkshop)
			{
				itemStoreSaleNews.SetAsFirstSibling();
			}

			// New unseen announcement takes highest priority.
			if (newAnnouncement != null)
			{
				newAnnouncement.SetAsFirstSibling();
			}
		}

		/// <summary>
		/// Called after newsResponse is updated from web request.
		/// </summary>
		private static void receiveNewsResponse()
		{
			for (int index = 0; index < newsResponse.AppNews.NewsItems.Length; index++)
			{
				NewsItem item = newsResponse.AppNews.NewsItems[index];
				if (item == null || string.IsNullOrEmpty(item.Title))
					continue;

				ISleekBox newsBox = Glazier.Get().CreateBox();
				newsBox.SizeScale_X = 1.0f;
				newsBox.UseManualLayout = false;
				newsBox.UseChildAutoLayout = ESleekChildLayout.Vertical;
				newsBox.ChildAutoLayoutPadding = 5.0f;

				if (item.Title.StartsWith("Community Blog") && Glazier.Get().SupportsTilingSprite)
				{
					ISleekSprite backgroundSprite = Glazier.Get().CreateSprite(icons.load<Sprite>("CommunityBlogBackground"));
					backgroundSprite.IgnoreLayout = true;
					backgroundSprite.SizeScale_X = 1.0f;
					backgroundSprite.SizeScale_Y = 1.0f;
					backgroundSprite.TintColor = SleekColor.BackgroundIfLight(new Color(0.0f, 0.0f, 0.0f, 0.25f));
					backgroundSprite.DrawMethod = ESleekSpriteType.Tiled;
					newsBox.AddChild(backgroundSprite);
				}

				ISleekLabel titleLabel = Glazier.Get().CreateLabel();
				titleLabel.Text = item.Title;
				titleLabel.UseManualLayout = false;
				titleLabel.TextAlignment = TextAnchor.UpperLeft;
				titleLabel.FontSize = ESleekFontSize.Large;
				newsBox.AddChild(titleLabel);

				System.DateTime time = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
				time = time.AddSeconds(item.Date).ToLocalTime();

				ISleekLabel authorLabel = Glazier.Get().CreateLabel();
				authorLabel.Text = localization.format("News_Author", time, item.Author);
				authorLabel.UseManualLayout = false;
				authorLabel.TextAlignment = TextAnchor.UpperLeft;
				authorLabel.FontSize = ESleekFontSize.Tiny;
				authorLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
				newsBox.AddChild(authorLabel);

				try
				{
					const bool useLinkFiltering = false; // Allow our official news posts to directly link anywhere.
					InsertSteamBbCode(newsBox, item.Contents, useLinkFiltering, true);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception parsing announcement BB code:");
				}

				SleekWebLinkButton commentsButton = new SleekWebLinkButton();
				commentsButton.Text = localization.format("News_Comments_Link");
				commentsButton.Url = item.URL;
				commentsButton.UseManualLayout = false;
				commentsButton.UseChildAutoLayout = ESleekChildLayout.Vertical;
				commentsButton.UseHeightLayoutOverride = true;
				commentsButton.ExpandChildren = true;
				commentsButton.SizeOffset_Y = 30;
				newsBox.AddChild(commentsButton);

				mainScrollView.AddChild(newsBox);

				if (index == 0)
				{
					bool isNew;
					const string seenAnnouncementKey = "Newest_Announcement";
					long seenAnnouncement;
					if (ConvenientSavedata.get().read(seenAnnouncementKey, out seenAnnouncement))
					{
						isNew = seenAnnouncement != item.Date;
					}
					else
					{
						isNew = true;
					}

					if (isNew)
					{
						ConvenientSavedata.get().write(seenAnnouncementKey, item.Date);
						newAnnouncement = newsBox;
						ReviseNewsOrder();
					}
				}
			}
		}

		private static void OnUpdateDetected(string versionString, bool isRollback)
		{
			ISleekBox updateBox = Glazier.Get().CreateBox();
			updateBox.PositionOffset_X = 210;
			updateBox.PositionOffset_Y = mainHeaderOffset;
			updateBox.SizeOffset_Y = 40;
			updateBox.SizeOffset_X = -210;
			updateBox.SizeScale_X = 1f;
			updateBox.FontSize = ESleekFontSize.Medium;
			container.AddChild(updateBox);

			string key = isRollback ? "RollbackAvailable" : "UpdateAvailable";
			string message = localization.format(key, versionString);
			RichTextUtil.replaceNewlineMarkup(ref message);
			updateBox.Text = message;

			mainHeaderOffset += updateBox.SizeOffset_Y + 10;
			mainScrollView.PositionOffset_Y += updateBox.SizeOffset_Y + 10;
			mainScrollView.SizeOffset_Y -= (updateBox.SizeOffset_Y + 10);
		}

		/// <summary>
		/// Read News.txt file from Cloud directory to preview on main menu.
		/// </summary>
		private static bool readNewsPreview()
		{
			string previewPath = System.IO.Path.Combine(ReadWrite.PATH, "Cloud", "News.txt");
			if (!System.IO.File.Exists(previewPath))
				return false;

			string news = System.IO.File.ReadAllText(previewPath);

			NewsItem newsItem = new NewsItem();
			newsItem.Author = "Preview";
			newsItem.Title = "Preview";
			newsItem.Contents = news;
			newsItem.Date = System.DateTime.UtcNow.ToUnixTimeSeconds();

			newsResponse = new NewsResponse();
			newsResponse.AppNews = new AppNews();
			newsResponse.AppNews.NewsItems = new NewsItem[]
			{
				newsItem
			};

			receiveNewsResponse();
			return true;
		}

		internal static void receiveSteamNews(string data)
		{
			newsResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<NewsResponse>(data);
			receiveNewsResponse();
		}

		private static void spawnFeaturedWorkshopArticle()
		{
#if !DEDICATED_SERVER
			SteamUGCDetails_t details;
			if (SteamUGC.GetQueryUGCResult(featuredWorkshopHandle, 0, out details) == false)
			{
				UnturnedLog.warn("Unable to retrieve details for featured workshop article");
				return;
			}

			if (details.m_eResult != EResult.k_EResultOK)
			{
				UnturnedLog.warn("Error retrieving details for featured workshop item: " + details.m_eResult);
				return;
			}

			if (details.m_bBanned)
			{
				// Maybe account of manually featured file was hijacked and replaced with inappropriate content.
				UnturnedLog.warn("Ignoring featured workshop file because it was banned");
				return;
			}

			if (details.m_eVisibility == ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate)
			{
				// Sometimes banned items are not flagged as banned, only private.
				// Unlisted and friends-only are valid because it might be a pre-release curated map.
				UnturnedLog.warn("Ignoring featured workshop file because visibility is private");
				return;
			}

			if (string.IsNullOrWhiteSpace(details.m_rgchTitle))
			{
				UnturnedLog.warn($"Ignoring featured workshop file {details.m_nPublishedFileId} because title is empty");
				return;
			}

			if (ProfanityFilter.NaiveContainsHardcodedBannedWord(details.m_rgchTitle))
			{
				UnturnedLog.warn($"Ignoring featured workshop file {details.m_nPublishedFileId} because title contains banned string. May need moderator attention!");
				return;
			}

			foreach (string word in featuredWorkshopTitleBannedWords)
			{
				if (details.m_rgchTitle.Contains(word, System.StringComparison.InvariantCultureIgnoreCase))
				{
					UnturnedLog.warn($"Ignoring featured workshop file {details.m_nPublishedFileId} because title contains inappropriate string \"{word}\"");
					return;
				}
			}

			workshopBox = Glazier.Get().CreateBox();
			workshopBox.SizeScale_X = 1.0f;
			workshopBox.UseManualLayout = false;
			workshopBox.UseChildAutoLayout = ESleekChildLayout.Vertical;
			workshopBox.ChildAutoLayoutPadding = 5.0f;
			mainScrollView.AddChild(workshopBox);

			ReviseNewsOrder();

			MainMenuWorkshopFeaturedLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.featured;

			// Regardless of whether the map is showing within the explicitly featured window, or has naturally become popular,
			// we show the type (e.g. Highlighted: X) title and link to their Stockpile items if it's still the curated map.
			bool isExplicitlyFeatured = liveConfig.IsNowFeaturedTimeOrBypassed()
				&& liveConfig.IsFeatured(details.m_nPublishedFileId.m_PublishedFileId);

			string titleTextKey;
			if (isExplicitlyFeatured)
			{
				switch (liveConfig.type)
				{
					case EFeaturedWorkshopType.Curated:
						titleTextKey = "Curated_Workshop_Title";
						break;

					default:
					case EFeaturedWorkshopType.Highlighted:
						titleTextKey = "Highlighted_Workshop_Title";
						break;
				}
			}
			else
			{
				titleTextKey = "Featured_Workshop_Title";
			}

			string titleText = localization.format(titleTextKey, details.m_rgchTitle);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (LiveConfig.useEditorLiveConfig && isExplicitlyFeatured && liveConfig.useTimeWindow)
			{
				titleText += $" ({liveConfig.startTime.ToLocalTime()} - {liveConfig.endTime.ToLocalTime()})";
			}
#endif // !UNITY_EDITOR || DEVELOPMENT_BUILD

			ISleekElement titleLayout = Glazier.Get().CreateFrame();
			titleLayout.UseManualLayout = false;
			titleLayout.UseChildAutoLayout = ESleekChildLayout.Horizontal;
			workshopBox.AddChild(titleLayout);

			ISleekLabel titleLabel = Glazier.Get().CreateLabel();
			titleLabel.UseManualLayout = false;
			titleLabel.Text = titleText;
			titleLabel.FontSize = ESleekFontSize.Large;
			titleLabel.TextAlignment = TextAnchor.UpperLeft;
			titleLayout.AddChild(titleLabel);

			if (isExplicitlyFeatured && liveConfig.status != EMapStatus.None)
			{
				bool isUpdate = liveConfig.status == EMapStatus.Updated;

				ISleekLabel statusLabel = Glazier.Get().CreateLabel();
				statusLabel.UseManualLayout = false;
				statusLabel.TextAlignment = TextAnchor.UpperLeft;
				statusLabel.Text = Provider.localization.format(isUpdate ? "Updated" : "New");
				statusLabel.TextColor = Color.green;
				statusLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				titleLayout.AddChild(statusLabel);
			}

			// Button to hide this workshop article, and do not show it in the future.
			SleekDismissWorkshopArticleButton dismissWorkshopArticleButton = new SleekDismissWorkshopArticleButton();
			dismissWorkshopArticleButton.PositionOffset_X = -105;
			dismissWorkshopArticleButton.PositionOffset_Y = 5;
			dismissWorkshopArticleButton.PositionScale_X = 1.0f;
			dismissWorkshopArticleButton.SizeOffset_X = 100;
			dismissWorkshopArticleButton.SizeOffset_Y = 30;
			dismissWorkshopArticleButton.internalButton.Text = localization.format("Featured_Workshop_Dismiss");
			dismissWorkshopArticleButton.articleId = details.m_nPublishedFileId.m_PublishedFileId;
			dismissWorkshopArticleButton.targetContent = workshopBox;
			dismissWorkshopArticleButton.IgnoreLayout = true;
			workshopBox.AddChild(dismissWorkshopArticleButton);

			string previewURL;
			if (SteamUGC.GetQueryUGCPreviewURL(featuredWorkshopHandle, 0, out previewURL, 1024))
			{
				SleekWebImage webImage = new SleekWebImage();
				webImage.UseManualLayout = false;
				webImage.useImageDimensions = true;
				webImage.UseWidthLayoutOverride = true;
				webImage.UseHeightLayoutOverride = true;
				// / Many workshop items have a 1920x1080 preview image which feels overwhelming, so scale down to half that size.
				webImage.maxImageDimensionsWidth = 960;
				webImage.maxImageDimensionsHeight = 540;
				workshopBox.AddChild(webImage);
				webImage.Refresh(previewURL, shouldCache: false);
			}

			SleekReadMoreButton readMoreButton = new SleekReadMoreButton();
			readMoreButton.UseManualLayout = false;
			readMoreButton.UseChildAutoLayout = ESleekChildLayout.Vertical;
			readMoreButton.UseHeightLayoutOverride = true;
			readMoreButton.ExpandChildren = true;
			readMoreButton.SizeOffset_Y = 30;

			ISleekElement readMoreContent = Glazier.Get().CreateFrame();
			readMoreContent.UseManualLayout = false;
			readMoreContent.IsVisible = false;
			readMoreContent.UseChildAutoLayout = ESleekChildLayout.Vertical;

			readMoreButton.targetContent = readMoreContent;
			workshopBox.AddChild(readMoreButton);
			workshopBox.AddChild(readMoreContent);

			if (isExplicitlyFeatured && liveConfig.autoExpandDescription)
			{
				// Before refreshing ReadMore link text.
				readMoreContent.IsVisible = true;
			}

			try
			{
				string description = details.m_rgchDescription;
				if (isExplicitlyFeatured && !string.IsNullOrEmpty(liveConfig.overrideDescription))
				{
					description = liveConfig.overrideDescription;
				}

				const bool useLinkFiltering = true; // Workshop file could include links to untrusted sites, must filter.
				InsertSteamBbCode(readMoreContent, description, useLinkFiltering, false);
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, $"Caught exception parsing workshop file {details.m_nPublishedFileId} description BB code:");
			}

			readMoreButton.onText = localization.format("ReadMore_Link_On");
			readMoreButton.offText = localization.format("ReadMore_Link_Off");
			readMoreButton.Refresh();

			ISleekElement buttonsContainer = Glazier.Get().CreateFrame();
			buttonsContainer.UseManualLayout = false;
			buttonsContainer.UseChildAutoLayout = ESleekChildLayout.Horizontal;
			buttonsContainer.ExpandChildren = true;
			buttonsContainer.SizeOffset_Y = 30;
			buttonsContainer.UseHeightLayoutOverride = true;
			workshopBox.AddChild(buttonsContainer);

			SleekWebLinkButton viewOnWorkshopButton = new SleekWebLinkButton();
			viewOnWorkshopButton.UseManualLayout = false;
			viewOnWorkshopButton.Text = localization.format("Featured_Workshop_Link");
			viewOnWorkshopButton.Url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + details.m_nPublishedFileId;
			viewOnWorkshopButton.UseChildAutoLayout = ESleekChildLayout.Vertical;
			viewOnWorkshopButton.ExpandChildren = true;
			buttonsContainer.AddChild(viewOnWorkshopButton);

			int[] itemdefids = liveConfig.associatedStockpileItems;
			if (isExplicitlyFeatured && itemdefids != null && itemdefids.Length > 0)
			{
				List<int> eligibleItems = new List<int>(itemdefids.Length);
				foreach (int item in itemdefids)
				{
					if (item > 0 && !Provider.provider.economyService.isItemHiddenByCountryRestrictions(item))
					{
						eligibleItems.Add(item);
					}
				}

				int featuredItem = eligibleItems.RandomOrDefault();
				if (featuredItem > 0)
				{
					string itemName = Provider.provider.economyService.getInventoryName(featuredItem);
					if (string.IsNullOrEmpty(itemName))
					{
						UnturnedLog.warn("Unknown itemdefid {0} specified in featured workshop stockpile items", featuredItem);
					}
					else
					{
						string stockpileText = localization.format("Featured_Workshop_Stockpile_Link", itemName);

						SleekStockpileLinkButton stockpileButton = new SleekStockpileLinkButton();
						stockpileButton.UseManualLayout = false;
						stockpileButton.internalButton.Text = stockpileText;
						stockpileButton.itemdefid = featuredItem;
						stockpileButton.UseChildAutoLayout = ESleekChildLayout.Vertical;
						stockpileButton.ExpandChildren = true;
						buttonsContainer.AddChild(stockpileButton);
					}
				}
			}

			if (isExplicitlyFeatured && !string.IsNullOrEmpty(liveConfig.linkURL))
			{
				SleekWebLinkButton newsButton = new SleekWebLinkButton();
				newsButton.UseManualLayout = false;
				newsButton.Text = liveConfig.linkText;
				newsButton.Url = liveConfig.linkURL;
				newsButton.UseChildAutoLayout = ESleekChildLayout.Vertical;
				newsButton.ExpandChildren = true;
				buttonsContainer.AddChild(newsButton);
			}

			SleekWorkshopSubscriptionButton manageSubscription = new SleekWorkshopSubscriptionButton();
			manageSubscription.UseManualLayout = false;
			manageSubscription.subscribeText = localization.format("Featured_Workshop_Sub");
			manageSubscription.unsubscribeText = localization.format("Featured_Workshop_Unsub");
			manageSubscription.subscribeTooltip = localization.format("Subscribe_Tooltip", details.m_rgchTitle);
			manageSubscription.unsubscribeTooltip = localization.format("Unsubscribe_Tooltip", details.m_rgchTitle);
			manageSubscription.fileId = details.m_nPublishedFileId;
			manageSubscription.synchronizeText();
			manageSubscription.UseChildAutoLayout = ESleekChildLayout.Vertical;
			manageSubscription.ExpandChildren = true;
			buttonsContainer.AddChild(manageSubscription);
#endif // !DEDICATED_SERVER
		}

		/// <summary>
		/// Helper for handlePopularItemResults.
		/// If player has not dismissed item at index then proceed with query and return true.
		/// </summary>
		private static bool featurePopularItem(uint index)
		{
			SteamUGCDetails_t details;
			if (!SteamUGC.GetQueryUGCResult(popularWorkshopHandle, index, out details))
			{
				UnturnedLog.warn($"Unable to get popular workshop item details for index {index}");
				return false;
			}

			if (details.m_eResult != EResult.k_EResultOK)
			{
				UnturnedLog.warn($"Error retrieving details for popular workshop file {details.m_nPublishedFileId}: {details.m_eResult}");
				return false;
			}

			if (details.m_bBanned)
			{
				UnturnedLog.warn($"Ignoring popular workshop file {details.m_nPublishedFileId} because it was banned");
				return false;
			}

			if (details.m_eVisibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic)
			{
				// Sometimes banned items are not flagged as banned, only private.
				UnturnedLog.warn($"Ignoring popular workshop file {details.m_nPublishedFileId} because visibility is {details.m_eVisibility}");
				return false;
			}

#if !DEDICATED_SERVER
			var liveConfig = LiveConfig.Get().mainMenuWorkshop.popular;
			if (liveConfig.IsHidden(details.m_nPublishedFileId.m_PublishedFileId))
			{
				UnturnedLog.info($"Ignoring popular workshop file {details.m_nPublishedFileId} because it's not eligible");
				return false;
			}
#endif // !DEDICATED_SERVER

			// Nelson 2024-04-23: Looked at doing some content filtering here (e.g., featuredWorkshopTitleBannedWords)
			// but we don't have the full details yet. (We only query full details for actual featured workshop file.)

			bool wasDismissed = LocalNews.wasWorkshopItemDismissed(details.m_nPublishedFileId.m_PublishedFileId);
			if (wasDismissed)
				return false;

			queryFeaturedItem(details.m_nPublishedFileId);
			return true;
		}

		/// <summary>
		/// Nelson 2024-04-23: A concerned player raised the issue that mature content could potentially be returned in
		/// popular item results. Steam excludes certain mature content by default, but just in case, we check for these
		/// words and hide if contained in title.
		/// </summary>
		private static string[] featuredWorkshopTitleBannedWords = new string[]
		{
			"drug",
			"alcohol",
			"cigarette",
			"heroin",
			"cocaine",
		};

		/// <summary>
		/// Successfully queried popular workshop items.
		/// Tries to decide on an item that player has not dismissed.
		/// </summary>
		private static void handlePopularItemResults(SteamUGCQueryCompleted_t callback)
		{
#if !DEDICATED_SERVER
			UnturnedLog.info("Received popular workshop files");

			uint numResults = callback.m_unNumResultsReturned;
			if (numResults < 1)
			{
				UnturnedLog.warn("Popular workshop items response was empty");
				return;
			}

			MainMenuWorkshopPopularLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.popular;

			// Min in-case query returns less results than we want in the carousel (unlikely)
			int numCarouselResults = Mathf.Min((int) numResults, liveConfig.carouselItems);
			if (numCarouselResults > 0)
			{
				// List of indexes 0, 1, 2...
				List<uint> carouselIndexes = new List<uint>(numCarouselResults);
				for (uint index = 0; index < numCarouselResults; ++index)
					carouselIndexes.Add(index);

				// Randomly select popular indexes from the carousel.
				// This means that the most popular (carousel) items are randomly switched between,
				// rather than only showing the 1st most popular item. e.g. if the 2nd item is dismissed
				// then the 1st and 3rd items are randomly switched between.
				while (carouselIndexes.Count > 0)
				{
					int randomIndex = Random.Range(0, carouselIndexes.Count);
					uint carouselIndex = carouselIndexes[randomIndex];

					if (featurePopularItem(carouselIndex))
					{
						// Query was submitted, we can exit this function now.
						return;
					}

					carouselIndexes.RemoveAtFast(randomIndex);
				}
			}

			// All of the carousel items were dismissed, so now we proceed through returned items until we find a valid one.
			for (uint index = (uint) numCarouselResults; index < numResults; ++index)
			{
				if (featurePopularItem(index))
					return;
			}

			UnturnedLog.info("None of {0} popular workshop item(s) were eligible");
#endif // !DEDICATED_SERVER
		}

		private static void onPopularQueryCompleted(SteamUGCQueryCompleted_t callback, bool io)
		{
			if (io)
			{
				UnturnedLog.warn("IO error while querying popular workshop items");
			}
			else
			{
				if (callback.m_eResult == EResult.k_EResultOK)
				{
					handlePopularItemResults(callback);
				}
				else
				{
					UnturnedLog.warn("Error while querying popular workshop items: " + callback.m_eResult);
				}
			}
		}

		/// <summary>
		/// Response about the item we decided to display.
		/// </summary>
		private static void onFeaturedQueryCompleted(SteamUGCQueryCompleted_t callback, bool io)
		{
			if (io)
			{
				UnturnedLog.warn("IO error while querying featured workshop item");
			}
			else
			{
				if (callback.m_eResult == EResult.k_EResultOK)
				{
					UnturnedLog.info("Received workshop file details for news feed");
					try
					{
						spawnFeaturedWorkshopArticle();
					}
					catch (System.Exception exception)
					{
						UnturnedLog.warn("Workshop news article spawn failed!");
						UnturnedLog.exception(exception);
					}
				}
				else
				{
					UnturnedLog.warn("Error while querying featured workshop item: " + callback.m_eResult);
				}
			}
		}

#pragma warning disable
		private static CallResult<SteamUGCQueryCompleted_t> steamUGCQueryCompletedPopular;
		private static CallResult<SteamUGCQueryCompleted_t> steamUGCQueryCompletedFeatured;
#pragma warning restore
		private static void onSteamUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool io)
		{
			if (callback.m_handle == popularWorkshopHandle)
			{
				onPopularQueryCompleted(callback, io);

				SteamUGC.ReleaseQueryUGCRequest(popularWorkshopHandle);
				popularWorkshopHandle = UGCQueryHandle_t.Invalid;
			}
			else if (callback.m_handle == featuredWorkshopHandle)
			{
				onFeaturedQueryCompleted(callback, io);

				SteamUGC.ReleaseQueryUGCRequest(featuredWorkshopHandle);
				featuredWorkshopHandle = UGCQueryHandle_t.Invalid;
			}
		}

		protected static void queryFeaturedItem(PublishedFileId_t publishedFileID)
		{
			UnturnedLog.info("Requesting workshop file details for news feed ({0})", publishedFileID);

			if (featuredWorkshopHandle != UGCQueryHandle_t.Invalid)
			{
				SteamUGC.ReleaseQueryUGCRequest(featuredWorkshopHandle);
				featuredWorkshopHandle = UGCQueryHandle_t.Invalid;
			}

			PublishedFileId_t[] publishedFileIDs = new PublishedFileId_t[1];
			publishedFileIDs[0] = publishedFileID;
			featuredWorkshopHandle = SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIDs, 1);

			// We show the full description expandable.
			SteamUGC.SetReturnLongDescription(featuredWorkshopHandle, true);

			// Dependent items are needed for curated map + bundles separately.
			SteamUGC.SetReturnChildren(featuredWorkshopHandle, true);

#if UNITY_EDITOR
			// Disable cache in editor to work around visibility permissions issues.
			SteamUGC.SetAllowCachedResponse(featuredWorkshopHandle, 0);
#endif // UNITY_EDITOR

			SteamAPICall_t handle = SteamUGC.SendQueryUGCRequest(featuredWorkshopHandle);
			steamUGCQueryCompletedFeatured.Set(handle);
		}

		/// <summary>
		/// Submit query for recently trending popular workshop items.
		/// </summary>
		private static void queryPopularWorkshopItems()
		{
#if !DEDICATED_SERVER
			MainMenuWorkshopPopularLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop.popular;

			uint popularTrendDays = liveConfig.trendDays;
			if (popularTrendDays < 1 || liveConfig.carouselItems < 1)
			{
				UnturnedLog.warn("Not requesting popular workshop files for news feed");
				return;
			}
			else if (popularTrendDays > 180)
			{
				popularTrendDays = 180;
				UnturnedLog.warn("Clamping popular workshop trend days to {0}", popularTrendDays);
			}

			UnturnedLog.info("Requesting popular workshop files from the past {0} day(s) for news feed", popularTrendDays);
			popularWorkshopHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, Provider.APP_ID, Provider.APP_ID, 1);
			SteamUGC.SetRankedByTrendDays(popularWorkshopHandle, popularTrendDays);
			SteamUGC.SetReturnOnlyIDs(popularWorkshopHandle, true);
			SteamAPICall_t handle = SteamUGC.SendQueryUGCRequest(popularWorkshopHandle);
			steamUGCQueryCompletedPopular.Set(handle);
#endif // !DEDICATED_SERVER
		}

#if UNITY_STANDALONE_WIN && WITH_THIRDPARTYAC
		private void OnClickedThirdpartyAntiCheatButton(ISleekElement element)
		{
			ThirdpartyAntiCheat.OpenDirectory();
		}
#endif // UNITY_STANDALONE_WIN && WITH_THIRDPARTYAC

		private static bool CreateItemStoreSaleNews(ItemStore itemStore)
		{
			if (!Glazier.Get().SupportsAutomaticLayout)
			{
				// Nelson 2024-11-11: Main news feed isn't enabled in this case, so don't create sale news.
				// (public issue #4775)
				return false;
			}

			// If there are aren't at least this many listings it's not worthy of a sale promo.
			const int MIN_LISTINGS = 3;

			int[] availableListingIndices = itemStore.GetUnownedDiscountedBundleListingIndices();
			if (availableListingIndices == null || availableListingIndices.Length < MIN_LISTINGS)
			{
				return false;
			}

			List<int> filteredIndices = new List<int>(availableListingIndices);

			int[] excludedIndices = itemStore.GetExcludedListingIndices();
			if (excludedIndices != null)
			{
				foreach (int excludedIndex in excludedIndices)
				{
					filteredIndices.Remove(excludedIndex);
				}
			}

			for (int filterIndex = filteredIndices.Count - 1; filterIndex >= 0; --filterIndex)
			{
				int listingIndex = filteredIndices[filterIndex];
				int itemdefid = itemStore.GetListings()[listingIndex].itemdefid;
				if (!Provider.provider.economyService.IsItemEligibleForPromotion(itemdefid))
				{
					filteredIndices.RemoveAtFast(filterIndex);
				}
			}

			if (filteredIndices.Count < MIN_LISTINGS)
			{
				return false;
			}

			int layoutWidth = ScreenEx.GetWidthForLayout();
			int maxListings = Mathf.Max(MIN_LISTINGS, (layoutWidth - 450) / 200);

			List<int> randomIndices = new List<int>(maxListings);
			while (randomIndices.Count < maxListings && filteredIndices.Count > 0)
			{
				int removalIndex = filteredIndices.GetRandomIndex();
				randomIndices.Add(filteredIndices[removalIndex]);
				filteredIndices.RemoveAtFast(removalIndex);
			}

			itemStoreSaleNews = Glazier.Get().CreateBox();
			itemStoreSaleNews.SizeScale_X = 1.0f;
			itemStoreSaleNews.UseManualLayout = false;
			itemStoreSaleNews.UseChildAutoLayout = ESleekChildLayout.Vertical;
			itemStoreSaleNews.ChildAutoLayoutPadding = 5.0f;
			mainScrollView.AddChild(itemStoreSaleNews);
			ReviseNewsOrder();

			ISleekLabel titleLabel = Glazier.Get().CreateLabel();
			titleLabel.UseManualLayout = false;
			titleLabel.TextAlignment = TextAnchor.MiddleCenter;
			titleLabel.FontSize = ESleekFontSize.Large;
			itemStoreSaleNews.AddChild(titleLabel);

#if !DEDICATED_SERVER
			if (!string.IsNullOrEmpty(LiveConfig.Get().itemStore.saleTitle))
			{
				titleLabel.Text = LiveConfig.Get().itemStore.saleTitle;

				System.DateTime localStart = LiveConfig.Get().itemStore.saleStart.ToLocalTime();
				System.DateTime localEnd = LiveConfig.Get().itemStore.saleEnd.ToLocalTime();

				ISleekLabel saleWindowLabel = Glazier.Get().CreateLabel();
				saleWindowLabel.UseManualLayout = false;
				saleWindowLabel.TextAlignment = TextAnchor.MiddleLeft;
				saleWindowLabel.FontSize = ESleekFontSize.Medium;
				saleWindowLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				saleWindowLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
				saleWindowLabel.Text = ItemStoreMenu.instance.localization.format("SaleWindowFormat", localStart, localEnd);
				itemStoreSaleNews.AddChild(saleWindowLabel);
			}
			else
			{
				titleLabel.Text = ItemStoreMenu.instance.localization.format("DefaultSaleTitle");
			}
#endif // !DEDICATED_SERVER

			ISleekElement horizontalLayout = Glazier.Get().CreateFrame();
			horizontalLayout.UseManualLayout = false;
			horizontalLayout.UseChildAutoLayout = ESleekChildLayout.Horizontal;
			horizontalLayout.ChildAutoLayoutPadding = 5.0f;
			itemStoreSaleNews.AddChild(horizontalLayout);

			foreach (int listingIndex in randomIndices)
			{
				ItemStore.Listing listing = itemStore.GetListings()[listingIndex];
				SleekItemStoreListing button = new SleekItemStoreListing();
				button.UseManualLayout = false;
				button.UseWidthLayoutOverride = true;
				button.UseHeightLayoutOverride = true;
				button.SizeOffset_X = 200;
				button.SizeOffset_Y = 200;
				button.canShowAsInCart = false;
				button.SetListing(listing);
				horizontalLayout.AddChild(button);
			}

			return true;
		}

#if WITH_NOREDIST // Don't advertise items in U3-SDK.
		private static void OnPricesReceived()
		{
			if (hasCreatedItemStoreButton)
			{
				return;
			}
			hasCreatedItemStoreButton = true;

			ItemStore itemStore = ItemStore.Get();

			if (CreateItemStoreSaleNews(itemStore))
			{
				return;
			}

			ItemStore.Listing listing;
			int[] listingIndices;
			SleekItemStoreMainMenuButton.ELabelType labelType;

			if (itemStore.HasNewListings)
			{
				listingIndices = itemStore.GetNewListingIndices();
				labelType = SleekItemStoreMainMenuButton.ELabelType.New;
			}
			else if (ItemStore.Get().HasDiscountedListings)
			{
				listingIndices = itemStore.GetDiscountedListingIndices();
				labelType = SleekItemStoreMainMenuButton.ELabelType.Sale;
			}
			else if (ItemStore.Get().HasFeaturedListings && Random.value < 0.5f) // 50% chance of "featured" item.
			{
				listingIndices = itemStore.GetFeaturedListingIndices();
				labelType = SleekItemStoreMainMenuButton.ELabelType.None;
			}
			else
			{
				listingIndices = null;
				labelType = SleekItemStoreMainMenuButton.ELabelType.None;
			}

			List<int> filteredListingIndices;
			if (listingIndices != null)
			{
				filteredListingIndices = new List<int>(listingIndices);
			}
			else
			{
				// Randomly pick any purchasable item.
				ItemStore.Listing[] listings = itemStore.GetListings();
				int maxAttempts = Mathf.Min(5, listings.Length);
				filteredListingIndices = new List<int>(maxAttempts);
				for (int attemptIndex = 0; attemptIndex < maxAttempts; ++attemptIndex)
				{
					int randomIndex = listings.GetRandomIndex();
					if (!filteredListingIndices.Contains(randomIndex))
					{
						filteredListingIndices.Add(randomIndex);
					}
				}
			}

			int[] excludedIndices = itemStore.GetExcludedListingIndices();
			if (excludedIndices != null)
			{
				foreach (int excludedIndex in excludedIndices)
				{
					filteredListingIndices.Remove(excludedIndex);
				}
			}

			for (int filterIndex = filteredListingIndices.Count - 1; filterIndex >= 0; --filterIndex)
			{
				int listingIndex = filteredListingIndices[filterIndex];
				int itemdefid = itemStore.GetListings()[listingIndex].itemdefid;
				if (!Provider.provider.economyService.IsItemEligibleForPromotion(itemdefid))
				{
					filteredListingIndices.RemoveAtFast(filterIndex);
					continue;
				}

				System.Guid itemGuid = Provider.provider.economyService.getInventoryItemGuid(itemdefid);
				if (itemGuid != default)
				{
					ItemKeyAsset key = Assets.find<ItemKeyAsset>(itemGuid);
					if (key != null)
					{
						// Do not feature keys on the main menu because they are not useable without the associated box.
						// Boxes are OK to feature because they can be opened without a key, and country restrictions
						// have already been applied to the available listings so we know the player is allowed to open.
						filteredListingIndices.RemoveAtFast(filterIndex);
						continue;
					}
				}
			}

			if (filteredListingIndices.Count < 1)
			{
				// Every available item was removed.
				return;
			}

			int showListingIndex = filteredListingIndices.RandomOrDefault();
			listing = itemStore.GetListings()[showListingIndex];

			if (labelType == SleekItemStoreMainMenuButton.ELabelType.New
				&& ItemStoreSavedata.WasNewListingSeen(listing.itemdefid))
			{
				// New listing already seen, so disable label.
				labelType = SleekItemStoreMainMenuButton.ELabelType.None;
			}

			SleekItemStoreMainMenuButton itemButton = new SleekItemStoreMainMenuButton(listing, labelType);
			itemButton.PositionOffset_Y = 410;
			itemButton.SizeOffset_X = 200;
			itemButton.SizeOffset_Y = 50;
			container.AddChild(itemButton);
		}
#endif // WITH_NOREDIST

#if !DEDICATED_SERVER
		private static void PopulateAlertFromLiveConfig()
		{
			LiveConfigData liveConfig = LiveConfig.Get();
			if (string.IsNullOrEmpty(liveConfig.mainMenuAlert.header) || string.IsNullOrEmpty(liveConfig.mainMenuAlert.body))
			{
				// Alert might already be populated in which case we do not currently hide it.
				return;
			}

			long seenId;
			if (ConvenientSavedata.get().read("MainMenuAlertSeenId", out seenId) && seenId >= liveConfig.mainMenuAlert.id)
			{
				// Alert has been dismissed by the player.
				return;
			}

			bool bypassTimeWindow = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (LiveConfig.useEditorLiveConfig)
			{
				bypassTimeWindow = true;
			}
#endif // !UNITY_EDITOR || DEVELOPMENT_BUILD

			if (liveConfig.mainMenuAlert.useTimeWindow && !bypassTimeWindow)
			{
				System.DateTime now = System.DateTime.UtcNow;
				if (now < liveConfig.mainMenuAlert.startTime || now > liveConfig.mainMenuAlert.endTime)
				{
					return;
				}
			}

			if (alertBox == null)
			{
				alertBox = Glazier.Get().CreateButton();
				alertBox.OnClicked += onClickedAlertButton;
				alertBox.PositionOffset_X = 210;
				alertBox.PositionOffset_Y = mainHeaderOffset;
				alertBox.SizeOffset_Y = 60;
				alertBox.SizeOffset_X = -210;
				alertBox.SizeScale_X = 1f;
				container.AddChild(alertBox);

				alertImage = Glazier.Get().CreateImage();
				alertImage.PositionOffset_X = 10;
				alertImage.PositionOffset_Y = 10;
				alertImage.SizeOffset_X = 40;
				alertImage.SizeOffset_Y = 40;
				alertImage.IsVisible = false;
				alertBox.AddChild(alertImage);

				alertWebImage = new SleekWebImage();
				alertWebImage.PositionOffset_X = 10;
				alertWebImage.PositionOffset_Y = 10;
				alertWebImage.SizeOffset_X = 40;
				alertWebImage.SizeOffset_Y = 40;
				alertWebImage.IsVisible = false;
				alertBox.AddChild(alertWebImage);

				alertHeaderLabel = Glazier.Get().CreateLabel();
				alertHeaderLabel.PositionOffset_X = 60;
				alertHeaderLabel.SizeScale_X = 1;
				alertHeaderLabel.SizeOffset_X = -70;
				alertHeaderLabel.SizeOffset_Y = 30;
				alertHeaderLabel.TextAlignment = TextAnchor.MiddleLeft;
				alertHeaderLabel.FontSize = ESleekFontSize.Medium;
				alertHeaderLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				alertBox.AddChild(alertHeaderLabel);

				alertLinkLabel = Glazier.Get().CreateLabel();
				alertLinkLabel.PositionOffset_X = 60;
				alertLinkLabel.SizeScale_X = 1;
				alertLinkLabel.SizeOffset_X = -70;
				alertLinkLabel.SizeOffset_Y = 30;
				alertLinkLabel.TextAlignment = TextAnchor.MiddleRight;
				alertLinkLabel.FontSize = ESleekFontSize.Small;
				alertLinkLabel.IsVisible = false;
				alertBox.AddChild(alertLinkLabel);

				dismissAlertLabel = Glazier.Get().CreateLabel();
				dismissAlertLabel.PositionScale_Y = 1.0f;
				dismissAlertLabel.PositionOffset_X = 60;
				dismissAlertLabel.PositionOffset_Y = -35;
				dismissAlertLabel.SizeScale_X = 1;
				dismissAlertLabel.SizeOffset_X = -65;
				dismissAlertLabel.SizeOffset_Y = 30;
				dismissAlertLabel.TextAlignment = TextAnchor.LowerRight;
				dismissAlertLabel.FontSize = ESleekFontSize.Small;
				dismissAlertLabel.Text = localization.format("Featured_Workshop_Dismiss");
				alertBox.AddChild(dismissAlertLabel);

				alertBodyLabel = Glazier.Get().CreateLabel();
				alertBodyLabel.PositionOffset_X = 60;
				alertBodyLabel.PositionOffset_Y = 20;
				alertBodyLabel.SizeOffset_X = -70;
				alertBodyLabel.SizeOffset_Y = -20;
				alertBodyLabel.SizeScale_X = 1;
				alertBodyLabel.SizeScale_Y = 1;
				alertBodyLabel.TextAlignment = TextAnchor.UpperLeft;
				alertBodyLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				alertBodyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				alertBodyLabel.AllowRichText = true;
				alertBox.AddChild(alertBodyLabel);

				mainHeaderOffset += alertBox.SizeOffset_Y + 10;
				mainScrollView.PositionOffset_Y += alertBox.SizeOffset_Y + 10;
				mainScrollView.SizeOffset_Y -= (alertBox.SizeOffset_Y + 10);
			}

			Color color = Palette.hex(liveConfig.mainMenuAlert.color);
			alertHeaderLabel.Text = liveConfig.mainMenuAlert.header;
			alertHeaderLabel.TextColor = color;
			alertBodyLabel.Text = liveConfig.mainMenuAlert.body;
			dismissAlertLabel.TextColor = color * 0.5f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (LiveConfig.useEditorLiveConfig && liveConfig.mainMenuAlert.useTimeWindow)
			{
				alertHeaderLabel.Text += $" ({liveConfig.mainMenuAlert.startTime.ToLocalTime()} - {liveConfig.mainMenuAlert.endTime.ToLocalTime()})";
			}
#endif // !UNITY_EDITOR || DEVELOPMENT_BUILD

			if (!string.IsNullOrEmpty(liveConfig.mainMenuAlert.iconName))
			{
				alertImage.Texture = icons.load<Texture2D>(liveConfig.mainMenuAlert.iconName);
				alertImage.IsVisible = true;
			}
			else
			{
				alertImage.IsVisible = false;
			}

			if (!string.IsNullOrEmpty(liveConfig.mainMenuAlert.iconURL))
			{
				alertWebImage.Refresh(liveConfig.mainMenuAlert.iconURL);
				alertWebImage.IsVisible = true;
			}
			else
			{
				alertWebImage.IsVisible = false;
			}

			Color iconTintColor = liveConfig.mainMenuAlert.shouldTintIcon ? color : Color.white;
			alertImage.TintColor = iconTintColor;
			alertWebImage.color = iconTintColor;

			if (!string.IsNullOrEmpty(liveConfig.mainMenuAlert.link))
			{
				alertLinkLabel.Text = liveConfig.mainMenuAlert.link;
				alertLinkLabel.TextColor = color * 0.5f;
				alertLinkLabel.IsVisible = true;
				dismissAlertLabel.IsVisible = false;
			}
			else
			{
				alertLinkLabel.IsVisible = false;
				dismissAlertLabel.IsVisible = true;
			}
		}

		/// <summary>
		/// Entry point to deciding which workshop item is featured above recent announcements.
		/// </summary>
		private static void UpdateWorkshopFromLiveConfig()
		{
			if (!Glazier.Get().SupportsAutomaticLayout)
			{
				// Cannot show articles without automatic layout. An explanation will be shown instead.
				return;
			}

			MainMenuWorkshopLiveConfig liveConfig = LiveConfig.Get().mainMenuWorkshop;
			if (!liveConfig.allowNews)
			{
				// Either disabled, or live config did not actually load yet.
				return;
			}

			if (!Steamworks.SteamUser.BLoggedOn())
			{
				// Don't submit workshop queries in offline mode.
				return;
			}

			if (hasBegunQueryingLiveConfigWorkshop)
			{
				// Already started querying a file.
				return;
			}
			hasBegunQueryingLiveConfigWorkshop = true;

			ulong explicitlyFeaturedId = 0;
			if (liveConfig.featured.fileIds != null && liveConfig.featured.fileIds.Length > 0)
			{
				if (liveConfig.featured.IsNowFeaturedTimeOrBypassed())
				{
					explicitlyFeaturedId = liveConfig.featured.fileIds.RandomOrDefault();
				}
			}

			if (explicitlyFeaturedId > 0)
			{
				// We allow player to dismiss explicitly featured item, but they will see it at least once.
				// This prevents it from becoming annoying if they've already subscribed, and falls back to the usual popular items.
				if (!LocalNews.wasWorkshopItemDismissed(explicitlyFeaturedId))
				{
					queryFeaturedItem((PublishedFileId_t) explicitlyFeaturedId);
					return;
				}
			}

			// Above case early-exits if the query was performed, otherwise we fallback to popular items.
			// Explicitly featured (e.g. curated) ignore this featuredWorkshop option, but popular/trending respects it.
			if (OptionsSettings.featuredWorkshop)
			{
				queryPopularWorkshopItems();
			}
		}

		private static void OnLiveConfigRefreshed()
		{
			PopulateAlertFromLiveConfig();
			UpdateWorkshopFromLiveConfig();
		}

		/// <summary>
		/// Ensures workshop files are not refreshed more than once per main menu load.
		/// </summary>
		private static bool hasBegunQueryingLiveConfigWorkshop;
#endif // !DEDICATED_SERVER

		public void OnDestroy()
		{
#if WITH_NOREDIST
			ItemStore.Get().OnPricesReceived -= OnPricesReceived;
#endif
#if !DEDICATED_SERVER
			LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
#endif // !DEDICATED_SERVER
			playUI.OnDestroy();
			survivorsUI.OnDestroy();
			workshopUI.OnDestroy();
		}

		public MenuDashboardUI()
		{
			localization = Localization.read("/Menu/MenuDashboard.dat");
			SDG.NetTransport.TransportBase.OnGetMessage = localization.format;
			icons = Bundles.getIconsBundle("UI/Menu/Icons/MenuDashboard");

			// This is messy but MenuUI does not have its own translation file.
			MenuUI.copyNotificationButton.icon = icons.load<Texture2D>("Clipboard");
			MenuUI.copyNotificationButton.text = localization.format("Copy_Notification_Label");
			MenuUI.copyNotificationButton.tooltip = localization.format("Copy_Notification_Tooltip");
			MenuUI.markContentCorruptButton.icon = icons.load<Texture2D>("MarkContentCorrupt");
			MenuUI.markContentCorruptButton.text = localization.format("MarkContentCorrupt_Label");
			MenuUI.markContentCorruptButton.tooltip = localization.format("MarkContentCorrupt_Tooltip");

			newAnnouncement = null;
			workshopBox = null;
			itemStoreSaleNews = null;

			if (Steamworks.SteamUser.BLoggedOn())
			{
				// Only check base game version if this is not a modded build.
				if (Provider.GetModInfo() == null)
				{
					MenuUI.instance.StartCoroutine(MenuUI.instance.CheckForUpdates(OnUpdateDetected));
				}

				if (steamUGCQueryCompletedPopular == null)
					steamUGCQueryCompletedPopular = CallResult<SteamUGCQueryCompleted_t>.Create(onSteamUGCQueryCompleted);
				if (steamUGCQueryCompletedFeatured == null)
					steamUGCQueryCompletedFeatured = CallResult<SteamUGCQueryCompleted_t>.Create(onSteamUGCQueryCompleted);

				if (popularWorkshopHandle != UGCQueryHandle_t.Invalid)
				{
					SteamUGC.ReleaseQueryUGCRequest(popularWorkshopHandle);
					popularWorkshopHandle = UGCQueryHandle_t.Invalid;
				}
			}

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = true;

			playButton = new SleekButtonIcon(icons.load<Texture2D>("Play"));
			playButton.PositionOffset_Y = 170;
			playButton.SizeOffset_X = 200;
			playButton.SizeOffset_Y = 50;
			playButton.text = localization.format("PlayButtonText");
			playButton.tooltip = localization.format("PlayButtonTooltip");
			playButton.onClickedButton += onClickedPlayButton;
			playButton.fontSize = ESleekFontSize.Medium;
			playButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(playButton);

			survivorsButton = new SleekButtonIcon(icons.load<Texture2D>("Survivors"));
			survivorsButton.PositionOffset_Y = 230;
			survivorsButton.SizeOffset_X = 200;
			survivorsButton.SizeOffset_Y = 50;
			survivorsButton.text = localization.format("SurvivorsButtonText");
			survivorsButton.tooltip = localization.format("SurvivorsButtonTooltip");
			survivorsButton.onClickedButton += onClickedSurvivorsButton;
			survivorsButton.fontSize = ESleekFontSize.Medium;
			survivorsButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(survivorsButton);

			configurationButton = new SleekButtonIcon(icons.load<Texture2D>("Configuration"));
			configurationButton.PositionOffset_Y = 290;
			configurationButton.SizeOffset_X = 200;
			configurationButton.SizeOffset_Y = 50;
			configurationButton.text = localization.format("ConfigurationButtonText");
			configurationButton.tooltip = localization.format("ConfigurationButtonTooltip");
			configurationButton.onClickedButton += onClickedConfigurationButton;
			configurationButton.fontSize = ESleekFontSize.Medium;
			configurationButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(configurationButton);

			workshopButton = new SleekButtonIcon(icons.load<Texture2D>("Workshop"));
			workshopButton.PositionOffset_Y = 350;
			workshopButton.SizeOffset_X = 200;
			workshopButton.SizeOffset_Y = 50;
			workshopButton.text = localization.format("WorkshopButtonText");
			workshopButton.tooltip = localization.format("WorkshopButtonTooltip");
			workshopButton.onClickedButton += onClickedWorkshopButton;
			workshopButton.fontSize = ESleekFontSize.Medium;
			workshopButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(workshopButton);

			exitButton = new SleekButtonIcon(icons.load<Texture2D>("Exit"));
			exitButton.PositionOffset_Y = -50;
			exitButton.PositionScale_Y = 1f;
			exitButton.SizeOffset_X = 200;
			exitButton.SizeOffset_Y = 50;
			exitButton.text = localization.format("ExitButtonText");
			exitButton.tooltip = localization.format("ExitButtonTooltip");
			exitButton.onClickedButton += onClickedExitButton;
			exitButton.fontSize = ESleekFontSize.Medium;
			exitButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(exitButton);

			mainScrollView = Glazier.Get().CreateScrollView();
			mainScrollView.PositionOffset_X = 210;
			mainScrollView.PositionOffset_Y = 170;
			mainScrollView.SizeScale_X = 1.0f;
			mainScrollView.SizeScale_Y = 1.0f;
			mainScrollView.SizeOffset_X = -210;
			mainScrollView.SizeOffset_Y = -170;
			mainScrollView.ScaleContentToWidth = true;
			container.AddChild(mainScrollView);

			if (!Glazier.Get().SupportsAutomaticLayout)
			{
				ISleekLabel explanationLabel = Glazier.Get().CreateLabel();
				explanationLabel.Text = "Featured workshop file and news feed are no longer supported when using the -Glazier=IMGUI launch option.";
				explanationLabel.FontSize = ESleekFontSize.Large;
				explanationLabel.SizeScale_X = 1.0f;
				explanationLabel.SizeOffset_Y = 100;
				mainScrollView.ContentSizeOffset = new Vector2(0.0f, 100);
				mainScrollView.AddChild(explanationLabel);
			}
			else
			{
				mainScrollView.ContentUseManualLayout = false;
			}

			if (!Provider.isPro)
			{
				proButton = Glazier.Get().CreateButton();
				proButton.PositionOffset_X = 210;
				proButton.PositionOffset_Y = -100;
				proButton.PositionScale_Y = 1;
				proButton.SizeOffset_Y = 100;
				proButton.SizeOffset_X = -210;
				proButton.SizeScale_X = 1f;
				proButton.TooltipText = localization.format("Pro_Button_Tooltip");
				proButton.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
				proButton.TextColor = Palette.PRO;
				proButton.OnClicked += onClickedProButton;
				container.AddChild(proButton);

				proLabel = Glazier.Get().CreateLabel();
				proLabel.SizeScale_X = 1;
				proLabel.SizeOffset_Y = 50;
				proLabel.Text = localization.format("Pro_Title");
				proLabel.TextColor = Palette.PRO;
				proLabel.FontSize = ESleekFontSize.Large;
				proButton.AddChild(proLabel);

				featureLabel = Glazier.Get().CreateLabel();
				featureLabel.PositionOffset_Y = 50;
				featureLabel.SizeOffset_Y = -50;
				featureLabel.SizeScale_X = 1;
				featureLabel.SizeScale_Y = 1;
				featureLabel.Text = localization.format("Pro_Button");
				featureLabel.TextColor = Palette.PRO;
				proButton.AddChild(featureLabel);

				mainScrollView.SizeOffset_Y -= 110;
			}

			mainHeaderOffset = 170;
			alertBox = null;
#if WITH_NOREDIST // Don't advertise items in U3-SDK.
			hasCreatedItemStoreButton = false;
#endif

			// Warning on 32-bit Windows that it cannot join multiplayer.
#if !UNITY_64
			ISleekBox architectureBox = Glazier.Get().CreateBox();
			architectureBox.PositionOffset_X = 210;
			architectureBox.PositionOffset_Y = mainHeaderOffset;
			architectureBox.SizeOffset_Y = 200;
			architectureBox.SizeOffset_X = -210;
			architectureBox.SizeScale_X = 1.0f;
			container.AddChild(architectureBox);
			mainHeaderOffset += architectureBox.SizeOffset_Y + 10;
			mainScrollView.PositionOffset_Y += architectureBox.SizeOffset_Y + 10;
			mainScrollView.SizeOffset_Y -= (architectureBox.SizeOffset_Y + 10);

			ISleekLabel architectureLabel = Glazier.Get().CreateLabel();
			architectureLabel.PositionOffset_X = 20;
			architectureLabel.PositionOffset_Y = 20;
			architectureLabel.SizeOffset_X = -20;
			architectureLabel.SizeOffset_Y = -20;
			architectureLabel.SizeScale_X = 1.0f;
			architectureLabel.SizeScale_Y = 1.0f;
			architectureLabel.Text = "Sorry, 32-bit Windows is no longer supported in multiplayer. :(\nYou can however host and play multiplayer from the archived 32-bit Windows compatibility version:\n1. Right-click Unturned in your Steam Library\n2. Select Properties... > Betas\n3. From the dropdown select \"32bit-windows\"";
			architectureLabel.FontSize = ESleekFontSize.Large;
			architectureLabel.TextAlignment = TextAnchor.MiddleLeft;
			architectureBox.AddChild(architectureLabel);
#endif // !UNITY_64

			string betaName;
			if (SteamApps.GetCurrentBetaName(out betaName, 64) && string.Equals(betaName, "preview", System.StringComparison.InvariantCultureIgnoreCase))
			{
				CreatePreviewBranchChangelogButton();
			}

#if UNITY_STANDALONE_WIN && WITH_THIRDPARTYAC
			if (!Dedicator.hasThirdpartyAntiCheat)
			{
				ISleekButton tpacButton = Glazier.Get().CreateButton();
				tpacButton.PositionOffset_X = 210;
				tpacButton.PositionOffset_Y = mainHeaderOffset;
				tpacButton.SizeOffset_Y = 60;
				tpacButton.SizeOffset_X = -210;
				tpacButton.SizeScale_X = 1f;
				tpacButton.OnClicked += OnClickedThirdpartyAntiCheatButton;
				container.AddChild(tpacButton);

				ISleekImage tpacIcon = Glazier.Get().CreateImage();
				tpacIcon.PositionOffset_X = 10;
				tpacIcon.PositionOffset_Y = 10;
				tpacIcon.SizeOffset_X = 40;
				tpacIcon.SizeOffset_Y = 40;
				tpacIcon.Texture = icons.load<Texture2D>(ThirdpartyAntiCheat.IconName);
				tpacButton.AddChild(tpacIcon);

				ISleekLabel headerLabel = Glazier.Get().CreateLabel();
				headerLabel.PositionOffset_X = 60;
				headerLabel.SizeScale_X = 1;
				headerLabel.SizeOffset_X = -60;
				headerLabel.SizeOffset_Y = 30;
				headerLabel.Text = localization.format(ThirdpartyAntiCheat.MenuHeaderKey);
				headerLabel.FontSize = ESleekFontSize.Medium;
				tpacButton.AddChild(headerLabel);

				ISleekLabel bodyLabel = Glazier.Get().CreateLabel();
				bodyLabel.PositionOffset_X = 60;
				bodyLabel.PositionOffset_Y = 20;
				bodyLabel.SizeOffset_X = -60;
				bodyLabel.SizeOffset_Y = -20;
				bodyLabel.SizeScale_X = 1;
				bodyLabel.SizeScale_Y = 1;
				bodyLabel.Text = localization.format(ThirdpartyAntiCheat.MenuBodyKey);
				tpacButton.AddChild(bodyLabel);

				mainHeaderOffset += tpacButton.SizeOffset_Y + 10;
				mainScrollView.PositionOffset_Y += tpacButton.SizeOffset_Y + 10;
				mainScrollView.SizeOffset_Y -= (tpacButton.SizeOffset_Y + 10);
			}
#endif // UNITY_STANDALONE_WIN && WITH_THIRDPARTYAC

#if WITH_NOREDIST // Don't advertise items in U3-SDK.
			// Bind before creating item store menu.
			ItemStore.Get().OnPricesReceived += OnPricesReceived;
#endif // WITH_NOREDIST

#if !DEDICATED_SERVER
			hasBegunQueryingLiveConfigWorkshop = false;
			LiveConfig.OnRefreshed += OnLiveConfigRefreshed;
			OnLiveConfigRefreshed();
#endif // !DEDICATED_SERVER

			pauseUI = new MenuPauseUI();
			creditsUI = new MenuCreditsUI();
			titleUI = new MenuTitleUI();
			playUI = new MenuPlayUI();
			survivorsUI = new MenuSurvivorsUI();
			configUI = new MenuConfigurationUI();
			workshopUI = new MenuWorkshopUI();

			if (Provider.connectionFailureInfo != ESteamConnectionFailureInfo.NONE)
			{
				ESteamConnectionFailureInfo info = Provider.connectionFailureInfo;
				string reason = Provider.connectionFailureReason;
				uint duration = Provider.connectionFailureDuration;

				// Number of workshop items the server was unauthorized to download.
				int invalidItemCount = Provider.provider.workshopService.serverInvalidItemsCount;

				Provider.resetConnectionFailure();
				Provider.provider.workshopService.resetServerInvalidItems();

				if (invalidItemCount > 0)
				{
					// Is the connection failure related to missing assets?
					// We know the server is not authorized to download some items, so the missing asset is likely caused by that.
					bool isMissingItemRelated;

					switch (info)
					{
						case ESteamConnectionFailureInfo.BARRICADE:
						case ESteamConnectionFailureInfo.STRUCTURE:
						case ESteamConnectionFailureInfo.VEHICLE:
						case ESteamConnectionFailureInfo.MAP:
						case ESteamConnectionFailureInfo.HASH_LEVEL:
							isMissingItemRelated = true;
							break;

						default:
							isMissingItemRelated = false;
							break;
					}

					if (isMissingItemRelated)
					{
						UnturnedLog.info("Connection failure {0} is asset related and therefore probably caused by the {1} download-restricted workshop item(s)", info, invalidItemCount);
						info = ESteamConnectionFailureInfo.WORKSHOP_DOWNLOAD_RESTRICTION;
					}
				}

				string formattedLabel = null;
				bool shouldVerifyGameFiles = false;

				switch (info)
				{
					case ESteamConnectionFailureInfo.BANNED:
						formattedLabel = localization.format("Banned", duration, reason);
						break;
					case ESteamConnectionFailureInfo.KICKED:
						formattedLabel = localization.format("Kicked", reason);
						break;
					case ESteamConnectionFailureInfo.WHITELISTED:
						formattedLabel = localization.format("Whitelisted");
						break;
					case ESteamConnectionFailureInfo.PASSWORD:
						formattedLabel = localization.format("Password");
						break;
					case ESteamConnectionFailureInfo.FULL:
						formattedLabel = localization.format("Full");
						break;
					case ESteamConnectionFailureInfo.HASH_LEVEL:
						formattedLabel = localization.format("Hash_Level");
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.HASH_ASSEMBLY:
						formattedLabel = localization.format("Hash_Assembly");
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.VERSION:
						formattedLabel = localization.format("Version", reason, Provider.APP_VERSION);
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.PRO_SERVER:
						formattedLabel = localization.format("Pro_Server");
						break;
					case ESteamConnectionFailureInfo.PRO_CHARACTER:
						formattedLabel = localization.format("Pro_Character");
						break;
					case ESteamConnectionFailureInfo.PRO_DESYNC:
						formattedLabel = localization.format("Pro_Desync");
						break;
					case ESteamConnectionFailureInfo.PRO_APPEARANCE:
						formattedLabel = localization.format("Pro_Appearance");
						break;
					case ESteamConnectionFailureInfo.AUTH_VERIFICATION:
						formattedLabel = localization.format("Auth_Verification");
						break;
					case ESteamConnectionFailureInfo.AUTH_NO_STEAM:
						formattedLabel = localization.format("Auth_No_Steam");
						break;
					case ESteamConnectionFailureInfo.AUTH_LICENSE_EXPIRED:
						formattedLabel = localization.format("Auth_License_Expired");
						break;
					case ESteamConnectionFailureInfo.AUTH_VAC_BAN:
						formattedLabel = localization.format("Auth_VAC_Ban");
						break;
					case ESteamConnectionFailureInfo.AUTH_ELSEWHERE:
						formattedLabel = localization.format("Auth_Elsewhere");
						break;
					case ESteamConnectionFailureInfo.AUTH_TIMED_OUT:
						formattedLabel = localization.format("Auth_Timed_Out");
						break;
					case ESteamConnectionFailureInfo.AUTH_USED:
						formattedLabel = localization.format("Auth_Used");
						break;
					case ESteamConnectionFailureInfo.AUTH_NO_USER:
						formattedLabel = localization.format("Auth_No_User");
						break;
					case ESteamConnectionFailureInfo.AUTH_PUB_BAN:
						formattedLabel = localization.format("Auth_Pub_Ban");
						break;
					case ESteamConnectionFailureInfo.AUTH_NETWORK_IDENTITY_FAILURE:
						formattedLabel = localization.format("Auth_Network_Identity_Failure");
						break;
					case ESteamConnectionFailureInfo.AUTH_ECON_SERIALIZE:
						formattedLabel = localization.format("Auth_Econ_Serialize");
						break;
					case ESteamConnectionFailureInfo.AUTH_ECON_DESERIALIZE:
						formattedLabel = localization.format("Auth_Econ_Deserialize");
						break;
					case ESteamConnectionFailureInfo.AUTH_ECON_VERIFY:
						formattedLabel = localization.format("Auth_Econ_Verify");
						break;
					case ESteamConnectionFailureInfo.AUTH_EMPTY:
						formattedLabel = localization.format("Auth_Empty");
						break;
					case ESteamConnectionFailureInfo.ALREADY_CONNECTED:
						formattedLabel = localization.format("Already_Connected");
						break;
					case ESteamConnectionFailureInfo.ALREADY_PENDING:
						formattedLabel = localization.format("Already_Pending");
						break;
					case ESteamConnectionFailureInfo.LATE_PENDING:
						formattedLabel = localization.format("Late_Pending");
						break;
					case ESteamConnectionFailureInfo.NOT_PENDING:
						formattedLabel = localization.format("Not_Pending");
						break;
					case ESteamConnectionFailureInfo.NAME_PLAYER_SHORT:
						formattedLabel = localization.format("Name_Player_Short");
						break;
					case ESteamConnectionFailureInfo.NAME_PLAYER_LONG:
						formattedLabel = localization.format("Name_Player_Long");
						break;
					case ESteamConnectionFailureInfo.NAME_PLAYER_INVALID:
						formattedLabel = localization.format("Name_Player_Invalid");
						break;
					case ESteamConnectionFailureInfo.NAME_PLAYER_NUMBER:
						formattedLabel = localization.format("Name_Player_Number");
						break;
					case ESteamConnectionFailureInfo.NAME_CHARACTER_SHORT:
						formattedLabel = localization.format("Name_Character_Short");
						break;
					case ESteamConnectionFailureInfo.NAME_CHARACTER_LONG:
						formattedLabel = localization.format("Name_Character_Long");
						break;
					case ESteamConnectionFailureInfo.NAME_CHARACTER_INVALID:
						formattedLabel = localization.format("Name_Character_Invalid");
						break;
					case ESteamConnectionFailureInfo.NAME_CHARACTER_NUMBER:
						formattedLabel = localization.format("Name_Character_Number");
						break;
					case ESteamConnectionFailureInfo.TIMED_OUT:
						formattedLabel = localization.format("Timed_Out");
						break;
					case ESteamConnectionFailureInfo.TIMED_OUT_LOGIN:
						formattedLabel = localization.format("Timed_Out_Login");
						break;
					case ESteamConnectionFailureInfo.MAP:
						formattedLabel = localization.format("Map");
						break;
					case ESteamConnectionFailureInfo.SHUTDOWN:
						formattedLabel = string.IsNullOrEmpty(reason) ? localization.format("Shutdown") : localization.format("Shutdown_Reason", reason);
						break;
					case ESteamConnectionFailureInfo.PING:
						formattedLabel = reason; // Hack, this gets formatted by Provider.
						break;
					case ESteamConnectionFailureInfo.PLUGIN:
						formattedLabel = string.IsNullOrEmpty(reason) ? localization.format("Plugin") : localization.format("Plugin_Reason", reason);
						break;
					case ESteamConnectionFailureInfo.BARRICADE:
						formattedLabel = localization.format("Barricade", reason);
						break;
					case ESteamConnectionFailureInfo.STRUCTURE:
						formattedLabel = localization.format("Structure", reason);
						break;
					case ESteamConnectionFailureInfo.VEHICLE:
						formattedLabel = localization.format("Vehicle", reason);
						break;
					case ESteamConnectionFailureInfo.CLIENT_MODULE_DESYNC:
						formattedLabel = localization.format("Client_Module_Desync");
						break;
					case ESteamConnectionFailureInfo.SERVER_MODULE_DESYNC:
						formattedLabel = localization.format("Server_Module_Desync");
						break;
#if WITH_THIRDPARTYAC
					case ESteamConnectionFailureInfo.THIRDPARTYAC_BROKEN:
						formattedLabel = localization.format(ThirdpartyAntiCheat.DisconnectBrokenKey);
						break;
					case ESteamConnectionFailureInfo.THIRDPARTYAC_UPDATE:
						formattedLabel = localization.format(ThirdpartyAntiCheat.DisconnectUpdateKey);
						break;
					case ESteamConnectionFailureInfo.THIRDPARTYAC_UNKNOWN:
						formattedLabel = localization.format(ThirdpartyAntiCheat.DisconnectUnknownKey);
						break;
#endif // WITH_THIRDPARTYAC
					case ESteamConnectionFailureInfo.LEVEL_VERSION:
						formattedLabel = reason; // Hack, this gets formatted by Provider.
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.ECON_HASH:
						formattedLabel = localization.format("Econ_Hash");
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.HASH_MASTER_BUNDLE:
						formattedLabel = localization.format("Master_Bundle_Hash", reason);
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.REJECT_UNKNOWN:
						formattedLabel = localization.format("Reject_Unknown", reason);
						break;
					case ESteamConnectionFailureInfo.WORKSHOP_DOWNLOAD_RESTRICTION:
						formattedLabel = localization.format("Workshop_Download_Restriction", invalidItemCount);
						break;

					case ESteamConnectionFailureInfo.WORKSHOP_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format("Workshop_Advertisement_Mismatch");
						break;

					case ESteamConnectionFailureInfo.CUSTOM:
						formattedLabel = reason;
						break;
					case ESteamConnectionFailureInfo.CUSTOM_SHOULD_VERIFY_GAME_FILES:
						formattedLabel = reason;
						shouldVerifyGameFiles = true;
						break;

					case ESteamConnectionFailureInfo.LATE_PENDING_STEAM_AUTH:
						formattedLabel = localization.format("Late_Pending_Steam_Auth");
						break;

					case ESteamConnectionFailureInfo.LATE_PENDING_STEAM_ECON:
						formattedLabel = localization.format("Late_Pending_Steam_Econ");
						break;

					case ESteamConnectionFailureInfo.LATE_PENDING_STEAM_GROUPS:
						formattedLabel = localization.format("Late_Pending_Steam_Groups");
						break;

					case ESteamConnectionFailureInfo.NAME_PRIVATE_LONG:
						formattedLabel = localization.format("Name_Private_Long");
						break;
					case ESteamConnectionFailureInfo.NAME_PRIVATE_INVALID:
						formattedLabel = localization.format("Name_Private_Invalid");
						break;
					case ESteamConnectionFailureInfo.NAME_PRIVATE_NUMBER:
						formattedLabel = localization.format("Name_Private_Number");
						break;
					case ESteamConnectionFailureInfo.HASH_RESOURCES:
						formattedLabel = localization.format("Hash_Resources");
						shouldVerifyGameFiles = true;
						break;
					case ESteamConnectionFailureInfo.SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR:
						formattedLabel = localization.format("SkinColorWithinThresholdOfTerrainColor");
						break;
					case ESteamConnectionFailureInfo.STEAM_ID_MISMATCH:
						formattedLabel = localization.format("Steam_ID_Mismatch");
						break;
					case ESteamConnectionFailureInfo.CONNECT_RATE_LIMITING:
						formattedLabel = localization.format("Connect_Rate_Limiting");
						break;
					case ESteamConnectionFailureInfo.BAD_PACKET_RATE_LIMITING:
						formattedLabel = localization.format("Bad_Packet_Rate_Limiting");
						break;
					case ESteamConnectionFailureInfo.TOO_MANY_CLIENTS_WITH_SAME_IP_ADDRESS:
						formattedLabel = localization.format("Too_Many_Clients_With_Same_IP_Address");
						break;

					case ESteamConnectionFailureInfo.SERVER_MAP_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format("Server_Map_Advertisement_Mismatch");
						break;
					case ESteamConnectionFailureInfo.SERVER_VAC_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format("Server_VAC_Advertisement_Mismatch");
						break;
#if WITH_THIRDPARTYAC
					case ESteamConnectionFailureInfo.SERVER_THIRDPARTYAC_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format(ThirdpartyAntiCheat.AdvertisementMismatchKey);
						break;
#endif // WITH_THIRDPARTYAC
					case ESteamConnectionFailureInfo.SERVER_MAXPLAYERS_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format("Server_MaxPlayers_Advertisement_Mismatch");
						break;
					case ESteamConnectionFailureInfo.SERVER_CAMERAMODE_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format("Server_CameraMode_Advertisement_Mismatch");
						break;
					case ESteamConnectionFailureInfo.SERVER_PVP_ADVERTISEMENT_MISMATCH:
						formattedLabel = localization.format("Server_PvP_Advertisement_Mismatch");
						break;
					case ESteamConnectionFailureInfo.HWID_MODIFIED:
						formattedLabel = localization.format("HWID_Modified");
						break;

					case ESteamConnectionFailureInfo.MOD_NAME_MISMATCH:
						if (Provider._modInfo != null)
						{
							formattedLabel = localization.format("Mod_Name_Mismatch", reason, Provider._modInfo.Name);
						}
						else
						{
							formattedLabel = localization.format("Mod_Name_Server", reason);
						}
						break;

					case ESteamConnectionFailureInfo.MOD_VERSION_MISMATCH:
						formattedLabel = localization.format("Mod_Version_Mismatch", reason, Provider._modInfo?.Name ?? "???", Provider._modInfo?.FormatModVersion() ?? "???");
						shouldVerifyGameFiles = true;
						break;

					default:
						formattedLabel = localization.format("Failure_Unknown", info, reason);
						break;
				}

				if (string.IsNullOrEmpty(formattedLabel))
				{
					formattedLabel = string.Format("Error: {0} Reason: {1}", info, reason);
				}

				MenuUI.alert(formattedLabel);
				UnturnedLog.info(formattedLabel);

				if (shouldVerifyGameFiles)
				{
					UnturnedLog.info("Showing option to verify game files");
					MenuUI.markContentCorruptButton.IsVisible = true;
				}
			}

			if (Steamworks.SteamUser.BLoggedOn())
			{
				if (Glazier.Get().SupportsAutomaticLayout)
				{
					bool hasNewsPreview = readNewsPreview();
					if (!hasNewsPreview)
					{
						MenuUI.instance.StartCoroutine(MenuUI.instance.requestSteamNews());
					}
				}
				else
				{
					// Cannot show articles without automatic layout. An explanation will be shown instead.
				}
			}
		}

		private void OnClickedPreviewBranchChangelog(ISleekElement button)
		{
			Provider.provider.browserService.open("https://support.smartlydressedgames.com/hc/en-us/articles/12462494977172");
		}

		private void CreatePreviewBranchChangelogButton()
		{
			ISleekButton button = Glazier.Get().CreateButton();
			button.PositionOffset_X = 210;
			button.PositionOffset_Y = mainHeaderOffset;
			button.SizeOffset_Y = 60;
			button.SizeOffset_X = -210;
			button.SizeScale_X = 1.0f;
			button.Text = "Click here to open preview branch changelog.";
			button.OnClicked += OnClickedPreviewBranchChangelog;
			container.AddChild(button);
			mainHeaderOffset += button.SizeOffset_Y + 10;
			mainScrollView.PositionOffset_Y += button.SizeOffset_Y + 10;
			mainScrollView.SizeOffset_Y -= (button.SizeOffset_Y + 10);
		}

		private MenuPauseUI pauseUI;
		private MenuCreditsUI creditsUI;
		private MenuTitleUI titleUI;
		private MenuPlayUI playUI;
		private MenuSurvivorsUI survivorsUI;
		private MenuConfigurationUI configUI;
		private MenuWorkshopUI workshopUI;

		private const string STEAM_CLAN_IMAGE = "https://clan.fastly.steamstatic.com/images/";
	}
}
