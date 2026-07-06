#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableFuel))]
	public static class UseableFuel_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableFuel.ReceivePlayGlug), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayGlug_Read(in ClientInvocationContext context)
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
			UseableFuel netObj = voidNetObj as UseableFuel;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableFuel, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayGlug();
		}
		[NetInvokableGeneratedMethod(nameof(UseableFuel.ReceivePlayGlug), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayGlug_Write(NetPakWriter writer)
		{
		}
	}
}
