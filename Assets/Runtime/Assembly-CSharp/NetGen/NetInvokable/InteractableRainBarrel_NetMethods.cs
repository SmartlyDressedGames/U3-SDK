#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableRainBarrel))]
	public static class InteractableRainBarrel_NetMethods
	{
		private static void ReceiveFull_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableRainBarrel netObj = voidNetObj as InteractableRainBarrel;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableRainBarrel, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Boolean newFull;
#if LOG_INVOKE_READ_ERRORS
			bool newFull_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newFull);
#if LOG_INVOKE_READ_ERRORS
			if (!newFull_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFull));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFull(newFull);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableRainBarrel.ReceiveFull), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveFull_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveFull_DeferredRead);
				return;
			}
			InteractableRainBarrel netObj = voidNetObj as InteractableRainBarrel;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableRainBarrel, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newFull;
#if LOG_INVOKE_READ_ERRORS
			bool newFull_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newFull);
#if LOG_INVOKE_READ_ERRORS
			if (!newFull_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFull));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFull(newFull);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableRainBarrel.ReceiveFull), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFull_Write(NetPakWriter writer, System.Boolean newFull)
		{
			writer.WriteBit(newFull);
		}
	}
}
