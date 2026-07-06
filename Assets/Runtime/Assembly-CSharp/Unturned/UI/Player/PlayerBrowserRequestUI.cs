////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal class PlayerBrowserRequestUI : SleekFullscreenBox
	{
		private Local localization;

		public bool isActive
		{
			get;
			private set;
		}

		private ISleekBox textBox;
		private ISleekButton yesButton;
		private ISleekButton noButton;

		/// <summary>
		/// Nelson 2024-08-19: This link has been checked with WebUtils.CanParseThirdPartyUrl, but is not the
		/// potentially altered link to go through Steam's link filter. This way the UI shows the original link.
		/// </summary>
		private string url;

		public void open(string msg, string url)
		{
			if (isActive)
			{
				return;
			}

			isActive = true;
			this.url = url;

			textBox.Text = localization.format("Request") + "\n" + url + "\n\n\"" + msg + "\"";

			AnimateIntoView();
		}

		public void close()
		{
			if (!isActive)
			{
				return;
			}

			isActive = false;
			url = null;

			AnimateOutOfView(0, 1);
		}

		private void onClickedYesButton(ISleekElement button)
		{
			if (!string.IsNullOrEmpty(url))
			{
				if (Provider.provider.browserService.canOpenBrowser)
				{
					if (WebUtils.ParseThirdPartyUrl(url, out string parsedUrl))
					{
						Provider.provider.browserService.open(parsedUrl);
					}
					else
					{
						UnturnedLog.error($"Ignoring potentially unsafe browser request URL \"{url}\" (Error: Prompt shouldn't have been displayed if this is the case?)");
					}
				}
			}

			PlayerLifeUI.open();
			close();
		}

		private void onClickedNoButton(ISleekElement button)
		{
			PlayerLifeUI.open();
			close();
		}

		public PlayerBrowserRequestUI() : base()
		{
			localization = Localization.read("/Player/PlayerBrowserRequest.dat");

			PositionScale_Y = 1.0f;
			PositionOffset_X = 10;
			PositionOffset_Y = 10;
			SizeOffset_X = -20;
			SizeOffset_Y = -20;
			SizeScale_X = 1.0f;
			SizeScale_Y = 1.0f;
			isActive = false;
			url = null;

			textBox = Glazier.Get().CreateBox();
			textBox.PositionOffset_X = -200;
			textBox.PositionOffset_Y = -50;
			textBox.PositionScale_X = 0.5f;
			textBox.PositionScale_Y = 0.5f;
			textBox.SizeOffset_X = 400;
			textBox.SizeOffset_Y = 100;
			AddChild(textBox);

			yesButton = Glazier.Get().CreateButton();
			yesButton.PositionOffset_X = -200;
			yesButton.PositionOffset_Y = 60;
			yesButton.PositionScale_X = 0.5f;
			yesButton.PositionScale_Y = 0.5f;
			yesButton.SizeOffset_X = 195;
			yesButton.SizeOffset_Y = 30;
			yesButton.Text = localization.format("Yes_Button");
			yesButton.TooltipText = localization.format("Yes_Button_Tooltip");
			yesButton.OnClicked += onClickedYesButton;
			AddChild(yesButton);

			noButton = Glazier.Get().CreateButton();
			noButton.PositionOffset_X = 5;
			noButton.PositionOffset_Y = 60;
			noButton.PositionScale_X = 0.5f;
			noButton.PositionScale_Y = 0.5f;
			noButton.SizeOffset_X = 195;
			noButton.SizeOffset_Y = 30;
			noButton.Text = localization.format("No_Button");
			noButton.TooltipText = localization.format("No_Button_Tooltip");
			noButton.OnClicked += onClickedNoButton;
			AddChild(noButton);
		}
	}
}
