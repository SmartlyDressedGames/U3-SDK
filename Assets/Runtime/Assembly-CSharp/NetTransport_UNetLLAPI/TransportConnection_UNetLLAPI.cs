////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_UNETLLAPI
#pragma warning disable CS0618

using UnityEngine.Networking;
using SDG.Unturned;

namespace SDG.NetTransport.UNetLLAPI
{
	internal struct TransportConnection_UNetLLAPI : ITransportConnection
	{
		public TransportConnection_UNetLLAPI(ServerTransport_UNetLLAPI serverTransport, int connectionId)
		{
			this.serverTransport = serverTransport;
			this.connectionId = connectionId;
		}

		public int connectionId;

		public bool TryGetIPv4Address(out uint address)
		{
			address = 0;
			return false;
		}

		public bool TryGetPort(out ushort port)
		{
			port = 0;
			return false;
		}

		public bool Equals(ITransportConnection other)
		{
			return false; // todo
		}

		public void CloseConnection()
		{

		}

		public void Send(byte[] buffer, long size, ESendType sendType)
		{
			byte errorCode;
			NetworkTransport.Send(serverTransport.serverHostId, connectionId, 0, buffer, (int) size, out errorCode);
			NetworkError error = (NetworkError) errorCode;
			if(error != NetworkError.Ok)
			{
				UnturnedLog.error("Server failed to send packet to {0} - Size: {1} Channel: {2}", connectionId, size, 0);
			}
		}

		public ServerTransport_UNetLLAPI serverTransport;
	}
}

#pragma warning restore CS0618
#endif // WITH_UNETLLAPI
