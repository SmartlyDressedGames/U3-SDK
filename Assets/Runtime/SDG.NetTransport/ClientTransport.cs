////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Unturned.SystemEx;

namespace SDG.NetTransport
{
	public delegate void ClientTransportReady();
	public delegate void ClientTransportFailure(string message);

	/// <summary>
	/// Abstracts client communication to a dedicated server.
	/// </summary>
	public interface IClientTransport
	{
		/// <summary>
		/// Called immediately after construction.
		/// </summary>
		void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback);

		/// <summary>
		/// Called immediately prior to disconnecting.
		/// </summary>
		void TearDown();

		/// <summary>
		/// Receive message, if any, from server.
		/// </summary>
		/// <returns>True if a message was received.</returns>
		bool Receive(byte[] buffer, out long size);

		/// <summary>
		/// Send message to server.
		/// </summary>
		void Send(byte[] buffer, long size, ENetReliability reliability);

		/// <summary>
		/// Get real IPv4 address of remote server NOT a relay server.
		/// </summary>
		/// <returns>True if address was available.</returns>
		bool TryGetIPv4Address(out IPv4Address address);

		/// <summary>
		/// Get real port of remote server NOT a relay server.
		/// Connection port is for game network traffic.
		/// </summary>
		/// <returns>True if port was available.</returns>
		bool TryGetConnectionPort(out ushort connectionPort);

		/// <summary>
		/// Get real port of remote server NOT a relay server.
		/// Query port is for Steam's "A2S" system.
		/// </summary>
		/// <returns>True if port was available.</returns>
		bool TryGetQueryPort(out ushort queryPort);

		/// <summary>
		/// Get ping calculated at transport level, if supported.
		/// </summary>
		/// <param name="pingMs">Ping measured in milliseconds.</param>
		/// <returns>True if ping was available.</returns>
		bool TryGetPing(out int pingMs);
	}

	public class ClientTransport_Null : IClientTransport
	{
		public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
		{}

		public bool Receive(byte[] buffer, out long size)
		{
			size = 0;
			return false;
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{}

		public void TearDown()
		{}

		public bool TryGetIPv4Address(out IPv4Address address)
		{
			address = IPv4Address.Zero;
			return false;
		}

		public bool TryGetConnectionPort(out ushort connectionPort)
		{
			connectionPort = 0;
			return false;
		}

		public bool TryGetQueryPort(out ushort queryPort)
		{
			queryPort = 0;
			return false;
		}

		public bool TryGetPing(out int pingMs)
		{
			pingMs = 0;
			return false;
		}
	}
}
