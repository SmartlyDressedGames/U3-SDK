////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy
{
	public class SteamworksEconomyItemDefinition : IEconomyItemDefinition
	{
		public SteamItemDef_t steamItemDef
		{
			get;
			protected set;
		}

		public void readFromStream(NetworkStream networkStream)
		{
			this.steamItemDef = (SteamItemDef_t) networkStream.readInt32();
		}

		public void writeToStream(NetworkStream networkStream)
		{
			networkStream.writeInt32((int) steamItemDef);
		}

		public string getPropertyValue(string key)
		{
			string value;
			uint length = 1024;
			SteamInventory.GetItemDefinitionProperty(steamItemDef, key, out value, ref length);

			return value;
		}

		public SteamworksEconomyItemDefinition(SteamItemDef_t newSteamItemDef)
		{
			steamItemDef = newSteamItemDef;
		}
	}
}
