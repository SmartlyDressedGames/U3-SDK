#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableStorage))]
	public static class InteractableStorage_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveInteractRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveInteractRequest_Read(in ServerInvocationContext context)
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
			InteractableStorage netObj = voidNetObj as InteractableStorage;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStorage, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean quickGrab;
#if LOG_INVOKE_READ_ERRORS
			bool quickGrab_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out quickGrab);
#if LOG_INVOKE_READ_ERRORS
			if (!quickGrab_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quickGrab));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveInteractRequest(context, quickGrab);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveInteractRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveInteractRequest_Write(NetPakWriter writer, System.Boolean quickGrab)
		{
			writer.WriteBit(quickGrab);
		}
		private static void ReceiveDisplay_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableStorage netObj = voidNetObj as InteractableStorage;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableStorage, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.UInt16 skin;
#if LOG_INVOKE_READ_ERRORS
			bool skin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out skin);
#if LOG_INVOKE_READ_ERRORS
			if (!skin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(skin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 mythic;
#if LOG_INVOKE_READ_ERRORS
			bool mythic_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out mythic);
#if LOG_INVOKE_READ_ERRORS
			if (!mythic_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(mythic));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String tags;
#if LOG_INVOKE_READ_ERRORS
			bool tags_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out tags);
#if LOG_INVOKE_READ_ERRORS
			if (!tags_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(tags));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String dynamicProps;
#if LOG_INVOKE_READ_ERRORS
			bool dynamicProps_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out dynamicProps);
#if LOG_INVOKE_READ_ERRORS
			if (!dynamicProps_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(dynamicProps));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDisplay(id, quality, state, skin, mythic, tags, dynamicProps);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveDisplay), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDisplay_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveDisplay_DeferredRead);
				return;
			}
			InteractableStorage netObj = voidNetObj as InteractableStorage;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStorage, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.UInt16 skin;
#if LOG_INVOKE_READ_ERRORS
			bool skin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out skin);
#if LOG_INVOKE_READ_ERRORS
			if (!skin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(skin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 mythic;
#if LOG_INVOKE_READ_ERRORS
			bool mythic_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out mythic);
#if LOG_INVOKE_READ_ERRORS
			if (!mythic_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(mythic));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String tags;
#if LOG_INVOKE_READ_ERRORS
			bool tags_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out tags);
#if LOG_INVOKE_READ_ERRORS
			if (!tags_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(tags));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String dynamicProps;
#if LOG_INVOKE_READ_ERRORS
			bool dynamicProps_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out dynamicProps);
#if LOG_INVOKE_READ_ERRORS
			if (!dynamicProps_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(dynamicProps));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDisplay(id, quality, state, skin, mythic, tags, dynamicProps);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveDisplay), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDisplay_Write(NetPakWriter writer, System.UInt16 id, System.Byte quality, System.Byte[] state, System.UInt16 skin, System.UInt16 mythic, System.String tags, System.String dynamicProps)
		{
			writer.WriteUInt16(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteUInt16(skin);
			writer.WriteUInt16(mythic);
			writer.WriteString(tags);
			writer.WriteString(dynamicProps);
		}
		private static void ReceiveRotDisplay_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableStorage netObj = voidNetObj as InteractableStorage;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableStorage, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Byte rotComp;
#if LOG_INVOKE_READ_ERRORS
			bool rotComp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rotComp);
#if LOG_INVOKE_READ_ERRORS
			if (!rotComp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotComp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRotDisplay(rotComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveRotDisplay), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRotDisplay_Read(in ClientInvocationContext context)
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
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveRotDisplay_DeferredRead);
				return;
			}
			InteractableStorage netObj = voidNetObj as InteractableStorage;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStorage, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte rotComp;
#if LOG_INVOKE_READ_ERRORS
			bool rotComp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rotComp);
#if LOG_INVOKE_READ_ERRORS
			if (!rotComp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotComp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRotDisplay(rotComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveRotDisplay), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRotDisplay_Write(NetPakWriter writer, System.Byte rotComp)
		{
			writer.WriteUInt8(rotComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveRotDisplayRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRotDisplayRequest_Read(in ServerInvocationContext context)
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
			InteractableStorage netObj = voidNetObj as InteractableStorage;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableStorage, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte rotComp;
#if LOG_INVOKE_READ_ERRORS
			bool rotComp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rotComp);
#if LOG_INVOKE_READ_ERRORS
			if (!rotComp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotComp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRotDisplayRequest(context, rotComp);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableStorage.ReceiveRotDisplayRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRotDisplayRequest_Write(NetPakWriter writer, System.Byte rotComp)
		{
			writer.WriteUInt8(rotComp);
		}
	}
}
