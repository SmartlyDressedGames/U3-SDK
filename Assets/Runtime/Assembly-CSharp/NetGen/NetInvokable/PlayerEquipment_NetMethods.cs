#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerEquipment))]
	public static class PlayerEquipment_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveItemHotkeySuggeston), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveItemHotkeySuggeston_Read(in ClientInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte hotkeyIndex;
#if LOG_INVOKE_READ_ERRORS
			bool hotkeyIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out hotkeyIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!hotkeyIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hotkeyIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid expectedAssetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool expectedAssetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out expectedAssetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!expectedAssetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(expectedAssetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveItemHotkeySuggeston(context, hotkeyIndex, expectedAssetGuid, page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveItemHotkeySuggeston), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveItemHotkeySuggeston_Write(NetPakWriter writer, System.Byte hotkeyIndex, System.Guid expectedAssetGuid, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(hotkeyIndex);
			writer.WriteGuid(expectedAssetGuid);
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveToggleVisionRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveToggleVisionRequest_Read(in ServerInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveToggleVisionRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveToggleVisionRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleVisionRequest_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveToggleVision), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveToggleVision_Read(in ClientInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveToggleVision();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveToggleVision), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleVision_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveSlot), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSlot_Read(in ClientInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte slot;
#if LOG_INVOKE_READ_ERRORS
			bool slot_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out slot);
#if LOG_INVOKE_READ_ERRORS
			if (!slot_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(slot));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
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
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			netObj.ReceiveSlot(slot, id, state);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveSlot), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSlot_Write(NetPakWriter writer, System.Byte slot, System.UInt16 id, System.Byte[] state)
		{
			writer.WriteUInt8(slot);
			writer.WriteUInt16(id);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveUpdateStateTemp), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdateStateTemp_Read(in ClientInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte[] newState;
			byte newState_Length;
			reader.ReadUInt8(out newState_Length);
			newState = new byte[newState_Length];
			reader.ReadBytes(newState);
			netObj.ReceiveUpdateStateTemp(newState);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveUpdateStateTemp), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdateStateTemp_Write(NetPakWriter writer, System.Byte[] newState)
		{
			byte newState_Length = (byte) newState.Length;
			writer.WriteUInt8(newState_Length);
			writer.WriteBytes(newState, newState_Length);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveUpdateState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdateState_Read(in ClientInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] newState;
			byte newState_Length;
			reader.ReadUInt8(out newState_Length);
			newState = new byte[newState_Length];
			reader.ReadBytes(newState);
			netObj.ReceiveUpdateState(page, index, newState);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveUpdateState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdateState_Write(NetPakWriter writer, System.Byte page, System.Byte index, System.Byte[] newState)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(index);
			byte newState_Length = (byte) newState.Length;
			writer.WriteUInt8(newState_Length);
			writer.WriteBytes(newState, newState_Length);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveEquip), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEquip_Read(in ClientInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid newAssetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool newAssetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out newAssetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!newAssetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAssetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newQuality;
#if LOG_INVOKE_READ_ERRORS
			bool newQuality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newQuality);
#if LOG_INVOKE_READ_ERRORS
			if (!newQuality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newQuality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] newState;
			byte newState_Length;
			reader.ReadUInt8(out newState_Length);
			newState = new byte[newState_Length];
			reader.ReadBytes(newState);
			SDG.Unturned.NetId useableNetId;
#if LOG_INVOKE_READ_ERRORS
			bool useableNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out useableNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!useableNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(useableNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveEquip(page, x, y, newAssetGuid, newQuality, newState, useableNetId);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveEquip), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEquip_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Guid newAssetGuid, System.Byte newQuality, System.Byte[] newState, SDG.Unturned.NetId useableNetId)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteGuid(newAssetGuid);
			writer.WriteUInt8(newQuality);
			byte newState_Length = (byte) newState.Length;
			writer.WriteUInt8(newState_Length);
			writer.WriteBytes(newState, newState_Length);
			writer.WriteNetId(useableNetId);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveEquipRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEquipRequest_Read(in ServerInvocationContext context)
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
			PlayerEquipment netObj = voidNetObj as PlayerEquipment;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerEquipment, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveEquipRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerEquipment.ReceiveEquipRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEquipRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
	}
}
