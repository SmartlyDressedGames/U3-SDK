#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerQuests))]
	public static class PlayerQuests_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveCutsceneMode), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveCutsceneMode_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newCutsceneMode;
#if LOG_INVOKE_READ_ERRORS
			bool newCutsceneMode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newCutsceneMode);
#if LOG_INVOKE_READ_ERRORS
			if (!newCutsceneMode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newCutsceneMode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveCutsceneMode(newCutsceneMode);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveCutsceneMode), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveCutsceneMode_Write(NetPakWriter writer, System.Boolean newCutsceneMode)
		{
			writer.WriteBit(newCutsceneMode);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveMarkerState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveMarkerState_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newIsMarkerPlaced;
#if LOG_INVOKE_READ_ERRORS
			bool newIsMarkerPlaced_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newIsMarkerPlaced);
#if LOG_INVOKE_READ_ERRORS
			if (!newIsMarkerPlaced_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newIsMarkerPlaced));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newMarkerPosition;
#if LOG_INVOKE_READ_ERRORS
			bool newMarkerPosition_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newMarkerPosition);
#if LOG_INVOKE_READ_ERRORS
			if (!newMarkerPosition_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newMarkerPosition));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String newMarkerTextOverride;
#if LOG_INVOKE_READ_ERRORS
			bool newMarkerTextOverride_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out newMarkerTextOverride);
#if LOG_INVOKE_READ_ERRORS
			if (!newMarkerTextOverride_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newMarkerTextOverride));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveMarkerState(newIsMarkerPlaced, newMarkerPosition, newMarkerTextOverride);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveMarkerState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveMarkerState_Write(NetPakWriter writer, System.Boolean newIsMarkerPlaced, UnityEngine.Vector3 newMarkerPosition, System.String newMarkerTextOverride)
		{
			writer.WriteBit(newIsMarkerPlaced);
			writer.WriteClampedVector3(newMarkerPosition);
			writer.WriteString(newMarkerTextOverride);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSetMarkerRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSetMarkerRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Boolean newIsMarkerPlaced;
#if LOG_INVOKE_READ_ERRORS
			bool newIsMarkerPlaced_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newIsMarkerPlaced);
#if LOG_INVOKE_READ_ERRORS
			if (!newIsMarkerPlaced_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newIsMarkerPlaced));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newMarkerPosition;
#if LOG_INVOKE_READ_ERRORS
			bool newMarkerPosition_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newMarkerPosition);
#if LOG_INVOKE_READ_ERRORS
			if (!newMarkerPosition_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newMarkerPosition));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSetMarkerRequest(newIsMarkerPlaced, newMarkerPosition);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSetMarkerRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSetMarkerRequest_Write(NetPakWriter writer, System.Boolean newIsMarkerPlaced, UnityEngine.Vector3 newMarkerPosition)
		{
			writer.WriteBit(newIsMarkerPlaced);
			writer.WriteClampedVector3(newMarkerPosition);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRadioFrequencyState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRadioFrequencyState_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 newRadioFrequency;
#if LOG_INVOKE_READ_ERRORS
			bool newRadioFrequency_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newRadioFrequency);
#if LOG_INVOKE_READ_ERRORS
			if (!newRadioFrequency_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRadioFrequency));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRadioFrequencyState(newRadioFrequency);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRadioFrequencyState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRadioFrequencyState_Write(NetPakWriter writer, System.UInt32 newRadioFrequency)
		{
			writer.WriteUInt32(newRadioFrequency);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSetRadioFrequencyRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSetRadioFrequencyRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.UInt32 newRadioFrequency;
#if LOG_INVOKE_READ_ERRORS
			bool newRadioFrequency_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newRadioFrequency);
#if LOG_INVOKE_READ_ERRORS
			if (!newRadioFrequency_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRadioFrequency));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSetRadioFrequencyRequest(newRadioFrequency);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSetRadioFrequencyRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSetRadioFrequencyRequest_Write(NetPakWriter writer, System.UInt32 newRadioFrequency)
		{
			writer.WriteUInt32(newRadioFrequency);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveGroupState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveGroupState_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			Steamworks.CSteamID newGroupID;
#if LOG_INVOKE_READ_ERRORS
			bool newGroupID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newGroupID);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroupID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroupID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.EPlayerGroupRank newGroupRank;
#if LOG_INVOKE_READ_ERRORS
			bool newGroupRank_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newGroupRank);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroupRank_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroupRank));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveGroupState(newGroupID, newGroupRank);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveGroupState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveGroupState_Write(NetPakWriter writer, Steamworks.CSteamID newGroupID, SDG.Unturned.EPlayerGroupRank newGroupRank)
		{
			writer.WriteSteamID(newGroupID);
			writer.WriteEnum(newGroupRank);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAcceptGroupInvitationRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAcceptGroupInvitationRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			Steamworks.CSteamID newGroupID;
#if LOG_INVOKE_READ_ERRORS
			bool newGroupID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newGroupID);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroupID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroupID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAcceptGroupInvitationRequest(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAcceptGroupInvitationRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAcceptGroupInvitationRequest_Write(NetPakWriter writer, Steamworks.CSteamID newGroupID)
		{
			writer.WriteSteamID(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveDeclineGroupInvitationRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDeclineGroupInvitationRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			Steamworks.CSteamID newGroupID;
#if LOG_INVOKE_READ_ERRORS
			bool newGroupID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newGroupID);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroupID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroupID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDeclineGroupInvitationRequest(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveDeclineGroupInvitationRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDeclineGroupInvitationRequest_Write(NetPakWriter writer, Steamworks.CSteamID newGroupID)
		{
			writer.WriteSteamID(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveLeaveGroupRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLeaveGroupRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveLeaveGroupRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveLeaveGroupRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLeaveGroupRequest_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveDeleteGroupRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDeleteGroupRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveDeleteGroupRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveDeleteGroupRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDeleteGroupRequest_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveCreateGroupRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveCreateGroupRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveCreateGroupRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveCreateGroupRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveCreateGroupRequest_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAddGroupInviteClient), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAddGroupInviteClient_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			Steamworks.CSteamID newGroupID;
#if LOG_INVOKE_READ_ERRORS
			bool newGroupID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newGroupID);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroupID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroupID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAddGroupInviteClient(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAddGroupInviteClient), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAddGroupInviteClient_Write(NetPakWriter writer, Steamworks.CSteamID newGroupID)
		{
			writer.WriteSteamID(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRemoveGroupInviteClient), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRemoveGroupInviteClient_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			Steamworks.CSteamID newGroupID;
#if LOG_INVOKE_READ_ERRORS
			bool newGroupID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newGroupID);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroupID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroupID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRemoveGroupInviteClient(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRemoveGroupInviteClient), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRemoveGroupInviteClient_Write(NetPakWriter writer, Steamworks.CSteamID newGroupID)
		{
			writer.WriteSteamID(newGroupID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAddGroupInviteRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAddGroupInviteRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			Steamworks.CSteamID targetID;
#if LOG_INVOKE_READ_ERRORS
			bool targetID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out targetID);
#if LOG_INVOKE_READ_ERRORS
			if (!targetID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(targetID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAddGroupInviteRequest(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAddGroupInviteRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAddGroupInviteRequest_Write(NetPakWriter writer, Steamworks.CSteamID targetID)
		{
			writer.WriteSteamID(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceivePromoteRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePromoteRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			Steamworks.CSteamID targetID;
#if LOG_INVOKE_READ_ERRORS
			bool targetID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out targetID);
#if LOG_INVOKE_READ_ERRORS
			if (!targetID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(targetID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePromoteRequest(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceivePromoteRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePromoteRequest_Write(NetPakWriter writer, Steamworks.CSteamID targetID)
		{
			writer.WriteSteamID(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveDemoteRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDemoteRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			Steamworks.CSteamID targetID;
#if LOG_INVOKE_READ_ERRORS
			bool targetID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out targetID);
#if LOG_INVOKE_READ_ERRORS
			if (!targetID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(targetID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDemoteRequest(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveDemoteRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDemoteRequest_Write(NetPakWriter writer, Steamworks.CSteamID targetID)
		{
			writer.WriteSteamID(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveKickFromGroup), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveKickFromGroup_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			Steamworks.CSteamID targetID;
#if LOG_INVOKE_READ_ERRORS
			bool targetID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out targetID);
#if LOG_INVOKE_READ_ERRORS
			if (!targetID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(targetID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveKickFromGroup(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveKickFromGroup), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveKickFromGroup_Write(NetPakWriter writer, Steamworks.CSteamID targetID)
		{
			writer.WriteSteamID(targetID);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRenameGroupRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRenameGroupRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.String newName;
#if LOG_INVOKE_READ_ERRORS
			bool newName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out newName);
#if LOG_INVOKE_READ_ERRORS
			if (!newName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRenameGroupRequest(newName);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRenameGroupRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRenameGroupRequest_Write(NetPakWriter writer, System.String newName)
		{
			writer.WriteString(newName);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSellToVendor), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSellToVendor_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean asManyAsPossible;
#if LOG_INVOKE_READ_ERRORS
			bool asManyAsPossible_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out asManyAsPossible);
#if LOG_INVOKE_READ_ERRORS
			if (!asManyAsPossible_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(asManyAsPossible));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSellToVendor(context, assetGuid, index, asManyAsPossible);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSellToVendor), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSellToVendor_Write(NetPakWriter writer, System.Guid assetGuid, System.Byte index, System.Boolean asManyAsPossible)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteUInt8(index);
			writer.WriteBit(asManyAsPossible);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveBuyFromVendor), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBuyFromVendor_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean asManyAsPossible;
#if LOG_INVOKE_READ_ERRORS
			bool asManyAsPossible_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out asManyAsPossible);
#if LOG_INVOKE_READ_ERRORS
			if (!asManyAsPossible_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(asManyAsPossible));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBuyFromVendor(context, assetGuid, index, asManyAsPossible);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveBuyFromVendor), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBuyFromVendor_Write(NetPakWriter writer, System.Guid assetGuid, System.Byte index, System.Boolean asManyAsPossible)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteUInt8(index);
			writer.WriteBit(asManyAsPossible);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSetFlag), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSetFlag_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int16 value;
#if LOG_INVOKE_READ_ERRORS
			bool value_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out value);
#if LOG_INVOKE_READ_ERRORS
			if (!value_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(value));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSetFlag(id, value);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveSetFlag), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSetFlag_Write(NetPakWriter writer, System.UInt16 id, System.Int16 value)
		{
			writer.WriteUInt16(id);
			writer.WriteInt16(value);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRemoveFlag), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRemoveFlag_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRemoveFlag(id);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRemoveFlag), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRemoveFlag_Write(NetPakWriter writer, System.UInt16 id)
		{
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAddQuest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAddQuest_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAddQuest(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAddQuest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAddQuest_Write(NetPakWriter writer, System.Guid assetGuid)
		{
			writer.WriteGuid(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRemoveQuest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRemoveQuest_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean wasCompleted;
#if LOG_INVOKE_READ_ERRORS
			bool wasCompleted_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out wasCompleted);
#if LOG_INVOKE_READ_ERRORS
			if (!wasCompleted_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(wasCompleted));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRemoveQuest(assetGuid, wasCompleted);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveRemoveQuest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRemoveQuest_Write(NetPakWriter writer, System.Guid assetGuid, System.Boolean wasCompleted)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteBit(wasCompleted);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveTrackQuest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTrackQuest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTrackQuest(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveTrackQuest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTrackQuest_Write(NetPakWriter writer, System.Guid assetGuid)
		{
			writer.WriteGuid(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAbandonQuestRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAbandonQuestRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAbandonQuestRequest(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveAbandonQuestRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAbandonQuestRequest_Write(NetPakWriter writer, System.Guid assetGuid)
		{
			writer.WriteGuid(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveChooseDialogueResponseRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChooseDialogueResponseRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte messageIndex;
#if LOG_INVOKE_READ_ERRORS
			bool messageIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out messageIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!messageIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(messageIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte responseIndex;
#if LOG_INVOKE_READ_ERRORS
			bool responseIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out responseIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!responseIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(responseIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChooseDialogueResponseRequest(context, assetGuid, messageIndex, responseIndex);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveChooseDialogueResponseRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChooseDialogueResponseRequest_Write(NetPakWriter writer, System.Guid assetGuid, System.Byte messageIndex, System.Byte responseIndex)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteUInt8(messageIndex);
			writer.WriteUInt8(responseIndex);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveChooseDefaultNextDialogueRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChooseDefaultNextDialogueRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChooseDefaultNextDialogueRequest(context, assetGuid, index);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveChooseDefaultNextDialogueRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChooseDefaultNextDialogueRequest_Write(NetPakWriter writer, System.Guid assetGuid, System.Byte index)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteUInt8(index);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveQuests), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveQuests_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveQuests(context);
		}
		// ReceiveQuests write will be called directly.
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveTalkWithNpcResponse), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTalkWithNpcResponse_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.NetId targetNpcNetId;
#if LOG_INVOKE_READ_ERRORS
			bool targetNpcNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out targetNpcNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!targetNpcNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(targetNpcNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid dialogueAssetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool dialogueAssetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out dialogueAssetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!dialogueAssetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(dialogueAssetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte messageIndex;
#if LOG_INVOKE_READ_ERRORS
			bool messageIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out messageIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!messageIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(messageIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean hasNextDialogue;
#if LOG_INVOKE_READ_ERRORS
			bool hasNextDialogue_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out hasNextDialogue);
#if LOG_INVOKE_READ_ERRORS
			if (!hasNextDialogue_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hasNextDialogue));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTalkWithNpcResponse(context, targetNpcNetId, dialogueAssetGuid, messageIndex, hasNextDialogue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveTalkWithNpcResponse), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTalkWithNpcResponse_Write(NetPakWriter writer, SDG.Unturned.NetId targetNpcNetId, System.Guid dialogueAssetGuid, System.Byte messageIndex, System.Boolean hasNextDialogue)
		{
			writer.WriteNetId(targetNpcNetId);
			writer.WriteGuid(dialogueAssetGuid);
			writer.WriteUInt8(messageIndex);
			writer.WriteBit(hasNextDialogue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveOpenDialogue), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveOpenDialogue_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid dialogueAssetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool dialogueAssetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out dialogueAssetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!dialogueAssetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(dialogueAssetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte messageIndex;
#if LOG_INVOKE_READ_ERRORS
			bool messageIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out messageIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!messageIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(messageIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean hasNextDialogue;
#if LOG_INVOKE_READ_ERRORS
			bool hasNextDialogue_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out hasNextDialogue);
#if LOG_INVOKE_READ_ERRORS
			if (!hasNextDialogue_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hasNextDialogue));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveOpenDialogue(context, dialogueAssetGuid, messageIndex, hasNextDialogue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveOpenDialogue), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveOpenDialogue_Write(NetPakWriter writer, System.Guid dialogueAssetGuid, System.Byte messageIndex, System.Boolean hasNextDialogue)
		{
			writer.WriteGuid(dialogueAssetGuid);
			writer.WriteUInt8(messageIndex);
			writer.WriteBit(hasNextDialogue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveOpenVendor), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveOpenVendor_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerQuests netObj = voidNetObj as PlayerQuests;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerQuests, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid vendorAssetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool vendorAssetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out vendorAssetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!vendorAssetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(vendorAssetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid dialogueAssetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool dialogueAssetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out dialogueAssetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!dialogueAssetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(dialogueAssetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte messageIndex;
#if LOG_INVOKE_READ_ERRORS
			bool messageIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out messageIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!messageIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(messageIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean hasNextDialogue;
#if LOG_INVOKE_READ_ERRORS
			bool hasNextDialogue_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out hasNextDialogue);
#if LOG_INVOKE_READ_ERRORS
			if (!hasNextDialogue_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hasNextDialogue));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveOpenVendor(context, vendorAssetGuid, dialogueAssetGuid, messageIndex, hasNextDialogue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerQuests.ReceiveOpenVendor), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveOpenVendor_Write(NetPakWriter writer, System.Guid vendorAssetGuid, System.Guid dialogueAssetGuid, System.Byte messageIndex, System.Boolean hasNextDialogue)
		{
			writer.WriteGuid(vendorAssetGuid);
			writer.WriteGuid(dialogueAssetGuid);
			writer.WriteUInt8(messageIndex);
			writer.WriteBit(hasNextDialogue);
		}
	}
}
