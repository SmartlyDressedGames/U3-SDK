////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace Unturned.SystemEx
{
	/// <summary>
	/// Unfortunately a lot of Unturned and Steam code uses IPv4 addresses and
	/// the System.Net.IPAddress class does not handle them in the way we want.
	/// </summary>
	public struct IPv4Address : System.IEquatable<IPv4Address>, System.IComparable<IPv4Address>
	{
		public static readonly IPv4Address Zero = new IPv4Address(0);

		public uint value;

		public IPv4Address(uint value)
		{
			this.value = value;
		}

		public IPv4Address(string input)
		{
			TryParse(input, out value);
		}

		/// <summary>
		/// 127.0.0.0/8
		/// </summary>
		public bool IsLoopback => (value >> 24) == 127u;

		/// <summary>
		/// Local/private
		/// 10.0.0.0/8
		/// 172.16.0.0/12
		/// 192.168.0.0/16
		/// </summary>
		public bool IsLocalPrivate
		{
			get
			{
				const uint prefix0 = ((172u << 24) | (16u << 16)) >> 20;
				const uint prefix1 = (192u << 8) | 168u;
				return (value >> 24) == 10u || (value >> 20) == prefix0 || (value >> 16) == prefix1;
			}
		}

		/// <summary>
		/// Link-local
		/// 169.254.0.0/16
		/// </summary>
		public bool IsLinkLocal
		{
			get
			{
				const uint prefix = (169u << 8) | 254u;
				return (value >> 16) == prefix;
			}
		}

		/// <summary>
		/// NOT loopback, local/private, nor link-local
		/// </summary>
		public bool IsWideAreaNetwork => !(IsLoopback || IsLocalPrivate || IsLinkLocal);

		public bool IsZero
		{
			get => value == 0;
		}

		public override string ToString()
		{
			return $"{value >> 24}.{(value >> 16) & 0xFF}.{(value >> 8) & 0xFF}.{value & 0xFF}";
		}

		public override bool Equals(object rhs)
		{
			return rhs is IPv4Address && this == (IPv4Address) rhs;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public static bool operator ==(IPv4Address lhs, IPv4Address rhs)
		{
			return lhs.value == rhs.value;
		}

		public static bool operator !=(IPv4Address lhs, IPv4Address rhs)
		{
			return lhs.value != rhs.value;
		}

		public bool Equals(IPv4Address rhs)
		{
			return value == rhs.value;
		}

		public int CompareTo(IPv4Address rhs)
		{
			return value.CompareTo(rhs.value);
		}

		public static bool TryParse(string input, out uint address)
		{
			return TryParse(input, 0, input?.Length ?? 0, out address);
		}

		public static bool TryParse(string input, int startIndex, int length, out uint address)
		{
			address = 0;

			if (string.IsNullOrEmpty(input) || length < 7)
			{
				// a.b.c.d is 7 chars
				return false;
			}

			int finalCharIndex = startIndex + length;

			// Skip any leading white space, including zero-width spaces. (public issue #4413)
			while (startIndex < finalCharIndex)
			{
				char letter = input[startIndex];
				if (char.IsWhiteSpace(letter)
					|| letter == '\u200b' // zero-width space (https://en.wikipedia.org/wiki/Zero-width_space)
					|| letter == '\u200c' // zero-width non-joiner (https://en.wikipedia.org/wiki/Zero-width_non-joiner)
					|| letter == '\u200d' // zero-width joiner (https://en.wikipedia.org/wiki/Zero-width_joiner)
					|| letter == '\u2060' // word joiner (https://en.wikipedia.org/wiki/Word_joiner)
					|| letter == '\ufeff') // zero-width no-break space
				{
					++startIndex;
				}
				else
				{
					break;
				}
			}

			// Skip any trailing white space, including zero-width spaces. (public issue #4413)
			while (finalCharIndex - 1 > startIndex)
			{
				char letter = input[finalCharIndex - 1];
				if (char.IsWhiteSpace(letter)
					|| letter == '\u200b' // zero-width space (https://en.wikipedia.org/wiki/Zero-width_space)
					|| letter == '\u200c' // zero-width non-joiner (https://en.wikipedia.org/wiki/Zero-width_non-joiner)
					|| letter == '\u200d' // zero-width joiner (https://en.wikipedia.org/wiki/Zero-width_joiner)
					|| letter == '\u2060' // word joiner (https://en.wikipedia.org/wiki/Word_joiner)
					|| letter == '\ufeff') // zero-width no-break space
				{
					--finalCharIndex;
				}
				else
				{
					break;
				}
			}

			// If we moved indices the length may be too short. (e.g., startIndex might equal finalCharIndex now)
			int adjustedLength = finalCharIndex - startIndex + 1;
			if (adjustedLength < 7)
			{
				// a.b.c.d is 7 chars
				return false;
			}

			int delimiter0 = input.IndexOf('.', startIndex);
			if (delimiter0 < startIndex + 1 || delimiter0 + 6 > finalCharIndex) // .b.c.d is 6 chars
			{
				// Either not found (-1),
				// or is the first char (0) in which case first digit is missing,
				// or there is not enough room for remaining delimiters and digits.
				return false;
			}

			int delimiter1 = input.IndexOf('.', delimiter0 + 2); // +2 to skip next digit
			if (delimiter1 < 0 || delimiter1 + 4 > finalCharIndex) // .c.d is 4 chars
			{
				// Either not found (-1),
				// or there is not enough room for remaining delimiters and digits.
				return false;
			}

			int delimiter2 = input.IndexOf('.', delimiter1 + 2); // +2 to skip next digit
			if (delimiter2 < 0 || delimiter2 + 2 > finalCharIndex) // .d is 2 chars
			{
				// Either not found (-1),
				// or there is not enough room for final digit.
				return false;
			}

			string string0 = input.Substring(startIndex, delimiter0 - startIndex);
			string string1 = input.Substring(delimiter0 + 1, delimiter1 - delimiter0 - 1);
			string string2 = input.Substring(delimiter1 + 1, delimiter2 - delimiter1 - 1);
			string string3 = input.Substring(delimiter2 + 1, finalCharIndex - delimiter2 - 1);

			uint number0;
			uint number1;
			uint number2;
			uint number3;
			if (!uint.TryParse(string0, out number0) || !uint.TryParse(string1, out number1) || !uint.TryParse(string2, out number2) || !uint.TryParse(string3, out number3))
				return false;

			const uint max = byte.MaxValue;
			if (number0 > max || number1 > max || number2 > max || number3 > max)
				return false;

			address = (number0 << 24) | (number1 << 16) | (number2 << 8) | number3;
			return true;
		}

		public static bool TryParsePortRange(string input, out ushort minPort, out ushort maxPort)
		{
			return TryParsePortRange(input, 0, input?.Length ?? 0, out minPort, out maxPort);
		}

		public static bool TryParsePortRange(string input, int startIndex, int length, out ushort minPort, out ushort maxPort)
		{
			minPort = ushort.MinValue;
			maxPort = ushort.MaxValue;

			if (string.IsNullOrEmpty(input))
				return false;

			int hyphenIndex = input.IndexOf('-', startIndex, length);
			if (hyphenIndex < 0)
			{
				// Hyphen not found, so try parsing the same number for min and max.
				string portString = input.Substring(startIndex, length);
				ushort port;
				if (ushort.TryParse(portString, out port))
				{
					minPort = port;
					maxPort = port;
					return true;
				}
				else
				{
					return false;
				}
			}

			int finalCharIndex = startIndex + length;
			if (hyphenIndex < startIndex + 1 || hyphenIndex + 1 > finalCharIndex)
			{
				// Either is the first char (0) in which case first digit is missing,
				// or there is not enough room for remaining digits.
				return false;
			}

			string minString = input.Substring(startIndex, hyphenIndex - startIndex);
			string maxString = input.Substring(hyphenIndex + 1, finalCharIndex - hyphenIndex - 1);

			if (!ushort.TryParse(minString, out minPort) || !ushort.TryParse(maxString, out maxPort))
				return false;

			if (minPort > maxPort)
			{
				ushort temp = maxPort;
				maxPort = minPort;
				minPort = temp;
			}
			return true;
		}

		public static bool TryParse(string input, out IPv4Address address)
		{
			return TryParse(input, out address.value);
		}

		public static bool TryParse(string input, int startIndex, int length, out IPv4Address address)
		{
			return TryParse(input, startIndex, length, out address.value);
		}

		public static bool TryParseWithOptionalPort(string input, out uint address, out ushort? optionalPort)
		{
			if (string.IsNullOrEmpty(input))
			{
				address = 0;
				optionalPort = null;
				return false;
			}

			int portDelimiterIndex = input.LastIndexOf(':');
			if (portDelimiterIndex < 0)
			{
				// No port
				optionalPort = null;
				return TryParse(input, out address);
			}
			else
			{
				ushort tempPort;
				if (ushort.TryParse(input.Substring(portDelimiterIndex + 1), out tempPort))
				{
					optionalPort = tempPort;
					return TryParse(input, 0, portDelimiterIndex, out address);
				}
				else
				{
					address = 0;
					optionalPort = null;
					return false;
				}
			}
		}

		public static bool TryParseWithOptionalPort(string input, out IPv4Address address, out ushort? optionalPort)
		{
			return TryParseWithOptionalPort(input, out address.value, out optionalPort);
		}

		public static bool TryParseWithOptionalPortRange(string input, out uint address, out ushort? optionalMinPort, out ushort? optionalMaxPort)
		{
			if (string.IsNullOrEmpty(input))
			{
				address = 0;
				optionalMinPort = null;
				optionalMaxPort = null;
				return false;
			}

			int portDelimiterIndex = input.LastIndexOf(':');
			if (portDelimiterIndex < 0)
			{
				// No port range
				optionalMinPort = null;
				optionalMaxPort = null;
				return TryParse(input, out address);
			}
			else
			{
				ushort tempMinPort;
				ushort tempMaxPort;
				string portRangeString = input.Substring(portDelimiterIndex + 1);
				if (TryParsePortRange(portRangeString, out tempMinPort, out tempMaxPort))
				{
					optionalMinPort = tempMinPort;
					optionalMaxPort = tempMaxPort;
					return TryParse(input, 0, portDelimiterIndex, out address);
				}
				else
				{
					address = 0;
					optionalMinPort = null;
					optionalMaxPort = null;
					return false;
				}
			}
		}

		public static bool TryParseWithOptionalPortRange(string input, out IPv4Address address, out ushort? optionalMinPort, out ushort? optionalMaxPort)
		{
			return TryParseWithOptionalPortRange(input, out address.value, out optionalMinPort, out optionalMaxPort);
		}
	}
}
