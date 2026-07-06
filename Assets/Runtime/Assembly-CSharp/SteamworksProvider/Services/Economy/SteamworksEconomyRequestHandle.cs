////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy
{
	public class SteamworksEconomyRequestHandle : IEconomyRequestHandle
	{
		public SteamInventoryResult_t steamInventoryResult
		{
			get;
			protected set;
		}

		private EconomyRequestReadyCallback economyRequestReadyCallback
		{
			get;
			set;
		}

		public void triggerInventoryRequestReadyCallback(IEconomyRequestResult inventoryRequestResult)
		{
			if (economyRequestReadyCallback == null)
			{
				return;
			}

			economyRequestReadyCallback(this, inventoryRequestResult);
		}

		public SteamworksEconomyRequestHandle(SteamInventoryResult_t newSteamInventoryResult, EconomyRequestReadyCallback newEconomyRequestReadyCallback)
		{
			steamInventoryResult = newSteamInventoryResult;
			economyRequestReadyCallback = newEconomyRequestReadyCallback;
		}
	}
}
