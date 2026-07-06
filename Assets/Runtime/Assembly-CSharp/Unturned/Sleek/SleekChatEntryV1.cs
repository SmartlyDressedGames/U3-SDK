////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekChatEntryV1 : SleekWrapper
	{
		/// <summary>
		/// Does this label fade out as the chat message gets older?
		/// </summary>
		public bool shouldFadeOutWithAge;

		protected ReceivedChatMessage _representingChatMessage;
		/// <summary>
		/// Chat message values to show.
		/// </summary>
		public ReceivedChatMessage representingChatMessage
		{
			get => _representingChatMessage;
			set
			{
				_representingChatMessage = value;

				if (string.IsNullOrEmpty(_representingChatMessage.iconURL))
				{
					Texture2D avatar;
					if (OptionsSettings.ShouldAnonymizeMultiplayerDetails || _representingChatMessage.speaker == null)
					{
						avatar = null;
					}
					else
					{
						avatar = Provider.provider.communityService.getIcon(_representingChatMessage.speaker.playerID.steamID, true);
					}
					avatarImage.Texture = avatar;

					avatarImage.IsVisible = true;
					remoteImage.IsVisible = false;
				}
				else
				{
					remoteImage.Refresh(_representingChatMessage.iconURL);

					avatarImage.IsVisible = false;
					remoteImage.IsVisible = true;
				}

				contentsLabel.TextColor = _representingChatMessage.color;
				contentsLabel.AllowRichText = _representingChatMessage.useRichTextFormatting;
				contentsLabel.Text = _representingChatMessage.contents;
			}
		}

		public override void OnUpdate()
		{
			if (!shouldFadeOutWithAge)
				return;

			float decay = representingChatMessage.age - Provider.preferenceData.Chat.Fade_Delay; // Seconds since this message started fading
			decay = Mathf.Clamp01(decay); // Fade over 1 second
			float alpha = 1.0f - decay; // alpha = 1 prior to decay, 0 after

			Color avatarColor = avatarImage.TintColor;
			avatarColor.a = alpha;
			avatarImage.TintColor = avatarColor;
			remoteImage.color = avatarColor;

			Color contentsColor = contentsLabel.TextColor;
			contentsColor.a = alpha;
			contentsLabel.TextColor = contentsColor;
		}

		public SleekChatEntryV1() : base()
		{
			avatarImage = Glazier.Get().CreateImage();
			avatarImage.PositionOffset_Y = 4;
			avatarImage.SizeOffset_X = 32;
			avatarImage.SizeOffset_Y = 32;
			avatarImage.IsVisible = false;
			AddChild(avatarImage);

			remoteImage = new SleekWebImage();
			remoteImage.PositionOffset_Y = 4;
			remoteImage.SizeOffset_X = 32;
			remoteImage.SizeOffset_Y = 32;
			remoteImage.IsVisible = false;
			AddChild(remoteImage);

			// Was 40px tall, but Chinese characters were getting truncated.
			contentsLabel = Glazier.Get().CreateLabel();
			contentsLabel.PositionOffset_X = 40;
			contentsLabel.PositionOffset_Y = -4;
			contentsLabel.SizeOffset_X = -40;
			contentsLabel.SizeOffset_Y = 48;
			contentsLabel.SizeScale_X = 1;
			contentsLabel.FontSize = ESleekFontSize.Medium;
			contentsLabel.TextAlignment = TextAnchor.MiddleLeft;
			contentsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(contentsLabel);
		}

		private ISleekImage avatarImage;
		private SleekWebImage remoteImage;
		private ISleekLabel contentsLabel;
	}
}
