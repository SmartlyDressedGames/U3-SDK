////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public enum EBbCodeWidgetType
	{
		RichTextLabel,
		Image,
		YouTubeButton,
		LinkButton,
	}

	public struct BbCodeWidget
	{
		public EBbCodeWidgetType widgetType;
		public string widgetData;

		public BbCodeWidget(EBbCodeWidgetType widgetType, string widgetData)
		{
			this.widgetType = widgetType;
			this.widgetData = widgetData;
		}
	}

	/// <summary>
	/// Converts Steam BBcode tokens into widgets displayable using Glazier UI.
	/// </summary>
	public class BbCodeWidgetConverter
	{
		public BbCodeWidgetConverter()
		{
			richTextStringBuilder = new System.Text.StringBuilder();
		}

		/// <summary>
		/// If false, expect LineBreak tokens in input. (default false)
		/// If true, insert line breaks where appropriate.
		/// Steam's new visual editor doesn't emit newlines, instead inferring line breaks from paragraph blocks. To
		/// make life easier we will do the same for the main menu announcement feed.
		/// </summary>
		public bool InferLineBreaks
		{
			get => _inferLineBreaks;
			set => _inferLineBreaks = value;
		}
		private bool _inferLineBreaks = false;

		public List<BbCodeWidget> Convert(List<BbCodeToken> tokens)
		{
			richTextStringBuilder.Clear();
			inputTokens = tokens;
			inputIndex = -1;
			hasToken = false;

			List<BbCodeWidget> outputWidgets = new List<BbCodeWidget>();

			AdvanceToken();

			int readCount = 0;
			while (hasToken)
			{
				ConvertToken(outputWidgets);

				++readCount;
				if (readCount >= 10000)
				{
					ErrorMessage = "Infinite loop attempting to convert tokens into widgets";
					break;
				}
			}

			return outputWidgets;
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

		private void AdvanceToken()
		{
			++inputIndex;
			hasToken = inputIndex < inputTokens.Count;
			if (hasToken)
			{
				currentToken = inputTokens[inputIndex];
			}
		}

		private EBbCodeTokenType PeekNextTokenType()
		{
			if (inputIndex + 1 < inputTokens.Count)
			{
				return inputTokens[inputIndex + 1].tokenType;
			}
			else
			{
				return EBbCodeTokenType.Invalid;
			}
		}

		private void ConvertToken(List<BbCodeWidget> outputWidgets)
		{
			if (currentToken.tokenType == EBbCodeTokenType.PreviewYouTubeOpen)
			{
				ConvertPreviewYouTube(outputWidgets);
			}
			else if (currentToken.tokenType == EBbCodeTokenType.ImgOpen)
			{
				ConvertImage(outputWidgets);
			}
			else if (currentToken.tokenType == EBbCodeTokenType.UrlOpen)
			{
				ConvertLinkButton(outputWidgets);
			}
			else
			{
				ConvertRichText(outputWidgets);
			}
		}

		private void ConvertPreviewYouTube(List<BbCodeWidget> outputWidgets)
		{
			string ytValue = currentToken.GetUnquotedValue();
			if (!string.IsNullOrEmpty(ytValue))
			{
				int delimiterIndex = ytValue.IndexOf(';');
				string videoId;
				if (delimiterIndex > 0)
				{
					videoId = ytValue.Substring(0, delimiterIndex);
				}
				else
				{
					videoId = ytValue;
				}
				outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.YouTubeButton, videoId));
			}

			AdvanceToken();
			if (hasToken && currentToken.tokenType == EBbCodeTokenType.PreviewYouTubeClose)
			{
				AdvanceToken();
			}
			if (hasToken && currentToken.tokenType == EBbCodeTokenType.LineBreak)
			{
				// Skip next line break because video will create one.
				AdvanceToken();
			}
		}

		private void ConvertImage(List<BbCodeWidget> outputWidgets)
		{
			bool hasSrcKey = currentToken.TryParseValue("src", out string imageUrl);

			AdvanceToken();
			if (hasToken && currentToken.tokenType == EBbCodeTokenType.String)
			{
				if (!hasSrcKey)
				{
					imageUrl = currentToken.tokenValue;
				}

				outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.Image, imageUrl));
				AdvanceToken();

				if (hasToken && currentToken.tokenType == EBbCodeTokenType.ImgClose)
				{
					AdvanceToken();
				}
			}
			else if (hasToken && currentToken.tokenType == EBbCodeTokenType.ImgClose)
			{
				if (hasSrcKey)
				{
					outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.Image, imageUrl));
				}
				AdvanceToken();
			}

			if (hasToken && currentToken.tokenType == EBbCodeTokenType.LineBreak)
			{
				// Skip next line break because image will create one.
				AdvanceToken();
			}
		}

		private void ConvertLinkButton(List<BbCodeWidget> outputWidgets)
		{
			string url = currentToken.GetUnquotedValue();
			string displayText = null;
			AdvanceToken();
			if (hasToken && currentToken.tokenType == EBbCodeTokenType.String)
			{
				if (string.IsNullOrEmpty(url))
				{
					url = currentToken.tokenValue;
				}
				else
				{
					displayText = currentToken.tokenValue;
				}
				AdvanceToken();
			}
			if (hasToken && currentToken.tokenType == EBbCodeTokenType.UrlClose)
			{
				AdvanceToken();
			}

			if (hasToken && currentToken.tokenType == EBbCodeTokenType.LineBreak)
			{
				// Skip next line break because link button will create one.
				AdvanceToken();
			}

			if (string.IsNullOrEmpty(displayText))
			{
				outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.LinkButton, url));
			}
			else
			{
				outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.LinkButton, $"{url},{displayText}"));
			}
		}

		private void ConvertRichText(List<BbCodeWidget> outputWidgets)
		{
			richTextStringBuilder.Clear();

			bool isInsideOrderedList = false;
			int orderedListIndex = 0;
			bool wasPreviousCharLineBreak = true; // Prevent line break on first line.

			do
			{
				bool wantedToInsertLineBreak = false;
				switch (currentToken.tokenType)
				{
					case EBbCodeTokenType.String:
						richTextStringBuilder.Append(currentToken.tokenValue);
						break;

					case EBbCodeTokenType.BoldOpen:
						richTextStringBuilder.Append("<b>");
						break;

					case EBbCodeTokenType.BoldClose:
						richTextStringBuilder.Append("</b>");
						break;

					case EBbCodeTokenType.ParagraphClose:
					case EBbCodeTokenType.ListItemClose:
						if (_inferLineBreaks)
						{
							if (!wasPreviousCharLineBreak)
							{
								richTextStringBuilder.Append('\n');
							}
							switch (PeekNextTokenType())
							{
								case EBbCodeTokenType.ParagraphOpen:
								case EBbCodeTokenType.H1Open:
								case EBbCodeTokenType.H2Open:
								case EBbCodeTokenType.H3Open:
									richTextStringBuilder.Append('\n');
									break;
							}
						}
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.ItalicOpen:
						richTextStringBuilder.Append("<i>");
						break;

					case EBbCodeTokenType.ItalicClose:
						richTextStringBuilder.Append("</i>");
						break;

					case EBbCodeTokenType.H1Open:
						if (!wasPreviousCharLineBreak && _inferLineBreaks)
						{
							richTextStringBuilder.Append("\n\n");
						}
						wantedToInsertLineBreak = true;
						richTextStringBuilder.Append("<size=20>"); // ESleekFontSize.Large
						break;

					case EBbCodeTokenType.H1Close:
						richTextStringBuilder.Append("</size>");
						if (_inferLineBreaks)
						{
							richTextStringBuilder.Append("\n\n");
						}
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.H2Open:
						if (!wasPreviousCharLineBreak && _inferLineBreaks)
						{
							richTextStringBuilder.Append("\n\n");
						}
						wantedToInsertLineBreak = true;
						richTextStringBuilder.Append("<size=17>");
						break;

					case EBbCodeTokenType.H2Close:
						richTextStringBuilder.Append("</size>");
						if (_inferLineBreaks)
						{
							richTextStringBuilder.Append("\n\n");
						}
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.H3Open:
						if (!wasPreviousCharLineBreak && _inferLineBreaks)
						{
							richTextStringBuilder.Append("\n\n");
						}
						wantedToInsertLineBreak = true;
						richTextStringBuilder.Append("<size=14>"); // ESleekFontSize.Medium
						break;

					case EBbCodeTokenType.H3Close:
						richTextStringBuilder.Append("</size>");
						if (_inferLineBreaks)
						{
							richTextStringBuilder.Append("\n\n");
						}
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.UrlOpen:
					case EBbCodeTokenType.UrlClose:
						// See below for inline link reasoning.
						break;

					case EBbCodeTokenType.BulletListOpen:
					case EBbCodeTokenType.BulletListClose:
						// Nelson 2024-10-17: Separate label for list contents to work around public issue #4745.
						PushPendingRichText(outputWidgets);
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.OrderedListOpen:
						isInsideOrderedList = true;
						orderedListIndex = 0;
						// Nelson 2024-10-17: Separate label for list contents to work around public issue #4745.
						PushPendingRichText(outputWidgets);
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.OrderedListClose:
						isInsideOrderedList = false;
						// Nelson 2024-10-17: Separate label for list contents to work around public issue #4745.
						PushPendingRichText(outputWidgets);
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.ListItemOpen:
						if (!wasPreviousCharLineBreak && _inferLineBreaks)
						{
							richTextStringBuilder.Append('\n');
						}
						wantedToInsertLineBreak = true;
						if (isInsideOrderedList)
						{
							richTextStringBuilder.Append(orderedListIndex + 1);
							richTextStringBuilder.Append(". ");
							++orderedListIndex;
						}
						else
						{
							richTextStringBuilder.Append("• ");
						}
						break;

					case EBbCodeTokenType.LineBreak:
						richTextStringBuilder.Append('\n');
						wantedToInsertLineBreak = true;
						break;

					case EBbCodeTokenType.QuoteOpen:
						// Nelson 2025-01-29: Molt tested the announcement display, and it doesn't show the author.
						// I'll keep it if non-empty in case we use this elsewhere. Otherwise, for announcements, it's
						// essentially an indented block.
						if (string.IsNullOrEmpty(currentToken.tokenValue))
						{
							richTextStringBuilder.Append($"<indent=2em>");
						}
						else
						{
							richTextStringBuilder.Append($"<indent=2em><b>{currentToken.tokenValue}:</b>\n");
						}
						break;

					case EBbCodeTokenType.QuoteClose:
						richTextStringBuilder.Append("</indent>");
						if (_inferLineBreaks)
						{
							richTextStringBuilder.Append('\n');
						}
						wantedToInsertLineBreak = true;
						break;
				}

				EBbCodeTokenType previousTokenType = currentToken.tokenType;
				AdvanceToken();
				if (currentToken.tokenType == EBbCodeTokenType.PreviewYouTubeOpen
					|| currentToken.tokenType == EBbCodeTokenType.ImgOpen)
				{
					break;
				}

				// Special handling for inline links: if they're on their own line we create a button, otherwise we
				// insert them as regular text.
				if (currentToken.tokenType == EBbCodeTokenType.UrlOpen && (previousTokenType == EBbCodeTokenType.LineBreak || previousTokenType == EBbCodeTokenType.ParagraphOpen))
				{
					break;
				}

				wasPreviousCharLineBreak = wantedToInsertLineBreak;
			}
			while (hasToken);

			PushPendingRichText(outputWidgets);
		}

		private void PushPendingRichText(List<BbCodeWidget> outputWidgets)
		{
			if (richTextStringBuilder.Length > 0)
			{
				string text = richTextStringBuilder.ToString();
				outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.RichTextLabel, text));
				richTextStringBuilder.Clear();
			}
		}

		private List<BbCodeToken> inputTokens;
		private int inputIndex;
		private bool hasToken;
		private BbCodeToken currentToken;

		private bool hasError;
		private string errorMessage;

		private System.Text.StringBuilder richTextStringBuilder;
	}
}
