////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace Unturned.SystemEx
{
	public struct IPv4SubnetMask : System.IEquatable<IPv4SubnetMask>, System.IComparable<IPv4SubnetMask>
	{
		public uint value;

		public IPv4SubnetMask(int networkBits)
		{
			value = SingleAddress.value << (32 - networkBits);
		}

		public IPv4SubnetMask(uint value)
		{
			this.value = value;
		}

		/// <summary>
		/// Is this a /32 subnet mask?
		/// </summary>
		public bool IsSingleAddress => value == 0xFFFFFFFFu;

		/// <summary>
		/// Network bits is the number of leading ones.
		/// </summary>
		public int CountNetworkBits
		{
			get
			{
				if (value == 0)
				{
					return 0;
				}
				else if (value == 0xFFFFFFFFu)
				{
					return 32;
				}

				return 32 - CountHostBits;
			}
		}

		/// <summary>
		/// Host bits is the number of trailing zeroes.
		/// </summary>
		public int CountHostBits
		{
			get
			{
				if (value == 0)
				{
					return 32;
				}
				else if (value == 0xFFFFFFFFu)
				{
					return 0;
				}

				int result = 0;
				uint temp = value;
				while (temp > 0 && (temp & 1) == 0)
				{
					++result;
					temp >>= 1;
				}

				return result;
			}
		}

		public bool ContainsHost(IPv4Address routingPrefix, IPv4Address hostAddress)
		{
			return (hostAddress.value & value) == routingPrefix.value;
		}

		public IPv4Address MaskRoutingPrefix(IPv4Address address)
		{
			return new IPv4Address(address.value & value);
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public override bool Equals(object rhs)
		{
			return rhs is IPv4SubnetMask && this == (IPv4SubnetMask) rhs;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public static bool operator ==(IPv4SubnetMask lhs, IPv4SubnetMask rhs)
		{
			return lhs.value == rhs.value;
		}

		public static bool operator !=(IPv4SubnetMask lhs, IPv4SubnetMask rhs)
		{
			return lhs.value != rhs.value;
		}

		public bool Equals(IPv4SubnetMask rhs)
		{
			return value == rhs.value;
		}

		public int CompareTo(IPv4SubnetMask rhs)
		{
			return value.CompareTo(rhs.value);
		}

		public static bool TryParse(string input, out IPv4SubnetMask mask)
		{
			return TryParse(input, 0, input?.Length ?? 0, out mask);
		}

		public static bool TryParse(string input, int startIndex, int length, out IPv4SubnetMask mask)
		{
			if (string.IsNullOrEmpty(input) || length < 1)
			{
				mask = SingleAddress;
				return false;
			}

			string substring = input.Substring(startIndex, length);
			if (!int.TryParse(substring, out int routingPrefixBits))
			{
				mask = SingleAddress;
				return false;
			}

			if (routingPrefixBits < 1 || routingPrefixBits > 31)
			{
				mask = SingleAddress;
				return false;
			}

			mask = new IPv4SubnetMask(routingPrefixBits);
			return true;
		}

		public static IPv4SubnetMask SingleAddress = new IPv4SubnetMask(0xFFFFFFFFu);
	}
}
