////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Matchmaking
{
	public interface IMatchmakingService : IService
	{
		IServerInfoRequestHandle requestServerInfo(uint ip, ushort port, ServerInfoRequestReadyCallback callback);

		//void requestServerList(EServerList serverList, List<IMatchmakingFilter> filters);

		//string expandIP(uint ip);
		//uint compressIP(string ip);
	}
}