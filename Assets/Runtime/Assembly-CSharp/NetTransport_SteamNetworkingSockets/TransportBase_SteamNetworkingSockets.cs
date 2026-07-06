////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using Steamworks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace SDG.NetTransport.SteamNetworkingSockets
{
	public abstract class TransportBase_SteamNetworkingSockets : TransportBase
	{
		/// <summary>
		/// Log verbose information that should not be included in release builds.
		/// </summary>
		[Conditional("LOG_NETTRANSPORT_STEAMNETWORKINGSOCKETS")]
		internal void DebugLog(string format, params object[] args)
		{
			UnturnedLog.info(format, args);
		}

		/// <summary>
		/// Log helpful information that should be included in release builds.
		/// </summary>
		internal void Log(string format, params object[] args)
		{
			UnturnedLog.info(format, args);
		}

		internal string AddressToString(SteamNetworkingIPAddr address, bool withPort = true)
		{
			string buffer;
			address.ToString(out buffer, withPort);
			return buffer;
		}

		internal string IdentityToString(SteamNetworkingIdentity identity)
		{
			string buffer;
			identity.ToString(out buffer);
			return buffer;
		}

		internal string IdentityToString(ref SteamNetworkingMessage_t message)
		{
			return IdentityToString(message.m_identityPeer);
		}

		internal string IdentityToString(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			return IdentityToString(callback.m_info.m_identityRemote);
		}

		protected void DumpSteamNetworkingMessage(SteamNetworkingMessage_t message)
		{
			Log("Message Number {0}", message.m_nMessageNumber);
			Log("\tData: {0}", message.m_pData);
			Log("\tSize: {0}", message.m_cbSize);
			Log("\tConnection: {0}", message.m_conn);
			Log("\tPeer Identity: {0}", IdentityToString(message.m_identityPeer));
		}

		protected void LogDebugOutput()
		{
			DebugOutput debugOutput;
			while (debugOutputQueue.TryDequeue(out debugOutput))
			{
				string type;
				switch (debugOutput.type)
				{
					case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Bug:
						type = "Bug";
						break;

					case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Error:
						type = "Error";
						break;

					case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Important:
						type = "Important";
						break;

					case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Warning:
						type = "Warning";
						break;

					default:
						type = null;
						break;
				}

				if (string.IsNullOrEmpty(type))
				{
					UnturnedLog.info($"SteamNetworkingSockets: {debugOutput.message}");
				}
				else
				{
					UnturnedLog.info($"SteamNetworkingSockets {type}: {debugOutput.message}");
				}
			}
		}

		internal int ReliabilityToSendFlags(ENetReliability reliability)
		{
			// Nagle value defaults to 5ms apparently, so we ignore nodelay here for now.
			switch (reliability)
			{
				default:
				case ENetReliability.Reliable:
					return Constants.k_nSteamNetworkingSend_Reliable;

				case ENetReliability.Unreliable:
					return Constants.k_nSteamNetworkingSend_Unreliable;
			}
		}

		protected ESteamNetworkingSocketsDebugOutputType SelectDebugOutputDetailLevel()
		{
			if (clLogSteamNetworkingSockets.hasValue)
			{
				try
				{
					return (ESteamNetworkingSocketsDebugOutputType) clLogSteamNetworkingSockets.value;
				}
				catch
				{
					Log("Unable to match {0} with a SNS output type", clLogSteamNetworkingSockets.value);
				}
			}

			// Anything other than None logs quite a bit, so this was changed from Warning back to None.
			// e.g. Issue #3406
			return ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None;
		}

		protected virtual List<SteamNetworkingConfigValue_t> BuildDefaultConfig()
		{
			List<SteamNetworkingConfigValue_t> configList = new List<SteamNetworkingConfigValue_t>();

			if (clAllowWithoutAuth)
			{
				SteamNetworkingConfigValue_t allowWithoutAuth = new SteamNetworkingConfigValue_t();
				allowWithoutAuth.m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
				allowWithoutAuth.m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth;
				allowWithoutAuth.m_val.m_int32 = 1;
				configList.Add(allowWithoutAuth);
			}

			if (clSendBufferSize.hasValue && clSendBufferSize.value > 0)
			{
				SteamNetworkingConfigValue_t sendBufferSize = new SteamNetworkingConfigValue_t();
				sendBufferSize.m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
				sendBufferSize.m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize;
				sendBufferSize.m_val.m_int32 = clSendBufferSize.value;
				configList.Add(sendBufferSize);
			}

			if (clEnableDiagnosticsUI)
			{
				SteamNetworkingConfigValue_t enableDiagnosticsUI = new SteamNetworkingConfigValue_t();
				enableDiagnosticsUI.m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
				enableDiagnosticsUI.m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_EnableDiagnosticsUI;
				enableDiagnosticsUI.m_val.m_int32 = 1;
				configList.Add(enableDiagnosticsUI);
			}

			SteamNetworkingConfigValue_t timeoutInitial = new SteamNetworkingConfigValue_t();
			timeoutInitial.m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
			timeoutInitial.m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial;
			timeoutInitial.m_val.m_int32 = 30 * 1000;
			configList.Add(timeoutInitial);

			SteamNetworkingConfigValue_t timeoutConnected = new SteamNetworkingConfigValue_t();
			timeoutConnected.m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
			timeoutConnected.m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
			timeoutConnected.m_val.m_int32 = 30 * 1000;
			configList.Add(timeoutConnected);

			return configList;
		}

		/// <summary>
		/// Should certificate authentication be disabled for UDP connections?
		/// </summary>
		protected static CommandLineFlag clAllowWithoutAuth = new CommandLineFlag(false, "-SNS_AllowWithoutAuth");

		protected FSteamNetworkingSocketsDebugOutput GetDebugOutputFunction()
		{
			debugOutputFunc = OnDebugOutput;
			return debugOutputFunc;
		}

		/// <summary>
		/// This callback may be called from a service thread. It must be threadsafe and fast! Do not make any other
		/// Steamworks calls from within the handler.
		/// </summary>
		private void OnDebugOutput(ESteamNetworkingSocketsDebugOutputType nType, System.IntPtr pszMsg)
		{
			try
			{
				// 2022-08-29: used StringBuilder in the past, but a dev pointed out a crash in pre-Unity-2021 mono with this.
				string message = InteropHelp.PtrToStringUTF8(pszMsg);
				if (!string.IsNullOrEmpty(message))
				{
					DebugOutput debugOutput = new DebugOutput();
					debugOutput.type = nType;
					debugOutput.message = message;
					debugOutputQueue.Enqueue(debugOutput);
				}
			}
			catch
			{
				// Hack? Ideally there should be no exceptions, however if there IS somehow an exception then it crashes
				// everything so we silently catch it.
			}
		}

		private struct DebugOutput
		{
			public ESteamNetworkingSocketsDebugOutputType type;
			public string message;
		}

		/// <summary>
		/// Ensures GC does not release the delegate.
		/// </summary>
		private FSteamNetworkingSocketsDebugOutput debugOutputFunc;

		private ConcurrentQueue<DebugOutput> debugOutputQueue = new ConcurrentQueue<DebugOutput>();

		/// <summary>
		/// Does host want extra debug output?
		/// </summary>
		private static CommandLineInt clLogSteamNetworkingSockets = new CommandLineInt("-LogSteamNetworkingSockets");

		/// <summary>
		/// Overrides k_ESteamNetworkingConfig_SendBufferSize.
		/// </summary>
		private static CommandLineInt clSendBufferSize = new CommandLineInt("-SNS_SendBufferSize");

		/// <summary>
		/// Overrides k_ESteamNetworkingConfig_EnableDiagnosticsUI.
		/// </summary>
		private static CommandLineFlag clEnableDiagnosticsUI = new CommandLineFlag(false, "-SNS_EnableDiagnosticsUI");
	}
}
