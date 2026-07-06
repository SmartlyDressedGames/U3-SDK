#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableFarm))]
	public static class InteractableFarm_NetMethods
	{
		private static void ReceivePlanted_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableFarm netObj = voidNetObj as InteractableFarm;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableFarm, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.UInt32 newPlanted;
#if LOG_INVOKE_READ_ERRORS
			bool newPlanted_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newPlanted);
#if LOG_INVOKE_READ_ERRORS
			if (!newPlanted_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPlanted));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlanted(newPlanted);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableFarm.ReceivePlanted), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlanted_Read(in ClientInvocationContext context)
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceivePlanted_DeferredRead);
				return;
			}
			InteractableFarm netObj = voidNetObj as InteractableFarm;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableFarm, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 newPlanted;
#if LOG_INVOKE_READ_ERRORS
			bool newPlanted_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newPlanted);
#if LOG_INVOKE_READ_ERRORS
			if (!newPlanted_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPlanted));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlanted(newPlanted);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableFarm.ReceivePlanted), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlanted_Write(NetPakWriter writer, System.UInt32 newPlanted)
		{
			writer.WriteUInt32(newPlanted);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableFarm.ReceiveHarvestRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveHarvestRequest_Read(in ServerInvocationContext context)
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
			InteractableFarm netObj = voidNetObj as InteractableFarm;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableFarm, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveHarvestRequest(context);
		}
		// ReceiveHarvestRequest write will be called directly.
	}
}
