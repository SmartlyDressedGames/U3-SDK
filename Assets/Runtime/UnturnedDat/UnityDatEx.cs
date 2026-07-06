////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class UnityDatEx
	{
		/// <summary>
		/// Parse from 2 comma-delimited floats optionally surrounded by parenthesis. (e.g. "1, 2" OR "(1, 2)")
		/// </summary>
		/// <returns>True if both floats were parsed successfully, false if empty or otherwise unable to parse.</returns>
		public static bool TryParseVector2(this IDatValue node, out Vector2 value)
		{
			if (string.IsNullOrEmpty(node.Value))
			{
				value = default;
				return false;
			}

			int startIndex;
			int endIndex;

			int openingParenthesisIndex = node.Value.IndexOf('(');
			int closingParenthesisIndex;
			if (openingParenthesisIndex >= 0)
			{
				closingParenthesisIndex = node.Value.IndexOf(')', openingParenthesisIndex + 2);
				if (closingParenthesisIndex < 0)
				{
					value = default;
					return false;
				}

				startIndex = openingParenthesisIndex + 1;
				endIndex = closingParenthesisIndex - 1;
			}
			else
			{
				startIndex = 0;
				endIndex = node.Value.Length - 1;
			}

			int delimiterIndex = node.Value.IndexOf(',', startIndex);
			if (delimiterIndex < 0 || delimiterIndex + 1 > endIndex)
			{
				value = default;
				return false;
			}

			if (!float.TryParse(node.Value.Substring(startIndex, delimiterIndex - startIndex), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value.x))
			{
				value = default;
				return false;
			}

			if (!float.TryParse(node.Value.Substring(delimiterIndex + 1, endIndex - delimiterIndex), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value.y))
			{
				value = default;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parse from 2 comma-delimited floats. (e.g. 1, 2)
		/// </summary>
		/// <returns>Vector if both floats were parsed successfully, defaultValue if empty or otherwise unable to parse.</returns>
		public static Vector2 ParseVector2(this IDatValue node, Vector2 defaultValue = default)
		{
			return TryParseVector2(node, out Vector2 value) ? value : defaultValue;
		}

		/// <summary>
		/// Parse from either 2 comma-delimited floats (e.g., 1, 2), or a sub-dictionary.
		/// </summary>
		/// <returns>True if both floats were parsed successfully OR if sub-dictionary exists (regardless of validity).
		/// False if empty, missing, not a value/dictionary, or otherwise unable to parse.</returns>
		public static bool TryParseVector2(this IDatDictionary dictionary, string key, out Vector2 value)
		{
			if (!dictionary.TryGetNode(key, out IDatNode node))
			{
				value = default;
				return false;
			}

			if (node is IDatValue valueNode)
			{
				return TryParseVector2(valueNode, out value);
			}
			else if (node is IDatDictionary dictionaryNode)
			{
				dictionaryNode.TryParseFloat("X", out value.x);
				dictionaryNode.TryParseFloat("Y", out value.y);
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}

		/// <summary>
		/// Parse from either 2 comma-delimited floats (e.g., 1, 2), or a sub-dictionary.
		/// </summary>
		/// <returns>Vector if both floats were parsed successfully OR if sub-dictionary exists (regardless of validity).
		/// defaultValue if empty, missing, not a value/dictionary, or otherwise unable to parse.</returns>
		public static Vector2 ParseVector2(this IDatDictionary dictionary, string key, Vector2 defaultValue = default)
		{
			return TryParseVector2(dictionary, key, out Vector2 value) ? value : defaultValue;
		}

		/// <summary>
		/// Parse from 3 comma-delimited floats optionally surrounded by parenthesis. (e.g. "1, 2, 3" OR "(1, 2, 3)")
		/// Note: Duplicated at Vector3Ex.TryParseVector3.
		/// </summary>
		/// <returns>True if all three floats were parsed successfully, false if empty or otherwise unable to parse.</returns>
		public static bool TryParseVector3(this IDatValue node, out Vector3 value)
		{
			if (string.IsNullOrEmpty(node.Value))
			{
				value = default;
				return false;
			}

			int startIndex;
			int endIndex;

			int openingParenthesisIndex = node.Value.IndexOf('(');
			int closingParenthesisIndex;
			if (openingParenthesisIndex >= 0)
			{
				closingParenthesisIndex = node.Value.IndexOf(')', openingParenthesisIndex + 2);
				if (closingParenthesisIndex < 0)
				{
					value = default;
					return false;
				}

				startIndex = openingParenthesisIndex + 1;
				endIndex = closingParenthesisIndex - 1;
			}
			else
			{
				startIndex = 0;
				endIndex = node.Value.Length - 1;
			}

			int firstDelimiterIndex = node.Value.IndexOf(',', startIndex);
			if (firstDelimiterIndex < 0 || firstDelimiterIndex + 2 > endIndex)
			{
				value = default;
				return false;
			}

			int secondDelimiterIndex = node.Value.IndexOf(',', firstDelimiterIndex + 2);
			if (secondDelimiterIndex < 0 || secondDelimiterIndex + 1 > endIndex)
			{
				value = default;
				return false;
			}

			if (!float.TryParse(node.Value.Substring(startIndex, firstDelimiterIndex - startIndex), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value.x))
			{
				value = default;
				return false;
			}

			if (!float.TryParse(node.Value.Substring(firstDelimiterIndex + 1, secondDelimiterIndex - firstDelimiterIndex - 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value.y))
			{
				value = default;
				return false;
			}

			if (!float.TryParse(node.Value.Substring(secondDelimiterIndex + 1, endIndex - secondDelimiterIndex), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value.z))
			{
				value = default;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parse from 3 comma-delimited floats. (e.g. 1, 2, 3)
		/// </summary>
		public static Vector3 ParseVector3(this IDatValue node, Vector3 defaultValue = default)
		{
			return TryParseVector3(node, out Vector3 value) ? value : defaultValue;
		}

		/// <summary>
		/// Parse from either 3 comma-delimited floats (e.g., 1, 2, 3), or a sub-dictionary.
		/// </summary>
		/// <returns>True if all 3 floats were parsed successfully OR if sub-dictionary exists (regardless of validity).
		/// False if empty, missing, not a value/dictionary, or otherwise unable to parse.</returns>
		public static bool TryParseVector3(this IDatDictionary dictionary, string key, out Vector3 value)
		{
			if (!dictionary.TryGetNode(key, out IDatNode node))
			{
				value = default;
				return false;
			}

			if (node is IDatValue valueNode)
			{
				return TryParseVector3(valueNode, out value);
			}
			else if (node is IDatDictionary dictionaryNode)
			{
				dictionaryNode.TryParseFloat("X", out value.x);
				dictionaryNode.TryParseFloat("Y", out value.y);
				dictionaryNode.TryParseFloat("Z", out value.z);
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}

		/// <summary>
		/// Parse from either 3 comma-delimited floats (e.g., 1, 2), or a sub-dictionary.
		/// </summary>
		/// <returns>Vector if all 3 floats were parsed successfully OR if sub-dictionary exists (regardless of validity).
		/// defaultValue if empty, missing, not a value/dictionary, or otherwise unable to parse.</returns>
		public static Vector3 ParseVector3(this IDatDictionary dictionary, string key, Vector3 defaultValue = default)
		{
			return TryParseVector3(dictionary, key, out Vector3 value) ? value : defaultValue;
		}

		/// <summary>
		/// Some older code reads Vector3 from separate _X, _Y, _Z keys.
		/// Data.readVector3
		/// </summary>
		public static Vector3 LegacyParseVector3(this IDatDictionary dict, string key)
		{
			// Allow using "newer" dictionary and single-line formats as well.
			if (TryParseVector3(dict, key, out Vector3 value))
			{
				return value;
			}

			return new Vector3(dict.ParseFloat(key + "_X"), dict.ParseFloat(key + "_Y"), dict.ParseFloat(key + "_Z"));
		}
	}
}
