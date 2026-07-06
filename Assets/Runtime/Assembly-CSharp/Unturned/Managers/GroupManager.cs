////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void GroupInfoReadyHandler(GroupInfo group);

	public class QueuedGroupExit
	{
		public CSteamID playerID;
		public CSteamID groupId;
		public float remainingSeconds;
	}

	public class GroupManager : SteamCaller
	{
		public static readonly byte SAVEDATA_VERSION = 3;

		private static GroupManager manager;
		public static GroupManager instance => manager;

		public static event GroupInfoReadyHandler groupInfoReady;

		private static CSteamID availableGroupID;
		private static Dictionary<CSteamID, GroupInfo> knownGroups;
		private static List<QueuedGroupExit> queuedExits;

		public static CSteamID generateUniqueGroupID()
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			CSteamID groupID = availableGroupID;
			availableGroupID.SetAccountID(new AccountID_t(availableGroupID.GetAccountID().m_AccountID + 1));
			return groupID;
		}

		public static GroupInfo addGroup(CSteamID groupID, string name)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			GroupInfo group = new GroupInfo(groupID, name, 0);
			knownGroups.Add(groupID, group);
			return group;
		}

		public static GroupInfo getGroupInfo(CSteamID groupID)
		{
			GroupInfo group = null;
			knownGroups.TryGetValue(groupID, out group);
			return group;
		}

		public static GroupInfo getOrAddGroup(CSteamID groupID, string name, out bool wasCreated)
		{
			wasCreated = false;

			GroupInfo group = getGroupInfo(groupID);
			if (group == null)
			{
				group = addGroup(groupID, name);
				wasCreated = true;
			}

			return group;
		}

		public static void deleteGroup(CSteamID groupID)
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			CancelAllQueuedExitsForGroup(groupID); // Fixes part of public issue #4362.

			// Remove beforehand so that leaveGroup does not trigger sendGroupInfo.
			knownGroups.Remove(groupID);

			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player == null || client.player.quests == null)
				{
					continue;
				}

				if (client.player.quests.isMemberOfGroup(groupID))
				{
					client.player.quests.leaveGroup(true);
				}
			}
		}

		private static void triggerGroupInfoReady(GroupInfo group)
		{
			groupInfoReady?.Invoke(group);
		}

		[System.Obsolete]
		public static void sendGroupInfo(CSteamID steamID, GroupInfo group)
		{
			ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
			if (transportConnection != null)
			{
				sendGroupInfo(transportConnection, group);
			}
		}

		public static void sendGroupInfo(ITransportConnection transportConnection, GroupInfo group)
		{
			SendGroupInfo.Invoke(ENetReliability.Reliable, transportConnection, group.groupID, group.name, group.members);
		}

		public static void sendGroupInfo(List<ITransportConnection> transportConnections, GroupInfo group)
		{
			SendGroupInfo.Invoke(ENetReliability.Reliable, transportConnections, group.groupID, group.name, group.members);
		}

		[System.Obsolete]
		public static void sendGroupInfo(IEnumerable<ITransportConnection> transportConnections, GroupInfo group)
		{
			List<ITransportConnection> list = transportConnections as List<ITransportConnection>;
			if (list != null)
			{
				sendGroupInfo(list, group);
			}
			else
			{
				throw new System.ArgumentException("should be a list", nameof(transportConnections));
			}
		}

		public static void sendGroupInfo(GroupInfo group)
		{
			sendGroupInfo(Provider.GatherRemoteClientConnectionsMatchingPredicate((SteamPlayer client) =>
			{
				return client.player.quests.isMemberOfGroup(group.groupID);
			}), group);
		}

		[System.Obsolete]
		public void tellGroupInfo(CSteamID steamID, CSteamID groupID, string name, uint members)
		{
			ReceiveGroupInfo(groupID, name, members);
		}

		private static readonly ClientStaticMethod<CSteamID, string, uint> SendGroupInfo = ClientStaticMethod<CSteamID, string, uint>.Get(ReceiveGroupInfo);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellGroupInfo))]
		public static void ReceiveGroupInfo(CSteamID groupID, string name, uint members)
		{
			GroupInfo group = getGroupInfo(groupID);
			if (group == null)
			{
				group = new GroupInfo(groupID, name, members);
				knownGroups.Add(group.groupID, group);
			}
			else
			{
				group.name = name;
				group.members = members;
			}
			triggerGroupInfoReady(group);
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				// Hack we use "console user" account ids to avoid clashing with lobby ("chat") or clan ids.
				// Obviously this needs to be rewritten to NOT use steam ids at some point.
				availableGroupID = new CSteamID(new AccountID_t(1), EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeConsoleUser);

				knownGroups = new Dictionary<CSteamID, GroupInfo>();
				queuedExits = new List<QueuedGroupExit>();

				if (Provider.isServer && Level.info != null)
				{
					load();
				}
			}
		}

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onLevelLoaded += onLevelLoaded;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Groups: {knownGroups?.Count}");
			results.Add($"Queued group exits: {queuedExits?.Count}");
		}

		/// <summary>
		/// Is player already waiting to exit their group?
		/// </summary>
		public static bool isPlayerInGroupExitQueue(Player player)
		{
			foreach (QueuedGroupExit exit in queuedExits)
			{
				if (exit.playerID == player.channel.owner.playerID.steamID)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Add player to exit queue if enabled, or immediately remove.
		/// </summary>
		public static void requestGroupExit(Player player)
		{
			uint queueSeconds = Provider.modeConfigData.Gameplay.Timer_Leave_Group;
			if (queueSeconds > 0)
			{
				if (isPlayerInGroupExitQueue(player))
				{
					// Nelson 2023-11-06: requestGroupExit wasn't ignoring requests previously. (public issue #4168)
					return;
				}

				alertGroupmatesTimer(player, queueSeconds);

				QueuedGroupExit exit = new QueuedGroupExit();
				exit.playerID = player.channel.owner.playerID.steamID;
				exit.groupId = player.quests.groupID;
				exit.remainingSeconds = queueSeconds;

				queuedExits.Add(exit);
			}
			else
			{
				alertGroupmatesLeft(player);

				player.quests.leaveGroup();
			}
		}

		/// <summary>
		/// Remove player from queue if they're waiting to exit their group.
		/// </summary>
		public static void cancelGroupExit(Player player)
		{
			for (int index = queuedExits.Count - 1; index >= 0; index--)
			{
				QueuedGroupExit exit = queuedExits[index];
				if (exit.playerID == player.channel.owner.playerID.steamID)
				{
					queuedExits.RemoveAtFast(index);
					return;
				}
			}
		}

		public static void CancelAllQueuedExitsForGroup(CSteamID groupId)
		{
			for (int index = queuedExits.Count - 1; index >= 0; --index)
			{
				QueuedGroupExit exit = queuedExits[index];
				if (exit.groupId == groupId)
				{
					queuedExits.RemoveAtFast(index);
				}
			}
		}

		private static void serverSendMessageToGroupmates(Player player, string message)
		{
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player == null)
					continue;

				if (!client.player.quests.isMemberOfSameGroupAs(player))
					continue;

				ChatManager.serverSendMessage(message, Color.yellow, toPlayer: client);
			}
		}

		private static void alertGroupmatesTimer(Player player, uint remainingSeconds)
		{
			string playerName = player.channel.owner.playerID.playerName;
			string message = Provider.localization.format("Player_Group_Queue_Leave", playerName, remainingSeconds);
			serverSendMessageToGroupmates(player, message);
		}

		private static void alertGroupmatesLeft(Player player)
		{
			string playerName = player.channel.owner.playerID.playerName;
			string message = Provider.localization.format("Player_Group_Left", playerName);
			serverSendMessageToGroupmates(player, message);
		}

		private void tickGroupExitQueue(float deltaTime)
		{
			for (int index = queuedExits.Count - 1; index >= 0; index--)
			{
				QueuedGroupExit exit = queuedExits[index];
				exit.remainingSeconds -= deltaTime;
				if (exit.remainingSeconds > 0)
					continue;

				// Timer finished, remove from queue!
				queuedExits.RemoveAtFast(index);

				Player player = PlayerTool.getPlayer(exit.playerID);
				if (player == null) // They may have left the server.
					continue;

				if (player.quests.groupID != exit.groupId)
				{
					// Their group may have changed without notifying us and canceling the request.
					continue;
				}

				alertGroupmatesLeft(player);

				// Don't force (force=false) in case something changed e.g. entering arena.
				player.quests.leaveGroup();
			}
		}

		private void Update()
		{
			if (Provider.isServer && queuedExits != null && queuedExits.Count > 0)
			{
				tickGroupExitQueue(Time.deltaTime);
			}
		}

		public static void load()
		{
			if (LevelSavedata.fileExists("/Groups.dat"))
			{
				River river = LevelSavedata.openRiver("/Groups.dat", true);
				byte version = river.readByte();

				if (version > 0)
				{
					availableGroupID = river.readSteamID();
					if (version < 3)
					{
						// Hack we use "console user" account ids to avoid clashing with lobby ("chat") or clan ids.
						availableGroupID.SetEUniverse(EUniverse.k_EUniversePublic);
						availableGroupID.SetEAccountType(EAccountType.k_EAccountTypeConsoleUser);
					}

					if (version > 1)
					{
						uint maxAccountId = availableGroupID.GetAccountID().m_AccountID;

						int groupsCount = river.readInt32();
						for (int groupIndex = 0; groupIndex < groupsCount; groupIndex++)
						{
							CSteamID groupID = river.readSteamID();
							string name = river.readString();
							uint members = river.readUInt32();

							if (members < 1 || string.IsNullOrEmpty(name))
							{
								continue;
							}

							if (knownGroups.ContainsKey(groupID))
							{
								continue;
							}

							// Sanity check that next available group ID is not already used. Maybe plugin did something weird.
							maxAccountId = MathfEx.Max(maxAccountId, groupID.GetAccountID().m_AccountID + 1);

							knownGroups.Add(groupID, new GroupInfo(groupID, name, members));
						}

						availableGroupID.SetAccountID(new AccountID_t(maxAccountId));
					}
				}

				river.closeRiver();
			}
		}

		public static void save()
		{
			uint maxAccountId = availableGroupID.GetAccountID().m_AccountID;

			Dictionary<CSteamID, GroupInfo>.ValueCollection groups = knownGroups.Values;

			List<GroupInfo> validGroups = new List<GroupInfo>();
			foreach (GroupInfo group in groups)
			{
				if (group.members < 1 || string.IsNullOrEmpty(group.name))
				{
					continue;
				}

				// Sanity check that next available group ID is not already used. Maybe plugin did something weird.
				maxAccountId = MathfEx.Max(maxAccountId, group.groupID.GetAccountID().m_AccountID + 1);

				validGroups.Add(group);
			}

			availableGroupID.SetAccountID(new AccountID_t(maxAccountId));

			River river = LevelSavedata.openRiver("/Groups.dat", false);
			river.writeByte(SAVEDATA_VERSION);
			river.writeSteamID(availableGroupID);
			river.writeInt32(validGroups.Count);
			foreach (GroupInfo group in validGroups)
			{
				river.writeSteamID(group.groupID);
				river.writeString(group.name);
				river.writeUInt32(group.members);
			}
			river.closeRiver();
		}
	}
}
