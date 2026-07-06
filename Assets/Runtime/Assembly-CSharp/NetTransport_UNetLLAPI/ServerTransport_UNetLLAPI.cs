////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_UNETLLAPI
#pragma warning disable CS0618

using SDG.Unturned;
using UnityEngine.Networking;

namespace SDG.NetTransport.UNetLLAPI
{
	/// <summary>
	/// UNet is deprecated. This implementation exists to ensure the abstraction is solid.
	/// </summary>
	public class ServerTransport_UNetLLAPI : IServerTransport
	{
		public void Initialize(ConnectionFailureHandler connectionClosedCallback)
		{
			NetworkTransport.Init();

			ConnectionConfig defaultConfig = new ConnectionConfig();
			HostTopology topology = new HostTopology(defaultConfig, Unturned.Provider.maxPlayers);
			serverHostId = NetworkTransport.AddHost(topology, Unturned.Provider.port);
			UnturnedLog.info("Hosted LLAPI server on port {0}", Unturned.Provider.port);
		}

		public void TearDown()
		{
			NetworkTransport.Shutdown();
		}

		public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
		{
			int hostId;
			int connectionId;
			int channelId;
			int receivedSize;
			byte error;
			NetworkEventType eventType = NetworkTransport.Receive(out hostId, out connectionId, out channelId, buffer, buffer.Length, out receivedSize, out error);

			switch(eventType)
			{
				default:
				case NetworkEventType.Nothing:
					// The event queue has nothing to report.
					break;
			}

			size = 0;
			transportConnection = default;
			return false;
		}
		
		internal int serverHostId;
	}
}

#pragma warning restore CS0618
#endif // WITH_UNETLLAPI
