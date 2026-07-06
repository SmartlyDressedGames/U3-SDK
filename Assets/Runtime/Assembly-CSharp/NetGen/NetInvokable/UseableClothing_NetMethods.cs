#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableClothing))]
	public static class UseableClothing_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableClothing.ReceivePlayWear), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayWear_Read(in ClientInvocationContext context)
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
			UseableClothing netObj = voidNetObj as UseableClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayWear();
		}
		[NetInvokableGeneratedMethod(nameof(UseableClothing.ReceivePlayWear), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayWear_Write(NetPakWriter writer)
		{
		}
	}
}
