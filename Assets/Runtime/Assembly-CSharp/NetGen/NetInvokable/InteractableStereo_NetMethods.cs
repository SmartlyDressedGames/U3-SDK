#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableStereo))]
	public static class InteractableStereo_NetMethods
	{
		private static void ReceiveTrack_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableStereo netObj = voidNetObj as InteractableStereo;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableStereo, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Guid newTrack;
#if LOG_INVOKE_READ_ERRORS
			bool newTrack_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out newTrack);
#if LOG_INVOKE_READ_ERRORS
			if (!newTrack_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newTrack));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTrack(newTrack);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveTrack), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTrack_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveTrack_DeferredRead);
				return;
			}
			InteractableStereo netObj = voidNetObj as InteractableStereo;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStereo, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid newTrack;
#if LOG_INVOKE_READ_ERRORS
			bool newTrack_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out newTrack);
#if LOG_INVOKE_READ_ERRORS
			if (!newTrack_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newTrack));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTrack(newTrack);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveTrack), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTrack_Write(NetPakWriter writer, System.Guid newTrack)
		{
			writer.WriteGuid(newTrack);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveTrackRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTrackRequest_Read(in ServerInvocationContext context)
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
			InteractableStereo netObj = voidNetObj as InteractableStereo;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStereo, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid newTrack;
#if LOG_INVOKE_READ_ERRORS
			bool newTrack_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out newTrack);
#if LOG_INVOKE_READ_ERRORS
			if (!newTrack_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newTrack));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTrackRequest(context, newTrack);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveTrackRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTrackRequest_Write(NetPakWriter writer, System.Guid newTrack)
		{
			writer.WriteGuid(newTrack);
		}
		private static void ReceiveChangeVolume_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableStereo netObj = voidNetObj as InteractableStereo;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableStereo, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Byte newVolume;
#if LOG_INVOKE_READ_ERRORS
			bool newVolume_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newVolume);
#if LOG_INVOKE_READ_ERRORS
			if (!newVolume_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newVolume));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeVolume(newVolume);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveChangeVolume), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChangeVolume_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveChangeVolume_DeferredRead);
				return;
			}
			InteractableStereo netObj = voidNetObj as InteractableStereo;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStereo, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newVolume;
#if LOG_INVOKE_READ_ERRORS
			bool newVolume_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newVolume);
#if LOG_INVOKE_READ_ERRORS
			if (!newVolume_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newVolume));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeVolume(newVolume);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveChangeVolume), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChangeVolume_Write(NetPakWriter writer, System.Byte newVolume)
		{
			writer.WriteUInt8(newVolume);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveChangeVolumeRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChangeVolumeRequest_Read(in ServerInvocationContext context)
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
			InteractableStereo netObj = voidNetObj as InteractableStereo;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStereo, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newVolume;
#if LOG_INVOKE_READ_ERRORS
			bool newVolume_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newVolume);
#if LOG_INVOKE_READ_ERRORS
			if (!newVolume_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newVolume));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeVolumeRequest(context, newVolume);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStereo.ReceiveChangeVolumeRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChangeVolumeRequest_Write(NetPakWriter writer, System.Byte newVolume)
		{
			writer.WriteUInt8(newVolume);
		}
	}
}
