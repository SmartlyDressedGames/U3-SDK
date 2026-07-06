////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Parameters for connecting to a game server.
	///
	/// Admittedly there are other parameters to the Connect method,
	/// but those are for detecting advertisement discrepencies and can be null.
	/// </summary>
	public class ServerConnectParameters
	{
		/// <summary>
		/// Server's public IP address of a Steam "Fake IP" address.
		/// </summary>
		public IPv4Address address;

		/// <summary>
		/// Port for Steam's "A2S" query system. This the port we refer to when
		/// sharing a server's address (e.g., 127.0.0.1:queryport).
		/// </summary>
		public ushort queryPort;

		/// <summary>
		/// Port for game traffic. i.e., Steam manages the query port socket while
		/// we manage the connection port socket. The game assumes it's the query
		/// port plus one. NOTE HOWEVER after the introduction of "Fake IP" support
		/// (2023-12-07) the connection port is the same as the query port for fake
		/// IPs. In keeping with the spirit of fake values to simplify existing code,
		/// we act as if the connection port is plus one except in the appropriate
		/// ClientTransport code when the fake IP is detected.
		/// </summary>
		public ushort connectionPort;

		/// <summary>
		/// Referred to as "Server Code" in menus.
		/// Used if address is zero.
		/// </summary>
		public CSteamID steamId;

		public string password;

		public ServerConnectParameters(IPv4Address address, ushort queryPort, ushort connectionPort, string password)
		{
			this.address = address;
			this.queryPort = queryPort;
			this.connectionPort = connectionPort;
			this.password = password;
		}

		public ServerConnectParameters(CSteamID steamId, string password)
		{
			this.steamId = steamId;
			this.password = password;
		}
	}
}
