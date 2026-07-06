////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class SleekWorkshopSubscriptionButton : SleekWrapper
	{
		public PublishedFileId_t fileId;
		public string subscribeText;
		public string unsubscribeText;
		public string subscribeTooltip;
		public string unsubscribeTooltip;

		public override bool UseManualLayout
		{
			set
			{
				base.UseManualLayout = value;
				button.UseManualLayout = value;
				button.UseChildAutoLayout = value ? ESleekChildLayout.None : ESleekChildLayout.Horizontal;
				button.ExpandChildren = !value;
			}
		}

		public void synchronizeText()
		{
			bool subscribed = Provider.provider.workshopService.getSubscribed(fileId.m_PublishedFileId);
			button.Text = subscribed ? unsubscribeText : subscribeText;
			button.TooltipText = subscribed ? unsubscribeTooltip : subscribeTooltip;
		}

		protected void handleClickedButton(ISleekElement thisButton)
		{
			bool parentSubscribed = Provider.provider.workshopService.getSubscribed(fileId.m_PublishedFileId);
			bool newSubscribed = !parentSubscribed;

			Provider.provider.workshopService.setSubscribed(fileId.m_PublishedFileId, newSubscribed);
			synchronizeText();
		}

		public SleekWorkshopSubscriptionButton() : base()
		{
			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.TextAlignment = UnityEngine.TextAnchor.MiddleCenter;
			button.OnClicked += handleClickedButton;
			AddChild(button);
		}

		private ISleekButton button;
	}
}
