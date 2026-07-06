#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableTire))]
	public static class UseableTire_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableTire.ReceivePlayAttach), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayAttach_Read(in ClientInvocationContext context)
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
			UseableTire netObj = voidNetObj as UseableTire;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableTire, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayAttach();
		}
		[NetInvokableGeneratedMethod(nameof(UseableTire.ReceivePlayAttach), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayAttach_Write(NetPakWriter writer)
		{
		}
	}
}
