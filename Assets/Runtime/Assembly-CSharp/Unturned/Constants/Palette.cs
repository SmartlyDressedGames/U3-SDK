////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Palette
	{
		public static readonly Color SERVER = Color.green;
		public static readonly Color ADMIN = Color.cyan;
		public static readonly Color PRO = new Color(210 / 255f, 191 / 255f, 34 / 255f);

		public static readonly Color COLOR_W = new Color(180 / 255f, 180 / 255f, 180 / 255f); // #b4b4b4
		public static readonly Color COLOR_R = new Color(191 / 255f, 31 / 255f, 31 / 255f); // #bf1f1f
		public static readonly Color COLOR_G = new Color(31 / 255f, 135 / 255f, 31 / 255f); // #1f871f
		public static readonly Color COLOR_B = new Color(50 / 255f, 152 / 255f, 200 / 255f); // #3298c8
		public static readonly Color COLOR_O = new Color(171 / 255f, 128 / 255f, 25 / 255f); // #ab8019
		public static readonly Color COLOR_Y = new Color(220 / 255f, 180 / 255f, 19 / 255f); // #dcb413
		public static readonly Color COLOR_P = new Color(106 / 255f, 70 / 255f, 109 / 255f); // #6a466d

		public static readonly Color AMBIENT = new Color(0.7f, 0.7f, 0.7f);
		public static readonly Color MYTHICAL = new Color(250 / 255f, 50 / 255f, 25 / 255f);

		public static string hex(Color32 color)
		{
			return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		}

		public static Color hex(string color)
		{
			uint data;
			if (!string.IsNullOrEmpty(color) && color.Length == 7 && uint.TryParse(color.Substring(1, color.Length - 1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out data))
			{
				uint r = (data >> 16) & 0xFF;
				uint g = (data >> 8) & 0xFF;
				uint b = data & 0xFF;

				return new Color32((byte) r, (byte) g, (byte) b, byte.MaxValue);
			}
			else
			{
				return Color.white;
			}
		}
	}
}