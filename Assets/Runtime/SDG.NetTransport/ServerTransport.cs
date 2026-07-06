////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetTransport
{
	/// <summary>
	/// Callback when client should be removed from the server due to a lower-level connection change.
	/// For example this can happen if the application is closed before the Steam authentication ticket
	/// is canceled. That case is not an error, but some cases like timeout is considered an error.
	/// </summary>
	public delegate void ServerTransportConnectionFailureCallback(ITransportConnection transportConnection, string debugString, bool isError);

	/// <summary>
	/// Abstracts dedicated server communication with clients.
	/// </summary>
	public interface IServerTransport
	{
		/// <summary>
		/// Called immediately after construction.
		/// connectionFailureCallback is invoked if a connection unexpectedly closes, i.e., game code did not request it.
		/// </summary>
		void Initialize(ServerTransportConnectionFailureCallback connectionFailureCallback);

		/// <summary>
		/// Called when shutting down the server.
		/// </summary>
		void TearDown();

		/// <summary>
		/// Receive message, if any, from a client.
		/// </summary>
		/// <returns>True if a message was received.</returns>
		bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection);
	}
}
