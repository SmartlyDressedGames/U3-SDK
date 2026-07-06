#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableVehicle))]
	public static class InteractableVehicle_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(InteractableVehicle.ReceivePaintColor), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePaintColor_Read(in ClientInvocationContext context)
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
			InteractableVehicle netObj = voidNetObj as InteractableVehicle;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableVehicle, but was {voidNetObj.GetType().Name}");
				return;
			}
			UnityEngine.Color32 newPaintColor;
#if LOG_INVOKE_READ_ERRORS
			bool newPaintColor_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadColor32RGBA(out newPaintColor);
#if LOG_INVOKE_READ_ERRORS
			if (!newPaintColor_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPaintColor));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePaintColor(newPaintColor);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableVehicle.ReceivePaintColor), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePaintColor_Write(NetPakWriter writer, UnityEngine.Color32 newPaintColor)
		{
			writer.WriteColor32RGBA(newPaintColor);
		}
	}
}
