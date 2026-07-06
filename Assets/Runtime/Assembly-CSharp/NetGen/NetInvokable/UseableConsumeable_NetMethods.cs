#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableConsumeable))]
	public static class UseableConsumeable_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableConsumeable.ReceivePlayConsume), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayConsume_Read(in ClientInvocationContext context)
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
			UseableConsumeable netObj = voidNetObj as UseableConsumeable;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableConsumeable, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EConsumeMode mode;
#if LOG_INVOKE_READ_ERRORS
			bool mode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out mode);
#if LOG_INVOKE_READ_ERRORS
			if (!mode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(mode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlayConsume(mode);
		}
		[NetInvokableGeneratedMethod(nameof(UseableConsumeable.ReceivePlayConsume), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayConsume_Write(NetPakWriter writer, SDG.Unturned.EConsumeMode mode)
		{
			writer.WriteEnum(mode);
		}
	}
}
