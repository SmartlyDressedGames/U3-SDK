////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public static class VersionUtils
	{
		/// <summary>
		/// Convert 32-bit version into 8-char string.
		/// String is advertised on server list for clients to filter their local map version.
		/// </summary>
		public static string binaryToHexadecimal(uint binaryVersion)
		{
			// X = hexadecimal, 8 = 4 bits per char
			return binaryVersion.ToString("X8");
		}

		/// <summary>
		/// Parse 32-bit version from 8-char string.
		/// String is advertised on server list for clients to filter their local map version.
		/// </summary>
		public static bool hexadecimalToBinary(string hexadecimalVersion, out uint binaryVersion)
		{
			if (string.IsNullOrEmpty(hexadecimalVersion) || hexadecimalVersion.Length != 8)
			{
				binaryVersion = 0;
				return false;
			}

			return uint.TryParse(hexadecimalVersion, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out binaryVersion);
		}
	}
}
