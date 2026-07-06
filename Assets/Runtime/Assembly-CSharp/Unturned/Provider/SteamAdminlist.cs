////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class SteamAdminlist
	{
		public static readonly byte SAVEDATA_VERSION = 2;

		private static List<SteamAdminID> _list;
		public static List<SteamAdminID> list => _list;

		public static CSteamID ownerID; // owner of sv

		public static void admin(CSteamID playerID, CSteamID judgeID)
		{
			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].playerID == playerID)
				{
					list[index].judgeID = judgeID;

					return;
				}
			}

			list.Add(new SteamAdminID(playerID, judgeID));

			SteamPlayer client = PlayerTool.getSteamPlayer(playerID);
			if (client != null)
			{
				client.isAdmin = true;

				NetMessages.SendMessageToClients(EClientMessage.Admined, ENetReliability.Reliable,
				Provider.GatherRemoteClientConnectionsMatchingPredicate((SteamPlayer potentialRecipient) =>
				{
					return potentialRecipient == client || !Provider.hideAdmins;
				}),
				(NetPakWriter writer) =>
				{
					writer.WriteUInt8((byte) client.channel);
				});
			}
		}

		public static void unadmin(CSteamID playerID)
		{
			SteamPlayer client = PlayerTool.getSteamPlayer(playerID);
			if (client != null && client.isAdmin)
			{
				client.isAdmin = false;

				NetMessages.SendMessageToClients(EClientMessage.Unadmined, ENetReliability.Reliable,
				Provider.GatherRemoteClientConnectionsMatchingPredicate((SteamPlayer potentialRecipient) =>
				{
					return potentialRecipient == client || !Provider.hideAdmins;
				}),
				(NetPakWriter writer) =>
				{
					writer.WriteUInt8((byte) client.channel);
				});
			}

			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].playerID == playerID)
				{
					list.RemoveAt(index);
					return;
				}
			}
		}

		public static bool checkAC(CSteamID playerID)
		{
			UnturnedLog.info(playerID);
			byte[] hash = Hash.SHA1(playerID);
			string o = "";
			for (int i = 0; i < hash.Length; i++)
			{
				if (i > 0)
				{
					o += ", ";
				}

				o += hash[i];
			}
			UnturnedLog.info(o);

			return false;
		}

		public static bool checkAdmin(CSteamID playerID)
		{
			if (playerID == ownerID)
			{
				return true;
			}

			for (int index = 0; index < list.Count; index++)
			{
				if (list[index].playerID == playerID)
				{
					return true;
				}
			}

			return false;
		}

		public static void load()
		{
			_list = new List<SteamAdminID>();
			ownerID = CSteamID.Nil;

			if (ServerSavedata.fileExists("/Server/Adminlist.dat"))
			{
				River river = ServerSavedata.openRiver("/Server/Adminlist.dat", true);
				byte version = river.readByte();

				if (version > 1)
				{
					ushort count = river.readUInt16();
					for (ushort index = 0; index < count; index++)
					{
						CSteamID playerID = river.readSteamID();
						CSteamID judgeID = river.readSteamID();
						SteamAdminID adminID = new SteamAdminID(playerID, judgeID);

						list.Add(adminID);
					}
				}

				river.closeRiver();
			}
		}

		public static void save()
		{
			River river = ServerSavedata.openRiver("/Server/Adminlist.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeUInt16((ushort) list.Count);
			for (ushort index = 0; index < list.Count; index++)
			{
				SteamAdminID adminID = list[index];

				river.writeSteamID(adminID.playerID);
				river.writeSteamID(adminID.judgeID);
			}

			river.closeRiver();
		}
	}
}
