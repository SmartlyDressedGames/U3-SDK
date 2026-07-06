#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(ItemManager))]
	public static class ItemManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveDestroyItem), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDestroyItem_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean shouldPlayEffect;
#if LOG_INVOKE_READ_ERRORS
			bool shouldPlayEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out shouldPlayEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!shouldPlayEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(shouldPlayEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ItemManager.ReceiveDestroyItem(x, y, instanceID, shouldPlayEffect);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveDestroyItem), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDestroyItem_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt32 instanceID, System.Boolean shouldPlayEffect)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt32(instanceID);
			writer.WriteBit(shouldPlayEffect);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveTakeItemRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTakeItemRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte to_x;
#if LOG_INVOKE_READ_ERRORS
			bool to_x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out to_x);
#if LOG_INVOKE_READ_ERRORS
			if (!to_x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(to_x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte to_y;
#if LOG_INVOKE_READ_ERRORS
			bool to_y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out to_y);
#if LOG_INVOKE_READ_ERRORS
			if (!to_y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(to_y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte to_rot;
#if LOG_INVOKE_READ_ERRORS
			bool to_rot_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out to_rot);
#if LOG_INVOKE_READ_ERRORS
			if (!to_rot_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(to_rot));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte to_page;
#if LOG_INVOKE_READ_ERRORS
			bool to_page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out to_page);
#if LOG_INVOKE_READ_ERRORS
			if (!to_page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(to_page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ItemManager.ReceiveTakeItemRequest(context, x, y, instanceID, to_x, to_y, to_rot, to_page);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveTakeItemRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTakeItemRequest_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt32 instanceID, System.Byte to_x, System.Byte to_y, System.Byte to_rot, System.Byte to_page)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt32(instanceID);
			writer.WriteUInt8(to_x);
			writer.WriteUInt8(to_y);
			writer.WriteUInt8(to_rot);
			writer.WriteUInt8(to_page);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveClearRegionItems), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClearRegionItems_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			ItemManager.ReceiveClearRegionItems(x, y);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveClearRegionItems), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClearRegionItems_Write(NetPakWriter writer, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveItem), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveItem_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			System.Byte amount;
#if LOG_INVOKE_READ_ERRORS
			bool amount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out amount);
#if LOG_INVOKE_READ_ERRORS
			if (!amount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(amount));
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean shouldPlayEffect;
#if LOG_INVOKE_READ_ERRORS
			bool shouldPlayEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out shouldPlayEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!shouldPlayEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(shouldPlayEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ItemManager.ReceiveItem(x, y, id, amount, quality, state, point, instanceID, shouldPlayEffect);
		}
		[NetInvokableGeneratedMethod(nameof(ItemManager.ReceiveItem), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveItem_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 id, System.Byte amount, System.Byte quality, System.Byte[] state, UnityEngine.Vector3 point, System.UInt32 instanceID, System.Boolean shouldPlayEffect)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(id);
			writer.WriteUInt8(amount);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteClampedVector3(point);
			writer.WriteUInt32(instanceID);
			writer.WriteBit(shouldPlayEffect);
		}
		// ReceiveItems read will be called directly.
		// ReceiveItems write will be called directly.
	}
}
