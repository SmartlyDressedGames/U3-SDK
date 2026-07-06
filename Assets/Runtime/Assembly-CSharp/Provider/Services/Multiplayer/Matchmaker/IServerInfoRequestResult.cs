////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Multiplayer;

namespace SDG.Provider.Services.Matchmaking
{
	public interface IServerInfoRequestResult
	{
		IServerInfo serverInfo
		{
			get;
		}
	}
}
