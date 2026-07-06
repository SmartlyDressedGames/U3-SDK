////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public static class PaletteUtility
	{
		public static string toRGB(Color color)
		{
			Color32 value = color;
			return "#" + value.r.ToString("X2") + value.g.ToString("X2") + value.b.ToString("X2");
		}

		public static string toRGBA(Color color)
		{
			Color32 value = color;
			return "#" + value.r.ToString("X2") + value.g.ToString("X2") + value.b.ToString("X2") + value.a.ToString("X2");
		}

		public static bool tryParse(string value, out Color color)
		{
			color = Color.white;

			if (!string.IsNullOrEmpty(value))
			{
				uint data;
				switch (value.Length)
				{
					case 6:
						if (uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out data))
						{
							color.r = (byte) ((data >> 16) & 0xFF);
							color.g = (byte) ((data >> 8) & 0xFF);
							color.b = (byte) (data & 0xFF);
							color.a = byte.MaxValue;
							return true;
						}

						break;
					case 7:
						if (uint.TryParse(value.Substring(1, value.Length - 1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out data))
						{
							color.r = (byte) ((data >> 16) & 0xFF);
							color.g = (byte) ((data >> 8) & 0xFF);
							color.b = (byte) (data & 0xFF);
							color.a = byte.MaxValue;
							return true;
						}

						break;
					case 8:
						if (uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out data))
						{
							color.r = (byte) ((data >> 24) & 0xFF);
							color.g = (byte) ((data >> 16) & 0xFF);
							color.b = (byte) ((data >> 8) & 0xFF);
							color.a = (byte) (data & 0xFF);
							return true;
						}

						break;
					case 9:
						if (uint.TryParse(value.Substring(1, value.Length - 1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out data))
						{
							color.r = (byte) ((data >> 24) & 0xFF);
							color.g = (byte) ((data >> 16) & 0xFF);
							color.b = (byte) ((data >> 8) & 0xFF);
							color.a = (byte) (data & 0xFF);
							return true;
						}

						break;
				}
			}

			return false;
		}
	}
}
