////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class SteamBlacklistID
	{
		private CSteamID _playerID;
		public CSteamID playerID => _playerID;

		private uint _ip;
		public uint ip => _ip;

		internal byte[][] hwids;

		public CSteamID judgeID;
		public string reason;
		public uint duration;
		public uint banned;

		public bool isExpired => Provider.time > banned + duration;

		public uint getTime()
		{
			return duration - (Provider.time - banned);
		}

		public bool DoesAnyHwidMatch(System.Collections.Generic.IEnumerable<byte[]> clientHwids)
		{
			if (hwids == null || clientHwids == null)
				return false;

			foreach (byte[] banHwid in hwids)
			{
				foreach (byte[] clientHwid in clientHwids)
				{
					if (Hash.verifyHash(banHwid, clientHwid))
					{
						return true;
					}
				}
			}

			return false;
		}

		public SteamBlacklistID(CSteamID newPlayerID, uint newIP, CSteamID newJudgeID, string newReason, uint newDuration, uint newBanned, byte[][] newHwids)
		{
			_playerID = newPlayerID;
			_ip = newIP;

			judgeID = newJudgeID;
			reason = newReason;
			duration = newDuration;
			banned = newBanned;
			hwids = newHwids;
		}
	}
}
