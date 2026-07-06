////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
#if UNITY_EDITOR
	public
#else
	internal
#endif
		enum EDatTokenType
	{
		/// <summary>
		/// Not a valid token.
		/// </summary>
		Null,

		/// <summary>
		/// Dictionary key.
		/// Can span multiple lines. (can contain line breaks)
		/// </summary>
		Key,

		/// <summary>
		/// String with '\n', '\\', and '\t' escape sequences handled.
		/// Can span multiple lines. (can contain line breaks)
		/// </summary>
		Value,

		/// <summary>
		/// Single line of comment text. (no line breaks)
		/// Can be an inline comment following a quoted value.
		/// </summary>
		Comment,

		/// <summary>
		/// '\n' or "\r\n"
		/// Ideally, this wouldn't be a token. Nevertheless, line breaks significantly affect dat parsing.
		/// </summary>
		LineBreak,

		/// <summary>
		/// '{'
		/// </summary>
		OpenDictionary,

		/// <summary>
		/// '}'
		/// </summary>
		CloseDictionary,

		/// <summary>
		/// '['
		/// </summary>
		OpenList,

		/// <summary>
		/// ']'
		/// </summary>
		CloseList,
	}

#if UNITY_EDITOR
	public
#else
	internal
#endif
		struct DatToken
	{
		public EDatTokenType type;
		public string value;

		public DatToken(EDatTokenType type)
		{
			this.type = type;
			value = null;
		}

		public DatToken(EDatTokenType type, string value)
		{
			this.type = type;
			this.value = value;
		}

		public override string ToString()
		{
			if (value == null)
			{
				return $"(Type: {type}, Value: null)";
			}
			else
			{
				return $"(Type: {type}, Value: \"{value}\")";
			}
		}
	}

	/// <summary>
	/// Commas are treated as whitespace because many modded "v2" assets had unnecessary commas being treated as
	/// dictionary keys or empty list entries.
	/// </summary>
#if UNITY_EDITOR
	public
#else
	internal
#endif
		class DatTokenizer
	{
		public DatTokenizer()
		{
			errorMessages = new List<string>();
			tokens = new List<DatToken>();
			contextStack = new List<EContext>();
			stringBuilder = new System.Text.StringBuilder();
		}

		public bool HasError => errorMessages.Count > 0;

		/// <summary>
		/// If true, comments are parsed into comment tokens. Otherwise, they are skipped.
		/// </summary>
		public bool EnableComments
		{
			get;
			set;
		}

		public void Tokenize(System.IO.TextReader inputReader)
		{
			this.inputReader = inputReader;
			hasChar = false; // reset hasChar before ReadChar because it uses previous char value
			currentLineNumber = 1;
			tokens.Clear();
			errorMessages.Clear();
			contextStack.Clear();
			tokenIndex = 0;

			ReadChar();
			SkipUtf8Bom();

			while (hasChar)
			{
				if (currentChar == '/')
				{
					if (EnableComments)
					{
						ReadComment();
					}
					else
					{
						SkipToEndOfLine();
					}
				}
				else if (currentChar == '\r')
				{
					ReadChar();
					if (currentChar == '\n')
					{
						ReadChar();
					}
					PushToken(EDatTokenType.LineBreak);
				}
				else if (currentChar == '\n')
				{
					ReadChar();
					PushToken(EDatTokenType.LineBreak);
				}
				else if (currentChar == '{')
				{
					PushToken(EDatTokenType.OpenDictionary);
					PushContext(EContext.Dictionary);
					ReadChar();
					if (hasChar && currentChar == ',')
					{
						ReadChar();
					}
				}
				else if (currentChar == '}')
				{
					// ReadChar after so PopContext has appropriate line number for error
					PopContext(EContext.Dictionary);
					PushToken(EDatTokenType.CloseDictionary);
					ReadChar();
					if (hasChar && currentChar == ',')
					{
						ReadChar();
					}
				}
				else if (currentChar == '[')
				{
					PushToken(EDatTokenType.OpenList);
					PushContext(EContext.List);
					ReadChar();
					if (hasChar && currentChar == ',')
					{
						ReadChar();
					}
				}
				else if (currentChar == ']')
				{
					// ReadChar after so PopContext has appropriate line number for error
					PopContext(EContext.List);
					PushToken(EDatTokenType.CloseList);
					ReadChar();
					if (hasChar && currentChar == ',')
					{
						ReadChar();
					}
				}
				else if (char.IsWhiteSpace(currentChar))
				{
					ReadChar();
				}
				else
				{
					if (GetContext() == EContext.Dictionary)
					{
						ReadDictionaryKey();
						SkipSpacesAndTabs();
						if (hasChar && !char.IsWhiteSpace(currentChar))
						{
							ReadStringValue();
						}
					}
					else
					{
						ReadStringValue();
					}
				}
			}
		}

		public void Tokenize(string input)
		{
			using (System.IO.StringReader stringReader = new System.IO.StringReader(input))
			{
				Tokenize(stringReader);
			}
		}

		public bool ReadToken(out DatToken token)
		{
			if (tokenIndex < tokens.Count)
			{
				token = tokens[tokenIndex];
				++tokenIndex;
				return true;
			}
			else
			{
				token = new DatToken(EDatTokenType.Null);
				return false;
			}
		}

		private void ReadChar()
		{
			bool wasPreviousCharCarriageReturn = hasChar && currentChar == '\r';

			int readResult = inputReader.Read();
			hasChar = readResult >= 0;
			currentChar = hasChar ? (char) readResult : default;

			currentLineNumber += (hasChar && (currentChar == '\r' || (currentChar == '\n' && !wasPreviousCharCarriageReturn)) ? 1 : 0);
		}

		/// <summary>
		/// Skip UTF-8 byte order mark. (if any)
		/// </summary>
		private void SkipUtf8Bom()
		{
			if (!hasChar || currentChar != 0xEF)
				return;

			ReadChar();
			if (!hasChar || currentChar != 0xBB)
				return;

			ReadChar();
			if (!hasChar || currentChar != 0xBF)
				return;

			ReadChar();
		}

		private void SkipSpacesAndTabs()
		{
			while (hasChar && (currentChar == ' ' || currentChar == '\t'))
			{
				ReadChar();
			}
		}

		private void SkipToEndOfLine()
		{
			while (hasChar && currentChar != '\r' && currentChar != '\n')
			{
				ReadChar();
			}
		}

		private void ReadComment()
		{
			// currentChar is '/'

			// Skip // leading into comment text. 
			do
			{
				ReadChar();
			}
			while (hasChar && currentChar == '/');

			// Skip optional first space leading into comment text.
			if (hasChar && currentChar == ' ')
			{
				ReadChar();
			}

			stringBuilder.Clear();
			while (hasChar && currentChar != '\r' && currentChar != '\n')
			{
				stringBuilder.Append(currentChar);
				ReadChar();
			}

			// Push a comment token in either case to preserve spacer comment lines.
			//
			// Like above. ;)
			if (stringBuilder.Length > 0)
			{
				PushToken(EDatTokenType.Comment, stringBuilder.ToString());
			}
			else
			{
				PushToken(EDatTokenType.Comment);
			}
		}

		private void ReadQuotedString(EDatTokenType type)
		{
			// currentChar is "

			int openingLineNumber = currentLineNumber;
			ReadChar(); // "

			bool escapeNextChar = false;
			bool foundEnd = false;

			stringBuilder.Clear();
			while (hasChar)
			{
				if (escapeNextChar)
				{
					if (currentChar == 'n')
					{
						currentChar = '\n';
					}
					else if (currentChar == 't')
					{
						currentChar = '\t';
					}
					else if (currentChar != '\\' && currentChar != '"')
					{
						// Unrecognized escape sequence. Append the previously-ignored backslash.
						// 2023-05-27: the 3.23.7.0 update added '\n' handling for unquoted strings
						// which broke some mods that were using '\' in file paths. To work around
						// this we treat the invalid escape as just '\' and report an error.
						stringBuilder.Append('\\');
						PushErrorMessage($"unrecognized escape sequence (\\{currentChar}) on line {currentLineNumber} — if this is a file path please use forward slash (/)");
					}
				}
				else
				{
					if (currentChar == '"')
					{
						// end of quoted string
						ReadChar(); // "
						foundEnd = true;
						break;
					}
					else if (currentChar == '\\')
					{
						escapeNextChar = true;
						ReadChar();
						continue;
					}
				}

				escapeNextChar = false;
				stringBuilder.Append(currentChar);
				ReadChar();
			}

			if (!foundEnd)
			{
				PushErrorMessage($"missing closing quotation mark (\") for string opened on line {openingLineNumber}");
			}

			PushToken(type, stringBuilder.ToString());

			if (hasChar && currentChar == ',')
			{
				ReadChar();
			}
		}

		private void ReadDictionaryKey()
		{
			// currentChar is not whitespace

			if (currentChar == '"')
			{
				ReadQuotedString(EDatTokenType.Key);
			}
			else
			{
				stringBuilder.Clear();
				do
				{
					stringBuilder.Append(currentChar);
					ReadChar();
				}
				while (hasChar && !char.IsWhiteSpace(currentChar));
				PushToken(EDatTokenType.Key, stringBuilder.ToString());
			}
		}

		private void ReadStringValue()
		{
			// currentChar is not whitespace

			if (currentChar == '"')
			{
				ReadQuotedString(EDatTokenType.Value);
			}
			else
			{
				bool escapeNextChar = false;

				stringBuilder.Clear();
				do
				{
					if (escapeNextChar)
					{
						if (currentChar == 'n')
						{
							currentChar = '\n';
						}
						else if (currentChar == 't')
						{
							currentChar = '\t';
						}
						else if (currentChar != '\\')
						{
							// Unrecognized escape sequence. Append the previously-ignored backslash.
							// 2023-05-27: the 3.23.7.0 update added '\n' handling for unquoted strings
							// which broke some mods that were using '\' in file paths. To work around
							// this we treat the invalid escape as just '\' and report an error.
							stringBuilder.Append('\\');
							PushErrorMessage($"unrecognized escape sequence (\\{currentChar}) on line {currentLineNumber} — if this is a file path please use forward slash (/)");
						}
					}
					else
					{
						if (currentChar == '\r' || currentChar == '\n')
						{
							break;
						}
						else if (currentChar == '\\')
						{
							escapeNextChar = true;
							ReadChar();
							continue;
						}
					}

					escapeNextChar = false;
					stringBuilder.Append(currentChar);
					ReadChar();
				}
				while (hasChar);

				PushToken(EDatTokenType.Value, stringBuilder.ToString());
			}
		}

		private EContext GetContext()
		{
			int count = contextStack.Count;
			return count > 0 ? contextStack[count - 1] : EContext.Dictionary;
		}

		private void PushToken(EDatTokenType type)
		{
			tokens.Add(new DatToken(type));
		}

		private void PushToken(EDatTokenType type, string value)
		{
			tokens.Add(new DatToken(type, value));
		}

		private void PushErrorMessage(string message)
		{
			errorMessages.Add(message);
		}

		private void PushContext(EContext context)
		{
			contextStack.Add(context);
		}

		private void PopContext(EContext expectedContext)
		{
			int count = contextStack.Count;
			if (count > 0)
			{
				EContext actualContext = contextStack[count - 1];
				if (expectedContext == actualContext)
				{
					contextStack.RemoveAt(count - 1);
					return;
				}
			}

			switch (expectedContext)
			{
				case EContext.Dictionary:
					PushErrorMessage($"unexpected end of dictionary/object '}}' on line {currentLineNumber}");
					break;

				case EContext.List:
					PushErrorMessage($"unexpected end of list ']' on line {currentLineNumber}");
					break;
			}
		}

		public void DebugDumpTokensToStringBuilder(System.Text.StringBuilder output)
		{
			for (int index = 0; index < tokens.Count; ++index)
			{
				output.Append(index);
				output.Append(' ');
				output.Append(tokens[index].type);
				if (string.IsNullOrEmpty(tokens[index].value))
				{
					output.AppendLine();
				}
				else
				{
					output.Append(' ');
					output.AppendLine(tokens[index].value);
				}
			}
		}

		public string DebugDumpTokensToString()
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			DebugDumpTokensToStringBuilder(output);
			return output.ToString();
		}

		public List<string> errorMessages;

		private System.IO.TextReader inputReader;
		private int currentLineNumber;
		private char currentChar;
		private bool hasChar;

		private List<DatToken> tokens;
		private List<EContext> contextStack;
		private int tokenIndex;
		private System.Text.StringBuilder stringBuilder;

		private enum EContext
		{
			Dictionary,
			List,
		}
	}
}
