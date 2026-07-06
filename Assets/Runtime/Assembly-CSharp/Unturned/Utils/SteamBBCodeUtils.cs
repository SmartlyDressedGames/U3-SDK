////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public static class SteamBBCodeUtils
	{
		/// <summary>
		/// In-game rich text does not support embedded YouTube videos, but they look great in the web browser,
		/// so we simply remove them from the in-game text.
		/// </summary>
		public static void removeYouTubePreviews(ref string bbcode)
		{
			// Example: [previewyoutube=AFuCXBU2Onk;full][/previewyoutube]
			const string openingMarkup = "[previewyoutube=";
			const string closingMarkup = "[/previewyoutube]";

			int searchIndex = 0;
			while (searchIndex < bbcode.Length)
			{
				int openingIndex = bbcode.IndexOf(openingMarkup, searchIndex);
				if (openingIndex < 0)
				{
					// Finished removing all YouTube tags.
					return;
				}

				int closingIndex = bbcode.IndexOf(closingMarkup, openingIndex + openingMarkup.Length);
				if (closingIndex < 0)
				{
					// Incorrectly formatted markup.
					return;
				}

				bbcode = bbcode.Remove(openingIndex, closingIndex + closingMarkup.Length - openingIndex);
				searchIndex = openingIndex;
			}
		}

		/// <summary>
		/// Unfortunately in-game rich text does not have code formatting yet, so remove the tags while preserving text.
		/// </summary>
		public static void removeCodeFormatting(ref string bbcode)
		{
			bbcode = bbcode.Replace("[code]", string.Empty);
			bbcode = bbcode.Replace("[/code]", string.Empty);
		}
	}
}
