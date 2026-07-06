////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekBlueprint : SleekWrapper
	{
		private BlueprintStatus blueprintStatus;
		public Blueprint blueprint => blueprintStatus.blueprint;

		private ISleekButton backgroundButton;
		private ISleekLabel titleLabel;
		private ISleekLabel descriptionLabel;
		private ISleekElement formulaImagesContainer;
		private ISleekElement formulaLabelsContainer;
		private ISleekImage preferencesIcon;

		private List<SleekItemIcon> pooledItemIcons = new List<SleekItemIcon>();
		private List<ISleekImage> pooledImages = new List<ISleekImage>();
		private List<ISleekLabel> pooledLabels = new List<ISleekLabel>();

		internal delegate void Clicked(BlueprintStatus blueprintStatus);
		internal event Clicked OnClickedBlueprint;

		private void RefreshPreferencesAndTooltip()
		{
#if !DEDICATED_SERVER
			EBlueprintPreferences preferences = PlayerCrafting.GetBlueprintPreferences(blueprint);
			if (preferences != EBlueprintPreferences.None)
			{
				string iconName = null;
				switch (preferences)
				{
					case EBlueprintPreferences.Ignored:
						iconName = "BlueprintHiddenIcon";
						break;

					case EBlueprintPreferences.Favorited:
						iconName = "FavoriteBlueprintIcon";
						break;
				}

				preferencesIcon.Texture = PlayerDashboardCraftingUI.icons.load<Texture2D>(iconName);
				preferencesIcon.IsVisible = true;
			}
			else
			{
				preferencesIcon.IsVisible = false;
			}

			Local localization = PlayerDashboardCraftingUI.localization;
			tooltipSb.Clear();
			tooltipSb.AppendLine(titleLabel.Text);
			TagAsset categoryTag = blueprint.GetCategoryTag();
			if (categoryTag != null)
			{
				tooltipSb.AppendFormat(localization.format("BlueprintCategoryLabel"), categoryTag.RichTextOrPreferredFontColor);
				tooltipSb.AppendLine();
			}
			tooltipSb.AppendLine();

			if (preferences != EBlueprintPreferences.Ignored && blueprintStatus.IsCraftable)
			{
				string skipCraftingText = MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.SkipActionCraftingMenu);
				if (OptionsSettings.ShouldClickBlueprintToCraft)
				{
					string format = localization.format("InvertSkipCraftingTooltip");
					tooltipSb.AppendFormat(format, skipCraftingText);
				}
				else
				{
					string format = PlayerDashboardInventoryUI.localization.format("ActionBlueprint_SkipCraftingTooltip");
					tooltipSb.AppendFormat(format, skipCraftingText);
				}
				tooltipSb.AppendLine();

				string craftAllText = MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other);
				tooltipSb.AppendFormat(PlayerDashboardInventoryUI.localization.format("ActionBlueprint_CraftAllTooltip"),
					craftAllText);
			}
			else
			{
				PlayerDashboardCraftingUI.BuildNotCraftableTooltip(tooltipSb, blueprintStatus, preferences);
			}
			backgroundButton.TooltipText = tooltipSb.ToString();
#endif
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
#if !DEDICATED_SERVER
			PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged -= RefreshPreferencesAndTooltip;
#endif
		}

		internal SleekBlueprint()
		{
			backgroundButton = Glazier.Get().CreateButton();
			backgroundButton.SizeScale_X = 1.0f;
			backgroundButton.SizeScale_Y = 1.0f;
			backgroundButton.OnClicked += onClickedBackgroundButton;
			AddChild(backgroundButton);

			titleLabel = Glazier.Get().CreateLabel();
			titleLabel.PositionOffset_X = 5;
			titleLabel.PositionOffset_Y = 5;
			titleLabel.SizeOffset_X = -10;
			titleLabel.SizeOffset_Y = 30;
			titleLabel.SizeScale_X = 1f;
			titleLabel.AllowRichText = true;
			titleLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			titleLabel.FontSize = ESleekFontSize.Medium;
			AddChild(titleLabel);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 5;
			descriptionLabel.PositionOffset_Y = -35;
			descriptionLabel.PositionScale_Y = 1f;
			descriptionLabel.SizeOffset_X = -10;
			descriptionLabel.SizeOffset_Y = 30;
			descriptionLabel.SizeScale_X = 1f;
			descriptionLabel.AllowRichText = true;
			descriptionLabel.FontSize = ESleekFontSize.Medium;
			descriptionLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(descriptionLabel);

			formulaImagesContainer = Glazier.Get().CreateFrame();
			formulaImagesContainer.PositionScale_X = 0.5f;
			formulaImagesContainer.SizeScale_Y = 1f;
			AddChild(formulaImagesContainer);

			formulaLabelsContainer = Glazier.Get().CreateFrame();
			formulaLabelsContainer.PositionScale_X = 0.5f;
			formulaLabelsContainer.SizeScale_Y = 1f;
			AddChild(formulaLabelsContainer);

			preferencesIcon = Glazier.Get().CreateImage();
			preferencesIcon.PositionOffset_X = -50;
			preferencesIcon.PositionOffset_Y = -40;
			preferencesIcon.PositionScale_X = 1;
			preferencesIcon.PositionScale_Y = 1;
			preferencesIcon.SizeOffset_X = 40;
			preferencesIcon.SizeOffset_Y = 40;
			preferencesIcon.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
			AddChild(preferencesIcon);

#if !DEDICATED_SERVER
			PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged += RefreshPreferencesAndTooltip;
#endif
		}

		internal void SetBlueprintStatus(BlueprintStatus blueprintStatus)
		{
			this.blueprintStatus = blueprintStatus;

			backgroundButton.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, blueprintStatus.IsCraftable ? 1.0f : 0.5f);

			Local localization = PlayerDashboardCraftingUI.localization;

			inputItemsSb.Clear();
			titleSb.Clear();

			int usedItemIconCount = 0;
			int usedImageCount = 0;
			int usedLabelCount = 0;

			CachingAssetRef[] requiredTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
			bool requiresDescriptionLabel = blueprint.RequiresSkill || requiredTags != null;
			if (requiresDescriptionLabel)
			{
				descSb.Clear();

				if (blueprint.RequiresSkill)
				{
					int specialityIndex = blueprint.SkillSpecialityIndex;
					int skillIndex = blueprint.SkillIndex;
					int hasLevel = Player.LocalPlayer.skills.skills[specialityIndex][skillIndex].level;
					bool meetsLevelRequirement = hasLevel >= blueprint.level;
					Local skillsLocalization = PlayerDashboardSkillsUI.localization;
					string nameText = skillsLocalization.format("Speciality_" + specialityIndex + "_Skill_" + skillIndex);
					string levelText = skillsLocalization.format("Level_" + blueprint.level);
					string skillText = PlayerDashboardCraftingUI.localization.format("Requirements_Skill", nameText, levelText);
					ESleekTint tint = meetsLevelRequirement ? ESleekTint.FONT : ESleekTint.BAD;
					Color skillColor = new SleekColor(tint).Get();
					descSb.Append("<color=");
					descSb.Append(Palette.hex(skillColor));
					descSb.Append('>');
					descSb.Append(skillText);
					descSb.Append("</color>");
				}

				if (requiredTags != null)
				{
					for (int tagIndex = 0; tagIndex < requiredTags.Length; ++tagIndex)
					{
						ref CachingAssetRef tagRef = ref requiredTags[tagIndex];
						TagAsset tag = tagRef.Get<TagAsset>();
						if (tag == null)
							continue;

						if (descSb.Length > 0)
						{
							descSb.Append(PlayerDashboardCraftingUI.localization.format("Requirements_Separator"));
						}

						bool hasTag = Player.LocalPlayer.crafting.IsCraftingTagAvailable(tag);
						if (hasTag)
						{
							descSb.Append(tag.RichTextOrPreferredFontColor);
						}
						else
						{
							descSb.Append("<color=");
							descSb.Append(Palette.hex(OptionsSettings.badColor));
							descSb.Append('>');
							descSb.Append(tag.PlainTextName);
							descSb.Append("</color>");
						}
					}
				}

				descriptionLabel.Text = PlayerDashboardCraftingUI.localization.format("Requirements_Label", descSb.ToString());
				descriptionLabel.IsVisible = true;
			}
			else
			{
				descriptionLabel.IsVisible = false;
			}

			float formulaPosition = 0;
			const float formulaItemPadding = 5;
			for (int inputIndex = 0; inputIndex < blueprint.supplies.Length; inputIndex++)
			{
				BlueprintSupply inputItemConfig = blueprint.supplies[inputIndex];
				ItemAsset supplyAsset = inputItemConfig.FindItemAsset();
				if (supplyAsset == null)
				{
					continue;
				}

				BlueprintInputItemStatus inputItemStatus = blueprintStatus.inputItems[inputIndex];

				SleekItemIcon inputItemIcon = CreateItemIcon(supplyAsset, ref usedItemIconCount);
				inputItemIcon.PositionOffset_X = formulaPosition;

				byte[] iconState = inputItemStatus.FirstItemOrNull?.state;
				if (iconState == null)
				{
					iconState = supplyAsset.getState(false);
				}

				inputItemIcon.Refresh(supplyAsset.id, 100, iconState, supplyAsset,
					Mathf.RoundToInt(inputItemIcon.SizeOffset_X), Mathf.RoundToInt(inputItemIcon.SizeOffset_Y));

				ISleekLabel inputItemLabel = CreateLabel(ref usedLabelCount);
				inputItemLabel.PositionOffset_X = inputItemIcon.PositionOffset_X - 100;
				inputItemLabel.PositionOffset_Y = inputItemIcon.PositionOffset_Y;
				inputItemLabel.PositionScale_Y = inputItemIcon.PositionScale_Y;
				inputItemLabel.SizeOffset_X = inputItemIcon.SizeOffset_X + 100;
				inputItemLabel.SizeOffset_Y = inputItemIcon.SizeOffset_Y;
				inputItemLabel.AllowRichText = false;

				if (blueprint.Operation == EBlueprintOperation.FillTargetItem && inputIndex == 0)
				{
					inputItemLabel.TextColor = ESleekTint.FONT;
					inputItemLabel.Text = $"x{inputItemStatus.totalAmount}";
				}
				else
				{
					inputItemLabel.TextColor = inputItemStatus.isMissingRequiredAmount ? ESleekTint.BAD : ESleekTint.FONT;
					inputItemLabel.Text = PlayerDashboardCraftingUI.localization.format("BlueprintAmountLabel",
						inputItemStatus.totalAmount, inputItemConfig.amount);
				}

				inputItemsSb.Append(supplyAsset.RarityRichTextName);
				if (inputItemConfig.amount > 1)
				{
					inputItemsSb.Append(" x");
					inputItemsSb.Append(inputItemConfig.amount);
				}
				if (!inputItemConfig.ShouldConsume)
				{
					inputItemsSb.Append(' ');
					inputItemsSb.Append(localization.format("BlueprintTitle_ToolItem"));
				}

				formulaPosition += inputItemIcon.SizeOffset_X;
				formulaPosition += formulaItemPadding;

				bool isLastItem = inputIndex == blueprint.supplies.Length - 1;
				if (!isLastItem)
				{
					inputItemsSb.Append(localization.format("BlueprintTitle_ItemSeparator"));

					Texture2D plusIcon = PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus");
					ISleekImage plusImage = CreateImage(plusIcon, ref usedImageCount);
					plusImage.PositionOffset_X = formulaPosition;

					formulaPosition += plusImage.SizeOffset_X;
					formulaPosition += formulaItemPadding;
				}
			}

			if (blueprint.TargetItem != null)
			{
				Texture2D arrowIcon = PlayerDashboardCraftingUI.icons.load<Texture2D>("Arrow");
				ISleekImage arrowImage = CreateImage(arrowIcon, ref usedImageCount);
				arrowImage.PositionOffset_X = formulaPosition;

				formulaPosition += arrowImage.SizeOffset_X;
				formulaPosition += formulaItemPadding;

				BlueprintSupply targetItemConfig = blueprint.TargetItem;
				ItemAsset targetItemAsset = blueprint.TargetItem.FindItemAsset();
				if (targetItemAsset != null)
				{
					BlueprintInputItemStatus targetItemStatus = blueprintStatus.targetStatus;

					SleekItemIcon targetItemIcon = CreateItemIcon(targetItemAsset, ref usedItemIconCount);
					targetItemIcon.PositionOffset_X = formulaPosition;

					formulaPosition += targetItemIcon.SizeOffset_X;
					formulaPosition += formulaItemPadding;

					byte[] iconState = null;
					byte targetItemQuality = 0;
					int targetItemAmount = 0;
					Item targetItem = targetItemStatus.FirstItemOrNull;
					if (targetItem != null)
					{
						iconState = targetItem.state;
						targetItemQuality = targetItem.quality;
						targetItemAmount = targetItem.amount;
					}
					if (iconState == null)
					{
						iconState = targetItemAsset.getState(false);
					}

					targetItemIcon.Refresh(targetItemAsset.id, 100, iconState, targetItemAsset,
						Mathf.RoundToInt(targetItemIcon.SizeOffset_X), Mathf.RoundToInt(targetItemIcon.SizeOffset_Y));

					ISleekLabel targetItemLabel = CreateLabel(ref usedLabelCount);
					targetItemLabel.PositionOffset_X = targetItemIcon.PositionOffset_X - 100;
					targetItemLabel.PositionOffset_Y = targetItemIcon.PositionOffset_Y;
					targetItemLabel.PositionScale_Y = targetItemIcon.PositionScale_Y;
					targetItemLabel.SizeOffset_X = targetItemIcon.SizeOffset_X + 100;
					targetItemLabel.SizeOffset_Y = targetItemIcon.SizeOffset_Y;
					targetItemLabel.TextColor = ESleekTint.FOREGROUND;
					targetItemLabel.AllowRichText = true;

					if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
					{
						int delta = 100 - targetItemQuality;
						Color currentQualityColor = ItemTool.getQualityColor(targetItemQuality / 100.0f);
						string currentQualityText = RichTextUtil.wrapWithColor($"{targetItemQuality}%", currentQualityColor);
						targetItemLabel.Text = RichTextUtil.wrapWithColor($"{targetItemQuality} +{delta}%", currentQualityColor);
						titleSb.Append(localization.format("BlueprintTitle_OperationRepair",
							targetItemAsset.RarityRichTextName, currentQualityText, inputItemsSb));
					}
					else if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
					{
						int amountNeeded = targetItemAsset.MaxAmount - targetItemAmount;
						int inputAmount = 0;
						if (blueprintStatus.inputItems.Count > 0)
						{
							inputAmount = blueprintStatus.inputItems[0].totalAmount;
						}

						int transferAmount = Mathf.Min(amountNeeded, inputAmount);
						targetItemLabel.Text = $"x{targetItemAmount} +{transferAmount}";
						titleSb.Append(localization.format("BlueprintTitle_OperationFill",
							targetItemAsset.RarityRichTextName, transferAmount, inputItemsSb));
					}
				}
			}
			if (titleSb.Length < 1)
			{
				titleSb.Append(inputItemsSb);
			}

			if (blueprint.outputs != null && blueprint.outputs.Length > 0)
			{
				titleSb.Append(localization.format("BlueprintTitle_OutputSeparator"));

				ISleekImage equalsImage = CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Equals"), ref usedImageCount);
				equalsImage.PositionOffset_X = formulaPosition;

				formulaPosition += equalsImage.SizeOffset_X;
				formulaPosition += formulaItemPadding;

				for (int index = 0; index < blueprint.outputs.Length; index++)
				{
					BlueprintOutput output = blueprint.outputs[index];

					ItemAsset productAsset = output.FindItemAsset();
					if (productAsset != null)
					{
						titleSb.Append(productAsset.RarityRichTextName);
						if (output.amount > 1)
						{
							titleSb.Append(" x");
							titleSb.Append(output.amount);
						}

						SleekItemIcon outputItemIcon = CreateItemIcon(productAsset, ref usedItemIconCount);
						outputItemIcon.PositionOffset_X = formulaPosition;

						byte quality;
						byte[] state;
						if (blueprint.transferState)
						{
							blueprintStatus.GetPreviewOutputTransferState(productAsset, out quality, out state);
						}
						else
						{
							quality = 100;
							state = productAsset.getState();
						}
						outputItemIcon.Refresh(productAsset.id, quality, state, productAsset,
							Mathf.RoundToInt(outputItemIcon.SizeOffset_X), Mathf.RoundToInt(outputItemIcon.SizeOffset_Y));

						if (output.amount > 1 || quality != 100)
						{
							ISleekLabel outputItemLabel = CreateLabel(ref usedLabelCount);
							outputItemLabel.PositionOffset_X = outputItemIcon.PositionOffset_X - 100;
							outputItemLabel.PositionOffset_Y = outputItemIcon.PositionOffset_Y;
							outputItemLabel.PositionScale_Y = outputItemIcon.PositionScale_Y;
							outputItemLabel.SizeOffset_X = outputItemIcon.SizeOffset_X + 100;
							outputItemLabel.SizeOffset_Y = outputItemIcon.SizeOffset_Y;
							outputItemLabel.AllowRichText = true;
							outputItemLabel.TextColor = ESleekTint.FOREGROUND;

							string text = string.Empty;
							if (quality != 100)
							{
								Color qualityColor = ItemTool.getQualityColor(quality / 100.0f);
								text = $"<color={Palette.hex(qualityColor)}>{quality}%</color>";
							}
							if (output.amount > 1)
							{
								if (!string.IsNullOrEmpty(text))
								{
									text += "\n";
								}
								text += $"x{output.amount}";
							}

							outputItemLabel.Text = text;
						}

						formulaPosition += outputItemIcon.SizeOffset_X;
						formulaPosition += formulaItemPadding;

						if (index < blueprint.outputs.Length - 1)
						{
							titleSb.Append(localization.format("BlueprintTitle_ItemSeparator"));
							
							ISleekImage plusImage = CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"), ref usedImageCount);
							plusImage.PositionOffset_X = formulaPosition;

							formulaPosition += plusImage.SizeOffset_X;
							formulaPosition += formulaItemPadding;
						}
					}
				}
			}

			string titleText = titleSb.ToString();
			titleLabel.Text = titleText;

			formulaPosition -= formulaItemPadding; // Remove trailing spacing.
			formulaLabelsContainer.PositionOffset_X = -formulaPosition / 2;
			formulaLabelsContainer.SizeOffset_X = formulaPosition;
			formulaImagesContainer.PositionOffset_X = -formulaPosition / 2;
			formulaImagesContainer.SizeOffset_X = formulaPosition;

			RefreshPreferencesAndTooltip();

			while(usedItemIconCount < pooledItemIcons.Count)
			{
				pooledItemIcons[usedItemIconCount].IsVisible = false;
				++usedItemIconCount;
			}
			while (usedImageCount < pooledImages.Count)
			{
				pooledImages[usedImageCount].IsVisible = false;
				++usedImageCount;
			}
			while (usedLabelCount < pooledLabels.Count)
			{
				pooledLabels[usedLabelCount].IsVisible = false;
				++usedLabelCount;
			}
		}

		private void onClickedBackgroundButton(ISleekElement internalButton)
		{
			OnClickedBlueprint?.Invoke(blueprintStatus);
		}

		private SleekItemIcon CreateItemIcon(ItemAsset asset, ref int index)
		{
			float size_x;
			float size_y;
			if (asset.size_y > 2)
			{
				size_y = 100.0f;
				size_x = size_y * ((float) asset.size_x / (float) asset.size_y);
			}
			else
			{
				size_x = asset.size_x * 50.0f;
				size_y = asset.size_y * 50.0f;
			}

			SleekItemIcon itemIcon;
			if (index < pooledItemIcons.Count)
			{
				itemIcon = pooledItemIcons[index];
				itemIcon.IsVisible = true;
				itemIcon.Clear();
			}
			else
			{
				itemIcon = new SleekItemIcon();
				itemIcon.PositionScale_Y = 0.5f;
				pooledItemIcons.Add(itemIcon);
				formulaImagesContainer.AddChild(itemIcon);
			}
			++index;

			itemIcon.PositionOffset_Y = -size_y / 2;
			itemIcon.SizeOffset_X = size_x;
			itemIcon.SizeOffset_Y = size_y;

			return itemIcon;
		}

		private ISleekImage CreateImage(Texture2D texture, ref int index)
		{
			ISleekImage image;
			if (index < pooledImages.Count)
			{
				image = pooledImages[index];
				image.IsVisible = true;
				image.Texture = texture;
			}
			else
			{
				image = Glazier.Get().CreateImage(texture);
				image.PositionOffset_Y = -10;
				image.PositionScale_Y = 0.5f;
				image.SizeOffset_X = 20;
				image.SizeOffset_Y = 20;
				image.TintColor = ESleekTint.FOREGROUND;
				pooledImages.Add(image);
				formulaImagesContainer.AddChild(image);
			}
			++index;
			return image;
		}

		private ISleekLabel CreateLabel(ref int index)
		{
			ISleekLabel label;
			if (index < pooledLabels.Count)
			{
				label = pooledLabels[index];
				label.IsVisible = true;
			}
			else
			{
				label = Glazier.Get().CreateLabel();
				label.TextAlignment = TextAnchor.LowerRight;
				label.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				pooledLabels.Add(label);
				formulaLabelsContainer.AddChild(label);
			}
			++index;
			return label;
		}

		private static System.Text.StringBuilder inputItemsSb = new System.Text.StringBuilder();
		private static System.Text.StringBuilder titleSb = new System.Text.StringBuilder();
		private static System.Text.StringBuilder descSb = new System.Text.StringBuilder();
		private static System.Text.StringBuilder tooltipSb = new System.Text.StringBuilder();
	}
}
