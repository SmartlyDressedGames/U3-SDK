////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekSelectedBlueprintRequiredTag : SleekWrapper
	{
		public void SetTag(CachingAssetRef tagRef, bool isMissing)
		{
			if (_tagRef != tagRef)
			{
				_tagRef = tagRef;

				TagAsset tag = _tagRef.Get<TagAsset>();
				if (tag != null)
				{
					icon.Texture = tag.Icon;
					icon.TintColor = tag.ShouldTintIcon ? ESleekTint.FOREGROUND : ESleekTint.NONE;
					nameLabel.Text = tag.PlainTextName;
					nameLabel.TextColor = tag.NameColorOrPreferredFontColor;
				}
			}

			missingLabel.IsVisible = isMissing;
			nameLabel.SizeOffset_Y = isMissing ? 30 : 50;
		}

		public SleekSelectedBlueprintRequiredTag() : base()
		{
			ISleekBox backgroundBox = Glazier.Get().CreateBox();
			backgroundBox.SizeScale_X = 1.0f;
			backgroundBox.SizeScale_Y = 1.0f;
			AddChild(backgroundBox);

			icon = Glazier.Get().CreateImage();
			icon.PositionOffset_X = 5;
			icon.PositionOffset_Y = 5;
			icon.SizeOffset_X = 40;
			icon.SizeOffset_Y = 40;
			AddChild(icon);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 50;
			nameLabel.SizeOffset_X = -50;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.AllowRichText = false;
			AddChild(nameLabel);

			missingLabel = Glazier.Get().CreateLabel();
			missingLabel.PositionOffset_X = 50;
			missingLabel.PositionOffset_Y = 20;
			missingLabel.SizeOffset_X = -50;
			missingLabel.SizeScale_X = 1.0f;
			missingLabel.SizeOffset_Y = 30;
			missingLabel.TextColor = ESleekTint.BAD;
			missingLabel.Text = PlayerDashboardCraftingUI.localization.format("Details_TagMissing");
			missingLabel.TextAlignment = TextAnchor.MiddleLeft;
			AddChild(missingLabel);
		}

		private CachingAssetRef _tagRef;
		private ISleekImage icon;
		private ISleekLabel nameLabel;
		private ISleekLabel missingLabel;
	}
}
