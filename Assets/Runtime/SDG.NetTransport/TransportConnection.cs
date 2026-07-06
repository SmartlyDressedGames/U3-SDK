////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetTransport
{
	/// <summary>
	/// Connection between this server and an individual client.
	/// </summary>
	public interface ITransportConnection
		// Implements IEquatable because ServerTransport_SteamNetworking returns new structs for every message received.
		: System.IEquatable<ITransportConnection>
	{
		/// <summary>
		/// Get real IPv4 address of remote player NOT a relay server.
		/// </summary>
		/// <returns>True if address was available.</returns>
		bool TryGetIPv4Address(out uint address);

		/// <summary>
		/// Get real port of remote player NOT a relay server.
		/// </summary>
		/// <returns>True if port was available.</returns>
		bool TryGetPort(out ushort port);

		/// <summary>
		/// Some transport implementations (SteamNetworking and SteamNetworkingSockets) know the client's Steam ID
		/// before going through the authentication process on the server.
		/// </summary>
		/// <param name="steamId">Client's Steam ID, or zero if unavailable.</param>
		/// <returns>True if Steam ID was available.</returns>
		public bool TryGetSteamId(out ulong steamId);

		/// <summary>
		/// Get real address of remote player NOT a relay server.
		/// </summary>
		/// <returns>Null if address was unavailable.</returns>
		System.Net.IPAddress GetAddress();

		/// <summary>
		/// Get string representation of remote end point.
		/// </summary>
		/// <returns>Null if address was unavailable.</returns>
		string GetAddressString(bool withPort);

		/// <summary>
		/// Called when server wants to end connection with this client early, e.g. when banning them.
		/// Game currently depends on implementation flushing reliable messages before fully disposing.
		/// </summary>
		void CloseConnection();

		/// <summary>
		/// Send message to client.
		/// </summary>
		void Send(byte[] buffer, long size, ENetReliability reliability);
	}
}
