#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableThrowable))]
	public static class UseableThrowable_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableThrowable.ReceiveToss), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveToss_Read(in ClientInvocationContext context)
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
			UseableThrowable netObj = voidNetObj as UseableThrowable;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableThrowable, but was {voidNetObj.GetType().Name}");
				return;
			}
			UnityEngine.Vector3 origin;
#if LOG_INVOKE_READ_ERRORS
			bool origin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out origin);
#if LOG_INVOKE_READ_ERRORS
			if (!origin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(origin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 force;
#if LOG_INVOKE_READ_ERRORS
			bool force_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out force);
#if LOG_INVOKE_READ_ERRORS
			if (!force_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(force));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveToss(origin, force);
		}
		[NetInvokableGeneratedMethod(nameof(UseableThrowable.ReceiveToss), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToss_Write(NetPakWriter writer, UnityEngine.Vector3 origin, UnityEngine.Vector3 force)
		{
			writer.WriteClampedVector3(origin);
			writer.WriteClampedVector3(force);
		}
		[NetInvokableGeneratedMethod(nameof(UseableThrowable.ReceivePlaySwing), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlaySwing_Read(in ClientInvocationContext context)
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
			UseableThrowable netObj = voidNetObj as UseableThrowable;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableThrowable, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlaySwing();
		}
		[NetInvokableGeneratedMethod(nameof(UseableThrowable.ReceivePlaySwing), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlaySwing_Write(NetPakWriter writer)
		{
		}
	}
}
