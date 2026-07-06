////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	/// <summary>
	/// When adding or removing entries remember to update NetMessages size and regenerate NetCode!
	/// </summary>
	[NetEnum]
	public enum EServerMessage
	{
		/// <summary>
		/// Client requesting workshop files to download.
		/// </summary>
		GetWorkshopFiles,

		/// <summary>
		/// Client has loaded the level.
		/// </summary>
		ReadyToConnect,

		/// <summary>
		/// Client providing Steam login token.
		/// </summary>
		Authenticate,

#if WITH_THIRDPARTYAC
		/// <summary>
		/// Client sending thirdparty anti-cheat payload to server.
		/// </summary>
		ThirdPartyAntiCheat,
#endif // WITH_THIRDPARTYAC

		/// <summary>
		/// Client sent a ping.
		/// </summary>
		PingRequest,

		/// <summary>
		/// Client responded to our ping.
		/// </summary>
		PingResponse,

		/// <summary>
		/// Client calling an RPC.
		/// </summary>
		InvokeMethod,

		/// <summary>
		/// Client providing asset GUIDs with their file hashes to check integrity.
		/// </summary>
		ValidateAssets,

		/// <summary>
		/// Client intends to disconnect. It is fine if server does not receive this message
		/// because players are also removed for transport failure (e.g. timeout) and for expiry
		/// of Steam authentication ticket. This message is useful to know the client instigated
		/// the disconnection rather than an error.
		/// </summary>
		GracefullyDisconnect,

		// Refer to top of file for instructions on adding/removing entries.
	}
}
