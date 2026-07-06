#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableDoor))]
	public static class InteractableDoor_NetMethods
	{
		private static void ReceiveOpen_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableDoor netObj = voidNetObj as InteractableDoor;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableDoor, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Boolean newOpen;
#if LOG_INVOKE_READ_ERRORS
			bool newOpen_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newOpen);
#if LOG_INVOKE_READ_ERRORS
			if (!newOpen_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOpen));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveOpen(newOpen);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableDoor.ReceiveOpen), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveOpen_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveOpen_DeferredRead);
				return;
			}
			InteractableDoor netObj = voidNetObj as InteractableDoor;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableDoor, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newOpen;
#if LOG_INVOKE_READ_ERRORS
			bool newOpen_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newOpen);
#if LOG_INVOKE_READ_ERRORS
			if (!newOpen_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOpen));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveOpen(newOpen);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableDoor.ReceiveOpen), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveOpen_Write(NetPakWriter writer, System.Boolean newOpen)
		{
			writer.WriteBit(newOpen);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableDoor.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
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
			InteractableDoor netObj = voidNetObj as InteractableDoor;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableDoor, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean desiredOpen;
#if LOG_INVOKE_READ_ERRORS
			bool desiredOpen_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out desiredOpen);
#if LOG_INVOKE_READ_ERRORS
			if (!desiredOpen_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(desiredOpen));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveToggleRequest(context, desiredOpen);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableDoor.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleRequest_Write(NetPakWriter writer, System.Boolean desiredOpen)
		{
			writer.WriteBit(desiredOpen);
		}
	}
}
