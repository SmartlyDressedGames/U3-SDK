////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	/// <summary>
	/// Optional parameter for error logging.
	/// </summary>
	public readonly struct ClientInvocationContext
	{
		public enum EOrigin
		{
			Remote,
			Loopback,
			Deferred,
		}

		public readonly EOrigin origin;
		public readonly NetPakReader reader;

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("DEBUG_NETINVOKABLES")]
		public void ReadParameterFailed(string parameterName)
		{
			UnturnedLog.warn("{0}: unable to read {1}", clientMethodInfo, parameterName);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("DEBUG_NETINVOKABLES")]
		public void IndexOutOfRange(string parameterName, int index, int max)
		{
			UnturnedLog.error("{0}: {1} out of range ({2}/{3})", clientMethodInfo, parameterName, index, max);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("DEBUG_NETINVOKABLES")]
		public void LogWarning(string message)
		{
			UnturnedLog.warn("{0}: {1}", clientMethodInfo, message);
		}

		internal ClientInvocationContext(EOrigin origin, NetPakReader reader, ClientMethodInfo clientMethodInfo)
		{
			this.origin = origin;
			this.reader = reader;
			this.clientMethodInfo = clientMethodInfo;
		}

		internal readonly ClientMethodInfo clientMethodInfo;
	}
}
