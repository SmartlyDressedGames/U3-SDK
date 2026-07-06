////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace Unturned.SystemEx
{
	/// <summary>
	/// Join replaces Combine in newer versions of .NET. Not supported by Unity yet.
	/// </summary>
	public static class PathEx
	{
		public static string Join(DirectoryInfo path1, string path2)
		{
			return Path.Combine(path1.FullName, path2);
		}

		public static string Join(DirectoryInfo path1, string path2, string path3)
		{
			return Path.Combine(path1.FullName, path2, path3);
		}

		public static string Join(DirectoryInfo path1, string path2, string path3, string path4)
		{
			return Path.Combine(path1.FullName, path2, path3, path4);
		}

		public static string Join(DirectoryInfo path1, string path2, string path3, string path4, string path5)
		{
			return Path.Combine(path1.FullName, path2, path3, path4, path5);
		}

		public static string Join(DirectoryInfo path1, string path2, string path3, string path4, string path5, string path6)
		{
			return Path.Combine(path1.FullName, path2, path3, path4, path5, path6);
		}

		public static string ReplaceInvalidFileNameChars(string input, char replacement)
		{
			// Only create string builder if necessary.
			System.Text.StringBuilder sb = null;

			int inputLength = input.Length;
			for (int charIndex = 0; charIndex < inputLength; ++charIndex)
			{
				char inputChar = input[charIndex];
				if (System.Array.IndexOf(invalidFileNameChars, inputChar) >= 0)
				{
					if (sb == null)
					{
						sb = new System.Text.StringBuilder(inputLength);
						if (charIndex > 0)
						{
							// Catch up to where we are.
							sb.Append(input, 0, charIndex);
						}
					}

					sb.Append(replacement);
				}
				else if (sb != null)
				{
					sb.Append(inputChar);
				}
			}

			if (sb != null)
			{
				return sb.ToString();
			}
			else
			{
				return input;
			}
		}

		static PathEx()
		{
			invalidFileNameChars = Path.GetInvalidFileNameChars();
		}

		private static char[] invalidFileNameChars;
	}
}
