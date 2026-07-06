////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Editor-only helper to read all text/dialogue.
	/// </summary>
	public static class TextDebug
	{
		public static void LogAllText()
		{
			DirectoryInfo rootDir = UnityPaths.TempDirectory.CreateSubdirectory("TextDebug");
			DirectoryInfo npcsDir = rootDir.CreateSubdirectory("NPCs");

			List<ObjectNPCAsset> allNpcs = new List<ObjectNPCAsset>();
			Assets.find(allNpcs);
			foreach (ObjectNPCAsset npc in allNpcs)
			{
				if (npc.origin == Assets.coreOrigin)
					continue;

				DialogueAsset rootDialogue = npc.FindDialogueAsset();
				if (rootDialogue != null)
				{
					string fileName = PathEx.Join(npcsDir, npc.name + ".txt");
					WriteNpcDialogue(npc, rootDialogue, fileName);
				}
			}

			DirectoryInfo notesDir = rootDir.CreateSubdirectory("Notes");

			List<ObjectAsset> allObjects = new List<ObjectAsset>();
			Assets.find(allObjects);
			foreach (ObjectAsset asset in allObjects)
			{
				if (asset.origin == Assets.coreOrigin)
					continue;

				if (asset.interactability == EObjectInteractability.NOTE)
				{
					string fileName = PathEx.Join(notesDir, asset.name + ".txt");
					File.WriteAllText(fileName, asset.interactabilityText);
				}
				else if (asset.interactability == EObjectInteractability.QUEST)
				{
					DialogueAsset rootDialogue = asset.FindInteractabilityDialogueAsset();
					if (rootDialogue != null)
					{
						string fileName = PathEx.Join(npcsDir, asset.name + ".txt");
						WriteNpcDialogue(asset, rootDialogue, fileName);
					}
				}
			}

			string itemsFilePath = PathEx.Join(rootDir, "Items.txt");
			using (FileStream fs = new FileStream(itemsFilePath, FileMode.Create, FileAccess.Write))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				List<ItemAsset> allItems = new List<ItemAsset>();
				Assets.find(allItems);
				foreach (ItemAsset asset in allItems)
				{
					if (asset.origin == Assets.coreOrigin)
						continue;

					sw.WriteLine(asset.FriendlyNameWithFriendlyType);
					sw.WriteLine(RichTextUtil.replaceColorTags(asset.itemDescription).Replace("\n", "<br>"));
					sw.WriteLine();
				}
			}
		}

		private static void WriteNpcDialogue(ObjectAsset npc, DialogueAsset rootDialogue, string fileName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(npc.FriendlyName);
			stringBuilder.AppendLine();
			Stack<DialogueAsset> history = new Stack<DialogueAsset>();
			history.Push(rootDialogue);
			AppendMessages(rootDialogue, stringBuilder, 0, history);
			File.WriteAllText(fileName, stringBuilder.ToString());
		}

		private static string GetIndentationPrefix(int indentationLevel)
		{
			string indentationPrefix = string.Empty;
			for (int level = 0; level < indentationLevel; ++level)
			{
				indentationPrefix += '\t';
			}
			return indentationPrefix;
		}

		private static void AppendQuestInfo(QuestAsset questAsset, StringBuilder stringBuilder, int indentationLevel)
		{
			string indentationPrefix = GetIndentationPrefix(indentationLevel);
			stringBuilder.Append(indentationPrefix);
			stringBuilder.AppendLine($"Quest {RichTextUtil.replaceColorTags(questAsset.questName)}:");
			stringBuilder.Append(indentationPrefix);
			stringBuilder.AppendLine(RichTextUtil.replaceColorTags(questAsset.questDescription));
		}

		private static void AppendVendorInfo(VendorAsset vendorAsset, StringBuilder stringBuilder, int indentationLevel)
		{
			string indentationPrefix = GetIndentationPrefix(indentationLevel);
			stringBuilder.Append(indentationPrefix);
			stringBuilder.AppendLine($"Vendor {RichTextUtil.replaceColorTags(vendorAsset.vendorName)}:");
			stringBuilder.Append(indentationPrefix);
			stringBuilder.AppendLine(RichTextUtil.replaceColorTags(vendorAsset.vendorDescription));
		}

		private static void AppendWithPrefixBeforeNewlines(StringBuilder stringBuilder, string text, string prefix)
		{
			string[] lines = text.SplitLines();
			foreach (string line in lines)
			{
				stringBuilder.Append(prefix);
				stringBuilder.AppendLine(line);
			}
		}

		private static void AppendMessages(DialogueAsset dialogueAsset, StringBuilder stringBuilder, int indentationLevel, Stack<DialogueAsset> history)
		{
			string indentationPrefix = GetIndentationPrefix(indentationLevel);
			for (int messageIndex = 0; messageIndex < dialogueAsset.messages.Length; ++messageIndex)
			{
				DialogueMessage message = dialogueAsset.messages[messageIndex];
				foreach (DialoguePage page in message.pages)
				{
					AppendWithPrefixBeforeNewlines(stringBuilder, RichTextUtil.replaceColorTags(page.text), indentationPrefix);
				}

				List<DialogueResponse> responses = new List<DialogueResponse>();
				dialogueAsset.GetAllResponsesForMessage(messageIndex, responses);
				foreach (DialogueResponse response in responses)
				{
					stringBuilder.Append(indentationPrefix);
					stringBuilder.AppendLine("> " + RichTextUtil.replaceColorTags(response.text));

					VendorAsset vendor = response.FindVendorAsset();
					if (vendor != null)
					{
						AppendVendorInfo(vendor, stringBuilder, indentationLevel + 1);
					}

					QuestAsset quest = response.FindQuestAsset();
					if (quest != null)
					{
						AppendQuestInfo(quest, stringBuilder, indentationLevel + 1);
					}

					DialogueAsset nestedDialogueAsset = response.FindDialogueAsset();
					if (nestedDialogueAsset != null)
					{
						if (!history.Contains(nestedDialogueAsset))
						{
							history.Push(nestedDialogueAsset);
							AppendMessages(nestedDialogueAsset, stringBuilder, indentationLevel + 1, history);
							history.Pop();
						}
					}
				}
			}
		}
	}
}
#endif // UNITY_EDITOR
