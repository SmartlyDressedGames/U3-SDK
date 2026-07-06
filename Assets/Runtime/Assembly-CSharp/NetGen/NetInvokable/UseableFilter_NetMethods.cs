#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableFilter))]
	public static class UseableFilter_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableFilter.ReceivePlayFilter), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayFilter_Read(in ClientInvocationContext context)
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
			UseableFilter netObj = voidNetObj as UseableFilter;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFilter, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayFilter();
		}
		[NetInvokableGeneratedMethod(nameof(UseableFilter.ReceivePlayFilter), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayFilter_Write(NetPakWriter writer)
		{
		}
	}
}
