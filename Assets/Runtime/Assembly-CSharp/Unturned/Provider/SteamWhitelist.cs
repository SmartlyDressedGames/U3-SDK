////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class SteamWhitelist
	{
		public static readonly byte SAVEDATA_VERSION = 2;

		private static List<SteamWhitelistID> _list;
		public static List<SteamWhitelistID> list => _list;

		public static void whitelist(CSteamID steamID, string tag, CSteamID judgeID)
		{
			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].steamID == steamID)
				{
					list[index].tag = tag;
					list[index].judgeID = judgeID;

					return;
				}
			}

			list.Add(new SteamWhitelistID(steamID, tag, judgeID));
		}

		public static bool unwhitelist(CSteamID steamID)
		{
			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].steamID == steamID)
				{
					if (Provider.isWhitelisted)
					{
						Provider.kick(steamID, "Removed from whitelist.");
					}

					list.RemoveAt(index);
					return true;
				}
			}

			return false;
		}

		public static bool checkWhitelisted(CSteamID steamID)
		{
			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].steamID == steamID)
				{
					return true;
				}
			}

			return false;
		}

		public static void load()
		{
			_list = new List<SteamWhitelistID>();

			if (ServerSavedata.fileExists("/Server/Whitelist.dat"))
			{
				River river = ServerSavedata.openRiver("/Server/Whitelist.dat", true);
				byte version = river.readByte();

				if (version > 1)
				{
					ushort count = river.readUInt16();
					for (ushort index = 0; index < count; index++)
					{
						CSteamID player = river.readSteamID();
						string tag = river.readString();
						CSteamID judge = river.readSteamID();

						SteamWhitelistID whitelistID = new SteamWhitelistID(player, tag, judge);

						list.Add(whitelistID);
					}
				}

				river.closeRiver();
			}
		}

		public static void save()
		{
			River river = ServerSavedata.openRiver("/Server/Whitelist.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeUInt16((ushort) list.Count);
			for (ushort index = 0; index < list.Count; index++)
			{
				SteamWhitelistID id = list[index];

				river.writeSteamID(id.steamID);
				river.writeString(id.tag);
				river.writeSteamID(id.judgeID);
			}

			river.closeRiver();
		}
	}
}
