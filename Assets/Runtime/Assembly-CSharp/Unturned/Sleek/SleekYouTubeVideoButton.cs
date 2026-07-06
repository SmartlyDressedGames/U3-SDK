////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekYouTubeVideoButton : SleekWrapper
	{
		public void Refresh(string videoId)
		{
			string videoUrl = $"https://www.youtube.com/watch?v={videoId}";
			linkButton.Url = videoUrl;

			string thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
			webImage.Refresh(thumbnailUrl, false);
		}

		public SleekYouTubeVideoButton(IconsBundle icons)
		{
			// 1280x720 * 75%
			SizeOffset_X = 960 + 20;
			SizeOffset_Y = 540 + 20;

			linkButton = new SleekWebLinkButton();
			linkButton.SizeOffset_X = SizeOffset_X;
			linkButton.SizeOffset_Y = SizeOffset_Y;
			AddChild(linkButton);

			webImage = new SleekWebImage();
			webImage.PositionOffset_X = 10;
			webImage.PositionOffset_Y = 10;
			webImage.SizeOffset_X = 960;
			webImage.SizeOffset_Y = 540;
			AddChild(webImage);

			ISleekImage playIcon = Glazier.Get().CreateImage(icons.load<Texture2D>("PlayVideo"));
			playIcon.PositionOffset_X = -32;
			playIcon.PositionOffset_Y = -32;
			playIcon.PositionScale_X = 0.5f;
			playIcon.PositionScale_Y = 0.5f;
			playIcon.SizeOffset_X = 64;
			playIcon.SizeOffset_Y = 64;
			playIcon.TintColor = ESleekTint.FOREGROUND;
			AddChild(playIcon);
		}

		private SleekWebLinkButton linkButton;
		private SleekWebImage webImage;
	}
}
