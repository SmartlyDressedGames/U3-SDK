////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define LOG_RATE_LIMITING
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using SDG.NetTransport;

namespace SDG.Unturned
{
	/// <summary>
	/// Counts hits per-IPv4 address (if available) and per-SteamID (if available).
	/// Connection is blocked if more than "threshold" hits occur within category (IPv4/SteamID).
	/// Hit count resets when "window" seconds have passed since last hit.
	/// </summary>
	internal class TransportConnectionRateLimiter
	{
		struct AddressRateLimitingEntry
		{
			public uint address;
			public int counter;
			public double realtime;
		}

		struct SteamIdRateLimitingEntry
		{
			public CSteamID steamId;
			public int counter;
			public double realtime;
		}

		enum ERateLimitingResult
		{
			NOT_IN_LIST,
			HIT_RATE_LIMIT,
			WITHIN_RATE_LIMIT,
		}

		public bool IsBlocked(ITransportConnection transportConnection)
		{
			bool blocked = false;

			if (transportConnection.TryGetSteamId(out ulong transportSteamId))
			{
				blocked |= IsBlockedBySteamIdRateLimiting(new CSteamID(transportSteamId));
			}

			if (!Provider.configData.Server.Use_FakeIP)
			{
				if (transportConnection.TryGetIPv4Address(out uint remoteAddress))
				{
					blocked |= IsBlockedByAddressRateLimiting(remoteAddress);
				}
			}

			return blocked;
		}

		public bool IsBlockedByAddressRateLimiting(uint connectionAddress)
		{
			double time = Time.realtimeSinceStartupAsDouble;

			ERateLimitingResult result = ERateLimitingResult.NOT_IN_LIST;
			for (int index = addressRateLimitingLog.Count - 1; index >= 0; --index)
			{
				AddressRateLimitingEntry entry = addressRateLimitingLog[index];
				if (time - entry.realtime > window)
				{
					// Remove inapplicable entries to avoid list getting big.
					addressRateLimitingLog.RemoveAt(index);
#if LOG_RATE_LIMITING
					UnturnedLog.info($"Remove old connect rate limit for {entry.address} at {entry.realtime} s");
#endif
					continue;
				}

				if (result == ERateLimitingResult.NOT_IN_LIST && entry.address == connectionAddress)
				{
					entry.counter += 1;
					entry.realtime = time;
					addressRateLimitingLog[index] = entry;
					if (entry.counter > threshold)
					{
						// This is the 3rd request in a short window.
						result = ERateLimitingResult.HIT_RATE_LIMIT;
#if LOG_RATE_LIMITING
						UnturnedLog.info($"Hit connect rate limit for {connectionAddress} at {time} s ({entry.counter})");
#endif
					}
					else
					{
						result = ERateLimitingResult.WITHIN_RATE_LIMIT;
#if LOG_RATE_LIMITING
						UnturnedLog.info($"Update connect rate limit for {connectionAddress} at {time} s ({entry.counter})");
#endif
					}
				}
			}

			if (result != ERateLimitingResult.NOT_IN_LIST)
			{
				return result == ERateLimitingResult.HIT_RATE_LIMIT;
			}

			AddressRateLimitingEntry newEntry = new AddressRateLimitingEntry();
			newEntry.address = connectionAddress;
			newEntry.counter = 1;
			newEntry.realtime = time;
			addressRateLimitingLog.Add(newEntry);
#if LOG_RATE_LIMITING
			UnturnedLog.info($"Add connect rate limit for {connectionAddress} at {time} s");
#endif
			return false;
		}

		public bool IsBlockedBySteamIdRateLimiting(CSteamID connectionSteamId)
		{
			double time = Time.realtimeSinceStartupAsDouble;

			ERateLimitingResult result = ERateLimitingResult.NOT_IN_LIST;
			for (int index = steamIdRateLimitingLog.Count - 1; index >= 0; --index)
			{
				SteamIdRateLimitingEntry entry = steamIdRateLimitingLog[index];
				if (time - entry.realtime > window)
				{
					// Remove inapplicable entries to avoid list getting big.
					steamIdRateLimitingLog.RemoveAt(index);
#if LOG_RATE_LIMITING
					UnturnedLog.info($"Remove old ID rate limit for {entry.steamId} at {entry.realtime} s");
#endif
					continue;
				}

				if (result == ERateLimitingResult.NOT_IN_LIST && entry.steamId == connectionSteamId)
				{
					entry.counter += 1;
					entry.realtime = time;
					steamIdRateLimitingLog[index] = entry;
					if (entry.counter > threshold)
					{
						// This is the 3rd request in a short window.
						result = ERateLimitingResult.HIT_RATE_LIMIT;
#if LOG_RATE_LIMITING
						UnturnedLog.info($"Hit connect rate limit for {connectionSteamId} at {time} s ({entry.counter})");
#endif
					}
					else
					{
						result = ERateLimitingResult.WITHIN_RATE_LIMIT;
#if LOG_RATE_LIMITING
						UnturnedLog.info($"Update connect rate limit for {connectionSteamId} at {time} s ({entry.counter})");
#endif
					}
				}
			}

			if (result != ERateLimitingResult.NOT_IN_LIST)
			{
				return result == ERateLimitingResult.HIT_RATE_LIMIT;
			}

			SteamIdRateLimitingEntry newEntry = new SteamIdRateLimitingEntry();
			newEntry.steamId = connectionSteamId;
			newEntry.counter = 1;
			newEntry.realtime = time;
			steamIdRateLimitingLog.Add(newEntry);
#if LOG_RATE_LIMITING
			UnturnedLog.info($"Add ID rate limit for {connectionSteamId} at {time} s");
#endif
			return false;
		}

		/// <summary>
		/// If hit is within this many seconds of previous hit, it counts. Otherwise, counter is reset.
		/// </summary>
		public float window = 40.0f;
		/// <summary>
		/// If more than this many hits occur the limit is reached.
		/// </summary>
		public int threshold = 2;

		private List<AddressRateLimitingEntry> addressRateLimitingLog = new List<AddressRateLimitingEntry>();
		private List<SteamIdRateLimitingEntry> steamIdRateLimitingLog = new List<SteamIdRateLimitingEntry>();
	}
}
