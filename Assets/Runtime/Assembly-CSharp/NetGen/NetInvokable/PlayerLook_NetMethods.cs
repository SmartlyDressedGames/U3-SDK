#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerLook))]
	public static class PlayerLook_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerLook.ReceiveFreecamAllowed), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveFreecamAllowed_Read(in ClientInvocationContext context)
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
			PlayerLook netObj = voidNetObj as PlayerLook;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLook, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean isAllowed;
#if LOG_INVOKE_READ_ERRORS
			bool isAllowed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isAllowed);
#if LOG_INVOKE_READ_ERRORS
			if (!isAllowed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isAllowed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFreecamAllowed(isAllowed);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLook.ReceiveFreecamAllowed), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFreecamAllowed_Write(NetPakWriter writer, System.Boolean isAllowed)
		{
			writer.WriteBit(isAllowed);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLook.ReceiveWorkzoneAllowed), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWorkzoneAllowed_Read(in ClientInvocationContext context)
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
			PlayerLook netObj = voidNetObj as PlayerLook;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLook, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean isAllowed;
#if LOG_INVOKE_READ_ERRORS
			bool isAllowed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isAllowed);
#if LOG_INVOKE_READ_ERRORS
			if (!isAllowed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isAllowed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWorkzoneAllowed(isAllowed);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLook.ReceiveWorkzoneAllowed), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWorkzoneAllowed_Write(NetPakWriter writer, System.Boolean isAllowed)
		{
			writer.WriteBit(isAllowed);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLook.ReceiveSpecStatsAllowed), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSpecStatsAllowed_Read(in ClientInvocationContext context)
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
			PlayerLook netObj = voidNetObj as PlayerLook;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLook, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean isAllowed;
#if LOG_INVOKE_READ_ERRORS
			bool isAllowed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isAllowed);
#if LOG_INVOKE_READ_ERRORS
			if (!isAllowed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isAllowed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSpecStatsAllowed(isAllowed);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLook.ReceiveSpecStatsAllowed), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSpecStatsAllowed_Write(NetPakWriter writer, System.Boolean isAllowed)
		{
			writer.WriteBit(isAllowed);
		}
	}
}
