////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Splits string and compares substrings ignoring case.
	/// Tokens containing a colon ':' are ignored so that they can represent special filters like MasterBundleSearchFilter.
	/// </summary>
	public struct TokenSearchFilter
	{
		public static TokenSearchFilter? parse(string filter)
		{
			if (string.IsNullOrEmpty(filter))
				return null;

			workingTokens.Clear();

			int fromIndex = 0;
			while (fromIndex < filter.Length)
			{
				int nextIndexOfSeparator = filter.IndexOf(' ', fromIndex);
				int indexOfColon = filter.IndexOf(':', fromIndex);

				if (nextIndexOfSeparator < 0)
				{
					// Reached end of string.
					nextIndexOfSeparator = filter.Length;
				}

				bool nextTokenContainsColon = indexOfColon >= 0 && indexOfColon < nextIndexOfSeparator;
				if (nextTokenContainsColon == false)
				{
					int tokenLength = nextIndexOfSeparator - fromIndex;
					if (tokenLength > 0)
					{
						string token = filter.Substring(fromIndex, nextIndexOfSeparator - fromIndex);
						workingTokens.Add(token);
					}
				}

				fromIndex = nextIndexOfSeparator + 1;
			}

			if (workingTokens.Count < 1)
				return null;

			string[] tokens = workingTokens.ToArray();
			return new TokenSearchFilter(tokens);
		}

		public bool ignores(string name)
		{
			foreach (string token in tokens)
			{
				int indexOfToken = name.IndexOf(token, StringComparison.InvariantCultureIgnoreCase);
				if (indexOfToken < 0)
				{
					// Does not contain token, so ignore.
					return true;
				}
			}

			return false;
		}

		public bool matches(string name)
		{
			return ignores(name) == false;
		}

		public TokenSearchFilter(string[] tokens)
		{
			this.tokens = tokens;
		}

		private string[] tokens;
		private static List<string> workingTokens = new List<string>();
	}
}
