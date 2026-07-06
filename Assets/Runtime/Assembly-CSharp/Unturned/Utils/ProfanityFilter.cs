////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned
{
	public class ProfanityFilter
	{
		private static string[] curseWords = null;
		public static string[] getCurseWords()
		{
			if (curseWords == null)
			{
				LoadCurseWords();
			}

			return curseWords;
		}

		internal delegate void FilterDelegate(ref string message);
		internal static FilterDelegate filter = NaiveDefaultFilter;

#if !DISABLESTEAMWORKS
		public static CommandLineFlag shouldInitSteamTextFiltering = new CommandLineFlag(true, "-NoSteamTextFiltering");

		internal static void InitSteam()
		{
			if (shouldInitSteamTextFiltering)
			{
				if (SteamUtils.InitFilterText())
				{
					filter = SteamFilter;
				}
				else
				{
					UnturnedLog.info("Unable to initialize Steam text filtering");
				}
			}
			else
			{
				UnturnedLog.info("Not initializing Steam text filtering");
			}
		}

		private static void SteamFilter(ref string message)
		{
			string filteredMessage;
			int filteredLength = SteamUtils.FilterText(ETextFilteringContext.k_ETextFilteringContextUnknown, CSteamID.Nil, message, out filteredMessage, (uint) ((message.Length * sizeof(char)) + 1));
			// UnturnedLog.info($"Message: {message} FilteredMessage: {filteredMessage} FilteredLength: {filteredLength}");
			if (filteredLength > 0)
			{
				// filteredLength is the number of characters replaced, not length of filteredMessage.
				message = filteredMessage;
			}
		}
#endif // !DISABLESTEAMWORKS

		private static void NaiveDefaultFilter(ref string message)
		{
			filterOutCurseWords(ref message);
		}

		internal static void ApplyFilter(bool enableProfanityFilter, ref string message)
		{
			if (NaiveContainsHardcodedBannedWord(message))
			{
				// If changing this please remember to update the support article:
				// https://support.smartlydressedgames.com/hc/en-us/articles/15312306673556
				message = "<3";
				return;
			}

			if (enableProfanityFilter)
			{
				filter(ref message);
			}
		}

		public static bool filterOutCurseWords(ref string text, char replacementChar = '#')
		{
			bool foundAnyCurseWords = false;

			if (text.Length > 0)
			{
				foreach (string curseWord in getCurseWords())
				{
					int step = indexOfCurseWord(text, curseWord, 0);

					// At one point we also checked if there were ANY spaces before considering spaces between curses,
					// but that was problematic because chat messages like "Nelson: CurseCurse" have a space anyway,
					// and signs formatted simple text like "Hello" as a curse (####o).
					while (step != -1)
					{
						if ((step == 0 || !char.IsLetterOrDigit(text[step - 1])) && (step == text.Length - curseWord.Length || !char.IsLetterOrDigit(text[step + curseWord.Length])))
						{
							replaceCurseWord(ref text, step, curseWord.Length, replacementChar);

							step = indexOfCurseWord(text, curseWord, step);
							foundAnyCurseWords = true;
						}
						else
						{
							step = indexOfCurseWord(text, curseWord, step + 1);
						}
					}
				}
			}

			return foundAnyCurseWords;
		}

		public static bool NaiveContainsHardcodedBannedWord(string message)
		{
			if (string.IsNullOrEmpty(message))
				return false;

			foreach (string word in hardcodedBannedWords)
			{
				int index = indexOfCurseWord(message, word, 0);
				while (index != -1)
				{
					if ((index == 0 || !char.IsLetterOrDigit(message[index - 1])) && (index == message.Length - word.Length || !char.IsLetterOrDigit(message[index + word.Length])))
					{
						return true;
					}
					else
					{
						index = indexOfCurseWord(message, word, index + 1);
					}
				}
			}

			return false;
		}

		private static int indexOfCurseWord(string userText, string curseWord, int startIndex)
		{
			// e.g. userText length is 5, curseWord length is 3, so only iterate <= index 2
			int userIterLength = userText.Length - curseWord.Length;
			for (int userCharIndex = startIndex; userCharIndex <= userIterLength; ++userCharIndex)
			{
				bool matchedEntireCurseWord = true;

				for (int curseCharIndex = 0; curseCharIndex < curseWord.Length; ++curseCharIndex)
				{
					char userChar = char.ToLower(userText[userCharIndex + curseCharIndex]);
					char curseChar = curseWord[curseCharIndex];
					bool match = userChar == curseChar;
					if (match)
					{
						continue;
					}
					else
					{
						// Test common leetspeak alternatives.
						switch (curseChar)
						{
							case 'a':
								match = userChar == '4' || userChar == '@';
								break;
							case 'e':
								match = userChar == '3';
								break;
							case 'h':
								match = userChar == '#';
								break;
							case 'i':
								match = userChar == '1';
								break;
							case 'l':
								match = userChar == '1';
								break;
							case 'o':
								match = userChar == '0';
								break;
							case 's':
								match = userChar == '$' || userChar == '5';
								break;
							case 't':
								match = userChar == '7';
								break;
						}

						if (match)
						{
							continue;
						}
						else
						{
							matchedEntireCurseWord = false;
							break;
						}
					}
				}

				if (matchedEntireCurseWord)
				{
					return userCharIndex;
				}
			}

			return -1;
		}

		private static void replaceCurseWord(ref string text, int startIndex, int curseWordLength, char replacementChar)
		{
			string filtered = text.Substring(0, startIndex);

			for (int write = 0; write < curseWordLength; write++)
			{
				filtered += replacementChar;
			}

			filtered += text.Substring(startIndex + curseWordLength, text.Length - startIndex - curseWordLength);

			text = filtered;
		}

		private static void LoadCurseWords()
		{
			if (string.IsNullOrEmpty(Provider.localizationRoot)) // Running in unity editor
			{
				curseWords = File.ReadAllLines(ReadWrite.PATH + "/Localization/English/Curse_Words.txt");
			}
			else
			{
				string languageCurseWordsPath = Provider.localizationRoot + "/Curse_Words.txt";
				if (File.Exists(languageCurseWordsPath))
				{
					curseWords = File.ReadAllLines(languageCurseWordsPath);
				}
				else
				{
					string englishCurseWordsPath = Provider.path + "/English/Curse_Words.txt";
					if (File.Exists(englishCurseWordsPath))
					{
						curseWords = File.ReadAllLines(englishCurseWordsPath);
					}
					else
					{
						curseWords = new string[0];
					}
				}
			}

			if (curseWords == null || curseWords.Length < 1)
			{
				UnturnedLog.error("Failed to load list of curse words for profanity filter!");
				curseWords = new string[0];
			}
			else
			{
				ProcessLoadedCurseWords();
			}
		}

		private static void ProcessLoadedCurseWords()
		{
			List<string> filteredCurseWords = new List<string>();

			// Remove empty lines and comments
			for (int filterIndex = curseWords.Length - 1; filterIndex >= 0; filterIndex--)
			{
				string curseWord = curseWords[filterIndex];
				if (string.IsNullOrEmpty(curseWord) || curseWord.StartsWith("#"))
					continue;

				filteredCurseWords.Add(curseWord);
			}

			curseWords = filteredCurseWords.ToArray();
		}

		/// <summary>
		/// 2023-04-17: suggestion is to have a hardcoded list of hate speech that gets filtered
		///	regardless of whether profanity filter is enabled.
		/// </summary>
		private static readonly string[] hardcodedBannedWords = new string[]
		{
			"nigger",
			"niggers",
			"niger",
			"nigers",
			"jew",
			"jews",
			"fag",
			"fags",
			"faggot",
			"faggots",
			"fagot",
			"fagots",
			"faggit",
			"faggits",
			"fagit",
			"fagits",
			"rape",
			"raped",
		};
	}
}
