////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class SteamPublished
	{
		private string _name;
		public string name => _name;

		private PublishedFileId_t _id;
		public PublishedFileId_t id => _id;

		public SteamPublished(string newName, PublishedFileId_t newID)
		{
			_name = newName;
			_id = newID;
		}
	}
}