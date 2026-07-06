////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace Unturned.SystemEx
{
	public static class StringExtension
	{
		public static bool ContainsChar(this string s, char value)
		{
			if (string.IsNullOrEmpty(s))
				return false;

			foreach (char c in s)
			{
				if (c == value)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Split string into non-empty lines.
		/// </summary>
		public static string[] SplitLines(this string s)
		{
			// Handles Windows \r\n by removing the empty entry between \r and \n.
			return s.Split(SplitNewLineSeparators, System.StringSplitOptions.RemoveEmptyEntries);
		}
		private static readonly char[] SplitNewLineSeparators = { '\r', '\n' };

		/// <summary>
		/// Split string into lines, including blank lines.
		/// </summary>
		public static string[] SplitLinesIncludingEmpty(this string s)
		{
			return s.Split(SplitNewLineSeparatorStrings, System.StringSplitOptions.None);
		}
		private static string[] SplitNewLineSeparatorStrings = new string[2] { "\r\n", "\n" };

		/// <summary>
		/// Consider all platform new line characters.
		/// Windows: \r\n Mac: \r Linux: \n
		/// </summary>
		public static bool ContainsNewLine(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return false;

			foreach (char c in s)
			{
				if (c == '\n' || c == '\r')
				{
					return true;
				}
			}

			return false;
		}

		public static int CountChar(this string s, char value)
		{
			if (string.IsNullOrEmpty(s))
				return 0;

			int count = 0;
			foreach (char c in s)
			{
				if (c == value)
				{
					++count;
				}
			}
			return count;
		}

		public static int CountNewlines(this string s)
		{
			return s.CountChar('\n');
		}

		public static bool Contains(this string s, string value, System.StringComparison comparisonType)
		{
			return s.IndexOf(value, comparisonType) >= 0;
		}
	}
}
