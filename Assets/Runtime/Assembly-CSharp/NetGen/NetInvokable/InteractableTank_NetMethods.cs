#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableTank))]
	public static class InteractableTank_NetMethods
	{
		private static void ReceiveAmount_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableTank netObj = voidNetObj as InteractableTank;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableTank, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.UInt16 newAmount;
#if LOG_INVOKE_READ_ERRORS
			bool newAmount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newAmount);
#if LOG_INVOKE_READ_ERRORS
			if (!newAmount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAmount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAmount(newAmount);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableTank.ReceiveAmount), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAmount_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveAmount_DeferredRead);
				return;
			}
			InteractableTank netObj = voidNetObj as InteractableTank;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableTank, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt16 newAmount;
#if LOG_INVOKE_READ_ERRORS
			bool newAmount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newAmount);
#if LOG_INVOKE_READ_ERRORS
			if (!newAmount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAmount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAmount(newAmount);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableTank.ReceiveAmount), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAmount_Write(NetPakWriter writer, System.UInt16 newAmount)
		{
			writer.WriteUInt16(newAmount);
		}
	}
}
