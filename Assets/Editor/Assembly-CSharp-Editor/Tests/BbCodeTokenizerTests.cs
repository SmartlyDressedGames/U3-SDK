////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using System.Collections.Generic;

internal class BbCodeTokenizerTests
{
	[Test]
	public void TokenizeStringOneLine()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("Hello, world!");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(1, tokens.Count);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[0].tokenType);
		Assert.AreEqual("Hello, world!", tokens[0].tokenValue);
	}

	[Test]
	public void TokenizeStringTwoLines()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("Hello,\nworld!");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[0].tokenType);
		Assert.AreEqual("Hello,", tokens[0].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[1].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[2].tokenType);
		Assert.AreEqual("world!", tokens[2].tokenValue);
	}

	[Test]
	public void TokenizeBold()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[b]Hello, world![/b]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count);
		Assert.AreEqual(EBbCodeTokenType.BoldOpen, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("Hello, world!", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.BoldClose, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeItalics()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[i]Hello, world![/i]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.ItalicOpen, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("Hello, world!", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.ItalicClose, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeH1()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[h1]Hello, world![/h1]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.H1Open, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("Hello, world!", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.H1Close, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeH2()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[h2]Hello, world![/h2]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.H2Open, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("Hello, world!", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.H2Close, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeH3()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[h3]Hello, world![/h3]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.H3Open, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("Hello, world!", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.H3Close, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeUrl()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[url=https://smartlydressedgames.com/]SDG Website[/url]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.UrlOpen, tokens[0].tokenType);
		Assert.AreEqual("https://smartlydressedgames.com/", tokens[0].GetUnquotedValue());
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("SDG Website", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.UrlClose, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeUrlQuoted()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[url=\"https://smartlydressedgames.com/\"]SDG Website[/url]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.UrlOpen, tokens[0].tokenType);
		Assert.AreEqual("https://smartlydressedgames.com/", tokens[0].GetUnquotedValue());
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("SDG Website", tokens[1].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.UrlClose, tokens[2].tokenType);
	}

	[Test]
	public void TokenizeImg()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[img]{STEAM_CLAN_IMAGE}/6119952/fc8705208415f8ea63641b999ac28c36cfef1c10.jpg[/img]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(3, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.ImgOpen, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[1].tokenType);
		Assert.AreEqual("{STEAM_CLAN_IMAGE}/6119952/fc8705208415f8ea63641b999ac28c36cfef1c10.jpg", tokens[1].GetUnquotedValue());
		Assert.AreEqual(EBbCodeTokenType.ImgClose, tokens[2].tokenType);
	}

	/// <summary>
	/// Nelson 2025-07-02: Steam's new announcement visual editor changes how img tags are formatted.
	/// </summary>
	[Test]
	public void TokenizeImgWithSrcKeyPair()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[img src=\"{STEAM_CLAN_IMAGE}/6119952/fc8705208415f8ea63641b999ac28c36cfef1c10.jpg\"][/img]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(2, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.ImgOpen, tokens[0].tokenType);
		Assert.IsTrue(tokens[0].TryParseValue("src", out string imgUrl), $"parsed src from {tokens[0].tokenValue}");
		Assert.AreEqual("{STEAM_CLAN_IMAGE}/6119952/fc8705208415f8ea63641b999ac28c36cfef1c10.jpg", imgUrl);
		Assert.AreEqual(EBbCodeTokenType.ImgClose, tokens[1].tokenType);
	}

	[Test]
	public void TokenizePreviewYouTube()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[previewyoutube=CXs71qEvLNU;full][/previewyoutube]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(2, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.PreviewYouTubeOpen, tokens[0].tokenType);
		Assert.AreEqual("CXs71qEvLNU;full", tokens[0].GetUnquotedValue());
		Assert.AreEqual(EBbCodeTokenType.PreviewYouTubeClose, tokens[1].tokenType);
	}

	[Test]
	public void TokenizeBulletList()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[list]\n[*]Item A\n[*]Item B\n[/list]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(9, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.BulletListOpen, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[1].tokenType);
		Assert.AreEqual(EBbCodeTokenType.ListItemOpen, tokens[2].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[3].tokenType);
		Assert.AreEqual("Item A", tokens[3].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[4].tokenType);
		Assert.AreEqual(EBbCodeTokenType.ListItemOpen, tokens[5].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[6].tokenType);
		Assert.AreEqual("Item B", tokens[6].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[7].tokenType);
		Assert.AreEqual(EBbCodeTokenType.BulletListClose, tokens[8].tokenType);
	}

	[Test]
	public void TokenizeOrderedList()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[olist]\n[*]Item A\n[*]Item B\n[/olist]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(9, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.OrderedListOpen, tokens[0].tokenType);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[1].tokenType);
		Assert.AreEqual(EBbCodeTokenType.ListItemOpen, tokens[2].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[3].tokenType);
		Assert.AreEqual("Item A", tokens[3].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[4].tokenType);
		Assert.AreEqual(EBbCodeTokenType.ListItemOpen, tokens[5].tokenType);
		Assert.AreEqual(EBbCodeTokenType.String, tokens[6].tokenType);
		Assert.AreEqual("Item B", tokens[6].tokenValue);
		Assert.AreEqual(EBbCodeTokenType.LineBreak, tokens[7].tokenType);
		Assert.AreEqual(EBbCodeTokenType.OrderedListClose, tokens[8].tokenType);
	}

	/// <summary>
	/// Text wrapped in brackets [] but not matching a tag should be included as-is. For example, a lot of the update
	/// notes adding new items with legacy IDs mention the IDs wrapped in brackets like: Eaglefire [ID 4]
	/// </summary>
	[Test]
	public void TokenizeNotAToken()
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize("[IDs 45-60]");
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		Assert.AreEqual(1, tokens.Count, tokenizer.DebugDumpTokensToString(tokens));
		Assert.AreEqual(EBbCodeTokenType.String, tokens[0].tokenType);
		Assert.AreEqual("[IDs 45-60]", tokens[0].tokenValue);
	}

	[TestCase("[b][i]Bold Italic[/i][/b]", "<b><i>Bold Italic</i></b>")]
	[TestCase("[b][i]Bold Italic\nMulti-Line[/i][/b]", "<b><i>Bold Italic<br>Multi-Line</i></b>")]
	[TestCase("[h1]header[/h1]\n[h2]sub-header[/h2]\n[h3]sub-sub-header[/h3]", "<h1>header</h1><br><h2>sub-header</h2><br><h3>sub-sub-header</h3>")]
	public void TestConversion(string input, string expectedOutput)
	{
		BbCodeTokenizer tokenizer = new BbCodeTokenizer();
		List<BbCodeToken> tokens = tokenizer.Tokenize(input);
		Assert.IsFalse(tokenizer.HasError, $"Error message: \"{tokenizer.ErrorMessage}\"");
		string actualOutput = TokensToRichText(tokens);
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	/// <summary>
	/// Purpose: For more thorough testing this allows us to use TestCase attributes to provide expected output in HTML.
	/// </summary>
	private static string TokensToRichText(List<BbCodeToken> tokens)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		foreach (BbCodeToken token in tokens)
		{
			switch (token.tokenType)
			{
				case EBbCodeTokenType.String:
					sb.Append(token.tokenValue);
					break;

				case EBbCodeTokenType.BoldOpen:
					sb.Append("<b>");
					break;

				case EBbCodeTokenType.BoldClose:
					sb.Append("</b>");
					break;

				case EBbCodeTokenType.ItalicOpen:
					sb.Append("<i>");
					break;

				case EBbCodeTokenType.ItalicClose:
					sb.Append("</i>");
					break;

				case EBbCodeTokenType.BulletListOpen:
					sb.Append("<ul>");
					break;

				case EBbCodeTokenType.BulletListClose:
					sb.Append("</ul>");
					break;

				case EBbCodeTokenType.OrderedListOpen:
					sb.Append("<ol>");
					break;

				case EBbCodeTokenType.OrderedListClose:
					sb.Append("</ol>");
					break;

				case EBbCodeTokenType.ListItemOpen:
					sb.Append("<li>");
					sb.Append(token.tokenValue);
					sb.Append("</li>");
					break;

				case EBbCodeTokenType.H1Open:
					sb.Append("<h1>");
					break;

				case EBbCodeTokenType.H1Close:
					sb.Append("</h1>");
					break;

				case EBbCodeTokenType.H2Open:
					sb.Append("<h2>");
					break;

				case EBbCodeTokenType.H2Close:
					sb.Append("</h2>");
					break;

				case EBbCodeTokenType.H3Open:
					sb.Append("<h3>");
					break;

				case EBbCodeTokenType.H3Close:
					sb.Append("</h3>");
					break;

				case EBbCodeTokenType.UrlOpen:
					sb.Append("<a href=");
					sb.Append(token.GetUnquotedValue());
					sb.Append(">");
					break;

				case EBbCodeTokenType.UrlClose:
					sb.Append("</a>");
					break;

				case EBbCodeTokenType.ImgOpen:
					sb.Append("<img>");
					break;

				case EBbCodeTokenType.ImgClose:
					sb.Append("</img>");
					break;

				case EBbCodeTokenType.PreviewYouTubeOpen:
					sb.Append("<yt>");
					break;

				case EBbCodeTokenType.PreviewYouTubeClose:
					sb.Append("</yt>");
					break;

				case EBbCodeTokenType.LineBreak:
					sb.Append("<br>");
					break;
			}
		}

		return sb.ToString();
	}
}
