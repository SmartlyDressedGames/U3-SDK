////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class DatParser
	{
		public DatParser()
		{
			tokenizer = new DatTokenizer();
			errorMessages = new List<string>();
		}

		public IDatDictionary Parse(System.IO.TextReader inputReader)
		{
			tokenizer.EnableComments = enableMetadata;
			tokenizer.Tokenize(inputReader);
			errorMessages.Clear();
			hasToken = false;
			currentLineNumber = 1;

			if (tokenizer.HasError)
			{
				errorMessages.AddRange(tokenizer.errorMessages);
			}

			ReadToken();

			DatDictionary underlyingDictionary = new DatDictionary();
			IDatDictionary rootDictionary = underlyingDictionary;
			if (enableMetadata)
			{
				DatDictionaryWithMetadata rootDictionaryWithMetadata = new DatDictionaryWithMetadata(underlyingDictionary);
				rootDictionaryWithMetadata.openingLineNumber = 1;
				rootDictionary = rootDictionaryWithMetadata;
			}

			while (hasToken)
			{
				switch (currentToken.type)
				{
					case EDatTokenType.Key:
					{
						string key = currentToken.value;
						IDatNode value = ReadDictionaryValue();
						AddValueToDictionary(underlyingDictionary, rootDictionary, key, value);
						break;
					}

					case EDatTokenType.Comment:
					{
						BuildComment();
						break;
					}

					default:
					{
						ReadToken();
						break;
					}
				}
			}

			if (enableMetadata)
			{
				// Ending line number is effectively the final line number shown in the text editor.
				// (Depends whether there are trailing line breaks.)
				((DatDictionaryWithMetadata) rootDictionary).closingLineNumber = currentLineNumber;
			}

			return rootDictionary;
		}

		public IDatDictionary Parse(string input)
		{
			using (System.IO.StringReader stringReader = new System.IO.StringReader(input))
			{
				return Parse(stringReader);
			}
		}

		public IDatDictionary Parse(byte[] input)
		{
			using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(input))
			using (System.IO.StreamReader streamReader = new System.IO.StreamReader(memoryStream))
			{
				return Parse(streamReader);
			}
		}

		/// <summary>
		/// If true, create "WithMetadata" subclasses that remember line numbers and comments.
		/// </summary>
		public bool EnableMetadata
		{
			get => enableMetadata;
			set
			{
				enableMetadata = value;
				if (enableMetadata && commentLines == null)
				{
					commentLines = new List<string>();
				}
			}
		}

		public bool HasError => errorMessages.Count > 0;

		/// <summary>
		/// Get the first error message.
		/// </summary>
		public string ErrorMessage
		{
			get => errorMessages.Count > 0 ? errorMessages[0] : null;
		}

		public IReadOnlyList<string> ErrorMessages => errorMessages;

		private void ReadToken()
		{
			hasToken = tokenizer.ReadToken(out currentToken);
			if (currentToken.type == EDatTokenType.LineBreak)
			{
				++currentLineNumber;
			}
		}

		private void AddValueToDictionary(DatDictionary underlyingDictionary, IDatDictionary dictionary, string key, IDatNode value)
		{
			if (enableMetadata)
			{
				((DatNodeWithMetadataBase) value).parentNode = dictionary;
			}

			if (dictionary.TryGetNode(key, out IDatNode existingValue))
			{
				string existingString = GetNodeStringForErrorMessage(existingValue);
				string newString = GetNodeStringForErrorMessage(value);
				PushErrorMessage($"duplicate key \"{key}\" on line {currentLineNumber} replacing existing value {existingString} with {newString}");
			}

			underlyingDictionary[key] = value;
		}

		private string GetNodeStringForErrorMessage(IDatNode node)
		{
			if (node == null)
			{
				return "null";
			}
			else if (node is IDatList list)
			{
				return $"list with {list.Count} item(s)";
			}
			else if (node is IDatDictionary dictionary)
			{
				return $"dictionary with {dictionary.Count} item(s)";
			}
			else if (node is IDatValue value)
			{
				if (value.Value == null)
				{
					return "value(null)";
				}
				else
				{
					return $"\"{value.Value}\"";
				}
			}
			else
			{
				// unimplemented???
				return node.GetType().Name;
			}
		}

		private void BuildComment()
		{
			if (enableMetadata)
			{
				if (commentLines.Count < 1)
				{
					commentStartingLineNumber = currentLineNumber;
				}
				commentEndingLineNumber = currentLineNumber;
				commentLines.Add(currentToken.value);
			}
			ReadToken();
		}

		private DatComment? ConsumePrefixComment()
		{
			DatComment? prefixComment = null;
			if (enableMetadata)
			{
				if (commentLines.Count > 0)
				{
					prefixComment = new DatComment()
					{
						MessageLines = commentLines.ToArray(),
						StartingLineNumber = commentStartingLineNumber,
						EndingLineNumber = commentEndingLineNumber,
					};

					commentLines.Clear();
				}
			}
			return prefixComment;
		}

		private IDatNode ReadDictionaryValue()
		{
			int keyLineNumber = currentLineNumber;
			ReadToken(); // Advance past key

			DatComment? prefixComment = ConsumePrefixComment();

			// There is a weird case here: even if key is assigned a value, a list/dictionary start on the
			// next line takes precedence.
			DatToken maybeValueToken = currentToken;
			if (hasToken && currentToken.type == EDatTokenType.Value)
			{
				ReadToken(); // Advance past value to inline comment or line break.
			}

			DatToken maybeComment = currentToken;
			if (hasToken && currentToken.type == EDatTokenType.Comment)
			{
				ReadToken(); // Advance past inline comment to line break.
			}
			if (hasToken && currentToken.type == EDatTokenType.LineBreak)
			{
				ReadToken(); // Advance past line break to next line's value.
			}

			if (hasToken)
			{
				switch (currentToken.type)
				{
					case EDatTokenType.OpenDictionary:
					{
						IDatDictionary dictionary = ReadDictionary();
						if (enableMetadata)
						{
							DatDictionaryWithMetadata dictionaryWithMetadata = (DatDictionaryWithMetadata) dictionary;
							dictionaryWithMetadata.prefixComment = prefixComment;
						}
						return dictionary;
					}

					case EDatTokenType.OpenList:
					{
						IDatList list = ReadList();
						if (enableMetadata)
						{
							DatListWithMetadata listWithMetadata = (DatListWithMetadata) list;
							listWithMetadata.prefixComment = prefixComment;
						}
						return list;
					}
				}
			}

			string value = maybeValueToken.type == EDatTokenType.Value ? maybeValueToken.value : null;
			DatValue valueNode = new DatValue(value);
			if (enableMetadata)
			{
				string inlineComment = maybeComment.type == EDatTokenType.Comment ? maybeComment.value : null;
				return new DatValueWithMetadata(valueNode, keyLineNumber, inlineComment, prefixComment);
			}
			else
			{
				return valueNode;
			}
		}

		private IDatDictionary ReadDictionary()
		{
			int openingLineNumber = currentLineNumber;
			ReadToken(); // Advance past {

			DatDictionary underlyingDictionary = new DatDictionary();
			IDatDictionary dictionary = underlyingDictionary;

			if (enableMetadata)
			{
				dictionary = new DatDictionaryWithMetadata(underlyingDictionary);
				commentLines.Clear();
			}

			bool foundEnd = false;

			while (hasToken && !foundEnd)
			{
				switch (currentToken.type)
				{
					case EDatTokenType.CloseDictionary:
					{
						if (enableMetadata)
						{
							DatDictionaryWithMetadata dictionaryWithMetadata = (DatDictionaryWithMetadata) dictionary;
							dictionaryWithMetadata.openingLineNumber = openingLineNumber;
							dictionaryWithMetadata.closingLineNumber = currentLineNumber;
						}
						ReadToken(); // Advance past }
						foundEnd = true;
						break;
					}

					case EDatTokenType.Key:
					{
						string key = currentToken.value;
						IDatNode value = ReadDictionaryValue();
						AddValueToDictionary(underlyingDictionary, dictionary, key, value);
						break;
					}

					case EDatTokenType.Comment:
					{
						BuildComment();
						break;
					}

					default:
					{
						ReadToken();
						continue;
					}
				}
			}

			if (!foundEnd)
			{
				PushErrorMessage($"missing closing curly bracket '}}' for dictionary opened on line {openingLineNumber}");
			}

			return dictionary;
		}

		private IDatList ReadList()
		{
			int openingLineNumber = currentLineNumber;
			ReadToken(); // Advance past [

			DatList underlyingList = new DatList();
			IDatList list = enableMetadata ? new DatListWithMetadata(underlyingList) : underlyingList;

			if (enableMetadata)
			{
				commentLines.Clear();
			}

			bool foundEnd = false;

			while (hasToken && !foundEnd)
			{
				switch (currentToken.type)
				{
					case EDatTokenType.CloseList:
					{
						if (enableMetadata)
						{
							DatListWithMetadata listWithMetadata = (DatListWithMetadata) list;
							listWithMetadata.openingLineNumber = openingLineNumber;
							listWithMetadata.closingLineNumber = currentLineNumber;
						}

						ReadToken(); // Advance past ]
						foundEnd = true;
						break;
					}

					case EDatTokenType.OpenDictionary:
					{
						DatComment? prefixComment = ConsumePrefixComment();
						IDatDictionary dictionary = ReadDictionary();
						if (enableMetadata)
						{
							DatDictionaryWithMetadata dictionaryWithMetadata = (DatDictionaryWithMetadata) dictionary;
							dictionaryWithMetadata.parentNode = list;
							dictionaryWithMetadata.prefixComment = prefixComment;
						}
						underlyingList.Add(dictionary);
						break;
					}

					case EDatTokenType.OpenList:
					{
						DatComment? prefixComment = ConsumePrefixComment();
						IDatList childList = ReadList();
						if (enableMetadata)
						{
							DatListWithMetadata childListWithMetadata = (DatListWithMetadata) childList;
							childListWithMetadata.parentNode = list;
							childListWithMetadata.prefixComment = prefixComment;
						}
						underlyingList.Add(childList);
						break;
					}

					case EDatTokenType.Value:
					{
						DatComment? prefixComment = ConsumePrefixComment();
						string value = currentToken.value;
						int lineNumber = currentLineNumber;
						ReadToken();
						DatValue valueNode = new DatValue(value);
						if (enableMetadata)
						{
							string comment = null;
							if (hasToken && currentToken.type == EDatTokenType.Comment)
							{
								comment = currentToken.value;
								ReadToken();
							}
							DatValueWithMetadata valueWithMetadata = new DatValueWithMetadata(valueNode, lineNumber, comment, prefixComment);
							valueWithMetadata.parentNode = list;
							underlyingList.Add(valueWithMetadata);
						}
						else
						{
							underlyingList.Add(valueNode);
						}
						break;
					}

					case EDatTokenType.Comment:
					{
						BuildComment();
						break;
					}

					default:
					{
						ReadToken();
						break;
					}
				}
			}

			if (!foundEnd)
			{
				PushErrorMessage($"missing closing bracket ']' for list opened on line {openingLineNumber}");
			}

			return list;
		}

		/// <summary>
		/// Note: parsing does not stop when an error is encountered. Unfortunately lots of third-party assets
		/// have typos which technically work correctly if ignored, and the old parser didn't log them.
		/// </summary>
		private void PushErrorMessage(string message)
		{
			errorMessages.Add(message);
		}

		private DatTokenizer tokenizer;
		private int currentLineNumber;
		private DatToken currentToken;
		private bool hasToken;

		private List<string> errorMessages;
		private bool enableMetadata;

		private List<string> commentLines;
		private int commentStartingLineNumber;
		private int commentEndingLineNumber;
	}
}
