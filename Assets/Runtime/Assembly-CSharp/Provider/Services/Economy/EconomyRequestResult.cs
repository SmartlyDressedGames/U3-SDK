////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Economy
{
	public class EconomyRequestResult : IEconomyRequestResult
	{
		public EEconomyRequestState economyRequestState
		{
			get;
			protected set;
		}

		public IEconomyItem[] items
		{
			get;
			protected set;
		}

		public EconomyRequestResult(EEconomyRequestState newEconomyRequestState, IEconomyItem[] newItems)
		{
			economyRequestState = newEconomyRequestState;
			items = newItems;
		}
	}
}
