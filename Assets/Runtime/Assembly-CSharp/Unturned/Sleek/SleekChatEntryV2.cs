////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekChatEntryV2 : SleekWrapper
	{
		/// <summary>
		/// Does this label fade out as the chat message gets older?
		/// </summary>
		public bool shouldFadeOutWithAge;

		public bool forceVisibleWhileBrowsingChatHistory;

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
			if (forceVisibleWhileBrowsingChatHistory)
			{
				alpha = 1.0f;
			}

			Color avatarColor = avatarImage.TintColor;
			avatarColor.a = alpha;
			avatarImage.TintColor = avatarColor;
			remoteImage.color = avatarColor;

			Color contentsColor = contentsLabel.TextColor;
			contentsColor.a = alpha;
			contentsLabel.TextColor = contentsColor;
		}

		public SleekChatEntryV2() : base()
		{
			UseManualLayout = false;
			UseChildAutoLayout = ESleekChildLayout.Horizontal;
			ChildPerpendicularAlignment = ESleekChildPerpendicularAlignment.Top;

			ISleekElement iconContainer = Glazier.Get().CreateFrame();
			iconContainer.UseManualLayout = false;
			iconContainer.UseWidthLayoutOverride = true;
			iconContainer.UseHeightLayoutOverride = true;
			iconContainer.SizeOffset_X = 40;
			iconContainer.SizeOffset_Y = 40;
			AddChild(iconContainer);

			avatarImage = Glazier.Get().CreateImage();
			avatarImage.PositionOffset_X = 4;
			avatarImage.PositionOffset_Y = 4;
			avatarImage.SizeOffset_X = 32;
			avatarImage.SizeOffset_Y = 32;
			avatarImage.IsVisible = false;
			iconContainer.AddChild(avatarImage);

			remoteImage = new SleekWebImage();
			remoteImage.PositionOffset_X = 4;
			remoteImage.PositionOffset_Y = 4;
			remoteImage.SizeOffset_X = 32;
			remoteImage.SizeOffset_Y = 32;
			remoteImage.IsVisible = false;
			iconContainer.AddChild(remoteImage);

			ISleekElement verticalLayout = Glazier.Get().CreateFrame();
			verticalLayout.UseManualLayout = false;
			verticalLayout.UseChildAutoLayout = ESleekChildLayout.Vertical;
			verticalLayout.ExpandChildren = true;
			AddChild(verticalLayout);

			contentsLabel = Glazier.Get().CreateLabel();
			contentsLabel.UseManualLayout = false;
			contentsLabel.FontSize = ESleekFontSize.Medium;
			contentsLabel.TextAlignment = TextAnchor.MiddleLeft;
			contentsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			verticalLayout.AddChild(contentsLabel);
		}

		private ISleekImage avatarImage;
		private SleekWebImage remoteImage;
		private ISleekLabel contentsLabel;
	}
}
