////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Community;

namespace SDG.Provider.Services.Multiplayer
{
	public interface IServerInfo
	{
		ICommunityEntity entity
		{
			get;
		}

		string name
		{
			get;
		}

		byte players
		{
			get;
		}

		byte capacity
		{
			get;
		}

		int ping
		{
			get;
		}
	}
}
