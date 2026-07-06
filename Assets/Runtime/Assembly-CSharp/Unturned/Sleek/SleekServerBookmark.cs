////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Entry in the MenuPlayServerBookmarksUI list.
	/// </summary>
	public class SleekServerBookmark : SleekWrapper
	{
		private ServerBookmarkDetails bookmarkDetails;

		private ISleekButton button;
		private SleekButtonIcon toggleBookmarkButton;
		private SleekWebImage thumbnail;
		private ISleekLabel nameLabel;
		private ISleekLabel descLabel;
		private ISleekLabel hostLabel;

		internal event System.Action<ServerBookmarkDetails> OnClickedBookmark;

		private void OnClickedButton(ISleekElement button)
		{
			OnClickedBookmark?.Invoke(bookmarkDetails);
		}

		private void OnClickedToggleBookmarkButton(ISleekElement button)
		{
			bookmarkDetails.isBookmarked = !bookmarkDetails.isBookmarked;
			if (bookmarkDetails.isBookmarked)
			{
				ServerBookmarksManager.AddBookmark(bookmarkDetails);
			}
			else
			{
				ServerBookmarksManager.RemoveBookmark(bookmarkDetails.steamId);
			}
			RefreshBookmarkButton();
		}

		private void RefreshBookmarkButton()
		{
			if (bookmarkDetails.isBookmarked)
			{
				button.IsClickable = true;

				toggleBookmarkButton.tooltip = MenuPlayServerInfoUI.localization.format("Bookmark_Off_Button");
				toggleBookmarkButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Bookmark_Remove");
			}
			else
			{
				button.IsClickable = false;

				toggleBookmarkButton.tooltip = MenuPlayServerInfoUI.localization.format("Bookmark_On_Button");
				toggleBookmarkButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Bookmark_Add");
			}
		}

		internal SleekServerBookmark(ServerBookmarkDetails bookmarkDetails) : base()
		{
			this.bookmarkDetails = bookmarkDetails;

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.SizeOffset_X = -40;
			button.OnClicked += OnClickedButton;

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 45;
			nameLabel.SizeScale_X = 1;
			nameLabel.SizeOffset_X = -45;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.Text = bookmarkDetails.name;
			button.AddChild(nameLabel);

			if (string.IsNullOrEmpty(bookmarkDetails.description))
			{
				nameLabel.SizeOffset_Y = 40;
			}
			else
			{
				nameLabel.SizeOffset_Y = 30;

				descLabel = Glazier.Get().CreateLabel();
				descLabel.PositionOffset_X = 45;
				descLabel.PositionOffset_Y = 15;
				descLabel.SizeScale_X = 1;
				descLabel.SizeOffset_X = -45;
				descLabel.SizeOffset_Y = 30;
				descLabel.FontSize = ESleekFontSize.Small;
				descLabel.AllowRichText = true;
				descLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				descLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				descLabel.TextAlignment = TextAnchor.MiddleLeft;
				descLabel.Text = bookmarkDetails.description;
				button.AddChild(descLabel);
			}

			if (!string.IsNullOrEmpty(bookmarkDetails.thumbnailUrl))
			{
				thumbnail = new SleekWebImage();
				thumbnail.PositionOffset_X = 4;
				thumbnail.PositionOffset_Y = 4;
				thumbnail.SizeOffset_X = 32;
				thumbnail.SizeOffset_Y = 32;
				thumbnail.Refresh(bookmarkDetails.thumbnailUrl);
				button.AddChild(thumbnail);
			}

			hostLabel = Glazier.Get().CreateLabel();
			hostLabel.PositionOffset_X = 45;
			hostLabel.SizeScale_X = 1;
			hostLabel.SizeOffset_X = -50;
			hostLabel.SizeOffset_Y = 40;
			hostLabel.TextAlignment = TextAnchor.MiddleRight;
			if (string.IsNullOrEmpty(bookmarkDetails.host))
			{
				hostLabel.Text = bookmarkDetails.steamId.ToString();
			}
			else if (bookmarkDetails.queryPort > 0)
			{
				hostLabel.Text = $"{bookmarkDetails.host}:{bookmarkDetails.queryPort}";
			}
			else
			{
				hostLabel.Text = bookmarkDetails.host;
			}
			hostLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
			button.AddChild(hostLabel);

			toggleBookmarkButton = new SleekButtonIcon(null, 20);
			toggleBookmarkButton.PositionScale_X = 1.0f;
			toggleBookmarkButton.PositionOffset_X = -40;
			toggleBookmarkButton.SizeOffset_X = 40;
			toggleBookmarkButton.SizeScale_Y = 1;
			toggleBookmarkButton.iconPositionOffset = 10;
			toggleBookmarkButton.iconColor = ESleekTint.FOREGROUND;
			toggleBookmarkButton.onClickedButton += OnClickedToggleBookmarkButton;
			AddChild(toggleBookmarkButton);
			RefreshBookmarkButton();

			AddChild(button);
		}
	}
}
#endif // !DEDICATED_SERVER
