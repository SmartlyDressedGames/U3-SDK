#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableGrower))]
	public static class UseableGrower_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableGrower.ReceivePlayGrow), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayGrow_Read(in ClientInvocationContext context)
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
			UseableGrower netObj = voidNetObj as UseableGrower;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGrower, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayGrow();
		}
		[NetInvokableGeneratedMethod(nameof(UseableGrower.ReceivePlayGrow), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayGrow_Write(NetPakWriter writer)
		{
		}
	}
}
