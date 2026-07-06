////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class SteamContent
	{
		private Steamworks.PublishedFileId_t _publishedFileID;
		public Steamworks.PublishedFileId_t publishedFileID => _publishedFileID;

		private string _path;
		public string path => _path;

		private ESteamUGCType _type;
		public ESteamUGCType type => _type;

		public SteamContent(Steamworks.PublishedFileId_t newPublishedFileID, string newPath, ESteamUGCType newType)
		{
			_publishedFileID = newPublishedFileID;
			_path = newPath;
			_type = newType;
		}
	}
}