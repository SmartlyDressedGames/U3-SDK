////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Diagnostics;

namespace SDG.NetTransport.SystemSockets
{
	public abstract class TransportBase_SystemSockets
	{
		[Conditional("LOG_NETTRANSPORT_SYSTEMSOCKETS")]
		internal static void Log(string format, params object[] args)
		{
			UnturnedLog.info(format, args);
		}
	}
}
