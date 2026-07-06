////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Matchmaking;
using SDG.SteamworksProvider.Services.Multiplayer;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Matchmaking
{
	public class SteamworksServerInfoRequestHandle : IServerInfoRequestHandle
	{
		public HServerQuery query;
		public ISteamMatchmakingPingResponse pingResponse;
		private ServerInfoRequestReadyCallback callback;

		public void onServerResponded(gameserveritem_t server)
		{
			SteamworksServerInfo serverInfo = new SteamworksServerInfo(server);
			SteamworksServerInfoRequestResult result = new SteamworksServerInfoRequestResult(serverInfo);
			triggerCallback(result);

			cleanupQuery();
			SteamworksMatchmakingService.serverInfoRequestHandles.Remove(this);
		}

		public void onServerFailedToRespond()
		{
			SteamworksServerInfoRequestResult result = new SteamworksServerInfoRequestResult(null);
			triggerCallback(result);

			cleanupQuery();
			SteamworksMatchmakingService.serverInfoRequestHandles.Remove(this);
		}

		public void triggerCallback(IServerInfoRequestResult result)
		{
			if (callback == null)
			{
				return;
			}

			callback(this, result);
		}

		private void cleanupQuery()
		{
			SteamMatchmakingServers.CancelServerQuery(query);
			query = HServerQuery.Invalid;
		}

		public SteamworksServerInfoRequestHandle(ServerInfoRequestReadyCallback newCallback)
		{
			callback = newCallback;
		}
	}
}
