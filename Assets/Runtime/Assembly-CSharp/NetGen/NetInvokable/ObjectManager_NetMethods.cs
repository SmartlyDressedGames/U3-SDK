#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(ObjectManager))]
	public static class ObjectManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveObjectRubble), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveObjectRubble_Read(in ClientInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte section;
#if LOG_INVOKE_READ_ERRORS
			bool section_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out section);
#if LOG_INVOKE_READ_ERRORS
			if (!section_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(section));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean isAlive;
#if LOG_INVOKE_READ_ERRORS
			bool isAlive_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isAlive);
#if LOG_INVOKE_READ_ERRORS
			if (!isAlive_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isAlive));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 ragdoll;
#if LOG_INVOKE_READ_ERRORS
			bool ragdoll_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out ragdoll);
#if LOG_INVOKE_READ_ERRORS
			if (!ragdoll_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(ragdoll));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveObjectRubble(x, y, index, section, isAlive, ragdoll);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveObjectRubble), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveObjectRubble_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index, System.Byte section, System.Boolean isAlive, UnityEngine.Vector3 ragdoll)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
			writer.WriteUInt8(section);
			writer.WriteBit(isAlive);
			writer.WriteClampedVector3(ragdoll);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveTalkWithNpcRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTalkWithNpcRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SDG.Unturned.NetId netId;
#if LOG_INVOKE_READ_ERRORS
			bool netId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out netId);
#if LOG_INVOKE_READ_ERRORS
			if (!netId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(netId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveTalkWithNpcRequest(context, netId);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveTalkWithNpcRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTalkWithNpcRequest_Write(NetPakWriter writer, SDG.Unturned.NetId netId)
		{
			writer.WriteNetId(netId);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveUseObjectQuest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUseObjectQuest_Read(in ServerInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveUseObjectQuest(context, x, y, index);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveUseObjectQuest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUseObjectQuest_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveUseObjectDropper), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUseObjectDropper_Read(in ServerInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveUseObjectDropper(context, x, y, index);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveUseObjectDropper), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUseObjectDropper_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveObjectResourceState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveObjectResourceState_Read(in ClientInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 amount;
#if LOG_INVOKE_READ_ERRORS
			bool amount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out amount);
#if LOG_INVOKE_READ_ERRORS
			if (!amount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(amount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveObjectResourceState(x, y, index, amount);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveObjectResourceState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveObjectResourceState_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index, System.UInt16 amount)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
			writer.WriteUInt16(amount);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveObjectBinaryState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveObjectBinaryState_Read(in ClientInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean isUsed;
#if LOG_INVOKE_READ_ERRORS
			bool isUsed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isUsed);
#if LOG_INVOKE_READ_ERRORS
			if (!isUsed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isUsed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveObjectBinaryState(x, y, index, isUsed);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveObjectBinaryState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveObjectBinaryState_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index, System.Boolean isUsed)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
			writer.WriteBit(isUsed);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveToggleObjectBinaryStateRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveToggleObjectBinaryStateRequest_Read(in ServerInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean isUsed;
#if LOG_INVOKE_READ_ERRORS
			bool isUsed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out isUsed);
#if LOG_INVOKE_READ_ERRORS
			if (!isUsed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(isUsed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ObjectManager.ReceiveToggleObjectBinaryStateRequest(context, x, y, index, isUsed);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveToggleObjectBinaryStateRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleObjectBinaryStateRequest_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index, System.Boolean isUsed)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
			writer.WriteBit(isUsed);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveClearRegionObjects), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClearRegionObjects_Read(in ClientInvocationContext context)
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
			ObjectManager.ReceiveClearRegionObjects(x, y);
		}
		[NetInvokableGeneratedMethod(nameof(ObjectManager.ReceiveClearRegionObjects), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClearRegionObjects_Write(NetPakWriter writer, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		// ReceiveObjects read will be called directly.
		// ReceiveObjects write will be called directly.
	}
}
