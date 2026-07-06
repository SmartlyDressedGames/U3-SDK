////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Matchmaking;
using SDG.Provider.Services.Multiplayer;
using SDG.SteamworksProvider.Services.Multiplayer;

namespace SDG.SteamworksProvider.Services.Matchmaking
{
	public class SteamworksServerInfoRequestResult : IServerInfoRequestResult
	{
		public IServerInfo serverInfo
		{
			get;
			protected set;
		}

		public SteamworksServerInfoRequestResult(SteamworksServerInfo newServerInfo)
		{
			serverInfo = newServerInfo;
		}
	}
}
