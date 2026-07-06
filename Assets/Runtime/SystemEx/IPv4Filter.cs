////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace Unturned.SystemEx
{
	public struct IPv4Filter
	{
		/// <summary>
		/// Host address or routing prefix.
		/// </summary>
		public IPv4Address address;
		/// <summary>
		/// Applicable if address is a routing prefix.
		/// </summary>
		public IPv4SubnetMask subnetMask;
		/// <summary>
		/// Filter matches if host port is >= min. This allows an exact port to be specified
		/// if min and max are equal, a range of ports, or ignored by setting to zero.
		/// </summary>
		public ushort minPort;
		/// <summary>
		/// Filter matches if host port is <= max. This allows an exact port to be specified
		/// if min and max are equal, a range of ports, or ignored by setting to ushort.MaxValue.
		/// </summary>
		public ushort maxPort;

		public IPv4Filter(IPv4Address address, IPv4SubnetMask subnetMask, ushort minPort, ushort maxPort)
		{
			this.address = address;
			this.subnetMask = subnetMask;
			this.minPort = minPort;
			this.maxPort = maxPort;
		}

		/// <summary>
		/// Applicable when using a subnet mask. An address will match if >= min address and <= max address.
		/// </summary>
		public void GetAddressRange(out IPv4Address minAddress, out IPv4Address maxAddress)
		{
			minAddress = address;
			uint hostMask = IPv4SubnetMask.SingleAddress.value ^ subnetMask.value;
			maxAddress = new IPv4Address(address.value + hostMask);
		}

		public bool Matches(IPv4Address hostAddress, ushort port)
		{
			return subnetMask.ContainsHost(address, hostAddress) && port >= minPort && port <= maxPort;
		}

		public override string ToString()
		{
			if (minPort == ushort.MinValue && maxPort == ushort.MaxValue)
			{
				// No port filter.

				if (subnetMask.IsSingleAddress)
				{
					return address.ToString();
				}
				else
				{
					return $"{address}/{subnetMask.CountNetworkBits}";
				}
			}
			else if (minPort == maxPort)
			{
				// Specific port filter.

				if (subnetMask.IsSingleAddress)
				{
					return $"{address}:{minPort}";
				}
				else
				{
					return $"{address}/{subnetMask.CountNetworkBits}:{minPort}";
				}
			}
			else
			{
				// Port range filter.

				if (subnetMask.IsSingleAddress)
				{
					return $"{address}:{minPort}-{maxPort}";
				}
				else
				{
					return $"{address}/{subnetMask.CountNetworkBits}:{minPort}-{maxPort}";
				}
			}
		}

		public override bool Equals(object rhs)
		{
			return rhs is IPv4Filter && this == (IPv4Filter) rhs;
		}

		public override int GetHashCode()
		{
			return address.GetHashCode() ^ subnetMask.GetHashCode() & minPort.GetHashCode() ^ maxPort.GetHashCode();
		}

		public static bool operator ==(IPv4Filter lhs, IPv4Filter rhs)
		{
			return lhs.address == rhs.address && lhs.subnetMask == rhs.subnetMask && lhs.minPort == rhs.minPort && lhs.maxPort == rhs.maxPort;
		}

		public static bool operator !=(IPv4Filter lhs, IPv4Filter rhs)
		{
			return !(lhs == rhs);
		}

		public bool Equals(IPv4Filter rhs)
		{
			return this == rhs;
		}

		public int CompareTo(IPv4Filter rhs)
		{
			if (address != rhs.address)
			{
				return address.CompareTo(rhs.address);
			}

			if (subnetMask != rhs.subnetMask)
			{
				return subnetMask.CompareTo(rhs.subnetMask);
			}

			if (minPort != rhs.minPort)
			{
				return minPort.CompareTo(rhs.minPort);
			}

			return maxPort.CompareTo(rhs.maxPort);
		}

		public static bool TryParse(string input, out IPv4Filter filter)
		{
			if (string.IsNullOrEmpty(input))
			{
				filter = default;
				return false;
			}

			int portDelimiterIndex = input.LastIndexOf(':');
			if (portDelimiterIndex < 0)
			{
				int subnetDelimiterIndex = input.LastIndexOf('/');
				if (subnetDelimiterIndex < 0)
				{
					// No port, No subnet mask.
					filter.subnetMask = IPv4SubnetMask.SingleAddress;
					filter.minPort = 0;
					filter.maxPort = ushort.MaxValue;
					return IPv4Address.TryParse(input, out filter.address);
				}
				else
				{
					// No port, Yes subnet mask.
					string subnetMaskString = input.Substring(subnetDelimiterIndex + 1);
					bool parsedSubnetMask = IPv4SubnetMask.TryParse(subnetMaskString, out filter.subnetMask);
					filter.minPort = 0;
					filter.maxPort = ushort.MaxValue;
					bool parsedAddress = IPv4Address.TryParse(input, 0, subnetDelimiterIndex, out filter.address);
					filter.address = filter.subnetMask.MaskRoutingPrefix(filter.address);
					return parsedAddress && parsedSubnetMask;
				}
			}
			else
			{
				int subnetDelimiterIndex = input.LastIndexOf('/');
				if (subnetDelimiterIndex < 0)
				{
					// Yes port, No subnet mask.
					filter.subnetMask = IPv4SubnetMask.SingleAddress;
					string portString = input.Substring(portDelimiterIndex + 1);
					bool parsedPorts = IPv4Address.TryParsePortRange(portString, out filter.minPort, out filter.maxPort);
					return IPv4Address.TryParse(input, 0, portDelimiterIndex, out filter.address) && parsedPorts;
				}
				else
				{
					// Yes port, Yes subnet mask.
					string subnetMaskString = input.Substring(subnetDelimiterIndex + 1, portDelimiterIndex - subnetDelimiterIndex - 1);
					string portString = input.Substring(portDelimiterIndex + 1);
					bool parsedSubnetMask = IPv4SubnetMask.TryParse(subnetMaskString, out filter.subnetMask);
					bool parsedPorts = IPv4Address.TryParsePortRange(portString, out filter.minPort, out filter.maxPort);
					bool parsedAddress = IPv4Address.TryParse(input, 0, subnetDelimiterIndex, out filter.address);
					filter.address = filter.subnetMask.MaskRoutingPrefix(filter.address);
					return parsedAddress && parsedSubnetMask && parsedPorts;
				}
			}
		}
	}
}
