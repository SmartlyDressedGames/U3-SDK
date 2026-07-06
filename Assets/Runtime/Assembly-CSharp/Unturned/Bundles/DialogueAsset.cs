////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class DialogueAsset : Asset
	{
		public DialogueMessage[] messages
		{
			get;
			protected set;
		}

		public DialogueResponse[] responses
		{
			get;
			protected set;
		}

		public override EAssetType assetCategory => EAssetType.NPC;

		public DialogueMessage GetAvailableMessage(Player player)
		{
			for (int messageIndex = 0; messageIndex < messages.Length; messageIndex++)
			{
				DialogueMessage message = messages[messageIndex];

				if (message.areConditionsMet(player))
				{
					return message;
				}
			}

			return null;
		}

		internal void GetAllResponsesForMessage(int messageIndex, List<DialogueResponse> messageResponses)
		{
			DialogueMessage message = messages[messageIndex];
			if (message.responses != null && message.responses.Length > 0)
			{
				for (int responseIndex = 0; responseIndex < message.responses.Length; responseIndex++)
				{
					DialogueResponse response = responses[message.responses[responseIndex]];
					messageResponses.Add(response);
				}
			}
			else
			{
				for (int responseIndex = 0; responseIndex < responses.Length; responseIndex++)
				{
					DialogueResponse response = responses[responseIndex];

					if (response.messages != null && response.messages.Length > 0)
					{
						bool found = false;
						for (int index = 0; index < response.messages.Length; index++)
						{
							if (response.messages[index] == messageIndex)
							{
								found = true;
								break;
							}
						}

						if (!found)
						{
							continue;
						}
					}

					messageResponses.Add(response);
				}
			}
		}

		public void getAvailableResponses(Player player, int messageIndex, List<DialogueResponse> availableResponses)
		{
			DialogueMessage message = messages[messageIndex];
			if (message.responses != null && message.responses.Length > 0)
			{
				for (int responseIndex = 0; responseIndex < message.responses.Length; responseIndex++)
				{
					DialogueResponse response = responses[message.responses[responseIndex]];

					if (response.areConditionsMet(player))
					{
						availableResponses.Add(response);
					}
				}
			}
			else
			{
				for (int responseIndex = 0; responseIndex < responses.Length; responseIndex++)
				{
					DialogueResponse response = responses[responseIndex];

					if (response.messages != null && response.messages.Length > 0)
					{
						bool found = false;
						for (int index = 0; index < response.messages.Length; index++)
						{
							if (response.messages[index] == messageIndex)
							{
								found = true;
								break;
							}
						}

						if (!found)
						{
							continue;
						}
					}

					if (response.areConditionsMet(player))
					{
						availableResponses.Add(response);
					}
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 2000 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 2000");
			}

			int messageCount = p.data.ParseInt32("Messages");
			int responseCount = p.data.ParseUInt8("Responses");

			messages = new DialogueMessage[messageCount];
			for (byte messageIndex = 0; messageIndex < messages.Length; messageIndex++)
			{
				DialoguePage[] messagePages = new DialoguePage[p.data.ParseUInt8("Message_" + messageIndex + "_Pages")];
				for (byte pageIndex = 0; pageIndex < messagePages.Length; pageIndex++)
				{
					string pageText = p.localization.format("Message_" + messageIndex + "_Page_" + pageIndex);
					pageText = ItemTool.filterRarityRichText(pageText);
					RichTextUtil.replaceNewlineMarkup(ref pageText);

					if (string.IsNullOrEmpty(pageText))
					{
						throw new System.NotSupportedException("missing message " + messageIndex + " page " + pageIndex);
					}

					messagePages[pageIndex] = new DialoguePage(pageText);
				}

				byte[] messageResponses = new byte[p.data.ParseUInt8("Message_" + messageIndex + "_Responses")];
				for (byte responseIndex = 0; responseIndex < messageResponses.Length; responseIndex++)
				{
					string messageResponseKey = "Message_" + messageIndex + "_Response_" + responseIndex;
					messageResponses[responseIndex] = p.data.ParseUInt8(messageResponseKey);
					if (messageResponses[responseIndex] >= responseCount)
					{
						Assets.ReportError(this, "{0} out of bounds ({1})", messageResponseKey, responseCount);
					}
				}

				System.Guid prevGuid;
				ushort prev = p.data.ParseGuidOrLegacyId("Message_" + messageIndex + "_Prev", out prevGuid);

				byte? faceOverride;
				if (p.data.ContainsKey("Message_" + messageIndex + "_FaceOverride"))
				{
					faceOverride = p.data.ParseUInt8("Message_" + messageIndex + "_FaceOverride");
				}
				else
				{
					faceOverride = null;
				}

				NPCConditionsList messageConditionsList = new NPCConditionsList();
				messageConditionsList.Parse(p.data, p.localization, this, "Message_" + messageIndex + "_Conditions", "Message_" + messageIndex + "_Condition_");

				NPCRewardsList messageRewardsList = new NPCRewardsList();
				messageRewardsList.Parse(p.data, p.localization, this, "Message_" + messageIndex + "_Rewards", "Message_" + messageIndex + "_Reward_");

				messages[messageIndex] = new DialogueMessage(messageIndex, messagePages, messageResponses, prev, prevGuid, faceOverride, messageConditionsList, messageRewardsList);
			}

			responses = new DialogueResponse[responseCount];
			for (byte responseIndex = 0; responseIndex < responses.Length; responseIndex++)
			{
				byte[] responseMessages = new byte[p.data.ParseUInt8("Response_" + responseIndex + "_Messages")];
				for (byte messageIndex = 0; messageIndex < responseMessages.Length; messageIndex++)
				{
					string responseMessageKey = "Response_" + responseIndex + "_Message_" + messageIndex;
					responseMessages[messageIndex] = p.data.ParseUInt8(responseMessageKey);
					if (responseMessages[messageIndex] >= messageCount)
					{
						Assets.ReportError(this, "{0} out of bounds ({1})", responseMessageKey, messageCount);
					}
				}

				System.Guid responseDialogueGuid;
				System.Guid responseQuestGuid;
				System.Guid responseVendorGuid;
				ushort responseDialogue = p.data.ParseGuidOrLegacyId("Response_" + responseIndex + "_Dialogue", out responseDialogueGuid);
				ushort responseQuest = p.data.ParseGuidOrLegacyId("Response_" + responseIndex + "_Quest", out responseQuestGuid);
				ushort responseVendor = p.data.ParseGuidOrLegacyId("Response_" + responseIndex + "_Vendor", out responseVendorGuid);

				string responseText = p.localization.format("Response_" + responseIndex);
				responseText = ItemTool.filterRarityRichText(responseText);
				RichTextUtil.replaceNewlineMarkup(ref responseText);

				if (string.IsNullOrEmpty(responseText))
				{
					throw new System.NotSupportedException("missing response " + responseIndex);
				}

				NPCConditionsList responseConditionsList = new NPCConditionsList();
				responseConditionsList.Parse(p.data, p.localization, this, "Response_" + responseIndex + "_Conditions", "Response_" + responseIndex + "_Condition_");

				NPCRewardsList responseRewardsList = new NPCRewardsList();
				responseRewardsList.Parse(p.data, p.localization, this, "Response_" + responseIndex + "_Rewards", "Response_" + responseIndex + "_Reward_");

				responses[responseIndex] = new DialogueResponse(responseIndex, responseMessages, responseDialogue, responseDialogueGuid, responseQuest, responseQuestGuid, responseVendor, responseVendorGuid, responseText, responseConditionsList, responseRewardsList);
			}
		}

		[System.Obsolete("Please use GetAvailableMessage which returns the DialogueMessage rather than index")]
		public int getAvailableMessage(Player player)
		{
			DialogueMessage message = GetAvailableMessage(player);
			return message != null ? message.index : -1;
		}

		[System.Obsolete("Server now tracks dialogue tree")]
		public bool doesPlayerHaveAccessToVendor(Player player, VendorAsset vendorAsset)
		{
			return true;
		}
	}
}
