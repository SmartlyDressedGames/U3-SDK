////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Text.RegularExpressions;
using UnityEngine;

namespace SDG.Unturned
{
	public static class RichTextUtil
	{
		// Remove all of the rich formatting so that the shadow text displays correctly.
		// </*color.*?>
		// * matches 0 or more of the previous character (/), so that both <c and </c are caught
		// . matches any character, and * again allows 0 or more, but is limited by ? making it catch as few as possible. This catches the =#COLOR part of <color=#abcabc>
		private static Regex richTextColorTagRegex = new Regex("</*color.*?>", RegexOptions.IgnoreCase);

		/// <summary>
		/// Remove all color rich formatting so that shadow text displays correctly.
		/// </summary>
		public static string replaceColorTags(string text)
		{
			// Nelson 2024-12-19: Return as-is if null/empty because Replace throws an exception if passed a null string.
			return string.IsNullOrEmpty(text) ? text : richTextColorTagRegex.Replace(text, string.Empty);
		}

		/// <summary>
		/// Shadow text needs the color tags removed, otherwise the shadow uses those colors.
		/// </summary>
		public static GUIContent makeShadowContent(GUIContent content)
		{
			return new GUIContent(replaceColorTags(content.text), replaceColorTags(content.tooltip));
		}

		/// <summary>
		/// Wrap text with color tags.
		/// </summary>
		public static string wrapWithColor(string text, string color)
		{
			return string.Format("<color={0}>{1}</color>", color, text);
		}

		/// <summary>
		/// Wrap text with color tags.
		/// </summary>
		public static string wrapWithColor(string text, Color32 color)
		{
			return wrapWithColor(text, Palette.hex(color));
		}

		/// <summary>
		/// Wrap text with color tags.
		/// </summary>
		public static string wrapWithColor(string text, Color color)
		{
			return wrapWithColor(text, (Color32) color);
		}

		/// <summary>
		/// Replace br tags with newlines.
		/// </summary>
		public static void replaceNewlineMarkup(ref string s)
		{
			s = s.Replace("<br>", "\n");
		}

		/// <summary>
		/// Should player be allowed to write given text on a sign?
		/// Keep in mind that newer signs use TMP, whereas older signs use uGUI.
		/// </summary>
		public static bool isTextValidForSign(string text)
		{
			if (string.IsNullOrEmpty(text))
				return true;

			if (text.IndexOf("<size", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false; // Font size tags can be abused to write huge text.

			if (text.IndexOf("<voffset", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false; // Vertical offset tags can be abused to write the text in the sky.

			if (text.IndexOf("<sprite", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false; // Sprites can be abused to lag or crash the game it seems.

			return true;
		}

		/// <summary>
		/// Disable style, align, and space because they make server list unfair.
		/// </summary>
		internal static bool IsTextValidForServerListShortDescription(string text)
		{
			if (string.IsNullOrEmpty(text))
				return true;

			if (!isTextValidForSign(text))
				return false;

			if (text.IndexOf("<style", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false;

			if (text.IndexOf("<align", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false;

			if (text.IndexOf("<space", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false;

			if (text.IndexOf("<scale", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false;

			if (text.IndexOf("<pos", System.StringComparison.OrdinalIgnoreCase) != -1)
				return false;

			return true;
		}
	}
}
