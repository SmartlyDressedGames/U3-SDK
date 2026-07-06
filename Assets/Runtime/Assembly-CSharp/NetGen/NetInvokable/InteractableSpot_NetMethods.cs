#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableSpot))]
	public static class InteractableSpot_NetMethods
	{
		private static void ReceivePowered_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableSpot netObj = voidNetObj as InteractableSpot;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableSpot, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Boolean newPowered;
#if LOG_INVOKE_READ_ERRORS
			bool newPowered_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newPowered);
#if LOG_INVOKE_READ_ERRORS
			if (!newPowered_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPowered));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePowered(newPowered);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSpot.ReceivePowered), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePowered_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceivePowered_DeferredRead);
				return;
			}
			InteractableSpot netObj = voidNetObj as InteractableSpot;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableSpot, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newPowered;
#if LOG_INVOKE_READ_ERRORS
			bool newPowered_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newPowered);
#if LOG_INVOKE_READ_ERRORS
			if (!newPowered_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPowered));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePowered(newPowered);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSpot.ReceivePowered), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePowered_Write(NetPakWriter writer, System.Boolean newPowered)
		{
			writer.WriteBit(newPowered);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSpot.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
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
			InteractableSpot netObj = voidNetObj as InteractableSpot;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableSpot, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean desiredPowered;
#if LOG_INVOKE_READ_ERRORS
			bool desiredPowered_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out desiredPowered);
#if LOG_INVOKE_READ_ERRORS
			if (!desiredPowered_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(desiredPowered));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveToggleRequest(context, desiredPowered);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSpot.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleRequest_Write(NetPakWriter writer, System.Boolean desiredPowered)
		{
			writer.WriteBit(desiredPowered);
		}
	}
}
