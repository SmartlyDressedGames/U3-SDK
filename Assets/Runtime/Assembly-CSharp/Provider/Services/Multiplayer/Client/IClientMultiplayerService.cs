////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Community;
using System;
using System.IO;

namespace SDG.Provider.Services.Multiplayer.Client
{
	public interface IClientMultiplayerService : IService
	{
		/// <summary>
		/// Information about currently connected server.
		/// </summary>
		IServerInfo serverInfo
		{
			get;
		}

		/// <summary>
		/// Whether a server is currently connected to.
		/// </summary>
		bool isConnected
		{
			get;
		}

		/// <summary>
		/// Whether connection attempts are being made.
		/// </summary>
		bool isAttempting
		{
			get;
		}

		/// <summary>
		/// Network buffer memory stream.
		/// </summary>
		MemoryStream stream
		{
			get;
		}

		/// <summary>
		/// Network buffer memory stream reader.
		/// </summary>
		BinaryReader reader
		{
			get;
		}

		/// <summary>
		/// Network buffer memory stream writer.
		/// </summary>
		BinaryWriter writer
		{
			get;
		}

		/// <summary>
		/// Connect to a server.
		/// </summary>
		/// <param name="newServerInfo">Server to join.</param>
		void connect(IServerInfo newServerInfo);

		/// <summary>
		/// Disconnect from current server.
		/// </summary>
		void disconnect();

		/// <summary>
		/// Receive a packet from an entity across the network.
		/// </summary>
		/// <param name="entity">Sender of data.</param>
		/// <param name="data"></param>
		/// <param name="length"></param>
		/// <returns>Whether any data was read.</returns>
		bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel);

		/// <summary>
		/// Send a packet to an entity across the network.
		/// </summary>
		/// <param name="entity">Recipient of data.</param>
		/// <param name="data">Packet to send.</param>
		/// <param name="length">Size of data in array.</param>
		void write(ICommunityEntity entity, byte[] data, ulong length);

		/// <summary>
		/// Send a packet to an entity across the network.
		/// </summary>
		/// <param name="entity">Recipient of data.</param>
		/// <param name="data">Packet to send.</param>
		/// <param name="length">Size of data in array.</param>
		/// <param name="method">Type of send to use.</param>
		[Obsolete("Used by old multiplayer code, please use send without method instead.")]
		void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel);
	}
}