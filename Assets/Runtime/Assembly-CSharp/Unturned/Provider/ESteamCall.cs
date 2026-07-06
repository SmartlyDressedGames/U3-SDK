////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum ESteamCall
	{
		/// <summary>
		/// Replaced by ServerMethodHandle.
		/// </summary>
		[System.Obsolete]
		SERVER,

		/// <summary>
		/// Replaced by ClientInstanceMethod.InvokeAndLoopback or ClientStaticMethod.InvokeAndLoopback.
		/// </summary>
		[System.Obsolete]
		ALL,

		/// <summary>
		/// Replaced by ClientMethodHandle invoked with Provider.EnumerateClients_Remote.
		/// Unlike ESteamCall.CLIENTS this is not loopback invoked.
		/// </summary>
		[System.Obsolete]
		OTHERS,

		/// <summary>
		/// Replaced by ClientMethodHandle invoked with SteamChannel.GetOwnerTransportConnection.
		/// </summary>
		[System.Obsolete]
		OWNER,

		/// <summary>
		/// Replaced by ClientMethodHandle invoked with SteamChannel.EnumerateClients_RemoteNotOwner.
		/// </summary>
		[System.Obsolete]
		NOT_OWNER,

		/// <summary>
		/// Replaced by ClientMethodHandle invoked with Provider.EnumerateClients.
		/// Unlike ESteamCall.OTHERS this will be loopback invoked in singleplayer or listen server.
		/// </summary>
		[System.Obsolete]
		CLIENTS,

		/// <summary>
		/// May have been used by voice in early versions, but has been completely removed.
		/// </summary>
		[System.Obsolete]
		PEERS
	}
}
