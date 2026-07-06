////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Customization
	{
		public static readonly byte FREE_CHARACTERS = 1;
		public static readonly byte PRO_CHARACTERS = 4;

		public static readonly byte FACES_FREE = 10;
		public static readonly byte HAIRS_FREE = 5;
		public static readonly byte BEARDS_FREE = 5;

		public static readonly byte FACES_PRO = 22;
		public static readonly byte HAIRS_PRO = 18;
		public static readonly byte BEARDS_PRO = 11;

		public static readonly Color[] SKINS = new Color[]
		{
			new Color(244 / 255f, 230 / 255f, 210 / 255f),
			new Color(217 / 255f, 202 / 255f, 180 / 255f),
			new Color(190 / 255f, 165 / 255f, 130 / 255f),
			new Color(157 / 255f, 136 / 255f, 107 / 255f),
			new Color(148 / 255f, 118 / 255f, 75 / 255f),
			new Color(112 / 255f, 96 / 255f, 73 / 255f),
			new Color(83 / 255f, 71 / 255f, 54 / 255f),
			new Color(75 / 255f, 61 / 255f, 49 / 255f),
			new Color(51 / 255f, 44 / 255f, 37 / 255f),
			new Color(35 / 255f, 31 / 255f, 28 / 255f)
		};

		public static readonly Color[] COLORS = new Color[]
		{
			new Color(215 / 255f, 215 / 255f, 215 / 255f),
			new Color(193 / 255f, 193 / 255f, 193 / 255f),
			new Color(205 / 255f, 192 / 255f, 140 / 255f),
			new Color(172 / 255f, 106 / 255f, 57 / 255f),
			new Color(102 / 255f, 80 / 255f, 55 / 255f),
			new Color(87 / 255f, 69 / 255f, 47 / 255f),
			new Color(71 / 255f, 57 / 255f, 40 / 255f),
			new Color(53 / 255f, 44 / 255f, 34 / 255f),
			new Color(55 / 255f, 55 / 255f, 55 / 255f),
			new Color(25 / 255f, 25 / 255f, 25 / 255f)
		};

		public static readonly Color[] MARKER_COLORS = new Color[]
		{
			Palette.COLOR_B,
			Palette.COLOR_G,
			Palette.COLOR_O,
			Palette.COLOR_P,
			Palette.COLOR_R,
			Palette.COLOR_Y
		};

		public static readonly byte SKILLSETS = 11;

		public static bool checkSkin(Color color)
		{
			for (int index = 0; index < SKINS.Length; index++)
			{
				if (Mathf.Abs(color.r - SKINS[index].r) < 0.01f && Mathf.Abs(color.g - SKINS[index].g) < 0.01f && Mathf.Abs(color.b - SKINS[index].b) < 0.01f)
				{
					return true;
				}
			}

			return false;
		}

		public static bool checkColor(Color color)
		{
			for (int index = 0; index < COLORS.Length; index++)
			{
				if (Mathf.Abs(color.r - COLORS[index].r) < 0.01f && Mathf.Abs(color.g - COLORS[index].g) < 0.01f && Mathf.Abs(color.b - COLORS[index].b) < 0.01f)
				{
					return true;
				}
			}

			return false;
		}
	}
}