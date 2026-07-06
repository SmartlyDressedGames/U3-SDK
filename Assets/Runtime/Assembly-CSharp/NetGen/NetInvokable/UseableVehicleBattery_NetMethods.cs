#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableVehicleBattery))]
	public static class UseableVehicleBattery_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableVehicleBattery.ReceivePlayReplace), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayReplace_Read(in ClientInvocationContext context)
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
			UseableVehicleBattery netObj = voidNetObj as UseableVehicleBattery;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableVehicleBattery, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayReplace();
		}
		[NetInvokableGeneratedMethod(nameof(UseableVehicleBattery.ReceivePlayReplace), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayReplace_Write(NetPakWriter writer)
		{
		}
	}
}
