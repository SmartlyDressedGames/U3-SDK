////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class SteamWhitelistID
	{
		private CSteamID _steamID;
		public CSteamID steamID => _steamID;

		public string tag;
		public CSteamID judgeID;

		public SteamWhitelistID(CSteamID newSteamID, string newTag, CSteamID newJudgeID)
		{
			_steamID = newSteamID;

			tag = newTag;
			judgeID = newJudgeID;
		}
	}
}