////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public abstract class ServerListComparer_Base : IComparer<SteamServerAdvertisement>
	{
		public int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.isDeprioritizedByServerCuration != rhs.isDeprioritizedByServerCuration)
				return rhs.isDeprioritizedByServerCuration ? -1 : 1;

			return CompareDetails(lhs, rhs);
		}

		protected abstract int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs);
	}

	/// <summary>
	/// Sort servers by name A to Z.
	/// </summary>
	public class ServerListComparer_NameAscending : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return lhs.name.CompareTo(rhs.name);
		}
	}

	/// <summary>
	/// Sort servers by name Z to A.
	/// </summary>
	public class ServerListComparer_NameDescending : ServerListComparer_NameAscending
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	/// <summary>
	/// Sort servers by map name A to Z.
	/// </summary>
	public class ServerListComparer_MapAscending : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (string.Equals(lhs.map, rhs.map))
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.map.CompareTo(rhs.map);
			}
		}
	}

	/// <summary>
	/// Sort servers by map name Z to A.
	/// </summary>
	public class ServerListComparer_MapDescending : ServerListComparer_MapAscending
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	/// <summary>
	/// Sort servers by player count high to low.
	/// </summary>
	public class ServerListComparer_PlayersDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.players == rhs.players)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return rhs.players - lhs.players;
			}
		}
	}

	/// <summary>
	/// Sort servers by player count low to high.
	/// </summary>
	public class ServerListComparer_PlayersInverted : ServerListComparer_PlayersDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	/// <summary>
	/// Sort servers by max player count high to low.
	/// </summary>
	public class ServerListComparer_MaxPlayersDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.maxPlayers == rhs.maxPlayers)
			{
				if (lhs.players == rhs.players)
				{
					return lhs.name.CompareTo(rhs.name);
				}
				else
				{
					return rhs.players - lhs.players;
				}
			}
			else
			{
				return rhs.maxPlayers - lhs.maxPlayers;
			}
		}
	}

	/// <summary>
	/// Sort servers by max player count low to high.
	/// </summary>
	public class ServerListComparer_MaxPlayersInverted : ServerListComparer_MaxPlayersDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	/// <summary>
	/// Sort servers by normalized player count high to low.
	/// </summary>
	public class ServerListComparer_FullnessDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			float lhsValue = lhs.NormalizedPlayerCount;
			float rhsValue = rhs.NormalizedPlayerCount;
			if (MathfEx.IsNearlyEqual(lhsValue, rhsValue))
			{
				if (lhs.players == rhs.players)
				{
					return lhs.name.CompareTo(rhs.name);
				}
				else
				{
					return rhs.players - lhs.players;
				}
			}
			else
			{
				return rhsValue.CompareTo(lhsValue);
			}
		}
	}

	/// <summary>
	/// Sort servers by normalized player count low to high.
	/// </summary>
	public class ServerListComparer_FullnessInverted : ServerListComparer_FullnessDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	/// <summary>
	/// Sort servers by ping low to high.
	/// </summary>
	public class ServerListComparer_PingAscending : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.sortingPing == rhs.sortingPing)
			{
				if (lhs.players == rhs.players)
				{
					return lhs.name.CompareTo(rhs.name);
				}
				else
				{
					return rhs.players - lhs.players;
				}
			}
			else
			{
				return lhs.sortingPing - rhs.sortingPing;
			}
		}
	}

	/// <summary>
	/// Sort servers by ping high to low.
	/// </summary>
	public class ServerListComparer_PingDescending : ServerListComparer_PingAscending
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_AnticheatDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
#if WITH_THIRDPARTYAC
			if (lhs.IsThirdpartyAntiCheatEnabled != rhs.IsThirdpartyAntiCheatEnabled)
			{
				return lhs.IsThirdpartyAntiCheatEnabled ? -1 : 1;
			}
#endif // WITH_THIRDPARTYAC

			if (lhs.IsVACSecure == rhs.IsVACSecure)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.IsVACSecure ? -1 : 1;
			}
		}
	}

	public class ServerListComparer_AnticheatInverted : ServerListComparer_AnticheatDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_PerspectiveDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.cameraMode == rhs.cameraMode)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.cameraMode.CompareTo(rhs.cameraMode);
			}
		}
	}

	public class ServerListComparer_PerspectiveInverted : ServerListComparer_PerspectiveDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_CombatDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.isPvP == rhs.isPvP)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.isPvP ? 1 : -1;
			}
		}
	}

	public class ServerListComparer_CombatInverted : ServerListComparer_CombatDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_PasswordDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.isPassworded == rhs.isPassworded)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.isPassworded ? 1 : -1;
			}
		}
	}

	public class ServerListComparer_PasswordInverted : ServerListComparer_PasswordDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_WorkshopDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.isWorkshop == rhs.isWorkshop)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.isWorkshop ? 1 : -1;
			}
		}
	}

	public class ServerListComparer_WorkshopInverted : ServerListComparer_WorkshopDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_GoldDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.isPro == rhs.isPro)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.isPro ? -1 : 1;
			}
		}
	}

	public class ServerListComparer_GoldInverted : ServerListComparer_GoldDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_CheatsDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.hasCheats == rhs.hasCheats)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				return lhs.hasCheats ? 1 : -1;
			}
		}
	}

	public class ServerListComparer_CheatsInverted : ServerListComparer_CheatsDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_MonetizationDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.monetization == rhs.monetization)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				int lhsValue = orderMap[(int) lhs.monetization];
				int rhsValue = orderMap[(int) rhs.monetization];
				return lhsValue - rhsValue;
			}
		}

		public ServerListComparer_MonetizationDefault()
		{
			orderMap = new int[5];
			orderMap[(int) EServerMonetizationTag.None] = 0;
			orderMap[(int) EServerMonetizationTag.NonGameplay] = 1;
			orderMap[(int) EServerMonetizationTag.Unspecified] = 2;
			orderMap[(int) EServerMonetizationTag.Monetized] = 3;
			orderMap[(int) EServerMonetizationTag.Any] = 4;
		}

		private int[] orderMap;
	}

	public class ServerListComparer_MonetizationInverted : ServerListComparer_MonetizationDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_PluginsDefault : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (lhs.pluginFramework == rhs.pluginFramework)
			{
				return lhs.name.CompareTo(rhs.name);
			}
			else
			{
				int lhsValue = orderMap[(int) lhs.pluginFramework];
				int rhsValue = orderMap[(int) rhs.pluginFramework];
				return lhsValue - rhsValue;
			}
		}

		public ServerListComparer_PluginsDefault()
		{
			orderMap = new int[4];
			orderMap[(int) SteamServerAdvertisement.EPluginFramework.None] = 0;
			orderMap[(int) SteamServerAdvertisement.EPluginFramework.Rocket] = 1;
			orderMap[(int) SteamServerAdvertisement.EPluginFramework.OpenMod] = 1;
			orderMap[(int) SteamServerAdvertisement.EPluginFramework.Unknown] = 1;
		}

		private int[] orderMap;
	}

	public class ServerListComparer_PluginsInverted : ServerListComparer_PluginsDefault
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			return -base.CompareDetails(lhs, rhs);
		}
	}

	public class ServerListComparer_UtilityScore : ServerListComparer_Base
	{
		protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
		{
			if (MathfEx.IsNearlyEqual(lhs.utilityScore, rhs.utilityScore, 0.001f))
			{
				// Steam ID hash code is stable within session and essentially outside of server owner's control.
				return lhs.steamID.GetHashCode().CompareTo(rhs.steamID.GetHashCode());
			}
			else
			{
				return -lhs.utilityScore.CompareTo(rhs.utilityScore);
			}
		}
	}

	/// <summary>
	/// Information about a game server retrieved through Steam's "A2S" query system.
	/// Available when joining using the Steam server list API (in-game server browser)
	/// or querying the Server's A2S port directly (connect by IP menu), but not when
	/// joining by Steam ID.
	/// </summary>
	public class SteamServerAdvertisement
	{
		public enum EPluginFramework
		{
			None,
			Rocket,
			OpenMod,
			Unknown,
		}

		public enum EInfoSource
		{
			/// <summary>
			/// Join server by IP.
			/// </summary>
			DirectConnect,

			InternetServerList,
			FavoriteServerList,
			FriendServerList,
			HistoryServerList,
			LanServerList,
		}

		public enum EAnycastProxyMode
		{
			/// <summary>
			/// Server is not using an anycast proxy.
			/// </summary>
			None,

			/// <summary>
			/// Server host indicated an anycast proxy is in use.
			/// </summary>
			TaggedByHost,

			/// <summary>
			/// Moderator flagged server as using an anycast proxy. (EHostBanFlags.QueryPingWarning)
			/// </summary>
			FlaggedByModerator,
		}

		private CSteamID _steamID;
		public CSteamID steamID => _steamID;

		private uint _ip;
		public uint ip => _ip;

		public ushort queryPort;
		public ushort connectionPort;

		private string _name;
		public string name => _name;

		private string _map;
		public string map => _map;

		private bool _isPvP;
		public bool isPvP => _isPvP;

		private bool _hasCheats;
		public bool hasCheats => _hasCheats;

		private bool _isWorkshop;
		public bool isWorkshop => _isWorkshop;

		private EGameMode _mode;
		public EGameMode mode => _mode;

		private ECameraMode _cameraMode;
		public ECameraMode cameraMode => _cameraMode;

		public EServerMonetizationTag monetization
		{
			get;
			private set;
		}

		private int _pingMs;
		/// <summary>
		/// Ping time measured in milliseconds.
		/// </summary>
		public int PingMs => _pingMs;
		[System.Obsolete("Renamed to PingMs to clarify units.")]
		public int ping => _pingMs;

		internal int sortingPing;

		private int _players;
		public int players => _players;

		private int _maxPlayers;
		public int maxPlayers => _maxPlayers;

		private bool _isPassworded;
		public bool isPassworded => _isPassworded;

		public bool IsVACSecure
		{
			get;
			private set;
		}

#if WITH_THIRDPARTYAC
		public bool IsThirdpartyAntiCheatEnabled
		{
			get;
			private set;
		}
#endif // WITH_THIRDPARTYAC

		private bool _isPro;
		public bool isPro => _isPro;

		/// <summary>
		/// ID of network transport implementation to use.
		/// </summary>
		public string networkTransport
		{
			get;
			protected set;
		}

		/// <summary>
		/// Known plugin systems.
		/// </summary>
		public EPluginFramework pluginFramework
		{
			get;
			protected set;
		}

		public string thumbnailURL
		{
			get;
			protected set;
		}

		public string descText
		{
			get;
			protected set;
		}

		/// <summary>
		/// Probably just checks whether IP is link-local, but may as well use Steam's utility function.
		/// </summary>
		public bool IsAddressUsingSteamFakeIP()
		{
			return SteamNetworkingUtils.IsFakeIPv4(ip);
		}

		/// <summary>
		/// Active player count divided by max player count.
		/// </summary>
		internal float NormalizedPlayerCount
		{
			get => _maxPlayers > 0 ? Mathf.Clamp01(_players / (float) _maxPlayers) : 0.0f;
		}

		internal float utilityScore;
		internal EInfoSource infoSource;
		internal EAnycastProxyMode anycastProxyMode;

		private static AnimationCurve pingCurve = new AnimationCurve(
			new Keyframe(50.0f, 1.0f), // Anything lower than 50 ms is a perfect score.
			new Keyframe(100.0f, 0.6f), // Lower than 100 ms is pretty good.
			new Keyframe(300.0f, 0.3f), // Lower than 300 ms is decent.
			new Keyframe(900.0f, 0.1f) // Above 900 ms is very bad.
			);
		/// <summary>
		/// Nelson 2024-08-20: This score is intended to prioritize low ping without making it the be-all end-all. The
		/// old default of sorting by ping could put near-empty servers at the top of the list, and encouraged using
		/// anycast caching to make the server appear as low-ping as possible.
		/// </summary>
		private float PingUtilityScore
		{
			get
			{
				return pingCurve.Evaluate(sortingPing);
			}
		}

		private static AnimationCurve fullnessCurve = new AnimationCurve(
			new Keyframe(0.0f, 0.1f), // Empty servers are very low score.
			new Keyframe(0.5f, 0.8f),
			new Keyframe(0.75f, 1.0f),
			new Keyframe(1.0f, 0.8f) // Full servers are probably interesting/good, but should not be at the very top.
			);
		/// <summary>
		/// Nelson 2024-08-20: This score is intended to prioritize servers around 75% capacity. My thought process is
		/// that near-empty and near-full servers are already easy to find, but typically if you want to play online you
		/// want a server with space for you and your friends. Unfortunately, servers with plenty of players but an even
		/// higher max players make a 50% score plenty good.
		/// </summary>
		private float FullnessUtilityScore
		{
			get
			{
				// Nelson 2024-08-20: We clamp max players to 100 because there are plenty of good servers with inflated
				// max player counts (e.g., 200) when the highest actual player count is less than 100. Without this
				// clamping those servers are considered less than 50% full.
				int adjustedMaxPlayers = Mathf.Clamp(_maxPlayers, 1, 100);
				float adjustedPlayerCount = Mathf.Clamp01(_players / (float) adjustedMaxPlayers);
				return fullnessCurve.Evaluate(adjustedPlayerCount);
			}
		}

		private static AnimationCurve playerCountCurve = new AnimationCurve(
			new Keyframe(2, 0.1f), // 2 players and below is the lowest score.
			new Keyframe(18.0f, 0.8f), // 20 players and above is a healthy population.
			new Keyframe(64.0f, 1.0f)
			);
		/// <summary>
		/// Nelson 2024-08-20: This score is intended to balance out the downside of the fullness score decreasing for
		/// servers with very high max player counts, and over-scoring servers with low max players.
		/// </summary>
		private float PlayerCountUtilityScore
		{
			get
			{
				return playerCountCurve.Evaluate(_players);
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal string GetUtilityScoreDebugText()
		{
			return $"Total Score: {utilityScore}\nPing Score: {PingUtilityScore}\nFullness Score: {FullnessUtilityScore}\nPlayer Count Score: {PlayerCountUtilityScore}";
		}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		/// <summary>
		/// Called before inserting to server list.
		/// </summary>
		internal void CalculateUtilityScore()
		{
			utilityScore = PingUtilityScore * FullnessUtilityScore * PlayerCountUtilityScore;
		}

		/// <summary>
		/// Parses value between two keys <stuff>thing</stuff> would parse thing
		/// </summary>
		protected string parseTagValue(string tags, string startKey, string endKey)
		{
			int startIndex = tags.IndexOf(startKey);
			if (startIndex == -1)
				return null;
			startIndex += startKey.Length;

			int endIndex = tags.IndexOf(endKey, startIndex);
			if (endIndex == -1)
				return null;

			if (endIndex == startIndex) // 0 length value
				return null;

			return tags.Substring(startIndex, endIndex - startIndex);
		}

		protected bool hasTagKey(string tags, string key, int thumbnailIndex)
		{
			int keyIndex = tags.IndexOf(key);
			if (keyIndex == -1)
				return false;

			if (thumbnailIndex == -1)
			{
				return true;
			}
			else
			{
				// Key was after thumbnail portion, in which case it was a false positive part of URL.
				return keyIndex < thumbnailIndex;
			}
		}

#if !DEDICATED_SERVER
		internal void SetServerListHostBanFlags(HostBans.EHostBanFlags hostBanFlags)
		{
			this.hostBanFlags = hostBanFlags;
			if (hostBanFlags.HasFlag(HostBans.EHostBanFlags.QueryPingWarning) && anycastProxyMode == EAnycastProxyMode.None)
			{
				// Prevent server from showing up among those with lowest ping.
				sortingPing += LiveConfig.Get().queryPingWarningOffsetMs;
				anycastProxyMode = EAnycastProxyMode.FlaggedByModerator;
			}
		}

		internal HostBans.EHostBanFlags hostBanFlags;
#endif // !DEDICATED_SERVER

		internal string serverCurationLabels;
		internal bool isDeniedByServerCurationRule;
		internal bool isDeprioritizedByServerCuration;
		/// <summary>
		/// If set, this server was denied by a server curation list.
		/// </summary>
		internal ServerListCurationRule deniedByRule;

		public SteamServerAdvertisement(gameserveritem_t data, EInfoSource infoSource)
		{
			_steamID = data.m_steamID;
			_ip = data.m_NetAdr.GetIP();
			queryPort = data.m_NetAdr.GetQueryPort();
			// GetConnectionPort is not trustworthy because we pass queryPort as gamePort to GameServer.Init as a
			// workaround because the legacy Steam server browser provides connection port rather than query port.
			connectionPort = (ushort) (queryPort + 1);

			_name = data.GetServerName();

			ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref _name);

			_map = data.GetMap();

			string tags = data.GetGameTags();

			if (tags.Length > 0)
			{
				int thumbnailIndex = tags.IndexOf("<tn>");

				_isPvP = hasTagKey(tags, "PVP", thumbnailIndex);
				_hasCheats = hasTagKey(tags, "CHy", thumbnailIndex);
				_isWorkshop = hasTagKey(tags, "WSy", thumbnailIndex);

				if (hasTagKey(tags, Provider.getModeTagAbbreviation(EGameMode.EASY), thumbnailIndex))
				{
					_mode = EGameMode.EASY;
				}
				else if (hasTagKey(tags, Provider.getModeTagAbbreviation(EGameMode.HARD), thumbnailIndex))
				{
					_mode = EGameMode.HARD;
				}
				else
				{
					_mode = EGameMode.NORMAL;
				}

				if (hasTagKey(tags, Provider.getCameraModeTagAbbreviation(ECameraMode.FIRST), thumbnailIndex))
				{
					_cameraMode = ECameraMode.FIRST;
				}
				else if (hasTagKey(tags, Provider.getCameraModeTagAbbreviation(ECameraMode.THIRD), thumbnailIndex))
				{
					_cameraMode = ECameraMode.THIRD;
				}
				else if (hasTagKey(tags, Provider.getCameraModeTagAbbreviation(ECameraMode.BOTH), thumbnailIndex))
				{
					_cameraMode = ECameraMode.BOTH;
				}
				else
				{
					_cameraMode = ECameraMode.VEHICLE;
				}

				if (hasTagKey(tags, Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.None), thumbnailIndex))
				{
					monetization = EServerMonetizationTag.None;
				}
				else if (hasTagKey(tags, Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.NonGameplay), thumbnailIndex))
				{
					monetization = EServerMonetizationTag.NonGameplay;
				}
				else if (hasTagKey(tags, Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.Monetized), thumbnailIndex))
				{
					monetization = EServerMonetizationTag.Monetized;
				}
				else
				{
					monetization = EServerMonetizationTag.Unspecified;
				}

				_isPro = hasTagKey(tags, "GLD", thumbnailIndex);
#if WITH_THIRDPARTYAC
				IsThirdpartyAntiCheatEnabled = hasTagKey(tags, "BEy", thumbnailIndex);
#endif
				bool isUsingAnycastProxy = hasTagKey(tags, "ACP", thumbnailIndex);
				if (isUsingAnycastProxy)
				{
					anycastProxyMode = EAnycastProxyMode.TaggedByHost;
				}

				networkTransport = parseTagValue(tags, "<net>", "</net>");
				if (string.IsNullOrEmpty(networkTransport))
				{
					UnturnedLog.warn("Unable to parse net transport tag for server \"{0}\" from \"{1}\"", name, tags);
				}

				string pluginFrameworkTag = parseTagValue(tags, "<pf>", "</pf>");
				if (string.IsNullOrEmpty(pluginFrameworkTag))
				{
					if (data.m_nBotPlayers == 1) // Legacy RocketMod sets bot player count to one.
					{
						pluginFramework = EPluginFramework.Rocket;
					}
					else
					{
						pluginFramework = EPluginFramework.None;
					}
				}
				else
				{
					if (pluginFrameworkTag.Equals("rm"))
					{
						pluginFramework = EPluginFramework.Rocket;
					}
					else if (pluginFrameworkTag.Equals("om"))
					{
						pluginFramework = EPluginFramework.OpenMod;
					}
					else
					{
						pluginFramework = EPluginFramework.Unknown;
					}
				}

				thumbnailURL = parseTagValue(tags, "<tn>", "</tn>");

				//string _desc = parseTagValue(tags, "<dc>", "</dc>"); // Fallback in case GS desc is deprecated
				string _desc = data.GetGameDescription();
				if (!RichTextUtil.IsTextValidForServerListShortDescription(_desc))
				{
					// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2660
					// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/3997
					_desc = null;
				}
				else
				{
					ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref _desc);
				}

				if (_desc.ContainsNewLine() || _desc.ContainsChar('\t'))
				{
					// Prevent control characters in server description because some hosts found they could be used to
					// overlay text onto another server listing.
					_desc = null;
					UnturnedLog.warn($"Control characters not allowed in server \"{name}\" description");
				}

				descText = _desc;
			}
			else
			{
				_isPvP = true;
				_hasCheats = false;
				_mode = EGameMode.NORMAL;
				_cameraMode = ECameraMode.FIRST;
				monetization = EServerMonetizationTag.Unspecified;
				_isPro = true;
#if WITH_THIRDPARTYAC
				IsThirdpartyAntiCheatEnabled = false;
#endif
				networkTransport = null;
				pluginFramework = EPluginFramework.None;
				thumbnailURL = null;
				descText = null;
			}

			_pingMs = data.m_nPing;
			sortingPing = _pingMs;
#if !DEDICATED_SERVER
			if (anycastProxyMode == EAnycastProxyMode.TaggedByHost)
			{
				sortingPing += LiveConfig.Get().queryPingWarningOffsetMs;
			}
#endif // !DEDICATED_SERVER
			_maxPlayers = data.m_nMaxPlayers;

			// m_nPlayers includes bot players, so plugin devs try to fake the human player count.
			if (data.m_nPlayers < 0 || data.m_nBotPlayers < 0 || data.m_nPlayers > byte.MaxValue || data.m_nBotPlayers > byte.MaxValue)
			{
				// Stop them from trying to overflow player count.
				_players = 0;
			}
			else
			{
				_players = Mathf.Max(0, data.m_nPlayers - data.m_nBotPlayers);
			}

			_isPassworded = data.m_bPassword;
			IsVACSecure = data.m_bSecure;
			this.infoSource = infoSource;
		}

		public SteamServerAdvertisement(CSteamID steamId)
		{
			_steamID = steamId;
		}

		public override string ToString()
		{
			return "Name: " + name + " Map: " + map + " PvP: " + isPvP + " Mode: " + mode + " Ping: " + PingMs + " Players: " + players + "/" + maxPlayers + " Passworded: " + isPassworded;
		}
	}
}
