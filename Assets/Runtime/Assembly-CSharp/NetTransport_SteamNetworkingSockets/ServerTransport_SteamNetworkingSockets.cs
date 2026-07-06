////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SDG.NetTransport.SteamNetworkingSockets
{
	public class ServerTransport_SteamNetworkingSockets : TransportBase_SteamNetworkingSockets, IServerTransport
	{
		public void Initialize(ServerTransportConnectionFailureCallback connectionFailureCallback)
		{
			this.connectionFailureCallback = connectionFailureCallback;

			steamNetConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.CreateGameServer(OnSteamNetConnectionStatusChanged);
			steamNetAuthenticationStatusChanged = Callback<SteamNetAuthenticationStatus_t>.CreateGameServer(OnSteamNetAuthenticationStatusChanged);

			ESteamNetworkingSocketsDebugOutputType detailLevel = SelectDebugOutputDetailLevel();
			if (detailLevel != ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None)
			{
				didSetupDebugOutput = true;
				Log("Server set SNS debug output detail level to {0}", detailLevel);
				SteamGameServerNetworkingUtils.SetDebugOutputFunction(detailLevel, GetDebugOutputFunction());
			}

			Framework.Utilities.TimeUtility.updated += OnUpdate;
			SDG.Unturned.CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			List<SteamNetworkingConfigValue_t> configList = BuildDefaultConfig();
			SteamNetworkingConfigValue_t[] configArray = configList.ToArray();

			SteamNetworkingIPAddr localAddress = new SteamNetworkingIPAddr();
			if (string.IsNullOrEmpty(Unturned.Provider.bindAddress))
			{
				localAddress.Clear();
			}
			else
			{
				// 2021-12-10 bug with SteamGameServerNetworkingUtils.SteamNetworkingIPAddr_ParseString
				if (Unturned.Provider.ip > 0)
				{
					// We set port later.
					localAddress.SetIPv4(Unturned.Provider.ip, 0);
				}
				else
				{
					Log("Unable to parse \"{0}\" as listen bind address", Unturned.Provider.bindAddress);
					localAddress.Clear();
				}
			}

			localAddress.m_port = Unturned.Provider.GetServerConnectionPort();

			if (clUseIpSocket)
			{
				ipListenSocket = SteamGameServerNetworkingSockets.CreateListenSocketIP(ref localAddress, configArray.Length, configArray);
				Log("Server listen socket bound to {0}", AddressToString(localAddress));
			}
			else
			{
				Log("Server skipping creation of IP listen socket");
			}

			if (SDG.Unturned.Provider.configData.Server.Use_FakeIP)
			{
				fakeIpListenSocket = SteamGameServerNetworkingSockets.CreateListenSocketP2PFakeIP(0, configArray.Length, configArray);
				Log("Server FakeIP listen socket: {0}", fakeIpListenSocket);

				SteamGameServerNetworkingSockets.GetFakeIP(0, out SteamNetworkingFakeIPResult_t fakeIpInfo);
				if (fakeIpInfo.m_eResult == EResult.k_EResultBusy)
				{
					// Need to wait for callback.
					steamNetworkingFakeIpResultCallback = Callback<SteamNetworkingFakeIPResult_t>.CreateGameServer(OnSteamNetworkingFakeIpResultCallback);
					Log("Waiting for FakeIP callback...");
				}
				else
				{
					OnSteamNetworkingFakeIpResultCallback(fakeIpInfo);
				}
			}

			if (clUseP2pSocket)
			{
				p2pListenSocket = SteamGameServerNetworkingSockets.CreateListenSocketP2P(0, configArray.Length, configArray);
				Log("Server P2P listen socket: {0}", p2pListenSocket);
			}
			else
			{
				Log("Server skipping creation of P2P listen socket");
			}

			if (ipListenSocket == HSteamListenSocket.Invalid
				&& fakeIpListenSocket == HSteamListenSocket.Invalid
				&& p2pListenSocket == HSteamListenSocket.Invalid)
			{
				Log("SNS did not create any sockets! This will probably not work properly!");
			}

			pollGroup = SteamGameServerNetworkingSockets.CreatePollGroup();
			DebugLog("Server created poll group {0}", pollGroup);
		}

		public void TearDown()
		{
			Framework.Utilities.TimeUtility.updated -= OnUpdate;
			SDG.Unturned.CommandLogMemoryUsage.OnExecuted -= OnLogMemoryUsage;
			steamNetConnectionStatusChanged.Dispose();
			steamNetAuthenticationStatusChanged.Dispose();
			steamNetworkingFakeIpResultCallback?.Dispose();

			if (ipListenSocket != HSteamListenSocket.Invalid)
			{
				bool closeIpListenSocketResult = SteamGameServerNetworkingSockets.CloseListenSocket(ipListenSocket);
				if (!closeIpListenSocketResult)
				{
					Log("Server failed to close IP listen socket {0}", ipListenSocket);
				}
			}

			if (fakeIpListenSocket != HSteamListenSocket.Invalid)
			{
				bool closeFakeIpListenSocketResult = SteamGameServerNetworkingSockets.CloseListenSocket(fakeIpListenSocket);
				if (!closeFakeIpListenSocketResult)
				{
					Log("Server failed to close \"Fake IP\" listen socket {0}", closeFakeIpListenSocketResult);
				}
			}

			if (p2pListenSocket != HSteamListenSocket.Invalid)
			{
				bool closeP2pListenSocketResult = SteamGameServerNetworkingSockets.CloseListenSocket(p2pListenSocket);
				if (!closeP2pListenSocketResult)
				{
					Log("Server failed to close P2P listen socket {0}", p2pListenSocket);
				}
			}

			bool destroyPollGroupResult = SteamGameServerNetworkingSockets.DestroyPollGroup(pollGroup);
			if (!destroyPollGroupResult)
			{
				Log("Server failed to destroy poll group {0}", pollGroup);
			}

			if (didSetupDebugOutput)
			{
				didSetupDebugOutput = false;
				SteamGameServerNetworkingUtils.SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None, null);
			}
		}

		public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
		{
			while (true)
			{
				int messageCount = SteamGameServerNetworkingSockets.ReceiveMessagesOnPollGroup(pollGroup, messageAddresses, messageAddresses.Length);
				if (messageCount < 1)
					break;

				IntPtr messageAddress = messageAddresses[0];
				SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messageAddress);

				if (message.m_pData == IntPtr.Zero || message.m_cbSize < 1)
				{
					// Yes, this can actually happen for some reason.
					SteamNetworkingMessage_t.Release(messageAddress);
					DebugLog("Server dropping empty message from {0} (Size: {1})", IdentityToString(ref message), message.m_cbSize);
					continue;
				}

				TransportConnection_SteamNetworkingSockets steamTransportConnection = FindConnection(message.m_conn);
				if (steamTransportConnection == null || steamTransportConnection.wasClosed)
				{
					SteamNetworkingMessage_t.Release(messageAddress);
					DebugLog("Server dropping message from {0}", IdentityToString(ref message));
					continue;
				}

				transportConnection = steamTransportConnection;

				size = message.m_cbSize;
				if (size > buffer.Length)
				{
					size = buffer.Length;
					DebugLog("Server received {0} byte message from {1} (truncated from {2} bytes)", size, IdentityToString(ref message), message.m_cbSize);
				}
				else
				{
					DebugLog("Server received {0} byte message from {1}", size, IdentityToString(ref message));
				}

				Marshal.Copy(message.m_pData, buffer, 0, (int) size);

				SteamNetworkingMessage_t.Release(messageAddress);
				return true;
			}

			size = 0;
			transportConnection = default;
			return false;
		}

		internal void CloseConnection(TransportConnection_SteamNetworkingSockets transportConnection)
		{
			if (transportConnection.wasClosed)
			{
				// Game code may have closed connection before ProblemDetectedLocally.
				return;
			}

			DebugLog("Server closing connection {0}", transportConnection.steamConnectionHandle);
			transportConnection.wasClosed = true;
			const bool bEnableLinger = true;
			bool result = SteamGameServerNetworkingSockets.CloseConnection(transportConnection.steamConnectionHandle, 0, null, bEnableLinger);
			if (!result)
			{
				// Ideally we would log this as a warning, however CloseConnection often returns false despite the docs
				// saying we should be closing the connection on the game side in all cases. My guess is that Steam
				// internally closes the connection when the auth ticket ends, but we still close just in case.
				// Log("Server failed to close connection {0}", transportConnection);
			}
			transportConnections.RemoveFast(transportConnection);
		}

		/// <summary>
		/// Find game connection associated with Steam connection.
		/// </summary>
		private TransportConnection_SteamNetworkingSockets FindConnection(HSteamNetConnection steamConnectionHandle)
		{
			foreach (TransportConnection_SteamNetworkingSockets transportConnection in transportConnections)
			{
				if (transportConnection.steamConnectionHandle == steamConnectionHandle)
				{
					return transportConnection;
				}
			}

			return null;
		}

		private void OnUpdate()
		{
			LogDebugOutput();
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Steam networking sockets transport connections: {transportConnections.Count}");
		}

#pragma warning disable
		private Callback<SteamNetworkingFakeIPResult_t> steamNetworkingFakeIpResultCallback;
#pragma warning restore
		private void OnSteamNetworkingFakeIpResultCallback(SteamNetworkingFakeIPResult_t callback)
		{
			if (callback.m_eResult == EResult.k_EResultOK)
			{
				SDG.Unturned.CommandWindow.Log("//////////////////////////////////////////////////////");

				SDG.Unturned.Local localization = SDG.Unturned.Provider.localization;
				string ipString = new global::Unturned.SystemEx.IPv4Address(callback.m_unIP).ToString();
				string addrString = $"{ipString}:{callback.m_unPorts[0]}";
				SDG.Unturned.CommandWindow.Log(localization.format("FakeIPHeader", addrString));
				SDG.Unturned.CommandWindow.Log(localization.format("FakeIPDetails"));
				SDG.Unturned.CommandWindow.Log(localization.format("FakeIPCopy", "CopyFakeIP"));
				SDG.Unturned.CommandWindow.Log("//////////////////////////////////////////////////////");
			}
			else
			{
				SDG.Unturned.CommandWindow.LogError($"Fatal FakeIP result: {callback.m_eResult}");
				SDG.Unturned.Provider.QuitGame($"fatal fake IP result ({callback.m_eResult})");
			}
		}

#pragma warning disable
		private Callback<SteamNetConnectionStatusChangedCallback_t> steamNetConnectionStatusChanged;
#pragma warning restore
		private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
		{
			DebugLog("Connection with {0} changed state from {1} to {2}", IdentityToString(ref callback), callback.m_eOldState, callback.m_info.m_eState);
			switch (callback.m_info.m_eState)
			{
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None:
					return; // Handle case just to avoid the unknown state logging.

				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
					HandleState_Connecting(ref callback);
					return;

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
					DebugLog("Connection with {0} in unhandled state {1}", IdentityToString(ref callback), callback.m_info.m_eState);
					return;
			}
		}

		private void HandleState_Connecting(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			if (Unturned.Provider.didServerShutdownTimerReachZero)
			{
				DebugLog("Server rejecting {0} because shutdown timer finished", IdentityToString(ref callback));
				const int endReason = 1002;
				bool result = SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, endReason, null, false);
				if (!result)
				{
					Log("Server failed to close connecting connection while shutdown from {0} (End Reason: {1})", IdentityToString(ref callback), endReason);
				}
			}
			else if (Unturned.Provider.hasRoomForNewConnection)
			{
				ulong remoteSteamId = callback.m_info.m_identityRemote.GetSteamID64();
				if (remoteSteamId == 0 || Unturned.Provider.shouldNetIgnoreSteamId(new CSteamID(remoteSteamId)))
				{
					DebugLog("Server rejecting {0} because they've been blocked", IdentityToString(ref callback));
					const int endReason = 1003;
					bool result = SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, endReason, null, false);
					if (!result)
					{
						Log("Server failed to close connecting connection from {0} (blocked) (End Reason: {1})", IdentityToString(ref callback), endReason);
					}
				}
				else
				{
					DebugLog("Assigning {0} to poll group {1}", IdentityToString(ref callback), pollGroup);
					bool setPollGroupResult = SteamGameServerNetworkingSockets.SetConnectionPollGroup(callback.m_hConn, pollGroup);
					if (!setPollGroupResult)
					{
						DebugLog("Server failed to assign {0} to poll group {1}", IdentityToString(ref callback), pollGroup);
					}

					DebugLog("Server accepting connection from {0}", IdentityToString(ref callback));
					EResult result = SteamGameServerNetworkingSockets.AcceptConnection(callback.m_hConn);
					if (result != EResult.k_EResultOK)
					{
						// 2022-06-13 changing from Log to DebugLog because in this issue it looks like bad state
						// is getting spammed (similar to below) https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/3247
						DebugLog("Server error {0} accepting connection from {1}", result, IdentityToString(ref callback));
					}

					// 2022-03-28 received some logs where k_EResultInvalidState was spammed - slowing down the server,
					// so now we explicitly close the connection after a failure to be safe.
					if (!setPollGroupResult || result != EResult.k_EResultOK)
					{
						DebugLog("Server closing connection with {0} after failed connecting attempt", callback.m_hConn);
						SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, false);
					}
				}
			}
			else
			{
				DebugLog("Server rejecting {0} because server is full", IdentityToString(ref callback));
				const int endReason = 1001;
				bool result = SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, endReason, null, false);
				if (!result)
				{
					Log("Server failed to close connecting connection from {0} (End Reason: {1})", IdentityToString(ref callback), endReason);
				}
			}
		}

		private void HandleState_Connected(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			TransportConnection_SteamNetworkingSockets newConnection = new TransportConnection_SteamNetworkingSockets(this, ref callback);
			transportConnections.Add(newConnection);
			DebugLog("Server accepted connection from {0}", IdentityToString(ref callback));
		}

		/// <summary>
		/// Must close the handle to free up resources.
		/// </summary>
		private void HandleState_ClosedByPeer(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			DebugLog("Server connection with {0} closed by peer ({1}) \"{2}\"", IdentityToString(ref callback), callback.m_info.m_eEndReason, callback.m_info.m_szEndDebug);
			TransportConnection_SteamNetworkingSockets existingConnection = FindConnection(callback.m_hConn);
			if (existingConnection != null)
			{
				try
				{
					string debugString = $"ClosedByPeer Reason: {callback.m_info.m_eEndReason} Message: \"{callback.m_info.m_szEndDebug}\"";
					bool isError = (ESteamNetConnectionEnd) callback.m_info.m_eEndReason != ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Generic;
					connectionFailureCallback.Invoke(existingConnection, debugString, isError);
				}
				catch (System.Exception e)
				{
					SDG.Unturned.UnturnedLog.exception(e, "SteamNetworkingSockets caught exception during closed by peer failure callback:");
				}
				existingConnection.CloseConnection(); // In case connectionFailureCallback did not close properly.
			}
			else
			{
				DebugLog("No transport connection associated with handle {0} during ClosedByPeer", callback.m_hConn);
				SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, false);
			}
		}

		/// <summary>
		/// Must close the handle to free up resources.
		/// </summary>
		private void HandleState_ProblemDetectedLocally(ref SteamNetConnectionStatusChangedCallback_t callback)
		{
			DebugLog("Server connection with {0} problem detected locally ({1}) \"{2}\"", IdentityToString(ref callback), callback.m_info.m_eEndReason, callback.m_info.m_szEndDebug);
			TransportConnection_SteamNetworkingSockets existingConnection = FindConnection(callback.m_hConn);
			if (existingConnection != null)
			{
				try
				{
					string debugString = $"ProblemDetectedLocally Reason: {callback.m_info.m_eEndReason} Message: \"{callback.m_info.m_szEndDebug}\"";
					connectionFailureCallback.Invoke(existingConnection, debugString, true);
				}
				catch (System.Exception e)
				{
					SDG.Unturned.UnturnedLog.exception(e, "SteamNetworkingSockets caught exception during problem detected locally failure callback:");
				}
				existingConnection.CloseConnection(); // In case connectionFailureCallback did not close properly.
			}
			else
			{
				DebugLog("No transport connection associated with handle {0} during ProblemDetectedLocally", callback.m_hConn);
				SteamGameServerNetworkingSockets.CloseConnection(callback.m_hConn, 0, null, false);
			}
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
		}

		private ServerTransportConnectionFailureCallback connectionFailureCallback;
		private HSteamListenSocket ipListenSocket = HSteamListenSocket.Invalid;
		private HSteamListenSocket fakeIpListenSocket = HSteamListenSocket.Invalid;
		private HSteamListenSocket p2pListenSocket = HSteamListenSocket.Invalid;
		private HSteamNetPollGroup pollGroup;
		private List<TransportConnection_SteamNetworkingSockets> transportConnections = new List<TransportConnection_SteamNetworkingSockets>();
		private IntPtr[] messageAddresses = new IntPtr[1];
		private bool didSetupDebugOutput;

		/// <summary>
		/// Defaults to true. If false, skip Steam Networking Sockets creation of regular IP socket.
		/// </summary>
		private static SDG.Unturned.CommandLineFlag clUseIpSocket = new SDG.Unturned.CommandLineFlag(true, "-SNS_DisableIPSocket");

		/// <summary>
		/// Defaults to true. If false, skip Steam Networking Sockets creation of non-FakeIP P2P socket.
		/// (this is the socket used by "server codes")
		/// </summary>
		private static SDG.Unturned.CommandLineFlag clUseP2pSocket = new SDG.Unturned.CommandLineFlag(true, "-SNS_DisableP2PSocket");
	}
}
