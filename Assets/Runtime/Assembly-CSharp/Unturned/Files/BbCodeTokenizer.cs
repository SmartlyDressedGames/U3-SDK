////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public enum EBbCodeTokenType
	{
		/// <summary>
		/// Null token.
		/// </summary>
		Invalid,

		/// <summary>
		/// Text between tags.
		/// </summary>
		String,

		/// <summary>
		/// [b]
		/// </summary>
		BoldOpen,

		/// <summary>
		/// [/b]
		/// </summary>
		BoldClose,

		/// <summary>
		/// [i]
		/// </summary>
		ItalicOpen,

		/// <summary>
		/// [/i]
		/// </summary>
		ItalicClose,

		/// <summary>
		/// [list]
		/// </summary>
		BulletListOpen,

		/// <summary>
		/// [/list]
		/// </summary>
		BulletListClose,

		/// <summary>
		/// [olist]
		/// </summary>
		OrderedListOpen,

		/// <summary>
		/// [/olist]
		/// </summary>
		OrderedListClose,

		/// <summary>
		/// [*] value
		/// Nelson 2025-07-02: manually written lists typically don't have a ListItemClose token.
		/// </summary>
		ListItemOpen,

		/// <summary>
		/// [/*]
		/// Nelson 2025-07-02: Steam's new visual editor adds closing tokens to list items, but
		/// manually-written list items typically don't have them.
		/// </summary>
		ListItemClose,

		/// <summary>
		/// [h1]
		/// </summary>
		H1Open,

		/// <summary>
		/// [/h1]
		/// </summary>
		H1Close,

		/// <summary>
		/// [h2]
		/// </summary>
		H2Open,

		/// <summary>
		/// [/h2]
		/// </summary>
		H2Close,

		/// <summary>
		/// [h3]
		/// </summary>
		H3Open,

		/// <summary>
		/// [/h3]
		/// </summary>
		H3Close,

		/// <summary>
		/// [url=value]
		/// </summary>
		UrlOpen,

		/// <summary>
		/// [/url]
		/// </summary>
		UrlClose,

		/// <summary>
		/// [img]
		/// </summary>
		ImgOpen,

		/// <summary>
		/// [/img]
		/// </summary>
		ImgClose,

		/// <summary>
		/// [previewyoutube=value]
		/// </summary>
		PreviewYouTubeOpen,

		/// <summary>
		/// [/previewyoutube]
		/// </summary>
		PreviewYouTubeClose,

		/// <summary>
		/// '\n' or "\r\n"
		/// </summary>
		LineBreak,

		/// <summary>
		/// [quote=value] (value is author)
		/// </summary>
		QuoteOpen,

		/// <summary>
		/// [/quote]
		/// </summary>
		QuoteClose,

		/// <summary>
		/// [p]
		/// </summary>
		ParagraphOpen,

		/// <summary>
		/// [/p]
		/// </summary>
		ParagraphClose,

		/// <summary>
		/// [u]
		/// </summary>
		UnderlineOpen,

		/// <summary>
		/// [/u]
		/// </summary>
		UnderlineClose,
	}

	public struct BbCodeToken
	{
		public EBbCodeTokenType tokenType;
		public string tokenValue;

		public BbCodeToken(EBbCodeTokenType tokenType)
		{
			this.tokenType = tokenType;
			tokenValue = null;
		}

		public BbCodeToken(EBbCodeTokenType tokenType, string tokenValue)
		{
			this.tokenType = tokenType;
			this.tokenValue = tokenValue;
		}

		public bool TryParseValue(string key, out string value)
		{
			if (string.IsNullOrEmpty(tokenValue))
			{
				value = null;
				return false;
			}

			return CommandLine.TryParseValue(tokenValue, key, out value);
		}

		/// <summary>
		/// Steam's new visual editor quotes value in [url=x] tag. If value is not quoted, this method returns as-is.
		/// If it IS quoted, this methods returns without quotation marks.
		/// </summary>
		public string GetUnquotedValue()
		{
			if (string.IsNullOrEmpty(tokenValue))
			{
				return tokenValue;
			}

			if (tokenValue.Length >= 2 && tokenValue.StartsWith('"') && tokenValue.EndsWith('"'))
			{
				return tokenValue.Substring(1, tokenValue.Length - 2);
			}
			else
			{
				return tokenValue;
			}
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(tokenValue))
			{
				return tokenType.ToString();
			}
			else
			{
				return $"{tokenType}: {tokenValue}";
			}
		}
	}

	/// <summary>
	/// Breaks down Steam's version of BBcode into tokens like, "[b]", "[i]", "actual text", etc.
	/// </summary>
	public class BbCodeTokenizer
	{
		public BbCodeTokenizer()
		{
			tagStringBuilder = new System.Text.StringBuilder();
			stringBuilder = new System.Text.StringBuilder();
		}

		/// <summary>
		/// If true, parse newlines in the input as LineBreak tokens. (default true)
		/// If false, exclude LineBreak tokens from output.
		/// Steam's new visual editor doesn't emit newlines, instead inferring line breaks from paragraph blocks. To
		/// make life easier we will do the same for the main menu announcement feed.
		/// </summary>
		public bool ParseLineBreaks
		{
			get => _parseLineBreaks;
			set => _parseLineBreaks = value;
		}
		private bool _parseLineBreaks = true;

		public List<BbCodeToken> Tokenize(System.IO.TextReader inputReader)
		{
			this.inputReader = inputReader;
			ErrorMessage = null;
			hasChar = false;
			currentLineNumber = 1;

			List<BbCodeToken> outputTokens = new List<BbCodeToken>();

			ReadChar();

			int readCount = 0;
			while (hasChar)
			{
				ReadToken(outputTokens);

				++readCount;
				if (readCount >= 10000)
				{
					ErrorMessage = "Infinite loop attempting to tokenize";
					break;
				}
			}

			return outputTokens;
		}

		public List<BbCodeToken> Tokenize(string input)
		{
			using (System.IO.StringReader stringReader = new System.IO.StringReader(input))
			{
				return Tokenize(stringReader);
			}
		}

		public string DebugDumpTokensToString(List<BbCodeToken> tokens)
		{
			stringBuilder.Clear();
			for (int tokenIndex = 0; tokenIndex < tokens.Count; ++tokenIndex)
			{
				stringBuilder.Append(tokenIndex);
				stringBuilder.Append(": ");

				BbCodeToken token = tokens[tokenIndex];
				if (string.IsNullOrEmpty(token.tokenValue))
				{
					stringBuilder.AppendLine(token.tokenType.ToString());
				}
				else
				{
					stringBuilder.Append(token.tokenType.ToString());
					stringBuilder.Append(": ");
					stringBuilder.AppendLine(token.tokenValue);
				}
			}
			return stringBuilder.ToString();
		}

		public bool HasError => hasError;

		public string ErrorMessage
		{
			get => errorMessage;
			private set
			{
				errorMessage = value;
				hasError = !string.IsNullOrEmpty(errorMessage);
			}
		}

		private void ReadChar()
		{
			bool wasPreviousCharCarriageReturn = hasChar && currentChar == '\r';

			currentReadResult = inputReader.Read();
			hasChar = currentReadResult >= 0;
			currentChar = hasChar ? (char) currentReadResult : default;

			currentLineNumber += (hasChar && (currentChar == '\r' || (currentChar == '\n' && !wasPreviousCharCarriageReturn)) ? 1 : 0);
		}

		private void ReadToken(List<BbCodeToken> outputTokens)
		{
			if (currentChar == '[')
			{
				ReadTag(outputTokens);
			}
			else if (currentChar == '\r')
			{
				ReadChar();
				if (hasChar && currentChar == '\n')
				{
					ReadChar();
				}
				if (_parseLineBreaks)
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.LineBreak));
				}
			}
			else if (currentChar == '\n')
			{
				ReadChar();
				if (_parseLineBreaks)
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.LineBreak));
				}
			}
			else
			{
				ReadString(outputTokens);
			}
		}

		private void ReadString(List<BbCodeToken> tokens)
		{
			stringBuilder.Clear();
			do
			{
				stringBuilder.Append(currentChar);
				ReadChar();
				if (currentChar == '[' || currentChar == '\r' || currentChar == '\n')
				{
					break;
				}
			}
			while (hasChar);

			if (stringBuilder.Length > 0)
			{
				string value = stringBuilder.ToString();
				tokens.Add(new BbCodeToken(EBbCodeTokenType.String, value));
			}
		}

		private void ReadTag(List<BbCodeToken> outputTokens)
		{
			bool hasValueToParse = false;

			stringBuilder.Clear();
			stringBuilder.Append(currentChar);

			tagStringBuilder.Clear();
			ReadChar();
			while (hasChar)
			{
				stringBuilder.Append(currentChar);

				if (currentChar == ']')
				{
					ReadChar();
					break;
				}
				if (currentChar == ' ' || currentChar == '=')
				{
					// ' ' happens with key-value pairs, e.g., [img src=x]
					// '=' happens with, e.g., [url=x]
					hasValueToParse = true;
					ReadChar();
					break;
				}
				tagStringBuilder.Append(currentChar);
				ReadChar();
			}

			string tag = tagStringBuilder.ToString();
			string value = null;

			if (hasValueToParse)
			{
				tagStringBuilder.Clear();
				while (hasChar)
				{
					stringBuilder.Append(currentChar);
					if (currentChar == ']')
					{
						ReadChar();
						break;
					}
					tagStringBuilder.Append(currentChar);
					ReadChar();
				}

				value = tagStringBuilder.ToString();
			}

			bool isNotATag = false;
			bool isClosingTag = tag.StartsWith('/');
			if (isClosingTag)
			{
				if (string.Equals(tag, "/p"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ParagraphClose));
				}
				else if (string.Equals(tag, "/b"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BoldClose));
				}
				else if (string.Equals(tag, "/i"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ItalicClose));
				}
				else if (string.Equals(tag, "/u"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.UnderlineClose));
				}
				else if (string.Equals(tag, "/*"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ListItemClose));
				}
				else if (string.Equals(tag, "/list"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BulletListClose));
				}
				else if (string.Equals(tag, "/olist"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.OrderedListClose));
				}
				else if (string.Equals(tag, "/h1"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H1Close));
				}
				else if (string.Equals(tag, "/h2"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H2Close));
				}
				else if (string.Equals(tag, "/h3"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H3Close));
				}
				else if (string.Equals(tag, "/url"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.UrlClose));
				}
				else if (string.Equals(tag, "/img"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ImgClose));
				}
				else if (string.Equals(tag, "/previewyoutube"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.PreviewYouTubeClose));
				}
				else if (string.Equals(tag, "/quote"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.QuoteClose));
				}
				else
				{
					isNotATag = true;
				}
			}
			else
			{
				if (string.Equals(tag, "p"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ParagraphOpen));
				}
				else if (string.Equals(tag, "b"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BoldOpen));
				}
				else if (string.Equals(tag, "i"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ItalicOpen));
				}
				else if (string.Equals(tag, "u"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.UnderlineOpen));
				}
				else if (string.Equals(tag, "*"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ListItemOpen));
				}
				else if (string.Equals(tag, "list"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BulletListOpen));
				}
				else if (string.Equals(tag, "olist"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.OrderedListOpen));
				}
				else if (string.Equals(tag, "h1"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H1Open));
				}
				else if (string.Equals(tag, "h2"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H2Open));
				}
				else if (string.Equals(tag, "h3"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H3Open));
				}
				else if (string.Equals(tag, "url"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.UrlOpen, value));
				}
				else if (string.Equals(tag, "img"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ImgOpen, value));
				}
				else if (string.Equals(tag, "previewyoutube"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.PreviewYouTubeOpen, value));
				}
				else if (string.Equals(tag, "quote"))
				{
					outputTokens.Add(new BbCodeToken(EBbCodeTokenType.QuoteOpen, value));
				}
				else
				{
					isNotATag = true;
				}
			}

			if (isNotATag)
			{
				outputTokens.Add(new BbCodeToken(EBbCodeTokenType.String, stringBuilder.ToString()));
			}
		}

		private System.IO.TextReader inputReader;
		private int currentLineNumber;
		private int currentReadResult;
		private char currentChar;
		private bool hasChar;

		private bool hasError;
		private string errorMessage;

		private System.Text.StringBuilder tagStringBuilder;
		private System.Text.StringBuilder stringBuilder;
	}
}
