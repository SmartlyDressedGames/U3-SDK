////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy
{
	public class SteamworksEconomyItemInstance : IEconomyItemInstance
	{
		public SteamItemInstanceID_t steamItemInstanceID
		{
			get;
			protected set;
		}

		public void readFromStream(NetworkStream networkStream)
		{
			this.steamItemInstanceID = (SteamItemInstanceID_t) networkStream.readUInt64();
		}

		public void writeToStream(NetworkStream networkStream)
		{
			networkStream.writeUInt64((ulong) steamItemInstanceID);
		}

		public SteamworksEconomyItemInstance(SteamItemInstanceID_t newSteamItemInstanceID)
		{
			steamItemInstanceID = newSteamItemInstanceID;
		}
	}
}
