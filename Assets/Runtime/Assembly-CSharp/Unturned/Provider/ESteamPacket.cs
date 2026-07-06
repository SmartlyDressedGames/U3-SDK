////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	[Obsolete]
	public enum ESteamPacket
	{
		// Order of these update types was moved to match client/server message until RPC rewrite.
		[Obsolete]
		UPDATE_RELIABLE_BUFFER,
		[Obsolete]
		UPDATE_UNRELIABLE_BUFFER,
		[Obsolete]
		UPDATE_RELIABLE_CHUNK_BUFFER,
		[Obsolete]
		UPDATE_UNRELIABLE_CHUNK_BUFFER,
		[Obsolete]
		UPDATE_VOICE,

		[Obsolete]
		SHUTDOWN, // server shutdown
		[Obsolete]
		WORKSHOP, // workshop info
		[Obsolete]
		CONNECT, // player connecting to server
		[Obsolete]
		VERIFY, // server asking for verification
		[Obsolete]
		AUTHENTICATE, // player providing verification
		[Obsolete]
		REJECTED, // server rejected player
		[Obsolete]
		ACCEPTED, // server accepted player
		[Obsolete]
		ADMINED, // server admined a player
		[Obsolete]
		UNADMINED, // server unadmined a player
		[Obsolete]
		BANNED, // player is banned on server
		[Obsolete]
		KICKED, // player was kicked from a server
		[Obsolete]
		CONNECTED, // a new player connected
		[Obsolete]
		DISCONNECTED, // an old player disconnected
		[Obsolete]
		PING_REQUEST, // just so everyone knows you still exist
		[Obsolete]
		PING_RESPONSE, // sync time on the server

		[Obsolete("Unused and will kick sender.")]
		UPDATE_RELIABLE_INSTANT, // send rpc on tcp
		[Obsolete("Unused and will kick sender.")]
		UPDATE_UNRELIABLE_INSTANT, // send rpc on udp

		[Obsolete("Unused and will kick sender.")]
		UPDATE_RELIABLE_CHUNK_INSTANT, // send chunk on tcp buffer
		[Obsolete("Unused and will kick sender.")]
		UPDATE_UNRELIABLE_CHUNK_INSTANT, // send chunk on udp buffer

		[Obsolete]
		THIRDPARTYAC, // back/forth thirdparty anti-cheat info
		[Obsolete]
		GUIDTABLE, // building GUID table

		/// <summary>
		/// Server response to a non-rejected CONNECT request. Notifies client they are in the queue.
		/// </summary>
		[Obsolete]
		CLIENT_PENDING,

		[Obsolete]
		MAX, // New types should be added before this one. Used to check if packet index is valid before casting to enum.
	}
}
