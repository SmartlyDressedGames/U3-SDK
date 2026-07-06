////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NameTool
	{
		public static bool checkNames(string input, string name)
		{
			return input.Length <= name.Length && name.ToLower().IndexOf(input.ToLower()) != -1;
		}

		/// <summary>
		/// If updating this method please remember to update the support article:
		/// https://support.smartlydressedgames.com/hc/en-us/articles/13452208765716
		/// </summary>
		public static bool isValid(string name)
		{
			foreach (char letter in name)
			{
				if (letter <= 31)
				{
					return false;
				}

				if (letter >= 126)
				{
					return false;
				}

				if (letter == '/' || letter == '\\' || letter == '`' || letter == '\'' || letter == '"')
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Does name contain rich text tags?
		/// Some players were abusing rich text enabled servers by inserting admin colors into their steam name.
		/// </summary>
		public static bool containsRichText(string name)
		{
			int openingIndex = name.IndexOf('<');
			if (openingIndex < 0)
			{
				return false;
			}

			int closingIndex = name.IndexOf('>', openingIndex + 1);
			if (closingIndex < 0)
			{
				return false;
			}

			// Nelson 2025-01-28: Making this stricter. If it has an opening and closing angle brackets, block.
			// (public issue #4861)
			return true;
		}
	}
}
