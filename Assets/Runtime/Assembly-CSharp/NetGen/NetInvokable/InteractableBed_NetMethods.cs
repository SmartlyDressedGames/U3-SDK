#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableBed))]
	public static class InteractableBed_NetMethods
	{
		private static void ReceiveClaim_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableBed netObj = voidNetObj as InteractableBed;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableBed, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			Steamworks.CSteamID newOwner;
#if LOG_INVOKE_READ_ERRORS
			bool newOwner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newOwner);
#if LOG_INVOKE_READ_ERRORS
			if (!newOwner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOwner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveClaim(newOwner);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableBed.ReceiveClaim), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClaim_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveClaim_DeferredRead);
				return;
			}
			InteractableBed netObj = voidNetObj as InteractableBed;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableBed, but was {voidNetObj.GetType().Name}");
				return;
			}
			Steamworks.CSteamID newOwner;
#if LOG_INVOKE_READ_ERRORS
			bool newOwner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newOwner);
#if LOG_INVOKE_READ_ERRORS
			if (!newOwner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOwner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveClaim(newOwner);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableBed.ReceiveClaim), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClaim_Write(NetPakWriter writer, Steamworks.CSteamID newOwner)
		{
			writer.WriteSteamID(newOwner);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableBed.ReceiveClaimRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClaimRequest_Read(in ServerInvocationContext context)
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
			InteractableBed netObj = voidNetObj as InteractableBed;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableBed, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveClaimRequest(context);
		}
		// ReceiveClaimRequest write will be called directly.
	}
}
