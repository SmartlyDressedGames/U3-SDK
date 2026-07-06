#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableGenerator))]
	public static class InteractableGenerator_NetMethods
	{
		private static void ReceiveFuel_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableGenerator netObj = voidNetObj as InteractableGenerator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableGenerator, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.UInt16 newFuel;
#if LOG_INVOKE_READ_ERRORS
			bool newFuel_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newFuel);
#if LOG_INVOKE_READ_ERRORS
			if (!newFuel_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFuel));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFuel(newFuel);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableGenerator.ReceiveFuel), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveFuel_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveFuel_DeferredRead);
				return;
			}
			InteractableGenerator netObj = voidNetObj as InteractableGenerator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableGenerator, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt16 newFuel;
#if LOG_INVOKE_READ_ERRORS
			bool newFuel_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newFuel);
#if LOG_INVOKE_READ_ERRORS
			if (!newFuel_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFuel));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFuel(newFuel);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableGenerator.ReceiveFuel), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFuel_Write(NetPakWriter writer, System.UInt16 newFuel)
		{
			writer.WriteUInt16(newFuel);
		}
		private static void ReceivePowered_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableGenerator netObj = voidNetObj as InteractableGenerator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableGenerator, but was {voidNetObj.GetType().Name}");
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
		[NetInvokableGeneratedMethod(nameof(InteractableGenerator.ReceivePowered), ENetInvokableGeneratedMethodPurpose.Read)]
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
			InteractableGenerator netObj = voidNetObj as InteractableGenerator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableGenerator, but was {voidNetObj.GetType().Name}");
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
		[NetInvokableGeneratedMethod(nameof(InteractableGenerator.ReceivePowered), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePowered_Write(NetPakWriter writer, System.Boolean newPowered)
		{
			writer.WriteBit(newPowered);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableGenerator.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
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
			InteractableGenerator netObj = voidNetObj as InteractableGenerator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableGenerator, but was {voidNetObj.GetType().Name}");
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
		[NetInvokableGeneratedMethod(nameof(InteractableGenerator.ReceiveToggleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleRequest_Write(NetPakWriter writer, System.Boolean desiredPowered)
		{
			writer.WriteBit(desiredPowered);
		}
	}
}
