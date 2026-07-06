#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableArrestStart))]
	public static class UseableArrestStart_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableArrestStart.ReceivePlayArrest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayArrest_Read(in ClientInvocationContext context)
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
			UseableArrestStart netObj = voidNetObj as UseableArrestStart;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableArrestStart, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayArrest();
		}
		[NetInvokableGeneratedMethod(nameof(UseableArrestStart.ReceivePlayArrest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayArrest_Write(NetPakWriter writer)
		{
		}
	}
}
