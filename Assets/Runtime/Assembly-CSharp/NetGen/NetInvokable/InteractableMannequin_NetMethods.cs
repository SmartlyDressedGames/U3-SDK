#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableMannequin))]
	public static class InteractableMannequin_NetMethods
	{
		private static void ReceivePose_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableMannequin netObj = voidNetObj as InteractableMannequin;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableMannequin, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Byte poseComp;
#if LOG_INVOKE_READ_ERRORS
			bool poseComp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out poseComp);
#if LOG_INVOKE_READ_ERRORS
			if (!poseComp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(poseComp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePose(poseComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceivePose), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePose_Read(in ClientInvocationContext context)
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceivePose_DeferredRead);
				return;
			}
			InteractableMannequin netObj = voidNetObj as InteractableMannequin;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableMannequin, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte poseComp;
#if LOG_INVOKE_READ_ERRORS
			bool poseComp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out poseComp);
#if LOG_INVOKE_READ_ERRORS
			if (!poseComp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(poseComp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePose(poseComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceivePose), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePose_Write(NetPakWriter writer, System.Byte poseComp)
		{
			writer.WriteUInt8(poseComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceivePoseRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePoseRequest_Read(in ServerInvocationContext context)
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
			InteractableMannequin netObj = voidNetObj as InteractableMannequin;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableMannequin, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte poseComp;
#if LOG_INVOKE_READ_ERRORS
			bool poseComp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out poseComp);
#if LOG_INVOKE_READ_ERRORS
			if (!poseComp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(poseComp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePoseRequest(context, poseComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceivePoseRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePoseRequest_Write(NetPakWriter writer, System.Byte poseComp)
		{
			writer.WriteUInt8(poseComp);
		}
		private static void ReceiveUpdate_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableMannequin netObj = voidNetObj as InteractableMannequin;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableMannequin, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			netObj.ReceiveUpdate(state);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceiveUpdate), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdate_Read(in ClientInvocationContext context)
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveUpdate_DeferredRead);
				return;
			}
			InteractableMannequin netObj = voidNetObj as InteractableMannequin;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableMannequin, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			netObj.ReceiveUpdate(state);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceiveUpdate), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdate_Write(NetPakWriter writer, System.Byte[] state)
		{
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceiveUpdateRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdateRequest_Read(in ServerInvocationContext context)
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
			InteractableMannequin netObj = voidNetObj as InteractableMannequin;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableMannequin, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EMannequinUpdateMode updateMode;
#if LOG_INVOKE_READ_ERRORS
			bool updateMode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out updateMode);
#if LOG_INVOKE_READ_ERRORS
			if (!updateMode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(updateMode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveUpdateRequest(context, updateMode);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableMannequin.ReceiveUpdateRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdateRequest_Write(NetPakWriter writer, SDG.Unturned.EMannequinUpdateMode updateMode)
		{
			writer.WriteEnum(updateMode);
		}
	}
}
