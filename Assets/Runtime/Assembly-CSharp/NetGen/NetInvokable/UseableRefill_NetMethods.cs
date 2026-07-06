#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableRefill))]
	public static class UseableRefill_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableRefill.ReceivePlayUse), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayUse_Read(in ClientInvocationContext context)
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
			UseableRefill netObj = voidNetObj as UseableRefill;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableRefill, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayUse();
		}
		[NetInvokableGeneratedMethod(nameof(UseableRefill.ReceivePlayUse), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayUse_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableRefill.ReceivePlayRefill), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayRefill_Read(in ClientInvocationContext context)
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
			UseableRefill netObj = voidNetObj as UseableRefill;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableRefill, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayRefill();
		}
		[NetInvokableGeneratedMethod(nameof(UseableRefill.ReceivePlayRefill), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayRefill_Write(NetPakWriter writer)
		{
		}
	}
}
