////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum EQuestViewMode
	{
		BEGIN, // accepting quest
		END, // completing quest
		DETAILS // reviewing info
	}

	public class PlayerNPCQuestUI
	{
		private static SleekFullscreenBox container;
		public static Local localization;
		public static IconsBundle icons;

		public static bool active;

		public static void open(QuestAsset newQuest, DialogueAsset newDialogueContext, DialogueMessage newDialogueMessageContext, DialogueResponse newPendingResponse, EQuestViewMode newMode)
		{
			if (active)
			{
				return;
			}

			active = true;
			updateQuest(newQuest, newDialogueContext, newDialogueMessageContext, newPendingResponse, newMode);

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, 1);
		}

		public static void closeNicely()
		{
			close();

			if (mode == EQuestViewMode.BEGIN || mode == EQuestViewMode.END)
			{
				// Cancel response, re-open dialogue we opened the quest UI from.
				PlayerNPCDialogueUI.OpenCurrentDialogue();
			}
			else if (mode == EQuestViewMode.DETAILS)
			{
				PlayerDashboardInventoryUI.active = false;
				PlayerDashboardCraftingUI.active = false;
				PlayerDashboardSkillsUI.active = false;
				PlayerDashboardInformationUI.active = true;

				PlayerDashboardUI.open();
			}
		}

		private static void updateQuest(QuestAsset newQuest, DialogueAsset newDialogueContext, DialogueMessage newDialogueMessageContext, DialogueResponse newPendingResponse, EQuestViewMode newMode)
		{
			quest = newQuest;
			pendingResponse = newPendingResponse;
			dialogueContext = newDialogueContext;
			dialogueMessageContext = newDialogueMessageContext;
			mode = newMode;

			if (quest == null)
			{
				return;
			}

			beginContainer.IsVisible = mode == EQuestViewMode.BEGIN;
			endContainer.IsVisible = mode == EQuestViewMode.END;
			detailsContainer.IsVisible = mode == EQuestViewMode.DETAILS;

			// Re-enable buttons. (disabled when clicked)
			SetButtonsAreClickable(true);

			if (mode == EQuestViewMode.DETAILS)
			{
				if (Player.LocalPlayer.quests.GetTrackedQuest() == quest)
				{
					trackButton.Text = localization.format("Track_Off");
				}
				else
				{
					trackButton.Text = localization.format("Track_On");
				}
			}

			nameLabel.Text = quest.questName;
			descriptionLabel.Text = quest.questDescription;

			float conditionsAndRewardsContentHeight = 0;

			if (quest.conditions != null && quest.conditions.Length > 0)
			{
				conditionsLabel.IsVisible = true;
				conditionsContainer.IsVisible = true;

				conditionsContainer.RemoveAllChildren();
				float conditionsOffset = 0;

				areConditionsMet.Clear();
				foreach (INPCCondition condition in quest.conditions)
				{
					areConditionsMet.Add(condition.isConditionMet(Player.LocalPlayer));
				}

				for (int conditionIndex = 0; conditionIndex < quest.conditions.Length; conditionIndex++)
				{
					INPCCondition condition = quest.conditions[conditionIndex];
					if (!condition.AreUIRequirementsMet(areConditionsMet))
					{
						// This condition should not yet be visible.
						continue;
					}

					bool isComplete = areConditionsMet[conditionIndex];
					Texture2D icon = null;
					if (mode != EQuestViewMode.BEGIN)
					{
						if (isComplete)
						{
							icon = icons.load<Texture2D>("Complete");
						}
						else
						{
							icon = icons.load<Texture2D>("Incomplete");
						}
					}

					ISleekElement conditionUI = condition.createUI(Player.LocalPlayer, icon);

					if (conditionUI == null)
					{
						continue;
					}

					conditionUI.PositionOffset_Y = conditionsOffset;
					conditionsContainer.AddChild(conditionUI);

					conditionsOffset += conditionUI.SizeOffset_Y;
				}

				conditionsContainer.SizeOffset_Y = conditionsOffset;

				conditionsAndRewardsContentHeight += conditionsLabel.SizeOffset_Y;
				conditionsAndRewardsContentHeight += conditionsContainer.SizeOffset_Y;
			}
			else
			{
				conditionsLabel.IsVisible = false;
				conditionsContainer.IsVisible = false;
			}

			if (quest.rewards != null && quest.rewards.Length > 0)
			{
				rewardsLabel.IsVisible = true;
				rewardsContainer.IsVisible = true;

				rewardsContainer.RemoveAllChildren();
				float rewardsOffset = 0;
				for (int rewardIndex = 0; rewardIndex < quest.rewards.Length; rewardIndex++)
				{
					INPCReward reward = quest.rewards[rewardIndex];

					ISleekElement rewardUI = reward.createUI(Player.LocalPlayer);

					if (rewardUI == null)
					{
						continue;
					}

					rewardUI.PositionOffset_Y = rewardsOffset;
					rewardsContainer.AddChild(rewardUI);

					rewardsOffset += rewardUI.SizeOffset_Y;
				}

				rewardsLabel.PositionOffset_Y = conditionsAndRewardsContentHeight;
				conditionsAndRewardsContentHeight += rewardsLabel.SizeOffset_Y;

				rewardsContainer.PositionOffset_Y = conditionsAndRewardsContentHeight;
				rewardsContainer.SizeOffset_Y = rewardsOffset;
				conditionsAndRewardsContentHeight += rewardsContainer.SizeOffset_Y;
			}
			else
			{
				rewardsLabel.IsVisible = false;
				rewardsContainer.IsVisible = false;
			}

			conditionsAndRewardsScrollView.ContentSizeOffset = new Vector2(0.0f, conditionsAndRewardsContentHeight);

			// Conditions and rewards scroll view is contained within detailsBox, inset from the edges.
			const int SCREEN_PADDING = 10; // Vertical space at top and bottom of screen.
			float spaceAvailable = Screen.height / GraphicsSettings.userInterfaceScale;
			spaceAvailable -= SCREEN_PADDING; // Top of screen.
			spaceAvailable -= LOWER_BUTTONS_VERTICAL_OFFSET; // Offset between bottom of questBox and lower buttons.
			spaceAvailable -= LOWER_BUTTONS_HEIGHT; // Height of beginContainer, endContainer, and detailsContainer.
			spaceAvailable -= SCREEN_PADDING; // Bottom of screen.

			float questBoxOccupiedHeight = (conditionsAndRewardsScrollView.PositionOffset_Y
					+ conditionsAndRewardsContentHeight
					+ QUEST_BOX_INNER_SPACING);

			if (questBoxOccupiedHeight >= spaceAvailable)
			{
				questBox.PositionOffset_Y = 0;
				questBox.PositionScale_Y = 0.0f;
				questBox.SizeOffset_Y = -LOWER_BUTTONS_HEIGHT - LOWER_BUTTONS_VERTICAL_OFFSET;
				questBox.SizeScale_Y = 1.0f;
			}
			else
			{
				questBox.PositionOffset_Y = (questBoxOccupiedHeight * -0.5f) - ((LOWER_BUTTONS_VERTICAL_OFFSET + LOWER_BUTTONS_HEIGHT) / 2);
				questBox.PositionScale_Y = 0.5f;
				questBox.SizeOffset_Y = questBoxOccupiedHeight;
				questBox.SizeScale_Y = 0.0f;
			}
		}

		private static void onClickedAcceptButton(ISleekElement button)
		{
			// Disable buttons to prevent double-click local mis-prediction.
			SetButtonsAreClickable(false);

			Player.LocalPlayer.quests.ClientChooseDialogueResponse(dialogueContext.GUID, dialogueMessageContext.index, pendingResponse.index);
		}

		private static void onClickedDeclineButton(ISleekElement button)
		{
			// Disable buttons to prevent double-click local mis-prediction.
			SetButtonsAreClickable(false);

			close();

			// Cancel response, re-open dialogue we opened the quest UI from.
			PlayerNPCDialogueUI.OpenCurrentDialogue();
		}

		private static void onClickedContinueButton(ISleekElement button)
		{
			// Disable buttons to prevent double-click local mis-prediction.
			SetButtonsAreClickable(false);

			Player.LocalPlayer.quests.ClientChooseDialogueResponse(dialogueContext.GUID, dialogueMessageContext.index, pendingResponse.index);
		}

		private static void onClickedTrackButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.ClientTrackQuest(quest);
			if (!Provider.isServer)
			{
				Player.LocalPlayer.quests.TrackQuest(quest);
			}

			closeNicely();
		}

		private static void onClickedAbandonButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.ClientAbandonQuest(quest);

			closeNicely();
		}

		private static void onClickedReturnButton(ISleekElement button)
		{
			closeNicely();
		}

		private static void SetButtonsAreClickable(bool isClickable)
		{
			acceptButton.IsClickable = isClickable;
			declineButton.IsClickable = isClickable;
			continueButton.IsClickable = isClickable;
		}

		public PlayerNPCQuestUI()
		{
			localization = Localization.read("/Player/PlayerNPCQuest.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerNPCQuest");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;

			questBox = Glazier.Get().CreateBox();
			questBox.PositionOffset_X = -250;
			questBox.PositionScale_X = 0.5f;
			questBox.SizeOffset_X = 500;
			container.AddChild(questBox);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = QUEST_BOX_INNER_SPACING;
			nameLabel.PositionOffset_Y = QUEST_BOX_INNER_SPACING;
			nameLabel.SizeOffset_X = QUEST_BOX_INNER_SPACING * -2;
			nameLabel.SizeOffset_Y = 30;
			nameLabel.SizeScale_X = 1.0f;
			nameLabel.TextAlignment = TextAnchor.UpperLeft;
			nameLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			nameLabel.AllowRichText = true;
			nameLabel.FontSize = ESleekFontSize.Medium;
			questBox.AddChild(nameLabel);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = QUEST_BOX_INNER_SPACING;
			descriptionLabel.PositionOffset_Y = nameLabel.SizeOffset_Y; // Ignore QUEST_BOX_INNER_SPACING to be closer.
			descriptionLabel.SizeOffset_X = QUEST_BOX_INNER_SPACING * -2;
			descriptionLabel.SizeOffset_Y = 70;
			descriptionLabel.SizeScale_X = 1.0f;
			descriptionLabel.TextAlignment = TextAnchor.UpperLeft;
			descriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			descriptionLabel.AllowRichText = true;
			questBox.AddChild(descriptionLabel);

			conditionsAndRewardsScrollView = Glazier.Get().CreateScrollView();
			conditionsAndRewardsScrollView.PositionOffset_X = QUEST_BOX_INNER_SPACING;
			conditionsAndRewardsScrollView.PositionOffset_Y = descriptionLabel.PositionOffset_Y + descriptionLabel.SizeOffset_Y + QUEST_BOX_INNER_SPACING;
			conditionsAndRewardsScrollView.SizeOffset_X = QUEST_BOX_INNER_SPACING * -2;
			conditionsAndRewardsScrollView.SizeOffset_Y = -conditionsAndRewardsScrollView.PositionOffset_Y - QUEST_BOX_INNER_SPACING;
			conditionsAndRewardsScrollView.SizeScale_X = 1.0f;
			conditionsAndRewardsScrollView.SizeScale_Y = 1.0f;
			conditionsAndRewardsScrollView.ScaleContentToWidth = true;
			questBox.AddChild(conditionsAndRewardsScrollView);

			conditionsLabel = Glazier.Get().CreateLabel();
			conditionsLabel.SizeOffset_Y = 30;
			conditionsLabel.SizeScale_X = 1.0f;
			conditionsLabel.TextAlignment = TextAnchor.MiddleLeft;
			conditionsLabel.Text = localization.format("Conditions");
			conditionsLabel.FontSize = ESleekFontSize.Medium;
			conditionsAndRewardsScrollView.AddChild(conditionsLabel);

			conditionsContainer = Glazier.Get().CreateFrame();
			conditionsContainer.PositionOffset_Y = 30;
			conditionsContainer.SizeScale_X = 1.0f;
			conditionsAndRewardsScrollView.AddChild(conditionsContainer);

			rewardsLabel = Glazier.Get().CreateLabel();
			rewardsLabel.SizeOffset_Y = 30;
			rewardsLabel.SizeScale_X = 1.0f;
			rewardsLabel.TextAlignment = TextAnchor.MiddleLeft;
			rewardsLabel.Text = localization.format("Rewards");
			rewardsLabel.FontSize = ESleekFontSize.Medium;
			conditionsAndRewardsScrollView.AddChild(rewardsLabel);

			rewardsContainer = Glazier.Get().CreateFrame();
			rewardsContainer.SizeScale_X = 1.0f;
			conditionsAndRewardsScrollView.AddChild(rewardsContainer);

			beginContainer = Glazier.Get().CreateFrame();
			beginContainer.PositionOffset_Y = LOWER_BUTTONS_VERTICAL_OFFSET;
			beginContainer.PositionScale_Y = 1.0f;
			beginContainer.SizeOffset_Y = LOWER_BUTTONS_HEIGHT;
			beginContainer.SizeScale_X = 1.0f;
			questBox.AddChild(beginContainer);
			beginContainer.IsVisible = false;

			endContainer = Glazier.Get().CreateFrame();
			endContainer.PositionOffset_Y = LOWER_BUTTONS_VERTICAL_OFFSET;
			endContainer.PositionScale_Y = 1.0f;
			endContainer.SizeOffset_Y = LOWER_BUTTONS_HEIGHT;
			endContainer.SizeScale_X = 1.0f;
			questBox.AddChild(endContainer);
			endContainer.IsVisible = false;

			detailsContainer = Glazier.Get().CreateFrame();
			detailsContainer.PositionOffset_Y = LOWER_BUTTONS_VERTICAL_OFFSET;
			detailsContainer.PositionScale_Y = 1.0f;
			detailsContainer.SizeOffset_Y = LOWER_BUTTONS_HEIGHT;
			detailsContainer.SizeScale_X = 1.0f;
			questBox.AddChild(detailsContainer);
			detailsContainer.IsVisible = false;

			acceptButton = Glazier.Get().CreateButton();
			acceptButton.SizeOffset_X = -5;
			acceptButton.SizeScale_X = 0.5f;
			acceptButton.SizeScale_Y = 1.0f;
			acceptButton.Text = localization.format("Accept");
			acceptButton.TooltipText = localization.format("Accept_Tooltip");
			acceptButton.FontSize = ESleekFontSize.Medium;
			acceptButton.OnClicked += onClickedAcceptButton;
			beginContainer.AddChild(acceptButton);

			declineButton = Glazier.Get().CreateButton();
			declineButton.PositionOffset_X = 5;
			declineButton.PositionScale_X = 0.5f;
			declineButton.SizeOffset_X = -5;
			declineButton.SizeScale_X = 0.5f;
			declineButton.SizeScale_Y = 1.0f;
			declineButton.Text = localization.format("Decline");
			declineButton.TooltipText = localization.format("Decline_Tooltip");
			declineButton.FontSize = ESleekFontSize.Medium;
			declineButton.OnClicked += onClickedDeclineButton;
			beginContainer.AddChild(declineButton);

			continueButton = Glazier.Get().CreateButton();
			continueButton.SizeScale_X = 1.0f;
			continueButton.SizeScale_Y = 1.0f;
			continueButton.Text = localization.format("Continue");
			continueButton.TooltipText = localization.format("Continue_Tooltip");
			continueButton.FontSize = ESleekFontSize.Medium;
			continueButton.OnClicked += onClickedContinueButton;
			endContainer.AddChild(continueButton);

			trackButton = Glazier.Get().CreateButton();
			trackButton.SizeOffset_X = -5;
			trackButton.SizeScale_X = 0.333f;
			trackButton.SizeScale_Y = 1.0f;
			trackButton.TooltipText = localization.format("Track_Tooltip");
			trackButton.FontSize = ESleekFontSize.Medium;
			trackButton.OnClicked += onClickedTrackButton;
			detailsContainer.AddChild(trackButton);

			abandonButton = Glazier.Get().CreateButton();
			abandonButton.PositionOffset_X = 5;
			abandonButton.PositionScale_X = 0.333f;
			abandonButton.SizeOffset_X = -10;
			abandonButton.SizeScale_X = 0.333f;
			abandonButton.SizeScale_Y = 1.0f;
			abandonButton.Text = localization.format("Abandon");
			abandonButton.TooltipText = localization.format("Abandon_Tooltip");
			abandonButton.FontSize = ESleekFontSize.Medium;
			abandonButton.OnClicked += onClickedAbandonButton;
			detailsContainer.AddChild(abandonButton);

			returnButton = Glazier.Get().CreateButton();
			returnButton.PositionOffset_X = 5;
			returnButton.PositionScale_X = 0.667f;
			returnButton.SizeOffset_X = -5;
			returnButton.SizeScale_X = 0.333f;
			returnButton.SizeScale_Y = 1.0f;
			returnButton.Text = localization.format("Return");
			returnButton.TooltipText = localization.format("Return_Tooltip");
			returnButton.FontSize = ESleekFontSize.Medium;
			returnButton.OnClicked += onClickedReturnButton;
			detailsContainer.AddChild(returnButton);
		}

		private static QuestAsset quest;

		/// <summary>
		/// Valid when opened in Begin or End mode.
		/// 
		/// If the quest is ready to complete the UI is opened in End mode to allow
		/// the player to see what rewards they will receive after clicking continue. 
		/// Otherwise, in Begin mode the UI is opened to allow the player to review
		/// the conditions before accepting or declining the request.
		///
		/// If the player cancels the pending response is NOT chosen.
		/// </summary>
		private static DialogueResponse pendingResponse;

		/// <summary>
		/// Valid when opened in Begin or End mode.
		/// The player clicked pendingResponse in this dialogue to open the quest UI.
		/// </summary>
		private static DialogueAsset dialogueContext;
		private static DialogueMessage dialogueMessageContext;

		private static EQuestViewMode mode;

		private static ISleekBox questBox;
		private static ISleekLabel nameLabel;
		private static ISleekLabel descriptionLabel;
		private static ISleekScrollView conditionsAndRewardsScrollView;
		private static ISleekLabel conditionsLabel;
		private static ISleekElement conditionsContainer;
		private static ISleekLabel rewardsLabel;
		private static ISleekElement rewardsContainer;

		private static ISleekElement beginContainer;
		private static ISleekButton acceptButton;
		private static ISleekButton declineButton;
		private static ISleekElement endContainer;
		private static ISleekButton continueButton;
		private static ISleekElement detailsContainer;
		private static ISleekButton trackButton;
		private static ISleekButton abandonButton;
		private static ISleekButton returnButton;

		private const int LOWER_BUTTONS_HEIGHT = 50; // Height of beginContainer, endContainer, and detailsContainer.
		private const int LOWER_BUTTONS_VERTICAL_OFFSET = 10; // Offset between bottom of questBox and lower buttons.
		private const int QUEST_BOX_INNER_SPACING = 5; // Distance between questBox and inner labels and scroll view.

		private static List<bool> areConditionsMet = new List<bool>(8);
	}
}
