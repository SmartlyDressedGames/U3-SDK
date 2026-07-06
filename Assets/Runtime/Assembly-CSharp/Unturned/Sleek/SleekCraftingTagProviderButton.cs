////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class SleekCraftingTagProviderButton : SleekWrapper
	{
		public ICraftingTagProvider tagProvider;
		private Asset currentAsset;
		private HashSet<TagAsset> currentTags = new HashSet<TagAsset>();

		internal void SetTagProvider(NearbyCraftingTagProvider tagProvider)
		{
			this.tagProvider = tagProvider.component;

			if (currentAsset != null && currentAsset.Equals(tagProvider.asset) && currentTags.SetEquals(tagProvider.tags))
			{
				return;
			}
			currentAsset = tagProvider.asset;
			currentTags.Clear();

			string richTextName;
			if (tagProvider.asset is ItemAsset itemAsset)
			{
				nameLabel.PositionOffset_X = 50;
				nameLabel.SizeOffset_X = -50;
				nameLabel.Text = itemAsset.itemName;
				nameLabel.TextColor = ItemTool.getRarityColorUI(itemAsset.rarity);
				icon.Refresh(itemAsset, 40, 40);
				icon.IsVisible = true;
				richTextName = itemAsset.RarityRichTextName;
			}
			else
			{
				nameLabel.PositionOffset_X = 0;
				nameLabel.SizeOffset_X = 0;
				nameLabel.Text = tagProvider.asset.FriendlyName;
				nameLabel.TextColor = ESleekTint.FONT;
				icon.IsVisible = false;
				richTextName = tagProvider.asset.FriendlyName;
			}

			tagsSb.Clear();
			tagsSb.AppendFormat(PlayerDashboardCraftingUI.localization.format("TagProvider_Tooltip", richTextName));
			tagsSb.AppendLine();
			bool isFirst = true;
			foreach (TagAsset tag in tagProvider.tags)
			{
				currentTags.Add(tag);

				if (!isFirst)
				{
					tagsSb.Append(PlayerDashboardCraftingUI.localization.format("Requirements_Separator"));
				}
				tagsSb.Append(tag.RichTextOrPreferredFontColor);
				isFirst = false;
			}

			tagsSb.AppendLine();
			tagsSb.AppendLine();
			tagsSb.Append(PlayerDashboardCraftingUI.localization.format("CombineFiltersTooltip",
				MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.modify)));

			button.TooltipText = tagsSb.ToString();
		}

		public event System.Action<ICraftingTagProvider> OnClicked;

		public SleekCraftingTagProviderButton() : base()
		{
			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.AllowRichText = true;
			button.OnClicked += OnClickedInternalButton;
			AddChild(button);

			icon = new SleekItemIcon();
			icon.PositionOffset_X = 5;
			icon.PositionOffset_Y = 5;
			icon.SizeOffset_X = 40;
			icon.SizeOffset_Y = 40;
			icon.IsVisible = false;
			AddChild(icon);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.SizeScale_Y = 1.0f;
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameLabel.TextAlignment = UnityEngine.TextAnchor.MiddleLeft;
			nameLabel.FontSize = ESleekFontSize.Medium;
			AddChild(nameLabel);
		}

		private ISleekButton button;
		private SleekItemIcon icon;
		private ISleekLabel nameLabel;

		private void OnClickedInternalButton(ISleekElement internalButton)
		{
			OnClicked?.Invoke(tagProvider);
		}

		private static System.Text.StringBuilder tagsSb = new System.Text.StringBuilder();
	}
}
