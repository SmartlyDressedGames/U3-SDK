#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableCarlockpick))]
	public static class UseableCarlockpick_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableCarlockpick.ReceivePlayJimmy), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayJimmy_Read(in ClientInvocationContext context)
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
			UseableCarlockpick netObj = voidNetObj as UseableCarlockpick;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableCarlockpick, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean isFailure;
#if LOG_INVOKE_READ_ERRORS
			bool isFailure_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isFailure);
#if LOG_INVOKE_READ_ERRORS
			if (!isFailure_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isFailure));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlayJimmy(isFailure);
		}
		[NetInvokableGeneratedMethod(nameof(UseableCarlockpick.ReceivePlayJimmy), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayJimmy_Write(NetPakWriter writer, System.Boolean isFailure)
		{
			writer.WriteBit(isFailure);
		}
	}
}
