////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned
{
	/// <summary>
	/// Optional parameter for error logging and responding to the invoker.
	/// </summary>
	public readonly struct ServerInvocationContext
	{
		public enum EOrigin
		{
			Remote,
			Loopback,
			Obsolete,
		}

		public readonly EOrigin origin;
		public readonly NetPakReader reader;

		internal bool IsOwnerOf(SteamChannel legacyComponent)
		{
			return legacyComponent.owner != null && legacyComponent.owner == callingPlayer;
		}

		public Player GetPlayer()
		{
			return callingPlayer?.player;
		}

		public SteamPlayer GetCallingPlayer()
		{
			return callingPlayer;
		}

		public ITransportConnection GetTransportConnection()
		{
			return callingPlayer.transportConnection;
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("DEBUG_NETINVOKABLES")]
		public void ReadParameterFailed(string parameterName)
		{
			CommandWindow.LogWarningFormat("{0} {1}: unable to read {2}", GetTransportConnection(), serverMethodInfo, parameterName);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("DEBUG_NETINVOKABLES")]
		public void LogWarning(string message)
		{
			CommandWindow.LogWarningFormat("{0} {1}: {2}", GetTransportConnection(), serverMethodInfo, message);
		}

		public void Kick(string reason)
		{
			if (callingPlayer != null)
			{
				Provider.kick(callingPlayer.playerID.steamID, reason);
			}
		}

		[System.Obsolete("Only exists for plugins manually calling obsolete RPCs with steamID sender parameter. Do not use directly. Will remove.")]
		internal static ServerInvocationContext FromSteamIDForBackwardsCompatibility(Steamworks.CSteamID steamID)
		{
			return new ServerInvocationContext(steamID);
		}

		internal ServerInvocationContext(EOrigin origin, SteamPlayer callingPlayer, NetPakReader reader, ServerMethodInfo serverMethodInfo)
		{
			this.origin = origin;
			this.callingPlayer = callingPlayer;
			this.reader = reader;
			this.serverMethodInfo = serverMethodInfo;
		}

		[System.Obsolete("Only exists for plugins manually calling obsolete RPCs with steamID sender parameter.")]
		private ServerInvocationContext(Steamworks.CSteamID steamID)
		{
			origin = EOrigin.Obsolete;
			callingPlayer = PlayerTool.getSteamPlayer(steamID);
			reader = null;
			serverMethodInfo = null;
		}

		private readonly SteamPlayer callingPlayer;
		private readonly ServerMethodInfo serverMethodInfo;
	}
}
