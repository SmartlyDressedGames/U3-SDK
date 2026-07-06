////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned; // for steamgroup classes (temp)
using Steamworks;
using UnityEngine;

namespace SDG.Provider.Services.Community
{
	public interface ICommunityService : IService
	{
		void setStatus(string status);

		Texture2D getIcon(int id);
		Texture2D getIcon(CSteamID steamID, bool shouldCache = false);

		SteamGroup getCachedGroup(CSteamID steamID);
		SteamGroup[] getGroups();
		bool checkGroup(CSteamID steamID);
	}
}
