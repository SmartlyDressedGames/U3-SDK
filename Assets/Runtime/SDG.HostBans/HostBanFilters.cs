////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unturned.SystemEx;

namespace SDG.HostBans
{
	public struct HostBanIPv4Filter
	{
		public IPv4Filter filter;
		public EHostBanFlags flags;

#if UNITY_EDITOR
		public int banId;
#endif // UNITY_EDITOR

		public override string ToString()
		{
			return filter.ToString();
		}
	}

	public struct HostBanRegexFilter
	{
		public Regex regex;
		public EHostBanFlags flags;

#if UNITY_EDITOR
		public int banId;
#endif // UNITY_EDITOR

		public override string ToString()
		{
			return regex.ToString();
		}
	}

	public struct HostBanSteamIdFilter
	{
		public ulong steamId;
		public EHostBanFlags flags;

#if UNITY_EDITOR
		public int banId;
#endif // UNITY_EDITOR

		public override string ToString()
		{
			return steamId.ToString();
		}
	}

	public class HostBanFilters
	{
		public EHostBanFlags IsAddressMatch(IPv4Address ip, ushort port)
		{
			foreach (HostBanIPv4Filter filter in addresses)
			{
				if (filter.filter.Matches(ip, port))
					return filter.flags;
			}

			return EHostBanFlags.None;
		}

		public EHostBanFlags IsNameMatch(string name)
		{
			return GetRegexResult(nameRegexes, name);
		}

		public EHostBanFlags IsDescriptionMatch(string description)
		{
			return GetRegexResult(descriptionRegexes, description);
		}

		public EHostBanFlags IsThumbnailMatch(string thumbnail)
		{
			return GetRegexResult(thumbnailRegexes, thumbnail);
		}

		public EHostBanFlags IsSteamIdMatch(ulong steamId)
		{
			foreach (HostBanSteamIdFilter filter in steamIds)
			{
				if (filter.steamId == steamId)
					return filter.flags;
			}

			return EHostBanFlags.None;
		}

		public List<HostBanIPv4Filter> addresses;
		public List<HostBanRegexFilter> nameRegexes;
		public List<HostBanRegexFilter> descriptionRegexes;
		public List<HostBanRegexFilter> thumbnailRegexes;
		public List<HostBanSteamIdFilter> steamIds;

		public void ReadConfiguration(NetPakReader reader)
		{
			byte version;
			if (!reader.ReadUInt8(out version) || version > 5)
			{
				// Forwards compatibility.
				addresses = new List<HostBanIPv4Filter>();
				nameRegexes = new List<HostBanRegexFilter>();
				descriptionRegexes = new List<HostBanRegexFilter>();
				thumbnailRegexes = new List<HostBanRegexFilter>();
				steamIds = new List<HostBanSteamIdFilter>();
				return;
			}
			bool hasSubnetMask = version > 4;

			int addressCount;
			if (reader.ReadInt32(out addressCount) && addressCount > 0)
			{
				addresses = new List<HostBanIPv4Filter>(addressCount);
				for (int index = 0; index < addressCount; ++index)
				{
					HostBanIPv4Filter filter = new HostBanIPv4Filter();
					reader.ReadUInt32(out filter.filter.address.value);
					reader.ReadUInt16(out filter.filter.minPort);
					reader.ReadUInt16(out filter.filter.maxPort);
					if (hasSubnetMask)
					{
						reader.ReadUInt32(out filter.filter.subnetMask.value);
					}
					else
					{
						filter.filter.subnetMask = IPv4SubnetMask.SingleAddress;
					}

					uint flags;
					reader.ReadUInt32(out flags);
					filter.flags = (EHostBanFlags) flags;

					addresses.Add(filter);
				}
			}
			else
			{
				addresses = new List<HostBanIPv4Filter>();
			}

			nameRegexes = ReadRegexes(reader);

			if (version > 1)
			{
				descriptionRegexes = ReadRegexes(reader);
				thumbnailRegexes = ReadRegexes(reader);
			}
			else
			{
				descriptionRegexes = new List<HostBanRegexFilter>();
				thumbnailRegexes = new List<HostBanRegexFilter>();
			}

			if (version > 3)
			{
				int steamIdCount;
				if (reader.ReadInt32(out steamIdCount) && steamIdCount > 0)
				{
					steamIds = new List<HostBanSteamIdFilter>(steamIdCount);
					for (int index = 0; index < steamIdCount; ++index)
					{
						HostBanSteamIdFilter filter = new HostBanSteamIdFilter();
						reader.ReadUInt64(out filter.steamId);

						uint flags;
						reader.ReadUInt32(out flags);
						filter.flags = (EHostBanFlags) flags;

						steamIds.Add(filter);
					}
				}
				else
				{
					steamIds = new List<HostBanSteamIdFilter>();
				}
			}
			else
			{
				steamIds = new List<HostBanSteamIdFilter>();
			}
		}

		private EHostBanFlags GetRegexResult(List<HostBanRegexFilter> regexes, string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				foreach (HostBanRegexFilter filter in regexes)
				{
					if (filter.regex.IsMatch(text))
						return filter.flags;
				}
			}

			return EHostBanFlags.None;
		}

		private List<HostBanRegexFilter> ReadRegexes(NetPakReader reader)
		{
			int regexCount;
			if (reader.ReadInt32(out regexCount) && regexCount > 0)
			{
				List<HostBanRegexFilter> regexes = new List<HostBanRegexFilter>(regexCount);
				for (int index = 0; index < regexCount; ++index)
				{
					HostBanRegexFilter filter = new HostBanRegexFilter();

					string pattern;
					reader.ReadString(out pattern);
					filter.regex = new Regex(pattern);

					uint flags;
					reader.ReadUInt32(out flags);
					filter.flags = (EHostBanFlags) flags;

					regexes.Add(filter);
				}
				return regexes;
			}
			else
			{
				return new List<HostBanRegexFilter>();
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public void Dump(System.Text.StringBuilder sb)
		{
			foreach (HostBanIPv4Filter bannedAddress in addresses)
			{
				sb.AppendLine(bannedAddress.ToString());
			}

			foreach (HostBanRegexFilter bannedName in nameRegexes)
			{
				sb.Append("Name: ");
				sb.AppendLine(bannedName.ToString());
			}

			foreach (HostBanRegexFilter bannedDescription in descriptionRegexes)
			{
				sb.Append("Description: ");
				sb.AppendLine(bannedDescription.ToString());
			}

			foreach (HostBanRegexFilter bannedThumbnail in thumbnailRegexes)
			{
				sb.Append("Thumbnail: ");
				sb.AppendLine(bannedThumbnail.ToString());
			}

			foreach (HostBanSteamIdFilter bannedSteamId in steamIds)
			{
				sb.AppendLine(bannedSteamId.ToString());
			}
		}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
	}
}
