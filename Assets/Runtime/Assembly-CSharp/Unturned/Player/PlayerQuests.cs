////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define LOG_NPC_DIALOGUE_RPCS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SDG.Unturned
{
	public delegate void ExternalConditionsUpdated();
	public delegate void FlagsUpdated();
	public delegate void FlagUpdated(ushort id);
	public delegate void TrackedQuestUpdated(PlayerQuests sender);
	public delegate void GroupIDChangedHandler(PlayerQuests sender, CSteamID oldGroupID, CSteamID newGroupID);
	public delegate void GroupRankChangedHandler(PlayerQuests sender, EPlayerGroupRank oldGroupRank, EPlayerGroupRank newGroupRank);
	public delegate void GroupInvitesChangedHandler(PlayerQuests sender);
	public delegate void GroupUpdatedHandler(PlayerQuests sender);
	public delegate void QuestCompletedHandler(PlayerQuests sender, QuestAsset asset);

	public class PlayerQuests : PlayerCaller
	{
		private const byte SAVEDATA_VERSION_ADDED_NPC_SPAWN_ID = 8;
		private const byte SAVEDATA_VERSION_ADDED_TRACKED_QUEST_GUID = 9;
		private const byte SAVEDATA_VERSION_ADDED_QUEST_LIST_GUIDS = 10;
		private const byte SAVEDATA_VERSION_ADDED_NPC_CUTSCENE_MODE = 11;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_NPC_CUTSCENE_MODE;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;
		public static readonly uint DEFAULT_RADIO_FREQUENCY = 460327;

		private static PlayerQuestFlagComparator flagComparator = new PlayerQuestFlagComparator();
		private static PlayerQuestComparator questComparator = new PlayerQuestComparator();

		[System.Obsolete("Replaced by DialogueTarget. Will be removed in a future version!")]
		public InteractableObjectNPC checkNPC;

		private IDialogueTarget _dialogueTarget;
		public IDialogueTarget DialogueTarget
		{
			get => _dialogueTarget;
			private set
			{
				_dialogueTarget = value;
#pragma warning disable
				checkNPC = value as InteractableObjectNPC;
#pragma warning restore
			}
		}

		private DialogueAsset serverCurrentDialogueAsset;
		private VendorAsset serverCurrentVendorAsset;
		private DialogueMessage serverCurrentDialogueMessage;

		/// <summary>
		/// The dialogue to go to when a message has no available responses.
		/// If this is not specified the previous dialogue is used as a default.
		/// </summary>
		private DialogueAsset serverDefaultNextDialogueAsset;

		private Dictionary<ushort, PlayerQuestFlag> flagsMap;

		public ExternalConditionsUpdated onExternalConditionsUpdated;
		public FlagsUpdated onFlagsUpdated;
		public FlagUpdated onFlagUpdated;

		/// <summary>
		/// For level objects with QuestCondition called when quests are added or removed.
		/// </summary>
		internal event System.Action<ushort> OnLocalPlayerQuestsChanged;

		public delegate void AnyFlagChangedHandler(PlayerQuests quests, PlayerQuestFlag flag);
		/// <summary>
		/// Event specifically for plugins to listen to global quest progress.
		/// </summary>
		public static event AnyFlagChangedHandler onAnyFlagChanged;

		public delegate void GroupChangedCallback(PlayerQuests sender, CSteamID oldGroupID, EPlayerGroupRank oldGroupRank, CSteamID newGroupID, EPlayerGroupRank newGroupRank);

		/// <summary>
		/// Event for plugins when group or rank changes.
		/// </summary>
		public static event GroupChangedCallback onGroupChanged;

		private static void broadcastGroupChanged(PlayerQuests sender, CSteamID oldGroupID, EPlayerGroupRank oldGroupRank, CSteamID newGroupID, EPlayerGroupRank newGroupRank)
		{
			try
			{
				onGroupChanged?.Invoke(sender, oldGroupID, oldGroupRank, newGroupID, newGroupRank);
			}
			catch (System.Exception e)
			{
				UnturnedLog.warn("Plugin raised an exception from onGroupChanged:");
				UnturnedLog.exception(e);
			}
		}

		public static GroupUpdatedHandler groupUpdated;
		private static void triggerGroupUpdated(PlayerQuests sender)
		{
			groupUpdated?.Invoke(sender);
		}

		public event TrackedQuestUpdated TrackedQuestUpdated;
		private void TriggerTrackedQuestUpdated()
		{
			if (TrackedQuestUpdated == null)
			{
				return;
			}

			// Ideally the client should not be throwing any exceptions, but this was especially critical because a ReceiveQuests exception
			// was breaking equipment from here.
			try
			{
				TrackedQuestUpdated(this);
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Caught exception during TriggerTrackedQuestUpdated:");
			}
		}

		public event GroupIDChangedHandler groupIDChanged;
		private void triggerGroupIDChanged(CSteamID oldGroupID, CSteamID newGroupID)
		{
			groupIDChanged?.Invoke(this, oldGroupID, newGroupID);
		}

		public event GroupRankChangedHandler groupRankChanged;
		private void triggerGroupRankChanged(EPlayerGroupRank oldGroupRank, EPlayerGroupRank newGroupRank)
		{
			groupRankChanged?.Invoke(this, oldGroupRank, newGroupRank);
		}

		public event GroupInvitesChangedHandler groupInvitesChanged;
		private void triggerGroupInvitesChanged()
		{
			groupInvitesChanged?.Invoke(this);
		}

		public event QuestCompletedHandler questCompleted;
		private void triggerQuestCompleted(QuestAsset asset)
		{
			questCompleted?.Invoke(this, asset);
		}

		public List<PlayerQuestFlag> flagsList
		{
			get;
			private set;
		}

		private QuestAsset _trackedQuest;
		public QuestAsset GetTrackedQuest()
		{
			return _trackedQuest;
		}

		[System.Obsolete("Replaced by GetTrackedQuest")]
		public ushort TrackedQuestID => _trackedQuest?.id ?? 0;

		public List<PlayerQuest> questsList
		{
			get;
			private set;
		}

		private bool _isMarkerPlaced;
		public bool isMarkerPlaced
		{
			get => _isMarkerPlaced;
			private set => _isMarkerPlaced = value;
		}

		private Vector3 _markerPosition;
		public Vector3 markerPosition
		{
			get => _markerPosition;
			private set => _markerPosition = value;
		}

		/// <summary>
		/// Overrides label text next to marker on map.
		/// Used by plugins. Not saved to disk.
		/// </summary>
		private string _markerTextOverride;
		public string markerTextOverride
		{
			get => _markerTextOverride;
			private set => _markerTextOverride = value;
		}

		private uint _radioFrequency;
		public uint radioFrequency
		{
			get => _radioFrequency;
			private set => _radioFrequency = value;
		}

		private CSteamID _groupID;
		public CSteamID groupID
		{
			get => _groupID;
			private set => _groupID = value;
		}

		private EPlayerGroupRank _groupRank;
		public EPlayerGroupRank groupRank
		{
			get => _groupRank;
			private set => _groupRank = value;
		}

		public HashSet<CSteamID> groupInvites
		{
			get;
			private set;
		}

		/// <summary>
		/// Kept serverside. Used to check whether the player is currently in their Steam group or just a normal in-game group.
		/// </summary>
		private bool inMainGroup;

		public bool useMaxGroupMembersLimit => Provider.modeConfigData.Gameplay.Max_Group_Members > 0;

		public bool hasSpaceForMoreMembersInGroup
		{
			get
			{
				if (useMaxGroupMembersLimit)
				{
					GroupInfo group = GroupManager.getGroupInfo(groupID);
					return group != null && group.hasSpaceForMoreMembersInGroup;
				}
				else
				{
					return true;
				}
			}
		}

		/// <summary>
		/// Check before allowing changes to this player's <see cref="groupID"/>
		/// </summary>
		public bool canChangeGroupMembership => !LevelManager.isPlayerInArena(player);

		/// <summary>
		/// Can rename the group.
		/// </summary>
		public bool hasPermissionToChangeName => groupRank == EPlayerGroupRank.OWNER;

		/// <summary>
		/// Can promote and demote members.
		/// </summary>
		public bool hasPermissionToChangeRank => groupRank == EPlayerGroupRank.OWNER;

		public bool hasPermissionToInviteMembers => groupRank == EPlayerGroupRank.ADMIN || groupRank == EPlayerGroupRank.OWNER;

		public bool hasPermissionToKickMembers => groupRank == EPlayerGroupRank.ADMIN || groupRank == EPlayerGroupRank.OWNER;

		public bool hasPermissionToCreateGroup => Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups;

		public bool hasPermissionToLeaveGroup
		{
			get
			{
				if (!Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups)
				{
					return false;
				}

				if (groupRank == EPlayerGroupRank.OWNER)
				{
					GroupInfo group = GroupManager.getGroupInfo(groupID);
					if (group != null && group.members > 1)
					{
						return false; // Prevent owners from leaving groups with members
					}
				}

				return true;
			}
		}

		public bool hasPermissionToDeleteGroup
		{
			get
			{
				if (!Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups)
				{
					return false;
				}

				return !inMainGroup && groupRank == EPlayerGroupRank.OWNER;
			}
		}

		public bool canBeKickedFromGroup => groupRank != EPlayerGroupRank.OWNER;

		public bool isMemberOfAGroup => groupID != CSteamID.Nil;

		/// <summary>
		/// If set, default spawn logic will check for a location node or spawnpoint node matching name.
		/// Saved and loaded between sessions.
		/// </summary>
		public string npcSpawnId;

		private bool npcCutsceneMode;

		/// <summary>
		/// If true, hide viewmodel and prevent using equipped item. For example, to prevent shooting gun on top of a
		/// first-person scene. This could be expanded in the future with other flags and options.
		/// </summary>
		public bool IsCutsceneModeActive()
		{
			return npcCutsceneMode;
		}

		public void ServerSetCutsceneModeActive(bool active)
		{
			if (npcCutsceneMode != active)
			{
				npcCutsceneMode = active;
				if (channel.IsLocalPlayer)
				{
					player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
				}

				SendCutsceneMode.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), active);
			}
		}

		public bool isMemberOfGroup(CSteamID groupID)
		{
			return isMemberOfAGroup && this.groupID == groupID;
		}

		public bool isMemberOfSameGroupAs(Player player)
		{
			return player.quests.isMemberOfGroup(groupID);
		}

		[System.Obsolete]
		public void tellSetMarker(CSteamID steamID, bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride)
		{
			ReceiveMarkerState(newIsMarkerPlaced, newMarkerPosition, newMarkerTextOverride);
		}

		private static readonly ClientInstanceMethod<bool> SendCutsceneMode = ClientInstanceMethod<bool>.Get(typeof(PlayerQuests), nameof(ReceiveCutsceneMode));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveCutsceneMode(bool newCutsceneMode)
		{
			npcCutsceneMode = newCutsceneMode;
			if (channel.IsLocalPlayer)
			{
				player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
			}
		}

		private static readonly ClientInstanceMethod<bool, Vector3, string> SendMarkerState = ClientInstanceMethod<bool, Vector3, string>.Get(typeof(PlayerQuests), nameof(ReceiveMarkerState));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSetMarker))]
		public void ReceiveMarkerState(bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride)
		{
			isMarkerPlaced = newIsMarkerPlaced;
			markerPosition = newMarkerPosition;
			markerTextOverride = newMarkerTextOverride;
		}

		[System.Obsolete]
		public void askSetMarker(CSteamID steamID, bool newIsMarkerPlaced, Vector3 newMarkerPosition)
		{
			ReceiveSetMarkerRequest(newIsMarkerPlaced, newMarkerPosition);
		}

		private static readonly ServerInstanceMethod<bool, Vector3> SendSetMarkerRequest = ServerInstanceMethod<bool, Vector3>.Get(typeof(PlayerQuests), nameof(ReceiveSetMarkerRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5, legacyName = nameof(askSetMarker))]
		public void ReceiveSetMarkerRequest(bool newIsMarkerPlaced, Vector3 newMarkerPosition)
		{
			replicateSetMarker(newIsMarkerPlaced, newMarkerPosition, string.Empty);
		}

		/// <summary>
		/// Called serverside to set marker on clients.
		/// </summary>
		public void replicateSetMarker(bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride = "")
		{
			SendMarkerState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newIsMarkerPlaced, newMarkerPosition, newMarkerTextOverride);
		}

		/// <summary>
		/// Ask server to set marker.
		/// </summary>
		public void sendSetMarker(bool newIsMarkerPlaced, Vector3 newMarkerPosition)
		{
			SendSetMarkerRequest.Invoke(GetNetId(), ENetReliability.Reliable, newIsMarkerPlaced, newMarkerPosition);
		}

		[System.Obsolete]
		public void tellSetRadioFrequency(CSteamID steamID, uint newRadioFrequency)
		{
			ReceiveRadioFrequencyState(newRadioFrequency);
		}

		private static readonly ClientInstanceMethod<uint> SendRadioFrequencyState = ClientInstanceMethod<uint>.Get(typeof(PlayerQuests), nameof(ReceiveRadioFrequencyState));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSetRadioFrequency))]
		public void ReceiveRadioFrequencyState(uint newRadioFrequency)
		{
			radioFrequency = newRadioFrequency;
		}

		[System.Obsolete]
		public void askSetRadioFrequency(CSteamID steamID, uint newRadioFrequency)
		{
			ReceiveSetRadioFrequencyRequest(newRadioFrequency);
		}

		private static readonly ServerInstanceMethod<uint> SendSetRadioFrequencyRequest = ServerInstanceMethod<uint>.Get(typeof(PlayerQuests), nameof(ReceiveSetRadioFrequencyRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5, legacyName = nameof(askSetRadioFrequency))]
		public void ReceiveSetRadioFrequencyRequest(uint newRadioFrequency)
		{
			SendRadioFrequencyState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newRadioFrequency);
		}

		public void sendSetRadioFrequency(uint newRadioFrequency)
		{
			SendSetRadioFrequencyRequest.Invoke(GetNetId(), ENetReliability.Reliable, newRadioFrequency);
		}

		[System.Obsolete]
		public void tellSetGroup(CSteamID steamID, CSteamID newGroupID, byte newGroupRank)
		{
			ReceiveGroupState(newGroupID, (EPlayerGroupRank) newGroupRank);
		}

		private static readonly ClientInstanceMethod<CSteamID, EPlayerGroupRank> SendGroupState = ClientInstanceMethod<CSteamID, EPlayerGroupRank>.Get(typeof(PlayerQuests), nameof(ReceiveGroupState));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSetGroup))]
		public void ReceiveGroupState(CSteamID newGroupID, EPlayerGroupRank newGroupRank)
		{
			CSteamID oldGroupID = groupID;
			groupID = newGroupID;

			EPlayerGroupRank oldGroupRank = groupRank;
			groupRank = newGroupRank;

			if (oldGroupID != newGroupID)
			{
				triggerGroupIDChanged(oldGroupID, newGroupID);
			}

			if (oldGroupRank != groupRank)
			{
				triggerGroupRankChanged(oldGroupRank, groupRank);
			}

			triggerGroupUpdated(this);

			// Event specifically for plugins.
			broadcastGroupChanged(this, oldGroupID, oldGroupRank, newGroupID, groupRank);
		}

		private bool removeGroupInvite(CSteamID newGroupID)
		{
			if (groupInvites.Remove(newGroupID))
			{
				triggerGroupInvitesChanged();
				triggerGroupUpdated(this);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Call serverside to replicate new rank to clients
		/// </summary>
		public void changeRank(EPlayerGroupRank newRank)
		{
			SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), groupID, newRank);
		}

		[System.Obsolete]
		public void askJoinGroupInvite(CSteamID steamID, CSteamID newGroupID)
		{
			ReceiveAcceptGroupInvitationRequest(newGroupID);
		}

		/// <summary>
		/// Set player's group to their Steam group (if any) without testing restrictions.
		/// </summary>
		public void ServerAssignToMainGroup()
		{
			CSteamID newGroupID = channel.owner.playerID.@group;
			inMainGroup = newGroupID != CSteamID.Nil;
			EPlayerGroupRank newRank = EPlayerGroupRank.MEMBER;
			SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newGroupID, newRank);
		}

		public bool ServerAssignToGroup(CSteamID newGroupID, EPlayerGroupRank newRank, bool bypassMemberLimit)
		{
			// Does not handle removal from group (nil) because askJoinGroupInvite calls this.

			GroupInfo group = GroupManager.getGroupInfo(newGroupID);
			if (group != null && (bypassMemberLimit || group.hasSpaceForMoreMembersInGroup))
			{
				SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newGroupID, newRank);
				inMainGroup = false;

				group.members++;
				GroupManager.sendGroupInfo(group);
				return true;
			}
			else
			{
				return false;
			}
		}

		private static readonly ServerInstanceMethod<CSteamID> SendAcceptGroupInvitationRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveAcceptGroupInvitationRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askJoinGroupInvite))]
		public void ReceiveAcceptGroupInvitationRequest(CSteamID newGroupID)
		{
			if (!canChangeGroupMembership)
			{
				return;
			}

			if (newGroupID == channel.owner.playerID.group) // Main group
			{
				if (!Provider.modeConfigData.Gameplay.Allow_Static_Groups)
				{
					return;
				}

				ServerAssignToMainGroup();
			}
			else
			{
				if (!ServerRemoveGroupInvitation(newGroupID))
				{
					// There was no pending invitation to this group.
					return;
				}

				ServerAssignToGroup(newGroupID, EPlayerGroupRank.MEMBER, false);
			}
		}

		public void SendAcceptGroupInvitation(CSteamID newGroupID)
		{
			SendAcceptGroupInvitationRequest.Invoke(GetNetId(), ENetReliability.Reliable, newGroupID);
		}

		[System.Obsolete]
		public void askIgnoreGroupInvite(CSteamID steamID, CSteamID newGroupID)
		{
			ReceiveDeclineGroupInvitationRequest(newGroupID);
		}

		private static readonly ServerInstanceMethod<CSteamID> SendDeclineGroupInvitationRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveDeclineGroupInvitationRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askJoinGroupInvite))]
		public void ReceiveDeclineGroupInvitationRequest(CSteamID newGroupID)
		{
			ServerRemoveGroupInvitation(newGroupID);
		}

		public void SendDeclineGroupInvitation(CSteamID newGroupID)
		{
			SendDeclineGroupInvitationRequest.Invoke(GetNetId(), ENetReliability.Reliable, newGroupID);
		}

		/// <param name="force">Ignores group changing rules when true.</param>
		public void leaveGroup(bool force = false)
		{
			if (!force)
			{
				if (!canChangeGroupMembership)
				{
					return;
				}

				if (!hasPermissionToLeaveGroup)
				{
					return;
				}
			}

			// Group info may be null when leaving after group is deleted.
			GroupInfo group = GroupManager.getGroupInfo(groupID);
			if (group != null)
			{
				if (group.members > 0)
				{
					group.members--;
				}

				GroupManager.sendGroupInfo(group);
			}

			SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), CSteamID.Nil, EPlayerGroupRank.MEMBER);
			inMainGroup = false;
		}

		private float lastLeaveGroupRequestRealtime;

		[System.Obsolete]
		public void askLeaveGroup(CSteamID steamID)
		{
			ReceiveLeaveGroupRequest();
		}

		private static readonly ServerInstanceMethod SendLeaveGroupRequest = ServerInstanceMethod.Get(typeof(PlayerQuests), nameof(ReceiveLeaveGroupRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askLeaveGroup))]
		public void ReceiveLeaveGroupRequest()
		{
			// Prevent spamming leaving group messages.
			if (Time.realtimeSinceStartup - lastLeaveGroupRequestRealtime < 5)
				return;
			lastLeaveGroupRequestRealtime = Time.realtimeSinceStartup;

			GroupManager.requestGroupExit(player);
		}

		public void sendLeaveGroup()
		{
			SendLeaveGroupRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
		}

		public void deleteGroup()
		{
			if (!canChangeGroupMembership)
			{
				return;
			}

			if (!hasPermissionToDeleteGroup)
			{
				return;
			}

			GroupManager.deleteGroup(groupID);
		}

		[System.Obsolete]
		public void askDeleteGroup(CSteamID steamID)
		{
			ReceiveDeleteGroupRequest();
		}

		private static readonly ServerInstanceMethod SendDeleteGroupRequest = ServerInstanceMethod.Get(typeof(PlayerQuests), nameof(ReceiveDeleteGroupRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askDeleteGroup))]
		public void ReceiveDeleteGroupRequest()
		{
			deleteGroup();
		}

		public void sendDeleteGroup()
		{
			SendDeleteGroupRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
		}

		[System.Obsolete]
		public void askCreateGroup(CSteamID steamID)
		{
			ReceiveCreateGroupRequest();
		}

		private static readonly ServerInstanceMethod SendCreateGroupRequest = ServerInstanceMethod.Get(typeof(PlayerQuests), nameof(ReceiveCreateGroupRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askCreateGroup))]
		public void ReceiveCreateGroupRequest()
		{
			if (!canChangeGroupMembership)
			{
				return;
			}

			if (!hasPermissionToCreateGroup)
			{
				return;
			}

			CSteamID newGroupID = GroupManager.generateUniqueGroupID();

			GroupInfo group = GroupManager.addGroup(newGroupID, channel.owner.playerID.playerName + "'s Group");
			group.members++;
			GroupManager.sendGroupInfo(channel.GetOwnerTransportConnection(), group);

			SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newGroupID, EPlayerGroupRank.OWNER);
			inMainGroup = false;
		}

		public void sendCreateGroup()
		{
			SendCreateGroupRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
		}

		private void addGroupInvite(CSteamID newGroupID)
		{
			groupInvites.Add(newGroupID);

			triggerGroupInvitesChanged();
			triggerGroupUpdated(this);
		}

		[System.Obsolete]
		public void tellAddGroupInvite(CSteamID steamID, CSteamID newGroupID)
		{
			ReceiveAddGroupInviteClient(newGroupID);
		}

		private static readonly ClientInstanceMethod<CSteamID> SendAddGroupInviteClient = ClientInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveAddGroupInviteClient));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellAddGroupInvite))]
		public void ReceiveAddGroupInviteClient(CSteamID newGroupID)
		{
			addGroupInvite(newGroupID);
		}

		private static readonly ClientInstanceMethod<CSteamID> SendRemoveGroupInviteClient = ClientInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveRemoveGroupInviteClient));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveRemoveGroupInviteClient(CSteamID newGroupID)
		{
			removeGroupInvite(newGroupID);
		}

		public bool ServerRemoveGroupInvitation(CSteamID groupId)
		{
			if (!removeGroupInvite(groupId))
			{
				// There was no pending invitation to this group.
				return false;
			}

			if (!channel.IsLocalPlayer)
			{
				SendRemoveGroupInviteClient.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), groupId);
			}

			return true;
		}

		/// <summary>
		/// Serverside send packet telling player about this invite
		/// </summary>
		public void sendAddGroupInvite(CSteamID newGroupID)
		{
			if (groupInvites.Contains(newGroupID))
			{
				return;
			}

			addGroupInvite(newGroupID);

			GroupInfo group = GroupManager.getGroupInfo(newGroupID);
			if (group != null)
			{
				GroupManager.sendGroupInfo(channel.GetOwnerTransportConnection(), group);
			}

			if (!channel.IsLocalPlayer)
			{
				SendAddGroupInviteClient.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), newGroupID);
			}
		}

		[System.Obsolete]
		public void askAddGroupInvite(CSteamID steamID, CSteamID targetID)
		{
			ReceiveAddGroupInviteRequest(targetID);
		}

		private static readonly ServerInstanceMethod<CSteamID> SendAddGroupInviteRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveAddGroupInviteRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askAddGroupInvite))]
		public void ReceiveAddGroupInviteRequest(CSteamID targetID)
		{
			if (!isMemberOfAGroup)
			{
				return;
			}

			if (!hasPermissionToInviteMembers)
			{
				return;
			}

			if (!hasSpaceForMoreMembersInGroup)
			{
				return;
			}

			Player target = PlayerTool.getPlayer(targetID);
			if (target == null)
			{
				return;
			}

			if (target.quests.isMemberOfAGroup)
			{
				return;
			}

			target.quests.sendAddGroupInvite(groupID);
		}

		public void sendAskAddGroupInvite(CSteamID targetID)
		{
			SendAddGroupInviteRequest.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
		}

		[System.Obsolete]
		public void askPromote(CSteamID steamID, CSteamID targetID)
		{
			ReceivePromoteRequest(targetID);
		}

		private static readonly ServerInstanceMethod<CSteamID> SendPromoteRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceivePromoteRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askPromote))]
		public void ReceivePromoteRequest(CSteamID targetID)
		{
			if (!isMemberOfAGroup)
			{
				return;
			}

			if (!hasPermissionToChangeRank)
			{
				return;
			}

			Player target = PlayerTool.getPlayer(targetID);
			if (target == null)
			{
				return;
			}

			if (!target.quests.isMemberOfSameGroupAs(player))
			{
				return;
			}

			if (target.quests.groupRank == EPlayerGroupRank.OWNER)
			{
				CommandWindow.LogWarning("Request to promote owner of group?");
				return;
			}

			target.quests.changeRank(target.quests.groupRank + 1);

			if (target.quests.groupRank == EPlayerGroupRank.OWNER) // We promoted them to owner
			{
				changeRank(EPlayerGroupRank.ADMIN); // So demote ourselves because there can only be one owner
			}
		}

		public void sendPromote(CSteamID targetID)
		{
			SendPromoteRequest.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
		}

		[System.Obsolete]
		public void askDemote(CSteamID steamID, CSteamID targetID)
		{
			ReceiveDemoteRequest(targetID);
		}

		private static readonly ServerInstanceMethod<CSteamID> SendDemoteRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveDemoteRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askDemote))]
		public void ReceiveDemoteRequest(CSteamID targetID)
		{
			if (!isMemberOfAGroup)
			{
				return;
			}

			if (!hasPermissionToChangeRank)
			{
				return;
			}

			Player target = PlayerTool.getPlayer(targetID);
			if (target == null)
			{
				return;
			}

			if (!target.quests.isMemberOfSameGroupAs(player))
			{
				return;
			}

			if (target.quests.groupRank != EPlayerGroupRank.ADMIN)
			{
				CommandWindow.LogWarning("Request to demote non-admin member of group?");
				return;
			}

			target.quests.changeRank(target.quests.groupRank - 1);
		}

		public void sendDemote(CSteamID targetID)
		{
			SendDemoteRequest.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
		}

		[System.Obsolete]
		public void askKickFromGroup(CSteamID steamID, CSteamID targetID)
		{
			ReceiveKickFromGroup(targetID);
		}

		private static readonly ServerInstanceMethod<CSteamID> SendKickFromGroup = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), nameof(ReceiveKickFromGroup));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askKickFromGroup))]
		public void ReceiveKickFromGroup(CSteamID targetID)
		{
			if (!isMemberOfAGroup)
			{
				return;
			}

			if (!hasPermissionToKickMembers)
			{
				return;
			}

			Player target = PlayerTool.getPlayer(targetID);
			if (target == null)
			{
				return;
			}

			if (!target.quests.isMemberOfSameGroupAs(player))
			{
				return;
			}

			if (!target.quests.canBeKickedFromGroup)
			{
				return;
			}

			target.quests.leaveGroup();
		}

		public void sendKickFromGroup(CSteamID targetID)
		{
			SendKickFromGroup.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
		}

		[System.Obsolete]
		public void askRenameGroup(CSteamID steamID, string newName)
		{
			ReceiveRenameGroupRequest(newName);
		}

		private static readonly ServerInstanceMethod<string> SendRenameGroupRequest = ServerInstanceMethod<string>.Get(typeof(PlayerQuests), nameof(ReceiveRenameGroupRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askRenameGroup))]
		public void ReceiveRenameGroupRequest(string newName)
		{
			if (newName.Length > 32)
			{
				newName = newName.Substring(0, 32);
			}

			if (!isMemberOfAGroup)
			{
				return;
			}

			if (!hasPermissionToChangeName)
			{
				return;
			}

			GroupInfo group = GroupManager.getGroupInfo(groupID);
			group.name = newName;
			GroupManager.sendGroupInfo(group);
		}

		public void sendRenameGroup(string newName)
		{
			SendRenameGroupRequest.Invoke(GetNetId(), ENetReliability.Reliable, newName);
		}

		public void setFlag(ushort id, short value)
		{
			PlayerQuestFlag flag;
			if (flagsMap.TryGetValue(id, out flag))
			{
				flag.value = value;
			}
			else
			{
				flag = new PlayerQuestFlag(id, value);
				flagsMap.Add(id, flag);

				int index = flagsList.BinarySearch(flag, flagComparator);
				index = ~index;
				flagsList.Insert(index, flag);
			}

			if (channel.IsLocalPlayer)
			{
				// Move into quest delegate
				if (id == 29)
				{
					if (value >= 1)
					{
						bool data;
						if (Provider.provider.achievementsService.getAchievement("Ensign", out data) && !data)
						{
							Provider.provider.achievementsService.setAchievement("Ensign");
						}
					}

					if (value >= 2)
					{
						bool data;
						if (Provider.provider.achievementsService.getAchievement("Lieutenant", out data) && !data)
						{
							Provider.provider.achievementsService.setAchievement("Lieutenant");
						}
					}

					if (value >= 3)
					{
						bool data;
						if (Provider.provider.achievementsService.getAchievement("Major", out data) && !data)
						{
							Provider.provider.achievementsService.setAchievement("Major");
						}
					}
				}

				onFlagUpdated?.Invoke(id);

				TriggerTrackedQuestUpdated();
			}

			if (Provider.isServer && onAnyFlagChanged != null)
			{
				onAnyFlagChanged.Invoke(this, flag);
			}
		}

		public bool getFlag(ushort id, out short value)
		{
			PlayerQuestFlag flag;
			if (flagsMap.TryGetValue(id, out flag))
			{
				value = flag.value;
				return true;
			}
			else
			{
				value = 0;
				return false;
			}
		}

		public void removeFlag(ushort id)
		{
			PlayerQuestFlag flag;
			if (flagsMap.TryGetValue(id, out flag))
			{
				int index = flagsList.BinarySearch(flag, flagComparator);

				if (index >= 0)
				{
					flagsMap.Remove(id);
					flagsList.RemoveAt(index);

					if (channel.IsLocalPlayer)
					{
						onFlagUpdated?.Invoke(id);

						TriggerTrackedQuestUpdated();
					}
				}
			}
		}

		public int countValidQuests()
		{
			int count = 0;
			foreach (PlayerQuest quest in questsList)
			{
				if (quest == null || quest.asset == null)
					continue;

				++count;
			}

			return count;
		}

		public void AddQuest(QuestAsset questAsset)
		{
			if (questAsset == null)
				return;

			int index = FindIndexOfQuest(questAsset);
			if (index < 0)
			{
				PlayerQuest quest = new PlayerQuest(questAsset);
				questsList.Add(quest);
			}

			TrackQuest(questAsset);

			if (channel.IsLocalPlayer && OnLocalPlayerQuestsChanged != null)
			{
				OnLocalPlayerQuestsChanged.Invoke(questAsset.id);
			}
		}

		[System.Obsolete]
		public void addQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				AddQuest(questAsset);
			}
		}

		[System.Obsolete]
		public bool getQuest(ushort id, out PlayerQuest quest)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				int index = FindIndexOfQuest(questAsset);
				if (index >= 0)
				{
					quest = questsList[index];
					return true;
				}
				else
				{
					quest = null;
					return false;
				}
			}
			else
			{
				quest = null;
				return false;
			}
		}

		public ENPCQuestStatus GetQuestStatus(QuestAsset questAsset)
		{
			if (questAsset == null)
			{
				return ENPCQuestStatus.NONE;
			}

			int index = FindIndexOfQuest(questAsset);
			if (index >= 0) // currently have it
			{
				if (questAsset.areConditionsMet(player))
				{
					return ENPCQuestStatus.READY; // ready to turn in
				}
				else
				{
					return ENPCQuestStatus.ACTIVE; // working on it
				}
			}
			else
			{
				short flag;
				if (getFlag(questAsset.id, out flag)) // already did it
				{
					return ENPCQuestStatus.COMPLETED;
				}
				else
				{
					return ENPCQuestStatus.NONE; // don't have it, didn't have it, so no quest status
				}
			}
		}

		[System.Obsolete]
		public ENPCQuestStatus getQuestStatus(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				return GetQuestStatus(questAsset);
			}
			else
			{
				return ENPCQuestStatus.NONE;
			}
		}

		public void RemoveQuest(QuestAsset questAsset, bool wasCompleted = false)
		{
			int questIndex = FindIndexOfQuest(questAsset);
			if (questIndex >= 0)
			{
				questsList.RemoveAt(questIndex);
			}

			if (_trackedQuest != null && _trackedQuest == questAsset)
			{
				if (questsList.Count > 0)
				{
					TrackQuest(questsList[0].asset);
				}
				else
				{
					TrackQuest(null);
				}
			}

			if (channel.IsLocalPlayer && questAsset != null)
			{
				if (wasCompleted)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Quest", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Quest");
					}
				}

				OnLocalPlayerQuestsChanged?.Invoke(questAsset.id);
			}

			if (questAsset != null && wasCompleted)
			{
				triggerQuestCompleted(questAsset);
			}
		}

		[System.Obsolete]
		public void removeQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				RemoveQuest(questAsset);
			}
		}

		public void trackHordeKill()
		{
			for (int questIndex = 0; questIndex < questsList.Count; questIndex++)
			{
				PlayerQuest quest = questsList[questIndex];

				if (quest == null || quest.asset == null || quest.asset.conditions == null)
				{
					continue;
				}

				for (int conditionIndex = 0; conditionIndex < quest.asset.conditions.Length; conditionIndex++)
				{
					NPCHordeKillsCondition condition = quest.asset.conditions[conditionIndex] as NPCHordeKillsCondition;

					if (condition == null)
					{
						continue;
					}

					if (condition.nav == player.movement.nav)
					{
						short flag;
						getFlag(condition.id, out flag);
						flag++;
						sendSetFlag(condition.id, flag);
					}
				}
			}
		}

		public void trackZombieKill(Zombie zombie)
		{
			if (zombie == null)
			{
				return;
			}

			float sqrDistance = (transform.position - zombie.transform.position).sqrMagnitude;

			for (int questIndex = 0; questIndex < questsList.Count; questIndex++)
			{
				PlayerQuest quest = questsList[questIndex];

				if (quest == null || quest.asset == null || quest.asset.conditions == null)
				{
					continue;
				}

				for (int conditionIndex = 0; conditionIndex < quest.asset.conditions.Length; conditionIndex++)
				{
					NPCZombieKillsCondition condition = quest.asset.conditions[conditionIndex] as NPCZombieKillsCondition;

					if (condition == null)
					{
						continue;
					}

					if (condition.zombie != EZombieSpeciality.NONE && condition.zombie != zombie.speciality)
					{
						// Condition requires a specific zombie type.
						continue;
					}

					if (condition.nav != byte.MaxValue && condition.nav != player.movement.bound)
					{
						// Condition requires zombie to be in specific navmesh.
						continue;
					}

					if (condition.sqrRadius > 0.01f && sqrDistance > condition.sqrRadius)
					{
						// Too far away from the zombie.
						continue;
					}

					if (condition.sqrMinRadius > 0.01f && sqrDistance < condition.sqrMinRadius)
					{
						// Too close to the zombie.
						continue;
					}

					short flag;
					getFlag(condition.id, out flag);
					flag++;
					sendSetFlag(condition.id, flag);
				}
			}
		}

		public void trackObjectKill(System.Guid objectGuid, byte nav)
		{
			foreach (PlayerQuest quest in questsList)
			{
				if (quest == null || quest.asset == null || quest.asset.conditions == null)
					continue;

				foreach (INPCCondition condition in quest.asset.conditions)
				{
					NPCObjectKillsCondition objectCondition = condition as NPCObjectKillsCondition;
					if (objectCondition == null)
						continue;

					if (objectCondition.nav == byte.MaxValue || objectCondition.nav == nav)
					{
						if (objectCondition.objectGuid.Equals(objectGuid))
						{
							short flag;
							getFlag(objectCondition.id, out flag);
							flag++;
							sendSetFlag(objectCondition.id, flag);
						}
					}
				}
			}
		}

		public void trackTreeKill(System.Guid treeGuid)
		{
			foreach (PlayerQuest quest in questsList)
			{
				if (quest == null || quest.asset == null || quest.asset.conditions == null)
					continue;

				foreach (INPCCondition condition in quest.asset.conditions)
				{
					NPCTreeKillsCondition treeCondition = condition as NPCTreeKillsCondition;
					if (treeCondition == null)
						continue;

					if (treeCondition.treeGuid.Equals(treeGuid))
					{
						short flag;
						getFlag(treeCondition.id, out flag);
						flag++;
						sendSetFlag(treeCondition.id, flag);
					}
				}
			}
		}

		public void trackAnimalKill(Animal animal)
		{
			if (animal == null)
			{
				return;
			}

			for (int questIndex = 0; questIndex < questsList.Count; questIndex++)
			{
				PlayerQuest quest = questsList[questIndex];

				if (quest == null || quest.asset == null || quest.asset.conditions == null)
				{
					continue;
				}

				for (int conditionIndex = 0; conditionIndex < quest.asset.conditions.Length; conditionIndex++)
				{
					NPCAnimalKillsCondition condition = quest.asset.conditions[conditionIndex] as NPCAnimalKillsCondition;

					if (condition == null)
					{
						continue;
					}

					if (condition.animal == animal.id)
					{
						short flag;
						getFlag(condition.id, out flag);
						flag++;
						sendSetFlag(condition.id, flag);
					}
				}
			}
		}

		public void trackPlayerKill(Player enemyPlayer)
		{
			if (enemyPlayer == null)
			{
				return;
			}

			for (int questIndex = 0; questIndex < questsList.Count; questIndex++)
			{
				PlayerQuest quest = questsList[questIndex];

				if (quest == null || quest.asset == null || quest.asset.conditions == null)
				{
					continue;
				}

				for (int conditionIndex = 0; conditionIndex < quest.asset.conditions.Length; conditionIndex++)
				{
					NPCPlayerKillsCondition condition = quest.asset.conditions[conditionIndex] as NPCPlayerKillsCondition;

					if (condition == null)
					{
						continue;
					}

					// In the future we could compare reputation or something.
					short flag;
					getFlag(condition.id, out flag);
					flag++;
					sendSetFlag(condition.id, flag);
				}
			}
		}

		/// <summary>
		/// Called on server to finalize and remove quest.
		/// </summary>
		public void CompleteQuest(QuestAsset questAsset, bool ignoreNPC = false)
		{
			if (questAsset == null)
				return;

			if (!ignoreNPC)
			{
				if (DialogueTarget == null)
				{
					return;
				}

				if ((DialogueTarget.GetDialogueTargetWorldPosition() - transform.position).sqrMagnitude > 400)
				{
					return;
				}
			}

			if (GetQuestStatus(questAsset) != ENPCQuestStatus.READY)
			{
				return;
			}

			ServerRemoveQuest(questAsset, wasCompleted: true);
			sendSetFlag(questAsset.id, 1);

			questAsset.ApplyConditions(player);
			questAsset.GrantRewards(player);
		}

		[System.Obsolete]
		public void completeQuest(ushort id, bool ignoreNPC = false)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				CompleteQuest(questAsset);
			}
		}

		[System.Obsolete]
		public void askSellToVendor(CSteamID steamID, ushort id, byte index)
		{
			throw new System.NotSupportedException();
		}

		private static readonly ServerInstanceMethod<System.Guid, byte, bool> SendSellToVendor = ServerInstanceMethod<System.Guid, byte, bool>.Get(typeof(PlayerQuests), nameof(ReceiveSellToVendor));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askSellToVendor))]
		public void ReceiveSellToVendor(in ServerInvocationContext context, System.Guid assetGuid, byte index, bool asManyAsPossible)
		{
			if (DialogueTarget == null)
			{
				context.LogWarning("null NPC");
				return;
			}

			if ((DialogueTarget.GetDialogueTargetWorldPosition() - transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("NPC out of range");
				return;
			}

			if (serverCurrentVendorAsset == null)
			{
				context.LogWarning("no current vendor");
				return;
			}

			VendorAsset asset = Assets.find<VendorAsset>(assetGuid);
			if (asset == null)
			{
				context.LogWarning("null asset");
				return;
			}

			if (asset != serverCurrentVendorAsset)
			{
				context.LogWarning($"selling index {index} for vendor {asset.name} does not match server current vendor {serverCurrentVendorAsset.name}");
				return;
			}

			if (asset.buying == null)
			{
				context.LogWarning("null buying array");
				return;
			}

			if (index >= asset.buying.Length)
			{
				context.LogWarning($"index ({index}) out of bounds ({asset.buying.Length})");
				return;
			}

			VendorBuying buying = asset.buying[index];
			if (buying == null)
			{
				context.LogWarning("null entry");
				return;
			}

			int loopCount = 0; // Some mods have infinite loops which can crash the server.
			do
			{
				if (!buying.canSell(player) || !buying.areConditionsMet(player))
				{
					return;
				}

				buying.ApplyConditions(player);
				buying.GrantRewards(player);
				buying.sell(player);

				++loopCount;
			}
			while (asManyAsPossible && loopCount < 10);
		}

		public void sendSellToVendor(System.Guid assetGuid, byte index, bool asManyAsPossible)
		{
			SendSellToVendor.Invoke(GetNetId(), ENetReliability.Unreliable, assetGuid, index, asManyAsPossible);
		}

		[System.Obsolete]
		public void sendSellToVendor(ushort id, byte index)
		{
			throw new System.NotSupportedException();
		}

		[System.Obsolete]
		public void askBuyFromVendor(CSteamID steamID, ushort id, byte index)
		{
			throw new System.NotSupportedException();
		}

		private static readonly ServerInstanceMethod<System.Guid, byte, bool> SendBuyFromVendor = ServerInstanceMethod<System.Guid, byte, bool>.Get(typeof(PlayerQuests), nameof(ReceiveBuyFromVendor));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askBuyFromVendor))]
		public void ReceiveBuyFromVendor(in ServerInvocationContext context, System.Guid assetGuid, byte index, bool asManyAsPossible)
		{
			if (DialogueTarget == null)
			{
				context.LogWarning("null NPC");
				return;
			}

			if ((DialogueTarget.GetDialogueTargetWorldPosition() - transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("NPC out of range");
				return;
			}

			if (serverCurrentVendorAsset == null)
			{
				context.LogWarning("no current vendor");
				return;
			}

			VendorAsset asset = Assets.find<VendorAsset>(assetGuid);
			if (asset == null)
			{
				context.LogWarning("null asset");
				return;
			}

			if (asset != serverCurrentVendorAsset)
			{
				context.LogWarning($"buying index {index} for vendor {asset.name} does not match server current vendor {serverCurrentVendorAsset.name}");
				return;
			}

			if (asset.selling == null)
			{
				context.LogWarning("null selling array");
				return;
			}

			if (index >= asset.selling.Length)
			{
				context.LogWarning($"index ({index}) out of bounds ({asset.selling.Length})");
				return;
			}

			VendorSellingBase selling = asset.selling[index];
			if (selling == null)
			{
				context.LogWarning("null entry");
				return;
			}

			if (selling is VendorSellingVehicle)
			{
				// This is a bit messy, but we do not want huge pileups of vehicles, and some hosts have complained about
				// players with lots of money spawning hundreds of vehicles all at once.
				asManyAsPossible = false;

				if (Time.realtimeSinceStartup - lastVehiclePurchaseRealtime < 5.0f)
				{
					lastVehiclePurchaseRealtime = Time.realtimeSinceStartup;
					context.LogWarning("vehicle purchase cooldown");
					return;
				}
			}

			int loopCount = 0; // Some mods have infinite loops which can crash the server.
			do
			{
				if (!selling.canBuy(player) || !selling.areConditionsMet(player))
				{
					return;
				}

				selling.ApplyConditions(player);
				selling.GrantRewards(player);
				selling.buy(player);

				++loopCount;
			}
			while (asManyAsPossible && loopCount < 10);
		}

		public void sendBuyFromVendor(System.Guid assetGuid, byte index, bool asManyAsPossible)
		{
			SendBuyFromVendor.Invoke(GetNetId(), ENetReliability.Unreliable, assetGuid, index, asManyAsPossible);
		}

		[System.Obsolete]
		public void sendBuyFromVendor(ushort id, byte index)
		{
			throw new System.NotSupportedException();
		}

		[System.Obsolete]
		public void tellSetFlag(CSteamID steamID, ushort id, short value)
		{
			ReceiveSetFlag(id, value);
		}

		private static readonly ClientInstanceMethod<ushort, short> SendSetFlag = ClientInstanceMethod<ushort, short>.Get(typeof(PlayerQuests), nameof(ReceiveSetFlag));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSetFlag))]
		public void ReceiveSetFlag(ushort id, short value)
		{
			setFlag(id, value);
		}

		public void sendSetFlag(ushort id, short value)
		{
			setFlag(id, value);

			if (!channel.IsLocalPlayer)
			{
				SendSetFlag.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), id, value);
			}
		}

		[System.Obsolete]
		public void tellRemoveFlag(CSteamID steamID, ushort id)
		{
			ReceiveRemoveFlag(id);
		}

		private static readonly ClientInstanceMethod<ushort> SendRemoveFlag = ClientInstanceMethod<ushort>.Get(typeof(PlayerQuests), nameof(ReceiveRemoveFlag));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellRemoveFlag))]
		public void ReceiveRemoveFlag(ushort id)
		{
			removeFlag(id);
		}

		public void sendRemoveFlag(ushort id)
		{
			removeFlag(id);

			if (!channel.IsLocalPlayer)
			{
				SendRemoveFlag.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), id);
			}
		}

		[System.Obsolete]
		public void tellAddQuest(CSteamID steamID, ushort id)
		{

		}

		private static readonly ClientInstanceMethod<System.Guid> SendAddQuest = ClientInstanceMethod<System.Guid>.Get(typeof(PlayerQuests), nameof(ReceiveAddQuest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveAddQuest(System.Guid assetGuid)
		{
			QuestAsset asset = Assets.find<QuestAsset>(assetGuid);
			if (asset != null)
			{
				AddQuest(asset);
			}
		}

		public void ServerAddQuest(QuestAsset questAsset)
		{
			if (questAsset == null)
				return;

			AddQuest(questAsset);

			if (!channel.IsLocalPlayer)
			{
				SendAddQuest.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), questAsset.GUID);
			}
		}

		[System.Obsolete]
		public void sendAddQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				ServerAddQuest(questAsset);
			}
		}

		[System.Obsolete]
		public void tellRemoveQuest(CSteamID steamID, ushort id)
		{

		}

		private static readonly ClientInstanceMethod<System.Guid, bool> SendRemoveQuest = ClientInstanceMethod<System.Guid, bool>.Get(typeof(PlayerQuests), nameof(ReceiveRemoveQuest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveRemoveQuest(System.Guid assetGuid, bool wasCompleted)
		{
			QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
			if (questAsset != null)
			{
				RemoveQuest(questAsset, wasCompleted);
			}
		}

		public void ServerRemoveQuest(QuestAsset questAsset)
		{
			ServerRemoveQuest(questAsset, wasCompleted: false);
		}

		public void ServerRemoveQuest(QuestAsset questAsset, bool wasCompleted = false)
		{
			if (questAsset == null)
				return;

			RemoveQuest(questAsset, wasCompleted);

			if (!channel.IsLocalPlayer)
			{
				SendRemoveQuest.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), questAsset.GUID, wasCompleted);
			}

			if (!wasCompleted)
			{
				questAsset.GrantAbandonmentRewards(player);
			}
		}

		[System.Obsolete]
		public void sendRemoveQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			ServerRemoveQuest(questAsset);
		}

		public void TrackQuest(QuestAsset questAsset)
		{
			if (_trackedQuest != null && _trackedQuest == questAsset)
			{
				_trackedQuest = null;
			}
			else
			{
				_trackedQuest = questAsset;
			}

			if (channel.IsLocalPlayer)
			{
				TriggerTrackedQuestUpdated();
			}
		}

		[System.Obsolete]
		public void trackQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			TrackQuest(questAsset);
		}

		[System.Obsolete]
		public void askTrackQuest(CSteamID steamID, ushort id)
		{

		}

		private static readonly ServerInstanceMethod<System.Guid> SendTrackQuest = ServerInstanceMethod<System.Guid>.Get(typeof(PlayerQuests), nameof(ReceiveTrackQuest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5)]
		public void ReceiveTrackQuest(System.Guid assetGuid)
		{
			QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
			TrackQuest(questAsset);
		}

		public void ClientTrackQuest(QuestAsset questAsset)
		{
			SendTrackQuest.Invoke(GetNetId(), ENetReliability.Reliable, questAsset?.GUID ?? System.Guid.Empty);
		}

		[System.Obsolete]
		public void sendTrackQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			ClientTrackQuest(questAsset);
		}

		[System.Obsolete("Identical to ServerRemoveQuest")]
		public void AbandonQuest(QuestAsset questAsset)
		{
			ServerRemoveQuest(questAsset);
		}

		[System.Obsolete]
		public void abandonQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				AbandonQuest(questAsset);
			}
		}

		[System.Obsolete]
		public void askAbandonQuest(CSteamID steamID, ushort id)
		{

		}

		private static readonly ServerInstanceMethod<System.Guid> SendAbandonQuestRequest = ServerInstanceMethod<System.Guid>.Get(typeof(PlayerQuests), nameof(ReceiveAbandonQuestRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5)]
		public void ReceiveAbandonQuestRequest(System.Guid assetGuid)
		{
			QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
			if (questAsset != null)
			{
				ServerRemoveQuest(questAsset);
			}
		}

		/// <summary>
		/// Called by quest details UI to request server to abandon quest.
		/// </summary>
		public void ClientAbandonQuest(QuestAsset questAsset)
		{
			if (questAsset != null)
			{
				SendAbandonQuestRequest.Invoke(GetNetId(), ENetReliability.Reliable, questAsset.GUID);
			}
		}

		[System.Obsolete]
		public void sendAbandonQuest(ushort id)
		{
			QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
			if (questAsset != null)
			{
				ClientAbandonQuest(questAsset);
			}
		}

		private static readonly ServerInstanceMethod<System.Guid, byte, byte> SendChooseDialogueResponseRequest = ServerInstanceMethod<System.Guid, byte, byte>.Get(typeof(PlayerQuests), nameof(ReceiveChooseDialogueResponseRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 20)]
		public void ReceiveChooseDialogueResponseRequest(in ServerInvocationContext context, System.Guid assetGuid, byte messageIndex, byte responseIndex)
		{
			if (DialogueTarget == null)
			{
				context.LogWarning("null npc");
				return;
			}

			if ((DialogueTarget.GetDialogueTargetWorldPosition() - transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("npc too far away");
				return;
			}

			if (serverCurrentDialogueAsset == null)
			{
				context.LogWarning("no current dialogue");
				return;
			}

			if (serverCurrentDialogueMessage == null)
			{
				context.LogWarning("no current message");
				return;
			}

			DialogueAsset asset = Assets.find<DialogueAsset>(assetGuid);
			if (asset == null)
			{
				context.LogWarning("null dialogue asset");
				return;
			}

			if (asset != serverCurrentDialogueAsset || messageIndex != serverCurrentDialogueMessage.index)
			{
				context.LogWarning($"message {messageIndex} for dialogue {asset.name} does not match server current message {serverCurrentDialogueMessage.index} for dialogue {serverCurrentDialogueAsset.name}");
				return;
			}

			if (asset.responses == null || responseIndex >= asset.responses.Length)
			{
				context.LogWarning($"response index ({responseIndex}) out of bounds ({asset.responses.Length})");
				return;
			}

			if (serverCurrentDialogueMessage.responses != null && serverCurrentDialogueMessage.responses.Length > 0)
			{
				bool hasResponse = false;
				for (int checkIndex = 0; checkIndex < serverCurrentDialogueMessage.responses.Length; checkIndex++)
				{
					if (responseIndex == serverCurrentDialogueMessage.responses[checkIndex])
					{
						hasResponse = true;
						break;
					}
				}

				if (!hasResponse)
				{
					context.LogWarning("no matching response");
					return;
				}
			}

			DialogueResponse response = asset.responses[responseIndex];
			if (response == null || !response.areConditionsMet(player))
			{
				context.LogWarning("response conditions not met");
				return;
			}

			if (response.messages != null && response.messages.Length > 0)
			{
				bool hasMessage = false;
				for (int checkIndex = 0; checkIndex < response.messages.Length; checkIndex++)
				{
					if (serverCurrentDialogueMessage.index == response.messages[checkIndex])
					{
						hasMessage = true;
						break;
					}
				}

				if (!hasMessage)
				{
					context.LogWarning("no matching message");
					return;
				}
			}

#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ReceiveChooseDialogueResponseRequest Dialogue: {asset?.name ?? "null"}, Message: {messageIndex}, Response: {responseIndex}");
#endif // LOG_NPC_DIALOGUE_RPCS

			response.ApplyConditions(player);
			response.GrantRewards(player);

			VendorAsset nextVendorAsset = response.FindVendorAsset();
			DialogueAsset nextDialogueAsset = response.FindDialogueAsset();
			DialogueMessage nextMessage = nextDialogueAsset?.GetAvailableMessage(player);
			if (nextVendorAsset != null)
			{
				if (nextDialogueAsset == null || nextMessage == null)
				{
					// Asset has not linked a "thanks for shopping" dialogue, so vendor should return to current.
					nextDialogueAsset = serverCurrentDialogueAsset;
					nextMessage = serverCurrentDialogueMessage;
				}

				serverDefaultNextDialogueAsset = GetDefaultNextDialogueAsset(nextDialogueAsset, nextMessage, serverCurrentDialogueAsset);
				serverCurrentDialogueAsset = nextDialogueAsset;
				serverCurrentDialogueMessage = nextMessage;
				serverCurrentVendorAsset = nextVendorAsset;
#if LOG_NPC_DIALOGUE_RPCS
				UnturnedLog.info($"SendOpenVendor Vendor: {serverCurrentVendorAsset?.name ?? "null"} Dialogue: {serverCurrentDialogueAsset?.name ?? "null"} Message: {serverCurrentDialogueMessage.index} NextDialogue: {serverDefaultNextDialogueAsset?.name ?? "null"}");
#endif // LOG_NPC_DIALOGUE_RPCS
				SendOpenVendor.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), nextVendorAsset.GUID, nextDialogueAsset.GUID, nextMessage.index, serverDefaultNextDialogueAsset != null);
			}
			else if (nextDialogueAsset != null && nextMessage != null)
			{
				serverDefaultNextDialogueAsset = GetDefaultNextDialogueAsset(nextDialogueAsset, nextMessage, serverCurrentDialogueAsset);
				serverCurrentDialogueAsset = nextDialogueAsset;
				serverCurrentDialogueMessage = nextMessage;
				serverCurrentVendorAsset = null;
#if LOG_NPC_DIALOGUE_RPCS
				UnturnedLog.info($"SendOpenDialogue Dialogue: {serverCurrentDialogueAsset?.name ?? "null"} Message: {serverCurrentDialogueMessage.index} NextDialogue: {serverDefaultNextDialogueAsset?.name ?? "null"}");
#endif // LOG_NPC_DIALOGUE_RPCS
				SendOpenDialogue.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), nextDialogueAsset.GUID, nextMessage.index, serverDefaultNextDialogueAsset != null);
			}

			if (nextMessage != null)
			{
				// Warning: if changing this ordering please refer to comment in GetDefaultNextDialogueAsset!
				// Prior to 2023-09-04 the client would notify the server when a message was seen
				// and then the server would apply conditions and grand rewards. To improve
				// compatibility we do this AFTER sending the message to show.
				nextMessage.ApplyConditions(player);
				nextMessage.GrantRewards(player);
			}
		}

		private static List<DialogueResponse> tempDialogueResponses = new List<DialogueResponse>();
		private DialogueAsset GetDefaultNextDialogueAsset(DialogueAsset asset, DialogueMessage message, DialogueAsset previousAsset)
		{
			DialogueAsset dialogueOverride = message.FindPrevDialogueAsset();
			if (dialogueOverride != null)
			{
				return dialogueOverride;
			}

			// nextMessage applies conditions and rewards AFTER sending OpenDialogue reliably,
			// so responses should be consistent between client and server here.
			tempDialogueResponses.Clear();
			asset.getAvailableResponses(player, message.index, tempDialogueResponses);
			if (tempDialogueResponses.Count > 0)
			{
				return null;
			}

			// Default to previous.
			return previousAsset;
		}

		private static readonly ServerInstanceMethod<System.Guid, byte> SendChooseDefaultNextDialogueRequest = ServerInstanceMethod<System.Guid, byte>.Get(typeof(PlayerQuests), nameof(ReceiveChooseDefaultNextDialogueRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 20)]
		public void ReceiveChooseDefaultNextDialogueRequest(in ServerInvocationContext context, System.Guid assetGuid, byte index)
		{
			if (DialogueTarget == null)
			{
				context.LogWarning("null npc");
				return;
			}

			if ((DialogueTarget.GetDialogueTargetWorldPosition() - transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("npc too far away");
				return;
			}

			if (serverDefaultNextDialogueAsset == null)
			{
				context.LogWarning("no next dialogue");
				return;
			}

			if (serverCurrentDialogueAsset == null)
			{
				context.LogWarning("no current dialogue");
				return;
			}

			if (serverCurrentDialogueMessage == null)
			{
				context.LogWarning("no current message");
				return;
			}

			DialogueAsset asset = Assets.find<DialogueAsset>(assetGuid);
			if (asset == null)
			{
				context.LogWarning("null dialogue asset");
				return;
			}

			if (asset != serverCurrentDialogueAsset || index != serverCurrentDialogueMessage.index)
			{
				context.LogWarning($"message {index} for dialogue {asset.name} does not match server current message {serverCurrentDialogueMessage.index} for dialogue {serverCurrentDialogueAsset.name}");
				return;
			}

#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ReceiveChooseDefaultNextDialogueRequest Dialogue: {asset?.name ?? "null"} Message: {index} NextDialogue: {serverDefaultNextDialogueAsset?.name ?? "null"}");
#endif // LOG_NPC_DIALOGUE_RPCS

			DialogueAsset nextDialogueAsset = serverDefaultNextDialogueAsset;
			DialogueMessage nextMessage = nextDialogueAsset?.GetAvailableMessage(player);

			// Prevent this method from being spammed in a loop.
			serverDefaultNextDialogueAsset = null;

			if (nextMessage != null)
			{
				// Nelson 2023-10-12: in other cases we use FindPrevDialogueAsset() ?? serverCurrentDialogueAsset,
				// but in this case we come from a dialogue with a default response, so using it will cause a loop.
				// For example: Captain Sydney's dialogue Captain_Join_Request uses the default Next to return to
				// Welcome, which was then setting Captain_Join_Request as the default next dialogue.
				serverDefaultNextDialogueAsset = nextMessage.FindPrevDialogueAsset();

				serverCurrentDialogueAsset = nextDialogueAsset;
				serverCurrentDialogueMessage = nextMessage;
				serverCurrentVendorAsset = null;
#if LOG_NPC_DIALOGUE_RPCS
				UnturnedLog.info($"SendOpenDialogue Dialogue: {serverCurrentDialogueAsset?.name ?? "null"} Message: {serverCurrentDialogueMessage.index} NextDialogue: {serverDefaultNextDialogueAsset?.name ?? "null"}");
#endif // LOG_NPC_DIALOGUE_RPCS
				SendOpenDialogue.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), nextDialogueAsset.GUID, nextMessage.index, serverDefaultNextDialogueAsset != null);

				// Prior to 2023-09-04 the client would notify the server when a message was seen
				// and then the server would apply conditions and grand rewards. To improve
				// compatibility we do this AFTER sending the message to show.
				nextMessage.ApplyConditions(player);
				nextMessage.GrantRewards(player);
			}
		}

		public void ClientChooseDialogueResponse(System.Guid assetGuid, byte messageIndex, byte responseIndex)
		{
#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ClientChooseDialogueResponse Asset: {Assets.find(assetGuid)?.name ?? "null"} Message index: {messageIndex} Response index: {responseIndex}");
#endif
			SendChooseDialogueResponseRequest.Invoke(GetNetId(), ENetReliability.Reliable, assetGuid, messageIndex, responseIndex);
		}

		/// <summary>
		/// Called when there are no responses to choose, but server has indicated a next dialogue is available.
		/// </summary>
		public void ClientChooseDefaultNextDialogue(System.Guid assetGuid, byte index)
		{
#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ClientChooseDefaultNextDialogue Asset: {Assets.find(assetGuid)?.name ?? "null"} Index: {index}");
#endif
			SendChooseDefaultNextDialogueRequest.Invoke(GetNetId(), ENetReliability.Reliable, assetGuid, index);
		}

		[System.Obsolete]
		public void tellQuests(CSteamID steamID)
		{ }

		private static readonly ClientInstanceMethod SendQuests = ClientInstanceMethod.Get(typeof(PlayerQuests), nameof(ReceiveQuests));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveQuests(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			reader.ReadBit(out _isMarkerPlaced);
			reader.ReadClampedVector3(out _markerPosition);
			reader.ReadString(out _markerTextOverride);
			reader.ReadUInt32(out _radioFrequency);
			reader.ReadSteamID(out _groupID);
			reader.ReadEnum(out _groupRank);

			if (channel.IsLocalPlayer)
			{
				ushort flagCount;
				reader.ReadUInt16(out flagCount);
				for (ushort flagIndex = 0; flagIndex < flagCount; ++flagIndex)
				{
					ushort id;
					reader.ReadUInt16(out id);
					short value;
					reader.ReadInt16(out value);

					PlayerQuestFlag flag = new PlayerQuestFlag(id, value);
					flagsMap.Add(id, flag);
					flagsList.Add(flag);
				}

				int questCount;
				reader.ReadInt32(out questCount);
				for (int questIndex = 0; questIndex < questCount; ++questIndex)
				{
					System.Guid assetGuid;
					reader.ReadGuid(out assetGuid);
					QuestAsset asset = Assets.find<QuestAsset>(assetGuid);
					if (asset != null)
					{
						PlayerQuest quest = new PlayerQuest(asset);
						questsList.Add(quest);
					}
				}

				System.Guid trackedQuestGuid;
				reader.ReadGuid(out trackedQuestGuid);
				_trackedQuest = Assets.find<QuestAsset>(trackedQuestGuid);

				reader.ReadBit(out npcCutsceneMode);
				player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);

				onFlagsUpdated?.Invoke();

				TriggerTrackedQuestUpdated();
			}
		}

		[System.Obsolete]
		public void askQuests(CSteamID steamID)
		{ }

		private void WriteAllState(NetPakWriter writer)
		{
			writer.WriteBit(isMarkerPlaced);
			writer.WriteClampedVector3(markerPosition);
			writer.WriteString(markerTextOverride);
			writer.WriteUInt32(radioFrequency);
			writer.WriteSteamID(groupID);
			writer.WriteEnum(groupRank);
		}

		private void WriteOwnerState(NetPakWriter writer)
		{
			writer.WriteUInt16((ushort) flagsList.Count);
			for (ushort flagIndex = 0; flagIndex < flagsList.Count; ++flagIndex)
			{
				PlayerQuestFlag flag = flagsList[flagIndex];

				writer.WriteUInt16(flag.id);
				writer.WriteInt16(flag.value);
			}

			writer.WriteInt32(questsList.Count);
			foreach (PlayerQuest quest in questsList)
			{
				writer.WriteGuid(quest?.asset?.GUID ?? System.Guid.Empty);
			}

			writer.WriteGuid(_trackedQuest?.GUID ?? System.Guid.Empty);
			writer.WriteBit(npcCutsceneMode);
		}

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			bool sendingToOwner = channel.owner == client;

			if (channel.IsLocalPlayer && sendingToOwner)
			{
				// Server is the owner of this player (local player on listen server), so do not send any state because
				// this is how it worked prior to RPC rewrites.
				return;
			}

			if (isMemberOfAGroup)
			{
				GroupInfo group = GroupManager.getGroupInfo(groupID);
				if (group != null)
				{
					GroupManager.sendGroupInfo(client.transportConnection, group);
				}
			}

			SendQuests.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, SendQuests_Write, sendingToOwner);
		}

		private void SendQuests_Write(NetPakWriter writer, bool sendingToOwner)
		{
			WriteAllState(writer);
			if (sendingToOwner)
			{
				WriteOwnerState(writer);
			}
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			if (isMemberOfAGroup)
			{
				GroupInfo group = GroupManager.getGroupInfo(groupID);
				if (group != null)
				{
					GroupManager.sendGroupInfo(transportConnections, group);
				}
			}

			SendQuests.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, WriteAllState);
		}

		private void OnPlayerNavChanged(PlayerMovement sender, byte oldNav, byte newNav)
		{
			if (newNav == byte.MaxValue)
			{
				return;
			}

			ZombieManager.regions[newNav].UpdateBoss();
		}

		private void onExperienceUpdated(uint experience)
		{
			TriggerTrackedQuestUpdated();
		}

		private void onReputationUpdated(int reputation)
		{
			TriggerTrackedQuestUpdated();
		}

		private void onInventoryStateUpdated()
		{
			TriggerTrackedQuestUpdated();
		}

		private void onTimeOfDayChanged()
		{
			onExternalConditionsUpdated?.Invoke();
		}

		private static readonly ClientInstanceMethod<NetId, System.Guid, byte, bool> SendTalkWithNpcResponse = ClientInstanceMethod<NetId, System.Guid, byte, bool>.Get(typeof(PlayerQuests), nameof(ReceiveTalkWithNpcResponse));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveTalkWithNpcResponse(in ClientInvocationContext context, NetId targetNpcNetId, System.Guid dialogueAssetGuid, byte messageIndex, bool hasNextDialogue)
		{
			IDialogueTarget targetNpc = InteractableObjectNPC.GetDialogueTargetFromNetId(targetNpcNetId);
			if (targetNpc == null)
			{
				context.LogWarning("unable to find referenced NPC");
				return;
			}

			DialogueAsset rootDialogueAsset = Assets.find<DialogueAsset>(dialogueAssetGuid);
			ClientAssetIntegrity.QueueRequest(dialogueAssetGuid, rootDialogueAsset, "talk with NPC response");
			if (rootDialogueAsset == null)
			{
				context.LogWarning("null root dialogue");
				return;
			}

			if (messageIndex >= rootDialogueAsset.messages.Length)
			{
				context.LogWarning($"message index ({messageIndex}) out of bounds ({rootDialogueAsset.messages.Length})");
				return;
			}

			DialogueTarget = targetNpc;

			PlayerLifeUI.close();

			PlayerLifeUI.npc = targetNpc;
			DialogueTarget.SetIsTalkingWithLocalPlayer(true);

#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ReceiveTalkWithNpcResponse Dialogue: {rootDialogueAsset?.name ?? "null"} Message: {messageIndex} HasNextDialogue: {hasNextDialogue}");
#endif // LOG_NPC_DIALOGUE_RPCS
			PlayerNPCDialogueUI.open(rootDialogueAsset, rootDialogueAsset.messages[messageIndex], hasNextDialogue);
		}

		/// <summary>
		/// Called in singleplayer and on the server after client requests NPC dialogue.
		/// </summary>
		internal void ApproveTalkWithNpcRequest(IDialogueTarget newDialogueTarget, DialogueAsset rootDialogueAsset)
		{
			Assert.IsNotNull(newDialogueTarget);
			Assert.IsNotNull(rootDialogueAsset);
			DialogueMessage message = rootDialogueAsset.GetAvailableMessage(player);
			if (message == null)
			{
				UnturnedLog.warn($"Unable to approve talk with NPC ({newDialogueTarget.GetDialogueTargetDebugName()}) request because there is no valid message");
				return;
			}

			DialogueTarget = newDialogueTarget;
			serverCurrentDialogueAsset = rootDialogueAsset;
			serverCurrentDialogueMessage = message;
			serverCurrentVendorAsset = null;
			serverDefaultNextDialogueAsset = GetDefaultNextDialogueAsset(rootDialogueAsset, message, null);
#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"SendTalkWithNpcResponse Dialogue: {rootDialogueAsset?.name ?? "null"} Message: {message.index} NextDialogue: {serverDefaultNextDialogueAsset?.name ?? "null"}");
#endif // LOG_NPC_DIALOGUE_RPCS
			SendTalkWithNpcResponse.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), DialogueTarget.GetDialogueTargetNetId(), rootDialogueAsset.GUID, serverCurrentDialogueMessage.index, serverDefaultNextDialogueAsset != null);

			// Prior to 2023-09-04 the client would notify the server when a message was seen
			// and then the server would apply conditions and grand rewards. To improve
			// compatibility we do this AFTER sending the message to show.
			serverCurrentDialogueMessage.ApplyConditions(player);
			serverCurrentDialogueMessage.GrantRewards(player);
		}

		internal void ClearActiveNpc()
		{
			DialogueTarget = null;
			serverCurrentDialogueAsset = null;
			serverCurrentDialogueMessage = null;
			serverCurrentVendorAsset = null;
			serverDefaultNextDialogueAsset = null;
#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info("ClearActiveNpc");
#endif // LOG_NPC_DIALOGUE_RPCS
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, bool> SendOpenDialogue = ClientInstanceMethod<System.Guid, byte, bool>.Get(typeof(PlayerQuests), nameof(ReceiveOpenDialogue));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveOpenDialogue(in ClientInvocationContext context, System.Guid dialogueAssetGuid, byte messageIndex, bool hasNextDialogue)
		{
			DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(dialogueAssetGuid);
			ClientAssetIntegrity.QueueRequest(dialogueAssetGuid, dialogueAsset, "open dialogue");
			
			if (dialogueAsset == null)
			{
				context.LogWarning("missing dialogue asset");
				return;
			}

			if (dialogueAsset.messages == null)
			{
				context.LogWarning("dialogue null messages");
				return;
			}

			if (messageIndex >= dialogueAsset.messages.Length)
			{
				context.LogWarning($"message index ({messageIndex}) out of bounds ({dialogueAsset.messages.Length})");
			}

			if (PlayerNPCVendorUI.active)
			{
				PlayerNPCVendorUI.close();
			}

			if (PlayerNPCQuestUI.active)
			{
				PlayerNPCQuestUI.close();
			}

#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ReceiveOpenDialogue Dialogue: {dialogueAsset?.name ?? "null"} Message: {messageIndex} HasNextDialogue: {hasNextDialogue}");
#endif // LOG_NPC_DIALOGUE_RPCS
			DialogueMessage message = dialogueAsset.messages[messageIndex];
			PlayerNPCDialogueUI.open(dialogueAsset, message, hasNextDialogue);
		}

		private static readonly ClientInstanceMethod<System.Guid, System.Guid, byte, bool> SendOpenVendor = ClientInstanceMethod<System.Guid, System.Guid, byte, bool>.Get(typeof(PlayerQuests), nameof(ReceiveOpenVendor));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveOpenVendor(in ClientInvocationContext context, System.Guid vendorAssetGuid, System.Guid dialogueAssetGuid, byte messageIndex, bool hasNextDialogue)
		{
			VendorAsset vendorAsset = Assets.find<VendorAsset>(vendorAssetGuid);
			DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(dialogueAssetGuid);
			ClientAssetIntegrity.QueueRequest(vendorAssetGuid, vendorAsset, "open vendor");
			ClientAssetIntegrity.QueueRequest(dialogueAssetGuid, dialogueAsset, "open vendor");

			if (vendorAsset == null)
			{
				context.LogWarning("missing vendor asset");
				return;
			}

			if (dialogueAsset == null)
			{
				context.LogWarning("missing dialogue asset");
				return;
			}

			if (dialogueAsset.messages == null)
			{
				context.LogWarning("dialogue null messages");
				return;
			}

			if (messageIndex >= dialogueAsset.messages.Length)
			{
				context.LogWarning($"message index ({messageIndex}) out of bounds ({dialogueAsset.messages.Length})");
			}

			if (PlayerNPCDialogueUI.active)
			{
				PlayerNPCDialogueUI.close();
			}

			if (PlayerNPCQuestUI.active)
			{
				PlayerNPCQuestUI.close();
			}

#if LOG_NPC_DIALOGUE_RPCS
			UnturnedLog.info($"ReceiveOpenVendor Vendor: {vendorAsset?.name ?? "null"} Dialogue: {dialogueAsset?.name ?? "null"} Message: {messageIndex} HasNextDialogue: {hasNextDialogue}");
#endif // LOG_NPC_DIALOGUE_RPCS
			DialogueMessage message = dialogueAsset.messages[messageIndex];
			PlayerNPCVendorUI.open(vendorAsset, dialogueAsset, message, hasNextDialogue);
		}

		internal PlayerDelayedQuestRewardsComponent GetOrCreateDelayedQuestRewards()
		{
			if (!hasCreatedDelayedRewards && delayedRewardsComponent == null)
			{
				hasCreatedDelayedRewards = true;
				delayedRewardsGameObject = new GameObject();
				delayedRewardsComponent = delayedRewardsGameObject.AddComponent<PlayerDelayedQuestRewardsComponent>();
				delayedRewardsComponent.player = player;
			}
			return delayedRewardsComponent;
		}

		internal void InterruptDelayedQuestRewards(EDelayedQuestRewardsInterruption interruption)
		{
			if (delayedRewardsComponent != null)
			{
				delayedRewardsComponent.Interrupt(interruption);
			}
		}

		private GameObject delayedRewardsGameObject;
		private PlayerDelayedQuestRewardsComponent delayedRewardsComponent;
		/// <summary>
		/// Prevent re-creating it during destroy (e.g. plugin granting rewards) from leaking gameobject.
		/// </summary>
		private bool hasCreatedDelayedRewards;

		private void OnLifeUpdated(bool isDead)
		{
			if (isDead)
			{
				ServerSetCutsceneModeActive(false);
			}
		}

		internal void InitializePlayer()
		{
			flagsMap = new Dictionary<ushort, PlayerQuestFlag>();
			flagsList = new List<PlayerQuestFlag>();
			questsList = new List<PlayerQuest>();
			groupInvites = new HashSet<CSteamID>();

			if (Provider.isServer || channel.IsLocalPlayer)
			{
				player.life.onLifeUpdated += OnLifeUpdated;
			}

			if (Provider.isServer)
			{
				load();

				player.movement.PlayerNavChanged += OnPlayerNavChanged;

				if (channel.IsLocalPlayer)
				{
					onFlagsUpdated?.Invoke();
				}
			}

			if (channel.IsLocalPlayer)
			{
				player.skills.onExperienceUpdated += onExperienceUpdated;
				player.skills.onReputationUpdated += onReputationUpdated;

				player.inventory.onInventoryStateUpdated += onInventoryStateUpdated;

				LightingManager.onTimeOfDayChanged += onTimeOfDayChanged;
			}
		}

		private void Start()
		{
			// What a mess!
			if (channel.IsLocalPlayer || Provider.isServer)
			{
				try
				{
					Player.onPlayerCreated?.Invoke(player);
				}
				catch (System.Exception e)
				{
					UnturnedLog.warn("Exception during onPlayerCreated:");
					UnturnedLog.exception(e);
				}
			}
		}

		private void OnDestroy()
		{
			if (channel.IsLocalPlayer)
			{
				LightingManager.onTimeOfDayChanged -= onTimeOfDayChanged;
			}

			hasCreatedDelayedRewards = true; // Prevent re-creating.
			if (delayedRewardsGameObject != null)
			{
				Destroy(delayedRewardsGameObject);
				delayedRewardsGameObject = null;
			}
		}

		private bool wasLoadCalled;

		public void load()
		{
			wasLoadCalled = true;

			isMarkerPlaced = false;
			markerPosition = Vector3.zero;
			markerTextOverride = string.Empty;
			radioFrequency = DEFAULT_RADIO_FREQUENCY;

			if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Quests.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				River river = PlayerSavedata.openRiver(channel.owner.playerID, "/Player/Quests.dat", true);
				byte version = river.readByte();

				if (version > 0)
				{
					if (version > 6)
					{
						isMarkerPlaced = river.readBoolean();
						markerPosition = river.readSingleVector3();
					}

					if (version > 5)
					{
						radioFrequency = river.readUInt32();
					}

					if (version > 2)
					{
						groupID = river.readSteamID();
					}
					else
					{
						groupID = CSteamID.Nil;
					}

					if (version > 3)
					{
						groupRank = (EPlayerGroupRank) river.readByte();
					}
					else
					{
						groupRank = EPlayerGroupRank.MEMBER;
					}

					if (version > 4)
					{
						inMainGroup = river.readBoolean();
					}
					else
					{
						inMainGroup = false;
					}

					ushort flagCount = river.readUInt16();
					for (ushort flagIndex = 0; flagIndex < flagCount; flagIndex++)
					{
						ushort id = river.readUInt16();
						short value = river.readInt16();

						PlayerQuestFlag flag = new PlayerQuestFlag(id, value);
						flagsMap.Add(id, flag);
						flagsList.Add(flag);
					}

					if (version >= SAVEDATA_VERSION_ADDED_QUEST_LIST_GUIDS)
					{
						int questCount = river.readInt32();
						for (int questIndex = 0; questIndex < questCount; ++questIndex)
						{
							System.Guid assetGuid = river.readGUID();
							QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
							if (questAsset != null)
							{
								PlayerQuest quest = new PlayerQuest(questAsset);
								questsList.Add(quest);
							}
						}
					}
					else
					{
						ushort questCount = river.readUInt16();
						for (ushort questIndex = 0; questIndex < questCount; questIndex++)
						{
							ushort id = river.readUInt16();

							PlayerQuest quest = new PlayerQuest(id);
							questsList.Add(quest);
						}
					}

					if (version >= SAVEDATA_VERSION_ADDED_TRACKED_QUEST_GUID)
					{
						_trackedQuest = Assets.find<QuestAsset>(river.readGUID());
					}
					else if (version > 1)
					{
						_trackedQuest = Assets.find(EAssetType.NPC, river.readUInt16()) as QuestAsset;
					}
					else
					{
						_trackedQuest = null;
					}

					if (version < SAVEDATA_VERSION_ADDED_NPC_SPAWN_ID)
					{
						npcSpawnId = null;
					}
					else
					{
						npcSpawnId = river.readString();
					}

					if (version >= SAVEDATA_VERSION_ADDED_NPC_CUTSCENE_MODE)
					{
						npcCutsceneMode = river.readBoolean();
					}
					else
					{
						npcCutsceneMode = false;
					}
				}

				river.closeRiver();
			}

			if (channel.IsLocalPlayer)
			{
				player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
			}

			if (Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups)
			{
				if (groupID == CSteamID.Nil)
				{
					if (channel.owner.lobbyID != CSteamID.Nil && Provider.modeConfigData.Gameplay.Allow_Lobby_Groups)
					{
						bool wasCreated;
						GroupInfo lobbyGroup = GroupManager.getOrAddGroup(channel.owner.lobbyID, channel.owner.playerID.playerName + "'s Group",
																	 out wasCreated);

						if (wasCreated || lobbyGroup.hasSpaceForMoreMembersInGroup)
						{
							groupID = channel.owner.lobbyID;
							lobbyGroup.members++;
							groupRank = wasCreated ? EPlayerGroupRank.OWNER : EPlayerGroupRank.MEMBER;
							inMainGroup = false;
							GroupManager.sendGroupInfo(lobbyGroup);
						}
						else
						{
							loadMainGroup();
						}
					}
					else
					{
						loadMainGroup();
					}
				}
				else
				{
					if (inMainGroup)
					{
						if (Provider.modeConfigData.Gameplay.Allow_Static_Groups)
						{
							if (groupID != channel.owner.playerID.@group) // Kick out of group because they left it on Steam)
							{
								loadMainGroup();
							}
						}
						else
						{
							loadMainGroup(); // Kicks them out of the main group if disabled
						}
					}
					else
					{
						GroupInfo group = GroupManager.getGroupInfo(groupID);
						if (group == null) // Deleted by owner or group file reset so kick out
						{
							loadMainGroup();
						}
					}
				}
			}
			else
			{
				loadMainGroup();
			}
		}

		private void loadMainGroup()
		{
			if (Provider.modeConfigData.Gameplay.Allow_Static_Groups)
			{
				groupID = channel.owner.playerID.@group;
				inMainGroup = groupID != CSteamID.Nil;
			}
			else
			{
				groupID = CSteamID.Nil;
				inMainGroup = false;
			}
			groupRank = EPlayerGroupRank.MEMBER;
		}

		private int FindIndexOfQuest(QuestAsset asset)
		{
			if (asset != null)
			{
				for (int index = 0; index < questsList.Count; ++index)
				{
					PlayerQuest quest = questsList[index];
					if (quest.asset == asset)
					{
						return index;
					}
				}
			}

			return -1;
		}

		public void save()
		{
			if (!wasLoadCalled)
				return;

			River river = PlayerSavedata.openRiver(channel.owner.playerID, "/Player/Quests.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);

			river.writeBoolean(isMarkerPlaced);
			river.writeSingleVector3(markerPosition);
			river.writeUInt32(radioFrequency);
			river.writeSteamID(groupID);
			river.writeByte((byte) groupRank);
			river.writeBoolean(inMainGroup);

			river.writeUInt16((ushort) flagsList.Count);
			for (ushort flagIndex = 0; flagIndex < flagsList.Count; flagIndex++)
			{
				PlayerQuestFlag flag = flagsList[flagIndex];

				river.writeUInt16(flag.id);
				river.writeInt16(flag.value);
			}

			river.writeInt32(questsList.Count);
			foreach (PlayerQuest quest in questsList)
			{
				river.writeGUID(quest.asset?.GUID ?? System.Guid.Empty);
			}

			river.writeGUID(_trackedQuest?.GUID ?? System.Guid.Empty);
			river.writeString(string.IsNullOrEmpty(npcSpawnId) ? string.Empty : npcSpawnId);
			river.writeBoolean(npcCutsceneMode);

			river.closeRiver();
		}

		private float lastVehiclePurchaseRealtime = -10.0f;
	}
}
