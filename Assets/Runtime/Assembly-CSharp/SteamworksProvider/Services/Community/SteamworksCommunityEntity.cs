////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Community;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Community
{
	public struct SteamworksCommunityEntity : ICommunityEntity
	{
		public static readonly SteamworksCommunityEntity INVALID = new SteamworksCommunityEntity(CSteamID.Nil);

		public bool isValid => steamID.IsValid();

		public CSteamID steamID;

		public void readFromStream(NetworkStream networkStream)
		{
			this.steamID = (CSteamID) networkStream.readUInt64();
		}

		public void writeToStream(NetworkStream networkStream)
		{
			networkStream.writeUInt64((ulong) steamID);
		}

		public SteamworksCommunityEntity(CSteamID newSteamID)
		{
			this.steamID = newSteamID;
		}
	}
}
