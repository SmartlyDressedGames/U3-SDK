#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableFisher))]
	public static class UseableFisher_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceiveBobberInWaterConfirmation), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBobberInWaterConfirmation_Read(in ServerInvocationContext context)
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
			UseableFisher netObj = voidNetObj as UseableFisher;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFisher, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			SDG.Unturned.NetId waterVolumeNetId;
#if LOG_INVOKE_READ_ERRORS
			bool waterVolumeNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out waterVolumeNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!waterVolumeNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(waterVolumeNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBobberInWaterConfirmation(context, waterVolumeNetId);
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceiveBobberInWaterConfirmation), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBobberInWaterConfirmation_Write(NetPakWriter writer, SDG.Unturned.NetId waterVolumeNetId)
		{
			writer.WriteNetId(waterVolumeNetId);
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceiveCatchConfirmation), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveCatchConfirmation_Read(in ServerInvocationContext context)
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
			UseableFisher netObj = voidNetObj as UseableFisher;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFisher, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveCatchConfirmation(context);
		}
		// ReceiveCatchConfirmation write will be called directly.
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceiveFishNotification), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveFishNotification_Read(in ClientInvocationContext context)
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
			UseableFisher netObj = voidNetObj as UseableFisher;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFisher, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid nextRewardGuid;
#if LOG_INVOKE_READ_ERRORS
			bool nextRewardGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out nextRewardGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!nextRewardGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(nextRewardGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int32 newSeed;
#if LOG_INVOKE_READ_ERRORS
			bool newSeed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out newSeed);
#if LOG_INVOKE_READ_ERRORS
			if (!newSeed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newSeed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFishNotification(nextRewardGuid, newSeed);
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceiveFishNotification), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFishNotification_Write(NetPakWriter writer, System.Guid nextRewardGuid, System.Int32 newSeed)
		{
			writer.WriteGuid(nextRewardGuid);
			writer.WriteInt32(newSeed);
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceivePlayReel), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayReel_Read(in ClientInvocationContext context)
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
			UseableFisher netObj = voidNetObj as UseableFisher;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFisher, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayReel();
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceivePlayReel), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayReel_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceivePlayCast), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayCast_Read(in ClientInvocationContext context)
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
			UseableFisher netObj = voidNetObj as UseableFisher;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFisher, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayCast();
		}
		[NetInvokableGeneratedMethod(nameof(UseableFisher.ReceivePlayCast), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayCast_Write(NetPakWriter writer)
		{
		}
	}
}
