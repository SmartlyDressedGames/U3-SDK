#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableLibrary))]
	public static class InteractableLibrary_NetMethods
	{
		private static void ReceiveAmount_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableLibrary netObj = voidNetObj as InteractableLibrary;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableLibrary, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.UInt32 newAmount;
#if LOG_INVOKE_READ_ERRORS
			bool newAmount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newAmount);
#if LOG_INVOKE_READ_ERRORS
			if (!newAmount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAmount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAmount(newAmount);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableLibrary.ReceiveAmount), ENetInvokableGeneratedMethodPurpose.Read)]
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
			InteractableLibrary netObj = voidNetObj as InteractableLibrary;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableLibrary, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 newAmount;
#if LOG_INVOKE_READ_ERRORS
			bool newAmount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newAmount);
#if LOG_INVOKE_READ_ERRORS
			if (!newAmount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAmount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAmount(newAmount);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableLibrary.ReceiveAmount), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAmount_Write(NetPakWriter writer, System.UInt32 newAmount)
		{
			writer.WriteUInt32(newAmount);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableLibrary.ReceiveTransferLibraryRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTransferLibraryRequest_Read(in ServerInvocationContext context)
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
			InteractableLibrary netObj = voidNetObj as InteractableLibrary;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableLibrary, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte transaction;
#if LOG_INVOKE_READ_ERRORS
			bool transaction_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out transaction);
#if LOG_INVOKE_READ_ERRORS
			if (!transaction_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(transaction));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt32 delta;
#if LOG_INVOKE_READ_ERRORS
			bool delta_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out delta);
#if LOG_INVOKE_READ_ERRORS
			if (!delta_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(delta));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTransferLibraryRequest(context, transaction, delta);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableLibrary.ReceiveTransferLibraryRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTransferLibraryRequest_Write(NetPakWriter writer, System.Byte transaction, System.UInt32 delta)
		{
			writer.WriteUInt8(transaction);
			writer.WriteUInt32(delta);
		}
	}
}
