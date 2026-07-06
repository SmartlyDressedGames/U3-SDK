////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class SleekSelectedBlueprint : SleekWrapper
	{
		/// <summary>
		/// Note: this can be different from status.blueprint after status refreshes because status is pooled.
		/// </summary>
		public Blueprint SelectedBlueprint
		{
			get;
			private set;
		}

		internal void SetSelectedBlueprintStatus(BlueprintStatus status)
		{
			this.status = status;
			SelectedBlueprint = status?.blueprint;
			if (SelectedBlueprint == null)
			{
				return;
			}

			float offset = 0;

			PopulateSummary();
			if (summaryContainer.IsVisible)
			{
				summaryContainer.PositionOffset_Y = offset;
				offset += summaryContainer.SizeOffset_Y;
			}

			PopulateInputItems();
			if (inputItemsContainer.IsVisible)
			{
				inputItemsContainer.PositionOffset_Y = offset;
				offset += inputItemsContainer.SizeOffset_Y;
			}

			PopulateOutputItems();
			if (outputItemsContainer.IsVisible)
			{
				outputItemsContainer.PositionOffset_Y = offset;
				offset += outputItemsContainer.SizeOffset_Y;
			}

			PopulateSkills();
			if (skillContainer.IsVisible)
			{
				skillContainer.PositionOffset_Y = offset;
				offset += skillContainer.SizeOffset_Y;
			}

			PopulateRequiredTags();
			if (requiredTagsContainer.IsVisible)
			{
				requiredTagsContainer.PositionOffset_Y = offset;
				offset += requiredTagsContainer.SizeOffset_Y;
			}

			PopulateConditions();
			if (conditionsContainer.IsVisible)
			{
				conditionsContainer.PositionOffset_Y = offset;
				offset += conditionsContainer.SizeOffset_Y;
			}

			PopulateRewards();
			if (rewardsContainer.IsVisible)
			{
				rewardsContainer.PositionOffset_Y = offset;
				offset += rewardsContainer.SizeOffset_Y;
			}

			detailScrollView.ContentSizeOffset = new Vector2(0, offset);

			// Nelson 2025-07-01: for some mysterious reason (?) when using auto layout if the summary container
			// is hidden when other elements are updated they get zero width. Forcing a layout update fixes this.
			// (public issue #5085)
			bool usingAutoLayout = extendedDescriptionBox != null;
			if (usingAutoLayout)
			{
				detailScrollView.ForceLayoutUpdate();
			}

#if !DEDICATED_SERVER
			RefreshPreferencesAndCraftButtonTooltip(PlayerCrafting.GetBlueprintPreferences(status.blueprint));
#endif
		}

		private ItemAsset currentPrimaryItemAsset;
		private void SetPrimaryItem(ItemAsset asset, byte[] state)
		{
			if (currentPrimaryItemAsset != asset)
			{
				currentPrimaryItemAsset = asset;
				primaryItemIcon.Clear();
			}

			if (asset.size_y >= asset.size_x)
			{
				primaryItemIcon.SizeOffset_Y = primaryItemIcon.SizeOffset_X;
			}
			else
			{
				float iconRatio = asset.size_y / (float) asset.size_x;
				primaryItemIcon.SizeOffset_Y = primaryItemIcon.SizeOffset_X * iconRatio;
			}

			primaryItemIcon.Refresh(asset.id, 100, state, asset, Mathf.RoundToInt(primaryItemIcon.SizeOffset_X),
				Mathf.RoundToInt(primaryItemIcon.SizeOffset_Y));

			summaryContainer.TooltipText = asset.itemDescription;
		}

		/// <summary>
		/// Update the title box describing the "most important" item: item to repair, salvage, craft, etc.
		/// </summary>
		private void PopulateSummary()
		{
			if (extendedDescriptionBox != null)
			{
				extendedDescriptionBox.IsVisible = false;
			}

			Local l10n = PlayerDashboardCraftingUI.localization;
			if (SelectedBlueprint.TargetItem != null)
			{
				ItemAsset targetAsset = SelectedBlueprint.TargetItem.FindItemAsset();
				if (targetAsset == null)
				{
					summaryContainer.IsVisible = false;
					return;
				}

				Item targetItemInstance = status.targetStatus.FirstItemOrNull;
				byte[] targetItemState = targetItemInstance?.state;
				if (targetItemState == null)
				{
					targetItemState = targetAsset.getState();
				}
				SetPrimaryItem(targetAsset, targetItemState);

				descriptionLabel.IsVisible = true;
				if (SelectedBlueprint.Operation == EBlueprintOperation.RepairTargetItem)
				{
					titleLabel.Text = l10n.format("Details_RepairTitle", targetAsset.RarityRichTextName);

					byte targetItemQuality = targetItemInstance?.quality ?? 0;

					int delta = 100 - targetItemQuality;

					Color currentQualityColor = ItemTool.getQualityColor(targetItemQuality / 100.0f);
					Color newQualityColor = Palette.COLOR_G;
					string currentQualityText = RichTextUtil.wrapWithColor($"{targetItemQuality}%", currentQualityColor);
					string newQualityText = RichTextUtil.wrapWithColor("100%", newQualityColor);
					string deltaQualityText = RichTextUtil.wrapWithColor($"{delta}%", newQualityColor);
					descriptionLabel.Text = l10n.format("Details_RepairDescription", deltaQualityText, currentQualityText,
						newQualityText);
				}
				else if (SelectedBlueprint.Operation == EBlueprintOperation.FillTargetItem)
				{
					titleLabel.Text = l10n.format("Details_FillTitle", targetAsset.RarityRichTextName);

					int targetItemAmount = targetItemInstance?.amount ?? 0;

					int amountNeeded = targetAsset.MaxAmount - targetItemAmount;
					int inputAmount = 0;
					if (status.inputItems.Count > 0)
					{
						inputAmount = status.inputItems[0].totalAmount;
					}

					int transferAmount = Mathf.Min(amountNeeded, inputAmount);
					int newTargetAmount = targetItemAmount + transferAmount;
					descriptionLabel.Text = l10n.format("Details_FillDescription", transferAmount, targetItemAmount,
						newTargetAmount);
				}
				else
				{
					summaryContainer.IsVisible = false;
				}
			}
			else if (SelectedBlueprint.CategoryTagRef == EBlueprintTypeEx.salvageCategoryTagRef
				&& SelectedBlueprint.supplies != null && SelectedBlueprint.supplies.Length == 1)
			{
				// Nelson 2025-05-02: not super happy with this special case. But, I'm not convinced what the right
				// cleaner/tidier solution at the moment is. We could have a per-blueprint "title" or "verb" option
				// and an option for which item is the "primary" item. It feels like there's some overlap here
				// between "operations" modifying a target item and different kinds of regular crafting recipes.
				// We also don't want to have too many per-blueprint options making it harder to configure.
				BlueprintSupply input = SelectedBlueprint.supplies[0];
				ItemAsset inputAsset = input.FindItemAsset();
				if (inputAsset == null)
				{
					summaryContainer.IsVisible = false;
					return;
				}

				titleLabel.Text = l10n.format("Details_SalvageTitle", inputAsset.RarityRichTextName);

				Item itemInstance = status.inputItems[0].FirstItemOrNull;
				byte[] itemState = itemInstance?.state;
				if (itemState == null)
				{
					itemState = inputAsset.getState();
				}
				SetPrimaryItem(inputAsset, itemState);

				if (extendedDescriptionBox != null)
				{
					descriptionLabel.IsVisible = false;

					ItemDescriptionBuilder descriptionBuilder = ItemDescriptionBuilderUtils.CreateForUI(inputAsset);
					inputAsset.BuildDescription(descriptionBuilder, itemInstance);
					extendedDescriptionBox.Text = ItemDescriptionBuilderUtils.FormatLines();
					extendedDescriptionBox.IsVisible = true;
				}
				else
				{
					string rarityDesc = PlayerDashboardInventoryUI.localization.format("Rarity_" + (int) inputAsset.rarity);
					string typeDesc = PlayerDashboardInventoryUI.localization.format("Type_" + (int) inputAsset.type);
					descriptionLabel.Text = RichTextUtil.wrapWithColor(PlayerDashboardInventoryUI.localization.format(
						"Rarity_Type_Label", rarityDesc, typeDesc), ItemTool.getRarityColorUI(inputAsset.rarity));
				}
			}
			else if (SelectedBlueprint.outputs.Length == 1)
			{
				BlueprintOutput output = SelectedBlueprint.outputs[0];
				ItemAsset outputAsset = output.FindItemAsset();
				if (outputAsset == null)
				{
					summaryContainer.IsVisible = false;
					return;
				}

				titleLabel.Text = l10n.format("Details_CraftTitle", outputAsset.RarityRichTextName);

				byte outputQuality;
				byte[] outputState;
				if (SelectedBlueprint.transferState)
				{
					status.GetPreviewOutputTransferState(outputAsset, out outputQuality, out outputState);
				}
				else
				{
					outputQuality = 100;
					outputState = outputAsset.getState();
				}
				SetPrimaryItem(outputAsset, outputState);

				if (extendedDescriptionBox != null)
				{
					descriptionLabel.IsVisible = false;

					ItemDescriptionBuilder descriptionBuilder = ItemDescriptionBuilderUtils.CreateForUI(outputAsset);
					outputAsset.BuildDescription(descriptionBuilder, null);
					extendedDescriptionBox.Text = ItemDescriptionBuilderUtils.FormatLines();
					extendedDescriptionBox.IsVisible = true;
				}
				else
				{
					string rarityDesc = PlayerDashboardInventoryUI.localization.format("Rarity_" + (int) outputAsset.rarity);
					string typeDesc = PlayerDashboardInventoryUI.localization.format("Type_" + (int) outputAsset.type);
					descriptionLabel.Text = RichTextUtil.wrapWithColor(PlayerDashboardInventoryUI.localization.format(
						"Rarity_Type_Label", rarityDesc, typeDesc), ItemTool.getRarityColorUI(outputAsset.rarity));
				}
			}
			else
			{
				summaryContainer.IsVisible = false;
				return;
			}

			summaryContainer.IsVisible = true;
			if (descriptionLabel.IsVisible)
			{
				descriptionLabel.PositionOffset_Y = primaryItemIcon.PositionOffset_Y + primaryItemIcon.SizeOffset_Y;
				summaryContainer.SizeOffset_Y = descriptionLabel.PositionOffset_Y + descriptionLabel.SizeOffset_Y;
			}
			else
			{
				summaryContainer.SizeOffset_Y = primaryItemIcon.PositionOffset_Y + primaryItemIcon.SizeOffset_Y;
			}
		}

		private void PopulateInputItems()
		{
			inputItemsContainer.IsVisible = SelectedBlueprint.supplies.Length > 0;
			if (!inputItemsContainer.IsVisible)
			{
				return;
			}

			consumingInputIndices.Clear();
			nonConsumingInputIndices.Clear();
			for (int inputItemIndex = 0; inputItemIndex < SelectedBlueprint.supplies.Length; ++inputItemIndex)
			{
				BlueprintSupply inputItemConfig = SelectedBlueprint.supplies[inputItemIndex];
				if (inputItemConfig.FindItemAsset() == null)
					continue;

				if (inputItemConfig.ShouldConsume)
				{
					consumingInputIndices.Add(inputItemIndex);
				}
				else
				{
					nonConsumingInputIndices.Add(inputItemIndex);
				}
			}

			float offset = 0;
			int widgetIndex = 0;

			if (consumingInputIndices.Count > 0)
			{
				inputItemsLabel.IsVisible = true;
				offset += 40;
				foreach (int inputItemIndex in consumingInputIndices)
				{
					AddInputItemWidget(inputItemIndex, ref widgetIndex, ref offset);
				}
			}
			else
			{
				inputItemsLabel.IsVisible = false;
			}

			if (nonConsumingInputIndices.Count > 0)
			{
				toolItemsLabel.IsVisible = true;
				toolItemsLabel.PositionOffset_Y = offset;
				offset += 40;
				foreach (int inputItemIndex in nonConsumingInputIndices)
				{
					AddInputItemWidget(inputItemIndex, ref widgetIndex, ref offset);
				}
			}
			else
			{
				toolItemsLabel.IsVisible = false;
			}

			inputItemsContainer.SizeOffset_Y = offset;

			while (widgetIndex < inputItemWidgets.Count)
			{
				SleekSelectedBlueprintItem inputItemWidget = inputItemWidgets[widgetIndex];
				inputItemWidget.IsVisible = false;
				++widgetIndex;
			}
		}

		private void AddInputItemWidget(int inputItemIndex, ref int widgetIndex, ref float offset)
		{
			BlueprintSupply inputItemConfig = SelectedBlueprint.supplies[inputItemIndex];
			BlueprintInputItemStatus inputItemStatus = status.inputItems[inputItemIndex];

			SleekSelectedBlueprintItem inputItemWidget;
			if (widgetIndex < inputItemWidgets.Count)
			{
				inputItemWidget = inputItemWidgets[widgetIndex];
				inputItemWidget.IsVisible = true;
			}
			else
			{
				inputItemWidget = new SleekSelectedBlueprintItem();
				inputItemWidget.SizeScale_X = 1.0f;
				inputItemsContainer.AddChild(inputItemWidget);
				inputItemWidgets.Add(inputItemWidget);
			}
			inputItemWidget.PositionOffset_Y = offset;
			inputItemWidget.blueprintStatus = status;
			inputItemWidget.SetInputItem(inputItemConfig, inputItemStatus, inputItemIndex);
			offset += inputItemWidget.SizeOffset_Y;
			++widgetIndex;
		}

		private void PopulateOutputItems()
		{
			outputItemsContainer.IsVisible = SelectedBlueprint.outputs.Length > 0;
			if (!outputItemsContainer.IsVisible)
			{
				return;
			}

			float offset = 40;

			int widgetIndex = 0;
			for (int outputItemIndex = 0; outputItemIndex < SelectedBlueprint.outputs.Length; ++outputItemIndex)
			{
				BlueprintOutput output = SelectedBlueprint.outputs[outputItemIndex];
				if (output.FindItemAsset() == null)
					continue;

				SleekSelectedBlueprintItem outputItemWidget;
				if (widgetIndex < outputItemWidgets.Count)
				{
					outputItemWidget = outputItemWidgets[widgetIndex];
					outputItemWidget.IsVisible = true;
				}
				else
				{
					outputItemWidget = new SleekSelectedBlueprintItem();
					outputItemWidget.SizeScale_X = 1.0f;
					outputItemsContainer.AddChild(outputItemWidget);
					outputItemWidgets.Add(outputItemWidget);
				}
				outputItemWidget.PositionOffset_Y = offset;
				outputItemWidget.blueprintStatus = status;
				outputItemWidget.SetOutputItem(status, output, outputItemIndex);
				offset += outputItemWidget.SizeOffset_Y;
				++widgetIndex;
			}

			outputItemsContainer.SizeOffset_Y = offset;

			while (widgetIndex < outputItemWidgets.Count)
			{
				SleekSelectedBlueprintItem outputItemWidget = outputItemWidgets[widgetIndex];
				outputItemWidget.IsVisible = false;
				++widgetIndex;
			}
		}

		private void PopulateSkills()
		{
			skillContainer.IsVisible = SelectedBlueprint.RequiresSkill;
			if (!skillContainer.IsVisible)
			{
				return;
			}

			int specialityIndex = SelectedBlueprint.SkillSpecialityIndex;
			int skillIndex = SelectedBlueprint.SkillIndex;
			int hasLevel = Player.LocalPlayer.skills.skills[specialityIndex][skillIndex].level;
			bool meetsLevelRequirement = hasLevel >= SelectedBlueprint.level;
			Local skillsLocalization = PlayerDashboardSkillsUI.localization;
			string nameText = skillsLocalization.format("Speciality_" + specialityIndex + "_Skill_" + skillIndex);
			string levelText = skillsLocalization.format("Level_" + SelectedBlueprint.level);
			skillBox.Text = PlayerDashboardCraftingUI.localization.format("Requirements_Skill", nameText, levelText);
			skillBox.TextColor = meetsLevelRequirement ? ESleekTint.FONT : ESleekTint.BAD;
		}

		private void PopulateRequiredTags()
		{
			CachingAssetRef[] tagRefs = SelectedBlueprint.GetApplicableRequiredNearbyCraftingTags();
			requiredTagsContainer.IsVisible = !tagRefs.IsNullOrEmpty();
			if (!requiredTagsContainer.IsVisible)
			{
				return;
			}

			int widgetIndex = 0;
			for (int tagIndex = 0; tagIndex < tagRefs.Length; ++tagIndex)
			{
				ref CachingAssetRef tagRef = ref tagRefs[tagIndex];
				TagAsset tag = tagRef.Get<TagAsset>();
				if (tag == null)
					continue;

				SleekSelectedBlueprintRequiredTag widget;
				if (widgetIndex < requiredTags.Count)
				{
					widget = requiredTags[widgetIndex];
					widget.IsVisible = true;
				}
				else
				{
					widget = new SleekSelectedBlueprintRequiredTag();
					widget.SizeScale_X = 1.0f;
					widget.SizeOffset_Y = 50;
					requiredTagsContainer.AddChild(widget);
					requiredTags.Add(widget);
				}
				widget.SetTag(tag, !Player.LocalPlayer.crafting.IsCraftingTagAvailable(tag));
				widget.PositionOffset_Y = 40 + widgetIndex * 50;
				++widgetIndex;
			}

			requiredTagsContainer.SizeOffset_Y = 40 + widgetIndex * 50;

			while (widgetIndex < requiredTags.Count)
			{
				SleekSelectedBlueprintRequiredTag widget = requiredTags[widgetIndex];
				widget.IsVisible = false;
				++widgetIndex;
			}
		}

		private void PopulateConditions()
		{
			conditionsContainer.IsVisible = !SelectedBlueprint.questConditions.IsNullOrEmpty();
			if (!conditionsContainer.IsVisible)
			{
				return;
			}

			conditionsElementsContainer.RemoveAllChildren();
			bool hasAnyVisibleConditions = false;

			float offset = 0;
			for (int index = 0; index < SelectedBlueprint.questConditions.Length; ++index)
			{
				INPCCondition condition = SelectedBlueprint.questConditions[index];
				ISleekElement conditionUI = condition.createUI(Player.LocalPlayer, null);

				if (conditionUI == null)
				{
					continue;
				}

				conditionUI.PositionOffset_Y = offset;
				conditionsElementsContainer.AddChild(conditionUI);
				offset += conditionUI.SizeOffset_Y;
				hasAnyVisibleConditions = true;
			}

			conditionsContainer.IsVisible = hasAnyVisibleConditions;

			conditionsElementsContainer.SizeOffset_Y = offset;
			conditionsContainer.SizeOffset_Y = offset + 40;
		}

		private void PopulateRewards()
		{
			rewardsContainer.IsVisible = !SelectedBlueprint.questRewards.IsNullOrEmpty();
			if (!rewardsContainer.IsVisible)
			{
				return;
			}

			rewardsElementsContainer.RemoveAllChildren();
			bool hasAnyVisibleRewards = false;

			float offset = 0;
			for (int index = 0; index < SelectedBlueprint.questRewards.Length; ++index)
			{
				INPCReward reward = SelectedBlueprint.questRewards[index];
				ISleekElement rewardUI = reward.createUI(Player.LocalPlayer);

				if (rewardUI == null)
				{
					continue;
				}

				rewardUI.PositionOffset_Y = offset;
				rewardsElementsContainer.AddChild(rewardUI);
				offset += rewardUI.SizeOffset_Y;
				hasAnyVisibleRewards = true;
			}

			rewardsContainer.IsVisible = hasAnyVisibleRewards;

			rewardsElementsContainer.SizeOffset_Y = offset;
			rewardsContainer.SizeOffset_Y = offset + 40;
		}

		private void OnClickedCraftButton(ISleekElement button)
		{
			if (Player.LocalPlayer.equipment.isBusy)
			{
				return;
			}

			bool asManyAsPossible = InputEx.GetKey(ControlsSettings.other);
			Player.LocalPlayer.crafting.SendRequestToCraft(SelectedBlueprint, asManyAsPossible);
		}

		private void OnSwappedPreferencesState(SleekButtonState button, int state)
		{
#if !DEDICATED_SERVER
			EBlueprintPreferences newPreferences = (EBlueprintPreferences) state;
			PlayerCrafting.SetBlueprintPreferences(status.blueprint, newPreferences);
			RefreshPreferencesAndCraftButtonTooltip(newPreferences);
#endif // !DEDICATED_SERVER
		}

		private static System.Text.StringBuilder tooltipSb = new System.Text.StringBuilder();
		private void RefreshPreferencesAndCraftButtonTooltip(EBlueprintPreferences preferences)
		{
			craftButton.isClickable = preferences != EBlueprintPreferences.Ignored && status.IsCraftable;
			preferencesButton.state = (int) preferences;

			if (preferences != EBlueprintPreferences.Ignored && status.IsCraftable)
			{
				craftButton.tooltip = PlayerDashboardInventoryUI.localization.format("ActionBlueprint_CraftAllTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
			}
			else
			{
				tooltipSb.Clear();
				PlayerDashboardCraftingUI.BuildNotCraftableTooltip(tooltipSb, status, preferences);
				craftButton.tooltip = tooltipSb.ToString();
			}
		}

		public SleekSelectedBlueprint()
		{
			Local l10n = PlayerDashboardCraftingUI.localization;
			IconsBundle icons = PlayerDashboardCraftingUI.icons;
			bool useManualLayout = !Glazier.Get().SupportsAutomaticLayout;

			detailScrollView = Glazier.Get().CreateScrollView();
			detailScrollView.SizeScale_X = 1f;
			detailScrollView.SizeScale_Y = 1f;
			detailScrollView.SizeOffset_Y = -90;
			detailScrollView.ScaleContentToWidth = true;
			detailScrollView.ContentUseManualLayout = useManualLayout;
			AddChild(detailScrollView);

			craftButton = new SleekButtonIcon(icons.load<Texture2D>("CraftIcon"), 40);
			craftButton.PositionOffset_Y = -80;
			craftButton.PositionScale_Y = 1f;
			craftButton.SizeScale_X = 1f;
			craftButton.SizeOffset_Y = 50;
			craftButton.onClickedButton += OnClickedCraftButton;
			craftButton.text = l10n.format("Craft");
			craftButton.fontSize = ESleekFontSize.Medium;
			craftButton.iconColor = ESleekTint.FOREGROUND;
			AddChild(craftButton);

			preferencesButton = new SleekButtonState(20,
				new GUIContent(l10n.format("VisibilityButton_Visible_Label"), icons.load<Texture2D>("BlueprintVisibleIcon"), l10n.format("VisibilityButton_Visible_Tooltip")),
				new GUIContent(l10n.format("VisibilityButton_Hidden_Label"), icons.load<Texture2D>("BlueprintHiddenIcon"), l10n.format("VisibilityButton_Hidden_Tooltip")),
				new GUIContent(l10n.format("PreferencesButton_Favorited_Label"), icons.load<Texture2D>("FavoriteBlueprintIcon"), l10n.format("PreferencesButton_Favorited_Tooltip"))
				);
			preferencesButton.PositionOffset_Y = -30;
			preferencesButton.PositionScale_Y = 1.0f;
			preferencesButton.SizeScale_X = 1.0f;
			preferencesButton.SizeOffset_Y = 30;
			preferencesButton.onSwappedState += OnSwappedPreferencesState;
			preferencesButton.UseContentTooltip = true;
			preferencesButton.button.iconColor = ESleekTint.FOREGROUND;
			AddChild(preferencesButton);

			summaryContainer = Glazier.Get().CreateBox();
			summaryContainer.SizeScale_X = 1.0f;
			summaryContainer.AllowRichText = true;
			summaryContainer.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			summaryContainer.UseManualLayout = useManualLayout;
			summaryContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(summaryContainer);

			titleLabel = Glazier.Get().CreateLabel();
			titleLabel.SizeScale_X = 1.0f;
			titleLabel.SizeOffset_Y = 40;
			titleLabel.FontSize = ESleekFontSize.Medium;
			titleLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			titleLabel.AllowRichText = true;
			titleLabel.TextColor = ESleekTint.FONT;
			summaryContainer.AddChild(titleLabel);

			primaryItemIcon = new SleekItemIcon();
			primaryItemIcon.PositionOffset_X = -100;
			primaryItemIcon.PositionScale_X = 0.5f;
			primaryItemIcon.PositionOffset_Y = 40;
			primaryItemIcon.SizeOffset_X = 200;
			summaryContainer.AddChild(primaryItemIcon);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.SizeScale_X = 1.0f;
			descriptionLabel.SizeOffset_Y = 40;
			descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			descriptionLabel.AllowRichText = true;
			descriptionLabel.TextColor = ESleekTint.FONT;
			summaryContainer.AddChild(descriptionLabel);

			if (!useManualLayout)
			{
				extendedDescriptionBox = Glazier.Get().CreateBox();
				extendedDescriptionBox.UseManualLayout = useManualLayout;
				extendedDescriptionBox.UseChildAutoLayout = ESleekChildLayout.Vertical;
				extendedDescriptionBox.ChildAutoLayoutPadding = 5f;
				extendedDescriptionBox.AllowRichText = true;
				extendedDescriptionBox.TextAlignment = TextAnchor.UpperLeft;
				extendedDescriptionBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				extendedDescriptionBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				detailScrollView.AddChild(extendedDescriptionBox);
			}

			inputItemsContainer = Glazier.Get().CreateFrame();
			inputItemsContainer.SizeScale_X = 1.0f;
			inputItemsContainer.UseManualLayout = useManualLayout;
			inputItemsContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(inputItemsContainer);

			inputItemsLabel = Glazier.Get().CreateLabel();
			inputItemsLabel.SizeScale_X = 1.0f;
			inputItemsLabel.SizeOffset_Y = 40;
			inputItemsLabel.FontSize = ESleekFontSize.Medium;
			inputItemsLabel.Text = l10n.format("Details_InputItems");
			inputItemsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			inputItemsContainer.AddChild(inputItemsLabel);

			toolItemsLabel = Glazier.Get().CreateLabel();
			toolItemsLabel.SizeScale_X = 1.0f;
			toolItemsLabel.SizeOffset_Y = 40;
			toolItemsLabel.FontSize = ESleekFontSize.Medium;
			toolItemsLabel.Text = l10n.format("Details_ToolItems");
			toolItemsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			inputItemsContainer.AddChild(toolItemsLabel);

			outputItemsContainer = Glazier.Get().CreateFrame();
			outputItemsContainer.SizeScale_X = 1.0f;
			outputItemsContainer.UseManualLayout = useManualLayout;
			outputItemsContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(outputItemsContainer);

			ISleekLabel outputItemsLabel = Glazier.Get().CreateLabel();
			outputItemsLabel.SizeScale_X = 1.0f;
			outputItemsLabel.SizeOffset_Y = 40;
			outputItemsLabel.FontSize = ESleekFontSize.Medium;
			outputItemsLabel.Text = l10n.format("Details_OutputItems");
			outputItemsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			outputItemsContainer.AddChild(outputItemsLabel);

			skillContainer = Glazier.Get().CreateFrame();
			skillContainer.SizeScale_X = 1.0f;
			skillContainer.SizeOffset_Y = 70;
			skillContainer.UseManualLayout = useManualLayout;
			skillContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(skillContainer);

			ISleekLabel requiredSkillsLabel = Glazier.Get().CreateLabel();
			requiredSkillsLabel.SizeScale_X = 1.0f;
			requiredSkillsLabel.SizeOffset_Y = 40;
			requiredSkillsLabel.FontSize = ESleekFontSize.Medium;
			requiredSkillsLabel.Text = l10n.format("Details_RequiredSkills");
			requiredSkillsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			skillContainer.AddChild(requiredSkillsLabel);

			skillBox = Glazier.Get().CreateBox();
			skillBox.PositionOffset_Y = 40;
			skillBox.SizeScale_X = 1.0f;
			skillBox.SizeOffset_Y = 30;
			skillBox.FontSize = ESleekFontSize.Medium;
			skillBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			skillContainer.AddChild(skillBox);

			requiredTagsContainer = Glazier.Get().CreateFrame();
			requiredTagsContainer.SizeScale_X = 1f;
			requiredTagsContainer.UseManualLayout = useManualLayout;
			requiredTagsContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(requiredTagsContainer);

			ISleekLabel requiredTagsLabel = Glazier.Get().CreateLabel();
			requiredTagsLabel.SizeScale_X = 1.0f;
			requiredTagsLabel.SizeOffset_Y = 40;
			requiredTagsLabel.FontSize = ESleekFontSize.Medium;
			requiredTagsLabel.Text = l10n.format("Details_RequiredTags");
			requiredTagsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			requiredTagsContainer.AddChild(requiredTagsLabel);

			conditionsContainer = Glazier.Get().CreateFrame();
			conditionsContainer.SizeScale_X = 1f;
			conditionsContainer.UseManualLayout = useManualLayout;
			conditionsContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(conditionsContainer);

			ISleekLabel conditionsLabel = Glazier.Get().CreateLabel();
			conditionsLabel.SizeScale_X = 1.0f;
			conditionsLabel.SizeOffset_Y = 40;
			conditionsLabel.FontSize = ESleekFontSize.Medium;
			conditionsLabel.Text = l10n.format("Details_Conditions");
			conditionsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			conditionsContainer.AddChild(conditionsLabel);

			conditionsElementsContainer = Glazier.Get().CreateFrame();
			conditionsElementsContainer.PositionOffset_Y = 40;
			conditionsElementsContainer.SizeScale_X = 1f;
			conditionsContainer.AddChild(conditionsElementsContainer);

			rewardsContainer = Glazier.Get().CreateFrame();
			rewardsContainer.SizeScale_X = 1f;
			rewardsContainer.UseManualLayout = useManualLayout;
			rewardsContainer.UseHeightLayoutOverride = true;
			detailScrollView.AddChild(rewardsContainer);

			ISleekLabel rewardsLabel = Glazier.Get().CreateLabel();
			rewardsLabel.SizeScale_X = 1.0f;
			rewardsLabel.SizeOffset_Y = 40;
			rewardsLabel.FontSize = ESleekFontSize.Medium;
			rewardsLabel.Text = l10n.format("Details_Rewards");
			rewardsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			rewardsContainer.AddChild(rewardsLabel);

			rewardsElementsContainer = Glazier.Get().CreateFrame();
			rewardsElementsContainer.PositionOffset_Y = 40;
			rewardsElementsContainer.SizeScale_X = 1f;
			rewardsContainer.AddChild(rewardsElementsContainer);
		}

		private BlueprintStatus status;

		private ISleekScrollView detailScrollView;
		private SleekButtonIcon craftButton;
		private SleekButtonState preferencesButton;

		private ISleekBox summaryContainer;
		private ISleekLabel titleLabel;
		private SleekItemIcon primaryItemIcon;
		private ISleekLabel descriptionLabel;
		private ISleekBox extendedDescriptionBox;

		private ISleekElement inputItemsContainer;
		private ISleekLabel inputItemsLabel;
		private ISleekLabel toolItemsLabel;
		private ISleekElement outputItemsContainer;

		private ISleekElement skillContainer;
		private ISleekBox skillBox;

		private ISleekElement requiredTagsContainer;
		private List<SleekSelectedBlueprintRequiredTag> requiredTags = new List<SleekSelectedBlueprintRequiredTag>();

		private ISleekElement conditionsContainer;
		private ISleekElement conditionsElementsContainer;
		private ISleekElement rewardsContainer;
		private ISleekElement rewardsElementsContainer;

		private List<int> consumingInputIndices = new List<int>();
		private List<int> nonConsumingInputIndices = new List<int>();
		private SleekSelectedBlueprintItem targetItemWidget;
		private List<SleekSelectedBlueprintItem> inputItemWidgets = new List<SleekSelectedBlueprintItem>();
		private List<SleekSelectedBlueprintItem> outputItemWidgets = new List<SleekSelectedBlueprintItem>();
	}
}
