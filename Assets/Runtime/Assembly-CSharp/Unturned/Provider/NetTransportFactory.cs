////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using SDG.NetTransport.SteamNetworking;
using SDG.NetTransport.SteamNetworkingSockets;
using SDG.NetTransport.SystemSockets;
using System;

namespace SDG.Unturned
{
	/// <summary>
	/// Not extendable until transport API is better finalized.
	/// </summary>
	internal static class NetTransportFactory
	{
		internal const string SystemSocketsTag = "sys";
		internal const string SteamNetworkingSocketsTag = "sns";
		internal const string SteamNetworkingTag = "def"; // Stood for default when this was the default in the past.

		internal static string GetTag(IServerTransport serverTransport)
		{
			Type type = serverTransport.GetType();
			if (type == typeof(ServerTransport_SystemSockets))
			{
				return SystemSocketsTag;
			}
			else if (type == typeof(ServerTransport_SteamNetworkingSockets))
			{
				return SteamNetworkingSocketsTag;
			}
			else if (type == typeof(ServerTransport_SteamNetworking))
			{
				return SteamNetworkingTag;
			}
			else
			{
				UnturnedLog.warn("Unknown net transport \"{0}\", using default tag", type.Name);
				// If changing the default remember to update the other functions as well!
				return SteamNetworkingSocketsTag;
			}
		}

		internal static IClientTransport CreateClientTransport(string tag)
		{
			if (string.Equals(tag, SystemSocketsTag, StringComparison.OrdinalIgnoreCase))
			{
				return new ClientTransport_SystemSockets();
			}
			else if (string.Equals(tag, SteamNetworkingSocketsTag, StringComparison.OrdinalIgnoreCase))
			{
				return new ClientTransport_SteamNetworkingSockets();
			}
			else if (string.Equals(tag, SteamNetworkingTag, StringComparison.OrdinalIgnoreCase))
			{
				return new ClientTransport_SteamNetworking();
			}
			else
			{
				UnturnedLog.warn("Unknown net transport tag \"{0}\", using default", tag);
				// If changing the default remember to update the other functions as well!
				return new ClientTransport_SteamNetworkingSockets();
			}
		}

		internal static IServerTransport CreateServerTransport()
		{
			if (clImpl.hasValue)
			{
				string value = clImpl.value;
				if (string.Equals(value, "SystemSockets", StringComparison.OrdinalIgnoreCase))
				{
					return new ServerTransport_SystemSockets();
				}
				else if (string.Equals(value, "SteamNetworkingSockets", StringComparison.OrdinalIgnoreCase))
				{
					return new ServerTransport_SteamNetworkingSockets();
				}
				else if (string.Equals(value, "SteamNetworking", StringComparison.OrdinalIgnoreCase))
				{
					// We do not want hosts using old/legacy/deprecated Steam networking because it has a plethora of
					// known issues. That being said, keep a "secret" bypass around just in case.
					if (clBypassEnableOldSteamNetworking)
					{
						return new ServerTransport_SteamNetworking();
					}
					else
					{
						UnturnedLog.warn("Old Steam networking is no longer supported. Please remove this option from your command-line arguments.");
					}
				}
				else
				{
					UnturnedLog.warn("Unknown net transport implementation \"{0}\"", value);
				}
			}

			// If changing the default remember to update the other functions as well!
			return new ServerTransport_SteamNetworkingSockets();
		}

		private static CommandLineString clImpl = new CommandLineString("-NetTransport");
		private static CommandLineFlag clBypassEnableOldSteamNetworking = new CommandLineFlag(false, "-BypassEnableOldSteamNetworking");
	}
}
