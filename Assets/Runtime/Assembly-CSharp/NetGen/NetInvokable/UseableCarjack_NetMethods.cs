#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableCarjack))]
	public static class UseableCarjack_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableCarjack.ReceivePlayPull), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayPull_Read(in ClientInvocationContext context)
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
			UseableCarjack netObj = voidNetObj as UseableCarjack;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableCarjack, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayPull();
		}
		[NetInvokableGeneratedMethod(nameof(UseableCarjack.ReceivePlayPull), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayPull_Write(NetPakWriter writer)
		{
		}
	}
}
