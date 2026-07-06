////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.SteamworksProvider.Services.Community;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Multiplayer
{
	public class SteamworksServerInfo : IServerInfo
	{
		public ICommunityEntity entity
		{
			get;
			protected set;
		}

		public string name
		{
			get;
			protected set;
		}

		public byte players
		{
			get;
			protected set;
		}

		public byte capacity
		{
			get;
			protected set;
		}

		public int ping
		{
			get;
			protected set;
		}

		public SteamworksServerInfo(gameserveritem_t server)
		{
			entity = new SteamworksCommunityEntity(server.m_steamID);
			name = server.GetServerName();
			players = (byte) server.m_nPlayers;
			capacity = (byte) server.m_nMaxPlayers;
			ping = server.m_nPing;
		}
	}
}
