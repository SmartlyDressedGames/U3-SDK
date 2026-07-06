////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public static class DatNodeEx
	{
		public static string DebugDumpToString(this IDatNode node)
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			node.DebugDumpToStringBuilder(output);
			return output.ToString();
		}

		public static bool TryParseStruct<T>(this IDatNode node, out T value) where T : struct, IDatParseable
		{
			value = default;
			return value.TryParse(node);
		}

		public static T ParseStruct<T>(this IDatNode node, T defaultValue = default) where T : struct, IDatParseable
		{
			return TryParseStruct<T>(node, out T value) ? value : defaultValue;
		}

		/// <summary>
		/// Recursively traverses node hierarchy to build a string path to this node.
		/// </summary>
		public static bool TryGetNodePath(this IDatNode node, out string path)
		{
			if (!node.TryGetParentNode(out IDatNode parentNode))
			{
				path = null;
				return false;
			}

			// We did get parent node.
			if (parentNode == null)
			{
				// Root!
				path = string.Empty;
				return true;
			}

			if (!TryGetNodePath(parentNode, out string parentPath))
			{
				path = null;
				return false;
			}

			if (parentNode is IDatDictionary parentDictionary)
			{
				string key = null;
				foreach (KeyValuePair<string, IDatNode> kvp in parentDictionary)
				{
					if (kvp.Value == node)
					{
						key = kvp.Key;
						break;
					}
				}

				if (key == null)
				{
					// Something is in a bad state here. :S
					path = null;
					return false;
				}

				path = $"{parentPath}/{key}";
				return true;
			}
			else if (parentNode is IDatList parentList)
			{
				int index = parentList.IndexOf(node);
				if (index < 0)
				{
					// Something is in a bad state here. :S
					path = null;
					return false;
				}

				path = $"{parentPath}/{index}";
				return true;
			}
			else
			{
				// ???
				path = null;
				return false;
			}
		}

		/// <summary>
		/// Useful when code knows path will be available.
		/// </summary>
		public static string GetPath(this IDatNode node)
		{
			return TryGetNodePath(node, out string path) ? path : null;
		}

		/// <summary>
		/// Useful when code knows parsed line number will be available.
		/// </summary>
		public static int GetParsedLineNumber(this IDatNode node)
		{
			return node.TryGetParsedLineNumber(out int lineNumber) ? lineNumber : -1;
		}
	}
}
