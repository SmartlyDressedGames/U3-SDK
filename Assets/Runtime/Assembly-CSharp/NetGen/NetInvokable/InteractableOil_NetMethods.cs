#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableOil))]
	public static class InteractableOil_NetMethods
	{
		private static void ReceiveFuel_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableOil netObj = voidNetObj as InteractableOil;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableOil, but was {voidNetObj.GetType().Name}");
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
		[NetInvokableGeneratedMethod(nameof(InteractableOil.ReceiveFuel), ENetInvokableGeneratedMethodPurpose.Read)]
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
			InteractableOil netObj = voidNetObj as InteractableOil;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableOil, but was {voidNetObj.GetType().Name}");
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
		[NetInvokableGeneratedMethod(nameof(InteractableOil.ReceiveFuel), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFuel_Write(NetPakWriter writer, System.UInt16 newFuel)
		{
			writer.WriteUInt16(newFuel);
		}
	}
}
