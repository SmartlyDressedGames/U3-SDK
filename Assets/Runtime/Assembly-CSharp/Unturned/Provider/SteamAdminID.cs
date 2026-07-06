////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class SteamAdminID
	{
		private CSteamID _playerID;
		public CSteamID playerID => _playerID;

		public CSteamID judgeID;

		public SteamAdminID(CSteamID newPlayerID, CSteamID newJudgeID)
		{
			_playerID = newPlayerID;

			judgeID = newJudgeID;
		}
	}
}