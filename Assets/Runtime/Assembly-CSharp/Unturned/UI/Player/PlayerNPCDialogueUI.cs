////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define LOG_NPC_DIALOGUE
#endif
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerNPCDialogueUI
	{
		private const string KEYWORD_PAUSE = "<pause>";

		private static SleekFullscreenBox container;
		private static Local localization;
		public static IconsBundle icons;

		public static bool active;
		private static DialogueAsset dialogue;
		private static DialogueMessage message;

		/// <summary>
		/// If true, the player can press Interact [F] when there are no responses
		/// and the "next" dialogue will be opened.
		/// </summary>
		private static bool hasNextDialogue;

		private static List<DialogueResponse> responses = new List<DialogueResponse>();

		private static ISleekBox dialogueBox;
		private static ISleekLabel characterLabel;
		private static ISleekLabel messageLabel;
		private static ISleekLabel pageLabel;
		private static ISleekScrollView responseBox;
		private static List<SleekButtonIcon> responseButtons = new List<SleekButtonIcon>();

		/// <summary>
		/// Each dialogue message is separated into multiple pages.
		/// </summary>
		private static int dialoguePageIndex;

		/// <summary>
		/// Current page localized text with name_npc and name_char formatted in.
		/// </summary>
		private static string pageFormattedText;

		/// <summary>
		/// Seconds elapsed while viewing current page not including pause timer.
		/// Used to gradually show the message text.
		/// </summary>
		private static float pageAnimationTime;

		/// <summary>
		/// Seconds to wait before resuming pageAnimationTime counting.
		/// </summary>
		private static float pauseTimer;

		/// <summary>
		/// Appends chars from pageFormattedText according to pageAnimationTime.
		/// </summary>
		private static StringBuilder animatedTextBuilder = new StringBuilder();

		/// <summary>
		/// Rich text formatting tags to close those opened by visible text in animatedTextBuilder.
		/// For example, if animatedTextBuilder includes an opening color=#, this includes the closing color markup.
		/// Required depending on Glazier used.
		/// </summary>
		private static string animatedTextClosingRichTags;

		/// <summary>
		/// Number of chars of pageFormattedText currently visible.
		/// </summary>
		private static int animatedCharsVisibleCount;

		/// <summary>
		/// Added to animation visible chars to skip time on markup.
		/// </summary>
		private static int pageAnimationTimeVisibleCharsOffset;

		/// <summary>
		/// Seconds elapsed since responses started becoming visible.
		/// Used to gradually enable responses rather than all at once.
		/// </summary>
		private static float responsesVisibleTime;

		/// <summary>
		/// Animated toward total number of responses to make them gradually visible.
		/// </summary>
		private static int visibleResponsesCount;

		/// <summary>
		/// If true, animation is finished and there is another page to show when Interact [F] is pressed.
		/// </summary>
		public static bool CanAdvanceToNextPage
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, text on current page is in the process of gradually appearing.
		/// </summary>
		public static bool IsDialogueAnimating
		{
			get;
			private set;
		}

		/// <summary>
		/// Used by quest UI to return to current dialogue.
		/// </summary>
		public static void OpenCurrentDialogue()
		{
			open(dialogue, message, hasNextDialogue);
		}

		public static void open(DialogueAsset newDialogue, DialogueMessage newMessage, bool newHasNextDialogue)
		{
			if (active)
			{
				updateDialogue(newDialogue, newMessage, newHasNextDialogue);

				return;
			}

			active = true;

			if (PlayerLifeUI.npc != null)
			{
				characterLabel.Text = PlayerLifeUI.npc.GetDialogueTargetNameShownToPlayer(Player.LocalPlayer);
			}
			else
			{
				characterLabel.Text = "null";
			}

			updateDialogue(newDialogue, newMessage, newHasNextDialogue);

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

		private static void AddDefaultGoodbyeResponse()
		{
			string text;
			Local levelLocal = Level.info?.getLocalization();
			if (levelLocal != null && levelLocal.has("DefaultGoodbyeResponse"))
			{
				text = levelLocal.format("DefaultGoodbyeResponse");
			}
			else
			{
				text = localization.format("Goodbye");
			}

			// Technically allows modders to disable goodbye per-level if they really want.
			// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2952
			if (!string.IsNullOrEmpty(text))
			{
				responses.Add(new DialogueResponse(0, null, 0, default, 0, default, 0, default, text, default, default));
			}
		}

		private static void updateDialogue(DialogueAsset newDialogue, DialogueMessage newMessage, bool newHasNextDialogue)
		{
#if LOG_NPC_DIALOGUE
			UnturnedLog.info($"Update dialogue: {newDialogue?.FriendlyName}, Message: {newMessage.index}, Has next: {newHasNextDialogue}");
#endif

			dialogue = newDialogue;
			message = newMessage;
			hasNextDialogue = newHasNextDialogue;

			if (dialogue == null)
			{
				return;
			}

			responseBox.IsVisible = false;
			responseBox.ContentSizeOffset = Vector2.zero; // Reset height in case anim does not update for zero responses.

			responses.Clear();
			dialogue.getAvailableResponses(Player.LocalPlayer, newMessage.index, responses);

			if (PlayerLifeUI.npc != null)
			{
				PlayerLifeUI.npc.SetFaceOverride(message.faceOverride);
			}

			if (responses.Count == 0 && !hasNextDialogue)
			{
				AddDefaultGoodbyeResponse();
			}

			responseBox.RemoveAllChildren();
			responseButtons.Clear();

			for (int responseIndex = 0; responseIndex < responses.Count; responseIndex++)
			{
				DialogueResponse response = responses[responseIndex];

				string text = response.text;
				text = text.Replace("<name_npc>", PlayerLifeUI.npc != null ? PlayerLifeUI.npc.GetDialogueTargetNameShownToPlayer(Player.LocalPlayer) : "null");
				text = text.Replace("<name_char>", Player.LocalPlayer.channel.owner.playerID.characterName);

				QuestAsset quest = response.FindQuestAsset();

				Texture2D icon = null;
				if (quest != null)
				{
					if (Player.LocalPlayer.quests.GetQuestStatus(quest) == ENPCQuestStatus.READY)
					{
						icon = icons.load<Texture2D>("Quest_End");
					}
					else
					{
						icon = icons.load<Texture2D>("Quest_Begin");
					}
				}
				else if (!response.IsVendorRefNull())
				{
					icon = icons.load<Texture2D>("Vendor");
				}

				SleekButtonIcon responseButton = new SleekButtonIcon(icon);
				responseButton.PositionOffset_Y = responseIndex * 30;
				responseButton.SizeOffset_Y = 30;
				responseButton.SizeScale_X = 1;
				responseButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
				responseButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
				responseButton.enableRichText = true;
				responseButton.text = text;
				responseButton.onClickedButton += onClickedResponseButton;
				responseBox.AddChild(responseButton);
				responseButton.IsVisible = false;
				responseButtons.Add(responseButton);
			}

			dialoguePageIndex = 0;
			UpdatePage();
		}

		/// <summary>
		/// Update timers and UI for current page index.
		/// </summary>
		private static void UpdatePage()
		{
			messageLabel.Text = string.Empty;
			pageLabel.IsVisible = false;

			pageAnimationTime = 0.0f;
			pauseTimer = 0.0f;
			animatedTextBuilder.Length = 0;
			animatedTextClosingRichTags = string.Empty;
			animatedCharsVisibleCount = 0;
			pageAnimationTimeVisibleCharsOffset = 0;

			responsesVisibleTime = 0.0f;
			visibleResponsesCount = 0;

			IsDialogueAnimating = true;
			CanAdvanceToNextPage = false;

			if (message != null && message.pages != null && dialoguePageIndex < message.pages.Length)
			{
				pageFormattedText = message.pages[dialoguePageIndex].text;
				pageFormattedText = pageFormattedText.Replace("<name_npc>", PlayerLifeUI.npc != null ? PlayerLifeUI.npc.GetDialogueTargetNameShownToPlayer(Player.LocalPlayer) : "null");
				pageFormattedText = pageFormattedText.Replace("<name_char>", Player.LocalPlayer.channel.owner.playerID.characterName);
			}
			else
			{
				pageFormattedText = "?";
			}

			if (OptionsSettings.talk)
			{
				SkipAnimation();
			}
		}

		private static bool DoNextCharsMatchKeyword(string text, int index, string keyword)
		{
			if (index + keyword.Length > text.Length)
			{
				return false;
			}

			for (int keywordIndex = 0; keywordIndex < keyword.Length; keywordIndex++)
			{
				if (text[index + keywordIndex] != keyword[keywordIndex])
				{
					return false;
				}
			}

			return true;
		}

		private static bool FindNextRichTextMarkupSpan(string text, int index, out int begin, out int end)
		{
			begin = -1;
			end = -1;

			while (index < text.Length)
			{
				if (text[index] == '<')
				{
					if (begin == -1)
					{
						begin = index;
					}
				}
				else if (text[index] == '>')
				{
					if (index == text.Length - 1 || text[index + 1] != '<')
					{
						end = index;
						return begin >= 0;
					}
				}

				index++;
			}

			return false;
		}

		public static void AdvancePage()
		{
			if (dialoguePageIndex == message.pages.Length - 1)
			{
				Player.LocalPlayer.quests.ClientChooseDefaultNextDialogue(dialogue.GUID, message.index);
			}
			else
			{
				dialoguePageIndex++;
				UpdatePage();
			}
		}

		private static void OnPageAnimationFinished()
		{
			IsDialogueAnimating = false;

			if (message != null && message.pages != null)
			{
				if (dialoguePageIndex < message.pages.Length - 1)
				{
					CanAdvanceToNextPage = true;

					pageLabel.Text = localization.format("Page", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
					pageLabel.IsVisible = true;
				}
				else if (dialoguePageIndex == message.pages.Length - 1 && hasNextDialogue)
				{
					CanAdvanceToNextPage = true;

					pageLabel.Text = localization.format("Page", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
					pageLabel.IsVisible = true;

					responseBox.IsVisible = true;
				}
				else
				{
					responseBox.IsVisible = true;
				}
			}
			else
			{
				responseBox.IsVisible = true;
			}
		}

		/// <summary>
		/// Show complete text for the current page and make responses visible.
		/// Called if dialogue animation is disabled, and when the player presses Interact [F] during animation.
		/// </summary>
		public static void SkipAnimation()
		{
			messageLabel.Text = pageFormattedText.Replace("<pause>", "");

			visibleResponsesCount = responses.Count;
			for (int index = 0; index < responses.Count; index++)
			{
				responseButtons[index].IsVisible = true;
			}
			responseBox.ContentSizeOffset = new Vector2(0.0f, responses.Count * 30);

			OnPageAnimationFinished();
		}

		/// <summary>
		/// Called when the player presses Interact [F] in dialogue screen.
		/// </summary>
		internal static void HandleInteractPressed()
		{
			if (IsDialogueAnimating || visibleResponsesCount < responses.Count)
			{
				SkipAnimation();
				return;
			}

			if (!OptionsSettings.talk && responsesVisibleTime < 0.15f)
			{
				// If player has dialogue animation enabled and pressed "skip" within 150 ms of
				// the animation ending we don't advance in case it was an attempt to skip.
				return;
			}

			if (CanAdvanceToNextPage)
			{
				AdvancePage();
			}
			else
			{
				close();

				PlayerLifeUI.open();
			}
		}

		// animates text
		public static void UpdateAnimation()
		{
			if (dialogue == null)
			{
				return;
			}

			if (IsDialogueAnimating)
			{
				if (pauseTimer > 0)
				{
					pauseTimer -= Time.deltaTime;
				}
				else
				{
					pageAnimationTime += Time.deltaTime;
				}
				int newLength = Mathf.Min(pageFormattedText.Length, Mathf.CeilToInt(pageAnimationTime * 30.0f) + pageAnimationTimeVisibleCharsOffset);

				if (animatedCharsVisibleCount != newLength)
				{
					while (animatedCharsVisibleCount < pageFormattedText.Length && animatedCharsVisibleCount < newLength)
					{
						char character = pageFormattedText[animatedCharsVisibleCount];

						if (character == '<')
						{
							if (animatedTextClosingRichTags.Length > 0)
							{
								newLength += animatedTextClosingRichTags.Length;
								animatedCharsVisibleCount += animatedTextClosingRichTags.Length;
								pageAnimationTimeVisibleCharsOffset += animatedTextClosingRichTags.Length;

								animatedTextBuilder.Append(animatedTextClosingRichTags);
								animatedTextClosingRichTags = string.Empty;
							}
							else
							{
								if (DoNextCharsMatchKeyword(pageFormattedText, animatedCharsVisibleCount, KEYWORD_PAUSE))
								{
									pauseTimer += 0.5f;

									newLength = animatedCharsVisibleCount + KEYWORD_PAUSE.Length;
									animatedCharsVisibleCount = newLength;
									pageAnimationTimeVisibleCharsOffset += KEYWORD_PAUSE.Length - 1;
								}
								else
								{
									int tagsBegin;
									int tagsEnd;
									if (FindNextRichTextMarkupSpan(pageFormattedText, animatedCharsVisibleCount, out tagsBegin, out tagsEnd))
									{
										int diff = tagsEnd - tagsBegin + 1;

										newLength += diff;
										animatedCharsVisibleCount += diff;
										pageAnimationTimeVisibleCharsOffset += diff;

										animatedTextBuilder.Append(pageFormattedText.Substring(tagsBegin, diff));

										if (FindNextRichTextMarkupSpan(pageFormattedText, tagsEnd + 1, out tagsBegin, out tagsEnd))
										{
											diff = tagsEnd - tagsBegin + 1;
											animatedTextClosingRichTags = pageFormattedText.Substring(tagsBegin, diff);
										}
									}
									else
									{
										// Did not find closing '>' for current '<'
										animatedTextBuilder.Append(character);
										animatedCharsVisibleCount++;
									}
								}
							}
						}
						else
						{
							animatedTextBuilder.Append(character);
							animatedCharsVisibleCount++;
						}
					}

					messageLabel.Text = animatedTextBuilder.ToString() + animatedTextClosingRichTags;

					if (animatedCharsVisibleCount == pageFormattedText.Length)
					{
						OnPageAnimationFinished();
					}
				}
			}
			else
			{
				responsesVisibleTime += Time.deltaTime;
				int newVisibleResponsesCount = Mathf.Min(responses.Count, Mathf.FloorToInt(responsesVisibleTime * 10.0f));

				if (visibleResponsesCount != newVisibleResponsesCount)
				{
					while (visibleResponsesCount < newVisibleResponsesCount)
					{
						responseButtons[visibleResponsesCount].IsVisible = true;
						responseBox.ContentSizeOffset = new Vector2(0.0f, newVisibleResponsesCount * 30);

						visibleResponsesCount++;
					}
				}
			}
		}

		private static void onClickedResponseButton(ISleekElement button)
		{
			// Disable to prevent double-click local mis-prediction.
			SetResponseButtonsAreClickable(false);

			int buttonIndex = responseBox.FindIndexOfChild(button);
			DialogueResponse response = responses[buttonIndex];

			QuestAsset newQuest = response.FindQuestAsset();
			if (newQuest != null)
			{
				close();

				// Response isn't chosen until player clicks "continue" in quest UI,
				// otherwise we return to the current dialogue.
				PlayerNPCQuestUI.open(newQuest, dialogue, message, response, Player.LocalPlayer.quests.GetQuestStatus(newQuest) == ENPCQuestStatus.READY ? EQuestViewMode.END : EQuestViewMode.BEGIN);
			}
			else
			{
				DialogueAsset newDialogue = response.FindDialogueAsset();
				VendorAsset newVendor = response.FindVendorAsset();
				// If both are null the server will not send a response to choosing this response.
				if (newDialogue == null && newVendor == null)
				{
					close();

					PlayerLifeUI.open();
				}

				// Nelson 2023-10-13: send RPC because some responses have no dialogue/vendor/quest but grant rewards!
				Player.LocalPlayer.quests.ClientChooseDialogueResponse(dialogue.GUID, message.index, response.index);
			}
		}

		private static void SetResponseButtonsAreClickable(bool clickable)
		{
			foreach (SleekButtonIcon button in responseButtons)
			{
				button.isClickable = clickable;
			}
		}

		public PlayerNPCDialogueUI()
		{
			localization = Localization.read("/Player/PlayerNPCDialogue.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerNPCDialogue");

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

			dialogueBox = Glazier.Get().CreateBox();
			dialogueBox.PositionOffset_X = -250;
			dialogueBox.PositionOffset_Y = -200;
			dialogueBox.PositionScale_X = 0.5f;
			dialogueBox.PositionScale_Y = 0.85f;
			dialogueBox.SizeOffset_X = 500;
			dialogueBox.SizeOffset_Y = 100;
			container.AddChild(dialogueBox);

			characterLabel = Glazier.Get().CreateLabel();
			characterLabel.PositionOffset_X = 5;
			characterLabel.PositionOffset_Y = 5;
			characterLabel.SizeOffset_X = -10;
			characterLabel.SizeOffset_Y = 30;
			characterLabel.SizeScale_X = 1.0f;
			characterLabel.TextAlignment = TextAnchor.UpperLeft;
			characterLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			characterLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			characterLabel.AllowRichText = true;
			characterLabel.FontSize = ESleekFontSize.Medium;
			dialogueBox.AddChild(characterLabel);

			messageLabel = Glazier.Get().CreateLabel();
			messageLabel.PositionOffset_X = 5;
			messageLabel.PositionOffset_Y = 30;
			messageLabel.SizeOffset_X = -10;
			messageLabel.SizeOffset_Y = -35;
			messageLabel.SizeScale_X = 1.0f;
			messageLabel.SizeScale_Y = 1.0f;
			messageLabel.TextAlignment = TextAnchor.UpperLeft;
			messageLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			messageLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			messageLabel.AllowRichText = true;
			dialogueBox.AddChild(messageLabel);

			pageLabel = Glazier.Get().CreateLabel();
			pageLabel.PositionOffset_X = -30;
			pageLabel.PositionOffset_Y = -30;
			pageLabel.PositionScale_X = 1.0f;
			pageLabel.PositionScale_Y = 1.0f;
			pageLabel.SizeOffset_X = 30;
			pageLabel.SizeOffset_Y = 30;
			pageLabel.TextAlignment = TextAnchor.LowerRight;
			dialogueBox.AddChild(pageLabel);

			responseBox = Glazier.Get().CreateScrollView();
			responseBox.PositionOffset_X = -250;
			responseBox.PositionOffset_Y = -100;
			responseBox.PositionScale_X = 0.5f;
			responseBox.PositionScale_Y = 0.85f;
			responseBox.SizeOffset_X = 500;
			responseBox.SizeScale_Y = 0.15f;
			responseBox.ScaleContentToWidth = true;
			container.AddChild(responseBox);
			responseBox.IsVisible = false;
			responseButtons.Clear();
		}
	}
}
