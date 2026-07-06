////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy
{
	public class SteamworksEconomyItem : IEconomyItem
	{
		public SteamItemDetails_t steamItemDetail
		{
			get;
			protected set;
		}

		public IEconomyItemDefinition itemDefinitionID
		{
			get;
			protected set;
		}

		public IEconomyItemInstance itemInstanceID
		{
			get;
			protected set;
		}

		public void readFromStream(NetworkStream networkStream)
		{
			this.itemDefinitionID.readFromStream(networkStream);
			this.itemInstanceID.readFromStream(networkStream);
		}

		public void writeToStream(NetworkStream networkStream)
		{
			this.itemDefinitionID.writeToStream(networkStream);
			this.itemInstanceID.writeToStream(networkStream);
		}

		public SteamworksEconomyItem(SteamItemDetails_t newSteamItemDetail)
		{
			steamItemDetail = newSteamItemDetail;

			itemDefinitionID = new SteamworksEconomyItemDefinition(steamItemDetail.m_iDefinition);
			itemInstanceID = new SteamworksEconomyItemInstance(steamItemDetail.m_itemId);
		}
	}
}
