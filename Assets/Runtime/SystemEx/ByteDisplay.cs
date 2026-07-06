////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace Unturned.SystemEx
{
	/// <summary>
	/// Nelson 2024-11-29:
	/// Ubuntu's "Units Policy" seems like a good point of reference: https://wiki.ubuntu.com/UnitsPolicy
	/// In summary:
	/// - Use base-10 for network bandwidth, disk sizes, and as the default for file sizes.
	/// - Use base-2 for RAM sizes and perhaps as an option for file sizes.
	/// - IEC standard for base-2 (e.g., 1 KiB = 1,024 bytes, 1 MiB = 1,024 KiB)
	/// - SI standard for base-10 units (e.g., 1 kB = 1,000 bytes, 1 MB = 1,000 kB)
	/// </summary>
	public static class ByteDisplay
	{
		/// <summary>
		/// base-10 units with G3 format string
		/// </summary>
		public static string Base10ToString(long byteCount)
		{
			InternalBase10(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString("G3"));
		}

		public static string Base10ToString(long byteCount, IFormatProvider formatProvider)
		{
			InternalBase10(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString(formatProvider));
		}

		public static string Base10ToString(long byteCount, string format)
		{
			InternalBase10(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString(format));
		}

		public static string Base10ToString(long byteCount, string format, IFormatProvider formatProvider)
		{
			InternalBase10(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString(format, formatProvider));
		}

		/// <summary>
		/// base-2 units with G3 format string
		/// </summary>
		public static string Base2ToString(long byteCount)
		{
			InternalBase2(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString("G3"));
		}

		public static string Base2ToString(long byteCount, IFormatProvider formatProvider)
		{
			InternalBase2(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString(formatProvider));
		}

		public static string Base2ToString(long byteCount, string format)
		{
			InternalBase2(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString(format));
		}

		public static string Base2ToString(long byteCount, string format, IFormatProvider formatProvider)
		{
			InternalBase2(byteCount, out double value, out string unitsFormatString);
			return string.Format(unitsFormatString, value.ToString(format, formatProvider));
		}

		/// <summary>
		/// Nelson 2024-11-29: Where the Base10 and Base2 functions are relatively unopinionated, this one is for
		/// displaying file sizes in the UI. It uses base-2 on Windows and base-10 on other platforms.
		/// </summary>
		public static string FileSizeToString(long byteCount)
		{
#if UNITY_STANDALONE_WIN
			return Base2ToString(byteCount);
#else
			return Base10ToString(byteCount);
#endif
		}

		#region Internal
		private static void InternalBase10(long byteCount, out double value, out string formatString)
		{
			if (byteCount == 0)
			{
				value = 0;
				formatString = BASE_10_FORMAT_STRINGS[0];
				return;
			}

			double power = Math.Log(Math.Abs(byteCount), 1000);
			long roundedDownPower = (long) power;

			formatString = roundedDownPower < BASE_10_FORMAT_STRINGS.Length ?
				BASE_10_FORMAT_STRINGS[roundedDownPower] : BASE_10_FORMAT_STRINGS[0];

			double divisor = Math.Pow(1000, roundedDownPower);
			value = byteCount / divisor;
		}

		private static void InternalBase2(long byteCount, out double value, out string formatString)
		{
			if (byteCount == 0)
			{
				value = 0;
				formatString = BASE_2_FORMAT_STRINGS[0];
				return;
			}

			double power = Math.Log(Math.Abs(byteCount), 1024);
			long roundedDownPower = (long) power;

			formatString = roundedDownPower < BASE_2_FORMAT_STRINGS.Length ?
				BASE_2_FORMAT_STRINGS[roundedDownPower] : BASE_2_FORMAT_STRINGS[0];

			double divisor = Math.Pow(1024, roundedDownPower);
			value = byteCount / divisor;
		}

		private static string[] BASE_10_FORMAT_STRINGS = new string[]
		{
			"{0} B",
			"{0} kB",
			"{0} MB",
			"{0} GB",
			"{0} TB",
		};

		private static string[] BASE_2_FORMAT_STRINGS = new string[]
		{
			"{0} B",
			"{0} KiB",
			"{0} MiB",
			"{0} GiB",
			"{0} TiB",
		};
		#endregion Internal
	}
}
