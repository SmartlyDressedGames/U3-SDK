#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableSentry))]
	public static class InteractableSentry_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(InteractableSentry.ReceiveShoot), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveShoot_Read(in ClientInvocationContext context)
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
			InteractableSentry netObj = voidNetObj as InteractableSentry;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableSentry, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveShoot();
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSentry.ReceiveShoot), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveShoot_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSentry.ReceiveAlert), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAlert_Read(in ClientInvocationContext context)
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
			InteractableSentry netObj = voidNetObj as InteractableSentry;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableSentry, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte yaw;
#if LOG_INVOKE_READ_ERRORS
			bool yaw_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out yaw);
#if LOG_INVOKE_READ_ERRORS
			if (!yaw_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(yaw));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte pitch;
#if LOG_INVOKE_READ_ERRORS
			bool pitch_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out pitch);
#if LOG_INVOKE_READ_ERRORS
			if (!pitch_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(pitch));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAlert(yaw, pitch);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSentry.ReceiveAlert), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAlert_Write(NetPakWriter writer, System.Byte yaw, System.Byte pitch)
		{
			writer.WriteUInt8(yaw);
			writer.WriteUInt8(pitch);
		}
	}
}
