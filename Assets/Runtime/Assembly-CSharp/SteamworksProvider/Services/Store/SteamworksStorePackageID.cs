////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Store;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Store
{
	public class SteamworksStorePackageID : IStorePackageID
	{
		public AppId_t appID
		{
			get;
			protected set;
		}

		public SteamworksStorePackageID(uint appID)
		{
			this.appID = new AppId_t(appID);
		}
	}
}
