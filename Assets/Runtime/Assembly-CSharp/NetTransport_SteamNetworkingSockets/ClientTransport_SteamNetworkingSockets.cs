////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define SNS_LOG_PING
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unturned.SystemEx;

namespace SDG.NetTransport.SteamNetworkingSockets
{
	public class ClientTransport_SteamNetworkingSockets : TransportBase_SteamNetworkingSockets, IClientTransport
	{
		public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
		{
			connectedCallback = callback;
			this.failureCallback = failureCallback;

			steamNetConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamNetConnectionStatusChanged);
			steamNetAuthenticationStatusChanged = Callback<SteamNetAuthenticationStatus_t>.Create(OnSteamNetAuthenticationStatusChanged);

			ESteamNetworkingSocketsDebugOutputType detailLevel = SelectDebugOutputDetailLevel();
			if (detailLevel != ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None)
			{
				didSetupDebugOutput = true;
				Log("Client set SNS debug output detail level to {0}", detailLevel);
				SteamNetworkingUtils.SetDebugOutputFunction(detailLevel, GetDebugOutputFunction());
			}

			Framework.Utilities.TimeUtility.updated += OnUpdate;

			if (clAllowWithoutAuth)
			{
				isWaitingForAuthAvailability = false;
				Log("Client bypassing test for Steam Networking availability");
				Connect();
			}
			else
			{
				isWaitingForAuthAvailability = true;
				ESteamNetworkingAvailability authAvailability = Steamworks.SteamNetworkingSockets.InitAuthentication();
				if (authAvailability != ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current)
				{
					Log("Client testing for Steam Networking availability ({0})", authAvailability);
				}
				HandleAuth(authAvailability);
			}
		}

		public void TearDown()
		{
			steamNetConnectionStatusChanged.Dispose();
			steamNetAuthenticationStatusChanged.Dispose();

			if (!didCloseConnection && connection != HSteamNetConnection.Invalid)
			{
				didCloseConnection = true;
				const bool bEnableLinger = true; // Finish sending Reliable messages. In particular the GracefullyDisconnect message.
				bool result = Steamworks.SteamNetworkingSockets.CloseConnection(connection, /*nReason*/ 0, /*pszDebug*/ null, bEnableLinger);
				Log("Client disconnect from {0} result: {1}", connection, result);
			}

			Framework.Utilities.TimeUtility.updated -= OnUpdate;

			if (didSetupDebugOutput)
			{
				didSetupDebugOutput = false;
				SteamNetworkingUtils.SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None, null);
			}
		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
			if (!isConnected || didCloseConnection)
				return;

			int sendFlags = ReliabilityToSendFlags(reliability);

			EResult result;
			unsafe
			{
				fixed (byte* bufferPtr = buffer)
				{
					IntPtr bufferIntPtr = new IntPtr(bufferPtr);
					long messageNumber;
					result = Steamworks.SteamNetworkingSockets.SendMessageToConnection(connection, bufferIntPtr, (uint) size, sendFlags, out messageNumber);
				}
			}

			if (result == EResult.k_EResultOK)
			{
				DebugLog("Client sent {0} byte message to server", size);
			}
			else
			{
				DebugLog("Client error {0} sending {1} byte message to server", result, size);
			}
		}

		public bool Receive(byte[] buffer, out long size)
		{
			size = 0;

			if (!isConnected || didCloseConnection)
				return false;

#if SNS_LOG_PING
			if (Steamworks.SteamNetworkingSockets.GetQuickConnectionStatus(connection, out SteamNetworkingQuickConnectionStatus info))
			{
				Log($"SteamNetworkingSockets Ping: {info.m_nPing}ms");
			}
#endif // SNS_LOG_PING

			while (true)
			{
				int messageCount = Steamworks.SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messageAddresses, messageAddresses.Length);
				if (messageCount < 1)
					return false;

				IntPtr messageAddress = messageAddresses[0];
				SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messageAddress);

				if (message.m_pData == IntPtr.Zero || message.m_cbSize < 1)
				{
					// Yes, this can actually happen for some reason.
					SteamNetworkingMessage_t.Release(messageAddress);
					DebugLog("Client dropping empty message from server (Size: {0})", message.m_cbSize);
					continue;
				}

				size = message.m_cbSize;
				if (size > buffer.Length)
				{
					size = buffer.Length;
					DebugLog("Client received {0} byte message from server (truncated from {1} bytes)", size, message.m_cbSize);
				}
				else
				{
					DebugLog("Client received {0} byte message from server", size);
				}

				Marshal.Copy(message.m_pData, buffer, 0, (int) size);

				SteamNetworkingMessage_t.Release(messageAddress);
				return true;
			}
		}

		public bool TryGetIPv4Address(out IPv4Address address)
		{
			if (!isConnected || didCloseConnection)
			{
				address = IPv4Address.Zero;
				return false;
			}

			if (isRemoteUsingFakeIP)
			{
				address = SDG.Unturned.Provider.CurrentServerConnectParameters.address;
				return true;
			}
			else
			{
				bool hasConnectionInfo = Steamworks.SteamNetworkingSockets.GetConnectionInfo(connection, out SteamNetConnectionInfo_t info);
				uint ipv4 = hasConnectionInfo ? info.m_addrRemote.GetIPv4() : 0;
				address = new IPv4Address(ipv4);
				return hasConnectionInfo && ipv4 != 0;
			}
		}

		public bool TryGetConnectionPort(out ushort connectionPort)
		{
			if (!isConnected || didCloseConnection)
			{
				connectionPort = 0;
				return false;
			}

			if (isRemoteUsingFakeIP)
			{
				// Technically the connection port is unused in "Fake IP" mode, but we preserve
				// the assumptions about it to avoid breaking existing code.
				connectionPort = SDG.Unturned.Provider.CurrentServerConnectParameters.connectionPort;
				return true;
			}
			else
			{
				bool hasConnectionInfo = Steamworks.SteamNetworkingSockets.GetConnectionInfo(connection, out SteamNetConnectionInfo_t info);
				connectionPort = hasConnectionInfo ? info.m_addrRemote.m_port : (ushort) 0;
				return hasConnectionInfo && connectionPort != 0;
			}
		}

		public bool TryGetQueryPort(out ushort queryPort)
		{
			if (!isConnected || didCloseConnection)
			{
				queryPort = 0;
				return false;
			}

			if (isRemoteUsingFakeIP)
			{
				// Ordinarily the query port is the connection port minus one, but in "Fake IP"
				// mode the query port is used for both purposes, so we preserve the assumptions
				// about it to avoid breaking existing code.
				queryPort = SDG.Unturned.Provider.CurrentServerConnectParameters.queryPort;
				return true;
			}
			else
			{
				bool hasConnectionInfo = Steamworks.SteamNetworkingSockets.GetConnectionInfo(connection, out SteamNetConnectionInfo_t info);
				queryPort = hasConnectionInfo ? SDG.Unturned.MathfEx.ClampToUShort(info.m_addrRemote.m_port - 1) : (ushort) 0;
				return hasConnectionInfo && queryPort != 0;
			}
		}

		public bool TryGetPing(out int pingMs)
		{
			if (!isConnected || didCloseConnection)
			{
				pingMs = 0;
				return false;
			}

			SteamNetConnectionRealTimeStatus_t status = default;
			SteamNetConnectionRealTimeLaneStatus_t lanes = default;
			EResult result = Steamworks.SteamNetworkingSockets.GetConnectionRealTimeStatus(connection, ref status, 0, ref lanes);
			if (result != EResult.k_EResultOK)
			{
				pingMs = 0;
				return false;
			}

			pingMs = status.m_nPing;
			return true;
		}

		private void OnUpdate()
		{
			LogDebugOutput();
		}

		private void Connect()
		{
			List<SteamNetworkingConfigValue_t> configList = BuildDefaultConfig();
			SteamNetworkingConfigValue_t[] configArray = configList.ToArray();

			if (!Unturned.Provider.CurrentServerConnectParameters.address.IsZero)
			{
				SteamNetworkingIPAddr address = new SteamNetworkingIPAddr();

				uint ipv4 = Unturned.Provider.CurrentServerConnectParameters.address.value;
				if (SteamNetworkingUtils.IsFakeIPv4(ipv4))
				{
					// Important to use query port rather than connection port for "Fake IP" mode!
					// Please refer to ServerConnectParameters.connectionPort for more information.
					isRemoteUsingFakeIP = true;
					address.SetIPv4(ipv4, Unturned.Provider.CurrentServerConnectParameters.queryPort);
					Log("Client connecting to {0} (FakeIP)", AddressToString(address));
				}
				else
				{
					address.SetIPv4(ipv4, Unturned.Provider.CurrentServerConnectParameters.connectionPort);
					Log("Client connecting to {0}", AddressToString(address));
				}

				connection = Steamworks.SteamNetworkingSockets.ConnectByIPAddress(ref address, configArray.Length, configArray);
			}
			else
			{
				SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
				identity.SetSteamID(Unturned.Provider.CurrentServerConnectParameters.steamId);
				connection = Steamworks.SteamNetworkingSockets.ConnectP2P(ref identity, 0, configArray.Length, configArray);
				Log("Client connecting to {0} (P2P)", IdentityToString(identity));
			}
		}

#pragma warning disable
		private Callback<SteamNetConnectionStatusChangedCallback_t> steamNetConnectionStatusChanged;
#pragma warning restore
		private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
		{
			if (callback.m_hConn != connection)
			{
				DebugLog("Unknown connection with {0} status changed", IdentityToString(callback.m_info.m_identityRemote));
				return;
			}

			DebugLog("Connection with {0} changed state from {1} to {2}", IdentityToString(callback.m_info.m_identityRemote), callback.m_eOldState, callback.m_info.m_eState);
			switch (callback.m_info.m_eState)
			{
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
					return; // Handle case just to avoid the unknown state logging.

				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					HandleState_Connected(ref callback);
					return;

				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
					HandleState_ClosedByPeer(ref callback);
					return;

				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					HandleState_ProblemDetectedLocally(ref callback);
					return;

				default:
					DebugLog("Connection with {0} in unhandled state {1}", IdentityToString(callback.m_info.m_identityRemote), callback.m_info.m_eState);
					return;
			}
		}

		private void HandleState_Connected(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			if (connectedCallback == null)
				return;

			isConnected = true;
			Log("Client connection with {0} ready", connection);
			connectedCallback();
			connectedCallback = null; // Prevent accidentally calling it multiple times.
		}

		private string GetMessageForEndReason(int endReasonCode)
		{
			ESteamNetConnectionEnd endReason;
			try
			{
				endReason = (ESteamNetConnectionEnd) endReasonCode;
			}
			catch
			{
				endReason = ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Invalid;
			}

			// 1xxx: Application ended the connection in a "usual" manner.
			//       E.g.: user intentionally disconnected from the server,
			//             gameplay ended normally, etc
			if (endReasonCode >= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Min &&
				endReasonCode <= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Max)
			{
				switch (endReasonCode)
				{
					case 1001: // Server Full
						return GetMessageText("SteamNetworkingSockets_EndReason_App_1001");
					case 1002: // Server Shutdown
						return GetMessageText("SteamNetworkingSockets_EndReason_App_1002");
				}

				switch (endReason)
				{
					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Generic:
						return null; // Client should have ended the connection as well.

					default:
						return GetMessageText("SteamNetworkingSockets_EndReason_App_Unknown", endReasonCode);
				}
			}

			// 2xxx: Application ended the connection in some sort of exceptional
			//       or unusual manner that might indicate a bug or configuration
			//       issue.
			if (endReasonCode >= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_AppException_Min &&
				endReasonCode <= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_AppException_Max)
			{
				switch (endReason)
				{
					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_AppException_Generic:
						return GetMessageText("SteamNetworkingSockets_EndReason_AppException_Generic");

					default:
						return GetMessageText("SteamNetworkingSockets_EndReason_AppException_Unknown", endReasonCode);
				}
			}

			// 3xxx: Connection failed or ended because of problem with the
			//       local host or their connection to the Internet.
			if (endReasonCode >= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_Min &&
				endReasonCode <= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_Max)
			{
				switch (endReason)
				{
					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_OfflineMode:
						return GetMessageText("SteamNetworkingSockets_EndReason_Local_OfflineMode");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_ManyRelayConnectivity:
					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_HostedServerPrimaryRelay:
						return GetMessageText("SteamNetworkingSockets_EndReason_Local_RelayConnectivity");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_NetworkConfig:
						return GetMessageText("SteamNetworkingSockets_EndReason_Local_NetworkConfig");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Local_Rights:
						return GetMessageText("SteamNetworkingSockets_EndReason_Local_Rights");

					default:
						return GetMessageText("SteamNetworkingSockets_EndReason_Local_Unknown", endReasonCode);
				}
			}

			// 4xxx: Connection failed or ended, and it appears that the
			//       cause does NOT have to do with the local host or their
			//       connection to the Internet.  It could be caused by the
			//       remote host, or it could be somewhere in between.
			if (endReasonCode >= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_Min &&
				endReasonCode <= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_Max)
			{
				switch (endReason)
				{
					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_BadCrypt:
						return GetMessageText("SteamNetworkingSockets_EndReason_Remote_BadCrypt");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_BadCert:
						return GetMessageText("SteamNetworkingSockets_EndReason_Remote_BadCert");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_Timeout:
						return GetMessageText("SteamNetworkingSockets_EndReason_Remote_Timeout");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_BadProtocolVersion:
						return GetMessageText("SteamNetworkingSockets_EndReason_Remote_BadProtocolVersion");

					default:
						return GetMessageText("SteamNetworkingSockets_EndReason_Remote_Unknown", endReasonCode);
				}
			}

			// 5xxx: Connection failed for some other reason.
			if (endReasonCode >= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_Min &&
				endReasonCode <= (int) ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_Max)
			{
				switch (endReason)
				{
					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_InternalError:
						return GetMessageText("SteamNetworkingSockets_EndReason_Misc_InternalError");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_Timeout:
						return GetMessageText("SteamNetworkingSockets_EndReason_Misc_Timeout");

					case ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_SteamConnectivity:
						return GetMessageText("SteamNetworkingSockets_EndReason_Misc_SteamConnectivity");

					default:
						return GetMessageText("SteamNetworkingSockets_EndReason_Misc_Unknown", endReasonCode);
				}
			}

			return GetMessageText("SteamNetworkingSockets_EndReason_Unknown", endReasonCode);
		}

		private void InvokeFailureCallback(string message)
		{
			if (failureCallback == null)
				return;

			ClientTransportFailure tempFailure = failureCallback;
			failureCallback = null; // Avoid calling twice if callback causes another failure somehow.
			tempFailure(message);
		}

		private void InvokeFailureCallback(int endReasonCode)
		{
			if (failureCallback == null)
				return;

			string message = GetMessageForEndReason(endReasonCode);
			if (string.IsNullOrEmpty(message))
				return; // Non-error e.g. app generic 1000.

			InvokeFailureCallback(message);
		}

		/// <summary>
		/// Must close the handle to free up resources.
		/// </summary>
		private void HandleState_ClosedByPeer(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			Log("Client connection closed by peer ({0}) \"{1}\"", callback.m_info.m_eEndReason, callback.m_info.m_szEndDebug);
			didCloseConnection = true;
			bool result = Steamworks.SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, false);
			if (!result)
			{
				Log("Client failed to release connection closed by peer");
			}
			InvokeFailureCallback(callback.m_info.m_eEndReason);
		}

		/// <summary>
		/// Must close the handle to free up resources.
		/// </summary>
		private void HandleState_ProblemDetectedLocally(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			Log("Client connection problem detected locally ({0}) \"{1}\"", callback.m_info.m_eEndReason, callback.m_info.m_szEndDebug);
			didCloseConnection = true;
			bool result = Steamworks.SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, false);
			if (!result)
			{
				Log("Client failed to release connection after problem detected locally");
			}
			InvokeFailureCallback(callback.m_info.m_eEndReason);
		}

#pragma warning disable
		private Callback<SteamNetAuthenticationStatus_t> steamNetAuthenticationStatusChanged;
#pragma warning restore
		private void OnSteamNetAuthenticationStatusChanged(SteamNetAuthenticationStatus_t callback)
		{
			if (string.IsNullOrEmpty(callback.m_debugMsg))
			{
				Log("Readiness to participate in authenticated communications changed to {0}", callback.m_eAvail);
			}
			else
			{
				Log("Readiness to participate in authenticated communications changed to {0} \"{1}\"", callback.m_eAvail, callback.m_debugMsg);
			}

			if (isWaitingForAuthAvailability)
			{
				HandleAuth(callback.m_eAvail);
			}
		}

		private void HandleAuth(ESteamNetworkingAvailability authAvailability)
		{
			switch (authAvailability)
			{
				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_CannotTry:
					HandleAuth_CannotTry();
					return;

				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Failed:
					HandleAuth_Failed();
					return;

				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Previously:
					HandleAuth_Previously();
					return;

				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Retrying:
				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_NeverTried:
				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Waiting:
				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Attempting:
					return; // Wait for an actual result.

				case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current:
					HandleAuth_Current();
					return;
			}
		}

		private void HandleAuth_CannotTry()
		{
			isWaitingForAuthAvailability = false;
			InvokeFailureCallback(GetMessageText("SteamNetworkingSockets_Unavailable_CannotTry"));
		}

		private void HandleAuth_Failed()
		{
			isWaitingForAuthAvailability = false;
			InvokeFailureCallback(GetMessageText("SteamNetworkingSockets_Unavailable_Failed"));
		}

		private void HandleAuth_Previously()
		{
			isWaitingForAuthAvailability = false;
			InvokeFailureCallback(GetMessageText("SteamNetworkingSockets_Unavailable_Previously"));
		}

		private void HandleAuth_Current()
		{
			Log("Client Steam Networking available");
			isWaitingForAuthAvailability = false;
			Connect();
		}

		private ClientTransportReady connectedCallback;
		private ClientTransportFailure failureCallback;
		private HSteamNetConnection connection = HSteamNetConnection.Invalid;
		private bool isWaitingForAuthAvailability;
		private bool isConnected;
		private bool didCloseConnection;
		private bool didSetupDebugOutput;
		private bool isRemoteUsingFakeIP;

		/// <summary>
		/// Recycled array for every read call.
		/// </summary>
		private IntPtr[] messageAddresses = new IntPtr[1];
	}
}
