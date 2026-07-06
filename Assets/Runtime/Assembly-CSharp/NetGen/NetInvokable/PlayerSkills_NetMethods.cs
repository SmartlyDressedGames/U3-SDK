#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerSkills))]
	public static class PlayerSkills_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveExperience), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveExperience_Read(in ClientInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 newExperience;
#if LOG_INVOKE_READ_ERRORS
			bool newExperience_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newExperience);
#if LOG_INVOKE_READ_ERRORS
			if (!newExperience_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newExperience));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveExperience(newExperience);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveExperience), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveExperience_Write(NetPakWriter writer, System.UInt32 newExperience)
		{
			writer.WriteUInt32(newExperience);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveReputation), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveReputation_Read(in ClientInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Int32 newReputation;
#if LOG_INVOKE_READ_ERRORS
			bool newReputation_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out newReputation);
#if LOG_INVOKE_READ_ERRORS
			if (!newReputation_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newReputation));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveReputation(newReputation);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveReputation), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveReputation_Write(NetPakWriter writer, System.Int32 newReputation)
		{
			writer.WriteInt32(newReputation);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveBoost), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBoost_Read(in ClientInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EPlayerBoost newBoost;
#if LOG_INVOKE_READ_ERRORS
			bool newBoost_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newBoost);
#if LOG_INVOKE_READ_ERRORS
			if (!newBoost_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newBoost));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBoost(newBoost);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveBoost), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBoost_Write(NetPakWriter writer, SDG.Unturned.EPlayerBoost newBoost)
		{
			writer.WriteEnum(newBoost);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveSingleSkillLevel), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSingleSkillLevel_Read(in ClientInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte speciality;
#if LOG_INVOKE_READ_ERRORS
			bool speciality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out speciality);
#if LOG_INVOKE_READ_ERRORS
			if (!speciality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(speciality));
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
			System.Byte level;
#if LOG_INVOKE_READ_ERRORS
			bool level_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out level);
#if LOG_INVOKE_READ_ERRORS
			if (!level_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(level));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSingleSkillLevel(speciality, index, level);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveSingleSkillLevel), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSingleSkillLevel_Write(NetPakWriter writer, System.Byte speciality, System.Byte index, System.Byte level)
		{
			writer.WriteUInt8(speciality);
			writer.WriteUInt8(index);
			writer.WriteUInt8(level);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveUpgradeRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpgradeRequest_Read(in ServerInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte speciality;
#if LOG_INVOKE_READ_ERRORS
			bool speciality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out speciality);
#if LOG_INVOKE_READ_ERRORS
			if (!speciality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(speciality));
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
			System.Boolean force;
#if LOG_INVOKE_READ_ERRORS
			bool force_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out force);
#if LOG_INVOKE_READ_ERRORS
			if (!force_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(force));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveUpgradeRequest(speciality, index, force);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveUpgradeRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpgradeRequest_Write(NetPakWriter writer, System.Byte speciality, System.Byte index, System.Boolean force)
		{
			writer.WriteUInt8(speciality);
			writer.WriteUInt8(index);
			writer.WriteBit(force);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveBoostRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBoostRequest_Read(in ServerInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveBoostRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveBoostRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBoostRequest_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceivePurchaseRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePurchaseRequest_Read(in ServerInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			SDG.Unturned.NetId volumeNetId;
#if LOG_INVOKE_READ_ERRORS
			bool volumeNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out volumeNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!volumeNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(volumeNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePurchaseRequest(volumeNetId);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceivePurchaseRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePurchaseRequest_Write(NetPakWriter writer, SDG.Unturned.NetId volumeNetId)
		{
			writer.WriteNetId(volumeNetId);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerSkills.ReceiveMultipleSkillLevels), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveMultipleSkillLevels_Read(in ClientInvocationContext context)
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
			PlayerSkills netObj = voidNetObj as PlayerSkills;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerSkills, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveMultipleSkillLevels(context);
		}
		// ReceiveMultipleSkillLevels write will be called directly.
	}
}
