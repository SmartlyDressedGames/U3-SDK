////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class UnityDatColorEx
	{
		/// <summary>
		/// Parse hexadecimal color with optional '#' in front. Alpha is opaque.
		/// </summary>
		public static bool TryParseColor32RGB(this IDatValue node, out Color32 value)
		{
			if (string.IsNullOrEmpty(node.Value))
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}

			int startIndex = node.Value[0] == '#' ? 1 : 0;
			if (node.Value.Length != 6 + startIndex) // We require six characters for RGB color.
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}

			byte r;
			if (!byte.TryParse(node.Value.Substring(startIndex, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out r))
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}

			byte g;
			if (!byte.TryParse(node.Value.Substring(startIndex + 2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out g))
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}

			byte b;
			if (!byte.TryParse(node.Value.Substring(startIndex + 4, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out b))
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}

			value = new Color32(r, g, b, byte.MaxValue);
			return true;
		}

		/// <summary>
		/// Parse hexadecimal color with optional '#' in front. Alpha is opaque.
		/// </summary>
		public static Color32 ParseColor32RGB(this IDatValue node, Color32 defaultValue = default)
		{
			return TryParseColor32RGB(node, out Color32 value) ? value : new Color32(defaultValue.r, defaultValue.g, defaultValue.b, byte.MaxValue);
		}

		/// <summary>
		/// Parse either hexadecimal color with optional '#' in front, or sub-dictionary with R, G, and B keys.
		/// Alpha is opaque.
		/// </summary>
		public static bool TryParseColor32RGB(this IDatDictionary dictionary, string key, out Color32 value)
		{
			if (!dictionary.TryGetNode(key, out IDatNode node))
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}

			if (node is IDatValue valueNode)
			{
				return TryParseColor32RGB(valueNode, out value);
			}
			else if (node is IDatDictionary dictionaryNode)
			{
				dictionaryNode.TryParseUInt8("R", out byte r);
				dictionaryNode.TryParseUInt8("G", out byte g);
				dictionaryNode.TryParseUInt8("B", out byte b);
				value = new Color32(r, g, b, byte.MaxValue);
				return true;
			}
			else
			{
				value = new Color32(0, 0, 0, byte.MaxValue);
				return false;
			}
		}

		/// <summary>
		/// Parse either hexadecimal color with optional '#' in front, or sub-dictionary with R, G, and B keys.
		/// Alpha is opaque.
		/// </summary>
		public static Color32 ParseColor32RGB(this IDatDictionary dictionary, string key, Color32 defaultValue = default)
		{
			return TryParseColor32RGB(dictionary, key, out Color32 value) ? value : new Color32(defaultValue.r, defaultValue.g, defaultValue.b, byte.MaxValue);
		}

		/// <summary>
		/// Parse hexadecimal color with optional '#' in front and optional alpha.
		/// If alpha is not specified, defaults to opaque.
		/// </summary>
		public static bool TryParseColor32RGBA(this IDatValue node, out Color32 value)
		{
			if (string.IsNullOrEmpty(node.Value))
			{
				value = default;
				return false;
			}

			int startIndex = node.Value[0] == '#' ? 1 : 0;
			bool withAlpha;
			if (node.Value.Length == 8 + startIndex)
			{
				withAlpha = true;
			}
			else if (node.Value.Length == 6 + startIndex)
			{
				withAlpha = false;
			}
			else // We require at least six characters for RGB color.
			{
				value = default;
				return false;
			}

			byte r;
			if (!byte.TryParse(node.Value.Substring(startIndex, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out r))
			{
				value = default;
				return false;
			}

			byte g;
			if (!byte.TryParse(node.Value.Substring(startIndex + 2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out g))
			{
				value = default;
				return false;
			}

			byte b;
			if (!byte.TryParse(node.Value.Substring(startIndex + 4, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out b))
			{
				value = default;
				return false;
			}

			byte a;
			if (withAlpha)
			{
				if (!byte.TryParse(node.Value.Substring(startIndex + 6, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out a))
				{
					value = default;
					return false;
				}
			}
			else
			{
				a = byte.MaxValue;
			}

			value = new Color32(r, g, b, a);
			return true;
		}

		/// <summary>
		/// Parse hexadecimal color with optional '#' in front and optional alpha.
		/// If alpha is not specified, defaults to opaque.
		/// </summary>
		public static Color32 ParseColor32RGBA(this IDatValue node, Color32 defaultValue = default)
		{
			return TryParseColor32RGBA(node, out Color32 value) ? value : defaultValue;
		}

		/// <summary>
		/// Parse either hexadecimal color with optional '#' in front and optional alpha, or sub-dictionary with R, G, B, and A keys.
		/// If alpha is not specified, defaults to opaque.
		/// </summary>
		public static bool TryParseColor32RGBA(this IDatDictionary dictionary, string key, out Color32 value)
		{
			if (!dictionary.TryGetNode(key, out IDatNode node))
			{
				value = default;
				return false;
			}

			if (node is IDatValue valueNode)
			{
				return TryParseColor32RGBA(valueNode, out value);
			}
			else if (node is IDatDictionary dictionaryNode)
			{
				dictionaryNode.TryParseUInt8("R", out byte r);
				dictionaryNode.TryParseUInt8("G", out byte g);
				dictionaryNode.TryParseUInt8("B", out byte b);
				dictionaryNode.TryParseUInt8("A", out byte a);
				value = new Color32(r, g, b, a);
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}

		/// <summary>
		/// Parse either hexadecimal color with optional '#' in front and optional alpha, or sub-dictionary with R, G, B, and A keys.
		/// If alpha is not specified, defaults to opaque.
		/// </summary>
		public static Color32 ParseColor32RGBA(this IDatDictionary dictionary, string key, Color32 defaultValue = default)
		{
			return TryParseColor32RGBA(dictionary, key, out Color32 value) ? value : defaultValue;
		}

		/// <summary>
		/// Some older code reads color excluding alpha from separate _R, _G, _B keys.
		/// Data.readColor
		/// </summary>
		public static Color LegacyParseColor(this IDatDictionary dict, string key, Color defaultValue)
		{
			// Allow using "newer" dictionary and hexadecimal formats as well.
			if (TryParseColor32RGB(dict, key, out Color32 value))
			{
				Color convertedColor = (Color) value;
				convertedColor.a = 1.0f;
				return convertedColor;
			}

			return new Color(dict.ParseFloat(key + "_R", defaultValue.r), dict.ParseFloat(key + "_G", defaultValue.g), dict.ParseFloat(key + "_B", defaultValue.b));
		}

		/// <summary>
		/// Some older code reads 8-bit per channel color excluding alpha from separate _R, _G, _B keys.
		/// Data.ReadColor32RGB
		/// </summary>
		public static Color32 LegacyParseColor32RGB(this IDatDictionary dict, string key, Color32 defaultValue)
		{
			// Allow using "newer" dictionary and hexadecimal formats as well.
			if (TryParseColor32RGB(dict, key, out Color32 value))
			{
				return value;
			}

			return new Color32(dict.ParseUInt8(key + "_R", defaultValue.r), dict.ParseUInt8(key + "_G", defaultValue.g), dict.ParseUInt8(key + "_B", defaultValue.b), byte.MaxValue);
		}
	}
}
