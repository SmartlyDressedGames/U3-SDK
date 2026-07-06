#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableOven))]
	public static class InteractableOven_NetMethods
	{
		private static void ReceiveLit_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableOven netObj = voidNetObj as InteractableOven;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableOven, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Boolean newLit;
#if LOG_INVOKE_READ_ERRORS
			bool newLit_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newLit);
#if LOG_INVOKE_READ_ERRORS
			if (!newLit_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newLit));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveLit(newLit);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableOven.ReceiveLit), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLit_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveLit_DeferredRead);
				return;
			}
			InteractableOven netObj = voidNetObj as InteractableOven;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableOven, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newLit;
#if LOG_INVOKE_READ_ERRORS
			bool newLit_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newLit);
#if LOG_INVOKE_READ_ERRORS
			if (!newLit_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newLit));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveLit(newLit);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableOven.ReceiveLit), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLit_Write(NetPakWriter writer, System.Boolean newLit)
		{
			writer.WriteBit(newLit);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableOven.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveToggleRequest_Read(in ServerInvocationContext context)
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
			InteractableOven netObj = voidNetObj as InteractableOven;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableOven, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean desiredLit;
#if LOG_INVOKE_READ_ERRORS
			bool desiredLit_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out desiredLit);
#if LOG_INVOKE_READ_ERRORS
			if (!desiredLit_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(desiredLit));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveToggleRequest(context, desiredLit);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableOven.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleRequest_Write(NetPakWriter writer, System.Boolean desiredLit)
		{
			writer.WriteBit(desiredLit);
		}
	}
}
