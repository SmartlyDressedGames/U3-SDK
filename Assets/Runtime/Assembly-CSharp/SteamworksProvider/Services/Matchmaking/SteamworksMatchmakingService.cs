////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Matchmaking;
using Steamworks;
using System.Collections.Generic;

namespace SDG.SteamworksProvider.Services.Matchmaking
{
	public class SteamworksMatchmakingService : Service, IMatchmakingService
	{
		public static List<SteamworksServerInfoRequestHandle> serverInfoRequestHandles;


		//private SteamworksServerInfoRequestHandle findServerInfoRequestHandle(SteamInventoryResult_t steamInventoryResult)
		//{
		//	return steamworksEconomyRequestHandles.Find(handle => handle.steamInventoryResult == steamInventoryResult);
		//}

		public IServerInfoRequestHandle requestServerInfo(uint ip, ushort port, ServerInfoRequestReadyCallback callback)
		{
			SteamworksServerInfoRequestHandle handle = new SteamworksServerInfoRequestHandle(callback);

			ISteamMatchmakingPingResponse pingResponse = new ISteamMatchmakingPingResponse(handle.onServerResponded, handle.onServerFailedToRespond);
			handle.pingResponse = pingResponse;

			HServerQuery query = SteamMatchmakingServers.PingServer(ip, (ushort) (port + 1), pingResponse);
			handle.query = query;

			serverInfoRequestHandles.Add(handle);
			return handle;
		}
		//	  private HServerQuery serverQuery
		//	  {
		//		  get;
		//		  set;
		//	  }

		//	  private ISteamMatchmakingPingResponse steamMatchmakingPingResponse
		//	  {
		//		  get;
		//		  set;
		//	  }

		//	  private ISteamMatchmakingServerListResponse steamMatchmakingServerListResponse
		//	  {
		//		  get;
		//		  set;
		//	  }

		//	  private void cleanupServerQuery()
		//	  {
		//		  if(serverQuery == HServerQuery.Invalid)
		//		  {
		//			  return;
		//		  }

		//		  SteamMatchmakingServers.CancelServerQuery(serverQuery);
		//		  serverQuery = HServerQuery.Invalid;
		//	  }

		//	  public void requestServerInfo(uint ip, ushort port)
		//	  {
		//		  if(serverQuery != HServerQuery.Invalid)
		//		  {
		//			  return;
		//		  }

		//		  serverQuery = SteamMatchmakingServers.PingServer(ip, (ushort) (port + 1), steamMatchmakingPingResponse);
		//	  }

		//	  public void requestServerList(EServerList serverList, List<IMatchmakingFilter> filters)
		//{
		//	MatchMakingKeyValuePair_t[] pairs = new MatchMakingKeyValuePair_t[filters.Count];
		//	for(int index = 0; index < pairs.Length; index++)
		//	{
		//			  IMatchmakingFilter filter = filters[index];
		//		MatchMakingKeyValuePair_t pair = new MatchMakingKeyValuePair_t();
		//		pair.m_szKey = filter.key;
		//		pair.m_szValue = filter.value;

		//		pairs[index] = pair;
		//	}

		//	switch(serverList)
		//	{ 
		//		case EServerList.INTERNET:
		//			SteamMatchmakingServers.RequestInternetServerList(SteamUtils.GetAppID(), pairs, (uint) pairs.Length, this.steamMatchmakingServerListResponse);
		//			return;
		//		case EServerList.LAN:
		//			SteamMatchmakingServers.RequestLANServerList(SteamUtils.GetAppID(), this.steamMatchmakingServerListResponse);
		//			return;
		//		case EServerList.HISTORY:
		//			SteamMatchmakingServers.RequestHistoryServerList(SteamUtils.GetAppID(), pairs, (uint) pairs.Length, this.steamMatchmakingServerListResponse);
		//			return;
		//		case EServerList.FAVORITES:
		//			SteamMatchmakingServers.RequestFavoritesServerList(SteamUtils.GetAppID(), pairs, (uint) pairs.Length, this.steamMatchmakingServerListResponse);
		//			return;
		//		case EServerList.FRIENDS:
		//			SteamMatchmakingServers.RequestFriendsServerList(SteamUtils.GetAppID(), pairs, (uint) pairs.Length, this.steamMatchmakingServerListResponse);
		//			return;
		//	}
		//}

		//	  public string expandIP(uint ip)
		//	  {
		//		  string expanded = ((ip >> 24) & 255) + "." + ((ip >> 16) & 255) + "." + ((ip >> 8) & 255) + "." + (ip & 255);
		//		  return expanded;
		//	  }

		//	  public uint compressIP(string ip)
		//	  {
		//		  string[] components = ip.Split('.');
		//		  if(components.Length != 4)
		//		  {
		//			  throw new System.ArgumentException("IP should be 4 bytes separated by fullstops.", "ip");
		//		  }

		//		  uint compressed = uint.Parse(components[0]) << 24 | uint.Parse(components[1]) << 16 | uint.Parse(components[2]) << 8 | uint.Parse(components[3]);
		//		  return compressed;
		//	  }

		//	  // Found the steamID of a server from the IP:Port
		//	  private void onServerResponded(gameserveritem_t gameServerItem)
		//	  {
		//		  cleanupServerQuery();
		//	  }

		//	  // Failed to find the steamID of a server from the IP:Port
		//	  private void onServerFailedToRespond()
		//	  {
		//		  cleanupServerQuery();
		//	  }

		//	  private void onServerListResponded(HServerListRequest serverListRequest, int index)
		//	  {

		//	  }

		//	  private void onServerListFailedToRespond(HServerListRequest serverListRequest, int index)
		//	  {

		//	  }

		//	  private void onRefreshComplete(HServerListRequest serverListRequest, EMatchMakingServerResponse matchMakingServerResponse)
		//	  {

		//	  }

		//	  public SteamworksMatchmakingService()
		//	  {
		//		  serverQuery = HServerQuery.Invalid;
		//		  steamMatchmakingPingResponse = new ISteamMatchmakingPingResponse(onServerResponded, onServerFailedToRespond);

		//		  steamMatchmakingServerListResponse = new ISteamMatchmakingServerListResponse(onServerListResponded, onServerListFailedToRespond, onRefreshComplete);
		//	  }
	}
}