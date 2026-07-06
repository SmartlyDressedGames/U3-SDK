////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	public class SleekDismissWorkshopArticleButton : SleekWrapper
	{
		public ulong articleId;
		public ISleekElement targetContent;
		
		public override bool UseManualLayout
		{
			set
			{
				base.UseManualLayout = value;
				internalButton.UseManualLayout = value;
			}
		}

		public SleekDismissWorkshopArticleButton()
		{
			internalButton = Glazier.Get().CreateButton();
			internalButton.SizeScale_X = 1.0f;
			internalButton.SizeScale_Y = 1.0f;
			internalButton.OnClicked += OnClicked;
			AddChild(internalButton);
		}

		private void OnClicked(ISleekElement button)
		{
			targetContent.IsVisible = !targetContent.IsVisible;
			LocalNews.dismissWorkshopItem(articleId);
		}

		internal ISleekButton internalButton;
	}
}
