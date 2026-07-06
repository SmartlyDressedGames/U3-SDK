////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class SteamBlacklist
	{
		public const byte SAVEDATA_VERSION_ADDED_HWID = 4;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_HWID;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_ADDED_HWID;

		public static readonly uint PERMANENT = 60 * 60 * 24 * 365; // 1 years
		public static readonly uint TEMPORARY = 60 * 3; // 3 minutes

		private static List<SteamBlacklistID> _list;
		public static List<SteamBlacklistID> list => _list;

		[System.Obsolete]
		public static void ban(CSteamID playerID, CSteamID judgeID, string reason, uint duration)
		{
			ban(playerID, 0, judgeID, reason, duration);
		}

		[System.Obsolete("Now accepts list of HWIDs")]
		public static void ban(CSteamID playerID, uint ip, CSteamID judgeID, string reason, uint duration)
		{
			ban(playerID, ip, null, judgeID, reason, duration);
		}

		public static void ban(CSteamID playerID, uint ip, IEnumerable<byte[]> hwids, CSteamID judgeID, string reason, uint duration)
		{
			Provider.ban(playerID, reason, duration);

			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].playerID == playerID)
				{
					list[index].judgeID = judgeID;
					list[index].reason = reason;
					list[index].duration = duration;
					list[index].banned = Provider.time;

					return;
				}
			}

			byte[][] hwidsCopy;
			if (hwids != null)
			{
				List<byte[]> tempHwids = new List<byte[]>(LocalHwid.MAX_HWIDS);
				foreach (byte[] hwid in hwids)
				{
					tempHwids.Add(hwid);
				}
				hwidsCopy = tempHwids.ToArray();
			}
			else
			{
				hwidsCopy = null;
			}

			list.Add(new SteamBlacklistID(playerID, ip, judgeID, reason, duration, Provider.time, hwidsCopy));
		}

		public static bool unban(CSteamID playerID)
		{
			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].playerID == playerID)
				{
					list.RemoveAt(index);
					return true;
				}
			}

			return false;
		}

		[System.Obsolete]
		public static bool checkBanned(CSteamID playerID, out SteamBlacklistID blacklistID)
		{
			return checkBanned(playerID, 0, out blacklistID);
		}

		[System.Obsolete("Now checks HWID")]
		public static bool checkBanned(CSteamID playerID, uint ip, out SteamBlacklistID blacklistID)
		{
			return checkBanned(playerID, ip, null, out blacklistID);
		}

		public static bool checkBanned(CSteamID playerID, uint ip, IEnumerable<byte[]> hwids, out SteamBlacklistID blacklistID)
		{
			blacklistID = null;

			for (int index = list.Count - 1; index >= 0; --index)
			{
				if (list[index].playerID == playerID || (list[index].ip == ip && ip != 0) || list[index].DoesAnyHwidMatch(hwids))
				{
					if (list[index].isExpired)
					{
						list.RemoveAt(index);
						return false;
					}
					else
					{
						blacklistID = list[index];

						return true;
					}
				}
			}

			return false;
		}

		public static void load()
		{
			_list = new List<SteamBlacklistID>();

			if (ServerSavedata.fileExists("/Server/Blacklist.dat"))
			{
				River river = ServerSavedata.openRiver("/Server/Blacklist.dat", true);
				byte version = river.readByte();

				if (version > 1)
				{
					ushort count = river.readUInt16();
					for (ushort index = 0; index < count; index++)
					{
						CSteamID player = river.readSteamID();

						uint ip;
						if (version > 2)
						{
							ip = river.readUInt32();
						}
						else
						{
							ip = 0;
						}

						CSteamID judge = river.readSteamID();

						string reason = river.readString();
						uint duration = river.readUInt32();
						uint banned = river.readUInt32();

						byte[][] hwids;
						if (version >= SAVEDATA_VERSION_ADDED_HWID)
						{
							int hwidCount = river.readInt32();
							if (hwidCount > 0)
							{
								hwids = new byte[hwidCount][];
								for (int hwidIndex = 0; hwidIndex < hwidCount; ++hwidIndex)
								{
									hwids[hwidIndex] = river.readBytes();
								}
							}
							else
							{
								hwids = null;
							}
						}
						else
						{
							hwids = null;
						}

						SteamBlacklistID blacklistID = new SteamBlacklistID(player, ip, judge, reason, duration, banned, hwids);

						if (!blacklistID.isExpired)
						{
							list.Add(blacklistID);
						}
					}
				}

				river.closeRiver();
			}
		}

		public static void save()
		{
			River river = ServerSavedata.openRiver("/Server/Blacklist.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeUInt16((ushort) list.Count);
			for (ushort index = 0; index < list.Count; index++)
			{
				SteamBlacklistID blacklistID = list[index];

				river.writeSteamID(blacklistID.playerID);
				river.writeUInt32(blacklistID.ip);
				river.writeSteamID(blacklistID.judgeID);
				river.writeString(blacklistID.reason);
				river.writeUInt32(blacklistID.duration);
				river.writeUInt32(blacklistID.banned);

				if (blacklistID.hwids == null)
				{
					river.writeInt32(0);
				}
				else
				{
					river.writeInt32(blacklistID.hwids.Length);
					foreach (byte[] hwid in blacklistID.hwids)
					{
						river.writeBytes(hwid);
					}
				}
			}

			river.closeRiver();
		}
	}
}
